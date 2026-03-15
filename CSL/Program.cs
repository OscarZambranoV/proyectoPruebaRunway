using BL.DependencyInjection;
using DAL.DependencyInjection;
using Dapper;
using Npgsql;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

DefaultTypeMap.MatchNamesWithUnderscores = true;

builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);
builder.Configuration.AddEnvironmentVariables();

builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<CSL.Filters.RequireSessionFilter>();
    options.Filters.Add(new Microsoft.AspNetCore.Mvc.AutoValidateAntiforgeryTokenAttribute());
});

builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
    options.Cookie.Name = "LandingAdmin.AntiCsrf";
    options.Cookie.HttpOnly = false;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
});

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
});

builder.Services.AddHttpContextAccessor();

var connectionString = ResolveConnectionString(builder.Configuration);
builder.Services.AddDalServices(connectionString);
builder.Services.AddBlServices();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.Use(async (context, next) =>
{
    if (HttpMethods.IsGet(context.Request.Method))
    {
        var antiforgery = context.RequestServices.GetRequiredService<Microsoft.AspNetCore.Antiforgery.IAntiforgery>();
        var tokens = antiforgery.GetAndStoreTokens(context);
        if (!string.IsNullOrWhiteSpace(tokens.RequestToken))
        {
            context.Response.Cookies.Append("XSRF-TOKEN", tokens.RequestToken,
                new CookieOptions
                {
                    HttpOnly = false,
                    IsEssential = true,
                    SameSite = SameSiteMode.Lax,
                    Path = "/"
                });
        }
    }
    await next();
});

app.UseSession();
app.UseAuthorization();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

static string ResolveConnectionString(IConfiguration configuration)
{
    var env   = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
    var dbUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
    var cfg   = configuration.GetConnectionString("DefaultConnection");

    var cs = !string.IsNullOrWhiteSpace(env)  ? env
           : !string.IsNullOrWhiteSpace(dbUrl) ? ConvertDatabaseUrl(dbUrl)
           : cfg;

    if (string.IsNullOrWhiteSpace(cs))
        throw new InvalidOperationException(
            "Connection string no configurada. Usar variable de entorno ConnectionStrings__DefaultConnection o DATABASE_URL.");

    if (cs.Contains("REPLACE_ME", StringComparison.OrdinalIgnoreCase))
        throw new InvalidOperationException(
            "Connection string tiene placeholder REPLACE_ME. Configurar credenciales reales.");

    return cs;
}

static string ConvertDatabaseUrl(string databaseUrl)
{
    var uri = new Uri(databaseUrl);
    var parts = uri.UserInfo.Split(':', 2);
    if (parts.Length != 2)
        throw new InvalidOperationException(
            "DATABASE_URL no tiene formato valido: postgres://user:pass@host:port/db");

    return new NpgsqlConnectionStringBuilder
    {
        Host     = uri.Host,
        Port     = uri.Port > 0 ? uri.Port : 5432,
        Database = uri.AbsolutePath.TrimStart('/'),
        Username = WebUtility.UrlDecode(parts[0]),
        Password = WebUtility.UrlDecode(parts[1]),
        SslMode  = SslMode.Prefer
    }.ConnectionString;
}

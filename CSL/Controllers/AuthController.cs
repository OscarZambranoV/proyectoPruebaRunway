using CSL.Filters;
using Microsoft.AspNetCore.Mvc;

namespace CSL.Controllers;

[AllowNoSession]
public sealed class AuthController : Controller
{
    [HttpGet]
    public IActionResult Login()
    {
        if (HttpContext.Session.GetInt32("UserId") is not null)
            return RedirectToAction("Index", "MenuItem");

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Login(string usuario, string password)
    {
        var adminUser = Environment.GetEnvironmentVariable("ADMIN_USER") ?? "admin";
        var adminPass = Environment.GetEnvironmentVariable("ADMIN_PASSWORD") ?? "changeme";

        if (usuario == adminUser && password == adminPass)
        {
            HttpContext.Session.SetInt32("UserId", 1);
            return RedirectToAction("Index", "MenuItem");
        }

        ViewBag.Error = "Usuario o contrasena incorrectos.";
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Login", "Auth");
    }
}

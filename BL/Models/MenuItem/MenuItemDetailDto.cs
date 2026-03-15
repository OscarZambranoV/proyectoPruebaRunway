namespace BL.Models.MenuItem;

public sealed class MenuItemDetailDto
{
    public int IdMenuItem { get; set; }
    public string Etiqueta { get; set; } = string.Empty;
    public string UrlDestino { get; set; } = string.Empty;
    public int Orden { get; set; }
    public int Activo { get; set; }
}

using System.ComponentModel.DataAnnotations;

namespace CSL.Models.MenuItem;

public sealed class MenuItemSaveInput
{
    public int IdMenuItem { get; set; }

    [Required]
    [StringLength(100)]
    public string Etiqueta { get; set; } = string.Empty;

    [Required]
    [StringLength(300)]
    public string UrlDestino { get; set; } = string.Empty;

    public int Orden { get; set; }
}

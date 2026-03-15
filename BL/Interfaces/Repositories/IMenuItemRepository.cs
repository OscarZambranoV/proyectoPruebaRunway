using BL.Models.MenuItem;

namespace BL.Interfaces.Repositories;

public interface IMenuItemRepository
{
    Task<IReadOnlyList<MenuItemListDto>> ListarAsync();
    Task<MenuItemDetailDto?> ObtenerAsync(int idMenuItem);
    Task<bool> ExisteEtiquetaAsync(string etiqueta, int? idMenuItem);
    Task<int> GuardarAsync(MenuItemSaveRequest request);
    Task<MenuItemDeleteResult> EliminarAsync(int idMenuItem, int usuarioAccion);
}

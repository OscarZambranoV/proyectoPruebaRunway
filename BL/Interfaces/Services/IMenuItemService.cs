using BL.Common;
using BL.Models.MenuItem;

namespace BL.Interfaces.Services;

public interface IMenuItemService
{
    Task<IReadOnlyList<MenuItemListDto>> ListarAsync();
    Task<MenuItemDetailDto?> ObtenerAsync(int idMenuItem);
    Task<ServiceResult<int>> GuardarAsync(MenuItemSaveRequest request);
    Task<ServiceResult<int>> EliminarAsync(int idMenuItem, int usuarioAccion);
}

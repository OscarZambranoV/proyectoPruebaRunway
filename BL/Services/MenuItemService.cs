using BL.Common;
using BL.Interfaces.Repositories;
using BL.Interfaces.Services;
using BL.Models.MenuItem;

namespace BL.Services;

public sealed class MenuItemService : IMenuItemService
{
    private readonly IMenuItemRepository _repo;

    public MenuItemService(IMenuItemRepository repo)
    {
        _repo = repo;
    }

    public Task<IReadOnlyList<MenuItemListDto>> ListarAsync()
        => _repo.ListarAsync();

    public Task<MenuItemDetailDto?> ObtenerAsync(int idMenuItem)
        => _repo.ObtenerAsync(idMenuItem);

    public async Task<ServiceResult<int>> GuardarAsync(MenuItemSaveRequest request)
    {
        request.Etiqueta   = request.Etiqueta?.Trim() ?? string.Empty;
        request.UrlDestino = request.UrlDestino?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(request.Etiqueta))
            return ServiceResult<int>.Fail("La etiqueta es obligatoria.");

        if (request.Etiqueta.Length > 100)
            return ServiceResult<int>.Fail("La etiqueta no puede superar 100 caracteres.");

        if (string.IsNullOrWhiteSpace(request.UrlDestino))
            return ServiceResult<int>.Fail("La URL destino es obligatoria.");

        if (request.UrlDestino.Length > 300)
            return ServiceResult<int>.Fail("La URL destino no puede superar 300 caracteres.");

        if (request.Orden < 0)
            return ServiceResult<int>.Fail("El orden debe ser mayor o igual a 0.");

        var idParaExcluir = request.IdMenuItem > 0 ? request.IdMenuItem : (int?)null;
        var existe = await _repo.ExisteEtiquetaAsync(request.Etiqueta, idParaExcluir);
        if (existe)
            return ServiceResult<int>.Fail("Ya existe un item de menu con esa etiqueta.");

        try
        {
            var id = await _repo.GuardarAsync(request);
            if (id == 0)
                return ServiceResult<int>.Fail("No se pudo guardar el item de menu.");

            return ServiceResult<int>.Ok(id);
        }
        catch (InvalidOperationException ex)
        {
            return ServiceResult<int>.Fail(ex.Message);
        }
    }

    public async Task<ServiceResult<int>> EliminarAsync(int idMenuItem, int usuarioAccion)
    {
        if (idMenuItem <= 0)
            return ServiceResult<int>.Fail("El id del item de menu no es valido.");

        var resultado = await _repo.EliminarAsync(idMenuItem, usuarioAccion);

        if (resultado.RowsAffected == 0)
            return ServiceResult<int>.Fail("No se encontro el item de menu.");

        return ServiceResult<int>.Ok(idMenuItem);
    }
}

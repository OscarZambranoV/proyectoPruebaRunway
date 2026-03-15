using BL.Interfaces.Services;
using BL.Models.MenuItem;
using CSL.Filters;
using CSL.Models.MenuItem;
using Microsoft.AspNetCore.Mvc;

namespace CSL.Controllers;

public sealed class MenuItemController : Controller
{
    private readonly IMenuItemService _menuItemService;

    public MenuItemController(IMenuItemService menuItemService)
    {
        _menuItemService = menuItemService;
    }

    [HttpGet]
    public IActionResult Index() => View();

    [HttpGet]
    public async Task<IActionResult> Listar()
    {
        var data = await _menuItemService.ListarAsync();
        return Json(data);
    }

    [HttpGet]
    public async Task<IActionResult> Obtener(int id)
    {
        var data = await _menuItemService.ObtenerAsync(id);
        return Json(data);
    }

    [HttpPost]
    public async Task<IActionResult> Guardar([FromBody] MenuItemSaveInput input)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var usuarioAccion = HttpContext.Session.GetInt32("UserId") ?? 0;

        var request = new MenuItemSaveRequest
        {
            IdMenuItem    = input.IdMenuItem,
            Etiqueta      = input.Etiqueta,
            UrlDestino    = input.UrlDestino,
            Orden         = input.Orden,
            UsuarioAccion = usuarioAccion
        };

        var result = await _menuItemService.GuardarAsync(request);
        return Json(new { success = result.Success, message = result.Error, id = result.Data });
    }

    [HttpPost]
    public async Task<IActionResult> Eliminar([FromBody] int id)
    {
        var usuarioAccion = HttpContext.Session.GetInt32("UserId") ?? 0;
        var result = await _menuItemService.EliminarAsync(id, usuarioAccion);
        return Json(new { success = result.Success, message = result.Error });
    }
}

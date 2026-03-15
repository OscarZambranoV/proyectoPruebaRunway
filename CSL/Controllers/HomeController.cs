using BL.Interfaces.Services;
using CSL.Filters;
using Microsoft.AspNetCore.Mvc;

namespace CSL.Controllers;

[AllowNoSession]
public sealed class HomeController : Controller
{
    private readonly IMenuItemService _menuItemService;

    public HomeController(IMenuItemService menuItemService)
    {
        _menuItemService = menuItemService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var items = await _menuItemService.ListarAsync();
        ViewBag.MenuItems = items;
        return View();
    }
}

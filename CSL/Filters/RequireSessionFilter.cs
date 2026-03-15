using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CSL.Filters;

public sealed class RequireSessionFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        var descriptor = context.ActionDescriptor as ControllerActionDescriptor;
        if (descriptor is null) return;

        var allowNoSession =
            descriptor.ControllerTypeInfo.GetCustomAttributes(typeof(AllowNoSessionAttribute), true).Length > 0 ||
            descriptor.MethodInfo.GetCustomAttributes(typeof(AllowNoSessionAttribute), true).Length > 0;

        if (allowNoSession) return;

        var userId = context.HttpContext.Session.GetInt32("UserId");
        if (userId is null)
        {
            var isAjax = context.HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest"
                      || (context.HttpContext.Request.ContentType?.Contains("application/json") ?? false)
                      || (context.HttpContext.Request.Headers.Accept.ToString().Contains("application/json"));

            if (isAjax)
            {
                context.Result = new UnauthorizedResult();
            }
            else
            {
                context.Result = new RedirectToActionResult("Login", "Auth", null);
            }
        }
    }

    public void OnActionExecuted(ActionExecutedContext context) { }
}

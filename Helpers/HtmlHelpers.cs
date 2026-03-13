using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace PrinterMonitor.Helpers
{
    public static class HtmlHelpers
    {
        public static string isActive(this IHtmlHelper html, string controller, string? action = null)
        {
            var routeData = html.ViewContext.RouteData;

            var routeAction = routeData.Values["action"]?.ToString();
            var routeController = routeData.Values["controller"]?.ToString();

            var controllerMatch = controller == routeController;
            var actionMatch = action == null || action == routeAction;

            return controllerMatch && actionMatch ? "active" : "";
        }
        
    }
}
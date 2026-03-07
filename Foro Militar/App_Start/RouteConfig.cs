using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Foro_Militar
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            // ← AGREGAR ESTO PRIMERO (antes de MapRoute)
            routes.MapMvcAttributeRoutes();

            // Ruta para el dashboard de comunidad por slug
            routes.MapRoute(
                name: "CommunityDashboard",
                url: "communities/{slug}",
                defaults: new { controller = "Communities", action = "Dashboard" },
                constraints: new { slug = @"(?-i)^[a-z0-9][a-z0-9\-]*$" }  // ← (?-i) fuerza case-sensitive
            );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}

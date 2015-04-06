using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace AadAppRoleManager.Web
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "DeleteAssignment",
                url: "Application/{applicationId}/AppRole/{appRoleId}/Delete/{assignmentId}",
                defaults: new { controller = "Application", action = "DeleteAssignment" }
            );

            routes.MapRoute(
                name: "CreateAssignment",
                url: "Application/{applicationId}/AppRole/{appRoleId}/Create",
                defaults: new { controller = "Application", action = "CreateAssignment" }
            );

            routes.MapRoute(
                name: "AppRole",
                url: "Application/{applicationId}/AppRole/{appRoleId}",
                defaults: new { controller = "Application", action = "AppRole" }
            );

            routes.MapRoute(
                name: "Application",
                url: "Application/{applicationId}",
                defaults: new { controller = "Application", action = "Application" }
            );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}

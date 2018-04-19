using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Ajf.Nuget.Logging;
using Serilog;
using Web;

namespace HansJuergenWeb.WebHJ
{
    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
            Log.Logger = StandardLoggerConfigurator.GetEnrichedLogger();
            Log.Logger.Information("Starting...");

            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            Log.Logger.Information("Done app start...");

        }
    }
}
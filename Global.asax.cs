using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace OpenLawOffice.Web
{
    public class MvcApplication : System.Web.HttpApplication
    {
        private bool _isInstalled = false;

        protected void Application_Start()
        {
            _isInstalled = IsInstalled();

            Common.ObjectMapper.MapAssembly(typeof(MvcApplication).Assembly);
            Common.ObjectMapper.MapAssembly(typeof(Data.Database).Assembly);

            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            if (!_isInstalled)
            {
                if (IsInstalled())
                {
                    _isInstalled = true;
                    return;
                }

                if (!Request.Path.Contains("/Installation"))
                {
                    Context.Response.Redirect("/Installation");
                }
            }
        }

        private bool IsInstalled()
        {
            // Test for installation

            // The most very basic installation must consist of an administrative user and therefore
            // if we cannot list the users, the installation has not been completed.
            try
            {
                Data.Account.Users.List();
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Routing;

namespace ProjectTemplate
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);

            // ✅ TEST DATABASE CONNECTION ON STARTUP
            try
            {
                DatabaseHelper dbHelper = new DatabaseHelper();
                dbHelper.TestConnection();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("❌ Database connection failed: " + ex.Message);
            }
        }
    }
}

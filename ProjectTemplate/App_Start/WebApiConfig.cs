using System.Net.Http.Headers;
using System.Web.Http;

public static class WebApiConfig
{
    public static void Register(HttpConfiguration config)
    {
        // Force JSON response format
        config.Formatters.JsonFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/html"));
        config.Formatters.Remove(config.Formatters.XmlFormatter);

        // Enable attribute-based routing
        config.MapHttpAttributeRoutes();

        // Define default API route
        config.Routes.MapHttpRoute(
            name: "DefaultApi",
            routeTemplate: "api/{controller}/{id}",
            defaults: new { id = RouteParameter.Optional }
        );
    }
}

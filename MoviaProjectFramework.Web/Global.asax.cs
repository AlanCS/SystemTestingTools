using Autofac;
using MovieProject.Logic;
using MovieProject.Logic.Proxy;
using System;
using System.Net.Http;
using System.Reflection;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;

namespace MoviaProjectFramework.Web
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            //services
            //    .AddHttpClient<IMovieDatabaseProxy, MovieDatabaseProxy>()
            //    .AddPolicyHandler(retryPolicy)
            //    .AddPolicyHandler(timeoutPolicy) // We place the timeoutPolicy inside the retryPolicy, to make it time out each try
            //    .ConfigureHttpClient(c =>
            //    {
            //        c.BaseAddress = new Uri(omdb.Value.Url);
            //        c.DefaultRequestHeaders.Add("Referer", Logic.Constants.Website);
            //        c.Timeout = TimeSpan.FromMilliseconds(1500); // Overall timeout across all tries
            //    });


            var builder = new ContainerBuilder();
            
            builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly(), typeof(SearchService).Assembly)
                   .AsImplementedInterfaces();

            builder.Register(ctx => new HttpClient() { BaseAddress = new Uri(System.Configuration.ConfigurationManager.AppSettings["omdb.Value.Url"]) })
                .Named<HttpClient>("omdbHttpClient")
                .SingleInstance();

            builder.Register<IMovieDatabaseProxy>(c => new MovieDatabaseProxy(c.Resolve("omdbHttpClient"), null));

            var Container = builder.Build();
        }
    }
}

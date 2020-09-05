using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;

namespace SystemTestingTools
{
    /// <summary>
    /// IApplicationBuilder extensions
    /// </summary>
    public static class IApplicationBuilderExtensions
    {
        /// <summary>
        /// Exposed the SystemTestingTools stubs folder for browsing, the address will be /Stubs by default, or Configuration.ExposeFolderForNavigationAs
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static IApplicationBuilder ExposeStubsForDirectoryBrowsing(this IApplicationBuilder app)
        {
            if (Constants.GlobalConfiguration == null) throw new System.Exception("Add the line services.InterceptHttpCallsAfterSending() first");

            // copied from https://docs.microsoft.com/en-us/aspnet/core/fundamentals/static-files?view=aspnetcore-3.1
            app.UseFileServer(new FileServerOptions
            {
                FileProvider = new PhysicalFileProvider(Constants.GlobalConfiguration.RootStubsFolder),
                RequestPath = new PathString("/" + Constants.GlobalConfiguration.ExposeStubsAs),
                EnableDirectoryBrowsing = true
            });

            return app;
        }
    }
}

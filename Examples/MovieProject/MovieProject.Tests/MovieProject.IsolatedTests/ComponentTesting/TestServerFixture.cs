using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MovieProject.Web;
using System;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using SystemTestingTools;
using Xunit;

namespace IsolatedTests.ComponentTestings
{
    public class TestServerFixture : IDisposable
    {
        public TestServer Server { get; private set; }

        public string StubsFolder { get; private set; }

        public TestServerFixture()
        {
            StartServer();
            SanityCheckServer();
        }

        private void StartServer()
        {
            StubsFolder = new Regex(@"\\bin\\.*").Replace(System.Environment.CurrentDirectory, "") + @"\ComponentTesting\Stubs";

            Startup.wcfInterceptor = WcfHttpInterceptor.CreateInterceptor(); // you only need this line if you are working with HTTP WCF calls
            
            var builder = Program.CreateWebHostBuilder(new string[0]) // use the exact same builder as the website, to test the wiring
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    // make small changes to configuration, such as disabling caching
                    config.AddJsonFile("appsettings.tests.json", optional: false, reloadOnChange: true);
                })                
                .ConfigureInterceptionOfHttpClientCalls()
                .IntercepLogs(minimumLevelToIntercept: LogLevel.Information, 
                                namespaceToIncludeStart: new[] { "MovieProject" },
                                namespaceToExcludeStart: new[] { "Microsoft" }) // redundand exclusion, just here to show the possible configuration
                .UseEnvironment("Development");

            Server = new TestServer(builder);   
        }

        /// <summary>
        /// Run a quick sanity check, before running any tests
        /// </summary>
        /// <returns></returns>
        private void SanityCheckServer()
        {
            HttpResponseMessage response = null;
            using (var client = Server.CreateClient())
                response = client.GetAsync("/healthcheck").Result; // we run the async method synchronously because it's called from a contructor, that can't be async

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
                throw new ApplicationException("TestServer doesn't respond to basic request to /healthcheck");

            if(ContextRepo.UnsessionedLogs.Count > 1)
                throw new ApplicationException("Unexpected logs are showing up when starting application");

            if (ContextRepo.UnsessionedLogs.FirstOrDefault()?.ToString() != "Information: Application is starting")
                throw new ApplicationException("Logs don't seem to be wired correctly");
        }

        public void Dispose()
        {
            Server.Dispose();
        }
    }

    [CollectionDefinition("SharedServer collection")]
    public class SharedServerCollection : ICollectionFixture<TestServerFixture>
    {
        // as suggested in https://xunit.github.io/docs/shared-context
        // done so this collection is shared between many tests
    }
}

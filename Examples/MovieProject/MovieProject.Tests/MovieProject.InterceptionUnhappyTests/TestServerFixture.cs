using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MovieProject.Web;
using System;
using System.Linq;
using System.Net.Http;
using SystemTestingTools;
using Xunit;

namespace MovieProject.InterceptionTests
{
    public class TestServerFixture : IDisposable
    {
        public TestServer Server { get; private set; }

        public TestServerFixture()
        {
            StartServer();
            SanityCheckServer();
        }

        private void StartServer()
        {
            var builder = Program.CreateWebHostBuilder(new string[0]) // use the exact same builder as the website, to test the wiring
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    // make small changes to configuration, such as disabling caching
                    config.AddJsonFile("appsettings.tests.json", optional: false, reloadOnChange: true);
                })
                //.InterceptHttpCallsBeforeSending() we don't intercept Before sending the request, but After, which is configured in website startup
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

            if (UnsessionedData.UnsessionedLogs.Count != 1)
                throw new ApplicationException($"Expected to find 1 log during startup, found {UnsessionedData.UnsessionedLogs.Count}");

            var firstMessage = UnsessionedData.UnsessionedLogs.First()?.ToString();
            if (firstMessage != "Information: Application is starting")
                throw new ApplicationException($"First log was not the expected one: {firstMessage}");
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

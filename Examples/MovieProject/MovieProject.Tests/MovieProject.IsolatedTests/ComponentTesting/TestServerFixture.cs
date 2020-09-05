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

        public static string MovieUrl = "http://www.omdbapi.com/?apikey=863d6589&type=movie";

        public TestServerFixture()
        {
            StartServer();
            SanityCheckServer();
        }

        private void StartServer()
        {
            StubsFolder = new Regex(@"\\bin\\.*").Replace(System.Environment.CurrentDirectory, "") + @"\ComponentTesting\Stubs";
           
            var builder = Program.CreateWebHostBuilder(new string[0]) // use the exact same builder as the website, to test the wiring
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    // make small changes to configuration, such as disabling caching
                    config.AddJsonFile("appsettings.tests.json", optional: false, reloadOnChange: true);
                })                
                .InterceptHttpCallsBeforeSending()
                .IntercepLogs(minimumLevelToIntercept: LogLevel.Information, 
                                namespaceToIncludeStart: new[] { "MovieProject" },
                                namespaceToExcludeStart: new[] { "Microsoft" }) // redundand exclusion, just here to show the possible configuration
                .UseEnvironment("Development");

            SetupGlobalStubs();

            Server = new TestServer(builder);   
        }

        private void SetupGlobalStubs()
        {
            // this will be the default response, returned if it can't find a match for the session
            var response = ResponseFactory.FromFiddlerLikeResponseFile($"{StubsFolder}/OmdbApi/Fake_Responses/Happy/200_NoRunTime_NoPlot_YearTooOld.txt");
            var comeAlongMovieUrl = $"{MovieUrl}&t=fantastika";
            UnsessionedData.AppendGlobalHttpCallStub(HttpMethod.Get, new System.Uri(comeAlongMovieUrl), response);
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

            if(UnsessionedData.UnsessionedLogs.Count != 1)
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

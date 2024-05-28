using FluentAssertions;
using FluentAssertions.Execution;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using SystemTestingTools;
using Xunit;

namespace MovieProject.InterceptionTests
{
    [Collection("SharedServer collection")]
    [Trait("Project", "MovieProject.InterceptionUnhappyTests")]
    public class GetMovieTests
    {
        private readonly TestServerFixture Fixture;

        public GetMovieTests(TestServerFixture fixture)
        {
            Fixture = fixture;
        }

        [Fact]
        public async Task When_UserAsksForMovie_ButWrongUrl_Then_FindResponseInPreApproved()
        {
            // arrange
            var client = Fixture.Server.CreateClient();
            client.CreateSession();
            string errorMessage = "GET http://www.omdbapifake.com/?apikey=863d6589&type=movie&t=matrix received exception [No such host is known.]";

            // act
            var httpResponse = await client.GetAsync("/api/movie/matrix");

            using (new AssertionScope())
            {
                // assert logs                
                var logs = client.GetSessionLogs();
                logs.Should().HaveCount(1);
                logs[0].ToString().Should().Be($"Error: {errorMessage}");

                // assert outgoing
                AssertOutgoingRequests(client);

                await CheckResponse();
            }

            // check 2 more times to see if the same response will be returned
            httpResponse = await client.GetAsync("/api/movie/matrix");
            await CheckResponse();
            httpResponse = await client.GetAsync("/api/movie/matrix");
            await CheckResponse();

            using (new AssertionScope())
            {
                // assert logs
                client.GetSessionLogs().Should().HaveCount(3);

                // assert outgoing
                client.GetSessionOutgoingRequests().Should().HaveCount(3);
            }

            async Task CheckResponse()
            {
                using (new AssertionScope())
                {
                    // assert return
                    httpResponse.StatusCode.Should().Be(HttpStatusCode.OK);
                    httpResponse.GetHeaderValue("SystemTestingToolsStub").Should().Be($@"Recording [omdb/pre-approved/happy/matrix] reason {errorMessage}");

                    var movie = await httpResponse.ReadJsonBody<Logic.DTO.Media>();
                    movie.Id.Should().Be("tt0133093");
                    movie.Name.Should().Be("The Matrix");
                }
            }
        }

        [Fact]
        public async Task When_UserAsksForMovie_ButWrongUrl_AndNoRecordingFound_Then_ReturnFallback()
        {
            // arrange
            var client = Fixture.Server.CreateClient();
            client.CreateSession();

            // act
            var httpResponse = await client.GetAsync("/api/movie/gibberish");

            using (new AssertionScope())
            {
                // assert logs
                
                string errorMessageWindows = "GET http://www.omdbapifake.com/?apikey=863d6589&type=movie&t=gibberish received exception [No such host is known.]"; // windows error message
                string errorMessageUbuntu = "GET http://www.omdbapifake.com/?apikey=863d6589&type=movie&t=gibberish received exception [Name or service not known]"; // ubuntu error message

                var logs = client.GetSessionLogs();
                logs.Should().HaveCount(1);
                logs[0].ToString().Should().BeOneOf($"Error: {errorMessageWindows}", $"Error: {errorMessageUbuntu}");

                // assert outgoing
                AssertOutgoingRequests(client);

                // assert return
                httpResponse.StatusCode.Should().Be(HttpStatusCode.OK);
                httpResponse.GetHeaderValue("SystemTestingToolsStub").Should()
                    .BeOneOf($@"Recording [omdb/pre-approved/happy/last_fallback] reason {errorMessageWindows} and could not find better match",
                             $@"Recording [omdb/pre-approved/happy/last_fallback] reason {errorMessageUbuntu} and could not find better match");

                var movie = await httpResponse.ReadJsonBody<Logic.DTO.Media>();
                movie.Id.Should().Be("tt0123456");
                movie.Name.Should().Be("Fake Movie Last Fallback");
            }
        }

        [Fact]
        public async Task When_SendingHeaderWithStubInstructions_WebSiteObeys()
        {
            // arrange
            var client = Fixture.Server.CreateClient();
            client.CreateSession();

            var request = new HttpRequestMessage(HttpMethod.Get, "/api/movie/the godfather");
            request.Headers.Add("SystemTestingTools_ReturnStub", "omdb/pre-approved/unhappy/NotFound");

            // act
            var httpResponse = await client.SendAsync(request);

            using (new AssertionScope())
            {
                // assert outgoing
                var outgoing = AssertOutgoingRequests(client);
                outgoing[0].GetHeaderValue("SystemTestingTools_ReturnStub").Should().Be("omdb/pre-approved/unhappy/NotFound");

                // assert return
                httpResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
                httpResponse.GetHeaderValue("SystemTestingToolsStub").Should().StartWith("Stub [omdb/pre-approved/unhappy/NotFound.txt] reason instructions from header");

                var message = await httpResponse.ReadBody();
                message.Should().Be("Search terms didn't match any movie");
            }
        }

        private static List<HttpRequestMessage> AssertOutgoingRequests(HttpClient client)
        {
            var outgoing = client.GetSessionOutgoingRequests();
            outgoing.Should().HaveCount(1);
            outgoing[0].GetEndpoint().Should().StartWith("GET http://www.omdbapifake.com/?apikey=863d6589&type=movie&t=");
            var dates = outgoing[0].GetDatesSent();
            dates.Should().HaveCount(1);
            dates[0].Should().BeCloseTo(DateTime.Now, TimeSpan.FromMilliseconds(700));

            return outgoing;
        }
    }
}

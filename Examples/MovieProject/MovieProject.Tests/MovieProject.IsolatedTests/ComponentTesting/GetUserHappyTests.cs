using FluentAssertions;
using FluentAssertions.Execution;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using SystemTestingTools;
using Xunit;

namespace IsolatedTests.ComponentTestings
{
    [Collection("SharedServer collection")]
    [Trait("Project", "User Component Tests (Happy)")]
    public class GetUserHappyTests
    {
        private readonly TestServerFixture Fixture;

        private static string Url = "https://jsonplaceholder.typicode.com/users";

        public GetUserHappyTests(TestServerFixture fixture)
        {        
            Fixture = fixture;
        }

        [Fact]
        public async Task When_UserAsksForUserList_Then_ReturnListProperly()
        {
            // arrange
            var client = Fixture.Server.CreateClient();
            client.CreateSession();
            var response = ResponseFactory.FromFiddlerLikeResponseFile($"{Fixture.StubsFolder}/UserApi/Real_Responses/Happy/200_ListUsers.txt");

            response.ModifyJsonBody<MovieProject.Logic.Proxy.DTO.User[]>(dto =>
            {
                dto[0].Name = "Changed in code";
            });

            client.AppendHttpCallStub(HttpMethod.Get, new System.Uri(Url), response);

            // act
            var httpResponse = await client.GetAsync("/api/users");

            using (new AssertionScope())
            {
                // assert logs
                var logs = client.GetSessionLogs();
                logs.Should().BeEmpty();

                // assert outgoing
                var outgoingRequests = client.GetSessionOutgoingRequests();
                outgoingRequests.Count.Should().Be(1);
                outgoingRequests[0].GetEndpoint().Should().Be($"GET {Url}");
                outgoingRequests[0].GetHeaderValue("Referer").Should().Be(MovieProject.Logic.Constants.Website);

                // assert return
                httpResponse.StatusCode.Should().Be(HttpStatusCode.OK);

                var list = await httpResponse.ReadJsonBody<List<string>>();
                list.Count.Should().Be(10);
                list[0].Should().Be("Changed in code");
                list[1].Should().Be("Chelsey Dietrich");
                list[2].Should().Be("Clementina DuBuque");
                list[3].Should().Be("Clementine Bauch");
                list[4].Should().Be("Ervin Howell");
                list[5].Should().Be("Glenna Reichert");
                list[6].Should().Be("Kurtis Weissnat");
                list[7].Should().Be("Mrs. Dennis Schulist");
                list[8].Should().Be("Nicholas Runolfsdottir V");
                list[9].Should().Be("Patricia Lebsack");
            }
        }
    }
}

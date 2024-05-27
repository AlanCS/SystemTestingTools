using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Execution;
using IsolatedTests.ComponentTestings;
using MovieProject.Logic.DTO.MovieProject.Logic.Proxy.DTO;
using Newtonsoft.Json;
using SystemTestingTools;
using Xunit;

namespace MovieProject.IsolatedTests.ComponentTesting
{
    [Collection("SharedServer collection")]
    [Trait("Project", "User Component Tests (Happy)")]
    public class SearchUserHappyTests
    {
        private readonly TestServerFixture Fixture;

        private static string Url = "https://jsonplaceholder.typicode.com/searchUsers";

        public SearchUserHappyTests(TestServerFixture fixture)
        {        
            Fixture = fixture;
        }
        
        [Fact]
        public async Task When_UserSearchesWithValidParameters_Then_ReturnListProperly()
        {
            // arrange
            var complexParameter = new UserSearchModel
            {
                Username = "Bret",
            };
            var serialisedComplexParameters = JsonConvert.SerializeObject(complexParameter);
            
            var client = Fixture.Server.CreateClient();
            client.CreateSession();
            var response = ResponseFactory.FromFiddlerLikeResponseFile($"{Fixture.StubsFolder}/UserApi/Real_Responses/Happy/200_SearchListUsers.txt");

            var uri = new System.Uri(@$"{Url}?Username=Bret");            
            client.AppendHttpCallStub(HttpMethod.Get, uri, response);

            // act
            var httpResponse = await client.GetAsync($"/api/searchUsers?jsonSearchModel={serialisedComplexParameters}");

            using (new AssertionScope())
            {
                // assert logs
                var logs = client.GetSessionLogs();
                logs.Should().BeEmpty();

                // assert outgoing
                var outgoingRequests = client.GetSessionOutgoingRequests();
                outgoingRequests.Count.Should().Be(1);
                outgoingRequests[0].GetEndpoint().Should().Be($"GET {Url}?Username=Bret");
                outgoingRequests[0].GetHeaderValue("Referer").Should().Be(MovieProject.Logic.Constants.Website);

                // assert return
                httpResponse.StatusCode.Should().Be(HttpStatusCode.OK);

                var expectedResponse = new List<string>()
            {
                "Leanne Graham"
            };


                var list = await httpResponse.ReadJsonBody<List<string>>();
                list.Count.Should().Be(1);
                list.Should().BeEquivalentTo(expectedResponse);
            }
        }
    }
}

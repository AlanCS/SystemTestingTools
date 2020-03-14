using IsolatedTests.Helpers;
using Shouldly;
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
            var response = ResponseFactory.FromFiddlerLikeResponseFile($"{Fixture.MocksFolder}/UserApi/Real_Responses/Happy/200_ListUsers.txt");

            response.ModifyJsonBody<MovieProject.Logic.Proxy.DTO.User[]>(dto =>
            {
                dto[0].Name = "Changed in code";
            });

            client.AppendMockHttpCall(HttpMethod.Get, new System.Uri(Url), response);

            // act
            var httpResponse = await client.GetAsync("/api/users");

            // assert logs
            var logs = client.GetSessionLogs();
            logs.ShouldBeEmpty();

            // assert outgoing
            var outgoingRequests = client.GetSessionOutgoingRequests();
            outgoingRequests.Count.ShouldBe(1);
            outgoingRequests[0].ShouldBeEndpoint($"GET {Url}");
            outgoingRequests[0].ShouldContainHeader("Referer", MovieProject.Logic.Constants.Website);

            // assert return
            httpResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

            var list = await httpResponse.ReadJsonBody<List<string>>();
            list.Count.ShouldBe(10);
            list[0].ShouldBe("Changed in code");
            list[1].ShouldBe("Chelsey Dietrich");
            list[2].ShouldBe("Clementina DuBuque");
            list[3].ShouldBe("Clementine Bauch");
            list[4].ShouldBe("Ervin Howell");
            list[5].ShouldBe("Glenna Reichert");
            list[6].ShouldBe("Kurtis Weissnat");
            list[7].ShouldBe("Mrs. Dennis Schulist");
            list[8].ShouldBe("Nicholas Runolfsdottir V");
            list[9].ShouldBe("Patricia Lebsack");
        }
    }
}

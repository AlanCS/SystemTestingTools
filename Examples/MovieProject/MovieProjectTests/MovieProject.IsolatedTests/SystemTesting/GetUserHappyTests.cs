using IsolatedTests.Helpers;
using Shouldly;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using SystemTestingTools;
using Xunit;

namespace IsolatedTests.SystemTestings
{
    [Collection("SharedServer collection")]
    [Trait("Project", "User System Tests (Happy)")]
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
            httpResponse.ShouldNotBeNullAndHaveStatus(HttpStatusCode.OK);

            var list = await httpResponse.ParseResponse<List<string>>();
            list.Count.ShouldBe(10);
            list[0].ShouldBe("Chelsey Dietrich");
            list[1].ShouldBe("Clementina DuBuque");
            list[2].ShouldBe("Clementine Bauch");
            list[3].ShouldBe("Ervin Howell");
            list[4].ShouldBe("Glenna Reichert");
            list[5].ShouldBe("Kurtis Weissnat");
            list[6].ShouldBe("Leanne Graham");
            list[7].ShouldBe("Mrs. Dennis Schulist");
            list[8].ShouldBe("Nicholas Runolfsdottir V");
            list[9].ShouldBe("Patricia Lebsack");
        }
    }
}

using FluentAssertions;
using FluentAssertions.Execution;
using MovieProject.Logic.DTO;
using MovieProject.Logic.Proxy.DTO;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using SystemTestingTools;
using Xunit;

namespace IsolatedTests.ComponentTestings
{
    [Collection("SharedServer collection")]
    [Trait("Project", "Post Component Tests (Happy)")]
    public class PostHappyTests
    {
        private readonly TestServerFixture Fixture;

        private static string Url = "https://jsonplaceholder.typicode.com/post";

        public PostHappyTests(TestServerFixture fixture)
        {
            Fixture = fixture;
        }

        [Fact]
        public async Task When_Creating_Post_Success()
        {
            // arrange
            var client = Fixture.Server.CreateClient();
            client.CreateSession();
            var response = ResponseFactory.FromFiddlerLikeResponseFile($"{Fixture.StubsFolder}/PostApi/Real_Responses/Happy/200_Post.txt");
            client.AppendHttpCallStub(HttpMethod.Post, new System.Uri(Url), response);

            var request = new Post
            {
                Body = "testPost",
                Title = "testTitle",
                UserId = 12345678
            };

            // act
            var post = await client.PostAsync("/api/post",
                new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json"));

            // asserts
            post.StatusCode.Should().Be(HttpStatusCode.Created);
            client.GetSessionLogs().Should().BeEmpty();

            var outgoingRequests = client.GetSessionOutgoingRequests();
            outgoingRequests.Should().HaveCount(1);
            outgoingRequests[0].GetEndpoint().Should().Be("POST https://jsonplaceholder.typicode.com/post");

            var requestBody = await outgoingRequests[0].ReadJsonBody<Post>();
            requestBody.Should().BeEquivalentTo(request);
        }
    }
}

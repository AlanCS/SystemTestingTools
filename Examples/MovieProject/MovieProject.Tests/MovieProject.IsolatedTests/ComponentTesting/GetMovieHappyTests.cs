using FluentAssertions;
using FluentAssertions.Execution;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using SystemTestingTools;
using Xunit;

namespace IsolatedTests.ComponentTestings
{
    [Collection("SharedServer collection")]
    [Trait("Project", "MovieProject Component Tests (Happy)")]
    public class GetMovieHappyTests
    {
        private readonly TestServerFixture Fixture;

        private static string MovieUrl = "http://www.omdbapi.com/?apikey=863d6589&type=movie";        

        public GetMovieHappyTests(TestServerFixture fixture)
        {        
            Fixture = fixture;
        }

        [Fact(Skip = "example of how NOT write Component tests, because it doesn't assert logs and outgoing requests")]
        public async Task When_UserAsksForMovieWithMostFields_Then_ReturnMovieProperly_Incomplete()
        {
            // arrange
            var client = Fixture.Server.CreateClient();
            client.CreateSession();
            var response = ResponseFactory.FromFiddlerLikeResponseFile($"{Fixture.StubsFolder}/OmdbApi/Real_Responses/Happy/200_ContainsMostFields_TheMatrix.txt");
            var matrixMovieUrl = $"{MovieUrl}&t=matrix";
            client.AppendHttpCallStub(HttpMethod.Get, new System.Uri(matrixMovieUrl), response);

            // act
            var httpResponse = await client.GetAsync("/api/movie/matrix");

            // assert
            httpResponse.Should().NotBeNull();
            httpResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var movie = JsonSerializer.Deserialize<MovieProject.Logic.DTO.Media>(await httpResponse.Content.ReadAsStringAsync());
            movie.Should().NotBeNull();

            movie.Id.Should().Be("tt0133093");
            movie.Name.Should().Be("The Matrix");
            movie.Year.Should().Be("1999");
            movie.Runtime.Should().Be("136 min");
            movie.Plot.Should().Be("A computer hacker learns from mysterious rebels about the true nature of his reality and his role in the war against its controllers.");
        }

        [Fact]
        public async Task When_UserAsksForMovieWithMostFields_Then_ReturnMovieProperly()
        {
            // arrange
            var client = Fixture.Server.CreateClient();
            client.CreateSession();
            var response = ResponseFactory.FromRecordedFile($"{Fixture.StubsFolder}/OmdbApi/Real_Responses/Happy/200_ContainsMostFields_TheMatrix.txt");
            var matrixMovieUrl = $"{MovieUrl}&t=matrix";
            client.AppendHttpCallStub(HttpMethod.Get, new System.Uri(matrixMovieUrl), response);

            // act
            var httpResponse = await client.GetAsync("/api/movie/matrix");

            using (new AssertionScope())
            {
                // assert logs
                client.GetSessionLogs().Should().BeEmpty();

                // assert outgoing
                var outgoingRequests = client.GetSessionOutgoingRequests();
                outgoingRequests.Count.Should().Be(1);
                outgoingRequests[0].GetEndpoint().Should().Be($"GET {matrixMovieUrl}");
                outgoingRequests[0].GetHeaderValue("Referer").Should().Be(MovieProject.Logic.Constants.Website);

                // assert return
                httpResponse.StatusCode.Should().Be(HttpStatusCode.OK);

                var movie = await httpResponse.ReadJsonBody<MovieProject.Logic.DTO.Media>();
                movie.Id.Should().Be("tt0133093");
                movie.Name.Should().Be("The Matrix");
                movie.Year.Should().Be("1999");
                movie.Runtime.Should().Be("2.3h");
                movie.Plot.Should().Be("A computer hacker learns from mysterious rebels about the true nature of his reality and his role in the war against its controllers.");
                movie.Language.Should().Be(MovieProject.Logic.DTO.Media.Languages.English);
            }
        }

        [Fact]
        public async Task When_UserAsksForMovieWithMostFields_With_ResponseBodyOnly_Then_ReturnMovieProperly()
        {
            // arrange
            var client = Fixture.Server.CreateClient();
            client.CreateSession();
            // FromBodyOnlyFile work well, but if possible it's nicer to read from a more comprehensive file format (the one that FromFiddlerLikeResponseFile uses)
            var response = ResponseFactory.FromBodyOnlyFile($"{Fixture.StubsFolder}/OmdbApi/Real_Responses/Happy/200_ContainsMostFields_TheMatrix.json", HttpStatusCode.OK);
            var matrixMovieUrl = $"{MovieUrl}&t=matrix";
            client.AppendHttpCallStub(HttpMethod.Get, new System.Uri(matrixMovieUrl), response);

            // act
            var httpResponse = await client.GetAsync("/api/movie/matrix");

            using (new AssertionScope())
            {
                // assert logs
                client.GetSessionLogs().Should().BeEmpty();

                // assert outgoing
                var outgoingRequests = client.GetSessionOutgoingRequests();
                outgoingRequests.Count.Should().Be(1);
                outgoingRequests[0].GetEndpoint().Should().Be($"GET {matrixMovieUrl}");
                outgoingRequests[0].GetHeaderValue("Referer").Should().Be(MovieProject.Logic.Constants.Website);

                // assert return
                httpResponse.StatusCode.Should().Be(HttpStatusCode.OK);

                var movie = await httpResponse.ReadJsonBody<MovieProject.Logic.DTO.Media>();
                movie.Id.Should().Be("tt0133093");
                movie.Name.Should().Be("The Matrix");
                movie.Year.Should().Be("1999");
                movie.Runtime.Should().Be("2.3h");
                movie.Plot.Should().Be("A computer hacker learns from mysterious rebels about the true nature of his reality and his role in the war against its controllers.");
                movie.Language.Should().Be(MovieProject.Logic.DTO.Media.Languages.English);
            }
        }

        [Fact]
        public async Task When_UserAsksForMovie_30times_WithSameStub_Works()
        {
            // arrange
            var client = Fixture.Server.CreateClient();
            client.CreateSession();
            // FromBodyOnlyFile work well, but if possible it's nicer to read from a more comprehensive file format (the one that FromFiddlerLikeResponseFile uses)
            var response = ResponseFactory.FromBodyOnlyFile($"{Fixture.StubsFolder}/OmdbApi/Real_Responses/Happy/200_ContainsMostFields_TheMatrix.json", HttpStatusCode.OK);
            var matrixMovieUrl = $"{MovieUrl}&t=matrix";
            client.AppendHttpCallStub(HttpMethod.Get, new System.Uri(matrixMovieUrl), response, counter: 30);

            // act
            for (int i = 0; i < 30; i++)
                await client.GetAsync("/api/movie/matrix");

            using (new AssertionScope())
            {
                // assert logs
                client.GetSessionLogs().Should().BeEmpty();

                // assert outgoing
                var outgoingRequests = client.GetSessionOutgoingRequests();
                outgoingRequests.Count.Should().Be(30);
                outgoingRequests[0].GetEndpoint().Should().Be($"GET {matrixMovieUrl}");
                outgoingRequests[0].GetHeaderValue("Referer").Should().Be(MovieProject.Logic.Constants.Website);
            }

            // one more time should cause an error
            var httpResponse = await client.GetAsync("/api/movie/matrix");

            // assert logs
            var logs = client.GetSessionLogs();
            logs.Should().HaveCount(1);
            logs[0].Message.Should().Contain("No stubs found for");

            httpResponse.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        }

        [Fact]
        public async Task When_UserAsksForMovieThatDoesntExist_Then_Return400Status()
        {
            // arrange
            var client = Fixture.Server.CreateClient();
            client.CreateSession();
            var response = ResponseFactory.FromRecordedFile($"{Fixture.StubsFolder}/OmdbApi/Real_Responses/Happy/200_MovieNotFound.txt");
            client.AppendHttpCallStub(HttpMethod.Get, new System.Uri($"{MovieUrl}&t=some_weird_title"), response);

            // act
            var httpResponse = await client.GetAsync("/api/movie/some_weird_title");

            using (new AssertionScope())
            {
                // assert logs
                client.GetSessionLogs().Should().BeEmpty();

                // assert outgoing
                var outgoingRequests = client.GetSessionOutgoingRequests();
                outgoingRequests.Count.Should().Be(1);
                outgoingRequests[0].GetEndpoint().Should().Be($"GET {MovieUrl}&t=some_weird_title");
                outgoingRequests[0].GetHeaderValue("Referer").Should().Be(MovieProject.Logic.Constants.Website);

                // assert return
                httpResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
                var message = await httpResponse.ReadBody();
                message.Should().Be("Search terms didn't match any movie");
            }
        }

        [Fact]
        public async Task When_UserAsksForMoviFewFields_Then_ReturnMovieProperly()
        {
            // arrange
            var client = Fixture.Server.CreateClient();
            client.CreateSession();
            var response = ResponseFactory.FromFiddlerLikeResponseFile($"{Fixture.StubsFolder}/OmdbApi/Real_Responses/Happy/200_FewFields_OldMovie.txt");
            var comeAlongMovieUrl = $"{MovieUrl}&t=come along, do";
            client.AppendHttpCallStub(HttpMethod.Get, new System.Uri(comeAlongMovieUrl), response);

            // act
            var httpResponse = await client.GetAsync("/api/movie/come along, do");

            using (new AssertionScope())
            {
                // assert logs
                client.GetSessionLogs().Should().BeEmpty();

                // assert outgoing
                var outgoingRequests = client.GetSessionOutgoingRequests();
                outgoingRequests.Count.Should().Be(1);
                outgoingRequests[0].GetEndpoint().Should().Be($"GET {comeAlongMovieUrl}");
                outgoingRequests[0].GetHeaderValue("Referer").Should().Be(MovieProject.Logic.Constants.Website);

                // assert return
                httpResponse.StatusCode.Should().Be(HttpStatusCode.OK);

                var movie = await httpResponse.ReadJsonBody<MovieProject.Logic.DTO.Media>();
                movie.Id.Should().Be("tt0000182");
                movie.Name.Should().Be("Come Along, Do!");
                movie.Year.Should().Be("1898");
                movie.Runtime.Should().Be("1 min");
                movie.Plot.Should().Be("A couple look at a statue while eating in an art gallery.");
            }
        }

        [Fact]
        public async Task When_UserAsksForMovieWithSomeInvalidValues_Then_ReturnMovieProperly()
        {
            // arrange
            var client = Fixture.Server.CreateClient();
            client.CreateSession();
            // we rely on the global stub here

            // act
            var httpResponse = await client.GetAsync("/api/movie/fantastika");

            using (new AssertionScope())
            {
                // assert logs
                client.GetSessionLogs().Should().BeEmpty();

                // assert outgoing
                var outgoingRequests = client.GetSessionOutgoingRequests();
                outgoingRequests.Count.Should().Be(1);
                outgoingRequests[0].GetHeaderValue("Referer").Should().Be(MovieProject.Logic.Constants.Website);

                // assert return
                httpResponse.StatusCode.Should().Be(HttpStatusCode.OK);

                var movie = await httpResponse.ReadJsonBody<MovieProject.Logic.DTO.Media>();
                movie.Id.Should().Be("tt1185643");
                movie.Name.Should().Be("Fantastika vs. Wonderwoman");
                movie.Runtime.Should().Be("Unknown");
                movie.Year.Should().Be("Unknown");
                movie.Plot.Should().Be("");
            }
        }


        [Fact]
        public async Task When_UserAddsMovieToResearchQueue_Then_Success()
        {
            // arrange
            var client = Fixture.Server.CreateClient();
            client.CreateSession();
            var response = ResponseFactory.FromBodyOnlyFile($"{Fixture.StubsFolder}/OmdbApi/Fake_Responses/Happy/200_AddToQueue.json", HttpStatusCode.OK);
            var matrixMovieUrl = $"{MovieUrl}";
            client.AppendHttpCallStub(HttpMethod.Post, new System.Uri(matrixMovieUrl), response);

            // act
            var httpResponse = await client.PostAsJsonAsync("/api/movie?imdb=tt000001   ", new { });

            using (new AssertionScope())
            {
                // assert logs
                client.GetSessionLogs().Should().BeEmpty();

                // assert outgoing
                var outgoingRequests = client.GetSessionOutgoingRequests();
                outgoingRequests.Count.Should().Be(1);
                outgoingRequests[0].GetEndpoint().Should().Be($"POST {matrixMovieUrl}");
                outgoingRequests[0].GetHeaderValue("Referer").Should().Be(MovieProject.Logic.Constants.Website);
                var request = await outgoingRequests[0].ReadJsonBody<MovieProject.Logic.DTO.Media>();
                request.Should().NotBeNull();
                request.Id.Should().Be("tt000001");
                request.Name.Should().Be("TO BE RESEARCHED"); // check if outgoing post body is correctly formatted

                // assert return
                httpResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            }
        }
    }
}

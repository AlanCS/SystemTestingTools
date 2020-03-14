using Shouldly;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace SystemTestingTools.UnitTests
{
    [Trait("Project", "SystemTestingTools Unit Tests (unhappy)")]
    public class RequestResponseRecorderTests
    {
        [Fact]
        public async Task When_ValidRequestAndResponsesAreFired_Then_RecorderCanGetSpecificData()
        {
            // arrange
            var request = new HttpRequestMessage(HttpMethod.Post, "https://www.whatever.com/someendpoint");
            request.Headers.Add("User-Agent", "MyApp");
            string requestBody = @"{""user"":""Alan"", ""trickyField"":""--!?@Divider:"", ""trickyField2"":""HTTP/1.1 200 OK""}";
            request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");

            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Headers.Add("Server", "Kestrel");
            string responseBody = @"{""value"":""whatever"", ""trickyField"":""--!?@Divider:"", ""trickyField2"":""HTTP/1.1 200 OK""}";
            response.Content = new StringContent(responseBody, Encoding.UTF8, "application/json");

            RequestResponseRecorder.FileWriter = NSubstitute.Substitute.For<IFileWriter>();

            var sut = new RequestResponseRecorder("");            

            // act
            var result =  await sut.Summarize(request, response);

            // asserts
            result.ShouldNotBeNull();

            result.Metadata.LocalMachine = System.Environment.MachineName;
            result.Metadata.RequestedByCode.ShouldEndWith(@"Tool\SystemTestingTools.UnitTests\RequestResponseRecorderTests.cs");
            result.Metadata.ToolUrl.ShouldBe(Constants.Website);
            result.Metadata.ToolNameAndVersion.ShouldBe("SystemTestingTools 1.2.0.0");
            result.Metadata.User.ShouldContain(System.Environment.UserName);
            result.Metadata.Timezone.ShouldNotBeNullOrWhiteSpace();
            result.Metadata.DateTime.ShouldBeLessThan(System.DateTime.Now.AddSeconds(1));
            result.Metadata.DateTime.ShouldBeGreaterThan(System.DateTime.Now.AddSeconds(-1));

            result.Request.Method.ShouldBe(HttpMethod.Post);
            result.Request.Url.ShouldBe("https://www.whatever.com/someendpoint");
            result.Request.Body.ShouldBe(requestBody);
            result.Request.Headers.Count.ShouldBe(2);
            result.Request.Headers["User-Agent"].ShouldBe("MyApp");
            result.Request.Headers["Content-Type"].ShouldBe("application/json; charset=utf-8");           

            result.Response.HttpVersion.ShouldBe(new System.Version(1, 1));
            result.Response.Status.ShouldBe(HttpStatusCode.OK);
            result.Response.Body.ShouldBe(responseBody);
            result.Response.Headers.Count.ShouldBe(2);
            result.Response.Headers["Server"].ShouldBe("Kestrel");
            result.Response.Headers["Content-Type"].ShouldBe("application/json; charset=utf-8");
        }
    }
}

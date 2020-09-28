
using FluentAssertions;
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using SystemTestingTools.Internal;
using Xunit;

namespace SystemTestingTools.UnitTests
{
    [Trait("Project", "SystemTestingTools Unit Tests (unhappy)")]
    public class RecordingFormatterTests
    {
        [Fact]
        public async Task Summarize_Maps_Fields_Properly()
        {
            //arrange
           var request = new HttpRequestMessage(HttpMethod.Post, "https://www.whatever.com/someendpoint");
            request.Headers.Add("User-Agent", "MyApp");
            string requestBody = @"{""user"":""Alan"", ""trickyField"":""--!?@Divider:"", ""trickyField2"":""HTTP/1.1 200 OK""}";
            request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");

            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Headers.Add("Server", "Kestrel");
            string responseBody = @"{""value"":""whatever"", ""trickyField"":""--!?@Divider:"", ""trickyField2"":""HTTP/1.1 200 OK""}";
            response.Content = new StringContent(responseBody, Encoding.UTF8, "application/json");


            // act
            var result = await RecordingFormatter.Summarize(request, response, TimeSpan.FromMilliseconds(1000));

            // asserts
            result.Should().NotBeNull();

            result.Metadata.LocalMachine = System.Environment.MachineName;

            result.Metadata.RecordedFrom.Should().NotBeNullOrEmpty();
            result.Metadata.RecordedFrom.Should().EndWith(@"(No httpcontext available)");

            result.Metadata.ToolUrl.Should().Be(Constants.Website);
            result.Metadata.ToolNameAndVersion.Should().StartWith("SystemTestingTools ");
            result.Metadata.User.Should().Contain(System.Environment.UserName);
            result.Metadata.Timezone.Should().NotBeNullOrWhiteSpace();
            result.Metadata.DateTime.Should().BeAfter(System.DateTime.Now.AddSeconds(-1));
            result.Metadata.DateTime.Should().BeBefore(System.DateTime.Now.AddSeconds(1));
            result.Metadata.latencyMiliseconds.Should().Be(1000);

            result.Request.Method.Should().Be(HttpMethod.Post);
            result.Request.Url.Should().Be("https://www.whatever.com/someendpoint");
            result.Request.Body.Should().Be(requestBody);
            result.Request.Headers.Count.Should().Be(2);
            result.Request.Headers["User-Agent"].Should().Be("MyApp");
            result.Request.Headers["Content-Type"].Should().Be("application/json; charset=utf-8");

            result.Response.HttpVersion.Should().Be(new System.Version(1, 1));
            result.Response.Status.Should().Be(HttpStatusCode.OK);
            result.Response.Body.Should().Be(responseBody);
            result.Response.Headers.Count.Should().Be(2);
            result.Response.Headers["Server"].Should().Be("Kestrel");
            result.Response.Headers["Content-Type"].Should().Be("application/json; charset=utf-8");
        }
    }
}

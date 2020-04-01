using System.Text.RegularExpressions;
using Xunit;
using Shouldly;
using System.Net;
using System.Threading.Tasks;

namespace SystemTestingTools.UnitTests
{
    [Trait("Project", "SystemTestingTools Unit Tests (happy)")]
    public class StubResponseFactoryHappyTests
    {
        private string FilesFolder;
        public StubResponseFactoryHappyTests()
        {
            FilesFolder = new Regex(@"\\bin\\.*").Replace(System.Environment.CurrentDirectory, "") + @"\files\";
        }

        [Fact]
        public async Task Read401File()
        {
            var sut = ResponseFactory.FromFiddlerLikeResponseFile( FilesFolder + @"happy\401_InvalidKey.txt");

            sut.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
            sut.Headers.ShouldContainHeader("Server", "cloudflare");
            sut.Headers.ShouldContainHeader("CF-RAY", "4afa0cb0efb66563-SYD");

            var body = await sut.GetResponseString();

            body.ShouldBe(@"{""Response"":""False"",""Error"":""Invalid API key!""}");
        }

        [Fact]
        public async Task Read401File_WithoutBody()
        {
            var sut = ResponseFactory.FromFiddlerLikeResponseFile( FilesFolder + @"happy\401_Unauthorized_WithoutBody.txt");

            sut.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);

            sut.Content.Headers.ShouldContainHeader("Content-Length", "0");
            sut.Content.Headers.ShouldContainHeader("Expires", "Sat, 02 Mar 2019 02:53:56 GMT");

            sut.Headers.ShouldContainHeader("Server", "Kestrel");
            sut.Headers.ShouldContainHeader("Request-Context", "appId=cid-v1:494f2efb-2457-4434-a651-e820e8fa4933");
            sut.Headers.ShouldContainHeader("Strict-Transport-Security", "max-age=2592000");
            sut.Headers.ShouldContainHeader("Request-Id", "|27bb47e5-4fd78bc240436a6d.");
            sut.Headers.ShouldContainHeader("X-Powered-By", "ASP.NET");            
            sut.Headers.ShouldContainHeader("Date", "Sat, 02 Mar 2019 02:53:56 GMT");
            sut.Headers.ShouldContainHeader("Connection", "keep-alive");
            sut.Headers.ShouldContainHeader("x-cache-hit", "false");
            sut.Headers.ShouldContainHeader("Cache-Control", "no-store, must-revalidate, no-cache"); // this seems to be resorted for some weird reason, but all the elements are still here :)

            var body = await sut.GetResponseString();

            body.ShouldBeNullOrEmpty();
        }


        [Fact]
        public async Task Read424File()
        {
            var sut = ResponseFactory.FromFiddlerLikeResponseFile(FilesFolder + @"happy\424_NoHeaders.txt");

            sut.StatusCode.ShouldBe(HttpStatusCode.FailedDependency);

            var body = await sut.GetResponseString();

            body.ShouldBe(@"{""Content"":""No headers found here""}");
        }

        [Fact]
        public async Task Read200File()
        {
            var sut = ResponseFactory.FromFiddlerLikeResponseFile(FilesFolder + @"happy\200_MovieNotFound.txt");

            sut.StatusCode.ShouldBe(HttpStatusCode.OK);
            
            sut.Content.Headers.ShouldContainHeader("Content-Type", "application/json; charset=utf-8");
            sut.Content.Headers.ShouldContainHeader("Content-Length", "47");
            sut.Content.Headers.ShouldContainHeader("Expires", "Thu, 28 Feb 2019 10:42:23 GMT");
            sut.Content.Headers.ShouldContainHeader("Last-Modified", "Wed, 27 Feb 2019 10:42:23 GMT");

            sut.Headers.ShouldContainHeader("Date", "Wed, 27 Feb 2019 10:42:23 GMT");
            sut.Headers.ShouldContainHeader("Set-Cookie", "__cfduid=d336dbbaa07967596b7d935784ddf84471551264143; expires=Thu, 27-Feb-20 10:42:23 GMT; path=/; domain=.omdbapi.com; HttpOnly");
            sut.Headers.ShouldContainHeader("Cache-Control", "public, max-age=86400");                        
            sut.Headers.ShouldContainHeader("Vary", "*");
            sut.Headers.ShouldContainHeader("X-AspNet-Version", "4.0.30319");
            sut.Headers.ShouldContainHeader("X-Powered-By", "ASP.NET");
            sut.Headers.ShouldContainHeader("Access-Control-Allow-Origin", "*");
            sut.Headers.ShouldContainHeader("CF-Cache-Status", "MISS");
            sut.Headers.ShouldContainHeader("CF-RAY", "4afa0b5f1b756563-SYD");
            sut.Headers.ShouldContainHeader("Server", "cloudflare");

            var body = await sut.GetResponseString();

            body.ShouldBe(@"{
  ""Response"": ""False"",
  ""Error"": ""Movie not found!""
}");
        }

        [Fact]
        public async Task Read200WithComments()
        {
            var sut = ResponseFactory.FromFiddlerLikeResponseFile(FilesFolder + @"happy\200_WithComments.txt");

            sut.StatusCode.ShouldBe(HttpStatusCode.OK);

            var body = await sut.GetResponseString();

            body.ShouldBe(@"{""Content"":""testing comments""}");

            sut.Content.Headers.ShouldContainHeader("Content-Type", "application/json; charset=utf-8");
            sut.Content.Headers.ShouldContainHeader("Expires", "Mon, 04 Mar 2019 07:17:49 GMT");
            sut.Content.Headers.ShouldContainHeader("Last-Modified", "Sun, 03 Mar 2019 07:17:48 GMT");

            sut.Headers.ShouldContainHeader("Date", "Sun, 03 Mar 2019 07:17:49 GMT");
            sut.Headers.ShouldContainHeader("Cache-Control", "public, max-age=86400");
            sut.Headers.ShouldContainHeader("Vary", "*");
            sut.Headers.ShouldContainHeader("X-AspNet-Version", "4.0.30319");
            sut.Headers.ShouldContainHeader("X-Powered-By", "ASP.NET");
            sut.Headers.ShouldContainHeader("Access-Control-Allow-Origin", "*");
            sut.Headers.ShouldContainHeader("CF-Cache-Status", "MISS");
            sut.Headers.ShouldContainHeader("CF-RAY", "4b19d532ce5419aa-SYD");
            sut.Headers.ShouldContainHeader("Server", "cloudflare");
        }

        [Fact]
        public async Task FromBodyOnlyFile()
        {
            var fullFileName = FilesFolder + @"happy\OnlyBody.txt";

            var sut = ResponseFactory.FromBodyOnlyFile(fullFileName, HttpStatusCode.OK);

            sut.StatusCode.ShouldBe(HttpStatusCode.OK);

            var body = await sut.GetResponseString();

            body.ShouldBe(@"{""Content"":""Only the raw body found here!""}");

            sut.Headers.ShouldBeEmpty();
        }

        [Fact]
        public async Task FromMemory()
        {
            var content = @"{""Content"":""this will be in the body""}";

            var sut = ResponseFactory.From(content, HttpStatusCode.OK);

            sut.StatusCode.ShouldBe(HttpStatusCode.OK);

            var body = await sut.GetResponseString();

            body.ShouldBe(content);
        }
    }
}

using FluentAssertions;
using MovieProject.Logic.Proxy.DTO;
using NSubstitute;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using SystemTestingTools.Internal;
using Xunit;

namespace SystemTestingTools.UnitTests
{
    [Trait("Project", "SystemTestingTools Unit Tests (unhappy)")]
    public class RecordingManagerTests
    {
        readonly string folder = Path.Combine(Path.GetTempPath(),"SystemTestingTools");

        readonly string expectedFileContent = @"SystemTestingTools_Recording_V2
Observations: !! EXPLAIN WHY THIS EXAMPLE IS IMPORTANT HERE !!
Date: 2019-03-17 15:44:37.597 my_time_zone
Recorded from: MovieProject.Web 1.2.0.1 (htts://localhost/api/whatever?param=2)
Local machine: Machine001
User: A01/MyUser
Using tool: SystemTestingTools 0.1.0.0 (http://www.whatever.com)
Duration: 1200 ms

REQUEST
post https://www.whatever.com/someendpoint
User-Agent:MyApp

{""user"":""Alan"", ""trickyField"":""--!?@Divider:"", ""trickyField2"":""HTTP/1.1 200 OK""}

--!?@Divider: Any text BEFORE this line = comments, AFTER = response in Fiddler like format

HTTP/1.1 200 OK
Server:Kestrel

{""value"":""whatever"", ""trickyField"":""--!?@Divider:"", ""trickyField2"":""HTTP/1.1 200 OK""}
";

        [Fact]
        public async Task When_FilesAlreadyExistInFolder_And_ValidRequestResponse_Then_CreateTextFileInRightFormat_And_CanLoadFile()
        {
            // this test relies on usage of the FileSystem, it's unusual, but considered best balance 
            // of efficient + realist testing vs potential downsides, using the temporary folder of the machine
            // should be ok

            // arrange
            if(Directory.Exists(folder)) Directory.Delete(folder, true); // if folder exists, it was the result of previous tests

            // create folder and some files in it, so we make sure our code creates the next file correctly
            Directory.CreateDirectory(folder);
            File.CreateText(Path.Combine(folder, "OK_00001.txt"));
            File.CreateText(Path.Combine(folder, "Forbidden.txt"));
            File.CreateText(Path.Combine(folder, "Whatever.txt"));
            File.CreateText(Path.Combine(folder, "some_random_file.txt"));

            var sut = new RecordingManager(folder);

            var input = CreateLog();

            // act
            var fileName = sut.Save(input);

            // asserts
            fileName.Should().Be("OK_0002");
            var createdFile = Path.Combine(folder, "OK_0002.txt");
            File.Exists(createdFile).Should().BeTrue();

            var content = File.ReadAllText(createdFile);

            content.Should().Be(expectedFileContent);

            var deserializedResponse = ResponseFactory.FromRecordedFile(createdFile);

            deserializedResponse.StatusCode.Should().Be(input.Response.Status);
            (await deserializedResponse.Content.ReadAsStringAsync()).Should().Be(input.Response.Body);

            foreach (var item in input.Response.Headers)
                deserializedResponse.Headers.ShouldContainHeader(item.Key, item.Value);
        }

        [Fact]
        public void When_Files_Dont_Exist_Create_First_File()
        {
            var fileSystem = Substitute.For<IFileSystemFacade>();
            fileSystem.GetTextFileNames(folder, "bla").Returns(new List<string>());
            var sut = new RecordingManager(folder);
            sut._fileSystem = fileSystem;

            // act
            var fileName = sut.Save(CreateLog(), folder, "bla");

            // asserts
            fileName.Should().Be("bla_0001");
        }

        [Fact]
        public async Task GetRecordings_Including_Old_One_And_ResendAndUpdate_Works()
        {
            var folder = Path.Combine(Path.GetFullPath("../../../"), "files/recordings");

            var destinationFolder = folder.Replace("/recordings", "/recordings_temp");

            // we copy files from original folder to another one, because they will be updated, and we need to keep the original files for 
            // repeatable unit testing
            CopyFolderAndFiles(folder, destinationFolder); 

            var sut = new RecordingManager(destinationFolder);

            // act
            var recordings = sut.GetRecordings(destinationFolder);

            // asserts
            recordings.Count.Should().Be(3);

            recordings[0].File.Should().Be(@"happy\200_ContainsSomeFields_PacificChallenge");
            recordings[0].FileFullPath.Should().EndWith(@"recordings_temp\happy\200_ContainsSomeFields_PacificChallenge.txt");

            await AssertHappyRecording(recordings[1]);
            await AssertUnhappyRecording(recordings[2]);

            await recordings.ReSendRequestAndUpdateFile();

            recordings.Should().OnlyContain(c => c.DateTime >= DateTime.Now.AddSeconds(-1));

            Directory.Delete(destinationFolder, true); // clear folder as not needed anymore
        }


        public static void CopyFilesRecursively(DirectoryInfo source, DirectoryInfo target)
        {
            foreach (DirectoryInfo dir in source.GetDirectories())
                CopyFilesRecursively(dir, target.CreateSubdirectory(dir.Name));
            foreach (FileInfo file in source.GetFiles())
                file.CopyTo(Path.Combine(target.FullName, file.Name));
        }

        public static void CopyFolderAndFiles(string sourcePath, string targetPath)
        {
            var source = new DirectoryInfo(sourcePath);
            var target = new DirectoryInfo(targetPath);

            if (target.Exists)
                target.Delete(true);

            target.Create();

            CopyFilesRecursively(source, target);
        }

        private static async Task AssertHappyRecording(Recording recording)
        {
            recording.DateTime.Should().Be(DateTime.Parse("2020-08-18 20:26:34.231"));
            recording.File.Should().Be(@"happy\TheMatrix");

            recording.Request.Should().NotBeNull();
            recording.Request.GetEndpoint().Should().Be("get http://www.omdbapi.com/?apikey=863d6589&type=movie&t=matrix");
            recording.Request.Headers.Count().Should().Be(2);
            recording.Request.GetHeaderValue("Referer").Should().Be("https://github.com/AlanCS/SystemTestingTools");
            recording.Request.GetHeaderValue("Request-Id").Should().Be("|5de5fb2f-495c9d0e1abf3ad2.1.");

            recording.Response.Should().NotBeNull();
            recording.Response.StatusCode.Should().Be(HttpStatusCode.OK);
            recording.Response.GetHeaderValue("Cache-Control").Should().Be("public, max-age=86400");
            recording.Response.GetHeaderValue("CF-RAY").Should().Be("5c4aefbf09dc16b5-SYD");
            recording.Response.Content.GetHeaderValue("Content-Type").Should().Be("application/json; charset=utf-8");

            var mediaResponse = await recording.Response.ReadJsonBody<ExternalMedia>();

            mediaResponse.Should().NotBeNull();
            mediaResponse.ImdbId.Should().Be("tt0133093");
            mediaResponse.Title.Should().Be("The Matrix");
            mediaResponse.Awards.Should().Be("Won 4 Oscars. Another 37 wins & 50 nominations.");
            mediaResponse.ImdbVotes.Should().Be("1,624,177");
            mediaResponse.Director.Should().Be("Lana Wachowski, Lilly Wachowski");
        }

        private static async Task AssertUnhappyRecording(Recording recording)
        {
            recording.DateTime.Should().Be(DateTime.Parse("2020-08-25 19:22:12.829"));
            recording.File.Should().Be(@"unhappy\post_new");

            recording.Request.Should().NotBeNull();
            recording.Request.GetEndpoint().Should().Be("post http://www.omdbapi.com/?apikey=863d6589&type=movie");
            recording.Request.Headers.Should().HaveCountGreaterOrEqualTo(3);
            recording.Request.GetHeaderValue("Referer").Should().Be("https://github.com/AlanCS/SystemTestingTools");
            recording.Request.GetHeaderValue("Request-Id").Should().Be("|d965bbc7-4858cbfaf0e34479.1.");
            recording.Request.GetHeaderValue("SystemTestingTools_Stubs").Should().Be("gibberish");

            var mediaRequest = await recording.Request.ReadJsonBody<MovieProject.Logic.DTO.Media>();

            mediaRequest.Should().NotBeNull();
            mediaRequest.Id.Should().Be("tt00011");
            mediaRequest.Name.Should().Be("TO BE RESEARCHED");

            recording.Response.Should().NotBeNull();
            recording.Response.StatusCode.Should().Be(HttpStatusCode.OK);
            recording.Response.GetHeaderValue("Cache-Control").Should().Be("public, max-age=3600");
            recording.Response.GetHeaderValue("CF-RAY").Should().Be("5c843f47ee01d699-SYD");
            recording.Response.Content.GetHeaderValue("Content-Type").Should().Be("application/json; charset=utf-8");

            var mediaResponse = await recording.Response.ReadJsonBody<ExternalMedia>();

            mediaResponse.Should().NotBeNull();
            mediaResponse.Response.Should().Be("False");
            mediaResponse.Error.Should().Be("Incorrect IMDb ID.");
        }

        private RequestResponse CreateLog()
        {
            var log = new RequestResponse()
            {
                Metadata = new RequestResponse.MetadataInfo()
                {
                    DateTime = System.DateTime.Parse("2019-03-17 15:44:37.597"),
                    Timezone = "my_time_zone",
                    LocalMachine = "Machine001",
                    User = "A01/MyUser",
                    RecordedFrom = @"MovieProject.Web 1.2.0.1 (htts://localhost/api/whatever?param=2)",
                    ToolUrl = "http://www.whatever.com",
                    ToolNameAndVersion = "SystemTestingTools 0.1.0.0",
                    latencyMiliseconds = 1200
                },
                Request = new RequestResponse.RequestInfo()
                {
                    Method = HttpMethod.Post,
                    Url = "https://www.whatever.com/someendpoint",
                    Body = @"{""user"":""Alan"", ""trickyField"":""--!?@Divider:"", ""trickyField2"":""HTTP/1.1 200 OK""}",
                    Headers = new Dictionary<string, string>() { { "User-Agent", "MyApp" } }
                },
                Response = new RequestResponse.ResponseInfo()
                {
                    Status = HttpStatusCode.OK,
                    Body = @"{""value"":""whatever"", ""trickyField"":""--!?@Divider:"", ""trickyField2"":""HTTP/1.1 200 OK""}",
                    HttpVersion = new System.Version(1, 1),
                    Headers = new Dictionary<string, string>() { { "Server", "Kestrel" } }
                }
            };

            return log;
        }
    }
}

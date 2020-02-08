using System;
using System.Text.RegularExpressions;
using Xunit;
using Shouldly;

namespace SystemTestingTools.UnitTests
{
    [Trait("Project", "SystemTestingTools Unit Tests (unhappy)")]
    public class MockResponseFactoryUnhappyTests
    {
        private string FilesFolder;
        public MockResponseFactoryUnhappyTests()
        {
            FilesFolder = new Regex(@"\\bin\\.*").Replace(System.Environment.CurrentDirectory, "") + @"\files\";
        }

        [Fact]
        public void FileDoesntExist()
        {
            var fullFileName = FilesFolder + "ThisFileDoesntExist.txt";            

            Action act = () => ResponseFactory.FromFiddlerLikeResponseFile(fullFileName);

            var exception = Assert.Throws<ArgumentException>(act);
            exception.Message.ShouldBe($"Could not find file '{fullFileName}'");
        }

        [Fact]
        public void FileIsEmpty()
        {
            var fullFileName = FilesFolder + @"unhappy\Empty.txt";

            Action act = () => ResponseFactory.FromFiddlerLikeResponseFile(fullFileName);

            var exception = Assert.Throws<ArgumentException>(act);
            exception.Message.ShouldBe($"File content is empty");
        }

        [Fact]
        public void NoHttpStatusCode()
        {
            var fullFileName = FilesFolder + @"unhappy\NoHttpStatusCode.txt";

            Action act = () => ResponseFactory.FromFiddlerLikeResponseFile(fullFileName);

            var exception = Assert.Throws<ArgumentException>(act);
            exception.Message.ShouldBe($"File is not in the right format, please consult {Constants.Website}");
        }

        [Fact]
        public void OnlyBody()
        {
            // should throw exception if we try to read with method FromFiddlerLikeResponseFile, but not FromBodyOnlyFile
            var fullFileName = FilesFolder + @"unhappy\OnlyBody.txt";

            Action act = () => ResponseFactory.FromFiddlerLikeResponseFile(fullFileName);

            var exception = Assert.Throws<ArgumentException>(act);
            exception.Message.ShouldBe($"File is not in the right format, please consult {Constants.Website}");
        }

        [Fact]
        public void InvalidHttpStatus()
        {
            var fullFileName = FilesFolder + @"unhappy\InvalidHttpStatus.txt";

            Action act = () => ResponseFactory.FromFiddlerLikeResponseFile(fullFileName);

            var exception = Assert.Throws<ArgumentException>(act);
            exception.Message.ShouldBe($"Not a valid Http Status code: 800");
        }
    }
}

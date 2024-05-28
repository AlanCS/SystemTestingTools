using FluentAssertions;
using FluentAssertions.Execution;
using System;
using System.IO;
using System.Numerics;
using System.Text.RegularExpressions;
using Xunit;

namespace SystemTestingTools.UnitTests
{
    public class ValueObjectsTests
    {
        private string FilesFolder;
        public ValueObjectsTests()
        {
            FilesFolder = EnvironmentHelper.GetProjectFolder("files");
        }

        [Fact]
        public void FileName_Happy()
        {
            FileName file = "Success";
        }

        [Theory]
        [InlineData(" ", null)]
        [InlineData("Success.txt", "Success.txt should not contain extension")]
        [InlineData("/Success", "/Success contains invalid chars")]
        public void FileName_Unhappy(string fileName, string expectedMessage)
        {
            if (expectedMessage == null)
            {
                var ex = Assert.Throws<ArgumentNullException>(() => { FileName file = fileName; });
                ex.Message.Should().Be("Value cannot be null. (Parameter 'value')");
            }
            else
            {
                var ex = Assert.Throws<ArgumentException>(() => { FileName file = fileName; });
                ex.Message.Should().Be(expectedMessage);
            }
        }

        [Fact]
        public void FileFullPath_Happy()
        {
            FileFullPath file1 = FilesFolder + @"/happy/401_InvalidKey";
            FileFullPath file2 = FilesFolder + @"/happy/401_InvalidKey.txt";
        }

        [Fact]
        public void FileFullPath_Empty_Unhappy()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => { FileFullPath file = "    "; });
            ex.Message.Should().Be("Value cannot be null. (Parameter 'value')");
        }

        [Fact]
        public void FileFullPath_FolderDoesntExist_Unhappy()
        {
            var fullFileName = FilesFolder + @"happy/401_InvalidKeyAAA";

            var ex = Assert.Throws<DirectoryNotFoundException>(() => { FileFullPath file = fullFileName; });

            var folder = Path.GetDirectoryName(fullFileName);

            ex.Message.Should().StartWith($"Could not find folder '{folder}', the only folder that exist is ");
        }

        [Fact]
        public void FolderAbsolutePath_Happy()
        {
            FolderAbsolutePath folder = FilesFolder + "/happy";

            folder.AppendPath(null).Should().Be(folder);
            folder.AppendPath("bla").Should().Be(Path.Combine(folder,"bla"));
        }

        [Fact]
        public void FolderAbsolutePath_Empty_Unhappy()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => { FolderAbsolutePath file = "    "; });
            ex.Message.Should().Be("Value cannot be null. (Parameter 'value')");
        }

        [Fact]
        public void FolderAbsolutePath_Folder_DoesntExist()
        {
            var folder = FilesFolder + "happyAAA";
            var ex = Assert.Throws<ArgumentException>(() => { FolderAbsolutePath target = folder; });
            ex.Message.Should().Be($"Folder does not exist: {folder}");
        }

        [Fact]
        public void FolderRelativePath_Happy()
        {
            FolderRelativePath folder = "new/happy/movie";
        }

        [Theory]
        [InlineData(" ", null)]
        [InlineData("some/invalid|path\0", "Invalid chars for folder name found: some/invalid|path\0")]
        public void FolderRelativePath_Unhappy(string folder, string expectedMessage)
        {
            if (expectedMessage == null)
            {
                var ex = Assert.Throws<ArgumentNullException>(() => { FolderRelativePath target = folder; });
                ex.Message.Should().Be("Value cannot be null. (Parameter 'value')");
            }
            else
            {
                var ex = Assert.Throws<ArgumentException>(() => { FolderRelativePath target = folder; });
                ex.Message.Should().Be(expectedMessage);
            }
        }

        [Fact]
        public void RelativeUri_Happy()
        {
            var url = "stub/here";
            RelativeUri uri1 = url;
            RelativeUri uri2 = "/" + url;
            RelativeUri uri3 = url + "/";

            using (new AssertionScope())
            {
                uri1.ToString().Should().Be(url);
                uri2.ToString().Should().Be(url);
                uri3.ToString().Should().Be(url);
            }
        }


        [Theory]
        [InlineData(" ", null)]
        [InlineData("Stubs|here", "Not a valid Uri path: Stubs|here")]
        public void RelativeUri_Unhappy(string path, string expectedMessage)
        {
            if (expectedMessage == null)
            {
                var ex = Assert.Throws<ArgumentNullException>(() => { RelativeUri target = path; });
                ex.Message.Should().Be("Value cannot be null. (Parameter 'value')");
            }
            else
            {
                var ex = Assert.Throws<ArgumentException>(() => { RelativeUri target = path; });
                ex.Message.Should().Be(expectedMessage);
            }
        }
    }
}

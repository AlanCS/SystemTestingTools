using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

namespace SystemTestingTools
{
    internal interface IFileSystemFacade
    {
        bool FolderExists(string folderPath);
        List<string> GetTextFileNames(string folderPath);
        void CreateNewTextFile(string folderPath, string fileName, string content);
    }

    internal class FileSystemFacade : IFileSystemFacade
    {
        public bool FolderExists(string folderPath)
        {
            return Directory.Exists(folderPath);
        }

        public List<string> GetTextFileNames(string folderPath)
        {
            var existingFiles = Directory.GetFiles(folderPath, "*.txt");

            var fileNames = existingFiles.Select(c => Path.GetFileName(c)).ToList();

            return fileNames;
        }

        public void CreateNewTextFile(string folderPath, string fileName, string content)
        {
            var fullFileName = Path.Combine(folderPath, $"{fileName}.txt");

            using (var file = File.CreateText(fullFileName))
            {
                file.Write(content);
                file.Close();
            }
        }
    }
}

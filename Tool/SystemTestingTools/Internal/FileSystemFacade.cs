using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SystemTestingTools
{
    internal interface IFileSystemFacade
    {
        bool FolderExists(string folderPath);
        void CreateFile(string fullFileName, string content);
        void DeleteFolder(string folderPath);
        void CreateFolder(string folderPath);
        List<string> GetTextFileNames(string folderPath, string fileName);
        string[] GetTextFileNames(string folderPath);
        string ReadContent(string fullFileName);
    }

    internal class FileSystemFacade : IFileSystemFacade
    {
        public bool FolderExists(string folderPath)
        {
            return Directory.Exists(folderPath);
        }

        public void CreateFolder(string folderPath)
        {
            Directory.CreateDirectory(folderPath);
        }

        public void DeleteFolder(string folderPath)
        {
            Directory.Delete(folderPath, true);
        }

        public string[] GetTextFileNames(string folderPath)
        {
            var existingFiles = Directory.GetFiles(folderPath, "*.txt", SearchOption.AllDirectories);

            return existingFiles;
        }

        public List<string> GetTextFileNames(string folderPath, string fileName)
        {
            var existingFiles = Directory.GetFiles(folderPath, $"{fileName}*.txt");

            var fileNames = existingFiles.Select(c => Path.GetFileName(c)).ToList();

            return fileNames;
        }

        public string ReadContent(string fullFileName)
        {
            return File.ReadAllText(fullFileName);
        }

        public void CreateFile(string fullFileName, string content)
        {
            if (!fullFileName.EndsWith(".txt")) fullFileName += ".txt";

            using (var file = File.CreateText(fullFileName))
            {
                file.Write(content);
                file.Close();
            }
        }
    }
}

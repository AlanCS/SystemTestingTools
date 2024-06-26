﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using SystemTestingTools.Internal;

namespace SystemTestingTools
{
    internal interface IRecordingsManager
    {
        string Save(RequestResponse log, FolderRelativePath relativeFolder = null, FileName fileName = null, int howManyFilesToKeep = 0);
    }

    internal class RecordingManager : IRecordingsManager
    {        
        internal IFileSystemFacade _fileSystem = new FileSystemFacade();

        // gets only file names that start with numbers
        private static Regex fileNameRegex = new Regex(@".+?([0-9]+)\.", RegexOptions.Compiled);
        private readonly FolderAbsolutePath baseDirectory;

        public RecordingManager(FolderAbsolutePath baseDirectory = null)
        {
            this.baseDirectory = baseDirectory;
        }

        public string Save(RequestResponse log, FolderRelativePath relativeFolder = null, FileName fileName = null, int howManyFilesToKeep = 0)
        {            
            var finalFolder = baseDirectory.AppendPath(relativeFolder);

            if (!Directory.Exists(finalFolder))
                Directory.CreateDirectory(finalFolder);

            if (fileName == null) fileName = log.Response.Status.ToString();

            var finalFileName = GetFinalFileName(finalFolder, fileName, howManyFilesToKeep);

            if (finalFileName == null) return null; // limit reached

            SaveToFile(Path.Combine(finalFolder, finalFileName), log);

            return finalFileName;
        }

        public void SaveToFile(string fullFileName, RequestResponse log)
        {
            _fileSystem.CreateFile(fullFileName, RecordingFormatter.Format(log));
        }

        public List<Recording> GetRecordings(FolderAbsolutePath folder)
        {
            var list = new List<Recording>();

            foreach (var fullFilePath in _fileSystem.GetTextFileNames(folder))
            {
                var content = _fileSystem.ReadContent(fullFilePath);
                if (!RecordingFormatter.IsValid(content)) continue;
                var recording = RecordingFormatter.Read(content);
                if (recording == null) continue;
                recording.FileFullPath = fullFilePath;
                recording.File = StandardizeFileNameForDisplay(folder, fullFilePath);
                list.Add(recording);
            }
            list = list.OrderBy(c => c.File).ToList(); // we sort so it's the same order in windows and linux, so we can more easily test it
            return list;
        }

        private static string StandardizeFileNameForDisplay(FolderAbsolutePath folder,  string str)
        {
            // we replace something like C:\Users\AlanPC\Documents\GitHub\SystemTestingTools\Tool\SystemTestingTools.UnitTests\files/recordings\happy\TheMatrix.txt
            // to happy/TheMatrix, so we can easily search by folder name

            return str.Replace(folder, "").Replace(".txt", "").TrimStart('/', '\\').Replace("\\", "/");
        }

        private FileName GetFinalFileName(string finalFolder, FileName fileName, int howManyFilesToKeep)
        {
            if (howManyFilesToKeep == 1) return fileName;

            var total = GetTotalFilesWithSameName(finalFolder, fileName);

            if (howManyFilesToKeep > 0 && total >= howManyFilesToKeep) return null;

            return $"{fileName}_{total + 1:0000}";
        }

        private long GetTotalFilesWithSameName(string folderPath, string fileName)
        {
            // files are created with incremental numbers, IE: OK_00001.txt, InternalServerError_00002.txt
            // we get the current max number

            var max = _fileSystem.GetTextFileNames(folderPath, fileName)
                .Where(c => fileNameRegex.IsMatch(c))
                .Select(c => Convert.ToInt64(fileNameRegex.Match(c).Groups[1].Value))
                .DefaultIfEmpty(0)
                .Max();

            return max;
        }
    }
}

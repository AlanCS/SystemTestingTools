using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;



namespace SystemTestingTools
{
    internal interface IFileWriter
    {
        string Write(RequestResponse log);
    }

    internal class FileWriter : IFileWriter
    {        
        private static long _callsCounter = 0;
        // gets only file names that start with numbers
        private static Regex fileNameRegex = new Regex("([0-9]+)", RegexOptions.Compiled);

        private IFileSystemFacade _fileSystem = new FileSystemFacade();
        private readonly string folder;

        public FileWriter(string Folder)
        {
            if (string.IsNullOrWhiteSpace(Folder)) throw new ArgumentNullException(Folder);
            if (!_fileSystem.FolderExists(Folder)) throw new ArgumentException($"{Folder} does not exist");
            SetupCounter(Folder);
            folder = Folder;
        }

        private void SetupCounter(string SaveToFolder)
        {
            // files are created with incremental numbers, IE: 1_OK.txt, 2_InternalServerError.txt
            // we get the current max number
            var max = _fileSystem.GetTextFileNames(SaveToFolder)
                .Where(c => fileNameRegex.IsMatch(c))
                .Select(c => Convert.ToInt64(fileNameRegex.Match(c).Groups[1].Value))
                .DefaultIfEmpty(0)
                .Max();

            _callsCounter = max;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="log"></param>
        /// <returns>created file name</returns>
        public string Write(RequestResponse log)
        {
            Interlocked.Increment(ref _callsCounter);

            var content = new StringBuilder();

            // details about request
            content.AppendLine($"METADATA");
            content.AppendLine($"Observations: !! EXPLAIN WHY THIS EXAMPLE IS IMPORTANT HERE !!");
            content.AppendLine($"Date: {log.Metadata.DateTime.ToString("yyyy-MM-dd HH:mm:ss.fff")} {log.Metadata.Timezone}");
            content.AppendLine($"Requested by code: {log.Metadata.RequestedByCode}");
            content.AppendLine($"Local machine: {log.Metadata.LocalMachine}");
            content.AppendLine($"User: {log.Metadata.User}");
            content.AppendLine($"Using tool: {log.Metadata.ToolNameAndVersion} ({log.Metadata.ToolUrl})");            

            content.AppendLine();
            content.AppendLine($"REQUEST");
            content.AppendLine($"{log.Request.Method.ToString().ToLower()} {log.Request.Url}");
            AddHeadersToContent(log.Request.Headers);
            if (log.Request.Body != null) content.AppendLine(log.Request.Body);

            // lots of thought went into the bellow lines, because a separator needed to be created to split the response that
            // the method ResponseFactory.FromFiddlerLikeResponseFile can read, from (optional) comments
            // it has been decided that a new line + "--!?@Divider: some text here" + new line would be an acceptable separator
            // as we want to keep files in human readable/editable format
            // we still have the risk (very low) that such string could be found in real use case, it seemed like an acceptable 
            // risk/downside comparing to the upsides

            // we could have put the metadata + request details in a different file, but that would risk the 2 files from getting
            // separated, which defeats the purpose of a high quality documentation file of what request lead to what response
            content.AppendLine();
            content.AppendLine("--!?@Divider: Any text BEFORE this line = comments, AFTER = response in Fiddler like format");
            content.AppendLine();

            // details about response
            content.AppendLine($"HTTP/{log.Response.HttpVersion} {(int)log.Response.Status} {log.Response.Status}");
            AddHeadersToContent(log.Response.Headers);
            content.AppendLine();

            if (log.Response.Body != null)
                content.AppendLine(FormatBody(log.Response.Body, log.Response.Headers.GetValueOrDefault("Content-Type")));

            var filename = $"{_callsCounter}_{log.Response.Status}";
            _fileSystem.CreateNewTextFile(folder, filename, content.ToString().Trim());

            return filename;

            // new feature in C#7 (https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/local-functions)
            void AddHeadersToContent(Dictionary<string, string> headers)
            {
                headers.ToList().ForEach((header) => content.AppendLine($"{header.Key}:{header.Value}"));
            }
        }

        internal string FormatBody(string body, string contentType)
        {
            if (string.IsNullOrEmpty(contentType)) return body;

            contentType = contentType.ToLower();

            // we format JSONs because it's a common format and it makes it way easier to visualize

            if (contentType.StartsWith("application/json"))
                return body.FormatJson();

            return body;
        }
    }
}

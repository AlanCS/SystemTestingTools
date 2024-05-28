using System;
using System.IO;
using System.Linq;
using static SystemTestingTools.Helper;
using static SystemTestingTools.Internal.Enums;

namespace SystemTestingTools
{
    /// <summary>
    /// Full path to a file that exists in disk
    /// </summary>
    public class FileFullPath : StringValueObject
    {

        /// <summary>
        /// Full path to a file that exists in disk
        /// </summary>
        public FileFullPath(string value) : base(value)
        {
            if (!Path.HasExtension(_value)) _value += ".txt";

            var folder = Path.GetDirectoryName(_value);

            CheckFolderExists(folder);

            if (!File.Exists(_value))
            {
                var filesCount = Directory.GetFiles(folder).Length;
                throw new FileNotFoundException($"Could not find file '{_value}', there are {filesCount} other files in the folder {folder}");
            }
        }

        private static void CheckFolderExists(string folder)
        {
            if (Directory.Exists(folder)) return;

            string parentFolder = folder;
            do
            {
                parentFolder = Path.GetDirectoryName(parentFolder);
            } while (!Directory.Exists(parentFolder) && !string.IsNullOrEmpty(parentFolder));


            throw new DirectoryNotFoundException($"Could not find folder '{folder}', the only folder that exist is '{parentFolder}'");
        }

        public string ReadContent()
        {
            string content = File.ReadAllText(_value);

            if (string.IsNullOrEmpty(content)) throw new ArgumentException($"Content of {_value} is empty");

            return content;
        }

        /// <summary>
        /// convert from string
        /// </summary>
        public static implicit operator FileFullPath(string value)
        {
            if (value == null)
                return null;

            return new FileFullPath(value);
        }

        /// <summary>
        /// convert to string
        /// </summary>
        public static implicit operator string(FileFullPath obj) => obj._value;

        internal KnownContentTypes GetExtension()
        {
            var contentType = KnownContentTypes.Other;
            if (_value.EndsWith(".json", StringComparison.CurrentCultureIgnoreCase))
                contentType = KnownContentTypes.Json;
            if (_value.EndsWith(".xml", StringComparison.CurrentCultureIgnoreCase))
                contentType = KnownContentTypes.Xml;

            return contentType;
        }
    }

}

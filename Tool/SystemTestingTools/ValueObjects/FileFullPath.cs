using System;
using System.IO;
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
            if (!File.Exists(_value)) throw new ArgumentException($"Could not find file '{_value}'");
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

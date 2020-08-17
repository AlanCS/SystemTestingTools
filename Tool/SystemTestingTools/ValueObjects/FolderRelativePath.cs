using System;
using System.IO;

namespace SystemTestingTools
{
    /// <summary>
    /// Part of a folder path
    /// </summary>
    public class FolderRelativePath : StringValueObject
    {
        /// <summary>
        /// Part of a folder path
        /// </summary>
        public FolderRelativePath(string value) : base(value)
        {
            if (_value.IndexOfAny(Path.GetInvalidPathChars()) >= 0) throw new ArgumentException($"Invalid chars for folder name found: {_value}");            
        }

        /// <summary>
        /// convert from string
        /// </summary>
        public static implicit operator FolderRelativePath(string value)
        {
            if (value == null)
                return null;

            return new FolderRelativePath(value);
        }

        /// <summary>
        /// convert to string
        /// </summary>
        public static implicit operator string(FolderRelativePath obj) => obj._value;
    }
}

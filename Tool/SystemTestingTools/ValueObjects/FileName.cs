using System;
using System.IO;

namespace SystemTestingTools
{
    /// <summary>
    /// Filename (without an extension)
    /// </summary>
    public class FileName : StringValueObject
    {
        /// <summary>
        /// Filename (without an extension)
        /// </summary>
        public FileName(string value) : base(value)
        {
            if (_value.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0) throw new ArgumentException($"{_value} contains invalid chars");
            if (Path.HasExtension(_value)) throw new ArgumentException($"{_value} should not contain extension");            
        }

        /// <summary>
        /// convert from string
        /// </summary>
        public static implicit operator FileName(string value)
        {
            if (value == null)
                return null;

            return new FileName(value);
        }

        /// <summary>
        /// convert to string
        /// </summary>
        public static implicit operator string(FileName obj) => obj._value;
    }
}

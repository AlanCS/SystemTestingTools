using System;

namespace SystemTestingTools
{
    /// <summary>
    /// Relative Uri to be added to a domain
    /// </summary>
    public class RelativeUri : StringValueObject
    {
        /// <summary>
        /// Relative Uri to be added to a domain
        /// </summary>
        /// <param name="value"></param>
        public RelativeUri(string value) : base(value)
        {
            _value = _value.Trim('/', '\\');
            if (!Uri.IsWellFormedUriString(_value, UriKind.Relative)) throw new ArgumentException($"Not a valid Uri path: {_value}");
        }

        /// <summary>
        /// convert from string
        /// </summary>
        public static implicit operator RelativeUri(string value)
        {
            if (value == null)
                return null;

            return new RelativeUri(value);
        }

        /// <summary>
        /// convert to string
        /// </summary>
        public static implicit operator string(RelativeUri obj) => obj._value;
    }
}

using System;

namespace SystemTestingTools
{
    /// <summary>
    /// Inspired by https://stackoverflow.com/questions/3436101/create-custom-string-class
    /// </summary>
    public abstract class StringValueObject
    {
        /// <summary>
        /// Underlying string value
        /// </summary>
        protected string _value;

        /// <summary>
        /// String backed object
        /// </summary>
        public StringValueObject(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) throw new ArgumentNullException(nameof(value));
            this._value = value;
        }

        public override string ToString() => _value;

        public override int GetHashCode() => _value.GetHashCode();
    }
}

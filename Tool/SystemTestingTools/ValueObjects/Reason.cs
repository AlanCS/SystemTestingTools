namespace SystemTestingTools
{
    /// <summary>
    /// Reason why a stub is being returned
    /// </summary>
    public class Reason : StringValueObject
    {
        /// <summary>
        /// Reason why a stub is being returned
        /// </summary>
        public Reason(string value) : base(value)
        {
            
        }

        /// <summary>
        /// convert from string
        /// </summary>
        public static implicit operator Reason(string value)
        {
            if (value == null)
                return null;

            return new Reason(value);
        }

        /// <summary>
        /// convert to string
        /// </summary>
        public static implicit operator string(Reason obj) => obj._value;
    }
}

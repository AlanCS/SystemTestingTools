using System;

namespace MovieProject.Logic.Exceptions
{
    // useful to have exceptions specializing in downstream dependencies, so we can easily tell them apart in logs
    public class DownstreamException : Exception
    {
        public DownstreamException(string message, Exception innerException = null) : base(message, innerException)
        {

        }
    }
}

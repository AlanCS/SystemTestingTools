using System;

namespace MovieProject.Logic.Exceptions
{
    // useful to have exceptions specializing in downstream dependencies, so we can easily tell them apart in logs
    public class DownstreamException : ApplicationException
    {
        public DownstreamException(string message) : base(message)
        {
            
        }
    }
}

using System;
using System.Net;
using System.Net.Http;

namespace SystemTestingTools
{
    /// <summary>
    /// A HttpResponseMessage representing a stub
    /// </summary>
    public class StubHttpResponseMessage : HttpResponseMessage
    {
        private string _File;
        /// <summary>
        /// The file it was generated from
        /// </summary>
        public string File
        {
            get
            {
                return _File;
            }
            set
            {
                _File = value;
            }
        }

        private bool IsModified = false;

        internal void SetFile(FileFullPath fullPath)
        {
            if (Global.InterceptionConfiguration?.RootStubsFolder != null)
                File = fullPath.ToString().Replace(Global.InterceptionConfiguration.RootStubsFolder, "").TrimStart('\\', '/');
            else
                File = fullPath;
        }

        /// <summary>
        /// A HttpResponseMessage representing a stub
        /// </summary>
        /// <param name="statusCode"></param>
        public StubHttpResponseMessage(HttpStatusCode statusCode) : base(statusCode)
        {
        }

        /// <summary>
        /// Parse the response body as a class, change it and store it again in the response
        /// </summary>
        public void ModifyStubJsonBody<T>(Action<T> dtoModifier) where T : class
        {
            this.ModifyJsonBody<T>(dtoModifier);
            IsModified = true;
        }

        /// <summary>
        /// Summarize
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if(IsModified)
                return $"Stub (modified) [{File}] with status [{StatusCode}]";
            else
                return $"Stub [{File}] with status [{StatusCode}]";
        }
    }
}

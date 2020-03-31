using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;

namespace SystemTestingTools
{
    internal class RequestResponse
    {
        public MetadataInfo Metadata = new MetadataInfo();
        public RequestInfo Request = new RequestInfo();
        public ResponseInfo Response = new ResponseInfo();

        internal abstract class RequestResponseInfo
        {
            public Dictionary<string, string> Headers = new Dictionary<string, string>();
            public string Body;
        }

        [DebuggerDisplay("{Method} {Url}")]
        internal class RequestInfo : RequestResponseInfo
        {
            public HttpMethod Method;
            public string Url;
        }


        [DebuggerDisplay("{Status}")]
        internal class ResponseInfo : RequestResponseInfo
        {
            public Version HttpVersion;
            public HttpStatusCode Status;
        }

        internal class MetadataInfo
        {
            public DateTime DateTime;
            public string Timezone;
            public string RecordedFrom;
            public string LocalMachine;
            public string User;
            public string ToolUrl;
            public string ToolNameAndVersion;
        }
    }
}

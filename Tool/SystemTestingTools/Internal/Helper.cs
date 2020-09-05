using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
[assembly: InternalsVisibleTo("SystemTestingTools.UnitTests")]

namespace SystemTestingTools
{
    internal static class Helper
    {
        internal static (Encoding encoding, string mediaType) ParseContentType(string contentType)
        {
            if (string.IsNullOrWhiteSpace(contentType))
                return (Encoding.UTF8, "text/plain");

            var parts = contentType.Split(";");

            var parsedEnconding = Encoding.UTF8;              

            if (parts.Length > 1)
                parsedEnconding = ParseCharset(parts[1]);

            return (parsedEnconding, parts[0]);
        }

        private static Encoding ParseCharset(string charset)
        {
            charset = charset.ToLower().Replace("charset=", "").Trim();
            if (charset == "utf-16") charset = "unicode";

            try
            {
                return Encoding.GetEncoding(charset);
            }
            catch (ArgumentException)
            {
                // worst case scenario when trying to parse some weird charset
                return Encoding.UTF8;
            }            
        }

        internal static string SeparatedByComa(this IEnumerable<string> values)
        {
            return string.Join(",", values);
        }

        internal static void ApppendHeaders<T1, T2>(this Dictionary<T1, T2> currentDic, Dictionary<T1, T2> otherDic)
        {
            foreach (var item in otherDic)
                currentDic.Add(item.Key, item.Value);
        }

        internal enum KnownContentTypes
        {
            Json,
            Xml,
            Other // for now, we only care about the most common
        }

        internal static string GetContentType(this KnownContentTypes contentType)
        {
            switch (contentType)
            {
                case KnownContentTypes.Json:
                    return "application/json";
                case KnownContentTypes.Xml:
                    return "application/xml";
            }
            return "text/plain";
        }

        internal static KnownContentTypes GetKnownContentType(string contentType)
        {
            if (string.IsNullOrEmpty(contentType)) return KnownContentTypes.Other;

            contentType = contentType.ToLower().Trim();

            if (contentType.StartsWith("application/json") || contentType.StartsWith("text/json"))
                return KnownContentTypes.Json;

            if (contentType.StartsWith("application/xml") || contentType.StartsWith("text/xml"))
                return KnownContentTypes.Xml;

            return KnownContentTypes.Other;
        }

        private static Regex HeaderParser = new Regex(@"(.+?):(.+?)$", RegexOptions.Compiled | RegexOptions.Multiline);

        internal static Dictionary<string, string> GetHeaders(string headerContents)
        {
            var result = new Dictionary<string, string>();

            if (string.IsNullOrWhiteSpace(headerContents)) return result;

            var headers = HeaderParser.Matches(headerContents);

            if (headers.Count == 0) throw new ArgumentException($"Header part of content could not be parsed");

            result = headers.ToDictionary(c => c.Groups[1].Value, c => c.Groups[2].Value.Trim());

            return result;
        }

        internal static StringContent ParseHeadersAndBody(string headerContent, string bodyContent, HttpHeaders headers)
        {
            string contentType = null;
            Dictionary<string, string> headerDic = null;
            if (!string.IsNullOrEmpty(headerContent))
            {
                headerDic = GetHeaders(headerContent);
                headerDic.TryGetValue("Content-Type", out contentType);
            }
            var format = ParseContentType(contentType);

            var result = new StringContent(bodyContent, format.encoding, format.mediaType);

            if (headerDic != null)
                foreach (var header in headerDic)
                {
                    if (header.Key.ToLower() == "content-type") continue; // this has already been added when creating new StringContent(

                    if (!headers.TryAddWithoutValidation(header.Key, header.Value))
                        if (!result.Headers.TryAddWithoutValidation(header.Key, header.Value))
                            throw new ApplicationException($"Could not add header '{header.Key}'");
                }

            return result;
        }

    }
}

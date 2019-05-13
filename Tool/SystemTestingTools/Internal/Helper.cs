using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

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
    }
}

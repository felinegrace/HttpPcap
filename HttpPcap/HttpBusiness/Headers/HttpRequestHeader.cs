using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Amber.Kit.HttpPcap.HttpBusiness
{
    class HttpRequestHeader
    {
        public string method { get; private set; }
        public string host { get; private set; }
        public string uri { get; private set; }
        private string parseHost(string asciiRequest)
        {
            Regex regex = new Regex(@"\bhost:.(\S*)", RegexOptions.IgnoreCase);
            Match match = regex.Match(asciiRequest);
            return match.Groups[1].Value;
        }
        public HttpRequestHeader(byte[] rawHttpRequestCollection)
        {
            host = string.Empty;
            method = string.Empty;

            string asciiRequest = System.Text.Encoding.ASCII.GetString(rawHttpRequestCollection.ToArray());

            host = parseHost(asciiRequest);

            int firstReturnIndex = asciiRequest.IndexOf("\r\n");
            if (firstReturnIndex > 0)
            {
                string firstLine = asciiRequest.Substring(0, firstReturnIndex);
                int firstSpace = firstLine.IndexOf(" ");
                if (firstSpace > 0)
                {
                    method = firstLine.Substring(0, firstSpace);
                    int urllen = firstLine.LastIndexOf(" ") - firstSpace - 1;
                    if (urllen > 0)
                        uri = firstLine.Substring(firstSpace + 1, urllen);
                }
            }
        }
    }
}

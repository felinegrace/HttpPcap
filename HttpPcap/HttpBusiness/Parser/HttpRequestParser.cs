using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace Amber.Kit.HttpPcap.HttpBusiness
{
    class HttpRequestParser
    {
        public HttpRequest httpRequest { get; private set; }
        private void parseFirstLine(string header)
        {
            int firstReturnIndex = header.IndexOf("\r\n");
            if (firstReturnIndex > 0)
            {
                string firstLine = header.Substring(0, firstReturnIndex);
                int firstSpace = firstLine.IndexOf(" ");
                if (firstSpace > 0)
                {
                    httpRequest.method = firstLine.Substring(0, firstSpace);
                    int urllen = firstLine.LastIndexOf(" ") - firstSpace - 1;
                    if (urllen > 0)
                    {
                        string rawUri = firstLine.Substring(firstSpace + 1, urllen);

                        //decode as unicode(UTF8)
                        httpRequest.uri = HttpUtility.UrlDecode(rawUri);
                    }
                        
                }
            }
        }
        private void parseHost(string header)
        {
            Regex regex = new Regex(@"\bHost:.(\S*)", RegexOptions.IgnoreCase);
            Match match = regex.Match(header);
            httpRequest.host = match.Groups[1].Value;
        }

        public HttpRequestParser(byte[] rawStream)
        {
            httpRequest = new HttpRequest();
            httpRequest.rawHeader = System.Text.Encoding.ASCII.GetString(rawStream.ToArray());
            parseFirstLine(httpRequest.rawHeader);
            parseHost(httpRequest.rawHeader);
        }
    }
}

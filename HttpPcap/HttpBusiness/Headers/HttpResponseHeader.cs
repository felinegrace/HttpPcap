using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amber.Kit.HttpPcap.HttpBusiness
{
    class HttpResponseHeader
    {
        int statusCode { get; set; }

        public HttpResponseHeader(byte[] rawHttpResponseCollection)
        {
            string asciiResponse = System.Text.Encoding.ASCII.GetString(rawHttpResponseCollection.ToArray());

            int firstLineEndIndex = asciiResponse.IndexOf("\r\n");
            if (firstLineEndIndex > 0)
            {
                string firstLine = asciiResponse.Substring(0, firstLineEndIndex);
                string[] splitFirstLineBySpace = firstLine.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (splitFirstLineBySpace.Length >= 2)
                {
                    statusCode = int.Parse(splitFirstLineBySpace[1]);
                }
            }
        }
    }
}

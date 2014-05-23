using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using Amber.Kit.HttpPcap.Common;
using System.IO;
using System.IO.Compression;

namespace Amber.Kit.HttpPcap.HttpBusiness
{
    class HttpResponseParser
    {
        public HttpResponse httpResponse { get; private set; }
        public bool responseIntegrity { get; private set; }

        private readonly byte[] bsrbsn = { (byte)'\r', (byte)'\n' };
        private readonly byte[] bsrbsnbsrbsn = { (byte)'\r', (byte)'\n', (byte)'\r', (byte)'\n' };

        private void storeResponseHeaderAndChunkedData(byte[] rawStream)
        {

            int headerEnds = BytesHelper.indexOf(rawStream, 0, rawStream.Length, bsrbsnbsrbsn).First();
            httpResponse.rawHeaderStream.AddRange(rawStream.Take(headerEnds));
            int chunkedIndex = headerEnds + bsrbsnbsrbsn.Length;
            int chunkedLength = rawStream.Length - chunkedIndex;
            if (chunkedLength > 0)
            {
                IEnumerable<byte> rawChunkedStream = rawStream.Skip(chunkedIndex).Take(chunkedLength);
                httpResponse.rawChunkedStream.AddRange(rawChunkedStream);
                parseChunkedStream(rawChunkedStream);
            }
        }

        private void parseChunkedStream(IEnumerable<byte> rawChunkedStream)
        {
            int currentPosition = 0;
            int rawChunkedStreamLength = rawChunkedStream.Count();
            int dataNotDigestedCount = rawChunkedStreamLength - currentPosition;
            while (dataNotDigestedCount > 0)
            {
                int chunkLengthEnds = BytesHelper.indexOf(rawChunkedStream, currentPosition, dataNotDigestedCount, bsrbsn).FirstOrDefault();
                if(chunkLengthEnds == 0)
                {
                	//this is not valid chunk data
                    throw new PcapException("this is not valid chunk data");
                }
                byte[] chunkLengthBytes = rawChunkedStream.Skip(currentPosition).Take(chunkLengthEnds - currentPosition).ToArray();
                string chunkLengthString = System.Text.Encoding.ASCII.GetString(chunkLengthBytes);
                int chunkLength = Convert.ToInt32(chunkLengthString, 16);
                //find length = 0 terminates chunk
                if (chunkLength == 0)
                {
                    decodeChunkedStream();
                    return;
                }
                //or append more chunk
                else
                {
                    currentPosition = chunkLengthEnds + bsrbsn.Length;
                    httpResponse.rawChunkedEntityBody.AddRange(rawChunkedStream.Skip(currentPosition).Take(chunkLength));
                    currentPosition += chunkLength + bsrbsn.Length;
                }
                dataNotDigestedCount = rawChunkedStreamLength - currentPosition;
            }


        }
        private void decodeChunkedStream()
        {
            Stream decodedChunkedEntityBody = null;
            Stream rawChunkedEntityBody = new System.IO.MemoryStream(httpResponse.rawChunkedEntityBody.ToArray());
            if (httpResponse.contentEncoding.Equals("gzip"))
            {
                decodedChunkedEntityBody = new GZipStream(rawChunkedEntityBody, CompressionMode.Decompress);
            }
            else if (httpResponse.contentEncoding.Equals("deflate"))
            {
                decodedChunkedEntityBody = new DeflateStream(rawChunkedEntityBody, CompressionMode.Decompress);
            }
            Encoding encoding = Encoding.GetEncoding(httpResponse.charset.Equals("") ? "utf-8" : httpResponse.charset);
            if (decodedChunkedEntityBody != null)
            {
                httpResponse.chunkedEntityBody = new StreamReader(decodedChunkedEntityBody, encoding).ReadToEnd();
            }
        }
        
        private void parseFirstLine(string rawStream)
        {
            int firstLineEndIndex = rawStream.IndexOf("\r\n");
            if (firstLineEndIndex > 0)
            {
                string firstLine = rawStream.Substring(0, firstLineEndIndex);
                string[] splitFirstLineBySpace = firstLine.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (splitFirstLineBySpace.Length >= 2)
                {
                    httpResponse.statusCode = int.Parse(splitFirstLineBySpace[1]);
                }
            }
        }
        private void parseTransferEncoding(string rawStream)
        {
            Regex regex = new Regex(@"\bTransfer-Encoding:.(\S*)", RegexOptions.IgnoreCase);
            Match match = regex.Match(rawStream);
            httpResponse.transferEncoding = match.Groups[1].Value;
        }

        private void parseContentEncoding(string rawStream)
        {
            Regex regex = new Regex(@"\bContent-Encoding:.(\S*)", RegexOptions.IgnoreCase);
            Match match = regex.Match(rawStream);
            httpResponse.contentEncoding = match.Groups[1].Value;
        }

        private void parseCharset(string rawStream)
        {
            Regex regex = new Regex("charset=([\\w|-]+)", RegexOptions.IgnoreCase);
            Match match = regex.Match(rawStream);
            httpResponse.charset = match.Groups[1].Value;
        }

        public HttpResponseParser(byte[] rawStream)
        {
            httpResponse = new HttpResponse();
            responseIntegrity = false;

            storeResponseHeaderAndChunkedData(rawStream);

            string unicodeRequest = System.Text.Encoding.ASCII.GetString(httpResponse.rawHeaderStream.ToArray());
            parseFirstLine(unicodeRequest);
            parseTransferEncoding(unicodeRequest);
            parseContentEncoding(unicodeRequest);
            parseCharset(unicodeRequest);
            if( ! httpResponse.transferEncoding.Equals("chunked"))
            {
                responseIntegrity = true;
            }
        }

        public void moreChunkedStream(byte[] rawChunkedStream)
        {
            httpResponse.rawChunkedStream.AddRange(rawChunkedStream);
            if (BytesHelper.isEndWith(rawChunkedStream, bsrbsnbsrbsn))
            {
                parseChunkedStream(httpResponse.rawChunkedStream);
                responseIntegrity = true;
            }
        }
    }
}

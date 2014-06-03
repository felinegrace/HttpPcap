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
        private readonly string bsrbsnString = "\r\n";
        private readonly byte[] bsrbsnbsrbsn = { (byte)'\r', (byte)'\n', (byte)'\r', (byte)'\n' };
        private readonly byte[] zerobsrbsnbsrbsn = { (byte)'0', (byte)'\r', (byte)'\n', (byte)'\r', (byte)'\n' };
        private int exceptedEntityLength { get; set; }
        private int receivedEntityLength { get; set; }
        private void storeResponseHeaderAndEntity(byte[] rawStream)
        {
            bool isHeaderEnded = false;
            int currentPosition = 0;
            IEnumerable<int> lineEnds = BytesHelper.indexOf(rawStream, currentPosition, rawStream.Length, bsrbsn);

            foreach (int nextLineEnds in lineEnds)
            {
                if(isHeaderEnded)
                {
                    break;
                }
                if (nextLineEnds == currentPosition)
                {
                    isHeaderEnded = true;
                }
                else
                {
                    int nextLineByteCount = nextLineEnds - currentPosition;
                    string nextLine = System.Text.Encoding.ASCII.GetString(rawStream.Skip(currentPosition).Take(nextLineByteCount).ToArray());
                    if (nextLine.Trim().Equals(string.Empty))
                    {
                        isHeaderEnded = true;
                    }
                    else
                    {
                        httpResponse.rawHeader += nextLine + bsrbsnString;
                    }
                    currentPosition += nextLineByteCount;
                }
                currentPosition += bsrbsn.Length;
            }
            if(isHeaderEnded == false)
            {
                throw new PcapException("corrupted header.");
            }
            
            this.receivedEntityLength = rawStream.Length - currentPosition;
            if (this.receivedEntityLength > 0)
            {
                IEnumerable<byte> rawEntityStream = rawStream.Skip(currentPosition).Take(this.receivedEntityLength);
                httpResponse.rawEntity.AddRange(rawEntityStream);
            }
        }

        private void parseChunkedEntity(IEnumerable<byte> rawChunkedEntity)
        {
            int currentPosition = 0;
            int rawChunkedStreamLength = rawChunkedEntity.Count();
            int dataNotDigestedCount = rawChunkedStreamLength - currentPosition;
            while (dataNotDigestedCount > 0)
            {
                int chunkLengthEnds = BytesHelper.indexOf(rawChunkedEntity, currentPosition, dataNotDigestedCount, bsrbsn).FirstOrDefault();
                if(chunkLengthEnds == 0)
                {
                	//this is not valid chunk data
                    throw new PcapException("this is not valid chunk data");
                }
                byte[] chunkLengthBytes = rawChunkedEntity.Skip(currentPosition).Take(chunkLengthEnds - currentPosition).ToArray();
                string chunkLengthString = System.Text.Encoding.ASCII.GetString(chunkLengthBytes);
                int chunkLength = Convert.ToInt32(chunkLengthString, 16);
                //find length = 0 terminates chunk
                if (chunkLength == 0)
                {
                    if(httpResponse.isTextEntityBody)
                    {
                        decodeEntityAsText(httpResponse.rawChunkedEntity);
                    }
                    return;
                }
                //or append more chunk
                else
                {
                    currentPosition = chunkLengthEnds + bsrbsn.Length;
                    httpResponse.rawChunkedEntity.AddRange(rawChunkedEntity.Skip(currentPosition).Take(chunkLength));
                    currentPosition += chunkLength + bsrbsn.Length;
                }
                dataNotDigestedCount = rawChunkedStreamLength - currentPosition;
            }


        }
        private void decodeEntityAsText(List<byte> rawEntity)
        {
            Stream decodedChunkedEntity = null;
            Stream rawChunkedEntity = new System.IO.MemoryStream(rawEntity.ToArray());
            if (httpResponse.contentEncoding.Equals("gzip"))
            {
                decodedChunkedEntity = new GZipStream(rawChunkedEntity, CompressionMode.Decompress);
            }
            else if (httpResponse.contentEncoding.Equals("deflate"))
            {
                decodedChunkedEntity = new DeflateStream(rawChunkedEntity, CompressionMode.Decompress);
            }
            else
            {
                decodedChunkedEntity = rawChunkedEntity;
            }
            Encoding encoding = Encoding.GetEncoding(httpResponse.charset.Equals("") ? "utf-8" : httpResponse.charset);
            if (decodedChunkedEntity != null)
            {
                httpResponse.textEntity = new StreamReader(decodedChunkedEntity, encoding).ReadToEnd();
            }
        }

        private void parseFirstLine(string header)
        {
            int firstLineEndIndex = header.IndexOf("\r\n");
            if (firstLineEndIndex > 0)
            {
                string firstLine = header.Substring(0, firstLineEndIndex);
                string[] splitFirstLineBySpace = firstLine.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (splitFirstLineBySpace.Length >= 2)
                {
                    httpResponse.statusCode = int.Parse(splitFirstLineBySpace[1]);
                }
            }
        }
        private void parseTransferEncoding(string header)
        {
            Regex regex = new Regex(@"\bTransfer-Encoding:.(\S*)", RegexOptions.IgnoreCase);
            Match match = regex.Match(header);
            httpResponse.transferEncoding = match.Groups[1].Value;
        }

        private void parseContentEncoding(string header)
        {
            Regex regex = new Regex(@"\bContent-Encoding:.(\S*)", RegexOptions.IgnoreCase);
            Match match = regex.Match(header);
            httpResponse.contentEncoding = match.Groups[1].Value;
        }

        private void parseContentType(string header)
        {
            Regex contentTypeRegex = new Regex(@"\bContent-Type:.(\S*)", RegexOptions.IgnoreCase);
            Match contentTypeMatch = contentTypeRegex.Match(header);
            string contentType = contentTypeMatch.Groups[1].Value;

            httpResponse.isTextEntityBody = contentType.Contains("text") ||
                contentType.Contains("javascript") ||
                contentType.Contains("json");

            Regex charsetRegex = new Regex(@"charset=(\S*)", RegexOptions.IgnoreCase);
            Match charserMatch = charsetRegex.Match(header);
            httpResponse.charset = charserMatch.Groups[1].Value;
        }

        private void parseContentLength(string header)
        {
            Regex regex = new Regex(@"\bContent-Length:.(\S*)", RegexOptions.IgnoreCase);
            Match match = regex.Match(header);
            if(match.Groups.Count > 1)
            {
                string contentLengthString = match.Groups[1].Value;
                this.exceptedEntityLength = Convert.ToInt32(contentLengthString);
            }
        }

        public HttpResponseParser(byte[] rawStream)
        {
            httpResponse = new HttpResponse();
            responseIntegrity = false;

            storeResponseHeaderAndEntity(rawStream);

            parseFirstLine(httpResponse.rawHeader);
            parseTransferEncoding(httpResponse.rawHeader);
            parseContentEncoding(httpResponse.rawHeader);
            parseContentType(httpResponse.rawHeader);
            parseContentLength(httpResponse.rawHeader);
            checkResponseIntegrity();
        }

        public void moreStream(byte[] rawStream)
        {
            httpResponse.rawEntity.AddRange(rawStream);
            receivedEntityLength += rawStream.Length;
            checkResponseIntegrity();
        }

        private void checkResponseIntegrity()
        {
            //check chunked response ending
            if (httpResponse.transferEncoding.Equals("chunked"))
            {
                if (BytesHelper.isEndWith(httpResponse.rawEntity, zerobsrbsnbsrbsn))
                {
                    parseChunkedEntity(httpResponse.rawEntity);
                    responseIntegrity = true;
                }
            }
            //check response not chunked, if content is up to length
            else if (exceptedEntityLength > 0)
            {
                if (receivedEntityLength >= exceptedEntityLength)
                {
                    if (httpResponse.isTextEntityBody)
                    {
                        decodeEntityAsText(httpResponse.rawEntity);
                    }
                    responseIntegrity = true;
                }
            }
            //wtf
            //its nothing
            else if (exceptedEntityLength == 0)
            {
                responseIntegrity = true;
            }
        }
    }
}

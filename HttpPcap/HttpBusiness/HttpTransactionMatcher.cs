using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amber.Kit.HttpPcap.Common;
namespace Amber.Kit.HttpPcap.HttpBusiness
{
    class HttpTransactionMatcher
    {

        /// <summary>
        /// key by request sequence number.
        /// response ack = request sequence + length.
        /// </summary>
        private Dictionary<uint, HttpTransactionPair> transactionDict { get; set; }

        public HttpTransactionMatcher()
        {
            transactionDict = new Dictionary<uint, HttpTransactionPair>();
        }

        public void resetDict()
        {
            transactionDict.Clear();
        }


        public void newRequest(string rawRequestSeqNum, int rawRequestLength, HttpRequestParser httpRequestParser)
        {
            uint key = Convert.ToUInt32(rawRequestSeqNum);
            key += (uint)rawRequestLength;

            if(transactionDict.ContainsKey(key))
            {
                transactionDict.Remove(key);
            }

            HttpTransactionPair httpTransactionPair = new HttpTransactionPair();

            httpTransactionPair.httpRequestParser = httpRequestParser;
            httpTransactionPair.httpResponseParser = null;
            transactionDict.Add(key, httpTransactionPair);
        }

        public void newResponse(string rawResponseAckNum, byte[] rawResponseStream, 
            out bool responseIntegrity,
            out HttpTransactionPair httpTransactionPair
            )
        {
            responseIntegrity = false;
            httpTransactionPair = null;

            uint key = Convert.ToUInt32(rawResponseAckNum);

            bool hasMatchedRequest = transactionDict.TryGetValue(key, out httpTransactionPair);
            //throw not matched response
            if ( hasMatchedRequest == false )
            {
                return;
            }
            try
            {
                if (httpTransactionPair.httpResponseParser == null)
                {
                    httpTransactionPair.httpResponseParser = new HttpResponseParser(rawResponseStream);
                }
                else
                {
                    httpTransactionPair.httpResponseParser.moreStream(rawResponseStream);
                }
                //if all data is gethered, remove this transaction
                responseIntegrity = httpTransactionPair.httpResponseParser.responseIntegrity;
                if (responseIntegrity == true)
                {
                    transactionDict.Remove(key);
                }
            }
            catch (System.Exception)
            {
                //throw currupted transaction
                transactionDict.Remove(key);
            }
        }
    }
}

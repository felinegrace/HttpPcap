using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amber.Kit.HttpPcap.HttpBusiness
{
    class HttpTransactionMatcher
    {

        /// <summary>
        /// key by request sequence number.
        /// response ack = request sequence + length.
        /// </summary>
        private Dictionary<uint, HttpTransaction> transactionDict { get; set; }

        public HttpTransactionMatcher()
        {
            transactionDict = new Dictionary<uint, HttpTransaction>();
        }

        public void resetDict()
        {
            transactionDict.Clear();
        }


        public void newRequest(string rawRequestSeqNum, byte[] rawRequestCollection)
        {
            uint key = Convert.ToUInt32(rawRequestSeqNum);
            key += (uint)rawRequestCollection.Length;

            if(transactionDict.ContainsKey(key))
            {
                transactionDict.Remove(key);
            }

            HttpTransaction transaction = new HttpTransaction();
            transaction.rawRequest.AddRange(rawRequestCollection);

            transactionDict.Add(key, transaction);
        }

        public void newResponse(string rawResponseAckNum, byte[] rawResponseCollection, 
            out bool responseIntegrity,
            out byte[] requestData,
            out byte[] responseData
            )
        {
            responseIntegrity = false;
            requestData = null;
            responseData = null;

            uint key = Convert.ToUInt32(rawResponseAckNum);

            HttpTransaction transaction = null;
            bool hasMatchedRequest = transactionDict.TryGetValue(key, out transaction);
            //throw not matched response
            if ( hasMatchedRequest == false )
            {
                return;
            }

            transaction.rawResponse.AddRange(rawResponseCollection);

            //to see if response ends with 0d 0a 0d 0a (\r\n\r\n)
            if (transaction.rawResponse.ElementAt<byte>(transaction.rawResponse.Count - 4) == 0x0d &&
                transaction.rawResponse.ElementAt<byte>(transaction.rawResponse.Count - 3) == 0x0a &&
                transaction.rawResponse.ElementAt<byte>(transaction.rawResponse.Count - 2) == 0x0d &&
                transaction.rawResponse.ElementAt<byte>(transaction.rawResponse.Count - 1) == 0x0a)
            {
                responseIntegrity = true;
                requestData = transaction.rawRequest.ToArray();
                responseData = transaction.rawResponse.ToArray();
                transactionDict.Remove(key);
            }

                
        }
    }
}

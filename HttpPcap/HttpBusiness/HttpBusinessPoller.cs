using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amber.Kit.HttpPcap.Common;

namespace Amber.Kit.HttpPcap.HttpBusiness
{
    

    class HttpBusinessPoller : SingleListServer<Descriptor>
    {
        
        private HttpTransactionMatcher httpTransactionMatcher { get; set; }
        public Action<HttpRequest> onRequest { get; set; }
        public Action<HttpResponse> onResponse { get; set; }
        public Action<HttpTransaction> onTransaction { get; set; }
        private HttpPcapConfig httpPcapConfig { get; set; }

        public HttpBusinessPoller(HttpPcapConfig httpPcapConfig)
        {
            this.httpPcapConfig = httpPcapConfig;
            httpTransactionMatcher = new HttpTransactionMatcher();
        }

        public void ignoreUnresponsedRequest()
        {
            httpTransactionMatcher.resetDict();
        }

        protected override void handleRequest(Descriptor request)
        {
            HttpPacketParser parser = new HttpPacketParser(request);
            byte[] byteData = request.des;
            int nReceived = request.desLength;
            if( !parser.isTcp() )
                return;
            //filter all non-data packet
            if (!parser.isTcpACKwithData())
                return;
            if (parser.headerLength >= nReceived) 
                return;
            //current cannot deal with SourcePort == DestinationPort
            //wait for further update
            if (parser.tcpHeader.SourcePort == parser.tcpHeader.DestinationPort)
                return;
            if (httpPcapConfig.isRequest(parser.tcpHeader.SourcePort, parser.tcpHeader.DestinationPort))
            {
                onHttpRequest(parser);
            }
            
            else if (httpPcapConfig.isResponse(parser.tcpHeader.SourcePort, parser.tcpHeader.DestinationPort))
            {
                onHttpResponse(parser);
            }
        }

        private void onHttpRequest(HttpPacketParser parser)
        {
            HttpRequestParser httpRequestParser = new HttpRequestParser(parser.tcpHeader.Data);
            if (httpPcapConfig.remoteDomainFilter != "" &&
                httpRequestParser.httpRequest.host.IndexOf(httpPcapConfig.remoteDomainFilter) == -1)
                return;

            httpTransactionMatcher.newRequest(parser.tcpHeader.SequenceNumber, parser.tcpHeader.Data.Length, httpRequestParser);
            if (onRequest != null)
            {
                onRequest(httpRequestParser.httpRequest);
            }
        }

        private void onHttpResponse(HttpPacketParser parser)
        {
            bool responseIntegrity;
            HttpTransactionPair httpTransactionPair;
            httpTransactionMatcher.newResponse(
                parser.tcpHeader.AcknowledgementNumber, parser.tcpHeader.Data,
                out responseIntegrity, out httpTransactionPair);
            if (responseIntegrity)
            {
                if (onResponse != null)
                {
                    onResponse(httpTransactionPair.httpResponseParser.httpResponse);
                }
                if (onTransaction != null)
                {
                    HttpTransaction httpTransaction = new HttpTransaction();
                    httpTransaction.httpRequest = httpTransactionPair.httpRequestParser.httpRequest;
                    httpTransaction.httpResponse = httpTransactionPair.httpResponseParser.httpResponse;
                    onTransaction(httpTransaction);
                }
            }
        }

        protected override void onStart()
        {
            ignoreUnresponsedRequest();
        }
    }
}

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
        public Action<byte[]> onRequest { get; set; }
        public Action<byte[]> onResponse { get; set; }
        public Action<byte[], byte[]> onTransaction { get; set; }
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
            HttpRequestHeader httpRequestHeader = parser.parsePayloadAsHttpRequest();
            if (httpPcapConfig.remoteDomainFilter != "" &&
                httpRequestHeader.host.IndexOf(httpPcapConfig.remoteDomainFilter) == -1)
                return;
            httpTransactionMatcher.newRequest(parser.tcpHeader.SequenceNumber, parser.tcpHeader.Data);
            if (onRequest != null)
            {
                onRequest(parser.tcpHeader.Data);
            }
        }

        private void onHttpResponse(HttpPacketParser parser)
        {
            bool responseIntegrity;
            byte[] requestData;
            byte[] responseData;
            httpTransactionMatcher.newResponse(
                parser.tcpHeader.AcknowledgementNumber, parser.tcpHeader.Data, 
                out responseIntegrity, out requestData, out responseData);
            if (responseIntegrity)
            {
                
                if (onResponse != null)
                {
                    onResponse(responseData);
                }
                if (onTransaction != null)
                {
                    onTransaction(requestData, responseData);
                }
            }

        }

        protected override void onStart()
        {
            ignoreUnresponsedRequest();
        }
    }
}

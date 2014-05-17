using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amber.Kit.HttpPcap.CommonObject;
using Amber.Kit.HttpPcap.WinPcap;
using Amber.Kit.HttpPcap.HttpBusiness;
using Cabinet.Utility;
using Amber.Kit.HttpPcap.RawSocket;
using Amber.Kit.HttpPcap.Common;

namespace Amber.Kit.HttpPcap
{
    public class HttpPcap
    {
        private HttpPcapConfig httpPcapConfig { get; set; }
        private HttpBusinessPoller httpBusinessPoller { get; set; }
        private PacketPollerBase packetPoller { get; set; }
        private RawSocketPacketPoller rawSocketPoller { get; set; }
        public HttpPcap(HttpPcapConfig httpPcapConfig)
        {
            httpPcapConfig.validate();
            this.httpPcapConfig = httpPcapConfig;
            httpBusinessPoller = new HttpBusinessPoller(httpPcapConfig);
            httpBusinessPoller.onRequest = this.onReq;
            httpBusinessPoller.onResponse = this.onResp;
            httpBusinessPoller.onTransaction = this.onTransaction;
   
            switch(httpPcapConfig.pcapMode)
            {
                case "winpcap":
                {
                    packetPoller = new PcapPacketPoller(httpPcapConfig.pcapIpAddress, this.onPacket);
                    break;
                }
                case "rawsocket":
                {
                    packetPoller = new RawSocketPacketPoller(httpPcapConfig.pcapIpAddress, this.onPacket);
                    break;
                }
            }
        }

        public void start()
        {
            httpBusinessPoller.start();
            packetPoller.start();
        }

        public void stop()
        {
            httpBusinessPoller.stop();
            packetPoller.stop();
        }


        private void onPacket(Descriptor descriptor)
        {
            if (descriptor.des[9] != 0x06) return;
            int srcport = BytesHelper.bytes2ushort(descriptor.des, 20, true);
            int desport = BytesHelper.bytes2ushort(descriptor.des, 22, true);
            if (srcport == desport) return;
            if (httpPcapConfig.isRequest(srcport, desport) ||
                httpPcapConfig.isResponse(srcport, desport))
            {
                byte[] ipPacket = new byte[descriptor.desLength];
                Array.Copy(descriptor.des, 0, ipPacket, 0, ipPacket.Length);
                httpBusinessPoller.postRequest(new DescriptorReference(ipPacket, ipPacket.Length));
            }
        }

        private void onReq(byte[] req)
        {
            Logger.debug("\n^^^req = {0}", System.Text.Encoding.UTF8.GetString(req));

        }

        private void onResp(byte[] resp)
        {
            Logger.debug("\n&&&rsp = {0}", System.Text.Encoding.UTF8.GetString(resp));


        }

        private void onTransaction(byte[] req, byte[] resp)
        {
            Logger.debug("\n>>>req = {0}", System.Text.Encoding.UTF8.GetString(req));

            Logger.debug("\n>>>rsp = {0}", System.Text.Encoding.UTF8.GetString(resp));

        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amber.Kit.HttpPcap;
using Cabinet.Utility;
using Amber.Kit.HttpPcap.Common;
namespace Demo_HttpPcapConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Logger.enable();

            HttpPcapConfig config = new HttpPcapConfig();
            config.serverPortsFilter.Add(80);
            config.pcapIpAddress = "10.31.31.31";
            config.remoteDomainFilter = "www.baidu.com";
            config.pcapMode = "winpcap";
            HttpPcap pcap = new HttpPcap(config);
            pcap.onHttpPcapRequestEvent += onReq;
            pcap.onHttpPcapResponseEvent += onResp;
            pcap.onHttpPcapTransactionEvent += onTrans;
            pcap.onHttpPcapErrorEvent += onErr;
            pcap.start();
            ConsoleKeyInfo ch;
            do
            {
                ch = Console.ReadKey();
            } while (ch.Key != ConsoleKey.Q);
            pcap.stop();
        }

        private static void onReq(object sender, HttpPacketEventArgs args)
        {
            Logger.debug("\n^^^req = {0}", System.Text.Encoding.UTF8.GetString(args.content));

        }

        private static void onResp(object sender, HttpPacketEventArgs args)
        {
            Logger.debug("\n&&&rsp = {0}", System.Text.Encoding.UTF8.GetString(args.content));


        }

        private static void onTrans(object sender, HttpTransactionEventArgs args)
        {
            Logger.debug("\n>>>req = {0}", System.Text.Encoding.UTF8.GetString(args.requestContent));

            Logger.debug("\n>>>rsp = {0}", System.Text.Encoding.UTF8.GetString(args.responseContent));

        }

        private static void onErr(object sender, HttpPcapErrorEventArgs args)
        {
            Logger.error("\n!!!err = {0}", args.message);
        }
    }
}

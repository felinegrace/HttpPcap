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
            config.serverPortsFilter.Add(8090);
            config.pcapIpAddress = "10.148.219.209";
            config.remoteDomainFilter = "";
            config.pcapMode = "rawsocket";
            HttpPcapEntry pcap;
            try
            {
                pcap = new HttpPcapEntry(config);

            }
            catch(Exception ex)
            {
                Logger.error("\n~~~config err = {0}", ex.Message);
                Console.WriteLine("press any key to quit.");
                Console.ReadKey();
                return;
            }
            pcap.onHttpPcapRequestEvent += onReq;
            pcap.onHttpPcapResponseEvent += onResp;
            pcap.onHttpPcapTransactionEvent += onTrans;
            pcap.onHttpPcapErrorEvent += onErr;
            ConsoleKey ch;
            ch = ConsoleKey.S;
            do
            {
                switch (ch)
                {
                    case ConsoleKey.S:
                        pcap.start();
                        break;
                    case ConsoleKey.T:
                        pcap.stop();
                        break;
                }
                ch = Console.ReadKey().Key;
            } while (ch != ConsoleKey.Q);
            pcap.stop();
        }

        private static void onReq(object sender, HttpPacketEventArgs args)
        {
            Logger.debug("\n^^^req = {0}", System.Text.Encoding.UTF8.GetString(args.content));
        }

        //回应包因为有加密的情况,演示程序只打印其中一部分.
        private static void onResp(object sender, HttpPacketEventArgs args)
        {
            int partLength = args.content.Length > 128 ? 128 : args.content.Length - 1;
            Logger.debug("\n^^^part of rsp = {0}", System.Text.Encoding.UTF8.GetString(args.content, 0, partLength));
        }

        private static void onTrans(object sender, HttpTransactionEventArgs args)
        {
            Logger.debug("\n>>>req = {0}", System.Text.Encoding.UTF8.GetString(args.requestContent));
            int partLength = args.responseContent.Length > 128 ? 128 : args.responseContent.Length - 1;
            Logger.debug("\n>>>part of rsp = {0}", System.Text.Encoding.UTF8.GetString(args.responseContent, 0, partLength));
        }

        private static void onErr(object sender, HttpPcapErrorEventArgs args)
        {
            Logger.error("\n!!!err = {0}", args.message);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amber.Kit.HttpPcap;
using Cabinet.Utility;
using Amber.Kit.HttpPcap.Common;
using System.Web;
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
            pcap.onHttpRequestEvent += onReq;
            pcap.onHttpResponseEvent += onResp;
            pcap.onHttpTransactionEvent += onTrans;
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

        private static void onReq(object sender, HttpRequest args)
        {
            Logger.debug("\n^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^\n" +
                "uri = \n{0}\nraw = \n{1}\n" +
                "^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^\n", 
                args.uri, System.Text.Encoding.UTF8.GetString(args.rawStream.ToArray()));
        }

        //回应包因为有加密的情况,演示程序只打印其中一部分.
        private static void onResp(object sender, HttpResponse args)
        {
            //Logger.debug("\n^^^part of rsp = {0}", System.Text.Encoding.UTF8.GetString(args.rawStream.ToArray()));
        }

        private static void onTrans(object sender, HttpTransaction args)
        {
            //Logger.debug("\n>>>req = {0}", System.Text.Encoding.UTF8.GetString(args.httpResponse.rawStream.ToArray()));
            //int partLength = args.httpResponse.rawStream.Count > 128 ? 128 : args.httpResponse.rawStream.Count;
            //Logger.debug("\n>>>part of rsp = {0}", System.Text.Encoding.UTF8.GetString(args.httpResponse.rawStream.ToArray(), 0, partLength));
        }

        private static void onErr(object sender, HttpPcapError args)
        {
            Logger.error("\n!!!err = {0}", args.message);
        }
    }
}

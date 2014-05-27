using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Amber.Kit.HttpPcap;

//Logger就是简单打印

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
            catch (Exception ex)
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
            Logger.debug("^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^");
            Logger.debug("request uri = {0}", args.uri);
            Logger.debug("^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^");
        }

        private static void onResp(object sender, HttpResponse args)
        {
            Logger.debug("______________________________________________________________");
            Logger.debug("response code = {0}", args.statusCode);
            if (args.isTextEntityBody)
                Logger.debug("response text entity = {0}", args.textEntity);
            Logger.debug("______________________________________________________________");

        }

        private static void onTrans(object sender, HttpTransaction args)
        {
            Logger.debug("&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&");
            Logger.debug("request uri = {0}", args.httpRequest.uri);
            Logger.debug("response code = {0}", args.httpResponse.statusCode);
            if (args.httpResponse.isTextEntityBody)
                Logger.debug("response text entity = {0}", args.httpResponse.textEntity);
            Logger.debug("&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&");
        }

        private static void onErr(object sender, HttpPcapError args)
        {
            Logger.error("\n!!!err = {0}", args.message);
        }
    }
}

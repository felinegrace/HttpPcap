using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amber.Kit.HttpPcap;
using Cabinet.Utility;
using Amber.Kit.HttpPcap.CommonObject;
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
            pcap.start();
            while(true)
            {

            }
        }


    }
}

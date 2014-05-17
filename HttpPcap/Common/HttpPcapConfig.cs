using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amber.Kit.HttpPcap.CommonObject
{
    public class HttpPcapConfig
    {
        public string pcapMode { get; set; }
        public string pcapIpAddress { get; set; }
        public string remoteDomainFilter { get; set; }
        public List<int> serverPortsFilter { get; set; }
        public List<int> clientPortsFilter { get; set; }

        public HttpPcapConfig()
        {
            pcapMode = string.Empty;
            pcapIpAddress = string.Empty;
            remoteDomainFilter = string.Empty;
            serverPortsFilter = new List<int>();
            clientPortsFilter = new List<int>();
        }


        public bool isRequest(int sourcePort, int destPort)
        {
            bool isClient = false;
            bool isServer = false;
            if (clientPortsFilter.Count == 0)
                isClient = true;
            if (clientPortsFilter.Contains<int>(sourcePort))
                isClient = true;
            if (serverPortsFilter.Count == 0)
                isServer = true;
            if (serverPortsFilter.Contains<int>(destPort))
                isServer = true;
            return isClient && isServer;
        }

        public bool isResponse(int sourcePort, int destPort)
        {
            bool isClient = false;
            bool isServer = false;
            if (clientPortsFilter.Count == 0)
                isClient = true;
            if (clientPortsFilter.Contains<int>(destPort))
                isClient = true;
            if (serverPortsFilter.Count == 0)
                isServer = true;
            if (serverPortsFilter.Contains<int>(sourcePort))
                isServer = true;
            return isClient && isServer;
        }

        public void validate()
        {
            
            if (pcapMode == string.Empty)
            {
                throw new PcapException("must specify a pcapMode.");
            }
            if ( !( pcapMode.Equals("winpcap") || pcapMode.Equals("rawsocket")) )
            {
                throw new PcapException("pcapMode must be winpcap or rawsocket.");
            }
            if (pcapIpAddress == string.Empty)
            {
                throw new PcapException("must specify an IpAddress.");
            }
            if (serverPortsFilter.Count == 0)
            {
                throw new PcapException("must specify one or more server ports.");
            }
            foreach(int clientPort in clientPortsFilter)
            {
                if(serverPortsFilter.Contains(clientPort))
                {
                    throw new PcapException("client ports must not be the same as server ports.");
                }
            }
        }
    }
}

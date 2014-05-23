using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amber.Kit.HttpPcap.Common;

namespace Amber.Kit.HttpPcap.HttpBusiness
{
    class HttpPacketParser
    {
        public IPHeader ipHeader { get; private set; }
        public TCPHeader tcpHeader { get; private set; }
        public int headerLength { get; private set; }

        public HttpPacketParser(Descriptor descriptor)
        {
            ipHeader = new IPHeader(descriptor.des, descriptor.desLength);
            if(ipHeader.ProtocolType == IPHeader.Protocol.Tcp)
            {
                headerLength += ipHeader.HeaderLength;
                tcpHeader = new TCPHeader(ipHeader.Data, ipHeader.MessageLength);
                headerLength += tcpHeader.HeaderLength;
            }
        }

        public bool isTcp()
        {
            return tcpHeader != null;
        }

        public bool isTcpACKwithData()
        {
            if (!isTcp())
                return false;
            else
            {
                return tcpHeader.Flags == 0x18;
            }
        }

        public bool isTcpSYNwithData()
        {
            if (!isTcp())
                return false;
            else
            {
                return tcpHeader.Flags == 0x10;
            }
        }

    }
}

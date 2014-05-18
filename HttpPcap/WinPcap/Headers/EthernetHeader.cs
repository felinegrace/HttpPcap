using Amber.Kit.HttpPcap.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Amber.Kit.HttpPcap.WinPcap
{
    class EthernetHeader
    {
        const int headerLength = 14;
        public ushort ethernetProtocol { get; set; }
        public byte[] ethernetData { get; set; }
        public EthernetHeader(byte[] packetData)
        {
            readProtocal(packetData);   
            if(isIP())
            {
                ethernetData = new byte[packetData.Length - headerLength];
                Array.Copy(packetData,
                           headerLength,  //start copying from the end of the header
                           ethernetData, 0,
                           ethernetData.Length);
            }
        }

        public EthernetHeader(IntPtr packetDataPtr, uint packetDataLength)
        {
            byte[] protocolBuffer = new byte[headerLength];
            Marshal.Copy(packetDataPtr, protocolBuffer, 0, headerLength);
            readProtocal(protocolBuffer);
            if (isIP())
            {
                ethernetData = new byte[packetDataLength - headerLength];
                IntPtr ethernetDataOffset = new IntPtr(packetDataPtr.ToInt64() + headerLength);
                Marshal.Copy(ethernetDataOffset, ethernetData, 0, ethernetData.Length);
            }
        }

        private void readProtocal(byte[] headerBuffer)
        {
            ethernetProtocol = BytesHelper.bytes2ushort(headerBuffer, 12 , true);
        }

        public bool isIP()
        {
            return ethernetProtocol == 0x0800;
        }
    }
}

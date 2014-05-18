using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Amber.Kit.HttpPcap.Common;



namespace Amber.Kit.HttpPcap.RawSocket
{
    class RawSocketPacketPoller : PacketPollerBase
    {
        private Socket socket { get; set; }

        private const int SIO_RCVALL = unchecked((int)0x98000001);
        private byte[] IN = new byte[4] { 1, 0, 0, 0 };
        private byte[] OUT = new byte[4];
        public RawSocketPacketPoller(string ipAddress, Action<Descriptor> onPacket)
            : base(ipAddress, onPacket)
        {
            socket = null;
            descriptor = DescriptorBuffer.create(maxiumBytesStoredOfEachPacket);
        }
        protected override void onPolling()
        {
            if (socket.Poll(0, SelectMode.SelectRead))
            {
                int length = socket.Receive(descriptor.des, 0, descriptor.desCapacity, SocketFlags.None);
                descriptor.desLength = length;
                onPacket(descriptor);
            }
            
        }

        protected override void onStart()
        {
            if (ipAddress == null)
            {
                throw new PcapException("cannot find network interface.");
            }
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.IP);
            socket.Bind(new IPEndPoint(IPAddress.Parse(ipAddress), 0));
            socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.HeaderIncluded, true);
            socket.IOControl(SIO_RCVALL, IN, OUT);
        }

        protected override void onStop()
        {
            if(socket != null)
            {
                socket.Close();
                socket = null;
            }
        }
    }
}

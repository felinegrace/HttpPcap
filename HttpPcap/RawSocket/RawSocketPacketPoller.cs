using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Amber.Kit.HttpPcap.Common;



namespace Amber.Kit.HttpPcap.RawSocket
{
    class RawSocketPacketPoller : PacketCaptureBase
    {
        private Socket socket { get; set; }

        private byte[] IN = new byte[4] { 1, 0, 0, 0 };
        private byte[] OUT = new byte[4];

        private IocpReceiveAction iocpReceiveAction { get; set; }
        public RawSocketPacketPoller(string ipAddress, Action<Descriptor> onPacket)
            : base(ipAddress, onPacket)
        {
            socket = null;
            descriptor = DescriptorBuffer.create(maxiumBytesStoredOfEachPacket);
            iocpReceiveAction = new IocpReceiveAction(onPacket);
        }

        public override void stop()
        {
            if (socket != null)
            {
                iocpReceiveAction.detachSocket();
                socket.Close();
                socket = null;
            }
        }

        protected override void run()
        {
            if (socket == null)
            {
                if (ipAddress == null)
                {
                    throw new PcapException("cannot find network interface.");
                }
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.IP);
                socket.Bind(new IPEndPoint(IPAddress.Parse(ipAddress), 0));
                socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.HeaderIncluded, true);
                socket.IOControl(IOControlCode.ReceiveAll, IN, OUT);
                iocpReceiveAction.attachSocket(socket);
                iocpReceiveAction.recv();
            }
        }
    }
}

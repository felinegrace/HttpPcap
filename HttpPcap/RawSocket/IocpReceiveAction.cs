using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using Amber.Kit.HttpPcap.Common;

namespace Amber.Kit.HttpPcap.RawSocket
{
    class IocpReceiveAction : IocpActionBase
    {
        protected Socket socket { get; set; }
        private DescriptorBuffer buffer { get; set; }
        private bool continousReceive { get; set; }
        private Action<Descriptor> onReceivedAction { get; set; }

        private const int defaultReceiveBufferSize = 1024 * 4;

        public IocpReceiveAction(Action<Descriptor> onReceivedAction)
        {
            this.onReceivedAction = onReceivedAction;
            buffer = DescriptorBuffer.create(defaultReceiveBufferSize);
            iocpEventArgs.SetBuffer(buffer.des, 0, defaultReceiveBufferSize);
        }

        public void attachSocket(Socket socket)
        {
            this.socket = socket;
            this.iocpAsyncDelegate = new IocpAsyncDelegate(socket.ReceiveAsync);
        }

        public void detachSocket()
        {
            this.iocpAsyncDelegate = null;
            try
            {
                
                socket.Shutdown(SocketShutdown.Receive);
                this.socket = null;
            }
            // throws if client process has already closed
            catch (Exception) { }
        }

        protected sealed override void onIocpEvent(out bool continousAsyncCall)
        {
            // Check if the remote host closed the connection.
            if (iocpEventArgs.BytesTransferred > 0)
            {
                checkSocketError();
                buffer.desLength = iocpEventArgs.BytesTransferred;
                onReceivedAction(buffer);
            }
            continousAsyncCall = continousReceive;
        }

        public void recv()
        {
            continousReceive = true;
            iocpOperation();
        }

        public void shutdown()
        {
            continousReceive = false;
            try
            {
                socket.Shutdown(SocketShutdown.Receive);
            }
            // throws if client process has already closed
            catch (Exception) { }
        }
    }

}

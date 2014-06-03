using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using Amber.Kit.HttpPcap.Common;

namespace Amber.Kit.HttpPcap.RawSocket
{
    abstract class IocpActionBase
    {
        protected delegate bool IocpAsyncDelegate(SocketAsyncEventArgs args);
        protected IocpAsyncDelegate iocpAsyncDelegate { get; set; }
        protected SocketAsyncEventArgs iocpEventArgs { get; private set; }

        protected IocpActionBase()
        {
            iocpEventArgs = new SocketAsyncEventArgs();
            iocpEventArgs.UserToken = this;
            iocpEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(this.onIocpEventBase);
        }

        protected abstract void onIocpEvent(out bool continousAsyncCall);

        protected void iocpOperation()
        {
            bool continousAsyncCall = true;
            while (continousAsyncCall == true &&
                //false if I/O operation completed synchronously
                iocpAsyncDelegate(iocpEventArgs) == false)
            {
                continousAsyncCall = false;
                onIocpEvent(out continousAsyncCall);
            }
        }

        protected void onIocpEventBase(object sender, SocketAsyncEventArgs socketAsyncEventArgs)
        {
            //incoming param socketAsyncEventArgs should be exactly the same as wrapped iocpEventArgs
            if (socketAsyncEventArgs != iocpEventArgs)
            {
                throw new PcapException("Iocp Event Args not match.");
            }
            bool continousAsyncCall = false;
            onIocpEvent(out continousAsyncCall);
            while (continousAsyncCall == true &&
                //false if I/O operation completed synchronously
                iocpAsyncDelegate(socketAsyncEventArgs) == false)
            {
                continousAsyncCall = false;
                onIocpEvent(out continousAsyncCall);
            }
        }

        protected void checkSocketError()
        {
            if (iocpEventArgs.SocketError == SocketError.OperationAborted)
            {
                //ignored operation cancel
            }
            else if (iocpEventArgs.SocketError != SocketError.Success)
            {
                throw new PcapException(iocpEventArgs.SocketError.ToString());
            }
        }

        class IocpErrorEventArgs : EventArgs
        {
            public string message { get; private set; }
            public IocpErrorEventArgs(string message)
            {
                this.message = message;
            }
        }
    }

}

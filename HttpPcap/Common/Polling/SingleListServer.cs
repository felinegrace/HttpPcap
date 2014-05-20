using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Amber.Kit.HttpPcap.Common
{
    abstract class SingleListServer<T> : EventablePollingThread
    {
        private object queueLocker { get; set; }
        private class ServerEventArgs : EventArgs
        {
            public T request { get; set; }
            public ServerEventArgs(T request)
                : base()
            {
                this.request = request;
            }
        }
        private delegate void ServerEventHandler(object sender, ServerEventArgs args);
        private event ServerEventHandler ServerEvent;
        private Queue<T> requestQueue { get; set; }
        public SingleListServer() : this(new Queue<T>() , new AutoResetEvent(false))
        {
            
        }
        public SingleListServer(Queue<T> requestQueue , AutoResetEvent notifyEvent) : base(notifyEvent)
        {
            this.requestQueue = requestQueue;
            queueLocker = new object();
            ServerEvent = new ServerEventHandler(this.onServerEvent);
        }
        public void postRequest(T request)
        {
            lock (queueLocker)
            {
                requestQueue.Enqueue(request);
            }
            notify();
        }
        protected sealed override void onEventablePoll()
        {
            lock (queueLocker)
            {
                while (requestQueue.Count > 0)
                {
                    T request = requestQueue.Dequeue();
                    ServerEventArgs arg = new ServerEventArgs(request);
                    ServerEvent(this, arg); 
                }
            }
        }
        private void onServerEvent(object sender, ServerEventArgs args)
        {
            handleRequest(args.request);
        }
        protected abstract void handleRequest(T request);

    }
}

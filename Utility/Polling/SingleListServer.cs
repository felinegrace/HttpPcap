using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Cabinet.Utility
{
    public abstract class SingleListServer<T> : EventablePollingThread
    {
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
        private ConcurrentQueue<T> requestQueue { get; set; }
        public SingleListServer() : this(new ConcurrentQueue<T>() , new AutoResetEvent(false))
        {

        }
        public SingleListServer(ConcurrentQueue<T> requestQueue , AutoResetEvent notifyEvent) : base(notifyEvent)
        {
            this.requestQueue = requestQueue;
            ServerEvent = new ServerEventHandler(this.onServerEvent);
        }
        public void postRequest(T request)
        {
            requestQueue.Enqueue(request);
            notify();
        }
        protected sealed override void onEventablePoll()
        {
            while (!requestQueue.IsEmpty)
            {
                T request;
                bool hasMore = requestQueue.TryDequeue(out request);
                if(hasMore)
                {
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

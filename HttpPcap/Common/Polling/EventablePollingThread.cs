using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Amber.Kit.HttpPcap.Common
{
    abstract class EventablePollingThread : PollingThread
    {
        private AutoResetEvent notifyEvent { get; set; }
        public EventablePollingThread(AutoResetEvent notifyEvent)
        {
            this.notifyEvent = notifyEvent;
        }

        public override void stop()
        {
            base.stop();
            notifyEvent.Set();
            
        }

        protected void notify()
        {
            notifyEvent.Set();
        }

        protected sealed override void onPolling()
        {
            //Logger.debug("EventablePollingThread: waiting for signal...");
            notifyEvent.WaitOne(-1);
            //Logger.debug("EventablePollingThread: received signal, peeking...");
            onEventablePoll();
        }

        protected abstract void onEventablePoll();
    }
}

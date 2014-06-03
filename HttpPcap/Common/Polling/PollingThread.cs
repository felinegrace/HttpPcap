using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Amber.Kit.HttpPcap.Common
{
    abstract class PollingThread : AsyncObjectBase
    {
        
        private AutoResetEvent terminalEvent { get; set; }
        
        public PollingThread()
        {
            terminalEvent = new AutoResetEvent(false);
        }

        public override void stop()
        {
            terminalEvent.Set();
        }

        protected override void run()
        {
            try
            {
                terminalEvent.Reset();
                onStart();
                while (!terminalEvent.WaitOne(0))
                {
                    onPolling();
                }
            }
            catch (System.Exception ex)
            {
                stop();
                if (onError != null)
                    onError(ex.Message);
            }
            finally
            {
                onStop();
            }
        }
        protected abstract void onPolling();
        protected virtual void onStart()
        {

        }
        protected virtual void onStop()
        {

        }
    }
}

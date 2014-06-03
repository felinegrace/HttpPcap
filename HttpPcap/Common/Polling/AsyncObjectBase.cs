using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Amber.Kit.HttpPcap.Common
{
    abstract class AsyncObjectBase : ExecutableObjectBase
    {
        private Thread thread { get; set; }
        static void invoke(object obj)
        {
            AsyncObjectBase transObject = obj as AsyncObjectBase;
            transObject.run();
        }
        public override sealed void start()
        {
            thread = new Thread(invoke);
            thread.Start(this);
        }

        public override void stop()
        {
            
        }

        protected abstract void run();
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amber.Kit.HttpPcap.Common
{
    abstract class ExecutableObjectBase
    {
        public Action<string> onError { get; set; }

        public abstract void start();

        public abstract void stop();
    }
}

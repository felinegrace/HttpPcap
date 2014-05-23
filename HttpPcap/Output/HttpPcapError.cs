using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amber.Kit.HttpPcap
{
    /// <summary>
    /// 事件<see cref = "Amber.Kit.HttpPcap.HttpPcapEntry.onHttpPcapErrorEvent">HttpPcapEntry.onHttpPcapErrorEvent</see>的参数.<para/>
    /// </summary>
    public class HttpPcapError : EventArgs
    {
        /// <summary>
        /// 以字符串表示的错误原因.<para/>
        /// 如遇到错误请联系我 :) <para/>
        /// </summary>
        public string message { get; private set; }
        internal HttpPcapError(string message)
        {
            this.message = message;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amber.Kit.HttpPcap
{
    /// <summary>
    /// 事件<see cref = "Amber.Kit.HttpPcap.HttpPcapEntry.onHttpTransactionEvent">HttpPcapEntry.onHttpTransactionEvent</see>的参数.<para/>
    /// </summary>
    public class HttpTransaction : EventArgs
    {
        /// <summary>
        /// 此HTTP事务对应的请求.
        /// </summary>
        public HttpRequest httpRequest { get; set; }
        /// <summary>
        /// 此HTTP事务对应的回应.
        /// </summary>
        public HttpResponse httpResponse { get; set; }

        internal HttpTransaction()
        {

        }
    }
}

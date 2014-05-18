using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amber.Kit.HttpPcap
{
    /// <summary>
    /// 事件<see cref = "Amber.Kit.HttpPcap.HttpPcapEntry.onHttpPcapRequestEvent">HttpPcapEntry.onHttpPcapRequestEvent</see>
    /// 与<see cref = "Amber.Kit.HttpPcap.HttpPcapEntry.onHttpPcapRequestEvent">HttpPcapEntry.onHttpPcapResponseEvent</see>
    /// 的通用参数.<para/>
    /// </summary>
    public class HttpPacketEventArgs : EventArgs
    {
        /// <summary>
        /// 以byte数组表示的原始的包数据.<para/>
        /// 数据内容不一定是可显示的字符串,有些回应数据可能会以gzip等方式压缩.<para/>
        /// 由于时间紧迫,没有实现解压缩.<para/>
        /// </summary>
        public byte[] content { get; private set; }
        internal HttpPacketEventArgs(byte[] content)
        {
            this.content = content;
        }
    }

    /// <summary>
    /// 事件<see cref = "Amber.Kit.HttpPcap.HttpPcapEntry.onHttpPcapTransactionEvent">HttpPcapEntry.onHttpPcapTransactionEvent</see>的参数.<para/>
    /// </summary>
    public class HttpTransactionEventArgs : EventArgs
    {
        /// <summary>
        /// 以byte数组表示的原始的请求包数据.<para/>
        /// </summary>
        public byte[] requestContent { get; private set; }

        /// <summary>
        /// 以byte数组表示的原始的回应包数据.<para/>
        /// 数据内容不一定是可显示的字符串,有些回应数据可能会以gzip等方式压缩.<para/>
        /// 由于时间紧迫,没有实现解压缩.<para/>
        /// </summary>
        public byte[] responseContent { get; private set; }
        internal HttpTransactionEventArgs(byte[] requestContent, byte[] responseContent)
        {
            this.requestContent = requestContent;
            this.responseContent = responseContent;
        }
    }

    /// <summary>
    /// 事件<see cref = "Amber.Kit.HttpPcap.HttpPcapEntry.onHttpPcapErrorEvent">HttpPcapEntry.onHttpPcapErrorEvent</see>的参数.<para/>
    /// </summary>
    public class HttpPcapErrorEventArgs : EventArgs
    {
        /// <summary>
        /// 以字符串表示的错误原因.<para/>
        /// 如遇到错误请联系我 :) <para/>
        /// </summary>
        public string message { get; private set; }
        internal HttpPcapErrorEventArgs(string message)
        {
            this.message = message;
        }
    }
}

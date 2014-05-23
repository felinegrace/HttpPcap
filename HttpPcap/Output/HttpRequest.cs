using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amber.Kit.HttpPcap
{
    /// <summary>
    /// 事件<see cref = "Amber.Kit.HttpPcap.HttpPcapEntry.onHttpRequestEvent">HttpPcapEntry.onHttpRequestEvent</see>的参数.<para/>
    /// </summary>
    public class HttpRequest : EventArgs
    {
        /// <summary>
        /// 以byte序列表示的原始请求包数据.<para/>
        /// 在此处的数据中的uri没有经过转义.<para/>
        /// </summary>
        public List<byte> rawStream { get; set; }
        public string method { get; set; }
        public string uri { get; set; }
        public string host { get; set; }

        internal HttpRequest()
        {
            rawStream = new List<byte>();
            method = string.Empty;
            uri = string.Empty;
            host = string.Empty;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace Amber.Kit.HttpPcap
{
    /// <summary>
    /// 事件<see cref = "Amber.Kit.HttpPcap.HttpPcapEntry.onHttpResponseEvent">HttpPcapEntry.onHttpResponseEvent</see>的参数.
    /// </summary>
    public class HttpResponse : EventArgs
    {
        /// <summary>
        /// 以byte序列表示的原始回应包数据.<para/>
        /// 数据内容不一定是可显示的字符串,有些回应数据可能会以gzip等方式压缩.<para/>
        /// 由于时间紧迫,没有实现解压缩.<para/>
        /// </summary>
        public List<byte> rawHeaderStream { get; set; }
        public List<byte> rawChunkedStream { get; set; }
        public List<byte> rawChunkedEntityBody { get; set; }
        public string chunkedEntityBody { get; set; }
        public int statusCode { get; set; }
        public string transferEncoding { get; set; }
        public string contentEncoding { get; set; }
        public string charset { get; set; }
        internal HttpResponse()
        {
            rawHeaderStream = new List<byte>();
            rawChunkedStream = new List<byte>();
            rawChunkedEntityBody = new List<byte>();
            chunkedEntityBody = string.Empty;
            transferEncoding = string.Empty;
            contentEncoding = string.Empty;
            charset = string.Empty;
        }
    }
}

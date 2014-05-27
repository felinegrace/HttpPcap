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
        /// 以字符串表示的原始请求包头.<para/>
        /// 在此处的数据中的uri没有经过转义.<para/>
        /// 可以用于提取本类中未涉及的其他信息行.<para/>
        /// </summary>
        public string rawHeader { get; set; }
        /// <summary>
        /// 以字符串表示的<see href="http://tools.ietf.org/html/rfc2616#section-5.1.1">HTTP方法(RFC2616-5.1.1)</see>,如GET.<para/>
        /// </summary>
        public string method { get; set; }
        /// <summary>
        /// 以字符串表示的<see href="http://tools.ietf.org/html/rfc2616#section-5.1.2">HTTP请求URI(RFC2616-5.1.2)</see>,在此URI中出现的unicode字符会被转义.<para/>
        /// </summary>
        public string uri { get; set; }

        /// <summary>
        /// 以字符串表示的<see href="http://tools.ietf.org/html/rfc2616#section-14.23">主机名称或者地址(RFC2616-14.23)</see>.<para/>
        /// </summary>
        public string host { get; set; }

        internal HttpRequest()
        {
            method = string.Empty;
            uri = string.Empty;
            host = string.Empty;
        }
    }
}

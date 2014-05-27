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
        /// 以字符串表示的原始回应包头.<para/>
        /// 可以用于提取本类中未涉及的其他信息行.<para/>
        /// </summary>
        public string rawHeader { get; set; }
        /// <summary>
        /// 以byte序列表示的回应包体.<para/>
        /// 数据内容不一定全是可显示的字符串.<para/>
        /// 如果此回应包是<see href="http://tools.ietf.org/html/rfc2616#section-3.6">分段传输(RFC2616-3.6)</see>的,本段内容则为原始的保留网络上传输格式的未合并的分段传输数据.<para/>
        /// </summary>
        public List<byte> rawEntity { get; set; }
        /// <summary>
        /// 以byte序列表示的原始的<see href="http://tools.ietf.org/html/rfc2616#section-3.6.1">解包合并(RFC2616-3.6.1)</see>的分段传输数据.<para/>
        /// 数据内容不一定是可显示的字符串.<para/>
        /// 如果此回应包是<see href="http://tools.ietf.org/html/rfc2616#section-3.6">分段传输(RFC2616-3.6)</see>的,本段内容才有效.<para/>
        /// </summary>
        public List<byte> rawChunkedEntity { get; set; }
        /// <summary>
        /// 以字符串表示的<see href="http://tools.ietf.org/html/rfc2616#section-3.5">解码(RFC2616-3.5)</see>后的文本分段传输数据.<para/>
        /// 如果此回应包是<see href="http://tools.ietf.org/html/rfc2616#section-3.6">分段传输(RFC2616-3.6)</see>的,且分段传输的<see href="http://tools.ietf.org/html/rfc2616#section-3.7">媒体类型(RFC2616-3.7)</see>是文本(text/*),本段内容才有效.<para/>
        /// </summary>
        public string textEntity { get; set; }
        /// <summary>
        /// 以整数表示的回应包<see href="http://tools.ietf.org/html/rfc2616#section-6.1.1">状态码(RFC2616-6.1.1).</see><para/>
        /// </summary>
        public int statusCode { get; set; }
        /// <summary>
        /// 以字符串表示的回应包是否为<see href="http://tools.ietf.org/html/rfc2616#section-3.6">分段传输(RFC2616-3.6)</see>的.<para/>
        /// 如果是分段传输则为<code>transferEncoding.Equals("chunked")</code><para/>
        /// </summary>
        public string transferEncoding { get; set; }
        /// <summary>
        /// 以字符串表示的<see href="http://tools.ietf.org/html/rfc2616#section-3.5">解码(RFC2616-3.5)</see>格式.<para/>
        /// 仅支持<code>contentEncoding.Equals("gzip")</code><code>contentEncoding.Equals("deflate")</code><para/>
        /// 其它格式会导致乱码.<para/>
        /// </summary>
        public string contentEncoding { get; set; }
        /// <summary>
        /// 以布尔值表示的<see href="http://tools.ietf.org/html/rfc2616#section-3.7">媒体类型(RFC2616-3.7)</see>是否为文本(text/*,*/javascript,*/json).<para/>
        /// </summary>
        public bool isTextEntityBody { get; set; }
        /// <summary>
        /// 以字符串表示的<see href="http://tools.ietf.org/html/rfc2616#section-3.7">媒体类型(RFC2616-3.7)</see>中所指明的字符集.<para/>
        /// </summary>
        public string charset { get; set; }
        internal HttpResponse()
        {
            rawEntity = new List<byte>();
            rawChunkedEntity = new List<byte>();
            textEntity = string.Empty;
            transferEncoding = string.Empty;
            contentEncoding = string.Empty;
            charset = string.Empty;
        }
    }
}

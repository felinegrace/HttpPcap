using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amber.Kit.HttpPcap.HttpBusiness
{
    class HttpTransactionPair
    {
        public HttpRequestParser httpRequestParser { get; set; }
        public HttpResponseParser httpResponseParser { get; set; }
    }
}

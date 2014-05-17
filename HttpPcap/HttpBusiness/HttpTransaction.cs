using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amber.Kit.HttpPcap.HttpBusiness
{
    class HttpTransaction
    {
        public List<byte> rawRequest { get; set; }
        public List<byte> rawResponse { get; set; }

        internal HttpTransaction()
        {
            rawRequest = new List<byte>();
            rawResponse = new List<byte>();
        }
    }
}

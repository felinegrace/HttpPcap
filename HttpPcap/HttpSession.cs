using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amber.Kit.HttpPcap
{
    [Serializable()]
    public struct httpsession
    {
        public string url;
        public string method;
        public DateTime senddtime;
        public DateTime responoversetime;
        public int id;
        public string ack;
        public List<byte> sendraw;
        public List<byte> responseraw;
        public int statucode;
    }

}

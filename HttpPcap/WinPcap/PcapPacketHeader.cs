using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Amber.Kit.HttpPcap.WinPcap
{
    

    class PcapPacketHeader
    {
        public PcapStructWrapper.LlsTimeVal internalTimeStamp { private get; set; }
        public DateTime timeStamp
        {
            get
            {
                DateTime result;
                DateTime internalTimeStampBase = new DateTime(1970, 1, 1);
                result = internalTimeStampBase.AddSeconds((double)(internalTimeStamp.tv_sec));
                result = result.AddMilliseconds((double)(internalTimeStamp.tv_usec));
                return result;
            }
        }

        public int caplen { get; set; }
        public int len { get; set; }
    }
}

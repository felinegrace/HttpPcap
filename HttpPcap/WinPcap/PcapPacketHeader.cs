using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Amber.Kit.HttpPcap.WinPcap
{
    /// <summary>
    /// struct timeval
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct LlsTimeVal
    {
        public uint tv_sec;
        public uint tv_usec;
    }

    /// <summary>
    /// struct pcap_pkthdr 
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct LlsPcapPacketHeader
    {
        /// <summary>
        /// time stamp 
        /// </summary>
        public LlsTimeVal ts;

        /// <summary>
        /// length of portion present
        /// </summary>
        public uint caplen;

        /// <summary>
        /// length this packet (off wire) 
        /// </summary>
        public uint len;
    }

    public class PcapPacketHeader
    {
        public LlsTimeVal internalTimeStamp { private get; set; }
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

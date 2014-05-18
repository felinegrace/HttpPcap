using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Amber.Kit.HttpPcap.WinPcap
{
    /// <summary>
    /// 封装部分了WinPcap的C结构体,以便在C#中调用,并以前缀LLS(即LowLevelStruct)注明.
    /// </summary>
    public class PcapStructWrapper
    {
        /// <summary>
        /// struct pcap_addr 
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct LlsPcapAddr
        {
            /// <summary>
            /// if not NULL, a pointer to the next element in the list; 
            /// NULL for the last element of the list 
            /// </summary>
            public IntPtr next;

            /// <summary>
            /// a pointer to a struct sockaddr containing an address 
            /// </summary>
            public IntPtr addr;

            /// <summary>
            /// if not NULL, a pointer to a struct sockaddr that contains the netmask corresponding to the address pointed to by addr. 
            /// </summary>
            public IntPtr netmask;

            /// <summary>
            /// if not NULL, a pointer to a struct sockaddr that contains the broadcast address corresponding to the address pointed to by addr; 
            /// may be null if the interface doesn't support 
            /// </summary>
            public IntPtr broadaddr;

            /// <summary>
            /// if not NULL, a pointer to a struct sockaddr that contains the destination address corresponding to the address pointed to by addr; 
            /// may be null if the interface isn't a point- to-point interface 
            /// </summary>
            public IntPtr dstaddr;
        }

        /// <summary>
        /// struct sockaddr , actual it is sockaddr_in
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct LlsSockaddr
        {
            /// <summary>
            /// Address family (must be AF_INET).
            /// </summary>
            public short family;

            /// <summary>
            /// IP port.
            /// </summary>
            public ushort port;

            /// <summary>
            /// IP address.
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public byte[] addr;

            /// <summary>
            /// Padding to make structure the same size as SOCKADDR.
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public byte[] zero;
        }

        /// <summary>
        /// struct pcap_if 
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct LlsPcapIf
        {
            /// <summary>
            /// if not NULL, a pointer to the next element in the list; 
            /// NULL for the last element of the list 
            /// </summary>
            public IntPtr next;

            /// <summary>
            /// a pointer to a string giving a name for the device to pass to pcap_open_live() 
            /// </summary>
            public string name;

            /// <summary>
            /// if not NULL, a pointer to a string giving a human-readable description of the device
            /// </summary>
            public string description;

            /// <summary>
            /// a pointer to the first element of a list of addresses for the interface 
            /// </summary>
            public IntPtr addresses;

            /// <summary>
            /// PCAP_IF_ interface flags. 
            /// Currently the only possible flag is PCAP_IF_LOOPBACK, 
            /// that is set if the interface is a loopback interface.
            /// </summary>
            public uint flags;
        }

        /// <summary>
        /// struct timeval
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct LlsTimeVal
        {
            /// <summary>
            /// Time interval, in seconds.
            /// </summary>
            public uint tv_sec;

            /// <summary>
            /// Time interval, in microseconds. This value is used in combination with the tv_sec member to represent time interval values that are not a multiple of seconds.
            /// </summary>
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
    }
    
}

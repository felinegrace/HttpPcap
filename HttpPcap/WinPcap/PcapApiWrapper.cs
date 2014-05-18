using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Amber.Kit.HttpPcap.WinPcap
{
    /// <summary>
    /// 封装部分了WinPcap的C函数接口,以便在C#中调用.
    /// </summary>
    public class PcapApiWrapper
    {
        private const string pcapBinaryName = "wpcap.dll";

        /// <summary>
        /// Open a generic source in order to capture / send (WinPcap only) traffic.
        /// The pcap_open() replaces all the pcap_open_xxx() functions with a single call.
        /// This function hides the differences between the different pcap_open_xxx() functions 
        /// so that the programmer does not have to manage different opening function. 
        /// In this way, the 'true' open function is decided according to the source type, 
        /// which is included into the source string (in the form of source prefix).
        /// </summary>
        /// <param name="source">
        /// zero-terminated string containing the source name to open. The source name has to include the format prefix according to the new Source Specification Syntax and it cannot be NULL.
        /// On on Linux systems with 2.2 or later kernels, a device argument of "any" (i.e. rpcap://any) can be used to capture packets from all interfaces. 
        /// In order to makes the source syntax easier, please remember that:
        ///  - the adapters returned by the pcap_findalldevs_ex() can be used immediately by the pcap_open()
        ///  - in case the user wants to pass its own source string to the pcap_open(), the pcap_createsrcstr() helps in creating the correct source identifier.
        /// </param>
        /// <param name="snaplen">
        /// length of the packet that has to be retained. 
        /// For each packet received by the filter, 
        /// only the first 'snaplen' bytes are stored in the buffer and passed to the user application. 
        /// For instance, snaplen equal to 100 means that only the first 100 bytes of each packet are stored.
        /// </param>
        /// <param name="flags">
        /// keeps several flags that can be needed for capturing packets. The allowed flags are defined in the pcap_open() flags .
        /// </param>
        /// <param name="read_timeout">
        /// read timeout in milliseconds. 
        /// The read timeout is used to arrange that the read not necessarily return immediately when a packet is seen, 
        /// but that it waits for some amount of time to allow more packets to arrive and to read multiple packets from the OS kernel in one operation. 
        /// Not all platforms support a read timeout; on platforms that don't, the read timeout is ignored.
        /// </param>
        /// <param name="auth">
        /// a pointer to a 'struct pcap_rmtauth' that keeps the information required to authenticate the user on a remote machine. 
        /// In case this is not a remote capture, this pointer can be set to NULL.
        /// </param>
        /// <param name="errbuf">
        /// a pointer to a user-allocated buffer which will contain the error in case this function fails.
        /// </param>
        /// <returns>
        /// A pointer to a 'pcap_t' 
        /// which can be used as a parameter to the following calls (pcap_compile() and so on) 
        /// and that specifies an opened WinPcap session. 
        /// In case of problems, it returns NULL and the 'errbuf' variable keeps the error message.
        /// </returns>
        [DllImport(pcapBinaryName)]
        public static extern IntPtr pcap_open(string source, int snaplen, int flags, int read_timeout, IntPtr auth, StringBuilder errbuf);
        
        
        /// <summary>
        /// close the files associated with p and deallocates resources.
        /// </summary>
        /// <param name="p">
        /// A pointer to a 'pcap_t' 
        /// </param>
        [DllImport(pcapBinaryName)]
        public static extern void pcap_close(IntPtr p);

        /// <summary>
        /// Construct a list of network devices that can be opened with pcap_open_live().
        /// </summary>
        /// <param name="devicelist">
        /// alldevsp is set to point to the first element of the list; each element of the list is of type pcap_if_t,
        /// </param>
        /// <param name="errbuf">
        /// a pointer to a user-allocated buffer which will contain the error in case this function fails.
        /// </param>
        /// <returns>
        /// -1 is returned on failure, in which case errbuf is filled in with an appropriate error message; 
        /// 0 is returned on success.
        /// </returns>
        [DllImport(pcapBinaryName)]
        public static extern int pcap_findalldevs(ref IntPtr devicelist, StringBuilder errbuf);

        /// <summary>
        /// Free an interface list returned by pcap_findalldevs().
        /// </summary>
        /// <param name="devicelist">
        /// a list allocated by pcap_findalldevs().
        /// </param>
        [DllImport(pcapBinaryName)]
        public static extern void pcap_freealldevs(IntPtr devicelist);

        /// <summary>
        /// Read a packet from an interface or from an offline capture.
        /// This function is used to retrieve the next available packet, 
        /// bypassing the callback method traditionally provided by libpcap.
        /// </summary>
        /// <param name="p">
        /// A pointer to a 'pcap_t'
        /// </param>
        /// <param name="pkt_header">
        /// pointers to the header
        /// </param>
        /// <param name="packetdata">
        /// the next captured packet
        /// </param>
        /// <returns>
        /// 1 if the packet has been read without problems
        /// 0 if the timeout set with pcap_open_live() has elapsed. In this case pkt_header and pkt_data don't point to a valid packet
        /// -1 if an error occurred
        /// -2 if EOF was reached reading from an offline capture
        /// </returns>
        [DllImport(pcapBinaryName)]
        public static extern int pcap_next_ex(IntPtr p, ref IntPtr pkt_header, ref IntPtr packetdata);

        /// <summary>
        /// Set the minumum amount of data received by the kernel in a single call.
        /// If an old buffer was already created with a previous call to pcap_setbuff(), 
        /// it is deleted and its content is discarded. 
        /// pcap_open_live() creates a 1 MByte buffer by default.
        /// </summary>
        /// <param name="p">
        /// A pointer to a 'pcap_t'
        /// </param>
        /// <param name="size">
        /// the size of the buffer in bytes
        /// </param>
        /// <returns>
        /// 0 when the call succeeds, -1 otherwise
        /// </returns>
        [DllImport(pcapBinaryName)]
        public static extern int pcap_setmintocopy(IntPtr p, int size);

        internal static bool isNotNullPtr(IntPtr ptr)
        {
            return ptr != IntPtr.Zero;
        }

        internal static T toLowLevelStruct<T>(IntPtr LlsPtr)
        {
            return (T)Marshal.PtrToStructure(LlsPtr, typeof(T));
        }
    }
}

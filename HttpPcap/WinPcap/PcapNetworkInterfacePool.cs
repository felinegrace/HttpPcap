using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Amber.Kit.HttpPcap.CommonObject;

namespace Amber.Kit.HttpPcap.WinPcap
{
    class PcapNetworkInterfacePool
    {
        /// <summary>
        /// struct pcap_addr 
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct LlsPcapAddr
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

        [StructLayout(LayoutKind.Sequential)]
        private struct LlsSockaddr
        {
            public short family;
            public ushort port;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public byte[] addr;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public byte[] zero;
        }

        /// <summary>
        /// struct pcap_if 
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct LlsPcapIf
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

        private LlsPcapIf newLlsPcapNetworkInterface()
        {
            LlsPcapIf newItem;
            newItem.addresses = IntPtr.Zero;
            newItem.description = new StringBuilder().ToString();
            newItem.flags = 0;
            newItem.name = new StringBuilder().ToString();
            newItem.next = IntPtr.Zero;
            return newItem;
        }

        public List<PcapNetworkInterface> interfaceList { get; set; }
        public PcapNetworkInterfacePool()
        {
            interfaceList = new List<PcapNetworkInterface>();
        }

        public void findAllInterfaces()
        {
            IntPtr intfIterator = IntPtr.Zero;
            IntPtr usedToFreeList = IntPtr.Zero;
            StringBuilder errorMessage = new StringBuilder(0x100);

            if (PcapApiWrapper.pcap_findalldevs(ref intfIterator, errorMessage) == -1)
            {
                throw new PcapException(errorMessage.ToString());
            }
            usedToFreeList = intfIterator;
            interfaceList.Clear();

            try
            {
                LlsPcapIf intf = newLlsPcapNetworkInterface();
                while (PcapApiWrapper.isNotNullPtr(intfIterator))
                {
                    intf = PcapApiWrapper.toLowLevelStruct<LlsPcapIf>(intfIterator);
                    fillItemToList(intf);
                    intfIterator = intf.next;
                }
            }
            finally
            {
                if (PcapApiWrapper.isNotNullPtr(usedToFreeList))
                {
                    PcapApiWrapper.pcap_freealldevs(usedToFreeList);
                }
            }
        }

        private void fillItemToList(LlsPcapIf intf)
        {
            PcapNetworkInterface curInterface = new PcapNetworkInterface();
          
            curInterface.name = intf.name;
            curInterface.description = intf.description;
            if (PcapApiWrapper.isNotNullPtr(intf.addresses))
            {
                LlsPcapAddr pcapAddr = PcapApiWrapper.toLowLevelStruct<LlsPcapAddr>(intf.addresses);
                if (PcapApiWrapper.isNotNullPtr(pcapAddr.addr))
                {
                    LlsSockaddr addr = PcapApiWrapper.toLowLevelStruct<LlsSockaddr>(pcapAddr.addr);
                    curInterface.address =
                        addr.addr[0].ToString() + "." +
                        addr.addr[1].ToString() + "." +
                        addr.addr[2].ToString() + "." +
                        addr.addr[3].ToString();
                }
                if (PcapApiWrapper.isNotNullPtr(pcapAddr.netmask))
                {
                    LlsSockaddr mask = PcapApiWrapper.toLowLevelStruct<LlsSockaddr>(pcapAddr.netmask);
                    curInterface.netmask =
                        mask.addr[0].ToString() + "." +
                        mask.addr[1].ToString() + "." +
                        mask.addr[2].ToString() + "." +
                        mask.addr[3].ToString();
                }
            }

            interfaceList.Add(curInterface);
        }
    }
}

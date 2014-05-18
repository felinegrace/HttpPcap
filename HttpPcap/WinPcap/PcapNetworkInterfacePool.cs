using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Amber.Kit.HttpPcap.Common;

namespace Amber.Kit.HttpPcap.WinPcap
{
    class PcapNetworkInterfacePool
    {

        private PcapStructWrapper.LlsPcapIf newLlsPcapNetworkInterface()
        {
            PcapStructWrapper.LlsPcapIf newItem;
            newItem.addresses = IntPtr.Zero;
            newItem.description = new StringBuilder().ToString();
            newItem.flags = 0;
            newItem.name = new StringBuilder().ToString();
            newItem.next = IntPtr.Zero;
            return newItem;
        }

        private List<PcapNetworkInterface> interfaceList { get; set; }
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
                PcapStructWrapper.LlsPcapIf intf = newLlsPcapNetworkInterface();
                while (PcapApiWrapper.isNotNullPtr(intfIterator))
                {
                    intf = PcapApiWrapper.toLowLevelStruct<PcapStructWrapper.LlsPcapIf>(intfIterator);
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

        private void fillItemToList(PcapStructWrapper.LlsPcapIf intf)
        {
            PcapNetworkInterface curInterface = new PcapNetworkInterface();
          
            curInterface.name = intf.name;
            curInterface.description = intf.description;
            while (PcapApiWrapper.isNotNullPtr(intf.addresses))
            {
                PcapStructWrapper.LlsPcapAddr pcapAddr = PcapApiWrapper.toLowLevelStruct<PcapStructWrapper.LlsPcapAddr>(intf.addresses);
                if (PcapApiWrapper.isNotNullPtr(pcapAddr.addr))
                {
                    PcapStructWrapper.LlsSockaddr addr = PcapApiWrapper.toLowLevelStruct<PcapStructWrapper.LlsSockaddr>(pcapAddr.addr);
                    curInterface.address.Add(
                        addr.addr[0].ToString() + "." +
                        addr.addr[1].ToString() + "." +
                        addr.addr[2].ToString() + "." +
                        addr.addr[3].ToString());
                }
                intf.addresses = pcapAddr.next;
            }

            interfaceList.Add(curInterface);
        }

        public string getNetworkInterfaceNameByIpAddress(string ipAddress)
        {
            foreach(PcapNetworkInterface pcapNetworkInterface in interfaceList)
            {
                if (pcapNetworkInterface.address.Contains(ipAddress))
                {
                    return pcapNetworkInterface.name;
                }
            }
            return null;
        }
    }
}

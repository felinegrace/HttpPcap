using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Amber.Kit.HttpPcap.Common;


namespace Amber.Kit.HttpPcap.WinPcap
{
    class PcapPacketPoller : PacketPollerBase
    {

        private IntPtr pcapHandle { get; set; }

        private const int waitTimeOut = 1000;
        private const int PCAP_OPENFLAG_PROMISCUOUS = 1;
        private const int PCAP_OPENFLAG_DATATX_UDP = 2;
        private const int PCAP_OPENFLAG_NOCAPTURE_RPCAP = 4;
        private const int PCAP_OPENFLAG_NOCAPTURE_LOCAL = 8;
        private const int PCAP_OPENFLAG_MAX_RESPONSIVENESS = 16;
        private const int internalCopyThreshold = 128;
        public PcapPacketPoller(string ipAddress, Action<Descriptor> onPacket) 
            : base(ipAddress, onPacket)
        {
            pcapHandle = IntPtr.Zero;
        }

        private bool nextPacket()
        {
            IntPtr llsPacketHeaderPtr = IntPtr.Zero;
            IntPtr llPacketBufferPtr = IntPtr.Zero;
            //problem not found
            //seems impossible
            //if (!PcapApiWrapper.isNotNullPtr(this.pcapHandle))
            //{
            //    return false;
            //}
            switch (PcapApiWrapper.pcap_next_ex(this.pcapHandle, ref llsPacketHeaderPtr, ref llPacketBufferPtr))
            {
                case 0:
                    return false; //TIMEOUT

                case 1:
                    {
                        PcapStructWrapper.LlsPcapPacketHeader llsPacketHeader =
                            PcapApiWrapper.toLowLevelStruct<PcapStructWrapper.LlsPcapPacketHeader>(llsPacketHeaderPtr);
                        //warning:  part of packet causes caplen < len would not be captured 
                        //          (packet actual length > maxiumBytesStoredOfEachPacket)
                        EthernetHeader ethernetHeader = new EthernetHeader(llPacketBufferPtr, llsPacketHeader.caplen);
                        if (ethernetHeader.isIP())
                        {
                            descriptor = new DescriptorReference(ethernetHeader.ethernetData, ethernetHeader.ethernetData.Length);
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }

                case -1:
                    return false; //ERROR

                case -2:
                    return false; //EOF

                default:
                    return false; //UNKNOWN

            }
        }

        protected override void onPolling()
        {
 	        bool result = nextPacket();
            if (result == true)
            {
                if (onPacket != null)
                {
                    onPacket(descriptor);
                }
            }
        }

        protected sealed override void onStart()
        {

            PcapNetworkInterfacePool pcapNetworkInterfacePool = new PcapNetworkInterfacePool();
            pcapNetworkInterfacePool.findAllInterfaces();
            string networkInterfaceName = pcapNetworkInterfacePool.getNetworkInterfaceNameByIpAddress(ipAddress);
            
            if (networkInterfaceName == null)
            {
                throw new PcapException("cannot find network interface.");
            }
            StringBuilder errBuffer = new StringBuilder();
            pcapHandle = PcapApiWrapper.pcap_open(
                networkInterfaceName, 
                maxiumBytesStoredOfEachPacket,
                PCAP_OPENFLAG_PROMISCUOUS,
                waitTimeOut,
                IntPtr.Zero, errBuffer);

            if (!PcapApiWrapper.isNotNullPtr(pcapHandle))
            {
                throw new PcapException(errBuffer.ToString());
            }

            int setInternalBufferResult = PcapApiWrapper.pcap_setmintocopy(pcapHandle, internalCopyThreshold);
            if(setInternalBufferResult == -1)
            {
                throw new PcapException("error setting internal copy threshold.");
            }

            
        }

        protected sealed override void onStop()
        {
            if (PcapApiWrapper.isNotNullPtr(pcapHandle))
            {
                PcapApiWrapper.pcap_close(pcapHandle);
                pcapHandle = IntPtr.Zero;
            }
        }
    }
}

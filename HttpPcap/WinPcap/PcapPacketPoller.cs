using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Cabinet.Utility;
using Amber.Kit.HttpPcap.CommonObject;


namespace Amber.Kit.HttpPcap.WinPcap
{
    public class PcapPacketPoller : PollingThread
    {
        private IntPtr pcapHandle { get; set; }
        public string networkInterfaceName { get; set; }
        private PcapPacketHeader packetHeader { get; set; }
        private byte[] packetBuffer { get; set; }
        public Action<PcapPacketHeader, byte[]> onPacket { get; set; }


        private const int maxiumBytesStoredOfEachPacket = 256 * 256;
        private const int waitMillisecondsForReadingAfterPacketArrived = 0;
        private const int PCAP_OPENFLAG_PROMISCUOUS = 1;
        private const int PCAP_OPENFLAG_DATATX_UDP = 2;
        private const int PCAP_OPENFLAG_NOCAPTURE_RPCAP = 4;
        private const int PCAP_OPENFLAG_NOCAPTURE_LOCAL = 8;
        private const int PCAP_OPENFLAG_MAX_RESPONSIVENESS = 16;
        private const int internalCopyThreshold = 128;
        public PcapPacketPoller(Action<PcapPacketHeader, byte[]> onPacket)
        {
            pcapHandle = IntPtr.Zero;
            this.onPacket = onPacket;
            packetHeader = new PcapPacketHeader();
            packetBuffer = new byte[maxiumBytesStoredOfEachPacket];
        }

        private bool nextPacket()
        {
            IntPtr llsPacketHeaderPtr = IntPtr.Zero;
            IntPtr llPacketBufferPtr = IntPtr.Zero;
            switch (PcapApiWrapper.pcap_next_ex(this.pcapHandle, ref llsPacketHeaderPtr, ref llPacketBufferPtr))
            {
                case 0:
                    return false; //TIMEOUT

                case 1:
                    {
                        LlsPcapPacketHeader llsPacketHeader =
                            PcapApiWrapper.toLowLevelStruct<LlsPcapPacketHeader>(llsPacketHeaderPtr);
                        packetHeader.internalTimeStamp = llsPacketHeader.ts;
                        packetHeader.caplen = (int)llsPacketHeader.caplen;
                        packetHeader.len = (int)llsPacketHeader.len;
                                                    
                        //warning:  part of packet causes caplen < len would not be captured 
                        //          (packet actual length > maxiumBytesStoredOfEachPacket)
                        Marshal.Copy(llPacketBufferPtr, packetBuffer, 0, packetHeader.caplen);
                        return true;
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
                    onPacket(packetHeader, packetBuffer);
                }
            }
        }

        protected override void onStart()
        {   
            if(networkInterfaceName == null)
            {
                throw new PcapException("no interface set.");
            }
            StringBuilder errBuffer = new StringBuilder();
            pcapHandle = PcapApiWrapper.pcap_open(
                networkInterfaceName, 
                maxiumBytesStoredOfEachPacket,
                PCAP_OPENFLAG_PROMISCUOUS,
                waitMillisecondsForReadingAfterPacketArrived,
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

        protected override void onStop()
        {
            if (PcapApiWrapper.isNotNullPtr(pcapHandle))
            {
                PcapApiWrapper.pcap_close(pcapHandle);
                pcapHandle = IntPtr.Zero;
            }
        }
    }
}

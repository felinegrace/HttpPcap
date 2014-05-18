using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amber.Kit.HttpPcap.Common
{
    abstract class PacketPollerBase : PollingThread
    {
        public string ipAddress { get; set; }
        public Action<Descriptor> onPacket { get; set; }
        protected Descriptor descriptor { get; set; }

        protected const int maxiumBytesStoredOfEachPacket = 256 * 256;

        public PacketPollerBase(string ipAddress, Action<Descriptor> onPacket)
        {
            this.ipAddress = ipAddress;
            this.onPacket = onPacket;
        }
    }
}

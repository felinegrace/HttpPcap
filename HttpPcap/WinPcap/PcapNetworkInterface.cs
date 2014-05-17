using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amber.Kit.HttpPcap.WinPcap
{
    class PcapNetworkInterface
    {

        public string name { get; set; }
        public string description { get; set; }
        public List<string> address { get; set; }

        public PcapNetworkInterface()
        {
            address = new List<string>();
        }


    }
}

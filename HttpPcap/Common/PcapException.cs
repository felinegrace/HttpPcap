using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amber.Kit.HttpPcap.Common
{
    class PcapException : Exception
    {
        public PcapException(string message) : base(message)
        {

        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amber.Kit.HttpPcap.CommonObject
{
    public class CommonConfig
    {
        public static bool isusepcap = true;
        public static bool isdebug = false;
        public static int interfaceSelected = 0;
        public static int[] ports;
        public static string filtedomain;
        public static bool iswhiteport(int port)
        {

            for (int i = 0; i < ports.Length; i++)
            {
                if (port == ports[i])
                    return true;
            }
            return false;
        }
    }
}

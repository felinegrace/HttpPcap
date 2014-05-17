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

        public static int[] serverPorts;
        public static int[] clientPorts;

        public static bool isRequest(int sourcePort, int destPort)
        {
            bool isClient = false;
            bool isServer = false;
            if (clientPorts.Length == 0)
                isClient = true;
            if (clientPorts.Contains<int>(sourcePort))
                isClient = true;
            if (serverPorts.Length == 0)
                isServer = true;
            if (serverPorts.Contains<int>(destPort))
                isServer = true;
            return isClient && isServer;
        }

        public static bool isResponse(int sourcePort, int destPort)
        {
            bool isClient = false;
            bool isServer = false;
            if (clientPorts.Length == 0)
                isClient = true;
            if (clientPorts.Contains<int>(destPort))
                isClient = true;
            if (serverPorts.Length == 0)
                isServer = true;
            if (serverPorts.Contains<int>(sourcePort))
                isServer = true;
            return isClient && isServer;
        }
    }
}

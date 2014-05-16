using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amber.Kit.HttpPcap;
using Cabinet.Utility;

namespace Demo_HttpPcapConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Logger.enable();
            Entry e = new Entry();
            e.InitUiParms();
            e.button1_Click(true, "www.baidu.com", "80", "10.31.31.31");
            while(true)
            {

            }
        }
    }
}

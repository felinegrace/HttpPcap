using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace Demo_HttpPcapConsole
{
    public class Logger
    {
        static int enabled = -1;
        //static object mutex = new object();

        //must call in main thread
        public static void enable()
        {
            if (enabled == -1)
                log4net.Config.XmlConfigurator.Configure(new System.IO.FileInfo(@"Config\log4net.config"));
            enabled = 1;
        }

        public static void disable()
        {
            enabled = 0;
        }

        
        public static log4net.ILog getLogger()
        {
            /*
            if (!configured)
            {
                lock (mutex)
                {
                    if (!configured)
                    {
                        configured = true;
                        configure();
                    }
                }
            }
            */
            return log4net.LogManager.GetLogger("general");
        }
        

        public static void debug(string format , params object[] args)
        {
            if (enabled == 1)
                getLogger().DebugFormat(format, args);
        }

        public static void info(string format, params object[] args)
        {
            if (enabled == 1)
                getLogger().InfoFormat(format, args);
        }

        public static void error(string format, params object[] args)
        {
            if (enabled == 1)
                getLogger().ErrorFormat(format, args);
        }

        public static string logObjectList(List<object> list)
        {
            try
            {
                return JsonConvert.SerializeObject(list);
            }
            catch(Exception ex)
            {
                return String.Format("[paring error: {0}]", ex.Message);
            }
            
        }
    }
}



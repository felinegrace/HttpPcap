using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cabinet.Utility
{
    public class Logger4LINQ : System.IO.TextWriter
    {
        //private string buffer;
        private Encoding encoding;
        public override Encoding Encoding
        {
            get
            {
                if (encoding == null)
                {
                    encoding = new UnicodeEncoding(false, false);
                }
                return encoding;
            }
        }

        public Logger4LINQ()
        {
            //buffer = string.Empty;
        }


        public override void Write(string value)
        {
            //buffer += value;
            //if (buffer.EndsWith("\r\n"))
            //{
            //    commitAndClear();
            //}

            Logger.debug("LINQ: {0}.", value);
        }

        //private void commitAndClear()
        //{
        //    Logger.debug("LINQ: {0}.",buffer);
        //    buffer = string.Empty;
        //}

        public override void Write(char[] buffer, int index, int count)
        {
            Write(new string(buffer, index, count));
        }
    }

}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amber.Kit.HttpPcap.Common
{
    class BytesHelper
    {
        public static ushort bytes2ushort(byte[] source, int start, bool isBigEndian)
        {
            if (isBigEndian)
                return (ushort)(source[start] * 0x100 + source[start + 1]);
            else
                return (ushort)(source[start + 1] * 0x100 + source[start]);

        }

    }
}

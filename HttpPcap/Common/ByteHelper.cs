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

        public static IEnumerable<int> indexOf(IEnumerable<byte> source, int start, int count, byte[] pattern)
        {
            IEnumerable<int> index = Enumerable.Range(start, count - pattern.Length + 1);
            for (int i = 0; i < pattern.Length; i++)
            {
                index = index.Where(n => source.ElementAt(n + i) == pattern[i]).ToArray();
            }
            return index;
        }

        public static bool isEndWith(IEnumerable<byte> source, byte[] pattern)
        {
            int sourceCount = source.Count<byte>();
            int patternCount = pattern.Length;
            int sourceStartAt = sourceCount - pattern.Length;
            if (sourceStartAt < 0)
            {
                return false;
            }
            for(int i = 0 ; i < patternCount ; i++)
            {
                if(source.ElementAt<byte>(sourceStartAt + i) != pattern[i])
                {
                    return false;
                }
            }
            return true;
        }
    }
}

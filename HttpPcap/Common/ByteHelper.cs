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
            for (int i = start; i < start + count; i++)
            {
                int j = 0;
                for (; j < pattern.Length; j++)
                {
                    if (source.ElementAt<byte>(i + j) != pattern[j])
                    {
                        break;
                    }
                    else
                    {
                        int a = 1;
                    }
                }
                if (j == pattern.Length)
                {
                    yield return i;
                    i += j - 1;
                }
            }
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

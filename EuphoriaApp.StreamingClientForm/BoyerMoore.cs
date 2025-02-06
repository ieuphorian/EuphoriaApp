using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EuphoriaApp.StreamingClientForm
{
    public class BoyerMoore
    {
        public unsafe List<int> SearchAll(byte[] searchArray, byte[] pattern, int startIndex = 0)
        {
            var jumpTable = new int[256];
            var patternLength = pattern.Length;
            for (var jIndex = 0; jIndex < 256; jIndex++)
                jumpTable[jIndex] = patternLength;
            for (var jIndex = 0; jIndex < patternLength - 1; jIndex++)
                jumpTable[pattern[jIndex]] = patternLength - jIndex - 1;

            var index = startIndex;
            var limit = searchArray.Length - patternLength;
            var patternLengthMinusOne = patternLength - 1;
            var list = new List<int>();
            fixed (byte* pointerToByteArray = searchArray)
            {
                var pointerToByteArrayStartingIndex = pointerToByteArray + startIndex;
                fixed (byte* pointerToPattern = pattern)
                {
                    while (index <= limit)
                    {
                        var j = patternLengthMinusOne;
                        while (j >= 0 && pointerToPattern[j] == pointerToByteArrayStartingIndex[index + j])
                            j--;
                        if (j < 0)
                            list.Add(index);
                        index += Math.Max(jumpTable[pointerToByteArrayStartingIndex[index + j]] - patternLength + 1 + j, 1);
                    }
                }
            }

            return list;
        }
    }
}

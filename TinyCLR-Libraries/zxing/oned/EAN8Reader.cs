/*
 * Copyright 2008 ZXing authors
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System.Text;
using ZXing.Common;

namespace ZXing.OneD
{
   /// <summary>
   /// <p>Implements decoding of the EAN-8 format.</p>
   ///
   /// <author>Sean Owen</author>
   /// </summary>
   public sealed class EAN8Reader : UPCEANReader
   {
      private int[] decodeMiddleCounters;

      public EAN8Reader()
      {
         decodeMiddleCounters = new int[4];
      }

      override protected internal int decodeMiddle(BitArray row,
                                 int[] startRange,
                                 StringBuilder result)
      {
         int[] counters = decodeMiddleCounters;
         counters[0] = 0;
         counters[1] = 0;
         counters[2] = 0;
         counters[3] = 0;
         int end = row.Size;
         int rowOffset = startRange[1];

         for (int x = 0; x < 4 && rowOffset < end; x++)
         {
            int bestMatch;
            if (!decodeDigit(row, counters, rowOffset, L_PATTERNS, out bestMatch))
               return -1;
            result.Append((char)('0' + bestMatch));
            foreach (int counter in counters)
            {
               rowOffset += counter;
            }
         }

         int[] middleRange = findGuardPattern(row, rowOffset, true, MIDDLE_PATTERN);
         if (middleRange == null)
            return -1;
         rowOffset = middleRange[1];

         for (int x = 0; x < 4 && rowOffset < end; x++)
         {
            int bestMatch;
            if (!decodeDigit(row, counters, rowOffset, L_PATTERNS, out bestMatch))
               return -1;
            result.Append((char)('0' + bestMatch));
            foreach (int counter in counters)
            {
               rowOffset += counter;
            }
         }

         return rowOffset;
      }

      override internal BarcodeFormat BarcodeFormat
      {
         get { return BarcodeFormat.EAN_8; }
      }
   }
}

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


using System.Collections;
using ZXing.Common;
using ZXing.OneD.RSS;
using ZXing.OneD.RSS.Expanded;

namespace ZXing.OneD
{
   /// <summary>
   /// <author>dswitkin@google.com (Daniel Switkin)</author>
   /// <author>Sean Owen</author>
   /// </summary>
   public sealed class MultiFormatOneDReader : OneDReader
   {
      private readonly ArrayList readers;

      public MultiFormatOneDReader(Hashtable hints)
      {
         var possibleFormats = hints == null || !hints.Contains(DecodeHintType.POSSIBLE_FORMATS) ? null :
             (ArrayList)hints[DecodeHintType.POSSIBLE_FORMATS];
         bool useCode39CheckDigit = hints != null && hints.Contains(DecodeHintType.ASSUME_CODE_39_CHECK_DIGIT) &&
             hints[DecodeHintType.ASSUME_CODE_39_CHECK_DIGIT] != null;
         this.readers = new ArrayList();
         if (possibleFormats != null)
         {
            if (possibleFormats.Contains(BarcodeFormat.EAN_13) ||
                possibleFormats.Contains(BarcodeFormat.UPC_A) ||
                possibleFormats.Contains(BarcodeFormat.EAN_8) ||
                possibleFormats.Contains(BarcodeFormat.UPC_E))
            {
               readers.Add(new MultiFormatUPCEANReader(hints));
            }
            if (possibleFormats.Contains(BarcodeFormat.CODE_39))
            {
               readers.Add(new Code39Reader(useCode39CheckDigit));
            }
            if (possibleFormats.Contains(BarcodeFormat.CODE_93))
            {
               readers.Add(new Code93Reader());
            }
            if (possibleFormats.Contains(BarcodeFormat.CODE_128))
            {
               readers.Add(new Code128Reader());
            }
            if (possibleFormats.Contains(BarcodeFormat.ITF))
            {
               readers.Add(new ITFReader());
            }
            if (possibleFormats.Contains(BarcodeFormat.CODABAR))
            {
               readers.Add(new CodaBarReader());
            }
            if (possibleFormats.Contains(BarcodeFormat.RSS_14))
            {
               readers.Add(new RSS14Reader());
            }
            if (possibleFormats.Contains(BarcodeFormat.RSS_EXPANDED))
            {
               readers.Add(new RSSExpandedReader());
            }
         }
         if (readers.Count == 0)
         {
            readers.Add(new MultiFormatUPCEANReader(hints));
            readers.Add(new Code39Reader());
            readers.Add(new CodaBarReader());
            readers.Add(new Code93Reader());
            readers.Add(new Code128Reader());
            readers.Add(new ITFReader());
            readers.Add(new RSS14Reader());
            readers.Add(new RSSExpandedReader());
         }
      }

      override public Result decodeRow(int rowNumber,
                              BitArray row,
                              Hashtable hints)
      {
         foreach (OneDReader reader in readers)
         {
            var result = reader.decodeRow(rowNumber, row, hints);
            if (result != null)
               return result;
         }

         return null;
      }

      public override void reset()
      {
         foreach (Reader reader in readers)
         {
            reader.reset();
         }
      }
   }
}
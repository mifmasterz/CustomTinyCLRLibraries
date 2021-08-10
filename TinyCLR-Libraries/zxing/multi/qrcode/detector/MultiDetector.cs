/*
 * Copyright 2009 ZXing authors
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
using ZXing.QrCode.Internal;

namespace ZXing.Multi.QrCode.Internal
{
   /// <summary>
   /// <p>Encapsulates logic that can detect one or more QR Codes in an image, even if the QR Code
   /// is rotated or skewed, or partially obscured.</p>
   ///
   /// <author>Sean Owen</author>
   /// <author>Hannes Erven</author>
   /// </summary>
   public sealed class MultiDetector : Detector
   {
      private static readonly DetectorResult[] EMPTY_DETECTOR_RESULTS = new DetectorResult[0];

      public MultiDetector(BitMatrix image)
         : base(image)
      {
      }

      public DetectorResult[] detectMulti(Hashtable hints)
      {
         BitMatrix image = Image;
         ResultPointCallback resultPointCallback =
             hints == null || !hints.Contains(DecodeHintType.NEED_RESULT_POINT_CALLBACK) ? null : (ResultPointCallback)hints[DecodeHintType.NEED_RESULT_POINT_CALLBACK];
         MultiFinderPatternFinder finder = new MultiFinderPatternFinder(image, resultPointCallback);
         FinderPatternInfo[] infos = finder.findMulti(hints);

         if (infos.Length == 0)
         {
            throw NotFoundException.Instance;
         }

         var result = new ArrayList();
         foreach (FinderPatternInfo info in infos)
         {
            var oneResult = processFinderPatternInfo(info);
            if (oneResult != null)
               result.Add(oneResult);
         }
         if (result.Count == 0)
         {
            return EMPTY_DETECTOR_RESULTS;
         }
         else
         {
            return (DetectorResult[])result.ToArray();
         }
      }
   }
}

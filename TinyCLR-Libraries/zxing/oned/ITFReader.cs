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

using System;
using System.Collections;
using System.Text;
using ZXing.Common;

namespace ZXing.OneD
{
   /// <summary>
   /// <p>Implements decoding of the ITF format, or Interleaved Two of Five.</p>
   ///
   /// <p>This Reader will scan ITF barcodes of certain lengths only.
   /// At the moment it reads length 6, 10, 12, 14, 16, 24, and 44 as these have appeared "in the wild". Not all
   /// lengths are scanned, especially shorter ones, to avoid false positives. This in turn is due to a lack of
   /// required checksum function.</p>
   ///
   /// <p>The checksum is optional and is not applied by this Reader. The consumer of the decoded
   /// value will have to apply a checksum if required.</p>
   ///
   /// <p><a href="http://en.wikipedia.org/wiki/Interleaved_2_of_5">http://en.wikipedia.org/wiki/Interleaved_2_of_5</a>
   /// is a great reference for Interleaved 2 of 5 information.</p>
   ///
   /// <author>kevin.osullivan@sita.aero, SITA Lab.</author>
   /// </summary>
   public sealed class ITFReader : OneDReader
   {
      private static readonly int MAX_AVG_VARIANCE = (int)(PATTERN_MATCH_RESULT_SCALE_FACTOR * 0.42f);
      private static readonly int MAX_INDIVIDUAL_VARIANCE = (int)(PATTERN_MATCH_RESULT_SCALE_FACTOR * 0.8f);

      private const int W = 3; // Pixel width of a wide line
      private const int N = 1; // Pixed width of a narrow line

      private static readonly int[] DEFAULT_ALLOWED_LENGTHS = { 44, 24, 20, 18, 16, 14, 12, 10, 8, 6 };

      // Stores the actual narrow line width of the image being decoded.
      private int narrowLineWidth = -1;

      /// <summary>
      /// Start/end guard pattern.
      ///
      /// Note: The end pattern is reversed because the row is reversed before
      /// searching for the END_PATTERN
      /// </summary>
      private static readonly int[] START_PATTERN = { N, N, N, N };
      private static readonly int[] END_PATTERN_REVERSED = { N, N, W };

      /// <summary>
      /// Patterns of Wide / Narrow lines to indicate each digit
      /// </summary>
      internal static int[][] PATTERNS = new int[][]
                                           {
                                              new int[] {N, N, W, W, N}, // 0
                                              new int[] {W, N, N, N, W}, // 1
                                              new int[] {N, W, N, N, W}, // 2
                                              new int[] {W, W, N, N, N}, // 3
                                              new int[] {N, N, W, N, W}, // 4
                                              new int[] {W, N, W, N, N}, // 5
                                              new int[] {N, W, W, N, N}, // 6
                                              new int[] {N, N, N, W, W}, // 7
                                              new int[] {W, N, N, W, N}, // 8
                                              new int[] {N, W, N, W, N} // 9
                                           };

      override public Result decodeRow(int rowNumber, BitArray row, Hashtable hints)
      {
         // Find out where the Middle section (payload) starts & ends
         int[] startRange = decodeStart(row);
         if (startRange == null)
            return null;

         int[] endRange = decodeEnd(row);
         if (endRange == null)
            return null;

         StringBuilder result = new StringBuilder(20);
         if (!decodeMiddle(row, startRange[1], endRange[0], result))
            return null;

         String resultString = result.ToString();

         int[] allowedLengths = null;
         if (hints != null && hints.Contains(DecodeHintType.ALLOWED_LENGTHS))
         {
            allowedLengths = (int[])hints[DecodeHintType.ALLOWED_LENGTHS];

         }
         if (allowedLengths == null)
         {
            allowedLengths = DEFAULT_ALLOWED_LENGTHS;
         }

         // To avoid false positives with 2D barcodes (and other patterns), make
         // an assumption that the decoded string must be 6, 10 or 14 digits.
         int length = resultString.Length;
         bool lengthOK = false;
         foreach (int allowedLength in allowedLengths)
         {
            if (length == allowedLength)
            {
               lengthOK = true;
               break;
            }
         }
         if (!lengthOK)
         {
            return null;
         }

         return new Result(
            resultString,
            null, // no natural byte representation for these barcodes
            new ResultPoint[]
               {
                  new ResultPoint(startRange[1], (float) rowNumber),
                  new ResultPoint(endRange[0], (float) rowNumber)
               },
            BarcodeFormat.ITF);
      }

      /// <summary>
      /// <param name="row">row of black/white values to search</param>
      /// <param name="payloadStart">offset of start pattern</param>
      /// <param name="resultString"><see cref="StringBuilder" />to append decoded chars to</param>
      /// <exception cref="NotFoundException">if decoding could not complete successfully</exception>
      /// </summary>
      private static bool decodeMiddle(BitArray row,
                                       int payloadStart,
                                       int payloadEnd,
                                       StringBuilder resultString)
      {
         // Digits are interleaved in pairs - 5 black lines for one digit, and the
         // 5
         // interleaved white lines for the second digit.
         // Therefore, need to scan 10 lines and then
         // split these into two arrays
         int[] counterDigitPair = new int[10];
         int[] counterBlack = new int[5];
         int[] counterWhite = new int[5];

         while (payloadStart < payloadEnd)
         {
            // Get 10 runs of black/white.
            if (!recordPattern(row, payloadStart, counterDigitPair))
               return false;

            // Split them into each array
            for (int k = 0; k < 5; k++)
            {
               int twoK = k << 1;
               counterBlack[k] = counterDigitPair[twoK];
               counterWhite[k] = counterDigitPair[twoK + 1];
            }

            int bestMatch;
            if (!decodeDigit(counterBlack, out bestMatch))
               return false;
            resultString.Append((char)('0' + bestMatch));
            if (!decodeDigit(counterWhite, out bestMatch))
               return false;
            resultString.Append((char)('0' + bestMatch));

            foreach (int counterDigit in counterDigitPair)
            {
               payloadStart += counterDigit;
            }
         }

         return true;
      }

      /// <summary>
      /// Identify where the start of the middle / payload section starts.
      ///
      /// <param name="row">row of black/white values to search</param>
      /// <returns>Array, containing index of start of 'start block' and end of</returns>
      ///         'start block'
      /// <exception cref="NotFoundException"></exception>
      /// </summary>
      int[] decodeStart(BitArray row)
      {
         int endStart = skipWhiteSpace(row);
         if (endStart < 0)
            return null;

         int[] startPattern = findGuardPattern(row, endStart, START_PATTERN);
         if (startPattern == null)
            return null;

         // Determine the width of a narrow line in pixels. We can do this by
         // getting the width of the start pattern and dividing by 4 because its
         // made up of 4 narrow lines.
         narrowLineWidth = (startPattern[1] - startPattern[0]) >> 2;

         if (!validateQuietZone(row, startPattern[0]))
            return null;

         return startPattern;
      }

      /// <summary>
      /// The start & end patterns must be pre/post fixed by a quiet zone. This
      /// zone must be at least 10 times the width of a narrow line.  Scan back until
      /// we either get to the start of the barcode or match the necessary number of
      /// quiet zone pixels.
      ///
      /// Note: Its assumed the row is reversed when using this method to find
      /// quiet zone after the end pattern.
      ///
      /// ref: http://www.barcode-1.net/i25code.html
      ///
      /// <param name="row">bit array representing the scanned barcode.</param>
      /// <param name="startPattern">index into row of the start or end pattern.</param>
      /// <exception cref="NotFoundException">if the quiet zone cannot be found, a ReaderException is thrown.</exception>
      /// </summary>
      private bool validateQuietZone(BitArray row, int startPattern)
      {

         int quietCount = this.narrowLineWidth * 10;  // expect to find this many pixels of quiet zone

         for (int i = startPattern - 1; quietCount > 0 && i >= 0; i--)
         {
            if (row[i])
            {
               break;
            }
            quietCount--;
         }
         if (quietCount != 0)
         {
            // Unable to find the necessary number of quiet zone pixels.
            return false;
         }
         return true;
      }

      /// <summary>
      /// Skip all whitespace until we get to the first black line.
      ///
      /// <param name="row">row of black/white values to search</param>
      /// <returns>index of the first black line.</returns>
      /// <exception cref="NotFoundException">Throws exception if no black lines are found in the row</exception>
      /// </summary>
      private static int skipWhiteSpace(BitArray row)
      {
         int width = row.Size;
         int endStart = row.getNextSet(0);
         if (endStart == width)
         {
            return -1;
         }

         return endStart;
      }

      /// <summary>
      /// Identify where the end of the middle / payload section ends.
      ///
      /// <param name="row">row of black/white values to search</param>
      /// <returns>Array, containing index of start of 'end block' and end of 'end</returns>
      ///         block'
      /// <exception cref="NotFoundException"></exception>
      /// </summary>
      int[] decodeEnd(BitArray row)
      {
         // For convenience, reverse the row and then
         // search from 'the start' for the end block
         row.reverse();
         int endStart = skipWhiteSpace(row);
         if (endStart < 0)
            return null;
         int[] endPattern = findGuardPattern(row, endStart, END_PATTERN_REVERSED);
         if (endPattern == null)
         {
            row.reverse();
            return null;
         }

         // The start & end patterns must be pre/post fixed by a quiet zone. This
         // zone must be at least 10 times the width of a narrow line.
         // ref: http://www.barcode-1.net/i25code.html
         if (!validateQuietZone(row, endPattern[0]))
         {
            row.reverse();
            return null;
         }

         // Now recalculate the indices of where the 'endblock' starts & stops to
         // accommodate
         // the reversed nature of the search
         int temp = endPattern[0];
         endPattern[0] = row.Size - endPattern[1];
         endPattern[1] = row.Size - temp;

         row.reverse();
         return endPattern;
      }

      /// <summary>
      /// <param name="row">row of black/white values to search</param>
      /// <param name="rowOffset">position to start search</param>
      /// <param name="pattern">pattern of counts of number of black and white pixels that are</param>
      ///                  being searched for as a pattern
      /// <returns>start/end horizontal offset of guard pattern, as an array of two</returns>
      ///         ints
      /// <exception cref="NotFoundException">if pattern is not found</exception>
      /// </summary>
      private static int[] findGuardPattern(BitArray row,
                                            int rowOffset,
                                            int[] pattern)
      {

         // TODO: This is very similar to implementation in UPCEANReader. Consider if they can be
         // merged to a single method.
         int patternLength = pattern.Length;
         int[] counters = new int[patternLength];
         int width = row.Size;
         bool isWhite = false;

         int counterPosition = 0;
         int patternStart = rowOffset;
         for (int x = rowOffset; x < width; x++)
         {
            if (row[x] ^ isWhite)
            {
               counters[counterPosition]++;
            }
            else
            {
               if (counterPosition == patternLength - 1)
               {
                  if (patternMatchVariance(counters, pattern, MAX_INDIVIDUAL_VARIANCE) < MAX_AVG_VARIANCE)
                  {
                     return new int[] { patternStart, x };
                  }
                  patternStart += counters[0] + counters[1];
                  Array.Copy(counters, 2, counters, 0, patternLength - 2);
                  counters[patternLength - 2] = 0;
                  counters[patternLength - 1] = 0;
                  counterPosition--;
               }
               else
               {
                  counterPosition++;
               }
               counters[counterPosition] = 1;
               isWhite = !isWhite;
            }
         }
         return null;
      }

      /// <summary>
      /// Attempts to decode a sequence of ITF black/white lines into single
      /// digit.
      ///
      /// <param name="counters">the counts of runs of observed black/white/black/... values</param>
      /// <returns>The decoded digit</returns>
      /// <exception cref="NotFoundException">if digit cannot be decoded</exception>
      /// </summary>
      private static bool decodeDigit(int[] counters, out int bestMatch)
      {
         int bestVariance = MAX_AVG_VARIANCE; // worst variance we'll accept
         bestMatch = -1;
         int max = PATTERNS.Length;
         for (int i = 0; i < max; i++)
         {
            int[] pattern = PATTERNS[i];
            int variance = patternMatchVariance(counters, pattern, MAX_INDIVIDUAL_VARIANCE);
            if (variance < bestVariance)
            {
               bestVariance = variance;
               bestMatch = i;
            }
         }
         return bestMatch >= 0;
      }
   }
}
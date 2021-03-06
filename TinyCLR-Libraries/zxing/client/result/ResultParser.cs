/*
* Copyright 2007 ZXing authors
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
using System.Text.RegularExpressions;

namespace ZXing.Client.Result
{

   /// <summary> <p>Abstract class representing the result of decoding a barcode, as more than
   /// a String -- as some type of structured data. This might be a subclass which represents
   /// a URL, or an e-mail address. {@link #parseResult(com.google.zxing.Result)} will turn a raw
   /// decoded string into the most appropriate type of structured representation.</p>
   /// 
   /// <p>Thanks to Jeff Griffin for proposing rewrite of these classes that relies less
   /// on exception-based mechanisms during parsing.</p>
   /// 
   /// </summary>
   /// <author>  Sean Owen
   /// </author>
   /// <author>www.Redivivus.in (suraj.supekar@redivivus.in) - Ported from ZXING Java Source 
   /// </author>
   public abstract class ResultParser
   {
      private static ResultParser[] PARSERS = {
                                                 new BookmarkDoCoMoResultParser(),
                                                 new AddressBookDoCoMoResultParser(),
                                                 new EmailDoCoMoResultParser(),
                                                 new AddressBookAUResultParser(),
                                                 new VCardResultParser(),
                                                 new BizcardResultParser(),
                                                 new VEventResultParser(),
                                                 new EmailAddressResultParser(),
                                                 new SMTPResultParser(),
                                                 new TelResultParser(),
                                                 new SMSMMSResultParser(),
                                                 new SMSTOMMSTOResultParser(),
                                                 new GeoResultParser(),
                                                 new WifiResultParser(),
                                                 new URLTOResultParser(),
                                                 new URIResultParser(),
                                                 new ISBNResultParser(),
                                                 new ProductResultParser(),
                                                 new ExpandedProductResultParser()
                                              };

#if SILVERLIGHT4 || SILVERLIGHT5
      private static readonly Regex DIGITS = new Regex("\\d*");
      private static readonly Regex ALPHANUM = new Regex("[a-zA-Z0-9]*");
      private static readonly Regex AMPERSAND = new Regex("&");
      private static readonly Regex EQUALS = new Regex("=");
#else
      private static readonly Regex DIGITS = new Regex("\\d*", RegexOptions.Compiled);
      private static readonly Regex ALPHANUM = new Regex("[a-zA-Z0-9]*", RegexOptions.Compiled);
      private static readonly Regex AMPERSAND = new Regex("&", RegexOptions.Compiled);
      private static readonly Regex EQUALS = new Regex("=", RegexOptions.Compiled);
#endif

      /// <summary>
      /// Attempts to parse the raw {@link Result}'s contents as a particular type
      /// of information (email, URL, etc.) and return a {@link ParsedResult} encapsulating
      /// the result of parsing.
      /// </summary>
      /// <param name="theResult">The result.</param>
      /// <returns></returns>
      public abstract ParsedResult parse(ZXing.Result theResult);

      public static ParsedResult parseResult(ZXing.Result theResult)
      {
         foreach (var parser in PARSERS)
         {
            var result = parser.parse(theResult);
            if (result != null)
            {
               return result;
            }
         }
         return new TextParsedResult(theResult.Text, null);
      }

      protected static void maybeAppend(String value_Renamed, System.Text.StringBuilder result)
      {
         if (value_Renamed != null)
         {
            result.Append('\n');
            result.Append(value_Renamed);
         }
      }

      protected static void maybeAppend(String[] value_Renamed, System.Text.StringBuilder result)
      {
         if (value_Renamed != null)
         {
            for (int i = 0; i < value_Renamed.Length; i++)
            {
               result.Append('\n');
               result.Append(value_Renamed[i]);
            }
         }
      }

      protected static String[] maybeWrap(System.String value_Renamed)
      {
         return value_Renamed == null ? null : new System.String[] { value_Renamed };
      }

      protected static String unescapeBackslash(System.String escaped)
      {
         if (escaped != null)
         {
            int backslash = escaped.IndexOf('\\');
            if (backslash >= 0)
            {
               int max = escaped.Length;
               System.Text.StringBuilder unescaped = new System.Text.StringBuilder(max - 1);
               unescaped.Append(escaped.ToCharArray(), 0, backslash);
               bool nextIsEscaped = false;
               for (int i = backslash; i < max; i++)
               {
                  char c = escaped[i];
                  if (nextIsEscaped || c != '\\')
                  {
                     unescaped.Append(c);
                     nextIsEscaped = false;
                  }
                  else
                  {
                     nextIsEscaped = true;
                  }
               }
               return unescaped.ToString();
            }
         }
         return escaped;
      }

      protected static int parseHexDigit(char c)
      {
         if (c >= 'a')
         {
            if (c <= 'f')
            {
               return 10 + (c - 'a');
            }
         }
         else if (c >= 'A')
         {
            if (c <= 'F')
            {
               return 10 + (c - 'A');
            }
         }
         else if (c >= '0')
         {
            if (c <= '9')
            {
               return c - '0';
            }
         }
         return -1;
      }

      protected static bool isStringOfDigits(String value, int length)
      {
         return value != null && length == value.Length && DIGITS.Match(value).Success;
      }

      protected static bool isSubstringOfDigits(String value, int offset, int length)
      {
         if (value == null)
         {
            return false;
         }
         int max = offset + length;
         return value.Length >= max && DIGITS.Match(value.Substring(offset, length)).Success;
      }

      protected static bool isSubstringOfAlphaNumeric(String value, int offset, int length)
      {
         if (value == null)
         {
            return false;
         }
         int max = offset + length;
         return value.Length >= max && ALPHANUM.Match(value.Substring(offset, length)).Success;
      }

      internal static Hashtable parseNameValuePairs(String uri)
      {
         int paramStart = uri.IndexOf('?');
         if (paramStart < 0)
         {
            return null;
         }
         var result = new Hashtable(3);
         foreach (var keyValue in AMPERSAND.Split(uri.Substring(paramStart + 1)))
         {
            appendKeyValue(keyValue, result);
         }
         return result;
      }

      private static void appendKeyValue(String keyValue,
                                         Hashtable result)
      {
         String[] keyValueTokens = EQUALS.Split(keyValue, 2);
         if (keyValueTokens.Length == 2)
         {
            String key = keyValueTokens[0];
            String value = keyValueTokens[1];
            try
            {
               //value = URLDecoder.decode(value, "UTF-8");
               value = urlDecode(value);
               result[key] = value;
            }
            catch (Exception uee)
            {
               throw new InvalidOperationException("url decoding failed", uee); // can't happen
            }
            result[key] = value;
         }
      }

      internal static String[] matchPrefixedField(String prefix, String rawText, char endChar, bool trim)
      {
         IList matches = null;
         int i = 0;
         int max = rawText.Length;
         while (i < max)
         {
            //UPGRADE_WARNING: Method 'java.lang.String.indexOf' was converted to 'System.String.IndexOf' which may throw an exception. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1101'"
            i = rawText.IndexOf(prefix, i);
            if (i < 0)
            {
               break;
            }
            i += prefix.Length; // Skip past this prefix we found to start
            int start = i; // Found the start of a match here
            bool done = false;
            while (!done)
            {
               //UPGRADE_WARNING: Method 'java.lang.String.indexOf' was converted to 'System.String.IndexOf' which may throw an exception. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1101'"
               i = rawText.IndexOf(endChar, i);
               if (i < 0)
               {
                  // No terminating end character? uh, done. Set i such that loop terminates and break
                  i = rawText.Length;
                  done = true;
               }
               else if (rawText[i - 1] == '\\')
               {
                  // semicolon was escaped so continue
                  i++;
               }
               else
               {
                  // found a match
                  if (matches == null)
                  {
                     matches = new ArrayList();
                  }
                  String element = unescapeBackslash(rawText.Substring(start, (i) - (start)));
                  if (trim)
                  {
                     element = element.Trim();
                  }
                  matches.Add(element);
                  i++;
                  done = true;
               }
            }
         }
         if (matches == null || (matches.Count == 0))
         {
            return null;
         }
         return SupportClass.toStringArray(matches);
      }

      internal static String matchSinglePrefixedField(System.String prefix, System.String rawText, char endChar, bool trim)
      {
         String[] matches = matchPrefixedField(prefix, rawText, endChar, trim);
         return matches == null ? null : matches[0];
      }

      private static String urlDecode(String escaped)
      {
         // No we can't use java.net.URLDecoder here. JavaME doesn't have it.
         if (escaped == null)
         {
            return null;
         }
         char[] escapedArray = escaped.ToCharArray();

         int first = findFirstEscape(escapedArray);
         if (first < 0)
         {
            return escaped;
         }

         int max = escapedArray.Length;
         // final length is at most 2 less than original due to at least 1 unescaping
         var unescaped = new System.Text.StringBuilder(max - 2);
         // Can append everything up to first escape character
         unescaped.Append(escapedArray, 0, first);

         for (int i = first; i < max; i++)
         {
            char c = escapedArray[i];
            if (c == '+')
            {
               // + is translated directly into a space
               unescaped.Append(' ');
            }
            else if (c == '%')
            {
               // Are there even two more chars? if not we will just copy the escaped sequence and be done
               if (i >= max - 2)
               {
                  unescaped.Append('%'); // append that % and move on
               }
               else
               {
                  int firstDigitValue = parseHexDigit(escapedArray[++i]);
                  int secondDigitValue = parseHexDigit(escapedArray[++i]);
                  if (firstDigitValue < 0 || secondDigitValue < 0)
                  {
                     // bad digit, just move on
                     unescaped.Append('%');
                     unescaped.Append(escapedArray[i - 1]);
                     unescaped.Append(escapedArray[i]);
                  }
                  unescaped.Append((char)((firstDigitValue << 4) + secondDigitValue));
               }
            }
            else
            {
               unescaped.Append(c);
            }
         }
         return unescaped.ToString();
      }

      private static int findFirstEscape(char[] escapedArray)
      {
         int max = escapedArray.Length;
         for (int i = 0; i < max; i++)
         {
            char c = escapedArray[i];
            if (c == '+' || c == '%')
            {
               return i;
            }
         }
         return -1;
      }
   }
}
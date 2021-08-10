//
// In order to convert some functionality to Visual C#, the Java Language Conversion Assistant
// creates "support classes" that duplicate the original functionality.  
//
// Support classes replicate the functionality of the original code, but in some cases they are 
// substantially different architecturally. Although every effort is made to preserve the 
// original architecture of the application in the converted project, the user should be aware that 
// the primary goal of these support classes is to replicate functionality, and that at times 
// the architecture of the resulting solution may differ somewhat.
//

using System;
using System.Collections;
using System.Text;

namespace ZXing
{
   /// <summary>
   /// Contains conversion support elements such as classes, interfaces and static methods.
   /// </summary>
   public static class SupportClass
   {
      /// <summary>
      /// Converts an array of sbytes to an array of bytes
      /// </summary>
      /// <param name="sbyteArray">The array of sbytes to be converted</param>
      /// <returns>The new array of bytes</returns>
      public static byte[] ToByteArray(sbyte[] sbyteArray)
      {
         byte[] byteArray = null;

         if (sbyteArray != null)
         {
            byteArray = new byte[sbyteArray.Length];
            for (int index = 0; index < sbyteArray.Length; index++)
               byteArray[index] = (byte)sbyteArray[index];
         }
         return byteArray;
      }

      /// <summary>
      /// Converts a string to an array of bytes
      /// </summary>
      /// <param name="sourceString">The string to be converted</param>
      /// <returns>The new array of bytes</returns>
      public static byte[] ToByteArray(System.String sourceString)
      {
         return System.Text.Encoding.UTF8.GetBytes(sourceString);
      }

      /// <summary>
      /// Converts a array of object-type instances to a byte-type array.
      /// </summary>
      /// <param name="tempObjectArray">Array to convert.</param>
      /// <returns>An array of byte type elements.</returns>
      public static byte[] ToByteArray(System.Object[] tempObjectArray)
      {
         byte[] byteArray = null;
         if (tempObjectArray != null)
         {
            byteArray = new byte[tempObjectArray.Length];
            for (int index = 0; index < tempObjectArray.Length; index++)
               byteArray[index] = (byte)tempObjectArray[index];
         }
         return byteArray;
      }

      /*******************************/
      /// <summary>
      /// This method returns the literal value received
      /// </summary>
      /// <param name="literal">The literal to return</param>
      /// <returns>The received value</returns>
      public static long Identity(long literal)
      {
         return literal;
      }

      /// <summary>
      /// This method returns the literal value received
      /// </summary>
      /// <param name="literal">The literal to return</param>
      /// <returns>The received value</returns>
      public static ulong Identity(ulong literal)
      {
         return literal;
      }

      /// <summary>
      /// This method returns the literal value received
      /// </summary>
      /// <param name="literal">The literal to return</param>
      /// <returns>The received value</returns>
      public static float Identity(float literal)
      {
         return literal;
      }

      /// <summary>
      /// This method returns the literal value received
      /// </summary>
      /// <param name="literal">The literal to return</param>
      /// <returns>The received value</returns>
      public static double Identity(double literal)
      {
         return literal;
      }

      /*******************************/
      /// <summary>
      /// Copies an array of chars obtained from a String into a specified array of chars
      /// </summary>
      /// <param name="sourceString">The String to get the chars from</param>
      /// <param name="sourceStart">Position of the String to start getting the chars</param>
      /// <param name="sourceEnd">Position of the String to end getting the chars</param>
      /// <param name="destinationArray">Array to return the chars</param>
      /// <param name="destinationStart">Position of the destination array of chars to start storing the chars</param>
      /// <returns>An array of chars</returns>
      public static void GetCharsFromString(System.String sourceString, int sourceStart, int sourceEnd, char[] destinationArray, int destinationStart)
      {
         int sourceCounter = sourceStart;
         int destinationCounter = destinationStart;
         while (sourceCounter < sourceEnd)
         {
            destinationArray[destinationCounter] = (char)sourceString[sourceCounter];
            sourceCounter++;
            destinationCounter++;
         }
      }

      ///*******************************/
      ///// <summary>
      ///// Sets the capacity for the specified List
      ///// </summary>
      ///// <param name="vector">The List which capacity will be set</param>
      ///// <param name="newCapacity">The new capacity value</param>
      //public static void SetCapacity<T>(System.Collections.Generic.IList<T> vector, int newCapacity) where T : new()
      //{
      //   while (newCapacity > vector.Count)
      //      vector.Add(new T());
      //   while (newCapacity < vector.Count)
      //      vector.RemoveAt(vector.Count - 1);
      //}


      /*******************************/
      /// <summary>
      /// Receives a byte array and returns it transformed in an sbyte array
      /// </summary>
      /// <param name="byteArray">Byte array to process</param>
      /// <returns>The transformed array</returns>
      public static sbyte[] ToSByteArray(byte[] byteArray)
      {
         sbyte[] sbyteArray = null;
         if (byteArray != null)
         {
            sbyteArray = new sbyte[byteArray.Length];
            for (int index = 0; index < byteArray.Length; index++)
               sbyteArray[index] = (sbyte)byteArray[index];
         }
         return sbyteArray;
      }

      public static String[] toStringArray(IList strings)
      {
         var result = new String[strings.Count];
         strings.CopyTo(result, 0);
         return result;
      }

      public static bool StartsWith(this string val, string start)
      {
         if (val.Length < start.Length)
            return false;
         var firstPart = val.Substring(0, start.Length);
         return String.Compare(firstPart, start) == 0;
      }

      public static void Sort(this IList list, IComparer comparer)
      {
         if (list.Count < 2)
            return;
         for (int index = 0; list.Count - 1 < index; index++)
         {
            var left = list[index];
            var right = list[index + 1];
            if (comparer.Compare(left, right) > 0)
            {
               list.RemoveAt(index + 1);
               list.Insert(index, right);
               index = -1;
            }
         }
      }

      public static ArrayList GetRange(this ArrayList list, int start, int length)
      {
         var result = new ArrayList();
         for (var index = start; index < start + length && index < list.Count; index++)
         {
            result.Add(list[index]);
         }
         return result;
      }

      //public static string Join<T>(string separator, IEnumerable<T> values)
      //{
      //   var builder = new StringBuilder();
      //   separator = separator ?? String.Empty;
      //   if (values != null)
      //   {
      //      foreach (var value in values)
      //      {
      //         builder.Append(value);
      //         builder.Append(separator);
      //      }
      //      if (builder.Length > 0)
      //         builder.Length -= separator.Length;
      //   }

      //   return builder.ToString();
      //}
   }
}
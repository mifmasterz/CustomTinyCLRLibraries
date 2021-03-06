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
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
//using System.Text.RegularExpressions;

namespace ZXing.Client.Result
{
   ///<author>Sean Owen</author>
   public sealed class CalendarParsedResult : ParsedResult
   {
      private static readonly Regex DATE_TIME = new Regex("[0-9]{8}(T[0-9]{6}Z?)?"
#if !(SILVERLIGHT4 || SILVERLIGHT5)
, RegexOptions.Compiled);
#else
);
#endif

      private const string DATE_FORMAT = "yyyyMMdd";

      //static CalendarParsedResult()
      //{
      //   // For dates without a time, for purposes of interacting with Android, the resulting timestamp
      //   // needs to be midnight of that day in GMT. See:
      //   // http://code.google.com/p/android/issues/detail?id=8330
      //   DATE_FORMAT.setTimeZone(TimeZone.getTimeZone("GMT"));
      //}

      private const string DATE_TIME_FORMAT = "yyyyMMdd'T'HHmmss";

      private readonly String summary;
      private readonly DateTime start;
      private readonly bool startAllDay;
      private readonly NullableDateTime end;
      private readonly bool endAllDay;
      private readonly String location;
      private readonly String organizer;
      private readonly String[] attendees;
      private readonly String description;
      private readonly double latitude;
      private readonly double longitude;

      public CalendarParsedResult(String summary,
                                  String startString,
                                  String endString,
                                  String location,
                                  String organizer,
                                  String[] attendees,
                                  String description,
                                  double latitude,
                                  double longitude)
         : base(ParsedResultType.CALENDAR)
      {
         this.summary = summary;
         try
         {
            this.start = parseDate(startString).Value;
            this.end = endString == null ? new NullableDateTime() : parseDate(endString);
         }
         catch (Exception pe)
         {
            throw new ArgumentException(pe.ToString());
         }
         this.startAllDay = startString.Length == 8;
         this.endAllDay = endString != null && endString.Length == 8;
         this.location = location;
         this.organizer = organizer;
         this.attendees = attendees;
         this.description = description;
         this.latitude = latitude;
         this.longitude = longitude;
      }

      public String Summary
      {
         get { return summary; }
      }

      /// <summary>
      /// Gets the start.
      /// </summary>
      public DateTime Start
      {
         get { return start; }
      }

      /// <summary>
      /// Determines whether [is start all day].
      /// </summary>
      /// <returns>if start time was specified as a whole day</returns>
      public bool isStartAllDay()
      {
         return startAllDay;
      }

      /// <summary>
      /// May return null if the event has no duration.
      /// </summary>
      public NullableDateTime End
      {
         get { return end; }
      }

      /// <summary>
      /// Gets a value indicating whether this instance is end all day.
      /// </summary>
      /// <value>true if end time was specified as a whole day</value>
      public bool isEndAllDay
      {
         get { return endAllDay; }
      }

      public String Location
      {
         get { return location; }
      }

      public String Organizer
      {
         get { return organizer; }
      }

      public String[] Attendees
      {
         get { return attendees; }
      }

      public String Description
      {
         get { return description; }
      }

      public double Latitude
      {
         get { return latitude; }
      }

      public double Longitude
      {
         get { return longitude; }
      }

      public override String DisplayResult
      {
         get
         {
            var result = new StringBuilder(100);
            maybeAppend(summary, result);
            maybeAppend(format(startAllDay, new NullableDateTime(start)), result);
            maybeAppend(format(endAllDay, end), result);
            maybeAppend(location, result);
            maybeAppend(organizer, result);
            maybeAppend(attendees, result);
            maybeAppend(description, result);
            return result.ToString();
         }
      }

      /// <summary>
      /// Parses a string as a date. RFC 2445 allows the start and end fields to be of type DATE (e.g. 20081021)
      /// or DATE-TIME (e.g. 20081021T123000 for local time, or 20081021T123000Z for UTC).
      /// </summary>
      /// <param name="when">The string to parse</param>
      /// <returns></returns>
      /// <exception cref="ArgumentException">if not a date formatted string</exception>
      private static NullableDateTime parseDate(String when)
      {
         if (!DATE_TIME.Match(when).Success)
         {
            throw new ArgumentException("no date format: " + when);
         }
         if (when.Length == 8)
         {
            // Show only year/month/day
            return new NullableDateTime(ParseExactDate(when));
         }
         else
         {
            // The when string can be local time, or UTC if it ends with a Z
            DateTime date;
            if (when.Length == 16 && when[15] == 'Z')
            {
               date = ParseExactDateTime(when.Substring(0, 15));
               // date = TimeZoneInfo.ConvertTime(date, TimeZoneInfo.Local);
            }
            else
            {
               date = ParseExactDateTime(when);
            }
            return new NullableDateTime(date);
         }
      }

      private static DateTime ParseExactDate(string dateString)
      {
         return new DateTime(
            int.Parse(dateString.Substring(0, 4)),
            int.Parse(dateString.Substring(4, 2)),
            int.Parse(dateString.Substring(6, 2)));
      }

      private static DateTime ParseExactDateTime(string dateString)
      {
         return new DateTime(
            int.Parse(dateString.Substring(0, 4)),
            int.Parse(dateString.Substring(4, 2)),
            int.Parse(dateString.Substring(6, 2)),
            int.Parse(dateString.Substring(9, 2)),
            int.Parse(dateString.Substring(11, 2)),
            int.Parse(dateString.Substring(13, 2))
            );
      }

      private static String format(bool allDay, NullableDateTime date)
      {
         if (date == null)
         {
            return null;
         }
         if (allDay)
            return date.Value.ToString("D");
         return date.Value.ToString("F");
      }
   }
}

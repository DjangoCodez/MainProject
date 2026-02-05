using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Linq;
using System.Collections.Generic;

namespace SoftOne.Soe.Business.Util
{
    /// <summary>
    /// Class for handling time intervals
    /// 
    /// TODO: THIS CLASS IS NOT TESTED AT ALL!!!!
    /// </summary>
    public sealed class TimeChunk : IEquatable<TimeChunk>
    {
        #region Properties

        /// <summary>
        /// Interval start
        /// </summary>
        public TimeSpan Start { get; set; }

        /// <summary>
        /// Interval stop
        /// </summary>
        public TimeSpan Stop { get; set; }

        /// <summary>
        /// Interval start original
        /// </summary>
        public TimeSpan StartOriginal { get; set; }

        /// <summary>
        /// Interval stop original
        /// </summary>
        public TimeSpan StopOriginal { get; set; }

        /// <summary>
        /// Start minutes relative DATETIME_DEFAULT
        /// </summary>
        public int TotalStartMinutes
        {
            get
            {
                if (totalsAreSet)
                    return totalStartMinutes;
                else
                    return (int)Start.TotalMinutes;
            }
        }

        /// <summary>
        /// Stop minutes relative DATETIME_DEFAULT
        /// </summary>
        public int TotalStopMinutes
        {
            get
            {
                if (totalsAreSet)
                    return totalStopMinutes;
                else
                    return (int)Stop.TotalMinutes;
            }
        }

        /// <summary>
        /// If true, use the start TimeDeviationCause otherwise use the stop TimeDeviationCause
        /// </summary>
        public bool UseStartCause { get; set; }

        /// <summary>
        /// Length of interval in minutes
        /// </summary>
        public int IntervallMinutes
        {
            get
            {
                if (totalsAreSet)
                    return (TotalStopMinutes - TotalStartMinutes);
                else
                    return (int)Stop.Subtract(Start).TotalMinutes;
            }
        }

        /// <summary>
        /// True when used for evaluation of breaks
        /// </summary>
        public bool IsBreak { get; set; }

        #endregion

        #region Private Variables

        private int totalStartMinutes;
        private int totalStopMinutes;

        //When true this class has handled timeblocks past midnight.
        //When start and stop are set outside this class the value is false,
        //Therefore when setting start and stop outside this class you have to ensure that the timespans you create has to take midninght into consideration.
        //In the future we dont want this variable.
        private readonly bool totalsAreSet = false;

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor creating a TimeChunk out of a TimeSpans
        /// </summary>
        /// <param name="start">Start</param>
        /// <param name="stop">Stop</param>
        /// <param name="timeChunkOriginal">Original TimeChunk</param>
        /// <param name="useStartCause">True if is StartCause, false if StopCause</param>
        public TimeChunk(TimeSpan start, TimeSpan stop, TimeChunk timeChunkOriginal = null, bool useStartCause = true)
        {
            this.Start = start;
            this.Stop = stop;
            this.StartOriginal = timeChunkOriginal != null ? timeChunkOriginal.Start : start;
            this.StopOriginal = timeChunkOriginal != null ? timeChunkOriginal.Stop : stop;
            this.UseStartCause = useStartCause;
        }

        /// <summary>
        /// Constructor creating a TimeChunk out of a DateTimes. Ensure handle of midnight.
        /// </summary>
        /// <param name="startTime">Start</param>
        /// <param name="stopTime">Stop</param>
        /// <param name="useStartCause">True if is StartCause, false if StopCause</param>
        public TimeChunk(DateTime startTime, DateTime stopTime, bool useStartCause = true)
        {
            TimeSpan start = CalendarUtility.GetTimeSpanFromDateTime(startTime);
            TimeSpan stop = CalendarUtility.GetTimeSpanFromDateTime(stopTime);

            this.Start = start;
            this.Stop = stop;
            this.StartOriginal = start;
            this.StopOriginal = stop;
            this.UseStartCause = useStartCause;

            this.totalStartMinutes = CalendarUtility.TimeToMinutes(startTime);
            this.totalStopMinutes = CalendarUtility.TimeToMinutes(stopTime);
            this.totalsAreSet = true;
        }

        /// <summary>
        /// Constructor creating a TimeChunk out of a TimeScheduleTemplateBlock. Ensure handle of midnight.
        /// </summary>
        /// <param name="scheduleBlock">Schedule block</param>
        /// <param name="useStartCause">True if is StartCause, false if StopCause</param>
        public TimeChunk(TimeScheduleTemplateBlock templateBlock, bool useStartCause = true)
        {
            this.Start = CalendarUtility.GetTimeSpanFromDateTime(templateBlock.StartTime); //compensate for blocks on multiple days, sometimes we have a block that start past midnight
            this.Stop = CalendarUtility.GetTimeSpanFromDateTime(templateBlock.StopTime); //compensate for blocks on multiple days
            this.StartOriginal = Start;
            this.StopOriginal = Stop;
            this.UseStartCause = useStartCause;

            this.totalStartMinutes = CalendarUtility.TimeToMinutes(templateBlock.StartTime);
            this.totalStopMinutes = CalendarUtility.TimeToMinutes(templateBlock.StopTime);
            this.totalsAreSet = true;
        }

        /// <summary>
        /// Constructor creating a TimeChunk out of a TimeBlock. Ensure handle of midnight.
        /// </summary>
        /// <param name="timeBlock">Time block</param>
        /// <param name="useStartCause">True if is StartCause, false if StopCause</param>
        public TimeChunk(TimeBlock timeBlock, bool useStartCause = true)
        {
            this.Start = CalendarUtility.GetTimeSpanFromDateTime(timeBlock.StartTime); //compensate for blocks on multiple days, sometimes we have a timeblock that start past midnight
            this.Stop = CalendarUtility.GetTimeSpanFromDateTime(timeBlock.StopTime); //compensate for blocks on multiple days
            this.StartOriginal = Start;
            this.StopOriginal = Stop;
            this.UseStartCause = useStartCause;

            this.totalStartMinutes = CalendarUtility.TimeToMinutes(timeBlock.StartTime);
            this.totalStopMinutes = CalendarUtility.TimeToMinutes(timeBlock.StopTime);
            this.totalsAreSet = true;
        }

        #endregion

        #region Public methods

        public DateTime GetStartTime()
        {
            return CalendarUtility.GetDateTime(this.Start);
        }

        public DateTime GetStopTime()
        {
            return CalendarUtility.GetDateTime(this.Stop);
        }

        public void IncreaseStartTime(int minutes)
        {
            this.Start = this.Start.Add(new TimeSpan(0, minutes, 0));
            if (this.totalsAreSet)
                this.totalStartMinutes += minutes;
        }

        public void DecreaseStartTime(int minutes)
        {
            this.Start = this.Start.Subtract(new TimeSpan(0, minutes, 0));
            if (this.totalsAreSet)
                this.totalStartMinutes -= minutes;
        }

        public void IncreaseStopTime(int minutes)
        {
            this.Stop = this.Stop.Add(new TimeSpan(0, minutes, 0));
            if (this.totalsAreSet)
                this.totalStopMinutes += minutes;
        }

        public void DecreaseStopTime(int minutes)
        {
            this.Stop = this.Stop.Subtract(new TimeSpan(0, minutes, 0));
            if (this.totalsAreSet)
                this.totalStopMinutes -= minutes;
        }

        public bool EvaluateRelativeTime(bool isStartExpression, int relativeMinutes)
        {
            return isStartExpression
                ? this.StartsAfterRelativeTime(relativeMinutes)
                : this.StopsBeforeRelativeTime(relativeMinutes);
        }

        public bool StartsAfterRelativeTime(int relativeMinutes)
        {
            return this.TotalStartMinutes >= relativeMinutes;
        }

        public bool StartsBeforeRelativeTime(int relativeMinutes)
        {
            return this.TotalStartMinutes <= relativeMinutes;
        }

        public bool StopsAfterRelativeTime(int relativeMinutes)
        {
            return this.TotalStopMinutes >= relativeMinutes;
        }

        public bool StopsBeforeRelativeTime(int relativeMinutes)
        {
            return this.TotalStopMinutes <= relativeMinutes;
        }

        /// <summary>
        /// Logical or operation of two timeblocks
        /// </summary>
        /// <param name="other">The other timeblock</param>
        /// <returns>An array of timeblocks</returns>
        public TimeChunk[] Or(TimeChunk other)
        {
            if (!other.Intersect(this))
                return new TimeChunk[] { this, other };

            TimeSpan start = this.Start < other.Start ? this.Start : other.Start;
            TimeSpan stop = this.Stop > other.Stop ? this.Stop : other.Stop;

            TimeChunk result = new TimeChunk(start, stop, null, this.UseStartCause);

            return new TimeChunk[] { result };
        }

        /// <summary>
        /// Checks if two TimeChunks intersect
        /// </summary>
        /// <param name="other">The other timeblock</param>
        /// <returns>True if there is an intersection</returns>
        public bool Intersect(TimeChunk other)
        {
            //Check for possible intersection scenarios (and their reverse counterparts)
            return ((other.Start == this.Start && other.Stop == this.Stop) //Same interval
                || (other.Start <= this.Start && other.Stop > this.Start) //other starts before and ends in this
                || (this.Start <= other.Start && this.Stop > other.Start) //this starts before and ends in other
                || (other.Stop >= this.Stop && other.Start < this.Stop) //other starts in this and stops after this
                || (this.Stop >= other.Stop && this.Start < other.Stop)); //this starts in other and stops after others
        }

        #endregion

        #region IEquatable<TimeChunk> Members

        /// <summary>
        /// Compare this class to another
        /// </summary>
        /// <param name="other">Another TimeChunk</param>
        /// <returns>True of the start and sto is equal</returns>
        public bool Equals(TimeChunk other)
        {
            return this.Start.Equals(other.Start) && this.Stop.Equals(other.Stop);
        }

        #endregion
    }

    public class TimeRange
    {
        public DateTime StartTime { get; set; }
        public DateTime StopTime { get; set; }
        public TimeSpan Length
        {
            get
            {
                return StopTime.Subtract(StartTime);

            }
        }

        public TimeRange(DateTime starTime, DateTime stopTime)
        {
            this.StartTime = starTime;
            this.StopTime = stopTime;
        }
    }

    public static class TimeChunkExtensions
    {
        public static int GetMinutesFromTimeChunks(this List<TimeChunk> timeChunks, bool doRemoveDuplicates = false)
        {
            int minutes = 0;
            if (!timeChunks.IsNullOrEmpty())
            {
                if (doRemoveDuplicates)
                    timeChunks.RemoveDuplicateTime();
                timeChunks.ForEach(i => minutes += i.IntervallMinutes);
            }
            return minutes;
        }

        public static List<TimeChunk> FindIntersectingTimeChunks(this List<TimeChunk> timeChunks)
        {
            List<TimeChunk> matchingTimeChunks = new List<TimeChunk>();

            //Find intersections
            List<TimeSpan> stopTimes = timeChunks.Select(i => i.Stop).Distinct().ToList();
            foreach (TimeSpan stopTime in stopTimes)
            {
                //Add highest endtime to matches
                TimeChunk timeChunk = timeChunks.OrderBy(tc => tc.Start).FirstOrDefault(tc => tc.Stop.TotalMinutes == stopTime.TotalMinutes);
                if (timeChunk != null)
                    matchingTimeChunks.Add(timeChunk);
            }

            //Remove overlapping
            return matchingTimeChunks.RemoveOverlappingTimeChunks();
        }

        public static List<TimeChunk> RemoveOverlappingTimeChunks(this List<TimeChunk> timeChunks)
        {
            List<TimeChunk> validTimeChunks = new List<TimeChunk>();

            if (timeChunks.IsNullOrEmpty())
                return validTimeChunks;

            TimeChunk largestTimeChunk = timeChunks.OrderByDescending(i => i.IntervallMinutes).FirstOrDefault();
            if (largestTimeChunk == null)
                return validTimeChunks;

            validTimeChunks.Add(largestTimeChunk);

            foreach (TimeChunk timeChunk in timeChunks)
            {
                if (!CalendarUtility.IsDatesOverlapping(timeChunk.Start, timeChunk.Stop, largestTimeChunk.Start, largestTimeChunk.Stop))
                    validTimeChunks.Add(timeChunk);
            }

            return validTimeChunks;
        }

        public static bool RemoveDuplicateTime(this List<TimeChunk> timeChunks)
        {
            bool changed = false;

            // Some kind of strange autocorrelation to remove duplicates
            if (!timeChunks.IsNullOrEmpty())
            {
                for (int i = 0; i < timeChunks.Count; i++)
                {
                    for (int j = timeChunks.Count - 1; j >= 0; j--)
                    {
                        // Don't compare to self
                        if (j == i)
                            continue;

                        if (timeChunks[i].Intersect(timeChunks[j]))
                        {
                            // If the timechunks are a dubplicate, remove one
                            if (timeChunks[i].Equals(timeChunks[j]))
                            {
                                timeChunks.RemoveAt(j);
                                if (j < i)
                                    i--;
                                changed = true;
                            }
                            // else If the chunks intersect, add them together
                            else if (timeChunks[i].Intersect(timeChunks[j]))
                            {
                                timeChunks[i] = timeChunks[i].Or(timeChunks[j])[0];
                                timeChunks.RemoveAt(j);
                                if (j < i)
                                    i--;
                                changed = true;
                            }
                        }
                    }
                }
            }
            return changed;
        }
    }
}

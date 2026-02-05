using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Linq;
using System.Collections.Generic;

namespace SoftOne.Soe.Business.Util
{
    public class ApplyBreakDTO
    {
        #region Properties

        #region Schedule

        public TimeScheduleTemplateBlockDTO ScheduleBreakDTO { get; set; }
        public TimeCodeDTO TimeCodeDTO
        {
            get
            {
                return this.ScheduleBreakDTO?.TimeCode;
            }
        }

        //Schedule in/out
        public DateTime ScheduleIn { get; set; }
        public DateTime ScheduleOut { get; set; }
        public int ScheduleInMinutes { get; set; }
        public int ScheduleOutMinutes { get; set; }

        //Schedule window minutes from midnight
        public int ScheduleBreakWindowStartMinutes { get; set; }
        public int ScheduleBreakWindowStopMinutes { get; set; }

        //Schedule window time
        public DateTime ScheduleBreakWindowStartTime { get; set; }
        public DateTime ScheduleBreakWindowStopTime { get; set; }

        //EmployeeGroup settings
        public bool AutogenTimeblocks { get; set; }
        public bool AutogenBreakOnStamping { get; set; }

        #endregion

        #region Presence

        //Presence start/stop
        public DateTime PresenceBreakStart { get; set; }
        public DateTime PresenceBreakStop { get; set; }
        public int PresenceBreakStartMinutes { get; set; }
        public int PresenceBreakStopMinutes { get; set; }

        //Presence total minutes
        public int PresenceBreakMinutes { get; set; }
        public int PresenceBreakMinutesOutsideWindow { get; set; }
        public ApplyBreakPaddingSetting PaddingSetting { get; set; }
        public bool DoCreateZeroBreak
        {
            get
            {
                return this.PresenceBreakMinutes == 0 && this.TimeCodeDTO.MinMinutes == 0 && !this.AutogenBreakOnStamping;
            }
        }

        #endregion

        public TermGroup_TimeCodeRuleType CalculatedRuleType { get; set; }

        #endregion

        #region Ctor

        public ApplyBreakDTO(TimeScheduleTemplateBlockDTO templateBlockBreak, List<TimeScheduleTemplateBlock> templateBlocks, List<TimeBlock> presenceBreaks, EmployeeGroup employeeGroup, ApplyBreakPaddingSetting paddingRule)
        {
            //Schedule
            this.ScheduleBreakDTO = templateBlockBreak;
            this.ScheduleIn = templateBlocks.GetScheduleIn();
            this.ScheduleOut = templateBlocks.GetScheduleOut();
            this.ScheduleInMinutes = CalendarUtility.TimeToMinutes(this.ScheduleIn);
            this.ScheduleOutMinutes = CalendarUtility.TimeToMinutes(this.ScheduleOut);
            this.ScheduleBreakWindowStartMinutes = CalendarUtility.GetTimeInMinutes(this.TimeCodeDTO.StartType, this.TimeCodeDTO.StartTimeMinutes, this.ScheduleInMinutes, this.ScheduleOutMinutes);
            this.ScheduleBreakWindowStopMinutes = CalendarUtility.GetTimeInMinutes(this.TimeCodeDTO.StopType, this.TimeCodeDTO.StopTimeMinutes, this.ScheduleInMinutes, this.ScheduleOutMinutes);
            this.ScheduleBreakWindowStartTime = CalendarUtility.GetDateFromMinutes(CalendarUtility.DATETIME_DEFAULT, this.ScheduleBreakWindowStartMinutes);
            this.ScheduleBreakWindowStopTime = CalendarUtility.GetDateFromMinutes(CalendarUtility.DATETIME_DEFAULT, this.ScheduleBreakWindowStopMinutes);

            //Presence
            this.PresenceBreakStart = presenceBreaks.GetStartTime();
            this.PresenceBreakStop = presenceBreaks.GetStopTime();
            this.PresenceBreakStartMinutes = CalendarUtility.TimeToMinutes(this.PresenceBreakStart);
            this.PresenceBreakStopMinutes = CalendarUtility.TimeToMinutes(this.PresenceBreakStop);
            this.PresenceBreakMinutes = presenceBreaks.GetMinutes();
            this.PresenceBreakMinutesOutsideWindow = 0;
            this.PaddingSetting = paddingRule;

            //EmployeeGroup settings
            if (employeeGroup != null)
            {
                this.AutogenTimeblocks = employeeGroup.AutogenTimeblocks;
                this.AutogenBreakOnStamping = employeeGroup.AutogenBreakOnStamping;
            }

            this.CalculatedRuleType = TermGroup_TimeCodeRuleType.Unknown;
        }

        #endregion
    }

    public class ApplyBreakPaddingSetting
    {
        public DateTime SlotStartTime { get; set; }
        public DateTime SlotStopTime { get; set; }
        public bool DoMoveForward { get; set; }
        public bool DoMoveBackward
        {
            get
            {
                return !this.DoMoveForward;
            }
        }

        public ApplyBreakPaddingSetting(DateTime slotStartTime, DateTime slotStopTime, bool doMoveForward)
        {
            this.SlotStartTime = slotStartTime;
            this.SlotStopTime = slotStopTime;
            this.DoMoveForward = doMoveForward;
        }
    }

    public class EvaluteBreakPeriodDTO
    {
        #region Properties

        public int? Key { get; }
        public List<TimeScheduleTemplateBlock> ScheduleBlocks { get; }
        public List<TimeBlock> TimeBlocks { get; }

        public DateTime StartTime
        {
            get
            {
                return this.GetSchedule(excludeBreaks: true).FirstOrDefault()?.StartTime ?? CalendarUtility.DATETIME_DEFAULT;
            }
        }
        public DateTime StopTime
        {
            get
            {
                return this.GetSchedule(excludeBreaks: true).LastOrDefault()?.StopTime ?? CalendarUtility.DATETIME_DEFAULT;
            }
        }

        #endregion

        #region Ctor

        public EvaluteBreakPeriodDTO(List<TimeScheduleTemplateBlock> scheduleBlocks, List<TimeBlock> timeBlocks = null, int? key = null)
        {
            this.Key = key;
            this.ScheduleBlocks = scheduleBlocks ?? new List<TimeScheduleTemplateBlock>();
            this.TimeBlocks = timeBlocks ?? new List<TimeBlock>();
        }

        #endregion

        #region Public methods

        public List<TimeScheduleTemplateBlock> GetSchedule(bool excludeBreaks = false)
        {
            return this.ScheduleBlocks?.Where(i => !excludeBreaks || !i.IsBreak).OrderBy(i => i.StartTime).ToList() ?? new List<TimeScheduleTemplateBlock>();
        }

        public List<TimeScheduleTemplateBlock> GetScheduleBreaks()
        {
            return this.ScheduleBlocks?.Where(i => i.IsBreak).OrderBy(i => i.StartTime).ToList() ?? new List<TimeScheduleTemplateBlock>();
        }

        public List<TimeBlock> GetTimeBlocks(bool excludeBreaks = false)
        {
            return this.TimeBlocks?.Where(i => !excludeBreaks || !i.IsBreak).OrderBy(i => i.StartTime).ToList() ?? new List<TimeBlock>();
        }

        public void AddTimeBlock(TimeBlock timeBlock)
        {
            if (timeBlock == null || this.TimeBlocks.Contains(timeBlock))
                return;

            this.TimeBlocks.Add(timeBlock);
        }

        #endregion
    }
}

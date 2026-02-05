using System;
using System.Collections.Generic;
using System.Text;

namespace SoftOne.Soe.Shared.DTO
{
    /// <summary>
    /// Import schedule information for a specific employee for a specific period
    /// </summary>
    public class EmployeeActiveScheduleIO
    {
        /// <summary>
        /// Use EmployeeNr to identify the employee or use ExternalEmployeeCode
        /// </summary>
        public string EmployeeNr { get; set; }
        /// <summary>
        /// Use EmployeeNr to identify the employee or use ExternalEmployeeCode
        /// </summary>
        public string EmployeeExternalCode { get; set; }

        public List<ActiveScheduleInterval> ActiveScheduleIntervals { get; set; } = new List<ActiveScheduleInterval>();
    }

    /// <summary>
    /// Use to active schedule for a specific period. Without any blocks the employee is still activeted for the period.
    /// </summary>
    public class ActiveScheduleInterval
    {
        /// <summary>
        /// Start date for the period
        /// </summary>
        public DateTime StartDate { get; set; }
        /// <summary>
        /// Stop date for the period
        /// </summary>
        public DateTime StopDate { get; set; }
        /// <summary>
        /// List of all days during the period. All existing days will be removed and replaced with the new ones. Empty list will remove all days for the period. If the day is not in the list the day (and scheduled blocks) will be removed.
        /// </summary>

        public List<ActiveScheduleDay> ActiveScheduleDays { get; set; } = new List<ActiveScheduleDay>();
    }

    /// <summary>
    /// Each schedule needs to be connected to a date. 
    /// </summary>
    public class ActiveScheduleDay
    {
        /// <summary>
        /// Date of which the scheduled day is connected to
        /// </summary>
        public DateTime Date { get; set; }
        /// <summary>
        /// List of all blocks during the day. All existing blocks will be removed and replaced with the new ones. Empty list will remove all blocks for the day.
        /// </summary>
        public List<ActiveScheduleBlock> ActiveScheduleBlocks { get; set; } = new List<ActiveScheduleBlock>();
        /// <summary>
        /// List of all breaks during the day. All existing breaks will be removed and replaced with the new ones. Empty list will remove all breaks for the day.
        /// </summary>
        public List<ActiveSceduleBreakBlock> ActiveSceduleBreakBlocks { get; set; } = new List<ActiveSceduleBreakBlock>();
        /// <summary>
        /// External code for the day. Is empty the default day type is used.
        /// </summary>
        public string DayExternalCode { get; set; }
    }

    /// <summary>
    ///  Create a block for each change during the day. The breaks need to overlap the block.
    /// </summary>
    public class ActiveScheduleBlock
    {
        /// <summary>
        /// Start time of the block
        /// </summary>
        public DateTime StartTime { get; set; }
        /// <summary>
        /// Stop time of the block
        /// 
        public DateTime StopTime { get; set; }
        /// <summary>
        /// Set the code for the shift type. Is empty the default shift type is used.
        /// </summary>
        public string ShiftTypeExternalCode { get; set; }
        /// <summary>
        /// Set the code for where the block is connected to. Is empty the default account on employee is used.
        /// </summary>
        public string HierarchicalAccountExternalCode { get; set; }
        /// <summary>
        /// Set the code for the account. Is empty the default account on employee is used.
        /// </summary>        
        public string AccountExternalCodeSIECostCenter { get; set; }
        /// <summary>
        /// Set the code form the account. Is empty the default account on employee is used.
        /// </summary>
        public string AccountExternalCodeSIEProject { get; set; }
        public string AccountExternalCodeSIEDepartment { get; set; }
        public string AccountExternalCodeSIECostUnit { get; set; }
        public string AccountExternalCodeSIEShop { get; set; }
    }

    /// <summary>
    /// Each break need to be it's own block.
    /// </summary>
    public class ActiveSceduleBreakBlock
    {
        /// <summary>
        /// Start time of the break
        /// </summary>
        public DateTime StartTime { get; set; }
        /// <summary>
        /// Stop time of the break
        /// </summary>
        public DateTime StopTime { get; set; }
        /// <summary>
        /// Set the code for the break type. Is empty the default break type is used (based on minutes).
        /// </summary>
        public string BreakTypeExternalCode { get; set; }
        /// <summary>
        /// Set the code for the account. Is empty the default account on employee is used. Set to the same as schedule block.
        /// </summary>
        public string AccountExternalCode { get; set; }
    }
}

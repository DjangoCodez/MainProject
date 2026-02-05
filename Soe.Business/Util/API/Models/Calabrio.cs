using System;

namespace SoftOne.Soe.Business.Util.API.Models
{

    #region Absence


    public class AbsenseRequest
    {
        public string BusinessUnitId { get; set; }
    }


    public class AbsenceResponse
    {
        public AbsenceResult[] Result { get; set; }
        public string Message { get; set; }
    }

    public class AbsenceResult
    {
        public string Id { get; set; }
        public int Priority { get; set; }
        public string Name { get; set; }
        public bool Requestable { get; set; }
        public bool InWorkTime { get; set; }
        public bool InPaidTime { get; set; }
        public string PayrollCode { get; set; }
        public bool Confidential { get; set; }
    }

    #endregion

    #region activity

    public class ActivityRequest
    {
        public string BusinessUnitId { get; set; }
    }

    public class ActivityResponse
    {
        public ActivityResult[] Result { get; set; }
        public string Message { get; set; }
    }

    public class ActivityResult
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public bool InReadyTime { get; set; }
        public bool RequiresSkill { get; set; }
        public bool InWorkTime { get; set; }
        public bool InPaidTime { get; set; }
        public string ReportLevelDetail { get; set; }
        public bool RequiresSeat { get; set; }
        public string PayrollCode { get; set; }
        public bool AllowOverwrite { get; set; }
        public string DisplayColor { get; set; }
    }

    #endregion

    #region Schedule


    public class ScheduleByTeamRequest
    {
        public string BusinessUnitId { get; set; }
        public string TeamId { get; set; }
        public Period Period { get; set; }
        public string ScenarioId { get; set; }
    }

    public class Period
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }


    public class ScheduleByTeamResponse
    {
        public Schedule[] Result { get; set; }
        public string Message { get; set; }
    }

    public class Schedule
    {
        public string PersonId { get; set; }
        public string Date { get; set; }
        public Shift[] Shift { get; set; }
    }

    public class Shift
    {
        public string Name { get; set; }
        public ShiftPeriod Period { get; set; }
        public string ActivityId { get; set; }
        public string AbsenceId { get; set; }
        public int DisplayColor { get; set; }
    }

    public class ShiftPeriod
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }

    #endregion

    #region Teams


    public class TeamRequest
    {
        public string BusinessUnitId { get; set; }
        public TeamPeriod Period { get; set; }
    }

    public class TeamPeriod
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }



    public class TeamResponse
    {
        public TeamResult[] Result { get; set; }
        public object Message { get; set; }
    }

    public class TeamResult
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }


    #endregion

    #region Employee


    public class EmployeeByTeamRequest
    {
        public string TeamId { get; set; }
        public DateTime? Date { get; set; }
    }



    public class EmployeeByTeamResponse
    {
        public EmployeeResult[] Result { get; set; }
        public string Message { get; set; }
    }

    public class EmployeeResult
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmploymentNumber { get; set; }
        public string Email { get; set; }
        public string DisplayName { get; set; }
        public string TimeZoneId { get; set; }
        public string BusinessUnitId { get; set; }
        public string SiteId { get; set; }
        public string TeamId { get; set; }
        public string WorkflowControlSetId { get; set; }
        public int FirstDayOfWeek { get; set; }
    }



    #endregion
}

using System;
using System.Runtime.Serialization;
using System.Collections.Generic;

namespace SoftOne.Soe.Business.DTO
{
    #region Account

    [DataContract]
    public class TSAccountItem
    {
        [DataMember]
        public int AccountId { get; set; }
        [DataMember]
        public string AccountNr { get; set; }
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public DateTime? Created { get; set; }
        [DataMember]
        public DateTime? Modified { get; set; }
        [DataMember]
        public int State { get; set; }
    }

    #endregion

    #region Employee

    [DataContract]
    public class TSEmployeeItemStatus
    {
        public int EmployeeId { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public DateTime StampTime { get; set; }
        [DataMember]
        public string TimeTerminalName { get; set; }
    }

    [DataContract]
    public class TSEmployeeItem : SoftOne.Soe.Common.Interfaces.Common.IEmployeeUserBasic
    {
        [DataMember]
        public int EmployeeId { get; set; }
        [DataMember]
        public int EmployeeGroupId { get; set; }
        [DataMember]
        public string EmployeeNr { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string FirstName { get; set; }
        [DataMember]
        public string LastName { get; set; }
        [DataMember]
        public string SocialSec { get; set; }
        [DataMember]
        public string CardNumber { get; set; }
        [DataMember]
        public string EMail { get; set; }

        [DataMember]
        public DateTime? Created { get; set; }
        [DataMember]
        public DateTime? Modified { get; set; }
        [DataMember]
        public int State { get; set; }
    }

    #endregion

    #region EmployeeGroup

    [DataContract]
    public class TSEmployeeGroupItem
    {
        [DataMember]
        public int EmployeeGroupId { get; set; }
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public DateTime? Created { get; set; }
        [DataMember]
        public DateTime? Modified { get; set; }
        [DataMember]
        public int State { get; set; }
    }

    #endregion

    #region EmployeeSchedule

    [DataContract]
    public class TSSyncEmployeeScheduleResult
    {
        public TSSyncEmployeeScheduleResult()
        {
            AllItemsFetched = true;
        }

        [DataMember]
        public bool AllItemsFetched { get; set; }
        [DataMember]
        public List<TSEmployeeScheduleItem> Items { get; set; }
    }

    [DataContract]
    public class TSEmployeeScheduleItem
    {
        [DataMember]
        public int TimeScheduleTemplateBlockId { get; set; }
        [DataMember]
        public int? TimeScheduleTemplatePeriodId { get; set; }
        [DataMember]
        public int? TimeScheduleEmployeePeriodId { get; set; }
        [DataMember]
        public int TimeCodeId { get; set; }
        [DataMember]
        public int EmployeeId { get; set; }
        [DataMember]
        public int AccountId { get; set; }
        [DataMember]
        public DateTime StartTime { get; set; }
        [DataMember]
        public DateTime StopTime { get; set; }
        [DataMember]
        public bool NoSchedule { get; set; }
        [DataMember]
        public int BreakType { get; set; }

        [DataMember]
        public DateTime? Created { get; set; }
        [DataMember]
        public DateTime? Modified { get; set; }
        [DataMember]
        public int State { get; set; }
    }

    #endregion

    #region TimeAccumulator

    [DataContract]
    public class TSTimeAccumulatorEmployeeItem
    {
        [DataMember]
        public int TimeAccumulatorId { get; set; }
        [DataMember]
        public int EmployeeId { get; set; }
        [DataMember]
        public string EmployeeNr { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public decimal SumToday { get; set; }
        [DataMember]
        public decimal SumPeriod { get; set; }
        [DataMember]
        public decimal SumAccToday { get; set; }
        [DataMember]
        public decimal SumYear { get; set; }

        [DataMember]
        public DateTime SyncDate { get; set; }
    }

    #endregion

    #region TimeCode

    [DataContract]
    public class TSTimeCodeItem
    {
        [DataMember]
        public int TimeCodeId { get; set; }
        [DataMember]
        public int Type { get; set; }
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public int BreakMinMinutes { get; set; }
        [DataMember]
        public int BreakMaxMinutes { get; set; }
        [DataMember]
        public int BreakDefaultMinutes { get; set; }

        [DataMember]
        public int BreakStartType { get; set; }
        [DataMember]
        public int BreakStopType { get; set; }
        [DataMember]
        public int BreakStartTimeMinutes { get; set; }
        [DataMember]
        public int BreakStopTimeMinutes { get; set; }

        [DataMember]
        public DateTime? Created { get; set; }
        [DataMember]
        public DateTime? Modified { get; set; }
        [DataMember]
        public int State { get; set; }
    }

    #endregion

    #region TimeDeviationCause

    [DataContract]
    public class TSTimeDeviationCauseItem
    {
        [DataMember]
        public int TimeDeviationCauseId { get; set; }
        [DataMember]
        public int Type { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public List<int> EmployeeGroupIds { get; set; }

        [DataMember]
        public DateTime? Created { get; set; }
        [DataMember]
        public DateTime? Modified { get; set; }
        [DataMember]
        public int State { get; set; }
    }

    #endregion

    #region TimeStampEntry

    [DataContract]
    public class TSTimeStampEntryItem
    {
        [DataMember]
        public int TimeStampEntryId { get; set; }
        [DataMember]
        public int TimeStampEntryInternalId { get; set; }
        [DataMember]
        public int EmployeeId { get; set; }
        [DataMember]
        public int? TimeDeviationCauseId { get; set; }
        [DataMember]
        public int? AccountId { get; set; }
        [DataMember]
        public int? TimeScheduleTemplatePeriodId { get; set; }
        [DataMember]
        public int Type { get; set; }
        [DataMember]
        public DateTime Time { get; set; }
        [DataMember]
        public DateTime? Created { get; set; }
        [DataMember]
        public DateTime? Modified { get; set; }
        [DataMember]
        public int Status { get; set; }
        [DataMember]
        public int State { get; set; }
    }

    #endregion
}

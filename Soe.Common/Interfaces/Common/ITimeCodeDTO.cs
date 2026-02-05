using SoftOne.Soe.Common.Util;
using System;

namespace SoftOne.Soe.Common.Interfaces.Common
{    
    public interface ITimeCodeDTO
    {
        int ActorCompanyId { get; set; }
        int TimeCodeId { get; set; }
        string Code { get; set; }
        string Name { get; set; }
        string Description { get; set; }
        bool Payed { get; set; }
        bool FactorBasedOnWorkPercentage { get; set; }
        int MinutesByConstantRules { get; set; }
        SoeTimeCodeType Type { get; set; }
        TermGroup_TimeCodeRegistrationType RegistrationType { get; set; }
        TermGroup_TimeCodeRoundingType RoundingType { get; set; }
        int? RoundingTimeCodeId { get; set; }
        int? RoundingInterruptionTimeCodeId { get; set; }
        string RoundingGroupKey { get; set; }
        int RoundingValue { get; set; }
        bool RoundStartTime { get; set; }

        TermGroup_TimeCodeClassification Classification { get; set; }
        DateTime? Created { get; set; }
        string CreatedBy { get; set; }
        DateTime? Modified { get; set; }
        string ModifiedBy { get; set; }
        SoeEntityState State { get; set; }
    }
}

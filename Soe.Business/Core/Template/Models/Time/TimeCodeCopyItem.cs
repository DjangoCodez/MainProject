using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;

namespace SoftOne.Soe.Business.Core.Template.Models.Time
{
    public class TimeCodeCopyItem
    {
        public int TimeCodeId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Type { get; set; }
        public int RegistrationType { get; set; }
        public bool Payed { get; set; }
        public int MinutesByConstantRules { get; set; }
        public bool FactorBasedOnWorkPercentage { get; set; }

        public int Classification { get; set; }

        //Rounding
        public int RoundingType { get; set; }
        public int RoundingValue { get; set; }
        public int? RoundingTimeCodeId { get; set; }
        public int? RoundingInterruptionTimeCodeId { get; set; }
        public string RoundingGroupKey { get; set; }

        //Adjstment
        public bool RoundStartTime { get; set; }
        public int AdjustQuantityByBreakTime { get; set; }
        public int? AdjustQuantityTimeCodeId { get; set; }
        public int? AdjustQuantityTimeScheduleTypeId { get; set; }

        //Work
        public bool IsWorkOutsideSchedule { get; set; }

        //Absence
        public bool IsAbsence { get; set; }
        public int? KontekId { get; set; }

        //Break
        public int MinMinutes { get; set; }
        public int MaxMinutes { get; set; }
        public int DefaultMinutes { get; set; }
        public int StartType { get; set; }
        public int StopType { get; set; }
        public int StartTimeMinutes { get; set; }
        public int StopTimeMinutes { get; set; }
        public DateTime? StartTime { get; set; }
        public int? TimeCodeBreakGroupId { get; set; }

        //Expense
        public TermGroup_ExpenseType ExpenseType { get; set; }
        public string Comment { get; set; }
        public bool StopAtDateStart { get; set; }
        public bool StopAtDateStop { get; set; }
        public bool StopAtPrice { get; set; }
        public bool StopAtVat { get; set; }
        public bool StopAtAccounting { get; set; }
        public bool StopAtComment { get; set; }
        public bool CommentMandatory { get; set; }
        public decimal? FixedQuantity { get; internal set; }
        public bool ShowInTerminal { get; internal set; }
        public bool HideForEmployee { get; internal set; }

        //Material
        public string Note { get; internal set; }

        //Relations
        public List<TimeCodePayrollProductCopyItem> TimeCodePayrollProducts { get; set; } = new List<TimeCodePayrollProductCopyItem>();
        public List<TimeCodeInvoiceProductCopyItem> TimeCodeInvoiceProducts { get; set; } = new List<TimeCodeInvoiceProductCopyItem>();
        public List<TimeCodeRuleCopyItem> TimeCodeRules { get; set; } = new List<TimeCodeRuleCopyItem>();

        public TimeCode CopyTimeCode(TemplateCompanyTimeDataItem templateCompanyTimeData, Company newCompany)
        {
            if (templateCompanyTimeData == null || newCompany == null)
                return null;

            TimeCode timeCode = null;
            switch (this.Type)
            {
                case (int)SoeTimeCodeType.Work:
                    timeCode = new TimeCodeWork()
                    {
                        IsWorkOutsideSchedule = this.IsWorkOutsideSchedule,
                    };
                    break;
                case (int)SoeTimeCodeType.Absense:
                    timeCode = new TimeCodeAbsense()
                    {
                        IsAbsence = this.IsAbsence,
                        KontekId = this.KontekId,
                    };
                    break;
                case (int)SoeTimeCodeType.Break:
                    timeCode = new TimeCodeBreak()
                    {
                        MinMinutes = this.MinMinutes,
                        MaxMinutes = this.MaxMinutes,
                        DefaultMinutes = this.DefaultMinutes,
                        StartType = this.StartType,
                        StopType = this.StopType,
                        StartTimeMinutes = this.StartTimeMinutes,
                        StopTimeMinutes = this.StopTimeMinutes,
                        StartTime = this.StartTime,
                        TimeCodeBreakGroupId = this.TimeCodeBreakGroupId.HasValue ? templateCompanyTimeData.GetTimeCodeBreakGroup(this.TimeCodeBreakGroupId.Value)?.TimeCodeBreakGroupId : null
                    };
                    break;
                case (int)SoeTimeCodeType.AdditionDeduction:
                    timeCode = new TimeCodeAdditionDeduction()
                    {
                        ExpenseType = this.ExpenseType == TermGroup_ExpenseType.Unknown ? (int)TermGroup_ExpenseType.Time : (int)this.ExpenseType,
                        Comment = this.Comment,
                        StopAtDateStart = this.StopAtDateStart,
                        StopAtDateStop = this.StopAtDateStop,
                        StopAtPrice = this.StopAtPrice,
                        StopAtVat = this.StopAtVat,
                        StopAtAccounting = this.StopAtAccounting,
                        StopAtComment = this.StopAtComment,
                        CommentMandatory = this.CommentMandatory,
                        HideForEmployee = this.HideForEmployee,
                        ShowInTerminal = this.ShowInTerminal,
                        FixedQuantity = this.FixedQuantity,
                    };
                    break;
                case (int)SoeTimeCodeType.Material:
                    timeCode = new TimeCodeMaterial()
                    {
                        Note = this.Note,
                    };
                    break;
            }

            if (timeCode == null)
                return null;

            timeCode.Company = newCompany;
            timeCode.Code = this.Code;
            timeCode.Name = this.Name;
            timeCode.Description = this.Description;
            timeCode.Type = this.Type;
            timeCode.RegistrationType = this.RegistrationType;
            timeCode.Payed = this.Payed;
            timeCode.MinutesByConstantRules = this.MinutesByConstantRules;
            timeCode.FactorBasedOnWorkPercentage = this.FactorBasedOnWorkPercentage;
            timeCode.Classification = this.Classification;

            //Rounding
            timeCode.RoundingType = this.RoundingType;
            timeCode.RoundingValue = this.RoundingValue;
            timeCode.RoundingTimeCodeId = this.RoundingTimeCodeId.HasValue ? templateCompanyTimeData.GetTimeCode(this.RoundingTimeCodeId.Value)?.TimeCodeId : null;
            timeCode.RoundingInterruptionTimeCodeId = this.RoundingInterruptionTimeCodeId.HasValue ? templateCompanyTimeData.GetTimeCode(this.RoundingInterruptionTimeCodeId.Value)?.TimeCodeId : null;
            timeCode.RoundingGroupKey = this.RoundingGroupKey;           
            timeCode.RoundStartTime = this.RoundStartTime;

            //Adjustment
            timeCode.AdjustQuantityByBreakTime = this.AdjustQuantityByBreakTime;
            timeCode.AdjustQuantityTimeScheduleTypeId = null; //Cannot be set due to TimeScheduleTypes are copied later and dependent on TimeCode..
            timeCode.AdjustQuantityTimeCodeId = this.AdjustQuantityTimeCodeId.HasValue ? templateCompanyTimeData.GetTimeCode(this.AdjustQuantityTimeCodeId.Value)?.TimeCodeId : null;
            timeCode.AdjustQuantityTimeScheduleTypeId = this.AdjustQuantityTimeScheduleTypeId.HasValue ? templateCompanyTimeData.GetTimeScheduleType(this.AdjustQuantityTimeScheduleTypeId.Value)?.TimeScheduleTypeId : null;
            return timeCode;
        }

        public void SetValuesFromTimeCode(TimeCode timeCode)
        {
            if (timeCode == null)
                return;

            TimeCodeId = timeCode.TimeCodeId;
            Code = timeCode.Code;
            Name = timeCode.Name;
            Description = timeCode.Description;
            Type = timeCode.Type;
            RegistrationType = timeCode.RegistrationType;
            Classification = timeCode.Classification;
            Payed = timeCode.Payed;
            MinutesByConstantRules = timeCode.MinutesByConstantRules;
            FactorBasedOnWorkPercentage = timeCode.FactorBasedOnWorkPercentage;

            //Rounding
            RoundingType = timeCode.RoundingType;
            RoundingValue = timeCode.RoundingValue;
            RoundingTimeCodeId = timeCode.RoundingTimeCodeId;
            RoundingInterruptionTimeCodeId = timeCode.RoundingInterruptionTimeCodeId;
            RoundingGroupKey = timeCode.RoundingGroupKey;
            RoundStartTime = timeCode.RoundStartTime;

            //Adjustment
            AdjustQuantityByBreakTime = timeCode.AdjustQuantityByBreakTime;
            AdjustQuantityTimeCodeId = timeCode.AdjustQuantityTimeCodeId;
            AdjustQuantityTimeScheduleTypeId = timeCode.AdjustQuantityTimeScheduleTypeId;

            if (timeCode is TimeCodeWork timeCodeWork)
            {
                IsWorkOutsideSchedule = timeCodeWork.IsWorkOutsideSchedule;
            }
            else if (timeCode is TimeCodeAbsense timeCodeAbsence)
            {
                IsAbsence = true;
                KontekId = timeCodeAbsence.KontekId;
            }
            else if (timeCode is TimeCodeBreak timeCodeBreak)
            {
                MinMinutes = timeCodeBreak.MinMinutes;
                MaxMinutes = timeCodeBreak.MaxMinutes;
                DefaultMinutes = timeCodeBreak.DefaultMinutes;
                StartType = timeCodeBreak.StartType;
                StopType = timeCodeBreak.StopType;
                StartTimeMinutes = timeCodeBreak.StartTimeMinutes;
                StopTimeMinutes = timeCodeBreak.StopTimeMinutes;
                StartTime = timeCodeBreak.StartTime;
                TimeCodeBreakGroupId = timeCodeBreak.TimeCodeBreakGroupId;
            }
            else if (timeCode is TimeCodeAdditionDeduction timeCodeExpense)
            {
                ExpenseType = (TermGroup_ExpenseType)timeCodeExpense.ExpenseType;
                Comment = timeCodeExpense.Comment;
                StopAtDateStart = timeCodeExpense.StopAtDateStart;
                StopAtDateStop = timeCodeExpense.StopAtDateStop;
                StopAtPrice = timeCodeExpense.StopAtPrice;
                StopAtVat = timeCodeExpense.StopAtVat;
                StopAtAccounting = timeCodeExpense.StopAtAccounting;
                StopAtComment = timeCodeExpense.StopAtComment;
                CommentMandatory = timeCodeExpense.CommentMandatory;
                HideForEmployee = timeCodeExpense.HideForEmployee;
                ShowInTerminal = timeCodeExpense.ShowInTerminal;
                FixedQuantity = timeCodeExpense.FixedQuantity;
            }
            else if (timeCode is TimeCodeMaterial timeCodeMaterial)
            {
                Note = timeCodeMaterial.Note;
            }
        }
    }

    public class TimeCodeBreakGroupCopyItem
    {
        public int TimeCodeBreakGroupId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }

    public class TimeCodePayrollProductCopyItem
    {
        public int ProductId { get; set; }
        public decimal Factor { get; set; }
    }

    public class TimeCodeInvoiceProductCopyItem
    {
        public int ProductId { get; set; }
        public decimal Factor { get; set; }
    }

    public class TimeCodeRuleCopyItem
    {
        public int Type { get; set; }
        public int Value { get; set; }
        public DateTime? Time { get; set; }
    }
    public class TimeCodeRankingGroupCopyItem {
        public int TimeCodeRankingGroupId { get; set; }
        public int ActorCompanyId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? StopDate { get; set; }
        public string Description { get; set; }

        public List<TimeCodeRankingCopyItem> TimeCodeRankings { get; set; } 

    }
    public class TimeCodeRankingCopyItem {
        public int TimeCodeRankingId { get; set; }
        public int ActorCompanyId { get; set; }
        public int LeftTimeCodeId { get; set; }
        public int RightTimeCodeId { get; set; }
        public int OperatorType { get; set; }
        public int TimeCodeRankingGroupId { get; set; }
    }
}


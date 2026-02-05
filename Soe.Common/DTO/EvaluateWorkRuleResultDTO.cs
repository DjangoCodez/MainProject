using SoftOne.Soe.Common.Attributes;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;

namespace SoftOne.Soe.Common.DTO
{
    [TSInclude]
    public class EvaluateWorkRuleResultDTO
    {
        public DateTime? Date { get; set; }
        public int? EmployeeId { get; set; }
        public SoeScheduleWorkRules EvaluatedWorkRule { get; set; }
        public TermGroup_ShiftHistoryType Action { get; set; }
        public DateTime? WorkTimeReachedDateFrom { get; set; }
        public DateTime? WorkTimeReachedDateTo { get; set; }
        public DateTime? RestTimeDayReachedDateFrom { get; set; }
        public DateTime? RestTimeDayReachedDateTo { get; set; }
        public int ErrorNumber { get; set; }
        public String ErrorMessage { get; set; }
        public String EmployeeName { get; set; }
        public bool Success { get; set; }
        public bool IsRuleForMinors { get; set; }
        public bool IsRuleRestTimeDayMandatory { get; set; }
        public bool IsRuleRestTimeWeekMandatory { get; set; }
        public bool CanUserOverrideRuleForMinorsViolation { get; set; }
        public bool CanUserOverrideRuleViolation
        {
            get
            {
                if (this.ErrorNumber == (int)ActionResultSave.RuleShiftsOverlap || this.ErrorNumber == (int)ActionResultSave.RuleContainsAttestedTransactions || this.ErrorNumber == (int)ActionResultSave.RuleDayIsLocked)
                    return false;
                if (this.ErrorNumber == (int)ActionResultSave.RuleRestDayReached && this.IsRuleRestTimeDayMandatory)
                    return false;
                if (this.ErrorNumber == (int)ActionResultSave.RuleRestWeekReached && this.IsRuleRestTimeWeekMandatory)
                    return false;
                if (this.IsRuleForMinors && !this.CanUserOverrideRuleForMinorsViolation)
                    return false;
                if (this.ErrorNumber == (int)ActionResultSave.RuleShiftRequestPreventTooEarlyStop)
                    return false;
                return true;
            }
        }

        public EvaluateWorkRuleResultDTO()
        {
            EvaluatedWorkRule = SoeScheduleWorkRules.None;
            ErrorNumber = 0;
            ErrorMessage = String.Empty;
            Success = true;
        }

        public EvaluateWorkRuleResultDTO(bool success)
        {
            EvaluatedWorkRule = SoeScheduleWorkRules.None;
            ErrorNumber = 0;
            ErrorMessage = String.Empty;
            Success = success;
        }

        public EvaluateWorkRuleResultDTO(int errNr, string errMessage)
        {
            EvaluatedWorkRule = SoeScheduleWorkRules.None;
            ErrorNumber = errNr;
            ErrorMessage = errMessage;
        }
        public EvaluateWorkRuleResultDTO(int errNr, string errMessage, string employeeName, DateTime? date = null)
        {
            EvaluatedWorkRule = SoeScheduleWorkRules.None;
            ErrorNumber = errNr;
            ErrorMessage = errMessage;
            EmployeeName = employeeName;
            Date = date;
        }
    }

    [TSInclude]
    public class EvaluateWorkRulesActionResult
    {
        public EvaluateWorkRulesActionResult()
        {
            this.EvaluatedRuleResults = new List<EvaluateWorkRuleResultDTO>();
            this.Result = new ActionResult();
        }
        public EvaluateWorkRulesActionResult(ActionResultSave actionResultSave)
        {
            this.EvaluatedRuleResults = new List<EvaluateWorkRuleResultDTO>();
            this.Result = new ActionResult((int)actionResultSave);
        }

        public List<EvaluateWorkRuleResultDTO> EvaluatedRuleResults { get; set; }
        public ActionResult Result { get; set; }
        public bool AllRulesSucceded
        {
            get
            {
                foreach (EvaluateWorkRuleResultDTO ruleResult in EvaluatedRuleResults)
                {
                    if (!ruleResult.Success)
                        return false;
                }
                return true;
            }
        }
        public bool CanUserOverrideRuleViolation
        {
            get
            {
                foreach (EvaluateWorkRuleResultDTO ruleResult in EvaluatedRuleResults)
                {
                    if (!ruleResult.CanUserOverrideRuleViolation)
                        return false;
                }
                return true;
            }
        }
        private String errorMessage;
        public String ErrorMessage
        {
            get
            {
                errorMessage = String.Empty;
                foreach (EvaluateWorkRuleResultDTO ruleResult in EvaluatedRuleResults)
                {
                    if (!ruleResult.Success)
                        errorMessage += ruleResult.ErrorMessage + "\n";
                }
                return errorMessage;
            }
        }
    }

    public class EvaluateAllWorkRulesActionResult
    {
        public List<EvaluateAllWorkRulesResultDTO> EvaluatedRuleResults { get; set; }
        public ActionResult Result { get; set; }

        public EvaluateAllWorkRulesActionResult()
        {
            this.EvaluatedRuleResults = new List<EvaluateAllWorkRulesResultDTO>();
            this.Result = new ActionResult();
        }
    }

    public class EvaluateAllWorkRulesResultDTO
    {
        public int EmployeeId { get; set; }
        public List<string> Violations { get; set; }
    }

    public class EvaluateDeviationsAgainstWorkRules
    {
        public EvaluateDeviationsAgainstWorkRules()
        {
            EvaluateWorkRulesResult = new EvaluateWorkRulesActionResult();
        }

        public EvaluateWorkRulesActionResult EvaluateWorkRulesResult { get; set; }
        public bool Success { get; set; }
        public bool EvaluateRulesFailed { get; set; }
        public bool SendXeMailNeeded { get; set; }
        public bool SendXeMailSucceded { get; set; }

        public string ErrorMessage { get; set; }
        public string InfoMessage { get; set; }
    }
}

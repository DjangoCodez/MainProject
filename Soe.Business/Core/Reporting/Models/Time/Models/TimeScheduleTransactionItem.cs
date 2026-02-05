using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;

namespace SoftOne.Soe.Business.Core.Reporting.Models.Time.Models
{
    public class TimeScheduleTransactionItem
    {
        public TimeScheduleTransactionItem()
        {
            ExtraFieldAnalysisFields = new List<ExtraFieldAnalysisField>();
            AccountAnalysisFields = new List<AccountAnalysisField>();
        }
        public int TimeScheduleTemplateBlockId { get; set; }
        public int EmployeeId { get; set; }
        public int EmployeeGroupId { get; set; }
        public string SocialSec { get; set; }
        public int? ShiftTypeId { get; set; }
        public int? AccountId { get; set; }
        public int TimeCodeId { get; set; }
        public int? TimeDeviationCauseId { get; set; }
        public int? TimeScheduleScenarioHeadId { get; set; }

        public DateTime Date { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime StopTime { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal UnitPriceCurrency { get; set; }
        public decimal UnitPriceEntCurrency { get; set; }
        public decimal UnitPriceLedgerCurrency { get; set; }
        public decimal Amount { get; set; }
        public decimal AmountCurrency { get; set; }
        public decimal AmountEntCurrency { get; set; }
        public decimal AmountLedgerCurrency { get; set; }
        public decimal GrossAmount { get; set; }
        public decimal GrossAmountCurrency { get; set; }
        public decimal GrossAmountEntCurrency { get; set; }
        public decimal GrossAmountLedgerCurrency { get; set; }
        public decimal VatAmount { get; set; }
        public decimal VatAmountCurrency { get; set; }
        public decimal VatAmountEntCurrency { get; set; }
        public decimal VatAmountLedgerCurrency { get; set; }
        public decimal Quantity
        {
            get { return Convert.ToDecimal((StopTime - StartTime).TotalMinutes); }
        }
        public decimal GrossQuantity { get; set; }
        public decimal NetQuantity
        {
            get { return this.IsBreak ? this.Quantity : this._netQuantity; }
            set { this._netQuantity = value; }
        }
        public bool IsBreak { get; set; }
        public bool SubstituteShift { get; set; }
        public bool SubstituteShiftCalculated { get; set; }
        public bool ExtraShift { get; set; }
        public bool IsPreliminary { get; set; }
        public string Link { get; set; }
        public string Description { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }

        // Extensions
        public List<AccountInternalDTO> AccountInternals { get; set; }
        public int? ScheduleTypeId { get; set; }
        public List<GrossTimeRule> GrossTimeRules { get; set; }

        public string GroupName { get; set; }
        public string EmployeeNrSort { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public double NetLength
        {
            get { return this.IsBreak ? this.Length : this._netLength; }
            set { this._netLength = value; }
        }
        public double Length
        {
            get { return (this.StopTime - this.StartTime).TotalMinutes; }
        }

        public bool IsZeroSchedule
        {
            get { return this.StartTime == this.StopTime; }
        }

        public decimal QuantityHours
        {
            get
            {
                return decimal.Round(decimal.Divide(Quantity, 60));

            }
        }
        
        //Private variables
        private double _netLength { get; set; }
        private decimal _netQuantity { get; set; }

        public List<ExtraFieldAnalysisField> ExtraFieldAnalysisFields { get; set; }
        public List<AccountAnalysisField> AccountAnalysisFields { get; set; }

        //Matrix
        public AccountAndInternalAccountComboDTO AccountAndInternalAccountCombo { get; set; }
        public TermGroup_TimeScheduleTemplateBlockType Type { get; set; }
        
        public decimal EmploymentPercent { get; set; }
        public string EmployeeGroup { get; set; }
    }

    public static class TimeScheduleTransactionItemExtensions
    {
        public static string GetAccountingIdString(this TimeScheduleTransactionItem timeScheduleTransactionItem)
        {
            string str = string.Empty;
            if (timeScheduleTransactionItem.AccountId.HasValue)
                str = timeScheduleTransactionItem.AccountId.ToString();
            else
                str = "0";

            if (timeScheduleTransactionItem.AccountInternals != null)

                foreach (var ai in timeScheduleTransactionItem.AccountInternals)
                    str += $"|{ai.AccountId}";

            return str;
        }
        public static string GroupOn(this TimeScheduleTransactionItem timeScheduleTransactionItem, List<TermGroup_ScheduleTransactionMatrixColumns> columns, bool mergeOnAccount)
        {
            string value = string.Empty;

            foreach (var column in columns)
            {
                switch (column)
                {
                    case TermGroup_ScheduleTransactionMatrixColumns.EmployeeNr:
                        value += $"#{timeScheduleTransactionItem.EmployeeId}";
                        break;
                    case TermGroup_ScheduleTransactionMatrixColumns.EmployeeName:
                        value += $"#{timeScheduleTransactionItem.EmployeeId}{timeScheduleTransactionItem.FirstName}{timeScheduleTransactionItem.LastName}";
                        break;
                    case TermGroup_ScheduleTransactionMatrixColumns.Date:
                        value += $"#{timeScheduleTransactionItem.Date}";
                        break;
                    case TermGroup_ScheduleTransactionMatrixColumns.StartTime:
                        value += $"#{timeScheduleTransactionItem.StartTime}";
                        break;
                    case TermGroup_ScheduleTransactionMatrixColumns.StopTime:
                        value += $"#{timeScheduleTransactionItem.StopTime}";
                        break;
                    /*case TermGroup_ScheduleTransactionMatrixColumns.NetMinutes:
                    case TermGroup_ScheduleTransactionMatrixColumns.NetHours:
                    case TermGroup_ScheduleTransactionMatrixColumns.NetHoursString:

                        if (!mergeOnAccount)
                            value += $"#{timeScheduleTransactionItem.NetQuantity}";
                        else
                            value += $"#";
                        break;
                    case TermGroup_ScheduleTransactionMatrixColumns.GrossMinutes:
                    case TermGroup_ScheduleTransactionMatrixColumns.GrossHours:
                    case TermGroup_ScheduleTransactionMatrixColumns.GrossHoursString:
                        if (!mergeOnAccount)
                            value += $"#{timeScheduleTransactionItem.Quantity}";
                        else
                            value += $"#";
                        break;*/
                    case TermGroup_ScheduleTransactionMatrixColumns.NetCost:
                        value += $"#{timeScheduleTransactionItem.Amount}";
                        break;
                    case TermGroup_ScheduleTransactionMatrixColumns.GrossCost:
                        value += $"#{timeScheduleTransactionItem.GrossAmount}";
                        break;
                    case TermGroup_ScheduleTransactionMatrixColumns.IsBreak:
                        value += $"#{timeScheduleTransactionItem.IsBreak}";
                        break;
                    case TermGroup_ScheduleTransactionMatrixColumns.ExtraShift:
                        value += $"#{timeScheduleTransactionItem.ExtraShift}";
                        break;
                    case TermGroup_ScheduleTransactionMatrixColumns.SubstituteShift:
                        value += $"#{timeScheduleTransactionItem.SubstituteShift}";
                        break;
                    case TermGroup_ScheduleTransactionMatrixColumns.SubstituteShiftCalculated:
                        value += $"#{timeScheduleTransactionItem.SubstituteShiftCalculated}";
                        break;
                    case TermGroup_ScheduleTransactionMatrixColumns.EmploymentPercent:
                        value += $"#{timeScheduleTransactionItem.Date}";
                        break;
                    case TermGroup_ScheduleTransactionMatrixColumns.IsPreliminary:
                        value += $"#{timeScheduleTransactionItem.IsPreliminary}";
                        break;
                    case TermGroup_ScheduleTransactionMatrixColumns.Description:
                        value += $"#{timeScheduleTransactionItem.Description}";
                        break;
                    case TermGroup_ScheduleTransactionMatrixColumns.ShiftTypeName:
                    case TermGroup_ScheduleTransactionMatrixColumns.ShiftTypeScheduleTypeName:
                    case TermGroup_ScheduleTransactionMatrixColumns.ScheduleTypeName:
                        value += $"#{timeScheduleTransactionItem.ShiftTypeId}";
                        break;
                    case TermGroup_ScheduleTransactionMatrixColumns.Created:
                        value += $"#{timeScheduleTransactionItem.Created}";
                        break;
                    case TermGroup_ScheduleTransactionMatrixColumns.Modified:
                        value += $"#{timeScheduleTransactionItem.Modified}";
                        break;
                    case TermGroup_ScheduleTransactionMatrixColumns.CreatedBy:
                        value += $"#{timeScheduleTransactionItem.CreatedBy}";
                        break;
                    case TermGroup_ScheduleTransactionMatrixColumns.ModifiedBy:
                        value += $"#{timeScheduleTransactionItem.ModifiedBy}";
                        break;
                    case TermGroup_ScheduleTransactionMatrixColumns.TimeCodeName:
                    case TermGroup_ScheduleTransactionMatrixColumns.TimeCodeCode:
                        value += $"#{timeScheduleTransactionItem.TimeCodeId}";
                        break;
                    case TermGroup_ScheduleTransactionMatrixColumns.TimeRuleName:
                        value += $"#{timeScheduleTransactionItem.GrossTimeRules?.Count}";
                        break;
                    case TermGroup_ScheduleTransactionMatrixColumns.AccountNr:
                    case TermGroup_ScheduleTransactionMatrixColumns.AccountName:
                        value += $"#{timeScheduleTransactionItem.AccountId}";
                        break;
                    case TermGroup_ScheduleTransactionMatrixColumns.AccountInternalNrs:
                    case TermGroup_ScheduleTransactionMatrixColumns.AccountInternalNames:
                        foreach (var account in timeScheduleTransactionItem.AccountAnalysisFields)
                        {
                            value += $"#{timeScheduleTransactionItem.AccountAnalysisFields[timeScheduleTransactionItem.AccountAnalysisFields.IndexOf(account)].AccountDimId}";
                        }
                        if (!mergeOnAccount)
                            value += $"#{timeScheduleTransactionItem.AccountId}";
                        break;
                    case TermGroup_ScheduleTransactionMatrixColumns.ExtraFieldEmployee:

                        foreach (var extraFiled in timeScheduleTransactionItem.ExtraFieldAnalysisFields)
                        {
                            var extraFieldAnalysisFields = timeScheduleTransactionItem.ExtraFieldAnalysisFields;
                            var efr = extraFieldAnalysisFields[extraFieldAnalysisFields.IndexOf(extraFiled)].ExtraFieldRecord != null ?
                                extraFieldAnalysisFields[extraFieldAnalysisFields.IndexOf(extraFiled)].ExtraFieldRecord.ExtraFieldRecordId.ToString() : "";
                            value += $"#{efr}";
                        }
                        break;
                    default:
                        break;
                }
            }
            
            return value;
        }
    }
}

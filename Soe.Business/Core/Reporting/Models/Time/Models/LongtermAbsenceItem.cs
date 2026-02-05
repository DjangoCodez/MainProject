using SoftOne.Soe.Shared.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using static SoftOne.Soe.Business.Core.ImportExportManager;

namespace SoftOne.Soe.Business.Core.Reporting.Models.Time.Models
{
    public class LongtermAbsenceItem
    {
        public LongtermAbsenceItem()
        {
            AccountAnalysisFields = new List<AccountAnalysisField>();
        }

        public string EmployeeNr { get; set; }
        public string Name { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public List<AccountAnalysisField> AccountAnalysisFields { get; set; }
        public List<AccountAnalysisField> EmployeeAccountAnalysisFields { get; set; }
        public string PayrollTypeLevel1Name { get; set; }
        public string PayrollTypeLevel2Name { get; set; }
        public string PayrollTypeLevel3Name { get; set; }
        public int PayrollTypeLevel1 { get; set; }
        public int PayrollTypeLevel2 { get; set; }
        public int PayrollTypeLevel3 { get; set; }
        public int PayrollTypeLevel4 { get; set; }
        public DateTime StartDateInInterval { get; set; }
        public DateTime StopDateInInterval { get; set; }
        public double NumberOfDaysTotal { get; internal set; }
        public bool EntireSelectedPeriod { get; internal set; }
        public int NumberOfDaysInInterval { get; internal set; }
        public int NumberOfDaysBeforeInterval { get; internal set; }
        public int NumberOfDaysAfterInterval { get; internal set; }
        public DateTime StartDate { get; set; }
        public DateTime StopDate { get; set; }
        public string SocialSec { get; internal set; }
        public decimal Ratio { get; internal set; }

        public DateTime Created { get; set; }
        public DateTime? Modified { get; set; }

        public LongTermAbsenceOutputRow ToLongTermAbsenceOutput()
        {
            return new LongTermAbsenceOutputRow
            {
                EmployeeNr = EmployeeNr,
                Name = Name,
                FirstName = FirstName,
                LastName = LastName,
                PayrollTypeLevel1 = PayrollTypeLevel1,
                PayrollTypeLevel2 = PayrollTypeLevel2,
                PayrollTypeLevel3 = PayrollTypeLevel3,
                PayrollTypeLevel4 = PayrollTypeLevel4,
                StartDateInInterval = StartDateInInterval,
                StopDateInInterval = StopDateInInterval,
                NumberOfDaysTotal = NumberOfDaysTotal,
                EntireSelectedPeriod = EntireSelectedPeriod,
                NumberOfDaysInInterval = NumberOfDaysInInterval,
                NumberOfDaysBeforeInterval = NumberOfDaysBeforeInterval,
                NumberOfDaysAfterInterval = NumberOfDaysAfterInterval,
                StartDate = StartDate,
                StopDate = StopDate, 
                SocialSec = SocialSec,
                Ratio = Ratio != 0 ? Ratio : (decimal?)null,
                Created = Created,
                Modified = Modified,
                TransactionAccounts = AccountAnalysisFields?.Select(s => new LongTermAbsenceAccount()
                {
                    AccountNr = s.AccountNr,
                    AccountName = s.Name,
                    AccountDimNr = s.AccountDimNr
                }
                ).ToList(),
                EmployeeAccounts = AccountAnalysisFields?.Select(s => new LongTermAbsenceAccount()
                {
                    AccountNr = s.AccountNr,
                    AccountName = s.Name,
                    AccountDimNr = s.AccountDimNr
                }
                ).ToList(),
            };
        }
    }
}

using SoftOne.Soe.Common.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftOne.Soe.Business.Core.Reporting.Models.Time.Models
{
    public class TimeTransactionMatrixItem : TimePayrollTransactionDTO
    {
        public TimeTransactionMatrixItem(TimePayrollTransactionDTO timePayrollTransactionDTO)
        {
            TransactionDTO = timePayrollTransactionDTO;
        }
        public TimePayrollTransactionDTO TransactionDTO { get; set; }
        public string ExternalAuthId { get; set; }
        public string UserName { get; set; }
        public string EmployeeExternalCode { get; set; }
        public string PayrollGroup { get; set; }
        public string EmployeeGroup { get; set; }
        public string EmploymentType { get; set; }
        public List<AccountAnalysisField> EmployeeAccountAnalysisFields { get; set; }
        public List<AccountAnalysisField> EmployeeHierchicalAnalysisFields { get; set; }

    }
}

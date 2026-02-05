using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;

namespace SoftOne.Soe.Business.Util.SalaryPaymentAdapters
{
    class BGCLBAdapter : SalaryPaymentBaseAdapter
    {
        public BGCLBAdapter(List<Employee> employees, List<TimePayrollTransaction> transactions, TimePeriod timePeriod)
            : base(employees, transactions, timePeriod)
        {
            base.ExportFormat = SoeTimeSalaryPaymentExportFormat.Text;
            base.ExportType = TermGroup_TimeSalaryPaymentExportType.BGCLB;
        }

        public override byte[] CreateFile()
        {
            throw new NotImplementedException();
        }        
    }
}

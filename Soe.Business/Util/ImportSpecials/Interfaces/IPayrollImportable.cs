using SoftOne.Soe.Common.DTO;
using System;
using System.Collections.Generic;

namespace SoftOne.Soe.Business.Util.ImportSpecials.Interfaces
{
    interface IPayrollImportable
    {
        PayrollImportHeadDTO ParseToPayrollImportHead(int actorCompanyId, byte[] file, DateTime? paymentDate, List<EmployeeDTO> employees, List<AccountDimDTO> accountDims, List<PayrollProductGridDTO> payrollProducts, List<TimeDeviationCauseDTO> timeDeviationCauses);
    }
}


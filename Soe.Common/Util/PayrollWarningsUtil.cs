using System.Collections.Generic;
namespace SoftOne.Soe.Common.Util
{
    public class PayrollWarningsUtil
    {
        #region Stopping warnings
        public static List<TermGroup_PayrollControlFunctionType> GetStoppingPayrollWarnings() => new List<TermGroup_PayrollControlFunctionType>
        {
            TermGroup_PayrollControlFunctionType.NetSalaryDiff,
            TermGroup_PayrollControlFunctionType.NetSalaryNegative,
            TermGroup_PayrollControlFunctionType.PeriodHasNotBeenCalculated,
            TermGroup_PayrollControlFunctionType.GrossSalaryNegative,
            TermGroup_PayrollControlFunctionType.EmploymentTaxDiff,
            TermGroup_PayrollControlFunctionType.SupplementChargeDiff
        };
        #endregion


    }
}

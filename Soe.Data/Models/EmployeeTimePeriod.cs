using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Data
{
    public partial class EmployeeTimePeriod : ICreatedModified, IState
    {

        public PayrollControlFunctionOutcome GetControlFunctionOutcome(TermGroup_PayrollControlFunctionType type)
        {            
            return PayrollControlFunctionOutcome?.FirstOrDefault(x => x.Type == (int)type);
        }
    }

    public partial class EmployeeTimePeriodProductSetting : ICreatedModified, IState
    {

    }

    public partial class EmployeeTimePeriodValue : ICreatedModified, IState
    {

    }

    public static partial class EntityExtensions
    {
        #region EmployeeTimePeriod

        public static EmployeeTimePeriodDTO ToDTO(this EmployeeTimePeriod e)
        {
            if (e == null)
                return null;

            return new EmployeeTimePeriodDTO()
            {
                EmployeeTimePeriodId = e.EmployeeTimePeriodId,
                ActorCompanyId = e.ActorCompanyId,
                EmployeeId = e.EmployeeId,
                TimePeriodId = e.TimePeriodId,
                Status = (SoeEmployeeTimePeriodStatus)e.Status,
                SalarySpecificationPublishDate = e.SalarySpecificationPublishDate,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
            };
        }

        public static IEnumerable<EmployeeTimePeriodDTO> ToDTOs(this IEnumerable<EmployeeTimePeriod> l)
        {
            var dtos = new List<EmployeeTimePeriodDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static List<EmployeeTimePeriod> Filter(this IEnumerable<EmployeeTimePeriod> l, IEnumerable<int> employeeIds, IEnumerable<int> timePeriodIds)
        {
            return l?
                .Where(e => employeeIds.Contains(e.EmployeeId) && timePeriodIds.Contains(e.TimePeriodId))
                .ToList() ?? new List<EmployeeTimePeriod>();
        }

        public static TimePeriod GetTimePeriod(this IEnumerable<EmployeeTimePeriod> l, DateTime date)
        {
            return l?.Select(e => e.TimePeriod).FirstOrDefault(e => e != null && e.StartDate <= date && e.StopDate >= date);
        }

        public static decimal GetGrossSalarySum(this List<EmployeeTimePeriod> l)
        {
            return l.Sum(x => x.GetGrossSalarySum());
        }

        public static decimal GetTaxSum(this List<EmployeeTimePeriod> l)
        {
            return l.Sum(x => x.GetTaxSum());
        }

        public static decimal GetNetSalarySum(this List<EmployeeTimePeriod> l)
        {
            return l.Sum(x => x.GetNetSum());
        }

        public static decimal GetBenefitSum(this List<EmployeeTimePeriod> l)
        {
            return l.Sum(x => x.GetBenefitSum());
        }

        public static decimal GetCompensationSum(this List<EmployeeTimePeriod> l)
        {
            return l.Sum(x => x.GetCompensationSum());
        }

        public static decimal GetDeductionSum(this List<EmployeeTimePeriod> l)
        {
            return l.Sum(x => x.GetDeductionSum());
        }

        public static decimal GetEmploymentTaxCreditSum(this List<EmployeeTimePeriod> l)
        {
            return l.Sum(x => x.GetEmploymentTaxCreditSum());
        }

        public static decimal Getvalue(this EmployeeTimePeriod l, SoeEmployeeTimePeriodValueType valueType)
        {
            if (l.EmployeeTimePeriodValue == null)
                return 0;

            return l.EmployeeTimePeriodValue.ToList().Getvalue(valueType);
        }

        public static decimal GetTaxSum(this EmployeeTimePeriod l)
        {
            if (l.EmployeeTimePeriodValue == null)
                return 0;

            return l.EmployeeTimePeriodValue.ToList().GetTaxSum();
        }

        public static decimal GetTableTaxSum(this EmployeeTimePeriod l)
        {
            if (l.EmployeeTimePeriodValue == null)
                return 0;

            return l.EmployeeTimePeriodValue.ToList().GetTableTaxSum();
        }

        public static decimal GetOneTimeTaxSum(this EmployeeTimePeriod l)
        {
            if (l.EmployeeTimePeriodValue == null)
                return 0;

            return l.EmployeeTimePeriodValue.ToList().GetOneTimeTaxSum();
        }

        public static decimal GetOptionalTaxSum(this EmployeeTimePeriod l)
        {
            if (l.EmployeeTimePeriodValue == null)
                return 0;

            return l.EmployeeTimePeriodValue.ToList().GetOptionalTaxSum();
        }

        public static decimal GetSINKTaxSum(this EmployeeTimePeriod l)
        {
            if (l.EmployeeTimePeriodValue == null)
                return 0;

            return l.EmployeeTimePeriodValue.ToList().GetSinkTaxSum();
        }

        public static decimal GetASINKTaxSum(this EmployeeTimePeriod l)
        {
            if (l.EmployeeTimePeriodValue == null)
                return 0;

            return l.EmployeeTimePeriodValue.ToList().GetASinkTaxSum();
        }

        public static decimal GetEmploymentTaxCreditSum(this EmployeeTimePeriod l)
        {
            if (l.EmployeeTimePeriodValue == null)
                return 0;

            return l.EmployeeTimePeriodValue.ToList().GetEmploymentTaxCreditSum();
        }

        public static decimal GetEmploymentTaxBasisSum(this EmployeeTimePeriod l)
        {
            if (l.EmployeeTimePeriodValue == null)
                return 0;

            return l.EmployeeTimePeriodValue.ToList().GetEmploymentTaxBasisSum();
        }

        public static decimal GetSupplementChargeCreditSum(this EmployeeTimePeriod l)
        {
            if (l.EmployeeTimePeriodValue == null)
                return 0;

            return l.EmployeeTimePeriodValue.ToList().GetSupplementChargeCreditSum();
        }

        public static decimal GetGrossSalarySum(this EmployeeTimePeriod l)
        {
            if (l.EmployeeTimePeriodValue == null)
                return 0;

            return l.EmployeeTimePeriodValue.ToList().GetGrossSalarySum();
        }

        public static decimal GetVacationCompensationSum(this EmployeeTimePeriod l)
        {
            if (l.EmployeeTimePeriodValue == null)
                return 0;

            return l.EmployeeTimePeriodValue.ToList().GetVacationCompensationSum();
        }

        public static decimal GetBenefitSum(this EmployeeTimePeriod l)
        {
            if (l.EmployeeTimePeriodValue == null)
                return 0;

            return l.EmployeeTimePeriodValue.ToList().GetBenefitSum();
        }

        public static decimal GetCompensationSum(this EmployeeTimePeriod l)
        {
            if (l.EmployeeTimePeriodValue == null)
                return 0;

            return l.EmployeeTimePeriodValue.ToList().GetCompensationSum();
        }

        public static decimal GetDeductionSum(this EmployeeTimePeriod l)
        {
            if (l.EmployeeTimePeriodValue == null)
                return 0;

            return l.EmployeeTimePeriodValue.ToList().GetDeductionSum();
        }

        public static decimal GetUnionFeeSum(this EmployeeTimePeriod l)
        {
            if (l.EmployeeTimePeriodValue == null)
                return 0;

            return l.EmployeeTimePeriodValue.ToList().GetUnionFeeSum();
        }

        public static decimal GetNetSum(this EmployeeTimePeriod l)
        {
            if (l.EmployeeTimePeriodValue == null)
                return 0;

            return l.EmployeeTimePeriodValue.ToList().GetNetSum();
        }
        public static bool IsOpenOrHigher(this EmployeeTimePeriod l)
        {
            return (l.Status != (int)SoeEmployeeTimePeriodStatus.None);
        }

        public static bool IsLockOrHigher(this EmployeeTimePeriod l)
        {
            return (l.Status == (int)SoeEmployeeTimePeriodStatus.Locked || l.Status == (int)SoeEmployeeTimePeriodStatus.Paid);
        }

        public static bool HasStoppingWarnings(this EmployeeTimePeriod l)
        {
            return l.PayrollControlFunctionOutcome.Where(x => x.State == (int)(SoeEntityState.Active)).ToList().HasStoppingWarnings();
        }

        #endregion

        #region EmployeeTimePeriodValue

        public static decimal Getvalue(this List<EmployeeTimePeriodValue> l, SoeEmployeeTimePeriodValueType valueType)
        {
            return l?.Where(x => x.State == (int)SoeEntityState.Active && x.Type == (int)valueType).Sum(x => x.Value) ?? 0;
        }

        public static decimal GetTaxSum(this List<EmployeeTimePeriodValue> l)
        {
            return l.Getvalue(SoeEmployeeTimePeriodValueType.TableTax) + l.Getvalue(SoeEmployeeTimePeriodValueType.OneTimeTax) + l.Getvalue(SoeEmployeeTimePeriodValueType.OptionalTax) + l.Getvalue(SoeEmployeeTimePeriodValueType.SINKTax) + l.Getvalue(SoeEmployeeTimePeriodValueType.ASINKTax);
        }

        public static decimal GetTableTaxSum(this List<EmployeeTimePeriodValue> l)
        {
            return l.Getvalue(SoeEmployeeTimePeriodValueType.TableTax);
        }

        public static decimal GetOneTimeTaxSum(this List<EmployeeTimePeriodValue> l)
        {
            return l.Getvalue(SoeEmployeeTimePeriodValueType.OneTimeTax);
        }

        public static decimal GetOptionalTaxSum(this List<EmployeeTimePeriodValue> l)
        {
            return l.Getvalue(SoeEmployeeTimePeriodValueType.OptionalTax);
        }

        public static decimal GetSinkTaxSum(this List<EmployeeTimePeriodValue> l)
        {
            return l.Getvalue(SoeEmployeeTimePeriodValueType.SINKTax);
        }

        public static decimal GetASinkTaxSum(this List<EmployeeTimePeriodValue> l)
        {
            return l.Getvalue(SoeEmployeeTimePeriodValueType.ASINKTax);
        }

        public static decimal GetEmploymentTaxCreditSum(this List<EmployeeTimePeriodValue> l)
        {
            return l.Getvalue(SoeEmployeeTimePeriodValueType.EmploymentTaxCredit);
        }

        public static decimal GetEmploymentTaxBasisSum(this List<EmployeeTimePeriodValue> l)
        {
            return l.Getvalue(SoeEmployeeTimePeriodValueType.EmploymentTaxBasis);
        }

        public static decimal GetSupplementChargeCreditSum(this List<EmployeeTimePeriodValue> l)
        {
            return l.Getvalue(SoeEmployeeTimePeriodValueType.SupplementChargeCredit);
        }

        public static decimal GetGrossSalarySum(this List<EmployeeTimePeriodValue> l)
        {
            return l.Getvalue(SoeEmployeeTimePeriodValueType.GrossSalary);
        }

        public static decimal GetNetSalarySum(this List<EmployeeTimePeriodValue> l)
        {
            return l.Getvalue(SoeEmployeeTimePeriodValueType.NetSalary);
        }

        public static decimal GetVacationCompensationSum(this List<EmployeeTimePeriodValue> l)
        {
            return l.Getvalue(SoeEmployeeTimePeriodValueType.VacationCompensation);
        }

        public static decimal GetBenefitSum(this List<EmployeeTimePeriodValue> l)
        {
            return l.Getvalue(SoeEmployeeTimePeriodValueType.Benefit);
        }

        public static decimal GetCompensationSum(this List<EmployeeTimePeriodValue> l)
        {
            return l.Getvalue(SoeEmployeeTimePeriodValueType.Compensation);
        }

        public static decimal GetDeductionSum(this List<EmployeeTimePeriodValue> l)
        {
            return l.Getvalue(SoeEmployeeTimePeriodValueType.Deduction);
        }

        public static decimal GetUnionFeeSum(this List<EmployeeTimePeriodValue> l)
        {
            return l.Getvalue(SoeEmployeeTimePeriodValueType.UnionFee);
        }

        public static decimal GetNetSum(this List<EmployeeTimePeriodValue> l)
        {
            return l.Getvalue(SoeEmployeeTimePeriodValueType.NetSalary);
        }

        #endregion

        #region EmployeeTimePeriodProductSetting

        public static EmployeeTimePeriodProductSettingDTO ToDTO(this EmployeeTimePeriodProductSetting e)
        {
            if (e == null)
                return null;

            return new EmployeeTimePeriodProductSettingDTO()
            {
                EmployeeTimePeriodProductSettingId = e.EmployeeTimePeriodProductSettingId,
                EmployeeTimePeriodId = e.EmployeeTimePeriodId,
                PayrollProductId = e.PayrollProductId,
                TaxCalculationType = (TermGroup_PayrollProductTaxCalculationType)e.TaxCalculationType,
                PrintOnSalarySpecification = e.PrintOnSalarySpecification,
                UseSettings = e.UseSettings,
                Note = e.Note,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
            };
        }

        public static IEnumerable<EmployeeTimePeriodProductSettingDTO> ToDTOs(this IEnumerable<EmployeeTimePeriodProductSetting> l)
        {
            var dtos = new List<EmployeeTimePeriodProductSettingDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static EmployeeTimePeriodProductSetting GetSetting(this EmployeeTimePeriod l, int payrollproductId)
        {
            return l.EmployeeTimePeriodProductSetting.FirstOrDefault(x => x.PayrollProductId == payrollproductId && x.UseSettings && x.State == (int)SoeEntityState.Active);
        }

        #endregion

        #region PayrollControlFunctionOutcome

        public static bool HasStoppingWarnings(this List<PayrollControlFunctionOutcome> l)
        {
            return l.Any(x => x.IsStoppingPayrollWarning());
        }

        #endregion
    }
}

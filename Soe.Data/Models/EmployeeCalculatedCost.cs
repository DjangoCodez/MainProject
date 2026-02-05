using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Data
{
    public partial class EmployeeCalculatedCost : ICreatedModifiedNotNull, IState
    {

    }

    public static partial class EntityExtensions
    {
        #region EmployeeTaxSE

        public static EmployeeTaxSEDTO ToDTO(this EmployeeTaxSE e, List<GenericType> employeeTaxTypes = null)
        {
            if (e == null)
                return null;

            return new EmployeeTaxSEDTO()
            {
                EmployeeTaxId = e.EmployeeTaxId,
                EmployeeId = e.EmployeeId,
                Year = e.Year,
                MainEmployer = e.MainEmployer,
                Type = (TermGroup_EmployeeTaxType)e.Type,
                TypeName = employeeTaxTypes?.FirstOrDefault(x => x.Id == e.Type)?.Name,
                TaxRate = e.TaxRate,
                TaxRateColumn = e.TaxRateColumn,
                OneTimeTaxPercent = e.OneTimeTaxPercent,
                EstimatedAnnualSalary = e.EstimatedAnnualSalary,
                AdjustmentType = (TermGroup_EmployeeTaxAdjustmentType)e.AdjustmentType,
                AdjustmentValue = e.AdjustmentValue,
                AdjustmentPeriodFrom = e.AdjustmentPeriodFrom,
                AdjustmentPeriodTo = e.AdjustmentPeriodTo,
                SchoolYouthLimitInitial = e.SchoolYouthLimitInitial,
                SinkType = (TermGroup_EmployeeTaxSinkType)e.SinkType,
                EmploymentTaxType = (TermGroup_EmployeeTaxEmploymentTaxType)e.EmploymentTaxType,
                EmploymentAbroadCode = (TermGroup_EmployeeTaxEmploymentAbroadCode)e.EmploymentAbroadCode,
                RegionalSupport = e.RegionalSupport,
                FirstEmployee = e.FirstEmployee,
                SecondEmployee = e.SecondEmployee,
                SalaryDistressAmount = e.SalaryDistressAmount,
                SalaryDistressAmountType = (TermGroup_EmployeeTaxSalaryDistressAmountType)e.SalaryDistressAmountType,
                SalaryDistressReservedAmount = e.SalaryDistressReservedAmount,
                CsrExportDate = e.CSRExportDate,
                CsrImportDate = e.CSRImportDate,
                TinNumber = e.TinNumber,
                CountryCode = e.CountryCode,
                ApplyEmploymentTaxMinimumRule = e.ApplyEmploymentTaxMinimumRule,
                SalaryDistressCase = e.SalaryDistressCase,
                BirthPlace = e.BirthPlace,
                CountryCodeBirthPlace = e.CountryCodeBirthPlace,
                CountryCodeCitizen = e.CountryCodeCitizen,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State,
            };
        }

        public static IEnumerable<EmployeeTaxSEDTO> ToDTOs(this IEnumerable<EmployeeTaxSE> l)
        {
            var dtos = new List<EmployeeTaxSEDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        #endregion
    }
}

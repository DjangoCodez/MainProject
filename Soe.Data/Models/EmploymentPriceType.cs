using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Data
{
    public partial class EmploymentPriceType : ICreatedModified, IState
    {

    }

    public partial class EmploymentPriceTypePeriod : ICreatedModified, IState
    {

    }

    public partial class EmploymentType : ICreatedModified, IState
    {

    }

    public static partial class EntityExtensions
    {
        #region EmploymentPriceType

        public static EmploymentPriceTypeDTO ToDTO(this EmploymentPriceType e, bool includePeriods, bool includeReadOnlyFlag)
        {
            if (e == null)
                return null;

            #region Try load

            if (!e.IsAdded())
            {
                if (!e.PayrollPriceTypeReference.IsLoaded)
                {
                    e.PayrollPriceTypeReference.Load();
                    DataProjectLogCollector.LogLoadedEntityInExtension("EmploymentPriceType.cs e.PayrollPriceTypeReference");
                }
                if (includePeriods && !e.EmploymentPriceTypePeriod.IsLoaded)
                {
                    e.EmploymentPriceTypePeriod.Load();
                    DataProjectLogCollector.LogLoadedEntityInExtension("EmploymentPriceType.cs e.EmploymentPriceTypePeriod");
                }
                if (includeReadOnlyFlag && !e.EmploymentReference.IsLoaded)
                {
                    e.EmploymentReference.Load();
                    DataProjectLogCollector.LogLoadedEntityInExtension("EmploymentPriceType.cs e.EmploymentReference");
                }
            }

            #endregion

            EmploymentPriceTypeDTO dto = new EmploymentPriceTypeDTO()
            {
                EmploymentPriceTypeId = e.EmploymentPriceTypeId,
                EmploymentId = e.EmploymentId,
                EmployeeId = e.Employment.EmployeeId,
                PayrollPriceTypeId = e.PayrollPriceTypeId,
                Code = e.PayrollPriceType?.Code ?? string.Empty,
                Name = e.PayrollPriceType?.Name ?? string.Empty,
                PayrollPriceType = e.PayrollPriceType != null ? (TermGroup_SoePayrollPriceType)e.PayrollPriceType.Type : TermGroup_SoePayrollPriceType.Misc,
                ReadOnly = false,
            };

            if (includePeriods)
                dto.Periods = e.EmploymentPriceTypePeriod?.Where(p => p.State == (int)SoeEntityState.Active).ToDTOs().ToList() ?? new List<EmploymentPriceTypePeriodDTO>();
            dto.Type = e.PayrollPriceType?.ToDTO(false);

            return dto;
        }

        public static IEnumerable<EmploymentPriceTypeDTO> ToDTOs(this IEnumerable<EmploymentPriceType> l, bool includePeriods, bool includeReadOnlyFlag)
        {
            var dtos = new List<EmploymentPriceTypeDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO(includePeriods, includeReadOnlyFlag));
                }
            }
            return dtos;
        }

        public static EmploymentPriceTypePeriod GetEmploymentPriceTypePeriod(this EmploymentPriceType e, DateTime? date)
        {
            if (e.EmploymentPriceTypePeriod == null)
                return null;

            if (!date.HasValue)
                date = DateTime.Today;

            return e.EmploymentPriceTypePeriod.Where(p => p.State == (int)SoeEntityState.Active && (!p.FromDate.HasValue || p.FromDate.Value <= date.Value)).OrderBy(p => p.FromDate).LastOrDefault();
        }

        public static decimal? GetEmploymentPriceTypeAmount(this EmploymentPriceType e, DateTime? date)
        {
            return e.GetEmploymentPriceTypePeriod(date)?.Amount;
        }

        #endregion

        #region EmploymentPriceTypePeriod

        public static EmploymentPriceTypePeriodDTO ToDTO(this EmploymentPriceTypePeriod e)
        {
            if (e == null)
                return null;

            return new EmploymentPriceTypePeriodDTO()
            {
                EmploymentPriceTypePeriodId = e.EmploymentPriceTypePeriodId,
                EmploymentPriceTypeId = e.EmploymentPriceTypeId,
                FromDate = e.FromDate,
                Amount = e.Amount,
                PayrollLevelId = e.PayrollLevelId,
                PayrollLevelName = e.PayrollLevel?.Name ?? string.Empty,
            };
        }

        public static IEnumerable<EmploymentPriceTypePeriodDTO> ToDTOs(this IEnumerable<EmploymentPriceTypePeriod> l)
        {
            var dtos = new List<EmploymentPriceTypePeriodDTO>();
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

using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Data
{
    public partial class PayrollPriceType : ICreatedModified, IState
    {
        public string TypeName { get; set; }
    }

    public static partial class EntityExtensions
    {
        #region PayrollPriceFormula

        public static PayrollPriceFormulaDTO ToDTO(this PayrollPriceFormula e)
        {
            if (e == null)
                return null;

            return new PayrollPriceFormulaDTO()
            {
                PayrollPriceFormulaId = e.PayrollPriceFormulaId,
                ActorCompanyId = e.ActorCompanyId,
                Code = e.Code,
                Name = e.Name,
                Description = e.Description,
                Formula = e.Formula,
                FormulaPlain = e.FormulaPlain,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State,
            };
        }

        public static IEnumerable<PayrollPriceFormulaDTO> ToDTOs(this IEnumerable<PayrollPriceFormula> l)
        {
            var dtos = new List<PayrollPriceFormulaDTO>();
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

        #region PayrollPriceType

        public static PayrollPriceTypeDTO ToDTO(this PayrollPriceType e, bool includePeriods)
        {
            if (e == null)
                return null;

            #region Try load

            try
            {
                if (!e.IsAdded() && includePeriods && !e.PayrollPriceTypePeriod.IsLoaded)
                {
                    e.PayrollPriceTypePeriod.Load();
                    DataProjectLogCollector.LogLoadedEntityInExtension("PayrollPriceType.cs e.PayrollPriceTypePeriod");
                }
            }
            catch (InvalidOperationException ex) { ex.ToString(); }

            #endregion

            PayrollPriceTypeDTO dto = new PayrollPriceTypeDTO()
            {
                PayrollPriceTypeId = e.PayrollPriceTypeId,
                ActorCompanyId = e.ActorCompanyId,
                Type = e.Type,
                TypeName = e.TypeName,
                Code = e.Code,
                Name = e.Name,
                Description = e.Description,
                ConditionAgeYears = e.ConditionAgeYears,
                ConditionEmployeedMonths = e.ConditionEmployedMonths,
                ConditionExperienceMonths = e.ConditionExperienceMonths,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State
            };

            if (includePeriods)
                dto.Periods = e.PayrollPriceTypePeriod?.Where(p => p.State == (int)SoeEntityState.Active).ToDTOs().ToList() ?? new List<PayrollPriceTypePeriodDTO>();

            return dto;
        }

        public static IEnumerable<PayrollPriceTypeDTO> ToDTOs(this IEnumerable<PayrollPriceType> l, bool includePeriods = false)
        {
            var dtos = new List<PayrollPriceTypeDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO(includePeriods));
                }
            }
            return dtos;
        }

        public static PayrollPriceTypeGridDTO ToGridDTO(this PayrollPriceType e)
        {
            if (e == null)
                return null;

            return new PayrollPriceTypeGridDTO()
            {
                PayrollPriceTypeId = e.PayrollPriceTypeId,
                TypeName = e.TypeName,
                Code = e.Code,
                Name = e.Name,
                Description = e.Description,
            };
        }

        public static IEnumerable<PayrollPriceTypeGridDTO> ToGridDTOs(this IEnumerable<PayrollPriceType> l)
        {
            var dtos = new List<PayrollPriceTypeGridDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToGridDTO());
                }
            }
            return dtos;
        }

        public static PayrollPriceTypeSmallDTO ToSmallDTO(this PayrollPriceType e)
        {
            if (e == null)
                return null;

            return new PayrollPriceTypeSmallDTO()
            {
                PayrollPriceTypeId = e.PayrollPriceTypeId,
                Code = e.Code,
                Name = e.Name,
            };
        }

        public static IEnumerable<PayrollPriceTypeSmallDTO> ToSmallDTOs(this IEnumerable<PayrollPriceType> l)
        {
            var dtos = new List<PayrollPriceTypeSmallDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToSmallDTO());
                }
            }
            return dtos;
        }

        public static PayrollPriceType GetPayrollPriceType(this List<PayrollPriceType> l, int payrollPriceTypeId)
        {
            return l?.FirstOrDefault(p => p.PayrollPriceTypeId == payrollPriceTypeId);
        }

        public static PayrollPriceTypePeriod GetPeriod(this PayrollPriceType e, DateTime date)
        {
            return e?.PayrollPriceTypePeriod
             .Where(p => (!p.FromDate.HasValue || p.FromDate.Value <= date) && p.State == (int)SoeEntityState.Active)
             .OrderBy(p => p.FromDate)
             .LastOrDefault();
        }

        #endregion

        #region PayrollPriceTypePeriod

        public static PayrollPriceTypePeriodDTO ToDTO(this PayrollPriceTypePeriod e)
        {
            if (e == null)
                return null;

            return new PayrollPriceTypePeriodDTO()
            {
                PayrollPriceTypePeriodId = e.PayrollPriceTypePeriodId,
                PayrollPriceTypeId = e.PayrollPriceTypeId,
                FromDate = e.FromDate,
                Amount = e.Amount
            };
        }

        public static IEnumerable<PayrollPriceTypePeriodDTO> ToDTOs(this IEnumerable<PayrollPriceTypePeriod> l)
        {
            var dtos = new List<PayrollPriceTypePeriodDTO>();
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

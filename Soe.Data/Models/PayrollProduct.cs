using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SoftOne.Soe.Data
{
    public partial class PayrollProduct : IComparable<PayrollProduct>, ICreatedModified, IState, IPayrollProduct, IPayrollType //NOSONAR
    {
        public string SysPayrollTypeLevel1Name { get; set; }
        public string SysPayrollTypeLevel2Name { get; set; }
        public string SysPayrollTypeLevel3Name { get; set; }
        public string SysPayrollTypeLevel4Name { get; set; }
        public string SysPayrollTypeName
        {
            get
            {
                string sysPayrollTypeName = "";

                if (String.IsNullOrEmpty(SysPayrollTypeLevel1Name))
                    return sysPayrollTypeName;
                sysPayrollTypeName += SysPayrollTypeLevel1Name;

                if (String.IsNullOrEmpty(SysPayrollTypeLevel2Name))
                    return sysPayrollTypeName;
                sysPayrollTypeName += "-" + SysPayrollTypeLevel2Name;

                if (String.IsNullOrEmpty(SysPayrollTypeLevel3Name))
                    return sysPayrollTypeName;
                sysPayrollTypeName += "-" + SysPayrollTypeLevel3Name;

                if (String.IsNullOrEmpty(SysPayrollTypeLevel4Name))
                    return sysPayrollTypeName;
                sysPayrollTypeName += "-" + SysPayrollTypeLevel4Name;

                return sysPayrollTypeName;
            }
        }
        public string ResultTypeText { get; set; }
        public string NumberAndName
        {
            get
            {
                return $"{this.Number}. {this.Name}";
            }
        }
        public string ExternalNumberOrNumber
        {
            get
            {
                return string.IsNullOrEmpty(this.ExternalNumber) ? this.Number : this.ExternalNumber;
            }
        }

        #region IComparable<PayrollProduct>

        /// <summary>
        /// Custom compare method for the PayrollProduct entity, compares the number as an integer if it is possible to convert it, otherwise the number is compared as a string
        /// </summary>
        /// <param name="other">The PayrollProductto compare to</param>
        /// <returns>As any other CompareTo-function</returns>
        public int CompareTo(PayrollProduct other)
        {
            if (Int32.TryParse(this.Number, out int myNo) && Int32.TryParse(other.Number, out int otherNo))
                return myNo.CompareTo(otherNo);
            else
                return this.Number.CompareTo(other.Number);
        }

        #endregion
    }

    public partial class PayrollProductSetting : ICreatedModified, IPayrollProductSetting
    {
        public static readonly string DEFAULT_ACCOUNTINGPRIO = "1=0,2=0,3=0,4=0,5=0,6=0";
    }

    public static partial class EntityExtensions
    {
        #region PayrollProduct

        public static ProductSmallDTO ToSmallDTO(this PayrollProduct e)
        {
            if (e == null)
                return null;

            return new ProductSmallDTO()
            {
                ProductId = e.ProductId,
                Number = e.Number,
                Name = e.Name
            };
        }

        public static IEnumerable<ProductSmallDTO> ToSmallDTOs(this IEnumerable<PayrollProduct> l)
        {
            var dtos = new List<ProductSmallDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToSmallDTO());
                }
            }
            return dtos;
        }

        public static PayrollProductDTO ToDTO(this PayrollProduct e, bool includeSettings, bool includeAccountSettings, bool includePurchaseAccounts, bool includePriceTypes, bool includePriceFormulas)
        {
            if (e == null)
                return null;

            #region Try load

            try
            {
                if (includeSettings && !e.IsAdded() && !e.PayrollProductSetting.IsLoaded)
                {
                    e.PayrollProductSetting.Load();
                    DataProjectLogCollector.LogLoadedEntityInExtension("PayrollProduct.cs e.PayrollProductSetting");
                }
            }
            catch (InvalidOperationException ex) { ex.ToString(); }

            #endregion

            // Create ProductDTO
            ProductDTO dto = e.ToDTO();

            // Create PayrollProductDTO and copy properties from ProductDTO
            PayrollProductDTO ppdto = new PayrollProductDTO();
            var properties = dto.GetType().GetProperties();
            foreach (var property in properties)
            {
                PropertyInfo pi = dto.GetType().GetProperty(property.Name);
                if (pi.CanWrite)
                    property.SetValue(ppdto, pi.GetValue(dto, null), null);
            }

            // Set PayrollProduct specific properties
            ppdto.SysPayrollProductId = e.SysPayrollProductId;
            ppdto.SysPayrollTypeLevel1 = e.SysPayrollTypeLevel1;
            ppdto.SysPayrollTypeLevel2 = e.SysPayrollTypeLevel2;
            ppdto.SysPayrollTypeLevel3 = e.SysPayrollTypeLevel3;
            ppdto.SysPayrollTypeLevel4 = e.SysPayrollTypeLevel4;
            ppdto.ExternalNumber = e.ExternalNumber.NullToEmpty();
            ppdto.ShortName = e.ShortName;
            ppdto.Factor = e.Factor;
            ppdto.ResultType = e.ResultType;
            ppdto.Payed = e.Payed;
            ppdto.ExcludeInWorkTimeSummary = e.ExcludeInWorkTimeSummary;
            ppdto.AverageCalculated = e.AverageCalculated;
            ppdto.Export = e.Export;
            ppdto.IncludeAmountInExport = e.IncludeAmountInExport;
            ppdto.UseInPayroll = e.UseInPayroll;
            ppdto.DontUseFixedAccounting = e.DontUseFixedAccounting;

            // Relations
            if (includeSettings)
                ppdto.Settings = e.PayrollProductSetting?.Where(p => p.State == (int)SoeEntityState.Active).ToDTOs(includeAccountSettings, includePurchaseAccounts, includePriceTypes, includePriceFormulas).ToList() ?? new List<PayrollProductSettingDTO>();

            return ppdto;
        }

        public static IEnumerable<PayrollProductDTO> ToDTOs(this IEnumerable<PayrollProduct> l, bool includeSettings, bool includeAccountSettings, bool includePurchaseAccounts, bool includePriceTypes, bool includePriceFormulas)
        {
            var dtos = new List<PayrollProductDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO(includeSettings, includeAccountSettings, includePurchaseAccounts, includePriceTypes, includePriceFormulas));
                }
            }
            return dtos;
        }

        public static PayrollProductGridDTO ToGridDTO(this PayrollProduct e)
        {
            if (e == null)
                return null;

            return new PayrollProductGridDTO()
            {
                ProductId = e.ProductId,
                Number = e.Number,
                NumberSort = e.NumberSort,
                ExternalNumber = e.ExternalNumber,
                ShortName = e.ShortName,
                Name = e.Name,
                SysPayrollTypeLevel1 = (e.SysPayrollTypeLevel1 ?? 0),
                SysPayrollTypeLevel1Name = e.SysPayrollTypeLevel1Name,
                SysPayrollTypeLevel2 = (e.SysPayrollTypeLevel2 ?? 0),
                SysPayrollTypeLevel2Name = e.SysPayrollTypeLevel2Name,
                SysPayrollTypeLevel3 = (e.SysPayrollTypeLevel3 ?? 0),
                SysPayrollTypeLevel3Name = e.SysPayrollTypeLevel3Name,
                SysPayrollTypeLevel4 = (e.SysPayrollTypeLevel4 ?? 0),
                SysPayrollTypeLevel4Name = e.SysPayrollTypeLevel4Name,
                Factor = e.Factor,
                ResultType = (TermGroup_PayrollResultType)e.ResultType,
                ResultTypeText = e.ResultTypeText,
                Payed = e.Payed,
                ExcludeInWorkTimeSummary = e.ExcludeInWorkTimeSummary,
                AverageCalculated = e.AverageCalculated,
                UseInPayroll = e.UseInPayroll,
                Export = e.Export,
                IncludeAmountInExport = e.IncludeAmountInExport,
                PayrollType = (TermGroup_PayrollType)e.PayrollType,
                SysPayrollTypeName = e.SysPayrollTypeName,
                State = (SoeEntityState)e.State
            };
        }

        public static IEnumerable<PayrollProductGridDTO> ToGridDTOs(this IEnumerable<PayrollProduct> l)
        {
            var dtos = new List<PayrollProductGridDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToGridDTO());
                }
            }
            return dtos;
        }

        public static ProductTimeCodeDTO ToProductTimeCodeDTO(this PayrollProduct e)
        {
            if (e == null)
                return null;

            return new ProductTimeCodeDTO()
            {
                Id = e.ProductId,
                Name = e.Name,
                State = e.State
            };
        }

        public static IEnumerable<ProductTimeCodeDTO> ToProductTimeCodeDTOs(this IEnumerable<PayrollProduct> l)
        {
            var dtos = new List<ProductTimeCodeDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToProductTimeCodeDTO());
                }
            }            
            return dtos;
        }

        public static List<PayrollProduct> FilterSelectableChildPayrollProducts(this List<PayrollProduct> l)
        {
            return l.Where(p =>
               p.SysPayrollTypeLevel1 != (int)TermGroup_SysPayrollType.SE_Tax &&
               p.SysPayrollTypeLevel1 != (int)TermGroup_SysPayrollType.SE_EmploymentTaxCredit &&
               p.SysPayrollTypeLevel1 != (int)TermGroup_SysPayrollType.SE_EmploymentTaxDebit &&
               p.SysPayrollTypeLevel1 != (int)TermGroup_SysPayrollType.SE_SupplementChargeCredit &&
               p.SysPayrollTypeLevel1 != (int)TermGroup_SysPayrollType.SE_SupplementChargeDebit).ToList();
        }

        public static List<PayrollProduct> GetPayrollProducts(this List<PayrollProduct> l, TermGroup_SysPayrollType sysPayrollTypeLevel1, TermGroup_SysPayrollType? sysPayrollTypeLevel2 = null, TermGroup_SysPayrollType? sysPayrollTypeLevel3 = null, TermGroup_SysPayrollType? sysPayrollTypeLevel4 = null)
        {
            return l?
                .Where(p =>
                    (int)sysPayrollTypeLevel1 == p.SysPayrollTypeLevel1 &&
                    (!sysPayrollTypeLevel2.HasValue || (int)sysPayrollTypeLevel2.Value == p.SysPayrollTypeLevel2) &&
                    (!sysPayrollTypeLevel3.HasValue || (int)sysPayrollTypeLevel3.Value == p.SysPayrollTypeLevel3) &&
                    (!sysPayrollTypeLevel4.HasValue || (int)sysPayrollTypeLevel4.Value == p.SysPayrollTypeLevel4))
                .ToList();
        }

        public static List<PayrollProduct> TryAddPayrollProduct(this List<PayrollProduct> l, PayrollProduct e)
        {
            if (e != null)
            {
                if (l == null)
                    l = new List<PayrollProduct> { e };
                else if (!l.Any(i => i.ProductId == e.ProductId))
                    l.Add(e);
            }
            return l;
        }

        public static List<PayrollProduct> TryAddPayrollProducts(this List<PayrollProduct> l, List<PayrollProduct> l2)
        {
            if (!l2.IsNullOrEmpty())
            {
                foreach (var e in l2)
                {
                    l.TryAddPayrollProduct(e);
                }
            }
            return l;
        }

        public static PayrollProduct GetPayrollProduct(this List<PayrollProduct> l, int productId, bool includeInactive = false)
        {
            return l?.FirstOrDefault(p => p.ProductId == productId && (includeInactive || p.State == (int)SoeEntityState.Active));
        }

        public static PayrollProduct GetPayrollProduct(this List<PayrollProduct> l, string number, bool includeInactive = false)
        {
            return l?.FirstOrDefault(p => p.Number == number && (includeInactive || p.State == (int)SoeEntityState.Active));
        }

        public static PayrollProduct GetPayrollProduct(this List<PayrollProduct> l, TermGroup_SysPayrollType sysPayrollTypeLevel1, TermGroup_SysPayrollType? sysPayrollTypeLevel2 = null, TermGroup_SysPayrollType? sysPayrollTypeLevel3 = null, TermGroup_SysPayrollType? sysPayrollTypeLevel4 = null)
        {
            return l?.FirstOrDefault(p =>
                (int)sysPayrollTypeLevel1 == p.SysPayrollTypeLevel1 &&
                (!sysPayrollTypeLevel2.HasValue || (int)sysPayrollTypeLevel2.Value == p.SysPayrollTypeLevel2) &&
                (!sysPayrollTypeLevel3.HasValue || (int)sysPayrollTypeLevel3.Value == p.SysPayrollTypeLevel3) &&
                (!sysPayrollTypeLevel4.HasValue || (int)sysPayrollTypeLevel4.Value == p.SysPayrollTypeLevel4));
        }

        public static PayrollProduct GetPayrollProduct(this List<PayrollProduct> l, int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return l?.FirstOrDefault(p =>
                (!sysPayrollTypeLevel1.HasValue || sysPayrollTypeLevel1.Value == p.SysPayrollTypeLevel1) &&
                (!sysPayrollTypeLevel2.HasValue || sysPayrollTypeLevel2.Value == p.SysPayrollTypeLevel2) &&
                (!sysPayrollTypeLevel3.HasValue || sysPayrollTypeLevel3.Value == p.SysPayrollTypeLevel3) &&
                (!sysPayrollTypeLevel4.HasValue || sysPayrollTypeLevel4.Value == p.SysPayrollTypeLevel4));
        }

        public static PayrollProduct FirstOrDefault(this IQueryable<PayrollProduct> oQuery, string number, int actorCompanyId, bool includeInactive)
        {
            return oQuery.FirstOrDefault(p =>
                p.Company.Any(c => c.ActorCompanyId == actorCompanyId) &&
                p.Number == number &&
                (includeInactive ? p.State != (int)SoeEntityState.Deleted : p.State == (int)SoeEntityState.Active));
        }

        public static PayrollProduct FirstOrDefault(this IQueryable<PayrollProduct> oQuery, int productId, bool includeInactive)
        {
            return oQuery.FirstOrDefault(p =>
                p.ProductId == productId &&
                (includeInactive ? p.State != (int)SoeEntityState.Deleted : p.State == (int)SoeEntityState.Active));
        }

        public static IQueryable<PayrollProduct> Filter(this IQueryable<PayrollProduct> query, int actorCompanyId)
        {
            return query.Where(p =>
                p.Company.Any(c => c.ActorCompanyId == actorCompanyId) &&
                p.State == (int)SoeEntityState.Active);
        }

        public static IQueryable<PayrollProduct> FilterLevels(this IQueryable<PayrollProduct> query, TermGroup_SysPayrollType? sysPayrollTypeLevel1, TermGroup_SysPayrollType? sysPayrollTypeLevel2 = null, TermGroup_SysPayrollType? sysPayrollTypeLevel3 = null, TermGroup_SysPayrollType? sysPayrollTypeLevel4 = null)
        {
            return query.FilterLevels((int?)sysPayrollTypeLevel1, (int?)sysPayrollTypeLevel2, (int?)sysPayrollTypeLevel3, (int?)sysPayrollTypeLevel4);
        }

        public static IQueryable<PayrollProduct> FilterLevels(this IQueryable<PayrollProduct> query, int? sysPayrollTypeLevel1, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            if (sysPayrollTypeLevel1.HasValue)
                query = query.Where(pp => pp.SysPayrollTypeLevel1 == sysPayrollTypeLevel1.Value);
            if (sysPayrollTypeLevel2.HasValue)
                query = query.Where(pp => pp.SysPayrollTypeLevel2 == sysPayrollTypeLevel2.Value);
            if (sysPayrollTypeLevel3.HasValue)
                query = query.Where(pp => pp.SysPayrollTypeLevel3 == sysPayrollTypeLevel3.Value);
            if (sysPayrollTypeLevel4.HasValue)
                query = query.Where(pp => pp.SysPayrollTypeLevel4 == sysPayrollTypeLevel4.Value);
            return query;
        }

        public static string GetTurnedNumberForVacation(this PayrollProduct e)
        {
            if (e != null)
            {
                switch (e.Number)
                {
                    case "13230":
                        return "13285";
                    case "13231":
                        return "13286";
                    case "13232":
                        return "13287";
                    case "13240":
                        return "13292";
                    case "13241":
                        return "13293";
                    case "13310":
                        return "13385";
                    case "13311":
                        return "13386";
                    case "13312":
                        return "13387";
                    case "13313":
                        return "13388";
                }
            }

            return null;
        }

        public static bool IsValidForAddedTransactionDialog(this PayrollProduct e)
        {
            if (!e.UseInPayroll)
                return false;

            if (PayrollRulesUtil.IsVacationSalary(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4))
                return false;

            return true;
        }

        public static int GetCoherentIntervalDays(this PayrollProduct e)
        {
            return e != null && e.IsAbsenceSickOrWorkInjury() ? Constants.SICKNESS_RELAPSEDAYS : 1;
        }

        public static void SetProperties(this PayrollProduct e, IPayrollProduct input)
        {
            if (e == null || input == null)
                return;

            // Product
            e.ProductUnitId = input.ProductUnitId.ToNullable();
            e.ProductGroupId = input.ProductGroupId.ToNullable();
            e.Type = (int)SoeProductType.PayrollProduct;
            e.Number = input.Number;
            e.Name = input.Name;
            e.ExternalNumber = input.ExternalNumber ?? "";
            e.Description = input.Description;
            e.AccountingPrio = "1=0,2=0,3=0,4=0,5=0,6=0"; // Not used, moved to PayrollProductSetting
            e.State = input.State;

            // PayrollProduct
            e.SysPayrollProductId = input.SysPayrollProductId;
            e.ShortName = input.ShortName;
            e.PayrollType = input.PayrollType;
            e.ResultType = input.ResultType;
            e.SysPayrollTypeLevel1 = input.SysPayrollTypeLevel1;
            e.SysPayrollTypeLevel2 = input.SysPayrollTypeLevel2;
            e.SysPayrollTypeLevel3 = input.SysPayrollTypeLevel3;
            e.SysPayrollTypeLevel4 = input.SysPayrollTypeLevel4;
            e.Factor = input.Factor;
            e.AverageCalculated = input.AverageCalculated;
            e.DontUseFixedAccounting = input.DontUseFixedAccounting;
            e.ExcludeInWorkTimeSummary = input.ExcludeInWorkTimeSummary;
            e.Export = input.Export;
            e.IncludeAmountInExport = input.IncludeAmountInExport;
            e.Payed = input.Payed;
            e.UseInPayroll = input.UseInPayroll;
        }

        public static void SetProperties(this PayrollProductSetting e, IPayrollProductSetting input, bool setKeys = true)
        {
            if (e == null || input == null)
                return;

            if (setKeys)
                e.ChildProductId = input.ChildProductId.ToNullable();

            e.QuantityRoundingMinutes = input.QuantityRoundingMinutes;
            e.QuantityRoundingType = input.QuantityRoundingType;
            e.CentRoundingType = input.CentRoundingType;
            e.CentRoundingLevel = input.CentRoundingLevel;
            e.TimeUnit = input.TimeUnit;
            e.TaxCalculationType = input.TaxCalculationType;
            e.PensionCompany = input.PensionCompany;
            e.AccountingPrio = !String.IsNullOrEmpty(input.AccountingPrio) ? input.AccountingPrio : PayrollProductSetting.DEFAULT_ACCOUNTINGPRIO;
            e.CalculateSupplementCharge = input.CalculateSupplementCharge;
            e.CalculateSicknessSalary = input.CalculateSicknessSalary;
            e.DontPrintOnSalarySpecificationWhenZeroAmount = input.DontPrintOnSalarySpecificationWhenZeroAmount;
            e.DontIncludeInRetroactivePayroll = input.DontIncludeInRetroactivePayroll;
            e.DontIncludeInAbsenceCost = input.DontIncludeInAbsenceCost;
            e.PrintOnSalarySpecification = input.PrintOnSalarySpecification;
            e.PrintDate = input.PrintDate;
            e.UnionFeePromoted = input.UnionFeePromoted;
            e.VacationSalaryPromoted = input.VacationSalaryPromoted;
            e.WorkingTimePromoted = input.WorkingTimePromoted;
        }

        public static void ValidateTimeUnitForSettings(this PayrollProduct e)
        {
            if (e == null)
                return;

            //Set default if TimeUnit is invalid
            foreach (PayrollProductSetting setting in e.PayrollProductSetting.Where(s => s.State == (int)SoeEntityState.Active).ToList())
            {
                if (e.ResultType == (int)TermGroup_PayrollResultType.Quantity)
                    setting.TimeUnit = (int)TermGroup_PayrollProductTimeUnit.Hours;

                if (setting.PayrollProduct.IsAbsenceVacation() || setting.PayrollProduct.IsVacationAddition() || setting.PayrollProduct.IsVacationSalary())
                {
                    if (setting.TimeUnit != (int)TermGroup_PayrollProductTimeUnit.Hours && setting.TimeUnit != (int)TermGroup_PayrollProductTimeUnit.WorkDays && setting.TimeUnit != (int)TermGroup_PayrollProductTimeUnit.CalenderDays && setting.TimeUnit != (int)TermGroup_PayrollProductTimeUnit.VacationCoefficient)
                        setting.TimeUnit = (int)TermGroup_PayrollProductTimeUnit.VacationCoefficient;
                }
                else
                {
                    if (setting.TimeUnit != (int)TermGroup_PayrollProductTimeUnit.Hours && setting.TimeUnit != (int)TermGroup_PayrollProductTimeUnit.WorkDays && setting.TimeUnit != (int)TermGroup_PayrollProductTimeUnit.CalenderDays && setting.TimeUnit != (int)TermGroup_PayrollProductTimeUnit.CalenderDayFactor)
                        setting.TimeUnit = (int)TermGroup_PayrollProductTimeUnit.Hours;
                }
            }
        }

        #endregion

        #region PayrollProductAccountStd

        public static AccountInternal GetAccountInternal(this PayrollProductAccountStd e, int accountDimId)
        {
            try
            {
                foreach (var accountInternal in e.AccountInternal)
                {
                    if (!accountInternal.AccountReference.IsLoaded)
                    {
                        accountInternal.AccountReference.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("PayrollProduct.cs accountInternal.AccountReference");
                    }
                }
            }
            catch (Exception ex) { ex.ToString(); }

            return e.AccountInternal?.FirstOrDefault(i => i.Account?.AccountDimId == accountDimId);
        }

        #endregion

        #region PayrollProductPriceFormula

        public static PayrollProductPriceFormulaDTO ToDTO(this PayrollProductPriceFormula e)
        {
            if (e == null)
                return null;

            return new PayrollProductPriceFormulaDTO()
            {
                PayrollProductPriceFormulaId = e.PayrollProductPriceFormulaId,
                PayrollProductSettingId = e.PayrollProductSettingId,
                PayrollPriceFormulaId = e.PayrollPriceFormulaId,
                FromDate = e.FromDate,
                ToDate = e.ToDate,
                FormulaName = e.PayrollPriceFormula?.Name ?? string.Empty,
            };
        }

        public static IEnumerable<PayrollProductPriceFormulaDTO> ToDTOs(this IEnumerable<PayrollProductPriceFormula> l)
        {
            var dtos = new List<PayrollProductPriceFormulaDTO>();
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

        #region PayrollProductPriceType

        public static PayrollProductPriceTypeDTO ToDTO(this PayrollProductPriceType e, bool includePeriods)
        {
            if (e == null)
                return null;

            #region Try load

            try
            {
                if (includePeriods && !e.IsAdded())
                {
                    if (!e.PayrollPriceTypeReference.IsLoaded)
                    {
                        e.PayrollPriceTypeReference.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("PayrollProduct.cs e.PayrollPriceTypeReference");
                    }
                    if (e.PayrollPriceType != null && !e.PayrollPriceType.PayrollPriceTypePeriod.IsLoaded)
                    {
                        e.PayrollPriceType.PayrollPriceTypePeriod.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("PayrollProduct.cs e.PayrollPriceType.PayrollPriceTypePeriod");
                    }
                    if (!e.PayrollProductPriceTypePeriod.IsLoaded)
                    {
                        e.PayrollProductPriceTypePeriod.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("PayrollProduct.cs e.PayrollProductPriceTypePeriod");
                    }
                }
            }
            catch (InvalidOperationException ex) { ex.ToString(); }

            #endregion

            PayrollProductPriceTypeDTO dto = new PayrollProductPriceTypeDTO()
            {
                PayrollProductPriceTypeId = e.PayrollProductPriceTypeId,
                PayrollProductSettingId = e.PayrollProductSettingId,
                PayrollPriceTypeId = e.PayrollPriceTypeId,
                PriceTypeName = e.PayrollPriceType?.Name ?? string.Empty,
            };

            // Relations
            if (includePeriods)
            {
                dto.Periods = e.PayrollProductPriceTypePeriod?.Where(p => p.State == (int)SoeEntityState.Active).ToDTOs().ToList() ?? new List<PayrollProductPriceTypePeriodDTO>();
                dto.PriceTypePeriods = e.PayrollPriceType?.PayrollPriceTypePeriod?.Where(p => p.State == (int)SoeEntityState.Active).ToDTOs().ToList();
            }

            return dto;
        }

        public static IEnumerable<PayrollProductPriceTypeDTO> ToDTOs(this IEnumerable<PayrollProductPriceType> l, bool includePeriods)
        {
            var dtos = new List<PayrollProductPriceTypeDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO(includePeriods));
                }
            }
            return dtos;
        }

        #endregion

        #region PayrollProductPriceTypePeriod

        public static PayrollProductPriceTypePeriodDTO ToDTO(this PayrollProductPriceTypePeriod e)
        {
            if (e == null)
                return null;

            return new PayrollProductPriceTypePeriodDTO()
            {
                PayrollProductPriceTypePeriodId = e.PayrollProductPriceTypePeriodId,
                PayrollProductPriceTypeId = e.PayrollProductPriceTypeId,
                FromDate = e.FromDate,
                Amount = e.Amount
            };
        }

        public static IEnumerable<PayrollProductPriceTypePeriodDTO> ToDTOs(this IEnumerable<PayrollProductPriceTypePeriod> l)
        {
            var dtos = new List<PayrollProductPriceTypePeriodDTO>();
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

        #region PayrollProductSetting

        public static PayrollProductSettingDTO ToDTO(this PayrollProductSetting e, bool includeAccountSettings, bool includePurchaseAccounts, bool includePriceTypes, bool includePriceFormulas)
        {
            if (e == null)
                return null;

            #region Try load

            try
            {
                if (!e.IsAdded())
                {
                    if (includeAccountSettings)
                    {
                        if (!e.PayrollProductAccountStd.IsLoaded)
                        {
                            e.PayrollProductAccountStd.Load();
                            DataProjectLogCollector.LogLoadedEntityInExtension("PayrollProduct.cs e.PayrollProductAccountStd");
                        }
                        if (e.PayrollProductAccountStd != null && e.PayrollProductAccountStd.Count > 0)
                        {
                            foreach (var accStd in e.PayrollProductAccountStd)
                            {
                                if (!accStd.AccountStdReference.IsLoaded)
                                {
                                    accStd.AccountStdReference.Load();
                                    DataProjectLogCollector.LogLoadedEntityInExtension("PayrollProduct.cs accStd.AccountStdReference");
                                }
                                if (accStd.AccountStd != null && !accStd.AccountStd.AccountReference.IsLoaded)
                                {
                                    accStd.AccountStd.AccountReference.Load();
                                    DataProjectLogCollector.LogLoadedEntityInExtension("PayrollProduct.cs accStd.AccountStd.AccountReference");
                                }
                                if (!accStd.AccountInternal.IsLoaded)
                                {
                                    accStd.AccountInternal.Load();
                                    DataProjectLogCollector.LogLoadedEntityInExtension("PayrollProduct.cs accStd.AccountInternal");
                                }
                                if (accStd.AccountInternal != null && accStd.AccountInternal.Count > 0)
                                {
                                    foreach (var accInt in accStd.AccountInternal)
                                    {
                                        if (!accInt.AccountReference.IsLoaded)
                                        {
                                            accInt.AccountReference.Load();
                                            DataProjectLogCollector.LogLoadedEntityInExtension("PayrollProduct.cs accInt.AccountReference");
                                        }
                                        if (accInt.Account != null && !accInt.Account.AccountDimReference.IsLoaded)
                                        {
                                            accInt.Account.AccountDimReference.Load();
                                            DataProjectLogCollector.LogLoadedEntityInExtension("PayrollProduct.cs accInt.Account.AccountDimReference");
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (includePriceTypes)
                    {
                        if (!e.PayrollProductPriceType.IsLoaded)
                        {
                            e.PayrollProductPriceType.Load();
                            DataProjectLogCollector.LogLoadedEntityInExtension("PayrollProduct.cs e.PayrollProductPriceType");
                        }
                        foreach (var priceType in e.PayrollProductPriceType)
                        {
                            if (!priceType.PayrollProductPriceTypePeriod.IsLoaded)
                            {
                                priceType.PayrollProductPriceTypePeriod.Load();
                                DataProjectLogCollector.LogLoadedEntityInExtension("PayrollProduct.cs priceType.PayrollProductPriceTypePeriod");
                            }
                        }
                    }

                    if (includePriceFormulas && !e.PayrollProductPriceFormula.IsLoaded)
                    {
                        e.PayrollProductPriceFormula.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("PayrollProduct.cs e.PayrollProductPriceFormula");
                    }
                }
            }
            catch (InvalidOperationException ex) { ex.ToString(); }

            #endregion

            PayrollProductSettingDTO dto = new PayrollProductSettingDTO()
            {
                PayrollProductSettingId = e.PayrollProductSettingId,
                ProductId = e.ProductId,
                ChildProductId = e.ChildProductId,
                PayrollGroupId = e.PayrollGroupId,

                QuantityRoundingMinutes = e.QuantityRoundingMinutes,
                QuantityRoundingType = e.QuantityRoundingType,
                CentRoundingType = e.CentRoundingType,
                CentRoundingLevel = e.CentRoundingLevel,
                TaxCalculationType = e.TaxCalculationType,
                TimeUnit = e.TimeUnit,
                PensionCompany = e.PensionCompany,
                AccountingPrio = e.AccountingPrio,
                CalculateSupplementCharge = e.CalculateSupplementCharge,
                CalculateSicknessSalary = e.CalculateSicknessSalary,
                DontPrintOnSalarySpecificationWhenZeroAmount = e.DontPrintOnSalarySpecificationWhenZeroAmount,
                DontIncludeInRetroactivePayroll = e.DontIncludeInRetroactivePayroll,
                DontIncludeInAbsenceCost = e.DontIncludeInAbsenceCost,
                PrintOnSalarySpecification = e.PrintOnSalarySpecification,
                PrintDate = e.PrintDate,
                UnionFeePromoted = e.UnionFeePromoted,
                VacationSalaryPromoted = e.VacationSalaryPromoted,
                WorkingTimePromoted = e.WorkingTimePromoted,

                PayrollGroupName = e.PayrollGroup?.Name ?? string.Empty,
                IsReadOnly = !e.PayrollGroupId.HasValue,
            };

            // Relations
            if (includePriceTypes)
                dto.PriceTypes = e.PayrollProductPriceType?.Where(p => p.State == (int)SoeEntityState.Active).ToDTOs(true).ToList() ?? new List<PayrollProductPriceTypeDTO>();
            if (includePriceFormulas)
                dto.PriceFormulas = e.PayrollProductPriceFormula?.Where(p => p.State == (int)SoeEntityState.Active).ToDTOs().ToList() ?? new List<PayrollProductPriceFormulaDTO>();

            if (includePurchaseAccounts)
            {
                // Purchase
                dto.PurchaseAccounts = new Dictionary<int, AccountSmallDTO>();

                if (!e.PayrollProductAccountStd.IsNullOrEmpty())
                {
                    PayrollProductAccountStd accStd = e.PayrollProductAccountStd.FirstOrDefault(c => c.Type == (int)ProductAccountType.Purchase);
                    Account account = accStd?.AccountStd?.Account;
                    if (account != null)
                        dto.PurchaseAccounts.Add(Constants.ACCOUNTDIM_STANDARD, account.ToSmallDTO());

                    if (accStd?.AccountInternal != null)
                    {
                        foreach (var accInt in accStd.AccountInternal.Where(a => a.Account?.AccountDim != null && a.Account.AccountDim.AccountDimNr != Constants.ACCOUNTDIM_STANDARD))
                        {
                            dto.PurchaseAccounts.Add(accInt.Account.AccountDim.AccountDimNr, accInt.Account.ToSmallDTO());
                        }
                    }
                }
            }
            else if (includeAccountSettings)
            {
                dto.AccountingSettings = new List<AccountingSettingsRowDTO>();
                if (!e.PayrollProductAccountStd.IsNullOrEmpty())
                    AddAccountingSettingsRowDTO(e, dto, ProductAccountType.Purchase);
            }

            return dto;
        }

        public static IEnumerable<PayrollProductSettingDTO> ToDTOs(this IEnumerable<PayrollProductSetting> l, bool includeAccountSettings, bool includePurchaseAccounts, bool includePriceTypes, bool includePriceFormulas)
        {
            var dtos = new List<PayrollProductSettingDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO(includeAccountSettings, includePurchaseAccounts, includePriceTypes, includePriceFormulas));
                }
            }
            return dtos;
        }

        public static List<PayrollProductSetting> GetSettings(this List<PayrollProduct> l)
        {
            return l?.SelectMany(e => e?.PayrollProductSetting?.Where(i => i.State == (int)SoeEntityState.Active)).ToList() ?? new List<PayrollProductSetting>();
        }

        public static PayrollProductSetting GetSetting(this PayrollProduct e, int? payrollGroupId, bool getDefaultIfNotFound = true)
        {
            if (e == null || e.PayrollProductSetting.IsNullOrEmpty())
                return null;

            PayrollProductSetting payrollProductSetting = null;
            if (payrollGroupId.HasValue)
                payrollProductSetting = e.PayrollProductSetting.FirstOrDefault(i => i.State == (int)SoeEntityState.Active && i.PayrollGroupId.HasValue && i.PayrollGroupId.Value == payrollGroupId.Value);
            if (payrollProductSetting == null && getDefaultIfNotFound)
                payrollProductSetting = e.PayrollProductSetting.FirstOrDefault(i => i.State == (int)SoeEntityState.Active && !i.PayrollGroupId.HasValue);

            return payrollProductSetting;
        }

        public static PayrollProductSetting GetSetting(this List<PayrollProductSetting> e, int? payrollGroupId, int payrollProductId, bool getDefaultIfNotFound = true)
        {
            if (e == null)
                return null;

            List<PayrollProductSetting> settingsOnProduct = e.Where(w => w.ProductId == payrollProductId).ToList();
            PayrollProductSetting payrollProductSetting = null;
            if (payrollGroupId.HasValue)
                payrollProductSetting = settingsOnProduct.FirstOrDefault(i => i.PayrollGroupId.HasValue && i.PayrollGroupId.Value == payrollGroupId.Value);
            if (payrollProductSetting == null && getDefaultIfNotFound)
                payrollProductSetting = settingsOnProduct.FirstOrDefault(i => !i.PayrollGroupId.HasValue);
            return payrollProductSetting;
        }

        public static PayrollProductSettingDTO GetSetting(this List<PayrollProductSettingDTO> e, int? payrollGroupId, int payrollProductId, bool getDefaultIfNotFound = true)
        {
            if (e == null)
                return null;

            List<PayrollProductSettingDTO> settingsOnProduct = e.Where(w => w.ProductId == payrollProductId).ToList();
            PayrollProductSettingDTO payrollProductSetting = null;
            if (payrollGroupId.HasValue)
                payrollProductSetting = settingsOnProduct.FirstOrDefault(i => i.PayrollGroupId.HasValue && i.PayrollGroupId.Value == payrollGroupId.Value);
            if (payrollProductSetting == null && getDefaultIfNotFound)
                payrollProductSetting = settingsOnProduct.FirstOrDefault(i => !i.PayrollGroupId.HasValue);
            return payrollProductSetting;
        }

        private static void AddAccountingSettingsRowDTO(PayrollProductSetting setting, PayrollProductSettingDTO dto, ProductAccountType type)
        {
            PayrollProductAccountStd payrollProductSettingAccountStd = setting.PayrollProductAccountStd.FirstOrDefault(c => c.Type == (int)type);
            if (payrollProductSettingAccountStd == null)
                return;

            AccountingSettingsRowDTO accountingDto = new AccountingSettingsRowDTO()
            {
                Type = (int)type,
                Percent = payrollProductSettingAccountStd.Percent ?? 0
            };
            dto.AccountingSettings.Add(accountingDto);

            //AccountStd
            if (payrollProductSettingAccountStd.AccountStd != null)
            {
                accountingDto.AccountDim1Nr = Constants.ACCOUNTDIM_STANDARD;
                if (payrollProductSettingAccountStd.AccountStd.Account?.State == (int)SoeEntityState.Active)
                {
                    accountingDto.Account1Id = payrollProductSettingAccountStd.AccountStd.AccountId;
                    accountingDto.Account1Nr = payrollProductSettingAccountStd.AccountStd.Account.AccountNr;
                    accountingDto.Account1Name = payrollProductSettingAccountStd.AccountStd.Account.Name;
                }
            }

            //AccountInternal
            if (payrollProductSettingAccountStd.AccountInternal != null)
            {
                int position = 2;
                foreach (var accountInternal in payrollProductSettingAccountStd.AccountInternal.Where(a => a.Account?.State == (int)SoeEntityState.Active && a.Account.AccountDim != null && a.Account.AccountDim.AccountDimNr != Constants.ACCOUNTDIM_STANDARD).OrderBy(a => a.Account.AccountDim.AccountDimNr))
                {
                    accountingDto.SetAccountValues(position, accountInternal.Account.AccountDim.AccountDimNr, accountInternal.Account.AccountId, accountInternal.Account.AccountNr, accountInternal.Account.Name);
                    position++;
                }
            }
        }

        #endregion

        #region PayrollProductReportSetting

        public static List<PayrollProductReportSetting> GetPayrollProductReportSettingsForGroup(this List<PayrollProductReportSetting> payrollProductReportSettings, int? payrollGroupId, int payrollProductId)
        {
            List<PayrollProductReportSetting> filtered = new List<PayrollProductReportSetting>();

            var payrollProductSetting = payrollProductReportSettings.Select(s => s.PayrollProductSetting).ToList().GetSetting(payrollGroupId, payrollProductId, true);

            if (payrollProductSetting == null)
                return filtered;

            foreach (var e in payrollProductReportSettings.Where(w => w.State == (int)SoeEntityState.Active))
            {
                if (e.PayrollProductSettingId == payrollProductSetting.PayrollProductSettingId)
                    filtered.Add(e);
            }

            return filtered;

        }
        public static bool IncludeInReport(this List<PayrollProductReportSetting> payrollProductReportSettings, int? reportId, TermGroup_PayrollProductReportSettingType type, int payrollProductId, int? payrollGroupId, string field = null)
        {
            if (payrollProductReportSettings == null || !reportId.HasValue)
                return false;
            PayrollProductReportSetting reportSetting = null;

            if (reportId.HasValue)
            {
                var filteredPayrollProductReportSettings = payrollProductReportSettings.GetPayrollProductReportSettingsForGroup(payrollGroupId, payrollProductId);


                if (type == TermGroup_PayrollProductReportSettingType.Unknown && !string.IsNullOrEmpty(field)) // special case for report settings with field and type unknown
                    reportSetting = filteredPayrollProductReportSettings.FirstOrDefault(i => i.ReportId == reportId && !string.IsNullOrEmpty(i.Field) && i.Field == field);
                else //use setting when type is known
                    reportSetting = filteredPayrollProductReportSettings.FirstOrDefault(i => i.ReportId == reportId && i.Type == (int)type);
            }
            
            if (reportSetting == null && field.HasValue())
            {
                var filteredPayrollProductReportSettings = payrollProductReportSettings.Where(w => (!w.PayrollProductSetting.PayrollGroupId.HasValue || w.PayrollProductSetting.PayrollGroupId == payrollGroupId) && w.Type == (int)TermGroup_PayrollProductReportSettingType.NotReportSpecificField && w.Field.HasValue() && w.Field == field).ToList();
                reportSetting = filteredPayrollProductReportSettings.FirstOrDefault(i => i.PayrollProductSetting.ProductId == payrollProductId);
            }

            if (reportSetting == null)
                return false;
            return reportSetting.Included;
        }

        #endregion
    }
}

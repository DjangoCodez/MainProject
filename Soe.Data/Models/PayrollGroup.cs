using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Data.Util;

namespace SoftOne.Soe.Data
{
    public partial class PayrollGroup : ICreatedModified, IState
    {
        public List<string> ExternalCodes { get; set; }
        public string ExternalCodesString { get; set; }
        public bool ExternalCodesIsLoaded { get; set; }
    }

    public static partial class EntityExtensions
    {
        #region PayrollGroup

        public static PayrollGroupDTO ToDTO(this PayrollGroup e, bool includePriceTypes, bool includePriceFormulas, bool includeSettings, bool includePayrollGroupReports, bool includeTimePeriod, bool includeAccounts, bool includePayrollGroupVacationGroup, bool includePayrollGroupPayrollProduct)
        {
            if (e == null)
                return null;

            #region Try load

            try
            {
                if (!e.IsAdded())
                {
                    if (includePriceTypes)
                    {
                        if (!e.PayrollGroupPriceType.IsLoaded)
                        {
                            e.PayrollGroupPriceType.Load();
                            DataProjectLogCollector.LogLoadedEntityInExtension("PayrollGroup.cs e.PayrollGroupPriceType");
                        }
                        foreach (var priceType in e.PayrollGroupPriceType)
                        {
                            if (!priceType.PayrollPriceTypeReference.IsLoaded)
                            {
                                priceType.PayrollPriceTypeReference.Load();
                                DataProjectLogCollector.LogLoadedEntityInExtension("PayrollGroup.cs priceType.PayrollPriceTypeReference");
                            }
                            if (!priceType.PayrollGroupPriceTypePeriod.IsLoaded)
                            {
                                priceType.PayrollGroupPriceTypePeriod.Load();
                                DataProjectLogCollector.LogLoadedEntityInExtension("PayrollGroup.cs priceType.PayrollGroupPriceTypePeriod");
                            }
                        }
                    }
                    if (includeSettings && !e.PayrollGroupSetting.IsLoaded)
                    {
                        e.PayrollGroupSetting.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("PayrollGroup.cs e.PayrollGroupSetting");
                    }
                    if (includePayrollGroupReports && !e.PayrollGroupReport.IsLoaded)
                    {
                        e.PayrollGroupReport.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("PayrollGroup.cs e.PayrollGroupReport");
                    }
                    if (includeAccounts && !e.PayrollGroupAccountStd.IsLoaded)
                    {
                        e.PayrollGroupAccountStd.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("PayrollGroup.cs e.PayrollGroupAccountStd");
                    }
                    if (includePayrollGroupVacationGroup && !e.PayrollGroupVacationGroup.IsLoaded)
                    {
                        e.PayrollGroupVacationGroup.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("PayrollGroup.cs e.PayrollGroupVacationGroup");
                    }
                    if (includePayrollGroupPayrollProduct && !e.PayrollGroupPayrollProduct.IsLoaded)
                    {
                        e.PayrollGroupPayrollProduct.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("PayrollGroup.cs e.PayrollGroupPayrollProduct");
                    }
                }
            }
            catch (InvalidOperationException ex) { ex.ToString(); }

            #endregion

            PayrollGroupDTO dto = new PayrollGroupDTO()
            {
                PayrollGroupId = e.PayrollGroupId,
                ActorCompanyId = e.ActorCompanyId,
                Name = e.Name,
                TimePeriodHeadId = e.TimePeriodHeadId,
                OneTimeTaxFormulaId = e.OneTimeTaxFormulaId,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State
            };

            // Relations
            if (includePriceTypes)
                dto.PriceTypes = e.PayrollGroupPriceType?.Where(p => p.State == (int)SoeEntityState.Active).ToDTOs(true).ToList() ?? new List<PayrollGroupPriceTypeDTO>();
            if (includePriceFormulas)
                dto.PriceFormulas = e.PayrollGroupPriceFormula?.Where(p => p.State == (int)SoeEntityState.Active).ToDTOs().ToList() ?? new List<PayrollGroupPriceFormulaDTO>();
            if (includeSettings)
                dto.Settings = e.PayrollGroupSetting?.Where(r => r.State == (int)SoeEntityState.Active).ToDTOs().ToList() ?? new List<PayrollGroupSettingDTO>();
            if (includePayrollGroupReports)
            {
                dto.Reports = e.PayrollGroupReport?.Where(x => x.State == (int)SoeEntityState.Active).ToDTOs().ToList() ?? new List<PayrollGroupReportDTO>();
                dto.ReportIds = dto.Reports?.Select(r => r.ReportId).ToList() ?? new List<int>();
            }
            if (includeTimePeriod && e.TimePeriodHead != null)
                dto.TimePeriodHead = e.TimePeriodHead.ToDTO(true);
            if (includeAccounts)
                dto.Accounts = e.ToAccountsDTOs();
            if (includePayrollGroupVacationGroup)
                dto.Vacations = e.PayrollGroupVacationGroup?.Where(x => x.State == (int)SoeEntityState.Active).ToDTOs(false).ToList() ?? new List<PayrollGroupVacationGroupDTO>();
            if (includePayrollGroupPayrollProduct)
                dto.PayrollProducts = e.PayrollGroupPayrollProduct?.Where(x => x.State == (int)SoeEntityState.Active).ToDTOs().ToList() ?? new List<PayrollGroupPayrollProductDTO>();

            if (!e.ExternalCodes.IsNullOrEmpty())
            {
                dto.ExternalCodes = e.ExternalCodes;
                dto.ExternalCodesString = e.ExternalCodesString;
            }

            return dto;
        }

        public static IEnumerable<PayrollGroupDTO> ToDTOs(this IEnumerable<PayrollGroup> l, bool includePriceTypes = false, bool includePriceFormulas = false, bool includeSettings = false, bool includePayrollGroupReports = false, bool includeTimePeriod = false, bool includeAccounts = false, bool includePayrollGroupVacationGroup = false, bool includePayrollGroupPayrollProduct = false)
        {
            var dtos = new List<PayrollGroupDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO(includePriceTypes, includePriceFormulas, includeSettings, includePayrollGroupReports, includeTimePeriod, includeAccounts, includePayrollGroupVacationGroup, includePayrollGroupPayrollProduct));
                }
            }
            return dtos;
        }

        public static PayrollGroupSmallDTO ToSmallDTO(this PayrollGroup e, bool includePriceTypes = false)
        {
            if (e == null)
                return null;

            #region Try load

            try
            {
                if (!e.IsAdded() && includePriceTypes)
                {
                    if (!e.PayrollGroupPriceType.IsLoaded)
                    {
                        e.PayrollGroupPriceType.Load();

                        DataProjectLogCollector.LogLoadedEntityInExtension("PayrollGroup.cs e.PayrollGroupPriceType");
                    }
                    foreach (var payrollGroupPriceType in e.PayrollGroupPriceType)
                    {
                        if (!payrollGroupPriceType.PayrollGroupPriceTypePeriod.IsLoaded)
                        {
                            payrollGroupPriceType.PayrollGroupPriceTypePeriod.Load();
                            DataProjectLogCollector.LogLoadedEntityInExtension("PayrollGroup.cs payrollGroupPriceType.PayrollGroupPriceTypePeriod");
                        }
                        if (!payrollGroupPriceType.PayrollPriceTypeReference.IsLoaded)
                        {
                            payrollGroupPriceType.PayrollPriceTypeReference.Load();
                            DataProjectLogCollector.LogLoadedEntityInExtension("PayrollGroup.cs payrollGroupPriceType.PayrollPriceTypeReference");
                        }
                        if (!payrollGroupPriceType.PayrollPriceType.PayrollPriceTypePeriod.IsLoaded)
                        {
                            payrollGroupPriceType.PayrollPriceType.PayrollPriceTypePeriod.Load();
                            DataProjectLogCollector.LogLoadedEntityInExtension("PayrollGroup.cs payrollGroupPriceType.PayrollPriceType.PayrollPriceTypePeriod");
                        }
                    }
                }
            }
            catch (InvalidOperationException ex) { ex.ToString(); }

            #endregion

            PayrollGroupSmallDTO dto = new PayrollGroupSmallDTO()
            {
                PayrollGroupId = e.PayrollGroupId,
                Name = e.Name,
            };

            if (includePriceTypes)
                dto.PriceTypes = e.PayrollGroupPriceType?.Where(p => p.State == (int)SoeEntityState.Active).ToDTOs(true).ToList() ?? new List<PayrollGroupPriceTypeDTO>();

            return dto;
        }

        public static IEnumerable<PayrollGroupSmallDTO> ToSmallDTOs(this IEnumerable<PayrollGroup> l, bool includePriceTypes = false)
        {
            var dtos = new List<PayrollGroupSmallDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToSmallDTO(includePriceTypes));
                }
            }
            return dtos;
        }

        public static PayrollGroupGridDTO ToGridDTO(this PayrollGroup e)
        {
            if (e == null)
                return null;

            PayrollGroupGridDTO dto = new PayrollGroupGridDTO()
            {
                PayrollGroupId = e.PayrollGroupId,
                Name = e.Name,
                State = (SoeEntityState)e.State
            };

            if (e.TimePeriodHead != null)
                dto.TimePeriodHeadName = e.TimePeriodHead.Name;

            return dto;
        }

        public static IEnumerable<PayrollGroupGridDTO> ToGridDTOs(this IEnumerable<PayrollGroup> l)
        {
            List<PayrollGroupGridDTO> dtos = new List<PayrollGroupGridDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToGridDTO());
                }
            }
            return dtos;
        }

        public static Dictionary<int, string> ToDictionary(this IEnumerable<PayrollGroup> l)
        {
            var dict = new Dictionary<int, string>();

            foreach (var e in l)
            {
                if (!dict.ContainsKey(e.PayrollGroupId))
                    dict.Add(e.PayrollGroupId, e.Name);
            }

            return dict;
        }

        public static IEnumerable<SmallGenericType> ToSmallGenericTypes(this IEnumerable<PayrollGroup> l)
        {
            List<SmallGenericType> dtos = new List<SmallGenericType>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToSmallGenericType());
                }
            }
            return dtos;
        }

        public static SmallGenericType ToSmallGenericType(this PayrollGroup e)
        {
            if (e == null)
                return null;

            return new SmallGenericType()
            {
                Id = e.PayrollGroupId,
                Name = e.Name,
            };
        }

        #endregion

        #region PayrollGroupAccounts

        public static List<PayrollGroupAccountsDTO> ToAccountsDTOs(this PayrollGroup e)
        {
            if (e == null)
                return null;

            #region Try load

            try
            {
                if (!e.IsAdded() && !e.PayrollGroupAccountStd.IsLoaded)
                {
                    e.PayrollGroupAccountStd.Load();
                    DataProjectLogCollector.LogLoadedEntityInExtension("PayrollGroup.cs e.PayrollGroupAccountStd");
                }
            }
            catch (InvalidOperationException ex) { ex.ToString(); }

            #endregion

            if (e.PayrollGroupAccountStd == null || e.PayrollGroupAccountStd.Count == 0)
                return null;

            List<PayrollGroupAccountsDTO> dtos = new List<PayrollGroupAccountsDTO>();

            #region EmploymentTax

            foreach (PayrollGroupAccountStd account in e.PayrollGroupAccountStd.Where(p => p.Type == (int)PayrollGroupAccountType.EmploymentTax))
            {
                PayrollGroupAccountsDTO dto = new PayrollGroupAccountsDTO()
                {
                    FromInterval = account.FromInterval,
                    ToInterval = account.ToInterval,
                    EmploymentTaxAccountId = account.AccountId,
                    EmploymentTaxPercent = account.Percent ?? 0,
                };
                if (account.AccountStd != null)
                {
                    dto.EmploymentTaxAccountNr = account.AccountStd.Account.AccountNr;
                    dto.EmploymentTaxAccountName = account.AccountStd.Account.Name;
                }
                dtos.Add(dto);
            }

            #endregion

            #region PayrollTax

            foreach (PayrollGroupAccountStd account in e.PayrollGroupAccountStd.Where(p => p.Type == (int)PayrollGroupAccountType.PayrollTax))
            {
                PayrollGroupAccountsDTO dto = dtos.FirstOrDefault(d => d.FromInterval == account.FromInterval && d.ToInterval == account.ToInterval);
                if (dto == null)
                {
                    dto = new PayrollGroupAccountsDTO()
                    {
                        FromInterval = account.FromInterval,
                        ToInterval = account.ToInterval,
                    };
                    dtos.Add(dto);
                }

                if (account.AccountStd != null)
                {
                    dto.PayrollTaxAccountNr = account.AccountStd.Account.AccountNr;
                    dto.PayrollTaxAccountName = account.AccountStd.Account.Name;
                }

                dto.PayrollTaxAccountId = account.AccountId;
                dto.PayrollTaxPercent = account.Percent ?? 0;
            }

            #endregion

            #region OwnSupplementCharge

            foreach (PayrollGroupAccountStd account in e.PayrollGroupAccountStd.Where(p => p.Type == (int)PayrollGroupAccountType.OwnSupplementCharge))
            {
                PayrollGroupAccountsDTO dto = dtos.FirstOrDefault(d => d.FromInterval == account.FromInterval && d.ToInterval == account.ToInterval);
                if (dto == null)
                {
                    dto = new PayrollGroupAccountsDTO()
                    {
                        FromInterval = account.FromInterval,
                        ToInterval = account.ToInterval,
                    };
                    dtos.Add(dto);
                }

                if (account.AccountStd != null && account.AccountStd.Account != null)
                {
                    dto.OwnSupplementChargeAccountNr = account.AccountStd.Account.AccountNr;
                    dto.OwnSupplementChargeAccountName = account.AccountStd.Account.Name;
                }

                dto.OwnSupplementChargeAccountId = account.AccountId;
                dto.OwnSupplementChargePercent = account.Percent ?? 0;
            }

            #endregion

            return dtos;
        }

        public static void AddPayrollGroupAccountStd(this PayrollGroup group, PayrollGroupAccountType type, int accountId, decimal? percent = null, decimal? fromInterval = null, decimal? toInterval = null)
        {
            if (!group.PayrollGroupAccountStd.Any(a => a.Type == (int)type && a.FromInterval == fromInterval && a.ToInterval == toInterval))
            {
                group.PayrollGroupAccountStd.Add(new PayrollGroupAccountStd()
                {
                    AccountId = accountId,
                    Type = (int)type,
                    Percent = percent,
                    FromInterval = fromInterval,
                    ToInterval = toInterval
                });
            }
        }

        #endregion

        #region PayrollGroupPayrollProduct

        public static PayrollGroupPayrollProductDTO ToDTO(this PayrollGroupPayrollProduct e)
        {
            if (e == null)
                return null;

            return new PayrollGroupPayrollProductDTO()
            {
                PayrollGroupPayrollProductId = e.PayrollGroupPayrollProductId,
                PayrollGroupId = e.PayrollGroupId,
                ProductId = e.ProductId,
                Distribute = e.Distribute,
                ProductName = e.PayrollProduct?.Name ?? string.Empty,
                ProductNr = e.PayrollProduct?.Number ?? string.Empty,
                State = (SoeEntityState)e.State,
            };
        }

        public static IEnumerable<PayrollGroupPayrollProductDTO> ToDTOs(this IEnumerable<PayrollGroupPayrollProduct> l)
        {
            var dtos = new List<PayrollGroupPayrollProductDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static FixedPayrollRowDTO ToDTOFixedPayrollRowDTO(this PayrollGroupPayrollProduct e, bool includeProduct)
        {
            if (e == null)
                return null;

            if (!e.IsAdded() && includeProduct && !e.PayrollProductReference.IsLoaded)
            {
                e.PayrollProductReference.Load();
                DataProjectLogCollector.LogLoadedEntityInExtension("PayrollGroup.cs e.PayrollProductReference");
            }

            return new FixedPayrollRowDTO()
            {
                ProductId = e.ProductId,
                ActorCompanyId = 0,
                EmployeeId = 0,
                FromDate = null,
                ToDate = null,
                UnitPrice = 0,
                Quantity = 1,
                Amount = 0,
                VatAmount = 0,
                IsSpecifiedUnitPrice = false,
                Distribute = e.Distribute,
                IsReadOnly = true,
                ProductName = e.PayrollProduct?.Name ?? string.Empty,
                ProductNr = e.PayrollProduct?.Number ?? string.Empty,
            };
        }

        public static IEnumerable<FixedPayrollRowDTO> ToDTOFixedPayrollRowDTOs(this IEnumerable<PayrollGroupPayrollProduct> l, bool includeProduct)
        {
            var dtos = new List<FixedPayrollRowDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTOFixedPayrollRowDTO(includeProduct));
                }
            }
            return dtos;
        }

        #endregion

        #region PayrollGroupPriceFormula

        public static PayrollGroupPriceFormulaDTO ToDTO(this PayrollGroupPriceFormula e)
        {
            if (e == null)
                return null;

            return new PayrollGroupPriceFormulaDTO()
            {
                PayrollGroupPriceFormulaId = e.PayrollGroupPriceFormulaId,
                PayrollGroupId = e.PayrollGroupId,
                PayrollPriceFormulaId = e.PayrollPriceFormulaId,
                ShowOnEmployee = e.ShowOnEmployee,
                FromDate = e.FromDate,
                ToDate = e.ToDate,
                FormulaPlain = e.PayrollPriceFormula?.FormulaPlain ?? string.Empty,
                FormulaName = e.PayrollPriceFormula?.Name ?? string.Empty,
            };
        }

        public static IEnumerable<PayrollGroupPriceFormulaDTO> ToDTOs(this IEnumerable<PayrollGroupPriceFormula> l)
        {
            var dtos = new List<PayrollGroupPriceFormulaDTO>();
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

        #region PayrollGroupPriceType

        public static PayrollGroupPriceTypeDTO ToDTO(this PayrollGroupPriceType e, bool includePeriods)
        {
            if (e == null)
                return null;

            #region Try load

            try
            {
                if (!e.IsAdded() && includePeriods)
                {
                    if (!e.PayrollPriceTypeReference.IsLoaded)
                    {
                        e.PayrollPriceTypeReference.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("PayrollGroup.cs e.PayrollPriceTypeReference");
                    }
                    if (!e.PayrollGroupPriceTypePeriod.IsLoaded)
                    {
                        e.PayrollGroupPriceTypePeriod.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("PayrollGroup.cs e.PayrollGroupPriceTypePeriod");
                    }
                }
            }
            catch (InvalidOperationException ex) { ex.ToString(); }

            #endregion

            PayrollGroupPriceTypeDTO dto = new PayrollGroupPriceTypeDTO()
            {
                PayrollGroupPriceTypeId = e.PayrollGroupPriceTypeId,
                PayrollGroupId = e.PayrollGroupId,
                PayrollPriceTypeId = e.PayrollPriceTypeId,
                Sort = e.Sort,
                ShowOnEmployee = e.ShowOnEmployee,
                ReadOnlyOnEmployee = e.ReadOnlyOnEmployee,
                PriceTypeCode = e.PayrollPriceType?.Code ?? string.Empty,
                PriceTypeName = e.PayrollPriceType?.Name ?? string.Empty,
                PayrollPriceType = e.PayrollPriceType?.ToDTO(false),
                PayrollLevelId = e.PayrollLevelId,
                PayrollLevelName = e.PayrollLevel?.Name ?? string.Empty,
            };

            if (includePeriods)
                dto.Periods = e.PayrollGroupPriceTypePeriod?.Where(p => p.State == (int)SoeEntityState.Active).ToDTOs().ToList() ?? new List<PayrollGroupPriceTypePeriodDTO>();

            return dto;
        }

        public static IEnumerable<PayrollGroupPriceTypeDTO> ToDTOs(this IEnumerable<PayrollGroupPriceType> l, bool includePeriods)
        {
            var dtos = new List<PayrollGroupPriceTypeDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO(includePeriods));
                }
            }
            return dtos;
        }

        public static List<PayrollGroupPriceType> GetPayrollGroupPriceTypes(this PayrollGroup e, int payrollPriceTypeId, bool onlyWithLevels = false)
        {
            return e?.PayrollGroupPriceType?.Where(p => p.PayrollPriceTypeId == payrollPriceTypeId && (!onlyWithLevels || p.PayrollLevelId.HasValue) && p.State == (int)SoeEntityState.Active).ToList() ?? new List<PayrollGroupPriceType>();
        }

        public static PayrollGroupPriceType GetPayrollGroupPriceType(this PayrollGroup e, int payrollPriceTypeId, int? payrollLevelId)
        {
            List<PayrollGroupPriceType> priceTypes = e.GetPayrollGroupPriceTypes(payrollPriceTypeId);
            if (payrollLevelId.HasValue)
                return priceTypes.FirstOrDefault(p => p.PayrollLevelId == payrollLevelId.Value);
            else
                return priceTypes.FirstOrDefault(p => !p.PayrollLevelId.HasValue);
        }

        public static PayrollGroupPriceTypePeriod GetPeriod(this PayrollGroupPriceType e, DateTime date)
        {
            return e?.PayrollGroupPriceTypePeriod?
                .Where(p => (!p.FromDate.HasValue || p.FromDate.Value <= date) && p.State == (int)SoeEntityState.Active)
                .OrderByDescending(p => p.FromDate)
                .FirstOrDefault();
        }


        #endregion

        #region PayrollGroupPriceTypePeriod

        public static PayrollGroupPriceTypePeriodDTO ToDTO(this PayrollGroupPriceTypePeriod e)
        {
            if (e == null)
                return null;

            return new PayrollGroupPriceTypePeriodDTO()
            {
                PayrollGroupPriceTypePeriodId = e.PayrollGroupPriceTypePeriodId,
                PayrollGroupPriceTypeId = e.PayrollGroupPriceTypeId,
                FromDate = e.FromDate,
                Amount = e.Amount
            };
        }

        public static IEnumerable<PayrollGroupPriceTypePeriodDTO> ToDTOs(this IEnumerable<PayrollGroupPriceTypePeriod> l)
        {
            var dtos = new List<PayrollGroupPriceTypePeriodDTO>();
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

        #region PayrollGroupReport

        public static PayrollGroupReportDTO ToDTO(this PayrollGroupReport e)
        {
            if (e == null)
                return null;

            try
            {
                if (!e.IsAdded() && !e.ReportReference.IsLoaded)
                {
                    e.ReportReference.Load();
                    DataProjectLogCollector.LogLoadedEntityInExtension("PayrollGroup.cs e.ReportReference");
                }
            }
            catch (InvalidOperationException ex) { ex.ToString(); }

            return new PayrollGroupReportDTO()
            {
                PayrollGroupReportId = e.PayrollGroupReportId,
                ActorCompanyId = e.ActorCompanyId,
                PayrollGroupId = e.PayrollGroupId,
                ReportId = e.ReportId,
                ReportName = e.Report?.Name ?? string.Empty,
                ReportNr = e.Report?.ReportNr ?? 0,
                ReportDescription = e.Report?.Description ?? string.Empty,
                SysReportTemplateTypeId = e.SysReportTemplateTypeId,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State,
            };
        }

        public static IEnumerable<PayrollGroupReportDTO> ToDTOs(this IEnumerable<PayrollGroupReport> l)
        {
            var dtos = new List<PayrollGroupReportDTO>();
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

        #region PayrollGroupSetting

        public static PayrollGroupSettingDTO ToDTO(this PayrollGroupSetting e)
        {
            return new PayrollGroupSettingDTO()
            {
                Id = e.PayrollGroupSettingId,
                PayrollGroupId = e.PayrollGroupId,
                DataType = (SettingDataType)e.DataType,
                Type = (PayrollGroupSettingType)e.Type,
                Name = e.Name,
                StrData = e.StrData,
                IntData = e.IntData,
                DecimalData = e.DecimalData,
                BoolData = e.BoolData,
                DateData = e.DateData,
                TimeData = e.TimeData,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
            };
        }

        public static IEnumerable<PayrollGroupSettingDTO> ToDTOs(this IEnumerable<PayrollGroupSetting> l)
        {
            var dtos = new List<PayrollGroupSettingDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    if (e.State == (int)SoeEntityState.Active)
                        dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static PayrollGroupSetting GetPayrollGroupSetting(this List<PayrollGroupSetting> l, int payrollGroupId, PayrollGroupSettingType type)
        {
            return l?.FirstOrDefault(pg => pg.PayrollGroupId == payrollGroupId && pg.Type == (int)type && pg.State == (int)SoeEntityState.Active);
        }

        #endregion

        #region PayrollGroupVacationGroup

        public static PayrollGroupVacationGroupDTO ToDTO(this PayrollGroupVacationGroup e, bool includeVacationGroupSE)
        {
            if (e == null)
                return null;

            if (e.VacationGroup == null)
                return null;

            #region Try load

            try
            {
                if (includeVacationGroupSE && !e.IsAdded())
                {
                    if (!e.VacationGroupReference.IsLoaded)
                    {
                        e.VacationGroupReference.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("PayrollGroup.cs e.VacationGroupReference");
                    }
                    if (!e.VacationGroup.VacationGroupSE.IsLoaded)
                    {
                        e.VacationGroup.VacationGroupSE.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("PayrollGroup.cs e.VacationGroup.VacationGroupSE");
                    }
                }
            }
            catch (InvalidOperationException ex) { ex.ToString(); }

            #endregion

            PayrollGroupVacationGroupDTO dto = new PayrollGroupVacationGroupDTO()
            {
                PayrollGroupVacationGroupId = e.PayrollGroupVacationGroupId,
                PayrollGroupId = e.PayrollGroupId,
                VacationGroupId = e.VacationGroupId,
                IsDefault = e.Default,
                Name = e.VacationGroup.Name,
                Type = e.VacationGroup.Type,
            };

            if (includeVacationGroupSE && e.VacationGroup != null && e.VacationGroup.VacationGroupSE != null && e.VacationGroup.VacationGroupSE.Count > 0)
            {
                VacationGroupSE se = e.VacationGroup.VacationGroupSE.First();
                dto.CalculationType = (TermGroup_VacationGroupCalculationType)se.CalculationType;
                dto.VacationHandleRule = (TermGroup_VacationGroupVacationHandleRule)se.VacationHandleRule;
                dto.VacationDaysHandleRule = (TermGroup_VacationGroupVacationDaysHandleRule)se.VacationDaysHandleRule;
            }

            return dto;
        }

        public static IEnumerable<PayrollGroupVacationGroupDTO> ToDTOs(this IEnumerable<PayrollGroupVacationGroup> l, bool includeVacationGroupSE)
        {
            var dtos = new List<PayrollGroupVacationGroupDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO(includeVacationGroupSE));
                }
            }
            return dtos;
        }

        #endregion

    }
}

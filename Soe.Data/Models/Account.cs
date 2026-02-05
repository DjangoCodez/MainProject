using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Data
{
    public partial class Account : IAccountId, ICreatedModified, IState
    {
        public string Type { get; set; }
        public string VatType { get; set; }
        public decimal Balance { get; set; }
        public bool IsLinkedToShiftType { get; set; }
        public string ParentAccountName { get; set; }
        public string AccountNrPlusName
        {
            get { return $"{this.AccountNr}. {this.Name}"; }
        }
        public string AccountNrSort
        {
            get { return this.AccountNr.PadLeft(20, '0'); }
        }
        public string AccountHierachyPayrollExportExternalCode { get; set; }
        public string AccountHierachyPayrollExportUnitExternalCode { get; set; }
    }

    public partial class AccountInternal : IAccountId
    {

    }

    public partial class AccountStd
    {
        public int? GrossProfitCode { get; set; }
    }

    public partial class AccountHistory
    {
        public string SysAccountStdTypeName { get; set; }
    }

    public static partial class EntityExtensions
    {
        #region Account

        public static AccountDTO ToDTO(this Account e, bool includeAccountDim = false, bool includeInternalAccounts = false)
        {
            if (e == null)
                return null;

            AccountDTO dto = new AccountDTO()
            {
                AccountId = e.AccountId,
                AccountNr = e.AccountNr,
                Name = e.Name,
                Description = e.Description,
                AccountDimId = e.AccountDimId,
                State = (SoeEntityState)e.State,
                AttestWorkFlowHeadId = e.AttestWorkFlowHeadId,
                ParentAccountId = e.ParentAccountId,
                ExternalCode = e.ExternalCode,
                HierarchyOnly = e.HierarchyOnly,
                HierarchyNotOnSchedule = e.HierarchyNotOnSchedule,
            };

            // Extensions
            dto.AccountDimNr = e.AccountDim?.AccountDimNr ?? 0;
            dto.AmountStop = e.AccountStd?.AmountStop ?? 1;
            dto.UnitStop = e.AccountStd?.UnitStop ?? false;
            dto.Unit = e.AccountStd?.Unit ?? "";
            dto.RowTextStop = e.AccountStd?.RowTextStop ?? false;
            dto.IsAccrualAccount = e.AccountStd?.isAccrualAccount ?? false;
            dto.AccountTypeSysTermId = e.AccountStd?.AccountTypeSysTermId ?? 0;

            if (includeAccountDim && e.AccountDim != null)
                dto.AccountDim = e.AccountDim.ToDTO(false, false);

            if (includeInternalAccounts)
            {
                dto.AccountInternals = new List<AccountInternalDTO>();
                if (!e.AccountMapping.IsNullOrEmpty())
                {
                    foreach (AccountMapping map in e.AccountMapping)
                    {
                        if (map.AccountDim == null || map.AccountDim.State != (int)SoeEntityState.Active)
                            continue;

                        dto.AccountInternals.Add(new AccountInternalDTO()
                        {
                            AccountId = map.AccountInternal?.Account?.AccountId ?? 0,
                            AccountNr = map.AccountInternal?.Account?.AccountNr ?? "",
                            Name = map.AccountInternal?.Account?.Name ?? "",
                            AccountDimId = map.AccountDimId,
                            AccountDimNr = map.AccountDim.AccountDimNr,
                            SysSieDimNr = map.AccountDim.SysSieDimNr,
                            SysSieDimNrOrAccountDimNr = map.AccountDim.SysSieDimNrOrAccountDimNr,
                            MandatoryLevel = map.MandatoryLevel ?? (int)TermGroup_AccountMandatoryLevel.None,
                            UseVatDeduction = map.AccountInternal?.UseVatDeduction ?? false,
                            VatDeduction = map.AccountInternal?.VatDeduction ?? 0
                        });
                    }
                }
            }

            return dto;
        }

        public static List<AccountDTO> ToDTOs(this IEnumerable<Account> l, bool includeAccountDim = false, bool includeInternalAccounts = false)
        {
            var dtos = new List<AccountDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    var dto = e.ToDTO(includeAccountDim, includeInternalAccounts);
                    if (dto != null)
                        dtos.Add(dto);
                }
            }
            return dtos;
        }

        public static AccountDTO ToDTO(this AccountStd e, bool includeAccountDim = false, bool includeInternalAccounts = false)
        {
            return e?.Account?.ToDTO(includeAccountDim, includeInternalAccounts);
        }

        public static IEnumerable<AccountDTO> ToDTOs(this List<AccountStd> l, bool includeAccountDim = false, bool includeInternalAccounts = false)
        {
            var dtos = new List<AccountDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO(includeAccountDim, includeInternalAccounts));
                }
            }
            return dtos;
        }

        public static AccountGridDTO ToGridDTO(this Account e, bool getCategories = false, bool setParentAccount = false)
        {
            if (e == null)
                return null;

            #region Try load
            try
            {
                if (!e.IsAdded() && !e.AccountStdReference.IsLoaded)
                {
                    e.AccountStdReference.Load();
                    DataProjectLogCollector.LogLoadedEntityInExtension("Account.cs e.AccountStdReference");
                }
            }
            catch (InvalidOperationException ex) { ex.ToString(); }

            #endregion

            AccountGridDTO dto = new AccountGridDTO()
            {
                AccountId = e.AccountId,
                AccountDimId = e.AccountDimId,
                AccountNr = e.AccountNr,
                Name = e.Name,
                State = (SoeEntityState)e.State,
                IsLinkedToShiftType = e.IsLinkedToShiftType,
                ExternalCode = e.ExternalCode,
                ParentAccountName = setParentAccount ? e.ParentAccountName : String.Empty,
            };

            // AccountStd
            if (e.AccountStd != null)
            {
                dto.AccountTypeSysTermId = e.AccountStd.AccountTypeSysTermId;
                dto.Type = e.Type;
                dto.SysVatAccountId = e.AccountStd.SysVatAccountId;
                dto.VatType = e.VatType;
                dto.Balance = e.Balance;
            }

            if (getCategories && e.AccountInternal?.CategoryAccount != null && e.AccountInternal.CategoryAccount.Any(ca => ca.State == (int)SoeEntityState.Active))
                dto.Categories = e.AccountInternal.CategoryAccount.Where(ca => ca.State == (int)SoeEntityState.Active).Select(a => a.Category.Name).Aggregate((x, y) => x + ", " + y);

            return dto;
        }

        public static IEnumerable<AccountGridDTO> ToGridDTOs(this IEnumerable<Account> l, bool getCategories = false, bool setParentAccount = false)
        {
            var dtos = new List<AccountGridDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToGridDTO(getCategories, setParentAccount));
                }
            }
            return dtos;
        }        

        public static AccountEditDTO ToEditDTO(this Account e, bool includeInternalAccounts)
        {
            if (e == null)
                return null;

            #region Try load
            try
            {
                if (!e.IsAdded())
                {
                    if (!e.AccountStdReference.IsLoaded)
                    {
                        e.AccountStdReference.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("Account.cs e.AccountStdReference");
                    }
                    if (includeInternalAccounts)
                    {
                        if (!e.AccountMapping.IsLoaded)
                        {
                            e.AccountMapping.Load();
                            DataProjectLogCollector.LogLoadedEntityInExtension("Account.cs e.AccountMapping");
                        }
                        if (e.AccountMapping != null)
                        {
                            foreach (var map in e.AccountMapping)
                            {
                                if (!map.AccountDimReference.IsLoaded)
                                {
                                    map.AccountDimReference.Load();
                                    DataProjectLogCollector.LogLoadedEntityInExtension("Account.cs map.AccountDimReference");
                                }
                            }
                        }
                        if (!e.AccountInternalReference.IsLoaded)
                        {
                            e.AccountInternalReference.Load();
                            DataProjectLogCollector.LogLoadedEntityInExtension("Account.cs e.AccountInternalReference");
                        }
                    }
                }
            }
            catch (InvalidOperationException ex) { ex.ToString(); }
            #endregion

            // Account
            AccountEditDTO dto = new AccountEditDTO()
            {
                AccountId = e.AccountId,
                AccountDimId = e.AccountDimId,
                AccountNr = e.AccountNr,
                Name = e.Name,
                Description = e.Description,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State,
                Active = e.State == (int)SoeEntityState.Active,
                AttestWorkFlowHeadId = e.AttestWorkFlowHeadId,
                ParentAccountId = e.ParentAccountId,
                ExternalCode = e.ExternalCode,
                HierarchyOnly = e.HierarchyOnly,
                HierarchyNotOnSchedule = e.HierarchyNotOnSchedule,
            };
            dto.UseVatDeductionDim = e.AccountDim?.UseVatDeduction ?? false;
            dto.UseVatDeduction = e.AccountInternal?.UseVatDeduction ?? false;
            dto.VatDeduction = e.AccountInternal?.VatDeduction ?? 100;

            // AccountStd
            dto.SysVatAccountId = e.AccountStd?.SysVatAccountId;
            dto.AccountTypeSysTermId = e.AccountStd?.AccountTypeSysTermId ?? 0;
            dto.AmountStop = e.AccountStd?.AmountStop ?? 1;
            dto.Unit = e.AccountStd?.Unit ?? String.Empty;
            dto.UnitStop = e.AccountStd?.UnitStop ?? false;
            dto.RowTextStop = e.AccountStd?.RowTextStop ?? false;
            dto.SieKpTyp = e.AccountStd?.SieKpTyp ?? String.Empty;
            dto.ExcludeVatVerification = e.AccountStd?.ExcludeVatVerification;
            dto.SysAccountSruCode1Id = e.AccountStd?.AccountSru != null && e.AccountStd.AccountSru.Any() ? e.AccountStd.AccountSru.First().SysAccountSruCodeId : (int?)null;
            dto.SysAccountSruCode2Id = e.AccountStd?.AccountSru != null && e.AccountStd.AccountSru.Count == 2 ? e.AccountStd.AccountSru.Last().SysAccountSruCodeId : (int?)null;
            dto.isAccrualAccount = e.AccountStd?.isAccrualAccount ?? false;

            // Extensions
            dto.IsStdAccount = e.AccountStd != null;

            if (includeInternalAccounts)
            {
                dto.AccountInternals = new List<AccountInternalDTO>();
                foreach (AccountMapping map in e.AccountMapping/*.Where(m => m.AccountInternal != null)*/)
                {
                    dto.AccountInternals.Add(new AccountInternalDTO()
                    {
                        AccountId = map.AccountInternal?.Account.AccountId ?? 0,
                        AccountNr = map.AccountInternal?.Account.AccountNr ?? string.Empty,
                        Name = map.AccountInternal?.Account.Name ?? string.Empty,
                        AccountDimId = map.AccountDimId,
                        AccountDimNr = map.AccountDim.AccountDimNr,
                        SysSieDimNr = map.AccountDim.SysSieDimNr,
                        SysSieDimNrOrAccountDimNr = map.AccountDim.SysSieDimNrOrAccountDimNr,
                        MandatoryLevel = map.MandatoryLevel ?? (int)TermGroup_AccountMandatoryLevel.None,
                        UseVatDeduction = map.AccountInternal?.UseVatDeduction ?? false,
                        VatDeduction = map.AccountInternal?.VatDeduction ?? 0
                    });
                }
            }

            // External codes
            dto.AccountHierachyPayrollExportExternalCode = e.AccountHierachyPayrollExportExternalCode;
            dto.AccountHierachyPayrollExportUnitExternalCode = e.AccountHierachyPayrollExportUnitExternalCode;

            return dto;
        }
                
        public static AccountSmallDTO ToSmallDTO(this Account e)
        {
            if (e == null)
                return null;

            return new AccountSmallDTO()
            {
                AccountId = e.AccountId,
                AccountDimId = e.AccountDimId,
                ParentAccountId = e.ParentAccountId,
                Number = e.AccountNr,
                Name = e.Name
            };
        }

        public static IEnumerable<AccountSmallDTO> ToSmallDTOs(this IEnumerable<Account> l)
        {
            var dtos = new List<AccountSmallDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToSmallDTO());
                }
            }
            return dtos;
        }

        public static bool IsAccountStd(this Account account, AccountDim accountDimStd)
        {
            return accountDimStd.IsStandard && account.AccountDimId == accountDimStd.AccountDimId;
        }

        public static bool IsAccountInternal(this Account account, AccountDim accountDimStd)
        {
            return !account.IsAccountStd(accountDimStd);
        }

        #endregion

        #region AccountInternal

        public static AccountInternalDTO ToDTO(this AccountInternal e, List<AccountDimDTO> accountDims = null)
        {
            if (e == null)
                return null;

            var accountDim = e.Account?.AccountDim == null ? accountDims?.FirstOrDefault(ad => ad.AccountDimId == e.Account?.AccountDimId) : null;

            return new AccountInternalDTO()
            {
                AccountId = e.AccountId,
                AccountNr = e.Account?.AccountNr ?? String.Empty,
                Name = e.Account?.Name ?? String.Empty,
                AccountDimId = e.Account?.AccountDimId ?? 0,
                AccountDimNr = e.Account?.AccountDim?.AccountDimNr ?? accountDim?.AccountDimNr ?? 0,
                SysSieDimNr = e.Account?.AccountDim?.SysSieDimNr ?? accountDim?.SysSieDimNr ?? 0,
                SysSieDimNrOrAccountDimNr = e.Account?.AccountDim?.SysSieDimNrOrAccountDimNr ?? accountDim?.SysSieDimNr ?? accountDim?.AccountDimNr ?? 0,
                UseVatDeduction = e.UseVatDeduction,
                VatDeduction = e.VatDeduction,
            };
        }

        public static AccountDTO ToAccountDTO(this AccountInternal e)
        {
            if (e?.Account == null)
                return null;

            return new AccountDTO()
            {
                AccountId = e.Account.AccountId,
                AccountNr = e.Account.AccountNr,
                Name = e.Account.Name,
                AccountDimId = e.Account.AccountDimId,
                AccountDimNr = e.Account.AccountDim?.AccountDimNr ?? 0,
            };
        }

        public static List<AccountInternalDTO> ToDTOs(this IEnumerable<AccountInternal> l, List<AccountDimDTO> accountDims = null)
        {
            var dtos = new List<AccountInternalDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO(accountDims));
                }
            }
            return dtos;
        }

        public static IEnumerable<AccountDTO> ToAccountDTOs(this IEnumerable<AccountInternal> l)
        {
            var dtos = new List<AccountDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToAccountDTO());
                }
            }
            return dtos;
        }

        public static AccountInternal FromDTO(this AccountInternalDTO dto)
        {
            if (dto == null)
                return null;

            AccountInternal e = new AccountInternal()
            {
                AccountId = dto.AccountId,
                Account = new Account()
                {
                    AccountId = dto.AccountId,
                    AccountNr = dto.AccountNr,
                    Name = dto.Name,
                    AccountDimId = dto.AccountDimId,
                },
            };

            return e;
        }

        public static bool Match(this List<AccountInternal> current, List<AccountInternal> input)
        {
            var currentAccountInternals = current?.OrderBy(i => i.AccountId).ToList() ?? new List<AccountInternal>();
            var inputAccountInternals = input?.OrderBy(i => i.AccountId).ToList() ?? new List<AccountInternal>();
            if (currentAccountInternals.Count != inputAccountInternals.Count)
                return false;

            for (int i = 0; i < currentAccountInternals.Count; i++)
            {
                if (currentAccountInternals[i].AccountId != inputAccountInternals[i].AccountId)
                    return false;
            }

            return true;
        }

        public static bool ValidOnFiltered<T>(this IEnumerable<T> l, List<AccountInternalDTO> validAccountInternals) where T: IAccountId
        {
            if (l.IsNullOrEmpty() || validAccountInternals == null)
                return false;

            foreach (var item in validAccountInternals.GroupBy(g => g.AccountDimId))
            {
                if (item.IsNullOrEmpty() || item.Key == 0)
                    continue;

                List<int> accountIds = item.Select(s => s.AccountId).ToList();
                if (!l.Any(a => accountIds.Contains(a.AccountId)))
                    return false;
            }

            return true;
        }

        public static AccountAndInternalAccountComboDTO GetInternalAccountDimNrs(this List<AccountInternalDTO> l, List<AccountDimDTO> accountDimInternals, List<AccountDTO> accounts)
        {
            if (l == null)
                return new AccountAndInternalAccountComboDTO();

            accountDimInternals = accountDimInternals.Where(d => d.AccountDimNr != 1).ToList();
            AccountAndInternalAccountComboDTO combo = new AccountAndInternalAccountComboDTO();

            if (l.Any())
            {
                int dimCounter = 2;
                foreach (AccountDimDTO accountDimInternal in accountDimInternals)
                {
                    foreach (var e in l)
                    {
                        AccountDTO account = accounts.FirstOrDefault(a => a.AccountId == e.AccountId);
                        if (account != null && account.AccountDimId == accountDimInternal.AccountDimId)
                        {
                            switch (dimCounter)
                            {
                                case 2:
                                    combo.Dim2Id = e.AccountId;
                                    combo.Dim2Name = string.IsNullOrEmpty(e.Name) ? account.Name : e.Name;
                                    combo.Dim2Nr = string.IsNullOrEmpty(e.AccountNr) ? account.AccountNr : e.AccountNr;
                                    combo.Dim2SIENr = accountDimInternal.SysSieDimNr ?? 0;
                                    break;
                                case 3:
                                    combo.Dim3Id = e.AccountId;
                                    combo.Dim3Name = string.IsNullOrEmpty(e.Name) ? account.Name : e.Name;
                                    combo.Dim3Nr = string.IsNullOrEmpty(e.AccountNr) ? account.AccountNr : e.AccountNr;
                                    combo.Dim3SIENr = accountDimInternal.SysSieDimNr ?? 0;
                                    break;
                                case 4:
                                    combo.Dim4Id = e.AccountId;
                                    combo.Dim4Name = string.IsNullOrEmpty(e.Name) ? account.Name : e.Name;
                                    combo.Dim4Nr = string.IsNullOrEmpty(e.AccountNr) ? account.AccountNr : e.AccountNr;
                                    combo.Dim4SIENr = accountDimInternal.SysSieDimNr ?? 0;
                                    break;
                                case 5:
                                    combo.Dim5Id = e.AccountId;
                                    combo.Dim5Name = string.IsNullOrEmpty(e.Name) ? account.Name : e.Name;
                                    combo.Dim5Nr = string.IsNullOrEmpty(e.AccountNr) ? account.AccountNr : e.AccountNr;
                                    combo.Dim5SIENr = accountDimInternal.SysSieDimNr ?? 0;
                                    break;
                                case 6:
                                    combo.Dim6Id = e.AccountId;
                                    combo.Dim6Name = string.IsNullOrEmpty(e.Name) ? account.Name : e.Name;
                                    combo.Dim6Nr = string.IsNullOrEmpty(e.AccountNr) ? account.AccountNr : e.AccountNr;
                                    combo.Dim6SIENr = accountDimInternal.SysSieDimNr ?? 0;
                                    break;
                            }
                        }
                    }
                    dimCounter++;
                }
            }

            combo.AccountString = combo.Dim1Nr + ";" + combo.Dim2Nr + ";" + combo.Dim3Nr + ";" + combo.Dim4Nr + ";" + combo.Dim5Nr + ";" + combo.Dim6Nr;

            return combo;
        }

        #endregion

        #region AccountHistory

        public static AccountHistoryDTO ToDTO(this AccountHistory e)
        {
            if (e == null)
                return null;

            #region Try load
            try
            {
                if (!e.IsAdded())
                {
                    if (!e.AccountReference.IsLoaded)
                    { 
                        e.AccountReference.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("Account.cs e.AccountReference");
                    }
                    if (!e.UserReference.IsLoaded)
                    { 
                        e.UserReference.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("Account.cs e.UserReference");
                    }
                }
            }
            catch (InvalidOperationException ex) { ex.ToString(); }

            #endregion

            return new AccountHistoryDTO()
            {
                AccountHistoryId = e.AccountHistoryId,
                AccountId = e.Account != null ? e.Account.AccountId : 0,
                UserId = e.User != null ? e.User.UserId : 0,
                SysAccountStdTypeId = e.SysAccountStdTypeId,
                Date = e.Date,
                AccountNr = e.AccountNr,
                Name = e.Name,
                SieKpTyp = e.SieKpTyp,

                // Extensions
                UserName = e.User?.Name,
                SysAccountStdTypeName = e.SysAccountStdTypeName
            };
        }

        public static IEnumerable<AccountHistoryDTO> ToDTOs(this IEnumerable<AccountHistory> l)
        {
            var dtos = new List<AccountHistoryDTO>();
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

        #region AccountMappping

        public static AccountMappingDTO ToDTO(this AccountMapping e)
        {
            if (e == null)
                return null;

            return new AccountMappingDTO()
            {
                AccountId = e.AccountId,
                AccountDimId = e.AccountDimId,
                DefaultAccountId = e.DefaultAccountId,
            };
        }

        public static List<AccountMappingDTO> ToDTOs(this List<AccountMapping> l)
        {
            var dtos = new List<AccountMappingDTO>();
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

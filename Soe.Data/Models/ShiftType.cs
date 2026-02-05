using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Data
{
    public partial class ShiftType : ICreatedModified, IState
    {
        public string TimeScheduleTemplateBlockTypeName { get; set; }
        public string TimeScheduleTypeName { get; set; }
        public string CategoryNames { get; set; }
        public List<int> CategoryIds { get; set; }
        public string AccountingStringAccountNames { get; set; }
        public string SkillNames { get; set; }
        public List<int> ChildHierarchyAccountIds { get; set; }
    }

    public partial class ShiftTypeLink
    {
        public List<ShiftType> ShiftTypes { get; set; }
    }

    public partial class ShiftTypeEmployeeStatisticsTarget : ICreatedModified, IState
    {
        public string EmployeeStatisticsTypeName { get; set; }
    }

    public partial class ShiftTypeHierarchyAccount : ICreatedModified, IState
    {

    }

    public static partial class EntityExtensions
    {
        #region ShiftType

        public static ShiftTypeDTO ToDTO(this ShiftType e, bool includeAccounts, bool includeSkills, bool includeEmployeeStatisticsTargets, bool includeAccountingSettings, bool includeShiftTypeLinkIds, List<ShiftTypeLink> links = null, bool loadCategories = false, bool setAccountInternalIds = false, bool accountingSettingsOrdered = false)
        {
            if (e == null)
                return null;

            #region Try load

            try
            {
                if (!e.IsAdded())
                {
                    if (includeAccounts && !e.AccountInternal.IsLoaded)
                    {
                        e.AccountInternal.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("ShiftType.cs e.AccountInternal");
                    }
                    if (includeSkills && !e.ShiftTypeSkill.IsLoaded)
                    {
                        e.ShiftTypeSkill.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("ShiftType.cs e.ShiftTypeSkill");
                    }
                    if (includeEmployeeStatisticsTargets && !e.ShiftTypeEmployeeStatisticsTarget.IsLoaded)
                    {
                        e.ShiftTypeEmployeeStatisticsTarget.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("ShiftType.cs e.ShiftTypeEmployeeStatisticsTarget");
                    }
                }
            }
            catch (InvalidOperationException ex) { ex.ToString(); }

            #endregion

            ShiftTypeDTO dto = new ShiftTypeDTO()
            {
                ShiftTypeId = e.ShiftTypeId,
                ActorCompanyId = e.ActorCompanyId,
                TimeScheduleTypeId = e.TimeScheduleTypeId,
                TimeScheduleTypeName = e.TimeScheduleTypeName,
                TimeScheduleTemplateBlockType = e.TimeScheduleTemplateBlockType != null ? (TermGroup_TimeScheduleTemplateBlockType)e.TimeScheduleTemplateBlockType : (TermGroup_TimeScheduleTemplateBlockType?)null,
                Name = e.Name,
                Description = e.Description,
                Color = e.Color,
                ExternalId = e.ExternalId,
                ExternalCode = e.ExternalCode,
                DefaultLength = e.DefaultLength,
                StartTime = e.StartTime,
                StopTime = e.StopTime,
                NeedsCode = e.NeedsCode,
                HandlingMoney = e.HandlingMoney,
                AccountId = e.AccountId,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State
            };

            if (e.Account != null)
            {
                dto.AccountNrAndName = e.Account.AccountNr + " " + e.Account.Name;
                dto.AccountIsNotActive = e.Account.State != (int)SoeEntityState.Active;               
            }
            
            if (includeAccounts)
            {
                dto.AccountInternals = new Dictionary<int, AccountSmallDTO>();

                if (!e.AccountInternal.IsNullOrEmpty())
                {
                    foreach (var account in e.AccountInternal)
                    {
                        if (!account.AccountReference.IsLoaded)
                        {
                            account.AccountReference.Load();
                            DataProjectLogCollector.LogLoadedEntityInExtension("ShiftType.cs account.AccountReference");
                        }
                        if (account.Account != null && !account.Account.AccountDimReference.IsLoaded)
                        {
                            account.Account.AccountDimReference.Load();
                            DataProjectLogCollector.LogLoadedEntityInExtension("ShiftType.cs account.Account.AccountDimReference");
                        }
                        if (account.Account.AccountDim != null)
                        {
                            if (!dto.AccountInternals.ContainsKey(account.Account.AccountDim.AccountDimNr))
                                dto.AccountInternals.Add(account.Account.AccountDim.AccountDimNr, new AccountSmallDTO()
                                {
                                    AccountDimId = account.Account?.AccountDimId ?? 0,
                                    AccountId = account.AccountId,
                                    Number = account.Account?.AccountNr ?? String.Empty,
                                    Name = account.Account?.Name ?? String.Empty
                                });
                        }
                    }
                }
            }
            else if (setAccountInternalIds)
            {
                dto.AccountInternalIds = new List<int>();
                if (e.AccountInternal != null && e.AccountInternal.Count > 0)
                {
                    foreach (var account in e.AccountInternal)
                    {
                        dto.AccountInternalIds.Add(account.AccountId);
                    }
                }
            }

            if (includeAccountingSettings)
            {
                if (accountingSettingsOrdered)
                {
                    dto.AccountingSettings = new AccountingSettingsRowDTO()
                    {
                        AccountDim2Nr = e.AccountSettingDim2Nr ?? 0,
                        Account2Id = e.AccountSetting2Id ?? 0,
                        Account2Nr = e.AccountSetting2Nr,
                        Account2Name = e.AccountSetting2Name,
                        AccountDim3Nr = e.AccountSettingDim3Nr ?? 0,
                        Account3Id = e.AccountSetting3Id ?? 0,
                        Account3Nr = e.AccountSetting3Nr,
                        Account3Name = e.AccountSetting3Name,
                        AccountDim4Nr = e.AccountSettingDim4Nr ?? 0,
                        Account4Id = e.AccountSetting4Id ?? 0,
                        Account4Nr = e.AccountSetting4Nr,
                        Account4Name = e.AccountSetting4Name,
                        AccountDim5Nr = e.AccountSettingDim5Nr ?? 0,
                        Account5Id = e.AccountSetting5Id ?? 0,
                        Account5Nr = e.AccountSetting5Nr,
                        Account5Name = e.AccountSetting5Name,
                        AccountDim6Nr = e.AccountSettingDim6Nr ?? 0,
                        Account6Id = e.AccountSetting6Id ?? 0,
                        Account6Nr = e.AccountSetting6Nr,
                        Account6Name = e.AccountSetting6Name,
                    };
                }
                else
                {
                    dto.AccountingSettings = CreateAccountingSettingsRowDTO(e);
                }

            }
            if (includeSkills)
                dto.ShiftTypeSkills = e.ShiftTypeSkill?.ToDTOs().ToList() ?? new List<ShiftTypeSkillDTO>();

            if (includeShiftTypeLinkIds)
            {
                if (links == null)
                {
                    var entities = e.GetContext(out _);
                    if (entities != null)
                        links = entities.ShiftTypeLink.Where(l => l.ActorCompanyId == e.ActorCompanyId).ToList();
                }

                if (links != null)
                {
                    List<Guid> guids = links.Where(s => s.ShiftTypeId == e.ShiftTypeId).Select(t => t.Guid).ToList();
                    if (!guids.IsNullOrEmpty())
                        dto.LinkedShiftTypeIds = links.Where(l => guids.Contains(l.Guid)).Select(s => s.ShiftTypeId).Distinct().ToList();
                    else
                        dto.LinkedShiftTypeIds = new List<int>();
                }
                else
                {
                    dto.LinkedShiftTypeIds = new List<int>();
                }
            }

            if (includeEmployeeStatisticsTargets)
            {
                dto.EmployeeStatisticsTargets = new List<ShiftTypeEmployeeStatisticsTargetDTO>();
                if (e.ShiftTypeEmployeeStatisticsTarget != null && e.ShiftTypeEmployeeStatisticsTarget.Count > 0)
                    dto.EmployeeStatisticsTargets = e.ShiftTypeEmployeeStatisticsTarget.Where(t => t.State == (int)SoeEntityState.Active).ToDTOs().ToList();
            }

            if (loadCategories)
                dto.CategoryIds = e.CategoryIds;
            if (e.ShiftTypeHierarchyAccount != null)
                dto.HierarchyAccounts = e.ShiftTypeHierarchyAccount.Where(s => s.State == (int)SoeEntityState.Active).ToDTOs();
            if (e.ChildHierarchyAccountIds != null)
                dto.ChildHierarchyAccountIds = e.ChildHierarchyAccountIds;

            return dto;
        }

        private static AccountingSettingsRowDTO CreateAccountingSettingsRowDTO(ShiftType shiftType)
        {
            AccountingSettingsRowDTO accountingDto = new AccountingSettingsRowDTO();

            if (shiftType.AccountInternal != null)
            {
                int position = 1;
                foreach (AccountInternal accountInternal in shiftType.AccountInternal.Where(a => a.Account != null && a.Account.AccountDim != null && a.Account.AccountDim.AccountDimNr != Constants.ACCOUNTDIM_STANDARD).OrderBy(a => a.Account.AccountDim.AccountDimNr))
                {
                    position++;
                    accountingDto.SetAccountValues(position, accountInternal.Account.AccountDim.AccountDimNr, accountInternal.Account.AccountId, accountInternal.Account.AccountNr, accountInternal.Account.Name);
                }
            }

            return accountingDto;
        }

        public static IEnumerable<ShiftTypeDTO> ToDTOs(this IEnumerable<ShiftType> l, bool includeAccounts = false, bool includeSkills = false, bool includeEmployeeStatisticsTargets = false, bool includeAccountingSettings = false, bool includeShiftTypeLinkIds = false, bool setAccountInternalIds = false, List<ShiftTypeLink> links = null)
        {
            var dtos = new List<ShiftTypeDTO>();
            if (!l.IsNullOrEmpty())
            {
                #region Links

                if (includeShiftTypeLinkIds && links == null)
                {
                    var entity = l.FirstOrDefault();
                    if (entity != null)
                    {
                        var entities = entity.GetContext(out _);
                        if (entities != null)
                            links = entities.ShiftTypeLink.Where(s => s.ActorCompanyId == entity.ActorCompanyId).ToList();
                    }
                }

                #endregion

                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO(includeAccounts, includeSkills, includeEmployeeStatisticsTargets, includeAccountingSettings, includeShiftTypeLinkIds, links: links, setAccountInternalIds: setAccountInternalIds));
                }
            }
            return dtos;
        }

        public static ShiftTypeGridDTO ToGridDTO(this ShiftType e)
        {
            if (e == null)
                return null;

            return new ShiftTypeGridDTO()
            {
                ShiftTypeId = e.ShiftTypeId,
                TimeScheduleTemplateBlockType = e.TimeScheduleTemplateBlockType != null ? (TermGroup_TimeScheduleTemplateBlockType)e.TimeScheduleTemplateBlockType : (TermGroup_TimeScheduleTemplateBlockType?)null,
                TimeScheduleTemplateBlockTypeName = e.TimeScheduleTemplateBlockTypeName,
                Name = e.Name,
                NeedsCode = e.NeedsCode,
                NeedsCodeName = $"{e.NeedsCode} {e.Name}",
                Description = e.Description,
                TimeScheduleTypeId = e.TimeScheduleTypeId,
                TimeScheduleTypeName = e.TimeScheduleTypeName,
                CategoryNames = e.CategoryNames,
                Color = e.Color,
                DefaultLength = e.DefaultLength,    
                AccountId = e.AccountId,
                AccountingStringAccountNames = e.AccountingStringAccountNames,
                SkillNames = e.SkillNames,
                ExternalCode = e.ExternalCode,                
                AccountIsNotActive =  e.Account != null ? (e.Account.State != (int)SoeEntityState.Active) : false,
        };
        }

        public static IEnumerable<ShiftTypeGridDTO> ToGridDTOs(this IEnumerable<ShiftType> l)
        {
            var dtos = new List<ShiftTypeGridDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToGridDTO());
                }
            }
            return dtos;
        }

        public static ShiftType GetShiftType(this List<ShiftType> l, string code)
        {
            return
                l?.FirstOrDefault(e => e.ExternalCode == code) ??
                l?.FirstOrDefault(e => e.NeedsCode == code);
        }

        #endregion

        #region ShiftTypeHierarchyAccount

        public static ShiftTypeHierarchyAccountDTO ToDTO(this ShiftTypeHierarchyAccount e)
        {
            if (e == null)
                return null;

            return new ShiftTypeHierarchyAccountDTO()
            {
                ShiftTypeHierarchyAccountId = e.ShiftTypeHierarchyAccountId,
                AccountId = e.AccountId,
                AccountPermissionType = (TermGroup_AttestRoleUserAccountPermissionType)e.AccountPermissionType
            };
        }

        public static List<ShiftTypeHierarchyAccountDTO> ToDTOs(this IEnumerable<ShiftTypeHierarchyAccount> l)
        {
            List<ShiftTypeHierarchyAccountDTO> dtos = new List<ShiftTypeHierarchyAccountDTO>();
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

        #region ShiftTypeEmployeeStatisticsTarget

        public static ShiftTypeEmployeeStatisticsTargetDTO ToDTO(this ShiftTypeEmployeeStatisticsTarget e)
        {
            if (e == null)
                return null;

            return new ShiftTypeEmployeeStatisticsTargetDTO()
            {
                ShiftTypeEmployeeStatisticsTargetId = e.ShiftTypeEmployeeStatisticsTargetId,
                ShiftTypeId = e.ShiftTypeId,
                EmployeeStatisticsType = (TermGroup_EmployeeStatisticsType)e.EmployeeStatisticsType,
                TargetValue = e.TargetValue,
                FromDate = e.FromDate,
                EmployeeStatisticsTypeName = e.EmployeeStatisticsTypeName,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State
            };
        }

        public static IEnumerable<ShiftTypeEmployeeStatisticsTargetDTO> ToDTOs(this IEnumerable<ShiftTypeEmployeeStatisticsTarget> l)
        {
            var dtos = new List<ShiftTypeEmployeeStatisticsTargetDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos.OrderBy(d => d.EmployeeStatisticsType).ThenBy(d => d.FromDate);
        }

        #endregion
    }
}

using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects.DataClasses;
using System.Linq;

namespace SoftOne.Soe.Data
{
    public partial class AttestRoleUser : ICreatedModified, IState
    {
        public void SetParent(AttestRoleUser parent)
        {
            this.ParentAttestRoleUserId = parent?.AttestRoleUserId;
        }
        public void AddChildren(params AttestRoleUser[] children)
        {
            if (children.IsNullOrEmpty())
                return;

            foreach (var child in children)
            {
                child.ParentAttestRoleUserId = this.AttestRoleUserId;
            }
        }
    }

    public static partial class EntityExtensions
    {
        #region AttestRoleUser

        public static UserAttestRoleDTO ToDTO(this AttestRoleUser e, List<AccountDimDTO> accountDims, List<GenericType> permissionTypes, bool addAccountNameToAttestRole)
        {
            if (e == null)
                return null;

            var accountDim = e.Account != null ? accountDims?.FirstOrDefault(x => x.AccountDimId == e.Account.AccountDimId) : null;
            var permissionType = permissionTypes?.FirstOrDefault(p => p.Id == e.AccountPermissionType);

            UserAttestRoleDTO dto = new UserAttestRoleDTO()
            {
                AttestRoleUserId = e.AttestRoleUserId,
                ParentAttestRoleUserId = e.ParentAttestRoleUserId,
                AttestRoleId = e.AttestRoleId,
                UserId = e.UserId,
                Name = e.AttestRole?.Name ?? string.Empty,
                ModuleName = e.AttestRole?.ModuleName ?? string.Empty,
                DateFrom = e.DateFrom,
                DateTo = e.DateTo,
                MaxAmount = e.MaxAmount,
                AccountId = e.AccountId,
                AccountName = e.Account?.Name ?? string.Empty,
                AccountDimId = accountDim?.AccountDimId,
                AccountDimName = accountDim?.Name ?? string.Empty,
                IsExecutive = e.IsExecutive,
                IsNearestManager = e.IsNearestManager,
                AccountPermissionType = (TermGroup_AttestRoleUserAccountPermissionType)e.AccountPermissionType,
                AccountPermissionTypeName = permissionType?.Name,
                IsDelegated = e.UserCompanyRoleDelegateHistoryRowId.HasValue,
                State = (SoeEntityState)(e.AttestRole?.State ?? 0),
                AttestRoleSort = e.AttestRole?.Sort ?? 0,
                RoleId = e.RoleId
            };

            if (addAccountNameToAttestRole && !string.IsNullOrEmpty(dto.AccountName))
                dto.Name = $"{dto.Name} ({dto.AccountName})";
            if (e.Children != null)
                dto.Children = e.Children.Where(x => x.State == (int)SoeEntityState.Active).ToDTOs(accountDims, permissionTypes, addAccountNameToAttestRole);

            return dto;
        }

        public static List<UserAttestRoleDTO> ToDTOs(this IEnumerable<AttestRoleUser> l, List<AccountDimDTO> accountDims, List<GenericType> permissionTypes, bool addAccountNameToAttestRole)
        {
            var dtos = new List<UserAttestRoleDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO(accountDims, permissionTypes, addAccountNameToAttestRole));
                }
            }
            return dtos;
        }

        public static List<AttestRoleUser> FromDTOs(this IEnumerable<UserAttestRoleDTO> l, User user)
        {
            List<AttestRoleUser> attestRoleUsers = new List<AttestRoleUser>();

            if (l.IsNullOrEmpty() || user == null)
                return attestRoleUsers;

            foreach (UserAttestRoleDTO dto in l)
            {
                AttestRoleUser attestRoleUser = new AttestRoleUser()
                {
                    AttestRoleUserId = dto.AttestRoleUserId,
                    DateFrom = dto.DateFrom,
                    DateTo = dto.DateTo,
                    MaxAmount = dto.MaxAmount,
                    UserId = user.UserId,
                    AttestRoleId = dto.AttestRoleId,
                    AccountId = dto.AccountId,
                    IsExecutive = dto.IsExecutive,
                    IsNearestManager = dto.IsNearestManager,
                    AccountPermissionType = (int)dto.AccountPermissionType,
                    RoleId = dto.RoleId
                };

                if (dto.Children == null)
                    dto.Children = new List<UserAttestRoleDTO>();

                foreach (UserAttestRoleDTO childDto in dto.Children)
                {
                    AttestRoleUser child = new AttestRoleUser()
                    {
                        AttestRoleUserId = childDto.AttestRoleUserId,
                        ParentAttestRoleUserId = childDto.ParentAttestRoleUserId,
                        DateFrom = childDto.DateFrom,
                        DateTo = childDto.DateTo,
                        UserId = user.UserId,
                        AttestRoleId = dto.AttestRoleId,
                        AccountId = childDto.AccountId,
                        IsExecutive = childDto.IsExecutive,
                        IsNearestManager = childDto.IsNearestManager,
                        AccountPermissionType = (int)childDto.AccountPermissionType
                    };

                    if (attestRoleUser.Children == null)
                        attestRoleUser.Children = new EntityCollection<AttestRoleUser>();
                    attestRoleUser.Children.Add(child);

                    if (childDto.Children == null)
                        childDto.Children = new List<UserAttestRoleDTO>();

                    foreach (UserAttestRoleDTO subChildDto in childDto.Children)
                    {
                        AttestRoleUser subChild = new AttestRoleUser()
                        {
                            AttestRoleUserId = subChildDto.AttestRoleUserId,
                            ParentAttestRoleUserId = subChildDto.ParentAttestRoleUserId,
                            DateFrom = subChildDto.DateFrom,
                            DateTo = subChildDto.DateTo,
                            UserId = user.UserId,
                            AttestRoleId = dto.AttestRoleId,
                            AccountId = subChildDto.AccountId,
                            IsExecutive = subChildDto.IsExecutive,
                            IsNearestManager = subChildDto.IsNearestManager,
                            AccountPermissionType = (int)subChildDto.AccountPermissionType
                        };

                        if (child.Children == null)
                            child.Children = new EntityCollection<AttestRoleUser>();
                        child.Children.Add(subChild);
                    }
                }

                attestRoleUsers.Add(attestRoleUser);
            }

            return attestRoleUsers;
        }

        public static List<AttestRoleUser> GetParentsWithAccountIds(this List<AttestRoleUser> l)
        {
            return l.Where(i => i.AccountId.HasValue && !i.ParentAttestRoleUserId.HasValue).ToList();
        }

        public static List<AttestRoleUser> Filter(this List<AttestRoleUser> l, DateTime? date)
        {
            if (l.IsNullOrEmpty())
                return new List<AttestRoleUser>();

            List<AttestRoleUser> valid = new List<AttestRoleUser>();

            if (date.HasValue)
            {
                foreach (var e in l)
                {
                    if (CalendarUtility.IsDateInRange(date.Value, e.DateFrom, e.DateTo))
                        valid.Add(e);
                }
            }
            else
            {
                valid.AddRange(l);
            }

            return valid;
        }

        public static List<AttestRoleUser> Filter(this List<AttestRoleUser> l, DateTime? dateFrom, DateTime? dateTo, List<int> accountIds = null)
        {
            if (l.IsNullOrEmpty())
                return new List<AttestRoleUser>();
            if (!dateFrom.HasValue && !dateTo.HasValue)
                return l;

            List<AttestRoleUser> validAttestRoleUsers = new List<AttestRoleUser>();

            //Fix for dateTo has time but saved AttestRoleUser is beginning of day
            if (dateFrom.HasValue)
                dateFrom = dateFrom.Value.Date;
            if (dateTo.HasValue)
                dateTo = dateTo.Value.Date;

            foreach (var e in l)
            {
                bool isDatesValid = CalendarUtility.HasOverlappingDays(dateFrom ?? CalendarUtility.DATETIME_MINVALUE, dateTo ?? CalendarUtility.DATETIME_MAXVALUE, e.DateFrom ?? CalendarUtility.DATETIME_MINVALUE, e.DateTo ?? CalendarUtility.DATETIME_MAXVALUE);
                bool isAccountValid = accountIds == null || accountIds.Contains(e.AccountId ?? 0);
                if (isDatesValid && isAccountValid)
                    validAttestRoleUsers.Add(e);
            }

            return validAttestRoleUsers;
        }

        public static List<AttestRoleUser> Filter(this List<AttestRoleUser> l, DateTime? dateFrom, DateTime? dateTo, bool onlyWithAccountId, bool ignoreDates, bool onlyDefaultAccounts)
        {
            if (l.IsNullOrEmpty())
                return new List<AttestRoleUser>();

            if (onlyDefaultAccounts)
                l = l.Where(i => i.AccountPermissionType == (int)TermGroup_AttestRoleUserAccountPermissionType.Complete).ToList();
            else
                l = l.Where(i => i.AccountPermissionType == (int)TermGroup_AttestRoleUserAccountPermissionType.Complete ||
                                 i.AccountPermissionType == (int)TermGroup_AttestRoleUserAccountPermissionType.Secondary).ToList();

            if (onlyWithAccountId)
                l = l.Where(i => i.AccountId.HasValue).ToList();
            if (!ignoreDates)
                l = l.Filter(dateFrom, dateTo);

            return l;
        }

        public static AttestRoleUser GetClosestAttestRole(this IEnumerable<AttestRoleUser> l, UserAttestRoleDTO userAttestRole, DateTime minDate, DateTime maxDate)
        {
            if (l == null || userAttestRole == null)
                return null;

            List<AttestRoleUser> matchingAttestRoles = l.Where(a => a.AttestRoleId == userAttestRole.AttestRoleId && (!userAttestRole.AccountId.HasValue || userAttestRole.AccountId == a.AccountId) && a.State == (int)SoeEntityState.Active && CalendarUtility.IsDatesOverlapping(userAttestRole.DateFrom ?? minDate, userAttestRole.DateTo ?? maxDate, a.DateFrom ?? minDate, (a.DateTo.HasValue && a.DateTo.Value != CalendarUtility.DATETIME_DEFAULT ? a.DateTo.Value : maxDate))).ToList();

            AttestRoleUser e = null;
            if (matchingAttestRoles.Count > 1)
                e = matchingAttestRoles.FirstOrDefault(i => !i.DateTo.HasValue);
            if (e == null)
                e = matchingAttestRoles.OrderByDescending(i => i.DateTo).FirstOrDefault();
            return e;
        }

        public static List<AccountDTO> GetValidAccounts(this List<AttestRoleUser> l, List<AccountDimDTO> accountDims, List<AccountDTO> possibleValidAccounts, DateTime? startDate, DateTime? stopDate)
        {
            if (possibleValidAccounts.IsNullOrEmpty())
                return new List<AccountDTO>();

            if (!startDate.HasValue)
                startDate = DateTime.Today;
            if (!stopDate.HasValue)
                stopDate = DateTime.Today;

            possibleValidAccounts.ResetVirtualParentId();

            if (l.ShowAll(startDate))
                return possibleValidAccounts.GetHighestAccounts(accountDims);

            List<AccountDTO> validAccounts = new List<AccountDTO>();
            List<AttestRoleUser> parents = l.GetParentsWithAccountIds();
            foreach (AttestRoleUser parent in parents)
            {
                if (!IsAttestRoleUserAccountValid(parent, out AccountDTO parentAccount))
                    continue;

                List<AttestRoleUser> childs = l.GetChildrensWithAccountId(parent);
                if (childs.IsNullOrEmpty())
                {
                    validAccounts.Add(parentAccount);
                }
                else
                {
                    foreach (AttestRoleUser child in childs)
                    {
                        if (!IsAttestRoleUserAccountValid(child, out AccountDTO childAccount))
                            continue;

                        List<AttestRoleUser> grandChilds = l.GetChildrensWithAccountId(child);                                               
                        if (grandChilds.IsNullOrEmpty())
                        {
                            if (childAccount != null && !childAccount.ParentAccountId.HasValue && validAccounts.Exists(a => a.AccountId == childAccount.AccountId))
                                childAccount = childAccount.CloneDTO(); //Account is virtual and already added as valid, must clone to prevent first is overrided by second

                            childAccount.TrySetVirtualParentId(parentAccount);
                            validAccounts.Add(childAccount);
                        }
                        else
                        {
                            childAccount.TrySetVirtualParentId(parentAccount);
                            foreach (AttestRoleUser grandChild in grandChilds)
                            {
                                if (IsAttestRoleUserAccountValid(grandChild, out AccountDTO grandChildAccount))
                                {
                                    validAccounts.Add(grandChildAccount);
                                    grandChildAccount.TrySetVirtualParentId(childAccount);
                                }
                            }
                        }
                    }
                }
            }

            bool IsAttestRoleUserAccountValid(AttestRoleUser attestRoleUser, out AccountDTO account)
            {
                account = attestRoleUser.IsValid(startDate, stopDate) ? possibleValidAccounts.FirstOrDefault(i => i.AccountId == attestRoleUser.AccountId.Value) : null;
                return account != null;
            }

            return validAccounts;
        }

        public static List<int> GetAccountIds(this List<AttestRoleUser> l)
        {
            return l?.Where(i => i.AccountId.HasValue).Select(i => i.AccountId.Value).ToList() ?? new List<int>();
        }

        public static List<AttestRoleUser> GetSortedAttestRoleUsers(this List<AttestRoleUser> l)
        {
            return l.OrderByDescending(o => o.AttestRole?.Sort ?? int.MaxValue).ToList();
        }

        public static AttestRoleUser GetExecutiveAttestRoleUser(this List<AttestRoleUser> l, List<int> userIds, int startSort = 0)
        {
            var sorted = l.Where(w => userIds.Contains(w.UserId)).ToList().GetSortedAttestRoleUsers().Where(w => w.AttestRole != null && w.AttestRole.Sort > startSort).ToList();

            if (sorted.Count == 1)
                return sorted.First();

            if (sorted.Count > 1 && sorted.First().AttestRole.Sort != sorted.Skip(1).First().AttestRole.Sort)
                return sorted.First();

            return null;

        }

        public static List<UserAttestRoleDTO> GetSortedAttestRoleUsers(this List<UserAttestRoleDTO> l)
        {
            return l.OrderBy(o => o.AttestRoleSort ?? int.MaxValue).ToList();
        }

        public static List<UserAttestRoleDTO> GetExecutiveAttestRoleUsers(this List<UserAttestRoleDTO> l, List<int> userIds, int startSort = 0)
        {
            var sorted = l.Where(w => userIds.Contains(w.UserId)).ToList().GetSortedAttestRoleUsers().Where(w => w.AttestRoleSort > startSort).ToList();

            if (sorted.Count <= 1)
                return sorted;

            var sortLevel = sorted.First().AttestRoleSort;
            return sorted.Where(w => w.AttestRoleSort == sortLevel).ToList();
        }
        
        public static UserAttestRoleDTO GetExecutiveAttestRoleUser(this List<UserAttestRoleDTO> l, List<int> userIds, int startSort = 0)
        {
            var sorted = l.Where(w => userIds.Contains(w.UserId)).ToList().GetSortedAttestRoleUsers().Where(w => w.AttestRoleSort > startSort).ToList();

            if (sorted.Count == 1)
                return sorted.First();

            if (sorted.Count > 1 && sorted.First().AttestRoleSort != sorted.Skip(1).First().AttestRoleSort)
                return sorted.First();

            return null;

        }

        public static Dictionary<DateTime, List<int>> GetValidAccountIdsByDate(this List<AttestRoleUser> l, DateTime dateFrom, DateTime dateTo)
        {
            Dictionary<DateTime, List<int>> dict = new Dictionary<DateTime, List<int>>();

            DateTime date = dateFrom;
            while (date <= dateTo)
            {
                dict.Add(date, l.Filter(date).GetAccountIds());
                date = date.AddDays(1);
            }

            return dict;
        }

        public static List<AttestRoleUser> GetByUser(this List<AttestRoleUser> l, int userId, DateTime? dateFrom = null, DateTime? dateTo = null)
        {
            return l?
                .Where(e =>
                    e.UserId == userId &&
                    (!dateFrom.HasValue || !e.DateFrom.HasValue || dateFrom.Value >= e.DateFrom.Value) &&
                    (!dateTo.HasValue || !e.DateTo.HasValue || dateTo.Value >= e.DateTo.Value)
                ).ToList() ?? new List<AttestRoleUser>();
        }

        public static List<AttestRoleUser> GetChildrensWithAccountId(this List<AttestRoleUser> l, AttestRoleUser parent)
        {
            return l.Where(i => i.AccountId.HasValue && i.ParentAttestRoleUserId == parent.AttestRoleUserId).ToList();
        }

        public static bool HasValidTransition(this List<AttestUserRoleView> l, AttestTransitionDTO transition, DateTime? date)
        {
            if (l.IsNullOrEmpty() || transition == null)
                return false;

            return l.Any(v =>
            v.AttestStateFromId == transition.AttestStateFromId &&
            v.AttestStateToId == transition.AttestStateToId &&
            (!date.HasValue || v.DateFrom <= date.Value) &&
            (!date.HasValue || v.DateTo >= date.Value));
        }

        public static bool ShowAll(this List<AttestRoleUser> l, DateTime? date = null)
        {
            if (l.IsNullOrEmpty())
                return false;
            return l.Any(i => i.AttestRole != null && i.AttestRole.ShowAllCategories && (!date.HasValue || i.IsValid(date.Value)));
        }

        public static bool ShowUncategorized(this List<AttestRoleUser> l, DateTime? date = null)
        {
            if (l.IsNullOrEmpty())
                return false;
            return l.Any(i => i.AttestRole != null && i.AttestRole.ShowUncategorized && (!date.HasValue || i.IsValid(date.Value)));
        }

        public static bool AlsoAttestAdditionsFromTime(this List<AttestRoleUser> l, DateTime? date = null)
        {
            if (l.IsNullOrEmpty())
                return false;
            return l.Any(i => i.AttestRole != null && i.AttestRole.AlsoAttestAdditionsFromTime && (!date.HasValue || i.IsValid(date.Value)));
        }

        public static bool IsValid(this AttestRoleUser e, DateTime date)
        {
            return (!e.DateFrom.HasValue || e.DateFrom.Value <= date) && (!e.DateTo.HasValue || e.DateTo.Value >= date);
        }

        public static bool IsValid(this AttestRoleUser e, DateTime? dateFrom, DateTime? dateTo)
        {
            if (e == null)
                return false;
            if (dateFrom.HasValue && dateTo.HasValue && dateFrom.Value.Date == dateTo.Value.Date)
                return e.IsValid(dateFrom.Value);
            if (!dateFrom.HasValue && !dateTo.HasValue)
                return true;
            if (CalendarUtility.HasOverlappingDays(
                    dateFrom ?? CalendarUtility.DATETIME_MINVALUE,
                    dateTo ?? CalendarUtility.DATETIME_MAXVALUE,
                    e.DateFrom ?? CalendarUtility.DATETIME_MINVALUE,
                    e.DateTo ?? CalendarUtility.DATETIME_MAXVALUE))
                return true;
            return false;
        }

        public static bool IsModified(this AttestRoleUser e, AttestRoleUser other)
        {
            if (e == null || other == null)
                return false;

            return e.AttestRoleId != other.AttestRoleId ||
                   e.AccountId != other.AccountId ||
                   e.DateFrom != other.DateFrom ||
                   e.DateTo != other.DateTo ||
                   e.MaxAmount != other.MaxAmount ||
                   e.AccountPermissionType != other.AccountPermissionType ||
                   e.IsExecutive != other.IsExecutive ||
                   e.IsNearestManager != other.IsNearestManager ||
                   e.RoleId != other.RoleId;
        }

        public static bool IsChildModified(this AttestRoleUser e, AttestRoleUser other)
        {
            if (e == null || other == null)
                return false;

            return e.AccountId != other.AccountId ||
                   e.DateFrom != other.DateFrom ||
                   e.DateTo != other.DateTo ||
                   e.AccountPermissionType != other.AccountPermissionType ||
                   e.IsExecutive != other.IsExecutive ||
                   e.IsNearestManager != other.IsNearestManager;
        }

        public static void Update(this AttestRoleUser e, AttestRoleUser other)
        {
            if (e == null || other == null)
                return;

            e.AttestRoleId = other.AttestRoleId;
            e.DateFrom = other.DateFrom;
            e.DateTo = other.DateTo;
            e.MaxAmount = other.MaxAmount;
            e.AccountId = other.AccountId.HasValue && other.AccountId.Value != 0 ? other.AccountId : (int?)null;
            e.AccountPermissionType = other.AccountPermissionType;
            e.IsExecutive = other.IsExecutive;
            e.IsNearestManager = other.IsNearestManager;
            e.RoleId = (other.RoleId == 0 ? null : other.RoleId);
        }

        public static void UpdateChild(this AttestRoleUser child, AttestRoleUser childInput)
        {
            if (child == null || childInput == null)
                return;

            child.DateFrom = childInput.DateFrom;
            child.DateTo = childInput.DateTo;
            child.AccountId = childInput.AccountId.HasValue && childInput.AccountId.Value != 0 ? childInput.AccountId : (int?)null;
            child.AccountPermissionType = childInput.AccountPermissionType;
            child.IsExecutive = childInput.IsExecutive;
            child.IsNearestManager = childInput.IsNearestManager;
        }

        #endregion
    }
}

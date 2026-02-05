using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Data
{
    public partial class UserCompanyRole : IUserCompanyRole, ICreatedModified, IState
    {
        public int StateId { get { return this.State; } }
    }

    public static partial class EntityExtensions
    {
        #region UserCompanyRole

        public static bool IsCurrent(this UserCompanyRole e, int actorCompanyId, int roleId, int userId)
        {
            return e != null && e.ActorCompanyId == actorCompanyId && e.RoleId == roleId && e.UserId == userId;
        }

        public static bool IsModified(this UserCompanyRole e, UserCompanyRoleDTO other)
        {
            if (e == null || other == null)
                return false;

            return e.Default != other.Default ||
                   e.DateFrom != other.DateFrom ||
                   e.DateTo != other.DateTo;
        }

        public static void Update(this UserCompanyRole e, UserCompanyRoleDTO other)
        {
            if (e == null || other == null)
                return;

            e.Default = other.Default;
            e.DateFrom = other.DateFrom;
            e.DateTo = other.DateTo;
        }

        public static List<UserCompanyRole> GetSortedAttestRoleUsers(this IEnumerable<UserCompanyRole> l)
        {
            return l?.OrderByDescending(o => o.Role?.Sort ?? int.MaxValue).ToList() ?? new List<UserCompanyRole>();
        }

        public static List<UserCompanyRoleDTO> GetExecutiveUserCompanyRoleUsers(this List<UserCompanyRoleDTO> l, List<int> userIds, int startSort = 0)
        {
            var sorted = l.Where(w => userIds.Contains(w.UserId)).ToList().GetSortedAttestRoleUsers().Where(w => w.RoleSort > startSort).ToList();
            if (!sorted.Any())
                return new List<UserCompanyRoleDTO>();

            var sort = sorted.First().RoleSort;
            return sorted.Where(w => w.RoleSort == sort).ToList();
        }

        public static List<UserCompanyRole> GetByUser(this IEnumerable<UserCompanyRole> l, int userId, DateTime? startDate = null, DateTime? stopDate = null)
        {
            return l?
                .Where(e => e.UserId == userId && 
                (!startDate.HasValue || !e.DateFrom.HasValue || startDate.Value >= e.DateFrom.Value) && 
                (!stopDate.HasValue || !e.DateTo.HasValue || stopDate.Value >= e.DateTo.Value))
                .ToList() ?? new List<UserCompanyRole>();
        }

        public static UserCompanyRole GetExecutiveUserCompanyRoleUser(this IEnumerable<UserCompanyRole> l, List<int> userIds, int startSort = 0)
        {
            var sorted = l?
                .Where(w => userIds.Contains(w.UserId))
                .GetSortedAttestRoleUsers()
                .Where(w => w.Role != null && w.Role.Sort > startSort)
                .ToList() ?? new List<UserCompanyRole>();

            if (sorted.Count == 1)
                return sorted.First();
            if (sorted.Count > 1 && sorted.First().Role.Sort != sorted.Skip(1).First().Role.Sort)
                return sorted.First();
            return null;
        }

        #endregion

        #region UserCompanyRoleDelegateHistory

        public static UserCompanyRoleDelegateHistoryHeadDTO ToDTO(this UserCompanyRoleDelegateHistoryHead e)
        {
            if (e == null)
                return null;

            return new UserCompanyRoleDelegateHistoryHeadDTO()
            {
                UserCompanyRoleDelegateHistoryHeadId = e.UserCompanyRoleDelegateHistoryHeadId,
                ActorCompanyId = e.ActorCompanyId,
                FromUserId = e.FromUserId,
                ToUserId = e.ToUserId,
                ByUserId = e.ByUserId,
                Rows = e.UserCompanyRoleDelegateHistoryRow.ToDTOs(),
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State,
            };
        }

        public static List<UserCompanyRoleDelegateHistoryHeadDTO> ToDTOs(this IEnumerable<UserCompanyRoleDelegateHistoryHead> l)
        {
            List<UserCompanyRoleDelegateHistoryHeadDTO> dtos = new List<UserCompanyRoleDelegateHistoryHeadDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static UserCompanyRoleDelegateHistoryRowDTO ToDTO(this UserCompanyRoleDelegateHistoryRow e)
        {
            if (e == null)
                return null;

            return new UserCompanyRoleDelegateHistoryRowDTO()
            {
                UserCompanyRoleDelegateHistoryRowId = e.UserCompanyRoleDelegateHistoryRowId,
                UserCompanyRoleDelegateHistoryHeadId = e.UserCompanyRoleDelegateHistoryHeadId,
                ParentId = e.ParentId,
                RoleId = e.RoleId,
                AttestRoleId = e.AttestRoleId,
                AccountId = e.AccountId,
                DateFrom = e.DateFrom,
                DateTo = e.DateTo,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State,
            };
        }

        public static List<UserCompanyRoleDelegateHistoryRowDTO> ToDTOs(this IEnumerable<UserCompanyRoleDelegateHistoryRow> l)
        {
            List<UserCompanyRoleDelegateHistoryRowDTO> dtos = new List<UserCompanyRoleDelegateHistoryRowDTO>();
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

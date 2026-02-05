using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace SoftOne.Soe.Data
{
    public partial class User : IUser, ICreatedModified, IState
    {
        public string LoginNameSort
        {
            get
            {
                string source = LoginName;
                return source.All(c => Char.IsDigit(c)) ? source.PadLeft(50, '0') : source; 
            }
        }
        public string LoggedIn { get; set; }
        public UserSession LastUserSession { get; set; }
        public string DefaultRoleName { get; set; }
        public string SocialSec
        {
            get
            {
                return this.ContactPerson != null ? StringUtility.NullToEmpty(this.ContactPerson.SocialSec) : String.Empty;
            }
        }
        private bool? isAdmin = null;
        public string ExternalAuthId { get; set; }
        public string SoftOneIdLoginName { get; set; }
        public string Categories { get; set; }
        //Change DefaultRoleId to ActiveRoleId when field is removed in db
        public int ActiveRoleId
        {
            get
            {
                return this.DefaultRoleId;
            }
            set
            {
                this.DefaultRoleId = value;
            }
        }
        public void SetRole(Role role)
        {
            if (role == null)
                return;

            this.ActiveRoleId = role.RoleId;
            if (!this.isAdmin.HasValue)
                this.isAdmin = this.SysUser || role.IsAdmin;
        }
        public bool IsAdmin(int activeRoleId)
        {
            if (!this.isAdmin.HasValue && activeRoleId > 0)
            {
                this.isAdmin = false;
                if (this.SysUser)
                {
                    this.isAdmin = true;
                }
                else
                {
                    var entities = this.GetContext(out bool createdNewContext);
                    if (entities != null)
                    {
                        this.isAdmin = entities.Role.FirstOrDefault(i => i.RoleId == activeRoleId)?.IsAdmin ?? false;
                        if (createdNewContext)
                            entities.Dispose();
                    }
                }
            }
            return this.isAdmin ?? false;
        }
    }

    public partial class UserSession
    {
        public string LoginMonth
        {
            get
            {
                return $"{this.Login.Year}{this.Login.Month.ToString().PadLeft(2, '0')}";
            }
        }
    }

    public static partial class EntityExtensions
    {
        #region User

        public static UserDTO ToDTO(this User e, int defaultRoleId = 0)
        {
            if (e == null)
                return null;

            UserDTO dto = new UserDTO()
            {
                LicenseId = e.LicenseId,
                LicenseNr = e.License?.LicenseNr ?? string.Empty,
                LicenseGuid = e.License?.LicenseGuid ?? Guid.Empty,
                DefaultActorCompanyId = e.DefaultActorCompanyId,
                UserId = e.UserId,
                LoginName = e.LoginName,
                Name = e.Name,
                Email = e.Email,
                State = (SoeEntityState)e.State,
                ChangePassword = e.ChangePassword,
                LangId = e.LangId,
                IsSuperAdmin = e.IsSuperAdmin,
                EstatusLoginId = e.EstatusLoginId,
                IsMobileUser = e.IsMobileUser,
                BlockedFromDate = e.BlockedFromDate,
                idLoginGuid = e.idLoginGuid,
                EmailCopy = e.EmailCopy,
            };

            if (defaultRoleId > 0)
            {
                dto.DefaultRoleId = defaultRoleId;
                dto.IsAdmin = e.IsAdmin(defaultRoleId);
            }

            return dto;
        }

        public static UserDTO Clone(this UserDTO e)
        {
            if (e == null)
                return null;

            UserDTO dto = new UserDTO()
            {
                LicenseId = e.LicenseId,
                LicenseNr = e.LicenseNr,
                LicenseGuid = e.LicenseGuid,
                DefaultActorCompanyId = e.DefaultActorCompanyId,
                UserId = e.UserId,
                LoginName = e.LoginName,
                Name = e.Name,
                Email = e.Email,
                State = e.State,
                ChangePassword = e.ChangePassword,
                LangId = e.LangId,
                IsSuperAdmin = e.IsSuperAdmin,
                EstatusLoginId = e.EstatusLoginId,
                IsMobileUser = e.IsMobileUser,
                BlockedFromDate = e.BlockedFromDate,
                idLoginGuid = e.idLoginGuid,
                EmailCopy = e.EmailCopy,
                DefaultRoleId = e.DefaultRoleId,
                IsAdmin = e.IsAdmin
            };

            return dto;
        }

        public static IEnumerable<UserDTO> ToDTOs(this IEnumerable<User> l)
        {
            var dtos = new List<UserDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static UserSmallDTO ToSmallDTO(this User e)
        {
            if (e == null)
                return null;

            return new UserSmallDTO()
            {
                LicenseId = e.LicenseId,
                DefaultActorCompanyId = e.DefaultActorCompanyId,
                UserId = e.UserId,
                LoginName = e.LoginName,
                Name = e.Name,
                Email = e.Email,
                LangId = e.LangId,
                BlockedFromDate = e.BlockedFromDate,
                IdLoginActive = e.idLoginActive,
                State = (SoeEntityState)e.State,
                ChangePassword = e.ChangePassword,
                Categories = e.Categories,
                DefaultRoleName = e.DefaultRoleName,
            };
        }

        public static UserForOriginDTO ToForOriginDTO(this User e)
        {
            if (e == null)
                return null;

            return new UserForOriginDTO()
            {
                UserId = e.UserId,
                LoginName = e.LoginName,
                Name = e.Name,
                Categories = e.Categories,
            };
        }

        public static IEnumerable<UserSmallDTO> ToSmallDTOs(this IEnumerable<User> l)
        {
            var dtos = new List<UserSmallDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToSmallDTO());
                }
            }
            return dtos;
        }

        public static IEnumerable<UserForOriginDTO> ToForOriginDTOs(this IEnumerable<User> l)
        {
            var dtos = new List<UserForOriginDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToForOriginDTO());
                }
            }
            return dtos;
        }

        public static UserGridDTO ToGridDTO(this User e)
        {
            if (e == null)
                return null;

            return new UserGridDTO()
            {
                UserId = e.UserId,
                LoginName = e.LoginName,
                Name = e.Name,
                DefaultRoleName = e.DefaultRoleName,
                Email = e.Email,
                IdLoginActive = e.idLoginActive,
                State = (SoeEntityState)e.State,
                DefaultActorCompanyId = e.DefaultActorCompanyId,
                ExternalAuthId = e.ExternalAuthId,
                SoftOneIdLoginName = e.SoftOneIdLoginName,
            };
        }

        public static IEnumerable<UserGridDTO> ToGridDTOs(this IEnumerable<User> l)
        {
            var dtos = new List<UserGridDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToGridDTO());
                }
            }
            return dtos;
        }

        public static UserRequestTypeDTO ToRequestTypeDTO(this User e, TermGroup_EmployeeRequestTypeFlags? type)
        {
            if (e == null)
                return null;

            return new UserRequestTypeDTO()
            {
                UserId = e.UserId,
                LoginName = e.LoginName,
                Name = e.Name,
                State = (SoeEntityState)e.State,
                EmployeeRequestTypes = type ?? TermGroup_EmployeeRequestTypeFlags.Undefined,
            };
        }

        public static IEnumerable<UserRequestTypeDTO> ToRequestTypeDTOs(this IEnumerable<User> l)
        {
            var dtos = new List<UserRequestTypeDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToRequestTypeDTO(TermGroup_EmployeeRequestTypeFlags.Undefined));
                }
            }
            return dtos;
        }

        public static Dictionary<int, string> ToDict(this List<User> users, bool addEmptyRow, bool includeKey, bool useFullName, bool includeLoginName)
        {
            Dictionary<int, string> dict = new Dictionary<int, string>();

            if (addEmptyRow)
                dict.Add(0, " ");

            List<User> usersSorted = (useFullName && !includeLoginName ? users.OrderBy(u => u.Name) : users.OrderBy(u => u.LoginName.PadLeft(50, '0'))).Distinct().ToList();
            foreach (User user in usersSorted)
            {
                string name = string.Empty;
                if (includeKey)
                    name += string.Format("{0}. ", user.UserId);
                else if (includeLoginName)
                    name += string.Format("({0}) ", user.LoginName);

                name += useFullName ? user.Name : user.LoginName;
                dict.Add(user.UserId, name);
            }

            return dict;
        }

        public static List<UserCompanyRole> GetRoles(this User e, DateTime? date)
        {
            if (!date.HasValue)
                date = DateTime.Now;

            return e?.UserCompanyRole?.Where(r =>
                    (!r.DateFrom.HasValue || r.DateFrom.Value <= date) &&
                    (!r.DateTo.HasValue || r.DateTo.Value >= date) &&
                    r.State == (int)SoeEntityState.Active).ToList() ?? new List<UserCompanyRole>();
        }

        public static List<AttestRoleUser> GetAttestRoles(this User e, DateTime? date)
        {
            if (!date.HasValue)
                date = DateTime.Now;

            return e?.AttestRoleUser?.Where(r =>
                    (!r.DateFrom.HasValue || r.DateFrom.Value <= date) &&
                    (!r.DateTo.HasValue || r.DateTo.Value >= date) &&
                    r.State == (int)SoeEntityState.Active).ToList() ?? new List<AttestRoleUser>();
        }

        public static bool HasUserRole(this User e, int activeRoleId, int fromId, int? toId = null, DateTime? date = null)
        {
            if (e.IsAdmin(activeRoleId))
                return true;

            List<int> ids = fromId.ObjToList();
            if (toId.HasValue)
                ids.Add(toId.Value);

            var roles = e.GetRoles(date);
            return ids.Distinct().All(id => roles.Any(r => r.RoleId == id));
        }

        public static bool HasAttestRole(this User e, int activeRoleId, int fromId, int? toId = null, DateTime? date = null)
        {
            if (e.IsAdmin(activeRoleId))
                return true;

            List<int> ids = fromId.ObjToList();
            if (toId.HasValue)
                ids.Add(toId.Value);

            var roles = e.GetAttestRoles(date);
            return ids.Distinct().All(id => roles.Any(r => r.AttestRoleId == id));
        }

        public static List<User> ToList(this IQueryable<User> query, DateTime date, bool? active)
        {
            query = query.Where(u => 
                !u.UserCompanyRole.Any() || 
                 u.UserCompanyRole.Any(ucr => 
                    ucr.State == (int)SoeEntityState.Active &&
                    (!ucr.DateFrom.HasValue || ucr.DateFrom <= date) &&
                    (!ucr.DateTo.HasValue || ucr.DateTo >= date)
                    )
                );

            if (active == true)
                query = query.Where(u => u.State == (int)SoeEntityState.Active);
            else if (active == false)
                query = query.Where(u => u.State == (int)SoeEntityState.Inactive);
            else
                query = query.Where(u => u.State != (int)SoeEntityState.Deleted);

            return query.ToList();
        }

        public static List<User> ToList(this IQueryable<User> query, DateTime dateFrom, DateTime dateTo, bool? active)
        {
            query = query.Include(u => u.UserCompanyRole);

            if (active == true)
                query = query.Where(u => u.State == (int)SoeEntityState.Active);
            else if (active == false)
                query = query.Where(u => u.State == (int)SoeEntityState.Inactive);
            else
                query = query.Where(u => u.State != (int)SoeEntityState.Deleted);

            var list = query.ToList();

            list = list.Where(u =>
                !u.UserCompanyRole.Any() ||
                u.UserCompanyRole.Any(ucr =>
                    ucr.State == (int)SoeEntityState.Active &&
                    CalendarUtility.IsDatesOverlapping(
                        dateFrom,
                        dateTo,
                        ucr.DateFrom ?? DateTime.MinValue,
                        ucr.DateTo ?? DateTime.MaxValue,
                        validateDatesAreTouching: true
                        )
                )
            ).ToList();

            return list;
        }


        public static List<User> ToList(this IQueryable<UserCompanyRole> query, DateTime dateFrom, DateTime dateTo, bool? active)
        {
            if (active == true)
                query = query.Where(ucr => ucr.User.State == (int)SoeEntityState.Active);
            else if (active == false)
                query = query.Where(ucr => ucr.User.State == (int)SoeEntityState.Inactive);
            else
                query = query.Where(ucr => ucr.User.State != (int)SoeEntityState.Deleted);

            var list = query
                .Select(ucr => ucr.User)
                .Include(u => u.UserCompanyRole)
                .Distinct()
                .ToList();

            list = list.Where(u =>
                u.UserCompanyRole.Any(ucr => 
                    ucr.State == (int)SoeEntityState.Active && 
                    CalendarUtility.IsDatesOverlapping(
                        dateFrom, 
                        dateTo,
                        ucr.DateFrom ?? DateTime.MinValue,
                        ucr.DateTo ?? DateTime.MaxValue,
                        validateDatesAreTouching: true
                        )
                )
            ).ToList();

            return list;
        }

        public static List<User> OrderByLogin(this List<User> l)
        {
            return l?.OrderBy(u => u.LoginNameSort).ToList() ?? new List<User>();
        }

        #endregion

        #region UserSession

        public static UserSessionDTO ToDTO(this UserSession e)
        {
            if (e == null)
                return null;

            return new UserSessionDTO()
            {
                UserSessionId = e.UserSessonId,
                Login = e.Login,
                Logout = e.Logout,
                RemoteLogin = e.RemoteLogin,
                Description = e.Description,
                UserId = e.User?.UserId ?? 0,
                MobileLogin = e.MobileLogin,
                Browser = e.Browser,
                Screen = e.Screen,
                Silverlight = e.Silverlight,
                Platform = e.Platform,
                ClientIP = e.ClientIP,
                Host = e.Host,
                CacheCredentials = e.CacheCredentials,
                Token = e.Token
            };
        }

        public static IEnumerable<UserSessionDTO> ToDTOs(this IEnumerable<UserSession> l)
        {
            var dtos = new List<UserSessionDTO>();
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

        #region UserReplacement

        public static UserReplacementDTO ToDTO(this UserReplacement e)
        {
            if (e == null)
                return null;

            return new UserReplacementDTO()
            {
                UserReplacementId = e.UserReplacementId,
                ActorCompanyId = e.ActorCompanyId,
                Type = (UserReplacementType)e.Type,
                OriginUserId = e.OriginUserId,
                ReplacementUserId = e.ReplacementUserId,
                StartDate = e.StartDate,
                StopDate = e.StopDate,
                State = (SoeEntityState)e.State
            };
        }

        #endregion
    }
}

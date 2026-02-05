using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using System.Collections.Generic;

namespace SoftOne.Soe.Data
{
    public partial class Role : ICreatedModified, IState
    {
        public bool IsAdmin
        {
            get
            {
                return this.TermId.HasValue && this.TermId.Value == (int)TermGroup_Roles.Systemadmin;
            }
        }
        public string ActualName { get; set; }
        public string SystemRoleName { get; set; }
        public List<string> ExternalCodes { get; set; }
        public string ExternalCodesString { get; set; }
    }

    public static partial class EntityExtensions
    {
        #region Role

        public static RoleDTO ToDTO(this Role e)
        {
            if (e == null)
                return null;

            RoleDTO dto = new RoleDTO()
            {
                RoleId = e.RoleId,
                ActorCompanyId = e.ActorCompanyId,
                Name = e.Name,
                ActualName = e.ActualName ?? e.Name,
                TermId = e.TermId,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State,
                Sort = e.Sort
            };

            if (!e.ExternalCodes.IsNullOrEmpty())
            {
                dto.ExternalCodes = e.ExternalCodes;
                dto.ExternalCodesString = e.ExternalCodesString;
            }

            return dto;
        }

        public static IEnumerable<RoleDTO> ToDTOs(this IEnumerable<Role> l)
        {
            var dtos = new List<RoleDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static RoleEditDTO ToEditDTO(this Role e)
        {
            if (e == null)
                return null;

            RoleEditDTO dto = new RoleEditDTO()
            {
                RoleId = e.RoleId,
                Name = e.Name,
                ExternalCodesString = e.ExternalCodesString,
                FavoriteOption = e.FavoriteOption,
                IsAdmin = e.IsAdmin,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State,
                Sort = e.Sort
            };

            if (string.IsNullOrEmpty(dto.Name) && dto.IsAdmin)
                dto.Name = e.SystemRoleName;

            return dto;
        }

        public static RoleGridDTO ToGridDTO(this Role e)
        {
            if (e == null)
                return null;

            return new RoleGridDTO()
            {
                RoleId = e.RoleId,
                Name = e.Name,
                ExternalCodesString = e.ExternalCodesString,
                Sort = e.Sort,
                State = (SoeEntityState)e.State
            };
        }

        public static List<RoleGridDTO> ToGridDTOs(this IEnumerable<Role> l)
        {
            List<RoleGridDTO> dtos = new List<RoleGridDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToGridDTO());
                }
            }
            return dtos;
        }

        public static UserCompanyRoleDTO ToDTO(this UserCompanyRole e)
        {
            if (e == null)
                return null;

            return new UserCompanyRoleDTO()
            {
                UserId = e.UserId,
                ActorCompanyId = e.ActorCompanyId,
                RoleId = e.RoleId,
                Name = e.Role?.Name,
                Default = e.Default,
                DateFrom = e.DateFrom,
                RoleSort = e.Role?.Sort ?? 0,
                DateTo = e.DateTo,
                State = (SoeEntityState)e.State,
                UserCompanyRoleId = e.UserCompanyRoleId,
                IsDelegated = e.UserCompanyRoleDelegateHistoryRowId.HasValue                
            };
        }

        public static List<UserCompanyRoleDTO> ToDTOs(this IEnumerable<UserCompanyRole> l)
        {
            List<UserCompanyRoleDTO> dtos = new List<UserCompanyRoleDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static UserAttestRoleDTO ToDTO(this AttestRoleUser e)
        {
            if (e == null)
                return null;

            return new UserAttestRoleDTO()
            {
                AttestRoleUserId = e.AttestRoleUserId,
                AttestRoleId = e.AttestRoleId,
                UserId = e.UserId,
                Name = e.AttestRole?.Name,
                DateFrom = e.DateFrom,
                DateTo = e.DateTo,
                MaxAmount = e.MaxAmount,
                AccountId = e.AccountId,
                AccountName = e.Account?.Name,
                AccountDimId = e.Account?.AccountDimId,
                AccountDimName = e.Account?.AccountDim?.Name,
                AttestRoleSort = e.AttestRole?.Sort ?? 0,
                ParentAttestRoleUserId = e.ParentAttestRoleUserId,
            };
        }

        public static List<UserAttestRoleDTO> ToDTOs(this IEnumerable<AttestRoleUser> l)
        {
            List<UserAttestRoleDTO> dtos = new List<UserAttestRoleDTO>();
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

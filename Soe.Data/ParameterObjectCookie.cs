using SoftOne.Soe.Common.DTO;
using System;

namespace SoftOne.Soe.Data
{
    public class ParameterObjectCookie
    {
        /// <summary> Company id </summary>
        public int Cid { get; set; }
        /// <summary> User id </summary>
        public int Uid { get; set; }
        /// <summary> User Email </summary>
        public string Ue { get; set; }
        /// <summary> User HasUserVerifiedEmail </summary>
        public bool Uhve { get; set; }
        /// <summary> User language id </summary>
        public int? Ulid { get; set; }
        /// <summary> RoleId</summary>
        public int Rid { get; set; }
        /// <summary> SupportUser id </summary>
        public int? SUid { get; set; }
        /// <summary> SupportCompany id </summary>
        public int? SCid { get; set; }
        /// <summary> Support LoggedIn by Company </summary>
        public bool SLC { get; set; }
        /// <summary> Support LoggedIn by User </summary>
        public bool SLU { get; set; }
        /// <summary> Super Admin Mode </summary>
        public bool SUM { get; set; }
        /// <summary> Include Inactive Accounts </summary>
        public bool IIA { get; set; }

        public static ParameterObjectCookie ToCookie(ParameterObject parameterObject)
        {
            return new ParameterObjectCookie
            {
                Cid = parameterObject?.ActorCompanyId ?? 0,
                Uid = parameterObject?.UserId ?? 0,
                Ue = parameterObject?.UserEmail,
                Uhve = parameterObject?.UserHasVerifiedEmail ?? false,
                Rid = parameterObject?.ActiveRoleId ?? 0,
                SUid = parameterObject?.SupportUserId,
                SCid = parameterObject?.SupportActorCompanyId,
                SLC = parameterObject?.IsSupportLoggedInByCompany ?? false,
                SLU = parameterObject?.IsSupportLoggedInByUser ?? false,
                SUM = parameterObject?.IsSuperAdminMode ?? false,
                IIA = parameterObject?.IncludeInactiveAccounts ?? false,
            };
        }
        public ParameterObject ToParameterObject()
        {
            var supportCompany = this.SCid.HasValue ? CompanyDTO.Create(this.SCid.Value, 0, string.Empty) : null;
            return new ParameterObject
            {
                ActorCompanyId = this.Cid,
                UserId = Uid,
                UserHasVerifiedEmail = Uhve,
                LicenseId = this.Lid,
                SupportActorCompanyId = supportCompany?.ActorCompanyId,
                SupportUserId = this.SUid,
                ActiveRoleId = this.Rid,
                IsSupportLoggedInByCompany = this.SLC,
                IsSupportLoggedInByUser = this.SLU,
                IsSuperAdminMode = this.SUM,
                IncludeInactiveAccounts = this.IIA,
            };
        }
    }
}

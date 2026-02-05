using System;

namespace SoftOne.Soe.Common.Security
{
    public static class SoeClaimType
    {
        public const string LegacyLogIn = "urn:soe:legacy_login";
        public const string LicenseId = "urn:soe:license_id";
        public const string LicenseGuid = "urn:soe:license_guid";
        public const string ChoosenLicenseId = "urn:soe:choosen_license_id";
        public const string ActorCompanyId = "urn:soe:actor_company_id";
        public const string RoleId = "urn:soe:role_id";
        public const string UserId = "urn:soe:user_id";
        public const string UserGuid = "urn:soe:user_guid";
        public const string UserName = "urn:soe:user_name";
        public const string SupportUserId = "urn:soe:support_user_id";
        public const string SupportActorCompanyId = "urn:soe:support_actor_company_id";
        public const string SupportActiveRoleId = "urn:soe:support_active_role_id";
        public const string IsSuperAdminMode = "urn:soe:super_admin";
        public const string IsSupportLoggedInByCompany = "urn:soe:support_by_company";
        public const string IsSupportLoggedInByUser = "urn:soe:support_by_user";
        public const string IncludeInactiveAccounts = "urn:soe:iia";
        public const string Evo = "urn:soe:evo";

    }
}

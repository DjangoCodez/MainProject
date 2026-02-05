using System;
using System.Security.Claims;
using System.Security.Principal;

namespace Soe.WebServices.External
{
    public static class UserExtended
    {
        public static Guid GetUserGuid(this IPrincipal user)
        {
            var claim = ((ClaimsIdentity)user.Identity).FindFirst("urn:soe:user_guid");
            if (claim == null)
                return Guid.Empty;
            Guid id = Guid.Empty;
            if (Guid.TryParse(claim.Value, out id))
                return id;
            return Guid.Empty;
        }
        public static int GetUserId(this IPrincipal user)
        {
            var claim = ((ClaimsIdentity)user.Identity).FindFirst("urn:soe:user_id");
            if (claim == null)
                return 0;
            int userId = 0;
            if (claim != null)
                int.TryParse(claim.Value, out userId);
            return userId;
        }
        public static int GetCompanyId(this IPrincipal user)
        {
            var claim = ((ClaimsIdentity)user.Identity).FindFirst("urn:soe:company_id");
            if (claim == null)
                return 0;
            int companyId = 0;
            if (claim != null)
                int.TryParse(claim.Value, out companyId);
            return companyId;
        }

        public static int GetRoleId(this IPrincipal user)
        {
            var claim = ((ClaimsIdentity)user.Identity).FindFirst("urn:soe:role_id");
            if (claim == null)
                return 0;
            int roleId = 0;
            if (claim != null)
                int.TryParse(claim.Value, out roleId);
            return roleId;
        }
    }
}
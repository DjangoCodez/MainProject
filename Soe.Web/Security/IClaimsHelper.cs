using SoftOne.Soe.Common.Security;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Web;

namespace SoftOne.Soe.Web.Security
{
    public interface IClaimsHelper
    {
        ClaimsIdentity GetIdentity(bool legacyLogin, int licenseId, Guid? licenseGuid, int actorCompanyId, int roleId, int userId, Guid? userGuid, string userName, int? supportActorCompanyId, int? supportUserId, bool isSuperAdminMode = false, bool isSupportedLoggedInByCompany = false, bool isSupportLoggedInByUser = false);
        void UpdateClaim(HttpContext context, string claimType, string value);
        void UpdateClaims(HttpContext context, Dictionary<string, string> claimsDict);
        int? GetIntClaim(HttpContext context, string type);
        bool? GetBoolClaim(HttpContext context, string type);
        Guid? GetGuidClaim(HttpContext context, string type);

    }

    public class DefaultClaimsHelper : IClaimsHelper
    {
        private readonly string authenticationType;

        public DefaultClaimsHelper(string authenticationType)
        {
            this.authenticationType = authenticationType;
        }

        public ClaimsIdentity GetIdentity(bool legacyLogin, int licenseId, Guid? licenseGuid, int actorCompanyId, int roleId, int userId, Guid? userGuid, string userName, int? supportActorCompanyId, int? supportUserId, bool isSuperAdminMode = false, bool isSupportedLoggedInByCompany = false, bool isSupportLoggedInByUser = false)
        {
            var claims = new List<Claim>
            {
                new Claim(SoeClaimType.LegacyLogIn, legacyLogin ? Boolean.FalseString : Boolean.TrueString),
                new Claim(SoeClaimType.LicenseGuid, licenseGuid?.ToString() ?? string.Empty),
                new Claim(SoeClaimType.LicenseId, licenseId.ToString()),
                new Claim(SoeClaimType.ActorCompanyId, actorCompanyId.ToString()),
                new Claim(SoeClaimType.UserId, userId.ToString()),
                new Claim(SoeClaimType.RoleId, roleId.ToString()),
                new Claim(SoeClaimType.UserGuid, userGuid?.ToString() ?? string.Empty),
                new Claim(SoeClaimType.IsSuperAdminMode, isSuperAdminMode.ToString()),
                new Claim(SoeClaimType.IsSupportLoggedInByCompany, isSupportedLoggedInByCompany.ToString()),
                new Claim(SoeClaimType.IsSupportLoggedInByUser, isSupportLoggedInByUser.ToString()),
                new Claim(SoeClaimType.SupportActorCompanyId, (supportActorCompanyId ?? 0).ToString()),
                new Claim(SoeClaimType.SupportUserId, (supportUserId ?? 0).ToString())
            };

            if (!string.IsNullOrEmpty(userName))
                claims.Add(new Claim(SoeClaimType.UserName, userName));

            return new ClaimsIdentity(claims, authenticationType);
        }

        public int? GetIntClaim(HttpContext context, string type)
        {
            if (context?.User?.Identity == null || context.User.Identity is not ClaimsIdentity identity)
                return null;

            var claim = identity.FindFirst(type);
            if (claim != null && Int32.TryParse(claim.Value, out int ret))
                return ret.ToNullable();

            return null;
        }

        public bool? GetBoolClaim(HttpContext context, string type)
        {
            if (context?.User?.Identity == null || context.User.Identity is not ClaimsIdentity identity)
                return null;

            var claim = identity.FindFirst(type);
            if (claim != null && Boolean.TryParse(claim.Value, out bool ret))
                return ret;

            return null;
        }

        public Guid? GetGuidClaim(HttpContext context, string type)
        {
            if (context?.User?.Identity == null || context.User.Identity is not ClaimsIdentity identity)
                return null;

            var claim = identity.FindFirst(type);
            if (claim != null && Guid.TryParse(claim.Value, out Guid ret))
            {
                if (ret == Guid.Empty)
                    return null;
                return ret;
            }
            return null;
        }

        public void UpdateClaim(HttpContext context, string claimType, string value)
        {
            var claimsDict = new Dictionary<string, string>
            {
                { claimType, value }
            };
            UpdateClaims(context, claimsDict);
        }

        public void UpdateClaims(HttpContext context, Dictionary<string, string> claimsDict)
        {
            var identity = (ClaimsIdentity)context.User.Identity;
            var newClaims = new List<Claim>();

            foreach (Claim claim in identity.Claims)
            {
                if (claimsDict.ContainsKey(claim.Type))
                    newClaims.Add(new Claim(claim.Type, claimsDict[claim.Type]));
                else
                    newClaims.Add(claim);
            }

            foreach (Claim claim in identity.Claims)
            {
                identity.RemoveClaim(claim);
            }

            identity.AddClaims(newClaims);
            context.GetOwinContext().Authentication.SignIn(new ClaimsIdentity(newClaims, authenticationType));
        }
    }
}

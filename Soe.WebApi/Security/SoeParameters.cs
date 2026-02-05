namespace Soe.WebApi.Security
{
    public class SoeParameters
    {
        public int LicenseId { get; set; }
        public string LicenseNr { get; set; }
        public int ActorCompanyId { get; set; }
        public int RoleId { get; set; }
        public int UserId { get; set; }
        public string LoginName { get; set; }
        public int SysCountryId { get; set; }
        public bool IsSupportAdmin { get; set; }
        public bool IsSupportSuperAdmin { get; set; }
        public int SupportUserId { get; set; }
        public int SupportCompanyId { get; set; }
    }
}
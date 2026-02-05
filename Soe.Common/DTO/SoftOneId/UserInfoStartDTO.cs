using System;

namespace SoftOne.Soe.Common.DTO.SoftOneId
{
    public class UserInfoStartDTO
    {
        public string UserName { get; set; }
        public string LicenseNr { get; set; }
        public Guid IdLoginGuid { get; set; }
        public byte[] Password { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public int sysServerId { get; set; }
        public string Url { get; set; }
        public int SysCompDbId { get; set; }
        public int LicenseId { get; set; }
        public string LicenseName { get; set; }
        public int SysServerId { get; set; }
    }
}

using System;

namespace SoftOne.Soe.Common.DTO.SoftOneId
{
    public class IdLoginDTO
    {
        public int IdLoginId { get; set; }
        public DateTime Created { get; set; }
        public Guid LoginGuid { get; set; }
        public DateTime Modified { get; set; }
        public int State { get; set; }
        public string Url { get; set; }
        public string UserName { get; set; }
        public string NewPassword { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string InitialToken { get; set; }
    }

    public class LicenseLoginInfo
    {
        public Guid LoginGuid { get; set; }
        public int LicenseId { get; set; }
        public int DbId { get; set; }
        public int State { get; set; }
        public int LicenseState { get; set; }
    }
}

using System;

namespace SoftOne.Soe.Common.DTO.SoftOneId
{
    public class ParameterClaimsObjectDTO
    {
        public int RoleId { get; set; }
        public int UserId { get; set; }
        public Guid IdUserGuid { get; set; }
        public string UserName { get; set; }
        public int ActorCompanyId { get; set; }
        public int? SoeSupportUserId { get; set; }
        public bool IsSupportLoggedInByCompany { get; set; }
        public bool IsSupportLoggedInByUser { get; set; }
        public bool IsSuperAdminMode { get; set; }
    }
}

namespace SoftOne.Soe.Common.DTO.SignatoryContract
{
    using SoftOne.Soe.Common.Attributes;
    using SoftOne.Soe.Common.Util;


    [TSInclude]

    public class GetPermissionResultDTO
    {
        public TermGroup_SignatoryContractPermissionType PermissionType { get; set; }
        public string PermissionLabel { get; set; }
        public bool HasPermission { get; set; }
        public bool IsAuthorized { get; set; } = false;
        public bool? IsAuthenticated { get; set; } = null;
        public bool? IsAuthenticationRequired { get; set; } = null;
        public AuthenticationDetailsDTO AuthenticationDetails { get; set; } = null;
    }
}

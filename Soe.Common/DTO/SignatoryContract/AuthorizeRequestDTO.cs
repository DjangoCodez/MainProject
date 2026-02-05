namespace SoftOne.Soe.Common.DTO.SignatoryContract
{
    using SoftOne.Soe.Common.Attributes;
    using SoftOne.Soe.Common.Util;

    [TSInclude]

    public class AuthorizeRequestDTO
    {
        public TermGroup_SignatoryContractPermissionType PermissionType { get; set; }

        public int? SignatoryContractId { get; set; }
    }
}

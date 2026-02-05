namespace SoftOne.Soe.Common.DTO.SignatoryContract
{
    using SoftOne.Soe.Common.Attributes;

    [TSInclude]
    public class SignatoryContractRevokeDTO
    {
        public int SignatoryContractId { get; set; }

        public string RevokedReason { get; set; }

    }
}

namespace SoftOne.Soe.Common.DTO.SignatoryContract
{
    using SoftOne.Soe.Common.Attributes;


    [TSInclude]

    public class AuthenticationResponseDTO
    {
        public int SignatoryContractAuthenticationRequestId { get; set; }
        public string Username { get; set; } = null;
        public string Password { get; set; } = null;
        public string Code { get; set; } = null;
    }
}

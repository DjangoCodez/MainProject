namespace SoftOne.Soe.Common.DTO.SoftOneId
{
    public class ValidateUserResultDTO
    {
        public bool IsValid { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Language { get; set; }
        public string LogotypeUrl { get; set; }
        public string Url { get; set; }
        public string ErrorMessage { get; set; }

    }

    public class ValidateUserRequestDTO
    {
        public int UserId { get; set; }
        public string SessionToken { get; set; }
        public bool GetExtendedInformation { get; set; }
    }

}

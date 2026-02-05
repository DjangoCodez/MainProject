namespace SoftOne.Soe.Common.DTO.SignatoryContract
{
    using SoftOne.Soe.Common.Attributes;
    using System;

    [TSInclude]
    public class SignatoryContractRecipientDTO
    {
        public int SignatoryContractRecipientId { get; set; }

        public int SignatoryContractId { get; set; }

        public int RecipientUserId { get; set; }

        public string RecipientUserName { get; set; }

        public Guid RecipientIdLoginGuid { get; set; }

    }
}

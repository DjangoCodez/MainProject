namespace SoftOne.Soe.Common.DTO.SignatoryContract
{
    using SoftOne.Soe.Common.Attributes;
    using SoftOne.Soe.Common.Util;
    using System;
    using System.Collections.Generic;

    [TSInclude]

    public class AuthenticationDetailsDTO
    {
        public int AuthenticationRequestId { get; set; }
        public SignatoryContractAuthenticationMethodType AuthenticationMethodType { get; set; }
        public string Message { get; set; }
        public DateTime ValidUntilUTC { get; set; }
    }
}

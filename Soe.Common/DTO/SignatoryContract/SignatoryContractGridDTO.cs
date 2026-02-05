namespace SoftOne.Soe.Common.DTO.SignatoryContract
{
    using SoftOne.Soe.Common.Attributes;
    using System;
    using System.Collections.Generic;

    [TSInclude]
    public class SignatoryContractGridDTO
    {
        public int SignatoryContractId { get; set; }

        public int ActorCompanyId { get; set; }

        public int? ParentSignatoryContractId { get; set; }

        public int SignedByUserId { get; set; }

        public int CreationMethodType { get; set; }

        public int RequiredAuthenticationMethodType { get; set; }

        public bool CanPropagate { get; set; }

        public DateTime Created { get; set; }

        public DateTime? RevokedAtUTC { get; set; }

        public DateTime? RevokedAt
        {
            get
            {
                return RevokedAtUTC.HasValue ? 
                    RevokedAtUTC.Value.ToLocalTime() 
                    : (DateTime?)null;
            }
        }

        public string RevokedBy { get; set; }

        public string RevokedReason { get; set; }

        public List<int> PermissionTypes { get; set; } = new List<int>();

        public List<string> PermissionNames { get; set; } = new List<string>();

        public string Permissions
        {
            get
            {
                return string.Join(", ", PermissionNames);
            }
        }

        public string AuthenticationMethod { get; set; }

        public int RecipientUserId { get; set; }

        public string RecipientUserName { get; set; }

    }
}

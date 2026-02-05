using SoftOne.Soe.Common.Attributes;
using System;
using System.Collections.Generic;

namespace SoftOne.Soe.Common.DTO.ClientManagement
{
    [TSInclude]
    public class ServiceUserDTO
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public List<int> AttestRoleIds { get; set; }
        public ServiceProviderDTO ServiceProvider { get; set; }
        public string ConnectionCode { get; set; }
        public DateTime Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }

    }

    [TSInclude]
    public class ServiceProviderDTO
    {
        public string LicenseNumber { get; set; }
        public string LicenseName { get; set; }
        public string CompanyName { get; set; }
    }

    [TSInclude]
    public class CompanyConnectionRequestDTO
    {
        public string MCName { get; set; }
        public string MCLicenseNr { get; set; }
        public string MCLicenseName { get; set; }
        public string CreatedBy { get; set; }
    }
}

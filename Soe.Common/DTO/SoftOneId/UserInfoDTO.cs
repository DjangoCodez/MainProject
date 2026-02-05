using System;
using System.Collections.Generic;

namespace SoftOne.Soe.Common.DTO.SoftOneId
{
    public class UserInfoDTO
    {
        public int SysCompDbId { get; set; }
        public Guid IdLoginGuid { get; set; }
        public int UserId { get; set; }
        public string LicenseNr { get; set; }
        public int LicenseId { get; set; }
        public string LicenseName { get; set; }
        public Guid LicenseGuid { get; set; }
        public int SysServerId { get; set; }
        public string Url { get; set; }
        public string Email { get; set; }
        public string MobilePhone { get; set; }
        public int SysLanguageId { get; set; }
        public string UsernameInGo { get; set; }
        public string ExternalAuthId { get; set; }
        public Guid IdProviderGuid { get; set; }
        public List<MissingMandatoryInformation> MissingMandatoryInformation { get; set; }
    }

    public class MissingMandatoryInformation
    {
        public string Type { get; set; }
        public string Group { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public bool CheckBox { get; set; }
        public string CheckBoxName { get; set; }
        public bool Mandatory { get; set; }
        public int ActorCompanyId { get; set; }
    }
}

using System;

namespace SoftOne.Soe.Business.Core.Reporting.Models.Status.Models
{
    public class LicenseInformationItem
    {
        public string Database { get; set; }
        public string LicenseNr { get; set; }
        public string LicenseName { get; set; }
        public DateTime? LastLogin { get; internal set; }
    }
}

using SoftOne.Soe.Common.Util;

namespace SoftOne.Soe.Business.Util
{
    public class FeatureTrackItem
    {
        public SoeFeatureType Type { get; set; }
        public int LicenseId { get; set; }
        public string LicenseNr { get; set; }
        public int ActorCompanyId { get; set; }
        public int? CompanyNr { get; set; }
        public int RoleId { get; set; }
        public string Name { get; set; }
        public int FeaturesAdded { get; set; }
        public int FeaturesPromoted { get; set; }
        public int FeaturesDegraded { get; set; }
        public int FeaturesDeleted { get; set; }
        public int FeaturesIgnored { get; set; }
    }
}

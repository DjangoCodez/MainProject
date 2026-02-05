using SoftOne.Soe.Common.Attributes;

namespace SoftOne.Soe.Common.DTO
{
    [TSInclude]
    public class UserAgentClientInfoDTO
    {
        public string Data { get; set; }

        public string OsFamily { get; set; }
        public string OsVersion { get; set; }

        public string DeviceBrand { get; set; }
        public string DeviceFamily { get; set; }
        public string DeviceModel { get; set; }

        public string UserAgentFamily { get; set; }
        public string UserAgentVersion { get; set; }
    }
}

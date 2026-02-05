using SoftOne.Soe.Common.DTO;
using UAParser;

namespace SoftOne.Soe.Business.Util
{
    public static class UserAgentUtility
    {
        public static ClientInfo Parse(string data)
        {
            Parser uaParser = UAParser.Parser.GetDefault();
            ClientInfo ci = uaParser.Parse(data);
            return ci;
        }

        public static UserAgentClientInfoDTO ToDTO(this ClientInfo ci)
        {
            UserAgentClientInfoDTO dto = new UserAgentClientInfoDTO();

            dto.Data = ci.String;
            dto.OsFamily = ci.OS.Family;
            dto.OsVersion = ci.OS.Major;
            if (!string.IsNullOrEmpty(ci.OS.Minor))
                dto.OsVersion += "." + ci.OS.Minor;
            if (!string.IsNullOrEmpty(ci.OS.Patch))
                dto.OsVersion += "." + ci.OS.Patch;
            if (!string.IsNullOrEmpty(ci.OS.PatchMinor))
                dto.OsVersion += "." + ci.OS.PatchMinor;
            dto.DeviceBrand = ci.Device.Brand;
            dto.DeviceFamily = ci.Device.Family;
            dto.DeviceModel = ci.Device.Model;
            dto.UserAgentFamily = ci.UA.Family;
            dto.UserAgentVersion = ci.UA.Major;
            if (!string.IsNullOrEmpty(ci.UA.Minor))
                dto.UserAgentVersion += "." + ci.UA.Minor;
            if (!string.IsNullOrEmpty(ci.UA.Patch))
                dto.UserAgentVersion += "." + ci.UA.Patch;

            return dto;
        }

        public static UserAgentClientInfoDTO ParseToDTO(string data)
        {
            if (string.IsNullOrEmpty(data))
                return null;

            ClientInfo ci = Parse(data);
            return ci.ToDTO();
        }
    }
}

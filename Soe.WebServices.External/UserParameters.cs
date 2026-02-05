using SoftOne.Soe.Common.Util;

namespace Soe.WebServices.External
{
    public class UserParameters
    {
        public UserParameters(int userId, int roleId, int companyId)
        {
            this.UserId = userId;
            this.RoleId = roleId;
            this.CompanyId = companyId;
        }

        public int UserId { get; set; }
        public int RoleId { get; set; }
        public int CompanyId { get; set; }

        public MobileDeviceType MobileDeviceType { get; set; }

        public void MobileDeviceTypeFromString(string deviceType)
        {
            MobileDeviceType = MobileDeviceType.Unknown;

            if (string.IsNullOrEmpty(deviceType))
                return;

            switch (deviceType)
            {
                case "Android":
                    MobileDeviceType = MobileDeviceType.Android;
                    break;
                case "iOS":
                    MobileDeviceType = MobileDeviceType.IOS;
                    break;
            }
        }

        new public string ToString()
        {
            return $"UserId {UserId} RoleId {RoleId} CompanyId {CompanyId}";
        }
    }
}
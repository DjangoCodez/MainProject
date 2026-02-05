namespace SoftOne.Soe.Business.Core.Mobile
{
    public class MobileManagerUtil
    {
        private string RemoveDecimal(string version)
        {
            return version.Contains(".") ? version.Split('.')[0] : version;

        }

        public bool IsCallerExpectedVersionOlderOrEqualToGivenVersion(string callerExpectedVersion, decimal compareVersion)
        {
            decimal.TryParse(RemoveDecimal(callerExpectedVersion), out decimal callerVersionDecimal);

            if (callerVersionDecimal <= compareVersion)
                return true;

            return false;

        }

        public bool IsCallerExpectedVersionNewerThenGivenVersion(string callerExpectedVersion, decimal compareVersion)
        {
            decimal.TryParse(RemoveDecimal(callerExpectedVersion), out decimal callerVersionDecimal);

            if (callerVersionDecimal > compareVersion)
                return true;

            return false;
        }
    }
}

using System;

namespace SoftOne.Soe.Util.Exceptions
{
    [Serializable]
    public class SoeCopyCompanyException : SoeException
    {
        public SoeCopyCompanyException(string message, int templateCompanyId, int newCompanyId, string source)
            : base(message + ". " + GetCompanyInfo(templateCompanyId, newCompanyId))
        {
            base.Source = source;
        }

        public SoeCopyCompanyException(string message, int templateCompanyId, int newCompanyId, Exception innerException, string source)
            : base(message + ". " + GetCompanyInfo(templateCompanyId, newCompanyId), innerException)
        {
            base.Source = source;
        }

        private static string GetCompanyInfo(int templateCompanyId, int newCompanyId)
        {
            return "Template Company [" + templateCompanyId + "]. " +
                   "New Company [" + newCompanyId + "]";
        }
    }
}

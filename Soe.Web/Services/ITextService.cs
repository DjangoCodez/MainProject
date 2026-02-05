using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using System.Threading;

namespace SoftOne.Soe.Web.Services
{
    public interface ITextService
    {
        string GetText(int sysTermId, int sysTermGroupId = (int)TermGroup.General, string defaultTerm = "", string cultureCode = null);
        string GetModuleName(int module);
        string GetSupportText();
    }

    public class TermCacheManagerTextService : ITextService
    {
        public TermCacheManagerTextService()
        {
            
        }
        public string GetText(int sysTermId, int sysTermGroupId = (int)TermGroup.General, string defaultTerm = "", string cultureCode = null)
        {
            cultureCode = cultureCode ?? Thread.CurrentThread.CurrentCulture.Name;
            return TermCacheManager.Instance.GetText(sysTermId, sysTermGroupId, defaultTerm, cultureCode);
        }
        public string GetModuleName(int module)
        {
            string moduleName = "";

            switch (module)
            {
                case (int)SoeModule.Manage:
                    moduleName = GetText(7, defaultTerm: "Administrera");
                    break;
                case (int)SoeModule.Economy:
                    moduleName = GetText(6, defaultTerm: "Ekonomi");
                    break;
                case (int)SoeModule.Billing:
                    moduleName = GetText(1829, defaultTerm: "Försäljning");
                    break;
                case (int)SoeModule.Estatus:
                    moduleName = GetText(2245, defaultTerm: "Fastighet");
                    break;
                case (int)SoeModule.Time:
                    moduleName = GetText(5002, defaultTerm: "Personal");
                    break;                
            }

            return moduleName;
        }
        public string GetSupportText()
        {
            return "(" + GetText(5773, defaultTerm: "Support") + ")";
        }
    }
}

using SoftOne.Soe.Common.Util;

namespace SoftOne.Soe.Business.Core.ManagerFacades
{
    public interface ITextService
    {
        string GetText(int sysTermId);
        string GetText(int sysTermId, string defaultTerm, bool forceDefaultTerm = false);
        string GetText(int sysTermId, TermGroup termGroup, int langId = 0);
        string GetText(int sysTermId, int sysTermGroupId, int langId, string defaultTerm);
        string GetText(int sysTermId, int sysTermGroupId, string defaultTerm = "");
    }
}

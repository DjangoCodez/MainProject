using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;

namespace SoftOne.Soe.Business.Core.ManagerFacades
{
    internal class TextService : ManagerBase, ITextService
    {
        public TextService(ParameterObject managerContext) : base(managerContext) { }

        public new string GetText(int sysTermId) => base.GetText(sysTermId);
        public new string GetText(int sysTermId, string defaultTerm, bool forceDefaultTerm = false) => base.GetText(sysTermId, defaultTerm, forceDefaultTerm);
        public new string GetText(int sysTermId, TermGroup termGroup, int langId = 0) => base.GetText(sysTermId, termGroup, langId);
        public new string GetText(int sysTermId, int sysTermGroupId, int langId, string defaultTerm) => base.GetText(sysTermId, sysTermGroupId, langId, defaultTerm);
        public new string GetText(int sysTermId, int sysTermGroupId, string defaultTerm = "") => base.GetText(sysTermId, sysTermGroupId, defaultTerm);
    }
}

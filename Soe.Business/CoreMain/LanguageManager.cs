using SoftOne.Soe.Business.DataCache;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Data;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;

namespace SoftOne.Soe.Business.Core
{
    public class LanguageManager : ManagerBase
    {
        #region Ctor

        public LanguageManager(ParameterObject parameterObject) : base(parameterObject) { }

        #endregion

        #region SysLanguage

        public List<SysLanguageDTO> GetSysLanguages()
        {
            using (var entities = new SOESysEntities())
            {
                using (var transaction = new TransactionScope(TransactionScopeOption.Suppress))
                {
                    return entities.SysLanguage.AsNoTracking().ToList().ToDTOs();
                }
            }
        }

        public Dictionary<int, string> GetSysLanguageDict(bool addEmptyRow)
        {
            Dictionary<int, string> languageDict = new Dictionary<int, string>();

            if (addEmptyRow)
                languageDict.Add(0, " ");

            foreach (var language in SysDbCache.Instance.SysLanguages)
            {
                if (!languageDict.ContainsKey(language.SysLanguageId))
                    languageDict.Add(language.SysLanguageId, language.Name);
            }

            return languageDict;
        }

        public SysLanguageDTO GetSysLanguage(string langCode)
        {
            return SysDbCache.Instance.SysLanguages.FirstOrDefault(l => l.LangCode == langCode);
        }

        public SysLanguageDTO GetSysLanguage(int langId)
        {
            return SysDbCache.Instance.SysLanguages.FirstOrDefault(l => l.SysLanguageId == langId);
        }

        public string GetSysLanguageCode(int langId)
        {
            return GetSysLanguage(langId)?.LangCode ?? string.Empty;
        }

        public int GetSysLanguageId(string langCode)
        {
            return GetSysLanguage(langCode)?.SysLanguageId ?? 0;
        }

        public bool IsValidSysLanguage(string langCode)
        {
            return GetSysLanguage(langCode) != null;
        }

        #endregion
    }
}

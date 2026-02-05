using SoftOne.Soe.Common.Util;
using System;
using System.Collections;

namespace SoftOne.Soe.Business.Util
{
    /// <summary>
    /// Help class for storing terms, each object will store terms in different languages corresponding to a termId
    /// </summary>
    [Serializable]
    public class TermObject
    {
        private readonly Hashtable termTable;

        private readonly int termId;
        public int TermId
        {
            get
            {
                return this.termId;
            }
        }

        private readonly int termGroupId;
        public int TermGroupId
        {
            get
            {
                return this.termGroupId;
            }
        }

        public TermObject(int termId, int termGroupId)
        {
            this.termTable = new Hashtable();
            this.termId = termId;
            this.termGroupId = termGroupId;
        }

        public void SetTerm(string term, int langId, bool replaceNewLineForLangIdOne = false)
        {
            if (langId != 1 || replaceNewLineForLangIdOne)
                term = StringUtility.ReplaceValue(term, "\\n", Environment.NewLine);

            if (this.termTable.ContainsKey(Convert.ToString(langId)))
                this.termTable[Convert.ToString(langId)] = term;
            else
                this.termTable.Add(Convert.ToString(langId), term);
        }

        public string GetTerm(int langId)
        {
            return GetTerm(langId.ToString());
        }

        public string GetTerm(string langId)
        {
            return (string)this.termTable[langId];
        }
    }

    /// <summary>
    /// Help class for storing terms by translationKey, each object will store terms in different languages corresponding to a termId
    /// </summary>
    [Serializable]
    public class TermByKeyObject
    {
        private readonly Hashtable termTable;

        private readonly string translationKey;
        public string TranslationKey
        {
            get
            {
                return this.translationKey;
            }
        }

        public TermByKeyObject(string translationKey)
        {
            this.termTable = new Hashtable();
            this.translationKey = translationKey;
        }

        public void SetTerm(string term, int langId)
        {
            if (this.termTable.ContainsKey(Convert.ToString(langId)))
                this.termTable[Convert.ToString(langId)] = term;
            else
                this.termTable.Add(Convert.ToString(langId), term);
        }

        public string GetTerm(int langId)
        {
            string name = (string)this.termTable[Convert.ToString(langId)];
            if (langId != 1)
                name = StringUtility.ReplaceValue(name, "\\n", Environment.NewLine);
            return name;
        }
    }
}

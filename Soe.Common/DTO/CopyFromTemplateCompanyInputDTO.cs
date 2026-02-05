using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Common.DTO
{
    public class CopyFromTemplateCompanyInputDTO
    {
        #region Variables

        public bool Update { get; set; }
        public int TemplateCompanyId { get; set; }
        public string TemplateCompanyName { get; set; } = null;
        public int ActorCompanyId { get; set; }
        public int UserId { get; set; }
        public bool LiberCopy { get; set; }
        public Dictionary<TemplateCompanyCopy, bool> CopyDict { get; }
        public int SysCompDbId { get; set; }

        #endregion

        #region Ctor

        public CopyFromTemplateCompanyInputDTO()
        {
            this.CopyDict = new Dictionary<TemplateCompanyCopy, bool>();
        }

        #endregion

        #region Public methods

        public void SetCopy(TemplateCompanyCopy copy, bool value = true)
        {
            if (!this.CopyDict.ContainsKey(copy))
                this.CopyDict.Add(copy, value);
            else
                this.CopyDict[copy] = value;
        }
        public bool DoCopy(TemplateCompanyCopy copy)
        {
            return this.doCopy(copy) || this.DoCopyAll();
        }
        public bool DoCopyAll()
        {
            return this.doCopy(TemplateCompanyCopy.All);
        }
        public bool DoCopyAny()
        {
            return this.CopyDict.Any(i => i.Value);
        }

        #endregion

        #region Help-methods

        public bool doCopy(TemplateCompanyCopy copy)
        {
            return this.CopyDict.ContainsKey(copy) && this.CopyDict[copy];
        }

        public string DoCopyKeys()
        {
            return string.Join(", ", this.CopyDict.Where(i => i.Value).Select(i => i.Key.ToString()));
        }

        #endregion
    }
}

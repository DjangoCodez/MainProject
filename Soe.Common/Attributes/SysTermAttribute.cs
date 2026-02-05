using SoftOne.Soe.Common.Util;
using System;

namespace SoftOne.Soe.Common.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class SysTermAttribute : Attribute
    {
        public SysTermAttribute(int sysTermId, string defaultText)
        {
            this.SysTermId = sysTermId;
            this.DefaultTerm = defaultText;
        }

        public int SysTermId { get; set; }
        public string DefaultTerm { get; set; }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class SysTermGroupAttribute : Attribute
    {
        public SysTermGroupAttribute(TermGroup group)
        {
            this.TermGroup = group;
        }

        public TermGroup TermGroup { get; set; }
    }
}

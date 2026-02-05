using System;

namespace SoftOne.Soe.Common.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class SoeGridAttribute : Attribute
    {
        public bool IsReadOnly { get; private set; }
        public bool Hidden { get; private set; }

        public SoeGridAttribute(bool hidden = false, bool isReadOnly = false)
        {
            this.IsReadOnly = IsReadOnly;
            this.Hidden = hidden;
        }
    }
}

using System;

namespace SoftOne.Soe.Common.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class ExcelExportAttribute : Attribute
    {
        public string TitleText { get; private set; }

        public ExcelExportAttribute(string titleText = null)
        {
            this.TitleText = titleText;
        }
    }
}

using System;

namespace SoftOne.Soe.Data.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple=true)]
    public class AutoToDTOAttribute : Attribute
    {
        public Type ToType { get; set; }
        public Type FromType { get; set; }
        public bool AutoConvertNullable { get; private set; }
        public bool AutoConvertEnums { get; private set; }
        public bool IsPublic { get; private set; }

        public string ToTypename
        {
            get
            {
                return ToType.Name;
            }
        }

        public AutoToDTOAttribute(Type toType, Type fromType = null, bool autoConvertNullable = true, bool autoConvertEnums = true, bool isPublic = true)
        {
            this.ToType = toType;
            this.FromType = fromType;
            this.AutoConvertEnums = autoConvertEnums;
            this.AutoConvertNullable = autoConvertNullable;
            this.IsPublic = isPublic;
        }
    }
}

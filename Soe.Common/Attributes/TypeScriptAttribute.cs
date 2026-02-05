using System;

namespace SoftOne.Soe.Common.Attributes
{
    //Use to include class in TypeWriter's migration to TypeScript.
    public class TSIncludeAttribute : Attribute
    {
        public TSIncludeAttribute()
        {
        }
    }
    
    public class TSIgnoreAttribute : Attribute
    {
        public TSIgnoreAttribute()
        {
        }
    }
}

using System;

namespace SoftOne.Soe.Util.Exceptions
{
    [Serializable]
    public class SoeEntityNotFoundException : SoeException
    {
        public SoeEntityNotFoundException(string entityName, string source)
            : base(entityName + " not found")
        {
            base.Source = source;
        }

        public SoeEntityNotFoundException(string entityName, Exception innerException, string source)
            : base(entityName + " not found", innerException)
        {
            base.Source = source;
        }
    }
}

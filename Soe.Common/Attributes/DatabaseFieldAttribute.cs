using System;

namespace SoftOne.Soe.Common.Attributes
{
    public class DatabaseFieldAttribute : Attribute
    {
        public string DbPath { get; set; }
        public DatabaseFieldAttribute(string dbPath)
        {
            this.DbPath = dbPath;
        }
    }
}

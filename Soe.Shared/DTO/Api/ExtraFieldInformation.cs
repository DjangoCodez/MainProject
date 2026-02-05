using System;
using System.Collections.Generic;
using System.Text;

namespace SoftOne.Soe.Shared.DTO.Api
{
    public class ExtraFieldInformation
    {
        public string Name { get; set; }
        public string Key { get; set; }
        public ExtraFieldEntityType ExtraFieldEntityType { get; set; }
    }

    public enum ExtraFieldEntityType
    {
        Unknown = 0,
        Account = 1,
        Employee = 2,
        Supplier = 3,
        Customer = 4
    };
}

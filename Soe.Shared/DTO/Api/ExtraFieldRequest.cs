using System;
using System.Collections.Generic;
using System.Text;

namespace SoftOne.Soe.Shared.DTO.Api
{
    public class ExtraFieldResponse
    {
        public ExtraFieldResponse() { }

        public ExtraFieldEntityType ExtraFieldEntityType { get; set; }
        /// <summary>
        /// Keys from ExtraFieldInformation
        /// </summary>
        public List<string> Keys { get; set; }
        public string Key { get; set; }
        /// <summary>
        /// Employee => employee numbers
        /// Customer => customer numbers
        /// Suppliers => supplier numbers
        /// Account = account numbers
        /// </summary>
        public List<string> Codes { get; set; }
    }
}

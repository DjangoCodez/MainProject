using SoftOne.Soe.Common.Attributes;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;

namespace SoftOne.Soe.Common.DTO
{
    [TSInclude]
    public class SysExtraFieldDTO
    {
        public SysExtraFieldDTO()
        {
            Translations = new List<CompTermDTO>();
        }
        public int? SysExtraFieldId { get; set; }
        public SoeEntityType Entity { get; set; }
        public SysExtraFieldType SysType { get; set; }
        public TermGroup_ExtraFieldType Type { get; set; }
        public List<CompTermDTO> Translations { get; set; }
        
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }

        // extended properties
        public string Name { get; set; }
    }
}

using SoftOne.Soe.Common.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace SoftOne.Soe.Common.DTO.Import
{
    [TSInclude]
    public class ExcelImportTemplateDTO
    {
        public int ExcelImportTemplateDTOID { get; set; }
        public string Href { get; set; }
        public string Description { get; set; }
    }

    [TSInclude]
    public class ExcelImportDTO
    {
        public string Filename { get; set; }
        public bool DoNotUpdateWithEmptyValues { get; set; }
        public byte[] Bytes { get; set; }
    }
}

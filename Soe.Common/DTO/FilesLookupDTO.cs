using SoftOne.Soe.Common.Attributes;
using SoftOne.Soe.Common.Util;
using System.Collections.Generic;

namespace SoftOne.Soe.Common.DTO
{
    [TSInclude]
    public class FilesLookupDTO
    {
        public SoeEntityType Entity { get; set; }
        public List<ImportFileDTO> Files { get; set; }
    }
}

using System.Collections.Generic;

namespace SoftOne.Soe.Common.DTO
{
    public class GenerateDeviationsDTO
    {
        public List<TimeBlockDTO> TimeBlocks { get; set; }
        public List<TimeTransactionItem> TimeTransactionItems { get; set; }
        public List<ApplyAbsenceDTO> ApplyAbsenceItems { get; set; }
    }
}

using SoftOne.Soe.Common.Util;

namespace SoftOne.Soe.Common.DTO
{
    public class StateAnalysisDTO
    {
        public SoeStatesAnalysis State { get; set; }
        public int NoOfItems { get; set; }
        public int NoOfActorsForItems { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TotalAmount2 { get; set; }
        public decimal TotalAmount3 { get; set; }
    }
}

using System.Collections.Generic;

namespace SoftOne.Soe.Common.DTO
{
    public class TimeDeviationsCauseForWTSDTO
    {
        public int TimeDeviationCauseId { get; set; }
        public string Name { get; set; }
        public Dictionary<int, string> Children { get; set; }
    }
}

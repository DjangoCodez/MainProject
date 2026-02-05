using System;

namespace SoftOne.Soe.Common.DTO
{
    public class MatchedItemDTO : IEquatable<MatchedItemDTO>
    {
        public int CompanyId { get; set; }
        public string CompanyName { get; set; }
        public int SourceItemId { get; set; }
        public string SourceItemName { get; set; }
        public int TargetItemId { get; set; }
        public string TargetItemName { get; set; }

        public override string ToString()
        {
            return "[" + CompanyId + ":" + CompanyName + ": " + SourceItemId + ":" + SourceItemName + " => " + TargetItemId + ":" + TargetItemName + "]";
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as MatchedItemDTO);
        }

        public override int GetHashCode()
        {
            return CompanyId + SourceItemId;
        }

        public bool Equals(MatchedItemDTO other)
        {
            return other != null && this.CompanyId == other.CompanyId && this.SourceItemId == other.SourceItemId;
        }
    }
}

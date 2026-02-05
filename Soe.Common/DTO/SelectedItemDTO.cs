
namespace SoftOne.Soe.Common.DTO
{
    public class SelectedItemDTO
    {
        public int CompanyId { get; set; }
        public int ItemId { get; set; }

        public override string ToString()
        {
            return "[" + this.CompanyId + ":" + this.ItemId + "]";
        }
    }
}

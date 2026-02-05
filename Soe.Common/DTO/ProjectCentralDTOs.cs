using SoftOne.Soe.Common.Attributes;
using SoftOne.Soe.Common.Util;
using System;

namespace SoftOne.Soe.Common.DTO
{
    [TSInclude]
    public class ProjectCentralStatusDTO
    {
        public int AssociatedId { get; set; }
        public int EmployeeId { get; set; }
        public string Description { get; set; }
        public string Name { get; set; }
        public string TypeName { get; set; }
        public string CostTypeName { get; set; }
        public string GroupRowTypeName { get; set; }
        public string EmployeeName { get; set; }
        public string Modified { get; set; }
        public string ModifiedBy { get; set; }
        public decimal Budget { get; set; }
        public decimal BudgetTime { get; set; }
        public decimal Value { get; set; }
        public decimal Value2 { get; set; }
        public decimal Diff { get; set; }
        public decimal Diff2 { get; set; }
        public bool HasInfo { get; set; }
        public string Info { get; set; }
        public string ActorName { get; set; }
        public bool IsEditable { get; set; }
        public bool IsVisible { get; set; }
        public DateTime? Date { get; set; }
        public ProjectCentralStatusRowType RowType { get; set; }
        public ProjectCentralHeaderGroupType GroupRowType { get; set; }
        public SoeOriginType OriginType { get; set; }
        public ProjectCentralBudgetRowType Type { get; set; }
    }
}

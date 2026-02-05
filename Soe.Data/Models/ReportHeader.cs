using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;

namespace SoftOne.Soe.Data
{
    public partial class ReportHeader : ICreatedModified, IState
    {
        public string TemplateType { get; set; }
        public string NameAndDescription
        {
            get
            {
                return !string.IsNullOrEmpty(this.Description) ? (this.Name + " (" + this.Description + ")") : this.Name;
            }
        }
    }


    public partial class ReportHeaderInterval : IReportHeaderInterval
    {

    }

    public static partial class EntityExtensions
    {
        #region ReportGroup

        //public static ReportGroupDTO ToDTO(this ReportGroup e)
        //{
        //    if (e == null)
        //        return null;

        //    ReportGroupDTO dto = new ReportGroupDTO()
        //    {
        //        ReportGroupId = e.ReportGroupId,
        //        ActorCompanyId = e.Company != null ? e.Company.ActorCompanyId : 0,  // TODO: Add foreign key to model
        //        TemplateTypeId = e.TemplateTypeId,
        //        Module = (SoeModule)e.Module,
        //        Name = e.Name,
        //        Description = e.Description,
        //        ShowLabel = e.ShowLabel,
        //        ShowSum = e.ShowSum,
        //        InvertRow = e.InvertRow,
        //        Created = e.Created,
        //        CreatedBy = e.CreatedBy,
        //        Modified = e.Modified,
        //        ModifiedBy = e.ModifiedBy,
        //        State = (SoeEntityState)e.State
        //    };

        //    // Extensions
        //    dto.TemplateType = e.TemplateType;

        //    return dto;
        //}

        //public static IEnumerable<ReportGroupDTO> ToDTOs(this IEnumerable<ReportGroup> l)
        //{
        //    var dtos = new List<ReportGroupDTO>();
        //    if (l != null)
        //    {
        //        foreach (var e in l)
        //        {
        //            dtos.Add(e.ToDTO());
        //        }
        //    }
        //    return dtos;
        //}

        #endregion
    }
}

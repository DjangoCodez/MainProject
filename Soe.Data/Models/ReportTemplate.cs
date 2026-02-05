using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using System.Collections.Generic;

namespace SoftOne.Soe.Data
{
    public partial class ReportTemplate : ICreatedModified, IState
    {

    }

    public static partial class EntityExtensions
    {
        #region ReportTemplate

        public static ReportTemplateDTO ToDTO(this ReportTemplate e)
        {
            if (e == null)
                return null;

            return new ReportTemplateDTO()
            {
                ReportTemplateId = e.ReportTemplateId,
                ActorCompanyId = e.Company?.ActorCompanyId ?? 0,  // Add foreign key to model
                SysReportTemplateTypeId = e.SysTemplateTypeId,
                SysReportTypeId = e.SysReportTypeId,
                Module = (SoeModule)e.Module,
                IsSystem = false,
                Name = e.Name,
                Description = e.Description,
                ReportNr = e.ReportNr,
                FileName = e.FileName,
                GroupByLevel1 = e.GroupByLevel1,
                GroupByLevel2 = e.GroupByLevel2,
                GroupByLevel3 = e.GroupByLevel3,
                GroupByLevel4 = e.GroupByLevel4,
                SortByLevel1 = e.SortByLevel1,
                SortByLevel2 = e.SortByLevel2,
                SortByLevel3 = e.SortByLevel3,
                SortByLevel4 = e.SortByLevel4,
                Special = e.Special,
                IsSortAscending = e.IsSortAscending,
                ShowGroupingAndSorting = e.ShowGroupingAndSorting,
                ShowOnlyTotals = e.ShowOnlyTotals,
                ValidExportTypes = e.GetValidExportTypes(),
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State
            };
        }

        public static IEnumerable<ReportTemplateDTO> ToDTOs(this IEnumerable<ReportTemplate> l)
        {
            var dtos = new List<ReportTemplateDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static ReportTemplateGridDTO ToGridDTO(this ReportTemplate e)
        {
            if (e == null)
                return null;

            return new ReportTemplateGridDTO()
            {
                ReportTemplateId = e.ReportTemplateId,
                Name = e.Name,
                Description = e.Description,
            };
        }

        public static IEnumerable<ReportTemplateGridDTO> ToGridDTOs(this IEnumerable<ReportTemplate> l)
        {
            var dtos = new List<ReportTemplateGridDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToGridDTO());
                }
            }
            return dtos;
        }

        public static List<int> GetValidExportTypes(this ReportTemplate e)
        {
            return ReportTemplateDTO.GetValidExportTypes(e?.ValidExportTypes, SoeReportType.CrystalReport);
        }

        public static bool IsValid(this ReportTemplate e, TermGroup_ReportExportType exportType)
        {
            return e.GetValidExportTypes().Contains((int)exportType);
        }

        #endregion
    }
}

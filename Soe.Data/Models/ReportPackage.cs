using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Data
{
    public partial class ReportPackage : ICreatedModified, IState
    {

    }

    public static partial class EntityExtensions
    {
        #region ReportPackage

        public static ReportPackageDTO ToDTO(this ReportPackage e)
        {
            if (e == null)
                return null;

            return new ReportPackageDTO()
            {
                ReportPackageId = e.ReportPackageId,
                ActorCompanyId = e.Company != null ? e.Company.ActorCompanyId : 0,  // Add foreign key to model
                Module = (SoeModule)e.Module,
                Name = e.Name,
                Description = e.Description,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State
            };
        }

        public static IEnumerable<ReportPackageDTO> ToDTOs(this IEnumerable<ReportPackage> l)
        {
            var dtos = new List<ReportPackageDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static ReportPackageGridDTO ToGridDTO(this ReportPackage e)
        {
            if (e == null)
                return null;

            return new ReportPackageGridDTO()
            {
                ReportPackageId = e.ReportPackageId,
                Name = e.Name,
                Description = e.Description,
            };
        }

        public static IEnumerable<ReportPackageGridDTO> ToGridDTOs(this IEnumerable<ReportPackage> l)
        {
            var dtos = new List<ReportPackageGridDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToGridDTO());
                }
            }
            return dtos;
        }

        public static List<Report> GetActiveReports(this ReportPackage e)
        {
            return e?.Report?.Where(r => r.State == (int)SoeEntityState.Active).ToList() ?? new List<Report>();
        }

        public static List<int> GetActiveReportIds(this ReportPackage e)
        {
            return e.GetActiveReports().Select(r => r.ReportId).ToList();
        }

        #endregion
    }
}

using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System.Collections.Generic;
using System.Linq;
using SoftOne.Soe.Common.Interfaces.Common;

namespace SoftOne.Soe.Data
{
    public partial class VacationYearEndHead : ICreatedModifiedNotNull, IState
    {
        public int? DataStorageId { get; set; }
        public string ContentTypeName { get; set; }
        public string Content { get; set; }
        public string EmployeesFailed { get; set; }
        public VacationYearEndResultDTO Result { get; set; }
    }

    public partial class VacationYearEndRow : ICreatedModified, IState
    {

    }

    public static partial class EntityExtensions
    {
        #region VacationYearEnd

        public static VacationYearEndHeadDTO ToDTO(this VacationYearEndHead e)
        {
            if (e == null)
                return null;

            return new VacationYearEndHeadDTO()
            {
                VacationYearEndHeadId = e.VacationYearEndHeadId,
                DataStorageId = e.DataStorageId,
                Date = e.Date,
                ContentType = (TermGroup_VacationYearEndHeadContentType)e.ContentType,
                ContentTypeName = e.ContentTypeName,
                Content = e.Content,
                EmployeesFailed = e.EmployeesFailed,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Rows = e.VacationYearEndRow.ToDTOs().ToList(),
            };
        }

        public static IEnumerable<VacationYearEndHeadDTO> ToDTOs(this IEnumerable<VacationYearEndHead> l)
        {
            var dtos = new List<VacationYearEndHeadDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static VacationYearEndRowDTO ToDTO(this VacationYearEndRow e)
        {
            if (e == null)
                return null;

            return new VacationYearEndRowDTO()
            {
                EmployeeId = e.EmployeeId,
                EmployeeVacationSEId = e.EmployeeVacationSEId,
            };
        }

        public static IEnumerable<VacationYearEndRowDTO> ToDTOs(this IEnumerable<VacationYearEndRow> l)
        {
            var dtos = new List<VacationYearEndRowDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static bool ContainsAnyEmployee(this VacationYearEndHead e, List<int> employeeIds)
        {
            if (e?.VacationYearEndRow == null || employeeIds.IsNullOrEmpty())
                return false;

            foreach (int employeeId in employeeIds)
            {
                if (e.VacationYearEndRow.Any(i => i.EmployeeId == employeeId && i.State == (int)SoeEntityState.Active && i.Status == (int)TermGroup_VacationYearEndStatus.Succeded))
                    return true;
            }
            return false;
        }

        #endregion

        #region VacationYearEndRow

        public static List<VacationYearEndRow> GetSucceededRows(this VacationYearEndHead e)
        {
            return e.GetRowsByStatus(TermGroup_VacationYearEndStatus.Succeded);
        }
        public static List<VacationYearEndRow> GetFailedRows(this VacationYearEndHead e)
        {
            return e.GetRowsByStatus(TermGroup_VacationYearEndStatus.Failed);
        }
        public static List<VacationYearEndRow> GetRowsByStatus(this VacationYearEndHead e, TermGroup_VacationYearEndStatus status)
        {
            return e?.VacationYearEndRow?.Where(r => r.Status == (int)status).ToList() ?? new List<VacationYearEndRow>();
        }

        #endregion
    }
}

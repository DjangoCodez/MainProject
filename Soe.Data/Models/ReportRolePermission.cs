using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Data
{
    public partial class ReportRolePermission : ICreatedModified, IState
    {

    }

    public static partial class EntityExtensions
    {
        #region ReportRolePermission

        public static ReportRolePermissionDTO ToDTO(this ReportRolePermission e)
        {
            if (e == null)
                return null;

            return new ReportRolePermissionDTO()
            {
                ReportId = e.ReportId,
                ActorCompanyId = e.ActorCompanyId,
                RoleId = e.RoleId,
            };
        }

        public static IEnumerable<ReportRolePermissionDTO> ToDTOs(this IEnumerable<ReportRolePermission> l)
        {
            var dtos = new List<ReportRolePermissionDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static List<ReportRolePermission> Filter(this List<ReportRolePermission> l, int reportId)
        {
            return l?.Where(i => i.ReportId == reportId).ToList() ?? new List<ReportRolePermission>();
        }

        public static bool HasReportRolePermission(this IEnumerable<ReportRolePermission> permissions, int reportId, int? roleId)
        {
            if (permissions == null || !roleId.HasValue)
                return true;

            //No permissions set for report means that all roles have permission
            List<ReportRolePermission> permissionsForReport = permissions.Where(p => p.ReportId == reportId).ToList();
            return permissionsForReport.Count == 0 || permissionsForReport.Any(p => p.RoleId == roleId.Value);
        }

        public static List<ReportRolePermissionDTO> Filter(this List<ReportRolePermissionDTO> l, int reportId)
        {
            return l?.Where(i => i.ReportId == reportId).ToList() ?? new List<ReportRolePermissionDTO>();
        }

        public static bool HasReportRolePermission(this IEnumerable<ReportRolePermissionDTO> permissions, int reportId, int? roleId)
        {
            if (permissions == null || !roleId.HasValue)
                return true;

            //No permissions set for report means that all roles have permission
            List<ReportRolePermissionDTO> permissionsForReport = permissions.Where(p => p.ReportId == reportId).ToList();
            return permissionsForReport.Count == 0 || permissionsForReport.Any(p => p.RoleId == roleId.Value);
        }

        #endregion
    }
}

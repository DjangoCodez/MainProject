using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System.Collections.Generic;


namespace SoftOne.Soe.Data
{
    public partial class ReportPrintout
    {

    }

    public static partial class EntityExtensions
    {
        #region ReportPrintout

        public static IEnumerable<ReportPrintoutDTO> ToDTOs(this IEnumerable<ReportPrintout> l, bool includeData, bool includeXml)
        {
            var dtos = new List<ReportPrintoutDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO(includeData, includeXml));
                }
            }
            return dtos;
        }

        public static ReportPrintoutDTO ToDTO(this ReportPrintout e, bool includeData, bool includeXml)
        {
            if (e == null)
                return null;

            return new ReportPrintoutDTO()
            {
                ReportPrintoutId = e.ReportPrintoutId,
                ActorCompanyId = e.ActorCompanyId,
                ReportId = e.ReportId,
                ReportPackageId = e.ReportPackageId,
                ReportUrlId = e.ReportUrlId,
                ReportTemplateId = e.ReportTemplateId,
                SysReportTemplateTypeId = e.SysReportTemplateTypeId,
                ExportType = (TermGroup_ReportExportType)e.ExportType,
                ExportFormat = (SoeExportFormat)e.ExportFormat,
                DeliveryType = (TermGroup_ReportPrintoutDeliveryType)e.DeliveryType,
                Status = e.Status,
                ResultMessage = e.ResultMessage,
                EmailMessage = e.EmailMessage,
                ReportName = e.ReportName,
                Selection = e.Selection,
                OrderedDeliveryTime = e.OrderedDeliveryTime,
                DeliveredTime = e.DeliveredTime,
                CleanedTime = e.CleanedTime,
                XML = includeXml ? e.XML : null,
                Data = includeData ? e.Data : null,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                UserId = e.UserId,
                RoleId = e.RoleId,
                ResultMessageDetails = e.ResultMessageDetails
            };
        }

        public static ReportPrintoutDTO ToDTO(this ReportPrintoutSmallView e)
        {
            if (e == null)
                return null;

            return new ReportPrintoutDTO()
            {
                ReportPrintoutId = e.ReportPrintoutId,
                ActorCompanyId = e.ActorCompanyId,
                ReportId = e.ReportId,
                ReportPackageId = e.ReportPackageId,
                ReportUrlId = e.ReportUrlId,
                ReportTemplateId = e.ReportTemplateId,
                SysReportTemplateTypeId = e.SysReportTemplateTypeId,
                ExportType = (TermGroup_ReportExportType)e.ExportType,
                ExportFormat = (SoeExportFormat)e.ExportFormat,
                DeliveryType = (TermGroup_ReportPrintoutDeliveryType)e.DeliveryType,
                Status = e.Status,
                ResultMessage = e.ResultMessage,
                EmailMessage = e.EmailMessage,
                ReportName = e.ReportName,
                Selection = e.Selection,
                OrderedDeliveryTime = e.OrderedDeliveryTime,
                DeliveredTime = e.DeliveredTime,
                CleanedTime = e.CleanedTime,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                UserId = e.UserId,
                RoleId = e.RoleId,
                ResultMessageDetails = e.ResultMessageDetails,
            };
        }

        #endregion
    }
}

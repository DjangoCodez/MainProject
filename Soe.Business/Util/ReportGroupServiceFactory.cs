using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SoftOne.Soe.Business.Util.ReportGroups
{
    public static class ReportGroupServiceFactory
    {
        public static IReportGroupService Create(bool isSupportAdmin, bool isInSupportLicense, ReportManager rm, int actorCompanyId)
        {
            if (isSupportAdmin && isInSupportLicense)
            {
                return new SysReportGroupService(rm);
            }
            else
            {
                return new CompReportGroupService(rm, actorCompanyId);
            }
        }
    }

    public interface IReportGroupService
    {
        ReportAbstractionDTO GetReport(int reportId);
        ReportGroupDTO GetReportGroup(int reportGroupId, bool loadReportGroupMapping, bool loadReportHeaderGroupMapping);
        List<ReportGroupDTO> GetReportGroups(int module, bool loadReportGroupMapping, bool loadReportHeaderGroupMapping);
        List<ReportGroupDTO> GetReportGroups(int module, int templateTypeId, bool loadReportGroupMapping, bool loadReportHeaderGroupMapping);
        ReportHeaderDTO GetReportHeader(int reportHeaderId, bool loadReportHeaderIntervals);
        List<ReportHeaderDTO> GetReportHeaders(int module, int templateTypeId, bool loadReportHeaderInterval);

        ActionResult DeleteReportGroup(int reportGroupId);
        ActionResult DeleteReportGroupHeaderMapping(int groupId, int headerId);
        ActionResult ReorderReportGroupHeaderMapping(int groupId, int reportHeaderId, bool isUp);
        ActionResult DeleteReportGroupMapping(int reportId, int groupId);
        ActionResult ReorderReportGroupMapping(int reportId, int groupId, bool isUp);
        bool ReportHeaderExistInReportGroup(int reportGroupId, int reportHeaderId);
        bool ReportGroupExistsInReport(int reportId, int reportGroupId);
        ActionResult AddReportGroupHeaderMapping(ReportGroupHeaderMappingDTO reportGroupHeaderMapping, int reportGroupId, int reportHeaderId);
        ActionResult AddReportGroupMapping(ReportGroupMappingDTO reportGroupMapping, int reportId, int reportGroupId);
        ActionResult UpdateReportGroup(ReportGroupDTO reportGroup);
        ActionResult AddReportGroup(ReportGroupDTO reportGroup);
        ActionResult DeleteReportGroupHeaderMappings(int reportGroupId);
        List<ReportGroupHeaderMappingDTO> GetReportGroupHeaderMappings(int reportGroupId);
        List<ReportGroupMappingDTO> GetReportGroupMappings(int reportId);
        ReportHeaderDTO GetPrevNextReportHeaderById(int reportHeaderId, int module, SoeFormMode mode);
        ReportGroupDTO GetPrevNextReportGroupById(int reportGroupId, int module, SoeFormMode mode);
        List<ReportHeaderIntervalDTO> GetReportHeaderIntervals(int reportHeaderId);
        ActionResult UpdateReportHeader(ReportHeaderDTO reportHeader, Collection<FormIntervalEntryItem> formIntervalEntryItems);
        ActionResult DeleteReportHeader(ReportHeaderDTO reportHeader);
        ActionResult DeleteReportHeaders(List<int> reportHeaderIds);
        ActionResult DeleteReportGroups(List<int> reportGroupIds);
        ActionResult AddReportHeader(ReportHeaderDTO reportHeader, Collection<FormIntervalEntryItem> formIntervalEntryItems);
        bool HasReportRolePermission(int reportId, int roleId);

    }
    public class CompReportGroupService : IReportGroupService
    {
        private ReportManager rm;
        private int ActorCompanyId;

        public CompReportGroupService(ReportManager rmIn, int actorCompanyIdIn) { 
            
            if (rmIn == null) throw new ArgumentNullException(nameof(rmIn));
            if (actorCompanyIdIn == 0) throw new ArgumentNullException(nameof(actorCompanyIdIn));

            this.rm = rmIn;
            this.ActorCompanyId = actorCompanyIdIn;
        }

        public ReportGroupDTO GetReportGroup(int reportGroupId, bool loadReportGroupMapping, bool loadReportHeaderGroupMapping)
        {
            return this.rm.GetReportGroup(reportGroupId, this.ActorCompanyId, loadReportGroupMapping, loadReportHeaderGroupMapping).ToDTO();
        }

        public List<ReportGroupDTO> GetReportGroups(int module, bool loadReportGroupMapping, bool loadReportHeaderGroupMapping)
        {
            return this.rm.GetReportGroupsByModule(module, this.ActorCompanyId, loadReportGroupMapping, loadReportHeaderGroupMapping).ToDTOs();
        }
        public List<ReportGroupDTO> GetReportGroups(int module, int templateTypeId, bool loadReportGroupMapping, bool loadReportHeaderGroupMapping)
        {
            return this.rm.GetReportGroupsByModule(module, this.ActorCompanyId, templateTypeId, loadReportGroupMapping, loadReportHeaderGroupMapping).ToDTOs();
        }

        public ActionResult AddReportGroup(ReportGroupDTO reportGroup)
        {
            return this.rm.AddReportGroup(reportGroup.FromDTO(), this.ActorCompanyId);
        }

        public ActionResult AddReportGroupHeaderMapping(ReportGroupHeaderMappingDTO reportGroupHeaderMapping, int reportGroupId, int reportHeaderId)
        {
            return this.rm.AddReportGroupHeaderMapping(reportGroupHeaderMapping.FromDTO(), reportGroupId, reportHeaderId, this.ActorCompanyId);
        }

        public ActionResult AddReportHeader(ReportHeaderDTO reportHeader, Collection<FormIntervalEntryItem> formIntervalEntryItems)
        {
            return this.rm.AddReportHeader(reportHeader.FromDTO(), this.ActorCompanyId, formIntervalEntryItems);
        }

        public ActionResult DeleteReportGroupHeaderMapping(int groupId, int headerId)
        {
            return this.rm.DeleteReportGroupHeaderMapping(groupId, headerId);
        }

        public ActionResult DeleteReportGroupHeaderMappings(int reportGroupId)
        {
            return this.rm.DeleteReportGroupHeaderMappings(reportGroupId, this.ActorCompanyId);
        }

        public ActionResult DeleteReportHeader(ReportHeaderDTO reportHeader)
        {
            return this.rm.DeleteReportHeader(reportHeader.FromDTO(), this.ActorCompanyId);
        }

        public ReportGroupDTO GetPrevNextReportGroupById(int reportGroupId, int module, SoeFormMode mode)
        {
            return this.rm.GetPrevNextReportGroupById(reportGroupId, module, this.ActorCompanyId, mode).ToDTO();
        }
        public ReportHeaderDTO GetPrevNextReportHeaderById(int reportHeaderId, int module, SoeFormMode mode)
        {
            return this.rm.GetPrevNextReportHeaderById(reportHeaderId, module, this.ActorCompanyId, mode).ToDTO();
        }

        public List<ReportGroupHeaderMappingDTO> GetReportGroupHeaderMappings(int reportGroupId)
        {
            return this.rm.GetReportGroupHeaderMappings(reportGroupId, this.ActorCompanyId).ToDTOs(true);
        }

        public List<ReportHeaderIntervalDTO> GetReportHeaderIntervals(int reportHeaderId)
        {
            return this.rm.GetReportHeaderIntervals(reportHeaderId).ToDTOs();
        }

        public ActionResult ReorderReportGroupHeaderMapping(int groupId, int reportHeaderId, bool isUp)
        {
            return this.rm.ReorderReportGroupHeaderMapping(groupId, reportHeaderId, isUp, this.ActorCompanyId);
        }

        public bool ReportHeaderExistInReportGroup(int reportGroupId, int reportHeaderId)
        {
            return this.rm.ReportHeaderExistInReportGroup(reportGroupId, reportHeaderId);
        }

        public ActionResult UpdateReportGroup(ReportGroupDTO reportGroup)
        {
            return this.rm.UpdateReportGroup(reportGroup.FromDTO(), this.ActorCompanyId);
        }

        public ActionResult UpdateReportHeader(ReportHeaderDTO reportHeader, Collection<FormIntervalEntryItem> formIntervalEntryItems)
        {
            var result = this.rm.UpdateReportHeader(reportHeader.FromDTO(), this.ActorCompanyId);
            if (result.Success)
            {
                result = this.rm.UpdateReportHeaderInterval(reportHeader.ReportHeaderId, this.ActorCompanyId, formIntervalEntryItems);
            }
            return result;
        }

        public ActionResult DeleteReportGroup(int reportGroupId)
        {
            var reportGroup = this.rm.GetReportGroup(reportGroupId, this.ActorCompanyId, false, false);
            return this.rm.DeleteReportGroup(reportGroup, this.ActorCompanyId);
        }

        public ReportHeaderDTO GetReportHeader(int reportHeaderId, bool loadReportHeaderIntervals)
        {
            return this.rm.GetReportHeader(reportHeaderId, this.ActorCompanyId, loadReportHeaderIntervals).ToDTO();
        }
        public List<ReportHeaderDTO> GetReportHeaders(int module, bool loadReportHeaderInterval = false)
        {
            return this.rm.GetReportHeadersByModule(module, this.ActorCompanyId, loadReportHeaderInterval).ToDTOs();
        }
        public ActionResult DeleteReportGroups(List<int> reportGroupIds)
        {
            return this.rm.DeleteReportGroups(reportGroupIds, this.ActorCompanyId);
        }
        public ActionResult DeleteReportHeaders(List<int> reportHeaderIds)
        {
            return this.rm.DeleteReportHeaders(reportHeaderIds, this.ActorCompanyId);
        }

        public List<ReportHeaderDTO> GetReportHeaders(int module, int templateTypeId, bool loadReportHeaderInterval)
        {
            if (templateTypeId <= 0) templateTypeId = -1;
            return this.rm.GetReportHeadersByModule(module, this.ActorCompanyId, templateTypeId, loadReportHeaderInterval).ToDTOs();
        }

        public ReportAbstractionDTO GetReport(int reportId)
        {
            var report = this.rm.GetReport(reportId, this.ActorCompanyId);

            if (report == null) return null;

            int templateTypeId = 0;
            if (report.Standard)
            {
                SysReportTemplate sysReportTemplate = rm.GetSysReportTemplate(report.ReportTemplateId);
                if (sysReportTemplate != null)
                    templateTypeId = sysReportTemplate.SysReportTemplateTypeId;
            }
            else
            {
                ReportTemplate reportTemplate = rm.GetReportTemplate(report.ReportTemplateId, this.ActorCompanyId);
                if (reportTemplate != null)
                    templateTypeId = reportTemplate.SysTemplateTypeId;
            }

            return new ReportAbstractionDTO
            {
                ReportId = report.ReportId,
                IsSys = false,
                ReportTemplateId = report.ReportTemplateId,
                SysTemplateTypeId = templateTypeId,
                Name = report.Name,
            };
        }

        public List<ReportGroupMappingDTO> GetReportGroupMappings(int reportId)
        {
            return this.rm.GetReportGroupMappings(reportId, this.ActorCompanyId).ToDTOs();
        }

        public ActionResult DeleteReportGroupMapping(int reportId, int groupId)
        {
            return this.rm.DeleteReportGroupMapping(reportId, groupId);
        }

        public ActionResult ReorderReportGroupMapping(int reportId, int groupId, bool isUp)
        {
            return this.rm.ReorderReportGroupMapping(reportId, groupId, this.ActorCompanyId, isUp);
        }

        public bool ReportGroupExistsInReport(int reportId, int reportGroupId)
        {
            return this.rm.ReportGroupExistInReport(reportId, reportGroupId);
        }

        public ActionResult AddReportGroupMapping(ReportGroupMappingDTO reportGroupMapping, int reportId, int reportGroupId)
        {
            return this.rm.AddReportGroupMapping(reportGroupMapping.FromDTO(), reportGroupId, reportId, this.ActorCompanyId);
        }

        public bool HasReportRolePermission(int reportId, int roleId)
        {
            return this.rm.HasReportRolePermission(reportId, roleId);
        }
    }

    public class SysReportGroupService : IReportGroupService
    {
        private ReportManager rm;
        public SysReportGroupService(ReportManager rmIn) {
            if (rmIn == null) throw new ArgumentNullException(nameof(rmIn));

            this.rm = rmIn;
        }
        public ReportGroupDTO GetReportGroup(int reportGroupId, bool loadReportGroupMapping, bool loadReportHeaderGroupMapping)
        {
            return this.rm.GetSysReportGroup(reportGroupId, loadReportGroupMapping, loadReportHeaderGroupMapping).ToDTO();
        }
        public List<ReportGroupDTO> GetReportGroups(int module, bool loadReportGroupMapping, bool loadReportHeaderGroupMapping)
        {
            return this.rm.GetSysReportGroups(loadReportGroupMapping, loadReportHeaderGroupMapping).ToDTOs();
        }
        public List<ReportGroupDTO> GetReportGroups(int module, int templateTypeId, bool loadReportGroupMapping, bool loadReportHeaderGroupMapping)
        {
            return this.rm.GetSysReportGroups(loadReportGroupMapping, loadReportHeaderGroupMapping, templateTypeId: templateTypeId).ToDTOs();
        }
        public ActionResult AddReportGroup(ReportGroupDTO reportGroup)
        {
            return this.rm.SaveSysReportGroup(reportGroup);
        }

        public ActionResult AddReportGroupHeaderMapping(ReportGroupHeaderMappingDTO reportGroupHeaderMapping, int reportGroupId, int reportHeaderId)
        {
            return this.rm.AddSysReportGroupHeaderMapping(reportGroupHeaderMapping.FromDTOToSys(), reportGroupId, reportHeaderId);
        }

        public ActionResult AddReportHeader(ReportHeaderDTO reportHeader, Collection<FormIntervalEntryItem> formIntervalEntryItems)
        {
            if (reportHeader.ReportHeaderIntervals == null)
                reportHeader.ReportHeaderIntervals = new List<ReportHeaderIntervalDTO>();

            foreach (FormIntervalEntryItem formIntervalEntryItem in formIntervalEntryItems)
            {
                ReportHeaderIntervalDTO reportHeaderInterval = new ReportHeaderIntervalDTO
                {
                    ReportHeaderId = reportHeader.ReportHeaderId,
                    IntervalFrom = formIntervalEntryItem.From,
                    IntervalTo = formIntervalEntryItem.To,
                    SelectValue = formIntervalEntryItem.LabelType.ToNullable()
                };
                
                reportHeader.ReportHeaderIntervals.Add(reportHeaderInterval);
            }
            return this.rm.SaveSysReportHeader(reportHeader);
        }

        public ActionResult DeleteReportGroupHeaderMapping(int groupId, int headerId)
        {
            return this.rm.DeleteSysReportGroupHeaderMapping(groupId, headerId);
        }

        public ActionResult DeleteReportGroupHeaderMappings(int reportGroupId)
        {
            return this.rm.DeleteSysReportGroupHeaderMappings(reportGroupId);
        }

        public ActionResult DeleteReportHeader(ReportHeaderDTO reportHeader)
        {
            return this.rm.DeleteSysReportHeader(reportHeader.ReportHeaderId);
        }

        public ReportGroupDTO GetPrevNextReportGroupById(int reportGroupId, int module, SoeFormMode mode)
        {
            return this.rm.GetSysReportGroup(reportGroupId, false, false).ToDTO();
        }
        public ReportHeaderDTO GetPrevNextReportHeaderById(int reportHeaderId, int module, SoeFormMode mode)
        {
            return this.rm.GetSysReportHeader(reportHeaderId, false).ToDTO();
        }
        public List<ReportGroupHeaderMappingDTO> GetReportGroupHeaderMappings(int reportGroupId)
        {
            var group = this.rm.GetSysReportGroup(reportGroupId, false, true);
            if (group != null)
            {
                return group.SysReportGroupHeaderMapping.ToDTOs(true);
            }
            return new List<ReportGroupHeaderMappingDTO>();
        }

        public List<ReportHeaderIntervalDTO> GetReportHeaderIntervals(int reportHeaderId)
        {
            var header = this.rm.GetSysReportHeader(reportHeaderId, true);
            if (header != null)
            {
                var dto = header.ToDTO();
                return dto.ReportHeaderIntervals;
            }
            return new List<ReportHeaderIntervalDTO>();
        }

        public ActionResult ReorderReportGroupHeaderMapping(int groupId, int reportHeaderId, bool isUp)
        {
            return this.rm.ReorderSysReportGroupHeaderMapping(groupId, reportHeaderId, isUp);
        }

        public bool ReportHeaderExistInReportGroup(int reportGroupId, int reportHeaderId)
        {
            return this.rm.SysReportHeaderExistsInGroup(reportGroupId, reportHeaderId);
        }

        public ActionResult UpdateReportGroup(ReportGroupDTO reportGroup)
        {
            return this.rm.SaveSysReportGroup(reportGroup);
        }

        public ActionResult UpdateReportHeader(ReportHeaderDTO reportHeader, Collection<FormIntervalEntryItem> formIntervalEntryItems)
        {
            if (reportHeader.ReportHeaderIntervals == null || formIntervalEntryItems.Count > 0)
                reportHeader.ReportHeaderIntervals = new List<ReportHeaderIntervalDTO>();

            foreach (FormIntervalEntryItem formIntervalEntryItem in formIntervalEntryItems)
            {
                ReportHeaderIntervalDTO reportHeaderInterval = new ReportHeaderIntervalDTO
                {
                    ReportHeaderId = reportHeader.ReportHeaderId,
                    IntervalFrom = formIntervalEntryItem.From,
                    IntervalTo = formIntervalEntryItem.To,
                    SelectValue = formIntervalEntryItem.LabelType.ToNullable()
                };

                reportHeader.ReportHeaderIntervals.Add(reportHeaderInterval);
            }
            return this.rm.SaveSysReportHeader(reportHeader);
        }

        public ActionResult DeleteReportGroup(int reportGroupId)
        {
            return this.rm.DeleteSysReportGroup(reportGroupId);
        }
        public ActionResult DeleteReportGroups(List<int> reportGroupIds)
        {
            return this.rm.DeleteSysReportGroups(reportGroupIds);
        }
        public ActionResult DeleteReportHeaders(List<int> reportHeaderIds)
        {
            return this.rm.DeleteSysReportHeaders(reportHeaderIds);
        }
        public ReportHeaderDTO GetReportHeader(int reportHeaderId, bool loadReportHeaderIntervals)
        {
            return this.rm.GetSysReportHeader(reportHeaderId, loadReportHeaderIntervals).ToDTO();
        }
        public List<ReportHeaderDTO> GetReportHeaders(int module, int templateTypeId, bool loadReportHeaderInterval = false)
        {
            if (templateTypeId <= 0)
                templateTypeId = -1;

            return this.rm.GetSysReportHeaders(loadReportHeaderInterval, templateTypeId: templateTypeId).ToDTOs();
        }

        public ReportAbstractionDTO GetReport(int reportId)
        {
            var template = this.rm.GetSysReportTemplate(reportId);
            if (template == null) return null;

            return new ReportAbstractionDTO()
            {
                Name = template.Name,
                ReportTemplateId = template.SysReportTemplateId,
                SysTemplateTypeId = template.SysReportTemplateTypeId,
                IsSys = true
            };
        }

        public List<ReportGroupMappingDTO> GetReportGroupMappings(int sysReportTemplateId)
        {
            return this.rm.GetSysReportGroupMappings(sysReportTemplateId).ToDTOs();
        }

        public ActionResult DeleteReportGroupMapping(int sysReportTemplateId, int sysGroupId)
        {
            return this.rm.DeleteSysReportGroupMapping(sysReportTemplateId, sysGroupId);
        }

        public ActionResult ReorderReportGroupMapping(int sysReportTemplateId, int sysGroupId, bool isUp)
        {
            return this.rm.ReorderSysReportGroupMapping(sysReportTemplateId, sysGroupId, isUp);
        }

        public bool ReportGroupExistsInReport(int sysReportTemplateId, int sysGroupId)
        {
            return this.rm.GetSysReportGroupMapping(sysReportTemplateId, sysGroupId) != null;
        }

        public ActionResult AddReportGroupMapping(ReportGroupMappingDTO reportGroupMapping, int reportId, int reportGroupId)
        {
            return this.rm.AddSysReportGroupMapping(reportGroupMapping.FromDTOToSys(), reportId, reportGroupId);
        }
        public bool HasReportRolePermission(int sysReportTemplateId, int roleId)
        {
            return true;
        }
    }
}
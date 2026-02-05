using SoftOne.Soe.Business.Core.CrGen;
using SoftOne.Soe.Business.DataCache;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Util;
using SoftOne.Soe.Util.Exceptions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Transactions;
using Newtonsoft.Json;
using System.Xml.Linq;
using System.IO;
using System.Data.Entity.Core.Objects.DataClasses;
using System.Data.Entity;
using SoftOne.Soe.Common.DTO.ApiExternal;
using SoftOne.Soe.Business.Util.ReportGroups;

namespace SoftOne.Soe.Business.Core
{
    public class ReportManager : ManagerBase
    {
        #region Variables

        // Create a logger for use in this class
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly Dictionary<int, SysReportTemplateType> sysReportTemplateTypesStandard = new Dictionary<int, SysReportTemplateType>();
        private readonly Dictionary<int, SysReportTemplateType> sysReportTemplateTypesCustom = new Dictionary<int, SysReportTemplateType>();

        #endregion

        #region Ctor

        public ReportManager(ParameterObject parameterObject) : base(parameterObject) { }

        #endregion

        #region DataStorage

        public DataStorage GetDataStorage(int dataStorageId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.DataStorage.NoTracking();
            return GetDataStorage(entities, dataStorageId);
        }

        public DataStorage GetDataStorage(CompEntities entities, int dataStorageId)
        {
            return GeneralManager.GetDataStorage(entities, dataStorageId, base.ActorCompanyId);
        }

        #endregion

        #region Report

        public List<Report> GetReports(int actorCompanyId, int? roleId, List<int> reportIds = null, List<int> sysReportTemplateTypeIds = null, int? module = null, bool onlyOriginal = false, bool onlyStandard = false, bool onlyWithSelections = false, bool loadSysReportTemplate = false, bool loadReportSelection = false, bool loadRolePermission = false, bool onlyShowInAccountingReports = false, bool setTemplateTypeProperties = true, bool filterReports = true, bool setIsSystemReport = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Report.NoTracking();
            return GetReports(entities, actorCompanyId, roleId, reportIds, sysReportTemplateTypeIds, module, onlyOriginal, onlyStandard, onlyWithSelections, loadSysReportTemplate, loadReportSelection, loadRolePermission, onlyShowInAccountingReports, setTemplateTypeProperties, filterReports: filterReports, setIsSystemReport: setIsSystemReport);
        }

        public List<Report> GetReports(CompEntities entities, int actorCompanyId, int? roleId, List<int> reportIds = null, List<int> sysReportTemplateTypeIds = null, int? module = null, bool onlyOriginal = false, bool onlyStandard = false, bool onlyWithSelections = false, bool loadSysReportTemplate = false, bool loadReportSelection = false, bool loadRolePermission = false, bool onlyShowInAccountingReports = false, bool setTemplateTypeProperties = true, bool filterReports = true, bool setIsSystemReport = false)
        {
            IQueryable<Report> query = entities.Report;
            if (loadReportSelection)
                query = query.Include("ReportSelection");
            if (loadRolePermission)
                query = query.Include("ReportRolePermission");

            query = (from a in query
                     where a.ActorCompanyId == actorCompanyId &&
                     a.State == (int)SoeEntityState.Active
                     select a);

            if (!reportIds.IsNullOrEmpty())
                query = query.Where(r => reportIds.Contains(r.ReportId));
            if (module.HasValue)
                query = query.Where(r => r.Module == module.Value || r.Module == (int)SoeModule.None);

            List<Report> reports = query.ToList();
            if (reports.Any())
            {
                List<ReportRolePermissionDTO> permissions = roleId.HasValue ? GetReportRolePermissionsFromCache(actorCompanyId) : null;
                if (filterReports)
                {
                    reports = FilterValidReports(reports, permissions, roleId, onlyOriginal, onlyStandard, onlyWithSelections, onlyShowInAccountingReports);
                    if (reports.Any())
                    {
                        if (!sysReportTemplateTypeIds.IsNullOrEmpty())
                            loadSysReportTemplate = true;

                        SetReportProperties(reports, actorCompanyId, permissions, loadSysReportTemplate: loadSysReportTemplate, setTemplateTypeProperties: setTemplateTypeProperties, setIsSystemReport: setIsSystemReport);

                        if (loadSysReportTemplate)
                            reports = reports.Where(r => r.SysReportTemplateTypeId.HasValue && sysReportTemplateTypeIds.Contains(r.SysReportTemplateTypeId.Value)).ToList();
                    }
                }
            }

            return reports;
        }

        public List<Report> GetReportsByTemplateType(int actorCompanyId, int? roleId, SoeReportTemplateType sysReportTemplateType, int? module = null, bool onlyOriginal = false, bool onlyStandard = false, bool onlyWithSelections = false, bool loadSysReportTemplate = false, bool loadReportSelection = false, bool loadRolePermission = false)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            entitiesReadOnly.Report.NoTracking();
            return GetReports(
                entitiesReadOnly,
                actorCompanyId,
                roleId,
                sysReportTemplateTypeIds: new List<int> { (int)sysReportTemplateType },
                module: module,
                onlyOriginal: onlyOriginal,
                onlyStandard: onlyStandard,
                onlyWithSelections: onlyWithSelections,
                loadSysReportTemplate: loadSysReportTemplate,
                loadReportSelection: loadReportSelection,
                loadRolePermission: loadRolePermission);
        }

        public List<Report> GetReportByPackage(int actorCompanyId, int? roleId, int reportPackageId)
        {
            List<Report> validReports = new List<Report>();

            //Get ReportPackage
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.ReportPackage.NoTracking();
            var reportPackage = (from rp in entities.ReportPackage.Include("Report")
                                 where rp.ReportPackageId == reportPackageId &&
                                 rp.Company.ActorCompanyId == actorCompanyId
                                 select rp).FirstOrDefault();

            if (reportPackage != null)
            {
                List<ReportRolePermissionDTO> permissions = roleId.HasValue ? GetReportRolePermissionsFromCache(actorCompanyId) : null;

                foreach (Report report in reportPackage.GetActiveReports())
                {
                    if (!permissions.HasReportRolePermission(report.ReportId, roleId))
                        continue;

                    validReports.Add(report);
                }

                SetReportProperties(validReports, actorCompanyId, permissions);
            }

            return validReports.OrderBy(r => r.ReportNr).ToList();
        }

        public List<Report> GetReportsWithDrilldown(int actorCompanyId, int? roleId, bool onlyOriginal = false, bool onlyStandard = false)
        {
            return GetReports(actorCompanyId, roleId, onlyOriginal: onlyOriginal, onlyStandard: onlyStandard, onlyShowInAccountingReports: true);
        }

        public Dictionary<int, string> GetReportsByTemplateTypeDict(int actorCompanyId, SoeReportTemplateType sysReportTemplateType, bool onlyOriginal = false, bool onlyStandard = false, bool addEmptyRow = false, int? roleId = null)
        {
            Dictionary<int, string> dict = new Dictionary<int, string>();

            if (addEmptyRow)
                dict.Add(0, " ");

            List<Report> reports = GetReportsByTemplateType(actorCompanyId, roleId, sysReportTemplateType, onlyOriginal: onlyOriginal, onlyStandard: onlyStandard);
            foreach (Report report in reports)
            {
                //Take description if exists
                string value = report.Description;
                if (string.IsNullOrEmpty(value))
                    value = report.Name;

                //Include ReportNr
                value = report.ReportNr + ". " + value;

                if (!dict.ContainsKey(report.ReportId))
                    dict.Add(report.ReportId, value);
            }

            return dict;
        }

        public TermGroup_ReportExportType GetReportExportType(int reportId, int actorCompanyId)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            return (TermGroup_ReportExportType)(from r in entitiesReadOnly.Report
                                                where r.ReportId == reportId &&
                                                r.ActorCompanyId == actorCompanyId &&
                                                r.State == (int)SoeEntityState.Active
                                                select r.ExportType).FirstOrDefault();
        }

        public Report GetReport(int reportId, int actorCompanyId, bool loadReportGroupMapping = false, bool loadReportSelection = false, bool loadReportRolePermission = false, bool loadSysReportTemplateType = false, bool loadSettings = false, bool loadSysReportTemplateSettings = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.ReportTemplate.NoTracking();
            return GetReport(entities, reportId, actorCompanyId, loadReportGroupMapping, loadReportSelection, loadReportRolePermission, loadSysReportTemplateType, loadSettings, loadSysReportTemplateSettings);
        }

        public Report GetReport(CompEntities entities, int reportId, int actorCompanyId, bool loadReportGroupMapping = false, bool loadReportSelection = false, bool loadReportRolePermission = false, bool loadSysReportTemplateType = false, bool loadSettings = true, bool loadSysReportTemplateSettings = false)
        {
            IQueryable<Report> query = (from r in entities.Report
                                        where r.ReportId == reportId &&
                                        r.ActorCompanyId == actorCompanyId &&
                                        r.State == (int)SoeEntityState.Active
                                        select r);

            if (loadSettings)
            {
                query = query.Include("ReportSetting");
            }

            var report = query.FirstOrDefault();

            if (report != null)
            {
                if (loadReportGroupMapping && !report.ReportGroupMapping.IsLoaded)
                    report.ReportGroupMapping.Load();

                if (loadReportSelection)
                {
                    if (!report.ReportSelectionReference.IsLoaded)
                        report.ReportSelectionReference.Load();

                    if (report.ReportSelection != null)
                    {
                        if (!report.ReportSelection.ReportSelectionInt.IsLoaded)
                            report.ReportSelection.ReportSelectionInt.Load();
                        if (!report.ReportSelection.ReportSelectionStr.IsLoaded)
                            report.ReportSelection.ReportSelectionStr.Load();
                        if (!report.ReportSelection.ReportSelectionDate.IsLoaded)
                            report.ReportSelection.ReportSelectionDate.Load();
                    }
                }

                if (loadSysReportTemplateSettings)
                    SetReportSysReportTemplateSettings(report);

                if (loadReportRolePermission && !report.ReportRolePermission.IsLoaded)
                    report.ReportRolePermission.Load();

                if (loadSysReportTemplateType)
                    SetReportSysReportTemplateTypeProperties(report, actorCompanyId);
            }

            return report;
        }

        public Report GetReportByNr(int actorCompanyId, int reportNr)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Report.NoTracking();
            return GetReportByNr(entities, actorCompanyId, reportNr);
        }

        public Report GetReportByNr(CompEntities entities, int actorCompanyId, int reportNr)
        {
            return (from r in entities.Report
                    where r.ReportNr == reportNr &&
                    r.ActorCompanyId == actorCompanyId &&
                    r.State == (int)SoeEntityState.Active
                    select r).FirstOrDefault();
        }

        public Report GetReportByNr(CompEntities entities, int actorCompanyId, int reportNr, SoeReportTemplateType reportTemplateType)
        {
            List<Report> reports = (from r in entities.Report
                                    where r.ReportNr == reportNr &&
                                    r.ActorCompanyId == actorCompanyId &&
                                    r.State == (int)SoeEntityState.Active
                                    select r).ToList();

            foreach (Report report in reports)
            {
                SysReportTemplateType sysReportTemplateType = GetSysReportTemplateType(report, actorCompanyId);
                if (sysReportTemplateType != null && sysReportTemplateType.SysReportTemplateTypeId == (int)reportTemplateType)
                    return report;
            }

            return null;
        }

        public Report GetPrevNextReport(int reportId, int module, int actorCompanyId, SoeFormMode mode)
        {
            Report report = null;
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            entitiesReadOnly.Report.NoTracking();
            if (mode == SoeFormMode.Next)
            {
                report = (from r in entitiesReadOnly.Report
                          where r.ReportId > reportId &&
                          r.Module == module &&
                          r.ActorCompanyId == actorCompanyId &&
                          r.State == (int)SoeEntityState.Active
                          orderby r.ReportId ascending
                          select r).FirstOrDefault();
            }
            else if (mode == SoeFormMode.Prev)
            {
                report = (from r in entitiesReadOnly.Report
                          where r.ReportId < reportId &&
                          r.Module == module &&
                          r.ActorCompanyId == actorCompanyId &&
                          r.State == (int)SoeEntityState.Active
                          orderby r.ReportId descending
                          select r).FirstOrDefault();
            }

            return report;
        }
        public Report GetStandardReport(int actorCompanyId, SoeReportTemplateType sysReportTemplateType, SoeReportType sysReportType = SoeReportType.CrystalReport)
        {
            Report report = null;
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            entitiesReadOnly.ReportTemplate.NoTracking();
            using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
            List<int> sysReportTemplateIds = (from srt in sysEntitiesReadOnly.SysReportTemplate
                                              where srt.SysReportTemplateTypeId == (int)sysReportTemplateType &&
                                              srt.SysReportTypeId == (int)sysReportType
                                              select srt.SysReportTemplateId).ToList();

            if (sysReportTemplateIds != null)
            {
                entitiesReadOnly.Report.NoTracking();
                report = (from r in entitiesReadOnly.Report
                          where sysReportTemplateIds.Contains(r.ReportTemplateId) &&
                          r.ActorCompanyId == actorCompanyId &&
                          r.State == (int)SoeEntityState.Active
                          && r.Standard
                          orderby r.ReportNr, r.ReportId
                          select r).FirstOrDefault();
            }

            return report;
        }
        public Report GetSettingReport(SettingMainType settingMainType, CompanySettingType settingType, SoeReportTemplateType reportTemplateType, int actorCompanyId, int userId, int? roleId = null)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetSettingReport(entities, settingMainType, settingType, reportTemplateType, actorCompanyId, userId, roleId);
        }

        public Report GetSettingReport(CompEntities entities, SettingMainType settingMainType, CompanySettingType settingType, SoeReportTemplateType reportTemplateType, int actorCompanyId, int userId, int? roleId = null)
        {
            Report report = null;

            // Get from setting
            int defaultReportId = SettingManager.GetIntSetting(entities, settingMainType, (int)settingType, userId, actorCompanyId, 0);
            if (defaultReportId > 0)
            {
                report = GetReport(defaultReportId, actorCompanyId, loadSysReportTemplateType: true);
                if (report != null)
                {
                    //Validate SysReportTemplateTypeId
                    if (!report.SysReportTemplateTypeId.HasValue || report.SysReportTemplateTypeId.Value != (int)reportTemplateType)
                        return null;

                    //Validate ReportRolePermission
                    if (roleId.HasValue)
                    {
                        List<ReportRolePermission> permissions = GetReportRolePermissionsByReport(entities, report.ReportId);
                        if (!permissions.HasReportRolePermission(report.ReportId, roleId))
                            report = null;
                    }
                }
            }

            return report;
        }

        public Report GetSettingOrStandardReport(SettingMainType settingMainType, CompanySettingType settingType, SoeReportTemplateType reportTemplateType, SoeReportType sysReportType, int actorCompanyId, int userId, int? roleId = null)
        {
            Report report = GetSettingReport(settingMainType, settingType, reportTemplateType, actorCompanyId, userId, roleId);
            if (report == null)
            {
                report = GetStandardReport(actorCompanyId, reportTemplateType, sysReportType);
                if (report != null)
                {
                    //Since GetSettingReport is setting these properties.....
                    SetReportSysReportTemplateTypeProperties(report, actorCompanyId);
                }
            }

            return report;
        }

        public Report GetBillingInvoiceProjectTimeProjectReport(SoeReportTemplateType reportTemplateType, int actorCompanyId, int roleId, bool ignoreInclude = false)
        {
            Report report = null;

            if (reportTemplateType.IsValidIn(SoeReportTemplateType.BillingInvoice, SoeReportTemplateType.BillingOrder, SoeReportTemplateType.BillingOffer, SoeReportTemplateType.TimeProjectReport))
            {
                bool skipMergeProjectReport = false;

                if (!ignoreInclude && reportTemplateType == SoeReportTemplateType.TimeProjectReport)
                    ignoreInclude = true;
                if (!ignoreInclude && reportTemplateType.IsValidIn(SoeReportTemplateType.BillingInvoice, SoeReportTemplateType.BillingOrder))
                    skipMergeProjectReport = true;

                if (!skipMergeProjectReport || ignoreInclude)
                    report = GetSettingOrStandardReport(SettingMainType.Company, CompanySettingType.BillingDefaultTimeProjectReportTemplate, SoeReportTemplateType.TimeProjectReport, SoeReportType.CrystalReport, actorCompanyId, 0, roleId);
            }

            return report;
        }
        public Report GetCompanySettingReport(SettingMainType settingMainType, CompanySettingType settingType, SoeReportTemplateType reportTemplateType, int actorCompanyId, int userId, int? roleId = null)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetCompanySettingReport(entities, settingMainType, settingType, reportTemplateType, actorCompanyId, userId, roleId);
        }
        public Report GetCompanySettingReport(CompEntities entities, SettingMainType settingMainType, CompanySettingType settingType, SoeReportTemplateType reportTemplateType, int actorCompanyId, int userId, int? roleId = null)
        {
            return GetSettingReport(entities, settingMainType, settingType, reportTemplateType, actorCompanyId, userId, roleId);
        }

        public int GetCompanySettingReportId(SettingMainType settingMainType, CompanySettingType settingType, SoeReportTemplateType reportTemplateType, int actorCompanyId, int userId, int? roleId = null)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetCompanySettingReportId(entities, settingMainType, settingType, reportTemplateType, actorCompanyId, userId, roleId);
        }

        public int GetCompanySettingReportId(CompEntities entities, SettingMainType settingMainType, CompanySettingType settingType, SoeReportTemplateType reportTemplateType, int actorCompanyId, int userId, int? roleId = null)
        {
            string key = $"GetCompanySettingReportId#{settingMainType}#{settingType}#{reportTemplateType}#{actorCompanyId}#{userId}#{roleId}";
            var value = BusinessMemoryCache<int?>.Get(key);

            if (value.HasValue)
                return value.Value;

            Report report = GetCompanySettingReport(entities, settingMainType, settingType, reportTemplateType, actorCompanyId, userId, roleId);
            value = report?.ReportId ?? 0;
            BusinessMemoryCache<int?>.Set(key, value);

            return value.Value;
        }

        public int GetNextFreeReportNr(int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.ReportTemplate.NoTracking();
            Report report = (from r in entities.Report
                             where r.ActorCompanyId == actorCompanyId
                             orderby r.ReportNr descending
                             select r).FirstOrDefault();

            return report?.ReportNr + 1 ?? 1;
        }

        public bool HasReportGroupsAndHeaders(int actorCompanyId, int reportId, int sysReportTemplateId, bool reportIsStandard)
        {
            if (reportIsStandard && this.SysReportTemplateHasGroupsAndHeaders(sysReportTemplateId))
                return true;

            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            var entities = entitiesReadOnly;
            var groupIds = entities.ReportGroupMapping
                .Where(m => m.ReportId == reportId)
                .Select(m => m.ReportGroupId)
                .ToList();

            if (groupIds.IsNullOrEmpty())
                return false;

            var mappings = entities.ReportGroupHeaderMapping
                .Where(m => groupIds.Contains(m.ReportGroupId))
                .Select(m => m.ReportGroupId)
                .ToList();

            return groupIds.Any(g => mappings.Any(m => m == g));
        }
        public bool HasReportGroupsAndHeaders(int reportId)
        {
            using var entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            entitiesReadOnly.Report.NoTracking();
            List<ReportGroup> reportGroups = (from rgm in entitiesReadOnly.ReportGroupMapping
                                              where rgm.Report.ReportId == reportId
                                              select rgm.ReportGroup).ToList();

            foreach (ReportGroup reportGroup in reportGroups)
            {
                bool exists = (from rghm in entitiesReadOnly.ReportGroupHeaderMapping
                               where rghm.ReportGroupId == reportGroup.ReportGroupId
                               select rghm.ReportHeader).Any();

                if (exists)
                    return true;
            }

            return false;
        }

        public bool ReportExist(int actorCompanyId, int reportNr)
        {
            return GetReportByNr(actorCompanyId, reportNr) != null;
        }

        public bool ExistsStandardReport(SoeReportTemplateType sysReportTemplate, SoeReportType sysReportType, int actorCompanyId)
        {
            return GetStandardReport(actorCompanyId, sysReportTemplate, sysReportType) != null;
        }

        public bool IsAccountingReport(int selectionType)
        {
            return (selectionType > 0 && selectionType <= 10);
        }

        public bool IsLedgerReport(int selectionType)
        {
            return (selectionType > 10 && selectionType <= 20);
        }

        public bool IsBillingReport(int module, int selectionType, int sysReportTemplateTypeId)
        {
            //Fix for reminder and interest (moved from billing to economy)
            if (sysReportTemplateTypeId == (int)SoeReportTemplateType.BillingInvoiceInterest || sysReportTemplateTypeId == (int)SoeReportTemplateType.BillingInvoiceReminder)
                return true;

            return (selectionType > 20 && selectionType <= 30) || (module == (int)SoeModule.Billing);
        }

        public bool IsTimeReport(int module, int selectionType)
        {
            return (selectionType > 30 && selectionType <= 40) || (module == (int)SoeModule.Time);
        }

        public List<GenericType> GetReportExportTypes(Report report, SoeReportType sysReportType)
        {
            if (report == null)
                return new List<GenericType>();

            int? reportTemplateId = null, sysReportTemplateId = null;
            if (report.Standard)
                sysReportTemplateId = report.ReportTemplateId;
            else
                reportTemplateId = report.ReportTemplateId;

            return GetReportExportTypes(sysReportTemplateId, reportTemplateId, sysReportType);
        }

        public List<GenericType> GetReportExportTypes(int? sysReportTemplateId, int? reportTemplateId, SoeReportType sysReportType)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
            string validExportTypes = "";
            if (sysReportTemplateId.HasValue)
                validExportTypes = sysEntitiesReadOnly.SysReportTemplate.Where(t => t.SysReportTemplateId == sysReportTemplateId.Value && t.SysReportTypeId == (int)sysReportType).Select(t => t.ValidExportTypes).FirstOrDefault();
            else if (reportTemplateId.HasValue)
                validExportTypes = entitiesReadOnly.ReportTemplate.Where(t => t.ReportTemplateId == reportTemplateId.Value && t.SysReportTypeId == (int)sysReportType).Select(t => t.ValidExportTypes).FirstOrDefault();

            return GetReportExportTypes(ReportTemplateDTO.GetValidExportTypes(validExportTypes, sysReportType));
        }

        public List<GenericType> GetReportExportTypes(List<int> validExportTypes)
        {
            List<GenericType> exportTypes = new List<GenericType>();

            if (validExportTypes.IsNullOrEmpty())
                return exportTypes;

            List<GenericType> allExportTypes = base.GetTermGroupContent(TermGroup.ReportExportType);
            if (allExportTypes.IsNullOrEmpty())
                return exportTypes;

            foreach (int validExportType in validExportTypes)
            {
                GenericType exportType = allExportTypes.FirstOrDefault(i => i.Id == validExportType);
                if (exportType != null)
                    exportTypes.Add(exportType);
            }

            return exportTypes;
        }

        public List<ReportSettingDTO> GetReportSettings(CompEntities entities, int reportId)
        {
            return entities.ReportSetting
               .Where(m => m.ReportId == reportId && m.State == (int)SoeEntityState.Active)
               .Select(m => new ReportSettingDTO
               {
                   ReportId = m.ReportId,
                   Type = (TermGroup_ReportSettingType)m.Type,
                   IntData = m.IntData,
                   StrData = m.StrData,
                   BoolData = m.BoolData,
                   DataTypeId = (SettingDataType)m.DataTypeId,
                   Value = m.Value
               }
               ).ToList();
        }

        public ActionResult SaveReport(ReportDTO reportDTO, int actorCompanyId)
        {
            if (reportDTO == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "Provided Report to save is empty");

            // Default result is successful
            ActionResult result = new ActionResult();

            int reportId = reportDTO.ReportId;

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        #region Report

                        // Get existing report
                        Report report = reportId > 0 ? GetReport(entities, reportId, actorCompanyId, false, false, true, false, true) : null;
                        if (report == null)
                        {
                            #region Report Add

                            // New report, check if specified number already exists
                            if (ReportNrExists(entities, actorCompanyId, reportId, reportDTO.ReportNr))
                                return new ActionResult((int)ActionResultSave.Duplicate, GetText(12115, "Angivet rapportnummer finns redan på en annan rapport. \n Välj ett annat nummer och försök igen."));

                            report = new Report()
                            {
                                ActorCompanyId = actorCompanyId,
                                ReportRolePermission = new System.Data.Entity.Core.Objects.DataClasses.EntityCollection<ReportRolePermission>()
                            };

                            SetCreatedProperties(report);
                            entities.Report.AddObject(report);

                            #endregion
                        }
                        else
                        {
                            #region Report Update

                            // Existing report, if report number has changed, check if specified number already exists
                            if (report.ReportNr != reportDTO.ReportNr && ReportNrExists(entities, actorCompanyId, reportId, reportDTO.ReportNr))
                                return new ActionResult((int)ActionResultSave.Duplicate, GetText(12115, "Angivet rapportnummer finns redan på en annan rapport. \n Välj ett annat nummer och försök igen."));

                            SetModifiedProperties(report);

                            #endregion
                        }
                        report.Standard = reportDTO.Standard;
                        report.Module = (int)reportDTO.Module;
                        report.Original = true;
                        report.ReportTemplateId = reportDTO.ReportTemplateId;
                        report.ReportNr = reportDTO.ReportNr;
                        report.Name = reportDTO.Name;
                        report.Description = reportDTO.Description;
                        report.ExportType = (int)reportDTO.ExportType;
                        report.FileType = (int)reportDTO.ExportFileType;
                        report.IncludeAllHistoricalData = reportDTO.IncludeAllHistoricalData;
                        report.IncludeBudget = reportDTO.IncludeBudget;
                        report.NoOfYearsBackinPreviousYear = reportDTO.NoOfYearsBackinPreviousYear;
                        report.GetDetailedInformation = reportDTO.DetailedInformation;
                        report.ShowInAccountingReports = reportDTO.ShowInAccountingReports;
                        report.GroupByLevel1 = (int)reportDTO.GroupByLevel1;
                        report.GroupByLevel2 = (int)reportDTO.GroupByLevel2;
                        report.GroupByLevel3 = (int)reportDTO.GroupByLevel3;
                        report.GroupByLevel4 = (int)reportDTO.GroupByLevel4;
                        report.SortByLevel1 = (int)reportDTO.SortByLevel1;
                        report.SortByLevel2 = (int)reportDTO.SortByLevel2;
                        report.SortByLevel3 = (int)reportDTO.SortByLevel3;
                        report.SortByLevel4 = (int)reportDTO.SortByLevel4;
                        report.Special = reportDTO.Special;
                        report.IsSortAscending = reportDTO.IsSortAscending;
                        report.ShowRowsByAccount = reportDTO.ShowRowsByAccount;
                        report.NrOfDecimals = reportDTO.NrOfDecimals;

                        #region Report Sync Roles
                        var allRoles = report.ReportRolePermission.ToDictionary(r => r.RoleId);
                        var allActiveRoles = report.ReportRolePermission.Where(r => r.State == (int)SoeEntityState.Active).Select(r => r.RoleId).ToArray();
                        var addedRoles = reportDTO.RoleIds.Except(allActiveRoles).ToArray();
                        var removedRoles = allActiveRoles.Except(reportDTO.RoleIds).ToArray();

                        foreach (var removed in removedRoles)
                        {
                            ChangeEntityState(allRoles[removed], SoeEntityState.Deleted);
                        }

                        foreach (var added in addedRoles)
                        {
                            if (allRoles.TryGetValue(added, out ReportRolePermission role))
                            {
                                ChangeEntityState(role, SoeEntityState.Active);
                            }
                            else
                            {
                                role = new ReportRolePermission
                                {
                                    ActorCompanyId = actorCompanyId,
                                    RoleId = added
                                };

                                SetCreatedProperties(role);
                                report.ReportRolePermission.Add(role);
                            }

                        }
                        #endregion

                        #region Report Settings

                        if (!reportDTO.Settings.IsNullOrEmpty())
                        {
                            foreach (var settingDto in reportDTO.Settings)
                            {
                                var setting = report.ReportSetting.FirstOrDefault(s => s.Type == (int)settingDto.Type);
                                if (setting == null)
                                {
                                    setting = new ReportSetting
                                    {
                                        Type = (int)settingDto.Type,
                                        Name = "",
                                        DataTypeId = (int)GetReportSettingDataType(settingDto.Type),

                                    };
                                    SetCreatedProperties(setting);
                                    report.ReportSetting.Add(setting);
                                }
                                else
                                {
                                    SetModifiedProperties(setting);
                                }

                                setting.IntData = settingDto.IntData;
                                setting.StrData = string.IsNullOrEmpty(settingDto.StrData) ? null : settingDto.StrData;
                                setting.BoolData = settingDto.BoolData;
                            }
                        }

                        #endregion

                        result = SaveChanges(entities, transaction);
                        if (result.Success)
                        {
                            if (reportDTO.ImportCompanyId > 0 && reportDTO.ImportReportId > 0)
                            {
                                if (reportDTO.IsNewGroupsAndHeaders)
                                    this.ImportReportHeadersAndReportGroupsFromReportCreateNew(entities, reportDTO.ImportReportId, reportDTO.ReportId, reportDTO.ImportCompanyId, actorCompanyId);
                                else
                                    this.ImportReportHeadersAndReportGroupsFromReportReuseExisting(entities, reportDTO.ImportReportId, reportDTO.ReportId, reportDTO.ImportCompanyId, actorCompanyId);
                            }
                            //Commit transaction
                            transaction.Complete();

                            reportId = report.ReportId;
                        }

                        #endregion
                    }
                }
                catch (Exception ex)
                {
                    base.LogError(ex, this.log);
                    result.Exception = ex;
                    result.IntegerValue = 0;
                }
                finally
                {
                    if (result.Success)
                    {
                        //Set success properties
                        result.IntegerValue = reportId;
                    }
                    else
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }

                return result;
            }
        }

        private bool ReportNrExists(CompEntities entities, int actorCompanyId, int reportId, int reportNr)
        {
            return entities.Report.Where(r => r.ActorCompanyId == actorCompanyId && r.ReportNr == reportNr && r.ReportId != reportId && r.State == (int)SoeEntityState.Active).Any();
        }

        private static SettingDataType GetReportSettingDataType(TermGroup_ReportSettingType setting)
        {
            switch (setting)
            {
                case TermGroup_ReportSettingType.ProjectOverviewExtendedInfo:
                case TermGroup_ReportSettingType.StockInventoryExcludeZeroQuantity:
                case TermGroup_ReportSettingType.ExcludeItemsWithZeroQuantityForSpecificDate:
                case TermGroup_ReportSettingType.HidePriceAndRowSum:
                    return SettingDataType.Boolean;
                case TermGroup_ReportSettingType.GroupedInvoiceByTaxDeduction:
                    return SettingDataType.Boolean;
                case TermGroup_ReportSettingType.GroupedOfferByTaxDeduction:
                    return SettingDataType.Boolean;
                default:
                    return SettingDataType.String;
            }
        }

        public ActionResult AddReport(Report report, int actorCompanyId)
        {
            if (report == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "Report");

            using (CompEntities entities = new CompEntities())
            {
                report.Company = CompanyManager.GetCompany(entities, actorCompanyId);
                if (report.Company == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte"));

                return AddEntityItem(entities, report, "Report");
            }
        }

        public ActionResult UpdateReport(Report report, int actorCompanyId)
        {
            if (report == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "Report");

            using (CompEntities entities = new CompEntities())
            {
                Report originalReport = GetReport(entities, report.ReportId, actorCompanyId);
                if (originalReport == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "Report");

                return this.UpdateEntityItem(entities, originalReport, report, "Report");
            }
        }

        public ActionResult DeleteReport(int reportId, int actorCompanyId)
        {
            using (CompEntities entities = new CompEntities())
            {
                Report report = GetReport(entities, reportId, actorCompanyId);
                if (report == null)
                    return new ActionResult((int)ActionResultDelete.EntityNotFound, "Report");

                if (!report.ReportPackage.IsLoaded)
                    report.ReportPackage.Load();
                if (report.ReportPackage.Any(i => i.State == (int)SoeEntityState.Active))
                    return new ActionResult((int)ActionResultDelete.ReportHasReportPackages, GetText(11551, "Rapporten är kopplad till rapportpaket"));

                if (!report.PayrollGroupReport.IsLoaded)
                    report.PayrollGroupReport.Load();
                if (report.PayrollGroupReport.Any(i => i.State == (int)SoeEntityState.Active))
                    return new ActionResult((int)ActionResultDelete.ReportHasReportPayrollGroups, GetText(11552, "Rapporten är kopplad till löneavtal"));

                if (!report.ChecklistHead.IsLoaded)
                    report.ChecklistHead.Load();
                if (report.ChecklistHead.Any(i => i.State == (int)SoeEntityState.Active))
                    return new ActionResult((int)ActionResultDelete.ReportHasReportChecklists, GetText(11553, "Rapporten är kopplad till checklistor"));

                // Customers
                if (entities.Customer.Any(c => c.ActorCompanyId == actorCompanyId && c.State == (int)SoeEntityState.Active && (c.AgreementTemplate == reportId || c.BillingTemplate == reportId || c.OrderTemplate == reportId || c.OfferTemplate == reportId)))
                    return new ActionResult((int)ActionResultDelete.ReportHasCustomers, GetText(7883, "Rapporten är kopplad till kunder"));

                if (!report.ReportRolePermission.IsLoaded)
                    report.ReportRolePermission.Load();

                foreach (var role in report.ReportRolePermission)
                {
                    ChangeEntityState(entities, role, SoeEntityState.Deleted, false, discardCheckes: true);
                }

                return ChangeEntityState(entities, report, SoeEntityState.Deleted, true);
            }
        }

        public ActionResult DeleteReport(Report report, int actorCompanyId)
        {
            if (report == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, "Report");

            using (CompEntities entities = new CompEntities())
            {
                Report originalReport = GetReport(entities, report.ReportId, actorCompanyId);
                if (originalReport == null)
                    return new ActionResult((int)ActionResultDelete.EntityNotFound, "Report");

                if (!report.ReportPackage.IsLoaded)
                    report.ReportPackage.Load();
                if (report.ReportPackage.Any(i => i.State == (int)SoeEntityState.Active))
                    return new ActionResult((int)ActionResultDelete.ReportHasReportPackages, GetText(11551, "Rapporten är kopplad till rapportpaket"));

                if (!report.PayrollGroupReport.IsLoaded)
                    report.PayrollGroupReport.Load();
                if (report.PayrollGroupReport.Any(i => i.State == (int)SoeEntityState.Active))
                    return new ActionResult((int)ActionResultDelete.ReportHasReportPayrollGroups, GetText(11552, "Rapporten är kopplad till löneavtal"));

                if (!report.ChecklistHead.IsLoaded)
                    report.ChecklistHead.Load();
                if (report.ChecklistHead.Any(i => i.State == (int)SoeEntityState.Active))
                    return new ActionResult((int)ActionResultDelete.ReportHasReportChecklists, GetText(11553, "Rapporten är kopplad till checklistor"));

                // Customers
                if (entities.Customer.Any(c => c.ActorCompanyId == actorCompanyId && c.State == (int)SoeEntityState.Active && (c.AgreementTemplate == report.ReportId || c.BillingTemplate == report.ReportId || c.OrderTemplate == report.ReportId || c.OfferTemplate == report.ReportId)))
                    return new ActionResult((int)ActionResultDelete.ReportHasCustomers, GetText(7883, "Rapporten är kopplad till kunder"));

                return ChangeEntityState(entities, originalReport, SoeEntityState.Deleted, true);
            }
        }

        #region Help-methods

        private List<Report> FilterValidReports(List<Report> reports, List<ReportRolePermissionDTO> permissions, int? roleId, bool onlyOriginal, bool onlyStandard, bool onlyWithSelections, bool onlyShowInAccountingReports)
        {
            List<Report> validReports = new List<Report>();

            foreach (Report report in reports)
            {
                if (onlyShowInAccountingReports && !report.ShowInAccountingReports)
                    continue;
                if (!report.IsValid(onlyOriginal, onlyStandard, onlyWithSelections))
                    continue;
                if (!permissions.HasReportRolePermission(report.ReportId, roleId))
                    continue;

                validReports.Add(report);
            }

            return validReports;
        }

        private void SetReportProperties(List<Report> reports, int actorCompanyId, List<ReportRolePermissionDTO> permissions, bool loadSysReportTemplate = false, bool setTemplateTypeProperties = true, bool setIsSystemReport = false)
        {
            if (reports.IsNullOrEmpty())
                return;

            int langId = GetLangId();
            List<GenericType> termsSysReportTemplateType = base.GetTermGroupContent(TermGroup.SysReportTemplateType, langId: langId);
            List<GenericType> termsReportExportType = base.GetTermGroupContent(TermGroup.ReportExportType, langId: langId);
            foreach (Report report in reports)
            {
                if (loadSysReportTemplate)
                    SetReportSysReportTemplateTypeProperties(report, actorCompanyId);
                if (setTemplateTypeProperties)
                    SetReportSysReportTemplateTypeProperties(report, actorCompanyId);
                SetReportSysReportTypeProperties(report, termsSysReportTemplateType);
                SetReportRoleProperties(report, permissions);
                SetReportExportTypeProperties(report, termsReportExportType);
                SetReportSelectionProperties(report);
            }
            if (setIsSystemReport) SetReportIsSystemProperty(reports);
        }

        private void SetReportIsSystemProperty(List<Report> reports)
        {
            var templateIds = reports.Select(r => r.ReportTemplateId).ToHashSet();
            using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
            var dict = sysEntitiesReadOnly.SysReportTemplate
                .Where(t => templateIds.Contains(t.SysReportTemplateId))
                .Select(t => new { t.SysReportTemplateId, t.IsSystemReport })
                .ToDictionary(t => t.SysReportTemplateId, t => t.IsSystemReport);

            reports.ForEach(r => r.IsSystemReport = dict.ContainsKey(r.ReportTemplateId) ? dict.GetValue(r.ReportTemplateId) : false);
        }

        private void SetReportSysReportTemplateSettings(Report report)
        {
            SysReportTemplate sysReportTemplate = GetSysReportTemplate(report.ReportTemplateId, false, false, false, true);
            if (sysReportTemplate == null)
                return;

            report.ReportTemplateSettings = sysReportTemplate.SysReportTemplateSettings.ToList();
        }

        private void SetReportSysReportTemplateTypeProperties(Report report, int actorCompanyId)
        {
            SysReportTemplateType sysReportTemplateType = GetSysReportTemplateType(report, actorCompanyId);
            if (sysReportTemplateType == null)
                return;

            report.SysReportTemplateTypeId = sysReportTemplateType.SysReportTemplateTypeId;
            report.SysReportTemplateTypeSelectionType = sysReportTemplateType.SelectionType;
            report.SysReportTemplateTypeGroupMapping = sysReportTemplateType.GroupMapping;
            report.SysReportTemplateTypeModule = sysReportTemplateType.Module;
        }

        private void SetReportSysReportTypeProperties(Report report, List<GenericType> terms)
        {
            if (report == null)
                return;

            report.SysReportTypeName = terms?.FirstOrDefault(t => t.Id == report.SysReportTemplateTypeId)?.Name ?? string.Empty;
        }

        private void SetReportRoleProperties(Report report, List<ReportRolePermissionDTO> permissions)
        {
            if (report == null || permissions.IsNullOrEmpty())
                return;

            List<ReportRolePermissionDTO> permissionsForReport = permissions.Filter(report.ReportId);
            if (!permissionsForReport.IsNullOrEmpty())
            {
                List<string> roleNames = new List<string>();
                foreach (var permissionForReportByRole in permissionsForReport.GroupBy(r => r.RoleId))
                    roleNames.Add(RoleManager.GetRoleName(permissionForReportByRole.Key));
                report.RoleNames = roleNames.ToCommaSeparated();
            }
            else
            {
                report.RoleNames = GetText(4366, "Alla");
            }
        }

        private void SetReportExportTypeProperties(Report report, List<GenericType> terms)
        {
            if (report == null)
                return;

            report.ExportTypeName = terms.FirstOrDefault(t => t.Id == report.ExportType)?.Name;
        }

        public static void SetReportSelectionProperties(Report report)
        {
            if (report?.ReportSelection == null)
                return;

            report.ReportSelectionText = report.ReportSelection.ReportSelectionText;
            report.NameWithReportSelectionText = $"{report.Name} {report.ReportSelection.ReportSelectionText}";
        }

        #endregion

        #endregion

        #region ReportMenu 

        public List<ReportMenuDTO> GetReportsForMenu(int module, SoeReportType sysReportType, int actorCompanyId, int roleId, int userId, bool isSupportAdmin)
        {
            List<ReportMenuDTO> menuItems = new List<ReportMenuDTO>();
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            #region Init

            bool isTabFavorites = module == 0;
            bool isTabReports = sysReportType == SoeReportType.CrystalReport;
            bool isTabAnalysis = sysReportType == SoeReportType.Analysis;
            bool isTabQueue = module == -1;
            bool isModuleBilling = module == (int)SoeModule.Billing;
            bool isModuleEconomy = module == (int)SoeModule.Economy;
            bool isModuleTime = module == (int)SoeModule.Time;

            #endregion

            #region Which are migrated

            List<int> migratedSysReportTemplateTypeIds = new List<int>();

            if (isModuleEconomy || isTabFavorites)
            {
                if (isTabReports || isTabFavorites)
                {
                    migratedSysReportTemplateTypeIds.AddRange(new List<int>()
                    {
                        (int)SoeReportTemplateType.GeneralLedger,
                        (int)SoeReportTemplateType.VoucherList,
                        (int)SoeReportTemplateType.BalanceReport,
                        (int)SoeReportTemplateType.ResultReport,
                        (int)SoeReportTemplateType.ResultReportV2,
                        (int)SoeReportTemplateType.CustomerBalanceList,
                        (int)SoeReportTemplateType.SupplierBalanceList,
                        (int)SoeReportTemplateType.SupplierInvoiceJournal,
                        (int)SoeReportTemplateType.CustomerInvoiceJournal,
                        (int)SoeReportTemplateType.FixedAssets,
                        (int)SoeReportTemplateType.TaxAudit,
                        (int)SoeReportTemplateType.TaxAudit_FI,
                        (int)SoeReportTemplateType.SruReport,
                        (int)SoeReportTemplateType.CustomerPaymentJournal,
                        (int)SoeReportTemplateType.SupplierPaymentJournal,
                        (int)SoeReportTemplateType.PeriodAccountingRegulationsReport,
                        (int)SoeReportTemplateType.PeriodAccountingForecastReport,
                        (int)SoeReportTemplateType.InterestRateCalculation,
                        (int)SoeReportTemplateType.BillingInvoiceReminder,
                        (int)SoeReportTemplateType.SymbrioEdiSupplierInvoice,
                        (int)SoeReportTemplateType.FinvoiceEdiSupplierInvoice,
                        (int)SoeReportTemplateType.IOCustomerInvoice,
                        (int)SoeReportTemplateType.IOVoucher
                    }
                );
                }

                if (isTabAnalysis || isTabFavorites)
                {
                    migratedSysReportTemplateTypeIds.AddRange(new List<int>()
                    {
                        (int)SoeReportTemplateType.SupplierAnalysis,
                        (int)SoeReportTemplateType.CustomerAnalysis,
                        (int)SoeReportTemplateType.EmployeeSkillAnalysis,
                        (int)SoeReportTemplateType.ShiftTypeSkillAnalysis,
                        (int)SoeReportTemplateType.EmployeeEndReasonsAnalysis,
                        (int)SoeReportTemplateType.EmployeeFixedPayLinesAnalysis,
                        (int)SoeReportTemplateType.PayrollProductsAnalysis,
                        (int)SoeReportTemplateType.EmploymentHistoryAnalysis,
                        (int)SoeReportTemplateType.EmployeeSalaryDistressAnalysis,
                        (int)SoeReportTemplateType.OrderAnalysis,
                        (int)SoeReportTemplateType.InvoiceAnalysis,
                        (int)SoeReportTemplateType.InvoiceProductAnalysis,
                        (int)SoeReportTemplateType.EmployeeSalaryUnionFeesAnalysis,
                        (int)SoeReportTemplateType.InventoryAnalysis,
                        (int)SoeReportTemplateType.DepreciationAnalysis,
                    });
                }
            }

            if (isModuleBilling || isTabFavorites)
            {
                if (isTabReports || isTabFavorites)
                {
                    migratedSysReportTemplateTypeIds.AddRange(new List<int>()
                    {
                        (int)SoeReportTemplateType.ProjectTimeReport,
                        (int)SoeReportTemplateType.ProjectStatisticsReport,
                        (int)SoeReportTemplateType.ProjectTransactionsReport,
                        (int)SoeReportTemplateType.TimeProjectReport,
                        (int)SoeReportTemplateType.BillingContract,
                        (int)SoeReportTemplateType.BillingOffer,
                        (int)SoeReportTemplateType.BillingOrder,
                        (int)SoeReportTemplateType.BillingInvoice,
                        (int)SoeReportTemplateType.BillingInvoiceInterest,
                        (int)SoeReportTemplateType.BillingInvoiceReminder,
                        (int)SoeReportTemplateType.BillingOrderOverview,
                        (int)SoeReportTemplateType.HousholdTaxDeduction,
                        (int)SoeReportTemplateType.StockSaldoListReport,
                        (int)SoeReportTemplateType.StockInventoryReport,
                        (int)SoeReportTemplateType.StockTransactionListReport,
                        (int)SoeReportTemplateType.OriginStatisticsReport,
                        (int)SoeReportTemplateType.PurchaseOrder,
                        (int)SoeReportTemplateType.ExpenseReport,
                        (int)SoeReportTemplateType.OrderChecklistReport,
                        (int)SoeReportTemplateType.ProductListReport,
                        (int)SoeReportTemplateType.BillingStatisticsReport,
                        (int)SoeReportTemplateType.OrderContractChange,
                        (int)SoeReportTemplateType.TaxReductionBalanceListReport,
                    }
                );
                }

                if (isTabAnalysis || isTabFavorites)
                {
                    migratedSysReportTemplateTypeIds.AddRange(new List<int>()
                    {
                        (int)SoeReportTemplateType.InvoiceProductAnalysis,
                        (int)SoeReportTemplateType.EmploymentHistoryAnalysis,
                        (int)SoeReportTemplateType.OrderAnalysis,
                        (int)SoeReportTemplateType.InvoiceAnalysis,
                        (int)SoeReportTemplateType.InvoiceProductUnitConvertAnalysis,
                    });
                }
            }

            if (isModuleTime || isTabFavorites)
            {
                if (isTabReports || isTabFavorites)
                {
                    migratedSysReportTemplateTypeIds.AddRange(new List<int>()
                    {
                        (int)SoeReportTemplateType.TimeEmploymentContract,
                        (int)SoeReportTemplateType.TimeEmploymentDynamicContract,
                        (int)SoeReportTemplateType.EmployeeListReport,
                        (int)SoeReportTemplateType.EmployeeVacationDebtReport,
                        (int)SoeReportTemplateType.PayrollProductReport,
                        (int)SoeReportTemplateType.PayrollTransactionStatisticsReport,
                        (int)SoeReportTemplateType.TimeAbsenceReport,
                        (int)SoeReportTemplateType.TimeAccumulatorReport,
                        (int)SoeReportTemplateType.TimeAccumulatorDetailedReport,
                        (int)SoeReportTemplateType.TimeCategorySchedule,
                        (int)SoeReportTemplateType.TimeCategoryStatistics,
                        (int)SoeReportTemplateType.TimeEmployeeSchedule,
                        (int)SoeReportTemplateType.TimeEmployeeScheduleSmallReport,
                        (int)SoeReportTemplateType.TimeEmployeeTemplateSchedule,
                        (int)SoeReportTemplateType.TimeEmploymentContract,
                        (int)SoeReportTemplateType.TimeMonthlyReport,
                        (int)SoeReportTemplateType.TimePayrollTransactionReport,
                        (int)SoeReportTemplateType.TimePayrollTransactionSmallReport,
                        (int)SoeReportTemplateType.TimeSalaryControlInfoReport,
                        (int)SoeReportTemplateType.TimeSalarySpecificationReport,
                        (int)SoeReportTemplateType.TimeScheduleBlockHistory,
                        (int)SoeReportTemplateType.TimeScheduleCopyReport,
                        (int)SoeReportTemplateType.TimeScheduleTasksAndDeliverysReport,
                        (int)SoeReportTemplateType.TimeStampEntryReport,
                        (int)SoeReportTemplateType.TimeEmployeeLineSchedule,
                        (int)SoeReportTemplateType.CollectumReport,
                        (int)SoeReportTemplateType.ForaReport,
                        (int)SoeReportTemplateType.ForaMonthlyReport,
                        (int)SoeReportTemplateType.KPAReport,
                        (int)SoeReportTemplateType.KPADirektReport,
                        (int)SoeReportTemplateType.Bygglosen,
                        (int)SoeReportTemplateType.Kronofogden,
                        (int)SoeReportTemplateType.SCB_KSJUReport,
                        (int)SoeReportTemplateType.SCB_KLPReport,
                        (int)SoeReportTemplateType.SCB_KSPReport,
                        (int)SoeReportTemplateType.SCB_SLPReport,
                        (int)SoeReportTemplateType.SNReport,
                        (int)SoeReportTemplateType.KU10Report,
                        (int)SoeReportTemplateType.SKDReport,
                        (int)SoeReportTemplateType.EmployeeVacationInformationReport,
                        (int)SoeReportTemplateType.EmployeeTimePeriodReport,
                        (int)SoeReportTemplateType.CertificateOfEmploymentReport,
                        (int)SoeReportTemplateType.PayrollPeriodWarningCheck,
                        (int)SoeReportTemplateType.PayrollAccountingReport,
                        (int)SoeReportTemplateType.PayrollVacationAccountingReport,
                        (int)SoeReportTemplateType.PayrollSlip,
                        (int)SoeReportTemplateType.RoleReport,
                        (int)SoeReportTemplateType.AgdEmployeeReport,
                        (int)SoeReportTemplateType.FolksamGTP,
                        (int)SoeReportTemplateType.IFMetall,
                        (int)SoeReportTemplateType.SkandiaPension,
                        (int)SoeReportTemplateType.SEF,
                        (int)SoeReportTemplateType.AgiAbsence,
                    });
                }
                if (isTabAnalysis || isTabFavorites)
                {
                    migratedSysReportTemplateTypeIds.AddRange(new List<int>()
                    {
                        (int)SoeReportTemplateType.TimeTransactionAnalysis,
                        (int)SoeReportTemplateType.PayrollTransactionAnalysis,
                        (int)SoeReportTemplateType.EmployeeAnalysis,
                        (int)SoeReportTemplateType.ScheduleAnalysis,
                        (int)SoeReportTemplateType.EmployeeDateAnalysis,
                        (int)SoeReportTemplateType.TimeStampEntryAnalysis,
                        (int)SoeReportTemplateType.UserAnalysis,
                        (int)SoeReportTemplateType.SupplierAnalysis,
                        (int)SoeReportTemplateType.CustomerAnalysis,
                        (int)SoeReportTemplateType.InvoiceProductAnalysis,
                        (int)SoeReportTemplateType.Generic,
                        (int)SoeReportTemplateType.StaffingneedsFrequencyAnalysis,
                        (int)SoeReportTemplateType.EmployeeSkillAnalysis,
                        (int)SoeReportTemplateType.OrganisationHrAnalysis,
                        (int)SoeReportTemplateType.ShiftTypeSkillAnalysis,
                        (int)SoeReportTemplateType.EmployeeEndReasonsAnalysis,
                        (int)SoeReportTemplateType.EmployeeSalaryAnalysis,
                        (int)SoeReportTemplateType.EmployeeTimePeriodAnalysis,
                        (int)SoeReportTemplateType.StaffingStatisticsAnalysis,
                        (int)SoeReportTemplateType.AggregatedTimeStatisticsAnalysis,
                        (int)SoeReportTemplateType.EmployeeMeetingAnalysis,
                        (int)SoeReportTemplateType.TimeScheduledSummary,
                        (int)SoeReportTemplateType.EmployeeExperienceAnalysis,
                        (int)SoeReportTemplateType.EmployeeDocumentAnalysis,
                        (int)SoeReportTemplateType.EmployeeAccountAnalysis,
                        (int)SoeReportTemplateType.ReportStatisticsAnalysis,
                        (int)SoeReportTemplateType.SoftOneStatusResultAnalysis,
                        (int)SoeReportTemplateType.SoftOneStatusEventAnalysis,
                        (int)SoeReportTemplateType.SoftOneStatusUpTimeAnalysis,
                        (int)SoeReportTemplateType.EmployeeFixedPayLinesAnalysis,
                        (int)SoeReportTemplateType.PayrollProductsAnalysis,
                        (int)SoeReportTemplateType.EmploymentHistoryAnalysis,
                        (int)SoeReportTemplateType.EmployeeSalaryDistressAnalysis,
                        (int)SoeReportTemplateType.OrderAnalysis,
                        (int)SoeReportTemplateType.InvoiceProductAnalysis,
                        (int)SoeReportTemplateType.InvoiceAnalysis,
                        (int)SoeReportTemplateType.EmployeeSalaryUnionFeesAnalysis,
                        (int)SoeReportTemplateType.EmploymentDaysAnalysis,
                        (int)SoeReportTemplateType.AccountHierachyAnalysis,
                        (int)SoeReportTemplateType.AnnualProgressAnalysis,
                        (int)SoeReportTemplateType.LongtermAbsenceAnalysis,
                        (int)SoeReportTemplateType.VacationBalanceAnalysis,
                        (int)SoeReportTemplateType.ShiftQueueAnalysis,
                        (int)SoeReportTemplateType.ShiftHistoryAnalysis,
                        (int)SoeReportTemplateType.ShiftRequestAnalysis,
                        (int)SoeReportTemplateType.AbsenceRequestAnalysis,
                        (int)SoeReportTemplateType.VismaPayrollChangesAnalysis,
                        (int)SoeReportTemplateType.TimeStampHistoryAnalysis,
                        (int)SoeReportTemplateType.VerticalTimeTrackerAnalysis,
                        (int)SoeReportTemplateType.HorizontalTimeTrackerAnalysis,
                        (int)SoeReportTemplateType.LicenseInformationAnalysis,
                        (int)SoeReportTemplateType.AgiAbsenceAnalysis,
                        (int)SoeReportTemplateType.EmployeeChildAnalysis,
                        (int)SoeReportTemplateType.EmployeePayrollAdditionsAnalysis,
                        (int)SoeReportTemplateType.AnnualLeaveTransactionAnalysis,
                        (int)SoeReportTemplateType.SwapShiftAnalysis,
                    });
                }
            }

            #endregion

            #region Get templates (sys and comp)

            int? sysCountryId = CountryCurrencyManager.GetSysCountryIdFromCompany(actorCompanyId, defaultSysCountryId: (int)TermGroup_Country.SE);

            using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
            List<SysLinkTable> allSysLinks = (from c in sysEntitiesReadOnly.SysLinkTable
                                              where c.SysLinkTableRecordType == (int)SysLinkTableRecordType.LinkSysReportTemplateToCountryId &&
                                              c.SysLinkTableIntegerValueType == (int)SysLinkTableIntegerValueType.SysCountryId
                                              select c).ToList();

            var sysTemplates = (from t in sysEntitiesReadOnly.SysReportTemplate.Include("SysReportTemplateType")
                                where (isTabFavorites || t.SysReportTemplateType.Module == module) &&
                                (sysReportType == SoeReportType.Unknown || t.SysReportType.SysReportTypeId == (int)sysReportType) &&
                                migratedSysReportTemplateTypeIds.Contains(t.SysReportTemplateTypeId)
                                select new
                                {
                                    t.SysReportTemplateType.Module,
                                    t.SysReportTypeId,
                                    t.SysReportTemplateId,
                                    t.SysReportTemplateTypeId,
                                    t.ReportNr,
                                    t.Name,
                                    t.IsSystemReport,
                                    t.SysCountryId
                                }).ToList();

            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.ReportTemplate.NoTracking();
            var companyTemplates = (from t in entities.ReportTemplate
                                    where (isTabFavorites || t.Module == module) &&
                                    (sysReportType == SoeReportType.Unknown || t.SysReportTypeId == (int)sysReportType) &&
                                    t.Company.ActorCompanyId == actorCompanyId &&
                                    migratedSysReportTemplateTypeIds.Contains(t.SysTemplateTypeId)
                                    select new
                                    {
                                        t.Module,
                                        t.SysReportTypeId,
                                        t.ReportTemplateId,
                                        t.SysTemplateTypeId,
                                        t.ReportNr,
                                        t.Name
                                    }).ToList();

            var templates = sysTemplates.Select(t => new
            {
                t.Module,
                SysReportType = (SoeReportType)t.SysReportTypeId,
                ReportTemplateId = t.SysReportTemplateId,
                t.ReportNr,
                t.Name,
                ReportTemplateTypeId = t.SysReportTemplateTypeId,
                IsCompanyTemplate = false,
                t.IsSystemReport,
            }).Concat(companyTemplates.Select(t => new
            {
                t.Module,
                SysReportType = (SoeReportType)t.SysReportTypeId,
                t.ReportTemplateId,
                t.ReportNr,
                t.Name,
                ReportTemplateTypeId = t.SysTemplateTypeId,
                IsCompanyTemplate = true,
                IsSystemReport = false,
            })).ToList();

            #endregion

            #region Get favorites

            entitiesReadOnly.UserReportFavorite.NoTracking();
            List<UserReportFavorite> favorites = (from r in entitiesReadOnly.UserReportFavorite.Include("Report")
                                                  where r.UserId == userId &&
                                                  r.ActorCompanyId == actorCompanyId
                                                  select r).ToList();

            #endregion

            List<GenericType> groupNames = GetTermGroupContent(TermGroup.SysReportTemplateTypeGroup);
            bool isSysAdminOrSupport = isSupportAdmin || RoleManager.GetRole(roleId)?.TermId == (int)TermGroup_Roles.Systemadmin;

            if (isTabFavorites)
            {
                #region Favorites

                List<SysReportTemplateType> sysReportTemplateTypes = GetSysReportTemplateTypesFromCache();

                foreach (UserReportFavorite favorite in favorites)
                {
                    var template = templates.FirstOrDefault(t => t.ReportTemplateId == favorite.Report.ReportTemplateId && t.Module == favorite.Report.Module && t.IsCompanyTemplate != favorite.Report.Standard);
                    if (template == null)
                    {
                        template = templates.FirstOrDefault(t => t.ReportTemplateId == favorite.Report.ReportTemplateId && t.IsCompanyTemplate != favorite.Report.Standard);
                        if (template == null)
                        {
                            template = templates.FirstOrDefault(t => t.ReportTemplateId == favorite.Report.ReportTemplateId);
                            if (template == null)
                                continue;
                        }
                    }

                    SysReportTemplateType templateType = sysReportTemplateTypes.FirstOrDefault(i => i.Module == template.Module && i.SysReportTemplateTypeId == template.ReportTemplateTypeId);
                    if (templateType == null)
                        continue;

                    menuItems.Add(new ReportMenuDTO()
                    {
                        ReportId = favorite.Report.ReportId,
                        SysReportType = template.SysReportType,
                        SysReportTemplateTypeId = template.ReportTemplateTypeId,
                        ReportTemplateId = favorite.Report.ReportTemplateId,
                        ReportNr = favorite.Report.ReportNr,
                        Name = favorite.Name,
                        IsSystemReport = template.IsSystemReport,
                        Active = true,
                        IsCompanyTemplate = true,
                        GroupName = groupNames.FirstOrDefault(i => i.Id == templateType.Group)?.Name ?? string.Empty,
                        GroupOrder = GetSysReportTemplateTypeGroupSortOrder(templateType.Group),
                        Module = (SoeModule)templateType.Module,
                        IsFavorite = true,
                        Description = favorite.Report.Description
                    });
                }

                #endregion
            }
            else
            {
                #region Other

                if (templates.Any())
                {
                    List<SysReportTemplateType> sysReportTemplateTypes = GetSysReportTemplateTypesFromCache(module);
                    List<Report> reports = GetReports(actorCompanyId, null, module: module, onlyOriginal: true, loadRolePermission: isSysAdminOrSupport, setTemplateTypeProperties: false);

                    foreach (var template in templates)
                    {
                        // Filter system reports on country/language
                        if (!template.IsCompanyTemplate)
                        {
                            List<SysLinkTable> sysLinks = allSysLinks.Where(s => s.SysLinkTableKeyItemId == template.ReportTemplateId).ToList();
                            if (sysLinks.Any() && !sysLinks.Any(i => i.SysLinkTableIntegerValue == sysCountryId.Value))
                                continue;
                        }

                        SysReportTemplateType templateType = sysReportTemplateTypes.FirstOrDefault(i => i.SysReportTemplateTypeId == template.ReportTemplateTypeId);
                        if (templateType == null)
                            continue;

                        string groupName = groupNames.FirstOrDefault(i => i.Id == templateType.Group)?.Name ?? string.Empty;

                        List<Report> reportsForTemplateType = reports.Where(r => template.ReportTemplateId == r.ReportTemplateId && template.IsCompanyTemplate == !r.Standard).ToList();
                        if (reportsForTemplateType.Any())
                        {
                            menuItems.AddRange(reportsForTemplateType.Select(report => new ReportMenuDTO
                            {
                                ReportId = report.ReportId,
                                SysReportType = template.SysReportType,
                                SysReportTemplateTypeId = template.ReportTemplateTypeId,
                                ReportTemplateId = template.ReportTemplateId,
                                ReportNr = report.ReportNr,
                                Name = report.Name,
                                Active = true,
                                IsCompanyTemplate = template.IsCompanyTemplate,
                                IsStandard = report.Standard,
                                IsSystemReport = template.IsSystemReport,
                                GroupName = groupName,
                                GroupOrder = GetSysReportTemplateTypeGroupSortOrder(templateType.Group),
                                Module = (SoeModule)templateType.Module,
                                IsFavorite = favorites.Any(f => f.ReportId == report.ReportId),
                                NoRolesSpecified = isSysAdminOrSupport && report.ReportRolePermission.IsNullOrEmpty(),
                                Description = report.Description
                            }));
                        }
                        else
                        {
                            string reportName = sysReportType == SoeReportType.Analysis ? GetText(template.ReportTemplateTypeId, (int)TermGroup.SysReportTemplateType) : string.Empty;

                            menuItems.Add(new ReportMenuDTO
                            {
                                ReportId = null,
                                SysReportType = template.SysReportType,
                                SysReportTemplateTypeId = template.ReportTemplateTypeId,
                                ReportTemplateId = template.ReportTemplateId,
                                ReportNr = template.ReportNr,
                                Name = reportName.IsNullOrEmpty() ? template.Name : reportName,
                                Active = false,
                                IsSystemReport = template.IsSystemReport,
                                IsCompanyTemplate = template.IsCompanyTemplate,
                                IsStandard = false,
                                GroupName = groupName,
                                GroupOrder = GetSysReportTemplateTypeGroupSortOrder(templateType.Group),
                                Module = (SoeModule)templateType.Module,
                                IsFavorite = false,
                                NoRolesSpecified = false,
                            });
                        }
                    }
                }

                #endregion
            }

            menuItems = menuItems.OrderBy(i => i.GroupOrder).ThenBy(i => i.Name).ToList();

            #region Printable or not

            foreach (ReportMenuDTO menuItem in menuItems)
            {
                switch (menuItem.SysReportTemplateTypeId)
                {
                    case (int)SoeReportTemplateType.TimeEmploymentContract:
                    case (int)SoeReportTemplateType.TimeSalaryControlInfoReport:
                    case (int)SoeReportTemplateType.TimeSalarySpecificationReport:
                    case (int)SoeReportTemplateType.TimeScheduleTasksAndDeliverysReport:
                    case (int)SoeReportTemplateType.BillingInvoiceReminder:
                    case (int)SoeReportTemplateType.SymbrioEdiSupplierInvoice:
                    case (int)SoeReportTemplateType.FinvoiceEdiSupplierInvoice:
                    case (int)SoeReportTemplateType.IOCustomerInvoice:
                    case (int)SoeReportTemplateType.IOVoucher:
                    case (int)SoeReportTemplateType.InterestRateCalculation:
                    case (int)SoeReportTemplateType.OrderChecklistReport:
                    case (int)SoeReportTemplateType.TimeProjectReport:
                    case (int)SoeReportTemplateType.ExpenseReport:
                    case (int)SoeReportTemplateType.Generic:
                        menuItem.PrintableFromMenu = false;
                        break;
                    default:
                        menuItem.PrintableFromMenu = true;
                        break;
                }
            }

            #endregion

            #region Permissions

            List<ReportMenuDTO> validatedMenuItems = new List<ReportMenuDTO>();
            List<ReportRolePermissionDTO> permissions = GetReportRolePermissionsFromCache(actorCompanyId);

            Feature reportPermission = Feature.None;
            if (isModuleBilling)
                reportPermission = isTabReports ? Feature.Billing_Distribution_Reports_Edit : Feature.Billing_Analysis;
            else if (isModuleEconomy)
                reportPermission = isTabReports ? Feature.Economy_Distribution_Reports_Edit : Feature.Economy_Analysis;
            else if (isModuleTime)
                reportPermission = isTabReports ? Feature.Time_Distribution_Reports_Edit : Feature.Time_Analysis;

            bool hasEditPermission = FeatureManager.HasRolePermission(reportPermission, Permission.Modify, roleId, actorCompanyId);

            foreach (ReportMenuDTO item in menuItems)
            {
                if (item.ReportId.HasValue)
                {
                    if (permissions.HasReportRolePermission(item.ReportId.Value, roleId))
                    {
                        validatedMenuItems.Add(item);
                    }
                    else if (module > 0 && FeatureManager.HasRolePermission(reportPermission, Permission.Modify, roleId, actorCompanyId))
                    {
                        item.NoPrintPermission = true;
                        validatedMenuItems.Add(item);
                    }
                }
                else
                {
                    if (hasEditPermission)
                        validatedMenuItems.Add(item);
                }
            }

            #endregion

            return validatedMenuItems;
        }

        public ReportMenuDTO GetPrintedReportForMenu(int reportPrintoutId, int actorCompanyId)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            entitiesReadOnly.ReportPrintout.NoTracking();
            ReportPrintoutDTO printout = GetReportPrintoutWithoutData(entitiesReadOnly, reportPrintoutId, actorCompanyId);
            if (printout == null)
                return null;

            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Report.NoTracking();
            ReportMenuDTO dto = (from report in entities.Report
                                 where report.ActorCompanyId == actorCompanyId &&
                                 report.ReportId == printout.ReportId &&
                                 report.State == (int)SoeEntityState.Active
                                 select new ReportMenuDTO()
                                 {
                                     IsStandard = report.Standard,
                                     ReportId = report.ReportId,
                                     ReportTemplateId = report.ReportTemplateId,
                                     Module = (SoeModule)report.Module,
                                     Name = printout.ReportName,
                                 }).FirstOrDefault();

            if (dto == null)
                return null;

            // Set SysReportType
            if (dto.IsStandard)
            {
                using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
                SysReportTemplate template = sysEntitiesReadOnly.SysReportTemplate.FirstOrDefault(t => t.SysReportTemplateId == dto.ReportTemplateId);
                dto.SysReportType = template != null ? (SoeReportType)template.SysReportTypeId : SoeReportType.CrystalReport;
            }
            else
            {
                ReportTemplate template = entitiesReadOnly.ReportTemplate.FirstOrDefault(t => t.ReportTemplateId == dto.ReportTemplateId);
                dto.SysReportType = template != null ? (SoeReportType)template.SysReportTypeId : SoeReportType.CrystalReport;
            }

            dto.SysReportTemplateTypeId = printout.SysReportTemplateTypeId.Value;
            dto.Active = true;
            dto.PrintableFromMenu = true;

            if (dto.Module == SoeModule.None && dto.SysReportTemplateTypeId > 0)
            {
                using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
                dto.Module = (SoeModule)sysEntitiesReadOnly.SysReportTemplateType.Where(t => t.SysReportTemplateTypeId == dto.SysReportTemplateTypeId).Select(x => x.Module).FirstOrDefault();
            }

            if (!HasPermissionToPrintReport(entitiesReadOnly, dto.ReportId, actorCompanyId, base.RoleId) || (printout.SysReportTemplateTypeId.HasValue && printout.SysReportTemplateTypeId.Value == (int)SoeReportTemplateType.PayrollSlip))
                dto.NoPrintPermission = true;

            return dto;
        }

        private bool HasPermissionToPrintReport(CompEntities entities, int? reportId, int actorCompanyId, int roleId)
        {
            if (!reportId.HasValue)
                return false;
            List<ReportRolePermissionDTO> permissions = GetReportRolePermissionsFromCache(entities, CacheConfig.Company(actorCompanyId, 60));
            return permissions.HasReportRolePermission(reportId.Value, roleId);
        }

        public string GetPrintedXMLForMenu(int reportPrintoutId, int actorCompanyId)
        {
            using var entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            entitiesReadOnly.ReportPrintout.NoTracking();
            var xmlCompressed = (from p in entitiesReadOnly.ReportPrintout
                                 where p.ReportPrintoutId == reportPrintoutId &&
                                 p.ActorCompanyId == actorCompanyId &&
                                 p.SysReportTemplateTypeId.HasValue
                                 select p.XMLCompressed).FirstOrDefault();

            string xml = string.Empty;
            if (xmlCompressed != null)
            {
                xml = ZipUtility.UnzipString(xmlCompressed);
            }

            if (string.IsNullOrEmpty(xml))
            {
                xml = (from p in entitiesReadOnly.ReportPrintout
                       where p.ReportPrintoutId == reportPrintoutId &&
                       p.ActorCompanyId == actorCompanyId &&
                       p.SysReportTemplateTypeId.HasValue
                       select p.XML).FirstOrDefault();
            }

            if (!string.IsNullOrEmpty(xml))
            {
                try
                {
                    var reportTemplateType = (from p in entitiesReadOnly.ReportPrintout
                                              where p.ReportPrintoutId == reportPrintoutId &&
                                              p.ActorCompanyId == actorCompanyId &&
                                              p.SysReportTemplateTypeId.HasValue
                                              select p.SysReportTemplateTypeId).FirstOrDefault();

                    if (reportTemplateType.HasValue)
                    {
                        var ds = ReportGenManager.CreateDataSet(XDocument.Parse(xml), (SoeReportTemplateType)reportTemplateType.Value);
                        string tempPath = ConfigSettings.SOE_SERVER_DIR_TEMP_PHYSICAL + Guid.NewGuid();
                        ds.WriteXml(tempPath, XmlWriteMode.WriteSchema);
                        xml = File.ReadAllText(tempPath);
                        File.Delete(tempPath);
                        return xml;
                    }
                }
                catch (Exception ex)
                {
                    LogError(ex, log);
                    return xml;
                }
            }

            return xml;
        }

        public List<ReportJobStatusDTO> GetReportGenerationQueue(int userId, int actorCompanyId, List<int> reportPrintoutIds = null, bool showDetails = false)
        {
            List<ReportJobStatusDTO> queue = new List<ReportJobStatusDTO>();

            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.ReportPrintout.NoTracking();
            IQueryable<ReportJobStatus> query = (from p in entities.ReportPrintout
                                                 where p.UserId == userId &&
                                                       p.ActorCompanyId == actorCompanyId &&
                                                       p.Status != (int)TermGroup_ReportPrintoutStatus.Cleaned &&
                                                       p.Status != (int)TermGroup_ReportPrintoutStatus.Internal &&
                                                       p.Status != (int)TermGroup_ReportPrintoutStatus.DeletedByUser
                                                 orderby p.Created descending
                                                 select new ReportJobStatus()
                                                 {
                                                     ReportPrintoutId = p.ReportPrintoutId,
                                                     ReportName = p.ReportName,
                                                     DeliveredTime = p.DeliveredTime,
                                                     ResultMessage = p.ResultMessage,
                                                     ResultMessageDetails = p.ResultMessageDetails,
                                                     Created = p.Created,
                                                     Status = p.Status,
                                                     ExportType = (TermGroup_ReportExportType)p.ExportType,
                                                     SysReportTemplateTypeId = (SoeReportTemplateType)p.SysReportTemplateTypeId
                                                 });

            if (!reportPrintoutIds.IsNullOrEmpty())
                query = query.Where(p => reportPrintoutIds.Contains(p.ReportPrintoutId));

            List<ReportJobStatus> reportPrintouts = query.ToList();

            foreach (ReportJobStatus reportPrintout in reportPrintouts)
            {
                //Hide from queue
                if (reportPrintout.ExportType == TermGroup_ReportExportType.NoExport)
                    continue;

                queue.Add(new ReportJobStatusDTO()
                {
                    Name = reportPrintout.ReportName,
                    ReportPrintoutId = reportPrintout.ReportPrintoutId,
                    PrintoutDelivered = reportPrintout.DeliveredTime,
                    PrintoutRequested = reportPrintout.Created,
                    PrintoutStatus = (TermGroup_ReportPrintoutStatus)reportPrintout.Status,
                    PrintoutErrorMessage = GetReportPrintoutErrorMessage((SoeReportDataResultMessage)reportPrintout.ResultMessage, reportPrintout.ResultMessageDetails, showDetails),
                    ExportType = reportPrintout.ExportType,
                    SysReportTemplateTypeId = reportPrintout.SysReportTemplateTypeId
                });
            }

            if (reportPrintouts.Any(a => a.Status == (int)TermGroup_ReportPrintoutStatus.Queued) && !reportPrintouts.Any(a => a.Status == (int)TermGroup_ReportPrintoutStatus.Ordered))
                ReportDataManager.TryStartPrintReportDTO(null, actorCompanyId, userId, null, 0);

            return queue;
        }

        public ReportPrintoutDTO GetMatrixGridResult(int userId, int actorCompanyId, int reportPrintoutId)
        {
            ReportPrintout reportPrintout = GetReportPrintout(reportPrintoutId, actorCompanyId);
            ReportPrintoutDTO dto = reportPrintout.ToDTO(true, false);
            if (dto.Data != null)
            {
                dto.Json = JsonConvert.DeserializeObject(Encoding.UTF8.GetString(dto.Data)).ToString();
                dto.Data = null;
            }

            return dto;
        }

        public List<MatrixLayoutColumn> GetMatrixLayoutColumns(int sysReportTemplateTypeId, int actorCompanyId, SoeModule module)
        {
            switch (module)
            {
                case SoeModule.Economy:
                    return EconomyMatrixDataManager.GetMatrixLayoutColumns((SoeReportTemplateType)sysReportTemplateTypeId);
                case SoeModule.Time:
                    //TODO: Manage should be its own module
                    if ((SoeReportTemplateType)sysReportTemplateTypeId == SoeReportTemplateType.UserAnalysis ||
                        (SoeReportTemplateType)sysReportTemplateTypeId == SoeReportTemplateType.ReportStatisticsAnalysis ||
                        (SoeReportTemplateType)sysReportTemplateTypeId == SoeReportTemplateType.OrganisationHrAnalysis ||
                        (SoeReportTemplateType)sysReportTemplateTypeId == SoeReportTemplateType.SoftOneStatusResultAnalysis ||
                        (SoeReportTemplateType)sysReportTemplateTypeId == SoeReportTemplateType.SoftOneStatusEventAnalysis ||
                        (SoeReportTemplateType)sysReportTemplateTypeId == SoeReportTemplateType.SoftOneStatusUpTimeAnalysis ||
                        (SoeReportTemplateType)sysReportTemplateTypeId == SoeReportTemplateType.LicenseInformationAnalysis ||
                        (SoeReportTemplateType)sysReportTemplateTypeId == SoeReportTemplateType.EmploymentHistoryAnalysis ||
                        (SoeReportTemplateType)sysReportTemplateTypeId == SoeReportTemplateType.AccountHierachyAnalysis ||
                        (SoeReportTemplateType)sysReportTemplateTypeId == SoeReportTemplateType.OrderAnalysis)
                        return ManageMatrixDataManager.GetMatrixLayoutColumns((SoeReportTemplateType)sysReportTemplateTypeId);
                    else
                        return TimeMatrixDataManager.GetMatrixLayoutColumns((SoeReportTemplateType)sysReportTemplateTypeId);
                case SoeModule.Billing:
                    return BillingMatrixDataManager.GetMatrixLayoutColumns((SoeReportTemplateType)sysReportTemplateTypeId);
                default:
                    return new List<MatrixLayoutColumn>();
            }
        }

        public List<Insight> GetInsights(int sysReportTemplateTypeId, SoeModule module, int actorCompanyId, int roleId)
        {
            switch (module)
            {
                case SoeModule.Economy:
                    return EconomyMatrixDataManager.GetInsights((SoeReportTemplateType)sysReportTemplateTypeId, actorCompanyId, roleId);
                case SoeModule.Time:
                    //TODO: Manage should be its own module
                    if ((SoeReportTemplateType)sysReportTemplateTypeId == SoeReportTemplateType.UserAnalysis ||
                        (SoeReportTemplateType)sysReportTemplateTypeId == SoeReportTemplateType.ReportStatisticsAnalysis ||
                        (SoeReportTemplateType)sysReportTemplateTypeId == SoeReportTemplateType.OrganisationHrAnalysis)
                        return ManageMatrixDataManager.GetInsights((SoeReportTemplateType)sysReportTemplateTypeId, actorCompanyId, roleId);
                    else
                        return TimeMatrixDataManager.GetInsights((SoeReportTemplateType)sysReportTemplateTypeId, actorCompanyId, roleId);
                case SoeModule.Billing:
                    return BillingMatrixDataManager.GetInsights((SoeReportTemplateType)sysReportTemplateTypeId, actorCompanyId, roleId);
                default:
                    return new List<Insight>();
            }
        }

        public List<SelectablePayrollMonthYearDTO> GetReportPayrollYears(int actorCompanyId)
        {
            List<SelectablePayrollMonthYearDTO> selectableYears = new List<SelectablePayrollMonthYearDTO>();

            List<TimePeriodHeadDTO> timePeriodHeads = TimePeriodManager.GetTimePeriodHeadsIncludingPeriodsForType(actorCompanyId, TermGroup_TimePeriodType.Payroll).ToDTOs(true).ToList();
            List<TimePeriodDTO> timePeriods = new List<TimePeriodDTO>();
            timePeriodHeads.ToList().ForEach(x => timePeriods.AddRange(x.TimePeriods));
            timePeriods = timePeriods.Where(x => x.PaymentDate.HasValue).OrderByDescending(x => x.PaymentDate).ToList();

            int id = 0;
            foreach (IGrouping<int, TimePeriodDTO> periodsByYear in timePeriods.GroupBy(x => x.PaymentDate.Value.Year))
            {
                id++;
                selectableYears.Add(new SelectablePayrollMonthYearDTO
                {
                    Id = id,
                    DisplayName = periodsByYear.Key.ToString(),
                    TimePeriodIds = periodsByYear.Select(x => x.TimePeriodId).ToList(),
                });

            }
            return selectableYears;
        }

        public List<SelectablePayrollMonthYearDTO> GetReportPayrollMonths(int actorCompanyId)
        {
            List<SelectablePayrollMonthYearDTO> selectableMonths = new List<SelectablePayrollMonthYearDTO>();

            List<TimePeriodHeadDTO> timePeriodHeads = TimePeriodManager.GetTimePeriodHeadsIncludingPeriodsForType(actorCompanyId, TermGroup_TimePeriodType.Payroll).ToDTOs(true).ToList();
            List<TimePeriodDTO> timePeriods = new List<TimePeriodDTO>();
            timePeriodHeads.ToList().ForEach(x => timePeriods.AddRange(x.TimePeriods));
            timePeriods = timePeriods.Where(x => x.PaymentDate.HasValue).OrderByDescending(x => x.PaymentDate).ToList();
            int id = 0;
            foreach (IGrouping<int, TimePeriodDTO> periodsByYear in timePeriods.GroupBy(x => x.PaymentDate.Value.Year))
            {
                foreach (var periodsByMonth in periodsByYear.GroupBy(x => x.PaymentDate.Value.Month))
                {
                    id++;
                    selectableMonths.Add(new SelectablePayrollMonthYearDTO
                    {
                        Id = id,
                        DisplayName = periodsByYear.Key.ToString() + ", " + CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(periodsByMonth.Key).First().ToString().ToUpper() + CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(periodsByMonth.Key).Substring(1),
                        TimePeriodIds = periodsByMonth.Select(x => x.TimePeriodId).ToList()
                    });
                }
            }

            return selectableMonths;
        }

        public List<SelectableTimePeriodDTO> GetReportPayrollTimePeriods(int actorCompanyId, bool hidePastPeriods = false)
        {
            return TimePeriodManager
                .GetTimePeriodsConnectedToPayrollGroups(TermGroup_TimePeriodType.Payroll, actorCompanyId, addTimePeriodHeadName: false)
                .Where(tp => tp.PaymentDate.HasValue && (!hidePastPeriods || tp.PaymentDate >= DateTime.Today))
                .Select(tp => new SelectableTimePeriodDTO
                {
                    DisplayName = string.Format("({0}), {1}, {2}", tp.PaymentDate.ToShortDateString(), tp.TimePeriodHead.Name, tp.Name),
                    Id = tp.TimePeriodId,
                    Start = tp.StartDate,
                    Stop = tp.StopDate,
                    PaymentDate = tp.PaymentDate.Value
                })
                .ToList();
        }

        /// <summary>
        /// Returns all Payroll types as a flat list, where the link between them are defined by SysTermId and ParentSysTermId. 
        /// </summary>
        /// <returns></returns>
        public List<SelectablePayrollTypeDTO> GetReportPayrollTypes(int actorCompanyId, int? sysCountryId = null)
        {
            int companySysCountryId = CompanyManager.GetCompanySysCountryId(actorCompanyId);
            sysCountryId = sysCountryId ?? this.GetLangId();

            using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
            var payrollTypes = (from s in sysEntitiesReadOnly.SysPayrollTypeView
                                where s.SysCountryId == companySysCountryId
                                select new SelectablePayrollTypeDTO
                                {
                                    Id = s.SysTermId,
                                    SysTermId = s.SysTermId,
                                    ParentSysTermId = s.ParentId ?? 0,
                                    Name = s.Name
                                }).ToList();

            payrollTypes.AddRange(from a in TimeAccumulatorManager.GetTimeAccumulators(actorCompanyId)
                                  select new SelectablePayrollTypeDTO
                                  {
                                      Id = a.TimeAccumulatorId,
                                      Name = a.Name,
                                      ParentSysTermId = (int)TermGroup_SysPayrollType.SE_Time_Accumulator,
                                      SysTermId = (int)TermGroup_SysPayrollType.SE_Time_Accumulator_AccumulatorPlaceholder
                                  });

            if (sysCountryId != 1 && !payrollTypes.IsNullOrEmpty())
            {
                List<GenericType> terms = GetTermGroupContent(TermGroup.SysPayrollType, (int)sysCountryId);
                payrollTypes.ForEach(w => w.Name = terms.FirstOrDefault(f => f.Id == w.SysTermId)?.Name ?? w.Name);
            }


            return payrollTypes;
        }

        public IDictionary<int, string> GetSortingAndGroupingForReport(int actorCompanyId, int reportId, Feature feature)
        {
            SortedDictionary<int, string> reportGroupAndSortingTypes = new SortedDictionary<int, string>();
            if (feature != Feature.Time_Distribution_Reports_Edit)
                return reportGroupAndSortingTypes;

            Report report = GetReport(reportId, actorCompanyId, loadSysReportTemplateType: true);
            if (report == null || !report.SysReportTemplateTypeId.HasValue)
                return reportGroupAndSortingTypes;

            int langId = GetLangId();

            if (report.SysReportTemplateTypeId == (int)SoeReportTemplateType.TimePayrollTransactionReport || report.SysReportTemplateTypeId == (int)SoeReportTemplateType.TimePayrollTransactionSmallReport)
            {
                reportGroupAndSortingTypes = base.GetTermGroupDictSorted(TermGroup.ReportGroupAndSortingTypes, langId, true, true, (int)TermGroup_ReportGroupAndSortingTypes.Unknown, (int)TermGroup_ReportGroupAndSortingTypes.PayrollTransactionDate);
            }
            else if (report.SysReportTemplateTypeId == (int)SoeReportTemplateType.EmployeeListReport)
            {
                reportGroupAndSortingTypes = base.GetTermGroupDictSorted(TermGroup.ReportGroupAndSortingTypes, langId, true, true, (int)TermGroup_ReportGroupAndSortingTypes.Unknown, (int)TermGroup_ReportGroupAndSortingTypes.Unknown);
                reportGroupAndSortingTypes.AddRange(base.GetTermGroupDictSorted(TermGroup.ReportGroupAndSortingTypes, langId, true, true, (int)TermGroup_ReportGroupAndSortingTypes.EmployeeCategoryName, (int)TermGroup_ReportGroupAndSortingTypes.EmployeeGender));
            }

            return reportGroupAndSortingTypes;
        }

        public ReportItemDTO GetReportItem(int reportId, int actorCompanyId, SoeReportType sysReportType)
        {
            Report report = GetReport(reportId, actorCompanyId);
            if (report == null)
                return null;

            List<GenericType> validExportTypes = GetReportExportTypes(report, sysReportType);

            return new ReportItemDTO
            {
                ReportId = report.ReportId,
                Description = report.Description,
                IncludeBudget = report.IncludeBudget,
                ShowRowsByAccount = report.ShowRowsByAccount,
                DefaultExportType = (validExportTypes.FirstOrDefault(t => t.Id == report.ExportType) ?? validExportTypes.FirstOrDefault()).ToSmallGenericType(),
                SupportedExportTypes = validExportTypes.ToSmallGenericTypes(),
                ExportFileType = report.FileType,
            };
        }

        public Dictionary<int, string> GetReportUserSelections(int reportId, ReportUserSelectionType type, int userId, int roleId, int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.ReportUserSelection.NoTracking();
            return GetReportUserSelections(entities, reportId, type, userId, roleId, actorCompanyId);
        }

        public Dictionary<int, string> GetReportUserSelections(CompEntities entities, int reportId, ReportUserSelectionType type, int userId, int roleId, int actorCompanyId)
        {
            var selections = (from rus in entities.ReportUserSelection.Include("ReportUserSelectionAccess")
                              where rus.ReportId == reportId &&
                              rus.Type == (int)type &&
                              (!rus.UserId.HasValue || rus.UserId.Value == userId) &&
                              rus.State == (int)SoeEntityState.Active
                              orderby rus.Name
                              select new
                              {
                                  rus.ReportUserSelectionId,
                                  rus.Name,
                                  rus.UserId,
                                  rus.ReportUserSelectionAccess
                              }).ToList();

            Dictionary<int, string> result = new Dictionary<int, string>();
            foreach (var selection in selections)
            {
                List<ReportUserSelectionAccess> accesses = selection.ReportUserSelectionAccess.Where(a => a.State == (int)SoeEntityState.Active).ToList();
                if (accesses.IsNullOrEmpty())
                {
                    // No access records, it's either privat or public and has been filtered in original query
                    result.Add(selection.ReportUserSelectionId, selection.Name);
                }
                else
                {
                    TermGroup_ReportUserSelectionAccessType accessType = (TermGroup_ReportUserSelectionAccessType)accesses.First().Type;
                    if (accessType == TermGroup_ReportUserSelectionAccessType.Role)
                    {
                        if (accesses.Any(a => a.RoleId == roleId))
                            result.Add(selection.ReportUserSelectionId, selection.Name);
                    }
                    else if (accessType == TermGroup_ReportUserSelectionAccessType.MessageGroup)
                    {
                        foreach (ReportUserSelectionAccess access in accesses.Where(a => a.MessageGroupId.HasValue).ToList())
                        {
                            MessageGroup group = CommunicationManager.GetMessageGroup(entities, access.MessageGroupId.Value, true);
                            if (CommunicationManager.IsUserInMessageGroup(group, actorCompanyId, userId, roleId: roleId, dateFrom: DateTime.Today, dateTo: DateTime.Today))
                            {
                                result.Add(selection.ReportUserSelectionId, selection.Name);
                                break;
                            }
                        }
                    }
                }
            }

            return result;
        }

        public ReportUserSelection GetReportUserSelection(int reportUserSelectionId, bool loadReport = false, bool loadAccess = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.ReportUserSelection.NoTracking();
            return GetReportUserSelection(entities, reportUserSelectionId, loadReport, loadAccess);
        }

        public ReportUserSelection GetReportUserSelection(CompEntities entities, int reportUserSelectionId, bool loadReport = false, bool loadAccess = false)
        {
            IQueryable<ReportUserSelection> query = (from rus in entities.ReportUserSelection
                                                     where rus.ReportUserSelectionId == reportUserSelectionId &&
                                                     rus.State == (int)SoeEntityState.Active
                                                     select rus);

            if (loadReport)
                query = query.Include("Report");
            if (loadAccess)
                query = query.Include("ReportUserSelectionAccess");

            return query.FirstOrDefault();
        }

        public ReportUserSelectionDTO GetReportSelectionFromReportPrintout(int reportPrintoutId, int actorCompanyId, int userId)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            ReportPrintoutDTO reportPrintout = GetReportPrintoutWithoutData(entitiesReadOnly, reportPrintoutId, actorCompanyId, userId);
            if (reportPrintout == null || string.IsNullOrEmpty(reportPrintout.Selection))
                return null;

            return new ReportUserSelectionDTO()
            {
                ReportId = reportPrintout.ReportId ?? 0,
                ActorCompanyId = actorCompanyId,
                UserId = userId,
                Type = ReportUserSelectionType.DataSelection,
                Selections = ReportDataSelectionDTO.FromJSON(reportPrintout.Selection),
            };
        }

        public ActionResult SaveReportUserSelection(ReportUserSelectionDTO dto)
        {
            if (dto.Selections.IsNullOrEmpty())
                return new ActionResult((int)ActionResultSave.EntityNotFound, "ReportDataSelectionDTO");

            Report report = GetReport(dto.ReportId, base.ActorCompanyId);
            if (report == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, "Report");

            ActionResult result = null;

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        // Get existing
                        ReportUserSelection reportUserSelection = dto.ReportUserSelectionId > 0 ? GetReportUserSelection(entities, dto.ReportUserSelectionId, loadAccess: true) : null;
                        if (reportUserSelection == null)
                        {
                            #region Add

                            reportUserSelection = new ReportUserSelection()
                            {
                                //Set FK
                                ReportId = report.ReportId,
                                ActorCompanyId = base.ActorCompanyId,
                            };
                            entities.ReportUserSelection.AddObject(reportUserSelection);
                            SetCreatedProperties(reportUserSelection);

                            #endregion
                        }
                        else
                        {
                            #region Update

                            SetModifiedProperties(reportUserSelection);

                            #endregion
                        }

                        // Common
                        reportUserSelection.UserId = dto.UserId;
                        reportUserSelection.Type = (int)dto.Type;
                        reportUserSelection.Name = dto.Name;
                        reportUserSelection.Description = dto.Description;
                        reportUserSelection.State = (int)dto.State;
                        reportUserSelection.Selection = ReportDataSelectionDTO.ToJSON(dto.Selections);
                        reportUserSelection.ScheduledJobHeadId = dto.ScheduledJobHeadId;

                        #region Access

                        if (reportUserSelection.ReportUserSelectionAccess != null)
                        {
                            // Check existing
                            // If still exists in input, remove from input
                            // If no longer left in input, set as deleted
                            foreach (ReportUserSelectionAccess access in reportUserSelection.ReportUserSelectionAccess.Where(a => a.State == (int)SoeEntityState.Active).ToList())
                            {
                                if (dto.Access != null)
                                {
                                    ReportUserSelectionAccessDTO acc = null;
                                    if (access.Type == (int)TermGroup_ReportUserSelectionAccessType.Role)
                                        acc = dto.Access.FirstOrDefault(a => a.Type == (TermGroup_ReportUserSelectionAccessType)access.Type && a.RoleId == access.RoleId);
                                    else if (access.Type == (int)TermGroup_ReportUserSelectionAccessType.MessageGroup)
                                        acc = dto.Access.FirstOrDefault(a => a.Type == (TermGroup_ReportUserSelectionAccessType)access.Type && a.MessageGroupId == access.MessageGroupId);

                                    if (acc != null)
                                        dto.Access.Remove(acc);
                                    else
                                        ChangeEntityState(access, SoeEntityState.Deleted);
                                }
                            }
                        }

                        // Add new, if input still contains items
                        if (!dto.Access.IsNullOrEmpty())
                        {
                            if (reportUserSelection.ReportUserSelectionAccess == null)
                                reportUserSelection.ReportUserSelectionAccess = new EntityCollection<ReportUserSelectionAccess>();

                            foreach (ReportUserSelectionAccessDTO acc in dto.Access)
                            {
                                ReportUserSelectionAccess access = new ReportUserSelectionAccess() { Type = (int)acc.Type };
                                if (acc.Type == TermGroup_ReportUserSelectionAccessType.Role)
                                    access.RoleId = acc.RoleId;
                                else if (acc.Type == TermGroup_ReportUserSelectionAccessType.MessageGroup)
                                    access.MessageGroupId = acc.MessageGroupId;

                                SetCreatedProperties(access);
                                reportUserSelection.ReportUserSelectionAccess.Add(access);
                            }
                        }

                        #endregion

                        result = SaveChanges(entities, transaction);
                        if (result.Success)
                        {
                            transaction.Complete();
                            result.IntegerValue = reportUserSelection.ReportUserSelectionId;
                        }
                    }
                }
                catch (Exception ex)
                {
                    base.LogError(ex, this.log);
                    result.Exception = ex;
                }
                finally
                {
                    if (!result.Success)
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }
            }

            return result;
        }

        public ActionResult DeleteReportUserSelection(int reportUserSelectionId)
        {
            ActionResult result = new ActionResult();

            using (CompEntities entities = new CompEntities())
            {
                ReportUserSelection reportUserSelection = GetReportUserSelection(entities, reportUserSelectionId, loadAccess: true);
                if (reportUserSelection == null)
                    return new ActionResult((int)ActionResultDelete.EntityNotFound, "ReportUserSelection");

                foreach (ReportUserSelectionAccess access in reportUserSelection.ReportUserSelectionAccess.Where(a => a.State != (int)SoeEntityState.Deleted).ToList())
                {
                    ChangeEntityState(entities, access, SoeEntityState.Deleted, false);
                }

                return ChangeEntityState(entities, reportUserSelection, SoeEntityState.Deleted, true);
            }
        }

        public ActionResult DeletePrintedReport(int reportPrintoutId, int actorCompanyId, int userId)
        {
            using (CompEntities entities = new CompEntities())
            {
                ReportPrintout reportPrintout = GetReportPrintout(entities, reportPrintoutId, actorCompanyId, userId);
                if (reportPrintout == null)
                    return new ActionResult((int)ActionResultDelete.EntityNotFound, "ReportPrintout");

                bool deleteAllowed = true;
                if (reportPrintout.Status == (int)TermGroup_ReportPrintoutStatus.Ordered && reportPrintout.Created > DateTime.Now.AddMinutes(-5))
                    deleteAllowed = false;

#if DEBUG
                deleteAllowed = reportPrintout.Created < DateTime.Now.AddSeconds(-30);
#endif

                if (deleteAllowed)
                {
                    reportPrintout.Data = null;
                    reportPrintout.XML = null;
                    reportPrintout.DataCompressed = null;
                    reportPrintout.XMLCompressed = null;
                    reportPrintout.Selection = "";
                    reportPrintout.Status = (int)TermGroup_ReportPrintoutStatus.DeletedByUser;
                    SetModifiedProperties(reportPrintout);

                    ActionResult result = SaveChanges(entities);
                    if (!result.Success)
                        return result;

                    ReportDataManager.TryStartPrintReportDTO(null, actorCompanyId, userId, null, 0);
                    Thread.Sleep(1000);

                    return result;
                }
                else
                {
                    return new ActionResult(1, "Delete not allow, try again later");
                }
            }
        }

        public ActionResult SaveUserReportFavorite(int reportId, int actorCompanyId, int userId)
        {
            Report report = GetReport(reportId, actorCompanyId);
            if (report == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, "Report");

            ActionResult result = null;

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        // Get existing
                        UserReportFavorite favorite = GetUserReportFavorite(entities, reportId, actorCompanyId, userId);
                        if (favorite == null)
                        {
                            favorite = new UserReportFavorite()
                            {
                                ReportId = reportId,
                                ActorCompanyId = actorCompanyId,
                                UserId = userId,
                                Name = report.Name
                            };

                            entities.UserReportFavorite.AddObject(favorite);
                        }

                        result = SaveChanges(entities, transaction);
                        if (result.Success)
                            transaction.Complete();
                    }
                }
                catch (Exception ex)
                {
                    base.LogError(ex, this.log);
                    result.Exception = ex;
                }
                finally
                {
                    if (!result.Success)
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }
            }

            return result;
        }

        public ActionResult RenameUserReportFavorite(int reportId, int actorCompanyId, int userId, string name)
        {
            // Default result is successful
            ActionResult result = new ActionResult();

            Report report = GetReport(reportId, actorCompanyId);
            if (report == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, "Report");

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        // Get existing
                        UserReportFavorite favorite = GetUserReportFavorite(entities, reportId, actorCompanyId, userId);
                        if (favorite != null)
                        {
                            favorite.Name = name;

                            result = SaveChanges(entities, transaction);
                            if (result.Success)
                                transaction.Complete();
                        }
                    }
                }
                catch (Exception ex)
                {
                    base.LogError(ex, this.log);
                    result.Exception = ex;
                }
                finally
                {
                    if (!result.Success)
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }
            }

            return result;
        }

        public ActionResult DeleteUserReportFavorite(int reportId, int actorCompanyId, int userId)
        {
            // Default result is successful
            ActionResult result = new ActionResult();

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        // Get existing
                        UserReportFavorite favorite = GetUserReportFavorite(entities, reportId, actorCompanyId, userId);
                        if (favorite != null)
                        {
                            entities.DeleteObject(favorite);

                            result = SaveChanges(entities, transaction);
                            if (result.Success)
                                transaction.Complete();
                        }
                    }
                }
                catch (Exception ex)
                {
                    base.LogError(ex, this.log);
                    result.Exception = ex;
                }
                finally
                {
                    if (!result.Success)
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }
            }

            return result;
        }

        #region Help-methods

        private UserReportFavorite GetUserReportFavorite(CompEntities entities, int reportId, int actorCompanyId, int userId)
        {
            return (from f in entities.UserReportFavorite
                    where f.ReportId == reportId &&
                    f.ActorCompanyId == actorCompanyId &&
                    f.UserId == userId
                    select f).FirstOrDefault();
        }

        private int GetSysReportTemplateTypeGroupSortOrder(int group)
        {
            int sort;
            switch ((TermGroup_SysReportTemplateTypeGroup)group)
            {
                case TermGroup_SysReportTemplateTypeGroup.Employee:
                    sort = 1;
                    break;
                case TermGroup_SysReportTemplateTypeGroup.Schedule:
                    sort = 2;
                    break;
                case TermGroup_SysReportTemplateTypeGroup.Time:
                    sort = 3;
                    break;
                case TermGroup_SysReportTemplateTypeGroup.Payroll:
                    sort = 4;
                    break;
                case TermGroup_SysReportTemplateTypeGroup.HR:
                    sort = 5;
                    break;
                case TermGroup_SysReportTemplateTypeGroup.Registry:
                    sort = 6;
                    break;
                case TermGroup_SysReportTemplateTypeGroup.Accounting:
                    sort = 7;
                    break;
                case TermGroup_SysReportTemplateTypeGroup.AccountsRecievable:
                    sort = 8;
                    break;
                case TermGroup_SysReportTemplateTypeGroup.AccountsPayable:
                    sort = 9;
                    break;
                case TermGroup_SysReportTemplateTypeGroup.Sales:
                    sort = 10;
                    break;
                case TermGroup_SysReportTemplateTypeGroup.Project:
                    sort = 11;
                    break;
                case TermGroup_SysReportTemplateTypeGroup.Stock:
                    sort = 12;
                    break;
                case TermGroup_SysReportTemplateTypeGroup.Purchase:
                    sort = 13;
                    break;
                default:
                    sort = 100;
                    break;
            }

            return sort;
        }

        private string GetReportPrintoutErrorMessage(SoeReportDataResultMessage resultMessage, string resultMessageDetails, bool showDetails = false)
        {
            if (resultMessage == SoeReportDataResultMessage.Success)
                return string.Empty;
            if (resultMessage == SoeReportDataResultMessage.CreateVoucherFailed)
            {
                showDetails = true;
            }
            return $"{GetText(5970, "Rapport kunde inte skrivas ut")}. {GetRetReportResultMessage(resultMessage)} ({(int)resultMessage}). {(showDetails ? resultMessageDetails : string.Empty)}";
        }

        private string GetRetReportResultMessage(SoeReportDataResultMessage resultMessage)
        {
            string message = "";

            switch (resultMessage)
            {
                #region Core

                case SoeReportDataResultMessage.Error:
                    message = GetText(11793, "Ett fel uppstod");
                    break;
                case SoeReportDataResultMessage.EmptyInput:
                    message = GetText(11794, "Felaktigt urval");
                    break;
                case SoeReportDataResultMessage.ReportTemplateDataNotFound:
                    message = GetText(5977, "Rapportmall hittades inte");
                    break;
                case SoeReportDataResultMessage.DocumentNotCreated:
                    message = GetText(11795, "Rapportunderlag i xml kunde inte skapas");
                    break;
                case SoeReportDataResultMessage.ReportFailed:
                    message = GetText(11796, "Rapportmall kunde inte initieras");
                    break;
                case SoeReportDataResultMessage.ExportFailed:
                    message = GetText(11798, "Rapport kunde inte genereras");
                    break;

                #endregion

                #region Economy 

                case SoeReportDataResultMessage.BalanceReportHasNoGroupsOrHeaders:
                case SoeReportDataResultMessage.ResultReportHasNoGroupsOrHeaders:
                    message = GetText(11799, "Rapport saknar grupper och rubriker");
                    break;
                case SoeReportDataResultMessage.CreateVoucherFailed:
                    message = GetText(7207, "Verifikat kunde inte skapas");
                    break;

                #endregion

                #region Billing 

                case SoeReportDataResultMessage.EdiEntryNotFound:
                    message = GetText(11800, "EDI-post hittades inte");
                    break;
                case SoeReportDataResultMessage.EdiEntryCouldNotParseXML:
                    message = GetText(11801, "EDI-xml kunde inte tolkas");
                    break;
                case SoeReportDataResultMessage.EdiEntryCouldNotSavePDF:
                    message = GetText(11802, "EDI-pdf kunde inte genereras");
                    break;

                #endregion

                #region Time 

                case SoeReportDataResultMessage.ReportsNotAuthorized:
                    message = GetText(11803, "Behörighet saknas för rapporten");
                    break;

                    #endregion

            }

            return message;
        }

        #endregion

        #endregion

        #region ApiMatrix

        public List<ApiMatrixReport> GetApiMatrixReports(int module)
        {
            List<ApiMatrixReport> apiMatrixReports = new List<ApiMatrixReport>();
            var reports = GetReportsForMenu(module, SoeReportType.Analysis, base.ActorCompanyId, base.RoleId, base.UserId, false);
            foreach (var report in reports)
            {
                ApiMatrixReport apiMatrixReport = new ApiMatrixReport()
                {
                    ReportId = report.ReportId,
                    SysReportTemplateTypeId = report.SysReportTemplateTypeId,
                    Description = report.Description,
                    GroupName = report.GroupName,
                    Name = report.Name,
                    ReportNr = report.ReportNr,
                    ApiMatrixReportSelectionInfos = new List<ApiMatrixReportSelectionInfo>()
                };

                var columns = GetMatrixLayoutColumns(report.SysReportTemplateTypeId, base.ActorCompanyId, report.Module);

                foreach (var col in columns)
                {
                    apiMatrixReport.ApiMatrixColumns.Add(new ApiMatrixColumns()
                    {
                        Field = col.Field,
                        Title = col.Title,
                    });
                }

                var templateType = (SoeReportTemplateType)report.SysReportTemplateTypeId;
                var employeeSelection = new ApiMatrixReportSelectionInfo() { ApiMatrixSelectionType = ApiMatrixSelectionType.Employee };
                var payrollProductSelection = new ApiMatrixReportSelectionInfo() { ApiMatrixSelectionType = ApiMatrixSelectionType.PayrollProduct, Description = "PayrollProducts selection" };
                var dateRangeSelection = new ApiMatrixReportSelectionInfo() { ApiMatrixSelectionType = ApiMatrixSelectionType.DateRange };
                List<ApiMatrixReportSelectionInfo> timeReportStandardSelection = new List<ApiMatrixReportSelectionInfo>();
                timeReportStandardSelection.Add(employeeSelection);
                timeReportStandardSelection.Add(dateRangeSelection);
                switch (templateType)
                {
                    case SoeReportTemplateType.TimeTransactionAnalysis:
                    case SoeReportTemplateType.PayrollTransactionAnalysis:
                    case SoeReportTemplateType.EmployeeAnalysis:
                    case SoeReportTemplateType.ScheduleAnalysis:
                    case SoeReportTemplateType.EmployeeDateAnalysis:
                    case SoeReportTemplateType.TimeStampEntryAnalysis:
                    case SoeReportTemplateType.EmployeeSkillAnalysis:
                    case SoeReportTemplateType.OrganisationHrAnalysis:
                    case SoeReportTemplateType.EmployeeEndReasonsAnalysis:
                    case SoeReportTemplateType.EmployeeSalaryAnalysis:
                    case SoeReportTemplateType.StaffingStatisticsAnalysis:
                    case SoeReportTemplateType.AnnualProgressAnalysis:
                    case SoeReportTemplateType.AggregatedTimeStatisticsAnalysis:
                    case SoeReportTemplateType.EmployeeMeetingAnalysis:
                    case SoeReportTemplateType.TimeScheduledSummary:
                    case SoeReportTemplateType.EmployeeExperienceAnalysis:
                    case SoeReportTemplateType.EmployeeDocumentAnalysis:
                    case SoeReportTemplateType.EmployeeAccountAnalysis:
                    case SoeReportTemplateType.EmployeeFixedPayLinesAnalysis:
                    case SoeReportTemplateType.EmploymentHistoryAnalysis:
                    case SoeReportTemplateType.EmployeeSalaryDistressAnalysis:
                    case SoeReportTemplateType.EmployeeSalaryUnionFeesAnalysis:
                    case SoeReportTemplateType.EmploymentDaysAnalysis:
                    case SoeReportTemplateType.ShiftQueueAnalysis:
                    case SoeReportTemplateType.ShiftHistoryAnalysis:
                    case SoeReportTemplateType.ShiftRequestAnalysis:
                    case SoeReportTemplateType.AbsenceRequestAnalysis:
                        apiMatrixReport.ApiMatrixReportSelectionInfos.AddRange(timeReportStandardSelection);
                        break;
                    case SoeReportTemplateType.StaffingneedsFrequencyAnalysis:
                    case SoeReportTemplateType.ReportStatisticsAnalysis:
                    case SoeReportTemplateType.AccountHierachyAnalysis:
                        apiMatrixReport.ApiMatrixReportSelectionInfos.Add(dateRangeSelection);
                        break;
                    case SoeReportTemplateType.LongtermAbsenceAnalysis:
                        apiMatrixReport.ApiMatrixReportSelectionInfos.Add(payrollProductSelection);
                        apiMatrixReport.ApiMatrixReportSelectionInfos.AddRange(timeReportStandardSelection);
                        apiMatrixReport.ApiMatrixReportSelectionInfos.Add(new ApiMatrixReportSelectionInfo() { ApiMatrixSelectionType = ApiMatrixSelectionType.Text, Description = "Key = numberOfDays" });
                        break;
                    case SoeReportTemplateType.UserAnalysis:
                    case SoeReportTemplateType.SupplierAnalysis:
                    case SoeReportTemplateType.CustomerAnalysis:
                    case SoeReportTemplateType.InvoiceProductAnalysis:
                    case SoeReportTemplateType.Generic:
                    case SoeReportTemplateType.ShiftTypeSkillAnalysis:
                    case SoeReportTemplateType.EmployeeTimePeriodAnalysis:
                    case SoeReportTemplateType.PayrollProductsAnalysis:
                    case SoeReportTemplateType.OrderAnalysis:
                    case SoeReportTemplateType.InvoiceAnalysis:
                        // No action required
                        break;

                    default:
                        // Handle any unhandled cases
                        break;
                }

                apiMatrixReports.Add(apiMatrixReport);
            }
            return apiMatrixReports;
        }

        #endregion

        #region ReportPrintout

        private ReportPrintout DecompressReportPrintOut(ReportPrintout reportPrintout)
        {
            if (reportPrintout != null)
            {
                if (reportPrintout.XMLCompressed != null && reportPrintout.XML == null)
                {
                    reportPrintout.XML = ZipUtility.UnzipString(reportPrintout.XMLCompressed);
                    reportPrintout.XMLCompressed = null;
                }

                if (reportPrintout.DataCompressed != null && reportPrintout.Data == null)
                {
                    reportPrintout.Data = CompressionUtil.Decompress(reportPrintout.DataCompressed);
                    reportPrintout.DataCompressed = null;
                }
            }

            return reportPrintout;
        }

        public ActionResult CompressReportPrintout(CompEntities entities, ReportPrintout reportPrintout)
        {
            try
            {
                reportPrintout.XMLCompressed = GeneralManager.CompressString(reportPrintout.XML);
                reportPrintout.DataCompressed = GeneralManager.CompressData(reportPrintout.Data);
                reportPrintout.Data = null;
                reportPrintout.XML = null;
                return SaveChanges(entities);
            }
            catch (Exception ex)
            {
                LogError(ex.ToString());
                return new ActionResult(ex);
            }
        }

        public ReportPrintout GetReportPrintout(int reportPrintoutId, int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.ReportPrintout.NoTracking();
            return GetReportPrintout(entities, reportPrintoutId, actorCompanyId);
        }

        public ReportPrintout GetReportPrintout(CompEntities entities, int reportPrintoutId, int actorCompanyId)
        {
            if (reportPrintoutId == 0)
                return null;

            entities.CommandTimeout = 10 * 60;

            var reportPrintout = (from r in entities.ReportPrintout
                                  where r.ReportPrintoutId == reportPrintoutId &&
                                  r.ActorCompanyId == actorCompanyId
                                  select r).FirstOrDefault();

            return DecompressReportPrintOut(reportPrintout);
        }

        public ReportPrintoutDTO GetReportPrintoutWithoutData(CompEntities entities, int reportPrintoutId, int actorCompanyId, int userId)
        {
            var reportPrintout = GetReportPrintoutWithoutData(entities, reportPrintoutId, actorCompanyId);
            if (reportPrintout != null && reportPrintout.UserId != userId)
                return null;

            return reportPrintout;
        }

        public ReportPrintoutDTO GetReportPrintoutWithoutData(CompEntities entities, int reportPrintoutId, int actorCompanyId)
        {
            if (reportPrintoutId == 0)
                return null;

            entities.CommandTimeout = 10 * 60;

            var reportPrintout = (from e in entities.ReportPrintout
                                  where e.ReportPrintoutId == reportPrintoutId &&
                                  e.ActorCompanyId == actorCompanyId
                                  select new ReportPrintoutDTO()
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
                                      RoleId = e.RoleId
                                  }).FirstOrDefault();

            return reportPrintout;
        }
        public List<ReportPrintoutDTO> GetReportPrintoutStatisticsForPeriod(CompEntities entities, int actorCompanyId, DateTime dateFrom, DateTime dateTo)
        {
            entities.CommandTimeout = 10 * 60;

            var reportPrintout = (from e in entities.ReportPrintout
                                  where e.Created >= dateFrom && e.Created <= dateTo &&
                                  e.ActorCompanyId == actorCompanyId
                                  select new ReportPrintoutDTO()
                                  {
                                      ReportTemplateId = e.ReportTemplateId,
                                      SysReportTemplateTypeId = e.SysReportTemplateTypeId,
                                      Status = e.Status,
                                      ResultMessage = e.ResultMessage,
                                      ReportName = e.ReportName,
                                      DeliveredTime = e.DeliveredTime,
                                      Created = e.Created,
                                      CreatedBy = e.CreatedBy,
                                      UserId = e.UserId,
                                      RoleId = e.RoleId
                                  });

            return reportPrintout.ToList();
        }
        public ReportPrintout GetReportPrintout(int reportPrintOutId, int actorCompanyId, int userId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.ReportPrintout.NoTracking();
            return GetReportPrintout(entities, reportPrintOutId, actorCompanyId, userId);
        }

        public ReportPrintout GetReportPrintout(CompEntities entities, int reportPrintOutId, int actorCompanyId, int userId)
        {
            var reportPrintout = (from r in entities.ReportPrintout
                                  where r.UserId == userId &&
                                  r.ActorCompanyId == actorCompanyId &&
                                  r.ReportPrintoutId == reportPrintOutId
                                  select r).FirstOrDefault();

            return DecompressReportPrintOut(reportPrintout);
        }

        public List<ReportPrintoutDTO> GetReportPrintoutsForGauge(int userId, int actorCompanyId, int daysBack)
        {
            DateTime date = DateTime.Now.AddDays(-daysBack);
            using var entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            return (from e in entitiesReadOnly.ReportPrintout
                    where e.UserId == userId &&
                    e.ActorCompanyId == actorCompanyId &&
                    e.Created > date
                    select new ReportPrintoutDTO()
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
                        XML = null,
                        Data = null,
                        Created = e.Created,
                        CreatedBy = e.CreatedBy,
                        Modified = e.Modified,
                        ModifiedBy = e.ModifiedBy,
                        UserId = e.UserId,
                        RoleId = e.RoleId
                    }).OrderByDescending(a => a.Created).ToList();
        }

        public ActionResult CleanReportPrintouts(int actorCompanyId, DateTime cleanToDate)
        {
            ActionResult result = null;

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();

                    using (var transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        var reportPrintouts = (from r in entities.ReportPrintout
                                               where r.ActorCompanyId == actorCompanyId &&
                                               !r.CleanedTime.HasValue &&
                                               r.Created <= cleanToDate
                                               select r).ToList();

                        foreach (var reportPrintout in reportPrintouts)
                        {
                            reportPrintout.CleanedTime = DateTime.Now;
                            reportPrintout.Data = null;
                            reportPrintout.XML = null;
                            reportPrintout.DataCompressed = null;
                            reportPrintout.XMLCompressed = null;
                            reportPrintout.Status = reportPrintout.Status != (int)TermGroup_ReportPrintoutStatus.DeletedByUser ? (int)TermGroup_ReportPrintoutStatus.Cleaned : reportPrintout.Status;
                            reportPrintout.Selection = "";
                            SetModifiedProperties(reportPrintout);
                        }

                        result = SaveChanges(entities, transaction);

                        //Commit transaction
                        if (result.Success)
                            transaction.Complete();
                    }
                }
                catch (Exception ex)
                {
                    LogError(ex, log);
                    result.Exception = ex;
                }
                finally
                {
                    if (result.Success)
                    {
                        //Set success properties                        
                    }
                    else
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }

                return result;
            }
        }

        #endregion

        #region ReportUrl

        public ReportUrl GetReportUrl(int reportUrlId, int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.ReportUrl.NoTracking();
            return GetReportUrl(entities, reportUrlId, actorCompanyId);
        }

        public ReportUrl GetReportUrl(CompEntities entities, int reportUrlId, int actorCompanyId)
        {
            return (from r in entities.ReportUrl
                    where r.ActorCompanyId == actorCompanyId &&
                    r.ReportUrlId == reportUrlId
                    select r).FirstOrDefault();
        }

        public ReportUrl GetReportUrl(string guid, int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.ReportUrl.NoTracking();
            return GetReportUrl(entities, guid, actorCompanyId);
        }

        public ReportUrl GetReportUrl(CompEntities entities, string guid, int actorCompanyId)
        {
            return (from r in entities.ReportUrl
                    where r.ActorCompanyId == actorCompanyId &&
                    r.Guid == guid
                    select r).FirstOrDefault();
        }

        public ActionResult SaveReportUrl(string guid, string url, int reportId, int sysReportTemplateTypeId, int actorCompanyId)
        {
            using (CompEntities entities = new CompEntities())
            {
                ReportUrl reportUrl = new ReportUrl()
                {
                    Guid = guid,
                    Url = url,
                    SysReportTemplateTypeId = sysReportTemplateTypeId,

                    //Set FK
                    ActorCompanyId = actorCompanyId,
                    ReportId = reportId,
                };
                SetCreatedProperties(reportUrl);
                entities.ReportUrl.AddObject(reportUrl);

                ActionResult result = SaveChanges(entities);
                if (result.Success)
                    result.IntegerValue = reportUrl.ReportUrlId;

                return result;

            }
        }

        public bool DoShowReportPrintoutErrorMessage(ReportPrintout reportPrintout)
        {
            if (reportPrintout == null)
                return true;

            bool doNotShowMessage = false;
            switch (reportPrintout.DeliveryType)
            {
                case (int)TermGroup_ReportPrintoutDeliveryType.Instant:
                case (int)TermGroup_ReportPrintoutDeliveryType.Generate:
                    //Do not show message if report was generated
                    doNotShowMessage = reportPrintout.Status == (int)TermGroup_ReportPrintoutStatus.Delivered;
                    break;
                case (int)TermGroup_ReportPrintoutDeliveryType.XEMail:
                case (int)TermGroup_ReportPrintoutDeliveryType.Email:
                    //Do not show message in either case. Message is fetched from db later
                    doNotShowMessage = reportPrintout.Status == (int)TermGroup_ReportPrintoutStatus.Sent || reportPrintout.Status == (int)TermGroup_ReportPrintoutStatus.SentFailed;
                    break;
            }
            return !doNotShowMessage;
        }

        public string GetReportPrintUrl(int sysReportTemplateTypeId, int id)
        {
            ReportSelectionDTO reportItem = null;

            switch ((SoeReportTemplateType)sysReportTemplateTypeId)
            {
                case SoeReportTemplateType.GeneralLedger:
                    #region General Ledger

                    // Used specifically from "Kontoanalysen" in account edit page
                    Report report = GetSettingReport(SettingMainType.Company, CompanySettingType.AccountingDefaultAnalysisReport, SoeReportTemplateType.GeneralLedger, base.ActorCompanyId, this.UserId, this.RoleId);

                    if (report != null)
                    {
                        Account account = AccountManager.GetAccount(base.ActorCompanyId, id, onlyActive: false);
                        if (account != null)
                            reportItem = new GeneralLedgerReportDTO(base.ActorCompanyId, report.ReportId, sysReportTemplateTypeId, account.AccountId, account.AccountNr, account.AccountDimId, webBaseUrl: true);

                    }
                    #endregion
                    break;
            }

            return reportItem != null ? reportItem.ToString(true) : string.Empty;
        }

        public string GetTimeEmploymentContractShortSubstitutePrintUrl(int actorCompanyId, List<int> employeeIds, int userId, int roleId, List<DateTime> dates, bool printedFromSchedulePlanning)
        {
            DateTime? changesForDate = dates?.OrderBy(x => x).LastOrDefault();

            Report report = GetSettingReport(SettingMainType.Company, CompanySettingType.DefaultEmploymentContractShortSubstituteReport, SoeReportTemplateType.TimeEmploymentContract, actorCompanyId, userId, roleId);
            if (report == null)
                return string.Empty;

            var reportItem = new TimeEmploymentReportDTO(actorCompanyId, report.ReportId, report.SysReportTemplateTypeId ?? 0, DateTime.MinValue, DateTime.MaxValue, changesForDate, dates, employeeIds, printedFromSchedulePlanning);
            ActionResult result = SaveReportUrl(reportItem.ReportGuid, reportItem.ToString(false), report.ReportId, (int)SoeReportTemplateType.TimeEmploymentContract, actorCompanyId);

            return result.Success ? reportItem.ToShortString(true) : string.Empty;
        }

        public string GetPayrollSlipReportPrintUrl(int actorCompanyId, int timePeriodId, int employeeId, int reportId)
        {
            int payrollSlipDataStorageId = GeneralManager.GetDataStorageId(SoeDataStorageRecordType.PayrollSlipXML, timePeriodId, employeeId, base.ActorCompanyId);
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            TimePeriod timePeriod = TimePeriodManager.GetTimePeriod(entitiesReadOnly, timePeriodId, actorCompanyId);
            if (timePeriod == null)
                return string.Empty;

            var reportItem = new TimePayrollSlipReportDTO(actorCompanyId, reportId, (int)SoeReportTemplateType.PayrollSlip, timePeriod.StartDate, timePeriod.StopDate, employeeId, timePeriodId, preliminary: (payrollSlipDataStorageId == 0));
            ActionResult result = SaveReportUrl(reportItem.ReportGuid, reportItem.ToString(false), reportId, (int)SoeReportTemplateType.PayrollSlip, actorCompanyId);

            return result.Success ? reportItem.ToShortString(true) : string.Empty;
        }

        public string GetTimeMonthlyReportPrintUrl(int actorCompanyId, DateTime startDate, DateTime stopDate, int employeeId, int reportId)
        {
            var reportItem = new TimeEmployeeReportDTO(actorCompanyId, reportId, (int)SoeReportTemplateType.TimeMonthlyReport, startDate, stopDate, employeeId);
            ActionResult result = SaveReportUrl(reportItem.ReportGuid, reportItem.ToString(false), reportId, (int)SoeReportTemplateType.TimeMonthlyReport, actorCompanyId);

            return result.Success ? reportItem.ToShortString(true) : string.Empty;
        }

        public string GetTimeMonthlyReportPrintUrl(List<int> employeeIds, DateTime startDate, DateTime stopDate, int reportId, SoeReportTemplateType reportTemplateType)
        {
            var reportItem = new TimeEmployeeReportDTO(base.ActorCompanyId, reportId, (int)reportTemplateType, startDate, stopDate, 0, null, employeeIds, null, null, null, false);
            ActionResult result = SaveReportUrl(reportItem.ReportGuid, reportItem.ToString(false), reportId, (int)reportTemplateType, base.ActorCompanyId);

            return result.Success ? reportItem.ToShortString(true) : string.Empty;
        }

        public string GetTimeEmploymentContractPrintUrl(int actorCompanyId, int employeeId, int employmentId, int reportId, int reportTemplateTypeId, DateTime dateFrom, DateTime dateTo, List<DateTime> dates, bool printedFromSchedulePlanning = false)
        {
            DateTime? changesForDate = dates?.OrderBy(x => x).LastOrDefault();

            var reportItem = new TimeEmploymentReportDTO(actorCompanyId, reportId, reportTemplateTypeId, dateFrom, dateTo, changesForDate, dates, employeeId, employmentId, printedFromSchedulePlanning);
            ActionResult result = SaveReportUrl(reportItem.ReportGuid, reportItem.ToString(false), reportId, reportTemplateTypeId, actorCompanyId);

            return result.Success ? reportItem.ToShortString(true) : string.Empty;
        }

        public string GetBalanceListPrintUrl(List<int> invoiceIds, int reportId, int sysReportTemplateType, List<int> paymentRowIds)
        {
            var reportItem = new BalanceListReportDTO(base.ActorCompanyId, reportId, sysReportTemplateType, invoiceIds, paymentRowIds);
            ActionResult result = SaveReportUrl(reportItem.ReportGuid, reportItem.ToString(false), reportId, sysReportTemplateType, base.ActorCompanyId);

            return result.Success ? reportItem.ToShortString(true) : string.Empty;
        }

        public string GetInvoiceReminderPrintUrl(List<int> customerInvoiceIds)
        {
            CompanySettingType companySettingType = CompanySettingType.CustomerDefaultReminderTemplate;
            SoeReportTemplateType reportTemplatetype = SoeReportTemplateType.BillingInvoiceReminder;
            int reportId = GetCompanySettingReportId(SettingMainType.Company, companySettingType, reportTemplatetype, base.ActorCompanyId, base.UserId);

            var reportItem = new BillingInvoiceReportDTO(ActorCompanyId, reportId, (int)reportTemplatetype, customerInvoiceIds, false, true);
            ActionResult result = SaveReportUrl(reportItem.ReportGuid, reportItem.ToString(false), reportId, (int)reportTemplatetype, ActorCompanyId);

            return result.Success ? reportItem.ToShortString(true) : string.Empty;
        }

        public string GetInvoiceInterestPrintUrl(List<int> customerInvoiceIds)
        {
            CompanySettingType companySettingType = CompanySettingType.CustomerDefaultInterestTemplate;
            SoeReportTemplateType reportTemplatetype = SoeReportTemplateType.BillingInvoiceInterest;
            int reportId = GetCompanySettingReportId(SettingMainType.Company, companySettingType, reportTemplatetype, base.ActorCompanyId, base.UserId);

            var reportItem = new BillingInvoiceReportDTO(ActorCompanyId, reportId, (int)reportTemplatetype, customerInvoiceIds, false, true);
            ActionResult result = SaveReportUrl(reportItem.ReportGuid, reportItem.ToString(false), reportId, (int)reportTemplatetype, ActorCompanyId);

            return result.Success ? reportItem.ToShortString(true) : string.Empty;
        }

        public string GetInterestRateCalculationPrintUrl(List<int> customerInvoiceIds, int reportId, int sysReportTemplateType)
        {
            var reportItem = new BillingInvoiceReportDTO(ActorCompanyId, reportId, sysReportTemplateType, customerInvoiceIds, false, true);
            ActionResult result = SaveReportUrl(reportItem.ReportGuid, reportItem.ToString(false), reportId, sysReportTemplateType, ActorCompanyId);

            return result.Success ? reportItem.ToShortString(true) : string.Empty;
        }

        public string GetProductListPrintUrl(List<int> productIds, int reportId, int sysReportTemplateType)
        {
            var reportItem = new ProductListReportDTO(base.ActorCompanyId, reportId, sysReportTemplateType, productIds);
            ActionResult result = SaveReportUrl(reportItem.ReportGuid, reportItem.ToString(false), reportId, sysReportTemplateType, base.ActorCompanyId);

            return result.Success ? reportItem.ToShortString(true) : string.Empty;
        }

        public string GetCustomerInvoiceIOReportPrintUrl(List<int> customerInvoiceIoIds, int reportId, int sysReportTemplateType)
        {
            var reportItem = new CustomerInvoiceIOReportDTO(base.ActorCompanyId, reportId, sysReportTemplateType, customerInvoiceIoIds);
            ActionResult result = SaveReportUrl(reportItem.ReportGuid, reportItem.ToString(false), reportId, sysReportTemplateType, base.ActorCompanyId);

            return result.Success ? reportItem.ToShortString(true) : string.Empty;
        }

        public string GetVoucherHeadIOReportUrl(List<int> voucherHeadIoIds, int reportId, int sysReportTemplateType)
        {
            var reportItem = new VoucherIOReportDTO(base.ActorCompanyId, reportId, sysReportTemplateType, voucherHeadIoIds);
            ActionResult result = SaveReportUrl(reportItem.ReportGuid, reportItem.ToString(false), reportId, sysReportTemplateType, base.ActorCompanyId);

            return result.Success ? reportItem.ToShortString(true) : string.Empty;
        }

        public OrderChecklistReportDTO GetChecklistPrintUrlDTO(int invoiceId, int headRecordId, int reportId)
        {
            SoeReportTemplateType reportTemplatetype = SoeReportTemplateType.OrderChecklistReport;
            return new OrderChecklistReportDTO(ActorCompanyId, reportId, (int)reportTemplatetype, invoiceId, headRecordId);
        }

        public string GetChecklistPrintUrl(int invoiceId, int headRecordId, int reportId)
        {
            var reportItem = GetChecklistPrintUrlDTO(invoiceId, headRecordId, reportId);
            ActionResult result = SaveReportUrl(reportItem.ReportGuid, reportItem.ToString(false), reportId, reportItem.ReportTemplateTypeId, base.ActorCompanyId);

            return result.Success ? reportItem.ToShortString(true) : string.Empty;
        }

        public string GetVoucherListPrintUrl(List<int> voucherHeadIds)
        {
            CompanySettingType companySettingType = CompanySettingType.AccountingDefaultVoucherList;
            SoeReportTemplateType reportTemplatetype = SoeReportTemplateType.VoucherList;
            int reportId = GetCompanySettingReportId(SettingMainType.Company, companySettingType, reportTemplatetype, base.ActorCompanyId, base.UserId);

            var reportItem = new VoucherListReportDTO(ActorCompanyId, reportId, (int)reportTemplatetype, voucherHeadIds, true);
            ActionResult result = SaveReportUrl(reportItem.ReportGuid, reportItem.ToString(false), reportId, (int)reportTemplatetype, base.ActorCompanyId);

            return result.Success ? reportItem.ToShortString(true) : string.Empty;
        }

        public string GetProjectTransactionsPrintUrl(int actorCompanyId, int reportId, int sysReportTemplateTypeId, int exportType, List<int> projectIds, string offerNrFrom, string offerNrTo, string orderNrFrom, string orderNrTo, string invoiceNrFrom, string invoiceNrTo,
                    string employeeNrFrom, string employeeNrTo, string payrollProductNrFrom, string payrollProductNrTo, string invoiceProductNrFrom, string invoiceProductNrTo,
                    DateTime? payrollTransactionDateFrom, DateTime? payrollTransactionDateTo, DateTime? invoiceTransactionDateFrom, DateTime? invoiceTransactionDateTo,
                    bool includeChildProjects, int dim2Id, string dim2From, string dim2To,
                    int dim3Id, string dim3From, string dim3To, int dim4Id, string dim4From, string dim4To, int dim5Id, string dim5From, string dim5To, int dim6Id, string dim6From, string dim6To, int exportTypeId)
        {
            var reportItem = new ProjectTransactionsReportDTO(actorCompanyId, reportId, (int)SoeReportTemplateType.ProjectTransactionsReport, projectIds,
                    offerNrFrom, offerNrTo, orderNrFrom, orderNrTo, invoiceNrFrom, invoiceNrTo,
                    employeeNrFrom, employeeNrTo, payrollProductNrFrom, payrollProductNrTo, invoiceProductNrFrom, invoiceProductNrTo,
                    payrollTransactionDateFrom, payrollTransactionDateTo, invoiceTransactionDateFrom, invoiceTransactionDateTo,
                    includeChildProjects, dim2Id, dim2From, dim2To,
                    dim3Id, dim3From, dim3To, dim4Id, dim4From, dim4To, dim5Id, dim5From, dim5To, dim6Id, dim6From, dim6To, exportTypeId);

            ActionResult result = SaveReportUrl(reportItem.ReportGuid, reportItem.ToString(false), reportId, (int)SoeReportTemplateType.ProjectTransactionsReport, ActorCompanyId);

            return result.Success ? reportItem.ToShortString(true) : string.Empty;
        }


        public string GetOrderPrintUrl(List<int> invoiceIds, int reportId, List<int> emailRecipients, int languageId, string invoiceNr, int actorCustomerId, OrderInvoiceRegistrationType registrationType, bool invoiceCopy)
        {
            var reportItem = GetBillingInvoiceReportDTO(invoiceIds, reportId, emailRecipients, languageId, invoiceNr, actorCustomerId, registrationType, invoiceCopy);
            ActionResult result = SaveReportUrl(reportItem.ReportGuid, reportItem.ToString(false), reportItem.ReportId, reportItem.ReportTemplateTypeId, ActorCompanyId);

            return result.Success ? reportItem.ToShortString(true) : string.Empty;
        }

        public string GetPurchasePrintUrl(List<int> purchaseIds, int reportId, int languageId)
        {
            var reportItem = new PurchaseOrderReportDTO(this.ActorCompanyId, purchaseIds, reportId, languageId, (int)SoeReportTemplateType.PurchaseOrder);
            ActionResult result = SaveReportUrl(reportItem.ReportGuid, reportItem.ToString(false), reportItem.ReportId, reportItem.ReportTemplateTypeId, this.ActorCompanyId);

            return result.Success ? reportItem.ToShortString(true) : string.Empty;
        }

        public string GetOrderPrintUrlSingle(int invoiceId, int reportId, List<int> emailRecipients, int languageId, string invoiceNr, int actorCustomerId, bool printTimeReport, bool includeOnlyInvoicedTime, OrderInvoiceRegistrationType registrationType, bool invoiceCopy, int emailTemplateId, bool asReminder)
        {
            var reportItem = GetBillingInvoiceReportDTOSingle(invoiceId, reportId, emailRecipients, languageId, invoiceNr, actorCustomerId, printTimeReport, includeOnlyInvoicedTime, registrationType, invoiceCopy, emailTemplateId, asReminder);
            ActionResult result = SaveReportUrl(reportItem.ReportGuid, reportItem.ToString(false), reportItem.ReportId, reportItem.ReportTemplateTypeId, ActorCompanyId);

            return result.Success ? reportItem.ToShortString(true) : string.Empty;
        }

        public string GetHouseholdPrintUrl(List<int> customerInvoiceRowIds, int reportId, int sysReportTemplateTypeId, int sequenceNumber, bool useGreen)
        {
            var reportItem = new HouseholdTaxDeductionReportDTO(base.ActorCompanyId, reportId, sysReportTemplateTypeId, base.ActorCompanyId, sequenceNumber, customerInvoiceRowIds, useGreen: useGreen);
            ActionResult result = SaveReportUrl(reportItem.ReportGuid, reportItem.ToString(false), reportItem.ReportId, reportItem.ReportTemplateTypeId, ActorCompanyId);

            return result.Success ? reportItem.ToShortString(true) : string.Empty;
        }

        public BillingInvoiceReportDTO GetInvoiceReminderReportDTOSingle(int invoiceId, int actorCompanyId, List<int> emailRecipients, int? emailTemplateId, int languageId, string invoiceNr)
        {
            CompanySettingType companySettingType = CompanySettingType.CustomerDefaultReminderTemplate;
            SoeReportTemplateType reportTemplateType = SoeReportTemplateType.BillingInvoiceReminder;
            int reportId = GetCompanySettingReportId(SettingMainType.Company, companySettingType, reportTemplateType, actorCompanyId, base.UserId);
            int selectedEmailTemplateId = emailTemplateId ?? 0;
            if (selectedEmailTemplateId == 0)
                selectedEmailTemplateId = this.SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.BillingDefaultEmailTemplate, 0, ActorCompanyId, 0);

            return new BillingInvoiceReportDTO(ActorCompanyId, reportId, (int)reportTemplateType, invoiceId, invoiceNr, false, true, false, false, false, languageId, emailTemplateId: selectedEmailTemplateId, emailFileName: invoiceNr, emailRecipients: emailRecipients, email: true);
        }

        public BillingInvoiceReportDTO GetBillingInvoiceReportDTOSingle(int invoiceId, int reportId, List<int> emailRecipients, int languageId, string invoiceNr, int actorCustomerId, bool printTimeReport, bool includeOnlyInvoicedTime, OrderInvoiceRegistrationType registrationType, bool invoiceCopy, int emailTemplateId, bool asReminder, string singleRecipient = "")
        {
            SoeReportTemplateType reportTemplateType;
            CompanySettingType companySettingType;
            CompanySettingType emailTemplateSettingType;

            switch (registrationType)
            {
                case OrderInvoiceRegistrationType.Offer:
                    reportTemplateType = SoeReportTemplateType.BillingOffer;
                    companySettingType = CompanySettingType.BillingDefaultOfferTemplate;
                    emailTemplateSettingType = CompanySettingType.BillingOfferDefaultEmailTemplate;
                    break;
                case OrderInvoiceRegistrationType.Order:
                    reportTemplateType = SoeReportTemplateType.BillingOrder;
                    companySettingType = CompanySettingType.BillingDefaultOrderTemplate;
                    emailTemplateSettingType = CompanySettingType.BillingOrderDefaultEmailTemplate;
                    break;
                case OrderInvoiceRegistrationType.Invoice:
                    reportTemplateType = SoeReportTemplateType.BillingInvoice;
                    companySettingType = CompanySettingType.BillingDefaultInvoiceTemplate;
                    emailTemplateSettingType = CompanySettingType.BillingDefaultEmailTemplate;
                    var customerInvoice = InvoiceManager.GetCustomerInvoiceSmallEx(invoiceId);
                    if (customerInvoice.CashSale)
                    {
                        companySettingType = CompanySettingType.BillingDefaultInvoiceTemplateCashSales;
                    }
                    break;
                case OrderInvoiceRegistrationType.Contract:
                    reportTemplateType = SoeReportTemplateType.BillingContract;
                    companySettingType = CompanySettingType.BillingDefaultContractTemplate;
                    emailTemplateSettingType = CompanySettingType.BillingContractDefaultEmailTemplate;
                    break;
                default:
                    throw new SoeGeneralException("GetOrderPrintUrl got unknown registrationtype Error!", this.ToString());
            }

            if (reportId == 0 && actorCustomerId > 0)
            {
                var cm = new CustomerManager(null);
                reportId = cm.GetCustomerInvoiceReportTemplate(actorCustomerId, registrationType);
            }
            reportId = reportId == 0 ? GetCompanySettingReportId(SettingMainType.Company, companySettingType, reportTemplateType, ActorCompanyId, UserId) : reportId;

            BillingInvoiceReportDTO reportItem;
            if ((emailRecipients != null && emailRecipients.Any()) || !singleRecipient.IsNullOrEmpty())
            {
                int selectedEmailTemplateId = emailTemplateId;
                if (selectedEmailTemplateId == 0)
                    selectedEmailTemplateId = this.SettingManager.GetIntSetting(SettingMainType.Company, (int)emailTemplateSettingType, 0, ActorCompanyId, 0);

                // Fallback
                if (selectedEmailTemplateId == 0)
                    selectedEmailTemplateId = this.SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.BillingDefaultEmailTemplate, 0, ActorCompanyId, 0);

                reportItem = new BillingInvoiceReportDTO(ActorCompanyId, reportId, (int)reportTemplateType, invoiceId, invoiceNr, invoiceCopy, asReminder, false, printTimeReport, includeOnlyInvoicedTime, languageId, emailTemplateId: selectedEmailTemplateId, emailFileName: invoiceNr, emailRecipients: emailRecipients, email: true, singleRecipient: singleRecipient);
            }
            else
            {
                reportItem = new BillingInvoiceReportDTO(ActorCompanyId, reportId, (int)reportTemplateType, invoiceId, invoiceNr, invoiceCopy, asReminder, false, printTimeReport, includeOnlyInvoicedTime, languageId);
            }

            return reportItem;
        }

        public BillingInvoiceReportDTO GetBillingInvoiceReportDTO(List<int> invoiceIds, int reportId, List<int> emailRecipients, int languageId, string invoiceNr, int actorCustomerId, OrderInvoiceRegistrationType registrationType, bool invoiceCopy)
        {
            SoeReportTemplateType reportTemplateType;
            CompanySettingType companySettingType;
            CompanySettingType emailTemplateSettingType;
            switch (registrationType)
            {
                case OrderInvoiceRegistrationType.Order:
                    reportTemplateType = SoeReportTemplateType.BillingOrder;
                    companySettingType = CompanySettingType.BillingDefaultOrderTemplate;
                    emailTemplateSettingType = CompanySettingType.BillingOrderDefaultEmailTemplate;
                    break;
                case OrderInvoiceRegistrationType.Invoice:
                    reportTemplateType = SoeReportTemplateType.BillingInvoice;
                    companySettingType = CompanySettingType.BillingDefaultInvoiceTemplate;
                    emailTemplateSettingType = CompanySettingType.BillingDefaultEmailTemplate;
                    break;
                default:
                    throw new SoeGeneralException("GetOrderPrintUrl got unknown registrationtype Error!", this.ToString());
            }

            if (reportId == 0 && actorCustomerId > 0)
                reportId = CustomerManager.GetCustomerInvoiceReportTemplate(actorCustomerId, registrationType);
            reportId = reportId == 0 ? GetCompanySettingReportId(SettingMainType.Company, companySettingType, reportTemplateType, ActorCompanyId, UserId) : reportId;

            var includeTimeProjectReport = false;
            var includeOnlyInvoicedTimeInTimeProjectReport = false;
            if (registrationType == OrderInvoiceRegistrationType.Order)
            {
                includeTimeProjectReport = true; //this.SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.BillingOrderIncludeTimeProjectinReport, 0, base.ActorCompanyId, 0); -REMOVED ITEM 49453
                includeOnlyInvoicedTimeInTimeProjectReport = this.SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.ProjectIncludeOnlyInvoicedTimeInTimeProjectReport, 0, base.ActorCompanyId, 0, true);
            }
            else if (registrationType == OrderInvoiceRegistrationType.Invoice)
            {
                includeTimeProjectReport = true; //this.SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.BillingIncludeTimeProjectinReport, 0, base.ActorCompanyId, 0); - REMOVED ITEM 49453
                includeOnlyInvoicedTimeInTimeProjectReport = this.SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.ProjectIncludeOnlyInvoicedTimeInTimeProjectReport, 0, base.ActorCompanyId, 0, true);
            }

            BillingInvoiceReportDTO reportItem;
            if (emailRecipients != null && emailRecipients.Any())
            {
                var emailTmplId = this.SettingManager.GetIntSetting(SettingMainType.Company, (int)emailTemplateSettingType, 0, ActorCompanyId, 0);// Fallback
                if (emailTmplId == 0)
                    emailTmplId = this.SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.BillingDefaultEmailTemplate, 0, ActorCompanyId, 0);

                reportItem = new BillingInvoiceReportDTO(ActorCompanyId, reportId, (int)reportTemplateType, invoiceIds, invoiceCopy, false, includeProjectReport: includeTimeProjectReport, includeOnlyInvoiced: includeOnlyInvoicedTimeInTimeProjectReport, emailTemplateId: emailTmplId, emailFileName: invoiceNr, emailRecipients: emailRecipients, email: true);
            }
            else
            {
                reportItem = new BillingInvoiceReportDTO(ActorCompanyId, reportId, (int)reportTemplateType, invoiceIds, invoiceCopy, false, includeProjectReport: includeTimeProjectReport, includeOnlyInvoiced: includeOnlyInvoicedTimeInTimeProjectReport);
            }

            return reportItem;
        }

        public string GetDefaultAccountingOrderPrintUrl(int voucherHeadId)
        {
            CompanySettingType companySettingType = CompanySettingType.AccountingDefaultAccountingOrder;
            SoeReportTemplateType reportTemplatetype = SoeReportTemplateType.VoucherList;
            int reportId = GetCompanySettingReportId(SettingMainType.Company, companySettingType, reportTemplatetype, base.ActorCompanyId, base.UserId);

            var reportItem = new VoucherListReportDTO(ActorCompanyId, reportId, (int)reportTemplatetype, voucherHeadId, true);
            ActionResult result = SaveReportUrl(reportItem.ReportGuid, reportItem.ToString(false), reportId, (int)SoeReportTemplateType.VoucherList, base.ActorCompanyId);

            return result.Success ? reportItem.ToShortString(true) : string.Empty;
        }

        public string GetPayrollProductListPrintUrl(List<int> productIds, int reportId, int sysReportTemplateType)
        {
            var reportItem = new PayrollProductReportDTO(base.ActorCompanyId, reportId, sysReportTemplateType, (int)TermGroup_ReportExportType.Pdf, productIds);
            ActionResult result = SaveReportUrl(reportItem.ReportGuid, reportItem.ToString(false), reportId, sysReportTemplateType, base.ActorCompanyId);

            return result.Success ? reportItem.ToShortString(true) : string.Empty;
        }

        public string GetTimeEmployeeSchedulePrintUrl(List<int> employeeIds, List<int> shiftTypeIds, DateTime startDate, DateTime stopDate, int reportId, SoeReportTemplateType reportTemplateType)
        {
            var reportItem = new TimeEmployeeReportDTO(base.ActorCompanyId, reportId, (int)reportTemplateType, startDate, stopDate, 0, shiftTypeIds, employeeIds, null, null, null, false);
            ActionResult result = SaveReportUrl(reportItem.ReportGuid, reportItem.ToString(false), reportId, (int)reportTemplateType, base.ActorCompanyId);

            return result.Success ? reportItem.ToShortString(true) : string.Empty;
        }

        public string GetTimeScheduleTasksAndDeliverysReportPrintUrl(List<int> timeScheduleTaskIds, List<int> timeScheduleDeliveryHeadIds, DateTime startDate, DateTime stopDate, bool isDayView)
        {
            CompanySettingType companySettingType = isDayView ? CompanySettingType.TimeDefaultScheduleTasksAndDeliverysDayReport : CompanySettingType.TimeDefaultScheduleTasksAndDeliverysWeekReport;
            SoeReportTemplateType reportTemplatetype = SoeReportTemplateType.TimeScheduleTasksAndDeliverysReport;
            int reportId = GetCompanySettingReportId(SettingMainType.Company, companySettingType, reportTemplatetype, base.ActorCompanyId, base.UserId);

            var reportItem = new TimeScheduleTasksAndDeliverysReportDTO(base.ActorCompanyId, reportId, (int)reportTemplatetype, startDate, stopDate, timeScheduleTaskIds, timeScheduleDeliveryHeadIds, false);
            ActionResult result = SaveReportUrl(reportItem.ReportGuid, reportItem.ToString(false), reportId, (int)reportTemplatetype, base.ActorCompanyId);

            return result.Success ? reportItem.ToShortString(true) : string.Empty;
        }

        public string GetStockInventoryReportPrintUrl(List<int> stockInventoryIds, int reportId)
        {
            int langId = GetLangId();
            var reportItem = new StockInventoryReportDTO(base.ActorCompanyId, stockInventoryIds, reportId, langId, (int)SoeReportTemplateType.StockInventoryReport);
            ActionResult result = SaveReportUrl(reportItem.ReportGuid, reportItem.ToString(false), reportId, (int)SoeReportTemplateType.StockInventoryReport, base.ActorCompanyId);
            return result.Success ? reportItem.ToShortString(true) : string.Empty;
        }

        #endregion

        #region ReportPackage

        public List<ReportPackage> GetReportPackages(int actorCompanyId, bool loadReport)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Report.NoTracking();
            return GetReportPackages(entities, actorCompanyId, loadReport);
        }

        public List<ReportPackage> GetReportPackages(CompEntities entities, int actorCompanyId, bool loadReport)
        {
            List<ReportPackage> reportPackages = (from rp in entities.ReportPackage
                                                  where ((rp.Company.ActorCompanyId == actorCompanyId) &&
                                                  (rp.State == (int)SoeEntityState.Active))
                                                  select rp).ToList();

            if (loadReport)
            {
                foreach (ReportPackage reportPackage in reportPackages)
                {
                    if (reportPackage.Report != null && !reportPackage.Report.IsLoaded)
                        reportPackage.Report.Load();
                }
            }

            return reportPackages;
        }

        public IEnumerable<ReportPackage> GetReportPackagesForModule(int actorCompanyId, int module, bool loadReport)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Report.NoTracking();
            return GetReportPackagesForModule(entities, actorCompanyId, module, loadReport);
        }

        public IEnumerable<ReportPackage> GetReportPackagesForModule(CompEntities entities, int actorCompanyId, int module, bool loadReport)
        {
            List<ReportPackage> reportPackages = (from rp in entities.ReportPackage
                                                  where ((rp.Company.ActorCompanyId == actorCompanyId) &&
                                                  (rp.Module == module || rp.Module == (int)SoeModule.None) &&
                                                  (rp.State == (int)SoeEntityState.Active))
                                                  select rp).ToList();

            if (loadReport)
            {
                foreach (ReportPackage reportPackage in reportPackages)
                {
                    if (reportPackage.Report != null && !reportPackage.Report.IsLoaded)
                        reportPackage.Report.Load();
                }
            }

            return reportPackages;
        }

        public ReportPackage GetReportPackage(int reportPackageId, int actorCompanyId, bool loadReport, bool loadReportSelection)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.ReportPackage.NoTracking();
            return GetReportPackage(entities, reportPackageId, actorCompanyId, loadReport, loadReportSelection);
        }

        public ReportPackage GetReportPackage(CompEntities entities, int reportPackageId, int actorCompanyId, bool loadReport, bool loadReportSelection)
        {
            ReportPackage reportPackage = (from rp in entities.ReportPackage
                                           where rp.ReportPackageId == reportPackageId &&
                                           rp.Company.ActorCompanyId == actorCompanyId &&
                                           rp.State == (int)SoeEntityState.Active
                                           select rp).FirstOrDefault();

            if (reportPackage != null && loadReport)
            {
                if (reportPackage.Report != null && !reportPackage.Report.IsLoaded)
                    reportPackage.Report.Load();

                if (loadReportSelection)
                {
                    foreach (Report report in reportPackage.Report)
                    {
                        if (!report.ReportSelectionReference.IsLoaded)
                            report.ReportSelectionReference.Load();

                        if (report.ReportSelection != null)
                        {
                            if (!report.ReportSelection.ReportSelectionInt.IsLoaded)
                                report.ReportSelection.ReportSelectionInt.Load();
                            if (!report.ReportSelection.ReportSelectionStr.IsLoaded)
                                report.ReportSelection.ReportSelectionStr.Load();
                            if (!report.ReportSelection.ReportSelectionDate.IsLoaded)
                                report.ReportSelection.ReportSelectionDate.Load();
                        }
                    }
                }
            }

            return reportPackage;
        }

        public ReportPackage GetPrevNextReportPackage(int reportPackageId, int module, int actorCompanyId, SoeFormMode mode)
        {
            ReportPackage reportPackage = null;
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            entitiesReadOnly.Report.NoTracking();
            if (mode == SoeFormMode.Next)
            {
                reportPackage = (from rp in entitiesReadOnly.ReportPackage
                                 where rp.ReportPackageId > reportPackageId &&
                                 rp.Module == module &&
                                 rp.Company.ActorCompanyId == actorCompanyId &&
                                 rp.State == (int)SoeEntityState.Active
                                 orderby rp.ReportPackageId ascending
                                 select rp).FirstOrDefault();
            }
            else if (mode == SoeFormMode.Prev)
            {
                reportPackage = (from rp in entitiesReadOnly.ReportPackage
                                 where rp.ReportPackageId < reportPackageId &&
                                 rp.Module == module &&
                                 rp.Company.ActorCompanyId == actorCompanyId &&
                                 rp.State == (int)SoeEntityState.Active
                                 orderby rp.ReportPackageId descending
                                 select rp).FirstOrDefault();
            }

            return reportPackage;
        }

        public bool ReportExistInReportPackage(int reportPackageId, int reportId, int actorCompanyId)
        {
            using (CompEntities entities = new CompEntities())
            {
                Report report = GetReport(entities, reportId, actorCompanyId);
                if (report == null)
                    return false;

                ReportPackage reportPackage = GetReportPackage(entities, reportPackageId, actorCompanyId, true, false);
                if (reportPackage != null && reportPackage.Report.Contains(report))
                    return true;

                return false;
            }
        }

        public ActionResult AddReportPackage(ReportPackage reportPackage, int actorCompanyId)
        {
            if (reportPackage == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "ReportPackage");

            using (CompEntities entities = new CompEntities())
            {
                reportPackage.Company = CompanyManager.GetCompany(entities, actorCompanyId);
                if (reportPackage.Company == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte"));

                return AddEntityItem(entities, reportPackage, "ReportPackage");
            }
        }

        public ActionResult UpdateReportPackage(ReportPackage reportPackage, int[] reportArr, int actorCompanyId)
        {
            if (reportPackage == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "ReportPackage");

            using (CompEntities entities = new CompEntities())
            {
                ReportPackage originalReportPackage = GetReportPackage(entities, reportPackage.ReportPackageId, actorCompanyId, true, false);
                if (originalReportPackage == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte"));

                for (int i = 0; i < reportArr.Length; i++)
                {
                    int reportId = reportArr[i];
                    if (reportId > 0)
                    {
                        Report report = GetReport(entities, reportId, actorCompanyId);
                        if (report != null && !originalReportPackage.Report.Contains(report))
                            originalReportPackage.Report.Add(report);
                    }
                }

                return UpdateEntityItem(entities, originalReportPackage, reportPackage, "ReportPackage");
            }
        }

        /// <summary>
        /// Sets a ReportPackage to Deleted
        /// </summary>
        /// <param name="reportPackage">ReportPackage to delete</param>
        /// <param name="actorCompanyId">The ActorCompanyId</param>
        /// <returns>ActionResult</returns>
        public ActionResult DeleteReportPackage(ReportPackage reportPackage, int actorCompanyId)
        {
            if (reportPackage == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, "ReportPackage");

            using (CompEntities entities = new CompEntities())
            {
                ReportPackage originalReportPackage = GetReportPackage(entities, reportPackage.ReportPackageId, actorCompanyId, false, false);
                if (originalReportPackage == null)
                    return new ActionResult((int)ActionResultDelete.EntityNotFound, "ReportPackage");

                return ChangeEntityState(entities, originalReportPackage, SoeEntityState.Deleted, true);
            }
        }

        public ActionResult DeleteReportPackageMapping(int reportPackageId, int reportId, int actorCompanyId)
        {
            using (CompEntities entities = new CompEntities())
            {
                ReportPackage reportPackage = GetReportPackage(entities, reportPackageId, actorCompanyId, false, false);
                if (reportPackage == null)
                    return new ActionResult((int)ActionResultDelete.EntityNotFound, "ReportPackage");

                if (!reportPackage.Report.IsLoaded)
                    reportPackage.Report.Load();

                foreach (Report report in reportPackage.Report)
                {
                    if (report.ReportId == reportId)
                    {
                        reportPackage.Report.Remove(report);
                        break;
                    }
                }

                return SaveDeletions(entities);
            }
        }

        #endregion

        #region ReportTemplate

        public List<ReportTemplate> GetReportTemplates(int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.ReportTemplate.NoTracking();
            return GetReportTemplates(entities, actorCompanyId);
        }

        public List<ReportTemplate> GetReportTemplates(CompEntities entities, int actorCompanyId)
        {
            return (from rt in entities.ReportTemplate
                    where rt.Company.ActorCompanyId == actorCompanyId &&
                    rt.State == (int)SoeEntityState.Active
                    select rt).ToList();
        }

        public List<ReportTemplate> GetReportTemplatesForModule(int actorCompanyId, int module)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.ReportTemplate.NoTracking();
            return GetReportTemplatesForModule(entities, actorCompanyId, module);
        }

        public List<ReportTemplate> GetReportTemplatesForModule(CompEntities entities, int actorCompanyId, int module)
        {
            List<ReportTemplate> validReportTemplates = new List<ReportTemplate>();
            entities.CommandTimeout = 36000;
            List<ReportTemplate> reportTemplates = (from t in entities.ReportTemplate
                                                    where t.Company.ActorCompanyId == actorCompanyId &&
                                                    (t.Module == module || t.Module == (int)SoeModule.None) &&
                                                    t.State == (int)SoeEntityState.Active
                                                    select t).ToList();


            if (!reportTemplates.IsNullOrEmpty())
            {
                List<SysReportTemplateType> sysReportTemplateTypes = GetSysReportTemplateTypesFromCache(module);
                List<GenericType> sysReportTemplateGroups = GetTermGroupContent(TermGroup.SysReportTemplateTypeGroup, skipUnknown: true);

                foreach (ReportTemplate reportTemplate in reportTemplates)
                {
                    SysReportTemplateType sysReportTemplateType = sysReportTemplateTypes.FirstOrDefault();
                    if (sysReportTemplateType == null)
                        continue;

                    reportTemplate.Name = sysReportTemplateTypes.GetGroupName(sysReportTemplateGroups, sysReportTemplateType.SysReportTemplateTypeId) + reportTemplate.Name;
                    validReportTemplates.Add(reportTemplate);
                }
            }

            return validReportTemplates;
        }

        public Dictionary<int, string> GetReportTemplatesForModuleDict(int actorCompanyId, int module, bool addEmptyRow)
        {
            Dictionary<int, string> dict = new Dictionary<int, string>();

            if (addEmptyRow)
                dict.Add(0, " ");

            Dictionary<int, string> sysReportTemplateTypeTerms = base.GetTermGroupDict(TermGroup.SysReportTemplateType);
            sysReportTemplateTypeTerms.Remove((int)SoeReportTemplateType.Generic);// Remove the general type   
            Dictionary<int, string> sysReportTemplateTypesDict = GetSysReportTemplateTypesForModuleDict(module, sysReportTemplateTypeTerms, false);

            List<ReportTemplate> reportTemplates = GetReportTemplatesForModule(actorCompanyId, module);
            foreach (ReportTemplate reportTemplate in reportTemplates.OrderBy(i => i.Name))
            {
                string value = "";

                if (sysReportTemplateTypesDict.ContainsKey(reportTemplate.SysReportTypeId))
                    value = sysReportTemplateTypesDict[reportTemplate.SysReportTypeId] + " ";
                value += !String.IsNullOrEmpty(reportTemplate.Description) ? reportTemplate.Description : reportTemplate.Name;

                dict.Add(reportTemplate.ReportTemplateId, value);
            }

            return dict;
        }

        public List<ReportTemplateGridDTO> GetReportTemplatesGridDTOsForModule(int actorCompanyId, int module)
        {
            List<ReportTemplateGridDTO> dtos = new List<ReportTemplateGridDTO>();
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();

            var reportTemplates = (from t in entitiesReadOnly.ReportTemplate
                                   where t.Company.ActorCompanyId == actorCompanyId &&
                                   (t.Module == module || t.Module == (int)SoeModule.None) &&
                                   t.State == (int)SoeEntityState.Active
                                   select new
                                   {
                                       t.ReportTemplateId,
                                       t.SysTemplateTypeId,
                                       t.ReportNr,
                                       t.Name,
                                       t.Description,
                                   }).ToList();

            if (!reportTemplates.IsNullOrEmpty())
            {
                List<SysReportTemplateType> sysReportTemplateTypes = GetSysReportTemplateTypesFromCache(module);
                List<GenericType> sysReportTemplateGroups = GetTermGroupContent(TermGroup.SysReportTemplateTypeGroup, skipUnknown: true);

                foreach (var reportTemplate in reportTemplates)
                {
                    SysReportTemplateType sysReportTemplateType = sysReportTemplateTypes.FirstOrDefault(i => i.SysReportTemplateTypeId == reportTemplate.SysTemplateTypeId);
                    if (sysReportTemplateType == null)
                        continue;

                    ReportTemplateGridDTO dto = new ReportTemplateGridDTO();
                    dto.ReportTemplateId = reportTemplate.ReportTemplateId;
                    dto.ReportNr = reportTemplate.ReportNr;
                    dto.Name = reportTemplate.Name;
                    dto.Description = reportTemplate.Description;
                    dto.SysReportTemplateGroupName = sysReportTemplateTypes.GetGroupName(sysReportTemplateGroups, sysReportTemplateType.SysReportTemplateTypeId);
                    dto.SysReportTemplateTypeName = GetText(sysReportTemplateType.SysReportTermId, (int)TermGroup.SysReportTemplateType);
                    dto.CombinedDisplayName = dto.SysReportTemplateGroupName + " - ";
                    if (!String.IsNullOrEmpty(dto.SysReportTemplateTypeName))
                        dto.CombinedDisplayName += dto.SysReportTemplateTypeName + " ,";
                    dto.CombinedDisplayName += dto.Name;
                    if (dto.ReportNr.HasValue)
                        dto.CombinedDisplayName += " (" + dto.ReportNr.Value.ToString() + ")";
                    dtos.Add(dto);
                }
            }

            return dtos.OrderBy(i => i.SysReportTemplateTypeName).ToList();
        }

        public ReportTemplate GetReportTemplate(int reportTemplateId, int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.ReportTemplate.NoTracking();
            return GetReportTemplate(entities, reportTemplateId, actorCompanyId);
        }

        public ReportTemplate GetReportTemplate(CompEntities entities, int reportTemplateId, int actorCompanyId)
        {
            return (from rt in entities.ReportTemplate
                    where rt.ReportTemplateId == reportTemplateId &&
                    rt.Company.ActorCompanyId == actorCompanyId &&
                    rt.State == (int)SoeEntityState.Active
                    select rt).FirstOrDefault();
        }

        public int? GetReportTemplateTypeId(int reportTemplateId, int actorCompanyId)
        {
            string key = $"GetReportTemplateTypeId#{reportTemplateId}#{actorCompanyId}";
            var value = BusinessMemoryCache<int?>.Get(key);

            if (value.HasValue)
            {
                if (value == 0)
                    return null;
                return value;
            }

            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            entitiesReadOnly.ReportTemplate.NoTracking();
            value = GetReportTemplateTypeId(entitiesReadOnly, reportTemplateId, actorCompanyId);

            if (!value.HasValue)
                value = 0;

            BusinessMemoryCache<int?>.Set(key, value, 60 * 5);

            return value;
        }

        public int? GetReportTemplateTypeId(CompEntities entities, int reportTemplateId, int actorCompanyId)
        {
            return (from rt in entities.ReportTemplate
                    where rt.ReportTemplateId == reportTemplateId &&
                    rt.Company.ActorCompanyId == actorCompanyId &&
                    rt.State == (int)SoeEntityState.Active
                    select rt.SysTemplateTypeId)?.FirstOrDefault();
        }

        public List<GenericType> GetExportTypes()
        {
            return GetTermGroupContent(TermGroup.ReportExportType);
        }

        /// <summary>
        /// Checks whether a ReportTemplate is used in any Report
        /// </summary>
        /// <param name="company">The ReportTemplate to check</param>
        /// <returns>True if the ReportTemplate is used, otherwise false</returns>
        public bool ReportTemplateHasReports(int reportTemplateId)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            int counter = (from r in entitiesReadOnly.Report
                           where r.ReportTemplateId == reportTemplateId &&
                           r.State == (int)SoeEntityState.Active
                           select r).Count();

            if (counter > 0)
                return true;
            return false;
        }

        public bool ReportTemplateExist(string name, int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.ReportTemplate.NoTracking();
            int counter = (from rt in entities.ReportTemplate
                           where rt.Name.ToLower() == name.ToLower() &&
                           rt.Company.ActorCompanyId == actorCompanyId &&
                           rt.State == (int)SoeEntityState.Active
                           select rt).Count();

            if (counter > 0)
                return true;
            return false;
        }

        public ActionResult SaveReportTemplate(ReportTemplateDTO reportTemplateInput, byte[] templateData, int actorCompanyId)
        {
            ActionResult result = new ActionResult(true);

            if (reportTemplateInput == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "ReportTemplate");

            int reportTemplateId = reportTemplateInput.ReportTemplateId;

            #region Prereq

            if (!reportTemplateInput.SysReportTemplateTypeId.HasValue || reportTemplateInput.SysReportTemplateTypeId.Value == 0)
                return new ActionResult((int)ActionResultSave.EntityIsNull, GetText(11771, "Typ måste anges"));
            if (!reportTemplateInput.SysReportTypeId.HasValue || reportTemplateInput.SysReportTypeId.Value == 0)
                reportTemplateInput.SysReportTypeId = ReportManager.GetSysReportTypeBySysReportTemplate(reportTemplateId)?.SysReportTypeId ?? (int?)SoeReportType.CrystalReport;

            if (templateData != null && !ReportGenManager.ValidateReportTemplate(reportTemplateInput.SysReportTemplateTypeId.Value, templateData, (SoeReportType)reportTemplateInput.SysReportTypeId))
                return new ActionResult((int)ActionResultSave.NothingSaved, GetText(1339, "Felaktig fil, kunde inte valideras"));

            #endregion

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        #region Prereq

                        Company company = CompanyManager.GetCompany(entities, actorCompanyId);
                        if (company == null)
                            return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte"));

                        #endregion

                        #region ReportTemplate

                        ReportTemplate reportTemplate = GetReportTemplate(entities, reportTemplateInput.ReportTemplateId, actorCompanyId);
                        if (reportTemplate == null)
                        {
                            #region Add

                            if (templateData == null)
                                return new ActionResult((int)ActionResultSave.EntityIsNull, GetText(11772, "Mall måste laddas upp"));

                            reportTemplate = new ReportTemplate()
                            {
                                SysReportTypeId = (int)SoeReportType.CrystalReport,
                                Company = company,
                            };
                            SetCreatedProperties(reportTemplate);
                            entities.ReportTemplate.AddObject(reportTemplate);

                            #endregion
                        }
                        else
                        {
                            SetModifiedProperties(reportTemplate);
                        }

                        reportTemplate.SysTemplateTypeId = reportTemplateInput.SysReportTemplateTypeId.Value;
                        reportTemplate.ReportNr = reportTemplateInput.ReportNr;
                        reportTemplate.Name = reportTemplateInput.Name;
                        reportTemplate.Description = reportTemplateInput.Description;
                        reportTemplate.Module = (int)reportTemplateInput.Module;
                        reportTemplate.FileName = reportTemplateInput.FileName;
                        reportTemplate.GroupByLevel1 = reportTemplateInput.GroupByLevel1;
                        reportTemplate.GroupByLevel2 = reportTemplateInput.GroupByLevel2;
                        reportTemplate.GroupByLevel3 = reportTemplateInput.GroupByLevel3;
                        reportTemplate.GroupByLevel4 = reportTemplateInput.GroupByLevel4;
                        reportTemplate.SortByLevel1 = reportTemplateInput.SortByLevel1;
                        reportTemplate.SortByLevel2 = reportTemplateInput.SortByLevel2;
                        reportTemplate.SortByLevel3 = reportTemplateInput.SortByLevel3;
                        reportTemplate.SortByLevel4 = reportTemplateInput.SortByLevel4;
                        reportTemplate.Special = reportTemplateInput.Special;
                        reportTemplate.IsSortAscending = reportTemplateInput.IsSortAscending;
                        reportTemplate.ShowGroupingAndSorting = reportTemplateInput.ShowGroupingAndSorting;
                        reportTemplate.ShowOnlyTotals = reportTemplateInput.ShowOnlyTotals;
                        reportTemplate.ValidExportTypes = reportTemplateInput.ValidExportTypes.ToCommaSeparated();

                        if (templateData != null)
                            reportTemplate.Template = templateData;

                        #endregion

                        result = SaveChanges(entities, transaction);
                        if (result.Success)
                        {
                            //Commit transaction
                            transaction.Complete();

                            reportTemplateId = reportTemplate.ReportTemplateId;
                        }
                    }
                }
                catch (Exception ex)
                {
                    base.LogError(ex, this.log);
                    result.Exception = ex;
                    result.IntegerValue = 0;
                }
                finally
                {
                    if (result.Success)
                    {
                        //Set success properties
                        result.IntegerValue = reportTemplateId;
                    }
                    else
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }
            }

            return result;
        }

        public ActionResult AddReportTemplate(ReportTemplate reportTemplate, int actorCompanyId)
        {
            if (reportTemplate == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "ReportTemplate");

            using (CompEntities entities = new CompEntities())
            {
                reportTemplate.Company = CompanyManager.GetCompany(entities, actorCompanyId);
                if (reportTemplate.Company == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte"));

                return AddEntityItem(entities, reportTemplate, "ReportTemplate");
            }
        }

        public ActionResult UpdateReportTemplate(ReportTemplate reportTemplate, int actorCompanyId)
        {
            if (reportTemplate == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "ReportTemplate");

            using (CompEntities entities = new CompEntities())
            {
                ReportTemplate originalReportTemplate = GetReportTemplate(entities, reportTemplate.ReportTemplateId, actorCompanyId);
                if (originalReportTemplate == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "ReportTemplate");

                return this.UpdateEntityItem(entities, originalReportTemplate, reportTemplate, "ReportTemplate");
            }
        }

        /// <summary>
        /// Sets a ReportTemplate to Deleted
        /// </summary>
        /// <param name="reportTemplate">ReportTemplate to delete</param>
        /// <param name="actorCompanyId">The ActorCompanyId</param>
        /// <returns>ActionResult</returns>
        public ActionResult DeleteReportTemplate(int reportTemplateId, int actorCompanyId)
        {
            if (ReportTemplateHasReports(reportTemplateId))
                return new ActionResult((int)ActionResultDelete.ReportTemplateHasReports);

            using (CompEntities entities = new CompEntities())
            {
                ReportTemplate originalReportTemplate = GetReportTemplate(entities, reportTemplateId, actorCompanyId);
                if (originalReportTemplate == null)
                    return new ActionResult((int)ActionResultDelete.EntityNotFound, "ReportTemplate");

                return ChangeEntityState(entities, originalReportTemplate, SoeEntityState.Deleted, true);
            }
        }

        #endregion

        #region ReportTemplateCountryHandling

        public List<SysLinkTable> GetSysLinks(SOESysEntities entities, int recordType, int recordId)
        {
            return (from lt in entities.SysLinkTable
                    where lt.SysLinkTableRecordType == recordType &&
                    lt.SysLinkTableKeyItemId == recordId
                    select lt).ToList();
        }

        public List<SysLinkDTO> GetSysReportTemplateCountries(int recordId)
        {
            using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
            return GetSysReportTemplateCountries(sysEntitiesReadOnly, recordId);
        }

        public List<SysLinkDTO> GetSysReportTemplateCountries(SOESysEntities entities, int recordId)
        {
            var dtos = new List<SysLinkDTO>();

            List<SysCountry> sysCountries = CountryCurrencyManager.GetSysCountries(entities, true);
            List<SysLinkTable> sysLinks = GetSysLinks(entities, (int)SysLinkTableRecordType.LinkSysReportTemplateToCountryId, recordId).ToList();

            foreach (SysLinkTable sysLink in sysLinks)
            {
                SysCountry sysCountry = sysCountries.FirstOrDefault(c => c.SysCountryId == sysLink.SysLinkTableIntegerValue);
                dtos.Add(sysLink.ToDTO(sysCountry != null ? sysCountry.Name : ""));
            }
            return dtos;
        }

        public ActionResult DeleteSysLink(SOESysEntities entities, SysLinkTableRecordType type, SysLinkTableIntegerValueType intValueType, int sysLinkTableKeyItemId, int sysLinkTableIntegerValue)
        {
            var sysLinks = (from t in entities.SysLinkTable
                            where t.SysLinkTableRecordType == (int)type &&
                            t.SysLinkTableIntegerValueType == (int)intValueType &&
                            t.SysLinkTableKeyItemId == sysLinkTableKeyItemId &&
                            t.SysLinkTableIntegerValue == sysLinkTableIntegerValue
                            select t).ToList();

            foreach (var sysLink in sysLinks)
            {
                entities.SysLinkTable.Remove(sysLink);
            }

            return SaveChanges(entities);
        }

        #endregion

        #region ReportRolePermission

        public List<ReportRolePermission> GetReportRolePermissionsByCompany(int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.ReportRolePermission.NoTracking();
            return GetReportRolePermissionsByCompany(entities, actorCompanyId);
        }

        public List<ReportRolePermission> GetReportRolePermissionsByCompany(CompEntities entities, int actorCompanyId)
        {
            return (from p in entities.ReportRolePermission
                    where p.ActorCompanyId == actorCompanyId &&
                    p.State == (int)SoeEntityState.Active
                    select p).ToList();
        }

        public List<ReportRolePermission> GetReportRolePermissionByReport(int reportId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.ReportRolePermission.NoTracking();
            return GetReportRolePermissionsByReport(entities, reportId);
        }

        public List<ReportRolePermission> GetReportRolePermissionsByReport(CompEntities entities, int reportId)
        {
            return (from p in entities.ReportRolePermission
                    where p.ReportId == reportId &&
                    p.State == (int)SoeEntityState.Active
                    select p).ToList();
        }

        public bool HasReportRolePermission(List<int> reportIds, int roleId, int actorCompanyId)
        {
            return HasReportRolePermission(GetReportRolePermissionsByCompany(actorCompanyId), reportIds, roleId);
        }

        public bool HasReportRolePermission(List<ReportRolePermission> permissions, List<int> reportIds, int roleId)
        {
            if (permissions == null)
                return true;

            //Must have permissios for each Report
            foreach (int reportId in reportIds)
            {
                if (!permissions.HasReportRolePermission(reportId, roleId))
                    return false;
            }

            return true;
        }

        public bool HasReportRolePermission(int reportId, int roleId, CompEntities entities = null, SoeReportTemplateType reportTemplateType = SoeReportTemplateType.Unknown)
        {
            string key = $"HasReportRolePermission#{reportId}#{roleId}#{reportTemplateType}";
            var value = BusinessMemoryCache<bool?>.Get(key);

            if (value.HasValue)
                return value.Value;

            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            if (entities == null)
                entities = entitiesReadOnly;

            int actorCompanyId;
            if (base.ActorCompanyId == 0)
                actorCompanyId = entities.Report.First(f => f.ReportId == reportId).ActorCompanyId;
            else
                actorCompanyId = base.ActorCompanyId;

            List<ReportRolePermissionDTO> permissions = GetReportRolePermissionsFromCache(entities, CacheConfig.Company(actorCompanyId, 60));

            if (reportTemplateType != SoeReportTemplateType.PayrollSlip && reportTemplateType != SoeReportTemplateType.TimeSalarySpecificationReport)
            {
                value = permissions.HasReportRolePermission(reportId, roleId);
            }
            else
            {
                bool hasTimeTimeTimeSalarySpecificationPermission = FeatureManager.HasRolePermission(Feature.Time_Time_TimeSalarySpecification, Permission.Readonly, roleId, actorCompanyId);
                bool hasPermissionToPayroll = FeatureManager.HasRolePermission(Feature.Time_Payroll_Calculation_Edit, Permission.Readonly, roleId, actorCompanyId);
                if (hasTimeTimeTimeSalarySpecificationPermission && !hasPermissionToPayroll)
                {
                    int timePayrollSlipReportId = GetCompanySettingReportId(SettingMainType.Company, CompanySettingType.DefaultPayrollSlipReport, SoeReportTemplateType.PayrollSlip, actorCompanyId, 0, null);
                    int timeSalarySpecificationReportId = GetCompanySettingReportId(SettingMainType.Company, CompanySettingType.TimeDefaultTimeSalarySpecificationReport, SoeReportTemplateType.TimeSalarySpecificationReport, actorCompanyId, 0, null);
                    value = timePayrollSlipReportId == reportId || timeSalarySpecificationReportId == reportId;
                }
                else
                {
                    value = permissions.HasReportRolePermission(reportId, roleId);
                }
            }

            BusinessMemoryCache<bool?>.Set(key, value.Value, 20);

            return value.Value;
        }

        public bool HasReportRolePermission(SettingMainType settingMainType, CompanySettingType settingType, SoeReportTemplateType reportTemplateType, int actorCompanyId, int userId, int? roleId = null)
        {
            int defaultReportId = SettingManager.GetIntSetting(settingMainType, (int)settingType, userId, actorCompanyId, 0);
            if (defaultReportId > 0 && roleId.HasValue)
            {
                List<ReportRolePermission> permissions = GetReportRolePermissionByReport(defaultReportId);
                if (permissions.HasReportRolePermission(defaultReportId, roleId))
                    return true;
            }

            return false;
        }

        public ActionResult SaveReportRolePermission(List<ReportRolePermission> inputPermissions, int reportId, int actorCompanyId)
        {
            if (inputPermissions == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "AttestRoleUser");

            using (CompEntities entities = new CompEntities())
            {
                List<ReportRolePermission> existingPermissions = GetReportRolePermissionsByReport(entities, reportId);

                #region Update/Delete

                foreach (ReportRolePermission existingPermission in existingPermissions)
                {
                    ReportRolePermission permission = inputPermissions.FirstOrDefault(p => p.ReportId == existingPermission.ReportId && p.RoleId == existingPermission.RoleId);
                    if (permission != null)
                    {
                        //Update
                        SetModifiedProperties(existingPermission);
                        inputPermissions.Remove(permission);
                    }
                    else
                    {
                        //Delete
                        entities.DeleteObject(existingPermission);
                    }
                }

                #endregion

                #region Add

                //Add remaining input items
                foreach (ReportRolePermission inputPermission in inputPermissions)
                {
                    ReportRolePermission permission = new ReportRolePermission()
                    {
                        //Set FK
                        ReportId = reportId,
                        RoleId = inputPermission.RoleId,
                        ActorCompanyId = actorCompanyId,
                    };
                    SetCreatedProperties(permission);
                    entities.ReportRolePermission.AddObject(permission);
                }

                #endregion

                return SaveChanges(entities);
            }
        }

        #endregion

        #region ReportSelection

        public ReportSelection GetReportSelection(int reportId, int actorCompanyId)
        {
            using var entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            return (from r in entitiesReadOnly.Report
                    where r.ReportId == reportId &&
                    r.ActorCompanyId == actorCompanyId
                    select r.ReportSelection).FirstOrDefault();
        }

        /// <summary>
        /// Save a ReportSelection with the given Selections
        /// If the Report is a original, a copy is created and the selections are connected to that Report
        /// </summary>
        /// <param name="es">The EvaluatedSelection</param>
        /// <returns>ActionResult. IntegerValue will contain the ReportId of the Report with the saved ReportSelection if the save succeeded</returns>
        public ActionResult SaveReportSelection(EvaluatedSelection es)
        {
            if (es == null)
                return null;

            ActionResult result = new ActionResult(false);

            using (CompEntities entities = new CompEntities())
            {
                bool success = false;

                try
                {
                    entities.Connection.Open();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        #region Report

                        //Report
                        Report report = GetReport(entities, es.ReportId, es.ActorCompanyId, loadReportSelection: true);
                        if (report == null)
                            return new ActionResult((int)ActionResultSave.EntityNotFound, "Report");

                        //Can not change original, create a copy
                        if (report.Original)
                        {
                            #region Report

                            Report newReport = new Report()
                            {
                                ReportNr = report.ReportNr,
                                Name = report.Name,
                                Description = report.Description,
                                Standard = report.Standard,
                                Original = false,
                                ReportTemplateId = report.ReportTemplateId,
                                Module = report.Module,
                                GroupByLevel1 = report.GroupByLevel1,
                                GroupByLevel2 = report.GroupByLevel2,
                                GroupByLevel3 = report.GroupByLevel3,
                                GroupByLevel4 = report.GroupByLevel4,
                                SortByLevel1 = report.SortByLevel1,
                                SortByLevel2 = report.SortByLevel2,
                                SortByLevel3 = report.SortByLevel3,
                                SortByLevel4 = report.SortByLevel4,
                                IsSortAscending = report.IsSortAscending,
                                Special = report.Special,
                                IncludeAllHistoricalData = report.IncludeAllHistoricalData,
                                IncludeBudget = report.IncludeBudget,
                                NoOfYearsBackinPreviousYear = report.NoOfYearsBackinPreviousYear,
                                GetDetailedInformation = report.GetDetailedInformation,

                                //Set FK
                                ActorCompanyId = es.ActorCompanyId,
                            };

                            if (AddEntityItem(entities, newReport, "Report", transaction).Success)
                            {
                                #region Copy ReportGroupMapping

                                List<ReportGroupMapping> reportGroupMappings = GetReportGroupMappings(entities, report.ReportId, es.ActorCompanyId);
                                foreach (ReportGroupMapping reportGroupMapping in reportGroupMappings)
                                {
                                    if (!reportGroupMapping.ReportReference.IsLoaded)
                                        reportGroupMapping.ReportReference.Load();
                                    if (!reportGroupMapping.ReportGroupReference.IsLoaded)
                                        reportGroupMapping.ReportGroupReference.Load();

                                    ReportGroupMapping newReportGroupMapping = new ReportGroupMapping()
                                    {
                                        Order = reportGroupMapping.Order,
                                    };

                                    newReportGroupMapping.Report = newReport;
                                    newReportGroupMapping.ReportGroup = GetReportGroup(entities, reportGroupMapping.ReportGroup.ReportGroupId, es.ActorCompanyId, false, false);
                                    if (newReportGroupMapping.Report != null && newReportGroupMapping.ReportGroup != null)
                                    {
                                        success = AddEntityItem(entities, newReportGroupMapping, "ReportGroupMapping", transaction).Success;
                                        if (!success)
                                            break;
                                    }
                                }

                                #endregion

                                #region Copy ReportGroupMapping

                                List<ReportRolePermission> permissions = GetReportRolePermissionsByReport(entities, report.ReportId);
                                foreach (ReportRolePermission permission in permissions)
                                {
                                    ReportRolePermission newReportRolePermission = new ReportRolePermission()
                                    {
                                        ReportId = newReport.ReportId,
                                        RoleId = permission.RoleId,
                                        ActorCompanyId = newReport.ActorCompanyId,
                                    };
                                    SetCreatedProperties(newReportRolePermission);
                                    entities.ReportRolePermission.AddObject(newReportRolePermission);
                                }

                                success = SaveChanges(entities).Success;

                                #endregion
                            }

                            #endregion

                            if (success)
                                report = newReport;
                        }

                        #endregion

                        #region ReportSelection

                        //ReportSelection
                        if (report.ReportSelection == null)
                        {
                            report.ReportSelection = new ReportSelection()
                            {
                                ReportSelectionText = es.ReportSelectionText,
                            };
                            success = AddEntityItem(entities, report.ReportSelection, "ReportSelection", transaction).Success;
                        }
                        else
                        {
                            if (report.ReportSelection.ReportSelectionText != es.ReportSelectionText)
                            {
                                report.ReportSelection.ReportSelectionText = es.ReportSelectionText;
                                success = SaveEntityItem(entities, report, transaction).Success;
                            }
                            else
                            {
                                success = true;
                            }

                            #region Delete ReportSelections

                            //ReportSelectionInt
                            List<ReportSelectionInt> reportSelectionInts = GetReportSelectionInts(entities, report.ReportSelection.ReportSelectionId);
                            foreach (ReportSelectionInt reportSelectionInt in reportSelectionInts)
                            {
                                entities.DeleteObject(reportSelectionInt);
                            }

                            //ReportSelectionStr
                            List<ReportSelectionStr> reportSelectionStrs = GetReportSelectionStrs(entities, report.ReportSelection.ReportSelectionId);
                            foreach (ReportSelectionStr reportSelectionStr in reportSelectionStrs)
                            {
                                entities.DeleteObject(reportSelectionStr);
                            }

                            List<ReportSelectionDate> reportSelectionDates = GetReportSelectionDates(entities, report.ReportSelection.ReportSelectionId);
                            foreach (ReportSelectionDate reportSelectionDate in reportSelectionDates)
                            {
                                entities.DeleteObject(reportSelectionDate);
                            }

                            #endregion
                        }

                        #region Save ReportSelections

                        #region Voucher

                        if (es.SV_IsEvaluated)
                        {
                            //VoucherSeries
                            if (es.SV_HasVoucherSeriesTypeNrInterval)
                            {
                                ReportSelectionInt reportSelectionVoucherSeries = new ReportSelectionInt()
                                {
                                    ReportSelectionType = (int)SoeSelectionData.Int_Voucher_VoucherSeriesId,
                                    SelectFrom = es.SV_VoucherSeriesTypeNrFrom,
                                    SelectTo = es.SV_VoucherSeriesTypeNrTo,

                                    //Set references
                                    ReportSelection = report.ReportSelection,
                                };
                                entities.ReportSelectionInt.AddObject(reportSelectionVoucherSeries);
                            }

                            //VoucherNr
                            if (success && es.SV_HasVoucherNrInterval)
                            {
                                ReportSelectionInt reportSelectionVoucherNr = new ReportSelectionInt()
                                {
                                    ReportSelectionType = (int)SoeSelectionData.Int_Voucher_VoucherNr,
                                    SelectFrom = es.SV_VoucherNrFrom,
                                    SelectTo = es.SV_VoucherNrTo,

                                    //Set references
                                    ReportSelection = report.ReportSelection,
                                };
                                entities.ReportSelectionInt.AddObject(reportSelectionVoucherNr);
                            }
                        }

                        #endregion

                        #region Budget
                        if (es.SV_IsEvaluated && es.SSTD_BudgetId.HasValue)
                        {
                            ReportSelectionInt reportSelectionBudget = new ReportSelectionInt()
                            {
                                ReportSelectionType = (int)SoeSelectionData.Int_BudgetId,
                                SelectFrom = (int)es.SSTD_BudgetId,
                                SelectTo = (int)es.SSTD_BudgetId,

                                //Set references
                                ReportSelection = report.ReportSelection,
                            };
                            entities.ReportSelectionInt.AddObject(reportSelectionBudget);
                        }

                        #endregion

                        #region Account

                        if (es.SA_IsEvaluated && success && es.SA_HasAccountInterval)
                        {
                            int i = 1;
                            foreach (AccountIntervalDTO accountInterval in es.SA_AccountIntervals)
                            {
                                if (!String.IsNullOrEmpty(accountInterval.AccountNrFrom) && !String.IsNullOrEmpty(accountInterval.AccountNrTo))
                                {
                                    ReportSelectionStr reportSelectionAccount = new ReportSelectionStr()
                                    {
                                        ReportSelectionType = (int)SoeSelectionData.Str_Account,
                                        SelectFrom = accountInterval.AccountNrFrom,
                                        SelectTo = accountInterval.AccountNrTo,
                                        SelectGroup = accountInterval.AccountDimId,
                                        Order = i,

                                        //Set references
                                        ReportSelection = report.ReportSelection,
                                    };
                                    entities.ReportSelectionStr.AddObject(reportSelectionAccount);
                                    i++;
                                }
                            }
                        }

                        #endregion

                        #region Ledger

                        if (es.SL_IsEvaluated)
                        {
                            //ActorNr
                            if (es.SL_HasActorNrInterval)
                            {
                                ReportSelectionStr reportSelectionActorNr = new ReportSelectionStr()
                                {
                                    ReportSelectionType = (int)SoeSelectionData.Str_Ledger_ActorNr,
                                    SelectFrom = es.SL_ActorNrFrom,
                                    SelectTo = es.SL_ActorNrTo,

                                    //Set references
                                    ReportSelection = report.ReportSelection,
                                };
                                entities.ReportSelectionStr.AddObject(reportSelectionActorNr);
                            }

                            //InvoiceSeqNr
                            if (success && es.SL_HasInvoiceSeqNrInterval)
                            {
                                ReportSelectionInt reportSelectionInvoiceSeqNr = new ReportSelectionInt()
                                {
                                    ReportSelectionType = (int)SoeSelectionData.Int_Ledger_InvoiceSeqNr,
                                    SelectFrom = es.SL_InvoiceSeqNrFrom,
                                    SelectTo = es.SL_InvoiceSeqNrTo,

                                    //Set references
                                    ReportSelection = report.ReportSelection,
                                };
                                entities.ReportSelectionInt.AddObject(reportSelectionInvoiceSeqNr);
                            }

                            //DateRegard
                            if (success)
                            {
                                ReportSelectionInt reportSelectionDateRegard = new ReportSelectionInt()
                                {
                                    ReportSelectionType = (int)SoeSelectionData.Int_Ledger_DateRegard,
                                    SelectFrom = es.SL_DateRegard,
                                    SelectTo = es.SL_DateRegard,

                                    //Set references
                                    ReportSelection = report.ReportSelection,
                                };
                                entities.ReportSelectionInt.AddObject(reportSelectionDateRegard);
                            }

                            //SortOrder
                            if (success)
                            {
                                ReportSelectionInt reportSelectionSortOrder = new ReportSelectionInt()
                                {
                                    ReportSelectionType = (int)SoeSelectionData.Int_Ledger_SortOrder,
                                    SelectFrom = es.SL_SortOrder,
                                    SelectTo = es.SL_SortOrder,

                                    //Set references
                                    ReportSelection = report.ReportSelection,
                                };
                                entities.ReportSelectionInt.AddObject(reportSelectionSortOrder);
                            }

                            //InvoiceSelection
                            if (success)
                            {
                                ReportSelectionInt reportSelectionInvoiceSelection = new ReportSelectionInt()
                                {
                                    ReportSelectionType = (int)SoeSelectionData.Int_Ledger_InvoiceSelection,
                                    SelectFrom = es.SL_InvoiceSelection,
                                    SelectTo = es.SL_InvoiceSelection,

                                    //Set references
                                    ReportSelection = report.ReportSelection,
                                };
                                entities.ReportSelectionInt.AddObject(reportSelectionInvoiceSelection);
                            }
                        }

                        #endregion

                        #region Billing

                        if (es.SB_IsEvaluated)
                        {
                            //CustomerNr
                            if (success && es.SB_HasCustomerNrInterval)
                            {
                                ReportSelectionStr reportSelectionCustomerNr = new ReportSelectionStr()
                                {
                                    ReportSelectionType = (int)SoeSelectionData.Str_Billing_CustomerNr,
                                    SelectFrom = es.SB_CustomerNrFrom,
                                    SelectTo = es.SB_CustomerNrTo,

                                    //Set references
                                    ReportSelection = report.ReportSelection,
                                };
                                entities.ReportSelectionStr.AddObject(reportSelectionCustomerNr);
                            }

                            //InvoiceNr
                            if (success && es.SB_HasInvoiceNrInterval)
                            {
                                ReportSelectionStr reportSelectionInvoiceNr = new ReportSelectionStr()
                                {
                                    ReportSelectionType = (int)SoeSelectionData.Str_Billing_InvoiceNr,
                                    SelectFrom = es.SB_InvoiceNrFrom,
                                    SelectTo = es.SB_InvoiceNrTo,

                                    //Set references
                                    ReportSelection = report.ReportSelection,
                                };
                                entities.ReportSelectionStr.AddObject(reportSelectionInvoiceNr);
                            }

                            //ProjectNr
                            if (success && es.SB_HasProjectNrInterval)
                            {
                                ReportSelectionStr reportSelectionProjectNr = new ReportSelectionStr()
                                {
                                    ReportSelectionType = (int)SoeSelectionData.Str_Billing_ProjectNr,
                                    SelectFrom = es.SB_ProjectNrFrom,
                                    SelectTo = es.SB_ProjectNrTo,

                                    //Set references
                                    ReportSelection = report.ReportSelection,
                                };
                                entities.ReportSelectionStr.AddObject(reportSelectionProjectNr);
                            }

                            //EmployeeNr
                            if (success && es.SB_HasEmployeeNrInterval)
                            {
                                ReportSelectionStr reportSelectionEmployeeNr = new ReportSelectionStr()
                                {
                                    ReportSelectionType = (int)SoeSelectionData.Str_Billing_EmployeeNr,
                                    SelectFrom = !String.IsNullOrEmpty(es.SB_EmployeeNrFrom) ? es.SB_EmployeeNrFrom : String.Empty,
                                    SelectTo = !String.IsNullOrEmpty(es.SB_EmployeeNrTo) ? es.SB_EmployeeNrTo : String.Empty,

                                    //Set references
                                    ReportSelection = report.ReportSelection,
                                };
                                entities.ReportSelectionStr.AddObject(reportSelectionEmployeeNr);
                            }

                            //SortOrder
                            if (success)
                            {
                                ReportSelectionInt reportSelectionSortOrder = new ReportSelectionInt()
                                {
                                    ReportSelectionType = (int)SoeSelectionData.Int_Billing_SortOrder,
                                    SelectFrom = es.SB_SortOrder,
                                    SelectTo = es.SB_SortOrder,

                                    //Set references
                                    ReportSelection = report.ReportSelection,
                                };
                                entities.ReportSelectionInt.AddObject(reportSelectionSortOrder);
                            }
                        }

                        #endregion

                        #region Time

                        if (es.ST_IsEvaluated && success)
                        {
                            if (es.ST_EmployeeIds != null)
                            {
                                foreach (int employeeId in es.ST_EmployeeIds)
                                {
                                    ReportSelectionInt reportSelectionEmployeeId = new ReportSelectionInt()
                                    {
                                        ReportSelectionType = (int)SoeSelectionData.Int_Time_EmployeeId,
                                        SelectFrom = employeeId,
                                        SelectTo = employeeId,

                                        //Set references
                                        ReportSelection = report.ReportSelection,
                                    };
                                    entities.ReportSelectionInt.AddObject(reportSelectionEmployeeId);
                                }
                            }

                            if (es.ST_CategoryIds != null)
                            {
                                foreach (int categoryId in es.ST_CategoryIds)
                                {
                                    ReportSelectionInt reportSelectionCategoryId = new ReportSelectionInt()
                                    {
                                        ReportSelectionType = (int)SoeSelectionData.Int_Time_CategoryId,
                                        SelectFrom = categoryId,
                                        SelectTo = categoryId,

                                        //Set references
                                        ReportSelection = report.ReportSelection,
                                    };
                                    entities.ReportSelectionInt.AddObject(reportSelectionCategoryId);
                                }
                            }

                            if (es.ST_ShiftTypeIds != null)
                            {
                                foreach (int shiftTypeId in es.ST_ShiftTypeIds)
                                {
                                    ReportSelectionInt reportSelectionShiftTypeId = new ReportSelectionInt()
                                    {
                                        ReportSelectionType = (int)SoeSelectionData.Int_Time_ShiftTypeIds,
                                        SelectFrom = shiftTypeId,
                                        SelectTo = shiftTypeId,

                                        //Set references
                                        ReportSelection = report.ReportSelection,
                                    };
                                    entities.ReportSelectionInt.AddObject(reportSelectionShiftTypeId);
                                }
                            }

                            if (es.ST_PayrollProductIds != null)
                            {
                                foreach (int payrollProductId in es.ST_PayrollProductIds)
                                {
                                    ReportSelectionInt reportSelectionPayrollProductId = new ReportSelectionInt()
                                    {
                                        ReportSelectionType = (int)SoeSelectionData.Int_Time_PayrollProductId,
                                        SelectFrom = payrollProductId,
                                        SelectTo = payrollProductId,

                                        //Set references
                                        ReportSelection = report.ReportSelection,
                                    };
                                    entities.ReportSelectionInt.AddObject(reportSelectionPayrollProductId);
                                }
                            }

                            // Export & Export File types
                            if (es.ExportType > TermGroup_ReportExportType.Unknown)
                            {
                                report.ExportType = (int)es.ExportType;
                                report.ReportSelectionText = es.ReportSelectionText;
                            }

                            if (es.ExportFileType > TermGroup_ReportExportFileType.Unknown)
                            {
                                report.FileType = (int)es.ExportFileType;
                                report.ReportSelectionText = es.ReportSelectionText;
                            }
                        }

                        #endregion

                        #endregion

                        #endregion

                        if (success)
                            success = SaveChanges(entities, transaction).Success;

                        if (success)
                        {
                            //Commit transaction
                            transaction.Complete();

                            result.IntegerValue = report.ReportId;
                            result.Success = true;
                        }
                    }
                }
                finally
                {
                    if (result.Success)
                    {
                        //Set success properties
                    }
                    else
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }

                return result;
            }

        }

        #region SelectionVoucher

        public SelectionVoucher GetReportSelectionVoucher(int reportId, int actorCompanyId)
        {
            ReportSelection reportSelection = GetReportSelection(reportId, actorCompanyId);
            if (reportSelection == null)
                return null;

            SelectionVoucher selectionVoucher = new SelectionVoucher();

            //VoucherSeries
            ReportSelectionInt selectionIntVoucherSeriesType = GetReportSelectionIntByType(reportSelection.ReportSelectionId, (int)SoeSelectionData.Int_Voucher_VoucherSeriesId);
            if (selectionIntVoucherSeriesType != null)
            {
                selectionVoucher.VoucherSeriesTypeNrFrom = selectionIntVoucherSeriesType.SelectFrom;
                selectionVoucher.VoucherSeriesTypeNrTo = selectionIntVoucherSeriesType.SelectTo;
            }

            //VoucherNr
            ReportSelectionInt selectionIntVoucherNr = GetReportSelectionIntByType(reportSelection.ReportSelectionId, (int)SoeSelectionData.Int_Voucher_VoucherNr);
            if (selectionIntVoucherNr != null)
            {
                selectionVoucher.VoucherNrFrom = selectionIntVoucherNr.SelectFrom;
                selectionVoucher.VoucherNrTo = selectionIntVoucherNr.SelectTo;
            }

            return selectionVoucher;
        }

        #endregion

        #region SelectionAccount

        public SelectionAccount GetReportSelectionAccount(int reportId, int actorCompanyId)
        {
            ReportSelection reportSelection = GetReportSelection(reportId, actorCompanyId);
            if (reportSelection == null)
                return null;

            SelectionAccount selectionAccount = new SelectionAccount();

            //VoucherSeries
            List<ReportSelectionStr> selectionStrAccounts = GetReportSelectionStrsByType(reportSelection.ReportSelectionId, (int)SoeSelectionData.Str_Account);
            foreach (ReportSelectionStr selectionStrAccount in selectionStrAccounts)
            {
                selectionAccount.AddAccountInterval(
                    new AccountIntervalDTO()
                    {
                        AccountDimId = selectionStrAccount.SelectGroup,
                        AccountNrFrom = selectionStrAccount.SelectFrom,
                        AccountNrTo = selectionStrAccount.SelectTo,
                    });
            }

            return selectionAccount;
        }

        #endregion

        #region SelectionLedger

        public SelectionLedger GetReportSelectionLedger(int reportId, int actorCompanyId)
        {
            ReportSelection reportSelection = GetReportSelection(reportId, actorCompanyId);
            if (reportSelection == null)
                return null;

            SelectionLedger selectionLedger = new SelectionLedger();

            //InvoiceSelection

            //SupplierNr
            ReportSelectionStr selectionStrActorNr = GetReportSelectionStrByType(reportSelection.ReportSelectionId, (int)SoeSelectionData.Str_Ledger_ActorNr);
            if (selectionStrActorNr != null)
            {
                selectionLedger.ActorNrFrom = selectionStrActorNr.SelectFrom;
                selectionLedger.ActorNrTo = selectionStrActorNr.SelectTo;
            }

            //InvoiceSeqNr
            ReportSelectionInt selectionIntInvoiceSeqNr = GetReportSelectionIntByType(reportSelection.ReportSelectionId, (int)SoeSelectionData.Int_Ledger_InvoiceSeqNr);
            if (selectionIntInvoiceSeqNr != null)
            {
                selectionLedger.InvoiceSeqNrFrom = selectionIntInvoiceSeqNr.SelectFrom;
                selectionLedger.InvoiceSeqNrTo = selectionIntInvoiceSeqNr.SelectTo;
            }

            //DateRegard
            ReportSelectionInt selectionIntDateRegard = GetReportSelectionIntByType(reportSelection.ReportSelectionId, (int)SoeSelectionData.Int_Ledger_DateRegard);
            if (selectionIntDateRegard != null)
            {
                selectionLedger.DateRegard = selectionIntDateRegard.SelectFrom;
            }

            //SortOrder
            ReportSelectionInt selectionIntSortOrder = GetReportSelectionIntByType(reportSelection.ReportSelectionId, (int)SoeSelectionData.Int_Ledger_SortOrder);
            if (selectionIntSortOrder != null)
            {
                selectionLedger.SortOrder = selectionIntSortOrder.SelectFrom;
            }

            //InvoiceSelection
            ReportSelectionInt selectionIntInvoiceSelection = GetReportSelectionIntByType(reportSelection.ReportSelectionId, (int)SoeSelectionData.Int_Ledger_InvoiceSelection);
            if (selectionIntInvoiceSelection != null)
            {
                selectionLedger.InvoiceSelection = selectionIntInvoiceSelection.SelectFrom;
            }

            return selectionLedger;
        }

        #endregion

        #endregion

        #region ReportSelectionStr

        public List<ReportSelectionStr> GetReportSelectionStrs(int reportSelectionId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.ReportSelectionStr.NoTracking();
            return GetReportSelectionStrs(entities, reportSelectionId);
        }

        public List<ReportSelectionStr> GetReportSelectionStrs(CompEntities entities, int reportSelectionId)
        {
            return (from rss in entities.ReportSelectionStr
                    where rss.ReportSelection.ReportSelectionId == reportSelectionId
                    orderby rss.Order ascending
                    select rss).ToList();
        }

        public List<ReportSelectionStr> GetReportSelectionStrsByType(int reportSelectionId, int reportSelectionType)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.ReportSelectionStr.NoTracking();
            return GetReportSelectionStrsByType(entities, reportSelectionId, reportSelectionType);
        }

        public List<ReportSelectionStr> GetReportSelectionStrsByType(CompEntities entities, int reportSelectionId, int reportSelectionType)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            return (from rss in entitiesReadOnly.ReportSelectionStr
                    where rss.ReportSelection.ReportSelectionId == reportSelectionId &&
                    rss.ReportSelectionType == reportSelectionType
                    orderby rss.Order ascending
                    select rss).ToList();
        }

        public ReportSelectionStr GetReportSelectionStrByType(int reportSelectionId, int reportSelectionType)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.ReportSelectionStr.NoTracking();
            return GetReportSelectionStrByType(entities, reportSelectionId, reportSelectionType);
        }

        public ReportSelectionStr GetReportSelectionStrByType(CompEntities entities, int reportSelectionId, int reportSelectionType)
        {
            return (from rss in entities.ReportSelectionStr
                    where rss.ReportSelection.ReportSelectionId == reportSelectionId &&
                    rss.ReportSelectionType == reportSelectionType
                    orderby rss.Order ascending
                    select rss).FirstOrDefault();
        }

        #endregion

        #region ReportSelectionInt

        public List<ReportSelectionInt> GetReportSelectionInts(int reportSelectionId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.ReportSelectionInt.NoTracking();
            return GetReportSelectionInts(entities, reportSelectionId);
        }

        public List<ReportSelectionInt> GetReportSelectionInts(CompEntities entities, int reportSelectionId)
        {
            return (from rsi in entities.ReportSelectionInt
                    where rsi.ReportSelection.ReportSelectionId == reportSelectionId
                    orderby rsi.Order ascending
                    select rsi).ToList();
        }

        public ReportSelectionInt GetReportSelectionIntByType(int reportSelectionId, int reportSelectionType)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.ReportSelectionInt.NoTracking();
            return GetReportSelectionIntByType(entities, reportSelectionId, reportSelectionType);
        }

        public ReportSelectionInt GetReportSelectionIntByType(CompEntities entities, int reportSelectionId, int reportSelectionType)
        {
            return (from rsi in entities.ReportSelectionInt
                    where rsi.ReportSelection.ReportSelectionId == reportSelectionId &&
                    rsi.ReportSelectionType == reportSelectionType
                    orderby rsi.Order ascending
                    select rsi).FirstOrDefault();
        }

        #endregion

        #region ReportSelectionDate

        public List<ReportSelectionDate> GetReportSelectionDates(CompEntities entities, int reportSelectionId)
        {
            return (from rsd in entities.ReportSelectionDate
                    where rsd.ReportSelection.ReportSelectionId == reportSelectionId
                    orderby rsd.Order ascending
                    select rsd).ToList();
        }

        #endregion

        #region ReportGroup

        public List<ReportGroup> GetReportGroupsByCompany(int actorCompanyId, bool loadReportGroupMapping = false, bool loadReportGroupHeaderMapping = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.ReportGroup.NoTracking();
            return GetReportGroupsByCompany(entities, actorCompanyId, loadReportGroupMapping, loadReportGroupHeaderMapping);
        }

        public List<ReportGroup> GetReportGroupsByCompany(CompEntities entities, int actorCompanyId, bool loadReportGroupMapping = false, bool loadReportGroupHeaderMapping = false)
        {
            using var entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            List<ReportGroup> reportGroups = (from rg in entitiesReadOnly.ReportGroup
                                              where rg.Company.ActorCompanyId == actorCompanyId &&
                                              rg.State == (int)SoeEntityState.Active
                                              orderby rg.TemplateTypeId, rg.Name ascending
                                              select rg).ToList();

            foreach (ReportGroup reportGroup in reportGroups)
            {
                if (loadReportGroupMapping && !reportGroup.ReportGroupMapping.IsLoaded)
                    reportGroup.ReportGroupMapping.Load();
                if (loadReportGroupHeaderMapping && !reportGroup.ReportGroupHeaderMapping.IsLoaded)
                    reportGroup.ReportGroupHeaderMapping.Load();
            }

            return reportGroups;
        }

        public List<ReportGroup> GetReportGroupsByModule(int module, int actorCompanyId, int templateTypeId, bool loadReportGroupMapping = false, bool loadReportGroupHeaderMapping = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.ReportGroup.NoTracking();
            return GetReportGroupsByModule(entities, module, actorCompanyId, templateTypeId, loadReportGroupMapping, loadReportGroupHeaderMapping);
        }

        public List<ReportGroup> GetReportGroupsByModule(CompEntities entities, int module, int actorCompanyId, int templateTypeId, bool loadReportGroupMapping = false, bool loadReportGroupHeaderMapping = false)
        {
            IQueryable<ReportGroup> reportGroups = null;

            if (templateTypeId > 0)
            {
                reportGroups = (from rg in entities.ReportGroup
                                where rg.Company.ActorCompanyId == actorCompanyId &&
                                rg.Module == module &&
                                (rg.TemplateTypeId == templateTypeId || rg.TemplateTypeId == (int)SoeReportTemplateType.Generic) &&
                                rg.State != (int)SoeEntityState.Deleted
                                orderby rg.Name ascending
                                select rg);
            }
            else
            {
                reportGroups = (from rg in entities.ReportGroup
                                where rg.Company.ActorCompanyId == actorCompanyId &&
                                rg.Module == module &&
                                rg.State != (int)SoeEntityState.Deleted
                                orderby rg.TemplateTypeId, rg.Name ascending
                                select rg);
            }

            var reportGroupsList = reportGroups.ToList();

            foreach (ReportGroup reportGroup in reportGroupsList)
            {
                if (loadReportGroupMapping && !reportGroup.ReportGroupMapping.IsLoaded)
                    reportGroup.ReportGroupMapping.Load();
                if (loadReportGroupHeaderMapping && !reportGroup.ReportGroupHeaderMapping.IsLoaded)
                    reportGroup.ReportGroupHeaderMapping.Load();
            }

            return reportGroupsList;
        }
        public IReportGroupService GetReportGroupService(int actorCompanyId)
        {
            bool isSupportLicense = LicenseManager.GetLicense(base.LicenseId)?.Support == true;
            bool isSupportLoggedIn = this.parameterObject.SoeUser.LicenseId == base.LicenseId;
            return ReportGroupServiceFactory.Create(isSupportLoggedIn, isSupportLicense, this, actorCompanyId);
        }
        public List<ReportGroupDTO> GetReportGroups(int module, int actorCompanyId, bool loadReportGroupMapping = false, bool loadReportGroupHeaderMapping = false)
        {
            var service = GetReportGroupService(actorCompanyId);
            return service.GetReportGroups(module, loadReportGroupMapping, loadReportGroupHeaderMapping);
        }
        public List<ReportHeaderDTO> GetReportHeaders(int module, int actorCompanyId, bool loadReportHeaderInterval = false)
        {
            var service = GetReportGroupService(actorCompanyId);
            return service.GetReportHeaders(module, -1, loadReportHeaderInterval);
        }
        public ActionResult DeleteReportGroups(int actorCompanyId, List<int> reportGroupIds)
        {
            var service = GetReportGroupService(actorCompanyId);
            return service.DeleteReportGroups(reportGroupIds);
        }
        public ActionResult DeleteReportHeaders(int actorCompanyId, List<int> reportHeaderIds)
        {
            var service = GetReportGroupService(actorCompanyId);
            return service.DeleteReportHeaders(reportHeaderIds);
        }

        public List<ReportGroup> GetReportGroupsByModule(int module, int actorCompanyId, bool loadReportGroupMapping = false, bool loadReportGroupHeaderMapping = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.ReportGroup.NoTracking();
            return GetReportGroupsByModule(entities, module, actorCompanyId, loadReportGroupMapping, loadReportGroupHeaderMapping);
        }

        public List<ReportGroup> GetReportGroupsByModule(CompEntities entities, int module, int actorCompanyId, bool loadReportGroupMapping = false, bool loadReportGroupHeaderMapping = false)
        {
            var sysReportTemplateTypeTerms = base.GetTermGroupDict(TermGroup.SysReportTemplateType);

            // Need to complete resultset with template type
            List<ReportGroup> reportGroups = GetReportGroupsByModule(entities, module, actorCompanyId, -1, loadReportGroupMapping, loadReportGroupHeaderMapping);
            foreach (ReportGroup reportGroup in reportGroups)
            {
                if (sysReportTemplateTypeTerms.ContainsKey(reportGroup.TemplateTypeId))
                    reportGroup.TemplateType = sysReportTemplateTypeTerms[reportGroup.TemplateTypeId];
            }

            return reportGroups;
        }

        public List<ReportGroupDTO> GetReportGroups(int actorCompanyId, int reportId)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            var entities = entitiesReadOnly;
            var groupMappings = entities.ReportGroupMapping
                .Where(rm => rm.Report.ActorCompanyId == actorCompanyId &&
                    rm.ReportGroup.Company.ActorCompanyId == actorCompanyId &&
                    rm.ReportId == reportId)
                .Include(rm => rm.ReportGroup)
                .OrderBy(rm => rm.Order)
                .ToList();

            var groups = groupMappings
                .Select(rm => rm.ReportGroup.ToDTO())
                .ToList();

            var groupIds = groups.Select(g => g.ReportGroupId);
            var headerMappings = entities.ReportGroupHeaderMapping
                .Where(hm => groupIds.Contains(hm.ReportGroupId) && hm.ReportGroup.Company.ActorCompanyId == actorCompanyId)
                .Include("ReportHeader.ReportHeaderInterval")
                .OrderBy(hm => hm.Order)
                .ToList();

            foreach (var group in groups)
            {
                group.ReportHeaders = headerMappings
                    .Where(hm => hm.ReportGroupId == group.ReportGroupId)
                    .Select(hm => hm.ReportHeader)
                    .ToList()
                    .ToDTOs();
            }
            return groups;
        }
        public List<ReportGroup> GetReportGroupsByReport(CompEntities entities, int reportId, int actorCompanyId, bool loadReportGroupMapping = false, bool loadReportGroupHeaderMapping = false)
        {
            List<ReportGroup> reportGroups = (from m in entities.ReportGroupMapping
                                              where m.Report.ReportId == reportId &&
                                              m.ReportGroup.Company.ActorCompanyId == actorCompanyId
                                              orderby m.Order ascending
                                              select m.ReportGroup).ToList();

            foreach (ReportGroup reportGroup in reportGroups)
            {
                if (loadReportGroupMapping && !reportGroup.ReportGroupMapping.IsLoaded)
                    reportGroup.ReportGroupMapping.Load();
                if (loadReportGroupHeaderMapping && !reportGroup.ReportGroupHeaderMapping.IsLoaded)
                    reportGroup.ReportGroupHeaderMapping.Load();
            }

            return reportGroups;
        }

        public List<ReportGroupDTO> GetReportGroupsForReport(int actorCompanyId, Report report)
        {
            if (report.Standard && SysReportTemplateHasGroups(report.ReportTemplateId))
            {
                //For now, if we have SysReportGroups, we'll use them.
                //In the future, users might want to run a "GO Report" with their own groups.
                return GetSysReportGroups(report.ReportTemplateId);
            }
            return GetReportGroups(actorCompanyId, report.ReportId);
        }


        public ReportGroup GetReportGroup(int reportGroupId, int actorCompanyId, bool loadReportGroupMapping, bool loadReportGroupHeaderMapping)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.ReportGroup.NoTracking();
            return GetReportGroup(entities, reportGroupId, actorCompanyId, loadReportGroupMapping, loadReportGroupHeaderMapping);
        }

        public ReportGroup GetReportGroup(CompEntities entities, int reportGroupId, int actorCompanyId, bool loadReportGroupMapping, bool loadReportGroupHeaderMapping)
        {
            ReportGroup reportGroup = (from r in entities.ReportGroup
                                       where r.ReportGroupId == reportGroupId &&
                                       r.Company.ActorCompanyId == actorCompanyId
                                       select r).FirstOrDefault<ReportGroup>();

            if (reportGroup != null)
            {
                if (loadReportGroupMapping && !reportGroup.ReportGroupMapping.IsLoaded)
                    reportGroup.ReportGroupMapping.Load();
                if (loadReportGroupHeaderMapping && !reportGroup.ReportGroupHeaderMapping.IsLoaded)
                    reportGroup.ReportGroupHeaderMapping.Load();
            }

            return reportGroup;
        }

        public ReportGroup GetPrevNextReportGroupById(int reportGroupId, int module, int actorCompanyId, SoeFormMode mode)
        {
            ReportGroup reportGroup = null;
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            entitiesReadOnly.ReportGroup.NoTracking();
            if (mode == SoeFormMode.Next)
            {
                reportGroup = (from r in entitiesReadOnly.ReportGroup
                               where ((r.ReportGroupId > reportGroupId) &&
                               (r.Module == module) &&
                               (r.Company.ActorCompanyId == actorCompanyId) &&
                               (r.State == (int)SoeEntityState.Active))
                               orderby r.ReportGroupId ascending
                               select r).FirstOrDefault<ReportGroup>();
            }
            else
            {
                reportGroup = (from r in entitiesReadOnly.ReportGroup
                               where ((r.ReportGroupId < reportGroupId) &&
                               (r.Module == module) &&
                               (r.Company.ActorCompanyId == actorCompanyId) &&
                               (r.State == (int)SoeEntityState.Active))
                               orderby r.ReportGroupId descending
                               select r).FirstOrDefault<ReportGroup>();
            }

            return reportGroup;
        }

        public bool IsReportGroupInUse(ReportGroup reportGroup)
        {
            if (reportGroup == null)
                return false;
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            int counter = (from r in entitiesReadOnly.ReportGroupMapping
                           where r.ReportGroupId == reportGroup.ReportGroupId
                           select r).Count();

            if (counter > 0)
                return true;

            counter = (from r in entitiesReadOnly.ReportGroupHeaderMapping
                       where r.ReportGroupId == reportGroup.ReportGroupId
                       select r).Count();

            if (counter > 0)
                return true;

            return false;
        }

        public bool ImportReportHeadersAndReportGroupsFromReportCreateNew(CompEntities entities, int importReportId, int destinationReportId, int importCompanyId, int destinationCompanyId)
        {
            bool result = true;

            Report importReport = GetReport(entities, importReportId, importCompanyId, true);
            if (importReport == null)
                return false;

            Company destinationCompany = CompanyManager.GetCompany(entities, destinationCompanyId);
            if (destinationCompany == null)
                return false;

            Report destinationReport = GetReport(entities, destinationReportId, destinationCompanyId, true);
            if (destinationReport == null)
                return false;

            //Get ReportGroups for destination Company once
            List<ReportGroup> destinationReportGroups = GetReportGroupsByModule(entities, destinationReport.Module, destinationCompanyId, -1, true, true);
            List<ReportHeader> destinationReportHeaders = GetReportHeadersByModule(entities, destinationReport.Module, destinationCompanyId, -1, false);
            int reportGroupMappingOrder = 1;
            foreach (ReportGroupMapping importMapping in importReport.ReportGroupMapping.OrderBy(i => i.Order))
            {
                bool reportGroupOk = false;

                if (!importMapping.ReportGroupReference.IsLoaded)
                    importMapping.ReportGroupReference.Load();

                if (importMapping.ReportGroup == null)
                    continue;

                //Check if ReportGroup already exist in destination Company
                bool reportGroupExists = destinationReportGroups.Any(rg => rg.Name == importMapping.ReportGroup.Name && rg.Description == importMapping.ReportGroup.Description);

                //Add ReportGroup to destination Company
                ReportGroup destinationReportGroup = new ReportGroup()
                {
                    Name = importMapping.ReportGroup.Name,
                    Description = importMapping.ReportGroup.Description,
                    ShowSum = importMapping.ReportGroup.ShowSum,
                    ShowLabel = importMapping.ReportGroup.ShowLabel,
                    TemplateTypeId = importMapping.ReportGroup.TemplateTypeId,
                    Module = importMapping.ReportGroup.Module,
                    InvertRow = importMapping.ReportGroup.InvertRow,

                    //Set references
                    Company = destinationCompany,
                };

                if (reportGroupExists)
                    destinationReportGroup.Description += GetText(8561, "Kopierad från mall") + " " + DateTime.Now.ToShortDateString();

                if (AddEntityItem(entities, destinationReportGroup, "ReportGroup").Success)
                {
                    //Make sure ReportGroupHeaderMapping is loaded
                    if (!importMapping.ReportGroup.ReportGroupHeaderMapping.IsLoaded)
                        importMapping.ReportGroup.ReportGroupHeaderMapping.Load();

                    int reportGroupHeaderMappingOrder = 1;
                    foreach (ReportGroupHeaderMapping importReportGroupHeaderMapping in importMapping.ReportGroup.ReportGroupHeaderMapping.OrderBy(i => i.Order))
                    {
                        //Make sure ReportHeader is loaded
                        if (!importReportGroupHeaderMapping.ReportHeaderReference.IsLoaded)
                            importReportGroupHeaderMapping.ReportHeaderReference.Load();

                        if (importReportGroupHeaderMapping.ReportHeader == null)
                            continue;

                        bool reportHeaderExits = destinationReportHeaders.Any(x => x.Name == importReportGroupHeaderMapping.ReportHeader.Name && x.Description == importReportGroupHeaderMapping.ReportHeader.Description);

                        //Add ReportHeader for destination Company
                        ReportHeader destReportHeader = new ReportHeader()
                        {
                            Name = importReportGroupHeaderMapping.ReportHeader.Name,
                            Description = importReportGroupHeaderMapping.ReportHeader.Description,
                            ShowRow = importReportGroupHeaderMapping.ReportHeader.ShowRow,
                            ShowZeroRow = importReportGroupHeaderMapping.ReportHeader.ShowZeroRow,
                            ShowSum = importReportGroupHeaderMapping.ReportHeader.ShowSum,
                            ShowLabel = importReportGroupHeaderMapping.ReportHeader.ShowLabel,
                            TemplateTypeId = importReportGroupHeaderMapping.ReportHeader.TemplateTypeId,
                            Module = importReportGroupHeaderMapping.ReportHeader.Module,
                            DoNotSummarizeOnGroup = importReportGroupHeaderMapping.ReportHeader.DoNotSummarizeOnGroup,
                            InvertRow = importReportGroupHeaderMapping.ReportHeader.InvertRow,

                            //Set references
                            Company = destinationCompany,
                        };

                        if (reportHeaderExits)
                            destReportHeader.Description += GetText(8561, "Kopierad från mall") + " " + DateTime.Now.ToShortDateString();


                        if (AddEntityItem(entities, destReportHeader, "ReportHeader").Success)
                        {
                            //Make sure ReportHeaderInterval is loaded
                            if (!importReportGroupHeaderMapping.ReportHeader.ReportHeaderInterval.IsLoaded)
                                importReportGroupHeaderMapping.ReportHeader.ReportHeaderInterval.Load();

                            foreach (ReportHeaderInterval templateReportHeaderInterval in importReportGroupHeaderMapping.ReportHeader.ReportHeaderInterval)
                            {
                                ReportHeaderInterval reportHeaderInterval = new ReportHeaderInterval
                                {
                                    IntervalFrom = templateReportHeaderInterval.IntervalFrom,
                                    IntervalTo = templateReportHeaderInterval.IntervalTo,
                                    SelectValue = templateReportHeaderInterval.SelectValue,

                                    //References
                                    ReportHeader = destReportHeader,
                                };

                                if (!AddEntityItem(entities, reportHeaderInterval, "ReportHeaderInterval").Success)
                                {
                                    string message = "Error while importing ReportHeaderInterval for ReportHeader [" + destReportHeader.Name + "]. " +
                                                        "ReportHeaderId [" + destReportHeader.ReportHeaderId + "]. " +
                                                        "Could not add";
                                    base.LogError(new SoeImportReportException(message, importReport.ReportId, destinationReport.ReportId, importCompanyId, destinationCompanyId, this.ToString()), this.log);

                                    result = false;
                                }
                            }

                            //Add ReportGroupHeaderMapping to destination ReportGroup
                            ReportGroupHeaderMapping destReportGroupHeaderMapping = new ReportGroupHeaderMapping()
                            {
                                Order = reportGroupHeaderMappingOrder,

                                //References
                                ReportHeader = destReportHeader,
                                ReportGroup = destinationReportGroup,
                            };

                            if (AddEntityItem(entities, destReportGroupHeaderMapping, "ReportGroupHeaderMapping").Success)
                            {
                                reportGroupHeaderMappingOrder++;
                            }
                            else
                            {
                                string message = "Error while importing ReportGroupHeaderMapping for ReportGroup [" + destinationReportGroup.Name + "]. " +
                                                    "ReportGroupId [" + destinationReportGroup.ReportGroupId + "]. " +
                                                    "Could not add";
                                base.LogError(new SoeImportReportException(message, importReport.ReportId, destinationReport.ReportId, importCompanyId, destinationCompanyId, this.ToString()), this.log);

                                result = false;
                            }
                        }
                        else
                        {
                            string message = "Error while importing ReportHeader [" + destReportHeader.Name + "]. " +
                                                "ReportHeaderId [" + destReportHeader.ReportHeaderId + "]. " +
                                                "Could not add";
                            base.LogError(new SoeImportReportException(message, importReport.ReportId, destinationReport.ReportId, importCompanyId, destinationCompanyId, this.ToString()), this.log);

                            result = false;
                        }
                    }

                    reportGroupOk = true;
                }
                else
                {
                    string message = "Error while importing ReportGroup [" + importMapping.ReportGroup.Name + "]. " +
                                        "ReportGroupId [" + importMapping.ReportGroup.ReportGroupId + "]. " +
                                        "Could not add";
                    base.LogError(new SoeImportReportException(message, importReport.ReportId, destinationReport.ReportId, importCompanyId, destinationCompanyId, this.ToString()), this.log);

                    result = false;
                }

                //ReportGroup added 
                if (reportGroupOk)
                {
                    //Add ReportGroupMapping to destination Report
                    ReportGroupMapping destReportGroupMapping = new ReportGroupMapping()
                    {
                        Order = reportGroupMappingOrder,

                        //References
                        Report = destinationReport,
                        ReportGroup = destinationReportGroup,
                    };

                    if (AddEntityItem(entities, destReportGroupMapping, "ReportGroupMapping").Success)
                    {
                        reportGroupMappingOrder++;
                    }
                    else
                    {
                        string message = "Error while importing ReportGroupMapping for ReportGroup [" + destinationReportGroup.Name + "]. " +
                                         "ReportGroupId [" + importMapping.ReportGroup.ReportGroupId + "]. " +
                                         "Could not add";
                        base.LogError(new SoeImportReportException(message, importReport.ReportId, destinationReport.ReportId, importCompanyId, destinationCompanyId, this.ToString()), this.log);

                        result = false;
                    }
                }
            }


            return result;
        }

        public bool ImportReportHeadersAndReportGroupsFromReportCreateNew(int importReportId, int destinationReportId, int importCompanyId, int destinationCompanyId)
        {
            bool result = true;

            using (CompEntities entities = new CompEntities())
            {
                Report importReport = GetReport(entities, importReportId, importCompanyId, true);
                if (importReport == null)
                    return false;

                Company destinationCompany = CompanyManager.GetCompany(entities, destinationCompanyId);
                if (destinationCompany == null)
                    return false;

                Report destinationReport = GetReport(entities, destinationReportId, destinationCompanyId, true);
                if (destinationReport == null)
                    return false;

                //Get ReportGroups for destination Company once
                List<ReportGroup> destinationReportGroups = GetReportGroupsByModule(entities, destinationReport.Module, destinationCompanyId, -1, true, true);
                List<ReportHeader> destinationReportHeaders = GetReportHeadersByModule(entities, destinationReport.Module, destinationCompanyId, -1, false);
                int reportGroupMappingOrder = 1;
                foreach (ReportGroupMapping importMapping in importReport.ReportGroupMapping.OrderBy(i => i.Order))
                {
                    bool reportGroupOk = false;

                    if (!importMapping.ReportGroupReference.IsLoaded)
                        importMapping.ReportGroupReference.Load();

                    if (importMapping.ReportGroup == null)
                        continue;

                    //Check if ReportGroup already exist in destination Company
                    bool reportGroupExists = destinationReportGroups.Any(rg => rg.Name == importMapping.ReportGroup.Name && rg.Description == importMapping.ReportGroup.Description);

                    //Add ReportGroup to destination Company
                    ReportGroup destinationReportGroup = new ReportGroup()
                    {
                        Name = importMapping.ReportGroup.Name,
                        Description = importMapping.ReportGroup.Description,
                        ShowSum = importMapping.ReportGroup.ShowSum,
                        ShowLabel = importMapping.ReportGroup.ShowLabel,
                        TemplateTypeId = importMapping.ReportGroup.TemplateTypeId,
                        Module = importMapping.ReportGroup.Module,
                        InvertRow = importMapping.ReportGroup.InvertRow,

                        //Set references
                        Company = destinationCompany,
                    };

                    if (reportGroupExists)
                        destinationReportGroup.Description += GetText(8561, "Kopierad från mall") + " " + DateTime.Now.ToShortDateString();

                    if (AddEntityItem(entities, destinationReportGroup, "ReportGroup").Success)
                    {
                        //Make sure ReportGroupHeaderMapping is loaded
                        if (!importMapping.ReportGroup.ReportGroupHeaderMapping.IsLoaded)
                            importMapping.ReportGroup.ReportGroupHeaderMapping.Load();

                        int reportGroupHeaderMappingOrder = 1;
                        foreach (ReportGroupHeaderMapping importReportGroupHeaderMapping in importMapping.ReportGroup.ReportGroupHeaderMapping.OrderBy(i => i.Order))
                        {
                            //Make sure ReportHeader is loaded
                            if (!importReportGroupHeaderMapping.ReportHeaderReference.IsLoaded)
                                importReportGroupHeaderMapping.ReportHeaderReference.Load();

                            if (importReportGroupHeaderMapping.ReportHeader == null)
                                continue;

                            bool reportHeaderExits = destinationReportHeaders.Any(x => x.Name == importReportGroupHeaderMapping.ReportHeader.Name && x.Description == importReportGroupHeaderMapping.ReportHeader.Description);

                            //Add ReportHeader for destination Company
                            ReportHeader destReportHeader = new ReportHeader()
                            {
                                Name = importReportGroupHeaderMapping.ReportHeader.Name,
                                Description = importReportGroupHeaderMapping.ReportHeader.Description,
                                ShowRow = importReportGroupHeaderMapping.ReportHeader.ShowRow,
                                ShowZeroRow = importReportGroupHeaderMapping.ReportHeader.ShowZeroRow,
                                ShowSum = importReportGroupHeaderMapping.ReportHeader.ShowSum,
                                ShowLabel = importReportGroupHeaderMapping.ReportHeader.ShowLabel,
                                TemplateTypeId = importReportGroupHeaderMapping.ReportHeader.TemplateTypeId,
                                Module = importReportGroupHeaderMapping.ReportHeader.Module,
                                DoNotSummarizeOnGroup = importReportGroupHeaderMapping.ReportHeader.DoNotSummarizeOnGroup,
                                InvertRow = importReportGroupHeaderMapping.ReportHeader.InvertRow,

                                //Set references
                                Company = destinationCompany,
                            };

                            if (reportHeaderExits)
                                destReportHeader.Description += GetText(8561, "Kopierad från mall") + " " + DateTime.Now.ToShortDateString();


                            if (AddEntityItem(entities, destReportHeader, "ReportHeader").Success)
                            {
                                //Make sure ReportHeaderInterval is loaded
                                if (!importReportGroupHeaderMapping.ReportHeader.ReportHeaderInterval.IsLoaded)
                                    importReportGroupHeaderMapping.ReportHeader.ReportHeaderInterval.Load();

                                foreach (ReportHeaderInterval templateReportHeaderInterval in importReportGroupHeaderMapping.ReportHeader.ReportHeaderInterval)
                                {
                                    ReportHeaderInterval reportHeaderInterval = new ReportHeaderInterval
                                    {
                                        IntervalFrom = templateReportHeaderInterval.IntervalFrom,
                                        IntervalTo = templateReportHeaderInterval.IntervalTo,
                                        SelectValue = templateReportHeaderInterval.SelectValue,

                                        //References
                                        ReportHeader = destReportHeader,
                                    };

                                    if (!AddEntityItem(entities, reportHeaderInterval, "ReportHeaderInterval").Success)
                                    {
                                        string message = "Error while importing ReportHeaderInterval for ReportHeader [" + destReportHeader.Name + "]. " +
                                                            "ReportHeaderId [" + destReportHeader.ReportHeaderId + "]. " +
                                                            "Could not add";
                                        base.LogError(new SoeImportReportException(message, importReport.ReportId, destinationReport.ReportId, importCompanyId, destinationCompanyId, this.ToString()), this.log);

                                        result = false;
                                    }
                                }

                                //Add ReportGroupHeaderMapping to destination ReportGroup
                                ReportGroupHeaderMapping destReportGroupHeaderMapping = new ReportGroupHeaderMapping()
                                {
                                    Order = reportGroupHeaderMappingOrder,

                                    //References
                                    ReportHeader = destReportHeader,
                                    ReportGroup = destinationReportGroup,
                                };

                                if (AddEntityItem(entities, destReportGroupHeaderMapping, "ReportGroupHeaderMapping").Success)
                                {
                                    reportGroupHeaderMappingOrder++;
                                }
                                else
                                {
                                    string message = "Error while importing ReportGroupHeaderMapping for ReportGroup [" + destinationReportGroup.Name + "]. " +
                                                        "ReportGroupId [" + destinationReportGroup.ReportGroupId + "]. " +
                                                        "Could not add";
                                    base.LogError(new SoeImportReportException(message, importReport.ReportId, destinationReport.ReportId, importCompanyId, destinationCompanyId, this.ToString()), this.log);

                                    result = false;
                                }
                            }
                            else
                            {
                                string message = "Error while importing ReportHeader [" + destReportHeader.Name + "]. " +
                                                    "ReportHeaderId [" + destReportHeader.ReportHeaderId + "]. " +
                                                    "Could not add";
                                base.LogError(new SoeImportReportException(message, importReport.ReportId, destinationReport.ReportId, importCompanyId, destinationCompanyId, this.ToString()), this.log);

                                result = false;
                            }
                        }

                        reportGroupOk = true;
                    }
                    else
                    {
                        string message = "Error while importing ReportGroup [" + importMapping.ReportGroup.Name + "]. " +
                                            "ReportGroupId [" + importMapping.ReportGroup.ReportGroupId + "]. " +
                                            "Could not add";
                        base.LogError(new SoeImportReportException(message, importReport.ReportId, destinationReport.ReportId, importCompanyId, destinationCompanyId, this.ToString()), this.log);

                        result = false;
                    }

                    //ReportGroup added 
                    if (reportGroupOk)
                    {
                        //Add ReportGroupMapping to destination Report
                        ReportGroupMapping destReportGroupMapping = new ReportGroupMapping()
                        {
                            Order = reportGroupMappingOrder,

                            //References
                            Report = destinationReport,
                            ReportGroup = destinationReportGroup,
                        };

                        if (AddEntityItem(entities, destReportGroupMapping, "ReportGroupMapping").Success)
                        {
                            reportGroupMappingOrder++;
                        }
                        else
                        {
                            string message = "Error while importing ReportGroupMapping for ReportGroup [" + destinationReportGroup.Name + "]. " +
                                             "ReportGroupId [" + importMapping.ReportGroup.ReportGroupId + "]. " +
                                             "Could not add";
                            base.LogError(new SoeImportReportException(message, importReport.ReportId, destinationReport.ReportId, importCompanyId, destinationCompanyId, this.ToString()), this.log);

                            result = false;
                        }
                    }
                }
            }

            return result;
        }

        public bool ImportReportHeadersAndReportGroupsFromReportReuseExisting(CompEntities entities, int importReportId, int destinationReportId, int importCompanyId, int destinationCompanyId)
        {
            bool result = true;

            Report importReport = GetReport(entities, importReportId, importCompanyId, true);
            if (importReport == null)
                return false;

            Company destinationCompany = CompanyManager.GetCompany(entities, destinationCompanyId);
            if (destinationCompany == null)
                return false;

            Report destinationReport = GetReport(entities, destinationReportId, destinationCompanyId, true);
            if (destinationReport == null)
                return false;

            //Get ReportGroups for destination Company once
            List<ReportGroup> destinationReportGroups = GetReportGroupsByModule(entities, destinationReport.Module, destinationCompanyId, -1, true, true);

            int reportGroupMappingOrder = 1;
            foreach (ReportGroupMapping importMapping in importReport.ReportGroupMapping.OrderBy(i => i.Order))
            {
                bool reportGroupOk = false;

                if (!importMapping.ReportGroupReference.IsLoaded)
                    importMapping.ReportGroupReference.Load();

                if (importMapping.ReportGroup == null)
                    continue;

                //Check if ReportGroup already exist in destination Company
                ReportGroup destinationReportGroup = destinationReportGroups.FirstOrDefault(rg => rg.Name == importMapping.ReportGroup.Name && rg.Description == importMapping.ReportGroup.Description);
                if (destinationReportGroup != null)
                {
                    //Re-use ReportGroup if it exists with same name in destination Company
                    //Do not need to copy ReportGroup, ReportGroupHeaderMapping, ReportHeader and ReportHeaderIntervals
                    reportGroupOk = true;
                }
                else
                {
                    //Add ReportGroup to destination Company
                    destinationReportGroup = new ReportGroup()
                    {
                        Name = importMapping.ReportGroup.Name,
                        Description = importMapping.ReportGroup.Description,
                        ShowSum = importMapping.ReportGroup.ShowSum,
                        ShowLabel = importMapping.ReportGroup.ShowLabel,
                        TemplateTypeId = importMapping.ReportGroup.TemplateTypeId,
                        Module = importMapping.ReportGroup.Module,
                        InvertRow = importMapping.ReportGroup.InvertRow,

                        //Set references
                        Company = destinationCompany,
                    };

                    if (AddEntityItem(entities, destinationReportGroup, "ReportGroup").Success)
                    {
                        //Make sure ReportGroupHeaderMapping is loaded
                        if (!importMapping.ReportGroup.ReportGroupHeaderMapping.IsLoaded)
                            importMapping.ReportGroup.ReportGroupHeaderMapping.Load();

                        int reportGroupHeaderMappingOrder = 1;
                        foreach (ReportGroupHeaderMapping importReportGroupHeaderMapping in importMapping.ReportGroup.ReportGroupHeaderMapping.OrderBy(i => i.Order))
                        {
                            //Make sure ReportHeader is loaded
                            if (!importReportGroupHeaderMapping.ReportHeaderReference.IsLoaded)
                                importReportGroupHeaderMapping.ReportHeaderReference.Load();

                            if (importReportGroupHeaderMapping.ReportHeader == null)
                                continue;

                            //Add ReportHeader for destination Company
                            ReportHeader destReportHeader = new ReportHeader()
                            {
                                Name = importReportGroupHeaderMapping.ReportHeader.Name,
                                Description = importReportGroupHeaderMapping.ReportHeader.Description,
                                ShowRow = importReportGroupHeaderMapping.ReportHeader.ShowRow,
                                ShowZeroRow = importReportGroupHeaderMapping.ReportHeader.ShowZeroRow,
                                ShowSum = importReportGroupHeaderMapping.ReportHeader.ShowSum,
                                ShowLabel = importReportGroupHeaderMapping.ReportHeader.ShowLabel,
                                TemplateTypeId = importReportGroupHeaderMapping.ReportHeader.TemplateTypeId,
                                Module = importReportGroupHeaderMapping.ReportHeader.Module,

                                //Set references
                                Company = destinationCompany,
                            };

                            if (AddEntityItem(entities, destReportHeader, "ReportHeader").Success)
                            {
                                //Make sure ReportHeaderInterval is loaded
                                if (!importReportGroupHeaderMapping.ReportHeader.ReportHeaderInterval.IsLoaded)
                                    importReportGroupHeaderMapping.ReportHeader.ReportHeaderInterval.Load();

                                foreach (ReportHeaderInterval templateReportHeaderInterval in importReportGroupHeaderMapping.ReportHeader.ReportHeaderInterval)
                                {
                                    ReportHeaderInterval reportHeaderInterval = new ReportHeaderInterval
                                    {
                                        IntervalFrom = templateReportHeaderInterval.IntervalFrom,
                                        IntervalTo = templateReportHeaderInterval.IntervalTo,
                                        SelectValue = templateReportHeaderInterval.SelectValue,

                                        //References
                                        ReportHeader = destReportHeader,
                                    };

                                    if (!AddEntityItem(entities, reportHeaderInterval, "ReportHeaderInterval").Success)
                                    {
                                        string message = "Error while importing ReportHeaderInterval for ReportHeader [" + destReportHeader.Name + "]. " +
                                                         "ReportHeaderId [" + destReportHeader.ReportHeaderId + "]. " +
                                                         "Could not add";
                                        base.LogError(new SoeImportReportException(message, importReport.ReportId, destinationReport.ReportId, importCompanyId, destinationCompanyId, this.ToString()), this.log);

                                        result = false;
                                    }
                                }

                                //Add ReportGroupHeaderMapping to destination ReportGroup
                                ReportGroupHeaderMapping destReportGroupHeaderMapping = new ReportGroupHeaderMapping()
                                {
                                    Order = reportGroupHeaderMappingOrder,

                                    //References
                                    ReportHeader = destReportHeader,
                                    ReportGroup = destinationReportGroup,
                                };

                                if (AddEntityItem(entities, destReportGroupHeaderMapping, "ReportGroupHeaderMapping").Success)
                                {
                                    reportGroupHeaderMappingOrder++;
                                }
                                else
                                {
                                    string message = "Error while importing ReportGroupHeaderMapping for ReportGroup [" + destinationReportGroup.Name + "]. " +
                                                     "ReportGroupId [" + destinationReportGroup.ReportGroupId + "]. " +
                                                     "Could not add";
                                    base.LogError(new SoeImportReportException(message, importReport.ReportId, destinationReport.ReportId, importCompanyId, destinationCompanyId, this.ToString()), this.log);

                                    result = false;
                                }
                            }
                            else
                            {
                                string message = "Error while importing ReportHeader [" + destReportHeader.Name + "]. " +
                                                 "ReportHeaderId [" + destReportHeader.ReportHeaderId + "]. " +
                                                 "Could not add";
                                base.LogError(new SoeImportReportException(message, importReport.ReportId, destinationReport.ReportId, importCompanyId, destinationCompanyId, this.ToString()), this.log);

                                result = false;
                            }
                        }

                        reportGroupOk = true;
                    }
                    else
                    {
                        string message = "Error while importing ReportGroup [" + importMapping.ReportGroup.Name + "]. " +
                                         "ReportGroupId [" + importMapping.ReportGroup.ReportGroupId + "]. " +
                                         "Could not add";
                        base.LogError(new SoeImportReportException(message, importReport.ReportId, destinationReport.ReportId, importCompanyId, destinationCompanyId, this.ToString()), this.log);

                        result = false;
                    }
                }

                //ReportGroup added or already exist in destination Company
                if (reportGroupOk)
                {
                    //Add ReportGroupMapping to destination Report
                    ReportGroupMapping destReportGroupMapping = new ReportGroupMapping()
                    {
                        Order = reportGroupMappingOrder,

                        //References
                        Report = destinationReport,
                        ReportGroup = destinationReportGroup,
                    };

                    if (AddEntityItem(entities, destReportGroupMapping, "ReportGroupMapping").Success)
                    {
                        reportGroupMappingOrder++;
                    }
                    else
                    {
                        string message = "Error while importing ReportGroupMapping for ReportGroup [" + destinationReportGroup.Name + "]. " +
                                         "ReportGroupId [" + importMapping.ReportGroup.ReportGroupId + "]. " +
                                         "Could not add";
                        base.LogError(new SoeImportReportException(message, importReport.ReportId, destinationReport.ReportId, importCompanyId, destinationCompanyId, this.ToString()), this.log);

                        result = false;
                    }
                }
            }


            return result;
        }

        public bool ImportReportHeadersAndReportGroupsFromReportReuseExisting(int importReportId, int destinationReportId, int importCompanyId, int destinationCompanyId)
        {
            bool result = true;

            using (CompEntities entities = new CompEntities())
            {
                Report importReport = GetReport(entities, importReportId, importCompanyId, true);
                if (importReport == null)
                    return false;

                Company destinationCompany = CompanyManager.GetCompany(entities, destinationCompanyId);
                if (destinationCompany == null)
                    return false;

                Report destinationReport = GetReport(entities, destinationReportId, destinationCompanyId, true);
                if (destinationReport == null)
                    return false;

                //Get ReportGroups for destination Company once
                List<ReportGroup> destinationReportGroups = GetReportGroupsByModule(entities, destinationReport.Module, destinationCompanyId, -1, true, true);

                int reportGroupMappingOrder = 1;
                foreach (ReportGroupMapping importMapping in importReport.ReportGroupMapping.OrderBy(i => i.Order))
                {
                    bool reportGroupOk = false;

                    if (!importMapping.ReportGroupReference.IsLoaded)
                        importMapping.ReportGroupReference.Load();

                    if (importMapping.ReportGroup == null)
                        continue;

                    //Check if ReportGroup already exist in destination Company
                    ReportGroup destinationReportGroup = destinationReportGroups.FirstOrDefault(rg => rg.Name == importMapping.ReportGroup.Name && rg.Description == importMapping.ReportGroup.Description);
                    if (destinationReportGroup != null)
                    {
                        //Re-use ReportGroup if it exists with same name in destination Company
                        //Do not need to copy ReportGroup, ReportGroupHeaderMapping, ReportHeader and ReportHeaderIntervals
                        reportGroupOk = true;
                    }
                    else
                    {
                        //Add ReportGroup to destination Company
                        destinationReportGroup = new ReportGroup()
                        {
                            Name = importMapping.ReportGroup.Name,
                            Description = importMapping.ReportGroup.Description,
                            ShowSum = importMapping.ReportGroup.ShowSum,
                            ShowLabel = importMapping.ReportGroup.ShowLabel,
                            TemplateTypeId = importMapping.ReportGroup.TemplateTypeId,
                            Module = importMapping.ReportGroup.Module,
                            InvertRow = importMapping.ReportGroup.InvertRow,

                            //Set references
                            Company = destinationCompany,
                        };

                        if (AddEntityItem(entities, destinationReportGroup, "ReportGroup").Success)
                        {
                            //Make sure ReportGroupHeaderMapping is loaded
                            if (!importMapping.ReportGroup.ReportGroupHeaderMapping.IsLoaded)
                                importMapping.ReportGroup.ReportGroupHeaderMapping.Load();

                            int reportGroupHeaderMappingOrder = 1;
                            foreach (ReportGroupHeaderMapping importReportGroupHeaderMapping in importMapping.ReportGroup.ReportGroupHeaderMapping.OrderBy(i => i.Order))
                            {
                                //Make sure ReportHeader is loaded
                                if (!importReportGroupHeaderMapping.ReportHeaderReference.IsLoaded)
                                    importReportGroupHeaderMapping.ReportHeaderReference.Load();

                                if (importReportGroupHeaderMapping.ReportHeader == null)
                                    continue;

                                //Add ReportHeader for destination Company
                                ReportHeader destReportHeader = new ReportHeader()
                                {
                                    Name = importReportGroupHeaderMapping.ReportHeader.Name,
                                    Description = importReportGroupHeaderMapping.ReportHeader.Description,
                                    ShowRow = importReportGroupHeaderMapping.ReportHeader.ShowRow,
                                    ShowZeroRow = importReportGroupHeaderMapping.ReportHeader.ShowZeroRow,
                                    ShowSum = importReportGroupHeaderMapping.ReportHeader.ShowSum,
                                    ShowLabel = importReportGroupHeaderMapping.ReportHeader.ShowLabel,
                                    TemplateTypeId = importReportGroupHeaderMapping.ReportHeader.TemplateTypeId,
                                    Module = importReportGroupHeaderMapping.ReportHeader.Module,

                                    //Set references
                                    Company = destinationCompany,
                                };

                                if (AddEntityItem(entities, destReportHeader, "ReportHeader").Success)
                                {
                                    //Make sure ReportHeaderInterval is loaded
                                    if (!importReportGroupHeaderMapping.ReportHeader.ReportHeaderInterval.IsLoaded)
                                        importReportGroupHeaderMapping.ReportHeader.ReportHeaderInterval.Load();

                                    foreach (ReportHeaderInterval templateReportHeaderInterval in importReportGroupHeaderMapping.ReportHeader.ReportHeaderInterval)
                                    {
                                        ReportHeaderInterval reportHeaderInterval = new ReportHeaderInterval
                                        {
                                            IntervalFrom = templateReportHeaderInterval.IntervalFrom,
                                            IntervalTo = templateReportHeaderInterval.IntervalTo,
                                            SelectValue = templateReportHeaderInterval.SelectValue,

                                            //References
                                            ReportHeader = destReportHeader,
                                        };

                                        if (!AddEntityItem(entities, reportHeaderInterval, "ReportHeaderInterval").Success)
                                        {
                                            string message = "Error while importing ReportHeaderInterval for ReportHeader [" + destReportHeader.Name + "]. " +
                                                             "ReportHeaderId [" + destReportHeader.ReportHeaderId + "]. " +
                                                             "Could not add";
                                            base.LogError(new SoeImportReportException(message, importReport.ReportId, destinationReport.ReportId, importCompanyId, destinationCompanyId, this.ToString()), this.log);

                                            result = false;
                                        }
                                    }

                                    //Add ReportGroupHeaderMapping to destination ReportGroup
                                    ReportGroupHeaderMapping destReportGroupHeaderMapping = new ReportGroupHeaderMapping()
                                    {
                                        Order = reportGroupHeaderMappingOrder,

                                        //References
                                        ReportHeader = destReportHeader,
                                        ReportGroup = destinationReportGroup,
                                    };

                                    if (AddEntityItem(entities, destReportGroupHeaderMapping, "ReportGroupHeaderMapping").Success)
                                    {
                                        reportGroupHeaderMappingOrder++;
                                    }
                                    else
                                    {
                                        string message = "Error while importing ReportGroupHeaderMapping for ReportGroup [" + destinationReportGroup.Name + "]. " +
                                                         "ReportGroupId [" + destinationReportGroup.ReportGroupId + "]. " +
                                                         "Could not add";
                                        base.LogError(new SoeImportReportException(message, importReport.ReportId, destinationReport.ReportId, importCompanyId, destinationCompanyId, this.ToString()), this.log);

                                        result = false;
                                    }
                                }
                                else
                                {
                                    string message = "Error while importing ReportHeader [" + destReportHeader.Name + "]. " +
                                                     "ReportHeaderId [" + destReportHeader.ReportHeaderId + "]. " +
                                                     "Could not add";
                                    base.LogError(new SoeImportReportException(message, importReport.ReportId, destinationReport.ReportId, importCompanyId, destinationCompanyId, this.ToString()), this.log);

                                    result = false;
                                }
                            }

                            reportGroupOk = true;
                        }
                        else
                        {
                            string message = "Error while importing ReportGroup [" + importMapping.ReportGroup.Name + "]. " +
                                             "ReportGroupId [" + importMapping.ReportGroup.ReportGroupId + "]. " +
                                             "Could not add";
                            base.LogError(new SoeImportReportException(message, importReport.ReportId, destinationReport.ReportId, importCompanyId, destinationCompanyId, this.ToString()), this.log);

                            result = false;
                        }
                    }

                    //ReportGroup added or already exist in destination Company
                    if (reportGroupOk)
                    {
                        //Add ReportGroupMapping to destination Report
                        ReportGroupMapping destReportGroupMapping = new ReportGroupMapping()
                        {
                            Order = reportGroupMappingOrder,

                            //References
                            Report = destinationReport,
                            ReportGroup = destinationReportGroup,
                        };

                        if (AddEntityItem(entities, destReportGroupMapping, "ReportGroupMapping").Success)
                        {
                            reportGroupMappingOrder++;
                        }
                        else
                        {
                            string message = "Error while importing ReportGroupMapping for ReportGroup [" + destinationReportGroup.Name + "]. " +
                                             "ReportGroupId [" + importMapping.ReportGroup.ReportGroupId + "]. " +
                                             "Could not add";
                            base.LogError(new SoeImportReportException(message, importReport.ReportId, destinationReport.ReportId, importCompanyId, destinationCompanyId, this.ToString()), this.log);

                            result = false;
                        }
                    }
                }
            }

            return result;
        }

        public ActionResult AddReportGroup(ReportGroup reportGroup, int actorCompanyId)
        {
            if (reportGroup == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "ReportGroup");

            using (CompEntities entities = new CompEntities())
            {
                reportGroup.Company = CompanyManager.GetCompany(entities, actorCompanyId);
                if (reportGroup.Company == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte"));

                var result = AddEntityItem(entities, reportGroup, "ReportGroup");
                result.IntegerValue = reportGroup.ReportGroupId;
                return result;
            }
        }

        public ActionResult UpdateReportGroup(ReportGroup reportGroup, int actorCompanyId)
        {
            if (reportGroup == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "ReportGroup");

            using (CompEntities entities = new CompEntities())
            {
                ReportGroup orginalReportGroup = GetReportGroup(entities, reportGroup.ReportGroupId, actorCompanyId, false, false);
                if (orginalReportGroup == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "ReportGroup");

                return UpdateEntityItem(entities, orginalReportGroup, reportGroup, "ReportGroup");
            }
        }
        public ActionResult DeleteReportGroup(ReportGroup reportGroup, int actorCompanyId)
        {
            if (reportGroup == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, "ReportGroup");

            //Check relation dependencies
            if (IsReportGroupInUse(reportGroup))
                return new ActionResult((int)ActionResultDelete.ReportGroupInUse);

            using (CompEntities entities = new CompEntities())
            {
                ReportGroup orginalReportGroup = GetReportGroup(entities, reportGroup.ReportGroupId, actorCompanyId, false, false);
                if (orginalReportGroup == null)
                    return new ActionResult((int)ActionResultDelete.EntityNotFound, "ReportGroup");

                return ChangeEntityState(entities, orginalReportGroup, SoeEntityState.Deleted, true);
            }
        }

        public ActionResult DeleteReportGroups(List<int> reportGroupIds, int actorCompanyId)
        {
            ActionResult actionResult = new ActionResult();
            int delete_error = 0;

            foreach (int reportGroupId in reportGroupIds)
            {
                // Loop each one to delete
                using (CompEntities entities = new CompEntities())
                {
                    if (ReportGroupExistsInAnyReport(entities, reportGroupId))
                    {
                        actionResult.ErrorNumber = (int)ActionResultDelete.ReportGroupInUse;
                        delete_error++;
                    }
                    else
                    {
                        ReportGroup orginalReportGroup = GetReportGroup(entities, reportGroupId, actorCompanyId, false, false);
                        actionResult = ChangeEntityState(entities, orginalReportGroup, SoeEntityState.Deleted, true);
                    }
                }
            }

            if (delete_error > 0)
            {
                actionResult.ObjectsAffected = reportGroupIds.Count - delete_error;
                actionResult.IntegerValue = reportGroupIds.Count - delete_error;
                actionResult.IntegerValue2 = delete_error;  // Return errors
            }
            else actionResult.IntegerValue = reportGroupIds.Count;

            return actionResult;
        }

        #endregion

        #region ReportGroupMapping

        public List<ReportGroupMapping> GetReportGroupMappings(int reportId, int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.ReportGroupMapping.NoTracking();
            return GetReportGroupMappings(entities, reportId, actorCompanyId);
        }

        public List<ReportGroupMapping> GetReportGroupMappings(CompEntities entities, int reportId, int actorCompanyId)
        {
            return (from m in entities.ReportGroupMapping
                        .Include("ReportGroup")
                    where m.Report.ActorCompanyId == actorCompanyId &&
                    m.ReportId == reportId &&
                    m.Report.State == (int)SoeEntityState.Active &&
                    m.ReportGroup.State == (int)SoeEntityState.Active
                    orderby m.Order
                    select m).ToList();
        }

        public ReportGroupMapping GetReportGroupMapping(CompEntities entities, int reportId, int reportGroupId)
        {
            return (from m in entities.ReportGroupMapping
                    where m.ReportId == reportId &&
                    m.ReportGroupId == reportGroupId &&
                    m.Report.State == (int)SoeEntityState.Active &&
                    m.ReportGroup.State == (int)SoeEntityState.Active
                    select m).FirstOrDefault();
        }

        public bool ReportGroupExistInReport(int reportId, int reportGroupId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.ReportGroupMapping.NoTracking();
            return ReportGroupExistInReport(entities, reportId, reportGroupId);
        }

        public bool ReportGroupExistInReport(CompEntities entities, int reportId, int reportGroupId)
        {
            int counter = (from m in entities.ReportGroupMapping
                           where m.ReportId == reportId &&
                           m.ReportGroupId == reportGroupId
                           select m).Count();

            if (counter > 0)
                return true;
            return false;
        }

        public bool ReportGroupExistsInAnyReport(CompEntities entities, int reportGroupId)
        {
            var existingMapping = (from m in entities.ReportGroupMapping
                                   where m.ReportGroupId == reportGroupId &&
                                   m.Report.State == (int)SoeEntityState.Active
                                   select m).FirstOrDefault();

            if (existingMapping != null)
                return true;
            return false;
        }

        public ActionResult AddReportGroupMapping(ReportGroupMapping reportGroupMapping, int reportGroupId, int reportId, int actorCompanyId)
        {
            if (reportGroupMapping == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "ReportGroupMapping");

            using (CompEntities entities = new CompEntities())
            {
                reportGroupMapping.Report = GetReport(entities, reportId, actorCompanyId);
                if (reportGroupMapping.Report == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "Report");

                reportGroupMapping.ReportGroup = GetReportGroup(entities, reportGroupId, actorCompanyId, false, false);
                if (reportGroupMapping.ReportGroup == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "ReportGroup");

                return AddEntityItem(entities, reportGroupMapping, "ReportGroupMapping");
            }
        }

        public ActionResult DeleteReportGroupMapping(int reportId, int reportGroupId)
        {
            using (CompEntities entities = new CompEntities())
            {
                ReportGroupMapping reportGroupMapping = GetReportGroupMapping(entities, reportId, reportGroupId);
                if (reportGroupMapping == null)
                    return new ActionResult((int)ActionResultDelete.EntityNotFound, "ReportGroupMapping");

                return DeleteEntityItem(entities, reportGroupMapping);
            }
        }

        public ActionResult ReorderReportGroupMapping(int reportId, int reportGroupId, int actorCompanyId, bool isUp)
        {
            using (CompEntities entities = new CompEntities())
            {
                int order = 1;
                bool changeNext = false;
                ReportGroupMapping current = null;
                ReportGroupMapping prev = null;

                List<ReportGroupMapping> reportGroupMappings = GetReportGroupMappings(entities, reportId, actorCompanyId);
                foreach (ReportGroupMapping reportGroupMapping in reportGroupMappings)
                {
                    current = reportGroupMapping;
                    current.Order = order;

                    if (changeNext)
                    {
                        changeNext = false;
                        current.Order = order - 1;
                    }

                    if (current.ReportGroupId == reportGroupId)
                    {
                        if (isUp && prev != null && reportGroupMapping.Order > 1)
                        {
                            current.Order = prev.Order;
                            prev.Order = order;
                        }
                        else if (!isUp)
                        {
                            changeNext = true;
                            current.Order = order + 1;
                        }
                    }

                    order++;
                    prev = current;
                }

                if (changeNext)
                    current.Order--;

                return SaveChanges(entities);
            }
        }

        #endregion

        #region ReportHeader

        public List<ReportHeader> GetReportHeadersByCompany(int actorCompanyId, bool loadReportHeaderInterval = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.ReportHeader.NoTracking();
            return GetReportHeadersByCompany(entities, actorCompanyId, loadReportHeaderInterval);
        }

        public List<ReportHeader> GetReportHeadersByCompany(CompEntities entities, int actorCompanyId, bool loadReportHeaderInterval = false)
        {
            List<ReportHeader> reportHeaders = (from rh in entities.ReportHeader
                                                where rh.Company.ActorCompanyId == actorCompanyId &&
                                                rh.State == (int)SoeEntityState.Active
                                                orderby rh.TemplateTypeId, rh.Name ascending
                                                select rh).ToList();

            if (loadReportHeaderInterval)
            {
                foreach (ReportHeader reportHeader in reportHeaders)
                {
                    if (!reportHeader.ReportHeaderInterval.IsLoaded)
                        reportHeader.ReportHeaderInterval.Load();
                }
            }

            return reportHeaders;
        }

        public List<ReportHeader> GetReportHeadersByModule(int module, int actorCompanyId, int templateTypeId, bool loadReportHeaderInterval = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.ReportHeader.NoTracking();
            return GetReportHeadersByModule(entities, module, actorCompanyId, templateTypeId, loadReportHeaderInterval);
        }

        public List<ReportHeader> GetReportHeadersByModule(CompEntities entities, int module, int actorCompanyId, int templateTypeId, bool loadReportHeaderInterval = false)
        {
            List<ReportHeader> reportHeaders = null;

            if (templateTypeId > 0)
            {
                reportHeaders = (from rh in entities.ReportHeader
                                 where rh.Company.ActorCompanyId == actorCompanyId &&
                                 rh.Module == module &&
                                 rh.State != (int)SoeEntityState.Deleted &&
                                 (rh.TemplateTypeId == templateTypeId || rh.TemplateTypeId == (int)SoeReportTemplateType.Generic)
                                 orderby rh.Name ascending
                                 select rh).ToList();
            }
            else
            {
                reportHeaders = (from rh in entities.ReportHeader
                                 where rh.Company.ActorCompanyId == actorCompanyId &&
                                 rh.Module == module &&
                                 rh.State != (int)SoeEntityState.Deleted
                                 orderby rh.TemplateTypeId, rh.Name ascending
                                 select rh).ToList();
            }

            if (loadReportHeaderInterval)
            {
                foreach (ReportHeader reportHeader in reportHeaders)
                {
                    if (!reportHeader.ReportHeaderInterval.IsLoaded)
                        reportHeader.ReportHeaderInterval.Load();
                }
            }

            var sysReportTemplateTypeTerms = base.GetTermGroupDict(TermGroup.SysReportTemplateType);
            foreach (ReportHeader reportHeader in reportHeaders)
            {
                if (sysReportTemplateTypeTerms.ContainsKey(reportHeader.TemplateTypeId))
                    reportHeader.TemplateType = sysReportTemplateTypeTerms[reportHeader.TemplateTypeId];
            }

            return reportHeaders;
        }

        public List<ReportHeader> GetReportHeadersByModule(int module, int actorCompanyId, bool loadReportHeaderInterval = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.ReportHeader.NoTracking();
            return GetReportHeadersByModule(entities, module, actorCompanyId, loadReportHeaderInterval);
        }

        public List<ReportHeader> GetReportHeadersByModule(CompEntities entities, int module, int actorCompanyId, bool loadReportHeaderInterval = false)
        {
            var sysReportTemplateTypeTerms = base.GetTermGroupDict(TermGroup.SysReportTemplateType);

            // Need to complete resultset with template type
            List<ReportHeader> reportHeaders = GetReportHeadersByModule(entities, module, actorCompanyId, -1, loadReportHeaderInterval);
            foreach (ReportHeader reportHeader in reportHeaders)
            {
                if (sysReportTemplateTypeTerms.ContainsKey(reportHeader.TemplateTypeId))
                    reportHeader.TemplateType = sysReportTemplateTypeTerms[reportHeader.TemplateTypeId];
            }

            return reportHeaders;
        }

        public List<ReportHeader> GetReportHeadersByReport(CompEntities entities, int reportGroupId, int actorCompanyId, bool loadReportHeaderInterval = false)
        {
            List<ReportHeader> reportHeaders = (from m in entities.ReportGroupHeaderMapping
                                                where m.ReportGroup.ReportGroupId == reportGroupId &&
                                                m.ReportGroup.Company.ActorCompanyId == actorCompanyId
                                                orderby m.Order ascending
                                                select m.ReportHeader).ToList();

            if (loadReportHeaderInterval)
            {
                foreach (ReportHeader reportHeader in reportHeaders)
                {
                    if (!reportHeader.ReportHeaderInterval.IsLoaded)
                        reportHeader.ReportHeaderInterval.Load();
                }
            }

            return reportHeaders;
        }

        public ReportHeader GetReportHeader(int reportHeaderId, int actorCompanyId, bool loadReportHeaderInterval)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.ReportHeader.NoTracking();
            return GetReportHeader(entities, reportHeaderId, actorCompanyId, loadReportHeaderInterval);
        }

        public ReportHeader GetReportHeader(CompEntities entities, int reportHeaderId, int actorCompanyId, bool loadReportHeaderInterval, bool onlyActive = true)
        {
            ReportHeader reportHeader = (from r in entities.ReportHeader
                                         where r.ReportHeaderId == reportHeaderId &&
                                         r.Company.ActorCompanyId == actorCompanyId &&
                                         ((onlyActive && r.State == (int)SoeEntityState.Active) || (!onlyActive && r.State != (int)SoeEntityState.Deleted))
                                         select r).FirstOrDefault();

            if (reportHeader != null && loadReportHeaderInterval && !reportHeader.ReportHeaderInterval.IsLoaded)
                reportHeader.ReportHeaderInterval.Load();

            return reportHeader;
        }

        public ReportHeader GetPrevNextReportHeaderById(int reportHeaderId, int module, int actorCompanyId, SoeFormMode mode)
        {
            ReportHeader reportHeader = null;
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            entitiesReadOnly.ReportHeader.NoTracking();
            if (mode == SoeFormMode.Next)
            {
                reportHeader = (from r in entitiesReadOnly.ReportHeader
                                where r.ReportHeaderId > reportHeaderId &&
                                r.Module == module &&
                                r.Company.ActorCompanyId == actorCompanyId &&
                                r.State == (int)SoeEntityState.Active
                                orderby r.ReportHeaderId ascending
                                select r).FirstOrDefault();
            }
            else
            {
                reportHeader = (from r in entitiesReadOnly.ReportHeader
                                where r.ReportHeaderId < reportHeaderId &&
                                r.Module == module &&
                                r.Company.ActorCompanyId == actorCompanyId &&
                                r.State == (int)SoeEntityState.Active
                                orderby r.ReportHeaderId descending
                                select r).FirstOrDefault();
            }

            return reportHeader;
        }

        public bool ReportHeaderInUse(ReportHeader reportHeader)
        {
            if (reportHeader == null)
                return false;
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            int counter = (from r in entitiesReadOnly.ReportGroupHeaderMapping
                           where r.ReportHeaderId == reportHeader.ReportHeaderId
                           select r).Count();

            if (counter > 0)
                return true;
            return false;
        }

        public bool ReportHeaderExistsInAnyGroup(CompEntities entities, int reportHeaderId)
        {
            var existingGroup = (from m in entities.ReportGroupHeaderMapping
                                 where m.ReportHeaderId == reportHeaderId &&
                                 m.ReportGroup.State == (int)SoeEntityState.Active
                                 select m).FirstOrDefault();

            if (existingGroup != null)
                return true;
            return false;
        }

        public ActionResult AddReportHeader(ReportHeader reportHeader, int actorCompanyId, Collection<FormIntervalEntryItem> formIntervalEntryItems)
        {
            if (reportHeader == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "ReportHeader");

            using (CompEntities entities = new CompEntities())
            {
                reportHeader.Company = CompanyManager.GetCompany(entities, actorCompanyId);
                if (reportHeader.Company == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte"));

                ActionResult result = AddEntityItem(entities, reportHeader, "ReportHeader");
                if (!result.Success)
                    return result;

                foreach (FormIntervalEntryItem formIntervalEntryItem in formIntervalEntryItems)
                {
                    ReportHeaderInterval reportHeaderInterval = new ReportHeaderInterval
                    {
                        ReportHeader = reportHeader,
                        IntervalFrom = formIntervalEntryItem.From,
                        IntervalTo = formIntervalEntryItem.To,
                    };

                    if (formIntervalEntryItem.LabelType != 0)
                        reportHeaderInterval.SelectValue = formIntervalEntryItem.LabelType;

                    AddEntityItem(entities, reportHeaderInterval, "ReportHeaderInterval");
                }

                return result;
            }
        }

        public ActionResult UpdateReportHeader(ReportHeader reportHeader, int actorCompanyId)
        {
            if (reportHeader == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "ReportHeader");

            using (CompEntities entities = new CompEntities())
            {
                ReportHeader orginalReportHeader = GetReportHeader(entities, reportHeader.ReportHeaderId, actorCompanyId, false);
                if (orginalReportHeader == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "ReportHeader");

                return UpdateEntityItem(entities, orginalReportHeader, reportHeader, "ReportHeader");
            }
        }

        public ActionResult DeleteReportHeader(ReportHeader reportHeader, int actorCompanyId)
        {
            if (reportHeader == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, "ReportHeader");

            //Check relation dependencies
            if (ReportHeaderInUse(reportHeader))
                return new ActionResult((int)ActionResultDelete.ReportHeaderInUse);

            using (CompEntities entities = new CompEntities())
            {
                ReportHeader orginalReportHeader = GetReportHeader(entities, reportHeader.ReportHeaderId, actorCompanyId, false);
                if (orginalReportHeader == null)
                    return new ActionResult((int)ActionResultDelete.EntityNotFound, "ReportHeader");

                return ChangeEntityState(entities, orginalReportHeader, SoeEntityState.Deleted, true);
            }
        }


        public ActionResult DeleteReportHeaders(List<int> reportHeaderIds, int actorCompanyId)
        {
            ActionResult result = new ActionResult();
            int delete_error = 0;

            foreach (int reportHeaderId in reportHeaderIds)
            {
                // Loop each one to delete
                using (CompEntities entities = new CompEntities())
                {
                    if (ReportHeaderExistsInAnyGroup(entities, reportHeaderId))
                    {
                        result.ErrorNumber = (int)ActionResultDelete.ReportHeaderInUse;
                        delete_error++;
                    }
                    else
                    {
                        ReportHeader orginalReportHeader = GetReportHeader(entities, reportHeaderId, actorCompanyId, false);
                        result = ChangeEntityState(entities, orginalReportHeader, SoeEntityState.Deleted, true);
                    }
                }
            }
            if (delete_error > 0)
            {
                result.ObjectsAffected = reportHeaderIds.Count - delete_error;
                result.IntegerValue = reportHeaderIds.Count - delete_error;
                result.IntegerValue2 = delete_error;  // Return errors
            }
            else result.IntegerValue = reportHeaderIds.Count;

            return result;
        }

        #endregion

        #region ReportGroupHeaderMapping

        public List<ReportGroupHeaderMapping> GetReportGroupHeaderMappings(int reportGroupId, int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.ReportGroupHeaderMapping.NoTracking();
            return GetReportGroupHeaderMappings(entities, reportGroupId, actorCompanyId);
        }

        public List<ReportGroupHeaderMapping> GetReportGroupHeaderMappings(CompEntities entities, int reportGroupId, int actorCompanyId)
        {
            return (from m in entities.ReportGroupHeaderMapping
                        .Include("ReportHeader")
                    where m.ReportGroup.Company.ActorCompanyId == actorCompanyId &&
                    m.ReportGroupId == reportGroupId
                    orderby m.Order ascending
                    select m).ToList();
        }

        public List<ReportGroupHeaderMapping> GetReportGroupHeaderMappings(CompEntities entities, int actorCompanyId)
        {
            return (from m in entities.ReportGroupHeaderMapping
                        .Include("ReportGroup")
                        .Include("ReportHeader.ReportHeaderInterval")
                    where m.ReportGroup.Company.ActorCompanyId == actorCompanyId
                    orderby m.Order ascending
                    select m).ToList();
        }

        public ReportGroupHeaderMapping GetReportGroupHeaderMapping(CompEntities entities, int reportGroupId, int reportHeaderId)
        {
            return (from m in entities.ReportGroupHeaderMapping
                    where m.ReportHeaderId == reportHeaderId &&
                    m.ReportGroupId == reportGroupId
                    select m).FirstOrDefault();
        }

        public bool ReportHeaderExistInReportGroup(int reportGroupId, int reportHeaderId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.ReportGroupHeaderMapping.NoTracking();
            return ReportHeaderExistInReportGroup(entities, reportGroupId, reportHeaderId);
        }

        public bool ReportHeaderExistInReportGroup(CompEntities entities, int reportGroupId, int reportHeaderId)
        {
            int counter = (from m in entities.ReportGroupHeaderMapping
                           where m.ReportGroupId == reportGroupId &&
                           m.ReportHeaderId == reportHeaderId
                           select m).Count();

            if (counter > 0)
                return true;
            return false;
        }

        public ActionResult AddReportGroupHeaderMapping(ReportGroupHeaderMapping reportGroupHeaderMapping, int reportGroupId, int reportHeaderId, int actorCompanyId)
        {
            if (reportGroupHeaderMapping == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "DeleteReportHeader");

            using (CompEntities entities = new CompEntities())
            {
                reportGroupHeaderMapping.ReportHeader = GetReportHeader(entities, reportHeaderId, actorCompanyId, false);
                if (reportGroupHeaderMapping.ReportHeader == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "ReportHeader");

                reportGroupHeaderMapping.ReportGroup = GetReportGroup(entities, reportGroupId, actorCompanyId, false, false);
                if (reportGroupHeaderMapping.ReportGroup == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "ReportGroup");

                return AddEntityItem(entities, reportGroupHeaderMapping, "ReportGroupHeaderMapping");
            }
        }

        public ActionResult DeleteReportGroupHeaderMapping(int reportGroupId, int reportHeaderId)
        {
            using (CompEntities entities = new CompEntities())
            {
                ReportGroupHeaderMapping reportGroupHeaderMapping = GetReportGroupHeaderMapping(entities, reportGroupId, reportHeaderId);
                if (reportGroupHeaderMapping == null)
                    return new ActionResult((int)ActionResultDelete.EntityNotFound, "ReportGroupHeaderMapping");

                return DeleteEntityItem(entities, reportGroupHeaderMapping);
            }
        }

        public ActionResult DeleteReportGroupHeaderMappings(int reportGroupId, int actorCompanyId)
        {
            using (CompEntities entities = new CompEntities())
            {
                List<ReportGroupHeaderMapping> reportHeaderGroupMappings = GetReportGroupHeaderMappings(entities, reportGroupId, actorCompanyId);
                foreach (ReportGroupHeaderMapping reportHeaderGroupMapping in reportHeaderGroupMappings)
                {
                    entities.DeleteObject(reportHeaderGroupMapping);
                }

                return SaveDeletions(entities);
            }
        }

        public ActionResult ReorderReportGroupHeaderMapping(int reportGroupId, int reportHeaderId, bool isUp, int actorCompanyId)
        {
            using (CompEntities entities = new CompEntities())
            {
                int order = 1;
                bool changeNext = false;
                ReportGroupHeaderMapping current = null;
                ReportGroupHeaderMapping prev = null;

                List<ReportGroupHeaderMapping> reportHeaderGroupMappings = GetReportGroupHeaderMappings(entities, reportGroupId, actorCompanyId);
                foreach (ReportGroupHeaderMapping reportHeaderGroupMapping in reportHeaderGroupMappings)
                {
                    current = reportHeaderGroupMapping;
                    current.Order = order;

                    if (changeNext)
                    {
                        changeNext = false;
                        current.Order = order - 1;
                    }

                    if (current.ReportHeaderId == reportHeaderId)
                    {
                        if (isUp && prev != null && reportHeaderGroupMapping.Order > 1)
                        {
                            current.Order = prev.Order;
                            prev.Order = order;
                        }
                        else if (!isUp)
                        {
                            changeNext = true;
                            current.Order = order + 1;
                        }
                    }

                    order++;
                    prev = current;
                }

                if (changeNext)
                    current.Order--;

                return SaveChanges(entities);
            }
        }

        #endregion

        #region ReportHeaderInterval

        public IEnumerable<ReportHeaderInterval> GetReportHeaderIntervals(int reportHeaderId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.ReportHeaderInterval.NoTracking();
            return GetReportHeaderIntervals(entities, reportHeaderId);
        }

        public IEnumerable<ReportHeaderInterval> GetReportHeaderIntervals(CompEntities entities, int reportHeaderId)
        {
            return (from rhi in entities.ReportHeaderInterval
                    where rhi.ReportHeader.ReportHeaderId == reportHeaderId
                    select rhi).ToList();
        }

        public IEnumerable<ReportHeaderInterval> GetReportHeaderIntervalsForCompany(int actorCompanyId, bool onlyGrossProfit = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.ReportHeaderInterval.NoTracking();
            return GetReportHeaderIntervalsForCompany(entities, actorCompanyId, onlyGrossProfit);
        }

        public IEnumerable<ReportHeaderInterval> GetReportHeaderIntervalsForCompany(CompEntities entities, int actorCompanyId, bool onlyGrossProfit = false)
        {
            if (onlyGrossProfit)
            {
                return (from rhi in entities.ReportHeaderInterval
                        .Include("ReportHeader.Company")
                        where rhi.ReportHeader.Company.ActorCompanyId == actorCompanyId &&
                        rhi.SelectValue != null
                        select rhi).ToList();
            }
            else
            {
                return (from rhi in entities.ReportHeaderInterval
                        .Include("ReportHeader.Company")
                        where rhi.ReportHeader.Company.ActorCompanyId == actorCompanyId
                        select rhi).ToList();
            }
        }

        public List<ReportHeaderInterval> GetReportHeaderIntervals(CompEntities entities, int reportGroupId, int actorCompanyId)
        {
            var nestedReportHeaderIntervals = (from r in entities.ReportGroupHeaderMapping
                                               where ((r.ReportGroup.ReportGroupId == reportGroupId) &&
                                               (r.ReportHeader.Company.ActorCompanyId == actorCompanyId))
                                               select r.ReportHeader.ReportHeaderInterval).ToList();

            List<ReportHeaderInterval> reportHeaderIntervals = new List<ReportHeaderInterval>();
            foreach (var nestedReportHeaderInterval in nestedReportHeaderIntervals)
            {
                foreach (var reportHeaderInterval in nestedReportHeaderInterval)
                {
                    reportHeaderIntervals.Add(reportHeaderInterval);
                }
            }
            return reportHeaderIntervals;
        }

        public ActionResult UpdateReportHeaderInterval(int reportHeaderId, int actorCompanyId, Collection<FormIntervalEntryItem> formIntervalEntryItems)
        {
            // Default result is successful
            ActionResult result = new ActionResult();

            using (CompEntities entities = new CompEntities())
            {
                var reportHeaderIntervals = GetReportHeaderIntervals(entities, reportHeaderId);
                foreach (ReportHeaderInterval reportHeaderInterval in reportHeaderIntervals)
                {
                    entities.DeleteObject(reportHeaderInterval);
                }
                entities.SaveChanges();

                ReportHeader reportHeader = GetReportHeader(entities, reportHeaderId, actorCompanyId, false);
                if (reportHeader == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "ReportHeader");

                foreach (FormIntervalEntryItem formIntervalEntryItem in formIntervalEntryItems)
                {
                    ReportHeaderInterval reportHeaderInterval = new ReportHeaderInterval
                    {
                        ReportHeader = reportHeader,
                        IntervalFrom = formIntervalEntryItem.From,
                        IntervalTo = formIntervalEntryItem.To,
                    };

                    if (formIntervalEntryItem.LabelType != 0)
                        reportHeaderInterval.SelectValue = formIntervalEntryItem.LabelType;

                    result = AddEntityItem(entities, reportHeaderInterval, "ReportHeaderInterval");
                    if (!result.Success)
                        return result;
                }
            }
            return result;
        }

        #endregion

        #region SysReportTemplate

        public SysReportTemplate GetFirstSysReportTemplate(SoeReportTemplateType sysReportTemplateType)
        {
            using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
            return sysEntitiesReadOnly.SysReportTemplate.Where(i => i.SysReportTemplateTypeId == (int)sysReportTemplateType).FirstOrDefault();
        }

        public List<SysReportTemplateView> GetAllSysReportTemplates()
        {
            using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
            List<SysReportTemplateView> sysReportTempalates = sysEntitiesReadOnly.SysReportTemplateView.ToList();
            return sysReportTempalates.OrderBy(i => i.GroupName).ThenBy(i => i.Name).ToList();
        }

        public List<(SysReportTemplateView Template, List<SysLinkTable> CountryLinks)> GetSysReportTemplatesForModule(int module, int actorCompanyId, bool filterTemplatesOnCountry, int? sysCountryId = null)
        {
            var validSysReportTemplateTypes = new List<(SysReportTemplateView Template, List<SysLinkTable> CountryLinks)>();

            if (!sysCountryId.HasValue)
                sysCountryId = CountryCurrencyManager.GetSysCountryIdFromCompany(actorCompanyId, defaultSysCountryId: (int)TermGroup_Country.SE);

            using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
            List<SysReportTemplateView> sysReportTemplates = (from srtv in sysEntitiesReadOnly.SysReportTemplateView
                                                              where srtv.Module == module
                                                              select srtv).ToList();

            if (!sysReportTemplates.IsNullOrEmpty())
            {
                List<SysReportTemplateType> sysReportTemplateTypes = GetSysReportTemplateTypesFromCache(module);
                List<GenericType> sysReportTemplateGroups = GetTermGroupContent(TermGroup.SysReportTemplateTypeGroup, skipUnknown: true);
                List<SysLinkTable> allSysLinks = (from c in sysEntitiesReadOnly.SysLinkTable
                                                  where c.SysLinkTableRecordType == (int)SysLinkTableRecordType.LinkSysReportTemplateToCountryId &&
                                                  c.SysLinkTableIntegerValueType == (int)SysLinkTableIntegerValueType.SysCountryId
                                                  select c).ToList();

                foreach (SysReportTemplateView sysReportTemplate in sysReportTemplates)
                {
                    // Check if report is valid for current country
                    List<SysLinkTable> sysLinks = allSysLinks.Where(s => s.SysLinkTableKeyItemId == sysReportTemplate.SysReportTemplateId).ToList();
                    if (filterTemplatesOnCountry && sysLinks.Any() && (!sysCountryId.HasValue || !sysLinks.Any(i => i.SysLinkTableIntegerValue == sysCountryId.Value)))
                        continue;

                    SysReportTemplateType sysReportTemplateType = sysReportTemplateTypes.FirstOrDefault(i => i.SysReportTemplateTypeId == sysReportTemplate.SysTemplateTypeId);
                    if (sysReportTemplateType == null)
                        continue;

                    sysReportTemplate.SysReportTemplateTypeName = GetText(sysReportTemplateType.SysReportTermId, (int)TermGroup.SysReportTemplateType);
                    sysReportTemplate.GroupName = sysReportTemplateTypes.GetGroupName(sysReportTemplateGroups, sysReportTemplateType.SysReportTemplateTypeId);
                    if (!sysReportTemplate.GroupName.IsNullOrEmpty() && sysReportTemplate.Name.StartsWith(sysReportTemplate.GroupName + " - ") && sysReportTemplate.Name.Length > sysReportTemplate.GroupName.Length + 3)
                        sysReportTemplate.Name = sysReportTemplate.Name.Right(sysReportTemplate.Name.Length - (sysReportTemplate.GroupName.Length + 3)).Trim();

                    validSysReportTemplateTypes.Add((sysReportTemplate, sysLinks));
                }
            }

            return validSysReportTemplateTypes.OrderBy(i => i.Template.GroupName).ThenBy(i => i.Template.Name).ToList();
        }

        public Dictionary<int, string> GetSysReportTemplatesForModuleDict(int module, bool addEmptyRow, int actorCompanyId)
        {
            Dictionary<int, string> dict = new Dictionary<int, string>();

            if (addEmptyRow)
                dict.Add(0, " ");

            var sysReportTemplates = GetSysReportTemplatesForModule(module, actorCompanyId, true);
            foreach (var sysReportTemplate in sysReportTemplates)
            {
                if (!dict.ContainsKey(sysReportTemplate.Template.SysReportTemplateId))
                    dict.Add(sysReportTemplate.Template.SysReportTemplateId, sysReportTemplate.Template.Name);
            }

            return dict;
        }

        public SysReportTemplate GetSysReportTemplate(int sysReportTemplateId, bool loadCountries = true, bool loadSysReportType = false, bool loadSysReportTemplateType = false, bool useCache = false)
        {
            SysReportTemplate template;
            string key = $"GetSysReportTemplate#{sysReportTemplateId}#{loadCountries}#{loadSysReportType}#{loadSysReportTemplateType}";
            if (useCache)
            {
                template = BusinessMemoryCache<SysReportTemplate>.Get(key);
                if (template != null)
                    return template;
            }
            using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
            sysEntitiesReadOnly.Database.CommandTimeout = 240;
            template = GetSysReportTemplate(sysEntitiesReadOnly, sysReportTemplateId, loadCountries, loadSysReportType, loadSysReportTemplateType);
            if (template != null && useCache)
                BusinessMemoryCache<SysReportTemplate>.Set(key, template, 120);

            return template;
        }

        public SysReportTemplate GetSysReportTemplate(SOESysEntities entities, int sysReportTemplateId, bool loadCountries = true, bool loadSysReportType = false, bool loadSysReportTemplateType = false)
        {
            IQueryable<SysReportTemplate> query = entities.Set<SysReportTemplate>()
                                                    .Include("SysReportTemplateSettings");

            if (loadSysReportTemplateType)
                query = query.Include("SysReportTemplateType");
            if (loadSysReportType)
                query = query.Include("SysReportType");

            SysReportTemplate sysReportTemplate = (from srt in query
                                                   where srt.SysReportTemplateId == sysReportTemplateId
                                                   select srt).FirstOrDefault();

            if (sysReportTemplate != null && loadCountries)
            {
                var sysLinks = GetSysReportTemplateCountries(sysReportTemplate.SysReportTemplateId);
                if (!sysLinks.IsNullOrEmpty())
                    sysReportTemplate.SysCountryIds = sysLinks.Select(i => i.SysLinkTableIntegerValue).ToList();
            }

            return sysReportTemplate;
        }

        public int? GetSysReportTemplateTypeId(int sysReportTemplateId)
        {
            string key = $"GetSysReportTemplateTypeId#{sysReportTemplateId}";
            var value = BusinessMemoryCache<int?>.Get(key);

            if (value.HasValue)
            {
                if (value == 0)
                    return null;
                return value;
            }

            using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
            value = GetSysReportTemplateTypeId(sysEntitiesReadOnly, sysReportTemplateId);

            if (!value.HasValue)
                value = 0;

            BusinessMemoryCache<int?>.Set(key, value, 60 * 5);

            return value;
        }

        public int? GetSysReportTemplateTypeId(SOESysEntities entities, int sysReportTemplateId)
        {
            return (from t in entities.SysReportTemplate
                    where t.SysReportTemplateId == sysReportTemplateId
                    select t.SysReportTemplateTypeId)?.FirstOrDefault();
        }

        public int GetModuleForReportTemplate(int sysReportTemplateId)
        {
            using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
            return (from srtv in sysEntitiesReadOnly.SysReportTemplateView
                    where srtv.SysReportTemplateId == sysReportTemplateId
                    select srtv.Module).FirstOrDefault();
        }

        public ActionResult SaveSysReportTemplate(ReportTemplateDTO reportTemplateInput, byte[] templateData)
        {
            ActionResult result = new ActionResult(true);

            if (reportTemplateInput == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "ReportTemplate");

            int reportTemplateId = reportTemplateInput.ReportTemplateId;

            #region Prereq

            if (!reportTemplateInput.SysReportTemplateTypeId.HasValue || reportTemplateInput.SysReportTemplateTypeId.Value == 0)
                return new ActionResult((int)ActionResultSave.EntityIsNull, GetText(11771, "Typ måste anges"));
            if (!reportTemplateInput.SysReportTypeId.HasValue || reportTemplateInput.SysReportTypeId.Value == 0)
                reportTemplateInput.SysReportTypeId = ReportManager.GetSysReportTypeBySysReportTemplate(reportTemplateId)?.SysReportTypeId ?? (int?)SoeReportType.CrystalReport;

            if (reportTemplateInput.SysReportTemplateTypeId < 500 && templateData != null && !ReportGenManager.ValidateReportTemplate(reportTemplateInput.SysReportTemplateTypeId.Value, templateData, (SoeReportType)reportTemplateInput.SysReportTypeId))
                return new ActionResult((int)ActionResultSave.NothingSaved, GetText(1339, "Felaktig fil, kunde inte valideras"));
            else if (reportTemplateInput.SysReportTemplateTypeId >= 500)
                templateData = Encoding.UTF8.GetBytes("0x3078");

            #endregion

            using (SOESysEntities entities = new SOESysEntities())
            {
                try
                {
                    entities.Connection.Open();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        #region Prereq

                        #endregion

                        #region SysReportTemplate

                        SysReportTemplate sysReportTemplate = GetSysReportTemplate(entities, reportTemplateInput.ReportTemplateId, loadCountries: false);
                        if (sysReportTemplate == null)
                        {
                            #region Add

                            if (templateData == null)
                            {
                                List<int> exportTypesWithoutTemplate = new List<int>();
                                exportTypesWithoutTemplate.Add((int)TermGroup_ReportExportType.MatrixGrid);
                                exportTypesWithoutTemplate.Add((int)TermGroup_ReportExportType.MatrixExcel);

                                if (reportTemplateInput.ValidExportTypes.Except(exportTypesWithoutTemplate).Any())
                                    return new ActionResult((int)ActionResultSave.EntityIsNull, GetText(11772, "Mall måste laddas upp"));
                                else
                                    templateData = new byte[0];
                            }

                            sysReportTemplate = new SysReportTemplate()
                            {
                                SysReportTypeId = reportTemplateInput.SysReportTemplateTypeId >= 1000 ? (int)SoeReportType.Analysis : (int)SoeReportType.CrystalReport,
                            };
                            SetCreatedPropertiesOnEntity(sysReportTemplate);
                            entities.SysReportTemplate.Add(sysReportTemplate);

                            #endregion
                        }
                        else
                        {
                            SetModifiedPropertiesOnEntity(sysReportTemplate);
                        }

                        sysReportTemplate.SysReportTemplateTypeId = reportTemplateInput.SysReportTemplateTypeId.Value;
                        sysReportTemplate.ReportNr = reportTemplateInput.ReportNr;
                        sysReportTemplate.Name = reportTemplateInput.Name;
                        sysReportTemplate.Description = reportTemplateInput.Description;
                        sysReportTemplate.FileName = StringUtility.NullToEmpty(reportTemplateInput.FileName);
                        sysReportTemplate.GroupByLevel1 = reportTemplateInput.GroupByLevel1;
                        sysReportTemplate.GroupByLevel2 = reportTemplateInput.GroupByLevel2;
                        sysReportTemplate.GroupByLevel3 = reportTemplateInput.GroupByLevel3;
                        sysReportTemplate.GroupByLevel4 = reportTemplateInput.GroupByLevel4;
                        sysReportTemplate.SortByLevel1 = reportTemplateInput.SortByLevel1;
                        sysReportTemplate.SortByLevel2 = reportTemplateInput.SortByLevel2;
                        sysReportTemplate.SortByLevel3 = reportTemplateInput.SortByLevel3;
                        sysReportTemplate.SortByLevel4 = reportTemplateInput.SortByLevel4;
                        sysReportTemplate.Special = reportTemplateInput.Special;
                        sysReportTemplate.IsSortAscending = reportTemplateInput.IsSortAscending;
                        sysReportTemplate.ShowGroupingAndSorting = reportTemplateInput.ShowGroupingAndSorting;
                        sysReportTemplate.ShowOnlyTotals = reportTemplateInput.ShowOnlyTotals;
                        sysReportTemplate.ValidExportTypes = reportTemplateInput.ValidExportTypes.ToCommaSeparated();
                        sysReportTemplate.IsSystemReport = reportTemplateInput.IsSystemReport;

                        if (templateData != null)
                            sysReportTemplate.Template = templateData;

                        #region Countries

                        if (reportTemplateInput.SysCountryIds != null)
                        {
                            var sysLinks = GetSysReportTemplateCountries(entities, sysReportTemplate.SysReportTemplateId);

                            #region Delete

                            foreach (var sysLink in sysLinks)
                            {
                                if (!reportTemplateInput.SysCountryIds.Any(sysCountryId => sysCountryId == sysLink.SysLinkTableIntegerValue))
                                {
                                    result = DeleteSysLink(entities, SysLinkTableRecordType.LinkSysReportTemplateToCountryId, SysLinkTableIntegerValueType.SysCountryId, sysReportTemplate.SysReportTemplateId, sysLink.SysLinkTableIntegerValue);
                                    if (!result.Success)
                                        return result;
                                }
                            }

                            #endregion

                            #region Add

                            foreach (int sysCountryId in reportTemplateInput.SysCountryIds)
                            {
                                if (!sysLinks.Any(i => i.SysLinkTableIntegerValue == sysCountryId))
                                {
                                    var sysLinkTable = new SysLinkTable()
                                    {
                                        SysLinkTableRecordType = (int)SysLinkTableRecordType.LinkSysReportTemplateToCountryId,
                                        SysLinkTableIntegerValueType = (int)SysLinkTableIntegerValueType.SysCountryId,
                                        SysLinkTableKeyItemId = sysReportTemplate.SysReportTemplateId,
                                        SysLinkTableIntegerValue = sysCountryId,
                                    };
                                    entities.SysLinkTable.Add(sysLinkTable);
                                }
                            }

                            #endregion
                        }

                        #endregion

                        #region Feild Settings

                        if (reportTemplateInput.ReportTemplateSettings != null)
                        {
                            //Add new settings
                            foreach (var setting in reportTemplateInput.ReportTemplateSettings.Where(x => x.IsModified && x.ReportTemplateSettingId == 0))
                            {
                                SysReportTemplateSetting sysReportTemplateSetting = new SysReportTemplateSetting()
                                {
                                    SysReportTemplateId = sysReportTemplate.SysReportTemplateId,
                                    SettingField = setting.SettingField,
                                    SettingType = setting.SettingType,
                                    SettingValue = setting.SettingValue,
                                    State = (int)SoeEntityState.Active
                                };
                                SetCreatedPropertiesOnEntity(sysReportTemplateSetting);
                                entities.SysReportTemplateSetting.Add(sysReportTemplateSetting);
                            }

                            //Update existing settings if modified
                            foreach (var setting in reportTemplateInput.ReportTemplateSettings.Where(x => x.IsModified && x.ReportTemplateSettingId > 0))
                            {
                                SysReportTemplateSetting sysReportTemplateSetting = entities.SysReportTemplateSetting.FirstOrDefault(x => x.SysReportTemplateSettingId == setting.ReportTemplateSettingId);
                                if (sysReportTemplateSetting != null)
                                {
                                    sysReportTemplateSetting.SettingValue = setting.SettingValue;
                                    sysReportTemplateSetting.SettingField = setting.SettingField;
                                    sysReportTemplateSetting.SettingType = setting.SettingType;
                                    SetModifiedPropertiesOnEntity(sysReportTemplateSetting);
                                }
                            }
                        }

                        #endregion

                        #endregion

                        result = SaveChanges(entities, transaction);
                        if (result.Success)
                        {
                            //Commit transaction
                            transaction.Complete();

                            reportTemplateId = sysReportTemplate.SysReportTemplateId;
                            BusinessMemoryCache<int?>.Set($"GetSysReportTemplateTypeId#{reportTemplateId}", reportTemplateInput.SysReportTemplateTypeId);
                        }
                    }
                }
                catch (Exception ex)
                {
                    base.LogError(ex, this.log);
                    result.Exception = ex;
                    result.IntegerValue = 0;
                }
                finally
                {
                    if (result.Success)
                    {
                        //Set success properties
                        result.IntegerValue = reportTemplateId;
                    }
                    else
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }
            }

            return result;
        }
        #endregion
        #region SysReportTemplateType

        /// <summary>
        /// Get all SysReportTemplateType's
        /// Accessor for SysDbCache
        /// </summary>
        /// <returns></returns>
        public List<SysReportTemplateType> GetSysReportTemplateTypes()
        {
            using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
            return sysEntitiesReadOnly.SysReportTemplateType.ToList();
        }

        public List<SysReportTemplateType> GetSysReportTemplateTypesFromCache(int? module = null)
        {
            return SysDbCache.Instance.SysReportTemplateTypes.Where(t => (!module.HasValue || module.Value == t.Module)).ToList();
        }

        public SysReportTemplateType GetSysReportTemplateTypeFromCache(int sysReportTemplateTypeId)
        {
            return SysDbCache.Instance.SysReportTemplateTypes.FirstOrDefault(t => t.SysReportTemplateTypeId == sysReportTemplateTypeId);
        }

        public Dictionary<int, string> GetSysReportTemplateTypesForModuleDict(int module, bool addEmptyRow)
        {
            Dictionary<int, string> dict = new Dictionary<int, string>();

            if (addEmptyRow)
                dict.Add(0, "");

            List<GenericType> sysReportTemplateGroups = GetTermGroupContent(TermGroup.SysReportTemplateTypeGroup, skipUnknown: true);
            List<SysReportTemplateType> sysReportTemplateTypes = GetSysReportTemplateTypesFromCache(module);
            foreach (SysReportTemplateType sysReportTemplateType in sysReportTemplateTypes)
            {
                if (dict.ContainsKey(sysReportTemplateType.SysReportTemplateTypeId))
                    continue;

                string groupName = sysReportTemplateTypes.GetGroupName(sysReportTemplateGroups, sysReportTemplateType.SysReportTemplateTypeId);
                string typeName = GetText(sysReportTemplateType.SysReportTermId, (int)TermGroup.SysReportTemplateType);
                string name = groupName;
                if (!string.IsNullOrEmpty(name))
                    name += " - ";
                name += typeName;
                if (!string.IsNullOrEmpty(name))
                    dict.Add(sysReportTemplateType.SysReportTemplateTypeId, name);
            }

            return dict;
        }

        public Dictionary<int, string> GetSysReportTemplateTypesForModuleDict(int module, Dictionary<int, string> terms, bool addEmptyRow)
        {
            Dictionary<int, string> dict = new Dictionary<int, string>();
            if (addEmptyRow)
                dict.Add(0, " ");

            //Get all SysReportTemplateType for given module
            var sysReportTemplateTypes = GetSysReportTemplateTypesFromCache(module);

            //Validate the dict with terms against the SysReportTemplateType for the given module 
            foreach (var sysReportTemplateType in sysReportTemplateTypes)
            {
                //Only include items with term > 0
                if (sysReportTemplateType.SysReportTermId < 0)
                    continue;

                //Add values from tempDict to dict
                if (!dict.ContainsKey(sysReportTemplateType.SysReportTemplateTypeId) && terms.ContainsKey(sysReportTemplateType.SysReportTermId))
                    dict.Add(sysReportTemplateType.SysReportTemplateTypeId, terms[sysReportTemplateType.SysReportTermId]);
            }

            return dict.Sort();
        }

        public SysReportTemplateType GetSysReportTemplateType(int sysReportTemplateTypeId)
        {
            using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
            return GetSysReportTemplateType(sysEntitiesReadOnly, sysReportTemplateTypeId);
        }

        public SysReportTemplateType GetSysReportTemplateType(SOESysEntities entities, int sysReportTemplateTypeId)
        {
            return (from srtt in entities.SysReportTemplateType
                    where srtt.SysReportTemplateTypeId == sysReportTemplateTypeId
                    select srtt).FirstOrDefault();
        }

        public SysReportType GetSysReportTypeBySysReportTemplate(int sysReportTemplateId)
        {
            using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
            return GetSysReportTypeBySysReportTemplate(sysEntitiesReadOnly, sysReportTemplateId);
        }

        public SysReportType GetSysReportTypeBySysReportTemplate(SOESysEntities entities, int sysReportTemplateId)
        {
            return (from srtt in entities.SysReportTemplate
                    where srtt.SysReportTemplateId == sysReportTemplateId
                    select srtt).Select(x => x.SysReportType).FirstOrDefault();
        }

        public SoeReportTemplateType GetSoeReportTemplateType(Report report, int actorCompanyId)
        {
            if (report == null)
                return SoeReportTemplateType.Unknown;

            SysReportTemplateType templateType = GetSysReportTemplateType(report, actorCompanyId);

            return (SoeReportTemplateType)templateType.SysReportTemplateTypeId;
        }

        public SysReportTemplateType GetSysReportTemplateType(Report report, int actorCompanyId)
        {
            if (report == null)
                return null;

            SysReportTemplateType templateType;

            if (report.Standard)
            {
                if (this.sysReportTemplateTypesStandard.ContainsKey(report.ReportTemplateId))
                {
                    templateType = this.sysReportTemplateTypesStandard[report.ReportTemplateId];
                }
                else
                {
                    int? sysReportTemplateTypeId = GetSysReportTemplateTypeId(report.ReportTemplateId);
                    templateType = sysReportTemplateTypeId.HasValue ? GetSysReportTemplateTypeFromCache(sysReportTemplateTypeId.Value) : null;
                    if (templateType != null)
                        this.sysReportTemplateTypesStandard.Add(report.ReportTemplateId, templateType);
                }
            }
            else
            {
                if (this.sysReportTemplateTypesCustom.ContainsKey(report.ReportTemplateId))
                {
                    templateType = this.sysReportTemplateTypesCustom[report.ReportTemplateId];
                }
                else
                {
                    int? sysReportTemplateTypeId = GetReportTemplateTypeId(report.ReportTemplateId, actorCompanyId);
                    templateType = sysReportTemplateTypeId.HasValue ? GetSysReportTemplateTypeFromCache(sysReportTemplateTypeId.Value) : null;
                    if (templateType != null)
                        this.sysReportTemplateTypesCustom.Add(report.ReportTemplateId, templateType);
                }
            }

            return templateType;
        }

        #endregion

        #region SysReportType

        /// <summary>
        /// Get all SysReportTemplateType's
        /// Accessor for SysReportType
        /// </summary>
        /// <returns></returns>
        public List<SysReportType> GetSysReportTypes()
        {
            using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
            return sysEntitiesReadOnly.SysReportType
                            .ToList<SysReportType>();
        }

        public SysReportType GetSysReportType(int sysReportTypeId)
        {
            using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
            return GetSysReportType(sysEntitiesReadOnly, sysReportTypeId);
        }

        public SysReportType GetSysReportType(SOESysEntities entities, int sysReportTypeId)
        {
            return (from srt in entities.SysReportType
                    where srt.SysReportTypeId == sysReportTypeId
                    select srt).FirstOrDefault<SysReportType>();
        }

        #endregion

        #region SysReportGroup
        public ActionResult SaveSysReportGroup(ReportGroupDTO dto)
        {
            var result = new ActionResult();
            if (dto == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "SysReportGroup");

            using (SOESysEntities entities = new SOESysEntities())
            {
                try
                {
                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        if (entities.Connection.State != ConnectionState.Open)
                            entities.Connection.Open();

                        SysReportGroup group = null;
                        if (dto.ReportGroupId > 0)
                            group = GetSysReportGroup(entities, dto.ReportGroupId, false, false);

                        if (group == null)
                        {
                            //Add new
                            group = new SysReportGroup();
                            SetCreatedPropertiesOnEntity(group);
                            entities.SysReportGroup.Add(group);
                        }
                        else
                            SetModifiedPropertiesOnEntity(group);


                        group.Name = dto.Name;
                        group.Description = dto.Description;
                        group.TemplateTypeId = dto.TemplateTypeId;
                        group.ShowLabel = dto.ShowLabel;
                        group.ShowSum = dto.ShowSum;
                        group.InvertRow = dto.InvertRow;
                        group.State = (int)dto.State;

                        result = SaveChanges(entities, transaction);

                        if (result.Success)
                        {

                            transaction.Complete();
                            result.IntegerValue = group.SysReportGroupId;
                        }
                    }
                }
                catch (Exception ex)
                {
                    result.Exception = ex;
                    base.LogError(ex, this.log);
                }
                finally
                {
                    if (!result.Success)
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }
            }
            return result;
        }
        public SysReportGroupMapping GetSysReportGroupMapping(int sysReportTemplateId, int sysReportGroupId)
        {
            using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
            return sysEntitiesReadOnly.SysReportGroupMapping.FirstOrDefault(m => m.SysReportTemplateId == sysReportTemplateId && m.SysReportGroupId == sysReportGroupId);
        }
        public List<SysReportGroupMapping> GetSysReportGroupMappings(int sysReportTemplateId)
        {
            using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
            return sysEntitiesReadOnly.SysReportGroupMapping
                .Include("SysReportGroup")
                .Where(m => m.SysReportTemplateId == sysReportTemplateId)
                .OrderBy(m => m.Order)
                .ToList();
        }
        public ActionResult AddSysReportGroupHeaderMapping(SysReportGroupHeaderMapping reportGroupHeaderMapping, int sysReportGroupId, int sysReportHeaderId)
        {
            var result = new ActionResult();

            if (reportGroupHeaderMapping == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "ReportGroupHeaderMapping");

            using (SOESysEntities entities = new SOESysEntities())
            {
                try
                {
                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        if (entities.Connection.State != ConnectionState.Open)
                            entities.Connection.Open();

                        reportGroupHeaderMapping.SysReportHeader = GetSysReportHeader(entities, sysReportHeaderId, false);
                        if (reportGroupHeaderMapping.SysReportHeader == null)
                            return new ActionResult((int)ActionResultSave.EntityNotFound, "ReportHeader");

                        reportGroupHeaderMapping.SysReportGroup = GetSysReportGroup(entities, sysReportGroupId, false, false);
                        if (reportGroupHeaderMapping.SysReportGroup == null)
                            return new ActionResult((int)ActionResultSave.EntityNotFound, "ReportGroup");

                        entities.SysReportGroupHeaderMapping.Add(reportGroupHeaderMapping);

                        result = SaveChanges(entities, transaction);

                        if (result.Success)
                        {

                            transaction.Complete();
                        }
                    }
                }
                catch (Exception ex)
                {
                    result.Exception = ex;
                    base.LogError(ex, this.log);
                }
                finally
                {
                    if (!result.Success)
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }
            }
            return result;
        }

        public ActionResult AddSysReportGroupMapping(SysReportGroupMapping reportGroupMapping, int sysReportTemplateId, int sysReportGroupId)
        {
            var result = new ActionResult();

            if (reportGroupMapping == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "ReportGroupMapping");

            using (SOESysEntities entities = new SOESysEntities())
            {
                try
                {
                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        if (entities.Connection.State != ConnectionState.Open)
                            entities.Connection.Open();

                        reportGroupMapping.SysReportTemplate = GetSysReportTemplate(entities, sysReportTemplateId, false, false, false);
                        if (reportGroupMapping.SysReportTemplate == null)
                            return new ActionResult((int)ActionResultSave.EntityNotFound, "ReportTemplate");

                        reportGroupMapping.SysReportGroup = GetSysReportGroup(entities, sysReportGroupId, false, false);
                        if (reportGroupMapping.SysReportGroup == null)
                            return new ActionResult((int)ActionResultSave.EntityNotFound, "ReportGroup");

                        entities.SysReportGroupMapping.Add(reportGroupMapping);

                        result = SaveChanges(entities, transaction);

                        if (result.Success)
                        {

                            transaction.Complete();
                        }
                    }
                }
                catch (Exception ex)
                {
                    result.Exception = ex;
                    base.LogError(ex, this.log);
                }
                finally
                {
                    if (!result.Success)
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }
            }
            return result;
        }
        public ActionResult SaveSysReportHeader(ReportHeaderDTO dto)
        {
            var result = new ActionResult();
            if (dto == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "ReportHeader");

            using (SOESysEntities entities = new SOESysEntities())
            {
                try
                {
                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        if (entities.Connection.State != ConnectionState.Open)
                            entities.Connection.Open();


                        SysReportHeader header = null;
                        if (dto.ReportHeaderId > 0)
                        {
                            header = GetSysReportHeader(entities, dto.ReportHeaderId, true);
                        }

                        #region Header
                        if (header == null)
                        {
                            //Add new
                            header = new SysReportHeader();
                            header.SysReportHeaderInterval = new List<SysReportHeaderInterval>();
                            SetCreatedPropertiesOnEntity(header);
                            entities.SysReportHeader.Add(header);
                        }
                        else
                            SetModifiedPropertiesOnEntity(header);

                        header.Name = dto.Name;
                        header.Description = dto.Description;
                        header.TemplateTypeId = dto.TemplateTypeId;
                        header.ShowLabel = dto.ShowLabel;
                        header.ShowSum = dto.ShowSum;
                        header.ShowRow = dto.ShowRow;
                        header.ShowZeroRow = dto.ShowZeroRow;
                        header.DoNotSummarizeOnGroup = dto.DoNotSummarizeOnGroup;
                        header.State = (int)dto.State;

                        #endregion

                        #region Intervals
                        if (dto.ReportHeaderIntervals != null)
                        {
                            header.SysReportHeaderInterval
                                .ToList()
                                .ForEach(i => entities.SysReportHeaderInterval.Remove(i));

                            dto.ReportHeaderIntervals
                                .ForEach(i => entities.SysReportHeaderInterval.Add(i.FromDTOToSys(header)));
                        }
                        #endregion

                        result = SaveChanges(entities, transaction);

                        if (result.Success)
                        {
                            result.IntegerValue = header.SysReportHeaderId;
                            transaction.Complete();
                        }
                    }
                }
                catch (Exception ex)
                {
                    result.Exception = ex;
                    base.LogError(ex, this.log);
                }
                finally
                {
                    if (!result.Success)
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }
            }
            return result;
        }
        public ActionResult DeleteSysReportGroupHeaderMapping(int sysReportGroupId, int sysReportHeaderId)
        {
            var result = new ActionResult();

            if (sysReportGroupId == 0 || sysReportHeaderId == 0)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "ReportGroupHeaderMapping");

            using (SOESysEntities entities = new SOESysEntities())
            {
                try
                {
                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        if (entities.Connection.State != ConnectionState.Open)
                            entities.Connection.Open();

                        var mapping = entities.SysReportGroupHeaderMapping
                            .FirstOrDefault(m => m.SysReportGroupId == sysReportGroupId && m.SysReportHeaderId == sysReportHeaderId);

                        if (mapping != null)
                        {
                            entities.SysReportGroupHeaderMapping.Remove(mapping);
                            result = SaveChanges(entities, transaction);
                        }
                        else
                        {
                            result = new ActionResult((int)ActionResultSave.EntityIsNull, "ReportGroupHeaderMapping");
                        }


                        if (result.Success)
                        {
                            transaction.Complete();
                        }
                    }
                }
                catch (Exception ex)
                {
                    result.Exception = ex;
                    base.LogError(ex, this.log);
                }
                finally
                {
                    if (!result.Success)
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }
            }
            return result;
        }

        public ActionResult DeleteSysReportGroupMapping(int sysReportTemplateId, int sysReportGroupId)
        {
            var result = new ActionResult();

            if (sysReportGroupId == 0 || sysReportTemplateId == 0)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "ReportGroupMapping");

            using (SOESysEntities entities = new SOESysEntities())
            {
                try
                {
                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        if (entities.Connection.State != ConnectionState.Open)
                            entities.Connection.Open();

                        var mapping = entities.SysReportGroupMapping
                            .FirstOrDefault(m => m.SysReportGroupId == sysReportGroupId && m.SysReportTemplateId == sysReportTemplateId);

                        if (mapping != null)
                        {
                            entities.SysReportGroupMapping.Remove(mapping);
                            result = SaveChanges(entities, transaction);
                        }
                        else
                        {
                            result = new ActionResult((int)ActionResultSave.EntityIsNull, "ReportGroupMapping");
                        }

                        if (result.Success)
                        {
                            transaction.Complete();
                        }
                    }
                }
                catch (Exception ex)
                {
                    result.Exception = ex;
                    base.LogError(ex, this.log);
                }
                finally
                {
                    if (!result.Success)
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }
            }
            return result;
        }
        public ActionResult DeleteSysReportGroupHeaderMappings(int sysReportGroupId)
        {
            var result = new ActionResult();

            if (sysReportGroupId == 0)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "SysReportGroup");

            using (SOESysEntities entities = new SOESysEntities())
            {
                try
                {
                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        if (entities.Connection.State != ConnectionState.Open)
                            entities.Connection.Open();

                        var mappings = entities.SysReportGroupHeaderMapping
                            .Where(m => m.SysReportGroupId == sysReportGroupId)
                            .ToList();

                        if (!mappings.IsNullOrEmpty())
                        {
                            mappings.ForEach(m => entities.SysReportGroupHeaderMapping.Remove(m));
                            result = SaveChanges(entities);
                            if (result.Success)
                            {
                                transaction.Complete();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    result.Exception = ex;
                    base.LogError(ex, this.log);
                }
                finally
                {
                    if (!result.Success)
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }
            }
            return result;
        }
        public ActionResult DeleteSysReportGroup(int sysReportGroupId)
        {
            var result = new ActionResult();

            if (sysReportGroupId == 0)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "SysReportGroup");

            using (SOESysEntities entities = new SOESysEntities())
            {
                try
                {
                    if (entities.Connection.State != ConnectionState.Open)
                        entities.Connection.Open();

                    var group = GetSysReportGroup(entities, sysReportGroupId, true, false);

                    if (group != null && group.SysReportGroupMapping.IsNullOrEmpty())
                    {
                        group.State = (int)SoeEntityState.Deleted;
                        SetModifiedPropertiesOnEntity(group);
                        result = SaveChanges(entities);
                    }
                    else
                    {
                        result = new ActionResult((int)ActionResultDelete.ReportGroupInUse);
                    }

                }
                catch (Exception ex)
                {
                    result.Exception = ex;
                    base.LogError(ex, this.log);
                }
                finally
                {
                    entities.Connection.Close();
                }
            }
            return result;
        }

        public ActionResult DeleteSysReportHeader(int sysReportHeaderId)
        {
            var result = new ActionResult();

            if (sysReportHeaderId == 0)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "SysReportHeader");

            using (SOESysEntities entities = new SOESysEntities())
            {
                try
                {
                    if (entities.Connection.State != ConnectionState.Open)
                        entities.Connection.Open();

                    var header = GetSysReportHeader(entities, sysReportHeaderId, false);
                    header.State = (int)SoeEntityState.Deleted;

                    SetModifiedPropertiesOnEntity(header);
                    result = SaveChanges(entities);
                }
                catch (Exception ex)
                {
                    result.Exception = ex;
                    base.LogError(ex, this.log);
                }
                finally
                {
                    entities.Connection.Close();
                }
            }
            return result;
        }
        public bool SysReportTemplateHasGroups(int sysReportTemplateId)
        {
            using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
            return sysEntitiesReadOnly.SysReportGroupMapping.Any(m => m.SysReportTemplateId == sysReportTemplateId);
        }
        public bool SysReportTemplateHasGroupsAndHeaders(int sysReportTemplateId)
        {
            using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
            var groupIds = sysEntitiesReadOnly.SysReportGroupMapping
                .Where(m => m.SysReportTemplateId == sysReportTemplateId)
                .Select(m => m.SysReportGroupId)
                .ToList();

            if (groupIds.IsNullOrEmpty())
                return false;

            var mappings = sysEntitiesReadOnly.SysReportGroupHeaderMapping
                .Where(m => groupIds.Contains(m.SysReportGroupId))
                .Select(m => m.SysReportGroupId)
                .ToList();

            return groupIds.Any(g => mappings.Any(m => m == g));
        }
        public List<ReportGroupDTO> GetSysReportGroups(int sysReportTemplateId)
        {
            using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
            var mappings = sysEntitiesReadOnly.SysReportGroupMapping
                .Where(m => m.SysReportTemplateId == sysReportTemplateId)
                .Include(m => m.SysReportGroup)
                .OrderBy(m => m.Order)
                .ToList();

            var groups = mappings
                .Select(m => m.SysReportGroup.ToDTO())
                .ToList();

            var groupIds = groups.Select(g => g.ReportGroupId);
            var headerMappings = sysEntitiesReadOnly.SysReportGroupHeaderMapping
                .Where(m => groupIds.Contains(m.SysReportGroupId))
                .Include("SysReportHeader.SysReportHeaderInterval")
                .OrderBy(m => m.Order)
                .ToList();

            foreach (var group in groups)
            {
                group.ReportHeaders = headerMappings
                    .Where(hm => hm.SysReportGroupId == group.ReportGroupId && hm.SysReportHeader.State == (int)SoeEntityState.Active)
                    .Select(hm => hm.SysReportHeader)
                    .ToList()
                    .ToDTOs();
            }

            return groups.Where(g => g.State == SoeEntityState.Active).ToList();
        }

        public List<SysReportGroup> GetSysReportGroups(bool loadReportGroupMapping, bool loadReportHeaderGroupMapping, int templateTypeId = -1)
        {
            using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
            var query = sysEntitiesReadOnly.SysReportGroup.Where(g => g.State == (int)SoeEntityState.Active).AsQueryable();
            
            if (templateTypeId > 0)
                query = query.Where(g => g.TemplateTypeId == templateTypeId || g.TemplateTypeId == (int)SoeReportTemplateType.Generic);

            if (loadReportGroupMapping)
                query = query.Include(e => e.SysReportGroupMapping);

            if (loadReportHeaderGroupMapping)
                query = query.Include(e => e.SysReportGroupHeaderMapping);

            var groups = query.ToList();
            var sysReportTemplateTypeTerms = base.GetTermGroupDict(TermGroup.SysReportTemplateType);

            foreach (var group in groups)
            {
                if (group.SysReportGroupHeaderMapping != null)
                    group.SysReportGroupHeaderMapping = group.SysReportGroupHeaderMapping.OrderBy(m => m.Order).ToList();

                if (group.SysReportGroupMapping != null)
                    group.SysReportGroupMapping = group.SysReportGroupMapping.OrderBy(m => m.Order).ToList();

                if (sysReportTemplateTypeTerms.ContainsKey(group.TemplateTypeId))
                    group.TemplateType = sysReportTemplateTypeTerms[group.TemplateTypeId];
            }

            return groups;
        }
        public SysReportGroup GetSysReportGroup(int reportGroupId, bool loadReportGroupMapping, bool loadReportHeaderGroupMapping)
        {
            using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
            return this.GetSysReportGroup(sysEntitiesReadOnly, reportGroupId, loadReportGroupMapping, loadReportHeaderGroupMapping);
        }
        public SysReportGroup GetSysReportGroup(SOESysEntities entities, int reportGroupId, bool loadReportGroupMapping, bool loadReportHeaderGroupMapping)
        {
            var query = entities.SysReportGroup.Where(r => r.SysReportGroupId == reportGroupId);

            if (loadReportGroupMapping)
            {
                query = query.Include(e => e.SysReportGroupMapping);
            }
            if (loadReportHeaderGroupMapping)
            {
                query = query.Include("SysReportGroupHeaderMapping.SysReportHeader");
            }

            var group = query.FirstOrDefault();
            if (group == null) return null;

            if (group.SysReportGroupHeaderMapping != null)
            {
                group.SysReportGroupHeaderMapping = group.SysReportGroupHeaderMapping.OrderBy(m => m.Order).ToList();
            }
            if (group.SysReportGroupMapping != null)
            {
                group.SysReportGroupMapping = group.SysReportGroupMapping.OrderBy(m => m.Order).ToList();
            }

            return group;
        }
        public SysReportHeader GetSysReportHeader(int reportHeaderId, bool loadReportHeaderInterval)
        {
            using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
            return this.GetSysReportHeader(sysEntitiesReadOnly, reportHeaderId, loadReportHeaderInterval);
        }
        public SysReportHeader GetSysReportHeader(SOESysEntities entities, int reportHeaderId, bool loadReportHeaderInterval)
        {
            var query = entities.SysReportHeader.Where(r => r.SysReportHeaderId == reportHeaderId);
            if (loadReportHeaderInterval)
            {
                query = query.Include("SysReportHeaderInterval");
            }

            return query.FirstOrDefault();
        }
        public List<SysReportHeader> GetSysReportHeaders(bool loadReportHeaderInterval, int templateTypeId = -1)
        {
            using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
            return this.GetSysReportHeaders(sysEntitiesReadOnly, loadReportHeaderInterval, templateTypeId);
        }

        public List<SysReportHeader> GetSysReportHeaders(SOESysEntities entities, bool loadReportHeaderInterval, int templateTypeId = -1)
        {
            var query = entities.SysReportHeader.Where(h => h.State == (int)SoeEntityState.Active).AsQueryable();

            if (templateTypeId > 0)
                query = query.Where(h => h.TemplateTypeId == templateTypeId || h.TemplateTypeId == (int)SoeReportTemplateType.Generic);

            if (loadReportHeaderInterval)
                query = query.Include("SysReportHeaderInterval");

            var reportHeaders = query.ToList();
            var sysReportTemplateTypeTerms = base.GetTermGroupDict(TermGroup.SysReportTemplateType);
            foreach (var header in reportHeaders)
            {
                if (sysReportTemplateTypeTerms.ContainsKey(header.TemplateTypeId))
                    header.TemplateType = sysReportTemplateTypeTerms[header.TemplateTypeId];
            }
            return reportHeaders;
        }
        public ActionResult DeleteSysReportHeaders(List<int> sysReportHeaderIds)
        {
            var result = new ActionResult();
            foreach (var reportHeaderId in sysReportHeaderIds)
            {
                result = DeleteSysReportHeader(reportHeaderId);
                if (!result.Success)
                    return result;
            }
            return result;
        }
        public ActionResult DeleteSysReportGroups(List<int> sysReportGroupIds)
        {
            var result = new ActionResult();
            foreach (var reportGroupId in sysReportGroupIds)
            {
                result = DeleteSysReportGroup(reportGroupId);
                if (!result.Success)
                    return result;
            }
            return result;
        }

        public bool SysReportHeaderExistsInGroup(int sysReportGroupId, int sysReportHeaderId)
        {
            using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
            return sysEntitiesReadOnly.SysReportGroupHeaderMapping.Any(m => m.SysReportGroupId == sysReportGroupId && m.SysReportHeaderId == sysReportHeaderId);
        }
        public ActionResult ReorderSysReportGroupMapping(int sysReportTemplateId, int sysReportGroupId, bool isUp)
        {
            var result = new ActionResult();
            if (sysReportGroupId == 0 || sysReportTemplateId == 0)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "SysReportGroupMapping");

            using (SOESysEntities entities = new SOESysEntities())
            {
                try
                {
                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        if (entities.Connection.State != ConnectionState.Open)
                            entities.Connection.Open();

                        var mappings = entities.SysReportGroupMapping.Where(m => m.SysReportTemplateId == sysReportTemplateId)
                            .OrderBy(m => m.Order)
                            .ToList();

                        var mapping = sysReportGroupId == -1 ?
                            mappings.LastOrDefault() :
                            mappings.FirstOrDefault(m => m.SysReportGroupId == sysReportGroupId);

                        if (mappings.Count > 1 && mapping != null)
                        {
                            int currentIndex = mappings.IndexOf(mapping);
                            int newIndex = isUp ? currentIndex - 1 : currentIndex + 1;

                            if (newIndex >= 0 && newIndex < mappings.Count)
                            {
                                SysReportGroupMapping other = mappings[newIndex];
                                int temp = mapping.Order;
                                mapping.Order = other.Order;
                                other.Order = temp;
                            }
                        }
                        result = SaveChanges(entities, transaction);

                        if (result.Success)
                        {
                            transaction.Complete();
                        }
                    }
                }
                catch (Exception ex)
                {
                    result.Exception = ex;
                    base.LogError(ex, this.log);
                }
                finally
                {
                    if (!result.Success)
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }
            }
            return result;
        }
        public ActionResult ReorderSysReportGroupHeaderMapping(int reportGroupId, int reportHeaderId, bool isUp)
        {
            var result = new ActionResult();
            if (reportGroupId == 0 || reportHeaderId == 0)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "SysReportGroup");

            using (SOESysEntities entities = new SOESysEntities())
            {
                try
                {
                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        if (entities.Connection.State != ConnectionState.Open)
                            entities.Connection.Open();

                        var mappings = entities.SysReportGroupHeaderMapping.Where(m => m.SysReportGroupId == reportGroupId)
                            .OrderBy(m => m.Order)
                            .ToList();

                        var mapping = reportHeaderId == -1 ?
                            mappings.LastOrDefault() :
                            mappings.FirstOrDefault(m => m.SysReportHeaderId == reportHeaderId);

                        if (mappings.Count > 1 && mapping != null)
                        {
                            int currentIndex = mappings.IndexOf(mapping);
                            int newIndex = isUp ? currentIndex - 1 : currentIndex + 1;

                            if (newIndex >= 0 && newIndex < mappings.Count)
                            {
                                SysReportGroupHeaderMapping other = mappings[newIndex];
                                int temp = mapping.Order;
                                mapping.Order = other.Order;
                                other.Order = temp;
                            }
                        }
                        result = SaveChanges(entities, transaction);

                        if (result.Success)
                        {
                            transaction.Complete();
                        }
                    }
                }
                catch (Exception ex)
                {
                    result.Exception = ex;
                    base.LogError(ex, this.log);
                }
                finally
                {
                    if (!result.Success)
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }
            }
            return result;
        }
        #endregion

        #region Help-classes

        private class ReportJobStatus
        {
            public int ReportPrintoutId { get; set; }
            public string ReportName { get; set; }
            public DateTime? DeliveredTime { get; set; }
            public int ResultMessage { get; set; }
            public string ResultMessageDetails { get; set; }
            public DateTime Created { get; set; }
            public int Status { get; set; }
            public TermGroup_ReportExportType ExportType { get; set; }
            public SoeReportTemplateType? SysReportTemplateTypeId { get; set; }
        }

        #endregion
    }
}

using Common.Util;
using SoftOne.Soe.Business.Core.Template.Models;
using SoftOne.Soe.Business.Core.Template.Models.Core;
using SoftOne.Soe.Business.Evo.Cache;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SoftOne.Soe.Business.Core.Template.Managers
{
    public class CoreTemplateManager : ManagerBase
    {
        readonly CompanyTemplateManager companyTemplateManager;
        public CoreTemplateManager(ParameterObject parameterObject) : base(parameterObject)
        {
            companyTemplateManager = new CompanyTemplateManager(parameterObject);
        }

        public List<TemplateCompanyItem> GetGlobalTemplateCompanyItems()
        {
            var globalTemplateCompanies = CompanyManager.GetGlobalTemplateCompanies();
            List<TemplateCompanyItem> templateCompanyItems = new List<TemplateCompanyItem>();
            foreach (var comp in globalTemplateCompanies)
            {
                templateCompanyItems.Add(new TemplateCompanyItem()
                {
                    ActorCompanyId = comp.ActorCompanyId,
                    SysCompDbId = ConfigurationSetupUtil.GetCurrentSysCompDbId(),
                    Name = comp.Name,
                    SysCompDbName = ConfigurationSetupUtil.GetCurrentSysCompDbDTO().Name,
                    Global = comp.Global
                });
            }

            return templateCompanyItems;
        }

        public List<TemplateCompanyItem> GetTemplateCompanyItems(int licenseId)
        {
            var templateCompanies = CompanyManager.GetTemplateCompanies(licenseId);
            List<TemplateCompanyItem> templateCompanyItems = new List<TemplateCompanyItem>();
            foreach (var comp in templateCompanies.Where(w => !w.Global))
            {
                templateCompanyItems.Add(new TemplateCompanyItem()
                {
                    ActorCompanyId = comp.ActorCompanyId,
                    SysCompDbId = ConfigurationSetupUtil.GetCurrentSysCompDbId(),
                    Name = comp.Name,
                    SysCompDbName = ConfigurationSetupUtil.GetCurrentSysCompDbDTO().Name,
                });
            }
            return templateCompanyItems;
        }



        public LicenseCopyItem GetLicenseCopyItemFromApi(int sysCompDbId, int actorCompanyId)
        {
            if (sysCompDbId == 0)
                return null;

            if (sysCompDbId == ConfigurationSetupUtil.GetCurrentSysCompDbId())
                return GetLicenseCopyItem(actorCompanyId);

            return coreTemplateConnector.GetLicenseCopyItem(sysCompDbId, actorCompanyId);
        }

        public LicenseCopyItem GetLicenseCopyItem(int actorCompanyId)
        {
            try
            {
                Company company = CompanyManager.GetCompany(actorCompanyId);
                License license = LicenseManager.GetLicense(company.ActorCompanyId);

                LicenseCopyItem licenseCopyItem = new LicenseCopyItem()
                {
                    LicenseId = license.LicenseId,
                    LicenseNr = license.LicenseNr,
                    Name = license.Name + "_copy",
                    OrgNr = license.OrgNr,
                    Support = license.Support,
                    NrOfCompanies = license.NrOfCompanies,
                    MaxNrOfUsers = license.MaxNrOfUsers,
                    MaxNrOfEmployees = license.MaxNrOfEmployees,
                    MaxNrOfMobileUsers = license.MaxNrOfMobileUsers,
                    ConcurrentUsers = license.ConcurrentUsers,
                    TerminationDate = license.TerminationDate,
                    Created = license.Created,
                    CreatedBy = license.CreatedBy,
                    Modified = license.Modified,
                    ModifiedBy = license.ModifiedBy,
                    State = license.State,
                    AllowDuplicateUserLogin = license.AllowDuplicateUserLogin,
                    LegalName = license.LegalName,
                    IsAccountingOffice = license.IsAccountingOffice,
                    AccountingOfficeId = license.AccountingOfficeId,
                    AccountingOfficeName = license.AccountingOfficeName,
                    SysServerId = license.SysServerId
                };

                CompanyCopyItem companyCopyItem = new CompanyCopyItem()
                {
                    Name = company.Name,
                    ActorCompanyId = company.ActorCompanyId,
                    CompanyData = new TemplateCompanyDataItem(ConfigurationSetupUtil.GetCurrentSysCompDbId())
                };

                companyCopyItem.CompanyData.TemplateCompanyCoreDataItem.CompanySettingCopyItems = GetCompanySettingCopyItems(actorCompanyId);
                companyCopyItem.CompanyData.TemplateCompanyCoreDataItem.ImportCopyItems = GetImportCopyItems(actorCompanyId);
                companyCopyItem.CompanyData.TemplateCompanyCoreDataItem.CompanyFieldSettingCopyItems = GetCompanyFieldSettingCopyItems(actorCompanyId);
                companyCopyItem.CompanyData.TemplateCompanyCoreDataItem.CompanyAndFeatureCopyItems = GetCompanyAndFeatureCopyItems(actorCompanyId);
                companyCopyItem.CompanyData.TemplateCompanyCoreDataItem.RoleAndFeatureCopyItems = GetRoleAndFeatureCopyItems(actorCompanyId);
                companyCopyItem.CompanyData.TemplateCompanyCoreDataItem.ReportTemplateCopyItems = GetReportTemplateCopyItems(actorCompanyId);
                companyCopyItem.CompanyData.TemplateCompanyCoreDataItem.ReportCopyItems = GetReportCopyItems(actorCompanyId);
                companyCopyItem.CompanyData.TemplateCompanyCoreDataItem.ExternalCodeCopyItems = GetExternalCodeCopyItems(actorCompanyId);
                companyCopyItem.CompanyData.TemplateCompanyCoreDataItem.UserCopyItems = GetUserCopyItems(actorCompanyId);
                licenseCopyItem.CompanyCopyItems.Add(companyCopyItem);
                return licenseCopyItem;
            }
            catch (Exception ex)
            {
                LogError(ex.ToString());
            }

            return null;
        }

        public TemplateCompanyCoreDataItem GetTemplateCompanyCoreDataItem(CopyFromTemplateCompanyInputDTO inputDTO)
        {
            TemplateCompanyCoreDataItem item = new TemplateCompanyCoreDataItem();

            item.CompanySettingCopyItems = GetCompanySettingCopyItemsFromApi(inputDTO.SysCompDbId, inputDTO.TemplateCompanyId);

            if (inputDTO.DoCopy(TemplateCompanyCopy.All))
                item.ImportCopyItems = GetImportCopyItemsFromApi(inputDTO.SysCompDbId, inputDTO.TemplateCompanyId);

            if (inputDTO.DoCopy(TemplateCompanyCopy.CompanyFieldSettings))
                item.CompanyFieldSettingCopyItems = GetCompanyFieldSettingCopyItemsFromApi(inputDTO.SysCompDbId, inputDTO.TemplateCompanyId);

            if (inputDTO.DoCopy(TemplateCompanyCopy.RolesAndFeatures))
            {
                item.CompanyAndFeatureCopyItems = GetCompanyAndFeatureCopyItemsFromApi(inputDTO.SysCompDbId, inputDTO.TemplateCompanyId);
                item.RoleAndFeatureCopyItems = GetRoleAndFeatureCopyItemsFromApi(inputDTO.SysCompDbId, inputDTO.TemplateCompanyId);
            }
            if (inputDTO.DoCopy(TemplateCompanyCopy.ReportsAndReportTemplates))
            {
                item.ReportTemplateCopyItems = GetReportTemplateCopyItemsFromApi(inputDTO.SysCompDbId, inputDTO.TemplateCompanyId);
                item.ReportCopyItems = GetReportCopyItemsFromApi(inputDTO.SysCompDbId, inputDTO.TemplateCompanyId);
            }

            item.ExternalCodeCopyItems = GetExternalCodeCopyItemsFromApi(inputDTO.SysCompDbId, inputDTO.TemplateCompanyId);

            return item;
        }

        public List<TemplateResult> CopyTemplateCompanyCoreDataItem(CopyFromTemplateCompanyInputDTO inputDTO, TemplateCompanyDataItem templateCompanyDataItem)
        {
            List<TemplateResult> templateResults = new List<TemplateResult>();

            if (inputDTO.DoCopy(TemplateCompanyCopy.All))
                templateResults.Add(CopyImportsFromTemplateCompany(templateCompanyDataItem));

            if (inputDTO.DoCopy(TemplateCompanyCopy.CompanyFieldSettings))
                templateResults.Add(CopyCompanyFieldSettingsFromTemplateCompany(templateCompanyDataItem));

            if (inputDTO.doCopy(TemplateCompanyCopy.RolesAndFeatures))
                templateResults.Add(CopyRolesAndFeaturesFromTemplateCompany(templateCompanyDataItem));

            if (inputDTO.DoCopy(TemplateCompanyCopy.ReportsAndReportTemplates))
                templateResults.Add(CopyReportsFromTemplateCompany(templateCompanyDataItem));

            return templateResults;
        }

        public TemplateResult CopyImportsFromTemplateCompany(TemplateCompanyDataItem templateCompanyDataItem)
        {
            ActionResult result = new ActionResult();
            TemplateResult templateResult = new TemplateResult();

            try
            {

                #region Prereq
                List<Import> existingImports = ImportExportManager.GetImports(templateCompanyDataItem.DestinationActorCompanyId, false);

                if (templateCompanyDataItem?.TemplateCompanyCoreDataItem?.ImportCopyItems == null)
                    return templateResult;

                #endregion

                using (CompEntities entities = new CompEntities())
                {
                    #region Prereq

                    Company newCompany = CompanyManager.GetCompany(entities, templateCompanyDataItem.DestinationActorCompanyId);
                    if (newCompany == null)
                    {
                        result = new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte"));
                        return templateResult;
                    }

                    #endregion

                    foreach (var importCopyItem in templateCompanyDataItem.TemplateCompanyCoreDataItem.ImportCopyItems)
                    {
                        if (existingImports.Exists(e => e.Name == importCopyItem.Name))
                            continue;

                        #region New Import

                        Import newImport = new Import()
                        {
                            ActorCompanyId = templateCompanyDataItem.DestinationActorCompanyId,
                            Name = importCopyItem.Name,
                            ImportDefinitionId = importCopyItem.ImportDefinitionId,
                            Type = importCopyItem.Type,
                            Module = importCopyItem.Module,
                            Standard = importCopyItem.IsStandard,
                            State = (int)importCopyItem.State,
                            SpecialFunctionality = importCopyItem.SpecialFunctionality,
                            Guid = importCopyItem.Guid,
                            ImportHeadType = (int)importCopyItem.ImportHeadType
                        };

                        SetCreatedProperties(newImport);
                        newCompany.Import.Add(newImport);

                        #endregion
                    }

                    result = SaveChanges(entities);
                    if (!result.Success)
                        result = companyTemplateManager.LogCopyError("importCopyItems", templateCompanyDataItem, saved: true);
                }
            }
            catch (Exception ex)
            {
                result = new ActionResult(ex);
                LogError(ex.ToString());
            }

            templateResult.ActionResults.Add(result);
            return templateResult;

        }



        public List<ImportCopyItem> GetImportCopyItems(int actorCompanyId)
        {
            List<ImportCopyItem> importCopyItems = new List<ImportCopyItem>();

            try
            {
                List<Import> imports = ImportExportManager.GetImports(actorCompanyId, false);

                foreach (var import in imports)
                {
                    ImportCopyItem importCopyItem = new ImportCopyItem()
                    {
                        ActorCompanyId = actorCompanyId,
                        Name = import.Name,
                        ImportDefinitionId = import.ImportDefinitionId,
                        Type = import.Type,
                        Module = import.Module,
                        IsStandard = import.Standard,
                        State = (SoeEntityState)import.State,
                        SpecialFunctionality = import.SpecialFunctionality,
                        Guid = import.Guid,
                        ImportHeadType = (TermGroup_IOImportHeadType)(import.ImportHeadType ?? 0)
                    };

                    importCopyItems.Add(importCopyItem);
                }

            }
            catch (Exception ex)
            {
                LogError(ex.ToString());
            }

            return importCopyItems;
        }

        public List<ImportCopyItem> GetImportCopyItemsFromApi(int sysCompDbId, int actorCompanyId)
        {
            if (sysCompDbId == 0)
                return new List<ImportCopyItem>();

            if (sysCompDbId == ConfigurationSetupUtil.GetCurrentSysCompDbId())
                return GetImportCopyItems(actorCompanyId);

            return coreTemplateConnector.GetImportCopyItems(sysCompDbId, actorCompanyId);
        }

        public List<ReportTemplateCopyItem> GetReportTemplateCopyItems(int actorCompanyId)
        {
            List<ReportTemplateCopyItem> reportTemplateCopyItems = new List<ReportTemplateCopyItem>();

            try
            {
                List<ReportTemplate> reportTemplates = ReportManager.GetReportTemplates(actorCompanyId);

                foreach (var templateReportTemplate in reportTemplates)
                {
                    var reportTemplateCopyItem = new ReportTemplateCopyItem()
                    {
                        ReportTemplateId = templateReportTemplate.ReportTemplateId,
                        Name = templateReportTemplate.Name,
                        Description = templateReportTemplate.Description,
                        FileName = templateReportTemplate.FileName,
                        Template = Convert.ToBase64String(templateReportTemplate.Template),
                        SysReportTypeId = templateReportTemplate.SysReportTypeId,
                        SysTemplateTypeId = templateReportTemplate.SysTemplateTypeId,
                        Module = (SoeModule)templateReportTemplate.Module,
                        GroupByLevel1 = templateReportTemplate.GroupByLevel1,
                        GroupByLevel2 = templateReportTemplate.GroupByLevel2,
                        GroupByLevel3 = templateReportTemplate.GroupByLevel3,
                        GroupByLevel4 = templateReportTemplate.GroupByLevel4,
                        SortByLevel1 = templateReportTemplate.SortByLevel1,
                        SortByLevel2 = templateReportTemplate.SortByLevel2,
                        SortByLevel3 = templateReportTemplate.SortByLevel3,
                        SortByLevel4 = templateReportTemplate.SortByLevel4,
                        IsSortAscending = templateReportTemplate.IsSortAscending,
                        Special = templateReportTemplate.Special,
                        ReportNr = templateReportTemplate.ReportNr,
                        ShowOnlyTotals = templateReportTemplate.ShowOnlyTotals,
                        ShowGroupingAndSorting = templateReportTemplate.ShowGroupingAndSorting,
                        ValidExportTypes = templateReportTemplate.ValidExportTypes
                    };

                    reportTemplateCopyItems.Add(reportTemplateCopyItem);
                }
            }
            catch (Exception ex)
            {
                LogError(ex.ToString());
            }

            return reportTemplateCopyItems;
        }

        public List<ReportTemplateCopyItem> GetReportTemplateCopyItemsFromApi(int sysCompDbId, int actorCompanyId)
        {
            if (sysCompDbId == 0)
                return new List<ReportTemplateCopyItem>();

            if (sysCompDbId == ConfigurationSetupUtil.GetCurrentSysCompDbId())
                return GetReportTemplateCopyItems(actorCompanyId);

            return coreTemplateConnector.GetReportTemplateCopyItems(sysCompDbId, actorCompanyId);
        }

        public TemplateResult CopyReportsFromTemplateCompany(TemplateCompanyDataItem item)
        {
            ActionResult result = new ActionResult();
            TemplateResult templateResult = new TemplateResult();

            try
            {
                #region Prereq
                List<Report> existingReports = ReportManager.GetReports(item.DestinationActorCompanyId, null, loadReportSelection: true, loadRolePermission: true, loadSysReportTemplate: false, filterReports: false);

                if (item?.TemplateCompanyCoreDataItem?.ReportCopyItems == null)
                    return templateResult;

                #endregion

                using (CompEntities entities = new CompEntities())
                {
                    #region Prereq

                    Company newCompany = CompanyManager.GetCompany(entities, item.DestinationActorCompanyId);
                    if (newCompany == null)
                    {
                        result = new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte"));
                        return templateResult;
                    }

                    List<ReportTemplate> existingReportTemplates = ReportManager.GetReportTemplates(entities, item.DestinationActorCompanyId);

                    #endregion

                    foreach (var reportTemplateCopyItem in item.TemplateCompanyCoreDataItem.ReportTemplateCopyItems)
                    {

                        ReportTemplate reportTemplate = existingReportTemplates.FirstOrDefault(i => i.Name == reportTemplateCopyItem.Name && i.SysReportTypeId == reportTemplateCopyItem.SysReportTypeId);
                        if (reportTemplate == null)
                        {
                            reportTemplate = new ReportTemplate()
                            {
                                Name = reportTemplateCopyItem.Name,
                                Description = reportTemplateCopyItem.Description,
                                FileName = reportTemplateCopyItem.FileName,
                                Template = Convert.FromBase64String(reportTemplateCopyItem.Template),
                                SysReportTypeId = reportTemplateCopyItem.SysReportTypeId,
                                SysTemplateTypeId = reportTemplateCopyItem.SysTemplateTypeId,
                                Module = (int)reportTemplateCopyItem.Module,
                                GroupByLevel1 = reportTemplateCopyItem.GroupByLevel1,
                                GroupByLevel2 = reportTemplateCopyItem.GroupByLevel2,
                                GroupByLevel3 = reportTemplateCopyItem.GroupByLevel3,
                                GroupByLevel4 = reportTemplateCopyItem.GroupByLevel4,
                                SortByLevel1 = reportTemplateCopyItem.SortByLevel1,
                                SortByLevel2 = reportTemplateCopyItem.SortByLevel2,
                                SortByLevel3 = reportTemplateCopyItem.SortByLevel3,
                                SortByLevel4 = reportTemplateCopyItem.SortByLevel4,
                                IsSortAscending = reportTemplateCopyItem.IsSortAscending,
                                Special = reportTemplateCopyItem.Special,
                                ReportNr = reportTemplateCopyItem.ReportNr,
                                ShowOnlyTotals = reportTemplateCopyItem.ShowOnlyTotals,
                                ShowGroupingAndSorting = reportTemplateCopyItem.ShowGroupingAndSorting,
                                ValidExportTypes = reportTemplateCopyItem.ValidExportTypes,

                                //References
                                Company = newCompany,
                            };
                            entities.ReportTemplate.AddObject(reportTemplate);
                        }

                        item.TemplateCompanyCoreDataItem.AddReportTemplateMapping(reportTemplateCopyItem.ReportTemplateId, reportTemplate);
                    }

                    result = SaveChanges(entities);
                    if (!result.Success)
                        result = companyTemplateManager.LogCopyError("ReportCopyItems", item, saved: true);

                    foreach (var reportCopyItem in item.TemplateCompanyCoreDataItem.ReportCopyItems)
                    {
                        Report newReport = existingReports.FirstOrDefault(i => i.Name == reportCopyItem.Name && i.ReportNr == reportCopyItem.ReportNr);
                        if (newReport == null)
                        {

                            #region New Report

                            newReport = new Report()
                            {
                                ReportNr = reportCopyItem.ReportNr,
                                Name = reportCopyItem.Name,
                                Description = reportCopyItem.Description,
                                Standard = reportCopyItem.Standard,
                                Original = reportCopyItem.Original,
                                Module = (int)reportCopyItem.Module,
                                IncludeAllHistoricalData = reportCopyItem.IncludeAllHistoricalData,
                                GetDetailedInformation = reportCopyItem.GetDetailedInformation,
                                NoOfYearsBackinPreviousYear = reportCopyItem.NoOfYearsBackinPreviousYear,
                                IncludeBudget = reportCopyItem.IncludeBudget,
                                ShowInAccountingReports = reportCopyItem.ShowInAccountingReports,
                                ExportType = reportCopyItem.ExportType,
                                FileType = reportCopyItem.FileType,
                                GroupByLevel1 = reportCopyItem.GroupByLevel1,
                                GroupByLevel2 = reportCopyItem.GroupByLevel2,
                                GroupByLevel3 = reportCopyItem.GroupByLevel3,
                                GroupByLevel4 = reportCopyItem.GroupByLevel4,
                                SortByLevel1 = reportCopyItem.SortByLevel1,
                                SortByLevel2 = reportCopyItem.SortByLevel2,
                                SortByLevel3 = reportCopyItem.SortByLevel3,
                                SortByLevel4 = reportCopyItem.SortByLevel4,
                                IsSortAscending = reportCopyItem.IsSortAscending,
                                Special = reportCopyItem.Special,
                                NrOfDecimals = reportCopyItem.NrOfDecimals,
                                ActorCompanyId = newCompany.ActorCompanyId
                            };

                            SetCreatedProperties(newReport);
                            newCompany.Report.Add(newReport);
                        }

                        #endregion

                        #region ReportTemplate

                        bool foundReportTemplate = false;
                        if (reportCopyItem.Standard)
                        {
                            newReport.ReportTemplateId = reportCopyItem.ReportTemplateId;
                            foundReportTemplate = true;
                        }
                        else
                        {
                            newReport.ReportTemplateId = item.TemplateCompanyCoreDataItem.GetReportTemplate(reportCopyItem.ReportTemplateId)?.ReportTemplateId ?? 0;
                            if (newReport.ReportTemplateId != 0)
                                foundReportTemplate = true;
                            else
                                foundReportTemplate = false;
                        }

                        #endregion

                        if (foundReportTemplate)
                        {
                            // Copy ReportRolePermission
                            if (!reportCopyItem.ReportRolePermission.IsNullOrEmpty())
                            {
                                foreach (var templateReportRolePermission in reportCopyItem.ReportRolePermission)
                                {
                                    var role = item.TemplateCompanyCoreDataItem.GetRole(templateReportRolePermission.RoleId);
                                    if (role == null)
                                        continue;

                                    ReportRolePermission reportRolePermission = new ReportRolePermission()
                                    {
                                        Report = newReport,
                                        RoleId = item.TemplateCompanyCoreDataItem.GetRole(templateReportRolePermission.RoleId)?.RoleId ?? 0,
                                        ActorCompanyId = newCompany.ActorCompanyId
                                    };
                                    SetCreatedProperties(reportRolePermission);
                                    newReport.ReportRolePermission.Add(reportRolePermission);
                                }
                            }

                            // Copy ReportSelection
                            if (reportCopyItem.ReportSelection != null)
                            {
                                ReportSelection reportSelection = new ReportSelection()
                                {
                                    ReportSelectionText = reportCopyItem.ReportSelection.ReportSelectionText
                                };
                                newReport.ReportSelection = reportSelection;

                                foreach (var templateReportSelectionInt in reportCopyItem.ReportSelection.ReportSelectionValueCopyItems.Where(w => w.SelectFromInt.HasValue))
                                {
                                    ReportSelectionInt reportSelectionInt = new ReportSelectionInt()
                                    {
                                        ReportSelectionType = templateReportSelectionInt.ReportSelectionType,
                                        SelectFrom = templateReportSelectionInt.SelectFromInt ?? 0,
                                        SelectTo = templateReportSelectionInt.SelectToInt ?? 0
                                    };
                                    reportSelection.ReportSelectionInt.Add(reportSelectionInt);
                                }

                                foreach (var templateReportSelectionStr in reportCopyItem.ReportSelection.ReportSelectionValueCopyItems.Where(w => w.SelectFromStr != null))
                                {
                                    ReportSelectionStr reportSelectionStr = new ReportSelectionStr()
                                    {
                                        ReportSelectionType = templateReportSelectionStr.ReportSelectionType,
                                        SelectFrom = templateReportSelectionStr.SelectFromStr,
                                        SelectTo = templateReportSelectionStr.SelectToStr
                                    };
                                    reportSelection.ReportSelectionStr.Add(reportSelectionStr);
                                }

                                foreach (var templateReportSelectionDate in reportCopyItem.ReportSelection.ReportSelectionValueCopyItems.Where(w => w.SelectFromStr != null))
                                {
                                    ReportSelectionDate reportSelectionDate = new ReportSelectionDate()
                                    {
                                        ReportSelectionType = templateReportSelectionDate.ReportSelectionType,
                                        SelectFrom = templateReportSelectionDate.SelectFromDate ?? CalendarUtility.DATETIME_DEFAULT,
                                        SelectTo = templateReportSelectionDate.SelectToDate ?? CalendarUtility.DATETIME_DEFAULT
                                    };
                                    reportSelection.ReportSelectionDate.Add(reportSelectionDate);
                                }

                            }
                        }
                        else
                        {
                            result = companyTemplateManager.LogCopyError("ReportCopyItems", "ReportId", reportCopyItem.ReportId, reportCopyItem.ReportNr.ToString(), reportCopyItem.Name, item, add: true);
                        }

                        result = SaveChanges(entities);
                        if (!result.Success)
                            result = companyTemplateManager.LogCopyError("ReportCopyItems", item, saved: true);

                        item.TemplateCompanyCoreDataItem.AddReportMapping(reportCopyItem.ReportId, newReport);
                    }

                }
            }
            catch (Exception ex)
            {
                result = new ActionResult(ex);
                LogError(ex.ToString());
            }

            templateResult.ActionResults.Add(result);
            return templateResult;
        }

        public List<ReportCopyItem> GetReportCopyItems(int actorCompanyId)
        {
            List<ReportCopyItem> reportCopyItems = new List<ReportCopyItem>();

            try
            {
                List<Report> Reports = ReportManager.GetReports(actorCompanyId, null, loadReportSelection: true, loadRolePermission: true, loadSysReportTemplate: false, filterReports: false);

                foreach (var report in Reports)
                {
                    var reportCopyItem = new ReportCopyItem()
                    {
                        ReportId = report.ReportId,
                        ReportNr = report.ReportNr,
                        Name = report.Name,
                        Description = report.Description,
                        ReportTemplateId = report.ReportTemplateId,
                        Standard = report.Standard,
                        Original = report.Original,
                        Module = (SoeModule)report.Module,
                        IncludeAllHistoricalData = report.IncludeAllHistoricalData,
                        GetDetailedInformation = report.GetDetailedInformation,
                        NoOfYearsBackinPreviousYear = report.NoOfYearsBackinPreviousYear,
                        IncludeBudget = report.IncludeBudget,
                        ShowInAccountingReports = report.ShowInAccountingReports,
                        ExportType = report.ExportType,
                        FileType = report.FileType,
                        GroupByLevel1 = report.GroupByLevel1,
                        GroupByLevel2 = report.GroupByLevel2,
                        GroupByLevel3 = report.GroupByLevel3,
                        GroupByLevel4 = report.GroupByLevel4,
                        SortByLevel1 = report.SortByLevel1,
                        SortByLevel2 = report.SortByLevel2,
                        SortByLevel3 = report.SortByLevel3,
                        SortByLevel4 = report.SortByLevel4,
                        IsSortAscending = report.IsSortAscending,
                        Special = report.Special
                    };

                    if (!report.ReportRolePermission.IsNullOrEmpty())
                    {
                        foreach (var role in report.ReportRolePermission)
                        {
                            reportCopyItem.ReportRolePermission.Add(new RoleCopyItem()
                            {
                                RoleId = role.RoleId
                            });
                        }
                    }

                    if (report.ReportSelection != null)
                    {
                        var reportSelectionCopyItem = new ReportSelectionCopyItem()
                        {
                            ReportSelectionText = report.ReportSelection.ReportSelectionText
                        };

                        if (report.ReportSelection.ReportSelectionInt != null)
                        {
                            foreach (var reportSelectionInt in report.ReportSelection.ReportSelectionInt)
                            {
                                reportSelectionCopyItem.ReportSelectionValueCopyItems.Add(new ReportSelectionValueCopyItem()
                                {
                                    ReportSelectionType = reportSelectionInt.ReportSelectionType,
                                    SelectFromInt = reportSelectionInt.SelectFrom,
                                    SelectToInt = reportSelectionInt.SelectTo
                                });
                            }
                        }
                        if (report.ReportSelection.ReportSelectionStr != null)
                        {
                            foreach (var reportSelectionStr in report.ReportSelection.ReportSelectionStr)
                            {
                                reportSelectionCopyItem.ReportSelectionValueCopyItems.Add(new ReportSelectionValueCopyItem()
                                {
                                    ReportSelectionType = reportSelectionStr.ReportSelectionType,
                                    SelectFromStr = reportSelectionStr.SelectFrom,
                                    SelectToStr = reportSelectionStr.SelectTo
                                });
                            }
                        }

                        if (report.ReportSelection.ReportSelectionDate != null)
                        {
                            foreach (var reportSelectionDate in report.ReportSelection.ReportSelectionDate)
                            {
                                reportSelectionCopyItem.ReportSelectionValueCopyItems.Add(new ReportSelectionValueCopyItem()
                                {
                                    ReportSelectionType = reportSelectionDate.ReportSelectionType,
                                    SelectFromDate = reportSelectionDate.SelectFrom,
                                    SelectToDate = reportSelectionDate.SelectTo
                                });
                            }
                        }
                    }

                    reportCopyItems.Add(reportCopyItem);
                }
            }
            catch (Exception ex)
            {
                LogError(ex.ToString());
            }

            return reportCopyItems;
        }


        public List<ReportCopyItem> GetReportCopyItemsFromApi(int sysCompDbId, int actorCompanyId)
        {
            if (sysCompDbId == 0)
                return new List<ReportCopyItem>();

            if (sysCompDbId == ConfigurationSetupUtil.GetCurrentSysCompDbId())
                return GetReportCopyItems(actorCompanyId);

            return coreTemplateConnector.GetReportCopyItems(sysCompDbId, actorCompanyId);
        }

        public List<CompanySettingCopyItem> GetCompanySettingCopyItems(int actorCompanyId)
        {
            List<CompanySettingCopyItem> companySettingCopyItems = new List<CompanySettingCopyItem>();

            try
            {
                List<UserCompanySetting> userCompanySettings = SettingManager.GetUserCompanySettingsForCompany(actorCompanyId);

                foreach (var setting in userCompanySettings)
                {
                    CompanySettingCopyItem settingCopyItem = new CompanySettingCopyItem()
                    {
                        UserCompanySettingId = setting.UserCompanySettingId,
                        ActorCompanyId = setting.ActorCompanyId,
                        SettingTypeId = (CompanySettingType)setting.SettingTypeId,
                        DataTypeId = (SettingDataType)setting.DataTypeId,
                        StrData = setting.StrData,
                        IntData = setting.IntData,
                        BoolData = setting.BoolData,
                        DateData = setting.DateData,
                        DecimalData = setting.DecimalData,
                    };

                    companySettingCopyItems.Add(settingCopyItem);
                }
            }
            catch (Exception ex)
            {
                LogError(ex.ToString());
            }

            return companySettingCopyItems;
        }

        public List<CompanySettingCopyItem> GetCompanySettingCopyItemsFromApi(int sysCompDbId, int actorCompanyId)
        {
            if (sysCompDbId == 0)
                return new List<CompanySettingCopyItem>();

            if (sysCompDbId == ConfigurationSetupUtil.GetCurrentSysCompDbId())
                return GetCompanySettingCopyItems(actorCompanyId);

            return coreTemplateConnector.GetCompanySettingCopyItems(sysCompDbId, actorCompanyId);
        }

        public TemplateResult CopyCompanyFieldSettingsFromTemplateCompany(TemplateCompanyDataItem templateCompanyDataItem)
        {
            ActionResult result = new ActionResult();
            TemplateResult templateResult = new TemplateResult();

            try
            {
                #region Prereq

                List<CompanyFieldSetting> existingCompanyFieldSettings = FieldSettingManager.GetCompanyFieldSettings(templateCompanyDataItem.DestinationActorCompanyId);

                #endregion

                foreach (var templateCompanyFieldSetting in templateCompanyDataItem.TemplateCompanyCoreDataItem.CompanyFieldSettingCopyItems)
                {
                    var setting = existingCompanyFieldSettings.FirstOrDefault(s => s.FormId == templateCompanyFieldSetting.FormId && s.FieldId == templateCompanyFieldSetting.FieldId && s.SysSettingId == templateCompanyFieldSetting.SysSettingId);
                    if (setting == null)
                    {
                        try
                        {
                            result = FieldSettingManager.SaveCompanyFieldSetting(templateCompanyFieldSetting.FormId, templateCompanyFieldSetting.FieldId, templateCompanyFieldSetting.SysSettingId, templateCompanyFieldSetting.Value, templateCompanyDataItem.DestinationActorCompanyId);
                            if (!result.Success)
                                result = companyTemplateManager.LogCopyError("CompanyFieldSetting", "SysSettingId", templateCompanyFieldSetting.SysSettingId, "", "", templateCompanyDataItem, add: true);
                        }
                        catch (Exception ex)
                        {
                            result = companyTemplateManager.LogCopyError("CompanyFieldSetting", "SysSettingId", templateCompanyFieldSetting.SysSettingId, "", "", templateCompanyDataItem, ex);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                result = new ActionResult(ex);
                LogError(ex.ToString());
            }

            templateResult.ActionResults.Add(result);
            return templateResult;
        }

        public List<CompanyFieldSettingCopyItem> GetCompanyFieldSettingCopyItems(int actorCompanyId)
        {
            List<CompanyFieldSettingCopyItem> companyFieldSettingCopyItems = new List<CompanyFieldSettingCopyItem>();

            try
            {
                List<CompanyFieldSetting> companyFieldSettings = FieldSettingManager.GetCompanyFieldSettings(actorCompanyId);

                foreach (var companyFieldSetting in companyFieldSettings)
                {
                    CompanyFieldSettingCopyItem companyFieldSettingCopyItem = new CompanyFieldSettingCopyItem()
                    {
                        ActorCompanyId = actorCompanyId,
                        FormId = companyFieldSetting.FormId,
                        FieldId = companyFieldSetting.FieldId,
                        SysSettingId = companyFieldSetting.SysSettingId,
                        Value = companyFieldSetting.Value
                    };

                    companyFieldSettingCopyItems.Add(companyFieldSettingCopyItem);
                }
            }
            catch (Exception ex)
            {
                LogError(ex.ToString());
            }

            return companyFieldSettingCopyItems;
        }

        public List<CompanyFieldSettingCopyItem> GetCompanyFieldSettingCopyItemsFromApi(int sysCompDbId, int actorCompanyId)
        {
            if (sysCompDbId == 0)
                return new List<CompanyFieldSettingCopyItem>();

            if (sysCompDbId == ConfigurationSetupUtil.GetCurrentSysCompDbId())
                return GetCompanyFieldSettingCopyItems(actorCompanyId);

            return coreTemplateConnector.GetCompanyFieldSettingCopyItems(sysCompDbId, actorCompanyId);
        }

        public TemplateResult CopyRolesAndFeaturesFromTemplateCompany(TemplateCompanyDataItem item)
        {
            ActionResult result = new ActionResult();
            TemplateResult templateResult = new TemplateResult();
            List<int> roleIds = new List<int>();

            try
            {
                #region Prereq
                List<Role> existingRoles = RoleManager.GetRolesByCompany(item.DestinationActorCompanyId, loadExternalCode: true);
                List<RoleFeature> existingRoleFeatures = FeatureManager.GetRoleFeaturesForCompany(item.DestinationActorCompanyId);
                List<CompanyFeature> existingCompanyFeatures = FeatureManager.GetCompanyFeatures(item.DestinationActorCompanyId);

                #endregion

                using (CompEntities entities = new CompEntities())
                {
                    #region CompanyFeatures

                    foreach (var roleAndFeatureCopyItem in item.TemplateCompanyCoreDataItem.RoleAndFeatureCopyItems)
                    {
                        #region Role

                        Role existingRole = existingRoles.FirstOrDefault(r => r.TermId == roleAndFeatureCopyItem.TermId && r.Name == roleAndFeatureCopyItem.RoleName);
                        if (existingRole == null && !roleAndFeatureCopyItem.IsAdmin)
                        {
                            existingRole = new Role()
                            {
                                TermId = roleAndFeatureCopyItem.TermId,
                                Name = roleAndFeatureCopyItem.RoleName,
                                ExternalCodesString = roleAndFeatureCopyItem.ExternalCodesString,
                                Sort = roleAndFeatureCopyItem.Sort,

                                //Set FK
                                ActorCompanyId = item.DestinationActorCompanyId,
                            };
                            SetCreatedProperties(existingRole);
                            entities.Role.AddObject(existingRole);
                            existingRoles.Add(existingRole);
                        }
                        else if (roleAndFeatureCopyItem.IsAdmin)
                        {
                            existingRole = existingRoles.FirstOrDefault(r => r.TermId == roleAndFeatureCopyItem.TermId && r.Name == roleAndFeatureCopyItem.RoleName && r.IsAdmin);
                        }

                        if (existingRole == null)
                            continue;

                        result = SaveChanges(entities, null, useBulkSaveChanges: true);
                        if (!result.Success)
                            result = companyTemplateManager.LogCopyError("Role", "RoleId", roleAndFeatureCopyItem.RoleId, "", roleAndFeatureCopyItem.RoleName, item, add: true);

                        item.TemplateCompanyCoreDataItem.AddRoleMapping(roleAndFeatureCopyItem.RoleId, existingRole);

                        #endregion

                        #region RoleFeatures

                        foreach (var roleFeatureCopyItem in roleAndFeatureCopyItem.RoleFeatures)
                        {
                            #region RoleFeature

                            try
                            {
                                RoleFeature existingRoleFeature = existingRoleFeatures.FirstOrDefault(rf => rf.SysFeatureId == roleFeatureCopyItem.SysFeatureId && rf.RoleId == existingRole.RoleId);
                                if (existingRoleFeature == null)
                                {
                                    existingRoleFeature = new RoleFeature()
                                    {
                                        SysFeatureId = roleFeatureCopyItem.SysFeatureId,
                                        SysPermissionId = roleFeatureCopyItem.SysPermissionId,

                                        //Set FK
                                        RoleId = existingRole.RoleId,
                                    };
                                    entities.RoleFeature.AddObject(existingRoleFeature);
                                    SetCreatedProperties(existingRoleFeature);
                                    existingRoleFeatures.Add(existingRoleFeature);
                                }
                                else
                                {
                                    existingRoleFeature.SysPermissionId = roleFeatureCopyItem.SysPermissionId;
                                    SetModifiedProperties(existingRoleFeature);
                                }
                            }
                            catch (Exception ex)
                            {
                                result = companyTemplateManager.LogCopyError("RoleFeature", "SysFeatureId", roleFeatureCopyItem.SysFeatureId, "", roleAndFeatureCopyItem.RoleName, item, ex);
                            }

                            #endregion
                        }

                        result = SaveChanges(entities, null, useBulkSaveChanges: true);
                        if (!result.Success)
                            result = companyTemplateManager.LogCopyError("RoleFeature", "SysFeatureId", 0, "", "", item);

                        FeatureManager.ClearRolePermissionsFromCache(item.DestinationLicenseId, item.DestinationActorCompanyId, existingRole.RoleId);
                        roleIds.Add(existingRole.RoleId);

                        #endregion
                    }
                    #region CompanyFeatures

                    foreach (var companyFeatureCopyItem in item.TemplateCompanyCoreDataItem.CompanyAndFeatureCopyItems.SelectMany(s => s.CompanyFeatures))
                    {
                        #region CompanyFeature

                        try
                        {
                            CompanyFeature existingCompanyFeature = existingCompanyFeatures.FirstOrDefault(rf => rf.SysFeatureId == companyFeatureCopyItem.SysFeatureId);
                            if (existingCompanyFeature == null)
                            {
                                existingCompanyFeature = new CompanyFeature()
                                {
                                    SysFeatureId = companyFeatureCopyItem.SysFeatureId,
                                    SysPermissionId = companyFeatureCopyItem.SysPermissionId,

                                    //Set FK
                                    ActorCompanyId = item.DestinationActorCompanyId,
                                };
                                entities.CompanyFeature.AddObject(existingCompanyFeature);
                                SetCreatedProperties(existingCompanyFeature);
                                existingCompanyFeatures.Add(existingCompanyFeature);
                            }
                            else
                            {
                                existingCompanyFeature.SysPermissionId = companyFeatureCopyItem.SysPermissionId;
                                SetModifiedProperties(existingCompanyFeature);
                            }
                        }
                        catch (Exception ex)
                        {
                            result = companyTemplateManager.LogCopyError("CompanyFeature", "SysFeatureId", companyFeatureCopyItem.SysFeatureId, "", item.DestinationActorCompanyId.ToString(), item, ex);
                        }

                        #endregion
                    }

                    result = SaveChanges(entities, null, useBulkSaveChanges: true);
                    if (!result.Success)
                        result = companyTemplateManager.LogCopyError("CompanyFeature", "SysFeatureId", 0, "", "", item);

                    try
                    {
                        FeatureManager.ClearCompanyPermissionsFromCache(item.DestinationLicenseId, item.DestinationActorCompanyId);
                        FeatureManager.ClearLicensePermissionsFromCache(item.DestinationLicenseId);
                    }
                    catch (Exception ex)
                    {
                        LogError(ex.ToString());
                    }

                    #endregion
                }

                #endregion

            }
            catch (Exception ex)
            {
                result = new ActionResult(ex);
                LogError(ex.ToString());
            }

            try
            {
                foreach (var roleId in roleIds)
                {
                    EvoFeatureCacheInvalidationConnector.InvalidateRoleCache(roleId);
                }
                EvoFeatureCacheInvalidationConnector.InvalidateLicenseCache(item.DestinationLicenseId);
                EvoFeatureCacheInvalidationConnector.InvalidateCompanyCache(item.DestinationActorCompanyId);
            }
            catch (Exception ex)
            {
                LogError(ex.ToString());
            }

            templateResult.ActionResults.Add(result);
            return templateResult;
        }

        public List<RoleAndFeatureCopyItem> GetRoleAndFeatureCopyItemsFromApi(int sysCompDbId, int actorCompanyId)
        {
            if (sysCompDbId == 0)
                return new List<RoleAndFeatureCopyItem>();

            if (sysCompDbId == ConfigurationSetupUtil.GetCurrentSysCompDbId())
                return GetRoleAndFeatureCopyItems(actorCompanyId);

            return coreTemplateConnector.GetRoleAndFeatureCopyItems(sysCompDbId, actorCompanyId);
        }

        public List<RoleAndFeatureCopyItem> GetRoleAndFeatureCopyItems(int actorCompanyId)
        {
            List<RoleAndFeatureCopyItem> roleAndFeatureCopyItems = new List<RoleAndFeatureCopyItem>();

            try
            {
                List<Role> roles = RoleManager.GetRolesByCompany(actorCompanyId, loadExternalCode: true);
                List<RoleFeature> roleFeatures = FeatureManager.GetRoleFeaturesForCompany(actorCompanyId);

                foreach (var role in roles)
                {
                    RoleAndFeatureCopyItem roleAndFeatureCopyItem = new RoleAndFeatureCopyItem()
                    {
                        RoleId = role.RoleId,
                        ActorCompanyId = actorCompanyId,
                        TermId = role.TermId ?? 0,
                        RoleName = role.Name,
                        ExternalCodesString = role.ExternalCodesString,
                        Sort = role.Sort,
                        IsAdmin = role.IsAdmin
                    };

                    foreach (var roleFeature in roleFeatures.Where(rf => rf.RoleId == role.RoleId))
                    {
                        RoleFeatureCopyItem roleFeatureCopyItem = new RoleFeatureCopyItem()
                        {
                            SysFeatureId = roleFeature.SysFeatureId,
                            SysPermissionId = roleFeature.SysPermissionId
                        };

                        roleAndFeatureCopyItem.RoleFeatures.Add(roleFeatureCopyItem);
                    }

                    roleAndFeatureCopyItems.Add(roleAndFeatureCopyItem);
                }
            }
            catch (Exception ex)
            {
                LogError(ex.ToString());
            }

            return roleAndFeatureCopyItems;
        }

        public List<CompanyAndFeatureCopyItem> GetCompanyAndFeatureCopyItemsFromApi(int sysCompDbId, int actorCompanyId)
        {
            if (sysCompDbId == 0)
                return new List<CompanyAndFeatureCopyItem>();

            if (sysCompDbId == ConfigurationSetupUtil.GetCurrentSysCompDbId())
                return GetCompanyAndFeatureCopyItems(actorCompanyId);

            return coreTemplateConnector.GetCompanyAndFeatureCopyItems(sysCompDbId, actorCompanyId);
        }

        public List<CompanyAndFeatureCopyItem> GetCompanyAndFeatureCopyItems(int actorCompanyId)
        {
            List<CompanyAndFeatureCopyItem> roleAndFeatureCopyItems = new List<CompanyAndFeatureCopyItem>();

            try
            {
                List<CompanyFeature> roleFeatures = FeatureManager.GetCompanyFeatures(actorCompanyId);

                CompanyAndFeatureCopyItem roleAndFeatureCopyItem = new CompanyAndFeatureCopyItem()
                {
                    ActorCompanyId = actorCompanyId
                };

                foreach (var roleFeature in roleFeatures)
                {
                    CompanyFeatureCopyItem roleFeatureCopyItem = new CompanyFeatureCopyItem()
                    {
                        SysFeatureId = roleFeature.SysFeatureId,
                        SysPermissionId = roleFeature.SysPermissionId
                    };

                    roleAndFeatureCopyItem.CompanyFeatures.Add(roleFeatureCopyItem);
                }

                roleAndFeatureCopyItems.Add(roleAndFeatureCopyItem);

            }
            catch (Exception ex)
            {
                LogError(ex.ToString());
            }

            return roleAndFeatureCopyItems;
        }

        public List<ExternalCodeCopyItem> GetExternalCodeCopyItems(int actorCompanyId)
        {
            List<ExternalCodeCopyItem> externalCodeCopyItems = new List<ExternalCodeCopyItem>();
            List<int> externalCodeEntities = new List<int>() {
                    (int)TermGroup_CompanyExternalCodeEntity.EmployeeGroup,
                    (int)TermGroup_CompanyExternalCodeEntity.PayrollGroup,
                    (int)TermGroup_CompanyExternalCodeEntity.Role,
                    (int)TermGroup_CompanyExternalCodeEntity.AttestRole };
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            var externalCodes = entitiesReadOnly.CompanyExternalCode.Where(w => w.ActorCompanyId == actorCompanyId && w.State == (int)SoeEntityState.Active && externalCodeEntities.Contains(w.Entity)).ToList();

            foreach (var code in externalCodes)
            {
                ExternalCodeCopyItem item = new ExternalCodeCopyItem()
                {
                    ActorCompanyId = actorCompanyId,
                    CompanyExternalCodeId = code.CompanyExternalCodeId,
                    Entity = (TermGroup_CompanyExternalCodeEntity)code.Entity,
                    ExternalCode = code.ExternalCode,
                    RecordId = code.RecordId
                };

                externalCodeCopyItems.Add(item);
            }

            return externalCodeCopyItems;
        }

        public List<ExternalCodeCopyItem> GetExternalCodeCopyItemsFromApi(int sysCompDbId, int actorCompanyId)
        {
            if (sysCompDbId == 0)
                return new List<ExternalCodeCopyItem>();

            if (sysCompDbId == ConfigurationSetupUtil.GetCurrentSysCompDbId())
                return GetExternalCodeCopyItems(actorCompanyId);

            return coreTemplateConnector.GetExternalCodeCopyItems(sysCompDbId, actorCompanyId);
        }

        public int CopyCompanyExternalCodes(CompEntities entities, TemplateCompanyDataItem item, List<ExternalCodeCopyItem> existingExternalCodes, TermGroup_CompanyExternalCodeEntity entity)
        {
            int counter = 0;

            List<ExternalCodeCopyItem> filteredTemplateCompanyCoreDataItems = item.TemplateCompanyCoreDataItem.ExternalCodeCopyItems.Where(i => i.Entity == entity).ToList();
            if (filteredTemplateCompanyCoreDataItems.IsNullOrEmpty())
                return counter;

            List<ExternalCodeCopyItem> filteredExistingExternalCodes = existingExternalCodes.Where(i => i.Entity == entity).ToList();

            foreach (var templateCompanyCoreDataItems in filteredTemplateCompanyCoreDataItems)
            {
                int recordId = -1;
                switch (entity)
                {
                    case TermGroup_CompanyExternalCodeEntity.EmployeeGroup:
                        var employeeGroup = item.TemplateCompanyTimeDataItem.GetEmployeeGroup(templateCompanyCoreDataItems.RecordId);
                        if (employeeGroup != null)
                        {
                            recordId = employeeGroup.EmployeeGroupId;
                            counter++;
                        }
                        break;
                    case TermGroup_CompanyExternalCodeEntity.PayrollGroup:
                        var payrollGroup = item.TemplateCompanyTimeDataItem.GetPayrollGroup(templateCompanyCoreDataItems.RecordId);
                        if (payrollGroup != null)
                        {
                            recordId = payrollGroup.PayrollGroupId;
                            counter++;
                        }
                        break;
                    case TermGroup_CompanyExternalCodeEntity.Role:
                        var role = item.TemplateCompanyCoreDataItem.GetRole(templateCompanyCoreDataItems.RecordId);
                        if (role != null)
                        {
                            recordId = role.RoleId;
                            counter++;
                        }
                        break;
                    case TermGroup_CompanyExternalCodeEntity.AttestRole:
                        var attestRole = item.TemplateCompanyAttestDataItem.GetAttestRole(templateCompanyCoreDataItems.RecordId);
                        if (attestRole != null)
                        {
                            recordId = attestRole.AttestRoleId;
                            counter++;
                        }
                        break;
                }

                if (recordId < 0)
                    continue;


                CompanyExternalCode newExternalCode = null;
                var existingCode = filteredExistingExternalCodes.FirstOrDefault(w => w.RecordId == recordId);
                if (existingCode != null)
                    newExternalCode = entities.CompanyExternalCode.FirstOrDefault(w => w.CompanyExternalCodeId == existingCode.CompanyExternalCodeId);

                if (newExternalCode == null)
                {
                    newExternalCode = new CompanyExternalCode()
                    {
                        RecordId = recordId,
                        Entity = (int)entity,
                        ExternalCode = templateCompanyCoreDataItems.ExternalCode,

                        //Set FK
                        ActorCompanyId = item.DestinationActorCompanyId
                    };
                    entities.CompanyExternalCode.AddObject(newExternalCode);
                }
                else
                {
                    newExternalCode.ExternalCode = templateCompanyCoreDataItems.ExternalCode;
                }

                counter++;
            }

            return counter;
        }

        #region CompanySettings

        public void CopyAllSettings(TemplateCompanyDataItem item)
        {
            using (CompEntities entities = new CompEntities())
            {
                var existingSettings = SettingManager.GetUserCompanySettingsForCompany(entities, item.DestinationActorCompanyId);

                CopyBaseAccountSettingsFromTemplateCompany(entities, item, existingSettings);

                if (item.InputDTO.DoCopy(TemplateCompanyCopy.ManageSettings))
                    CopyManageSettingsFromTemplateCompany(entities, item, existingSettings);

                if (item.InputDTO.DoCopy(TemplateCompanyCopy.AccountingSettings))
                {
                    CopyAccountingVoucherSettingsFromTemplateCompany(entities, item, existingSettings);
                    CopyBaseAccountSettingsFromTemplateCompany(entities, item, existingSettings);
                }

                if (item.InputDTO.DoCopy(TemplateCompanyCopy.SupplierSettings))
                    CopySupplierSettingsFromTemplateCompany(entities, item, existingSettings);

                if (item.InputDTO.DoCopy(TemplateCompanyCopy.CustomerSettings))
                    CopyCustomerSettingsFromTemplateCompany(entities, item, existingSettings);

                if (item.InputDTO.DoCopy(TemplateCompanyCopy.BillingSettings))
                    CopyBillingSettingsFromTemplateCompany(entities, item, existingSettings);

                if (item.InputDTO.DoCopy(TemplateCompanyCopy.TimeSettings))
                    CopyTimeSettingsFromTemplateCompany(entities, item, existingSettings);

                if (item.InputDTO.DoCopy(TemplateCompanyCopy.PayrollSettings))
                    CopyPayrollSettingsFromTemplateCompany(entities, item, existingSettings);

                if (item.InputDTO.DoCopy(TemplateCompanyCopy.ProjectSettings))
                    CopyProjectSettingsFromTemplateCompany(entities, item, existingSettings);

            }
        }

        public List<CompanySettingCopyItem> GetCompanySettings(TemplateCompanyDataItem item, CompanySettingTypeGroup companySettingType)
        {
            return item.TemplateCompanyCoreDataItem.CompanySettingCopyItems.Where(w => w.ActorCompanyId.HasValue && SettingManager.GetCompanySettingTypesForGroup((int)companySettingType).Contains(w.SettingTypeId)).ToList();
        }

        private void SetValueOnUserCompanySetting(UserCompanySetting setting, CompanySettingCopyItem companySettingCopyItem)
        {
            setting.BoolData = companySettingCopyItem.BoolData;
            setting.IntData = companySettingCopyItem.IntData;
            setting.StrData = companySettingCopyItem.StrData;
            setting.DateData = companySettingCopyItem.DateData;
            setting.DecimalData = companySettingCopyItem.DecimalData;
        }

        public TemplateResult CopyManageSettingsFromTemplateCompany(CompEntities entities, TemplateCompanyDataItem item, List<UserCompanySetting> existingCompanySettingCopyItems)
        {
            TemplateResult templateResult = new TemplateResult();

            List<CompanySettingType> specialHandlingSettingTypes = new List<CompanySettingType>()
            {
                CompanySettingType.DefaultRole,
                CompanySettingType.CompanyAPIKey,
                CompanySettingType.DefaultEmployeeAccountDimEmployee,
                CompanySettingType.DefaultEmployeeAccountDimSelector,
            };

            foreach (var setting in GetCompanySettings(item, CompanySettingTypeGroup.Manage))
            {
                var currentSetting = existingCompanySettingCopyItems.FirstOrDefault(w => w.SettingTypeId == (int)setting.SettingTypeId);

                if (currentSetting == null)
                {
                    currentSetting = new UserCompanySetting()
                    {
                        ActorCompanyId = item.DestinationActorCompanyId,
                        SettingTypeId = (int)setting.SettingTypeId,
                        DataTypeId = (int)setting.DataTypeId
                    };
                    entities.UserCompanySetting.AddObject(currentSetting);
                }

                if (!specialHandlingSettingTypes.Contains(setting.SettingTypeId))
                    SetValueOnUserCompanySetting(currentSetting, setting);

                switch (setting.SettingTypeId)
                {
                    case CompanySettingType.DefaultRole:
                        var defaultRoleId = item.TemplateCompanyCoreDataItem.GetRole(setting.IntData ?? 0)?.RoleId;
                        if (defaultRoleId.HasValue)
                            currentSetting.IntData = defaultRoleId.Value;
                        break;
                    case CompanySettingType.DefaultEmployeeAccountDimEmployee:
                    case CompanySettingType.DefaultEmployeeAccountDimSelector:
                        var dimId = item.TemplateCompanyEconomyDataItem.GetAccountDim(setting.IntData ?? 0)?.AccountDimId;
                        if (dimId.HasValue)
                            currentSetting.IntData = dimId.Value;
                        break;
                    default:
                        break;
                }
            }

            templateResult.ActionResults.Add(SaveChanges(entities));
            return templateResult;
        }

        public TemplateResult CopyAccountingVoucherSettingsFromTemplateCompany(CompEntities entities, TemplateCompanyDataItem item, List<UserCompanySetting> existingCompanySettingCopyItems)
        {
            TemplateResult templateResult = new TemplateResult();

            List<CompanySettingType> specialHandlingSettingTypes = new List<CompanySettingType>()
            {
                CompanySettingType.AccountingVoucherSeriesTypeManual,
                CompanySettingType.AccountingVoucherSeriesTypeVat,
                CompanySettingType.StockDefaultVoucherSeriesType,
                CompanySettingType.PayrollAccountExportVoucherSeriesType,
                CompanySettingType.AccountdistributionVoucherSeriesType,
                CompanySettingType.AccountingDefaultAccountingOrder,
                CompanySettingType.AccountingDefaultVoucherList,
                CompanySettingType.AccountingDefaultAnalysisReport
            };

            var companySettings = GetCompanySettings(item, CompanySettingTypeGroup.Accounting);
            companySettings
                .AddRange(
                    GetCompanySettings(item, CompanySettingTypeGroup.Inventory).Where(s=> s.SettingTypeId == CompanySettingType.StockDefaultVoucherSeriesType).ToArray()
                );
            companySettings
                .AddRange(
                    GetCompanySettings(item, CompanySettingTypeGroup.BaseAccountsEmployeeGroup).Where(s => s.SettingTypeId == CompanySettingType.PayrollAccountExportVoucherSeriesType).ToArray()
                 );

            foreach (var setting in companySettings)
            {
                var currentSetting = existingCompanySettingCopyItems.FirstOrDefault(w => w.SettingTypeId == (int)setting.SettingTypeId);

                if (currentSetting == null)
                {
                    currentSetting = new UserCompanySetting()
                    {
                        ActorCompanyId = item.DestinationActorCompanyId,
                        SettingTypeId = (int)setting.SettingTypeId,
                        DataTypeId = (int)setting.DataTypeId
                    };
                    entities.UserCompanySetting.AddObject(currentSetting);
                }

                if (!specialHandlingSettingTypes.Contains(setting.SettingTypeId))
                    SetValueOnUserCompanySetting(currentSetting, setting);

                switch (setting.SettingTypeId)
                {
                    case CompanySettingType.AccountingVoucherSeriesTypeManual:
                    case CompanySettingType.AccountingVoucherSeriesTypeVat:
                    case CompanySettingType.StockDefaultVoucherSeriesType:
                    case CompanySettingType.PayrollAccountExportVoucherSeriesType:
                    case CompanySettingType.AccountdistributionVoucherSeriesType:
                        if (item.InputDTO.DoCopy(TemplateCompanyCopy.VoucherSeriesTypes))
                        {
                            var voucherSeriesId = item.TemplateCompanyEconomyDataItem.GetVoucherSeriesType(setting.IntData ?? 0)?.VoucherSeriesTypeId;
                            if (voucherSeriesId.HasValue)
                                currentSetting.IntData = voucherSeriesId.Value;
                        }
                        break;
                    case CompanySettingType.AccountingDefaultAccountingOrder:
                    case CompanySettingType.AccountingDefaultVoucherList:
                    case CompanySettingType.AccountingDefaultAnalysisReport:
                        var reportId = item.TemplateCompanyCoreDataItem.GetReport(setting.IntData ?? 0)?.ReportId;
                        if (reportId.HasValue)
                            currentSetting.IntData = reportId.Value;
                        break;
                    default:
                        break;
                }
            }

            templateResult.ActionResults.Add(SaveChanges(entities));
            return templateResult;
        }

        public TemplateResult CopyBaseAccountSettingsFromTemplateCompany(CompEntities entities, TemplateCompanyDataItem item, List<UserCompanySetting> existingCompanySettingCopyItems)
        {
            TemplateResult templateResult = new TemplateResult();

            if (item.InputDTO.DoCopy(TemplateCompanyCopy.AccountingSettings))
            {
                foreach (var setting in GetCompanySettings(item, CompanySettingTypeGroup.BaseAccountsCommon))
                    SetAccountIdInSetting(entities, item, existingCompanySettingCopyItems.FirstOrDefault(w => w.SettingTypeId == (int)setting.SettingTypeId), setting);

                foreach (var setting in GetCompanySettings(item, CompanySettingTypeGroup.BaseAccountsInventory))
                    SetAccountIdInSetting(entities, item, existingCompanySettingCopyItems.FirstOrDefault(w => w.SettingTypeId == (int)setting.SettingTypeId), setting);
            }
            if (item.InputDTO.DoCopy(TemplateCompanyCopy.SupplierSettings))
                foreach (var setting in GetCompanySettings(item, CompanySettingTypeGroup.BaseAccountsSupplier))
                    SetAccountIdInSetting(entities, item, existingCompanySettingCopyItems.FirstOrDefault(w => w.SettingTypeId == (int)setting.SettingTypeId), setting);

            if (item.InputDTO.DoCopy(TemplateCompanyCopy.TimeSettings))
            {
                foreach (var setting in GetCompanySettings(item, CompanySettingTypeGroup.BaseAccountsEmployee))
                    SetAccountIdInSetting(entities, item, existingCompanySettingCopyItems.FirstOrDefault(w => w.SettingTypeId == (int)setting.SettingTypeId), setting);

                foreach (var setting in GetCompanySettings(item, CompanySettingTypeGroup.BaseAccountsEmployeeGroup))
                    SetAccountIdInSetting(entities, item, existingCompanySettingCopyItems.FirstOrDefault(w => w.SettingTypeId == (int)setting.SettingTypeId), setting);
            }

            if (item.InputDTO.DoCopy(TemplateCompanyCopy.CustomerSettings))
                foreach (var setting in GetCompanySettings(item, CompanySettingTypeGroup.BaseAccountsCustomer))
                    SetAccountIdInSetting(entities, item, existingCompanySettingCopyItems.FirstOrDefault(w => w.SettingTypeId == (int)setting.SettingTypeId), setting);

            if (item.InputDTO.DoCopy(TemplateCompanyCopy.BillingSettings))
                foreach (var setting in GetCompanySettings(item, CompanySettingTypeGroup.BaseAccountsInvoiceProduct))
                    SetAccountIdInSetting(entities, item, existingCompanySettingCopyItems.FirstOrDefault(w => w.SettingTypeId == (int)setting.SettingTypeId), setting);

            templateResult.ActionResults.Add(SaveChanges(entities));
            return templateResult;
        }

        private void SetAccountIdInSetting(CompEntities entities, TemplateCompanyDataItem item, UserCompanySetting currentSetting, CompanySettingCopyItem companySettingCopyItem)
        {
            if (currentSetting == null)
            {
                currentSetting = new UserCompanySetting()
                {
                    ActorCompanyId = item.DestinationActorCompanyId,
                    SettingTypeId = (int)companySettingCopyItem.SettingTypeId,
                    DataTypeId = (int)companySettingCopyItem.DataTypeId
                };
                entities.UserCompanySetting.AddObject(currentSetting);

                var accountId = item.TemplateCompanyEconomyDataItem.GetAccount(companySettingCopyItem.IntData ?? 0)?.AccountId;
                if (accountId.HasValue)
                    currentSetting.IntData = accountId.Value;
            }
        }

        public TemplateResult CopySupplierSettingsFromTemplateCompany(CompEntities entities, TemplateCompanyDataItem item, List<UserCompanySetting> existingCompanySettingCopyItems)
        {
            TemplateResult templateResult = new TemplateResult();

            List<CompanySettingType> specialHandlingSettingTypes = new List<CompanySettingType>()
            {
                CompanySettingType.SupplierInvoiceVoucherSeriesType,
                CompanySettingType.SupplierPaymentVoucherSeriesType,
                CompanySettingType.SupplierPaymentDefaultPaymentCondition,
                CompanySettingType.SupplierDefaultBalanceList,
                CompanySettingType.SupplierDefaultPaymentSuggestionList,
                CompanySettingType.SupplierDefaultChecklistPayments,
            };

            foreach (var setting in GetCompanySettings(item, CompanySettingTypeGroup.Supplier))
            {
                var currentSetting = existingCompanySettingCopyItems.FirstOrDefault(w => w.SettingTypeId == (int)setting.SettingTypeId);

                if (currentSetting == null)
                {
                    currentSetting = new UserCompanySetting()
                    {
                        ActorCompanyId = item.DestinationActorCompanyId,
                        SettingTypeId = (int)setting.SettingTypeId,
                        DataTypeId = (int)setting.DataTypeId
                    };
                    entities.UserCompanySetting.AddObject(currentSetting);
                }

                if (!specialHandlingSettingTypes.Contains(setting.SettingTypeId))
                    SetValueOnUserCompanySetting(currentSetting, setting);

                switch (setting.SettingTypeId)
                {
                    case CompanySettingType.SupplierInvoiceVoucherSeriesType:
                    case CompanySettingType.SupplierPaymentVoucherSeriesType:
                        var voucherSeriesTypeId = item.TemplateCompanyEconomyDataItem.GetVoucherSeriesType(setting.IntData ?? 0)?.VoucherSeriesTypeId;
                        if (voucherSeriesTypeId.HasValue)
                            currentSetting.IntData = voucherSeriesTypeId.Value;
                        break;
                    case CompanySettingType.SupplierPaymentDefaultPaymentCondition:
                        var paymentConditionId = item.TemplateCompanyEconomyDataItem.GetPaymentCondition(setting.IntData ?? 0)?.PaymentConditionId;
                        if (paymentConditionId.HasValue)
                            currentSetting.IntData = paymentConditionId.Value;
                        break;
                    case CompanySettingType.SupplierDefaultBalanceList:
                    case CompanySettingType.SupplierDefaultPaymentSuggestionList:
                    case CompanySettingType.SupplierDefaultChecklistPayments:
                        var reportId = item.TemplateCompanyCoreDataItem.GetReport(setting.IntData ?? 0)?.ReportId;
                        if (reportId.HasValue)
                            currentSetting.IntData = reportId.Value;
                        break;
                    default:
                        break;
                }
            }

            templateResult.ActionResults.Add(SaveChanges(entities));
            return templateResult;
        }

        public TemplateResult CopyCustomerSettingsFromTemplateCompany(CompEntities entities, TemplateCompanyDataItem item, List<UserCompanySetting> existingCompanySettingCopyItems)
        {
            TemplateResult templateResult = new TemplateResult();

            List<CompanySettingType> specialHandlingSettingTypes = new List<CompanySettingType>()
            {
                CompanySettingType.CustomerInvoiceVoucherSeriesType,
                CompanySettingType.CustomerPaymentVoucherSeriesType,
                CompanySettingType.CustomerPaymentDefaultPaymentCondition,
                CompanySettingType.CustomerDefaultPaymentConditionClaimAndInterest,
                CompanySettingType.CustomerPaymentDefaultPaymentConditionHouseholdDeduction,
                CompanySettingType.CustomerInvoiceTemplate,
                CompanySettingType.CustomerDefaultBalanceList,
                CompanySettingType.CustomerDefaultReminderTemplate,
                CompanySettingType.CustomerDefaultInterestTemplate,
                CompanySettingType.CustomerDefaultInterestRateCalculationTemplate,
            };

            foreach (var setting in GetCompanySettings(item, CompanySettingTypeGroup.Customer))
            {
                var currentSetting = existingCompanySettingCopyItems.FirstOrDefault(w => w.SettingTypeId == (int)setting.SettingTypeId);

                if (currentSetting == null)
                {
                    currentSetting = new UserCompanySetting()
                    {
                        ActorCompanyId = item.DestinationActorCompanyId,
                        SettingTypeId = (int)setting.SettingTypeId,
                        DataTypeId = (int)setting.DataTypeId
                    };
                    entities.UserCompanySetting.AddObject(currentSetting);
                }

                if (!specialHandlingSettingTypes.Contains(setting.SettingTypeId))
                    SetValueOnUserCompanySetting(currentSetting, setting);

                switch (setting.SettingTypeId)
                {
                    case CompanySettingType.CustomerInvoiceVoucherSeriesType:
                    case CompanySettingType.CustomerPaymentVoucherSeriesType:
                        var voucherSeriesTypeId = item.TemplateCompanyEconomyDataItem.GetVoucherSeriesType(setting.IntData ?? 0)?.VoucherSeriesTypeId;
                        if (voucherSeriesTypeId.HasValue)
                            currentSetting.IntData = voucherSeriesTypeId.Value;
                        break;
                    case CompanySettingType.CustomerPaymentDefaultPaymentCondition:
                    case CompanySettingType.CustomerDefaultPaymentConditionClaimAndInterest:
                    case CompanySettingType.CustomerPaymentDefaultPaymentConditionHouseholdDeduction:
                        var paymentConditionId = item.TemplateCompanyEconomyDataItem.GetPaymentCondition(setting.IntData ?? 0)?.PaymentConditionId;
                        if (paymentConditionId.HasValue)
                            currentSetting.IntData = paymentConditionId.Value;
                        break;
                    case CompanySettingType.CustomerDefaultBalanceList:
                    case CompanySettingType.CustomerDefaultReminderTemplate:
                    case CompanySettingType.CustomerDefaultInterestTemplate:
                    case CompanySettingType.CustomerDefaultInterestRateCalculationTemplate:
                        var reportId = item.TemplateCompanyCoreDataItem.GetReport(setting.IntData ?? 0)?.ReportId;
                        if (reportId.HasValue)
                            currentSetting.IntData = reportId.Value;
                        break;
                    default:
                        break;
                }
            }

            templateResult.ActionResults.Add(SaveChanges(entities));
            return templateResult;
        }

        public TemplateResult CopyBillingSettingsFromTemplateCompany(CompEntities entities, TemplateCompanyDataItem item, List<UserCompanySetting> existingCompanySettingCopyItems)
        {
            TemplateResult templateResult = new TemplateResult();

            List<CompanySettingType> specialHandlingSettingTypes = new List<CompanySettingType>()
            {
                //Invoice
                CompanySettingType.BillingDefaultPriceListType,
                CompanySettingType.BillingDefaultDeliveryType,
                CompanySettingType.BillingDefaultDeliveryCondition,
                CompanySettingType.BillingDefaultInvoiceProductUnit,
                CompanySettingType.BillingDefaultWholeseller, 
  
                //Reports
                CompanySettingType.BillingDefaultInvoiceTemplate,
                CompanySettingType.CustomerDefaultReminderTemplate,
                CompanySettingType.BillingDefaultTimeProjectReportTemplate,
                CompanySettingType.BillingDefaultOrderTemplate,
                CompanySettingType.BillingDefaultWorkingOrderTemplate,
                CompanySettingType.BillingDefaultOfferTemplate,
                CompanySettingType.BillingDefaultContractTemplate,
                CompanySettingType.BillingDefaultExpenseReportTemplate,
                CompanySettingType.BillingDefaultHouseholdDeductionTemplate,

                //Offer and Order status
                CompanySettingType.BillingStatusTransferredOfferToOrder,
                CompanySettingType.BillingStatusTransferredOfferToInvoice,
                CompanySettingType.BillingStatusTransferredOrderToInvoice,
                CompanySettingType.BillingStatusTransferredOrderToContract,
                CompanySettingType.BillingStatusOrderDeliverFromStock,
                CompanySettingType.BillingStatusOrderReadyMobile, 
              
                //Other
                CompanySettingType.BillingDefaultVatCode,
                CompanySettingType.BillingStandardMaterialCode,

                //email template
                CompanySettingType.BillingDefaultEmailTemplate,
                CompanySettingType.BillingOfferDefaultEmailTemplate,
                CompanySettingType.BillingOrderDefaultEmailTemplate,
                CompanySettingType.BillingContractDefaultEmailTemplate,
                CompanySettingType.BillingDefaultEmailTemplateCashSales,
                CompanySettingType.BillingDefaultEmailTemplatePurchase,
                
                //project setting
                CompanySettingType.ProjectDefaultTimeCodeId
            };

            foreach (var setting in GetCompanySettings(item, CompanySettingTypeGroup.Billing))
            {
                var currentSetting = existingCompanySettingCopyItems.FirstOrDefault(w => w.SettingTypeId == (int)setting.SettingTypeId);

                if (currentSetting == null)
                {
                    currentSetting = new UserCompanySetting()
                    {
                        ActorCompanyId = item.DestinationActorCompanyId,
                        SettingTypeId = (int)setting.SettingTypeId,
                        DataTypeId = (int)setting.DataTypeId
                    };
                    entities.UserCompanySetting.AddObject(currentSetting);
                }

                if (!specialHandlingSettingTypes.Contains(setting.SettingTypeId))
                    SetValueOnUserCompanySetting(currentSetting, setting);

                switch (setting.SettingTypeId)
                {
                    case CompanySettingType.BillingDefaultPriceListType:
                        var priceListTypeId = item.TemplateCompanyBillingDataItem.GetPriceList(setting.IntData ?? 0)?.PriceListTypeId;
                        if (priceListTypeId.HasValue)
                            currentSetting.IntData = priceListTypeId.Value;
                        break;
                    case CompanySettingType.BillingDefaultDeliveryType:
                        var deliveryTypeId = item.TemplateCompanyBillingDataItem.GetDeliveryType(setting.IntData ?? 0)?.DeliveryTypeId;
                        if (deliveryTypeId.HasValue)
                            currentSetting.IntData = deliveryTypeId.Value;
                        break;
                    case CompanySettingType.BillingDefaultDeliveryCondition:
                        var deliveryConditionId = item.TemplateCompanyBillingDataItem.GetDeliveryCondition(setting.IntData ?? 0)?.DeliveryConditionId;
                        if (deliveryConditionId.HasValue)
                            currentSetting.IntData = deliveryConditionId.Value;
                        break;
                    case CompanySettingType.BillingDefaultInvoiceProductUnit:
                        var productUnitId = item.TemplateCompanyBillingDataItem.GetProductUnit(setting.IntData ?? 0)?.ProductUnitId;
                        if (productUnitId.HasValue)
                            currentSetting.IntData = productUnitId.Value;
                        break;
                    case CompanySettingType.BillingDefaultWholeseller:
                        currentSetting.IntData = setting.IntData;
                        break;
                    case CompanySettingType.BillingDefaultVatCode:
                        var vatCodeId = item.TemplateCompanyEconomyDataItem.GetVatCode(setting.IntData ?? 0)?.VatCodeId;
                        if (vatCodeId.HasValue)
                            currentSetting.IntData = vatCodeId.Value;
                        break;
                    case CompanySettingType.BillingStandardMaterialCode:
                        var materialCodeId = item.TemplateCompanyTimeDataItem.GetTimeCode(setting.IntData ?? 0)?.TimeCodeId;
                        if (materialCodeId.HasValue)
                            currentSetting.IntData = materialCodeId.Value;
                        break;
                    case CompanySettingType.BillingStatusTransferredOfferToOrder:
                    case CompanySettingType.BillingStatusTransferredOfferToInvoice:
                    case CompanySettingType.BillingStatusTransferredOrderToInvoice:
                    case CompanySettingType.BillingStatusOrderReadyMobile:
                    case CompanySettingType.BillingStatusTransferredOrderToContract:
                    case CompanySettingType.BillingStatusOrderDeliverFromStock:
                        var attestId = item.TemplateCompanyAttestDataItem.GetAttestState(setting.IntData ?? 0)?.AttestStateId;
                        if (attestId.HasValue)
                            currentSetting.IntData = attestId.Value;
                        break;
                    case CompanySettingType.BillingDefaultInvoiceTemplate:
                    case CompanySettingType.CustomerDefaultReminderTemplate:
                    case CompanySettingType.BillingDefaultTimeProjectReportTemplate:
                    case CompanySettingType.BillingDefaultOrderTemplate:
                    case CompanySettingType.BillingDefaultWorkingOrderTemplate:
                    case CompanySettingType.BillingDefaultOfferTemplate:
                    case CompanySettingType.BillingDefaultContractTemplate:
                    case CompanySettingType.BillingDefaultExpenseReportTemplate:
                    case CompanySettingType.BillingDefaultHouseholdDeductionTemplate:
                        var reportId = item.TemplateCompanyCoreDataItem.GetReport(setting.IntData ?? 0)?.ReportId;
                        if (reportId.HasValue)
                            currentSetting.IntData = reportId.Value;
                        break;
                    case CompanySettingType.BillingDefaultEmailTemplate:
                    case CompanySettingType.BillingOfferDefaultEmailTemplate:
                    case CompanySettingType.BillingOrderDefaultEmailTemplate:
                    case CompanySettingType.BillingContractDefaultEmailTemplate:
                    case CompanySettingType.BillingDefaultEmailTemplateCashSales:
                    case CompanySettingType.BillingDefaultEmailTemplatePurchase:
                        var templateId = item.TemplateCompanyBillingDataItem.GetEmailTemplate(setting.IntData ?? 0)?.EmailTemplateId;
                        if (templateId.HasValue)
                            currentSetting.IntData = templateId.Value;
                        break;
                    default:
                        break;
                }
            }

            templateResult.ActionResults.Add(SaveChanges(entities));
            return templateResult;
        }

        public TemplateResult CopyTimeSettingsFromTemplateCompany(CompEntities entities, TemplateCompanyDataItem item, List<UserCompanySetting> existingCompanySettingCopyItems)
        {
            TemplateResult templateResult = new TemplateResult();

            List<CompanySettingType> specialHandlingSettingTypes = new List<CompanySettingType>()
            {
                //Time
                CompanySettingType.TimeDefaultTimeCode,
                CompanySettingType.TimeDefaultEmployeeGroup,
                CompanySettingType.TimeDefaultPayrollGroup,
                CompanySettingType.TimeDefaultVacationGroup,
                CompanySettingType.TimeDefaultTimePeriodHead,
                CompanySettingType.TimeDefaultTimeDeviationCause,

                //SalaryExport
                CompanySettingType.SalaryExportPayrollMinimumAttestStatus,
                CompanySettingType.SalaryExportPayrollResultingAttestStatus,
                CompanySettingType.SalaryExportInvoiceMinimumAttestStatus,
                CompanySettingType.SalaryExportInvoiceResultingAttestStatus,
                CompanySettingType.SalaryExportExternalExportID, //Not relevant

                //SalaryPayment
                CompanySettingType.SalaryPaymentLockedAttestStateId,
                CompanySettingType.SalaryPaymentApproved1AttestStateId,
                CompanySettingType.SalaryPaymentApproved2AttestStateId,
                CompanySettingType.SalaryPaymentExportFileCreatedAttestStateId, 

                //Attest 
                CompanySettingType.TimeAutoAttestSourceAttestStateId,
                CompanySettingType.TimeAutoAttestTargetAttestStateId,
                CompanySettingType.MobileTimeAttestResultingAttestStatus,

                //Payroll
                CompanySettingType.PayrollAccountingDistributionPayrollProduct, 

                //Staffing
                CompanySettingType.TimeStaffingShiftAccountDimId,

                //Not relevant
                CompanySettingType.PayrollExportForaAgreementNumber,
                CompanySettingType.PayrollExportITP1Number,
                CompanySettingType.PayrollExportITP2Number,
                CompanySettingType.PayrollExportKPAAgreementNumber,
                CompanySettingType.PayrollExportSNKFOMemberNumber,
                CompanySettingType.PayrollExportSNKFOWorkPlaceNumber,
                CompanySettingType.PayrollExportSNKFOAffiliateNumber,
                CompanySettingType.PayrollExportSNKFOAgreementNumber,
                CompanySettingType.PayrollExportCommunityCode,
                CompanySettingType.PayrollExportSCBWorkSite,
                CompanySettingType.PayrollExportCFARNumber,
                CompanySettingType.PayrollArbetsgivarintygnuApiNyckel,
                CompanySettingType.PayrollArbetsgivarintygnuArbetsgivarId,
                CompanySettingType.PayrollExportKPAManagementNumber
            };

            foreach (var setting in GetCompanySettings(item, CompanySettingTypeGroup.Time))
            {
                var currentSetting = existingCompanySettingCopyItems.FirstOrDefault(w => w.SettingTypeId == (int)setting.SettingTypeId);

                if (currentSetting == null)
                {
                    currentSetting = new UserCompanySetting()
                    {
                        ActorCompanyId = item.DestinationActorCompanyId,
                        SettingTypeId = (int)setting.SettingTypeId,
                        DataTypeId = (int)setting.DataTypeId
                    };
                    entities.UserCompanySetting.AddObject(currentSetting);
                }

                if (!specialHandlingSettingTypes.Contains(setting.SettingTypeId))
                    SetValueOnUserCompanySetting(currentSetting, setting);

                switch (setting.SettingTypeId)
                {
                    case CompanySettingType.TimeDefaultTimeCode:
                        var timeCodeId = item.TemplateCompanyTimeDataItem.GetTimeCode(setting.IntData ?? 0)?.TimeCodeId;
                        if (timeCodeId.HasValue)
                            currentSetting.IntData = timeCodeId.Value;
                        break;
                    case CompanySettingType.TimeDefaultTimeDeviationCause:
                        var timeDeviationCauseId = item.TemplateCompanyTimeDataItem.GetTimeDeviationCause(setting.IntData ?? 0)?.TimeDeviationCauseId;
                        if (timeDeviationCauseId.HasValue)
                            currentSetting.IntData = timeDeviationCauseId.Value;
                        break;
                    case CompanySettingType.TimeDefaultEmployeeGroup:
                        var employeeGroupId = item.TemplateCompanyTimeDataItem.GetEmployeeGroup(setting.IntData ?? 0)?.EmployeeGroupId;
                        if (employeeGroupId.HasValue)
                            currentSetting.IntData = employeeGroupId.Value;
                        break;
                    case CompanySettingType.TimeDefaultPayrollGroup:
                        var payrollGroupId = item.TemplateCompanyTimeDataItem.GetPayrollGroup(setting.IntData ?? 0)?.PayrollGroupId;
                        if (payrollGroupId.HasValue)
                            currentSetting.IntData = payrollGroupId.Value;
                        break;
                    case CompanySettingType.TimeDefaultVacationGroup:
                        var vacationGroupId = item.TemplateCompanyTimeDataItem.GetVacationGroup(setting.IntData ?? 0)?.VacationGroupId;
                        if (vacationGroupId.HasValue)
                            currentSetting.IntData = vacationGroupId.Value;
                        break;
                    case CompanySettingType.TimeDefaultTimePeriodHead:
                        var timePeriodHeadId = item.TemplateCompanyTimeDataItem.GetTimePeriodHead(setting.IntData ?? 0)?.TimePeriodHeadId;
                        if (timePeriodHeadId.HasValue)
                            currentSetting.IntData = timePeriodHeadId.Value;
                        break;
                    case CompanySettingType.SalaryExportPayrollMinimumAttestStatus:
                    case CompanySettingType.SalaryExportPayrollResultingAttestStatus:
                    case CompanySettingType.SalaryExportInvoiceMinimumAttestStatus:
                    case CompanySettingType.SalaryExportInvoiceResultingAttestStatus:
                    case CompanySettingType.TimeAutoAttestSourceAttestStateId:
                    case CompanySettingType.TimeAutoAttestTargetAttestStateId:
                    case CompanySettingType.MobileTimeAttestResultingAttestStatus:
                        var attestStateId = item.TemplateCompanyAttestDataItem.GetAttestState(setting.IntData ?? 0)?.AttestStateId;
                        if (attestStateId.HasValue)
                            currentSetting.IntData = attestStateId.Value;
                        break;
                    case CompanySettingType.PayrollAccountingDistributionPayrollProduct:
                        var payrollProductId = item.TemplateCompanyTimeDataItem.GetPayrollProduct(setting.IntData ?? 0)?.ProductId;
                        if (payrollProductId.HasValue)
                            currentSetting.IntData = payrollProductId.Value;
                        break;
                    case CompanySettingType.TimeStaffingShiftAccountDimId:
                        var accountDimId = item.TemplateCompanyEconomyDataItem.GetAccountDim(setting.IntData ?? 0)?.AccountDimId;
                        if (accountDimId.HasValue)
                            currentSetting.IntData = accountDimId.Value;
                        break;
                    default:
                        break;
                }
            }

            templateResult.ActionResults.Add(SaveChanges(entities));
            return templateResult;
        }

        public TemplateResult CopyPayrollSettingsFromTemplateCompany(CompEntities entities, TemplateCompanyDataItem item, List<UserCompanySetting> existingCompanySettingCopyItems)
        {
            TemplateResult templateResult = new TemplateResult();

            List<CompanySettingType> specialHandlingSettingTypes = new List<CompanySettingType>()
            {
                CompanySettingType.SalaryPaymentApproved1AttestStateId,
                CompanySettingType.SalaryPaymentApproved2AttestStateId,
                CompanySettingType.SalaryPaymentExportFileCreatedAttestStateId,
                CompanySettingType.SalaryPaymentLockedAttestStateId,
                CompanySettingType.DefaultPayrollSlipReport,
                CompanySettingType.DefaultEmployeeVacationDebtReport,
                CompanySettingType.SalaryPaymentExportPaymentAccount,

                CompanySettingType.PayrollExportForaAgreementNumber,
                CompanySettingType.PayrollExportITP1Number,
                CompanySettingType.PayrollExportITP2Number,
                CompanySettingType.PayrollExportKPAAgreementNumber,
                CompanySettingType.PayrollExportSNKFOMemberNumber,
                CompanySettingType.PayrollExportSNKFOWorkPlaceNumber,
                CompanySettingType.PayrollExportSNKFOAffiliateNumber,
                CompanySettingType.PayrollExportSNKFOAgreementNumber,
                CompanySettingType.PayrollExportCommunityCode,
                CompanySettingType.PayrollExportSCBWorkSite,
                CompanySettingType.PayrollExportCFARNumber,
                CompanySettingType.PayrollArbetsgivarintygnuApiNyckel,
                CompanySettingType.PayrollArbetsgivarintygnuArbetsgivarId,
                CompanySettingType.PayrollExportKPAManagementNumber,
                CompanySettingType.PayrollExportFolksamCustomerNumber,
                CompanySettingType.PayrollExportPlaceOfEmploymentAddress,
                CompanySettingType.PayrollExportPlaceOfEmploymentCity,
                CompanySettingType.PayrollExportSkandiaSortingConcept,
            };

            foreach (var setting in GetCompanySettings(item, CompanySettingTypeGroup.Payroll))
            {
                var currentSetting = existingCompanySettingCopyItems.FirstOrDefault(w => w.SettingTypeId == (int)setting.SettingTypeId);

                if (currentSetting == null)
                {
                    currentSetting = new UserCompanySetting()
                    {
                        ActorCompanyId = item.DestinationActorCompanyId,
                        SettingTypeId = (int)setting.SettingTypeId,
                        DataTypeId = (int)setting.DataTypeId
                    };
                    entities.UserCompanySetting.AddObject(currentSetting);
                }

                if (!specialHandlingSettingTypes.Contains(setting.SettingTypeId))
                    SetValueOnUserCompanySetting(currentSetting, setting);

                switch (setting.SettingTypeId)
                {
                    case CompanySettingType.SalaryPaymentApproved1AttestStateId:
                    case CompanySettingType.SalaryPaymentApproved2AttestStateId:
                    case CompanySettingType.SalaryPaymentExportFileCreatedAttestStateId:
                    case CompanySettingType.SalaryPaymentLockedAttestStateId:
                        var attestStateId = item.TemplateCompanyAttestDataItem.GetAttestState(setting.IntData ?? 0)?.AttestStateId;
                        if (attestStateId.HasValue)
                            currentSetting.IntData = attestStateId.Value;
                        break;
                    case CompanySettingType.DefaultPayrollSlipReport:
                    case CompanySettingType.DefaultEmployeeVacationDebtReport:
                        var reportId = item.TemplateCompanyCoreDataItem.GetReport(setting.IntData ?? 0)?.ReportId;
                        if (reportId.HasValue)
                            currentSetting.IntData = reportId.Value;
                        break;
                    case CompanySettingType.SalaryPaymentExportPaymentAccount:
                        var paymentAccountId = item.TemplateCompanyEconomyDataItem.GetAccount(setting.IntData ?? 0)?.AccountId;
                        if (paymentAccountId.HasValue)
                            currentSetting.IntData = paymentAccountId.Value;
                        break;
                    default:
                        break;
                }
            }

            templateResult.ActionResults.Add(SaveChanges(entities));
            return templateResult;
        }

        public TemplateResult CopyProjectSettingsFromTemplateCompany(CompEntities entities, TemplateCompanyDataItem item, List<UserCompanySetting> existingCompanySettingCopyItems)
        {
            TemplateResult templateResult = new TemplateResult();

            List<CompanySettingType> specialHandlingSettingTypes = new List<CompanySettingType>()
            {
                    CompanySettingType.ProjectIncludeTimeProjectReport,
                    CompanySettingType.ProjectDefaultTimeCodeId,
            };

            foreach (var setting in GetCompanySettings(item, CompanySettingTypeGroup.Project))
            {
                var currentSetting = existingCompanySettingCopyItems.FirstOrDefault(w => w.SettingTypeId == (int)setting.SettingTypeId);

                if (currentSetting == null)
                {
                    currentSetting = new UserCompanySetting()
                    {
                        ActorCompanyId = item.DestinationActorCompanyId,
                        SettingTypeId = (int)setting.SettingTypeId,
                        DataTypeId = (int)setting.DataTypeId
                    };
                    entities.UserCompanySetting.AddObject(currentSetting);
                }

                if (!specialHandlingSettingTypes.Contains(setting.SettingTypeId))
                    SetValueOnUserCompanySetting(currentSetting, setting);

                switch (setting.SettingTypeId)
                {
                    case CompanySettingType.ProjectIncludeTimeProjectReport:
                        var reportId = item.TemplateCompanyCoreDataItem.GetReport(setting.IntData ?? 0)?.ReportId;
                        if (reportId.HasValue)
                            currentSetting.IntData = reportId.Value;
                        break;
                    case CompanySettingType.ProjectDefaultTimeCodeId:
                        var timeCodeId = item.TemplateCompanyBillingDataItem.GetTimeCode(setting.IntData ?? 0)?.TimeCodeId;
                        if (timeCodeId.HasValue)
                            currentSetting.IntData = timeCodeId.Value;
                        break;
                    default:
                        break;
                }
            }

            templateResult.ActionResults.Add(SaveChanges(entities));
            return templateResult;
        }


        #endregion

        #region User


        public TemplateResult CopyUsersFromTemplateCompany(TemplateCompanyDataItem item)
        {
            TemplateResult templateResult = new TemplateResult();

            using (CompEntities entities = new CompEntities())
            {
                var license = entities.License.FirstOrDefault();

                if (license == null)
                    return templateResult;

                var roles = entities.Role.Where(w => w.Company.LicenseId == license.LicenseId).ToList();

                var actorCompanyId = entities.Company.FirstOrDefault()?.ActorCompanyId;

                foreach (var userCopyItem in item.TemplateCompanyCoreDataItem.UserCopyItems)
                {
                    var user = new User() { License = license };

                    user.LoginName = userCopyItem.LoginName;
                    user.idLoginGuid = userCopyItem.IdLoginGuid;
                    user.Name = userCopyItem.Name;
                    user.EstatusLoginId = userCopyItem.EstatusLoginId;
                    user.DefaultActorCompanyId = actorCompanyId;
                    user.passwordhash = Encoding.UTF8.GetBytes(PasswordUtil.GenerateRandomPassword(4, 2));
                    Role firstFromDb = null;
                    foreach (var role in userCopyItem.UserRoleCopyItems)
                    {
                        var mappedRole = item.TemplateCompanyCoreDataItem.GetRole(role.RoleCopyItem.RoleId);
                        var roleFromDb = roles.FirstOrDefault(w => w.RoleId == mappedRole.RoleId);

                        if (firstFromDb == null)
                            firstFromDb = roleFromDb;

                        var userCompanyRole = new UserCompanyRole()
                        {
                            User = user,
                            Role = roleFromDb,
                            DateFrom = role.DateFrom,
                            DateTo = role.DateTo
                        };
                    }

                    user.DefaultRoleId = firstFromDb?.RoleId ?? roles[0].RoleId;
                }


                ActionResult result = SaveChanges(entities);
                if (!result.Success)
                    result = companyTemplateManager.LogCopyError("User", item, saved: true);
                templateResult.ActionResults.Add(result);
            }

            return templateResult;
        }

        public List<UserCopyItem> GetUserCopyItems(int actorCompanyId)
        {

            List<UserCopyItem> userCopyItems = new List<UserCopyItem>();

            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            var userList = entitiesReadOnly.User.Include("UserCompanyRole").Where(w => w.UserCompanyRole.Any(a => a.ActorCompanyId == actorCompanyId) && w.State == (int)SoeEntityState.Active).ToList();

            foreach (var user in userList)
            {
                var company = entitiesReadOnly.Company.FirstOrDefault(w => w.ActorCompanyId == user.DefaultActorCompanyId);
                var role = entitiesReadOnly.Role.FirstOrDefault(w => w.RoleId == user.DefaultRoleId);

                UserCopyItem item = new UserCopyItem()
                {
                    IdLoginGuid = user.idLoginGuid ?? Guid.Empty,
                    Name = user.Name,
                    LoginName = user.LoginName,
                    EstatusLoginId = user.EstatusLoginId,
                    DefaultCompanyName = company?.Name ?? string.Empty,
                    DefaultRoleName = role?.Name ?? string.Empty,
                };

                foreach (var userCompanyRole in user.UserCompanyRole)
                {
                    {
                        var roleCopyItem = new UserRoleCopyItem()
                        {
                            DateFrom = userCompanyRole.DateFrom,
                            DateTo = userCompanyRole.DateTo,
                            RoleCopyItem = new RoleCopyItem()
                            {
                                RoleId = userCompanyRole.Role.RoleId,
                            }
                        };

                        item.UserRoleCopyItems.Add(roleCopyItem);
                    }

                    userCopyItems.Add(item);
                }
            }

            return userCopyItems;
        }

        public List<UserCopyItem> GetUserCopyItemsFromApi(int sysCompDbId, int actorCompanyId)
        {
            if (sysCompDbId == ConfigurationSetupUtil.GetCurrentSysCompDbId())
                return GetUserCopyItems(actorCompanyId);

            return coreTemplateConnector.GetUserCopyItems(sysCompDbId, actorCompanyId);
        }

        #endregion
    }


}

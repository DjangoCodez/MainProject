using System.Web.Http;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System.Collections.Generic;
using Soe.WebApi.Models;
using Soe.WebApi.Controllers;
using System.Net.Http;
using Soe.WebApi.Extensions;
using System.Net;

namespace Soe.WebApi.V2.Report
{
    [RoutePrefix("V2/Report")]
    public class ReportV2Controller : SoeApiController
    {
        #region Variables

        private readonly ReportManager rm;
        private readonly ReportDataManager rdm;
        private readonly VoucherManager vm;
        private readonly ProjectManager pm;
        private readonly ImportExportManager iem;

        #endregion

        #region Constructor

        public ReportV2Controller(ReportManager rm, ReportDataManager rdm, VoucherManager vm, ProjectManager pm, ImportExportManager iem)
        {
            this.rm = rm;
            this.rdm = rdm;
            this.vm = vm;
            this.pm = pm;
            this.iem = iem;
        }

        #endregion


        #region Reports

        //[HttpGet]
        //[Route("Reports/{sysReportTemplateTypeId:int}/{onlyOriginal:bool}/{onlyStandard:bool}/{addEmptyRow:bool}/{useRole:bool}")]
        //public IHttpActionResult GetReports(int sysReportTemplateTypeId, bool onlyOriginal, bool onlyStandard, bool addEmptyRow, bool useRole)
        //{
        //    return Content(HttpStatusCode.OK, rm.GetReportsByTemplateTypeDict(base.ActorCompanyId, (SoeReportTemplateType)sysReportTemplateTypeId, onlyOriginal, onlyStandard, addEmptyRow, useRole ? base.RoleId : (int?)null).ToSmallGenericTypes());
        //}

        [HttpGet]
        [Route("Reports/{sysReportTemplateTypeId:int}/{onlyOriginal:bool}/{onlyStandard:bool}/{addEmptyRow:bool}/{useRole:bool}")]
        public IHttpActionResult GetReportsDict(int sysReportTemplateTypeId, bool onlyOriginal, bool onlyStandard, bool addEmptyRow, bool useRole)
        {
            return Content(HttpStatusCode.OK, rm.GetReportsByTemplateTypeDict(base.ActorCompanyId, (SoeReportTemplateType)sysReportTemplateTypeId, onlyOriginal, onlyStandard, addEmptyRow, useRole ? base.RoleId : (int?)null).ToSmallGenericTypes());
        }

        [HttpGet]
        [Route("Reports/{actorCompanyId:int}/{sysReportTemplateTypeId:int}/{onlyOriginal:bool}/{onlyStandard:bool}/{addEmptyRow:bool}/{useRole:bool}")]
        public IHttpActionResult GetReports(int actorCompanyId, int sysReportTemplateTypeId, bool onlyOriginal, bool onlyStandard, bool addEmptyRow, bool useRole)
        {
            return Content(HttpStatusCode.OK, rm.GetReportsByTemplateTypeDict(actorCompanyId, (SoeReportTemplateType)sysReportTemplateTypeId, onlyOriginal, onlyStandard, addEmptyRow, useRole ? base.RoleId : (int?)null).ToSmallGenericTypes());
        }

        [HttpGet]
        [Route("Reports/{module:int}/{onlyOriginal:bool}/{onlyStandard:bool}")]
        public IHttpActionResult GetReportViewsForModule(int module, bool onlyOriginal, bool onlyStandard)
        {
            return Content(HttpStatusCode.OK, rm.GetReports(base.ActorCompanyId, base.RoleId, module: module, onlyOriginal: onlyOriginal, onlyStandard: onlyStandard, loadReportSelection: true).ToReportViewGridDTOs());
        }

        [HttpGet]
        [Route("Report/{reportId:int}/{loadReportSelection:bool}/{loadSysReportTemplateType:bool}/{loadReportRolePermission:bool}")]
        public IHttpActionResult GetReport(int reportId, bool loadReportSelection, bool loadSysReportTemplateType, bool loadReportRolePermission)
        {
            return Content(HttpStatusCode.OK, rm.GetReport(reportId, base.ActorCompanyId, false, loadReportSelection, loadReportRolePermission, loadSysReportTemplateType).ToDTO());
        }

        [HttpGet]
        [Route("Report/ExportTypes/{sysReportTemplateId:int}/{userReportTemplateId:int}/{sysReportType:int}")]
        public IHttpActionResult GetReportExportTypes(int sysReportTemplateId, int userReportTemplateId, SoeReportType sysReportType)
        {
            return Content(HttpStatusCode.OK, rm.GetReportExportTypes(sysReportTemplateId.ToNullable(), userReportTemplateId.ToNullable(), sysReportType));
        }

        [HttpPost]
        [Route("Report/Save/")]
        public IHttpActionResult SaveReport(ReportDTO reportDTO)
        {
            return Content(HttpStatusCode.OK, rm.SaveReport(reportDTO, base.ActorCompanyId));
        }

        [HttpDelete]
        [Route("Report/{reportId}")]
        public IHttpActionResult DeleteReport(int reportId)
        {
            return Content(HttpStatusCode.OK, rm.DeleteReport(reportId, base.ActorCompanyId));
        }

        [HttpGet]
        [Route("StandardReport/{settingMainType:int}/{settingType:int}/{reportTemplateType:int}")]
        public IHttpActionResult GetStandardReport(SettingMainType settingMainType, CompanySettingType settingType, SoeReportTemplateType reportTemplateType)
        {
            return Content(HttpStatusCode.OK, rm.GetCompanySettingReport(settingMainType, settingType, reportTemplateType, base.ActorCompanyId, base.UserId, base.RoleId).ToDTO());
        }

        [HttpGet]
        [Route("StandardReportId/{settingMainType:int}/{settingType:int}/{reportTemplateType:int}")]
        public IHttpActionResult GetCompanySettingReportId(SettingMainType settingMainType, CompanySettingType settingType, SoeReportTemplateType reportTemplateType)
        {
            return Content(HttpStatusCode.OK, rm.GetCompanySettingReportId(settingMainType, settingType, reportTemplateType, base.ActorCompanyId, base.UserId, base.RoleId));
        }

        [HttpGet]
        [Route("SettingOrStandardReportId/{settingMainType:int}/{settingType:int}/{reportTemplateType:int}/{reportType:int}")]
        public IHttpActionResult GetSettingOrStandardReport(SettingMainType settingMainType, CompanySettingType settingType, SoeReportTemplateType reportTemplateType, SoeReportType reportType)
        {
            var report = rm.GetSettingOrStandardReport(settingMainType, settingType, reportTemplateType, reportType, base.ActorCompanyId, base.UserId, base.RoleId);
            return Content(HttpStatusCode.OK, report != null ? report.ReportId : 0);
        }

        [HttpGet]
        [Route("SettingReportCheckPermission/{settingMainType:int}/{settingType:int}/{reportTemplateType:int}")]
        public IHttpActionResult SettingReportCheckPermission(SettingMainType settingMainType, CompanySettingType settingType, SoeReportTemplateType reportTemplateType)
        {
            return Content(HttpStatusCode.OK, rm.GetSettingReport(settingMainType, settingType, reportTemplateType, base.ActorCompanyId, base.UserId, base.RoleId).ToSmallDTO());
        }

        [HttpGet]
        [Route("SettingReportHasPermission/{settingMainType:int}/{settingType:int}/{reportTemplateType:int}")]
        public IHttpActionResult SettingReportHasPermission(SettingMainType settingMainType, CompanySettingType settingType, SoeReportTemplateType reportTemplateType)
        {
            return Content(HttpStatusCode.OK, rm.HasReportRolePermission(settingMainType, settingType, reportTemplateType, base.ActorCompanyId, base.UserId, base.RoleId));
        }

        [HttpGet]
        [Route("Report/Small/{reportId:int}")]
        public IHttpActionResult GetSmallDTOReportName(int reportId)
        {
            return Content(HttpStatusCode.OK, rm.GetReport(reportId, base.ActorCompanyId).ToSmallDTO());
        }
        [HttpGet]
        [Route("Report/{reportId:int}")]
        public IHttpActionResult GetReportName(int reportId)
        {
            return Content(HttpStatusCode.OK, rm.GetReport(reportId, base.ActorCompanyId).ToDTO());
        }

        [HttpGet]
        [Route("ReportsInPackage/{reportPackageId:int}")]
        public IHttpActionResult GetReportViewsInPackage(int reportPackageId)
        {
            return Content(HttpStatusCode.OK, rm.GetReportByPackage(base.ActorCompanyId, base.RoleId, reportPackageId).ToReportViewGridDTOs());
        }


        [HttpPost]
        [Route("ReportsForTypes/")]
        public IHttpActionResult GetReportsForTypes(GetReportsForTypesModel model)
        {
            return Content(HttpStatusCode.OK, rm.GetReports(base.ActorCompanyId, base.RoleId, sysReportTemplateTypeIds: model.ReportTemplateTypeIds, module: (int?)model.Module, onlyOriginal: model.OnlyOriginal, onlyStandard: model.OnlyStandard).ToReportViewDTOs());
        }

        [HttpPost]
        [Route("Project/Search/")]
        public IHttpActionResult GetProjectsBySearchNoLimit(ReportProjectSearchModel model)
        {
            return Content(HttpStatusCode.OK, pm.GetProjectsBySearch(base.ActorCompanyId, model.StatusIds, model.CategoryIds, model.StopDate, model.WithoutStopDate, model.SetStatusName, true));
        }

        [HttpGet]
        [Route("ReportImport/{dataStorageId:int}")]
        public IHttpActionResult GetReportsFromFile(int dataStorageId)
        {
            return Content(HttpStatusCode.OK, iem.importReportsFromTransferFile(dataStorageId, base.ActorCompanyId));
        }


        #endregion

    }
}
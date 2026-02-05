using Soe.Edi.Common.DTO;
using Soe.Sys.Common.DTO;
using Soe.WebApi.Extensions;
using Soe.WebApi.Models;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Core.Status;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Status.Shared.DTO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using static Soe.Edi.Common.Enumerations;

namespace Soe.WebApi.Controllers.Manage
{
    [RoutePrefix("Manage/System")]
    public class ManageController : SoeApiController
    {
        #region Variables

        private readonly GeneralManager gm;
        private readonly PayrollManager pm;
        private readonly SysServiceManager ssm;
        private readonly TermManager tm;
        private readonly SysScheduledJobManager jm;
        private readonly ImportExportManager iem;
        private readonly CommodityCodeManager cm;

        #endregion

        #region Constructor

        public ManageController(GeneralManager gm, PayrollManager pm, SysServiceManager ssm, TermManager tm, SysScheduledJobManager jm, ImportExportManager iem, CommodityCodeManager cm)
        {
            this.gm = gm;
            this.pm = pm;
            this.ssm = ssm;
            this.tm = tm;
            this.jm = jm;
            this.iem = iem;
            this.cm = cm;
        }

        #endregion

        #region SysCompany

        #region SysCompany

        [HttpGet]
        [Route("SysCompany/SysCompany")]
        public IHttpActionResult GetSysCompanies()
        {
            return Content(HttpStatusCode.OK, ssm.GetSysCompanies());
        }

        [HttpGet]
        [Route("SysCompany/SysCompany/{sysCompanyId}")]
        public IHttpActionResult GetSysCompanies(int sysCompanyId)
        {
            return Content(HttpStatusCode.OK, ssm.GetSysCompany(sysCompanyId));
        }

        [HttpGet]
        [Route("SysCompany/SysCompany/{companyApiKey}/{sysCompDbId}")]
        public IHttpActionResult GetSysCompany(string companyApiKey, int sysCompDbId)
        {
            return Content(HttpStatusCode.OK, ssm.GetSysCompany(companyApiKey, sysCompDbId));
        }

        [HttpPost]
        [Route("SysCompany/SysCompany")]
        public IHttpActionResult SaveSysCompany(SysCompanyDTO sysCompanyDTO)
        {
            return Content(HttpStatusCode.OK, ssm.SaveSysCompany(sysCompanyDTO, 0));
        }

        #endregion

        #region SysCompDB

        [HttpGet]
        [Route("SysCompany/SysCompDB")]
        public IHttpActionResult GetSysCompDBs()
        {
            return Content(HttpStatusCode.OK, ssm.GetSysCompDBs());
        }

        [HttpGet]
        [Route("SysCompany/SysCompDB/{sysCompDbId}")]
        public IHttpActionResult GetSysCompDB(int sysCompDbId)
        {
            return Content(HttpStatusCode.OK, ssm.GetSysCompDB(sysCompDbId));
        }

        #endregion

        #region SysCompServer

        [HttpGet]
        [Route("SysCompany/SysCompServer")]
        public IHttpActionResult GetSysCompServers()
        {
            return Content(HttpStatusCode.OK, ssm.GetSysCompServers());
        }


        [HttpGet]
        [Route("SysCompany/SysCompServer/{sysCompServerId}")]
        public IHttpActionResult GetSysCompServer(int sysCompServerId)
        {
            return Content(HttpStatusCode.OK, ssm.GetSysCompServer(sysCompServerId));
        }

        #endregion

        #endregion

        #region SysInformation

        [HttpGet]
        [Route("SysInformation")]
        public IHttpActionResult GetSysInformations(HttpRequestMessage message)
        {
            if (message.HasAcceptValue(HttpExtensions.ACCEPT_GRID_DTO))
                return Content(HttpStatusCode.OK, gm.GetSysInformations(false, false, true).ToGridDTOs());

            return Content(HttpStatusCode.OK, gm.GetSysInformations(true, true, false).ToDTOs(false));
        }

        [HttpGet]
        [Route("SysInformation/{sysInformationId:int}")]
        public IHttpActionResult GetSysInformation(int sysInformationId)
        {
            return Content(HttpStatusCode.OK, gm.GetSysInformation(sysInformationId, true, true, true).ToDTO(true));
        }

        [HttpGet]
        [Route("SysInformation/Folders")]
        public IHttpActionResult GetSysInformationFolders()
        {
            return Content(HttpStatusCode.OK, gm.GetSysInformationFolders());
        }

        [HttpGet]
        [Route("SysInformation/SysCompDb")]
        public IHttpActionResult GetSysInformationSysCompDbs()
        {
            return Content(HttpStatusCode.OK, gm.GetSysInformationSysCompDbs());
        }

        [HttpGet]
        [Route("SysInformation/SysFeature")]
        public IHttpActionResult GetSysInformationSysFeatures()
        {
            return Content(HttpStatusCode.OK, gm.GetSysInformationSysFeatures());
        }

        [HttpGet]
        [Route("SysInformation/HasConfirmations/{sysInformationId:int}")]
        public IHttpActionResult SysInformationHasConfirmations(int sysInformationId)
        {
            return Content(HttpStatusCode.OK, gm.SysInformationHasConfirmations(sysInformationId));
        }

        [HttpGet]
        [Route("SysInformation/RecipientInfo/{sysInformationId:int}")]
        public IHttpActionResult GetSysInformationRecipientInfo(int sysInformationId)
        {
            return Content(HttpStatusCode.OK, gm.GetSysInformationRecipientInfo(sysInformationId, base.UserId, true));
        }

        [HttpPost]
        [Route("SysInformation")]
        public IHttpActionResult SaveSysInformation(InformationDTO model)
        {
            return Content(HttpStatusCode.OK, gm.SaveSysInformation(model));
        }

        [HttpPost]
        [Route("SysInformation/DeleteMultiple")]
        public IHttpActionResult DeleteSysInformations(ListIntModel model)
        {
            return Content(HttpStatusCode.OK, gm.DeleteSysInformation(model.Numbers));
        }

        [HttpPost]
        [Route("SysInformation/DeleteNotificationSent/{sysInformationId:int}/{sysCompDbId:int}")]
        public IHttpActionResult DeleteSysInformationNotificationSent(int sysInformationId, int sysCompDbId)
        {
            return Content(HttpStatusCode.OK, gm.DeleteSysInformationNotificationSent(sysInformationId, sysCompDbId));
        }

        [HttpDelete]
        [Route("SysInformation/{sysInformationId:int}")]
        public IHttpActionResult DeleteSysInformation(int sysInformationId)
        {
            List<int> sysInformationIds = new List<int>();
            sysInformationIds.Add(sysInformationId);

            return Content(HttpStatusCode.OK, gm.DeleteSysInformation(sysInformationIds));
        }

        #endregion

        #region Wholeseller

        #region SysWholeseller

        [HttpGet]
        [Route("SysWholeseller/SysWholeseller")]
        public IHttpActionResult GetSysWholesellers()
        {
            return Content(HttpStatusCode.OK, ssm.GetSysWholesellers());
        }

        [HttpGet]
        [Route("SysWholeseller/SysWholeseller/{sysWholesellerId}")]
        public IHttpActionResult GetSysWholeseller(int sysWholesellerId)
        {
            return Content(HttpStatusCode.OK, ssm.GetSysWholeseller(sysWholesellerId));
        }

        [HttpPost]
        [Route("SysWholeseller/SysWholeseller")]
        public IHttpActionResult SaveSysWholeseller(SysWholesellerDTO sysWholesellerDTO)
        {
            return Content(HttpStatusCode.OK, ssm.SaveSysWholeseller(sysWholesellerDTO));
        }

        #endregion

        #endregion  

        #region Edi

        #region SysEdiMessageRaw

        [HttpGet]
        [Route("Edi/SysEdiMessageRaw")]
        public IHttpActionResult GetSysEdiMessageRaws()
        {
            return Content(HttpStatusCode.OK, ssm.GetSysEdiMessageRaws());
        }

        [HttpGet]
        [Route("Edi/SysEdiMessageRaw/{sysEdiMessageRaw}")]
        public IHttpActionResult GetSysEdiMessageRaw(int sysEdiMessageRaw)
        {
            return Content(HttpStatusCode.OK, ssm.GetSysEdiMessageRaw(sysEdiMessageRaw));
        }

        #endregion

        #region SysEdiMessageHead

        [HttpGet]
        [Route("Edi/SysEdiMessageHead")]
        public IHttpActionResult GetSysEdiMessageHeads()
        {
            return Content(HttpStatusCode.OK, ssm.GetSysEdiMessageHeads());
        }

        [HttpGet]
        [Route("Edi/SysEdiMessageHeadMsg/{sysEdiMessageHeadId}")]
        public IHttpActionResult GetSysEdiMessageHeadMsg(int sysEdiMessageHeadId)
        {
            return Content(HttpStatusCode.OK, ssm.GetSysEdiMessageHeadMessage(sysEdiMessageHeadId));
        }

        [HttpGet]
        [Route("Edi/SysEdiMessageGridHead/{status}/{take}/{missingSysCompanyId}")]
        public IHttpActionResult GetSysEdiMessageHeads(SysEdiMessageHeadStatus status, int take, bool missingSysCompanyId)
        {
            return Content(HttpStatusCode.OK, ssm.GetSysEdiMessageGridHeads(status, take, missingSysCompanyId));
        }

        [HttpGet]
        [Route("Edi/SysEdiMessageHead/{sysEdiMessageHead}")]
        public IHttpActionResult GetSysEdiMessageHead(int sysEdiMessageHead)
        {
            return Content(HttpStatusCode.OK, ssm.GetSysEdiMessageHead(sysEdiMessageHead));
        }

        [HttpPost]
        [Route("Edi/SysEdiMessageHead")]
        public IHttpActionResult GetSysEdiMessageHead(SysEdiMessageHeadDTO model)
        {
            return Content(HttpStatusCode.OK, ssm.SaveSysEdiMessageHead(model));
        }

        #endregion

        [HttpGet]
        [Route("Edi/EdiEntries/{fromDate}/{toDate}")]
        public IHttpActionResult GetEdiEntries(string fromDate, string toDate)
        {
            var em = new EdiManager(this.ParameterObject);
            return Content(HttpStatusCode.OK, em.GetEdiEntries(BuildDateTimeFromString(fromDate, true).Value, BuildDateTimeFromString(toDate, true).Value));
        }

        #endregion

        #region Import

        [HttpGet]
        [Route("Import/StandardDefinitions/")]
        public IHttpActionResult GetSysImportDefinitions()
        {
            return Content(HttpStatusCode.OK, iem.GetSysImportDefinitions().ToDTOs(false));
        }

        [HttpGet]
        [Route("Import/StandardDefinition/{sysImportDefinitionId:int}")]
        public IHttpActionResult GetSysImportDefinition(int sysImportDefinitionId)
        {
            return Content(HttpStatusCode.OK, iem.GetSysImportDefinition(sysImportDefinitionId).ToDTO(true));
        }

        [HttpGet]
        [Route("Import/SysImportHeadsDict/")]
        public IHttpActionResult GetSysImportHeadsDict()
        {
            return Content(HttpStatusCode.OK, iem.GetSysImportHeads().ToSmallGenericTypes());
        }

        [HttpGet]
        [Route("Import/SysImportHead/{sysImportHeadId:int}")]
        public IHttpActionResult GetSysImportHead(int sysImportHeadId)
        {
            return Content(HttpStatusCode.OK, iem.GetSysImportHead(sysImportHeadId).ToDTO(true, true));
        }

        [HttpPost]
        [Route("Import/StandardDefinition")]
        public IHttpActionResult SaveSysImportDefinitions(SysImportDefinitionDTO definition)
        {
            return Content(HttpStatusCode.OK, iem.SaveSysImportDefinition(definition, definition.SysImportDefinitionLevels));
        }

        [HttpDelete]
        [Route("Import/StandardDefinition/{sysImportDefinitionId:int}")]
        public IHttpActionResult DeleteSysImportDefinitions(int sysImportDefinitionId)
        {
            return Content(HttpStatusCode.OK, iem.DeleteSysImportDefinition(sysImportDefinitionId));
        }

        #endregion

        #region Scheduler

        #region RegisteredJob

        [HttpGet]
        [Route("Scheduler/RegisteredJob")]
        public IHttpActionResult GetRegisteredJobs()
        {
            return Content(HttpStatusCode.OK, jm.GetJobs(null).ToDTOs(false));
        }

        [HttpPost]
        [Route("Scheduler/RegisteredJob/")]
        public IHttpActionResult SaveJob(SysJobDTO job)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, jm.SaveJob(job, job.SysJobSettings ?? new List<SysJobSettingDTO>()));
        }

        [HttpPost]
        [Route("Scheduler/RegisteredJob/UpdateState/")]
        public IHttpActionResult UpdateJobState(Dictionary<int, int> itemsToUpdate)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, jm.UpdateJobState(itemsToUpdate));
        }

        [HttpDelete]
        [Route("Scheduler/RegisteredJob/{sysJobId:int}")]
        public IHttpActionResult DeleteJob(int sysJobId)
        {
            return Content(HttpStatusCode.OK, jm.DeleteJob(sysJobId));
        }

        #endregion

        #region ScheduledJob

        [HttpGet]
        [Route("Scheduler/ScheduledJob/{sysScheduledJobId:int}/{loadSettings:bool}/{loadJob:bool}")]
        public IHttpActionResult GetScheduledJob(int sysScheduledJobId, bool loadSettings, bool loadJob)
        {
            return Content(HttpStatusCode.OK, jm.GetScheduledJob(sysScheduledJobId, loadSettings, loadJob).ToDTO(loadSettings, loadJob, loadJob));
        }

        [HttpGet]
        [Route("Scheduler/ScheduledJob")]
        public IHttpActionResult GetScheduledJobs()
        {
            return Content(HttpStatusCode.OK, jm.GetScheduledJobs(null).ToDTOs(false, false, false));
        }

        [HttpGet]
        [Route("Scheduler/ScheduledJob/Log/{sysScheduledJobId:int}")]
        public IHttpActionResult GetScheduleJobLog(int sysScheduledJobId)
        {
            return Content(HttpStatusCode.OK, jm.GetScheduledJobLogs(sysScheduledJobId).ToDTOs(true));
        }

        [HttpGet]
        [Route("Scheduler/ScheduledJob/NoOfActive")]
        public IHttpActionResult GetNumberOfActiveScheduledJobs()
        {
            return Content(HttpStatusCode.OK, jm.GetNumberOfActiveScheduledJobs());
        }

        [HttpPost]
        [Route("Scheduler/ScheduledJob/Run/")]
        //public IHttpActionResult RunScheduledJob(int sysScheduledJobId)
        public IHttpActionResult RunScheduledJob(SysScheduledJobModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
            {
                int actorCompanyId = base.ActorCompanyId;
                Guid guid = Guid.NewGuid();
                var culture = Thread.CurrentThread.CurrentCulture;
                var workingThread = new Thread(() => jm.ExecuteScheduledJobSync(model.SysScheduledJobId, 0));
                workingThread.Start();
                return Content(HttpStatusCode.OK, new SoeProgressInfo(guid));
            }
        }

        [HttpPost]
        [Route("Scheduler/ScheduledJob/RunByService/")]
        public IHttpActionResult ActivateScheduledJob(SysScheduledJobModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, jm.RunScheduleJobByService(model.SysScheduledJobId));
        }

        [HttpPost]
        [Route("Scheduler/ScheduledJob/")]
        public IHttpActionResult SaveScheduledJob(SysScheduledJobDTO job)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, jm.SaveScheduledJob(job, job.SysJobSettings, job.SysJobId));
        }

        [HttpDelete]
        [Route("Scheduler/ScheduledJob/{sysScheduledJobId:int}")]
        public IHttpActionResult DeleteScheduledJob(int sysScheduledJobId)
        {
            return Content(HttpStatusCode.OK, jm.DeleteScheduledJob(sysScheduledJobId));
        }

        #endregion

        #endregion

        #region SoftOneServerUtility

        #region PageStatuses

        [HttpGet]
        [Route("SoftOneServerUtility/PageStatus/")]
        public IHttpActionResult GetSoftOneServerUtilityPageStatuses()
        {
            return Content(HttpStatusCode.OK, gm.GetSysPageStatuses(true).ToDTOs());
        }

        #endregion

        #endregion

        #region SysPayrollPrice

        [HttpGet]
        [Route("SysPayrollPrice/{sysCountryId:int}/{sysPayrollPricesString}/{setName:bool}/{setTypeName:bool}/{setAmountTypeName:bool}/{includeIntervals:bool}/{onlyLatest:bool}/{dateString}")]
        public IHttpActionResult GetSysPayrollPrices(int sysCountryId, string sysPayrollPricesString, bool setName, bool setTypeName, bool setAmountTypeName, bool includeIntervals, bool onlyLatest, string dateString)
        {
            List<int> sysPayrollPricesInts = (string.IsNullOrEmpty(sysPayrollPricesString) || sysPayrollPricesString == "null") ? new List<int>() : sysPayrollPricesString.Split(',').Select(Int32.Parse).ToList();

            List<TermGroup_SysPayrollPrice> sysPayrollPrices = new List<TermGroup_SysPayrollPrice>();
            foreach (int sysPayrollPricesInt in sysPayrollPricesInts)
            {
                sysPayrollPrices.Add((TermGroup_SysPayrollPrice)sysPayrollPricesInt);
            }

            return Content(HttpStatusCode.OK, pm.GetSysPayrollPrices(sysCountryId, sysPayrollPrices, setName, setTypeName, setAmountTypeName, includeIntervals, onlyLatest, BuildDateTimeFromString(dateString, true)).ToDTOs(includeIntervals));
        }

        [HttpGet]
        [Route("SysPayrollPrice/{sysPayrollPriceId:int}/{includeIntervals:bool}/{setName:bool}/{setTypeName:bool}/{setAmountTypeName:bool}")]
        public IHttpActionResult GetSysPayrollPrice(int sysPayrollPriceId, bool includeIntervals, bool setName, bool setTypeName, bool setAmountTypeName)
        {
            return Content(HttpStatusCode.OK, pm.GetSysPayrollPrice(sysPayrollPriceId, setName, setTypeName, setAmountTypeName).ToDTO(includeIntervals));
        }

        [HttpGet]
        [Route("SysPayrollPrice/Amount/{sysTermId:int}/{date}")]
        public IHttpActionResult GetSysPayrollPriceAmount(int sysTermId, string date)
        {
            return Content(HttpStatusCode.OK, pm.GetSysPayrollPriceAmount(base.ActorCompanyId, sysTermId, BuildDateTimeFromString(date, true)));
        }

        [HttpGet]
        [Route("SysPayrollPrice/ValidatePassword/{password}")]
        public IHttpActionResult ValidatePasswordForSysPayrollPrice(string password)
        {
            return Content(HttpStatusCode.OK, pm.ValidatePasswordForSysPayrollPrice(password));
        }

        [HttpPost]
        [Route("SysPayrollPrice")]
        public IHttpActionResult SaveSysPayrollPrice(SysPayrollPriceDTO sysPayrollPrice)
        {
            return Content(HttpStatusCode.OK, pm.SaveSysPayrollPrices(new List<SysPayrollPriceDTO>() { sysPayrollPrice }));
        }

        #endregion      

        #region SysVehicleType

        [HttpGet]
        [Route("SysVehicleType/")]
        public IHttpActionResult GetSysVehicleTypes()
        {
            return Content(HttpStatusCode.OK, pm.GetSysVehicleTypes().ToDTOs());
        }

        [HttpGet]
        [Route("SysVehicleType/{sysVehicleTypeId:int}")]
        public IHttpActionResult GetSysVehicleType(int sysVehicleTypeId)
        {
            return Content(HttpStatusCode.OK, pm.GetSysVehicleType(sysVehicleTypeId).ToDTO());
        }

        [HttpGet]
        [Route("SysVehicleType/ByCode/{modelCode}")]
        public IHttpActionResult GetSysVehicleByCode(string modelCode)
        {
            return Content(HttpStatusCode.OK, pm.GetSysVehicle(modelCode));
        }

        [HttpGet]
        [Route("SysVehicleType/ManufacturingYear/")]
        public IHttpActionResult GetSysVehicleManufacturingYears()
        {
            return Content(HttpStatusCode.OK, pm.GetSysVehicleManufacturingYears());
        }

        [HttpGet]
        [Route("SysVehicleType/VehicleMake/{type:int}/{manufacturingYear:int}")]
        public IHttpActionResult GetSysVehicleMakes(int type, int manufacturingYear)
        {
            return Content(HttpStatusCode.OK, pm.GetSysVehicleMakes((TermGroup_VehicleType)type, manufacturingYear));
        }

        [HttpGet]
        [Route("SysVehicleType/VehicleModel/{type:int}/{manufacturingYear:int}/{make}")]
        public IHttpActionResult GetSysVehicleModels(int type, int manufacturingYear, string make)
        {
            return Content(HttpStatusCode.OK, pm.GetSysVehicleModels((TermGroup_VehicleType)type, manufacturingYear, make));
        }

        [HttpPost]
        [Route("SysVehicleType")]
        public async Task<IHttpActionResult> SaveSysVehicleType()
        {
            if (Request.Content.IsMimeMultipartContent())
            {
                var data = await Request.Content.ParseMultipartAsync();

                HttpPostedFile file = data.Files["file"];
                if (file != null)
                {
                    // Can not use TryParse with a nullable DateTime
                    DateTime? dateFrom = null;
                    DateTime date = DateTime.MinValue;
                    if (data.Fields.Count > 0 && data.Fields.ContainsKey("dateFrom") && DateTime.TryParse(data.Fields["dateFrom"].Value, out date))
                        dateFrom = date;

                    return Content(HttpStatusCode.OK, pm.SaveSysVehicleType(new MemoryStream(file.File), file.Filename, dateFrom));
                }
            }

            throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
        }

        [HttpDelete]
        [Route("SysVehicleType/{sysVehicleTypeId:int}")]
        public IHttpActionResult DeleteSysVehicleType(int sysVehicleTypeId)
        {
            return Content(HttpStatusCode.OK, pm.DeleteSysVehicleType(sysVehicleTypeId));
        }

        #endregion

        #region Intrastat - CommodityCodes

        [HttpGet]
        [Route("CommodityCodes/{langId:int}")]
        public IHttpActionResult GetCommodyCodes(int langId)
        {
            return Content(HttpStatusCode.OK, cm.GetSysIntrastatCodesDTOs(langId));
        }

        #endregion

        #region SysTermGroups

        [HttpGet]
        [Route("SysTermGroups/")]
        public IHttpActionResult GetSysTermGroups()
        {
            return Content(HttpStatusCode.OK, tm.GetSysTermGroupsFromDatabase().ToSmallGenericTypes());
        }

        #endregion

        #region SysTerms

        [HttpGet]
        [Route("SysTerms/{sysTermGroupId:int}/{langId:int}/{date}/")]
        public IHttpActionResult GetSysTerms(int sysTermGroupId, int langId, string date)
        {
            return Content(HttpStatusCode.OK, tm.GetSysTermsFromDatabase(langId, sysTermGroupId, BuildDateTimeFromString(date, true, CalendarUtility.DATETIME_DEFAULT).Value, WildCard.GreaterThan).ToDTOs());
        }

        [HttpGet]
        [Route("SysTerms/{sysTermGroupId:int}/{langId:int}/")]
        public IHttpActionResult GetSysTerms(int sysTermGroupId, int langId)
        {
            return Content(HttpStatusCode.OK, tm.GetSysTermsFromDatabase(langId, sysTermGroupId, null, WildCard.GreaterThan).ToDTOs());
        }

        [HttpPost]
        [Route("SysTerms/Suggestion/")]
        public IHttpActionResult GetSysTermSuggestion(SysTermSuggestionModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, TermCacheManager.Instance.TranslateText(SoeTranslationClient.BingTranslate, model.Text, model.PrimaryLanguageId, model.SecondaryLanguageId));
        }

        [HttpPost]
        [Route("SysTerms/")]
        public IHttpActionResult SaveSySTerm(List<SysTermDTO> terms)
        {
            return Content(HttpStatusCode.OK, tm.SaveSysTerms(terms));
        }

        #endregion

        #region TestCases

        [HttpGet]
        [Route("Test/TestCases/")]
        public IHttpActionResult GetTestCases(TestCaseType testCaseType)
        {
            return Content(HttpStatusCode.OK, SoftOneStatusConnector.GetTestCases(testCaseType));
        }

        [HttpGet]
        [Route("Test/Testcase/{testCaseId:int}")]
        public IHttpActionResult GetTestCase(int testCaseId)
        {
            return Content(HttpStatusCode.OK, SoftOneStatusConnector.GetTestCase(testCaseId));
        }

        [HttpPost]
        [Route("Test/TestCase/")]
        public IHttpActionResult SaveTestCase(TestCaseDTO testCase)
        {
            return Content(HttpStatusCode.OK, SoftOneStatusConnector.SaveTestCase(testCase));
        }

        [HttpGet]
        [Route("Test/Testcase/TestCaseGroup/{testCaseGroupId:int}")]
        public IHttpActionResult GetTestCaseGroup(int testCaseGroupId)
        {
            return Content(HttpStatusCode.OK, SoftOneStatusConnector.GetTestCaseGroup(testCaseGroupId));
        }

        [HttpGet]
        [Route("Test/Testcase/TestCaseGroups/")]
        public IHttpActionResult GetTestCaseGroups()
        {
            return Content(HttpStatusCode.OK, SoftOneStatusConnector.GetTestCaseGroups());
        }

        [HttpPost]
        [Route("Test/TestCase/TestCaseGroup")]
        public IHttpActionResult SaveTestCaseGroup(TestCaseGroupDTO testCaseGroup)
        {
            return Content(HttpStatusCode.OK, SoftOneStatusConnector.SaveTestCaseGroup(testCaseGroup));
        }

        [HttpPost]
        [Route("Test/TestCase/TestCaseGroup/Run/{testCaseGroupId:int}")]
        public IHttpActionResult RunTestCaseGroup(int testCaseGroupId)
        {
            return Content(HttpStatusCode.OK, SoftOneStatusConnector.ScheduleTestGroupsNow(new List<int>() { testCaseGroupId }));
        }
        [HttpPost]
        [Route("Test/TestCase/TestCaseGroup/ScheduleNow")]
        public IHttpActionResult RunTestCaseGroup([FromBody] List<int> testCaseGroupIds)
        {
            return Content(HttpStatusCode.OK, SoftOneStatusConnector.ScheduleTestGroupsNow(testCaseGroupIds));
        }

        [HttpGet]
        [Route("Test/Tracking/{trackingGuid:Guid}")]
        public IHttpActionResult GetTestCaseTracking(Guid trackingGuid)
        {
            return Content(HttpStatusCode.OK, SoftOneStatusConnector.GetTestCaseTracking(trackingGuid));
        }

        [HttpGet]
        [Route("Test/TestCaseResultsByTestCaseId/{testCaseId:int}")]
        public IHttpActionResult GetTestCaseResultsByTestCaseId(int testCaseId)
        {
            return Content(HttpStatusCode.OK, SoftOneStatusConnector.GetTestCaseResultsByTestCaseId(testCaseId));
        }

        [HttpGet]
        [Route("Test/TestCaseResultsByTestCaseGroupId/{testCaseGroupId:int}")]
        public IHttpActionResult GetTestCaseResultsByTestCaseGroupId(int testCaseGroupId)
        {
            return Content(HttpStatusCode.OK, SoftOneStatusConnector.GetTestCaseResultsByTestCaseGroupId(testCaseGroupId));
        }

        [HttpGet]
        [Route("Test/TestCaseGroupResults/{testCaseGroupId:int}")]
        public IHttpActionResult GetTestCaseGroupResults(int testCaseGroupId)
        {
            return Content(HttpStatusCode.OK, SoftOneStatusConnector.GetTestCaseGroupResults(testCaseGroupId));
        }

        [HttpGet]
        [Route("Test/GetTestCaseGroupOverview")]
        public IHttpActionResult GetTestCaseGroupOverview()
        {
            return Content(HttpStatusCode.OK, SoftOneStatusConnector.GetTestCaseGroupOverview());
        }
        [HttpGet]
        [Route("Test/GetTestCaseGroupOverviewByGroup/{testCaseGroupId}")]
        public IHttpActionResult GetTestCaseGroupOverviewByGroup(int testCaseGroupId)
        {
            return Content(HttpStatusCode.OK, SoftOneStatusConnector.GetTestCaseGroupOverviewByGroup(testCaseGroupId));
        }
        [HttpGet]
        [Route("Test/TestCaseSettings")]
        public IHttpActionResult GetTestCaseSettings()
        {
            return Content(HttpStatusCode.OK, SoftOneStatusConnector.GetTestCaseSettings());
        }
        #endregion
    }
}
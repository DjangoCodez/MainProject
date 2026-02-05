using Soe.WebApi.Extensions;
using Soe.WebApi.Models;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace Soe.WebApi.Controllers.Time
{
    [RoutePrefix("Time/Employee")]
    public class EmployeeController : SoeApiController
    {
        #region Variables

        private readonly ActorManager am;
        private readonly AnnualLeaveManager alm;
        private readonly ContactManager com;
        private readonly EmployeeManager em;
        private readonly TimeAccumulatorManager tam;
        private readonly TimePeriodManager tpm;
        private readonly TimeScheduleManager tsm;
        private readonly PayrollManager pm;
        private readonly SettingManager sm;
        private readonly ReportManager rm;

        #endregion

        #region Constructor

        public EmployeeController(ActorManager am, AnnualLeaveManager alm, ContactManager com, EmployeeManager em, TimeAccumulatorManager tam, TimeScheduleManager tsm, TimePeriodManager tpm, PayrollManager pm, SettingManager sm, ReportManager rm)
        {
            this.am = am;
            this.alm = alm;
            this.com = com;
            this.em = em;
            this.tam = tam;
            this.tsm = tsm;
            this.tpm = tpm;
            this.pm = pm;
            this.sm = sm;
            this.rm = rm;
        }

        #endregion

        #region Accumulators

        [HttpPost]
        [Route("Accumulators")]
        public IHttpActionResult GetEmployeeAccumulators(GetEmployeeAccumulatorsModel model)
        {
            return Content(HttpStatusCode.OK, tam.GetEmployeeAccumulators(model.EmployeeIds, model.AccumulatorIds, model.DateFrom, model.DateTo, model.CompareModel, model.OwnLimitMin, model.OwnLimitMax));
        }

        #endregion

        #region AnnualLeave

        [HttpGet]
        [Route("AnnualLeaveGroup")]
        public IHttpActionResult GetAnnualLeaveGroups(HttpRequestMessage message)
        {
            if (message.HasAcceptValue(HttpExtensions.ACCEPT_GENERIC_TYPE))
                return Content(HttpStatusCode.OK, alm.GetAnnualLeaveGroupsDict(base.ActorCompanyId, message.GetBoolValueFromQS("addEmptyRow")).ToSmallGenericTypes());

            return Content(HttpStatusCode.OK, alm.GetAnnualLeaveGroups(base.ActorCompanyId).ToDTOs());
        }
        
        #endregion

        #region CardNumber

        [HttpGet]
        [Route("CardNumber")]
        public IHttpActionResult GetCardNumbers()
        {
            return Content(HttpStatusCode.OK, em.GetCardNumbers(base.ActorCompanyId, base.RoleId, base.UserId));
        }

        [HttpGet]
        [Route("CardNumber/Exists/{cardNumber}/{excludeEmployeeId:int}")]
        public IHttpActionResult CardNumberExists(string cardNumber, int excludeEmployeeId)
        {
            return Content(HttpStatusCode.OK, em.CardNumberExists(base.ActorCompanyId, cardNumber, excludeEmployeeId));
        }

        [HttpDelete]
        [Route("CardNumber/{employeeId:int}")]
        public IHttpActionResult DeleteCardNumber(int employeeId)
        {
            return Content(HttpStatusCode.OK, em.ClearCardNumber(employeeId, base.ActorCompanyId));
        }

        #endregion

        #region CalculatedCosts

        [HttpGet]
        [Route("CalculatedCosts/{employeeId:int}")]
        public IHttpActionResult GetCalculatedCosts(int employeeId)
        {
            return Content(HttpStatusCode.OK, em.GetEmployeeCalculatedCostDTOs(employeeId, base.ActorCompanyId,false));
        }

        #endregion

        #region CSR

        [HttpGet]
        [Route("CsrInquiry/{employeeId:int}/{year:int}")]
        public IHttpActionResult CsrInquiry(int employeeId, int year)
        {
            return Content(HttpStatusCode.OK, em.CsrInquiry(base.ActorCompanyId, employeeId, year));
        }

        [HttpGet]
        [Route("CsrExportEmployees/{year:int}")]
        public IHttpActionResult GetEmployeesForCSRExportDTO(int year)
        {
            return Content(HttpStatusCode.OK, em.GetEmployeesForCSRExport(base.ActorCompanyId, year));
        }

        [HttpPost]
        [Route("TaxSe/{employeeTaxId:int}/{employeeId:int}")]
        public IHttpActionResult SaveEmployeeTaxSe(int employeeTaxId, int employeeId)
        {
            return Content(HttpStatusCode.OK, em.SaveEmployeeTaxSE(employeeTaxId, employeeId, base.ActorCompanyId, TermGroup_TrackChangesActionMethod.Employee_CsrInquiry));
        }

        [HttpPost]
        [Route("CsrExportPrint")]
        public IHttpActionResult GetTaxSeReportURL(GetCSRReportModel model)
        {
            return Content(HttpStatusCode.OK, em.GetEmployeeTaxSEPrintURL(base.ActorCompanyId, base.UserId, model.IdsToTransfer, model.year));
        }

        [HttpPost]
        [Route("CsrResponses")]
        public IHttpActionResult GetCsrInquiries(GetCSRResponseModel model)
        {
            return Content(HttpStatusCode.OK, em.CsrInquiries(base.ActorCompanyId, model.IdsToTransfer, model.year));
        }

        [HttpGet]
        [Route("CsrImport/{dataStorageId:int}")]
        public IHttpActionResult importCsrFromDataStorage(int dataStorageId)
        {
            return Content(HttpStatusCode.OK, em.ImportEmployeeTaxSEDataFromDataStorage(dataStorageId, base.ActorCompanyId, base.UserId));
        }

        #endregion

        #region Employee

        [HttpGet]
        [Route("")]
        public IHttpActionResult GetEmployees(HttpRequestMessage message)
        {
            return Content(HttpStatusCode.OK, em.GetAllEmployeesDict(base.ActorCompanyId, message.GetBoolValueFromQS("addEmptyRow"), message.GetBoolValueFromQS("concatNumberAndName"), message.GetBoolValueFromQS("getHidden"), message.GetBoolValueFromQS("orderByName")).ToSmallGenericTypes());
        }

        [HttpGet]
        [Route("AllEmployees/")]
        public IHttpActionResult GetAllEmployees()
        {
            return Content(HttpStatusCode.OK, em.GetAllEmployees(base.ActorCompanyId, loadEmployment: true).ToDTOs());
        }

        [HttpGet]
        [Route("EmployeeForGrid")]
        public IHttpActionResult GetEmployeesForGrid(HttpRequestMessage message)
        {
            DateTime date = message.GetDateValueFromQS("date").ToValueOrToday();
            List<int> employeeFilter = message.GetIntListValueFromQS("employeeFilter", nullIfEmpty: true);
            bool showInactive = message.GetBoolValueFromQS("showInactive");
            bool showEnded = message.GetBoolValueFromQS("showEnded");
            bool showNotStarted = message.GetBoolValueFromQS("showNotStarted");
            bool setAge = message.GetBoolValueFromQS("setAge");
            bool loadPayrollGroups = message.GetBoolValueFromQS("loadPayrollGroups");
            bool loadAnnualLeaveGroups = message.GetBoolValueFromQS("loadAnnualLeaveGroups");

            if (message.HasAcceptValue(HttpExtensions.ACCEPT_SMALL_DTO))
                return Content(HttpStatusCode.OK, em.GetEmployeesForGridSmall(base.ActorCompanyId, base.UserId, base.RoleId, date, employeeFilter, showInactive, showEnded, showNotStarted));

            return Content(HttpStatusCode.OK, em.GetEmployeesForGrid(base.ActorCompanyId, base.UserId, base.RoleId, date, employeeFilter, showInactive, showEnded, showNotStarted, setAge, loadPayrollGroups, loadAnnualLeaveGroups));
        }

        [HttpGet]
        [Route("EmployeeForEdit/{employeeId:int}/{loadMeetings:bool}/{loadTemplateGroups:bool}")]
        public IHttpActionResult GetEmployeeForEdit(int employeeId, bool loadMeetings, bool loadTemplateGroups)
        {
            return Content(HttpStatusCode.OK, am.GetEmployeeUserDTOFromEmployee(employeeId, base.ActorCompanyId, date: DateTime.Today, 
                applyFeatures: true,
                loadEmploymentAccounting: true,
                loadEmploymentPriceTypes: true,
                loadEmploymentVacationGroups: true,
                loadEmploymentVacationGroupSE: true,
                loadEmployeeAccounts: true,
                loadEmployeeChilds: true,
                loadEmployeeChildCares: true,
                loadEmployeeFactors: true,
                loadEmployeeMeetings: loadMeetings, 
                loadEmployeeSettings: true,
                loadEmployeeTemplate: true,
                loadEmployeeTemplateGroups: loadTemplateGroups,
                loadEmployeeSkills: true,
                loadEmployeeUnionFees: true,
                loadEmployeeVacation: true,
                loadExternalAuthId: true,
                clearPassword: true
                ));
        }

        [HttpGet]
        [Route("Export/{employeeId:int}")]
        public IHttpActionResult GetEmployeeForExport(int employeeId)
        {
            return Content(HttpStatusCode.OK, am.DownloadEmployeeUserDTOFromEmployee(employeeId, base.ActorCompanyId));
        }

        [HttpGet]
        [Route("EmployeesForProject/{addEmptyRow:bool}/{getHidden:bool}/{addNoReplacementEmployee:bool}/{includeEmployeeId:int?}")]
        public IHttpActionResult GetEmployeesForProject(bool addEmptyRow, bool getHidden, bool addNoReplacementEmployee, int? includeEmployeeId)
        {
            return Content(HttpStatusCode.OK, em.GetEmployeesDictForProject(base.ActorCompanyId, base.UserId, base.RoleId, addEmptyRow, getHidden, addNoReplacementEmployee, includeEmployeeId).ToSmallGenericTypes());
        }

        [HttpGet]
        [Route("EmployeesForProjectTimeCode/{addEmptyRow:bool}/{getHidden:bool}/{addNoReplacementEmployee:bool}/{includeEmployeeId:int?}/{fromDateString}/{toDateString}")]
        public IHttpActionResult GetEmployeesForProjectWithTimeCode(bool addEmptyRow, bool getHidden, bool addNoReplacementEmployee, int? includeEmployeeId, string fromDateString, string toDateString)
        {
            return Content(HttpStatusCode.OK, em.GetEmployeesForProjectWithTimeCode(base.ActorCompanyId, base.UserId, base.RoleId, addEmptyRow, getHidden, addNoReplacementEmployee, includeEmployeeId, BuildDateTimeFromString(fromDateString, true), BuildDateTimeFromString(toDateString, true)));
        }

        [HttpGet]
        [Route("EmployeesWithPayrollExport/{timePeriodId:int}/{payrollGroupId:int}")]
        public IHttpActionResult GetEmployeesWithPayrollExport(int timePeriodId, int payrollGroupId)
        {
            return Content(HttpStatusCode.OK, em.GetEmployeesForPayrollPaymentSelection(base.ActorCompanyId, base.UserId, base.RoleId, timePeriodId, payrollGroupId));
        }

        [HttpGet]
        [Route("VacantEmployeeIds/")]
        public IHttpActionResult GetVacantEmployeeIds()
        {
            return Content(HttpStatusCode.OK, em.GetVacantEmployeeIds(base.ActorCompanyId));
        }

        [HttpGet]
        [Route("PayrollCalculationTree/{filterIds}/{timePeriodId:int}")]
        public IHttpActionResult GetEmployeesForPayrollCalculationTree(string filterIds, int timePeriodId)
        {
            return Content(HttpStatusCode.OK, em.GetEmployeesForTree(StringUtility.SplitNumericList(filterIds, nullIfEmpty: true), timePeriodId));
        }

        [HttpGet]
        [Route("PayrollCalculation/{employeeId:int}/{timePeriodId:int}/{dateFrom}/{dateTo}/{taxYear:int?}")]
        public IHttpActionResult GetEmployeeForPayrollCalculation(int employeeId, int timePeriodId, string dateFrom, string dateTo, int? taxYear)
        {
            return Content(HttpStatusCode.OK, em.GetEmployeeForPayrollCalculation(base.ActorCompanyId, employeeId, timePeriodId, BuildDateTimeFromString(dateFrom, true), BuildDateTimeFromString(dateTo, true), taxYear.ToNullable()));
        }

        [HttpGet]
        [Route("AbsencePlanning/{dateFrom}/{dateTo}/{mandatoryEmployeeId:int}/{excludeCurrentUserEmployee:bool}/{timeScheduleScenarioHeadId:int?}")]
        public IHttpActionResult GetEmployeesForAbsencePlanning(string dateFrom, string dateTo, int mandatoryEmployeeId, bool excludeCurrentUserEmployee, int? timeScheduleScenarioHeadId)
        {
            List<int> employeeIds = null;
            bool useHidden = sm.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.TimeUseStaffing, base.UserId, base.ActorCompanyId, 0);
            List<EmployeeListDTO> employees = new List<EmployeeListDTO>();
            if (timeScheduleScenarioHeadId > 0)
            {
                int hiddenEmployeeId = em.GetHiddenEmployeeId(base.ActorCompanyId);
                var scenarioHead = tsm.GetTimeScheduleScenarioHead(timeScheduleScenarioHeadId.Value, base.ActorCompanyId, true, false).ToDTO();
                if (scenarioHead == null)
                    return Content(HttpStatusCode.OK, employees);

                employeeIds = scenarioHead.Employees?.Select(x => x.EmployeeId).ToList() ?? new List<int>();

                if (!employeeIds.Contains(hiddenEmployeeId))
                    useHidden = false;
            }

            var tempEmployees = em.GetEmployeeList(base.ActorCompanyId, base.RoleId, base.UserId, employeeIds, null, useHidden, false, false, false, false, false, false, BuildDateTimeFromString(dateFrom, true), BuildDateTimeFromString(dateTo, true), true, true, true, mandatoryEmployeeId, excludeCurrentUserEmployee: excludeCurrentUserEmployee);
            var hiddenEmployee = tempEmployees.FirstOrDefault(x => x.Hidden);
            if (hiddenEmployee != null)
            {
                employees.Add(hiddenEmployee);
                tempEmployees.Remove(hiddenEmployee);
            }
            var noReplacementEmployee = tempEmployees.FirstOrDefault(x => x.EmployeeId == Constants.NO_REPLACEMENT_EMPLOYEEID);
            if (noReplacementEmployee != null)
            {
                employees.Add(noReplacementEmployee);
                tempEmployees.Remove(noReplacementEmployee);
            }

            employees.AddRange(tempEmployees);

            return Content(HttpStatusCode.OK, employees);
        }

        [HttpGet]
        [Route("TimeAttestTree/{filterIds}/{dateFrom}/{dateTo}")]
        public IHttpActionResult GetEmployeeListForTimeAttestTree(string filterIds, string dateFrom, string dateTo)
        {
            return Content(HttpStatusCode.OK, em.GetEmployeesForTree(StringUtility.SplitNumericList(filterIds, nullIfEmpty: true), BuildDateTimeFromString(dateFrom, true, CalendarUtility.DATETIME_DEFAULT).Value, BuildDateTimeFromString(dateTo, true, CalendarUtility.DATETIME_DEFAULT).Value));
        }

        [HttpGet]
        [Route("Planning/{employeeIds}/{categoryIds}/{getHidden:bool}/{getInactive:bool}/{loadSkills:bool}/{loadAvailability:bool}/{loadImage:bool}/{dateFrom}/{dateTo}/{includeSecondaryCategoriesOrAccounts:bool}/{displayMode:int}")]
        public IHttpActionResult GetEmployeeListForPlanning(string employeeIds, string categoryIds, bool getHidden, bool getInactive, bool loadSkills, bool loadAvailability, bool loadImage, string dateFrom, string dateTo, bool includeSecondaryCategoriesOrAccounts, TimeSchedulePlanningDisplayMode displayMode)
        {
            return Content(HttpStatusCode.OK, em.GetEmployeeList(base.ActorCompanyId, base.RoleId, base.UserId, StringUtility.SplitNumericList(employeeIds, true), StringUtility.SplitNumericList(categoryIds, true), getHidden, getInactive, loadSkills, loadAvailability, loadImage, false, true, BuildDateTimeFromString(dateFrom, true), BuildDateTimeFromString(dateTo, true), false, false, includeSecondaryCategoriesOrAccounts: includeSecondaryCategoriesOrAccounts, displayMode: displayMode));
        }

        [HttpGet]
        [Route("Planning/{dateFrom}/{dateTo}/{includeSecondary:bool}/{employedInCurrentYear:bool}/{employeeIds}")]
        public IHttpActionResult GetEmployeeListSmallForPlanning(string dateFrom, string dateTo, bool includeSecondary, bool employedInCurrentYear, string employeeIds)
        {
            return Content(HttpStatusCode.OK, em.GetEmployeeListSmall(base.ActorCompanyId, base.RoleId, base.UserId, BuildDateTimeFromString(dateFrom, true).Value, BuildDateTimeFromString(dateTo, true).Value, includeSecondary, employedInCurrentYear, StringUtility.SplitNumericList(employeeIds, true)));
        }

        [HttpGet]
        [Route("GetEmployeeLicenseInfo")]
        public IHttpActionResult GetEmployeeLicenseInfo()
        {
            return Content(HttpStatusCode.OK, em.GetNrOfEmployeesAndMaxByLicense(base.LicenseId, true));
        }

        [HttpGet]
        [Route("ContactInfo/{employeeId:int}")]
        public IHttpActionResult GetContactInfoForEmployee(int employeeId)
        {
            return Content(HttpStatusCode.OK, com.GetContactInfoForEmployee(employeeId, base.ActorCompanyId));
        }

        [HttpGet]
        [Route("Planning/EmployeePost/{employeePostIds}/{dateFrom}/{dateTo}")]
        public IHttpActionResult GetEmployeePostForPlanning(string employeePostIds, string dateFrom, string dateTo)
        {
            return Content(HttpStatusCode.OK, tsm.GetEmployeePostList(base.ActorCompanyId, StringUtility.SplitNumericList(employeePostIds, true), BuildDateTimeFromString(dateFrom, true), BuildDateTimeFromString(dateTo, true)));
        }

        [HttpGet]
        [Route("Employee/{employeeId:int}/{dateFrom}/{dateTo}/{includeEmployments:bool}/{includeEmployeeGroup:bool}/{includePayrollGroup:bool}/{includeVacationGroup:bool}/{includeEmployeeTax:bool}/{taxYear:int?}")]
        public IHttpActionResult GetEmployee(int employeeId, string dateFrom, string dateTo, bool includeEmployments, bool includeEmployeeGroup, bool includePayrollGroup, bool includeVacationGroup, bool includeEmployeeTax, int? taxYear)
        {
            includeEmployments = (includeEmployments || includeEmployeeGroup || includePayrollGroup || includeVacationGroup);
            DateTime? from = BuildDateTimeFromString(dateFrom, true);
            DateTime? to = BuildDateTimeFromString(dateTo, true);
            return Content(HttpStatusCode.OK, em.GetEmployee(employeeId, base.ActorCompanyId, dateFrom: from, dateTo: to, loadEmployment: includeEmployments, loadVacationGroup: includeVacationGroup, loadContactPerson: true, loadEmployeeTax: includeEmployeeTax, loadEmployeeVacation: includeVacationGroup).ToDTO(includeEmployments: includeEmployments, includeEmployeeGroup: includeEmployeeGroup, includePayrollGroup: includePayrollGroup, includeVacationGroup: includeVacationGroup, includeEmployeeTax: includeEmployeeTax, employmentTypes: em.GetEmploymentTypes(base.ActorCompanyId), disbursementMethodTerms: base.GetTermGroupContent(TermGroup.EmployeeDisbursementMethod), employeeTaxTypes: base.GetTermGroupContent(TermGroup.EmployeeTaxType), taxYear: taxYear, dateFrom: from, dateTo: to));
        }

        [HttpGet]
        [Route("HiddenEmployeeId/")]
        public IHttpActionResult GetHiddenEmployeeId()
        {
            return Content(HttpStatusCode.OK, em.GetHiddenEmployeeId(base.ActorCompanyId));
        }

        [HttpGet]
        [Route("Count")]
        public IHttpActionResult GetEmployeesCount()
        {
            return Content(HttpStatusCode.OK, em.GetAllEmployeesCount(base.ActorCompanyId));
        }

        [HttpGet]
        [Route("EmployeeForUser/")]
        public IHttpActionResult GetEmployeeForUser()
        {
            return Content(HttpStatusCode.OK, em.GetEmployeeForUser(base.UserId, base.ActorCompanyId).ToSmallDTO());
        }

        [HttpGet]
        [Route("EmployeeForUser/TimeCode/{date}")]
        public IHttpActionResult GetEmployeeForUserWithTimeCode(string date)
        {
            return Content(HttpStatusCode.OK, em.GetProjectEmployeeForUser(base.UserId, base.ActorCompanyId, false, true).ToEmployeeTimeCodeDTO(BuildDateTimeFromString(date, true), em.GetEmployeeGroupsFromCache(base.ActorCompanyId)));
        }

        [HttpGet]
        [Route("EmployeeAndGroupForUser/")]
        public IHttpActionResult GetEmployeeAndGroupForUser()
        {
            return Content(HttpStatusCode.OK, em.GetEmployeeForUser(base.UserId, base.ActorCompanyId, loadEmployment: true).ToDTO(includeEmployments: true, includeEmployeeGroup: true, employeeGroups: em.GetEmployeeGroupsFromCache(base.ActorCompanyId)));
        }

        [HttpGet]
        [Route("EmployeesWithoutUsers/{onlyActive:bool}/{addEmptyRow:bool}/{concatNumberAndName:bool}")]
        public IHttpActionResult GetEmployeesWithoutUsers(bool onlyActive, bool addEmptyRow, bool concatNumberAndName)
        {
            return Content(HttpStatusCode.OK, em.GetEmployeesWithoutUsersDict(base.ActorCompanyId, base.UserId, base.RoleId, onlyActive, addEmptyRow, concatNumberAndName).ToSmallGenericTypes());
        }

        [HttpGet]
        [Route("GetLastUsedEmployeeSequenceNumber/")]
        public IHttpActionResult GetLastUsedEmployeeSequenceNumber()
        {
            return Content(HttpStatusCode.OK, em.GetLastUsedEmployeeSequenceNumber(base.ActorCompanyId));
        }

        [HttpGet]
        [Route("EmployeeNumberExists/{employeeNr}/{getHidden:bool}/{excludeEmployeeId:int}")]
        public IHttpActionResult EmployeeNrExists(string employeeNr, bool getHidden, int excludeEmployeeId)
        {
            return Content(HttpStatusCode.OK, em.EmployeeExists(employeeNr, base.ActorCompanyId, getHidden, excludeEmployeeId));
        }

        [HttpGet]
        [Route("ValidateEmployeeSocialSecNumberNotExists/{socialSecNr}/{excludeEmployeeId:int}")]
        public IHttpActionResult ValidateEmployeeSocialSecNumberNotExists(string socialSecNr, int excludeEmployeeId)
        {
            return Content(HttpStatusCode.OK, em.ValidateEmployeeSocialSecNumberNotExists(socialSecNr, base.ActorCompanyId, excludeEmployeeId));
        }

        [HttpGet]
        [Route("IsEmployeeCurrentUser/{employeeId:int}")]
        public IHttpActionResult IsEmployeeCurrentUser(int employeeId)
        {
            return Content(HttpStatusCode.OK, em.IsEmployeeCurrentUser(employeeId, base.UserId));
        }

        [HttpGet]
        [Route("DefaultEmployeeAccountDimName")]
        public IHttpActionResult GetDefaultEmployeeAccountDimName()
        {
            return Content(HttpStatusCode.OK, em.GetDefaultEmployeeAccountDimName());
        }

        [HttpPost]
        [Route("Planning/Employee/Availability")]
        public IHttpActionResult GetEmployeeAvailability(ListIntModel model)
        {
            return Content(HttpStatusCode.OK, em.GetEmployeeListAvailability(base.ActorCompanyId, model.Numbers));
        }

        [HttpPost]
        [Route("ValidateEmployeeAccounts")]
        public IHttpActionResult ValidateEmployeeAccounts(ValidateEmployeeAccountsModel model)
        {
                return Content(HttpStatusCode.OK, am.ValidateEmployeeAccounts(model.Accounts, model.MustHaveMainAllocation, model.MustHaveDefault));
        }

        [HttpPost]
        [Route("ValidateSaveEmployee")]
        public IHttpActionResult ValidateSaveEmployee(ValidateSaveEmployeeModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, am.ValidateSaveEmployee(model.EmployeeUser, model.ContactAdresses));
        }

        [HttpGet]
        [Route("ValidateInactivateEmployee/{employeeId:int}")]
        public IHttpActionResult ValidateInactivateEmployee(int employeeId)
        {
            return Content(HttpStatusCode.OK, am.ValidateInactivateEmployee(employeeId));
        }

        [HttpGet]
        [Route("ValidateDeleteEmployee/{employeeId:int}")]
        public IHttpActionResult ValidateDeleteEmployee(int employeeId)
        {
            return Content(HttpStatusCode.OK, am.ValidateDeleteEmployee(employeeId));
        }

        [HttpGet]
        [Route("ValidateImmediateDeleteEmployee/{employeeId:int}")]
        public IHttpActionResult ValidateImmediateDeleteEmployee(int employeeId)
        {
            return Content(HttpStatusCode.OK, am.ValidateImmediateDeleteEmployee(employeeId));
        }

        [HttpPost]
        [Route("")]
        public IHttpActionResult SaveEmployee(SaveEmployeeModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
            {
                return Content(HttpStatusCode.OK, am.SaveEmployeeUser(model.ActionMethod,
                    model.EmployeeUser,
                    contactAddresses: model.ContactAdresses,
                    employeePositions: model.EmployeePositions,
                    employeeSkills: model.EmployeeSkills,
                    userReplacement: model.UserReplacement,
                    employeeTax: model.EmployeeTax,
                    files: model.Files,
                    userRoles: model.UserRoles,
                    saveRoles: model.SaveRoles,
                    saveAttestRoles: model.SaveAttestRoles,
                    generateCurrentChanges: true,
                    doAcceptAttestedTemporaryEmployments: false,
                    logChanges: true,
                    extraFields: model.ExtraFields));
            }
        }

        [HttpPost]
        [Route("SaveEmployeeFromTemplate")]
        public IHttpActionResult SaveEmployeeFromTemplate(SaveEmployeeFromTemplateHeadDTO model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, em.SaveEmployeeFromTemplate(model, base.ActorCompanyId));
        }

        [HttpPost]
        [Route("PrintEmploymentContractFromTemplate")]
        public IHttpActionResult PrintEmploymentContractFromTemplate(PrintEmploymentContractFromTemplateDTO model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, em.PrintEmploymentContractFromTemplate(model.EmployeeId, model.EmployeeTemplateId, model.SubstituteDates, true, base.ActorCompanyId));
        }

        [HttpPost]
        [Route("ValidateInactivateEmployees")]
        public IHttpActionResult ValidateInactivateEmployees(ListIntModel model)
        {
            return Content(HttpStatusCode.OK, am.ValidateInactivateEmployees(model.Numbers, base.ActorCompanyId));
        }

        [HttpPost]
        [Route("InactivateEmployees")]
        public IHttpActionResult InactivateEmployees(ListIntModel model)
        {
            return Content(HttpStatusCode.OK, am.InactivateEmployees(model.Numbers, base.ActorCompanyId));
        }

        [HttpPost]
        [Route("Vacant")]
        public IHttpActionResult CreateVacantEmployees(CreateVacantEmployeesModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, am.CreateVacantEmployees(model.Employees, base.LicenseId, base.ActorCompanyId, base.UserId));
        }

        [HttpPost]
        [Route("MarkAsVacant")]
        public IHttpActionResult MarkAsVacant(ListIntModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, em.MarkEmployeesAsVacant(model.Numbers, base.ActorCompanyId));
        }

        [HttpPost]
        [Route("Note")]
        public IHttpActionResult SaveEmployeeNote(SaveEmployeeNoteModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, em.SaveEmployeeNote(model.Note, model.EmployeeId, base.ActorCompanyId));
        }

        [HttpPost]
        [Route("Delete")]
        public IHttpActionResult DeleteEmployee(DeleteEmployeeDTO model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, em.DeleteEmployee(model));
        }

        #endregion

        #region EmployeeAccount

        [HttpGet]
        [Route("EmployeeAccount/{employeeId:int}/{dateString}")]
        public IHttpActionResult GetEmployeeAccountIds(int employeeId, string dateString)
        {
            return Content(HttpStatusCode.OK, em.GetEmployeeAccountIds(base.ActorCompanyId, employeeId, BuildDateTimeFromString(dateString, true)));
        }

        [HttpGet]
        [Route("EmployeeAccount/Default/{employeeId:int}/{dateString}")]
        public IHttpActionResult GetDefaultEmployeeAccountId(int employeeId, string dateString)
        {
            return Content(HttpStatusCode.OK, em.GetDefaultEmployeeAccountId(base.ActorCompanyId, employeeId, BuildDateTimeFromString(dateString, true)));
        }

        [HttpGet]
        [Route("EmployeeAccount/HasMultiple/{employeeId:int}/{dateFromString}/{dateToString}")]
        public IHttpActionResult HasMultipelEmployeeAccounts(int employeeId, string dateFromString, string dateToString)
        {
            return Content(HttpStatusCode.OK, em.HasMultipelEmployeeAccounts(base.ActorCompanyId, employeeId, BuildDateTimeFromString(dateFromString, true), BuildDateTimeFromString(dateToString, true)));
        }

        [HttpPost]
        [Route("EmployeeAccount/")]
        public IHttpActionResult GetEmployeeAccounts(GetEmployeeAccountsModel model)
        {
            return Content(HttpStatusCode.OK, em.GetEmployeeAccounts(base.ActorCompanyId, model.EmployeeIds, model.DateFrom, model.DateTo).ToDTOs());
        }

        #endregion

        #region EmployeeCalculateVacationResultHead

        [HttpGet]
        [Route("EmployeeCalculateVacationResultHead/Grid")]
        public IHttpActionResult GetEmployeeCalculateVacationHeadsForGrid()
        {
            return Content(HttpStatusCode.OK, pm.GetEmployeeCalculateVacationHeadsForGrid(base.ActorCompanyId));
        }

        #endregion

        #region EmployeeCalculateVacationResult

        [HttpGet]
        [Route("EmployeeCalculateVacationResult/Employee/{employeeCalculateVacationResultHeadId:int}/{employeeId:int}/{onlyActive:bool}")]
        public IHttpActionResult GetEmployeeCalculateVacationResults(int employeeCalculateVacationResultHeadId, int employeeId, bool onlyActive)
        {
            return Content(HttpStatusCode.OK, pm.GetEmployeeCalculateVacationResults(employeeCalculateVacationResultHeadId, employeeId, onlyActive).ToDTOs());
        }

        [HttpPost]
        [Route("EmployeeCalculateVacationResult/Values/")]
        public IHttpActionResult SaveEmployeeCalculateVacationResultValues(UpdateEmployeeCalculateVacationResultModel model)
        {
            return Content(HttpStatusCode.OK, pm.SaveEmployeeCalculationResultValues(model.EmployeeCalculateVacationResultHeadId, model.EmployeeId, model.Results));
        }

        [HttpDelete]
        [Route("EmployeeCalculateVacationResultHead/{employeeCalculateVacationResultHeadId:int}")]
        public IHttpActionResult DeleteEmployeeCalculateVacationResultHead(int employeeCalculateVacationResultHeadId)
        {
            return Content(HttpStatusCode.OK, pm.DeleteEmployeeCalculateVacationResultHead(base.ActorCompanyId, employeeCalculateVacationResultHeadId));
        }

        [HttpDelete]
        [Route("EmployeeCalculateVacationResult/Employee/{employeeCalculateVacationResultHeadId:int}/{employeeId:int}")]
        public IHttpActionResult DeleteEmployeeCalculateVacationResultsForEmployee(int employeeCalculateVacationResultHeadId, int employeeId)
        {
            return Content(HttpStatusCode.OK, pm.DeleteEmployeeCalculateVacationResultsForEmployee(employeeCalculateVacationResultHeadId, employeeId));
        }
        #endregion

        #region EmployeeChild

        [HttpGet]
        [Route("EmployeeChild")]
        public IHttpActionResult GetEmployeeChilds(HttpRequestMessage message)
        {
            if (message.HasAcceptValue(HttpExtensions.ACCEPT_GENERIC_TYPE))
                return Content(HttpStatusCode.OK, em.GetEmployeeChildsDict(message.GetIntValueFromQS("employeeId"), message.GetBoolValueFromQS("addEmptyRow")).ToSmallGenericTypes());

            return Content(HttpStatusCode.OK, em.GetEmployeeChilds(message.GetIntValueFromQS("employeeId"), base.ActorCompanyId).ToDTOs());
        }

        #endregion

        #region EmployeeCollectiveAgreement

        [HttpGet]
        [Route("EmployeeCollectiveAgreement")]
        public IHttpActionResult GetEmployeeCollectiveAgreements(HttpRequestMessage message)
        {
            if (message.HasAcceptValue(HttpExtensions.ACCEPT_GENERIC_TYPE))
                return Content(HttpStatusCode.OK, em.GetEmployeeCollectiveAgreementsDict(base.ActorCompanyId, message.GetBoolValueFromQS("addEmptyRow")).ToSmallGenericTypes());
            else if (message.HasAcceptValue(HttpExtensions.ACCEPT_GRID_DTO))
                return Content(HttpStatusCode.OK, em.GetEmployeeCollectiveAgreements(base.ActorCompanyId, false, true, true, true, true, true).ToGridDTOs());

            return Content(HttpStatusCode.OK, em.GetEmployeeCollectiveAgreements(base.ActorCompanyId).ToDTOs());
        }

        [HttpGet]
        [Route("EmployeeCollectiveAgreement/{employeeCollectiveAgreementId:int}")]
        public IHttpActionResult GetEmployeeCollectiveAgreement(int employeeCollectiveAgreementId)
        {
            return Content(HttpStatusCode.OK, em.GetEmployeeCollectiveAgreement(base.ActorCompanyId, employeeCollectiveAgreementId).ToDTO());
        }

        [HttpPost]
        [Route("EmployeeCollectiveAgreement")]
        public IHttpActionResult SaveEmployeeCollectiveAgreement(EmployeeCollectiveAgreementDTO employeeCollectiveAgreementDTO)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, em.UpsertEmployeeCollectiveAgreement(employeeCollectiveAgreementDTO, base.ActorCompanyId));
        }

        [HttpDelete]
        [Route("EmployeeCollectiveAgreement/{employeeCollectiveAgreementId:int}")]
        public IHttpActionResult DeleteEmployeeCollectiveAgreement(int employeeCollectiveAgreementId)
        {
            return Content(HttpStatusCode.OK, em.DeleteEmployeeCollectiveAgreement(employeeCollectiveAgreementId));
        }

        #endregion

        #region EmployeeGroup

        [HttpGet]
        [Route("EmployeeGroup")]
        public IHttpActionResult GetEmployeeGroups(HttpRequestMessage message)
        {
            if (message.HasAcceptValue(HttpExtensions.ACCEPT_GENERIC_TYPE))
                return Content(HttpStatusCode.OK, em.GetEmployeeGroupsDict(base.ActorCompanyId, message.GetBoolValueFromQS("addEmptyRow")).ToSmallGenericTypes());
            else if (message.HasAcceptValue(HttpExtensions.ACCEPT_SMALL_DTO))
                return Content(HttpStatusCode.OK, em.GetEmployeeGroups(base.ActorCompanyId).ToSmallDTOs());

            Dictionary<int, string> timeReportTypes = em.GetTermGroupContent(TermGroup.TimeReportType).ToDictionary();
            return Content(HttpStatusCode.OK, em.GetEmployeeGroups(base.ActorCompanyId, loadTimeDeviationCauseMappings: true, loadTimeDeviationCauses: true, loadDayTypes: true).ToDTOs(false, timeReportTypes));
        }

        [HttpGet]
        [Route("EmployeeGroup/{employeeGroupId:int}")]
        public IHttpActionResult GetEmployeeGroup(int employeeGroupId)
        {
            if (Request.HasAcceptValue(HttpExtensions.ACCEPT_SMALL_DTO))
                return Content(HttpStatusCode.OK, em.GetEmployeeGroup(employeeGroupId).ToSmallDTO());

            return Content(HttpStatusCode.OK, em.GetEmployeeGroup(employeeGroupId).ToDTO(false));
        }

        [HttpGet]
        [Route("EmployeeGroup/{employeeId:int}/{dateString}")]
        public IHttpActionResult GetEmployeeGroupId(int employeeId, string dateString)
        {
            EmployeeGroup group = em.GetEmployeeGroupForEmployee(employeeId, base.ActorCompanyId, base.BuildDateTimeFromString(dateString, true).Value);
            return Content(HttpStatusCode.OK, group != null ? group.EmployeeGroupId : 0);
        }

        #endregion

        #region EmployeeMeeting

        [HttpGet]
        [Route("EmployeeMeeting/{employeeId:int}/{userId:int}")]
        public IHttpActionResult GetEmployeeMeetings(int employeeId, int userId)
        {
            return Content(HttpStatusCode.OK, em.GetEmployeeMeetings(employeeId, base.ActorCompanyId, userId, true).ToDTOs(true));
        }

        #endregion

        #region EmployeeSetting

        [HttpGet]
        [Route("EmployeeSetting/Area/{area:int}")]
        public IHttpActionResult GetAvailableEmployeeSettingsByArea(int area)
        {
            return Content(HttpStatusCode.OK, em.GetAvailableEmployeeSettingsByArea(base.ActorCompanyId, (TermGroup_EmployeeSettingType)area));
        }

        #endregion

        #region EmployeeSkill

        [HttpGet]
        [Route("EmployeeSkill/Search")]
        public IHttpActionResult SearchEmployeeSkills(HttpRequestMessage message)
        {
            string employeeNrFrom = message.GetStringValueFromQS("employeeNrFrom");
            string employeeNrTo = message.GetStringValueFromQS("employeeNrTo");
            int categoryId = message.GetIntValueFromQS("categoryId");
            int positionId = message.GetIntValueFromQS("positionId");
            int skillId = message.GetIntValueFromQS("skillId");
            string endDate = message.GetStringValueFromQS("endDate");
            bool getMissingSkill = message.GetBoolValueFromQS("getMissingSkill");
            bool getMissingPosition = message.GetBoolValueFromQS("getMissingPosition");
            int accountId = message.GetIntValueFromQS("accountId");
            return Content(HttpStatusCode.OK, tsm.SearchEmployeeSkills(base.ActorCompanyId, employeeNrFrom, employeeNrTo, categoryId, positionId, skillId, base.BuildDateTimeFromString(endDate, true), getMissingSkill, getMissingPosition, accountId));
        }

        #endregion

        #region EmployeeStatistics

        [HttpGet]
        [Route("EmployeeStatistics/EmployeeData/{employeeId:int}/{dateFrom}/{dateTo}/{type:int}")]
        public IHttpActionResult GetEmployeeStatisticsEmployeeData(int employeeId, string dateFrom, string dateTo, int type)
        {
            return Content(HttpStatusCode.OK, em.GetEmployeeStatisticsEmployeeData(employeeId, base.BuildDateTimeFromString(dateFrom, true).Value, base.BuildDateTimeFromString(dateTo, true).Value, (TermGroup_EmployeeStatisticsType)type));
        }

        #endregion

        #region EmployeeTax

        [HttpGet]
        [Route("EmployeeTax/{employeeTaxId:int}")]
        public IHttpActionResult GetEmployeeTaxSE(int employeeTaxId)
        {
            return Content(HttpStatusCode.OK, em.GetEmployeeTaxSE(employeeTaxId));
        }

        [HttpGet]
        [Route("EmployeeTax/{employeeId:int}/{year:int}")]
        public IHttpActionResult GetEmployeeTaxSEByYear(int employeeId, int year)
        {
            return Content(HttpStatusCode.OK, em.GetEmployeeTaxSE(employeeId, year).ToDTO());
        }

        [HttpGet]
        [Route("EmployeeTax/Years/{employeeId:int}")]
        public IHttpActionResult GetEmployeeTaxSEYears(int employeeId)
        {
            return Content(HttpStatusCode.OK, em.GetEmployeeTaxSEYears(employeeId));
        }

        [HttpGet]
        [Route("EmployeeTax/SchoolYouthLimit/CalculateRemaining/{schoolYouthLimitInitial:decimal}/{schoolYouthLimitUsed:decimal}/{date}")]
        public IHttpActionResult CalculateSchoolYouthLimitRemaining(decimal schoolYouthLimitInitial, decimal schoolYouthLimitUsed, string date)
        {
            decimal sysPayrollPriceSchoolYouthLimit = pm.GetSysPayrollPriceAmount(base.ActorCompanyId, (int)TermGroup_SysPayrollPrice.SE_SchoolYouthLimit, BuildDateTimeFromString(date, true, CalendarUtility.DATETIME_DEFAULT).Value);
            return Content(HttpStatusCode.OK, PayrollRulesUtil.CalculateSchoolYouthLimitRemaining(sysPayrollPriceSchoolYouthLimit, schoolYouthLimitInitial, schoolYouthLimitUsed));
        }

        [HttpGet]
        [Route("EmployeeTax/SchoolYouthLimit/CalculateUsed/{employeeId:int}/{date}")]
        public IHttpActionResult CalculateSchoolYouthLimitUsed(int employeeId, string date)
        {
            return Content(HttpStatusCode.OK, pm.GetSchoolYouthLimitUsed(base.ActorCompanyId, employeeId, BuildDateTimeFromString(date, true, CalendarUtility.DATETIME_DEFAULT).Value));
        }

        #endregion

        #region EmployeeTemplate

        [HttpGet]
        [Route("EmployeeTemplate")]
        public IHttpActionResult GetEmployeeTemplates(HttpRequestMessage message)
        {
            if (message.HasAcceptValue(HttpExtensions.ACCEPT_GENERIC_TYPE))
                return Content(HttpStatusCode.OK, em.GetEmployeeTemplatesDict(base.ActorCompanyId, message.GetBoolValueFromQS("addEmptyRow")).ToSmallGenericTypes());
            else if (message.HasAcceptValue(HttpExtensions.ACCEPT_GRID_DTO))
                return Content(HttpStatusCode.OK, em.GetEmployeeTemplates(base.ActorCompanyId, true).ToGridDTOs().OrderBy(t => t.EmployeeCollectiveAgreementName).ThenBy(t => t.Name));

            return Content(HttpStatusCode.OK, em.GetEmployeeTemplates(base.ActorCompanyId).ToDTOs());
        }

        [HttpGet]
        [Route("EmployeeTemplate/OfTypeSubstituteShifts")]
        public IHttpActionResult GetEmployeeTemplatesOfTypeSubstituteShifts(HttpRequestMessage message)
        {
            return Content(HttpStatusCode.OK, em.GetEmployeeTemplatesOfTypeSubstituteShifts(base.ActorCompanyId).ToSmallGenericTypes());
        }

        [HttpGet]
        [Route("EmployeeTemplate/{employeeTemplateId:int}")]
        public IHttpActionResult GetEmployeeTemplate(int employeeTemplateId)
        {
            return Content(HttpStatusCode.OK, em.GetEmployeeTemplate(base.ActorCompanyId, employeeTemplateId).ToDTO());
        }

        [HttpGet]
        [Route("EmployeeTemplate/HasEmployeeTemplates")]
        public IHttpActionResult HasEmployeeTemplates()
        {
            return Content(HttpStatusCode.OK, em.HasEmployeeTemplates(base.ActorCompanyId));
        }

        [HttpGet]
        [Route("EmployeeTemplate/HasEmployeeTemplatesOfTypeSubstituteShifts")]
        public IHttpActionResult HasEmployeeTemplatesOfTypeSubstituteShifts()
        {
            return Content(HttpStatusCode.OK, em.HasEmployeeTemplatesOfTypeSubstituteShifts(base.ActorCompanyId));
        }

        [HttpPost]
        [Route("EmployeeTemplate")]
        public IHttpActionResult SaveEmployeeTemplate(EmployeeTemplateDTO employeeTemplateDTO)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, em.SaveEmployeeTemplate(employeeTemplateDTO, base.ActorCompanyId));
        }

        [HttpDelete]
        [Route("EmployeeTemplate/{employeeTemplateId:int}")]
        public IHttpActionResult DeleteEmployeeTemplate(int employeeTemplateId)
        {
            return Content(HttpStatusCode.OK, em.DeleteEmployeeTemplate(employeeTemplateId));
        }

        #endregion

        #region EmployeeTimePeriod

        [Route("EmployeeTimePeriod/{employeeId:int}/{timePeriodId:int}")]
        public IHttpActionResult GetEmployeeTimePeriod(int employeeId, int timePeriodId)
        {
            return Content(HttpStatusCode.OK, tpm.GetEmployeeTimePeriod(employeeId, timePeriodId, base.ActorCompanyId).ToDTO());
        }

        #endregion

        #region EmployeeVehicle

        [HttpGet]
        [Route("EmployeeVehicle/{loadEmployee:bool}/{loadDeduction:bool}/{loadEquipment:bool}/{loadTax:bool}")]
        public IHttpActionResult GetEmployeeVehicles(bool loadEmployee, bool loadDeduction, bool loadEquipment, bool loadTax)
        {
            return Content(HttpStatusCode.OK, em.GetEmployeeVehicles(base.ActorCompanyId, loadEmployee, loadDeduction, loadEquipment, loadTax, true).ToGridDTOs());
        }

        [HttpGet]
        [Route("EmployeeVehicle/{employeeVehicleId:int}/{loadEmployee:bool}/{loadDeduction:bool}/{loadEquipment:bool}/{loadTax:bool}")]
        public IHttpActionResult GetEmployeeVehicle(int employeeVehicleId, bool loadEmployee, bool loadDeduction, bool loadEquipment, bool loadTax)
        {
            return Content(HttpStatusCode.OK, em.GetEmployeeVehicle(employeeVehicleId, loadEmployee, loadDeduction, loadEquipment, loadTax).ToDTO(loadDeduction, loadEquipment, loadTax));
        }

        [HttpPost]
        [Route("EmployeeVehicle")]
        public IHttpActionResult SaveEmployeeVehicle(EmployeeVehicleDTO employeeVehicle)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, em.SaveEmployeeVehicle(employeeVehicle));
        }

        [HttpDelete]
        [Route("EmployeeVehicle/{employeeVehicleId:int}")]
        public IHttpActionResult DeleteEmployeeVehicle(int employeeVehicleId)
        {
            return Content(HttpStatusCode.OK, em.DeleteEmployeeVehicle(employeeVehicleId));
        }

        #endregion

        #region Employment

        [HttpGet]
        [Route("Employment/{employeeId:int}/{date}")]
        public IHttpActionResult GetEmployments(int employeeId, string date)
        {
            return Content(HttpStatusCode.OK, am.GetEmployments(employeeId, base.ActorCompanyId, BuildDateTimeFromString(date, true), loadEmploymentAccounting: true));
        }

        [HttpPost]
        [Route("TimeEmploymentContractUrl")]
        public IHttpActionResult GetTimeEmploymentContractUrl(PayrollGroupContractReportPrintModel model)
        {
            if (!model.DateFrom.HasValue)
                model.DateFrom = DateTime.MinValue;
            if (!model.DateTo.HasValue)
                model.DateTo = DateTime.MaxValue;

            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, rm.GetTimeEmploymentContractPrintUrl(base.ActorCompanyId, model.EmployeeId, model.EmploymentId, model.ReportId, model.ReportTemplateTypeId, model.DateFrom.Value, model.DateTo.Value, model.DateChanges, model.PrintedFromScheduleplanning));
        }

        [HttpGet]
        [Route("Employment/ExperienceMonths/{employmentId:int}/{date}")]
        public IHttpActionResult GetExperienceMonthsForEmployment(int employmentId, string date)
        {
            return Content(HttpStatusCode.OK, em.GetExperienceMonthsForEmployment(base.ActorCompanyId, employmentId, BuildDateTimeFromString(date, true)));
        }

        [HttpGet]
        [Route("Employment/ExperienceMonths/PreviousEmployent/{currentEmploymentId:int}")]
        public IHttpActionResult GetExperienceMonthsForPreviousEmployent(int currentEmploymentId)
        {
            return Content(HttpStatusCode.OK, em.GetExperienceMonthsForPreviousEmployent(base.ActorCompanyId, currentEmploymentId));
        }

        [HttpGet]
        [Route("ExperienceMonths/{employeeId:int}/{date}")]
        public IHttpActionResult GetExperienceMonthsForEmployee(int employeeId, string date)
        {
            return Content(HttpStatusCode.OK, em.GetExperienceMonthsForEmployee(base.ActorCompanyId, employeeId, BuildDateTimeFromString(date, true)));
        }

        #endregion

        #region EmploymentType

        [HttpGet]
        [Route("EmploymentType")]
        public IHttpActionResult GetEmploymentTypesForGrid()
        {
            return Content(HttpStatusCode.OK, em.GetEmploymentTypesFromDB(base.ActorCompanyId).OrderByDescending(x => x.Standard).ThenBy(x => x.Type).ThenBy(x => x.Name));
        }

        [HttpGet]
        [Route("EmploymentType/{employmentTypeId:int}")]
        public IHttpActionResult GetEmploymentType(int employmentTypeId)
        {
            return Content(HttpStatusCode.OK, em.GetEmploymentTypesFromDB(base.ActorCompanyId).GetEmploymentType(employmentTypeId));
        }

        [HttpGet]
        [Route("EmploymentType/Employment/{language:int}")]
        public IHttpActionResult GetEmploymentTypes(int language)
        {
            return Content(HttpStatusCode.OK, em.GetEmploymentTypes(base.ActorCompanyId, (TermGroup_Languages)language).ToSmallEmploymentTypes());
        }

        [HttpGet]
        [Route("EmploymentType/Standard/{language:int}")]
        public IHttpActionResult GetStandardEmploymentTypes(int language)
        {
            return Content(HttpStatusCode.OK, em.GetStandardEmploymentTypes(base.ActorCompanyId, (TermGroup_Languages)language).ToSmallEmploymentTypes());
        }

        [HttpPost]
        [Route("EmploymentType")]
        public IHttpActionResult SaveEmploymentType(EmploymentTypeDTO employmentTypeDTO)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, em.SaveEmploymentType(employmentTypeDTO, base.ActorCompanyId));
        }

        [HttpPost]
        [Route("EmploymentType/UpdateState")]
        public IHttpActionResult UpdateEmploymentTypesState(UpdateEntityStatesModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, em.UpdateEmploymentTypesState(model.Dict, base.ActorCompanyId));
        }

        [HttpDelete]
        [Route("EmploymentType/{employmentTypeId:int}")]
        public IHttpActionResult DeleteEmploymentType(int employmentTypeId)
        {
            return Content(HttpStatusCode.OK, em.DeleteEmploymentType(base.ActorCompanyId, employmentTypeId));
        }

        #endregion

        #region EndReason

        [HttpGet]
        [Route("EndReason")]
        public IHttpActionResult GetEndReasons()
        {
            return Content(HttpStatusCode.OK, em.GetEndReasons(base.ActorCompanyId));
        }

        [HttpGet]
        [Route("EndReason/{endReasonId:int}")]
        public IHttpActionResult GetEndReason(int endReasonId)
        {
            return Content(HttpStatusCode.OK, em.GetCompanyEndReason(base.ActorCompanyId, endReasonId).ToDTO());
        }

        [HttpGet]
        [Route("EndReason/Employment/{language:int}")]
        public IHttpActionResult GetEmploymentEndReasons(int language)
        {
            return Content(HttpStatusCode.OK, em.GetSystemEndReasons(base.ActorCompanyId, language: language, includeCompanyEndReasons: true, active: true).ToSmallGenericTypes());
        }

        [HttpPost]
        [Route("EndReason")]
        public IHttpActionResult SaveEndReason(EndReasonDTO endReasonDTO)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, em.SaveEndReason(endReasonDTO, base.ActorCompanyId));
        }

        [HttpPost]
        [Route("EndReason/UpdateState")]
        public IHttpActionResult UpdateEndReasonsState(UpdateEntityStatesModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, em.UpdateEndReasonsState(model.Dict, base.ActorCompanyId));
        }

        [HttpDelete]
        [Route("EndReason/{endReasonId:int}")]
        public IHttpActionResult DeleteEndReason(int endReasonId)
        {
            return Content(HttpStatusCode.OK, em.DeleteEndReason(base.ActorCompanyId, endReasonId));
        }

        #endregion

        #region FollowUpType

        [HttpGet]
        [Route("FollowUpType")]
        public IHttpActionResult GetFollowUpTypes()
        {
            return Content(HttpStatusCode.OK, em.GetFollowUpTypes(base.ActorCompanyId).ToGridDTOs());
        }

        [HttpGet]
        [Route("FollowUpType/{followUpTypeId:int}")]
        public IHttpActionResult GetFollowUpType(int followUpTypeId)
        {
            return Content(HttpStatusCode.OK, em.GetFollowUpType(base.ActorCompanyId, followUpTypeId).ToDTO());
        }

        [HttpPost]
        [Route("FollowUpType")]
        public IHttpActionResult SaveFollowUpType(FollowUpTypeDTO followUpTypeDTO)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, em.SaveFollowUpType(followUpTypeDTO, base.ActorCompanyId));
        }

        [HttpPost]
        [Route("FollowUpType/UpdateState")]
        public IHttpActionResult UpdateFollowUpTypesState(UpdateEntityStatesModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, em.UpdateFollowUpTypesState(model.Dict, base.ActorCompanyId));
        }

        [HttpDelete]
        [Route("FollowUpType/{followUpTypeId:int}")]
        public IHttpActionResult DeleteFollowUpType(int followUpTypeId)
        {
            return Content(HttpStatusCode.OK, em.DeleteFollowUpType(base.ActorCompanyId, followUpTypeId));
        }

        #endregion

        #region PayrollReview

        [HttpGet]
        [Route("PayrollReviewHead/{loadRows:bool}/{loadPayrollGroups:bool}/{loadPayrollPriceTypes:bool}/{loadPayrollLevels:bool}/{setStatusName:bool}")]
        public IHttpActionResult GetPayrollReviewHeads(bool loadRows, bool loadPayrollGroups, bool loadPayrollPriceTypes, bool loadPayrollLevels, bool setStatusName)
        {
            return Content(HttpStatusCode.OK, pm.GetPayrollReviewHeads(base.ActorCompanyId, loadRows, loadPayrollGroups, loadPayrollPriceTypes, loadPayrollLevels, setStatusName).ToDTOs(loadRows));
        }

        [HttpGet]
        [Route("PayrollReviewHead/{payrollReviewHeadId:int}/{loadRows:bool}/{loadPayrollGroups:bool}/{loadPayrollPriceTypes:bool}/{loadPayrollLevels:bool}/{setStatusName:bool}")]
        public IHttpActionResult GetPayrollReviewHead(int payrollReviewHeadId, bool loadRows, bool loadPayrollGroups, bool loadPayrollPriceTypes, bool loadPayrollLevels, bool setStatusName)
        {
            return Content(HttpStatusCode.OK, pm.GetPayrollReviewHead(payrollReviewHeadId, loadRows, loadPayrollGroups, loadPayrollPriceTypes, loadPayrollLevels, setStatusName).ToDTO(loadRows));
        }

        [HttpPost]
        [Route("PayrollReviewHead/")]
        public IHttpActionResult SavePayrollReview(PayrollReviewHeadDTO payrollReviewInput)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, pm.SavePayrollReview(payrollReviewInput));
        }

        [HttpPost]
        [Route("PayrollReviewHead/Employees/")]
        public IHttpActionResult GetEmployeesForPayrollReview(PayrollReviewEmployeeModel model)
        {
            return Content(HttpStatusCode.OK, pm.GetEmployeesForPayrollReview(base.ActorCompanyId, base.UserId, base.RoleId, model.FromDate, model.PayrollGroupIds, model.PayrollPriceTypeIds, model.PayrollLevelIds, model.EmployeeIds));
        }

        [HttpPost]
        [Route("PayrollReviewHead/Update/{payrollReviewHeadId:int}/{keepFuture:bool}")]
        public IHttpActionResult UpdatePayrollReview(int payrollReviewHeadId, bool keepFuture)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, pm.UpdatePayrollReview(payrollReviewHeadId, keepFuture));
        }

        [HttpPost]
        [Route("PayrollReviewHead/Export/")]
        public IHttpActionResult ExportToExcel(PayrollReviewHeadDTO head)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, pm.ExportPayrollReview(head));
        }

        [HttpPost]
        [Route("PayrollReviewHead/Import/{dateFrom}")]
        public async Task<IHttpActionResult> ImportFromExcel(string dateFrom)
        {
            if (Request.Content.IsMimeMultipartContent())
            {
                var data = await Request.Content.ParseMultipartAsync();

                HttpPostedFile file = data.Files["file"];
                if (file != null)
                    return Content(HttpStatusCode.OK, pm.ImportPayrollReview(new MemoryStream(file.File), BuildDateTimeFromString(dateFrom, true).Value, base.ActorCompanyId));
            }

            throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
        }

        [HttpDelete]
        [Route("PayrollReviewHead/{payrollReviewHeadId:int}")]
        public IHttpActionResult DeletePayrollReview(int payrollReviewHeadId)
        {
            return Content(HttpStatusCode.OK, pm.DeletePayrollReview(payrollReviewHeadId));
        }

        #endregion

        #region EmployeePosition

        [HttpGet]
        [Route("EmployeePosition/{employeeId:int}/{loadSysPosition:bool}")]
        public IHttpActionResult GetEmployeePositions(int employeeId, bool loadSysPosition)
        {
            return Content(HttpStatusCode.OK, em.GetEmployeePositions(employeeId, loadSysPosition).ToDTOs());
        }

        #endregion

        #region Position

        [HttpGet]
        [Route("Position")]
        public IHttpActionResult GetPositions(HttpRequestMessage message)
        {
            bool loadSkills = message.GetBoolValueFromQS("loadSkills");

            if (message.HasAcceptValue(HttpExtensions.ACCEPT_GENERIC_TYPE))
                return Content(HttpStatusCode.OK, em.GetPositionsDict(base.ActorCompanyId, message.GetBoolValueFromQS("addEmptyRow")).ToSmallGenericTypes());
            else if (message.HasAcceptValue(HttpExtensions.ACCEPT_GRID_DTO))
                return Content(HttpStatusCode.OK, em.GetPositions(base.ActorCompanyId, loadSkills).ToGridDTOs());

            return Content(HttpStatusCode.OK, em.GetPositions(base.ActorCompanyId, loadSkills).ToDTOs(loadSkills));
        }

        [HttpGet]
        [Route("Position/{employeePositionId:int}/{loadSkills:bool}")]
        public IHttpActionResult GetPosition(int employeePositionId, bool loadSkills)
        {
            return Content(HttpStatusCode.OK, em.GetPositionIncludingSkill(employeePositionId).ToDTO(loadSkills));
        }

        [HttpPost]
        [Route("Position")]
        public IHttpActionResult SavePosition(PositionDTO position)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, em.SavePosition(position, position.PositionSkills, base.ActorCompanyId));
        }

        [HttpPost]
        [Route("PositionGridUpdate")]
        public IHttpActionResult UpdatePositionGrid()
        {
            return Content(HttpStatusCode.OK, em.UpdateAllLinkedPositions(base.ActorCompanyId));
        }

        [HttpPost]
        [Route("SysPositionGridUpdateAndLink")]
        public IHttpActionResult UpdateAndLinkSysPositionGrid(List<SysPositionGridDTO> sysPositions)
        {
            return Content(HttpStatusCode.OK, em.CopyAndLinkSysPositions(sysPositions, true, base.ActorCompanyId));
        }

        [HttpPost]
        [Route("SysPositionGridUpdate")]
        public IHttpActionResult UpdateSysPositionGrid(List<SysPositionGridDTO> sysPositions)
        {
            return Content(HttpStatusCode.OK, em.CopyAndLinkSysPositions(sysPositions, false, base.ActorCompanyId));
        }

        [HttpDelete]
        [Route("Position/{positionId:int}")]
        public IHttpActionResult DeletePosition(int positionId)
        {
            return Content(HttpStatusCode.OK, em.DeletePosition(positionId));
        }
        #endregion

        #region Vacation

        [HttpGet]
        [Route("Vacation/{employeeId:int}")]
        public IHttpActionResult GetEmployeeVacation(int employeeId)
        {
            return Content(HttpStatusCode.OK, em.GetLatestEmployeeVacationSE(employeeId).ToDTO());
        }

        [HttpGet]
        [Route("Vacation/GetPrelUsedVacationDays/{employeeId:int}/{dateString}")]
        public IHttpActionResult GetPrelUsedVacationDays(int employeeId, string dateString)
        {
            return Content(HttpStatusCode.OK, pm.GetEmployeeVacationPrelUsedDays(base.ActorCompanyId, employeeId, BuildDateTimeFromString(dateString, true).Value));
        }

        [HttpGet]
        [Route("Vacation/GetPrelPayedDaysYear1/{employeeId:int}")]
        public IHttpActionResult GetPrelPayedDaysYear1(int employeeId)
        {
            decimal? days = em.GetPrelPayedDaysYear1(employeeId, base.ActorCompanyId);
            return Content(HttpStatusCode.OK, days ?? 0);
        }

        #endregion

        #region VacationGroup

        [HttpGet]
        [Route("VacationGroup/{loadTypeNames:bool}/{loadOnlyActive:bool}")]
        public IHttpActionResult GetVacationGroups(bool loadTypeNames, bool loadOnlyActive)
        {
            return Content(HttpStatusCode.OK, pm.GetVacationGroups(base.ActorCompanyId, loadTypeNames, loadOnlyActive).ToGridDTOs());
        }

        [HttpGet]
        [Route("VacationGroup/{vacationGroupId:int}")]
        public IHttpActionResult GetVacationGroup(int vacationGroupId)
        {
            return Content(HttpStatusCode.OK, pm.GetVacationGroup(vacationGroupId, setLatestVacationYearEnd: true, loadExternalCode: true).ToDTO());
        }

        [HttpGet]
        [Route("VacationGroup/Employee/{employeeId:int}/{dateString}")]
        public IHttpActionResult GetVacationGroupForEmployee(int employeeId, string dateString)
        {
            return Content(HttpStatusCode.OK, pm.GetVacationGroupForEmployee(base.ActorCompanyId, employeeId, BuildDateTimeFromString(dateString, true)).ToDTO());
        }

        [HttpDelete]
        [Route("DeleteVacationGroup/{vacationGroupId:int}")]
        public IHttpActionResult DeleteVacationGroup(int vacationGroupId)
        {
            return Content(HttpStatusCode.OK, pm.DeleteVacationGroup(vacationGroupId));
        }

        [HttpPost]
        [Route("VacationGroup")]
        public IHttpActionResult SaveVacationGroup(VacationGroupDTO vacationGroup)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, pm.SaveVacationGroup(vacationGroup, vacationGroup.VacationGroupSE, base.ActorCompanyId));
        }

        #endregion
    }
}
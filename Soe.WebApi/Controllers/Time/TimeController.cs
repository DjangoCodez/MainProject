using Soe.WebApi.Binders;
using Soe.WebApi.Extensions;
using Soe.WebApi.Models;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Core.TimeEngine;
using SoftOne.Soe.Business.Core.TimeTree;
using SoftOne.Soe.Business.Util;
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
using System.Web.Http.ModelBinding;

namespace Soe.WebApi.Controllers.Time
{
    [RoutePrefix("Time/Time")]
    public class TimeController : SoeApiController
    {
        #region Variables

        private readonly AttestManager am;
        private readonly TimeTreeAttestManager ttam;
        private readonly TimeTreePayrollManager ttpm;
        private readonly CommunicationManager cm;
        private readonly EmailManager emm;
        private readonly SettingManager sm;
        private readonly TimeAccumulatorManager tam;
        private readonly TimeBlockManager tbm;
        private readonly TimeCodeManager tcm;
        private readonly TimeDeviationCauseManager tdcm;
        private readonly TimeEngineManager tem;
        private readonly TimePeriodManager tpm;
        private readonly TimeRuleManager trm;
        private readonly TimeTransactionManager ttm;
        private readonly TimeSalaryManager tsam;
        private readonly TimeStampManager tsm;
        private readonly TimeHibernatingManager thm;

        #endregion

        #region Constructor

        public TimeController(AttestManager am, TimeTreeAttestManager ttam, TimeTreePayrollManager ttpm, CommunicationManager cm, EmailManager emm, SettingManager sm, TimeAccumulatorManager tam, TimeCodeManager tcm, TimeDeviationCauseManager tdcm, TimeEngineManager tem, TimeBlockManager tbm, TimePeriodManager tpm, TimeRuleManager trm, TimeTransactionManager ttm, TimeSalaryManager tsam, TimeStampManager tsm, TimeHibernatingManager thm)
        {
            this.am = am;
            this.ttam = ttam;
            this.ttpm = ttpm;
            this.cm = cm;
            this.emm = emm;
            this.sm = sm;
            this.tam = tam;
            this.tbm = tbm;
            this.tcm = tcm;
            this.tdcm = tdcm;
            this.tem = tem;
            this.tpm = tpm;
            this.trm = trm;
            this.ttm = ttm;
            this.tsam = tsam;
            this.tsm = tsm;
            this.thm = thm;
        }

        #endregion

        #region Attest (Time)

        [HttpPost]
        [Route("Attest/Tree/")]
        public IHttpActionResult GetTimeAttestTree(AttestTreeModel model)
        {
            model.Beautify();
            return Content(HttpStatusCode.OK, ttam.GetAttestTree(model.Grouping, model.Sorting, model.StartDate, model.StopDate, model.TimePeriodId.ToNullable(), model.Settings));
        }

        [HttpPost]
        [Route("Attest/Tree/Refresh/")]
        public IHttpActionResult RefreshAttestTree(RefreshAttestTreeModel model)
        {
            return Content(HttpStatusCode.OK, ttam.RefreshAttestTree(model.Tree, model.StartDate, model.StopDate, model.TimePeriodId.ToNullable(), model.Settings));
        }

        [HttpPost]
        [Route("Attest/Tree/Refresh/Group")]
        public IHttpActionResult RefreshAttestTreeGroupNode(RefreshAttestTreeGroupNodeModel model)
        {
            return Content(HttpStatusCode.OK, ttam.RefreshAttestTreeGroupNode(model.Tree, model.GroupNode));
        }

        [HttpPost]
        [Route("Attest/Tree/Warnings/")]
        public IHttpActionResult GetAttestTreeWarnings(GetAttestTreeWarningsModel model)
        {
            return Content(HttpStatusCode.OK, ttam.GetAttestTreeWarnings(model.Tree, model.StartDate, model.StopDate, model.EmployeeIds, model.TimePeriodId.ToNullable(), model.DoShowOnlyWithWarnings, model.FlushCache));
        }

        [HttpGet]
        [Route("Attest/MessageGroup/")]
        public IHttpActionResult GetAttestTreeMessageGroups()
        {
            return Content(HttpStatusCode.OK, ttam.GetAttestTreeMessageGroups());
        }

        [HttpGet]
        [Route("Attest/EmployeeDays/{gridName}/{employeeId:int}/{dateFrom}/{dateTo}/{hasDayFilter:bool}/{includeProjectTimeBlocks:bool}/{includeShifts:bool}/{includeTimeStamps:bool}/{includeTimeBlocks:bool}/{includeTimeCodeTransactions}/{includeTimeInvoiceTransactions}/{doNotShowDaysOutsideEmployeeAccount}/{filterAccountIds}/{cacheKeyToUse}")]
        public IHttpActionResult GetAttestEmployeeDays(string gridName, int employeeId, string dateFrom, string dateTo, bool hasDayFilter, bool includeProjectTimeBlocks, bool includeShifts, bool includeTimeStamps, bool includeTimeBlocks, bool includeTimeCodeTransactions, bool includeTimeInvoiceTransactions, bool doNotShowDaysOutsideEmployeeAccount, string filterAccountIds, string cacheKeyToUse)
        {
            List<AgGridColumnSettingDTO> columnSettings = sm.GetAgGridSettings(gridName);

            var startDate = BuildDateTimeFromString(dateFrom, true, CalendarUtility.DATETIME_DEFAULT).Value;
            var stopDate = BuildDateTimeFromString(dateTo, true, CalendarUtility.DATETIME_DEFAULT).Value;
            var input = GetAttestEmployeeInput.CreateAttestInputForWeb(base.ActorCompanyId, base.UserId, base.RoleId, employeeId, startDate, stopDate, null, cacheKeyToUse);
            input.SetOptionalParameters(
                validateEmployee: true, 
                doNotShowDaysOutsideEmployeeAccount: doNotShowDaysOutsideEmployeeAccount, 
                filterAccountIds: StringUtility.SplitNumericList(filterAccountIds, skipZero: true),
                hasDayFilter: hasDayFilter
                );

            bool includeDaytypes = true;
            bool includeTemplateSchedule = true;
            bool includeSchedule = true;
            bool includeSums = true;
            bool includeTimePayrollTransactions = true;
            bool includeAttestStates = true;
            input.CalculateLoadingsForGrid(columnSettings, includeDaytypes, includeTemplateSchedule, includeSchedule, includeShifts, includeTimeStamps, includeTimeBlocks, includeProjectTimeBlocks, includeTimeCodeTransactions, includeTimeInvoiceTransactions, includeTimePayrollTransactions, includeSums, includeAttestStates);

            return Content(HttpStatusCode.OK, ttam.GetAttestEmployeeDays(input));
        }

        [HttpPost]
        [Route("Attest/EmployeePeriods/")]
        public IHttpActionResult GetTimeAttestEmployeePeriods(GetTimeAttestEmployeePeriodsModel model)
        {
            List<AgGridColumnSettingDTO> columnSettings = sm.GetAgGridSettings(AgGridType.AttestGroup);

            var input = GetAttestEmployeePeriodsInput.CreateInputForWeb(base.ActorCompanyId, base.UserId, model.DateFrom, model.DateTo, model.TimePeriodId.ToNullable(), model.Grouping, model.GroupId, model.VisibleEmployeeIds, model.IsAdditional, model.IncludeAdditionalEmployees, model.DoNotShowDaysOutsideEmployeeAccount, model.CacheKeyToUse);
            input.CalculateLoadingsForGrid(columnSettings, includeSchedule: true, includeTimeBlocks: true, includeSums: true, includeAttestStates: true);

            return Content(HttpStatusCode.OK, ttam.GetAttestEmployeePeriods(input));
        }

        [HttpPost]
        [Route("Attest/EmployeePeriods/Preview")]
        public IHttpActionResult GetTimeAttestEmployeePeriodsPreview(GetTimeAttestEmployeePeriodsPreviewModel model)
        {
            return Content(HttpStatusCode.OK, ttam.GetAttestEmployeePeriodsPreview(model.Tree, model.GroupNode));
        }

        [HttpGet]
        [Route("Attest/AttestTransitionLogs/{timeBlockDateId:int}/{employeeId:int}/{timePayrollTransactionId:int}")]
        public IHttpActionResult GetAttestTransitionLogs(int timeBlockDateId, int employeeId, int timePayrollTransactionId)
        {
            return Content(HttpStatusCode.OK, am.GetAttestTransitionLogs(employeeId, timeBlockDateId, timePayrollTransactionId));
        }

        [HttpPost]
        [Route("Attest/Employee/TimeStamp/")]
        public IHttpActionResult SaveTimeStampEntries(SaveTimeStampsModel model)
        {
            return Content(HttpStatusCode.OK, tsm.SaveTimeStampEntries(model.Entries, model.Date, model.EmployeeId, base.ActorCompanyId, model.DiscardBreakEvaluation));
        }

        [HttpPost]
        [Route("Attest/ValidateDeviationChange/")]
        public IHttpActionResult ValidateDeviationChange(ValidateDeviationChangeModel model)
        {
            return Content(HttpStatusCode.OK, tem.ValidateDeviationChange(model.EmployeeId, model.TimeBlockId, model.TimeScheduleTemplatePeriodId, model.TimeBlockGuidId, model.ClientChange, model.Date, model.StartTime, model.StopTime, model.TimeBlocks, model.OnlyUseInTimeTerminal, model.TimeDeviationCauseId, model.EmployeeChildId, model.Comment, model.AccountSetting));
        }

        [HttpPost]
        [Route("Attest/SaveGeneratedDeviations/")]
        public IHttpActionResult SaveGeneratedDeviations(SaveGeneratedDeviationsModel model)
        {
            return Content(HttpStatusCode.OK, tem.SaveGeneratedDeviations(model.TimeBlocks, model.TimeCodeTransactions, model.TimePayrollTransactions, model.ApplyAbsences, model.TimeBlockDateId, model.TimeScheduleTemplatePeriodId, model.EmployeeId, model.PayrollImportEmployeeTransactionIds));
        }

        [HttpPost]
        [Route("Attest/Tree/AutoAttest/")]
        public IHttpActionResult RunAutoAttest(RunAutoAttestModel model)
        {
            return Content(HttpStatusCode.OK, tem.RunAutoAttest(model.EmployeeIds, model.StartDate, model.StopDate));
        }

        [HttpPost]
        [Route("Attest/Employees")]
        public IHttpActionResult SaveAttestForEmployees(SaveAttestForEmployeesModel model)
        {
            if (model.TimePeriodId.HasValue && model.TimePeriodId.Value > 0)
                return Content(HttpStatusCode.OK, tem.SaveAttestForEmployees(model.CurrentEmployeeId, model.EmployeeIds, model.AttestStateToId, model.TimePeriodId.Value, model.IsPayrollAttest));
            else if (model.StartDate.HasValue && model.StopDate.HasValue)
                return Content(HttpStatusCode.OK, tem.SaveAttestForEmployees(model.CurrentEmployeeId, model.EmployeeIds, model.AttestStateToId, model.StartDate.Value, model.StopDate.Value, model.IsPayrollAttest));
            else
                return Content(HttpStatusCode.NotFound, new ActionResult(false));
        }

        [HttpPost]
        [Route("Attest/Employee")]
        public IHttpActionResult SaveAttestForEmployee(SaveAttestForEmployeeModel model)
        {
            return Content(HttpStatusCode.OK, tem.SaveAttestForEmployee(model.Items, model.EmployeeId, model.AttestStateToId, model.IsMySelf));
        }

        [HttpPost]
        [Route("Attest/Employee/Validation")]
        public IHttpActionResult SaveAttestForEmployeeValidation(SaveAttestForEmployeeValidationModel model)
        {
            return Content(HttpStatusCode.OK, ttam.SaveAttestForEmployeeValidation(model.Items, model.AttestStateToId, model.IsMySelf, model.EmployeeId, base.ActorCompanyId, base.RoleId, base.UserId));
        }

        [HttpPost]
        [Route("Attest/Transactions")]
        public IHttpActionResult SaveAttestForTransactions(SaveAttestForTransactionsModel model)
        {
            return Content(HttpStatusCode.OK, tem.SaveAttestForTransactions(model.Items, model.AttestStateToId, model.IsMySelf));
        }

        [HttpPost]
        [Route("Attest/Transactions/Validation")]
        public IHttpActionResult SaveAttestForTransactionsValidation(SaveAttestForTransactionsValidationModel model)
        {
            return Content(HttpStatusCode.OK, ttam.SaveAttestForTransactionsValidation(model.Items, model.AttestStateToId, model.IsMySelf, base.ActorCompanyId, base.UserId));
        }

        [HttpPost]
        [Route("Attest/UnlockDay")]
        public IHttpActionResult UnlockDay(UnlockDayModel model)
        {
            int attestStateId = am.GetInitialAttestStateId(base.ActorCompanyId, TermGroup_AttestEntity.PayrollTime);
            return Content(HttpStatusCode.OK, tem.SaveAttestForEmployee(model.Items, model.EmployeeId, attestStateId, forceWholeDay: true));
        }

        [HttpPost]
        [Route("Attest/SendReminder")]
        public IHttpActionResult SendAttestReminder(SendAttestReminderModel model)
        {
            return Content(HttpStatusCode.OK, tem.SendAttestReminder(model.EmployeeIds, model.StartDate, model.StopDate, model.DoSendToExecutive, model.DoSendToEmployee));
        }

        [HttpGet]
        [Route("Attest/AdditionDeduction/{employeeId:int}/{dateFrom}/{dateTo}/{timePeriodId:int}/{isMySelf:bool}")]
        public IHttpActionResult GetAdditionDeductions(int employeeId, string dateFrom, string dateTo, int timePeriodId, bool isMySelf)
        {
            return Content(HttpStatusCode.OK, ttam.GetAttestEmployeeAdditionDeductions(employeeId, BuildDateTimeFromString(dateFrom, true, CalendarUtility.DATETIME_DEFAULT).Value, BuildDateTimeFromString(dateTo, true, CalendarUtility.DATETIME_DEFAULT).Value, (timePeriodId != 0 ? timePeriodId.ToNullable() : null), isMySelf: isMySelf));
        }

        [HttpPost]
        [Route("Attest/AdditionDeduction/Validation")]
        public IHttpActionResult SaveAttestForAdditionDeductionsValidation(SaveAttestForAdditionDeductionsValidationModel model)
        {
            return Content(HttpStatusCode.OK, ttam.SaveAttestForAdditionDeductionsValidation(model.TransactionItems, model.AttestStateToId, model.IsMySelf, model.EmployeeId, base.ActorCompanyId, base.UserId));
        }

        [HttpPost]
        [Route("Attest/UnhandledShiftChangesEmployees/Recalculate")]
        public IHttpActionResult RecalculateUnhandledShiftChangesEmployees(RecalculateUnhandledShiftChangesModel model)
        {
            return Content(HttpStatusCode.OK, tem.RecalculateUnhandledShiftChanges(model.UnhandledEmployees, model.DoRecalculateShifts, model.DoRecalculateExtraShifts));
        }

        [HttpGet]
        [Route("Attest/CalculationFunction/Description/{option:int}/")]
        public IHttpActionResult GetTimeAttestFunctionOptionDescription(int option)
        {
            return Content(HttpStatusCode.OK, ttam.GetTimeAttestFunctionOptionDescription(option));
        }


        [HttpPost]
        [Route("Attest/CalculationFunction/Employee")]
        public IHttpActionResult ApplyCalculationFunctionForEmployee(ApplyCalculationFunctionForEmployeeModel model)
        {
            return Content(HttpStatusCode.OK, tem.ApplyCalculationFunctionForEmployee(model.Items, (SoeTimeAttestFunctionOption)model.Option, null));
        }

        [HttpPost]
        [Route("Attest/CalculationFunction/Employees/")]
        public IHttpActionResult ApplyCalculationFunctionForEmployees(ApplyCalculationFunctionForEmployeesModel model)
        {
            return Content(HttpStatusCode.OK, tem.ApplyCalculationFunctionForEmployees(model.Items, (SoeTimeAttestFunctionOption)model.Option, model.TimeScheduleScenarioHeadId));
        }

        [HttpPost]
        [Route("Attest/CalculationFunction/Validation")]
        public IHttpActionResult ApplyCalculationFunctionValidation(ApplyCalculationFunctionValidationModel model)
        {
            return Content(HttpStatusCode.OK, ttam.ApplyCalculationFunctionValidation(model.EmployeeId, model.Items, model.Option, base.ActorCompanyId, base.UserId));
        }

        [HttpPost]
        [Route("Attest/CalculationFunction/Employees/CreateTransactionsForPlannedPeriodCalculation")]
        public IHttpActionResult CreateTransactionsForPlannedPeriodCalculation(CreateTransactionsForPlannedPeriodCalculationModel model)
        {
            return Content(HttpStatusCode.OK, tem.CreateTransactionsForPlannedPeriodCalculation(model.EmployeeId, model.TimePeriodId));
        }

        [HttpPost]
        [Route("Attest/CalculationFunction/Employees/GetCalculationsFromPeriod")]
        public IHttpActionResult GetCalculationsFromPeriod(PeriodCalculationForEmployeesModel model)
        {
            return Content(HttpStatusCode.OK, ttpm.GetCalculationsFromPeriod(model.EmployeeIds, model.TimePeriodId));
        }
        #endregion

        #region Earned Holiday

        [HttpPost]
        [Route("EarnedHoliday/Load/")]
        public IHttpActionResult LoadEarnedHolidaysContent(EarnedHolidayModel model)
        {
            return Content(HttpStatusCode.OK, ttm.LoadEarnedHolidaysContent(model.HolidayId, model.Year, model.LoadSuggestions, base.UserId, base.RoleId, base.ActorCompanyId, employeeEarnedHolidaysInput: model.EmployeeEarnedHolidays));
        }

        [HttpPost]
        [Route("EarnedHoliday/CreateTransactions/")]
        public IHttpActionResult CreateTransactionsForEarnedHoliday(ManageTransactionsForEarnedHolidayModel model)
        {
            return Content(HttpStatusCode.OK, tem.CreateTransctionsForEarnedHoliday(model.HolidayId, model.EmployeeIds, model.Year));
        }

        [HttpPost]
        [Route("EarnedHoliday/DeleteTransactions/")]
        public IHttpActionResult DeleteTransactionsForEarnedHolidayContent(ManageTransactionsForEarnedHolidayModel model)
        {
            return Content(HttpStatusCode.OK, tem.DeleteTransctionsForEarnedHoliday(model.HolidayId, model.EmployeeIds, model.Year));
        }

        #endregion

        #region PlanningPeriod

        [HttpGet]
        [Route("PlanningPeriodHeadWithPeriods/{timePeriodHeadId:int}/{dateString}")]
        public IHttpActionResult GetPlanningPeriod(int timePeriodHeadId, string dateString)
        {
            return Content(HttpStatusCode.OK, tpm.GetPlanningPeriodHeadWithPeriods(BuildDateTimeFromString(dateString, true).Value, timePeriodHeadId, base.ActorCompanyId));
        }

        #endregion

        #region TimeAbsenceDetails

        [HttpGet]
        [Route("TimeAbsenceDetails/{employeeId:int}/{dateFrom}/{dateTo}")]
        public IHttpActionResult GetAbsenceDetails(int employeeId, string dateFrom, string dateTo)
        {
            return Content(HttpStatusCode.OK, tbm.GetTimeAbsenceDetails(employeeId, BuildDateTimeFromString(dateFrom, true, CalendarUtility.DATETIME_DEFAULT).Value, BuildDateTimeFromString(dateTo, true, CalendarUtility.DATETIME_DEFAULT).Value));
        }

        [HttpPost]
        [Route("TimeAbsenceDetails/Ratio")]
        public IHttpActionResult SaveAbsenceDetails(SaveTimeAbsenceDetailsModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, tem.SaveAbsenceDetailsRatio(model.EmployeeId, model.TimeAbsenceDetails));
        }

        #endregion

        #region TimeAbsenceRules

        [HttpGet]
        [Route("TimeAbsenceRules")]
        public IHttpActionResult GetTimeAbsenceRules(HttpRequestMessage message)
        {
            var input = new GetTimeAbsenceRulesInput(base.ActorCompanyId)
            {
                LoadTimeCode = true,
                LoadEmployeeGroups = true,
            };
            if (message.HasAcceptValue(HttpExtensions.ACCEPT_GRID_DTO))
                return Content(HttpStatusCode.OK, trm.GetTimeAbsenceRules(input).ToGridDTOs());
            return Content(HttpStatusCode.OK, trm.GetTimeAbsenceRules(input).ToDTOs());
        }

        [HttpGet]
        [Route("TimeAbsenceRule/{timeAbsenceRuleHeadId:int}")]
        public IHttpActionResult GetTimeAbsenceRule(int timeAbsenceRuleHeadId)
        {
            var input = new GetTimeAbsenceRulesInput(base.ActorCompanyId, timeAbsenceRuleHeadId)
            {
                LoadEmployeeGroups = true,
                LoadRows = true,
                LoadRowProducts = true,
            };
            return Content(HttpStatusCode.OK, trm.GetTimeAbsenceRuleHead(input).ToDTO(true, true));
        }

        [HttpGet]
        [Route("TimeAbsenceRuleRows/{timeAbsenceRuleHeadId:int}")]
        public IHttpActionResult GetTimeAbsenceRuleRows(int timeAbsenceRuleHeadId)
        {
            return Content(HttpStatusCode.OK, trm.GetTimeAbsenceRuleRows(timeAbsenceRuleHeadId).ToDTOs());
        }

        [HttpPost]
        [Route("TimeAbsenceRule")]
        public IHttpActionResult SaveTimeAbsenceRule(TimeAbsenceRuleHeadDTO model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, trm.SaveTimeAbsenceRuleHead(model, model.TimeAbsenceRuleRows));
        }

        [HttpDelete]
        [Route("TimeAbsenceRule/{timeAbsenceRuleHeadId:int}")]
        public IHttpActionResult DeleteTimeAbsenceRule(int timeAbsenceRuleHeadId)
        {
            return Content(HttpStatusCode.OK, trm.DeleteTimeAbsenceRuleHead(timeAbsenceRuleHeadId));
        }

        #endregion

        #region TimeAccumulator

        [HttpGet]
        [Route("TimeAccumulator")]
        public IHttpActionResult GetTimeAccumulators(HttpRequestMessage message)
        {
            if (message.HasAcceptValue(HttpExtensions.ACCEPT_GRID_DTO))
                return Content(HttpStatusCode.OK, tam.GetTimeAccumulators(base.ActorCompanyId, onlyActive: message.GetBoolValueFromQS("onlyActive"), loadTimePeriodHead: true).ToGridDTOs());
            else if (message.HasAcceptValue(HttpExtensions.ACCEPT_GENERIC_TYPE))
                return Content(HttpStatusCode.OK, tam.GetTimeAccumulatorsDict(base.ActorCompanyId, message.GetBoolValueFromQS("addEmptyRow"), message.GetBoolValueFromQS("includeVacationBalance"), message.GetBoolValueFromQS("includeWorkTimeAccountBalance")).ToSmallGenericTypes());

            return Content(HttpStatusCode.OK, tam.GetTimeAccumulators(base.ActorCompanyId, onlyActive: message.GetBoolValueFromQS("onlyActive")).ToDTOs());
        }

        [HttpGet]
        [Route("TimeAccumulator/{timeAccumulatorId:int}/{onlyActive:bool}/{loadEmployeeGroups:bool}/{loadTimeWorkReductionEarning:bool}")]
        public IHttpActionResult GetTimeAccumulator(int timeAccumulatorId, bool onlyActive, bool loadEmployeeGroups, bool loadTimeWorkReductionEarning)
        {
            return Content(HttpStatusCode.OK, tam.GetTimeAccumulator(base.ActorCompanyId, timeAccumulatorId, onlyActive: onlyActive, loadEmployeeGroups: loadEmployeeGroups, loadTimeWorkReductionEarning: loadTimeWorkReductionEarning).ToDTO());
        }

        [HttpGet]
        [Route("TimeAccumulator/{employeeId:int}/{startDate}/{stopDate}/{addSourceIds:bool}/{calculateDay:bool}/{calculatePeriod:bool}/{calculatePlanningPeriod:bool}/{calculateYear:bool}/{calculateAccToday:bool}/{calculateAccTodayValue:bool}")]
        public IHttpActionResult GetTimeAccumulatorItems(int employeeId, string startDate, string stopDate, bool addSourceIds, bool calculateDay, bool calculatePeriod, bool calculatePlanningPeriod, bool calculateYear, bool calculateAccToday, bool calculateAccTodayValue)
        {
            DateTime? paramStartDate = BuildDateTimeFromString(startDate, true);
            DateTime? paramStopDate = BuildDateTimeFromString(stopDate, true);
            if (!paramStartDate.HasValue || !paramStopDate.HasValue)
                return Content(HttpStatusCode.OK, new List<TimeAccumulatorItem>());

            GetTimeAccumulatorItemsInput timeAccInput = GetTimeAccumulatorItemsInput.CreateInput(base.ActorCompanyId, base.UserId, employeeId, paramStartDate.Value, paramStopDate.Value, addSourceIds: addSourceIds, calculateDay: calculateDay, calculatePeriod: calculatePeriod, calculatePlanningPeriod: calculatePlanningPeriod, calculateYear: calculateYear, calculateAccToday: calculateAccToday, calculateAccTodayValue: calculateAccTodayValue);
            var result = tam.GetTimeAccumulatorItems(timeAccInput);
            return Content(HttpStatusCode.OK, result);
        }

        [HttpPost]
        [Route("TimeAccumulator/")]
        public IHttpActionResult SaveTimeAccumulator(TimeAccumulatorDTO timeAccumulator)
        {
            return Content(HttpStatusCode.OK, tam.SaveTimeAccumulator(timeAccumulator));
        }

        [HttpPost]
        [Route("TimeAccumulator/Recalculate")]
        public IHttpActionResult RecalculateTimeAccumulators(ListIntModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, tam.CalculateTimeAccumulatorYearBalance(base.ActorCompanyId, model.Numbers));
        }

        [HttpDelete]
        [Route("TimeAccumulator/{timeAccumulatorId:int}")]
        public IHttpActionResult DeleteTimeAccumulator(int timeAccumulatorId)
        {
            return Content(HttpStatusCode.OK, tam.DeleteTimeAccumulator(base.ActorCompanyId, timeAccumulatorId));
        }

        #endregion

        #region TimeBlockDate

        [HttpGet]
        [Route("TimeBlockDate/Id/{employeeId:int}/{date}")]
        public IHttpActionResult GetTimeBlockDateId(int employeeId, string date)
        {
            return Content(HttpStatusCode.OK, tbm.GetTimeBlockDate(base.ActorCompanyId, employeeId, BuildDateTimeFromString(date, true).Value)?.TimeBlockDateId ?? 0);
        }

        #endregion

        #region TimeCalendar


        [HttpGet]
        [Route("TimeCalendar/Period/{employeeId:int}/{fromDateString}/{toDateString}/{sysPayrollTypeLevel1:int}/{sysPayrollTypeLevel2:int}/{sysPayrollTypeLevel3:int}/{sysPayrollTypeLevel4:int}")]
        public IHttpActionResult GetTimeCalendarPeriods(int employeeId, string fromDateString, string toDateString, int sysPayrollTypeLevel1, int sysPayrollTypeLevel2, int sysPayrollTypeLevel3, int sysPayrollTypeLevel4)
        {
            Dictionary<int, List<int>> excludedLevels = new Dictionary<int, List<int>>();
            excludedLevels.Add(1, new List<int>()
            {
                (int)TermGroup_SysPayrollType.SE_Benefit,
                (int)TermGroup_SysPayrollType.SE_Tax,
                (int)TermGroup_SysPayrollType.SE_Compensation,
                (int)TermGroup_SysPayrollType.SE_Deduction,
                (int)TermGroup_SysPayrollType.SE_CostDeduction,
                (int)TermGroup_SysPayrollType.SE_OccupationalPension,
                (int)TermGroup_SysPayrollType.SE_EmploymentTaxCredit,
                (int)TermGroup_SysPayrollType.SE_EmploymentTaxDebit,
                (int)TermGroup_SysPayrollType.SE_SupplementChargeCredit,
                (int)TermGroup_SysPayrollType.SE_SupplementChargeDebit,
                (int)TermGroup_SysPayrollType.SE_NetSalary,
            });

            return Content(HttpStatusCode.OK, ttm.GetTimeCalendarPeriods(base.ActorCompanyId, employeeId, base.BuildDateTimeFromString(fromDateString, true).Value, base.BuildDateTimeFromString(toDateString, true).Value, sysPayrollTypeLevel1 != 0 ? (int?)sysPayrollTypeLevel1 : null, sysPayrollTypeLevel2 != 0 ? (int?)sysPayrollTypeLevel2 : null, sysPayrollTypeLevel3 != 0 ? (int?)sysPayrollTypeLevel3 : null, sysPayrollTypeLevel4 != 0 ? (int?)sysPayrollTypeLevel4 : null, excludedLevels));
        }

        public List<TimeCalendarPeriodDTO> GetTimeCalendarPeriods(int actorCompanyId, int employeeId, DateTime fromDate, DateTime toDate, int? sysPayrollTypeLevel1, int? sysPayrollTypeLevel2, int? sysPayrollTypeLevel3, int? sysPayrollTypeLevel4, Dictionary<int, List<int>> excludedLevels, bool includeHolidays)
        {
            if (actorCompanyId != base.ActorCompanyId)
                return new List<TimeCalendarPeriodDTO>();

            return ttm.GetTimeCalendarPeriods(actorCompanyId, employeeId, fromDate, toDate, sysPayrollTypeLevel1, sysPayrollTypeLevel2, sysPayrollTypeLevel3, sysPayrollTypeLevel4, excludedLevels, includeHolidays);
        }

        #endregion

        #region TimeCode

        [HttpGet]
        [Route("TimeCode")]
        public IHttpActionResult GetTimeCodes(HttpRequestMessage message)
        {
            SoeTimeCodeType timeCodeType = (SoeTimeCodeType)message.GetIntValueFromQS("timeCodeType");
            bool onlyActive = message.GetBoolValueFromQS("onlyActive");
            bool loadPayrollProducts = message.GetBoolValueFromQS("loadPayrollProducts");
            bool onlyWithInvoiceProduct = message.GetBoolValueFromQS("onlyWithInvoiceProduct");
            bool includeType = message.GetBoolValueFromQS("includeType");

            if (message.HasAcceptValue(HttpExtensions.ACCEPT_GRID_DTO))
                return Content(HttpStatusCode.OK, tcm.GetTimeCodes(base.ActorCompanyId, timeCodeType, onlyActive, loadPayrollProducts).ToGridDTOs(loadPayrollProducts, base.GetTermGroupContent(TermGroup.YesNo), base.GetTermGroupContent(TermGroup.TimeCodeClassification)));
            else if (message.HasAcceptValue(HttpExtensions.ACCEPT_GENERIC_TYPE))
                if (includeType)
                    return Content(HttpStatusCode.OK, tcm.GetTimeCodesDict(base.ActorCompanyId, message.GetBoolValueFromQS("addEmptyRow"), message.GetBoolValueFromQS("concatCodeAndName"), includeType).ToSmallGenericTypes());
                else
                    return Content(HttpStatusCode.OK, tcm.GetTimeCodesDict(base.ActorCompanyId, message.GetBoolValueFromQS("addEmptyRow"), message.GetBoolValueFromQS("concatCodeAndName"), timeCodeType).ToSmallGenericTypes());

            return Content(HttpStatusCode.OK, tcm.GetTimeCodes(base.ActorCompanyId, timeCodeType, onlyActive, loadPayrollProducts, onlyWithInvoiceProduct).ToDTOs(loadPayrollProducts, true));
        }

        [HttpGet]
        [Route("TimeCode/{timeCodeType:int}/{timeCodeId:int}/{loadInvoiceProducts:bool}/{loadPayrollProducts:bool}/{loadTimeCodeDeviationCauses:bool}/{loadEmployeeGroups:bool}")]
        public IHttpActionResult GetTimeCode(int timeCodeType, int timeCodeId, bool loadInvoiceProducts, bool loadPayrollProducts, bool loadTimeCodeDeviationCauses, bool loadEmployeeGroups)
        {
            TimeCode timeCode = tcm.GetTimeCode(timeCodeId, base.ActorCompanyId, false, loadInvoiceProducts, loadPayrollProducts, true, loadTimeCodeDeviationCauses, loadEmployeeGroups);
            if (timeCode != null)
            {
                switch ((SoeTimeCodeType)timeCodeType)
                {
                    case SoeTimeCodeType.Absense:
                        return Content(HttpStatusCode.OK, timeCode.ToAbsenceDTO());
                    case SoeTimeCodeType.AdditionDeduction:
                        return Content(HttpStatusCode.OK, timeCode.ToAdditionDeductionDTO());
                    case SoeTimeCodeType.Break:
                        return Content(HttpStatusCode.OK, timeCode.ToBreakDTO());
                    case SoeTimeCodeType.Material:
                        return Content(HttpStatusCode.OK, timeCode.ToMaterialDTO());
                    case SoeTimeCodeType.Work:
                        return Content(HttpStatusCode.OK, timeCode.ToWorkDTO());
                    default:
                        return Content(HttpStatusCode.OK, timeCode.ToDTO());
                }
            }
            else
            {
                return Content(HttpStatusCode.OK, timeCode);
            }
        }

        [HttpGet]
        [Route("TimeCode/Break/{addEmptyRow:bool}")]
        public IHttpActionResult GetTimeCodeBreaks(bool addEmptyRow)
        {
            return Content(HttpStatusCode.OK, tcm.GetTimeCodeBreaks(base.ActorCompanyId, addEmptyRow).ToSmallBreakDTOs());
        }

        [HttpGet]
        [Route("TimeCode/AdditionDeduction/{checkInvoiceProduct:bool}/{isMySelf:bool}")]
        public IHttpActionResult GetTimeCodeAdditionDeductions(bool checkInvoiceProduct, bool isMySelf)
        {
            var timeCodes = tcm.GetTimeCodeAdditionDeductions(base.ActorCompanyId, checkInvoiceProduct, isMySelf).ToAdditionDeductionDTOs();
            return Content(HttpStatusCode.OK, timeCodes);
        }

        [HttpPost]
        [Route("TimeCode/")]
        public IHttpActionResult SaveTimeCode(TimeCodeSaveDTO timeCode)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, tcm.SaveTimeCode(timeCode, base.ActorCompanyId, base.RoleId));
        }

        [HttpPost]
        [Route("TimeCode/UpdateState")]
        public IHttpActionResult UpdateTimeCodeState(UpdateEntityStatesModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, tcm.UpdateTimeCodeState(model.Dict, base.ActorCompanyId));
        }

        [HttpDelete]
        [Route("TimeCode/{timeCodeId:int}")]
        public IHttpActionResult DeleteTimeCode(int timeCodeId)
        {
            return Content(HttpStatusCode.OK, tcm.DeleteTimeCode(timeCodeId, base.ActorCompanyId));
        }

        #endregion

        #region TimeCodeBreakGroup

        [HttpGet]
        [Route("TimeCodeBreakGroups/")]
        public IHttpActionResult GetTimeCodeBreakGroups()
        {
            return Content(HttpStatusCode.OK, tcm.GetTimeCodeBreakGroups(base.ActorCompanyId).ToGridDTOs());
        }

        [HttpGet]
        [Route("TimeCodeBreakGroup/{timeCodeBreakGroupId:int}")]
        public IHttpActionResult GetTimeCodeBreakGroup(int timeCodeBreakGroupId)
        {
            return Content(HttpStatusCode.OK, tcm.GetTimeCodeBreakGroup(timeCodeBreakGroupId, base.ActorCompanyId).ToDTO());
        }

        [HttpPost]
        [Route("TimeCodeBreakGroup/")]
        public IHttpActionResult SaveTimeCodeBreakGroup(TimeCodeBreakGroupDTO timeCodeBreakGroup)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, tcm.SaveTimeCodeBreakGroup(timeCodeBreakGroup, base.ActorCompanyId));
        }

        [HttpDelete]
        [Route("TimeCodeBreakGroup/{timeCodeBreakGroupId:int}")]
        public IHttpActionResult DeleteTimeCodeBreakGroup(int timeCodeBreakGroupId)
        {
            return Content(HttpStatusCode.OK, tcm.DeleteTimeCodeBreakGroup(timeCodeBreakGroupId, base.ActorCompanyId));
        }

        #endregion

        #region TimeDeviationCause

        [HttpGet]
        [Route("TimeDeviationCause/Grid/")]
        public IHttpActionResult GetTimeDeviationCauses()
        {
            return Content(HttpStatusCode.OK, tdcm.GetTimeDeviationCauses(base.ActorCompanyId, sortByName: true, loadTimeCode: true, setTimeDeviationTypeName: true).ToGridDTOs());
        }

        [HttpGet]
        [Route("TimeDeviationCause/")]
        public IHttpActionResult GetTimeDeviationCauses(HttpRequestMessage message)
        {
            if (message.HasAcceptValue(HttpExtensions.ACCEPT_GENERIC_TYPE))
            {
                int egId = message.GetIntValueFromQS("employeeGroupId");
                int? employeeGroupId = egId != 0 ? egId : (int?)null;

                if (employeeGroupId.HasValue)
                    return Content(HttpStatusCode.OK, tdcm.GetTimeDeviationCausesDictByEmployeeGroup(base.ActorCompanyId, employeeGroupId.Value, message.GetBoolValueFromQS("addEmptyRow"), message.GetBoolValueFromQS("removeAbsence")).ToSmallGenericTypes());
                else
                    return Content(HttpStatusCode.OK, tdcm.GetTimeDeviationCausesDict(base.ActorCompanyId, message.GetBoolValueFromQS("addEmptyRow"), message.GetBoolValueFromQS("removeAbsence")).ToSmallGenericTypes());
            }
            else if (message.HasAcceptValue(HttpExtensions.ACCEPT_GRID_DTO))
            {
                int egId = message.GetIntValueFromQS("employeeGroupId");
                int? employeeGroupId = egId != 0 ? egId : (int?)null;

                return Content(HttpStatusCode.OK, tdcm.GetTimeDeviationCausesByEmployeeGroup(base.ActorCompanyId, employeeGroupId, loadTimeCode: true, removeAbsence: message.GetBoolValueFromQS("removeAbsence"), setTimeDeviationTypeName: true).ToGridDTOs());
            }
            else
            {
                if (message.GetBoolValueFromQS("onlyAbsence"))
                    return Content(HttpStatusCode.OK, tdcm.GetTimeDeviationCausesAbsence(base.ActorCompanyId).ToDTOs());

                int egId = message.GetIntValueFromQS("employeeGroupId");
                if (egId > 0)
                {
                    var onlyUseInTimeTerminal = message.GetBoolValueFromQS("onlyUseInTimeTerminal");
                    return Content(HttpStatusCode.OK, tdcm.GetTimeDeviationCausesByEmployeeGroup(base.ActorCompanyId, egId, sort: true, loadTimeCode: true, onlyUseInTimeTerminal: onlyUseInTimeTerminal, setTimeDeviationTypeName: true).ToDTOs());
                }
                else
                {
                    var getEmployeeGroups = message.GetBoolValueFromQS("getEmployeeGroups");
                    return Content(HttpStatusCode.OK, tdcm.GetTimeDeviationCauses(base.ActorCompanyId, loadEmployeeGroups: getEmployeeGroups).ToDTOs());
                }
            }
        }

        [HttpGet]
        [Route("TimeDeviationCause/Hibernating/")]
        public IHttpActionResult GetHibernatingTimeDeviationCauses()
        {
            return Content(HttpStatusCode.OK, tdcm.GetTimeDeviationCausesHibernating(base.ActorCompanyId).ToDTOs());
        }

        [HttpGet]
        [Route("TimeDeviationCause/{timeDeviationCauseId:int}")]
        public IHttpActionResult GetTimeDeviationCause(int timeDeviationCauseId)
        {
            return Content(HttpStatusCode.OK, tdcm.GetTimeDeviationCause(timeDeviationCauseId, base.ActorCompanyId).ToDTO());
        }

        [HttpGet]
        [Route("TimeDeviationCause/StandardIdFromPrio/{employeeId:int}/{dateString}")]
        public IHttpActionResult GetStandardTimeDeviationCauseIdFromPrio(int employeeId, string dateString)
        {
            return Content(HttpStatusCode.OK, tdcm.GetTimeDeviationCauseIdFromPrio(employeeId, base.ActorCompanyId, base.BuildDateTimeFromString(dateString, true)));
        }

        [HttpGet]
        [Route("TimeDeviationCause/AbsenceAnnouncements/{employeeGroupId:int}/{addEmptyRow:bool}")]
        public IHttpActionResult GetTimeDeviationCauseAbsenceAnnouncements(int employeeGroupId, bool addEmptyRow)
        {
            return Content(HttpStatusCode.OK, tdcm.GetTimeDeviationCausesAbsenceAnnouncementDict(base.ActorCompanyId, employeeGroupId, addEmptyRow).ToSmallGenericTypes());
        }

        [HttpGet]
        [Route("TimeDeviationCauseRequests/")]
        public IHttpActionResult GetTimeDeviationCauseRequests(HttpRequestMessage message)
        {
            int employeeGroupId = message.GetIntValueFromQS("employeeGroupId");
            int employeeId = message.GetIntValueFromQS("employeeId");

            if (message.HasAcceptValue(HttpExtensions.ACCEPT_GENERIC_TYPE))
            {
                return Content(HttpStatusCode.OK, tdcm.GetTimeDeviationCausesRequestsDict(base.ActorCompanyId, employeeGroupId, message.GetBoolValueFromQS("addEmptyRow")).ToSmallGenericTypes());
            }
            else
            {
                return Content(HttpStatusCode.OK, tdcm.GetTimeDeviationCausesEmployeeRequests(base.ActorCompanyId, employeeGroupId, employeeId).ToDTOs());
            }
        }

        [HttpGet]
        [Route("TimeDeviationCause/Employee/Absence/{employeeId:int}/{date}/{onlyUseInTimeTerminal:bool}")]
        public IHttpActionResult GetAbsenceTimeDeviationCauses(int employeeId, string date, bool onlyUseInTimeTerminal)
        {
            return Content(HttpStatusCode.OK, tdcm.GetTimeDeviationCausesAbsenceFromEmployeeId(base.ActorCompanyId, employeeId, BuildDateTimeFromString(date, true, DateTime.Today), onlyUseInTimeTerminal).ToDTOs());
        }

        [HttpPost]
        [Route("TimeDeviationCause/")]
        public IHttpActionResult SaveTimeDeviationCauses(TimeDeviationCauseDTO input)
        {
            return Content(HttpStatusCode.OK, tdcm.SaveTimeDeviationCauses(base.ActorCompanyId, input));
        }


        [HttpPost]
        [Route("TimeDeviationCause/Delete")]
        public IHttpActionResult DeleteTimeDeviationCause(TimeDeviationCauseDTO input)
        {
            return Content(HttpStatusCode.OK, tdcm.DeleteTimeDeviationCause(input.TimeDeviationCauseId, base.ActorCompanyId));
        }

        #endregion

        #region TimeDeviationsAfterEmployment

        [HttpGet]
        [Route("DeviationsAfterEmployment/")]
        public IHttpActionResult GetDeviationsAfterEmployment()
        {
            return Content(HttpStatusCode.OK, tem.GetDeviationsAfterEmployment());
        }

        [HttpPost]
        [Route("DeviationsAfterEmployment/Delete")]
        public IHttpActionResult DeleteDeviationsDaysAfterEmployment(DeviationsAfterEmploymentModel model)
        {
            return Content(HttpStatusCode.OK, tem.DeleteDeviationsDaysAfterEmployment(model.Deviations.ObjToList()));
        }

        #endregion

        #region TimeHibernatingAbsence

        [HttpGet]
        [Route("TimeHibernatingAbscence/{employeeId:int}/{employmentId:int}")]
        public IHttpActionResult GetHibernatingAbsenceHead(int employeeId, int employmentId)
        {
            return Content(HttpStatusCode.OK, thm.GetHibernatingAbsenceHead(employeeId, employmentId, includeRows: true, includeEmployee: true, includeEmployment: true).ToDTO(fillEmptyDays: true));
        }


        [HttpPost]
        [Route("TimeHibernatingAbscence")]
        public IHttpActionResult SaveHibernatingAbsenceHead(SaveTimeHibernatingAbsenceHeadModel model)
        {
            return Content(HttpStatusCode.OK, thm.SaveHibernatingAbsenceHead(model.TimeHibernatingAbsenceHead));
        }
        #endregion

        #region TimePayrollTransaction

        [HttpGet]
        [Route("TimePayrollTransaction/AccountStd/{timePayrollTransactionId:int}")]
        public AccountDTO GetTimePayrollTransactionAccountStd(int timePayrollTransactionId)
        {
            return ttm.GetTimePayrollTransactionWithAccountStd(timePayrollTransactionId, base.ActorCompanyId)?.AccountStd.Account.ToDTO(includeAccountDim: true);
        }

        [HttpDelete]
        [Route("TimePayrollTransaction/{timePayrollTransactionId:int}/{deleteChilds:bool}")]
        public ActionResult DeleteTimePayrollTransaction(int timePayrollTransactionId, bool deleteChilds)
        {
            return ttm.DeleteTimePayrollTransaction(timePayrollTransactionId, deleteChilds);
        }

        [HttpPost]
        [Route("TimePayrollTransaction/Reverse/Validation")]
        public IHttpActionResult ReverseTransactionsValidation(ReverseTransactionsModel model)
        {
            return Content(HttpStatusCode.OK, tem.ReverseTransactionsValidation(model.EmployeeId, model.Dates));
        }

        [HttpPost]
        [Route("TimePayrollTransaction/Reverse")]
        public IHttpActionResult ReverseTransactions(ReverseTransactionsModel model)
        {
            return Content(HttpStatusCode.OK, tem.ReverseTransactionsAngular(model.EmployeeId, model.Dates, model.TimeDeviationCauseId, model.TimePeriodId, model.EmployeeChildId));
        }

        #endregion

        #region TimePeriodHead

        public IEnumerable<TimePeriodHeadDTO> GetTimePeriodHeadsIncludingPeriodsForType(int actorCompanyId, TermGroup_TimePeriodType type)
        {
            if (actorCompanyId != base.ActorCompanyId)
                return null;
            return tpm.GetTimePeriodHeadsIncludingPeriodsForType(actorCompanyId, type).ToDTOs(true);
        }

        [HttpGet]
        [Route("TimePeriodHead")]
        public IHttpActionResult GetTimePeriodHeads(HttpRequestMessage message)
        {
            int typeValue = message.GetIntValueFromQS("type");

            if (message.HasAcceptValue(HttpExtensions.ACCEPT_GENERIC_TYPE))
            {
                int accountId = message.GetIntValueFromQS("accountId");

                return Content(HttpStatusCode.OK, tpm.GetTimePeriodHeadsDict(base.ActorCompanyId, (TermGroup_TimePeriodType)typeValue, message.GetBoolValueFromQS("addEmptyRow"), accountId != 0 ? accountId : (int?)null).ToSmallGenericTypes());
            }
            else if (message.HasAcceptValue(HttpExtensions.ACCEPT_GRID_DTO))
                return Content(HttpStatusCode.OK, tpm.GetTimePeriodHeads(base.ActorCompanyId, (TermGroup_TimePeriodType)typeValue, message.GetBoolValueFromQS("loadTypeNames"), message.GetBoolValueFromQS("loadAccountNames"), null, message.GetBoolValueFromQS("loadChildNames")).ToGridDTOs());

            return Content(HttpStatusCode.OK, tpm.GetTimePeriodHeads(base.ActorCompanyId, (TermGroup_TimePeriodType)typeValue, message.GetBoolValueFromQS("loadTypeNames"), false).ToDTOs(message.GetBoolValueFromQS("loadTimePeriods")));
        }

        [HttpGet]
        [Route("TimePeriodHead/{timePeriodHeadId:int}")]
        public IHttpActionResult GetTimePeriodHead(int timePeriodHeadId)
        {
            return Content(HttpStatusCode.OK, tpm.GetTimePeriodHead(timePeriodHeadId, base.ActorCompanyId, loadPeriods: true).ToDTO(true));
        }

        [HttpPost]
        [Route("TimePeriodHead/")]
        public IHttpActionResult SaveTimePeriod(SaveTimePeriodHeadModel model)
        {
            return Content(HttpStatusCode.OK, tpm.SaveTimePeriodHead(model.TimePeriodHead, base.ActorCompanyId, model.RemovePeriodLinks));
        }

        [HttpDelete]
        [Route("TimePeriodHead/{timePeriodHeadId:int}/{removePeriodLinks:bool}")]
        public IHttpActionResult DeleteTimePeriodHead(int timePeriodHeadId, bool removePeriodLinks)
        {
            return Content(HttpStatusCode.OK, tpm.DeleteTimePeriodHead(timePeriodHeadId, base.ActorCompanyId, removePeriodLinks));
        }

        #endregion

        #region TimePeriod

        [HttpGet]
        [Route("TimePeriod/")]
        public IHttpActionResult GetTimePeriods(HttpRequestMessage message)
        {
            int timePeriodHeadId = message.GetIntValueFromQS("timePeriodHeadId");

            if (message.HasAcceptValue(HttpExtensions.ACCEPT_GENERIC_TYPE))
                return Content(HttpStatusCode.OK, tpm.GetTimePeriodsDict(timePeriodHeadId, message.GetBoolValueFromQS("addEmptyRow"), base.ActorCompanyId).ToSmallGenericTypes());

            if (timePeriodHeadId == 0)
                return Content(HttpStatusCode.OK, tpm.GetDefaultTimePeriods(base.ActorCompanyId).ToDTOs());
            else
                return Content(HttpStatusCode.OK, tpm.GetTimePeriods(timePeriodHeadId, base.ActorCompanyId).ToDTOs());
        }
        [HttpGet]
        [Route("PeriodsForCalculation/{type:int}/{dateFrom}/{dateTo}/{includePeriodsWithoutChildren:bool}")]
        public IHttpActionResult GetPeriodsForCalculation(int type, string dateFrom, string dateTo, bool includePeriodsWithoutChildren)
        {
            return Content(HttpStatusCode.OK, tpm.GetPeriodsForCalculation((TermGroup_TimePeriodType)type, BuildDateTimeFromString(dateFrom, true).Value, BuildDateTimeFromString(dateTo, true).Value, base.ActorCompanyId, includePeriodsWithoutChildren: includePeriodsWithoutChildren));
        }


        [HttpGet]
        [Route("TimePeriod/{timePeriodHeadId:int}/{dateString}/{loadTimePeriodHead:bool}")]
        public IHttpActionResult GetTimePeriod(int timePeriodHeadId, string dateString, bool loadTimePeriodHead)
        {
            TimePeriodDTO dto = tpm.GetTimePeriod(BuildDateTimeFromString(dateString, true).Value, timePeriodHeadId, base.ActorCompanyId, loadTimePeriodHead).ToDTO();
            if (dto != null)
                dto.TimePeriodHeadId = timePeriodHeadId;
            return Content(HttpStatusCode.OK, dto);
        }

        #endregion

        #region TimeRule

        [HttpGet]
        [Route("TimeRule")]
        public IHttpActionResult GetTimeRules(HttpRequestMessage message)
        {
            return Content(HttpStatusCode.OK, trm.GetTimeRuleGridDTOs());
        }

        [HttpGet]
        [Route("TimeRule/{timeRuleId:int}")]
        public IHttpActionResult GetTimeRule(int timeRuleId)
        {
            return Content(HttpStatusCode.OK, trm.GetTimeRule(timeRuleId, base.ActorCompanyId, active: null, loadRows: true, loadExpressions: true).ToEditDTO());
        }

        [HttpGet]
        [Route("TimeRule/TimeCode/Left")]
        public IHttpActionResult GetTimeRuleTimeCodesLeft()
        {
            return Content(HttpStatusCode.OK, trm.GetTimeRuleTimeCodesLeft().ToSmallGenericTypes());
        }

        [HttpGet]
        [Route("TimeRule/TimeCode/Right")]
        public IHttpActionResult GetTimeRuleTimeCodesRight()
        {
            return Content(HttpStatusCode.OK, trm.GetTimeRuleTimeCodesRight().ToSmallGenericTypes());
        }

        [HttpGet]
        [Route("TimeRule/ImportedDetails/{timeRuleId:int}/{loadDetails:bool}")]
        public IHttpActionResult GetTimeRuleImportedDetails(int timeRuleId, bool loadDetails)
        {
            return Content(HttpStatusCode.OK, trm.GetTimeRuleImportedDetails(base.ActorCompanyId, timeRuleId, loadDetails));
        }

        [HttpPost]
        [Route("TimeRule/")]
        public IHttpActionResult SaveTimeRule(TimeRuleDTO timeRule)
        {
            return Content(HttpStatusCode.OK, trm.SaveTimeRule(timeRule, base.ActorCompanyId, true));
        }

        [HttpPost]
        [Route("TimeRule/ValidateStructure/")]
        public IHttpActionResult ValidateTimeRuleStructure(ValidateTimeRuleStructureModel model)
        {
            return Content(HttpStatusCode.OK, trm.ValidateTimeRuleStructure(model.Widgets));
        }

        [HttpPost]
        [Route("TimeRule/UpdateState")]
        public IHttpActionResult UpdateTimeRuleState(UpdateEntityStatesModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, trm.UpdateTimeRulesState(model.Dict, base.ActorCompanyId));
        }

        [HttpPost]
        [Route("TimeRule/Export")]
        public IHttpActionResult ExportTimeRules(ListIntModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, trm.ExportTimeRules(model.Numbers, base.ActorCompanyId));
        }

        [HttpPost]
        [Route("TimeRule/Import")]
        public async Task<IHttpActionResult> ImportTimeRules()
        {
            if (Request.Content.IsMimeMultipartContent())
            {
                var data = await Request.Content.ParseMultipartAsync();

                HttpPostedFile file = data.Files["file"];
                if (file != null)
                    return Content(HttpStatusCode.OK, trm.ImportTimeRules(new MemoryStream(file.File), base.ActorCompanyId));
            }

            throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
        }

        [HttpPost]
        [Route("TimeRule/Import/Match")]
        public IHttpActionResult ImportTimeRulesMatch(TimeRuleExportImportDTO model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, trm.ImportTimeRuleMatch(model, base.ActorCompanyId));
        }

        [HttpPost]
        [Route("TimeRule/Import/Save")]
        public IHttpActionResult ImportTimeRulesSave(TimeRuleExportImportDTO model)
        {
            return Content(HttpStatusCode.OK, trm.ImportTimeRulesSave(model, base.ActorCompanyId));
        }

        [HttpDelete]
        [Route("TimeRule/{timeRuleId:int}")]
        public IHttpActionResult DeleteTimeRule(int timeRuleId)
        {
            return Content(HttpStatusCode.OK, trm.DeleteTimeRule(base.ActorCompanyId, timeRuleId));
        }

        #endregion

        #region TimeSalaryExport

        [HttpGet]
        [Route("TimeSalaryExport/")]
        public IHttpActionResult GetExportedSalaries()
        {
            return Content(HttpStatusCode.OK, tsam.GetTimeSalaryExportDTOs(base.ActorCompanyId, true));
        }

        [HttpGet]
        [Route("TimeSalaryExport/Selection/{dateFromString}/{dateToString}/{accountDimId:int}")]
        public IHttpActionResult GetTimeSalaryExportSelection(string dateFromString, string dateToString, int accountDimId)
        {
            return Content(HttpStatusCode.OK, tsam.GetTimeSalaryExportSelection(base.ActorCompanyId, BuildDateTimeFromString(dateFromString, true).Value, BuildDateTimeFromString(dateToString, true).Value, accountDimId));
        }

        [HttpPost]
        [Route("TimeSalaryExport/Validate/")]
        public IHttpActionResult ValidateExportSalary(ExportSalaryModel model)
        {
            return Content(HttpStatusCode.OK, tsam.ValidateExport(base.ActorCompanyId, model.EmployeeIds, model.StartDate, model.StopDate));
        }

        [HttpPost]
        [Route("TimeSalaryExport/")]
        public IHttpActionResult ExportSalary(ExportSalaryModel model)
        {
            return Content(HttpStatusCode.OK, tsam.Export(model.EmployeeIds, model.StartDate, model.StopDate, model.ExportTarget, base.ActorCompanyId, base.UserId, SoeModule.Time, model.LockPeriod, model.IsPreliminary));
        }

        [HttpPost]
        [Route("TimeSalaryExport/SendEmail/{timeSalaryExportId:int}")]
        public IHttpActionResult SendEmailToPayrollAdministrator(int timeSalaryExportId)
        {
            return Content(HttpStatusCode.OK, emm.SendEmailToPayrollAdministrator(base.ActorCompanyId, timeSalaryExportId));
        }

        [HttpPost]
        [Route("TimeSalaryExport/SendSFTP/{timeSalaryExportId:int}")]
        public IHttpActionResult SendPayrollToSftp(int timeSalaryExportId)
        {
            return Content(HttpStatusCode.OK, cm.SendPayrollToSftp(base.ActorCompanyId, timeSalaryExportId));
        }

        [HttpDelete]
        [Route("TimeSalaryExport/{timeSalaryExportId:int}")]
        public IHttpActionResult UndoSalaryExport(int timeSalaryExportId)
        {
            return Content(HttpStatusCode.OK, tsam.UndoExportInBatch(timeSalaryExportId, base.ActorCompanyId, base.UserId));
        }

        #endregion

        #region TimeStamp

        [HttpGet]
        [Route("TimeStamp/{timeStampEntryId:int}")]
        public IHttpActionResult GetTimeStamp(int timeStampEntryId)
        {
            return Content(HttpStatusCode.OK, tsm.GetTimeStampEntry(timeStampEntryId).ToDTO());
        }

        [HttpGet]
        [Route("TimeStamp/UserAgentClientInfo/{timeStampEntryId:int}")]
        public IHttpActionResult GetTimeStampEntryUserAgentClientInfo(int timeStampEntryId)
        {
            return Content(HttpStatusCode.OK, tsm.GetTimeStampEntryUserAgentClientInfo(base.ActorCompanyId, timeStampEntryId));
        }

        [HttpGet]
        [Route("TimeStamp/CreateTimeStampsAccourdingToSchedule/{timeScheduleTemplatePeriodId:int}/{dateString}/{employeeId:int}/{employeeGroupId:int}")]
        public IHttpActionResult CreateTimeStampsAccourdingToSchedule(int timeScheduleTemplatePeriodId, string dateString, int employeeId, int employeeGroupId)
        {
            return Content(HttpStatusCode.OK, tsm.CreateTimeStampsAccourdingToSchedule(timeScheduleTemplatePeriodId, base.BuildDateTimeFromString(dateString, true).Value, employeeId, employeeGroupId, base.ActorCompanyId, base.UserId));
        }

        [HttpGet]
        [Route("TimeStamp/TimeStampAddition/{isMySelf:bool}")]
        public IHttpActionResult GetTimeStampAdditions(bool isMySelf)
        {
            return Content(HttpStatusCode.OK, tsm.GetTimeStampAdditions(base.ActorCompanyId, isMySelf));
        }

        [HttpPost]
        [Route("TimeStamp/Search/")]
        public IHttpActionResult SearchTimeStamps(SearchTimeStampModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, tsm.GetTimeStampEntriesDTO(model.dateFrom, model.dateTo, model.EmployeeIds, base.ActorCompanyId));
        }

        [HttpPost]
        [Route("TimeStamp/Save/")]
        public IHttpActionResult SaveTimeStamps(List<TimeStampEntryDTO> items)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, tsm.SaveAdjustedTimeStampEntries(items));
        }

        #endregion

        #region TimeTerminal

        [HttpGet]
        [Route("TimeTerminal")]
        public IHttpActionResult GetTimeTerminals(HttpRequestMessage message)
        {
            int type = message.GetIntValueFromQS("type");
            bool onlyActive = message.GetBoolValueFromQS("onlyActive");
            bool onlyRegistered = message.GetBoolValueFromQS("onlyRegistered");
            bool onlySynchronized = message.GetBoolValueFromQS("onlySynchronized");
            bool loadSettings = message.GetBoolValueFromQS("loadSettings");
            bool loadCompanies = message.GetBoolValueFromQS("loadCompanies");
            bool loadTypeNames = message.GetBoolValueFromQS("loadTypeNames");
            bool ignoreLimitToAccount = message.GetBoolValueFromQS("ignoreLimitToAccount");

            return Content(HttpStatusCode.OK, tsm.GetTimeTerminals(base.ActorCompanyId, (TimeTerminalType)type, onlyActive, onlyRegistered, onlySynchronized, loadSettings, loadCompanies, loadTypeNames, ignoreLimitToAccount).ToDTOs(false, true, true));
        }

        [HttpGet]
        [Route("TimeTerminal/{timeTerminalId:int}")]
        public IHttpActionResult GetTimeTerminal(int timeTerminalId)
        {
            return Content(HttpStatusCode.OK, tsm.GetTimeTerminalDiscardState(timeTerminalId, true).ToDTO(false, true, false));
        }

        [HttpGet]
        [Route("TimeTerminal/AccountDim/{timeTerminalId:int}/{dimNr:int}")]
        public IHttpActionResult GetTimeTerminalAccountDim(int timeTerminalId, int dimNr)
        {
            return Content(HttpStatusCode.OK, tsm.GetTimeTerminalAccountDim(base.ActorCompanyId, timeTerminalId, dimNr));
        }

        [HttpGet]
        [Route("TimeTerminal/TimeZone")]
        public IHttpActionResult GetTimeZones()
        {
            return Content(HttpStatusCode.OK, tsm.GetTimeZones());
        }

        [HttpGet]
        [Route("TimeTerminal/GroupName")]
        public IHttpActionResult GetTerminalGroupNames()
        {
            return Content(HttpStatusCode.OK, tsm.GetTerminalGroupNames(base.ActorCompanyId));
        }

        [HttpGet]
        [Route("TimeTerminal/HasAnyTerminalSpecifiedBoolSetting/{settingType:int}")]
        public IHttpActionResult HasAnyTerminalSpecifiedBoolSetting(int settingType)
        {
            return Content(HttpStatusCode.OK, tsm.HasAnyTerminalSpecifiedBoolSetting(base.ActorCompanyId, (TimeTerminalSettingType)settingType));
        }

        [HttpGet]
        [Route("TimeTerminal/HasAnyTerminalSpecifiedIntSetting/{settingType:int}/{allowZero:bool}")]
        public IHttpActionResult HasAnyTerminalSpecifiedIntSetting(int settingType, bool allowZero)
        {
            return Content(HttpStatusCode.OK, tsm.HasAnyTerminalSpecifiedIntSetting(base.ActorCompanyId, (TimeTerminalSettingType)settingType, allowZero));
        }

        [HttpGet]
        [Route("TimeTerminal/GetAnyTerminalSpecifiedIntSetting/{settingType:int}")]
        public IHttpActionResult GetAnyTerminalSpecifiedIntSetting(int settingType)
        {
            return Content(HttpStatusCode.OK, tsm.GetAnyTerminalSpecifiedIntSetting(base.ActorCompanyId, (TimeTerminalSettingType)settingType));
        }

        [HttpPost]
        [Route("TimeTerminal")]
        public IHttpActionResult SaveTimeTerminal(TimeTerminalDTO model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, tsm.SaveTimeTerminal(model, base.ActorCompanyId));
        }

        #endregion
    }
}
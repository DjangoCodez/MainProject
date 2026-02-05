using Soe.WebApi.Binders;
using Soe.WebApi.Extensions;
using Soe.WebApi.Models;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Core.TimeEngine;
using SoftOne.Soe.Business.Core.TimeTree;
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
    [RoutePrefix("Time/Payroll")]
    public class PayrollController : SoeApiController
    {
        #region Variables

        private readonly AccountManager acm;
        private readonly TimeTreePayrollManager ttpm;
        private readonly EmployeeManager em;
        private readonly GeneralManager gm;
        private readonly PayrollManager pm;
        private readonly ProductManager prm;
        private readonly TimeWorkAccountManager twam;
        private readonly TimeEngineManager tem;
        private readonly TimePeriodManager tpm;
        private readonly TimeSalaryManager tsm;
        private readonly TimeTransactionManager ttm;
        private readonly UserManager um;
        private readonly FeatureManager fm;
        private readonly ExtraFieldManager efm;

        #endregion

        #region Constructor

        public PayrollController(AccountManager acm, TimeTreePayrollManager ttpm, EmployeeManager em, GeneralManager gm, PayrollManager pm, ProductManager prm, TimeWorkAccountManager twam, TimeEngineManager tem, TimePeriodManager tpm, TimeSalaryManager tsm, TimeTransactionManager ttm, UserManager um, FeatureManager fm, ExtraFieldManager efm)
        {
            this.acm = acm;
            this.ttpm = ttpm;
            this.em = em;
            this.gm = gm;
            this.pm = pm;
            this.prm = prm;
            this.twam = twam;
            this.tem = tem;
            this.tpm = tpm;
            this.tsm = tsm;
            this.ttm = ttm;
            this.um = um;
            this.fm = fm;
            this.efm = efm;
        }

        #endregion

        #region AccountProvisionBase

        [HttpGet]
        [Route("AccountProvisionBase/Columns/{timePeriodId:int}")]
        public IHttpActionResult GetAccountProvisionBaseColumns(int timePeriodId)
        {
            return Content(HttpStatusCode.OK, pm.GetAccountProvisionBaseColumns(timePeriodId, base.ActorCompanyId));
        }

        [HttpGet]
        [Route("AccountProvisionBase/{timePeriodId:int}")]
        public IHttpActionResult GetAccountProvisionBase(int timePeriodId)
        {
            return Content(HttpStatusCode.OK, pm.GetAccountProvisionBase(timePeriodId, base.ActorCompanyId));
        }

        [HttpPost]
        [Route("AccountProvisionBase")]
        public IHttpActionResult SaveAccountProvisionBase(List<AccountProvisionBaseDTO> provisions)
        {
            return Content(HttpStatusCode.OK, tem.SaveAccountProvisionBase(provisions));
        }

        [HttpPost]
        [Route("AccountProvisionBase/Lock/{timePeriodId:int}")]
        public IHttpActionResult LockAccountProvisionBase(int timePeriodId)
        {
            return Content(HttpStatusCode.OK, tem.LockAccountProvisionBase(timePeriodId));
        }

        [HttpPost]
        [Route("AccountProvisionBase/Unlock/{timePeriodId:int}")]
        public IHttpActionResult UnLockAccountProvisionBase(int timePeriodId)
        {
            return Content(HttpStatusCode.OK, tem.UnLockAccountProvisionBase(timePeriodId));
        }

        #endregion

        #region AccountProvisionTransaction

        [HttpGet]
        [Route("AccountProvisionTransaction/{timePeriodId:int}")]
        public IHttpActionResult GetAccountProvisionTransactions(int timePeriodId)
        {
            return Content(HttpStatusCode.OK, pm.GetAccountProvisionTransactions(timePeriodId, base.ActorCompanyId, base.UserId));
        }

        [HttpPost]
        [Route("AccountProvisionTransaction/Update")]
        public IHttpActionResult UpdateAccountProvisionTransactions(AccountProvisionTransactionsModel model)
        {
            return Content(HttpStatusCode.OK, tem.UpdateAccountProvisionTransactions(model.Transactions));
        }

        [HttpPost]
        [Route("AccountProvisionTransaction/Attest")]
        public IHttpActionResult SaveAttestForAccountProvision(AccountProvisionTransactionsModel model)
        {
            return Content(HttpStatusCode.OK, tem.SaveAttestForAccountProvision(model.Transactions));
        }

        #endregion

        #region MassRegistration

        [HttpGet]
        [Route("MassRegistration")]
        public IHttpActionResult GetMassRegistrations(HttpRequestMessage message)
        {
            if (message.HasAcceptValue(HttpExtensions.ACCEPT_GRID_DTO))
                return Content(HttpStatusCode.OK, pm.GetMassRegistrationTemplateHeads(base.ActorCompanyId, false, false).ToGridDTOs());

            bool loadRows = message.GetBoolValueFromQS("loadRows");
            return Content(HttpStatusCode.OK, pm.GetMassRegistrationTemplateHeads(base.ActorCompanyId, loadRows, true).ToDTOs(loadRows));
        }

        [HttpGet]
        [Route("MassRegistration/{massRegistrationTemplateHeadId:int}")]
        public IHttpActionResult GetMassRegistration(int massRegistrationTemplateHeadId)
        {
            List<AccountDimDTO> dims = acm.GetAccountDimsByCompany(base.ActorCompanyId, false, false, true, true, false, false).ToDTOs().ToList();

            return Content(HttpStatusCode.OK, pm.GetMassRegistrationTemplateHead(base.ActorCompanyId, massRegistrationTemplateHeadId, true, true, true, true, true).ToDTO(true, null, dims));
        }

        [HttpPost]
        [Route("MassRegistration")]
        public IHttpActionResult SaveMassRegistration(MassRegistrationTemplateHeadDTO model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, pm.SaveMassRegistrationTemplate(model, base.ActorCompanyId));
        }

        [HttpPost]
        [Route("MassRegistration/CreateTransactions")]
        public IHttpActionResult CreateTransactions(MassRegistrationTemplateHeadDTO model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, tem.CreateAddedTransactionsFromTemplate(model));
        }

        [HttpPost]
        [Route("MassRegistration/Export/")]
        public IHttpActionResult ExportMassRegistration(MassRegistrationTemplateHeadDTO head)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, pm.ExportMassRegistration(head));
        }

        [HttpPost]
        [Route("MassRegistration/Import/{type:int}/{dateString}/{clearRows:bool}")]
        public async Task<IHttpActionResult> ImportMassRegistration(int type, string dateString, bool clearRows)
        {
            if (Request.Content.IsMimeMultipartContent())
            {
                var data = await Request.Content.ParseMultipartAsync();

                HttpPostedFile file = data.Files["file"];
                if (file != null)
                    return Content(HttpStatusCode.OK, pm.ImportMassRegistration(new MemoryStream(file.File), (TermGroup_MassRegistrationImportType)type, BuildDateTimeFromString(dateString, true), base.ActorCompanyId));
            }

            throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
        }

        [HttpDelete]
        [Route("MassRegistration/{massRegistrationTemplateHeadId:int}/{deleteTransactions:bool}")]
        public IHttpActionResult DeleteMassRegistration(int massRegistrationTemplateHeadId, bool deleteTransactions)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, pm.DeleteMassRegistrationTemplateHead(base.ActorCompanyId, massRegistrationTemplateHeadId, deleteTransactions));
        }

        #endregion

        #region TimeWorkAccount

        [HttpGet]
        [Route("TimeWorkAccount")]
        public IHttpActionResult GetTimeWorkAccounts()
        {
            return Content(HttpStatusCode.OK, twam.GetTimeWorkAccounts().ToDTOs());
        }

        #endregion

        #region TimeWorkAccountYearEmployee

        [HttpGet]
        [Route("TimeWorkAccountYearEmployee/{employeeId:int}")]
        public IHttpActionResult GetTimeWorkYearEmployee(int employeeId)
        {
            return Content(HttpStatusCode.OK, twam.GetTimeWorkAccountYearEmployees(employeeId, base.ActorCompanyId).ToDTOs());
        }

        #endregion

        #region EmployeeTimeWorkAccount

        [HttpGet]
        [Route("EmployeeTimeWorkAccount/{employeeId:int}/{loadAccount:bool}")]
        public IHttpActionResult GetEmployeeTimeWorkAccount(int employeeId, bool loadAccount)
        {
            return Content(HttpStatusCode.OK, em.GetEmployeeTimeWorkAccounts(employeeId, loadAccount).ToDTOs());
        }

        #endregion

        #region Payment

        [HttpGet]
        [Route("Payment")]
        public IHttpActionResult GetTimeSalaryPaymentExports()
        {
            return Content(HttpStatusCode.OK, tsm.GetTimeSalaryPaymentExportsForGrid(base.ActorCompanyId, base.UserId));
        }

        [HttpPost]
        [Route("Payment")]
        public IHttpActionResult ExportSalaryPayment(ExportSalaryPaymentModel model)
        {
            return Content(HttpStatusCode.OK, tsm.ExportSalaryPayment(base.ActorCompanyId, model.TimePeriodHeadId, model.TimePeriodId, model.EmployeeIds, model.PublishDate, model.DebitDate, base.UserId, base.RoleId));
        }

        [HttpPost]
        [Route("PaymentExtended")]
        public IHttpActionResult ExportSalaryPaymentExtendedSelection(ExportSalaryPaymentExtendedModel model)
        {
            return Content(HttpStatusCode.OK, tsm.ExportSalaryPaymentExtendedSelection(base.ActorCompanyId, base.UserId, base.RoleId, model.BasedOnTimeSalarPaymentExportId, model.CurrencyDate, model.CurrencyRate, model.Currency));
        }

        [HttpPost]
        [Route("Payment/SalarySpecificationPublishDate/{timeSalaryPaymentExportId:int}/{salarySpecificationPublishDate}")]
        public IHttpActionResult SetSalarySpecificationPublishDate(int timeSalaryPaymentExportId, string salarySpecificationPublishDate)
        {
            return Content(HttpStatusCode.OK, tsm.SetSalarySpecificationPublishDate(base.ActorCompanyId, timeSalaryPaymentExportId, BuildDateTimeFromString(salarySpecificationPublishDate, true), base.UserId, base.RoleId));
        }

        [HttpPost]
        [Route("Payment/Warnings")]
        public IHttpActionResult GetSalaryPaymentExportWarnings(ExportSalaryPaymentModel model)
        {
            return Content(HttpStatusCode.OK, tsm.GetSalaryPaymentExportWarnings(base.ActorCompanyId, model.EmployeeIds, model.TimePeriodId));
        }

        [HttpDelete]
        [Route("Payment/{timeSalaryPaymentExportId:int}")]
        public IHttpActionResult UndoSalaryPaymentExport(int timeSalaryPaymentExportId)
        {
            return Content(HttpStatusCode.OK, tsm.UndoSalaryPaymentExport(base.ActorCompanyId, timeSalaryPaymentExportId, base.UserId));
        }

        #endregion

        #region PayrollCalculation

        [HttpPost]
        [Route("PayrollCalculation/Tree/")]
        public IHttpActionResult GetPayrollCalculationTree(PayrollCalculationTreeModel model)
        {
            model.Beautify();
            return Content(HttpStatusCode.OK, ttpm.GetPayrollCalculationTree(model.Grouping, model.Sorting, model.TimePeriodId, model.Settings));
        }

        [HttpPost]
        [Route("PayrollCalculation/RefreshTree/")]
        public IHttpActionResult RefreshPayrollCalculationTree(RefreshPayrollCalculationTreeModel model)
        {
            return Content(HttpStatusCode.OK, ttpm.RefreshPayrollCalculationTree(model.Tree, model.TimePeriodId, model.Settings));
        }

        [HttpPost]
        [Route("PayrollCalculation/TreeWarnings/")]
        public IHttpActionResult GetPayrollCalculationTreeWarnings(PayrollCalculationTreeWarningsModel model)
        {
            return Content(HttpStatusCode.OK, ttpm.GetPayrollCalculationTreeWarnings(model.Tree, model.EmployeeIds, model.TimePeriodId, model.WarningFilter, model.FlushCache));
        }

        [HttpPost]
        [Route("PayrollCalculation/EmployeePeriods/")]
        public IHttpActionResult GetPayrollCalculationEmployeePeriods(GetPayrollCalculationEmployeePeriodsModel model)
        {
            return Content(HttpStatusCode.OK, ttpm.GetPayrollCalculationEmployeePeriods(base.ActorCompanyId, model.TimePeriodId, model.VisibleEmployeeIds, model.CacheKeyToUse, model.FlushCache, model.IgnoreEmploymentStopDate));
        }

        [HttpPost]
        [Route("PayrollCalculation/Employee/RecalculatePayrollPeriod/{employeeId:int}/{timePeriodId:int}/{includeScheduleTransactions:bool}/{ignoreEmploymentStopDate:bool}")]
        public IHttpActionResult RecalculatePayrollPeriod(int employeeId, int timePeriodId, bool includeScheduleTransactions, bool ignoreEmploymentStopDate)
        {
            return Content(HttpStatusCode.OK, tem.RecalculatePayrollPeriod(employeeId, timePeriodId, includeScheduleTransactions, ignoreEmploymentStopDate));
        }

        [HttpPost]
        [Route("PayrollCalculation/Employees/RecalculatePayrollPeriod/")]
        public IHttpActionResult RecalculatePayrollPeriod(RecalculatePayrollPeriodModel model)
        {
            Guid key = Guid.Parse(model.Key);
            return Content(HttpStatusCode.OK, tem.RecalculatePayrollPeriod(key, model.EmployeeIds, model.TimePeriodId, model.IncludeScheduleTransactions, model.IgnoreEmploymentStopDate));
        }

        [HttpPost]
        [Route("PayrollCalculation/Employee/RecalculateAccounting/")]
        public IHttpActionResult RecalculateAccounting(EmployeesForTimePeriodModel model)
        {
            return Content(HttpStatusCode.OK, tem.RecalculateAccountingFromPayroll(model.EmployeeIds, model.TimePeriodId));
        }

        [HttpPost]
        [Route("PayrollCalculation/Employees/RecalculateExportedEmploymentTax/")]
        public IHttpActionResult RecalculateExportedEmploymentTax(EmployeesForTimePeriodModel model)
        {
            return Content(HttpStatusCode.OK, tem.RecalculateExportedEmploymentTaxJOB(model.EmployeeIds, model.TimePeriodId));
        }

        //[HttpPost]
        //[Route("PayrollCalculation/Employees/RecalculatePayrollPeriod/")]
        //public IHttpActionResult RecalculatePayrollPeriod(RecalculatePayrollPeriodModel model)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return Error(HttpStatusCode.BadRequest, ModelState, null, null);
        //    }
        //    else
        //    {
        //        Guid key = Guid.Parse(model.Key);
        //        var workingThread = new Thread(() => tem.RecalculatePayrollPeriod(key, model.EmployeeIds, model.TimePeriodId, model.IncludeScheduleTransactions, info: monitor.RegisterNewProgressProcess(key), monitor: monitor));
        //        workingThread.Start();
        //        return Content(HttpStatusCode.OK, new SoeProgressInfo(key));
        //    }
        //}
        [HttpGet]
        [Route("PayrollCalculation/PayrollWarnings/{employeeId:int}/{employeeTimePeriodId:int}/{showDeleted:bool}/")]
        public IHttpActionResult GetPayrollWarningsForEmployee(int employeeId, int employeeTimePeriodId, bool showDeleted)
        {
            return Content(HttpStatusCode.OK, pm.GetPayrollWarnings(base.ActorCompanyId, employeeId, employeeTimePeriodId, null, showDeleted, loadOutcomes: true, setTerms: true).ToDTOs(loadChanges: true).ToList());
        }
        [HttpPost]
        [Route("PayrollCalculation/PayrollWarnings/Run/")]
        public IHttpActionResult RunPayrollControllForEmployee(PayrollWarningsCalculateModel model)
        {
            return Content(HttpStatusCode.OK, tem.RecalculatePayrollControll(model.EmployeeIds, model.TimePeriodId));
        }
        [HttpPost]
        [Route("PayrollCalculation/PayrollWarningsGroup/")]
        public IHttpActionResult GetPayrollWarningsForGroop(PayrollWarningsGroupModel model)
        {
            return Content(HttpStatusCode.OK, pm.GetPayrollWarningsForGroup(base.ActorCompanyId, model.EmployeeIds, model.TimePeriodId, model.ShowDeleted, loadOutcomes: true, setTerms: true).ToDTOs(loadChanges: true).ToList());
        }
        [HttpGet]
        [Route("PayrollCalculation/Employee/Chart/SalaryHistory/{timePeriodId:int}/{employeeId:int}")]
        public IHttpActionResult GetPayrollHistoryForEmployee(int employeeId, int timePeriodId)
        {
            return Content(HttpStatusCode.OK, ttpm.GetSalaryHistoryForEmployee(base.ActorCompanyId, timePeriodId, employeeId));
        }

        [HttpGet]
        [Route("PayrollCalculation/Employees/RecalculatePayrollPeriodResult/{key}")]
        public IHttpActionResult GetRecalculatePayrollPeriodForEmployeesResult(string key)
        {
            return Content(HttpStatusCode.OK, monitor.GetResult(Guid.Parse(key)));
        }

        [HttpGet]
        [Route("PayrollCalculation/Products/{timePeriodId:int}/{employeeId:int}/{showAllTransactions:bool}")]
        public IHttpActionResult GetPayrollCalculationProducts(int timePeriodId, int employeeId, bool showAllTransactions)
        {
            return Content(HttpStatusCode.OK, ttpm.GetPayrollCalculationProducts(base.ActorCompanyId, timePeriodId, employeeId, showAllTransactions: showAllTransactions));
        }

        [HttpPost]
        [Route("PayrollCalculation/PeriodSum/")]
        public IHttpActionResult GetPayrollCalculationPeriodSum(GetPayrollCalculationPeriodSumModel model)
        {
            return Content(HttpStatusCode.OK, PayrollRulesUtil.CalculateSum(model.PayrollCalculationProducts));
        }

        [HttpPost]
        [Route("PayrollCalculation/Employee/LockPayrollPeriod/{employeeId:int}/{timePeriodId:int}")]
        public IHttpActionResult LockPayrollPeriod(int employeeId, int timePeriodId)
        {
            return Content(HttpStatusCode.OK, tem.LockPayrollPeriod(employeeId, timePeriodId, base.RoleId, false));
        }

        [HttpPost]
        [Route("PayrollCalculation/Employees/LockPayrollPeriod/")]
        public IHttpActionResult LockPayrollPeriod(EmployeesForTimePeriodModel model)
        {
            return Content(HttpStatusCode.OK, tem.LockPayrollPeriod(model.EmployeeIds, model.TimePeriodId, base.RoleId, false));
        }

        [HttpPost]
        [Route("PayrollCalculation/Employee/UnLockPayrollPeriod/{employeeId:int}/{timePeriodId:int}")]
        public IHttpActionResult UnLockPayrollPeriod(int employeeId, int timePeriodId)
        {
            return Content(HttpStatusCode.OK, tem.UnLockPayrollPeriod(employeeId, timePeriodId));
        }

        [HttpPost]
        [Route("PayrollCalculation/Employees/UnLockPayrollPeriod/")]
        public IHttpActionResult UnLockPayrollPeriod(EmployeesForTimePeriodModel model)
        {
            return Content(HttpStatusCode.OK, tem.UnLockPayrollPeriod(model.EmployeeIds, model.TimePeriodId));

        }

        [HttpPost]
        [Route("PayrollCalculation/Employees/Chart/AverageSalaryCost/")]
        public IHttpActionResult GetAverageSalaryCostForEmployees(EmployeesForTimePeriodModel model)
        {
            return Content(HttpStatusCode.OK, ttpm.GetAverageSalaryCostForEmployees(base.ActorCompanyId, model.TimePeriodId, model.EmployeeIds));
        }
        [HttpPost]
        [Route("PayrollCalculation/PayrollWarnings/Save/")]
        public IHttpActionResult SavePayrollWarningsForEmployee(List<PayrollControlFunctionOutcomeDTO> model)
        {
            return Content(HttpStatusCode.OK, pm.SavePayrollWarnings(base.ActorCompanyId, model));
        }
        [HttpPost]
        [Route("PayrollCalculation/CreateFinalSalary/")]
        public IHttpActionResult CreateFinalSalaryForEmployee(CreateFinalSalaryModel model)
        {
            return Content(HttpStatusCode.OK, tem.CreateFinalSalary(model.EmployeeId, model.TimePeriodId, model.CreateReport));
        }

        [HttpPost]
        [Route("PayrollCalculation/CreateFinalSalaries/")]
        public IHttpActionResult CreateFinalSalaryForEmployees(CreateFinalSalariesModel model)
        {
            return Content(HttpStatusCode.OK, tem.CreateFinalSalaries(model.EmployeeIds, model.TimePeriodId, model.CreateReport));
        }

        [HttpDelete]
        [Route("PayrollCalculation/DeleteFinalSalary/{employeeId:int}/{timePeriodId:int}")]
        public IHttpActionResult DeleteFinalSalary(int employeeId, int timePeriodId)
        {
            return Content(HttpStatusCode.OK, tem.DeleteFinalSalary(employeeId, timePeriodId));
        }

        [HttpDelete]
        [Route("PayrollCalculation/DeleteFinalSalaries/{employeeIds}/{timePeriodId:int}")]
        public IHttpActionResult DeleteFinalSalaries(string employeeIds, int timePeriodId)
        {
            return Content(HttpStatusCode.OK, tem.DeleteFinalSalaries(StringUtility.SplitNumericList(employeeIds), timePeriodId));
        }

        [HttpDelete]
        [Route("PayrollCalculation/ClearPayrollCalculation/{employeeId:int}/{timePeriodId:int}")]
        public IHttpActionResult ClearPayrollCalculation(int employeeId, int timePeriodId)
        {
            return Content(HttpStatusCode.OK, tem.ClearPayrollCalculation(employeeId, timePeriodId));
        }
        [HttpGet]
        [Route("PayrollCalculation/GetUnhandledPayrollTransactions/{employeeId:int}/{startDate}/{stopDate}/{isBackwards:bool}")]
        public IHttpActionResult GetUnhandledPayrollTransactions(int employeeId, string startDate, string stopDate, bool isBackwards)
        {
            return Content(HttpStatusCode.OK, tem.GetUnhandledPayrollTransactions(employeeId, BuildDateTimeFromString(startDate, true), BuildDateTimeFromString(stopDate, true), isBackwards));
        }

        [HttpPost]
        [Route("PayrollCalculation/AssignPayrollTransactionsToTimePeriod")]
        public IHttpActionResult AssignPayrollTransactionsToTimePeriod(AssignPayrollTransactionsToTimePeriodModel model)
        {
            return Content(HttpStatusCode.OK, tem.AssignPayrollTransactionsToTimePeriod(model.Transactions, model.ScheduleTransactions, model.TimePeriod, model.PeriodType, model.EmployeeId));
        }

        [HttpPost]
        [Route("PayrollCalculation/AddedTransaction")]
        public IHttpActionResult SaveAddedTransaction(SaveAddedTransactionModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, tem.SaveAddedTransaction(model.Transaction, null, model.AccountingSettings, model.EmployeeId, model.TimePeriodId, model.IgnoreEmploymentHasEnded));
        }

        [HttpGet]
        [Route("PayrollCalculation/EmployeeTimePeriodProductSetting/{employeeId:int}/{timePeriodId:int}/{payrollProdutId:int}")]
        public IHttpActionResult GetEmployeeTimePeriodProductSetting(int employeeId, int timePeriodId, int payrollProdutId)
        {
            return Content(HttpStatusCode.OK, tpm.GetEmployeeTimePeriodProductSetting(payrollProdutId, employeeId, timePeriodId, base.ActorCompanyId));
        }

        [HttpPost]
        [Route("PayrollCalculation/EmployeeTimePeriodProductSetting")]
        public IHttpActionResult SaveEmployeeTimePeriodProductSetting(SaveEmployeeTimePeriodProductSettingModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, tpm.SaveEmployeeTimePeriodProductSetting(model.TimePeriodId, model.EmployeeId, model.setting, base.ActorCompanyId));
        }

        [HttpDelete]
        [Route("PayrollCalculation/EmployeeTimePeriodProductSetting/{employeeTimePeriodProductSettingId:int}")]
        public ActionResult DeleteEmployeeTimePeriodProductSetting(int employeeTimePeriodProductSettingId)
        {
            return tpm.DeleteEmployeeTimePeriodProductSetting(employeeTimePeriodProductSettingId);
        }

        [HttpGet]
        [Route("PayrollCalculation/FixedPayrollRow/{employeeId:int}/{timePeriodId:int}")]
        public IHttpActionResult GetEmployeeFixedPayrollRows(int employeeId, int timePeriodId)
        {
            return Content(HttpStatusCode.OK, em.GetEmployeeFixedPayrollRows(base.ActorCompanyId, employeeId, timePeriodId, true));
        }

        [HttpPost]
        [Route("PayrollCalculation/FixedPayrollRow")]
        public IHttpActionResult SaveFixedPayrollRows(SaveFixedPayrollRowsModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, tem.SaveFixedPayrollRows(model.rows, model.employeeId));
        }

        [HttpGet]
        [Route("PayrollCalculation/PayrollSlipDataStorageId/{employeeId:int}/{timePeriodId:int}")]
        public IHttpActionResult GetPayrollSlipDataStorageId(int employeeId, int timePeriodId)
        {
            return Content(HttpStatusCode.OK, gm.GetDataStorageId(SoeDataStorageRecordType.PayrollSlipXML, timePeriodId, employeeId, base.ActorCompanyId));
        }

        [HttpGet]
        [Route("PayrollCalculation/EmploymentTaxBasisBeforeGivenPeriod/{timePeriodId:int}/{employeeId:int}")]
        public IHttpActionResult GetEmploymentTaxBasisBeforeGivenPeriod(int timePeriodId, int employeeId)
        {
            return Content(HttpStatusCode.OK, tpm.GetEmploymentTaxBasisBeforeGivenPeriod(base.ActorCompanyId, timePeriodId, employeeId));
        }

        #endregion

        #region PayrollGroup

        [HttpGet]
        [Route("PayrollGroup")]
        public IHttpActionResult GetPayrollGroups(HttpRequestMessage message)
        {
            if (message.HasAcceptValue(HttpExtensions.ACCEPT_GRID_DTO))
                return Content(HttpStatusCode.OK, pm.GetPayrollGroups(base.ActorCompanyId, loadTimePeriods: true, onlyActive: false).ToGridDTOs());
            else if (message.HasAcceptValue(HttpExtensions.ACCEPT_SMALL_DTO))
                return Content(HttpStatusCode.OK, pm.GetPayrollGroupsSmall(base.ActorCompanyId, message.GetBoolValueFromQS("addEmptyRow"), true));

            return Content(HttpStatusCode.OK, pm.GetPayrollGroups(base.ActorCompanyId).ToDTOs());
        }

        [HttpGet]
        [Route("PayrollGroup/{payrollGroupId:int}/{includePriceTypes:bool}/{includePriceFormulas:bool}/{includeSettings:bool}/{includePayrollGroupReports:bool}/{includeTimePeriod:bool}/{includeAccounts:bool}/{includePayrollGroupVacationGroup:bool}/{includePayrollGroupPayrollProduct:bool}")]
        public IHttpActionResult GetPayrollGroup(int payrollGroupId, bool includePriceTypes, bool includePriceFormulas, bool includeSettings, bool includePayrollGroupReports, bool includeTimePeriod, bool includeAccounts, bool includePayrollGroupVacationGroup, bool includePayrollGroupPayrollProduct)
        {
            return Content(HttpStatusCode.OK, pm.GetPayrollGroup(payrollGroupId, false, includePriceTypes, includePriceFormulas, includeSettings, includePayrollGroupReports, includeTimePeriod, includeAccounts, includePayrollGroupVacationGroup, includePayrollGroupPayrollProduct, loadExternalCode: true).ToDTO(includePriceTypes, includePriceFormulas, includeSettings, includePayrollGroupReports, includeTimePeriod, includeAccounts, includePayrollGroupVacationGroup, includePayrollGroupPayrollProduct));
        }

        [HttpGet]
        [Route("PayrollGroup/Reports/{checkRolePermission:bool}")]
        public IHttpActionResult GetCompanyPayrollGroupReports(bool checkRolePermission)
        {
            return Content(HttpStatusCode.OK, pm.GetCompanyPayrollGroupReports(base.ActorCompanyId, checkRolePermission, base.RoleId));
        }

        [HttpGet]
        [Route("PayrollGroup/PriceTypesExists/{payrollGroupId:int}/{priceTypeIds}")]
        public IHttpActionResult PriceTypesExistsInPayrollGroup(int payrollGroupId, string priceTypeIds)
        {
            return Content(HttpStatusCode.OK, pm.PriceTypesExistsInPayrollGroup(payrollGroupId, StringUtility.SplitNumericList(priceTypeIds, nullIfEmpty: true)));
        }

        [HttpPost]
        [Route("PayrollGroup")]
        public IHttpActionResult SavePayrollGroup(PayrollGroupDTO payrollGroup)
        {
            return Content(HttpStatusCode.OK, pm.SavePayrollGroup(payrollGroup));
        }

        [HttpDelete]
        [Route("PayrollGroup/{payrollGroupId:int}")]
        public IHttpActionResult DeletePayrollGroup(int payrollGroupId)
        {
            return Content(HttpStatusCode.OK, pm.DeletePayrollGroup(payrollGroupId));
        }

        #endregion

        #region PayrollGroupAccount

        [HttpGet]
        [Route("PayrollGroupAccount/Dates/{sysCountryId:int}")]
        public IHttpActionResult GetPayrollGroupPriceFormulas(int sysCountryId)
        {
            return Content(HttpStatusCode.OK, pm.GetPayrollGroupAccountDates(sysCountryId));
        }

        #endregion

        #region PayrollGroupPriceFormula

        [HttpGet]
        [Route("PayrollGroupPriceFormula/{payrollGroupId:int}/{showOnEmployee:bool}")]
        public IHttpActionResult GetPayrollGroupPriceFormulas(int payrollGroupId, bool showOnEmployee)
        {
            return Content(HttpStatusCode.OK, pm.GetPayrollGroupPriceFormulas(payrollGroupId, showOnEmployee).ToDTOs());
        }

        #endregion

        #region PayrollGroupPriceType

        [HttpGet]
        [Route("PayrollGroupPriceType/{payrollGroupId:int}/{showOnEmployee:bool}")]
        public IHttpActionResult GetPayrollGroupPriceTypes(int payrollGroupId, bool showOnEmployee)
        {
            return Content(HttpStatusCode.OK, pm.GetPayrollGroupPriceTypes(payrollGroupId, showOnEmployee).ToDTOs(true));
        }

        #endregion

        #region PayrollGroupVacationGroup

        [HttpGet]
        [Route("PayrollGroupVacationGroup/{payrollGroupId:int}/{loadVacationGroupSE:bool}")]
        public IHttpActionResult GetPayrollGroupVacationGroups(int payrollGroupId, bool loadVacationGroupSE)
        {
            return Content(HttpStatusCode.OK, pm.GetPayrollGroupVacationGroups(payrollGroupId, loadVacationGroupSE).ToDTOs(loadVacationGroupSE));
        }

        #endregion

        #region PayrollLevel
        [HttpGet]
        [Route("PayrollLevel")]
        public IHttpActionResult GetPayrollLevels()
        {
            return Content(HttpStatusCode.OK, pm.GetPayrollLevels(base.ActorCompanyId).ToDTOs());
        }

        [HttpGet]
        [Route("PayrollLevel/{payrollLevelId:int}")]
        public IHttpActionResult GetPayrollLevel(int payrollLevelId)
        {
            return Content(HttpStatusCode.OK, pm.GetPayrollLevel(base.ActorCompanyId, payrollLevelId).ToDTO());
        }
        [HttpPost]
        [Route("PayrollLevel")]
        public IHttpActionResult SavePayrollLevel(PayrollLevelDTO model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, pm.SavePayrollLevel(model, base.ActorCompanyId));
        }
        [HttpDelete]
        [Route("PayrollLevel/{payrollLevelId:int}")]
        public IHttpActionResult DeletePayrollLevel(int payrollLevelId)
        {
            return Content(HttpStatusCode.OK, pm.DeletePayrollLevel(payrollLevelId));
        }

        #endregion

        #region PayrollImportHead

        [HttpGet]
        [Route("PayrollImportHead/{includeFile:bool}/{includeEmployees:bool}/{includeScheduleAndTransactionInfo:bool}/{setStatuses:bool}")]
        public IHttpActionResult GetPayrollImportHeads(bool includeFile, bool includeEmployees, bool includeScheduleAndTransactionInfo, bool setStatuses)
        {
            return Content(HttpStatusCode.OK, pm.GetPayrollImportHeads(includeFile, includeEmployees, includeScheduleAndTransactionInfo, setStatuses));
        }

        [HttpPost]
        [Route("PayrollImportHead/Import/{type:int}/{dateString}/{comment}/{skipMissingEmployeeValidation:bool}")]
        public async Task<IHttpActionResult> ImportPayrollImportHead(int type, string dateString, string comment, bool skipMissingEmployeeValidation)
        {
            if (Request.Content.IsMimeMultipartContent())
            {
                var data = await Request.Content.ParseMultipartAsync();

                HttpPostedFile file = data.Files["file"];
                if (file != null)
                    return Content(HttpStatusCode.OK, pm.ImportPayrollImportHead(base.ActorCompanyId, BuildDateTimeFromString(dateString, true).Value, file.File, file.Filename, (TermGroup_PayrollImportHeadFileType)type, comment == Constants.SOE_WEBAPI_STRING_EMPTY ? string.Empty : comment, skipMissingEmployeeValidation));
            }

            throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
        }

        [HttpPost]
        [Route("PayrollImportHead/Validate/")]
        public IHttpActionResult ValidatePayrollImport(RollbackPayrollImportExecuteModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, tem.ValidatePayrollImport(model.PayrollImportHeadId, model.PayrollImportEmployeeIds));
        }

        [HttpPost]
        [Route("PayrollImportHead/Execute/")]
        public IHttpActionResult PayrollImportHeadExecute(PayrollImportExecuteModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, tem.PayrollImport(model.PayrollImportHeadId, model.PayrollImportEmployeeIds));
        }

        [HttpPost]
        [Route("PayrollImportHead/ExecuteRollback/")]
        public IHttpActionResult PayrollImportHeadExecuteRollback(RollbackPayrollImportExecuteModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, tem.PayrollImportRollback(model.PayrollImportHeadId, model.PayrollImportEmployeeIds, false, model.RollbackOutcomeForAllEmployees, false));
        }

        [HttpPost]
        [Route("PayrollImportHead/ExecuteRollbackFile/")]
        public IHttpActionResult PayrollImportHeadExecuteRollbackFile(RollbackPayrollImportExecuteModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, tem.PayrollImportRollback(model.PayrollImportHeadId, model.PayrollImportEmployeeIds, true, false, model.RollbackFileContentForAllEmployees));
        }

        #endregion

        #region PayrollImportEmployee

        [HttpGet]
        [Route("PayrollImportEmployee/{payrollImportHeadId:int}/{includeSchedule:bool}/{includeTransactions:bool}/{includeLinks:bool}/{setStatuses:bool}")]
        public IHttpActionResult GetPayrollImportEmployee(int payrollImportHeadId, bool includeSchedule, bool includeTransactions, bool includeLinks, bool setStatuses)
        {
            return Content(HttpStatusCode.OK, pm.GetPayrollImportEmployees(payrollImportHeadId, includeSchedule: includeSchedule, includeTransactions: includeTransactions, includeTransactionAccounts: includeTransactions, includeLinks: includeLinks, setStatuses: setStatuses));
        }

        [HttpGet]
        [Route("PayrollImportEmployee/Transaction/{payrollImportEmployeeId:int}/{setStatuses:bool}")]
        public IHttpActionResult GetPayrollImportEmployeeTransactions(int payrollImportEmployeeId, bool setStatuses)
        {
            return Content(HttpStatusCode.OK, pm.GetPayrollImportEmployeeTransactions(payrollImportEmployeeId, setStatuses, loadAccountInternalsExtended: true).ToDTOs());
        }

        #endregion

        #region PayrollImportEmployeeTransactions

        [HttpGet]
        [Route("PayrollImportEmployee/Transaction/Link/{payrollImportEmployeeTransactionId:int}")]
        public IHttpActionResult GetPayrollImportEmployeeTransactionLinks(int payrollImportEmployeeTransactionId)
        {
            return Content(HttpStatusCode.OK, pm.GetPayrollImportEmployeeTransactionLinks(payrollImportEmployeeTransactionId, base.ActorCompanyId));
        }

        [HttpPost]
        [Route("PayrollImportEmployee/Transaction")]
        public IHttpActionResult SavePayrollImportEmployeeTransaction(PayrollImportEmployeeTransactionDTO model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, pm.SavePayrollImportEmployeeTransaction(model));
        }

        [HttpPost]
        [Route("PayrollImportEmployee/Transaction/SetAsProcessed")]
        public IHttpActionResult SetPayrollImportEmployeeTransactionAsProcessed(IntModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, pm.SetPayrollImportEmployeeTransactionAsProcessed(model.Id, base.ActorCompanyId));
        }

        [HttpDelete]
        [Route("PayrollImportEmployee/Transaction/{payrollImportEmployeeTransactionId:int}")]
        public IHttpActionResult DeletePayrollImportEmployeeTransaction(int payrollImportEmployeeTransactionId)
        {
            return Content(HttpStatusCode.OK, pm.DeletePayrollImportEmployeeTransaction(payrollImportEmployeeTransactionId));
        }

        #endregion

        #region PayrollImportEmployeeSchedule

        [HttpGet]
        [Route("PayrollImportEmployee/Schedule/{payrollImportEmployeeId:int}")]
        public IHttpActionResult GetPayrollImportEmployeeSchedules(int payrollImportEmployeeId)
        {
            return Content(HttpStatusCode.OK, pm.GetPayrollImportEmployeeSchedules(payrollImportEmployeeId, false).ToDTOs());
        }

        [HttpPost]
        [Route("PayrollImportEmployee/Schedule")]
        public IHttpActionResult SavePayrollImportEmployeeSchedule(PayrollImportEmployeeScheduleDTO model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, pm.SavePayrollImportEmployeeSchedule(model));
        }

        [HttpDelete]
        [Route("PayrollImportEmployee/Schedule/{payrollImportEmployeeScheduleId:int}")]
        public IHttpActionResult DeletePayrollImportEmployeeSchedule(int payrollImportEmployeeScheduleId)
        {
            return Content(HttpStatusCode.OK, pm.DeletePayrollImportEmployeeSchedule(payrollImportEmployeeScheduleId));
        }

        #endregion

        #region PayrollProduct

        [HttpGet]
        [Route("PayrollProduct")]
        public IHttpActionResult GetPayrollProducts(HttpRequestMessage message)
        {
            if (message.HasAcceptValue(HttpExtensions.ACCEPT_GRID_DTO))
                return Content(HttpStatusCode.OK, prm.GetPayrollProducts(base.ActorCompanyId, null, false, false, false, true, true).ToGridDTOs());
            else if (message.HasAcceptValue(HttpExtensions.ACCEPT_GENERIC_TYPE))
                return Content(HttpStatusCode.OK, prm.GetPayrollProductsDict(base.ActorCompanyId, message.GetBoolValueFromQS("addEmptyRow"), message.GetBoolValueFromQS("concatNumberAndName")).ToSmallGenericTypes());
            else if (message.HasAcceptValue(HttpExtensions.ACCEPT_SMALL_DTO))
                return Content(HttpStatusCode.OK, prm.GetPayrollProducts(base.ActorCompanyId, true).ToSmallDTOs());

            return Content(HttpStatusCode.OK, prm.GetPayrollProducts(base.ActorCompanyId, true, checkValidForAddedTransactionDialog: message.GetBoolValueFromQS("checkValidForAddedTransactionDialog")).ToDTOs(false, false, false, false, false));
        }

        [HttpGet]
        [Route("PayrollProduct/Small")]
        public IHttpActionResult GetPayrollProductsSmall(HttpRequestMessage message)
        {
            return Content(HttpStatusCode.OK, prm.GetPayrollProducts(base.ActorCompanyId, true).ToSmallDTOs());
        }

        [HttpGet]
        [Route("PayrollProduct/{productId:int}")]
        public IHttpActionResult GetPayrollProduct(int productId)
        {
            PayrollProductDTO product = prm.GetPayrollProduct(productId, true, true, true, true).ToDTO(true, true, false, true, true);
            if (product != null)
            {
                // Load extra field records for the settings
                foreach (PayrollProductSettingDTO setting in product.Settings)
                {
                    setting.ExtraFields = efm.GetExtraFieldRecords(setting.PayrollProductSettingId, (int)SoeEntityType.PayrollProductSetting, base.ActorCompanyId, true).ToDTOs();
                }
            }

            return Content(HttpStatusCode.OK, product);
        }

        [HttpGet]
        [Route("PayrollProduct/Account/{type:int}/{employeeId:int}/{productId:int}/{projectId:int}/{customerId:int}/{getInternalAccounts:bool}/{dateString}")]
        public IHttpActionResult GetPayrollProductAccount(ProductAccountType type, int employeeId, int productId, int projectId, int customerId, bool getInternalAccounts, string dateString)
        {
            return Content(HttpStatusCode.OK, acm.GetPayrollProductAccount(type, base.ActorCompanyId, employeeId, productId, projectId, customerId, getInternalAccounts, BuildDateTimeFromString(dateString, true)));
        }

        [HttpGet]
        [Route("PayrollProduct/Children/{excludeProductId:int}")]
        public IHttpActionResult GetSelectableChildPayrollProducts(int excludeProductId)
        {
            return Content(HttpStatusCode.OK, prm.GetSelectableChildPayrollProducts(base.ActorCompanyId, excludeProductId).ToSmallGenericTypes());
        }

        [HttpGet]
        [Route("PayrollProduct/PriceTypesAndFormulas/")]
        public IHttpActionResult GetPayrollPriceTypesAndFormulas()
        {
            return Content(HttpStatusCode.OK, prm.GetPayrollPriceTypesAndFormulas(base.ActorCompanyId));
        }

        [HttpPost]
        [Route("PayrollProduct")]
        public IHttpActionResult SavePayrollProduct(PayrollProductDTO model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, prm.SavePayrollProduct(model, base.ActorCompanyId));
        }

        [HttpPost]
        [Route("PayrollProduct/UpdateState")]
        public IHttpActionResult UpdatePayrollProductsState(UpdateEntityStatesModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, prm.ChangePayrollProductStates(model.Dict));
        }

        [HttpDelete]
        [Route("PayrollProduct/{productId:int}")]
        public IHttpActionResult DeletePayrollProducts(int productId)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, prm.DeletePayrollProduct(productId, base.ActorCompanyId));
        }

        #endregion

        #region PayrollPriceFormula

        [HttpGet]
        [Route("PayrollPriceFormula")]
        public IHttpActionResult GetPayrollPriceFormulas(HttpRequestMessage message)
        {
            var onlyActive = true;
            if (message.GetBoolValueFromQS("showInactive"))
                onlyActive = false;

            if (message.HasAcceptValue(HttpExtensions.ACCEPT_GENERIC_TYPE))
                return Content(HttpStatusCode.OK, pm.GetPayrollPriceFormulasDict(base.ActorCompanyId, message.GetBoolValueFromQS("addEmptyRow")).ToSmallGenericTypes());

            return Content(HttpStatusCode.OK, pm.GetPayrollPriceFormulas(base.ActorCompanyId, false, onlyActive).ToDTOs());
        }

        [HttpGet]
        [Route("PayrollPriceFormula/{payrollPriceFormulaId:int}")]
        public IHttpActionResult GetPayrollPriceFormula(int payrollPriceFormulaId)
        {
            return Content(HttpStatusCode.OK, pm.GetPayrollPriceFormula(base.ActorCompanyId, payrollPriceFormulaId).ToDTO());
        }

        [HttpGet]
        [Route("PayrollPriceFormula/PayrollPriceType")]
        public IHttpActionResult GetPayrollPriceTypesForFormulaBuilder()
        {
            return Content(HttpStatusCode.OK, pm.GetPayrollPriceTypesForFormulaBuilder(base.ActorCompanyId));
        }

        [HttpGet]
        [Route("PayrollPriceFormula/FixedValue")]
        public IHttpActionResult GetFixedValuesForFormulaBuilder()
        {
            return Content(HttpStatusCode.OK, pm.GetPayrollPriceFormulaFixedValuesForFormulaBuilder(base.ActorCompanyId));
        }

        [HttpGet]
        [Route("PayrollPriceFormula/PayrollPriceFormula/{excludedFormulaId:int}")]
        public IHttpActionResult GetPayrollPriceFormulasForFormulaBuilder(int excludedFormulaId)
        {
            return Content(HttpStatusCode.OK, pm.GetPayrollPriceFormulasForFormulaBuilder(base.ActorCompanyId, excludedFormulaId));
        }

        [HttpGet]
        [Route("PayrollPriceFormula/EvaluateFormulaGivenEmployeeId/{date}/{employeeId:int}/{productId:int}")]
        public IHttpActionResult EvaluateFormulaGivenEmployeeId(string date, int employeeId, int productId)
        {
            return Content(HttpStatusCode.OK, pm.EvaluatePayrollPriceFormula(base.ActorCompanyId, employeeId, productId, BuildDateTimeFromString(date, true).Value));
        }

        [HttpGet]
        [Route("PayrollPriceFormula/EvaluateFormulaGivenEmploymentId/{date}/{employmentId:int}/{productId:int}/{payrollGroupPriceFormulaId:int}/{payrollProductPriceFormulaId:int}/{payrollPriceFormulaId:int}/{inputValue:decimal}")]
        public IHttpActionResult EvaluateFormulaGivenEmploymentId(string date, int employmentId, int productId, int payrollGroupPriceFormulaId, int payrollProductPriceFormulaId, int payrollPriceFormulaId, decimal inputValue)
        {
            if (!fm.HasRolePermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_Employments_Payroll_Salary_IncludeAllPeriods, Permission.Readonly, RoleId, ActorCompanyId, LicenseId))
            {
                var employment = em.GetEmployment(employmentId);

                if (employment != null)
                {
                    using (CompEntities entities = new CompEntities())
                    {
                        var employee = entities.Employee.FirstOrDefault(f => f.EmployeeId == employment.EmployeeId);
                        if (employee != null)
                        {
                            if (!um.HasPermissionToEmployee(entities, DateTime.Today, DateTime.Today, ActorCompanyId, UserId, RoleId, employee))
                            {
                                PayrollPriceFormulaResultDTO payrollPriceFormulaResultDTO = new PayrollPriceFormulaResultDTO()
                                {
                                    Amount = 0,
                                    Formula = "********",
                                    FormulaExtracted = "********",
                                    FormulaPlain = "********",
                                };

                                return Content(HttpStatusCode.OK, payrollPriceFormulaResultDTO);
                            }
                        }
                    }
                }
            }
            return Content(HttpStatusCode.OK, pm.EvaluatePayrollPriceFormula(base.ActorCompanyId, employmentId, productId, BuildDateTimeFromString(date, true).Value, payrollGroupPriceFormulaId != 0 ? payrollGroupPriceFormulaId : (int?)null, payrollProductPriceFormulaId != 0 ? payrollProductPriceFormulaId : (int?)null, payrollPriceFormulaId != 0 ? payrollPriceFormulaId : (int?)null, inputValue != 0 ? inputValue : (decimal?)null));
        }

        [HttpPost]
        [Route("PayrollPriceFormula/EvaluateFormula")]
        public IHttpActionResult EvaluateFormula(EvaluateFormulaModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, pm.EvaluateFormula(model.Formula, model.Identifiers));
        }

        [HttpPost]
        [Route("PayrollPriceFormula")]
        public IHttpActionResult SavePayrollPriceFormula(PayrollPriceFormulaDTO payrollPriceFormula)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, pm.SavePayrollPriceFormula(payrollPriceFormula, base.ActorCompanyId));
        }

        [HttpDelete]
        [Route("PayrollPriceFormula/{payrollPriceFormulaId:int}")]
        public IHttpActionResult DeletePayrollPriceFormula(int payrollPriceFormulaId)
        {
            return Content(HttpStatusCode.OK, pm.DeletePayrollPriceFormula(payrollPriceFormulaId));
        }

        #endregion

        #region PayrollPriceType

        [HttpGet]
        [Route("PayrollPriceType")]
        public IHttpActionResult GetPayrollPriceTypes(HttpRequestMessage message)
        {
            if (message.HasAcceptValue(HttpExtensions.ACCEPT_GENERIC_TYPE))
                return Content(HttpStatusCode.OK, pm.GetPayrollPriceTypesDict(base.ActorCompanyId, null, message.GetBoolValueFromQS("addEmptyRow"), false).ToSmallGenericTypes());
            else if (message.HasAcceptValue(HttpExtensions.ACCEPT_GRID_DTO))
                return Content(HttpStatusCode.OK, pm.GetPayrollPriceTypes(base.ActorCompanyId, null, false).ToGridDTOs());

            bool includePeriods = message.GetBoolValueFromQS("includePeriods");
            return Content(HttpStatusCode.OK, pm.GetPayrollPriceTypes(base.ActorCompanyId, null, includePeriods).ToDTOs(includePeriods));
        }

        [HttpGet]
        [Route("PayrollPriceType/{payrollPriceTypeId:int}/{includePeriods:bool}")]
        public IHttpActionResult GetPayrollPriceType(int payrollPriceTypeId, bool includePeriods)
        {
            return Content(HttpStatusCode.OK, pm.GetPayrollPriceType(payrollPriceTypeId).ToDTO(includePeriods));
        }

        [HttpPost]
        [Route("PayrollPriceType")]
        public IHttpActionResult SavePayrollPriceType(PayrollPriceTypeDTO payrollPriceType)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, pm.SavePayrollPriceType(payrollPriceType, base.ActorCompanyId));
        }

        [HttpDelete]
        [Route("PayrollPriceType/{payrollPriceTypeId:int}")]
        public IHttpActionResult DeletePayrollPriceType(int payrollPriceTypeId)
        {
            return Content(HttpStatusCode.OK, pm.DeletePayrollPriceType(payrollPriceTypeId));
        }

        #endregion

        #region PayrollStartValueHead

        [HttpGet]
        [Route("PayrollStartValueHead/{includeRows:bool}/{includePayrollProduct:bool}")]
        public IHttpActionResult GetPayrollStartValueHeads(bool includeRows, bool includePayrollProduct)
        {
            return Content(HttpStatusCode.OK, pm.GetPayrollStartValueHeads(base.ActorCompanyId, includeRows, includePayrollProduct).ToDTOs(includeRows, includePayrollProduct));
        }

        [HttpGet]
        [Route("PayrollStartValueHead/{payrollStartValueHeadId:int}/{includeRows:bool}/{includePayrollProduct:bool}/{includeTransaction:bool}")]
        public IHttpActionResult GetPayrollStartValueHead(int payrollStartValueHeadId, bool includeRows, bool includePayrollProduct, bool includeTransaction)
        {
            return Content(HttpStatusCode.OK, pm.GetPayrollStartValueHead(base.ActorCompanyId, payrollStartValueHeadId, includeRows, includePayrollProduct, includeTransaction).ToDTO(includeRows, includePayrollProduct, includeTransaction));
        }

        [HttpPost]
        [Route("PayrollStartValueHead/Import/Add/{dateFromString}/{dateToString}/{importedFrom}")]
        public async Task<IHttpActionResult> ImportPayrollStartValueHeadAdd(string dateFromString, string dateToString, string importedFrom)
        {
            if (Request.Content.IsMimeMultipartContent())
            {
                var data = await Request.Content.ParseMultipartAsync();

                HttpPostedFile file = data.Files["file"];
                if (file != null)
                    return Content(HttpStatusCode.OK, pm.ImportPayrollStartValueHeadAdd(new MemoryStream(file.File), BuildDateTimeFromString(dateFromString, true).Value, BuildDateTimeFromString(dateToString, true).Value, importedFrom, base.ActorCompanyId));
            }

            throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
        }

        [HttpPost]
        [Route("PayrollStartValueHead/Import/Update/{payrollStartValueHeadId:int}/{updateType:int}")]
        public async Task<IHttpActionResult> ImportPayrollStartValueHeadUpdate(int payrollStartValueHeadId, int updateType)
        {
            if (Request.Content.IsMimeMultipartContent())
            {
                var data = await Request.Content.ParseMultipartAsync();

                HttpPostedFile file = data.Files["file"];
                if (file != null)
                    return Content(HttpStatusCode.OK, pm.ImportPayrollStartValueHeadUpdate(new MemoryStream(file.File), payrollStartValueHeadId, (PayrollStartValueUpdateType)updateType, base.ActorCompanyId));
            }

            throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
        }

        [HttpDelete]
        [Route("PayrollStartValueHead/{payrollStartValueHeadId:int}")]
        public IHttpActionResult DeletePayrollStartValueHead(int payrollStartValueHeadId)
        {
            return Content(HttpStatusCode.OK, tem.DeletePayrollStartValueHead(payrollStartValueHeadId));
        }

        #endregion

        #region PayrollStartValueRow

        [HttpGet]
        [Route("PayrollStartValueRow/{payrollStartValueHeadId:int}/{employeeId:int}/{includeAppellation:bool}/{includePayrollProduct:bool}/{includeTransaction:bool}")]
        public IHttpActionResult GetPayrollStartValueRows(int payrollStartValueHeadId, int employeeId, bool includeAppellation, bool includePayrollProduct, bool includeTransaction)
        {
            return Content(HttpStatusCode.OK, pm.GetPayrollStartValueRows(base.ActorCompanyId, payrollStartValueHeadId, employeeId, includeAppellation, includePayrollProduct, includeTransaction).ToDTOs(includePayrollProduct: includePayrollProduct, includeTransaction: includeTransaction));
        }

        [HttpGet]
        [Route("PayrollStartValueRow/Transaction/{employeeId:int}/{payrollStartValueRowId:int}")]
        public IHttpActionResult GetPayrollStartValueRowTransactions(int employeeId, int payrollStartValueRowId)
        {
            return Content(HttpStatusCode.OK, ttm.GetTimePayrollTransactionsForStartValues(base.ActorCompanyId, employeeId, payrollStartValueRowId).ToCompactDTOs());
        }

        [HttpPost]
        [Route("PayrollStartValueRow/Update")]
        public IHttpActionResult SavePayrollStartValues(PayrollStartValueRowsModel model)
        {
            return Content(HttpStatusCode.OK, tem.SavePayrollStartValues(model.StartValueRows, model.PayrollStartValueHeadId));
        }

        [HttpPost]
        [Route("PayrollStartValueRow/Transaction/{payrollStartValueHeadId:int}/{employeeId:int?}")]
        public IHttpActionResult SaveTransactionsForPayrollStartValue(int payrollStartValueHeadId, int? employeeId)
        {
            if (employeeId.HasValue && employeeId.Value == 0)
                return Content(HttpStatusCode.OK, tem.SaveTransactionsForPayrollStartValue(null, payrollStartValueHeadId));
            else
                return Content(HttpStatusCode.OK, tem.SaveTransactionsForPayrollStartValue(employeeId, payrollStartValueHeadId));
        }

        [HttpDelete]
        [Route("PayrollStartValueRow/Transaction/{payrollStartValueHeadId:int}/{employeeId:int?}")]
        public IHttpActionResult DeleteTransactionsForPayrollStartValue(int payrollStartValueHeadId, int? employeeId)
        {
            if (employeeId.HasValue && employeeId.Value == 0)
                return Content(HttpStatusCode.OK, tem.DeleteTransactionsForPayrollStartValue(null, payrollStartValueHeadId));
            else
                return Content(HttpStatusCode.OK, tem.DeleteTransactionsForPayrollStartValue(employeeId, payrollStartValueHeadId));
        }

        #endregion

        #region RetroactivePayroll

        [HttpGet]
        [Route("Retroactive/")]
        public IHttpActionResult GetRetroactivePayrolls()
        {
            return Content(HttpStatusCode.OK, pm.GetRetroactivePayrolls(base.ActorCompanyId, loadTimePeriod: true, loadEmployees: true).ToDTOs());
        }

        [HttpGet]
        [Route("Retroactive/{retroactivePayrollId:int}")]
        public IHttpActionResult GetRetroactivePayroll(int retroactivePayrollId)
        {
            return Content(HttpStatusCode.OK, pm.GetRetroactivePayroll(retroactivePayrollId, base.ActorCompanyId, loadTimePeriod: true, loadEmployees: true).ToDTO());
        }

        [HttpGet]
        [Route("Retroactive/Employee/{timePeriodId:int}/{employeeId:int}")]
        public IHttpActionResult GetRetroactivePayrollsForEmployee(int timePeriodId, int employeeId)
        {
            return Content(HttpStatusCode.OK, pm.GetRetroactivePayrollsForEmployee(timePeriodId, employeeId, base.ActorCompanyId, loadTimePeriod: true, setNumberOfEmployees: true).ToDTOs());
        }

        [HttpPost]
        [Route("Retroactive/Save/")]
        public IHttpActionResult SaveRetroactivePayroll(RetroactivePayrollModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, tem.SaveRetroactivePayroll(model.RetroactivePayroll));
        }

        [HttpDelete]
        [Route("Retroactive/Delete/{retroactivePayrollId:int}")]
        public IHttpActionResult DeleteRetroactivePayroll(int retroactivePayrollId)
        {
            return Content(HttpStatusCode.OK, tem.DeleteRetroactivePayroll(retroactivePayrollId));
        }

        [HttpPost]
        [Route("Retroactive/Calculate/")]
        public IHttpActionResult CalculateRetroactivePayroll(RetroactivePayrollModel model)
        {
            return Content(HttpStatusCode.OK, tem.CalculateRetroactivePayroll(model.RetroactivePayroll, model.IncludeAlreadyCalculated, model.FilterEmployeeIds));
        }

        [HttpPost]
        [Route("Retroactive/DeleteOutcomes/")]
        public IHttpActionResult DeleteRetroactivePayrollOutcomes(RetroactivePayrollModel model)
        {
            return Content(HttpStatusCode.OK, tem.DeleteRetroactivePayrollOutcomes(model.RetroactivePayroll));
        }

        [HttpPost]
        [Route("Retroactive/CreateTransactions/")]
        public IHttpActionResult CreateRetroactivePayrollTransactions(RetroactivePayrollModel model)
        {
            return Content(HttpStatusCode.OK, tem.CreateRetroactivePayrollTransactions(model.RetroactivePayroll, model.FilterEmployeeIds));
        }

        [HttpPost]
        [Route("Retroactive/DeleteTransactions/")]
        public IHttpActionResult DeleteRetroactivePayrollTransactions(RetroactivePayrollModel model)
        {
            return Content(HttpStatusCode.OK, tem.DeleteRetroactivePayrollTransactions(model.RetroactivePayroll, model.FilterEmployeeIds));
        }

        #endregion

        #region RetroactivePayrollAccount

        [HttpGet]
        [Route("Retroactive/Account/{retroactivePayrollId:int}")]
        public IHttpActionResult GetRetroactiveAccounts(int retroactivePayrollId)
        {
            return Content(HttpStatusCode.OK, pm.GetRetroactiveAccounts(retroactivePayrollId, base.ActorCompanyId));
        }

        #endregion

        #region RetroactivePayrollEmployee

        [HttpPost]
        [Route("Retroactive/Employee/")]
        public IHttpActionResult GetRetroactiveEmployees(GetRetroactiveEmployeesModel model)
        {
            return Content(HttpStatusCode.OK, pm.GetRetroactiveEmployees(model.RetroactivePayrollId, model.TimePeriodId, base.ActorCompanyId, model.FilterEmployeeIds, model.IgnoreEmploymentStopDate));
        }

        [HttpPost]
        [Route("Retroactive/Employee/Filter")]
        public IHttpActionResult GetRetroactiveEmployees(FilterRetroactiveEmployeesModel model)
        {
            return Content(HttpStatusCode.OK, pm.FilterRetroactiveEmployees(base.ActorCompanyId, base.RoleId, base.UserId, model.Employees, model.AccountOrCategoryId));
        }

        [HttpGet]
        [Route("Retroactive/Review/Employees/{retroactivePayrollId:int}")]
        public IHttpActionResult GetRetroactivePayrollEmployees(int retroactivePayrollId)
        {
            return Content(HttpStatusCode.OK, pm.GetRetroactivePayrollEmployeesForReview(retroactivePayrollId, base.ActorCompanyId).ToList());
        }

        [HttpGet]
        [Route("Retroactive/Outcome/Employee/{retroactivePayrollId:int}/{employeeId:int}")]
        public IHttpActionResult GetRetroactivePayrollOutcomeForEmployee(int retroactivePayrollId, int employeeId)
        {
            return Content(HttpStatusCode.OK, pm.GetRetroactivePayrollOutcomeForEmployee(base.ActorCompanyId, retroactivePayrollId, employeeId).ToList());
        }

        [HttpPost]
        [Route("Retroactive/Outcome/Employee/Update/")]
        public IHttpActionResult SaveRetroactivePayrollOutcomeForEmployee(SaveRetroactivePayrollOutcomeModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, tem.SaveRetroactivePayrollOutcome(model.RetroactivePayrollId, model.EmployeeId, model.RetroactivePayrollOutcomeDTOs));
        }

        [HttpGet]
        [Route("Retroactive/Outcome/Transaction/{employeeId:int}/{retroactivePayrollOutcomeId:int}")]
        public IHttpActionResult GetRetroactivePayrollOutcomeTransactions(int employeeId, int retroactivePayrollOutcomeId)
        {
            return Content(HttpStatusCode.OK, pm.GetRetroactivePayrollOutcomeTransactions(base.ActorCompanyId, retroactivePayrollOutcomeId, employeeId));
        }

        [HttpGet]
        [Route("Retroactive/Basis/Outcome/{employeeId:int}/{retroactivePayrollOutcomeId:int}")]
        public IHttpActionResult GetRetroactivePayrollBasisForOutcome(int employeeId, int retroactivePayrollOutcomeId)
        {
            return Content(HttpStatusCode.OK, pm.GetRetroactivePayrollBasis(base.ActorCompanyId, retroactivePayrollOutcomeId, employeeId));
        }

        [HttpGet]
        [Route("Retroactive/Basis/Outcome/Transaction/{employeeId:int}/{retroactivePayrollOutcomeId:int}/{retroactiveTimePayrollTransactionId:int}/{retroactiveTimePayrollScheduleTransactionId:int}")]
        public IHttpActionResult GetRetroactivePayrollBasisForTransaction(int employeeId, int retroactivePayrollOutcomeId, int retroactiveTimePayrollTransactionId, int retroactiveTimePayrollScheduleTransactionId)
        {
            return Content(HttpStatusCode.OK, pm.GetRetroactivePayrollBasis(base.ActorCompanyId, retroactivePayrollOutcomeId, employeeId, retroactiveTimePayrollTransactionId, retroactiveTimePayrollScheduleTransactionId));
        }

        #endregion

        #region UnionFee

        [HttpGet]
        [Route("UnionFee")]
        public IHttpActionResult GetUnionFees()
        {
            return Content(HttpStatusCode.OK, pm.GetUnionFees(base.ActorCompanyId, loadPriceTypes: true, loadPayrollProducts: true).ToGridDTOs());
        }

        [HttpGet]
        [Route("UnionFeeDict/{addEmptyRow}")]
        public IHttpActionResult GetUnionFeesDict(bool addEmptyRow)
        {
            return Content(HttpStatusCode.OK, pm.GetUnionFeesDict(base.ActorCompanyId, addEmptyRow).ToSmallGenericTypes());
        }

        [HttpGet]
        [Route("UnionFee/{unionFeeId:int}")]
        public IHttpActionResult GetUnionFee(int unionFeeId)
        {
            return Content(HttpStatusCode.OK, pm.GetUnionFee(unionFeeId, base.ActorCompanyId).ToDTO());
        }

        [HttpPost]
        [Route("UnionFee/")]
        public IHttpActionResult SaveUnionFee(UnionFeeDTO unionFee)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, pm.SaveUnionFee(unionFee, base.ActorCompanyId));
        }

        [HttpDelete]
        [Route("UnionFee/{unionFeeId:int}")]
        public IHttpActionResult DeleteUnionFee(int unionFeeId)
        {
            return Content(HttpStatusCode.OK, pm.DeleteUnionFee(unionFeeId, base.ActorCompanyId));
        }

        #endregion

        #region Vacation

        #region EmployeeVacationPeriod

        [HttpGet]
        [Route("EmployeeVacationPeriod/{employeeId:int}/{startDate}/{stopDate}")]
        public IHttpActionResult GetEmployeeVacationPeriod(int employeeId, string startDate, string stopDate)
        {
            return Content(HttpStatusCode.OK, pm.GetEmployeeVacationPeriod(base.ActorCompanyId, employeeId, BuildDateTimeFromString(startDate, true).Value, BuildDateTimeFromString(stopDate, true).Value));
        }

        [HttpGet]
        [Route("EmployeeVacationPeriod/{employeeId:int}/{timePeriodId:int}")]
        public IHttpActionResult GetEmployeeVacationPeriod(int employeeId, int timePeriodId)
        {
            return Content(HttpStatusCode.OK, pm.GetEmployeeVacationPeriod(base.ActorCompanyId, employeeId, timePeriodId));
        }

        #endregion

        #region VacationGroup

        [HttpGet]
        [Route("VacationGroup/{addEmptyRow:bool}")]
        public IHttpActionResult GetVacationGroups(bool addEmptyRow)
        {
            return Content(HttpStatusCode.OK, pm.GetVacationGroupsDict(base.ActorCompanyId, addEmptyRow).ToSmallGenericTypes());
        }

        [HttpGet]
        [Route("VacationGroup/EndDate")]
        public IHttpActionResult GetVacationGroupEndDates([ModelBinder(typeof(CommaDelimitedArrayModelBinder))] int[] vacationGroupIds)
        {
            return Content(HttpStatusCode.OK, pm.GetVacationGroupEndDates(vacationGroupIds.ToList(), DateTime.Today));
        }

        #endregion

        #region VacationYearEnd

        [HttpGet]
        [Route("VacationYearEnd")]
        public IHttpActionResult GetVacationYearEnds()
        {
            return Content(HttpStatusCode.OK, pm.GetVacationYearEndsForGrid().ToDTOs());
        }

        [HttpGet]
        [Route("VacationYearEnd/Result/{vacationYearEndHeadId:int}")]
        public IHttpActionResult GetVacationYearEndResult(int vacationYearEndHeadId)
        {
            return Content(HttpStatusCode.OK, pm.GetVacationYearEndResult(vacationYearEndHeadId));
        }

        [HttpPost]
        [Route("VacationYearEnd")]
        public IHttpActionResult SaveVacationYearEnd(CreateVacationYearEndModel model)
        {
            return Content(HttpStatusCode.OK, tem.SaveVacationYearEnd(model.ContentType, model.ContentTypeIds, model.Date));
        }

        [HttpPost]
        [Route("VacationYearEnd/Validate")]
        public IHttpActionResult ValidateVacationYearEnd(ValidateVacationYearEndModel model)
        {
            return Content(HttpStatusCode.OK, tem.ValidateVacationYearEnd(model.Date, model.VacationGroupIds, model.EmployeeIds));
        }

        [HttpDelete]
        [Route("VacationYearEnd/{vacationYearEndHeadId:int}")]
        public IHttpActionResult DeleteVacationYearEnd(int vacationYearEndHeadId)
        {
            return Content(HttpStatusCode.OK, tem.DeleteVacationYearEnd(vacationYearEndHeadId));
        }

        #endregion

        [HttpGet]
        [Route("Vacation/GetEarningYearIsVacationYearVacationDays/{vacationGroupId:int}/{employeeId:int}/{dateString}/{dateFromString}/{dateToString}")]
        public IHttpActionResult GetEarningYearIsVacationYearVacationDays(int vacationGroupId, int employeeId, string dateString, string dateFromString, string dateToString)
        {
            return Content(HttpStatusCode.OK, pm.GetEarningYearIsVacationYearVacationDays(vacationGroupId, employeeId, base.ActorCompanyId, BuildDateTimeFromString(dateString, true).Value, BuildDateTimeFromString(dateFromString, true), BuildDateTimeFromString(dateToString, true)));
        }

        [HttpGet]
        [Route("Vacation/GetVacationDaysPaidByLaw/{employeeId:int}/{dateString}")]
        public IHttpActionResult GetVacationDaysPaidByLaw(int employeeId, string dateString)
        {
            return Content(HttpStatusCode.OK, (int)(pm.GetVacationDaysPaidByLaw(base.ActorCompanyId, employeeId, BuildDateTimeFromString(dateString, true).Value).Value));
        }

        #endregion
    }
}
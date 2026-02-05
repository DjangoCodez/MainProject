using CrystalDecisions.CrystalReports.Engine;
using Newtonsoft.Json;
using SoftOne.Soe.Business.Core.CrGen;
using SoftOne.Soe.Business.Core.Logger;
using SoftOne.Soe.Business.Core.RptGen;
using SoftOne.Soe.Business.DataCache;
using SoftOne.Soe.Business.DTO;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Business.Util.LogCollector;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.DTO.SoftOneLogger;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Util;
using SoftOne.Soe.Util.Exceptions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Linq;

namespace SoftOne.Soe.Business.Core.Reporting
{
    public abstract class BaseReportDataManager : ManagerBase
    {
        #region Constants

        public const string ROOT = "SOE";
        public const string ROOTPREFIX = "SOE_";

        private Company _company;
        protected Company Company
        {
            get
            {
                if (this.reportResult?.Input?.Company != null)
                {
                    return this.reportResult.Input.Company;
                }
                else
                {
                    if (_company == null && reportResult != null)
                        _company = CompanyManager.GetCompany(reportResult.ActorCompanyId);
                    return _company;
                }
            }
            set
            {
                this._company = value;
            }
        }

        protected CreateReportResult reportResult;

        public bool UseWebApiInternal
        {
            get
            {
#if DEBUG
                return false;
#else
                var value = BusinessMemoryCache<bool?>.Get("UseWebApiInternal");

                if (value.HasValue)
                    return value.Value;

                value = SettingManager.GetBoolSetting(SettingMainType.Application, (int)ApplicationSettingType.UseWebApiInternal, 0, 0, 0);
                BusinessMemoryCache<bool>.Set("UseWebApiInternal", value.Value, 60 * 15);

                return value.Value;
#endif
            }

        }

        #endregion

        #region Variables

        // Create a logger for use in this class
        protected readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        protected string additionalLogInfo = "";
        protected readonly int payrollSlipXMLVersion = 5;
        protected readonly bool useCrGen = true;
        protected int reportLanguageId;
        public List<RptGenRequestPicturesDTO> CrGenRequestPictures = new List<RptGenRequestPicturesDTO>();

        #endregion

        #region Ctor

        protected BaseReportDataManager(ParameterObject parameterObject) : base(parameterObject)
        {

        }

        #endregion

        #region CrGen

        protected string AddToCrGenRequestPicturesDTO(string path, byte[] filedata = null)
        {
            if (!string.IsNullOrEmpty(path))
            {
                try
                {
                    var crGenRequestPicturesDTO = new RptGenRequestPicturesDTO();
                    var dataProvided = filedata != null && filedata.Length > 0;
                    crGenRequestPicturesDTO.Path = FixPathForCrGen(path, !dataProvided);
                    if (dataProvided)
                        crGenRequestPicturesDTO.Data = filedata;
                    else
                        crGenRequestPicturesDTO.Data = File.ReadAllBytes(path);

                    if (!CrGenRequestPictures.Any(p => p.Path.Equals(path)))
                        CrGenRequestPictures.Add(crGenRequestPicturesDTO);

                    return crGenRequestPicturesDTO.Path;
                }
                catch (Exception ex)
                {
                    base.LogError(ex, log);
                }
            }

            return "";
        }

        protected static string FixPathForCrGen(string path, bool copyFile)
        {
            if (!string.IsNullOrEmpty(path))
            {
                string fileName = Path.GetFileName(path);
                string onlyPath = path.Replace(fileName, "");
                string newPath = path.Replace(onlyPath, Constants.SOE_CRGEN_PATH);
                newPath = newPath.Replace(fileName, Guid.NewGuid().ToString() + fileName);

                if (copyFile)
                {
                    if (!Directory.Exists(Constants.SOE_CRGEN_PATH))
                        Directory.CreateDirectory(Constants.SOE_CRGEN_PATH);
                    File.Copy(path, newPath);
                }
                return newPath;
            }

            return "";
        }

        #endregion

        #region Permissions

        protected (PermissionParameterObject Param, PermissionCacheRepository Repository) GetPermissionRepository()
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            return FeatureManager.GetPermissionRepository(this.Company?.LicenseId ?? base.LicenseId, this.Company?.ActorCompanyId ?? base.ActorCompanyId, base.RoleId, entitiesReadOnly);
        }

        #endregion

        #region Selections

        public bool TryGetBoolFromSelection(CreateReportResult reportResult, out bool value, string key = null)
        {
            var selection = reportResult?.Input?.GetSelection<BoolSelectionDTO>(key);
            if (selection != null)
                value = selection.Value;
            else if (reportResult?.EvaluatedSelection != null && reportResult.EvaluatedSelection.ST_ShowAllEmployees && !string.IsNullOrEmpty(key) && key == "showAllEmployees")
                value = true;
            else
                value = false;

            return selection != null;
        }

        public bool TryGetBoolFromSelection(List<ReportDataSelectionDTO> selections, out bool value, string key = null)
        {
            var selection = selections?.GetSelection<BoolSelectionDTO>(key);
            if (selection != null)
                value = selection.Value;
            else
                value = false;

            return selection != null;
        }

        public bool TryGetDateFromSelection(CreateReportResult reportResult, out DateTime date, string key = null)
        {
            var selection = reportResult?.Input?.GetSelection<DateSelectionDTO>(key);
            if (selection != null)
                date = selection.Date;
            else if (reportResult != null && reportResult.EvaluatedSelection != null)
            {
                if (reportResult.EvaluatedSelection.DateTo != CalendarUtility.DATETIME_DEFAULT && reportResult.EvaluatedSelection.DateTo != DateTime.MinValue)
                    date = reportResult.EvaluatedSelection.DateTo;
                else if (reportResult.EvaluatedSelection.DateFrom != CalendarUtility.DATETIME_DEFAULT && reportResult.EvaluatedSelection.DateFrom != DateTime.MinValue)
                    date = reportResult.EvaluatedSelection.DateFrom;
                else
                    date = CalendarUtility.DATETIME_DEFAULT;

                return true;
            }
            else
                date = CalendarUtility.DATETIME_DEFAULT;

            return selection != null;
        }

        public bool TryGetDatesFromSysTimeIntervalSelection(CreateReportResult reportResult, out DateTime from, out DateTime to)
        {
            from = DateTime.Today;
            to = DateTime.Today;
            DateTime? fromDate = null;
            DateTime? toDate = null;
            var result = TryGetDatesFromSysTimeIntervalSelection(reportResult, null, out fromDate, out toDate);

            if (result && fromDate.HasValue && toDate.HasValue)
            {
                from = fromDate.Value;
                to = toDate.Value;
                return true;
            }

            return false;
        }

        public bool TryGetDatesFromSysTimeIntervalSelection(CreateReportResult reportResult, int? sysTimeInterval, out DateTime? from, out DateTime? to)
        {
            from = null;
            to = null;
            DateRangeSelectionDTO selection = reportResult?.Input?.GetSelection<DateRangeSelectionDTO>();

            if (sysTimeInterval.HasValue || selection?.Id != null)
            {
                int id = (sysTimeInterval ?? selection.Id) ?? 0;
                SysTimeInterval timeRange = CalendarManager.GetSysTimeInterval(id, true);

                if (timeRange != null)
                {
                    DateRangeDTO interval = CalendarManager.GetSysTimeIntervalDateRange(id, DateTime.Today);
                    from = interval.Start;
                    to = interval.Stop;

                    if (selection != null && selection.UseMinMaxIfEmpty)
                    {
                        from = CalendarUtility.GetBeginningOfDay(from.Value.Date == DateTime.MinValue ? CalendarUtility.DATETIME_MINVALUE : from);
                        to = CalendarUtility.GetEndOfDay(to.Value.Date == DateTime.MinValue ? CalendarUtility.DATETIME_MAXVALUE : to);
                    }
                    else
                    {
                        from = CalendarUtility.GetBeginningOfDay(from.Value.Date == DateTime.MinValue ? DateTime.Today : from);
                        to = CalendarUtility.GetEndOfDay(to.Value.Date == DateTime.MinValue ? from : to);
                    }

                    if (selection != null)
                    {
                        selection.From = from.Value;
                        selection.To = to.Value;
                    }

                    return true;
                }

                return false;
            }

            return false;
        }

        public bool TryGetDatesFromSelection(
            CreateReportResult reportResult,
            List<int> selectedTimePeriodIds,
            out DateTime from,
            out DateTime to
            )
        {
            return TryGetDatesFromSelection(reportResult, selectedTimePeriodIds, out from, out to, out _, false);
        }

        public bool TryGetDatesFromSelection(
            CreateReportResult reportResult,
            List<int> selectedTimePeriodIds,
            out DateTime from,
            out DateTime to,
            out List<TimePeriod> selectedTimePeriods,
            bool alwaysLoadPeriods = false
            )
        {
            bool result = TryGetDatesFromSelection(reportResult, out from, out to);

            bool hasValidFrom = from != DateTime.MinValue && from != CalendarUtility.DATETIME_DEFAULT;
            bool hasValidTo = to != DateTime.MinValue && to != CalendarUtility.DATETIME_DEFAULT;

            // Load periods if flag is set or if dates are not valid
            if (alwaysLoadPeriods || !hasValidFrom || !hasValidTo)
            {
                using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
                selectedTimePeriods = entitiesReadOnly.TimePeriod
                    .Where(w => selectedTimePeriodIds.Contains(w.TimePeriodId) && w.State == (int)SoeEntityState.Active)
                    .OrderBy(o => o.StartDate)
                    .ToList();
            }
            else
            {
                selectedTimePeriods = null;
            }

            if (!hasValidFrom)
                from = selectedTimePeriods?.FirstOrDefault()?.StartDate ?? reportResult.EvaluatedSelection?.DateFrom ?? CalendarUtility.DATETIME_DEFAULT;

            if (!hasValidTo)
                to = selectedTimePeriods?.LastOrDefault()?.StopDate ?? reportResult.EvaluatedSelection?.DateTo ?? CalendarUtility.DATETIME_DEFAULT;

            return result;
        }

        public bool TryGetDatesFromSelection(CreateReportResult reportResult, out DateTime from, out DateTime to)
        {
            DateRangeSelectionDTO selection = reportResult?.Input?.GetSelection<DateRangeSelectionDTO>();
            if (selection != null)
            {
                if (selection?.Id != null && selection.Id != 0 && TryGetDatesFromSysTimeIntervalSelection(reportResult, out from, out to))
                    return true;

                from = selection.From;
                to = selection.To;

                if (selection.UseMinMaxIfEmpty)
                {
                    from = CalendarUtility.GetBeginningOfDay(from.Date == DateTime.MinValue ? CalendarUtility.DATETIME_MINVALUE : from);
                    to = CalendarUtility.GetEndOfDay(to.Date == DateTime.MinValue ? CalendarUtility.DATETIME_MAXVALUE : to);
                }
                else
                {
                    if (reportResult.SoeReportType == SoeReportType.CrystalReport &&
                        (reportResult.ReportTemplateType == SoeReportTemplateType.StockSaldoListReport ||
                        reportResult.ReportTemplateType == SoeReportTemplateType.StockTransactionListReport))
                    {
                        from = CalendarUtility.GetBeginningOfDay(from.Date == DateTime.MinValue ? CalendarUtility.DATETIME_MINVALUE : from);
                    }
                    else
                    {
                        from = CalendarUtility.GetBeginningOfDay(from.Date == DateTime.MinValue ? DateTime.Today : from);
                        to = CalendarUtility.GetEndOfDay(to.Date == DateTime.MinValue ? from : to);
                    }
                }

                return true;
            }
            else if (reportResult != null && reportResult.EvaluatedSelection != null)
            {
                from = reportResult.EvaluatedSelection.DateFrom;
                to = reportResult.EvaluatedSelection.DateTo;
                return true;
            }
            else
            {
                from = to = CalendarUtility.DATETIME_DEFAULT;
            }

            return selection != null;
        }

        public bool TryGetDatesFromSelection(CreateReportResult reportResult, out DateTime from, out DateTime to, string key)
        {
            var selection = reportResult?.Input?.GetSelection<DateRangeSelectionDTO>(key);

            if (selection != null)
            {
                from = selection.From;
                to = selection.To;

                if (selection.UseMinMaxIfEmpty)
                {
                    from = CalendarUtility.GetBeginningOfDay(from.Date == DateTime.MinValue ? CalendarUtility.DATETIME_MINVALUE : from);
                    to = CalendarUtility.GetEndOfDay(to.Date == DateTime.MinValue ? CalendarUtility.DATETIME_MAXVALUE : to);
                }
                else
                {
                    from = CalendarUtility.GetBeginningOfDay(from.Date == DateTime.MinValue ? DateTime.Today : from);
                    to = CalendarUtility.GetEndOfDay(to.Date == DateTime.MinValue ? from : to);
                }

                return true;
            }
            else if (reportResult != null && reportResult.EvaluatedSelection != null)
            {
                from = reportResult.EvaluatedSelection.DateFrom;
                to = reportResult.EvaluatedSelection.DateTo;
                return true;
            }
            else
            {
                from = to = CalendarUtility.DATETIME_DEFAULT;
            }

            return selection != null;
        }

        public bool TryGetDatesFromSelection(CreateReportResult reportResult, out DateTime? from, out DateTime? to, string key)
        {
            from = null;
            to = null;
            DateRangeSelectionDTO selection = reportResult?.Input?.GetSelection<DateRangeSelectionDTO>(key);

            if (selection != null)
            {
                from = selection.From;
                to = selection.To;

                if (selection.UseMinMaxIfEmpty)
                {
                    from = CalendarUtility.GetBeginningOfDay(from.Value.Date == DateTime.MinValue ? CalendarUtility.DATETIME_MINVALUE : from);
                    to = CalendarUtility.GetEndOfDay(to.Value.Date == DateTime.MinValue ? CalendarUtility.DATETIME_MAXVALUE : to);
                }
                else
                {
                    from = CalendarUtility.GetBeginningOfDay(from.Value.Date == DateTime.MinValue ? DateTime.Today : from);
                    to = CalendarUtility.GetEndOfDay(to.Value.Date == DateTime.MinValue ? from : to);
                }

                return true;
            }
            else if (reportResult != null && reportResult.EvaluatedSelection != null)
            {
                from = reportResult.EvaluatedSelection.DateFrom;
                to = reportResult.EvaluatedSelection.DateTo;
                return true;
            }
            else
            {
                from = to = CalendarUtility.DATETIME_DEFAULT;
            }

            return selection != null;
        }

        public bool TryGetDatesFromSelection(CreateReportResult reportResult, out List<DateTime> dates)
        {
            var selection = reportResult?.Input?.GetSelection<DatesSelectionDTO>();
            if (selection != null)
            {
                dates = new List<DateTime>();
                if (!selection.Dates.IsNullOrEmpty())
                    dates.AddRange(selection.Dates);
                return true;
            }
            else
                dates = new List<DateTime>();

            return !dates.IsNullOrEmpty();
        }

        public bool TryGetTextFromSelection(CreateReportResult reportResult, out string text, string key = null)
        {
            var selection = reportResult?.Input?.GetSelection<TextSelectionDTO>(key);
            if (selection != null)
                text = selection.Text;
            else
                text = null;

            return (selection != null);
        }

        public bool TryGetAccountFilters(CreateReportResult reportResult, out List<AccountFilterSelectionDTO> value, string key)
        {
            var selection = reportResult?.Input?.GetSelection<AccountFilterSelectionsDTO>(key);
            if (selection != null)
                value = selection.Filters;
            else
                value = new List<AccountFilterSelectionDTO>();
            return selection != null;
        }

        public bool TryGetIdFromSelection(CreateReportResult reportResult, out int? id, string key = null)
        {
            var selection = reportResult?.Input?.GetSelection<IdSelectionDTO>(key);
            if (selection != null)
                id = selection.Id.ToNullable();
            else
                id = null;

            return selection != null;
        }

        public bool TryGetIdsFromSelection(CreateReportResult reportResult, out List<int> ids, string key = null, bool nullIfEmpty = false)
        {
            var selection = reportResult?.Input?.GetSelection<IdListSelectionDTO>(key);
            if (selection != null)
            {
                ids = selection.Ids?.ToList() ?? new List<int>();
            }
            else
            {
                if (reportResult != null && reportResult.EvaluatedSelection != null && reportResult.EvaluatedSelection.ST_TimePeriodIds != null)
                {
                    ids = reportResult.EvaluatedSelection.ST_TimePeriodIds;
                    if (ids.Any())
                        return true;
                }
                else if (reportResult != null && reportResult.EvaluatedSelection != null && reportResult.EvaluatedSelection.SB_HasPurchaseIds)
                {
                    ids = reportResult.EvaluatedSelection.SB_PurchaseIds;
                    if (ids.Any())
                        return true;
                }
                else
                    ids = new List<int>();
            }

            if (nullIfEmpty && !ids.Any())
                ids = null;

            return selection != null;
        }

        public bool TryGetAttachmentsFromSelection(CreateReportResult reportResult, out List<KeyValuePair<string, byte[]>> attachments, string key = null, bool nullIfEmpty = false)
        {
            var selection = reportResult?.Input?.GetSelection<AttachmentsListSelectionDTO>(key);
            if (selection != null)
                attachments = selection.Attachments?.ToList() ?? new List<KeyValuePair<string, byte[]>>();
            else
                attachments = new List<KeyValuePair<string, byte[]>>();

            if (nullIfEmpty && !attachments.Any())
                attachments = null;

            return selection != null;
        }

        public bool TryGetPayrollPriceTypeSelection(CreateReportResult reportResult, out List<int> ids, string key = null, bool nullIfEmpty = false)
        {
            ids = new List<int>();
            var selection = reportResult?.Input?.GetSelection<PayrollPriceTypeSelectionDTO>(key);
            if (selection != null)
            {
                ids = selection.Ids?.ToList() ?? new List<int>();

                if (!ids.Any() && reportResult != null)
                {
                    List<PayrollPriceType> priceTypes = PayrollManager.GetPayrollPriceTypes(reportResult.ActorCompanyId, null, false);
                    if (selection.TypeIds.Any())
                        ids = priceTypes.Where(w => selection.TypeIds.Contains(w.Type)).Select(x => x.PayrollPriceTypeId).ToList();
                    else
                        ids = priceTypes.Select(x => x.PayrollPriceTypeId).ToList();
                }

            }

            if (nullIfEmpty && !ids.Any())
                ids = null;

            return selection != null;
        }

        public bool TryGetUserIdsFromSelection(CreateReportResult reportResult, out List<int> ids, out bool includeInactive)
        {
            var selection = reportResult?.Input?.GetSelection<UserDataSelectionDTO>("users");
            if (selection != null)
            {
                ids = selection.Ids?.ToList() ?? new List<int>();
                includeInactive = selection.IncludeInactive;
            }
            else
            {
                ids = new List<int>();
                includeInactive = false;
            }

            return selection != null;
        }

        public bool TryGetEmployeeIdsFromSelection(CreateReportResult reportResult, List<int> timePeriodIds, out List<Employee> employees, out List<int> employeeIds)
        {
            return TryGetEmployeeIdsFromSelection(reportResult, DateTime.Today.Date, DateTime.Today.Date, timePeriodIds, out employees, out employeeIds, out _, out _);
        }

        public bool TryGetEmployeeIdsFromSelection(CreateReportResult reportResult, List<int> timePeriodIds, out List<Employee> employees, out List<int> employeeIds, out List<int> accountIds, out TermGroup_EmployeeSelectionAccountingType selectionAccountingType)
        {
            return TryGetEmployeeIdsFromSelection(reportResult, DateTime.Today.Date, DateTime.Today.Date, timePeriodIds, out employees, out employeeIds, out accountIds, out selectionAccountingType);
        }

        public bool TryGetEmployeeIdsFromSelection(CreateReportResult reportResult, DateTime dateFrom, DateTime dateTo, out List<Employee> employees, out List<int> employeeIds)
        {
            return TryGetEmployeeIdsFromSelection(reportResult, dateFrom, dateTo, null, out employees, out employeeIds, out _, out _);
        }

        public bool TryGetEmployeeIdsFromSelection(CreateReportResult reportResult, DateTime dateFrom, DateTime dateTo, out List<Employee> employees, out List<int> employeeIds, out List<int> accountIds, out TermGroup_EmployeeSelectionAccountingType selectionAccountingType, int? timeScheduleScenarioHeadId = null)
        {
            return TryGetEmployeeIdsFromSelection(reportResult, dateFrom, dateTo, null, out employees, out employeeIds, out accountIds, out selectionAccountingType, timeScheduleScenarioHeadId: timeScheduleScenarioHeadId);
        }

        public bool TryGetEmployeeIdsFromSelection(CreateReportResult reportResult, DateTime dateFrom, DateTime dateTo, List<int> timePeriods, out List<Employee> employees, out List<int> employeeIds, out List<int> accountIds, out TermGroup_EmployeeSelectionAccountingType selectionAccountingType, int? timeScheduleScenarioHeadId = null)
        {
            employees = null;
            selectionAccountingType = TermGroup_EmployeeSelectionAccountingType.EmployeeCategory;
            var selection = reportResult?.Input?.GetSelection<EmployeeSelectionDTO>("employees");

            if (selection != null)
            {
                selectionAccountingType = selection.AccountingType;
                employeeIds = selection.EmployeeIds?.ToList() ?? new List<int>();
                employees = EmployeeManager.GetEmployeesByFilter(reportResult.ActorCompanyId, reportResult.UserId, reportResult.RoleId, dateFrom, dateTo, timePeriods, reportResult.ReportTemplateType, selection.AccountingType, selection.AccountIds?.ToList(), selection.CategoryIds, selection.EmployeeGroupIds, selection.PayrollGroupIds, selection.VacationGroupIds, selection.IncludeInactive, selection.OnlyInactive, selection.IncludeEnded, selection.IncludeHidden, selection.IncludeVacant, selection.DoValidateEmployment, selection.IncludeSecondary, timeScheduleScenarioHeadId);

                if (employeeIds.Any())
                {
                    var ids = employeeIds.ToList();
                    employeeIds = employees.Where(i => ids.Contains(i.EmployeeId)).Select(s => s.EmployeeId).ToList();
                    employees = employees.Where(w => ids.Contains(w.EmployeeId)).ToList();
                }
                else
                    employeeIds = employees.Select(s => s.EmployeeId).ToList();

                if (!selection.IncludeVacant)
                {
                    var employeeIdsInSelection = employeeIds;
                    using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
                    var vacantEmployeeIdss = entitiesReadOnly.Employee.Where(w => employeeIdsInSelection.Contains(w.EmployeeId) && w.Vacant).Select(s => s.EmployeeId).Distinct().ToList();
                    employeeIds = employeeIds.Where(w => !vacantEmployeeIdss.Contains(w)).ToList();
                }
                if (!selection.IncludeHidden)
                {
                    using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
                    var hiddenId = base.GetHiddenEmployeeIdFromCache(entities, CacheConfig.Company(reportResult.ActorCompanyId));
                    employeeIds = employeeIds.Where(w => w != hiddenId).ToList();
                }
            }
            else
            {
                if (reportResult != null && reportResult.EvaluatedSelection != null && reportResult.EvaluatedSelection.ST_EmployeeIds != null)
                {
                    employeeIds = reportResult.EvaluatedSelection.ST_EmployeeIds;
                }
                else
                    employeeIds = new List<int>();
            }

            var result = ValidateEmployeeIds(reportResult, employeeIds);
            employeeIds = result.employeeIds;

            accountIds = selection?.AccountIds != null ? selection.AccountIds.ToList() : new List<int>();
            return result.isValid;
        }

        public bool SelectionHasSpecifiedEmployeeIds(CreateReportResult reportResult)
        {
            var selection = reportResult?.Input?.GetSelection<EmployeeSelectionDTO>("employees");
            return selection != null && selection.EmployeeIds != null && selection.EmployeeIds.Any();
        }

        public bool TryAccountingDatesFromSelection(CompEntities entities, CreateReportResult reportResult, out DateTime selectionDateFrom, out DateTime selectionDateTo)
        {
            AccountIntervalSelectionDTO rangeFrom = null;
            AccountIntervalSelectionDTO rangeTo = null;

            var selectionTimePeriodIds = new List<int>();

            var ok1 = TryGetAccountIntervalSelectionRange(reportResult, out rangeFrom, "accountPeriodFrom");
            var ok2 = TryGetAccountIntervalSelectionRange(reportResult, out rangeTo, "accountPeriodTo");

            int periodFromId = rangeFrom != null ? rangeFrom.Value.Value : 0;
            int periodToId = rangeTo != null ? rangeTo.Value.Value : 0;

            var accountYear = AccountManager.GetAccountYear(entities, rangeFrom.YearId.Value, true);
            var accountYearTo = AccountManager.GetAccountYear(entities, rangeTo.YearId.Value, true);

            selectionDateFrom = accountYear?.From ?? CalendarUtility.DATETIME_DEFAULT;
            selectionDateTo = accountYearTo?.To ?? CalendarUtility.DATETIME_MAXVALUE;

            if (periodFromId != 0)
            {
                selectionTimePeriodIds.Add(periodFromId);
            }
            if (periodToId != 0)
            {
                selectionTimePeriodIds.Add(periodToId);
            }

            if (selectionTimePeriodIds.Any())
            {
                var selectedTimePeriods = entities.AccountPeriod.Where(w => selectionTimePeriodIds.Contains(w.AccountPeriodId)).OrderBy(o => o.From).ToList();
                selectionDateFrom = selectedTimePeriods.FirstOrDefault(x => x.AccountPeriodId == periodFromId)?.From ?? selectionDateFrom;
                selectionDateTo = selectedTimePeriods.FirstOrDefault(x => x.AccountPeriodId == periodToId)?.To ?? selectionDateTo;
            }

            return (ok1 && ok2);
        }

        public bool TryGetAccountDim(CreateReportResult reportResult,
            out List<AccountDimSelectionDTO> accountDim,
            out List<int?> accountIds, string key = null)
        {
            accountDim = new List<AccountDimSelectionDTO>();
            accountIds = new List<int?>();
            var selection = reportResult?.Input?.GetSelection<AccountDimSelectionDTO>(key);

            if (selection != null)
            {
                foreach (var item in selection.AccountIds)
                {
                    accountIds.Add(item);
                }
            }
            return selection != null;
        }

        public bool TryGetAccountIntervalSelectionRange(CreateReportResult reportResult, out AccountIntervalSelectionDTO value, string key)
        {
            var selection = reportResult?.Input?.GetSelection<AccountIntervalSelectionDTO>(key);
            if (selection != null)
                value = selection;
            else
                value = null;
            return selection != null;
        }

        private (bool isValid, List<int> employeeIds) ValidateEmployeeIds(CreateReportResult reportResult, List<int> employeeIds)
        {
            if (reportResult.ReportTemplateType.IsPayrollReport())
            {
                if (FeatureManager.HasRolePermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_Employments_Payroll_Salary, Permission.Readonly, base.RoleId, base.ActorCompanyId, base.LicenseId))
                    return (true, employeeIds);

                var employee = EmployeeManager.GetEmployeeByUser(base.ActorCompanyId, base.UserId, false, false);
                if (employee != null)
                    employeeIds = employeeIds.Where(w => w == employee.EmployeeId).ToList();
                else
                    employeeIds = new List<int>();

                if (employeeIds.IsNullOrEmpty())
                {
                    reportResult.SetErrorMessage(SoeReportDataResultMessage.ReportsNotAuthorized);
                    return (false, new List<int>());
                }
            }
            return (true, employeeIds);
        }

        public List<int> GetAccountIdsFromSelection(CreateReportResult reportResult)
        {
            var selection = reportResult?.Input?.GetSelection<EmployeeSelectionDTO>("employees");
            if (selection != null)
                return selection.AccountIds?.ToList() ?? new List<int>();
            else
                return new List<int>();
        }

        public bool TryGetEmployeePostIdsFromSelection(CreateReportResult reportResult, DateTime dateFrom, DateTime dateTo, out List<int> employeePostIds)
        {
            var selection = reportResult?.Input?.GetSelection<EmployeeSelectionDTO>();
            if (selection != null)
                employeePostIds = selection.EmployeePostIds.ToList() ?? new List<int>();
            else
                employeePostIds = new List<int>();

            return selection != null;
        }

        public bool TryGetIncludeInactiveFromSelection(CreateReportResult reportResult, out bool includeInactive, out bool onlyInactive, out bool? activeEmployees)
        {
            var selection = reportResult?.Input?.GetSelection<EmployeeSelectionDTO>();
            if (selection != null)
            {
                includeInactive = selection.IncludeInactive;
                onlyInactive = selection.OnlyInactive;
            }
            else
            {
                includeInactive = false;
                onlyInactive = false;
            }

            if (onlyInactive)
                activeEmployees = false;
            else if (includeInactive)
                activeEmployees = null;
            else
                activeEmployees = true;

            return selection != null;
        }

        public bool TryGetPayrollProductIdsFromSelections(CreateReportResult reportResult, out List<int> payrollProductIds)
        {
            payrollProductIds = new List<int>();
            var selections = reportResult?.Input?.GetSelections()?.OfType<PayrollProductRowSelectionDTO>().ToList();

            if (selections != null && selections.Any())
            {
                foreach (var selection in selections)
                {
                    if (selection != null && !selection.PayrollProductIds.IsNullOrEmpty())
                        payrollProductIds.AddRange(selection.PayrollProductIds?.ToList());

                    else if (selection != null && selection.SysPayrollTypeLevel1.GetValueOrDefault() != 0)
                        payrollProductIds.AddRange(ProductManager.GetPayrollProductIdsByType(reportResult.ActorCompanyId, selection.SysPayrollTypeLevel1, selection.SysPayrollTypeLevel2, selection.SysPayrollTypeLevel3, selection.SysPayrollTypeLevel4));
                }
            }

            payrollProductIds = payrollProductIds.Distinct().ToList();

            return payrollProductIds.Any();
        }

        public MatrixColumnsSelectionDTO TryGetMatrixColumnsSelectionDTOFromSelection(CreateReportResult reportResult)
        {
            return reportResult?.Input?.GetSelection<MatrixColumnsSelectionDTO>();
        }

        public bool TryGetDetailedInformationFromSelection(CreateReportResult reportResult, out bool getDetailedInformation)
        {
            getDetailedInformation = reportResult.GetDetailedInformation;

            return true;
        }

        public bool TryGetSpecialFromSelection(CreateReportResult reportResult, out string value)
        {
            value = reportResult.ReportSpecial;

            if (!string.IsNullOrEmpty(value))
                return true;
            else
                return false;
        }

        public void GetEnumFromSelection<T>(CreateReportResult reportResult, out T value, T defaultValue, string key = null) where T : struct, IConvertible
        {
            Type type = typeof(T);
            var selection = reportResult?.Input?.GetSelection<IdSelectionDTO>(key);
            if (selection != null && Enum.IsDefined(type, selection.Id))
                value = (T)(object)selection.Id;
            else
                value = defaultValue;
        }

        public List<Tuple<decimal, TimeCodeTransaction>> GetTimeCodeTransactionsFromSelection(CompEntities entities, CreateReportResult reportResult, List<TimeAccumulatorTimeCode> timeAccumulatorTimeCodes, DateTime dateTo, int employeeId)
        {
            List<Tuple<decimal, TimeCodeTransaction>> transactions = new List<System.Tuple<decimal, TimeCodeTransaction>>();
            foreach (TimeAccumulatorTimeCode timeAccTimeCode in timeAccumulatorTimeCodes)
            {
                List<TimeCodeTransaction> items = TimeAccumulatorManager.GetTimeCodeTransactionsForTimeAccumulatorReport(entities, dateTo, employeeId, timeAccTimeCode.TimeCodeId);
                foreach (var item in items)
                {
                    transactions.Add(Tuple.Create(timeAccTimeCode.Factor, item));
                }

            }
            return transactions;
        }

        public List<Tuple<decimal, TimeInvoiceTransaction>> GetTimeInvoiceTransactionsFromSelection(CompEntities entities, CreateReportResult reportResult, List<TimeAccumulatorInvoiceProduct> timeAccumulatorInvoiceProducts, DateTime dateTo, int employeeId)
        {
            List<Tuple<decimal, TimeInvoiceTransaction>> transactions = new List<System.Tuple<decimal, TimeInvoiceTransaction>>();
            foreach (TimeAccumulatorInvoiceProduct timeAccInvoiceProduct in timeAccumulatorInvoiceProducts)
            {
                List<TimeInvoiceTransaction> items = TimeAccumulatorManager.GetTimeInvoiceTransactionsForTimeAccumulatorReport(entities, dateTo, employeeId, timeAccInvoiceProduct.InvoiceProductId);
                foreach (var item in items)
                {
                    transactions.Add(Tuple.Create(timeAccInvoiceProduct.Factor, item));
                }
            }
            return transactions;
        }

        public List<Tuple<decimal, TimePayrollTransaction>> GetTimePayrollTransactionsFromSelection(CompEntities entities, CreateReportResult reportResult, List<TimeAccumulatorPayrollProduct> timeAccumulatorPayrollProducts, DateTime dateTo, int employeeId)
        {
            List<Tuple<decimal, TimePayrollTransaction>> transactions = new List<System.Tuple<decimal, TimePayrollTransaction>>();
            foreach (TimeAccumulatorPayrollProduct timeAccPayrollProduct in timeAccumulatorPayrollProducts)
            {
                List<TimePayrollTransaction> items = TimeAccumulatorManager.GetTimePayrollTransactionsForTimeAccumulatorReport(entities, dateTo, employeeId, timeAccPayrollProduct.PayrollProductId);
                foreach (var item in items)
                {
                    transactions.Add(Tuple.Create(timeAccPayrollProduct.Factor, item));
                }

            }
            return transactions;
        }

        #endregion

        #region Export

        protected bool IsExportTypeRequiringReportDocument(TermGroup_ReportExportType exportType)
        {
            return exportType != TermGroup_ReportExportType.Unknown && exportType != TermGroup_ReportExportType.Xml;
        }

        protected void ExportData(ReportPrintoutDTO dto, CreateReportResult reportResult)
        {
            if (dto == null || reportResult == null || !reportResult.Success || !reportResult.HasValidSelection || reportResult.ExportType == TermGroup_ReportExportType.NoExport)
                return;

            if (dto.ExportFormat == SoeExportFormat.Xml)
                ExportDataAsXml(dto, reportResult);
            else if (reportResult.ExportType != TermGroup_ReportExportType.File)
                ExportDataAsReport(dto, reportResult);

            if (dto.Data == null)
                reportResult.ResultMessage = SoeReportDataResultMessage.ExportFailed;
        }

        protected void SaveReportPrintoutElsewhere(ReportPrintoutDTO dto, CreateReportResult reportResult)
        {
            if (dto == null || reportResult == null || !reportResult.Success || !reportResult.HasValidSelection)
                return;

            try
            {
                if ((dto.SysReportTemplateTypeId == (int)SoeReportTemplateType.TimeEmploymentContract || dto.SysReportTemplateTypeId == (int)SoeReportTemplateType.TimeEmploymentDynamicContract) && dto.ExportFormat == SoeExportFormat.Pdf)
                {
                    bool savePrintout;
                    TryGetBoolFromSelection(reportResult, out savePrintout, "savePrintout");

                    if (savePrintout)
                    {
                        DateTime selectionDate;
                        List<DateTime> substituteDates;

                        bool isPrintedFromSchedulePlanning;
                        TryGetBoolFromSelection(reportResult, out isPrintedFromSchedulePlanning, "isPrintedFromSchedulePlanning");
                        if (isPrintedFromSchedulePlanning)
                        {
                            if (!TryGetDatesFromSelection(reportResult, out substituteDates))
                                return;

                            selectionDate = substituteDates.OrderByDescending(i => i.Date).FirstOrDefault();
                        }
                        else
                        {
                            if (!TryGetDateFromSelection(reportResult, out selectionDate))
                                return;

                            substituteDates = new List<DateTime>() { selectionDate };
                        }

                        List<Employee> employees;
                        List<int> selectionEmployeeIds;
                        if (!TryGetEmployeeIdsFromSelection(reportResult, selectionDate, selectionDate, out employees, out selectionEmployeeIds))
                            return;

                        if (selectionEmployeeIds.Count == 1)
                        {
                            int? employmentId = 0;
                            if (TryGetIdFromSelection(reportResult, out employmentId, key: "employmentId") || isPrintedFromSchedulePlanning)
                            {
                                using (CompEntities entities = new CompEntities())
                                {
                                    int employeeId = selectionEmployeeIds.First();
                                    var employment = entities.Employment.FirstOrDefault(f => f.EmploymentId == employmentId && f.EmployeeId == employeeId);

                                    if (employment == null && isPrintedFromSchedulePlanning)
                                        employment = entities.Employment.FirstOrDefault(f => f.EmployeeId == employeeId && f.Employee.ActorCompanyId == ActorCompanyId);

                                    if (employment != null)
                                        EmployeeManager.SaveEmploymentContractOnPrint(entities, dto.ReportId ?? 0, employeeId, dto.Data, dto.XML, dto.ReportName);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogError(ex, log);
            }
        }

        protected void ExportDataAsXml(ReportPrintoutDTO dto, CreateReportResult reportResult)
        {
            if (dto == null || reportResult == null || !reportResult.Success)
                return;

            if (reportResult.XmlDocument != null)
            {
                //Never write schema for XmlDocument
                dto.Data = ExportXmlDocumentWithoutSchema(reportResult);
            }
            else if (reportResult.Document != null)
            {
                if (reportResult.IgnoreSchema)
                    dto.Data = ExportXDocumentWithoutSchema(reportResult);
                else
                    dto.Data = ExportXDocumentWithSchema(reportResult);
            }
            else if (reportResult.Documents != null)
            {
                List<XDocument> documents = reportResult.Documents;
                XDocument document = reportResult.Documents.First();
                documents.Remove(document);

                foreach (var doc in documents)
                    document.Root.Add(doc.Root.Elements());

                reportResult.Document = document;
                if (reportResult.IgnoreSchema)
                    dto.Data = ExportXDocumentWithoutSchema(reportResult);
                else
                    dto.Data = ExportXDocumentWithSchema(reportResult);
            }
        }

        protected void ExportDataAsReport(ReportPrintoutDTO dto, CreateReportResult reportResult)
        {
            if (dto == null || reportResult == null || !reportResult.Success || !reportResult.HasValidSelection)
                return;

            List<CreateReportResultOutput> outputs = new List<CreateReportResultOutput>();
            List<byte[]> datas = new List<byte[]>();
            bool hasMultipleDocuments = false;

            if (!reportResult.Outputs.IsNullOrEmpty())
            {
                hasMultipleDocuments = true;
                outputs = reportResult.Outputs;
            }
            else
                outputs.Add(new CreateReportResultOutput() { Document = reportResult.Document });

            while (outputs.Any())
            {
                CreateReportResultOutput output = outputs.First();
                try
                {
                    RptGenResultDTO crGenResult = null;
                    RptGenConnector crgen = RptGenConnector.GetConnector(parameterObject, reportResult.SoeReportType);
                    if (output.Pdf == null || reportResult.ExportType != TermGroup_ReportExportType.Pdf)
                        crGenResult = crgen.GenerateReport(dto.ExportType, reportResult.Template, output.Document, reportResult.DataSet, this.CrGenRequestPictures, GetCulture(GetLangId()), ReportGenManager.GetXsdFileString(reportResult.ReportTemplateType), $"rr reportid:{reportResult.ReportId} actorcompanyid: {reportResult.ActorCompanyId}");
                    else
                        crGenResult = new RptGenResultDTO() { GeneratedReport = output.Pdf };

                    if (crGenResult != null)
                    {
                        var data = crGenResult.GeneratedReport;

                        if (data != null)
                        {
                            if (!hasMultipleDocuments)
                            {
                                dto.Data = data;
                            }
                            else
                            {
                                if (dto.Datas == null)
                                    dto.Datas = new List<byte[]>() { data };
                                else
                                    dto.Datas.Add(data);
                            }
                        }
                    }
                }
                finally
                {
                    outputs.Remove(output);
                }
            }


            if (!dto.Datas.IsNullOrEmpty() && (dto.ExportFormat == SoeExportFormat.Pdf || dto.ExportFormat == SoeExportFormat.MergedPDF))
                MergePDFs(reportResult, dto);
        }

        private byte[] ExportXmlDocumentWithoutSchema(CreateReportResult reportResult)
        {
            return Encoding.UTF8.GetBytes(reportResult.XmlDocument.OuterXml);
        }

        private byte[] ExportXDocumentWithoutSchema(CreateReportResult reportResult)
        {
            return Encoding.UTF8.GetBytes(reportResult.Document.ToString());
        }

        private byte[] ExportXDocumentWithSchema(CreateReportResult reportResult)
        {
            DataSet ds = ReportGenManager.CreateDataSet(reportResult.Document, reportResult.ReportTemplateType);
            if (ds == null)
                return null;

            byte[] result = null;
            MemoryStream memoryStream = null;
            string path = string.Empty;

            try
            {
                memoryStream = new MemoryStream();
                ds.WriteXml(memoryStream, XmlWriteMode.WriteSchema);
                result = memoryStream.ToArray();
            }
            catch (Exception ex)
            {
                reportResult.SetErrorMessage(SoeReportDataResultMessage.ExportFailed);
                SysServiceManager.LogError("ExportXDocumentWithSchema ds.ReadXml(DataSet.WriteXml MS failed - trying creating byte[] " + ex.Message.ToString());
            }
            finally
            {
                if (memoryStream != null)
                {
                    memoryStream.Flush();
                    memoryStream.Close();
                    memoryStream.Dispose();
                }
            }

            if (result == null)
            {
                try
                {
                    path = ConfigSettings.SOE_SERVER_DIR_TEMP_PHYSICAL + Guid.NewGuid().ToString() + "_File";
                    ds.WriteXml(path, XmlWriteMode.WriteSchema);
                    result = File.ReadAllBytes(path);
                }
                catch (Exception ex)
                {
                    reportResult.SetErrorMessage(SoeReportDataResultMessage.ExportFailed);
                    SysServiceManager.LogError("ExportXDocumentWithSchema ds.ReadXml(DataSet.WriteXml to disk failed - trying creating file " + ex.Message.ToString());
                }

                try
                {
                    File.Delete(path);
                }
                catch (Exception ex)
                {
                    LogError(ex, this.log);
                }
            }

            if (result == null)
            {

                FileStream fileStream = null;

                try
                {
                    path = ConfigSettings.SOE_SERVER_DIR_TEMP_PHYSICAL + Guid.NewGuid().ToString() + "_FS";
                    fileStream = new FileStream(path, FileMode.Create);
                    ds.WriteXml(fileStream, XmlWriteMode.WriteSchema);
                    result = new byte[fileStream.Length];
                    fileStream.Read(result, 0, result.Length);
                }
                catch (Exception ex)
                {
                    reportResult.SetErrorMessage(SoeReportDataResultMessage.ExportFailed);
                    SysServiceManager.LogError("ExportXDocumentWithSchema ds.ReadXml(DataSet.WriteXml fileStream failed - trying creating byte[] " + ex.Message.ToString());
                }
                finally
                {
                    if (fileStream != null)
                    {
                        fileStream.Flush();
                        fileStream.Close();
                        fileStream.Dispose();
                    }

                    try
                    {
                        File.Delete(path);
                    }
                    catch (Exception ex)
                    {
                        LogError(ex, this.log);
                    }
                }
            }

            if (result == null)
            {

                FileStream fileStream = null;

                try
                {
                    path = ConfigSettings.SOE_SERVER_DIR_TEMP_PHYSICAL + Guid.NewGuid().ToString() + "_FS_source";
                    for (int x = 0; x < 20; x++)
                    {
                        fileStream = new FileStream(path, FileMode.Create);
                        ds.WriteXml(fileStream, XmlWriteMode.WriteSchema);

                        using (FileStream fsSource = new FileStream(path,
                                            FileMode.Open, FileAccess.Read))
                        {
                            // Read the source file into a byte array.
                            long fLength = fsSource.Length / 20 + 1;
                            byte[] bytes = new byte[fLength];
                            int numBytesToRead = (int)fLength;
                            int numBytesRead = 0;
                            long readPosition = 0;
                            while (numBytesToRead > 0)
                            {
                                fsSource.Position = readPosition;
                                readPosition += fLength;
                                int n = fsSource.Read(bytes, numBytesRead, numBytesToRead);

                                // Break when the end of the file is reached.
                                if (n == 0) break;

                                numBytesRead += n;
                                numBytesToRead -= n;
                            }

                            result = bytes;
                        }
                    }
                }

                catch (Exception ex)
                {
                    reportResult.SetErrorMessage(SoeReportDataResultMessage.ExportFailed);
                    SysServiceManager.LogError("ExportXDocumentWithSchema ds.ReadXml(DataSet.WriteXml fileStream bit by bit failed - trying creating byte[] " + ex.Message.ToString());
                }
                finally
                {
                    if (fileStream != null)
                    {
                        fileStream.Flush();
                        fileStream.Close();
                        fileStream.Dispose();
                    }

                    try
                    {
                        File.Delete(path);
                    }
                    catch (Exception ex)
                    {
                        LogError(ex, this.log);
                    }
                }
            }

            return result;

        }

        protected void MergeReportPackage(ReportPrintoutDTO dto, DirectoryInfo directory)
        {
            if (dto == null || directory == null)
                return;

            string destinationFileName = StringUtility.GetValidFilePath(directory.FullName) + dto.ReportFileName;
            if (PDFUtility.MergeFiles(directory, destinationFileName))
            {
                FileStream fileStream = null;

                try
                {
                    fileStream = File.OpenRead(destinationFileName);
                    dto.Data = PDFUtility.GetDataFromStream(fileStream);
                }
                finally
                {
                    if (fileStream != null)
                    {
                        fileStream.Flush();
                        fileStream.Close();
                        fileStream.Dispose();
                    }
                }
            }
        }

        protected void MergePDFs(CreateReportResult reportResult, ReportPrintoutDTO dto)
        {
            if (!dto.Datas.IsNullOrEmpty())
            {
                if (dto.Datas.Count == 1)
                {
                    dto.Data = dto.Datas.First();
                    return;
                }

                DirectoryInfo directory = Directory.CreateDirectory(ConfigSettings.SOE_SERVER_DIR_TEMP_REPORT_PHYSICAL + Guid.NewGuid().ToString());
                int reportCounter = 0;
                foreach (var data in dto.Datas)
                {
                    reportCounter++;
                    string fileName = StringUtility.GetValidFilePath(directory.FullName) + reportCounter.ToString().PadLeft(4, '0') + dto.ReportFileName + "." + dto.ExportFormat;
                    if (data != null)
                        File.WriteAllBytes(fileName, data);
                }

                string destinationFileName = StringUtility.GetValidFilePath(directory.FullName) + dto.ReportFileName + "." + dto.ExportFormat;

                if (PDFUtility.MergeFiles(directory, destinationFileName))
                    dto.Data = File.ReadAllBytes(destinationFileName);

                try
                {
                    foreach (var file in directory.GetFiles())
                        File.Delete(file.FullName);
                }
                catch
                {
                    try
                    {
                        Thread.Sleep(1000);
                        foreach (var file in directory.GetFiles())
                            File.Delete(file.FullName);
                    }
                    catch (Exception ex)
                    {

                        LogError(ex, this.log);
                    }
                }
            }
        }

        protected ActionResult SendReportByEmail(CreateReportResult reportResult, ReportPrintoutDTO dto)
        {
            if (dto == null || reportResult == null || !dto.IsEmailValid)
                return new ActionResult((int)ActionResultSave.EntityIsNull);

            var fileName = dto.EmailFileName + Constants.SOE_SERVER_FILE_PDF_SUFFIX;

            if (reportResult.EmailTemplate == null)
            {
                var emailTemplate = EmailManager.GetEmailTemplate(dto.EmailTemplateId.Value, dto.ActorCompanyId);
                reportResult.EmailTemplate = emailTemplate != null ? emailTemplate.ToDTO() : null;

            }

            if (reportResult.EmailTemplate == null)
                return new ActionResult(4146, GetText(4146, "E-postmall hittades inte"));

            var emailAttachments = reportResult.EmailAttachments != null ? reportResult.EmailAttachments : new List<KeyValuePair<string, byte[]>>();
            emailAttachments.Add(new KeyValuePair<string, byte[]>(fileName, dto.Data));
            if (reportResult.MergePdfs && emailAttachments.Count > 1)
                emailAttachments = PDFUtility.MergeDictionary(fileName, emailAttachments);

            Dictionary<string, string> customMailArg = null;
            if (reportResult.InvoiceDistributionId != 0)
            {
                customMailArg = new Dictionary<string, string>() { { "SOEInvoiceDistribution", $"{ConfigurationSetupUtil.GetCurrentSysCompDbId()}#{reportResult.InvoiceDistributionId}" } };
            }

            return EmailManager.SendEmailWithAttachment(dto.ActorCompanyId, reportResult.EmailTemplate, dto.EmailRecipients, emailAttachments, new List<string> { reportResult.SingleRecipient }, customMailArg);
        }

        #endregion

        #region PersonalData

        protected PersonalDataEmployeeReportRepository personalDataRepository { get; private set; }
        protected void InitPersonalDataEmployeeReportRepository(CompEntities entities = null)
        {
            this.personalDataRepository = new PersonalDataEmployeeReportRepository(parameterObject, reportResult, entities);
        }
        protected void InitPersonalDataEmployeeReportRepository(EvaluatedSelection es)
        {
            this.personalDataRepository = new PersonalDataEmployeeReportRepository(parameterObject, es);
        }

        #endregion

        #region CreateReportResult

        public ReportJobDefinitionDTO CreateReportJobDefinitionDTO(int reportId, SoeReportTemplateType sysReportTemplateTypeId, TermGroup_ReportExportType exportType, List<int> timePeriodIds, List<int> employeeIds)
        {
            ReportJobDefinitionDTO dto = new ReportJobDefinitionDTO(reportId, sysReportTemplateTypeId, exportType);

            if (!timePeriodIds.IsNullOrEmpty())
                dto.Selections.Add(new IdListSelectionDTO(timePeriodIds, "periods", "periods"));
            if (!employeeIds.IsNullOrEmpty())
                dto.Selections.Add(new EmployeeSelectionDTO(employeeIds, "employees"));

            return dto;
        }

        public CreateReportResult InitReportResult(ReportJobDefinitionDTO job, int actorCompanyId, int userId, int roleId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return InitReportResult(entities, job, actorCompanyId, userId, roleId);
        }

        public CreateReportResult InitReportResult(CompEntities entities, ReportJobDefinitionDTO job, int actorCompanyId, int userId, int roleId)
        {
            CreateReportResult result = new CreateReportResult();
            if (job == null)
                result.SetErrorMessage(SoeReportDataResultMessage.EmptyInput);
            result.Input = InitReportResultInput(entities, job, actorCompanyId, userId, roleId);
            return result;
        }

        protected CreateReportResult InitReportResult(ReportPrintoutDTO dto)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return InitReportResult(entities, dto);
        }

        protected CreateReportResult InitReportResult(CompEntities entities, ReportPrintoutDTO dto)
        {
            CreateReportResult createReportResult = new CreateReportResult();
            if (dto == null)
                createReportResult.SetErrorMessage(SoeReportDataResultMessage.EmptyInput);
            createReportResult.Input = InitReportResultInput(entities, dto);
            return createReportResult;
        }

        protected CreateReportResult InitReportResult(EvaluatedSelection es)
        {
            CreateReportResult createReportResult = new CreateReportResult();

            createReportResult.EvaluatedSelection = es;
            if (createReportResult.EvaluatedSelection == null)
            {
                createReportResult.SetErrorMessage(SoeReportDataResultMessage.EmptyInput);
                return createReportResult;
            }

            GetReportTemplateInfo(es.IsReportStandard, es.ReportTemplateId, es.ActorCompanyId, out SoeReportTemplateType reportTemplateType, out byte[] templateData, out SoeReportType soeReportType, out bool _);
            es.ReportTemplateType = reportTemplateType;
            es.ReportType = soeReportType;
            es.Template = templateData;
            if (es.Template == null)
            {
                createReportResult.SetErrorMessage(SoeReportDataResultMessage.ReportTemplateDataNotFound);
                return createReportResult;
            }

            //Set Force Default Values from SysReportTemplateSettings
            SetSyReportTemplateForceValues(createReportResult);

            //Special case
            if (es.ReportTemplateType == SoeReportTemplateType.HousholdTaxDeduction && es.SB_HTDShowFile)
            {
                es.ReportTemplateType = SoeReportTemplateType.HouseholdTaxDeductionFile;
                es.ExportType = TermGroup_ReportExportType.Xml;
                es.IgnoreSchema = true;
            }

            return createReportResult;
        }

        private void SetSyReportTemplateForceValues(CreateReportResult createReportResult)
        {
            if (createReportResult == null)
                return;
            if (createReportResult.EvaluatedSelection.IsReportStandard)
            {
                #region Local function
                bool getBooleanValue(string value)
                {
                    return value.ToLower() == "true";
                }
                #endregion

                SysReportTemplate sysReportTemplate = ReportManager.GetSysReportTemplate(createReportResult.EvaluatedSelection.ReportTemplateId, loadSysReportType: true, useCache: true);
                if (sysReportTemplate != null)
                {
                    var settings = sysReportTemplate.SysReportTemplateSettings.Where(x => x.State == (int)SoeEntityState.Active).ToList();
                    var fieldIdsWithForce = settings.Where(s => s.SettingType == (int)SoeReportSettingFieldMetaData.ForceDefaultValue).Select(s => s.SettingField).ToList();
                    foreach (var setting in settings.Where(s => fieldIdsWithForce.Contains(s.SettingField) && s.SettingType == (int)SoeReportSettingFieldMetaData.DefaultValue))
                    {
                        switch ((SoeReportSettingField)setting.SettingField)
                        {
                            case SoeReportSettingField.IncludeAllHistoricalData:
                                createReportResult.EvaluatedSelection.IncludeAllHistoricalData = getBooleanValue(setting.SettingValue);
                                break;
                            case SoeReportSettingField.IncludeBudget:
                                createReportResult.EvaluatedSelection.IncludeAllHistoricalData = getBooleanValue(setting.SettingValue);
                                break;
                            case SoeReportSettingField.ShowRowsByAccount:
                                createReportResult.EvaluatedSelection.ShowRowsByAccount = getBooleanValue(setting.SettingValue);
                                break;
                            case SoeReportSettingField.Nrofdecimals:
                                if (int.TryParse(setting.SettingValue, out int nrofdecimals))
                                    createReportResult.EvaluatedSelection.NrOfDecimals = nrofdecimals;
                                break;
                            case SoeReportSettingField.NoOfYearsBackinPreviousYear:
                                if (int.TryParse(setting.SettingValue, out int noOfYearsBackinPreviousYear))
                                    createReportResult.EvaluatedSelection.NoOfYearsBackinPreviousYear = noOfYearsBackinPreviousYear;
                                break;
                            case SoeReportSettingField.IncludeDetailedInformation:
                                createReportResult.EvaluatedSelection.GetDetailedInformation = getBooleanValue(setting.SettingValue);
                                break;
                            case SoeReportSettingField.ShowInAccountingReports:
                                createReportResult.EvaluatedSelection.ShowInAccountingReports = getBooleanValue(setting.SettingValue);
                                break;
                        }
                    }
                }
            }
        }

        protected CreateReportResultInput InitReportResultInput(CompEntities entities, ReportJobDefinitionDTO job, int actorCompanyId, int userId, int roleId)
        {
            CreateReportResultInput input = new CreateReportResultInput(job);
            SetReportResultInputValues(entities, input, actorCompanyId, userId, roleId, job?.ReportId, job?.SysReportTemplateTypeId, job?.ExportType);
            return input;
        }

        protected CreateReportResultInput InitReportResultInput(CompEntities entities, ReportPrintoutDTO dto)
        {
            CreateReportResultInput input = new CreateReportResultInput(dto);
            SetReportResultInputValues(entities, input, dto?.ActorCompanyId, dto?.UserId, dto?.RoleId, dto?.ReportId, dto?.SysReportTemplateType, dto?.ExportType);
            return input;
        }

        private void SetReportResultInputValues(CompEntities entities, CreateReportResultInput input, int? actorCompanyId, int? userId, int? roleId, int? reportId, SoeReportTemplateType? templateType, TermGroup_ReportExportType? exportType)
        {
            if (input == null)
                return;

            //Identification
            input.Company = actorCompanyId.HasValue ? CompanyManager.GetCompany(entities, actorCompanyId.Value) : null;
            input.User = userId.HasValue ? UserManager.GetUser(entities, userId.Value) : null;
            input.Role = roleId.HasValue && actorCompanyId.HasValue ? RoleManager.GetRole(entities, roleId.Value, actorCompanyId.Value) : null;
            if (input.Role == null && input.Company != null && input.User != null)
            {
                LogInfo($"SetReportResultInputValues no role found on DTO userid {input.UserId} machine {Environment.MachineName} ");
                roleId = UserManager.GetUserCompanyRolesByUserAndCompany(entities, input.User.UserId, input.Company.ActorCompanyId)?.FirstOrDefault()?.RoleId;
                if (roleId.HasValue)
                {
                    input.Role = RoleManager.GetRole(entities, roleId.Value, input.Company.ActorCompanyId);
                    LogInfo($"SetReportResultInputValues2 role found on User userid {input.UserId} machine {Environment.MachineName} ");
                }
            }

            //Report
            if (reportId > 0 && input.Company != null)
                input.Report = ReportManager.GetReport(entities, reportId.Value, input.Company.ActorCompanyId, loadSysReportTemplateSettings: true);

            //Set Force default values from sysTemplate settings
            SetSyReportTemplateForceValues(input);

            //ReportTemplate
            GetReportTemplateInfo(input.Report, out SoeReportTemplateType reportTemplateType, out byte[] reportTemplateData, out SoeReportType soeReportType, out bool showOnlyTotals);
            input.ReportTemplateType = reportTemplateType;
            input.ReportTemplateData = reportTemplateData;
            input.SoeReportType = soeReportType;
            input.ShowOnlyTotals = showOnlyTotals;

            //Selection
            input.ExportType = exportType ?? TermGroup_ReportExportType.Unknown;
            if (input.ExportType == TermGroup_ReportExportType.Unknown && input.Report != null)
                input.ExportType = (TermGroup_ReportExportType)input.Report.ExportType;
            if (input.ExportType == TermGroup_ReportExportType.Unknown && input.SoeReportType == SoeReportType.CrystalReport)
                input.ExportType = TermGroup_ReportExportType.Pdf;
            input.SetSelectionJson();
            input.ValidateSelection(templateType);

            //Document
            if (input.SoeReportType == SoeReportType.CrystalReport)
            {
                input.DefaultDocument = templateType.HasValue ? ReportGenManager.GetDefaultXDocument(templateType.Value) : null;
                input.RootElementDefault = XmlUtil.GetRootElement(input.DefaultDocument);
                input.RootName = input.RootElementDefault.Name.ToString().Substring(ROOTPREFIX.Length);
                input.Document = XmlUtil.CreateDocument();
                input.ElementRoot = new XElement($"{ROOT}_{input.RootName}");
                input.ElementFirst = new XElement(input.RootName);
                input.DefaultElementGroups = input.DefaultDocument.Descendants().FirstOrDefault(i => i.Name.LocalName == input.RootName)?.Descendants().Where(i => i.Elements().Any()).ToList();
                if (input.DefaultElementGroups == null)
                    log.Error($"input.DefaultElementGroups is null for SysReportTemplateTypeId {templateType} and RootName {input.RootName}");
                input.ValidateDocument();
            }

            //Lang
            var langSelection = input.GetSelection<IdSelectionDTO>(ReportJobDefinitionDTO.LANG);
            if (langSelection?.Id.ToNullable() != null)
                SetLanguage(langSelection.Id);
        }

        private void SetSyReportTemplateForceValues(CreateReportResultInput input)
        {
            if (input == null || input.Report == null || input.Report.ReportTemplateSettings == null || input.Report.ReportTemplateSettings.Count() == 0)
                return;

            #region Local function
            bool getBooleanValue(string value)
            {
                return value.ToLower() == "true";
            }
            #endregion

            var settings = input.Report.ReportTemplateSettings.Where(x => x.State == (int)SoeEntityState.Active).ToList();
            var fieldIdsWithForce = settings.Where(s => s.SettingType == (int)SoeReportSettingFieldMetaData.ForceDefaultValue).Select(s => s.SettingField).ToList();
            foreach (var setting in settings.Where(s => fieldIdsWithForce.Contains(s.SettingField) && s.SettingType == (int)SoeReportSettingFieldMetaData.DefaultValue))
            {
                switch ((SoeReportSettingField)setting.SettingField)
                {
                    case SoeReportSettingField.IncludeAllHistoricalData:
                        input.Report.IncludeAllHistoricalData = getBooleanValue(setting.SettingValue);
                        break;
                    case SoeReportSettingField.IncludeBudget:
                        input.Report.IncludeAllHistoricalData = getBooleanValue(setting.SettingValue);
                        break;
                    case SoeReportSettingField.ShowRowsByAccount:
                        input.Report.ShowRowsByAccount = getBooleanValue(setting.SettingValue);
                        break;
                    case SoeReportSettingField.Nrofdecimals:
                        if (int.TryParse(setting.SettingValue, out int nrofdecimals))
                            input.Report.NrOfDecimals = nrofdecimals;
                        break;
                    case SoeReportSettingField.NoOfYearsBackinPreviousYear:
                        if (int.TryParse(setting.SettingValue, out int noOfYearsBackinPreviousYear))
                            input.Report.NoOfYearsBackinPreviousYear = noOfYearsBackinPreviousYear;
                        break;
                    case SoeReportSettingField.IncludeDetailedInformation:
                        input.Report.GetDetailedInformation = getBooleanValue(setting.SettingValue);
                        break;
                    case SoeReportSettingField.ShowInAccountingReports:
                        input.Report.ShowInAccountingReports = getBooleanValue(setting.SettingValue);
                        break;
                }
            }
        }

        private void GetReportTemplateInfo(Report report, out SoeReportTemplateType reportTemplateType, out byte[] templateData, out SoeReportType soeReportType, out bool showOnlyTotals)
        {
            reportTemplateType = SoeReportTemplateType.Unknown;
            templateData = null;
            soeReportType = SoeReportType.CrystalReport;
            showOnlyTotals = false;
            if (report == null)
                return;

            GetReportTemplateInfo(report.Standard, report.ReportTemplateId, report.ActorCompanyId, out reportTemplateType, out templateData, out soeReportType, out showOnlyTotals);
        }

        private void GetReportTemplateInfo(bool isReportStandard, int reportTemplateId, int actorCompanyId, out SoeReportTemplateType reportTemplateType, out byte[] templateData, out SoeReportType soeReportType, out bool showOnlyTotals)
        {
            reportTemplateType = SoeReportTemplateType.Unknown;
            templateData = null;
            soeReportType = SoeReportType.CrystalReport;
            showOnlyTotals = false;

            if (isReportStandard)
            {
                SysReportTemplate sysReportTemplate = ReportManager.GetSysReportTemplate(reportTemplateId, loadSysReportType: true, useCache: true);
                if (sysReportTemplate != null)
                {
                    reportTemplateType = (SoeReportTemplateType)sysReportTemplate.SysReportTemplateTypeId;
                    templateData = sysReportTemplate.Template;
                    soeReportType = (SoeReportType)sysReportTemplate.SysReportType.SysReportTypeId;
                    showOnlyTotals = sysReportTemplate.ShowOnlyTotals;
                }
            }
            else
            {
                ReportTemplate reportTemplate = ReportManager.GetReportTemplate(reportTemplateId, actorCompanyId);
                if (reportTemplate != null)
                {
                    reportTemplateType = (SoeReportTemplateType)reportTemplate.SysTemplateTypeId;
                    templateData = reportTemplate.Template;
                    soeReportType = (SoeReportType)reportTemplate.SysReportTypeId;
                    showOnlyTotals = reportTemplate.ShowOnlyTotals;
                }
            }
        }


        #endregion

        #region ReportDocument

        protected bool DoCreateReportDocument(CreateReportResult reportResult)
        {
            if (reportResult == null || !reportResult.Success)
                return false;
            if (reportResult.XmlDocument != null)
                return false;
            if (reportResult.ExportType == TermGroup_ReportExportType.File)
                return false;
            return true;
        }

        protected XDocument GetValidatedDocument(XDocument document, CreateReportResult reportResult)
        {
            string errorMessage;

            if (!ReportGenManager.ValidateDocument(document, reportResult.ReportTemplateType, out errorMessage))
            {
                reportResult.ResultMessageDetails = errorMessage;
                return null;
            }

            return document;
        }

        protected XDocument GetValidatedDocument(XDocument document, SoeReportTemplateType reportTemplateType)
        {
            string errorMessage;
            if (!ReportGenManager.ValidateDocument(document, reportTemplateType, out errorMessage))
                return null;

            return document;
        }

        #endregion

        #region ReportPrintout

        protected ReportPrintoutDTO GetReportPrintout(int actorCompanyId, int userId, int roleId, int reportPrintoutId)
        {
            using (CompEntities entities = new CompEntities())
            {
                var reportPrintout = (from rp in entities.ReportPrintoutSmallView
                                      where rp.ReportPrintoutId == reportPrintoutId &&
                                      rp.ActorCompanyId == actorCompanyId &&
                                      rp.UserId == userId &&
                                      rp.RoleId == roleId
                                      select rp).FirstOrDefault();

                return reportPrintout.ToDTO();
            }
        }

        protected ReportPrintoutDTO GetNextQueuedReportPrintoutForUser(int actorCompanyId, int userId, int excludeReportPrintoutId = 0)
        {
            Thread.Sleep(new Random().Next(1, 400));

            using (CompEntities entities = new CompEntities())
            {
                var reportPrintout = (from rp in entities.ReportPrintoutSmallView
                                      where rp.ActorCompanyId == actorCompanyId &&
                                      rp.Status == (int)TermGroup_ReportPrintoutStatus.Queued &&
                                      rp.UserId == userId &&
                                      rp.ReportPrintoutId != excludeReportPrintoutId
                                      orderby rp.ReportPrintoutId
                                      select rp).FirstOrDefault();

                return reportPrintout.ToDTO();
            }
        }

        protected ReportPrintoutDTO InitReportPrintout(List<EvaluatedSelection> evaluatedSelections, EvaluatedSelection mainEs)
        {
            if (evaluatedSelections == null || evaluatedSelections.Count <= 1 || mainEs == null)
                return null;

            //Handle merged reports as Report not ReportPackage
            if (mainEs.ReportPackageId == 0)
            {
                CreateReportResult createReportResult = InitReportResult(mainEs);
                if (!createReportResult.Success)
                    return null;
                return InitReportPrintout(createReportResult);
            }

            return new ReportPrintoutDTO()
            {
                ReportPackageId = mainEs.ReportPackageId,
                ReportName = mainEs.ReportName,
                Selection = mainEs.Selection,
                Created = DateTime.Now,
                CreatedBy = GetUserDetails(parameterObject.SoeUser),
                Status = (int)TermGroup_ReportPrintoutStatus.Ordered,

                //Email
                EmailTemplateId = mainEs.EmailTemplateId,
                EmailFileName = mainEs.EmailFileName,
                EmailRecipients = mainEs.EmailRecipients,

                //Set FK
                ActorCompanyId = mainEs.ActorCompanyId,
                UserId = parameterObject?.UserId,
                RoleId = parameterObject?.RoleId,
            };
        }

        protected ReportPrintoutDTO InitReportPrintout(ReportUserSelection selection)
        {
            if (selection == null)
                return null;

            if (selection.Report == null && !selection.ReportReference.IsLoaded)
                selection.ReportReference.Load();

            return new ReportPrintoutDTO()
            {
                ExportType = (TermGroup_ReportExportType)selection.Report.ExportType,
                ReportId = selection.ReportId,
                ReportName = selection.Report.Name,
                Selection = selection.Selection,
                Created = DateTime.Now,
                CreatedBy = GetUserDetails(parameterObject.SoeUser),
                Status = (int)TermGroup_ReportPrintoutStatus.Ordered,
                SysReportTemplateTypeId = (int)ReportManager.GetSoeReportTemplateType(selection.Report, selection.Report.ActorCompanyId),

                //Set FK
                ActorCompanyId = selection.ActorCompanyId,
                UserId = selection.UserId,
                RoleId = selection.UserId.HasValue ? UserManager.GetDefaultRoleId(selection.ActorCompanyId, selection.UserId.Value) : (int?)null,
            };
        }

        protected ReportPrintoutDTO InitReportPrintout(CreateReportResult reportResult)
        {
            if (reportResult == null || !reportResult.HasValidSelection)
                return null;

            ReportPrintoutDTO dto = CreateReportPrintout(reportResult);
            if (dto != null)
            {
                SetTermGroup_ReportExportFileType(reportResult);
                SetReportExportSettings(dto, reportResult);
                SetReportName(dto, reportResult);

                ActionResult result = ValidateTemplateTypeSpecifics(reportResult);
                if (!result.Success)
                {
                    if (result.ErrorNumber == (int)ActionResultSave.ReportPrintoutWarning)
                        dto.ResultMessage = (int)TermGroup_ReportPrintoutStatus.Warning;
                    else if (result.ErrorNumber == (int)ActionResultSave.ReportPrintoutError)
                        dto.ResultMessage = (int)TermGroup_ReportPrintoutStatus.Error;

                    dto.ResultMessageDetails = result.ErrorMessage;
                }
            }

            return dto;
        }

        protected ReportPrintoutDTO InitReportPrintoutForEdi(CreateReportResult reportResult)
        {
            if (reportResult == null || !reportResult.HasValidSelection)
                return null;

            ReportPrintoutDTO dto = CreateReportPrintout(reportResult);
            if (dto != null)
            {
                SetEdiExportSettings(dto, reportResult);
                SetReportName(dto, reportResult);

                if (dto.UserId == 0)
                {
                    dto.UserId = null;
                }
                if (dto.RoleId == 0)
                {
                    dto.RoleId = null;
                }
            }

            return dto;
        }

        protected ReportPrintoutDTO CreateReportPrintout(CreateReportResult reportResult)
        {
            if (reportResult == null)
                return null;

            SetTermGroup_ReportExportFileType(reportResult);

            return new ReportPrintoutDTO()
            {
                //Common
                ReportId = reportResult.ReportId,
                ReportPackageId = reportResult.ReportPackageId,
                ReportName = reportResult.ReportName,
                ReportTemplateId = reportResult.ReportTemplateId,
                SysReportTemplateTypeId = (int)reportResult.ReportTemplateType,
                Selection = reportResult.Selection,
                Created = DateTime.Now,
                CreatedBy = GetUserDetails(parameterObject.SoeUser),
                Status = (int)TermGroup_ReportPrintoutStatus.Queued,

                //Email
                EmailTemplateId = reportResult.EmailTemplateId,
                EmailFileName = reportResult.EmailFileName,
                EmailRecipients = reportResult.EmailRecipients,
                SingleRecipient = reportResult.SingleRecipient,

                //Set FK
                ActorCompanyId = reportResult.ActorCompanyId,
                ReportUrlId = reportResult.ReportUrlId,
                UserId = parameterObject?.UserId,
                RoleId = parameterObject?.RoleId,
            };
        }

        /// <summary>
        /// User should not be able to print the same report with the same selection twice.
        /// Preventing mistakes if the user does not realise he has printed the report.
        /// </summary>
        /// <param name="actorCompanyId"></param>
        /// <param name="userId"></param>
        /// <param name="selectionDTOs"></param>
        /// <param name="exportType"></param>
        /// <returns></returns>
        protected ActionResult ValidateDuplicatePrintJobs(int actorCompanyId, int userId, ICollection<ReportDataSelectionDTO> selectionDTOs, TermGroup_ReportExportType exportType)
        {
            ActionResult result = new ActionResult(true);

            if (selectionDTOs.IsNullOrEmpty())
                return result;

            foreach (var selection in selectionDTOs)
            {
                if (selection != null)
                    selection.Beautify();
            }

            DateTime limit = DateTime.Now.AddHours(-1);
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            entitiesReadOnly.ReportPrintout.NoTracking();

            var runningSelections = (from r in entitiesReadOnly.ReportPrintout
                                     where r.ActorCompanyId == actorCompanyId &&
                                     r.UserId == userId &&
                                    (r.Status == (int)TermGroup_ReportPrintoutStatus.Ordered || r.Status == (int)TermGroup_ReportPrintoutStatus.Queued) &&
                                     r.ExportType == (int)exportType &&
                                     r.Created > limit
                                     select r.Selection).ToList();

            string selectionJson = JsonConvert.SerializeObject(selectionDTOs, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore }).ToString();

            foreach (var runningSelection in runningSelections)
            {
                if (runningSelection.Equals(selectionJson))
                    result = new ActionResult(GetText(3677, "Du har redan en pågående utskrift av denna rapport med samma urval.\nVänta tills utskriften blir klar."));
            }

            return result;
        }

        protected ActionResult ValidateSimultaneousPrintJobs(int actorCompanyId, int userId, ReportJobDefinitionDTO reportJobDefinitionDTO, int maxAllowedSimultaneousPrintJobs)
        {
            return ValidateSimultaneousPrintJobs(actorCompanyId, userId, reportJobDefinitionDTO.ExportType, maxAllowedSimultaneousPrintJobs);
        }

        /// <summary>
        /// User should not be able to have many simultaneous jobs running (until we get a proper queue handler).
        /// </summary>
        /// <param name="actorCompanyId"></param>
        /// <param name="userId"></param>
        /// <param name="exportType"></param>
        /// <param name="maxAllowedSimultaneousPrintJobs"></param>
        /// <returns></returns>
        protected ActionResult ValidateSimultaneousPrintJobs(int actorCompanyId, int userId, TermGroup_ReportExportType exportType, int maxAllowedSimultaneousPrintJobs)
        {
            ActionResult result = new ActionResult(true);
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            DateTime limit = DateTime.Now.AddHours(-1);

            entitiesReadOnly.ReportPrintout.NoTracking();
            var runningJobs = (from r in entitiesReadOnly.ReportPrintout
                               where r.ActorCompanyId == actorCompanyId &&
                               r.UserId == userId &&
                               r.Status == (int)TermGroup_ReportPrintoutStatus.Ordered &&
                               r.ExportType == (int)exportType &&
                               r.Created > limit
                               select r).Count();

            if (runningJobs >= maxAllowedSimultaneousPrintJobs)
            {
                result.Success = false;
                if (runningJobs == 1)
                    result = new ActionResult(GetText(3678, "Du har redan en pågående utskrift.\nVänta tills utskriften blir klar innan du gör en ny."));
                else
                    result = new ActionResult(String.Format(GetText(3679, "Du har redan {0} pågående utskrifter.\nVänta tills en av dem blir klar innan du gör en ny."), runningJobs));
            }

            return result;
        }

        protected ActionResult ValidateTemplateTypeSpecifics(CreateReportResult reportResult)
        {
            if (reportResult == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "CreateReportResult");

            ActionResult result = new ActionResult(true);

            if (reportResult.Input != null && reportResult.Input.ForceValidation)
                return result;

            List<int> selectionTimePeriodIds;

            switch (reportResult.ReportTemplateType)
            {
                case SoeReportTemplateType.PayrollSlip:
                    #region PayrollSlip

                    if (!TryGetIdsFromSelection(reportResult, out selectionTimePeriodIds, "periods"))
                        return new ActionResult((int)ActionResultSave.ReportPrintoutError, GetText(12149, "Du måste välja minst en period"));

                    #endregion
                    break;
                case SoeReportTemplateType.PayrollTransactionStatisticsReport:
                    #region PayrollTransactionStatisticsReport

                    if (!TryGetIdsFromSelection(reportResult, out selectionTimePeriodIds, "periods"))
                        return new ActionResult((int)ActionResultSave.ReportPrintoutError, GetText(12149, "Du måste välja minst en period"));

                    #endregion
                    break;
                case SoeReportTemplateType.PayrollAccountingReport:
                    #region PayrollAccountingReport

                    if (!TryGetIdsFromSelection(reportResult, out selectionTimePeriodIds, "periods"))
                        return new ActionResult((int)ActionResultSave.ReportPrintoutError, GetText(12149, "Du måste välja minst en period"));

                    bool createVoucher;
                    if (TryGetBoolFromSelection(reportResult, out createVoucher, "createVoucher") && createVoucher)
                    {
                        int? voucherSeriesTypeId;
                        if (TryGetIdFromSelection(reportResult, out voucherSeriesTypeId, "voucherSeriesType") && voucherSeriesTypeId.HasValue)
                        {
                            DateTime voucherDate;
                            if (TryGetDateFromSelection(reportResult, out voucherDate, "voucherDate") && VoucherManager.VoucherExists(voucherDate, voucherSeriesTypeId.Value, TermGroup_VoucherHeadSourceType.Payroll, reportResult.Input.ActorCompanyId))
                                return new ActionResult((int)ActionResultSave.ReportPrintoutWarning, GetText(11875, "Det finns redan löneverifikat skapad på datumet. Vill du fortsätta?"));
                        }
                    }

                    #endregion
                    break;
                case SoeReportTemplateType.KU10Report:
                    #region KU10Report

                    if (!TryGetIdsFromSelection(reportResult, out selectionTimePeriodIds, "periods"))
                        return new ActionResult((int)ActionResultSave.ReportPrintoutError, GetText(12149, "Du måste välja minst en period"));

                    #endregion
                    break;
                case SoeReportTemplateType.SKDReport:
                    #region SKDReport

                    if (!TryGetIdsFromSelection(reportResult, out selectionTimePeriodIds, "periods"))
                        return new ActionResult((int)ActionResultSave.ReportPrintoutError, GetText(12149, "Du måste välja minst en period"));

                    #endregion
                    break;
                case SoeReportTemplateType.CollectumReport:
                    #region CollectumReport

                    if (!TryGetIdsFromSelection(reportResult, out selectionTimePeriodIds, "periods"))
                        return new ActionResult((int)ActionResultSave.ReportPrintoutError, GetText(12149, "Du måste välja minst en period"));

                    #endregion
                    break;
                case SoeReportTemplateType.EmployeeTimePeriodReport:
                    #region EmployeeTimePeriodReport

                    if (!TryGetIdsFromSelection(reportResult, out selectionTimePeriodIds, "periods"))
                        return new ActionResult((int)ActionResultSave.ReportPrintoutError, GetText(12149, "Du måste välja minst en period"));

                    #endregion
                    break;
                case SoeReportTemplateType.PayrollPeriodWarningCheck:
                    #region PayrollPeriodWarningCheck

                    if (!TryGetIdsFromSelection(reportResult, out selectionTimePeriodIds, "periods"))
                        return new ActionResult((int)ActionResultSave.ReportPrintoutError, GetText(12149, "Du måste välja minst en period"));

                    #endregion
                    break;
                case SoeReportTemplateType.SCB_SLPReport:
                case SoeReportTemplateType.SCB_KLPReport:
                case SoeReportTemplateType.SNReport:
                    #region SCB_SLPReport + SCB_KLPReport + SNReport

                    if (!TryGetIdsFromSelection(reportResult, out selectionTimePeriodIds, "periods"))
                        return new ActionResult((int)ActionResultSave.ReportPrintoutError, GetText(12149, "Du måste välja minst en period"));

                    #endregion
                    break;
                case SoeReportTemplateType.KPAReport:
                    #region KPAReport

                    if (!TryGetIdsFromSelection(reportResult, out selectionTimePeriodIds, "periods"))
                        return new ActionResult((int)ActionResultSave.ReportPrintoutError, GetText(12149, "Du måste välja minst en period"));

                    #endregion
                    break;
                case SoeReportTemplateType.ForaReport:
                    #region ForaReport

                    if (!TryGetIdsFromSelection(reportResult, out selectionTimePeriodIds, "periods"))
                        return new ActionResult((int)ActionResultSave.ReportPrintoutError, GetText(12149, "Du måste välja minst en period"));

                    #endregion
                    break;
                case SoeReportTemplateType.AgdEmployeeReport:
                    #region AgdEmployeeReport

                    if (!TryGetIdsFromSelection(reportResult, out selectionTimePeriodIds, "periods"))
                        return new ActionResult((int)ActionResultSave.ReportPrintoutError, GetText(12149, "Du måste välja minst en period"));

                    #endregion
                    break;
                case SoeReportTemplateType.KPADirektReport:
                    #region KPADirektReport

                    if (!TryGetIdsFromSelection(reportResult, out selectionTimePeriodIds, "periods"))
                        return new ActionResult((int)ActionResultSave.ReportPrintoutError, GetText(12149, "Du måste välja minst en period"));

                    #endregion
                    break;
                case SoeReportTemplateType.PayrollVacationAccountingReport:
                    #region PayrollVacationAccountingReport

                    bool createVacationVoucher;
                    if (TryGetBoolFromSelection(reportResult, out createVacationVoucher, "createVoucher") && createVacationVoucher)
                    {
                        int? voucherSeriesTypeId;
                        if (TryGetIdFromSelection(reportResult, out voucherSeriesTypeId, "voucherSeriesType") && voucherSeriesTypeId.HasValue)
                        {
                            DateTime voucherDate;
                            if (TryGetDateFromSelection(reportResult, out voucherDate, "voucherDate") && VoucherManager.VoucherExists(voucherDate, voucherSeriesTypeId.Value, TermGroup_VoucherHeadSourceType.PayrollVacation, reportResult.Input.ActorCompanyId))
                                return new ActionResult((int)ActionResultSave.ReportPrintoutWarning, GetText(11875, "Det finns redan löneverifikat skapad på datumet. Vill du fortsätta?"));
                        }
                    }

                    #endregion
                    break;
                case SoeReportTemplateType.Bygglosen:
                    #region Bygglosen

                    if (!TryGetIdsFromSelection(reportResult, out selectionTimePeriodIds, "periods"))
                        return new ActionResult((int)ActionResultSave.ReportPrintoutError, GetText(12149, "Du måste välja minst en period"));

                    #endregion
                    break;
                case SoeReportTemplateType.Kronofogden:
                    #region Kronofogden

                    if (!TryGetIdsFromSelection(reportResult, out selectionTimePeriodIds, "periods"))
                        return new ActionResult((int)ActionResultSave.ReportPrintoutError, GetText(12149, "Du måste välja minst en period"));

                    #endregion
                    break;
            }

            return result;
        }

        protected ActionResult ChangeStatusOnReportPrintout(ReportPrintoutDTO dto, TermGroup_ReportPrintoutStatus targetStatus)
        {
            using (CompEntities entities = new CompEntities())
            {
                if (entities.ReportPrintout.Any(f => f.ReportPrintoutId == dto.ReportPrintoutId))
                {
                    int status = (int)targetStatus;
                    base.BulkUpdateChanges<ReportPrintout>(entities.ReportPrintout.Where(f => f.ReportPrintoutId == dto.ReportPrintoutId), u => new ReportPrintout { Status = status });
                    dto.Status = (int)targetStatus;
                    return SaveChanges(entities);
                }
                else
                    return new ActionResult(true);
            }
        }

        protected ActionResult SaveReportPrintoutStarted(ReportPrintoutDTO dto)
        {
            using (CompEntities entities = new CompEntities())
            {
                entities.CommandTimeout = 300;

                ReportPrintout reportPrintout = new ReportPrintout
                {
                    ReportPrintoutId = dto.ReportPrintoutId,
                    SysReportTemplateTypeId = dto.SysReportTemplateTypeId,
                    ExportType = (int)dto.ExportType,
                    ExportFormat = (int)dto.ExportFormat,
                    DeliveryType = (int)dto.DeliveryType,
                    Status = dto.Status,
                    ReportName = dto.ReportName,
                    Selection = dto.Selection != null ? dto.Selection : String.Empty,
                    OrderedDeliveryTime = dto.OrderedDeliveryTime,
                    Created = dto.Created,
                    CreatedBy = dto.CreatedBy,

                    //Set when finished
                    DeliveredTime = null,
                    ResultMessage = 0,
                    EmailMessage = String.Empty,
                    XML = null,
                    Data = null,
                    Modified = null,
                    ModifiedBy = null,

                    //Set FK
                    ReportId = dto.ReportId,
                    ReportPackageId = dto.ReportPackageId, //not real FK
                    ReportTemplateId = dto.ReportTemplateId, //no real FK
                    ReportUrlId = dto.ReportUrlId.ToNullable(),
                    ActorCompanyId = dto.ActorCompanyId,
                    UserId = dto.UserId,
                    RoleId = dto.RoleId,
                    ResultMessageDetails = dto.ResultMessageDetails,
                };
                if (dto.ReportPrintoutId == 0)
                    entities.ReportPrintout.AddObject(reportPrintout);

                ActionResult result = SaveChanges(entities);
                if (result.Success)
                    dto.ReportPrintoutId = reportPrintout.ReportPrintoutId;
                else
                    dto.Status = (int)TermGroup_ReportPrintoutStatus.Error;

                return result;
            }
        }

        protected ActionResult SaveReportPrintoutFinished(ref ReportPrintoutDTO dto, CreateReportResult reportResult, bool internalPrintout = false)
        {
            if (reportResult == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "CreateReportResult");

            #region Update dto

            try
            {
                if (reportResult.Document != null)
                {
                    try
                    {
                        dto.XML = reportResult.Document.ToString();
                    }
                    catch (Exception ex)
                    {
                        DirectoryInfo directory = Directory.CreateDirectory(ConfigSettings.SOE_SERVER_DIR_TEMP_REPORT_PHYSICAL + Guid.NewGuid().ToString());
                        string tempPath = directory.FullName + @"\" + Guid.NewGuid().ToString();
                        try
                        {
                            LogCollector.LogError("reportResult.Document.ToString() in SaveReportPrintoutFinished failed " + ex.ToString());
                            reportResult.Document.Save(tempPath);
                            dto.XML = File.ReadAllText(tempPath);
                            Thread.Sleep(1000);
                            File.Delete(tempPath);
                        }
                        catch (Exception ex2)
                        {
                            LogCollector.LogError("dto.XML = File.ReadAllText(tempPath) in SaveReportPrintoutFinished failed " + ex2.ToString());
                            dto.XMLCompressed = CompressionUtil.CompressXDocument(reportResult.Document);
                        }

                    }
                }
                else if (reportResult.XmlDocument != null)
                    dto.XML = reportResult.XmlDocument.OuterXml.ToString();
                else if (reportResult.Data != null)
                    dto.Data = reportResult.Data;
            }
            catch (Exception ex3)
            {
                LogCollector.LogError("SaveReportPrintoutFinished " + ex3.ToString());
            }

            dto.DeliveredTime = DateTime.Now;
            dto.Status = reportResult.ResultMessage == SoeReportDataResultMessage.Success ? (int)TermGroup_ReportPrintoutStatus.Delivered : (int)TermGroup_ReportPrintoutStatus.Error;
            dto.ResultMessage = (int)reportResult.ResultMessage;
            dto.ResultMessageDetails = reportResult.ResultMessageDetails;
            dto.Modified = DateTime.Now;
            dto.ModifiedBy = GetUserDetails(parameterObject.SoeUser);

            if (dto.ExportType == TermGroup_ReportExportType.Insight)
                return new ActionResult();

            #region Email

            if (dto.Status == (int)TermGroup_ReportPrintoutStatus.Delivered)
            {
                if (dto.DeliveryType == TermGroup_ReportPrintoutDeliveryType.Email && dto.IsEmailValid)
                {
                    ActionResult result = SendReportByEmail(reportResult, dto);
                    if (result.Success)
                    {
                        dto.Status = (int)TermGroup_ReportPrintoutStatus.Sent;
                        dto.EmailMessage = result.StringValue;
                    }
                    else
                    {
                        dto.Status = (int)TermGroup_ReportPrintoutStatus.SentFailed;
                        dto.EmailMessage = result.ErrorMessage;
                    }
                }
                if (dto.Status == (int)TermGroup_ReportPrintoutStatus.Sent)
                {
                    switch (dto.SysReportTemplateTypeId)
                    {
                        case (int)SoeReportTemplateType.BillingOffer:
                        case (int)SoeReportTemplateType.BillingOrder:
                        case (int)SoeReportTemplateType.BillingInvoice:
                            List<int> invoiceIds = reportResult.EvaluatedSelection.SB_InvoiceIds;
                            if (invoiceIds != null && invoiceIds.Any() && reportResult.InvoiceDistributionId.HasValue)
                            {
                                var invoiceDest = new InvoiceDistributionManager(this.parameterObject);
                                invoiceDest.UpdateInvoiceEmailStatus(invoiceIds.First(), (TermGroup_ReportPrintoutStatus)dto.Status, dto.EmailMessage, reportResult.InvoiceDistributionId.Value);
                            }
                            break;
                        case (int)SoeReportTemplateType.PurchaseOrder:
                            List<int> purchaseIds = reportResult.EvaluatedSelection.SB_PurchaseIds;
                            if (purchaseIds != null && purchaseIds.Any() && reportResult.InvoiceDistributionId.HasValue)
                            {
                                var invoiceDest = new InvoiceDistributionManager(this.parameterObject);
                                invoiceDest.UpdatePurchaseEmailStatus(new PurchaseManager(parameterObject), purchaseIds.First(), dto.Status, dto.EmailMessage, reportResult.InvoiceDistributionId.Value);
                            }
                            break;

                    }
                }
            }

            #endregion

            #endregion

            #region Save ReportPrintout

            using (CompEntities entities = new CompEntities())
            {
                entities.CommandTimeout = 300;

                ReportPrintout reportPrintout = ReportManager.GetReportPrintout(entities, dto.ReportPrintoutId, dto.ActorCompanyId);
                if (reportPrintout == null)
                    return new ActionResult((int)ActionResultSave.EntityIsNull, "ReportPrintout");

                reportPrintout.DeliveredTime = dto.DeliveredTime;
                reportPrintout.Status = dto.Status;
                reportPrintout.ResultMessage = dto.ResultMessage;
                reportPrintout.ResultMessageDetails = dto.ResultMessageDetails;
                reportPrintout.EmailMessage = dto.EmailMessage;
                reportPrintout.ReportName = dto.ReportName;
                reportPrintout.Modified = dto.Modified;
                reportPrintout.ModifiedBy = dto.ModifiedBy;
                reportPrintout.XMLCompressed = dto.XMLCompressed ?? GeneralManager.CompressString(dto.XML);
                reportPrintout.DataCompressed = GeneralManager.CompressData(dto.Data);
                reportPrintout.ExportFormat = (int)dto.ExportFormat;

                if (internalPrintout)
                {
                    reportPrintout.Status = (int)TermGroup_ReportPrintoutStatus.Internal;
                    reportPrintout.XMLCompressed = null;
                    reportPrintout.DataCompressed = null;
                }

                return SaveChanges(entities);
            }

            #endregion
        }

        protected void SetTermGroup_ReportExportFileType(CreateReportResult reportResult)
        {
            if (reportResult.Input != null && reportResult.ExportType == TermGroup_ReportExportType.File && reportResult.Input.ExportFileType == TermGroup_ReportExportFileType.Unknown)
            {
                using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
                var report = entitiesReadOnly.Report.FirstOrDefault(f => f.ReportId == reportResult.ReportId && f.ActorCompanyId == reportResult.ActorCompanyId);

                if (report != null)
                {
                    if (report.FileType == (int)SoeReportTemplateType.Unknown)
                    {
                        switch (reportResult.ReportTemplateType)
                        {
                            case SoeReportTemplateType.AgdEmployeeReport:
                                reportResult.Input.ExportFileType = TermGroup_ReportExportFileType.AGD;
                                break;
                            case SoeReportTemplateType.SKDReport:
                                reportResult.Input.ExportFileType = TermGroup_ReportExportFileType.AGD;
                                break;
                            case SoeReportTemplateType.CollectumReport:
                                reportResult.Input.ExportFileType = TermGroup_ReportExportFileType.Collectum;
                                break;
                            case SoeReportTemplateType.ForaReport:
                                reportResult.Input.ExportFileType = TermGroup_ReportExportFileType.Fora;
                                break;
                            case SoeReportTemplateType.KPAReport:
                                reportResult.Input.ExportFileType = TermGroup_ReportExportFileType.KPA;
                                break;
                            case SoeReportTemplateType.KPADirektReport:
                                reportResult.Input.ExportFileType = TermGroup_ReportExportFileType.KPADirekt;
                                break;
                            case SoeReportTemplateType.Bygglosen:
                                reportResult.Input.ExportFileType = TermGroup_ReportExportFileType.Bygglosen;
                                break;
                            case SoeReportTemplateType.SCB_KSJUReport:
                                reportResult.Input.ExportFileType = TermGroup_ReportExportFileType.SCB_KSJU;
                                break;
                            case SoeReportTemplateType.SNReport:
                                reportResult.Input.ExportFileType = TermGroup_ReportExportFileType.Payroll_SN_Statistics;
                                break;
                            case SoeReportTemplateType.KU10Report:
                                reportResult.Input.ExportFileType = TermGroup_ReportExportFileType.KU10;
                                break;
                            case SoeReportTemplateType.PayrollAccountingReport:
                                reportResult.Input.ExportFileType = TermGroup_ReportExportFileType.Payroll_SIE_Accounting;
                                break;
                            case SoeReportTemplateType.PayrollVacationAccountingReport:
                                reportResult.Input.ExportFileType = TermGroup_ReportExportFileType.Payroll_SIE_VacationAccounting;
                                break;
                            case SoeReportTemplateType.SCB_KLPReport:
                                reportResult.Input.ExportFileType = TermGroup_ReportExportFileType.SCB_KLP;
                                break;
                            case SoeReportTemplateType.Kronofogden:
                                reportResult.Input.ExportFileType = TermGroup_ReportExportFileType.Kronofogden;
                                break;
                            case SoeReportTemplateType.FolksamGTP:
                                reportResult.Input.ExportFileType = TermGroup_ReportExportFileType.FolksamGTP;
                                break;
                            case SoeReportTemplateType.IFMetall:
                                reportResult.Input.ExportFileType = TermGroup_ReportExportFileType.IFMetall;
                                break;
                            case SoeReportTemplateType.SkandiaPension:
                                reportResult.Input.ExportFileType = TermGroup_ReportExportFileType.SkandiaPension;
                                break;
                            case SoeReportTemplateType.ForaMonthlyReport:
                                reportResult.Input.ExportFileType = TermGroup_ReportExportFileType.ForaMonthly;
                                break;
                            case SoeReportTemplateType.SEF:
                                reportResult.Input.ExportFileType = TermGroup_ReportExportFileType.SEF;
                                break;
                            case SoeReportTemplateType.TaxAudit:
                                reportResult.Input.ExportFileType = TermGroup_ReportExportFileType.eSKD;
                                break;
                            case SoeReportTemplateType.AgiAbsence:
                                reportResult.Input.ExportFileType = TermGroup_ReportExportFileType.AGD_Franvarouppgift;
                                break;
                        }
                    }
                    else
                        reportResult.Input.ExportFileType = (TermGroup_ReportExportFileType)report.FileType;
                }
            }
            else if (reportResult?.ExportType != null && reportResult.ExportType == TermGroup_ReportExportType.MatrixExcel)
            {
                switch (reportResult.ReportTemplateType)
                {
                    case SoeReportTemplateType.FolksamGTP:
                        reportResult.Input.ExportFileType = TermGroup_ReportExportFileType.FolksamGTP;
                        break;
                    case SoeReportTemplateType.IFMetall:
                        reportResult.Input.ExportFileType = TermGroup_ReportExportFileType.IFMetall;
                        break;
                    case SoeReportTemplateType.SkandiaPension:
                        reportResult.Input.ExportFileType = TermGroup_ReportExportFileType.SkandiaPension;
                        break;
                    case SoeReportTemplateType.ForaMonthlyReport:
                        reportResult.Input.ExportFileType = TermGroup_ReportExportFileType.ForaMonthly;
                        break;
                    case SoeReportTemplateType.SEF:
                        reportResult.Input.ExportFileType = TermGroup_ReportExportFileType.SEF;
                        break;

                    default:
                        break;
                }
            }
            return;
        }

        protected void SetReportName(ReportPrintoutDTO dto, CreateReportResult reportResult)
        {
            if (reportResult == null || !reportResult.HasValidSelection || dto == null)
                return;

            //ReportName
            dto.ReportName = (!String.IsNullOrEmpty(reportResult.ReportPackageName) ? reportResult.ReportPackageName : reportResult.ReportName).Trim();

            //ReportFileName
            dto.ReportFileName = dto.ReportName.Trim();

            //Append postfix
            if (!String.IsNullOrEmpty(reportResult.ReportNamePostfix))
            {
                dto.ReportName += " " + reportResult.ReportNamePostfix;
                dto.ReportFileName += " " + reportResult.ReportNamePostfix;
            }
            else
            {
                dto.ReportFileName += " " + DateTime.Now.ToString("yyyyMMddHHmmssFFF");
            }

            //Validate
            dto.ReportFileName = dto.ReportFileName.ToValidFileName("_");
        }

        protected void SetReportExportSettings(ReportPrintoutDTO dto, CreateReportResult reportResult)
        {
            if (dto == null || reportResult == null || !reportResult.HasValidSelection)
                return;

            dto.DeliveryType = ReportGenManager.GetReportDeliveryType(email: reportResult.Email);
            dto.ExportType = reportResult.ExportType;
            dto.ExportFormat = ReportGenManager.GetReportExportFormat(dto.ExportType, reportResult.ExportFileType);
            dto.ReportFileType = ReportGenManager.GetReportFileFype(dto.ExportFormat);
        }

        protected void SetEdiExportSettings(ReportPrintoutDTO dto, CreateReportResult reportResult)
        {
            if (dto == null || reportResult == null || !reportResult.HasValidSelection)
                return;

            dto.DeliveryType = TermGroup_ReportPrintoutDeliveryType.Generate;
            dto.ExportType = reportResult.ExportType;
            dto.ExportFormat = ReportGenManager.GetReportExportFormat(dto.ExportType);
            dto.ReportFileType = ReportGenManager.GetReportFileFype(dto.ExportFormat);
        }

        protected void SetReportPackageExportSettings(ReportPrintoutDTO dto, CreateReportResult reportResult)
        {
            if (reportResult == null || !reportResult.HasValidSelection || dto == null)
                return;

            Directory.CreateDirectory(ConfigSettings.SOE_SERVER_DIR_TEMP_REPORT_PHYSICAL + Guid.NewGuid().ToString());
            dto.DeliveryType = ReportGenManager.GetReportDeliveryType(email: reportResult.EvaluatedSelection.Email);
            dto.ExportType = TermGroup_ReportExportType.Pdf; //Always PDf
            dto.ExportFormat = SoeExportFormat.MergedPDF; //Always merged PDF
            dto.ReportFileType = ReportGenManager.GetReportFileFype(dto.ExportFormat);
        }

        #endregion

        #region ReportSetting elements

        protected void AddReportSettingElements(List<ReportSetting> reportSettings, XElement parent)
        {
            if (reportSettings != null && reportSettings.Any())
            {
                foreach (ReportSetting setting in reportSettings)
                {
                    string settingName = Enum.GetName(typeof(TermGroup_ReportSettingType), setting.Type) ?? "";
                    string settingValue = setting.Value;

                    if (settingName != "")
                    {
                        XElement settingElement = new XElement("ReportSetting",
                            new XAttribute("id", setting.Type),
                            new XElement("SettingName", settingName),
                            new XElement("SettingValue", settingValue));

                        parent.Add(settingElement);
                    }
                    else
                        AddDefaultReportSettingElements(parent);
                }

            }
            else
            {
                AddDefaultReportSettingElements(parent);
            }
        }

        protected void AddDefaultReportSettingElements(XElement parent)
        {
            XElement settingElement = new XElement("ReportSetting",
                        new XAttribute("id", 1),
                        new XElement("SettingName"),
                        new XElement("SettingValue"));

            parent.Add(settingElement);
        }

        #endregion

        #region ReportHeaderLabels elements

        #region ERP

        protected XElement CreateActorReportHeaderLabelsElement()
        {
            return new XElement("ReportHeaderLabels",
                        new XElement("LoginNameLabel", this.GetReportText(24, "Användare")),
                        new XElement("PageLabel", this.GetReportText(4, "Sida:")),
                        new XElement("DateLabel", this.GetReportText(5, "Datum")),
                        new XElement("TimeLabel", this.GetReportText(6, "Tid")),
                        new XElement("NumberLabel", this.GetReportText(243, "Nummer")),
                        new XElement("NameLabel", this.GetReportText(322, "Namn")),
                        new XElement("OrgNrLabel", this.GetReportText(614, "Orgnr")),
                        new XElement("VatNrLabel", this.GetReportText(447, "VAT-No")),
                        new XElement("CountryLabel", this.GetReportText(121, "Land")),
                        new XElement("ReferenceLabel", this.GetReportText(313, "Referens")),
                        new XElement("VatTypeLabel", this.GetReportText(91, "Momstyp")),
                        new XElement("PaymentConditionLabel", this.GetReportText(437, "Betalningsvillkor")),
                        new XElement("PhoneJobLabel", this.GetReportText(125, "Telefon")),
                        new XElement("EmailLabel", this.GetReportText(124, "Epost")),
                        new XElement("WebLabel", this.GetReportText(438, "Hemsida")),
                        new XElement("DeliveryAddressLabel", this.GetReportText(88, "Leveransadress")),
                        new XElement("DistributionAdressLabel", this.GetReportText(439, "Utdelningsadress")),
                        new XElement("VisitingAddressLabel", this.GetReportText(440, "Besöksadress")),
                        new XElement("BillingAddressLabel", this.GetReportText(441, "Fakturaadress")),
                        new XElement("AdressStreetLabel", this.GetReportText(385, "Gatuadress")),
                        new XElement("AdressCOLabel", this.GetReportText(382, "Adress c/o")),
                        new XElement("AdressPostalCodeLabel", this.GetReportText(383, "Postnummer")),
                        new XElement("AdressPostalAddressLabel", this.GetReportText(384, "Postort")),
                        new XElement("AdressPostalCountryLabel", this.GetReportText(121, "Land")),
                        new XElement("PaymentTypeLabel", this.GetReportText(442, "Betalningstyp")),
                        new XElement("PaymentNameLabel", this.GetReportText(322, "Namn")),
                        new XElement("PaymentAccountLabel", this.GetReportText(205, "Konto")),
                        new XElement("StandardLabel", this.GetReportText(443, "Standard")),
                        new XElement("CategoriesLabel", this.GetReportText(363, "Kategorier")),
                        new XElement("CategoryCodeLabel", this.GetReportText(364, "Kategorikod")),
                        new XElement("CategoryNameLabel", this.GetReportText(365, "Kategorinamn")),
                        new XElement("AddressesLabel", this.GetReportText(378, "Adresser")),
                        new XElement("TelecomLabel", this.GetReportText(444, "Telekom")),
                        new XElement("CurrencyLabel", this.GetReportText(332, "Valuta")),
                        new XElement("AccountDimLabel", this.GetReportText(445, "Konteringsnivå")),
                        new XElement("SupplierIdLabel", this.GetReportText(9227, "Leverantörsid")),
                        new XElement("SupplierNameLabel", this.GetReportText(9228, "Leverantörsnamn")),
                        new XElement("SupplierPaymentsLabel", this.GetReportText(824, "Betalningsuppgifter"))
                        );

        }

        protected XElement CreateAccountingReportHeaderLabelsElement()
        {
            return new XElement("ReportHeaderLabels",
                    new XElement("AccountYearLabel", this.GetReportText(1, "Redovisningsår:")),
                    new XElement("AccountPeriodLabel", this.GetReportText(2, "Aktuell period:")),
                    new XElement("LoginNameLabel", this.GetReportText(24, "Användare:")),
                    new XElement("PageLabel", this.GetReportText(4, "Sida:")),
                    new XElement("DateLabel", this.GetReportText(5, "Datum:")),
                    new XElement("TimeLabel", this.GetReportText(6, "Tid:")),
                    new XElement("TotalLabel", this.GetReportText(18, "Totalt")),
                    new XElement("VoucherTotalLabel", this.GetReportText(19, "Totalt verifikat")),
                    new XElement("VoucherSerieTotalLabel", this.GetReportText(22, "Totalt verifikatserie")),
                    new XElement("AccountPeriodTotalLabel", this.GetReportText(20, "Totalt period")),
                    new XElement("AccountYearTotalLabel", this.GetReportText(21, "Totalt år")),
                    new XElement("AccountYearToPerLabel", this.GetReportText(10109, "Totalt tom per")),
                    new XElement("PreviousYearPeriodLabel", this.GetReportText(1045, "Fg år period")),
                    new XElement("PreviousYearTotalLabel", this.GetReportText(1044, "Fg år totalt")),
                    new XElement("PreviousYearToPerLabel", this.GetReportText(1046, "Fg år tom per")),
                    new XElement("ReportSelectionTextLabel", this.GetReportText(819, "Urval namn")));
        }

        protected XElement CreateAccountingOrderReportHeaderLabelsElement(XElement parent)
        {
            parent.Add(
                new XElement("AccountingOrderPreparedByLabel", this.GetReportText(295, "Upprättad av:")),
                new XElement("AccountingOrderDateLabel", this.GetReportText(296, "Datum:")),
                new XElement("AccountingOrderAttestLabel", this.GetReportText(297, "Attest:")));

            return parent;
        }

        protected XElement CreateEnterpriseCurrencyReportHeaderLabelsElement(XElement parent)
        {
            parent.Add(
                new XElement("EntCurrencyNameLabel", this.GetReportText(301, "Valutanamn koncern")),
                new XElement("EntCurrencyCodeLabel", this.GetReportText(302, "Valutakod koncern")));

            return parent;
        }

        protected XElement CreateAccountDistributionHeadListReportHeaderLabelsElement()
        {
            return new XElement("ReportHeaderLabels",
                    new XElement("LoginNameLabel", this.GetReportText(24, "Användare:")),
                    new XElement("PageLabel", this.GetReportText(4, "Sida:")),
                    new XElement("DateLabel", this.GetReportText(5, "Datum:")),
                    new XElement("TimeLabel", this.GetReportText(6, "Tid:")),
                    new XElement("TotalLabel", this.GetReportText(18, "Totalt")));
        }

        protected XElement CreateFixedAssetsgReportHeaderLabelsElement()
        {
            return new XElement("ReportHeaderLabels",
                new XElement("LoginNameLabel", this.GetReportText(24, "Användare:")),
                new XElement("PageLabel", this.GetReportText(4, "Sida:")),
                new XElement("DateLabel", this.GetReportText(5, "Datum:")),
                new XElement("TimeLabel", this.GetReportText(6, "Tid:")),
                new XElement("TotalLabel", this.GetReportText(18, "Totalt")),
                new XElement("AccountYearLabel", this.GetReportText(1, "Redovisningsår:")),
                new XElement("AccountPeriodLabel", this.GetReportText(2, "Aktuell period:")),
                new XElement("ReportSelectionTextLabel", this.GetReportText(819, "Urval namn")),
                new XElement("CategoryIntervalLabel", this.GetReportText(867, "Kategori intervall:")),
                new XElement("InventoryIntervalLabel", this.GetReportText(868, "Inventarieintervall:")),
                new XElement("PrognoseIntervalLabel", this.GetReportText(9226, "Prognosintervall:")));
        }

        protected XElement CreateSupplierReportHeaderLabelsElement()
        {
            return new XElement("ReportHeaderLabels",
                        new XElement("LoginNameLabel", this.GetReportText(24, "Användare:")),
                        new XElement("PageLabel", this.GetReportText(4, "Sida:")),
                        new XElement("DateLabel", this.GetReportText(5, "Datum:")),
                        new XElement("TimeLabel", this.GetReportText(6, "Tid:")),
                        new XElement("ReportRegardLabel", this.GetReportText(61, "Redovisningen avser")),
                        new XElement("SortOrderLabel", this.GetReportText(62, "Sorteringsordning")),
                        new XElement("InvoiceSelectionLabel", this.GetReportText(433, "Fakturor")),
                        new XElement("DateIntervalLabel", this.GetReportText(76, "Datumintervall:")),
                        new XElement("InvoiceIntervalLabel", this.GetReportText(77, "Fakturaintervall:")),
                        new XElement("SupplierIntervalLabel", this.GetReportText(78, "Leverantörintervall:")),
                        new XElement("ReportSumLabel", this.GetReportText(79, "Totalt")),
                        new XElement("EntCurrencyNameLabel", this.GetReportText(301, "Valutanamn koncern")),
                        new XElement("EntCurrencyCodeLabel", this.GetReportText(302, "Valutakod koncern")),
                        new XElement("DateRegardLabel", this.GetReportText(9233, "Datum avseende på")),
                        new XElement("CurrencyLabel", this.GetReportText(97, "Valuta")),
                        new XElement("CurrencyRateLabel", this.GetReportText(98, "Kurs"))
                        );
        }

        protected XElement CreateCustomerReportHeaderLabelsElement()
        {
            return new XElement("ReportHeaderLabels",
                        new XElement("LoginNameLabel", this.GetReportText(24, "Användare:")),
                        new XElement("PageLabel", this.GetReportText(4, "Sida:")),
                        new XElement("DateLabel", this.GetReportText(5, "Datum:")),
                        new XElement("TimeLabel", this.GetReportText(6, "Tid:")),
                        new XElement("DateRegardLabel", this.GetReportText(61, "Redovisningen avser")),
                        new XElement("SortOrderLabel", this.GetReportText(62, "Sorteringsordning")),
                        new XElement("InvoiceSelectionLabel", this.GetReportText(433, "Fakturor")),
                        new XElement("DateIntervalLabel", this.GetReportText(76, "Datumintervall:")),
                        new XElement("InvoiceIntervalLabel", this.GetReportText(77, "Fakturaintervall:")),
                        new XElement("CustomerIntervalLabel", this.GetReportText(81, "Kundintervall:")),
                        new XElement("ReportSumLabel", this.GetReportText(79, "Totalt")),
                        new XElement("EntCurrencyNameLabel", this.GetReportText(301, "Valutanamn koncern")),
                        new XElement("EntCurrencyCodeLabel", this.GetReportText(302, "Valutakod koncern")),
                        new XElement("CurrencyLabel", this.GetReportText(97, "Valuta")),
                        new XElement("CurrencyRateLabel", this.GetReportText(98, "Kurs")));
        }

        protected XElement CreateSEPAPaymentImportReportHeaderLabelsElement()
        {
            return new XElement("ReportHeaderLabels",
                    new XElement("LoginNameLabel", this.GetReportText(9212, "Användare:")),
                    new XElement("ReportTitleLabel", this.GetReportText(9213, "Titel:")),
                    new XElement("ReportDescriptionLabel", this.GetReportText(9214, "Beskrivning:")),
                    new XElement("ReportNrLabel", this.GetReportText(9215, "Rapport nummer:")),
                    new XElement("CompanyNameLabel", this.GetReportText(9216, "Företag:")),
                    new XElement("OrgNrLabel", this.GetReportText(9217, "Orgnr:")),
                    new XElement("PageLabel", this.GetReportText(9218, "Sida:")),
                    new XElement("DateLabel", this.GetReportText(9219, "Datum:")),
                    new XElement("TimeLabel", this.GetReportText(9220, "Tid:")));
        }

        protected XElement CreateInterestRateCalculationReportHeaderLabelsElement()
        {
            return new XElement("ReportHeaderLabels",
                    new XElement("LoginNameLabel", this.GetReportText(9212, "Användare:")),
                    new XElement("PageLabel", this.GetReportText(9218, "Sida:")),
                    new XElement("DateLabel", this.GetReportText(9219, "Datum:")),
                    new XElement("TimeLabel", this.GetReportText(9220, "Tid:")),
                    new XElement("DateIntervalLabel", this.GetReportText(0, "")),
                    new XElement("ReportTitleLabel", this.GetReportText(9213, "Titel:")),
                    new XElement("ReportDescriptionLabel", this.GetReportText(9214, "Beskrivning:")),
                    new XElement("ReportNrLabel", this.GetReportText(9215, "Rapport nummer:")),
                    new XElement("CompanyNameLabel", this.GetReportText(9216, "Företag:")),
                    new XElement("OrgNrLabel", this.GetReportText(9217, "Orgnr:")),
                    new XElement("InterestRateLabel", this.GetReportText(930, "Ränte:")),
                    new XElement("ReportSumLabel", this.GetReportText(79, "Totalt"))
                    );
        }

        protected XElement CreateBillingReportHeaderLabelsElement(SoeReportTemplateType reportTemplateType)
        {
            #region Prereq

            string invoiceIntervalLabel = String.Empty;

            switch (reportTemplateType)
            {
                case SoeReportTemplateType.BillingContract:
                    invoiceIntervalLabel = this.GetReportText(240, "Avtalsintervall:");
                    break;
                case SoeReportTemplateType.BillingOffer:
                    invoiceIntervalLabel = this.GetReportText(252, "Offertintervall:");
                    break;
                case SoeReportTemplateType.BillingOrder:
                case SoeReportTemplateType.BillingOrderOverview:
                    invoiceIntervalLabel = this.GetReportText(253, "Orderintervall:");
                    break;
                case SoeReportTemplateType.BillingInvoice:
                case SoeReportTemplateType.BillingInvoiceInterest:
                case SoeReportTemplateType.BillingInvoiceReminder:
                    invoiceIntervalLabel = this.GetReportText(77, "Fakturaintervall:");
                    break;

                //should never end up in here
                default:
                    invoiceIntervalLabel = this.GetReportText(77, "Fakturaintervall:");
                    break;
            }

            #endregion

            return new XElement("ReportHeaderLabels",
                    new XElement("LoginNameLabel", this.GetReportText(24, "Användare:")),
                    new XElement("PageLabel", this.GetReportText(4, "Sida:")),
                    new XElement("DateLabel", this.GetReportText(5, "Datum:")),
                    new XElement("TimeLabel", this.GetReportText(6, "Tid:")),
                    new XElement("SortOrderLabel", this.GetReportText(62, "Sorteringsordning")),
                    new XElement("InvoiceIntervalLabel", invoiceIntervalLabel),
                    new XElement("CustomerIntervalLabel", this.GetReportText(81, "Kundintervall:")),
                    new XElement("CompanyVatNrLabel", this.GetReportText(135, "Vårt moms.reg.nr")),
                    new XElement("CompanyAddressLabel", this.GetReportText(117, "Adress")),
                    new XElement("CompanyAddressCOLabel", this.GetReportText(118, "C/O adress")),
                    new XElement("CompanyPostalCodeLabel", this.GetReportText(119, "Postnummer")),
                    new XElement("CompanyPostalAddressLabel", this.GetReportText(120, "Postadress")),
                    new XElement("CompanyCountryLabel", this.GetReportText(121, "Land")),
                    new XElement("CompanyBoardHQLabel", this.GetReportText(136, "Styrelsens säte:")),
                    new XElement("CompanyEmailLabel", this.GetReportText(124, "Epost")),
                    new XElement("CompanyPhoneLabel", this.GetReportText(125, "Telefon")),
                    new XElement("CompanyFaxLabel", this.GetReportText(126, "Fax")),
                    new XElement("CompanyWebAddressLabel", this.GetReportText(134, "Webb")),
                    new XElement("CompanyBgLabel", this.GetReportText(127, "Bankgiro")),
                    new XElement("CompanyPgLabel", this.GetReportText(128, "Plusgiro")),
                    new XElement("CompanyBankLabel", this.GetReportText(360, "Bank")),
                    new XElement("CompanyBicLabel", this.GetReportText(450, "BIC")),
                    new XElement("CompanyIbanLabel", GetReportText(637, "IBAN")),
                    new XElement("CompanySepaLabel", this.GetReportText(362, "Sepa")),
                    new XElement("CompanyOrgNrLabel", this.GetReportText(409, "Organisationsnummer")),
                    new XElement("TimePeriodLabel", this.GetReportText(150, "Period:")),
                    new XElement("InvertedVatTextLongLabel", this.GetReportText(947, "Omvänd skattskyldighet inom byggbranschen. Köparen ansvarig enligt 8 v § i momslagen")));

        }

        protected XElement CreateChecklistReportHeaderLabelsElement()
        {
            return new XElement("ReportHeaderLabels",
                    new XElement("LoginNameLabel", this.GetReportText(24, "Användare:")),
                    new XElement("PageLabel", this.GetReportText(4, "Sida:")),
                    new XElement("DateLabel", this.GetReportText(5, "Datum:")),
                    new XElement("TimeLabel", this.GetReportText(6, "Tid:")));
        }

        protected XElement CreateProjectTransactionsReportHeaderLabelsElement()
        {
            return new XElement("ReportHeaderLabels",
                    new XElement("LoginNameLabel", this.GetReportText(24, "Användare:")),
                    new XElement("PageLabel", this.GetReportText(4, "Sida:")),
                    new XElement("DateLabel", this.GetReportText(5, "Datum:")),
                    new XElement("TimeLabel", this.GetReportText(6, "Tid:")));
        }

        protected XElement CreateProjectReportHeaderLabelsElement()
        {
            return new XElement("ReportHeaderLabels",
                    new XElement("LoginNameLabel", this.GetReportText(24, "Användare:")),
                    new XElement("PageLabel", this.GetReportText(4, "Sida:")),
                    new XElement("DateLabel", this.GetReportText(5, "Datum:")),
                    new XElement("TimeLabel", this.GetReportText(6, "Tid:"))
                    );
        }

        protected XElement CreateStockAndProjectReportHeaderLabelsElement()
        {
            return new XElement("ReportHeaderLabels",
                    new XElement("LoginNameLabel", this.GetReportText(24, "Användare:")),
                    new XElement("PageLabel", this.GetReportText(4, "Sida:")),
                    new XElement("DateLabel", this.GetReportText(5, "Datum:")),
                    new XElement("TimeLabel", this.GetReportText(6, "Tid:")),
                    new XElement("SortOrderLabel", this.GetReportText(62, "Sorteringsordning")),
                    new XElement("StockIntervalLabel", this.GetReportText(862, "Lagerintervall")),
                    new XElement("StockShelfIntervalLabel", this.GetReportText(865, "Hyllplatsintervall")),
                    new XElement("StockProductIntervalLabel", this.GetReportText(863, "Artikelintervall")),
                    new XElement("DateIntervalLabel", this.GetReportText(864, "Datumintervall")),
                    new XElement("ProductGroupIntervalLabel", this.GetReportText(1052, "Produktgruppintervall"))
                    );
        }

        protected XElement CreateStockInventoryReportHeaderLabelsElement()
        {
            return new XElement("ReportHeaderLabels",
                    new XElement("LoginNameLabel", this.GetReportText(24, "Användare:")),
                    new XElement("PageLabel", this.GetReportText(4, "Sida:")),
                    new XElement("DateLabel", this.GetReportText(5, "Datum:")),
                    new XElement("TimeLabel", this.GetReportText(6, "Tid:")),
                    new XElement("SortOrderLabel", this.GetReportText(62, "Sorteringsordning")),
                    new XElement("StockIntervalLabel", this.GetReportText(862, "Lagerintervall")),
                    new XElement("StockShelfIntervalLabel", this.GetReportText(865, "Hyllplatsintervall")),
                    new XElement("StockProductIntervalLabel", this.GetReportText(863, "Artikelintervall")),
                    new XElement("DateIntervalLabel", this.GetReportText(864, "Datumintervall")),
                    new XElement("StockInventoryHeaderTextLabel", this.GetReportText(895, "Inventeringsunderlag")),
                    new XElement("ProductGroupIntervalLabel", this.GetReportText(1052, "Produktgruppintervall")),
                    new XElement("StockValueDateLabel", this.GetReportText(1053, "Lagervärdesdatum"))
               );
        }

        protected XElement CreateStockInventoryReportHeaderElement(EvaluatedSelection es)
        {
            string accountYearInterval = GetAccountYearIntervalText(es);
            string accountPeriodInterval = GetAccountPeriodIntervalText(es);
            string sortOrderName = this.GetBillingInvoiceSortOrderText(es, (int)TermGroup.ReportBillingStockSortOrder);

            return new XElement("ReportHeader",
                    this.CreateReportTitleElement(es.ReportName),
                    this.CreateReportDescriptionElement(es.ReportDescription),
                    this.CreateReportNrElement(es.ReportNr.ToString()),
                    this.CreateCompanyElement(),
                    this.CreateCompanyOrgNrElement(),
                    new XElement("AccountYear", accountYearInterval),
                    new XElement("AccountPeriod", accountPeriodInterval),
                    this.CreateLoginNameElement(es.LoginName),
                    new XElement("SortOrder", es.SB_SortOrder),
                    new XElement("SortOrderName", sortOrderName),
                    new XElement("StockInterval", CreateStockIntervalText(es)),
                    new XElement("StockShelfInterval", CreateStockShelfIntervalText(es)),
                    new XElement("StockProductInterval", CreateStockProductIntervalText(es)),
                    new XElement("DateInterval", this.GetDateIntervalText(es)),
                    new XElement("StockInventoryHeaderText", this.GetStockInventoryHeaderText(es))
                    );
        }

        protected XElement CreatePurchaseOrderReportHeaderLabelsElement()
        {
            return new XElement("ReportHeaderLabels",
                    new XElement("LoginNameLabel", this.GetReportText(24, "Användare:")),
                    new XElement("PageLabel", this.GetReportText(4, "Sida:")),
                    new XElement("DateLabel", this.GetReportText(5, "Datum:")),
                    new XElement("TimeLabel", this.GetReportText(6, "Tid:")),
                    new XElement("SortOrderLabel", this.GetReportText(62, "Sorteringsordning")),
                    new XElement("PurchaseInvoiceIntervalLabel", this.GetReportText(1113, "Bestälningsintervall")),
                    new XElement("SupplierIntervalLabel", this.GetReportText(78, "Leverantörsintervall:")),
                    new XElement("CompanyVatNrLabel", this.GetReportText(135, "Vårt moms.reg.nr")),
                    new XElement("CompanyAddressLabel", this.GetReportText(117, "Adress")),
                    new XElement("CompanyAddressCOLabel", this.GetReportText(118, "C/O adress")),
                    new XElement("CompanyPostalCodeLabel", this.GetReportText(119, "Postnummer")),
                    new XElement("CompanyPostalAddressLabel", this.GetReportText(120, "Postadress")),
                    new XElement("CompanyCountryLabel", this.GetReportText(121, "Land")),
                    new XElement("CompanyBoardHQLabel", this.GetReportText(136, "Styrelsens säte:")),
                    new XElement("CompanyEmailLabel", this.GetReportText(124, "Epost")),
                    new XElement("CompanyPhoneLabel", this.GetReportText(125, "Telefon")),
                    new XElement("CompanyFaxLabel", this.GetReportText(126, "Fax")),
                    new XElement("CompanyWebAddressLabel", this.GetReportText(134, "Webb")),
                    new XElement("CompanyBgLabel", this.GetReportText(127, "Bankgiro")),
                    new XElement("CompanyPgLabel", this.GetReportText(128, "Plusgiro")),
                    new XElement("CompanyBankLabel", this.GetReportText(360, "Bank")),
                    new XElement("CompanyBicLabel", this.GetReportText(361, "BIC/SWIFT")),
                    new XElement("CompanySepaLabel", this.GetReportText(362, "Sepa")),
                    new XElement("CompanyOrgNrLabel", this.GetReportText(409, "Organisationsnummer")),
                    new XElement("TimePeriodLabel", this.GetReportText(150, "Period:"))
                );
        }

        #endregion

        #region HR

        protected XElement CreatePayrollReportHeaderLabelsElement()
        {
            return new XElement("ReportHeaderLabels",
                    new XElement("LoginNameLabel", this.GetReportText(24, "Användare:")),
                    new XElement("PageLabel", this.GetReportText(4, "Sida:")),
                    new XElement("DateLabel", this.GetReportText(5, "Datum:")),
                    new XElement("TimeLabel", this.GetReportText(6, "Tid:")),
                    new XElement("TimePeriodLabel", this.GetReportText(150, "Period:")),
                    new XElement("CompanyNameLabel", this.GetReportText(669, "Företag:")),
                    new XElement("OrgNrLabel", this.GetReportText(670, "Orgnr:")));
        }

        protected XElement CreatePayrollTransactionStatisticsPageHeaderLabelsElement()
        {
            return new XElement("PageHeaderLabels",
                    new XElement("TimeBlockDateLabel", this.GetReportText(671, "Tidblock")),
                    new XElement("AttestStateLabel", this.GetReportText(672, "Atteststatus")),
                    new XElement("PayrollProductNameLabel", this.GetReportText(673, "Löneart")),
                    new XElement("PayrollProductNumberLabel", this.GetReportText(674, "Löneart nummer")),
                    new XElement("PayrollProductDescriptionLabel", this.GetReportText(675, "Löneart beskrivning")),
                    new XElement("TimeCodeNameLabel", this.GetReportText(676, "Tidkod")),
                    new XElement("TimeCodeNumberLabel", this.GetReportText(677, "Tidkod nummer")),
                    new XElement("TimeCodeDescriptionLabel", this.GetReportText(678, "Tidkod beskrivning")),
                    new XElement("TimeBlockStartTimeLabel", this.GetReportText(679, "Starttid")),
                    new XElement("TimeBlockStopTimeLabel", this.GetReportText(680, "Sluttid")),
                    new XElement("SysPayrollTypeLevel1Label", this.GetReportText(681, "Lönetyp nivå1")),
                    new XElement("SysPayrollTypeLevel2Label", this.GetReportText(682, "Lönetyp nivå2")),
                    new XElement("SysPayrollTypeLevel3Label", this.GetReportText(683, "Lönetyp nivå3")),
                    new XElement("SysPayrollTypeLevel4Label", this.GetReportText(684, "Lönetyp nivå4")),
                    new XElement("UnitPriceLabel", this.GetReportText(685, "Pris")),
                    new XElement("UnitPriceCurrencyLabel", this.GetReportText(686, "Pris valuta")),
                    new XElement("UnitPriceEntCurrencyLabel", this.GetReportText(687, "Pris koncernvaluta")),
                    new XElement("UnitPriceLedgerCurrencyLabel", this.GetReportText(688, "Pris reskontravaluta")),
                    new XElement("AmountLabel", this.GetReportText(689, "Belopp")),
                    new XElement("AmountCurrencyLabel", this.GetReportText(690, "Belopp valuta")),
                    new XElement("AmountEntCurrencyLabel", this.GetReportText(691, "Belopp koncernvaluta")),
                    new XElement("AmountLedgerCurrencyLabel", this.GetReportText(692, "Belopp reskontravaluta")),
                    new XElement("VatAmountLabel", this.GetReportText(693, "Moms")),
                    new XElement("VatAmountCurrencyLabel", this.GetReportText(694, "Moms valuta")),
                    new XElement("VatAmountEntCurrencyLabel", this.GetReportText(695, "Moms koncernvaluta")),
                    new XElement("VatAmountLedgerCurrencyLabel", this.GetReportText(696, "Moms reskontravaluta")),
                    new XElement("QuantityLabel", this.GetReportText(697, "Antal")),
                    new XElement("ManuallyAddedLabel", this.GetReportText(698, "Manuellt tillagd")),
                    new XElement("AutoAttestFailedLabel", this.GetReportText(699, "Automatattest misslyckades")),
                    new XElement("ExportedLabel", this.GetReportText(700, "Exporterad")),
                    new XElement("IsPreliminaryLabel", this.GetReportText(701, "Preliminär")),
                    new XElement("TimeUnitLabel", this.GetReportText(816, "Tidenhet")),
                    new XElement("CalendarDayFactorLabel", this.GetReportText(817, "Kalenderdagsfaktor")),
                    new XElement("FormulaLabel", this.GetReportText(702, "Formel")),
                    new XElement("FormulaPlainLabel", this.GetReportText(703, "Löneformel")),
                    new XElement("FormulaExtractedLabel", this.GetReportText(704, "Belopp")),
                    new XElement("FormulaNamesLabel", this.GetReportText(705, "Beräknad lön")),
                    new XElement("FormulaOriginLabel", this.GetReportText(706, "Löneformel orginal")),
                    new XElement("WorkTimeWeekLabel", this.GetReportText(707, "Veckoarbetstid")),
                    new XElement("EmployeeGroupNameLabel", this.GetReportText(708, "Tidavtal")),
                    new XElement("PayrollGroupNameLabel", this.GetReportText(709, "Löneavtal")),
                    new XElement("PayrollCalculationPerformedLabel", this.GetReportText(710, "Löneberäkning utförd")),
                    new XElement("CreatedLabel", this.GetReportText(711, "Skapad")),
                    new XElement("ModifiedLabel", this.GetReportText(712, "Modifierad")),
                    new XElement("CommentLabel", this.GetReportText(713, "Kommentar")),
                    new XElement("CreatedByLabel", this.GetReportText(714, "Skapad av")),
                    new XElement("ModifiedByLabel", this.GetReportText(715, "Modifierad av")),
                    new XElement("AccountStringLabel", this.GetReportText(716, "Kontosträng")),
                    new XElement("IsRetroactiveLabel", this.GetReportText(925, "Retroaktiv lön")),
                    new XElement("PayrollsumLabel", this.GetReportText(1016, "Lönesumma")),
                    new XElement("officialsLabel", this.GetReportText(1017, "tjänstemän")),
                    new XElement("workerLabel", this.GetReportText(1018, "arbetare")),
                    new XElement("fulltimeJobsLabel", this.GetReportText(1019, "heltidstjänster")),
                    new XElement("workplaceNoLabel", this.GetReportText(1020, "Arbetsplatsnr")),
                    new XElement("fulltimeJobsDeclLabel", this.GetReportText(1021, "Heltidstjänster beräknas i denna rapport som ett snitt på den sysselsättningsgrad en anställd har inlagd i anställdakortet under vald period."))
                    );
        }

        protected XElement CreatePayrollAccountingReportHeaderLabelsElement()
        {
            return new XElement("ReportHeaderLabels",
                    new XElement("LoginNameLabel", this.GetReportText(24, "Användare:")),
                    new XElement("PageLabel", this.GetReportText(4, "Sida:")),
                    new XElement("DateLabel", this.GetReportText(5, "Datum:")),
                    new XElement("TimeLabel", this.GetReportText(6, "Tid:")),
                    new XElement("TimePeriodLabel", this.GetReportText(150, "Period:")),
                    new XElement("CompanyNameLabel", this.GetReportText(669, "Företag:")),
                    new XElement("OrgNrLabel", this.GetReportText(670, "Orgnr:")));
        }

        protected XElement CreatePayrollProductReportHeaderLabelsElement()
        {
            return new XElement("ReportHeaderLabels",
                    new XElement("LoginNameLabel", this.GetReportText(9212, "Användare:")),
                    new XElement("ReportTitleLabel", this.GetReportText(9213, "Titel:")),
                    new XElement("ReportDescriptionLabel", this.GetReportText(9214, "Beskrivning:")),
                    new XElement("ReportNrLabel", this.GetReportText(9215, "Rapport nummer:")),
                    new XElement("CompanyNameLabel", this.GetReportText(9216, "Företag:")),
                    new XElement("OrgNrLabel", this.GetReportText(9217, "Orgnr:")),
                    new XElement("PageLabel", this.GetReportText(9218, "Sida:")),
                    new XElement("DateLabel", this.GetReportText(9219, "Datum:")),
                    new XElement("TimeLabel", this.GetReportText(9220, "Tid:")));
        }

        protected XElement CreateTimeReportHeaderLabelsElement()
        {
            return new XElement("ReportHeaderLabels",
                    new XElement("LoginNameLabel", this.GetReportText(24, "Användare:")),
                    new XElement("PageLabel", this.GetReportText(4, "Sida:")),
                    new XElement("DateLabel", this.GetReportText(5, "Datum:")),
                    new XElement("TimeLabel", this.GetReportText(6, "Tid:")),
                    new XElement("TimePeriodLabel", this.GetReportText(150, "Period:")));
        }

        protected XElement CreatePayrollProductIntervalReportHeaderLabelsElement(XElement parent)
        {
            parent.Add(
                new XElement("PayrollProductIntervalLabel", this.GetReportText(774, "Löneartsurval:")));

            return parent;
        }

        protected void AddGrossNetCostHeaderPermissionAndSettingReportHeaderLabelElements(XElement parent)
        {
            if (parent == null)
                return;

            parent.Add(
                new XElement("NoShowCostsPermissionLabel", this.GetReportText(413, "Behörighet för att se kostnad saknas")),
                new XElement("NoShowGrossNetTimeSettingLabel", this.GetReportText(414, "Visa brutto-/nettotid ej aktiverat")));
        }

        protected XElement CreateDateIntervalLabelReportHeaderLabelsElement()
        {
            return new XElement("DateIntervalLabel", this.GetReportText(30, "Datumintervall:"));
        }
        protected void AddScenarioLabelReportHeaderLabelsElement(XElement parent)
        {
            if (parent == null)
                return;

            parent.Add(
                new XElement("ScenarioLabel", this.GetReportText(941, "Scenario:")));
        }

        protected void AddLendedDescriptionLabelsReportHeaderLabelsElement(XElement parent)
        {
            if (parent == null)
                return;

            parent.Add(
                new XElement("LendedInDescriptionLabel", this.GetReportText(1049, "Anställd har annan tillhörighet / Inlånad")),
                new XElement("LendedOutDescriptionLabel", this.GetReportText(1050, "Pass har annan tillhörighet / Utlånad")));
        }

        protected void AddPublicationLabelsReportHeaderLabelsElement(XElement parent)
        {
            if (parent == null)
                return;

            parent.Add(
                new XElement("PublicationDateLabel", this.GetReportText(1116, "Publicering skapad:")),
                new XElement("PublicationByLabel", this.GetReportText(1117, "Publicering skapad av:")));
        }

        #endregion

        #endregion

        #region ReportHeader elements

        protected void AddScenarioReportHeaderElements(XElement parent, TimeScheduleScenarioHead scenarioHead)
        {
            if (parent == null)
                return;

            parent.Add(
                new XElement("ScenarioName", scenarioHead?.Name ?? ""),
                new XElement("ScenarioDateInterval", string.Format("{0}-{1}", scenarioHead?.DateFrom.ToShortDateString(), scenarioHead?.DateTo.ToShortDateString())));
        }

        protected void AddShowOnlyTotalsReportHeaderElement(XElement parent, bool showOnlyTotals)
        {
            if (parent == null)
                return;

            parent.Add(
                new XElement("ShowOnlyTotals", showOnlyTotals.ToInt()));
        }

        protected void AddUseAccountHierarchyReportHeaderElement(XElement parent, bool useAccountHierarchy)
        {
            if (parent == null)
                return;

            parent.Add(
                new XElement("UseAccountHierarchy", useAccountHierarchy.ToInt()));
        }

        #region ERP

        protected XElement CreateLatestVoucherLabelReportHeaderLabelsElement()
        {
            return new XElement("LatestVoucherLabel", this.GetReportText(3, "Senaste verifikat:"));
        }

        protected XElement CreateAccountIntervalLabelReportHeaderLabelsElement()
        {
            return new XElement("AccountIntervalLabel", this.GetReportText(16, "Kontointervall:"));
        }

        protected XElement CreateVoucherIntervalLabelReportHeaderLabelsElement()
        {
            return new XElement("VoucherIntervalLabel", this.GetReportText(17, "Verifikatintervall:"));
        }

        protected XElement CreateVoucherSerieIntervalLabelReportHeaderLabelsElement()
        {
            return new XElement("VoucherSerieIntervalLabel", this.GetReportText(23, "Verifikatserieintervall:"));
        }

        protected XElement CreateActorHeaderLabelsElement(EvaluatedSelection es)
        {
            return new XElement("ReportHeaderLabels",
                        new XElement("LoginNameLabel", this.GetReportText(24, "Användare")),
                        new XElement("PageLabel", this.GetReportText(4, "Sida:")),
                        new XElement("DateLabel", this.GetReportText(5, "Datum")),
                        new XElement("TimeLabel", this.GetReportText(6, "Tid")),
                        new XElement("IntervalLabel", this.GetReportText(446, "Intervall:")));
        }

        protected XElement CreateAccountingReportHeaderElement(EvaluatedSelection es)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            string companyLogoPath = GetCompanyLogoFilePath(entitiesReadOnly, es.ActorCompanyId, false);

            return new XElement("ReportHeader",
                    this.CreateReportTitleElement(es.ReportName),
                    this.CreateReportDescriptionElement(es.ReportDescription),
                    this.CreateReportNrElement(es.ReportNr.ToString()),
                    this.CreateReportSelectionTextElement(es.ReportSelectionText),
                   this.CreateCompanyElement(),
                   this.CreateCompanyOrgNrElement(),
                   new XElement("AccountYear", GetAccountYearIntervalText(es)),
                   new XElement("AccountYearLong", GetLongAccountYearIntervalText(es)),
                   new XElement("AccountPeriod", GetAccountPeriodIntervalText(es)),
                   new XElement("PreviousAccountYear", GetPreviousAccountYearIntervalText(es)),
                   this.CreateLoginNameElement(es.LoginName),
                   new XElement("AccountPeriodFrom", es.DateFrom.Date),
                   new XElement("AccountPeriodTo", es.DateTo.Date),
                   new XElement("CompanyLogo", companyLogoPath));
        }

        protected XElement CreateTaxAuditReportHeaderElement(EvaluatedSelection es, XElement parent, AccountPeriod nextAccountPeriod)
        {
            DateTimeFormatInfo dtfi = new CultureInfo(Thread.CurrentThread.CurrentCulture.Name, false).DateTimeFormat;

            string taxVatPeriod = dtfi.MonthNames[es.DateFrom.Month - 1] + " " + es.DateFrom.Year.ToString() + " - " + dtfi.MonthNames[es.DateTo.Month - 1] + " " + es.DateTo.Year.ToString();
            string taxLabourPeriod = "";
            if (nextAccountPeriod != null)
                taxLabourPeriod = dtfi.MonthNames[nextAccountPeriod.From.Month - 1] + " " + nextAccountPeriod.From.Year.ToString();

            //Add 1 one month, and day 12 the month after that (16 if januari)
            DateTime taxAuditDate = es.DateTo.AddMonths(2);
            taxAuditDate = new DateTime(taxAuditDate.Year, taxAuditDate.Month, taxAuditDate.Month == 1 ? 16 : 12);

            parent.Add(
                this.CreateCompanyElement(),
                new XElement("CompanyTaxSupport", Convert.ToBoolean(Company.CompanyTaxSupport).ToInt()),
                new XElement("CompanyVATNr", Company.VatNr),
                new XElement("TaxAuditDate", taxAuditDate),
                new XElement("TaxVatPeriod", taxVatPeriod),
                new XElement("TaxLabourPeriod", taxLabourPeriod));

            return parent;
        }

        protected XElement CreateAccountDistributionHeadListReportHeaderElement(EvaluatedSelection es)
        {
            return new XElement("ReportHeader",
                    this.CreateReportTitleElement(es.ReportName),
                    this.CreateReportDescriptionElement(es.ReportDescription),
                    this.CreateReportNrElement(es.ReportNr.ToString()),
                   this.CreateCompanyElement(),
                   this.CreateCompanyOrgNrElement(),
                   this.CreateLoginNameElement(es.LoginName));

        }

        protected XElement CreateAccountDistributionHeadListReportHeaderElement(CreateReportResult reportResult)
        {
            return new XElement("ReportHeader",
                    this.CreateReportTitleElement(reportResult.ReportName),
                    this.CreateReportDescriptionElement(reportResult.ReportDescription),
                    this.CreateReportNrElement(reportResult.ReportNr.ToString()),
                   this.CreateCompanyElement(),
                   this.CreateCompanyOrgNrElement(),
                   this.CreateLoginNameElement(reportResult.LoginName));

        }

        protected XElement CreateFixedAssetsReportHeaderElement(EvaluatedSelection es)
        {
            DateTime prognoseInterval1From = CalendarUtility.GetFirstDateOfMonth(es.DateTo.AddMonths(1));
            DateTime prognoseInterval1To = CalendarUtility.DATETIME_DEFAULT;
            if (es.SFA_PrognoseType == 1)
                prognoseInterval1To = CalendarUtility.GetLastDateOfMonth(es.DateTo.AddMonths(1));
            if (es.SFA_PrognoseType == 2)
                prognoseInterval1To = CalendarUtility.GetLastDateOfMonth(es.DateTo.AddMonths(3));
            if (es.SFA_PrognoseType == 3)
                prognoseInterval1To = CalendarUtility.GetLastDateOfMonth(es.DateTo.AddMonths(6));
            if (es.SFA_PrognoseType == 4)
                prognoseInterval1To = CalendarUtility.GetLastDateOfMonth(es.DateTo.AddMonths(12));

            DateTime prognoseInterval2From = CalendarUtility.GetFirstDateOfMonth(prognoseInterval1To.AddMonths(1));
            DateTime prognoseInterval2To = CalendarUtility.DATETIME_DEFAULT;
            if (es.SFA_PrognoseType == 1)
                prognoseInterval2To = CalendarUtility.GetLastDateOfMonth(prognoseInterval1To.AddMonths(1));
            if (es.SFA_PrognoseType == 2)
                prognoseInterval2To = CalendarUtility.GetLastDateOfMonth(prognoseInterval1To.AddMonths(3));
            if (es.SFA_PrognoseType == 3)
                prognoseInterval2To = CalendarUtility.GetLastDateOfMonth(prognoseInterval1To.AddMonths(6));
            if (es.SFA_PrognoseType == 4)
                prognoseInterval2To = CalendarUtility.GetLastDateOfMonth(prognoseInterval1To.AddMonths(12));

            DateTime prognoseInterval3From = CalendarUtility.GetFirstDateOfMonth(prognoseInterval2To.AddMonths(1));
            DateTime prognoseInterval3To = CalendarUtility.DATETIME_DEFAULT;
            if (es.SFA_PrognoseType == 1)
                prognoseInterval3To = CalendarUtility.GetLastDateOfMonth(prognoseInterval2To.AddMonths(1));
            if (es.SFA_PrognoseType == 2)
                prognoseInterval3To = CalendarUtility.GetLastDateOfMonth(prognoseInterval2To.AddMonths(3));
            if (es.SFA_PrognoseType == 3)
                prognoseInterval3To = CalendarUtility.GetLastDateOfMonth(prognoseInterval2To.AddMonths(6));
            if (es.SFA_PrognoseType == 4)
                prognoseInterval3To = CalendarUtility.GetLastDateOfMonth(prognoseInterval2To.AddMonths(12));

            DateTime prognoseInterval4From = CalendarUtility.GetFirstDateOfMonth(prognoseInterval3To.AddMonths(1));
            DateTime prognoseInterval4To = CalendarUtility.DATETIME_DEFAULT;
            if (es.SFA_PrognoseType == 1)
                prognoseInterval4To = CalendarUtility.GetLastDateOfMonth(prognoseInterval3To.AddMonths(1));
            if (es.SFA_PrognoseType == 2)
                prognoseInterval4To = CalendarUtility.GetLastDateOfMonth(prognoseInterval3To.AddMonths(3));
            if (es.SFA_PrognoseType == 3)
                prognoseInterval4To = CalendarUtility.GetLastDateOfMonth(prognoseInterval3To.AddMonths(6));
            if (es.SFA_PrognoseType == 4)
                prognoseInterval4To = CalendarUtility.GetLastDateOfMonth(prognoseInterval3To.AddMonths(12));

            DateTime prognoseInterval5From = CalendarUtility.GetFirstDateOfMonth(prognoseInterval4To.AddMonths(1));
            DateTime prognoseInterval5To = CalendarUtility.DATETIME_DEFAULT;
            if (es.SFA_PrognoseType == 1)
                prognoseInterval5To = CalendarUtility.GetLastDateOfMonth(prognoseInterval4To.AddMonths(1));
            if (es.SFA_PrognoseType == 2)
                prognoseInterval5To = CalendarUtility.GetLastDateOfMonth(prognoseInterval4To.AddMonths(3));
            if (es.SFA_PrognoseType == 3)
                prognoseInterval5To = CalendarUtility.GetLastDateOfMonth(prognoseInterval4To.AddMonths(6));
            if (es.SFA_PrognoseType == 4)
                prognoseInterval5To = CalendarUtility.GetLastDateOfMonth(prognoseInterval4To.AddMonths(12));

            return new XElement("ReportHeader",
                   this.CreateReportTitleElement(es.ReportName),
                   this.CreateReportDescriptionElement(es.ReportDescription),
                   this.CreateReportNrElement(es.ReportNr.ToString()),
                   this.CreateReportSelectionTextElement(es.ReportSelectionText),
                   this.CreateCompanyElement(),
                   this.CreateCompanyOrgNrElement(),
                   new XElement("AccountYear", GetAccountYearIntervalText(es)),
                   new XElement("AccountYearLong", GetLongAccountYearIntervalText(es)),
                   new XElement("AccountPeriod", GetAccountPeriodIntervalText(es)),
                   new XElement("InventoryInterval", GetInventoryIntervalText(es)),
                   new XElement("CategoryInterval", GetCategoryIntervalText(es)),
                   new XElement("PrognoseInterval", this.GetInventoryPrognoseText(es, (int)TermGroup.PrognosTypes)),
                   new XElement("PrognoseInterval1", GetPrognoseIntervalText(prognoseInterval1From.ToString("yyyyMMdd"), prognoseInterval1To.ToString("yyyyMMdd"))),
                   new XElement("PrognoseInterval2", GetPrognoseIntervalText(prognoseInterval2From.ToString("yyyyMMdd"), prognoseInterval2To.ToString("yyyyMMdd"))),
                   new XElement("PrognoseInterval3", GetPrognoseIntervalText(prognoseInterval3From.ToString("yyyyMMdd"), prognoseInterval3To.ToString("yyyyMMdd"))),
                   new XElement("PrognoseInterval4", GetPrognoseIntervalText(prognoseInterval4From.ToString("yyyyMMdd"), prognoseInterval4To.ToString("yyyyMMdd"))),
                   new XElement("PrognoseInterval5", GetPrognoseIntervalText(prognoseInterval5From.ToString("yyyyMMdd"), prognoseInterval5To.ToString("yyyyMMdd"))),
                   this.CreateLoginNameElement(es.LoginName));
        }

        protected XElement CreateSupplierReportHeaderElement(EvaluatedSelection es)
        {
            Currency currency = CountryCurrencyManager.GetCurrencyFromType(Company.ActorCompanyId, TermGroup_CurrencyType.EnterpriseCurrency);

            if (es.ReportTemplateType == SoeReportTemplateType.SupplierBalanceList)
            {
                return new XElement("ReportHeader",
                        this.CreateReportTitleElement(es.ReportName),
                        this.CreateReportDescriptionElement(es.ReportDescription),
                        this.CreateReportNrElement(es.ReportNr.ToString()),
                        this.CreateCompanyElement(),
                        this.CreateCompanyOrgNrElement(),
                        this.CreateLoginNameElement(es.LoginName),
                        new XElement("DateRegardName", this.GetLedgerDateRegardText(es)),
                        new XElement("SortOrderName", this.GetSupplierSortOrderText(es)),
                        new XElement("InvoiceSelectionName", this.GetLedgerInvoiceSelectionText(es)),
                        new XElement("DateInterval", this.GetDateIntervalText(es)),
                        new XElement("InvoiceInterval", this.GetLedgerInvoiceIntervalText(es)),
                        new XElement("SupplierInterval", this.GetLedgerActorIntervalText(es)),
                        new XElement("EntCurrencyName", currency?.Name ?? ""),
                        new XElement("EntCurrencyCode", currency?.Code ?? ""),
                       new XElement("Showpending", es.SL_ShowPendingPaymentsInReport));
            }
            else if (es.ReportTemplateType == SoeReportTemplateType.SupplierPaymentJournal)
            {
                //      Show pending payments in betaljournal(?)
                es.SL_ShowPendingPaymentsInReport = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.SupplierInvoiceReportShowPendingPayments, 0, es.ActorCompanyId, 0);
                return new XElement("ReportHeader",
                        this.CreateReportTitleElement(es.ReportName),
                        this.CreateReportDescriptionElement(es.ReportDescription),
                        this.CreateReportNrElement(es.ReportNr.ToString()),
                        this.CreateCompanyElement(),
                        this.CreateCompanyOrgNrElement(),
                        this.CreateLoginNameElement(es.LoginName),
                        new XElement("DateRegardName", this.GetLedgerDateRegardText(es)),
                        new XElement("SortOrderName", this.GetSupplierSortOrderText(es)),
                        new XElement("InvoiceSelectionName", this.GetLedgerInvoiceSelectionText(es)),
                        new XElement("DateInterval", this.GetDateIntervalText(es)),
                        new XElement("InvoiceInterval", this.GetLedgerInvoiceIntervalText(es)),
                        new XElement("SupplierInterval", this.GetLedgerActorIntervalText(es)),
                        new XElement("EntCurrencyName", currency?.Name ?? ""),
                        new XElement("EntCurrencyCode", currency?.Code ?? ""),
                        new XElement("Showpending", es.SL_ShowPendingPaymentsInReport));
            }
            else
            {
                return new XElement("ReportHeader",
                      this.CreateReportTitleElement(es.ReportName),
                      this.CreateReportDescriptionElement(es.ReportDescription),
                      this.CreateReportNrElement(es.ReportNr.ToString()),
                      this.CreateCompanyElement(),
                      this.CreateCompanyOrgNrElement(),
                      this.CreateLoginNameElement(es.LoginName),
                      new XElement("DateRegardName", this.GetLedgerDateRegardText(es)),
                      new XElement("SortOrderName", this.GetSupplierSortOrderText(es)),
                      new XElement("InvoiceSelectionName", this.GetLedgerInvoiceSelectionText(es)),
                      new XElement("DateInterval", this.GetDateIntervalText(es)),
                      new XElement("InvoiceInterval", this.GetLedgerInvoiceIntervalText(es)),
                      new XElement("SupplierInterval", this.GetLedgerActorIntervalText(es)),
                      new XElement("EntCurrencyName", currency?.Name ?? ""),
                      new XElement("EntCurrencyCode", currency?.Code ?? ""));
            }
        }

        protected XElement CreateCustomerReportHeaderElement(EvaluatedSelection es)
        {
            Currency currency = CountryCurrencyManager.GetCurrencyFromType(Company.ActorCompanyId, TermGroup_CurrencyType.EnterpriseCurrency);

            return new XElement("ReportHeader",
                    this.CreateReportTitleElement(es.ReportName),
                    this.CreateReportDescriptionElement(es.ReportDescription),
                    this.CreateReportNrElement(es.ReportNr.ToString()),
                    this.CreateCompanyElement(),
                    this.CreateCompanyOrgNrElement(),
                    this.CreateLoginNameElement(es.LoginName),
                    new XElement("DateRegardName", this.GetLedgerDateRegardText(es)),
                    new XElement("SortOrderName", this.GetCustomerSortOrderText(es)),
                    new XElement("InvoiceSelectionName", this.GetLedgerInvoiceSelectionText(es)),
                    new XElement("DateInterval", this.GetDateIntervalText(es)),
                    new XElement("InvoiceInterval", this.GetLedgerInvoiceIntervalText(es)),
                    new XElement("CustomerInterval", this.GetLedgerActorIntervalText(es)),
                    new XElement("EntCurrencyName", currency?.Name ?? ""),
                    new XElement("EntCurrencyCode", currency?.Code ?? ""));
        }

        protected XElement CreateInterestRateCalculationPageHeaderElement(EvaluatedSelection es, decimal interestPercent)
        {
            return new XElement("ReportHeader",
                this.CreateLoginNameElement(es.LoginName),
                new XElement("DateInterval", this.GetDateIntervalText(es)),
                this.CreateReportTitleElement(es.ReportName),
                this.CreateReportDescriptionElement(es.ReportDescription),
                this.CreateReportNrElement(es.ReportNr.ToString()),
                this.CreateCompanyElement(),
                this.CreateCompanyOrgNrElement(),
                new XElement("InterestRate", interestPercent));
        }

        protected XElement CreateProductReportHeaderElement(EvaluatedSelection es)
        {
            Currency currency = CountryCurrencyManager.GetCurrencyFromType(Company.ActorCompanyId, TermGroup_CurrencyType.EnterpriseCurrency);

            return new XElement("ReportHeader",
                    this.CreateReportTitleElement(es.ReportName),
                    this.CreateReportDescriptionElement(es.ReportDescription),
                    this.CreateReportNrElement(es.ReportNr.ToString()),
                    this.CreateCompanyElement(),
                    this.CreateCompanyOrgNrElement(),
                    this.CreateLoginNameElement(es.LoginName),
                    new XElement("DateRegardName", this.GetLedgerDateRegardText(es)),
                    new XElement("SortOrderName", this.GetCustomerSortOrderText(es)),
                    new XElement("InvoiceSelectionName", this.GetLedgerInvoiceSelectionText(es)),
                    new XElement("DateInterval", this.GetDateIntervalText(es)),
                    new XElement("InvoiceInterval", this.GetLedgerInvoiceIntervalText(es)),
                    new XElement("ProductInterval", this.GetLedgerActorIntervalText(es)),
                    new XElement("EntCurrencyName", currency?.Name ?? ""),
                    new XElement("EntCurrencyCode", currency?.Code ?? ""));
        }

        protected XElement CreateSEPAPaymentImportReportHeaderElement(EvaluatedSelection es)
        {
            return new XElement("ReportHeader",
                   this.CreateLoginNameElement(es.LoginName),
                    this.CreateReportTitleElement(es.ReportName),
                    this.CreateReportDescriptionElement(es.ReportDescription),
                    this.CreateReportNrElement(es.ReportNr.ToString()),
                   this.CreateCompanyElement(),
                   this.CreateCompanyOrgNrElement());
        }

        protected XElement CreateBillingReportHeaderElement(EvaluatedSelection es, ReportDataHistoryRepository repository = null)
        {
            #region Prereq

            int sortOrderTermGroup;
            switch (es.ReportTemplateType)
            {
                case SoeReportTemplateType.BillingContract:
                    sortOrderTermGroup = (int)TermGroup.ReportBillingOrderSortOrder;
                    break;
                case SoeReportTemplateType.BillingOffer:
                    sortOrderTermGroup = (int)TermGroup.ReportBillingOfferSortOrder;
                    break;
                case SoeReportTemplateType.BillingOrder:
                case SoeReportTemplateType.BillingOrderOverview:
                    sortOrderTermGroup = (int)TermGroup.ReportBillingOrderSortOrder;
                    break;
                case SoeReportTemplateType.BillingInvoice:
                case SoeReportTemplateType.BillingInvoiceInterest:
                case SoeReportTemplateType.BillingInvoiceReminder:
                    sortOrderTermGroup = (int)TermGroup.ReportBillingInvoiceSortOrder;
                    break;

                //should never end up in here
                default:
                    sortOrderTermGroup = (int)TermGroup.ReportBillingInvoiceSortOrder;
                    break;
            }

            //From current selection
            string accountYearInterval = GetAccountYearIntervalText(es);
            string accountPeriodInterval = GetAccountPeriodIntervalText(es);
            string sortOrderName = this.GetBillingInvoiceSortOrderText(es, sortOrderTermGroup);
            string invoiceInterval = this.GetBillingInvoiceIntervalText(es);
            string customerInterval = this.GetBillingCustomerIntervalText(es);
            string dateInterval = this.GetBillingDateIntervalText(es);
            string createdDateInterval = this.GetBillingCreatedDateIntervalText(es);
            string paymentDateInterval = this.GetBillingPaymentDateIntervalText(es);
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            string companyLogoPath = GetCompanyLogoFilePath(entitiesReadOnly, es.ActorCompanyId, false);

            //From current selection or repository if used
            string companyName = "", companyOrgNr = "", companyVatNr = "",
                   distributionAddress = "", distributionAddressCO = "", distributionPostalCode = "", distributionPostalAddress = "", distributionCountry = "",
                   boardHqPostalAddress = "", boardHqCountry = "",
                   email = "", phoneHome = "", phoneJob = "", phoneMobile = "", fax = "", webAddress = "",
                   bg = "", pg = "", bank = "", bic = "", sepa = "", bicNR = "", bicBIC = "";

            int paymentConditionId = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.CustomerDefaultPaymentConditionClaimAndInterest, 0, es.ActorCompanyId, 0);
            PaymentCondition paymentCondition = paymentConditionId != 0 ? PaymentManager.GetPaymentCondition(paymentConditionId, es.ActorCompanyId) : null;
            var extendedTimeRegistration = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.ProjectUseExtendedTimeRegistration, 0, es.ActorCompanyId, 0);
            var showStartStopInTimeReport = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.BillingShowStartStopInTimeReport, this.UserId, this.ActorCompanyId, 0, false);
            var additionalDiscountInUse = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.BillingUseAdditionalDiscount, this.UserId, this.ActorCompanyId, 0, false);

            #endregion
            XElement paymentInformationElement = new XElement("Paymentinformation");
            if (repository != null && repository.HasSavedHistory)
            {
                #region Repository

                //Company
                companyName = repository.GetHistoryValue(SoeReportDataHistoryHeadTag.BillingInvoice_ReportHeader, SoeReportDataHistoryTag.BillingInvoice_ReportHeader_CompanyName);
                companyOrgNr = repository.GetHistoryValue(SoeReportDataHistoryHeadTag.BillingInvoice_ReportHeader, SoeReportDataHistoryTag.BillingInvoice_ReportHeader_CompanyOrgnr);
                companyVatNr = repository.GetHistoryValue(SoeReportDataHistoryHeadTag.BillingInvoice_ReportHeader, SoeReportDataHistoryTag.BillingInvoice_ReportHeader_CompanyVatNr);
                if (string.IsNullOrEmpty(companyName) && !string.IsNullOrEmpty(companyOrgNr))
                {
                    //Addresses
                    distributionAddress = repository.GetHistoryValue(SoeReportDataHistoryHeadTag.BillingInvoice_ReportHeader, SoeReportDataHistoryTag.BillingInvoice_ReportHeader_CompanyDistributionAddress);
                    distributionAddressCO = repository.GetHistoryValue(SoeReportDataHistoryHeadTag.BillingInvoice_ReportHeader, SoeReportDataHistoryTag.BillingInvoice_ReportHeader_CompanyDistributionAddressCO);
                    distributionPostalCode = repository.GetHistoryValue(SoeReportDataHistoryHeadTag.BillingInvoice_ReportHeader, SoeReportDataHistoryTag.BillingInvoice_ReportHeader_CompanyDistributionPostalCode);
                    distributionPostalAddress = repository.GetHistoryValue(SoeReportDataHistoryHeadTag.BillingInvoice_ReportHeader, SoeReportDataHistoryTag.BillingInvoice_ReportHeader_CompanyDistributionPostalAddress);
                    distributionCountry = repository.GetHistoryValue(SoeReportDataHistoryHeadTag.BillingInvoice_ReportHeader, SoeReportDataHistoryTag.BillingInvoice_ReportHeader_CompanyDistributionCountry);
                    boardHqPostalAddress = repository.GetHistoryValue(SoeReportDataHistoryHeadTag.BillingInvoice_ReportHeader, SoeReportDataHistoryTag.BillingInvoice_ReportHeader_CompanyBoardHQPostalAddress);
                    boardHqCountry = repository.GetHistoryValue(SoeReportDataHistoryHeadTag.BillingInvoice_ReportHeader, SoeReportDataHistoryTag.BillingInvoice_ReportHeader_CompanyBoardHQCountry);

                    //ECom
                    email = repository.GetHistoryValue(SoeReportDataHistoryHeadTag.BillingInvoice_ReportHeader, SoeReportDataHistoryTag.BillingInvoice_ReportHeader_CompanyEmail);
                    phoneHome = repository.GetHistoryValue(SoeReportDataHistoryHeadTag.BillingInvoice_ReportHeader, SoeReportDataHistoryTag.BillingInvoice_ReportHeader_CompanyPhoneHome);
                    phoneJob = repository.GetHistoryValue(SoeReportDataHistoryHeadTag.BillingInvoice_ReportHeader, SoeReportDataHistoryTag.BillingInvoice_ReportHeader_CompanyPhoneJob);
                    phoneMobile = repository.GetHistoryValue(SoeReportDataHistoryHeadTag.BillingInvoice_ReportHeader, SoeReportDataHistoryTag.BillingInvoice_ReportHeader_CompanyPhoneMobile);
                    fax = repository.GetHistoryValue(SoeReportDataHistoryHeadTag.BillingInvoice_ReportHeader, SoeReportDataHistoryTag.BillingInvoice_ReportHeader_CompanyFax);
                    webAddress = repository.GetHistoryValue(SoeReportDataHistoryHeadTag.BillingInvoice_ReportHeader, SoeReportDataHistoryTag.BillingInvoice_ReportHeader_CompanyWebAddress);

                    //Payment
                    PaymentInformation paymentInformation = PaymentManager.GetPaymentInformationFromActor(Company.ActorCompanyId, true, false);
                    bg = repository.GetHistoryValue(SoeReportDataHistoryHeadTag.BillingInvoice_ReportHeader, SoeReportDataHistoryTag.BillingInvoice_ReportHeader_CompanyBg);
                    pg = repository.GetHistoryValue(SoeReportDataHistoryHeadTag.BillingInvoice_ReportHeader, SoeReportDataHistoryTag.BillingInvoice_ReportHeader_CompanyPg);
                    bank = repository.GetHistoryValue(SoeReportDataHistoryHeadTag.BillingInvoice_ReportHeader, SoeReportDataHistoryTag.BillingInvoice_ReportHeader_CompanyBank);
                    bic = repository.GetHistoryValue(SoeReportDataHistoryHeadTag.BillingInvoice_ReportHeader, SoeReportDataHistoryTag.BillingInvoice_ReportHeader_CompanyBic);
                    sepa = repository.GetHistoryValue(SoeReportDataHistoryHeadTag.BillingInvoice_ReportHeader, SoeReportDataHistoryTag.BillingInvoice_ReportHeader_CompanySepa);
                    bicNR = bic;
                    bicBIC = bic;
                    if (bic.Contains("/"))
                    {
                        bicBIC = bic.Split('/')[0];
                        bicNR = bic.Split('/')[1];

                    }
                    else
                    {
                        bicNR = PaymentManager.GetPaymentNr(paymentInformation, TermGroup_SysPaymentType.BIC);
                        bicBIC = PaymentManager.GetPaymentBIC(paymentInformation, TermGroup_SysPaymentType.BIC);
                    }
                    var rows = paymentInformation.ActivePaymentInformationRows.Where(i => i.ShownInInvoice == true).ToList();
                    foreach (var row in rows)
                    {
                        XElement paymentInformationRowElement = new XElement("PaymentinformationRow");
                        paymentInformationRowElement.Add(new XElement("BIC", row.BIC));
                        if (row.CurrencyId.HasValue)
                        {
                            var currency = CountryCurrencyManager.GetCurrencyWithCode((int)row.CurrencyId);
                            paymentInformationRowElement.Add(new XElement("CurrencyCode", currency?.Code ?? ""));
                        }
                        else
                        {
                            paymentInformationRowElement.Add(new XElement("CurrencyCode", ""));
                        }
                        paymentInformationRowElement.Add(new XElement("PaymentType", (TermGroup_SysPaymentType)row.SysPaymentTypeId));
                        paymentInformationRowElement.Add(new XElement("PaymentNumber", row.PaymentNr));

                        paymentInformationElement.Add(paymentInformationRowElement);
                    }
                }
                else
                {
                    //Mandatory information missing use current
                    companyVatNr = Company.VatNr;

                    Contact contact = ContactManager.GetContactFromActor(Company.ActorCompanyId);
                    List<ContactAddressRow> contactAddressRows = contact != null ? ContactManager.GetContactAddressRows(contact.ContactId) : null;
                    distributionAddress = contactAddressRows.GetContactAddressRowText(TermGroup_SysContactAddressType.Distribution, TermGroup_SysContactAddressRowType.Address);
                    distributionAddressCO = contactAddressRows.GetContactAddressRowText(TermGroup_SysContactAddressType.Distribution, TermGroup_SysContactAddressRowType.AddressCO);
                    distributionPostalCode = contactAddressRows.GetContactAddressRowText(TermGroup_SysContactAddressType.Distribution, TermGroup_SysContactAddressRowType.PostalCode);
                    distributionPostalAddress = contactAddressRows.GetContactAddressRowText(TermGroup_SysContactAddressType.Distribution, TermGroup_SysContactAddressRowType.PostalAddress);
                    distributionCountry = contactAddressRows.GetContactAddressRowText(TermGroup_SysContactAddressType.Distribution, TermGroup_SysContactAddressRowType.Country);
                    boardHqPostalAddress = contactAddressRows.GetContactAddressRowText(TermGroup_SysContactAddressType.BoardHQ, TermGroup_SysContactAddressRowType.PostalAddress);
                    boardHqCountry = contactAddressRows.GetContactAddressRowText(TermGroup_SysContactAddressType.BoardHQ, TermGroup_SysContactAddressRowType.Country);

                    List<ContactECom> contactEcoms = contact != null ? ContactManager.GetContactEComs(contact.ContactId) : null;
                    email = contactEcoms.GetEComText(TermGroup_SysContactEComType.Email);
                    phoneHome = contactEcoms.GetEComText(TermGroup_SysContactEComType.PhoneHome);
                    phoneJob = contactEcoms.GetEComText(TermGroup_SysContactEComType.PhoneJob);
                    phoneMobile = contactEcoms.GetEComText(TermGroup_SysContactEComType.PhoneMobile);
                    fax = contactEcoms.GetEComText(TermGroup_SysContactEComType.Fax);
                    webAddress = contactEcoms.GetEComText(TermGroup_SysContactEComType.Web);

                    PaymentInformation paymentInformation = PaymentManager.GetPaymentInformationFromActor(Company.ActorCompanyId, true, false);
                    if (paymentInformation == null)
                    {
                        throw new ActionFailedException(GetText(7742, "Betalningsuppgifter saknas på företaget"));
                    }

                    bg = PaymentManager.GetPaymentNr(paymentInformation, TermGroup_SysPaymentType.BG);
                    pg = PaymentManager.GetPaymentNr(paymentInformation, TermGroup_SysPaymentType.PG);
                    bank = PaymentManager.GetPaymentNr(paymentInformation, TermGroup_SysPaymentType.Bank);
                    bic = PaymentManager.GetPaymentNr(paymentInformation, TermGroup_SysPaymentType.BIC);
                    bicNR = PaymentManager.GetPaymentNr(paymentInformation, TermGroup_SysPaymentType.BIC);
                    bicBIC = PaymentManager.GetPaymentBIC(paymentInformation, TermGroup_SysPaymentType.BIC);
                    sepa = PaymentManager.GetPaymentNr(paymentInformation, TermGroup_SysPaymentType.SEPA);
                    if (bic.Contains("/"))
                    {
                        bicBIC = bic.Split('/')[0];
                        bicNR = bic.Split('/')[1];
                    }
                    else
                    {
                        bicNR = PaymentManager.GetPaymentNr(paymentInformation, TermGroup_SysPaymentType.BIC);
                        bicBIC = PaymentManager.GetPaymentBIC(paymentInformation, TermGroup_SysPaymentType.BIC);
                    }
                    var rows = paymentInformation.ActivePaymentInformationRows.Where(i => i.ShownInInvoice == true).ToList();
                    foreach (var row in rows)
                    {
                        XElement paymentInformationRowElement = new XElement("PaymentinformationRow");
                        paymentInformationRowElement.Add(new XElement("BIC", row.BIC));
                        if (row.CurrencyId.HasValue)
                        {
                            var currency = CountryCurrencyManager.GetCurrencyWithCode((int)row.CurrencyId);
                            paymentInformationRowElement.Add(new XElement("CurrencyCode", currency?.Code ?? ""));
                        }
                        else
                        {
                            paymentInformationRowElement.Add(new XElement("CurrencyCode", ""));
                        }
                        paymentInformationRowElement.Add(new XElement("PaymentType", (TermGroup_SysPaymentType)row.SysPaymentTypeId));
                        paymentInformationRowElement.Add(new XElement("PaymentNumber", row.PaymentNr));

                        paymentInformationElement.Add(paymentInformationRowElement);
                    }
                }
                #endregion
            }
            else
            {
                #region Current

                if (Company != null)
                {
                    companyName = Company.Name;
                    companyOrgNr = Company.OrgNr;
                    companyVatNr = Company.VatNr;

                    Contact contact = ContactManager.GetContactFromActor(Company.ActorCompanyId);
                    List<ContactAddressRow> contactAddressRows = contact != null ? ContactManager.GetContactAddressRows(contact.ContactId) : null;
                    distributionAddress = contactAddressRows.GetContactAddressRowText(TermGroup_SysContactAddressType.Distribution, TermGroup_SysContactAddressRowType.Address);
                    distributionAddressCO = contactAddressRows.GetContactAddressRowText(TermGroup_SysContactAddressType.Distribution, TermGroup_SysContactAddressRowType.AddressCO);
                    distributionPostalCode = contactAddressRows.GetContactAddressRowText(TermGroup_SysContactAddressType.Distribution, TermGroup_SysContactAddressRowType.PostalCode);
                    distributionPostalAddress = contactAddressRows.GetContactAddressRowText(TermGroup_SysContactAddressType.Distribution, TermGroup_SysContactAddressRowType.PostalAddress);
                    distributionCountry = contactAddressRows.GetContactAddressRowText(TermGroup_SysContactAddressType.Distribution, TermGroup_SysContactAddressRowType.Country);
                    boardHqPostalAddress = contactAddressRows.GetContactAddressRowText(TermGroup_SysContactAddressType.BoardHQ, TermGroup_SysContactAddressRowType.PostalAddress);
                    boardHqCountry = contactAddressRows.GetContactAddressRowText(TermGroup_SysContactAddressType.BoardHQ, TermGroup_SysContactAddressRowType.Country);

                    List<ContactECom> contactEcoms = contact != null ? ContactManager.GetContactEComs(contact.ContactId) : null;
                    email = contactEcoms.GetEComText(TermGroup_SysContactEComType.Email);
                    phoneHome = contactEcoms.GetEComText(TermGroup_SysContactEComType.PhoneHome);
                    phoneJob = contactEcoms.GetEComText(TermGroup_SysContactEComType.PhoneJob);
                    phoneMobile = contactEcoms.GetEComText(TermGroup_SysContactEComType.PhoneMobile);
                    fax = contactEcoms.GetEComText(TermGroup_SysContactEComType.Fax);
                    webAddress = contactEcoms.GetEComText(TermGroup_SysContactEComType.Web);

                    PaymentInformation paymentInformation = PaymentManager.GetPaymentInformationFromActor(Company.ActorCompanyId, true, false);
                    bg = PaymentManager.GetPaymentNr(paymentInformation, TermGroup_SysPaymentType.BG);
                    pg = PaymentManager.GetPaymentNr(paymentInformation, TermGroup_SysPaymentType.PG);
                    bank = PaymentManager.GetPaymentNr(paymentInformation, TermGroup_SysPaymentType.Bank);
                    bic = PaymentManager.GetPaymentNr(paymentInformation, TermGroup_SysPaymentType.BIC);
                    if (bic.Contains("/"))
                    {
                        bicBIC = bic.Split('/')[0];
                        bicNR = bic.Split('/')[1];

                    }
                    else
                    {
                        bicNR = PaymentManager.GetPaymentNr(paymentInformation, TermGroup_SysPaymentType.BIC);
                        bicBIC = PaymentManager.GetPaymentBIC(paymentInformation, TermGroup_SysPaymentType.BIC);
                    }

                    sepa = PaymentManager.GetPaymentNr(paymentInformation, TermGroup_SysPaymentType.SEPA);

                    if (paymentInformation != null)
                    {
                        var rows = paymentInformation.ActivePaymentInformationRows.Where(i => i.ShownInInvoice == true).ToList();
                        foreach (var row in rows)
                        {
                            XElement paymentInformationRowElement = new XElement("PaymentinformationRow");
                            paymentInformationRowElement.Add(new XElement("BIC", row.BIC));
                            if (row.CurrencyId.HasValue)
                            {
                                var currency = CountryCurrencyManager.GetCurrencyWithCode((int)row.CurrencyId);
                                paymentInformationRowElement.Add(new XElement("CurrencyCode", currency?.Code ?? ""));
                            }
                            else
                            {
                                paymentInformationRowElement.Add(new XElement("CurrencyCode", ""));
                            }
                            paymentInformationRowElement.Add(new XElement("PaymentType", (TermGroup_SysPaymentType)row.SysPaymentTypeId));
                            paymentInformationRowElement.Add(new XElement("PaymentNumber", row.PaymentNr));
                            paymentInformationElement.Add(paymentInformationRowElement);
                        }
                    }
                }


                #endregion

                #region Add to Repository

                if (repository != null && repository.HasActivatedHistory)
                {
                    //Company
                    repository.AddHistory(SoeReportDataHistoryHeadTag.BillingInvoice_ReportHeader, SoeReportDataHistoryTag.BillingInvoice_ReportHeader_CompanyName, companyName);
                    repository.AddHistory(SoeReportDataHistoryHeadTag.BillingInvoice_ReportHeader, SoeReportDataHistoryTag.BillingInvoice_ReportHeader_CompanyOrgnr, companyOrgNr);
                    repository.AddHistory(SoeReportDataHistoryHeadTag.BillingInvoice_ReportHeader, SoeReportDataHistoryTag.BillingInvoice_ReportHeader_CompanyVatNr, companyVatNr);

                    //Addresses
                    repository.AddHistory(SoeReportDataHistoryHeadTag.BillingInvoice_ReportHeader, SoeReportDataHistoryTag.BillingInvoice_ReportHeader_CompanyDistributionAddress, distributionAddress);
                    repository.AddHistory(SoeReportDataHistoryHeadTag.BillingInvoice_ReportHeader, SoeReportDataHistoryTag.BillingInvoice_ReportHeader_CompanyDistributionAddressCO, distributionAddressCO);
                    repository.AddHistory(SoeReportDataHistoryHeadTag.BillingInvoice_ReportHeader, SoeReportDataHistoryTag.BillingInvoice_ReportHeader_CompanyDistributionPostalCode, distributionPostalCode);
                    repository.AddHistory(SoeReportDataHistoryHeadTag.BillingInvoice_ReportHeader, SoeReportDataHistoryTag.BillingInvoice_ReportHeader_CompanyDistributionPostalAddress, distributionPostalAddress);
                    repository.AddHistory(SoeReportDataHistoryHeadTag.BillingInvoice_ReportHeader, SoeReportDataHistoryTag.BillingInvoice_ReportHeader_CompanyDistributionCountry, distributionCountry);
                    repository.AddHistory(SoeReportDataHistoryHeadTag.BillingInvoice_ReportHeader, SoeReportDataHistoryTag.BillingInvoice_ReportHeader_CompanyBoardHQPostalAddress, boardHqPostalAddress);
                    repository.AddHistory(SoeReportDataHistoryHeadTag.BillingInvoice_ReportHeader, SoeReportDataHistoryTag.BillingInvoice_ReportHeader_CompanyBoardHQCountry, boardHqCountry);

                    //Ecom
                    repository.AddHistory(SoeReportDataHistoryHeadTag.BillingInvoice_ReportHeader, SoeReportDataHistoryTag.BillingInvoice_ReportHeader_CompanyEmail, email);
                    repository.AddHistory(SoeReportDataHistoryHeadTag.BillingInvoice_ReportHeader, SoeReportDataHistoryTag.BillingInvoice_ReportHeader_CompanyPhoneHome, phoneHome);
                    repository.AddHistory(SoeReportDataHistoryHeadTag.BillingInvoice_ReportHeader, SoeReportDataHistoryTag.BillingInvoice_ReportHeader_CompanyPhoneJob, phoneJob);
                    repository.AddHistory(SoeReportDataHistoryHeadTag.BillingInvoice_ReportHeader, SoeReportDataHistoryTag.BillingInvoice_ReportHeader_CompanyPhoneMobile, phoneMobile);
                    repository.AddHistory(SoeReportDataHistoryHeadTag.BillingInvoice_ReportHeader, SoeReportDataHistoryTag.BillingInvoice_ReportHeader_CompanyFax, fax);
                    repository.AddHistory(SoeReportDataHistoryHeadTag.BillingInvoice_ReportHeader, SoeReportDataHistoryTag.BillingInvoice_ReportHeader_CompanyWebAddress, webAddress);

                    //Payment
                    repository.AddHistory(SoeReportDataHistoryHeadTag.BillingInvoice_ReportHeader, SoeReportDataHistoryTag.BillingInvoice_ReportHeader_CompanyBg, bg);
                    repository.AddHistory(SoeReportDataHistoryHeadTag.BillingInvoice_ReportHeader, SoeReportDataHistoryTag.BillingInvoice_ReportHeader_CompanyPg, pg);
                    repository.AddHistory(SoeReportDataHistoryHeadTag.BillingInvoice_ReportHeader, SoeReportDataHistoryTag.BillingInvoice_ReportHeader_CompanyBank, bank);
                    repository.AddHistory(SoeReportDataHistoryHeadTag.BillingInvoice_ReportHeader, SoeReportDataHistoryTag.BillingInvoice_ReportHeader_CompanyBic, bic);
                    repository.AddHistory(SoeReportDataHistoryHeadTag.BillingInvoice_ReportHeader, SoeReportDataHistoryTag.BillingInvoice_ReportHeader_CompanyBicNR, bicNR);
                    repository.AddHistory(SoeReportDataHistoryHeadTag.BillingInvoice_ReportHeader, SoeReportDataHistoryTag.BillingInvoice_ReportHeader_CompanyBicBIC, bicBIC);
                    repository.AddHistory(SoeReportDataHistoryHeadTag.BillingInvoice_ReportHeader, SoeReportDataHistoryTag.BillingInvoice_ReportHeader_CompanySepa, sepa);
                }

                #endregion
            }

            return new XElement("ReportHeader",
                   this.CreateReportTitleElement(es.ReportName),
                   this.CreateReportDescriptionElement(es.ReportDescription),
                   this.CreateReportNrElement(es.ReportNr.ToString()),
                   this.CreateCompanyElement(),
                   this.CreateCompanyOrgNrElement(),
                   new XElement("AccountYear", accountYearInterval),
                   new XElement("AccountPeriod", accountPeriodInterval),
                   this.CreateLoginNameElement(es.LoginName),
                   new XElement("SortOrderName", sortOrderName),
                   new XElement("InvoiceInterval", invoiceInterval),
                   new XElement("CustomerInterval", customerInterval),
                   new XElement("DateInterval", dateInterval),
                   new XElement("CreatedDateInterval", createdDateInterval),
                   new XElement("CompanyVatNr", companyVatNr),
                   new XElement("CompanyAddress", distributionAddress),
                   new XElement("CompanyAddressCO", distributionAddressCO),
                   new XElement("CompanyPostalCode", distributionPostalCode),
                   new XElement("CompanyPostalAddress", distributionPostalAddress),
                   new XElement("CompanyCountry", distributionCountry),
                   new XElement("CompanyBoardHQPostalAddress", boardHqPostalAddress),
                   new XElement("CompanyBoardHQCountry", boardHqCountry),
                   new XElement("CompanyEmail", email),
                   new XElement("CompanyPhoneHome", phoneHome),
                   new XElement("CompanyPhoneWork", phoneJob),
                   new XElement("CompanyPhoneMobile", phoneMobile),
                   new XElement("CompanyFax", fax),
                   new XElement("CompanyWebAddress", webAddress),
                   new XElement("CompanyBg", bg),
                   new XElement("CompanyPg", pg),
                   new XElement("CompanyBank", bank),
                   new XElement("CompanyBic", bic),
                   new XElement("CompanyBicNR", bicNR),
                   new XElement("CompanyBicBIC", bicBIC),
                   new XElement("CompanySepa", sepa),
                   new XElement("CompanyLogo", companyLogoPath),
                   new XElement("ReminderPaymentCondition", paymentCondition != null ? paymentCondition.Name : ""),
                   new XElement("ExtendedTimeRegistration", extendedTimeRegistration),
                   new XElement("ShowStartStop", showStartStopInTimeReport),
                   new XElement("PaymentDateInterval", paymentDateInterval),
                   new XElement("AdditionalDiscountInUse", additionalDiscountInUse),
                   paymentInformationElement
                   );
        }

        protected XElement CreateChecklistReportHeaderElement(EvaluatedSelection es)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            string companyLogoPath = GetCompanyLogoFilePath(entitiesReadOnly, ActorCompanyId, false);
            return new XElement("ReportHeader",
                    this.CreateReportTitleElement(es.ReportName),
                    this.CreateReportDescriptionElement(es.ReportDescription),
                    this.CreateReportNrElement(es.ReportNr.ToString()),
                    this.CreateCompanyElement(),
                    this.CreateCompanyOrgNrElement(),
                    this.CreateLoginNameElement(es.LoginName),
                    new XElement("CompanyLogo", companyLogoPath));
        }

        protected XElement CreateProjectTransactionsReportHeaderElement(CreateReportResult reportResult, BillingReportParamsDTO reportParams)
        {
            return new XElement("ReportHeader",
                    this.CreateReportTitleElement(reportResult.ReportName),
                    this.CreateReportDescriptionElement(reportResult.ReportDescription),
                    this.CreateReportNrElement(reportResult.ReportNr.ToString()),
                    this.CreateCompanyElement(),
                    this.CreateCompanyOrgNrElement(),
                    this.CreateLoginNameElement(reportResult.LoginName),
                    new XElement("DateFrom", reportParams.SP_InvoiceTransactionDateFrom.HasValue ? ((DateTime)reportParams.SP_InvoiceTransactionDateFrom).ToShortDateString() : string.Empty),
                    new XElement("DateTo", reportParams.SP_InvoiceTransactionDateTo.HasValue ? ((DateTime)reportParams.SP_InvoiceTransactionDateTo).ToShortDateString() : string.Empty),
                    new XElement("PayrollDateFrom", reportParams.SP_PayrollTransactionDateFrom.HasValue ? ((DateTime)reportParams.SP_PayrollTransactionDateFrom).ToShortDateString() : string.Empty),
                    new XElement("PayrollDateTo", reportParams.SP_PayrollTransactionDateTo.HasValue ? ((DateTime)reportParams.SP_PayrollTransactionDateTo).ToShortDateString() : string.Empty),
                    new XElement("IncludeChildProjects", reportParams.SP_IncludeChildProjects),
                    new XElement("ExcludeInternalOrders", reportParams.SP_ExcludeInternalOrders),
                    new XElement("ExportType", reportResult.ExportType)
                    );

        }

        protected XElement CreateProjectTransactionsReportHeaderElement(EvaluatedSelection es)
        {
            return new XElement("ReportHeader",
                    this.CreateReportTitleElement(es.ReportName),
                    this.CreateReportDescriptionElement(es.ReportDescription),
                    this.CreateReportNrElement(es.ReportNr.ToString()),
                    this.CreateCompanyElement(),
                    this.CreateCompanyOrgNrElement(),
                    this.CreateLoginNameElement(es.LoginName),
                    new XElement("DateFrom", es.SP_InvoiceTransactionDateFrom.HasValue ? ((DateTime)es.SP_InvoiceTransactionDateFrom).ToShortDateString() : string.Empty),
                    new XElement("DateTo", es.SP_InvoiceTransactionDateTo.HasValue ? ((DateTime)es.SP_InvoiceTransactionDateTo).ToShortDateString() : string.Empty),
                    new XElement("PayrollDateFrom", es.SP_PayrollTransactionDateFrom.HasValue ? ((DateTime)es.SP_PayrollTransactionDateFrom).ToShortDateString() : string.Empty),
                    new XElement("PayrollDateTo", es.SP_PayrollTransactionDateTo.HasValue ? ((DateTime)es.SP_PayrollTransactionDateTo).ToShortDateString() : string.Empty),
                    new XElement("IncludeChildProjects", es.SP_IncludeChildProjects));

        }

        protected XElement CreateProjectReportHeaderElement(EvaluatedSelection es)
        {
            return new XElement("ReportHeader",
                    this.CreateReportTitleElement(es.ReportName),
                    this.CreateReportDescriptionElement(es.ReportDescription),
                    this.CreateReportNrElement(es.ReportNr.ToString()),
                    this.CreateCompanyElement(),
                    this.CreateCompanyOrgNrElement(),
                    this.CreateLoginNameElement(es.LoginName));
        }

        protected XElement CreateStockAndProductReportHeaderElement(EvaluatedSelection es)
        {
            string accountYearInterval = GetAccountYearIntervalText(es);
            string accountPeriodInterval = GetAccountPeriodIntervalText(es);
            string sortOrderName = GetBillingInvoiceSortOrderText(es, (int)TermGroup.ReportBillingStockSortOrder);

            return new XElement("ReportHeader",
                    this.CreateReportTitleElement(es.ReportName),
                    this.CreateReportDescriptionElement(es.ReportDescription),
                    this.CreateReportNrElement(es.ReportNr.ToString()),
                    this.CreateCompanyElement(),
                    this.CreateCompanyOrgNrElement(),
                    new XElement("AccountYear", accountYearInterval),
                    new XElement("AccountPeriod", accountPeriodInterval),
                    this.CreateLoginNameElement(es.LoginName),
                    new XElement("SortOrder", es.SB_SortOrder),
                    new XElement("SortOrderName", sortOrderName),
                    new XElement("StockInterval", CreateStockIntervalText(es)),
                    new XElement("StockShelfInterval", CreateStockShelfIntervalText(es)),
                    new XElement("StockProductInterval", CreateStockProductIntervalText(es)),
                    new XElement("DateInterval", this.GetDateIntervalText(es))
                    );
        }

        #endregion

        #region HR

        protected List<XElement> CreateTimeEmployeeScheduleCommmonXML(
            CompEntities entities,
            ref int weekXmlId,
            CreateReportResult reportResult,
            Employee employee,
            DateTime startDate,
            DateTime stopDate,
            List<string> weekNrs,
            List<int> shiftTypeIds,
            int? timeScheduleScenarioHeadId,
            bool onlyActive = true,
            bool returnWeek = false,
            bool loadGrossNetCost = false,
            bool doCalculateEmploymentTaxAndSupplementChargeCost = false,
            string hiddenLink = null,
            List<AccountInternalDTO> validAccountInternals = null,
            List<HolidayDTO> companyHolidays = null,
            List<AccountDimDTO> companyAccountDims = null,
            TermGroup_EmployeeSelectionAccountingType employeeSelectionAccountingType = TermGroup_EmployeeSelectionAccountingType.EmployeeCategory,
            List<AttestEmployeeDayDTO> employeeDayItem = null,
            List<EmployeeAccount> employeeAccounts = null,
            List<int> selectionAccountIds = null,
            bool excludeAbsence = false)
        {
            const int MAX_SHIFTS = 15;
            List<XElement> weekElements = new List<XElement>();
            List<XElement> dayElements = new List<XElement>();

            List<AttestEmployeeDayDTO> items;
            if (!employeeDayItem.IsNullOrEmpty())
                items = employeeDayItem;
            else
            {
                var input = GetAttestEmployeeInput.CreateAttestInputForWeb(reportResult.ActorCompanyId, reportResult.UserId, reportResult.RoleId, employee.EmployeeId, startDate, stopDate, null, null, loadGrossNetCost.ToLoadType(InputLoadType.GrossNetCost), InputLoadType.Shifts);
                input.SetOptionalParameters(companyHolidays, companyAccountDims, doGetOnlyActive: onlyActive, doGetHidden: true, doCalculateEmploymentTaxAndSupplementChargeCost: doCalculateEmploymentTaxAndSupplementChargeCost, filterShiftTypeIds: !shiftTypeIds.IsNullOrEmpty() ? shiftTypeIds : null, timeScheduleScenarioHeadId: timeScheduleScenarioHeadId, validAccountInternals: validAccountInternals, employeeSelectionAccountingType: employeeSelectionAccountingType);

                items = TimeTreeAttestManager.GetAttestEmployeeDays(entities, input);
            }

            if (!items.IsNullOrEmpty())
            {
                var employeeAccountsOnEmployee = employeeAccounts.IsNullOrEmpty() ? new List<EmployeeAccount>() : employeeAccounts.Where(w => w.EmployeeId == employee.EmployeeId).ToList();
                List<int> hiddenEmployeeShiftTypeIds = employee.Hidden ? TimeScheduleManager.GetShiftTypeIdsForUsersCategories(entities, reportResult.ActorCompanyId, reportResult.UserId, 0, true, false) : new List<int>();
                bool isHidden = !hiddenLink.IsNullOrEmpty();
                List<AttestEmployeeDayDTO> usedAttestEmployeeDays = new List<AttestEmployeeDayDTO>();
                int numberOfWeeks = 1;

                foreach (string weekNr in weekNrs)
                {
                    #region Week

                    var weekDayElements = new List<XElement>();

                    int dayXmlId = 1;
                    int employeeWeekScheduleTimeTotal = 0;

                    var itemsInWeek = items.Where(w => !usedAttestEmployeeDays.Contains(w) && DateTime.Parse(weekNr.Split('_')[2]) == CalendarUtility.GetBeginningOfWeek(w.Date)).ToList();
                    usedAttestEmployeeDays.AddRange(itemsInWeek);
                    if (numberOfWeeks > 1 && !itemsInWeek.Any())
                        continue;

                    foreach (var day in itemsInWeek.OrderBy(i => i.Date))
                    {
                        #region Day

                        List<AttestEmployeeDayShiftDTO> shifts = new List<AttestEmployeeDayShiftDTO>();
                        if (day.Shifts.IsNullOrEmpty() && day.IsWholedayAbsence)
                        {
                            shifts.Add(
                                new AttestEmployeeDayShiftDTO()
                                {
                                    EmployeeId = employee.EmployeeId,
                                    ShiftTypeName = day.AttestPayrollTransactions.FirstOrDefault()?.PayrollProductName,
                                    TimeDeviationCauseId = day.WholedayAbsenseTimeDeviationCauseFromTimeBlock,
                                    TimeDeviationCauseName = day.AttestPayrollTransactions.FirstOrDefault()?.PayrollProductName,
                                    StartTime = CalendarUtility.DATETIME_DEFAULT,
                                    StopTime = CalendarUtility.GetEndOfDay(CalendarUtility.DATETIME_DEFAULT),
                                    Link = Guid.NewGuid().ToString(),
                                    Type = TermGroup_TimeScheduleTemplateBlockType.Schedule,
                                    Description = String.Empty
                                }
                            );
                        }
                        else if (day.IsScheduleZeroDay && day.HasDeviations && !day.Shifts.IsNullOrEmpty() && !day.HasSchedule)
                        {
                            foreach (AttestEmployeeDayShiftDTO shift in day.Shifts)
                            {
                                shift.StartTime = CalendarUtility.DATETIME_DEFAULT;
                                shift.StopTime = CalendarUtility.GetEndOfDay(CalendarUtility.DATETIME_DEFAULT);
                                if (shift.ShiftTypeName.IsNullOrEmpty())
                                    shift.ShiftTypeName = shift.TimeDeviationCauseName;
                            }
                            shifts = day.Shifts;
                        }
                        else
                            shifts = day.Shifts;

                        if (shifts.Any() && employee.Hidden && hiddenEmployeeShiftTypeIds.Any())
                            shifts = shifts.Where(tb => hiddenEmployeeShiftTypeIds.Contains(tb.ShiftTypeId)).ToList();

                        if (shifts.Any() && hiddenLink.Contains("#"))
                        {
                            var listofHidden = hiddenLink.Split('#').Where(w => !string.IsNullOrEmpty(w)).ToList();
                            shifts = shifts.Where(b => listofHidden.Contains(b.Link.ToString())).ToList();
                        }
                        else if (shifts.Any() && isHidden)
                            shifts = shifts.Where(b => b.Link.ToString() == hiddenLink).ToList();
                        else
                            shifts = shifts.Where(b => b.Type != TermGroup_TimeScheduleTemplateBlockType.OnDuty).ToList();

                        if (isHidden && !employee.Hidden)
                            shifts = shifts.Where(s => s.Type == TermGroup_TimeScheduleTemplateBlockType.OnDuty).ToList();

                        if (!shifts.Any())
                            continue;

                        List<XElement> shiftElements = new List<XElement>();
                        Employment employment = employee.GetEmployment(day.Date);
                        bool addedToWeekTotal = false;
                        bool onDuty = shifts.Any(s => s.Type == TermGroup_TimeScheduleTemplateBlockType.OnDuty);

                        if (onDuty)
                            weekXmlId = 1;

                        day.RemoveBreaksOutsideSchedule(shifts, out int scheduleBreakMinutes);
                        if (!isHidden)
                        {
                            employeeWeekScheduleTimeTotal += Convert.ToInt32(day.ScheduleTime.TotalMinutes) - (day.TimeScheduleTypeFactorMinutes);
                            addedToWeekTotal = true;
                        }
                        else
                            scheduleBreakMinutes = 0;

                        bool hasAbsenceTransactions = day.AttestPayrollTransactions?.Any(a => a.IsAbsence) ?? false;
                        string absencePayrollProductName = hasAbsenceTransactions && day.AttestPayrollTransactions.All(s => s.IsAbsence()) ? day.AttestPayrollTransactions.FirstOrDefault().PayrollProductName : string.Empty;
                        string absencePayrollProductShortName = hasAbsenceTransactions && day.AttestPayrollTransactions.All(s => s.IsAbsence()) ? day.AttestPayrollTransactions.FirstOrDefault().PayrollProductShortName : string.Empty;
                        int scheduleTime = 0;
                        int occupiedTime = 0;

                        DateTime scheduleBreak1Start = CalendarUtility.DATETIME_DEFAULT;
                        DateTime scheduleBreak2Start = CalendarUtility.DATETIME_DEFAULT;
                        DateTime scheduleBreak3Start = CalendarUtility.DATETIME_DEFAULT;
                        DateTime scheduleBreak4Start = CalendarUtility.DATETIME_DEFAULT;
                        DateTime scheduleStartTime = day.ScheduleStartTime;
                        DateTime scheduleStopTime = day.ScheduleStopTime;
                        int scheduleBreak1Minutes = 0;
                        int scheduleBreak2Minutes = 0;
                        int scheduleBreak3Minutes = 0;
                        int scheduleBreak4Minutes = 0;

                        if (day.IsWholedayAbsence && day.Shifts.IsNullOrEmpty())
                        {
                            scheduleStartTime = DateTime.Parse("1900-01-01 00:00:00");
                            scheduleStopTime = DateTime.Parse("1900-01-01 23:59:59");
                        }

                        if (isHidden)
                        {

                            scheduleTime = Convert.ToInt32(shifts.Sum(s => (s.StopTime - s.StartTime).TotalMinutes)) - (onDuty ? 0 : Convert.ToInt32(shifts.Sum(s => s.Break1Minutes + s.Break2Minutes + s.Break3Minutes + s.Break4Minutes)));
                            scheduleStartTime = shifts.OrderBy(o => o.StopTime).First().StartTime;
                            scheduleStopTime = shifts.OrderBy(o => o.StopTime).Last().StopTime;

                            List<Tuple<DateTime, DateTime>> breakTuples = new List<Tuple<DateTime, DateTime>>();
                            foreach (var item in shifts.Where(s => s.Type != TermGroup_TimeScheduleTemplateBlockType.OnDuty))
                            {
                                if (item.Break1StartTime != CalendarUtility.DATETIME_DEFAULT && !breakTuples.Any(a => a.Item1 == item.Break1StartTime))
                                    breakTuples.Add(Tuple.Create(item.Break1StartTime, item.Break1StartTime.AddMinutes(item.Break1Minutes)));

                                if (item.Break2StartTime != CalendarUtility.DATETIME_DEFAULT && !breakTuples.Any(a => a.Item1 == item.Break2StartTime))
                                    breakTuples.Add(Tuple.Create(item.Break2StartTime, item.Break2StartTime.AddMinutes(item.Break2Minutes)));

                                if (item.Break3StartTime != CalendarUtility.DATETIME_DEFAULT && !breakTuples.Any(a => a.Item1 == item.Break3StartTime))
                                    breakTuples.Add(Tuple.Create(item.Break3StartTime, item.Break3StartTime.AddMinutes(item.Break3Minutes)));

                                if (item.Break4StartTime != CalendarUtility.DATETIME_DEFAULT && !breakTuples.Any(a => a.Item1 == item.Break4StartTime))
                                    breakTuples.Add(Tuple.Create(item.Break4StartTime, item.Break4StartTime.AddMinutes(item.Break4Minutes)));
                            }

                            int breakNumber = 1;

                            foreach (var b in breakTuples)
                            {
                                if (b.Item1 != DateTime.MinValue && b.Item1 != CalendarUtility.DATETIME_DEFAULT)
                                {
                                    var startTime = CalendarUtility.MergeDateAndTime(CalendarUtility.DATETIME_DEFAULT, b.Item1);

                                    if (breakNumber == 1)
                                    {
                                        scheduleBreak1Start = startTime;
                                        scheduleBreak1Minutes = Convert.ToInt32((b.Item2 - b.Item1).TotalMinutes);
                                        scheduleBreakMinutes += scheduleBreak1Minutes;
                                    }
                                    else if (breakNumber == 2)
                                    {
                                        scheduleBreak2Start = startTime;
                                        scheduleBreak2Minutes = Convert.ToInt32((b.Item2 - b.Item1).TotalMinutes);
                                        scheduleBreakMinutes += scheduleBreak2Minutes;
                                    }
                                    else if (breakNumber == 3)
                                    {
                                        scheduleBreak3Start = startTime;
                                        scheduleBreak3Minutes = Convert.ToInt32((b.Item2 - b.Item1).TotalMinutes);
                                        scheduleBreakMinutes += scheduleBreak3Minutes;
                                    }
                                    else if (breakNumber == 4)
                                    {
                                        scheduleBreak4Start = startTime;
                                        scheduleBreak4Minutes = Convert.ToInt32((b.Item2 - b.Item1).TotalMinutes);
                                        scheduleBreakMinutes += scheduleBreak4Minutes;
                                    }
                                }

                                breakNumber++;
                            }

                            if (hiddenLink.Contains("#"))
                            {
                                scheduleBreakMinutes = 0;
                                foreach (var linkGroup in shifts.GroupBy(g => g.Link))
                                {
                                    List<DateTime> startTimes = new List<DateTime>();

                                    foreach (var sh in linkGroup)
                                    {
                                        if (!startTimes.Contains(sh.Break1StartTime))
                                        {
                                            startTimes.Add(sh.Break1StartTime);
                                            scheduleBreakMinutes += sh.Break1Minutes;
                                        }

                                        if (!startTimes.Contains(sh.Break2StartTime))
                                        {
                                            startTimes.Add(sh.Break2StartTime);
                                            scheduleBreakMinutes += sh.Break2Minutes;
                                        }

                                        if (!startTimes.Contains(sh.Break3StartTime))
                                        {
                                            startTimes.Add(sh.Break3StartTime);
                                            scheduleBreakMinutes += sh.Break3Minutes;
                                        }

                                        if (!startTimes.Contains(sh.Break4StartTime))
                                        {
                                            startTimes.Add(sh.Break4StartTime);
                                            scheduleBreakMinutes += sh.Break4Minutes;
                                        }
                                    }
                                }

                                scheduleTime = Convert.ToInt32(shifts.Sum(s => (s.StopTime - s.StartTime).TotalMinutes)) - scheduleBreakMinutes;

                            }

                        }
                        else
                        {
                            scheduleTime = Convert.ToInt32(day.ScheduleTime.TotalMinutes) - day.TimeScheduleTypeFactorMinutes;
                            occupiedTime = Convert.ToInt32(day.OccupiedTime.TotalMinutes);
                            scheduleBreak1Start = day.ScheduleBreak1Start;
                            scheduleBreak2Start = day.ScheduleBreak2Start;
                            scheduleBreak3Start = day.ScheduleBreak3Start;
                            scheduleBreak4Start = day.ScheduleBreak4Start;
                            scheduleBreak1Minutes = day.ScheduleBreak1Minutes;
                            scheduleBreak2Minutes = day.ScheduleBreak2Minutes;
                            scheduleBreak3Minutes = day.ScheduleBreak3Minutes;
                            scheduleBreak4Minutes = day.ScheduleBreak4Minutes;
                        }

                        if (!addedToWeekTotal)
                            employeeWeekScheduleTimeTotal += scheduleTime;

                        #endregion

                        #region Day Element

                        if (excludeAbsence && day.IsWholedayAbsence)
                            continue;

                        XElement dayElement = new XElement("Day",
                            new XAttribute("id", dayXmlId),
                            new XElement("IsTemplate", 0),
                            new XElement("TemplateNbrOfWeeks", 0),
                            new XElement("TemplateDayNumber", 0),
                            new XElement("DayNr", (int)day.Date.DayOfWeek),
                            new XElement("IsZeroScheduleDay", day.IsScheduleZeroDay.ToInt()),
                            new XElement("IsAbsenceDay", day.IsWholedayAbsence.ToInt()),
                            new XElement("IsPreliminary", day.IsPrel.ToInt()),
                            new XElement("AbsencePayrollProductName", absencePayrollProductName),
                            new XElement("AbsencePayrollProductShortName", absencePayrollProductShortName),
                            new XElement("ScheduleStartTime", CalendarUtility.GetHoursAndMinutesString(scheduleStartTime)),
                            new XElement("ScheduleStopTime", CalendarUtility.GetHoursAndMinutesString(scheduleStopTime)),
                            new XElement("ScheduleTime", scheduleTime),
                            new XElement("OccupiedTime", occupiedTime),
                            new XElement("ScheduleTypeFactorMinutes", day.TimeScheduleTypeFactorMinutes),
                            new XElement("ScheduleBreakTime", scheduleBreakMinutes),
                            new XElement("ScheduleBreak1Start", scheduleBreak1Start),
                            new XElement("ScheduleBreak1Minutes", scheduleBreak1Minutes),
                            new XElement("ScheduleBreak2Start", scheduleBreak2Start),
                            new XElement("ScheduleBreak2Minutes", scheduleBreak2Minutes),
                            new XElement("ScheduleBreak3Start", scheduleBreak3Start),
                            new XElement("ScheduleBreak3Minutes", scheduleBreak3Minutes),
                            new XElement("ScheduleBreak4Start", scheduleBreak4Start),
                            new XElement("ScheduleBreak4Minutes", scheduleBreak4Minutes),
                            new XElement("ScheduleDate", day.Date),
                            new XElement("ScheduleGrossTimeMinutes", !onDuty ? day.GrossNetCosts.GetGrossTimeMinutes() : 0),
                            new XElement("ScheduleNetTimeMinutes", !onDuty ? day.GrossNetCosts.GetNetTimeMinutes() : 0),
                            new XElement("ScheduleEmploymentTaxCost", !onDuty ? day.GrossNetCosts.GetEmploymentTaxCost() : 0),
                            new XElement("ScheduleSupplementChargeCost", !onDuty ? day.GrossNetCosts.GetSupplementChargeCost() : 0),
                            new XElement("ScheduleTotalCost", !onDuty ? day.GrossNetCosts.GetTotalCostIncludingEmploymentTaxAndSupplementCharge() : 0));
                        if (!returnWeek)
                        {
                            dayElement.Add(
                                new XElement("ScheduleWeekNr", weekNr),
                                new XElement("EmployeeWeekTimeTotal", employeeWeekScheduleTimeTotal));
                        }
                        dayElement.Add(
                            new XElement("EmploymentWorkTimeWeek", employment?.GetWorkTimeWeek(day.Date) ?? 0),
                            new XElement("EmploymentPercent", employment?.GetPercent(day.Date) ?? 0));

                        #endregion

                        int shiftXmlId = 1;
                        foreach (var shift in shifts.Where(b => b.StartTime < b.StopTime).OrderBy(b => b.Link).ThenBy(b => b.StartTime).ToList())
                        {
                            #region Shift

                            DateTime startTime = CalendarUtility.GetScheduleTime(shift.StartTime, shift.StartTime.Date, shift.StartTime.Date);
                            DateTime stopTime = CalendarUtility.GetScheduleTime(shift.StopTime, shift.StartTime.Date, shift.StopTime.Date);
                            string shiftName = shift.ShiftTypeName.NullToEmpty();
                            string shiftColor = shift.ShiftTypeColor.EmptyToNull() ?? Constants.SHIFT_TYPE_DEFAULT_COLOR;
                            string shiftDescription = shift.ShiftTypeDescription.NullToEmpty();
                            bool onDutyShift = shift.Type == TermGroup_TimeScheduleTemplateBlockType.OnDuty;
                            int netTimeMinutes = !onDutyShift ? day.GrossNetCosts.GetNetTimeMinutes(shift.TimeScheduleTemplateBlockId) : Convert.ToInt32((shift.StopTime - shift.StartTime).TotalMinutes);

                            if (isHidden && netTimeMinutes == 0)
                                netTimeMinutes = Convert.ToInt32((shift.StopTime - shift.StartTime).TotalMinutes) - Convert.ToInt32(shift.Break1Minutes + shift.Break2Minutes + shift.Break3Minutes + shift.Break4Minutes);

                            int grossTimeMinutes = !onDutyShift ? day.GrossNetCosts.GetGrossTimeMinutes(shift.TimeScheduleTemplateBlockId) : netTimeMinutes;
                            if (grossTimeMinutes == 0 && netTimeMinutes != 0)
                                grossTimeMinutes = netTimeMinutes;
                            decimal totalCost = !onDutyShift ? day.GrossNetCosts.GetTotalCostIncludingEmploymentTaxAndSupplementCharge(shift.TimeScheduleTemplateBlockId) : 0;
                            decimal employmentTaxCost = !onDutyShift ? day.GrossNetCosts.GetEmploymentTaxCost(shift.TimeScheduleTemplateBlockId) : 0;
                            decimal supplementChargeCost = !onDutyShift ? day.GrossNetCosts.GetSupplementChargeCost(shift.TimeScheduleTemplateBlockId) : 0;

                            TimeDeviationCause timeDeviationCause = shift.TimeDeviationCauseId.HasValue ? TimeDeviationCauseManager.GetTimeDeviationCause(shift.TimeDeviationCauseId.Value, reportResult.ActorCompanyId, false) : null;

                            string timeDeviationCauseName = timeDeviationCause?.Name ?? string.Empty;
                            int lended = 0; //TODO: Get value from logic (0=No flag, 1=Lended out, 2=Lended in)

                            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
                            if (!selectionAccountIds.IsNullOrEmpty() && employeeAccountsOnEmployee.Any() && base.UseAccountHierarchyOnCompanyFromCache(entitiesReadOnly, reportResult.ActorCompanyId))
                            {
                                var employeeAccountsOnDate = employeeAccountsOnEmployee.GetEmployeeAccounts(day.Date, day.Date);

                                // case 1. If the shift's account is included in the list of accounts you are printing for, and is part of the employee's affiliation and is marked as default, then it is not lended = 0.
                                if (!shift.AccountId.HasValue || (selectionAccountIds.Contains(shift.AccountId.Value) && employeeAccountsOnDate.Any(e => e.AccountId == shift.AccountId.Value && e.Default)))
                                    lended = (int)SoeTimeScheduleTemplateBlockLendedType.None;
                                // case 2. If the shift's account is included in the list of accounts you are printing for, and is part of the employee's affiliation and is NOT marked as default, then it is lended = 2.
                                else if (selectionAccountIds.Contains(shift.AccountId.Value) && employeeAccountsOnDate.Any(e => e.AccountId == shift.AccountId.Value && !e.Default))
                                    lended = (int)SoeTimeScheduleTemplateBlockLendedType.LendedInFromOther;
                                // case 3. If the shift's account is NOT included in the list of accounts you are printing for, but is part of the employee's affiliation, then it is lent out = 1 (in this case, the employee has their default affiliation among the accounts you are printing for, which is why it is included).
                                else if (!selectionAccountIds.Contains(shift.AccountId.Value) && employeeAccountsOnDate.Any(e => e.AccountId == shift.AccountId.Value))
                                    lended = (int)SoeTimeScheduleTemplateBlockLendedType.LendedToOther;
                            }

                            // Set fixed background color if shift is absence
                            if (timeDeviationCause != null)
                                shiftColor = "#ef545e";   // @shiftAbsenceBackgroundColor

                            #endregion

                            #region Shifts Element

                            if (excludeAbsence && timeDeviationCause != null)
                                continue;

                            if (shiftXmlId <= MAX_SHIFTS)
                            {
                                dayElement.Add(
                                    new XElement("Shift" + shiftXmlId + "Name", shiftName),
                                    new XElement("Shift" + shiftXmlId + "StartTime", startTime),
                                    new XElement("Shift" + shiftXmlId + "StopTime", stopTime),
                                    new XElement("Shift" + shiftXmlId + "Description", shiftDescription),
                                    new XElement("Shift" + shiftXmlId + "Color", shiftColor),
                                    new XElement("Shift" + shiftXmlId + "GrossTimeMinutes", grossTimeMinutes),
                                    new XElement("Shift" + shiftXmlId + "NetTimeMinutes", netTimeMinutes),
                                    new XElement("Shift" + shiftXmlId + "EmploymentTaxCost", employmentTaxCost),
                                    new XElement("Shift" + shiftXmlId + "SupplementChargeCost", supplementChargeCost),
                                    new XElement("Shift" + shiftXmlId + "TotalCost", totalCost),
                                    new XElement("Shift" + shiftXmlId + "TimeDeviationCauseName", timeDeviationCauseName),
                                    new XElement("Shift" + shiftXmlId + "Lended", lended),
                                    new XElement("Shift" + shiftXmlId + "Link", shift.Link.NullToEmpty()));
                            }

                            shiftElements.Add(new XElement("Shifts",
                                new XAttribute("id", shiftXmlId),
                                new XElement("ShiftName", shiftName),
                                new XElement("ShiftDescription", shiftDescription),
                                new XElement("ShiftStartTime", startTime),
                                new XElement("ShiftStopTime", stopTime),
                                new XElement("Color", shiftColor),
                                new XElement("ShiftGrossTimeMinutes", grossTimeMinutes),
                                new XElement("ShiftNetTimeMinutes", netTimeMinutes),
                                new XElement("ShiftEmploymentTaxCost", employmentTaxCost),
                                new XElement("ShiftSupplementChargeCost", supplementChargeCost),
                                new XElement("ShiftTotalCost", totalCost),
                                new XElement("ShiftTimeDeviationCauseName", timeDeviationCauseName),
                                new XElement("ShiftLended", lended)));

                            shiftXmlId++;

                            #endregion
                        }

                        #region Default Shift Element

                        for (int shiftNr = shiftXmlId; shiftNr <= MAX_SHIFTS; shiftNr++)
                        {
                            dayElement.Add(
                                new XElement("Shift" + shiftNr + "Name", string.Empty),
                                new XElement("Shift" + shiftNr + "StartTime", CalendarUtility.DATETIME_DEFAULT),
                                new XElement("Shift" + shiftNr + "StopTime", CalendarUtility.DATETIME_DEFAULT),
                                new XElement("Shift" + shiftNr + "Description", String.Empty),
                                new XElement("Shift" + shiftNr + "Color", Constants.SHIFT_TYPE_DEFAULT_COLOR),
                                new XElement("Shift" + shiftNr + "GrossTimeMinutes", 0),
                                new XElement("Shift" + shiftNr + "NetTimeMinutes", 0),
                                new XElement("Shift" + shiftNr + "EmploymentTaxCost", 0),
                                new XElement("Shift" + shiftNr + "SupplementChargeCost", 0),
                                new XElement("Shift" + shiftNr + "TotalCost", 0),
                                new XElement("Shift" + shiftNr + "TimeDeviationCauseName", string.Empty),
                                new XElement("Shift" + shiftNr + "Lended", 0),
                                new XElement("Shift" + shiftNr + "Link", string.Empty));
                        }

                        foreach (var shiftElement in shiftElements)
                        {
                            dayElement.Add(shiftElement);
                        }

                        #endregion

                        weekDayElements.Add(dayElement);
                        dayElements.Add(dayElement);
                        dayXmlId++;
                    }

                    #region Default Day Element

                    if (dayXmlId == 1)
                    {
                        XElement dayElement = new XElement("Day",
                            new XAttribute("id", 1),
                            new XElement("DayNr", 0),
                            new XElement("IsZeroScheduleDay", 0),
                            new XElement("IsAbsenceDay", 0),
                            new XElement("IsPreliminary", 0),
                            new XElement("AbsencePayrollProductName", ""),
                            new XElement("AbsencePayrollProductShortName", ""),
                            new XElement("ScheduleStartTime", "00:00"),
                            new XElement("ScheduleStopTime", "00:00"),
                            new XElement("ScheduleTime", 0),
                            new XElement("OccupiedTime", 0),
                            new XElement("ScheduleBreakTime", 0),
                            new XElement("ScheduleBreak1Start", CalendarUtility.DATETIME_DEFAULT),
                            new XElement("ScheduleBreak1Minutes", 0),
                            new XElement("ScheduleBreak2Start", CalendarUtility.DATETIME_DEFAULT),
                            new XElement("ScheduleBreak2Minutes", 0),
                            new XElement("ScheduleBreak3Start", CalendarUtility.DATETIME_DEFAULT),
                            new XElement("ScheduleBreak3Minutes", 0),
                            new XElement("ScheduleBreak4Start", CalendarUtility.DATETIME_DEFAULT),
                            new XElement("ScheduleBreak4Minutes", 0),
                            new XElement("ScheduleDate", startDate));

                        weekDayElements.Add(dayElement);
                    }

                    #endregion

                    #region Week Element

                    XElement weekElement = new XElement("Week",
                        new XAttribute("id", weekXmlId),
                        new XElement("ScheduleWeekNr", Convert.ToInt32(weekNr.Split('_')[1])),
                        new XElement("EmployeeWeekTimeTotal", employeeWeekScheduleTimeTotal));
                    weekXmlId++;
                    numberOfWeeks++;

                    foreach (var weekDayElement in weekDayElements)
                    {
                        weekElement.Add(weekDayElement);
                    }

                    weekElements.Add(weekElement);

                    #endregion

                    #endregion
                }
            }

            //Return first day as List
            if (!returnWeek)
                return dayElements.Count > 0 ? new List<XElement>() { dayElements.FirstOrDefault() } : new List<XElement>();

            return weekElements;
        }

        protected XElement CreateTimeReportHeaderElement(EvaluatedSelection es)
        {
            TimePeriod timePeriod = null;
            if (es.ST_TimePeriodId.HasValue)
                timePeriod = TimePeriodManager.GetTimePeriod(es.ST_TimePeriodId.Value, es.ActorCompanyId);

            return new XElement("ReportHeader",
                    this.CreateReportTitleElement(es.ReportName),
                    this.CreateReportDescriptionElement(es.ReportDescription),
                    this.CreateReportNrElement(es.ReportNr.ToString()),
                    this.CreateCompanyElement(),
                    this.CreateCompanyOrgNrElement(),
                    this.CreateLoginNameElement(es.LoginName),
                    new XElement("TimePeriod", timePeriod?.Name ?? ""),
                    new XElement("SortByLevel1", es.SortByLevel1),
                    new XElement("SortByLevel2", es.SortByLevel2),
                    new XElement("SortByLevel3", es.SortByLevel3),
                    new XElement("SortByLevel4", es.SortByLevel4),
                    new XElement("IsSortAscending", es.IsSortAscending),
                    new XElement("GroupByLevel1", es.GroupByLevel1),
                    new XElement("GroupByLevel2", es.GroupByLevel2),
                    new XElement("GroupByLevel3", es.GroupByLevel3),
                    new XElement("GroupByLevel4", es.GroupByLevel4),
                    new XElement("Special", es.Special));
        }

        protected XElement CreateTimeReportHeaderElement(CreateReportResult reportResult)
        {
            if (reportResult.Input != null)
                return CreateTimeReportHeaderElement(reportResult.Input.Report, Company, reportResult.Input.User);
            else if (reportResult.EvaluatedSelection != null)
                return CreateTimeReportHeaderElement(reportResult.EvaluatedSelection);

            return null;
        }

        protected XElement CreateTimeReportHeaderElement(Report report, Company company, User user, TimePeriod timePeriod = null)
        {
            return new XElement("ReportHeader",
                    this.CreateReportTitleElement(report.Name),
                    this.CreateReportDescriptionElement(report.Description),
                    this.CreateReportNrElement(report.ReportNr.ToString()),
                    this.CreateCompanyElement(),
                    this.CreateCompanyOrgNrElement(),
                    this.CreateLoginNameElement(user.LoginName),
                    new XElement("TimePeriod", timePeriod?.Name ?? ""),
                    new XElement("SortByLevel1", (TermGroup_ReportGroupAndSortingTypes)report.SortByLevel1),
                    new XElement("SortByLevel2", (TermGroup_ReportGroupAndSortingTypes)report.SortByLevel2),
                    new XElement("SortByLevel3", (TermGroup_ReportGroupAndSortingTypes)report.SortByLevel3),
                    new XElement("SortByLevel4", (TermGroup_ReportGroupAndSortingTypes)report.SortByLevel4),
                    new XElement("IsSortAscending", report.IsSortAscending),
                    new XElement("GroupByLevel1", (TermGroup_ReportGroupAndSortingTypes)report.GroupByLevel1),
                    new XElement("GroupByLevel2", (TermGroup_ReportGroupAndSortingTypes)report.GroupByLevel2),
                    new XElement("GroupByLevel3", (TermGroup_ReportGroupAndSortingTypes)report.GroupByLevel3),
                    new XElement("GroupByLevel4", (TermGroup_ReportGroupAndSortingTypes)report.GroupByLevel4),
                    new XElement("Special", report.Special));

        }

        protected XElement CreateExtendedPersonellReportHeaderElement(EvaluatedSelection es)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return CreateExtendedPersonellReportHeaderElement(entities, es);
        }

        protected XElement CreateExtendedPersonellReportHeaderElement(CompEntities entities, EvaluatedSelection es)
        {
            return CreateExtendedPersonellReportHeaderElement(entities, es.ST_TimePeriodId, es.ActorCompanyId, es.ReportName, es.ReportDescription, es.ReportNr, es.LoginName);
        }

        protected XElement CreateExtendedPersonellReportHeaderElement(CreateReportResult reportResult, int? selectionTimePeriodId = null)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return CreateExtendedPersonellReportHeaderElement(entities, reportResult, selectionTimePeriodId);
        }

        protected XElement CreateExtendedPersonellReportHeaderElement(CompEntities entities, CreateReportResult reportResult, int? selectionTimePeriodId = null)
        {
            return CreateExtendedPersonellReportHeaderElement(entities, selectionTimePeriodId, reportResult.ActorCompanyId, reportResult.ReportName, reportResult.ReportDescription, reportResult.ReportNr, reportResult.LoginName);
        }

        protected XElement CreateExtendedPersonellReportHeaderElement(CompEntities entities, int? timePeriodId, int actorCompanyId, string reportName, string reportDescription, int reportNr, string loginName)
        {
            #region Prereq

            string companyVatNr = "", distributionAddress = "", distributionAddressCO = "", distributionPostalCode = "", distributionPostalAddress = "", distributionCountry = "",
                   email = "", phoneHome = "", phoneJob = "", phoneMobile = "", fax = "", webAddress = "";

            if (Company != null)
            {
                companyVatNr = Company.VatNr;

                Contact contact = ContactManager.GetContactFromActor(entities, Company.ActorCompanyId);
                List<ContactAddressRow> contactAddressRows = contact != null ? ContactManager.GetContactAddressRows(entities, contact.ContactId) : null;
                distributionAddress = contactAddressRows.GetContactAddressRowText(TermGroup_SysContactAddressType.Distribution, TermGroup_SysContactAddressRowType.Address);
                distributionAddressCO = contactAddressRows.GetContactAddressRowText(TermGroup_SysContactAddressType.Distribution, TermGroup_SysContactAddressRowType.AddressCO);
                distributionPostalCode = contactAddressRows.GetContactAddressRowText(TermGroup_SysContactAddressType.Distribution, TermGroup_SysContactAddressRowType.PostalCode);
                distributionPostalAddress = contactAddressRows.GetContactAddressRowText(TermGroup_SysContactAddressType.Distribution, TermGroup_SysContactAddressRowType.PostalAddress);
                distributionCountry = contactAddressRows.GetContactAddressRowText(TermGroup_SysContactAddressType.Distribution, TermGroup_SysContactAddressRowType.Country);

                List<ContactECom> contactEcoms = contact != null ? ContactManager.GetContactEComs(entities, contact.ContactId) : null;
                email = contactEcoms.GetEComText(TermGroup_SysContactEComType.Email);
                phoneHome = contactEcoms.GetEComText(TermGroup_SysContactEComType.PhoneHome);
                phoneJob = contactEcoms.GetEComText(TermGroup_SysContactEComType.PhoneJob);
                phoneMobile = contactEcoms.GetEComText(TermGroup_SysContactEComType.PhoneMobile);
                fax = contactEcoms.GetEComText(TermGroup_SysContactEComType.Fax);
                webAddress = contactEcoms.GetEComText(TermGroup_SysContactEComType.Web);
            }

            TimePeriod timePeriod = null;
            if (timePeriodId.HasValue)
                timePeriod = TimePeriodManager.GetTimePeriod(entities, timePeriodId.Value, actorCompanyId);

            #endregion

            return new XElement("ReportHeader",
                    this.CreateReportTitleElement(reportName),
                    this.CreateReportDescriptionElement(reportDescription),
                    this.CreateReportNrElement(reportNr.ToString()),
                    this.CreateCompanyElement(),
                    this.CreateCompanyOrgNrElement(),
                    this.CreateLoginNameElement(loginName),
                    new XElement("TimePeriod", timePeriod?.Name ?? ""),
                    new XElement("CompanyVatNr", companyVatNr),
                    new XElement("CompanyAddress", distributionAddress),
                    new XElement("CompanyAddressCO", distributionAddressCO),
                    new XElement("CompanyPostalCode", distributionPostalCode),
                    new XElement("CompanyPostalAddress", distributionPostalAddress),
                    new XElement("CompanyCountry", distributionCountry),
                    new XElement("CompanyEmail", email),
                    new XElement("CompanyPhoneHome", phoneHome),
                    new XElement("CompanyPhoneWork", phoneJob),
                    new XElement("CompanyPhoneMobile", phoneMobile),
                    new XElement("CompanyFax", fax),
                    new XElement("CompanyWebAddress", webAddress));
        }

        protected XElement CreatePayrollProductReportHeaderElement(string loginName, string reportName, string description, int reportNr, List<AccountDim> accountDims)
        {
            var element = new XElement("ReportHeader",
                   this.CreateLoginNameElement(loginName),
                   this.CreateReportTitleElement(reportName),
                   this.CreateReportDescriptionElement(description),
                   this.CreateReportNrElement(reportNr.ToString()),
                   this.CreateCompanyElement(),
                   this.CreateCompanyOrgNrElement());

            int numberOfdims = 6;
            int dimCounter = 1;

            while (dimCounter <= numberOfdims)
            {
                if (dimCounter == 1)
                {
                    foreach (var dim in accountDims.OrderBy(d => d.AccountDimNr))
                    {

                        element.Add(new XElement("AccountDimNr" + dimCounter + "Name", dim.Name));
                        element.Add(new XElement("AccountDimNr" + dimCounter + "ShortName", dim.ShortName));
                        dimCounter++;
                    }
                    continue;
                }

                element.Add(new XElement("AccountDimNr" + dimCounter + "Name", string.Empty));
                element.Add(new XElement("AccountDimNr" + dimCounter + "ShortName", string.Empty));

                dimCounter++;
            }

            return element;
        }

        protected void AddGrossNetCostHeaderPermissionAndSettingReportHeaderElements(XElement parent, int roleId, int actorCompanyId)
        {
            if (parent == null)
                return;

            bool hasShowCostsPermission = FeatureManager.HasRolePermission(Feature.Time_Schedule_SchedulePlanning_ShowCosts, Permission.Readonly, roleId, actorCompanyId);
            bool hasShowGrossNetTimeSetting = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.PayrollAgreementUseGrossNetTimeInStaffing, 0, actorCompanyId, 0);

            parent.Add(
                new XElement("HasShowCostsPermission", hasShowCostsPermission.ToInt()),
                new XElement("HasShowGrossNetTimeSetting", hasShowGrossNetTimeSetting.ToInt()));
        }

        #endregion

        #endregion

        #region PageHeaderLabel elements

        #region ERP

        protected void AddAccountDimPageHeaderLabelElements(XElement parent, AccountDim accountDimStd, List<AccountDim> accountDimInternals, bool addExludeAccounting = false)
        {
            if (parent == null)
                return;

            //AccountDim Std
            parent.Add(
                new XElement("DimensionStandardNameLabel", accountDimStd != null ? accountDimStd.Name : String.Empty),
                new XElement("DimensionStandardShortNameLabel", accountDimStd != null ? accountDimStd.ShortName : String.Empty));

            //AccountDim internal
            for (int i = 1; i <= Constants.ACCOUNTDIM_NROFDIMENSIONS; i++)
            {
                if (i > accountDimInternals.Count)
                {
                    parent.Add(
                        new XElement("DimensionInternalName" + i + "Label", String.Empty),
                        new XElement("DimensionInternalShortName" + i + "Label", String.Empty),
                        new XElement("DimensionInternal" + i + "SieType", String.Empty));
                    continue;
                }

                AccountDim accountDim = accountDimInternals.Any() ? accountDimInternals.ElementAt(i - 1) : null;
                if (addExludeAccounting)
                {
                    parent.Add(
                        new XElement("DimensionInternalName" + i + "Label", accountDim != null ? accountDim.Name : String.Empty),
                        new XElement("DimensionInternalShortName" + i + "Label", accountDim != null ? accountDim.ShortName : String.Empty),
                        new XElement("DimensionInternal" + i + "SieType", accountDim != null ? accountDim.SysSieDimNr.ToString() : String.Empty),
                        new XElement("DimensionInternal" + i + "ExcludeinAccountingExport", accountDim != null ? (accountDim.ExcludeinAccountingExport ? "1" : "0") : String.Empty));
                }
                else
                {
                    parent.Add(
                        new XElement("DimensionInternalName" + i + "Label", accountDim != null ? accountDim.Name : String.Empty),
                        new XElement("DimensionInternalShortName" + i + "Label", accountDim != null ? accountDim.ShortName : String.Empty),
                        new XElement("DimensionInternal" + i + "SieType", accountDim != null ? accountDim.SysSieDimNr.ToString() : String.Empty));
                }
            }
        }

        protected void AddAccountDimPageHeaderLabelElements(XElement parent, AccountDimDTO accountDimStd, List<AccountDimDTO> accountDimInternals)
        {
            if (parent == null)
                return;

            //AccountDim Std
            parent.Add(
                new XElement("DimensionStandardNameLabel", accountDimStd != null ? accountDimStd.Name : String.Empty),
                new XElement("DimensionStandardShortNameLabel", accountDimStd != null ? accountDimStd.ShortName : String.Empty));

            //AccountDim internal
            for (int i = 1; i <= Constants.NOOFDIMENSIONS; i++)
            {
                if (i > accountDimInternals.Count)
                {
                    parent.Add(
                        new XElement("DimensionInternalName" + i + "Label", String.Empty),
                        new XElement("DimensionInternalShortName" + i + "Label", String.Empty));
                    continue;
                }

                AccountDimDTO accountDim = accountDimInternals.Any() ? accountDimInternals.ElementAt(i - 1) : null;

                parent.Add(
                    new XElement("DimensionInternalName" + i + "Label", accountDim != null ? accountDim.Name : String.Empty),
                    new XElement("DimensionInternalShortName" + i + "Label", accountDim != null ? accountDim.ShortName : String.Empty));
            }
        }

        protected void AddAccountBalanceChangePeriodPageHeaderLabelElements(XElement parent)
        {
            if (parent == null)
                return;

            parent.Add(
                //Long
                new XElement("AccountBalancechangePeriodLabel", this.GetReportText(43, "Perioden")), //Omsättning inom period
                new XElement("AccountBalancechangePeriodLabel2", this.GetReportText(57, "Utfall"))); //Omsättning inom period
        }

        protected void AddAccountBalanceChangePageHeaderLabelElements(XElement parent)
        {
            if (parent == null)
                return;

            parent.Add(
                //Long
                new XElement("AccountBalancechangeToPeriodLabel", this.GetReportText(44, "Året totalt")), //Omsättning till period
                new XElement("AccountBalancechangeIncludingPeriodLabel", this.GetReportText(44, "Året totalt")), //Omsättning t.o.m period
                new XElement("AccountBalancechangeYearLabel", this.GetReportText(82, "Året")), //Omsättning hela året
                new XElement("AccountBalanceChangeDeviationLabel", this.GetReportText(58, "Avvikelse %")),

                //Short 
                new XElement("AccountBalancechangePeriodShortLabel", this.GetReportText(37, "Period")), //Oms per
                new XElement("AccountBalancechangePeriodShortLabel2", this.GetReportText(59, "Utfall")), //Oms per
                new XElement("AccountBalancechangeToPeriodShortLabel", this.GetReportText(38, "År tot")), //Oms till per
                new XElement("AccountBalancechangeIncludingPeriodShortLabel", this.GetReportText(38, "År tot")), //Oms t.o.m per
                new XElement("AccountBalancechangeYearShortLabel", this.GetReportText(82, "Året")), //Omsättning hela året
                new XElement("AccountBalanceChangeDeviationShortLabel", this.GetReportText(60, "Avvik %")),

                //Long diff
                new XElement("AccountBalancechangePeriodDiffLabel", this.GetReportText(55, "Jämf.år Omsättning inom period")),
                new XElement("AccountBalancechangeToPeriodDiffLabel", this.GetReportText(56, "Jämf.år Omsättning till period")),
                new XElement("AccountBalancechangeIncludingPeriodDiffLabel", this.GetReportText(148, "Jämf.år Omsättning t.o.m period")),

                //Short diff
                new XElement("AccountBalancechangePeriodDiffShortLabel", this.GetReportText(49, "Jmf Oms per")),
                new XElement("AccountBalancechangeToPeriodDiffShortLabel", this.GetReportText(50, "Jmf Oms till per")),
                new XElement("AccountBalancechangeIncludingPeriodDiffShortLabel", this.GetReportText(149, "Jmf Oms t.o.m per")),

                //Previous Year Long
                new XElement("PreviousYearAccountPeriodInBalanceLabel", this.GetReportText(469, "Ingående saldo jämförelseår")),
                new XElement("PreviousYearAccountPeriodOutBalanceLabel", this.GetReportText(470, "Utgående saldo jämförelseår")),
                new XElement("PreviousYearAccountOpeningBalanceLabel", this.GetReportText(471, "Ingående balans jämförelseår")),
                new XElement("PreviousYearAccountClosingBalanceLabel", this.GetReportText(472, "Utgående balans jämförelseår")),

                //Previous Year Short
                new XElement("PreviousYearAccountPeriodInBalanceShortLabel", this.GetReportText(473, "Ing saldo jmfår")),
                new XElement("PreviousYearAccountPeriodOutBalanceShortLabel", this.GetReportText(474, "Utg saldo jmfår")),
                new XElement("PreviousYearAccountOpeningBalanceShortLabel", this.GetReportText(475, "IB jmfår")),
                new XElement("PreviousYearAccountClosingBalanceShortLabel", this.GetReportText(476, "UB jmfår")),

                //Previous Year Long diff
                new XElement("PreviousYearAccountPeriodInBalanceDiffLabel", this.GetReportText(477, "Jämf.år Ingående saldo")),
                new XElement("PreviousYearAccountPeriodOutBalanceDiffLabel", this.GetReportText(478, "Jämf.år Utgående saldo")),
                new XElement("PreviousYearAccountOpeningBalanceDiffLabel", this.GetReportText(479, "Jämf.år Ingående balans")),
                new XElement("PreviousYearAccountClosingBalanceDiffLabel", this.GetReportText(480, "Jämf.år Utgående balans")),

                //Previous Year Short diff
                new XElement("PreviousYearAccountPeriodInBalanceDiffShortLabel", this.GetReportText(481, "Jmf Ing saldo")),
                new XElement("PreviousYearAccountPeriodOutBalanceDiffShortLabel", this.GetReportText(482, "Jmf Utg saldo")),
                new XElement("PreviousYearAccountOpeningBalanceDiffShortLabel", this.GetReportText(483, "Jmf IB")),
                new XElement("PreviousYearAccountClosingBalanceDiffShortLabel", this.GetReportText(484, "Jmf UB")),

                new XElement("PreviousYearSamePeriodLabel", this.GetReportText(485, "Jämförelseår samma period")),
                new XElement("PreviousYearSamePeriodShortLabel", this.GetReportText(486, "Jmfår per")),
                new XElement("PreviousYearTotalLabel", this.GetReportText(487, "Jämförelseår totalt")),
                new XElement("PreviousYearTotalShortLabel", this.GetReportText(488, "Jmfår tot")),
                new XElement("DiffLabel", this.GetReportText(489, "Diff")),

                //Budget
                new XElement("BudgetLabel", this.GetReportText(490, "Budget")),
                new XElement("ForecastLabel", this.GetReportText(491, "Prognos")),
                new XElement("BudgetForcastLabel", this.GetReportText(492, "Budget/Prognos")),
                new XElement("BudgetDiffLabel", this.GetReportText(493, "Jämfört med")),
                new XElement("BudgetDiffShortLabel", this.GetReportText(494, "Jmf med")),

                //Months
                new XElement("MonthLabel", this.GetReportText(405, "Månad")),
                new XElement("PeriodLabel", this.GetReportText(496, "Period")),
                new XElement("MonthJanuaryLabel", this.GetReportText(497, "Januari")),
                new XElement("MonthFebruaryLabel", this.GetReportText(498, "Februari")),
                new XElement("MonthMarchLabel", this.GetReportText(499, "Mars")),
                new XElement("MonthAprilLabel", this.GetReportText(500, "April")),
                new XElement("MonthMayLabel", this.GetReportText(501, "Maj")),
                new XElement("MonthJuneLabel", this.GetReportText(502, "Juni")),
                new XElement("MonthJulyLabel", this.GetReportText(503, "Juli")),
                new XElement("MonthAugustLabel", this.GetReportText(504, "Augusti")),
                new XElement("MonthSeptemberLabel", this.GetReportText(505, "September")),
                new XElement("MonthOctoberLabel", this.GetReportText(506, "Oktober")),
                new XElement("MonthNovemberLabel", this.GetReportText(507, "November")),
                new XElement("MonthDecemberLabel", this.GetReportText(508, "December")),
                new XElement("MonthJanuaryShortLabel", this.GetReportText(509, "Jan")),
                new XElement("MonthFebruaryShortLabel", this.GetReportText(510, "Feb")),
                new XElement("MonthMarchShortLabel", this.GetReportText(511, "Mar")),
                new XElement("MonthAprilShortLabel", this.GetReportText(512, "Apr")),
                new XElement("MonthMayShortLabel", this.GetReportText(513, "Maj")),
                new XElement("MonthJuneShortLabel", this.GetReportText(514, "Jun")),
                new XElement("MonthJulyShortLabel", this.GetReportText(515, "Jul")),
                new XElement("MonthAugustShortLabel", this.GetReportText(516, "Aug")),
                new XElement("MonthSeptemberShortLabel", this.GetReportText(517, "Sep")),
                new XElement("MonthOctoberShortLabel", this.GetReportText(518, "Okt")),
                new XElement("MonthNovemberShortLabel", this.GetReportText(519, "Nov")),
                new XElement("MonthDecemberShortLabel", this.GetReportText(520, "Dec")),
                new XElement("YearLabel", this.GetReportText(521, "År")));
        }

        protected void AddAccountBalanceChangePreviousPageHeaderLabelElements(XElement parent)
        {
            if (parent == null)
                return;

            parent.Add(
                //Long
                new XElement("AccountBalanceChangeAllPreviousToPeriodBalanceLabel", this.GetReportText(415, "Tidigare")),
                new XElement("AccountBalanceChangeAllPreviousThisYearBalanceLabel", this.GetReportText(416, "Tidigare i år")));
        }

        protected void AddAccountBalancePageHeaderLabelElements(XElement parent)
        {
            if (parent == null)
                return;

            parent.Add(
                //Long
                new XElement("AccountPeriodInBalanceLabel", this.GetReportText(39, "Ingående saldo")),
                new XElement("AccountPeriodOutBalanceLabel", this.GetReportText(40, "Utgående saldo")),
                new XElement("AccountOpeningBalanceLabel", this.GetReportText(41, "Ingående balans")),
                new XElement("AccountClosingBalanceLabel", this.GetReportText(42, "Utgående balans")),

                //Short
                new XElement("AccountPeriodInBalanceShortLabel", this.GetReportText(31, "Ing saldo")),
                new XElement("AccountPeriodOutBalanceShortLabel", this.GetReportText(32, "Utg saldo")),
                new XElement("AccountOpeningBalanceShortLabel", this.GetReportText(33, "IB")),
                new XElement("AccountClosingBalanceShortLabel", this.GetReportText(36, "UB")),

                //Long diff
                new XElement("AccountPeriodInBalanceDiffLabel", this.GetReportText(51, "Jämf.år Ingående saldo")),
                new XElement("AccountPeriodOutBalanceDiffLabel", this.GetReportText(52, "Jämf.år Utgående saldo")),
                new XElement("AccountOpeningBalanceDiffLabel", this.GetReportText(53, "Jämf.år Ingående balans")),
                new XElement("AccountClosingBalanceDiffLabel", this.GetReportText(54, "Jämf.år Utgående balans")),

                //Short diff
                new XElement("AccountPeriodInBalanceDiffShortLabel", this.GetReportText(45, "Jmf Ing saldo")),
                new XElement("AccountPeriodOutBalanceDiffShortLabel", this.GetReportText(46, "Jmf Utg saldo")),
                new XElement("AccountOpeningBalanceDiffShortLabel", this.GetReportText(47, "Jmf IB")),
                new XElement("AccountClosingBalanceDiffShortLabel", this.GetReportText(48, "Jmf UB")));



        }

        protected void AddEnterpriseCurrencyPageLabelElements(XElement parent)
        {
            if (parent == null)
                return;

            Currency currency = CountryCurrencyManager.GetCurrencyFromType(Company.ActorCompanyId, TermGroup_CurrencyType.EnterpriseCurrency);

            parent.Add(
                new XElement("EntCurrencyName", currency != null ? currency.Name : ""),
                new XElement("EntCurrencyCode", currency != null ? currency.Code : ""));
        }

        protected void CreateVoucherListPageHeaderLabelsElement(XElement parent)
        {
            parent.Add(
                new XElement("VoucherSerieNameLabel", this.GetReportText(7, "Serienamn")),
                new XElement("VoucherSerieNrLabel", this.GetReportText(13, "Serie")),
                new XElement("VoucherNrLabel", this.GetReportText(8, "Vernr")),
                new XElement("VoucherDateLabel", this.GetReportText(9, "Datum")),
                new XElement("VoucherTextLabel", this.GetReportText(27, "Text")),
                new XElement("VoucherRowDateLabel", this.GetReportText(26, "Datum")),
                new XElement("VoucherRowTextLabel", this.GetReportText(15, "Text")),
                new XElement("AccountNameLabel", this.GetReportText(10, "Benämning")),
                new XElement("DebitLabel", this.GetReportText(11, "Debet")),
                new XElement("CreditLabel", this.GetReportText(12, "Kredit")),
                new XElement("QuantityLabel", this.GetReportText(14, "Kvantitet")));
        }

        protected void CreateGeneralLedgerPageHeaderLabelsElement(XElement parent)
        {
            parent.Add(
                new XElement("VoucherSerieNameLabel", this.GetReportText(7, "Serienamn")),
                new XElement("VoucherSerieNrLabel", this.GetReportText(13, "Serie")),
                new XElement("VoucherNrLabel", this.GetReportText(8, "Vernr")),
                new XElement("VoucherRowDateLabel", this.GetReportText(26, "Datum")),
                new XElement("VoucherRowTextLabel", this.GetReportText(15, "Text")),
                new XElement("AccountNameLabel", this.GetReportText(10, "Benämning")),
                new XElement("DebitLabel", this.GetReportText(11, "Debet")),
                new XElement("CreditLabel", this.GetReportText(12, "Kredit")),
                new XElement("QuantityLabel", this.GetReportText(14, "Kvantitet")),
                new XElement("BalanceLabel", this.GetReportText(25, "Saldo")),
                new XElement("AccountOpeningBalanceLabel", this.GetReportText(41, "Ingående balans")),
                new XElement("AccountPeriodOutBalanceLabel", this.GetReportText(40, "Utgående saldo")),
                new XElement("AccountLabel", this.GetReportText(205, "Konto"))
                );
        }

        protected void CreateAccountDistributionHeadListPageHeaderLabelsElement(XElement parent, AccountDim accountDimStd, List<AccountDim> accountDimInternals)
        {
            parent.Add(
                new XElement("NameLabel", this.GetReportText(838, "Namn")),
                new XElement("DescriptionLabel", this.GetReportText(839, "Beskrivning")),
                new XElement("CalculationTypeLabel", this.GetReportText(840, "Beräkningstyp")),
                new XElement("StartDateLabel", this.GetReportText(841, "Startdatum")),
                new XElement("EndDateLabel", this.GetReportText(842, "Slutdatum")),
                new XElement("DayNumberLabel", this.GetReportText(843, "Dag i perioden")),
                new XElement("TriggerTypeLabel", this.GetReportText(844, "Typ")),
                new XElement("PeriodTypeLabel", this.GetReportText(845, "Periodiseringstyp")),
                new XElement("PeriodValueLabel", this.GetReportText(846, "Antal gånger")),
                new XElement("VoucherSerieNameLabel", this.GetReportText(847, "Verifikatserie")),
                new XElement("UsedInLabel", this.GetReportText(848, "Använd i")),
                new XElement("UseInVoucherLabel", this.GetReportText(849, "verifikatregistrering")),
                new XElement("UseInCustomerInvoiceLabel", this.GetReportText(850, "kundfaktura")),
                new XElement("UseInSupplierInvoiceLabel", this.GetReportText(851, "leverantörsfaktura")),
                new XElement("UseInImportLabel", this.GetReportText(852, "import")),
                new XElement("SortLabel", this.GetReportText(853, "Sortering")),
                new XElement("ConditionsLabel", this.GetReportText(854, "Villkor")),
                new XElement("KeepRowLabel", this.GetReportText(855, "Behåll rad")),
                new XElement("AmountLabel", this.GetReportText(856, "Belopp")),
                new XElement("ResultLabel", this.GetReportText(857, "Utfall")),
                new XElement("RowNumberLabel", this.GetReportText(858, "Radnr")),
                new XElement("CalculateRowNbrLabel", this.GetReportText(859, "Belopp från rad")),
                new XElement("SameBalanceLabel", this.GetReportText(860, "Samma tecken")),
                new XElement("OppositeBalanceLabel", this.GetReportText(861, "Motsatt tecken")),
                new XElement("AccountDimStdShortNameLabel", accountDimStd != null ? accountDimStd.ShortName : String.Empty),
                new XElement("SourceRefNrLabel", this.GetReportText(927, "Källa Ref. Nr.")),
                new XElement("VoucherDateLabel", this.GetReportText(928, "Bokföringsdatum")),
                new XElement("TotalAmountLabel", this.GetReportText(929, "Totalt belopp")),
                new XElement("PeriodAmountLabel", this.GetReportText(1056, "Belopp period")),
                new XElement("BalanceLabel", this.GetReportText(1057, "Saldo")),
                new XElement("PeriodsExecutedLabel", this.GetReportText(1058, "Utfört")),
                new XElement("PeriodsRemainingLabel", this.GetReportText(1059, "Kvar")),
                new XElement("LastExecutedLabel", this.GetReportText(1060, "Senast utförd")),
                new XElement("AccountLabel", this.GetReportText(205, "Konto")));

            for (int i = 1; i <= Constants.NOOFDIMENSIONS; i++)
            {
                if (i > accountDimInternals.Count)
                {
                    parent.Add(
                        new XElement("AccountDim" + i + "ShortNameLabel", String.Empty));
                    continue;
                }

                AccountDim accountDim = accountDimInternals.Any() ? accountDimInternals.ElementAt(i - 1) : null;
                parent.Add(
                    new XElement("AccountDim" + i + "ShortNameLabel", accountDim?.ShortName ?? String.Empty));
            }
        }

        protected void CreateFixedAssetsPageHeaderLabelsElement(XElement parent)
        {
            parent.Add(
                new XElement("WriteOffMethodNameLabel", this.GetReportText(866, "Avskrivningsmetod")),
                new XElement("WriteOffMethodTypeLabel", this.GetReportText(869, "Avskrivningstyp")),
                new XElement("VoucherSeriesLabel", this.GetReportText(870, "Verifikatserie")),
                new XElement("SupplierInvoiceLabel", this.GetReportText(871, "Lev.faktura")),
                new XElement("CustomerInvoiceLabel", this.GetReportText(872, "Kundfaktura")),
                new XElement("InvoiceSeqNrLabel", this.GetReportText(873, "Löpnr")),
                new XElement("InvoiceNrLabel", this.GetReportText(874, "Fakturanr")),
                new XElement("InventoryNrLabel", this.GetReportText(875, "Tillgångsnr")),
                new XElement("InventoryNameLabel", this.GetReportText(876, "Namn")),
                new XElement("InventoryAccountLabel", this.GetReportText(205, "Konto")),
                new XElement("InventoryAccountNrLabel", this.GetReportText(8705, "Kontonr")),
                new XElement("InventoryAccountNameLabel", this.GetReportText(204, "Kontonamn")),
                new XElement("DescriptionLabel", this.GetReportText(877, "Beskrivning")),
                new XElement("NotesLabel", this.GetReportText(878, "Anteckningar")),
                new XElement("PurchaseDateLabel", this.GetReportText(879, "Anskaffningsdatum")),
                new XElement("WriteOffDateLabel", this.GetReportText(880, "Avskrivningsstart")),
                new XElement("PurchaseAmountLabel", this.GetReportText(881, "Anskaffningsvärde")),
                new XElement("WriteOffAmountLabel", this.GetReportText(882, "Avskrivningsvärde")),
                new XElement("WriteOffSumLabel", this.GetReportText(883, "Tidigare avskrivet")),
                new XElement("WriteOffRemainingAmountLabel", this.GetReportText(884, "Redovisat värde")),
                new XElement("EndAmountLabel", this.GetReportText(885, "Restvärde")),
                new XElement("PeriodTypeLabel", this.GetReportText(886, "Period")),
                new XElement("PeriodValueLabel", this.GetReportText(887, "Antal perioder")),
                new XElement("WriteOffPeriodsLabel", this.GetReportText(888, "Tidigare avskrivna perioder")),
                new XElement("CreatedLabel", this.GetReportText(335, "Skapad")),
                new XElement("CreatedByLabel", this.GetReportText(336, "Skapad av")),
                new XElement("ModifiedLabel", this.GetReportText(460, "Ändrad")),
                new XElement("ModifiedByLabel", this.GetReportText(461, "Ändrad av")),
                new XElement("VoucherNrLabel", this.GetReportText(889, "VerifikatNr")),
                new XElement("InventoryLogUserLabel", this.GetReportText(890, "Användare")),
                new XElement("InventoryLogDateLabel", this.GetReportText(891, "Datum")),
                new XElement("InventoryLogTypeLabel", this.GetReportText(892, "Typ")),
                new XElement("InventoryLogAmountLabel", this.GetReportText(893, "Belopp")),
                new XElement("InventoryFixedAssetLabel", this.GetReportText(3476, "Inventarie")),
                new XElement("InventoryTotalLabel", this.GetReportText(4936, "Totalt")),
                new XElement("InventoryFixedAssetNameLabel", this.GetReportText(2076, "Benämning")),
                new XElement("InventoryLogAmountCurrencyLabel", this.GetReportText(894, "Belopp valuta"))
                );
        }

        protected void CreateCustomerReportPageHeaderLabelsElement(XElement parent)
        {
            parent.Add(
                new XElement("SeqNrLabel", this.GetReportText(63, "Löpnr")),
                new XElement("InvoiceDateLabel", this.GetReportText(64, "Fakt.datum")),
                new XElement("DueDateLabel", this.GetReportText(65, "Förf.datum")),
                new XElement("BillingTypeLabel", this.GetReportText(66, "Typ")),
                new XElement("InvoiceTotalAmountLabel", this.GetReportText(67, "Fakturabelopp")),
                new XElement("InvoiceVATAmountLabel", this.GetReportText(202, "Moms")),
                new XElement("InvoiceCentRoundingLabel", this.GetReportText(203, "Öresavr.")),
                new XElement("PaymentsLabel", this.GetReportText(69, "Betalningar")),
                new XElement("PaymentDateLabel", this.GetReportText(75, "Bet.datum")),
                new XElement("PaymentMethodLabel", this.GetReportText(70, "Bet.sätt")),
                new XElement("PaymentAmountLabel", this.GetReportText(71, "Bet.belopp")),
                new XElement("BalanceLabel", this.GetReportText(72, "Saldo")),
                new XElement("CustomerInvoiceLabel", this.GetReportText(80, "Kundfaktura")),
                new XElement("OCRLabel", this.GetReportText(74, "OCR")),
                new XElement("CustomerLabel", this.GetReportText(210, "Kund")),
                new XElement("VoucherLabel", this.GetReportText(211, "Fakturans ver:")));
        }

        protected void CreateSupplierBalanceListPageHeaderLabelsElement(XElement parent)
        {
            parent.Add(
                new XElement("SeqNrLabel", this.GetReportText(63, "Löpnr")),
                new XElement("PaymentSuggestionNrLabel", this.GetReportText(298, "Försl nr")),
                new XElement("InvoiceDateLabel", this.GetReportText(64, "Fakt.datum")),
                new XElement("DueDateLabel", this.GetReportText(65, "Förf.datum")),
                new XElement("BillingTypeLabel", this.GetReportText(66, "Typ")),
                new XElement("InvoiceTotalAmountLabel", this.GetReportText(67, "Fakturabelopp")),
                new XElement("InvoiceVATAmountLabel", this.GetReportText(202, "Moms")),
                new XElement("PaymentsLabel", this.GetReportText(69, "Betalningar")),
                new XElement("PaymentDateLabel", this.GetReportText(75, "Bet.datum")),
                new XElement("PaymentMethodLabel", this.GetReportText(70, "Bet.sätt")),
                new XElement("PaymentAmountLabel", this.GetReportText(71, "Bet.belopp")),
                new XElement("BalanceLabel", this.GetReportText(72, "Saldo")),
                new XElement("SupplierInvoiceLabel", this.GetReportText(73, "Lev.faktura")),
                new XElement("OCRLabel", this.GetReportText(74, "OCR")),
                new XElement("SupplierLabel", this.GetReportText(209, "Leverantör")),
                new XElement("VoucherLabel", this.GetReportText(211, "Fakturans ver:")),
                new XElement("MatchedLabel", this.GetReportText(402, "Matchad")),
                new XElement("ProjectLabel", this.GetReportText(559, "Projekt")),
                new XElement("WorkSiteKeyLabel", this.GetReportText(560, "ArbetsPl.ID")),
                new XElement("WorkSiteNumberLabel", this.GetReportText(561, "ArbetsPl.Nr")),
                new XElement("DescriptionLabel", this.GetReportText(776, "Interntext")),
                new XElement("TimeDiscountLabel", this.GetReportText(779, "Kassarabatt")),
                new XElement("TimeDiscountDateLabel", this.GetReportText(777, "Rabatt datum")),
                new XElement("TimeDiscountPercentLabel", this.GetReportText(778, "Rabattprocent")),
                new XElement("InvoicesLabel", this.GetReportText(433, "Fakturor")),
                new XElement("NotBookedLabel", this.GetReportText(9229, "Under avprickning")),
                new XElement("UnpayedLabel", this.GetReportText(9230, "Obetalda")),
                new XElement("CurrencyLabel", this.GetReportText(97, "Valuta")),
                new XElement("ShowNotBookedLabel", this.GetReportText(9231, "Visa 'Ej avprickade' betalningar")),
                new XElement("PaymentProposalLabel", this.GetReportText(9232, "Betalningsförslag")),
                new XElement("SupplierNameLabel", this.GetReportText(108, "Namn")),
                new XElement("SupplierPaymentAccountLabel", this.GetReportText(1012, "Betalkonto")),
                new XElement("SupplierPaymentBicLabel", this.GetReportText(450, "BIC"))

                );
        }

        protected void CreateCustomerBalanceListPageHeaderLabelsElement(XElement parent)
        {
            parent.Add(
                new XElement("SeqNrLabel", this.GetReportText(63, "Löpnr")),
                new XElement("InvoiceDateLabel", this.GetReportText(64, "Fakt.datum")),
                new XElement("DueDateLabel", this.GetReportText(65, "Förf.datum")),
                new XElement("BillingTypeLabel", this.GetReportText(66, "Typ")),
                new XElement("InvoiceTotalAmountLabel", this.GetReportText(67, "Fakturabelopp")),
                new XElement("InvoiceVATAmountLabel", this.GetReportText(202, "Moms")),
                new XElement("InvoiceCentRoundingLabel", this.GetReportText(203, "Öresavr.")),
                new XElement("PaymentsLabel", this.GetReportText(69, "Betalningar")),
                new XElement("PaymentDateLabel", this.GetReportText(75, "Bet.datum")),
                new XElement("PaymentMethodLabel", this.GetReportText(70, "Bet.sätt")),
                new XElement("PaymentAmountLabel", this.GetReportText(71, "Bet.belopp")),
                new XElement("BalanceLabel", this.GetReportText(72, "Saldo")),
                new XElement("CustomerInvoiceLabel", this.GetReportText(80, "Kundfaktura")),
                new XElement("OCRLabel", this.GetReportText(74, "OCR")),
                new XElement("CustomerLabel", this.GetReportText(210, "Kund")),
                new XElement("VoucherLabel", this.GetReportText(211, "Fakturans ver:")),
                new XElement("MatchedLabel", this.GetReportText(402, "Matchad")),
                new XElement("DescriptionLabel", this.GetReportText(776, "Interntext")));
        }

        protected void CreateCustomerInvoiceJournalPageHeaderLabelsElement(XElement parent)
        {
            parent.Add(
                new XElement("SeqNrLabel", this.GetReportText(63, "Löpnr")),
                new XElement("InvoiceDateLabel", this.GetReportText(64, "Fakt.datum")),
                new XElement("DueDateLabel", this.GetReportText(65, "Förf.datum")),
                new XElement("BillingTypeLabel", this.GetReportText(66, "Typ")),
                new XElement("InvoiceTotalAmountLabel", this.GetReportText(67, "Fakturabelopp")),
                new XElement("InvoiceVATAmountLabel", this.GetReportText(202, "Moms")),
                new XElement("InvoiceCentRoundingLabel", this.GetReportText(203, "Öresavr.")),
                new XElement("PaymentsLabel", this.GetReportText(69, "Betalningar")),
                new XElement("PaymentDateLabel", this.GetReportText(75, "Bet.datum")),
                new XElement("PaymentMethodLabel", this.GetReportText(70, "Bet.sätt")),
                new XElement("PaymentAmountLabel", this.GetReportText(71, "Bet.belopp")),
                new XElement("BalanceLabel", this.GetReportText(72, "Saldo")),
                new XElement("CustomerInvoiceLabel", this.GetReportText(80, "Kundfaktura")),
                new XElement("OCRLabel", this.GetReportText(74, "OCR")),
                new XElement("CustomerLabel", this.GetReportText(210, "Kund")),
                new XElement("VoucherLabel", this.GetReportText(211, "Fakturans ver:")),
                new XElement("ExclusiveVATLabel", this.GetReportText(212, "Exkl.moms")),
                new XElement("AccountNameLabel", this.GetReportText(204, "Kontonamn")),
                new XElement("AccountLabel", this.GetReportText(205, "Konto")),
                new XElement("DebitLabel", this.GetReportText(206, "Debet")),
                new XElement("CreditLabel", this.GetReportText(207, "Kredit")),
                new XElement("QuantityLabel", this.GetReportText(208, "Antal")),
                new XElement("HouseholdTaxDeductionLabel", this.GetReportText(260, "Husavdrag")),
                new XElement("MatchedLabel", this.GetReportText(402, "Matchad")),
                new XElement("DescriptionLabel", this.GetReportText(776, "Interntext")));
        }

        protected void CreateCustomerPaymentJournalPageHeaderLabelsElement(XElement parent)
        {
            parent.Add(
                new XElement("SeqNrLabel", this.GetReportText(63, "Löpnr")),
                new XElement("InvoiceDateLabel", this.GetReportText(64, "Fakt.datum")),
                new XElement("DueDateLabel", this.GetReportText(65, "Förf.datum")),
                new XElement("PayDateLabel", this.GetReportText(262, "Betaldatum")), //Differ (added) from CustomerInvoiceJournal
                new XElement("BillingTypeLabel", this.GetReportText(66, "Typ")),
                new XElement("InvoiceTotalAmountLabel", this.GetReportText(67, "Fakturabelopp")),
                new XElement("InvoiceVATAmountLabel", this.GetReportText(202, "Moms")),
                new XElement("InvoiceCentRoundingLabel", this.GetReportText(203, "Öresavr.")),
                new XElement("PaymentsLabel", this.GetReportText(69, "Betalningar")),
                new XElement("PaymentDateLabel", this.GetReportText(75, "Bet.datum")),
                new XElement("PaymentMethodLabel", this.GetReportText(70, "Bet.sätt")),
                new XElement("PaymentAmountLabel", this.GetReportText(71, "Bet.belopp")),
                new XElement("BalanceLabel", this.GetReportText(72, "Saldo")),
                new XElement("CustomerInvoiceLabel", this.GetReportText(80, "Kundfaktura")),
                new XElement("PaymentLabel", this.GetReportText(261, "Betalning")), //Differ (added) from CustomerInvoiceJournal
                new XElement("OCRLabel", this.GetReportText(74, "OCR")),
                new XElement("CustomerLabel", this.GetReportText(210, "Kund")),
                new XElement("VoucherLabel", this.GetReportText(263, "Betalningens ver:")), //Differ (changed) from CustomerInvoiceJournal
                new XElement("ExclusiveVATLabel", this.GetReportText(212, "Exkl.moms")),
                new XElement("AccountNameLabel", this.GetReportText(204, "Kontonamn")),
                new XElement("AccountLabel", this.GetReportText(205, "Konto")),
                new XElement("DebitLabel", this.GetReportText(206, "Debet")),
                new XElement("CreditLabel", this.GetReportText(207, "Kredit")),
                new XElement("QuantityLabel", this.GetReportText(208, "Antal")),
                new XElement("HouseholdTaxDeductionLabel", this.GetReportText(260, "Husavdrag")));
        }

        protected void CreateInterestRateCalculationPageHeaderLabelsElement(XElement parent)
        {
            parent.Add(
                new XElement("InvoiceNrLabel", this.GetReportText(89, "Fakturanr")),
                new XElement("InvoiceDateLabel", this.GetReportText(64, "Fakt.datum")),
                new XElement("DueDateLabel", this.GetReportText(65, "Förf.datum")),
                new XElement("InvoiceTotalAmountLabel", this.GetReportText(67, "Fakturabelopp")),
                new XElement("PaymentNrLabel", this.GetReportText(933, "Bet.löp")),
                new XElement("PaymentDateLabel", this.GetReportText(75, "Bet.datum")),
                new XElement("PaymentAmountLabel", this.GetReportText(71, "Bet.belopp")),
                new XElement("BalanceLabel", this.GetReportText(72, "Saldo")),
                new XElement("RateDaysLabel", this.GetReportText(931, "Räntedagar")),
                new XElement("RateAmountLabel", this.GetReportText(932, "Ränta"))
                );
        }

        protected void CreateSupplierInvoiceJournalPageHeaderLabelsElement(XElement parent)
        {
            parent.Add(
                new XElement("SeqNrLabel", this.GetReportText(63, "Löpnr")),
                new XElement("InvoiceDateLabel", this.GetReportText(64, "Fakt.datum")),
                new XElement("DueDateLabel", this.GetReportText(65, "Förf.datum")),
                new XElement("BillingTypeLabel", this.GetReportText(66, "Typ")),
                new XElement("InvoiceTotalAmountLabel", this.GetReportText(67, "Fakturabelopp")),
                new XElement("InvoiceVATAmountLabel", this.GetReportText(202, "Moms")),
                new XElement("PaymentsLabel", this.GetReportText(69, "Betalningar")),
                new XElement("PaymentDateLabel", this.GetReportText(75, "Bet.datum")),
                new XElement("PaymentMethodLabel", this.GetReportText(70, "Bet.sätt")),
                new XElement("PaymentAmountLabel", this.GetReportText(71, "Bet.belopp")),
                new XElement("BalanceLabel", this.GetReportText(72, "Saldo")),
                new XElement("SupplierInvoiceLabel", this.GetReportText(73, "Lev.faktura")),
                new XElement("OCRLabel", this.GetReportText(74, "OCR")),
                new XElement("SupplierLabel", this.GetReportText(209, "Leverantör")),
                new XElement("VoucherLabel", this.GetReportText(211, "Fakturans ver:")),
                new XElement("ExclusiveVATLabel", this.GetReportText(212, "Exkl.moms")),
                new XElement("AccountNameLabel", this.GetReportText(204, "Kontonamn")),
                new XElement("AccountLabel", this.GetReportText(205, "Konto")),
                new XElement("DebitLabel", this.GetReportText(206, "Debet")),
                new XElement("CreditLabel", this.GetReportText(207, "Kredit")),
                new XElement("QuantityLabel", this.GetReportText(208, "Antal")),
                new XElement("DescriptionLabel", this.GetReportText(776, "Interntext")));
        }

        protected void CreateSupplierPaymentJournalPageHeaderLabelsElement(XElement parent)
        {
            parent.Add(
                new XElement("SeqNrLabel", this.GetReportText(63, "Löpnr")),
                new XElement("InvoiceDateLabel", this.GetReportText(64, "Fakt.datum")),
                new XElement("DueDateLabel", this.GetReportText(65, "Förf.datum")),
                new XElement("PayDateLabel", this.GetReportText(262, "Betaldatum")), //Differ (added) from SupplierInvoiceJournal
                new XElement("BillingTypeLabel", this.GetReportText(66, "Typ")),
                new XElement("InvoiceTotalAmountLabel", this.GetReportText(67, "Fakturabelopp")),
                new XElement("InvoiceVATAmountLabel", this.GetReportText(202, "Moms")),
                new XElement("PaymentsLabel", this.GetReportText(69, "Betalningar")),
                new XElement("PaymentDateLabel", this.GetReportText(75, "Bet.datum")),
                new XElement("PaymentMethodLabel", this.GetReportText(70, "Bet.sätt")),
                new XElement("PaymentAmountLabel", this.GetReportText(71, "Bet.belopp")),
                new XElement("BalanceLabel", this.GetReportText(72, "Saldo")),
                new XElement("SupplierInvoiceLabel", this.GetReportText(73, "Lev.faktura")),
                new XElement("PaymentLabel", this.GetReportText(261, "Betalning")), //Differ (added) from SupplierInvoiceJournal
                new XElement("OCRLabel", this.GetReportText(74, "OCR")),
                new XElement("SupplierLabel", this.GetReportText(209, "Leverantör")),
                new XElement("VoucherLabel", this.GetReportText(263, "Betalningens ver:")), //Differ (changed) from SupplierInvoiceJournal
                new XElement("ExclusiveVATLabel", this.GetReportText(212, "Exkl.moms")),
                new XElement("AccountNameLabel", this.GetReportText(204, "Kontonamn")),
                new XElement("AccountLabel", this.GetReportText(205, "Konto")),
                new XElement("DebitLabel", this.GetReportText(206, "Debet")),
                new XElement("CreditLabel", this.GetReportText(207, "Kredit")),
                new XElement("QuantityLabel", this.GetReportText(208, "Antal")),
                new XElement("IncludePaymentsNotBockedLabel", this.GetReportText(9234, "Inkludera betalningar under avprickning")),
                new XElement("BockedLabel", this.GetReportText(9235, "Avprickade")),
                new XElement("NotBockedLabel", this.GetReportText(9229, "Under avprickning")),
                new XElement("NotBockedOrPaymentProposalLabel", this.GetReportText(9236, "Under avprickning eller betalförslag"))
                );
        }

        protected void CreateSEPAPaymentImportPageHeaderLabelsElement(XElement parent)
        {
            parent.Add(
                new XElement("PaymentMethodNameLabel", this.GetReportText(823, "Bet.sätt")),
                new XElement("PaymentMethodPaymentNrLabel", this.GetReportText(824, "Bet.nr")),
                new XElement("FileNameLabel", this.GetReportText(825, "Filnamn")),
                new XElement("CustomerNrLabel", this.GetReportText(826, "Kundnr")),
                new XElement("CustomerNameLabel", this.GetReportText(827, "Kundnamn")),
                new XElement("PayerNameLabel", this.GetReportText(934, "Betalare")),
                new XElement("PaymentNrLabel", this.GetReportText(828, "Löpnr")),
                new XElement("BookingDateLabel", this.GetReportText(829, "Fakt.datum")),
                new XElement("PaymentDateLabel", this.GetReportText(830, "Bet.datum")),
                new XElement("BatchIdLabel", this.GetReportText(831, "Arkivnr.")),
                new XElement("InvoiceNrLabel", this.GetReportText(832, "Fakt.nr")),
                new XElement("OCRLabel", this.GetReportText(833, "OCR")),
                new XElement("PaymentAmountLabel", this.GetReportText(834, "Bet.belopp")),
                new XElement("CorrectionLabel", this.GetReportText(835, "Notering")),
                new XElement("ErrorMessageLabel", this.GetReportText(836, "Felmeddelande")),
                new XElement("NoteLabel", this.GetReportText(837, "Intern. text")));
        }

        protected void CreateBillingInvoiceHeadReportHeaderLabelsElement(XElement parent, SoeReportTemplateType reportTemplateType, bool doPrintTaxBillText)
        {
            string invoiceNrLabel = string.Empty;
            string invoiceDateLabel = string.Empty;
            string invoiceDueDateLabel = this.GetReportText(101, "Förfallodatum");

            switch (reportTemplateType)
            {
                case SoeReportTemplateType.BillingOffer:
                    invoiceNrLabel = this.GetReportText(254, "Offertnr");
                    invoiceDateLabel = this.GetReportText(257, "Offertdatum");
                    invoiceDueDateLabel = this.GetReportText(451, "Giltig tom");
                    break;
                case SoeReportTemplateType.BillingOrder:
                case SoeReportTemplateType.BillingOrderOverview:
                    invoiceNrLabel = this.GetReportText(255, "Ordernr");
                    invoiceDateLabel = this.GetReportText(256, "Orderdatum");
                    break;
                case SoeReportTemplateType.BillingContract:
                    invoiceNrLabel = this.GetReportText(241, "Avtalsnr");
                    invoiceDateLabel = this.GetReportText(242, "Avtalsdatum");
                    invoiceDueDateLabel = this.GetReportText(259, "Avtalsslut");
                    break;
                case SoeReportTemplateType.BillingInvoice:
                case SoeReportTemplateType.BillingInvoiceInterest:
                case SoeReportTemplateType.BillingInvoiceReminder:
                    invoiceNrLabel = this.GetReportText(89, "Fakturanr");
                    invoiceDateLabel = this.GetReportText(100, "Fakturadatum");
                    break;
            }

            parent.Add(
                //Customer
                new XElement("CustomerNameLabel", this.GetReportText(84, "Kund")),
                new XElement("CustomerNrLabel", this.GetReportText(85, "Kundnr")),
                new XElement("CustomerGroupLabel", this.GetReportText(436, "Kundkategori")),
                new XElement("CustomerGroupParentLabel", this.GetReportText(555, "Huvud kundkategori")),
                new XElement("CustomerOrgNrLabel", this.GetReportText(86, "Organisationsnr")),
                new XElement("CustomerVatNrLabel", this.GetReportText(87, "VAT-nr")),
                new XElement("CustomerDeliveryAddressLabel", this.GetReportText(88, "Leveransadress")),
                new XElement("CustomerRegDateLabel", this.GetReportText(10104, "Första regdatum")),
                //Invoice
                new XElement("InvoiceNrLabel", invoiceNrLabel),
                new XElement("InvoiceSeqNrLabel", this.GetReportText(129, "Löpnr")),
                new XElement("InvoiceBillingTypeLabel", this.GetReportText(90, "Fakturatyp")),
                new XElement("InvoiceVatTypLabel", this.GetReportText(91, "Momstyp")),
                new XElement("InvoiceOCRLabel", this.GetReportText(92, "OCR")),
                new XElement("InvoiceSumAmountLabel", this.GetReportText(137, "Summa")),
                new XElement("InvoiceVatAmountLabel", this.GetReportText(95, "Momsbelopp")),
                new XElement("InvoiceFeeAmountLabel", this.GetReportText(138, "Fakt.avgift")),
                new XElement("InvoiceFreightAmountLabel", this.GetReportText(96, "Fraktkostnad")),
                new XElement("InvoiceCentRoundingLabel", this.GetReportText(142, "Öresutj.")),
                new XElement("InvoiceDescriptionLabel", this.GetReportText(287, "Intern text")),
                new XElement("InvoiceWorkingDescriptionLabel", this.GetReportText(403, "Arbetsbeskrivning")),
                new XElement("InvoiceTotalAmountLabel", this.GetReportText(93, "Att betala")),
                new XElement("InvoiceTotalAmountCurrencyLabel", this.GetReportText(94, "Att betala valuta")),
                new XElement("InvoiceTotalAmountCreditLabel", this.GetReportText(146, "Att erhålla")),
                new XElement("InvoiceTotalAmountCreditCurrencyLabel", this.GetReportText(147, "Att erhålla valuta")),
                new XElement("InvoiceCurrencyLabel", this.GetReportText(97, "Valuta")),
                new XElement("InvoiceCurrencyRateLabel", this.GetReportText(98, "Kurs")),
                new XElement("InvoiceCurrencyDateLabel", this.GetReportText(99, "Valutadatum")),
                new XElement("InvoiceDateLabel", invoiceDateLabel),
                new XElement("InvoiceDueDateLabel", invoiceDueDateLabel),
                new XElement("InvoiceVoucherDateLabel", this.GetReportText(102, "Bokföringsdatum")),
                new XElement("InvoiceDeliveryDateLabel", this.GetReportText(103, "Leveransdatum")),
                new XElement("InvoiceOrderDateLabel", this.GetReportText(145, "Beställningsdatum")),
                new XElement("InvoiceReferenceYourLabel", this.GetReportText(105, "Er referens")),
                new XElement("InvoiceReferenceOurLabel", this.GetReportText(106, "Vår referens")),
                new XElement("InvoiceReferenceOrderLabel", this.GetReportText(944, "Orderreferens")),
                new XElement("InvoiceLabelLabel", this.GetReportText(294, "Märkning")),
                new XElement("InvoicePaymentConditionLabel", this.GetReportText(104, "Betalningsvillkor")),
                new XElement("InvoiceDeliveryConditionLabel", this.GetReportText(132, "Leveransvillkor")),
                new XElement("InvoiceDeliveryTypeLabel", this.GetReportText(133, "Leveranssätt")),
                new XElement("InvoiceFTaxLabel", doPrintTaxBillText ? this.GetReportText(139, "Godkänd för F-skatt") : String.Empty),
                new XElement("InvoiceOrderNrLabel", this.GetReportText(265, "Ordernr")),
                new XElement("InvoiceContractNrLabel", this.GetReportText(241, "Avtalsnr")),
                new XElement("InvoiceIsCashSaleLabel", this.GetReportText(406, "Kontant faktura")),
                new XElement("RemainingAmountLabel", this.GetReportText(407, "Kvar att fakturera")),
                new XElement("PaidAmountLabel", this.GetReportText(945, "Tidigare betalt")),
                new XElement("RemainingAmount3Label", this.GetReportText(946, "Kvarstår")),
                //Months
                new XElement("MonthLabel", this.GetReportText(405, "Månad")),
                new XElement("PeriodLabel", this.GetReportText(496, "Period")),
                new XElement("MonthJanuaryLabel", this.GetReportText(497, "Januari")),
                new XElement("MonthFebruaryLabel", this.GetReportText(498, "Februari")),
                new XElement("MonthMarchLabel", this.GetReportText(499, "Mars")),
                new XElement("MonthAprilLabel", this.GetReportText(500, "April")),
                new XElement("MonthMayLabel", this.GetReportText(501, "Maj")),
                new XElement("MonthJuneLabel", this.GetReportText(502, "Juni")),
                new XElement("MonthJulyLabel", this.GetReportText(503, "Juli")),
                new XElement("MonthAugustLabel", this.GetReportText(504, "Augusti")),
                new XElement("MonthSeptemberLabel", this.GetReportText(505, "September")),
                new XElement("MonthOctoberLabel", this.GetReportText(506, "Oktober")),
                new XElement("MonthNovemberLabel", this.GetReportText(507, "November")),
                new XElement("MonthDecemberLabel", this.GetReportText(508, "December")),
                new XElement("MonthJanuaryShortLabel", this.GetReportText(509, "Jan")),
                new XElement("MonthFebruaryShortLabel", this.GetReportText(510, "Feb")),
                new XElement("MonthMarchShortLabel", this.GetReportText(511, "Mar")),
                new XElement("MonthAprilShortLabel", this.GetReportText(512, "Apr")),
                new XElement("MonthMayShortLabel", this.GetReportText(513, "Maj")),
                new XElement("MonthJuneShortLabel", this.GetReportText(514, "Jun")),
                new XElement("MonthJulyShortLabel", this.GetReportText(515, "Jul")),
                new XElement("MonthAugustShortLabel", this.GetReportText(516, "Aug")),
                new XElement("MonthSeptemberShortLabel", this.GetReportText(517, "Sep")),
                new XElement("MonthOctoberShortLabel", this.GetReportText(518, "Okt")),
                new XElement("MonthNovemberShortLabel", this.GetReportText(519, "Nov")),
                new XElement("MonthDecemberShortLabel", this.GetReportText(520, "Dec")),
                new XElement("YearLabel", this.GetReportText(521, "År")),
                //Statistics
                new XElement("OrderLabel", this.GetReportText(349, "Order")),
                new XElement("InvoiceLabel", this.GetReportText(522, "Faktura")),
                new XElement("OfferLabel", this.GetReportText(348, "Offert")),
                new XElement("ContractLabel", this.GetReportText(524, "Avtal")),
                new XElement("OrderRowsNotInvoicedLabel", this.GetReportText(525, "Ej fakturerade orderrader")),
                new XElement("OrderRowsInvoicedLabel", this.GetReportText(526, "Fakturerade orderrader")),
                new XElement("BilledLabel", this.GetReportText(527, "Fakturerade")),
                new XElement("MarginLabel", this.GetReportText(528, "Täckningsbidrag")),
                new XElement("MarginShortLabel", this.GetReportText(529, "T-bidrag")),
                new XElement("MarginPercentLabel", this.GetReportText(530, "Täckningsgrad")),
                new XElement("MarginPercentShortLabel", this.GetReportText(531, "T-grad")),
                new XElement("PayedLabel", this.GetReportText(408, "Betald")),
                new XElement("CostLabel", this.GetReportText(412, "Kostnad")),
                new XElement("RevenueLabel", this.GetReportText(532, "Intäkt")),
                new XElement("WorkLabel", this.GetReportText(533, "Arbete")),
                new XElement("MiscLabel", this.GetReportText(534, "Övrigt")),
                new XElement("AccLabel", this.GetReportText(535, "Upparbetat")),
                new XElement("IsBilledLabel", this.GetReportText(536, "Fakturerat")),
                new XElement("TotalLabel", this.GetReportText(537, "Total")),
                new XElement("MaterialLabel", this.GetReportText(538, "Material")),
                new XElement("HouseDeductionLabel", this.GetReportText(539, "Rot-avdrag")),
                new XElement("HouseDeductionShortLabel", this.GetReportText(540, "Rot")),
                new XElement("AverageLabel", this.GetReportText(541, "Snitt")),
                new XElement("MedianLabel", this.GetReportText(542, "Median")),
                new XElement("MaxPurchasePriceLabel", this.GetReportText(543, "Max inköpspris")),
                new XElement("MinPurchasePriceLabel", this.GetReportText(544, "Min inköpspris")),
                new XElement("ProfitLabel", this.GetReportText(545, "Vinst")),
                new XElement("LossLabel", this.GetReportText(546, "Förlust")),
                new XElement("WholesellerLabel", this.GetReportText(547, "Grossist")),
                new XElement("PackingslipLabel", this.GetReportText(7415, "Följesedel")),
                new XElement("PicklistLabel", this.GetReportText(7416, "Plocklista")),
                new XElement("InvoiceRetainedGuaranteeAmountLabel", this.GetReportText(7417, "Totalt innehållna medel")),
                new XElement("InvoiceCustomerAdressLabel", this.GetReportText(441, "Fakturaadress")),
                new XElement("InvoiceReminderFeeLabel", this.GetReportText(7418, "Påminnelseavgift")),
                new XElement("InvoiceAccruedInterestLabel", this.GetReportText(7419, "Upplupen ränta"))
                );

        }

        protected void CreateBillingInvoiceRowsReportHeaderLabelsElement(XElement parent, SoeReportTemplateType reportTemplateType)
        {
            parent.Add(
                new XElement("InvoiceRowProductNumberLabel", this.GetReportText(107, "Artikel")),
                new XElement("InvoiceRowProductNameLabel", this.GetReportText(108, "Namn")),
                new XElement("InvoiceRowProductDescriptionLabel", this.GetReportText(109, "Beskrivning")),
                new XElement("InvoiceRowProductUnitLabel", this.GetReportText(131, "Enhet")),
                new XElement("InvoiceRowQuantityLabel", this.GetReportText(130, "Antal")),
                new XElement("InvoiceRowInvoiceQuantityLabel", this.GetReportText(130, "Antal")),
                new XElement("InvoiceRowAmountLabel", this.GetReportText(110, "A pris")),
                new XElement("InvoiceRowVatAmountLabel", this.GetReportText(113, "Moms")),
                new XElement("InvoiceRowSumAmountLabel", this.GetReportText(111, "Belopp")),
                new XElement("InvoiceRowSumAmountCurrencyLabel", this.GetReportText(112, "Belopp valuta")),
                new XElement("InvoiceRowDiscountPercentLabel", this.GetReportText(114, "Rabatt %")),
                new XElement("InvoiceRowDiscountAmountLabel", this.GetReportText(115, "Rabatt")),
                new XElement("InvoiceRowDiscount2PercentLabel", this.GetReportText(1054, "Rabatt 2 %")),
                new XElement("InvoiceRowDiscount2AmountLabel", this.GetReportText(115, "Rabatt") + " 2"),
                new XElement("InvoiceRowTextLabel", this.GetReportText(116, "Text")),
                new XElement("InvoiceRowProductGroupLabel", this.GetReportText(556, "Produktgrupp")),
                new XElement("InvoiceRowProductCategoryLabel", this.GetReportText(557, "Artikelkategori")),
                new XElement("InvoiceRowProductCategoryParentLabel", this.GetReportText(558, "Huvud artikelkategori")),
                new XElement("StatusLabel", this.GetReportText(324, "Status")),
                new XElement("TenderSumLabel", this.GetReportText(452, "Anbudssumma")),
                new XElement("PreviousInvoicedLabel", this.GetReportText(453, "Tidigare fakturerat")),
                new XElement("PreviousInvoicedFromOrderLabel", this.GetReportText(454, "Tidigare fakturerat från order")),
                new XElement("LiftLabel", this.GetReportText(455, "Lyft")),
                new XElement("LiftThisLiftLabel", this.GetReportText(456, "Detta lyft")),
                new XElement("RemainingAmount2Label", this.GetReportText(457, "Återstående belopp")),
                new XElement("SafeGuardAmountLabel", this.GetReportText(458, "Garantibelopp")),
                new XElement("PageSingleLabel", this.GetReportText(4, "Sida:")),
                new XElement("PageMultipleLabel", this.GetReportText(459, "Sidor")),
                new XElement("CreatedLabel", this.GetReportText(335, "Skapad")),
                new XElement("CreatedByLabel", this.GetReportText(336, "Skapad av")),
                new XElement("ModifiedLabel", this.GetReportText(460, "Ändrad")),
                new XElement("ModifiedByLabel", this.GetReportText(461, "Ändrad av")),
                new XElement("RowDeliveryDateLabel", this.GetReportText(462, "Leveransdatum")),
                new XElement("RowLiftDateLabel", this.GetReportText(463, "Lyftdatum")),
                new XElement("RowDeliveradQuantityLabel", this.GetReportText(464, "Levererat antal")),
                new XElement("RemainingQuantityLabel", this.GetReportText(465, "Återstående antal")),
                new XElement("ProjectLabel", this.GetReportText(466, "Projekt")),
                new XElement("EmployeeNrLabel", this.GetReportText(151, "Anst.nr:")),
                new XElement("EmployeeNameLabel", this.GetReportText(152, "Namn:")),
                new XElement("NoteLabel", this.GetReportText(249, "Anteckningar")),
                new XElement("DateLabel", this.GetReportText(250, "Datum")),
                new XElement("InvoiceTimeInMinutesLabel", this.GetReportText(251, "Fakturerad tid")),
                new XElement("TimeCodeLabel", this.GetReportText(321, "Tidkod")),
                new XElement("HourLabel", this.GetReportText(467, "Timmar"))
                );
            if (reportTemplateType == SoeReportTemplateType.BillingInvoice)
            {
                parent.Add(new XElement("HouseholdDeductionLabel", this.GetReportText(9250, "Arbete som berättigar till hushållsavdrag")),
                new XElement("HouseholdDeductionTypeLabel", this.GetReportText(9251, "Typ av arbete")));
            }

        }

        protected void CreateChecklistsReportPageHeaderLabelsElement(XElement parent)
        {
            parent.Add(
                new XElement("ChecklistLabel", this.GetReportText(306, "Checklista")),
                new XElement("OrderLabel", this.GetReportText(307, "Order")),
                new XElement("ProjectLabel", this.GetReportText(308, "Projekt")),
                new XElement("SupervisorLabel", this.GetReportText(309, "Arbetsledare")),
                new XElement("ChiefLabel", this.GetReportText(310, "Chef")),
                new XElement("WorkerLabel", this.GetReportText(311, "Montör")),
                new XElement("CalculatorLabel", this.GetReportText(312, "Kalkylator")),
                new XElement("ReferenceLabel", this.GetReportText(313, "Referens")),
                new XElement("FixedLabel", this.GetReportText(314, "Åtgärdat")),
                new XElement("DateLabel", this.GetReportText(315, "Datum")),
                new XElement("SignLabel", this.GetReportText(316, "Signatur")));
        }

        protected void CreateProjectTransactionsReportPageHeaderLabelsElement(XElement parent)
        {
            parent.Add(
                new XElement("NameLabel", this.GetReportText(322, "Namn")),
                new XElement("DescriptionLabel", this.GetReportText(323, "Beskrivning")),
                new XElement("StatusLabel", this.GetReportText(324, "Status")),
                new XElement("TypeLabel", this.GetReportText(325, "Typ")),
                new XElement("StartLabel", this.GetReportText(326, "Start")),
                new XElement("StopLabel", this.GetReportText(327, "Stopp")),
                new XElement("QuantityLabel", this.GetReportText(328, "Kvantitet")),
                new XElement("AmountLabel", this.GetReportText(329, "Belopp")),
                new XElement("VatAmountLabel", this.GetReportText(330, "Momsbelopp")),
                new XElement("VatRateLabel", this.GetReportText(331, "Momssats")),
                new XElement("CurrencyLabel", this.GetReportText(332, "Valuta")),
                new XElement("LedgerCurrencyLabel", this.GetReportText(333, "Reskontravaluta")),
                new XElement("EntCurrencyLabel", this.GetReportText(334, "Koncernvaluta")),
                new XElement("CreatedLabel", this.GetReportText(335, "Skapad")),
                new XElement("CreatedByLabel", this.GetReportText(336, "Skapad av")),
                new XElement("ProjectLabel", this.GetReportText(337, "Projekt")),
                new XElement("CustomerNrLabel", this.GetReportText(338, "Kundnr")),
                new XElement("CustomerNameLabel", this.GetReportText(339, "Kundnamn")),
                new XElement("ProductNrLabel", this.GetReportText(340, "Artikelnr")),
                new XElement("ProductNameLabel", this.GetReportText(341, "Artikelnamn")),
                new XElement("ProductUnitLabel", this.GetReportText(342, "Enhet")),
                new XElement("EmployeeNrLabel", this.GetReportText(343, "Anställningsnr")),
                new XElement("EmployeeNameLabel", this.GetReportText(344, "Anställd")),
                new XElement("TimeCodeTransactionLabel", this.GetReportText(345, "Tidkodstransaktion")),
                new XElement("TimeInvoiceTransactionLabel", this.GetReportText(346, "Artikeltransaktion")),
                new XElement("TimePayrollTransactionLabel", this.GetReportText(347, "Löneartstransaktion")),
                new XElement("OfferLabel", this.GetReportText(348, "Offert")),
                new XElement("OrderLabel", this.GetReportText(349, "Order")),
                new XElement("InvoiceLabel", this.GetReportText(350, "Invoice")),
                new XElement("OfferNrLabel", this.GetReportText(351, "Offertnr")),
                new XElement("OrderNrLabel", this.GetReportText(352, "OrderNr")),
                new XElement("InvoiceNrLabel", this.GetReportText(353, "Fakturanr")),
                new XElement("OfferDateLabel", this.GetReportText(354, "Offertdatum")),
                new XElement("OrderDateLabel", this.GetReportText(355, "Orderdatum")),
                new XElement("InvoiceDateLabel", this.GetReportText(356, "Fakturadatum")),
                new XElement("TransactionDateLabel", this.GetReportText(357, "Transaktionsdatum")),
                new XElement("BillingRowIsTimeProjectRowLabel", this.GetReportText(358, "Tidrad")),
                new XElement("CommentLabel", this.GetReportText(359, "Kommentar")),
                new XElement("BillableMinutesInvoicedLabel", this.GetReportText(417, "Debiterbara timmar, fakturerat")),
                new XElement("BillableMinutesNotInvoicedLabel", this.GetReportText(418, "Debiterbara timmar, ej fakturerat")),
                new XElement("PersonellIncomeInvoicedLabel", this.GetReportText(419, "Intäkter personal, fakturerat")),
                new XElement("PersonellIncomeNotInvoicedLabel", this.GetReportText(420, "Intäkter personal, ej fakturerat")),
                new XElement("MaterialIncomeInvoicedLabel", this.GetReportText(421, "Intäkter material, fakturerat")),
                new XElement("MaterialIncomeNotInvoicedLabel", this.GetReportText(422, "Intäkter material, ej fakturerat")),
                new XElement("IncomeInvoicedLabel", this.GetReportText(423, "Intäkter, fakturerat")),
                new XElement("IncomeNotInvoicedLabel", this.GetReportText(424, "Intäkter, ej fakturerat")),
                new XElement("PersonellCostLabel", this.GetReportText(425, "Personalkostnad")),
                new XElement("MaterialCostLabel", this.GetReportText(426, "Materialkostnad")),
                new XElement("ExpensesLabel", this.GetReportText(427, "Utlägg")),
                new XElement("OverheadCostLabel", this.GetReportText(814, "Overheadkostnad")),
                new XElement("CostLabel", this.GetReportText(428, "Kostnad")),
                new XElement("ResultLabel", this.GetReportText(429, "Resultat")),
                new XElement("BudgetLabel", this.GetReportText(430, "Budget")),
                new XElement("OutcomeLabel", this.GetReportText(431, "Utfall")),
                new XElement("DeviationLabel", this.GetReportText(432, "Avvikelse")),
                new XElement("PreliminaryLabel", this.GetReportText(566, "Preliminärt")),
                new XElement("IncomeLabel", this.GetReportText(5200, "Intäkt")),
                new XElement("WorkHoursLabel", this.GetReportText(4911, "Arbetade timmar")),
                new XElement("NotInvoicedHoursLabel", this.GetReportText(4912, "Ej fakturerade timmar")),
                new XElement("OfferSumLabel", this.GetReportText(4913, "Anbudssumma")),
                new XElement("ProjectDescriptionLabel", this.GetReportText(4914, "Projektbeskrivning")),
                new XElement("MaterielLabel", this.GetReportText(4915, "Material")),
                new XElement("ProjectWorkHoursLabel", this.GetReportText(4916, "Arb. tim")),
                new XElement("ProjectOtherLabel", this.GetReportText(4917, "Övrigt")),
                new XElement("ProjectTotalLabel", this.GetReportText(4918, "Total")),
                new XElement("ProjectCompletionLabel", this.GetReportText(4919, "Färdig grad")),
                new XElement("ProjectFinalAmountLabel", this.GetReportText(4920, "Färdig bel.")),
                new XElement("ProjectTenderLabel", this.GetReportText(4921, "Anbud")),
                new XElement("ProjectInvoiceHoursLabel", this.GetReportText(4922, "Faktbar. tim")),
                new XElement("ProjectNotInvoicedLabel", this.GetReportText(4923, "Ej fakturerat")),
                new XElement("ProjectInvoicedLabel", this.GetReportText(4924, "Fakturerat")),
                new XElement("ProjectWorkInProgressLabel", this.GetReportText(4925, "Påg. arbete")),
                new XElement("ProjectGrossMarginLabel", this.GetReportText(4926, "TB")),
                new XElement("ProjectGrossCoverageLabel", this.GetReportText(4927, "TG")),
                new XElement("ProjectOBLabel", this.GetReportText(4928, "IB")),
                new XElement("ProjectWorkLabel", this.GetReportText(8100, "Arbete")),
                new XElement("ProjectCalculatedCostLabel", this.GetReportText(4930, "Kalk.kostnad")),
                new XElement("ProjectFinnishedWorkLabel", this.GetReportText(4931, "Upparbetat")),
                new XElement("ProjectRemindingWorkLabel", this.GetReportText(4932, "Rest")),
                new XElement("ProjectOngoingLabel", this.GetReportText(4933, "Löpande")),
                new XElement("ProjectFixedPriceLabel", this.GetReportText(4934, "Fastpris")),
                new XElement("ProjectBudgetCostLabel", this.GetReportText(4935, "Budg. kostnad")),
                new XElement("ProjectGrandTotalLabel", this.GetReportText(4936, "Totalt")),
                new XElement("ProjectHoursLabel", this.GetReportText(3654, "Timmar")),
                new XElement("ProjectHoursCostLabel", this.GetReportText(4937, "Timkostnad")),
                new XElement("ProjectBudgetIncomeLabel", this.GetReportText(4938, "Budg. intäkt")),
                new XElement("ProjectBudgetGrossMarginLabel", this.GetReportText(4939, "TB budg")),
                new XElement("ProjectBudgetGrossCoverageLabel", this.GetReportText(4940, "TG Budg")),
                new XElement("ProjectWorkInProgressLongLabel", this.GetReportText(4941, "Pågående arbete")),
                new XElement("ProjectResultLabel", this.GetReportText(4929, "Utfall"))
                );
        }

        protected void CreateProjectReportPageHeaderLabelsElement(XElement parent, SoeReportTemplateType reportTemplateType)
        {
            parent.Add(
                new XElement("ProjectNumberLabel", this.GetReportText(243, "Nummer:")),
                new XElement("ProjectNameLabel", this.GetReportText(244, "Projekt:")),
                new XElement("ProjectDescriptionLabel", this.GetReportText(245, "Beskrivning:")),
                new XElement("CreatedLabel", this.GetReportText(246, "Skapad datum:")),
                new XElement("CreatedByLabel", this.GetReportText(336, "Skapad av")),
                new XElement("ProjecStateLabel", this.GetReportText(248, "Aktiv:")),
                new XElement("EmployeeNrLabel", this.GetReportText(151, "Anst.nr:")),
                new XElement("EmployeeNameLabel", this.GetReportText(152, "Namn:")),
                new XElement("CustomerNrLabel", this.GetReportText(258, "Kundnr:")),
                new XElement("CustomerNameLabel", this.GetReportText(152, "Namn:")),
                new XElement("QuantityLabel", this.GetReportText(208, "Antal")),
                new XElement("ExportedLabel", this.GetReportText(194, "Exporterad")),
                new XElement("ProductNumberLabel", this.GetReportText(195, "Löneart nr")),
                new XElement("ProductNumberShortLabel", this.GetReportText(196, "Nummer")),
                new XElement("ProductNameLabel", this.GetReportText(197, "Löneart namn")),
                new XElement("ProductNameShortLabel", this.GetReportText(198, "Namn")),
                new XElement("ProductDescriptionLabel", this.GetReportText(199, "Löneart beskrivning")),
                new XElement("ProductDescriptionShortLabel", this.GetReportText(200, "Beskrivning")),
                new XElement("NoteLabel", this.GetReportText(249, "Anteckningar")),
                new XElement("DateLabel", this.GetReportText(250, "Datum")),
                new XElement("WeekLabel", this.GetReportText(214, "Vecka")),
                new XElement("MonthLabel", this.GetReportText(405, "Månad")),
                new XElement("OrderNrLabel", this.GetReportText(548, "Ordernummer")),
                new XElement("WeekDayLabel", this.GetReportText(4943, "Veckodag")),
                new XElement("OrderLabel", this.GetReportText(3797, "Order")),
                new XElement("CauseLabel", this.GetReportText(4488, "Orsak")),
                new XElement("InvoicingTypeLabel", this.GetReportText(4741, "Debiteringstyp")),
                new XElement("ExternalNoteLabel", this.GetReportText(4944, "Extern notering")),
                new XElement("WorkTimeLabel", this.GetReportText(4945, "Arbetad tid")),
                new XElement("InvoicedTimeLabel", this.GetReportText(4946, "Fakt. tid")),
                new XElement("AllLabel", this.GetReportText(4366, "Alla")),
                new XElement("TotalLabel", this.GetReportText(4936, "Totalt")),
                new XElement("InvoiceNrLabel", this.GetReportText(549, "Fakturanummer"))
                );

            if (reportTemplateType == SoeReportTemplateType.OrderContractChange)
            {
                parent.Add(
                    new XElement("InvoiceOrderDateLabel", this.GetReportText(145, "Beställningsdatum")),
                    new XElement("InvoicedToCustomer", this.GetReportText(1055, "Fakturerad till kund"))
                );
            }
        }

        protected void CreateStockAndProductReportPageHeaderLabelsElement(XElement parent)
        {
            parent.Add(
               new XElement("StockIdLabel", this.GetReportText(0, "StockId")),
               new XElement("StockNameLabel", this.GetReportText(896, "Lagerplats")),
               new XElement("StockCodeLabel", this.GetReportText(903, "Lagerplats kod")),
               new XElement("StateLabel", this.GetReportText(324, "Status")),
               new XElement("StockProductIdLabel", this.GetReportText(0, "StockProductId")),
               new XElement("InvoiceProductIdLabel", this.GetReportText(0, "InvoiceProductId")),
               new XElement("InvoiceProductNrLabel", this.GetReportText(340, "Artikelnr")),
               new XElement("InvoiceProductNameLabel", this.GetReportText(10, "Benämning")),
               new XElement("InvoiceProductUnitLabel", this.GetReportText(342, "Enhet")),
               new XElement("QuantityLabel", this.GetReportText(208, "Antal")),
               new XElement("OrderedQuantityLabel", this.GetReportText(900, "Beställt")),
               new XElement("ReservedQuantityLabel", this.GetReportText(899, "Reserverat")),
               new XElement("IsInInventoryLabel", this.GetReportText(898, "Under inventering")),
               new XElement("WarningLevelLabel", this.GetReportText(901, "Varning")),
               new XElement("AvgPrice", this.GetReportText(902, "Snittpris")),
               new XElement("StockShelfIdLabel", this.GetReportText(0, "StockShelfId")),
               new XElement("StockShelfNameLabel", this.GetReportText(897, "Hyllplats")),
               new XElement("AmountLabel", this.GetReportText(689, "Belopp")),
               new XElement("SumLabel", this.GetReportText(34, "Summa")),
               new XElement("TotalLabel", this.GetReportText(18, "Totalt")),
               new XElement("AvailableLabel", this.GetReportText(912, "Disponibelt")),
               new XElement("StockValueLabel", this.GetReportText(913, "Lagervärde")),
               new XElement("ProductGroupLabel", this.GetReportText(1051, "Produktgrupp"))
               );
        }

        protected void CreateStockTransactionReportPageHeaderLabelsElement(XElement parent)
        {
            parent.Add(
                   new XElement("StockNameLabel", this.GetReportText(896, "Lagerplats")),
                   new XElement("StockCodeLabel", this.GetReportText(903, "Lagerplats kod")),
                   new XElement("StateLabel", this.GetReportText(324, "Status")),
                   new XElement("StockProductIdLabel", this.GetReportText(0, "StockProductId")),
                   new XElement("InvoiceProductIdLabel", this.GetReportText(0, "InvoiceProductId")),
                   new XElement("InvoiceProductNrLabel", this.GetReportText(340, "Artikelnr")),
                   new XElement("InvoiceProductNameLabel", this.GetReportText(10, "Benämning")),
                   new XElement("InvoiceProductUnitLabel", this.GetReportText(342, "Enhet")),
                   new XElement("ActionTypeLabel", this.GetReportText(904, "Lagerhändelse kod")),
                   new XElement("ActionTypeNameLabel", this.GetReportText(905, "Lagerhändelse")),
                   new XElement("QuantityLabel", this.GetReportText(208, "Antal")),
                   new XElement("PriceLabel", this.GetReportText(685, "Pris")),
                   new XElement("NoteLabel", this.GetReportText(275, "Notering")),
                   new XElement("StockShelfIdLabel", this.GetReportText(0, "StockShelfId")),
                   new XElement("StockShelfNameLabel", this.GetReportText(897, "Hyllplats")),
                   new XElement("VoucherIdLabel", this.GetReportText(0, "VoucherId")),
                   new XElement("VoucherNrLabel", this.GetReportText(889, "Verifikatnr")),
                   new XElement("AmountLabel", this.GetReportText(689, "Belopp")),
                   new XElement("SumLabel", this.GetReportText(34, "Summa")),
                   new XElement("TotalLabel", this.GetReportText(18, "Totalt")),
                   new XElement("ProductGroupLabel", this.GetReportText(1051, "Produktgrupp")),
                   new XElement("SourceLabel", this.GetReportText(9238, "Källa"))
                   );

        }

        protected void CreateStockInventoryReportPageHeaderLabelsElement(XElement parent)
        {
            parent.Add(
                  new XElement("HeaderTextLabel", this.GetReportText(863, "Inventeringsunderlag")),
                  new XElement("StockCodeLabel", this.GetReportText(903, "Lagerplats kod")),
                  new XElement("StockNameLabel", this.GetReportText(896, "Lagerplats")),
                  new XElement("StartDateLabel", this.GetReportText(906, "Inventering start")),
                  new XElement("StopDateLabel", this.GetReportText(0907, "Inventering stopp")),
                  new XElement("CreatedLabel", this.GetReportText(798, "Skapad")),
                  new XElement("CreatedByLabel", this.GetReportText(799, "Skapad av")),
                  new XElement("ModifiedLabel", this.GetReportText(800, "Ändrad")),
                  new XElement("ModifiedByLabel", this.GetReportText(801, "Ändrad av")),
                  new XElement("InvoiceProductNrLabel", this.GetReportText(340, "Artikelnr")),
                  new XElement("InvoiceProductNameLabel", this.GetReportText(10, "Benämning")),
                  new XElement("InvoiceProductUnitLabel", this.GetReportText(342, "Enhet")),
                  new XElement("StockShelfCodeLabel", this.GetReportText(908, "Hyllplats kod")),
                  new XElement("StockShelfNameLabel", this.GetReportText(897, "Hyllplats")),
                  new XElement("StartingSaldoLabel", this.GetReportText(909, "Lagerantal")),
                  new XElement("InventorySaldoLabel", this.GetReportText(910, "Inventerat antal")),
                  new XElement("DifferenceLabel", this.GetReportText(911, "Inventeringsdifferens")),
                  new XElement("PriceLabel", this.GetReportText(685, "Pris")),
                  new XElement("RowCreatedLabel", this.GetReportText(798, "Skapad")),
                  new XElement("RowCreatedByLabel", this.GetReportText(799, "Skapad av")),
                  new XElement("RowModifiedLabel", this.GetReportText(800, "Ändrad")),
                  new XElement("RowModifiedByLabel", this.GetReportText(801, "Ändrad av")),
                  new XElement("AmountLabel", this.GetReportText(689, "Belopp")),
                  new XElement("SumLabel", this.GetReportText(34, "Summa")),
                  new XElement("DifferenceValueLabel", this.GetReportText(926, "Diff-värde")),
                  new XElement("TotalLabel", this.GetReportText(18, "Totalt")),
                  new XElement("ProductGroupLabel", this.GetReportText(1051, "Produktgrupp")),
                  new XElement("TransactionDateLabel", this.GetReportText(357, "Transaktionsdatum"))
                );
        }
        protected void CreatePurchaseOrderReportPageHeaderLabelsElement(XElement parent)
        {
            parent.Add(
                    new XElement("SupplierNameLabel", this.GetReportText(78, "Leverantörsintervall")),
                    new XElement("SupplierNrLabel", this.GetReportText(1041, "Leverantörsnr")),
                    new XElement("SupplierOurCustomerNrLabel", this.GetReportText(10111, "Vårt Kundnr")),
                    new XElement("SupplierGroupLabel", this.GetReportText(155, "Kategori")),
                    new XElement("SupplierOrgNrLabel", this.GetReportText(86, "Organisationsnummer")),
                    new XElement("SupplierVatNrLabel", this.GetReportText(87, "VAT-nr")),
                    new XElement("PurchaseDeliveryAddressLabel", this.GetReportText(88, "Leveransadress")),
                    new XElement("PurchaseNrLabel", this.GetReportText(9237, "Best nr")),
                    new XElement("PurchaseVatTypeLabel", this.GetReportText(91, "Momstyp")),
                    new XElement("PurchaseSumAmountLabel", this.GetReportText(79, "Total")),
                    new XElement("PurchaseVatAmountLabel", this.GetReportText(113, "Moms")),
                    new XElement("PurchaseCentRoundingLabel", this.GetReportText(142, "Öresutj.")),
                    new XElement("PurchaseCurrencyLabel", this.GetReportText(97, "Valuta")),
                    new XElement("PurchaseCurrencyRateLabel", this.GetReportText(98, "Kurs")),
                    new XElement("PurchaseCurrencyDateLabel", this.GetReportText(99, "Valutadatum")),
                    new XElement("PurchaseOrderDateLabel", this.GetReportText(145, "Beställningsdatum")),
                    new XElement("PurchaseReferenceYourLabel", this.GetReportText(105, "Er referens")),
                    new XElement("PurchaseReferenceOurLabel", this.GetReportText(106, "Vår referens")),
                    new XElement("PurchaseLabelLabel", this.GetReportText(294, "Märkning")),
                    new XElement("PurchaseAttentionLabel", this.GetReportText(1039, "Attention")),
                    new XElement("PurchasePaymentConditionLabel", this.GetReportText(104, "Betalningsvillkor")),
                    new XElement("PurchaseDeliveryConditionLabel", this.GetReportText(132, "Leveransvillkor")),
                    new XElement("PurchaseDeliveryTypeLabel", this.GetReportText(1040, "Leveranstyp")),
                    new XElement("PurchaseRowProductNumberLabel", this.GetReportText(340, "Artikelnr")),
                    new XElement("PurchaseRowProductNameLabel", this.GetReportText(341, "Artikelnamn")),
                    new XElement("PurchaseRowProductDescriptionLabel", this.GetReportText(109, "Beskrivning")),
                    new XElement("PurchaseRowProductUnitLabel", this.GetReportText(131, "Enhet")),
                    new XElement("PurchaseRowQuantityLabel", this.GetReportText(130, "Antal")),
                    new XElement("PurchaseRowDeliveradQuantityLabel", this.GetReportText(464, "Levererat antal")),
                    new XElement("PurchaseRemainingQuantityLabel", this.GetReportText(465, "Återstående antal")),
                    new XElement("PurchaseRowAmountLabel", this.GetReportText(110, "Apris")),
                    new XElement("PurchaseRowVatAmountLabel", this.GetReportText(113, "Moms")),
                    new XElement("PurchaseRowSumAmountLabel", this.GetReportText(111, "Belopp")),
                    new XElement("PurchaseRowSumAmountCurrencyLabel", this.GetReportText(112, "Belopp valuta")),
                    new XElement("PurchaseRowDiscountPercentLabel", this.GetReportText(114, "Rabatt %")),
                    new XElement("PurchaseRowDiscountAmountLabel", this.GetReportText(115, "Rabatt")),
                    new XElement("PurchaseRowTextLabel", this.GetReportText(15, "Text")),
                    new XElement("PurchaseRowWantedDeliveryDateLabel", this.GetReportText(1042, "Önskat leveransdatum")),
                    new XElement("PurchaseRowAccDeliveryDateLabel", this.GetReportText(1043, "Bekräftat leveransdatum")),
                    new XElement("PurchaseRowDeliveryDateLabel", this.GetReportText(103, "Leveransdatum")),
                    new XElement("TotalLabel", this.GetReportText(537, "Total")),
                    new XElement("StatusLabel", this.GetReportText(181, "Status")),
                    new XElement("PageSingleLabel", this.GetReportText(4, "Sida:")),
                    new XElement("PageMultipleLabel", this.GetReportText(459, "Sidor")),
                    new XElement("CreatedLabel", this.GetReportText(335, "Skapad")),
                    new XElement("CreatedByLabel", this.GetReportText(336, "Skapad av")),
                    new XElement("ModifiedLabel", this.GetReportText(460, "Ändrad")),
                    new XElement("ModifiedByLabel", this.GetReportText(461, "Ändrad av"))
                );
        }

        #endregion

        #region Time

        protected void CreateTimeMonthlyReportPageHeaderLabelsElement(XElement parent)
        {
            parent.Add(
                new XElement("EmployeeNrLabel", this.GetReportText(151, "Anst.nr:")),
                new XElement("EmployeeNameLabel", this.GetReportText(152, "Namn:")),
                new XElement("EmployeeGroupNameLabel", this.GetReportText(153, "Tidavtal:")),
                new XElement("EmployeeCategoryCodeLabel", this.GetReportText(154, "Kat.kod")),
                new XElement("EmployeeCategoryCodeShortLabel", this.GetReportText(155, "Kat")),
                new XElement("EmployeeCategoryNameLabel", this.GetReportText(156, "Kat.namn")),
                new XElement("EmployeeCategoryNameShortLabel", this.GetReportText(157, "Kat")),
                new XElement("ScheduleWeekDayLabel", this.GetReportText(158, "Dag")),
                new XElement("ScheduleWeekDateLabel", this.GetReportText(5, "Datum:")),
                new XElement("ScheduleStartTimeLabel", this.GetReportText(159, "Sch.in")),
                new XElement("ScheduleStartTimeShortLabel", this.GetReportText(160, "In")),
                new XElement("ScheduleStopTimeLabel", this.GetReportText(161, "Sch.ut")),
                new XElement("ScheduleStopTimeShortLabel", this.GetReportText(162, "Ut")),
                new XElement("ScheduleTimeLabel", this.GetReportText(163, "Schematid")),
                new XElement("ScheduleTimeShortLabel", this.GetReportText(164, "Schema")),
                new XElement("ScheduleTypeFactorMinutesLabel", this.GetReportText(434, "Uppräknat schema minuter")),
                new XElement("ScheduleTypeFactorMinutesShortLabel", this.GetReportText(435, "Uppräknat sch.min")),
                new XElement("ScheduleBreakTimeLabel", this.GetReportText(165, "Sch.rast")),
                new XElement("ScheduleBreakTimeShortLabel", this.GetReportText(166, "Rast")),
                new XElement("PresenceStartTimeLabel", this.GetReportText(167, "Närv.in")),
                new XElement("PresenceStartTimeShortLabel", this.GetReportText(168, "In")),
                new XElement("PresenceStopTimeLabel", this.GetReportText(169, "Närv.ut")),
                new XElement("PresenceStopTimeShortLabel", this.GetReportText(170, "Ut")),
                new XElement("PresenceTimeLabel", this.GetReportText(171, "Närvarotid")),
                new XElement("PresenceTimeShortLabel", this.GetReportText(172, "Närvaro")),
                new XElement("PresenceBreakTimeLabel", this.GetReportText(173, "Närvororast")),
                new XElement("PresenceBreakTimeShortLabel", this.GetReportText(174, "Rast")),
                new XElement("PresenceDaysTotalLabel", this.GetReportText(182, "Arb dgr:")),
                new XElement("PayrollAddedTimeLabel", this.GetReportText(188, "Mertid")),
                new XElement("PayrollOverTimeLabel", this.GetReportText(189, "Övertid")),
                new XElement("PayrollAddedAndOverTimeLabel", this.GetReportText(177, "Mertid+Ö")),
                new XElement("PayrollInconvinientWorkingHoursTimeLabel", this.GetReportText(178, "OB")),
                new XElement("PayrollInconvinientWorkingHoursScaledTimeLabel", this.GetReportText(266, "Vägd OB")),
                new XElement("PayrollAbsenceTimeLabel1", this.GetReportText(179, "Frv1")),
                new XElement("PayrollAbsenceTimeLabel2", this.GetReportText(180, "Frv2")),
                new XElement("AttestStateNameLabel", this.GetReportText(181, "Status")),
                new XElement("PayrollAbsenceTimeTotalLabel", this.GetReportText(183, "Frånvaro:")),
                new XElement("TotalTimeLabel", this.GetReportText(184, "Total tid (Närvaro plus Frånvaro):")),
                new XElement("TransactionsLabel", this.GetReportText(185, "Transaktioner")),
                new XElement("TransactionPayrollLabel", this.GetReportText(186, "Löneart")),
                new XElement("TransactionNumberLabel", this.GetReportText(187, "Antal")),
                new XElement("CalculatedCostPerHourLabel", this.GetReportText(286, "Kostnad")),
                new XElement("IsPreliminaryLabel", this.GetReportText(285, "Preliminär")),
                new XElement("IsPreliminaryShortLabel", this.GetReportText(288, "Prel")),
                new XElement("PayedTimeLabel", this.GetReportText(408, "Betald")),
                new XElement("GrossTimeLabel", this.GetReportText(410, "Bruttotid")),
                new XElement("NetTimeLabel", this.GetReportText(411, "Nettotid")),
                new XElement("CostLabel", this.GetReportText(412, "Kostnad")),
                new XElement("NumberOfDaysLabel", this.GetReportText(281, "Arbetat antal dagar")),
                new XElement("TotalAbsenceTimeLabel", this.GetReportText(387, "Total frånvaro")),
                new XElement("UnitPriceLabel", this.GetReportText(495, "Pris per enhet")),
                new XElement("FormulaLabel", this.GetReportText(523, "Formel")),
                new XElement("TimeUnitLabel", this.GetReportText(816, "Tidenhet")),
                new XElement("CalendarDayFactorLabel", this.GetReportText(817, "Kalenderdagsfaktor")),
                new XElement("NoteLabel", this.GetReportText(554, "Kommentar")),
                new XElement("WorkedNetTimeLabel", this.GetReportText(1100, "ARBETAD TID (netto)")),
                new XElement("PayrollInconvinientWorkingHoursNetTimeLabel", this.GetReportText(1101, "OB TID (netto)")),
                new XElement("NetCostLabel", this.GetReportText(1102, "KOSTNAD exkl arb.avgift")),
                new XElement("WorkedLabel", this.GetReportText(1103, "Arbetad")),
                new XElement("PayrollOvertimeWorkingHoursTimeLabel", this.GetReportText(1104, "ÖT")),
                new XElement("TimeTotalLabel", this.GetReportText(1105, "Tid totalt")),
                new XElement("GrossHoursLabel", this.GetReportText(1106, "TimBrutto")),
                new XElement("SalaryLabel", this.GetReportText(1107, "Lön")),
                new XElement("GrossHoursCalculationDescriptionLabel", this.GetReportText(1108, "TimBrutto beräknas som Arbetad schematid + Mertid + (ÖT50 *1,5) + (ÖT70*1,7) + (ÖT100*2) + (OB1 *0,5) + (OB2*0,7) + OB3")),
                new XElement("CostCalculationDescriptionLabel", this.GetReportText(1109, "Kostnad beräknas som TimBrutto * Timkostnad för anställd")),
                new XElement("AvarageCostCalculationDescriptionLabel", this.GetReportText(1110, "Ø kostnad (snittkostnad) beräknas som Kostnad / Arbetad tid totalt")),
                new XElement("AvarageSalaryCalculationDescriptionLabel", this.GetReportText(1111, "Ø lön (snittlön) beräknas som Kostnad / TimBrutto")));
        }

        protected void CreateTimeEmployeeSchedulePageHeaderLabelsElement(XElement parent)
        {
            parent.Add(
                new XElement("EmployeeNrLabel", this.GetReportText(151, "Anst.nr:")),
                new XElement("EmployeeNameLabel", this.GetReportText(152, "Namn:")),
                new XElement("EmployeeCategoryLabel", this.GetReportText(213, "Avdelning:")),
                new XElement("ScheduleWeekLabel", this.GetReportText(214, "Vecka")),
                new XElement("ScheduleTypeFactorMinutesLabel", this.GetReportText(434, "Uppräknat schema minuter")),
                new XElement("ScheduleWeekDayMondayShortLabel", this.GetReportText(215, "Mån")),
                new XElement("ScheduleWeekDayTuesdayShortLabel", this.GetReportText(216, "Tis")),
                new XElement("ScheduleWeekDayWednesdayShortLabel", this.GetReportText(217, "Ons")),
                new XElement("ScheduleWeekDayThursdayShortLabel", this.GetReportText(218, "Tors")),
                new XElement("ScheduleWeekDayFridayShortLabel", this.GetReportText(219, "Fre")),
                new XElement("ScheduleWeekDaySaturdayShortLabel", this.GetReportText(220, "Lör")),
                new XElement("ScheduleWeekDaySundayShortLabel", this.GetReportText(221, "Sön")),
                new XElement("IsPreliminaryLabel", this.GetReportText(285, "Preliminär")),
                new XElement("IsPreliminaryShortLabel", this.GetReportText(288, "Prel")),
                new XElement("EmployeeWeekTotalLabel", this.GetReportText(79, "Totalt")),
                new XElement("GrossTimeLabel", this.GetReportText(410, "Bruttotid")),
                new XElement("NetTimeLabel", this.GetReportText(411, "Nettotid")),
                new XElement("CostLabel", this.GetReportText(412, "Kostnad")),
                new XElement("EmploymentWorkTimeWeekLabel", this.GetReportText(707, "Veckoarbetstid")),
                new XElement("EmploymentPercentLabel", this.GetReportText(400, "Sysselsättningsgrad")),
                new XElement("AgreedTimeLabel", this.GetReportText(1030, "Avtalad tid")),
                new XElement("SchemeTimeLabel", this.GetReportText(163, "Schematid")),
                new XElement("AverageTimeLabel", this.GetReportText(1031, "Ø per vecka"))
                );
        }

        protected void CreateTimeCategorySchedulePageHeaderLabelsElement(XElement parent)
        {
            parent.Add(
                new XElement("EmployeeNrLabel", this.GetReportText(151, "Anst.nr:")),
                new XElement("EmployeeNameLabel", this.GetReportText(152, "Namn:")),
                new XElement("CategoryEmployeeLabel", this.GetReportText(230, "Anställd:")),
                new XElement("CategoryLabel", this.GetReportText(213, "Avdelning:")),
                new XElement("ScheduleWeekLabel", this.GetReportText(214, "Vecka")),
                new XElement("ScheduleTypeFactorMinutesLabel", this.GetReportText(434, "Uppräknat schema minuter")),
                new XElement("ScheduleWeekDayMondayShortLabel", this.GetReportText(215, "Mån")),
                new XElement("ScheduleWeekDayTuesdayShortLabel", this.GetReportText(216, "Tis")),
                new XElement("ScheduleWeekDayWednesdayShortLabel", this.GetReportText(217, "Ons")),
                new XElement("ScheduleWeekDayThursdayShortLabel", this.GetReportText(218, "Tors")),
                new XElement("ScheduleWeekDayFridayShortLabel", this.GetReportText(219, "Fre")),
                new XElement("ScheduleWeekDaySaturdayShortLabel", this.GetReportText(220, "Lör")),
                new XElement("ScheduleWeekDaySundayShortLabel", this.GetReportText(221, "Sön")),
                new XElement("ScheduleWeekDayMondayLabel", this.GetReportText(222, "Måndag")),
                new XElement("ScheduleWeekDayTuesdayLabel", this.GetReportText(223, "Tisdag")),
                new XElement("ScheduleWeekDayWednesdayLabel", this.GetReportText(224, "Onsdag")),
                new XElement("ScheduleWeekDayThursdayLabel", this.GetReportText(225, "Torsdag")),
                new XElement("ScheduleWeekDayFridayLabel", this.GetReportText(226, "Fredag")),
                new XElement("ScheduleWeekDaySaturdayLabel", this.GetReportText(227, "Lördag")),
                new XElement("ScheduleWeekDaySundayLabel", this.GetReportText(228, "Söndag")),
                new XElement("IsPreliminaryLabel", this.GetReportText(285, "Preliminär")),
                new XElement("IsPreliminaryShortLabel", this.GetReportText(288, "Prel")),
                new XElement("EmployeeWeekTotalLabel", this.GetReportText(79, "Totalt")),
                new XElement("CategoryTotalLabel", this.GetReportText(229, "Totalt Avdelning")),
                new XElement("GrossTimeLabel", this.GetReportText(410, "Bruttotid")),
                new XElement("NetTimeLabel", this.GetReportText(411, "Nettotid")),
                new XElement("CostLabel", this.GetReportText(412, "Kostnad")));
        }

        protected void CreateCreateTimeScheduleBlockHistoryPageHeaderLabelsElement(XElement parent)
        {
            parent.Add(
                new XElement("EmployeeNrLabel", this.GetReportText(151, "Anst.nr:")),
                new XElement("EmployeeNameLabel", this.GetReportText(152, "Namn:")),
                new XElement("EmployeeCategoryLabel", this.GetReportText(213, "Avdelning:")),
                new XElement("ScheduleWeekLabel", this.GetReportText(214, "Vecka")),
                new XElement("ScheduleWeekDayMondayShortLabel", this.GetReportText(215, "Mån")),
                new XElement("ScheduleWeekDayTuesdayShortLabel", this.GetReportText(216, "Tis")),
                new XElement("ScheduleWeekDayWednesdayShortLabel", this.GetReportText(217, "Ons")),
                new XElement("ScheduleWeekDayThursdayShortLabel", this.GetReportText(218, "Tors")),
                new XElement("ScheduleWeekDayFridayShortLabel", this.GetReportText(219, "Fre")),
                new XElement("ScheduleWeekDaySaturdayShortLabel", this.GetReportText(220, "Lör")),
                new XElement("ScheduleWeekDaySundayShortLabel", this.GetReportText(221, "Sön")),
                new XElement("IsPreliminaryLabel", this.GetReportText(285, "Preliminär")),
                new XElement("IsPreliminaryShortLabel", this.GetReportText(288, "Prel")),
                new XElement("EmployeeWeekTotalLabel", this.GetReportText(79, "Totalt")),
                new XElement("GrossTimeLabel", this.GetReportText(410, "Bruttotid")),
                new XElement("NetTimeLabel", this.GetReportText(411, "Nettotid")),
                new XElement("CostLabel", this.GetReportText(412, "Kostnad")));
        }

        protected void CreateTimeProjectReportPageHeaderLabelsElement(XElement parent)
        {
            parent.Add(
                new XElement("ProjectNumberLabel", this.GetReportText(243, "Nummer:")),
                new XElement("ProjectNameLabel", this.GetReportText(244, "Projekt:")),
                new XElement("ProjectDescriptionLabel", this.GetReportText(245, "Beskrivning:")),
                new XElement("ProjectInvoiceNrLabel", this.GetReportText(284, "Fakturanr:")),
                new XElement("ProjecCreatedLabel", this.GetReportText(246, "Skapad datum:")),
                new XElement("ProjecCreatedByLabel", this.GetReportText(247, "Skapad av:")),
                new XElement("ProjecStateLabel", this.GetReportText(248, "Aktiv:")),
                new XElement("EmployeeNrLabel", this.GetReportText(151, "Anst.nr:")),
                new XElement("EmployeeNameLabel", this.GetReportText(152, "Namn:")),
                new XElement("NoteLabel", this.GetReportText(249, "Anteckningar")),
                new XElement("DateLabel", this.GetReportText(250, "Datum")),
                new XElement("InvoiceTimeInMinutesLabel", this.GetReportText(251, "Fakturerad tid")),
                new XElement("TimeCodeLabel", this.GetReportText(321, "Tidkod")),
                new XElement("HourLabel", this.GetReportText(467, "Timmar")),
                new XElement("OrderLabel", this.GetReportText(307, "Order")));
        }

        protected void CreateTimePayrollTransactionReportPageHeaderLabelsElement(XElement parent)
        {
            parent.Add(
                new XElement("EmployeeNrLabel", this.GetReportText(151, "Anst.nr:")),
                new XElement("EmployeeNameLabel", this.GetReportText(152, "Namn:")),
                new XElement("EmployeeGroupNameLabel", this.GetReportText(153, "Tidavtal:")),
                new XElement("EmployeeCategoryCodeLabel", this.GetReportText(154, "Kat.kod")),
                new XElement("EmployeeCategoryCodeShortLabel", this.GetReportText(155, "Kat")),
                new XElement("EmployeeCategoryNameLabel", this.GetReportText(156, "Kat.namn")),
                new XElement("EmployeeCategoryNameShortLabel", this.GetReportText(157, "Kat")),
                new XElement("PayrollTransactionQuantityLabel", this.GetReportText(190, "Antal")),
                new XElement("PayrollTransactionTimeUnitLabel", this.GetReportText(812, "Tidenhet")),
                new XElement("PayrollTransactionCalendarDayFactorLabel", this.GetReportText(813, "Kalenderdagsfaktor")),
                new XElement("PayrollTransactionAmountLabel", this.GetReportText(191, "Belopp")),
                new XElement("PayrollTransactionDateLabel", this.GetReportText(192, "Datum")),
                new XElement("PayrollTransactionExportedLabel", this.GetReportText(194, "Exporterad")),
                new XElement("PayrollTransactionCommentLabel", this.GetReportText(201, "Kommentar")),
                new XElement("AttestStateNameLabel", this.GetReportText(193, "Status")),
                new XElement("PayrollProductNumberLabel", this.GetReportText(195, "Löneart nr")),
                new XElement("PayrollProductNumberShortLabel", this.GetReportText(196, "Nummer")),
                new XElement("PayrollProductNameLabel", this.GetReportText(197, "Löneart namn")),
                new XElement("PayrollProductNameShortLabel", this.GetReportText(198, "Namn")),
                new XElement("PayrollProductDescriptionLabel", this.GetReportText(199, "Löneart beskrivning")),
                new XElement("PayrollProductDescriptionShortLabel", this.GetReportText(200, "Beskrivning")),
                new XElement("IsPreliminaryLabel", this.GetReportText(285, "Preliminär")),
                new XElement("IsPreliminaryShortLabel", this.GetReportText(288, "Prel")),
                new XElement("VatLabel", this.GetReportText(202, "Moms")),
                new XElement("TimCodeCodeLabel", this.GetReportText(321, "Tidkod")),
                new XElement("TimCodeNameLabel", this.GetReportText(550, "Tidkod namn")),
                new XElement("FormulaLabel", this.GetReportText(551, "Formel")),
                new XElement("AttestStateLabel", this.GetReportText(552, "Attestnivå")),
                new XElement("NoteLabel", this.GetReportText(553, "Kommentar")),
                new XElement("CreatedByLabel", this.GetReportText(336, "Skapad av")),
                new XElement("ModifiedByLabel", this.GetReportText(282, "Modfierad av")),
                new XElement("CreatedLabel", this.GetReportText(335, "Skapad")),
                new XElement("ModifiedLabel", this.GetReportText(460, "Ändrad")),
                new XElement("GrossSalaryLabel", this.GetReportText(583, "Bruttolön")),
                new XElement("BenefitLabel", this.GetReportText(584, "Förmån")),
                new XElement("TaxLabel", this.GetReportText(585, "Skatt")),
                new XElement("CompensationLabel", this.GetReportText(587, "Ersättning")),
                new XElement("DeductionLabel", this.GetReportText(588, "Avdrag")),
                new XElement("NetSalaryLabel", this.GetReportText(589, "Nettolön")),
                new XElement("AbsenceLabel", this.GetReportText(268, "Frånvaro")),
                new XElement("SchemaLabel", this.GetReportText(164, "Schema")),
                new XElement("SchemaTimeLabel", this.GetReportText(163, "Schematid")),
                new XElement("PeriodLabel", this.GetReportText(37, "Period")),
                new XElement("TotalperiodLabel", this.GetReportText(20, "Totalt period")),
                new XElement("TotalAbsenceLabel", this.GetReportText(387, "Total frånvaro")),
                new XElement("SickAbsence", this.GetReportText(1023, "sjukfrånvaro")),
                new XElement("ForLabel", this.GetReportText(1022, "för")),
                new XElement("NoQualifyingDaysLabel", this.GetReportText(1024, "Antal karensdagar")),
                new XElement("ShortLabel", this.GetReportText(10105, "Kort")),
                new XElement("LongLabel", this.GetReportText(10106, "Lång")),
                new XElement("VacationLabel", this.GetReportText(1025, "Semester")),
                new XElement("ParentelLeaveLabel", this.GetReportText(1026, "Föräldraledig")),
                new XElement("TempParentelLeaveLabel", this.GetReportText(1027, "Tillfällig föräldraledighet")),
                new XElement("LeaveLabel", this.GetReportText(1028, "Permission")),
                new XElement("OtherAbsenceLabel", this.GetReportText(1029, "Övrig frånvaro")),
                new XElement("ScheduleWeekDayMondayLabel", this.GetReportText(222, "Måndag")),
                new XElement("ScheduleWeekDayTuesdayLabel", this.GetReportText(223, "Tisdag")),
                new XElement("ScheduleWeekDayWednesdayLabel", this.GetReportText(224, "Onsdag")),
                new XElement("ScheduleWeekDayThursdayLabel", this.GetReportText(225, "Torsdag")),
                new XElement("ScheduleWeekDayFridayLabel", this.GetReportText(226, "Fredag")),
                new XElement("ScheduleWeekDaySaturdayLabel", this.GetReportText(227, "Lördag")),
                new XElement("ScheduleWeekDaySundayLabel", this.GetReportText(228, "Söndag"))
                );


        }

        protected void CreateTimeAccumulatorReportPageHeaderLabelsElement(XElement parent)
        {
            parent.Add(
                new XElement("EmployeeNrLabel", this.GetReportText(151, "Anst.nr:")),
                new XElement("EmployeeNameLabel", this.GetReportText(152, "Namn:")),
                new XElement("EmployeeGroupNameLabel", this.GetReportText(153, "Tidavtal:")),
                new XElement("EmployeeCategoryCodeLabel", this.GetReportText(154, "Kat.kod")),
                new XElement("EmployeeCategoryNameLabel", this.GetReportText(156, "Kat.namn")),
                new XElement("EmployeeCategoryShortLabel", this.GetReportText(155, "Kat")),
                new XElement("EmployeeCategoryLabel", this.GetReportText(231, "Kategori")),
                new XElement("SumLabel", this.GetReportText(34, "Summa")),
                new XElement("TimeAccumulatorLabel", this.GetReportText(25, "Saldo")),
                new XElement("FinalSalaryLabel", this.GetReportText(943, "Slutlön")),
                new XElement("SumAccTodayLabel", this.GetReportText(935, "Aktuellt")),
                new XElement("SumAccTodayValueLabel", this.GetReportText(942, "Värde")),
                new XElement("TimeIncomingLabel", this.GetReportText(232, "Ingående")),
                new XElement("TimeChangeLabel", this.GetReportText(233, "Förändring")),
                new XElement("TimeOutgoingLabel", this.GetReportText(234, "Utgående")),
                new XElement("TimeAccumulatorMinLabel", this.GetReportText(235, "Min saldo")),
                new XElement("TimeAccumulatorMaxLabel", this.GetReportText(236, "Max saldo")),
                new XElement("TimeAccumulatorOutsideTheBorderLabel", this.GetReportText(237, "Utanför ramar")),
                new XElement("TimeAccumulatorTotalOverLabel", this.GetReportText(270, "Totalt över")),
                new XElement("TimeAccumulatorTotalUnderLabel", this.GetReportText(238, "Totalt under")),
                new XElement("TimeAccumulatorEntireReportLabel", this.GetReportText(239, "Hela rapporten")),
                new XElement("IsPreliminaryLabel", this.GetReportText(285, "Preliminär")),
                new XElement("IsPreliminaryShortLabel", this.GetReportText(288, "Prel")));
        }

        protected void CreateTimeAccumulatorDetailedReportPageHeaderLabelsElement(XElement parent)
        {
            parent.Add(
                new XElement("EmployeeNrLabel", this.GetReportText(151, "Anst.nr:")),
                new XElement("EmployeeNameLabel", this.GetReportText(152, "Namn:")),
                new XElement("EmployeeGroupNameLabel", this.GetReportText(153, "Tidavtal:")),
                new XElement("TimeAccumulatorLabel", this.GetReportText(25, "Saldo")),
                new XElement("FinalSalaryLabel", this.GetReportText(943, "Slutlön")),
                new XElement("TimeCodeLabel", this.GetReportText(321, "Tidkod")),
                new XElement("PayrollProductLabel", this.GetReportText(673, "Löneart")),
                new XElement("SumPeriodLabel", this.GetReportText(150, "Period")),
                new XElement("SumPlanningPeriodLabel", this.GetReportText(940, "Planeringsperiod")),
                new XElement("SumAccTodayLabel", this.GetReportText(935, "Aktuellt")),
                new XElement("SumAccTodayValueLabel", this.GetReportText(942, "Värde")),
                new XElement("SumYearLabel", this.GetReportText(936, "Året")),
                new XElement("FactorBasedOnWorkPercentageLabel", this.GetReportText(937, "Faktor baserad på sysselsättningsgrad")),
                new XElement("FactorBasedOnWorkPercentageShortLabel", this.GetReportText(0, "Faktor syssgrad")),
                new XElement("TimeBankLabel", this.GetReportText(939, "Tidbank")),
                new XElement("IsPreliminaryLabel", this.GetReportText(285, "Preliminär")),
                new XElement("IsPreliminaryShortLabel", this.GetReportText(288, "Prel")));
        }

        protected void CreateTimeCategoryStatisticsPageHeaderLabelsElement(XElement parent)
        {
            parent.Add(
                new XElement("EmployeeNrLabel", this.GetReportText(151, "Anst.nr:")),
                new XElement("EmployeeNameLabel", this.GetReportText(152, "Namn:")),
                new XElement("EmployeeGroupNameLabel", this.GetReportText(153, "Tidavtal:")),
                new XElement("EmployeeCategoryCodeLabel", this.GetReportText(154, "Kat.kod")),
                new XElement("EmployeeCategoryCodeShortLabel", this.GetReportText(155, "Kat")),
                new XElement("EmployeeCategoryNameLabel", this.GetReportText(156, "Kat.namn")),
                new XElement("EmployeeCategoryNameShortLabel", this.GetReportText(157, "Kat")),
                new XElement("ScheduleTimeLabel", this.GetReportText(163, "Schematid")),
                new XElement("ScheduleTimeShortLabel", this.GetReportText(164, "Schema")),
                new XElement("ScheduleBreakTimeLabel", this.GetReportText(165, "Sch.rast")),
                new XElement("ScheduleBreakTimeShortLabel", this.GetReportText(166, "Rast")),
                new XElement("PresenceTimeLabel", this.GetReportText(171, "Närvarotid")),
                new XElement("PresenceTimeShortLabel", this.GetReportText(172, "Närvaro")),
                new XElement("AbsenceTimeLabel", this.GetReportText(267, "Frånvarotid")),
                new XElement("AbsenceTimeShortLabel", this.GetReportText(268, "Frånvaro")),
                new XElement("PayrollAddedTimeLabel", this.GetReportText(188, "Mertid")),
                new XElement("PayrollOverTimeLabel", this.GetReportText(189, "Övertid")),
                new XElement("PayrollAddedAndOverTimeLabel", this.GetReportText(177, "Mertid+Ö")),
                new XElement("PayrollInconvinientWorkingHoursTimeLabel", this.GetReportText(178, "OB")),
                new XElement("PayrollInconvinientWorkingHoursScaledTimeLabel", this.GetReportText(266, "Vägd OB")),
                new XElement("TotalLabel", this.GetReportText(269, "Totalt")),
                new XElement("CalculatedCostPerHourLabel", this.GetReportText(286, "Kostnad")));
        }

        protected void CreateTimeStampEntryReportPageHeaderLabelsElement(XElement parent)
        {
            parent.Add(
                new XElement("EmployeeNrLabel", this.GetReportText(151, "Anst.nr:")),
                new XElement("EmployeeNameLabel", this.GetReportText(152, "Namn:")),
                new XElement("EmployeeSocialSecLabel", this.GetReportText(290, "Pers.nr:")),
                new XElement("EmployeeSexLabel", this.GetReportText(291, "Kön:")),
                new XElement("EmployeeGroupNameLabel", this.GetReportText(153, "Tidavtal:")),
                new XElement("TimeStampLabel", this.GetReportText(271, "Stämpling")),
                new XElement("WithoutCardLabel", this.GetReportText(272, "Utan kort")),
                new XElement("TimeLabel", this.GetReportText(273, "Tid")),
                new XElement("OriginalTimeLabel", this.GetReportText(404, "Original tid")),
                new XElement("StatusLabel", this.GetReportText(274, "Status")),
                new XElement("NoteLabel", this.GetReportText(275, "Notering")),
                new XElement("TimeBlockLabel", this.GetReportText(276, "Tidblock")),
                new XElement("StartTimeLabel", this.GetReportText(277, "Starttid")),
                new XElement("StopTimeLabel", this.GetReportText(278, "Stopptid")),
                new XElement("IsBreakLabel", this.GetReportText(279, "Rast")),
                new XElement("ManuallyAdjustedLabel", this.GetReportText(280, "Manuellt ändrad")),
                new XElement("CreatedLabel", this.GetReportText(335, "Skapad")),
                new XElement("CreatedByLabel", this.GetReportText(336, "Skapad av")));
        }

        protected void CreateEmployeeReportPageHeaderLabelsElement(XElement parent)
        {
            parent.Add(
                new XElement("EmployeeNrLabel", this.GetReportText(151, "Anst.nr:")),
                new XElement("EmployeeNameLabel", this.GetReportText(152, "Namn:")),
                new XElement("EmployeeSocialSecLabel", this.GetReportText(290, "Pers.nr:")),
                new XElement("EmployeeSexLabel", this.GetReportText(291, "Kön:")),
                new XElement("EmployeeUserNameLabel", this.GetReportText(399, "Användarnamn")),
                new XElement("EmployeeLanguageLabel", this.GetReportText(390, "Förvalt språk")),
                new XElement("EmployeeDefaultCompanyLabel", this.GetReportText(391, "Förvalt företag")),
                new XElement("EmployeeDefaultRoleLabel", this.GetReportText(392, "Förvald roll")),
                new XElement("EmployeeIsMobileUserLabel", this.GetReportText(393, "Mobilanvändare")),
                new XElement("EmployeeChangePasswordAtNextLoginLabel", this.GetReportText(394, "Byt lösenord vid nästa inloggning")),
                new XElement("EmployeeIsSysUserLabel", this.GetReportText(395, "Sysanvändare")),
                new XElement("EmployeeCalculatedCostPerHourLabel", this.GetReportText(401, "Kalkylkostnad")),
                new XElement("EmployeeNoteLabel", this.GetReportText(720, "Notering")),
                new XElement("EmploymentDateLabel", this.GetReportText(10102, "Anställningsdatum")),
                new XElement("EndDateLabel", this.GetReportText(10103, "Anställd t.o.m")),
                new XElement("DisbursementMethodLabel", this.GetReportText(8703, "Utbetalningssätt")),
                new XElement("DisbursementClearingNrLabel", this.GetReportText(8704, "Clearingnr")),
                new XElement("DisbursementAccountNrLabel", this.GetReportText(8705, "Kontonr")),
                new XElement("EmploymentDateFromLabel", this.GetReportText(721, "Startdatum")),
                new XElement("EmploymentDateToLabel", this.GetReportText(722, "Slutdatum")),
                new XElement("EmploymentBaseWorkTimeWeekLabel", this.GetReportText(723, "Grund veckoarbetstid")),
                new XElement("EmploymentWorkTimeWeekLabel", this.GetReportText(724, "Veckoarbetstid")),
                new XElement("EmploymentWorkPercentageLabel", this.GetReportText(400, "Sysselsättningsgrad")),
                new XElement("EmploymentExperienceMonthsLabel", this.GetReportText(725, "Branschvana månader")),
                new XElement("EmploymentExperienceAgreedOrEstablishedLabel", this.GetReportText(726, "Överenskommen/Konstaterad")),
                new XElement("EmploymentVacationDaysPayedLabel", this.GetReportText(727, "Semesterdagar betalda")),
                new XElement("EmploymentVacationDaysUnpayedLabel", this.GetReportText(728, "Semesterdagar obetalda")),
                new XElement("EmploymentSpecialConditionsLabel", this.GetReportText(729, "Särskillda villkor")),
                new XElement("EmploymentWorkPlaceLabel", this.GetReportText(730, "Arbetsplats")),
                new XElement("EmploymentSubstituteForLabel", this.GetReportText(731, "Vikarierar för")),
                new XElement("EmploymentEndReasonLabel", this.GetReportText(732, "Slutorsak")),
                new XElement("EmploymentTypeNameLabel", this.GetReportText(818, "Anställningsform")),
                new XElement("EmployeeGroupNameLabel", this.GetReportText(153, "Tidavtal:")),
                new XElement("EmployeeGroupIsAutogenLabel", this.GetReportText(733, "Avvikelserapportering")),
                new XElement("EmployeeGroupRuleWorkTimeWeekLabel", this.GetReportText(734, "Arbetstid (timmar per vecka)")),
                new XElement("EmployeeGroupRuleWorkTimeYearLabel", this.GetReportText(735, "Arbetstid (timmar per år)")),
                new XElement("EmployeeGroupRuleRestTimeWeekLabel", this.GetReportText(914, "Veckovila")),
                new XElement("EmployeeGroupRuleRestTimeDayLabel", this.GetReportText(736, "Dygnsvila")),
                new XElement("EmployeeGroupRuleWorkTimeYear2014Label", "Arbetstid (timmar per år) 2014"),
                new XElement("EmployeeGroupRuleWorkTimeYear2015Label", "Arbetstid (timmar per år) 2015"),
                new XElement("EmployeeGroupRuleWorkTimeYear2016Label", "Arbetstid (timmar per år) 2016"),
                new XElement("EmployeeGroupRuleWorkTimeYear2017Label", "Arbetstid (timmar per år) 2017"),
                new XElement("EmployeeGroupRuleWorkTimeYear2018Label", "Arbetstid (timmar per år) 2018"),
                new XElement("EmployeeGroupRuleWorkTimeYear2019Label", "Arbetstid (timmar per år) 2019"),
                new XElement("EmployeeGroupRuleWorkTimeYear2020Label", this.GetReportText(10107, "Arbetstid (timmar per år) 2020")),
                new XElement("EmployeeGroupRuleWorkTimeYear2021Label", this.GetReportText(10108, "Arbetstid (timmar per år) 2021")),
                new XElement("EmployeeGroupMaxScheduleTimeFullTimeLabel", this.GetReportText(9221, "Max schematid (heltid)")),
                new XElement("EmployeeGroupMaxScheduleTimePartTimeLabel", this.GetReportText(9222, "Max schematid (deltid)")),
                new XElement("EmployeeGroupMinScheduleTimeFullTimeLabel", this.GetReportText(9223, "Min schematid (heltid)")),
                new XElement("EmployeeGroupMinScheduleTimePartTimeLabel", this.GetReportText(9224, "Min schematid (deltid)")),
                new XElement("EmployeeGroupMaxScheduleTimeWithoutBreaksLabel", this.GetReportText(9225, "Max schematid utan rast")),
                new XElement("PayrollGroupNameLabel", this.GetReportText(742, "Löneavtal")),
                new XElement("VacationGroupNameLabel", this.GetReportText(949, "Semesteravtal")),
                new XElement("PriceTypeCodeLabel", this.GetReportText(743, "Kod")),
                new XElement("PriceTypeNameLabel", this.GetReportText(744, "Lönetyp")),
                new XElement("PriceTypeLabel", this.GetReportText(745, "Typ")),
                new XElement("PriceTypeCurrentAmountLabel", this.GetReportText(746, "Aktuellt belopp")),
                new XElement("FormulaNamesLabel", this.GetReportText(747, "Beräknad lön")),
                new XElement("FormulaPlainLabel", this.GetReportText(748, "Löneformel")),
                new XElement("FormulaExtractedLabel", this.GetReportText(749, "Belopp")),
                new XElement("TaxYearLabel", this.GetReportText(750, "År")),
                new XElement("TaxMainEmployerLabel", this.GetReportText(751, "Huvudarbetsgivare")),
                new XElement("TaxTypeLabel", this.GetReportText(752, "Beräkning")),
                new XElement("TaxRateLabel", this.GetReportText(753, "Skattetabell")),
                new XElement("TaxRateColumnLabel", this.GetReportText(754, "Kolumn")),
                new XElement("TaxOneTimeTaxPercentLabel", this.GetReportText(755, "Engångsskatt")),
                new XElement("TaxEstimatedAnnualSalaryLabel", this.GetReportText(756, "Beräknad årslön")),
                new XElement("TaxAdjustmentTypeLabel", this.GetReportText(757, "Jämkning")),
                new XElement("TaxAdjustmentValueLabel", this.GetReportText(758, "Belopp / %")),
                new XElement("TaxAdjustmentPeriodFromLabel", this.GetReportText(759, "Period från")),
                new XElement("TaxAdjustmentPeriodToLabel", this.GetReportText(760, "Period till")),
                new XElement("TaxSchoolYouthLimitInitialLabel", this.GetReportText(761, "Ursprungligt")),
                new XElement("TaxSchoolYouthLimitUsedLabel", this.GetReportText(762, "Utnyttjat")),
                new XElement("TaxSchoolYouthLimitremainingLabel", this.GetReportText(763, "Kvarvarande")),
                new XElement("TaxSinkTypeLabel", this.GetReportText(764, "Typ av SINK")),
                new XElement("TaxEmploymentTaxTypeLabel", this.GetReportText(765, "Arbetsgivaravgift")),
                new XElement("TaxEmploymentAbroadCodeLabel", this.GetReportText(766, "Utsänd personal")),
                new XElement("TaxRegionalSupport", this.GetReportText(767, "Regionalstöd")),
                new XElement("TaxSalaryDistressAmount", this.GetReportText(768, "Förbehållsbelopp")),
                new XElement("TaxSalaryDistressAmountType", this.GetReportText(769, "Typ av utmätningsbelopp")),
                new XElement("TaxSalaryDistressReservedAmount", this.GetReportText(770, "Utmätningsbelopp")),
                new XElement("CategoriesLabel", this.GetReportText(363, "Kategorier")),
                new XElement("CategoryCodeLabel", this.GetReportText(364, "Kategorikod")),
                new XElement("CategoryNameLabel", this.GetReportText(365, "Kategorinamn")),
                new XElement("CategoryIsDefaultLabel", this.GetReportText(366, "Standardkategori")),
                new XElement("CategoryStartDateLabel", this.GetReportText(771, "Datum från")),
                new XElement("CategorystopDateLabel", this.GetReportText(772, "Datum till")),
                new XElement("UserRolesLabel", this.GetReportText(367, "Behörighetsroller")),
                new XElement("UserRoleNameLabel", this.GetReportText(368, "Rollnamn")),
                new XElement("SkillsLabel", this.GetReportText(369, "Kompetenser")),
                new XElement("SkillTypeNameLabel", this.GetReportText(370, "Kompetenstyp")),
                new XElement("SkillNameLabel", this.GetReportText(371, "Namn")),
                new XElement("SkillEnddateLabel", this.GetReportText(372, "Slutdatum kompetens")),
                new XElement("PositionsLabel", this.GetReportText(373, "Befattningar")),
                new XElement("PositionLabel", this.GetReportText(388, "Befattning")),
                new XElement("EComsLabel", this.GetReportText(398, "Kontaktuppgifter")),
                new XElement("EComNameLabel", this.GetReportText(375, "Namn")),
                new XElement("EComTextLabel", this.GetReportText(376, "Kontaktuppgift")),
                new XElement("EComDescriptionLabel", this.GetReportText(377, "Beskrivning")),
                new XElement("AddressesLabel", this.GetReportText(378, "Adresser")),
                new XElement("AddressNameLabel", this.GetReportText(379, "Adresstyp")),
                new XElement("AddressTypeLabel", this.GetReportText(380, "Adressdatatyp")),
                new XElement("AddressLabel", this.GetReportText(381, "Adress")),
                new XElement("AddressCOLabel", this.GetReportText(382, "Adress c/o")),
                new XElement("AddressPostalCodeLabel", this.GetReportText(383, "Postnummer")),
                new XElement("AddressPostalAddressLabel", this.GetReportText(384, "Postort")),
                new XElement("AddressPostalCountryLabel", this.GetReportText(389, "Land")),
                new XElement("AddressStreetAddressLabel", this.GetReportText(385, "Gatuadress")),
                new XElement("AddressEntrenceCodeLabel", this.GetReportText(386, "Portkod")),
                new XElement("AccountingLabel", this.GetReportText(396, "Kontering")),
                new XElement("AccountInternalsLabel", this.GetReportText(397, "Internalkonton")),
                new XElement("CreatedByLabel", this.GetReportText(336, "Skapad av")),
                new XElement("MonthlySalaryLabel", this.GetReportText(1013, "Månadslön")),
                new XElement("HourlySalaryLabel", this.GetReportText(1014, "Timlön")),
                new XElement("EmployeeLabel", this.GetReportText(1015, "Anstnr")),
                new XElement("NameLabel", this.GetReportText(108, "Namn")),
                new XElement("EmployedNameLabel", this.GetReportText(344, "Anställd"))
                );
        }

        protected void CreatePayrollAccountingPageHeaderLabelsElement(XElement parent)
        {
            parent.Add(
                new XElement("AmountLabel", this.GetReportText(689, "Belopp")),
                new XElement("AmountEntCurrencyLabel", this.GetReportText(691, "Belopp koncernvaluta")),
                new XElement("AccountStringLabel", this.GetReportText(716, "Kontosträng")),
                new XElement("AccountNrLabel", this.GetReportText(717, "Konto no:")),
                new XElement("QuantityLabel", this.GetReportText(718, "Antal")),
                new XElement("TextLabel", this.GetReportText(719, "Text")),
                new XElement("DebetLabel", this.GetReportText(11, "Debet")),
                new XElement("CreditLabel", this.GetReportText(12, "Kredit")),
                new XElement("TotalLabel", this.GetReportText(537, "Total")),
                new XElement("DiffLabel", this.GetReportText(489, "Diff")));
        }

        protected void CreatePayrollSlipPageHeaderLabelsElement(XElement parent)
        {
            parent.Add(
                new XElement("EmployeeNrLabel", this.GetReportText(151, "Anst.nr:")),
                new XElement("EmployeeFirstName", this.GetReportText(803, "Förnamn:")),
                new XElement("EmployeeLastName", this.GetReportText(804, "Efternamn:")),
                new XElement("EmployeeNameLabel", this.GetReportText(152, "Namn:")),
                new XElement("EmployeeSocialSecLabel", this.GetReportText(805, "Personnummer:")),
                new XElement("IsPreliminaryLabel", this.GetReportText(285, "Preliminär")),
                new XElement("TimeUnitLabel", this.GetReportText(816, "Tidenhet")),
                new XElement("CalendarDayFactorLabel", this.GetReportText(817, "Kalenderdagsfaktor")),
                new XElement("IsRetroactiveLabel", this.GetReportText(925, "Retroaktiv lön")),
                new XElement("SettlementPeriodLabel", this.GetReportText(567, "Avräkningsperiod")),
                new XElement("ExtraPaymentLabel", this.GetReportText(568, "Extra utbetalning")),
                new XElement("PaymentDateLabel", this.GetReportText(569, "Utbetalningsdatum")),
                new XElement("NunberLabel", this.GetReportText(570, "Nr")),
                new XElement("DescriptionLabel", this.GetReportText(10, "Benämning")),
                new XElement("DateLabel", this.GetReportText(9, "Datum")),
                new XElement("QantityLabel", this.GetReportText(130, "Antal")),
                new XElement("PriceLabel", this.GetReportText(685, "Pris")),
                new XElement("AmountLabel", this.GetReportText(191, "Belopp")),
                new XElement("VacationDaysLabel", this.GetReportText(571, "Semesterdagar")),
                new XElement("PayedVacationDaysLabel", this.GetReportText(572, "Betalda")),
                new XElement("UnpayedVacationDaysLabel", this.GetReportText(9230, "Obetalda")),
                new XElement("AdvanceVacationLabel", this.GetReportText(573, "Förskott")),
                new XElement("SavedYear1VacationLabel", this.GetReportText(574, "Sparad år 1")),
                new XElement("SavedYear2VacationLabel", this.GetReportText(575, "Sparad år 2")),
                new XElement("SavedYear3VacationLabel", this.GetReportText(576, "Sparad år 3")),
                new XElement("SavedYear4VacationLabel", this.GetReportText(577, "Sparad år 4")),
                new XElement("SavedYear5VacationLabel", this.GetReportText(578, "Sparad år 5")),
                new XElement("DueVacationLabel", this.GetReportText(579, "Förfallna")),
                new XElement("VacationcoefficientLabel", this.GetReportText(580, "Sem.koefficient")),
                new XElement("BalancesLabel", this.GetReportText(581, "Saldon")),
                new XElement("CumulativeLabel", this.GetReportText(582, "Ackumulerad")),
                new XElement("GrossSalaryLabel", this.GetReportText(583, "Bruttolön")),
                new XElement("BenefitLabel", this.GetReportText(584, "Förmån")),
                new XElement("TaxLabel", this.GetReportText(585, "Skatt")),
                new XElement("SalaryperiodLabel", this.GetReportText(586, "Löneperioden")),
                new XElement("CompensationLabel", this.GetReportText(587, "Ersättning")),
                new XElement("DeductionLabel", this.GetReportText(588, "Avdrag")),
                new XElement("NetSalaryLabel", this.GetReportText(589, "Nettolön")),
                new XElement("AccountDepositLabel", this.GetReportText(590, "Kontoinsättning")),
                new XElement("PaidToBankAccountLabel", this.GetReportText(591, "Utbetalas till bankkonto")),
                new XElement("PaidToLabel", this.GetReportText(592, "Utbetalas till")),
                new XElement("SalaryspecificationLabel", this.GetReportText(593, "Lönespecifikation")),
                new XElement("PreliminaryPaymentLabel", this.GetReportText(594, "Preliminär utbetalning")),
                new XElement("RetroactiveSpecificationLabel", this.GetReportText(595, "Retroaktiv lön, se specifikation nedan")),
                new XElement("RetroactiveLabel", this.GetReportText(596, "Retroaktiv")),
                new XElement("ApprovalPaymentLabel", this.GetReportText(597, "Ovanstående godkännes för utbetalning från bankkonto")),
                new XElement("PayFromBankLabel", this.GetReportText(598, "Att utbetala bank")),
                new XElement("EmployersContributionLabel", this.GetReportText(765, "Arbetsgivaravgift")),
                new XElement("TaxDetails", this.GetReportText(9241, "Skatteuppgifter"))

                );

        }

        protected void CreateEmployeeVacationSEReportPageHeaderLabelsElement(XElement parent, int numberOfHoursHandelsGuaranteeAmount)
        {
            parent.Add(
                new XElement("EmployeeNrLabel", this.GetReportText(151, "Anst.nr:")),
                new XElement("EmployeeNameLabel", this.GetReportText(152, "Namn:")),
                new XElement("EarnedDaysLabel", this.GetReportText(780, "Intjänade dagar")),
                new XElement("PaidDaysLabel", this.GetReportText(781, "Betalda dagar")),
                new XElement("UnpaidDaysLabel", this.GetReportText(782, "Obetalda dagar")),
                new XElement("AdvanceLabel", this.GetReportText(783, "Förskott")),
                new XElement("SavedDaysLabel", this.GetReportText(784, "Sparade dagar")),
                new XElement("SavedDaysYear1Label", this.GetReportText(785, "Sparade år 1")),
                new XElement("SavedDaysYear2Label", this.GetReportText(786, "Sparade år 2")),
                new XElement("SavedDaysYear3Label", this.GetReportText(787, "Sparade år 3")),
                new XElement("SavedDaysYear4Label", this.GetReportText(788, "Sparade år 4")),
                new XElement("SavedDaysYear5Label", this.GetReportText(789, "Sparade år 5")),
                new XElement("SavedDaysOverdueLabel", this.GetReportText(790, "Förfallna dagar")),
                new XElement("UsedDaysLabel", this.GetReportText(791, "Uttagna dagar")),
                new XElement("RemainingDaysLabel", this.GetReportText(792, "Återstående dagar")),
                new XElement("EmploymentRateLabel", this.GetReportText(793, "Syss. grad (intjänad)")),
                new XElement("DebtInAdvanceLabel", this.GetReportText(794, "Skuld förskott")),
                new XElement("AmountLabel", this.GetReportText(795, "Belopp")),
                new XElement("DueDateLabel", this.GetReportText(796, "Förfaller")),
                new XElement("PrelUsedDaysLabel", this.GetReportText(802, "Prel.uttagna")),
                new XElement("DeleteLabel", this.GetReportText(797, "Ta bort")),
                new XElement("FactorsLabel", this.GetReportText(820, "Faktorer")),
                new XElement("TypeLabel", this.GetReportText(821, "Typ")),
                new XElement("FromLabel", this.GetReportText(822, "Fr.o.m.")),
                new XElement("CreatedLabel", this.GetReportText(798, "Skapad")),
                new XElement("CreatedByLabel", this.GetReportText(799, "Skapad av")),
                new XElement("ModifiedLabel", this.GetReportText(800, "Ändrad")),
                new XElement("ModifiedByLabel", this.GetReportText(801, "Ändrad av")),
                new XElement("PriceLabel", this.GetReportText(685, "Pris")),
                new XElement("QuantityLabel", this.GetReportText(130, "Antal")),
                new XElement("VacationRightLabel", this.GetReportText(954, "SR")),
                new XElement("VacationOwedLabel", this.GetReportText(948, "Semesterskuld")),
                new XElement("VacationAgreementLabel", this.GetReportText(949, "Semesteravtal")),
                new XElement("EarningYearLabel", this.GetReportText(950, "Intjänandeår")),
                new XElement("VacationOwedCalcToLabel", this.GetReportText(953, "Semesterskuld beräknad tom")),
                new XElement("TotalLabel", this.GetReportText(18, "Totalt")),
                new XElement("VacationSalaryOwedLabel", this.GetReportText(955, "Semesterlöneskuld")),
                new XElement("SumVacationSalaryOwedLabel", this.GetReportText(956, "Summa semesterlöneskuld (urval)")),
                new XElement("MissingLabel", this.GetReportText(957, "Saknas")),
                new XElement("SumLabel", this.GetReportText(34, "Summa")),
                new XElement("SettingCountDaysLabel", this.GetReportText(958, "Inställning - Beräkning av dagar")),
                new XElement("CalcTypeLabel", this.GetReportText(959, "Beräkningstyp")),
                new XElement("CalcTypeLabel1", this.GetReportText(960, "enligt semesterlagen")),
                new XElement("CalcTypeLabel2", this.GetReportText(961, "enligt kollektivavtal")),
                new XElement("CalcTypeLabel11", this.GetReportText(962, "procentuell beräkning enligt kollektivavtal")),
                new XElement("CalcTypeLabel13", this.GetReportText(963, "semestertillägg enligt semesterlagen")),
                new XElement("CalcTypeLabel14", this.GetReportText(964, "semestertillägg enligt kollektivavtal")),
                new XElement("CalcTypeLabel21", this.GetReportText(965, "AB-avtal")),
                new XElement("CalcTypeLabel22", this.GetReportText(966, "semestertillägg")),
                new XElement("IncreasdvacRightLabel", this.GetReportText(967, "Förhöjd semesterrätt - antal brytpunkter")),
                new XElement("DaysLabel", this.GetReportText(968, "Dagar")),
                new XElement("HoursLabel", this.GetReportText(467, "Timmar")),
                new XElement("LaborpassLabel", this.GetReportText(969, "Arbetspass")),
                new XElement("CalcPeriodEarningAmountLabel", this.GetReportText(970, "Beräkningsperiod intjänande av belopp")),
                new XElement("DateOfEmploymentLabel", this.GetReportText(10102, "Anställningsdatum")),
                new XElement("CalcOfVacationPurposeLabel", this.GetReportText(971, "Beräkning av semestergrundande frånvaro")),
                new XElement("CalcOfVacationRule1Label", this.GetReportText(972, "beräknas på faktisk ersättning dvs vad den anställde skulle fått om man arbetat")),
                new XElement("CalcOfVacationRule2Label", this.GetReportText(973, "uppräkning per dag")),
                new XElement("CalcOfVacationRule3Label", this.GetReportText(974, "uppräkning per timme")),
                new XElement("CalcOfVacationGaranteeAmountLabel", this.GetReportText(975, "Garantibelopp beräknas utifrån sysselsättningsgrad")),
                new XElement("CalcOfVacGarAmountHandelsLabel", this.GetReportText(976, "Garantibelopp beräknas utifrån 1796 timmar enligt Handels").Replace("1796", numberOfHoursHandelsGuaranteeAmount.ToString())),
                new XElement("JanuariLabel", this.GetReportText(977, "januari")),
                new XElement("februariLabel", this.GetReportText(978, "februari")),
                new XElement("marsLabel", this.GetReportText(979, "mars")),
                new XElement("aprilLabel", this.GetReportText(980, "april")),
                new XElement("majLabel", this.GetReportText(981, "maj")),
                new XElement("juniLabel", this.GetReportText(982, "juni")),
                new XElement("juliLabel", this.GetReportText(983, "juli")),
                new XElement("augustiLabel", this.GetReportText(984, "augusti")),
                new XElement("septemberLabel", this.GetReportText(985, "september")),
                new XElement("oktoberLabel", this.GetReportText(986, "oktober")),
                new XElement("novemberLabel", this.GetReportText(987, "november")),
                new XElement("decemberLabel", this.GetReportText(988, "december")),
                new XElement("VacationRightLongLabel", this.GetReportText(989, "Semesterrätt")),
                new XElement("VacationBaseAlterLabel", this.GetReportText(990, "Semestergrundande rörlig lön")),
                new XElement("VacationBaseLabel", this.GetReportText(991, "Semestergrundande lön")),
                new XElement("IfLabel", this.GetReportText(992, "Om")),
                new XElement("UseLabel", this.GetReportText(993, "använd")),
                new XElement("ElseUseLabel", this.GetReportText(994, "annars använd")),
                new XElement("PercentBaseLabel", this.GetReportText(995, "Procentsats")),
                new XElement("VacationSalaryLabel", this.GetReportText(996, "Semesterlön")),
                new XElement("VacationAddsLabel", this.GetReportText(997, "Semestertillägg")),
                new XElement("VacationHandeldByLabel", this.GetReportText(998, "Semester hanteras i")),
                new XElement("VacationSalaryPaidLabel", this.GetReportText(999, "dvs löner utbetalda denna period (kontantprincipen)")),
                new XElement("CalcMoveableVacLabel", this.GetReportText(1000, "Beräknad rörligt belopp")),
                new XElement("CalcMoveableAddLabel", this.GetReportText(1001, "Beräknad rörligt semestertillägg")),
                new XElement("PerDayLabel", this.GetReportText(1002, "per dag")),
                new XElement("IfCalcAmountLabel", this.GetReportText(1003, "Om Beräknad semesterlön per dag är större än Beräknat garantibelopp använd Beräknad semesterlön per dag annars  använd Beräknat garantibelopp")),
                new XElement("CalcGarAmountLabel", this.GetReportText(1004, "Beräknat garantibelopp")),
                new XElement("CalcAmountLabel", this.GetReportText(1005, "Beräkning av belopp")),
                new XElement("CalcDaysLabel", this.GetReportText(1006, "Beräkning av dagar")),
                new XElement("SettingCalcVacSalaryLabel", this.GetReportText(1007, "Inställning - beräkning av semesterlön")),
                new XElement("IncreasedVacRightLabel", this.GetReportText(1008, "Förhöjd semesterrätt - antal brytpunkter")),
                new XElement("TotalDebptLabel", this.GetReportText(1009, "Total skuld")),
                new XElement("CalcVatSalaryLabel", this.GetReportText(1010, "Beräknad semesterlön")),
                new XElement("DetCalcLabel", this.GetReportText(1011, "Detaljerad beräkning - underlag")),
                new XElement("VacationDateLabel", this.GetReportText(10141, "Semesterskuld per"))
                );
        }

        protected void CreateTimeScheduleTasksAndDeliverysPageHeaderLabelsElement(XElement parent)
        {
            parent.Add(
                new XElement("TimeScheduleTaskNameLabel", this.GetReportText(0, "Arbetsuppgift")),
                new XElement("TimeScheduleTaskTypeName", this.GetReportText(0, "Typ av arbetsuppgift")),
                new XElement("IncomingDeliveryHeadNameLabel", this.GetReportText(0, "Leverans")),
                new XElement("HandlingMoneyLabel", this.GetReportText(0, "Hantering av pengar")),
                new XElement("DescriptionLabel", this.GetReportText(839, "Beskrivning")),
                new XElement("RecurrenceDescriptionLabel", this.GetReportText(0, "Upprepning")),
                new XElement("RecurrenceDateLabel", this.GetReportText(839, "Inträffar")),
                new XElement("StartDateLabel", this.GetReportText(841, "Startdatum")),
                new XElement("StopDateLabel", this.GetReportText(842, "Slutdatum")),
                new XElement("StartTimeLabel", this.GetReportText(277, "Starttid")),
                new XElement("StopTimeLabel", this.GetReportText(278, "Stopptid")),
                new XElement("LengthLabel", this.GetReportText(0, "Behovets längd")),
                new XElement("OnlyOneEmployeeLabel", this.GetReportText(0, "Utförs av samma person")),
                new XElement("AllowOverlappingLabel", this.GetReportText(0, "Kan utföras parallellt")),
                new XElement("MinSplitLengthLabel", this.GetReportText(0, "Minsta längd vid delning")),
                new XElement("NbrOfPersonsLabel", this.GetReportText(0, "Antal personer")),
                new XElement("DontAssignBreakLeftoversLabel", this.GetReportText(0, "Raster tilldelas ej annan person")),
                new XElement("IsStaffingNeedsFrequency", this.GetReportText(0, "Från frekvensstatistik (försäljning)")),
                new XElement("ShiftTypeLabel", this.GetReportText(0, "Passtyp")),
                new XElement("TimeScheduleTypeLabel", this.GetReportText(0, "Schematyp")),
                new XElement("ColorLabel", this.GetReportText(0, "Färg")),
                new XElement("ExternalCodeLabel", this.GetReportText(0, "Extern kod") + " 1"),
                new XElement("NeedsCodeLabel", this.GetReportText(0, "Extern kod") + " 2"));
        }

        protected void AddTimeAccumulatorPageHeaderLabelElements(XElement parent, List<TimeAccumulator> timeAccumulators)
        {
            if (parent == null)
                return;

            for (int i = 1; i <= Constants.NOOFDIMENSIONS; i++)
            {
                TimeAccumulator timeAccumulator = null;
                if (timeAccumulators.Any() && timeAccumulators.Count >= i)
                    timeAccumulator = timeAccumulators.ElementAt(i - 1);

                parent.Add(
                    new XElement("TimeAccumulator" + i + "Label", timeAccumulator?.Name ?? String.Empty));
            }
        }

        protected void CreatePayrollProductPageHeaderLabelsElement(XElement parent)
        {
            parent.Add(
                new XElement("ProductIdLabel", this.GetReportText(9150, "Id")),
                new XElement("ProductTypeLabel", this.GetReportText(9151, "Typ av lön")),
                new XElement("ProductNumberLabel", this.GetReportText(9152, "Nummer")),
                new XElement("ProductNameLabel", this.GetReportText(9153, "Namn")),
                new XElement("ProductStateLabel", this.GetReportText(9154, "Status")),
                new XElement("ProductAccountingPrioLabel", this.GetReportText(9155, "Prioriteringsordning för konteringar")),
                new XElement("ProductFactorLabel", this.GetReportText(9156, "Faktor")),
                new XElement("ProductPayrollTypeLabel", this.GetReportText(9157, "Typ av lön")),
                new XElement("ProductShortNameLabel", this.GetReportText(9158, "Kortnamn")),
                new XElement("ProductExportLabel", this.GetReportText(9159, "Exportera till lön")),
                new XElement("ProductExcludeInWorkTimeSummaryLabel", this.GetReportText(9160, "Exl. från årsarbetstid")),
                new XElement("ProductPayedLabel", this.GetReportText(9162, "Betalas")),
                new XElement("ProductResultTypeLabel", this.GetReportText(9163, "Resultat typ")),
                new XElement("ProductAverageCalculatedLabel", this.GetReportText(9161, "Snitteberäknas")),
                new XElement("ProductIncludeAmountInExportLabel", this.GetReportText(9164, "Ta med pris i exporten")),
                new XElement("ProductSettingPayrollGroupIdLabel", this.GetReportText(9165, "Löneavtal id")),
                new XElement("ProductSettingPayrollGroupNameLabel", this.GetReportText(9166, "Löneavtal Namn")),
                new XElement("ProductSettingCentRoundingTypeLabel", this.GetReportText(9167, "Avrundning av belopp")),
                new XElement("ProductSettingCentRoundingLevelLabel", this.GetReportText(9168, "Avrundning nivå")),
                new XElement("ProductSettingTaxCalcTypeLabel", this.GetReportText(9169, "Skatteberäkning")),
                new XElement("ProductSettingPrintOnSalarySpecificationLabel", this.GetReportText(9170, "Visa på lönespecifikation")),
                new XElement("ProductSettingPrintDateLabel", this.GetReportText(9171, "Visa datum")),
                new XElement("ProductSettingPensionCompanyLabel", this.GetReportText(9172, "Pensionsbolag")),
                new XElement("ProductSettingVacationSalaryPromotedLabel", this.GetReportText(9173, "Semester lön")),
                new XElement("ProductSettingUnionFeePromotedLabel", this.GetReportText(9174, "Fackföreningsavgiftsgrundande")),
                new XElement("ProductSettingWorkingTimePromotedLabel", this.GetReportText(9175, "Arbetstidskontogrundande")),
                new XElement("ProductSettingCalculateSupplementedChargeLabel", this.GetReportText(9176, "Beräkna kompletterad betalning")),
                new XElement("ProductSettingTimeUnitLabel", this.GetReportText(9177, "Tidenhet")),
                new XElement("ProductSettingQuantityRoundingTypeLabel", this.GetReportText(9178, "Avrundning")),
                new XElement("ProductSettingChildPayrollProductLabel", this.GetReportText(9179, "Löneart")),
                new XElement("ProductSettingPayrollProductPriceTypeLabel", this.GetReportText(9180, "Produkt lönetyp")),
                new XElement("PriceTypeIdLabel", this.GetReportText(9181, "Lönetyp Id")),
                new XElement("ProductPriceTypeIdLabel", this.GetReportText(9182, "Lönetyp Id")),
                new XElement("ProductSettingIdLabel", this.GetReportText(9183, "Inställning Id")),
                new XElement("PriceTypeNameLabel", this.GetReportText(9184, "Lönetyp/löneformel")),
                new XElement("PriceTypesPeriodsLabel", this.GetReportText(9185, "Pris typ perioder")),
                new XElement("PriceTypesFromLabel", this.GetReportText(9186, "Fr.o.m")),
                new XElement("PriceTypesAmountLabel", this.GetReportText(9187, "Belopp")),
                new XElement("PriceFormulasLabel", this.GetReportText(9188, "Prisformel")),
                new XElement("PriceFormulaNameLabel", this.GetReportText(9189, "Prisformel Namn")),
                new XElement("PriceFormulaFromDateLabel", this.GetReportText(9190, "Fr.o.m")),
                new XElement("PriceFormulaIdLabel", this.GetReportText(9191, "Prisformel Id")),
                new XElement("PriceFormulaProductPriceFormulaIdLabel", this.GetReportText(9192, "Produkt prisformel Id")),
                new XElement("PriceFormulaPayrollProductSettingIdLabel", this.GetReportText(9193, "Produkt Inställning Id")),
                new XElement("PurchaseAccountsLabel", this.GetReportText(9194, "Kontoplan")),
                new XElement("PurchaseAccountsAccountIdLabel", this.GetReportText(9195, "Konto Id")),
                new XElement("PurchaseAccountsNumberLabel", this.GetReportText(9196, "Konto Nummer")),
                new XElement("PurchaseAccountsNameLabel", this.GetReportText(9197, "Konto Namn")),
                new XElement("PurchaseAccountsDescriptionLabel", this.GetReportText(9198, "Konto beskrivning")),
                new XElement("PurchaseAccountsPercentLabel", this.GetReportText(9199, "Konto procent")),
                new XElement("AccountingSettingLabel", this.GetReportText(9200, "Konto inställningar")),
                new XElement("AccountingSettingTypeLabel", this.GetReportText(9201, "Konto inställningar typ")),
                new XElement("AccountingSettingDimNrLabel", this.GetReportText(9202, "Konto Dim Nr")),
                new XElement("AccountingSettingDimNameLabel", this.GetReportText(9205, "Konto Dim Namn")),
                new XElement("AccountingSettingAccount1NrLabel", this.GetReportText(9203, "Konto Dim Nr1")),
                new XElement("AccountingSettingAccount1NameLabel", this.GetReportText(9204, "Konto Dim Namn1")),
                new XElement("AccountingSettingAccount2NrLabel", this.GetReportText(9203, "Konto Dim Nr2")),
                new XElement("AccountingSettingAccount2NameLabel", this.GetReportText(9204, "Konto Dim Namn2")),
                new XElement("AccountingSettingAccount3NrLabel", this.GetReportText(9203, "Konto Dim Nr3")),
                new XElement("AccountingSettingAccount3NameLabel", this.GetReportText(9204, "Konto Dim Namn3")),
                new XElement("AccountingSettingAccount4NrLabel", this.GetReportText(9203, "Konto Dim Nr4")),
                new XElement("AccountingSettingAccount4NameLabel", this.GetReportText(9204, "Konto Dim Namn4")),
                new XElement("AccountingSettingAccount5NrLabel", this.GetReportText(9203, "Konto Dim Nr5")),
                new XElement("AccountingSettingAccount5NameLabel", this.GetReportText(9204, "Konto Dim Namn5")),
                new XElement("AccountingSettingAccount6NrLabel", this.GetReportText(9203, "Konto Dim Nr6")),
                new XElement("AccountingSettingAccount6NameLabel", this.GetReportText(9204, "Konto Dim Namn6")),
                new XElement("AccountingSettingAccount7NrLabel", this.GetReportText(9203, "Konto Dim Nr7")),
                new XElement("AccountingSettingAccount7NameLabel", this.GetReportText(9204, "Konto Dim Namn7")),
                new XElement("AccountingSettingAccount8NrLabel", this.GetReportText(9203, "Konto Dim Nr8")),
                new XElement("AccountingSettingAccount8NameLabel", this.GetReportText(9204, "Konto Dim Namn8")),
                new XElement("AccountingSettingAccount9NrLabel", this.GetReportText(9203, "Konto Dim Nr9")),
                new XElement("AccountingSettingAccount9NameLabel", this.GetReportText(9204, "Konto Dim Namn9")),
                new XElement("AccountingSettingAccount10NrLabel", this.GetReportText(9203, "Konto Dim Nr10")),
                new XElement("AccountingSettingAccount10NameLabel", this.GetReportText(9204, "Konto Dim Namn10")),
                new XElement("AccountingSettingPercent1Label", this.GetReportText(9206, "Procent 1")),
                new XElement("AccountingSettingPercent2Label", this.GetReportText(9206, "Procent 2")),
                new XElement("AccountingSettingPercent3Label", this.GetReportText(9206, "Procent 3")),
                new XElement("AccountingSettingPercent4Label", this.GetReportText(9206, "Procent 4")),
                new XElement("AccountingSettingPercent5Label", this.GetReportText(9206, "Procent 5")),
                new XElement("AccountingSettingPercent6Label", this.GetReportText(9206, "Procent 6")),
                new XElement("AccountingSettingPercent7Label", this.GetReportText(9206, "Procent 7")),
                new XElement("AccountingSettingPercent8Label", this.GetReportText(9206, "Procent 8")),
                new XElement("AccountingSettingPercent9Label", this.GetReportText(9206, "Procent 9")),
                new XElement("AccountingSettingPercent10Label", this.GetReportText(9206, "Procent 10")),
                new XElement("CompanyCategoryRecordLabel", this.GetReportText(9207, "Företag kategori")),
                new XElement("CompanyCategoryCategoryIdLabel", this.GetReportText(9208, "Företag kategori Id")),
                new XElement("CompanyCategoryCategoryLabel", this.GetReportText(9207, "Företag kategori")),
                new XElement("CompanyCategoryStartDateLabel", this.GetReportText(9210, "Fr.o.m")),
                new XElement("CompanyCategoryEndDateLabel", this.GetReportText(9211, "Till")));
        }

        #endregion

        #endregion

        #region Elements

        protected void AddDefaultElement(CreateReportResult reportResult, XElement parentElement, string defaultElementName)
        {
            XElement element = GetDefaultElement(reportResult, defaultElementName);
            if (element != null)
                parentElement.Add(element);
        }

        protected void AddDefaultElement(CreateReportResult reportResult, List<XElement> elements, string defaultElementName)
        {
            XElement element = GetDefaultElement(reportResult, defaultElementName);
            if (element != null)
                elements.Add(element);
        }

        protected XElement GetDefaultElement(CreateReportResult reportResult, string defaultElementName)
        {
            return XmlUtil.GetDescendantElementWithChildElements(reportResult.Input.DefaultElementGroups, defaultElementName);
        }

        protected XElement CreateReportTitleElement(string reportName)
        {
            return new XElement("ReportTitle", reportName);
        }

        protected XElement CreateReportDescriptionElement(string reportDescription)
        {
            return new XElement("ReportDescription", reportDescription);
        }

        protected XElement CreateReportNrElement(string reportNr)
        {
            return new XElement("ReportNr", reportNr);
        }

        protected XElement CreateReportSelectionTextElement(string reportSelectionText)
        {
            return new XElement("ReportSelectionText", reportSelectionText);
        }

        protected XElement CreateCompanyElement()
        {
            return new XElement("Company", this.Company?.Name ?? string.Empty);
        }

        protected XElement CreateCompanyOrgNrElement()
        {
            return new XElement("Orgnr", this.Company?.OrgNr ?? string.Empty);
        }

        protected XElement CreateLoginNameElement(string loginName)
        {
            return new XElement("LoginName", loginName);
        }

        #region ERP

        protected XElement CreateLatestVoucherElement(EvaluatedSelection es)
        {
            return new XElement("LatestVoucher", GetLatestVoucherText(es));
        }

        protected XElement CreateAccountIntervalElement(EvaluatedSelection es, List<AccountDimDTO> accountDims)
        {
            return new XElement("AccountInterval", GetAccountIntervalText(es, accountDims));
        }

        protected XElement CreateAccountDim(EvaluatedSelection es, List<AccountDimDTO> accountDims)
        {
            return new XElement("AccountDimension", GetAccountDimText(es, accountDims));
        }

        protected XElement CreateVoucherIntervalElement(EvaluatedSelection es)
        {
            return new XElement("VoucherInterval", GetVoucherIntervalText(es));
        }

        protected XElement CreateVoucherSerieIntervalElement(EvaluatedSelection es)
        {
            return new XElement("VoucherSerieInterval", GetVoucherSerieIntervalText(es));
        }

        protected XElement CreateEmployeeIntervalElement(EvaluatedSelection es)
        {
            return new XElement("EmployeeInterval", GetStandardEmployeeIntervalText(es));
        }

        protected XElement CreateProjectIntervalElement(EvaluatedSelection es)
        {
            return new XElement("ProjectInterval", GetStandardProjectIntervalText(es));
        }

        protected XElement CreateCustomerIntervalElement(EvaluatedSelection es)
        {
            return new XElement("CustomerInterval", GetStandardCustomerIntervalText(es));
        }

        protected XElement CreateDateIntervalElement(EvaluatedSelection es)
        {
            return new XElement("DateInterval", GetDateIntervalText(es));
        }

        protected XElement CreateDateIntervalElement(DateTime date)
        {
            string dateString = date > CalendarUtility.DATETIME_MINVALUE ? date.ToShortDateString() : string.Empty;
            return new XElement("DateInterval", dateString);
        }

        protected XElement CreateDateIntervalElement(DateTime from, DateTime to)
        {
            string text = string.Empty;

            if (from != CalendarUtility.DATETIME_DEFAULT
                || to != CalendarUtility.DATETIME_DEFAULT)
            {
                string fromString = from > CalendarUtility.DATETIME_MINVALUE ? from.ToShortDateString() : string.Empty;
                string toString = to < CalendarUtility.DATETIME_MAXVALUE ? to.ToShortDateString() : string.Empty;
                text = string.Format("{0}-{1}", fromString, toString);
            }

            return new XElement("DateInterval", text);
        }

        protected XElement CreateIncludePreliminaryElement(bool includePreliminary)
        {
            return new XElement("IncludePreliminary", includePreliminary.ToInt());
        }

        protected XElement CreateTaxAuditTypeCodeElement(CompEntities entities, int actorCompanyId, AccountYear AYear, DateTime dateFrom, DateTime dateTo, List<SysVatAccount> sysVatAccounts, List<AccountStd> accountStds, BalanceItemDTO balanceItem, int vatnr1, int vatnr2, bool clearCache = false, bool reverseSign = false)
        {
            if (vatnr1 != 308)
            {
                //TypeCode
                XElement typeCodeElement = new XElement("TypeCode", new XAttribute("id", vatnr1 + "_" + vatnr2));

                SysVatAccount sysVatAccount = sysVatAccounts.FirstOrDefault(a => (a.VatNr1.HasValue && a.VatNr1.Value == vatnr1) && (a.VatNr2.HasValue && a.VatNr2.Value == vatnr2));
                var yearInBalances = AccountBalanceManager(actorCompanyId).GetYearInBalance(entities, AYear, accountStds, null, actorCompanyId);

                //accounts
                if (sysVatAccount != null)
                {
                    foreach (AccountStd accountStd in accountStds.Where(a => a.SysVatAccountId.HasValue && a.SysVatAccountId.Value == sysVatAccount.SysVatAccountId))
                    {
                        BalanceItemDTO balanceItemAccount = AccountBalanceManager(actorCompanyId).GetBalanceChange(entities, AYear, dateFrom, dateTo, accountStd, null, actorCompanyId, forceClearCache: clearCache);
                        decimal yearInBalance = yearInBalances.ContainsKey(accountStd.AccountId) ? yearInBalances[accountStd.AccountId].Balance : 0;

                        balanceItem.Balance = 0;

                        //Year starting balance
                        if (sysVatAccount.VatNr1 == 920)
                        {
                            balanceItem.Balance += (reverseSign) ? yearInBalance * -1 : yearInBalance;
                        }

                        //Period balance
                        if (balanceItemAccount != null && balanceItemAccount.Balance != 0)
                        {
                            balanceItem.Balance += (reverseSign) ? balanceItemAccount.Balance * -1 : balanceItemAccount.Balance;
                        }

                        if (balanceItem.Balance != 0)
                        {
                            typeCodeElement.Add(new XElement("Account",
                                        new XElement("AccountNr", accountStd.Account.AccountNr),
                                        new XElement("AccountName", accountStd.Account.Name),
                                        new XElement("AccountBalancechangePeriod", balanceItem.Balance)));
                        }
                    }
                }

                return typeCodeElement;
            }
            else
            {
                //TypeCode
                XElement typeCodeElement = new XElement("TypeCode", new XAttribute("id", vatnr2));
                typeCodeElement.Add(new XElement("Account",
                                    new XElement("AccountNr", ""),
                                    new XElement("AccountName", ""),
                                    new XElement("AccountBalancechangePeriod", 0.00)));

                return typeCodeElement;
            }
        }

        protected XElement CreateBillingInvoiceHeadElement(int invoiceHeadXmlId, SoeBillingInvoiceReportType billingInvoiceReportType, CustomerInvoice customerInvoice, List<ContactAddressRow> contactAddressRows, SysCurrency sysCurrency, string invoiceCopyText, string invoiceParentOrderNr, string invoiceBillingInterest, string invoiceInvertedVat, string creditedInvoicesText, string invoiceParentContractNr, ReportDataHistoryRepository repository = null, bool showCOLabel = false, SoeOriginType originType = SoeOriginType.None, bool useDeliveryAddressAsBillingAddress = false, decimal interestPercent = 0, bool showSalesPricePermission = true)
        {
            #region Prereq

            decimal invoiceFeeVatRate = 0;
            decimal invoiceFeeVatAmount = 0;
            decimal invoiceFreightVatRate = 0;
            decimal invoiceFreightVatAmount = 0;
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();

            if (customerInvoice.CustomerInvoiceRow != null)
            {
                foreach (CustomerInvoiceRow customerInvoiceRow in customerInvoice.ActiveCustomerInvoiceRows.Where(i => i.State == (int)SoeEntityState.Active).OrderBy(i => i.RowNr))
                {
                    //Why accountingRow?
                    if (customerInvoiceRow.Type == (int)SoeInvoiceRowType.AccountingRow || customerInvoiceRow.Type == (int)SoeInvoiceRowType.BaseProductRow)
                    {
                        // Freight VAT 
                        if (customerInvoiceRow.IsFreightAmountRow)
                        {
                            invoiceFreightVatRate = customerInvoiceRow.VatRate;
                            invoiceFreightVatAmount = customerInvoiceRow.VatAmount;
                        }
                        else if (customerInvoiceRow.IsInvoiceFeeRow)
                        {
                            invoiceFeeVatRate = customerInvoiceRow.VatRate;
                            invoiceFeeVatAmount = customerInvoiceRow.VatAmount;
                        }
                    }
                }
            }

            Customer customer = customerInvoice.Actor != null ? customerInvoice.Actor.Customer : null;
            if (customer == null)
                return null;

            var sysCountry = (customer.SysCountryId.HasValue) ? CountryCurrencyManager.GetSysCountry(customer.SysCountryId.Value) : null;

            bool isClosed = false;
            if (customerInvoice.Status == (int)SoeOriginStatus.OfferFullyInvoice || customerInvoice.Status == (int)SoeOriginStatus.OfferFullyOrder || customerInvoice.Status == (int)SoeOriginStatus.OrderClosed || customerInvoice.Status == (int)SoeOriginStatus.OrderFullyInvoice || customerInvoice.Status == (int)SoeOriginStatus.Payment)
                isClosed = true;

            string projectNr = string.Empty;
            string projectName = string.Empty;
            string shiftName = string.Empty;

            #endregion

            string customerName = "", customerNr = "", customerOrgNr = "", customerVatNr = "", customerGroupCode = "", customerGroup = "", customerGroupParentCode = "", customerGroupParentName = "";
            string billingAddress = "", billingAddressCo = "", billingPostalCode = "", billingPostalAddress = "", billingCountry = "", customerEmail = "";
            string deliveryName = "", deliveryAddress = "", deliveryAddressCo = "", deliveryPostalCode = "", deliveryPostalAddress = "", deliveryCountry = "", customerRegDate = "";
            string customerCountryCode = "", customerCountryName = "";

            string originTypeString = string.Empty;

            if (sysCountry != null)
            {
                customerCountryCode = sysCountry.Code;
                customerCountryName = sysCountry.Name;
            }

            // Let's find out if tax is included  2191
            if (!customerInvoice.PriceListTypeReference.IsLoaded)
                customerInvoice.PriceListTypeReference.Load();

            bool inclusiveVat = customerInvoice.PriceListType?.InclusiveVat ?? false;

            if (customerInvoice.Origin.Type == (int)SoeOriginType.Order)
                originTypeString = "Order";
            else if (customerInvoice.Origin.Type == (int)SoeOriginType.CustomerInvoice)
                originTypeString = "CustomerInvoice";
            else if (customerInvoice.Origin.Type == (int)SoeOriginType.Offer)
                originTypeString = "Offer";
            else if (customerInvoice.Origin.Type == (int)SoeOriginType.Contract)
                originTypeString = "Contract";
            if (customerInvoice.ProjectId.HasValue)
            {
                Project project = ProjectManager.GetProject(customerInvoice.ProjectId.Value, false);
                if (project != null)
                {
                    projectName = project.Name;
                    projectNr = project.Number;
                }
            }
            if (customerInvoice.ShiftTypeId.HasValue)
            {
                ShiftType shiftType = TimeScheduleManager.GetShiftType((int)customerInvoice.ShiftTypeId, loadAccounts: false, loadTimeScheduleType: false);
                if (shiftType != null)
                {
                    shiftName = shiftType.Name;
                }
            }
            Category category = CategoryManager.GetCategory(SoeCategoryType.Customer, SoeCategoryRecordEntity.Customer, customer.ActorCustomerId, customerInvoice.Origin.ActorCompanyId, onlyDefaultCategory: false);
            if (category != null)
            {
                customerGroupCode = category.Code;
                customerGroup = category.Name;
                customerGroupParentCode = (from c in entitiesReadOnly.Category where c.CategoryId == category.ParentId select c.Code).FirstOrDefault();
                customerGroupParentName = (from c in entitiesReadOnly.Category where c.CategoryId == category.ParentId select c.Name).FirstOrDefault();
            }

            if (repository != null && repository.HasSavedHistory)
            {
                #region Repository

                //Customer
                customerName = repository.GetHistoryValue(SoeReportDataHistoryHeadTag.BillingInvoice_InvoiceHead, SoeReportDataHistoryTag.BillingInvoice_InvoiceHead_CustomerName);
                customerNr = repository.GetHistoryValue(SoeReportDataHistoryHeadTag.BillingInvoice_InvoiceHead, SoeReportDataHistoryTag.BillingInvoice_InvoiceHead_CustomerNr);
                customerOrgNr = repository.GetHistoryValue(SoeReportDataHistoryHeadTag.BillingInvoice_InvoiceHead, SoeReportDataHistoryTag.BillingInvoice_InvoiceHead_CustomerOrgNr);
                customerVatNr = repository.GetHistoryValue(SoeReportDataHistoryHeadTag.BillingInvoice_InvoiceHead, SoeReportDataHistoryTag.BillingInvoice_InvoiceHead_CustomerVatNr);
                customerRegDate = repository.GetHistoryValue(SoeReportDataHistoryHeadTag.BillingInvoice_InvoiceHead, SoeReportDataHistoryTag.BillingInvoice_InvoiceHead_CustomerRegDate);
                customerEmail = repository.GetHistoryValue(SoeReportDataHistoryHeadTag.BillingInvoice_InvoiceHead, SoeReportDataHistoryTag.BillingInvoice_InvoiceHead_CustomerEmail);
                //Addresses
                billingAddress = repository.GetHistoryValue(SoeReportDataHistoryHeadTag.BillingInvoice_InvoiceHead, SoeReportDataHistoryTag.BillingInvoice_InvoiceHead_CustomerBillingAddress);
                billingAddressCo = repository.GetHistoryValue(SoeReportDataHistoryHeadTag.BillingInvoice_InvoiceHead, SoeReportDataHistoryTag.BillingInvoice_InvoiceHead_CustomerBillingAddressCO);
                billingPostalCode = repository.GetHistoryValue(SoeReportDataHistoryHeadTag.BillingInvoice_InvoiceHead, SoeReportDataHistoryTag.BillingInvoice_InvoiceHead_CustomerBillingPostalCode);
                billingPostalAddress = repository.GetHistoryValue(SoeReportDataHistoryHeadTag.BillingInvoice_InvoiceHead, SoeReportDataHistoryTag.BillingInvoice_InvoiceHead_CustomerBillingPostalAddress);
                billingCountry = repository.GetHistoryValue(SoeReportDataHistoryHeadTag.BillingInvoice_InvoiceHead, SoeReportDataHistoryTag.BillingInvoice_InvoiceHead_CustomerBillingCountry);
                deliveryName = repository.GetHistoryValue(SoeReportDataHistoryHeadTag.BillingInvoice_InvoiceHead, SoeReportDataHistoryTag.BillingInvoice_InvoiceHead_CustomerDeliveryName);
                deliveryAddress = repository.GetHistoryValue(SoeReportDataHistoryHeadTag.BillingInvoice_InvoiceHead, SoeReportDataHistoryTag.BillingInvoice_InvoiceHead_CustomerDeliveryAddress);
                deliveryAddressCo = repository.GetHistoryValue(SoeReportDataHistoryHeadTag.BillingInvoice_InvoiceHead, SoeReportDataHistoryTag.BillingInvoice_InvoiceHead_CustomerDeliveryAddressCO);
                deliveryPostalCode = repository.GetHistoryValue(SoeReportDataHistoryHeadTag.BillingInvoice_InvoiceHead, SoeReportDataHistoryTag.BillingInvoice_InvoiceHead_CustomerDeliveryPostalCode);
                deliveryPostalAddress = repository.GetHistoryValue(SoeReportDataHistoryHeadTag.BillingInvoice_InvoiceHead, SoeReportDataHistoryTag.BillingInvoice_InvoiceHead_CustomerDeliveryPostalAddress);
                deliveryCountry = repository.GetHistoryValue(SoeReportDataHistoryHeadTag.BillingInvoice_InvoiceHead, SoeReportDataHistoryTag.BillingInvoice_InvoiceHead_CustomerDeliveryCountry);

                #endregion
            }
            else
            {
                #region Current

                //Customer

                bool isOneTimeCustomer = false;
                isOneTimeCustomer = customer.IsOneTimeCustomer;

                if (isOneTimeCustomer)
                {
                    if (!String.IsNullOrEmpty(customerInvoice.InvoiceHeadText))
                    {
                        string[] separators = { Environment.NewLine, "\n", "\r" };
                        string[] headText = customerInvoice.InvoiceHeadText.Split(separators, StringSplitOptions.None);

                        int nrOftexts = headText.Count();
                        if (nrOftexts >= 1) { customerName = headText[0]; } else { customerName = ""; }
                    }
                    else
                        if (String.IsNullOrEmpty(customerName))
                        {
                            customerName = customer.Name;
                        }
                }
                else
                {
                    customerName = customer.Name;
                }

                customerNr = customer.CustomerNr;
                customerOrgNr = customer.OrgNr;
                customerVatNr = customer.VatNr;
                customerRegDate = customer.Created.HasValue ? customer.Created.Value.ToShortDateString() : String.Empty;

                if (customerInvoice.ContactEComId > 0)
                {
                    var ecomContact = ContactManager.GetContactECom((int)customerInvoice.ContactEComId, false);
                    customerEmail = ecomContact?.Text;
                }
                if (isOneTimeCustomer)
                {
                    customerEmail = customerInvoice.CustomerEmail != null ? customerInvoice.CustomerEmail : String.Empty;
                }
                //Addresses
                if (useDeliveryAddressAsBillingAddress && customerInvoice.BillingAdressText != null)
                {
                    //Task 15527
                    if (customerInvoice.BillingAdressText != String.Empty)
                    {
                        string[] separators = { Environment.NewLine, "\n", "\r" };
                        string[] address = customerInvoice.BillingAdressText.Split(separators, StringSplitOptions.None);

                        int nrOfAddresses = address.Count();
                        if (nrOfAddresses >= 1) { billingAddressCo = address[0]; } else { billingAddressCo = ""; }
                        if (nrOfAddresses >= 2) { billingAddress = address[1]; } else { billingAddress = ""; }
                        if (nrOfAddresses >= 3) { billingPostalCode = address[2]; } else { billingPostalCode = ""; }
                        if (nrOfAddresses >= 4) { billingPostalAddress = address[3]; } else { billingPostalAddress = ""; }
                        if (nrOfAddresses >= 5) { billingCountry = address[4]; } else { billingCountry = ""; }
                    }
                    else
                    {
                        billingAddress = contactAddressRows.GetContactAddressRowText(TermGroup_SysContactAddressType.Billing, TermGroup_SysContactAddressRowType.Address, customerInvoice.BillingAddressId);
                        billingAddressCo = contactAddressRows.GetContactAddressRowText(TermGroup_SysContactAddressType.Billing, TermGroup_SysContactAddressRowType.AddressCO, customerInvoice.BillingAddressId);
                        billingPostalCode = contactAddressRows.GetContactAddressRowText(TermGroup_SysContactAddressType.Billing, TermGroup_SysContactAddressRowType.PostalCode, customerInvoice.BillingAddressId);
                        billingPostalAddress = contactAddressRows.GetContactAddressRowText(TermGroup_SysContactAddressType.Billing, TermGroup_SysContactAddressRowType.PostalAddress, customerInvoice.BillingAddressId);
                        billingCountry = contactAddressRows.GetContactAddressRowText(TermGroup_SysContactAddressType.Billing, TermGroup_SysContactAddressRowType.Country, customerInvoice.BillingAddressId);
                    }
                }
                else
                {
                    billingAddress = contactAddressRows.GetContactAddressRowText(TermGroup_SysContactAddressType.Billing, TermGroup_SysContactAddressRowType.Address, customerInvoice.BillingAddressId);
                    billingAddressCo = contactAddressRows.GetContactAddressRowText(TermGroup_SysContactAddressType.Billing, TermGroup_SysContactAddressRowType.AddressCO, customerInvoice.BillingAddressId);
                    billingPostalCode = contactAddressRows.GetContactAddressRowText(TermGroup_SysContactAddressType.Billing, TermGroup_SysContactAddressRowType.PostalCode, customerInvoice.BillingAddressId);
                    billingPostalAddress = contactAddressRows.GetContactAddressRowText(TermGroup_SysContactAddressType.Billing, TermGroup_SysContactAddressRowType.PostalAddress, customerInvoice.BillingAddressId);
                    billingCountry = contactAddressRows.GetContactAddressRowText(TermGroup_SysContactAddressType.Billing, TermGroup_SysContactAddressRowType.Country, customerInvoice.BillingAddressId);
                }
                if (customerInvoice.DeliveryAddressId > 0)
                {
                    deliveryName = contactAddressRows.GetContactAddressRowText(TermGroup_SysContactAddressType.Delivery, TermGroup_SysContactAddressRowType.Name, customerInvoice.DeliveryAddressId);
                    deliveryAddress = contactAddressRows.GetContactAddressRowText(TermGroup_SysContactAddressType.Delivery, TermGroup_SysContactAddressRowType.Address, customerInvoice.DeliveryAddressId);
                    deliveryAddressCo = contactAddressRows.GetContactAddressRowText(TermGroup_SysContactAddressType.Delivery, TermGroup_SysContactAddressRowType.AddressCO, customerInvoice.DeliveryAddressId);
                    deliveryPostalCode = contactAddressRows.GetContactAddressRowText(TermGroup_SysContactAddressType.Delivery, TermGroup_SysContactAddressRowType.PostalCode, customerInvoice.DeliveryAddressId);
                    deliveryPostalAddress = contactAddressRows.GetContactAddressRowText(TermGroup_SysContactAddressType.Delivery, TermGroup_SysContactAddressRowType.PostalAddress, customerInvoice.DeliveryAddressId);
                    deliveryCountry = contactAddressRows.GetContactAddressRowText(TermGroup_SysContactAddressType.Delivery, TermGroup_SysContactAddressRowType.Country, customerInvoice.DeliveryAddressId);
                }

                #endregion

                #region Add to Repository

                if (repository != null && repository.HasActivatedHistory)
                {
                    repository.AddHistory(SoeReportDataHistoryHeadTag.BillingInvoice_InvoiceHead, SoeReportDataHistoryTag.BillingInvoice_InvoiceHead_CustomerName, customerName);
                    repository.AddHistory(SoeReportDataHistoryHeadTag.BillingInvoice_InvoiceHead, SoeReportDataHistoryTag.BillingInvoice_InvoiceHead_CustomerNr, customerNr);
                    repository.AddHistory(SoeReportDataHistoryHeadTag.BillingInvoice_InvoiceHead, SoeReportDataHistoryTag.BillingInvoice_InvoiceHead_CustomerOrgNr, customerOrgNr);
                    repository.AddHistory(SoeReportDataHistoryHeadTag.BillingInvoice_InvoiceHead, SoeReportDataHistoryTag.BillingInvoice_InvoiceHead_CustomerVatNr, customerVatNr);

                    repository.AddHistory(SoeReportDataHistoryHeadTag.BillingInvoice_InvoiceHead, SoeReportDataHistoryTag.BillingInvoice_InvoiceHead_CustomerBillingAddress, billingAddress);
                    repository.AddHistory(SoeReportDataHistoryHeadTag.BillingInvoice_InvoiceHead, SoeReportDataHistoryTag.BillingInvoice_InvoiceHead_CustomerBillingAddressCO, billingAddressCo);
                    repository.AddHistory(SoeReportDataHistoryHeadTag.BillingInvoice_InvoiceHead, SoeReportDataHistoryTag.BillingInvoice_InvoiceHead_CustomerBillingPostalCode, billingPostalCode);
                    repository.AddHistory(SoeReportDataHistoryHeadTag.BillingInvoice_InvoiceHead, SoeReportDataHistoryTag.BillingInvoice_InvoiceHead_CustomerBillingPostalAddress, billingPostalAddress);
                    repository.AddHistory(SoeReportDataHistoryHeadTag.BillingInvoice_InvoiceHead, SoeReportDataHistoryTag.BillingInvoice_InvoiceHead_CustomerBillingCountry, billingCountry);
                    repository.AddHistory(SoeReportDataHistoryHeadTag.BillingInvoice_InvoiceHead, SoeReportDataHistoryTag.BillingInvoice_InvoiceHead_CustomerDeliveryName, deliveryName);
                    repository.AddHistory(SoeReportDataHistoryHeadTag.BillingInvoice_InvoiceHead, SoeReportDataHistoryTag.BillingInvoice_InvoiceHead_CustomerDeliveryAddress, deliveryAddress);
                    repository.AddHistory(SoeReportDataHistoryHeadTag.BillingInvoice_InvoiceHead, SoeReportDataHistoryTag.BillingInvoice_InvoiceHead_CustomerDeliveryAddressCO, deliveryAddressCo);
                    repository.AddHistory(SoeReportDataHistoryHeadTag.BillingInvoice_InvoiceHead, SoeReportDataHistoryTag.BillingInvoice_InvoiceHead_CustomerDeliveryPostalCode, deliveryPostalCode);
                    repository.AddHistory(SoeReportDataHistoryHeadTag.BillingInvoice_InvoiceHead, SoeReportDataHistoryTag.BillingInvoice_InvoiceHead_CustomerDeliveryPostalAddress, deliveryPostalAddress);
                    repository.AddHistory(SoeReportDataHistoryHeadTag.BillingInvoice_InvoiceHead, SoeReportDataHistoryTag.BillingInvoice_InvoiceHead_CustomerDeliveryCountry, deliveryCountry);
                    repository.AddHistory(SoeReportDataHistoryHeadTag.BillingInvoice_InvoiceHead, SoeReportDataHistoryTag.BillingInvoice_InvoiceHead_CustomerRegDate, customerRegDate);
                    repository.AddHistory(SoeReportDataHistoryHeadTag.BillingInvoice_InvoiceHead, SoeReportDataHistoryTag.BillingInvoice_InvoiceHead_CustomerEmail, customerEmail);
                }

                #endregion
            }

            //If specified in company-settings, add C/O before distributionAddress.
            if (showCOLabel)
            {
                var coTerm = GetReportText(305, "C/O");
                if (!string.IsNullOrWhiteSpace(billingAddressCo) && !billingAddressCo.StartsWith(coTerm, StringComparison.CurrentCultureIgnoreCase))
                    billingAddressCo = string.Concat(coTerm, " ", billingAddressCo);
                if (!string.IsNullOrWhiteSpace(deliveryAddressCo) && !billingAddressCo.StartsWith(coTerm, StringComparison.CurrentCultureIgnoreCase))
                    deliveryAddressCo = string.Concat(coTerm, " ", deliveryAddressCo);
            }

            XElement invoiceHeadElement = new XElement("InvoiceHead");
            invoiceHeadElement.Add(
                new XAttribute("id", invoiceHeadXmlId));

            invoiceHeadElement.Add(
                new XElement("BillingInvoiceReportType", ((int)billingInvoiceReportType).ToString()));

            invoiceHeadElement.Add(
                new XElement("CustomerName", customerName),
                new XElement("CustomerNr", customerNr),
                new XElement("CustomerGroupCode", customerGroupCode),
                new XElement("CustomerGroup", customerGroup),
                new XElement("CustomerGroupParentCode", customerGroupParentCode),
                new XElement("CustomerGroupParentName", customerGroupParentName),
                new XElement("CustomerOrgNr", customerOrgNr),
                new XElement("CustomerVatNr", customerVatNr),
                new XElement("CustomerCountryCode", customerCountryCode),
                new XElement("CustomerCountryName", customerCountryName)
                );

            invoiceHeadElement.Add(
                new XElement("CustomerAddressBilling", billingAddress),
                new XElement("CustomerAddressCOBilling", billingAddressCo),
                new XElement("CustomerPostalCodeBilling", billingPostalCode),
                new XElement("CustomerPostalAddressBilling", billingPostalAddress),
                new XElement("CustomerCountryBilling", billingCountry));

            if (string.IsNullOrEmpty(customerInvoice.InvoiceHeadText))
            {
                invoiceHeadElement.Add(
                  new XElement("CustomerNameDelivery", deliveryName),
                  new XElement("CustomerAddressDelivery", deliveryAddress),
                  new XElement("CustomerAddressCODelivery", deliveryAddressCo),
                  new XElement("CustomerPostalCodeDelivery", deliveryPostalCode),
                  new XElement("CustomerPostalAddressDelivery", deliveryPostalAddress),
                  new XElement("CustomerCountryDelivery", deliveryCountry));
            }
            else
            {
                invoiceHeadElement.Add(
                  new XElement("CustomerNameDelivery", String.Empty),
                  new XElement("CustomerAddressDelivery", String.Empty),
                  new XElement("CustomerAddressCODelivery", String.Empty),
                  new XElement("CustomerPostalCodeDelivery", String.Empty),
                  new XElement("CustomerPostalAddressDelivery", String.Empty),
                  new XElement("CustomerCountryDelivery", String.Empty));
            }
            invoiceHeadElement.Add(new XElement("CustomerRegDate", customerRegDate));
            invoiceHeadElement.Add(new XElement("CustomerEmail", customerEmail));
            var orderDateTxt = CountryCurrencyManager.GetDateFormatedForCountry(customer.SysCountryId.GetValueOrDefault(), customerInvoice.OrderDate, GetCompanySysCountryIdFromCache(entitiesReadOnly, this.ActorCompanyId));
            var invoiceDateTxt = CountryCurrencyManager.GetDateFormatedForCountry(customer.SysCountryId.GetValueOrDefault(), customerInvoice.InvoiceDate, GetCompanySysCountryIdFromCache(entitiesReadOnly, this.ActorCompanyId));
            var orderDueDateTxt = CountryCurrencyManager.GetDateFormatedForCountry(customer.SysCountryId.GetValueOrDefault(), customerInvoice.DueDate, GetCompanySysCountryIdFromCache(entitiesReadOnly, this.ActorCompanyId));
            var orderDeliveryDateTxt = CountryCurrencyManager.GetDateFormatedForCountry(customer.SysCountryId.GetValueOrDefault(), customerInvoice.DeliveryDate, GetCompanySysCountryIdFromCache(entitiesReadOnly, this.ActorCompanyId));
            invoiceHeadElement.Add(
                new XElement("InvoiceNr", customerInvoice.InvoiceNr),
                new XElement("InvoiceParentOrderNr", invoiceParentOrderNr),
                new XElement("InvoiceParentContractNr", invoiceParentContractNr),
                new XElement("InvoiceSeqNr", customerInvoice.SeqNr.HasValue ? customerInvoice.SeqNr.Value : 0),
                new XElement("InvoiceBillingType", GetText(customerInvoice.BillingType, (int)TermGroup.InvoiceBillingType)),
                new XElement("InvoiceCreditedInvoicesText", creditedInvoicesText),
                new XElement("InvoiceVatTyp", GetText(customerInvoice.VatType, (int)TermGroup.InvoiceVatType)),
                new XElement("InvoiceOCR", customerInvoice.OCR),
                new XElement("InvoiceSumAmount", showSalesPricePermission ? customerInvoice.SumAmountCurrency : 0),
                new XElement("InvoiceSumAmountBase", showSalesPricePermission ? customerInvoice.SumAmount : 0),
                new XElement("InvoiceVatAmount", showSalesPricePermission ? customerInvoice.VATAmountCurrency : 0),
                new XElement("InvoiceVatAmountBase", showSalesPricePermission ? customerInvoice.VATAmount : 0),
                new XElement("InvoiceFeeAmount", customerInvoice.InvoiceFeeCurrency),
                new XElement("InvoiceFeeAmountBase", customerInvoice.InvoiceFee),
                new XElement("InvoiceFreightAmount", customerInvoice.FreightAmountCurrency),
                new XElement("InvoiceFreightAmountBase", customerInvoice.FreightAmount),
                new XElement("InvoiceCentRounding", customerInvoice.CentRounding),
                new XElement("InvoiceTotalAmount", showSalesPricePermission ? customerInvoice.TotalAmountCurrency : 0),
                new XElement("InvoiceTotalAmountBase", showSalesPricePermission ? customerInvoice.TotalAmount : 0),
                new XElement("InvoiceCurrency", sysCurrency != null ? sysCurrency.Name : ""),
                new XElement("InvoiceCurrencyCode", sysCurrency != null ? sysCurrency.Code : ""),
                new XElement("InvoiceCurrencyRate", customerInvoice.CurrencyRate),
                new XElement("InvoiceCurrencyDate", customerInvoice.CurrencyDate),
                new XElement("InvoiceDate", customerInvoice.InvoiceDate.HasValue ? customerInvoice.InvoiceDate.Value : CalendarUtility.DATETIME_DEFAULT),
                new XElement("InvoiceDueDate", customerInvoice.DueDate.HasValue ? customerInvoice.DueDate.Value : CalendarUtility.DATETIME_DEFAULT),
                new XElement("InvoiceVoucherDate", customerInvoice.VoucherDate.HasValue ? customerInvoice.VoucherDate.Value : CalendarUtility.DATETIME_DEFAULT),
                new XElement("InvoiceDeliveryDate", customerInvoice.DeliveryDate.HasValue ? customerInvoice.DeliveryDate.Value : CalendarUtility.DATETIME_DEFAULT),
                new XElement("InvoiceDeliveryDateText", customerInvoice.DeliveryDateText != null ? customerInvoice.DeliveryDateText : String.Empty),
                new XElement("InvoiceDateTxt", invoiceDateTxt != null ? invoiceDateTxt : String.Empty),
                new XElement("InvoiceOrderDateTxt", orderDateTxt != null ? orderDateTxt : String.Empty),
                new XElement("InvoiceDueDateTxt", orderDueDateTxt != null ? orderDueDateTxt : String.Empty),
                new XElement("InvoiceDeliveryDateTxt", orderDeliveryDateTxt != null ? orderDeliveryDateTxt : String.Empty),
                new XElement("InvoiceOrderDate", customerInvoice.OrderDate.HasValue ? customerInvoice.OrderDate.Value : CalendarUtility.DATETIME_DEFAULT),
                new XElement("InvoiceCreatedDate", customerInvoice.Created.HasValue ? customerInvoice.Created.Value : CalendarUtility.DATETIME_DEFAULT),
                new XElement("InvoiceCreatedBy", customerInvoice.CreatedBy != null ? customerInvoice.CreatedBy : ""),
                new XElement("InvoiceReferenceYour", customerInvoice.ReferenceYour),
                new XElement("InvoiceReferenceOur", customerInvoice.ReferenceOur),
                new XElement("InvoiceReferenceOrder", customerInvoice.OrderReference),
                new XElement("InvoiceText", customerInvoice.InvoiceText),
                new XElement("InvoiceHeadText", customerInvoice.InvoiceHeadText),
                new XElement("InvoiceLabel", customerInvoice.InvoiceLabel != null ? customerInvoice.InvoiceLabel : String.Empty),
                new XElement("InvoiceDescription", customerInvoice.Origin != null && !String.IsNullOrEmpty(customerInvoice.Origin.Description) ? customerInvoice.Origin.Description : String.Empty),
                new XElement("InvoiceWorkingDescription", originType == SoeOriginType.Order || originType == SoeOriginType.Offer || (originType == SoeOriginType.CustomerInvoice && customerInvoice.IncludeOnInvoice) ? customerInvoice.WorkingDescription : String.Empty),
                new XElement("InvoicePaymentCondition", customerInvoice.PaymentCondition != null ? customerInvoice.PaymentCondition.Name : ""),
                new XElement("InvoiceDeliveryConditionCode", customerInvoice.DeliveryCondition != null ? customerInvoice.DeliveryCondition.Code : ""),
                new XElement("InvoiceDeliveryConditionName", customerInvoice.DeliveryCondition != null ? customerInvoice.DeliveryCondition.Name : ""),
                new XElement("InvoiceDeliveryTypeCode", customerInvoice.DeliveryType != null ? customerInvoice.DeliveryType.Code : ""),
                new XElement("InvoiceDeliveryTypeName", customerInvoice.DeliveryType != null ? customerInvoice.DeliveryType.Name : ""),
                new XElement("InvoiceBillingInterest", invoiceBillingInterest),
                new XElement("InvoiceBillingInterestPercent", interestPercent),
                new XElement("InvoiceInvertedVat", invoiceInvertedVat),
                new XElement("InvoiceCopy", invoiceCopyText),
                new XElement("InvoiceIsCashSale", customerInvoice.CashSale.ToInt()),
                new XElement("InvoiceIsClosed", isClosed.ToInt()),
                new XElement("ProjectNumber", projectNr),
                new XElement("ProjectName", projectName),
                new XElement("ShiftName", shiftName),
                new XElement("OriginType", originTypeString),
                //new XElement("PaidAmount", customerInvoice.PaidAmount != null ? customerInvoice.PaidAmount : 0),
                new XElement("PaidAmount", customerInvoice.PaidAmount),
                new XElement("FullyPaid", customerInvoice.FullyPayed.ToInt()),
                new XElement("InvoiceInclusiveVat", inclusiveVat.ToInt()),
                new XElement("InvoiceFreightVatRate", invoiceFreightVatRate),
                new XElement("InvoiceFreightVatAmount", invoiceFreightVatAmount),
                new XElement("InvoiceFeeVatRate", invoiceFeeVatRate),
                new XElement("InvoiceFeeVatAmount", invoiceFeeVatAmount));

            Account accountDim1 = customerInvoice.DefaultDim2AccountId.HasValue ? AccountManager.GetAccount(customerInvoice.Origin.ActorCompanyId, customerInvoice.DefaultDim2AccountId.Value) : null;
            Account accountDim2 = customerInvoice.DefaultDim3AccountId.HasValue ? AccountManager.GetAccount(customerInvoice.Origin.ActorCompanyId, customerInvoice.DefaultDim3AccountId.Value) : null;
            Account accountDim3 = customerInvoice.DefaultDim4AccountId.HasValue ? AccountManager.GetAccount(customerInvoice.Origin.ActorCompanyId, customerInvoice.DefaultDim4AccountId.Value) : null;
            Account accountDim4 = customerInvoice.DefaultDim5AccountId.HasValue ? AccountManager.GetAccount(customerInvoice.Origin.ActorCompanyId, customerInvoice.DefaultDim5AccountId.Value) : null;
            Account accountDim5 = customerInvoice.DefaultDim6AccountId.HasValue ? AccountManager.GetAccount(customerInvoice.Origin.ActorCompanyId, customerInvoice.DefaultDim6AccountId.Value) : null;

            invoiceHeadElement.Add(
                new XElement("DefaultDim1Code", accountDim1?.AccountNr ?? string.Empty),
                new XElement("DefaultDim1Name", accountDim1?.Name ?? string.Empty),
                new XElement("DefaultDim2Code", accountDim2?.AccountNr ?? string.Empty),
                new XElement("DefaultDim2Name", accountDim2?.Name ?? string.Empty),
                new XElement("DefaultDim3Code", accountDim3?.AccountNr ?? string.Empty),
                new XElement("DefaultDim3Name", accountDim3?.Name ?? string.Empty),
                new XElement("DefaultDim4Code", accountDim4?.AccountNr ?? string.Empty),
                new XElement("DefaultDim4Name", accountDim4?.Name ?? string.Empty),
                new XElement("DefaultDim5Code", accountDim5?.AccountNr ?? string.Empty),
                new XElement("DefaultDim5Name", accountDim5?.Name ?? string.Empty),
                new XElement("DefaultDim6Code", String.Empty),
                new XElement("DefaultDim6Name", String.Empty)
                );

            invoiceHeadElement.Add(
                 new XElement("InvoiceState", customerInvoice.State),
                 new XElement("InvoiceOriginStatus", customerInvoice.Origin != null ? customerInvoice.Origin.Status : 0),
                 new XElement("BillingType", customerInvoice.BillingType));


            return invoiceHeadElement;
        }

        protected XElement CreateBillingInvoiceRowElement(int invoiceRowXmlId, EvaluatedSelection es, CustomerInvoiceRow customerInvoiceRow, Product product, List<AttestState> attestStates, List<VatCodeGridDTO> vatCodes, int householdProductId, int household50ProductId, int householdRutProductId, int householdGreen15ProductId, int householdGreen20ProductId, int householdGreen50ProductId, int fixedPriceProductId, int fixedPriceKeepPricesProductId, int rowHasStateToInvoice, List<GetTimeCodeTransactionsForOrderOverview_Result> timeCodeTransactions, bool useProjectTimeBlock, bool showSalesPricePermission, bool productRowDescriptionToUpperCase)
        {
            #region Prereq

            XElement invoiceRowElement = new XElement("InvoiceRow");
            invoiceRowElement.Add(
                new XAttribute("id", invoiceRowXmlId));
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();

            #endregion

            #region Product
            // Make sure ProductUnit is loaded
            if (!customerInvoiceRow.ProductUnitReference.IsLoaded)
                customerInvoiceRow.ProductUnitReference.Load();
            if (!customerInvoiceRow.CustomerInvoiceReference.IsLoaded)
                customerInvoiceRow.CustomerInvoiceReference.Load();
            if (!customerInvoiceRow.CustomerInvoice.OriginReference.IsLoaded)
                customerInvoiceRow.CustomerInvoice.OriginReference.Load();

            string productName = "";

            int? customerSysLanguageId = (from entry in entitiesReadOnly.Customer
                                          where entry.ActorCustomerId == customerInvoiceRow.CustomerInvoice.ActorId
                                        && entry.SysLanguageId != customerInvoiceRow.CustomerInvoice.Origin.ActorCompanyId
                                          select entry.SysLanguageId).FirstOrDefault();

            if (reportLanguageId == 0 && customerSysLanguageId.HasValue)
                reportLanguageId = customerSysLanguageId.Value;

            if (product != null && reportLanguageId > 0 && StringUtility.IsEqual(customerInvoiceRow.Text, product.Name, true))
            {
                var compTerm = TermManager.GetCompTerm(CompTermsRecordType.ProductName, product.ProductId, reportLanguageId);
                if (compTerm != null && !String.IsNullOrEmpty(compTerm.Name))
                    productName = productRowDescriptionToUpperCase ? compTerm.Name.ToUpper() : compTerm.Name;
            }

            if (String.IsNullOrEmpty(productName) && !String.IsNullOrEmpty(customerInvoiceRow.Text))
                productName = customerInvoiceRow.Text;

            if (String.IsNullOrEmpty(productName) && product != null)
                productName = product.Name;

            // Product unit
            string productUnitCode = string.Empty, productUnitName = string.Empty;
            if (customerInvoiceRow.ProductUnit != null)
            {
                productUnitCode = customerInvoiceRow.ProductUnit.Code;
                productUnitName = customerInvoiceRow.ProductUnit.Name;
            }

            if (reportLanguageId > 0 && customerInvoiceRow.ProductUnitId.HasValue)
            {
                var compTerm = TermManager.GetCompTerm(CompTermsRecordType.ProductUnitName, customerInvoiceRow.ProductUnitId.Value, reportLanguageId);
                if (compTerm != null && !String.IsNullOrEmpty(compTerm.Name))
                    productUnitName = productUnitCode = compTerm.Name;
            }

            int productVatType = -1;
            if (product != null)
                productVatType = ((InvoiceProduct)product).VatType;

            // HouseHoldDeduction
            int isHouseholdBaseProduct = 0;

            if (customerInvoiceRow.ProductId != null && (customerInvoiceRow.ProductId.Value == householdProductId || customerInvoiceRow.ProductId.Value == household50ProductId || customerInvoiceRow.ProductId.Value == householdRutProductId || customerInvoiceRow.ProductId.Value == householdGreen15ProductId || customerInvoiceRow.ProductId.Value == householdGreen20ProductId || customerInvoiceRow.ProductId.Value == householdGreen50ProductId))
                isHouseholdBaseProduct = 1;

            //Find vatcode
            bool isMixedVat = false;
            var rowVatCode = customerInvoiceRow.VatCodeId.HasValue ? vatCodes.FirstOrDefault(v => v.VatCodeId == (int)customerInvoiceRow.VatCodeId) : null;

            //Check if strange VAT Percent
            if (!vatCodes.IsNullOrEmpty())
            {
                foreach (var vatCode in vatCodes)
                {
                    isMixedVat = true;

                    if (vatCode.Percent == customerInvoiceRow.VatRate || customerInvoiceRow.VatRate == 0)
                    {
                        isMixedVat = false;
                        break;
                    }

                }
            }

            if (vatCodes.IsNullOrEmpty())
                isMixedVat = false;

            // Garantee
            int isGuarantee = 0;
            int productGuaranteeId = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.ProductGuarantee, 0, es.ActorCompanyId, 0);
            if (productGuaranteeId != 0 && customerInvoiceRow.ProductId == productGuaranteeId)
                isGuarantee = 1;

            #region Employee calculated cost

            bool useCalculatedCost = false;
            bool recalculateMarginalIncome = false;
            decimal minutes = 0;
            decimal employeeCost = 0;
            decimal calculatedCost = 0;
            decimal calculatedQuantity = 0;
            if (customerInvoiceRow.IsTimeProjectRow)
            {
                if (es.ReportTemplateType == SoeReportTemplateType.BillingOrderOverview && timeCodeTransactions.Count > 0)
                {
                    recalculateMarginalIncome = true;
                    foreach (var timeCodeTrans in timeCodeTransactions.Where(t => t.CustomerInvoiceRowId == customerInvoiceRow.CustomerInvoiceRowId))
                    {
                        if (useProjectTimeBlock)
                        {
                            decimal quantity = Convert.ToDecimal(CalendarUtility.TimeSpanToMinutes(timeCodeTrans.Stop, timeCodeTrans.Start)) / 60;

                            if (timeCodeTrans.CustomerInvoiceBillingType == (int)TermGroup_BillingType.Credit)
                                quantity = decimal.Negate(quantity);

                            decimal price = timeCodeTrans.UseCalculatedCost.HasValue && timeCodeTrans.UseCalculatedCost.Value ? EmployeeManager.GetEmployeeCalculatedCost(new Employee() { EmployeeId = timeCodeTrans.EmployeeId, ActorCompanyId = base.ActorCompanyId }, timeCodeTrans.TransactionDate.Date, customerInvoiceRow.CustomerInvoice?.ProjectId) : customerInvoiceRow.PurchasePrice;
                            decimal cost = quantity * price;

                            calculatedCost += cost;
                            calculatedQuantity += quantity;
                        }
                        else
                        {
                            decimal quantity = (timeCodeTrans.TransactionQuantity.HasValue && timeCodeTrans.TransactionQuantity.Value > 0 ? timeCodeTrans.TransactionQuantity.Value / 60 : 0);

                            if (timeCodeTrans.CustomerInvoiceBillingType == (int)TermGroup_BillingType.Credit)
                                quantity = decimal.Negate(quantity);

                            decimal price = timeCodeTrans.UseCalculatedCost.HasValue && timeCodeTrans.UseCalculatedCost.Value ? EmployeeManager.GetEmployeeCalculatedCost(new Employee() { EmployeeId = timeCodeTrans.EmployeeId, ActorCompanyId = base.ActorCompanyId }, timeCodeTrans.TransactionDate.Date, customerInvoiceRow.CustomerInvoice?.ProjectId) : customerInvoiceRow.PurchasePrice;
                            decimal cost = quantity * price;

                            calculatedCost += cost;
                            calculatedQuantity += quantity;
                        }
                    }
                }
                else
                {
                    if (es.GetDetailedInformation)
                    {
                        InvoiceProduct invoiceProduct = product as InvoiceProduct;
                        if (invoiceProduct == null || invoiceProduct.UseCalculatedCost == true)
                        {
                            useCalculatedCost = true;
                            List<TimeInvoiceTransaction> transactions = TimeTransactionManager.GetTimeInvoiceTransactionsForInvoiceRow(customerInvoiceRow.CustomerInvoiceRowId);

                            foreach (TimeInvoiceTransaction trans in transactions)
                            {
                                if (!trans.EmployeeReference.IsLoaded)
                                    trans.EmployeeReference.Load();

                                var employeeCalculatedCost = EmployeeManager.GetEmployeeCalculatedCost(entitiesReadOnly, trans.Employee, trans.TimeBlockDate.Date, customerInvoiceRow.CustomerInvoice?.ProjectId);
                                if (employeeCalculatedCost > 0)
                                {
                                    minutes += trans.Quantity;
                                    employeeCost += (trans.Quantity * employeeCalculatedCost);
                                }
                                else
                                {
                                    useCalculatedCost = false;
                                }
                            }

                            if (useCalculatedCost)
                                calculatedCost = minutes == 0 ? 0 : (employeeCost / minutes);
                        }
                    }
                }
            }


            decimal marginalIncome = customerInvoiceRow.MarginalIncome;
            decimal marginalIncomeRatio = customerInvoiceRow.MarginalIncomeRatio.HasValue ? customerInvoiceRow.MarginalIncomeRatio.Value : 0;
            if (recalculateMarginalIncome)
            {
                marginalIncome = customerInvoiceRow.SumAmount - (calculatedCost);
                marginalIncomeRatio = (customerInvoiceRow.SumAmount != 0 ? marginalIncome / customerInvoiceRow.SumAmount : 1) * 100;
                if (marginalIncome < 0 && marginalIncomeRatio > 0)
                    marginalIncomeRatio *= -1;
            }

            #endregion

            invoiceRowElement.Add(
                new XElement("InvoiceRowProductNumber", product != null ? product.Number : ""),
                new XElement("InvoiceRowProductName", productName),
                //new XElement("InvoiceRowProductName", product != null ? product.Name : customerInvoiceRow.Text),
                //new XElement("InvoiceRowProductDescription", product != null ? product.Description : ""),
                // 2012-11-23 Håkan: Product description replaced by ShowDescriptionAsTextRow setting on product
                new XElement("InvoiceRowProductDescription", ""),
                new XElement("InvoiceRowProductUnitCode", productUnitCode),
                new XElement("InvoiceRowProductUnitName", productUnitName));

            #endregion

            #region CustomerInvoiceRow


            invoiceRowElement.Add(
                new XElement("InvoiceRowQuantity", recalculateMarginalIncome ? calculatedQuantity : (customerInvoiceRow.Quantity.HasValue ? customerInvoiceRow.Quantity.Value : 0)),
                new XElement("InvoiceRowAmount", showSalesPricePermission ? customerInvoiceRow.AmountCurrency : 0),
                new XElement("InvoiceRowAmountBase", showSalesPricePermission ? customerInvoiceRow.Amount : 0),
                new XElement("InvoiceRowVatAmount", showSalesPricePermission ? customerInvoiceRow.VatAmountCurrency : 0),
                new XElement("InvoiceRowVatAmountBase", showSalesPricePermission ? customerInvoiceRow.VatAmount : 0),
                new XElement("InvoiceRowSumAmount", showSalesPricePermission ? customerInvoiceRow.SumAmountCurrency : 0),
                new XElement("InvoiceRowSumAmountCurrency", showSalesPricePermission ? customerInvoiceRow.SumAmountCurrency : 0), //only to support backwards compability
                new XElement("InvoiceRowSumAmountBase", showSalesPricePermission ? customerInvoiceRow.SumAmount : 0),
                new XElement("InvoiceRowDiscountPercent", customerInvoiceRow.DiscountPercent),
                new XElement("InvoiceRowDiscountAmount", customerInvoiceRow.DiscountAmountCurrency),
                new XElement("InvoiceRowDiscountAmountBase", customerInvoiceRow.DiscountAmount),
                new XElement("InvoiceRowDiscount2Percent", customerInvoiceRow.Discount2Percent),
                new XElement("InvoiceRowDiscount2Amount", customerInvoiceRow.Discount2AmountCurrency),
                new XElement("InvoiceRowDiscount2AmountBase", customerInvoiceRow.Discount2Amount),
                new XElement("InvoiceRowText", customerInvoiceRow.Text),
                new XElement("InvoiceRowType", customerInvoiceRow.Type),
                new XElement("InvoiceRowVatRate", customerInvoiceRow.VatRate),
                new XElement("InvoiceRowVatCodeName", rowVatCode?.Name ?? string.Empty),
                new XElement("InvoiceRowVatCodePercent", rowVatCode?.Percent ?? 0),
                new XElement("InvoiceRowVatCodeCode", rowVatCode?.Code ?? string.Empty),
                new XElement("InvoiceRowisStockRow", customerInvoiceRow.IsStockRow != null && customerInvoiceRow.IsStockRow == true),
                new XElement("InvoiceRowPurchasePrice", recalculateMarginalIncome ? (calculatedCost / calculatedQuantity) : (useCalculatedCost ? calculatedCost : customerInvoiceRow.PurchasePrice)),
                new XElement("InvoiceRowMarginalIncome", marginalIncome),
                new XElement("InvoiceRowMarginalIncomeRatio", marginalIncomeRatio),
                new XElement("isInvoicedRow", rowHasStateToInvoice),
                new XElement("InvoiceRowInvoiceQuantity", customerInvoiceRow.InvoiceQuantity ?? 0),
                new XElement("InvoiceRowIsHouseholdProduct", isHouseholdBaseProduct),
                new XElement("InvoiceRowIsGuarantee", isGuarantee),
                new XElement("InvoiceRowProductVatType", productVatType),
                new XElement("InvoiceRowSysWholesellerName", customerInvoiceRow.SysWholesellerName),
                new XElement("InvoiceRowRowNumber", customerInvoiceRow.RowNr)
                );

            #endregion

            #region Stock

            //Stock
            if (customerInvoiceRow.IsStockRow.HasValue && customerInvoiceRow.IsStockRow.Value && customerInvoiceRow.StockId != null)
            {
                invoiceRowElement.Add(
               new XElement("InvoiceRowStockCode", customerInvoiceRow.Stock.Code != null ? customerInvoiceRow.Stock.Code : ""),
               new XElement("InvoiceRowStockName", customerInvoiceRow.Stock.Name != null ? customerInvoiceRow.Stock.Name : ""));
            }
            else
            {
                invoiceRowElement.Add(
               new XElement("InvoiceRowStockCode", ""),
               new XElement("InvoiceRowStockName", ""));
            }
            //Shelf
            if (customerInvoiceRow.IsStockRow.HasValue && customerInvoiceRow.IsStockRow.Value && customerInvoiceRow.StockId != null)
            {
                if (!customerInvoiceRow.Stock.StockShelf.IsLoaded)
                    customerInvoiceRow.Stock.StockShelf.Load();

                var shelfCode = customerInvoiceRow.Stock.StockShelf.FirstOrDefault();
                if (shelfCode != null)
                {
                    invoiceRowElement.Add(
                   new XElement("InvoiceRowShelfCode", shelfCode.Code),
                   new XElement("InvoiceRowShelfName", shelfCode.Name));
                }
                else
                {
                    invoiceRowElement.Add(
                   new XElement("InvoiceRowShelfCode", ""),
                   new XElement("InvoiceRowShelfName", ""));
                }
            }
            else
            {
                invoiceRowElement.Add(
               new XElement("InvoiceRowShelfCode", ""),
               new XElement("InvoiceRowShelfName", ""));
            }



            #endregion

            #region ProductGroup

            //ProductGroup
            ProductGroup productGr = null;
            if (product != null && product.ProductGroupId != null)
                productGr = ProductGroupManager.GetProductGroup(product.ProductGroupId.Value);

            invoiceRowElement.Add(
                new XElement("InvoiceRowProductGroupCode", productGr != null ? productGr.Code : ""),
                new XElement("InvoiceRowProductGroupName", productGr != null ? productGr.Name : ""));

            #endregion

            string productCategoryCode = "", productCategoryName = "", productCategoryParentCode = "", productCategoryParentName = "";

            #region ProductCategory

            Category category = null;
            if (customerInvoiceRow.ProductId != null && customerInvoiceRow.CustomerInvoice != null && customerInvoiceRow.CustomerInvoice.Origin != null)
                category = CategoryManager.GetCategory(SoeCategoryType.Product, SoeCategoryRecordEntity.Product, (int)customerInvoiceRow.ProductId, customerInvoiceRow.CustomerInvoice.Origin.ActorCompanyId, onlyDefaultCategory: false);

            if (category != null)
            {
                productCategoryCode = category.Code;
                productCategoryName = category.Name;
                productCategoryParentCode = (from c in entitiesReadOnly.Category where c.CategoryId == category.ParentId select c.Code).FirstOrDefault();
                productCategoryParentName = (from c in entitiesReadOnly.Category where c.CategoryId == category.ParentId select c.Name).FirstOrDefault();
            }

            invoiceRowElement.Add(
                new XElement("InvoiceRowProductCategoryCode", productCategoryCode),
                new XElement("InvoiceRowProductCategoryName", productCategoryName),
                new XElement("InvoiceRowProductCategoryParentCode", productCategoryParentCode),
                new XElement("InvoiceRowProductCategoryParentName", productCategoryParentName));

            #endregion

            #region Lift product

            if (product != null && ((InvoiceProduct)product).CalculationType == (int)TermGroup_InvoiceProductCalculationType.Lift)
            {
                invoiceRowElement.Add(
                     new XElement("InvoiceRowProductCalculationType", ((InvoiceProduct)product).CalculationType),
                     new XElement("InvoiceRowLiftDate", customerInvoiceRow.Date.HasValue ? customerInvoiceRow.Date.Value : CalendarUtility.DATETIME_DEFAULT),
                     new XElement("InvoiceRowDate", CalendarUtility.DATETIME_DEFAULT),
                     new XElement("InvoiceRowDeliveryDateText", customerInvoiceRow.DeliveryDateText != null ? customerInvoiceRow.DeliveryDateText : String.Empty),
                     new XElement("isLiftRow", 1));
            }
            else
            {
                if (product != null && ((InvoiceProduct)product).CalculationType != 0)
                    invoiceRowElement.Add(new XElement("InvoiceRowProductCalculationType", ((InvoiceProduct)product).CalculationType));
                else if (fixedPriceProductId == customerInvoiceRow.ProductId || fixedPriceKeepPricesProductId == customerInvoiceRow.ProductId)
                    invoiceRowElement.Add(new XElement("InvoiceRowProductCalculationType", (int)TermGroup_InvoiceProductCalculationType.FixedPrice));
                else
                    invoiceRowElement.Add(new XElement("InvoiceRowProductCalculationType", 0));

                invoiceRowElement.Add(
                new XElement("InvoiceRowLiftDate", CalendarUtility.DATETIME_DEFAULT),
                new XElement("InvoiceRowDate", customerInvoiceRow.Date.HasValue ? customerInvoiceRow.Date.Value : CalendarUtility.DATETIME_DEFAULT),
                new XElement("InvoiceRowDeliveryDateText", customerInvoiceRow.DeliveryDateText != null ? customerInvoiceRow.DeliveryDateText : String.Empty),
                new XElement("isLiftRow", 0));

            }

            #endregion

            #region AttestState

            AttestState attestState = null;
            if (customerInvoiceRow.AttestStateId.HasValue)
                attestState = attestStates.FirstOrDefault(o => o.AttestStateId == customerInvoiceRow.AttestStateId.Value);

            invoiceRowElement.Add(
                new XElement("AttestState", attestState?.Name ?? string.Empty));

            invoiceRowElement.Add(
                new XElement("IsClosedRow", attestState != null && attestState.Closed ? 1 : 0));


            #endregion

            #region Created and modified

            invoiceRowElement.Add(
                new XElement("isEDI", customerInvoiceRow.EdiEntryId.HasValue ? 1 : 0),
                new XElement("isTimeProject", customerInvoiceRow.IsTimeProjectRow.ToInt()),
                new XElement("isMixedVat", isMixedVat.ToInt()),
                new XElement("Created", customerInvoiceRow.Created.HasValue ? customerInvoiceRow.Created.Value : CalendarUtility.DATETIME_DEFAULT),
                new XElement("CreatedBy", customerInvoiceRow.CreatedBy != null ? customerInvoiceRow.CreatedBy : string.Empty),
                new XElement("Modified", customerInvoiceRow.Modified.HasValue ? customerInvoiceRow.Modified.Value : CalendarUtility.DATETIME_DEFAULT),
                new XElement("ModifiedBy", customerInvoiceRow.ModifiedBy != null ? customerInvoiceRow.ModifiedBy : string.Empty));

            #endregion

            #region Weight

            if (product != null && ((InvoiceProduct)product).Weight.HasValue)
            {
                invoiceRowElement.Add(
                new XElement("InvoiceRowProductWeight", ((InvoiceProduct)product).Weight.Value),
                new XElement("InvoiceRowTotalWeight", ((InvoiceProduct)product).Weight.Value * customerInvoiceRow.Quantity));
            }
            else
            {
                invoiceRowElement.Add(
                new XElement("InvoiceRowProductWeight", 0),
                new XElement("InvoiceRowTotalWeight", 0));
            }

            #endregion

            return invoiceRowElement;
        }

        public XElement CreateProjectInvoiceDayElement(int projectInvoiceDayXmlId, ProjectTimeBlock projectTimeBlock, List<TimeCode> timeCodes, int actorCompanyId, bool showStartStopInTimeReport)
        {
            TimeCodeTransaction timeCodeTransaction = projectTimeBlock.TimeCodeTransaction.FirstOrDefault(tct => tct.Type == (int)TimeCodeTransactionType.TimeProject && tct.State == (int)SoeEntityState.Active);
            TimeCode timeCode = null;
            if (timeCodeTransaction != null)
            {
                timeCode = timeCodes?.FirstOrDefault(x => x.TimeCodeId == timeCodeTransaction.TimeCodeId);
                if (timeCode == null)
                {
                    //Maybe inactive....
                    timeCode = TimeCodeManager.GetTimeCode(timeCodeTransaction.TimeCodeId, actorCompanyId, false);
                }
            }

            TimeDeviationCause timeDeviationCause = TimeDeviationCauseManager.GetTimeDeviationCause(projectTimeBlock.TimeDeviationCauseId, actorCompanyId, false);
            string timeDeviationCauseName = timeDeviationCause?.Name ?? String.Empty;

            return new XElement("ProjectInvoiceDay",
                    new XAttribute("id", projectInvoiceDayXmlId),
                    new XElement("TCCode", timeCode?.Code ?? string.Empty),
                    new XElement("TCName", timeCode?.Name ?? string.Empty),
                    new XElement("InvoiceTimeInMinutes", projectTimeBlock.InvoiceQuantity),
                    new XElement("Date", projectTimeBlock.TimeBlockDate?.Date.ToShortDateString() ?? string.Empty),
                    new XElement("Note", projectTimeBlock.ExternalNote),
                    new XElement("ExternalNote", string.Empty),
                    new XElement("IsoDate", DateTime.Now.Date.ToShortDateString()),
                    new XElement("TDName", timeDeviationCauseName),
                    new XElement("TBStartTime", showStartStopInTimeReport ? projectTimeBlock.StartTime.ToShortTimeString() : string.Empty),
                    new XElement("TBStopTime", showStartStopInTimeReport ? projectTimeBlock.StopTime.ToShortTimeString() : string.Empty));
        }

        protected static XElement CreateOrderContractChangeElement(CompEntities entities, CreateReportResult es, BillingReportParamsDTO reportParams, CustomerInvoice customerInvoice, List<CustomerInvoiceSmallExDTO> connectInvoices)
        {
            #region Content


            #region OrderContractChange

            int invoiceOrderContractChangeXmlId = 1;

            var invoiceingDate = connectInvoices.OrderByDescending(i => i.InvoiceDate).FirstOrDefault()?.InvoiceDate;

            XElement invoiceElement = new XElement("Invoice",
             new XAttribute("id", invoiceOrderContractChangeXmlId),
            new XElement("OrderNr", customerInvoice.InvoiceNr),
            //       new XElement("InvoiceCreated", customerInvoice.Created.HasValue ? customerInvoice.Created.Value.ToShortDateString() : CalendarUtility.DATETIME_DEFAULT.ToShortDateString()),    
            //       new XElement("InvoiceCreatedBy", customerInvoice.CreatedBy),
            new XElement("OrderDate", customerInvoice.OrderDate),
            new XElement("OrderAmount", customerInvoice.SumAmount),
            new XElement("InvoiceLabel", customerInvoice.InvoiceLabel),
            new XElement("InvoiceDate", customerInvoice.InvoiceDate),
            new XElement("InvoiceingDate", invoiceingDate?.ToShortDateString() ?? string.Empty)
            );
            invoiceOrderContractChangeXmlId++;
            #endregion


            if (invoiceOrderContractChangeXmlId == 1)
            {

                invoiceElement = new XElement("Invoice",
                new XAttribute("id", 1),
                new XElement("InvoiceNr", ""),
                new XElement("OrderDate", CalendarUtility.DATETIME_DEFAULT.ToShortDateString()),
                new XElement("OrderAmount", 0),
                new XElement("InvoiceLabel", 0),
                new XElement("InvoiceDate", CalendarUtility.DATETIME_DEFAULT.ToShortDateString())
              );
            }
            #endregion

            return invoiceElement;
        }

        protected XElement CreateExpenseElement(CompEntities entities, EvaluatedSelection es, XElement expenseReportElement, CustomerInvoice customerInvoice, int includeExpenseInReport, out int nrOfExpenseRows)
        {
            #region Content
            List<XElement> invoiceElements = new List<XElement>();
            int invoiceXmlId = 1;

            if (customerInvoice == null)
            {
                nrOfExpenseRows = 0;
                return expenseReportElement;
            }

            #region InvoiceExpenseRow

            int invoiceExpenseRowXmlId = 1;
            var expenseRowsForInvoice = new List<ExpenseRowGridDTO>();

            if (customerInvoice.Origin != null && customerInvoice.Origin.Type == (int)SoeOriginType.Order)
            {
                expenseRowsForInvoice = ExpenseManager.GetExpenseRowsForGrid(customerInvoice.InvoiceId, es.ActorCompanyId, this.UserId, es.RoleId, true);
            }
            else
            {
                if (customerInvoice.Origin != null && customerInvoice.Origin.Type == (int)SoeOriginType.CustomerInvoice)
                {
                    List<CustomerInvoiceRow> invoiceRows = InvoiceManager.GetCustomerInvoiceRows(entities, customerInvoice.InvoiceId, false);
                    foreach (CustomerInvoiceRow row in invoiceRows.Where(r => r.IsExpense()))
                    {
                        if (customerInvoice.Origin.Type == (int)SoeOriginType.CustomerInvoice)
                        {
                            List<CustomerInvoiceRow> parentRows = InvoiceManager.GetParentCustomerInvoiceRows(entities, row.CustomerInvoiceRowId, es.ActorCompanyId);
                            foreach (CustomerInvoiceRow crow in parentRows)
                            {
                                var expenseRowForInvoice = ExpenseManager.GetExpenseRowsForGrid(crow.InvoiceId, es.ActorCompanyId, this.UserId, es.RoleId, true, crow.CustomerInvoiceRowId);
                                expenseRowsForInvoice.AddRange(expenseRowForInvoice);
                            }
                        }
                    }
                }
            }
            if (expenseRowsForInvoice.Count > 0 && (includeExpenseInReport != (int)TermGroup_IncludeExpenseInReportType.None || es.ReportTemplateType == SoeReportTemplateType.ExpenseReport))
            {
                #region Invoice

                Customer customer = customerInvoice.Actor?.Customer;

                XElement invoiceElement = new XElement("Invoices",
                        new XAttribute("id", invoiceXmlId),
                        new XElement("InvoiceNr", customerInvoice.InvoiceNr),
                        new XElement("InvoiceCreated", customerInvoice.Created.HasValue ? customerInvoice.Created.Value.ToShortDateString() : CalendarUtility.DATETIME_DEFAULT.ToShortDateString()),
                        new XElement("InvoiceCreatedBy", customerInvoice.CreatedBy),
                        new XElement("InvoiceCustomerNr", customer?.CustomerNr ?? string.Empty),
                        new XElement("InvoiceCustomerName", customer?.Name ?? string.Empty));

                #endregion

                foreach (var expenseRow in expenseRowsForInvoice)
                {
                    //Check expenserows 
                    if (expenseRow.InvoiceRowAttestStateId == 0)
                        continue;

                    if (es.HasDateInterval && (expenseRow.From < es.DateFrom.Date || expenseRow.From > es.DateTo.Date))
                        continue;

                    XElement invoiceExpenseRowElement = new XElement("CustomerInvoiceExpenseRow",
                        new XAttribute("id", invoiceExpenseRowXmlId),
                        new XElement("InvoiceNr", customerInvoice.InvoiceNr),
                        new XElement("InvoiceCustomerName", customer.Name),
                        new XElement("InvoiceExpenseRowEmployeeName", expenseRow.EmployeeName),
                        new XElement("InvoiceExpenseRowTimeCodeName", expenseRow.TimeCodeName),
                        new XElement("InvoiceExpenseRowPayrollAttestStateName", expenseRow.PayrollAttestStateName),
                        new XElement("InvoiceExpenseRowQuantity", expenseRow.Quantity),
                        new XElement("InvoiceExpenseRowQuantityType", expenseRow.TimeCodeRegistrationType),
                        new XElement("InvoiceExpenseRowUnitPrice", expenseRow.UnitPrice),
                        new XElement("InvoiceExpenseRowAmountExVat", expenseRow.AmountExVat),
                        new XElement("InvoiceExpenseRowVat", expenseRow.Vat),
                        new XElement("InvoiceExpenseRowAmount", expenseRow.Amount),
                        new XElement("InvoiceExpenseRowInvoicedAmount", expenseRow.InvoicedAmount),
                        new XElement("InvoiceExpenseRowComment", expenseRow.Comment),
                        new XElement("InvoiceExpenseRowExternalComment", expenseRow.ExternalComment),
                        new XElement("InvoiceExpenseRowDate", expenseRow.From.ToShortDateString())
                        );

                    invoiceElement.Add(invoiceExpenseRowElement);
                    invoiceExpenseRowXmlId++;
                }
                invoiceElements.Add(invoiceElement);
                invoiceXmlId++;
            }
            #endregion


            if (invoiceXmlId == 1)
            {
                #region Default element Invoice

                XElement defInvoiceElement = new XElement("Invoices",
                        new XAttribute("id", 1),
                        new XElement("InvoiceNr", ""),
                        new XElement("InvoiceCreated", CalendarUtility.DATETIME_DEFAULT.ToShortDateString()),
                        new XElement("InvoiceCreatedBy", "")
                        );
                XElement defInvoiceExpenseRowElement = new XElement("CustomerInvoiceExpenseRow",
                            new XAttribute("id", 1),
                             new XElement("InvoiceNr", " "),
                            new XElement("InvoiceCustomerName", " "),
                            new XElement("InvoiceExpenseRowEmployeeName", " "),
                            new XElement("InvoiceExpenseRowTimeCodeName", " "),
                            new XElement("InvoiceExpenseRowPayrollAttestStateName", " "),
                            new XElement("InvoiceExpenseRowQuantity", 0),
                            new XElement("InvoiceExpenseRowQuantityType", 0),
                            new XElement("InvoiceExpenseRowUnitPrice", 0),
                            new XElement("InvoiceExpenseRowAmountExVat", 0),
                            new XElement("InvoiceExpenseRowVat", 0),
                            new XElement("InvoiceExpenseRowAmount", 0),
                            new XElement("InvoiceExpenseRowInvoicedAmount", 0),
                            new XElement("InvoiceExpenseRowComment", " "),
                            new XElement("InvoiceExpenseRowExternalComment", " "),
                            new XElement("InvoiceExpenseRowDate", CalendarUtility.DATETIME_DEFAULT.ToShortDateString())
                            );
                defInvoiceElement.Add(defInvoiceExpenseRowElement);
                invoiceElements.Add(defInvoiceElement);
            }
            #endregion

            expenseReportElement.Add(invoiceElements);
            nrOfExpenseRows = 0;
            if (invoiceXmlId > 1)
            {
                nrOfExpenseRows = 2;
            }
            #endregion
            return expenseReportElement;
        }

        protected XElement CreateExpenseElement(CompEntities entities, CreateReportResult es, BillingReportParamsDTO reportParams, XElement expenseReportElement, CustomerInvoice customerInvoice, int includeExpenseInReport, out int nrOfExpenseRows)
        {
            #region Content
            List<XElement> invoiceElements = new List<XElement>();
            int invoiceXmlId = 1;

            if (customerInvoice == null)
            {
                nrOfExpenseRows = 0;
                return expenseReportElement;
            }

            #region InvoiceExpenseRow

            int invoiceExpenseRowXmlId = 1;
            var expenseRowsForInvoice = new List<ExpenseRowGridDTO>();

            if (customerInvoice.Origin != null && customerInvoice.Origin.Type == (int)SoeOriginType.Order)
            {
                expenseRowsForInvoice = ExpenseManager.GetExpenseRowsForGrid(entities, customerInvoice.InvoiceId, es.ActorCompanyId, this.UserId, es.RoleId, true);
            }
            else
            {
                if (customerInvoice.Origin != null && customerInvoice.Origin.Type == (int)SoeOriginType.CustomerInvoice)
                {
                    List<CustomerInvoiceRow> invoiceRows = InvoiceManager.GetCustomerInvoiceRows(entities, customerInvoice.InvoiceId, false);
                    foreach (CustomerInvoiceRow row in invoiceRows.Where(r => r.IsExpense()))
                    {
                        if (customerInvoice.Origin.Type == (int)SoeOriginType.CustomerInvoice)
                        {
                            List<CustomerInvoiceRow> parentRows = InvoiceManager.GetParentCustomerInvoiceRows(entities, row.CustomerInvoiceRowId, es.ActorCompanyId);
                            foreach (CustomerInvoiceRow crow in parentRows)
                            {
                                var expenseRowForInvoice = ExpenseManager.GetExpenseRowsForGrid(entities, crow.InvoiceId, es.ActorCompanyId, this.UserId, es.RoleId, true, crow.CustomerInvoiceRowId);
                                expenseRowsForInvoice.AddRange(expenseRowForInvoice);
                            }
                        }
                    }
                }
            }
            if (expenseRowsForInvoice.Count > 0 && (includeExpenseInReport != (int)TermGroup_IncludeExpenseInReportType.None || es.ReportTemplateType == SoeReportTemplateType.ExpenseReport))
            {
                #region Invoice

                Customer customer = customerInvoice.Actor?.Customer;

                XElement invoiceElement = new XElement("Invoices",
                        new XAttribute("id", invoiceXmlId),
                        new XElement("InvoiceNr", customerInvoice.InvoiceNr),
                        new XElement("InvoiceCreated", customerInvoice.Created.HasValue ? customerInvoice.Created.Value.ToShortDateString() : CalendarUtility.DATETIME_DEFAULT.ToShortDateString()),
                        new XElement("InvoiceCreatedBy", customerInvoice.CreatedBy),
                        new XElement("InvoiceCustomerNr", customer?.CustomerNr ?? string.Empty),
                        new XElement("InvoiceCustomerName", customer?.Name ?? string.Empty));

                #endregion

                foreach (var expenseRow in expenseRowsForInvoice)
                {
                    //Check expenserows 
                    if (expenseRow.InvoiceRowAttestStateId == 0)
                        continue;

                    if (reportParams.HasDateInterval && (expenseRow.From < reportParams.DateFrom.Date || expenseRow.From > reportParams.DateTo.Date))
                        continue;

                    XElement invoiceExpenseRowElement = new XElement("CustomerInvoiceExpenseRow",
                        new XAttribute("id", invoiceExpenseRowXmlId),
                        new XElement("InvoiceNr", customerInvoice.InvoiceNr),
                        new XElement("InvoiceCustomerName", customer.Name),
                        new XElement("InvoiceExpenseRowEmployeeName", expenseRow.EmployeeName),
                        new XElement("InvoiceExpenseRowTimeCodeName", expenseRow.TimeCodeName),
                        new XElement("InvoiceExpenseRowPayrollAttestStateName", expenseRow.PayrollAttestStateName),
                        new XElement("InvoiceExpenseRowQuantity", expenseRow.Quantity),
                        new XElement("InvoiceExpenseRowQuantityType", expenseRow.TimeCodeRegistrationType),
                        new XElement("InvoiceExpenseRowUnitPrice", expenseRow.UnitPrice),
                        new XElement("InvoiceExpenseRowAmountExVat", expenseRow.AmountExVat),
                        new XElement("InvoiceExpenseRowVat", expenseRow.Vat),
                        new XElement("InvoiceExpenseRowAmount", expenseRow.Amount),
                        new XElement("InvoiceExpenseRowInvoicedAmount", expenseRow.InvoicedAmount),
                        new XElement("InvoiceExpenseRowComment", expenseRow.Comment),
                        new XElement("InvoiceExpenseRowExternalComment", expenseRow.ExternalComment),
                        new XElement("InvoiceExpenseRowDate", expenseRow.From.ToShortDateString())
                        );

                    invoiceElement.Add(invoiceExpenseRowElement);
                    invoiceExpenseRowXmlId++;
                }
                invoiceElements.Add(invoiceElement);
                invoiceXmlId++;
            }
            #endregion


            if (invoiceXmlId == 1)
            {
                #region Default element Invoice

                XElement defInvoiceElement = new XElement("Invoices",
                        new XAttribute("id", 1),
                        new XElement("InvoiceNr", ""),
                        new XElement("InvoiceCreated", CalendarUtility.DATETIME_DEFAULT.ToShortDateString()),
                        new XElement("InvoiceCreatedBy", "")
                        );
                XElement defInvoiceExpenseRowElement = new XElement("CustomerInvoiceExpenseRow",
                            new XAttribute("id", 1),
                             new XElement("InvoiceNr", " "),
                            new XElement("InvoiceCustomerName", " "),
                            new XElement("InvoiceExpenseRowEmployeeName", " "),
                            new XElement("InvoiceExpenseRowTimeCodeName", " "),
                            new XElement("InvoiceExpenseRowPayrollAttestStateName", " "),
                            new XElement("InvoiceExpenseRowQuantity", 0),
                            new XElement("InvoiceExpenseRowQuantityType", 0),
                            new XElement("InvoiceExpenseRowUnitPrice", 0),
                            new XElement("InvoiceExpenseRowAmountExVat", 0),
                            new XElement("InvoiceExpenseRowVat", 0),
                            new XElement("InvoiceExpenseRowAmount", 0),
                            new XElement("InvoiceExpenseRowInvoicedAmount", 0),
                            new XElement("InvoiceExpenseRowComment", " "),
                            new XElement("InvoiceExpenseRowExternalComment", " "),
                            new XElement("InvoiceExpenseRowDate", CalendarUtility.DATETIME_DEFAULT.ToShortDateString())
                            );
                defInvoiceElement.Add(defInvoiceExpenseRowElement);
                invoiceElements.Add(defInvoiceElement);
            }
            #endregion

            expenseReportElement.Add(invoiceElements);
            nrOfExpenseRows = 0;
            if (invoiceXmlId > 1)
            {
                nrOfExpenseRows = 2;
            }
            #endregion
            return expenseReportElement;
        }

        protected XElement CreateTimeProjectElement(CompEntities entities, EvaluatedSelection es, XElement timeProjectReportElement, List<TimeCode> timeCodes, int invoiceId, int actorCompanyId, bool returnProjectElement, out int nrOfTimeProjectRows)
        {
            #region Content

            XElement projectElement = null;
            int projectXmlId = 1;
            nrOfTimeProjectRows = 0;
            bool showStartStopInTimeReport = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.BillingShowStartStopInTimeReport, this.UserId, this.ActorCompanyId, 0, false);

            if (es.SB_IncludeOnlyInvoiced)
            {

                Invoice invoice = InvoiceManager.GetInvoice(entities, invoiceId);

                string invoiceNr = invoice.InvoiceNr;

                List<Project> projects = new List<Project>();
                List<CustomerInvoiceRow> invoiceRows = InvoiceManager.GetCustomerInvoiceRows(entities, invoiceId, false);

                int attestStateTransferredOrderToInvoiceId = 0;
                List<AttestTransition> attestTransitions = null;
                if (invoice.Origin.Type == (int)SoeOriginType.Order)
                {
                    attestTransitions = AttestManager.GetAttestTransitions(entities, new List<TermGroup_AttestEntity> { TermGroup_AttestEntity.Order }, SoeModule.Billing, false, actorCompanyId);
                    attestStateTransferredOrderToInvoiceId = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.BillingStatusTransferredOrderToInvoice, 0, actorCompanyId, 0);
                }

                foreach (CustomerInvoiceRow row in invoiceRows.Where(r => r.Type == (int)SoeInvoiceRowType.ProductRow && r.IsTimeProjectRow))
                {
                    if (invoice.Origin.Type == (int)SoeOriginType.CustomerInvoice)
                    {
                        List<CustomerInvoiceRow> parentRows = InvoiceManager.GetParentCustomerInvoiceRows(entities, row.CustomerInvoiceRowId, actorCompanyId);

                        if (parentRows.Count > 0)
                        {
                            foreach (CustomerInvoiceRow parentRow in parentRows)
                            {
                                List<TimeInvoiceTransaction> trans = TimeTransactionManager.GetTimeInvoiceTransactionsForInvoiceRow(entities, parentRow.CustomerInvoiceRowId);

                                foreach (TimeInvoiceTransaction invTransaction in trans)
                                {
                                    bool addProject = false;
                                    bool addEmployee = false;

                                    if (!invTransaction.TimeCodeTransactionReference.IsLoaded)
                                        invTransaction.TimeCodeTransactionReference.Load();

                                    if (invTransaction.InvoiceQuantity == 0 || invTransaction.TimeCodeTransaction.InvoiceQuantity == 0)
                                        continue;

                                    Project p = projects.FirstOrDefault(pr => pr.ProjectId == invTransaction.TimeCodeTransaction.ProjectId);

                                    if (p == null)
                                    {
                                        addProject = true;
                                        p = ProjectManager.GetProject(entities, (int)invTransaction.TimeCodeTransaction.ProjectId);
                                    }

                                    if (!invTransaction.TimeCodeTransaction.TimeCodeReference.IsLoaded)
                                        invTransaction.TimeCodeTransaction.TimeCodeReference.Load();

                                    if (!invTransaction.TimeCodeTransaction.ProjectInvoiceDayReference.IsLoaded)
                                        invTransaction.TimeCodeTransaction.ProjectInvoiceDayReference.Load();

                                    if ((es.HasDateInterval) && !invTransaction.TimeBlockDateReference.IsLoaded)
                                        invTransaction.TimeBlockDateReference.Load();

                                    Employee e = null;

                                    if (p.Employees != null)
                                        e = p.Employees.FirstOrDefault(em => em.EmployeeId == (int)invTransaction.EmployeeId);
                                    else
                                        p.Employees = new List<Employee>();

                                    if (e == null)
                                    {
                                        if (!invTransaction.EmployeeReference.IsLoaded)
                                            invTransaction.EmployeeReference.Load();

                                        addEmployee = true;
                                        e = invTransaction.Employee;
                                    }

                                    if (e.Transactions == null)
                                        e.Transactions = new List<TimeInvoiceTransaction>();

                                    e.Transactions.Add(invTransaction);

                                    if (addEmployee)
                                        p.Employees.Add(e);

                                    if (addProject)
                                        projects.Add(p);
                                }
                            }
                        }
                        else
                        {
                            List<TimeInvoiceTransaction> trans = TimeTransactionManager.GetTimeInvoiceTransactionsForInvoiceRow(entities, row.CustomerInvoiceRowId);

                            foreach (TimeInvoiceTransaction invTransaction in trans)
                            {
                                bool addProject = false;
                                bool addEmployee = false;

                                if (!invTransaction.TimeCodeTransactionReference.IsLoaded)
                                    invTransaction.TimeCodeTransactionReference.Load();

                                if (invTransaction.InvoiceQuantity == 0 || invTransaction.TimeCodeTransaction.InvoiceQuantity == 0)
                                    continue;

                                Project p = projects.FirstOrDefault(pr => pr.ProjectId == invTransaction.TimeCodeTransaction.ProjectId);

                                if (p == null)
                                {
                                    addProject = true;
                                    p = ProjectManager.GetProject(entities, (int)invTransaction.TimeCodeTransaction.ProjectId);
                                }

                                if (!invTransaction.TimeCodeTransaction.TimeCodeReference.IsLoaded)
                                    invTransaction.TimeCodeTransaction.TimeCodeReference.Load();

                                if (!invTransaction.TimeCodeTransaction.ProjectInvoiceDayReference.IsLoaded)
                                    invTransaction.TimeCodeTransaction.ProjectInvoiceDayReference.Load();

                                if ((es.HasDateInterval) && !invTransaction.TimeBlockDateReference.IsLoaded)
                                    invTransaction.TimeBlockDateReference.Load();

                                Employee e = null;

                                if (p.Employees != null)
                                    e = p.Employees.FirstOrDefault(em => em.EmployeeId == (int)invTransaction.EmployeeId);
                                else
                                    p.Employees = new List<Employee>();

                                if (e == null)
                                {
                                    if (!invTransaction.EmployeeReference.IsLoaded)
                                        invTransaction.EmployeeReference.Load();

                                    addEmployee = true;
                                    e = invTransaction.Employee;
                                }

                                if (e.Transactions == null)
                                    e.Transactions = new List<TimeInvoiceTransaction>();

                                e.Transactions.Add(invTransaction);

                                if (addEmployee)
                                    p.Employees.Add(e);

                                if (addProject)
                                    projects.Add(p);
                            }
                        }
                    }
                    else if (invoice.Origin.Type == (int)SoeOriginType.Order)
                    {
                        var rowHasStateToInvoice = attestTransitions.IsNullOrEmpty() || attestTransitions.Any(x => x.AttestStateFromId == row.AttestStateId && x.AttestStateToId == attestStateTransferredOrderToInvoiceId);
                        if (!rowHasStateToInvoice && es.SB_IncludeOnlyInvoiced)
                            continue;

                        List<TimeInvoiceTransaction> trans = TimeTransactionManager.GetTimeInvoiceTransactionsForInvoiceRow(entities, row.CustomerInvoiceRowId);

                        foreach (TimeInvoiceTransaction invTransaction in trans)
                        {
                            bool addProject = false;
                            bool addEmployee = false;

                            if (!invTransaction.TimeCodeTransactionReference.IsLoaded)
                                invTransaction.TimeCodeTransactionReference.Load();

                            if (invTransaction.InvoiceQuantity == 0 || invTransaction.TimeCodeTransaction.InvoiceQuantity == 0)
                                continue;

                            Project p = projects.FirstOrDefault(pr => pr.ProjectId == invTransaction.TimeCodeTransaction.ProjectId);

                            if (p == null)
                            {
                                addProject = true;
                                p = ProjectManager.GetProject(entities, (int)invTransaction.TimeCodeTransaction.ProjectId);
                            }

                            if (!invTransaction.TimeCodeTransaction.TimeCodeReference.IsLoaded)
                                invTransaction.TimeCodeTransaction.TimeCodeReference.Load();

                            if (!invTransaction.TimeCodeTransaction.ProjectInvoiceDayReference.IsLoaded)
                                invTransaction.TimeCodeTransaction.ProjectInvoiceDayReference.Load();

                            if (es.HasDateInterval && !invTransaction.TimeBlockDateReference.IsLoaded)
                                invTransaction.TimeBlockDateReference.Load();

                            Employee e = null;

                            if (p.Employees != null)
                                e = p.Employees.FirstOrDefault(em => em.EmployeeId == (int)invTransaction.EmployeeId);
                            else
                                p.Employees = new List<Employee>();

                            if (e == null)
                            {
                                if (!invTransaction.EmployeeReference.IsLoaded)
                                    invTransaction.EmployeeReference.Load();

                                addEmployee = true;
                                e = invTransaction.Employee;
                            }

                            if (e.Transactions == null)
                                e.Transactions = new List<TimeInvoiceTransaction>();

                            e.Transactions.Add(invTransaction);

                            if (addEmployee)
                                p.Employees.Add(e);

                            if (addProject)
                                projects.Add(p);
                        }
                    }
                }

                if (projects.Count == 0)
                {
                    bool addProject = true;
                    Project p = ProjectManager.GetProject(entities, (int)invoice.ProjectId);
                    if (addProject)
                        projects.Add(p);
                }

                foreach (Project proj in projects)
                {
                    int employeeXmlId = 1;
                    int projectInvoiceDayXmlId = 1;
                    List<XElement> employeeElements = new List<XElement>();

                    List<Employee> employees = new List<Employee>();

                    if (proj.Employees != null)
                    {
                        employees.AddRange(proj.Employees);
                        foreach (Employee emp in employees)
                        {
                            if (!emp.ContactPersonReference.IsLoaded)
                                emp.ContactPersonReference.Load();

                            XElement employeeElement = new XElement("Employee",
                                        new XAttribute("id", employeeXmlId),
                                        new XElement("EmployeeNr", emp.EmployeeNr),
                                        new XElement("EmployeeName", emp.Name));

                            nrOfTimeProjectRows = +emp.Transactions.Count;

                            List<TimeInvoiceTransaction> transactions = new List<TimeInvoiceTransaction>();
                            transactions.AddRange(emp.Transactions.Where(t => t.EmployeeId == emp.EmployeeId && t.TimeCodeTransaction?.ProjectId == proj.ProjectId));

                            if (es.HasDateInterval)
                            {
                                if (es.DateFrom != CalendarUtility.DATETIME_DEFAULT)
                                {
                                    transactions = transactions.Where(t => t.TimeBlockDateId != null && t.TimeBlockDateId != 0 && t.TimeBlockDate.Date >= es.DateFrom).ToList();
                                }

                                if (es.DateTo != CalendarUtility.DATETIME_DEFAULT)
                                {
                                    transactions = transactions.Where(t => t.TimeBlockDateId != null && t.TimeBlockDateId != 0 && t.TimeBlockDate.Date <= es.DateTo).ToList();
                                }
                            }

                            foreach (var invoiceTransactions in transactions.GroupBy(x => x.TimeCodeTransactionId))
                            {
                                var trans = invoiceTransactions.First();

                                if (!trans.TimeCodeTransactionReference.IsLoaded)
                                    trans.TimeCodeTransactionReference.Load();

                                var timeCodeTransaction = trans.TimeCodeTransaction;

                                if (timeCodeTransaction == null)
                                    continue;

                                XElement dayElement = null;

                                if (timeCodeTransaction.ProjectTimeBlockId.HasValue && timeCodeTransaction.ProjectTimeBlockId > 0)
                                {
                                    var projectTimeBlock = timeCodeTransaction.ProjectTimeBlock != null ? timeCodeTransaction.ProjectTimeBlock : ProjectManager.GetProjectTimeBlock(entities, (int)timeCodeTransaction.ProjectTimeBlockId);
                                    if (projectTimeBlock != null)
                                    {
                                        dayElement = CreateProjectInvoiceDayElement(projectInvoiceDayXmlId, projectTimeBlock, timeCodes, es.ActorCompanyId, showStartStopInTimeReport);
                                    }
                                }
                                else if (trans.TimeCodeTransaction.ProjectInvoiceDay != null)
                                {
                                    dayElement = new XElement("ProjectInvoiceDay",
                                                    new XAttribute("id", projectInvoiceDayXmlId),
                                                    new XElement("TCCode", timeCodeTransaction.TimeCode != null ? timeCodeTransaction.TimeCode.Code : string.Empty),
                                                    new XElement("TCName", timeCodeTransaction.TimeCode != null ? timeCodeTransaction.TimeCode.Name : string.Empty),
                                                    new XElement("InvoiceTimeInMinutes", timeCodeTransaction.ProjectInvoiceDay.InvoiceTimeInMinutes),
                                                    new XElement("Date", timeCodeTransaction.ProjectInvoiceDay.Date.ToShortDateString()),
                                                    new XElement("Note", timeCodeTransaction.ProjectInvoiceDay.Note),
                                                    new XElement("ExternalNote", timeCodeTransaction.Comment),
                                                    new XElement("IsoDate", timeCodeTransaction.ProjectInvoiceDay.Date.ToString("yyyy-MM-dd")),
                                                    new XElement("TDName", string.Empty),
                                                    new XElement("TBStartTime", string.Empty),
                                                    new XElement("TBStopTime", string.Empty));
                                }

                                if (dayElement != null)
                                {
                                    employeeElement.Add(dayElement);
                                    projectInvoiceDayXmlId++;
                                }
                            }

                            employeeElements.Add(employeeElement);
                            employeeXmlId++;

                        }
                    }
                    if (proj.Employees == null || employeeElements.Count == 0)
                    {
                        //Add default element
                        XElement defaultEmployeeElement = new XElement("Employee",
                                new XAttribute("id", 1),
                                new XElement("EmployeeNr", 0),
                                new XElement("EmployeeName", ""));

                        XElement defaultDayElement = new XElement("ProjectInvoiceDay",
                            new XAttribute("id", 1),
                            new XElement("InvoiceTimeInMinutes", 0),
                            new XElement("Date", "00:00"),
                            new XElement("Note", "00:00"),
                            new XElement("ExternalNote", string.Empty),
                            new XElement("IsoDate", DateTime.Now.Date.ToString("yyyy-MM-dd")));

                        defaultEmployeeElement.Add(defaultDayElement);
                        employeeElements.Add(defaultEmployeeElement);
                    }

                    projectElement = new XElement("Project",
                        new XAttribute("id", projectXmlId),
                        new XElement("ProjectNumber", proj.Number),
                        new XElement("ProjectName", proj.Name),
                        new XElement("ProjectDescription", proj.Description),
                        new XElement("ProjectInvoiceNr", invoiceNr),
                        new XElement("ProjectCreated", proj.Created.HasValue ? proj.Created.Value.ToShortDateString() : ""),
                        new XElement("ProjectCreatedBy", proj.CreatedBy),
                        new XElement("ProjectState", proj.State),
                        new XElement("ProjectWorkSiteId", proj.WorkSiteKey),
                        new XElement("ProjectWorkSiteNumber", proj.WorkSiteNumber));


                    foreach (XElement employeeElement in employeeElements)
                    {
                        projectElement.Add(employeeElement);
                    }

                    if (timeProjectReportElement == null)
                        timeProjectReportElement = new XElement(projectElement);
                    else
                        timeProjectReportElement.Add(projectElement);
                    projectXmlId++;
                }

                //Detach
                foreach (var proj in projects)
                {
                    if (proj.Employees != null)
                    {
                        foreach (var emp in proj.Employees)
                        {
                            foreach (var trans in emp.Transactions)
                                base.TryDetachEntity(entities, trans);

                            base.TryDetachEntity(entities, emp);
                        }
                    }
                    base.TryDetachEntity(entities, proj);
                }

            }
            else
            {
                bool useProjectTimeBlock = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.UseProjectTimeBlocks, this.UserId, this.ActorCompanyId, 0, false);
                DateTime? fromDate = es.HasDateInterval && es.DateFrom != CalendarUtility.DATETIME_DEFAULT ? es.DateFrom : (DateTime?)null;
                DateTime? toDate = es.HasDateInterval && es.DateTo != CalendarUtility.DATETIME_DEFAULT ? es.DateTo : (DateTime?)null;

                List<Project> projects = ProjectManager.GetProjectsForInvoice(entities, invoiceId);

                if (projects.Count > 0)
                {
                    var invoice = (from i in entities.Invoice
                               .Include("Origin")
                                   where i.InvoiceId == invoiceId &&
                                   i.State == (int)SoeEntityState.Active
                                   select i).OfType<CustomerInvoice>().FirstOrDefault();

                    if (invoice == null)
                        return timeProjectReportElement;

                    string invoiceNr = invoice.InvoiceNr;

                    foreach (Project project in projects)
                    {
                        if (useProjectTimeBlock)
                        {
                            List<ProjectTimeBlock> projectTimeBlocks = new List<ProjectTimeBlock>();
                            if (invoice.Origin.Type == (int)SoeOriginType.CustomerInvoice)
                            {
                                List<int> connectedOrderDitinctIds = InvoiceManager.GetConnectedOrdersForCustomerInvoice(entities, invoice.InvoiceId);
                                var tempBlocks = ProjectManager.GetProjectTimeBlocksForProject(entities, project.ProjectId, null, fromDate, toDate);
                                foreach (var connectedOrderId in connectedOrderDitinctIds)
                                {
                                    if (projectTimeBlocks.Any(x => x.CustomerInvoiceId == connectedOrderId))//just to be sure
                                        continue;

                                    projectTimeBlocks.AddRange(tempBlocks.Where(x => x.CustomerInvoiceId == connectedOrderId).ToList());
                                }
                            }
                            else
                                projectTimeBlocks = ProjectManager.GetProjectTimeBlocksForProject(entities, project.ProjectId, invoiceId, fromDate, toDate);

                            nrOfTimeProjectRows += projectTimeBlocks.Count;

                            //Group the entire list by employeeId
                            List<IGrouping<int, ProjectTimeBlock>> projectTimeBlocksGroupedByEmployeeId = projectTimeBlocks.GroupBy(g => g.EmployeeId).ToList();

                            int employeeXmlId = 1;
                            var employeeElements = new List<XElement>();

                            foreach (IGrouping<int, ProjectTimeBlock> projectTimeBlockGroupedByEmployeeId in projectTimeBlocksGroupedByEmployeeId)
                            {
                                ProjectTimeBlock firstProjectTimeBlock = projectTimeBlockGroupedByEmployeeId.FirstOrDefault();
                                if (firstProjectTimeBlock == null)
                                    continue;

                                Employee employee = firstProjectTimeBlock.Employee;
                                if (employee == null)
                                    continue;

                                List<XElement> projectInvoiceDayElements = new List<XElement>();
                                int projectInvoiceDayXmlId = 1;

                                foreach (ProjectTimeBlock projectTimeBlock in projectTimeBlockGroupedByEmployeeId)
                                {
                                    if (projectTimeBlock.InvoiceQuantity == 0)
                                        continue;

                                    var dayElement = CreateProjectInvoiceDayElement(projectInvoiceDayXmlId, projectTimeBlock, timeCodes, es.ActorCompanyId, showStartStopInTimeReport);

                                    projectInvoiceDayElements.Add(dayElement);

                                    projectInvoiceDayXmlId++;
                                }

                                #region Employee

                                XElement employeeElement = new XElement("Employee",
                                    new XAttribute("id", employeeXmlId),
                                    new XElement("EmployeeNr", employee.EmployeeNr),
                                    new XElement("EmployeeName", employee.Name));

                                foreach (XElement projectInvoiceDayElement in projectInvoiceDayElements)
                                {
                                    employeeElement.Add(projectInvoiceDayElement);
                                }

                                projectInvoiceDayElements.Clear();
                                employeeElements.Add(employeeElement);

                                #endregion

                                employeeXmlId++;
                            }

                            #region Default element Employee

                            if (employeeXmlId == 1)
                            {
                                XElement defaultEmployeeElement = new XElement("Employee",
                                    new XAttribute("id", 1),
                                    new XElement("EmployeeNr", 0),
                                    new XElement("EmployeeName", ""));

                                XElement defaultDayElement = new XElement("ProjectInvoiceDay",
                                    new XAttribute("id", 1),
                                    new XElement("InvoiceTimeInMinutes", 0),
                                    new XElement("Date", CalendarUtility.DATETIME_DEFAULT),
                                    new XElement("Note", "00:00"),
                                    new XElement("ExternalNote", string.Empty),
                                    new XElement("IsoDate", DateTime.Now.Date.ToString("yyyy-MM-dd")));

                                defaultEmployeeElement.Add(defaultDayElement);
                                employeeElements.Add(defaultEmployeeElement);
                            }

                            #endregion

                            #region Project

                            projectElement = new XElement("Project",
                                new XAttribute("id", projectXmlId),
                                new XElement("ProjectNumber", project.Number),
                                new XElement("ProjectName", project.Name),
                                new XElement("ProjectDescription", project.Description),
                                new XElement("ProjectInvoiceNr", invoiceNr),
                                new XElement("ProjectCreated", project.Created.HasValue ? project.Created.Value.ToShortDateString() : ""),
                                new XElement("ProjectCreatedBy", project.CreatedBy),
                                new XElement("ProjectState", project.State),
                                new XElement("ProjectWorkSiteId", project.WorkSiteKey),
                                new XElement("ProjectWorkSiteNumber", project.WorkSiteNumber));

                            foreach (XElement employeeElement in employeeElements)
                            {
                                projectElement.Add(employeeElement);
                            }

                            employeeElements.Clear();

                            if (timeProjectReportElement == null)
                                timeProjectReportElement = new XElement(projectElement);
                            else
                                timeProjectReportElement.Add(projectElement);

                            #endregion

                            projectXmlId++;

                        }
                        else
                        {
                            List<ProjectInvoiceWeek> projectInvoiceWeeks = new List<ProjectInvoiceWeek>();

                            if (invoice.Origin.Type == (int)SoeOriginType.CustomerInvoice)
                            {
                                List<int> connectedOrderDitinctIds = InvoiceManager.GetConnectedOrdersForCustomerInvoice(entities, invoice.InvoiceId);
                                var tempWeeks = ProjectManager.GetProjectInvoiceWeeks(entities, project.ProjectId);
                                foreach (var connectedOrderId in connectedOrderDitinctIds)
                                {
                                    if (projectInvoiceWeeks.Any(x => x.RecordId == connectedOrderId))//just to be sure
                                        continue;

                                    projectInvoiceWeeks.AddRange(tempWeeks.Where(x => x.RecordId == connectedOrderId).ToList());
                                }
                            }
                            else
                                //projectInvoiceWeeks = ProjectManager.GetProjectInvoiceWeeks(entities, project.ProjectId, invoiceId);
                                projectInvoiceWeeks = ProjectManager.GetProjectInvoiceWeeks(entities, project.ProjectId, invoiceId, fromDate, toDate);

                            //Group the entire list by employeeId
                            List<IGrouping<int, ProjectInvoiceWeek>> projectInvoiceWeeksGroupedByEmployeeId = projectInvoiceWeeks.GroupBy(g => g.EmployeeId).ToList();

                            int employeeXmlId = 1;
                            List<XElement> employeeElements = new List<XElement>();

                            //Each employeeProjectInvoiceWeekItems contains all ProjectInvoiceWeeks for one employee
                            foreach (IGrouping<int, ProjectInvoiceWeek> projectInvoiceWeekGroupedByEmployeeId in projectInvoiceWeeksGroupedByEmployeeId)
                            {
                                ProjectInvoiceWeek firstProjectInvoiceWeek = projectInvoiceWeekGroupedByEmployeeId.FirstOrDefault();
                                if (firstProjectInvoiceWeek == null)
                                    continue;

                                Employee employee = firstProjectInvoiceWeek.Employee;
                                if (employee == null)
                                    continue;

                                List<XElement> projectInvoiceDayElements = new List<XElement>();
                                int projectInvoiceDayXmlId = 1;

                                //foreach ProjectInvoiceWeek for the employee
                                foreach (ProjectInvoiceWeek projectInvoiceWeek in projectInvoiceWeekGroupedByEmployeeId)
                                {
                                    //projectInvoiceDays contains all ProjectInvoiceDay items in a ProjectInvoiceWeek for the employee
                                    var projectInvoiceDays = ProjectManager.GetProjectInvoiceDays(entities, projectInvoiceWeek.ProjectInvoiceWeekId, fromDate, toDate, true);
                                    TimeCode timeCode = null;
                                    if (projectInvoiceWeek.TimeCodeId.HasValue)
                                        timeCode = timeCodes.FirstOrDefault(x => x.TimeCodeId == projectInvoiceWeek.TimeCodeId.Value);

                                    nrOfTimeProjectRows += projectInvoiceDays.Count(p => p.InvoiceTimeInMinutes > 0);

                                    foreach (ProjectInvoiceDay projectInvoiceDay in projectInvoiceDays)
                                    {
                                        #region ProjectInvoiceDay

                                        var timeCodeTransaction = projectInvoiceDay.TimeCodeTransaction.FirstOrDefault(t => t.State == (int)SoeEntityState.Active && t.TimeInvoiceTransaction.Any(i => i.State == (int)SoeEntityState.Active));
                                        var invoiceTimeInMinutes = timeCodeTransaction != null && timeCodeTransaction.TimeInvoiceTransaction.Any() ? projectInvoiceDay.InvoiceTimeInMinutes : 0;

                                        if (invoiceTimeInMinutes == 0)
                                            continue;

                                        XElement dayElement = new XElement("ProjectInvoiceDay",
                                            new XAttribute("id", projectInvoiceDayXmlId),
                                            new XElement("TCCode", timeCode != null ? timeCode.Code : string.Empty),
                                            new XElement("TCName", timeCode != null ? timeCode.Name : string.Empty),
                                            new XElement("InvoiceTimeInMinutes", invoiceTimeInMinutes),
                                            new XElement("Date", projectInvoiceDay.Date.ToShortDateString()),
                                            new XElement("Note", projectInvoiceDay.Note),
                                            new XElement("ExternalNote", string.Empty),
                                            new XElement("IsoDate", DateTime.Now.Date.ToString("yyyy-MM-dd")));
                                        projectInvoiceDayElements.Add(dayElement);

                                        #endregion

                                        projectInvoiceDayXmlId++;
                                    }
                                }

                                #region Employee

                                XElement employeeElement = new XElement("Employee",
                                    new XAttribute("id", employeeXmlId),
                                    new XElement("EmployeeNr", employee.EmployeeNr),
                                    new XElement("EmployeeName", employee.Name));

                                foreach (XElement projectInvoiceDayElement in projectInvoiceDayElements)
                                {
                                    employeeElement.Add(projectInvoiceDayElement);
                                }

                                projectInvoiceDayElements.Clear();
                                employeeElements.Add(employeeElement);

                                #endregion

                                employeeXmlId++;
                            }

                            #region Default element Employee

                            if (employeeXmlId == 1)
                            {
                                XElement defaultEmployeeElement = new XElement("Employee",
                                    new XAttribute("id", 1),
                                    new XElement("EmployeeNr", 0),
                                    new XElement("EmployeeName", ""));

                                XElement defaultDayElement = new XElement("ProjectInvoiceDay",
                                    new XAttribute("id", 1),
                                    new XElement("InvoiceTimeInMinutes", 0),
                                    new XElement("Date", CalendarUtility.DATETIME_DEFAULT),
                                    new XElement("Note", "00:00"),
                                    new XElement("ExternalNote", string.Empty),
                                    new XElement("IsoDate", DateTime.Now.Date.ToString("yyyy-MM-dd")));

                                defaultEmployeeElement.Add(defaultDayElement);
                                employeeElements.Add(defaultEmployeeElement);
                            }

                            #endregion

                            #region Project

                            projectElement = new XElement("Project",
                                new XAttribute("id", projectXmlId),
                                new XElement("ProjectNumber", project.Number),
                                new XElement("ProjectName", project.Name),
                                new XElement("ProjectDescription", project.Description),
                                new XElement("ProjectInvoiceNr", invoiceNr),
                                new XElement("ProjectCreated", project.Created.HasValue ? project.Created.Value.ToShortDateString() : ""),
                                new XElement("ProjectCreatedBy", project.CreatedBy),
                                new XElement("ProjectState", project.State),
                                new XElement("ProjectWorkSiteId", project.WorkSiteKey),
                                new XElement("ProjectWorkSiteNumber", project.WorkSiteNumber));

                            foreach (XElement employeeElement in employeeElements)
                            {
                                projectElement.Add(employeeElement);
                            }

                            employeeElements.Clear();

                            if (timeProjectReportElement == null)
                                timeProjectReportElement = new XElement(projectElement);
                            else
                                timeProjectReportElement.Add(projectElement);

                            #endregion

                            projectXmlId++;
                        }
                    }
                }
                else
                {
                    #region Default element Project

                    projectElement = new XElement("Project",
                        new XAttribute("id", 1),
                        new XElement("ProjectNumber", ""),
                        new XElement("ProjectName", ""),
                        new XElement("ProjectDescription", ""),
                        new XElement("ProjectInvoiceNr", ""),
                        new XElement("ProjectCreated", "00:00"),
                        new XElement("ProjectCreatedBy", ""),
                        new XElement("ProjectState", 0),
                        new XElement("ProjectWorkSiteId", ""),
                        new XElement("ProjectWorkSiteNumber", ""));

                    XElement defaultEmployeeElement = new XElement("Employee",
                        new XAttribute("id", 1),
                        new XElement("EmployeeNr", 0),
                        new XElement("EmployeeName", ""));

                    XElement defaultDayElement = new XElement("ProjectInvoiceDay",
                        new XAttribute("id", 1),
                        new XElement("InvoiceTimeInMinutes", 0),
                        new XElement("Date", CalendarUtility.DATETIME_DEFAULT),
                        new XElement("Note", "00:00"));

                    defaultEmployeeElement.Add(defaultDayElement);
                    projectElement.Add(defaultEmployeeElement);
                    timeProjectReportElement.Add(projectElement);

                    #endregion
                }
            }

            #endregion

            return timeProjectReportElement;
        }

        protected List<XElement> CreateInvoiceSignatureElements(int invoiceId)
        {

            List<XElement> signaturesElements = new List<XElement>();

            var signatureImageIds = GeneralManager.GetDataStorageRecordsForCustomerInvoice(Company.ActorCompanyId, base.RoleId, invoiceId, SoeEntityType.None, new List<SoeDataStorageRecordType> { SoeDataStorageRecordType.OrderInvoiceSignature }, false, null, true);

            #region signaturesElements

            int imageXmlId = 1;
            if (signatureImageIds.Any())
            {
                foreach (var image in signatureImageIds)
                {
                    string imagePath = GraphicsManager.GetImageFilePath(image, Company.ActorCompanyId);
                    if (this.useCrGen)
                        imagePath = AddToCrGenRequestPicturesDTO(imagePath);
                    string type = "1";

                    XElement invoiceSignatureImage = new XElement("InvoiceSignatureImage",
                        new XAttribute("id", imageXmlId),
                        new XElement("ImagePath", imagePath),
                        new XElement("Type", type),
                        new XElement("Description", image.Description));

                    signaturesElements.Add(invoiceSignatureImage);
                    imageXmlId++;
                }
            }

            if (!signatureImageIds.Any() && imageXmlId == 1)
            {
                string description = "";
                string imagePath = "";
                string type = "0";

                XElement invoiceSignatureImage = new XElement("InvoiceSignatureImage",
                    new XAttribute("id", imageXmlId),
                    new XElement("ImagePath", imagePath),
                    new XElement("Type", type),
                    new XElement("Description", description));

                signaturesElements.Add(invoiceSignatureImage);
            }

            #endregion

            return signaturesElements;

        }

        #endregion

        #region Time

        protected XElement CreateTimeAccumulatorEmployeeElement(PersonalDataEmployeeReportRepository personalDataRepository, int id = 1, Employee employee = null, EmployeeGroup employeeGroup = null, List<CompanyCategoryRecord> employeeCategoryRecords = null)
        {
            XElement element = null;
            if (employee != null && employeeGroup != null)
            {
                //Category
                string employeeCategoryName = "";
                string employeeCategoryCode = "";
                if (!employeeCategoryRecords.IsNullOrEmpty())
                {
                    //Only show the default category
                    CompanyCategoryRecord companyCategoryRecord = employeeCategoryRecords.OrderByDescending(i => i.Default).First();
                    employeeCategoryName = companyCategoryRecord.Category.Name;
                    employeeCategoryCode = companyCategoryRecord.Category.Code;
                }

                element = new XElement("Employee",
                    new XAttribute("id", id),
                    new XElement("EmployeeNr", employee.EmployeeNr),
                    new XElement("EmployeeName", employee.Name),
                    new XElement("EmployeeGroupName", employeeGroup.Name),
                    new XElement("EmployeeCategoryCode", employeeCategoryCode),
                    new XElement("EmployeeCategoryName", employeeCategoryName));
                personalDataRepository.AddEmployee(employee, element);
            }
            else
            {
                //Default element
                element = new XElement("Employee",
                    new XAttribute("id", id),
                    new XElement("EmployeeNr", String.Empty),
                    new XElement("EmployeeName", String.Empty),
                    new XElement("EmployeeGroupName", String.Empty),
                    new XElement("EmployeeCategoryCode", String.Empty),
                    new XElement("EmployeeCategoryName", String.Empty));

                element.Add(CreateTimeAccumulatorElement());
            }

            return element;
        }

        protected XElement CreateTimeAccumulatorElement(int id = 1, TimeAccumulator timeAccumulator = null, decimal balanceQuantity = 0, decimal sumAccToday = 0, decimal sumAccTodayValue = 0, bool showTimeAccTodayValue = false)
        {
            XElement element = new XElement("TimeAccumulator",
                new XAttribute("id", id),
                new XElement("TimeAccumulatorName", timeAccumulator?.Name ?? string.Empty),
                new XElement("TimeAccumulatorDescription", timeAccumulator?.Description ?? string.Empty),
                new XElement("TimeAccumulatorFinalSalary", showTimeAccTodayValue && timeAccumulator != null ? timeAccumulator.FinalSalary.ToInt() : 0),
                new XElement("TimeAccumulatorBalanceQuantity", balanceQuantity),
                new XElement("TimeAccumulatorSumAccToday", sumAccToday),
                new XElement("TimeAccumulatorSumAccTodayValue", showTimeAccTodayValue ? sumAccTodayValue : 0));

            if (timeAccumulator == null)
            {
                element.Add(CreateTimeAccumulatorTransactionElement());
                element.Add(CreateTimeAccumulatorTransactionElement(incomingTransaction: true));
                element.Add(CreateTimeAccumulatorEmployeeGroupRuleElement());
            }

            return element;
        }

        protected XElement CreateTimeAccumulatorEmployeeGroupRuleElement(int id = 1, TimeAccumulatorEmployeeGroupRule employeeGroupRule = null)
        {
            return new XElement("TimeAccumulatorRule",
                new XAttribute("id", id),
                new XElement("TimeAccumulatorRuleMin", employeeGroupRule?.MinMinutes ?? 0),
                new XElement("TimeAccumulatorRuleMax", employeeGroupRule?.MaxMinutes ?? 0),
                new XElement("TimeAccumulatorRuleType", "EmployeeGroupRule"));
        }

        protected XElement CreateTimeAccumulatorTransactionElement(int id = 1, object transaction = null, bool incomingTransaction = false)
        {
            XElement element = null;

            if (transaction != null)
            {
                if (transaction is GetTimeCodeTransactionsForAcc_Result timeCodeTransaction)
                {
                    element = new XElement("TimeTransaction",
                        new XAttribute("id", id),
                        new XElement("TransactionType", "TimeCodeTransaction"),
                        new XElement("Code", timeCodeTransaction.TimeCode?.Code ?? string.Empty),
                        new XElement("Namn", timeCodeTransaction.TimeCode?.Name ?? string.Empty),
                        new XElement("Quantity", timeCodeTransaction.Quantity),
                        new XElement("Factor", timeCodeTransaction.CalculateFactor()),
                        new XElement("Date", timeCodeTransaction.Date.ToShortDateString()),
                        new XElement("IncomingTransaction", incomingTransaction.ToInt()),
                        new XElement("IsPreliminary", 0));
                }
                else if (transaction is GetTimePayrollTransactionsForAcc_Result timePayrollTransaction)
                {
                    element = new XElement("TimeTransaction",
                        new XAttribute("id", id),
                        new XElement("TransactionType", "TimePayrollTransaction"),
                        new XElement("Code", timePayrollTransaction.Product?.Number ?? string.Empty),
                        new XElement("Namn", timePayrollTransaction.Product?.Name ?? string.Empty),
                        new XElement("Quantity", timePayrollTransaction.Quantity),
                        new XElement("Factor", timePayrollTransaction.Factor),
                        new XElement("Date", timePayrollTransaction.Date.ToShortDateString()),
                        new XElement("IncomingTransaction", incomingTransaction.ToInt()),
                        new XElement("IsPreliminary", timePayrollTransaction.IsPreliminary.ToInt()));
                }
                else if (transaction is GetTimeInvoiceTransactionsForAcc_Result timeInvoiceTransaction)
                {
                    element = new XElement("TimeTransaction",
                        new XAttribute("id", id),
                        new XElement("TransactionType", "TimeInvoiceTransaction"),
                        new XElement("Code", timeInvoiceTransaction.Product?.Number ?? string.Empty),
                        new XElement("Namn", timeInvoiceTransaction.Product?.Name ?? string.Empty),
                        new XElement("Quantity", timeInvoiceTransaction.Quantity),
                        new XElement("Factor", timeInvoiceTransaction.Factor),
                        new XElement("Date", timeInvoiceTransaction.Date.ToShortDateString()),
                        new XElement("IncomingTransaction", incomingTransaction.ToInt()),
                        new XElement("IsPreliminary", 0));
                }
            }
            else
            {
                //Default element
                element = new XElement("TimeTransaction",
                    new XAttribute("id", 1),
                    new XElement("TransactionType", ""),
                    new XElement("Code", 0),
                    new XElement("Namn", ""),
                    new XElement("Quantity", 0),
                    new XElement("Factor", 0),
                    new XElement("Date", "00:00"),
                    new XElement("IncomingTransaction", incomingTransaction.ToInt()),
                    new XElement("IsPreliminary", 0));
            }

            return element;
        }

        protected XElement CreateTimeProjectEmployeeElement(PersonalDataEmployeeReportRepository personalDataRepository, bool showSocialSec, IEnumerable<Employee> employees = null, IEnumerable<ProjectUser> projectusers = null, Project project = null)
        {
            #region Project element

            XElement projectElement = null;

            if (project == null)
            {
                projectElement = new XElement("Project",
                    new XElement("CustomerName", string.Empty),
                    new XElement("ProjectNumber", string.Empty),
                    new XElement("ProjectName", string.Empty),
                    new XElement("ProjectDescription", string.Empty),
                    new XElement("WorkSiteKey", string.Empty),
                    new XElement("WorkSiteNumber", string.Empty),
                    new XElement("StartDate", CalendarUtility.DATETIME_DEFAULT),
                    new XElement("EndDate", CalendarUtility.DATETIME_DEFAULT)
                    );
            }
            else
            {
                projectElement = new XElement("Project",
                    new XElement("CustomerName", project.Customer?.Name ?? string.Empty),
                    new XElement("ProjectNumber", project.Number),
                    new XElement("ProjectName", project.Name),
                    new XElement("ProjectDescription", project.Description),
                    new XElement("WorkSiteKey", project.WorkSiteKey),
                    new XElement("WorkSiteNumber", project.WorkSiteNumber),
                    new XElement("StartDate", project.StartDate != null ? project.StartDate : CalendarUtility.DATETIME_DEFAULT),
                    new XElement("EndDate", project.StopDate != null ? project.StopDate : CalendarUtility.DATETIME_DEFAULT)
                    );
            }
            #endregion

            #region Employee Element

            XElement employeeElement = null;

            if (!employees.IsNullOrEmpty())
            {
                foreach (Employee employee in employees)
                {
                    string employeeVATNumber = "", employeePhoneNumber = "", employeeEmailAddress = "";
                    string visitingAddressAddress = "", visitingAddressPostalCode = "", visitingAddressPostalAddress = "", visitingAddressCountry = "";
                    string distributionAddressAddress = "", distributionAddressPostalCode = "", distributionAddressPostalAddress = "";

                    Contact contact = EmployeeManager.GetEmployeeContact(employee.EmployeeId);
                    if (contact != null)
                    {
                        List<ContactECom> contactEcoms = ContactManager.GetContactEComs(contact.ContactId);
                        employeeVATNumber = contactEcoms.GetEComText(TermGroup_SysContactEComType.IndividualTaxNumber);
                        employeePhoneNumber = contactEcoms.GetEComText(TermGroup_SysContactEComType.PhoneHome);
                        if (employeePhoneNumber == "")
                            employeePhoneNumber = contactEcoms.GetEComText(TermGroup_SysContactEComType.PhoneMobile);
                        employeeEmailAddress = contactEcoms.GetEComText(TermGroup_SysContactEComType.Email);

                        List<ContactAddressRow> contactAddressRows = ContactManager.GetContactAddressRows(contact.ContactId);
                        visitingAddressAddress = contactAddressRows.GetContactAddressRowText(TermGroup_SysContactAddressType.Delivery, TermGroup_SysContactAddressRowType.Address);
                        visitingAddressPostalCode = contactAddressRows.GetContactAddressRowText(TermGroup_SysContactAddressType.Delivery, TermGroup_SysContactAddressRowType.PostalCode);
                        visitingAddressPostalAddress = contactAddressRows.GetContactAddressRowText(TermGroup_SysContactAddressType.Delivery, TermGroup_SysContactAddressRowType.PostalAddress);
                        visitingAddressCountry = contactAddressRows.GetContactAddressRowText(TermGroup_SysContactAddressType.Delivery, TermGroup_SysContactAddressRowType.Country);
                        distributionAddressAddress = contactAddressRows.GetContactAddressRowText(TermGroup_SysContactAddressType.Distribution, TermGroup_SysContactAddressRowType.Address);
                        distributionAddressPostalCode = contactAddressRows.GetContactAddressRowText(TermGroup_SysContactAddressType.Distribution, TermGroup_SysContactAddressRowType.PostalCode);
                        distributionAddressPostalAddress = contactAddressRows.GetContactAddressRowText(TermGroup_SysContactAddressType.Distribution, TermGroup_SysContactAddressRowType.PostalAddress);
                    }

                    employeeElement = new XElement("Employee",
                        new XElement("EmployeeLastName", employee.LastName),
                        new XElement("EmployeeFirstName", employee.FirstName),
                        new XElement("EmployeeDateOfBirth", showSocialSec ? employee.SocialSec : StringUtility.SocialSecYYMMDD_Dash_Stars(employee.SocialSec)),
                        new XElement("EmployeeVATNumber", employeeVATNumber),
                        new XElement("EmployeePhoneNumber", employeePhoneNumber),
                        new XElement("EmployeeEmailAddress", employeeEmailAddress),
                        new XElement("VisitingAddress", visitingAddressAddress),
                        new XElement("VisitingAddressPostalCode", visitingAddressPostalCode),
                        new XElement("VisitingAddressPostalAddress", visitingAddressPostalAddress),
                        new XElement("VisitingAddressCountry", visitingAddressCountry),
                        new XElement("DistributionAddress", distributionAddressAddress),
                        new XElement("DistributionAddressPostalCode", distributionAddressPostalCode),
                        new XElement("DistributionAddressPostalAddress", distributionAddressPostalAddress)
                        );
                    personalDataRepository.AddEmployee(employee, employeeElement);
                    projectElement.Add(employeeElement);
                }

            }
            else if (projectusers != null)
            {
                foreach (ProjectUser projectUser in projectusers)
                {
                    string userLastName = "", userFirstName = "";
                    string projectuserVATNumber = "", projectuserPhoneNumber = "", projectuserEmailAddress = "";
                    string visitingAddressAddress = "", visitingAddressPostalCode = "", visitingAddressPostalAddress = "", visitingAddressCountry = "";
                    string distributionAddressAddress = "", distributionAddressPostalCode = "", distributionAddressPostalAddress = "";

                    User user = UserManager.GetUser(projectUser.UserId);
                    if (user != null && !user.ContactPersonReference.IsLoaded)
                        user.ContactPersonReference.Load();

                    int contactId = user?.ContactPerson?.ActorContactPersonId ?? 0;
                    userFirstName = user?.ContactPerson?.FirstName ?? string.Empty;
                    userLastName = user?.ContactPerson?.LastName ?? string.Empty;

                    if (contactId != 0)
                    {
                        List<ContactAddressItem> contactAddreses = ContactManager.GetContactAddressItems(contactId);
                        projectuserVATNumber = contactAddreses.Where(a => a.ContactAddressItemType == ContactAddressItemType.IndividualTaxNumber).Select(a => a.EComText).FirstOrDefault();
                        projectuserPhoneNumber = contactAddreses.Where(a => a.ContactAddressItemType == ContactAddressItemType.EComPhoneHome).Select(a => a.EComText).FirstOrDefault();
                        if (projectuserPhoneNumber == "")
                            projectuserPhoneNumber = contactAddreses.Where(a => a.ContactAddressItemType == ContactAddressItemType.EComPhoneMobile).Select(a => a.EComText).FirstOrDefault();
                        projectuserEmailAddress = contactAddreses.Where(a => a.ContactAddressItemType == ContactAddressItemType.EComEmail).Select(a => a.EComText).FirstOrDefault();
                        visitingAddressAddress = contactAddreses.Where(a => a.ContactAddressItemType == ContactAddressItemType.AddressDelivery).Select(a => a.Address).FirstOrDefault();
                        visitingAddressPostalCode = contactAddreses.Where(a => a.ContactAddressItemType == ContactAddressItemType.AddressDelivery).Select(a => a.PostalCode).FirstOrDefault();
                        visitingAddressPostalAddress = contactAddreses.Where(a => a.ContactAddressItemType == ContactAddressItemType.AddressDelivery).Select(a => a.PostalAddress).FirstOrDefault();
                        visitingAddressCountry = contactAddreses.Where(a => a.ContactAddressItemType == ContactAddressItemType.AddressDelivery).Select(a => a.Country).FirstOrDefault();
                        distributionAddressAddress = contactAddreses.Where(a => a.ContactAddressItemType == ContactAddressItemType.AddressDistribution).Select(a => a.Address).FirstOrDefault();
                        distributionAddressPostalCode = contactAddreses.Where(a => a.ContactAddressItemType == ContactAddressItemType.AddressDistribution).Select(a => a.PostalCode).FirstOrDefault();
                        distributionAddressPostalAddress = contactAddreses.Where(a => a.ContactAddressItemType == ContactAddressItemType.AddressDistribution).Select(a => a.PostalAddress).FirstOrDefault();
                    }

                    employeeElement = new XElement("Employee",
                        new XElement("EmployeeLastName", userLastName),
                        new XElement("EmployeeFirstName", userFirstName),
                        new XElement("EmployeeDateOfBirth", showSocialSec ? user.SocialSec : StringUtility.SocialSecYYMMDD_Dash_Stars(user.SocialSec)),
                        new XElement("EmployeeVATNumber", projectuserVATNumber),
                        new XElement("EmployeePhoneNumber", projectuserPhoneNumber),
                        new XElement("EmployeeEmailAddress", projectuserEmailAddress),
                        new XElement("VisitingAddress", visitingAddressAddress),
                        new XElement("VisitingAddressPostalCode", visitingAddressPostalCode),
                        new XElement("VisitingAddressPostalAddress", visitingAddressPostalAddress),
                        new XElement("VisitingAddressCountry", visitingAddressCountry),
                        new XElement("DistributionAddress", distributionAddressAddress),
                        new XElement("DistributionAddressPostalCode", distributionAddressPostalCode),
                        new XElement("DistributionAddressPostalAddress", distributionAddressPostalAddress)
                        );
                    personalDataRepository.AddUser(user, employeeElement);
                    projectElement.Add(employeeElement);
                }
            }
            else
            {
                employeeElement = new XElement("Employee",
                    new XElement("EmployeeLastName", ""),
                    new XElement("EmployeeFirstName", ""),
                    new XElement("EmployeeDateOfBirth", ""),
                    new XElement("EmployeeVATNumber", ""),
                    new XElement("EmployeePhoneNumber", ""),
                    new XElement("EmployeeEmailAddress", ""),
                    new XElement("VisitingAddress", ""),
                    new XElement("VisitingAddressPostalCode", ""),
                    new XElement("VisitingAddressPostalAddress", ""),
                    new XElement("VisitingAddressCountry", ""),
                    new XElement("DistributionAddress", ""),
                    new XElement("DistributionAddressPostalCode", ""),
                    new XElement("DistributionAddressPostalAddress", "")
                    );
                projectElement.Add(employeeElement);
            }

            #endregion

            return projectElement;
        }

        protected XElement CreateTimePayrollProductElement(PayrollProductDTO product, List<Account> accounts, List<AccountDim> accountDims)
        {
            XElement payrollProductElement = null;
            XElement payrollGroupElement = null;
            XElement payrollPriceTypes = null;
            XElement payrollPriceFormulas = null;
            XElement payrollAccountingSetting = null;
            XElement payrollCompanyCategory = null;
            XElement payrollPurchaseAccount = null;

            List<XElement> payrollGroupElements = new List<XElement>();

            // Groups 
            if (product.Settings == null)
            {
                product.Settings = new List<PayrollProductSettingDTO>()
                {
                    new PayrollProductSettingDTO()
                    {
                        PayrollProductSettingId = -1
                    }
                };
            }

            List<int> childIds = product.Settings.Where(x => x.ChildProductId.HasValue).Select(x => x.ChildProductId.Value).ToList();
            List<PayrollProduct> childProducts = ProductManager.GetPayrollProducts(childIds);

            foreach (var i in product.Settings)
            {
                List<Account> usedAccounts = new List<Account>();

                if (i.PayrollProductSettingId != -1)
                {
                    var childProduct = i.ChildProductId.HasValue ? childProducts.FirstOrDefault(x => x.ProductId == i.ChildProductId.Value) : null;

                    payrollGroupElement = new XElement("PayrollProductSetting",
                         new XElement("PayrollGroupId", i.PayrollGroupId != null ? i.PayrollGroupId.ToString() : String.Empty),
                         new XElement("Name", i.PayrollGroupName != null ? i.PayrollGroupName.ToString() : String.Empty),
                         new XElement("CentRoundingType", ((TermGroup_PayrollProductCentRoundingType)i.CentRoundingType).ToString()),
                         new XElement("CentRoundingLevel", ((TermGroup_PayrollProductCentRoundingLevel)i.CentRoundingLevel).ToString()),
                         new XElement("TaxCalculationType", ((TermGroup_PayrollProductTaxCalculationType)i.TaxCalculationType).ToString()),
                         new XElement("PrintOnSalarySpecification", i.PrintOnSalarySpecification.ToString()),
                         new XElement("PrintDate", i.PrintDate.ToString()),
                         new XElement("PensionCompany", ((TermGroup_PensionCompany)i.PensionCompany).ToString()),
                         new XElement("VacationSalaryPromoted", i.VacationSalaryPromoted.ToString()),
                         new XElement("UnionFeePromoted", i.UnionFeePromoted.ToString()),
                         new XElement("WorkingTimePromoted", i.WorkingTimePromoted.ToString()),
                         new XElement("CalculateSupplementCharge", i.CalculateSupplementCharge.ToString()),
                         new XElement("TimeUnit", ((TermGroup_PayrollProductTimeUnit)i.TimeUnit).ToString()),
                         new XElement("QuantityRoundingType", ((TermGroup_PayrollProductQuantityRoundingType)i.QuantityRoundingType).ToString()),
                         new XElement("ChildProductId", childProduct != null ? childProduct.Number : ""));

                    usedAccounts = accounts.Where(p => i.PurchaseAccounts.Values.Select(v => v.AccountId).Contains(p.AccountId)).ToList();
                }
                else
                {
                    payrollGroupElement = new XElement("PayrollProductSetting",
                         new XElement("PayrollGroupId", string.Empty),
                         new XElement("Name", string.Empty),
                         new XElement("CentRoundingType", string.Empty),
                         new XElement("CentRoundingLevel", string.Empty),
                         new XElement("TaxCalculationType", string.Empty),
                         new XElement("PrintOnSalarySpecification", string.Empty),
                         new XElement("PrintDate", string.Empty),
                         new XElement("PensionCompany", string.Empty),
                         new XElement("VacationSalaryPromoted", string.Empty),
                         new XElement("UnionFeePromoted", string.Empty),
                         new XElement("WorkingTimePromoted", string.Empty),
                         new XElement("CalculateSupplementCharge", string.Empty),
                         new XElement("TimeUnit", string.Empty),
                         new XElement("QuantityRoundingType", string.Empty),
                         new XElement("ChildProductId", string.Empty));
                }

                int numberOfdims = 6;
                int dimCounter = 1;

                while (dimCounter <= numberOfdims)
                {
                    if (dimCounter == 1)
                    {
                        foreach (var dim in accountDims.OrderBy(d => d.AccountDimNr))
                        {
                            if (dimCounter <= 6)
                            {
                                var account = usedAccounts.FirstOrDefault(a => a.AccountDim.AccountDimId == dim.AccountDimId);
                                if (account != null)
                                {
                                    payrollGroupElement.Add(new XElement("AccountDimNr" + dimCounter + "Nr", account.AccountNr));
                                    payrollGroupElement.Add(new XElement("AccountDimNr" + dimCounter + "Name", account.Name));
                                    payrollGroupElement.Add(new XElement("AccountDimNr" + dimCounter + "Prio", GetText(AccountManager.GetProductAccountingPrio(i.AccountingPrio.Split(','), dimCounter), (int)TermGroup.PayrollProductAccountingPrio)));
                                }
                                else
                                {
                                    payrollGroupElement.Add(new XElement("AccountDimNr" + dimCounter + "Nr", string.Empty));
                                    payrollGroupElement.Add(new XElement("AccountDimNr" + dimCounter + "Name", string.Empty));
                                    payrollGroupElement.Add(new XElement("AccountDimNr" + dimCounter + "Prio", string.Empty));

                                }
                            }

                            dimCounter++;
                        }
                        continue;
                    }

                    payrollGroupElement.Add(new XElement("AccountDimNr" + dimCounter + "Nr", string.Empty));
                    payrollGroupElement.Add(new XElement("AccountDimNr" + dimCounter + "Name", string.Empty));
                    payrollGroupElement.Add(new XElement("AccountDimNr" + dimCounter + "Prio", string.Empty));

                    dimCounter++;
                }

                if (!i.PriceTypes.IsNullOrEmpty())
                {
                    foreach (var priceTypes in i.PriceTypes)
                    {
                        payrollPriceTypes = new XElement("PriceTypes",
                            new XElement("PayrollPriceTypeId", priceTypes.PayrollPriceTypeId.ToString()),
                            new XElement("PayrollProductPriceTypeId", priceTypes.PayrollProductPriceTypeId.ToString()),
                            new XElement("PayrollProductSettingId", priceTypes.PayrollProductSettingId.ToString()),
                            new XElement("PriceTypeName", priceTypes.PriceTypeName?.ToString() ?? string.Empty));
                        payrollGroupElement.Add(payrollPriceTypes);
                    }
                }
                else
                {
                    payrollPriceTypes = new XElement("PriceTypes",
                        new XElement("PayrollPriceTypeId", string.Empty),
                        new XElement("PayrollProductPriceTypeId", string.Empty),
                        new XElement("PayrollProductSettingId", string.Empty),
                        new XElement("PriceTypeName", string.Empty));
                    payrollGroupElement.Add(payrollPriceTypes);
                }

                if (!i.PriceFormulas.IsNullOrEmpty())
                {
                    foreach (var priceFormula in i.PriceFormulas)
                    {
                        payrollPriceFormulas = new XElement("PriceFormulas",
                            new XElement("FormulaName", priceFormula.FormulaName?.ToString() ?? string.Empty),
                            new XElement("FromDate", priceFormula.FromDate != null ? priceFormula.FromDate.ToString() : string.Empty),
                            new XElement("PayrollPriceFormulaId", priceFormula.PayrollPriceFormulaId.ToString()),
                            new XElement("PayrollProductPriceFormulaId", priceFormula.PayrollProductPriceFormulaId.ToString()),
                            new XElement("PayrollProductSettingId", priceFormula.PayrollProductSettingId.ToString()));
                        payrollGroupElement.Add(payrollPriceFormulas);
                    }
                }
                else
                {
                    payrollPriceFormulas = new XElement("PriceFormulas",
                        new XElement("FormulaName", string.Empty),
                        new XElement("FromDate", string.Empty),
                        new XElement("PayrollPriceFormulaId", string.Empty),
                        new XElement("PayrollProductPriceFormulaId", string.Empty),
                        new XElement("PayrollProductSettingId", string.Empty));
                    payrollGroupElement.Add(payrollPriceFormulas);
                }

                if (!i.PurchaseAccounts.IsNullOrEmpty())
                {
                    foreach (var purchaseAccount in i.PurchaseAccounts)
                    {
                        payrollPurchaseAccount = new XElement("PurchaseAccounts",
                            new XElement("AccountId", purchaseAccount.Value.AccountId),
                            new XElement("Number", purchaseAccount.Value.Number),
                            new XElement("Name", purchaseAccount.Value.Name),
                            new XElement("Description", purchaseAccount.Value.Description),
                            new XElement("Percent", purchaseAccount.Value.Percent.ToString()),
                            new XElement("DimNr", accounts.FirstOrDefault(a => a.AccountId == purchaseAccount.Value.AccountId)?.AccountDim?.AccountDimNr));
                        payrollGroupElement.Add(payrollPurchaseAccount);
                    }
                }
                else
                {
                    payrollPurchaseAccount = new XElement("PurchaseAccounts",
                            new XElement("AccountId", 0),
                            new XElement("Number", string.Empty),
                            new XElement("Name", string.Empty),
                            new XElement("Description", string.Empty),
                            new XElement("Percent", string.Empty),
                            new XElement("DimNr", string.Empty));
                    payrollGroupElement.Add(payrollPurchaseAccount);
                }

                if (!i.AccountSettings.IsNullOrEmpty())
                {
                    foreach (var accountSetting in i.AccountSettings)
                    {
                        payrollAccountingSetting = new XElement("AccountingSetting",
                            new XElement("Type", accountSetting.Type.ToString()),
                            new XElement("DimNr", accountSetting.DimNr.ToString()),
                            new XElement("DimName", accountSetting.DimName.ToString()),
                            new XElement("Account1Id", accountSetting.Account1Id.ToString()),
                            new XElement("Account1Nr", accountSetting.Account1Nr.ToString()),
                            new XElement("Account1Name", accountSetting.Account1Name.ToString()),
                            new XElement("Account2Id", accountSetting.Account2Id.ToString()),
                            new XElement("Account2Nr", accountSetting.Account2Nr.ToString()),
                            new XElement("Account2Name", accountSetting.Account2Name.ToString()),
                            new XElement("Account3Id", accountSetting.Account3Id.ToString()),
                            new XElement("Account3Nr", accountSetting.Account3Nr.ToString()),
                            new XElement("Account3Name", accountSetting.Account3Name.ToString()),
                            new XElement("Account4Id", accountSetting.Account4Id.ToString()),
                            new XElement("Account4Nr", accountSetting.Account4Nr.ToString()),
                            new XElement("Account4Name", accountSetting.Account4Name.ToString()),
                            new XElement("Account5Id", accountSetting.Account5Id.ToString()),
                            new XElement("Account5Nr", accountSetting.Account5Nr.ToString()),
                            new XElement("Account5Name", accountSetting.Account5Name.ToString()),
                            new XElement("Account6Id", accountSetting.Account6Id.ToString()),
                            new XElement("Account6Nr", accountSetting.Account6Nr.ToString()),
                            new XElement("Account6Name", accountSetting.Account6Name.ToString()),
                            new XElement("Account7Id", accountSetting.Account7Id.ToString()),
                            new XElement("Account7Nr", accountSetting.Account7Nr.ToString()),
                            new XElement("Account7Name", accountSetting.Account7Name.ToString()),
                            new XElement("Account8Id", accountSetting.Account8Id.ToString()),
                            new XElement("Account8Nr", accountSetting.Account8Nr.ToString()),
                            new XElement("Account8Name", accountSetting.Account8Name.ToString()),
                            new XElement("Account9Id", accountSetting.Account9Id.ToString()),
                            new XElement("Account9Nr", accountSetting.Account9Nr.ToString()),
                            new XElement("Account9Name", accountSetting.Account9Name.ToString()),
                            new XElement("Account10Id", accountSetting.Account10Id.ToString()),
                            new XElement("Account10Nr", accountSetting.Account10Nr.ToString()),
                            new XElement("Account10Name", accountSetting.Account10Name.ToString()));
                        payrollGroupElement.Add(payrollAccountingSetting);
                    }
                }
                else
                {
                    payrollAccountingSetting = new XElement("AccountingSetting",
                       new XElement("Type", string.Empty),
                       new XElement("DimNr", string.Empty),
                       new XElement("DimName", string.Empty),
                       new XElement("Account1Id", string.Empty),
                       new XElement("Account1Nr", string.Empty),
                       new XElement("Account1Name", string.Empty),
                       new XElement("Account2Id", string.Empty),
                       new XElement("Account2Nr", string.Empty),
                       new XElement("Account2Name", string.Empty),
                       new XElement("Account3Id", string.Empty),
                       new XElement("Account3Nr", string.Empty),
                       new XElement("Account3Name", string.Empty),
                       new XElement("Account4Id", string.Empty),
                       new XElement("Account4Nr", string.Empty),
                       new XElement("Account4Name", string.Empty),
                       new XElement("Account5Id", string.Empty),
                       new XElement("Account5Nr", string.Empty),
                       new XElement("Account5Name", string.Empty),
                       new XElement("Account6Id", string.Empty),
                       new XElement("Account6Nr", string.Empty),
                       new XElement("Account6Name", string.Empty),
                       new XElement("Account7Id", string.Empty),
                       new XElement("Account7Nr", string.Empty),
                       new XElement("Account7Name", string.Empty),
                       new XElement("Account8Id", string.Empty),
                       new XElement("Account8Nr", string.Empty),
                       new XElement("Account8Name", string.Empty),
                       new XElement("Account9Id", string.Empty),
                       new XElement("Account9Nr", string.Empty),
                       new XElement("Account9Name", string.Empty),
                       new XElement("Account10Id", string.Empty),
                       new XElement("Account10Nr", string.Empty),
                       new XElement("Account10Name", string.Empty));
                    payrollGroupElement.Add(payrollAccountingSetting);
                }

                if (!i.CategoryRecords.IsNullOrEmpty())
                {
                    foreach (var categoryRecords in i.CategoryRecords)
                    {
                        payrollCompanyCategory = new XElement("CompanyCategoryRecord",
                            new XElement("CategoryId", categoryRecords.CategoryId.ToString()),
                            new XElement("Category", categoryRecords.Category.ToString()),
                            new XElement("StartDate", categoryRecords.DateFrom.ToString()),
                            new XElement("EndDate", categoryRecords.DateTo.ToString()));
                        payrollGroupElement.Add(payrollCompanyCategory);
                    }
                }
                else
                {
                    payrollCompanyCategory = new XElement("CompanyCategoryRecord",
                        new XElement("CategoryId", string.Empty),
                        new XElement("Category", string.Empty),
                        new XElement("StartDate", string.Empty),
                        new XElement("EndDate", string.Empty));
                    payrollGroupElement.Add(payrollCompanyCategory);
                }
                payrollGroupElements.Add(payrollGroupElement);
            }


            payrollProductElement = new XElement("PayrollProduct",
                    new XElement("ProductId", product.ProductId.ToString()),
                    new XElement("Type", ((SoeProductType)product.Type).ToString()),
                    new XElement("Number", product.Number.ToString()),
                    new XElement("Name", product.Name.ToString()),
                    new XElement("State", ((SoeEntityState)product.State).ToString()),
                    new XElement("AccountingPrio", product.AccountingPrio.ToString()),
                    new XElement("Factor", product.Factor.ToString()),
                    new XElement("PayrollType", ((TermGroup_PayrollType)product.PayrollType).ToString()),
                    new XElement("ShortName", product.ShortName.ToString()),
                    new XElement("Export", product.Export.ToString()),
                    new XElement("ExcludeInWorkTimeSummary", product.ExcludeInWorkTimeSummary.ToString()),
                    new XElement("Payed", product.Payed.ToString()),
                    new XElement("ResultType", ((TermGroup_PayrollResultType)product.ResultType).ToString()),
                    new XElement("AverageCalculated", product.AverageCalculated.ToString()),
                    new XElement("IncludeAmountInExport", product.IncludeAmountInExport.ToString()),
                    new XElement("SysPayrollTypeLevel1", product.SysPayrollTypeLevel1.HasValue ? product.SysPayrollTypeLevel1.Value : 0),
                    new XElement("SysPayrollTypeLevel2", product.SysPayrollTypeLevel2.HasValue ? product.SysPayrollTypeLevel2.Value : 0),
                    new XElement("SysPayrollTypeLevel3", product.SysPayrollTypeLevel3.HasValue ? product.SysPayrollTypeLevel3.Value : 0),
                    new XElement("SysPayrollTypeLevel4", product.SysPayrollTypeLevel4.HasValue ? product.SysPayrollTypeLevel4.Value : 0),
                    new XElement("SysPayrollTypeLevel1Name", product.SysPayrollTypeLevel1.HasValue ? GetText(product.SysPayrollTypeLevel1.Value, (int)TermGroup.SysPayrollType) : string.Empty),
                    new XElement("SysPayrollTypeLevel2Name", product.SysPayrollTypeLevel2.HasValue ? GetText(product.SysPayrollTypeLevel2.Value, (int)TermGroup.SysPayrollType) : string.Empty),
                    new XElement("SysPayrollTypeLevel3Name", product.SysPayrollTypeLevel3.HasValue ? GetText(product.SysPayrollTypeLevel3.Value, (int)TermGroup.SysPayrollType) : string.Empty),
                    new XElement("SysPayrollTypeLevel4Name", product.SysPayrollTypeLevel4.HasValue ? GetText(product.SysPayrollTypeLevel4.Value, (int)TermGroup.SysPayrollType) : string.Empty));

            // Add groups
            payrollProductElement.Add(payrollGroupElements);

            return payrollProductElement;

        }
        public XDocument TryUpdatePayrollSlipXML(XDocument doc, string additionalLogInfo)
        {
            try
            {
                int versionInXML = GetPayrollXMLVersion(doc);

                if (versionInXML <= payrollSlipXMLVersion)
                {
                    #region Version

                    var reportHeader = XmlUtil.GetDescendantElement(doc, "PayrollSlip", "ReportHeader");
                    var value = XmlUtil.GetChildElement(reportHeader, "Version");
                    if (value == null)
                        reportHeader.Add(new XElement("Version", payrollSlipXMLVersion.ToString()));
                    else if (value.Value != payrollSlipXMLVersion.ToString())
                        value.Value = payrollSlipXMLVersion.ToString();

                    #endregion

                    var timePeriodElements = XmlUtil.GetDescendantElements(doc, "PayrollSlip", "TimePeriod");

                    #region Version 1

                    if (versionInXML < 1 && !timePeriodElements.IsNullOrEmpty())
                    {
                        foreach (var timePeriodElement in timePeriodElements)
                        {
                            var commentElement = XmlUtil.GetChildElement(timePeriodElement, "Comment");
                            if (commentElement == null)
                            {
                                var isPreliminaryElement = XmlUtil.GetChildElement(timePeriodElement, "IsPreliminary");

                                if (isPreliminaryElement != null)
                                {
                                    isPreliminaryElement.AddAfterSelf(new XElement("Comment", string.Empty));
                                }
                            }
                        }
                    }

                    if (versionInXML < 2 && !timePeriodElements.IsNullOrEmpty())
                    {
                        foreach (var timePeriodElement in timePeriodElements)
                        {
                            var employeeElement = XmlUtil.GetDescendantElement(timePeriodElement, "Employee");
                            if (employeeElement != null && XmlUtil.GetDescendantElement(employeeElement, "Acc1Name") == null)
                            {
                                var remainingDaysOverdueElement = XmlUtil.GetDescendantElement(employeeElement, "RemainingDaysOverdue");
                                if (remainingDaysOverdueElement != null)
                                {
                                    remainingDaysOverdueElement.AddAfterSelf(new XElement("Acc3Sum", 0));
                                    remainingDaysOverdueElement.AddAfterSelf(new XElement("Acc2Sum", 0));
                                    remainingDaysOverdueElement.AddAfterSelf(new XElement("Acc1Sum", 0));
                                    remainingDaysOverdueElement.AddAfterSelf(new XElement("Acc3Value", 0));
                                    remainingDaysOverdueElement.AddAfterSelf(new XElement("Acc2Value", 0));
                                    remainingDaysOverdueElement.AddAfterSelf(new XElement("Acc1Value", 0));
                                    remainingDaysOverdueElement.AddAfterSelf(new XElement("Acc3Name", string.Empty));
                                    remainingDaysOverdueElement.AddAfterSelf(new XElement("Acc2Name", string.Empty));
                                    remainingDaysOverdueElement.AddAfterSelf(new XElement("Acc1Name", string.Empty));
                                }
                            }
                        }
                    }

                    if (versionInXML < 3 && !timePeriodElements.IsNullOrEmpty())
                    {
                        foreach (var timePeriodElement in timePeriodElements)
                        {
                            var employeeElement = XmlUtil.GetDescendantElement(timePeriodElement, "Employee");
                            if (employeeElement != null && XmlUtil.GetDescendantElement(employeeElement, "VacationCoefficient") == null)
                            {
                                var acc1NameElement = XmlUtil.GetDescendantElement(employeeElement, "Acc3Sum");
                                if (acc1NameElement != null)
                                {
                                    acc1NameElement.AddAfterSelf(new XElement("VacationCoefficient", 0));
                                }
                            }
                        }
                    }

                    if (versionInXML < 4 && !timePeriodElements.IsNullOrEmpty())
                    {
                        foreach (var timePeriodElement in timePeriodElements)
                        {
                            var employeeElement = XmlUtil.GetDescendantElement(timePeriodElement, "Employee");
                            if (employeeElement != null && XmlUtil.GetDescendantElement(employeeElement, "Acc4Name") == null)
                            {
                                var vacationCoefficientElement = XmlUtil.GetDescendantElement(employeeElement, "VacationCoefficient");
                                if (vacationCoefficientElement != null)
                                {
                                    vacationCoefficientElement.AddAfterSelf(new XElement("Acc5Sum", 0));
                                    vacationCoefficientElement.AddAfterSelf(new XElement("Acc4Sum", 0));
                                    vacationCoefficientElement.AddAfterSelf(new XElement("Acc5Value", 0));
                                    vacationCoefficientElement.AddAfterSelf(new XElement("Acc4Value", 0));
                                    vacationCoefficientElement.AddAfterSelf(new XElement("Acc5Name", string.Empty));
                                    vacationCoefficientElement.AddAfterSelf(new XElement("Acc4Name", string.Empty));
                                }
                            }
                        }
                    }
                    if (versionInXML < 5)
                    {

                        var pageHeaderLabels = XmlUtil.GetDescendantElement(doc, "PayrollSlip", "PageHeaderLabels");
                        var label = XmlUtil.GetChildElement(pageHeaderLabels, "EmployersContributionLabel");
                        if (label == null)
                            pageHeaderLabels.Add(new XElement("EmployersContributionLabel", this.GetReportText(765, "Arbetsgivaravgift")));

                        var taxLabel = XmlUtil.GetChildElement(pageHeaderLabels, "TaxDetails");
                        if (taxLabel == null)
                            pageHeaderLabels.Add(new XElement("TaxDetails", this.GetReportText(9241, "Skatteuppgifter")));


                        if (!timePeriodElements.IsNullOrEmpty())
                        {
                            foreach (var timePeriodElement in timePeriodElements)
                            {
                                var employeeElement = XmlUtil.GetDescendantElement(timePeriodElement, "Employee");
                                if (employeeElement != null && XmlUtil.GetDescendantElement(employeeElement, "PeriodEmploymentTaxSum") == null)
                                {
                                    var periodNetSum = XmlUtil.GetDescendantElement(employeeElement, "PeriodNetSum");
                                    if (periodNetSum != null)
                                    {
                                        periodNetSum.AddAfterSelf(new XElement("PeriodEmploymentTaxSum", 0));
                                    }
                                }
                            }
                        }
                    }

                    #endregion
                }
            }
            catch (Exception ex)
            {
                string message = String.Format("TryUpdatePayrollSlipXML failed:{0}", additionalLogInfo);
                SoeGeneralException soeEx = new SoeGeneralException(message, ex, this.ToString());
                base.LogError(soeEx, this.log);
            }

            return doc;
        }
        private int GetPayrollXMLVersion(XDocument doc)
        {
            var payrollSlip = XmlUtil.GetDescendantElement(doc, "PayrollSlip", "ReportHeader");
            var element = XmlUtil.GetDescendantElement(payrollSlip, "Version");

            int version = 0;
            if (element != null)
                int.TryParse(element.Value, out version);
            return version;
        }
        protected XDocument CreatePayrollSlipXML(CompEntities entities, CreateReportResult reportResult, Employee employee, List<TimeAccumulator> timeAccumulators = null, TimePeriod currentTimePeriod = null, List<EmployeeGroup> employeeGroups = null, List<PayrollGroup> payrollGroups = null, List<PayrollPriceType> priceTypes = null, Dictionary<int, string> disbursementMethodsDict = null, List<PayrollProduct> companyProducts = null, List<AccountDim> accountDimInternals = null, PaymentInformation paymentInformation = null, List<PayrollStartValueHead> payrollStartValueHeads = null, List<VacationGroup> vacationGroups = null, bool isPreliminary = false, List<PayrollCalculationProductDTO> payrollCalculationProductItems = null, EmployeeTimePeriod currentEmployeeTimePeriod = null, bool returnNullOnNoRows = false)
        {
            #region Prereq

            if (!TryGetIdsFromSelection(reportResult, out List<int> selectionTimePeriodIds, "periods"))
                return null;

            #endregion

            #region Init repository

            InitPersonalDataEmployeeReportRepository(entities);

            #endregion

            #region Init document

            //Document
            XDocument document = XmlUtil.CreateDocument();

            //Root
            XElement rootElement = new XElement(ROOT + "_" + "PayrollSlip");

            //PayrollSlipReport
            XElement payrollSlipElement = new XElement("PayrollSlip");

            #endregion

            #region ReportHeaderLabels

            XElement reportHeaderLabelsElement = CreateTimeReportHeaderLabelsElement();
            reportHeaderLabelsElement.Add(CreateDateIntervalLabelReportHeaderLabelsElement());
            payrollSlipElement.Add(reportHeaderLabelsElement);

            #endregion

            #region ReportHeader

            XElement reportHeaderElement = CreateExtendedPersonellReportHeaderElement(entities, reportResult);
            string companyLogoPath = "";// GetCompanyLogoFilePath(entities, es.ActorCompanyId, true);
            reportHeaderElement.Add(new XElement("CompanyLogo", companyLogoPath));
            if (paymentInformation == null)
                paymentInformation = PaymentManager.GetPaymentInformationFromActor(entities, Company.ActorCompanyId, true, false);

            string bankAccountNr = PaymentManager.GetPaymentNr(paymentInformation, TermGroup_SysPaymentType.Bank);
            reportHeaderElement.Add(new XElement("CompanyBankAccountNr", bankAccountNr));
            reportHeaderElement.Add(new XElement("Version", payrollSlipXMLVersion));
            payrollSlipElement.Add(reportHeaderElement);

            #endregion

            #region PageHeaderLabels

            XElement pageHeaderLabelsElement = new XElement("PageHeaderLabels");
            CreatePayrollSlipPageHeaderLabelsElement(pageHeaderLabelsElement);
            payrollSlipElement.Add(pageHeaderLabelsElement);

            #endregion

            #region Prereq

            int employeeId = employee.EmployeeId;
            int timePeriodId = currentTimePeriod != null ? currentTimePeriod.TimePeriodId : selectionTimePeriodIds.FirstOrDefault();  //Only support for one period for now

            if (currentTimePeriod == null)
                currentTimePeriod = TimePeriodManager.GetTimePeriod(entities, timePeriodId, Company.ActorCompanyId, true);
            if (currentTimePeriod == null)
                return null;

            if (disbursementMethodsDict == null)
                disbursementMethodsDict = EmployeeManager.GetEmployeeDisbursementMethods((int)TermGroup_Languages.Swedish);
            if (companyProducts == null)
                companyProducts = ProductManager.GetPayrollProductsWithSettings(entities, Company.ActorCompanyId, null);
            if (accountDimInternals == null)
                accountDimInternals = AccountManager.GetAccountDimInternalsByCompany(entities, reportResult.ActorCompanyId);

            List<EmployeeTimePeriod> previousEmployeeTimePeriodValuesInYear = TimePeriodManager.GetPreviousLockedOrPaidEmployeeTimePeriods(entities, currentTimePeriod, employeeId, Company.ActorCompanyId);
            if (currentEmployeeTimePeriod == null)
                currentEmployeeTimePeriod = TimePeriodManager.GetEmployeeTimePeriod(entities, employeeId, timePeriodId, Company.ActorCompanyId);
            bool showSocialSec = FeatureManager.HasRolePermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_SocialSec, Permission.Readonly, reportResult.RoleId, reportResult.ActorCompanyId, entities: entities);

            #endregion

            #region Content

            int timePeriodXmlId = 1;

            if (currentEmployeeTimePeriod != null)
            {
                #region Payroll start values

                decimal importedStartValueTax = 0;
                decimal importedStartValueGrossSalary = 0;
                decimal importedStartValueBenefit = 0;

                if (payrollStartValueHeads.IsNullOrEmpty())
                    payrollStartValueHeads = PayrollManager.GetPayrollStartValueHeads(entities, Company.ActorCompanyId);

                if (!payrollStartValueHeads.IsNullOrEmpty() && currentTimePeriod.PaymentDate.HasValue && payrollStartValueHeads.Any(x => x.DateTo.Year == currentTimePeriod.PaymentDate.Value.Year))
                {
                    List<int> headIds = payrollStartValueHeads.Where(x => x.DateTo.Year == currentTimePeriod.PaymentDate.Value.Year).Select(x => x.PayrollStartValueHeadId).ToList();
                    List<int> rowIds = (from p in entities.PayrollStartValueRow
                                        where headIds.Contains(p.PayrollStartValueHeadId) &&
                                        p.EmployeeId == employeeId &&
                                        p.State == (int)SoeEntityState.Active
                                        select p.PayrollStartValueRowId).ToList();

                    List<TimePayrollTransaction> timePayrollTransactionsForStartValues = PayrollManager.GetTimePayrollTransactionsFromPayrollStartValueRowIds(entities, reportResult.ActorCompanyId, employeeId, rowIds);
                    foreach (TimePayrollTransaction timePayrollTransaction in timePayrollTransactionsForStartValues)
                    {
                        if (timePayrollTransaction.IsTaxAndNotOptional())
                            importedStartValueTax += timePayrollTransaction.Amount.HasValue ? timePayrollTransaction.Amount.Value : 0;
                        else if (timePayrollTransaction.IsBenefit())
                            importedStartValueBenefit += timePayrollTransaction.Amount.HasValue ? timePayrollTransaction.Amount.Value : 0;
                        else if (timePayrollTransaction.IsGrossSalary())
                            importedStartValueGrossSalary += timePayrollTransaction.Amount.HasValue ? timePayrollTransaction.Amount.Value : 0;
                    }
                }

                #endregion

                #region TimePeriod element

                XElement timePeriodElement = new XElement("TimePeriod",
                    new XAttribute("Id", timePeriodXmlId),
                    new XElement("Name", currentTimePeriod.Name),
                    new XElement("StartDate", currentTimePeriod.StartDate.ToShortDateString()),
                    new XElement("StopDate", currentTimePeriod.StopDate.ToShortDateString()),
                    new XElement("PayrollStartDate", currentTimePeriod.PayrollStartDate.HasValue ? currentTimePeriod.PayrollStartDate.Value.ToShortDateString() : ""),
                    new XElement("PayrollStopDate", currentTimePeriod.PayrollStopDate.HasValue ? currentTimePeriod.PayrollStopDate.Value.ToShortDateString() : ""),
                    new XElement("PaymentDate", currentTimePeriod.PaymentDate.HasValue ? currentTimePeriod.PaymentDate.Value.ToShortDateString() : ""),
                    new XElement("IsExtraPeriod", currentTimePeriod.ExtraPeriod ? 1 : 0),
                    new XElement("IsPreliminary", isPreliminary.ToInt()),
                    new XElement("Comment", !string.IsNullOrEmpty(currentTimePeriod.Comment) ? currentTimePeriod.Comment : string.Empty));
                timePeriodXmlId++;

                #endregion

                #region Employee element

                int employeeXmlId = 1;
                if (employee == null)
                    employee = EmployeeManager.GetEmployeeWithEmploymentAndEmploymentChangeBatch(entities, employeeId, Company.ActorCompanyId, true);

                if (employee != null)
                {
                    decimal vacationCoefficient = EmployeeManager.GetEmployeeFactor(entities, employee.EmployeeId, TermGroup_EmployeeFactorType.VacationCoefficient, currentTimePeriod.StopDate);
                    Contact contact = EmployeeManager.GetEmployeeContact(entities, employee.EmployeeId);
                    List<EmployeeTimePeriodProductSetting> employeeTimePeriodProductSettings = TimePeriodManager.GetEmployeeTimePeriodProductSettings(entities, employee.EmployeeId, currentTimePeriod.TimePeriodId, Company.ActorCompanyId);

                    #region Employee element

                    if (payrollCalculationProductItems == null)
                    {
                        //   payrollCalculationProductItems = TimeTreePayrollManager.GetPayrollCalculationProducts(entities, reportResult.ActorCompanyId, timePeriodId, employeeId, isPayrollSlip: true);
                        payrollCalculationProductItems = TimeTreePayrollManager.GetPayrollCalculationProducts(
                                  entities,
                                  reportResult.ActorCompanyId,
                                  currentTimePeriod,
                                  employee,
                                  getEmployeeTimePeriodSettings: false,
                                  showAllTransactions: true,
                                  companyAccountDims: new List<AccountDimDTO>(),
                                  employeeGroups: employeeGroups,
                                  timePayrollTransactionItems: null,
                                  timePayrollTransactionAccountStds: new List<AccountDTO>(),
                                  timePayrollTransactionAccountInternalItems: new List<GetTimePayrollTransactionAccountsForEmployee_Result>(),
                                  timePayrollScheduleTransactionItems: null,
                                  timePayrollScheduleTransactionAccountStds: new List<AccountDTO>(),
                                  timePayrollScheduleTransactionAccountInternalItems: new List<GetTimePayrollScheduleTransactionAccountsForEmployee_Result>(),
                                  employeeTimePeriods: null, isPayrollSlip: true).ToList();

                    }

                    payrollCalculationProductItems = payrollCalculationProductItems.Where(w => w.EmployeeId == employee.EmployeeId).ToList();

                    if (returnNullOnNoRows && !payrollCalculationProductItems.Any())
                        return null;

                    if (vacationGroups == null)
                        vacationGroups = PayrollManager.GetVacationGroupsWithVacationGroupSE(entities, reportResult.ActorCompanyId).ToList();

                    PayrollCalculationPeriodSumDTO payrollCalculationPeriodSumItem = PayrollRulesUtil.CalculateSum(payrollCalculationProductItems);
                    EmployeeVacationSE employeeVacationSE = EmployeeManager.GetLatestEmployeeVacationSE(entities, employeeId);
                    PayrollGroup payrollGroup = employee.GetPayrollGroup(currentTimePeriod.StartDate, currentTimePeriod.StopDate, payrollGroups);
                    VacationGroup vacationGroup = employee.GetVacationGroup(currentTimePeriod.StartDate, currentTimePeriod.StopDate, vacationGroups);
                    EmployeeGroup employeeGroup = !timeAccumulators.IsNullOrEmpty() ? employee.GetEmployeeGroup(currentTimePeriod.StopDate, employeeGroups) : null;

                    if (!timeAccumulators.IsNullOrEmpty() && employeeGroup == null && employee.Employment.GetEndDate() != null && employee.Employment.GetEndDate() < currentTimePeriod.StopDate)
                        employeeGroup = employee.GetEmployeeGroup(employee.Employment.GetEndDate(), employeeGroups);

                    bool calculateHours = false;
                    if (vacationGroup != null)
                    {
                        var vacationGroupSE = vacationGroup.VacationGroupSE.FirstOrDefault();
                        if (vacationGroupSE != null)
                            calculateHours = vacationGroupSE.VacationHandleRule == (int)TermGroup_VacationGroupVacationHandleRule.Hours;
                    }

                    decimal remainingVacationPaid = (employeeVacationSE != null) ? (calculateHours ? (employeeVacationSE.EarnedDaysRemainingHoursPaid.GetValue()) : (employeeVacationSE.RemainingDaysPaid.GetValue())) : 0;
                    decimal remainingVacationUnpaid = (employeeVacationSE != null) ? (calculateHours ? (employeeVacationSE.EarnedDaysRemainingHoursUnpaid.GetValue()) : (employeeVacationSE.RemainingDaysUnpaid.GetValue())) : 0;
                    decimal remainingVacationAdvance = (employeeVacationSE != null) ? (calculateHours ? (employeeVacationSE.EarnedDaysRemainingHoursAdvance.GetValue()) : (employeeVacationSE.RemainingDaysAdvance.GetValue())) : 0;
                    decimal remainingVacationYear1 = (employeeVacationSE != null) ? (calculateHours ? (employeeVacationSE.EarnedDaysRemainingHoursYear1.GetValue()) : (employeeVacationSE.RemainingDaysYear1.GetValue())) : 0;
                    decimal remainingVacationYear2 = (employeeVacationSE != null) ? (calculateHours ? (employeeVacationSE.EarnedDaysRemainingHoursYear2.GetValue()) : (employeeVacationSE.RemainingDaysYear2.GetValue())) : 0;
                    decimal remainingVacationYear3 = (employeeVacationSE != null) ? (calculateHours ? (employeeVacationSE.EarnedDaysRemainingHoursYear3.GetValue()) : (employeeVacationSE.RemainingDaysYear3.GetValue())) : 0;
                    decimal remainingVacationYear4 = (employeeVacationSE != null) ? (calculateHours ? (employeeVacationSE.EarnedDaysRemainingHoursYear4.GetValue()) : (employeeVacationSE.RemainingDaysYear4.GetValue())) : 0;
                    decimal remainingVacationYear5 = (employeeVacationSE != null) ? (calculateHours ? (employeeVacationSE.EarnedDaysRemainingHoursYear5.GetValue()) : (employeeVacationSE.RemainingDaysYear5.GetValue())) : 0;
                    decimal remainingVacationOverdue = (employeeVacationSE != null) ? (calculateHours ? (employeeVacationSE.EarnedDaysRemainingHoursOverdue.GetValue()) : (employeeVacationSE.RemainingDaysOverdue.GetValue())) : 0;

                    #region Accumulators

                    string acc1Name = string.Empty;
                    string acc2Name = string.Empty;
                    string acc3Name = string.Empty;
                    string acc4Name = string.Empty;
                    string acc5Name = string.Empty;
                    decimal acc1Value = 0;
                    decimal acc2Value = 0;
                    decimal acc3Value = 0;
                    decimal acc4Value = 0;
                    decimal acc5Value = 0;
                    decimal acc1Sum = 0;
                    decimal acc2Sum = 0;
                    decimal acc3Sum = 0;
                    decimal acc4Sum = 0;
                    decimal acc5Sum = 0;

                    if (!timeAccumulators.IsNullOrEmpty() && employeeGroup != null)
                    {
                        DateTime startOfYear = CalendarUtility.GetBeginningOfYear(currentTimePeriod.StopDate);
                        if (!employeeGroup.TimeAccumulatorEmployeeGroupRule.IsLoaded)
                            employeeGroup.TimeAccumulatorEmployeeGroupRule.Load();

                        List<TimeAccumulatorBalance> timeAccumulatorBalancesForEmployee = employeeGroup.TimeAccumulatorEmployeeGroupRule.Where(x => x.State == (int)SoeEntityState.Active).Any(w => w.ShowOnPayrollSlip) ? TimeAccumulatorManager.GetTimeAccumulatorBalancesForEmployee(entities, Company.ActorCompanyId, employee.EmployeeId) : new List<TimeAccumulatorBalance>();
                        int count = 1;
                        foreach (var item in employeeGroup.TimeAccumulatorEmployeeGroupRule.Where(x => x.State == (int)SoeEntityState.Active).Where(w => w.ShowOnPayrollSlip))
                        {
                            if (!item.TimeAccumulatorReference.IsLoaded)
                                item.TimeAccumulatorReference.Load();

                            if (item.TimeAccumulator.State != (int)SoeEntityState.Active)
                                continue;

                            if (count == 1)
                            {
                                acc1Name = item.TimeAccumulator.Name;
                                acc1Value += TimeAccumulatorManager.CalculateTimePayrollTransactions(entities, employee.EmployeeId, item.TimeAccumulatorId, startOfYear, CalendarUtility.GetEndOfDay(currentTimePeriod.StopDate));
                                acc1Value += TimeAccumulatorManager.CalculateTimeCodeTransactions(entities, employee.EmployeeId, item.TimeAccumulatorId, startOfYear, CalendarUtility.GetEndOfDay(currentTimePeriod.StopDate));
                                var timeAccumulatorBalance = timeAccumulatorBalancesForEmployee.FirstOrDefault(x => x.TimeAccumulatorId == item.TimeAccumulatorId && x.Date == startOfYear.Date);
                                acc1Value += timeAccumulatorBalance?.Quantity ?? 0;
                            }
                            if (count == 2)
                            {
                                acc2Name = item.TimeAccumulator.Name;
                                acc2Value += TimeAccumulatorManager.CalculateTimePayrollTransactions(entities, employee.EmployeeId, item.TimeAccumulatorId, startOfYear, CalendarUtility.GetEndOfDay(currentTimePeriod.StopDate));
                                acc2Value += TimeAccumulatorManager.CalculateTimeCodeTransactions(entities, employee.EmployeeId, item.TimeAccumulatorId, startOfYear, CalendarUtility.GetEndOfDay(currentTimePeriod.StopDate));
                                var timeAccumulatorBalance = timeAccumulatorBalancesForEmployee.FirstOrDefault(x => x.TimeAccumulatorId == item.TimeAccumulatorId && x.Date == startOfYear.Date);
                                acc2Value += timeAccumulatorBalance?.Quantity ?? 0;
                            }
                            if (count == 3)
                            {
                                acc3Name = item.TimeAccumulator.Name;
                                acc3Value += TimeAccumulatorManager.CalculateTimePayrollTransactions(entities, employee.EmployeeId, item.TimeAccumulatorId, startOfYear, CalendarUtility.GetEndOfDay(currentTimePeriod.StopDate));
                                acc3Value += TimeAccumulatorManager.CalculateTimeCodeTransactions(entities, employee.EmployeeId, item.TimeAccumulatorId, startOfYear, CalendarUtility.GetEndOfDay(currentTimePeriod.StopDate));
                                var timeAccumulatorBalance = timeAccumulatorBalancesForEmployee.FirstOrDefault(x => x.TimeAccumulatorId == item.TimeAccumulatorId && x.Date == startOfYear.Date);
                                acc3Value += timeAccumulatorBalance?.Quantity ?? 0;
                            }
                            if (count == 4)
                            {
                                acc4Name = item.TimeAccumulator.Name;
                                acc4Value += TimeAccumulatorManager.CalculateTimePayrollTransactions(entities, employee.EmployeeId, item.TimeAccumulatorId, startOfYear, CalendarUtility.GetEndOfDay(currentTimePeriod.StopDate));
                                acc4Value += TimeAccumulatorManager.CalculateTimeCodeTransactions(entities, employee.EmployeeId, item.TimeAccumulatorId, startOfYear, CalendarUtility.GetEndOfDay(currentTimePeriod.StopDate));
                                var timeAccumulatorBalance = timeAccumulatorBalancesForEmployee.FirstOrDefault(x => x.TimeAccumulatorId == item.TimeAccumulatorId && x.Date == startOfYear.Date);
                                acc4Value += timeAccumulatorBalance?.Quantity ?? 0;
                            }
                            if (count == 5)
                            {
                                acc5Name = item.TimeAccumulator.Name;
                                acc5Value += TimeAccumulatorManager.CalculateTimePayrollTransactions(entities, employee.EmployeeId, item.TimeAccumulatorId, startOfYear, CalendarUtility.GetEndOfDay(currentTimePeriod.StopDate));
                                acc5Value += TimeAccumulatorManager.CalculateTimeCodeTransactions(entities, employee.EmployeeId, item.TimeAccumulatorId, startOfYear, CalendarUtility.GetEndOfDay(currentTimePeriod.StopDate));
                                var timeAccumulatorBalance = timeAccumulatorBalancesForEmployee.FirstOrDefault(x => x.TimeAccumulatorId == item.TimeAccumulatorId && x.Date == startOfYear.Date);
                                acc5Value += timeAccumulatorBalance?.Quantity ?? 0;
                            }

                            count++;
                        }
                    }

                    #endregion

                    XElement employeeElement = new XElement("Employee",
                        new XAttribute("Id", employeeXmlId),
                        new XElement("EmployeeNr", employee.EmployeeNr),
                        new XElement("EmployeeFirstName", employee.FirstName),
                        new XElement("EmployeeLastName", employee.LastName),
                        new XElement("EmployeeName", employee.Name),
                        new XElement("EmployeeSocialSec", employee.ContactPerson != null && showSocialSec ? employee.SocialSec : StringUtility.SocialSecYYMMDD_Dash_Stars(employee.SocialSec)),
                        new XElement("DisbursementMethod", disbursementMethodsDict.ContainsKey(employee.DisbursementMethod) ? disbursementMethodsDict[employee.DisbursementMethod] : string.Empty),
                        new XElement("DisbursementClearingNr", employee.DisbursementClearingNr),
                        new XElement("DisbursementAccountNr", employee.DisbursementAccountNr),
                        new XElement("Note", employee.Note),
                        new XElement("PeriodGrossSum", payrollCalculationPeriodSumItem.Gross),
                        new XElement("PeriodBenefitSum", payrollCalculationPeriodSumItem.BenefitInvertExcluded),
                        new XElement("PeriodTaxSum", payrollCalculationPeriodSumItem.Tax),
                        new XElement("PeriodCompensationSum", payrollCalculationPeriodSumItem.Compensation),
                        new XElement("PeriodDeductionSum", payrollCalculationPeriodSumItem.Deduction),
                        new XElement("PeriodNetSum", payrollCalculationPeriodSumItem.Net),
                        new XElement("PeriodEmploymentTaxSum", payrollCalculationPeriodSumItem.EmploymentTaxDebit),
                        new XElement("YearGrossSum", payrollCalculationPeriodSumItem.Gross + previousEmployeeTimePeriodValuesInYear.GetGrossSalarySum() + importedStartValueGrossSalary),
                        new XElement("YearBenefitSum", payrollCalculationPeriodSumItem.BenefitInvertExcluded + previousEmployeeTimePeriodValuesInYear.GetBenefitSum() + importedStartValueBenefit),
                        new XElement("YearTaxSum", payrollCalculationPeriodSumItem.Tax + previousEmployeeTimePeriodValuesInYear.GetTaxSum() + importedStartValueTax),
                        new XElement("YearCompensationSum", payrollCalculationPeriodSumItem.Compensation + previousEmployeeTimePeriodValuesInYear.GetCompensationSum()),
                        new XElement("YearDeductionSum", payrollCalculationPeriodSumItem.Deduction + previousEmployeeTimePeriodValuesInYear.GetDeductionSum()),
                        new XElement("YearNetSum", payrollCalculationPeriodSumItem.Net + previousEmployeeTimePeriodValuesInYear.GetNetSalarySum()),
                        new XElement("YearEmploymentTaxSum", payrollCalculationPeriodSumItem.EmploymentTaxDebit + previousEmployeeTimePeriodValuesInYear.GetEmploymentTaxCreditSum()),
                        new XElement("RemainingDaysPaid", remainingVacationPaid),
                        new XElement("RemainingDaysUnpaid", remainingVacationUnpaid),
                        new XElement("RemainingDaysAdvance", remainingVacationAdvance),
                        new XElement("RemainingDaysYear1", remainingVacationYear1),
                        new XElement("RemainingDaysYear2", remainingVacationYear2),
                        new XElement("RemainingDaysYear3", remainingVacationYear3),
                        new XElement("RemainingDaysYear4", remainingVacationYear4),
                        new XElement("RemainingDaysYear5", remainingVacationYear5),
                        new XElement("RemainingDaysOverdue", remainingVacationOverdue),
                        new XElement("Acc1Name", acc1Name),
                        new XElement("Acc2Name", acc2Name),
                        new XElement("Acc3Name", acc3Name),
                        new XElement("Acc1Value", acc1Value),
                        new XElement("Acc2Value", acc2Value),
                        new XElement("Acc3Value", acc3Value),
                        new XElement("Acc1Sum", acc1Sum),
                        new XElement("Acc2Sum", acc2Sum),
                        new XElement("Acc3Sum", acc3Sum),
                        new XElement("VacationCoefficient", vacationCoefficient),
                        new XElement("Acc4Name", acc4Name),
                        new XElement("Acc5Name", acc5Name),
                        new XElement("Acc4Value", acc3Value),
                        new XElement("Acc5Value", acc3Value),
                        new XElement("Acc4Sum", acc4Sum),
                        new XElement("Acc5Sum", acc5Sum));

                    personalDataRepository.AddEmployee(employee, employeeElement);
                    employeeXmlId++;

                    #endregion

                    #region EmployeeContactInformation element

                    if (contact != null)
                        employeeElement.Add(CreateTimeEmployeeContactInformationElements(entities, contact));

                    #endregion

                    #region Transaction elements

                    int transactionXmlId = 1;
                    foreach (var payrollCalculationProductItem in payrollCalculationProductItems)
                    {
                        PayrollProduct payrollProductWithSetting = companyProducts.FirstOrDefault(x => x.ProductId == payrollCalculationProductItem.PayrollProductId);
                        PayrollProductSetting productSetting = payrollProductWithSetting?.GetSetting(payrollGroup?.PayrollGroupId);
                        EmployeeTimePeriodProductSetting employeeTimePeriodProductSetting = employeeTimePeriodProductSettings.FirstOrDefault(x => x.PayrollProductId == payrollCalculationProductItem.PayrollProductId);

                        bool showDateOnPaySlip = productSetting?.PrintDate ?? false;
                        bool showPayrollProductOnPaySlip = false;
                        if (employeeTimePeriodProductSetting != null && employeeTimePeriodProductSetting.UseSettings)
                            showPayrollProductOnPaySlip = employeeTimePeriodProductSetting.PrintOnSalarySpecification;
                        else
                            showPayrollProductOnPaySlip = productSetting?.PrintOnSalarySpecification ?? false;

                        if (payrollCalculationProductItem.Amount.IsNullOrEmpty() && (productSetting?.DontPrintOnSalarySpecificationWhenZeroAmount ?? false))
                            showPayrollProductOnPaySlip = false;

                        string note = employeeTimePeriodProductSetting != null ? employeeTimePeriodProductSetting.Note : string.Empty;

                        if (!string.IsNullOrEmpty(payrollCalculationProductItem.TransactionComment))
                            note = string.IsNullOrEmpty(note) ? payrollCalculationProductItem.TransactionComment : $"{note} - {payrollCalculationProductItem.TransactionComment}";

                        if (showPayrollProductOnPaySlip)
                        {
                            XElement transactionElement = new XElement("Transaction",
                                new XAttribute("Id", transactionXmlId),
                                new XElement("ProductNr", payrollCalculationProductItem.PayrollProductNumber),
                                new XElement("ProductName", payrollCalculationProductItem.PayrollProductName),
                                new XElement("DateFrom", showDateOnPaySlip && payrollCalculationProductItem.DateFrom.HasValue ? payrollCalculationProductItem.DateFrom.Value : CalendarUtility.DATETIME_DEFAULT),
                                new XElement("DateTo", showDateOnPaySlip && payrollCalculationProductItem.DateTo.HasValue ? payrollCalculationProductItem.DateTo.Value : CalendarUtility.DATETIME_DEFAULT),
                                new XElement("Quantity", !payrollCalculationProductItem.HideQuantity ? payrollCalculationProductItem.Quantity : 0),
                                new XElement("IsTime", payrollCalculationProductItem.IsQuantityOrFixed ? 0 : 1),
                                new XElement("UnitPrice", payrollCalculationProductItem.HideUnitprice ? 0 : payrollCalculationProductItem.UnitPrice ?? 0),
                                new XElement("Amount", payrollCalculationProductItem.Amount.HasValue ? payrollCalculationProductItem.Amount.Value : 0),
                                new XElement("Note", note),
                                new XElement("QuantityWorkDays", payrollCalculationProductItem.QuantityWorkDays),
                                new XElement("QuantityCalendarDays", payrollCalculationProductItem.QuantityCalendarDays),
                                new XElement("CalenderDayFactor", payrollCalculationProductItem.CalenderDayFactor),
                                new XElement("TimeUnit", payrollCalculationProductItem.TimeUnit),
                                new XElement("TimeUnitName", GetText(payrollCalculationProductItem.TimeUnit, (int)TermGroup.PayrollProductTimeUnit)),
                                new XElement("IsRetroactive", payrollCalculationProductItem.IsRetroactive.ToInt()));

                            for (int i = 1; i <= Constants.NOOFDIMENSIONS; i++)
                            {
                                if (i > accountDimInternals.Count)
                                {
                                    transactionElement.Add(
                                        new XElement("AccountInternalNr" + i, String.Empty),
                                        new XElement("AccountInternalName" + i, String.Empty));
                                    continue;
                                }

                                AccountDim accountDim = !accountDimInternals.IsNullOrEmpty() ? accountDimInternals.ElementAt(i - 1) : null;
                                AccountDTO accountInternal = accountDim != null && payrollCalculationProductItem.AccountInternals != null ? payrollCalculationProductItem.AccountInternals.FirstOrDefault(ai => ai.AccountDimId == accountDim.AccountDimId) : null;
                                transactionElement.Add(
                                    new XElement("AccountInternalNr" + i, accountInternal != null ? accountInternal.AccountNr : String.Empty),
                                    new XElement("AccountInternalName" + i, accountInternal != null ? accountInternal.Name : String.Empty));
                            }

                            employeeElement.Add(transactionElement);
                            transactionXmlId++;
                        }
                    }

                    #region Default transaction elemenet

                    if (transactionXmlId == 1)
                    {
                        XElement transactionElement = new XElement("Transaction",
                                new XAttribute("Id", 1),
                                new XElement("ProductNr", string.Empty),
                                new XElement("ProductName", string.Empty),
                                new XElement("DateFrom", string.Empty),
                                new XElement("DateTo", string.Empty),
                                new XElement("Quantity", 0),
                                new XElement("IsTime", 0),
                                new XElement("UnitPrice", 0),
                                new XElement("Amount", 0),
                                new XElement("Note", string.Empty),
                                new XElement("QuantityWorkDays", 0),
                                new XElement("QuantityCalendarDays", 0),
                                new XElement("CalenderDayFactor", 0),
                                new XElement("TimeUnit", 0),
                                new XElement("TimeUnitName", string.Empty)
                                );

                        employeeElement.Add(transactionElement);

                    }
                    #endregion

                    #endregion

                    timePeriodElement.Add(employeeElement);
                    payrollSlipElement.Add(timePeriodElement);
                }

                #endregion

                #region Default Employee element

                if (employeeXmlId == 1)
                {
                    XElement employeeElement = new XElement("Employee",
                    new XAttribute("Id", 1),
                    new XElement("EmployeeNr", string.Empty),
                    new XElement("EmployeeFirstName", string.Empty),
                    new XElement("EmployeeLastName", string.Empty),
                    new XElement("EmployeeName", string.Empty),
                    new XElement("EmployeeSocialSec", string.Empty),
                    new XElement("DisbursementMethod", string.Empty),
                    new XElement("DisbursementClearingNr", string.Empty),
                    new XElement("DisbursementAccountNr", string.Empty),
                    new XElement("Note", string.Empty),
                    new XElement("PeriodGrossSum", 0),
                    new XElement("PeriodBenefitSum", 0),
                    new XElement("PeriodTaxSum", 0),
                    new XElement("PeriodCompensationSum", 0),
                    new XElement("PeriodDeductionSum", 0),
                    new XElement("PeriodNetSum", 0),
                    new XElement("PeriodEmploymentTaxSum", 0),
                    new XElement("YearGrossSum", 0),
                    new XElement("YearBenefitSum", 0),
                    new XElement("YearTaxSum", 0),
                    new XElement("YearCompensationSum", 0),
                    new XElement("YearDeductionSum", 0),
                    new XElement("YearNetSum", 0),
                    new XElement("YearEmploymentTaxSum", 0),
                    new XElement("RemainingDaysPaid", 0),
                    new XElement("RemainingDaysUnpaid", 0),
                    new XElement("RemainingDaysAdvance", 0),
                    new XElement("RemainingDaysYear1", 0),
                    new XElement("RemainingDaysYear2", 0),
                    new XElement("RemainingDaysYear3", 0),
                    new XElement("RemainingDaysYear4", 0),
                    new XElement("RemainingDaysYear5", 0),
                    new XElement("RemainingDaysOverdue", 0),
                    new XElement("Acc1Name", 0),
                    new XElement("Acc2Name", 0),
                    new XElement("Acc3Name", 0),
                    new XElement("Acc1Value", 0),
                    new XElement("Acc2Value", 0),
                    new XElement("Acc3Value", 0),
                    new XElement("Acc1Sum", 0),
                    new XElement("Acc2Sum", 0),
                    new XElement("Acc3Sum", 0),
                    new XElement("VacationCoefficient", 0));

                    employeeElement.Add(new XElement("EmployeeEcom",
                        new XAttribute("Id", 1),
                        new XElement("EComType", 0),
                        new XElement("EComName", string.Empty),
                        new XElement("EComText", string.Empty),
                        new XElement("EComDescription", string.Empty)));

                    employeeElement.Add(new XElement("EmployeeAddress",
                        new XAttribute("Id", 1),
                        new XElement("AddressType", 0),
                        new XElement("AddressName", string.Empty),
                        new XElement("Address", string.Empty),
                        new XElement("AddressCO", string.Empty),
                        new XElement("AddressPostalCode", string.Empty),
                        new XElement("AddressPostalAddress", string.Empty),
                        new XElement("AddressCountry", string.Empty),
                        new XElement("AddressStreetAddress", string.Empty),
                        new XElement("AddressEntrenceCode", string.Empty)));

                    employeeElement.Add(new XElement("Transaction",
                        new XAttribute("Id", 1),
                        new XElement("ProductNr", string.Empty),
                        new XElement("ProductName", string.Empty),
                        new XElement("DateFrom", string.Empty),
                        new XElement("DateTo", string.Empty),
                        new XElement("Quantity", 0),
                        new XElement("IsTime", 0),
                        new XElement("UnitPrice", 0),
                        new XElement("Amount", 0),
                        new XElement("Note", string.Empty),
                        new XElement("QuantityWorkDays", 0),
                        new XElement("QuantityCalendarDays", 0),
                        new XElement("CalenderDayFactor", 0),
                        new XElement("TimeUnit", 0),
                        new XElement("TimeUnitName", string.Empty))
                        );

                    payrollSlipElement.Add(employeeElement);
                }

                #endregion
            }

            if (timePeriodXmlId == 1)
            {
                #region Default TimePeriod element

                XElement timePeriodElement = new XElement("TimePeriod",
                    new XAttribute("Id", 1),
                    new XElement("Name", string.Empty),
                    new XElement("StartDate", string.Empty),
                    new XElement("StopDate", string.Empty),
                    new XElement("PayrollStartDate", string.Empty),
                    new XElement("PayrollStopDate", string.Empty),
                    new XElement("PaymentDate", string.Empty),
                    new XElement("IsExtraPeriod", 0),
                    new XElement("IsPreliminary", 0));

                XElement employeeElement = new XElement("Employee",
                    new XAttribute("Id", 1),
                    new XElement("EmployeeNr", string.Empty),
                    new XElement("EmployeeFirstName", string.Empty),
                    new XElement("EmployeeLastName", string.Empty),
                    new XElement("EmployeeName", string.Empty),
                    new XElement("EmployeeSocialSec", string.Empty),
                    new XElement("DisbursementMethod", string.Empty),
                    new XElement("DisbursementClearingNr", string.Empty),
                    new XElement("DisbursementAccountNr", string.Empty),
                    new XElement("Note", string.Empty),
                    new XElement("PeriodGrossSum", 0),
                    new XElement("PeriodBenefitSum", 0),
                    new XElement("PeriodTaxSum", 0),
                    new XElement("PeriodCompensationSum", 0),
                    new XElement("PeriodDeductionSum", 0),
                    new XElement("PeriodNetSum", 0),
                    new XElement("PeriodEmploymentTaxSum", 0),
                    new XElement("YearGrossSum", 0),
                    new XElement("YearBenefitSum", 0),
                    new XElement("YearTaxSum", 0),
                    new XElement("YearCompensationSum", 0),
                    new XElement("YearDeductionSum", 0),
                    new XElement("YearNetSum", 0),
                    new XElement("YearEmploymentTaxSum", 0),
                    new XElement("RemainingDaysPaid", 0),
                    new XElement("RemainingDaysUnpaid", 0),
                    new XElement("RemainingDaysAdvance", 0),
                    new XElement("RemainingDaysYear1", 0),
                    new XElement("RemainingDaysYear2", 0),
                    new XElement("RemainingDaysYear3", 0),
                    new XElement("RemainingDaysYear4", 0),
                    new XElement("RemainingDaysYear5", 0),
                    new XElement("RemainingDaysOverdue", 0));


                employeeElement.Add(new XElement("EmployeeEcom",
            new XAttribute("Id", 1),
            new XElement("EComType", 0),
            new XElement("EComName", string.Empty),
            new XElement("EComText", string.Empty),
            new XElement("EComDescription", string.Empty)));

                employeeElement.Add(new XElement("EmployeeAddress",
            new XAttribute("Id", 1),
            new XElement("AddressType", 0),
            new XElement("AddressName", string.Empty),
            new XElement("Address", string.Empty),
            new XElement("AddressCO", string.Empty),
            new XElement("AddressPostalCode", string.Empty),
            new XElement("AddressPostalAddress", string.Empty),
            new XElement("AddressCountry", string.Empty),
            new XElement("AddressStreetAddress", string.Empty),
            new XElement("AddressEntrenceCode", string.Empty)));

                employeeElement.Add(new XElement("Transaction",
                    new XAttribute("Id", 1),
                    new XElement("ProductNr", string.Empty),
                    new XElement("ProductName", string.Empty),
                    new XElement("DateFrom", string.Empty),
                    new XElement("DateTo", string.Empty),
                    new XElement("Quantity", 0),
                    new XElement("IsTime", 0),
                    new XElement("UnitPrice", 0),
                    new XElement("Amount", 0),
                    new XElement("Note", string.Empty)));

                timePeriodElement.Add(employeeElement);
                payrollSlipElement.Add(timePeriodElement);

                #endregion
            }

            #endregion

            #region Close repository

            personalDataRepository.GenerateLogs();

            #endregion

            #region Close document

            rootElement.Add(payrollSlipElement);
            document.Add(rootElement);

            #endregion

            return document;
        }

        protected List<XElement> CreateSalesElementFromFrequency(CompEntities entities, int actorCompanyId, int roleId, DateTime dateFrom, DateTime dateTo, List<int> accountIds = null)
        {
            bool useAccountHierarchy = base.UseAccountHierarchyOnCompanyFromCache(entities, actorCompanyId);

            return CreateSalesElementFromFrequency(entities, actorCompanyId, roleId, dateFrom, dateTo, useAccountHierarchy, accountIds);
        }

        protected List<XElement> CreateSalesElementFromFrequency(CompEntities entities, int actorCompanyId, int roleId, DateTime dateFrom, DateTime dateTo, bool useAccountHierarchy, List<int> accountIds = null)
        {
            List<XElement> elements = new List<XElement>();
            int salesXmlId = 0;
            bool valid = true;
            dateTo = CalendarUtility.GetEndOfDay(dateTo);

            if (useAccountHierarchy)
            {
                AccountRepository accountRepository = AccountManager.GetAccountHierarchyRepositoryByUserSetting(entities, actorCompanyId, roleId, base.UserId, dateFrom, dateTo);
                List<AccountDTO> allAccountInternals = accountRepository?.GetAccounts() ?? new List<AccountDTO>();

                if (accountIds == null)
                    accountIds = allAccountInternals.Select(s => s.AccountId).ToList();
                else
                    accountIds = accountIds.Where(w => allAccountInternals.Select(s => s.AccountId).Contains(w)).ToList();

                if (accountIds.IsNullOrEmpty())
                    valid = false;
            }

            if (valid && FeatureManager.HasRolePermission(Feature.Time_Schedule_SchedulePlanning_Dashboard, Permission.Readonly, roleId, actorCompanyId, entities: entities))
            {
                var frequencies = base.GetStaffingNeedsFrequencyFromCache(entities, actorCompanyId, dateFrom, dateTo);
                if (!frequencies.IsNullOrEmpty())
                {
                    var accounts = AccountManager.GetAccountInternals(entities, actorCompanyId, true);
                    if (!accountIds.IsNullOrEmpty())
                        accounts = accounts.Where(w => accountIds.Contains(w.AccountId)).ToList();

                    if (accountIds != null)
                        frequencies = frequencies.Where(w => w.Amount != 0 && ((w.AccountId.HasValue && accountIds.Contains(w.AccountId.Value)) || (w.ParentAccountId.HasValue && accountIds.Contains(w.ParentAccountId.Value)))).ToList();

                    foreach (var accountFreq in frequencies.GroupBy(i => $"{i.AccountId}#{i.FrequencyType}#{i.TimeFrom.Date}"))
                    {
                        if (accountFreq == null)
                            continue;

                        var first = accountFreq.First();
                        var date = first.TimeFrom.Date;

                        var account = accounts.FirstOrDefault(f => f.Account.AccountId == accountFreq.First().AccountId);
                        if (account != null)
                        {
                            elements.Add(new XElement("Sales",
                                 new XAttribute("id", salesXmlId),
                                 new XElement("AccountNr", account.Account.AccountNr),
                                 new XElement("AccountName", account.Account.Name),
                                 new XElement("Amount", accountFreq.Sum(s => s.Amount)),
                                 new XElement("FrequencyType", first.FrequencyType),
                                 new XElement("Date", date),
                                 new XElement("Wk", CalendarUtility.GetWeekNr(date))));

                            salesXmlId++;
                        }
                        else
                        {
                            elements.Add(new XElement("Sales",
                                new XAttribute("id", salesXmlId),
                                new XElement("AccountNr", "Y"),
                                new XElement("AccountName", "Misc no matching account"),
                                new XElement("Amount", accountFreq.Sum(s => s.Amount)),
                                new XElement("FrequencyType", accountFreq.First().FrequencyType),
                                new XElement("Date", date),
                                new XElement("Wk", CalendarUtility.GetWeekNr(date))));
                            salesXmlId++;
                        }
                    }
                }
            }

            if (salesXmlId == 0)
            {
                elements.Add(new XElement("Sales",
                       new XAttribute("id", salesXmlId),
                       new XElement("AccountNr", ""),
                       new XElement("AccountName", ""),
                       new XElement("Amount", 0),
                       new XElement("FrequencyType", 0),
                       new XElement("Date", CalendarUtility.DATETIME_DEFAULT),
                       new XElement("Wk", 0)));
            }

            return elements;
        }

        protected List<XElement> CreateTimeEmployeeContactInformationElements(CompEntities entities, Contact contact)
        {
            #region EmployeeEcom element

            List<XElement> elements = new List<XElement>();

            int employeeEcomXmlId = 1;
            if (contact != null && contact.ContactECom != null)
            {
                foreach (var contactEComItem in contact.ContactECom)
                {
                    elements.Add(new XElement("EmployeeEcom",
                        new XAttribute("Id", employeeEcomXmlId),
                        new XElement("EComType", contactEComItem.SysContactEComTypeId),
                        new XElement("EComName", contactEComItem.Name.NullToEmpty()),
                        new XElement("EComText", contactEComItem.IsSecret ? "*" : contactEComItem.Text.NullToEmpty()),
                        new XElement("EComDescription", contactEComItem.IsSecret ? "*" : contactEComItem.Description.NullToEmpty())));

                    employeeEcomXmlId++;
                }
            }

            if (employeeEcomXmlId == 1)
            {
                elements.Add(new XElement("EmployeeEcom",
                    new XAttribute("Id", 1),
                    new XElement("EComType", 0),
                    new XElement("EComName", string.Empty),
                    new XElement("EComText", string.Empty),
                    new XElement("EComDescription", string.Empty)));
            }

            #endregion

            #region EmployeeAddress element

            int employeeAddressXmlId = 1;
            if (contact != null)
            {
                List<ContactAddress> contactAddresses = ContactManager.GetContactAddresses(entities, contact.ContactId);
                foreach (ContactAddress contactAddress in contactAddresses)
                {
                    #region Prereq

                    string address = "";
                    string addressCo = "";
                    string addressPostalCode = "";
                    string addressPostalAddress = "";
                    string addressCountry = "";
                    string addressStreetAddress = "";
                    string addressEntrenceCode = "";

                    if (contactAddress.ContactAddressRow != null && !contactAddress.IsSecret)
                    {
                        foreach (var contactAddressRow in contactAddress.ContactAddressRow)
                        {
                            if (contactAddressRow.SysContactAddressRowTypeId == (int)TermGroup_SysContactAddressRowType.Address)
                                address = contactAddressRow.Text;
                            if (contactAddressRow.SysContactAddressRowTypeId == (int)TermGroup_SysContactAddressRowType.AddressCO)
                                addressCo = contactAddressRow.Text;
                            if (contactAddressRow.SysContactAddressRowTypeId == (int)TermGroup_SysContactAddressRowType.PostalCode)
                                addressPostalCode = contactAddressRow.Text;
                            if (contactAddressRow.SysContactAddressRowTypeId == (int)TermGroup_SysContactAddressRowType.PostalAddress)
                                addressPostalAddress = contactAddressRow.Text;
                            if (contactAddressRow.SysContactAddressRowTypeId == (int)TermGroup_SysContactAddressRowType.Country)
                                addressCountry = contactAddressRow.Text;
                            if (contactAddressRow.SysContactAddressRowTypeId == (int)TermGroup_SysContactAddressRowType.StreetAddress)
                                addressStreetAddress = contactAddressRow.Text;
                            if (contactAddressRow.SysContactAddressRowTypeId == (int)TermGroup_SysContactAddressRowType.EntranceCode)
                                addressEntrenceCode = contactAddressRow.Text;
                        }
                    }

                    #endregion

                    elements.Add(new XElement("EmployeeAddress",
                        new XAttribute("Id", employeeAddressXmlId),
                        new XElement("AddressType", contactAddress.SysContactAddressTypeId),
                        new XElement("AddressName", contactAddress.Name),
                        new XElement("Address", address),
                        new XElement("AddressCO", addressCo),
                        new XElement("AddressPostalCode", addressPostalCode),
                        new XElement("AddressPostalAddress", addressPostalAddress),
                        new XElement("AddressCountry", addressCountry),
                        new XElement("AddressStreetAddress", addressStreetAddress),
                        new XElement("AddressEntrenceCode", addressEntrenceCode)));
                    employeeAddressXmlId++;
                }
            }

            if (employeeAddressXmlId == 1)
            {
                elements.Add(new XElement("EmployeeAddress",
                    new XAttribute("Id", 1),
                    new XElement("AddressType", 0),
                    new XElement("AddressName", string.Empty),
                    new XElement("Address", string.Empty),
                    new XElement("AddressCO", string.Empty),
                    new XElement("AddressPostalCode", string.Empty),
                    new XElement("AddressPostalAddress", string.Empty),
                    new XElement("AddressCountry", string.Empty),
                    new XElement("AddressStreetAddress", string.Empty),
                    new XElement("AddressEntrenceCode", string.Empty)));
            }

            #endregion

            return elements;
        }

        protected void AddEmployeeVacationSEElement(XElement parentElement, int employeeId, int actorCompanyId, bool includePrelUsedDays, DateTime? createdBefore = null, EmployeeVacationSE employeeVacationSE = null, Employee employee = null, List<EmployeeTimePeriod> employeeTimePeriods = null, DateTime? selectionDate = null)
        {
            employeeVacationSE = employeeVacationSE ?? EmployeeManager.GetLatestEmployeeVacationSE(employeeId, createdBefore);
            AddEmployeeVacationSEElement(parentElement, employeeId, actorCompanyId, includePrelUsedDays, employeeVacationSE, employee: employee, employeeTimePeriods: employeeTimePeriods, selectionDate: selectionDate);
        }

        protected void AddEmployeeVacationSEElement(XElement parentElement, int employeeId, int actorCompanyId, bool includePrelUsedDays, EmployeeVacationSE employeeVacationSE, Employee employee = null, List<EmployeeTimePeriod> employeeTimePeriods = null, DateTime? selectionDate = null)
        {
            decimal prelUsedDays = includePrelUsedDays ? PayrollManager.GetEmployeeVacationPrelUsedDays(actorCompanyId, employeeId, DateTime.Today, employee: employee, employeeTimePeriods: employeeTimePeriods, lastEmployeeVacation: employeeVacationSE).Sum : 0;

            XElement employeeVacationSEElement = new XElement("EmployeeVacationSE",
                new XAttribute("Id", 1),
            new XElement("EmployeeVacationSEId", (employeeVacationSE != null ? employeeVacationSE.EmployeeVacationSEId : 0)),
            new XElement("EarnedDaysPaid", (employeeVacationSE != null ? employeeVacationSE.EarnedDaysPaid ?? 0 : 0)),
            new XElement("EarnedDaysUnpaid", (employeeVacationSE != null ? employeeVacationSE.EarnedDaysUnpaid ?? 0 : 0)),
            new XElement("EarnedDaysAdvance", (employeeVacationSE != null ? employeeVacationSE.EarnedDaysAdvance ?? 0 : 0)),
            new XElement("SavedDaysYear1", (employeeVacationSE != null ? employeeVacationSE.SavedDaysYear1 ?? 0 : 0)),
            new XElement("SavedDaysYear2", (employeeVacationSE != null ? employeeVacationSE.SavedDaysYear2 ?? 0 : 0)),
            new XElement("SavedDaysYear3", (employeeVacationSE != null ? employeeVacationSE.SavedDaysYear3 ?? 0 : 0)),
            new XElement("SavedDaysYear4", (employeeVacationSE != null ? employeeVacationSE.SavedDaysYear4 ?? 0 : 0)),
            new XElement("SavedDaysYear5", (employeeVacationSE != null ? employeeVacationSE.SavedDaysYear5 ?? 0 : 0)),
            new XElement("SavedDaysOverdue", (employeeVacationSE != null ? employeeVacationSE.SavedDaysOverdue ?? 0 : 0)),
            new XElement("UsedDaysPaid", (employeeVacationSE != null ? employeeVacationSE.UsedDaysPaid ?? 0 : 0)),
            new XElement("PaidVacationAllowance", (employeeVacationSE != null ? employeeVacationSE.PaidVacationAllowance ?? 0 : 0)),
            new XElement("PaidVacationVariableAllowance", (employeeVacationSE != null ? employeeVacationSE.PaidVacationVariableAllowance ?? 0 : 0)),
            new XElement("UsedDaysUnpaid", (employeeVacationSE != null ? employeeVacationSE.UsedDaysUnpaid ?? 0 : 0)),
            new XElement("UsedDaysAdvance", (employeeVacationSE != null ? employeeVacationSE.UsedDaysAdvance ?? 0 : 0)),
            new XElement("UsedDaysYear1", (employeeVacationSE != null ? employeeVacationSE.UsedDaysYear1 ?? 0 : 0)),
            new XElement("UsedDaysYear2", (employeeVacationSE != null ? employeeVacationSE.UsedDaysYear2 ?? 0 : 0)),
            new XElement("UsedDaysYear3", (employeeVacationSE != null ? employeeVacationSE.UsedDaysYear3 ?? 0 : 0)),
            new XElement("UsedDaysYear4", (employeeVacationSE != null ? employeeVacationSE.UsedDaysYear4 ?? 0 : 0)),
            new XElement("UsedDaysYear5", (employeeVacationSE != null ? employeeVacationSE.UsedDaysYear5 ?? 0 : 0)),
            new XElement("UsedDaysOverdue", (employeeVacationSE != null ? employeeVacationSE.UsedDaysOverdue ?? 0 : 0)),
            new XElement("RemainingDaysPaid", (employeeVacationSE != null ? employeeVacationSE.RemainingDaysPaid ?? 0 : 0)),
            new XElement("RemainingDaysUnpaid", (employeeVacationSE != null ? employeeVacationSE.RemainingDaysUnpaid ?? 0 : 0)),
            new XElement("RemainingDaysAdvance", (employeeVacationSE != null ? employeeVacationSE.RemainingDaysAdvance ?? 0 : 0)),
            new XElement("RemainingDaysYear1", (employeeVacationSE != null ? employeeVacationSE.RemainingDaysYear1 ?? 0 : 0)),
            new XElement("RemainingDaysYear2", (employeeVacationSE != null ? employeeVacationSE.RemainingDaysYear2 ?? 0 : 0)),
            new XElement("RemainingDaysYear3", (employeeVacationSE != null ? employeeVacationSE.RemainingDaysYear3 ?? 0 : 0)),
            new XElement("RemainingDaysYear4", (employeeVacationSE != null ? employeeVacationSE.RemainingDaysYear4 ?? 0 : 0)),
            new XElement("RemainingDaysYear5", (employeeVacationSE != null ? employeeVacationSE.RemainingDaysYear5 ?? 0 : 0)),
            new XElement("RemainingDaysOverdue", (employeeVacationSE != null ? employeeVacationSE.RemainingDaysOverdue ?? 0 : 0)),
            new XElement("EarnedDaysRemainingHoursPaid", (employeeVacationSE != null ? employeeVacationSE.EarnedDaysRemainingHoursPaid ?? 0 : 0)),
            new XElement("EarnedDaysRemainingHoursUnpaid", (employeeVacationSE != null ? employeeVacationSE.EarnedDaysRemainingHoursUnpaid ?? 0 : 0)),
            new XElement("EarnedDaysRemainingHoursAdvance", (employeeVacationSE != null ? employeeVacationSE.EarnedDaysRemainingHoursAdvance ?? 0 : 0)),
            new XElement("EarnedDaysRemainingHoursYear1", (employeeVacationSE != null ? employeeVacationSE.EarnedDaysRemainingHoursYear1 ?? 0 : 0)),
            new XElement("EarnedDaysRemainingHoursYear2", (employeeVacationSE != null ? employeeVacationSE.EarnedDaysRemainingHoursYear2 ?? 0 : 0)),
            new XElement("EarnedDaysRemainingHoursYear3", (employeeVacationSE != null ? employeeVacationSE.EarnedDaysRemainingHoursYear3 ?? 0 : 0)),
            new XElement("EarnedDaysRemainingHoursYear4", (employeeVacationSE != null ? employeeVacationSE.EarnedDaysRemainingHoursYear4 ?? 0 : 0)),
            new XElement("EarnedDaysRemainingHoursYear5", (employeeVacationSE != null ? employeeVacationSE.EarnedDaysRemainingHoursYear5 ?? 0 : 0)),
            new XElement("EarnedDaysRemainingHoursOverdue", (employeeVacationSE != null ? employeeVacationSE.EarnedDaysRemainingHoursOverdue ?? 0 : 0)),
            new XElement("EmploymentRatePaid", (employeeVacationSE != null ? employeeVacationSE.EmploymentRatePaid ?? 0 : 0)),
            new XElement("EmploymentRateYear1", (employeeVacationSE != null ? employeeVacationSE.EmploymentRateYear1 ?? 0 : 1)),
            new XElement("EmploymentRateYear2", (employeeVacationSE != null ? employeeVacationSE.EmploymentRateYear2 ?? 0 : 1)),
            new XElement("EmploymentRateYear3", (employeeVacationSE != null ? employeeVacationSE.EmploymentRateYear3 ?? 0 : 1)),
            new XElement("EmploymentRateYear4", (employeeVacationSE != null ? employeeVacationSE.EmploymentRateYear4 ?? 0 : 1)),
            new XElement("EmploymentRateYear5", (employeeVacationSE != null ? employeeVacationSE.EmploymentRateYear5 ?? 0 : 1)),
            new XElement("EmploymentRateOverdue", (employeeVacationSE != null ? employeeVacationSE.EmploymentRateOverdue ?? 0 : 1)),
            new XElement("DebtInAdvanceAmount", (employeeVacationSE != null && (!employeeVacationSE.DebtInAdvanceDueDate.HasValue || employeeVacationSE.DebtInAdvanceDueDate.Value > selectionDate) ? employeeVacationSE.DebtInAdvanceAmount ?? 0 : 0)),
            new XElement("DebtInAdvanceDueDate", (employeeVacationSE != null && employeeVacationSE.DebtInAdvanceDueDate.HasValue ? employeeVacationSE.DebtInAdvanceDueDate.Value.ToShortDateString() : String.Empty)),
            new XElement("DebtInAdvanceDelete", employeeVacationSE != null ? employeeVacationSE.DebtInAdvanceDelete.ToInt() : 0),
            new XElement("PrelUsedDays", prelUsedDays),
            new XElement("EmployeeVacationSECreated", (employeeVacationSE != null && employeeVacationSE.Created.HasValue ? employeeVacationSE.Created.Value : CalendarUtility.DATETIME_DEFAULT)),
            new XElement("EmployeeVacationSECreatedBy", (employeeVacationSE != null ? employeeVacationSE.CreatedBy : String.Empty)),
            new XElement("EmployeeVacationSEModified", (employeeVacationSE != null && employeeVacationSE.Modified.HasValue ? employeeVacationSE.Modified.Value : CalendarUtility.DATETIME_DEFAULT)),
            new XElement("EmployeeVacationSEModifiedBy", (employeeVacationSE != null ? employeeVacationSE.ModifiedBy : String.Empty)));
            parentElement.Add(employeeVacationSEElement);
        }

        protected void AddEmployeeVacationSEDefaultElement(XElement parentElement)
        {
            XElement employeeVacationSEElement = new XElement("EmployeeVacationSE",
                new XAttribute("Id", 1),
                new XElement("EmployeeVacationSEId", 0),
                new XElement("EarnedDaysPaid", 0),
                new XElement("EarnedDaysUnpaid", 0),
                new XElement("EarnedDaysAdvance", 0),
                new XElement("SavedDaysYear1", 0),
                new XElement("SavedDaysYear2", 0),
                new XElement("SavedDaysYear3", 0),
                new XElement("SavedDaysYear4", 0),
                new XElement("SavedDaysYear5", 0),
                new XElement("SavedDaysOverdue", 0),
                new XElement("UsedDaysPaid", 0),
                new XElement("PaidVacationAllowance", 0),
                new XElement("PaidVacationVariableAllowance", 0),
                new XElement("UsedDaysUnpaid", 0),
                new XElement("UsedDaysAdvance", 0),
                new XElement("UsedDaysYear1", 0),
                new XElement("UsedDaysYear2", 0),
                new XElement("UsedDaysYear3", 0),
                new XElement("UsedDaysYear4", 0),
                new XElement("UsedDaysYear5", 0),
                new XElement("UsedDaysOverdue", 0),
                new XElement("RemainingDaysPaid", 0),
                new XElement("RemainingDaysUnpaid", 0),
                new XElement("RemainingDaysAdvance", 0),
                new XElement("RemainingDaysYear1", 0),
                new XElement("RemainingDaysYear2", 0),
                new XElement("RemainingDaysYear3", 0),
                new XElement("RemainingDaysYear4", 0),
                new XElement("RemainingDaysYear5", 0),
                new XElement("RemainingDaysOverdue", 0),
                new XElement("EarnedDaysRemainingHoursPaid", 0),
                new XElement("EarnedDaysRemainingHoursUnpaid", 0),
                new XElement("EarnedDaysRemainingHoursAdvance", 0),
                new XElement("EarnedDaysRemainingHoursYear1", 0),
                new XElement("EarnedDaysRemainingHoursYear2", 0),
                new XElement("EarnedDaysRemainingHoursYear3", 0),
                new XElement("EarnedDaysRemainingHoursYear4", 0),
                new XElement("EarnedDaysRemainingHoursYear5", 0),
                new XElement("EarnedDaysRemainingHoursOverdue", 0),
                new XElement("EmploymentRatePaid", 0),
                new XElement("EmploymentRateYear1", 0),
                new XElement("EmploymentRateYear2", 0),
                new XElement("EmploymentRateYear3", 0),
                new XElement("EmploymentRateYear4", 0),
                new XElement("EmploymentRateYear5", 0),
                new XElement("EmploymentRateOverdue", 0),
                new XElement("DebtInAdvanceAmount", 0),
                new XElement("DebtInAdvanceDueDate", ""),
                new XElement("DebtInAdvanceDelete", 0),
                new XElement("PrelUsedDays", 0),
                new XElement("Created", ""),
                new XElement("CreatedBy", ""),
                new XElement("Modified", ""),
                new XElement("ModifiedBy", ""));
            parentElement.Add(employeeVacationSEElement);
        }

        protected void AddEmployeeAccountsElement(XElement parentElement, Employee employee, DateTime date)
        {
            #region Accounts

            #region CostAccount element
            int employeeId = employee?.EmployeeId ?? 0;
            int costAccountXmlId = 1;

            var employment = employee.GetEmployment(date);

            EmploymentAccountStd costEmployeeAccount = null;

            if (employment != null)
            {
                if (!employment.FixedAccounting)
                    costEmployeeAccount = EmployeeManager.GetEmploymentAccount(employment.EmploymentId, EmploymentAccountType.Cost, date);
                else
                    costEmployeeAccount = EmployeeManager.GetEmploymentAccount(employment.EmploymentId, EmploymentAccountType.Fixed1, date);
            }

            if (costEmployeeAccount != null)
            {
                #region Prereq

                if (!costEmployeeAccount.AccountStdReference.IsLoaded)
                    costEmployeeAccount.AccountStdReference.Load();

                if (costEmployeeAccount.AccountStd != null && !costEmployeeAccount.AccountStd.AccountReference.IsLoaded)
                    costEmployeeAccount.AccountStd.AccountReference.Load();

                #endregion

                #region CostAccount element

                XElement costAccount;
                if (costEmployeeAccount.AccountStd != null)
                {
                    costAccount = new XElement("CostAccount",
                        new XAttribute("Id", costAccountXmlId),
                        new XElement("CostAccountNr", costEmployeeAccount.AccountStd.Account.AccountNr),
                        new XElement("CostAccountName", costEmployeeAccount.AccountStd.Account.Name));
                }
                else
                {
                    costAccount = new XElement("CostAccount",
                        new XAttribute("Id", 1),
                        new XElement("CostAccountNr", String.Empty),
                        new XElement("CostAccountName", String.Empty));
                }
                costAccountXmlId++;
                parentElement.Add(costAccount);

                #region InternalCostAccounts element

                int internalCostAccountXmlId = 1;
                foreach (var accountInternal in costEmployeeAccount.AccountInternal)
                {
                    #region Prereq

                    if (!accountInternal.AccountReference.IsLoaded)
                        accountInternal.AccountReference.Load();

                    #endregion

                    XElement internalaccount = new XElement("InternalCostAccounts",
                        new XAttribute("Id", internalCostAccountXmlId),
                        new XElement("CostInternalAccountNr", accountInternal.Account.AccountNr),
                        new XElement("CostInternalAccountName", accountInternal.Account.Name),
                        new XElement("CostSieDimNr", accountInternal.Account.AccountDim.SysSieDimNr.HasValue ? accountInternal.Account.AccountDim.SysSieDimNr.Value : 0),
                        new XElement("CostDimNr", accountInternal.Account.AccountDim.AccountDimNr));

                    costAccount.Add(internalaccount);
                    internalCostAccountXmlId++;
                }

                if (internalCostAccountXmlId == 1)
                {
                    XElement internalaccount = new XElement("InternalCostAccounts",
                        new XAttribute("Id", 1),
                        new XElement("CostInternalAccountNr", String.Empty),
                        new XElement("CostInternalAccountName", String.Empty),
                        new XElement("CostSieDimNr", 0),
                        new XElement("CostDimNr", 0));

                    costAccount.Add(internalaccount);
                }

                #endregion

                #endregion
            }
            if (costAccountXmlId == 1)
            {
                XElement costAccount = new XElement("CostAccount",
                       new XAttribute("Id", 1),
                       new XElement("CostAccountNr", String.Empty),
                       new XElement("CostAccountName", String.Empty));

                XElement internalaccount = new XElement("InternalCostAccounts",
                          new XAttribute("Id", 1),
                          new XElement("CostInternalAccountNr", String.Empty),
                          new XElement("CostInternalAccountName", String.Empty),
                          new XElement("CostSieDimNr", 0),
                          new XElement("CostDimNr", 0));

                costAccount.Add(internalaccount);
                parentElement.Add(costAccount);
            }

            #endregion

            #region RevenueAccount element

            int revenueAccountXmlId = 1;
            var revenueEmployeeAccount = EmployeeManager.GetEmploymentAccountFromEmployeeWithDim(employeeId, EmploymentAccountType.Income, date);
            if (revenueEmployeeAccount != null)
            {
                #region Prereq

                if (!revenueEmployeeAccount.AccountStdReference.IsLoaded)
                    revenueEmployeeAccount.AccountStdReference.Load();

                if (revenueEmployeeAccount.AccountStd != null && !revenueEmployeeAccount.AccountStd.AccountReference.IsLoaded)
                    revenueEmployeeAccount.AccountStd.AccountReference.Load();

                #endregion

                #region RevenueAccount element

                if (revenueEmployeeAccount.AccountStd != null)
                {
                    XElement revenueAccount = new XElement("RevenueAccount",
                        new XAttribute("Id", revenueAccountXmlId),
                        new XElement("RevenueAccountNr", revenueEmployeeAccount.AccountStd.Account.AccountNr),
                        new XElement("RevenueAccountName", revenueEmployeeAccount.AccountStd.Account.Name));

                    #region InternalRevenueAccounts element

                    int internalRevenueAccountXmlId = 1;
                    foreach (var accountInternal in revenueEmployeeAccount.AccountInternal)
                    {
                        #region Prereq

                        if (!accountInternal.AccountReference.IsLoaded)
                            accountInternal.AccountReference.Load();

                        #endregion

                        XElement internalAccountRevenue = new XElement("InternalRevenueAccounts",
                            new XAttribute("Id", internalRevenueAccountXmlId),
                            new XElement("RevenueInternalAccountNr", accountInternal.Account.AccountNr),
                            new XElement("RevenueInternalAccountName", accountInternal.Account.Name),
                            new XElement("RevenueSieDimNr", accountInternal.Account.AccountDim.SysSieDimNr.HasValue ? accountInternal.Account.AccountDim.SysSieDimNr.Value : 0),
                            new XElement("RevenueDimNr", accountInternal.Account.AccountDim.AccountDimNr));

                        revenueAccount.Add(internalAccountRevenue);
                        internalRevenueAccountXmlId++;
                    }

                    #endregion

                    parentElement.Add(revenueAccount);
                    revenueAccountXmlId++;
                }

                if (revenueAccountXmlId == 1)
                {
                    XElement revenueAccount = new XElement("RevenueAccount",
                        new XAttribute("Id", 1),
                        new XElement("RevenueAccountNr", String.Empty),
                        new XElement("RevenueAccountName", String.Empty));

                    XElement internalAccountRevenue = new XElement("InternalRevenueAccounts",
                        new XAttribute("Id", 1),
                        new XElement("RevenueInternalAccountNr", String.Empty),
                        new XElement("RevenueInternalAccountName", String.Empty),
                        new XElement("RevenueSieDimNr", 0),
                        new XElement("RevenueDimNr", 0));
                    revenueAccountXmlId++;
                    revenueAccount.Add(internalAccountRevenue);
                    parentElement.Add(revenueAccount);
                }

                #endregion
            }
            if (revenueAccountXmlId == 1)
            {
                XElement revenueAccount = new XElement("RevenueAccount",
                    new XAttribute("Id", 1),
                    new XElement("RevenueAccountNr", String.Empty),
                    new XElement("RevenueAccountName", String.Empty));

                XElement internalAccountRevenue = new XElement("InternalRevenueAccounts",
                    new XAttribute("Id", 1),
                    new XElement("RevenueInternalAccountNr", String.Empty),
                    new XElement("RevenueInternalAccountName", String.Empty),
                    new XElement("RevenueSieDimNr", 0),
                    new XElement("RevenueDimNr", 0));

                revenueAccount.Add(internalAccountRevenue);
                parentElement.Add(revenueAccount);
            }

            #endregion

            #endregion
        }

        protected void AddEmploymentEndReasonInfo(XElement parentElement, Employment employment, DateTime date, Dictionary<int, string> endReasonsDict)
        {
            if (parentElement == null)
                return;

            int endReason = employment != null ? employment.GetEndReason(date) : 0;
            string endReasonName = endReasonsDict != null && endReasonsDict.ContainsKey(endReason) ? endReasonsDict[endReason] : String.Empty;

            parentElement.Add(new XElement("EndReason", employment != null ? employment.GetEndReason(date) : 0));
            parentElement.Add(new XElement("EndReasonName", endReasonName));
        }

        #endregion

        #endregion

        #region Texts

        protected string GetReportText(int sysTermId, string defaultTerm)
        {
            return base.GetText(sysTermId, (int)TermGroup.Report, defaultTerm);
        }

        protected string GetDateIntervalText(EvaluatedSelection es)
        {
            string text = "";
            if (es != null && es.HasDateInterval)
            {
                if (es.DateFrom != CalendarUtility.DATETIME_MINVALUE)
                    text += es.DateFrom.ToShortDateString();
                text += "-";
                if (es.DateTo != CalendarUtility.DATETIME_MAXVALUE)
                    text += es.DateTo.ToShortDateString();
            }
            return text;
        }

        protected string GetStandardEmployeeIntervalText(EvaluatedSelection es)
        {
            string text = "";
            if (es.SB_HasEmployeeNrInterval)
            {
                text = es.SB_EmployeeNrFrom + "-" + es.SB_EmployeeNrTo;
            }
            return text;
        }

        protected string GetStandardCustomerIntervalText(EvaluatedSelection es)
        {
            string text = "";
            if (es.SB_HasCustomerNrInterval)
            {
                text = es.SB_CustomerNrFrom + "-" + es.SB_CustomerNrTo;
            }
            return text;
        }

        protected string GetStandardProjectIntervalText(EvaluatedSelection es)
        {
            string text = "";
            if (es.SB_HasProjectNrInterval)
            {
                text = es.SB_ProjectNrFrom + "-" + es.SB_ProjectNrTo;
            }
            return text;
        }

        protected string GetCompanyLogoFilePath(CompEntities entities, int actorCompanyId, bool refreshIfExists)
        {
            string path = LogoManager.GetCompanyLogoFilePath(entities, actorCompanyId, refreshIfExists);
            if (this.useCrGen)
                return AddToCrGenRequestPicturesDTO(path);
            return path;
        }

        #region ERP

        protected void GetDebetCredit(decimal amount, out decimal debet, out decimal credit)
        {
            debet = Decimal.Zero;
            credit = Decimal.Zero;
            if (amount != 0)
            {
                if (amount > 0)
                    debet = amount;
                else
                    credit = amount;
            }
        }

        /// <summary>
        /// Get AccountYear interval, if selection contains one
        /// </summary>
        /// <param name="es">The EvaluatedSelection</param>
        /// <returns>The AccountYear interval in format yyyyMM (ex: 200801-200812)</returns>
        protected string GetAccountYearIntervalText(EvaluatedSelection es)
        {
            string text = "";
            if (es.SSTD_HasAccountYearText)
                text = es.SSTD_AccountYearFromText + "-" + es.SSTD_AccountYearToText;
            return text;
        }

        /// <summary>
        /// Get AccountYear interval as date, if selection contains one
        /// </summary>
        /// <param name="es">The EvaluatedSelection</param>
        /// <returns>The AccountYear interval in format yyyyMMDD (ex: 20140101-20141231)</returns>
        protected string GetLongAccountYearIntervalText(EvaluatedSelection es)
        {
            string text = "";
            if (es.SSTD_HasAccountYearText)
                text = es.SSTD_LongAccountYearFromText + "-" + es.SSTD_LongAccountYearToText;
            return text;
        }

        /// <summary>
        /// Get PreviousAccountYear interval as date, if selection contains one
        /// </summary>
        /// <param name="es">The EvaluatedSelection</param>
        /// <returns>The PreviousAccountYear interval in format yyyyMMDD (ex: 20140101-20141231)</returns>
        protected string GetPreviousAccountYearIntervalText(EvaluatedSelection es)
        {
            string text = "";
            if (es.SSTD_HasAccountYearText)
                text = es.SSTD_PreviousAccountYearFromText + "-" + es.SSTD_PreviousAccountYearToText;
            return text;
        }

        /// <summary>
        /// Get AccountPeriod interval, if selection contains one
        /// </summary>
        /// <param name="es">The EvaluatedSelection</param>
        /// <returns>The AccountPeriod interval in format yyyyMM (ex: 200801-200812)</returns>
        protected string GetAccountPeriodIntervalText(EvaluatedSelection es)
        {
            string text = "";
            if (es.HasDateInterval)
                text = es.DateFrom.ToShortDateString() + "-" + es.DateTo.ToShortDateString();
            else if (es.SSTD_HasAccountPeriodText)
                text = es.SSTD_AccountPeriodFromText + "-" + es.SSTD_AccountPeriodToText;
            return text;
        }

        /// <summary>
        /// Get the latest VoucherNr for the VoucherSeries if the selection contains one AccountYear and one VoucherSeriesType
        /// (i.e. not interval)
        /// 
        /// </summary>
        /// <param name="es">The EvaluatedSelection</param>
        /// <returns>The VoucherNrLatest from VoucherSeries</returns>
        protected string GetLatestVoucherText(EvaluatedSelection es)
        {
            string text = "";
            if (es.SSTD_IsSameYear && es.SV_HasVoucherSeriesTypeNrInterval && es.SV_VoucherSeriesTypeNrFrom == es.SV_VoucherSeriesTypeNrTo)
            {
                VoucherSeries voucherSerie = VoucherManager.GetVoucherSerieByTypeNr(es.SV_VoucherSeriesTypeNrFrom, es.SSTD_AccountYearId);
                if (voucherSerie != null)
                    text = voucherSerie.VoucherNrLatest.ToString();
            }
            return text;
        }

        /// <summary>
        /// Get VoucherSerie interval, if selection contains one
        /// </summary>
        /// <param name="es">The EvaluatedSelection</param>
        /// <returns>The VoucherSerie interval</returns>
        protected string GetVoucherSerieIntervalText(EvaluatedSelection es)
        {
            string text = "";
            if (es.SV_HasVoucherSeriesTypeNrInterval)
                text = es.SV_VoucherSeriesTypeNrFrom.ToString() + "-" + es.SV_VoucherSeriesTypeNrTo.ToString();
            return text;
        }

        protected string GetAccountIntervalText(EvaluatedSelection es, List<AccountDimDTO> accountDims)
        {
            if (!es.SA_HasAccountInterval || es.SA_AccountIntervals == null || accountDims == null)
                return string.Empty;

            StringBuilder sb = new StringBuilder();

            foreach (AccountIntervalDTO accountInterval in es.SA_AccountIntervals)
            {
                if (accountDims.Any(i => i.AccountDimId == accountInterval.AccountDimId))
                    sb.Append(accountInterval.AccountNrFrom + "-" + accountInterval.AccountNrTo + " ");
            }

            foreach (AccountDimDTO accountDim in accountDims)
            {
                bool first = true;
                foreach (AccountIntervalDTO accountInterval in es.SA_AccountIntervals)
                {
                    if (accountInterval.AccountDimId == accountDim.AccountDimId)
                    {
                        if (first)
                        {
                            if (sb.Length > 0)
                                sb.Append("; ");
                            sb.Append(accountDim.Name + ": ");
                            first = false;
                        }
                        if (accountInterval.AccountNrFrom == accountInterval.AccountNrTo)
                        {
                            if (accountInterval.AccountNrFrom == " ")
                            {
                                sb.Append(" ");
                            }
                            else
                            {
                                Account accountFrom = AccountManager.GetAccountByNr(accountInterval.AccountNrFrom, accountDim.AccountDimId, es.ActorCompanyId, onlyActive: false);
                                if (accountFrom != null)
                                    sb.Append($"{accountInterval.AccountNrFrom} {accountFrom.Name}");
                            }
                        }
                        else
                        {
                            Account accountFrom = AccountManager.GetAccountByNr(accountInterval.AccountNrFrom, accountDim.AccountDimId, es.ActorCompanyId, onlyActive: false);
                            if (accountFrom == null)
                                sb.Append(accountInterval.AccountNrFrom);
                            else
                                sb.Append($"{accountInterval.AccountNrFrom} {accountFrom.Name}");

                            Account accountTo = AccountManager.GetAccountByNr(accountInterval.AccountNrTo, accountDim.AccountDimId, es.ActorCompanyId, onlyActive: false);
                            if (accountTo == null)
                                sb.Append($" - {accountInterval.AccountNrTo}");
                            else
                                sb.Append($" - {accountInterval.AccountNrTo} {accountTo.Name}");
                        }
                    }
                }
            }

            return sb.ToString();
        }

        protected string GetAccountDimText(EvaluatedSelection es, List<AccountDimDTO> dim)
        {
            StringBuilder sb = new StringBuilder();

            if (es.SSTD_AccountDimId > 0)
            {
                foreach (AccountDimDTO d in dim)
                {
                    if (es.SSTD_AccountDimId == d.AccountDimId)
                        sb.Append($"{d.AccountDimNr - 1} {d.SysSieDimNr} {d.Name}");
                }
            }

            return sb.ToString();
        }

        protected string GetInventoryIntervalText(EvaluatedSelection es)
        {
            string text = "";
            if (es.SFA_HasInventoryInterval)
                text = es.SFA_InventoryFrom.ToString() + "-" + es.SFA_InventoryTo.ToString();
            return text;
        }

        protected string GetCategoryIntervalText(EvaluatedSelection es)
        {
            string text = "";
            if (es.SFA_HasCategoryInterval)
            {
                if (es.SFA_CategoryFrom != "")
                {
                    text = es.SFA_CategoryFrom;
                }
                if (es.SFA_CategoryTo != "")
                {
                    text += " - ";
                    text += es.SFA_CategoryTo;
                }
            }
            return text;
        }

        protected string GetPrognoseIntervalText(string Interval1From, string Interval1To)
        {
            string text = "";


            if (Interval1From != "")
            {
                text = Interval1From;
            }
            if (Interval1To != "")
            {
                text += "-";
                text += Interval1To;
            }
            return text;
        }

        protected string GetVoucherIntervalText(EvaluatedSelection es)
        {
            string text = "";
            if (es.SV_HasVoucherNrInterval)
                text = es.SV_VoucherNrFrom.ToString() + "-" + es.SV_VoucherNrTo.ToString();
            return text;
        }

        protected string GetLedgerInvoiceIntervalText(EvaluatedSelection es)
        {
            string text = "";
            if (es.SL_HasInvoiceSeqNrInterval)
                text = es.SL_InvoiceSeqNrFrom + "-" + es.SL_InvoiceSeqNrTo;
            else if (es.SL_HasInvoiceIds)
                using (var entities = new CompEntities())
                {
                    foreach (int invoiceId in es.SL_InvoiceIds)
                    {
                        var invoiceTinyDTO = SupplierInvoiceManager.GetSupplierInvoiceTiny(entities, invoiceId);
                        if (invoiceTinyDTO != null && invoiceTinyDTO.SeqNr.HasValue)
                        {
                            if (string.IsNullOrEmpty(text))
                            {
                                text = invoiceTinyDTO.SeqNr.ToString();
                            }
                            else
                            {
                                text = text + "," + invoiceTinyDTO.SeqNr.ToString();
                            }
                        }
                    }
                }
            return text;
        }

        protected string GetLedgerActorIntervalText(EvaluatedSelection es)
        {
            string text = "";
            if (es.SL_HasActorNrInterval)
                text = es.SL_ActorNrFrom + "-" + es.SL_ActorNrTo;
            return text;
        }

        protected string GetLedgerDateRegardText(EvaluatedSelection es)
        {
            return GetText(es.SL_DateRegard, (int)TermGroup.ReportLedgerDateRegard);
        }

        protected string GetLedgerInvoiceSelectionText(EvaluatedSelection es)
        {
            return GetText(es.SL_InvoiceSelection, (int)TermGroup.ReportLedgerInvoiceSelection);
        }

        protected string GetSupplierSortOrderText(EvaluatedSelection es)
        {
            return GetText(es.SL_SortOrder, (int)TermGroup.ReportSupplierLedgerSortOrder);
        }

        protected string GetCustomerSortOrderText(EvaluatedSelection es)
        {
            return GetText(es.SL_SortOrder, (int)TermGroup.ReportCustomerLedgerSortOrder);
        }

        protected string GetBillingInvoiceIntervalText(EvaluatedSelection es)
        {
            string text = "";
            if (es.SB_HasInvoiceNrInterval)
                text = es.SB_InvoiceNrFrom + "-" + es.SB_InvoiceNrTo;
            else if (es.SB_HasInvoiceIds)
                text = StringUtility.GetCommaSeparatedString<int>(es.SB_InvoiceIds);
            return text;
        }

        protected string GetBillingCustomerIntervalText(EvaluatedSelection es)
        {
            string text = "";
            if (es.SB_HasCustomerNrInterval)
                text = es.SB_CustomerNrFrom + "-" + es.SB_CustomerNrTo;
            return text;
        }

        protected string GetBillingEmployeeIntervalText(EvaluatedSelection es)
        {
            string text = "";
            if (es.SB_HasEmployeeNrInterval)
                text = es.SB_EmployeeNrFrom + "-" + es.SB_EmployeeNrTo;
            return text;
        }

        protected string GetBillingProjectIntervalText(EvaluatedSelection es)
        {
            string text = "";
            if (es.SB_HasProjectNrInterval)
                text = es.SB_ProjectNrFrom + "-" + es.SB_ProjectNrTo;
            return text;
        }

        protected string GetBillingDateIntervalText(EvaluatedSelection es)
        {
            string text = "";
            if (es.HasDateInterval)
            {
                if (es.DateFrom != CalendarUtility.DATETIME_MINVALUE)
                    text += es.DateFrom.ToShortDateString();
                text += "-";
                if (es.DateTo != CalendarUtility.DATETIME_MAXVALUE)
                    text += es.DateTo.ToShortDateString();
            }
            return text;
        }

        protected string GetBillingCreatedDateIntervalText(EvaluatedSelection es)
        {
            string text = "";
            return text;
        }

        protected string GetBillingPaymentDateIntervalText(EvaluatedSelection es)
        {
            string text = "";
            if (es.SB_HasPaymentDateInterval)
            {
                if (es.SB_PaymentDateFrom != CalendarUtility.DATETIME_MINVALUE)
                    text += es.SB_PaymentDateFrom.ToShortDateString();
                text += "-";
                if (es.SB_PaymentDateTo != CalendarUtility.DATETIME_MAXVALUE)
                    text += es.SB_PaymentDateTo.ToShortDateString();
            }
            return text;
        }

        protected string GetBillingInvoiceSortOrderText(EvaluatedSelection es, int termGroup)
        {
            return GetText(es.SB_SortOrder, termGroup);
        }

        protected string GetInventoryPrognoseText(EvaluatedSelection es, int termGroup)
        {
            return GetText(es.SFA_PrognoseType, termGroup);
        }
        protected string CreateStockIntervalText(EvaluatedSelection es)
        {
            string stockFromText = String.Empty;
            if (es.SB_StockLocationIdFrom > 0)
            {
                Stock stockFrom = StockManager.GetStock(es.SB_StockLocationIdFrom);
                stockFromText = stockFrom.Name;
            }
            string stockToText = String.Empty;
            if (es.SB_StockLocationIdTo > 0)
            {
                Stock stockTo = StockManager.GetStock(es.SB_StockLocationIdTo);
                stockToText = stockTo.Name;
            }
            return stockFromText + " - " + stockToText;
        }

        protected string CreateStockShelfIntervalText(EvaluatedSelection es)
        {
            string shelfFromText = String.Empty;
            if (es.SB_StockShelfIdFrom > 0)
            {
                StockShelf shelfFrom = StockManager.GetStockShelf(es.SB_StockShelfIdFrom);
                shelfFromText = shelfFrom.Name;
            }
            string shelfToText = String.Empty;
            if (es.SB_StockShelfIdTo > 0)
            {
                StockShelf shelfTo = StockManager.GetStockShelf(es.SB_StockShelfIdTo);
                shelfToText = shelfTo.Name;
            }
            return shelfFromText + " - " + shelfToText;
        }



        protected string CreateStockProductIntervalText(EvaluatedSelection es)
        {
            string text = es.SB_ProductNrFrom + "-" + es.SB_ProductNrTo;
            return text;
        }

        protected string GetStockInventoryHeaderText(EvaluatedSelection es)
        {
            string text = "";
            if (es.SB_StockInventoryId > 0)
            {
                using (CompEntities entities = new CompEntities())
                {
                    StockInventoryHead head = StockManager.GetStockInventory(es.SB_StockInventoryId);
                    if (head != null)
                        text = head.HeaderText;
                }
            }

            return text;
        }

        #endregion

        #region HR

        #endregion

        #endregion
    }

    #region Help-classes

    public class ReportDataHistoryRepository : BaseReportDataManager
    {
        #region Variables

        private readonly int actorCompanyId;
        private readonly int recordId;
        private readonly int entity;
        private readonly int sysReportTemplateTypeId;
        private readonly bool validParameters;

        private readonly bool hasActivatedHistory;
        public bool HasActivatedHistory
        {
            get
            {
                return this.hasActivatedHistory && this.validParameters;
            }
        }
        public bool HasSavedHistory
        {
            get
            {
                return this.hasActivatedHistory && this.historys != null && this.historys.Count > 0;
            }
        }

        private List<ReportDataHistory> historys;
        private List<Tuple<SoeReportDataHistoryHeadTag, SoeReportDataHistoryTag, string>> addedHistorys;
        private bool HasAddedHistorys
        {
            get
            {
                if (this.addedHistorys == null)
                    return false;

                return this.addedHistorys.Count > 0;
            }
        }

        #endregion

        #region Factory

        public static ReportDataHistoryRepository CreateBillingInvoiceRepository(ParameterObject parameterObject, CustomerInvoice customerInvoice, int actorCompanyId)
        {
            bool hasValidParameters = false;

            if (customerInvoice != null && customerInvoice.Origin != null)
            {
                bool validStatus = customerInvoice.Origin.Status != (int)SoeOriginStatus.Draft;
                bool validType = customerInvoice.Origin.Type == (int)SoeOriginType.CustomerInvoice;
                hasValidParameters = validStatus && validType;
            }

            return new ReportDataHistoryRepository(parameterObject, customerInvoice?.InvoiceId ?? 0, (int)SoeEntityType.CustomerInvoice, (int)SoeReportTemplateType.BillingInvoice, (int)CompanySettingType.BillingUseInvoiceReportDataHistory, hasValidParameters, actorCompanyId);
        }

        #endregion

        #region Ctor

        private ReportDataHistoryRepository(ParameterObject parameterObject, int recordId, int entity, int sysReportTemplateTypeId, int activatedHistoryCompanySetting, bool validParameters, int actorCompanyId)
            : base(parameterObject)
        {
            this.actorCompanyId = actorCompanyId;
            this.recordId = recordId;
            this.entity = entity;
            this.sysReportTemplateTypeId = sysReportTemplateTypeId;
            this.validParameters = validParameters;

            this.hasActivatedHistory = SettingManager.GetBoolSetting(SettingMainType.Company, activatedHistoryCompanySetting, 0, actorCompanyId, 0);

            LoadHistory();
        }

        #endregion

        #region Public methods

        public string GetHistoryValue(SoeReportDataHistoryHeadTag headTag, SoeReportDataHistoryTag tag)
        {
            if (!HasActivatedHistory)
                return String.Empty;

            ReportDataHistory history = Filter(headTag, tag).FirstOrDefault();
            return history != null ? history.Value : String.Empty;
        }

        public void AddHistory(SoeReportDataHistoryHeadTag headTag, SoeReportDataHistoryTag tag, string value)
        {
            if (!HasActivatedHistory)
                return;
            if (IsAdded(tag))
                return;

            if (this.addedHistorys == null)
                this.addedHistorys = new List<Tuple<SoeReportDataHistoryHeadTag, SoeReportDataHistoryTag, string>>();
            this.addedHistorys.Add(Tuple.Create(headTag, tag, value));
        }

        public ActionResult SaveHistory()
        {
            if (!HasActivatedHistory)
                return new ActionResult(false);
            if (!HasAddedHistorys)
                return new ActionResult(false);

            using (CompEntities entities = new CompEntities())
            {
                foreach (var addedHistory in addedHistorys)
                {
                    ReportDataHistory history = new ReportDataHistory()
                    {
                        ActorCompanyId = this.actorCompanyId,
                        SysReportTemplateTypeId = this.sysReportTemplateTypeId,
                        RecordId = this.recordId,
                        Entity = this.entity,
                        HeadTagId = (int)addedHistory.Item1,
                        TagId = (int)addedHistory.Item2,
                        Value = addedHistory.Item3 == null ? string.Empty : addedHistory.Item3,
                    };
                    SetCreatedProperties(history);

                    entities.ReportDataHistory.AddObject(history);
                }

                this.addedHistorys = null;

                return SaveChanges(entities);
            }
        }

        #endregion

        #region Private methods

        private void LoadHistory()
        {
            if (HasActivatedHistory)
            {
                using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
                entitiesReadOnly.ReportDataHistory.NoTracking();
                this.historys = (from h in entitiesReadOnly.ReportDataHistory
                                 where h.ActorCompanyId == this.actorCompanyId &&
                                 h.SysReportTemplateTypeId == this.sysReportTemplateTypeId &&
                                 h.RecordId == this.recordId &&
                                 h.Entity == this.entity
                                 select h).ToList();
            }
            else
                this.historys = new List<ReportDataHistory>();
        }

        private List<ReportDataHistory> Filter(SoeReportDataHistoryHeadTag headTag, SoeReportDataHistoryTag tag)
        {
            if (this.historys == null)
                return null;

            return this.historys.Where(i => i.HeadTagId == (int)headTag && i.TagId == (int)tag).Sort();
        }

        private bool IsAdded(SoeReportDataHistoryTag tag)
        {
            if (this.historys == null)
                return false;

            return this.addedHistorys != null && this.addedHistorys.Any(i => i.Item2 == tag);
        }

        #endregion
    }

    public abstract class PersonalDataReportRepository : BaseReportDataManager
    {
        #region Variables

        protected PersonalDataLogBatchDTO batch;

        protected EvaluatedSelection es;
        private TermGroup_PersonalDataType dataType;

        #endregion

        #region Ctor

        protected PersonalDataReportRepository(ParameterObject parameterObject, EvaluatedSelection es, TermGroup_PersonalDataType dataType, string objectName) : base(parameterObject)
        {
            this.Init(es, dataType, objectName);
        }

        protected PersonalDataReportRepository(ParameterObject parameterObject, TermGroup_PersonalDataType dataType, string objectName, CreateReportResult reportResult, string reportUrl = null) : base(parameterObject)
        {
            this.Init(dataType, objectName, reportResult.ExportType, reportResult.ReportId, reportUrl);
        }

        protected PersonalDataReportRepository(ParameterObject parameterObject, TermGroup_PersonalDataType dataType, string objectName, int reportId, TermGroup_ReportExportType exportType, string reportUrl = null) : base(parameterObject)
        {
            this.Init(dataType, objectName, exportType, reportId, reportUrl);
        }

        private void Init(EvaluatedSelection es, TermGroup_PersonalDataType dataType, string objectName)
        {
            this.es = es;
            this.dataType = dataType;

            PersonalDataBatchType batchType;
            switch (es.ExportType)
            {
                case TermGroup_ReportExportType.Pdf:
                    batchType = PersonalDataBatchType.ReportPdf;
                    break;
                case TermGroup_ReportExportType.File:
                    batchType = PersonalDataBatchType.ReportFile;
                    break;
                default:
                    batchType = PersonalDataBatchType.ReportRaw;
                    break;
            }

            ReportUrl reportUrl = es.ReportUrlId.HasValue ? ReportManager.GetReportUrl(es.ReportUrlId.Value, es.ActorCompanyId) : null;
            this.batch = LoggerManager.GeneratePersonalDataBatch(Guid.NewGuid(), batchType, es.ReportId, objectName, reportUrl != null ? reportUrl.Url : "");
        }

        private void Init(TermGroup_PersonalDataType dataType, string objectName, TermGroup_ReportExportType exportType, int reportId, string reportUrl = null)
        {
            this.dataType = dataType;

            PersonalDataBatchType batchType;
            switch (exportType)
            {
                case TermGroup_ReportExportType.Pdf:
                    batchType = PersonalDataBatchType.ReportPdf;
                    break;
                case TermGroup_ReportExportType.File:
                    batchType = PersonalDataBatchType.ReportFile;
                    break;
                default:
                    batchType = PersonalDataBatchType.ReportRaw;
                    break;
            }

            this.batch = LoggerManager.GeneratePersonalDataBatch(Guid.NewGuid(), batchType, reportId, objectName, reportUrl ?? string.Empty);
        }

        #endregion

        #region Public methods

        public void GeneratePersonalDataLogs(List<PersonalDataTracker> trackers)
        {
            if (trackers.IsNullOrEmpty())
                return;

            List<PersonalDataLogDTO> personalDataLogs = new List<PersonalDataLogDTO>();
            foreach (PersonalDataTracker tracker in trackers)
            {
                personalDataLogs.AddRange(LoggerManager.GeneratePersonalDataLogs(tracker.DTO, dataType: this.dataType, filterInformationTypes: tracker.InformationTypes));
            }

            if (!personalDataLogs.Any())
                return;

            this.batch.PersonalDataLog.AddRange(personalDataLogs);
            LoggerConnector.SavePersonalDataLogServiceBusFireAndForget(this.batch);
        }

        #endregion
    }

    public class PersonalDataEmployeeReportRepository : PersonalDataReportRepository
    {
        #region Definitions

        private readonly List<string> DEFINITIONS_SOCIALSEC = new List<string>
        {
            "SocialSec",
            "EmployeeSocialSec",
            "EmployeeDateOfBirth",
            "PersonNr",
            "Personnummer"
        };
        private readonly List<string> DEFINITIONS_SALARYDISTRESSAMOUNT = new List<string>
        {
            "SalaryDistressAmount"
        };
        private readonly List<string> DEFINITIONS_SALARYDISTRESSRESERVEDAMOUNT = new List<string>
        {
            "SalaryDistressReservedAmount"
        };
        private readonly List<string> DEFINITIONS_HIGHRISKPROTECTION = new List<string>
        {
            "HighRiskProtection"
        };
        private readonly List<string> DEFINITIONS_HIGHRISKPROTECTIONTO = new List<string>
        {
            "HighRiskProtectionTo"
        };
        private readonly List<string> DEFINITIONS_MEDICALCERTIFICATEREMINDER = new List<string>
        {
            "MedicalCertificateReminder"
        };
        private readonly List<string> DEFINITIONS_MEDICALCERTIFICATEDAYS = new List<string>
        {
            "MedicalCertificateDays"
        };
        private readonly List<string> DEFINITIONS_ABSENCE105DAYSEXCLUDED = new List<string>
        {
            "Absence105DaysExcluded"
        };
        private readonly List<string> DEFINITIONS_ABSENCE105DAYSEXCLUDEDDAYS = new List<string>
        {
            "Absence105DaysExcludedDays"
        };
        private readonly List<string> DEFINITIONS_EMPLOYEEMEETINGID = new List<string>
        {
            "EmployeeMeetingId"
        };
        private readonly List<string> DEFINITIONS_EMPLOYEEUNIONFEEID = new List<string>
        {
            "EmployeeUnionFeeId"
        };
        private readonly List<string> DEFINITIONS_EMPLOYEECHILDID = new List<string>
        {
            "EmployeeChildId"
        };
        private readonly List<string> DEFINITIONS_EMPLOYEEVEHICLEID = new List<string>
        {
            "EmployeeVehicleId"
        };

        #endregion

        #region Variables

        private const TermGroup_PersonalDataType DATATYPE = TermGroup_PersonalDataType.Employee;
        private const string OBJECTNAME = "Employee";

        private List<PersonalDataTracker> trackers = null;
        private Guid userKey;
        private Guid employeeKey;
        private Guid employeeTaxKey;
        private Guid employeeMeetingKey;
        private Guid employeeUnionFeeKey;
        private Guid employeeChildKey;
        private Guid employeeVechicleKey;

        public List<EmployeeGroup> EmployeeGroups { get; set; }
        public List<PayrollGroup> PayrollGroups { get; set; }
        public List<PayrollPriceType> PayrollPriceTypes { get; set; }
        public List<AnnualLeaveGroup> AnnualLeaveGroups { get; set; }

        #endregion

        #region Ctor

        public PersonalDataEmployeeReportRepository(ParameterObject parameterObject, EvaluatedSelection es, CompEntities entities = null, List<EmployeeGroup> employeeGroups = null, List<PayrollGroup> payrollGroups = null, List<PayrollPriceType> priceTypes = null, List<AnnualLeaveGroup> annualLeaveGroups = null) : base(parameterObject, es, DATATYPE, OBJECTNAME)
        {
            this.EmployeeGroups = employeeGroups;
            this.PayrollGroups = payrollGroups;
            this.PayrollPriceTypes = priceTypes;
            this.AnnualLeaveGroups = annualLeaveGroups;

            this.Init(entities);
        }

        public PersonalDataEmployeeReportRepository(ParameterObject parameterObject, CreateReportResult reportResult, CompEntities entities = null) : base(parameterObject, DATATYPE, OBJECTNAME, reportResult)
        {
            this.Init(entities);
        }

        public PersonalDataEmployeeReportRepository(ParameterObject parameterObject, int reportId, TermGroup_ReportExportType exportType, CompEntities entities = null) : base(parameterObject, DATATYPE, OBJECTNAME, reportId, exportType)
        {
            this.Init(entities);
        }

        private void Init(CompEntities entities = null)
        {
            this.trackers = new List<PersonalDataTracker>();
            this.userKey = Guid.NewGuid();
            this.employeeKey = Guid.NewGuid();
            this.employeeTaxKey = Guid.NewGuid();
            this.employeeMeetingKey = Guid.NewGuid();
            this.employeeUnionFeeKey = Guid.NewGuid();
            this.employeeChildKey = Guid.NewGuid();
            this.employeeVechicleKey = Guid.NewGuid();

            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            bool addToCache = this.parameterObject != null && this.parameterObject.ActorCompanyId != 0;
            if (addToCache)
            {
                if (this.EmployeeGroups == null)
                    this.EmployeeGroups = GetEmployeeGroupsForPersonalDataRepoFromCache(entities ?? entitiesReadOnly, CacheConfig.Company(parameterObject.ActorCompanyId));
                if (this.PayrollGroups == null)
                    this.PayrollGroups = GetPayrollGroupsForPersonalDataRepoFromCache(entities ?? entitiesReadOnly, CacheConfig.Company(parameterObject.ActorCompanyId));
                if (this.PayrollPriceTypes == null)
                    this.PayrollPriceTypes = GetPayrollPriceTypesForPersonalDataRepoFromCache(entities ?? entitiesReadOnly, CacheConfig.Company(parameterObject.ActorCompanyId));
                if (this.AnnualLeaveGroups == null)
                    this.AnnualLeaveGroups = GetAnnualLeaveGroupsForPersonalDataRepoFromCache(entities ?? entitiesReadOnly, CacheConfig.Company(parameterObject.ActorCompanyId));

                ExtensionCache.Instance.AddToEmployeePayrollGroupExtensionCaches(base.parameterObject.ActorCompanyId, this.EmployeeGroups, this.PayrollGroups, this.PayrollPriceTypes, this.AnnualLeaveGroups);
            }
        }

        #endregion

        #region Public methods

        public void AddUser(User model, XElement element)
        {
            if (model == null || element == null)
                return;
            TrackModel(this.userKey, model.UserId, model, element);
        }

        public void AddEmployee(Employee model, XElement element)
        {
            if (model == null || element == null)
                return;
            TrackModel(this.employeeKey, model.EmployeeId, model, element);
        }

        public void AddEmployeeSocialSec(Employee model)
        {
            if (model == null)
                return;
            TrackInformationType(this.employeeKey, model.EmployeeId, model, TermGroup_PersonalDataInformationType.SocialSec);
        }

        public void AddEmployeeIllnessInformation(Employee model)
        {
            if (model == null)
                return;
            TrackInformationType(this.employeeKey, model.EmployeeId, model, TermGroup_PersonalDataInformationType.IllnessInformation);
        }

        public void AddEmployeeTax(EmployeeTaxSE model, XElement element)
        {
            if (model == null || element == null)
                return;
            TrackModel(this.employeeTaxKey, model.EmployeeTaxId, model, element);
        }

        public void AddEmployeeMeeting(EmployeeMeeting model, XElement element)
        {
            if (model == null || element == null)
                return;
            TrackModel(this.employeeMeetingKey, model.EmployeeMeetingId, model, element);
        }

        public void AddEmployeeUnionFee(EmployeeUnionFee model, XElement element)
        {
            if (model == null || element == null)
                return;
            TrackModel(this.employeeUnionFeeKey, model.EmployeeUnionFeeId, model, element);
        }

        public void AddEmployeeChild(EmployeeChild model, XElement element)
        {
            if (model == null || element == null)
                return;
            TrackModel(this.employeeChildKey, model.EmployeeChildId, model, element);
        }

        public void AddEmployeeVehicle(EmployeeVehicle model, XElement element)
        {
            if (model == null || element == null)
                return;
            TrackModel(this.employeeVechicleKey, model.EmployeeVehicleId, model, element);
        }

        public void GenerateLogs()
        {
            base.GeneratePersonalDataLogs(this.trackers);
        }

        #endregion

        #region Help-methods

        private PersonalDataTracker GetTracker(Guid key, int id)
        {
            return this.trackers.FirstOrDefault(t => t.Key == key && t.Id == id);
        }

        private bool HasTracker(Guid key, int id)
        {
            return GetTracker(key, id) != null;
        }

        private void AddTracker(Guid key, int id, object model, object dto, TermGroup_PersonalDataInformationType informationType)
        {
            AddTracker(key, id, model, dto, new List<TermGroup_PersonalDataInformationType> { informationType });
        }

        private void AddTracker(Guid key, int id, object model, object dto, List<TermGroup_PersonalDataInformationType> informationTypes)
        {
            this.trackers.Add(new PersonalDataTracker(key, id, model, dto, informationTypes));
        }

        private void TrackModel(Guid key, int id, object model, XElement element)
        {
            if (model == null || element == null)
                return;

            if (HasTracker(this.employeeKey, id))
                return;
            if (!TryParseModel(key, model, element, out List<TermGroup_PersonalDataInformationType> informationTypes, out object dto))
                return;

            AddTracker(key, id, model, dto, informationTypes);
        }

        private void TrackInformationType(Guid key, int id, object model, TermGroup_PersonalDataInformationType informationType)
        {
            if (model == null)
                return;

            PersonalDataTracker tracker = GetTracker(key, id);
            if (tracker != null && tracker.ContainsInformationType(informationType))
                return;
            if (!TryParseInformationType(key, model, informationType, tracker == null, out object dto))
                return;

            if (tracker != null)
                tracker.AddInformationType(informationType);
            else
                AddTracker(key, id, model, dto, informationType);
        }

        private bool TryParseModel(Guid key, object model, XElement element, out List<TermGroup_PersonalDataInformationType> informationTypes, out object dto)
        {
            dto = null;
            informationTypes = new List<TermGroup_PersonalDataInformationType>();

            if (key == this.userKey)
            {
                User user = model as User;
                if (user != null)
                {
                    if (ContainsSocialSecElement(element, user.SocialSec))
                        informationTypes.Add(TermGroup_PersonalDataInformationType.SocialSec);

                    if (informationTypes.Count > 0)
                        dto = user.ToDTO();
                }
            }
            else if (key == this.employeeKey)
            {
                Employee employee = model as Employee;
                if (employee != null)
                {
                    if (ContainsSocialSecElement(element, employee.SocialSec))
                        informationTypes.Add(TermGroup_PersonalDataInformationType.SocialSec);
                    if (ContainsIllnessInformation(element, employee))
                        informationTypes.Add(TermGroup_PersonalDataInformationType.IllnessInformation);

                    if (informationTypes.Count > 0)
                        dto = employee.ToDTO(employeeGroups: this.EmployeeGroups, payrollGroups: this.PayrollGroups, payrollPriceTypes: this.PayrollPriceTypes);
                }
            }
            else if (key == this.employeeTaxKey)
            {
                EmployeeTaxSE employeeTax = model as EmployeeTaxSE;
                if (employeeTax != null)
                {
                    if (ContainsDistressInformation(element, employeeTax))
                        informationTypes.Add(TermGroup_PersonalDataInformationType.SalaryDistress);

                    if (informationTypes.Count > 0)
                        dto = employeeTax.ToDTO();
                }
            }
            else if (key == this.employeeMeetingKey)
            {
                EmployeeMeeting employeeMeeting = model as EmployeeMeeting;
                if (employeeMeeting != null)
                {
                    if (ContainsEmployeeMeetingId(element, employeeMeeting.EmployeeMeetingId))
                        informationTypes.Add(TermGroup_PersonalDataInformationType.EmployeeMeeting);

                    if (informationTypes.Count > 0)
                        dto = employeeMeeting.ToDTO(false);
                }
            }
            else if (key == this.employeeUnionFeeKey)
            {
                EmployeeUnionFee unionFee = model as EmployeeUnionFee;
                if (unionFee != null)
                {
                    if (ContainsEmployeeUnionFeeId(element, unionFee.EmployeeUnionFeeId))
                        informationTypes.Add(TermGroup_PersonalDataInformationType.Unionfee);

                    if (informationTypes.Count > 0)
                        dto = unionFee.ToDTO();
                }
            }
            else if (key == this.employeeChildKey)
            {
                EmployeeChild employeeChild = model as EmployeeChild;
                if (employeeChild != null)
                {
                    if (ContainsEmployeeChildId(element, employeeChild.EmployeeChildId))
                        informationTypes.Add(TermGroup_PersonalDataInformationType.ParentalLeaveAndChild);

                    if (informationTypes.Count > 0)
                        dto = employeeChild.ToDTO();
                }
            }
            else if (key == this.employeeVechicleKey)
            {
                EmployeeVehicle employeeVehicle = model as EmployeeVehicle;
                if (employeeVehicle != null)
                {
                    if (ContainsEmployeeVehicleId(element, employeeVehicle.EmployeeVehicleId))
                        informationTypes.Add(TermGroup_PersonalDataInformationType.VehicleInformation);

                    if (informationTypes.Count > 0)
                        dto = employeeVehicle.ToDTO(false, false, false);
                }
            }

            return dto != null;
        }

        private bool TryParseInformationType(Guid key, object model, TermGroup_PersonalDataInformationType informationType, bool generateDTO, out object dto)
        {
            bool isValid = false;
            dto = null;

            if (key == this.userKey)
            {
                User user = model as User;
                if (user != null)
                {
                    if (informationType == TermGroup_PersonalDataInformationType.SocialSec)
                        isValid = true;

                    if (isValid && generateDTO)
                        dto = user.ToDTO();
                }
            }
            else if (key == this.employeeKey)
            {
                Employee employee = model as Employee;
                if (employee != null)
                {
                    if (informationType == TermGroup_PersonalDataInformationType.SocialSec)
                        isValid = true;
                    if (informationType == TermGroup_PersonalDataInformationType.IllnessInformation)
                        isValid = true;

                    if (isValid && generateDTO)
                        dto = employee.ToDTO();
                }
            }
            else if (key == this.employeeTaxKey)
            {
                //Not supported
            }
            else if (key == this.employeeMeetingKey)
            {
                //Not supported
            }
            else if (key == this.employeeUnionFeeKey)
            {
                //Not supported
            }
            else if (key == this.employeeChildKey)
            {
                //Not supported
            }
            else if (key == this.employeeVechicleKey)
            {
                //Not supported
            }

            return isValid;
        }

        private bool ContainsSocialSecElement(XElement element, string value)
        {
            return ContainsValue(element, DEFINITIONS_SOCIALSEC, value);
        }

        private bool ContainsIllnessInformation(XElement element, Employee model)
        {
            return
                ContainsHighRiskProtection(element, model.HighRiskProtection) ||
                ContainsHighRiskProtectionTo(element, model.HighRiskProtectionTo) ||
                ContainsMedicalCertificateReminder(element, model.MedicalCertificateReminder) ||
                ContainsMedicalCertificateDays(element, model.MedicalCertificateDays) ||
                ContainsAbsence105DaysExcluded(element, model.Absence105DaysExcluded) ||
                ContainsAbsence105DaysExcludedDays(element, model.Absence105DaysExcludedDays);
        }

        private bool ContainsHighRiskProtection(XElement element, bool value)
        {
            return ContainsValue(element, DEFINITIONS_HIGHRISKPROTECTION, value);
        }

        private bool ContainsHighRiskProtectionTo(XElement element, DateTime? value)
        {
            return value.HasValue && ContainsValue(element, DEFINITIONS_HIGHRISKPROTECTIONTO, value.Value);
        }

        private bool ContainsMedicalCertificateReminder(XElement element, bool value)
        {
            return ContainsValue(element, DEFINITIONS_MEDICALCERTIFICATEREMINDER, value);
        }

        private bool ContainsMedicalCertificateDays(XElement element, int? value)
        {
            return value.HasValue && ContainsValue(element, DEFINITIONS_MEDICALCERTIFICATEDAYS, value.Value);
        }

        private bool ContainsAbsence105DaysExcluded(XElement element, bool value)
        {
            return ContainsValue(element, DEFINITIONS_ABSENCE105DAYSEXCLUDED, value);
        }

        private bool ContainsAbsence105DaysExcludedDays(XElement element, int? value)
        {
            return value.HasValue && ContainsValue(element, DEFINITIONS_ABSENCE105DAYSEXCLUDEDDAYS, value.Value);
        }

        private bool ContainsDistressInformation(XElement element, EmployeeTaxSE entity)
        {
            return
                ContainsSalaryDistressAmount(element, entity.SalaryDistressAmount) ||
                ContainsSalaryDistressReservedAmount(element, entity.SalaryDistressAmount);
        }

        private bool ContainsSalaryDistressAmount(XElement element, decimal? value)
        {
            return value.HasValue && ContainsValue(element, DEFINITIONS_SALARYDISTRESSAMOUNT, value.Value);
        }

        private bool ContainsSalaryDistressReservedAmount(XElement element, decimal? value)
        {
            return value.HasValue && ContainsValue(element, DEFINITIONS_SALARYDISTRESSRESERVEDAMOUNT, value.Value);
        }

        private bool ContainsEmployeeMeetingId(XElement element, int value)
        {
            return ContainsValue(element, DEFINITIONS_EMPLOYEEMEETINGID, value);
        }

        private bool ContainsEmployeeUnionFeeId(XElement element, int value)
        {
            return ContainsValue(element, DEFINITIONS_EMPLOYEEUNIONFEEID, value);
        }

        private bool ContainsEmployeeChildId(XElement element, int value)
        {
            return ContainsValue(element, DEFINITIONS_EMPLOYEECHILDID, value);
        }

        private bool ContainsEmployeeVehicleId(XElement element, int value)
        {
            return ContainsValue(element, DEFINITIONS_EMPLOYEEVEHICLEID, value);
        }

        private bool ContainsValue(XElement element, List<string> definitions, bool value)
        {
            return ContainsValue(element, definitions, value.ToInt());
        }

        private bool ContainsValue(XElement element, List<string> definitions, int value)
        {
            return ContainsValue(element, definitions, value.ToString());
        }

        private bool ContainsValue(XElement element, List<string> definitions, decimal value)
        {
            return ContainsValue(element, definitions, value.ToString().Replace(",", "."));
        }

        private bool ContainsValue(XElement element, List<string> definitions, DateTime value)
        {
            return ContainsValue(element, definitions, value.ToShortDateString());
        }

        private bool ContainsValue(XElement element, List<string> definitions, string value, bool checkValue = false)
        {
            if (!checkValue)
                value = null;

            foreach (string definition in definitions)
            {
                if (ContainsValue(element, definition, value))
                    return true;
            }

            return false;
        }

        private bool ContainsValue(XElement element, string definition, string value = null)
        {
            List<XElement> childElements = XmlUtil.GetChildElements(element, definition);
            foreach (XElement childElement in childElements)
            {
                if (!string.IsNullOrEmpty(childElement.Value) && (value.IsNullOrEmpty() || childElement.Value == value))
                    return true;
            }
            return false;
        }

        #endregion
    }

    public class PersonalDataTracker
    {
        #region Variables

        public Guid Key { get; }
        public int Id { get; }
        public object Model { get; }
        public object DTO { get; }
        public List<TermGroup_PersonalDataInformationType> InformationTypes { get; }

        #endregion

        #region Ctor

        public PersonalDataTracker(Guid key, int id, object model, object dto, List<TermGroup_PersonalDataInformationType> informationTypes)
        {
            this.Key = key;
            this.Id = id;
            this.Model = model;
            this.DTO = dto;
            this.InformationTypes = informationTypes;
        }

        #endregion

        #region Public methods

        public void AddInformationType(TermGroup_PersonalDataInformationType informationType)
        {
            if (ContainsInformationType(informationType))
                return;

            this.InformationTypes.Add(informationType);
        }

        public bool ContainsInformationType(TermGroup_PersonalDataInformationType informationType)
        {
            return this.InformationTypes.Contains(informationType);
        }

        #endregion
    }

    public class CreateReportResult
    {
        #region Input

        public CreateReportResultInput Input { get; set; }
        public EvaluatedSelection EvaluatedSelection { get; set; } //EvaluatedSelection (should be removed when transition to angular is complete)
        public bool HasValidSelection { get { return this.Input != null ? this.Input.IsValid : this.EvaluatedSelection != null; } }
        public bool IsMigrated { get { return this.Input != null; } }

        //Identification
        public int ActorCompanyId { get { return (this.Input != null ? this.Input.ActorCompanyId : this.EvaluatedSelection?.ActorCompanyId) ?? 0; } }
        public int UserId { get { return (this.Input != null ? this.Input.UserId : this.EvaluatedSelection?.UserId) ?? 0; } }
        public int RoleId { get { return (this.Input != null ? this.Input.RoleId : this.EvaluatedSelection?.RoleId) ?? 0; } }
        public string LoginName { get { return (this.Input != null ? this.Input.User?.LoginName : this.EvaluatedSelection?.LoginName) ?? string.Empty; } }
        public int? ReportUrlId { get { return this.EvaluatedSelection?.ReportUrlId; } }

        //Report
        public int ReportId { get { return (this.Input != null ? this.Input.ReportId : this.EvaluatedSelection?.ReportId) ?? 0; } }
        public int ReportNr { get { return (this.Input != null ? this.Input.Report?.ReportNr : this.EvaluatedSelection?.ReportNr) ?? 0; } }
        private string reportName { get; set; }
        public string ReportName { get { return !string.IsNullOrEmpty(reportName) ? reportName : ((this.Input != null ? this.Input.Report?.Name : this.EvaluatedSelection?.ReportName) ?? string.Empty); } }
        public string ReportSpecial { get { return (this.Input != null ? this.Input.Report?.Special : this.EvaluatedSelection?.Special) ?? string.Empty; } }
        public string ReportDescription { get { return (this.Input != null ? this.Input.Report?.Description : this.EvaluatedSelection?.ReportDescription) ?? string.Empty; } }
        public bool IsReportStandard { get { return (this.Input != null ? this.Input.Report?.Standard : this.EvaluatedSelection?.IsReportStandard) ?? true; } }
        public bool GetDetailedInformation { get { return (this.Input != null ? this.Input.Report?.GetDetailedInformation : this.EvaluatedSelection?.GetDetailedInformation) ?? false; } }

        //ReportTemplate
        public SoeReportType SoeReportType { get { return this.Input != null ? this.Input.SoeReportType : SoeReportType.CrystalReport; } }
        public SoeReportTemplateType ReportTemplateType { get { return (this.Input != null ? this.Input.ReportTemplateType : this.EvaluatedSelection?.ReportTemplateType) ?? SoeReportTemplateType.Unknown; } }
        public int ReportTemplateId { get { return (this.Input != null ? this.Input.Report?.ReportTemplateId : this.EvaluatedSelection?.ReportTemplateId) ?? 0; } }
        public int DataStorageId { get { return (this.Input != null ? this.Input.DataStorageId : this.EvaluatedSelection?.ST_DataStorageId) ?? 0; } }
        public byte[] Template { get { return (this.Input != null ? this.Input.ReportTemplateData : this.EvaluatedSelection.Template); } }

        //Selection
        public TermGroup_ReportExportType ExportType { get { return (this.Input != null ? this.Input.ExportType : this.EvaluatedSelection?.ExportType) ?? TermGroup_ReportExportType.Unknown; } }
        public TermGroup_ReportExportFileType ExportFileType { get { return (this.Input != null ? this.Input.ExportFileType : this.EvaluatedSelection?.ExportFileType) ?? TermGroup_ReportExportFileType.Unknown; } }
        public string FilePath
        {
            get { return (this.Input != null ? this.Input.FilePath : this.EvaluatedSelection?.FilePath) ?? string.Empty; }
            set
            {
                if (this.EvaluatedSelection != null)
                    this.EvaluatedSelection.FilePath = value;
                if (this.Input != null)
                    this.Input.FilePath = value;
            }
        }
        public string Selection { get { return (this.Input != null ? this.Input.SelectionJson : this.EvaluatedSelection?.Selection) ?? string.Empty; } }

        //Not migrated
        public int ReportPackageId { get { return this.EvaluatedSelection?.ReportPackageId ?? 0; } }
        public string ReportPackageName { get { return this.EvaluatedSelection?.ReportPackageName ?? String.Empty; } }
        public string ReportPackageDescription { get { return this.EvaluatedSelection?.ReportPackageDescription ?? String.Empty; } }
        private string _ReportNamePostfix;
        public string ReportNamePostfix
        {

            get
            {
                return this.IsMigrated ? this._ReportNamePostfix : this.EvaluatedSelection?.ReportNamePostfix ?? String.Empty;
            }
            set { this._ReportNamePostfix = value; }
        }


        public bool IgnoreSchema { get { return this.EvaluatedSelection?.IgnoreSchema ?? false; } }
        public bool MergePdfs { get { return this.EvaluatedSelection?.MergePdfs ?? false; } }
        public int? InvoiceDistributionId { get { return this.EvaluatedSelection?.InvoiceDistributionId; } }
        public bool? Email { get { return this.EvaluatedSelection?.Email; } }
        public int? EmailTemplateId { get { return this.EvaluatedSelection?.EmailTemplateId; } }
        public List<int> EmailRecipients { get { return this.EvaluatedSelection?.EmailRecipients; } }
        public string EmailFileName { get { return this.EvaluatedSelection?.EmailFileName ?? string.Empty; } }
        public string SingleRecipient { get { return this.EvaluatedSelection?.SingleRecipients ?? string.Empty; } }
        public List<KeyValuePair<string, byte[]>> EmailAttachments { get { return this.EvaluatedSelection?.EmailAttachments; } }
        public EmailTemplateDTO EmailTemplate { get { return this.EvaluatedSelection?.EmailTemplate; } set { this.EvaluatedSelection.EmailTemplate = value; } }

        #endregion

        #region Output

        public SoeReportDataResultMessage ResultMessage { get; set; }
        public string ResultMessageDetails { get; set; }
        public bool Success { get; set; }
        public DataSet DataSet { get; set; }
        public byte[] Data { get; set; }
        public MatrixResult MatrixResult { get; set; }
        public List<byte[]> Datas { get; set; }
        public ReportDocument ReportDocument { get; set; }
        public XDocument Document { get; set; }
        public List<XDocument> Documents
        {
            get
            {
                if (!this.Outputs.IsNullOrEmpty())
                    return Outputs.Where(w => w.Document != null).Select(s => s.Document).ToList();

                return null;
            }
        }
        public XmlDocument XmlDocument { get; set; }
        public List<CreateReportResultOutput> Outputs = new List<CreateReportResultOutput>();

        #endregion

        #region Ctor

        public CreateReportResult()
        {
            this.Success = true;
        }

        public CreateReportResult(SoeReportDataResultMessage outputMessage, bool success)
        {
            this.ResultMessage = outputMessage;
            this.Success = success;
        }

        #endregion

        #region Public methods

        public void SetReportName(string name)
        {
            reportName = name;
        }

        public void SetSuccessMessage(SoeReportDataResultMessage outputMessage)
        {
            this.Success = true;
            this.ResultMessage = outputMessage;
        }

        public void SetErrorMessage(SoeReportDataResultMessage outputMessage, Exception ex = null)
        {
            this.Success = false;
            this.ResultMessage = outputMessage;
            if (ex != null)
                this.ResultMessageDetails = ex.ToString();
        }

        public void SetErrorMessage(SoeReportDataResultMessage outputMessage, string messageDetails)
        {
            this.Success = false;
            this.ResultMessage = outputMessage;
            this.ResultMessageDetails = messageDetails;
        }

        #endregion

    }

    public class CreateReportResultOutput
    {
        public XDocument Document { get; set; }
        public byte[] Pdf { get; set; }
    }

    public class CreateReportResultInput
    {
        #region Variables

        private bool isValid;
        public bool IsValid { get { return isValid; } }
        public bool ForceValidation { get; }

        //Identification
        public Company Company { get; set; }
        public User User { get; set; }
        public Role Role { get; set; }
        public int ActorCompanyId { get { return this.Company?.ActorCompanyId ?? 0; } }
        public int UserId { get { return this.User?.UserId ?? 0; } }
        public int RoleId { get { return this.Role?.RoleId ?? 0; } }

        //Report
        public Report Report { get; set; }
        public int ReportId { get { return this.Report?.ReportId ?? 0; } }
        public string ReportName { get { return this.Report?.Name ?? string.Empty; } }
        public string ReportDescription { get { return this.Report?.Description ?? string.Empty; } }
        public bool GetDetailedInformation { get { return this.Report?.GetDetailedInformation ?? false; } }

        //ReportTemplate

        public SoeReportType SoeReportType { get; set; }
        public SoeReportTemplateType ReportTemplateType { get; set; }
        public int ReportTemplateId { get; set; }
        public byte[] ReportTemplateData { get; set; }
        public bool ShowOnlyTotals { get; set; }

        //Selection
        public TermGroup_ReportExportType ExportType { get; set; }
        public TermGroup_ReportExportFileType ExportFileType { get; set; }
        public int ExportId { get; set; }
        public int ExportDefinitionLevelId { get; set; }
        public bool ForceNoColumnHeaders { get; set; }
        public string FilePath { get; set; }
        public bool? IsPreliminary { get; set; }
        public ReportJobDefinitionDTO ReportSelection { get; set; }
        public List<ReportDataSelectionDTO> Selections { get; set; }
        public string SelectionJson { get; set; }
        public int? DataStorageId { get; set; }

        //XML
        public XDocument DefaultDocument { get; set; }
        public XDocument Document { get; set; }
        public string RootName { get; set; }
        public XElement RootElementDefault { get; set; }
        public XElement ElementRoot { get; set; }
        public XElement ElementFirst { get; set; }
        public List<XElement> DefaultElementGroups { get; set; }

        #endregion

        #region Ctor

        public CreateReportResultInput(ReportPrintoutDTO dto)
        {
            this.ForceValidation = dto?.ForceValidation ?? false;
            this.ReportSelection = null;
            this.Selections = ReportDataSelectionDTO.FromJSON(dto?.Selection);
            this.isValid = true;
        }

        public CreateReportResultInput(ReportJobDefinitionDTO job)
        {
            this.ForceValidation = job?.ForceValidation ?? false;
            this.ReportSelection = job;
            this.Selections = null;
            this.isValid = true;
        }

        #endregion

        #region Public methods

        public List<ReportDataSelectionDTO> GetSelections()
        {
            if (this.Selections != null)
                return this.Selections;
            return this.ReportSelection?.Selections?.ToList() ?? new List<ReportDataSelectionDTO>();
        }

        public TSelection GetSelection<TSelection>(string key = null) where TSelection : ReportDataSelectionDTO
        {
            return this.GetSelections().GetSelection<TSelection>(key);
        }

        public bool HasSelections()
        {
            return !this.GetSelections().IsNullOrEmpty();
        }

        public void SetSelectionJson()
        {
            this.GetSelections().ForEach(s => s.Beautify());
            this.SelectionJson = ReportDataSelectionDTO.ToJSON(this.GetSelections());
        }

        public void ValidateSelection(SoeReportTemplateType? templateType)
        {
            if (this.Company == null || this.Role == null || this.User == null || this.Report == null || !this.HasSelections() || this.ReportTemplateType != templateType)
                this.isValid = false;
        }

        public void ValidateDocument()
        {
            if (this.DefaultDocument == null || this.RootElementDefault == null || this.RootElementDefault.Name == null)
                this.isValid = false;
        }

        #endregion
    }

    #endregion
}

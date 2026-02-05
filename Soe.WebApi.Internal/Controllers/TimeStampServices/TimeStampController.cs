using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.DTO;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Description;

namespace Soe.Api.Internal.Controllers.TimeStampServices
{

    [RoutePrefix("TimeStampServices/TimeStamp")]
    public class TimeStampController : ApiBase
    {
        public TimeStampController(WebApiInternalParamObject webApiInternalParamObject) : base(webApiInternalParamObject)
        {
        }

        #region Employee

        [HttpGet]
        [Route("GetEmployeeV2")]
        [ResponseType(typeof(GenericType<ActionResultSelect, TSEmployeeItem>))]
        public GenericType<ActionResultSelect, TSEmployeeItem> GetEmployeeV2(int actorCompanyId, string employeeNr, int timeTerminalId)
        {
            TimeStampManager tsm = new TimeStampManager(null);
            return tsm.GetEmployee(actorCompanyId, employeeNr, timeTerminalId);
        }

        [HttpGet]
        [Route("GetStampedInEmployees")]
        [ResponseType(typeof(List<TSEmployeeItemStatus>))]
        public List<TSEmployeeItemStatus> GetStampedInEmployees(int actorCompanyId, TermGroup_TimeStampAttendanceGaugeShowMode showMode)
        {
            var dm = new DashboardManager(null);
            var employeesStampedIn = dm.GetTimeStampAttendance(actorCompanyId, 0, 0, TermGroup_TimeStampAttendanceGaugeShowMode.AllLast24Hours, true, onlyIncludeAttestRoleEmployees: false, includeEmployeeNrInString: false);
            var stampedIn = new List<TSEmployeeItemStatus>();
            foreach (var e in employeesStampedIn)
            {
                stampedIn.Add(new TSEmployeeItemStatus() { Name = e.Name, StampTime = e.Time, TimeTerminalName = e.TimeTerminalName });
            }

            return stampedIn;
        }

        [HttpGet]
        [Route("GetNextEmployeeNr")]
        [ResponseType(typeof(string))]
        public string GetNextEmployeeNr(int actorCompanyId)
        {
            var em = new EmployeeManager(null);
            return em.GetNextEmployeeNr(actorCompanyId);
        }

        [HttpGet]
        [Route("GetCompanyNewsForEmployee")]
        [ResponseType(typeof(CompanyNewsBaseDTO))]
        public CompanyNewsBaseDTO GetCompanyNewsForEmployee(int employeeId, int actorCompanyId)
        {
            CompanyNewsBaseDTO dto = null;
            var em = new EmployeeManager(null);
            Employee employee = em.GetEmployee(employeeId, actorCompanyId, loadUser: true);
            if (employee != null && employee.User != null)
            {
                var gm = new GeneralManager(null);
                var um = new UserManager(null);
                int defaultRoleId = um.GetDefaultRoleId(actorCompanyId, employee.User);
                InformationDTO information = gm.GetUnreadInformations(employee.User.LicenseId, actorCompanyId, defaultRoleId, employee.User.UserId, false, false, true, um.GetUserLangId(employee.User.UserId)).FirstOrDefault();
                if (information != null)
                {
                    string text = "";
                    if (!String.IsNullOrEmpty(information.ShortText))
                    {
                        text = information.ShortText;
                        if (!String.IsNullOrEmpty(information.Text))
                            text += "<br><br>";
                    }
                    text += information.Text;
                    string plainText = StringUtility.HTMLToText(text, true);

                    // In old terminals, text cannot be null
                    if (String.IsNullOrEmpty(text))
                        text = information.Subject;

                    dto = new CompanyNewsBaseDTO()
                    {
                        CompanyNewsId = information.InformationId,
                        Title = information.Subject,
                        Text = text,
                        SimpleText = plainText
                    };
                }
            }

            return dto;
        }

        [HttpGet]
        [Route("SaveCompanyNewsAsRead")]
        [ResponseType(typeof(ActionResult))]
        public ActionResult SaveCompanyNewsAsRead(int employeeId, int companyNewsId, bool isRead, int actorCompanyId)
        {
            ActionResult result = new ActionResult(true);
            var em = new EmployeeManager(null);
            Employee employee = em.GetEmployee(employeeId, actorCompanyId, loadUser: true);
            if (employee != null && employee.User != null)
            {
                var gm = new GeneralManager(null);
                result = gm.SetInformationAsRead(companyNewsId, 0, employee.User.UserId, false, false);
            }

            return result;
        }

        [HttpGet]
        [Route("AddEmployeeToTerminal")]
        [ResponseType(typeof(ActionResult))]
        public ActionResult AddEmployeeToTerminal(int actorCompanyId, int timeTerminalId, int employeeId)
        {
            var tsm = new TimeStampManager(null);
            return tsm.AddEmployeeToTerminal(actorCompanyId, timeTerminalId, employeeId);
        }

        [HttpPost]
        [Route("SaveEmployeeV3")]
        [ResponseType(typeof(ActionResult))]
        public ActionResult SaveEmployeeV3(int actorCompanyId, int userId, TSEmployeeItem employee, int timeTerminalId)
        {
            var tsm = new TimeStampManager(null);
            return tsm.SaveEmployeeFromTimeStamp(actorCompanyId, employee, timeTerminalId, allowUpdatingEmployee: true);
        }

        #endregion

        #region TimeTerminal

        [HttpGet]
        [Route("GetTimeTerminal")]
        [ResponseType(typeof(TimeTerminalDTO))]
        public TimeTerminalDTO GetTimeTerminal(int timeTerminalId, int actorCompanyId, TimeTerminalType type)
        {
            TimeStampManager tsm = new TimeStampManager(null);
            List<TimeTerminalSettingType> validSettingTypes = tsm.GetValidSettingTypes(type);
            return tsm.GetTimeTerminal(timeTerminalId, actorCompanyId, type).ToDTO(true, true, true, validSettingTypes);
        }

        [HttpGet]
        [Route("GetTimeTerminals")]
        [ResponseType(typeof(List<TimeTerminalDTO>))]
        public List<TimeTerminalDTO> GetTimeTerminals(int actorCompanyId, TimeTerminalType type)
        {
            TimeStampManager tsm = new TimeStampManager(null);
            return tsm.GetTimeTerminals(actorCompanyId, type, true, false, false, false, false, false).ToDTOs(true, true, false);
        }

        [HttpGet]
        [Route("UpdateTimeTerminalRegisteredStatus")]
        [ResponseType(typeof(ActionResult))]
        public ActionResult UpdateTimeTerminalRegisteredStatus(int timeTerminalId, bool registered)
        {
            TimeStampManager tsm = new TimeStampManager(null);
            return tsm.UpdateTimeTerminalRegisteredStatus(timeTerminalId, registered);
        }

        [HttpGet]
        [Route("UpdateDbSchemaVersion")]
        [ResponseType(typeof(ActionResult))]
        public ActionResult UpdateDbSchemaVersion(int timeTerminalId, int dbSchemaVersion, string assemblyVersion)
        {
            TimeStampManager tsm = new TimeStampManager(null);
            return tsm.UpdateDbSchemaVersion(timeTerminalId, dbSchemaVersion, assemblyVersion);
        }

        [HttpGet]
        [Route("GetSysTerms")]
        [ResponseType(typeof(List<SysTermDTO>))]
        public List<SysTermDTO> GetSysTerms(int timeTerminalId, int actorCompanyId, int? sysCountryId, int termGroupId, DateTime? prevSyncDate)
        {
            var tsm = new TimeStampManager(null);
            return tsm.GetSysTerms(timeTerminalId, actorCompanyId, sysCountryId, new[] { termGroupId }, prevSyncDate);
        }

        [HttpGet]
        [Route("GetSysTermsFromGroupIds")]
        [ResponseType(typeof(List<SysTermDTO>))]
        public List<SysTermDTO> GetSysTermsFromGroupIds(int timeTerminalId, int actorCompanyId, int? sysCountryId, List<int> termGroupIds, DateTime? prevSyncDate)
        {
            var tsm = new TimeStampManager(null);
            return tsm.GetSysTerms(timeTerminalId, actorCompanyId, sysCountryId, termGroupIds, prevSyncDate);
        }

        #endregion

        #region SysLogg

        [HttpGet]
        [Route("LogError")]
        [ResponseType(typeof(ActionResult))]
        public ActionResult LogError(string server, string message, int? terminalId)
        {
            if (String.IsNullOrEmpty(message))
                return new ActionResult();

            // Skip some errors coming from old not updated terminals, filling the log
            if (message.Contains("GetCompanyNewsForEmployee") || message.Contains("DeleteOldRecords"))
                return new ActionResult();

            SysLogManager slm = new SysLogManager(null);
            if (terminalId.HasValue)
                slm.AddSysLogErrorMessage(server, THREAD, message, recordId: terminalId.Value, recordType: SoeSysLogRecordType.TimeTerminal);
            else
                slm.AddSysLogErrorMessage(server, THREAD, message);
            return new ActionResult(true);
        }

        #endregion

        #region Synchronize

        [HttpGet]
        [Route("GetServerTime")]
        [ResponseType(typeof(DateTime))]
        public DateTime GetServerTime()
        {
            return DateTime.Now;
        }

        [HttpGet]
        [Route("SetTimeTerminalLastSync")]
        [ResponseType(typeof(ActionResult))]
        public ActionResult SetTimeTerminalLastSync(int timeTerminalId)
        {
            TimeStampManager tsm = new TimeStampManager(null);
            return tsm.SetTimeTerminalLastSync(timeTerminalId);
        }

        [HttpGet]
        [Route("SyncTerminal")]
        [ResponseType(typeof(TimeTerminalDTO))]
        public TimeTerminalDTO SyncTerminal(int timeTerminalId, int actorCompanyId, TimeTerminalType type, DateTime? prevSyncDate)
        {
            TimeStampManager tsm = new TimeStampManager(null);
            List<TimeTerminalSettingType> validSettingTypes = tsm.GetValidSettingTypes(type);
            return tsm.SyncTimeTerminal(actorCompanyId, timeTerminalId, type, prevSyncDate).ToDTO(true, true, true, validSettingTypes);
        }

        [HttpGet]
        [Route("SyncAccountV2")]
        [ResponseType(typeof(List<TSAccountItem>))]
        public List<TSAccountItem> SyncAccountV2(int actorCompanyId, int timeTerminalId, DateTime? prevSyncDate)
        {
            TimeStampManager tsm = new TimeStampManager(null);
            return tsm.SyncAccountWithLimits(actorCompanyId, timeTerminalId, prevSyncDate).ToList();
        }

        [HttpGet]
        [Route("SyncEmployeeV3")]
        [ResponseType(typeof(List<TSEmployeeItem>))]
        public IEnumerable<TSEmployeeItem> SyncEmployeeV3(int actorCompanyId, DateTime? prevSyncDate, int timeTerminalId)
        {
            TimeStampManager tsm = new TimeStampManager(null);
            return tsm.SyncEmployee(actorCompanyId, prevSyncDate, timeTerminalId, true);
        }

        [HttpGet]
        [Route("SyncEmployeeGroup")]
        [ResponseType(typeof(List<TSEmployeeGroupItem>))]
        public IEnumerable<TSEmployeeGroupItem> SyncEmployeeGroup(int actorCompanyId, DateTime? prevSyncDate)
        {
            TimeStampManager tsm = new TimeStampManager(null);
            return tsm.SyncEmployeeGroup(actorCompanyId, prevSyncDate);
        }

        [HttpGet]
        [Route("SyncEmployeeScheduleV3")]
        [ResponseType(typeof(List<TSSyncEmployeeScheduleResult>))]
        public TSSyncEmployeeScheduleResult SyncEmployeeScheduleV3(int actorCompanyId, int accountDimId, DateTime startDate, DateTime stopDate, DateTime? prevSyncDate, int timeTerminalId)
        {
            TimeStampManager tsm = new TimeStampManager(null);
            return tsm.SyncEmployeeSchedule(actorCompanyId, accountDimId, startDate, stopDate, prevSyncDate, timeTerminalId);
        }

        [HttpGet]
        [Route("SyncOneEmployeeSchedule")]
        [ResponseType(typeof(TSSyncEmployeeScheduleResult))]
        public TSSyncEmployeeScheduleResult SyncOneEmployeeSchedule(int accountDimId, int employeeId, DateTime startDate, DateTime stopDate)
        {
            TimeStampManager tsm = new TimeStampManager(null);
            return tsm.SyncOneEmployeeSchedule(accountDimId, employeeId, startDate, stopDate);
        }

        [HttpGet]
        [Route("SyncDeviationCause")]
        [ResponseType(typeof(List<TSTimeDeviationCauseItem>))]

        public List<TSTimeDeviationCauseItem> SyncDeviationCause(int actorCompanyId, DateTime? prevSyncDate)
        {
            TimeStampManager tsm = new TimeStampManager(null);
            return tsm.SyncDeviationCause(actorCompanyId, prevSyncDate).ToList();
        }

        [HttpGet]
        [Route("SyncTimeCode")]
        [ResponseType(typeof(List<TSTimeCodeItem>))]
        public IEnumerable<TSTimeCodeItem> SyncTimeCode(int actorCompanyId, DateTime? prevSyncDate)
        {
            TimeStampManager tsm = new TimeStampManager(null);
            return tsm.SyncTimeCode(actorCompanyId, prevSyncDate);
        }

        [HttpGet]
        [Route("SyncTimeStampEntries")]
        [ResponseType(typeof(List<TSTimeStampEntryItem>))]
        public List<TSTimeStampEntryItem> SyncTimeStampEntries(int actorCompanyId, DateTime prevSyncDate, int terminalId)
        {
            TimeStampManager tsm = new TimeStampManager(null);
            return tsm.SyncTimeStampEntry(actorCompanyId, prevSyncDate, terminalId);
        }


        [HttpPost]
        [Route("SyncNewTimeStampEntriesV2")]
        [ResponseType(typeof(ActionResult))]
        public ActionResult SyncNewTimeStampEntriesV2(List<TSTimeStampEntryItem> entryItems, int timeTerminalId, int actorCompanyId, int accountDimId)
        {
            TimeStampManager tsm = new TimeStampManager(null);
            var result = new ActionResult();
            result.IntDict = tsm.SyncNewTimeStampEntries(entryItems, timeTerminalId, accountDimId, actorCompanyId, 0);
            result.DateTimeValue = DateTime.Now;

            return result;
        }

        [HttpPost]
        [Route("SyncNewEmployees")]
        [ResponseType(typeof(Dictionary<int, int>))]
        public Dictionary<int, int> SyncNewEmployees(List<TSEmployeeItem> employees, int timeTerminalId, int actorCompanyId)
        {
            TimeStampManager tsm = new TimeStampManager(null);
            return tsm.SyncNewEmployees(employees, timeTerminalId, actorCompanyId);
        }


        [HttpGet]
        [Route("GetTimeAccumulator")]
        [ResponseType(typeof(List<TSTimeAccumulatorEmployeeItem>))]
        public List<TSTimeAccumulatorEmployeeItem> GetTimeAccumulator(int actorCompanyId, int employeeId, int employeeGroupId, DateTime startDate, DateTime stopDateTime, string cultureName)
        {
            SetLanguage(cultureName);

            TimeStampManager tsm = new TimeStampManager(null);
            return tsm.GetTimeAccumulator(actorCompanyId, employeeId, employeeGroupId, startDate, stopDateTime).ToList();
        }


        [HttpGet]
        [Route("GetTimeAccumulatorV2")]
        [ResponseType(typeof(List<TSTimeAccumulatorEmployeeItem>))]
        public List<TSTimeAccumulatorEmployeeItem> GetTimeAccumulatorV2(int actorCompanyId, int employeeId, int employeeGroupId, DateTime startDate, DateTime stopDateTime, string cultureName, int timeTerminalId)
        {
            SetLanguage(cultureName);

            TimeStampManager tsm = new TimeStampManager(null);
            return tsm.GetTimeAccumulator(actorCompanyId, employeeId, employeeGroupId, startDate, stopDateTime, timeTerminalId).ToList();
        }


        [HttpPost]
        [Route("SaveTimeStampEntry")]
        [ResponseType(typeof(ActionResult))]
        public ActionResult SaveTimeStampEntry(TSTimeStampEntryItem entryItem, int timeTerminalId, int actorCompanyId, int accountDimId)
        {
            TimeStampManager tsm = new TimeStampManager(null);
            var dict = tsm.SyncNewTimeStampEntries(new List<TSTimeStampEntryItem>() { entryItem }, timeTerminalId, accountDimId, actorCompanyId, 0);
            var result = new ActionResult()
            {
                IntDict = dict,
                DateTimeValue = DateTime.Now,
            };

            return result;
        }

        #endregion
    }
}
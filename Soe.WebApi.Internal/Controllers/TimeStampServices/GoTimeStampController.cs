using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.DataCache;
using SoftOne.Soe.Business.Util.LogCollector;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Caching;
using System.Threading;
using System.Web.Http;
using System.Web.Http.Description;

namespace Soe.Api.Internal.Controllers.TimeStampServices
{
    [RoutePrefix("TimeStampServices/GoTimeStamp")]
    public class GoTimeStampController : ApiBase
    {
        #region Variables

        private GoTimeStampManager gtsm;

        #endregion

        #region Enums

        public enum EmployeeStatus
        {
            All,
            StampedIn
        }

        #endregion

        #region Constructor

        public GoTimeStampController(WebApiInternalParamObject webApiInternalParamObject) : base(webApiInternalParamObject)
        {
            this.gtsm = new GoTimeStampManager(null);
        }

        #endregion

        #region Status

        [HttpGet]
        [Route("Handshake")]
        [ResponseType(typeof(bool))]
        public IHttpActionResult Handshake()
        {
            return this.Content(HttpStatusCode.OK, true);
        }

        #endregion

        #region Account

        [HttpGet]
        [Route("{actorCompanyId:int}/{timeTerminalId:int}/{employeeId:int}/{dimNr:int}/account")]
        [ResponseType(typeof(List<GoTimeStampAccount>))]
        public IHttpActionResult GetAccounts(int actorCompanyId, int timeTerminalId, int employeeId, int dimNr)
        {
            List<Account> accounts = gtsm.GetAccounts(actorCompanyId, timeTerminalId, employeeId, dimNr);

            return this.Content(HttpStatusCode.OK, accounts.ToGoTimeStampDTOs());
        }

        #endregion

        #region DeviationCause

        [HttpGet]
        [Route("{actorCompanyId:int}/{timeTerminalId:int}/deviationCause")]
        [ResponseType(typeof(List<GoTimeStampDeviationCause>))]
        public IHttpActionResult SyncDeviationCause(int actorCompanyId, int timeTerminalId)
        {
            List<TimeDeviationCause> timeDeviationCauses = gtsm.SyncDeviationCauses(actorCompanyId, timeTerminalId);

            return this.Content(HttpStatusCode.OK, timeDeviationCauses.ToGoTimeStampDTOs());
        }

        [HttpGet]
        [Route("{actorCompanyId:int}/{timeTerminalId:int}/{employeeId:int}/deviationCause")]
        [ResponseType(typeof(List<GoTimeStampDeviationCause>))]
        public IHttpActionResult GetCurrentDeviationCauses(int actorCompanyId, int timeTerminalId, int employeeId, DateTime? localTime = null)
        {
            List<TimeDeviationCause> timeDeviationCauses = gtsm.GetCurrentDeviationCauses(actorCompanyId, timeTerminalId, employeeId, localTime ?? DateTime.Now);

            return this.Content(HttpStatusCode.OK, timeDeviationCauses.ToGoTimeStampDTOs());
        }

        #endregion

        #region Employee

        private static readonly ConcurrentDictionary<int, SemaphoreSlim> _semaphores = new ConcurrentDictionary<int, SemaphoreSlim>();

        [HttpGet]
        [Route("{actorCompanyId:int}/{timeTerminalId:int}/employee")]
        [ResponseType(typeof(GoTimeStampSyncEmployeeResult))]
        public IHttpActionResult GetEmployees(int actorCompanyId, int timeTerminalId, DateTime? prevSyncDate = null, bool? includeLastEmployeeGroup = false, bool? includeDeviationCauses = false, int? employeeId = null)
        {
            if (employeeId.HasValidValue())
            {
                var cacheKey = $"Employees_{actorCompanyId}_{timeTerminalId}_{employeeId.Value}";

                var cachedResult = BusinessMemoryCache<GoTimeStampSyncEmployeeResult>.Get(cacheKey);
                if (cachedResult != null)
                    return Content(HttpStatusCode.OK, cachedResult);

                var semaphore = _semaphores.GetOrAdd(employeeId.Value, new SemaphoreSlim(1, 1));
                semaphore.Wait();
                try
                {
                    // Check cache again in case another thread populated it while this thread was waiting.
                    cachedResult = BusinessMemoryCache<GoTimeStampSyncEmployeeResult>.Get(cacheKey);
                    if (cachedResult != null)
                        return Content(HttpStatusCode.OK, cachedResult);

                    var result = gtsm.SyncEmployees(actorCompanyId, timeTerminalId, includeLastEmployeeGroup, includeDeviationCauses, employeeId);
                    BusinessMemoryCache<GoTimeStampSyncEmployeeResult>.Set(cacheKey, result, 5);
                    return Content(HttpStatusCode.OK, result);
                }
                finally
                {
                    semaphore.Release();
                }
            }
            else
            {
                return Content(HttpStatusCode.OK, gtsm.SyncEmployees(actorCompanyId, timeTerminalId, includeLastEmployeeGroup, includeDeviationCauses, employeeId));
            }
        }


        [HttpGet]
        [Route("{actorCompanyId:int}/{timeTerminalId:int}/employee/attendance")]
        [ResponseType(typeof(List<GoTimeStampEmployeeStampStatus>))]
        public IHttpActionResult GetTimeStampAttendance(int actorCompanyId, int timeTerminalId)
        {
            return this.Content(HttpStatusCode.OK, gtsm.GetTimeStampAttendance(actorCompanyId, timeTerminalId, false));
        }

        [HttpGet]
        [Route("{actorCompanyId:int}/{timeTerminalId:int}/employee/attendance/single")]
        [ResponseType(typeof(GoTimeStampEmployeeStampStatus))]
        public IHttpActionResult GetTimeStampAttendanceForEmployee(int actorCompanyId, int timeTerminalId, int employeeId)
        {
            return this.Content(HttpStatusCode.OK, gtsm.GetTimeStampAttendanceForEmployee(actorCompanyId, timeTerminalId, employeeId));
        }

        [HttpGet]
        [Route("{actorCompanyId:int}/{timeTerminalId:int}/employee/cardnumber")]
        [ResponseType(typeof(GoTimeStampEmployee))]
        public IHttpActionResult GetEmployeeByCardNumber(int actorCompanyId, int timeTerminalId, string cardNumber)
        {
            GoTimeStampEmployee employee = gtsm.GetEmployeeByCardNumber(actorCompanyId, timeTerminalId, cardNumber);
            if (employee == null)
                employee = new GoTimeStampEmployee();

            return this.Content(HttpStatusCode.OK, employee);
        }

        [HttpPost]
        [Route("{actorCompanyId:int}/{timeTerminalId:int}/employee/cardnumber")]
        public IHttpActionResult LinkEmployeeToCardNumber(int actorCompanyId, int timeTerminalId, GoTimeStampLinkEmployeeToCardNumber model)
        {
            ActionResult result = gtsm.LinkCardNumberToEmployee(actorCompanyId, timeTerminalId, model.CardNumber, model.EmployeeNr);
            return this.Content(HttpStatusCode.OK, result.ToGoTimeStampDTO());
        }

        #endregion

        #region Information

        [HttpGet]
        [Route("{actorCompanyId:int}/{timeTerminalId:int}/employee/information/unread")]
        [ResponseType(typeof(List<GoTimeStampInformation>))]
        public IHttpActionResult GetUnreadInformation(int actorCompanyId, int timeTerminalId, int employeeId)
        {
            List<InformationDTO> infos = gtsm.GetUnreadInformation(actorCompanyId, timeTerminalId, employeeId);

            return this.Content(HttpStatusCode.OK, infos.ToGoTimeStampDTOs());
        }

        [HttpGet]
        [Route("{actorCompanyId:int}/{timeTerminalId:int}/employee/information")]
        [ResponseType(typeof(List<GoTimeStampInformation>))]
        public IHttpActionResult GetInformation(int actorCompanyId, int timeTerminalId, int employeeId, int informationId, int sourceType)
        {
            InformationDTO info = gtsm.GetInformation(actorCompanyId, timeTerminalId, employeeId, informationId, (SoeInformationSourceType)sourceType);

            return this.Content(HttpStatusCode.OK, info.ToGoTimeStampDTO());
        }

        [HttpPost]
        [Route("{actorCompanyId:int}/{timeTerminalId:int}/employee/information/setasread/")]
        public IHttpActionResult SetInformationAsRead(int actorCompanyId, int timeTerminalId, GoTimeStampSetInformationAsRead model)
        {
            return this.Content(HttpStatusCode.OK, gtsm.SetInformationAsRead(actorCompanyId, timeTerminalId, model.EmployeeId, model.InformationId, model.SourceType, model.Confirmed));
        }

        #endregion

        #region Schedule

        [HttpGet]
        [Route("{actorCompanyId:int}/{timeTerminalId:int}/employee/schedules/current")]
        [ResponseType(typeof(List<GoTimeStampScheduleBlock>))]
        public IHttpActionResult GetCurrentSchedules(int actorCompanyId, int timeTerminalId)
        {
            return this.Content(HttpStatusCode.OK, gtsm.GetScheduleForEmployees(actorCompanyId, timeTerminalId));
        }

        [HttpGet]
        [Route("{actorCompanyId:int}/{timeTerminalId:int}/employee/schedule/current")]
        [ResponseType(typeof(List<GoTimeStampScheduleBlock>))]
        public IHttpActionResult GetCurrentSchedule(int actorCompanyId, int timeTerminalId, int employeeId)
        {
            return this.Content(HttpStatusCode.OK, gtsm.GetCurrentSchedule(actorCompanyId, employeeId, timeTerminalId));
        }

        [HttpGet]
        [Route("{actorCompanyId:int}/{timeTerminalId:int}/employee/schedule/next")]
        [ResponseType(typeof(List<GoTimeStampScheduleBlock>))]
        public IHttpActionResult GetNextSchedule(int actorCompanyId, int timeTerminalId, int employeeId)
        {
            return this.Content(HttpStatusCode.OK, gtsm.GetNextSchedule(actorCompanyId, employeeId, timeTerminalId));
        }

        #endregion

        #region Terminal

        [HttpGet]
        [Route("{actorCompanyId:int}/{timeTerminalId:int}/timeTerminal")]
        [ResponseType(typeof(GoTimeStampTerminal))]
        public GoTimeStampTerminal SyncTerminal(int actorCompanyId, int timeTerminalId, DateTime? prevSyncDate = null, bool includeSettings = false)
        {
            return gtsm.SyncTerminal(actorCompanyId, timeTerminalId, prevSyncDate, includeSettings);
        }

        [HttpGet]
        [Route("{actorCompanyId:int}/{timeTerminalId:int}/timeTerminal/settings/{type}")]
        [ResponseType(typeof(GoTimeStampTerminal))]
        public GoTimeStampTerminalSetting GetTerminalSetting(int actorCompanyId, int timeTerminalId, string type)
        {
            Enum.TryParse(type, out TimeTerminalSettingType settingsType);
            return gtsm.GetTerminalSetting(actorCompanyId, timeTerminalId, settingsType);
        }

        #endregion

        #region TimeAccumulator

        [HttpGet]
        [Route("{actorCompanyId:int}/{timeTerminalId:int}/employee/accumulator/break")]
        [ResponseType(typeof(GoTimeStampAccumulator))]
        public IHttpActionResult GetBreakAccumulator(int actorCompanyId, int timeTerminalId, int employeeId)
        {
            return this.Content(HttpStatusCode.OK, gtsm.GetBreakAccumulator(actorCompanyId, timeTerminalId, employeeId));
        }

        [HttpGet]
        [Route("{actorCompanyId:int}/{timeTerminalId:int}/employee/accumulator/other")]
        [ResponseType(typeof(List<GoTimeStampAccumulator>))]
        public IHttpActionResult GetOtherAccumulators(int actorCompanyId, int timeTerminalId, int employeeId)
        {
            GoTimeStampManager goTimeStampManager = new GoTimeStampManager(GetParameterObject(actorCompanyId, 0));
            return this.Content(HttpStatusCode.OK, goTimeStampManager.GetOtherAccumulators(actorCompanyId, timeTerminalId, employeeId));
        }

        #endregion

        #region TimeStamp

        [HttpPost]
        [Route("{actorCompanyId:int}/{timeTerminalId:int}/employee/timeStamps")]
        public IHttpActionResult AddTimeStamps(int actorCompanyId, int timeTerminalId, GoTimeStampTimeStamp[] timestamps)
        {
            List<GoTimeStampTimeStamp> timeStampEntryItems = new List<GoTimeStampTimeStamp>();

            // Sort list of stamps to make account swapping in correct order (first out, then in on new account)
            foreach (GoTimeStampTimeStamp stamp in timestamps.OrderBy(t => t.EmployeeId).ThenBy(t => t.TimeStamp).ThenByDescending(t => t.EntryType))
            {
                if (!timeStampEntryItems.Any(a => stamp.ToString() == a.ToString()))
                {
                    if (string.IsNullOrEmpty(BusinessMemoryCache<string>.Get("AddTimeStamps#" + stamp.ToString())))
                    {
                        BusinessMemoryCache<string>.Set("AddTimeStamps#" + stamp.ToString(), $"actorCompanyId", 30);
                        timeStampEntryItems.Add(stamp);
                    }
                    else
                    {
                        LogCollector.LogInfo($"Go TimeStamp duplicate in cache {stamp}");
                    }
                }
                else
                {
                    LogCollector.LogInfo($"Go TimeStamp duplicate in same request {stamp}");
                }
            }

            timeStampEntryItems.ForEach(f => f.SetExtendedData());
            GoTimeStampManager goTimeStampManager = new GoTimeStampManager(GetParameterObject(actorCompanyId, 0));
            List<GoTimeStampEmployeeStampStatus> employeeStatuses = goTimeStampManager.SynchGTSTimeStamps(timeStampEntryItems, timeTerminalId, actorCompanyId);
            return this.Content(HttpStatusCode.OK, employeeStatuses);
        }

        [HttpGet]
        [Route("{actorCompanyId:int}/{timeTerminalId:int}/employee/timeStamps/history")]
        [ResponseType(typeof(List<GoTimeStampTimeStamp>))]
        public IHttpActionResult GetTimeStampHistory(int actorCompanyId, int timeTerminalId, int employeeId, int nbrOfEntries, bool loadExtended)
        {
            return this.Content(HttpStatusCode.OK, gtsm.GetTimeStampHistory(actorCompanyId, timeTerminalId, employeeId, nbrOfEntries, loadExtended).ToGoTimeStampDTOs());
        }

        #endregion

        #region TimeStampAddition

        [HttpGet]
        [Route("{actorCompanyId:int}/{timeTerminalId:int}/timeStampAddition")]
        [ResponseType(typeof(List<GoTimeStampTimeStampAddition>))]
        public IHttpActionResult GetTimeStampAdditions(int actorCompanyId, int timeTerminalId)
        {
            List<TimeStampAdditionDTO> timeStampAdditions = gtsm.GetTimeStampAdditions(actorCompanyId, timeTerminalId);

            return this.Content(HttpStatusCode.OK, timeStampAdditions.ToGoTimeStampDTOs());
        }

        #endregion
    }
}
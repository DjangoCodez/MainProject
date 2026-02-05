using SoftOne.Soe.Business.Core;
using System;
using System.Net;
using System.Web.Http;
using System.Web.Http.Description;


namespace Soe.Api.Internal.Controllers
{
    [RoutePrefix("TimeStampService/TimeSpot")]

    public class TimeSpotController : ApiBase
    {
        #region Variables

        private TimeSpotManager timeSpotManager;

        #endregion

        #region Constructor

        public TimeSpotController(WebApiInternalParamObject webApiInternalParamObject) : base(webApiInternalParamObject)
        {
            this.timeSpotManager = new TimeSpotManager();
        }

        #endregion

        [HttpGet]
        [Route("HelloWorld")]
        [ResponseType(typeof(string))]
        public IHttpActionResult HelloWorld(string question)
        {
            return Content(HttpStatusCode.OK, question);
        }

        [HttpGet]
        [Route("ValidateControlInfo")]
        [ResponseType(typeof(int))]
        public IHttpActionResult ValidateControlInfo(string guid)
        {
            int retval = timeSpotManager.ValidateControlInfo(guid);
            return Content(HttpStatusCode.OK, retval);
        }

        [HttpGet]
        [Route("ValidateCompanyNumber")]
        [ResponseType(typeof(string))]
        public IHttpActionResult ValidateCompanyNumber(string coNumber)
        {
            string retval = timeSpotManager.ValidateCompanyNumer(coNumber);
            return Content(HttpStatusCode.OK, retval);
        }

        [HttpGet]
        [Route("CheckEmployeeChanges")]
        [ResponseType(typeof(int))]
        public IHttpActionResult CheckEmployeeChanges(string guid, DateTime senasteKontroll)
        {
            int retval = timeSpotManager.CheckEmployeeChanges(guid, senasteKontroll);
            return Content(HttpStatusCode.OK, retval);
        }

        [HttpGet]
        [Route("CheckAccChanges")]
        [ResponseType(typeof(int))]
        public IHttpActionResult CheckAccChanges(string guid, DateTime senasteKontroll)
        {
            int retval = timeSpotManager.CheckAccChanges(guid, senasteKontroll);
            return Content(HttpStatusCode.OK, retval);
        }

        [HttpGet]
        [Route("SyncEmployeeInfo")]
        [ResponseType(typeof(string[]))]
        public IHttpActionResult SyncEmployeeInfo(string guid, DateTime senasteKontroll, string anstnr)
        {
            string[] retval = timeSpotManager.GetEmployeeInfo(guid, senasteKontroll, anstnr);
            return Content(HttpStatusCode.OK, retval);
        }

        [HttpGet]
        [Route("SyncAccInfo")]
        [ResponseType(typeof(string[]))]
        public IHttpActionResult SyncAccInfo(string guid, DateTime senasteKontroll, string Anstnr)
        {
            string[] retval = timeSpotManager.GetAccInfo(guid, senasteKontroll, Anstnr);
            return Content(HttpStatusCode.OK, retval);
        }

        [HttpGet]
        [Route("GetTerminalTransaction")]
        [ResponseType(typeof(int))]
        public IHttpActionResult GetTerminalTransaction(string guid, string trans)
        {
            int retval = timeSpotManager.GetTerminalTransaction(guid, trans);
            return Content(HttpStatusCode.OK, retval);
        }

        [HttpGet]
        [Route("SyncTransactions")]
        [ResponseType(typeof(string[]))]
        public IHttpActionResult SyncTransactions(string guid, string anstnr, int antal)
        {
            string[] retval = timeSpotManager.GetTransactions(guid, anstnr, antal);
            return Content(HttpStatusCode.OK, retval);
        }

        [HttpGet]
        [Route("SyncTimeCodes")]
        [ResponseType(typeof(string[]))]
        public IHttpActionResult SyncTimeCodes(string guid)
        {
            string[] retval = timeSpotManager.GetTimeCodes(guid);
            return Content(HttpStatusCode.OK, retval);
        }

        [HttpGet]
        [Route("SyncEmployeeCard")]
        [ResponseType(typeof(int))]
        public IHttpActionResult SyncEmployeeCard(string guid, string anstnr, string kortNr)
        {
            int retval = timeSpotManager.SetEmployeeCardNumer(guid, anstnr, kortNr);
            return Content(HttpStatusCode.OK, retval);
        }

        [HttpGet]
        [Route("SyncMachine")]
        [ResponseType(typeof(string))]
        public IHttpActionResult SyncMachine(string guid, string mac, string name, int id)
        {
            int retval = timeSpotManager.SetMachineIdentity(guid, mac, ref name, ref id);
            return Content(HttpStatusCode.OK, retval.ToString() + "#" + name + "#" + id.ToString());
        }

        [HttpGet]
        [Route("SyncMachineTest")]
        [ResponseType(typeof(int))]
        public IHttpActionResult SyncMachineTest(string guid, string mac)
        {
            string name = string.Empty;
            int id = 0;
            int retval = timeSpotManager.SetMachineIdentity(guid, mac, ref name, ref id);
            return Content(HttpStatusCode.OK, retval.ToString() + "#" + name + "#" + id.ToString());
        }

        [HttpGet]
        [Route("CheckTimeCodeEmp")]
        [ResponseType(typeof(string[]))]
        public IHttpActionResult CheckTimeCodeEmp(string guid, string anstnr)
        {
            string[] retval = timeSpotManager.GetEmpTimeCodes(guid, anstnr);
            return Content(HttpStatusCode.OK, retval);
        }

        [HttpGet]
        [Route("CheckTimeCodeEmpAll")]
        [ResponseType(typeof(string[]))]
        public IHttpActionResult CheckTimeCodeEmpAll(string guid, DateTime senasteKontroll)
        {
            string[] retval = timeSpotManager.CheckEmpTimeCodes(guid, senasteKontroll);
            return Content(HttpStatusCode.OK, retval);
        }

        [HttpGet]
        [Route("SyncSchedule")]
        [ResponseType(typeof(string[]))]
        public IHttpActionResult SyncSchedule(string guid, string anstNr, DateTime datumfr, DateTime datumto, DateTime syncTime)
        {
            string[] retval = timeSpotManager.GetSchedule(guid, anstNr, datumfr, datumto, syncTime);
            return Content(HttpStatusCode.OK, retval);
        }
    }
}
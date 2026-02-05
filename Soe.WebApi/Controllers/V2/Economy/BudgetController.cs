using Soe.WebApi.Controllers;
using SoftOne.Soe.Business.Core;
using System;
using System.Collections.Generic;
using System.Net;
using System.Web.Http;
using SoftOne.Soe.Data;
using SoftOne.Soe.Common.DTO;
using Soe.WebApi.Models;
using SoftOne.Soe.Common.Util;
using System.Globalization;
using System.Threading;

namespace Soe.WebApi.V2.Economy
{
    [RoutePrefix("V2/Economy/Budget")]
    public class BudgetController : SoeApiController
    {
        private readonly BudgetManager bm;

        public BudgetController(BudgetManager bm)
        {
            this.bm = bm;
        }

        #region Budget

        [HttpGet]
        [Route("Budget/{budgetType:int}/{actorCompanyId:int}")]
        public IHttpActionResult GetBudgetList(
            int budgetType, int actorCompanyId, int? budgetHeadId = null)
        {
            return Content(HttpStatusCode.OK, bm.GetBudgetHeadForGrid(
                actorCompanyId != 0 ? actorCompanyId : base.ActorCompanyId, budgetType, budgetHeadId));
        }

        [HttpGet]
        [Route("BudgetHead/{budgetHeadId:int}/{loadRows:bool}")]
        public IHttpActionResult GetBudget(int budgetHeadId, bool loadRows)
        {
            return Content(HttpStatusCode.OK, bm.GetBudgetHeadIncludingRows(budgetHeadId).ToFlattenedDTO());
        }

        [HttpPost]
        [Route("Budget")]
        public IHttpActionResult SaveBudgetHead(BudgetHeadFlattenedDTO dto)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, bm.SaveBudgetHeadFlattened(dto));
        }

        [HttpDelete]
        [Route("Budget/{budgetHeadId:int}")]
        public IHttpActionResult DeleteBudget(int budgetHeadId)
        {
            return Content(HttpStatusCode.OK, bm.DeleteBudgetHead(budgetHeadId));
        }

        [HttpPost]
        [Route("Budget/Result")]
        public IHttpActionResult GetBalanceChangePerPeriod(GetResultPerPeriodModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
            {
                int actorCompanyId = base.ActorCompanyId;
                Guid guid = Guid.Parse(model.Key);
                CultureInfo cultureInfo = Thread.CurrentThread.CurrentCulture;
                var workingThread = new Thread(() => GetBalanceChangePerPeriodThreading(cultureInfo, guid, model.NoOfPeriods, model.AccountYearId, model.AccountId, actorCompanyId, model.GetPrevious, model.Dims));
                workingThread.Start();
                return Content(HttpStatusCode.OK, new SoeProgressInfo(guid));
            }
        }

        [HttpGet]
        [Route("Budget/Result/{key}")]
        public IHttpActionResult GetBalanceChangeResult(Guid key)
        {
            return Content(HttpStatusCode.OK, bm.ConvertResultToBudgetRow(monitor.GetResult(key) as IEnumerable<BudgetBalanceDTO>));
        }

        private void GetBalanceChangePerPeriodThreading(CultureInfo cultureInfo, Guid key, int noOfPeriods, int accountYearId, int accountId, int actorCompanyId, bool getPrevious, List<int> dims)
        {
            SetLanguage(cultureInfo);

            SoeProgressInfo info = monitor.RegisterNewProgressProcess(key);
            bm.GetBalanceChangePerPeriod(noOfPeriods, accountYearId, accountId, actorCompanyId, DateTime.Today, getPrevious, dims, ref info, monitor);
        }

        #endregion
    }
}
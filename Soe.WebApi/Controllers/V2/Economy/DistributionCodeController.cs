using Soe.WebApi.Controllers;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System.Net;
using System.Web.Http;
using SoftOne.Soe.Business.Util;
using System;

namespace Soe.WebApi.V2.Time
{
    [RoutePrefix("V2/Economy/Accounting")]
    public class DistributionCodeController : SoeApiController
    {
        #region Variables

        private readonly BudgetManager bm;

        #endregion

        #region Constructor

        public DistributionCodeController(BudgetManager bm)
        {
            this.bm = bm;
        }

        #endregion

        #region DistributionCode

        [HttpGet]
        [Route("DistributionCode/Grid/{distributionId:int?}")]
        public IHttpActionResult GetDistributionCodesGrid(int? distributionId = null)
        {
            return Content(HttpStatusCode.OK, bm.GetDistributionCodesForGrid(base.ActorCompanyId, distributionId));
        }

        [HttpGet]
        [Route("DistributionCode")]
        public IHttpActionResult GetDistributionCodes(bool includePeriods, int? budgetType = null, DateTime? fromDate = null, DateTime? toDate = null)
        {
            return Content(HttpStatusCode.OK, bm.GetDistributionCodes(base.ActorCompanyId, includePeriods, true, (DistributionCodeBudgetType?)budgetType, fromDate, toDate).ToDTOs());
        }

        [HttpGet]
        [Route("DistributionCode/Dict")]
        public IHttpActionResult GetDistributionCodesDict(bool addEmptyRow)
        {
            return Content(HttpStatusCode.OK, bm.GetDistributionCodeDict(base.ActorCompanyId, addEmptyRow).ToSmallGenericTypes());
        }

        [HttpGet]
        [Route("DistributionCodesByType/{distributionCodeType:int}/{loadPeriods:bool}")]
        public IHttpActionResult GetDistributionCodesByType(int distributionCodeType, bool loadPeriods)
        {
            return Content(HttpStatusCode.OK, bm.GetDistributionCodesByType(base.ActorCompanyId, distributionCodeType, loadPeriods).ToDTOs());
        }

        [HttpGet]
        [Route("DistributionCode/{distributionCodeHeadId}")]
        public IHttpActionResult GetDistributionCode(int distributionCodeHeadId)
        {
            return Content(HttpStatusCode.OK, bm.GetDistributionCode(base.ActorCompanyId, distributionCodeHeadId).ToDTO(true));
        }

        [HttpPost]
        [Route("DistributionCode")]
        public IHttpActionResult SaveDistributionCode(DistributionCodeHeadDTO model)
        {
            return Content(HttpStatusCode.OK, bm.SaveDistributionCode(base.ActorCompanyId, model));
        }

        [HttpDelete]
        [Route("DistributionCode/{distributionCodeHeadId}")]
        public IHttpActionResult DeleteDistributionCode(int distributionCodeHeadId)
        {
            return Content(HttpStatusCode.OK, bm.DeleteDistributionCode(base.ActorCompanyId, distributionCodeHeadId));
        }

        #endregion DistributionCode
    }
}
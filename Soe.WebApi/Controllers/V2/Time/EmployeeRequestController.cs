using Soe.WebApi.Controllers;
using SoftOne.Soe.Business.Core.TimeEngine;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;

namespace Soe.WebApi.V2.Time
{
    [RoutePrefix("V2/Time/Schedule/EmployeeRequest")]
    public class EmployeeRequestController : SoeApiController
    {
        #region Variables

        private readonly TimeEngineManager tem;

        #endregion

        #region Constructor

        public EmployeeRequestController(TimeEngineManager tem)
        {
            this.tem = tem;
        }

        #endregion

        #region Availability

        [HttpGet]
        [Route("Grid/{fromDate:datetime}/{toDate:datetime}/{employeeRequestId:int?}")]
        public IHttpActionResult GetEmployeeRequestsGrid(DateTime fromDate, DateTime toDate, int? employeeRequestId = null)
        {
            var requests = tem.GetEmployeeRequests(
                0,
                employeeRequestId,
                new List<TermGroup_EmployeeRequestType>()
                {
                    TermGroup_EmployeeRequestType.InterestRequest,
                    TermGroup_EmployeeRequestType.NonInterestRequest
                },
                fromDate,
                toDate
            );

            return Content(HttpStatusCode.OK, requests.ToGridDTOs());
        }

        #endregion
    }
}

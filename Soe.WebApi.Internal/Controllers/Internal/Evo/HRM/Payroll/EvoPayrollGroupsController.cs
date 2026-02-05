using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.Description;
using Evonizer.Shared.Compare;

namespace Soe.Api.Internal.Controllers.Internal.Evo.HRM.Payroll
{
    [RoutePrefix("Internal/Evo/HRM/Payroll/PayrollGroups")]
    public class EvoPayrollGroupsController : ApiBase
    {
        #region Ctor
        public EvoPayrollGroupsController(WebApiInternalParamObject webApiInternalParamObject) : base(webApiInternalParamObject)
        {
        }
        #endregion

        #region API

        [HttpPost]
        [Route("Compare/{actorCompanyId:int}")]
        [ResponseType(typeof(CompareCollectionResponse))]
        public IHttpActionResult Compare(int actorCompanyId,  CompareCollectionRequest request)
        {
            if (request == null || request.Left == null)
                return Content(HttpStatusCode.BadRequest, "Request or Records cannot be null");

            var parameterObject = GetParameterObject(actorCompanyId, 0);
            var payrollManager = new PayrollManager(parameterObject);

            // Load local payroll groups
            var localGroups = payrollManager.GetPayrollGroups(actorCompanyId, loadTimePeriods: false, onlyActive: false) ?? new List<PayrollGroup>();

            var collection = localGroups.Select(s => new CompareCollectionItem() { Key = s.PayrollGroupId.ToString(), Items = CompareHelper.FromObject(s) }).ToList();

            request.Left = collection;
            var response = DtoComparer.Compare(request);

            return Content(HttpStatusCode.OK, response);
        }
        #endregion
    }
}
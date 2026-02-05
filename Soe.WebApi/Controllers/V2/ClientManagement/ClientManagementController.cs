using Soe.Sys.Common.DTO;
using Soe.WebApi.Controllers;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Common.Util;
using System.Linq;
using System.Net;
using System.Web.Http;

namespace Soe.WebApi.V2.ClientManagement
{
    [RoutePrefix("V2/Shared/ClientManagement")]
    public class ClientManagementController : SoeApiController
    {
        #region Variables

        private readonly ClientManagementManager cmm;
        private readonly FeatureManager fm;
        private readonly ClientManagementAggregatorManager cmam;

		#endregion

		#region Constructor
		public ClientManagementController(ClientManagementManager cmm, FeatureManager fm, ClientManagementAggregatorManager cmam)
        {
            this.cmm = cmm;
            this.fm = fm;
            this.cmam = cmam;
        }
        #endregion

        #region ConnectionRequest
        [HttpPost]
        [Route("ConnectionRequest")]
        public IHttpActionResult InitConnectionRequest()
        {
            if (!fm.HasRolePermission(Feature.ClientManagement_Clients, Permission.Modify, base.RoleId, base.ActorCompanyId, base.LicenseId))
            {
                return Content(HttpStatusCode.Forbidden, new ActionResult(false));
            }

            return Content(HttpStatusCode.OK, cmm.CreateMultiCompanyConnectionRequest());
        }

        [HttpGet]
        [Route("ConnectionRequest/{connectionRequestId:int}/Status")]
        public IHttpActionResult GetRequestStatus(int connectionRequestId)
        {
            if (!fm.HasRolePermission(Feature.ClientManagement_Clients, Permission.Modify, base.RoleId, base.ActorCompanyId, base.LicenseId) || base.CompanyGuid is null)
            {
                return Content(HttpStatusCode.Forbidden, new ActionResult(false));
            }

            return Content(HttpStatusCode.OK, cmm.GetCompanyRequestStatus(base.CompanyGuid.Value, connectionRequestId));
        }

        #endregion

        [HttpGet]
        [Route("Clients")]
        public IHttpActionResult GetClients()
        {
            if (!fm.HasRolePermission(Feature.ClientManagement_Clients, Permission.Readonly, base.RoleId, base.ActorCompanyId, base.LicenseId) || base.CompanyGuid is null)
            {
                return Content(HttpStatusCode.Forbidden, Enumerable.Empty<TargetCompanyDTO>());
            }

            return Content(HttpStatusCode.OK, cmm.GetTargetCompanies(base.CompanyGuid.Value));
        }

        [HttpGet]
        [Route("Suppliers/Invoices/Overview")]
        public IHttpActionResult GetSupplierInvoiceOverview()
        {
#if DEBUG
            SessionCache.ReloadCompany(base.ActorCompanyId);
            ParameterObject.SetSoeCompany(SessionCache.GetCompanyFromCache(base.ActorCompanyId));
#endif

			if (!fm.HasRolePermission(Feature.ClientManagement_Supplier_Invoices, Permission.Readonly, base.RoleId, base.ActorCompanyId, base.LicenseId) || base.CompanyGuid is null)
            {
                return Content(HttpStatusCode.Forbidden, new ActionResult(false));
            }
            return Content(HttpStatusCode.OK, cmam.GetSupplierInvoiceSummaryAggregatedData(base.ParameterObject.CompanyGuid.Value));
		}
	}
}
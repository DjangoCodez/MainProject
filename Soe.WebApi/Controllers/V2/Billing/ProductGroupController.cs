using System.Net;
using System.Web.Http;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.DTO;
using Soe.WebApi.Controllers;


namespace Soe.WebApi.V2.Billing
{
    [RoutePrefix("V2/Billing/ProductGroups")]
    public class ProductGroupController : SoeApiController
    {
        #region Variables

        private readonly InvoiceManager im;

        #endregion

        #region Constructor

        public ProductGroupController(InvoiceManager im)
        {
            this.im = im;
        }

        #endregion

        #region ProductGroup

        [HttpGet]
        [Route("Grid/{productGroupId:int?}")]
        public IHttpActionResult GetProductGroupsGrid(int? productGroupId = null)
        {
            return Content(HttpStatusCode.OK, im.GetProductGroups(base.ActorCompanyId, false, productGroupId));
        }

        [HttpGet]
        [Route("")]
        public IHttpActionResult GetProductGroups()
        {
            return Content(HttpStatusCode.OK, im.GetProductGroups(base.ActorCompanyId, false));
        }

        [HttpGet]
        [Route("{productGroupId:int}")]
        public IHttpActionResult GetProductGroup(int productGroupId)
        {
            return Content(HttpStatusCode.OK, im.GetProductGroup(productGroupId));
        }

        [HttpPost]
        [Route("ProductGroup")]
        public IHttpActionResult SaveProductGroup(ProductGroupDTO productGroupDTO)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, im.SaveProductGroup(productGroupDTO, base.ActorCompanyId));
        }

        [HttpDelete]
        [Route("{productGroupId:int}")]
        public IHttpActionResult DeleteProductGroup(int productGroupId)
        {
            return Content(HttpStatusCode.OK, im.DeleteProductGroup(productGroupId));
        }

        #endregion
    }
}
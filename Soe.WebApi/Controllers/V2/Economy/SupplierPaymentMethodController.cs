using System.Net;
using System.Linq;
using System.Web.Http;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System.Collections.Generic;
using Soe.WebApi.Models;
using Soe.WebApi.Controllers;
using System.Net.Http;
using Soe.WebApi.Extensions;

namespace Soe.WebApi.V2.Economy
{
    [RoutePrefix("V2/Economy/SupplierPaymentMethod")]
    public class SupplierPaymentMethodController : SoeApiController
    {
        #region Variables

        private readonly PaymentManager pm;
        private readonly SoeOriginType originalPaymentType;

        #endregion

        #region Constructor

        public SupplierPaymentMethodController(PaymentManager pm)
        {
            this.pm = pm;
            this.originalPaymentType = SoeOriginType.SupplierPayment;
        }

        #endregion

        #region PaymentInformation

        [HttpGet]
        [Route("PaymentInformation/{addEmptyRow:bool}")]
        public IHttpActionResult GetPaymentInformationViewsDict(bool addEmptyRow)
        {
            return Content(HttpStatusCode.OK, pm.GetPaymentInformationViewsDict(base.ActorCompanyId, addEmptyRow).ToSmallGenericTypes());
        }

        [HttpGet]
        [Route("PaymentInformation/Small/{addEmptyRow:bool}")]
        public IHttpActionResult GetPaymentInformationViewsSmall(bool addEmptyRow)
        {
            return Content(HttpStatusCode.OK, pm.GetPaymentInformationViewsSmall(base.ActorCompanyId, addEmptyRow));
        }

        [HttpGet]
        [Route("PaymentInformation/{supplierId:int}")]
        public IHttpActionResult GetPaymentInformationViews(int supplierId)
        {
            return Content(HttpStatusCode.OK, pm.GetPaymentInformationViews(supplierId, true));
        }

        #endregion

        #region PaymentMethod
        [HttpGet]
        [Route("PaymentMethodSupplierGrid/{addEmptyRow:bool}/{includePaymentInformationRows:bool}/{includeAccount:bool}/{onlyCashSales:bool}/{paymentMethodId:int?}")]
        public IHttpActionResult GetPaymentMethodsSupplierGrid(bool addEmptyRow, bool includePaymentInformationRows, bool includeAccount, bool onlyCashSales, int? paymentMethodId = null)
        {
            return Content(HttpStatusCode.OK, pm.GetPaymentMethods(this.originalPaymentType, base.ActorCompanyId, addEmptyRow, onlyCashSales, includeAccount, paymentMethodId).ToSupplierGridDTOs(includePaymentInformationRows, includeAccount));
        }

        [HttpGet]
        [Route("PaymentMethod/{addEmptyRow:bool}/{includePaymentInformationRows:bool}/{includeAccount:bool}/{onlyCashSales:bool}")]
        public IHttpActionResult GetPaymentMethods( bool addEmptyRow, bool includePaymentInformationRows, bool includeAccount, bool onlyCashSales)
        {
            return Content(HttpStatusCode.OK, pm.GetPaymentMethods(originalPaymentType, base.ActorCompanyId, addEmptyRow, onlyCashSales).ToDTOs(includePaymentInformationRows, includeAccount));
        }

        [HttpGet]
        [Route("PaymentMethod/{paymentMethodId:int}/{loadAccount:bool}/{loadPaymentInformationRow:bool}")]
        public IHttpActionResult GetPaymentMethod(int paymentMethodId, bool loadAccount, bool loadPaymentInformationRow)
        {
            return Content(HttpStatusCode.OK, pm.GetPaymentMethod(paymentMethodId, base.ActorCompanyId, loadAccount).ToDTO(loadPaymentInformationRow, loadAccount));
        }

        [HttpPost]
        [Route("PaymentMethod")]
        public IHttpActionResult SavePaymentMethod(PaymentMethodDTO paymentMethod)
        {
            paymentMethod.PaymentType = originalPaymentType;
            return Content(HttpStatusCode.OK, pm.SavePaymentMethod(paymentMethod, base.ActorCompanyId));
        }

        [HttpDelete]
        [Route("PaymentMethod/{paymentMethodId:int}")]
        public IHttpActionResult DeletePaymentMethod(int paymentMethodId)
        {
            return Content(HttpStatusCode.OK, pm.DeletePaymentMethod(paymentMethodId, base.ActorCompanyId));
        }

        #endregion

        #region SysPaymentMethod

        [HttpGet]
        [Route("SysPaymentMethod/{addEmptyRow:bool}")]
        public IHttpActionResult GetSysPaymentMethodsDict(bool addEmptyRow)
        {
            return Content(HttpStatusCode.OK, pm.GetSysPaymentMethodsDict(originalPaymentType, addEmptyRow).ToSmallGenericTypes());
        }

        #endregion
    }
}
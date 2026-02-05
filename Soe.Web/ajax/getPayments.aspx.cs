using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.DTO;
using System;

namespace SoftOne.Soe.Web.ajax
{
    public partial class getPayments : JsonBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            PaymentManager pm = new PaymentManager(ParameterObject);
            int paymentInformationId;
            if (Int32.TryParse(QS["pi"], out paymentInformationId))
            {
                PaymentInformationViewDTO paymentInformationView = pm.GetPaymentInformationView(paymentInformationId, SoeCompany.ActorCompanyId);
                if (paymentInformationView != null)
                {
                    ResponseObject = new
                    {
                        Found = true,
                        Name = paymentInformationView.Name,
                        PaymentNr = paymentInformationView.PaymentNr,
                    };
                }
            }

            if (ResponseObject == null)
            {
                ResponseObject = new
                {
                    Found = false
                };
            }
        }
    }
}

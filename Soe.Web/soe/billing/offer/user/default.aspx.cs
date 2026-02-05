using System;
using SoftOne.Soe.Common.Util;

namespace SoftOne.Soe.Web.soe.billing.offer.user
{
    public partial class _default : PageBase
    {
        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Billing_Offer_OffersUser;
            base.Page_Init(sender, e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            Response.Redirect("/soe/billing/offer/status/?classificationgroup=" + (int)SoeOriginStatusClassificationGroup.HandleOffers);
        }
    }
}

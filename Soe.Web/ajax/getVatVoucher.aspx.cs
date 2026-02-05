using SoftOne.Soe.Business.Core;
using System;

namespace SoftOne.Soe.Web.ajax
{
    public partial class getVatVoucher : JsonBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Int32.TryParse(QS["period"], out int accountPeriodId))
            {
                VoucherManager vm = new VoucherManager(ParameterObject);
                var vatVoucherHead = vm.GetVatVoucherHeadByPeriod(accountPeriodId, SoeCompany.ActorCompanyId);
                if (vatVoucherHead != null)
                {
                    ResponseObject = new
                    {
                        Found = true,
                        VatVoucherExists = true,
                        VatVoucherExistsLaterThanPeriod = false,
                        VoucherNr = vatVoucherHead.VoucherNr,
                        Date = vatVoucherHead.Date.ToShortDateString(),
                        Text = vatVoucherHead.Text != null ? vatVoucherHead.Text : String.Empty,
                    };
                }
                else
                {
                    vatVoucherHead = vm.GetVatVoucherHeadLaterThanPeriod(accountPeriodId, SoeCompany.ActorCompanyId);
                    if (vatVoucherHead != null)
                    {
                        ResponseObject = new
                        {
                            Found = true,
                            VatVoucherExists = false,
                            VatVoucherExistsLaterThanPeriod = true,
                            VoucherNr = vatVoucherHead.VoucherNr,
                            Date = vatVoucherHead.Date.ToShortDateString(),
                            Text = vatVoucherHead.Text != null ? vatVoucherHead.Text : String.Empty,
                        };
                    }
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
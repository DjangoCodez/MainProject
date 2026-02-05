using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Data;
using System;
using System.Collections;

namespace SoftOne.Soe.Web.ajax
{
    public partial class getVoucherNrLatest : JsonBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
			string serId = QS["serId"];
			if (!string.IsNullOrEmpty(serId))
			{
				VoucherManager vm = new VoucherManager(ParameterObject);
                VoucherSeries voucherSeries = vm.GetVoucherSerie(Int32.Parse(serId), SoeCompany.ActorCompanyId, true);

                Queue q = new Queue();
				q.Enqueue(new
				{
                    VoucherNrLatest = voucherSeries?.VoucherNrLatest ?? 0,
                    Template = voucherSeries?.VoucherSeriesType?.Template ?? false,
				});

				ResponseObject = q;
			}
        }
    }
}

using SoftOne.Soe.Business.Core;
using System;
using System.Collections;

namespace SoftOne.Soe.Web.ajax
{
    public partial class getTemplateVoucherSeries : JsonBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string ay = QS["ay"];
            if (!string.IsNullOrEmpty(ay))
            {
                VoucherManager vm = new VoucherManager(ParameterObject);
                int accountYear = Int32.Parse(ay);
                Queue q = new Queue();
                var v = vm.GetTemplateVoucherSerie(accountYear, SoeCompany.ActorCompanyId);
                if (v != null)
                {
                    q.Enqueue(new
                    {
                        VoucherSeriesId = v.VoucherSeriesId,
                        VoucherNrLatest = v.VoucherNrLatest,
                        Nr = v.VoucherSeriesType.VoucherSeriesTypeNr,
                        Name = v.VoucherSeriesType.Name,
                    });
                    ResponseObject = q;
                }
                else
                    ResponseObject = null;
            }
        }
    }
}

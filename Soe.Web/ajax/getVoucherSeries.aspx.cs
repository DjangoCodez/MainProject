using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Web.ajax
{
    public partial class getVoucherSeries : JsonBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Int32.TryParse(QS["ay"], out int accountYearIdFrom))
            {
                bool includeTemplate = StringUtility.GetBool(QS["inctemp"]);

                VoucherManager vm = new VoucherManager(ParameterObject);
                Queue q = new Queue();
                int position = 0;

                List<VoucherSeries> voucherSeries;
                if (Int32.TryParse(QS["ayto"], out int accountYearIdTo))
                    voucherSeries = vm.GetVoucherSeriesByYear(accountYearIdFrom, accountYearIdTo, SoeCompany.ActorCompanyId, includeTemplate, true);
                else
                    voucherSeries = vm.GetVoucherSeriesByYear(accountYearIdFrom, SoeCompany.ActorCompanyId, includeTemplate, true);

                foreach (var voucherSerie in voucherSeries.OrderBy(i => i.AccountYear.From).ThenBy(i => i.VoucherSeriesType.VoucherSeriesTypeNr))
                {
                    if (voucherSerie.VoucherSeriesType == null || voucherSerie.AccountYear == null)
                        continue;

                    q.Enqueue(new
                    {
                        Position = position,
                        VoucherSeriesId = voucherSerie.VoucherSeriesId,
                        VoucherNrLatest = voucherSerie.VoucherNrLatest,
                        Nr = voucherSerie.VoucherSeriesType.VoucherSeriesTypeNr,
                        Name = String.Format("{0} ({1}-{2}", voucherSerie.VoucherSeriesType.Name, voucherSerie.AccountYear.From.ToString("yyyyMMdd"), voucherSerie.AccountYear.To.ToString("yyyyMMdd")),
                    });

                    position++;
                }
                ResponseObject = q;
            }
        }
    }
}

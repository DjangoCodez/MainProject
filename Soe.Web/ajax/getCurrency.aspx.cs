using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections;
using System.Linq;

namespace SoftOne.Soe.Web.ajax
{
    public partial class getCurrency : JsonBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            CountryCurrencyManager ccm = new CountryCurrencyManager(ParameterObject);

            if (Int32.TryParse(QS["currencyId"], out int sysCurrencyId))
            {
                string code = "";
                string name = "";
                int intervalType = (int)TermGroup_CurrencyIntervalType.FirstDayOfMonth;
                bool useSysRate = false;
                string rateToBase = "";
                string rateFromBase = "";
                string rateToBaseInfo = "";
                string rateFromBaseInfo = "";

                Currency currency = ccm.GetCurrencyAndRateBySysCurrency(sysCurrencyId, SoeCompany.ActorCompanyId);
                if (currency != null)
                {
                    code = currency.Code;
                    name = currency.Name;
                    intervalType = currency.IntervalType;
                    useSysRate = !currency.DefineRateManually;

                    if (currency.CurrencyRate != null)
                    {
                        CurrencyRate currencyRate = currency.CurrencyRate.OrderByDescending(i => i.Date).FirstOrDefault();
                        if (currencyRate != null)
                        {
                            if (currencyRate.RateFromBase.HasValue)
                                rateFromBase = currencyRate.RateFromBase.Value.ToString();
                            if (currencyRate.RateToBase.HasValue)
                                rateToBase = currencyRate.RateToBase.Value.ToString();
                        }
                    }
                }
                else
                {
                    SysCurrency sysCurrency = ccm.GetSysCurrency(sysCurrencyId, true);
                    if (sysCurrency != null)
                    {
                        code = sysCurrency.Code;
                        name = sysCurrency.Name;
                        intervalType = Constants.CURRENCY_INTERVALTYPE_DEFAULT;
                        useSysRate = Constants.CURRENCY_USESYSRATE_DEFAULT == 1;
                    }
                }

                CompCurrency baseCurrency = ccm.GetCompanyBaseCurrency(SoeCompany.ActorCompanyId);
                if (baseCurrency != null)
                {
                    rateFromBaseInfo = code + "/" + baseCurrency.Code;
                    rateToBaseInfo = baseCurrency.Code + "/" + code;
                }

                Queue q = new Queue();
                q.Enqueue(new
                {
                    Found = true,
                    Code = code,
                    Name = name,
                    UseSysRate = useSysRate,
                    IntervalType = intervalType,
                    RateToBase = rateToBase,
                    RateFromBase = rateFromBase,
                    RateToBaseInfo = rateToBaseInfo,
                    RateFromBaseInfo = rateFromBaseInfo,
                });
                ResponseObject = q;
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

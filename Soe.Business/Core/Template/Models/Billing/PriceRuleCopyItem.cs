using SoftOne.Soe.Data;
using System.Collections.Generic;

namespace SoftOne.Soe.Business.Core.Template.Models.Billing
{
    public class PriceRuleCopyItem
    {
        public int TemplateActorCompanyId { get; set; }
        public List<PriceRule> TemplateCompanyPriceRules { get; set; }
        public List<PriceListType> TemplateCompanyPriceListTypes { get; set; }
    }
}

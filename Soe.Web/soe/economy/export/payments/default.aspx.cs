using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Web.Util;
using System;

namespace SoftOne.Soe.Web.soe.economy.export.payments
{
    public partial class _default : PageBase
    {
        #region Variables

        public string FormTitle { get; set; }
        public int exportType;
        public int accountYearId;
        public bool accountYearIsOpen;

        protected AccountManager am;

        #endregion

        protected override void Page_Init(object sender, EventArgs e)
        {
            base.Feature = Feature.Economy_Export_Payments;
            base.Page_Init(sender, e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Int32.TryParse(QS["type"], out exportType))
            {
                switch (exportType)
                {
                    case (int)TermGroup_SysPaymentMethod.LB:
                        this.Feature = Feature.Economy_Export_Payments_LB;
                        FormTitle = GetText(4021, "Exporterade LB filer");
                        break;
                    case (int)TermGroup_SysPaymentMethod.PG:
                        this.Feature = Feature.Economy_Export_Payments_PG;
                        FormTitle = GetText(1844, "Exporterade PG filer");
                        break;
                    case (int)TermGroup_SysPaymentMethod.SEPA:
                        this.Feature = Feature.Economy_Export_Payments_SEPA;
                        FormTitle = GetText(8211, "Exporterade SEPA filer");
                        break;
                    case (int)TermGroup_SysPaymentMethod.Nets:
                        this.Feature = Feature.Economy_Export_Payments_Nets;
                        FormTitle = GetText(9162, "Exporterade Nets filer");
                        break;
                    case (int)TermGroup_SysPaymentMethod.Cfp:
                        
                        this.Feature = Feature.Economy_Export_Payments_Cfp;
                        FormTitle = GetText(9249, "Exporterade Cfp (Pg) filer");
                        break;
                }
            }

            string guid = QS["exportfile"];
            if (!string.IsNullOrEmpty(guid))
            {
                Company company = CompanyManager.GetCompany(ParameterObject.ActorCompanyId);
                int sysCountryId = company?.SysCountryId ?? 0;
                if (Int32.TryParse(QS["paymentExportId"], out int paymentExportId))
                    ExportUtil.ExportPaymentFromDatabase(paymentExportId, sysCountryId);
            }

            am = new AccountManager(ParameterObject);
            am.GetAccountYearInfo(CurrentAccountYear, out accountYearId, out accountYearIsOpen);
        }
    }
}

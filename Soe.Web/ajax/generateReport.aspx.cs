using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Util.WebApiInternal;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web.UI.WebControls;

namespace SoftOne.Soe.Web.ajax
{
    public partial class generateReport : JsonBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            bool valid = false;

            if (Int32.TryParse(QS["templateType"], out int sysReportTemplateTypeId))
            {
                switch (sysReportTemplateTypeId)
                {
                    case (int)SoeReportTemplateType.SymbrioEdiSupplierInvoice:
                        #region SymbrioEdiSupplierInvoice

                        List<int> ediEntryIds = new List<int>();
                        if (!String.IsNullOrEmpty(QS["ediEntry"]))
                            ediEntryIds.Add(StringUtility.GetInt(QS["ediEntry"]));
                        else if (!String.IsNullOrEmpty(QS["ediEntrys"]))
                            ediEntryIds.AddRange(StringUtility.SplitNumericList(QS["ediEntrys"]));

                        ediEntryIds = ediEntryIds.Where(i => i > 0).ToList();
                        if (ediEntryIds.Count > 0)
                        {
                            valid = true;

                            if (UseCrystalService())
                            {
                                try
                                {
                                    string culture = Thread.CurrentThread.CurrentCulture.Name;
                                    var channel = GetCrystalServiceChannel();
                                    channel.GenerateReportForEdi(ediEntryIds, SoeCompany.ActorCompanyId, UserId, culture);
                                }
                                catch (Exception ex)
                                {
                                    SysLogManager.LogError<generateReport>(ex);
                                }
                            }
                            else if (UseWebApiInternal())
                            {
                                try
                                {
                                    string culture = Thread.CurrentThread.CurrentCulture.Name;
                                    var connector = new ReportConnector();
                                    connector.GenerateReportForEdi(ediEntryIds, SoeCompany.ActorCompanyId, UserId, culture);
                                }
                                catch (Exception ex)
                                {
                                    SysLogManager.LogError<generateReport>(ex);
                                }
                            }
                            else
                            {
                                ReportDataManager rdm = new ReportDataManager(ParameterObject);
                                rdm.GenerateReportForEdi(ediEntryIds, SoeCompany.ActorCompanyId);
                            }
                        }

                        #endregion
                        break;
                }
            }

            if (ResponseObject == null)
            {
                ResponseObject = new
                {
                    Found = valid,
                };
            }
        }
    }
}
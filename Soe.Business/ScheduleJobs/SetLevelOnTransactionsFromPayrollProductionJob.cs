using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Interfaces;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.ScheduledJobs
{
    public class SetLevelOnTransactionsFromPayrollProductionJob : ScheduledJobBase, IScheduledJob
    {
        public void Execute(SysScheduledJobDTO scheduledJob, int batchNr)
        {
            #region Init

            int? paramCompanyId = scheduledJob.SysJobSettings.Where(s => s.Name.ToLower() == "actorcompanyid").Select(s => s.IntData).FirstOrDefault();
            int? parampayrollproductid = scheduledJob.SysJobSettings.Where(s => s.Name.ToLower() == "payrollproductid").Select(s => s.IntData).FirstOrDefault();
            string parampayrollProductNumber = scheduledJob.SysJobSettings.Where(s => s.Name.ToLower() == "payrollproductnumber").Select(s => s.StrData).FirstOrDefault();
            DateTime? fromDate = scheduledJob.SysJobSettings.Where(s => s.Name.ToLower() == "fromdate").Select(s => s.DateData).FirstOrDefault();
            DateTime? toDate = scheduledJob.SysJobSettings.Where(s => s.Name.ToLower() == "todate").Select(s => s.DateData).FirstOrDefault();
            base.Init(scheduledJob, batchNr);

            #endregion

            // Check out scheduled job
            ActionResult result = CheckOutScheduledJob();
            if (result.Success)
            {
                if (paramCompanyId.HasValue && parampayrollproductid.HasValue && fromDate.HasValue && toDate.HasValue)
                {
                    try
                    {
                        // Execute job
                        TimeTransactionManager tm = new TimeTransactionManager(null);

                        result = tm.SetLevelOnTransactionsFromPayrollProduction(paramCompanyId.Value, parampayrollproductid.Value, fromDate.Value, toDate.Value);
                        CreateLogEntry(ScheduledJobLogLevel.Information, String.Format("Logg: '{0}'", result.ErrorMessage));
                    }
                    catch (Exception ex)
                    {
                        result = new ActionResult(ex);
                        CreateLogEntry(ScheduledJobLogLevel.Error, String.Format("Fel vid exekvering av jobb: '{0}'", result.ErrorMessage));
                        base.LogError(ex);
                    }
                }
                else if (!string.IsNullOrEmpty(parampayrollProductNumber))
                {
                    ProductManager pm = new ProductManager(null);
                    TimeTransactionManager tm = new TimeTransactionManager(null);
                    CompanyManager cm = new CompanyManager(null);
                    SettingManager sm = new SettingManager(null);
                    List<int> companyIds = sm.GetCompanyIdsWithCompanyBoolSetting(CompanySettingType.UsePayroll, true);

                    foreach (var id in companyIds)
                    {
                        Company company = cm.GetCompany(id);

                        if (company != null)
                        {
                            CreateLogEntry(ScheduledJobLogLevel.Information, String.Format("Beräknar för '{0}' ({1})", company.Name, company.ActorCompanyId));

                            var product = pm.GetPayrollProductByNumber(parampayrollProductNumber, id);
                            if (product != null)
                            {
                                result = tm.SetLevelOnTransactionsFromPayrollProduction(id, product.ProductId, fromDate.Value, toDate.Value);
                            }
                            else
                            {
                                CreateLogEntry(ScheduledJobLogLevel.Error, String.Format("Löneart saknas för '{0}' ({1})", company.Name, company.ActorCompanyId));
                            }
                        }
                    }
                }
                else
                {
                    result.Success = false;
                    result.ErrorMessage = "invalid parameters";
                }
            }

            // Check in scheduled job
            CheckInScheduledJob(result.Success);
        }

    }
}

using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Interfaces;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Linq;

namespace SoftOne.Soe.ScheduledJobs
{
    public class AutoTransferAutogiroSupplierPaymentsJob : ScheduledJobBase, IScheduledJob
    {
        public void Execute(SysScheduledJobDTO scheduledJob, int batchNr)
        {
            #region Init

            base.Init(scheduledJob, batchNr);

            var sm = new SettingManager(parameterObject);
            var cam = new CalendarManager(parameterObject);
            var am = new AccountManager(parameterObject);
            var im = new InvoiceManager(parameterObject);
            var pm = new PaymentManager(parameterObject);
            var sim = new SupplierInvoiceManager(parameterObject);

            int? paramCompanyId = scheduledJob.SysJobSettings.Where(s => s.Name.ToLower() == "actorcompanyid").Select(s => s.IntData).FirstOrDefault();
            DateTime? paramExecuteDate = scheduledJob.SysJobSettings.Where(s => s.Name.ToLower() == "date").Select(s => s.DateData).FirstOrDefault();
            DateTime? paramFilterDate = scheduledJob.SysJobSettings.Where(s => s.Name.ToLower() == "filterdate").Select(s => s.DateData).FirstOrDefault();

            #endregion

            // Check out scheduled job
            ActionResult result = CheckOutScheduledJob();

            if (result.Success)
            {
                try
                {
                    var sysHolidays = cam.GetSysHolidaysAndTypes();
                    var companies = base.GetCompanies(paramCompanyId);

                    CreateLogEntry(ScheduledJobLogLevel.Information, "Startar överföring av autogirobetalningar");

                    foreach (var company in companies)
                    {
                        // Check setting
                        if (sm.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.SupplierInvoiceAutoTransferAutogiroPaymentsToVoucher, 0, company.ActorCompanyId, 0))
                        {
                            var today = paramExecuteDate.HasValue ? paramExecuteDate.Value : DateTime.Today;

                            // Validate holiday
                            var todayIsHoliday = sysHolidays.Any(s => (s.SysHolidayType == null || s.SysHolidayType.SysCountryId == company.SysCountryId) && s.Date == today);
                            if (!todayIsHoliday)
                            {
                                if (today.DayOfWeek != DayOfWeek.Saturday && today.DayOfWeek != DayOfWeek.Sunday)
                                {
                                    // Get account year id
                                    var accountYearId = am.GetAccountYearId(today, company.ActorCompanyId);

                                    // Get payments
                                    var invoices = sim.GetSupplierPaymentsForGrid(SoeOriginStatusClassification.SupplierPaymentsPayed, TermGroup_ChangeStatusGridAllItemsSelection.Twelve_Months, company.ActorCompanyId);

                                    // Filter
                                    if (paramFilterDate.HasValue)
                                        invoices = invoices.Where(i => i.PayDate == paramFilterDate.Value).ToList();

                                    // Transfer
                                    result = pm.TransferPaymentRowsToVoucherUsePaymentDate(invoices.Where(i => i.SysPaymentTypeId == (int)TermGroup_SysPaymentType.Autogiro && i.PayDate <= today).ToList(), SoeOriginType.SupplierPayment, accountYearId, company.ActorCompanyId, true, sysHolidays);

                                    if (result.Success)
                                    {
                                        if(result.Strings != null && result.Strings.Count > 0)
                                        {
                                            foreach(var str in result.Strings)
                                            {
                                                CreateLogEntry(ScheduledJobLogLevel.Error, String.Format("Fel i överföringsjobb för '{0}' ({1})", company.Name, company.ActorCompanyId) + ": " + "Betalning med nr " + str + " saknar konteringsrader eller har nollkontering");
                                            }
                                        }
                                        CreateLogEntry(ScheduledJobLogLevel.Information, String.Format("Överföring av betalningar klar för '{0}' ({1})", company.Name, company.ActorCompanyId) + ": " + result.InfoMessage);
                                    }
                                    else
                                    {
                                        if (result.ErrorNumber > 0 && (result.ErrorMessage == String.Empty || result.ErrorMessage == null))
                                        {
                                            switch (result.ErrorNumber)
                                            {
                                                case (int)ActionResultSave.EntityNotFound:
                                                    CreateLogEntry(ScheduledJobLogLevel.Error, String.Format("Fel i överföringsjobb för '{0}' ({1})", company.Name, company.ActorCompanyId) + ": " + "EntityNotFound [" + result.ErrorMessage + "]");
                                                    break;
                                                case (int)ActionResultSave.Unknown:
                                                    CreateLogEntry(ScheduledJobLogLevel.Error, String.Format("Fel i överföringsjobb för '{0}' ({1})", company.Name, company.ActorCompanyId));
                                                    break;
                                                case (int)ActionResultSave.AccountYearNotFound:
                                                    CreateLogEntry(ScheduledJobLogLevel.Error, String.Format("Fel i överföringsjobb för '{0}' ({1})", company.Name, company.ActorCompanyId) + ": Redovisningsår saknas");
                                                    break;
                                                case (int)ActionResultSave.AccountYearNotOpen:
                                                    CreateLogEntry(ScheduledJobLogLevel.Error, String.Format("Fel i överföringsjobb för '{0}' ({1})", company.Name, company.ActorCompanyId) + ": Redovisningsåret är inte öppet");
                                                    break;
                                                default:
                                                    CreateLogEntry(ScheduledJobLogLevel.Error, String.Format("Fel vid exekvering av jobb: '{0}'", result.ErrorMessage));
                                                    break;
                                            }
                                        }
                                        else
                                        {
                                            CreateLogEntry(ScheduledJobLogLevel.Error, String.Format("Fel i överföringsjobb för '{0}' ({1})", company.Name, company.ActorCompanyId) + ": " + result.ErrorMessage);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    // Fix for not checking in error
                    if(result.Success == false)
                        result.Success = true;
                }
                catch (Exception ex)
                {
                    result = new ActionResult(ex);
                    base.LogError(ex);
                }

                if (!result.Success)
                {
                    #region Logging

                    string prefix = "Conversion failed. ";
                    switch (result.ErrorNumber)
                    {
                        case (int)ActionResultSave.EntityNotFound:
                            CreateLogEntry(ScheduledJobLogLevel.Error, prefix + "EntityNotFound [" + result.ErrorMessage + "]");
                            break;
                        case (int)ActionResultSave.Unknown:
                            CreateLogEntry(ScheduledJobLogLevel.Error, prefix + result.ErrorMessage);
                            break;
                        default:
                            CreateLogEntry(ScheduledJobLogLevel.Error, String.Format("Fel vid exekvering av jobb: '{0}'", result.ErrorMessage));
                            break;
                    }

                    #endregion
                }

                // Check in scheduled job
                CheckInScheduledJob(result.Success);
            }
        }
    }
}


using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Interfaces;
using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;

namespace SoftOne.Soe.ScheduledJobs
{
    public class CreateExpenseRows : ScheduledJobBase, IScheduledJob
    {
        public void Execute(SysScheduledJobDTO scheduledJob, int batchNr)
        {
            #region Init

            base.Init(scheduledJob, batchNr);

            CompanyManager cm = new CompanyManager(parameterObject);
            SettingManager sm = new SettingManager(parameterObject);
            TimeScheduleManager tsm = new TimeScheduleManager(parameterObject);
            DateTime hoppla = DateTime.Now.AddYears(-5);

            int? paramCompanyId = scheduledJob.SysJobSettings.Where(s => s.Name.ToLower() == "actorcompanyid").Select(s => s.IntData).FirstOrDefault();

            #endregion

            // Check out scheduled job
            ActionResult result = CheckOutScheduledJob();
            if (result.Success)
            {
                try
                {
                    using (CompEntities entities = new CompEntities())
                    {
                        List<int> companiesWithAdditionDeductionTimeCodes;
                        if (!paramCompanyId.HasValue)
                        {
                            companiesWithAdditionDeductionTimeCodes = entities.TimeCode.Where(w => w.Type == (int)SoeTimeCodeType.AdditionDeduction).Select(s => s.ActorCompanyId).ToList();
                            var activeCompanies = entities.Company.Where(w => w.License.State == (int)SoeEntityState.Active && w.State == (int)SoeEntityState.Active).Select(s => s.ActorCompanyId).ToList();
                            companiesWithAdditionDeductionTimeCodes = companiesWithAdditionDeductionTimeCodes.Where(w => activeCompanies.Contains(w)).ToList();
                            var transationsCompanyIds = entities.TimePayrollTransaction.Where(w => activeCompanies.Contains(w.ActorCompanyId)).Select(s => s.ActorCompanyId).Distinct().ToList();
                            companiesWithAdditionDeductionTimeCodes = companiesWithAdditionDeductionTimeCodes.Where(w => transationsCompanyIds.Contains(w)).ToList();
                            companiesWithAdditionDeductionTimeCodes = companiesWithAdditionDeductionTimeCodes.Distinct().ToList();
                        }
                        else
                            companiesWithAdditionDeductionTimeCodes = new List<int>() { paramCompanyId.Value };

                        entities.CommandTimeout = 30000;
                        List<TimeScheduleTemplateBlock> updatedBlocks = new List<TimeScheduleTemplateBlock>();
                        int count = 1;
                        int total = companiesWithAdditionDeductionTimeCodes.Count;

                        foreach (var companyId in companiesWithAdditionDeductionTimeCodes.OrderBy(o => o))
                        {
                            entities.Connection.Open();

                            using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                            {
                                int createdExpenses = 0;
                                var company = entities.Company.First(f => f.ActorCompanyId == companyId);
                                var employeeIds = entities.Employee.Where(w => w.ActorCompanyId == companyId).Select(s => s.EmployeeId).ToList();
                                CreateLogEntry(ScheduledJobLogLevel.Error, $"Startar: {company.ActorCompanyId} {company.Name} med {employeeIds.Count} anställda. {count}/{total}");
                                count++;
                                if (!entities.EmployeeSchedule.Any(w => employeeIds.Contains(w.EmployeeId)))
                                {
                                    CreateLogEntry(ScheduledJobLogLevel.Error, $"Hoppar ur: {company.ActorCompanyId} {company.Name} inga aktiveringar funna");
                                    entities.Connection.Close();
                                    continue;
                                }

                                var payrollTransactions = entities.TimePayrollTransaction.Include("TimeCodeTransaction").Include("TimeBlockDate").Where(w => w.State == (int)SoeEntityState.Active && w.ActorCompanyId == companyId && w.TimeCodeTransactionId.HasValue
                                && w.TimeCodeTransaction.IsAdditionOrDeduction).ToList();
                                if (!payrollTransactions.Any())
                                {
                                    CreateLogEntry(ScheduledJobLogLevel.Error, $"Hoppar ur: {company.ActorCompanyId} {company.Name} inga transaktioner funna");
                                    entities.Connection.Close();
                                    continue;
                                }

                                List<int> existingTimeCodeTransactionIds = entities.ExpenseRow.Where(w => w.ActorCompanyId == companyId && w.TimeCodeTransactionId.HasValue).Select(s => s.TimeCodeTransactionId.Value).Distinct().ToList();
                                List<TimeCodeTransaction> timeCodeTransactions = new List<TimeCodeTransaction>();

                                foreach (var trans in payrollTransactions)
                                {
                                    if (!timeCodeTransactions.Contains(trans.TimeCodeTransaction) && !existingTimeCodeTransactionIds.Contains(trans.TimeCodeTransactionId.Value) && trans.State == (int)SoeEntityState.Active)
                                        timeCodeTransactions.Add(trans.TimeCodeTransaction);
                                }

                                if (!timeCodeTransactions.Any())
                                {
                                    CreateLogEntry(ScheduledJobLogLevel.Error, $"Hoppar ur: {company.ActorCompanyId} {company.Name} inga tidkodstransaktioner funna");
                                    entities.Connection.Close();
                                    continue;
                                }

                                List<TimeBlockDate> timeBlockDates = new List<TimeBlockDate>();

                                foreach (var trans in payrollTransactions)
                                {
                                    if (!timeBlockDates.Contains(trans.TimeBlockDate))
                                        timeBlockDates.Add(trans.TimeBlockDate);
                                }

                                List<ExpenseHead> newHeads = new List<ExpenseHead>();

                                foreach (var expenseRowInput in timeCodeTransactions)
                                {
                                    int timeBlockDateId = expenseRowInput.TimePayrollTransaction.First().TimeBlockDate.TimeBlockDateId;
                                    int employeeId = expenseRowInput.TimePayrollTransaction.First().EmployeeId;

                                    ExpenseHead expenseHead = new ExpenseHead()
                                    {
                                        Start = expenseRowInput.Start,
                                        Stop = expenseRowInput.Stop,
                                        Comment = expenseRowInput.Comment,
                                        Accounting = expenseRowInput.Accounting,
                                        State = (int)SoeEntityState.Active,

                                        //Set FK
                                        ActorCompanyId = companyId,

                                        //Set references
                                        EmployeeId = employeeId,
                                        TimeBlockDateId = timeBlockDateId,
                                        CreatedBy = "Job485"
                                    };

                                    newHeads.Add(expenseHead);
                                    createdExpenses++;

                                    ExpenseRow expenseRow = new ExpenseRow()
                                    {

                                        Start = CalendarUtility.GetDateTime(CalendarUtility.DATETIME_DEFAULT, expenseRowInput.Start),
                                        Stop = CalendarUtility.GetDateTime(CalendarUtility.DATETIME_DEFAULT, expenseRowInput.Stop),
                                        Quantity = expenseRowInput.Quantity,
                                        Comment = expenseRowInput.Comment,
                                        ExternalComment = expenseRowInput.ExternalComment,
                                        IsSpecifiedUnitPrice = expenseRowInput.TimePayrollTransaction.Any(a => a.IsSpecifiedUnitPrice),
                                        Accounting = expenseRowInput.Accounting,
                                        UnitPrice = expenseRowInput.UnitPrice ?? 0,
                                        UnitPriceCurrency = expenseRowInput.UnitPriceCurrency ?? 0,
                                        UnitPriceLedgerCurrency = expenseRowInput.UnitPriceLedgerCurrency ?? 0,
                                        UnitPriceEntCurrency = expenseRowInput.UnitPriceEntCurrency ?? 0,
                                        Amount = expenseRowInput.Amount ?? 0,
                                        AmountCurrency = expenseRowInput.AmountCurrency ?? 0,
                                        AmountLedgerCurrency = expenseRowInput.AmountLedgerCurrency ?? 0,
                                        AmountEntCurrency = expenseRowInput.AmountEntCurrency ?? 0,
                                        InvoicedAmount = 0,
                                        InvoicedAmountCurrency = 0,
                                        InvoicedAmountLedgerCurrency = 0,
                                        InvoicedAmountEntCurrency = 0,
                                        Vat = expenseRowInput.Vat ?? 0,
                                        VatCurrency = expenseRowInput.VatCurrency ?? 0,
                                        VatLedgerCurrency = expenseRowInput.VatLedgerCurrency ?? 0,
                                        VatEntCurrency = expenseRowInput.VatEntCurrency ?? 0,
                                        CreatedBy = expenseHead.CreatedBy,
                                        State = (int)SoeEntityState.Active,
                                        TimeCodeTransactionId = expenseRowInput.TimeCodeTransactionId,

                                        //Set FK
                                        ActorCompanyId = companyId,

                                        EmployeeId = employeeId,
                                        TimeCodeId = expenseRowInput.TimeCodeId,
                                        TimePeriodId = expenseRowInput.TimePayrollTransaction.Any(a => a.TimePeriodId.HasValue) ? expenseRowInput.TimePayrollTransaction.First(a => a.TimePeriodId.HasValue).TimePeriodId : null
                                    };

                                    expenseHead.ExpenseRow = new System.Data.Entity.Core.Objects.DataClasses.EntityCollection<ExpenseRow>();
                                    expenseHead.ExpenseRow.Add(expenseRow);

                                    count++;
                                }

                                CreateLogEntry(ScheduledJobLogLevel.Error, $"Skapade {createdExpenses} utlägg på {company.ActorCompanyId} {company.Name}");

                                try
                                {
                                    entities.BulkInsert(newHeads);
                                }
                                catch (Exception ex)
                                {
                                    CreateLogEntry(ScheduledJobLogLevel.Error, String.Format("Fel vid exekvering av jobb  newHeads BulkInsert: '{0}'", ex.ToString()));
                                    base.LogError(ex);
                                    entities.Connection.Close();
                                    continue;
                                }

                                var rows = newHeads.SelectMany(s => s.ExpenseRow).ToList();

                                foreach (var row in rows)
                                    row.ExpenseHeadId = row.ExpenseHead.ExpenseHeadId;

                                try
                                {
                                    entities.BulkInsert(rows);
                                }
                                catch (Exception ex)
                                {
                                    CreateLogEntry(ScheduledJobLogLevel.Error, String.Format("Fel vid exekvering av jobb rows BulkInsert: '{0}'", ex.ToString()));
                                    base.LogError(ex);
                                    entities.Connection.Close();
                                    continue;
                                }

                                transaction.Complete();
                            }

                            entities.Connection.Close();
                        }
                    }
                }
                catch (Exception ex)
                {
                    result = new ActionResult(ex);
                    CreateLogEntry(ScheduledJobLogLevel.Error, String.Format("Fel vid exekvering av jobb: '{0}'", result.ErrorMessage));
                    base.LogError(ex);
                }

                // Check in scheduled job
                CheckInScheduledJob(result.Success);
            }
        }
    }
}

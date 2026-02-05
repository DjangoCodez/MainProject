using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;

namespace SoftOne.Soe.Business.Core.TimeEngine
{
    public partial class TimeEngine : ManagerBase
    {
        #region Tasks

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private TaskSaveExpenseValidationOutputDTO TaskSaveExpenseValidation()
        {
            var (iDTO, oDTO) = InitTask<TaskSaveExpenseInputDTO, TaskSaveExpenseValidationOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            using (CompEntities taskEntities = new CompEntities())
            {
                InitContext(taskEntities);

                oDTO.ValidationOutput = SaveExpenseValidation(iDTO.ExpenseRow);
            }

            return oDTO;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private TaskSaveExpenseOutputDTO TaskSaveExpense()
        {
            var (iDTO, oDTO) = InitTask<TaskSaveExpenseInputDTO, TaskSaveExpenseOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            using (CompEntities taskEntities = new CompEntities())
            {
                InitContext(taskEntities);

                try
                {
                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        InitTransaction(transaction);

                        oDTO.Result = SaveExpense(iDTO.ExpenseRow, iDTO.CustomerInvoiceId, null, iDTO.ReturnEntity);

                        TryCommit(oDTO);
                    }
                }
                catch (Exception ex)
                {
                    oDTO.Result.Exception = ex;
                    LogError(ex);
                }
                finally
                {
                    if (!oDTO.Result.Success)
                        LogTransactionFailed(this.ToString());

                    entities.Connection.Close();
                }
            }

            return oDTO;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private TaskSaveExpenseOutputDTO TaskDeleteExpense()
        {
            var (iDTO, oDTO) = InitTask<DeleteExpenseInputDTO, TaskSaveExpenseOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            using (CompEntities taskEntities = new CompEntities())
            {
                InitContext(taskEntities);

                try
                {
                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        InitTransaction(transaction);

                        oDTO.Result = DeleteExpenseRow(iDTO.ExpenseRowId, iDTO.NoErrorIfExpenseRowNotFound);

                        TryCommit(oDTO);
                    }
                }
                catch (Exception ex)
                {
                    oDTO.Result.Exception = ex;
                    LogError(ex);
                }
                finally
                {
                    if (!oDTO.Result.Success)
                        LogTransactionFailed(this.ToString());

                    entities.Connection.Close();
                }
            }

            return oDTO;
        }

        #endregion

        #region AdditionDeduction

        private ActionResult SetUnitPriceAndAmountOnAddictionDeductionTransactions(Employee employee, TimeEngineTemplate template, TimeBlockDate timeBlockDate)
        {
            ActionResult result = new ActionResult();

            if (template.Outcome.TimePayrollTransactions == null)
                return result;

            Employment employment = employee.GetEmployment(timeBlockDate.Date);
            if (employment == null)
                return result;

            var transactionsGroupedByTimeCode = template.Outcome.TimePayrollTransactions.GroupBy(x => x.TimeCodeTransaction.TimeCodeId).ToList();

            foreach (var transactionGroup in transactionsGroupedByTimeCode)
            {
                TimeCode timeCode = GetTimeCodeFromCache(transactionGroup.Key);

                foreach (var transaction in transactionGroup.Where(i => i.TimeCodeTransaction != null))
                {
                    PayrollPriceFormulaResultDTO formulaResult = null;
                    if (!transaction.IsSpecifiedUnitPrice)
                    {
                        PayrollProduct payrollProduct = GetPayrollProductWithSettingsAndAccountInternalsAndStdsFromCache(transaction.ProductId);
                        if (payrollProduct == null)
                            continue;

                        formulaResult = EvaluatePayrollPriceFormula(timeBlockDate.Date, employee, employment, payrollProduct);
                    }

                    SetUnitPriceAndAmountOnAddictionDeductionTransactions(employee, transaction, formulaResult, timeCode);
                }
            }

            return result;
        }
        private void SetUnitPriceAndAmountOnAddictionDeductionTransactions(Employee employee, TimePayrollTransaction timePayrollTransaction, PayrollPriceFormulaResultDTO formulaResult, TimeCode timeCode)
        {
            if (!timePayrollTransaction.IsAdditionOrDeduction)
                return;

            if (timePayrollTransaction.IsCompensation_Vat())
                return;

            if (timeCode == null)
            {
                if (timePayrollTransaction.TimeCodeTransaction == null)
                    return; //must be loaded

                if (timePayrollTransaction.TimeCodeTransaction.TimeCode != null)
                    timeCode = timePayrollTransaction.TimeCodeTransaction.TimeCode;
                else
                    timeCode = GetTimeCodeFromCache(timePayrollTransaction.TimeCodeTransaction.TimeCodeId);

                if (timeCode == null)
                    return;
            }

            //First set unitprice
            if (timePayrollTransaction.IsSpecifiedUnitPrice)
            {
                timePayrollTransaction.UnitPrice = timePayrollTransaction.TimeCodeTransaction.UnitPrice;
            }
            else
            {
                CreateTimePayrollTransactionExtended(timePayrollTransaction, employee.EmployeeId, actorCompanyId);
                SetTimePayrollTransactionFormulas(timePayrollTransaction, formulaResult);
                timePayrollTransaction.UnitPrice = Decimal.Round(formulaResult?.Amount ?? 0, 2, MidpointRounding.AwayFromZero);
            }

            CalculateAmountOnAdditionDeductionTransaction(timePayrollTransaction, timeCode);
            SetTimePayrollTransactionCurrencyAmounts(timePayrollTransaction);
        }
        private void CalculateAmountOnAdditionDeductionTransaction(TimePayrollTransaction timePayrollTransaction, TimeCode timeCode)
        {
            if (!timePayrollTransaction.IsAdditionOrDeduction)
                return;

            //Set amount based on quantity
            if (timePayrollTransaction.Quantity != 0 && timePayrollTransaction.UnitPrice.HasValue)
                timePayrollTransaction.Amount = Decimal.Round(decimal.Multiply(timePayrollTransaction.UnitPrice.Value, timeCode.IsRegistrationTypeTime ? (timePayrollTransaction.Quantity / 60) : timePayrollTransaction.Quantity), 2, MidpointRounding.AwayFromZero);
            else
                timePayrollTransaction.Amount = 0;
        }
        private ActionResult OverrideAccountingOnAddictionDeductionTransactions(TimeEngineTemplate template)
        {
            ActionResult result = new ActionResult();

            if (template.Outcome.TimePayrollTransactions == null)
                return result;

            foreach (var transaction in template.Outcome.TimePayrollTransactions)
            {
                if (transaction.TimeCodeTransaction != null)
                {
                    // Get Account
                    var accounting = transaction.TimeCodeTransaction.GetAccountingFromString(GetAccountDimWithAccountsFromCache());
                    if (accounting.AccountId > 0)
                        transaction.AccountStdId = accounting.AccountId;
                    if (accounting.AccountInternals != null)
                    {
                        foreach (var item in accounting.AccountInternals)
                        {
                            bool handled = false;

                            //Update if exists
                            foreach (var ai in transaction.AccountInternal.ToList())
                            {
                                var match = accounting.AccountInternals.FirstOrDefault(f => f.AccountDimId == ai.Account.AccountDimId);
                                if (match == null)
                                    continue;

                                if (item.AccountDimId == ai.Account.AccountDimId && ai.AccountId != match.AccountId)
                                {
                                    transaction.AccountInternal.Remove(ai);

                                    if (match.AccountId > 0)
                                    {
                                        AccountInternal accountInternal = GetAccountInternalWithAccountFromCache(match.AccountId);
                                        if (accountInternal != null)
                                            transaction.AccountInternal.Add(accountInternal);
                                    }
                                    handled = true;
                                }
                            }

                            //Add if dim didn't exist
                            if (!handled && item.AccountId > 0)
                            {
                                AccountInternal accountInternal = GetAccountInternalWithAccountFromCache(item.AccountId);
                                if (accountInternal != null)
                                    transaction.AccountInternal.Add(accountInternal);
                            }
                        }
                    }
                }
            }

            return result;
        }
        private ActionResult CreateVatTransaction(ExpenseRow expenseRow, TimeEngineTemplate template, TimeBlockDate timeBlockDate, int? timePeriodId)
        {
            //OBS! Logically, adding vat to an expense only works when 1 type of products are generated( can be multiple transactions i.e distributed transactions)
            //You should not use a productchain or connect the timecode to more then 1 product when using vat on an expense, since the vat will be excluded multiple times, 
            //once for each product

            ActionResult result = new ActionResult();

            #region Prereq

            if (template.Outcome.TimePayrollTransactions.IsNullOrEmpty() || !expenseRow.IsSpecifiedUnitPrice || expenseRow.Quantity == 0 || expenseRow.UnitPrice == 0 || expenseRow.Amount == 0 || expenseRow.Vat == 0)
                return result;

            TimeCodeTransaction timeCodeTransaction = template.Outcome.TimePayrollTransactions.First().TimeCodeTransaction;
            if (timeCodeTransaction == null)
                return result;

            TimeCode timeCode = GetTimeCodeFromCache(expenseRow.TimeCodeId);
            if (timeCode == null || !timeCode.IsExpense())
                return result;

            decimal? vatPercent = null;
            var comment = (timeCode as TimeCodeAdditionDeduction)?.Description;

            if (comment != null && comment.Contains("MOMS")) // Coop templösning, skriv t ex MOMS25% i beskrivningen på tillägg/utlägg
            {
                var vat = comment.Split(' ').FirstOrDefault(f => f.Contains("MOMS"));
                if (vat != null)
                {
                    decimal.TryParse(vat.Replace("MOMS", "").Replace("%", ""), out decimal vatPercentTemp);
                    vatPercent = vatPercentTemp;
                }
            }

            PayrollProduct vatProduct = GetPayrollProductCompensationVat(vatPercent);
            if (vatProduct == null)
                return result;

            AttestStateDTO attestStateInitial = GetAttestStateInitialFromCache(TermGroup_AttestEntity.PayrollTime);
            if (attestStateInitial == null)
                return result;

            #endregion

            #region Create vat transaction

            TimePayrollTransaction vatTransaction = CreateTimePayrollTransaction(vatProduct, timeBlockDate, 1, expenseRow.Vat, 0, expenseRow.Vat, expenseRow.Comment, attestStateInitial.AttestStateId, timePeriodId, template.EmployeeId);
            if (vatTransaction != null)
            {
                vatTransaction.TimeCodeTransaction = timeCodeTransaction;
                ApplyAccountingOnTimePayrollTransaction(vatTransaction, template.Identity.Employee, vatTransaction.TimeBlockDate.Date, vatProduct, setAccountInternal: true);
                entities.TimePayrollTransaction.AddObject(vatTransaction);
                template.Outcome.TimePayrollTransactions.Add(vatTransaction);
            }

            #endregion

            #region Recalculate unitprice and amount based on the expense amount with excluded vat

            decimal expenseAmountExcludedVat = ((expenseRow.Amount - expenseRow.Vat) != 0) ? (expenseRow.Amount - expenseRow.Vat) : 0;

            foreach (var transaction in template.Outcome.TimePayrollTransactions.Where(w => w.VatAmount.HasValue && w.VatAmount != 0))
            {
                transaction.UnitPrice = decimal.Round(expenseAmountExcludedVat / (timeCode.IsRegistrationTypeTime ? (transaction.Quantity / 60) : transaction.Quantity), 2, MidpointRounding.AwayFromZero);
                transaction.VatAmount = 0;
                transaction.VatAmountCurrency = 0;
                transaction.VatAmountEntCurrency = 0;
                transaction.VatAmountLedgerCurrency = 0;

                CalculateAmountOnAdditionDeductionTransaction(transaction, timeCode);
                SetTimePayrollTransactionCurrencyAmounts(transaction);
            }

            #endregion

            return result;
        }

        #endregion

        #region Expense

        private SaveExpenseValidationDTO SaveExpenseValidation(ExpenseRowDTO expenseRow)
        {
            SaveExpenseValidationDTO validationOutput = new SaveExpenseValidationDTO();

            if (this.UsePayroll() && expenseRow != null)
            {
                int? selectedTimePeriodId = GetGivenOrNextOpenTimePeriodId(expenseRow.TimePeriodId, expenseRow.Start, expenseRow.EmployeeId);
                if (selectedTimePeriodId.HasValue && IsEmployeeTimePeriodLockedForChanges(expenseRow.EmployeeId, timePeriodId: selectedTimePeriodId))
                    return new SaveExpenseValidationDTO(false, true, GetText(11934, "Löneperiod är låst"), GetText(11933, "Observera att du registrerar ett utlägg i en låst löneperiod. Det innebär att utlägget inte kommer hanteras i löneberäkningen. Utlägg bör registreras i aktuell löneperiod. Vill du fortsätta?"));
            }

            return validationOutput;
        }

        private ActionResult SaveExpense(ExpenseRowDTO expenseRowInput, int? customerInvoiceId, int? timeStampEntryExtendedId, bool returnEntity)
        {
            ActionResult result;

            #region Prereq

            Employee employee = GetEmployeeFromCache(expenseRowInput.EmployeeId);
            if (employee == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(10083, "Anställd hittades inte"));

            TimeBlockDate timeBlockDate = GetTimeBlockDateFromCache(expenseRowInput.EmployeeId, expenseRowInput.Start, true);
            if (timeBlockDate.IsLocked)
                return new ActionResult((int)ActionResultSave.Locked, GetText(91937, "Dagen är låst och kan ej behandlas"));

            TimeCode timeCode = GetTimeCodeWithProductsFromCache(expenseRowInput.TimeCodeId);
            if (timeCode == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(91938, "Tidkod hittades inte"));

            Project project = null;
            if (expenseRowInput.ProjectId > 0)
            {
                project = GetProject(expenseRowInput.ProjectId);
                if (project == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "Project");
            }

            CustomerInvoice customerInvoice = null;
            if (customerInvoiceId.HasValue && customerInvoiceId.Value > 0)
            {
                customerInvoice = InvoiceManager.GetCustomerInvoice(entities, customerInvoiceId.Value, loadOrigin: true);
                if (customerInvoice == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "CustomerInvoice");

                InvoiceManager.SetPriceListTypeInclusiveVat(entities, customerInvoice, base.ActorCompanyId);
            }

            #endregion

            #region Currencies

            int sysCurrencyIdLedger = customerInvoice != null && customerInvoice.ActorId.HasValue ? CountryCurrencyManager.GetLedgerSysCurrencyId(entities, customerInvoice.ActorId.Value) : 0;
            int sysCurrencyIdEnt = CountryCurrencyManager.GetCompanyBaseEntSysCurrencyId(entities, base.ActorCompanyId);

            //Dates
            DateTime ledgerDate = DateTime.Now;
            if (customerInvoice != null && customerInvoice.InvoiceDate.HasValue)
                ledgerDate = customerInvoice.InvoiceDate.Value;
            else if (customerInvoice != null && customerInvoice.VoucherDate.HasValue)
                ledgerDate = customerInvoice.VoucherDate.Value;

            //Rates
            decimal transactionRate = customerInvoice?.CurrencyRate ?? 1;
            decimal ledgerRate = CountryCurrencyManager.GetCurrencyRate(entities, sysCurrencyIdLedger, base.ActorCompanyId, ledgerDate);
            decimal entRate = CountryCurrencyManager.GetCurrencyRate(entities, sysCurrencyIdEnt, base.ActorCompanyId, ledgerDate);

            // Amount
            expenseRowInput.AmountCurrency = CountryCurrencyManager.GetCurrencyAmountFromBaseAmount(expenseRowInput.Amount, transactionRate);
            expenseRowInput.AmountLedgerCurrency = CountryCurrencyManager.GetCurrencyAmountFromBaseAmount(expenseRowInput.Amount, ledgerRate);
            expenseRowInput.AmountEntCurrency = CountryCurrencyManager.GetCurrencyAmountFromBaseAmount(expenseRowInput.Amount, entRate);

            // VATAmount
            expenseRowInput.VatCurrency = CountryCurrencyManager.GetCurrencyAmountFromBaseAmount(expenseRowInput.Vat, transactionRate);
            expenseRowInput.VatLedgerCurrency = CountryCurrencyManager.GetCurrencyAmountFromBaseAmount(expenseRowInput.Vat, ledgerRate);
            expenseRowInput.VatEntCurrency = CountryCurrencyManager.GetCurrencyAmountFromBaseAmount(expenseRowInput.Vat, entRate);

            // InvoicedAmount
            expenseRowInput.InvoicedAmountCurrency = CountryCurrencyManager.GetCurrencyAmountFromBaseAmount(expenseRowInput.InvoicedAmount, transactionRate);
            expenseRowInput.InvoicedAmountLedgerCurrency = CountryCurrencyManager.GetCurrencyAmountFromBaseAmount(expenseRowInput.InvoicedAmount, ledgerRate);
            expenseRowInput.InvoicedAmountEntCurrency = CountryCurrencyManager.GetCurrencyAmountFromBaseAmount(expenseRowInput.InvoicedAmount, entRate);

            #endregion

            #region Perform

            ExpenseRow expenseRow = null;
            bool updateSalary = expenseRowInput.ExpenseRowId == 0 || expenseRowInput.isDeleted || !expenseRowInput.isTimeReadOnly;

            if (expenseRowInput.ExpenseRowId == 0)
            {
                #region Add

                #region ExpenseHead

                ExpenseHead expenseHead = new ExpenseHead
                {
                    Start = expenseRowInput.Start,
                    Stop = expenseRowInput.Stop,
                    Comment = expenseRowInput.Comment,
                    Accounting = expenseRowInput.Accounting,
                    State = (int)SoeEntityState.Active,

                    //Set FK
                    ActorCompanyId = base.ActorCompanyId,

                    //Set references
                    Employee = employee,
                    TimeBlockDate = timeBlockDate,
                    Project = project,
                };
                SetCreatedProperties(expenseHead);
                entities.ExpenseHead.AddObject(expenseHead);

                #endregion

                #region ExpenseRow

                expenseRow = new ExpenseRow
                {
                    Start = CalendarUtility.GetDateTime(CalendarUtility.DATETIME_DEFAULT, expenseRowInput.Start),
                    Stop = CalendarUtility.GetDateTime(CalendarUtility.DATETIME_DEFAULT, expenseRowInput.Stop),
                    Quantity = expenseRowInput.Quantity,
                    Comment = expenseRowInput.Comment,
                    ExternalComment = expenseRowInput.ExternalComment,
                    IsSpecifiedUnitPrice = expenseRowInput.IsSpecifiedUnitPrice,
                    Accounting = expenseRowInput.Accounting,
                    UnitPrice = expenseRowInput.UnitPrice,
                    UnitPriceCurrency = expenseRowInput.UnitPriceCurrency,
                    UnitPriceLedgerCurrency = expenseRowInput.UnitPriceLedgerCurrency,
                    UnitPriceEntCurrency = expenseRowInput.UnitPriceEntCurrency,
                    Amount = expenseRowInput.Amount,
                    AmountCurrency = expenseRowInput.AmountCurrency,
                    AmountLedgerCurrency = expenseRowInput.AmountLedgerCurrency,
                    AmountEntCurrency = expenseRowInput.AmountEntCurrency,
                    InvoicedAmount = expenseRowInput.InvoicedAmount,
                    InvoicedAmountCurrency = expenseRowInput.InvoicedAmountCurrency,
                    InvoicedAmountLedgerCurrency = expenseRowInput.InvoicedAmountLedgerCurrency,
                    InvoicedAmountEntCurrency = expenseRowInput.InvoicedAmountEntCurrency,
                    Vat = expenseRowInput.Vat,
                    VatCurrency = expenseRowInput.VatCurrency,
                    VatLedgerCurrency = expenseRowInput.VatLedgerCurrency,
                    VatEntCurrency = expenseRowInput.VatEntCurrency,
                    State = (int)SoeEntityState.Active,

                    //Set FK
                    ActorCompanyId = base.ActorCompanyId,
                    TimePeriodId = expenseRowInput.TimePeriodId,
                    TimeStampEntryExtendedId = timeStampEntryExtendedId,

                    //Set references
                    ExpenseHead = expenseHead,
                    Employee = employee,
                    TimeCode = timeCode,
                    CustomerInvoice = customerInvoice,
                    Project = project,

                };
                SetCreatedProperties(expenseRow);
                entities.ExpenseRow.AddObject(expenseRow);

                result = SaveChanges(entities);
                if (!result.Success)
                    return result;

                #endregion

                #endregion
            }
            else
            {
                #region Update/Delete

                expenseRow = GetExpenseRowWithHeadAndInvoiceRow(expenseRowInput.ExpenseRowId);
                if (expenseRow == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "ExpenseRow");

                if (expenseRowInput.isDeleted)
                {
                    #region Delete

                    ChangeEntityState(expenseRow.ExpenseHead, SoeEntityState.Deleted);
                    ChangeEntityState(expenseRow, SoeEntityState.Deleted);

                    result = SaveChanges(entities);
                    if (result.Success && expenseRowInput.CustomerInvoiceId > 0)
                    {
                        // What to do with the invoice row?
                    }

                    #endregion
                }
                else
                {
                    #region Update

                    #region ExpenseHead

                    if (!expenseRowInput.isTimeReadOnly)
                    {
                        expenseRow.ExpenseHead.Start = expenseRowInput.Start;
                        expenseRow.ExpenseHead.Stop = expenseRowInput.Stop;
                        expenseRow.ExpenseHead.Comment = expenseRowInput.Comment;
                        expenseRow.ExpenseHead.Accounting = expenseRowInput.Accounting;
                        SetModifiedProperties(expenseRow.ExpenseHead);

                        //Set references
                        expenseRow.ExpenseHead.TimeBlockDate = timeBlockDate;
                        expenseRow.ExpenseHead.Employee = employee;
                        expenseRow.ExpenseHead.Project = project;
                    }

                    #endregion

                    #region ExpenseRow

                    if (!expenseRowInput.isTimeReadOnly)
                    {
                        expenseRow.Start = expenseRowInput.Start;
                        expenseRow.Stop = expenseRowInput.Stop;
                        expenseRow.Quantity = expenseRowInput.Quantity;
                        expenseRow.Comment = expenseRowInput.Comment;
                        expenseRow.Accounting = expenseRowInput.Accounting;
                        expenseRow.IsSpecifiedUnitPrice = expenseRowInput.IsSpecifiedUnitPrice;
                        expenseRow.UnitPrice = expenseRowInput.UnitPrice;
                        expenseRow.UnitPriceCurrency = expenseRowInput.UnitPriceCurrency;
                        expenseRow.UnitPriceLedgerCurrency = expenseRowInput.UnitPriceLedgerCurrency;
                        expenseRow.UnitPriceEntCurrency = expenseRowInput.UnitPriceEntCurrency;
                        expenseRow.Amount = expenseRowInput.Amount;
                        expenseRow.AmountCurrency = expenseRowInput.AmountCurrency;
                        expenseRow.AmountLedgerCurrency = expenseRowInput.AmountLedgerCurrency;
                        expenseRow.AmountEntCurrency = expenseRowInput.AmountEntCurrency;
                        expenseRow.Vat = expenseRowInput.Vat;
                        expenseRow.VatCurrency = expenseRowInput.VatCurrency;
                        expenseRow.VatLedgerCurrency = expenseRowInput.VatLedgerCurrency;
                        expenseRow.VatEntCurrency = expenseRowInput.VatEntCurrency;
                        if (!expenseRow.TimePeriodId.HasValue && expenseRowInput.TimePeriodId.HasValue)
                            expenseRow.TimePeriodId = expenseRowInput.TimePeriodId;
                    }

                    if (!expenseRowInput.isReadOnly)
                    {
                        expenseRow.ExternalComment = expenseRowInput.ExternalComment;
                        expenseRow.InvoicedAmount = expenseRowInput.InvoicedAmount;
                        expenseRow.InvoicedAmountCurrency = expenseRowInput.InvoicedAmountCurrency;
                        expenseRow.InvoicedAmountLedgerCurrency = expenseRowInput.InvoicedAmountLedgerCurrency;
                        expenseRow.InvoicedAmountEntCurrency = expenseRowInput.InvoicedAmountEntCurrency;
                    }

                    //Set references
                    expenseRow.Employee = employee;
                    expenseRow.TimeCode = timeCode;
                    expenseRow.Project = project;

                    SetModifiedProperties(expenseRow);

                    #endregion

                    result = SaveChanges(entities);

                    #endregion
                }

                #endregion
            }

            #region Save CustomerInvoiceRow

            if (expenseRow != null && (expenseRow.CustomerInvoiceRowId > 0 || expenseRow.InvoicedAmountCurrency != 0))
            {
                TimeCodeInvoiceProduct timeCodeInvoiceProduct = timeCode.TimeCodeInvoiceProduct.FirstOrDefault();
                if (timeCodeInvoiceProduct != null)
                {
                    CustomerInvoiceRow customerInvoiceRow = expenseRow.CustomerInvoiceRowId > 0 ? InvoiceManager.GetCustomerInvoiceRow(entities, expenseRowInput.CustomerInvoiceRowId) : null;
                    decimal invoiceQuantity = timeCode.RegistrationType == (int)TermGroup_TimeCodeRegistrationType.Time ? (expenseRowInput.Quantity) / 60 : expenseRowInput.Quantity;
                    decimal invoiceAmount = invoiceQuantity > 0 ? expenseRowInput.InvoicedAmountCurrency / invoiceQuantity : 0;

                    if (invoiceAmount == 0 && customerInvoiceRow != null)
                    {
                        result = InvoiceManager.DeleteCustomerInvoiceRow(currentTransaction, entities, base.ActorCompanyId, customerInvoice, customerInvoiceRow.CustomerInvoiceRowId, false, true);
                        if (result.Success)
                        {
                            expenseRow.CustomerInvoiceRowId = null;
                            expenseRow.CustomerInvoiceRow = null;
                            result = SaveChanges(entities);
                        }
                    }
                    else
                    {
                        result = InvoiceManager.SaveCustomerInvoiceRow(currentTransaction, entities, base.ActorCompanyId, customerInvoice, expenseRow.CustomerInvoiceRowId.GetValueOrDefault(), timeCodeInvoiceProduct.ProductId, invoiceQuantity, invoiceAmount, string.Empty, SoeInvoiceRowType.ProductRow, productRowType: SoeProductRowType.ExpenseRow, customerInvoiceRowDate: expenseRowInput.Start);
                        if (result.Success && result.Value != null)
                        {
                            expenseRow.CustomerInvoiceRow = result.Value as CustomerInvoiceRow;

                            result = SaveChanges(entities);
                            if (!result.Success)
                                return result;
                        }
                    }
                }
            }

            #endregion

            #region Save TimeCodeTransaction

            if (result.Success && expenseRow != null && updateSalary)
            {
                result = SaveExpenseTimeCodeTransactions(expenseRow, employee.EmployeeId, expenseRow.TimePeriodId, expenseRowInput.Start);
                if (returnEntity)
                    result.Value = expenseRow;
            }

            #endregion

            #region Save Files

            if (expenseRowInput.Files != null)
            {
                var images = expenseRowInput.Files.Where(f => f.ImageId.HasValue);
                GraphicsManager.UpdateImages(entities, images, expenseRow.ExpenseRowId);

                var files = expenseRowInput.Files.Where(f => f.Id.HasValue);
                GeneralManager.UpdateFiles(entities, files, expenseRow.ExpenseRowId, SoeEntityType.Expense);

                entities.SaveChanges();
            }

            #endregion

            #endregion

            return result;
        }

        private ActionResult SaveExpenseTimeCodeTransactions(ExpenseRow expenseRow, int employeeId, int? timePeriodId, DateTime? standsOnDate)
        {
            return SaveExpenseTimeCodeTransactions(new List<ExpenseRow> { expenseRow }, employeeId, timePeriodId, standsOnDate);
        }

        private ActionResult SaveExpenseTimeCodeTransactions(List<ExpenseRow> expenseRows, int employeeId, int? timePeriodId, DateTime? standsOnDate)
        {
            ActionResult result = new ActionResult(true);

            #region Prereq

            if (expenseRows.IsNullOrEmpty())
                return new ActionResult(true);

            decimal maxQuantity = 99999;
            if (expenseRows.Any(i => i.Quantity > 99999))
                return new ActionResult((int)ActionResultSave.EntityNotFound, string.Format(GetText(11931, "Går endast att justera {0} timmar i taget. Vänligen registrera dubbla transaktioner om vill justera mer än så"), Decimal.Floor(maxQuantity / 60)));

            Employee employee = GetEmployeeWithContactPersonAndEmploymentFromCache(employeeId);
            if (employee == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(8540, "Anställd kunde inte hittas"));

            List<EmployeeGroup> employeeGroups = GetEmployeeGroupsFromCache();
            int? selectedTimePeriodId = GetGivenOrNextOpenTimePeriodId(timePeriodId, standsOnDate, employee);

            #endregion

            #region Perform

            foreach (var row in expenseRows)
            {
                if (row.ExpenseHead == null)
                    continue;

                #region Prereq

                DateTime date;
                if (row.ExpenseHead.Stop.HasValue && row.ExpenseHead.Stop.Value.Date > CalendarUtility.DATETIME_DEFAULT)
                    date = row.ExpenseHead.Stop.Value.Date;
                else
                    date = row.ExpenseHead.Start ?? CalendarUtility.DATETIME_DEFAULT;
                if (date == CalendarUtility.DATETIME_DEFAULT)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "TimeBlockDate");

                TimeBlockDate timeBlockDate = GetTimeBlockDateFromCache(employeeId, date, true);
                if (timeBlockDate == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "TimeBlockDate");

                EmployeeGroup employeeGroup = employee.GetEmployeeGroup(timeBlockDate.Date, employeeGroups: employeeGroups);
                if (employeeGroup == null)
                    employeeGroup = employee.GetLastEmployeeGroup(GetEmployeeGroupsFromCache()); //Should be possible to report Addition/Deduction after last Employment
                if (employeeGroup == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, string.Format(GetText(12079, "Tidavtal saknas för anställd {0} den {1}"), employee.EmployeeNr, timeBlockDate.Date.ToShortDateString()));

                TimeCode timeCode = GetTimeCodeFromCache(row.TimeCodeId);
                if (timeCode == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(91938, "Tidkod hittades inte"));

                #endregion

                #region Repository

                TimeEngineTemplateIdentity identity = TimeEngineTemplateIdentity.CreateIdentity(employee, employeeGroup, timeBlockDate);
                TimeEngineTemplate template = CreateTemplate(identity); //Do not look for existing template for now

                #endregion

                #region TimeCodeTransaction

                TimeCodeTransaction timeCodeTransaction;
                if (row.TimeCodeTransactionId.HasValue && row.TimeCodeTransactionId != 0)
                {
                    timeCodeTransaction = GetTimeCodeTransactionWithExternalTransactions(row.TimeCodeTransactionId.Value, false);
                    if (timeCodeTransaction == null)
                        return new ActionResult((int)ActionResultSave.EntityNotFound, string.Format(GetText(91904, "Tidkodtransaktion hittades inte ({0})"), row.TimeCodeTransactionId.Value));

                    SetModifiedProperties(timeCodeTransaction);
                }
                else
                {
                    timeCodeTransaction = new TimeCodeTransaction();
                    entities.TimeCodeTransaction.AddObject(timeCodeTransaction);
                    SetCreatedProperties(timeCodeTransaction);
                }

                if (timeCodeTransaction.IsEarnedHoliday)
                {
                    //Earned holiday transactions are only alloweed to be deleted from addition/deduction view
                    timeCodeTransaction.State = row.State;
                    if (timeCodeTransaction.State == (int)SoeEntityState.Deleted)
                    {
                        //Delete external transactions
                        result = SetExternalTransactionsToDeleted(timeCodeTransaction, saveChanges: false);
                        if (!result.Success)
                            return result;
                    }
                }
                else
                {
                    if (timeCodeTransaction.TimeCodeTransactionId != 0)
                    {
                        //Delete external transactions
                        result = SetExternalTransactionsToDeleted(timeCodeTransaction, saveChanges: false);
                        if (!result.Success)
                            return result;

                        if (row.State == (int)SoeEntityState.Deleted)
                        {
                            ChangeEntityState(timeCodeTransaction, SoeEntityState.Deleted);
                            result = Save();
                            continue;
                        }
                    }

                    timeCodeTransaction.Start = row.ExpenseHead.Start.HasValue && row.ExpenseHead.Start > CalendarUtility.DATETIME_DEFAULT ? row.ExpenseHead.Start.Value : CalendarUtility.DATETIME_DEFAULT;
                    timeCodeTransaction.Stop = row.ExpenseHead.Stop.HasValue && row.ExpenseHead.Stop > CalendarUtility.DATETIME_DEFAULT ? row.ExpenseHead.Stop.Value : CalendarUtility.DATETIME_DEFAULT;
                    timeCodeTransaction.Amount = row.Amount;
                    timeCodeTransaction.UnitPrice = row.UnitPrice;
                    timeCodeTransaction.Vat = row.Vat;
                    timeCodeTransaction.Quantity = row.Quantity;
                    timeCodeTransaction.Comment = row.Comment;
                    timeCodeTransaction.State = row.State;
                    timeCodeTransaction.Type = (int)SoeTimeCodeType.AdditionDeduction;
                    timeCodeTransaction.IsAdditionOrDeduction = timeCode.IsAdditionAndDeduction();
                    timeCodeTransaction.TimeCodeId = row.TimeCodeId;
                    timeCodeTransaction.TimeBlockDate = timeBlockDate;
                    timeCodeTransaction.Accounting = row.Accounting;

                    // Currency
                    CountryCurrencyManager.SetCurrencyAmounts(entities, actorCompanyId, timeCodeTransaction);

                    if (timeCodeTransaction.State != (int)SoeEntityState.Deleted)
                        template.Outcome.TimeCodeTransactions.Add(timeCodeTransaction);

                    row.TimeCodeTransaction = timeCodeTransaction;

                    result = Save();
                    if (!result.Success)
                        return result;

                    result = SaveExternalTransactions(ref template, isAdditionOrDeduction: true, row.ProjectId.ToInt());
                    if (!result.Success)
                        return result;

                    SetTimePayrollTransactionsIsSpecifiedUnitPrice(template.Outcome.TimePayrollTransactions, row.IsSpecifiedUnitPrice);
                    SetTimePayrollTransactionsTimePeriodId(template.Outcome.TimePayrollTransactions, selectedTimePeriodId);

                    result = OverrideAccountingOnAddictionDeductionTransactions(template);
                    if (!result.Success)
                        return result;

                    result = Save();
                    if (!result.Success)
                        return result;

                    //Can not be done in SaveExternalTransactions(), it has to be done after accountinternal has been set in this method, bacause they will be passed on to the child transactions
                    result = CreateTransactionsFromPayrollProductChain(template);
                    if (!result.Success)
                        return result;

                    if (expenseRows.All(a => a.IsAccountingEmpty()))
                    {
                        result = CreateFixedAccountingTransactions(template);
                        if (!result.Success)
                            return result;
                    }

                    result = SetUnitPriceAndAmountOnAddictionDeductionTransactions(employee, template, timeBlockDate);
                    if (!result.Success)
                        return result;

                    result = CreateVatTransaction(row, template, timeBlockDate, selectedTimePeriodId);
                    if (!result.Success)
                        return result;

                    if (!row.IsSpecifiedUnitPrice && (!timeCodeTransaction.UnitPrice.HasValue || timeCodeTransaction.UnitPrice == 0) && (!timeCodeTransaction.Amount.HasValue || timeCodeTransaction.Amount == 0) && template.Outcome.TimePayrollTransactions.Count(x => x.State == (int)SoeEntityState.Active) == 1)
                    {
                        timeCodeTransaction.UnitPrice = template.Outcome.TimePayrollTransactions.First(x => x.State == (int)SoeEntityState.Active).UnitPrice;
                        timeCodeTransaction.Amount = template.Outcome.TimePayrollTransactions.First(x => x.State == (int)SoeEntityState.Active).Amount;
                    }
                }

                result = Save();
                if (!result.Success)
                    return result;

                #endregion
            }

            #endregion

            #region Check payroll warnings            

            if (timePeriodId.HasValue)
                ActivateWarningPayrollPeriodHasChanged(employeeId, timePeriodId.Value);
            else
                DoInitiatePayrollWarnings();

            #endregion

            return result;
        }

        private ActionResult DeleteExpenseRow(int expenseRowId, bool noErrorIfExpenseRowNotFound = false)
        {
            #region Prereq

            AttestStateDTO initialAttestState = GetAttestStateInitialFromCache(TermGroup_AttestEntity.PayrollTime);
            if (initialAttestState == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(8517, "Atteststatus - lägsta nivå saknas"));

            // Get expense row
            ExpenseRow expenseRow = GetExpenseRowWithHeadAndInvoiceRowAndTransactions(expenseRowId);
            if (expenseRow == null)
            {
                if (noErrorIfExpenseRowNotFound)
                    return new ActionResult(true);
                else
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "ExpenseRow");
            }

            #endregion

            #region Perform

            #region Set transactions to deleted

            if (expenseRow.TimeCodeTransaction != null)
            {
                if (expenseRow.TimeCodeTransaction.TimePayrollTransaction.Any(x => x.AttestStateId != initialAttestState.AttestStateId))
                    return new ActionResult((int)ActionResultSave.TimePayrollTransactionCannotDeleteNotInitialAttestState, GetText(5759, "Det finns attesterade transaktioner som inte kan tas bort"));

                ActionResult result = ChangeEntityState(expenseRow.TimeCodeTransaction, SoeEntityState.Deleted);
                if (!result.Success)
                    return result;

                foreach (var timePayrollTransaction in expenseRow.TimeCodeTransaction.TimePayrollTransaction)
                {
                    result = ChangeEntityState(timePayrollTransaction, SoeEntityState.Deleted);
                    if (!result.Success)
                        return result;
                }
            }

            #endregion

            ChangeEntityState(expenseRow.ExpenseHead, SoeEntityState.Deleted);
            ChangeEntityState(expenseRow, SoeEntityState.Deleted);

            if (expenseRow.CustomerInvoiceRow != null)
                ChangeEntityState(expenseRow.CustomerInvoiceRow, SoeEntityState.Deleted);

            #endregion

            return Save();
        }

        #endregion
    }
}

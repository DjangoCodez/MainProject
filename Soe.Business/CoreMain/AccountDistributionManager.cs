using SoftOne.Soe.Business.Core.AccountDistribution;
using SoftOne.Soe.Business.Core.AccountDistribution.Accrual;
using SoftOne.Soe.Business.Core.ManagerWrappers;
using SoftOne.Soe.Business.Core.Reporting.Models.Economy.Models;
using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Util;
using SoftOne.Soe.Util.Exceptions;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Transactions;

namespace SoftOne.Soe.Business.Core
{
    public class AccountDistributionManager : ManagerBase
    {
        #region Variables

        // Create a logger for use in this class
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #endregion

        #region Ctor

        public AccountDistributionManager(ParameterObject parameterObject) : base(parameterObject) { }

        #endregion

        #region AccountDistribution

        public List<AccountDistributionHead> GetAccountDistributionHeads(int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.AccountDistributionHead.NoTracking();
            return GetAccountDistributionHeads(entities, actorCompanyId);
        }

        public List<AccountDistributionHead> GetAccountDistributionHeads(CompEntities entities, int actorCompanyId)
        {
            return (from adh in entities.AccountDistributionHead
                    where adh.ActorCompanyId == actorCompanyId &&
                    adh.State == (int)SoeEntityState.Active
                    select adh).ToList();
        }


        public IEnumerable<AccountDistributionHead> GetAccountDistributionHeads(int actorCompanyId, SoeAccountDistributionType type, bool loadRows, bool loadOpen = true, bool loadClosed = true, bool loadEntries = false, bool loadAccount = false, int? accountDistributionHeadId = null)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.AccountDistributionHead.NoTracking();
            return GetAccountDistributionHeads(entities, actorCompanyId, type, loadRows, loadOpen, loadClosed, loadEntries, loadAccount, accountDistributionHeadId);
        }

        public IEnumerable<AccountDistributionHead> GetAccountDistributionHeads(CompEntities entities, int actorCompanyId, SoeAccountDistributionType type, bool loadRows, bool loadOpen, bool loadClosed, bool loadEntries, bool loadAccount, int? accountDistributionHeadId = null)
        {
            if (!loadClosed && !loadOpen)
                return new List<AccountDistributionHead>();
            else
            {
                IQueryable<AccountDistributionHead> headsQuery = (from adh in entities.AccountDistributionHead
                                                                  where adh.ActorCompanyId == actorCompanyId &&
                                                                  adh.Type == (int)type &&
                                                                  adh.State == (int)SoeEntityState.Active
                                                                  select adh);
                if (accountDistributionHeadId.HasValue)
                    headsQuery = headsQuery.Where(adh => adh.AccountDistributionHeadId == accountDistributionHeadId.Value);

                if (loadRows)
                    headsQuery = headsQuery.Include("AccountDistributionRow.AccountDistributionRowAccount");

                if (loadEntries)
                    headsQuery = headsQuery.Include("AccountDistributionEntry.AccountDistributionEntryRow");

                if (loadAccount)
                    headsQuery = headsQuery.Include("AccountDistributionHeadAccountDimMapping.AccountDim");

                if (loadOpen && loadClosed)
                    return headsQuery.OrderBy(adh => adh.Sort).ToList();
                else
                {
                    var currentDate = DateTime.Today.Date;
                    if (loadOpen)
                        headsQuery = headsQuery.Where(h => entities.AccountDistributionEntry.Any(ade => ade.AccountDistributionHeadId == h.AccountDistributionHeadId && ade.VoucherHeadId == null && ade.State == (int)SoeEntityState.Active) ||
                                                           (h.EndDate == null || h.EndDate >= currentDate));
                    if (loadClosed)
                        headsQuery = headsQuery.Where(h => !entities.AccountDistributionEntry.Any(ade => ade.AccountDistributionHeadId == h.AccountDistributionHeadId && ade.VoucherHeadId == null && ade.State == (int)SoeEntityState.Active) &&
                                                           (h.EndDate != null && h.EndDate < currentDate));
                    entities.CommandTimeout = 300;
                    return headsQuery.OrderBy(adh => adh.Sort).ToList();
                }
            }
        }

        /// <summary>
        /// Checks if given accountDistributionHead has any accountDistributionRow that contains the Accounts given in the AccountInterval list.
        /// Also loads the accountDistributionRows with properties from its accountDistributionHead
        /// </summary>
        /// <param name="accountDistributionHead">The accountDistributionHead to check</param>
        /// <param name="accountIntervals">The AccountIntervals the VoucherRow must contain</param>
        /// <param name="accountDimStdId">The AccountDim standard id</param>
        /// <returns>True if the VoucherHead has any VoucherRow that contains ALL Accounts in the given Interval</returns>
        public bool AccountDistributionHeadContainsAccounts(AccountDistributionHeadDTO accountDistributionHead, List<AccountIntervalDTO> accountIntervals, int accountDimStdId)
        {
            //Approve all if no filter given
            if (accountIntervals == null || !accountIntervals.Any())
                return true;

            foreach (AccountIntervalDTO accountInterval in accountIntervals)
            {
                foreach (AccountDistributionRowDTO accountDistributionRow in accountDistributionHead.Rows)
                {
                    if (AccountDistributionRowContainsAccount(accountDistributionRow, accountInterval, accountDimStdId))
                        return true;
                }
            }

            return false;
        }

        public bool AccountDistributionRowContainsAccount(AccountDistributionRowDTO accountDistributionRow, AccountIntervalDTO accountInterval, int accountDimStdId)
        {
            if (accountDistributionRow == null || accountInterval == null)
                return false;

            bool isAccountStd = accountInterval.AccountDimId == accountDimStdId;
            if (isAccountStd)
            {
                #region AccountStd

                if (Validator.IsAccountInInterval(accountDistributionRow.Dim1Nr, accountDimStdId, accountInterval))
                    return true;

                #endregion
            }
            return false;

        }

        /// <summary>
        /// Checks if given accountDistributionHead has any accountDistributionRow that contains the Accounts given in the AccountInterval list.
        /// Also loads the accountDistributionRows with properties from its accountDistributionHead
        /// </summary>
        /// <param name="accountDistributionHead">The accountDistributionHead to check</param>
        /// <param name="accountIntervals">The AccountIntervals the VoucherRow must contain</param>
        /// <param name="accountDimStdId">The AccountDim standard id</param>
        /// <returns>True if the VoucherHead has any VoucherRow that contains ALL Accounts in the given Interval</returns>
        public bool AccountDistributionEntryContainsAccounts(AccountDistributionEntry accountDistributionEntry, List<AccountIntervalDTO> accountIntervals, int accountDimStdId)
        {
            //Approve all if no filter given
            if (accountIntervals == null || !accountIntervals.Any())
                return true;

            foreach (AccountIntervalDTO accountInterval in accountIntervals)
            {
                foreach (AccountDistributionEntryRow entryRow in accountDistributionEntry.AccountDistributionEntryRow.ToList())
                {
                    if (AccountDistributionEntryRowContainsAccount(entryRow, accountInterval, accountDimStdId))
                        return true;
                }
            }

            return false;
        }

        public bool AccountDistributionEntryRowContainsAccount(AccountDistributionEntryRow entryRow, AccountIntervalDTO accountInterval, int accountDimStdId)
        {
            if (entryRow == null || accountInterval == null)
                return false;

            bool isAccountStd = accountInterval.AccountDimId == accountDimStdId;
            if (isAccountStd)
            {
                #region AccountStd

                if (Validator.IsAccountInInterval(entryRow.AccountStd.Account.AccountNr, accountDimStdId, accountInterval))
                    return true;

                #endregion
            }
            return false;

        }

        public List<AccountDistributionHead> GetAccountDistributionHeadsUsedIn(int actorCompanyId, SoeAccountDistributionType? type = null, DateTime? date = null, bool? useInVoucher = null, bool? useInSupplierInvoice = null, bool? useInCustomerInvoice = null, bool? useInImport = null, TermGroup_AccountDistributionTriggerType? triggerType = null, bool? useInPayrollVoucher = null, bool? useInPayrollVacationVoucher = null)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.AccountDistributionHead.NoTracking();
            return GetAccountDistributionHeadsUsedIn(entities, actorCompanyId, type, date, useInVoucher, useInSupplierInvoice, useInCustomerInvoice, useInImport, triggerType, useInPayrollVoucher, useInPayrollVacationVoucher);
        }

        /// <summary>
        /// Get account distribution heads for specified type and 'Use in'
        /// </summary>
        /// <param name="actorCompanyId">Company ID</param>
        /// <param name="type">Account distribution type</param>
        /// <param name="date">Date to use as condition if StartDate or EndDate is set on distribution head</param>
        /// <param name="useInVoucher">True if voucher distributions should be returned</param>
        /// <param name="useInSupplierInvoice">True if supplier invoice distributions should be returned</param>
        /// <param name="useInCustomerInvoice">True if customer invoice distributions should be returned</param>
        /// <returns>Collection of AccountDistributionHeads</returns>
        public List<AccountDistributionHead> GetAccountDistributionHeadsUsedIn(CompEntities entities, int actorCompanyId, SoeAccountDistributionType? type = null, DateTime? date = null, bool? useInVoucher = null, bool? useInSupplierInvoice = null, bool? useInCustomerInvoice = null, bool? useInImport = null, TermGroup_AccountDistributionTriggerType? triggerType = null, bool? useInPayrollVoucher = null, bool? useInPayrollVacationVoucher = null)
        {
            var query = from adh in entities.AccountDistributionHead
                        .Include("AccountDistributionHeadAccountDimMapping.AccountDim")
                        .Include("AccountDistributionRow.AccountStd.Account")
                        .Include("AccountDistributionRow.AccountDistributionRowAccount.AccountInternal.Account.AccountDim")
                        where adh.ActorCompanyId == actorCompanyId &&
                        adh.State == (int)SoeEntityState.Active
                        select adh;

            if (type != null)
                query = query.Where(adh => adh.Type == (int)type);

            if (triggerType != null)
                query = query.Where(adh => adh.TriggerType == (int)triggerType);

            if (date.HasValue)
            {
                query = query.Where(adh => (adh.StartDate == null || adh.StartDate.Value.Date <= date.Value.Date) &&
                                           (adh.EndDate == null || adh.EndDate.Value.Date >= date.Value.Date));
            }

            if (useInVoucher.HasValue)
                query = query.Where(adh => adh.UseInVoucher == useInVoucher.Value);
            if (useInSupplierInvoice.HasValue)
                query = query.Where(adh => adh.UseInSupplierInvoice == useInSupplierInvoice.Value);
            if (useInCustomerInvoice.HasValue)
                query = query.Where(adh => adh.UseInCustomerInvoice == useInCustomerInvoice.Value);
            if (useInImport.HasValue)
                query = query.Where(adh => adh.UseInImport == useInImport.Value);
            if (useInPayrollVoucher.HasValue)
                query = query.Where(adh => adh.UseInPayrollVoucher == useInPayrollVoucher.Value);
            if (useInPayrollVacationVoucher.HasValue)
                query = query.Where(adh => adh.UseInPayrollVacationVoucher == useInPayrollVacationVoucher.Value);

            query = query.OrderBy(adh => adh.Type).ThenBy(adh => adh.Sort);

            return query.ToList();
        }

        public List<AccountDistributionRow> GetDistributionRows(int accountDistributionHeadId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.AccountDistributionRow.NoTracking();
            return GetDistributionRows(entities, accountDistributionHeadId);
        }

        public List<AccountDistributionRow> GetDistributionRows(CompEntities entities, int accountDistributionHeadId)
        {
            return (from adr in entities.AccountDistributionRow
                            .Include("AccountStd")
                            .Include("AccountDistributionRowAccount")
                            .Include("AccountDistributionHead")
                    where adr.AccountDistributionHeadId == accountDistributionHeadId &&
                    adr.State == (int)SoeEntityState.Active
                    select adr).OrderBy(adr => adr.RowNbr).ToList();
        }

        public List<AccountDistributionRowDTO> GetAccountDistributionRows(int accountDistributionHeadId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.AccountDistributionRow.NoTracking();
            return GetAccountDistributionRows(entities, accountDistributionHeadId);
        }

        public List<AccountDistributionRowDTO> GetAccountDistributionRows(CompEntities entities, int accountDistributionHeadId)
        {
            List<AccountDistributionRowDTO> rowDTOs = new List<AccountDistributionRowDTO>();

            var query = (from adr in entities.AccountDistributionRow
                            .Include("AccountStd")
                            .Include("AccountDistributionRowAccount")
                         where adr.AccountDistributionHeadId == accountDistributionHeadId &&
                         adr.State == (int)SoeEntityState.Active
                         select adr).OrderBy(adr => adr.RowNbr);

            foreach (AccountDistributionRow row in query)
            {
                AccountDistributionRowDTO dto = row.ToDTO();

                #region AccountInternals

                AccountDistributionRowAccount rowAccount = row.AccountDistributionRowAccount.FirstOrDefault(a => a.DimNr == 2);
                if (rowAccount != null)
                {
                    dto.Dim2Id = rowAccount.AccountId.HasValue ? rowAccount.AccountId.Value : 0;
                    dto.Dim2KeepSourceRowAccount = rowAccount.KeepSourceRowAccount;
                }

                // Dim 3
                rowAccount = row.AccountDistributionRowAccount.FirstOrDefault(a => a.DimNr == 3);
                if (rowAccount != null)
                {
                    dto.Dim3Id = rowAccount.AccountId.HasValue ? rowAccount.AccountId.Value : 0;
                    dto.Dim3KeepSourceRowAccount = rowAccount.KeepSourceRowAccount;
                }

                // Dim 4
                rowAccount = row.AccountDistributionRowAccount.FirstOrDefault(a => a.DimNr == 4);
                if (rowAccount != null)
                {
                    dto.Dim4Id = rowAccount.AccountId.HasValue ? rowAccount.AccountId.Value : 0;
                    dto.Dim4KeepSourceRowAccount = rowAccount.KeepSourceRowAccount;
                }

                // Dim 5
                rowAccount = row.AccountDistributionRowAccount.FirstOrDefault(a => a.DimNr == 5);
                if (rowAccount != null)
                {
                    dto.Dim5Id = rowAccount.AccountId.HasValue ? rowAccount.AccountId.Value : 0;
                    dto.Dim5KeepSourceRowAccount = rowAccount.KeepSourceRowAccount;
                }

                // Dim 6
                rowAccount = row.AccountDistributionRowAccount.FirstOrDefault(a => a.DimNr == 6);
                if (rowAccount != null)
                {
                    dto.Dim6Id = rowAccount.AccountId.HasValue ? rowAccount.AccountId.Value : 0;
                    dto.Dim6KeepSourceRowAccount = rowAccount.KeepSourceRowAccount;
                }

                #endregion

                rowDTOs.Add(dto);
            }

            return rowDTOs;
        }

        public AccountDistributionHead GetAccountDistributionHead(int accountDistributionHeadId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.AccountDistributionHead.NoTracking();
            return GetAccountDistributionHead(entities, accountDistributionHeadId);
        }

        public AccountDistributionHead GetAccountDistributionHead(CompEntities entities, int accountDistributionHeadId)
        {
            return (from adh in entities.AccountDistributionHead
                        .Include("AccountDistributionHeadAccountDimMapping.AccountDim")
                        .Include("AccountDistributionRow.AccountStd.Account")
                        .Include("AccountDistributionRow.AccountDistributionRowAccount.AccountInternal.Account.AccountDim")
                        .Include("AccountDistributionEntry")
                    where adh.AccountDistributionHeadId == accountDistributionHeadId
                    select adh).FirstOrDefault();
        }

        public ActionResult SaveAccountDistribution(AccountDistributionHeadDTO distributionHeadDTOInput, List<AccountDistributionRowDTO> distributionRowsDTOInput, int actorCompanyId)
        {
            if (distributionHeadDTOInput == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "AccountDistributionHead");

            // Default result is successful
            ActionResult result = new ActionResult(true);

            bool distributionRowsChanged = false;
            bool datesChanged = false;
            DateTime startDate = distributionHeadDTOInput.StartDate ?? DateTime.Today;
            bool addDistribution = false;
            int distributionHeadId = distributionHeadDTOInput.AccountDistributionHeadId;

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        #region Prereq

                        // Get voucher series type
                        VoucherSeriesType voucherSeriesType = null;
                        if (distributionHeadDTOInput.VoucherSeriesTypeId.HasValue)
                            voucherSeriesType = VoucherManager.GetVoucherSeriesType(entities, distributionHeadDTOInput.VoucherSeriesTypeId.Value, actorCompanyId);

                        #endregion

                        #region Convert

                        // Convert collection of AccountDistributionRowDTOs to collection of AccountDistributionRows with connected AccountDistributionRowAccounts
                        List<AccountDistributionRow> distributionRowsInput = ConvertToAccountDistributionRows(entities, distributionRowsDTOInput, actorCompanyId);

                        #endregion

                        #region AccountDistributionHead

                        // Get existing AccountDistribution
                        AccountDistributionHead distributionHead = distributionHeadId != 0 ? GetAccountDistributionHead(entities, distributionHeadId) : null;
                        if (distributionHead == null)
                        {
                            addDistribution = true;

                            #region AccountDistributionHead Add

                            distributionHead = new AccountDistributionHead()
                            {
                                Type = distributionHeadDTOInput.Type,
                                Name = distributionHeadDTOInput.Name,
                                Description = distributionHeadDTOInput.Description,
                                TriggerType = (int)distributionHeadDTOInput.TriggerType,
                                CalculationType = (int)distributionHeadDTOInput.CalculationType,
                                Calculate = distributionHeadDTOInput.Calculate,
                                PeriodType = (int)distributionHeadDTOInput.PeriodType,
                                PeriodValue = distributionHeadDTOInput.PeriodValue,
                                Sort = distributionHeadDTOInput.Sort,
                                StartDate = distributionHeadDTOInput.StartDate,
                                EndDate = distributionHeadDTOInput.EndDate,
                                DayNumber = distributionHeadDTOInput.DayNumber,
                                Amount = distributionHeadDTOInput.Amount,
                                AmountOperator = distributionHeadDTOInput.AmountOperator,
                                KeepRow = distributionHeadDTOInput.KeepRow,
                                UseInVoucher = distributionHeadDTOInput.UseInVoucher,
                                UseInSupplierInvoice = distributionHeadDTOInput.UseInSupplierInvoice,
                                UseInCustomerInvoice = distributionHeadDTOInput.UseInCustomerInvoice,
                                UseInImport = distributionHeadDTOInput.UseInImport,
                                State = (int)SoeEntityState.Active,
                                UseInPayrollVoucher = distributionHeadDTOInput.UseInPayrollVoucher,
                                UseInPayrollVacationVoucher = distributionHeadDTOInput.UseInPayrollVacationVoucher,
                            };

                            // Set references
                            distributionHead.Company = CompanyManager.GetCompany(entities, actorCompanyId);
                            if (distributionHead.Company == null)
                                return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte"));

                            distributionHead.VoucherSeriesType = voucherSeriesType;

                            CreateExpressions(entities, distributionHeadDTOInput, distributionHead, actorCompanyId);

                            SetCreatedProperties(distributionHead);

                            #endregion
                        }
                        else
                        {
                            if (distributionHead.StartDate != distributionHeadDTOInput.StartDate || 
                                distributionHead.EndDate != distributionHeadDTOInput.EndDate)
                                datesChanged = true;

                            if (distributionHead.DayNumber != distributionHeadDTOInput.DayNumber)
                            {
                                datesChanged = true;
                                distributionRowsChanged = true;
                            }

                            #region AccountDistributionHead Update

                            distributionHead.Name = distributionHeadDTOInput.Name;
                            distributionHead.Description = distributionHeadDTOInput.Description;
                            distributionHead.TriggerType = (int)distributionHeadDTOInput.TriggerType;
                            distributionHead.CalculationType = (int)distributionHeadDTOInput.CalculationType;
                            distributionHead.Calculate = distributionHeadDTOInput.Calculate;
                            distributionHead.Sort = distributionHeadDTOInput.Sort;
                            distributionHead.PeriodType = (int)distributionHeadDTOInput.PeriodType;
                            distributionHead.PeriodValue = distributionHeadDTOInput.PeriodValue;
                            distributionHead.StartDate = distributionHeadDTOInput.StartDate;
                            distributionHead.EndDate = distributionHeadDTOInput.EndDate;
                            distributionHead.DayNumber = distributionHeadDTOInput.DayNumber;
                            distributionHead.Amount = distributionHeadDTOInput.Amount;
                            distributionHead.AmountOperator = distributionHeadDTOInput.AmountOperator;
                            distributionHead.KeepRow = distributionHeadDTOInput.KeepRow;
                            distributionHead.UseInVoucher = distributionHeadDTOInput.UseInVoucher;
                            distributionHead.UseInSupplierInvoice = distributionHeadDTOInput.UseInSupplierInvoice;
                            distributionHead.UseInCustomerInvoice = distributionHeadDTOInput.UseInCustomerInvoice;
                            distributionHead.UseInImport = distributionHeadDTOInput.UseInImport;
                            distributionHead.UseInPayrollVoucher = distributionHeadDTOInput.UseInPayrollVoucher;
                            distributionHead.UseInPayrollVacationVoucher = distributionHeadDTOInput.UseInPayrollVacationVoucher;

                            // Set foreign keys
                            distributionHead.VoucherSeriesType = voucherSeriesType;

                            distributionHead.AccountDistributionHeadAccountDimMapping.Clear();
                            CreateExpressions(entities, distributionHeadDTOInput, distributionHead, actorCompanyId);

                            SetModifiedProperties(distributionHead);

                            #endregion
                        }

                        #endregion

                        #region AccountDistributionRow

                        #region AccountDistributionRow Update/Delete

                        // Update or Delete existing AccountDistributionRows
                        foreach (AccountDistributionRow distributionRow in distributionHead.AccountDistributionRow.Where(r => r.State == (int)SoeEntityState.Active))
                        {
                            // Skip inactive or deleted rows
                            if (distributionRow.State != (int)SoeEntityState.Active)
                                continue;

                            // Try get AccountDistributionRow from input
                            AccountDistributionRow distributionRowInput = (from r in distributionRowsInput
                                                                           where r.AccountDistributionRowId == distributionRow.AccountDistributionRowId
                                                                           select r).FirstOrDefault();
                            distributionRowsChanged = true;

                            if (distributionRowInput != null)
                            {
                                #region AccountDistributionRow Update

                                // Update existing row
                                distributionRow.AccountStd = distributionRowInput.AccountStd;
                                distributionRow.RowNbr = distributionRowInput.RowNbr;
                                distributionRow.CalculateRowNbr = distributionRowInput.CalculateRowNbr;
                                distributionRow.SameBalance = distributionRowInput.SameBalance;
                                distributionRow.OppositeBalance = distributionRowInput.OppositeBalance;
                                distributionRow.Description = distributionRowInput.Description;
                                distributionRow.State = distributionRowInput.State;

                                // Update AccountInternal
                                distributionRow.AccountDistributionRowAccount.Clear();
                                foreach (AccountDistributionRowAccount rowAccount in distributionRowInput.AccountDistributionRowAccount.ToList())
                                {
                                    distributionRow.AccountDistributionRowAccount.Add(rowAccount);
                                }

                                // Detach the input row to prevent adding a new
                                base.TryDetachEntity(entities, distributionRowInput);

                                #endregion
                            }
                            else
                            {
                                #region AccountDistributionRow Delete

                                // Delete existing row
                                if (distributionRow.State != (int)SoeEntityState.Deleted)
                                    ChangeEntityState(distributionRow, SoeEntityState.Deleted);

                                #endregion
                            }
                        }

                        #endregion

                        #region AccountDistributionRow Add

                        // Get new AccountDistributionRows
                        IEnumerable<AccountDistributionRow> distributionRowsToAdd = (from r in distributionRowsInput
                                                                                     where r.AccountDistributionRowId == 0
                                                                                     select r).ToList();

                        foreach (AccountDistributionRow distributionRowToAdd in distributionRowsToAdd)
                        {
                            // Add AccountDistributionRow to AccountDistributionHead
                            distributionHead.AccountDistributionRow.Add(distributionRowToAdd);
                        }

                        #endregion

                        #endregion

                        // Add AccountDistributionHead to context
                        if (addDistribution)
                            entities.AccountDistributionHead.AddObject(distributionHead);

                        result = SaveChanges(entities, transaction);

                        if (result.Success && distributionHead.Type == (int)SoeAccountDistributionType.Period && ((datesChanged || distributionRowsChanged) && distributionHead.CalculationType == (int)TermGroup_AccountDistributionCalculationType.Amount))
                            {
                                result = UpdateAccruals(entities, transaction, startDate, distributionHeadId, actorCompanyId, datesChanged, distributionRowsChanged);

                                if (!result.Success)
                                    result = new ActionResult("Updating distribution entries failed.");
                            }

                        if (result.Success)
                        {
                            //Commit transaction
                            transaction.Complete();
                            distributionHeadId = distributionHead.AccountDistributionHeadId;
                        }
                    }
                }
                catch (Exception ex)
                {
                    base.LogError(ex, this.log);
                    result.Exception = ex;
                    result.IntegerValue = 0;
                }
                finally
                {
                    if (result.Success)
                    {
                        //Set success properties
                        result.IntegerValue = distributionHeadId;
                    }
                    else
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }

                return result;
            }
        }

        public ActionResult UpdatePreliminaryDistributionEntriesFromHead(CompEntities entities, TransactionScope transaction, int accountDistributionHeadId, int actorCompanyId, bool datesChanged, bool distributionRowsChanged)
        {
            ActionResult result = new ActionResult(true);

            #region Init

            HashSet<DateTime> processedDates = new HashSet<DateTime>();

            #endregion

            #region Prereq
            // Get AccountDims
            List<AccountDim> accountDims = AccountManager.GetAccountDimsByCompany(entities, actorCompanyId);

            var headDTO = GetAccountDistributionHead(entities, accountDistributionHeadId).ToDTO(true, true, accountDims);

            if (headDTO == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound);

            if (headDTO.CalculationType != TermGroup_AccountDistributionCalculationType.Amount)
                return new ActionResult((int)ActionResultSave.IncorrectInput);

            DateTime headStartDate = headDTO.StartDate.Value;
            DateTime headEndDate = headDTO.EndDate.GetValueOrDefault(DateTime.Today);
            DateTime firstDayOfEndDate = new DateTime(headEndDate.Year, headEndDate.Month, 1);
            DateTime lastDayOfEndDate = firstDayOfEndDate.AddMonths(1).AddDays(-1);

            int months = ((headEndDate.Year - headStartDate.Year) * 12) + headEndDate.Month - headStartDate.Month;
            if (headEndDate.Day == lastDayOfEndDate.Day || headEndDate.Day >= headDTO.DayNumber)
                months++;

            var existingEntries = entities.AccountDistributionEntry
                .Include("AccountDistributionEntryRow.AccountInternal.Account")
                .Where(e => e.AccountDistributionHeadId == accountDistributionHeadId &&
                            e.TriggerType == (int)TermGroup_AccountDistributionTriggerType.Distribution &&
                            e.State == (int)SoeEntityState.Active)
                .ToList();

            if (existingEntries.IsNullOrEmpty()) // No entries has been created yet -> nothing to update.
                return new ActionResult();

            var rowDtos = GetAccountDistributionRows(entities, accountDistributionHeadId);
            #endregion

            for (int month = 0; month < months; month++)
            {
                #region Init

                //Listan med alla konverterade verifikat
                var rowDTOs = new List<AccountingRowDTO>();

                #endregion

                #region Prereq
                DateTime periodDate = headStartDate.AddMonths(month);
                DateTime startDate = new DateTime(periodDate.Year, periodDate.Month, 1);
                DateTime endDate = startDate.AddMonths(1);
                AccountYear accountYear = AccountManager.GetAccountYear(entities, startDate, actorCompanyId, true);

                if (accountYear == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "AccountYear");

                DateTime yearStartDate = accountYear.From;

                //Hämta ut alla verifikat inom tidsintervallet
                List<VoucherRow> voucherRows = (from vr in entities.VoucherRow
                                                   .Include("AccountInternal.Account.AccountDim")
                                                   .Include("AccountStd.Account.AccountDim")
                                                where
                                                    vr.VoucherHead.Date >= startDate &&
                                                    vr.VoucherHead.Date < endDate &&
                                                    vr.VoucherHead.ActorCompanyId == actorCompanyId
                                                select vr).ToList();

                //Hämta ut resten av verifikat inom året
                List<VoucherRow> yearVoucherRows = (from vr in entities.VoucherRow
                                                   .Include("AccountInternal.Account.AccountDim")
                                                   .Include("AccountStd.Account.AccountDim")
                                                    where
                                                        vr.VoucherHead.Date >= yearStartDate &&
                                                        vr.VoucherHead.Date < startDate &&
                                                        vr.VoucherHead.ActorCompanyId == actorCompanyId
                                                    select vr).ToList();

                //Hämta ingåendebalans för året

                AccountBalanceManager abm = new AccountBalanceManager(parameterObject, actorCompanyId);
                List<AccountYearBalanceHead> balanceHeads = abm.GetAccountYearBalanceHeads(entities, accountYear.AccountYearId, actorCompanyId);

                #endregion

                int dayNumber = headDTO.DayNumber;
                if (headDTO.DayNumber < 1)
                    dayNumber = 1;
                else if (headDTO.DayNumber > endDate.AddDays(-1).Day)
                    dayNumber = endDate.AddDays(-1).Day;

                DateTime entryDate = new DateTime(periodDate.Year, periodDate.Month, dayNumber);

                var existingEntry = existingEntries.FirstOrDefault(e => e.Date.Month == periodDate.Month);
                var voucherRowsOriginal = voucherRows;

                if (existingEntry != null)
                {
                    if (existingEntry.VoucherHeadId != null)
                        continue;

                    if (distributionRowsChanged)
                    {
                        existingEntry.Date = entryDate;
                        foreach (AccountDistributionEntryRow entryRow in existingEntry.AccountDistributionEntryRow.ToList())
                        {
                            entryRow.AccountInternal.Clear();
                            entities.DeleteObject(entryRow);
                        }
                        existingEntry.AccountDistributionEntryRow.Clear();

                        #region periodtype: period or year
                        if (headDTO.PeriodType == TermGroup_AccountDistributionPeriodType.Period || headDTO.PeriodType == TermGroup_AccountDistributionPeriodType.Year)
                        {
                            rowDTOs.Clear();
                            voucherRows = voucherRowsOriginal;
                            AddVoucherToDTO(voucherRows, rowDTOs, accountDims);

                            if (headDTO.PeriodType == TermGroup_AccountDistributionPeriodType.Year)
                            {
                                AddVoucherToDTO(yearVoucherRows, rowDTOs, accountDims);
                                voucherRows = voucherRows.Concat(yearVoucherRows).ToList();
                                AddAccountBalanceToDTO(balanceHeads, rowDTOs);
                            }

                            List<AccountingRowDTO> matchedRowDTOs = MatchRowDTOs(headDTO, rowDTOs, accountDims);
                            if (matchedRowDTOs == null)
                                continue;

                            result = CreateAccountDistributionEntryRows(entities, existingEntry, rowDtos, headDTO.CalculationType, headDTO.PeriodType, actorCompanyId, matchedRowDTOs);
                        }
                        #endregion
                        

                        #region periodtype: amount
                        if (headDTO.PeriodType == TermGroup_AccountDistributionPeriodType.Amount)
                        {
                            rowDtos = GetAccountDistributionRows(entities, headDTO.AccountDistributionHeadId);
                            result = CreateAccountDistributionEntryRows(entities, existingEntry, rowDtos, headDTO.CalculationType, headDTO.PeriodType, actorCompanyId);
                        }
                        #endregion

                        if (!result.Success)
                            return result;
                    }

                    processedDates.Add(entryDate);
                }
                else
                {
                    #region Create new entry
                    AddVoucherToDTO(voucherRows, rowDTOs, accountDims);

                    List<AccountDistributionEntry> transferredEntrys = GetAccountDistributionEntries(entities, actorCompanyId, startDate, endDate).Where
                        (a => a.AccountDistributionHead.TriggerType == (int)TermGroup_AccountDistributionTriggerType.Distribution && a.VoucherHeadId != null).ToList();

                    #region periodtype: period or year
                    if (headDTO.PeriodType == TermGroup_AccountDistributionPeriodType.Period || headDTO.PeriodType == TermGroup_AccountDistributionPeriodType.Year)
                    {
                        if (transferredEntrys.Where(i => i.AccountDistributionHeadId == headDTO.AccountDistributionHeadId).ToList().Count > 0)
                            continue;

                        rowDTOs.Clear();
                        voucherRows = voucherRowsOriginal;
                        AddVoucherToDTO(voucherRows, rowDTOs, accountDims);

                        if (headDTO.PeriodType == TermGroup_AccountDistributionPeriodType.Year)
                        {
                            AddVoucherToDTO(yearVoucherRows, rowDTOs, accountDims);
                            voucherRows = voucherRows.Concat(yearVoucherRows).ToList();
                            AddAccountBalanceToDTO(balanceHeads, rowDTOs);
                        }

                        List<AccountingRowDTO> matchedRowDTOs = MatchRowDTOs(headDTO, rowDTOs, accountDims);
                        if (matchedRowDTOs == null)
                            continue;

                        result = CreateAccountDistributionEntry(entities, headDTO, actorCompanyId, entryDate, matchedRowDTOs, voucherRows);
                        if (!result.Success)
                            return result;
                    }
                    #endregion

                    #region periodtype: amount
                    if (headDTO.PeriodType == TermGroup_AccountDistributionPeriodType.Amount)
                    {
                        if (transferredEntrys.Where(i => i.AccountDistributionHeadId == headDTO.AccountDistributionHeadId).ToList().Count > 0)
                            continue;

                        result = CreateAccountDistributionEntry(entities, headDTO, actorCompanyId, entryDate);
                        if (!result.Success)
                            return result;
                    }
                    #endregion

                    processedDates.Add(entryDate);
                    #endregion
                }
            }

            // Remove entries that are now out of range
            if (datesChanged)
            {
                foreach (var oldEntry in existingEntries)
                {
                    if (!processedDates.Contains(oldEntry.Date.Date))
                    {
                        ActionResult deleteResult = DeleteAccountDistributionEntry(entities, oldEntry, false);
                        if (!deleteResult.Success)
                            return deleteResult;
                    }
                }
            }

            result = SaveChanges(entities, transaction);

            return result;
        }

        private List<AccountDistributionRow> ConvertToAccountDistributionRows(CompEntities entities, List<AccountDistributionRowDTO> distributionRowsDTOInput, int actorCompanyId)
        {
            List<AccountDistributionRow> rows = new List<AccountDistributionRow>();

            // Get internal accounts (Dim2-6)
            List<AccountInternal> accountInternals = AccountManager.GetAccountInternals(entities, actorCompanyId, true);

            foreach (AccountDistributionRowDTO item in distributionRowsDTOInput)
            {

                AccountDistributionRow row = new AccountDistributionRow()
                {
                    AccountDistributionRowId = item.AccountDistributionRowId,
                    RowNbr = item.RowNbr,
                    CalculateRowNbr = item.CalculateRowNbr,
                    SameBalance = item.SameBalance,
                    OppositeBalance = item.OppositeBalance,
                    Description = item.Description,
                    State = (int)item.State,

                    //Set references
                    AccountStd = item.Dim1Id.HasValue ? AccountManager.GetAccountStd(entities, (int)item.Dim1Id, actorCompanyId, true, false) : null,
                };

                #region AccountInternals

                //Dim 2
                if (item.Dim2Id != 0 || item.Dim2KeepSourceRowAccount)
                {
                    AccountInternal accountInternal = item.Dim2Id != 0 ? accountInternals.FirstOrDefault(a => a.AccountId == item.Dim2Id) : null;
                    AccountDistributionRowAccount rowAccount = new AccountDistributionRowAccount()
                    {
                        DimNr = 2,
                        AccountInternal = accountInternal,
                        KeepSourceRowAccount = item.Dim2KeepSourceRowAccount
                    };
                    row.AccountDistributionRowAccount.Add(rowAccount);
                }

                //Dim 3
                if (item.Dim3Id != 0 || item.Dim3KeepSourceRowAccount)
                {
                    AccountInternal accountInternal = item.Dim3Id != 0 ? accountInternals.FirstOrDefault(a => a.AccountId == item.Dim3Id) : null;
                    AccountDistributionRowAccount rowAccount = new AccountDistributionRowAccount()
                    {
                        DimNr = 3,
                        AccountInternal = accountInternal,
                        KeepSourceRowAccount = item.Dim3KeepSourceRowAccount
                    };
                    row.AccountDistributionRowAccount.Add(rowAccount);
                }

                //Dim 5
                if (item.Dim4Id != 0 || item.Dim4KeepSourceRowAccount)
                {
                    AccountInternal accountInternal = item.Dim4Id != 0 ? accountInternals.FirstOrDefault(a => a.AccountId == item.Dim4Id) : null;
                    AccountDistributionRowAccount rowAccount = new AccountDistributionRowAccount()
                    {
                        DimNr = 4,
                        AccountInternal = accountInternal,
                        KeepSourceRowAccount = item.Dim4KeepSourceRowAccount
                    };
                    row.AccountDistributionRowAccount.Add(rowAccount);
                }

                //Dim 5
                if (item.Dim5Id != 0 || item.Dim5KeepSourceRowAccount)
                {
                    AccountInternal accountInternal = item.Dim5Id != 0 ? accountInternals.FirstOrDefault(a => a.AccountId == item.Dim5Id) : null;
                    AccountDistributionRowAccount rowAccount = new AccountDistributionRowAccount()
                    {
                        DimNr = 5,
                        AccountInternal = accountInternal,
                        KeepSourceRowAccount = item.Dim5KeepSourceRowAccount
                    };
                    row.AccountDistributionRowAccount.Add(rowAccount);
                }

                //Dim 6
                if (item.Dim6Id != 0 || item.Dim6KeepSourceRowAccount)
                {
                    AccountInternal accountInternal = item.Dim6Id != 0 ? accountInternals.FirstOrDefault(a => a.AccountId == item.Dim6Id) : null;
                    AccountDistributionRowAccount rowAccount = new AccountDistributionRowAccount()
                    {
                        DimNr = 6,
                        AccountInternal = accountInternal,
                        KeepSourceRowAccount = item.Dim6KeepSourceRowAccount
                    };
                    row.AccountDistributionRowAccount.Add(rowAccount);
                }

                #endregion

                rows.Add(row);
            }

            return rows;
        }

        private void CreateExpressions(CompEntities entities, AccountDistributionHeadDTO distributionHeadDTOInput, AccountDistributionHead distributionHead, int actorCompanyId)
        {
            AccountDistributionHeadAccountDimMapping exp;

            exp = CreateExpression(entities, distributionHeadDTOInput.Dim1Id, distributionHeadDTOInput.Dim1Expression, actorCompanyId);
            if (exp != null)
                distributionHead.AccountDistributionHeadAccountDimMapping.Add(exp);

            exp = CreateExpression(entities, distributionHeadDTOInput.Dim2Id, distributionHeadDTOInput.Dim2Expression, actorCompanyId);
            if (exp != null)
                distributionHead.AccountDistributionHeadAccountDimMapping.Add(exp);

            exp = CreateExpression(entities, distributionHeadDTOInput.Dim3Id, distributionHeadDTOInput.Dim3Expression, actorCompanyId);
            if (exp != null)
                distributionHead.AccountDistributionHeadAccountDimMapping.Add(exp);

            exp = CreateExpression(entities, distributionHeadDTOInput.Dim4Id, distributionHeadDTOInput.Dim4Expression, actorCompanyId);
            if (exp != null)
                distributionHead.AccountDistributionHeadAccountDimMapping.Add(exp);

            exp = CreateExpression(entities, distributionHeadDTOInput.Dim5Id, distributionHeadDTOInput.Dim5Expression, actorCompanyId);
            if (exp != null)
                distributionHead.AccountDistributionHeadAccountDimMapping.Add(exp);

            exp = CreateExpression(entities, distributionHeadDTOInput.Dim6Id, distributionHeadDTOInput.Dim6Expression, actorCompanyId);
            if (exp != null)
                distributionHead.AccountDistributionHeadAccountDimMapping.Add(exp);
        }

        private AccountDistributionHeadAccountDimMapping CreateExpression(CompEntities entities, int accountDimId, string expression, int actorCompanyId)
        {
            AccountDistributionHeadAccountDimMapping exp = null;

            if (accountDimId != 0 && !String.IsNullOrEmpty(expression))
            {
                AccountDim accountDm = AccountManager.GetAccountDim(entities, accountDimId, actorCompanyId);
                if (accountDm != null)
                {
                    exp = new AccountDistributionHeadAccountDimMapping()
                    {
                        AccountDim = accountDm,
                        AccountExpression = expression
                    };
                }
            }

            return exp;
        }

        public ActionResult DeleteAccountDistribution(int accountDistributionHeadId)
        {
            using (CompEntities entities = new CompEntities())
            {
                // Get account distribution
                AccountDistributionHead distributionHead = GetAccountDistributionHead(entities, accountDistributionHeadId);
                if (distributionHead == null)
                    return new ActionResult((int)ActionResultDelete.EntityNotFound, "AccountDistributionHead");

                // Set account distribution state to deleted
                return ChangeEntityState(entities, distributionHead, SoeEntityState.Deleted, true);
            }
        }

        #endregion

        #region AccountDistributionEntry

        public List<AccountDistributionEntry> GetAccountDistributionEntries(int actorCompanyId, DateTime periodDate)
        {
            DateTime startDate = new DateTime(periodDate.Year, periodDate.Month, 1);
            DateTime endDate = startDate.AddMonths(1);

            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.VoucherHead.NoTracking();
            return GetAccountDistributionEntries(entities, actorCompanyId, startDate, endDate);
        }

		public List<AccountDistributionEntry> GetAccountDistributionEntriesReportData(int actorCompanyId, DateTime periodDateStart, DateTime? periodDateEnd = null)
		{
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();

            var headIdQuery = entitiesReadOnly.AccountDistributionEntry
                .Where(w =>
                    w.Date >= periodDateStart &&
                    w.AccountDistributionHeadId != null &&
                    w.AccountDistributionHeadId > 0
                );

            if (periodDateEnd != null)
            {
				DateTime endDate = ((DateTime)periodDateEnd).AddDays(1);
				endDate = new DateTime(endDate.Year, endDate.Month, endDate.Day);
				headIdQuery.Where(w => w.Date < endDate);
			}

			var headIdList = headIdQuery
                .Select(s => s.AccountDistributionHeadId ?? 0)
                .ToHashSet();

            var result = GetAccountDistributionEntriesQuery(entitiesReadOnly, actorCompanyId, false)
                .Where(w => w.AccountDistributionHead != null && 
                    headIdList.Contains(w.AccountDistributionHead.AccountDistributionHeadId));

			return result.ToList();
		}

		public List<AccountDistributionEntry> GetAccountDistributionEntries(CompEntities entities, int actorCompanyId, DateTime periodDateStart, DateTime periodDateEnd, bool checkHeadState = false)
		{
			return GetAccountDistributionEntriesQuery(entities, actorCompanyId, checkHeadState)
				  .Where(ade =>
					  ade.Date >= periodDateStart &&
					  ade.Date < periodDateEnd)
				  .ToList();
		}

		public IQueryable<AccountDistributionEntry> GetAccountDistributionEntriesQuery(CompEntities entities, int actorCompanyId, bool checkHeadState = false)
		{
			return (from ade in entities.AccountDistributionEntry
						.Include("AccountDistributionEntryRow.AccountInternal.Account.AccountDim")
						.Include("AccountDistributionEntryRow.AccountStd.Account.AccountDim")
						.Include("AccountDistributionHead")
						.Include("AccountDistributionEntryRow")
						.Include("VoucherHead.AccountPeriod.AccountYear")
					where ade.ActorCompanyId == actorCompanyId &&
					ade.State != 2 &&
					(checkHeadState ? ade.AccountDistributionHead.State == 0 : true) &&
					ade.InventoryId == null
					select ade);
		}

        public List<AccountDistributionEntryDTO> GetAccountDistributionEntriesForHead(int accountDistributionHeadId, int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.AccountDistributionHead.NoTracking();
            return GetAccountDistributionEntries(entities, actorCompanyId, accountDistributionHeadId);
        }

        public List<AccountDistributionEntryDTO> GetAccountDistributionEntries(CompEntities entities, int actorCompanyId, int accountDistributionHeadId)
        {
            return (from a in entities.AccountDistributionEntry
                      .Include("AccountDistributionEntryRow")
                    where a.ActorCompanyId == actorCompanyId &&
                      a.AccountDistributionHeadId == accountDistributionHeadId &&
                      a.State == 0
                    select a)
                    .ToList()
                    .ToDTOs(true);
        }

        public List<AccountDistributionEntryDTO> GetAccountDistributionEntriesDTO(int actorCompanyId, DateTime periodDate, SoeAccountDistributionType accountDistributionType, bool onlyActive = false)
        {
            List<AccountDistributionEntryDTO> entryDTOs = new List<AccountDistributionEntryDTO>();

            //Inventarie
            if (SoeAccountDistributionType.Period != accountDistributionType)
                return InventoryManager.GetAccountDistributionEntriesDTO(actorCompanyId, periodDate, accountDistributionType);

            int rowId = 1;
            List<AccountDistributionEntry> entrys = GetAccountDistributionEntries(actorCompanyId, periodDate);
            foreach (AccountDistributionEntry entry in entrys)
            {
                if (onlyActive && entry.State != (int)SoeEntityState.Active)
                {
                    continue;
                }

                AccountDistributionEntryDTO entryDTO = entry.ToDTO();
                entryDTO.VoucherSeriesTypeId = entry.AccountDistributionHead != null ? entry.AccountDistributionHead.VoucherSeriesTypeId : null;
                entryDTO.RowId = rowId;
                entryDTO.AccountDistributionEntryRowDTO = new List<AccountDistributionEntryRowDTO>();


                if (entry.VoucherHeadId == null && entry.State == (int)SoeEntityState.Active)
                {
                    entryDTO.Status = GetText(8058, "Preliminär");
                    entryDTO.IsSelectEnable = true;
                    entryDTO.State = entry.State;
                }
                else if (entry.VoucherHeadId != null && entry.State == (int)SoeEntityState.Active)
                {
                    if (entry.VoucherHead != null)
                        entryDTO.VoucherNr = entry.VoucherHead.VoucherNr;

                    entryDTO.IsSelectEnable = false;
                    entryDTO.Status = GetText(8059, "Överförd");
                    entryDTO.VoucherNr = entry.VoucherHead.VoucherNr;
                    entryDTO.AccountYearId = entry.VoucherHead.AccountPeriod.AccountYearId;
                    entryDTO.State = entry.State;
                }
                else
                {
                    entryDTO.Status = GetText(2244, "Borttagen");
                    entryDTO.State = entry.State;
                }

                if (entry.SourceSupplierInvoiceId.HasValue && entry.SourceSupplierInvoiceId != 0)
                {
                    SupplierInvoice suppInvoice = SupplierInvoiceManager.GetSupplierInvoice((int)entry.SourceSupplierInvoiceId, false, false, false, false, false, false, false, false);
                    if (suppInvoice == null)
                        continue;

                    entryDTO.SourceSupplierInvoiceSeqNr = suppInvoice.SeqNr;
                    entryDTO.InvoiceNr = suppInvoice.InvoiceNr;
                }
                else if (entry.SupplierInvoiceId.HasValue && entry.SupplierInvoiceId != 0)
                {
                    SupplierInvoice suppInvoice = SupplierInvoiceManager.GetSupplierInvoice((int)entry.SupplierInvoiceId, false, false, false, false, false, false, false, false);
                    if (suppInvoice == null)
                        continue;

                    entryDTO.SourceSupplierInvoiceSeqNr = suppInvoice.SeqNr;
                    entryDTO.InvoiceNr = suppInvoice.InvoiceNr;
                }
                else if (entry.SourceCustomerInvoiceId.HasValue && entry.SourceCustomerInvoiceId != 0)
                {
                    CustomerInvoice custInvoice = InvoiceManager.GetCustomerInvoice((int)entry.SourceCustomerInvoiceId);
                    if (custInvoice == null)
                        continue;

                    entryDTO.SourceCustomerInvoiceSeqNr = custInvoice.SeqNr;
                    entryDTO.InvoiceNr = custInvoice.InvoiceNr;
                }
                else if (entry.SourceVoucherHeadId.HasValue && entry.SourceVoucherHeadId != 0)
                {
                    VoucherHead voucher = VoucherManager.GetVoucherHead((int)entry.SourceVoucherHeadId);
                    if (voucher == null)
                        continue;

                    entryDTO.SourceVoucherNr = voucher.VoucherNr;
                }

                if (entry.AccountDistributionHead != null)
                {
                    switch (entry.TriggerType)
                    {
                        case (int)TermGroup_AccountDistributionTriggerType.Registration:
                            if (entryDTO.RegistrationType == TermGroup_AccountDistributionRegistrationType.CustomerInvoice)
                                entryDTO.TypeName = GetText(2, 544, "Kundfaktura");
                            else if (entryDTO.RegistrationType == TermGroup_AccountDistributionRegistrationType.SupplierInvoice)
                                entryDTO.TypeName = GetText(1, 544, "Lev.faktura");
                            else if (entryDTO.RegistrationType == TermGroup_AccountDistributionRegistrationType.Voucher)
                                entryDTO.TypeName = GetText(3, 544, "Verifikat");
                            else
                                entryDTO.TypeName = GetText(8062, "Direkt");
                            break;
                        case (int)TermGroup_AccountDistributionTriggerType.Distribution:
                            if (entryDTO.PeriodType == TermGroup_AccountDistributionPeriodType.Period)
                                entryDTO.TypeName = GetText(8060, "Periodsaldo");
                            if (entryDTO.PeriodType == TermGroup_AccountDistributionPeriodType.Year)
                                entryDTO.TypeName = GetText(8061, "Årsaldo");
                            if (entryDTO.PeriodType == TermGroup_AccountDistributionPeriodType.Amount)
                                entryDTO.TypeName = GetText(4836, "Belopp");
                            break;
                        default:
                            entryDTO.TypeName = GetText(8063, "Typ okänd");
                            break;
                    }
                }

                //TODO: kolla spårningen här, hur borde det skickas med? Bara som en textsträng eller ska det kunna bli en länk?
                foreach (AccountDistributionEntryRow entryRow in entry.AccountDistributionEntryRow)
                {
                    AccountDistributionEntryRowDTO entryRowDTO = new AccountDistributionEntryRowDTO();
                    entryRowDTO.AccountDistributionEntryId = entry.AccountDistributionEntryId;
                    entryRowDTO.AccountDistributionEntryRowId = entryRow.AccountDistributionEntryRowId;
                    entryRowDTO.SameBalance = entryRow.DebitAmount;
                    entryRowDTO.OppositeBalance = entryRow.CreditAmount;

                    entryDTO.Amount += entryRow.DebitAmount;

                    #region AccountStd

                    AccountStd accountStd = entryRow.AccountStd;
                    if (accountStd != null)
                    {
                        entryRowDTO.Dim1Id = accountStd.AccountId;
                        entryRowDTO.Dim1Nr = accountStd.Account.AccountNr;
                        entryRowDTO.Dim1Name = accountStd.Account.Name;
                        entryRowDTO.Dim1DimName = accountStd.Account.AccountDim.Name;
                    }

                    #endregion

                    #region AccountInternals

                    foreach (AccountInternal accountInternal in entryRow.AccountInternal)
                    {
                        switch (accountInternal.Account.AccountDim.AccountDimNr)
                        {
                            case 2:
                                entryRowDTO.Dim2Id = accountInternal.AccountId;
                                entryRowDTO.Dim2Nr = accountInternal.Account.AccountNr;
                                entryRowDTO.Dim2Name = accountInternal.Account.Name;
                                entryRowDTO.Dim2DimName = accountInternal.Account.AccountDim.Name;
                                break;
                            case 3:
                                entryRowDTO.Dim3Id = accountInternal.AccountId;
                                entryRowDTO.Dim3Nr = accountInternal.Account.AccountNr;
                                entryRowDTO.Dim3Name = accountInternal.Account.Name;
                                entryRowDTO.Dim3DimName = accountInternal.Account.AccountDim.Name;
                                break;
                            case 4:
                                entryRowDTO.Dim4Id = accountInternal.AccountId;
                                entryRowDTO.Dim4Nr = accountInternal.Account.AccountNr;
                                entryRowDTO.Dim4Name = accountInternal.Account.Name;
                                entryRowDTO.Dim4DimName = accountInternal.Account.AccountDim.Name;
                                break;
                            case 5:
                                entryRowDTO.Dim5Id = accountInternal.AccountId;
                                entryRowDTO.Dim5Nr = accountInternal.Account.AccountNr;
                                entryRowDTO.Dim5Name = accountInternal.Account.Name;
                                entryRowDTO.Dim5DimName = accountInternal.Account.AccountDim.Name;
                                break;
                            case 6:
                                entryRowDTO.Dim6Id = accountInternal.AccountId;
                                entryRowDTO.Dim6Nr = accountInternal.Account.AccountNr;
                                entryRowDTO.Dim6Name = accountInternal.Account.Name;
                                entryRowDTO.Dim6DimName = accountInternal.Account.AccountDim.Name;
                                break;
                        }
                    }

                    #endregion

                    entryDTO.AccountDistributionEntryRowDTO.Add(entryRowDTO);
                }

                entryDTOs.Add(entryDTO);

                rowId++;
            }

            return entryDTOs;
        }

        public AccountDistributionEntry GetAccountDistributionEntry(CompEntities entities, int actorCompanyId, int accountDistributionEntryId)
        {
            return (from m in entities.AccountDistributionEntry
                        .Include("AccountDistributionHead")
                        .Include("Inventory")
                        .Include("InventoryLog")
                        .Include("AccountDistributionEntryRow.AccountInternal.Account")
                    where m.AccountDistributionEntryId == accountDistributionEntryId && m.ActorCompanyId == actorCompanyId
                    select m).FirstOrDefault();
        }

        public List<AccountDistributionEntryDTO> GetAccountDistributionEntryDTOsForSource(int actorCompanyId, int accountDistributionHeadId, int registrationType, int sourceId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.AccountDistributionHead.NoTracking();
            return GetAccountDistributionEntriesForSource(entities, actorCompanyId, accountDistributionHeadId, registrationType, sourceId);
        }

        public List<AccountDistributionEntryDTO> GetAccountDistributionEntriesForSource(CompEntities entities, int actorCompanyId, int accountDistributionHeadId, int registrationType, int sourceId)
        {

            List<AccountDistributionEntry> entries = (from a in entities.AccountDistributionEntry
                                                      where a.ActorCompanyId == actorCompanyId &&
                                                      a.AccountDistributionHeadId == accountDistributionHeadId &&
                                                      a.RegistrationType == registrationType &&
                                                      (a.SourceCustomerInvoiceId == sourceId ||
                                                      a.SourceSupplierInvoiceId == sourceId ||
                                                      a.SourceVoucherHeadId == sourceId)
                                                      select a).ToList();

            List<AccountDistributionEntryDTO> entryDTOs = new List<AccountDistributionEntryDTO>();

            foreach (AccountDistributionEntry entry in entries)
            {
                AccountDistributionEntryDTO entryDTO = entry.ToDTO();
                entryDTOs.Add(entryDTO);
            }

            return entryDTOs;
        }

        public List<AccountDistributionEntry> GetAccountDistributionEntriesForSourceRow(CompEntities entities, int actorCompanyId, int registrationType, int sourceId, int sourceRowId)
        {
            List<AccountDistributionEntry> entries = (from a in entities.AccountDistributionEntry
                                                      where a.ActorCompanyId == actorCompanyId &&
                                                      a.RegistrationType == registrationType &&
                                                      (a.SourceCustomerInvoiceId == sourceId ||
                                                      a.SourceSupplierInvoiceId == sourceId ||
                                                      a.SourceVoucherHeadId == sourceId) &&
                                                      a.SourceRowId == sourceRowId
                                                      select a).ToList();

            return entries;
        }

        public ActionResult TransferAccountDistributionEntryDTOsToVoucher(List<AccountDistributionEntryDTO> inputEntryDTOs, int actorCompanyId, SoeAccountDistributionType accountDistributionType)
        {
            var result = new ActionResult();
            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();
                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        result = CreateTransferAccountDistributionEntryToVoucherTransaction(entities, inputEntryDTOs, actorCompanyId, accountDistributionType);
                        if (!result.Success)
                            return result;

                        result = SaveChanges(entities, transaction);
                        if (!result.Success)
                            return result;

                        transaction.Complete();
                    }
                }
                catch (Exception ex)
                {
                    base.LogError(ex, this.log);
                    result.Exception = ex;
                    result.Value = 0;
                }
                finally
                {
                    if (!result.Success)
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }
            }
            return result;
        }

        public ActionResult CreateTransferAccountDistributionEntryToVoucherTransaction(CompEntities entities, List<AccountDistributionEntryDTO> inputEntryDTOs, int actorCompanyId, SoeAccountDistributionType accountDistributionType, bool doReverse = false)
        {
            /**
             * This should be refactored. It's difficult to use at this point.
             * 1) Separate method for transforming EntryDTO to Entry
             * 2) Separate method for updating AccountDistributionHead/Inventory
             */
            ActionResult result = new ActionResult(true);
            #region Init

            var entryDTOs = new List<AccountDistributionEntryDTO>();
            var entryVoucherMapping = new Dictionary<int, List<int>>();
            bool mergeDistributionEntries = true;
            if (accountDistributionType == SoeAccountDistributionType.Period)
                mergeDistributionEntries = !SettingManager.GetBoolSetting(entities, SettingMainType.Company, (int)CompanySettingType.AccountingSeparateVouchersInPeriodAccounting, 0, actorCompanyId, 0);
            if (accountDistributionType != SoeAccountDistributionType.Auto && accountDistributionType != SoeAccountDistributionType.Period)
                mergeDistributionEntries = !SettingManager.GetBoolSetting(entities, SettingMainType.Company, (int)CompanySettingType.InventorySeparateVouchersInWriteOffs, 0, actorCompanyId, 0);

            #endregion

            #region Prereq

            List<AccountStd> accountStds = AccountManager.GetAccountStdsByCompany(entities, actorCompanyId, true);
            List<AccountInternal> accountInternals = AccountManager.GetAccountInternals(entities, actorCompanyId, true);

            #endregion

            if (!mergeDistributionEntries)
            {
                #region No merge

                entryDTOs = inputEntryDTOs.ToList();

                //onödigt här egentligen, men gör det för att det alltid ska vara likadant
                foreach (var accountDistributionEntryId in entryDTOs.Select(x => x.AccountDistributionEntryId))
                {
                    //Add mapping
                    entryVoucherMapping.Add(accountDistributionEntryId, new List<int>());
                    entryVoucherMapping[accountDistributionEntryId].Add(accountDistributionEntryId);
                }

                #endregion
            }
            else
            {
                #region Merge

                var entryRowDTOsDict = new Dictionary<KeyValuePair<int?, DateTime>, List<AccountDistributionEntryRowDTO>>();

                //Se till att slå samman alla rader efter datum först, så vi lätt kan merga ihop dem sen
                foreach (AccountDistributionEntryDTO entryDTO in inputEntryDTOs)
                {
                    var key = new KeyValuePair<int?, DateTime>(entryDTO.VoucherSeriesTypeId, entryDTO.Date.Date);
                    if (!entryRowDTOsDict.ContainsKey(key))
                        entryRowDTOsDict.Add(key, new List<AccountDistributionEntryRowDTO>());

                    foreach (AccountDistributionEntryRowDTO entryRowDTO in entryDTO.AccountDistributionEntryRowDTO)
                    {
                        entryRowDTOsDict[key].Add(entryRowDTO);
                    }
                }

                int counter = 1;
                foreach (var pair in entryRowDTOsDict)
                {
                    AccountDistributionEntryDTO newEntryDTO = new AccountDistributionEntryDTO
                    {
                        AccountDistributionEntryId = counter,
                        Date = pair.Key.Value,
                        VoucherSeriesTypeId = pair.Key.Key,
                        AccountDistributionEntryRowDTO = new List<AccountDistributionEntryRowDTO>(),
                    };

                    //Add mapping
                    entryVoucherMapping.Add(counter, new List<int>());

                    List<AccountDistributionEntryRowDTO> entryRowDTOs = entryRowDTOsDict[pair.Key];
                    foreach (AccountDistributionEntryRowDTO entryRowDTOOuter in entryRowDTOs)
                    {
                        if (entryRowDTOOuter.SameBalance != 0 || entryRowDTOOuter.OppositeBalance != 0)
                        {
                            AccountDistributionEntryRowDTO newEntryRowDTO = new AccountDistributionEntryRowDTO();
                            EntityUtil.CopyDTO(newEntryRowDTO, entryRowDTOOuter);
                            newEntryRowDTO.SameBalance = 0;
                            newEntryRowDTO.OppositeBalance = 0;

                            foreach (AccountDistributionEntryRowDTO entryRowDTOInner in entryRowDTOs.Where(p => p.Dim1Id == entryRowDTOOuter.Dim1Id && p.Dim2Id == entryRowDTOOuter.Dim2Id && p.Dim3Id == entryRowDTOOuter.Dim3Id && p.Dim4Id == entryRowDTOOuter.Dim4Id && p.Dim5Id == entryRowDTOOuter.Dim5Id && p.Dim6Id == entryRowDTOOuter.Dim6Id))
                            {
                                newEntryRowDTO.OppositeBalance += entryRowDTOInner.OppositeBalance;
                                newEntryRowDTO.SameBalance += entryRowDTOInner.SameBalance;

                                entryRowDTOInner.SameBalance = 0;
                                entryRowDTOInner.OppositeBalance = 0;

                                if (!entryVoucherMapping[counter].Any(a => a == entryRowDTOInner.AccountDistributionEntryId))
                                    entryVoucherMapping[counter].Add(entryRowDTOInner.AccountDistributionEntryId);
                            }

                            if (newEntryRowDTO.OppositeBalance != 0 || newEntryRowDTO.SameBalance != 0)
                                newEntryDTO.AccountDistributionEntryRowDTO.Add(newEntryRowDTO);
                        }
                    }

                    entryDTOs.Add(newEntryDTO);
                    counter++;
                }

                #endregion
            }

            #region AccountPeriod and VoucherSeries

            //liten fuling här för att slippa hämta upp för varje verifikat som ska skapas, eftersom gridden ändå bara hanterar hela månader, så borde vi kunna anta att alla åtminstone ligger på samma år
            DateTime month = DateTime.Now;
            if (entryDTOs.Count > 0)
                month = entryDTOs[0].Date;

            int accountYearId = AccountManager.GetAccountYearId(entities, month, actorCompanyId);
            List<AccountPeriod> accountPeriods = AccountManager.GetAccountPeriods(entities, accountYearId, false);
            List<VoucherSeries> voucherSeries = VoucherManager.GetVoucherSeriesByYear(entities, accountYearId, actorCompanyId, false);

            #endregion

            #region Transfer to Voucher

            foreach (AccountDistributionEntryDTO entryDTO in entryDTOs)
            {
                long seqNbr = 0;
                VoucherSeries voucherSerie = voucherSeries.FirstOrDefault(a => a.VoucherSeriesType.VoucherSeriesTypeId == entryDTO.VoucherSeriesTypeId);

                if (voucherSerie == null)
                {
                    var typeName = VoucherManager.GetVoucherSeriesType(entities, entryDTO.VoucherSeriesTypeId.GetValueOrDefault(), actorCompanyId)?.Name;
                    return new ActionResult(string.Format(GetText(7536, "Verifikatserie av typen {0} är inte upplagt för redovisningsåret"), typeName));
                }

                seqNbr = voucherSerie.VoucherNrLatest.GetValueOrDefault() + 1;

                //Check if period is opened
                var accountPeriod = accountPeriods.FirstOrDefault(a => a.From <= entryDTO.Date.Date && a.To >= entryDTO.Date.Date);

                result = AccountManager.ValidateAccountPeriod(accountPeriod);

                if (!result.Success)
                    return result;

                #region VoucherHead

                var voucherHead = new VoucherHead
                {
                    Date = entryDTO.Date,
                    Status = (int)TermGroup_AccountStatus.Open,
                    VoucherNr = seqNbr,
                    Text = string.Empty,

                    //Set Fk
                    ActorCompanyId = actorCompanyId,

                    //Set references
                    AccountPeriod = accountPeriod,
                    VoucherSeries = voucherSerie,
                };
                SetCreatedProperties(voucherHead);
                entities.VoucherHead.AddObject(voucherHead);

                string voucherText = string.Empty;

                //Uppdatera AccountDistributionEntry
                foreach (int entryId in entryVoucherMapping[entryDTO.AccountDistributionEntryId])
                {
                    AccountDistributionEntry entry = GetAccountDistributionEntry(entities, actorCompanyId, entryId);
                    if (entry == null)
                        continue;

                    //try to prevent double vouchers
                    if (entry.VoucherHead != null)
                        continue;

                    //Add to VoucherHead
                    voucherHead.AccountDistributionEntry.Add(entry);
                    entry.VoucherHead = voucherHead;

                    if (entry.InventoryLog != null)
                    {
                        foreach (InventoryLog inventoryLog in entry.InventoryLog)
                        {
                            //Add to VoucherHead
                            voucherHead.InventoryLog.Add(inventoryLog);
                            inventoryLog.VoucherHead = voucherHead;
                            if (!doReverse) ApplyDistributionEntryOnInventory(entry, entry.Inventory, inventoryLog);
                        }
                    }

                    if (!String.IsNullOrEmpty(voucherText))
                        voucherText += ", ";

                    if (entry.Inventory != null)
                        voucherText += entry.Inventory.Name;
                    else
                        voucherText += entry.AccountDistributionHead.Name;
                }

                voucherHead.Text = voucherText;

                //Update VoucherSerie
                if (voucherSerie != null)
                {
                    voucherSerie.VoucherNrLatest = seqNbr;
                    voucherSerie.VoucherDateLatest = entryDTO.Date;
                }

                #endregion

                #region VoucherRow

                foreach (AccountDistributionEntryRowDTO entryRowDTO in entryDTO.AccountDistributionEntryRowDTO)
                {
                    VoucherRow voucherRow = new VoucherRow
                    {
                        Amount = entryRowDTO.SameBalance - entryRowDTO.OppositeBalance,
                        Date = entryDTO.Date,

                        //Set references
                        AccountStd = accountStds.FirstOrDefault(a => a.AccountId == entryRowDTO.Dim1Id),
                    };

                    //Set currency amounts
                    CountryCurrencyManager.SetCurrencyAmounts(entities, actorCompanyId, voucherRow);

                    foreach (AccountInternal accountInternal in accountInternals.Where(a => a.AccountId == entryRowDTO.Dim2Id || a.AccountId == entryRowDTO.Dim3Id || a.AccountId == entryRowDTO.Dim4Id || a.AccountId == entryRowDTO.Dim5Id || a.AccountId == entryRowDTO.Dim6Id))
                    {
                        voucherRow.AccountInternal.Add(accountInternal);
                    }

                    voucherHead.VoucherRow.Add(voucherRow);
                }

                #endregion
            }

            #endregion
            return result;
        }

        public ActionResult DeleteAccountDistributionEntries(List<AccountDistributionEntryDTO> inputEntryDTOs, int actorCompanyId, SoeAccountDistributionType accountDistributionType)
        {
            ActionResult result = new ActionResult(true);

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {

                        foreach (AccountDistributionEntryDTO entryDTO in inputEntryDTOs)
                        {
                            AccountDistributionEntry entry = GetAccountDistributionEntry(entities, actorCompanyId, entryDTO.AccountDistributionEntryId);

                            #region Extra layer of validation for Inventory_WriteOff
                                if (accountDistributionType == SoeAccountDistributionType.Inventory_WriteOff && entry.VoucherHeadId != null && entry.State == (int)SoeEntityState.Active)
                            {
                                continue;
                            }
                            #endregion

                            result = DeleteAccountDistributionEntry(entities, entry);
                        }

                        //Commit transaction
                        if (result.Success)
                            transaction.Complete();
                    }
                }
                catch (Exception ex)
                {
                    base.LogError(ex, this.log);
                    result.Exception = ex;
                    result.Value = 0;
                }
                finally
                {
                    if (result.Success)
                    {
                        //Set success properties
                    }
                    else
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }

                return result;
            }
        }

        public ActionResult DeleteAccountDistributionEntriesPermanently(AccountDistributionEntryDTO inputEntryDTO, int actorCompanyId, SoeAccountDistributionType accountDistributionType)
        {
            ActionResult result = new ActionResult(true);

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {

                        AccountDistributionEntry entry = GetAccountDistributionEntry(entities, actorCompanyId, inputEntryDTO.AccountDistributionEntryId);
                        result = DeleteAccountDistributionEntryPermanently(entities, entry);

                        //Commit transaction
                        if (result.Success)
                            transaction.Complete();
                    }
                }
                catch (Exception ex)
                {
                    base.LogError(ex, this.log);
                    result.Exception = ex;
                    result.Value = 0;
                }
                finally
                {
                    if (result.Success)
                    {
                        //Set success properties
                    }
                    else
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }

                return result;
            }
        }

        public ActionResult DeleteAccountDistributionEntriesForSource(int actorCompanyId, int accountDistributionHeadId, int registrationType, int sourceId)
        {
            ActionResult result = new ActionResult(true);

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {

                        List<AccountDistributionEntry> entries = (from a in entities.AccountDistributionEntry
                                                                  where a.ActorCompanyId == actorCompanyId &&
                                                                  a.AccountDistributionHeadId == accountDistributionHeadId &&
                                                                  a.State == (int)SoeEntityState.Active &&
                                                                  a.VoucherHeadId == null &&
                                                                  a.RegistrationType == registrationType &&
                                                                  (a.SourceCustomerInvoiceId == sourceId ||
                                                                  a.SourceSupplierInvoiceId == sourceId ||
                                                                  a.SourceVoucherHeadId == sourceId)
                                                                  select a).ToList();

                        foreach (AccountDistributionEntry entry in entries)
                        {
                            DeleteAccountDistributionEntry(entities, entry);
                        }

                        //Commit transaction
                        if (result.Success)
                            transaction.Complete();
                    }
                }
                catch (Exception ex)
                {
                    base.LogError(ex, this.log);
                    result.Exception = ex;
                    result.Value = 0;
                }
                finally
                {
                    if (result.Success)
                    {
                        //Set success properties
                    }
                    else
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }

                return result;
            }
        }

        public ActionResult RestoreAccountDistributionEntries(AccountDistributionEntryDTO inputEntryDTO, int actorCompanyId, SoeAccountDistributionType accountDistributionType)
        {
            ActionResult result = new ActionResult(true);

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {

                        AccountDistributionEntry entry = GetAccountDistributionEntry(entities, actorCompanyId, inputEntryDTO.AccountDistributionEntryId);
                        result = RestoreAccountDistributionEntry(entities, entry);

                        //Commit transaction
                        if (result.Success)
                            transaction.Complete();
                    }
                }
                catch (Exception ex)
                {
                    base.LogError(ex, this.log);
                    result.Exception = ex;
                    result.Value = 0;
                }
                finally
                {
                    if (result.Success)
                    {
                        //Set success properties
                    }
                    else
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }

                return result;
            }
        }

        /// <summary>
        /// This has been replaced by CreateAccrualsForPeriod
        /// </summary>
        [Obsolete("Replaced by CreateAccrualsForPeriod")]
        public ActionResult TransferToAccountDistributionEntry(int actorCompanyId, DateTime periodDate, SoeAccountDistributionType accountDistributionType)
        {
            ActionResult result = new ActionResult(true);

            //Inventarie
            if (SoeAccountDistributionType.Period != accountDistributionType)
                return InventoryManager.TransferToAccountDistributionEntry(actorCompanyId, periodDate);
            
            using (CompEntities entities = new CompEntities())
            {
                #region Init

                //Listan med alla konverterade verifikat
                entities.CommandTimeout = 300;
                var rowDTOs = new List<AccountingRowDTO>();

                #endregion

                #region Prereq

                DateTime startDate = new DateTime(periodDate.Year, periodDate.Month, 1);
                DateTime endDate = startDate.AddMonths(1);
                AccountYear accountYear = AccountManager.GetAccountYear(startDate, actorCompanyId, true);

                if (accountYear == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "AccountYear");

                AccountPeriod accountPeriod = AccountManager.GetAccountPeriod(entities, startDate, actorCompanyId);
                result = AccountManager.ValidateAccountPeriod(accountPeriod, startDate);
                if (!result.Success)
                    return result;

                DateTime yearStartDate = accountYear.From;

                // Get AccountDims
                List<AccountDim> accountDims = AccountManager.GetAccountDimsByCompany(entities, actorCompanyId);

                //Hämta ut alla verifikat inom tidsintervallet
                List<VoucherRow> voucherRows = (from vr in entities.VoucherRow
                                                   .Include("AccountInternal.Account.AccountDim")
                                                   .Include("AccountStd.Account.AccountDim")
                                                where
                                                    vr.VoucherHead.Date >= startDate &&
													vr.VoucherHead.Date < endDate &&
													vr.VoucherHead.ActorCompanyId == actorCompanyId
                                                select vr).ToList();

                //Hämta ut resten av verifikat inom året
                List<VoucherRow> yearVoucherRows = (from vr in entities.VoucherRow
                                                   .Include("AccountInternal.Account.AccountDim")
                                                   .Include("AccountStd.Account.AccountDim")
                                                    where 
                                                        vr.VoucherHead.Date >= yearStartDate &&
														vr.VoucherHead.Date < startDate &&
														vr.VoucherHead.ActorCompanyId == actorCompanyId
                                                    select vr).ToList();

                //Hämta ingåendebalans för året

                AccountBalanceManager abm = new AccountBalanceManager(parameterObject, actorCompanyId);
                List<AccountYearBalanceHead> balanceHeads = abm.GetAccountYearBalanceHeads(accountYear.AccountYearId, actorCompanyId);

                #endregion

                //Radera de som man tidigare har lekt med
                List<AccountDistributionEntry> entrys = GetAccountDistributionEntries(entities, actorCompanyId, startDate, endDate);
                foreach (AccountDistributionEntry entry in entrys.Where
                    (a => a.AccountDistributionHead.TriggerType == (int)TermGroup_AccountDistributionTriggerType.Distribution && a.VoucherHeadId == null))
                {
                    int accountDistributionHeadId = entry.AccountDistributionHeadId.HasValue ? entry.AccountDistributionHeadId.Value : 0;
                    ActionResult result2 = DeleteAccountDistributionEntry(entities, entry);

                    if (result2.Success)
                    {
                        foreach (VoucherRow voucherRow in voucherRows.Where(a => a.AccountDistributionHeadId.HasValue && a.AccountDistributionHeadId == accountDistributionHeadId))
                        {
                            voucherRow.AccountDistributionHead = null;
                            voucherRow.AccountDistributionHeadId = null;
                        }
                    }
                }

                var voucherRowsOriginal = voucherRows;

                AddVoucherToDTO(voucherRows, rowDTOs, accountDims);

                //Plocka ut de AccountDistributionHeads som vi ska filtrera utifrån
                List<AccountDistributionHeadDTO> headDTOs = GetAccountDistributionHeadsUsedIn(actorCompanyId, SoeAccountDistributionType.Period, triggerType: TermGroup_AccountDistributionTriggerType.Distribution).ToDTOs(true, true, accountDims).ToList();

                headDTOs = headDTOs.Where(i => (i.StartDate == null || i.StartDate.Value.Date < endDate.Date) &&
                                               (i.EndDate == null || i.EndDate.Value.Date >= startDate.Date))
                                               .OrderBy(i => i.Sort).ToList();

                List<AccountDistributionEntry> transferredEntrys = GetAccountDistributionEntries(entities, actorCompanyId, startDate, endDate).Where
                    (a => a.AccountDistributionHead.TriggerType == (int)TermGroup_AccountDistributionTriggerType.Distribution && a.VoucherHeadId != null).ToList();

                #region periodtype: period or year
                foreach (AccountDistributionHeadDTO headDTO in headDTOs.Where(a => a.PeriodType == TermGroup_AccountDistributionPeriodType.Period || a.PeriodType == TermGroup_AccountDistributionPeriodType.Year).OrderBy(a => a.TriggerType))
                {
                    if (transferredEntrys.Where(i => i.AccountDistributionHeadId == headDTO.AccountDistributionHeadId).ToList().Count > 0)
                        continue;

                    int dayNumber = headDTO.DayNumber;
                    if (headDTO.DayNumber < 1)
                        dayNumber = 1;
                    else if (headDTO.DayNumber > endDate.AddDays(-1).Day)
                        dayNumber = endDate.AddDays(-1).Day;

                    DateTime entryDate = new DateTime(periodDate.Year, periodDate.Month, dayNumber);

                    rowDTOs.Clear();
                    voucherRows = voucherRowsOriginal;
                    AddVoucherToDTO(voucherRows, rowDTOs, accountDims);

                    if (headDTO.PeriodType == TermGroup_AccountDistributionPeriodType.Year)
                    {
                        AddVoucherToDTO(yearVoucherRows, rowDTOs, accountDims);
                        voucherRows = voucherRows.Concat(yearVoucherRows).ToList();
                        AddAccountBalanceToDTO(balanceHeads, rowDTOs);
                    }

                    List<AccountingRowDTO> matchedRowDTOs = MatchRowDTOs(headDTO, rowDTOs, accountDims);
                    if (matchedRowDTOs == null)
                        continue;

                    result = CreateAccountDistributionEntry(entities, headDTO, actorCompanyId, entryDate, matchedRowDTOs, voucherRows);
                    if (!result.Success)
                        return result;
                }
                #endregion

                #region periodtype: amount
                foreach (AccountDistributionHeadDTO headDTO in headDTOs.Where(a => a.PeriodType == TermGroup_AccountDistributionPeriodType.Amount).OrderBy(a => a.TriggerType))
                {
                    if (transferredEntrys.Where(i => i.AccountDistributionHeadId == headDTO.AccountDistributionHeadId).ToList().Count > 0)
                        continue;

                    int dayNumber = headDTO.DayNumber;
                    if (headDTO.DayNumber < 1)
                        dayNumber = 1;
                    else if (headDTO.DayNumber > endDate.AddDays(-1).Day)
                        dayNumber = endDate.AddDays(-1).Day;

                    DateTime entryDate = new DateTime(periodDate.Year, periodDate.Month, dayNumber);

                    result = CreateAccountDistributionEntry(entities, headDTO, actorCompanyId, entryDate);
                    if (!result.Success)
                        return result;
                }
                #endregion
            }

            return result;
        }

        public ActionResult CreateAccrualsForPeriod(int actorCompanyId, DateTime dateInPeriod)
        {
            using (var entities = new CompEntities())
            {
                var dim = AccountManager.GetAccountDimStd(entities, actorCompanyId, true);
                var accountYear = AccountManager.GetAccountYear(dateInPeriod, actorCompanyId, true);

                var dbService = new DBBulkService(entities, this);
                var dbUtility = new StateUtility(entities, this);
                var parameters = new AccrualGeneratorParameters(actorCompanyId, dateInPeriod, accountYear, dim);
                var currencySetter = new CurrencySetter(entities, actorCompanyId, this.CountryCurrencyManager);
                var queryService = new AccrualQueryService(entities, actorCompanyId, dim);
                var accrualGenerator = new PeriodicAccrualGenerator(parameters, queryService, currencySetter, dbService, dbUtility);

                return accrualGenerator.PerformGeneration();
            }
        }

        public ActionResult CreateAccrualsForAccountingRows(
            CompEntities entities, 
            TransactionScope transaction,
            int actorCompanyId,
            int sourceId,
            TermGroup_AccountDistributionRegistrationType registrationType,
            string accrualName)
        {
            var dim = AccountManager.GetAccountDimStd(entities, actorCompanyId, true);
            var accrualAccountMappings = AccountManager.GetAccrualAccountMappings(entities, actorCompanyId);
            int defaultAccrualCostAccId = SettingManager.GetCompanyIntSetting(entities, CompanySettingType.AccountCommonAccrualCostAccount);
            int defaultAccrualRevenueAccId = SettingManager.GetCompanyIntSetting(entities, CompanySettingType.AccountCommonAccrualRevenueAccount);
            var accrualCostAccount = AccountManager.GetAccount(entities, actorCompanyId, defaultAccrualCostAccId);
            var accrualRevenueAccount = AccountManager.GetAccount(entities, actorCompanyId, defaultAccrualRevenueAccId);
            var accountQuery = AccountManager.GetAccounts(entities, actorCompanyId);

            var dbService = new DBBulkService(entities, this, transaction);
            var dbUtility = new StateUtility(entities, this);
            var parameters = new AccountingRowAccrualGeneratorParameters(
                actorCompanyId,
                sourceId,
                dim,
                accrualAccountMappings, 
                registrationType, 
                accrualCostAccount.AccountId, 
                accrualRevenueAccount.AccountId,
                accrualName
                );
            var currencySetter = new CurrencySetter(entities, actorCompanyId, this.CountryCurrencyManager);
            var queryService = new AccrualQueryService(entities, actorCompanyId, dim);
            var accountService = new AccountService(entities, this.AccountManager);
            var accrualGenerator = new AccountingRowAccrualGenerator(parameters, queryService, currencySetter, dbService, dbUtility, accountService);

            return accrualGenerator.PerformGeneration();
        }
        public ActionResult UpdateAccruals(CompEntities entities, TransactionScope transaction, DateTime startDate, int accountDistributionHeadId, int actorCompanyId, bool datesChanged, bool distributionRowsChanged)
        {
            var dim = AccountManager.GetAccountDimStd(entities, actorCompanyId, true);
            var accountYear = AccountManager.GetAccountYear(entities, startDate, actorCompanyId, true);

            var dbService = new DBBulkService(entities, this, transaction);
            var dbUtility = new StateUtility(entities, this);
            var parameters = new AccrualUpdaterParameters(actorCompanyId, accountDistributionHeadId, datesChanged, distributionRowsChanged, dim, accountYear);
            var currencySetter = new CurrencySetter(entities, actorCompanyId, this.CountryCurrencyManager);
            var queryService = new AccrualQueryService(entities, actorCompanyId, dim);
            var accrualUpdater = new AccrualUpdater(parameters, queryService, currencySetter, dbService, dbUtility);

            return accrualUpdater.PerformUpdate();
        }

        public ActionResult CreateAccountDistributionEntry(CompEntities entities, AccountDistributionHeadDTO headDTO, int actorCompanyId, DateTime entryDate, List<AccountingRowDTO> matchedRowDTOs = null, List<VoucherRow> voucherRows = null)
        {
            ActionResult result = new ActionResult(true);

            if (headDTO.PeriodType == TermGroup_AccountDistributionPeriodType.Amount)
            {
                #region periodtype: amount
                AccountDistributionEntry entry = new AccountDistributionEntry()
                {
                    TriggerType = (int)headDTO.TriggerType,
                    Date = entryDate,
                    AccountDistributionHead = GetAccountDistributionHead(entities, headDTO.AccountDistributionHeadId)
                };

                List<AccountDistributionRowDTO> rowDtos = GetAccountDistributionRows(entities, headDTO.AccountDistributionHeadId);
                result = CreateAccountDistributionEntryRows(entities, entry, rowDtos, headDTO.CalculationType, headDTO.PeriodType, actorCompanyId);

                #endregion
            }
            else if (headDTO.PeriodType == TermGroup_AccountDistributionPeriodType.Period || headDTO.PeriodType == TermGroup_AccountDistributionPeriodType.Year)
            {
                #region periodtype: period or year

                if (matchedRowDTOs == null || voucherRows == null)
                    return new ActionResult((int)ActionResultSave.InsufficientInput);

                //När de slår ut varandra är det ingen ide att skapa upp poster
                decimal summa = matchedRowDTOs.Sum(a => a.Amount);
                if (summa != 0)
                {
                    #region AccountDistributionEntry

                    AccountDistributionEntry entry = new AccountDistributionEntry()
                    {
                        TriggerType = (int)headDTO.TriggerType,
                        Date = entryDate,

                        //Set references
                        AccountDistributionHead = GetAccountDistributionHead(entities, headDTO.AccountDistributionHeadId)
                    };

                    if (entry.AccountDistributionHead != null)
                    {
                        foreach (AccountingRowDTO matchedRowDTO in matchedRowDTOs)
                        {
                            VoucherRow row = voucherRows.FirstOrDefault(a => matchedRowDTO.InvoiceRowId != 0 && a.VoucherRowId == matchedRowDTO.InvoiceRowId);
                            if (row != null)
                                entry.AccountDistributionHead.VoucherRow.Add(row);
                        }
                    }

                    #endregion

                    List<AccountDistributionRowDTO> rowDtos = GetAccountDistributionRows(entities, headDTO.AccountDistributionHeadId);
                    result = CreateAccountDistributionEntryRows(entities, entry, rowDtos, headDTO.CalculationType, headDTO.PeriodType, actorCompanyId, matchedRowDTOs);

                }
                #endregion
            }

            return result;
        }

        private ActionResult CreateAccountDistributionEntryRows(CompEntities entities, AccountDistributionEntry entry, List<AccountDistributionRowDTO> rowDtos, TermGroup_AccountDistributionCalculationType calculationType, TermGroup_AccountDistributionPeriodType periodType, int actorCompanyId, List<AccountingRowDTO> matchedRowDTOs = null)
        {
            List<AccountDistributionEntryRow> childEntryRows = new List<AccountDistributionEntryRow>();

            if (periodType == TermGroup_AccountDistributionPeriodType.Amount)
            {
                foreach (AccountDistributionRowDTO dtoRow in rowDtos.Where(x => x.Dim1Id.HasValue))
                {
                    AccountDistributionEntryRow entryRow = new AccountDistributionEntryRow()
                    {
                        DebitAmount = dtoRow.SameBalance,
                        CreditAmount = dtoRow.OppositeBalance,
                        AccountStd = AccountManager.GetAccountStd(entities, dtoRow.Dim1Id.Value, actorCompanyId, true, false)
                    };

                    #region AccountInternals                                
                    AddAccountInternal(entities, entryRow, dtoRow.Dim2Id, actorCompanyId);
                    AddAccountInternal(entities, entryRow, dtoRow.Dim3Id, actorCompanyId);
                    AddAccountInternal(entities, entryRow, dtoRow.Dim4Id, actorCompanyId);
                    AddAccountInternal(entities, entryRow, dtoRow.Dim5Id, actorCompanyId);
                    AddAccountInternal(entities, entryRow, dtoRow.Dim6Id, actorCompanyId);
                    #endregion

                    childEntryRows.Add(entryRow);
                }
            }
            else if (periodType == TermGroup_AccountDistributionPeriodType.Period || periodType == TermGroup_AccountDistributionPeriodType.Year)
            {
                if (matchedRowDTOs == null)
                    return new ActionResult((int)ActionResultSave.InsufficientInput);

                decimal sum = 0, diff = 0;
                bool dim2Grouped = false, dim3Grouped = false, dim4Grouped = false, dim5Grouped = false, dim6Grouped = false;
                int lastItemCounter = 0, rowDtosCount = rowDtos.Count;
                List<IGrouping<int, AccountingRowDTO>> groupedRows;

                foreach (AccountDistributionRowDTO dtoRow in rowDtos.Where(i => i.CalculateRowNbr == 0))
                {
                    lastItemCounter++;

                    if (dtoRow.Dim2KeepSourceRowAccount)
                    {
                        groupedRows = matchedRowDTOs.GroupBy(i => i.Dim2Id).ToList();
                        dim2Grouped = true;
                    }
                    else if (dtoRow.Dim3KeepSourceRowAccount)
                    {
                        groupedRows = matchedRowDTOs.GroupBy(i => i.Dim3Id).ToList();
                        dim3Grouped = true;
                    }
                    else if (dtoRow.Dim4KeepSourceRowAccount)
                    {
                        groupedRows = matchedRowDTOs.GroupBy(i => i.Dim4Id).ToList();
                        dim4Grouped = true;
                    }
                    else if (dtoRow.Dim5KeepSourceRowAccount)
                    {
                        groupedRows = matchedRowDTOs.GroupBy(i => i.Dim5Id).ToList();
                        dim5Grouped = true;
                    }
                    else if (dtoRow.Dim6KeepSourceRowAccount)
                    {
                        groupedRows = matchedRowDTOs.GroupBy(i => i.Dim6Id).ToList();
                        dim6Grouped = true;
                    }
                    else
                        groupedRows = matchedRowDTOs.GroupBy(i => i.Dim1Id).ToList();

                    foreach (IGrouping<int, AccountingRowDTO> groupedRow in groupedRows)
                    {
                        sum = groupedRow.Sum(a => a.Amount);

                        #region AccountDistributionEntryRow

                        decimal creditAmount = 0;
                        decimal debitAmount = 0;

                        if (calculationType == TermGroup_AccountDistributionCalculationType.Percent)
                        {
                            if (sum > 0)
                            {
                                creditAmount = Decimal.Round((dtoRow.OppositeBalance / 100) * sum, 2);
                                debitAmount = Decimal.Round((dtoRow.SameBalance / 100) * sum, 2);
                            }
                            else
                            {
                                creditAmount = Decimal.Round((dtoRow.SameBalance / 100) * (sum * -1), 2);
                                debitAmount = Decimal.Round((dtoRow.OppositeBalance / 100) * (sum * -1), 2);
                            }
                        }
                        else if (calculationType == TermGroup_AccountDistributionCalculationType.Amount)
                        {
                            debitAmount = dtoRow.SameBalance;
                            creditAmount = dtoRow.OppositeBalance;
                        }

                        AccountDistributionEntryRow entryRow = new AccountDistributionEntryRow()
                        {
                            CreditAmount = creditAmount,
                            DebitAmount = debitAmount,

                            //Set references
                            AccountStd = dtoRow.Dim1Id.HasValue ? AccountManager.GetAccountStd(entities, (int)dtoRow.Dim1Id, actorCompanyId, true, false) : AccountManager.GetAccountStd(entities, groupedRow.Key, actorCompanyId, true, false),
                        };

                        diff += debitAmount - creditAmount;

                        #region AccountInternals                                

                        AddAccountInternal(entities, entryRow, dim2Grouped == true ? groupedRow.Key : dtoRow.Dim2Id, actorCompanyId);
                        AddAccountInternal(entities, entryRow, dim3Grouped == true ? groupedRow.Key : dtoRow.Dim3Id, actorCompanyId);
                        AddAccountInternal(entities, entryRow, dim4Grouped == true ? groupedRow.Key : dtoRow.Dim4Id, actorCompanyId);
                        AddAccountInternal(entities, entryRow, dim5Grouped == true ? groupedRow.Key : dtoRow.Dim5Id, actorCompanyId);
                        AddAccountInternal(entities, entryRow, dim6Grouped == true ? groupedRow.Key : dtoRow.Dim6Id, actorCompanyId);

                        #endregion

                        //adjust difference to last entry row
                        if (groupedRows.IndexOf(groupedRow) == groupedRows.Count - 1 &&
                            lastItemCounter == rowDtosCount && diff != 0)
                        {
                            if (entryRow.DebitAmount != 0)
                                entryRow.DebitAmount -= diff;  //diff > 0 = debet too big so make it smaller or else if minus, credit is bigger så by minus + minus = add
                            else if (entryRow.CreditAmount != 0)
                                entryRow.CreditAmount += diff; //diff < 0 = credit too big so make it smaller by using diff or else if plus, debet is larger so just add diff
                        }

                        //Set currency amounts, after any adjustments
                        CountryCurrencyManager.SetCurrencyAmounts(entities, actorCompanyId, entryRow);

                        entry.AccountDistributionEntryRow.Add(entryRow);

                        #region childRows
                        //handle entry rows based on current entry row
                        List<AccountDistributionRowDTO> childRows = rowDtos.Where(i => i.CalculateRowNbr == dtoRow.RowNbr).ToList();

                        foreach (var childRow in childRows)
                        {
                            lastItemCounter++;
                            sum = entryRow.DebitAmount - entryRow.CreditAmount;

                            if (calculationType == TermGroup_AccountDistributionCalculationType.Percent)
                            {
                                if (sum > 0)
                                {
                                    creditAmount = Decimal.Round((childRow.OppositeBalance / 100) * sum, 2);
                                    debitAmount = Decimal.Round((childRow.SameBalance / 100) * sum, 2);
                                }
                                else
                                {
                                    creditAmount = Decimal.Round((childRow.SameBalance / 100) * (sum * -1), 2);
                                    debitAmount = Decimal.Round((childRow.OppositeBalance / 100) * (sum * -1), 2);
                                }
                            }
                            else if (calculationType == TermGroup_AccountDistributionCalculationType.Amount)
                            {
                                debitAmount = childRow.SameBalance;
                                creditAmount = childRow.OppositeBalance;
                            }

                            AccountDistributionEntryRow childEntryRow = new AccountDistributionEntryRow()
                            {
                                CreditAmount = creditAmount,
                                DebitAmount = debitAmount,

                                //Set references
                                AccountStd = AccountManager.GetAccountStd(entities, (int)childRow.Dim1Id, actorCompanyId, true, false),
                            };

                            diff += debitAmount - creditAmount;

                            //Set currency amounts
                            CountryCurrencyManager.SetCurrencyAmounts(entities, actorCompanyId, childEntryRow);

                            #region AccountInternals                                

                            AddAccountInternal(entities, childEntryRow, dim2Grouped ? groupedRow.Key : childRow.Dim2Id, actorCompanyId);
                            AddAccountInternal(entities, childEntryRow, dim3Grouped ? groupedRow.Key : childRow.Dim3Id, actorCompanyId);
                            AddAccountInternal(entities, childEntryRow, dim4Grouped ? groupedRow.Key : childRow.Dim4Id, actorCompanyId);
                            AddAccountInternal(entities, childEntryRow, dim5Grouped ? groupedRow.Key : childRow.Dim5Id, actorCompanyId);
                            AddAccountInternal(entities, childEntryRow, dim6Grouped ? groupedRow.Key : childRow.Dim6Id, actorCompanyId);

                            #endregion

                            //adjust difference to last entry row
                            if (groupedRows.IndexOf(groupedRow) == groupedRows.Count - 1 &&
                                lastItemCounter == rowDtosCount && diff != 0)
                            {
                                if (childEntryRow.DebitAmount != 0)
                                    childEntryRow.DebitAmount += diff > 0 ? Decimal.Negate(diff) : diff;
                                else if (childEntryRow.CreditAmount != 0)
                                    childEntryRow.CreditAmount += diff < 0 ? Decimal.Negate(diff) : diff;
                            }

                            childEntryRows.Add(childEntryRow);
                        }

                        #endregion

                        #endregion
                    }
                }
            }

            foreach (var childRow in childEntryRows)
            {
                entry.AccountDistributionEntryRow.Add(childRow);
            }

            return SaveAccountDistributionEntry(entities, entry, actorCompanyId);
        }

        private List<AccountingRowDTO> MatchRowDTOs(AccountDistributionHeadDTO headDTO, List<AccountingRowDTO> rowDTOs, List<AccountDim> accountDims)
        {
            List<AccountingRowDTO> matchedRowDTOs = rowDTOs;

            #region Match

            // Dim 1
            if (accountDims.Any(a => a.AccountDimNr == 1))
            {
                Regex regEx = new Regex(StringUtility.WildCardToRegEx(headDTO.Dim1Expression));
                matchedRowDTOs = matchedRowDTOs.Where(a => regEx.IsMatch(a.Dim1Nr)).ToList();
                if (matchedRowDTOs.Count == 0)
                    return null;
            }

            // Dim 2
            if (accountDims.Count > 1)
            //if (accountDimNrs.Any(a => a == 2))
            {
                Regex regEx = new Regex(StringUtility.WildCardToRegEx(headDTO.Dim2Expression));
                matchedRowDTOs = matchedRowDTOs.Where(a => regEx.IsMatch(a.Dim2Nr)).ToList();
                if (matchedRowDTOs.Count == 0)
                    return null;
            }

            // Dim 3
            if (accountDims.Count > 2)
            //if (accountDimNrs.Any(a => a == 3))
            {
                Regex regEx = new Regex(StringUtility.WildCardToRegEx(headDTO.Dim3Expression));
                matchedRowDTOs = matchedRowDTOs.Where(a => regEx.IsMatch(a.Dim3Nr)).ToList();
                if (matchedRowDTOs.Count == 0)
                    return null;
            }

            // Dim 4
            if (accountDims.Count > 3)
            //if (accountDimNrs.Any(a => a == 4))
            {
                Regex regEx = new Regex(StringUtility.WildCardToRegEx(headDTO.Dim4Expression));
                matchedRowDTOs = matchedRowDTOs.Where(a => regEx.IsMatch(a.Dim4Nr)).ToList();
                if (matchedRowDTOs.Count == 0)
                    return null;
            }

            // Dim 5
            if (accountDims.Count > 4)
            //if (accountDimNrs.Any(a => a == 5))
            {
                Regex regEx = new Regex(StringUtility.WildCardToRegEx(headDTO.Dim5Expression));
                matchedRowDTOs = matchedRowDTOs.Where(a => regEx.IsMatch(a.Dim5Nr)).ToList();
                if (matchedRowDTOs.Count == 0)
                    return null;
            }

            // Dim 6
            if (accountDims.Count > 5)
            //if (accountDimNrs.Any(a => a == 6))
            {
                Regex regEx = new Regex(StringUtility.WildCardToRegEx(headDTO.Dim6Expression));
                matchedRowDTOs = matchedRowDTOs.Where(a => regEx.IsMatch(a.Dim6Nr)).ToList();
                if (matchedRowDTOs.Count == 0)
                    return null;
            }

            #endregion

            return matchedRowDTOs;
        }

        private void AddVoucherToDTO(List<VoucherRow> voucherRows, List<AccountingRowDTO> rowDTOs, List<AccountDim> accountDims)
        {
            // Only show active rows
            foreach (VoucherRow voucherRow in voucherRows.Where(x => x.State == (int)SoeEntityState.Active))
            {

                #region AccountingRowDTO

                AccountingRowDTO rowItem = new AccountingRowDTO()
                {
                    InvoiceRowId = voucherRow.VoucherRowId,
                    Date = voucherRow.Date,
                    Text = voucherRow.Text,
                    Quantity = voucherRow.Quantity,
                    Amount = voucherRow.Amount,
                    CreditAmount = voucherRow.Amount < 0 ? Math.Abs(voucherRow.Amount) : 0,
                    DebitAmount = voucherRow.Amount > 0 ? voucherRow.Amount : 0,
                    IsCreditRow = voucherRow.Amount < 0,
                    IsDebitRow = voucherRow.Amount > 0,
                    State = (SoeEntityState)voucherRow.State
                };

                #endregion

                #region AccountDim

                //För filtreringens skull så det inte är null
                rowItem.Dim1Nr = String.Empty;
                rowItem.Dim2Nr = String.Empty;
                rowItem.Dim3Nr = String.Empty;
                rowItem.Dim4Nr = String.Empty;
                rowItem.Dim5Nr = String.Empty;
                rowItem.Dim6Nr = String.Empty;

                // Get standard account
                AccountStd accountStd = voucherRow.AccountStd;
                if (accountStd != null)
                {
                    rowItem.Dim1Id = accountStd.AccountId;
                    rowItem.Dim1Nr = accountStd.Account.AccountNr;
                    rowItem.Dim1Name = accountStd.Account.Name;
                }

                // Internal accounts (dim 2-6)

                foreach (AccountInternal accountInternal in voucherRow.AccountInternal)
                {
                    var pos = accountDims.FindIndex(x => x.AccountDimId == accountInternal.Account.AccountDim.AccountDimId) + 1;

                    switch (pos)
                    {
                        case 2:
                            rowItem.Dim2Id = accountInternal.AccountId;
                            rowItem.Dim2Nr = accountInternal.Account.AccountNr;
                            rowItem.Dim2Name = accountInternal.Account.Name;
                            break;
                        case 3:
                            rowItem.Dim3Id = accountInternal.AccountId;
                            rowItem.Dim3Nr = accountInternal.Account.AccountNr;
                            rowItem.Dim3Name = accountInternal.Account.Name;
                            break;
                        case 4:
                            rowItem.Dim4Id = accountInternal.AccountId;
                            rowItem.Dim4Nr = accountInternal.Account.AccountNr;
                            rowItem.Dim4Name = accountInternal.Account.Name;
                            break;
                        case 5:
                            rowItem.Dim5Id = accountInternal.AccountId;
                            rowItem.Dim5Nr = accountInternal.Account.AccountNr;
                            rowItem.Dim5Name = accountInternal.Account.Name;
                            break;
                        case 6:
                            rowItem.Dim6Id = accountInternal.AccountId;
                            rowItem.Dim6Nr = accountInternal.Account.AccountNr;
                            rowItem.Dim6Name = accountInternal.Account.Name;
                            break;
                    }
                }


                /*
                foreach (AccountInternal accountInternal in voucherRow.AccountInternal)
                {
                    switch (accountInternal.Account.AccountDim.AccountDimNr)
                    {
                        case 2:
                            rowItem.Dim2Id = accountInternal.AccountId;
                            rowItem.Dim2Nr = accountInternal.Account.AccountNr;
                            rowItem.Dim2Name = accountInternal.Account.Name;
                            break;
                        case 3:
                            rowItem.Dim3Id = accountInternal.AccountId;
                            rowItem.Dim3Nr = accountInternal.Account.AccountNr;
                            rowItem.Dim3Name = accountInternal.Account.Name;
                            break;
                        case 4:
                            rowItem.Dim4Id = accountInternal.AccountId;
                            rowItem.Dim4Nr = accountInternal.Account.AccountNr;
                            rowItem.Dim4Name = accountInternal.Account.Name;
                            break;
                        case 5:
                            rowItem.Dim5Id = accountInternal.AccountId;
                            rowItem.Dim5Nr = accountInternal.Account.AccountNr;
                            rowItem.Dim5Name = accountInternal.Account.Name;
                            break;
                        case 6:
                            rowItem.Dim6Id = accountInternal.AccountId;
                            rowItem.Dim6Nr = accountInternal.Account.AccountNr;
                            rowItem.Dim6Name = accountInternal.Account.Name;
                            break;
                    }
                }
                */
                #endregion

                // Add row to account DataGrid
                rowDTOs.Add(rowItem);
            }
        }

        private void AddAccountBalanceToDTO(List<AccountYearBalanceHead> balanceHeads, List<AccountingRowDTO> rowDTOs)
        {
            foreach (AccountYearBalanceHead balanceHead in balanceHeads)
            {


                #region AccountingRowDTO

                AccountingRowDTO rowItem = new AccountingRowDTO()
                {
                    InvoiceRowId = 0,
                    Date = DateTime.Today,
                    Text = String.Empty,
                    Quantity = balanceHead.Quantity,
                    Amount = balanceHead.Balance,
                    CreditAmount = balanceHead.Balance < 0 ? Math.Abs(balanceHead.Balance) : 0,
                    DebitAmount = balanceHead.Balance > 0 ? balanceHead.Balance : 0,
                    IsCreditRow = balanceHead.Balance < 0,
                    IsDebitRow = balanceHead.Balance > 0,
                    State = SoeEntityState.Active
                };

                #endregion

                #region AccountDim

                //För filtreringens skull så det inte är null
                rowItem.Dim1Nr = String.Empty;
                rowItem.Dim2Nr = String.Empty;
                rowItem.Dim3Nr = String.Empty;
                rowItem.Dim4Nr = String.Empty;
                rowItem.Dim5Nr = String.Empty;
                rowItem.Dim6Nr = String.Empty;

                // Get standard account
                AccountStd accountStd = balanceHead.AccountStd;
                if (accountStd != null)
                {
                    rowItem.Dim1Id = accountStd.AccountId;
                    rowItem.Dim1Nr = accountStd.Account.AccountNr;
                    rowItem.Dim1Name = accountStd.Account.Name;
                }

                // Internal accounts (dim 2-6)
                foreach (AccountInternal accountInternal in balanceHead.AccountInternal)
                {
                    switch (accountInternal.Account.AccountDim.AccountDimNr)
                    {
                        case 2:
                            rowItem.Dim2Id = accountInternal.AccountId;
                            rowItem.Dim2Nr = accountInternal.Account.AccountNr;
                            rowItem.Dim2Name = accountInternal.Account.Name;
                            break;
                        case 3:
                            rowItem.Dim3Id = accountInternal.AccountId;
                            rowItem.Dim3Nr = accountInternal.Account.AccountNr;
                            rowItem.Dim3Name = accountInternal.Account.Name;
                            break;
                        case 4:
                            rowItem.Dim4Id = accountInternal.AccountId;
                            rowItem.Dim4Nr = accountInternal.Account.AccountNr;
                            rowItem.Dim4Name = accountInternal.Account.Name;
                            break;
                        case 5:
                            rowItem.Dim5Id = accountInternal.AccountId;
                            rowItem.Dim5Nr = accountInternal.Account.AccountNr;
                            rowItem.Dim5Name = accountInternal.Account.Name;
                            break;
                        case 6:
                            rowItem.Dim6Id = accountInternal.AccountId;
                            rowItem.Dim6Nr = accountInternal.Account.AccountNr;
                            rowItem.Dim6Name = accountInternal.Account.Name;
                            break;
                    }
                }

                #endregion

                // Add row to account DataGrid
                rowDTOs.Add(rowItem);
            }
        }

        public ActionResult SaveAccountDistributionEntry(CompEntities entities, AccountDistributionEntry accountDistributionEntry, int actorCompanyId)
        {
            ActionResult result = new ActionResult(true);

            try
            {
                if (accountDistributionEntry.AccountDistributionEntryId == 0)
                {
                    if (accountDistributionEntry.Company == null)
                        accountDistributionEntry.Company = CompanyManager.GetCompany(entities, actorCompanyId);

                    SetCreatedProperties(accountDistributionEntry);
                    entities.AccountDistributionEntry.AddObject(accountDistributionEntry);

                    if (entities.SaveChanges() == 0)
                    {
                        result.Success = false;
                        result.ErrorNumber = (int)ActionResultSave.NothingSaved;
                    }
                }
                else
                {
                    var distributionEntry = GetAccountDistributionEntry(entities, actorCompanyId, accountDistributionEntry.AccountDistributionEntryId);
                    result = UpdateEntityItem(entities, distributionEntry, accountDistributionEntry, "AccountDistributionEntry");
                }
            }
            catch (Exception ex)
            {
                base.LogError(ex, this.log);
                result.Exception = ex;
            }

            return result;
        }

        public ActionResult CreateAccountDistributionEntries(CompEntities entities, List<AccountDistributionEntry> accountDistributionEntries, int actorCompanyId)
        {
            ActionResult result = new ActionResult(true);

            try
            {
                foreach (var entry in accountDistributionEntries)
                {
                    // Only save new entries.
                    if (entry.AccountDistributionEntryId != 0)
                        continue;

                    if (entry.Company is null)
                        throw new UnauthorizedAccessException("User not allowed to CreateAccountDistributionEntries");

                    SetCreatedProperties(entry);
                    entities.AccountDistributionEntry.AddObject(entry);
                }

                result = SaveChanges(entities, useBulkSaveChanges: true);
            }
            catch (Exception ex)
            {
                base.LogError(ex, this.log);
                result = new ActionResult(ex);
            }

            return result;
        }

        public ActionResult AddAccountDistributionEntry(CompEntities entities, AccountDistributionEntry entryInput, List<AccountingRowDTO> accountRowItemsInput)
        {
            ActionResult result = new ActionResult(true);

            if (entryInput == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "AccountDistributionEntry");

            #region Prereq

            // Get company
            Company company = CompanyManager.GetCompany(entities, entryInput.ActorCompanyId);
            if (company == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte"));

            #endregion

            #region AccountDistributionEntry

            AccountDistributionEntry entry = new AccountDistributionEntry()
            {
                TriggerType = entryInput.TriggerType,
                Date = entryInput.Date,

                //Set references
                Company = company,

                //Set FK
                InventoryId = entryInput.InventoryId,
                SupplierInvoiceId = entryInput.SupplierInvoiceId,
                AccountDistributionHeadId = entryInput.AccountDistributionHeadId,
                VoucherHeadId = entryInput.VoucherHeadId,
            };
            SetCreatedProperties(entry);
            entities.AccountDistributionEntry.AddObject(entry);

            base.TryDetachEntity(entities, entryInput);

            #endregion

            #region AccountDistributionEntryRow

            if (accountRowItemsInput != null && accountRowItemsInput.Count > 0)
            {
                foreach (var item in accountRowItemsInput.Where(i => i.Dim1Id != 0))
                {
                    #region AccountDistributionEntryRow

                    AccountDistributionEntryRow entryRow = new AccountDistributionEntryRow()
                    {
                        DebitAmount = item.DebitAmount,
                        CreditAmount = item.CreditAmount,

                        //Set references
                        AccountStd = AccountManager.GetAccountStd(entities, item.Dim1Id, entryInput.ActorCompanyId, false, false),
                    };

                    //Set currency amounts
                    CountryCurrencyManager.SetCurrencyAmounts(entities, entryInput.ActorCompanyId, entryRow);

                    #region AccountInternals

                    AddAccountInternal(entities, entryRow, item.Dim2Id, entryInput.ActorCompanyId);
                    AddAccountInternal(entities, entryRow, item.Dim3Id, entryInput.ActorCompanyId);
                    AddAccountInternal(entities, entryRow, item.Dim4Id, entryInput.ActorCompanyId);
                    AddAccountInternal(entities, entryRow, item.Dim5Id, entryInput.ActorCompanyId);
                    AddAccountInternal(entities, entryRow, item.Dim6Id, entryInput.ActorCompanyId);

                    #endregion

                    entry.AccountDistributionEntryRow.Add(entryRow);

                    #endregion
                }
            }

            #endregion

            result.IntegerValue = entry.AccountDistributionEntryId;

            return result;
        }

        public ActionResult DeleteAccountDistributionEntry(CompEntities entities, AccountDistributionEntry accountDistributionEntry, bool saveChanges = true)
        {
            return ChangeEntityState(entities, accountDistributionEntry, SoeEntityState.Deleted, saveChanges);
        }

        public ActionResult DeleteAccountDistributionEntryPermanently(CompEntities entities, AccountDistributionEntry accountDistributionEntry)
        {
            foreach (AccountDistributionEntryRow entryRow in accountDistributionEntry.AccountDistributionEntryRow.ToList())
            {
                entryRow.AccountInternal.Clear();
                entities.DeleteObject(entryRow);
            }

            foreach (InventoryLog inventoryLog in accountDistributionEntry.InventoryLog.ToList())
            {
                entities.DeleteObject(inventoryLog);
            }

            accountDistributionEntry.AccountDistributionEntryRow.Clear();

            entities.DeleteObject(accountDistributionEntry);

            return SaveDeletions(entities);
        }

        public ActionResult RestoreAccountDistributionEntry(CompEntities entities, AccountDistributionEntry accountDistributionEntry)
        {
            return ChangeEntityState(entities, accountDistributionEntry, SoeEntityState.Active, true);
        }

        public List<AccountDistributionEntry> CreateAccountDistributionEntriesFromAccountingRowDTO(CompEntities entities, AccountingRowDTO rowItem, int sourceRowId, int actorCompanyId, int registrationType, int sourceId)
        {
            List<AccountDistributionEntry> entries = new List<AccountDistributionEntry>();

            #region Prereq

            if (rowItem == null)
                return null;

            // Get company
            Company company = CompanyManager.GetCompany(entities, actorCompanyId);
            if (company == null)
                return null;

            // Get account distribution
            AccountDistributionHead accountDistributionHead = GetAccountDistributionHead(entities, rowItem.AccountDistributionHeadId);
            if (accountDistributionHead == null || accountDistributionHead.AccountDistributionRow.IsNullOrEmpty())
                return null;

            #endregion

            DateTime startDate = rowItem.AccountDistributionStartDate.HasValue ? rowItem.AccountDistributionStartDate.Value : DateTime.Today;
            int nbrOfPeriods = rowItem.AccountDistributionNbrOfPeriods;

            IEnumerable<AccountDistributionRowDTO> rows = GetAccountDistributionRows(entities, accountDistributionHead.AccountDistributionHeadId);

            // Total amount over all periods
            decimal totalAmount = Math.Abs(rowItem.DebitAmount - rowItem.CreditAmount);
            // Amount for each period (last period may differ)
            decimal periodAmount = Decimal.Round(Decimal.Divide(totalAmount, nbrOfPeriods), 2);
            // Keep track of total distributed amount (used to get remaining on last period)
            decimal totalDistributedAmount = 0;

            for (int i = 1; i <= nbrOfPeriods; i++)
            {
                #region AccountDistributionEntry

                if (i > 1)
                {
                    // Get next period date
                    startDate = startDate.AddMonths(1);
                    int daysInCurrentMonth = DateTime.DaysInMonth(startDate.Year, startDate.Month);
                    int dayNumber = accountDistributionHead.DayNumber > 0 ? accountDistributionHead.DayNumber : daysInCurrentMonth;
                    if (dayNumber > daysInCurrentMonth)
                        dayNumber = daysInCurrentMonth;
                    if (startDate.Day != dayNumber)
                    {
                        while (startDate.Day != dayNumber)
                        {
                            startDate = startDate.AddDays(1);
                        }
                    }
                }

                AccountDistributionEntry entry = new AccountDistributionEntry()
                {
                    TriggerType = accountDistributionHead.TriggerType,
                    Date = startDate,
                    RegistrationType = registrationType,
                    SourceCustomerInvoiceId = registrationType == (int)TermGroup_AccountDistributionRegistrationType.CustomerInvoice ? sourceId : 0,
                    SourceSupplierInvoiceId = registrationType == (int)TermGroup_AccountDistributionRegistrationType.SupplierInvoice ? sourceId : 0,
                    SourceVoucherHeadId = registrationType == (int)TermGroup_AccountDistributionRegistrationType.Voucher ? sourceId : 0,
                    SourceRowId = sourceRowId,

                    //Set references
                    AccountDistributionHead = accountDistributionHead,
                    Company = company,
                };

                entities.AccountDistributionEntry.AddObject(entry);

                #endregion

                AccountDistributionEntryRow entryRow;
                decimal creditAmount = 0;
                decimal debitAmount = 0;

                // The last period will get the remaining if total amount is not equaly divided by number of periods
                if (i == nbrOfPeriods)
                    periodAmount = totalAmount - (periodAmount * (nbrOfPeriods - 1));

                // Distributed amount for current period (used to balance each period)
                decimal distributedCreditAmount = 0;
                decimal distributedDebitAmount = 0;
                int rowCounter = 0;
                foreach (AccountDistributionRowDTO row in rows)
                {
                    rowCounter++;

                    #region AccountDistributionEntryRow

                    //get account from rowitem if account distribution row has no account (keep account)
                    if (!row.Dim1Id.HasValue)
                        row.Dim1Id = rowItem.Dim1Id;

                    // Get standard account
                    AccountStd accountStd = AccountManager.GetAccountStd(entities, (int)row.Dim1Id, actorCompanyId, false, false);
                    if (accountStd == null)
                        return null;

                    // Calculate amounts
                    if (accountDistributionHead.CalculationType == (int)TermGroup_AccountDistributionCalculationType.Amount)
                    {
                        if (row.SameBalance != 0)
                        {
                            creditAmount = rowItem.CreditAmount != 0 ? row.SameBalance : 0;
                            debitAmount = rowItem.DebitAmount != 0 ? row.SameBalance : 0;
                        }
                        else
                        {
                            creditAmount = rowItem.DebitAmount != 0 ? row.OppositeBalance : 0;
                            debitAmount = rowItem.CreditAmount != 0 ? row.OppositeBalance : 0;
                        }
                    }
                    else if (accountDistributionHead.CalculationType == (int)TermGroup_AccountDistributionCalculationType.Percent)
                    {
                        if (row.SameBalance != 0)
                        {
                            creditAmount = rowItem.CreditAmount != 0 ? Decimal.Multiply(periodAmount, Decimal.Divide(row.SameBalance, 100m)) : 0;
                            debitAmount = rowItem.DebitAmount != 0 ? Decimal.Multiply(periodAmount, Decimal.Divide(row.SameBalance, 100m)) : 0;
                        }
                        else
                        {
                            creditAmount = rowItem.DebitAmount != 0 ? Decimal.Multiply(periodAmount, Decimal.Divide(row.OppositeBalance, 100m)) : 0;
                            debitAmount = rowItem.CreditAmount != 0 ? Decimal.Multiply(periodAmount, Decimal.Divide(row.OppositeBalance, 100m)) : 0;
                        }
                    }


                    creditAmount = Decimal.Round(creditAmount, 2);
                    debitAmount = Decimal.Round(debitAmount, 2);

                    // Last row must balance the voucher
                    if (rowCounter == rows.Count())
                    {
                        decimal leftToDistribute = distributedDebitAmount - distributedCreditAmount;
                        if (leftToDistribute > 0)
                            creditAmount = leftToDistribute;
                        else
                            debitAmount = leftToDistribute;
                    }

                    creditAmount = Math.Abs(creditAmount);
                    debitAmount = Math.Abs(debitAmount);

                    distributedCreditAmount += creditAmount;
                    distributedDebitAmount += debitAmount;
                    totalDistributedAmount += creditAmount + debitAmount;

                    // Create AccountDistributionEntryRow
                    entryRow = new AccountDistributionEntryRow()
                    {
                        CreditAmount = creditAmount,
                        DebitAmount = debitAmount,

                        //Set references
                        AccountStd = accountStd,
                    };
                    entry.AccountDistributionEntryRow.Add(entryRow);

                    //Set currency amounts
                    CountryCurrencyManager.SetCurrencyAmounts(entities, actorCompanyId, entryRow);

                    #endregion

                    #region AccountDistributionEntryRowMapping

                    AccountInternal accInt;
                    if (row.Dim2Id != 0)
                    {
                        accInt = AccountManager.GetAccountInternal(entities, row.Dim2Id, actorCompanyId);
                        if (accInt == null)
                            return null;
                        entryRow.AccountInternal.Add(accInt);
                    }
                    else if (row.Dim2KeepSourceRowAccount && rowItem.Dim2Id != 0)
                    {
                        accInt = AccountManager.GetAccountInternal(entities, (row.Dim2KeepSourceRowAccount ? rowItem.Dim2Id : row.Dim2Id), actorCompanyId);
                        if (accInt == null)
                            return null;
                        entryRow.AccountInternal.Add(accInt);
                    }
                    if (row.Dim3Id != 0)
                    {
                        accInt = AccountManager.GetAccountInternal(entities, (row.Dim3KeepSourceRowAccount ? rowItem.Dim3Id : row.Dim3Id), actorCompanyId);
                        if (accInt == null)
                            return null;
                        entryRow.AccountInternal.Add(accInt);
                    }
                    else if (row.Dim3KeepSourceRowAccount && rowItem.Dim3Id != 0)
                    {
                        accInt = AccountManager.GetAccountInternal(entities, rowItem.Dim3Id, actorCompanyId);
                        if (accInt == null)
                            return null;
                        entryRow.AccountInternal.Add(accInt);
                    }
                    if (row.Dim4Id != 0)
                    {
                        accInt = AccountManager.GetAccountInternal(entities, (row.Dim4KeepSourceRowAccount ? rowItem.Dim4Id : row.Dim4Id), actorCompanyId);
                        if (accInt == null)
                            return null;
                        entryRow.AccountInternal.Add(accInt);
                    }
                    else if (row.Dim4KeepSourceRowAccount && rowItem.Dim4Id != 0)
                    {
                        accInt = AccountManager.GetAccountInternal(entities, rowItem.Dim4Id, actorCompanyId);
                        if (accInt == null)
                            return null;
                        entryRow.AccountInternal.Add(accInt);
                    }
                    if (row.Dim5Id != 0)
                    {
                        accInt = AccountManager.GetAccountInternal(entities, (row.Dim5KeepSourceRowAccount ? rowItem.Dim5Id : row.Dim5Id), actorCompanyId);
                        if (accInt == null)
                            return null;
                        entryRow.AccountInternal.Add(accInt);
                    }
                    else if (row.Dim5KeepSourceRowAccount && rowItem.Dim5Id != 0)
                    {
                        accInt = AccountManager.GetAccountInternal(entities, rowItem.Dim5Id, actorCompanyId);
                        if (accInt == null)
                            return null;
                        entryRow.AccountInternal.Add(accInt);
                    }
                    if (row.Dim6Id != 0)
                    {
                        accInt = AccountManager.GetAccountInternal(entities, (row.Dim6KeepSourceRowAccount ? rowItem.Dim6Id : row.Dim6Id), actorCompanyId);
                        if (accInt == null)
                            return null;
                        entryRow.AccountInternal.Add(accInt);
                    }
                    else if (row.Dim6KeepSourceRowAccount && rowItem.Dim6Id != 0)
                    {
                        accInt = AccountManager.GetAccountInternal(entities, rowItem.Dim6Id, actorCompanyId);
                        if (accInt == null)
                            return null;
                        entryRow.AccountInternal.Add(accInt);
                    }

                    #endregion
                }

                entries.Add(entry);
            }

            return entries;
        }

        private void AddAccountInternal(CompEntities entities, AccountDistributionEntryRow entryRow, int accountId, int actorCompanyId)
        {
            if (accountId != 0)
            {
                AccountInternal accountInternal = AccountManager.GetAccountInternal(entities, accountId, actorCompanyId);
                if (accountInternal != null)
                    entryRow.AccountInternal.Add(accountInternal);
            }
        }

		public Dictionary<(int,int,int), AccrualSource> GetAccrualSourceDict(IEnumerable<AccountDistributionEntry> accountDistributionEntries)
		{
			var invoiceIds = accountDistributionEntries.Select(s => s.SourceSupplierInvoiceId).Where(id => id > 0).ToHashSet();
			invoiceIds.AddRange(accountDistributionEntries.Select(s => s.SourceCustomerInvoiceId).Where(id => id > 0).ToHashSet());
			var voucherHeadIds = accountDistributionEntries.Select(s => s.SourceVoucherHeadId).Where(id => id > 0).ToHashSet();

			using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
			var invoices = entitiesReadOnly.Invoice
				.Where(i => invoiceIds.Contains(i.InvoiceId))
				.Select(i => new { i.InvoiceId, i.VoucherDate, i.SeqNr })
				.ToDictionary(i => i.InvoiceId, i => new AccrualSource(i.VoucherDate, i.SeqNr));

			var voucherDates = entitiesReadOnly.VoucherHead
				.Where(v => voucherHeadIds.Contains(v.VoucherHeadId))
				.Select(v => new { v.VoucherHeadId, v.Date, v.VoucherNr })
				.ToDictionary(v => v.VoucherHeadId, v => new AccrualSource(v.Date, v.VoucherNr));

            var result = new Dictionary<(int,int,int), AccrualSource>();

			foreach (var ade in accountDistributionEntries)
            {
				AccrualSource accrualSource;
				bool supplierInvoiceMatch = false;
				bool customerInvoiceMatch = invoices.TryGetValue(ade.SourceCustomerInvoiceId ?? 0, out accrualSource);
				if (!customerInvoiceMatch)
					supplierInvoiceMatch = invoices.TryGetValue(ade.SourceSupplierInvoiceId ?? 0, out accrualSource);
				if (!supplierInvoiceMatch)
					voucherDates.TryGetValue(ade.SourceVoucherHeadId ?? 0, out accrualSource);

                if (accrualSource == null)
                    continue;

                var key = new AccrualSourceKey(ade.AccountDistributionHeadId ?? 0, ade.SourceCustomerInvoiceId, ade.SourceSupplierInvoiceId, ade.SourceVoucherHeadId)
                    .ToTuple();

				if (result.ContainsKey(key))
                    continue;
				
                result.Add(key, accrualSource);
            }

            return result;
		}

        #region Reverse entry
        public ActionResult ReverseInventoryAccountDistributionEntries(int actorCompanyId, SoeAccountDistributionType type, List<AccountDistributionEntryDTO> dtosToReverse)
        {
            if (type == SoeAccountDistributionType.Auto || type == SoeAccountDistributionType.Period)
                return new ActionResult((int)ActionResultSave.EntityNotFound, "Cannot reverse auto or period entries");

            var result = new ActionResult();
            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();
                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT)) {

                        var reversals = new List<AccountDistributionEntry>();
                        foreach (var entryId in dtosToReverse.Select(r => r.AccountDistributionEntryId))
                        {
                            var (actionResult, entry) = ReverseAccountDistributionEntry(entities, actorCompanyId, entryId);
                            if (!actionResult.Success) return actionResult;

                            reversals.Add(entry);
                        }

                        /* We need to persist the entries before we can transfer them to voucher.
                         * This is due to how the transfer to voucher method has been written, as it accepts
                         * DTOs and not the entity objects. 
                         */
                        result = SaveChanges(entities, transaction);
                        if (!result.Success)
                            return result;

                        result = AccountDistributionManager.CreateTransferAccountDistributionEntryToVoucherTransaction(
                            entities, 
                            reversals.ToDTOs(setRows: true), 
                            actorCompanyId, 
                            SoeAccountDistributionType.Inventory_WriteOff,
                            true);

                        if (!result.Success)
                            return result;

                        result = SaveChanges(entities, transaction);
                        if (!result.Success)
                            return result;

                        transaction.Complete();
                    }
                }
                catch (Exception ex)
                {
                    base.LogError(ex, this.log);
                    result.Exception = ex;
                    result.Value = 0;
                }
                finally
                {
                    if (!result.Success)
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }
            }
            return result;
        }

        private (ActionResult, AccountDistributionEntry) ReverseAccountDistributionEntry(CompEntities entities, int actorCompanyId, int accountDistributionEntryId)
        {
            var entry = GetAccountDistributionEntry(entities, actorCompanyId, accountDistributionEntryId);
            var canBeReversed = ValidateAccountDistributionCanBeReversed(entities, actorCompanyId, entry);

            if (!canBeReversed.Success)
                return (canBeReversed, null);

            var originalInventoryLog = entry.InventoryLog.FirstOrDefault();
            var inventory = entry.Inventory;

            ReverseApplyDistributionEntryOnInventory(entry, inventory, originalInventoryLog);
            var reversal = CreateDistributionEntryReversal(entities, entry);
            return (new ActionResult(), reversal);
        }

        private ActionResult ValidateAccountDistributionCanBeReversed(CompEntities entities, int actorCompanyId, AccountDistributionEntry entry)
        {
            if (entry.InventoryId is null || entry.Inventory is null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, "Inventory");

            if (entry.InventoryLog.IsNullOrEmpty())
                return new ActionResult((int)ActionResultSave.EntityNotFound, "InventoryLog");

            var log = entry.InventoryLog.FirstOrDefault();
            switch ((TermGroup_InventoryLogType)log.Type)
            {
                case TermGroup_InventoryLogType.Reversal:
                case TermGroup_InventoryLogType.Reversed:
                case TermGroup_InventoryLogType.Sold:
                case TermGroup_InventoryLogType.Discarded:
                    return new ActionResult((int)ActionResultSave.NotSupported, GetText(6026, "Avskrivningen har felaktig typ för att motbokas"));
            }


            if (entry.VoucherHeadId == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(6021, "Kan inte motboka en avskrivning utan verifikat.")); 

            var hasFutureUnreversed = entities.AccountDistributionEntry
                .Where(e => e.ActorCompanyId == actorCompanyId && e.InventoryId == entry.InventoryId)
                .Any(e => e.ActorCompanyId == actorCompanyId &&
                            e.Date > entry.Date &&
                            e.VoucherHeadId != null &&
                            e.InventoryLog.Any(l => l.Type != (int)TermGroup_InventoryLogType.Reversal &&
                                                    l.Type != (int)TermGroup_InventoryLogType.Reversed)
                    );

            if (hasFutureUnreversed)
                return new ActionResult((int)ActionResultSave.NothingSaved, GetText(6025, "Inventarien har framtida avskrivningar som inte är motbokade."));

            return new ActionResult();
        }

        private AccountDistributionEntry CreateDistributionEntryReversal(CompEntities entities, AccountDistributionEntry entry)
        {
            var copy = entry.Copy();
            SetCreatedProperties(copy);
            copy.ReverseAccounting();

            var originalLog = entry.InventoryLog.FirstOrDefault();
            var inventoryLog = originalLog.Copy();
            inventoryLog.Date = DateTime.Now;
            inventoryLog.Type = (int)TermGroup_InventoryLogType.Reversal;
            inventoryLog.UserId = base.UserId;
            inventoryLog.AccountDistributionEntry = copy;
            inventoryLog.Reverse();

            copy.InventoryLog.Add(inventoryLog);
            entities.AccountDistributionEntry.AddObject(copy);

            return copy;
        }
        private void ReverseApplyDistributionEntryOnInventory(AccountDistributionEntry entry, Inventory inventory, InventoryLog inventoryLog)
        {
            ApplyDistributionEntryOnInventory(entry, inventory, inventoryLog, true);
            inventoryLog.Type = (int)TermGroup_InventoryLogType.Reversed;
        }
        private void ApplyDistributionEntryOnInventory(AccountDistributionEntry entry, Inventory inventory, InventoryLog inventoryLog, bool doReverse = false)
        {
            // I am not sure what I think about this. Should be closer to the truth to base it on the InventoryLog?
            // However, this is how it was done previosuly in the TransferToVoucher code.
            var accountDistributionEntryRow = entry.AccountDistributionEntryRow.FirstOrDefault(a => a.DebitAmount > 0);
            if (accountDistributionEntryRow == null) return;

            if (inventory.Status == (int)TermGroup_InventoryStatus.Sold || inventory.Status == (int)TermGroup_InventoryStatus.Discarded)
                throw new ActionFailedException(GetText(910, "Inventariet har fel status för avskrivning"));

            var type = (TermGroup_InventoryLogType)inventoryLog.Type;

            int periodChange = type == TermGroup_InventoryLogType.WriteOff ? 1 : 0;
            decimal amountChange = accountDistributionEntryRow.DebitAmount;
            switch (type)
            {
                // The inventory's remaining amount is in general decremented. However, you can increment it's value using special actions.
                case TermGroup_InventoryLogType.WriteUp:
                case TermGroup_InventoryLogType.UnderWriteOff:
                    amountChange = -amountChange;
                    break;
            }

            if (doReverse)
            {
                amountChange = -amountChange;
                periodChange = -periodChange;
            }

            inventory.WriteOffRemainingAmount -= amountChange;
            inventory.WriteOffPeriods += periodChange;

            // Not sure about adding this else statement.
            inventory.Status = inventory.WriteOffRemainingAmount == 0 ?
                (int)TermGroup_InventoryStatus.WrittenOff :
                (int)TermGroup_InventoryStatus.Active;
        }
        #endregion
        #endregion

        #region AccountDistributionTraceView

        public List<AccountDistributionTraceViewDTO> GetAccountDistributionTraceViews(int accountDistributionHeadId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.AccountDistributionTraceView.NoTracking();
            return GetAccountDistributionTraceViews(entities, accountDistributionHeadId);
        }

        public List<AccountDistributionTraceViewDTO> GetAccountDistributionTraceViews(CompEntities entities, int accountDistributionHeadId)
        {
            int langId = GetLangId();
            var originStatusTexts = base.GetTermGroupDict(TermGroup.OriginStatus, langId);
            var originTypeTexts = base.GetTermGroupDict(TermGroup.OriginType, langId);

            var items = (from v in entities.AccountDistributionTraceView
                         where v.AccountDistributionHeadId == accountDistributionHeadId
                         select v).ToDTOs().ToList();

            foreach (var item in items.Where(i => i.OriginType > 0))
            {
                item.OriginStatusName = item.OriginStatus != 0 ? originStatusTexts[(int)item.OriginStatus] : "";
                item.OriginTypeName = item.OriginType != 0 ? originTypeTexts[(int)item.OriginType] : "";
            }

            return items;
        }


        #endregion

    }
}

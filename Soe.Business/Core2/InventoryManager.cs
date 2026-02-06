using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Transactions;

namespace SoftOne.Soe.Business.Core
{
    public class InventoryManager : ManagerBase
    {
        #region Variables

        // Create a logger for use in this class
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #endregion

        #region Ctor

        public InventoryManager(ParameterObject parameterObject) : base(parameterObject) { }

        #endregion

        #region Inventory

        /// <summary>
        /// Get all inventories for specified company
        /// </summary>
        /// <param name="actorCompanyId">Company ID</param>
        /// <param name="onlyActive">If true, only active inventories are returned</param>
        /// <returns>A collection of inventories</returns>
        public List<Inventory> GetInventories(int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetInventories(entities, actorCompanyId);
        }
        public List<Inventory> GetInventories(CompEntities entities, int actorCompanyId)
        {
            entities.Inventory.NoTracking();

            var query = from i in entities.Inventory
                        where i.ActorCompanyId == actorCompanyId &&
                            i.State == (int)SoeEntityState.Active
                        select i;

            return query.OrderBy(i => i.InventoryNr).ToList();
        }

        public List<Inventory> GetInventoriesForAnalysis(int actorCompanyId, DateTime? startDate, DateTime? endDate, bool loadOnlyActive)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetInventoriesForAnalysis(entities, actorCompanyId, startDate, endDate, loadOnlyActive);
        }

        public List<Inventory> GetInventories(int actorCompanyId, bool loadInventoryAccount, int userId, string statuses, bool loadCategories, bool loadInventoryWriteOffMethod = false, bool loadOnlyActive = true, int? inventoryId = null)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetInventories(entities, actorCompanyId, loadInventoryAccount, userId, statuses, loadCategories, loadInventoryWriteOffMethod, loadOnlyActive, inventoryId);
        }

        public List<Inventory> GetInventoriesForAnalysis(CompEntities entities, int actorCompanyId, DateTime? startDate, DateTime? endDate, bool loadOnlyActive = true)
        {
            IQueryable<Inventory> query = (from i in entities.Inventory.Include("InventoryWriteOffMethod")
                                           where i.ActorCompanyId == actorCompanyId
                                           select i);
            if (startDate.HasValue)
                query = query.Where(i => i.PurchaseDate >= startDate.Value);
            if (endDate.HasValue)
                query = query.Where(i => i.PurchaseDate < endDate.Value);

            if (loadOnlyActive)
            {
                query = query.Where(m => m.State == (int)SoeEntityState.Active);
            }
            var inventories = query.OrderBy(i => i.InventoryNr).ToList();

            #region InventoryAccount
            int inventoryBaseAccountId = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.AccountInventoryInventories, 0, actorCompanyId, 0);
            AccountStd inventoryBaseAccount = (inventoryBaseAccountId != 0) ? AccountManager.GetAccountStd(entities, inventoryBaseAccountId, actorCompanyId, true, false) : null;

            entities.InventoryAccountStd.NoTracking();
            var inventoryAccounts = (from i in entities.InventoryAccountStd
                                     where i.Inventory.Company.ActorCompanyId == actorCompanyId &&
                                     i.InventoryId.HasValue &&
                                     i.Type == (int)InventoryAccountType.Inventory &&
                                     i.Inventory.State == (int)SoeEntityState.Active
                                     select new
                                     {
                                        i.InventoryId,
                                        AccountNr = i.AccountStd != null && i.AccountStd.Account != null
                                            ? i.AccountStd.Account.AccountNr
                                            : null,
                                        Name = i.AccountStd != null && i.AccountStd.Account != null
                                            ? i.AccountStd.Account.Name
                                            : null
                                     }).ToList();
            #endregion

            List<CompanyCategoryRecord> categoryRecordsForCompany = CategoryManager.GetCompanyCategoryRecords(entities, SoeCategoryType.Inventory, actorCompanyId);

            foreach (var inventory in inventories)
            {
                #region InventoryAccount
                var account = inventoryAccounts.FirstOrDefault(a => a.InventoryId.HasValue && a.InventoryId == inventory.InventoryId);

                if (account != null)
                {
                    inventory.InventoryAccountNr = account.AccountNr;
                    inventory.InventoryAccountName = account.Name;
                    inventoryAccounts.Remove(account);
                }
                else if (inventoryBaseAccount != null)
                {
                    inventory.InventoryAccountNr = inventoryBaseAccount.Account.AccountNr;
                    inventory.InventoryAccountName = inventoryBaseAccount.Account.Name;
                }
                #endregion
                
                #region InventoryStatus
                inventory.StatusName = GetText(inventory.Status, (int)TermGroup.InventoryStatus);
                #endregion
               
                #region InventoryCategory
                if (categoryRecordsForCompany != null)
                {
                    inventory.CategoryNames = new List<string>();
                    foreach (CompanyCategoryRecord ccr in categoryRecordsForCompany.GetCategoryRecords(SoeCategoryRecordEntity.Inventory, inventory.InventoryId, date: null, discardDateIfEmpty: true))
                    {
                        inventory.CategoryNames.Add(ccr.Category.Name);
                    }
                }
                #endregion
            }

            return inventories;
        }
        public List<Inventory> GetInventories(CompEntities entities, int actorCompanyId, bool loadInventoryAccount, int userId, string statuses, bool loadCategories, bool loadInventoryWriteOffMethod = false, bool loadOnlyActive = true, int? inventoryId = null)
        {
            List<CompanyCategoryRecord> categoryRecordsForCompany = null;
            IQueryable<Inventory> query = (from i in entities.Inventory
                                           where i.ActorCompanyId == actorCompanyId
                                           select i);

            if (loadInventoryWriteOffMethod)
            {
                query = query.Include("InventoryWriteOffMethod");
            }
            if (loadOnlyActive)
            {
                query = query.Where(m => m.State == (int)SoeEntityState.Active);
            }

            if (inventoryId.HasValue)
                query = query.Where(m => m.InventoryId == inventoryId.Value);

            var inventories = query.OrderBy(i => i.InventoryNr).ToList();

            // Filter on status
            if (!string.IsNullOrEmpty(statuses))
            {
                var filterOnStatuses = statuses.Split(',').Select(Int32.Parse);
                inventories = inventories.Where(i => filterOnStatuses.Contains(i.Status)).ToList();
            }

            if (loadInventoryAccount)
            {
                int inventoryBaseAccountId = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.AccountInventoryInventories, userId, actorCompanyId, 0);
                AccountStd inventoryBaseAccount = (inventoryBaseAccountId != 0) ? AccountManager.GetAccountStd(entities, inventoryBaseAccountId, actorCompanyId, true, false) : null;

                entities.InventoryAccountStd.NoTracking();
                var inventoryAccounts = (from i in entities.InventoryAccountStd
                                         where i.Inventory.Company.ActorCompanyId == actorCompanyId &&
                                         i.InventoryId.HasValue &&
                                         i.Type == (int)InventoryAccountType.Inventory &&
                                         i.Inventory.State == (int)SoeEntityState.Active
                                         select new
                                         {
                                             i.InventoryId,
                                             AccountNr = i.AccountStd != null && i.AccountStd.Account != null
                                             ? i.AccountStd.Account.AccountNr
                                             : null,
                                             Name = i.AccountStd != null && i.AccountStd.Account != null
                                             ? i.AccountStd.Account.Name
                                             : null
                                         }).ToList();

                foreach (var inventory in inventories)
                {
                    var account = inventoryAccounts.FirstOrDefault(a => a.InventoryId.HasValue && a.InventoryId == inventory.InventoryId);

                    if (account != null)
                    {
                        inventory.InventoryAccountNr = account.AccountNr;
                        inventory.InventoryAccountName = account.Name;
                        inventoryAccounts.Remove(account);
                    }
                    else if (inventoryBaseAccount != null)
                    {
                        inventory.InventoryAccountNr = inventoryBaseAccount.Account.AccountNr;
                        inventory.InventoryAccountName = inventoryBaseAccount.Account.Name;
                    }
                }
            }

            if (loadCategories)
            {
                categoryRecordsForCompany = CategoryManager.GetCompanyCategoryRecords(entities, SoeCategoryType.Inventory, actorCompanyId);
            }

            foreach (var inventory in inventories)
            {
                inventory.StatusName = GetText(inventory.Status, (int)TermGroup.InventoryStatus);

                if (categoryRecordsForCompany != null)
                {
                    inventory.CategoryNames = new List<string>();
                    foreach (CompanyCategoryRecord ccr in categoryRecordsForCompany.GetCategoryRecords(SoeCategoryRecordEntity.Inventory, inventory.InventoryId, date: null, discardDateIfEmpty: true))
                    {
                        inventory.CategoryNames.Add(ccr.Category.Name);
                    }
                }
            }

            return inventories;
        }

        public List<Inventory> GetInventories(CompEntities entities, int actorCompanyId, bool loadAccountDistributionEntriesRows)
        {
            if (loadAccountDistributionEntriesRows)
            {
                return (from i in entities.Inventory
                            .Include("AccountDistributionEntry.InventoryLog")
                            .Include("AccountDistributionEntry.AccountDistributionEntryRow")
                            .Include("AccountDistributionEntry.AccountDistributionEntryRow.AccountInternal.Account.AccountDim")
                            .Include("AccountDistributionEntry.AccountDistributionEntryRow.AccountStd.Account.AccountDim")
                            .Include("InventoryAccountStd.AccountInternal")
                            .Include("InventoryWriteOffMethod")
                        where
                            i.ActorCompanyId == actorCompanyId &&
                            i.State == (int)SoeEntityState.Active &&
                            i.Status != (int)TermGroup_InventoryStatus.Draft &&
                            i.Status != (int)TermGroup_InventoryStatus.Discarded
                        select i).ToList();
            }
            else
            {
                return (from i in entities.Inventory
                            .Include("AccountDistributionEntry.InventoryLog")
                            .Include("InventoryWriteOffMethod")
                        where i.ActorCompanyId == actorCompanyId &&
                        i.State == (int)SoeEntityState.Active &&
                        i.Status != (int)TermGroup_InventoryStatus.Draft
                        select i).ToList();
            }
        }

        public List<InventoryAccountDTO> GetInventoryAccounts(int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.InventoryAccountStd.NoTracking();
            List<InventoryAccountDTO> inventoryAccounts = (from i in entities.InventoryAccountStd
                                                           where i.Inventory.Company.ActorCompanyId == actorCompanyId &&
                                                           i.InventoryId.HasValue &&
                                                           i.Type == (int)InventoryAccountType.Inventory &&
                                                           i.Inventory.State == (int)SoeEntityState.Active &&
                                                           i.AccountId.HasValue
                                                           select new InventoryAccountDTO
                                                           {
                                                               InventoryAccountId = i.AccountStd.Account.AccountId,
                                                               InventoryAccountNr = i.AccountStd.Account.AccountNr,
                                                               InventoryAccountName = i.AccountStd.Account.Name
                                                           }).Distinct().OrderBy(o => o.InventoryAccountId).ToList();


            return inventoryAccounts;
        }

        public Dictionary<int, string> GetInventoriesDict(int actorCompanyId, bool addEmptyValue = true)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Inventory.NoTracking();
            var inventories = (from i in entities.Inventory
                               where i.ActorCompanyId == actorCompanyId &&
                               i.State == (int)SoeEntityState.Active
                               orderby i.Name
                               select i).OrderBy(i => i.InventoryNr).ToList();

            Dictionary<int, string> dict = new Dictionary<int, string>();

            if (addEmptyValue)
                dict.Add(0, " ");

            foreach (var inventory in inventories)
            {
                dict.Add(inventory.InventoryId, inventory.InventoryNr + " - " + inventory.Name);
            }

            return dict;
        }

        public Inventory GetInventory(CompEntities entities, int inventoryId, int actorCompanyId)
        {
            return (from i in entities.Inventory
                    where i.InventoryId == inventoryId && i.ActorCompanyId == actorCompanyId
                    select i).FirstOrDefault();
        }

        /// <summary>
        /// Get specified inventory
        /// </summary>
        /// <param name="inventoryId">Inventory ID</param>
        /// <param name="loadAccountSettings">If true, account settings relations will be loaded</param>
        /// <param name="loadLogs">If true, InventoryLog relation is loaded</param>
        /// <param name="loadSupplierInvoiceInfo">If true, SupplierInvoiceInfo virtual field is populated</param>
        /// <param name="loadCustomerInvoiceInfo">If true, CustomerInvoiceInfo virtual field is populated</param>
        /// <returns>One inventory or null if not found</returns>
        public Inventory GetInventory(int inventoryId, int actorCompanyId, bool loadAccountSettings, bool loadCategories, bool loadLogs, bool loadSupplierInvoiceInfo, bool loadCustomerInvoiceInfo)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Inventory.NoTracking();
            return GetInventory(entities, inventoryId, actorCompanyId, loadAccountSettings, loadCategories, loadLogs, loadSupplierInvoiceInfo, loadCustomerInvoiceInfo);
        }

        /// <summary>
        /// Get specified inventory
        /// </summary>
        /// <param name="entities">Object context</param>
        /// <param name="inventoryId">Inventory ID</param>
        /// <param name="loadAccountSettings">If true, account settings relations will be loaded</param>
        /// <param name="loadLogs">If true, InventoryLog relation is loaded</param>
        /// <param name="loadSupplierInvoiceInfo">If true, SupplierInvoiceInfo virtual field is populated</param>
        /// <param name="loadCustomerInvoiceInfo">If true, CustomerInvoiceInfo virtual field is populated</param>
        /// <returns>One inventory or null if not found</returns>
        public Inventory GetInventory(CompEntities entities, int inventoryId, int actorCompanyId, bool loadAccountSettings, bool loadCategories, bool loadLogs, bool loadSupplierInvoiceInfo, bool loadCustomerInvoiceInfo)
        {
            var inventory = (from i in entities.Inventory
                             where i.InventoryId == inventoryId && i.ActorCompanyId == actorCompanyId
                             select i).FirstOrDefault();

            if (inventory != null)
            {
                // Set status name
                inventory.StatusName = GetText(inventory.Status, (int)TermGroup.InventoryStatus);

                // Set parent inventory name
                if (inventory.ParentId.HasValue)
                {
                    inventory.ParentName = GetInventoryNameAndNumber(entities, inventory.ParentId.Value);
                }

                // Set supplier invoice information
                if (inventory.SupplierInvoiceId.HasValue && loadSupplierInvoiceInfo)
                {
                    Invoice supplierInvoice = InvoiceManager.GetInvoice(entities, inventory.SupplierInvoiceId.Value, loadActor: true);
                    if (supplierInvoice != null)
                        inventory.SupplierInvoiceInfo = string.Format("{0} - {1}", supplierInvoice.InvoiceNr, supplierInvoice.ActorName);
                }

                // Set customer invoice information
                if (inventory.CustomerInvoiceId.HasValue && loadCustomerInvoiceInfo)
                {
                    Invoice customerInvoice = InvoiceManager.GetInvoice(entities, inventory.CustomerInvoiceId.Value, loadActor: true);
                    if (customerInvoice != null)
                        inventory.CustomerInvoiceInfo = string.Format("{0} - {1}", customerInvoice.InvoiceNr, customerInvoice.ActorName);
                }

                // Load relations
                if (loadAccountSettings)
                {
                    if (!inventory.InventoryAccountStd.IsLoaded)
                        inventory.InventoryAccountStd.Load();

                    foreach (var accountStd in inventory.InventoryAccountStd)
                    {
                        // Standard account
                        if (!accountStd.AccountStdReference.IsLoaded)
                            accountStd.AccountStdReference.Load();
                        if (accountStd.AccountStd != null && !accountStd.AccountStd.AccountReference.IsLoaded)
                            accountStd.AccountStd.AccountReference.Load();

                        // Internal accounts
                        if (!accountStd.AccountInternal.IsLoaded)
                            accountStd.AccountInternal.Load();

                        foreach (var accountInternal in accountStd.AccountInternal)
                        {
                            if (!accountInternal.AccountReference.IsLoaded)
                                accountInternal.AccountReference.Load();
                            if (accountInternal.Account != null && !accountInternal.Account.AccountDimReference.IsLoaded)
                                accountInternal.Account.AccountDimReference.Load();
                        }
                    }
                }

                if (loadCategories)
                {
                    inventory.CategoryIds = (from c in entities.CompanyCategoryRecord
                                             where c.RecordId == inventory.InventoryId &&
                                             c.Entity == (int)SoeCategoryRecordEntity.Inventory &&
                                             c.Category.Type == (int)SoeCategoryType.Inventory &&
                                             c.Category.ActorCompanyId == ActorCompanyId &&
                                             c.Category.State == (int)SoeEntityState.Active
                                             select c.CategoryId).ToList();
                }

                if (loadLogs && !inventory.InventoryLog.IsLoaded)
                    inventory.InventoryLog.Load();
            }

            return inventory;
        }

        public string GetInventoryNameAndNumber(CompEntities entities, int inventoryId)
        {
            var inventoryNameAndNumber = (from i in entities.Inventory
                                          where i.InventoryId == inventoryId
                                          select new { i.Name, i.InventoryNr }).FirstOrDefault();

            return (inventoryNameAndNumber != null) ?
                string.Format("{0} - {1}", inventoryNameAndNumber.InventoryNr, inventoryNameAndNumber.Name) : null;

        }

        public string GetInventoryNumber(CompEntities entities, int inventoryId)
        {
            return (from i in entities.Inventory
                    where i.InventoryId == inventoryId
                    select i.InventoryNr).FirstOrDefault();
        }

        public string GetNextInventoryNr(int actorCompanyId)
        {
            int lastNr = 0;
            using var entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            var inventoryNumbers = (from i in entitiesReadOnly.Inventory
                                    where i.ActorCompanyId == actorCompanyId
                                    select i).OrderBy(i => i.InventoryId).Select(i => i.InventoryNr).ToList();

            if (inventoryNumbers.Any())
            {
                Int32.TryParse(inventoryNumbers.Last(), out lastNr);

                // If unable to parse, numeric values are not used
                if (lastNr == 0)
                    return String.Empty;
            }

            lastNr++;

            // Check that number is not used
            if (inventoryNumbers.Any(x => x == lastNr.ToString()))
                return string.Empty;
            else
                return lastNr.ToString();
        }

        /// <summary>
        /// Check if specified inventory number exists for specified company
        /// </summary>
        /// <param name="entities">Object context</param>
        /// <param name="inventoryNr">Inventory number to check</param>
        /// <param name="actorCompanyId">Company ID</param>
        /// <returns>True if the number exist, otherwise false</returns>
        public bool InventoryExist(CompEntities entities, string inventoryNr, int actorCompanyId)
        {
            return (from i in entities.Inventory
                    where i.InventoryNr == inventoryNr &&
                    i.State == (int)SoeEntityState.Active &&
                    i.ActorCompanyId == actorCompanyId
                    select i).Count() > 0;
        }

        /// <summary>
        /// Insert or update inventory
        /// </summary>
        /// <param name="inventoryInput">Inventory entity to insert or update</param>
        /// <param name="categoryRecords">Collection of categories</param>
        /// <param name="accountSettings">Collection of account settings</param>
        /// <param name="actorCompanyId">Company ID</param>
        /// <param name="userId">ID of the user that saves the inventory</param>
        /// <returns>ActionResult</returns>
        public ActionResult SaveInventory(InventoryDTO inventoryInput, List<CompanyCategoryRecordDTO> categoryRecords, List<AccountingSettingDTO> accountSettings, int debtAccountId, int actorCompanyId)
        {
            if (inventoryInput == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "Inventory");

            // Default result is successful
            ActionResult result = new ActionResult();

            int inventoryId = inventoryInput.InventoryId;

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        #region Prereq

                        // Get company
                        Company company = CompanyManager.GetCompany(entities, actorCompanyId);
                        if (company == null)
                            return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte"));

                        #endregion

                        #region Inventory

                        // Get existing inventory
                        Inventory inventory = GetInventory(entities, inventoryId, actorCompanyId, true, false, false, false, false);
                        if (inventory == null || inventory.InventoryNr != inventoryInput.InventoryNr)
                        {
                            // Check if inventory number already exists
                            if (InventoryExist(entities, inventoryInput.InventoryNr, actorCompanyId))
                                return new ActionResult((int)ActionResultSave.InventoryExists);
                        }

                        InventoryLog inventoryLog = null;
                        if (inventory == null)
                        {
                            #region Inventory Add

                            inventory = new Inventory()
                            {
                                InventoryNr = inventoryInput.InventoryNr != null ? inventoryInput.InventoryNr : string.Empty,
                                Name = inventoryInput.Name,
                                Description = inventoryInput.Description,
                                Notes = inventoryInput.Notes,
                                Status = (int)inventoryInput.Status,
                                PurchaseDate = inventoryInput.PurchaseDate,
                                WriteOffDate = inventoryInput.WriteOffDate,
                                PurchaseAmount = inventoryInput.PurchaseAmount,
                                WriteOffAmount = inventoryInput.WriteOffAmount,
                                WriteOffSum = inventoryInput.WriteOffSum,
                                WriteOffPeriods = inventoryInput.WriteOffPeriods,
                                WriteOffRemainingAmount = inventoryInput.WriteOffRemainingAmount,
                                EndAmount = inventoryInput.EndAmount,
                                PeriodType = (int)inventoryInput.PeriodType,
                                PeriodValue = inventoryInput.PeriodValue,
                                State = (int)inventoryInput.State,

                                //Set references
                                Company = company,

                                //Set FK
                                ParentId = inventoryInput.ParentId.HasValue && inventoryInput.ParentId.Value <= 0 ? null : inventoryInput.ParentId,
                                InventoryWriteOffMethodId = inventoryInput.InventoryWriteOffMethodId,
                                VoucherSeriesTypeId = inventoryInput.VoucherSeriesTypeId,
                                SupplierInvoiceId = inventoryInput.SupplierInvoiceId != 0 ? inventoryInput.SupplierInvoiceId : null,
                                CustomerInvoiceId = inventoryInput.CustomerInvoiceId != 0 ? inventoryInput.CustomerInvoiceId : null,
                            };
                            SetCreatedProperties(inventory);
                            entities.Inventory.AddObject(inventory);

                            #endregion

                            #region Accounts Add / Silverlight

                            if (accountSettings != null && inventoryInput.AccountingSettings == null)
                                AddInventoryAccounts(entities, actorCompanyId, inventory, null, accountSettings);

                            #endregion

                            #region AccountingSettings / Angular

                            SetInventoryAccountingSettings(
                                entities,
                                actorCompanyId,
                                inventoryInput,
                                inventory,
                                result,
                                transaction);

                            #endregion

                            #region InventoryLog Add

                            inventoryLog = new InventoryLog()
                            {
                                Type = (int)TermGroup_InventoryLogType.Purchase,
                                Amount = inventoryInput.PurchaseAmount,
                                Date = inventoryInput.PurchaseDate.HasValue ? inventoryInput.PurchaseDate.Value : DateTime.Today,

                                //Set references
                                Inventory = inventory,
                                Company = company,
                                UserId = base.UserId,

                                //Set FK
                                InvoiceId = inventoryInput.SupplierInvoiceId,
                            };

                            //Set currency amounts
                            CountryCurrencyManager.SetCurrencyAmounts(entities, actorCompanyId, inventoryLog);

                            inventory.InventoryLog.Add(inventoryLog);

                            #endregion
                        }
                        else
                        {
                            #region Inventory Update

                            #region Check if InventoryLog should be updated

                            bool updatePurchaseLog = false;
                            // Check if supplier invoice relation has changed
                            int oldSuppInvId = inventory.SupplierInvoiceId.HasValue ? inventory.SupplierInvoiceId.Value : 0;
                            int newSuppInvId = inventoryInput.SupplierInvoiceId.HasValue ? inventoryInput.SupplierInvoiceId.Value : 0;
                            if (oldSuppInvId != newSuppInvId)
                                updatePurchaseLog = true;

                            // Check if purchase date has changed
                            if (inventory.PurchaseDate != inventoryInput.PurchaseDate)
                                updatePurchaseLog = true;

                            // Check if purchase amount has changed
                            if (inventory.PurchaseAmount != inventoryInput.PurchaseAmount)
                                updatePurchaseLog = true;

                            #endregion

                            inventory.InventoryNr = inventoryInput.InventoryNr;
                            inventory.Name = inventoryInput.Name;
                            inventory.Description = inventoryInput.Description;
                            inventory.Notes = inventoryInput.Notes;
                            inventory.Status = (int)inventoryInput.Status;
                            inventory.PurchaseDate = inventoryInput.PurchaseDate;
                            inventory.WriteOffDate = inventoryInput.WriteOffDate;
                            inventory.PurchaseAmount = inventoryInput.PurchaseAmount;
                            inventory.WriteOffAmount = inventoryInput.WriteOffAmount;
                            inventory.WriteOffSum = inventoryInput.WriteOffSum;
                            inventory.WriteOffPeriods = inventoryInput.WriteOffPeriods;
                            inventory.WriteOffRemainingAmount = inventoryInput.WriteOffRemainingAmount;
                            inventory.EndAmount = inventoryInput.EndAmount;
                            inventory.PeriodType = (int)inventoryInput.PeriodType;
                            inventory.PeriodValue = inventoryInput.PeriodValue;
                            inventory.State = (int)inventoryInput.State;

                            //Set references
                            inventory.Company = company;

                            //Set FK
                            inventory.ParentId = inventoryInput.ParentId.HasValue && inventoryInput.ParentId.Value <= 0 ? null : inventoryInput.ParentId;
                            inventory.InventoryWriteOffMethodId = inventoryInput.InventoryWriteOffMethodId;
                            inventory.VoucherSeriesTypeId = inventoryInput.VoucherSeriesTypeId;
                            inventory.SupplierInvoiceId = inventoryInput.SupplierInvoiceId.HasValue && inventoryInput.SupplierInvoiceId.Value <= 0 ? null : inventoryInput.SupplierInvoiceId;
                            inventory.CustomerInvoiceId = inventoryInput.CustomerInvoiceId.HasValue && inventoryInput.CustomerInvoiceId.Value <= 0 ? null : inventoryInput.CustomerInvoiceId;

                            SetModifiedProperties(inventory);

                            #endregion

                            #region Accounts Update / Silverlight

                            if (accountSettings != null && inventoryInput.AccountingSettings == null)
                                UpdateInventoryAccounts(entities, actorCompanyId, inventory, null, accountSettings);

                            #endregion

                            #region AccountingSettings / Angular

                            SetInventoryAccountingSettings(
                                entities,
                                actorCompanyId,
                                inventoryInput,
                                inventory,
                                result,
                                transaction);

                            #endregion

                            #region InventoryLog Update

                            if (updatePurchaseLog)
                            {
                                IEnumerable<InventoryLog> logs = GetInventoryLogs(entities, inventory.InventoryId, TermGroup_InventoryLogType.Purchase, true, false);
                                if (logs.Any())
                                {
                                    inventoryLog = logs.First();
                                    inventoryLog.Date = inventoryInput.PurchaseDate.HasValue ? inventoryInput.PurchaseDate.Value : DateTime.Today;
                                    inventoryLog.Amount = inventoryInput.PurchaseAmount;

                                    //Set references
                                    inventoryLog.UserId = base.UserId;

                                    //Set FK
                                    inventoryLog.InvoiceId = inventoryInput.SupplierInvoiceId;
                                }
                            }

                            result = SaveTrackChanges(entities, transaction, inventoryInput, inventory, TermGroup_TrackChangesAction.Update, actorCompanyId);
                            if (!result.Success)
                            {
                                return result;
                            }

                            #endregion
                        }

                        #endregion

                        #region Rebook debt

                        if (debtAccountId != 0)
                        {
                            // Get debt account
                            AccountStd debtAccount = null;
                            debtAccount = AccountManager.GetAccountStd(entities, debtAccountId, actorCompanyId, false, false);

                            // Get inventory account
                            AccountStd inventoryAccount = null;
                            int inventoryAccountId = 0;
                            AccountingSettingDTO accountSetting = accountSettings.FirstOrDefault(a => a.DimNr == Constants.ACCOUNTDIM_STANDARD);
                            if (accountSetting != null)
                                inventoryAccountId = accountSetting.Account1Id;
                            if (inventoryAccountId == 0)
                                inventoryAccountId = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.AccountInventoryInventories, 0, actorCompanyId, 0);
                            if (inventoryAccountId != 0)
                                inventoryAccount = AccountManager.GetAccountStd(entities, inventoryAccountId, actorCompanyId, false, false);

                            if (debtAccount != null && inventoryAccount != null)
                            {
                                // Get purchase inventory log
                                if (inventoryLog == null)
                                {
                                    IEnumerable<InventoryLog> logs = GetInventoryLogs(entities, inventory.InventoryId, TermGroup_InventoryLogType.Purchase, true, false);
                                    if (logs.Any())
                                        inventoryLog = logs.First();
                                }

                                #region AccountDistributionEntry

                                // Check if AccountDistributionEntry alredy exists
                                AccountDistributionEntry rebookEntry = null;
                                if (inventoryLog != null)
                                {
                                    if (!inventoryLog.AccountDistributionEntryReference.IsLoaded)
                                        inventoryLog.AccountDistributionEntryReference.Load();

                                    // Link AccountDistributionEntry to log record
                                    rebookEntry = inventoryLog.AccountDistributionEntry;
                                }

                                if (rebookEntry == null)
                                {
                                    // Create AccountDistributionEntry
                                    rebookEntry = new AccountDistributionEntry()
                                    {
                                        //Set references
                                        Inventory = inventory,
                                        Company = company,
                                    };
                                    SetCreatedProperties(rebookEntry);
                                }
                                else
                                    SetModifiedProperties(rebookEntry);

                                // Update AccountDistributionEntry
                                rebookEntry.Date = inventoryInput.PurchaseDate.HasValue ? inventoryInput.PurchaseDate.Value : DateTime.Today;

                                #endregion

                                #region AccountDistributionEntryRow

                                AccountDistributionEntryRow creditRow = null;
                                AccountDistributionEntryRow debitRow = null;

                                if (!rebookEntry.IsAdded() && !rebookEntry.AccountDistributionEntryRow.IsLoaded)
                                    rebookEntry.AccountDistributionEntryRow.Load();

                                if (rebookEntry.AccountDistributionEntryRow.Count == 0)
                                {
                                    // Create Credit row
                                    creditRow = new AccountDistributionEntryRow();
                                    rebookEntry.AccountDistributionEntryRow.Add(creditRow);

                                    // Create Debit row
                                    debitRow = new AccountDistributionEntryRow();
                                    rebookEntry.AccountDistributionEntryRow.Add(debitRow);
                                }

                                // Update Credit row
                                if (creditRow == null)
                                    creditRow = rebookEntry.AccountDistributionEntryRow.FirstOrDefault(a => a.CreditAmount > 0);
                                if (creditRow != null)
                                {
                                    creditRow.AccountStd = debtAccount;
                                    creditRow.CreditAmount = inventoryInput.PurchaseAmount;
                                }
                                // Update Debit row
                                if (debitRow == null)
                                    debitRow = rebookEntry.AccountDistributionEntryRow.FirstOrDefault(a => a.DebitAmount > 0);
                                if (debitRow != null)
                                {
                                    debitRow.AccountStd = inventoryAccount;
                                    debitRow.DebitAmount = inventoryInput.PurchaseAmount;
                                }

                                //Set currency amounts
                                CountryCurrencyManager.SetCurrencyAmounts(entities, actorCompanyId, creditRow);
                                CountryCurrencyManager.SetCurrencyAmounts(entities, actorCompanyId, debitRow);

                                #endregion
                            }
                        }

                        #endregion                        

                        result = SaveChanges(entities, transaction);
                        if (result.Success)
                        {
                            //Commit transaction
                            transaction.Complete();

                            inventoryId = inventory.InventoryId;
                        }
                        else
                            result.ErrorNumber = (int)ActionResultSave.InventoryNotSaved;
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
                        result.IntegerValue = inventoryId;

                        #region Categories

                        ActionResult categoryResult = new ActionResult();
                        categoryResult = CategoryManager.SaveCompanyCategoryRecords(entities, null, categoryRecords != null ? categoryRecords : new List<CompanyCategoryRecordDTO>(), actorCompanyId, SoeCategoryType.Inventory, SoeCategoryRecordEntity.Inventory, inventoryId);
                        if (!categoryResult.Success)
                        {
                            result.ErrorNumber = (int)ActionResultSave.InventoryCompanyCategoryNotSaved;
                        }

                        #endregion

                        #region files
                        //Save files
                        if (inventoryInput.InventoryFiles != null)
                        {
                            var images = inventoryInput.InventoryFiles.Where(f => f.ImageId.HasValue);
                            GraphicsManager.UpdateImages(entities, images, inventoryId);

                            var files = inventoryInput.InventoryFiles.Where(f => f.Id.HasValue);
                            GeneralManager.UpdateFiles(entities, files, inventoryId);

                            entities.SaveChanges();
                        }
                        #endregion

                    }
                    else
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }

                return result;
            }
        }


        private void SetInventoryAccountingSettings(
            CompEntities entities, 
            int actorCompanyId, 
            InventoryDTO inventoryInput, 
            Inventory inventory, 
            ActionResult result,
            TransactionScope transaction)
        {
            if (inventoryInput.AccountingSettings != null && inventoryInput.AccountingSettings.Count > 0)
            {
                #region Prereq

                List<AccountDim> dims = AccountManager.GetAccountDimsByCompany(entities, actorCompanyId, onlyInternal: true);

                #endregion

                #region Delete AccountingSettings

                // Loop over existing settings
                foreach (InventoryAccountStd inventoryAccountStd in inventory.InventoryAccountStd.ToList())
                {
                    //Delete account
                    inventoryAccountStd.AccountInternal.Clear();
                    inventory.InventoryAccountStd.Remove(inventoryAccountStd);
                    entities.DeleteObject(inventoryAccountStd);
                }

                #endregion

                #region Add AccountingSettings                           

                if (inventory.InventoryAccountStd == null)
                    inventory.InventoryAccountStd = new System.Data.Entity.Core.Objects.DataClasses.EntityCollection<InventoryAccountStd>();

                foreach (AccountingSettingsRowDTO settingInput in inventoryInput.AccountingSettings)
                {
                    if (settingInput.Account1Id <= 0
                        && settingInput.Account2Id <= 0
                        && settingInput.Account3Id <= 0
                        && settingInput.Account4Id <= 0
                        && settingInput.Account5Id <= 0
                        && settingInput.Account6Id <= 0)
                    {
                        continue;
                    }

                    InventoryAccountStd inventoryAccountStd = new InventoryAccountStd
                    {
                        Type = settingInput.Type
                    };

                    if (settingInput.Account1Id > 0)
                    {
                        inventoryAccountStd.AccountStd = AccountManager.GetAccountStd(
                            entities,
                            settingInput.Account1Id,
                            actorCompanyId,
                            true,
                            true);
                    }

                    inventory.InventoryAccountStd.Add(inventoryAccountStd);

                    // Internal accounts
                    int dimCounter = 1;
                    foreach (AccountDim dim in dims)
                    {
                        // Get internal account from input
                        dimCounter++;
                        int accountId = 0;
                        if (dimCounter == 2)
                            accountId = settingInput.Account2Id;
                        else if (dimCounter == 3)
                            accountId = settingInput.Account3Id;
                        else if (dimCounter == 4)
                            accountId = settingInput.Account4Id;
                        else if (dimCounter == 5)
                            accountId = settingInput.Account5Id;
                        else if (dimCounter == 6)
                            accountId = settingInput.Account6Id;

                        // Add account internal
                        AccountInternal accountInternal = AccountManager.GetAccountInternal(entities, accountId, actorCompanyId);
                        if (accountInternal != null)
                            inventoryAccountStd.AccountInternal.Add(accountInternal);
                    }

                }

                #endregion

                result = SaveChanges(entities, transaction);
            }
        }

        public ActionResult SaveNoteAndDescripiton(int inventoryId, string description, string notes, int actorCompanyId)
        {
            // Default result is successful
            ActionResult result = new ActionResult();

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();
                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        // Get existing inventory
                        Inventory inventory = GetInventory(entities, inventoryId, actorCompanyId, false, false, false, false, false);
                        if (inventory == null)
                            return new ActionResult((int)ActionResultSave.InventoryDoesNotExist);

                        // Update values
                        inventory.Description = description;
                        inventory.Notes = notes;

                        // Update Modified and ModifiedBy
                        SetModifiedProperties(inventory);

                        result = SaveChanges(entities, transaction);
                        if (result.Success)
                            transaction.Complete(); //Commit transaction
                        else
                            result.ErrorNumber = (int)ActionResultSave.InventoryNotSaved;
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
                    if (!result.Success)
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }

                return result;
            }
        }
        public ActionResult SaveTrackChanges(CompEntities entities, TransactionScope transaction, InventoryDTO input, Inventory existingInventory, TermGroup_TrackChangesAction actionType, int actorCompanyId)
        {
            if (existingInventory == null || input.InventoryId == 0) return new ActionResult();
            var inventoryId = existingInventory.InventoryId;
            var changes = new List<TrackChangesDTO>();

            #region helper functions

            Action<decimal, decimal, TermGroup_TrackChangesColumnType> checkDecimal = (oldVal, newVal, type) => {
                if (oldVal != newVal)                
                {
                    changes.Add(TrackChangesManager.CreateTrackChangesDTO(
                        actorCompanyId, SoeEntityType.Inventory, type, inventoryId, 
                        oldVal.ToString(), newVal.ToString(), 
                        actionType, SettingDataType.Decimal, SoeEntityType.None, 0, null, null
                    ));
                }
            };
            
            Action<int, int, TermGroup_TrackChangesColumnType> checkInt = (oldVal, newVal, type) => {
                if (oldVal != newVal)
                {
                     changes.Add(TrackChangesManager.CreateTrackChangesDTO(
                        actorCompanyId, SoeEntityType.Inventory, type, inventoryId,
                        oldVal.ToString(), newVal.ToString(),
                        actionType, SettingDataType.Integer, SoeEntityType.None, 0, null, null
                    ));
                }
            };

            Action<DateTime?, DateTime?, TermGroup_TrackChangesColumnType> checkDate = (oldVal, newVal, type) => {
                if (oldVal.GetValueOrDefault().Date != newVal.GetValueOrDefault().Date)
                {
                     changes.Add(TrackChangesManager.CreateTrackChangesDTO(
                        actorCompanyId, SoeEntityType.Inventory, type, inventoryId,
                        oldVal?.ToString("yyyy-MM-dd"), newVal?.ToString("yyyy-MM-dd"),
                        actionType, SettingDataType.Date, SoeEntityType.None, 0, null, null
                    ));
                }
            };

            Action<int, int> checkStatus = (oldStatus, newStatus) =>
            {
                if (oldStatus != newStatus)
                {
                    changes.Add(TrackChangesManager.CreateTrackChangesDTO(
                        actorCompanyId, SoeEntityType.Inventory, TermGroup_TrackChangesColumnType.Inventory_Status, inventoryId,
                        oldStatus.ToString(), newStatus.ToString(),
                        actionType, SettingDataType.Integer, SoeEntityType.None, 0,
                        existingInventory.StatusName, input.StatusName
                    ));
                }
            };

            Action<int, int> checkPeriodType = (oldPeriodType, newPeriodType) =>
            {
                if (oldPeriodType != newPeriodType)
                {
                    changes.Add(TrackChangesManager.CreateTrackChangesDTO(
                        actorCompanyId, SoeEntityType.Inventory, TermGroup_TrackChangesColumnType.Inventory_PeriodType, inventoryId,
                        oldPeriodType.ToString(), newPeriodType.ToString(),
                        actionType, SettingDataType.Integer, SoeEntityType.None, 0,
                        existingInventory.PeriodType.ToString(), input.PeriodType.ToString()
                    ));
                }
            };

            Action<int, int> checkWriteOffMethod = (oldWriteOffMethodId, newWriteOffMethodId) =>
            {
                if (oldWriteOffMethodId != newWriteOffMethodId)
                {
                    var fromValueName = existingInventory == null ? null : GetInventoryWriteOffMethod(entities, existingInventory.InventoryWriteOffMethodId, false, false)?.Name;
                    var toValueName = input == null ? null : GetInventoryWriteOffMethod(entities, input.InventoryWriteOffMethodId, false, false)?.Name;
                    changes.Add(TrackChangesManager.CreateTrackChangesDTO(
                        actorCompanyId,
                        SoeEntityType.Inventory,
                        TermGroup_TrackChangesColumnType.Inventory_WriteOffMethod,
                        inventoryId,
                        oldWriteOffMethodId.ToString(),
                        newWriteOffMethodId.ToString(),
                        actionType,
                        SettingDataType.Integer,
                        SoeEntityType.None,
                        0,
                        fromValueName,
                        toValueName
                    ));
                }
            };

            Action<int, int> checkVoucherSeriesType = (oldVoucherSeriesTypeId, newVoucherSeriesTypeId) =>
            {
                if (oldVoucherSeriesTypeId != newVoucherSeriesTypeId)
                {

                    var fromValueName = existingInventory == null ? null : VoucherManager.GetVoucherSeriesType(entities, existingInventory.VoucherSeriesTypeId, actorCompanyId)?.Name;
                    var toValueName = input == null ? null : VoucherManager.GetVoucherSeriesType(entities, input.VoucherSeriesTypeId, actorCompanyId)?.Name;

                    changes.Add(TrackChangesManager.CreateTrackChangesDTO(
                        actorCompanyId,
                        SoeEntityType.Inventory,
                        TermGroup_TrackChangesColumnType.Inventory_VoucherSeries,
                        inventoryId,
                        existingInventory?.VoucherSeriesTypeId.ToString(),
                        input?.VoucherSeriesTypeId.ToString(),
                        actionType,
                        SettingDataType.Integer,
                        SoeEntityType.None,
                        0,
                        fromValueName,
                        toValueName
                    ));
                }
            };

            #endregion

            var trackStringFields = new[] {
                new SmallGenericType { Name = "InventoryNr", Id = (int)TermGroup_TrackChangesColumnType.Inventory_Nr },
                new SmallGenericType { Name = "Name", Id = (int)TermGroup_TrackChangesColumnType.Inventory_Name },
                new SmallGenericType { Name = "Description", Id = (int)TermGroup_TrackChangesColumnType.Inventory_Description },
            }.ToList();

            changes.AddRange(TrackChangesManager.CreateTrackStringChanges(
                trackStringFields, 
                actorCompanyId, 
                SoeEntityType.Inventory, 
                inventoryId, 
                existingInventory, 
                input, 
                actionType
            ));

            if (actionType == TermGroup_TrackChangesAction.Update)
            {
                checkDecimal(existingInventory.PurchaseAmount, input.PurchaseAmount, TermGroup_TrackChangesColumnType.Inventory_PurchaseAmount);
                checkDecimal(existingInventory.WriteOffAmount, input.WriteOffAmount, TermGroup_TrackChangesColumnType.Inventory_WriteOffAmount);
                checkDecimal(existingInventory.WriteOffSum, input.WriteOffSum, TermGroup_TrackChangesColumnType.Inventory_WriteOffSum);
                checkDecimal(existingInventory.WriteOffRemainingAmount, input.WriteOffRemainingAmount, TermGroup_TrackChangesColumnType.Inventory_WriteOffRemainingAmount);
                checkDecimal(existingInventory.EndAmount, input.EndAmount, TermGroup_TrackChangesColumnType.Inventory_EndAmount);
                
                checkInt(existingInventory.WriteOffPeriods, input.WriteOffPeriods, TermGroup_TrackChangesColumnType.Inventory_WriteOffPeriods);
                checkInt(existingInventory.PeriodValue, input.PeriodValue, TermGroup_TrackChangesColumnType.Inventory_PeriodValue);

                checkDate(existingInventory.PurchaseDate, input.PurchaseDate, TermGroup_TrackChangesColumnType.Inventory_PurchaseDate);
                checkDate(existingInventory.WriteOffDate, input.WriteOffDate, TermGroup_TrackChangesColumnType.Inventory_WriteOffDate);

                checkStatus((int)existingInventory.Status, (int)input.Status);
                checkPeriodType((int)existingInventory.PeriodType, (int)input.PeriodType);

                checkWriteOffMethod(existingInventory.InventoryWriteOffMethodId, input.InventoryWriteOffMethodId);
                checkVoucherSeriesType(existingInventory.VoucherSeriesTypeId, input.VoucherSeriesTypeId);
            }

            return changes.Any() ? TrackChangesManager.AddTrackChanges(entities, transaction, changes) : new ActionResult();
        }

        /// <summary>
        /// Sets an inventorys state to Deleted
        /// </summary>
        /// <param name="inventoryId">ID of inventory to delete</param>
        /// <returns>ActionResult</returns>
        public ActionResult DeleteInventory(int inventoryId, int actorCompanyId)
        {
            var result = new ActionResult();

            using (CompEntities entities = new CompEntities())
            {
                // Get inventory
                Inventory inventory = GetInventory(entities, inventoryId, actorCompanyId, false, false, false, false, false);
                if (inventory == null)
                    return new ActionResult((int)ActionResultDelete.EntityIsNull, "Inventory");

                // Check relation dependencies
                if (!inventory.AccountDistributionEntry.IsLoaded)
                    inventory.AccountDistributionEntry.Load();

                if (inventory.AccountDistributionEntry.Any(a => a.VoucherHeadId != null))
                    return new ActionResult((int)ActionResultDelete.InventoryLinkedWithVouchers, GetText(12546, "Kunde inte ta bort inventarien eftersom den har kopplade verifikat"));

                // Set the inventory to deleted
                result = ChangeEntityState(entities, inventory, SoeEntityState.Deleted, true);
            }

            return result;
        }

        #region AccountSettings

        private void AddInventoryAccounts(CompEntities entities, int actorCompanyId, Inventory inventory, InventoryWriteOffTemplate template, List<AccountingSettingDTO> accountItems)
        {
            if (accountItems.Count == 0)
                return;

            // index
            // 1 = Inventory
            // 2 = AccWriteOff
            // 3 = WriteOff
            // 4 = AccOverWriteOff
            // 5 = OverWriteOff
            // 6 = AccWriteDown
            // 7 = WriteDown
            // 8 = AccWriteUp
            // 9 = WriteUp
            InventoryAccountStd inventoryAccountStd;
            for (int i = 1; i <= 9; i++)
            {
                inventoryAccountStd = AddInventoryAccount(entities, actorCompanyId, accountItems, i);
                if (inventoryAccountStd != null)
                {
                    if (inventory != null)
                        inventory.InventoryAccountStd.Add(inventoryAccountStd);
                    else if (template != null)
                        template.InventoryAccountStd.Add(inventoryAccountStd);
                }
            }
        }

        private InventoryAccountStd AddInventoryAccount(CompEntities entities, int actorCompanyId, List<AccountingSettingDTO> accountItems, int index)
        {
            // index
            // 1 = Inventory
            // 2 = AccWriteOff
            // 3 = WriteOff
            // 4 = AccOverWriteOff
            // 5 = OverWriteOff
            // 6 = AccWriteDown
            // 7 = WriteDown
            // 8 = AccWriteUp
            // 9 = WriteUp
            AccountingSettingDTO stdItem = accountItems.FirstOrDefault(a => a.Type == index);
            if (stdItem != null)
            {
                int stdAccountId = 0;
                int stdAccountType = 0;
                if (index == 1)
                {
                    stdAccountId = stdItem.Account1Id;
                    stdAccountType = (int)InventoryAccountType.Inventory;
                }
                else if (index == 2)
                {
                    stdAccountId = stdItem.Account2Id;
                    stdAccountType = (int)InventoryAccountType.AccWriteOff;
                }
                else if (index == 3)
                {
                    stdAccountId = stdItem.Account3Id;
                    stdAccountType = (int)InventoryAccountType.WriteOff;
                }
                else if (index == 4)
                {
                    stdAccountId = stdItem.Account4Id;
                    stdAccountType = (int)InventoryAccountType.AccOverWriteOff;
                }
                else if (index == 5)
                {
                    stdAccountId = stdItem.Account5Id;
                    stdAccountType = (int)InventoryAccountType.OverWriteOff;
                }
                else if (index == 6)
                {
                    stdAccountId = stdItem.Account6Id;
                    stdAccountType = (int)InventoryAccountType.AccWriteDown;
                }
                else if (index == 7)
                {
                    stdAccountId = stdItem.Account7Id;
                    stdAccountType = (int)InventoryAccountType.WriteDown;
                }
                else if (index == 8)
                {
                    stdAccountId = stdItem.Account8Id;
                    stdAccountType = (int)InventoryAccountType.AccWriteUp;
                }
                else if (index == 9)
                {
                    stdAccountId = stdItem.Account9Id;
                    stdAccountType = (int)InventoryAccountType.WriteUp;
                }

                // Standard account
                AccountStd accountStd = AccountManager.GetAccountStd(entities, stdAccountId, actorCompanyId, false, false);
                if (accountStd != null)
                {
                    var inventoryAccountStd = new InventoryAccountStd
                    {
                        Type = stdAccountType,
                        AccountStd = accountStd
                    };

                    // Add internal accounts
                    AddInternalAccountsToStdAccount(entities, actorCompanyId, inventoryAccountStd, accountItems, index);

                    return inventoryAccountStd;
                }
            }

            return null;
        }

        private void UpdateInventoryAccounts(CompEntities entities, int actorCompanyId, Inventory inventory, InventoryWriteOffTemplate template, List<AccountingSettingDTO> accountItems)
        {
            for (int index = 1; index <= 9; index++)
            {
                // Type may differ from index, since index is just the column order in the DataGrid
                // 1 = Inventory
                // 2 = AccWriteOff
                // 3 = WriteOff
                // 4 = AccOverWriteOff
                // 5 = OverWriteOff
                // 6 = AccWriteDown
                // 7 = WriteDown
                // 8 = AccWriteUp
                // 9 = WriteUp
                InventoryAccountType type = InventoryAccountType.Inventory;
                if (index == 1)
                    type = InventoryAccountType.Inventory;
                else if (index == 2)
                    type = InventoryAccountType.AccWriteOff;
                else if (index == 3)
                    type = InventoryAccountType.WriteOff;
                else if (index == 4)
                    type = InventoryAccountType.AccOverWriteOff;
                else if (index == 5)
                    type = InventoryAccountType.OverWriteOff;
                else if (index == 6)
                    type = InventoryAccountType.AccWriteDown;
                else if (index == 7)
                    type = InventoryAccountType.WriteDown;
                else if (index == 8)
                    type = InventoryAccountType.AccWriteUp;
                else if (index == 9)
                    type = InventoryAccountType.WriteUp;

                InventoryAccountStd inventoryAccountStd = null;
                if (inventory != null)
                    inventoryAccountStd = inventory.InventoryAccountStd.FirstOrDefault(a => a.Type == (int)type);
                else if (template != null)
                    inventoryAccountStd = template.InventoryAccountStd.FirstOrDefault(a => a.Type == (int)type);
                if (inventoryAccountStd == null)
                {
                    // No accounts exists, call add method instead
                    inventoryAccountStd = AddInventoryAccount(entities, actorCompanyId, accountItems, index);
                    if (inventoryAccountStd != null)
                    {
                        if (inventory != null)
                            inventory.InventoryAccountStd.Add(inventoryAccountStd);
                        else if (template != null)
                            template.InventoryAccountStd.Add(inventoryAccountStd);
                    }
                }
                else
                {
                    // Always remove and add internal accounts
                    inventoryAccountStd.AccountInternal.Clear();

                    AccountingSettingDTO stdItem = accountItems.FirstOrDefault(a => a.DimNr == Constants.ACCOUNTDIM_STANDARD);
                    if (stdItem != null)
                    {
                        int stdAccountId = 0;
                        if (index == 1)
                            stdAccountId = stdItem.Account1Id;
                        else if (index == 2)
                            stdAccountId = stdItem.Account2Id;
                        else if (index == 3)
                            stdAccountId = stdItem.Account3Id;
                        else if (index == 4)
                            stdAccountId = stdItem.Account4Id;
                        else if (index == 5)
                            stdAccountId = stdItem.Account5Id;
                        else if (index == 6)
                            stdAccountId = stdItem.Account6Id;
                        else if (index == 7)
                            stdAccountId = stdItem.Account7Id;
                        else if (index == 8)
                            stdAccountId = stdItem.Account8Id;
                        else if (index == 9)
                            stdAccountId = stdItem.Account9Id;

                        // Update standard account
                        AccountStd accountStd = AccountManager.GetAccountStd(entities, stdAccountId, actorCompanyId, false, false);
                        if (accountStd != null)
                        {
                            inventoryAccountStd.AccountStd = accountStd;

                            // Add internal accounts
                            AddInternalAccountsToStdAccount(entities, actorCompanyId, inventoryAccountStd, accountItems, index);
                        }
                        else
                        {
                            // Remove standard account
                            if (inventory != null)
                                inventory.InventoryAccountStd.Remove(inventoryAccountStd);
                            else if (template != null)
                                template.InventoryAccountStd.Remove(inventoryAccountStd);
                            entities.DeleteObject(inventoryAccountStd);
                        }
                    }
                    else
                    {
                        // Remove standard account
                        if (inventory != null)
                            inventory.InventoryAccountStd.Remove(inventoryAccountStd);
                        else if (template != null)
                            template.InventoryAccountStd.Remove(inventoryAccountStd);
                        entities.DeleteObject(inventoryAccountStd);
                    }
                }
            }
        }

        private void SetInventoryAccounts(CompEntities entities, int actorCompanyId, Inventory inventory, InventoryWriteOffTemplate template, List<AccountingSettingDTO> accountItems)
        {
            InventoryAccountStd inventoryAccountStd = null;

            foreach (var item in accountItems)
            {
                if (inventory != null)
                    inventoryAccountStd = inventory.InventoryAccountStd.FirstOrDefault(a => a.Type == item.Type);
                else if (template != null)
                    inventoryAccountStd = template.InventoryAccountStd.FirstOrDefault(a => a.Type == item.Type);

                AccountStd accountStd = AccountManager.GetAccountStd(entities, item.Account1Id, actorCompanyId, false, false);

                if (inventoryAccountStd == null && accountStd != null)
                {
                    //Add standard account
                    inventoryAccountStd = new InventoryAccountStd
                    {
                        Type = item.Type,
                        AccountStd = accountStd
                    };

                    if (inventory != null)
                        inventory.InventoryAccountStd.Add(inventoryAccountStd);
                    else if (template != null)
                        template.InventoryAccountStd.Add(inventoryAccountStd);
                }

                if (accountStd != null)
                {
                    //Update standard account
                    inventoryAccountStd.AccountStd = accountStd;

                    // Always remove and add internal accounts
                    inventoryAccountStd.AccountInternal.Clear();

                    if (item.Account2Id != 0)
                    {
                        AccountInternal accountInt = AccountManager.GetAccountInternal(entities, item.Account2Id, actorCompanyId);
                        if (accountInt != null)
                            inventoryAccountStd.AccountInternal.Add(accountInt);
                    }
                    if (item.Account3Id != 0)
                    {
                        AccountInternal accountInt = AccountManager.GetAccountInternal(entities, item.Account3Id, actorCompanyId);
                        if (accountInt != null)
                            inventoryAccountStd.AccountInternal.Add(accountInt);
                    }
                    if (item.Account4Id != 0)
                    {
                        AccountInternal accountInt = AccountManager.GetAccountInternal(entities, item.Account4Id, actorCompanyId);
                        if (accountInt != null)
                            inventoryAccountStd.AccountInternal.Add(accountInt);
                    }
                    if (item.Account5Id != 0)
                    {
                        AccountInternal accountInt = AccountManager.GetAccountInternal(entities, item.Account5Id, actorCompanyId);
                        if (accountInt != null)
                            inventoryAccountStd.AccountInternal.Add(accountInt);
                    }
                    if (item.Account6Id != 0)
                    {
                        AccountInternal accountInt = AccountManager.GetAccountInternal(entities, item.Account6Id, actorCompanyId);
                        if (accountInt != null)
                            inventoryAccountStd.AccountInternal.Add(accountInt);
                    }
                }
                else if (inventoryAccountStd != null && accountStd == null)
                {
                    // Remove standard account
                    if (inventory != null)
                        inventory.InventoryAccountStd.Remove(inventoryAccountStd);
                    else if (template != null)
                        template.InventoryAccountStd.Remove(inventoryAccountStd);
                    entities.DeleteObject(inventoryAccountStd);
                }
            }
        }

        private void AddInternalAccountsToStdAccount(CompEntities entities, int actorCompanyId, InventoryAccountStd inventoryAccountStd, List<AccountingSettingDTO> accountItems, int index)
        {
            // Add internal accounts
            foreach (AccountingSettingDTO intItem in accountItems.Where(a => a.DimNr != Constants.ACCOUNTDIM_STANDARD))
            {
                int intAccountId = 0;
                if (index == 1)
                    intAccountId = intItem.Account1Id;
                else if (index == 2)
                    intAccountId = intItem.Account2Id;
                else if (index == 3)
                    intAccountId = intItem.Account3Id;
                else if (index == 4)
                    intAccountId = intItem.Account4Id;
                else if (index == 5)
                    intAccountId = intItem.Account5Id;
                else if (index == 6)
                    intAccountId = intItem.Account6Id;
                else if (index == 7)
                    intAccountId = intItem.Account7Id;
                else if (index == 8)
                    intAccountId = intItem.Account8Id;
                else if (index == 9)
                    intAccountId = intItem.Account9Id;

                AccountInternal accountInt = AccountManager.GetAccountInternal(entities, intAccountId, actorCompanyId);
                if (accountInt != null)
                    inventoryAccountStd.AccountInternal.Add(accountInt);
            }
        }

        #endregion

        #endregion

        #region InventoryLog

        /// <summary>
        /// Get inventory trace views (InventoryLog) for specified inventory
        /// </summary>
        /// <param name="inventoryId">Inventory ID</param>
        /// <returns>Collection of trace view records</returns>
        public IEnumerable<InventoryTraceViewDTO> GetInventoryTraceViews(int inventoryId)
        {
            List<InventoryTraceViewDTO> dtos = new List<InventoryTraceViewDTO>();

            int langId = GetLangId();
            var inventoryLogTypes = base.GetTermGroupDict(TermGroup.InventoryLogType, langId);

            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.InventoryTraceView.NoTracking();
            var items = (from i in entities.InventoryTraceView
                         where i.InventoryId == inventoryId &&
                         (i.VoucherHeadId != null || i.InvoiceId != null)
                         orderby i.Date descending, i.InventoryLogId descending
                         select i).ToList();

            foreach (var item in items)
            {
                var dto = item.ToDTO();
                dto.TypeName = dto.Type != 0 ? inventoryLogTypes[(int)dto.Type] : "";
                dtos.Add(dto);
            }

            return dtos;
        }

        /// <summary>
        /// Insert an inventory log record
        /// </summary>
        /// <param name="entities">Object context</param>
        /// <param name="inventoryLogInput">InventoryLog entity to insert</param>
        public void AddInventoryLog(CompEntities entities, InventoryLog inventoryLogInput)
        {
            if (inventoryLogInput == null)
                return;

            #region Prereq

            // Get company
            Company company = CompanyManager.GetCompany(entities, inventoryLogInput.ActorCompanyId);
            if (company == null)
                return;

            #endregion

            #region InventoryLog

            InventoryLog inventoryLog = new InventoryLog()
            {
                Type = inventoryLogInput.Type,
                Amount = inventoryLogInput.Amount,
                Date = inventoryLogInput.Date,

                //Set references
                Company = company,

                //Set FK
                InventoryId = inventoryLogInput.InventoryId,
                InvoiceId = inventoryLogInput.InvoiceId,
                AccountDistributionEntryId = inventoryLogInput.AccountDistributionEntryId,
                VoucherHeadId = inventoryLogInput.VoucherHeadId,
                UserId = inventoryLogInput.UserId,
            };
            entities.InventoryLog.AddObject(inventoryLog);

            //Set currency amounts
            CountryCurrencyManager.SetCurrencyAmounts(entities, company.ActorCompanyId, inventoryLog);

            base.TryDetachEntity(entities, inventoryLogInput);

            #endregion
        }

        /// <summary>
        ///  Get all log records of specified type for specified inventory
        /// </summary>
        /// <param name="entities">Object context</param>
        /// <param name="inventoryId">Inventory ID</param>
        /// <param name="logType">Log type</param>
        /// <param name="loadInvoice">If true, Invoice relation will be loaded</param>
        /// <param name="loadAccountDistributionEntry">If true, AccountDistributionEntry relation will be loaded</param>
        /// <returns>Collection of inventory log records</returns>
        public IEnumerable<InventoryLog> GetInventoryLogs(CompEntities entities, int inventoryId, TermGroup_InventoryLogType logType, bool loadInvoice = false, bool loadAccountDistributionEntry = false)
        {
            var logs = (from i in entities.InventoryLog
                        where i.InventoryId == inventoryId &&
                        i.Type == (int)logType
                        orderby i.Date
                        select i).ToList();

            if (loadInvoice || loadAccountDistributionEntry)
            {
                foreach (var log in logs)
                {
                    if (loadInvoice && !log.InvoiceReference.IsLoaded)
                        log.InvoiceReference.Load();

                    if (loadAccountDistributionEntry && !log.AccountDistributionEntryReference.IsLoaded)
                        log.AccountDistributionEntryReference.Load();
                }
            }

            return logs;
        }

        /// <summary>
        /// Save inventory adjustment.
        /// Will create an inventory log record and an entry in AccountDistributionEntry table
        /// </summary>
        /// <param name="inventoryId">Inventory ID</param>
        /// <param name="type">Adjustment type</param>
        /// <param name="amount">Amount</param>
        /// <param name="date">Date</param>
        /// <param name="accountRowItemsInput">Accounting rows</param>
        /// <param name="invoiceId">Invoice ID</param>
        /// <param name="voucherHeadId">VoucherHead ID</param>
        /// <param name="actorCompanyId">Company ID</param>
        /// <returns>ActionResult</returns>
        public ActionResult SaveAdjustment(int inventoryId, TermGroup_InventoryLogType type, decimal amount, DateTime date, List<AccountingRowDTO> accountRowItemsInput, int? voucherHeadId, int? invoiceId, int actorCompanyId)
        {
            // Default result is successful
            ActionResult result = new ActionResult();

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        #region Prereq

                        // Get company
                        Company company = CompanyManager.GetCompany(entities, actorCompanyId);
                        if (company == null)
                            return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte"));

                        #endregion

                        #region AccountDistributionEntry

                        AccountDistributionEntry entry = new AccountDistributionEntry()
                        {
                            Date = date,

                            //Set references
                            Company = company,

                            //Set FK
                            InventoryId = inventoryId,
                            SupplierInvoiceId = invoiceId,
                        };

                        result = AccountDistributionManager.AddAccountDistributionEntry(entities, entry, accountRowItemsInput);
                        if (!result.Success)
                            return result;

                        #endregion

                        #region InventoryLog

                        InventoryLog inventoryLog = new InventoryLog()
                        {
                            Type = (int)type,
                            Date = date,
                            Amount = amount,

                            //Set FK
                            InventoryId = inventoryId,
                            InvoiceId = invoiceId,
                            AccountDistributionEntryId = result.IntegerValue,
                            VoucherHeadId = voucherHeadId,
                            ActorCompanyId = actorCompanyId,
                            UserId = base.UserId,
                        };

                        //Set currency amounts
                        CountryCurrencyManager.SetCurrencyAmounts(entities, actorCompanyId, inventoryLog);

                        AddInventoryLog(entities, inventoryLog);

                        #endregion

                        result = SaveChanges(entities, transaction);

                        //Commit transaction
                        if (result.Success)
                            transaction.Complete();
                    }
                }
                catch (Exception ex)
                {
                    base.LogError(ex, this.log);
                    result.Exception = ex;
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
        /// Save dispose (sold / discarded)
        /// Will create an inventory log record and a voucher
        /// </summary>
        /// <param name="inventoryId">Inventory ID</param>
        /// <param name="type">Adjustment type</param>
        /// <param name="voucherSeriesTypeId">VoucherSeriesType ID</param>
        /// <param name="amount">Amount</param>
        /// <param name="date">Date</param>
        /// <param name="accountRowItemsInput">Accounting rows</param>        
        /// <param name="actorCompanyId">Company ID</param>
        /// <returns>ActionResult</returns>
        public ActionResult SaveDispose(int inventoryId, TermGroup_InventoryLogType type, int voucherSeriesTypeId, decimal amount, DateTime date, string note, List<AccountingRowDTO> accountRowItemsInput, int actorCompanyId)
        {
            // Default result is successful
            ActionResult result = new ActionResult();

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        #region Prereq

                        // Get company
                        Company company = CompanyManager.GetCompany(entities, actorCompanyId);
                        if (company == null)
                            return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte"));

                        #endregion

                        #region Validation

                        //Validate AccountYear
                        AccountYear accountYear = AccountManager.GetAccountYear(entities, date, actorCompanyId);
                        result = AccountManager.ValidateAccountYear(accountYear);
                        if (!result.Success)
                            return result;

                        //Validate AccountPeriod
                        var period = AccountManager.GetAccountPeriod(entities, accountYear.AccountYearId, date, actorCompanyId);
                        result = AccountManager.ValidateAccountPeriod(period, date);
                        if (!result.Success)
                            return result;

                        #endregion

                        #region Inventory

                        Inventory inventory = GetInventory(entities, inventoryId, actorCompanyId);

                        inventory.Status = type == TermGroup_InventoryLogType.Sold ? (int)TermGroup_InventoryStatus.Sold : type == TermGroup_InventoryLogType.Discarded ? (int)TermGroup_InventoryStatus.Discarded : inventory.Status;
                        inventory.WriteOffRemainingAmount = 0;
                        inventory.WriteOffAmount = 0;

                        #endregion

                        #region Voucher

                        //var accYearId = AccountManager.GetAccountYearId(entities, date, actorCompanyId);
                        //var accPeriod = AccountManager.GetAccountPeriod(entities, accYearId, date, actorCompanyId);
                        var serie = VoucherManager.GetVoucherSerieByType(entities, voucherSeriesTypeId, accountYear.AccountYearId);

                        // Create voucher head
                        VoucherHeadDTO voucherHead = new VoucherHeadDTO()
                        {
                            AccountPeriodId = period.AccountPeriodId,
                            VoucherSeriesId = serie.VoucherSeriesId,
                            VoucherNr = (int)serie.VoucherNrLatest + 1,
                            Date = date,
                            Text = note,
                            Template = false,
                            VatVoucher = false,
                            Status = TermGroup_AccountStatus.Open,
                            ActorCompanyId = actorCompanyId
                        };

                        ActionResult saveVoucherResult = new ActionResult();
                        saveVoucherResult = VoucherManager.SaveVoucher(entities, transaction, voucherHead, accountRowItemsInput, null, null, actorCompanyId, false, useVoucherLock: true);

                        if (!saveVoucherResult.Success)
                            return saveVoucherResult;

                        #endregion

                        #region InventoryLog

                        InventoryLog inventoryLog = new InventoryLog()
                        {
                            Type = (int)type,
                            Date = date,
                            Amount = amount,

                            //Set FK
                            InventoryId = inventoryId,
                            VoucherHeadId = saveVoucherResult.IntegerValue,
                            AccountDistributionEntryId = null,
                            ActorCompanyId = actorCompanyId,
                            UserId = base.UserId,
                        };

                        //Set currency amounts
                        CountryCurrencyManager.SetCurrencyAmounts(entities, actorCompanyId, inventoryLog);

                        AddInventoryLog(entities, inventoryLog);

                        #endregion

						DeletePreliminaryDepreciationEntries(entities, actorCompanyId, inventoryId);

						result = SaveChanges(entities, transaction);

                        //Commit transaction
                        if (result.Success)
                            transaction.Complete();
                    }
                }
                catch (Exception ex)
                {
                    base.LogError(ex, this.log);
                    result.Exception = ex;
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

        #endregion

        #region InventoryWriteOffMethod

        /// <summary>
        /// Get all inventory write off methods for specified company
        /// </summary>
        /// <param name="actorCompanyId">Company ID</param>
        /// <returns>A collection of methods</returns>
        public List<InventoryWriteOffMethod> GetInventoryWriteOffMethods(int actorCompanyId, int? writeOffMethodId = null)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.InventoryWriteOffMethod.NoTracking();
            return GetInventoryWriteOffMethods(entities, actorCompanyId, writeOffMethodId);
        }

        public List<InventoryWriteOffMethod> GetInventoryWriteOffMethods(CompEntities entities, int actorCompanyId, int? writeOffMethodId = null)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            entitiesReadOnly.InventoryWriteOffMethod.NoTracking();
            var query = (from i in entities.InventoryWriteOffMethod
                         where i.ActorCompanyId == actorCompanyId &&
                         i.State == (int)SoeEntityState.Active
                         select i);

            if (writeOffMethodId.HasValue)
                query = query.Where(m => m.InventoryWriteOffMethodId == writeOffMethodId.Value);

            return query.OrderBy(m => m.Name).ToList();
        }

        public Dictionary<int, string> GetInventoryWriteOffMethodsDict(int actorCompanyId, bool addEmptyValue = true)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.InventoryWriteOffMethod.NoTracking();
            var methods = (from i in entities.InventoryWriteOffMethod
                           where i.ActorCompanyId == actorCompanyId &&
                           i.State == (int)SoeEntityState.Active
                           orderby i.Name
                           select i).ToList();

            Dictionary<int, string> dict = new Dictionary<int, string>();

            if (addEmptyValue)
                dict.Add(0, " ");

            foreach (var method in methods)
            {
                dict.Add(method.InventoryWriteOffMethodId, method.Name);
            }

            return dict;
        }

        /// <summary>
        /// Get specified inventory write off method
        /// </summary>
        /// <param name="methodId">InventoryWriteOffMethod ID</param>
        /// <param name="loadInventories">If true, Inventory relation is loaded</param>
        /// <param name="loadTemplates">If true, InventoryWriteOffTemplate relation is loaded</param>
        /// <param name="loadAccountDistributionEntries">If true, Inventory.AccountDistributionEntry records will be loaded</param>
        /// <returns>One method or null if not found</returns>
        public InventoryWriteOffMethod GetInventoryWriteOffMethod(int methodId, bool loadInventories, bool loadTemplates, bool loadAccountDistributionEntries = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.InventoryWriteOffMethod.NoTracking();
            return GetInventoryWriteOffMethod(entities, methodId, loadInventories, loadTemplates, loadAccountDistributionEntries);
        }

        /// <summary>
        /// Get specified inventory write off method
        /// </summary>
        /// <param name="entities">Object context</param>
        /// <param name="methodId">InventoryWriteOffMethod ID</param>
        /// <param name="loadInventories">If true, Inventory relation is loaded</param>
        /// <param name="loadTemplates">If true, InventoryWriteOffTemplate relation is loaded</param>
        /// <param name="loadAccountDistributionEntries">If true, Inventory.AccountDistributionEntry records will be loaded</param>
        /// <returns>One method or null if not found</returns>
        public InventoryWriteOffMethod GetInventoryWriteOffMethod(CompEntities entities, int methodId, bool loadInventories, bool loadTemplates, bool loadAccountDistributionEntries = false)
        {
            var query = (from i in entities.InventoryWriteOffMethod
                         where i.InventoryWriteOffMethodId == methodId
                         select i);

            if (loadAccountDistributionEntries)
            {
                query = query.Include("Inventory.AccountDistributionEntry");
            }

            var method = query.FirstOrDefault();

            // Load relations
            if (method != null)
            {
                if (loadInventories && !method.Inventory.IsLoaded)
                    method.Inventory.Load();
                if (loadTemplates && !method.InventoryWriteOffTemplate.IsLoaded)
                    method.InventoryWriteOffTemplate.Load();
            }

            return method;
        }

        /// <summary>
        /// Check whether user entered Inventory Write off Method 'Name' is already in used.
        /// </summary>
        /// <param name="entities">CompEntities</param>
        /// <param name="name">Inventory Write off method 'Name'</param>
        /// <param name="actorCompanyId">User Company Id</param>
        /// <param name="writeOffMethodId">Id of the already exists Inventory Write off method when updating it.</param>
        /// <returns></returns>
        private bool WriteOffMethodExists(CompEntities entities, string name, int actorCompanyId, int? writeOffMethodId = null)
        {
            var query = from wm in entities.InventoryWriteOffMethod
                        where wm.Company.ActorCompanyId == actorCompanyId && wm.Name == name && wm.State != (int)SoeEntityState.Deleted
                        select wm;
            if (writeOffMethodId.HasValue && writeOffMethodId.Value > 0)
                query = query.Where(wt => wt.InventoryWriteOffMethodId != writeOffMethodId.Value);
            return query.Any();
        }

        /// <summary>
        /// Insert or update inventory write off method
        /// </summary>
        /// <param name="methodInput">InventoryWriteOffMethodDTO to insert or update</param>
        /// <param name="actorCompanyId">Company ID</param>
        /// <returns>ActionResult</returns>
        public ActionResult SaveInventoryWriteOffMethod(InventoryWriteOffMethodDTO methodInput, int actorCompanyId)
        {
            if (methodInput == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "InventoryWriteOffMethod");

            // Default result is successful
            ActionResult result = new ActionResult();

            int methodId = methodInput.InventoryWriteOffMethodId;

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        #region Prereq

                        // Get company
                        Company company = CompanyManager.GetCompany(entities, actorCompanyId);
                        if (company == null)
                            return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte"));

                        #endregion

                        #region InventoryWriteOffMethod

                        // Get existing method
                        InventoryWriteOffMethod method = GetInventoryWriteOffMethod(entities, methodId, false, false);

                        if (WriteOffMethodExists(entities, methodInput.Name, actorCompanyId, method?.InventoryWriteOffMethodId ?? null))
                            return new ActionResult((int)ActionResultSave.EntityExists, string.Format(
                                GetText(92030, (int)TermGroup.General, "{0}: {1} finns redan."),
                                GetText(9177, (int)TermGroup.General, "Namn"),
                                methodInput.Name
                                ));

                        if (method == null)
                        {
                            #region InventoryWriteOffMethod Add

                            method = new InventoryWriteOffMethod()
                            {
                                Company = company,
                                Name = methodInput.Name,
                                Description = methodInput.Description,
                                Type = (int)methodInput.Type,
                                PeriodType = (int)methodInput.PeriodType,
                                PeriodValue = methodInput.PeriodValue,
                                YearPercent = methodInput.YearPercent
                            };

                            SetCreatedProperties(method);

                            entities.InventoryWriteOffMethod.AddObject(method);

                            #endregion
                        }
                        else
                        {
                            #region InventoryWriteOffMethod Update

                            // Update method
                            method.Company = company;
                            method.Name = methodInput.Name;
                            method.Description = methodInput.Description;
                            method.Type = (int)methodInput.Type;
                            method.PeriodType = (int)methodInput.PeriodType;
                            method.PeriodValue = methodInput.PeriodValue;
                            method.YearPercent = methodInput.YearPercent;

                            SetModifiedProperties(method);

                            #endregion
                        }

                        #endregion

                        result = SaveChanges(entities, transaction);
                        if (result.Success)
                        {
                            //Commit transaction
                            transaction.Complete();

                            methodId = method.InventoryWriteOffMethodId;
                        }
                        else
                            result.ErrorNumber = (int)ActionResultSave.InventoryWriteOffMethodNotSaved;
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
                        result.IntegerValue = methodId;
                    }
                    else
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }

                return result;
            }
        }

        /// <summary>
        /// Sets a write off methods state to Deleted
        /// </summary>
        /// <param name="methodId">ID of method to delete</param>
        /// <returns>ActionResult</returns>
        public ActionResult DeleteInventoryWriteOffMethod(int methodId)
        {
            ActionResult result = new ActionResult();

            using (CompEntities entities = new CompEntities())
            {
                // Get write off method
                InventoryWriteOffMethod method = GetInventoryWriteOffMethod(entities, methodId, true, true);
                if (method == null)
                    return new ActionResult((int)ActionResultDelete.EntityIsNull, "InventoryWriteOffMethod");

                // Check relation dependencies
                if (method.Inventory.Count(iw => iw.State == (int)SoeEntityState.Active) > 0)
                    return new ActionResult((int)ActionResultDelete.InventoryWriteOffMethodHasTemplates, GetText(92024, "Borttagning misslyckades.\nPosten används i Avskrivningen"));
                if (method.InventoryWriteOffTemplate.Count(iwt => iwt.State == (int)SoeEntityState.Active) > 0)
                    return new ActionResult((int)ActionResultDelete.InventoryWriteOffMethodHasInventories, GetText(92023, "Borttagning misslyckades.\nPosten används i Avskrivningsmall"));

                // Set the inventory to deleted
                result = ChangeEntityState(entities, method, SoeEntityState.Deleted, true);
            }

            return result;
        }

		#endregion

		#region InventoryWriteOffTemplate

		/// <summary>
		/// Get all inventory write off templates for specified company
		/// </summary>
		/// <param name="actorCompanyId">Company ID</param>
		/// <param name="inventoryWriteOffTemplateId">InventoryWriteOffTemplateId, leave empty or set as null to return all.</param>
		/// <param name="loadAccountingSettings">If true, account settings relations will be loaded. False by default.</param>
		/// <returns>A query to list templates for depreciation.</returns>
		public IEnumerable<InventoryWriteOffTemplate> GetInventoryWriteOffTemplates(int actorCompanyId, int? inventoryWriteOffTemplateId = null, bool loadAccountingSettings = false)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            entitiesReadOnly.InventoryWriteOffTemplate.NoTracking();
            var query = GetInventoryWriteOffTemplates(entitiesReadOnly, actorCompanyId, inventoryWriteOffTemplateId);

			if (loadAccountingSettings)
			{
                query = query
	                .Include("InventoryAccountStd.AccountStd.Account")
	                .Include("InventoryAccountStd.AccountInternal.Account")
                    .Include("InventoryAccountStd.AccountInternal.Account.AccountDim");
			}

            return query;
		}

		/// <summary>
		/// Get all inventory write off templates for specified company
		/// </summary>
		/// <param name="actorCompanyId">Company ID</param>
		/// <param name="inventoryWriteOffTemplateId">InventoryWriteOffTemplateId, leave empty or set as null to return all.</param>
		/// <returns>A query to list templates for depreciation.</returns>
		public IQueryable<InventoryWriteOffTemplate> GetInventoryWriteOffTemplates(CompEntities entities, int actorCompanyId, int? inventoryWriteOffTemplateId = null)
        {
            IQueryable<InventoryWriteOffTemplate> query = (from i in entities.InventoryWriteOffTemplate
                                                           where i.ActorCompanyId == actorCompanyId &&
                                                           i.State == (int)SoeEntityState.Active
                                                           orderby i.Name
                                                           select i);

            if (inventoryWriteOffTemplateId != null)
                query = query.Where(t => t.InventoryWriteOffTemplateId == inventoryWriteOffTemplateId);

            return query;
        }

        public Dictionary<int, string> GetInventoryWriteOffTemplatesDict(int actorCompanyId, bool addEmptyValue = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.InventoryWriteOffTemplate.NoTracking();
            var templates = (from i in entities.InventoryWriteOffTemplate
                             where i.ActorCompanyId == actorCompanyId &&
                             i.State == (int)SoeEntityState.Active
                             orderby i.Name
                             select i).ToList();


            Dictionary<int, string> dict = new Dictionary<int, string>();

            if (addEmptyValue)
                dict.Add(0, " ");

            foreach (var template in templates)
            {
                dict.Add(template.InventoryWriteOffTemplateId, template.Name);
            }

            return dict;
        }

        /// <summary>
        /// Get specified inventory write off template
        /// </summary>
        /// <param name="templateId">InventoryWriteOffTemplate ID</param>
        /// <param name="loadAccountSettings">If true, account settings relations will be loaded</param>
        /// <returns>One template or null if not found</returns>
        public InventoryWriteOffTemplate GetInventoryWriteOffTemplate(int templateId, bool loadAccountSettings)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.InventoryWriteOffTemplate.NoTracking();
            return GetInventoryWriteOffTemplate(entities, templateId, loadAccountSettings);
        }

        /// <summary>
        /// Get specified inventory write off template
        /// </summary>
        /// <param name="entities">Object context</param>
        /// <param name="templateId">InventoryWriteOffTemplate ID</param>
        /// <param name="loadAccountSettings">If true, account settings relations will be loaded</param>
        /// <returns>One template or null if not found</returns>
        public InventoryWriteOffTemplate GetInventoryWriteOffTemplate(CompEntities entities, int templateId, bool loadAccountSettings)
        {
            var template = (from i in entities.InventoryWriteOffTemplate
                            where i.InventoryWriteOffTemplateId == templateId
                            select i).FirstOrDefault();

            // Load relations
            if (template != null && loadAccountSettings)
            {
                if (!template.InventoryAccountStd.IsLoaded)
                    template.InventoryAccountStd.Load();

                foreach (var accountStd in template.InventoryAccountStd)
                {
                    // Standard account
                    if (!accountStd.AccountStdReference.IsLoaded)
                        accountStd.AccountStdReference.Load();
                    if (accountStd.AccountStd != null && !accountStd.AccountStd.AccountReference.IsLoaded)
                        accountStd.AccountStd.AccountReference.Load();

                    // Internal accounts
                    if (!accountStd.AccountInternal.IsLoaded)
                        accountStd.AccountInternal.Load();

                    foreach (var accountInt in accountStd.AccountInternal)
                    {
                        if (!accountInt.AccountReference.IsLoaded)
                            accountInt.AccountReference.Load();
                        if (accountInt.Account != null && !accountInt.Account.AccountDimReference.IsLoaded)
                            accountInt.Account.AccountDimReference.Load();
                    }
                }
            }

            return template;
        }

        /// <summary>
        /// Check whether user entered Inventory Write off template 'Name' is already in used.
        /// </summary>
        /// <param entities="entities">CompEntities</param>
        /// <param name="name">Inventory Write off template 'Name'</param>
        /// <param name="actorCompanyId">Actor COmpany Id</param>
        /// <param name="writeOffTemplateId">Id of the already exists Inventory Write off template when updating it.</param>
        /// <returns></returns>
        private bool WriteOffTemplateExists(CompEntities entities, string name, int actorCompanyId, int? writeOffTemplateId = null)
        {
            var query = from wt in entities.InventoryWriteOffTemplate
                        where wt.Company.ActorCompanyId == actorCompanyId && wt.Name == name && wt.State == (int)SoeEntityState.Active
                        select wt;
            if (writeOffTemplateId.HasValue)
                query = query.Where(wt => wt.InventoryWriteOffTemplateId != writeOffTemplateId.Value);
            return query.Any();
        }

        /// <summary>
        /// Insert or update inventory write off template
        /// </summary>
        /// <param name="templateInput">InventoryWriteOffTemplate entity to insert or update</param>
        /// <param name="actorCompanyId">Company ID</param>
        /// <returns>ActionResult</returns>
        public ActionResult SaveInventoryWriteOffTemplate(InventoryWriteOffTemplateDTO templateInput, List<AccountingSettingDTO> accountSettings, List<AccountingSettingDTO> accountSettingsAngular, int actorCompanyId)
        {
            if (templateInput == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "InventoryWriteOffTemplate");

            // Default result is successful
            ActionResult result = new ActionResult();

            int templateId = templateInput.InventoryWriteOffTemplateId;

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        #region Prereq

                        // Get company
                        Company company = CompanyManager.GetCompany(entities, actorCompanyId);
                        if (company == null)
                            return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte"));

                        #endregion

                        #region InventoryWriteOffTemplate

                        // Get existing method
                        InventoryWriteOffTemplate template = GetInventoryWriteOffTemplate(entities, templateId, true);

                        if (WriteOffTemplateExists(entities, templateInput.Name, actorCompanyId, template?.InventoryWriteOffTemplateId ?? null))
                            return new ActionResult((int)ActionResultSave.EntityExists, string.Format(
                                GetText(92030, (int)TermGroup.General, "{0}: {1} finns redan."),
                                GetText(9177, (int)TermGroup.General, "Namn"),
                                templateInput.Name
                                ));

                        if (template == null)
                        {
                            #region InventoryWriteOffMethod Add

                            template = new InventoryWriteOffTemplate()
                            {
                                Name = templateInput.Name,
                                Description = templateInput.Description,

                                //Set references
                                Company = company,

                                //Set FK
                                InventoryWriteOffMethodId = templateInput.InventoryWriteOffMethodId,
                                VoucherSeriesTypeId = templateInput.VoucherSeriesTypeId,
                            };
                            SetCreatedProperties(template);
                            entities.InventoryWriteOffTemplate.AddObject(template);

                            #endregion

                            #region Accounts

                            if (accountSettings != null)
                                AddInventoryAccounts(entities, actorCompanyId, null, template, accountSettings);
                            else if (accountSettingsAngular != null)
                                SetInventoryAccounts(entities, ActorCompanyId, null, template, accountSettingsAngular);

                            #endregion
                        }
                        else
                        {
                            #region InventoryWriteOffTemplate Update

                            // Update template
                            template.Name = templateInput.Name;
                            template.Description = templateInput.Description;

                            //Set references
                            template.Company = company;

                            //Set FK
                            template.InventoryWriteOffMethodId = templateInput.InventoryWriteOffMethodId;
                            template.VoucherSeriesTypeId = templateInput.VoucherSeriesTypeId;
                            SetModifiedProperties(template);

                            #endregion

                            #region Accounts

                            if (accountSettings != null)
                                UpdateInventoryAccounts(entities, actorCompanyId, null, template, accountSettings);
                            else if (accountSettingsAngular != null)
                                SetInventoryAccounts(entities, ActorCompanyId, null, template, accountSettingsAngular);

                            #endregion
                        }

                        #endregion

                        result = SaveChanges(entities, transaction);
                        if (result.Success)
                        {
                            //Commit transaction
                            transaction.Complete();

                            templateId = template.InventoryWriteOffTemplateId;
                        }
                        else
                            result.ErrorNumber = (int)ActionResultSave.InventoryWriteOffTemplateNotSaved;
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
                        result.IntegerValue = templateId;
                    }
                    else
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }

                return result;
            }
        }

        /// <summary>
        /// Sets a write off templates state to Deleted
        /// </summary>
        /// <param name="templateId">ID of template to delete</param>
        /// <returns>ActionResult</returns>
        public ActionResult DeleteInventoryWriteOffTemplate(int templateId)
        {
            ActionResult result = new ActionResult();

            using (CompEntities entities = new CompEntities())
            {
                // Get write off template
                InventoryWriteOffTemplate template = GetInventoryWriteOffTemplate(entities, templateId, false);
                if (template == null)
                    return new ActionResult((int)ActionResultDelete.EntityIsNull, "InventoryWriteOffTemplate");

                // Set the inventory to deleted
                result = ChangeEntityState(entities, template, SoeEntityState.Deleted, true);
            }

            return result;
        }

        #endregion

        #region AccountDistributionEntry

        public IEnumerable<AccountDistributionEntriesView> GetAccountDistributionEntries(int actorCompanyId, DateTime periodDate)
        {
            DateTime endDate = new DateTime(periodDate.AddMonths(1).Year, periodDate.AddMonths(1).Month, 1);
            DateTime startDate = new DateTime(periodDate.Year, periodDate.Month, 1);

            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.VoucherHead.NoTracking();
            return GetAccountDistributionEntries(entities, actorCompanyId, startDate, endDate);
        }

        public IEnumerable<AccountDistributionEntriesView> GetAccountDistributionEntries(CompEntities entities, int actorCompanyId, DateTime periodDateStart, DateTime periodDateEnd)
        {

            return (from ade in entities.AccountDistributionEntriesView
                    where ade.ActorCompanyId == actorCompanyId &&
                    ade.Date < periodDateEnd &&
                    ade.Date >= periodDateStart
                    select ade).ToList();

            /*
            return (from ade in entities.AccountDistributionEntry
                        .Include("AccountDistributionEntryRow.AccountInternal.Account.AccountDim")
                        .Include("AccountDistributionEntryRow.AccountStd.Account.AccountDim")
                        .Include("Inventory").Include("InventoryLog")
                        .Include("VoucherHead.AccountPeriod")
                    where ade.ActorCompanyId == actorCompanyId &&
                    ade.Date < periodDateEnd &&
                    ade.Date >= periodDateStart &&
                    ade.State !=(int)SoeEntityState.Deleted &&
                    ade.InventoryId != null
                    select ade).ToList();

            */
        }

        private void DeletePreliminaryDepreciationEntries(CompEntities entities, int actorCompanyId, int? inventoryId)
        {
            bool deleteAllDepreciations = inventoryId is null;
			var preliminaryDepreciationsList = entities.AccountDistributionEntry
                .Where(w =>
                    w.ActorCompanyId == actorCompanyId &&
                    // Target preliminary
                    w.VoucherHeadId == null &&
                    (
                        // Target depreciations
                        w.InventoryId == inventoryId || 
                        w.InventoryId != null && deleteAllDepreciations
                    ) &&
                    w.State == (int)SoeEntityState.Active
				)
                .ToList();

            foreach(var entry in preliminaryDepreciationsList)
			{
			    AccountDistributionManager.DeleteAccountDistributionEntry(entities, entry, saveChanges: false);
			}
        }

		private List<AccountDistributionEntryRow> GetAccountDistributionEntryRow(IEnumerable<AccountDistributionEntriesView> accountDistributionEntries)
        {
            var idList = accountDistributionEntries.Select(x => x.AccountDistributionEntryId).ToList();
            using (CompEntities entities = new CompEntities())
            {
                return (from entry in entities.AccountDistributionEntryRow
                        .Include("AccountStd.Account.AccountDim")
                        .Include("AccountInternal.Account.AccountDim")
                        where idList.Contains(entry.AccountDistributionEntryId)
                        select entry).ToList();
            }
        }

        public List<GetAccountDistributionEntrySum_Result> GetAccountDistributionTotals(int actorCompanyId, DateTime yearStart, DateTime periodStart, DateTime periodEnd)
        {
            using (var entities = new CompEntities())
            {
                entities.CommandTimeout = 300;

                return entities.GetAccountDistributionEntrySum(actorCompanyId, yearStart, periodStart, periodEnd).ToList();
            }
        }

        public decimal GetAccountDistributionWriteOffTotal(List<GetAccountDistributionEntrySum_Result> totalsList, int inventoryId)
        {
            var list = totalsList.Where(x => x.inventoryId == inventoryId && x.WriteOffTotal > 0);

            decimal value = 0;

            if (list.Any(a => a.InventoryLogType == (int)TermGroup_InventoryLogType.WriteOff || a.InventoryLogType == (int)TermGroup_InventoryLogType.WriteDown))
                value = list.Where(a => a.InventoryLogType == (int)TermGroup_InventoryLogType.WriteOff || a.InventoryLogType == (int)TermGroup_InventoryLogType.WriteDown).Sum(a => a.WriteOffTotal);

            //Dra bort uppskrivningar
            if (list.Any(a => a.InventoryLogType == (int)TermGroup_InventoryLogType.WriteUp))
                value -= list.Where(a => a.InventoryLogType == (int)TermGroup_InventoryLogType.WriteUp).Sum(a => a.WriteOffTotal);

            return value;
        }

        private decimal GetAccountDistributionWriteOffYear(List<GetAccountDistributionEntrySum_Result> totalsList, int inventoryId)
        {
            var list = totalsList.Where(x => x.inventoryId == inventoryId && x.WriteOffYear > 0);

            decimal value = 0;

            if (list.Any(a => a.InventoryLogType == (int)TermGroup_InventoryLogType.WriteOff || a.InventoryLogType == (int)TermGroup_InventoryLogType.WriteDown))
                value = list.Where(a => a.InventoryLogType == (int)TermGroup_InventoryLogType.WriteOff || a.InventoryLogType == (int)TermGroup_InventoryLogType.WriteDown).Sum(a => a.WriteOffYear);

            //Dra bort uppskrivningar
            if (list.Any(a => a.InventoryLogType == (int)TermGroup_InventoryLogType.WriteUp))
                value -= list.Where(a => a.InventoryLogType == (int)TermGroup_InventoryLogType.WriteUp).Sum(a => a.WriteOffYear);

            return value;
        }

        public List<AccountDistributionEntryDTO> GetAccountDistributionEntriesDTO(int actorCompanyId, DateTime periodDate, SoeAccountDistributionType accountDistributionType)
        {
            List<AccountDistributionEntryDTO> dtos = new List<AccountDistributionEntryDTO>();
            List<CompanyCategoryRecord> categoryRecordsForCompany = CategoryManager.GetCompanyCategoryRecords(SoeCategoryType.Inventory, actorCompanyId);

            int accountYearId = 0;
            bool yearIsOpen = false;
            DateTime yearStart;


            var accountYear = AccountManager.GetAccountYear(periodDate, actorCompanyId);
            if (accountYear != null)
            {
                AccountManager.GetAccountYearInfo(accountYear, out accountYearId, out yearIsOpen);
                yearStart = new DateTime(periodDate.Year, accountYear.From.Month, 1);
            }
            else
            {
                yearStart = CalendarUtility.GetFirstDateOfYear(periodDate);
            }

            if (yearStart > periodDate)
                yearStart = yearStart.AddYears(-1);

            DateTime toDate = new DateTime(periodDate.AddMonths(1).Year, periodDate.AddMonths(1).Month, 1);
            int rowId = 1;

            var accountDistributionEntrys = GetAccountDistributionEntries(actorCompanyId, periodDate);
            var accountDistributionTotals = GetAccountDistributionTotals(actorCompanyId, yearStart, periodDate, toDate);
            var accountDistributionEntryRows = GetAccountDistributionEntryRow(accountDistributionEntrys);

            // Get previous period
            var previousAccountDistributionEntrys = GetAccountDistributionEntries(actorCompanyId, periodDate.AddMonths(-1));

            foreach (AccountDistributionEntriesView entry in accountDistributionEntrys)
            {
                decimal amount = 0;
                decimal totalOffWrite = GetAccountDistributionWriteOffTotal(accountDistributionTotals, entry.InventoryId);

                var dto = new AccountDistributionEntryDTO
                {
                    AccountDistributionEntryId = entry.AccountDistributionEntryId,
                    ActorCompanyId = entry.ActorCompanyId,
                    VoucherSeriesTypeId = entry.VoucherSeriesTypeId,
                    TriggerType = (TermGroup_AccountDistributionTriggerType)entry.TriggerType,
                    Date = entry.Date,
                    VoucherHeadId = entry.VoucherHeadId,
                    Created = entry.Created,
                    CreatedBy = entry.CreatedBy,
                    Modified = entry.Modified,
                    ModifiedBy = entry.ModifiedBy,
                    DetailVisible = false,
                    InventoryName = entry.InventoryName,
                    InventoryNr = entry.InventoryNr,
                    RowId = rowId,
                    AccountDistributionEntryRowDTO = new List<AccountDistributionEntryRowDTO>(),
                    WriteOffYear = GetAccountDistributionWriteOffYear(accountDistributionTotals, entry.InventoryId),
                    WriteOffTotal = totalOffWrite + entry.WriteOffSum,
                    WriteOffAmount = entry.WriteOffAmount,
                    WriteOffSum = entry.WriteOffSum,
                    CurrentAmount = entry.WriteOffAmount - entry.WriteOffSum - totalOffWrite,
                    PeriodType = TermGroup_AccountDistributionPeriodType.Period,
                    PeriodError = false,
                    InventoryPurchaseDate = entry.PurchaseDate,
                    InventoryWriteOffDate = entry.WriteOffDate,
                    InventoryDescription = entry.Description,
                    InventoryNotes = entry.Notes,
                    InventoryId = entry.InventoryId,
                    IsReversal = false,
				};

                if (entry.VoucherHeadId == null)
                {
                    dto.Status = GetText(8058, "Preliminär");
                    dto.IsSelectEnable = true;

                    // Pre period check
                    if (dto.WriteOffTotal != dto.WriteOffSum)
                    {
                        var preItem = previousAccountDistributionEntrys.FirstOrDefault(i => i.InventoryId == entry.InventoryId &&
                            i.InventoryLogType != (int)TermGroup_InventoryLogType.Reversed &&
                            i.InventoryLogType != (int)TermGroup_InventoryLogType.Reversal);
                        dto.PeriodError = preItem == null || preItem.VoucherHeadId == null;
                    }
                }
                else
                {
                    dto.IsSelectEnable = false;
                    dto.Status = GetText(5705, "Verifikat");
                    dto.VoucherNr = entry.VoucherNr;
                    dto.AccountYearId = entry.AccountYearId;
                }

                if (!yearIsOpen)
                    dto.IsSelectEnable = false;

                if (entry.InventoryLogType == 0)
                {
                    dto.TypeName = GetText(7106, "Typ saknas");
                }
                else
                {
                    switch (entry.InventoryLogType)
                    {
                        case (int)TermGroup_InventoryLogType.Purchase:
                        case (int)TermGroup_InventoryLogType.WriteOff:
                        case (int)TermGroup_InventoryLogType.OverWriteOff:
                        case (int)TermGroup_InventoryLogType.UnderWriteOff:
                        case (int)TermGroup_InventoryLogType.WriteUp:
                        case (int)TermGroup_InventoryLogType.WriteDown:
                        case (int)TermGroup_InventoryLogType.Discarded:
                        case (int)TermGroup_InventoryLogType.Sold:
                        case (int)TermGroup_InventoryLogType.Reversal:
                        case (int)TermGroup_InventoryLogType.Reversed:
                            dto.TypeName = GetText(entry.InventoryLogType, (int)TermGroup.InventoryLogType);
                            break;
                        default:
                            dto.TypeName = GetText(7107, "Ogiltig typ"); //Ska aldrig kunna hända
                            break;
                    }
                    if (entry.InventoryLogType == (int)TermGroup_InventoryLogType.Reversal || 
                        entry.InventoryLogType == (int)TermGroup_InventoryLogType.Reversed)
                    { 
						dto.IsReversal = true;
					}
				}

                //TODO: kolla spårningen här, hur borde det skickas med? Bara som en textsträng eller ska det kunna bli en länk?
                //var accountDistributionEntryRows = GetAccountDistributionEntryRow(entry.AccountDistributionEntryId);
                foreach (AccountDistributionEntryRow entryRow in accountDistributionEntryRows.Where(r => r.AccountDistributionEntryId == entry.AccountDistributionEntryId))
                {
                    var dtoRow = new AccountDistributionEntryRowDTO
                    {
                        AccountDistributionEntryId = entry.AccountDistributionEntryId,
                        AccountDistributionEntryRowId = entryRow.AccountDistributionEntryRowId,
                        SameBalance = entryRow.DebitAmount,
                        OppositeBalance = entryRow.CreditAmount
                    };

                    amount += entryRow.DebitAmount;

                    AccountStd accountStd = entryRow.AccountStd;
                    if (accountStd != null)
                    {
                        dtoRow.Dim1Id = accountStd.AccountId;
                        dtoRow.Dim1Nr = accountStd.Account.AccountNr;
                        dtoRow.Dim1Name = accountStd.Account.Name;
                        dtoRow.Dim1DimName = accountStd.Account.AccountDim.Name;
                    }

                    foreach (AccountInternal accountInternal in entryRow.AccountInternal)
                    {
                        switch (accountInternal.Account.AccountDim.AccountDimNr)
                        {
                            case 2:
                                dtoRow.Dim2Id = accountInternal.AccountId;
                                dtoRow.Dim2Nr = accountInternal.Account.AccountNr;
                                dtoRow.Dim2Name = accountInternal.Account.Name;
                                dtoRow.Dim2DimName = accountInternal.Account.AccountDim.Name;
                                break;
                            case 3:
                                dtoRow.Dim3Id = accountInternal.AccountId;
                                dtoRow.Dim3Nr = accountInternal.Account.AccountNr;
                                dtoRow.Dim3Name = accountInternal.Account.Name;
                                dtoRow.Dim3DimName = accountInternal.Account.AccountDim.Name;
                                break;
                            case 4:
                                dtoRow.Dim4Id = accountInternal.AccountId;
                                dtoRow.Dim4Nr = accountInternal.Account.AccountNr;
                                dtoRow.Dim4Name = accountInternal.Account.Name;
                                dtoRow.Dim4DimName = accountInternal.Account.AccountDim.Name;
                                break;
                            case 5:
                                dtoRow.Dim5Id = accountInternal.AccountId;
                                dtoRow.Dim5Nr = accountInternal.Account.AccountNr;
                                dtoRow.Dim5Name = accountInternal.Account.Name;
                                dtoRow.Dim5DimName = accountInternal.Account.AccountDim.Name;
                                break;
                            case 6:
                                dtoRow.Dim6Id = accountInternal.AccountId;
                                dtoRow.Dim6Nr = accountInternal.Account.AccountNr;
                                dtoRow.Dim6Name = accountInternal.Account.Name;
                                dtoRow.Dim6DimName = accountInternal.Account.AccountDim.Name;
                                break;
                        }
                    }

                    dto.AccountDistributionEntryRowDTO.Add(dtoRow);
                }

                dto.Amount = amount;

                //categories
                if (categoryRecordsForCompany != null)
                {
                    List<string> categoryNames = new List<string>();
                    foreach (CompanyCategoryRecord ccr in categoryRecordsForCompany.GetCategoryRecords(SoeCategoryRecordEntity.Inventory, entry.InventoryId, date: null, discardDateIfEmpty: true))
                    {
                        categoryNames.Add(ccr.Category.Name);
                    }
                    dto.Categories = StringUtility.GetCommaSeparatedString(categoryNames);
                }

                dtos.Add(dto);
                rowId++;
            }

            return dtos;
        }

        public ActionResult TransferToAccountDistributionEntry(int actorCompanyId, DateTime periodDate)
        {
            ActionResult result = new ActionResult();

            using (CompEntities entities = new CompEntities())
            {
                entities.CommandTimeout = 300;

                #region Do delete

                entities.DeleteAccountDistributionEntries(actorCompanyId);

                #endregion

                #region Prereq

                Company company = CompanyManager.GetCompany(entities, actorCompanyId);
                List<Inventory> inventories = GetInventories(entities, actorCompanyId, true);

                // Get internal accounts (Dim2-6)
                List<AccountInternal> accountInternals = AccountManager.GetAccountInternals(entities, actorCompanyId, null);

                int defaultInvWriteOffAssetAccount = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.AccountInventoryAccWriteOff, 0, actorCompanyId, 0);
                int defaultInvWriteOffCostAccount = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.AccountInventoryWriteOff, 0, actorCompanyId, 0);

                if (defaultInvWriteOffAssetAccount == 0)
                {
                    result.Success = false;
                    result.ErrorMessage = GetText(7105, "Inget avskrivningsbaskonto hittades");
                    return result;
                }

                #endregion

                DateTime nextPeriod = new DateTime(periodDate.Date.AddMonths(1).Year, periodDate.Date.AddMonths(1).Month, 1);

                List<AccountYear> accountYears = AccountManager.GetAccountYears(entities, actorCompanyId, false, false);
                var accountStdList = AccountManager.GetAccountStdsByCompany(entities, actorCompanyId, true, true, true);
                //var currencyRates = CountryCurrencyManager.GetCompCurrencyRates(entities, actorCompanyId); For future implementation of currency..

                AccountStd accountStdDebit = null;
                AccountStd creditStdDebit = null;

                foreach (Inventory inventory in inventories)
                {
                    #region Inventory

                    // If inventory is sold, discarded or written off..
                    if (inventory.WriteOffRemainingAmount == 0)
                        continue;
                    // If there's no method for depriciation..
                    if (inventory.InventoryWriteOffMethod.YearPercent == 0 && inventory.InventoryWriteOffMethod.PeriodValue == 0)
                        continue;

                    decimal sum = inventory.AccountDistributionEntry.Where(c => c.InventoryLog.Any() && (c.InventoryLog.First().Type == (int)TermGroup_InventoryLogType.WriteOff || c.InventoryLog.First().Type == (int)TermGroup_InventoryLogType.WriteDown)).Sum(a => a.AccountDistributionEntryRow.Sum(b => b.DebitAmount)); //Alla avskrivningar
                    sum -= inventory.AccountDistributionEntry.Where(c => c.InventoryLog.Any() && c.InventoryLog.First().Type == (int)TermGroup_InventoryLogType.WriteUp).Sum(a => a.AccountDistributionEntryRow.Sum(b => b.DebitAmount));

                    // Total amount to depriciate (write off).
                    decimal writeOffValue = inventory.WriteOffAmount - inventory.WriteOffSum;

                    // If there's nothing left to write off..
                    if (writeOffValue - sum == 0)
                        continue;

                    if (inventory.WriteOffDate == null)
                    {
                        // This should never happend, as write off date is mandatory for inventories.
                        result.Success = false;
                        result.ErrorMessage = GetText(7104, "Alla inventarier kunde inte bearbetas. Avskrivningsdatum saknas för") + " " + inventory.Name;
                        continue;
                    }

                    #region Set Debit Account
                    //If debit/cost account for depreciation exist on Inventory we use that, otherwise we use company default.
                    int debitAccountId = 0;
                    var accountInternalDebit = new System.Data.Entity.Core.Objects.DataClasses.EntityCollection<AccountInternal>();

                    InventoryAccountStd inventoryAccountStandardWriteOff = inventory
                        .InventoryAccountStd
                        .FirstOrDefault(a => a.Type == (int)InventoryAccountType.WriteOff);
                    if (inventoryAccountStandardWriteOff?.AccountId.HasValue == true)
                    {
                        
                        debitAccountId = inventoryAccountStandardWriteOff.AccountId.Value;
                        accountInternalDebit = inventoryAccountStandardWriteOff.AccountInternal;
                    }
                    else
                    {
                        debitAccountId = defaultInvWriteOffCostAccount;
                    }
                    accountStdDebit = accountStdDebit == null || accountStdDebit.AccountId != debitAccountId ? accountStdList.FirstOrDefault(w => w.AccountId == debitAccountId) : accountStdDebit;
                    if (accountStdDebit == null)
                        return new ActionResult(inventory.InventoryNr + "-" + inventory.Name + " (" + GetText(3492, "Avskrivning") + " : " + GetText(1481, "Konto hittades inte") + ")");
                    #endregion

                    #region Set Credit Account
                    //If credit/asset account for depreciation exist on Inventory we use that one, otherwise we use company default.
                    int creditAccountId = 0;
                    var accountInternalCredit = new System.Data.Entity.Core.Objects.DataClasses.EntityCollection<AccountInternal>();

                    InventoryAccountStd inventoryAccountStandardAccWriteOff = inventory
                        .InventoryAccountStd
                        .FirstOrDefault(a => a.Type == (int)InventoryAccountType.AccWriteOff);

                    if (inventoryAccountStandardAccWriteOff?.AccountId.HasValue == true)
                    {

                        creditAccountId = inventoryAccountStandardAccWriteOff.AccountId.Value;
                        accountInternalCredit = inventoryAccountStandardAccWriteOff.AccountInternal;
                    }
                    else
                    {
                        creditAccountId = defaultInvWriteOffAssetAccount;
                    }
                    creditStdDebit = creditStdDebit == null || creditStdDebit.AccountId != creditAccountId ? accountStdList.FirstOrDefault(w => w.AccountId == creditAccountId) : creditStdDebit;
                    if (creditStdDebit == null)
                        return new ActionResult(inventory.InventoryNr + "-" + inventory.Name + " ( " + GetText(3491, "Ackumulerad avskrivning") + " : " + GetText(1481, "Konto hittades inte") + ")");

                    #endregion

                    var inventoryEntries = new List<AccountDistributionEntry>();

                    DateTime iPeriod = ((DateTime)inventory.WriteOffDate).AddMonths(inventory.WriteOffPeriods);
                    while (iPeriod < nextPeriod)
                    {
                        // If write off method period is Year and iPeriod is not the one where the write off should be.
                        if (inventory.InventoryWriteOffMethod.PeriodType == (int)TermGroup_InventoryWriteOffMethodPeriodType.Year &&
                            iPeriod.Month != ((DateTime)inventory.WriteOffDate).Month)
                        {
                            iPeriod = iPeriod.AddMonths(1);
                            continue;
                        }

                        //Get accounting year
                        AccountYear accountYear = accountYears.FirstOrDefault(a => a.From.Date <= iPeriod.AddSeconds(-1).Date && a.To.Date >= iPeriod.AddSeconds(-1).Date);
                        if (accountYear is null)
                        {
                            iPeriod = iPeriod.AddMonths(1);
                            continue;
                        }

                        //Set period start and stop.
                        DateTime periodStart = new DateTime(iPeriod.Year, iPeriod.Month, 1);
                        DateTime periodStop = periodStart.AddMonths(1);

                        //Check if there's a write-off for this month already. Previous delete may have failed, or the transaction may have become a voucher.
                        bool writeOffExist = inventory.AccountDistributionEntry.Any(c =>
                            c.InventoryLog.Any() &&
                            c.InventoryLog.First().Type == (int)TermGroup_InventoryLogType.WriteOff &&
                            c.Date >= periodStart &&
                            c.Date < periodStop);
                        if (writeOffExist)
                        {
                            iPeriod = iPeriod.AddMonths(1);
                            continue;
                        }

                        //Re-calculate for current period.
                        sum = inventory.AccountDistributionEntry.Where(
                            c => c.InventoryLog.Any() &&
                            c.State == (int)SoeEntityState.Active &&
                            (c.InventoryLog.First().Type == (int)TermGroup_InventoryLogType.WriteOff ||
                             c.InventoryLog.First().Type == (int)TermGroup_InventoryLogType.WriteDown)
                        ).Sum(a => a.AccountDistributionEntryRow.Sum(b => b.DebitAmount)); //All write-offs

                        if (inventory.AccountDistributionEntry.Any(c =>
                            c.State == (int)SoeEntityState.Active &&
                            c.InventoryLog.Any() &&
                            c.InventoryLog.First().Type == (int)TermGroup_InventoryLogType.WriteUp)
                        )
                            sum = sum - inventory.AccountDistributionEntry.Where(c =>
                                c.State == (int)SoeEntityState.Active &&
                                c.InventoryLog.Any() &&
                                c.InventoryLog.First().Type == (int)TermGroup_InventoryLogType.WriteUp
                            ).Sum(a => a.AccountDistributionEntryRow.Sum(b => b.DebitAmount));

                        decimal periodWriteOffAmount = 0;
                        int year = 0;
                        decimal yearInAmount = 0;
                        switch (inventory.InventoryWriteOffMethod.Type)
                        {
                            case (int)TermGroup_InventoryWriteOffMethodType.AccordingToTheBooks_MainRule:
                                #region AccordingToTheBooks_MainRule

                                //kontrollera vad året börjar på, så vi kan räkna ut hur mycket vi ska skriva av varje månad
                                decimal WriteOffThisYear = 0;

                                if (year != periodStart.Year)
                                {
                                    year = periodStart.Year;

                                    yearInAmount = inventory.AccountDistributionEntry.Where(c =>
                                        c.InventoryLog.Any() &&
                                        (c.InventoryLog.First().Type == (int)TermGroup_InventoryLogType.WriteOff || c.InventoryLog.First().Type == (int)TermGroup_InventoryLogType.WriteDown) &&
                                        c.Date < accountYear.From
                                    ).Sum(a => a.AccountDistributionEntryRow.Sum(b => b.DebitAmount)); //Alla avskrivningar

                                    if (inventory.AccountDistributionEntry.Any(c =>
                                            c.State == (int)SoeEntityState.Active &&
                                            c.InventoryLog.Any() &&
                                            c.InventoryLog.First().Type == (int)TermGroup_InventoryLogType.WriteUp &&
                                            c.Date < accountYear.From))
                                        yearInAmount = yearInAmount - inventory.AccountDistributionEntry.Where(c => c.
                                            InventoryLog.Any() && 
                                            c.State == (int)SoeEntityState.Active &&
                                            c.InventoryLog.First().Type == (int)TermGroup_InventoryLogType.WriteUp &&
                                            c.Date < accountYear.From
                                        ).Sum(a => a.AccountDistributionEntryRow.Sum(b => b.DebitAmount));
                                }

                                //Summan som ska skrivas av utslaget på året
                                WriteOffThisYear = Decimal.Multiply((writeOffValue - yearInAmount), inventory.InventoryWriteOffMethod.YearPercent / 100);

                                if (inventory.InventoryWriteOffMethod.PeriodType == (int)TermGroup_InventoryWriteOffMethodPeriodType.Year)
                                    periodWriteOffAmount = WriteOffThisYear;
                                else
                                    periodWriteOffAmount = WriteOffThisYear / 12;

                                #endregion
                                break;
                            case (int)TermGroup_InventoryWriteOffMethodType.AccordingToTheBooks_ComplementaryRule:
                                #region AccordingToTheBooks_ComplementaryRule

                                //Kontrollera hur många perioder det är kvar som summan ska slås ut på
                                //take account entries with preliminary status (not showing in writeoffperiods yet)                                                                                    
                                int preliminaryEntries = inventory.AccountDistributionEntry.Count(c =>
                                    c.VoucherHeadId == null &&
                                    c.State == (int)SoeEntityState.Active &&
                                    c.InventoryLog.Any() &&
                                    (c.InventoryLog.First().Type == (int)TermGroup_InventoryLogType.WriteOff || c.InventoryLog.First().Type == (int)TermGroup_InventoryLogType.WriteDown));

                                int periodsLeft = inventory.InventoryWriteOffMethod.PeriodValue - (inventory.WriteOffPeriods + preliminaryEntries);
                                if (periodsLeft > 0)
                                {
                                    //Summan som är kvar att skrivas av
                                    decimal sumLeft = writeOffValue - sum;

                                    //Dela summan på antalet perioder
                                    periodWriteOffAmount = sumLeft / periodsLeft;
                                }

                                #endregion
                                break;
                        }

                        //If less than one of currency left to write off, write off everthing. Detta för att motverka att huvudregeln kör i all evighet, och för att eventuella ören ska komma med sista avskrivningen på kompletteringsregeln
                        if ((writeOffValue - sum) - 1 < periodWriteOffAmount)
                            periodWriteOffAmount = writeOffValue - sum;

                        #region WriteOff

                        if (periodWriteOffAmount == 0)
                        {
                            //If there's nothing left to write off, go to next inventory.
                            iPeriod = nextPeriod;
                            continue;
                        }

                        var entry = new AccountDistributionEntry
                        {
                            Date = iPeriod,

                            //Set references
                            Inventory = inventory,
                            Company = company,
                        };

                        #region Debit Row
                        var debitEntryRow = new AccountDistributionEntryRow
                        {
                            DebitAmount = periodWriteOffAmount,
                            DebitAmountEntCurrency = periodWriteOffAmount,
                            CreditAmount = 0,

                            //Set references
                            AccountDistributionEntry = entry,
                            AccountStd = accountStdDebit,
                        };
                        foreach (var item in accountInternalDebit)
                        {
                            debitEntryRow.AccountInternal.Add(accountInternals.First(ai => ai.AccountId == item.AccountId));
                        }
                        #endregion

                        #region Credit Row
                        var creditEntryRow = new AccountDistributionEntryRow
                        {
                            CreditAmount = periodWriteOffAmount,
                            CreditAmountEntCurrency = periodWriteOffAmount,
                            DebitAmount = 0,

                            //Set references
                            AccountDistributionEntry = entry,
                            AccountStd = creditStdDebit
                        };
                        foreach (var item in accountInternalCredit)
                        {
                            creditEntryRow.AccountInternal.Add(accountInternals.First(ai => ai.AccountId == item.AccountId));
                        }
                        inventory.AccountDistributionEntry.Add(entry);
                        #endregion

                        #region Inventory Log
                        //Log
                        var inventoryLog = new InventoryLog
                        {
                            Amount = periodWriteOffAmount,
                            Date = iPeriod,
                            Type = (int)TermGroup_InventoryLogType.WriteOff,

                            //Set references
                            Inventory = inventory,
                            AccountDistributionEntry = entry,
                            Company = company,

                            //Set FK
                            UserId = base.UserId,
                        };
                        inventory.InventoryLog.Add(inventoryLog);
                        #endregion

                        #region Currency (Not implemented)
                        /*            Currency has not been implemented for Inventories or Write-offs.
                         *            Below is some incomplete pseudo-code for future implementation.
                        if(inventoryCurrency != baseCurrency)
                        {
                          var endOfMonth = iPeriod.AddMonths(1).AddDays(-1);
                          var rate = currencyRates.FirstOrDefault(r => 
                              r.Date == endOfMonth &&
                              r.currency == inventoryCurrency);
                          debitEntryRow.DebitAmountEntCurrency = debitEntryRow.DebitAmount * (rate?.RateFromBase ?? 1);
                          creditEntryRow.DebitAmountEntCurrency = creditEntryRow.DebitAmount * (rate?.RateFromBase ?? 1);
                          inventoryLog.AmountEntCurrency = inventoryLog.Amount * (rate?.RateFromBase ?? 1);
                        }
                        */
                        #endregion

                        #endregion

                        entry.AccountDistributionEntryRow.Add(debitEntryRow);
                        entry.AccountDistributionEntryRow.Add(creditEntryRow);
                        entry.InventoryLog.Add(inventoryLog);
                        inventoryEntries.Add(entry);
                        iPeriod = iPeriod.AddMonths(1);
                    }

                    if (inventoryEntries.Count == 0)
                        continue;

                    var result2 = AccountDistributionManager.CreateAccountDistributionEntries(entities, inventoryEntries, actorCompanyId);
                    if (result.Success && !result2.Success)
                    {
                        result = result2;
                        result.ErrorMessage = GetText(7015, "Alla avskrivningarna kunde inte genereras");
                    }

                    #endregion
                }
            }

            return result;
        }

        #endregion
    }
}

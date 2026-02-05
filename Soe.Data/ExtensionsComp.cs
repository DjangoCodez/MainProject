using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.DTO.Scanning;
using SoftOne.Soe.Common.DTO.SignatoryContract;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data.Util;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Core.Objects.DataClasses;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace SoftOne.Soe.Data
{
    public static partial class ExtensionsComp
    {
        #region EntityObject

        public static CompEntities GetContext(this IEntityWithRelationships entity, out bool createdNewContext)
        {
            createdNewContext = false;
            if (entity == null)
                return null;

            var relationshipManager = entity.RelationshipManager;
            var relatedEnd = relationshipManager.GetAllRelatedEnds().FirstOrDefault();
            if (relatedEnd == null)
                return null;

            var query = relatedEnd.CreateSourceQuery() as ObjectQuery;
            if (query == null)
                return null;

            var context = query.Context as CompEntities;

            if (context.IsDisposed)
            {
                context = new CompEntities();
                createdNewContext = true;
            }
            try
            {
                if (context.Connection == null) { } //check if connection is still alive, calling .Connection will result in a disposed connection exception accourding to documentation
                                                    //if (context.Connection.State == System.Data.ConnectionState.Closed)             
                                                    //    context.Connection.Open();

            }
            catch (ObjectDisposedException odExc)
            {
                odExc.ToString(); //prevent compiler warning
                context = new CompEntities();
                createdNewContext = true;
            }

            return context;
        }
        public static bool TrySetEntityProperty(this EntityObject entity, string property, object value)
        {
            PropertyInfo pi = entity?.GetType().GetProperty(property);
            if (pi == null || !pi.CanWrite)
                return false;

            pi.SetValue(entity, value, null);
            return true;
        }
        public static bool TrySetEntityProperty<T>(this T entity, string property, object value)
        {
            PropertyInfo pi = entity?.GetType().GetProperty(property);
            if (pi == null || !pi.CanWrite)
                return false;

            pi.SetValue(entity, value, null);
            return true;
        }
        public static object GetEntityProperty(this EntityObject entity, string property)
        {
            PropertyInfo pi = entity?.GetType().GetProperty(property);
            if (pi != null && pi.CanRead)
                return pi.GetValue(entity, null);
            return null;
        }
        public static bool IsAdded(this EntityObject entity)
        {
            return entity.EntityState == EntityState.Added;
        }
        public static bool IsDetached(this EntityObject entity)
        {
            return entity.EntityState == EntityState.Detached;
        }
        public static void NoTracking(this ObjectQuery query)
        {
            query.MergeOption = MergeOption.NoTracking;
        }
        public static void Add(this CompEntities entities, object entity)
        {
            entities.AddObject(entity.GetType().ToString(), entity);
        }

        #endregion

        #region Tables

        #region AccountDistribution

        #region AccountDistributionEntry

        public static List<AccountDistributionEntryDTO> ToDTOs(this List<AccountDistributionEntry> e, bool setRows = false)
        {
            var dtos = new List<AccountDistributionEntryDTO>();
            if (e != null)
            {
                foreach (var entry in e)
                {
                    dtos.Add(entry.ToDTO(setRows));
                }
            }
            return dtos;
        }
        public static AccountDistributionEntryDTO ToDTO(this AccountDistributionEntry e, bool setRows = false)
        {
            if (e == null)
                return null;

            AccountDistributionEntryDTO dto = new AccountDistributionEntryDTO()
            {
                AccountDistributionEntryId = e.AccountDistributionEntryId,
                ActorCompanyId = e.ActorCompanyId,
                AccountDistributionHeadId = e.AccountDistributionHeadId,
                AccountDistributionHeadName = e.AccountDistributionHead != null ? e.AccountDistributionHead.Name : String.Empty,
                TriggerType = (TermGroup_AccountDistributionTriggerType)e.TriggerType,
                PeriodType = e.AccountDistributionHead != null ? (TermGroup_AccountDistributionPeriodType)e.AccountDistributionHead.PeriodType : 0,
                Date = e.Date,
                VoucherHeadId = e.VoucherHeadId,
                SupplierInvoiceId = e.SupplierInvoiceId,
                InventoryId = e.InventoryId,
                RegistrationType = e.RegistrationType != null ? (TermGroup_AccountDistributionRegistrationType)e.RegistrationType : 0,
                VoucherSeriesTypeId = e.Inventory?.VoucherSeriesTypeId ?? e.Inventory?.VoucherSeriesTypeId ?? null,
                SourceSupplierInvoiceId = e.SourceSupplierInvoiceId,
                SourceCustomerInvoiceId = e.SourceCustomerInvoiceId,
                SourceVoucherHeadId = e.SourceVoucherHeadId,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy
            };

            if (e.AccountDistributionEntryRow != null && setRows)
                dto.AccountDistributionEntryRowDTO = e.AccountDistributionEntryRow.ToDTOs();

            return dto;
        }
        #endregion

        #region AccountDistributionEntryRow

        public static List<AccountDistributionEntryRowDTO> ToDTOs(this ICollection<AccountDistributionEntryRow> e)
        {
            var dtos = new List<AccountDistributionEntryRowDTO>();
            if (e != null)
            {
                foreach (var entry in e)
                {
                    dtos.Add(entry.ToDTO());
                }
            }
            return dtos;
        }

        public static AccountDistributionEntryRowDTO ToDTO(this AccountDistributionEntryRow e)
        {
            if (e == null)
                return null;

            AccountDistributionEntryRowDTO dto = new AccountDistributionEntryRowDTO()
            {
                AccountDistributionEntryRowId = e.AccountDistributionEntryRowId,
                AccountDistributionEntryId = e.AccountDistributionEntryId,
                Dim1Id = e.AccountId,
                SameBalance = e.DebitAmount,
                OppositeBalance = e.CreditAmount,
                DebitAmount = e.DebitAmount,
                CreditAmount = e.CreditAmount,
                DebitAmountCurrency = e.DebitAmountCurrency,
                CreditAmountCurrency = e.CreditAmountCurrency,
                DebitAmountEntCurrency = e.DebitAmountEntCurrency,
                CreditAmountEntCurrency = e.CreditAmountEntCurrency,
                DebitAmountLedgerCurrency = e.DebitAmountLedgerCurrency,
                CreditAmountLedgerCurrency = e.CreditAmountLedgerCurrency
            };

            /*
             * Not my proudest moment, but in this case the ordering doesn't matter. For now.
             * We should apply the internals according to it's dims in the future, and set the internal accounts.
             */
            int dimCounter = 1;
            foreach (var acc in e.AccountInternal)
            {
                dimCounter++;
                switch (dimCounter)
                {
                    case 2:
                        dto.Dim2Id = acc.AccountId;
                        break;
                    case 3:
                        dto.Dim3Id = acc.AccountId;
                        break;
                    case 4:
                        dto.Dim4Id = acc.AccountId;
                        break;
                    case 5:
                        dto.Dim5Id = acc.AccountId;
                        break;
                    case 6:
                        dto.Dim6Id = acc.AccountId;
                        break;
                }
            }

            return dto;
        }

        #endregion

        #region AccountDistributionHead

        public static AccountDistributionHeadDTO ToDTO(this AccountDistributionHead e, bool includeAccounts, bool includeRows, List<AccountDim> accountDims)
        {
            if (e == null)
                return null;

            #region Try load

            try
            {
                if (!e.IsAdded())
                {
                    if (includeAccounts)
                    {
                        if (!e.AccountDistributionHeadAccountDimMapping.IsLoaded)
                        {
                            e.AccountDistributionHeadAccountDimMapping.Load();
                            DataProjectLogCollector.LogLoadedEntityInExtension("AccountDistributionHeadAccountDimMapping");
                        }
                        foreach (var map in e.AccountDistributionHeadAccountDimMapping)
                        {
                            if (!map.AccountDimReference.IsLoaded)
                            {
                                map.AccountDimReference.Load();
                                DataProjectLogCollector.LogLoadedEntityInExtension("AccountDimReference");
                            }
                        }
                    }

                    if (includeRows)
                    {
                        if (!e.AccountDistributionRow.IsLoaded)
                        {
                            e.AccountDistributionRow.Load();
                            DataProjectLogCollector.LogLoadedEntityInExtension("AccountDistributionRow");
                        }
                        if (includeAccounts)
                        {
                            foreach (var row in e.AccountDistributionRow)
                            {
                                if (!row.AccountStdReference.IsLoaded)
                                {
                                    row.AccountStdReference.Load();
                                    DataProjectLogCollector.LogLoadedEntityInExtension("AccountStdReference");
                                }
                                if (row.AccountStd != null && !row.AccountStd.AccountReference.IsLoaded)
                                {
                                    row.AccountStd.AccountReference.Load();
                                    DataProjectLogCollector.LogLoadedEntityInExtension("AccountStd.AccountReference");
                                }
                                if (!row.AccountDistributionRowAccount.IsLoaded)
                                {
                                    row.AccountDistributionRowAccount.Load();
                                    DataProjectLogCollector.LogLoadedEntityInExtension("AccountDistributionRowAccount");
                                }
                                foreach (var rowAcc in row.AccountDistributionRowAccount)
                                {
                                    if (!rowAcc.AccountInternalReference.IsLoaded)
                                    {
                                        rowAcc.AccountInternalReference.Load();
                                        DataProjectLogCollector.LogLoadedEntityInExtension("rowAcc.AccountInternalReference");
                                    }
                                    if (rowAcc.AccountInternal != null)
                                    {
                                        if (!rowAcc.AccountInternal.AccountReference.IsLoaded)
                                        {
                                            rowAcc.AccountInternal.AccountReference.Load();
                                            DataProjectLogCollector.LogLoadedEntityInExtension("rowAcc.AccountInternal");
                                        }
                                        if (rowAcc.AccountInternal.Account != null && !rowAcc.AccountInternal.Account.AccountDimReference.IsLoaded)
                                        {
                                            rowAcc.AccountInternal.Account.AccountDimReference.Load();
                                            DataProjectLogCollector.LogLoadedEntityInExtension("rowAcc.AccountInternal.Account.AccountDimReference");
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

            }
            catch (InvalidOperationException ex) { ex.ToString(); }

            #endregion

            AccountDistributionHeadDTO dto = new AccountDistributionHeadDTO()
            {
                AccountDistributionHeadId = e.AccountDistributionHeadId,
                ActorCompanyId = e.ActorCompanyId,
                VoucherSeriesTypeId = e.VoucherSeriesTypeId,
                Type = e.Type,
                Name = e.Name,
                Description = e.Description,
                TriggerType = e.TriggerType == 0 ? TermGroup_AccountDistributionTriggerType.Registration : (TermGroup_AccountDistributionTriggerType)e.TriggerType,
                CalculationType = (TermGroup_AccountDistributionCalculationType)e.CalculationType,
                Calculate = e.Calculate,
                PeriodType = e.PeriodType == 0 ? TermGroup_AccountDistributionPeriodType.Period : (TermGroup_AccountDistributionPeriodType)e.PeriodType,
                PeriodValue = e.PeriodValue,
                Sort = e.Sort,
                StartDate = e.StartDate,
                EndDate = e.EndDate,
                DayNumber = e.DayNumber,
                Amount = e.Amount,
                AmountOperator = e.AmountOperator,
                KeepRow = e.KeepRow,
                UseInVoucher = e.UseInVoucher,
                UseInSupplierInvoice = e.UseInSupplierInvoice,
                UseInCustomerInvoice = e.UseInCustomerInvoice,
                UseInImport = e.UseInImport,
                UseInPayrollVoucher = e.UseInPayrollVoucher,
                UseInPayrollVacationVoucher = e.UseInPayrollVacationVoucher,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State
            };

            // Extensions
            if (includeAccounts && includeRows)
            {
                #region Expressions

                foreach (var exp in e.AccountDistributionHeadAccountDimMapping)
                {
                    var pos = accountDims.FindIndex(x => x.AccountDimId == exp.AccountDimId) + 1;
                    switch (pos)
                    {
                        case (1):
                            dto.Dim1Id = exp.AccountDimId;
                            dto.Dim1Expression = exp.AccountExpression;
                            break;
                        case (2):
                            dto.Dim2Id = exp.AccountDimId;
                            dto.Dim2Expression = exp.AccountExpression;
                            break;
                        case (3):
                            dto.Dim3Id = exp.AccountDimId;
                            dto.Dim3Expression = exp.AccountExpression;
                            break;
                        case (4):
                            dto.Dim4Id = exp.AccountDimId;
                            dto.Dim4Expression = exp.AccountExpression;
                            break;
                        case (5):
                            dto.Dim5Id = exp.AccountDimId;
                            dto.Dim5Expression = exp.AccountExpression;
                            break;
                        case (6):
                            dto.Dim6Id = exp.AccountDimId;
                            dto.Dim6Expression = exp.AccountExpression;
                            break;
                    }
                }

                #endregion

                #region Rows

                dto.Rows = new List<AccountDistributionRowDTO>();
                AccountStd accStd;

                foreach (var row in e.AccountDistributionRow)
                {
                    // Only show active rows
                    if (row.State != (int)SoeEntityState.Active)
                        continue;

                    AccountDistributionRowDTO rowItem = new AccountDistributionRowDTO()
                    {
                        AccountDistributionRowId = row.AccountDistributionRowId,
                        AccountDistributionHeadId = row.AccountDistributionHeadId,
                        RowNbr = row.RowNbr,
                        CalculateRowNbr = row.CalculateRowNbr,
                        SameBalance = row.SameBalance,
                        OppositeBalance = row.OppositeBalance,
                        Description = row.Description,
                        State = (SoeEntityState)row.State,
                        Dim2Nr = String.Empty,
                        Dim2Name = String.Empty,
                        Dim3Nr = String.Empty,
                        Dim3Name = String.Empty,
                        Dim4Nr = String.Empty,
                        Dim4Name = String.Empty,
                        Dim5Nr = String.Empty,
                        Dim5Name = String.Empty,
                        Dim6Nr = String.Empty,
                        Dim6Name = String.Empty
                    };

                    # region Standard account

                    accStd = row.AccountStd;
                    if (accStd != null)
                    {
                        rowItem.Dim1Id = accStd.AccountId;
                        rowItem.Dim1Nr = accStd.Account.AccountNr;
                        rowItem.Dim1Name = accStd.Account.Name;
                        rowItem.Dim1Disabled = false;
                        rowItem.Dim1Mandatory = true;
                    }
                    else
                    {
                        rowItem.Dim1Id = 0;
                        rowItem.Dim1Nr = "*";
                        rowItem.Dim1Name = string.Empty;
                        rowItem.Dim1Disabled = false;
                        rowItem.Dim1Mandatory = true;
                    }

                    #endregion

                    #region Internal accounts (dim 2-6)

                    foreach (AccountDistributionRowAccount rowAccount in row.AccountDistributionRowAccount)
                    {
                        Account account = rowAccount.AccountInternal?.Account;
                        int accountId = account?.AccountId ?? 0;
                        string accountNr = account?.AccountNr ?? "*";
                        string accountName = account?.Name ?? string.Empty;

                        switch (rowAccount.DimNr)
                        {
                            case 2:
                                rowItem.Dim2Id = accountId;
                                rowItem.Dim2Nr = accountNr;
                                rowItem.Dim2Name = accountName;
                                rowItem.Dim2KeepSourceRowAccount = rowAccount.KeepSourceRowAccount;
                                break;
                            case 3:
                                rowItem.Dim3Id = accountId;
                                rowItem.Dim3Nr = accountNr;
                                rowItem.Dim3Name = accountName;
                                rowItem.Dim3KeepSourceRowAccount = rowAccount.KeepSourceRowAccount;
                                break;
                            case 4:
                                rowItem.Dim4Id = accountId;
                                rowItem.Dim4Nr = accountNr;
                                rowItem.Dim4Name = accountName;
                                rowItem.Dim4KeepSourceRowAccount = rowAccount.KeepSourceRowAccount;
                                break;
                            case 5:
                                rowItem.Dim5Id = accountId;
                                rowItem.Dim5Nr = accountNr;
                                rowItem.Dim5Name = accountName;
                                rowItem.Dim5KeepSourceRowAccount = rowAccount.KeepSourceRowAccount;
                                break;
                            case 6:
                                rowItem.Dim6Id = accountId;
                                rowItem.Dim6Nr = accountNr;
                                rowItem.Dim6Name = accountName;
                                rowItem.Dim6KeepSourceRowAccount = rowAccount.KeepSourceRowAccount;
                                break;
                        }
                    }

                    #endregion

                    // Add row to collection
                    dto.Rows.Add(rowItem);
                }

                #endregion
            }

            return dto;
        }

        public static IEnumerable<AccountDistributionHeadDTO> ToDTOs(this IEnumerable<AccountDistributionHead> l, bool includeAccounts, bool includeRows, List<AccountDim> accountDims)
        {
            var dtos = new List<AccountDistributionHeadDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO(includeAccounts, includeRows, accountDims));
                }
            }
            return dtos;
        }

        public static AccountDistributionHeadSmallDTO ToSmallDTO(this AccountDistributionHead e, bool includeAccounts)
        {
            if (e == null)
                return null;

            #region Try load

            try
            {
                if (!e.IsAdded() && includeAccounts)
                {
                    if (!e.AccountDistributionHeadAccountDimMapping.IsLoaded)
                    {
                        e.AccountDistributionHeadAccountDimMapping.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("e.AccountDistributionHeadAccountDimMapping");
                    }
                    foreach (var map in e.AccountDistributionHeadAccountDimMapping)
                    {
                        if (!map.AccountDimReference.IsLoaded)
                        {
                            map.AccountDimReference.Load();
                            DataProjectLogCollector.LogLoadedEntityInExtension("e.AccountDistributionHeadAccountDimMapping");
                        }
                    }
                }
            }
            catch (InvalidOperationException ex) { ex.ToString(); }

            #endregion

            AccountDistributionHeadSmallDTO dto = new AccountDistributionHeadSmallDTO
            {
                AccountDistributionHeadId = e.AccountDistributionHeadId,
                Type = e.Type,
                Name = e.Name,
                Description = e.Description,
                CalculationType = (TermGroup_AccountDistributionCalculationType)e.CalculationType,
                TriggerType = (TermGroup_AccountDistributionTriggerType)e.TriggerType,
                Sort = e.Sort,
                PeriodValue = e.PeriodValue,
                StartDate = e.StartDate,
                EndDate = e.EndDate,
                DayNumber = e.DayNumber,
                Amount = e.Amount,
                AmountOperator = e.AmountOperator,
                KeepRow = e.KeepRow,
                UseInVoucher = e.UseInVoucher,
                UseInImport = e.UseInImport,
            };

            // Extensions
            if (includeAccounts)
            {
                #region Expressions

                foreach (var exp in e.AccountDistributionHeadAccountDimMapping)
                {
                    switch (exp.AccountDim.AccountDimNr)
                    {
                        case (1):
                            dto.Dim1Expression = exp.AccountExpression;
                            break;
                        case (2):
                            dto.Dim2Expression = exp.AccountExpression;
                            break;
                        case (3):
                            dto.Dim3Expression = exp.AccountExpression;
                            break;
                        case (4):
                            dto.Dim4Expression = exp.AccountExpression;
                            break;
                        case (5):
                            dto.Dim5Expression = exp.AccountExpression;
                            break;
                        case (6):
                            dto.Dim6Expression = exp.AccountExpression;
                            break;
                    }
                }

                #endregion
            }

            if (e.AccountDistributionEntry != null && e.Type == (int)SoeAccountDistributionType.Period && e.TriggerType == (int)TermGroup_AccountDistributionTriggerType.Registration)
            {
                dto.EntryTotalCount = 0;
                dto.EntryTransferredCount = 0;
                dto.EntryTotalAmount = 0;
                dto.EntryTransferredAmount = 0;
                dto.EntryPeriodAmount = 0;

                foreach (var entry in e.AccountDistributionEntry.Where(a => a.State == (int)SoeEntityState.Active))
                {
                    if (!entry.AccountDistributionEntryRow.IsLoaded)
                    {
                        entry.AccountDistributionEntryRow.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("entry.AccountDistributionEntryRow");
                    }

                    dto.EntryTotalCount++;

                    var entryRow = entry.AccountDistributionEntryRow.FirstOrDefault();
                    decimal rowAmount = entryRow.DebitAmount > 0 ? entryRow.DebitAmount : entryRow.CreditAmount;

                    dto.EntryTotalAmount += rowAmount;

                    if (entry.VoucherHeadId != null && entry.VoucherHeadId > 0)
                    {
                        dto.EntryTransferredCount++;
                        dto.EntryTransferredAmount += rowAmount;
                        dto.EntryLatestTransferDate = entry.Date;
                    }

                    if (dto.EntryPeriodAmount == 0)
                        dto.EntryPeriodAmount = rowAmount;
                }
            }

            return dto;
        }

        public static IEnumerable<AccountDistributionHeadSmallDTO> ToSmallDTOs(this IEnumerable<AccountDistributionHead> l, bool includeAccounts)
        {
            var dtos = new List<AccountDistributionHeadSmallDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToSmallDTO(includeAccounts));
                }
            }
            return dtos;
        }

        #endregion

        #region AccountDistributionRow

        public static AccountDistributionRowDTO ToDTO(this AccountDistributionRow e)
        {
            if (e == null)
                return null;

            AccountDistributionRowDTO dto = new AccountDistributionRowDTO()
            {
                AccountDistributionRowId = e.AccountDistributionRowId,
                AccountDistributionHeadId = e.AccountDistributionHeadId,
                Dim1Id = e.AccountId,
                RowNbr = e.RowNbr,
                CalculateRowNbr = e.CalculateRowNbr,
                SameBalance = e.SameBalance,
                OppositeBalance = e.OppositeBalance,
                Description = e.Description,
                State = (SoeEntityState)e.State
            };

            return dto;
        }

        #endregion

        #endregion

        #region BudgetHead

        public static IEnumerable<BudgetHeadDTO> ToGridDTOs(this IEnumerable<BudgetHead> l)
        {
            var dtos = new List<BudgetHeadDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO(true));
                }
            }
            return dtos;
        }

        public static IEnumerable<BudgetHeadDTO> ToDTOs(this IEnumerable<BudgetHead> l)
        {
            var dtos = new List<BudgetHeadDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO(false));
                }
            }
            return dtos;
        }

        public static BudgetHeadDTO ToDTO(this BudgetHead e, bool partial)
        {
            if (e == null)
                return null;

            BudgetHeadDTO budgetHeadDTO = new BudgetHeadDTO()
            {
                BudgetHeadId = e.BudgetHeadId,
                ActorCompanyId = e.ActorCompanyId,
                AccountYearId = e.AccountYearId,
                DistributionCodeHeadId = e.DistributionCodeHeadId,
                Name = e.Name,
                NoOfPeriods = e.NoOfPeriods,
                Type = e.Type,
                Status = e.Status,
                Created = e.Created,
                CreatedDate = e.Created.HasValue ? ((DateTime)e.Created).ToShortDateString() : "",
                UseDim2 = e.UseDim2,
                UseDim3 = e.UseDim3,
                FromDate = e.FromDate,
                ToDate = e.ToDate,
                Rows = new List<BudgetRowDTO>(),
            };

            if (!e.AccountInternal.IsLoaded)
            {
                e.AccountInternal.Load();
                DataProjectLogCollector.LogLoadedEntityInExtension("e.AccountInternal");
            }

            // Internal accounts (dim 2-6)
            foreach (AccountInternal accountInternal in e.AccountInternal)
            {
                if (!accountInternal.AccountReference.IsLoaded)
                {
                    accountInternal.AccountReference.Load();
                    DataProjectLogCollector.LogLoadedEntityInExtension("accountInternal.AccountReference");
                }

                if (!accountInternal.Account.AccountDimReference.IsLoaded)
                {
                    accountInternal.Account.AccountDimReference.Load();
                    DataProjectLogCollector.LogLoadedEntityInExtension("ntInternal.Account.AccountDimReference");
                }

                switch (accountInternal.Account.AccountDim.AccountDimNr)
                {
                    case 2:
                        budgetHeadDTO.Dim2Id = accountInternal.AccountId;
                        break;
                    case 3:
                        budgetHeadDTO.Dim3Id = accountInternal.AccountId;
                        break;
                }
            }

            if (!partial && !e.BudgetRow.IsNullOrEmpty())
            {
                int rowNumber = 1;
                foreach (BudgetRow budgetRow in e.BudgetRow.Where(r => r.State == (int)SoeEntityState.Active))
                {
                    BudgetRowDTO budgetRowDTO = new BudgetRowDTO()
                    {
                        BudgetRowId = budgetRow.BudgetRowId,
                        BudgetHeadId = budgetRow.BudgetHeadId,
                        AccountId = budgetRow.AccountId != null ? (int)budgetRow.AccountId : 0,
                        DistributionCodeHeadId = budgetRow.DistributionCodeHeadId,
                        TotalAmount = budgetRow.TotalAmount,
                        TotalQuantity = budgetRow.TotalQuantity,
                        BudgetRowNr = rowNumber,
                        Periods = new List<BudgetPeriodDTO>(),
                        Modified = budgetRow.Modified != null ? ((DateTime)budgetRow.Modified).ToString() : "",
                        ModifiedBy = budgetRow.ModifiedBy.NullToEmpty(),
                        Type = budgetRow.Type,
                        ShiftTypeId = budgetRow.ShiftTypeId,

                        //Extensions
                        BudgetHead = budgetHeadDTO,
                    };

                    #region AccountDim

                    //För filtreringens skull så det inte är null
                    budgetRowDTO.Dim1Nr = String.Empty;
                    budgetRowDTO.Dim2Nr = String.Empty;
                    budgetRowDTO.Dim3Nr = String.Empty;
                    budgetRowDTO.Dim4Nr = String.Empty;
                    budgetRowDTO.Dim5Nr = String.Empty;
                    budgetRowDTO.Dim6Nr = String.Empty;

                    if (!budgetRow.AccountStdReference.IsLoaded)
                    {
                        budgetRow.AccountStdReference.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("accountInternal.AccountReference");
                    }

                    // Get standard account
                    AccountStd accountStd = budgetRow.AccountStd;
                    if (accountStd != null)
                    {
                        if (!accountStd.AccountReference.IsLoaded)
                        {
                            accountStd.AccountReference.Load();
                            DataProjectLogCollector.LogLoadedEntityInExtension("accountStd.AccountReference");
                        }


                        budgetRowDTO.Dim1Id = accountStd.AccountId;
                        budgetRowDTO.Dim1Nr = accountStd.Account.AccountNr;
                        budgetRowDTO.Dim1Name = accountStd.Account.Name;
                    }

                    if (!budgetRow.AccountInternal.IsLoaded)
                    {
                        budgetRow.AccountInternal.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension(" budgetRow.AccountInternal");
                    }


                    // Internal accounts (dim 2-6)
                    foreach (AccountInternal accountInternal in budgetRow.AccountInternal)
                    {
                        if (!accountInternal.AccountReference.IsLoaded)
                        {
                            accountInternal.AccountReference.Load();
                            DataProjectLogCollector.LogLoadedEntityInExtension("accountInternal.AccountReference");
                        }

                        if (!accountInternal.Account.AccountDimReference.IsLoaded)
                        {
                            accountInternal.Account.AccountDimReference.Load();
                            DataProjectLogCollector.LogLoadedEntityInExtension("accountInternal.Account.AccountDimReference");
                        }

                        switch (accountInternal.Account.AccountDim.AccountDimNr)
                        {
                            case 2:
                                budgetRowDTO.Dim2Id = accountInternal.AccountId;
                                budgetRowDTO.Dim2Nr = accountInternal.Account.AccountNr;
                                budgetRowDTO.Dim2Name = accountInternal.Account.Name;
                                break;
                            case 3:
                                budgetRowDTO.Dim3Id = accountInternal.AccountId;
                                budgetRowDTO.Dim3Nr = accountInternal.Account.AccountNr;
                                budgetRowDTO.Dim3Name = accountInternal.Account.Name;
                                break;
                            case 4:
                                budgetRowDTO.Dim4Id = accountInternal.AccountId;
                                budgetRowDTO.Dim4Nr = accountInternal.Account.AccountNr;
                                budgetRowDTO.Dim4Name = accountInternal.Account.Name;
                                break;
                            case 5:
                                budgetRowDTO.Dim5Id = accountInternal.AccountId;
                                budgetRowDTO.Dim5Nr = accountInternal.Account.AccountNr;
                                budgetRowDTO.Dim5Name = accountInternal.Account.Name;
                                break;
                            case 6:
                                budgetRowDTO.Dim6Id = accountInternal.AccountId;
                                budgetRowDTO.Dim6Nr = accountInternal.Account.AccountNr;
                                budgetRowDTO.Dim6Name = accountInternal.Account.Name;
                                break;
                        }
                    }

                    #endregion

                    if (!budgetRow.BudgetRowPeriod.IsNullOrEmpty())
                    {
                        int periodNr = 1;
                        foreach (BudgetRowPeriod budgetPeriod in budgetRow.BudgetRowPeriod)
                        {
                            BudgetPeriodDTO budgetPeriodDTO = new BudgetPeriodDTO()
                            {
                                BudgetRowPeriodId = budgetPeriod.BudgetRowPeriodId,
                                BudgetRowId = budgetPeriod.BudgetRowId,
                                DistributionCodeHeadId = budgetPeriod.DistributionCodeHeadId,
                                PeriodNr = periodNr,
                                Amount = budgetPeriod.Amount,
                                Quantity = budgetPeriod.Quantity,
                                Type = budgetPeriod.Type,
                                StartDate = budgetPeriod.StartTime,

                                //Extensions
                                BudgetRow = budgetRowDTO,
                            };

                            budgetRowDTO.Periods.Add(budgetPeriodDTO);
                            periodNr++;
                        }
                    }

                    budgetHeadDTO.Rows.Add(budgetRowDTO);
                    rowNumber++;
                }
            }

            return budgetHeadDTO;
        }

        public static BudgetHeadFlattenedDTO ToFlattenedDTO(this BudgetHead e)
        {
            if (e == null)
                return null;

            BudgetHeadFlattenedDTO dto = new BudgetHeadFlattenedDTO()
            {
                BudgetHeadId = e.BudgetHeadId,
                ActorCompanyId = e.ActorCompanyId,
                AccountYearId = e.AccountYearId,
                DistributionCodeHeadId = e.DistributionCodeHeadId,
                Name = e.Name,
                NoOfPeriods = e.NoOfPeriods,
                Type = e.Type,
                Status = e.Status,
                Created = e.Created,
                CreatedDate = e.Created.HasValue ? ((DateTime)e.Created).ToShortDateString() : "",
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                UseDim2 = e.UseDim2,
                UseDim3 = e.UseDim3,
                Rows = new List<BudgetRowFlattenedDTO>(),
            };

            if (!e.AccountInternal.IsLoaded)
            {
                e.AccountInternal.Load();
                DataProjectLogCollector.LogLoadedEntityInExtension("e.AccountInternal");
            }


            // Internal accounts (dim 2-6)
            foreach (AccountInternal accountInternal in e.AccountInternal)
            {
                if (!accountInternal.AccountReference.IsLoaded)
                {
                    accountInternal.AccountReference.Load();
                    DataProjectLogCollector.LogLoadedEntityInExtension("accountInternal.AccountReference");
                }

                if (!accountInternal.Account.AccountDimReference.IsLoaded)
                {
                    accountInternal.Account.AccountDimReference.Load();
                    DataProjectLogCollector.LogLoadedEntityInExtension("accountInternal.Account.AccountDimReference");
                }

                switch (accountInternal.Account.AccountDim.AccountDimNr)
                {
                    case 2:
                        dto.Dim2Id = accountInternal.AccountId;
                        break;
                    case 3:
                        dto.Dim3Id = accountInternal.AccountId;
                        break;
                }
            }

            #region rows

            if (!e.BudgetRow.IsNullOrEmpty())
            {
                int rowNumber = 1;
                foreach (BudgetRow r in e.BudgetRow.Where(r => r.State == (int)SoeEntityState.Active))
                {
                    BudgetRowFlattenedDTO pDto = new BudgetRowFlattenedDTO()
                    {
                        BudgetRowId = r.BudgetRowId,
                        BudgetHeadId = r.BudgetHeadId,
                        AccountId = r.AccountId != null ? (int)r.AccountId : 0,
                        DistributionCodeHeadId = r.DistributionCodeHeadId,
                        TotalAmount = r.TotalAmount,
                        TotalQuantity = r.TotalQuantity,
                        BudgetRowNr = rowNumber,
                        Modified = r.Modified != null ? ((DateTime)r.Modified).ToString() : "",
                        ModifiedBy = r.ModifiedBy.NullToEmpty(),
                    };

                    #region AccountDim

                    //För filtreringens skull så det inte är null
                    pDto.Dim1Id = 0;
                    pDto.Dim2Id = 0;
                    pDto.Dim3Id = 0;
                    pDto.Dim4Id = 0;
                    pDto.Dim5Id = 0;
                    pDto.Dim6Id = 0;
                    pDto.Dim1Nr = String.Empty;
                    pDto.Dim2Nr = String.Empty;
                    pDto.Dim3Nr = String.Empty;
                    pDto.Dim4Nr = String.Empty;
                    pDto.Dim5Nr = String.Empty;
                    pDto.Dim6Nr = String.Empty;
                    pDto.Dim1Name = String.Empty;
                    pDto.Dim2Name = String.Empty;
                    pDto.Dim3Name = String.Empty;
                    pDto.Dim4Name = String.Empty;
                    pDto.Dim5Name = String.Empty;
                    pDto.Dim6Name = String.Empty;

                    if (!r.AccountStdReference.IsLoaded)
                    {
                        r.AccountStdReference.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("r.AccountStdReference");
                    }

                    // Get standard account
                    AccountStd accountStd = r.AccountStd;
                    if (accountStd != null)
                    {
                        if (!accountStd.AccountReference.IsLoaded)
                        {
                            accountStd.AccountReference.Load();
                            DataProjectLogCollector.LogLoadedEntityInExtension("accountStd.AccountReference");
                        }

                        pDto.Dim1Id = accountStd.AccountId;
                        pDto.Dim1Nr = accountStd.Account.AccountNr;
                        pDto.Dim1Name = accountStd.Account.Name;
                    }

                    if (!r.AccountInternal.IsLoaded)
                    {
                        r.AccountInternal.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("r.AccountInternal");
                    }

                    // Internal accounts (dim 2-6)
                    foreach (AccountInternal accountInternal in r.AccountInternal)
                    {
                        if (!accountInternal.AccountReference.IsLoaded)
                        {
                            accountInternal.AccountReference.Load();
                            DataProjectLogCollector.LogLoadedEntityInExtension("accountInternal.AccountReference");
                        }

                        if (!accountInternal.Account.AccountDimReference.IsLoaded)
                        {
                            accountInternal.Account.AccountDimReference.Load();
                            DataProjectLogCollector.LogLoadedEntityInExtension("accountInternal.Account.AccountDimReference");
                        }

                        switch (accountInternal.Account.AccountDim.AccountDimNr)
                        {
                            case 2:
                                pDto.Dim2Id = accountInternal.AccountId;
                                pDto.Dim2Nr = accountInternal.Account.AccountNr;
                                pDto.Dim2Name = accountInternal.Account.Name;
                                break;
                            case 3:
                                pDto.Dim3Id = accountInternal.AccountId;
                                pDto.Dim3Nr = accountInternal.Account.AccountNr;
                                pDto.Dim3Name = accountInternal.Account.Name;
                                break;
                            case 4:
                                pDto.Dim4Id = accountInternal.AccountId;
                                pDto.Dim4Nr = accountInternal.Account.AccountNr;
                                pDto.Dim4Name = accountInternal.Account.Name;
                                break;
                            case 5:
                                pDto.Dim5Id = accountInternal.AccountId;
                                pDto.Dim5Nr = accountInternal.Account.AccountNr;
                                pDto.Dim5Name = accountInternal.Account.Name;
                                break;
                            case 6:
                                pDto.Dim6Id = accountInternal.AccountId;
                                pDto.Dim6Nr = accountInternal.Account.AccountNr;
                                pDto.Dim6Name = accountInternal.Account.Name;
                                break;
                        }
                    }

                    #endregion

                    if (!r.BudgetRowPeriod.IsNullOrEmpty())
                    {
                        int periodNr = 1;

                        Type t = pDto.GetType();
                        foreach (BudgetRowPeriod bp in r.BudgetRowPeriod.OrderBy(p => p.PeriodNr))
                        {
                            PropertyInfo periodIdProperty = t.GetProperty("BudgetRowPeriodId" + periodNr);
                            if (periodIdProperty != null)
                                periodIdProperty.SetValue(pDto, bp.BudgetRowPeriodId);

                            PropertyInfo periodNrProperty = t.GetProperty("PeriodNr" + periodNr);
                            if (periodNrProperty != null)
                                periodNrProperty.SetValue(pDto, bp.PeriodNr);

                            PropertyInfo startDateProperty = t.GetProperty("StartDate" + periodNr);
                            if (startDateProperty != null)
                                startDateProperty.SetValue(pDto, bp.StartTime);

                            PropertyInfo amountProperty = t.GetProperty("Amount" + periodNr);
                            if (amountProperty != null)
                                amountProperty.SetValue(pDto, bp.Amount);

                            PropertyInfo quantityProperty = t.GetProperty("Quantity" + periodNr);
                            if (quantityProperty != null)
                                quantityProperty.SetValue(pDto, bp.Quantity);

                            periodNr++;
                        }
                    }

                    dto.Rows.Add(pDto);
                    rowNumber++;
                }
            }

            #endregion

            return dto;
        }

        public static BudgetHeadFlattenedDTO ToSalesBudgetFlattenedDTO(this BudgetHead e)
        {
            if (e == null)
                return null;

            BudgetHeadFlattenedDTO dto = new BudgetHeadFlattenedDTO()
            {
                BudgetHeadId = e.BudgetHeadId,
                ActorCompanyId = e.ActorCompanyId,
                AccountYearId = e.AccountYearId,
                DistributionCodeHeadId = e.DistributionCodeHeadId,
                Name = e.Name,
                NoOfPeriods = e.NoOfPeriods,
                Type = e.Type,
                Status = e.Status,
                Created = e.Created,
                CreatedDate = e.Created.HasValue ? ((DateTime)e.Created).ToShortDateString() : "",
                UseDim2 = e.UseDim2,
                UseDim3 = e.UseDim3,
                Rows = new List<BudgetRowFlattenedDTO>(),
            };

            if (!e.AccountInternal.IsLoaded)
                e.AccountInternal.Load();

            // Internal accounts (dim 2-6)
            foreach (AccountInternal accountInternal in e.AccountInternal)
            {
                if (!accountInternal.AccountReference.IsLoaded)
                {
                    accountInternal.AccountReference.Load();
                    DataProjectLogCollector.LogLoadedEntityInExtension("accountInternal.AccountReference");
                }

                if (!accountInternal.Account.AccountDimReference.IsLoaded)
                {
                    accountInternal.Account.AccountDimReference.Load();
                    DataProjectLogCollector.LogLoadedEntityInExtension("accountInternal.Account.AccountDimReference");
                }

                switch (accountInternal.Account.AccountDim.AccountDimNr)
                {
                    case 2:
                        dto.Dim2Id = accountInternal.AccountId;
                        break;
                    case 3:
                        dto.Dim3Id = accountInternal.AccountId;
                        break;
                }
            }

            #region rows

            if (!e.BudgetRow.IsNullOrEmpty())
            {
                int rowNumber = 1;
                foreach (BudgetRow r in e.BudgetRow.Where(r => r.State == (int)SoeEntityState.Active))
                {
                    BudgetRowFlattenedDTO pDto = new BudgetRowFlattenedDTO()
                    {
                        BudgetRowId = r.BudgetRowId,
                        BudgetHeadId = r.BudgetHeadId,
                        AccountId = r.AccountId != null ? (int)r.AccountId : 0,
                        DistributionCodeHeadId = r.DistributionCodeHeadId,
                        TotalAmount = r.TotalAmount,
                        TotalQuantity = r.TotalQuantity,
                        BudgetRowNr = rowNumber,
                        Modified = r.Modified != null ? ((DateTime)r.Modified).ToString() : "",
                        ModifiedBy = r.ModifiedBy.NullToEmpty(),
                    };

                    #region AccountDim

                    //För filtreringens skull så det inte är null
                    pDto.Dim1Nr = String.Empty;
                    pDto.Dim2Nr = String.Empty;
                    pDto.Dim3Nr = String.Empty;
                    pDto.Dim4Nr = String.Empty;
                    pDto.Dim5Nr = String.Empty;
                    pDto.Dim6Nr = String.Empty;

                    if (!r.AccountStdReference.IsLoaded)
                    {
                        r.AccountStdReference.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("r.AccountStdReference");
                    }

                    // Get standard account
                    AccountStd accountStd = r.AccountStd;
                    if (accountStd != null)
                    {
                        if (!accountStd.AccountReference.IsLoaded)
                        {
                            accountStd.AccountReference.Load();
                            DataProjectLogCollector.LogLoadedEntityInExtension("accountStd.AccountReference");
                        }

                        pDto.Dim1Id = accountStd.AccountId;
                        pDto.Dim1Nr = accountStd.Account.AccountNr;
                        pDto.Dim1Name = accountStd.Account.Name;
                    }

                    if (!r.AccountInternal.IsLoaded)
                    {
                        r.AccountInternal.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("r.AccountInternal");
                    }

                    // Internal accounts (dim 2-6)
                    foreach (AccountInternal accountInternal in r.AccountInternal)
                    {
                        if (!accountInternal.AccountReference.IsLoaded)
                        {
                            accountInternal.AccountReference.Load();
                            DataProjectLogCollector.LogLoadedEntityInExtension("accountInternal.AccountReference");
                        }

                        if (!accountInternal.Account.AccountDimReference.IsLoaded)
                        {
                            accountInternal.Account.AccountDimReference.Load();
                            DataProjectLogCollector.LogLoadedEntityInExtension("accountInternal.AccountReference");
                        }

                        switch (accountInternal.Account.AccountDim.AccountDimNr)
                        {
                            case 2:
                                pDto.Dim2Id = accountInternal.AccountId;
                                pDto.Dim2Nr = accountInternal.Account.AccountNr;
                                pDto.Dim2Name = accountInternal.Account.Name;
                                break;
                            case 3:
                                pDto.Dim1Id = accountInternal.AccountId;
                                pDto.Dim1Nr = accountInternal.Account.AccountNr;
                                pDto.Dim1Name = accountInternal.Account.Name;
                                break;
                            case 4:
                                pDto.Dim4Id = accountInternal.AccountId;
                                pDto.Dim4Nr = accountInternal.Account.AccountNr;
                                pDto.Dim4Name = accountInternal.Account.Name;
                                break;
                            case 5:
                                pDto.Dim5Id = accountInternal.AccountId;
                                pDto.Dim5Nr = accountInternal.Account.AccountNr;
                                pDto.Dim5Name = accountInternal.Account.Name;
                                break;
                            case 6:
                                pDto.Dim6Id = accountInternal.AccountId;
                                pDto.Dim6Nr = accountInternal.Account.AccountNr;
                                pDto.Dim6Name = accountInternal.Account.Name;
                                break;
                        }
                    }

                    #endregion

                    //In salesbugget we create periods in manager

                    dto.Rows.Add(pDto);
                    rowNumber++;
                }
            }

            #endregion

            return dto;
        }

        #endregion

        #region BudgetRowPeriod

        public static DateTime GetStopTime(this BudgetRowPeriod e)
        {
            DateTime stopTime = CalendarUtility.DATETIME_DEFAULT;
            if (e.StartTime.HasValue)
            {
                switch (e.Type)
                {
                    case (int)BudgetRowPeriodType.Year:
                        stopTime = e.StartTime.Value.AddYears(1);
                        break;
                    case (int)BudgetRowPeriodType.SixMonths:
                        stopTime = e.StartTime.Value.AddMonths(6);
                        break;
                    case (int)BudgetRowPeriodType.Quarter:
                        stopTime = e.StartTime.Value.AddMonths(3);
                        break;
                    case (int)BudgetRowPeriodType.Month:
                        stopTime = e.StartTime.Value.AddMonths(1);
                        break;
                    case (int)BudgetRowPeriodType.Week:
                        stopTime = e.StartTime.Value.AddDays(7);
                        break;
                    case (int)BudgetRowPeriodType.Day:
                        stopTime = e.StartTime.Value.AddDays(1);
                        break;
                    case (int)BudgetRowPeriodType.Hour:
                        stopTime = e.StartTime.Value.AddHours(1);
                        break;
                }
            }
            return stopTime;
        }

        #endregion

        #region Checklist

        #region ChecklistHead

        public static ChecklistHeadDTO ToDTO(this ChecklistHead e, bool includeRows)
        {
            if (e == null)
                return null;

            #region Try load

            try
            {
                if (includeRows && !e.IsAdded() && !e.ChecklistRow.IsLoaded)
                {
                    e.ChecklistRow.Load();
                    DataProjectLogCollector.LogLoadedEntityInExtension("e.ChecklistRow");
                }
            }
            catch (InvalidOperationException ex) { ex.ToString(); }

            #endregion

            var dto = new ChecklistHeadDTO
            {
                ChecklistHeadId = e.ChecklistHeadId,
                ActorCompanyId = e.ActorCompanyId,
                ReportId = e.ReportId,
                Type = (TermGroup_ChecklistHeadType)e.Type,
                Name = e.Name,
                Description = e.Description,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State,
                DefaultInOrder = e.DefaultInOrder,
                AddAttachementsToEInvoice = e.AddAttachementsToEInvoice
            };

            // Extensions
            dto.TypeName = e.TypeName;

            if (includeRows)
                dto.ChecklistRows = (e.ChecklistRow != null && e.ChecklistRow.Count > 0) ? e.ChecklistRow.Where(r => r.State == (int)SoeEntityState.Active).ToDTOs().ToList() : new List<ChecklistRowDTO>();

            return dto;
        }

        public static IEnumerable<ChecklistHeadDTO> ToDTOs(this IEnumerable<ChecklistHead> l, bool includeRows)
        {
            var dtos = new List<ChecklistHeadDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO(includeRows));
                }
            }
            return dtos;
        }

        public static ChecklistHeadGridDTO ToGridDTO(this ChecklistHead e)
        {
            if (e == null)
                return null;

            var dto = new ChecklistHeadGridDTO
            {
                ChecklistHeadId = e.ChecklistHeadId,
                Name = e.Name,
                Description = e.Description,
                TypeName = e.TypeName,
                DefaultInOrder = e.DefaultInOrder,
                AddAttachementsToEInvoice = e.AddAttachementsToEInvoice,
                IsActive = e.State == (int)SoeEntityState.Active,
            };

            return dto;
        }

        public static IEnumerable<ChecklistHeadGridDTO> ToGridDTOs(this IEnumerable<ChecklistHead> l)
        {
            var dtos = new List<ChecklistHeadGridDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToGridDTO());
                }
            }
            return dtos;
        }

        public static ChecklistHeadRecordDTO ToDTO(this ChecklistHeadRecord e)
        {
            if (e == null)
                return null;

            ChecklistHeadRecordDTO dto = new ChecklistHeadRecordDTO()
            {
                ChecklistHeadRecordId = e.ChecklistHeadRecordId,
                ChecklistHeadId = e.ChecklistHeadId,
                ActorCompanyId = e.ActorCompanyId,
                Entity = (SoeEntityType)e.Entity,
                RecordId = e.RecordId,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State
            };

            return dto;
        }

        public static IEnumerable<ChecklistHeadRecordDTO> ToDTOs(this IEnumerable<ChecklistHeadRecord> l)
        {
            var dtos = new List<ChecklistHeadRecordDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static ChecklistHeadRecordCompactDTO ToCompactDTO(this ChecklistHeadRecord e)
        {
            if (e == null)
                return null;

            ChecklistHeadRecordCompactDTO dto = new ChecklistHeadRecordCompactDTO()
            {
                ChecklistHeadRecordId = e.ChecklistHeadRecordId,
                ChecklistHeadId = e.ChecklistHeadId,
                ChecklistHeadName = e.ChecklistHead != null ? e.ChecklistHead.Name : string.Empty,
                AddAttachementsToEInvoice = e.AddAttachementsToEInvoice,
                RecordId = e.RecordId,
                State = (SoeEntityState)e.State,
                Created = e.Created
            };

            dto.ChecklistRowRecords = new List<ChecklistExtendedRowDTO>();

            foreach (ChecklistRowRecord rowRecord in e.ChecklistRowRecord)
            {
                var rowDto = new ChecklistExtendedRowDTO()
                {
                    RowRecordId = rowRecord.ChecklistRowRecordId,
                    HeadRecordId = rowRecord.ChecklistHeadRecordId,
                    Comment = rowRecord.Comment,
                    Date = rowRecord.Date,
                    DataTypeId = rowRecord.DataTypeId,
                    StrData = rowRecord.StrData,
                    IntData = rowRecord.IntData,
                    BoolData = rowRecord.BoolData,
                    DecimalData = rowRecord.DecimalData,
                    Created = rowRecord.Created,
                    CreatedBy = rowRecord.CreatedBy,
                    Modified = rowRecord.Modified,
                    ModifiedBy = rowRecord.ModifiedBy,
                };

                //Overwrite with values from record
                rowDto.Text = rowRecord.Text;
                rowDto.Type = (TermGroup_ChecklistRowType)rowRecord.Type;

                if (rowRecord.ChecklistRow != null)
                {
                    if (rowDto.Type == TermGroup_ChecklistRowType.MultipleChoice)
                        rowDto.CheckListMultipleChoiceAnswerHeadId = rowRecord.ChecklistRow.CheckListMultipleChoiceAnswerHeadId;

                    //ChecklistHead
                    rowDto.Guid = Guid.NewGuid();

                    //ChecklistRow
                    rowDto.RowId = rowRecord.ChecklistRow.ChecklistRowId;
                    rowDto.HeadId = rowRecord.ChecklistRow.ChecklistHeadId;
                    rowDto.RowNr = rowRecord.ChecklistRow.RowNr;
                    rowDto.Mandatory = rowRecord.ChecklistRow.Mandatory;
                    rowDto.Text = rowRecord.ChecklistRow.Text;
                    rowDto.Type = (TermGroup_ChecklistRowType)rowRecord.Type;
                }


                dto.ChecklistRowRecords.Add(rowDto);
            }

            return dto;
        }

        public static IEnumerable<ChecklistHeadRecordCompactDTO> ToCompactDTOs(this IEnumerable<ChecklistHeadRecord> l)
        {
            var dtos = new List<ChecklistHeadRecordCompactDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToCompactDTO());
                }
            }
            return dtos;
        }

        public static void CopyFrom(this ChecklistHead newItem, ChecklistHead source)
        {
            if (source == null || newItem == null)
                return;

            newItem.Type = source.Type;
            newItem.Name = source.Name;
            newItem.Description = source.Description;
        }

        #endregion

        #region ChecklistRow

        public static ChecklistRowDTO ToDTO(this ChecklistRow e)
        {
            if (e == null)
                return null;

            ChecklistRowDTO dto = new ChecklistRowDTO()
            {
                ChecklistRowId = e.ChecklistRowId,
                ChecklistHeadId = e.ChecklistHeadId,
                Type = (TermGroup_ChecklistRowType)e.Type,
                RowNr = e.RowNr,
                Mandatory = e.Mandatory,
                Text = e.Text,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State,
                CheckListMultipleChoiceAnswerHeadId = e.CheckListMultipleChoiceAnswerHeadId > 0 ? e.CheckListMultipleChoiceAnswerHeadId : (int?)null
            };

            return dto;
        }

        public static IEnumerable<ChecklistRowDTO> ToDTOs(this IEnumerable<ChecklistRow> l)
        {
            var dtos = new List<ChecklistRowDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static void CopyFrom(this ChecklistRow newItem, ChecklistRow source)
        {
            if (source == null || newItem == null)
                return;

            newItem.Text = source.Text;
            newItem.Type = source.Type;
            newItem.Mandatory = source.Mandatory;
            newItem.RowNr = source.RowNr;
            newItem.CheckListMultipleChoiceAnswerHeadId = source.CheckListMultipleChoiceAnswerHeadId;
        }

        #endregion

        #region ChecklistMultipleChoiceAnswerHead

        public static CheckListMultipleChoiceAnswerHeadDTO ToDTO(this CheckListMultipleChoiceAnswerHead e)
        {
            if (e == null)
                return null;

            CheckListMultipleChoiceAnswerHeadDTO dto = new CheckListMultipleChoiceAnswerHeadDTO()
            {
                CheckListMultipleChoiceAnswerHeadId = e.CheckListMultipleChoiceAnswerHeadId,
                Title = e.Title,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State
            };

            return dto;
        }

        public static IEnumerable<CheckListMultipleChoiceAnswerHeadDTO> ToDTOs(this IEnumerable<CheckListMultipleChoiceAnswerHead> l)
        {
            var dtos = new List<CheckListMultipleChoiceAnswerHeadDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static void CopyFrom(this CheckListMultipleChoiceAnswerHead newItem, CheckListMultipleChoiceAnswerHead source)
        {
            if (source == null || newItem == null)
                return;

            newItem.ActorCompanyId = source.ActorCompanyId;
            newItem.State = source.State;
            newItem.Title = source.Title;

            foreach (var row in source.CheckListMultipleChoiceAnswerRow)
            {
                newItem.CheckListMultipleChoiceAnswerRow.Add(new CheckListMultipleChoiceAnswerRow()
                {
                    Question = row.Question,
                    Created = DateTime.Now,
                    CreatedBy = "CopyFrom",
                });
            }
        }

        #endregion

        #region ChecklistMultipleChoiceAnswerRow

        public static CheckListMultipleChoiceAnswerRowDTO ToDTO(this CheckListMultipleChoiceAnswerRow e)
        {
            if (e == null)
                return null;

            CheckListMultipleChoiceAnswerRowDTO dto = new CheckListMultipleChoiceAnswerRowDTO()
            {
                CheckListMultipleChoiceAnswerRowId = e.CheckListMultipleChoiceAnswerRowId,
                CheckListMultipleChoiceAnswerHeadId = e.CheckListMultipleChoiceAnswerHeadId,
                Question = e.Question,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State,
            };

            return dto;
        }

        public static IEnumerable<CheckListMultipleChoiceAnswerRowDTO> ToDTOs(this IEnumerable<CheckListMultipleChoiceAnswerRow> l)
        {
            var dtos = new List<CheckListMultipleChoiceAnswerRowDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        #endregion

        #endregion

        #region ContractGroup

        public static ContractGroupDTO ToDTO(this ContractGroup e)
        {
            if (e == null)
                return null;

            ContractGroupDTO dto = new ContractGroupDTO()
            {
                ContractGroupId = e.ContractGroupId,
                ActorCompanyId = e.ActorCompanyId,
                Name = e.Name,
                Description = e.Description,
                Period = (TermGroup_ContractGroupPeriod)e.Period,
                Interval = e.Interval,
                DayInMonth = e.DayInMonth,
                PriceManagement = (TermGroup_ContractGroupPriceManagement)e.PriceManagement,
                InvoiceText = e.InvoiceText,
                InvoiceTextRow = e.InvoiceTextRow,
                OrderTemplate = e.OrderTemplate,
                InvoiceTemplate = e.InvoiceTemplate,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State
            };

            return dto;
        }

        public static IEnumerable<ContractGroupDTO> ToDTOs(this IEnumerable<ContractGroup> l)
        {
            var dtos = new List<ContractGroupDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static ContractGroupGridDTO ToGridDTO(this ContractGroup e)
        {
            if (e == null)
                return null;

            ContractGroupGridDTO dto = new ContractGroupGridDTO()
            {
                ContractGroupId = e.ContractGroupId,
                Name = e.Name,
                Description = e.Description,
            };

            return dto;
        }

        public static IEnumerable<ContractGroupGridDTO> ToGridDTOs(this IEnumerable<ContractGroup> l)
        {
            var dtos = new List<ContractGroupGridDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToGridDTO());
                }
            }
            return dtos;
        }

        #endregion

        #region Customer

        public static List<HouseholdTaxDeductionApplicantDTO> ToDTO(this List<HouseholdTaxDeductionApplicant> e)
        {
            var list = new List<HouseholdTaxDeductionApplicantDTO>();

            foreach (HouseholdTaxDeductionApplicant applicant in e)
            {
                list.Add(new HouseholdTaxDeductionApplicantDTO
                {
                    HouseholdTaxDeductionApplicantId = applicant.HouseholdTaxDeductionApplicantId,
                    Name = applicant.Name,
                    SocialSecNr = applicant.SocialSecNr,
                    Property = applicant.Property,
                    ApartmentNr = applicant.ApartmentNr,
                    CooperativeOrgNr = applicant.CooperativeOrgNr,
                    Share = applicant.Share ?? decimal.Zero,
                });
            }

            return list;
        }

        public static CustomerDTO ToDTO(this Customer e, bool includeContactAddresses, bool includeAccountingSettings, bool loadNote)
        {
            if (e == null)
                return null;

            #region Try load

            try
            {
                if (!e.IsAdded())
                {
                    //if (!e.HouseholdTaxDeductionApplicant.IsLoaded)
                    //    e.HouseholdTaxDeductionApplicant.Load();

                    if (includeAccountingSettings)
                    {
                        if (!e.CustomerAccountStd.IsLoaded)
                        {
                            e.CustomerAccountStd.Load();
                            DataProjectLogCollector.LogLoadedEntityInExtension("e.CustomerAccountStd");
                        }

                        foreach (CustomerAccountStd customerAccountStd in e.CustomerAccountStd)
                        {
                            if (!customerAccountStd.AccountStdReference.IsLoaded)
                            {
                                customerAccountStd.AccountStdReference.Load();
                                DataProjectLogCollector.LogLoadedEntityInExtension("customerAccountStd.AccountStdReference");
                            }
                            if (customerAccountStd.AccountStd != null)
                            {
                                if (!customerAccountStd.AccountStd.AccountReference.IsLoaded)
                                {
                                    customerAccountStd.AccountStd.AccountReference.Load();
                                    DataProjectLogCollector.LogLoadedEntityInExtension("customerAccountStd.AccountStd.AccountReference");
                                }
                                if (!customerAccountStd.AccountStd.Account.AccountDimReference.IsLoaded)
                                {
                                    customerAccountStd.AccountStd.Account.AccountDimReference.Load();
                                    DataProjectLogCollector.LogLoadedEntityInExtension("customerAccountStd.AccountStd.Account.AccountDimReference");
                                }
                            }
                            if (!customerAccountStd.AccountInternal.IsLoaded)
                            {
                                customerAccountStd.AccountInternal.Load();
                                DataProjectLogCollector.LogLoadedEntityInExtension("customerAccountStd.AccountInternal");
                            }
                            foreach (AccountInternal accountInternal in customerAccountStd.AccountInternal)
                            {
                                if (!accountInternal.AccountReference.IsLoaded)
                                {
                                    accountInternal.AccountReference.Load();
                                    DataProjectLogCollector.LogLoadedEntityInExtension("accountInternal.AccountReference");
                                }
                                if (!accountInternal.Account.AccountDimReference.IsLoaded)
                                {
                                    accountInternal.Account.AccountDimReference.Load();
                                    DataProjectLogCollector.LogLoadedEntityInExtension("accountInternal.Account.AccountDimReference");
                                }
                            }
                        }
                    }
                }
            }
            catch (InvalidOperationException ex) { ex.ToString(); }

            #endregion

            var dto = new CustomerDTO
            {
                ActorCustomerId = e.ActorCustomerId,
                VatType = e.VatType == 2 ? TermGroup_InvoiceVatType.Merchandise : (TermGroup_InvoiceVatType)e.VatType,
                DeliveryConditionId = e.DeliveryConditionId,
                DeliveryTypeId = e.DeliveryTypeId,
                PaymentConditionId = e.PaymentConditionId,
                PriceListTypeId = e.PriceListTypeId,
                CurrencyId = e.CurrencyId,
                SysCountryId = e.SysCountryId,
                SysLanguageId = e.SysLanguageId,
                SysWholeSellerId = e.SysWholeSellerId,
                CustomerNr = e.CustomerNr,
                Name = e.Name,
                OrgNr = e.OrgNr,
                VatNr = e.VatNr,
                InvoiceReference = e.InvoiceReference,
                GracePeriodDays = e.GracePeriodDays,
                PaymentMorale = e.PaymentMorale,
                SupplierNr = e.SupplierNr,
                OfferTemplate = e.OfferTemplate,
                OrderTemplate = e.OrderTemplate,
                BillingTemplate = e.BillingTemplate,
                AgreementTemplate = e.AgreementTemplate,
                ManualAccounting = e.ManualAccounting,
                DiscountMerchandise = e.DiscountMerchandise,
                Discount2Merchandise = e.Discount2Merchandise,
                DiscountService = e.DiscountService,
                Discount2Service = e.Discount2Service,
                DisableInvoiceFee = e.DisableInvoiceFee,
                Note = (!loadNote && !e.ShowNote) ? string.Empty : e.Note,
                ShowNote = e.ShowNote,
                FinvoiceAddress = e.FinvoiceAddress,
                FinvoiceOperator = e.FinvoiceOperator,
                IsFinvoiceCustomer = e.IsFinvoiceCustomer,
                BlockNote = e.BlockNote,
                BlockOrder = e.BlockOrder,
                BlockInvoice = e.BlockInvoice,
                CreditLimit = e.CreditLimit,
                IsCashCustomer = e.IsCashCustomer.HasValue && e.IsCashCustomer.Value,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State,
                Active = e.State == (int)SoeEntityState.Active,
                InvoiceDeliveryType = e.InvoiceDeliveryType,
                InvoiceDeliveryProvider = e.InvoiceDeliveryProvider,
                DepartmentNr = e.DepartmentNr,
                PayingCustomerId = e.PayingCustomerId,
                InvoicePaymentService = e.InvoicePaymentService,
                BankAccountNr = e.BankAccountNr,
                AddAttachementsToEInvoice = e.AddAttachementsToEInvoice,
                ContactEComId = e.ContactEComId,
                IsPrivatePerson = e.IsPrivatePerson.HasValue && (bool)e.IsPrivatePerson,
                ContactGLNId = e.ContactGLNId,
                InvoiceLabel = e.InvoiceLabel,
                ReminderContactEComId = e.ReminderContactEComId,
                AddSupplierInvoicesToEInvoice = e.AddSupplierInvoicesToEInvoices ?? true,
                IsOneTimeCustomer = e.IsOneTimeCustomer,
                ImportInvoicesDetailed = e.ImportInvoicesDetailed,
                IsEUCountryBased = e.IsEUCountryBased,
                OrderContactEComId = e.OrderContactEComId,
                TriangulationSales = e.TriangulationSales,
                ContractNr = e.ContractNr,
            };

            // CustomerUser
            dto.CustomerUsers = new List<CustomerUserDTO>();

            if (e.CustomerUser != null)
            {
                foreach (var user in e.CustomerUser.Where(cu => cu.State == (int)SoeEntityState.Active).OrderBy(cu => cu.User.Name))
                {
                    dto.CustomerUsers.Add(user.ToDTO());
                }
            }

            if (dto.IsPrivatePerson)
            {
                ActorConsent actorConsent = e.Actor?.ActorConsent.FirstOrDefault(a => a.ConsentType == (int)ActorConsentType.Unspecified);
                if (actorConsent != null)
                {
                    dto.HasConsent = actorConsent.HasConsent;
                    dto.ConsentDate = actorConsent.ConsentDate;
                    dto.ConsentModified = actorConsent.ConsentModified;
                    dto.ConsentModifiedBy = actorConsent.ConsentModifiedBy;
                }
            }

            // Houshold deduction applicants
            //dto.HouseholdApplicants = new List<HouseholdTaxDeductionApplicantDTO>();
            /*
            if (e.HouseholdTaxDeductionApplicant != null)
            {
                foreach (HouseholdTaxDeductionApplicant applicant in e.HouseholdTaxDeductionApplicant.Where(h => h.State == (int)SoeEntityState.Active))
                {
                    dto.HouseholdApplicants.Add(new HouseholdTaxDeductionApplicantDTO()
                    {
                        HouseholdTaxDeductionApplicantId = applicant.HouseholdTaxDeductionApplicantId,
                        Name = applicant.Name,
                        SocialSecNr = applicant.SocialSecNr,
                        Property = applicant.Property,
                        ApartmentNr = applicant.ApartmentNr,
                        CooperativeOrgNr = applicant.CooperativeOrgNr,
                        Share = applicant.Share.HasValue ? applicant.Share.Value : Decimal.Zero,
                    });
                }
            }
            */
            dto.CustomerProducts = new List<CustomerProductPriceSmallDTO>();

            if (e.CustomerProduct != null)
            {
                foreach (CustomerProduct p in e.CustomerProduct)
                {
                    if (!p.ProductReference.IsLoaded)
                    {
                        p.ProductReference.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("p.ProductReference");
                    }

                    dto.CustomerProducts.Add(new CustomerProductPriceSmallDTO()
                    {
                        CustomerProductId = p.CustomerProductId,
                        Name = p.Product.Name,
                        Number = p.Product.Number,
                        Price = p.Price,
                        ProductId = p.Product.ProductId,
                    });
                }
            }

            dto.CategoryIds = e.CategoryIds;

            #region ContactAddresses

            if (includeContactAddresses)
                dto.ContactAddresses = GetContactAddressItems(e.Actor.Contact.FirstOrDefault());

            #endregion

            // Accounts

            if (includeAccountingSettings)
            {
                dto.AccountingSettings = new List<AccountingSettingsRowDTO>();

                if (e.CustomerAccountStd != null && e.CustomerAccountStd.Count > 0)
                {
                    AddAccountingSettingsRowDTO(e, dto, CustomerAccountType.Debit);
                    AddAccountingSettingsRowDTO(e, dto, CustomerAccountType.Credit);
                    AddAccountingSettingsRowDTO(e, dto, CustomerAccountType.VAT);
                }
            }

            return dto;
        }

        private static void AddAccountingSettingsRowDTO(Customer customer, CustomerDTO dto, CustomerAccountType type)
        {
            AccountingSettingsRowDTO accDto = new AccountingSettingsRowDTO()
            {
                Type = (int)type,
                Percent = 0
            };
            dto.AccountingSettings.Add(accDto);

            CustomerAccountStd accStd = customer.CustomerAccountStd.FirstOrDefault(c => c.Type == (int)type);
            Account account = accStd?.AccountStd?.Account;
            if (account != null)
            {
                accDto.AccountDim1Nr = Constants.ACCOUNTDIM_STANDARD;
                accDto.Account1Id = account.AccountId;
                accDto.Account1Nr = account.AccountNr;
                accDto.Account1Name = account.Name;
            }

            if (accStd != null && accStd.AccountInternal != null)
            {
                int dimCounter = 2;
                foreach (var accInt in accStd.AccountInternal.Where(a => a.Account != null && a.Account.AccountDim != null && a.Account.AccountDim.AccountDimNr != Constants.ACCOUNTDIM_STANDARD).OrderBy(a => a.Account.AccountDim.AccountDimNr))
                {
                    account = accInt.Account;

                    // TODO: Does not support dim numbers over 6!!!
                    if (dimCounter == 2)
                    {
                        accDto.AccountDim2Nr = account.AccountDim.AccountDimNr;
                        accDto.Account2Id = account.AccountId;
                        accDto.Account2Nr = account.AccountNr;
                        accDto.Account2Name = account.Name;
                    }
                    else if (dimCounter == 3)
                    {
                        accDto.AccountDim3Nr = account.AccountDim.AccountDimNr;
                        accDto.Account3Id = account.AccountId;
                        accDto.Account3Nr = account.AccountNr;
                        accDto.Account3Name = account.Name;
                    }
                    else if (dimCounter == 4)
                    {
                        accDto.AccountDim4Nr = account.AccountDim.AccountDimNr;
                        accDto.Account4Id = account.AccountId;
                        accDto.Account4Nr = account.AccountNr;
                        accDto.Account4Name = account.Name;
                    }
                    else if (dimCounter == 5)
                    {
                        accDto.AccountDim5Nr = account.AccountDim.AccountDimNr;
                        accDto.Account5Id = account.AccountId;
                        accDto.Account5Nr = account.AccountNr;
                        accDto.Account5Name = account.Name;
                    }
                    else if (dimCounter == 6)
                    {
                        accDto.AccountDim6Nr = account.AccountDim.AccountDimNr;
                        accDto.Account6Id = account.AccountId;
                        accDto.Account6Nr = account.AccountNr;
                        accDto.Account6Name = account.Name;
                    }

                    dimCounter++;
                }
            }
        }

        private static void AddAccountingSettingsRowDTO(InvoiceProduct product, InvoiceProductDTO dto, ProductAccountType type)
        {
            AccountingSettingsRowDTO accDto = new AccountingSettingsRowDTO()
            {
                Type = (int)type,
                Percent = 0
            };
            dto.AccountingSettings.Add(accDto);

            ProductAccountStd accStd = product.ProductAccountStd.FirstOrDefault(c => c.Type == (int)type);
            Account account = accStd?.AccountStd?.Account;
            if (account != null)
            {
                accDto.AccountDim1Nr = Constants.ACCOUNTDIM_STANDARD;
                accDto.Account1Id = account.AccountId;
                accDto.Account1Nr = account.AccountNr;
                accDto.Account1Name = account.Name;
            }

            if (accStd != null && accStd.AccountInternal != null)
            {
                int dimCounter = 2;
                foreach (var accInt in accStd.AccountInternal.Where(a => a.Account != null && a.Account.AccountDim != null && a.Account.AccountDim.AccountDimNr != Constants.ACCOUNTDIM_STANDARD).OrderBy(a => a.Account.AccountDim.AccountDimNr))
                {
                    account = accInt.Account;

                    // TODO: Does not support dim numbers over 6!!!
                    if (dimCounter == 2)
                    {
                        accDto.AccountDim2Nr = account.AccountDim.AccountDimNr;
                        accDto.Account2Id = account.AccountId;
                        accDto.Account2Nr = account.AccountNr;
                        accDto.Account2Name = account.Name;
                    }
                    else if (dimCounter == 3)
                    {
                        accDto.AccountDim3Nr = account.AccountDim.AccountDimNr;
                        accDto.Account3Id = account.AccountId;
                        accDto.Account3Nr = account.AccountNr;
                        accDto.Account3Name = account.Name;
                    }
                    else if (dimCounter == 4)
                    {
                        accDto.AccountDim4Nr = account.AccountDim.AccountDimNr;
                        accDto.Account4Id = account.AccountId;
                        accDto.Account4Nr = account.AccountNr;
                        accDto.Account4Name = account.Name;
                    }
                    else if (dimCounter == 5)
                    {
                        accDto.AccountDim5Nr = account.AccountDim.AccountDimNr;
                        accDto.Account5Id = account.AccountId;
                        accDto.Account5Nr = account.AccountNr;
                        accDto.Account5Name = account.Name;
                    }
                    else if (dimCounter == 6)
                    {
                        accDto.AccountDim6Nr = account.AccountDim.AccountDimNr;
                        accDto.Account6Id = account.AccountId;
                        accDto.Account6Nr = account.AccountNr;
                        accDto.Account6Name = account.Name;
                    }

                    dimCounter++;
                }
            }
        }

        private static void AddAccountingSettingDTO(InventoryWriteOffTemplate writeOffTemplate, InventoryWriteOffTemplateDTO dto, InventoryAccountType type)
        {
            AccountingSettingDTO accDto = new AccountingSettingDTO()
            {
                Type = (int)type,
            };
            dto.AccountingSettings.Add(accDto);

            InventoryAccountStd accStd = writeOffTemplate.InventoryAccountStd.FirstOrDefault(c => c.Type == (int)type);
            Account account = accStd?.AccountStd?.Account;
            if (account != null)
            {
                accDto.Account1Id = account.AccountId;
                accDto.Account1Nr = account.AccountNr;
                accDto.Account1Name = account.Name;
            }

            if (accStd != null && accStd.AccountInternal != null)
            {
                int dimCounter = 2;
                foreach (var accInt in accStd.AccountInternal.Where(a => a.Account != null && a.Account.AccountDim != null && a.Account.AccountDim.AccountDimNr != Constants.ACCOUNTDIM_STANDARD).OrderBy(a => a.Account.AccountDim.AccountDimNr))
                {
                    account = accInt.Account;

                    // TODO: Does not support dim numbers over 6!!!
                    if (dimCounter == 2)
                    {
                        accDto.Account2Id = account.AccountId;
                        accDto.Account2Nr = account.AccountNr;
                        accDto.Account2Name = account.Name;
                    }
                    else if (dimCounter == 3)
                    {
                        accDto.Account3Id = account.AccountId;
                        accDto.Account3Nr = account.AccountNr;
                        accDto.Account3Name = account.Name;
                    }
                    else if (dimCounter == 4)
                    {
                        accDto.Account4Id = account.AccountId;
                        accDto.Account4Nr = account.AccountNr;
                        accDto.Account4Name = account.Name;
                    }
                    else if (dimCounter == 5)
                    {
                        accDto.Account5Id = account.AccountId;
                        accDto.Account5Nr = account.AccountNr;
                        accDto.Account5Name = account.Name;
                    }
                    else if (dimCounter == 6)
                    {
                        accDto.Account6Id = account.AccountId;
                        accDto.Account6Nr = account.AccountNr;
                        accDto.Account6Name = account.Name;
                    }

                    dimCounter++;
                }
            }

        }
        public static IEnumerable<CustomerDTO> ToDTOs(this IEnumerable<Customer> l, bool includeContactAddresses, bool includeAccountingSettings, bool loadNote)
        {
            var dtos = new List<CustomerDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO(includeContactAddresses, includeAccountingSettings, loadNote));
                }
            }
            return dtos;
        }

        #endregion

        #region CustomerUser

        public static CustomerUserDTO ToDTO(this CustomerUser e)
        {
            if (e == null)
                return null;

            CustomerUserDTO dto = new CustomerUserDTO()
            {
                CustomerUserId = e.CustomerUserId,
                ActorCustomerId = e.ActorCustomerId,
                ActorCompanyId = e.ActorCompanyId,
                UserId = e.UserId,
                Main = e.Main,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State
            };

            // Extensions
            if (e.User != null)
            {
                dto.LoginName = e.User.LoginName;
                dto.Name = e.User.Name;
            }

            return dto;
        }

        #endregion

        #region CustomerIO

        public static SoftOne.Soe.Common.DTO.CustomerIODTO ToDTO(this CustomerIO e)
        {
            // DTO has 100 properties
            var dto = new CustomerIODTO()
            {
                CustomerIOId = e.CustomerIOId,
                ActorCompanyId = e.ActorCompanyId,
                Import = e.Import,
                Type = Enum.IsDefined(typeof(SoftOne.Soe.Common.Util.TermGroup_IOType), e.Type) ? (SoftOne.Soe.Common.Util.TermGroup_IOType)e.Type : SoftOne.Soe.Common.Util.TermGroup_IOType.Unknown,
                Status = Enum.IsDefined(typeof(SoftOne.Soe.Common.Util.TermGroup_IOStatus), e.Status) ? (SoftOne.Soe.Common.Util.TermGroup_IOStatus)e.Status : SoftOne.Soe.Common.Util.TermGroup_IOStatus.Unprocessed,
                Source = Enum.IsDefined(typeof(SoftOne.Soe.Common.Util.TermGroup_IOSource), e.Source) ? (SoftOne.Soe.Common.Util.TermGroup_IOSource)e.Source : SoftOne.Soe.Common.Util.TermGroup_IOSource.Unknown,
                BatchId = e.BatchId,
                GracePeriodDays = e.GracePeriodDays,
                DeliveryMethod = e.DeliveryMethod,
                DefaultWholeseller = e.DefaultWholeseller,
                CustomerState = e.CustomerState,
                OfferTemplate = e.OfferTemplate,
                OrderTemplate = e.OrderTemplate,
                BillingTemplate = e.BillingTemplate,
                AgreementTemplate = e.AgreementTemplate,
                AccountsReceivableAccountNr = e.AccountsReceivableAccountNr,
                AccountsReceivableAccountInternal1 = e.AccountsReceivableAccountInternal1,
                AccountsReceivableAccountInternal2 = e.AccountsReceivableAccountInternal2,
                AccountsReceivableAccountInternal3 = e.AccountsReceivableAccountInternal3,
                AccountsReceivableAccountInternal4 = e.AccountsReceivableAccountInternal4,
                AccountsReceivableAccountInternal5 = e.AccountsReceivableAccountInternal5,
                SalesAccountNr = e.SalesAccountNr,
                SalesAccountInternal1 = e.SalesAccountInternal1,
                SalesAccountInternal2 = e.SalesAccountInternal2,
                SalesAccountInternal3 = e.SalesAccountInternal3,
                SalesAccountInternal4 = e.SalesAccountInternal4,
                SalesAccountInternal5 = e.SalesAccountInternal5,
                VATAccountNr = e.VATAccountNr,
                VATCodeNr = e.VATCodeNr,
                CategoryCode1 = e.CategoryCode1,
                CategoryCode2 = e.CategoryCode2,
                CategoryCode3 = e.CategoryCode3,
                CategoryCode4 = e.CategoryCode4,
                CategoryCode5 = e.CategoryCode5,
                VatType = e.VatType,
                DeliveryCondition = e.DeliveryCondition,
                PaymentCondition = e.PaymentCondition,
                DefaultPriceListType = e.DefaultPriceListType,
                Currency = e.Currency,
                Country = e.Country,
                CustomerNr = e.CustomerNr,
                Name = e.Name,
                OrgNr = e.OrgNr,
                GLN = e.GLN,
                VatNr = e.VatNr,
                InvoiceReference = e.InvoiceReference,
                SupplierNr = e.SupplierNr,
                ManualAccounting = e.ManualAccounting,
                DiscountMerchandise = e.DiscountMerchandise,
                DiscountService = e.DiscountService,
                DisableInvoiceFee = e.DisableInvoiceFee,
                Note = e.Note,
                ShowNote = e.ShowNote,
                FinvoiceAddress = e.FinvoiceAddress,
                FinvoiceOperator = e.FinvoiceOperator,
                IsFinvoiceCustomer = e.IsFinvoiceCustomer,
                BlockNote = e.BlockNote,
                BlockOrder = e.BlockOrder,
                BlockInvoice = e.BlockInvoice,
                CreditLimit = e.CreditLimit,
                IsCashCustomer = e.IsCashCustomer,
                DistributionAddress = e.DistributionAddress,
                DistributionCoAddress = e.DistributionCoAddress,
                DistributionPostalCode = e.DistributionPostalCode,
                DistributionPostalAddress = e.DistributionPostalAddress,
                DistributionCountry = e.DistributionCountry,
                BillingAddress = e.BillingAddress,
                BillingCoAddress = e.BillingCoAddress,
                BillingPostalCode = e.BillingPostalCode,
                BillingPostalAddress = e.BillingPostalAddress,
                BillingCountry = e.BillingCountry,
                BoardHQAddress = e.BoardHQAddress,
                BoardHQCountry = e.BoardHQCountry,
                VisitingAddress = e.VisitingAddress,
                VisitingCoAddress = e.VisitingCoAddress,
                VisitingPostalCode = e.VisitingPostalCode,
                VisitingPostalAddress = e.VisitingPostalAddress,
                VisitingCountry = e.VisitingCountry,
                DeliveryAddress = e.DeliveryAddress,
                DeliveryCoAddress = e.DeliveryCoAddress,
                DeliveryPostalCode = e.DeliveryPostalCode,
                DeliveryPostalAddress = e.DeliveryPostalAddress,
                DeliveryCountry = e.DeliveryCountry,
                Email1 = e.Email1,
                Email2 = e.Email2,
                PhoneHome = e.PhoneHome,
                PhoneMobile = e.PhoneMobile,
                PhoneJob = e.PhoneJob,
                Fax = e.Fax,
                Webpage = e.Webpage,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = e.State,
                ErrorMessage = e.ErrorMessage,
                ImportHeadType = Enum.IsDefined(typeof(SoftOne.Soe.Common.Util.TermGroup_IOImportHeadType), e.ImportHeadType) ? (SoftOne.Soe.Common.Util.TermGroup_IOImportHeadType)e.ImportHeadType : SoftOne.Soe.Common.Util.TermGroup_IOImportHeadType.Unknown,
                StatusName = e.StatusName,
                VatTypeName = e.VatTypeName,
                SysLanguageId = e.SysLanguageId,
                ContactEmail = e.ContactEmail,
                ContactFirstName = e.ContactFirstName,
                ContactLastName = e.ContactLastName,
                InvoiceDeliveryEmail = e.InvoiceDeliveryEmail,
                ImportInvoiceDetailed = e.ImportInvoiceDetailed == true,
                InvoiceLabel = e.InvoiceLabel,
                // Skipping IsSelected
                // Skipping IsModified
                // Contains 98 properties
            };
            return dto;
        }

        public static IEnumerable<SoftOne.Soe.Common.DTO.CustomerIODTO> ToDTOs(this IEnumerable<CustomerIO> e)
        {
            return e.Select(s => s.ToDTO()).ToList();
        }

        public static SoftOne.Soe.Common.DTO.CustomerInvoiceIODTO ToDTO(this CustomerInvoiceHeadIO e, bool addProductRows)
        {
            var dto = new CustomerInvoiceIODTO()
            {
                CustomerInvoiceHeadIOId = e.CustomerInvoiceHeadIOId,
                InvoiceId = e.InvoiceId,
                CustomerInvoiceNr = e.CustomerInvoiceNr,
                SeqNr = e.SeqNr,
                OCR = e.OCR,
                ActorCompanyId = e.ActorCompanyId,
                Import = e.Import,
                Type = e.Type,
                Status = e.Status,
                Source = e.Source,
                OriginType = e.OriginType,
                OriginStatus = e.OriginStatus,
                BatchId = e.BatchId,
                CustomerId = e.CustomerId,
                PaymentCondition = e.PaymentCondition,
                CustomerNr = e.CustomerNr,
                CustomerName = e.CustomerName,
                RegistrationType = e.RegistrationType,
                InvoiceDate = e.InvoiceDate,
                DueDate = e.DueDate,
                VoucherDate = e.VoucherDate,
                ReferenceOur = e.ReferenceOur,
                ReferenceYour = e.ReferenceYour,
                CurrencyRate = e.CurrencyRate,
                CurrencyDate = e.CurrencyDate,
                TotalAmount = e.TotalAmount,
                TotalAmountCurrency = e.TotalAmountCurrency,
                SumAmount = e.SumAmount,
                SumAmountCurrency = e.SumAmountCurrency,
                VATAmount = e.VATAmount,
                VATAmountCurrency = e.VATAmountCurrency,
                PaidAmount = e.PaidAmount,
                PaidAmountCurrency = e.PaidAmountCurrency,
                RemainingAmount = e.RemainingAmount,
                FreightAmount = e.FreightAmount,
                FreightAmountCurrency = e.FreightAmountCurrency,
                InvoiceFee = e.InvoiceFee,
                InvoiceFeeCurrency = e.InvoiceFeeCurrency,
                CentRounding = e.CentRounding,
                FullyPayed = e.FullyPayed,
                PaymentNr = e.PaymentNr,
                VoucherNr = e.VoucherNr,
                CreateAccountingInXE = e.CreateAccountingInXE,
                Note = e.Note,
                BillingType = e.BillingType,
                Currency = e.Currency,
                TransferType = e.TransferType,
                ErrorMessage = e.ErrorMessage,
                ImportHeadType = e.ImportHeadType,
                BillingAddressAddress = e.BillingAddressAddress,
                BillingAddressCO = e.BillingAddressCO,
                BillingAddressPostNr = e.BillingAddressPostNr,
                BillingAddressCity = e.BillingAddressCity,
                DeliveryAddressAddress = e.DeliveryAddressAddress,
                DeliveryAddressPostNr = e.DeliveryAddressPostNr,
                DeliveryAddressCity = e.DeliveryAddressCity,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                UseFixedPriceArticle = e.UseFixedPriceArticle,
                VatRate1 = e.VatRate1,
                VatRate2 = e.VatRate2,
                VatRate3 = e.VatRate3,
                VatAmount1 = e.VatAmount1,
                VatAmount2 = e.VatAmount2,
                VatAmount3 = e.VatAmount3,
                StatusName = e.StatusName,
                BillingTypeName = e.BillingTypeName,
                InvoiceDeliveryType = e.InvoiceDeliveryType,
                InvoiceLabel = e.InvoiceLabel,
                ExternalId = e.ExternalId,
                PriceListTypeId = e.PriceListTypeId,
                PaymentConditionCode = e.PaymentConditionCode,
                ProjectNr = e.ProjectNr,
                WorkingDescription = e.WorkingDescription,
            };

            if (addProductRows && e.CustomerInvoiceRowIO.IsLoaded)
            {
                dto.InvoiceRows = e.CustomerInvoiceRowIO.ToDTOs().ToList();
            }

            return dto;
        }

        public static IEnumerable<SoftOne.Soe.Common.DTO.CustomerInvoiceIODTO> ToDTOs(this IEnumerable<CustomerInvoiceHeadIO> e, bool addProductRows)
        {
            return e.Select(s => s.ToDTO(addProductRows)).ToList();
        }

        public static SoftOne.Soe.Common.DTO.CustomerInvoiceRowIODTO ToDTO(this CustomerInvoiceRowIO e)
        {
            // DTO has 50 properties
            var dto = new CustomerInvoiceRowIODTO()
            {
                CustomerInvoiceRowIOId = e.CustomerInvoiceRowIOId,
                CustomerInvoiceHeadIOId = e.CustomerInvoiceHeadIOId,
                InvoiceId = e.InvoiceId.HasValue ? e.InvoiceId.Value : default(System.Int32), // Auto converted from type System.Nullable`1[System.Int32]
                InvoiceRowId = e.InvoiceRowId.HasValue ? e.InvoiceRowId.Value : default(System.Int32), // Auto converted from type System.Nullable`1[System.Int32]
                InvoiceNr = e.InvoiceNr,
                ActorCompanyId = e.ActorCompanyId,
                Import = e.Import,
                Type = Enum.IsDefined(typeof(SoftOne.Soe.Common.Util.TermGroup_IOType), e.Type) ? (SoftOne.Soe.Common.Util.TermGroup_IOType)e.Type : SoftOne.Soe.Common.Util.TermGroup_IOType.Unknown,
                Status = e.Status,
                Source = e.Source,
                BatchId = e.BatchId,
                CustomerRowType = Enum.IsDefined(typeof(SoftOne.Soe.Common.Util.SoeInvoiceRowType), e.CustomerRowType) ? (SoftOne.Soe.Common.Util.SoeInvoiceRowType)e.CustomerRowType : SoftOne.Soe.Common.Util.SoeInvoiceRowType.Unknown,
                ProductNr = e.ProductNr,
                ProductName = e.ProductName,
                Quantity = e.Quantity,
                UnitPrice = e.UnitPrice,
                Discount = e.Discount,
                Unit = e.Unit,
                ProductUnitId = e.ProductUnitId,
                VatRate = e.VatRate,
                AccountNr = e.AccountNr,
                AccountDim2Nr = e.AccountDim2Nr,
                AccountDim3Nr = e.AccountDim3Nr,
                AccountDim4Nr = e.AccountDim4Nr,
                AccountDim5Nr = e.AccountDim5Nr,
                AccountDim6Nr = e.AccountDim6Nr,
                PurchasePrice = e.PurchasePrice,
                PurchasePriceCurrency = e.PurchasePriceCurrency,
                Amount = e.Amount,
                AmountCurrency = e.AmountCurrency,
                VatAmount = e.VatAmount,
                VatAmountCurrency = e.VatAmountCurrency,
                DiscountAmount = e.DiscountAmount,
                DiscountAmountCurrency = e.DiscountAmountCurrency,
                MarginalIncome = e.MarginalIncome,
                MarginalIncomeCurrency = e.MarginalIncomeCurrency,
                SumAmount = e.SumAmount,
                SumAmountCurrency = e.SumAmountCurrency,
                Text = e.Text,
                ErrorMessage = e.ErrorMessage,
                ImportHeadType = e.ImportHeadType,
                RowNr = e.RowNr,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = e.State,
                StatusName = e.StatusName,
                // Skipping IsSelected
                // Skipping IsModified
                // Contains 48 properties
            };
            return dto;
        }

        public static IEnumerable<SoftOne.Soe.Common.DTO.CustomerInvoiceRowIODTO> ToDTOs(this IEnumerable<CustomerInvoiceRowIO> e)
        {
            return e.Select(s => s.ToDTO()).ToList();
        }

        #endregion

        #region DeliveryCondition

        public static DeliveryConditionDTO ToDTO(this DeliveryCondition e)
        {
            if (e == null)
                return null;

            DeliveryConditionDTO dto = new DeliveryConditionDTO()
            {
                DeliveryConditionId = e.DeliveryConditionId,
                ActorCompanyId = e.Company != null ? e.Company.ActorCompanyId : 0,  // TODO: Add foreign key to model
                Code = e.Code,
                Name = e.Name,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy
            };

            return dto;
        }

        public static IEnumerable<DeliveryConditionDTO> ToDTOs(this IEnumerable<DeliveryCondition> l)
        {
            var dtos = new List<DeliveryConditionDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static DeliveryConditionGridDTO ToGridDTO(this DeliveryCondition e)
        {
            if (e == null)
                return null;

            DeliveryConditionGridDTO dto = new DeliveryConditionGridDTO()
            {
                DeliveryConditionId = e.DeliveryConditionId,
                Code = e.Code,
                Name = e.Name,
            };

            return dto;
        }

        public static IEnumerable<DeliveryConditionGridDTO> ToGridDTOs(this IEnumerable<DeliveryCondition> l)
        {
            var dtos = new List<DeliveryConditionGridDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToGridDTO());
                }
            }
            return dtos;
        }

        #endregion

        #region DeliveryType

        public static DeliveryTypeDTO ToDTO(this DeliveryType e)
        {
            if (e == null)
                return null;

            var dto = new DeliveryTypeDTO
            {
                DeliveryTypeId = e.DeliveryTypeId,
                ActorCompanyId = e.Company != null ? e.Company.ActorCompanyId : 0,  // TODO: Add foreign key to model
                Code = e.Code,
                Name = e.Name,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy
            };

            return dto;
        }

        public static IEnumerable<DeliveryTypeDTO> ToDTOs(this IEnumerable<DeliveryType> l)
        {
            var dtos = new List<DeliveryTypeDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static DeliveryTypeGridDTO ToGridDTO(this DeliveryType e)
        {
            if (e == null)
                return null;

            DeliveryTypeGridDTO dto = new DeliveryTypeGridDTO()
            {
                DeliveryTypeId = e.DeliveryTypeId,
                Code = e.Code,
                Name = e.Name,
            };

            return dto;
        }

        public static IEnumerable<DeliveryTypeGridDTO> ToGridDTOs(this IEnumerable<DeliveryType> l)
        {
            var dtos = new List<DeliveryTypeGridDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToGridDTO());
                }
            }
            return dtos;
        }

        #endregion

        #region DistributionCode

        public static DistributionCodeHeadDTO ToDTO(this DistributionCodeHead e, bool setInUse = false)
        {
            if (e == null)
                return null;

            DistributionCodeHeadDTO dto = new DistributionCodeHeadDTO()
            {
                DistributionCodeHeadId = e.DistributionCodeHeadId,
                ActorCompanyId = e.ActorCompanyId,
                Name = e.Name,
                NoOfPeriods = e.NoOfPeriods,
                TypeId = e.Type,
                Periods = new List<DistributionCodePeriodDTO>(),
                ParentId = e.ParentId,
                SubType = e.SubType,
                OpeningHoursId = e.OpeningHoursId,
                AccountDimId = e.AccountDimId,
                FromDate = e.FromDate
            };

            if (setInUse)
            {
                if (!e.BudgetHead.IsLoaded)
                {
                    e.BudgetHead.Load();
                    DataProjectLogCollector.LogLoadedEntityInExtension("e.BudgetHead");
                }
                if (!e.DistributionCodePeriod1.IsLoaded)
                {
                    e.DistributionCodePeriod1.Load();
                    DataProjectLogCollector.LogLoadedEntityInExtension(" e.DistributionCodePeriod1");
                }

                dto.IsInUse = e.BudgetHead.Any(h => h.State == (int)SoeEntityState.Active) || e.DistributionCodePeriod1.Any();
            }

            if (e.DistributionCodePeriod != null && e.DistributionCodePeriod.Count > 0)
            {
                int number = 1;
                foreach (DistributionCodePeriod p in e.DistributionCodePeriod)
                {
                    DistributionCodePeriodDTO pDto = new DistributionCodePeriodDTO()
                    {
                        Comment = p.Comment,
                        DistributionCodePeriodId = p.DistributionCodePeriodId,
                        Percent = p.Percent,
                        Number = number,
                        ParentToDistributionCodePeriodId = p.ParentToDistributionCodeHeadId,
                    };

                    dto.Periods.Add(pDto);
                    number++;
                }
            }

            return dto;
        }

        public static IEnumerable<DistributionCodeHeadDTO> ToDTOs(this IEnumerable<DistributionCodeHead> l, bool setInUse = false)
        {
            var dtos = new List<DistributionCodeHeadDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO(setInUse));
                }
            }
            return dtos;
        }

        #endregion

        #region DistributionCodePeriod

        public static DistributionCodePeriodDTO ToDTO(this DistributionCodePeriod p, int number)
        {
            if (p == null)
                return null;

            DistributionCodePeriodDTO dto = new DistributionCodePeriodDTO()
            {
                DistributionCodePeriodId = p.DistributionCodePeriodId,
                Comment = p.Comment,
                Percent = p.Percent,
                Number = number,
                ParentToDistributionCodePeriodId = p.ParentToDistributionCodeHeadId,
            };

            return dto;
        }

        public static List<DistributionCodePeriodDTO> ToDTOs(this IEnumerable<DistributionCodePeriod> l)
        {
            var dtos = new List<DistributionCodePeriodDTO>();
            if (l != null)
            {
                int number = 1;
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO(number));
                    number++;
                }
            }
            return dtos;
        }

        #endregion


        #region Document

        public static DocumentDTO ToDocumentDTO(this DataStorage e)
        {
            if (e == null)
                return null;

            DocumentDTO dto = new DocumentDTO()
            {
                DataStorageId = e.DataStorageId,
                UserId = e.UserId,
                Name = e.Name,
                Description = StringUtility.NullToEmpty(e.Description),
                FileName = StringUtility.NullToEmpty(e.FileName),
                Extension = StringUtility.NullToEmpty(e.Extension),
                FileSize = e.FileSize,
                Folder = StringUtility.NullToEmpty(e.Folder),
                ValidFrom = e.ValidFrom,
                ValidTo = e.ValidTo,
                NeedsConfirmation = e.NeedsConfirmation,
                Created = e.Created
            };

            // Relations
            if (e.DataStorageRecord != null)
            {
                // Connected message groups are stored as DataStorageRecords where RecordId = MessageGroupId.
                dto.Records = e.DataStorageRecord.Where(r => r.Type != (int)SoeDataStorageRecordType.MessageGroup).ToDTOs();
                dto.MessageGroupIds = e.DataStorageRecord.Where(r => r.Type == (int)SoeDataStorageRecordType.MessageGroup).Select(r => r.RecordId).ToList();
            }

            if (e.DataStorageRecipient != null)
                dto.Recipients = e.DataStorageRecipient.Where(r => r.State == (int)SoeEntityState.Active).ToDTOs();

            return dto;
        }

        public static List<DocumentDTO> ToDocumentDTOs(this IEnumerable<DataStorage> l)
        {
            List<DocumentDTO> dtos = new List<DocumentDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDocumentDTO());
                }
            }
            return dtos;
        }

        public static DocumentDTO ToDocumentDTO(this DataStorageDTO e)
        {
            if (e == null)
                return null;

            DocumentDTO dto = new DocumentDTO()
            {
                DataStorageId = e.DataStorageId,
                UserId = e.UserId,
                Name = e.Name,
                Description = StringUtility.NullToEmpty(e.Description),
                FileName = StringUtility.NullToEmpty(e.FileName),
                Extension = StringUtility.NullToEmpty(e.Extension),
                FileSize = e.FileSize,
                Folder = StringUtility.NullToEmpty(e.Folder),
                ValidFrom = e.ValidFrom,
                ValidTo = e.ValidTo,
                NeedsConfirmation = e.NeedsConfirmation,
                Created = e.Created
            };

            // Relations
            if (e.DataStorageRecords != null)
            {
                // Connected message groups are storde as DataStorageRecords where RecordId = MessageGroupId.
                dto.Records = e.DataStorageRecords.Where(r => r.Type != SoeDataStorageRecordType.MessageGroup).ToList();
                dto.MessageGroupIds = e.DataStorageRecords.Where(r => r.Type == SoeDataStorageRecordType.MessageGroup).Select(r => r.RecordId).ToList();
            }

            if (e.DataStorageRecipients != null)
                dto.Recipients = e.DataStorageRecipients.Where(r => r.State == SoeEntityState.Active).ToList();

            return dto;
        }

        public static List<DocumentDTO> ToDocumentDTOs(this IEnumerable<DataStorageDTO> l)
        {
            List<DocumentDTO> dtos = new List<DocumentDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDocumentDTO());
                }
            }
            return dtos;
        }

        #endregion

        #region EDI/Scanning

        #region CompanyEDi

        public static CompanyEdiDTO ToDTO(this CompanyEdi e)
        {
            if (e == null)
                return null;

            CompanyEdiDTO dto = new CompanyEdiDTO()
            {
                CompanyEdiId = e.CompanyEdiId,
                ActorCompanyId = e.ActorCompanyId,
                CompanyName = e.Company != null ? e.Company.Name : String.Empty,
                Type = e.Type,
                Source = e.Address,
                Username = e.Username,
                Password = e.Password,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State
            };

            return dto;
        }

        public static IEnumerable<CompanyEdiDTO> ToDTOs(this IEnumerable<CompanyEdi> l)
        {
            var dtos = new List<CompanyEdiDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        #endregion

        #region EdiEntry

        public static EdiEntryDTO ToDTO(this EdiEntry e, bool includeScanningEntryInvoice, bool includePdfAndXML = true)
        {
            if (e == null)
                return null;

            var dto = new EdiEntryDTO()
            {
                //Edi
                EdiEntryId = e.EdiEntryId,
                ActorCompanyId = e.ActorCompanyId,
                Type = (TermGroup_EDISourceType)e.Type,
                Status = (TermGroup_EDIStatus)e.Status,
                MessageType = (TermGroup_EdiMessageType)e.MessageType,
                SysWholesellerId = e.SysWholesellerId,
                WholesellerName = e.WholesellerName,
                BuyerId = e.BuyerId,
                BuyerReference = e.BuyerReference,
                BillingType = (TermGroup_BillingType)e.BillingType,
                VatRate = e.VatRate,
                PostalGiro = e.PostalGiro,
                BankGiro = e.BankGiro,
                OCR = e.OCR,
                IBAN = e.IBAN,
                ErrorCode = e.ErrorCode,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State,

                //Scanning
                ScanningEntryArrivalId = e.ScanningEntryArrivalId,
                ScanningEntryInvoiceId = e.ScanningEntryInvoiceId,

                //Dates
                Date = e.Date,
                InvoiceDate = e.InvoiceDate,
                DueDate = e.DueDate,

                //Sum
                Sum = e.Sum,
                SumCurrency = e.SumCurrency,
                SumVat = e.SumVat,
                SumVatCurrency = e.SumVatCurrency,

                //Currency
                CurrencyId = e.CurrencyId,
                CurrencyRate = e.CurrencyRate,
                CurrencyDate = e.CurrencyDate,

                //Order
                OrderId = e.OrderId,
                OrderStatus = (TermGroup_EDIOrderStatus)e.OrderStatus,
                OrderNr = e.OrderNr,

                //Invoice
                InvoiceId = e.InvoiceId,
                InvoiceStatus = (TermGroup_EDIInvoiceStatus)e.InvoiceStatus,
                InvoiceNr = e.InvoiceNr,
                SeqNr = e.SeqNr,

                //Supplier
                ActorSupplierId = e.ActorSupplierId,
                SellerOrderNr = e.SellerOrderNr,
                HasPDF = e.PDF != null
            };

            if (includePdfAndXML)
            {
                dto.XML = e.XML;
                dto.PDF = e.PDF;
                dto.FileName = e.FileName;
            }

            // Extensions
            if (includeScanningEntryInvoice && e.ScanningEntryInvoice != null)
                dto.ScanningEntryInvoice = e.ScanningEntryInvoice.ToDTO();

            return dto;
        }

        public static IEnumerable<EdiEntryDTO> ToDTOs(this IEnumerable<EdiEntry> l, bool includeScanningEntry)
        {
            var dtos = new List<EdiEntryDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO(includeScanningEntry));
                }
            }
            return dtos;
        }

        public static EdiEntryImageDTO ToImageDTO(this EdiEntry e, bool loadImage)
        {
            if (e == null)
                return null;

            EdiEntryImageDTO dto = new EdiEntryImageDTO()
            {
                EdiEntryId = e.EdiEntryId,
                Type = (TermGroup_EDISourceType)e.Type,
            };


            if ((TermGroup_EDISourceType)e.Type == TermGroup_EDISourceType.EDI)
            {
                if (loadImage)
                    dto.Image = e.PDF;

                dto.HasImage = e.PDF != null;
            }
            else if ((TermGroup_EDISourceType)e.Type == TermGroup_EDISourceType.Scanning)
            {
                if (loadImage)
                {
                    if (!e.ScanningEntryInvoiceReference.IsLoaded)
                    {
                        e.ScanningEntryInvoiceReference.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("e.ScanningEntryInvoiceReference");
                    }

                    dto.Image = e.ScanningEntryInvoice.Image;
                }

                if (e.ScanningEntryInvoiceReference.IsLoaded)
                    dto.HasImage = e.ScanningEntryInvoice.Image != null;
            }

            return dto;
        }

        #endregion

        #region ScanningEntry

        public static ScanningEntryDTO ToDTO(this ScanningEntry e)
        {
            if (e == null)
                return null;

            if (!e.IsAdded() && !e.ScanningEntryRow.IsLoaded)
            {
                e.ScanningEntryRow.Load();
                DataProjectLogCollector.LogLoadedEntityInExtension("e.ScanningEntryRow");
            }

            ScanningEntryDTO dto = new ScanningEntryDTO()
            {
                ScanningEntryId = e.ScanningEntryId,
                ActorCompanyId = e.ActorCompanyId,
                BatchId = e.BatchId,
                CompanyId = e.CompanyId,
                Type = e.Type,
                MessageType = (TermGroup_ScanningMessageType)e.MessageType,
                Status = (TermGroup_ScanningStatus)e.Status,
                Image = e.Image,
                XML = e.XML,
                ErrorCode = e.ErrorCode,
                OperatorMessage = e.OperatorMessage,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State,

                ScanningEntryRow = e.ScanningEntryRow.Where(r => r.State == (int)SoeEntityState.Active).ToDTOs(),
            };


            return dto;
        }

        public static IEnumerable<ScanningEntryDTO> ToDTOs(this IEnumerable<ScanningEntry> l)
        {
            var dtos = new List<ScanningEntryDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static ScanningEntryRow GetScanningEntryRow(this ScanningEntry e, ScanningEntryRowType type)
        {
            return e.ScanningEntryRow?.FirstOrDefault(i => i.Type == (int)type);
        }

        public static string GetScanningEntryRowStringValue(this ScanningEntry e, ScanningEntryRowType type)
        {
            var row = e.GetScanningEntryRow(type);
            return row != null ? row.Text : String.Empty;
        }

        public static int GetScanningEntryRowIntValue(this ScanningEntry e, ScanningEntryRowType type)
        {
            var row = e.GetScanningEntryRow(type);
            return row != null ? NumberUtility.ToInteger(row.Text) : 0;
        }

        public static decimal GetScanningEntryRowDecimalValue(this ScanningEntry e, ScanningEntryRowType type)
        {
            var row = e.GetScanningEntryRow(type);
            return row != null ? NumberUtility.ToDecimal(row.Text) : Decimal.Zero;
        }

        public static bool GetScanningEntryRowBoolValue(this ScanningEntry e, ScanningEntryRowType type)
        {
            var row = e.GetScanningEntryRow(type);
            return row != null && StringUtility.GetBool(row.Text);
        }

        public static DateTime GetScanningEntryRowDateValue(this ScanningEntry e, ScanningEntryRowType type)
        {
            var row = e.GetScanningEntryRow(type);
            return row != null ? CalendarUtility.GetDateTime(row.Text) : CalendarUtility.DATETIME_DEFAULT;
        }

        public static DateTime? GetScanningEntryRowNullableDateValue(this ScanningEntry e, ScanningEntryRowType type)
        {
            var row = e.GetScanningEntryRow(type);
            return row != null ? CalendarUtility.GetNullableDateTime(row.Text) : (DateTime?)null;
        }

        public static TermGroup_ScanningInterpretation GetInterpretationConfidence(this ScanningEntryRow e)
        {
            return e != null ? Validator.ValidateScanningEntryRow(null, e.ValidationError) : TermGroup_ScanningInterpretation.ValueNotFound;
        }

        #region Interpretation

        public static TermGroup_ScanningInterpretation GetScanningInterpretation(this ScanningEntry e, ScanningEntryRowType type)
        {
            var row = e.GetScanningEntryRow(type);
            return row != null ? Validator.ValidateScanningEntryRow(row.NewText, row.ValidationError) : TermGroup_ScanningInterpretation.ValueNotFound;
        }

        public static TermGroup_ScanningInterpretation GetScanningInterpretation(this ScanningEntryRowView e, ScanningEntryRowType type)
        {
            return e != null ? Validator.ValidateScanningEntryRow(e.NewText, e.ValidationError) : TermGroup_ScanningInterpretation.ValueNotFound;
        }

        public static bool IsAllRowsValid(this ScanningEntry e)
        {
            if (e.ScanningEntryRow == null)
                return false;

            List<int> validTypes = new List<int>()
            {
                (int)ScanningEntryRowType.IsCreditInvoice,
                (int)ScanningEntryRowType.InvoiceNr,
                (int)ScanningEntryRowType.InvoiceDate,
                (int)ScanningEntryRowType.DueDate,
                (int)ScanningEntryRowType.OrderNr,
                (int)ScanningEntryRowType.ReferenceYour,
                (int)ScanningEntryRowType.ReferenceOur,
                (int)ScanningEntryRowType.VatAmount,
                (int)ScanningEntryRowType.TotalAmountIncludeVat,
                (int)ScanningEntryRowType.CurrencyCode,
            };

            List<ScanningEntryRow> rows = e.ScanningEntryRow.Where(i => i.State == (int)SoeEntityState.Active && validTypes.Contains(i.Type)).ToList();
            return rows.Count > 0 && rows.Count == rows.Count(i => i.ValueIsValid());
        }

        public static bool ValueIsValid(this ScanningEntryRow e)
        {
            return e.ValidationError == ((int)TermGroup_ScanningInterpretation.ValueIsValid).ToString();
        }

        public static bool ValueIsUnsettled(this ScanningEntryRow e)
        {
            return e.ValidationError == ((int)TermGroup_ScanningInterpretation.ValueIsUnsettled).ToString();
        }

        public static bool ValueNotFound(this ScanningEntryRow e)
        {
            return e.ValidationError == ((int)TermGroup_ScanningInterpretation.ValueNotFound).ToString();
        }

        public static SupplierInvoiceInterpretationDTO ToSupplierInterpretationDTO(this EdiEntry e)
        {
            var scanningEntry = e.ScanningEntryInvoice;
            var interpretation = new SupplierInvoiceInterpretationDTO();
            interpretation.Metadata = new MetadataDTO
            {
                ArrivalTime = e.Created ?? CalendarUtility.DATETIME_DEFAULT,
                RawResponse = e.XML,
                Provider = "ReadSoft",
            };
            interpretation.Context = new ContextDTO
            {
                EdiEntryId = e.EdiEntryId,
                ScanningEntryId = e.ScanningEntryInvoiceId,
            };

            interpretation.InvoiceNumber = InterpretationValueFactory.InterpretedString(e.InvoiceNr, scanningEntry.GetScanningInterpretation(ScanningEntryRowType.InvoiceNr));
            interpretation.SupplierId = InterpretationValueFactory.InterpretedInt(e.ActorSupplierId, scanningEntry.GetScanningInterpretation(ScanningEntryRowType.OrgNr));
            interpretation.Description = InterpretationValueFactory.NoneInterpretedString();

            interpretation.InvoiceDate = InterpretationValueFactory.InterpretedDate(e.InvoiceDate, scanningEntry.GetScanningInterpretation(ScanningEntryRowType.InvoiceDate));
            interpretation.DueDate = InterpretationValueFactory.InterpretedDate(e.DueDate, scanningEntry.GetScanningInterpretation(ScanningEntryRowType.DueDate));

            interpretation.PaymentReferenceNumber = InterpretationValueFactory.InterpretedString(e.OCR, scanningEntry.GetScanningInterpretation(ScanningEntryRowType.OCR));

            var ourReference = scanningEntry.GetScanningEntryRow(ScanningEntryRowType.ReferenceOur);
            interpretation.SellerContactName = InterpretationValueFactory.InterpretedString(ourReference?.Text, scanningEntry.GetScanningInterpretation(ScanningEntryRowType.ReferenceOur));
            var yourReference = scanningEntry.GetScanningEntryRow(ScanningEntryRowType.ReferenceYour);
            interpretation.BuyerContactName = InterpretationValueFactory.InterpretedString(yourReference?.Text, scanningEntry.GetScanningInterpretation(ScanningEntryRowType.ReferenceYour));
            var orderNumber = scanningEntry.GetScanningEntryRow(ScanningEntryRowType.OrderNr);
            interpretation.BuyerOrderNumber = InterpretationValueFactory.InterpretedString(orderNumber?.Text, scanningEntry.GetScanningInterpretation(ScanningEntryRowType.OrderNr));
            interpretation.BuyerReference = InterpretationValueFactory.NoneInterpretedString();

            var isCreditInvoice = scanningEntry.GetScanningEntryRow(ScanningEntryRowType.IsCreditInvoice);
            interpretation.IsCreditInvoice = InterpretationValueFactory.InterpretedBool(scanningEntry.GetScanningEntryRowBoolValue(ScanningEntryRowType.IsCreditInvoice), isCreditInvoice.GetInterpretationConfidence());
            interpretation.AmountIncVat = InterpretationValueFactory.InterpretedDecimal(e.Sum, scanningEntry.GetScanningInterpretation(ScanningEntryRowType.TotalAmountIncludeVat));
            interpretation.AmountIncVatCurrency = InterpretationValueFactory.InterpretedDecimal(e.SumCurrency, scanningEntry.GetScanningInterpretation(ScanningEntryRowType.TotalAmountIncludeVat));
            interpretation.VatAmount = InterpretationValueFactory.InterpretedDecimal(e.SumVat, scanningEntry.GetScanningInterpretation(ScanningEntryRowType.VatAmount));
            interpretation.VatAmountCurrency = InterpretationValueFactory.InterpretedDecimal(e.SumVatCurrency, scanningEntry.GetScanningInterpretation(ScanningEntryRowType.VatAmount));
            interpretation.AmountExVat = InterpretationValueFactory.DerivedDecimal(e.Sum - e.SumVat);
            interpretation.AmountExVatCurrency = InterpretationValueFactory.DerivedDecimal(e.Sum - e.SumVat);
            interpretation.VatRatePercent = InterpretationValueFactory.InterpretedDecimal(e.VatRate, scanningEntry.GetScanningInterpretation(ScanningEntryRowType.VatRate));

            var currencyCode = scanningEntry.GetScanningEntryRow(ScanningEntryRowType.CurrencyCode);
            interpretation.CurrencyCode = InterpretationValueFactory.InterpretedString(currencyCode?.Text, currencyCode?.GetInterpretationConfidence());
            interpretation.CurrencyId = InterpretationValueFactory.InterpretedInt(e.CurrencyId, currencyCode?.GetInterpretationConfidence());
            interpretation.CurrencyRate = InterpretationValueFactory.DerivedDecimal(e.CurrencyRate);
            interpretation.CurrencyDate = InterpretationValueFactory.DerivedDate(e.CurrencyDate);

            interpretation.BankAccountBG = InterpretationValueFactory.InterpretedString(e.BankGiro, scanningEntry.GetScanningInterpretation(ScanningEntryRowType.Bankgiro));
            interpretation.BankAccountPG = InterpretationValueFactory.InterpretedString(e.PostalGiro, scanningEntry.GetScanningInterpretation(ScanningEntryRowType.Plusgiro));
            interpretation.BankAccountIBAN = InterpretationValueFactory.InterpretedString(e.IBAN, scanningEntry.GetScanningInterpretation(ScanningEntryRowType.IBAN));

            var orgNr = scanningEntry.GetScanningEntryRow(ScanningEntryRowType.OrgNr);
            interpretation.OrgNumber = InterpretationValueFactory.InterpretedString(orgNr?.Text, orgNr?.GetInterpretationConfidence());
            interpretation.Email = InterpretationValueFactory.NoneInterpretedString();

            interpretation.DeliveryCost = InterpretationValueFactory.NoneInterpretedDecimal();
            interpretation.AmountRounding = InterpretationValueFactory.NoneInterpretedDecimal();
            interpretation.SupplierName = InterpretationValueFactory.NoneInterpretedString();
            interpretation.AccountingRows = InterpretationValueFactory.NoneInterpretedAccountingRows();

            return interpretation;
        }

        #endregion

        #region HeaderFields

        #region BillingType

        public static TermGroup_BillingType GetBillingType(this ScanningEntry e, TermGroup_BillingType defaultBillingType)
        {
            var row = e.GetScanningEntryRow(ScanningEntryRowType.IsCreditInvoice);
            if (row == null)
                return defaultBillingType;

            return StringUtility.GetBool(row.Text) ? TermGroup_BillingType.Credit : TermGroup_BillingType.Debit;
        }

        public static bool IsCredit(this ScanningEntry e)
        {
            return e.GetBillingType(TermGroup_BillingType.Debit) == TermGroup_BillingType.Credit;
        }

        public static bool IsBillingTypeChanged(this ScanningEntry e, TermGroup_BillingType billingType)
        {
            return e.IsCredit() != (billingType == TermGroup_BillingType.Credit);
        }

        #endregion

        #region InvoiceNr

        public static string GetInvoiceNr(this ScanningEntry e)
        {
            return e.GetScanningEntryRowStringValue(ScanningEntryRowType.InvoiceNr);
        }

        public static bool IsInvoiceNrChanged(this ScanningEntry e, string invoiceNr)
        {
            return !StringUtility.IsEqual(e.GetInvoiceNr(), invoiceNr);
        }

        #endregion

        #region InvoiceDate

        public static DateTime? GetInvoiceDate(this ScanningEntry e)
        {
            return e.GetScanningEntryRowNullableDateValue(ScanningEntryRowType.InvoiceDate);
        }

        public static bool IsInvoiceDateChanged(this ScanningEntry e, DateTime? value)
        {
            return e.GetInvoiceDate().HasValue != value.HasValue || e.GetInvoiceDate() != value;
        }

        #endregion

        #region DueDate

        public static DateTime? GetDueDate(this ScanningEntry e)
        {
            return e.GetScanningEntryRowNullableDateValue(ScanningEntryRowType.DueDate);
        }

        public static bool IsDueDateChanged(this ScanningEntry e, DateTime? value)
        {
            return e.GetDueDate().HasValue != value.HasValue || e.GetDueDate() != value;
        }

        #endregion

        #region OrderNr

        public static string GetOrderNr(this ScanningEntry e)
        {
            return e.GetScanningEntryRowStringValue(ScanningEntryRowType.OrderNr);
        }

        public static bool IsOrderNrChanged(this ScanningEntry e, string value)
        {
            return !StringUtility.IsEqual(e.GetOrderNr(), value);
        }

        #endregion

        #region ReferenceYour

        public static string GetReferenceYour(this ScanningEntry e)
        {
            return e.GetScanningEntryRowStringValue(ScanningEntryRowType.ReferenceYour);
        }

        public static bool IsReferenceYourChanged(this ScanningEntry e, string value)
        {
            return !StringUtility.IsEqual(e.GetReferenceYour(), value);
        }

        #endregion

        #region ReferenceOur

        public static string GetReferenceOur(this ScanningEntry e)
        {
            return e.GetScanningEntryRowStringValue(ScanningEntryRowType.ReferenceOur);
        }

        public static bool IsReferenceOurChanged(this ScanningEntry e, string value)
        {
            return !StringUtility.IsEqual(e.GetReferenceOur(), value);
        }

        #endregion

        #region TotalAmountIncludeVat

        public static decimal GetTotalAmountIncludeVat(this ScanningEntry e)
        {
            return e.GetScanningEntryRowDecimalValue(ScanningEntryRowType.TotalAmountIncludeVat);
        }

        public static bool IsTotalAmountIncludeVatChanged(this ScanningEntry e, decimal value)
        {
            return e.GetTotalAmountIncludeVat() != value;
        }

        #endregion

        #region AmountExcludeVat

        public static decimal GetTotalAmountExludeVat(this ScanningEntry e)
        {
            return e.GetScanningEntryRowDecimalValue(ScanningEntryRowType.TotalAmountExludeVat);
        }

        public static bool IsTotalAmountExludeVatChanged(this ScanningEntry e, decimal value)
        {
            return decimal.Round(e.GetTotalAmountExludeVat() - value, 2) > 1;
        }

        #endregion

        #region VatAmount

        public static decimal GetVatAmount(this ScanningEntry e)
        {
            return e.GetScanningEntryRowDecimalValue(ScanningEntryRowType.VatAmount);
        }

        public static bool IsVatAmountChanged(this ScanningEntry e, decimal value)
        {
            return e.GetVatAmount() != value;
        }

        #endregion

        #region CurrencyCode

        public static string GetCurrencyCode(this ScanningEntry e)
        {
            return e.GetScanningEntryRowStringValue(ScanningEntryRowType.CurrencyCode);
        }

        public static bool IsCurrencyCodeChanged(this ScanningEntry e, string value)
        {
            return !StringUtility.IsEqual(e.GetCurrencyCode(), value);
        }

        #endregion

        #region OCRNr

        public static string GetOCRNr(this ScanningEntry e)
        {
            return e.GetScanningEntryRowStringValue(ScanningEntryRowType.OCR);
        }

        public static bool IsOCRNrChanged(this ScanningEntry e, string value)
        {
            return !StringUtility.IsEqual(e.GetOCRNr(), value);
        }

        #endregion

        #region Plusgiro

        public static string GetPlusgiro(this ScanningEntry e)
        {
            return e.GetScanningEntryRowStringValue(ScanningEntryRowType.Plusgiro);
        }

        public static bool IsPlusgiroChanged(this ScanningEntry e, string value)
        {
            return !StringUtility.IsEqual(e.GetPlusgiro(), value);
        }

        #endregion

        #region Bankgiro

        public static string GetBankgiro(this ScanningEntry e)
        {
            return e.GetScanningEntryRowStringValue(ScanningEntryRowType.Bankgiro);
        }

        public static bool IsBankgiroChanged(this ScanningEntry e, string value)
        {
            return !StringUtility.IsEqual(e.GetBankgiro(), value);
        }

        #endregion

        #region OrgNr

        public static string GetOrgNr(this ScanningEntry e)
        {
            return e.GetScanningEntryRowStringValue(ScanningEntryRowType.OrgNr);
        }

        public static bool IsOrgNrChanged(this ScanningEntry e, string value)
        {
            return !StringUtility.IsEqual(e.GetOrgNr(), value);
        }

        #endregion

        #region IBAN

        public static string GetIBAN(this ScanningEntry e)
        {
            return e.GetScanningEntryRowStringValue(ScanningEntryRowType.IBAN);
        }

        public static bool IsIBANChanged(this ScanningEntry e, string value)
        {
            return !StringUtility.IsEqual(e.GetIBAN(), value);
        }

        #endregion

        #region VatRate

        public static decimal? GetVatRate(this ScanningEntry e)
        {
            return NumberUtility.GetDecimalRemovePercentageSign(e.GetScanningEntryRowStringValue(ScanningEntryRowType.VatRate));
        }

        public static bool IsVatRateChanged(this ScanningEntry e, decimal? value)
        {
            return !NumberUtility.IsEqual(e.GetVatRate(), value);
        }

        #endregion

        #region VatNr

        public static string GetVatNr(this ScanningEntry e)
        {
            return e.GetScanningEntryRowStringValue(ScanningEntryRowType.VatNr);
        }

        public static bool IsVatNrChanged(this ScanningEntry e, string value)
        {
            return !StringUtility.IsEqual(e.GetVatNr(), value);
        }

        #endregion

        #region FreightAmount

        public static decimal GetFreightAmount(this ScanningEntry e)
        {
            return e.GetScanningEntryRowDecimalValue(ScanningEntryRowType.FreightAmount);
        }

        public static bool IsFreightAmountChanged(this ScanningEntry e, decimal value)
        {
            return e.GetFreightAmount() != value;
        }

        #endregion

        #region CentRounding

        public static decimal GetCentRounding(this ScanningEntry e)
        {
            return e.GetScanningEntryRowDecimalValue(ScanningEntryRowType.CentRounding);
        }

        public static bool IsCentRoundingChanged(this ScanningEntry e, decimal value)
        {
            return e.GetCentRounding() != value;
        }

        #endregion

        #endregion

        #endregion

        #region ScanningEntryRow

        public static ScanningEntryRowDTO ToDTO(this ScanningEntryRow e)
        {
            if (e == null)
                return null;

            ScanningEntryRowDTO dto = new ScanningEntryRowDTO()
            {
                Type = (ScanningEntryRowType)e.Type,
                TypeName = e.TypeName,
                Name = e.Name,
                Text = e.Text,
                Format = e.Format,
                ValidationError = e.ValidationError,
                Position = e.Position,
                PageNumber = e.PageNumber,
                NewText = e.NewText,

                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State,
            };

            return dto;
        }

        public static IEnumerable<ScanningEntryRowDTO> ToDTOs(this IEnumerable<ScanningEntryRow> l)
        {
            var dtos = new List<ScanningEntryRowDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        #endregion

        #endregion

        #region EdiConnection

        public static EdiConnectionDTO ToDTO(this EdiConnection e)
        {
            if (e == null)
                return null;

            var dto = new EdiConnectionDTO()
            {
                EdiConnectionId = e.EdiConnectionId,
                WholesellerCustomerNr = e.BuyerNr,
            };

            return dto;
        }

        public static IEnumerable<EdiConnectionDTO> ToDTOs(this IEnumerable<EdiConnection> l)
        {
            if (l != null)
                return l.GroupBy(k => k.BuyerNr).Select(s => s.First().ToDTO()).ToList();

            return null;
        }

        #endregion

        #region GrossProfitCode

        public static IEnumerable<GrossProfitCodeGridDTO> ToGridDTOs(this IEnumerable<GrossProfitCode> l)
        {
            var dtos = new List<GrossProfitCodeGridDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToGridDTO());
                }
            }
            return dtos;
        }

        public static IEnumerable<GrossProfitCodeDTO> ToDTOs(this IEnumerable<GrossProfitCode> l)
        {
            var dtos = new List<GrossProfitCodeDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static GrossProfitCodeGridDTO ToGridDTO(this GrossProfitCode e)
        {
            if (e == null)
                return null;

            if (e.AccountYear == null)
                return null;

            GrossProfitCodeGridDTO dto = new GrossProfitCodeGridDTO()
            {
                GrossProfitCodeId = e.GrossProfitCodeId,
                AccountYearId = e.AccountYearId,
                AccountDateFrom = e.AccountYear.From,
                AccountDateTo = e.AccountYear.To,
                Code = e.Code,
                Name = e.Name,
                Description = e.Description
            };

            return dto;
        }

        public static GrossProfitCodeDTO ToDTO(this GrossProfitCode e)
        {
            if (e == null)
                return null;

            if (e.AccountYear == null)
                return null;

            GrossProfitCodeDTO dto = new GrossProfitCodeDTO()
            {
                GrossProfitCodeId = e.GrossProfitCodeId,
                ActorCompanyId = e.ActorCompanyId,
                AccountYearId = e.AccountYearId,
                AccountDateFrom = e.AccountYear.From,
                AccountDateTo = e.AccountYear.To,
                AccountDimId = e.AccountDimId,
                AccountId = e.AccountId,
                Code = e.Code,
                Name = e.Name,
                Description = e.Description,
                OpeningBalance = e.OpeningBalance,
                Period1 = e.Period1,
                Period2 = e.Period2,
                Period3 = e.Period3,
                Period4 = e.Period4,
                Period5 = e.Period5,
                Period6 = e.Period6,
                Period7 = e.Period7,
                Period8 = e.Period8,
                Period9 = e.Period9,
                Period10 = e.Period10,
                Period11 = e.Period11,
                Period12 = e.Period12,
                Period13 = e.Period13,
                Period14 = e.Period14,
                Period15 = e.Period15,
                Period16 = e.Period16,
                Period17 = e.Period17,
                Period18 = e.Period18,
            };

            return dto;
        }

        #endregion

        #region Inventory

        #region Inventory

        public static InventoryDTO ToDTO(this Inventory e, bool includeAccountSettings, bool includeAngularAccountSettings)
        {
            if (e == null)
                return null;

            #region Try load

            try
            {
                if (includeAccountSettings && !e.IsAdded())
                {
                    if (!e.InventoryAccountStd.IsLoaded)
                    {
                        e.InventoryAccountStd.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("e.InventoryAccountStd");
                    }
                }
            }
            catch (InvalidOperationException ex) { ex.ToString(); }

            #endregion

            InventoryDTO dto = new InventoryDTO()
            {
                InventoryId = e.InventoryId,
                ActorCompanyId = e.ActorCompanyId,
                ParentId = e.ParentId,
                ParentName = e.ParentName,
                InventoryWriteOffMethodId = e.InventoryWriteOffMethodId,
                VoucherSeriesTypeId = e.VoucherSeriesTypeId,
                SupplierInvoiceId = e.SupplierInvoiceId,
                SupplierInvoiceInfo = e.SupplierInvoiceInfo,
                CustomerInvoiceId = e.CustomerInvoiceId,
                CustomerInvoiceInfo = e.CustomerInvoiceInfo,
                InventoryNr = e.InventoryNr,
                Name = e.Name,
                Description = e.Description,
                Notes = e.Notes,
                Status = (TermGroup_InventoryStatus)e.Status,
                StatusName = e.StatusName,
                PurchaseDate = e.PurchaseDate,
                WriteOffDate = e.WriteOffDate,
                PurchaseAmount = e.PurchaseAmount,
                WriteOffAmount = e.WriteOffAmount,
                WriteOffSum = e.WriteOffSum,
                WriteOffRemainingAmount = e.WriteOffRemainingAmount,
                WriteOffPeriods = e.WriteOffPeriods,
                EndAmount = e.EndAmount,
                PeriodType = (TermGroup_InventoryWriteOffMethodPeriodType)e.PeriodType,
                PeriodValue = e.PeriodValue,
                CategoryIds = e.CategoryIds,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State
            };

            // Accounts
            if (includeAccountSettings && !includeAngularAccountSettings && e.InventoryAccountStd != null && e.InventoryAccountStd.Count > 0)
            {
                // Inventory
                InventoryAccountStd accStd = e.InventoryAccountStd.FirstOrDefault(c => c.Type == (int)InventoryAccountType.Inventory);
                Account account = accStd?.AccountStd?.Account;
                dto.InventoryAccounts = new Dictionary<int, AccountSmallDTO>();
                dto.InventoryAccounts.Add(Constants.ACCOUNTDIM_STANDARD, account.ToSmallDTO());
                if (accStd != null && accStd.AccountInternal != null)
                {
                    foreach (var accInt in accStd.AccountInternal.Where(a => a.Account != null && a.Account.AccountDim != null && a.Account.AccountDim.AccountDimNr != Constants.ACCOUNTDIM_STANDARD))
                    {
                        dto.InventoryAccounts.Add(accInt.Account.AccountDim.AccountDimNr, accInt.Account.ToSmallDTO());
                    }
                }

                // AccWriteOff
                accStd = e.InventoryAccountStd.FirstOrDefault(c => c.Type == (int)InventoryAccountType.AccWriteOff);
                account = accStd?.AccountStd?.Account;
                dto.AccWriteOffAccounts = new Dictionary<int, AccountSmallDTO>();
                dto.AccWriteOffAccounts.Add(Constants.ACCOUNTDIM_STANDARD, account.ToSmallDTO());
                if (accStd != null && accStd.AccountInternal != null)
                {
                    foreach (var accInt in accStd.AccountInternal.Where(a => a.Account != null && a.Account.AccountDim != null && a.Account.AccountDim.AccountDimNr != Constants.ACCOUNTDIM_STANDARD))
                    {
                        dto.AccWriteOffAccounts.Add(accInt.Account.AccountDim.AccountDimNr, accInt.Account.ToSmallDTO());
                    }
                }

                // WriteOff
                accStd = e.InventoryAccountStd.FirstOrDefault(c => c.Type == (int)InventoryAccountType.WriteOff);
                account = accStd?.AccountStd?.Account;
                dto.WriteOffAccounts = new Dictionary<int, AccountSmallDTO>();
                dto.WriteOffAccounts.Add(Constants.ACCOUNTDIM_STANDARD, account.ToSmallDTO());
                if (accStd != null && accStd.AccountInternal != null)
                {
                    foreach (var accInt in accStd.AccountInternal.Where(a => a.Account != null && a.Account.AccountDim != null && a.Account.AccountDim.AccountDimNr != Constants.ACCOUNTDIM_STANDARD))
                    {
                        dto.WriteOffAccounts.Add(accInt.Account.AccountDim.AccountDimNr, accInt.Account.ToSmallDTO());
                    }
                }

                // AccOverWriteOff
                accStd = e.InventoryAccountStd.FirstOrDefault(c => c.Type == (int)InventoryAccountType.AccOverWriteOff);
                account = accStd?.AccountStd?.Account;
                dto.AccOverWriteOffAccounts = new Dictionary<int, AccountSmallDTO>();
                dto.AccOverWriteOffAccounts.Add(Constants.ACCOUNTDIM_STANDARD, account.ToSmallDTO());
                if (accStd != null && accStd.AccountInternal != null)
                {
                    foreach (var accInt in accStd.AccountInternal.Where(a => a.Account != null && a.Account.AccountDim != null && a.Account.AccountDim.AccountDimNr != Constants.ACCOUNTDIM_STANDARD))
                    {
                        dto.AccOverWriteOffAccounts.Add(accInt.Account.AccountDim.AccountDimNr, accInt.Account.ToSmallDTO());
                    }
                }

                // OverWriteOff
                accStd = e.InventoryAccountStd.FirstOrDefault(c => c.Type == (int)InventoryAccountType.OverWriteOff);
                account = accStd?.AccountStd?.Account;
                dto.OverWriteOffAccounts = new Dictionary<int, AccountSmallDTO>();
                dto.OverWriteOffAccounts.Add(Constants.ACCOUNTDIM_STANDARD, account.ToSmallDTO());
                if (accStd != null && accStd.AccountInternal != null)
                {
                    foreach (var accInt in accStd.AccountInternal.Where(a => a.Account != null && a.Account.AccountDim != null && a.Account.AccountDim.AccountDimNr != Constants.ACCOUNTDIM_STANDARD))
                    {
                        dto.OverWriteOffAccounts.Add(accInt.Account.AccountDim.AccountDimNr, accInt.Account.ToSmallDTO());
                    }
                }

                // AccWriteDown
                accStd = e.InventoryAccountStd.FirstOrDefault(c => c.Type == (int)InventoryAccountType.AccWriteDown);
                account = accStd?.AccountStd?.Account;
                dto.AccWriteDownAccounts = new Dictionary<int, AccountSmallDTO>();
                dto.AccWriteDownAccounts.Add(Constants.ACCOUNTDIM_STANDARD, account.ToSmallDTO());
                if (accStd != null && accStd.AccountInternal != null)
                {
                    foreach (var accInt in accStd.AccountInternal.Where(a => a.Account != null && a.Account.AccountDim != null && a.Account.AccountDim.AccountDimNr != Constants.ACCOUNTDIM_STANDARD))
                    {
                        dto.AccWriteDownAccounts.Add(accInt.Account.AccountDim.AccountDimNr, accInt.Account.ToSmallDTO());
                    }
                }

                // WriteDown
                accStd = e.InventoryAccountStd.FirstOrDefault(c => c.Type == (int)InventoryAccountType.WriteDown);
                account = accStd?.AccountStd?.Account;
                dto.WriteDownAccounts = new Dictionary<int, AccountSmallDTO>();
                dto.WriteDownAccounts.Add(Constants.ACCOUNTDIM_STANDARD, account.ToSmallDTO());
                if (accStd != null && accStd.AccountInternal != null)
                {
                    foreach (var accInt in accStd.AccountInternal.Where(a => a.Account != null && a.Account.AccountDim != null && a.Account.AccountDim.AccountDimNr != Constants.ACCOUNTDIM_STANDARD))
                    {
                        dto.WriteDownAccounts.Add(accInt.Account.AccountDim.AccountDimNr, accInt.Account.ToSmallDTO());
                    }
                }

                // AccWriteUp
                accStd = e.InventoryAccountStd.FirstOrDefault(c => c.Type == (int)InventoryAccountType.AccWriteUp);
                account = accStd?.AccountStd?.Account;
                dto.AccWriteUpAccounts = new Dictionary<int, AccountSmallDTO>();
                dto.AccWriteUpAccounts.Add(Constants.ACCOUNTDIM_STANDARD, account.ToSmallDTO());
                if (accStd != null && accStd.AccountInternal != null)
                {
                    foreach (var accInt in accStd.AccountInternal.Where(a => a.Account != null && a.Account.AccountDim != null && a.Account.AccountDim.AccountDimNr != Constants.ACCOUNTDIM_STANDARD))
                    {
                        dto.AccWriteUpAccounts.Add(accInt.Account.AccountDim.AccountDimNr, accInt.Account.ToSmallDTO());
                    }
                }

                // WriteUp
                accStd = e.InventoryAccountStd.FirstOrDefault(c => c.Type == (int)InventoryAccountType.WriteUp);
                account = accStd?.AccountStd?.Account;
                dto.WriteUpAccounts = new Dictionary<int, AccountSmallDTO>();
                dto.WriteUpAccounts.Add(Constants.ACCOUNTDIM_STANDARD, account.ToSmallDTO());
                if (accStd != null && accStd.AccountInternal != null)
                {
                    foreach (var accInt in accStd.AccountInternal.Where(a => a.Account != null && a.Account.AccountDim != null && a.Account.AccountDim.AccountDimNr != Constants.ACCOUNTDIM_STANDARD))
                    {
                        dto.WriteUpAccounts.Add(accInt.Account.AccountDim.AccountDimNr, accInt.Account.ToSmallDTO());
                    }
                }
            }
            else if (includeAngularAccountSettings)
            {
                dto.AccountingSettings = new List<AccountingSettingsRowDTO>();

                if (e.InventoryAccountStd != null && e.InventoryAccountStd.Count > 0)
                {
                    AddAccountingSettingsRowDTO(e, dto, InventoryAccountType.Inventory);
                    AddAccountingSettingsRowDTO(e, dto, InventoryAccountType.AccWriteOff);
                    AddAccountingSettingsRowDTO(e, dto, InventoryAccountType.WriteOff);
                    AddAccountingSettingsRowDTO(e, dto, InventoryAccountType.AccOverWriteOff);
                    AddAccountingSettingsRowDTO(e, dto, InventoryAccountType.OverWriteOff);
                    AddAccountingSettingsRowDTO(e, dto, InventoryAccountType.AccWriteDown);
                    AddAccountingSettingsRowDTO(e, dto, InventoryAccountType.WriteDown);
                    AddAccountingSettingsRowDTO(e, dto, InventoryAccountType.AccWriteUp);
                    AddAccountingSettingsRowDTO(e, dto, InventoryAccountType.WriteUp);
                }
            }

            return dto;
        }

        public static IEnumerable<InventoryDTO> ToDTOs(this IEnumerable<Inventory> l, bool includeAccountSettings)
        {
            var dtos = new List<InventoryDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO(includeAccountSettings, false));
                }
            }
            return dtos;
        }

        private static void AddAccountingSettingsRowDTO(Inventory inventory, InventoryDTO dto, InventoryAccountType type)
        {
            AccountingSettingsRowDTO accDto = new AccountingSettingsRowDTO()
            {
                Type = (int)type,
                Percent = 0
            };
            dto.AccountingSettings.Add(accDto);

            InventoryAccountStd accStd = inventory.InventoryAccountStd.FirstOrDefault(c => c.Type == (int)type);
            Account account = accStd?.AccountStd?.Account;
            if (account != null)
            {
                accDto.AccountDim1Nr = Constants.ACCOUNTDIM_STANDARD;
                accDto.Account1Id = account.AccountId;
                accDto.Account1Nr = account.AccountNr;
                accDto.Account1Name = account.Name;
            }

            if (accStd != null && accStd.AccountInternal != null)
            {
                foreach (var accInt in accStd.AccountInternal.Where(a => a.Account != null && a.Account.AccountDim != null && a.Account.AccountDim.AccountDimNr != Constants.ACCOUNTDIM_STANDARD).OrderBy(a => a.Account.AccountDim.AccountDimNr))
                {
                    account = accInt.Account;

                    // TODO: Does not support dim numbers over 6!!!
                    if (account.AccountDim.AccountDimNr == 2)
                    {
                        accDto.AccountDim2Nr = account.AccountDim.AccountDimNr;
                        accDto.Account2Id = account.AccountId;
                        accDto.Account2Nr = account.AccountNr;
                        accDto.Account2Name = account.Name;
                    }
                    else if (account.AccountDim.AccountDimNr == 3)
                    {
                        accDto.AccountDim3Nr = account.AccountDim.AccountDimNr;
                        accDto.Account3Id = account.AccountId;
                        accDto.Account3Nr = account.AccountNr;
                        accDto.Account3Name = account.Name;
                    }
                    else if (account.AccountDim.AccountDimNr == 4)
                    {
                        accDto.AccountDim4Nr = account.AccountDim.AccountDimNr;
                        accDto.Account4Id = account.AccountId;
                        accDto.Account4Nr = account.AccountNr;
                        accDto.Account4Name = account.Name;
                    }
                    else if (account.AccountDim.AccountDimNr == 5)
                    {
                        accDto.AccountDim5Nr = account.AccountDim.AccountDimNr;
                        accDto.Account5Id = account.AccountId;
                        accDto.Account5Nr = account.AccountNr;
                        accDto.Account5Name = account.Name;
                    }
                    else if (account.AccountDim.AccountDimNr == 6)
                    {
                        accDto.AccountDim6Nr = account.AccountDim.AccountDimNr;
                        accDto.Account6Id = account.AccountId;
                        accDto.Account6Nr = account.AccountNr;
                        accDto.Account6Name = account.Name;
                    }
                }
            }
        }

        public static InventoryGridDTO ToGridDTO(this Inventory e)
        {
            if (e == null)
                return null;

            InventoryGridDTO dto = new InventoryGridDTO()
            {
                InventoryId = e.InventoryId,
                InventoryNr = e.InventoryNr,
                Name = e.Name,
                Status = e.Status,
                StatusName = e.StatusName,
                Description = e.Description,
                PurchaseDate = e.PurchaseDate,
                PurchaseAmount = e.PurchaseAmount,
                WriteOffAmount = e.WriteOffAmount,
                WriteOffRemainingAmount = e.WriteOffRemainingAmount,
                WriteOffSum = e.WriteOffSum,
                EndAmount = e.EndAmount,
                InventoryWriteOffMethod = e.InventoryWriteOffMethod?.Name ?? "",
                InventoryWriteOffMethodId = e.InventoryWriteOffMethodId,
                InventoryAccountNr = e.InventoryAccountNr,
                InventoryAccountName = e.InventoryAccountName,
                Categories = e.CategoryNames != null ? e.CategoryNamesString : String.Empty,
            };

            return dto;
        }

        public static IEnumerable<InventoryGridDTO> ToGridDTOs(this IEnumerable<Inventory> l)
        {
            var dtos = new List<InventoryGridDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToGridDTO());
                }
            }
            return dtos;
        }

        public static InventorySearchResultDTO ToSearchResultDTO(this InventorySearchResult e)
        {
            if (e == null)
                return null;

            InventorySearchResultDTO dto = new InventorySearchResultDTO()
            {
                InventoryId = e.InventoryId,
                InventoryNr = e.InventoryNr,
                Name = e.Name,
                Description = e.Description
            };

            return dto;
        }

        public static IEnumerable<InventorySearchResultDTO> ToSearchResultDTOs(this IEnumerable<InventorySearchResult> l)
        {
            var dtos = new List<InventorySearchResultDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToSearchResultDTO());
                }
            }
            return dtos;
        }

        #endregion

        #region InventoryWriteOffMethod

        public static InventoryWriteOffMethodDTO ToDTO(this InventoryWriteOffMethod e)
        {
            if (e == null)
                return null;

            InventoryWriteOffMethodDTO dto = new InventoryWriteOffMethodDTO()
            {
                InventoryWriteOffMethodId = e.InventoryWriteOffMethodId,
                ActorCompanyId = e.ActorCompanyId,
                Name = e.Name,
                Description = e.Description,
                Type = (TermGroup_InventoryWriteOffMethodType)e.Type,
                PeriodType = (TermGroup_InventoryWriteOffMethodPeriodType)e.PeriodType,
                PeriodValue = e.PeriodValue,
                YearPercent = e.YearPercent,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State,
                HasAcitveWirteOffs = e.Inventory != null
                                        && e.Inventory
                                            .SelectMany(x => x.AccountDistributionEntry)
                                            .Any(x => x.VoucherHeadId > 0)
            };

            return dto;
        }

        public static InventoryWriteOffMethodGridDTO ToGridDTO(this InventoryWriteOffMethod e)
        {
            if (e == null)
                return null;

            InventoryWriteOffMethodGridDTO dto = new InventoryWriteOffMethodGridDTO()
            {
                InventoryWriteOffMethodId = e.InventoryWriteOffMethodId,
                Name = e.Name,
                Description = e.Description,
                Type = (TermGroup_InventoryWriteOffMethodType)e.Type,
                PeriodType = (TermGroup_InventoryWriteOffMethodPeriodType)e.PeriodType,
                PeriodValue = e.PeriodValue,
                State = (SoeEntityState)e.State,
                YearPercent = e.YearPercent
            };

            return dto;
        }

        public static IEnumerable<InventoryWriteOffMethodDTO> ToDTOs(this IEnumerable<InventoryWriteOffMethod> l)
        {
            var dtos = new List<InventoryWriteOffMethodDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static IEnumerable<InventoryWriteOffMethodGridDTO> ToGridDTOs(this IEnumerable<InventoryWriteOffMethod> l)
        {
            var dtos = new List<InventoryWriteOffMethodGridDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToGridDTO());
                }
            }
            return dtos;
        }

        #endregion

        #region InventoryWriteOffTemplate

        public static InventoryWriteOffTemplateDTO ToDTO(this InventoryWriteOffTemplate e, bool includeAccountSettings)
        {
            if (e == null)
                return null;

            #region Try load

            try
            {
                if (includeAccountSettings && !e.IsAdded())
                {
                    if (!e.InventoryAccountStd.IsLoaded)
                    {
                        e.InventoryAccountStd.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("e.InventoryAccountStd");
                    }
                }
            }
            catch (InvalidOperationException ex) { ex.ToString(); }

            #endregion

            InventoryWriteOffTemplateDTO dto = new InventoryWriteOffTemplateDTO()
            {
                InventoryWriteOffTemplateId = e.InventoryWriteOffTemplateId,
                ActorCompanyId = e.ActorCompanyId,
                InventoryWriteOffMethodId = e.InventoryWriteOffMethodId,
                VoucherSeriesTypeId = e.VoucherSeriesTypeId,
                Name = e.Name,
                Description = e.Description,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State
            };

            if (includeAccountSettings)
            {
                dto.AccountingSettings = new List<AccountingSettingDTO>();

                AddAccountingSettingDTO(e, dto, InventoryAccountType.Inventory);
                AddAccountingSettingDTO(e, dto, InventoryAccountType.AccWriteOff);
                AddAccountingSettingDTO(e, dto, InventoryAccountType.WriteOff);
                AddAccountingSettingDTO(e, dto, InventoryAccountType.AccOverWriteOff);
                AddAccountingSettingDTO(e, dto, InventoryAccountType.OverWriteOff);
                AddAccountingSettingDTO(e, dto, InventoryAccountType.AccWriteDown);
                AddAccountingSettingDTO(e, dto, InventoryAccountType.WriteDown);
                AddAccountingSettingDTO(e, dto, InventoryAccountType.AccWriteUp);
                AddAccountingSettingDTO(e, dto, InventoryAccountType.WriteUp);
            }

            // Accounts
            if (includeAccountSettings && e.InventoryAccountStd != null && e.InventoryAccountStd.Count > 0)
            {
                // Inventory
                InventoryAccountStd accStd = e.InventoryAccountStd.FirstOrDefault(c => c.Type == (int)InventoryAccountType.Inventory);
                Account account = accStd?.AccountStd?.Account;
                dto.InventoryAccounts = new Dictionary<int, AccountSmallDTO>();
                dto.InventoryAccounts.Add(Constants.ACCOUNTDIM_STANDARD, account.ToSmallDTO());
                if (accStd != null && accStd.AccountInternal != null)
                {
                    foreach (var accInt in accStd.AccountInternal.Where(a => a.Account != null && a.Account.AccountDim != null && a.Account.AccountDim.AccountDimNr != Constants.ACCOUNTDIM_STANDARD))
                    {
                        dto.InventoryAccounts.Add(accInt.Account.AccountDim.AccountDimNr, accInt.Account.ToSmallDTO());
                    }
                }

                // AccWriteOff
                accStd = e.InventoryAccountStd.FirstOrDefault(c => c.Type == (int)InventoryAccountType.AccWriteOff);
                account = accStd?.AccountStd?.Account;
                dto.AccWriteOffAccounts = new Dictionary<int, AccountSmallDTO>();
                dto.AccWriteOffAccounts.Add(Constants.ACCOUNTDIM_STANDARD, account.ToSmallDTO());
                if (accStd != null && accStd.AccountInternal != null)
                {
                    foreach (var accInt in accStd.AccountInternal.Where(a => a.Account != null && a.Account.AccountDim != null && a.Account.AccountDim.AccountDimNr != Constants.ACCOUNTDIM_STANDARD))
                    {
                        dto.AccWriteOffAccounts.Add(accInt.Account.AccountDim.AccountDimNr, accInt.Account.ToSmallDTO());
                    }
                }

                // WriteOff
                accStd = e.InventoryAccountStd.FirstOrDefault(c => c.Type == (int)InventoryAccountType.WriteOff);
                account = accStd?.AccountStd?.Account;
                dto.WriteOffAccounts = new Dictionary<int, AccountSmallDTO>();
                dto.WriteOffAccounts.Add(Constants.ACCOUNTDIM_STANDARD, account.ToSmallDTO());
                if (accStd != null && accStd.AccountInternal != null)
                {
                    foreach (var accInt in accStd.AccountInternal.Where(a => a.Account != null && a.Account.AccountDim != null && a.Account.AccountDim.AccountDimNr != Constants.ACCOUNTDIM_STANDARD))
                    {
                        dto.WriteOffAccounts.Add(accInt.Account.AccountDim.AccountDimNr, accInt.Account.ToSmallDTO());
                    }
                }

                // AccOverWriteOff
                accStd = e.InventoryAccountStd.FirstOrDefault(c => c.Type == (int)InventoryAccountType.AccOverWriteOff);
                account = accStd?.AccountStd?.Account;
                dto.AccOverWriteOffAccounts = new Dictionary<int, AccountSmallDTO>();
                dto.AccOverWriteOffAccounts.Add(Constants.ACCOUNTDIM_STANDARD, account.ToSmallDTO());
                if (accStd != null && accStd.AccountInternal != null)
                {
                    foreach (var accInt in accStd.AccountInternal.Where(a => a.Account != null && a.Account.AccountDim != null && a.Account.AccountDim.AccountDimNr != Constants.ACCOUNTDIM_STANDARD))
                    {
                        dto.AccOverWriteOffAccounts.Add(accInt.Account.AccountDim.AccountDimNr, accInt.Account.ToSmallDTO());
                    }
                }

                // OverWriteOff
                accStd = e.InventoryAccountStd.FirstOrDefault(c => c.Type == (int)InventoryAccountType.OverWriteOff);
                account = accStd?.AccountStd?.Account;
                dto.OverWriteOffAccounts = new Dictionary<int, AccountSmallDTO>();
                dto.OverWriteOffAccounts.Add(Constants.ACCOUNTDIM_STANDARD, account.ToSmallDTO());
                if (accStd != null && accStd.AccountInternal != null)
                {
                    foreach (var accInt in accStd.AccountInternal.Where(a => a.Account != null && a.Account.AccountDim != null && a.Account.AccountDim.AccountDimNr != Constants.ACCOUNTDIM_STANDARD))
                    {
                        dto.OverWriteOffAccounts.Add(accInt.Account.AccountDim.AccountDimNr, accInt.Account.ToSmallDTO());
                    }
                }

                // AccWriteDown
                accStd = e.InventoryAccountStd.FirstOrDefault(c => c.Type == (int)InventoryAccountType.AccWriteDown);
                account = accStd?.AccountStd?.Account;
                dto.AccWriteDownAccounts = new Dictionary<int, AccountSmallDTO>();
                dto.AccWriteDownAccounts.Add(Constants.ACCOUNTDIM_STANDARD, account.ToSmallDTO());
                if (accStd != null && accStd.AccountInternal != null)
                {
                    foreach (var accInt in accStd.AccountInternal.Where(a => a.Account != null && a.Account.AccountDim != null && a.Account.AccountDim.AccountDimNr != Constants.ACCOUNTDIM_STANDARD))
                    {
                        dto.AccWriteDownAccounts.Add(accInt.Account.AccountDim.AccountDimNr, accInt.Account.ToSmallDTO());
                    }
                }

                // WriteDown
                accStd = e.InventoryAccountStd.FirstOrDefault(c => c.Type == (int)InventoryAccountType.WriteDown);
                account = accStd?.AccountStd?.Account;
                dto.WriteDownAccounts = new Dictionary<int, AccountSmallDTO>();
                dto.WriteDownAccounts.Add(Constants.ACCOUNTDIM_STANDARD, account.ToSmallDTO());
                if (accStd != null && accStd.AccountInternal != null)
                {
                    foreach (var accInt in accStd.AccountInternal.Where(a => a.Account != null && a.Account.AccountDim != null && a.Account.AccountDim.AccountDimNr != Constants.ACCOUNTDIM_STANDARD))
                    {
                        dto.WriteDownAccounts.Add(accInt.Account.AccountDim.AccountDimNr, accInt.Account.ToSmallDTO());
                    }
                }

                // AccWriteUp
                accStd = e.InventoryAccountStd.FirstOrDefault(c => c.Type == (int)InventoryAccountType.AccWriteUp);
                account = accStd?.AccountStd?.Account;
                dto.AccWriteUpAccounts = new Dictionary<int, AccountSmallDTO>();
                dto.AccWriteUpAccounts.Add(Constants.ACCOUNTDIM_STANDARD, account.ToSmallDTO());
                if (accStd != null && accStd.AccountInternal != null)
                {
                    foreach (var accInt in accStd.AccountInternal.Where(a => a.Account != null && a.Account.AccountDim != null && a.Account.AccountDim.AccountDimNr != Constants.ACCOUNTDIM_STANDARD))
                    {
                        dto.AccWriteUpAccounts.Add(accInt.Account.AccountDim.AccountDimNr, accInt.Account.ToSmallDTO());
                    }
                }

                // WriteUp
                accStd = e.InventoryAccountStd.FirstOrDefault(c => c.Type == (int)InventoryAccountType.WriteUp);
                account = accStd?.AccountStd?.Account;
                dto.WriteUpAccounts = new Dictionary<int, AccountSmallDTO>();
                dto.WriteUpAccounts.Add(Constants.ACCOUNTDIM_STANDARD, account.ToSmallDTO());
                if (accStd != null && accStd.AccountInternal != null)
                {
                    foreach (var accInt in accStd.AccountInternal.Where(a => a.Account != null && a.Account.AccountDim != null && a.Account.AccountDim.AccountDimNr != Constants.ACCOUNTDIM_STANDARD))
                    {
                        dto.WriteUpAccounts.Add(accInt.Account.AccountDim.AccountDimNr, accInt.Account.ToSmallDTO());
                    }
                }
            }

            return dto;
        }

        public static InventoryWriteOffTemplateGridDTO ToGridDTO(this InventoryWriteOffTemplate e)
        {
            if (e == null)
                return null;

            InventoryWriteOffTemplateGridDTO dto = new InventoryWriteOffTemplateGridDTO()
            {
                InventoryWriteOffTemplateId = e.InventoryWriteOffTemplateId,
                Name = e.Name,
                Description = e.Description,
                InventoryWriteOffMethodId = e.InventoryWriteOffMethodId,
                VoucherSeriesTypeId = e.VoucherSeriesTypeId,
            };

            return dto;
        }

        public static IEnumerable<InventoryWriteOffTemplateDTO> ToDTOs(this IEnumerable<InventoryWriteOffTemplate> l, bool includeAccountSettings = false)
        {
            var dtos = new List<InventoryWriteOffTemplateDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    InventoryWriteOffTemplateDTO dto = ToDTO(e, includeAccountSettings);
                    dtos.Add(dto);
                }
            }
            return dtos;
        }

        public static IEnumerable<InventoryWriteOffTemplateGridDTO> ToGridDTOs(this IEnumerable<InventoryWriteOffTemplate> l)
        {
            var dtos = new List<InventoryWriteOffTemplateGridDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToGridDTO());
                }
            }
            return dtos;
        }

        #endregion

        #endregion

        #region Invoice

        #region Invoice

        public static IEnumerable<OriginUserDTO> ToDTOs(this IEnumerable<OriginUserView> l)
        {
            var dtos = new List<OriginUserDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static OriginUserDTO ToDTO(this OriginUserView e)
        {
            var dto = new OriginUserDTO
            {
                LoginName = e.LoginName,
                Name = e.Name,
                Main = e.Main,
                UserId = e.UserId,
                RoleId = e.RoleId ?? 0,
                ReadyDate = e.ReadyDate
            };
            return dto;
        }

        #endregion


        #region BillingInvoice

        public static BillingInvoiceDTO ToBillingInvoiceDTO(this CustomerInvoice e, bool includeRows)
        {
            if (e == null)
                return null;

            #region Try load

            try
            {
                if (!e.IsAdded())
                {
                    if (!e.OriginReference.IsLoaded)
                    {
                        e.OriginReference.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("e.OriginReference");
                    }
                    if (e.Origin != null && !e.Origin.OriginUser.IsLoaded)
                    {
                        e.Origin.OriginUser.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("e.Origin.OriginUser");
                    }
                    foreach (var originUser in e.Origin.OriginUser)
                    {
                        if (!originUser.UserReference.IsLoaded)
                        {
                            originUser.UserReference.Load();
                            DataProjectLogCollector.LogLoadedEntityInExtension("originUser.UserReference");
                        }
                    }

                    if (!e.ProjectReference.IsLoaded)
                    {
                        e.ProjectReference.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("e.ProjectReference");
                    }
                }
            }
            catch (InvalidOperationException ex) { ex.ToString(); }

            #endregion

            var dto = new BillingInvoiceDTO
            {
                InvoiceId = e.InvoiceId,
                ActorId = e.ActorId,
                DeliveryCustomerId = e.DeliveryCustomerId,
                ContactEComId = e.ContactEComId,
                ContactGLNId = e.ContactGLNId,
                ProjectId = e.ProjectId,
                PaymentConditionId = e.PaymentConditionId,
                DeliveryTypeId = e.DeliveryTypeId,
                DeliveryConditionId = e.DeliveryConditionId,
                DeliveryAddressId = e.DeliveryAddressId,
                BillingAddressId = e.BillingAddressId,
                PriceListTypeId = e.PriceListTypeId,
                SysWholeSellerId = e.SysWholeSellerId,
                DefaultDim1AccountId = e.DefaultDim1AccountId,
                DefaultDim2AccountId = e.DefaultDim2AccountId,
                DefaultDim3AccountId = e.DefaultDim3AccountId,
                DefaultDim4AccountId = e.DefaultDim4AccountId,
                DefaultDim5AccountId = e.DefaultDim5AccountId,
                DefaultDim6AccountId = e.DefaultDim6AccountId,
                BillingType = (TermGroup_BillingType)e.BillingType,
                VatType = (TermGroup_InvoiceVatType)e.VatType,
                OrderType = (TermGroup_OrderType)e.OrderType,
                InvoiceNr = e.InvoiceNr,
                SeqNr = e.SeqNr,
                OrderNumbers = e.OrderNumbers,
                //OCR = e.OCR,
                InvoiceText = e.InvoiceText,
                InvoiceHeadText = e.InvoiceHeadText,
                InvoiceLabel = e.InvoiceLabel,
                OrderReference = e.OrderReference,
                //InternalDescription = e.InternalDescription,
                //ExternalDescription = e.ExternalDescription,
                WorkingDescription = e.WorkingDescription,
                BillingAdressText = e.BillingAdressText,
                DeliveryDateText = e.DeliveryDateText,
                CurrencyId = e.CurrencyId,
                CurrencyRate = e.CurrencyRate,
                CurrencyDate = e.CurrencyDate,
                InvoiceDate = e.InvoiceDate,
                DueDate = e.DueDate,
                VoucherDate = e.VoucherDate,
                OrderDate = e.OrderDate,
                DeliveryDate = e.DeliveryDate,
                //TimeDiscountDate = e.TimeDiscountDate,
                //TimeDiscountPercent = e.TimeDiscountPercent,
                ReferenceOur = e.ReferenceOur,
                ReferenceYour = e.ReferenceYour,
                TotalAmount = e.TotalAmount,
                TotalAmountCurrency = e.TotalAmountCurrency,
                //TotalAmountEntCurrency = e.TotalAmountEntCurrency,
                //TotalAmountLedgerCurrency = e.TotalAmountLedgerCurrency,
                VatAmount = e.VATAmount,
                VatAmountCurrency = e.VATAmountCurrency,
                //VatAmountEntCurrency = e.VATAmountEntCurrency,
                //VatAmountLedgerCurrency = e.VATAmountLedgerCurrency,
                PaidAmount = e.PaidAmount,
                PaidAmountCurrency = e.PaidAmountCurrency,
                //PaidAmountEntCurrency = e.PaidAmountEntCurrency,
                //PaidAmountLedgerCurrency = e.PaidAmountLedgerCurrency,
                RemainingAmount = e.RemainingAmount,
                //RemainingAmountVat = e.RemainingAmountVat,
                RemainingAmountExVat = e.RemainingAmountExVat,
                CentRounding = e.CentRounding,
                FreightAmount = e.FreightAmount,
                FreightAmountCurrency = e.FreightAmountCurrency,
                //FreightAmountEntCurrency = e.FreightAmountEntCurrency,
                //FreightAmountLedgerCurrency = e.FreightAmountLedgerCurrency,
                InvoiceFee = e.InvoiceFee,
                InvoiceFeeCurrency = e.InvoiceFeeCurrency,
                //InvoiceFeeEntCurrency = e.InvoiceFeeEntCurrency,
                //InvoiceFeeLedgerCurrency = e.InvoiceFeeLedgerCurrency,
                SumAmount = e.SumAmount,
                SumAmountCurrency = e.SumAmountCurrency,
                MarginalIncomeCurrency = e.MarginalIncomeCurrency,
                MarginalIncomeRatio = e.MarginalIncomeRatio,
                IsTemplate = e.IsTemplate,
                ManuallyAdjustedAccounting = e.ManuallyAdjustedAccounting,
                //HasHouseholdTaxDeduction = e.HasHouseholdTaxDeduction,
                FixedPriceOrder = e.FixedPriceOrder,
                //MultipleAssetRows = e.MultipleAssetRows,
                InsecureDebt = e.InsecureDebt,
                PrintTimeReport = e.PrintTimeReport,
                BillingInvoicePrinted = e.BillingInvoicePrinted,
                CashSale = e.CashSale,
                AddAttachementsToEInvoice = e.AddAttachementsToEInvoice,
                StatusIcon = (SoeStatusIcon)e.StatusIcon,
                ShiftTypeId = e.ShiftTypeId,
                PlannedStartDate = e.PlannedStartDate,
                PlannedStopDate = e.PlannedStopDate,
                EstimatedTime = e.EstimatedTime,
                RemainingTime = e.RemainingTime,
                Priority = e.Priority,
                InvoiceDeliveryType = e.InvoiceDeliveryType,
                InvoiceDeliveryProvider = e.InvoiceDeliveryProvider,
                InvoicePaymentService = e.InvoicePaymentService,
                IncludeOnInvoice = e.IncludeOnInvoice,
                IncludeOnlyInvoicedTime = e.IncludeOnlyInvoicedTime,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                //State = (SoeEntityState)e.State,
                NbrOfChecklists = e.NbrOfChecklists,
                TriangulationSales = e.TriangulationSales,
                TransferedFromOrder = e.TransferedFromOrder,
                TransferedFromOffer = e.TransferedFromOffer,
                TransferedFromOriginType = e.TransferedFromType,
                AddSupplierInvoicesToEInvoice = e.AddSupplierInvoicesToEInvoices,
                CustomerName = e.CustomerName,
                CustomerEmail = e.CustomerEmail,
                CustomerPhoneNr = e.CustomerPhoneNr,
                ExternalInvoiceNr = e.ExternalId,
                ContractNr = e.ContractNr,
            };

            // Extensions
            dto.OriginStatusName = e.StatusName;
            if (e.Origin != null)
            {
                dto.OriginStatus = (SoeOriginStatus)e.Origin.Status;
                dto.OriginDescription = e.Origin.Description;
                dto.VoucherSeriesId = e.Origin.VoucherSeriesId;
                dto.VoucherSeriesTypeId = e.Origin.VoucherSeriesTypeId.GetValueOrDefault(0);

                if (!e.OriginInvoiceMapping.IsLoaded)
                {
                    e.OriginInvoiceMapping.Load();
                    DataProjectLogCollector.LogLoadedEntityInExtension("e.OriginInvoiceMapping");
                }

                dto.HasOrder = e.OriginInvoiceMapping.Any(o => o.Type == (int)SoeOriginInvoiceMappingType.Order);

                dto.OriginUsers = new List<OriginUserSmallDTO>();
                if (e.Origin.OriginUser != null)
                {
                    foreach (var user in e.Origin.OriginUser.Where(u => u.State == (int)SoeEntityState.Active).OrderByDescending(u => u.Main).ThenBy(u => u.User.Name))
                    {
                        dto.OriginUsers.Add(user.ToSmallDTO());
                    }
                }
            }
            else
                dto.OriginStatus = SoeOriginStatus.None;

            dto.ProjectNr = e.ProjectNr;
            dto.CustomerBlockNote = e.CustomerBlockNote;
            dto.CashSale = e.CashSale;
            dto.CategoryIds = e.CategoryIds;

            if (includeRows)
            {
                dto.CustomerInvoiceRows = new List<ProductRowDTO>();
                if (e.CustomerInvoiceRow != null && e.CustomerInvoiceRow.Count > 0)
                    dto.CustomerInvoiceRows = e.CustomerInvoiceRow.Where(r => r.State == (int)SoeEntityState.Active).ToProductRowDTOs();
            }

            return dto;
        }

        #endregion

        #endregion

        #region SupplierInvoice

        public static SupplierInvoiceDTO ToSupplierInvoiceDTO(this SupplierInvoice e, bool includeOrigin = false, bool includeRows = false, bool includeProjectRows = false, bool includeOrderRows = false, List<AccountDim> dims = null, GenericImageDTO image = null)
        {
            if (e == null)
                return null;

            // Create InvoiceDTO
            InvoiceDTO dto = e.ToDTO(includeOrigin);
            // Create SupplierInvoiceDTO and copy properties from InvoiceDTO
            SupplierInvoiceDTO sidto = new SupplierInvoiceDTO();
            var properties = dto.GetType().GetProperties();
            foreach (var property in properties)
            {
                PropertyInfo pi = dto.GetType().GetProperty(property.Name);
                if (pi.CanWrite)
                    property.SetValue(sidto, pi.GetValue(dto, null), null);
            }

            // Set SupplierInvoice specific properties
            sidto.PaymentMethodId = e.PaymentMethodId;
            sidto.AttestStateId = e.AttestStateId;
            sidto.AttestGroupId = e.AttestGroupId;
            sidto.InterimInvoice = e.InterimInvoice;
            sidto.MultipleDebtRows = e.MultipleDebtRows;
            sidto.BlockPayment = e.BlockPayment;
            sidto.OrderNr = e.OrderNr;
            if (e.Order != null)
            {
                sidto.OrderCustomerInvoiceId = e.Order.InvoiceId;
                sidto.OrderCustomerName = e.Order.Actor.Customer.Name;
                if (!e.ProjectId.HasValue && e.Order.ProjectId.HasValue)
                    sidto.OrderProjectId = e.Order.ProjectId.Value;
            }
            sidto.ProjectId = e.ProjectId;
            sidto.ProjectNr = e.Project != null ? e.Project.Number : "";
            sidto.ProjectName = e.Project != null ? e.Project.Name : "";

            sidto.VatDeductionAccountId = e.VatDeductionAccountId;
            sidto.VatDeductionPercent = e.VatDeductionPercent;
            sidto.VatDeductionType = (TermGroup_VatDeductionType)e.VatDeductionType;

            sidto.BlockReasonTextId = e.BlockReasonTextId;
            sidto.BlockReason = e.BlockReason;


            sidto.CurrencyId = e.CurrencyId;
            sidto.CurrencyRate = e.CurrencyRate;
            sidto.CurrencyDate = e.CurrencyDate;

            // Extensions
            sidto.Image = image;
            sidto.HasImage = (image != null); //(e as SupplierInvoice).HasImage;
            sidto.AttestStateName = e.AttestStateName;

            if (includeRows)
            {
                sidto.SupplierInvoiceRows = new List<SupplierInvoiceRowDTO>();
                foreach (var row in e.SupplierInvoiceRow)
                {
                    sidto.SupplierInvoiceRows.Add(row.ToSupplierInvoiceRowDTO(e, true, dims, e.VoucherDate));
                }
            }

            if (includeProjectRows && e.SupplierInvoiceProjectRows != null)
                sidto.SupplierInvoiceProjectRows = e.SupplierInvoiceProjectRows;

            if (includeOrderRows && e.SupplierInvoiceOrderRows != null)
                sidto.SupplierInvoiceOrderRows = e.SupplierInvoiceOrderRows;

            if ((includeOrderRows || includeProjectRows) && e.SupplierInvoiceCostAllocationRows != null)
                sidto.SupplierInvoiceCostAllocationRows = e.SupplierInvoiceCostAllocationRows;

            return sidto;
        }

        public static SupplierInvoiceRowDTO ToSupplierInvoiceRowDTO(this SupplierInvoiceRow e, SupplierInvoice invoice, bool includeAccountingRows, List<AccountDim> dims = null, DateTime? date = null)
        {
            if (e == null)
                return null;

            #region Try load

            try
            {
                if (!e.IsAdded())
                {
                    if (includeAccountingRows && !e.SupplierInvoiceAccountRow.IsLoaded)
                    {
                        e.SupplierInvoiceAccountRow.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("e.SupplierInvoiceAccountRow");
                    }
                }
            }
            catch (InvalidOperationException ex) { ex.ToString(); }

            #endregion

            var dto = new SupplierInvoiceRowDTO
            {
                SupplierInvoiceRowId = e.SupplierInvoiceRowId,
                InvoiceId = invoice?.InvoiceId ?? 0,
                Quantity = e.Quantity,
                Amount = e.Amount,
                AmountCurrency = e.AmountCurrency,
                AmountEntCurrency = e.AmountEntCurrency,
                AmountLedgerCurrency = e.AmountLedgerCurrency,
                VatAmount = e.VatAmount,
                VatAmountCurrency = e.VatAmountCurrency,
                VatAmountEntCurrency = e.VatAmountEntCurrency,
                VatAmountLedgerCurrency = e.VatAmountLedgerCurrency,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State
            };

            // Accounting rows
            if (includeAccountingRows && !e.SupplierInvoiceAccountRow.IsNullOrEmpty())
            {
                dto.AccountingRows = new List<AccountingRowDTO>();
                foreach (var row in e.SupplierInvoiceAccountRow)
                {
                    dto.AccountingRows.Add(row.AccountingRowDTO(dims, e.InvoiceId, date));
                }
            }

            return dto;
        }

        public static AccountingRowDTO AccountingRowDTO(this SupplierInvoiceAccountRow e, List<AccountDim> dims = null, int invoiceId = 0, DateTime? date = null)
        {
            if (e == null)
                return null;

            #region Try load

            try
            {
                if (!e.IsAdded())
                {

                    if (!e.UserReference.IsLoaded)
                    {
                        e.UserReference.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("e.UserReference");
                    }
                    if (!e.AccountStdReference.IsLoaded)
                    {
                        e.AccountStdReference.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("e.AccountStdReference");
                    }
                    if (e.AccountStd != null && !e.AccountStd.AccountReference.IsLoaded)
                    {
                        e.AccountStd.AccountReference.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("e.AccountStd.AccountReference");
                    }
                    if (e.AccountStd != null && e.AccountStd.Account != null && !e.AccountStd.Account.AccountMapping.IsLoaded)
                    {
                        e.AccountStd.Account.AccountMapping.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("e.AccountStd.Account.AccountMapping");
                    }
                    if (!e.AccountInternal.IsLoaded)
                    {
                        e.AccountInternal.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("e.AccountInternal");
                    }
                    if (e.AccountInternal != null)
                    {
                        foreach (var accInt in e.AccountInternal)
                        {
                            if (!accInt.AccountReference.IsLoaded)
                            {
                                accInt.AccountReference.Load();
                                DataProjectLogCollector.LogLoadedEntityInExtension("accInt.AccountReference");
                            }
                            if (accInt.Account != null && !accInt.Account.AccountDimReference.IsLoaded)
                            {
                                accInt.Account.AccountDimReference.Load();
                                DataProjectLogCollector.LogLoadedEntityInExtension("accInt.Account.AccountDimReference");
                            }
                        }
                    }
                }
            }
            catch (InvalidOperationException ex) { ex.ToString(); }

            #endregion

            var dto = new AccountingRowDTO
            {
                InvoiceRowId = e.SupplierInvoiceRow != null ? e.SupplierInvoiceRow.SupplierInvoiceRowId : 0,    // TODO: Add foreign key to model
                TempInvoiceRowId = e.SupplierInvoiceRow != null ? e.SupplierInvoiceRow.SupplierInvoiceRowId : 0,
                InvoiceAccountRowId = e.SupplierInvoiceAccountRowId,
                TempRowId = e.SupplierInvoiceAccountRowId,
                Type = (AccountingRowType)e.Type,
                RowNr = e.RowNr,
                Text = e.Text,
                Quantity = e.Quantity,
                Amount = e.Amount,
                CreditAmount = e.Amount < 0 ? Math.Abs(e.Amount) : 0,
                DebitAmount = e.Amount > 0 ? e.Amount : 0,
                AmountCurrency = e.AmountCurrency,
                CreditAmountCurrency = e.AmountCurrency < 0 ? Math.Abs(e.AmountCurrency) : 0,
                DebitAmountCurrency = e.AmountCurrency > 0 ? e.AmountCurrency : 0,
                AmountEntCurrency = e.AmountEntCurrency,
                CreditAmountEntCurrency = e.AmountEntCurrency < 0 ? Math.Abs(e.AmountEntCurrency) : 0,
                DebitAmountEntCurrency = e.AmountEntCurrency > 0 ? e.AmountEntCurrency : 0,
                AmountLedgerCurrency = e.AmountLedgerCurrency,
                CreditAmountLedgerCurrency = e.AmountLedgerCurrency < 0 ? Math.Abs(e.AmountLedgerCurrency) : 0,
                DebitAmountLedgerCurrency = e.AmountLedgerCurrency > 0 ? e.AmountLedgerCurrency : 0,
                SplitType = e.SplitType,
                SplitPercent = e.SplitPercent,
                IsCreditRow = e.CreditRow,
                IsDebitRow = e.DebitRow,
                IsVatRow = e.VatRow,
                IsContractorVatRow = e.ContractorVatRow,
                IsInterimRow = e.InterimRow,
                AttestStatus = e.AttestStatus,
                AttestUserId = e.AttestUserId,
                AccountDistributionHeadId = e.AccountDistributionHeadId != null ? (int)e.AccountDistributionHeadId : 0,
                InvoiceId = invoiceId,
                StartDate = e.StartDate,
                NumberOfPeriods = e.NumberOfPeriods,
                State = (SoeEntityState)e.State
            };

            if (date.HasValue)
                dto.Date = date.Value;

            // Extensions
            dto.AttestUserName = e.User != null ? e.User.Name : string.Empty;

            // Accounts
            AccountStd accStd = e.AccountStd;
            if (accStd != null)
            {
                dto.Dim1Id = accStd.AccountId;
                dto.Dim1Nr = accStd.Account != null ? accStd.Account.AccountNr : string.Empty;
                dto.Dim1Name = accStd.Account != null ? accStd.Account.Name : string.Empty;
                dto.Dim1Disabled = false;
                dto.Dim1Mandatory = true;
                dto.QuantityStop = accStd.UnitStop;
                dto.Unit = accStd.Unit;
                dto.AmountStop = accStd.AmountStop;
                dto.RowTextStop = accStd.RowTextStop;
            }

            // Set values from mappings
            if (dims != null)
            {
                int i = 1;
                foreach (AccountDim dim in dims)
                {
                    var mapping = accStd?.Account?.AccountMapping?.FirstOrDefault(m => m.AccountDimId == dim.AccountDimId);
                    if (mapping != null)
                    {
                        switch (i)
                        {
                            case (2):
                                dto.Dim2Disabled = mapping.MandatoryLevel == (int)TermGroup_AccountMandatoryLevel.Warn;
                                dto.Dim2Mandatory = mapping.MandatoryLevel == (int)TermGroup_AccountMandatoryLevel.Mandatory;
                                dto.Dim2Stop = mapping.MandatoryLevel == (int)TermGroup_AccountMandatoryLevel.Stop;
                                break;
                            case (3):
                                dto.Dim3Disabled = mapping.MandatoryLevel == (int)TermGroup_AccountMandatoryLevel.Warn;
                                dto.Dim3Mandatory = mapping.MandatoryLevel == (int)TermGroup_AccountMandatoryLevel.Mandatory;
                                dto.Dim3Stop = mapping.MandatoryLevel == (int)TermGroup_AccountMandatoryLevel.Stop;
                                break;
                            case (4):
                                dto.Dim4Disabled = mapping.MandatoryLevel == (int)TermGroup_AccountMandatoryLevel.Warn;
                                dto.Dim4Mandatory = mapping.MandatoryLevel == (int)TermGroup_AccountMandatoryLevel.Mandatory;
                                dto.Dim4Stop = mapping.MandatoryLevel == (int)TermGroup_AccountMandatoryLevel.Stop;
                                break;
                            case (5):
                                dto.Dim5Disabled = mapping.MandatoryLevel == (int)TermGroup_AccountMandatoryLevel.Warn;
                                dto.Dim5Mandatory = mapping.MandatoryLevel == (int)TermGroup_AccountMandatoryLevel.Mandatory;
                                dto.Dim5Stop = mapping.MandatoryLevel == (int)TermGroup_AccountMandatoryLevel.Stop;
                                break;
                            case (6):
                                dto.Dim6Disabled = mapping.MandatoryLevel == (int)TermGroup_AccountMandatoryLevel.Warn;
                                dto.Dim6Mandatory = mapping.MandatoryLevel == (int)TermGroup_AccountMandatoryLevel.Mandatory;
                                dto.Dim6Stop = mapping.MandatoryLevel == (int)TermGroup_AccountMandatoryLevel.Stop;
                                break;
                        }
                    }
                    i++;
                }
            }

            if (e.AccountInternal != null)
            {
                foreach (AccountInternal accInt in e.AccountInternal)
                {
                    if (accInt.Account != null && accInt.Account.AccountDim != null)
                    {
                        if (dims != null)
                        {
                            int index = 1;
                            foreach (AccountDim dim in dims)
                            {
                                if (dim.AccountDimId == accInt.Account.AccountDim.AccountDimId)
                                    break;
                                index++;
                            }

                            switch (index)
                            {
                                case 2:
                                    dto.Dim2Id = accInt.AccountId;
                                    dto.Dim2Nr = accInt.Account.AccountNr;
                                    dto.Dim2Name = accInt.Account.Name;
                                    break;
                                case 3:
                                    dto.Dim3Id = accInt.AccountId;
                                    dto.Dim3Nr = accInt.Account.AccountNr;
                                    dto.Dim3Name = accInt.Account.Name;
                                    break;
                                case 4:
                                    dto.Dim4Id = accInt.AccountId;
                                    dto.Dim4Nr = accInt.Account.AccountNr;
                                    dto.Dim4Name = accInt.Account.Name;
                                    break;
                                case 5:
                                    dto.Dim5Id = accInt.AccountId;
                                    dto.Dim5Nr = accInt.Account.AccountNr;
                                    dto.Dim5Name = accInt.Account.Name;
                                    break;
                                case 6:
                                    dto.Dim6Id = accInt.AccountId;
                                    dto.Dim6Nr = accInt.Account.AccountNr;
                                    dto.Dim6Name = accInt.Account.Name;
                                    break;
                            }
                        }
                        else
                        {
                            switch (accInt.Account.AccountDim.AccountDimNr)
                            {
                                case 2:
                                    dto.Dim2Id = accInt.AccountId;
                                    dto.Dim2Nr = accInt.Account.AccountNr;
                                    dto.Dim2Name = accInt.Account.Name;
                                    break;
                                case 3:
                                    dto.Dim3Id = accInt.AccountId;
                                    dto.Dim3Nr = accInt.Account.AccountNr;
                                    dto.Dim3Name = accInt.Account.Name;
                                    break;
                                case 4:
                                    dto.Dim4Id = accInt.AccountId;
                                    dto.Dim4Nr = accInt.Account.AccountNr;
                                    dto.Dim4Name = accInt.Account.Name;
                                    break;
                                case 5:
                                    dto.Dim5Id = accInt.AccountId;
                                    dto.Dim5Nr = accInt.Account.AccountNr;
                                    dto.Dim5Name = accInt.Account.Name;
                                    break;
                                case 6:
                                    dto.Dim6Id = accInt.AccountId;
                                    dto.Dim6Nr = accInt.Account.AccountNr;
                                    dto.Dim6Name = accInt.Account.Name;
                                    break;
                            }
                        }
                    }
                }
            }

            return dto;
        }

        public static IEnumerable<AccountingRowDTO> ToAccountingRowDTO(this IEnumerable<SupplierInvoiceAccountRow> e, List<AccountDim> dims = null)
        {
            return e.Select(s => s.AccountingRowDTO(dims)).ToList();
        }

        #endregion
        #region InvoiceText

        public static InvoiceTextDTO ToDTO(this InvoiceText e)
        {
            if (e == null)
                return null;
            InvoiceTextDTO dto = new InvoiceTextDTO()
            {
                InvoiceId = e.InvoiceId,
                EdiEntryId = e.EdiEntryId,
                Type = (InvoiceTextType)e.Type,
                Text = e.Text,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State
            };
            return dto;
        }

        #endregion

        #region InvoiceExport

        public static InvoiceExportDTO ToDTO(this InvoiceExport e)
        {
            if (e == null)
                return null;

            InvoiceExportDTO dto = new InvoiceExportDTO()
            {
                InvoiceExportId = e.InvoiceExportId,
                ExportDate = e.ExportDate,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                ActorCompanyId = e.ActorCompanyId,
                BatchId = e.BatchId,
                SysPaymentServiceId = (TermGroup_SysPaymentService)e.SysPaymentServiceId,
                TotalAmount = e.TotalAmount,
                NumberOfInvoices = e.NumberOfInvoices,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State
            };

            return dto;
        }

        public static IEnumerable<InvoiceExportDTO> ToDTOs(this IEnumerable<InvoiceExport> l)
        {
            var dtos = new List<InvoiceExportDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        #endregion

        #region InvoiceExportIO

        public static InvoiceExportIODTO ToDTO(this InvoiceExportIO e)
        {
            if (e == null)
                return null;

            InvoiceExportIODTO dto = new InvoiceExportIODTO()
            {
                InvoiceExportIOId = e.InvoiceExportIOId,
                InvoiceExportId = e.InvoiceExportId,
                CustomerId = e.CustomerId,
                CustomerName = e.CustomerName,
                InvoiceType = (TermGroup_BillingType)e.InvoiceType,
                InvoiceId = e.InvoiceId,
                InvoiceNr = e.InvoiceNr,
                InvoiceSeqnr = e.InvoiceSeqnr,
                InvoiceAmount = e.InvoiceAmount,
                Currency = e.Currency,
                InvoiceDate = e.InvoiceDate,
                DueDate = e.DueDate,
                BankAccount = e.BankAccount,
                PayerId = e.BankAccount,
                State = (SoeEntityState)e.State,
            };
            // Extensions
            //dto.TypeName = 
            return dto;
        }

        public static IEnumerable<InvoiceExportIODTO> ToDTOs(this IEnumerable<InvoiceExportIO> l)
        {
            var dtos = new List<InvoiceExportIODTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        #endregion

        #region InvoiceAttachment

        public static InvoiceAttachmentDTO ToDTO(this InvoiceAttachment e)
        {
            if (e == null)
                return null;

            InvoiceAttachmentDTO dto = new InvoiceAttachmentDTO()
            {
                InvoiceAttachmentId = e.InvoiceAttachmentId,
                InvoiceId = e.InvoiceId,
                DataStorageRecordId = e.DataStorageRecordId,
                EdiEntryId = e.EdiEntryId,
                AddAttachmentsOnEInvoice = e.AddAttachmentsOnEInvoice,
                AddAttachmentsOnTransfer = e.AddAttachmentsOnTransfer,
                LastDistributedDate = e.LastDistributedDate,
            };

            return dto;
        }

        public static IEnumerable<InvoiceAttachmentDTO> ToDTOs(this IEnumerable<InvoiceAttachment> l)
        {
            var dtos = new List<InvoiceAttachmentDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        #endregion

        #endregion

        #region Markup

        public static MarkupDTO ToDTO(this Markup e)
        {
            if (e == null)
                return null;

            MarkupDTO dto = new MarkupDTO()
            {
                MarkupId = e.MarkupId,
                ActorCompanyId = e.Company != null ? e.Company.ActorCompanyId : 0,  // TODO: Add foreign key to model
                SysWholesellerId = e.SysWholesellerId,
                ActorCustomerId = e.ActorCustomerId,
                Code = e.Code,
                ProductIdFilter = e.ProductIdFilter,
                MarkupPercent = e.MarkupPercent,
                DiscountPercent = e.DiscountPercent,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State,
                CategoryId = e.Category,
            };

            // Extensions
            dto.WholesellerName = e.WholesellerName;
            dto.WholesellerDiscountPercent = e.WholesellerDiscountPercent;

            return dto;
        }

        public static IEnumerable<MarkupDTO> ToDTOs(this IEnumerable<Markup> l)
        {
            var dtos = new List<MarkupDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        #endregion

        #region MatchCode

        public static MatchCodeDTO ToDTO(this MatchCode e)
        {
            if (e == null)
                return null;

            MatchCodeDTO dto = new MatchCodeDTO()
            {
                MatchCodeId = e.MatchCodeId,
                ActorCompanyId = e.ActorCompanyId,
                AccountId = e.AccountId,
                VatAccountId = e.VatAccountId,
                Type = (SoeInvoiceMatchingType)e.Type,
                TypeId = e.Type,
                Name = e.Name,
                Description = e.Description,
                AccountNr = e.AccountNr,
                VatAccountNr = e.VatAccountNr,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State
            };

            return dto;
        }

        public static IEnumerable<MatchCodeDTO> ToDTOs(this IEnumerable<MatchCode> l)
        {
            var dtos = new List<MatchCodeDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static MatchCodeGridDTO ToGridDTO(this MatchCode e)
        {
            if (e == null)
                return null;

            MatchCodeGridDTO dto = new MatchCodeGridDTO()
            {
                MatchCodeId = e.MatchCodeId,
                Name = e.Name,
                Description = e.Description,
                AccountNr = e.AccountNr,
            };

            return dto;
        }

        public static IEnumerable<MatchCodeGridDTO> ToGridDTOs(this IEnumerable<MatchCode> l)
        {
            var dtos = new List<MatchCodeGridDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToGridDTO());
                }
            }
            return dtos;
        }

        #endregion

        #region Origin

        public static OriginDTO ToDTO(this Origin e)
        {
            if (e == null)
                return null;

            OriginDTO dto = new OriginDTO()
            {
                OriginId = e.OriginId,
                ActorCompanyId = e.ActorCompanyId,
                VoucherSeriesId = e.VoucherSeriesId,
                Type = (SoeOriginType)e.Type,
                Description = e.Description,
                Status = (SoeOriginStatus)e.Status,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy
            };

            return dto;
        }

        public static IEnumerable<OriginDTO> ToDTOs(this IEnumerable<Origin> l)
        {
            var dtos = new List<OriginDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        #endregion

        #region OriginUser

        public static OriginUserDTO ToDTO(this OriginUser e)
        {
            if (e == null)
                return null;

            OriginUserDTO dto = new OriginUserDTO()
            {
                OriginUserId = e.OriginUserId,
                OriginId = e.Origin != null ? e.Origin.OriginId : 0, // TODO: Add foreign key to model
                RoleId = e.Role != null ? e.Role.RoleId : 0, // TODO: Add foreign key to model
                UserId = e.User != null ? e.User.UserId : 0, // TODO: Add foreign key to model
                Main = e.Main,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State
            };

            // Extensions
            if (e.User != null)
            {
                dto.LoginName = e.User.LoginName;
                dto.Name = e.User.Name;
            }

            return dto;
        }

        public static OriginUserSmallDTO ToSmallDTO(this OriginUser e)
        {
            if (e == null)
                return null;

            var dto = new OriginUserSmallDTO
            {
                OriginUserId = e.OriginUserId,
                UserId = e.User != null ? e.User.UserId : 0, // TODO: Add foreign key to model
                Main = e.Main,
                Name = e.User != null ? e.User.Name : string.Empty,
                IsReady = e.ReadyDate.HasValue
            };

            return dto;
        }

        #endregion

        #region PaymentCondition

        public static PaymentConditionDTO ToDTO(this PaymentCondition e)
        {
            if (e == null)
                return null;

            var dto = new PaymentConditionDTO
            {
                PaymentConditionId = e.PaymentConditionId,
                Code = e.Code,
                Name = e.Name,
                Days = e.Days,
                DiscountDays = e.DiscountDays,
                DiscountPercent = e.DiscountPercent,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                StartOfNextMonth = e.StartOfNextMonth
            };

            return dto;
        }

        public static IEnumerable<PaymentConditionDTO> ToDTOs(this IEnumerable<PaymentCondition> l)
        {
            var dtos = new List<PaymentConditionDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static PaymentConditionGridDTO ToGridDTO(this PaymentCondition e)
        {
            if (e == null)
                return null;

            var dto = new PaymentConditionGridDTO
            {
                PaymentConditionId = e.PaymentConditionId,
                Code = e.Code,
                Name = e.Name,
                Days = e.Days,
            };

            return dto;
        }

        public static IEnumerable<PaymentConditionGridDTO> ToGridDTOs(this IEnumerable<PaymentCondition> l)
        {
            var dtos = new List<PaymentConditionGridDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToGridDTO());
                }
            }
            return dtos;
        }

        #endregion

        #region PaymentImport

        public static PaymentImportDTO ToDTO(this PaymentImport e)
        {
            if (e == null)
                return null;

            var dto = new PaymentImportDTO
            {
                PaymentImportId = e.PaymentImportId,
                ImportDate = e.ImportDate,
                Filename = e.Filename,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                ActorCompanyId = e.ActorCompanyId,
                BatchId = e.BatchId,
                SysPaymentTypeId = (TermGroup_SysPaymentType)e.SysPaymentTypeId,
                Type = e.Type,
                TotalAmount = e.TotalAmount,
                NumberOfPayments = e.NumberOfPayments,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State,
                PaymentLabel = e.PaymentLabel,
                ImportType = (ImportPaymentType)e.ImportType,
                ImportPaymentTypeTermId = (TermGroup_ImportPaymentType)e.ImportType
            };

            return dto;
        }

        public static List<PaymentImportDTO> ToDTOs(this List<PaymentImport> l)
        {
            var dtos = new List<PaymentImportDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        #endregion

        #region PaymentImportIO

        public static PaymentImportIODTO ToDTO(this PaymentImportIO e)
        {
            if (e == null)
                return null;

            PaymentImportIODTO dto = new PaymentImportIODTO()
            {
                PaymentImportIOId = e.PaymentImportIOId,
                ActorCompanyId = e.ActorCompanyId,
                BatchNr = e.BatchNr,
                Type = (TermGroup_BillingType)e.Type,
                CustomerId = e.CustomerId,
                Customer = e.Customer,
                InvoiceId = e.InvoiceId,
                InvoiceNr = e.InvoiceNr,
                InvoiceSeqnr = e.InvoiceSeqnr,
                InvoiceAmount = e.ImportType == (int)ImportPaymentType.SupplierPayment && e.Type == (int)TermGroup_BillingType.Credit ? e.RestAmount + e.PaidAmount : e.InvoiceAmount,
                RestAmount = e.RestAmount,
                PaidAmount = e.PaidAmount,
                PaidAmountCurrency = e.PaidAmountCurrency,
                Currency = e.Currency,
                InvoiceDate = e.InvoiceDate,
                PaidDate = e.PaidDate,
                MatchCodeId = e.MatchCodeId,
                Status = (ImportPaymentIOStatus)e.Status,
                State = (ImportPaymentIOState)e.State,
                ImportType = (ImportPaymentType)e.ImportType,
                DueDate = e.DueDate,
            };

            return dto;
        }

        #endregion

        #region PaymentExport

        public static PaymentExportDTO ToDTO(this PaymentExport e, bool includeRows, List<AccountDim> dims)
        {
            if (e == null)
                return null;

            var dto = new PaymentExportDTO
            {
                PaymentExportId = e.PaymentExportId,
                Type = (TermGroup_SysPaymentType)e.Type,
                ExportDate = e.ExportDate,
                Filename = e.Filename,
                CustomerNr = e.CustomerNr,
                NumberOfPayments = e.NumberOfPayments,
                Foreign = e.Foreign,
                CancelledState = e.CancelledState,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                State = (SoeEntityState)e.State,
                TransferStatus = e.TransferStatus,
                TransferMsg = e.TransferMsg
            };

            if (includeRows)
                dto.PaymentRows = (e.PaymentRows != null && e.PaymentRows.Count > 0) ? e.PaymentRows.Where(r => r.State == (int)SoeEntityState.Active).ToDTOs(false, false, dims).ToList() : new List<PaymentRowDTO>();

            return dto;
        }

        public static IEnumerable<PaymentExportDTO> ToDTOs(this IEnumerable<PaymentExport> l, bool includeRows, List<AccountDim> dims)
        {
            var dtos = new List<PaymentExportDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO(includeRows, dims));
                }
            }
            return dtos;
        }

        #endregion

        #region PaymentInformation

        public static PaymentInformationDTO ToDTO(this PaymentInformation e, bool includeRows, bool? includeForeginPayments = null, bool setBIC = false)
        {
            if (e == null)
                return null;

            #region Try load

            try
            {
                if (includeRows && !e.IsAdded())
                {
                    if (!e.PaymentInformationRow.IsLoaded)
                    {
                        e.PaymentInformationRow.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("e.PaymentInformationRow");
                    }
                }
            }
            catch (InvalidOperationException ex) { ex.ToString(); }

            #endregion

            var dto = new PaymentInformationDTO()
            {
                PaymentInformationId = e.PaymentInformationId,
                ActorId = e.Actor?.ActorId ?? 0,    // TODO: Add foreign key to model
                DefaultSysPaymentTypeId = e.DefaultSysPaymentTypeId,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State,
            };

            if (includeRows)
                dto.Rows = e.PaymentInformationRow != null ? e.PaymentInformationRow.Where(r => r.State == (int)SoeEntityState.Active && (includeForeginPayments == null || (includeForeginPayments.Value ? r.IntermediaryCode > 0 : (r.IntermediaryCode == 0 || r.IntermediaryCode == null)))).ToDTOs(setBIC).ToList() : new List<PaymentInformationRowDTO>();

            return dto;
        }

        #endregion

        #region PaymentInformationRow

        public static PaymentInformationRowDTO ToDTO(this PaymentInformationRow e, bool setBIC = false)
        {
            if (e == null)
                return null;

            var dto = new PaymentInformationRowDTO
            {
                PaymentInformationRowId = e.PaymentInformationRowId,
                PaymentInformationId = e.PaymentInformation?.PaymentInformationId ?? 0,    // TODO: Add foreign key to model
                SysPaymentTypeId = e.SysPaymentTypeId,
                Default = e.Default,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State,
                BIC = e.BIC,
                ClearingCode = e.ClearingCode,
                PaymentCode = e.PaymentCode,
                PaymentMethodCode = e.PaymentMethodCode != 0 ? e.PaymentMethodCode : 0,
                PaymentForm = e.PaymentForm != null ? e.PaymentForm : 0,
                ChargeCode = e.ChargeCode != null ? e.ChargeCode : 0,
                IntermediaryCode = e.IntermediaryCode != null ? e.IntermediaryCode : 0,
                CurrencyAccount = e.CurrencyAccount,
                PayerBankId = e.PayerBankId,
                ShownInInvoice = e.ShownInInvoice.GetValueOrDefault(),
                BankConnected = e.BankConnected,
                CurrencyId = e.CurrencyId,
                CurrencyCode = e.CurrencyCode,
            };

            if (setBIC)
            {
                if (!string.IsNullOrEmpty(e.BIC))
                {
                    dto.PaymentNr = e.PaymentNr + " (" + e.BIC + ")";
                }
                else if (e.PaymentNr.Contains('/'))
                {
                    var parts = e.PaymentNr.Split('/');
                    dto.PaymentNr = parts[1] + " (" + parts[0] + ")";
                }
                else
                {
                    dto.PaymentNr = e.PaymentNr;
                }
            }
            else
            {
                dto.PaymentNr = e.PaymentNr;
            }

            // Extensions
            dto.SysPaymentTypeName = e.SysPaymentTypeName;

            return dto;
        }

        public static IEnumerable<PaymentInformationRowDTO> ToDTOs(this IEnumerable<PaymentInformationRow> l, bool setBIC = false)
        {
            var dtos = new List<PaymentInformationRowDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO(setBIC));
                }
            }
            return dtos;
        }

        public static PaymentInformationRow FromDTO(this PaymentInformationRowDTO e)
        {
            if (e == null)
                return null;

            PaymentInformationRow row = new PaymentInformationRow()
            {
                PaymentInformationRowId = e.PaymentInformationRowId,
                SysPaymentTypeId = e.SysPaymentTypeId,
                PaymentNr = e.PaymentNr,
                Default = e.Default,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (int)e.State
            };

            return row;
        }

        #endregion

        #region PaymentMethod

        public static PaymentMethodDTO ToDTO(this PaymentMethod e, bool includePaymentInformationRows, bool includeAccount = false)
        {
            #region Try load

            try
            {
                if (!e.IsAdded())
                {
                    if (includePaymentInformationRows && !e.PaymentInformationRowReference.IsLoaded)
                    {
                        e.PaymentInformationRowReference.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("e.PaymentInformationRowReference");
                    }
                    if (includeAccount && !e.AccountStdReference.IsLoaded)
                    {
                        e.AccountStdReference.Load();
                        e.AccountStd.AccountReference.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("e.AccountStd.AccountReference mm");
                    }
                }
            }
            catch (InvalidOperationException ex) { ex.ToString(); }

            #endregion

            PaymentMethodDTO dto = new PaymentMethodDTO
            {
                PaymentMethodId = e.PaymentMethodId,
                ActorCompanyId = e.Company != null ? e.Company.ActorCompanyId : 0,  // TODO: Add foreign key to model
                AccountId = e.AccountStd != null ? e.AccountStd.AccountId : 0,      // TODO: Add foreign key to model
                PaymentInformationRowId = e.PaymentInformationRow != null ? e.PaymentInformationRow.PaymentInformationRowId : (int?)null,   // TODO: Add foreign key to model
                SysPaymentMethodId = e.SysPaymentMethodId,
                PaymentType = (SoeOriginType)e.PaymentType,
                Name = e.Name,
                CustomerNr = e.CustomerNr,
                State = (SoeEntityState)e.State,
                UseInCashSales = e.UseInCashSales,
                UseRoundingInCashSales = e.UseRoundingInCashSales,
                TransactionCode = e.TransactionCode
            };

            // Extensions
            dto.PaymentNr = e.PaymentNr;
            dto.PayerBankId = e.PayerBankId;
            dto.SysPaymentMethodName = e.SysPaymentMethodName;
            dto.SysPaymentTypeId = e.SysPaymentTypeId;

            if (e.PaymentInformationRow != null)
            {
                dto.CurrencyCode = e.PaymentInformationRow.CurrencyCode;

                if (includePaymentInformationRows)
                    dto.PaymentInformationRow = e.PaymentInformationRow.ToDTO();
            }
            if (includeAccount && e.AccountStd != null)
                dto.AccountNr = e.AccountStd.Account.AccountNr;

            return dto;
        }
        public static PaymentMethodSupplierGridDTO ToSupplierGridDTO(this PaymentMethod e, bool includePaymentInformationRows, bool includeAccount = false)
        {
            #region Try load

            try
            {
                if (!e.IsAdded())
                {
                    if (includePaymentInformationRows && !e.PaymentInformationRowReference.IsLoaded)
                    {
                        e.PaymentInformationRowReference.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("e.PaymentInformationRowReference");
                    }
                    if (includeAccount && !e.AccountStdReference.IsLoaded)
                    {
                        e.AccountStdReference.Load();
                        e.AccountStd.AccountReference.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("e.AccountStd.AccountReference mm");
                    }
                }
            }
            catch (InvalidOperationException ex) { ex.ToString(); }

            #endregion

            PaymentMethodSupplierGridDTO dto = new PaymentMethodSupplierGridDTO
            {
                PaymentMethodId = e.PaymentMethodId,
                AccountId = e.AccountStd != null ? e.AccountStd.AccountId : 0,      // TODO: Add foreign key to model
                SysPaymentMethodId = e.SysPaymentMethodId,
                Name = e.Name,
                CustomerNr = e.CustomerNr
            };

            // Extensions
            dto.PaymentNr = e.PaymentNr;
            dto.PayerBankId = e.PayerBankId;
            dto.SysPaymentMethodName = e.SysPaymentMethodName;

            if (includeAccount && e.AccountStd != null)
                dto.AccountNr = e.AccountStd.Account.AccountNr;

            if (includePaymentInformationRows && e.PaymentInformationRow != null && e.PaymentInformationRow.CurrencyId.HasValue)
                dto.CurrencyCode = e.PaymentInformationRow.CurrencyCode;

            return dto;
        }

        public static PaymentMethodCustomerGridDTO ToCustomerGridDTO(this PaymentMethod e, bool includePaymentInformationRows, bool includeAccount = false)
        {
            #region Try load

            try
            {
                if (!e.IsAdded())
                {
                    if (includePaymentInformationRows && !e.PaymentInformationRowReference.IsLoaded)
                    {
                        e.PaymentInformationRowReference.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("e.PaymentInformationRowReference");
                    }
                    if (includeAccount && !e.AccountStdReference.IsLoaded)
                    {
                        e.AccountStdReference.Load();
                        e.AccountStd.AccountReference.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("e.AccountStd.AccountReference mm");
                    }
                }
            }
            catch (InvalidOperationException ex) { ex.ToString(); }

            #endregion

            PaymentMethodCustomerGridDTO dto = new PaymentMethodCustomerGridDTO
            {
                PaymentMethodId = e.PaymentMethodId,
                AccountId = e.AccountStd != null ? e.AccountStd.AccountId : 0,      // TODO: Add foreign key to model
                PaymentInformationRowId = e.PaymentInformationRow != null ? e.PaymentInformationRow.PaymentInformationRowId : (int?)null,   // TODO: Add foreign key to model
                SysPaymentMethodId = e.SysPaymentMethodId,
                Name = e.Name,
                CustomerNr = e.CustomerNr,
                UseInCashSales = e.UseInCashSales,
                UseRoundingInCashSales = e.UseRoundingInCashSales,
                TransactionCode = e.TransactionCode
            };

            dto.PaymentNr = e.PaymentNr;
            dto.SysPaymentMethodName = e.SysPaymentMethodName;

            if (includeAccount && e.AccountStd != null)
                dto.AccountNr = e.AccountStd.Account.AccountNr;

            return dto;
        }


        public static IEnumerable<PaymentMethodSupplierGridDTO> ToSupplierGridDTOs(this IEnumerable<PaymentMethod> l, bool includePaymentInformationRows, bool includeAccount = false)
        {
            var dtos = new List<PaymentMethodSupplierGridDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToSupplierGridDTO(includePaymentInformationRows, includeAccount));
                }
            }
            return dtos;
        }

        public static IEnumerable<PaymentMethodCustomerGridDTO> ToCustomerGridDTOs(this IEnumerable<PaymentMethod> l, bool includePaymentInformationRows, bool includeAccount = false)
        {
            var dtos = new List<PaymentMethodCustomerGridDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToCustomerGridDTO(includePaymentInformationRows, includeAccount));
                }
            }
            return dtos;
        }



        public static IEnumerable<PaymentMethodDTO> ToDTOs(this IEnumerable<PaymentMethod> l, bool includePaymentInformationRows, bool includeAccount = false)
        {
            var dtos = new List<PaymentMethodDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO(includePaymentInformationRows, includeAccount));
                }
            }
            return dtos;
        }

        public static PaymentMethodSmallDTO ToSmallDTO(this PaymentMethod e)
        {
            if (e == null)
                return null;

            #region Try load

            try
            {
                if (!e.IsAdded())
                {
                    if (!e.AccountStdReference.IsLoaded)
                    {
                        e.AccountStdReference.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("e.AccountStdReference");
                    }
                }
            }
            catch (InvalidOperationException ex) { ex.ToString(); }

            #endregion

            PaymentMethodSmallDTO dto = new PaymentMethodSmallDTO()
            {
                PaymentMethodId = e.PaymentMethodId,
                PaymentMethod = (TermGroup_SysPaymentMethod)e.SysPaymentMethodId,
                AccountStdAccountId = e.AccountStd.AccountId,
            };

            return dto;
        }

        #endregion

        #region PaymentRow

        public static PaymentRowDTO ToDTO(this PaymentRow e, bool includeAccountingRows, bool includePaymentOrigin, List<AccountDim> dims, bool checkMultipleRows = false)
        {
            if (e == null)
                return null;

            #region Try load

            try
            {
                if (!e.IsAdded())
                {
                    if (!e.PaymentReference.IsLoaded)
                    {
                        e.PaymentReference.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("e.AccountStdReference");
                    }
                    if (includePaymentOrigin)
                    {
                        if (e.Payment != null && !e.Payment.OriginReference.IsLoaded)
                        {
                            e.Payment.OriginReference.Load();
                            DataProjectLogCollector.LogLoadedEntityInExtension("e.Payment.OriginReference");
                        }
                    }
                    if (includeAccountingRows)
                    {
                        if (!e.PaymentAccountRow.IsLoaded)
                        {
                            e.PaymentAccountRow.Load();
                            DataProjectLogCollector.LogLoadedEntityInExtension("e.PaymentAccountRow");
                        }
                        foreach (var accRow in e.PaymentAccountRow)
                        {
                            if (!accRow.AccountStdReference.IsLoaded)
                            {
                                accRow.AccountStdReference.Load();
                                DataProjectLogCollector.LogLoadedEntityInExtension("accRow.AccountStdReference");
                            }
                            if (accRow.AccountStd != null && !accRow.AccountStd.AccountReference.IsLoaded)
                            {
                                accRow.AccountStd.AccountReference.Load();
                                DataProjectLogCollector.LogLoadedEntityInExtension("accRow.AccountStd.AccountReference");
                            }
                            if (!accRow.AccountInternal.IsLoaded)
                            {
                                accRow.AccountInternal.Load();
                                DataProjectLogCollector.LogLoadedEntityInExtension("accRow.AccountInternal");
                            }
                            foreach (var accInt in accRow.AccountInternal)
                            {
                                if (!accInt.AccountReference.IsLoaded)
                                {
                                    accInt.AccountReference.Load();
                                    DataProjectLogCollector.LogLoadedEntityInExtension("accInt.AccountReference");
                                }
                                if (accInt.Account != null && !accInt.Account.AccountDimReference.IsLoaded)
                                {
                                    accInt.Account.AccountDimReference.Load();
                                    DataProjectLogCollector.LogLoadedEntityInExtension("accInt.Account.AccountDimReference");
                                }
                            }
                        }
                    }
                }
            }
            catch (InvalidOperationException ex) { ex.ToString(); }

            #endregion

            var dto = new PaymentRowDTO
            {
                PaymentRowId = e.PaymentRowId,
                PaymentId = e.Payment != null ? e.Payment.PaymentId : 0,    // TODO: Add foreign key to model
                InvoiceId = e.InvoiceId,
                PaymentImportId = e.PaymentImport != null ? (int?)e.PaymentImport.PaymentImportId : null,   // TODO: Add foreign key to model
                VoucherHeadId = e.VoucherHeadId,
                SysPaymentTypeId = e.SysPaymentTypeId,
                SeqNr = e.SeqNr,
                CurrencyRate = e.CurrencyRate,
                CurrencyDate = e.CurrencyDate,
                Amount = e.Amount,
                AmountCurrency = e.AmountCurrency,
                AmountEntCurrency = e.AmountEntCurrency,
                AmountLedgerCurrency = e.AmountLedgerCurrency,
                AmountDiff = e.AmountDiff,
                AmountDiffCurrency = e.AmountDiffCurrency,
                AmountDiffEntCurrency = e.AmountDiffEntCurrency,
                AmountDiffLedgerCurrency = e.AmountDiffLedgerCurrency,
                BankFee = e.BankFee,
                BankFeeCurrency = e.BankFeeCurrency,
                BankFeeEntCurrency = e.BankFeeEntCurrency,
                BankFeeLedgerCurrency = e.BankFeeLedgerCurrency,
                HasPendingAmountDiff = e.HasPendingAmountDiff,
                HasPendingBankFee = e.HasPendingBankFee,
                IsSuggestion = e.IsSuggestion,
                PayDate = e.PayDate,
                PaymentNr = e.PaymentNr,
                Status = e.Status,
                StatusName = e.StatusName,
                StatusMsg = e.StatusMsg,
                Text = e.Text,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State,
                VoucherHasMultiplePayments = e.VoucherHasMultiplePayments,
                IsRestPayment = e.IsRestPayment,
                TransferStatus = e.TransferStatus
            };

            // Extensions
            if (e.Payment != null)
            {
                dto.PaymentMethodId = e.Payment.PaymentMethodId;
                if (e.Payment.Origin != null)
                {
                    dto.VoucherSeriesId = e.Payment.Origin.VoucherSeriesId;
                    dto.VoucherSeriesTypeId = e.Payment.Origin.VoucherSeries.VoucherSeriesTypeId;
                    dto.Description = e.Payment.Origin.Description;
                    dto.OriginStatus = e.Payment.Origin.Status;
                    dto.OriginDescription = e.Payment.Origin.Description;
                }
            }

            if (e.Invoice != null)
            {
                dto.BillingType = (TermGroup_BillingType)e.Invoice.BillingType;
                dto.ActorId = e.Invoice.ActorId;
                dto.CurrencyId = e.Invoice.CurrencyId;
                dto.VoucherDate = e.Invoice.VoucherDate;
                dto.InvoiceTotalAmount = e.Invoice.TotalAmount;
                dto.InvoiceTotalAmountCurrency = e.Invoice.TotalAmountCurrency;
                dto.VatAmount = e.Invoice.VATAmount;
                dto.VatAmountCurrency = e.Invoice.VATAmountCurrency;
                dto.PaidAmount = e.Invoice.PaidAmount;
                dto.PaidAmountCurrency = e.Invoice.PaidAmountCurrency;
                dto.FullyPaid = e.Invoice.FullyPayed;
            }

            if (includeAccountingRows)
            {
                dto.PaymentAccountRows = new List<AccountingRowDTO>();
                foreach (var row in e.PaymentAccountRow.Where(x => x.State == (int)SoeEntityState.Active))
                {
                    dto.PaymentAccountRows.Add(row.ToDTO(dims));
                }
            }

            return dto;
        }

        public static IEnumerable<PaymentRowDTO> ToDTOs(this IEnumerable<PaymentRow> l, bool includeAccountingRows, bool includePaymentOrigin, List<AccountDim> dims)
        {
            var dtos = new List<PaymentRowDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO(includeAccountingRows, includePaymentOrigin, dims));
                }
            }
            return dtos;
        }

        public static PaymentRowSmallDTO ToSmallDTO(this PaymentRow e)
        {
            if (e == null)
                return null;

            #region Try load

            try
            {
                if (!e.IsAdded())
                {
                    if (!e.PaymentReference.IsLoaded)
                        e.PaymentReference.Load();
                }
            }
            catch (InvalidOperationException ex) { ex.ToString(); }

            #endregion

            PaymentRowSmallDTO dto = new PaymentRowSmallDTO()
            {
                PaymentRowId = e.PaymentRowId,
                Amount = e.Amount,
                AmountCurrency = e.AmountCurrency,
                PayDate = e.PayDate,
                Created = e.Created,
                Status = e.Status,
            };


            if (e.Invoice != null)
            {
                if (!e.Invoice.CurrencyReference.IsLoaded)
                {
                    e.Invoice.CurrencyReference.Load();
                    DataProjectLogCollector.LogLoadedEntityInExtension("e.Invoice.CurrencyReference");
                }
                dto.CurrencyCode = e.Invoice.Currency.Code;
            }

            return dto;
        }

        public static IEnumerable<PaymentRowSmallDTO> ToSmallDTOs(this IEnumerable<PaymentRow> l)
        {
            var dtos = new List<PaymentRowSmallDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToSmallDTO());
                }
            }
            return dtos;
        }

        #endregion

        #region PaymentAccountRow

        public static AccountingRowDTO ToDTO(this PaymentAccountRow e, List<AccountDim> dims)
        {
            if (e == null)
                return null;

            #region Try load

            try
            {
                if (!e.IsAdded())
                {
                    if (!e.PaymentRowReference.IsLoaded)
                    {
                        e.PaymentRowReference.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("e.PaymentRowReference");
                    }
                    if (!e.AccountStdReference.IsLoaded)
                    {
                        e.AccountStdReference.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("e.AccountStdReference");
                    }
                    if (e.AccountStd != null && !e.AccountStd.AccountReference.IsLoaded)
                    {
                        e.AccountStd.AccountReference.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("e.AccountStd.AccountReference");
                    }
                    if (!e.AccountInternal.IsLoaded)
                    {
                        e.AccountInternal.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("e.AccountInternal");
                    }
                    if (e.AccountInternal != null)
                    {
                        foreach (var accInt in e.AccountInternal)
                        {
                            if (!accInt.AccountReference.IsLoaded)
                            {
                                accInt.AccountReference.Load();
                                DataProjectLogCollector.LogLoadedEntityInExtension("accInt.AccountReference");
                            }
                            if (accInt.Account != null && !accInt.Account.AccountDimReference.IsLoaded)
                            {
                                accInt.Account.AccountDimReference.Load();
                                DataProjectLogCollector.LogLoadedEntityInExtension("accInt.Account.AccountDimReference");
                            }
                        }
                    }
                }
            }
            catch (InvalidOperationException ex) { ex.ToString(); }

            #endregion

            AccountingRowDTO dto = new AccountingRowDTO()
            {
                InvoiceRowId = e.PaymentRow != null ? e.PaymentRow.PaymentRowId : 0,    // TODO: Add foreign key to model
                TempInvoiceRowId = e.PaymentRow != null ? e.PaymentRow.PaymentRowId : 0,
                InvoiceAccountRowId = e.PaymentAccountRowId,
                TempRowId = e.PaymentAccountRowId,
                VoucherRowId = e.VoucherRowId,
                RowNr = e.RowNr,
                Text = e.Text,
                Quantity = e.Quantity,
                Amount = e.Amount,
                CreditAmount = e.Amount < 0 ? Math.Abs(e.Amount) : 0,
                DebitAmount = e.Amount > 0 ? e.Amount : 0,
                AmountCurrency = e.AmountCurrency,
                CreditAmountCurrency = e.AmountCurrency < 0 ? Math.Abs(e.AmountCurrency) : 0,
                DebitAmountCurrency = e.AmountCurrency > 0 ? e.AmountCurrency : 0,
                AmountEntCurrency = e.AmountEntCurrency,
                CreditAmountEntCurrency = e.AmountEntCurrency < 0 ? Math.Abs(e.AmountEntCurrency) : 0,
                DebitAmountEntCurrency = e.AmountEntCurrency > 0 ? e.AmountEntCurrency : 0,
                AmountLedgerCurrency = e.AmountLedgerCurrency,
                CreditAmountLedgerCurrency = e.AmountLedgerCurrency < 0 ? Math.Abs(e.AmountLedgerCurrency) : 0,
                DebitAmountLedgerCurrency = e.AmountLedgerCurrency > 0 ? e.AmountLedgerCurrency : 0,
                IsCreditRow = e.CreditRow,
                IsDebitRow = e.DebitRow,
                IsVatRow = e.VatRow,
                State = (SoeEntityState)e.State,
            };

            // Accounts
            AccountStd accStd = e.AccountStd;
            if (accStd != null)
            {
                dto.Dim1Id = accStd.AccountId;
                dto.Dim1Nr = accStd.Account != null ? accStd.Account.AccountNr : String.Empty;
                dto.Dim1Name = accStd.Account != null ? accStd.Account.Name : String.Empty;
                dto.Dim1Disabled = false;
                dto.Dim1Mandatory = true;
                dto.QuantityStop = accStd.UnitStop;
                dto.Unit = accStd.Unit;
                dto.AmountStop = accStd.AmountStop;
                dto.RowTextStop = accStd.RowTextStop;
            }

            if (e.AccountInternal != null)
            {
                foreach (AccountInternal accInt in e.AccountInternal)
                {
                    if (accInt.Account != null && accInt.Account.AccountDim != null)
                    {
                        var dimPos = dims.IndexOf(dims.FirstOrDefault(x => x.AccountDimNr == accInt.Account.AccountDim.AccountDimNr)) + 1;
                        switch (dimPos)
                        {
                            case 2:
                                dto.Dim2Id = accInt.AccountId;
                                dto.Dim2Nr = accInt.Account.AccountNr;
                                dto.Dim2Name = accInt.Account.Name;
                                break;
                            case 3:
                                dto.Dim3Id = accInt.AccountId;
                                dto.Dim3Nr = accInt.Account.AccountNr;
                                dto.Dim3Name = accInt.Account.Name;
                                break;
                            case 4:
                                dto.Dim4Id = accInt.AccountId;
                                dto.Dim4Nr = accInt.Account.AccountNr;
                                dto.Dim4Name = accInt.Account.Name;
                                break;
                            case 5:
                                dto.Dim5Id = accInt.AccountId;
                                dto.Dim5Nr = accInt.Account.AccountNr;
                                dto.Dim5Name = accInt.Account.Name;
                                break;
                            case 6:
                                dto.Dim6Id = accInt.AccountId;
                                dto.Dim6Nr = accInt.Account.AccountNr;
                                dto.Dim6Name = accInt.Account.Name;
                                break;
                        }
                    }
                }
            }

            return dto;
        }

        #endregion

        #region PriceBasedMarkup

        public static PriceBasedMarkupDTO ToDTO(this PriceBasedMarkup e)
        {
            if (e == null)
                return null;

            var dto = new PriceBasedMarkupDTO
            {
                PriceBasedMarkupId = e.PriceBasedMarkupId,
                PriceListTypeId = e.PriceListTypeId,
                MinPrice = e.MinPrice,
                MaxPrice = e.MaxPrice,
                MarkupPercent = e.MarkupPercent,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State,
            };

            return dto;
        }

        public static IEnumerable<PriceBasedMarkupDTO> ToDTOs(this IEnumerable<PriceBasedMarkup> l)
        {
            var dtos = new List<PriceBasedMarkupDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        #endregion

        #region PriceList

        public static PriceListDTO ToDTO(this PriceList e)
        {
            if (e == null)
                return null;

            var dto = new PriceListDTO
            {
                ProductId = e.ProductId,
                PriceListTypeId = e.PriceListTypeId,
                Price = e.Price,
                Quantity = e.Quantity,
                StartDate = e.StartDate,
                StopDate = e.StopDate,
                PriceListId = e.PriceListId,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
            };

            return dto;
        }

        public static IEnumerable<PriceListDTO> ToDTOs(this IEnumerable<PriceList> l)
        {
            var dtos = new List<PriceListDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static IEnumerable<ProductComparisonDTO> ToProductComparisonDTOs(this IEnumerable<InvoiceProduct> l, int comparisonPriceListTypeId, int priceListTypeId, DateTime? priceDate = null)
        {
            var dtos = new List<ProductComparisonDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    if (!e.PriceList.IsLoaded)
                    {
                        e.PriceList.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("e.PriceList");
                    }

                    ProductComparisonDTO dto = new ProductComparisonDTO
                    {
                        Name = e.Name,
                        Number = e.Number,
                        ProductId = e.ProductId,
                        PurchasePrice = e.PurchasePrice,
                        StartDate = new DateTime(1901, 1, 1),
                        StopDate = new DateTime(9999, 1, 1)
                    };

                    DateTime currentDate = priceDate != null ? priceDate.Value : DateTime.Now;

                    var comparisonPriceList = e.PriceList.LastOrDefault(p => p.PriceListTypeId == comparisonPriceListTypeId && currentDate.Date.CompareTo(p.StartDate.Date) > -1 && currentDate.Date.CompareTo(p.StopDate.Date) < 1);
                    if (comparisonPriceList != null)
                    {
                        dto.ComparisonPrice = comparisonPriceList.Price;
                    }

                    var currentPriceList = e.PriceList.LastOrDefault(p => p.PriceListTypeId == priceListTypeId && currentDate.Date.CompareTo(p.StartDate.Date) > -1 && currentDate.Date.CompareTo(p.StopDate.Date) < 1);
                    if (currentPriceList != null)
                    {
                        dto.Price = currentPriceList.Price;
                        dto.StartDate = currentPriceList.StartDate;
                        dto.StopDate = currentPriceList.StopDate;
                    }

                    dtos.Add(dto);
                }
            }
            return dtos;
        }

        public static IEnumerable<ProductComparisonDTO> ToProductComparisonDTOs(this IEnumerable<PriceList> l, int comparisonPriceListTypeId, int priceListTypeId)
        {
            Dictionary<int, ProductComparisonDTO> retList = new Dictionary<int, ProductComparisonDTO>();
            foreach (var item in l)
            {
                if (!item.ProductReference.IsLoaded)
                {
                    item.ProductReference.Load();
                    DataProjectLogCollector.LogLoadedEntityInExtension("item.ProductReference");
                }

                ProductComparisonDTO dto;
                if (retList.ContainsKey(item.ProductId))
                {
                    dto = retList[item.ProductId];
                }
                else
                {
                    dto = new ProductComparisonDTO()
                    {
                        Name = item.Product.Name,
                        Number = item.Product.Number,
                        ProductId = item.Product.ProductId,
                    };

                    retList.Add(dto.ProductId, dto);
                }

                if (item.PriceListTypeId == comparisonPriceListTypeId)
                    dto.ComparisonPrice = item.Price;
                if (item.PriceListTypeId == priceListTypeId)
                    dto.Price = item.Price;
            }

            return retList.Values;
        }

        public static PriceListTypeDTO ToDTO(this PriceListType e)
        {
            if (e == null)
                return null;

            #region Try load

            try
            {
                if (!e.IsAdded())
                {
                    if (!e.CurrencyReference.IsLoaded)
                    {
                        e.CurrencyReference.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("e.CurrencyReference");
                    }
                }
            }
            catch (InvalidOperationException ex) { ex.ToString(); }

            #endregion

            var dto = new PriceListTypeDTO()
            {
                PriceListTypeId = e.PriceListTypeId,
                CurrencyId = e.Currency != null ? e.Currency.CurrencyId : 0,        // TODO: Add foreign key to model
                Name = e.Name,
                Description = e.Description,
                DiscountPercent = e.DiscountPercent,
                InclusiveVat = e.InclusiveVat,
                IsProjectPriceList = e.IsProjectPriceList.GetValueOrDefault(),
                State = (SoeEntityState)e.State,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
            };

            return dto;
        }

        public static IEnumerable<PriceListTypeDTO> ToDTOs(this IEnumerable<PriceListType> l)
        {
            var dtos = new List<PriceListTypeDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        #endregion

        #region PriceRule

        public static PriceRuleDTO ToDTO(this PriceRule e)
        {
            if (e == null)
                return null;

            #region Try load

            try
            {
                if (!e.IsAdded())
                {
                    if (!e.LRuleReference.IsLoaded)
                    {
                        e.LRuleReference.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("e.LRuleReference");
                    }
                    if (!e.RRuleReference.IsLoaded)
                    {
                        e.RRuleReference.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("e.RRuleReference");
                    }
                }
            }
            catch (InvalidOperationException ex) { ex.ToString(); }

            #endregion

            var dto = new PriceRuleDTO
            {
                RuleId = e.RuleId,
                LRuleId = e.LRule != null ? e.LRule.RuleId : (int?)null,            // TODO: Add foreign key to model
                LValueType = e.LValueType,
                LValue = e.LValue,
                RRuleId = e.RRule != null ? e.RRule.RuleId : (int?)null,            // TODO: Add foreign key to model
                RValueType = e.RValueType,
                RValue = e.RValue,
                OperatorType = e.OperatorType,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                UseNetPrice = e.UseNetPrice,

                //Set FK
                PriceListTypeId = e.PriceListType != null ? e.PriceListType.PriceListTypeId : 0,    // TODO: Add foreign key to model
                CompanyWholesellerPriceListId = e.CompanyWholesellerPricelist != null ? e.CompanyWholesellerPricelist.CompanyWholesellerPriceListId : (int?)null,   // TODO: Add foreign key to model
                PriceListImportedHeadId = e.PriceListImportedHeadId
            };

            // Extensions
            if (e.LRule != null)
                dto.LRule = e.LRule.ToDTO();
            if (e.RRule != null)
                dto.RRule = e.RRule.ToDTO();

            dto.lExampleType = e.lExampleType;
            dto.rExampleType = e.rExampleType;

            return dto;
        }

        public static IEnumerable<PriceRuleDTO> ToDTOs(this IEnumerable<PriceRule> l)
        {
            var dtos = new List<PriceRuleDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        #endregion

        #region ProductGroup

        public static IEnumerable<ProductGroupGridDTO> ToGridDTOs(this IEnumerable<ProductGroup> l)
        {
            var dtos = new List<ProductGroupGridDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToGridDTO());
                }
            }
            return dtos;
        }

        public static ProductGroupGridDTO ToGridDTO(this ProductGroup e)
        {
            if (e == null)
                return null;

            ProductGroupGridDTO dto = new ProductGroupGridDTO()
            {
                ProductGroupId = e.ProductGroupId,
                Code = e.Code,
                Name = e.Name,
            };

            return dto;
        }

        #endregion

        #region PurchaseCart

        public static PurchaseCartDTO ToDTO(this PurchaseCart p)
        {
            if (p == null)
                return null;

            var dto = new PurchaseCartDTO
            {
                PurchaseCartId = p.PurchaseCartId,
                Name = p.Name,
                Description = p.Description,
                Created = p.Created,
                CreatedBy = p.CreatedBy,
                Modified = p.Modified,
                ModifiedBy = p.ModifiedBy,
                Status = p.Status,
                State = (SoeEntityState)p.State,
                PriceStrategy = p.PriceStrategy,
                SeqNr = p.SeqNr,
                SelectedWholesellerIds = p.SelectedWholesellerIds.Split(',').Select(int.Parse).ToList(),
            };

            return dto;
        }
        #endregion

        #region InvoiceProduct

        public static InvoiceProductDTO ToDTO(this InvoiceProduct e, bool includePriceLists, bool includeAccountSettings)
        {
            if (e == null)
                return null;

            #region Try load

            try
            {
                if (!e.IsAdded())
                {
                    if (includeAccountSettings)
                    {
                        if (!e.ProductAccountStd.IsLoaded)
                        {
                            e.ProductAccountStd.Load();
                            DataProjectLogCollector.LogLoadedEntityInExtension("e.ProductAccountStd");
                        }
                        if (e.ProductAccountStd != null && e.ProductAccountStd.Count > 0)
                        {
                            foreach (var accStd in e.ProductAccountStd)
                            {
                                if (!accStd.AccountStdReference.IsLoaded)
                                {
                                    accStd.AccountStdReference.Load();
                                    DataProjectLogCollector.LogLoadedEntityInExtension("accStd.AccountStdReference");
                                }
                                if (accStd.AccountStd != null && !accStd.AccountStd.AccountReference.IsLoaded)
                                {
                                    accStd.AccountStd.AccountReference.Load();
                                    DataProjectLogCollector.LogLoadedEntityInExtension("accStd.AccountStd.AccountReference");
                                }
                                if (!accStd.AccountInternal.IsLoaded)
                                {
                                    accStd.AccountInternal.Load();
                                    DataProjectLogCollector.LogLoadedEntityInExtension("accStd.AccountInternal");
                                }
                                if (accStd.AccountInternal != null && accStd.AccountInternal.Count > 0)
                                {
                                    foreach (var accInt in accStd.AccountInternal)
                                    {
                                        if (!accInt.AccountReference.IsLoaded)
                                        {
                                            accInt.AccountReference.Load();
                                            DataProjectLogCollector.LogLoadedEntityInExtension("accInt.AccountReference");
                                        }
                                        if (accInt.Account != null && !accInt.Account.AccountDimReference.IsLoaded)
                                        {
                                            accInt.Account.AccountDimReference.Load();
                                            DataProjectLogCollector.LogLoadedEntityInExtension("accInt.Account.AccountDimReference");
                                        }
                                    }
                                }
                            }
                        }
                    }
                    if (includePriceLists && !e.PriceList.IsLoaded)
                    {
                        e.PriceList.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("e.PriceList");
                    }
                }
            }
            catch (InvalidOperationException ex) { ex.ToString(); }

            #endregion

            // Create ProductDTO
            ProductDTO dto = e.ToDTO();
            // Create InvoiceProductDTO and copy properties from ProductDTO
            InvoiceProductDTO ipdto = new InvoiceProductDTO();
            var properties = dto.GetType().GetProperties();
            foreach (var property in properties)
            {
                PropertyInfo pi = dto.GetType().GetProperty(property.Name);
                if (pi.CanWrite)
                    property.SetValue(ipdto, pi.GetValue(dto, null), null);
            }

            // Set InvoiceProduct specific properties
            ipdto.SysProductId = e.ExternalProductId;
            ipdto.SysPriceListHeadId = e.ExternalPriceListHeadId;
            ipdto.IsExternal = e.ExternalProductId.HasValue;
            ipdto.VatType = (TermGroup_InvoiceProductVatType)e.VatType;
            ipdto.VatFree = e.VatFree;
            ipdto.EAN = e.EAN;
            ipdto.PurchasePrice = e.PurchasePrice;
            ipdto.SysWholesellerName = e.SysWholesellerName;
            ipdto.CalculationType = (TermGroup_InvoiceProductCalculationType)e.CalculationType;
            ipdto.GuaranteePercentage = e.GuaranteePercentage;
            ipdto.TimeCodeId = e.TimeCodeId;
            ipdto.PriceListOrigin = e.PriceListOrigin;
            ipdto.ShowDescriptionAsTextRow = e.ShowDescriptionAsTextRow;
            ipdto.ShowDescrAsTextRowOnPurchase = e.ShowDescrAsTextRowOnPurchase;
            ipdto.DontUseDiscountPercent = e.DontUseDiscountPercent ?? false;
            ipdto.VatCodeId = e.VatCodeId;
            ipdto.HouseholdDeductionPercentage = e.HouseholdDeductionPercentage;
            ipdto.IsStockProduct = e.IsStockProduct ?? false;
            ipdto.HouseholdDeductionType = e.HouseholdDeductionType;
            ipdto.UseCalculatedCost = e.UseCalculatedCost ?? false;
            ipdto.Weight = e.Weight;
            ipdto.IntrastatCodeId = e.IntrastatCodeId;
            ipdto.SysCountryId = e.SysCountryId;
            ipdto.SysProductType = e.SysProductType;
            ipdto.DefaultGrossMarginCalculationType = e.DefaultGrossMarginCalculationType;

            // Relations
            if (includeAccountSettings)
            {
                ipdto.AccountingSettings = new List<AccountingSettingsRowDTO>();

                if (!e.ProductAccountStd.IsNullOrEmpty())
                {
                    AddAccountingSettingsRowDTO(e, ipdto, ProductAccountType.Purchase);
                    AddAccountingSettingsRowDTO(e, ipdto, ProductAccountType.Sales);
                    AddAccountingSettingsRowDTO(e, ipdto, ProductAccountType.VAT);
                    AddAccountingSettingsRowDTO(e, ipdto, ProductAccountType.SalesNoVat);
                    AddAccountingSettingsRowDTO(e, ipdto, ProductAccountType.SalesContractor);
                    AddAccountingSettingsRowDTO(e, ipdto, ProductAccountType.StockIn);
                    AddAccountingSettingsRowDTO(e, ipdto, ProductAccountType.StockInChange);
                    AddAccountingSettingsRowDTO(e, ipdto, ProductAccountType.StockOut);
                    AddAccountingSettingsRowDTO(e, ipdto, ProductAccountType.StockOutChange);
                    AddAccountingSettingsRowDTO(e, ipdto, ProductAccountType.StockInv);
                    AddAccountingSettingsRowDTO(e, ipdto, ProductAccountType.StockInvChange);
                    AddAccountingSettingsRowDTO(e, ipdto, ProductAccountType.StockLoss);
                    AddAccountingSettingsRowDTO(e, ipdto, ProductAccountType.StockLossChange);
                    AddAccountingSettingsRowDTO(e, ipdto, ProductAccountType.StockTransferChange);
                }
            }

            if (includePriceLists)
                ipdto.PriceLists = (e.PriceList != null && e.PriceList.Count > 0) ? e.PriceList.Where(x => x.State == (int)SoeEntityState.Active).ToDTOs().ToList() : new List<PriceListDTO>();

            ipdto.CategoryIds = e.CategoryIds;

            return ipdto;
        }

        public static InvoiceProductGridDTO ToGridDTO(this InvoiceProduct e)
        {
            if (e == null)
                return null;

            #region Try load

            try
            {
                if (!e.IsAdded())
                {
                    if (!e.ProductGroupReference.IsLoaded)
                    {
                        e.ProductGroupReference.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("e.ProductGroupReference");
                    }
                }
            }
            catch (InvalidOperationException ex) { ex.ToString(); }

            #endregion

            InvoiceProductGridDTO dto = new InvoiceProductGridDTO()
            {
                ProductId = e.ProductId,
                Number = e.Number,
                NumberSort = e.NumberSort,
                Name = e.Name,
                ProductGroupId = e.ProductGroupId,
                ProductGroupCode = e.ProductGroup != null ? e.ProductGroup.Code : String.Empty,
                ProductGroupName = e.ProductGroup != null ? e.ProductGroup.Name : String.Empty,
                ProductCategories = e.CategoryNames != null ? e.CategoryNamesString : String.Empty,
                SysProductId = e.ExternalProductId,
                External = e.ExternalProductId.HasValue,
                VatType = (TermGroup_InvoiceProductVatType)e.VatType,
                State = (SoeEntityState)e.State,
                EanCode = e.EAN,
                TimeCodeId = e.TimeCode != null ? e.TimeCode.TimeCodeId : (int?)null,
                TimeCodeName = e.TimeCode != null ? e.TimeCode.Name : String.Empty
            };

            return dto;
        }

        public static IEnumerable<ProductDTO> ToDTOs(this IEnumerable<InvoiceProduct> l)
        {
            var dtos = new List<ProductDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }


        public static IEnumerable<InvoiceProductGridDTO> ToGridDTOs(this IEnumerable<InvoiceProduct> l)
        {
            var dtos = new List<InvoiceProductGridDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToGridDTO());
                }
            }
            return dtos;
        }

        public static ProductRowsProductDTO ToProductRowsProductDTO(this InvoiceProduct e)
        {
            if (e == null)
                return null;

            #region Try load

            try
            {
                if (!e.IsAdded())
                {
                    if (!e.ProductUnitReference.IsLoaded)
                    {
                        e.ProductUnitReference.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("e.ProductUnitReference");
                    }
                }
            }
            catch (InvalidOperationException ex) { ex.ToString(); }

            #endregion

            var dto = new ProductRowsProductDTO
            {
                ProductId = e.ProductId,
                SysProductId = e.ExternalProductId,
                Number = e.Number,
                Name = e.Name,
                Description = e.Description,
                ShowDescriptionAsTextRow = e.ShowDescriptionAsTextRow,
                ShowDescrAsTextRowOnPurchase = e.ShowDescrAsTextRowOnPurchase,
                ProductUnitId = e.ProductUnitId,
                ProductUnitCode = e.ProductUnit != null ? e.ProductUnit.Code : string.Empty,
                VatType = (TermGroup_InvoiceProductVatType)e.VatType,
                CalculationType = (TermGroup_InvoiceProductCalculationType)e.CalculationType,
                GuaranteePercentage = e.GuaranteePercentage,
                SysWholesellerName = e.SysWholesellerName,
                DontUseDiscountPercent = e.DontUseDiscountPercent ?? false,
                PurchasePrice = e.PurchasePrice,
                SalesPrice = 0, // TODO: Set Sales price?
                VatCodeId = e.VatCodeId,
                IsStockProduct = e.IsStockProduct ?? false,
                IsSupplementCharge = e.IsSupplementCharge,
                IsLiftProduct = e.CalculationType == (int)TermGroup_InvoiceProductCalculationType.Lift,
                HouseholdDeductionType = e.HouseholdDeductionType,
                HouseholdDeductionPercentage = e.HouseholdDeductionPercentage,
                IsInactive = e.State == (int)SoeEntityState.Inactive,
                IntrastatCodeId = e.IntrastatCodeId,
                SysCountryId = e.SysCountryId,
                GrossMarginCalculationType = e.DefaultGrossMarginCalculationType.HasValue ? (TermGroup_GrossMarginCalculationType)e.DefaultGrossMarginCalculationType : TermGroup_GrossMarginCalculationType.Unknown,
                Weight = e.Weight,
                IsExternal = e.ExternalProductId.GetValueOrDefault() > 0
            };

            return dto;
        }

        public static IEnumerable<ProductRowsProductDTO> ToProductRowsProductDTOs(this IEnumerable<InvoiceProduct> l)
        {
            var dtos = new List<ProductRowsProductDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToProductRowsProductDTO());
                }
            }
            return dtos;
        }

        public static ProductTimeCodeDTO ToProductTimeCodeDTO(this InvoiceProduct e)
        {
            if (e == null)
                return null;

            return new ProductTimeCodeDTO
            {
                Id = e.ProductId,
                Name = e.Name,
                VatType = (TermGroup_InvoiceProductVatType)e.VatType,
                State = e.State
            };
        }

        public static IEnumerable<ProductTimeCodeDTO> ToProductTimeCodeDTOs(this IEnumerable<InvoiceProduct> l)
        {
            var dtos = new List<ProductTimeCodeDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToProductTimeCodeDTO());
                }
            }
            return dtos;
        }

        #endregion

        #region Project

        public static ProjectDTO ToDTO(this Project e, bool includeAccountSettings, bool useAccountSettingsDict)
        {
            if (e == null)
                return null;

            #region Try load

            try
            {
                if (includeAccountSettings && !e.IsAdded())
                {
                    if (!e.ProjectAccountStd.IsLoaded)
                    {
                        e.ProjectAccountStd.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("e.ProjectAccountStd");
                    }
                }
            }
            catch (InvalidOperationException ex) { ex.ToString(); }

            #endregion

            var dto = new ProjectDTO()
            {
                ProjectId = e.ProjectId,
                Type = (TermGroup_ProjectType)e.Type,
                ActorCompanyId = e.ActorCompanyId,
                ParentProjectId = e.ParentProjectId,
                CustomerId = e.CustomerId,
                Status = (TermGroup_ProjectStatus)e.Status,
                AllocationType = (TermGroup_ProjectAllocationType)e.AllocationType,
                StatusName = e.StatusName,
                Number = e.Number,
                Name = e.Name,
                Description = e.Description,
                StartDate = e.StartDate,
                StopDate = e.StopDate,
                Note = e.Note,
                UseAccounting = e.UseAccounting,
                PriceListTypeId = e.PriceListTypeId,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State,
                WorkSiteKey = e.WorkSiteKey,
                WorkSiteNumber = e.WorkSiteNumber,
                AttestWorkFlowHeadId = e.AttestWorkFlowHeadId,
                DefaultDim1AccountId = e.DefaultDim1AccountId,
                DefaultDim2AccountId = e.DefaultDim2AccountId,
                DefaultDim3AccountId = e.DefaultDim3AccountId,
                DefaultDim4AccountId = e.DefaultDim4AccountId,
                DefaultDim5AccountId = e.DefaultDim5AccountId,
                DefaultDim6AccountId = e.DefaultDim6AccountId

            };

            // Accounts
            if (includeAccountSettings)
            {
                if (useAccountSettingsDict)
                {
                    dto.AccountingSettings = new List<AccountingSettingsRowDTO>();

                    if (e.ProjectAccountStd != null && e.ProjectAccountStd.Count > 0)
                    {
                        AddAccountingSettingsRowDTO(e, dto, ProjectAccountType.Debit);
                        AddAccountingSettingsRowDTO(e, dto, ProjectAccountType.Credit);
                        AddAccountingSettingsRowDTO(e, dto, ProjectAccountType.SalesNoVat);
                        AddAccountingSettingsRowDTO(e, dto, ProjectAccountType.SalesContractor);
                    }
                }
                else if (e.ProjectAccountStd != null && e.ProjectAccountStd.Count > 0)
                {
                    // Credit
                    ProjectAccountStd accStd = e.ProjectAccountStd.FirstOrDefault(c => c.Type == (int)ProjectAccountType.Credit);
                    Account account = accStd?.AccountStd?.Account;
                    dto.CreditAccounts = new Dictionary<int, AccountSmallDTO>();
                    dto.CreditAccounts.Add(Constants.ACCOUNTDIM_STANDARD, account.ToSmallDTO());
                    if (accStd != null && accStd.AccountInternal != null)
                    {
                        foreach (var accInt in accStd.AccountInternal.Where(a => a.Account != null && a.Account.AccountDim != null && a.Account.AccountDim.AccountDimNr != Constants.ACCOUNTDIM_STANDARD))
                        {
                            dto.CreditAccounts.Add(accInt.Account.AccountDim.AccountDimNr, accInt.Account.ToSmallDTO());
                        }
                    }

                    // Debit
                    accStd = e.ProjectAccountStd.FirstOrDefault(c => c.Type == (int)ProjectAccountType.Debit);
                    account = accStd?.AccountStd?.Account;
                    dto.DebitAccounts = new Dictionary<int, AccountSmallDTO>();
                    dto.DebitAccounts.Add(Constants.ACCOUNTDIM_STANDARD, account.ToSmallDTO());
                    if (accStd != null && accStd.AccountInternal != null)
                    {
                        foreach (var accInt in accStd.AccountInternal.Where(a => a.Account != null && a.Account.AccountDim != null && a.Account.AccountDim.AccountDimNr != Constants.ACCOUNTDIM_STANDARD))
                        {
                            dto.DebitAccounts.Add(accInt.Account.AccountDim.AccountDimNr, accInt.Account.ToSmallDTO());
                        }
                    }

                    // NoVat
                    accStd = e.ProjectAccountStd.FirstOrDefault(c => c.Type == (int)ProjectAccountType.SalesNoVat);
                    account = accStd?.AccountStd?.Account;
                    dto.SalesNoVatAccounts = new Dictionary<int, AccountSmallDTO>();
                    dto.SalesNoVatAccounts.Add(Constants.ACCOUNTDIM_STANDARD, account.ToSmallDTO());
                    if (accStd != null && accStd.AccountInternal != null)
                    {
                        foreach (var accInt in accStd.AccountInternal.Where(a => a.Account != null && a.Account.AccountDim != null && a.Account.AccountDim.AccountDimNr != Constants.ACCOUNTDIM_STANDARD))
                        {
                            dto.SalesNoVatAccounts.Add(accInt.Account.AccountDim.AccountDimNr, accInt.Account.ToSmallDTO());
                        }
                    }

                    // Contractor
                    accStd = e.ProjectAccountStd.FirstOrDefault(c => c.Type == (int)ProjectAccountType.SalesContractor);
                    account = accStd?.AccountStd?.Account;
                    dto.SalesContractorAccounts = new Dictionary<int, AccountSmallDTO>();
                    dto.SalesContractorAccounts.Add(Constants.ACCOUNTDIM_STANDARD, account.ToSmallDTO());
                    if (accStd != null && accStd.AccountInternal != null)
                    {
                        foreach (var accInt in accStd.AccountInternal.Where(a => a.Account != null && a.Account.AccountDim != null && a.Account.AccountDim.AccountDimNr != Constants.ACCOUNTDIM_STANDARD))
                        {
                            dto.SalesContractorAccounts.Add(accInt.Account.AccountDim.AccountDimNr, accInt.Account.ToSmallDTO());
                        }
                    }
                }
            }

            return dto;
        }

        private static void AddAccountingSettingsRowDTO(Project project, ProjectDTO dto, ProjectAccountType type)
        {
            AccountingSettingsRowDTO accDto = new AccountingSettingsRowDTO()
            {
                Type = (int)type,
                Percent = 0
            };
            dto.AccountingSettings.Add(accDto);

            ProjectAccountStd accStd = project.ProjectAccountStd.FirstOrDefault(c => c.Type == (int)type);
            Account account = accStd?.AccountStd?.Account;
            if (account != null)
            {
                accDto.AccountDim1Nr = Constants.ACCOUNTDIM_STANDARD;
                accDto.Account1Id = account.AccountId;
                accDto.Account1Nr = account.AccountNr;
                accDto.Account1Name = account.Name;
            }

            if (accStd != null && accStd.AccountInternal != null)
            {
                int dimCounter = 2;
                foreach (var accInt in accStd.AccountInternal.Where(a => a.Account != null && a.Account.AccountDim != null && a.Account.AccountDim.AccountDimNr != Constants.ACCOUNTDIM_STANDARD).OrderBy(a => a.Account.AccountDim.AccountDimNr))
                {
                    account = accInt.Account;

                    // TODO: Does not support dim numbers over 6!!!
                    if (dimCounter == 2)
                    {
                        accDto.AccountDim2Nr = account.AccountDim.AccountDimNr;
                        accDto.Account2Id = account.AccountId;
                        accDto.Account2Nr = account.AccountNr;
                        accDto.Account2Name = account.Name;
                    }
                    else if (dimCounter == 3)
                    {
                        accDto.AccountDim3Nr = account.AccountDim.AccountDimNr;
                        accDto.Account3Id = account.AccountId;
                        accDto.Account3Nr = account.AccountNr;
                        accDto.Account3Name = account.Name;
                    }
                    else if (dimCounter == 4)
                    {
                        accDto.AccountDim4Nr = account.AccountDim.AccountDimNr;
                        accDto.Account4Id = account.AccountId;
                        accDto.Account4Nr = account.AccountNr;
                        accDto.Account4Name = account.Name;
                    }
                    else if (dimCounter == 5)
                    {
                        accDto.AccountDim5Nr = account.AccountDim.AccountDimNr;
                        accDto.Account5Id = account.AccountId;
                        accDto.Account5Nr = account.AccountNr;
                        accDto.Account5Name = account.Name;
                    }
                    else if (dimCounter == 6)
                    {
                        accDto.AccountDim6Nr = account.AccountDim.AccountDimNr;
                        accDto.Account6Id = account.AccountId;
                        accDto.Account6Nr = account.AccountNr;
                        accDto.Account6Name = account.Name;
                    }

                    dimCounter++;
                }
            }
        }

        public static ProjectSmallDTO ToSmallDTO(this Project e, int? userId, bool setUsers = false, bool setCustomer = false)
        {
            if (e == null)
                return null;

            ProjectSmallDTO dto = new ProjectSmallDTO()
            {
                ProjectId = e.ProjectId,
                Number = e.Number,
                Name = e.Name,
                NumberName = e.Number + " " + e.Name,
            };

            // Extensions
            dto.CustomerId = e.CustomerId;
            if (e.Customer != null)
                dto.CustomerName = e.Customer.Name;
            if (userId.HasValue && e.ProjectUser != null && e.ProjectUser.Any(u => u.UserId == userId))
                dto.TimeCodeId = e.ProjectUser.FirstOrDefault(u => u.UserId == userId)?.TimeCodeId;
            if (setUsers)
                dto.ProjectUsers = e.ProjectUser.Select(u => u.UserId).ToList();
            dto.AllocationType = (TermGroup_ProjectAllocationType)e.AllocationType;
            if (e.Invoice != null && e.Invoice.Count > 0)
            {
                dto.Invoices = new List<ProjectInvoiceSmallDTO>();
                dto.Invoices.Add(new ProjectInvoiceSmallDTO() { InvoiceId = 0, ProjectId = 0, InvoiceNr = " " });

                foreach (Invoice inv in e.Invoice.Where(i => i.Origin.Type == (int)SoeOriginType.Order && (i.Origin.Status == (int)SoeOriginStatus.Origin || i.Origin.Status == (int)SoeOriginStatus.OrderPartlyInvoice)).OrderBy(o => o.SeqNr))
                {
                    if (setCustomer)
                    {
                        /*if (!inv.ActorReference.IsLoaded)
                            inv.ActorReference.Load();

                        if (inv.Actor != null && !inv.Actor.CustomerReference.IsLoaded)
                            inv.Actor.CustomerReference.Load();*/

                        dto.Invoices.Add(new ProjectInvoiceSmallDTO()
                        {
                            ProjectId = dto.ProjectId,
                            InvoiceId = inv.InvoiceId,
                            InvoiceNr = inv.InvoiceNr,
                            CustomerName = inv.Actor != null && inv.Actor.Customer != null ? inv.Actor.Customer.Name : " ",
                            NumberName = $"{inv.InvoiceNr} - {Regex.Replace(inv?.Actor?.Customer?.Name ?? "", @"\t|\n|\r", " ")}"  //inv.Actor != null && inv.Actor.Customer != null ? inv.InvoiceNr + " - " + inv.Actor.Customer.Name : " "
                        });
                    }
                    else
                    {
                        dto.Invoices.Add(new ProjectInvoiceSmallDTO() { ProjectId = dto.ProjectId, InvoiceId = inv.InvoiceId, InvoiceNr = inv.InvoiceNr });
                    }
                }
            }
            return dto;
        }

        public static IEnumerable<ProjectSmallDTO> ToSmallDTOs(this IEnumerable<Project> l, int? userId, bool setUsers = false, bool setCustomer = false)
        {
            var dtos = new List<ProjectSmallDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToSmallDTO(userId, setUsers, setCustomer));
                }
            }
            return dtos;
        }

        public static ProjectTinyDTO ToTinyDTO(this Project e)
        {
            if (e == null)
                return null;

            ProjectTinyDTO dto = new ProjectTinyDTO()
            {
                ProjectId = e.ProjectId,
                Number = e.Number,
                Name = e.Name,
                Status = (TermGroup_ProjectStatus)e.Status
            };

            return dto;
        }

        public static IEnumerable<ProjectTinyDTO> ToTinyDTOs(this IEnumerable<Project> l)
        {
            var dtos = new List<ProjectTinyDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToTinyDTO());
                }
            }
            return dtos;
        }

        #region CaseProject

        public static CaseProjectDTO ToDTO(this CaseProject e, bool includeNotes)
        {
            if (e == null)
                return null;

            // Create ProjectDTO
            ProjectDTO dto = (e as Project).ToDTO(false, false);

            // Create CaseProjectDTO and copy properties from ProjectDTO
            CaseProjectDTO cpdto = new CaseProjectDTO();
            var properties = dto.GetType().GetProperties();
            foreach (var property in properties)
            {
                PropertyInfo pi = dto.GetType().GetProperty(property.Name);
                if (pi.CanWrite)
                    property.SetValue(cpdto, pi.GetValue(dto, null), null);
            }

            #region Try load

            try
            {
                if (!e.IsAdded())
                {
                    if (!e.CaseProjectNote.IsLoaded)
                    {
                        e.CaseProjectNote.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("e.CaseProjectNote");
                    }
                }
            }
            catch (InvalidOperationException ex) { ex.ToString(); }

            #endregion

            // Set CaseProjectDTO specific properties
            cpdto.Application = (TermGroup_CaseProjectApplication)e.Application;
            cpdto.CaseProjectType = (TermGroup_CaseProjectType)e.CaseProjectType;
            cpdto.LicenseId = e.LicenseId;
            cpdto.CustomerCompanyId = e.CustomerCompanyId;
            cpdto.Channel = (TermGroup_CaseProjectChannel)e.Channel;
            cpdto.Priority = (TermGroup_CaseProjectPriority)e.Priority;
            cpdto.Area = (TermGroup_CaseProjectArea)e.Area;
            cpdto.Result = (TermGroup_CaseProjectResult)e.Result;
            cpdto.AttestStateId = e.AttestStateId;
            cpdto.ReportedByUserId = e.ReportedByUserId;
            cpdto.ResponsibleUserId = e.ResponsibleUserId;
            cpdto.ClosedByUserId = e.ClosedByUserId;
            cpdto.WorkItemNr = e.WorkItemNr;
            cpdto.SprintId = e.SprintId;
            cpdto.ElapsedTime = e.ElapsedTime;
            cpdto.StopwatchDisabled = e.StopwatchDisabled;

            // Extensions
            cpdto.Notes = new List<CaseProjectNoteDTO>();
            if (e.CaseProjectNote != null && e.CaseProjectNote.Count > 0)
                cpdto.Notes = e.CaseProjectNote.ToDTOs().ToList();

            return cpdto;
        }

        public static CaseProjectGridDTO ToGridDTO(this CaseProject e)
        {
            if (e == null)
                return null;

            #region Try load

            try
            {
                if (!e.IsAdded())
                {
                    if (!e.AttestStateReference.IsLoaded)
                    {
                        e.AttestStateReference.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("e.AttestStateReference");
                    }
                }
            }
            catch (InvalidOperationException ex) { ex.ToString(); }

            #endregion

            CaseProjectGridDTO dto = new CaseProjectGridDTO()
            {
                ProjectId = e.ProjectId,
                Number = e.Number,
                LicenseName = e.License != null ? e.License.Name : String.Empty,
                CustomerName = e.CustomerCompany != null ? e.CustomerCompany.Name : String.Empty,
                Name = e.Name,
                CaseProjectType = (TermGroup_CaseProjectType)e.CaseProjectType,
                Priority = (TermGroup_CaseProjectPriority)e.Priority,
                AttestStateId = e.AttestStateId,
                StatusName = e.AttestState != null ? e.AttestState.Name : String.Empty,
                StatusImageSource = e.AttestState != null ? e.AttestState.ImageSource : String.Empty
            };

            return dto;
        }

        public static IEnumerable<CaseProjectGridDTO> ToGridDTOs(this IEnumerable<CaseProject> l)
        {
            var dtos = new List<CaseProjectGridDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToGridDTO());
                }
            }
            return dtos;
        }

        public static CaseProjectNoteDTO ToDTO(this CaseProjectNote e)
        {
            if (e == null)
                return null;

            CaseProjectNoteDTO dto = new CaseProjectNoteDTO()
            {
                CaseProjectNoteId = e.CaseProjectNoteId,
                ProjectId = e.ProjectId,
                Type = (CaseProjectNoteType)e.Type,
                Timestamp = e.Timestamp,
                UserId = e.UserId,
                Note = e.Note
            };

            return dto;
        }

        public static IEnumerable<CaseProjectNoteDTO> ToDTOs(this IEnumerable<CaseProjectNote> l)
        {
            var dtos = new List<CaseProjectNoteDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        #endregion

        #region TimeProject

        public static TimeProjectDTO ToTimeProjectDTO(this Project e, bool includeAccountSettings, bool useAccountSettingsDict)
        {
            if (e == null)
                return null;

            // Create ProjectDTO
            ProjectDTO dto = e.ToDTO(includeAccountSettings, useAccountSettingsDict);

            if (dto.CreditAccounts == null)
                dto.CreditAccounts = new Dictionary<int, AccountSmallDTO>();

            if (dto.DebitAccounts == null)
                dto.DebitAccounts = new Dictionary<int, AccountSmallDTO>();

            // Create TimeProjectDTO and copy properties from ProjectDTO
            TimeProjectDTO tpdto = new TimeProjectDTO();
            var properties = dto.GetType().GetProperties();
            foreach (var property in properties)
            {
                PropertyInfo pi = dto.GetType().GetProperty(property.Name);
                if (pi.CanWrite)
                    property.SetValue(tpdto, pi.GetValue(dto, null), null);
            }

            //Budget
            if (e.BudgetHead != null && e.BudgetHead.Count > 0)
            {
                //For now we only get the first
                BudgetHead head = e.BudgetHead.OrderByDescending(h => h.BudgetHeadId).FirstOrDefault();

                BudgetHeadDTO hDto = new BudgetHeadDTO()
                {
                    BudgetHeadId = head.BudgetHeadId,
                    ActorCompanyId = head.ActorCompanyId,
                    AccountYearId = head.AccountYearId,
                    DistributionCodeHeadId = head.DistributionCodeHeadId,
                    Name = head.Name,
                    NoOfPeriods = head.NoOfPeriods,
                    Type = head.Type,
                    Status = head.Status,
                    FromDate = head.FromDate,
                    ToDate = head.ToDate,
                    Created = head.Created,
                    CreatedDate = head.Created.HasValue ? ((DateTime)e.Created).ToShortDateString() : "",
                    Rows = new List<BudgetRowDTO>(),
                };

                if (head.BudgetRow != null && head.BudgetRow.Count > 0)
                {
                    int rowNumber = 1;
                    foreach (BudgetRow r in head.BudgetRow)
                    {
                        BudgetRowDTO pDto = new BudgetRowDTO()
                        {
                            BudgetRowId = r.BudgetRowId,
                            BudgetHeadId = r.BudgetHeadId,
                            Name = r.TimeCode != null && r.TimeCode.Name.HasValue() ? r.TimeCode.Name : "",
                            AccountId = r.AccountId != null ? (int)r.AccountId : 0,
                            TimeCodeId = r.TimeCodeId != null ? (int)r.TimeCodeId : 0,
                            DistributionCodeHeadId = r.DistributionCodeHeadId,
                            TotalAmount = r.TotalAmount,
                            TotalQuantity = r.TotalQuantity,
                            BudgetRowNr = rowNumber,
                            Type = r.Type,
                            Periods = new List<BudgetPeriodDTO>(),
                            Modified = r.Modified != null ? ((DateTime)r.Modified).ToString() : "",
                            ModifiedBy = r.ModifiedBy.NullToEmpty(),
                        };

                        #region AccountDim

                        //För filtreringens skull så det inte är null
                        /*pDto.Dim1Nr = String.Empty;
                        pDto.Dim2Nr = String.Empty;
                        pDto.Dim3Nr = String.Empty;
                        pDto.Dim4Nr = String.Empty;
                        pDto.Dim5Nr = String.Empty;
                        pDto.Dim6Nr = String.Empty;

                        if (!r.AccountStdReference.IsLoaded)
                            r.AccountStdReference.Load();

                        // Get standard account
                        AccountStd accountStd = r.AccountStd;
                        if (accountStd != null)
                        {
                            if (!accountStd.AccountReference.IsLoaded)
                                accountStd.AccountReference.Load();

                            pDto.Dim1Id = accountStd.AccountId;
                            pDto.Dim1Nr = accountStd.Account.AccountNr;
                            pDto.Dim1Name = accountStd.Account.Name;
                        }

                        if (!r.AccountInternal.IsLoaded)
                            r.AccountInternal.Load();

                        // Internal accounts (dim 2-6)
                        foreach (AccountInternal accountInternal in r.AccountInternal)
                        {
                            if (!accountInternal.AccountReference.IsLoaded)
                                accountInternal.AccountReference.Load();

                            if (!accountInternal.Account.AccountDimReference.IsLoaded)
                                accountInternal.Account.AccountDimReference.Load();

                            switch (accountInternal.Account.AccountDim.AccountDimNr)
                            {
                                case 2:
                                    pDto.Dim2Id = accountInternal.AccountId;
                                    pDto.Dim2Nr = accountInternal.Account.AccountNr;
                                    pDto.Dim2Name = accountInternal.Account.Name;
                                    break;
                                case 3:
                                    pDto.Dim3Id = accountInternal.AccountId;
                                    pDto.Dim3Nr = accountInternal.Account.AccountNr;
                                    pDto.Dim3Name = accountInternal.Account.Name;
                                    break;
                                case 4:
                                    pDto.Dim4Id = accountInternal.AccountId;
                                    pDto.Dim4Nr = accountInternal.Account.AccountNr;
                                    pDto.Dim4Name = accountInternal.Account.Name;
                                    break;
                                case 5:
                                    pDto.Dim5Id = accountInternal.AccountId;
                                    pDto.Dim5Nr = accountInternal.Account.AccountNr;
                                    pDto.Dim5Name = accountInternal.Account.Name;
                                    break;
                                case 6:
                                    pDto.Dim6Id = accountInternal.AccountId;
                                    pDto.Dim6Nr = accountInternal.Account.AccountNr;
                                    pDto.Dim6Name = accountInternal.Account.Name;
                                    break;
                            }
                        }*/

                        #endregion

                        /*if (r.BudgetRowPeriod != null && r.BudgetRowPeriod.Count() > 0)
                        {
                            int periodNr = 1;
                            foreach (BudgetRowPeriod bp in r.BudgetRowPeriod)
                            {
                                BudgetPeriodDTO rp = new BudgetPeriodDTO()
                                {
                                    BudgetRowPeriodId = bp.BudgetRowPeriodId,
                                    BudgetRowId = bp.BudgetRowId,
                                    PeriodNr = periodNr,
                                    Amount = bp.Amount,
                                    Quantity = bp.Quantity,
                                };

                                pDto.Periods.Add(rp);
                                periodNr++;
                            }
                        }*/

                        hDto.Rows.Add(pDto);
                        rowNumber++;
                    }
                }

                tpdto.BudgetHead = hDto;
            }

            if (e.ParentProject != null)
            {
                tpdto.ParentProjectNr = e.ParentProject.Number;
                tpdto.ParentProjectName = e.ParentProject.Name;
            }

            // Set TimeProjectDTO specific properties
            tpdto.PayrollProductAccountingPrio = (e as TimeProject).PayrollProductAccountingPrio;
            tpdto.InvoiceProductAccountingPrio = (e as TimeProject).InvoiceProductAccountingPrio;
            tpdto.OrderTemplateId = (e as TimeProject).OrderTemplateId;
            return tpdto;
        }

        public static TimeProjectDTO ToDTO(this TimeProject e, bool includeAccountSettings, bool hasInvoices, int nbrOfInvoices)
        {
            if (e == null)
                return null;

            TimeProjectDTO dto = (e as Project).ToTimeProjectDTO(includeAccountSettings, false);
            dto.PayrollProductAccountingPrio = e.PayrollProductAccountingPrio;
            dto.InvoiceProductAccountingPrio = e.InvoiceProductAccountingPrio;
            dto.OrderTemplateId = e.OrderTemplateId;
            dto.HasInvoices = hasInvoices;
            dto.NumberOfInvoices = nbrOfInvoices;

            return dto;
        }

        #endregion

        #region ProjectIO

        public static SoftOne.Soe.Common.DTO.ProjectIODTO ToDTO(this ProjectIO e)
        {
            // DTO has 47 properties
            var dto = new ProjectIODTO()
            {
                // Skipping IsModified
                // Skipping IsSelected
                StatusName = e.StatusName,
                AccountNr = e.AccountNr,
                AccountDim2Nr = e.AccountDim2Nr,
                AccountDim3Nr = e.AccountDim3Nr,
                AccountDim4Nr = e.AccountDim4Nr,
                AccountDim5Nr = e.AccountDim5Nr,
                AccountDim6Nr = e.AccountDim6Nr,
                ActorCompanyId = e.ActorCompanyId,
                AllocationType = e.AllocationType,
                BatchId = e.BatchId,
                BookAccordingToThisProject = e.BookAccordingToThisProject,
                CategoryCode1 = e.CategoryCode1,
                CategoryCode2 = e.CategoryCode2,
                CategoryCode3 = e.CategoryCode3,
                CategoryCode4 = e.CategoryCode4,
                CategoryCode5 = e.CategoryCode5,
                CategoryCode6 = e.CategoryCode6,
                CategoryCode7 = e.CategoryCode7,
                CategoryCode8 = e.CategoryCode8,
                CategoryCode9 = e.CategoryCode9,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                CustomerNr = e.CustomerNr,
                Description = e.Description,
                Import = e.Import,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                Name = e.Name,
                Note = e.Note,
                ParentProjectNr = e.ParentProjectNr,
                ParticipantEmployeeNr1 = e.ParticipantEmployeeNr1,
                ParticipantEmployeeNr2 = e.ParticipantEmployeeNr2,
                ParticipantEmployeeNr3 = e.ParticipantEmployeeNr3,
                ParticipantEmployeeNr4 = e.ParticipantEmployeeNr4,
                ParticipantEmployeeNr5 = e.ParticipantEmployeeNr5,
                ParticipantEmployeeNr6 = e.ParticipantEmployeeNr6,
                ProjectIOId = e.ProjectIOId,
                ProjectNr = e.ProjectNr,
                Source = e.Source,
                StartDate = e.StartDate,
                State = e.State,
                Status = e.Status,
                StopDate = e.StopDate,
                Type = e.Type,
                ErrorMessage = e.ErrorMessage,
                // Contains 45 properties
            };
            return dto;
        }

        public static IEnumerable<SoftOne.Soe.Common.DTO.ProjectIODTO> ToDTOs(this IEnumerable<ProjectIO> e)
        {
            return e.Select(s => s.ToDTO()).ToList();
        }

        #endregion

        #endregion

        #region Supplier

        public static SupplierDTO ToDTO(this Supplier e, bool includeContactAddresses)
        {
            if (e == null)
                return null;

            var dto = new SupplierDTO()
            {
                ActorSupplierId = e.ActorSupplierId,
                VatType = e.VatType == 2 ? TermGroup_InvoiceVatType.Merchandise : (TermGroup_InvoiceVatType)e.VatType,
                PaymentConditionId = e.PaymentConditionId,
                FactoringSupplierId = e.FactoringSupplierId,
                CurrencyId = e.CurrencyId,
                SysCountryId = e.SysCountryId,
                SysLanguageId = e.SysLanguageId,
                SupplierNr = e.SupplierNr,
                Name = e.Name,
                OrgNr = e.OrgNr,
                VatNr = e.VatNr,
                InvoiceReference = e.InvoiceReference,
                OurReference = e.OurReference,
                BIC = e.BIC,
                OurCustomerNr = e.OurCustomerNr,
                CopyInvoiceNrToOcr = e.CopyInvoiceNrToOcr,
                Interim = e.Interim,
                ManualAccounting = e.ManualAccounting,
                BlockPayment = e.BlockPayment,
                IsEDISupplier = e.IsEDISupplier,
                RiksbanksCode = e.RiksbanksCode,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                Note = e.Note,
                ShowNote = e.ShowNote,
                SysWholeSellerId = e.SysWholeSellerId,
                VatCodeId = e.VatCodeId,
                AttestWorkFlowGroupId = e.AttestWorkFlowGroupId,
                DeliveryConditionId = e.DeliveryConditionId,
                DeliveryTypeId = e.DeliveryTypeId,
                ContactEcomId = e.ContactEcomId,
                State = (SoeEntityState)e.State,
                Active = e.State == (int)SoeEntityState.Active,
                IsPrivatePerson = (e.IsPrivatePerson.HasValue && (bool)e.IsPrivatePerson),
                IntrastatCodeId = e.IntrastatCodeId,
                IsEUCountryBased = e.IsEUCountryBased,
            };

            if (e.TemplateAttestHead != null)
            {
                dto.TemplateAttestHead = e.TemplateAttestHead.ToDTO(false, false);

                if (!e.TemplateAttestHead.AttestWorkFlowRow.IsLoaded)
                {
                    e.TemplateAttestHead.AttestWorkFlowRow.Load();
                    DataProjectLogCollector.LogLoadedEntityInExtension("e.TemplateAttestHead.AttestWorkFlowRowl");
                }

                if (e.TemplateAttestHead.AttestWorkFlowRow.Count > 0)
                {
                    dto.TemplateAttestHead.Rows = new List<AttestWorkFlowRowDTO>();

                    foreach (AttestWorkFlowRow row in e.TemplateAttestHead.AttestWorkFlowRow)
                    {
                        dto.TemplateAttestHead.Rows.Add(row.ToDTO(false));
                    }
                }
            }

            if (dto.IsPrivatePerson)
            {
                ActorConsent actorConsent = e.Actor?.ActorConsent.FirstOrDefault(a => a.ConsentType == (int)ActorConsentType.Unspecified);
                if (actorConsent != null)
                {
                    dto.HasConsent = actorConsent.HasConsent;
                    dto.ConsentDate = actorConsent.ConsentDate;
                    dto.ConsentModified = actorConsent.ConsentModified;
                    dto.ConsentModifiedBy = actorConsent.ConsentModifiedBy;
                }
            }

            dto.CategoryIds = e.CategoryIds;

            #region ContactAddresses

            if (includeContactAddresses)
                dto.ContactAddresses = GetContactAddressItems(e.Actor.Contact.FirstOrDefault());

            if (includeContactAddresses)
            {
                dto.PaymentInformationDomestic = e.PaymentInformation.ToDTO(true, false);
                dto.PaymentInformationForegin = e.PaymentInformation.ToDTO(true, true);
            }

            #endregion

            #region Accounts

            if (false)
            {
                dto.AccountingSettings = new List<AccountingSettingsRowDTO>();

                if (e.SupplierAccountStd != null && e.SupplierAccountStd.Count > 0)
                {
                    AddAccountingSettingsRowDTO(e, dto, SupplierAccountType.Credit);
                    AddAccountingSettingsRowDTO(e, dto, SupplierAccountType.Debit);
                    AddAccountingSettingsRowDTO(e, dto, SupplierAccountType.VAT);
                    AddAccountingSettingsRowDTO(e, dto, SupplierAccountType.Interim);
                }
            }

            #endregion

            return dto;
        }

        public static IEnumerable<SupplierDTO> ToDTOs(this IEnumerable<Supplier> l, bool includeContactAddresses)
        {
            var dtos = new List<SupplierDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO(includeContactAddresses));
                }
            }
            return dtos;
        }

        private static void AddAccountingSettingsRowDTO(Supplier supplier, SupplierDTO dto, SupplierAccountType type)
        {
            AccountingSettingsRowDTO accDto = new AccountingSettingsRowDTO()
            {
                Type = (int)type,
                Percent = 0
            };
            dto.AccountingSettings.Add(accDto);

            SupplierAccountStd accStd = supplier.SupplierAccountStd.FirstOrDefault(c => c.Type == (int)type);
            Account account = accStd?.AccountStd?.Account;
            if (account != null)
            {
                accDto.AccountDim1Nr = Constants.ACCOUNTDIM_STANDARD;
                accDto.Account1Id = account.AccountId;
                accDto.Account1Nr = account.AccountNr;
                accDto.Account1Name = account.Name;
            }

            if (accStd != null && accStd.AccountInternal != null)
            {
                int dimCounter = 2;
                foreach (var accInt in accStd.AccountInternal.Where(a => a.Account != null && a.Account.AccountDim != null && a.Account.AccountDim.AccountDimNr != Constants.ACCOUNTDIM_STANDARD).OrderBy(a => a.Account.AccountDim.AccountDimNr))
                {
                    account = accInt.Account;

                    // TODO: Does not support dim numbers over 6!!!
                    if (dimCounter == 2)
                    {
                        accDto.AccountDim2Nr = account.AccountDim.AccountDimNr;
                        accDto.Account2Id = account.AccountId;
                        accDto.Account2Nr = account.AccountNr;
                        accDto.Account2Name = account.Name;
                    }
                    else if (dimCounter == 3)
                    {
                        accDto.AccountDim3Nr = account.AccountDim.AccountDimNr;
                        accDto.Account3Id = account.AccountId;
                        accDto.Account3Nr = account.AccountNr;
                        accDto.Account3Name = account.Name;
                    }
                    else if (dimCounter == 4)
                    {
                        accDto.AccountDim4Nr = account.AccountDim.AccountDimNr;
                        accDto.Account4Id = account.AccountId;
                        accDto.Account4Nr = account.AccountNr;
                        accDto.Account4Name = account.Name;
                    }
                    else if (dimCounter == 5)
                    {
                        accDto.AccountDim5Nr = account.AccountDim.AccountDimNr;
                        accDto.Account5Id = account.AccountId;
                        accDto.Account5Nr = account.AccountNr;
                        accDto.Account5Name = account.Name;
                    }
                    else if (dimCounter == 6)
                    {
                        accDto.AccountDim6Nr = account.AccountDim.AccountDimNr;
                        accDto.Account6Id = account.AccountId;
                        accDto.Account6Nr = account.AccountNr;
                        accDto.Account6Name = account.Name;
                    }

                    dimCounter++;
                }
            }
        }

        public static List<ContactAddressItem> GetContactAddressItems(Contact contact)
        {
            List<ContactAddressItem> items = new List<ContactAddressItem>();

            if (contact != null)
            {
                //
                // If DisplayAddress format is changed, also update ContactAddresses.SetDisplayAddress in Silverlight project and ContactAddressItemDTO.setDisplayAddress in Angular project
                //

                // Convert addresses into ContactAddressItems
                foreach (ContactAddress contactAddress in contact.ContactAddress)
                {
                    #region ContactAddress

                    if (contactAddress.SysContactAddressTypeId <= 0)
                        continue;

                    ContactAddressItemType type = (ContactAddressItemType)contactAddress.SysContactAddressTypeId;

                    ContactAddressItem item = new ContactAddressItem()
                    {
                        ContactId = contact.ContactId,
                        ContactAddressItemType = type,
                        IsSecret = contactAddress.IsSecret,
                        IsAddress = true,
                        Name = contactAddress.Name,
                        ContactAddressId = contactAddress.ContactAddressId,
                        SysContactAddressTypeId = contactAddress.SysContactAddressTypeId,
                        AddressName = GetContactAddressRowText(contactAddress, TermGroup_SysContactAddressRowType.Name),
                        Address = GetContactAddressRowText(contactAddress, TermGroup_SysContactAddressRowType.Address),
                        AddressCO = GetContactAddressRowText(contactAddress, TermGroup_SysContactAddressRowType.AddressCO),
                        PostalCode = GetContactAddressRowText(contactAddress, TermGroup_SysContactAddressRowType.PostalCode),
                        PostalAddress = GetContactAddressRowText(contactAddress, TermGroup_SysContactAddressRowType.PostalAddress),
                        StreetAddress = GetContactAddressRowText(contactAddress, TermGroup_SysContactAddressRowType.StreetAddress),
                        EntranceCode = GetContactAddressRowText(contactAddress, TermGroup_SysContactAddressRowType.EntranceCode),
                        Country = GetContactAddressRowText(contactAddress, TermGroup_SysContactAddressRowType.Country)
                    };

                    switch (type)
                    {
                        case ContactAddressItemType.AddressDistribution:
                            item.DisplayAddress = String.Format("{0}, {1} {2}", item.Address, item.PostalCode, item.PostalAddress);
                            item.Icon = "fa fa-mailbox";
                            break;
                        case ContactAddressItemType.AddressVisiting:
                            item.DisplayAddress = String.Format("{0}, {1} {2}", item.StreetAddress, item.PostalCode, item.PostalAddress);
                            item.Icon = "fa fa-home";
                            break;
                        case ContactAddressItemType.AddressBilling:
                            item.DisplayAddress = String.Format("{0}, {1} {2}", item.Address, item.PostalCode, item.PostalAddress);
                            item.Icon = "fa fa-file-text-o";
                            break;
                        case ContactAddressItemType.AddressBoardHQ:
                            item.DisplayAddress = item.PostalAddress;
                            item.Icon = "fa fa-building-o";
                            break;
                        case ContactAddressItemType.AddressDelivery:
                            item.DisplayAddress = String.Format("{0}, {1}, {2} {3}", item.AddressName, item.Address, item.PostalCode, item.PostalAddress);
                            item.Icon = "fa fa-truck";
                            break;
                        default:
                            item.DisplayAddress = String.Format("{0}, {1} {2}", item.Address, item.PostalCode, item.PostalAddress);
                            break;
                    }

                    items.Add(item);

                    #endregion
                }

                // Convert ContactECom's into ContactAddressItem's
                foreach (ContactECom contactECom in contact.ContactECom)
                {
                    #region ContactECom

                    if (contactECom.SysContactEComTypeId <= 0)
                        continue;

                    ContactAddressItem item = new ContactAddressItem()
                    {
                        ContactId = contact.ContactId,
                        IsSecret = contactECom.IsSecret,
                        ContactAddressItemType = (ContactAddressItemType)(contactECom.SysContactEComTypeId + 10),
                        IsAddress = false,
                        Name = contactECom.Name,
                        ContactEComId = contactECom.ContactEComId,
                        SysContactEComTypeId = contactECom.SysContactEComTypeId,
                        SysContactAddressTypeId = contactECom.SysContactEComTypeId,
                        EComText = contactECom.Text,
                        EComDescription = contactECom.Description,
                        DisplayAddress = contactECom.Text
                    };

                    // Set icon
                    switch ((ContactAddressItemType)(contactECom.SysContactEComTypeId + 10))
                    {
                        case ContactAddressItemType.EComEmail:
                            item.Icon = "fa fa-envelope";
                            break;
                        case ContactAddressItemType.EComPhoneHome:
                            item.Icon = "fa fa-phone";
                            break;
                        case ContactAddressItemType.EComPhoneJob:
                            item.Icon = "fa fa-phone";
                            break;
                        case ContactAddressItemType.EComPhoneMobile:
                            item.Icon = "fa fa-mobile";
                            break;
                        case ContactAddressItemType.EComFax:
                            item.Icon = "fa fa-fax";
                            break;
                        case ContactAddressItemType.EComWeb:
                            item.Icon = "fa fa-globe";
                            break;
                        case ContactAddressItemType.ClosestRelative:
                            item.Icon = "fa fa-user";
                            break;
                        case ContactAddressItemType.EcomCompanyAdminEmail:
                            item.Icon = "fa fa-envelope";
                            break;
                        case ContactAddressItemType.Coordinates:
                            item.Icon = "fa fa-map-marker";
                            break;
                        case ContactAddressItemType.IndividualTaxNumber:
                            item.Icon = "fa fa-money";
                            break;
                        case ContactAddressItemType.GlnNumber:
                            item.Icon = "fa fa-paper-plane";
                            break;
                    }

                    items.Add(item);

                    #endregion
                }
            }

            return items.OrderBy(i => i.ContactAddressItemType).ThenBy(i => i.Address).ToList();
        }

        private static string GetContactAddressRowText(ContactAddress address, TermGroup_SysContactAddressRowType rowType)
        {
            return address.ContactAddressRow.FirstOrDefault(r => r.SysContactAddressRowTypeId == (int)rowType)?.Text ?? string.Empty;
        }

        public static SupplierGridDTO ToGridDTO(this Supplier e)
        {
            if (e == null)
                return null;

            var dto = new SupplierGridDTO()
            {
                ActorSupplierId = e.ActorSupplierId,
                SupplierNr = e.SupplierNr,
                Name = e.Name,
                //ContactAddresses = GetContactAddressItems(e.Actor.Contact.FirstOrDefault()),
                State = (SoeEntityState)e.State,
                IsPrivatePerson = e.IsPrivatePerson
            };

            return dto;
        }

        public static IEnumerable<SupplierGridDTO> ToGridDTOs(this IEnumerable<Supplier> l)
        {
            var dtos = new List<SupplierGridDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToGridDTO());
                }
            }
            return dtos;
        }

        public static SupplierInvoiceSearchDTO ToDTO(this SupplierInvoiceSearchResult e)
        {
            if (e == null)
                return null;

            SupplierInvoiceSearchDTO dto = new SupplierInvoiceSearchDTO()
            {
                InvoiceId = e.InvoiceId,
                InvoiceNr = e.InvoiceNr,
                SeqNr = e.SeqNr,
                InvoiceDate = e.InvoiceDate,
                SupplierNr = e.SupplierNr,
                SupplierName = e.SupplierName
            };

            return dto;
        }

        public static IEnumerable<SupplierInvoiceSearchDTO> ToDTOs(this IEnumerable<SupplierInvoiceSearchResult> l)
        {
            var dtos = new List<SupplierInvoiceSearchDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static SupplierIODTO ToDTO(this SupplierIO e)
        {
            if (e == null)
                return null;

            SupplierIODTO dto = new SupplierIODTO()
            {
                SupplierIOId = e.SupplierIOId,
                ActorCompanyId = e.ActorCompanyId,
                Import = e.Import,
                Type = (TermGroup_IOType)e.Type,
                Status = (TermGroup_IOStatus)e.Status,
                Source = (TermGroup_IOSource)e.Source,
                ImportHeadType = (TermGroup_IOImportHeadType)e.ImportHeadType,
                BatchId = e.BatchId,
                ErrorMessage = e.ErrorMessage,
                SupplierId = e.SupplierId,
                SupplierNr = e.SupplierNr,
                Name = e.Name,
                OrgNr = e.OrgNr,
                VatNr = e.VatNr,
                VatType = e.VatType,
                RiksbanksCode = e.RiksbanksCode,
                OurCustomerNr = e.OurCustomerNr,
                FactoringSupplierNr = e.FactoringSupplierNr,
                SysCountry = e.SysCountry,
                Currency = e.Currency,
                StandardPaymentType = e.StandardPaymentType,
                BankGiroNr = e.BankGiroNr,
                PlusGiroNr = e.PlusGiroNr,
                BankNr = e.BankNr,
                BIC = e.BIC,
                IBAN = e.IBAN,
                DistributionAddress = e.DistributionAddress,
                DistributionCoAddress = e.DistributionCoAddress,
                DistributionPostalCode = e.DistributionPostalCode,
                DistributionPostalAddress = e.DistributionPostalAddress,
                DistributionCountry = e.DistributionCountry,
                BillingAddress = e.BillingAddress,
                BillingCoAddress = e.BillingCoAddress,
                BillingPostalCode = e.BillingPostalCode,
                BillingPostalAddress = e.BillingPostalAddress,
                BillingCountry = e.BillingCountry,
                BoardHQAddress = e.BoardHQAddress,
                BoardHQCountry = e.BoardHQCountry,
                VisitingAddress = e.VisitingAddress,
                VisitingCoAddress = e.VisitingCoAddress,
                VisitingPostalCode = e.VisitingPostalCode,
                VisitingPostalAddress = e.VisitingPostalAddress,
                VisitingCountry = e.VisitingCountry,
                DeliveryAddress = e.DeliveryAddress,
                DeliveryCoAddress = e.DeliveryCoAddress,
                DeliveryPostalCode = e.DeliveryPostalCode,
                DeliveryPostalAddress = e.DeliveryPostalAddress,
                DeliveryCountry = e.DeliveryCountry,
                Email1 = e.Email1,
                Email2 = e.Email2,
                PhoneHome = e.PhoneHome,
                PhoneMobile = e.PhoneMobile,
                PhoneJob = e.PhoneJob,
                Fax = e.Fax,
                Webpage = e.Webpage,
                PaymentConditionCode = e.PaymentConditionCode,
                CopyInvoiceNrToOcr = e.CopyInvoiceNrToOcr,
                BlockPayment = e.BlockPayment,
                ManualAccounting = e.ManualAccounting,
                AccountsPayableAccountNr = e.AccountsPayableAccountNr,
                AccountsPayableAccountInternal1 = e.AccountsPayableAccountInternal1,
                AccountsPayableAccountInternal2 = e.AccountsPayableAccountInternal2,
                AccountsPayableAccountInternal3 = e.AccountsPayableAccountInternal3,
                AccountsPayableAccountInternal4 = e.AccountsPayableAccountInternal4,
                AccountsPayableAccountInternal5 = e.AccountsPayableAccountInternal5,
                PurchaseAccountNr = e.PurchaseAccountNr,
                PurchaseAccountInternal1 = e.PurchaseAccountInternal1,
                PurchaseAccountInternal2 = e.PurchaseAccountInternal2,
                PurchaseAccountInternal3 = e.PurchaseAccountInternal3,
                PurchaseAccountInternal4 = e.PurchaseAccountInternal4,
                PurchaseAccountInternal5 = e.PurchaseAccountInternal5,
                VATAccountNr = e.VATAccountNr,
                VATCodeNr = e.VATCodeNr,
                State = (SoeEntityState)e.State,
            };

            // Extensions
            dto.StatusName = e.StatusName;
            dto.VatTypeName = e.VatTypeName;
            dto.StandardPaymentTypeName = e.StandardPaymentTypeName;

            return dto;
        }

        public static IEnumerable<SupplierIODTO> ToDTOs(this IEnumerable<SupplierIO> l)
        {
            var dtos = new List<SupplierIODTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static SupplierInvoiceHeadIODTO ToDTO(this SupplierInvoiceHeadIO e)
        {
            if (e == null)
                return null;

            SupplierInvoiceHeadIODTO dto = new SupplierInvoiceHeadIODTO()
            {
                SupplierInvoiceHeadIOId = e.SupplierInvoiceHeadIOId,
                ActorCompanyId = e.ActorCompanyId,
                Import = e.Import,
                Type = (TermGroup_IOType)e.Type,
                Status = (TermGroup_IOStatus)e.Status,
                Source = (TermGroup_IOSource)e.Source,
                ImportHeadType = (TermGroup_IOImportHeadType)e.ImportHeadType,
                BatchId = e.BatchId,
                ErrorMessage = e.ErrorMessage,
                SupplierId = e.SupplierId,
                SupplierNr = e.SupplierNr,
                InvoiceId = e.InvoiceId,
                SeqNr = e.SeqNr,
                BillingType = e.BillingType ?? (int)TermGroup_BillingType.None,
                InvoiceDate = e.InvoiceDate,
                DueDate = e.DueDate,
                VoucherDate = e.VoucherDate,
                ReferenceOur = e.ReferenceOur,
                ReferenceYour = e.ReferenceYour,
                OCR = e.OCR,
                CurrencyId = e.CurrencyId,
                Currency = e.Currency,
                CurrencyRate = e.CurrencyRate,
                CurrencyDate = e.CurrencyDate,
                TotalAmount = e.TotalAmount,
                TotalAmountCurrency = e.TotalAmountCurrency,
                VATAmount = e.VATAmount,
                VATAmountCurrency = e.VATAmountCurrency,
                PaidAmount = e.PaidAmount,
                PaidAmountCurrency = e.PaidAmountCurrency,
                RemainingAmount = e.RemainingAmount,
                FullyPayed = e.FullyPayed,
                PaymentNr = e.PaymentNr,
                VoucherNr = e.VoucherNr,
                CreateAccountingInXE = e.CreateAccountingInXE,
                Note = e.Note,
                State = (SoeEntityState)e.State,
                OriginStatus = e.OriginStatus.HasValue ? (SoeOriginStatus)e.OriginStatus.Value : SoeOriginStatus.Origin,
                SupplierInvoiceNr = e.SupplierInvoiceNr,
            };

            // Extensions
            dto.StatusName = e.StatusName;
            dto.BillingTypeName = e.BillingTypeName;

            return dto;
        }

        public static IEnumerable<SupplierInvoiceHeadIODTO> ToDTOs(this IEnumerable<SupplierInvoiceHeadIO> l)
        {
            var dtos = new List<SupplierInvoiceHeadIODTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static List<SupplierDistributionDTO> ToDistributionDTOs(this IEnumerable<Supplier> l)
        {
            var dtos = new List<SupplierDistributionDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDistributionDTO());
                }
            }
            return dtos;
        }

        public static SupplierDistributionDTO ToDistributionDTO(this Supplier e)
        {
            var supplier = new SupplierDistributionDTO
            {
                SupplierId = e.ActorSupplierId,
                Number = e.SupplierNr,
                Name = e.Name,
                OrgNr = e.OrgNr,
                PaymentInformationRows = e.PaymentInformation != null ?
                                            e.PaymentInformation.ToDistributionDTOs() :
                                            e.Actor?.PaymentInformation.ToDistributionDTOs()
            };
            return supplier;
        }

        #endregion

        #region SupplierProductPriceList

        public static SupplierProductPriceListGridDTO ToGridDTO(this SupplierProductPricelistDTO e)
        {
            if (e == null)
                return null;

            var dto = new SupplierProductPriceListGridDTO()
            {
                SupplierProductPriceListId = e.SupplierProductPriceListId,
                SupplierNr = e.SupplierNr,
                SupplierName = e.SupplierName,
                SysWholeSellerName = e.SysWholeSellerName,
                SysWholeSellerTypeName = e.SysWholeSellerTypeName,
                CurrencyCode = e.CurrencyCode,
                StartDate = e.StartDate,
                EndDate = e.EndDate,
                Created = e.Created
            };

            return dto;
        }

        public static IEnumerable<SupplierProductPriceListGridDTO> ToGridDTOs(this IEnumerable<SupplierProductPricelistDTO> l)
        {
            var dtos = new List<SupplierProductPriceListGridDTO>();

            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToGridDTO());
                }
            }

            return dtos;
        }

        #endregion

        #region SupplierInvoiceCostTransfer

        public static SupplierInvoiceProjectRowDTO ToSupplierInvoiceProjectRowDTO(this SupplierInvoiceCostTransferDTO dto)
        {
            if (dto == null)
            {
                return new SupplierInvoiceProjectRowDTO();
            }
            return new SupplierInvoiceProjectRowDTO()
            {
                TimeCodeTransactionId = dto.RecordId,
                State = dto.State,
                SupplierInvoiceId = dto.SupplierInvoiceId,
                ProjectId = dto.ProjectId,
                CustomerInvoiceId = dto.CustomerInvoiceId,
                TimeCodeId = dto.TimeCodeId,
                EmployeeId = dto.EmployeeId,
                Amount = dto.AmountCurrency,
                AmountCurrency = dto.AmountCurrency,
                AmountLedgerCurrency = dto.AmountLedgerCurrency,
                AmountEntCurrency = dto.AmountEntCurrency,
                IncludeSupplierInvoiceImage = dto.IncludeSupplierInvoiceImage,
                ChargeCostToProject = dto.ChargeCostToProject
            };
        }

        public static SupplierInvoiceOrderRowDTO ToSupplierInvoiceOrderRowDTO(this SupplierInvoiceCostTransferDTO dto)
        {
            if (dto == null)
            {
                return new SupplierInvoiceOrderRowDTO();
            }
            return new SupplierInvoiceOrderRowDTO()
            {
                IsModified = dto.RecordId != 0,
                State = dto.State,
                CustomerInvoiceRowId = dto.RecordId,
                SupplierInvoiceId = dto.SupplierInvoiceId,
                InvoiceProductId = dto.InvoiceProductId,
                AttestStateId = dto.AttestStateId,
                IncludeSupplierInvoiceImage = dto.IncludeSupplierInvoiceImage,
                ProjectId = dto.ProjectId,
                CustomerInvoiceId = dto.CustomerInvoiceId,
                SupplementCharge = dto.SupplementCharge,
                Amount = dto.Amount,
                AmountCurrency = dto.AmountCurrency,
                AmountLedgerCurrency = dto.AmountLedgerCurrency,
                AmountEntCurrency = dto.AmountEntCurrency,
                SumAmount = dto.SumAmount,
                SumAmountCurrency = dto.SumAmountCurrency,
                SumAmountLedgerCurrency = dto.SumAmountLedgerCurrency,
                SumAmountEntCurrency = dto.SumAmountEntCurrency
            };
        }

        #endregion

        #region SupplierAgreement

        public static SupplierAgreementDTO ToDTO(this SupplierAgreement e)
        {
            if (e == null)
                return null;

            var dto = new SupplierAgreementDTO
            {
                RebateListId = e.RebateListId,
                SysWholesellerId = e.SysWholesellerId,
                WholesellerName = e.WholesellerName,
                PriceListTypeId = e.PriceListTypeId,
                PriceListTypeName = e.PriceListTypeName,
                DiscountPercent = e.DiscountPercent,
                Code = e.Code,
                CodeType = e.CodeType,
                Date = e.Date,
                CategoryId = e.CategoryId
            };

            return dto;
        }

        public static IEnumerable<SupplierAgreementDTO> ToDTOs(this IEnumerable<SupplierAgreement> l)
        {
            var dtos = new List<SupplierAgreementDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        #endregion

        #region Textblock

        public static TextblockDTO ToDTO(this Textblock e)
        {
            if (e == null)
                return null;

            TextblockDTO dto = new TextblockDTO()
            {
                TextblockId = e.TextblockId,
                ActorCompanyId = e.ActorCompanyId,
                Headline = e.Headline,
                Text = e.Text,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State,
                Type = e.Type,
                ShowInContract = e.ShowInContract,
                ShowInOffer = e.ShowInOffer,
                ShowInOrder = e.ShowInOrder,
                ShowInInvoice = e.ShowInInvoice,
                ShowInPurchase = e.ShowInPurchase,
            };

            return dto;
        }

        public static IEnumerable<TextblockDTO> ToDTOs(this IEnumerable<Textblock> l)
        {
            var dtos = new List<TextblockDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static TextblockDTOBase ToBaseDTO(this Textblock t)
        {
            return new TextblockDTOBase()
            {
                TextblockId = t.TextblockId,
                Text = t.Text,
            };
        }

        public static IEnumerable<TextblockDTOBase> ToBaseDTOs(this IEnumerable<Textblock> l)
        {
            if (l != null)
                return l.Select(s => s.ToBaseDTO()).ToList();
            return null;
        }

        #endregion

        #region SignatoryContract
        public static SignatoryContractAuthenticationMethodType GetRequiredAuthenticationMethodType(this SignatoryContract e)
        {
            return (SignatoryContractAuthenticationMethodType)e.RequiredAuthenticationMethodType;
        }
        public static SignatoryContractAuthenticationMethodType GetAuthenticationMethodType(this SignatoryContractAuthenticationRequest e)
        {
            return (SignatoryContractAuthenticationMethodType)e.AuthenticationMethodType;
        }

        public static bool IsValid(this SignatoryContractAuthenticationRequest e)
        {
            return e != null && e.ExpiresAtUTC > DateTime.UtcNow;
        }

        public static bool IsAuthenticated(this SignatoryContractAuthenticationRequest e)
        {
            return e.IsValid() && e != null && e.AuthenticatedAtUTC.HasValue;
        }

        public static bool IsValid(this SignatoryContract e)
        {
            return e != null && e.RevokedAtUTC == null;
        }

        public static bool IsValid(this SignatoryContractPermission e)
        {
            return e != null;
        }

        #endregion

        #region VatCode

        public static VatCodeDTO ToDTO(this VatCode e)
        {
            if (e == null)
                return null;

            var dto = new VatCodeDTO()
            {
                VatCodeId = e.VatCodeId,
                AccountId = e.AccountId,
                PurchaseVATAccountId = e.PurchaseVATAccountId,
                Code = e.Code,
                Name = e.Name,
                Percent = e.Percent,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State
            };

            // Extensions
            dto.AccountNr = e.AccountStd != null && e.AccountStd.Account != null ? e.AccountStd.Account.AccountNr : String.Empty;
            dto.PurchaseVATAccountNr = e.PurchaseVATAccount != null && e.PurchaseVATAccount.AccountStd != null && e.PurchaseVATAccount.AccountStd.Account != null ? e.PurchaseVATAccount.AccountStd.Account.AccountNr : String.Empty;

            return dto;
        }

        public static IEnumerable<VatCodeDTO> ToDTOs(this IEnumerable<VatCode> l)
        {
            var dtos = new List<VatCodeDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static VatCodeGridDTO ToGridDTO(this VatCode e)
        {
            if (e == null)
                return null;

            VatCodeGridDTO dto = new VatCodeGridDTO()
            {
                VatCodeId = e.VatCodeId,
                Code = e.Code,
                Name = e.Name,
                Percent = e.Percent,
            };

            // Extensions
            dto.Account = e.AccountStd != null && e.AccountStd.Account != null ? e.AccountStd.Account.AccountNrPlusName : String.Empty;
            dto.PurchaseVATAccount = e.PurchaseVATAccountStd != null && e.PurchaseVATAccountStd.Account != null ? e.PurchaseVATAccountStd.Account.AccountNrPlusName : String.Empty;

            return dto;
        }

        public static IEnumerable<VatCodeGridDTO> ToGridDTOs(this IEnumerable<VatCode> l)
        {
            var dtos = new List<VatCodeGridDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToGridDTO());
                }
            }
            return dtos;
        }

        #endregion

        #region VoucherSeries

        public static VoucherSeriesDTO ToDTO(this VoucherSeries e)
        {
            if (e == null)
                return null;

            #region Try load

            try
            {
                if (!e.IsAdded())
                {
                    if (!e.VoucherSeriesTypeReference.IsLoaded)
                    {
                        e.VoucherSeriesTypeReference.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("e.VoucherSeriesTypeReference");
                    }
                }
            }
            catch (InvalidOperationException ex) { ex.ToString(); }

            #endregion

            var dto = new VoucherSeriesDTO
            {
                VoucherSeriesId = e.VoucherSeriesId,
                VoucherSeriesTypeId = e.VoucherSeriesTypeId,
                AccountYearId = e.AccountYearId,
                VoucherNrLatest = e.VoucherNrLatest,
                VoucherDateLatest = e.VoucherDateLatest,
                Status = e.Status == 0 ? null : (TermGroup_AccountStatus?)e.Status,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy
            };

            if (e.VoucherSeriesType != null)
            {
                // Fix for when startnr is updated after series is added
                if (!dto.VoucherDateLatest.HasValue && e.VoucherSeriesType.StartNr - 1 > dto.VoucherNrLatest)
                    dto.VoucherNrLatest = e.VoucherSeriesType.StartNr - 1;

                dto.VoucherSeriesTypeName = e.VoucherSeriesType.Name;
                dto.VoucherSeriesTypeIsTemplate = e.VoucherSeriesType.Template;
                dto.VoucherSeriesTypeNr = e.VoucherSeriesType.VoucherSeriesTypeNr;
            }

            return dto;
        }

        public static IEnumerable<VoucherSeriesDTO> ToDTOs(this IEnumerable<VoucherSeries> l)
        {
            var dtos = new List<VoucherSeriesDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static VoucherSeriesTypeDTO ToDTO(this VoucherSeriesType e)
        {
            if (e == null)
                return null;

            VoucherSeriesTypeDTO dto = new VoucherSeriesTypeDTO
            {
                VoucherSeriesTypeId = e.VoucherSeriesTypeId,
                ActorCompanyId = e.ActorCompanyId,
                Name = e.Name,
                VoucherSeriesTypeNr = e.VoucherSeriesTypeNr,
                StartNr = e.StartNr,
                Template = e.Template,
                YearEndSerie = e.YearEndSerie,
                ExternalSerie = e.ExternalSerie,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State
            };

            return dto;
        }

        public static IEnumerable<VoucherSeriesTypeDTO> ToDTOs(this IEnumerable<VoucherSeriesType> l)
        {
            var dtos = new List<VoucherSeriesTypeDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        #endregion

        #region Views

        #region Account

        public static AccountGridViewDTO ToDTO(this AccountGridView e)
        {
            if (e == null)
                return null;

            AccountGridViewDTO dto = new AccountGridViewDTO()
            {
                AccountId = e.AccountId,
                ActorCompanyId = e.ActorCompanyId,
                LangId = 0, //e.LangId,
                AccountDimId = e.AccountDimId,
                AccountTypeSysTermId = e.AccountTypeSysTermId,
                AccountNr = e.AccountNr,
                Name = e.Name,
                Type = "", //e.Type,
                State = (SoeEntityState)e.State,
                SysVatAccountId = e.SysVatAccountId,
            };

            return dto;
        }

        public static IEnumerable<AccountGridViewDTO> ToDTOs(this IEnumerable<AccountGridView> l)
        {
            var dtos = new List<AccountGridViewDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static AccountVatRateViewDTO ToDTO(this AccountVatRateView e)
        {
            if (e == null)
                return null;

            var dto = new AccountVatRateViewDTO
            {
                ActorCompanyId = e.ActorCompanyId,
                AccountDimId = e.AccountDimId,
                AccountId = e.AccountId,
                AccountNr = e.AccountNr,
                Name = e.Name,
                SysVatAccountId = e.SysVatAccountId
            };

            return dto;
        }

        public static IEnumerable<AccountVatRateViewDTO> ToDTOs(this IEnumerable<AccountVatRateView> l)
        {
            var dtos = new List<AccountVatRateViewDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        #endregion

        #region AccountDistribution

        public static AccountDistributionTraceViewDTO ToDTO(this AccountDistributionTraceView e)
        {
            if (e == null)
                return null;

            var dto = new AccountDistributionTraceViewDTO
            {
                AccountDistributionHeadId = e.AccountDistributionHeadId,
                IsVoucher = e.IsVoucher,
                VoucherHeadId = e.VoucherHeadId,
                IsInvoice = e.IsInvoice,
                InvoiceId = e.InvoiceId,
                Number = e.Number,
                Date = e.Date,
                Description = e.Description,
                OriginType = (SoeOriginType)e.OriginType,
                OriginStatus = (SoeOriginStatus)e.OriginStatus,
                State = (SoeEntityState)e.State,
            };

            return dto;
        }

        public static IEnumerable<AccountDistributionTraceViewDTO> ToDTOs(this IEnumerable<AccountDistributionTraceView> l)
        {
            var dtos = new List<AccountDistributionTraceViewDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        #endregion

        #region Attest

        #region AttestUserRoleView

        public static AttestUserRoleDTO ToDTO(this AttestUserRoleView e)
        {
            if (e == null)
                return null;

            AttestUserRoleDTO dto = new AttestUserRoleDTO()
            {
                AttestStateFromId = e.AttestStateFromId,
                AttestStateToId = e.AttestStateToId,
                AttestTransitionId = e.AttestTransitionId,
                UserId = e.UserId,
                AttestRoleId = e.AttestRoleId,
                ActorCompanyId = e.ActorCompanyId,
                Module = (SoeModule)e.Module,
                ShowUncategorized = e.ShowUncategorized,
                ShowAllCategories = e.ShowAllCategories,
                DateFrom = e.DateFrom,
                DateTo = e.DateTo
            };

            return dto;
        }

        public static IEnumerable<AttestUserRoleDTO> ToDTOs(this IEnumerable<AttestUserRoleView> l)
        {
            var dtos = new List<AttestUserRoleDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static bool ShowUncategorized(this List<AttestUserRoleView> l)
        {
            return l.Any(i => i.ShowAllCategories);
        }

        public static bool ShowAll(this List<AttestUserRoleView> l)
        {
            return l.Any(i => i.ShowAllCategories);
        }

        public static bool ShowTemplateSchedule(this List<AttestUserRoleView> l)
        {
            return l.Any(i => i.ShowTemplateSchedule);
        }

        public static bool IsDateValid(this AttestUserRoleView e, DateTime? dateFrom, DateTime? dateTo)
        {
            return CalendarUtility.IsDatesOverlappingNullable(dateFrom, dateTo, e.DateFrom, e.DateTo);
        }

        public static bool IsDateValid(this AttestUserRoleView e, DateTime date)
        {
            return e.DateFrom <= date && e.DateTo >= date;
        }

        #endregion

        #region TimeEmployeeTreeDTO

        public static List<int> GetEmployeeIds(this TimeEmployeeTreeDTO e)
        {
            List<int> employeeIds = new List<int>();
            if (!e.GroupNodes.IsNullOrEmpty())
            {
                foreach (TimeEmployeeTreeGroupNodeDTO groupNode in e.GroupNodes)
                {
                    employeeIds.AddRange(groupNode.GetEmployeeIdsDeep());
                }
            }
            return employeeIds.Distinct().ToList();
        }

        public static bool ContainsEmployeeNode(this TimeEmployeeTreeDTO e, int employeeId, int groupId)
        {
            TimeEmployeeTreeGroupNodeDTO groupNode = e.GroupNodes?.FirstOrDefault(i => i.Id == groupId);
            return groupNode?.GetEmployeeIdsDeep().Any(id => id == employeeId) ?? false;
        }

        #endregion

        #endregion

        #region CustomerInvoiceRowAttestStateView

        public static CustomerInvoiceRowAttestStateViewDTO ToDTO(this CustomerInvoiceRowAttestStateView e)
        {
            if (e == null)
                return null;

            var dto = new CustomerInvoiceRowAttestStateViewDTO
            {
                InvoiceId = e.InvoiceId,
                ActorCompanyId = e.ActorCompanyId,
                AttestStateId = e.AttestStateId,
                Name = e.Name,
                Sort = e.Sort,
                Color = e.Color
            };

            return dto;
        }

        public static IEnumerable<CustomerInvoiceRowAttestStateViewDTO> ToDTOs(this IEnumerable<CustomerInvoiceRowAttestStateView> l)
        {
            var dtos = new List<CustomerInvoiceRowAttestStateViewDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        #endregion

        #region CustomerInvoiceGridDTO

        public static CustomerInvoiceGridDTO ToGridDTO(this GetCustomerOrders_Result e, bool hideVatWarnings, bool setClosedStyle, bool foreign = false, List<decimal> vatRatesToValidate = null)
        {
            if (e == null)
                return null;

            var dto = new CustomerInvoiceGridDTO
            {
                CustomerInvoiceId = e.InvoiceId,
                OriginType = e.OriginType,
                SeqNr = e.InvoiceSeqNr ?? 0,
                InvoiceNr = e.InvoiceNr,
                DeliveryType = e.DeliveryType != null ? (int)e.DeliveryType : 0,
                BillingTypeId = e.BillingTypeId,
                InvoicePaymentServiceId = e.PaymentService.GetValueOrDefault(),
                //BillingTypeName = e.BillingTypeName,
                BillingAddress = e.BillingAddress,
                Status = e.Status,
                //StatusName = e.StatusName,
                ExportStatus = e.ExportStatus,
                ActorCustomerId = e.ActorId,
                ActorCustomerNr = e.ActorNr,
                ActorCustomerName = e.CustomerNameFromInvoice.IsNullOrEmpty() ? e.ActorName : e.CustomerNameFromInvoice,
                ActorCustomerNrName = e.ActorNr + " " + (e.CustomerNameFromInvoice.IsNullOrEmpty() ? e.ActorName : e.CustomerNameFromInvoice),
                InternalText = e.InternalText,
                WorkDescription = StringUtility.CleanStringForJson(e.Workingdescription), //because DB uses Utf16 but json serialization does not
                //InvoicePaymentServiceName = e.InvoicePaymentService,
                TotalAmount = e.InvoiceTotalAmount, //foreign ? 0 : e.InvoiceTotalAmount,
                TotalAmountText = e.InvoiceTotalAmount.ToString(), //foreign ? String.Empty : e.InvoiceTotalAmount.ToString(),
                TotalAmountCurrency = e.InvoiceTotalAmountCurrency, //foreign ? e.InvoiceTotalAmountCurrency : 0,
                TotalAmountCurrencyText = e.InvoiceTotalAmountCurrency.ToString(), //foreign ? e.InvoiceTotalAmountCurrency.ToString() : String.Empty,
                VATAmount = foreign ? 0 : e.VATAmount,
                PayAmount = e.InvoicePayAmount,//foreign ? 0 : e.InvoicePayAmount,
                PayAmountText = e.InvoicePayAmount.ToString(),//foreign ? String.Empty : e.InvoicePayAmount.ToString(),
                PayAmountCurrency = e.InvoicePayAmountCurrency,//foreign ? e.InvoicePayAmountCurrency : 0,
                PayAmountCurrencyText = e.InvoicePayAmountCurrency.ToString(),//foreign ? e.InvoicePayAmountCurrency.ToString() : String.Empty,
                PaidAmount = e.InvoicePaidAmount, //foreign ? 0 : e.InvoicePaidAmount,
                PaidAmountText = e.InvoicePaidAmount.ToString(), //foreign ? String.Empty : e.InvoicePaidAmount.ToString(),
                PaidAmountCurrency = e.InvoicePaidAmountCurrency, //foreign ? e.InvoicePaidAmountCurrency : 0,
                PaidAmountCurrencyText = e.InvoicePaidAmountCurrency.ToString(), //foreign ? e.InvoicePaidAmountCurrency.ToString() : String.Empty,
                RemainingAmount = e.RemainingAmount,
                RemainingAmountText = e.RemainingAmount.ToString(),
                RemainingAmountExVat = e.RemainingAmountExVat,
                RemainingAmountExVatText = e.RemainingAmount.ToString(),
                SysCurrencyId = e.SysCurrencyId,
                //CurrencyCode = e.CurrencyCode,
                InvoiceDate = e.InvoiceDate,
                OwnerActorId = e.OwnerActorId,

                InvoiceHeadText = e.InvoiceHeadText,
                RegistrationType = e.RegistrationType,
                DeliveryAddressId = e.DeliveryAddressId,
                BillingAddressId = e.BillingAddressId,
                BillingInvoicePrinted = e.BillingInvoicePrinted,
                HasHouseholdTaxDeduction = e.HasHouseholdTaxDeduction,
                HouseholdTaxDeductionType = e.HouseholdTaxDeductionType ?? 0,
                Categories = e.Categories,
                CustomerCategories = e.CustomerCategories,
                DeliverDateText = e.DeliveryDateText,
                DeliveryDate = e.DeliveryDate,
                OrderNumbers = e.OrderNumbers,
                ProjectNr = e.ProjectNumber,
                ProjectName = e.ProjectName,
                StatusIcon = e.StatusIcon,
                ShiftTypeName = e.ShiftTypeName,
                ShiftTypeColor = e.ShiftTypeColor != null && e.ShiftTypeColor.Length == 9 ? "#" + e.ShiftTypeColor.Substring(3) : e.ShiftTypeColor,
                FixedPriceOrder = e.FixedPriceOrder,
                OrderType = e.OrderType,
                DefaultDim2AccountId = e.DefaultDim2AccountId,
                DefaultDim3AccountId = e.DefaultDim3AccountId,
                DefaultDim4AccountId = e.DefaultDim4AccountId,
                DefaultDim5AccountId = e.DefaultDim5AccountId,
                DefaultDim6AccountId = e.DefaultDim6AccountId,
                Users = string.IsNullOrEmpty(e.OriginUsers) ? "" : e.OriginUsers.Trim().TrimEnd(','),
                DeliveryAddress = e.DeliveryAddress,
                CurrencyRate = e.CurrencyRate,
                ContactEComId = e.ContactEComId,
                ContactEComText = e.Email,
                MyReadyState = e.MyReadyState,
                ReferenceOur = e.ReferenceOur,
                ReferenceYour = e.ReferenceYour,
                PriceListName = e.PriceListName,
                MainUserName = e.MainUserName,
                InvoiceLabel = e.InvoiceLabel,
                MyOriginUserStatus = e.MyOriginUserStatus,
                MappedContractNr = e.MappedContractNr
            };

            if (e.OriginReadyUserCount == e.OriginUserCount)
            {
                dto.orderReadyStatePercent = 100;
            }
            else if (e.OriginReadyUserCount > 0 && e.OriginUserCount > 0)
            {
                dto.orderReadyStatePercent = Convert.ToInt32(decimal.Divide(e.OriginReadyUserCount, e.OriginUserCount) * 100);
            }

            //if (dto.orderReadyStatePercent > 0 && dto.orderReadyStatePercent < 100)
            //{
            dto.orderReadyStateText = $"{e.OriginReadyUserCount}/{e.OriginUserCount}";
            //}

            dto.TotalAmountExVat = (dto.TotalAmount - e.CentRounding) - dto.VATAmount;
            dto.TotalAmountExVatText = dto.TotalAmountExVat.ToString();
            dto.TotalAmountExVatCurrency = (dto.TotalAmountCurrency - e.CentRounding) - dto.VATAmountCurrency;
            dto.TotalAmountExVatCurrencyText = dto.TotalAmountExVatCurrency.ToString();

            #region Vat warning
            var hasError = false;

            if (hideVatWarnings)
                dto.VatRate = 0;
            else
            {
                // TODO: We need cent rounding for the calculation to be correct, but it is not included in the view.
                // This will cause small invoices to be misleading in this validation.
                decimal cent = 0;
                decimal amountCurrency = e.InvoiceTotalAmountCurrency; //dto.TotalAmountCurrency;
                decimal sum = amountCurrency - dto.VATAmount - cent;
                if (sum != 0)
                    dto.VatRate = Decimal.Round((dto.VATAmount / sum * 100), 1);

                if (vatRatesToValidate != null && vatRatesToValidate.Count > 0)
                {
                    // A workaround is to check the size of the amount.
                    if (dto.VatRate != 0 && !vatRatesToValidate.Contains(dto.VatRate))
                    {
                        if (Math.Abs(Math.Abs(amountCurrency / 4) - Math.Abs(amountCurrency * (dto.VATAmount / (amountCurrency - dto.VATAmount)))) < 1)
                            dto.VatRate = 0;
                    }

                    if (dto.VatRate != 0 && !vatRatesToValidate.Contains(dto.VatRate) && !dto.HasHouseholdTaxDeduction && !foreign)
                        hasError = true;
                }
                else
                {
                    // A workaround is to check the size of the amount.
                    if (dto.VatRate != 0 && dto.VatRate != 25M)
                    {
                        if (Math.Abs(Math.Abs(amountCurrency / 4) - Math.Abs(amountCurrency * (dto.VATAmount / (amountCurrency - dto.VATAmount)))) < 1)
                            dto.VatRate = 0;
                    }

                    if (dto.VatRate != 0 && dto.VatRate != 25M && !dto.HasHouseholdTaxDeduction && !foreign)
                        hasError = true;
                }
            }

            if (hasError)
            {
                dto.InfoIcon = dto.InfoIcon | (int)InvoiceRowInfoFlag.Error;
            }

            if (e.HasHouseholdTaxDeduction)
            {
                dto.InfoIcon = dto.InfoIcon | (int)InvoiceRowInfoFlag.HouseHold;
            }

            #endregion

            #region Closed style

            if (setClosedStyle)
            {
                switch ((SoeOriginType)e.OriginType)
                {
                    case SoeOriginType.Order:
                        if (dto.Status == (int)SoeOriginStatus.OrderFullyInvoice || dto.Status == (int)SoeOriginStatus.OrderClosed || dto.Status == (int)SoeOriginStatus.Cancel)
                        {
                            dto.UseClosedStyle = true;
                            dto.IsSelectDisabled = true;
                        }
                        break;
                }
            }

            #endregion

            return dto;
        }

        public static IEnumerable<CustomerInvoiceGridDTO> ToGridDTOs(this IEnumerable<GetCustomerOrders_Result> l, bool hideVatWarnings, bool setClosedStyle, int? baseSysCurrencyId, List<decimal> vatRatesToValidate = null)
        {

            var dtos = new List<CustomerInvoiceGridDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    var dto = e.ToGridDTO(hideVatWarnings, setClosedStyle, baseSysCurrencyId != 0 && e.SysCurrencyId != baseSysCurrencyId, vatRatesToValidate);
                    dtos.Add(dto);
                }
            }
            return dtos;
        }

        public static CustomerInvoiceGridDTO ToGridDTO(this GetCustomerContracts_Result e, bool hideVatWarnings, bool setClosedStyle, bool foreign = false)
        {
            if (e == null)
                return null;

            var dto = new CustomerInvoiceGridDTO
            {
                CustomerInvoiceId = e.InvoiceId,
                OriginType = (int)SoeOriginType.Contract,
                SeqNr = e.InvoiceSeqNr ?? 0,
                InvoiceNr = e.InvoiceNr,

                //BillingTypeName = e.BillingTypeName,
                Status = e.Status,
                //StatusName = e.StatusName,
                ActorCustomerId = e.ActorId,
                ActorCustomerNr = e.ActorNr,
                ActorCustomerName = e.ActorName,
                ActorCustomerNrName = e.ActorNr + " " + e.ActorName,
                InternalText = e.InternalText,
                TotalAmount = e.InvoiceTotalAmount, //foreign ? 0 : e.InvoiceTotalAmount,
                TotalAmountText = e.InvoiceTotalAmount.ToString(), //foreign ? String.Empty : e.InvoiceTotalAmount.ToString(),
                TotalAmountCurrency = e.InvoiceTotalAmountCurrency, //foreign ? e.InvoiceTotalAmountCurrency : 0,
                TotalAmountCurrencyText = e.InvoiceTotalAmountCurrency.ToString(), //foreign ? e.InvoiceTotalAmountCurrency.ToString() : String.Empty,
                VATAmount = foreign ? 0 : e.VATAmount,
                SysCurrencyId = e.SysCurrencyId,
                //CurrencyCode = e.CurrencyCode,
                InvoiceDate = e.InvoiceDate,
                DueDate = e.DueDate,
                OwnerActorId = e.OwnerActorId,
                DeliveryAddressId = e.DeliveryAddressId,
                BillingAddressId = e.BillingAddressId,
                Categories = e.Categories,
                CustomerCategories = e.CustomerCategories,
                StatusIcon = e.StatusIcon,
                DefaultDim2AccountId = e.DefaultDim2AccountId,
                DefaultDim3AccountId = e.DefaultDim3AccountId,
                DefaultDim4AccountId = e.DefaultDim4AccountId,
                DefaultDim5AccountId = e.DefaultDim5AccountId,
                DefaultDim6AccountId = e.DefaultDim6AccountId,
                Users = string.IsNullOrEmpty(e.OriginUsers) ? "" : e.OriginUsers.Trim().TrimEnd(','),
                DeliveryAddress = e.DeliveryAddress,
                DeliveryCity = e.DeliveryCity,
                DeliveryPostalCode = e.DeliveryPostalCode,
                CurrencyRate = e.CurrencyRate,
                ContactEComId = e.ContactEComId,
                PriceListName = e.PriceListName,
                MainUserName = e.MainUserName,
                NextContractPeriod = e.NextContractPeriodYear.ToString() + "-" + (e.NextContractPeriodValue < 10 ? "0" + e.NextContractPeriodValue.ToString() : e.NextContractPeriodValue.ToString()),
                ContractGroupName = e.ContractGroupName,
                NextInvoiceDate = e.NextContractPeriodDate,
                InvoicePaymentServiceId = e.InvoicePaymentService,
            };

            dto.TotalAmountExVat = (dto.TotalAmount - 0) - dto.VATAmount;
            dto.TotalAmountExVatText = dto.TotalAmountExVat.ToString();
            dto.TotalAmountExVatCurrency = (dto.TotalAmountCurrency - 0) - dto.VATAmountCurrency;
            dto.TotalAmountExVatCurrencyText = dto.TotalAmountExVatCurrency.ToString();

            //Set contract values

            switch (e.ContractPeriod)
            {
                case 1:
                    dto.ContractYearlyValue = dto.TotalAmount * (52 / (e.ContractInterval > 0 ? e.ContractInterval : 1));
                    dto.ContractYearlyValueExVat = dto.TotalAmountExVat * (52 / (e.ContractInterval > 0 ? e.ContractInterval : 1));
                    break;
                case 2:
                    dto.ContractYearlyValue = dto.TotalAmount * (12 / (e.ContractInterval > 0 ? e.ContractInterval : 1));
                    dto.ContractYearlyValueExVat = dto.TotalAmountExVat * (12 / (e.ContractInterval > 0 ? e.ContractInterval : 1));
                    break;
                case 3:
                    dto.ContractYearlyValue = dto.TotalAmount * (4 / (e.ContractInterval > 0 ? e.ContractInterval : 1));
                    dto.ContractYearlyValueExVat = dto.TotalAmountExVat * (4 / (e.ContractInterval > 0 ? e.ContractInterval : 1));
                    break;
                default:
                    dto.ContractYearlyValue = dto.TotalAmount;
                    dto.ContractYearlyValueExVat = dto.TotalAmountExVat;
                    break;
            }

            #region Closed style

            if (setClosedStyle)
            {
                if (dto.Status == (int)SoeOriginStatus.Cancel || dto.Status == (int)SoeOriginStatus.ContractClosed || (dto.DueDate.HasValue && dto.DueDate.Value.Date <= DateTime.Today))
                {
                    dto.UseClosedStyle = true;
                    dto.IsSelectDisabled = true;
                }
            }

            #endregion

            return dto;
        }

        public static IEnumerable<CustomerInvoiceGridDTO> ToGridDTOs(this IEnumerable<GetCustomerContracts_Result> l, bool hideVatWarnings, bool setClosedStyle, int? baseSysCurrencyId)
        {

            var dtos = new List<CustomerInvoiceGridDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    var dto = e.ToGridDTO(hideVatWarnings, setClosedStyle, baseSysCurrencyId != 0 && e.SysCurrencyId != baseSysCurrencyId);
                    dtos.Add(dto);
                }
            }
            return dtos;
        }

        public static CustomerInvoiceGridDTO ToGridDTO(this GetCustomerInvoices_Result e, bool hideVatWarnings, bool setClosedStyle, bool closeInvoicesWhenTransferredToVoucher, bool foreign = false, List<decimal> vatRatesToValidate = null)
        {
            if (e == null)
                return null;

            var dto = new CustomerInvoiceGridDTO
            {
                CustomerInvoiceId = e.InvoiceId,
                OriginType = e.OriginType,
                SeqNr = e.InvoiceSeqNr ?? 0,
                InvoiceNr = e.InvoiceNr,
                OCR = e.OCR,
                DeliveryType = e.DeliveryType.GetValueOrDefault(),
                InvoiceDeliveryProvider = e.InvoiceDeliveryProvider.GetValueOrDefault(),
                BillingTypeId = e.BillingTypeId,
                InvoicePaymentServiceId = e.PaymentService.GetValueOrDefault(),
                BillingAddress = e.BillingAddress,
                Status = e.Status,
                ExportStatus = e.ExportStatus,
                ActorCustomerId = e.ActorId,
                ActorCustomerNr = e.ActorNr,
                ActorCustomerName = e.CustomerNameFromInvoice.IsNullOrEmpty() ? e.ActorName : e.CustomerNameFromInvoice,
                ActorCustomerNrName = e.ActorNr + " " + (e.CustomerNameFromInvoice.IsNullOrEmpty() ? e.ActorName : e.CustomerNameFromInvoice),
                InternalText = e.InternalText,
                //InvoicePaymentServiceName = e.InvoicePaymentService,
                TotalAmount = e.InvoiceTotalAmount, //foreign ? 0 : e.InvoiceTotalAmount,
                TotalAmountText = e.InvoiceTotalAmount.ToString(), //foreign ? String.Empty : e.InvoiceTotalAmount.ToString(),
                TotalAmountCurrency = e.InvoiceTotalAmountCurrency, //foreign ? e.InvoiceTotalAmountCurrency : 0,
                TotalAmountCurrencyText = e.InvoiceTotalAmountCurrency.ToString(), //foreign ? e.InvoiceTotalAmountCurrency.ToString() : String.Empty,
                VATAmount = e.VATAmount, //= foreign ? 0 : e.VATAmount,
                VATAmountCurrency = e.VATAmountCurrency,
                PayAmount = e.InvoicePayAmount,//foreign ? 0 : e.InvoicePayAmount,
                PayAmountText = e.InvoicePayAmount.ToString(),//foreign ? String.Empty : e.InvoicePayAmount.ToString(),
                PayAmountCurrency = e.InvoicePayAmountCurrency,//foreign ? e.InvoicePayAmountCurrency : 0,
                PayAmountCurrencyText = e.InvoicePayAmountCurrency.ToString(),//foreign ? e.InvoicePayAmountCurrency.ToString() : String.Empty,
                PaidAmount = e.InvoicePaidAmount, //foreign ? 0 : e.InvoicePaidAmount,
                PaidAmountText = e.InvoicePaidAmount.ToString(), //foreign ? String.Empty : e.InvoicePaidAmount.ToString(),
                PaidAmountCurrency = e.InvoicePaidAmountCurrency, //foreign ? e.InvoicePaidAmountCurrency : 0,
                PaidAmountCurrencyText = e.InvoicePaidAmountCurrency.ToString(), //foreign ? e.InvoicePaidAmountCurrency.ToString() : String.Empty,
                RemainingAmount = e.RemainingAmount,
                RemainingAmountText = e.RemainingAmount.ToString(),
                RemainingAmountExVat = e.RemainingAmountExVat,
                RemainingAmountExVatText = e.RemainingAmountExVat.ToString(),
                SysCurrencyId = e.SysCurrencyId,
                //CurrencyCode = e.CurrencyCode,
                InvoiceDate = e.InvoiceDate,
                DueDate = e.DueDate,
                PayDate = e.PayDate, //Here set as PAIDDATE (payment) and not paydate (invoice)
                OwnerActorId = e.OwnerActorId,
                FullyPaid = e.FullyPayed,
                InvoiceHeadText = e.InvoiceHeadText,
                RegistrationType = e.RegistrationType,
                DeliveryAddressId = e.DeliveryAddressId,
                BillingAddressId = e.BillingAddressId,
                BillingInvoicePrinted = e.BillingInvoicePrinted,
                HasHouseholdTaxDeduction = e.HasHouseholdTaxDeduction,
                HouseholdTaxDeductionType = e.HouseholdTaxDeductionType ?? 0,
                InsecureDebt = e.InsecureDebt,
                MultipleAssetRows = e.MultipleAssetRows,
                NoOfReminders = e.NoOfReminders,
                NoOfPrintedReminders = e.NoOfPrintedReminders != null ? (int)e.NoOfPrintedReminders : 0,
                LastCreatedReminder = e.LastCreatedReminder,
                Categories = e.Categories,
                CustomerCategories = e.CustomerCategories,
                DeliverDateText = e.DeliveryDateText,
                DeliveryDate = e.DeliveryDate,
                OrderNumbers = e.OrderNumbers,
                CustomerGracePeriodDays = e.CustomerGracePeriodDays,
                ProjectNr = e.ProjectNumber,
                ProjectName = e.ProjectName,
                StatusIcon = e.StatusIcon,
                //FixedPriceOrderName = e.FixedPriceOrderName,
                OrderType = e.OrderType,
                OnlyPayment = e.OnlyPayment,
                DefaultDim2AccountId = e.DefaultDim2AccountId,
                DefaultDim3AccountId = e.DefaultDim3AccountId,
                DefaultDim4AccountId = e.DefaultDim4AccountId,
                DefaultDim5AccountId = e.DefaultDim5AccountId,
                DefaultDim6AccountId = e.DefaultDim6AccountId,
                Users = string.IsNullOrEmpty(e.OriginUsers) ? "" : e.OriginUsers.Trim().TrimEnd(','),
                DeliveryAddress = e.DeliveryAddress,
                CurrencyRate = e.CurrencyRate,
                ContactEComId = e.ContactEComId,
                ContactEComText = e.Email,
                ReminderContactEComId = e.ReminderContactEComId,
                ReminderContactEComText = e.ReminderEmail,
                ReferenceYour = e.ReferenceYour,
                PriceListName = e.PriceListName,
                MainUserName = e.MainUserName,
                InvoiceLabel = e.InvoiceLabel,
                Created = e.Created,
                ExternalInvoiceNr = e.ExternalInvoiceNr,
                EinvoiceDistStatus = e.EinvoiceDistStatus,
                isCashSales = e.CashSale,
            };

            dto.TotalAmountExVat = (dto.TotalAmount - e.CentRounding) - dto.VATAmount;
            dto.TotalAmountExVatText = dto.TotalAmountExVat.ToString();
            dto.TotalAmountExVatCurrency = (dto.TotalAmountCurrency - e.CentRounding) - dto.VATAmountCurrency;
            dto.TotalAmountExVatCurrencyText = dto.TotalAmountExVatCurrency.ToString();


            #region Vat warning
            var hasError = false;

            if (hideVatWarnings)
                dto.VatRate = 0;
            else
            {
                // TODO: We need cent rounding for the calculation to be correct, but it is not included in the view.
                // This will cause small invoices to be misleading in this validation.
                decimal cent = 0;
                decimal amountCurrency = e.InvoiceTotalAmountCurrency; //dto.TotalAmountCurrency;
                //decimal sum = amountCurrency - dto.VATAmount - cent;
                decimal sum = e.InvoiceSumAmountCurrency - cent;    // Altered sum as per PBI #108615
                if (sum != 0)
                    dto.VatRate = Decimal.Round((dto.VATAmount / sum * 100), 1);

                if (vatRatesToValidate != null && vatRatesToValidate.Count > 0)
                {
                    // A workaround is to check the size of the amount.
                    if (dto.VatRate != 0 && !vatRatesToValidate.Contains(dto.VatRate))
                    {
                        if (Math.Abs(Math.Abs(amountCurrency / 4) - Math.Abs(amountCurrency * (dto.VATAmount / (amountCurrency - dto.VATAmount)))) < 1)
                            dto.VatRate = 0;
                    }

                    if (dto.VatRate != 0 && !vatRatesToValidate.Contains(dto.VatRate) && !dto.HasHouseholdTaxDeduction && !foreign)
                        hasError = true;
                }
                else
                {
                    // A workaround is to check the size of the amount.
                    if (dto.VatRate != 0 && dto.VatRate != 25M)
                    {
                        if (Math.Abs(Math.Abs(amountCurrency / 4) - Math.Abs(amountCurrency * (dto.VATAmount / (amountCurrency - dto.VATAmount)))) < 1)
                            dto.VatRate = 0;
                    }

                    if (dto.VatRate != 0 && dto.VatRate != 25M && !dto.HasHouseholdTaxDeduction && !foreign)
                        hasError = true;
                }
            }

            if (dto.OriginType == (int)SoeOriginType.CustomerInvoice && ((dto.PaidAmount != 0 && !dto.FullyPaid) || dto.InsecureDebt))
                dto.InfoIcon = dto.InfoIcon | (int)InvoiceRowInfoFlag.Info;

            if (hasError)
                dto.InfoIcon = dto.InfoIcon | (int)InvoiceRowInfoFlag.Error;

            if (e.HasHouseholdTaxDeduction)
                dto.InfoIcon = dto.InfoIcon | (int)InvoiceRowInfoFlag.HouseHold;

            #endregion

            #region Closed style

            if (setClosedStyle)
            {
                switch ((SoeOriginType)e.OriginType)
                {
                    case SoeOriginType.CustomerInvoice:
                        if (closeInvoicesWhenTransferredToVoucher)
                        {
                            if (dto.Status == (int)SoeOriginStatus.Cancel || dto.ExportStatus == (int)SoeInvoiceExportStatusType.ExportedAndClosed ||
                                                    (dto.FullyPaid && dto.Status == (int)SoeOriginStatus.Voucher))
                            {
                                dto.UseClosedStyle = true;
                                dto.IsSelectDisabled = true;
                            }
                        }
                        else
                        {
                            if (dto.Status == (int)SoeOriginStatus.Cancel || dto.ExportStatus == (int)SoeInvoiceExportStatusType.ExportedAndClosed ||
                                                 (dto.FullyPaid && (dto.Status == (int)SoeOriginStatus.Voucher || dto.Status == (int)SoeOriginStatus.Origin)))
                            {
                                dto.UseClosedStyle = true;
                                dto.IsSelectDisabled = true;
                            }
                        }
                        break;
                    case SoeOriginType.Offer:
                        if (dto.Status == (int)SoeOriginStatus.OfferFullyOrder || dto.Status == (int)SoeOriginStatus.OfferFullyInvoice || dto.Status == (int)SoeOriginStatus.OfferClosed || dto.Status == (int)SoeOriginStatus.Cancel)
                        {
                            dto.UseClosedStyle = true;
                            dto.IsSelectDisabled = true;
                        }
                        break;
                    case SoeOriginType.Order:
                        if (dto.Status == (int)SoeOriginStatus.OrderFullyInvoice || dto.Status == (int)SoeOriginStatus.OrderClosed || dto.Status == (int)SoeOriginStatus.Cancel)
                        {
                            dto.UseClosedStyle = true;
                            dto.IsSelectDisabled = true;
                        }
                        break;
                    case SoeOriginType.Contract:
                        if (dto.Status == (int)SoeOriginStatus.Cancel || dto.Status == (int)SoeOriginStatus.ContractClosed || (dto.DueDate.HasValue && dto.DueDate.Value.Date <= DateTime.Today))
                        {
                            dto.UseClosedStyle = true;
                            dto.IsSelectDisabled = true;
                        }
                        break;
                }
            }

            #endregion

            return dto;
        }

        public static IEnumerable<CustomerInvoiceGridDTO> ToGridDTOs(this IEnumerable<GetCustomerInvoices_Result> l, bool hideVatWarnings, bool setClosedStyle, bool closeInvoicesWhenTransferredToVoucher, int? baseSysCurrencyId, List<decimal> vatRatesToValidate = null)
        {
            var dtos = new List<CustomerInvoiceGridDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToGridDTO(hideVatWarnings, setClosedStyle, closeInvoicesWhenTransferredToVoucher, baseSysCurrencyId != 0 && e.SysCurrencyId != baseSysCurrencyId, vatRatesToValidate));
                }
            }
            return dtos;
        }

        public static CustomerInvoiceSmallDialogDTO ToSmallDialogDTO(this GetCustomerInvoices_Result e)
        {
            if (e == null)
                return null;

            CustomerInvoiceSmallDialogDTO dto = new CustomerInvoiceSmallDialogDTO()
            {
                CustomerInvoiceId = e.InvoiceId,
                SeqNr = e.InvoiceSeqNr ?? 0,
                InvoiceNr = e.InvoiceNr,
                ActorCustomerId = e.ActorId,
                ActorCustomerName = String.IsNullOrEmpty(e.CustomerNameFromInvoice) ? e.ActorName : e.CustomerNameFromInvoice,
                TotalAmountCurrency = e.InvoiceTotalAmountCurrency,
                RemainingAmount = e.InvoicePayAmountCurrency,
                SysCurrencyId = e.SysCurrencyId
                //CurrencyCode = e.CurrencyCode,
            };

            return dto;
        }

        public static IEnumerable<CustomerInvoiceSmallDialogDTO> ToSmallDialogDTOs(this IEnumerable<GetCustomerInvoices_Result> l)
        {
            var dtos = new List<CustomerInvoiceSmallDialogDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToSmallDialogDTO());
                }
            }
            return dtos;
        }

        public static CustomerInvoiceGridDTO ToCustomerInvoiceGridDTO(this GetCustomerPayments_Result e, bool hideVatWarnings, bool setClosedStyle, bool closeInvoicesWhenTransferredToVoucher, bool foreign = false, List<decimal> vatRatesToValidate = null)
        {
            if (e == null)
                return null;

            var dto = new CustomerInvoiceGridDTO
            {
                CustomerInvoiceId = e.InvoiceId,
                OriginType = e.OriginType,
                SeqNr = e.InvoiceSeqNr.HasValue ? e.InvoiceSeqNr.Value : 0,
                InvoiceNr = e.InvoiceNr,
                OCR = e.OCR,
                CustomerPaymentId = e.OriginId,
                CustomerPaymentRowId = e.PaymentRowId,
                PaymentSeqNr = e.PaymentSeqNr,
                PaymentNr = e.PaymentNr,
                DeliveryType = 0,
                BillingTypeId = e.BillingTypeId,
                Status = e.Status,
                ExportStatus = 0,
                ActorCustomerId = e.ActorId,
                ActorCustomerNr = e.ActorNr,
                ActorCustomerName = e.CustomerNameFromInvoice.IsNullOrEmpty() ? e.ActorName : e.CustomerNameFromInvoice,
                InternalText = e.InternalText,
                InvoicePaymentServiceName = String.Empty,
                TotalAmount = foreign ? 0 : e.InvoiceTotalAmount,
                TotalAmountText = foreign ? String.Empty : e.InvoiceTotalAmount.ToString(),
                TotalAmountCurrency = foreign ? e.InvoiceTotalAmountCurrency : 0,
                TotalAmountCurrencyText = foreign ? e.InvoiceTotalAmountCurrency.ToString() : String.Empty,
                VATAmount = e.VATAmount,
                PayAmount = foreign ? 0 : e.InvoicePayAmount,
                PayAmountText = foreign ? String.Empty : e.InvoicePayAmount.ToString(),
                PayAmountCurrency = foreign ? e.InvoicePayAmountCurrency : 0,
                PayAmountCurrencyText = foreign ? e.InvoicePayAmountCurrency.ToString() : String.Empty,
                PaidAmount = foreign ? 0 : e.InvoicePaidAmount,
                PaidAmountText = foreign ? String.Empty : e.InvoicePaidAmount.ToString(),
                PaidAmountCurrency = foreign ? e.InvoicePaidAmountCurrency : 0,
                PaidAmountCurrencyText = foreign ? e.InvoicePaidAmountCurrency.ToString() : String.Empty,
                RemainingAmount = e.RemainingAmount,
                RemainingAmountText = e.RemainingAmount.ToString(),
                RemainingAmountExVat = e.RemainingAmountExVat,
                RemainingAmountExVatText = e.RemainingAmountExVat.ToString(),
                PaymentAmount = e.PaymentAmount,
                PaymentAmountCurrency = e.PaymentAmountCurrency,
                PaymentAmountDiff = e.PaymentAmountDiff,
                BankFee = e.BankFee,
                SysCurrencyId = e.SysCurrencyId,
                InvoiceDate = e.InvoiceDate,
                DueDate = e.DueDate,
                PayDate = e.PayDate, //Here set as PAIDDATE (payment) and not paydate (invoice)
                OwnerActorId = e.OwnerActorId,
                FullyPaid = e.FullyPayed,
                InvoiceHeadText = e.InvoiceHeadText,
                RegistrationType = e.RegistrationType,
                DeliveryAddressId = e.DeliveryAddressId,
                BillingAddressId = e.BillingAddressId,
                BillingInvoicePrinted = e.BillingInvoicePrinted,
                HouseholdTaxDeductionType = 0,//e.HouseholdTaxDeductionType,
                HasVoucher = e.HasVoucher,
                InsecureDebt = e.InsecureDebt,
                MultipleAssetRows = e.MultipleAssetRows,
                NoOfReminders = e.NoOfReminders,
                NoOfPrintedReminders = 0,
                Categories = String.Empty,
                DeliverDateText = e.DeliveryDateText,
                OrderNumbers = String.Empty,
                CustomerGracePeriodDays = e.CustomerGracePeriodDays,
                ProjectNr = String.Empty,
                CurrencyRate = e.CurrencyRate,
            };

            dto.TotalAmountExVat = dto.TotalAmount - dto.VATAmount;
            dto.TotalAmountExVatText = dto.TotalAmountExVat.ToString();
            dto.TotalAmountExVatCurrency = dto.TotalAmountCurrency - dto.VATAmountCurrency;
            dto.TotalAmountExVatCurrencyText = dto.TotalAmountExVatCurrency.ToString();

            #region Vat warning

            if (hideVatWarnings)
                dto.VatRate = 0;
            else
            {
                // TODO: We need cent rounding for the calculation to be correct, but it is not included in the view.
                // This will cause small invoices to be misleading in this validation.
                decimal cent = 0;
                decimal sum = dto.TotalAmountCurrency - dto.VATAmount - cent;
                if (sum != 0)
                    dto.VatRate = Decimal.Round((dto.VATAmount / sum * 100), 1);

                if (vatRatesToValidate != null && vatRatesToValidate.Count > 0)
                {
                    // A workaround is to check the size of the amount.
                    if (dto.VatRate != 0 && !vatRatesToValidate.Contains(dto.VatRate))
                    {
                        if (Math.Abs(Math.Abs(dto.TotalAmountCurrency / 4) - Math.Abs(dto.TotalAmountCurrency * (dto.VATAmount / (dto.TotalAmountCurrency - dto.VATAmount)))) < 1)
                            dto.VatRate = 0;
                    }
                }
                else
                {
                    // A workaround is to check the size of the amount.
                    if (dto.VatRate != 0 && dto.VatRate != 25M)
                    {
                        if (Math.Abs(Math.Abs(dto.TotalAmountCurrency / 4) - Math.Abs(dto.TotalAmountCurrency * (dto.VATAmount / (dto.TotalAmountCurrency - dto.VATAmount)))) < 1)
                            dto.VatRate = 0;
                    }
                }
            }

            #endregion

            return dto;
        }

        public static List<SmallGenericType> ToSmallGenericTypes(this IEnumerable<GetCustomerInvoices_Result> l)
        {
            List<SmallGenericType> smallList = new List<SmallGenericType>();
            foreach (var item in l)
            {
                smallList.Add(new SmallGenericType()
                {
                    Id = item.InvoiceId,
                    Name = item.InvoiceNr + " " + (String.IsNullOrEmpty(item.CustomerNameFromInvoice) ? item.ActorName : item.CustomerNameFromInvoice),
                });
            }
            return smallList;
        }

        #endregion

        #region SupplierInvoiceGridDTO

        public static SupplierInvoiceGridDTO ToGridDTO(this GetSupplierInvoices_Result e, bool hideVatWarnings, bool setClosedStyle, bool closeInvoicesWhenTransferredToVoucher, bool foreign = false)
        {
            if (e == null)
                return null;

            var dto = new SupplierInvoiceGridDTO
            {
                SupplierInvoiceId = e.InvoiceId,
                OwnerActorId = e.OwnerActorId,
                SeqNr = e.InvoiceSeqNr.HasValue ? e.InvoiceSeqNr.Value.ToString() : string.Empty,
                InvoiceNr = e.InvoiceNr,
                BillingTypeId = e.BillingTypeId,
                Status = e.Status,
                SupplierNr = e.ActorNr,
                SupplierName = e.ActorName,
                SupplierId = e.ActorId,
                TotalAmount = e.InvoiceTotalAmount,
                TotalAmountText = e.InvoiceTotalAmount.ToString(),
                TotalAmountCurrency = foreign ? e.InvoiceTotalAmountCurrency : 0,
                TotalAmountCurrencyText = foreign ? e.InvoiceTotalAmountCurrency.ToString() : string.Empty,
                VATAmount = e.VATAmount,
                VATAmountCurrency = foreign ? e.VATAmountCurrency : 0,
                PayAmount = !e.FullyPayed ? e.InvoicePayAmount : 0,
                PayAmountText = !e.FullyPayed ? e.InvoicePayAmount.ToString() : string.Empty,
                PayAmountCurrency = foreign && !e.FullyPayed ? e.InvoicePayAmountCurrency : 0,
                PayAmountCurrencyText = foreign && !e.FullyPayed ? e.InvoicePayAmountCurrency.ToString() : string.Empty,
                PaidAmount = e.InvoicePaidAmount,
                PaidAmountCurrency = e.InvoicePaidAmountCurrency,
                VatType = e.VatType,
                SysCurrencyId = e.SysCurrencyId,
                InvoiceDate = e.InvoiceDate,
                DueDate = e.DueDate,
                PayDate = e.PaidDate, //Here set as PAIDDATE (payment) and not paydate (invoice)
                VoucherDate = e.VoucherDate,
                CurrentAttestUserName = e.CurrentAttestUsers,
                AttestStateId = e.SupplierInvoiceAttestStateId,
                AttestGroupId = e.SupplierInvoiceAttestGroupId != null ? (int)e.SupplierInvoiceAttestGroupId : 0,
                FullyPaid = e.FullyPayed,
                PaymentStatuses = e.PaymentStatuses,
                StatusIcon = e.StatusIcon,
                HasVoucher = e.HasVoucher,
                CurrencyRate = e.CurrencyRate,
                InternalText = e.InternalText,
                TimeDiscountDate = e.TimeDiscountDate,
                TimeDiscountPercent = e.TimeDiscountPercent,
                isAttestRejected = e.IsAttestRejected,
                Ocr = e.OCR,
                IsOverdue = (e.DueDate.HasValue && e.DueDate.Value.Date < DateTime.Today.Date),
                IsAboutToDue = (e.DueDate.HasValue && e.DueDate.Value.Date <= DateTime.Today.AddDays(5).Date),
                HasAttestComment = e.Comment != null && !String.IsNullOrEmpty(e.Comment.Trim()),
                HasPDF = e.HasImage.HasValue && e.HasImage.Value == 1,
                BlockPayment = e.BlockPayment,
                BlockReason = e.BlockReason,
            };

            var paymentStatusIds = !string.IsNullOrEmpty(dto.PaymentStatuses) ? dto.PaymentStatuses.Split(',').Select(int.Parse).ToList() : new List<int>();
            dto.NoOfPaymentRows = paymentStatusIds.Count(x => x != (int)SoePaymentStatus.Cancel);
            dto.NoOfCheckedPaymentRows = paymentStatusIds.Count(x => x == (int)SoePaymentStatus.Checked);
            dto.SetValues(hideVatWarnings, setClosedStyle, closeInvoicesWhenTransferredToVoucher, foreign);

            return dto;
        }

        public static SupplierInvoiceGridDTO ToGridDTO(this GetSupplierInvoicesForGrid_Result e, bool hideVatWarnings, bool setClosedStyle, bool closeInvoicesWhenTransferredToVoucher, bool foreign = false)
        {
            if (e == null)
                return null;

            var dto = new SupplierInvoiceGridDTO
            {
                SupplierInvoiceId = e.InvoiceId,
                OwnerActorId = e.OwnerActorId,
                SeqNr = e.InvoiceSeqNr.HasValue ? e.InvoiceSeqNr.Value.ToString() : string.Empty,
                InvoiceNr = e.InvoiceNr,
                BillingTypeId = e.BillingTypeId,
                Status = e.Status,
                SupplierNr = e.ActorNr,
                SupplierName = e.ActorName,
                SupplierId = e.ActorId,
                TotalAmount = e.InvoiceTotalAmount,
                TotalAmountText = e.InvoiceTotalAmount.ToString(),
                TotalAmountCurrency = foreign ? e.InvoiceTotalAmountCurrency : 0,
                TotalAmountCurrencyText = foreign ? e.InvoiceTotalAmountCurrency.ToString() : string.Empty,
                VATAmount = e.VATAmount,
                VATAmountCurrency = foreign ? e.VATAmountCurrency : 0,
                PayAmount = !e.FullyPayed ? e.InvoicePayAmount : 0,
                PayAmountText = !e.FullyPayed ? e.InvoicePayAmount.ToString() : string.Empty,
                PayAmountCurrency = foreign && !e.FullyPayed ? e.InvoicePayAmountCurrency : 0,
                PayAmountCurrencyText = foreign && !e.FullyPayed ? e.InvoicePayAmountCurrency.ToString() : string.Empty,
                PaidAmount = e.InvoicePaidAmount,
                PaidAmountCurrency = e.InvoicePaidAmountCurrency,
                VatType = e.VatType,
                SysCurrencyId = e.SysCurrencyId,
                InvoiceDate = e.InvoiceDate,
                DueDate = e.DueDate,
                PayDate = e.PaidDate, //Here set as PAIDDATE (payment) and not paydate (invoice)
                VoucherDate = e.VoucherDate,
                CurrentAttestUserName = e.CurrentAttestUsers,
                AttestStateId = e.SupplierInvoiceAttestStateId,
                AttestGroupId = e.SupplierInvoiceAttestGroupId != null ? (int)e.SupplierInvoiceAttestGroupId : 0,
                FullyPaid = e.FullyPayed,
                PaymentStatuses = e.PaymentStatuses,
                StatusIcon = e.StatusIcon,
                HasVoucher = e.HasVoucher,
                CurrencyRate = e.CurrencyRate,
                InternalText = e.InternalText,
                TimeDiscountDate = e.TimeDiscountDate,
                TimeDiscountPercent = e.TimeDiscountPercent,
                isAttestRejected = e.IsAttestRejected,
                Ocr = e.OCR,
                IsOverdue = (e.DueDate.HasValue && e.DueDate.Value.Date < DateTime.Today.Date),
                IsAboutToDue = (e.DueDate.HasValue && e.DueDate.Value.Date <= DateTime.Today.AddDays(5).Date),
                HasAttestComment = e.Comment != null && !String.IsNullOrEmpty(e.Comment.Trim()),
                HasPDF = e.HasImage.HasValue && e.HasImage.Value == 1,
                BlockPayment = e.BlockPayment,
                BlockReason = e.BlockReason,
            };

            var paymentStatusIds = !string.IsNullOrEmpty(dto.PaymentStatuses) ? dto.PaymentStatuses.Split(',').Select(int.Parse).ToList() : new List<int>();
            dto.NoOfPaymentRows = paymentStatusIds.Count(x => x != (int)SoePaymentStatus.Cancel);
            dto.NoOfCheckedPaymentRows = paymentStatusIds.Count(x => x == (int)SoePaymentStatus.Checked);
            dto.SetValues(hideVatWarnings, setClosedStyle: false, closeInvoicesWhenTransferredToVoucher, foreign);

            if (e.IsClosed == 1)
            {
                dto.UseClosedStyle = true;
                dto.IsSelectDisabled = true;
            }

            return dto;
        }

        public static SupplierInvoiceGridDTO ToGridDTO(this GetSupplierInvoicesForProject_Result e, bool hideVatWarnings, bool setClosedStyle, bool closeInvoicesWhenTransferredToVoucher, bool foreign = false)
        {
            if (e == null)
                return null;

            var dto = new SupplierInvoiceGridDTO
            {
                SupplierInvoiceId = e.InvoiceId,
                SeqNr = e.InvoiceSeqNr.HasValue ? e.InvoiceSeqNr.Value.ToString() : string.Empty,
                InvoiceNr = e.InvoiceNr,
                BillingTypeId = e.BillingTypeId,
                Status = e.Status,
                SupplierNr = e.SupplierNr,
                SupplierName = e.SupplierName,
                SupplierId = e.SupplierId,
                TotalAmount = e.InvoiceTotalAmount,
                TotalAmountText = e.InvoiceTotalAmount.ToString(),
                TotalAmountCurrency = foreign ? e.InvoiceTotalAmountCurrency : 0,
                TotalAmountCurrencyText = foreign ? e.InvoiceTotalAmountCurrency.ToString() : string.Empty,
                VATAmount = e.VATAmount,
                VATAmountCurrency = foreign ? e.VATAmountCurrency : 0,
                PayAmount = !e.FullyPayed ? e.InvoicePayAmount : 0,
                PayAmountText = !e.FullyPayed ? e.InvoicePayAmount.ToString() : string.Empty,
                PayAmountCurrency = foreign && !e.FullyPayed ? e.InvoicePayAmountCurrency : 0,
                PayAmountCurrencyText = foreign && !e.FullyPayed ? e.InvoicePayAmountCurrency.ToString() : string.Empty,
                PaidAmount = e.InvoicePaidAmount,
                PaidAmountCurrency = e.InvoicePaidAmountCurrency,
                VatType = e.VatType,
                InvoiceDate = e.InvoiceDate,
                DueDate = e.DueDate,
                PayDate = e.PaidDate,
                CurrentAttestUserName = e.CurrentAttestUsers,
                AttestStateId = e.SupplierInvoiceAttestStateId,
                FullyPaid = e.FullyPayed,
                PaymentStatuses = e.PaymentStatuses,
                StatusIcon = e.StatusIcon,
                InternalText = e.InternalText,
                isAttestRejected = e.IsAttestRejected,
                IsOverdue = (e.DueDate.HasValue && e.DueDate.Value.Date < DateTime.Today.Date),
                HasAttestComment = e.Comment != null && !String.IsNullOrEmpty(e.Comment.Trim()),
                HasPDF = e.HasImage.HasValue && e.HasImage.Value == 1,
                BlockPayment = e.BlockPayment,
                BlockReason = e.BlockReason,
                ProjectAmount = e.ChargedToProject ?? 0,
                ProjectInvoicedAmount = e.ReInvoiceCost ?? 0,
                ProjectInvoicedSalesAmount = e.ReInvoiceSale ?? 0,
                OwnerActorId = e.OwnerActorId,
            };
            var paymentStatusIds = !string.IsNullOrEmpty(dto.PaymentStatuses) ? dto.PaymentStatuses.Split(',').Select(int.Parse).ToList() : new List<int>();
            dto.NoOfPaymentRows = paymentStatusIds.Count(x => x != (int)SoePaymentStatus.Cancel);
            dto.NoOfCheckedPaymentRows = paymentStatusIds.Count(x => x == (int)SoePaymentStatus.Checked);
            dto.SetValues(hideVatWarnings, setClosedStyle, closeInvoicesWhenTransferredToVoucher, foreign);
            return dto;
        }

        public static void SetValues(this SupplierInvoiceGridDTO dto, bool hideVatWarnings, bool setClosedStyle, bool closeInvoicesWhenTransferredToVoucher, bool foreign = false)
        {

            #region Vat check

            switch (dto.VatType)
            {
                case (int)TermGroup_InvoiceVatType.None:
                    if (dto.BillingTypeId == (int)TermGroup_BillingType.Debit && dto.VATAmount < 0)
                    {
                        dto.TotalAmountExVat = dto.TotalAmount;
                        dto.TotalAmount += dto.VATAmount;
                        dto.TotalAmountText = dto.TotalAmount.ToString();
                    }
                    else
                    {
                        dto.TotalAmountExVat = dto.TotalAmount - dto.VATAmount;
                    }
                    dto.TotalAmountExVatText = dto.TotalAmountExVat.ToString();
                    break;
                case (int)TermGroup_InvoiceVatType.Merchandise:
                    if (dto.BillingTypeId == (int)TermGroup_BillingType.Debit && dto.VATAmount < 0)
                    {
                        dto.TotalAmountExVat = dto.TotalAmount;
                        dto.TotalAmount += dto.VATAmount;
                        dto.TotalAmountText = dto.TotalAmount.ToString();
                    }
                    else
                    {
                        dto.TotalAmountExVat = dto.TotalAmount - dto.VATAmount;
                    }
                    dto.TotalAmountExVatText = dto.TotalAmountExVat.ToString();
                    break;
                case (int)TermGroup_InvoiceVatType.Contractor:
                    dto.TotalAmountExVat = dto.TotalAmount;
                    dto.TotalAmountExVatText = dto.TotalAmountExVat.ToString();
                    break;
                case (int)TermGroup_InvoiceVatType.NoVat:
                    dto.TotalAmountExVat = dto.TotalAmount;
                    dto.TotalAmountExVatText = dto.TotalAmountExVat.ToString();
                    break;
                default:
                    if (dto.BillingTypeId == (int)TermGroup_BillingType.Debit && dto.VATAmount < 0)
                    {
                        dto.TotalAmountExVat = dto.TotalAmount;
                        dto.TotalAmount += dto.VATAmount;
                        dto.TotalAmountText = dto.TotalAmount.ToString();
                    }
                    else
                    {
                        dto.TotalAmountExVat = dto.TotalAmount - dto.VATAmount;
                    }
                    dto.TotalAmountExVatText = dto.TotalAmountExVat.ToString();
                    break;
            }

            if (dto.BillingTypeId == (int)TermGroup_BillingType.Debit && dto.VATAmount < 0)
            {
                dto.TotalAmountExVatCurrency = dto.TotalAmountCurrency;
                dto.TotalAmountCurrency += dto.VATAmountCurrency;
                dto.TotalAmountText = dto.TotalAmountCurrency.ToString();
            }
            else
            {
                dto.TotalAmountExVatCurrency = dto.TotalAmountCurrency - dto.VATAmountCurrency;
                dto.TotalAmountExVatCurrencyText = dto.TotalAmountExVatCurrency.ToString();
            }


            #endregion

            #region Vat warning

            if (hideVatWarnings)
                dto.VatRate = 0;
            else
            {
                // TODO: We need cent rounding for the calculation to be correct, but it is not included in the view.
                // This will cause small invoices to be misleading in this validation.
                decimal cent = 0;
                decimal sum = dto.TotalAmountCurrency - dto.VATAmount - cent;
                if (sum != 0)
                    dto.VatRate = Decimal.Round((dto.VATAmount / sum * 100), 1);

                // A workaround is to check the size of the amount.
                if (dto.VatRate != 0 && dto.VatRate != 25M)
                {
                    if (Math.Abs(Math.Abs(dto.TotalAmountCurrency / 4) - Math.Abs(dto.TotalAmountCurrency * (dto.VATAmount / (dto.TotalAmountCurrency - dto.VATAmount)))) < 1)
                        dto.VatRate = 0;
                }
            }

            #endregion

            #region Closed style

            if (!setClosedStyle)
                return;

            if (!closeInvoicesWhenTransferredToVoucher)
            {
                if (dto.Status == (int)SoeOriginStatus.Cancel ||
                    (
                    dto.FullyPaid
                    && (dto.Status == (int)SoeOriginStatus.Voucher ||
                        dto.Status == (int)SoeOriginStatus.Origin)
                    )
                )
                {
                    dto.UseClosedStyle = true;
                    dto.IsSelectDisabled = true;
                }
                return;
            }

            if (dto.Status == (int)SoeOriginStatus.Cancel ||
                (
                    dto.FullyPaid
                    && dto.Status == (int)SoeOriginStatus.Voucher
                    && (
                        String.IsNullOrEmpty(dto.PaymentStatuses) ||
                        dto.PaymentStatuses
                            .Split(',')
                            .Select(int.Parse)
                            .All(s => s == (int)SoePaymentStatus.Cancel ||
                                      s == (int)SoePaymentStatus.Checked)
                    )
                )
            )
            {
                dto.UseClosedStyle = true;
                dto.IsSelectDisabled = true;
            }

            #endregion
        }

        #endregion

        #region SupplierPaymentGridDTO

        public static SupplierPaymentGridDTO ToGridDTO(this GetSupplierPayments_Result e, bool hideVatWarnings)
        {
            if (e == null)
                return null;

            SupplierPaymentGridDTO dto = new SupplierPaymentGridDTO
            {
                SupplierPaymentId = e.OriginId,
                SupplierInvoiceId = e.InvoiceId,
                OwnerActorId = e.OwnerActorId,
                InvoiceSeqNr = e.InvoiceSeqNr.HasValue ? e.InvoiceSeqNr.Value.ToString() : string.Empty,
                PaymentSeqNr = e.PaymentSeqNr,
                SequenceNumber = e.SequenceNumber,
                SequenceNumberRecordId = e.SequenceNumberRecordId,
                InvoiceNr = e.InvoiceNr,
                OCR = e.OCR,
                PaymentStatus = e.PaymentStatus,
                BillingTypeId = e.BillingTypeId,
                Status = e.Status,
                SupplierId = e.ActorId,
                SupplierNr = e.ActorNr,
                SupplierName = e.ActorName,
                TotalAmount = e.InvoiceTotalAmount,
                TotalAmountCurrency = e.InvoiceTotalAmountCurrency,
                VATAmount = e.VATAmount,
                PaidAmount = e.InvoicePaidAmount,
                PaidAmountCurrency = e.InvoicePaidAmountCurrency,
                PayAmount = e.InvoicePayAmount,
                PayAmountCurrency = e.InvoicePayAmountCurrency,
                SysCurrencyId = e.SysCurrencyId,
                CurrencyRate = e.CurrencyRate,
                InvoiceDate = e.InvoiceDate,
                DueDate = e.DueDate,
                PayDate = e.PayDate,
                AttestStateId = e.SupplierInvoiceAttestStateId,
                FullyPaid = e.FullyPayed,
                SysPaymentMethodId = e.SysPaymentMethodId,
                PaymentMethodName = e.PaymentMethodName,
                PaymentRowId = e.PaymentRowId,
                PaymentNr = e.PaymentNr,
                PaymentNrString = e.PaymentNrString,
                PaymentAmount = e.PaymentAmount,
                PaymentAmountCurrency = e.PaymentAmountCurrency,
                PaymentAmountDiff = e.PaymentAmountDiff,
                BankFee = e.BankFee,
                TimeDiscountDate = e.TimeDiscountDate,
                TimeDiscountPercent = e.TimeDiscountPercent,
                BlockPayment = e.BlockPayment,
                SupplierBlockPayment = e.SupplierBlockPayment,
                HasVoucher = e.HasVoucher,
                MultipleDebtRows = e.MultipleDebtRows,
                SysPaymentTypeId = e.SysPaymentTypeId,
                Description = e.InternalText,
            };

            dto.TotalAmountExVat = dto.TotalAmount - dto.VATAmount;
            dto.TotalAmountExVatCurrency = dto.TotalAmountCurrency - dto.VATAmountCurrency;

            #region Vat warning

            if (hideVatWarnings)
                dto.VatRate = 0;
            else
            {
                // TODO: We need cent rounding for the calculation to be correct, but it is not included in the view.
                // This will cause small invoices to be misleading in this validation.
                decimal cent = 0;
                decimal sum = dto.TotalAmountCurrency - dto.VATAmount - cent;
                if (sum != 0)
                    dto.VatRate = Decimal.Round((dto.VATAmount / sum * 100), 1);

                // A workaround is to check the size of the amount.
                if (dto.VatRate != 0 && dto.VatRate != 25M)
                {
                    if (Math.Abs(Math.Abs(dto.TotalAmountCurrency / 4) - Math.Abs(dto.TotalAmountCurrency * (dto.VATAmount / (dto.TotalAmountCurrency - dto.VATAmount)))) < 1)
                        dto.VatRate = 0;
                }
            }

            #endregion

            return dto;
        }

        public static SupplierPaymentGridDTO ToPaymentGridDTO(this GetSupplierInvoices_Result e, bool hideVatWarnings)
        {
            if (e == null)
                return null;

            SupplierPaymentGridDTO dto = new SupplierPaymentGridDTO
            {
                SupplierPaymentId = 0,
                SupplierInvoiceId = e.InvoiceId,
                OwnerActorId = e.OwnerActorId,
                InvoiceSeqNr = e.InvoiceSeqNr.HasValue ? e.InvoiceSeqNr.Value.ToString() : string.Empty,
                PaymentSeqNr = 0,
                SequenceNumber = 0,
                SequenceNumberRecordId = 0,
                InvoiceNr = e.InvoiceNr,
                OCR = e.OCR,
                BillingTypeId = e.BillingTypeId,
                //BillingTypeName = e.BillingTypeName,
                Status = e.Status,
                //StatusName = e.StatusName,
                SupplierNr = e.ActorNr,
                SupplierName = e.ActorName,
                SupplierId = e.ActorId,
                TotalAmount = e.InvoiceTotalAmount,
                TotalAmountCurrency = e.InvoiceTotalAmountCurrency,
                VATAmount = e.VATAmount,
                PaidAmount = e.InvoicePaidAmount,
                PaidAmountCurrency = e.InvoicePaidAmountCurrency,
                PayAmount = e.InvoicePayAmount,
                PayAmountCurrency = e.InvoicePayAmountCurrency,
                SysCurrencyId = e.SysCurrencyId,
                //CurrencyCode = e.CurrencyCode,
                CurrencyRate = e.CurrencyRate,
                InvoiceDate = e.InvoiceDate,
                DueDate = e.DueDate,
                PayDate = e.PayDate,
                VoucherDate = e.VoucherDate,
                AttestStateId = e.SupplierInvoiceAttestStateId,
                FullyPaid = e.FullyPayed,
                SysPaymentMethodId = e.SysPaymentMethodId,
                PaymentMethodName = e.PaymentMethodName,
                PaymentNr = e.PaymentNr,
                TimeDiscountDate = e.TimeDiscountDate,
                TimeDiscountPercent = e.TimeDiscountPercent,
                BlockPayment = e.BlockPayment,
                SupplierBlockPayment = e.SupplierBlockPayment,
                HasVoucher = e.HasVoucher,
                StatusIcon = e.StatusIcon,
                MultipleDebtRows = e.MultipleDebtRows,
                HasAttestComment = e.Comment != null && !String.IsNullOrEmpty(e.Comment.Trim()),
                BlockReason = e.BlockReason,
                Description = e.InternalText,
            };

            if (e.SysPaymentTypeId.HasValue)
                dto.SysPaymentTypeId = e.SysPaymentTypeId.Value;

            dto.TotalAmountExVat = dto.TotalAmount - dto.VATAmount;
            dto.TotalAmountExVatCurrency = dto.TotalAmountCurrency - dto.VATAmountCurrency;

            #region Vat warning

            if (hideVatWarnings)
                dto.VatRate = 0;
            else
            {
                // TODO: We need cent rounding for the calculation to be correct, but it is not included in the view.
                // This will cause small invoices to be misleading in this validation.
                decimal cent = 0;
                decimal sum = dto.TotalAmountCurrency - dto.VATAmount - cent;
                if (sum != 0)
                    dto.VatRate = Decimal.Round((dto.VATAmount / sum * 100), 1);

                // A workaround is to check the size of the amount.
                if (dto.VatRate != 0 && dto.VatRate != 25M)
                {
                    if (Math.Abs(Math.Abs(dto.TotalAmountCurrency / 4) - Math.Abs(dto.TotalAmountCurrency * (dto.VATAmount / (dto.TotalAmountCurrency - dto.VATAmount)))) < 1)
                        dto.VatRate = 0;
                }
            }

            #endregion

            return dto;
        }

        public static IEnumerable<SupplierPaymentGridDTO> ToPaymentGridDTOs(this IEnumerable<GetSupplierInvoices_Result> l, bool hideVatWarnings)
        {
            var dtos = new List<SupplierPaymentGridDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToPaymentGridDTO(hideVatWarnings));
                }
            }
            return dtos;
        }

        #endregion

        #region Currency

        public static CompCurrencyDTO ToDTO(this CompCurrency e, bool includeRates)
        {
            if (e == null)
                return null;

            CompCurrencyDTO dto = new CompCurrencyDTO
            {
                CurrencyId = e.CurrencyId,
                SysCurrencyId = e.SysCurrencyId,
                Code = e.Code,
                Name = e.Name,
                Date = e.Date,
                RateToBase = e.RateToBase,
            };

            // Extensions
            if (includeRates)
                dto.CompCurrencyRates = (e.CompCurrencyRates != null && e.CompCurrencyRates.Count > 0) ? e.CompCurrencyRates.ToDTOs().ToList() : new List<CompCurrencyRateDTO>();

            return dto;
        }

        public static IEnumerable<CompCurrencyDTO> ToDTOs(this IEnumerable<CompCurrency> l, bool includeRates)
        {
            var dtos = new List<CompCurrencyDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO(includeRates));
                }
            }
            return dtos;
        }
        public static IEnumerable<CurrencyDTO> ToExDTOs(this IEnumerable<Currency> l)
        {
            var dtos = new List<CurrencyDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }
        public static CurrencyDTO ToDTO(this Currency e)
        {
            if (e == null)
                return null;

            CurrencyDTO dto = new CurrencyDTO
            {
                CurrencyId = e.CurrencyId,
                SysCurrencyId = e.SysCurrencyId,
                IntervalType = (TermGroup_CurrencyIntervalType)e.IntervalType,
                IntervalName = e.IntervalName,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
            };

            if (e.CurrencyRate != null && e.CurrencyRate.Count > 0)
                dto.CurrencyRates = e.CurrencyRate.ToDTOs().ToList();

            return dto;
        }
        public static IEnumerable<CurrencyRateDTO> ToDTOs(this IEnumerable<CurrencyRate> l)
        {
            var dtos = new List<CurrencyRateDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }
        public static CurrencyRateDTO ToDTO(this CurrencyRate e)
        {
            if (e == null)
                return null;

            CurrencyRateDTO dto = new CurrencyRateDTO
            {
                CurrencyRateId = e.CurrencyRateId,
                CurrencyId = e.CurrencyId,
                RateToBase = e.RateToBase ?? 0m,
                RateFromBase = e.RateFromBase ?? 0m,
                Source = (TermGroup_CurrencySource)e.Source,
                SourceName = e.SourceName,
                Date = e.Date ?? DateTime.MinValue,
            };

            return dto;
        }
        public static IEnumerable<CurrencyGridDTO> ToGridDTOs(this IEnumerable<Currency> l)
        {
            var dtos = new List<CurrencyGridDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToGridDTO());
                }
            }
            return dtos;
        }
        public static CurrencyGridDTO ToGridDTO(this Currency e)
        {
            if (e == null)
                return null;

            CurrencyGridDTO dto = new CurrencyGridDTO
            {
                CurrencyId = e.CurrencyId,
                Code = e.Code,
                Name = e.Name,
                IntervalName = e.IntervalName
            };

            return dto;
        }

        public static CompCurrencyGridDTO ToGridDTO(this CompCurrency e, bool includeRates)
        {
            if (e == null)
                return null;

            CompCurrencyGridDTO dto = new CompCurrencyGridDTO()
            {
                CurrencyId = e.CurrencyId,
                Code = e.Code,
                Name = e.Name,
            };

            // Extensions
            if (includeRates)
                dto.CompCurrencyRates = (e.CompCurrencyRates != null && e.CompCurrencyRates.Count > 0) ? e.CompCurrencyRates.ToDTOs().ToList() : new List<CompCurrencyRateDTO>();

            return dto;
        }

        public static IEnumerable<CompCurrencyGridDTO> ToGridDTOs(this IEnumerable<CompCurrency> l, bool includeRates)
        {
            var dtos = new List<CompCurrencyGridDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToGridDTO(includeRates));
                }
            }
            return dtos;
        }

        public static CompCurrencySmallDTO ToSmallDTO(this CompCurrency e)
        {
            if (e == null)
                return null;

            CompCurrencySmallDTO dto = new CompCurrencySmallDTO()
            {
                CurrencyId = e.CurrencyId,
                Code = e.Code,
                Name = e.Name,
            };

            return dto;
        }

        public static IEnumerable<CompCurrencySmallDTO> ToSmallDTOs(this IEnumerable<CompCurrency> l)
        {
            var dtos = new List<CompCurrencySmallDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToSmallDTO());
                }
            }
            return dtos;
        }

        public static CompCurrencyRateDTO ToDTO(this CompCurrencyRate e)
        {
            if (e == null)
                return null;

            CompCurrencyRateDTO dto = new CompCurrencyRateDTO()
            {
                CurrencyId = e.CurrencyId,
                Code = e.Code,
                Name = e.Name,
                IntervalType = (TermGroup_CurrencyIntervalType)e.IntervalType,
                Source = (TermGroup_CurrencySource)e.Source,
                Date = e.Date,
                RateToBase = e.RateToBase,
            };

            // Extensions
            dto.IntervalTypeName = e.IntervalTypeName;
            dto.SourceName = e.SourceName;

            return dto;
        }

        public static IEnumerable<CompCurrencyRateDTO> ToDTOs(this IEnumerable<CompCurrencyRate> l)
        {
            var dtos = new List<CompCurrencyRateDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static CompCurrencyRateGridDTO ToGridDTO(this CompCurrencyRate e)
        {
            if (e == null)
                return null;

            CompCurrencyRateGridDTO dto = new CompCurrencyRateGridDTO()
            {
                CurrencyRateId = e.CurrencyRateId,
                Code = e.Code,
                Name = e.Name,
                Date = e.Date,
                RateToBase = e.RateToBase,
            };

            // Extensions
            dto.IntervalTypeName = e.IntervalTypeName;
            dto.SourceName = e.SourceName;

            return dto;
        }

        public static IEnumerable<CompCurrencyRateGridDTO> ToGridDTOs(this IEnumerable<CompCurrencyRate> l)
        {
            var dtos = new List<CompCurrencyRateGridDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToGridDTO());
                }
            }
            return dtos;
        }

        #endregion

        #region ContactEcomView

        public static ContactEcomView GetLastCreatedOrModified(this List<ContactEcomView> ecoms)
        {
            if (ecoms.IsNullOrEmpty())
                return null;

            ContactEcomView lastCreated = ecoms.OrderByDescending(o => o.Created).FirstOrDefault();
            ContactEcomView lastModified = ecoms.OrderByDescending(o => o.Modified).FirstOrDefault();
            return lastModified.IsModifiedAfterCreated() ? lastModified : lastCreated;
        }

        public static List<ContactEcomView> GetLastCreatedOrModifiedList(this List<ContactEcomView> ecoms, int take)
        {
            if (ecoms.IsNullOrEmpty())
                return null;
            return ecoms.OrderByDescending(o => o.GetLastCreatedOrModifiedDate()).Take(take).ToList();
        }

        public static bool IsModifiedAfterCreated(this ContactEcomView e)
        {
            return e.Modified.HasValue && e.Created.HasValue && e.Modified.Value > e.Created.Value;
        }

        public static DateTime GetLastCreatedOrModifiedDate(this ContactEcomView e)
        {
            return (e.IsModifiedAfterCreated() ? e.Modified : e.Created) ?? CalendarUtility.DATETIME_DEFAULT;
        }

        #endregion

        #region EDI/Scanning

        public static EdiEntryViewDTO ToDTO(this EdiEntryView e)
        {
            if (e == null)
                return null;

            EdiEntryViewDTO dto = new EdiEntryViewDTO()
            {
                //Edi
                EdiEntryId = e.EdiEntryId,
                ActorCompanyId = e.ActorCompanyId,
                Type = (TermGroup_EDISourceType)e.Type,
                Status = (TermGroup_EDIStatus)e.Status,
                BillingType = e.BillingType.HasValue ? (TermGroup_BillingType)e.BillingType : TermGroup_BillingType.None,
                WholesellerId = e.WholeSellerId,
                WholesellerName = e.WholesellerName,
                BuyerId = e.BuyerId,
                BuyerReference = e.BuyerReference,
                HasPdf = e.HasPdf,
                ErrorCode = e.ErrorCode,
                Created = e.Created,
                State = (SoeEntityState)e.State,

                //Scanning
                ScanningEntryId = e.ScanningEntryId,
                OperatorMessage = e.OperatorMessage,

                //Dates
                Date = e.Date,
                InvoiceDate = e.InvoiceDate,
                DueDate = e.DueDate,

                //Sum
                Sum = e.Sum,
                SumCurrency = e.SumCurrency,
                SumVat = e.SumVat,
                SumVatCurrency = e.SumVatCurrency,

                //Currency
                CurrencyId = e.CurrencyId,
                SysCurrencyId = e.SysCurrencyId,
                CurrencyRate = e.CurrencyRate,

                //Order
                OrderId = e.OrderId,
                OrderStatus = (TermGroup_EDIOrderStatus)e.OrderStatus,
                OrderNr = e.OrderNr,
                SellerOrderNr = e.SellerOrderNr,

                //Invoice
                InvoiceId = e.InvoiceState == (int)SoeEntityState.Deleted ? 0 : e.InvoiceId,
                InvoiceStatus = (TermGroup_EDIInvoiceStatus)e.InvoiceStatus,
                InvoiceNr = e.InvoiceNr,
                SeqNr = e.SeqNr,

                //Customer
                CustomerId = e.CustomerId,
                CustomerNr = e.CustomerNr,
                CustomerName = e.CustomerName,

                //Supplier
                SupplierId = e.SupplierId,
                SupplierNr = e.SupplierNr,
                SupplierName = e.SupplierName,
            };

            if (dto.Type == TermGroup_EDISourceType.EDI || dto.Type == TermGroup_EDISourceType.Finvoice)
            {
                dto.EdiMessageType = (TermGroup_EdiMessageType)e.MessageType;
                dto.ScanningMessageType = TermGroup_ScanningMessageType.Unknown;
                dto.ScanningMessageTypeName = String.Empty;
            }
            else if (dto.Type == TermGroup_EDISourceType.Scanning)
            {
                dto.EdiMessageType = TermGroup_EdiMessageType.Unknown;
                dto.EdiMessageTypeName = String.Empty;
                dto.ScanningMessageType = (TermGroup_ScanningMessageType)e.MessageType;
            }

            return dto;
        }

        public static EdiEntryViewDTO ToDTO(this ScanningEntryView e)
        {
            if (e == null)
                return null;

            EdiEntryViewDTO dto = new EdiEntryViewDTO()
            {
                //Edi
                EdiEntryId = e.EdiEntryId,
                ActorCompanyId = e.ActorCompanyId,
                Type = (TermGroup_EDISourceType)e.Type,
                Status = (TermGroup_EDIStatus)e.Status,
                BillingType = e.BillingType.HasValue ? (TermGroup_BillingType)e.BillingType : TermGroup_BillingType.None,
                WholesellerId = e.WholeSellerId,
                WholesellerName = e.WholesellerName,
                BuyerId = e.BuyerId,
                BuyerReference = e.BuyerReference,
                HasPdf = e.HasPdf,
                ErrorCode = e.ErrorCode,
                Created = e.Created,
                State = (SoeEntityState)e.State,

                //Scanning
                ScanningEntryId = e.ScanningEntryId,
                OperatorMessage = e.OperatorMessage,

                //Dates
                Date = e.Date,
                InvoiceDate = e.InvoiceDate,
                DueDate = e.DueDate,

                //Sum
                Sum = e.Sum,
                SumCurrency = e.SumCurrency,
                SumVat = e.SumVat,
                SumVatCurrency = e.SumVatCurrency,

                //Currency
                CurrencyId = e.CurrencyId,
                SysCurrencyId = e.SysCurrencyId,
                CurrencyRate = e.CurrencyRate,

                //Order
                OrderId = e.OrderId,
                OrderStatus = (TermGroup_EDIOrderStatus)e.OrderStatus,
                OrderNr = e.OrderNr,
                SellerOrderNr = e.SellerOrderNr,

                //Invoice
                InvoiceId = e.InvoiceId,
                InvoiceStatus = (TermGroup_EDIInvoiceStatus)e.InvoiceStatus,
                InvoiceNr = e.InvoiceNr,
                SeqNr = e.SeqNr,

                //Customer
                CustomerId = e.CustomerId,
                CustomerNr = e.CustomerNr,
                CustomerName = e.CustomerName,

                //Supplier
                SupplierId = e.SupplierId,
                SupplierNr = e.SupplierNr,
                SupplierName = e.SupplierName,
            };

            if (dto.Type == TermGroup_EDISourceType.EDI || dto.Type == TermGroup_EDISourceType.Finvoice)
            {
                dto.EdiMessageType = (TermGroup_EdiMessageType)e.MessageType;
                dto.ScanningMessageType = TermGroup_ScanningMessageType.Unknown;
                dto.ScanningMessageTypeName = String.Empty;
            }
            else if (dto.Type == TermGroup_EDISourceType.Scanning)
            {
                dto.ScanningStatus = (TermGroup_ScanningStatus)e.Status;
                dto.EdiMessageType = TermGroup_EdiMessageType.Unknown;
                dto.EdiMessageTypeName = String.Empty;
                dto.ScanningMessageType = (TermGroup_ScanningMessageType)e.MessageType;
            }

            return dto;
        }

        public static SupplierInvoiceGridDTO ToConvertedScanningSupplierGridDTO(this ScanningEntryView e, bool hideVatWarnings, bool setClosedStyle, bool foreign = false)
        {
            if (e == null)
                return null;

            SupplierInvoiceGridDTO dto = new SupplierInvoiceGridDTO()
            {
                //Edi
                EdiEntryId = e.EdiEntryId,
                EdiType = e.Type,
                //RoundedInterpretation = e.RoundedInterpretation,
                ScanningEntryId = e.ScanningEntryId,
                OperatorMessage = e.OperatorMessage,
                ErrorCode = e.ErrorCode,
                InvoiceStatus = e.InvoiceStatus,
                Created = e.Created,
                //To do, how we handle edi statuses in the grid
                //Type = (int)(TermGroup_EDISourceType)e.Type,
                Status = (int)(TermGroup_EDIStatus)e.Status,
                //StatusName = e.StatusName,
                BillingTypeId = e.BillingType.HasValue ? (int)e.BillingType : (int)TermGroup_BillingType.None,
                HasPDF = e.HasPdf || e.UsesDataStorage,

                //Dates
                InvoiceDate = e.InvoiceDate,
                DueDate = e.DueDate,

                //Sum
                TotalAmount = e.Sum,
                TotalAmountText = e.Sum.ToString(),
                TotalAmountCurrency = e.SumCurrency,
                TotalAmountCurrencyText = e.SumCurrency.ToString(),
                VATAmount = e.SumVat,
                VATAmountCurrency = e.SumVatCurrency,

                //Currency
                SysCurrencyId = e.SysCurrencyId,

                //Invoice
                SupplierInvoiceId = e.InvoiceId.HasValue ? (int)e.InvoiceId : 0,
                //Status = e.InvoiceStatus,
                InvoiceNr = e.InvoiceNr,
                SeqNr = e.SeqNr.HasValue ? e.SeqNr.ToString() : string.Empty,
                VatType = (int)TermGroup_InvoiceVatType.Merchandise,

                //Supplier
                SupplierNr = e.SupplierNr,
                SupplierName = e.SupplierName,
                SupplierId = e.SupplierId ?? 0,
            };

            #region Vat check

            if (!foreign)
            {
                dto.TotalAmountExVat = dto.TotalAmount - dto.VATAmount;
                dto.TotalAmountExVatText = dto.TotalAmountExVat.ToString();
            }

            dto.TotalAmountExVatCurrency = dto.TotalAmountCurrency - dto.VATAmountCurrency;
            dto.TotalAmountExVatCurrencyText = dto.TotalAmountExVatCurrency.ToString();

            #endregion

            #region Vat warning

            if (hideVatWarnings)
                dto.VatRate = 0;
            else
            {
                // TODO: We need cent rounding for the calculation to be correct, but it is not included in the view.
                // This will cause small invoices to be misleading in this validation.
                decimal cent = 0;
                decimal sum = dto.TotalAmountCurrency - dto.VATAmount - cent;
                if (sum != 0)
                    dto.VatRate = Decimal.Round((dto.VATAmount / sum * 100), 1);

                // A workaround is to check the size of the amount.
                if ((dto.VatRate != 0 && dto.VatRate != 25M) && (Math.Abs(Math.Abs(dto.TotalAmountCurrency / 4) - Math.Abs(dto.TotalAmountCurrency * (dto.VATAmount / (dto.TotalAmountCurrency - dto.VATAmount)))) < 1))
                    dto.VatRate = 0;
            }

            #endregion

            #region Closed style - NOT USED AT THE MOMENT

            /*if (setClosedStyle)
            {
                if (e.InvoiceStatus != (int)TermGroup_EDIInvoiceStatus.Unprocessed)
                {
                    dto.UseClosedStyle = true;
                    dto.IsSelectDisabled = true;
                }
            }*/

            #endregion

            //dto.RoundedInterpretation = e.RoundedInterpretation;
            ////dto.SupplierAttestGroupName = e.SupplierAttestGroupName;

            return dto;
        }


        public static SupplierInvoiceGridDTO ToConvertedEdiSupplierGridDTO(this EdiEntryView e, bool hideVatWarnings, bool setClosedStyle, bool foreign = false)
        {
            if (e == null)
                return null;

            var dto = new SupplierInvoiceGridDTO()
            {
                /*SourceTypeName = e.SourceTypeName,
                EdiMessageTypeName = e.MessageTypeName,
                BillingTypeName = e.BillingTypeName,
                CurrencyCode = e.CurrencyCode,
                StatusName = e.InvoiceStatusName,*/
                //Edi
                EdiEntryId = e.EdiEntryId,
                EdiType = e.Type,
                OperatorMessage = e.OperatorMessage,
                ErrorCode = e.ErrorCode,
                ErrorMessage = e.ErrorMessage,
                InvoiceStatus = e.InvoiceStatus,
                Created = e.Created,
                BillingTypeId = e.BillingType.HasValue ? (int)e.BillingType : (int)TermGroup_BillingType.None,
                //RoundedInterpretation = e.RoundedInterpretation,
                HasPDF = e.HasPdf,
                EdiMessageType = e.MessageType,

                //Dates
                InvoiceDate = e.InvoiceDate,
                DueDate = e.DueDate,

                //Sum
                TotalAmount = e.Sum,
                TotalAmountText = e.Sum.ToString(),
                TotalAmountCurrency = e.SumCurrency,
                TotalAmountCurrencyText = e.SumCurrency.ToString(),
                VATAmount = e.SumVat,
                VATAmountCurrency = e.SumVatCurrency,
                //Currency
                SysCurrencyId = e.SysCurrencyId,
                //Invoice
                SupplierInvoiceId = e.InvoiceId.HasValue ? (int)e.InvoiceId : 0,
                Status = e.InvoiceStatus,
                InvoiceNr = e.InvoiceNr,
                SeqNr = e.SeqNr.HasValue && e.SeqNr.Value > 0 ? e.SeqNr.ToString() : string.Empty,
                VatType = (int)TermGroup_InvoiceVatType.Merchandise,
                //Supplier
                SupplierName = e.SupplierNr + " " + e.SupplierName,
                SupplierId = e.SupplierId ?? 0
            };

            #region Vat check
            if (!foreign)
            {
                switch (dto.VatType)
                {
                    case (int)TermGroup_InvoiceVatType.Merchandise:
                        dto.TotalAmountExVat = dto.TotalAmount - dto.VATAmount;
                        dto.TotalAmountExVatText = dto.TotalAmountExVat.ToString();
                        break;
                    default:
                        dto.TotalAmountExVat = dto.TotalAmount - dto.VATAmount;
                        dto.TotalAmountExVatText = dto.TotalAmountExVat.ToString();
                        break;
                }
            }

            dto.TotalAmountExVatCurrency = dto.TotalAmountCurrency - dto.VATAmountCurrency;
            dto.TotalAmountExVatCurrencyText = dto.TotalAmountExVatCurrency.ToString();

            #endregion

            #region Vat warning

            if (hideVatWarnings)
                dto.VatRate = 0;
            else
            {
                // TODO: We need cent rounding for the calculation to be correct, but it is not included in the view.
                // This will cause small invoices to be misleading in this validation.
                decimal cent = 0;
                decimal sum = dto.TotalAmountCurrency - dto.VATAmount - cent;
                if (sum != 0)
                    dto.VatRate = Decimal.Round((dto.VATAmount / sum * 100), 1);

                // A workaround is to check the size of the amount.
                if (dto.VatRate != 0 && dto.VatRate != 25M)
                {
                    if (Math.Abs(Math.Abs(dto.TotalAmountCurrency / 4) - Math.Abs(dto.TotalAmountCurrency * (dto.VATAmount / (dto.TotalAmountCurrency - dto.VATAmount)))) < 1)
                        dto.VatRate = 0;
                }
            }

            #endregion

            #region Closed style

            if (setClosedStyle)
            {
                if (e.InvoiceStatus != (int)TermGroup_EDIInvoiceStatus.Unprocessed)
                {
                    dto.UseClosedStyle = true;
                    dto.IsSelectDisabled = true;
                }
            }

            #endregion

            return dto;
        }

        #endregion

        #region PaymentInformation

        public static PaymentInformationViewDTO ToDTO(this PaymentInformationView e, bool setBic = false)
        {
            if (e == null)
                return null;

            var dto = new PaymentInformationViewDTO()
            {
                ActorId = e.ActorId,
                SysPaymentTypeId = e.SysPaymentTypeId,
                DefaultSysPaymentTypeId = e.DefaultSysPaymentTypeId,
                PaymentInformationRowId = e.PaymentInformationRowId,
                Default = e.Default,
                PaymentNr = e.PaymentNr,
                PaymentNrDisplay = e.PaymentNr,
                CurrencyId = e.CurrencyId,
            };

            if (setBic)
            {
                if (!string.IsNullOrEmpty(e.BIC))
                {
                    dto.PaymentNrDisplay = e.PaymentNr + " (" + e.BIC + ")";
                }
                else if (e.PaymentNr.Contains('/'))
                {
                    var parts = e.PaymentNr.Split('/');
                    dto.PaymentNrDisplay = parts[1] + " (" + parts[0] + ")";
                }
            }

            return dto;
        }
        public static List<PaymentInformationDistributionRowDTO> ToDistributionDTOs(this EntityCollection<PaymentInformation> e)
        {
            var rows = new List<PaymentInformationDistributionRowDTO>();
            if (e == null)
                return rows;

            foreach (var pi in e)
            {
                rows.AddRange(pi.ToDistributionDTOs());
            }

            return rows;
        }

        public static List<PaymentInformationDistributionRowDTO> ToDistributionDTOs(this PaymentInformation e)
        {
            var rows = new List<PaymentInformationDistributionRowDTO>();
            if (e is null || e.PaymentInformationRow is null)
                return rows;

            foreach (var row in e.PaymentInformationRow)
            {
                if (row.State == 0)
                    rows.Add(new PaymentInformationDistributionRowDTO
                    {
                        SysPaymentTypeId = (TermGroup_SysPaymentType)row.SysPaymentTypeId,
                        PaymentNr = row.PaymentNr,
                        BIC = row.BIC,
                        Default = row.Default
                    });
            }
            return rows;
        }

        #endregion

        #region PriceRule

        public static CompanyPriceRuleDTO ToDTO(this GetCompanyPriceRules_Result e)
        {
            if (e == null)
                return null;

            var dto = new CompanyPriceRuleDTO
            {
                RuleId = e.RuleId,
                Date = DateTime.Today,
                PriceListTypeId = e.PriceListTypeId,
                CompanyWholesellerPriceListId = e.CompanyWholesellerPriceListId,
                RuleCompanyWholesellerPriceListId = e.RuleCompanyWholeSellerPriceListId,
                //SysWholesellerName = e.SysWholeSellerName,
                PriceListTypeName = e.PriceListTypeName,
                PriceListTypeDescription = e.PriceListTypeDescription,
                PriceListImportedHeadId = e.PricelistImportedHeadId,
            };

            return dto;
        }

        public static IEnumerable<CompanyPriceRuleDTO> ToDTOs(this IEnumerable<GetCompanyPriceRules_Result> l)
        {
            var dtos = new List<CompanyPriceRuleDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        #endregion

        #region Product

        #region InvoiceProductPriceSearchView

        public static CompanyWholesellerPriceListViewDTO ToDTO(this CompanyWholeSellerPriceListUsedView e)
        {
            if (e == null)
                return null;

            var priceListOrigin = e.PriceListOrigin.HasValue && Enum.IsDefined(typeof(PriceListOrigin), e.PriceListOrigin.Value) ? (PriceListOrigin)e.PriceListOrigin.Value : PriceListOrigin.Unknown;
            var dto = new CompanyWholesellerPriceListViewDTO
            {
                ActorCompanyId = e.ActorCompanyId,
                IsUsed = true,
                SysPriceListHeadId = e.SysPriceListHeadId,
                SysWholesellerId = e.SysWholesellerId,
                CompanyWholesellerPriceListId = e.CompanyWholesellerPriceListId,
                PriceListOrigin = priceListOrigin,
                Version = e.Version,
                PriceListImportedHeadId = priceListOrigin == PriceListOrigin.CompDbPriceList ? e.SysPriceListHeadId.ToNullable() : null,
                Date = e.Date ?? DateTime.MinValue,
                Provider = e.Provider
            };

            return dto;
        }

        public static IEnumerable<CompanyWholesellerPriceListViewDTO> ToDTOs(this IEnumerable<CompanyWholeSellerPriceListUsedView> l)
        {
            var dtos = new List<CompanyWholesellerPriceListViewDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static InvoiceProductPriceSearchViewDTO ToDTO(this PriceListImportedPriceSearchView e)
        {
            if (e == null)
                return null;

            var dto = new InvoiceProductPriceSearchViewDTO
            {
                CompanyWholesellerPriceListId = 0,
                SysPriceListHeadId = e.PriceListImportedHeadId,
                ProductId = e.ProductId,
                Number = e.ProductNumber,
                Name = e.Name,
                Code = e.ProductCode,
                GNP = e.GNP,
                PriceStatus = e.PriceStatus,
                SysWholesellerId = e.SysWholesellerId,
                PriceListType = "",
                PriceListOrigin = e.PriceListOrigin,
                PurchaseUnit = e.PurchaseUnit,
                SalesUnit = e.SalesUnit,
                ProductProviderType = (SoeSysPriceListProviderType)e.SysPriceListProviderType,
                ProductType = e.SysPriceListProviderType,
                Type = e.SysPriceListProviderType
            };

            /*
            // Extensions
            dto.PriceFormula = e.PriceFormula;
            dto.MarginalIncome = e.MarginalIncome;
            dto.MarginalIncomeRatio = e.MarginalIncomeRatio;
            */
            return dto;
        }

        public static IEnumerable<InvoiceProductPriceSearchViewDTO> ToDTOs(this IEnumerable<PriceListImportedPriceSearchView> l)
        {
            var dtos = new List<InvoiceProductPriceSearchViewDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }


        public static InvoiceProductPriceSearchViewDTO ToDTO(this SysProductPriceSearchView e)
        {
            if (e == null)
                return null;

            var dto = new InvoiceProductPriceSearchViewDTO
            {
                SysPriceListHeadId = e.SysPriceListHeadId,
                ProductId = e.ProductId,
                Number = e.ProductNumber,
                Name = e.Name,
                Code = e.ProductCode,
                GNP = e.GNP,
                //CustomerPrice = e.CustomerPrice,
                PriceStatus = e.PriceStatus,
                SysWholesellerId = e.SysWholesellerId,
                Wholeseller = e.Wholeseller,
                PriceListType = "",
                PriceListOrigin = e.PriceListOrigin,
                PurchaseUnit = e.PurchaseUnit,
                SalesUnit = e.SalesUnit,
                ProductProviderType = (SoeSysPriceListProviderType)e.SysPriceListProviderType,
                ProductType = e.SysPriceListProviderType,
                Type = e.SysPriceListProviderType,
                SalesPrice = e.SalesPrice,
                NettoNettoPrice = e.NetPrice
            };

            return dto;
        }

        public static IEnumerable<InvoiceProductPriceSearchViewDTO> ToDTOs(this IEnumerable<SysProductPriceSearchView> l)
        {
            var dtos = new List<InvoiceProductPriceSearchViewDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        #endregion

        #endregion

        #region TraceViews

        #region Contract

        public static ContractTraceViewDTO ToDTO(this ContractTraceView e)
        {
            if (e == null)
                return null;

            ContractTraceViewDTO dto = new ContractTraceViewDTO()
            {
                ContractId = e.ContractId,
                IsOrder = e.IsOrder,
                OrderId = e.OrderId,
                IsInvoice = e.IsInvoice,
                InvoiceId = e.InvoiceId,
                IsProject = e.IsProject,
                ProjectId = e.ProjectId,
                OriginType = (SoeOriginType)e.OriginType,
                OriginStatus = (SoeOriginStatus)e.OriginStatus,
                Description = e.Description,
                BillingType = (TermGroup_BillingType)e.BillingType,
                Number = e.Number,
                SysCurrencyId = e.SysCurrencyId,
                CurrencyRate = e.CurrencyRate,
                Amount = e.Amount,
                AmountCurrency = e.AmountCurrency,
                VatAmount = e.VatAmount,
                VatAmountCurrency = e.VatAmountCurrency,
                Date = e.Date,
                State = (SoeEntityState)e.State
            };

            // Extensions
            dto.Foreign = e.Foreign;

            return dto;
        }

        public static IEnumerable<ContractTraceViewDTO> ToDTOs(this IEnumerable<ContractTraceView> l)
        {
            var dtos = new List<ContractTraceViewDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        #endregion

        #region Inventory

        public static InventoryTraceViewDTO ToDTO(this InventoryTraceView e)
        {
            if (e == null)
                return null;

            InventoryTraceViewDTO dto = new InventoryTraceViewDTO()
            {
                InventoryId = e.InventoryId,
                InventoryLogId = e.InventoryLogId,
                VoucherHeadId = e.VoucherHeadId,
                VoucherNr = e.VoucherNr,
                InvoiceId = e.InvoiceId,
                InvoiceNr = e.InvoiceNr,
                AccountDistributionEntryId = e.AccountDistributionEntryId,
                Type = (TermGroup_InventoryLogType)e.Type,
                Date = e.Date,
                Amount = e.Amount
            };

            return dto;
        }

        public static IEnumerable<InventoryTraceViewDTO> ToDTOs(this IEnumerable<InventoryTraceView> l)
        {
            var dtos = new List<InventoryTraceViewDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        #endregion

        #region Invoice

        public static InvoiceTraceViewDTO ToDTO(this InvoiceTraceView e)
        {
            if (e == null)
                return null;

            InvoiceTraceViewDTO dto = new InvoiceTraceViewDTO()
            {
                //LangId = e.LangId,
                IsInvoice = e.IsInvoice,
                InvoiceId = e.InvoiceId,
                IsContract = e.IsContract,
                ContractId = e.ContractId,
                IsOffer = e.IsOffer,
                OfferId = e.OfferId,
                IsOrder = e.IsOrder,
                OrderId = e.OrderId,
                MappedInvoiceId = e.MappedInvoiceId,
                IsReminderInvoice = e.IsReminderInvoice,
                ReminderInvoiceId = e.ReminderInvoiceId,
                IsInterestInvoice = e.IsInterestInvoice,
                InterestInvoiceId = e.InterestInvoiceId,
                IsPayment = e.IsPayment,
                PaymentRowId = e.PaymentRowId,
                PaymentStatusId = (SoePaymentStatus)e.PaymentStatusId,
                IsEdi = e.IsEdi,
                EdiEntryId = e.EdiEntryId,
                EdiHasPdf = e.EdiHasPdf,
                IsVoucher = e.IsVoucher,
                VoucherHeadId = e.VoucherHeadId,
                IsInventory = e.IsInventory,
                InventoryId = e.InventoryId,
                InventoryName = e.InventoryName,
                InventoryDescription = e.InventoryDescription,
                InventoryStatusId = (TermGroup_InventoryStatus)e.InventoryStatusId,
                IsAccountDistribution = e.IsAccountDistribution,
                AccountDistributionHeadId = e.AccountDistributionHeadId,
                AccountDistributionName = e.AccountDistributionName,
                IsProject = e.IsProject,
                ProjectId = e.ProjectId,
                OriginType = (SoeOriginType)e.OriginType,
                OriginStatus = (SoeOriginStatus)e.OriginStatus,
                Description = e.Description,
                BillingType = (TermGroup_BillingType)e.BillingType,
                Number = e.Number,
                SysCurrencyId = e.SysCurrencyId,
                CurrencyRate = e.CurrencyRate,
                Amount = e.Amount ?? 0,
                AmountCurrency = e.AmountCurrency ?? 0,
                VatAmount = e.VatAmount,
                VatAmountCurrency = e.VatAmountCurrency,
                State = (SoeEntityState)e.State,
                TriggerType = e.TriggerType,
                IsStockVoucher = e.IsStockVoucher,
            };

            // Extensions
            dto.Foreign = e.Foreign;
            dto.Date = e.Date.HasValue ? e.Date : null;

            return dto;
        }

        public static IEnumerable<InvoiceTraceViewDTO> ToDTOs(this IEnumerable<InvoiceTraceView> l)
        {
            var dtos = new List<InvoiceTraceViewDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        #endregion

        #region Offer

        public static OfferTraceViewDTO ToDTO(this OfferTraceView e)
        {
            if (e == null)
                return null;

            OfferTraceViewDTO dto = new OfferTraceViewDTO()
            {
                OfferId = e.OfferId,
                IsOrder = e.IsOrder,
                OrderId = e.OrderId,
                IsInvoice = e.IsInvoice,
                InvoiceId = e.InvoiceId,
                IsProject = e.IsProject,
                ProjectId = e.ProjectId,
                OriginType = (SoeOriginType)e.OriginType,
                OriginStatus = (SoeOriginStatus)e.OriginStatus,
                Description = e.Description,
                BillingType = (TermGroup_BillingType)e.BillingType,
                Number = e.Number,
                SysCurrencyId = e.SysCurrencyId,
                CurrencyRate = e.CurrencyRate,
                Amount = e.Amount,
                AmountCurrency = e.AmountCurrency,
                VatAmount = e.VatAmount,
                VatAmountCurrency = e.VatAmountCurrency,
                Date = e.Date,
                State = (SoeEntityState)e.State
            };

            // Extensions
            dto.Foreign = e.Foreign;

            return dto;
        }

        public static IEnumerable<OfferTraceViewDTO> ToDTOs(this IEnumerable<OfferTraceView> l)
        {
            var dtos = new List<OfferTraceViewDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        #endregion

        #region Order

        public static OrderTraceViewDTO ToDTO(this OrderTraceView e)
        {
            if (e == null)
                return null;

            OrderTraceViewDTO dto = new OrderTraceViewDTO()
            {
                //LangId = e.LangId,
                //OriginTypeName = e.OriginTypeName,
                //OriginStatusName = e.OriginStatusName,
                //BillingTypeName = e.BillingTypeName,
                //CurrencyCode = e.CurrencyCode,
                OrderId = e.OrderId,
                IsContract = e.IsContract,
                ContractId = e.ContractId,
                IsOffer = e.IsOffer,
                OfferId = e.OfferId,
                IsInvoice = e.IsInvoice,
                InvoiceId = e.InvoiceId,
                IsStockVoucher = e.IsStockVoucher,
                VoucherHeadId = e.VoucherHeadId,
                IsEdi = e.IsEdi,
                EdiEntryId = e.EdiEntryId,
                EdiHasPdf = e.EdiHasPdf,
                IsProject = e.IsProject,
                ProjectId = e.ProjectId,
                IsSupplierInvoice = e.IsSupplierInvoice,
                SupplierInvoiceId = e.SupplierInvoiceId,
                IsPurchase = e.IsPurchaseRow,
                PurchaseId = e.PurchaseRowId,
                OriginType = (SoeOriginType)e.OriginType,
                OriginStatus = (SoeOriginStatus)e.OriginStatus,
                Description = e.Description,
                BillingType = (TermGroup_BillingType)e.BillingType,
                Number = e.Number,
                SysCurrencyId = e.SysCurrencyId,
                CurrencyRate = e.CurrencyRate,
                Amount = e.Amount,
                AmountCurrency = e.AmountCurrency,
                VatAmount = e.VatAmount,
                VatAmountCurrency = e.VatAmountCurrency,
                Date = e.Date,
                State = (SoeEntityState)e.State
            };

            // Extensions
            dto.Foreign = e.Foreign;

            return dto;
        }

        public static IEnumerable<OrderTraceViewDTO> ToDTOs(this IEnumerable<OrderTraceView> l)
        {
            var dtos = new List<OrderTraceViewDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }


        #endregion

        #region Payment

        public static PaymentTraceViewDTO ToDTO(this PaymentTraceView e)
        {
            if (e == null)
                return null;

            PaymentTraceViewDTO dto = new PaymentTraceViewDTO()
            {
                PaymentRowId = e.PaymentRowId,
                IsInvoice = e.IsInvoice,
                InvoiceId = e.InvoiceId,
                IsVoucher = e.IsVoucher,
                VoucherHeadId = e.VoucherHeadId,
                IsProject = e.IsProject,
                ProjectId = e.ProjectId,
                IsImport = e.IsImport,
                PaymentImportId = e.PaymentImportId,
                OriginType = (SoeOriginType)e.OriginType,
                OriginStatus = (SoeOriginStatus)e.OriginStatus,
                Description = e.Description,
                Number = e.Number,
                SequenceNumber = e.SequenceNumber,
                SysCurrencyId = e.SysCurrencyId,
                CurrencyRate = e.CurrencyRate,
                Amount = e.Amount ?? 0,
                AmountCurrency = e.AmountCurrency ?? 0,
                VatAmount = e.VatAmount,
                VatAmountCurrency = e.VatAmountCurrency,
                Date = e.Date,
                State = (SoeEntityState)e.State,
                RegistrationType = (OrderInvoiceRegistrationType)e.RegistrationType,
            };

            // Extensions
            dto.Foreign = e.Foreign;

            return dto;
        }

        public static IEnumerable<PaymentTraceViewDTO> ToDTOs(this IEnumerable<PaymentTraceView> l)
        {
            var dtos = new List<PaymentTraceViewDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        #endregion

        #region Project

        public static ProjectTraceViewDTO ToDTO(this ProjectTraceView e)
        {
            if (e == null)
                return null;

            ProjectTraceViewDTO dto = new ProjectTraceViewDTO()
            {
                /*PaymentStatusName = e.PaymentStatusName,
                OriginTypeName = e.OriginTypeName,
                OriginStatusName = e.OriginStatusName,
                BillingTypeName = e.BillingTypeName,
                CurrencyCode = e.CurrencyCode,*/
                ProjectId = e.ProjectId,
                IsContract = e.IsContract,
                ContractId = e.ContractId,
                IsOffer = e.IsOffer,
                OfferId = e.OfferId,
                IsOrder = e.IsOrder,
                OrderId = e.OrderId,
                IsCustomerInvoice = e.IsCustomerInvoice,
                CustomerInvoiceId = e.CustomerInvoiceId,
                IsSupplierInvoice = e.IsSupplierInvoice,
                SupplierInvoiceId = e.SupplierInvoiceId,
                IsPayment = e.IsPayment,
                PaymentRowId = e.PaymentRowId,
                PaymentStatusId = (SoePaymentStatus)e.PaymentStatusId,
                OriginType = (SoeOriginType)e.OriginType,
                OriginStatus = (SoeOriginStatus)e.OriginStatus,
                Description = e.Description,
                BillingType = (TermGroup_BillingType)e.BillingType,
                Number = e.Number,
                SysCurrencyId = e.SysCurrencyId,
                CurrencyRate = e.CurrencyRate,
                Amount = e.Amount,
                AmountCurrency = e.AmountCurrency,
                VatAmount = e.VatAmount,
                //VatAmountCurrency = e.VatAmountCurrency,
                Date = e.Date,
                State = (SoeEntityState)e.State
            };

            return dto;
        }

        public static IEnumerable<ProjectTraceViewDTO> ToDTOs(this IEnumerable<ProjectTraceView> l)
        {
            var dtos = new List<ProjectTraceViewDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        #endregion

        #region Purchase



        public static PurchaseTraceViewDTO ToDTO(this PurchaseTraceView e)
        {
            if (e == null)
                return null;

            PurchaseTraceViewDTO dto = new PurchaseTraceViewDTO()
            {
                PurchaseId = e.PurchaseId,
                IsOrder = e.IsOrder,
                OrderId = e.OrderId,
                IsDelivery = e.IsDelivery,
                PurchaseDeliveryId = e.PurchaseDeliveryId,
                OriginStatus = (SoeOriginStatus)e.OriginStatus,
                Description = e.Description,
                Number = e.Number,
                Date = e.Date,
                State = (SoeEntityState)e.State
            };

            return dto;
        }

        public static IEnumerable<PurchaseTraceViewDTO> ToDTOs(this IEnumerable<PurchaseTraceView> l)
        {
            var dtos = new List<PurchaseTraceViewDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        #endregion

        #region Voucher

        public static VoucherTraceViewDTO ToDTO(this VoucherTraceView e)
        {
            if (e == null)
                return null;

            VoucherTraceViewDTO dto = new VoucherTraceViewDTO()
            {
                //LangId = e.LangId,
                VoucherHeadId = e.VoucherHeadId,
                IsInvoice = e.IsInvoice,
                InvoiceId = e.InvoiceId,
                IsPayment = e.IsPayment,
                PaymentRowId = e.PaymentRowId,
                PaymentStatus = (SoePaymentStatus)e.PaymentStatus,
                //PaymentStatusName = e.PaymentStatusName,
                IsInventory = e.IsInventory,
                InventoryId = e.InventoryId,
                InventoryName = e.InventoryName,
                InventoryDescription = e.InventoryDescription,
                //InventoryTypeName = e.InventoryTypeName,
                InventoryStatusId = (TermGroup_InventoryStatus)e.InventoryStatusId,
                //InventoryStatusName = e.InventoryStatusName,
                IsAccountDistribution = e.IsAccountDistribution,
                AccountDistributionHeadId = e.AccountDistributionHeadId,
                AccountDistributionName = e.AccountDistributionName,
                OriginType = (SoeOriginType)e.OriginType,
                //OriginTypeName = e.OriginTypeName,
                OriginStatus = (SoeOriginStatus)e.OriginStatus,
                //OriginStatusName = e.OriginStatusName,
                Description = e.Description,
                Number = e.Number,
                SysCurrencyId = e.SysCurrencyId,
                //CurrencyCode = e.CurrencyCode,
                CurrencyRate = e.CurrencyRate ?? 1,
                Amount = e.Amount,
                AmountCurrency = e.AmountCurrency ?? 0,
                VatAmount = e.VatAmount,
                VatAmountCurrency = e.VatAmountCurrency,
                Date = e.Date,
                State = (SoeEntityState)e.State,
                RegistrationType = (OrderInvoiceRegistrationType)e.RegistrationType,
            };

            // Extensions
            dto.Foreign = e.Foreign;

            return dto;
        }

        public static IEnumerable<VoucherTraceViewDTO> ToDTOs(this IEnumerable<VoucherTraceView> l)
        {
            var dtos = new List<VoucherTraceViewDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        #endregion

        #endregion

        #endregion

        #region Stored procedures

        #region CustomerInvoiceSearch

        public static CustomerInvoiceSearchResultDTO ToDTO(this CustomerInvoiceSearchResult e)
        {
            if (e == null)
                return null;

            CustomerInvoiceSearchResultDTO dto = new CustomerInvoiceSearchResultDTO()
            {
                InvoiceId = e.InvoiceId,
                InvoiceNr = e.InvoiceNr,
                SeqNr = e.SeqNr,
                CustomerNr = e.CustomerNr,
                CustomerName = e.CustomerName
            };

            return dto;
        }

        public static IEnumerable<CustomerInvoiceSearchResultDTO> ToDTOs(this IEnumerable<CustomerInvoiceSearchResult> l)
        {
            var dtos = new List<CustomerInvoiceSearchResultDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        #endregion

        #region EDI/Scanning

        public static EdiEntryViewDTO ToDTO(this GetEdiEntrysResult e)
        {
            if (e == null)
                return null;

            EdiEntryViewDTO dto = new EdiEntryViewDTO()
            {
                /*StatusName = e.StatusName,
                SourceTypeName = e.SourceTypeName,
                BillingTypeName = e.BillingTypeName,
                CurrencyCode = e.CurrencyCode,
                OrderStatusName = e.OrderStatusName,
                InvoiceStatusName = e.InvoiceStatusName,
                dto.EdiMessageTypeName = e.MessageTypeName;
                LangId = e.LangId,*/

                //Edi
                EdiEntryId = e.EdiEntryId,
                ActorCompanyId = e.ActorCompanyId,
                Type = (TermGroup_EDISourceType)e.Type,
                Status = (TermGroup_EDIStatus)e.Status,
                WholesellerId = e.WholeSellerId,
                WholesellerName = e.WholesellerName,
                BuyerId = e.BuyerId,
                BuyerReference = e.BuyerReference,
                BillingType = e.BillingType.HasValue ? (TermGroup_BillingType)e.BillingType : TermGroup_BillingType.None,
                HasPdf = e.HasPdf,
                ErrorCode = e.ErrorCode,
                Created = e.Created,
                State = (SoeEntityState)e.State,
                ErrorMessage = e.ErrorMessage,

                //Scanning
                ScanningEntryId = e.ScanningEntryId,
                OperatorMessage = e.OperatorMessage,

                //Dates
                Date = e.Date,
                InvoiceDate = e.InvoiceDate,
                DueDate = e.DueDate,

                //Currency
                CurrencyId = e.CurrencyId,
                SysCurrencyId = e.SysCurrencyId,
                CurrencyRate = e.CurrencyRate,

                //Order
                OrderId = e.OrderId,
                OrderStatus = (TermGroup_EDIOrderStatus)e.OrderStatus,
                OrderNr = e.OrderNr,
                SellerOrderNr = e.SellerOrderNr,

                //Invoice
                InvoiceId = e.InvoiceId,
                InvoiceStatus = (TermGroup_EDIInvoiceStatus)e.InvoiceStatus,
                InvoiceNr = e.InvoiceNr,
                SeqNr = e.SeqNr,

                //Customer
                CustomerId = e.CustomerId,
                CustomerNr = e.CustomerNr,
                CustomerName = e.CustomerName,

                //Supplier
                SupplierId = e.SupplierId,
                SupplierNr = e.SupplierNr,
                SupplierName = e.SupplierName,
            };

            //Sum
            if (dto.BillingType == TermGroup_BillingType.Credit && e.Sum > 0)
            {
                dto.Sum = Decimal.Negate(e.Sum);
                dto.SumCurrency = Decimal.Negate(e.SumCurrency);
                dto.SumVat = Decimal.Negate(e.SumVat);
                dto.SumVatCurrency = Decimal.Negate(e.SumVatCurrency);
            }
            else
            {
                dto.Sum = e.Sum;
                dto.SumCurrency = e.SumCurrency;
                dto.SumVat = e.SumVat;
                dto.SumVatCurrency = e.SumVatCurrency;
            }

            if (dto.Type == TermGroup_EDISourceType.EDI || dto.Type == TermGroup_EDISourceType.Finvoice)
            {
                dto.EdiMessageType = (TermGroup_EdiMessageType)e.MessageType;
                dto.ScanningMessageType = TermGroup_ScanningMessageType.Unknown;
                dto.ScanningMessageTypeName = String.Empty;
            }
            else if (dto.Type == TermGroup_EDISourceType.Scanning)
            {
                dto.EdiMessageType = TermGroup_EdiMessageType.Unknown;
                dto.EdiMessageTypeName = String.Empty;
                dto.ScanningMessageType = (TermGroup_ScanningMessageType)e.MessageType;
            }

            // Extensions
            dto.RoundedInterpretation = e.RoundedInterpretation;
            dto.SupplierAttestGroupId = e.SupplierAttestGroupId;
            dto.SupplierAttestGroupName = e.SupplierAttestGroupName;

            return dto;
        }

        public static IEnumerable<EdiEntryViewDTO> ToDTOs(this IEnumerable<GetEdiEntrysResult> l)
        {
            var dtos = new List<EdiEntryViewDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }
        public static EdiEntryViewDTO ToDTO(this FinvoiceEntryView e)
        {
            if (e == null)
                return null;

            EdiEntryViewDTO dto = new EdiEntryViewDTO()
            {
                /*StatusName = e.StatusName,
                SourceTypeName = e.SourceTypeName,
                BillingTypeName = e.BillingTypeName,
                CurrencyCode = e.CurrencyCode,
                OrderStatusName = e.OrderStatusName,
                InvoiceStatusName = e.InvoiceStatusName,
                dto.EdiMessageTypeName = e.MessageTypeName;
                LangId = e.LangId,*/

                //Edi
                EdiEntryId = e.EdiEntryId,
                ActorCompanyId = e.ActorCompanyId,
                Type = (TermGroup_EDISourceType)e.Type,
                Status = (TermGroup_EDIStatus)e.Status,
                WholesellerId = e.WholeSellerId,
                WholesellerName = e.WholesellerName,
                BuyerId = e.BuyerId,
                BuyerReference = e.BuyerReference,
                BillingType = e.BillingType.HasValue ? (TermGroup_BillingType)e.BillingType : TermGroup_BillingType.None,
                HasPdf = e.HasPdf,
                ErrorCode = e.ErrorCode,
                Created = e.Created,
                State = (SoeEntityState)e.State,
                ErrorMessage = e.ErrorMessage,
                ImportSource = e.Source.HasValue ? (EdiImportSource)e.Source : EdiImportSource.Undefined,

                //Scanning
                ScanningEntryId = e.ScanningEntryId,
                OperatorMessage = e.OperatorMessage,

                //Dates
                Date = e.Date,
                InvoiceDate = e.InvoiceDate,
                DueDate = e.DueDate,

                //Currency
                CurrencyId = e.CurrencyId,
                SysCurrencyId = e.SysCurrencyId,
                CurrencyRate = e.CurrencyRate,

                //Order
                OrderId = e.OrderId,
                OrderStatus = (TermGroup_EDIOrderStatus)e.OrderStatus,
                OrderNr = e.OrderNr,
                SellerOrderNr = e.SellerOrderNr,

                //Invoice
                InvoiceId = e.InvoiceId,
                InvoiceStatus = (TermGroup_EDIInvoiceStatus)e.InvoiceStatus,
                InvoiceNr = e.InvoiceNr,
                SeqNr = e.SeqNr,

                //Customer
                CustomerId = e.CustomerId,
                CustomerNr = e.CustomerNr,
                CustomerName = e.CustomerName,

                //Supplier
                SupplierId = e.SupplierId,
                SupplierNr = e.SupplierNr,
                SupplierName = e.SupplierName,
            };

            //Sum
            if (dto.BillingType == TermGroup_BillingType.Credit && e.Sum > 0)
            {
                dto.Sum = Decimal.Negate(e.Sum);
                dto.SumCurrency = Decimal.Negate(e.SumCurrency);
                dto.SumVat = Decimal.Negate(e.SumVat);
                dto.SumVatCurrency = Decimal.Negate(e.SumVatCurrency);
            }
            else
            {
                dto.Sum = e.Sum;
                dto.SumCurrency = e.SumCurrency;
                dto.SumVat = e.SumVat;
                dto.SumVatCurrency = e.SumVatCurrency;
            }

            if (dto.Type == TermGroup_EDISourceType.EDI || dto.Type == TermGroup_EDISourceType.Finvoice)
            {
                dto.EdiMessageType = (TermGroup_EdiMessageType)e.MessageType;
                dto.ScanningMessageType = TermGroup_ScanningMessageType.Unknown;
                dto.ScanningMessageTypeName = String.Empty;
            }
            else if (dto.Type == TermGroup_EDISourceType.Scanning)
            {
                dto.EdiMessageType = TermGroup_EdiMessageType.Unknown;
                dto.EdiMessageTypeName = String.Empty;
                dto.ScanningMessageType = (TermGroup_ScanningMessageType)e.MessageType;
            }

            return dto;
        }

        #endregion

        #region GetAttestTransitionLogsForEmployeeResult

        public static List<AttestTransitionLogDTO> ToDTOs(this List<GetAttestTransitionLogsForEmployeeResult> l)
        {
            var dtos = new List<AttestTransitionLogDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static AttestTransitionLogDTO ToDTO(this GetAttestTransitionLogsForEmployeeResult e)
        {
            return new AttestTransitionLogDTO
            {
                TimePayrollTransactionId = e.TimePayrollTransactionId,
                AttestTransitionLogId = e.AttestTransitionLogId,
                AttestStateFromName = e.AttestStateFromName,
                AttestStateToName = e.AttestStateToName,
                AttestTransitionDate = e.AttestTransitionLogDate,
                AttestTransitionUserId = e.AttestTransitionUserId,
                AttestTransitionUserName = e.AttestTransitionLogUserName,
                AttestTransitionCreatedBySupport = e.AttestTransitionCreatedBySupport,
            };
        }

        public static List<AttestTransitionLogDTO> GetAttestTransitionLogs(this List<GetAttestTransitionLogsForEmployeeResult> l, int timePayrollTransactionId)
        {
            return l.Where(i => i.TimePayrollTransactionId == timePayrollTransactionId).OrderByDescending(i => i.AttestTransitionLogDate).ToList().ToDTOs();
        }

        #endregion

        #region GetTimePayrollTransactionsForEmployee_Result

        public static AttestPayrollTransactionDTO CreateTransactionItem(this GetTimePayrollTransactionsForEmployee_Result e, List<AccountDTO> accountInternals, AccountDTO accountStd, List<AccountDimDTO> accountDims, bool hasInfo = false, bool ignoreAccounting = false)
        {
            var transactionItem = new AttestPayrollTransactionDTO()
            {
                //TimePayrollTransaction
                EmployeeId = e.EmployeeId,
                TimePayrollTransactionId = e.TimePayrollTransactionId,
                Quantity = e.Quantity,
                UnitPrice = e.UnitPrice,
                UnitPriceCurrency = e.UnitPriceCurrency,
                UnitPriceEntCurrency = e.UnitPriceEntCurrency,
                Amount = e.Amount,
                AmountCurrency = e.AmountCurrency,
                AmountEntCurrency = e.AmountEntCurrency,
                VatAmount = e.VatAmount,
                VatAmountCurrency = e.VatAmountCurrency,
                VatAmountEntCurrency = e.VatAmountEntCurrency,
                ReversedDate = e.ReversedDate,
                IsReversed = e.IsReversed,
                IsPreliminary = e.IsPreliminary,
                IsExported = e.Exported,
                Comment = String.IsNullOrEmpty(e.TransactionComment) ? e.DeviationComment : e.TransactionComment,
                HasInfo = hasInfo,
                IsAdded = e.IsAdded,
                ManuallyAdded = e.ManuallyAdded,
                AddedDateFrom = e.AddedDateFrom,
                AddedDateTo = e.AddedDateTo,
                IsFixed = e.IsFixed,
                IsSpecifiedUnitPrice = e.IsSpecifiedUnitPrice,
                IsAdditionOrDeduction = e.IsAdditionOrDeduction,
                IsCentRounding = e.IsCentRounding,
                IsQuantityRounding = e.IsQuantityRounding,
                IncludedInPayrollProductChain = e.IncludedInPayrollProductChain,
                ParentId = e.ParentId,
                UnionFeeId = e.UnionFeeId,
                EmployeeVehicleId = e.EmployeeVehicleId,
                EarningTimeAccumulatorId = e.EarningTimeAccumulatorId,
                EmployeeChildId = e.EmployeeChildId,
                PayrollStartValueRowId = e.PayrollStartValueRowId,
                RetroactivePayrollOutcomeId = e.RetroactivePayrollOutcomeId,
                VacationYearEndRowId = e.VacationYearEndRowId,
                IsVacationFiveDaysPerWeek = e.IsVacationFiveDaysPerWeek,
                TransactionSysPayrollTypeLevel1 = e.TransactionSysPayrollTypeLevel1,
                TransactionSysPayrollTypeLevel2 = e.TransactionSysPayrollTypeLevel2,
                TransactionSysPayrollTypeLevel3 = e.TransactionSysPayrollTypeLevel3,
                TransactionSysPayrollTypeLevel4 = e.TransactionSysPayrollTypeLevel4,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,

                //TimePayrollTransactionExtended
                PayrollPriceFormulaId = e.PayrollPriceFormulaId,
                PayrollPriceTypeId = e.PayrollPriceTypeId,
                Formula = e.Formula,
                FormulaPlain = e.FormulaPlain,
                FormulaExtracted = e.FormulaExtracted,
                FormulaNames = e.FormulaNames,
                FormulaOrigin = e.FormulaOrigin,
                PayrollCalculationPerformed = e.PayrollCalculationPerformed,
                TimeUnit = e.TimeUnit ?? (int)TermGroup_PayrollProductTimeUnit.Hours,
                QuantityWorkDays = e.QuantityWorkDays ?? 0,
                QuantityCalendarDays = e.QuantityCalendarDays ?? 0,
                CalenderDayFactor = e.CalenderDayFactor ?? 0,
                IsDistributed = e.IsDistributed ?? false,

                //TimePayrollScheduleTransaction
                IsScheduleTransaction = false,

                //PayrollProduct
                PayrollProductId = e.ProductId,
                PayrollProductNumber = e.ProductNumber,
                PayrollProductName = e.ProductName,
                PayrollProductShortName = e.ProductShortName,
                PayrollProductFactor = e.PayrollProductFactor,
                PayrollProductPayed = e.PayrollProductPayed,
                PayrollProductExport = e.PayrollProductExport,
                PayrollProductUseInPayroll = e.PayrollProductUseInPayroll,
                PayrollProductSysPayrollTypeLevel1 = e.PayrollProductSysPayrollTypeLevel1,
                PayrollProductSysPayrollTypeLevel2 = e.PayrollProductSysPayrollTypeLevel2,
                PayrollProductSysPayrollTypeLevel3 = e.PayrollProductSysPayrollTypeLevel3,
                PayrollProductSysPayrollTypeLevel4 = e.PayrollProductSysPayrollTypeLevel4,
                IsAverageCalculated = e.AverageCalculated,

                //TimeCodeTransaction
                TimeCodeTransactionId = e.TimeCodeTransactionId,
                StartTime = e.StartTime,
                StopTime = e.StopTime,

                //TimeCode
                TimeCodeType = (SoeTimeCodeType)e.TimeCodeType,
                TimeCodeRegistrationType = (TermGroup_TimeCodeRegistrationType)e.TimeCodeRegistrationType,
                NoOfPresenceWorkOutsideScheduleTime = e.NoOfPresenceWorkOutsideScheduleTime,
                NoOfAbsenceAbsenceTime = e.NoOfAbsenceAbsenceTime,

                //TimeBlockDate
                TimeBlockDateId = e.TimeBlockDateId,
                Date = e.Date,

                //TimeBlock
                TimeBlockId = e.TimeBlockId,

                //AttestState
                AttestStateId = e.AttestStateId,
                AttestStateName = e.AttestStateName,
                AttestStateColor = e.AttestStateColor,
                AttestStateInitial = e.AttestStateInitial,
                AttestStateSort = e.AttestStateSort,
                HasSameAttestState = true,

                //Accounting
                AccountDims = accountDims,
                AccountStdId = accountStd?.AccountId ?? 0,
                AccountStd = accountStd,
                AccountInternalIds = accountInternals?.Select(a => a.AccountId).ToList() ?? new List<int>(),
                AccountInternals = accountInternals ?? new List<AccountDTO>(),

                //TimePeriod
                TimePeriodId = e.TimePeriodId,
                TimePeriodName = e.TimePeriodName,

                //Invoiced time
                InvoiceQuantity = e.InvoiceQuantity,
            };

            if (!ignoreAccounting)
                transactionItem.SetAccountingStrings();

            return transactionItem;
        }

        public static List<GetTimePayrollTransactionsForEmployee_Result> Filter(this List<GetTimePayrollTransactionsForEmployee_Result> l, int timePeriodId, DateTime startDate, DateTime stopDate)
        {
            return (from t in l
                    where ((t.Date >= startDate && t.Date <= stopDate && !t.TimePeriodId.HasValue) || (t.TimePeriodId.HasValue && t.TimePeriodId.Value == timePeriodId))
                    select t).ToList();
        }

        public static bool IsTimeBlockPresence(this List<GetTimePayrollTransactionsForEmployee_Result> l, int timeBlockId)
        {
            return l.Any(i => i.TimeBlockId == timeBlockId && i.TimeCodeType == (int)SoeTimeCodeType.Work);
        }

        public static bool IsTimeBlockAbsence(this List<GetTimePayrollTransactionsForEmployee_Result> l, int timeBlockId)
        {
            return l.Any(i => i.TimeBlockId == timeBlockId && i.TimeCodeType == (int)SoeTimeCodeType.Absense);
        }

        public static bool IsPayrollTransactionPresence(this List<GetTimePayrollTransactionsForEmployee_Result> l, int timePayrollTransactionId)
        {
            return l.Any(i => i.TimePayrollTransactionId == timePayrollTransactionId && i.TimeCodeType == (int)SoeTimeCodeType.Work);
        }

        public static bool IsPayrollTransactionAbsence(this List<GetTimePayrollTransactionsForEmployee_Result> l, int timePayrollTransactionId)
        {
            return l.Any(i => i.TimePayrollTransactionId == timePayrollTransactionId && i.TimeCodeType == (int)SoeTimeCodeType.Absense);
        }

        public static int SumQuantity(this IEnumerable<GetTimePayrollTransactionsForEmployee_Result> l)
        {
            if (l.IsNullOrEmpty())
                return 0;

            return (int)l.Sum(e => e.Quantity);
        }

        public static DateTime? GetStartDate(this IEnumerable<GetTimePayrollTransactionsForEmployee_Result> l)
        {
            if (l.IsNullOrEmpty())
                return null;

            return l.Min(e => e.Date);
        }

        public static DateTime? GetStopDate(this IEnumerable<GetTimePayrollTransactionsForEmployee_Result> l)
        {
            if (l.IsNullOrEmpty())
                return null;

            return l.Max(e => e.Date);
        }

        public static void SetAccountInternalIds(this List<GetTimePayrollTransactionsForEmployee_Result> l, List<GetTimePayrollTransactionAccountsForEmployee_Result> accounting)
        {
            if (l.IsNullOrEmpty() || accounting.IsNullOrEmpty())
                return;

            foreach (var e in l)
            {
                e.AccountInternalIds = accounting.Where(i => i.TimePayrollTransactionId == e.TimePayrollTransactionId).Select(i => i.AccountId).ToList();
            }
        }

        public static void ClearFormulasAndAmounts(this AttestPayrollTransactionDTO e)
        {
            if (e == null)
                return;

            e.Formula = null;
            e.FormulaPlain = null;
            e.FormulaExtracted = null;
            e.FormulaNames = null;
            e.FormulaOrigin = null;

            if (e.Amount.HasValue)
                e.Amount = decimal.Zero;

            if (e.AmountCurrency.HasValue)
                e.AmountCurrency = decimal.Zero;

            if (e.AmountEntCurrency.HasValue)
                e.AmountEntCurrency = decimal.Zero;

            if (e.UnitPrice.HasValue)
                e.UnitPrice = decimal.Zero;

            if (e.UnitPriceCurrency.HasValue)
                e.UnitPriceCurrency = decimal.Zero;

            if (e.UnitPriceEntCurrency.HasValue)
                e.UnitPriceEntCurrency = decimal.Zero;
        }

        #endregion

        #region GetTimePayrollTransactionAccountsForEmployee_Result

        public static List<GetTimePayrollTransactionAccountsForEmployee_Result> Filter(this List<GetTimePayrollTransactionAccountsForEmployee_Result> l, List<int> timePayrollTransactionIds, int timePeriodId)
        {
            return (from t in l
                    where timePayrollTransactionIds.Contains(t.TimePayrollTransactionId) &&
                    ((!t.TimePeriodId.HasValue) || (t.TimePeriodId.HasValue && t.TimePeriodId.Value == timePeriodId))
                    select t).ToList();
        }

        public static List<AccountDTO> GetAccountInternals(this List<GetTimePayrollTransactionAccountsForEmployee_Result> l, int timePayrollTransactionId)
        {
            List<AccountDTO> accounts = new List<AccountDTO>();

            if (l.IsNullOrEmpty())
                return accounts;

            foreach (var e in l.Where(i => i.TimePayrollTransactionId == timePayrollTransactionId && i.AccountDimNr != Constants.ACCOUNTDIM_STANDARD))
            {
                accounts.Add(new AccountDTO()
                {
                    AccountId = e.AccountId,
                    AccountNr = e.AccountNr,
                    Name = e.Name,
                    AccountDimId = e.AccountDimId,
                    AccountDimNr = e.AccountDimNr,
                });
            }

            return accounts;
        }

        #endregion

        #region GetTimePayrollTransactionsWithAccIntsForEmployeeResult

        public static string GetEmployeeChildName(this GetTimePayrollTransactionsWithAccIntsForEmployee_Result item)
        {
            if (item.ChildBirthDate.HasValue)
                return item.ChildBirthDate.Value.ToShortDateString();
            else
            {
                string fullName = item.ChildFirstName + " " + item.ChildLastName;
                fullName = fullName.Trim();
                return fullName;
            }
        }

        #endregion

        #region GetTimePayrollScheduleTransactionsForEmployee_Result

        public static AttestPayrollTransactionDTO CreateTransactionItem(this GetTimePayrollScheduleTransactionsForEmployee_Result e, List<AccountDTO> accountInternals, AccountDTO accountStd, List<AccountDimDTO> accountDims, SoeTimePayrollScheduleTransactionType type, string attestStateName = "", string attestStateColor = "#FFFFFF", bool hasInfo = false)
        {
            var transactionItem = new AttestPayrollTransactionDTO()
            {
                //TimePayrollTransaction
                EmployeeId = e.EmployeeId,
                TimePayrollTransactionId = e.TimePayrollScheduleTransactionId,
                Quantity = e.Quantity,
                UnitPrice = e.UnitPrice,
                UnitPriceCurrency = e.UnitPriceCurrency,
                UnitPriceEntCurrency = e.UnitPriceEntCurrency,
                Amount = e.Amount,
                AmountCurrency = e.AmountCurrency,
                AmountEntCurrency = e.AmountEntCurrency,
                VatAmount = e.VatAmount,
                VatAmountCurrency = e.VatAmountCurrency,
                VatAmountEntCurrency = e.VatAmountEntCurrency,
                ReversedDate = e.ReversedDate,
                IsReversed = e.IsReversed,
                IsPreliminary = false,
                IsExported = false,
                Comment = null,
                HasInfo = hasInfo,
                IsAdded = false,
                AddedDateFrom = null,
                AddedDateTo = null,
                IsFixed = false,
                IsSpecifiedUnitPrice = false,
                IsAdditionOrDeduction = false,
                UnionFeeId = null,
                EarningTimeAccumulatorId = null,
                EmployeeVehicleId = null,
                PayrollStartValueRowId = null,
                RetroactivePayrollOutcomeId = e.RetroactivePayrollOutcomeId,
                VacationYearEndRowId = null,
                IsVacationFiveDaysPerWeek = false,
                TransactionSysPayrollTypeLevel1 = e.TransactionSysPayrollTypeLevel1,
                TransactionSysPayrollTypeLevel2 = e.TransactionSysPayrollTypeLevel2,
                TransactionSysPayrollTypeLevel3 = e.TransactionSysPayrollTypeLevel3,
                TransactionSysPayrollTypeLevel4 = e.TransactionSysPayrollTypeLevel4,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = null,
                ModifiedBy = null,

                //TimePayrollTransactionExtended
                PayrollPriceFormulaId = e.PayrollPriceFormulaId,
                PayrollPriceTypeId = e.PayrollPriceTypeId,
                Formula = e.Formula,
                FormulaPlain = e.FormulaPlain,
                FormulaExtracted = e.FormulaExtracted,
                FormulaNames = e.FormulaNames,
                FormulaOrigin = e.FormulaOrigin,
                PayrollCalculationPerformed = false,
                TimeUnit = (int)TermGroup_PayrollProductTimeUnit.Hours,
                QuantityWorkDays = 0,
                QuantityCalendarDays = 0,
                CalenderDayFactor = 0,
                IsDistributed = false,

                //TimePayrollScheduleTransaction
                IsScheduleTransaction = true,
                ScheduleTransactionType = type,

                //PayrollProduct
                PayrollProductId = e.ProductId,
                PayrollProductNumber = e.ProductNumber,
                PayrollProductName = e.ProductName,
                PayrollProductShortName = e.ProductShortName,
                PayrollProductFactor = e.PayrollProductFactor,
                PayrollProductPayed = false, //item.PayrollProductPayed,
                PayrollProductExport = e.PayrollProductExport,
                PayrollProductUseInPayroll = e.PayrollProductUseInPayroll,
                PayrollProductSysPayrollTypeLevel1 = e.PayrollProductSysPayrollTypeLevel1,
                PayrollProductSysPayrollTypeLevel2 = e.PayrollProductSysPayrollTypeLevel2,
                PayrollProductSysPayrollTypeLevel3 = e.PayrollProductSysPayrollTypeLevel3,
                PayrollProductSysPayrollTypeLevel4 = e.PayrollProductSysPayrollTypeLevel4,
                IsAverageCalculated = e.AverageCalculated,

                //TimeCodeTransaction
                TimeCodeTransactionId = 0,
                StartTime = e.TimeBlockStartTime,
                StopTime = e.TimeBlockStopTime,

                //TimeCode
                TimeCodeType = SoeTimeCodeType.None,
                TimeCodeRegistrationType = TermGroup_TimeCodeRegistrationType.Unknown,
                NoOfPresenceWorkOutsideScheduleTime = null,
                NoOfAbsenceAbsenceTime = null,

                //TimeBlockDate
                TimeBlockDateId = e.TimeBlockDateId,
                Date = e.Date,

                //TimeBlock
                TimeBlockId = null,

                //AttestState
                AttestStateId = 0,
                AttestStateName = attestStateName,
                AttestStateColor = attestStateColor,
                AttestStateInitial = false,
                AttestStateSort = -1,
                HasSameAttestState = true,

                //Accounting
                AccountDims = accountDims,
                AccountStd = accountStd,
                AccountInternals = accountInternals ?? new List<AccountDTO>(),

                //TimePeriod
                TimePeriodId = null,
                TimePeriodName = String.Empty,

                //Flags
                IsPresence = true,
            };

            transactionItem.SetAccountingStrings();

            return transactionItem;
        }

        public static List<GetTimePayrollScheduleTransactionsForEmployee_Result> Filter(this List<GetTimePayrollScheduleTransactionsForEmployee_Result> l, DateTime startDate, DateTime stopDate)
        {
            return (from t in l
                    where (t.Date >= startDate && t.Date <= stopDate)
                    select t).ToList();
        }

        public static DateTime? GetStartDate(this IEnumerable<GetTimePayrollScheduleTransactionsForEmployee_Result> l)
        {
            if (l.IsNullOrEmpty())
                return null;

            return l.Min(e => e.Date);
        }

        public static DateTime? GetStopDate(this IEnumerable<GetTimePayrollScheduleTransactionsForEmployee_Result> l)
        {
            if (l.IsNullOrEmpty())
                return null;

            return l.Max(e => e.Date);
        }

        #endregion

        #region GetTimePayrollScheduleTransactionAccountsForEmployee_Result

        public static List<GetTimePayrollScheduleTransactionAccountsForEmployee_Result> Filter(this List<GetTimePayrollScheduleTransactionAccountsForEmployee_Result> l, List<int> timePayrollScheduleTransactionIds, int employeeId)
        {
            return (from t in l
                    where timePayrollScheduleTransactionIds.Contains(t.TimePayrollScheduleTransactionId) &&
                    t.EmployeeId == employeeId
                    select t).ToList();
        }

        public static List<AccountDTO> GetAccountInternals(this List<GetTimePayrollScheduleTransactionAccountsForEmployee_Result> l, int timePayrollScheduleTransactionId)
        {
            List<AccountDTO> accountInternalItems = new List<AccountDTO>();

            if (l.IsNullOrEmpty())
                return accountInternalItems;

            foreach (var e in l.Where(i => i.TimePayrollScheduleTransactionId == timePayrollScheduleTransactionId))
            {
                accountInternalItems.Add(new AccountDTO()
                {
                    AccountId = e.AccountId,
                    AccountNr = e.AccountNr,
                    Name = e.Name,
                    AccountDimId = e.AccountDimId,
                    AccountDimNr = e.AccountDimNr,
                });
            }

            return accountInternalItems;
        }

        #endregion

        #region GetTimeStampEntrysEmployeeSummaryResult

        public static bool HasTimeStamps(this List<GetTimeStampEntrysEmployeeSummaryResult> items)
        {
            var filteredItems = items.Where(i => i.NrOfTimeStamps > 0).ToList();
            return filteredItems.Count > 0;
        }

        public static bool HasDateTimeStamps(this List<GetTimeStampEntrysEmployeeSummaryResult> items, int timeBlockDateId)
        {
            var filteredItems = items.Where(i => i.TimeBlockDateId == timeBlockDateId && i.NrOfTimeStamps > 0).ToList();
            return filteredItems.Count > 0;
        }

        public static bool HasDateEmployeeManuallyAdjustedTimeStamps(this List<GetTimeStampEntrysEmployeeSummaryResult> items, int timeBlockDateId)
        {
            var filteredItems = items.Where(i => i.TimeBlockDateId == timeBlockDateId && i.NrOfEmployeeManuallyAdjustedTimeStamps > 0).ToList();
            return filteredItems.Count > 0;
        }

        #endregion

        #region GetTimeScheduleTemplateBlocksForEmployee_Result

        public static List<GetTimeScheduleTemplateBlocksForEmployee_Result> GetDay(this List<GetTimeScheduleTemplateBlocksForEmployee_Result> items, DateTime date)
        {
            return items?.Where(b => b.Date.HasValue && b.Date.Value.Date == date).ToList() ?? new List<GetTimeScheduleTemplateBlocksForEmployee_Result>();
        }

        public static List<GetTimeScheduleTemplateBlocksForEmployee_Result> GetWork(this List<GetTimeScheduleTemplateBlocksForEmployee_Result> items)
        {
            return items?.Where(i => !i.IsBreak && i.StartTime <= i.StopTime).OrderBy(i => i.StartTime).ToList() ?? new List<GetTimeScheduleTemplateBlocksForEmployee_Result>();
        }

        public static List<GetTimeScheduleTemplateBlocksForEmployee_Result> GetBreaks(this List<GetTimeScheduleTemplateBlocksForEmployee_Result> items)
        {
            return items?.Where(i => i.IsBreak && i.StartTime <= i.StopTime).OrderBy(i => i.StartTime).ToList() ?? new List<GetTimeScheduleTemplateBlocksForEmployee_Result>();
        }

        public static bool IsSchedule(this GetTimeScheduleTemplateBlocksForEmployee_Result e)
        {
            return e.Type == (int)TermGroup_TimeScheduleTemplateBlockType.Schedule;
        }
        public static bool IsOrder(this GetTimeScheduleTemplateBlocksForEmployee_Result e)
        {
            return e.Type == (int)TermGroup_TimeScheduleTemplateBlockType.Order;
        }
        public static bool IsBooking(this GetTimeScheduleTemplateBlocksForEmployee_Result e)
        {
            return e.Type == (int)TermGroup_TimeScheduleTemplateBlockType.Booking;
        }
        public static bool IsStandby(this GetTimeScheduleTemplateBlocksForEmployee_Result e)
        {
            return e.Type == (int)TermGroup_TimeScheduleTemplateBlockType.Standby;
        }

        #endregion

        #region GetTimeSchedulePlanningPeriods_Result

        public static bool HasAbsence(this List<GetTimeSchedulePlanningPeriods_Result> l)
        {
            if (l.IsNullOrEmpty())
                return false;
            return l.Any(x => x.TimeDeviationCauseId.HasValue);
        }

        public static bool HasOnDuty(this List<GetTimeSchedulePlanningPeriods_Result> l)
        {
            if (l.IsNullOrEmpty())
                return false;
            return l.Any(x => x.Type == (int)TermGroup_TimeScheduleTemplateBlockType.OnDuty);
        }

        public static bool IsPartTimeAbsence(this List<GetTimeSchedulePlanningPeriods_Result> l)
        {
            if (l.IsNullOrEmpty())
                return false;
            return l.HasAbsence() && l.Count(x => x.BreakType == (int)SoeTimeScheduleTemplateBlockBreakType.None && x.TimeDeviationCauseId.HasValue) < l.Count(x => x.BreakType == (int)SoeTimeScheduleTemplateBlockBreakType.None);
        }

        public static bool IsWholeDayAbsence(this List<GetTimeSchedulePlanningPeriods_Result> l)
        {
            if (l.IsNullOrEmpty())
                return false;
            return l.Count(x => x.BreakType == (int)SoeTimeScheduleTemplateBlockBreakType.None && x.TimeDeviationCauseId.HasValue) == l.Count(x => x.BreakType == (int)SoeTimeScheduleTemplateBlockBreakType.None);
        }

        public static bool HasDescription(this List<GetTimeSchedulePlanningPeriods_Result> l)
        {
            if (l.IsNullOrEmpty())
                return false;
            return l.Any(x => !string.IsNullOrEmpty(x.Description));
        }

        public static DateTime GetScheduleIn(this List<GetTimeSchedulePlanningPeriods_Result> l)
        {
            return l.GetScheduleInShift()?.StartTime ?? CalendarUtility.DATETIME_DEFAULT;
        }

        public static DateTime GetScheduleOut(this List<GetTimeSchedulePlanningPeriods_Result> l)
        {
            return l.GetScheduleOutShift()?.StopTime ?? CalendarUtility.DATETIME_DEFAULT;
        }

        public static GetTimeSchedulePlanningPeriods_Result GetScheduleInShift(this List<GetTimeSchedulePlanningPeriods_Result> l)
        {
            return l?.Where(i => i.Type == (int)TermGroup_TimeScheduleTemplateBlockType.Schedule || i.Type == (int)TermGroup_TimeScheduleTemplateBlockType.Standby || i.Type == (int)TermGroup_TimeScheduleTemplateBlockType.OnDuty).OrderBy(x => x.StartTime).FirstOrDefault();
        }

        public static GetTimeSchedulePlanningPeriods_Result GetScheduleOutShift(this List<GetTimeSchedulePlanningPeriods_Result> l)
        {
            return l?.Where(i => i.Type == (int)TermGroup_TimeScheduleTemplateBlockType.Schedule || i.Type == (int)TermGroup_TimeScheduleTemplateBlockType.Standby || i.Type == (int)TermGroup_TimeScheduleTemplateBlockType.OnDuty).OrderByDescending(x => x.StopTime).FirstOrDefault();
        }

        #endregion

        #region GetTemplateScheduleResult

        public static List<GetTemplateSchedule_Result> GetScheduleForPeriod(this List<GetTemplateSchedule_Result> items, int templatePeriodId)
        {
            return (from ts in items
                    where ts.TimeScheduleTemplatePeriodId == templatePeriodId
                    select ts).ToList();
        }

        public static List<GetTemplateSchedule_Result> GetWork(this List<GetTemplateSchedule_Result> items)
        {
            return items.Where(i => !i.IsBreak).OrderBy(i => i.StartTime).ToList();
        }

        public static List<GetTemplateSchedule_Result> GetBreaks(this List<GetTemplateSchedule_Result> items)
        {
            return items.Where(i => i.IsBreak).OrderBy(i => i.StartTime).ToList();
        }

        public static int GetWorkMinutes(this List<GetTemplateSchedule_Result> l)
        {
            return l.GetWork().GetMinutes() - l.GetBreaks().GetMinutes();
        }


        public static int GetMinutes(this List<GetTemplateSchedule_Result> l)
        {
            return l?.Sum(e => e.GetMinutes()) ?? 0;
        }

        public static int GetMinutes(this GetTemplateSchedule_Result e)
        {
            if (e == null)
                return 0;
            return (int)e.StopTime.Subtract(e.StartTime).TotalMinutes;
        }

        #endregion

        #region GetTimeScheduleTemplateBlockAccountsForEmployeeResult(

        public static bool ContainsAnyAccountId(this List<GetTimeScheduleTemplateBlockAccountsForEmployeeResult> l, int templateBlockId, List<int> templateBlockAccountIds)
        {
            return l.GetAccountsForTemplateBlock(templateBlockId).ContainsAny(templateBlockAccountIds);
        }

        public static List<GetTimeScheduleTemplateBlockAccountsForEmployeeResult> GetAccountsForTemplateBlock(this List<GetTimeScheduleTemplateBlockAccountsForEmployeeResult> l, int timeScheduleTemplateBlockId)
        {
            return l?.Where(i => i.TimeScheduleTemplateBlockId == timeScheduleTemplateBlockId).ToList();
        }

        public static bool ContainsAny(this List<GetTimeScheduleTemplateBlockAccountsForEmployeeResult> l, List<int> accountIds)
        {
            return l?.Any(i => i.ContainsAny(accountIds)) ?? false;
        }

        public static bool ContainsAny(this GetTimeScheduleTemplateBlockAccountsForEmployeeResult e, List<int> accountIds)
        {
            return e != null && accountIds.Contains(e.AccountId);
        }

        #endregion

        #region Product statistics

        public static StatisticsMostSoldProductsByAmountResultDTO ToDTO(this StatisticsMostSoldProductsByAmountResult e)
        {
            if (e == null)
                return null;

            StatisticsMostSoldProductsByAmountResultDTO dto = new StatisticsMostSoldProductsByAmountResultDTO()
            {
                ProductId = e.ProductId,
                Number = e.Number,
                Name = e.Name,
                SumAmount = e.SumAmount,
                SumPurchasePrice = e.SumPurchasePrice,
            };

            return dto;
        }

        public static IEnumerable<StatisticsMostSoldProductsByAmountResultDTO> ToDTOs(this IEnumerable<StatisticsMostSoldProductsByAmountResult> l)
        {
            var dtos = new List<StatisticsMostSoldProductsByAmountResultDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static StatisticsMostSoldProductsByQuantityResultDTO ToDTO(this StatisticsMostSoldProductsByQuantityResult e)
        {
            if (e == null)
                return null;

            StatisticsMostSoldProductsByQuantityResultDTO dto = new StatisticsMostSoldProductsByQuantityResultDTO()
            {
                ProductId = e.ProductId,
                Number = e.Number,
                Name = e.Name,
                Quantity = e.Quantity
            };

            return dto;
        }

        public static IEnumerable<StatisticsMostSoldProductsByQuantityResultDTO> ToDTOs(this IEnumerable<StatisticsMostSoldProductsByQuantityResult> l)
        {
            var dtos = new List<StatisticsMostSoldProductsByQuantityResultDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        #endregion

        #endregion

        #region SignatoryContract

        public static SignatoryContractGridDTO ToGridDTO(
            this SignatoryContract sc,
            Dictionary<int, string> permissionTerms = null,
            Dictionary<int, string> authenticationMethodTerms = null)
        {
            if (sc == null)
            {
                return null;
            }

            List<string> permissionNames = new List<string>();

            if (permissionTerms?.Any() == true)
            {
                permissionNames = sc.SignatoryContractPermission
                    .Where(s => permissionTerms.ContainsKey(s.PermissionType))
                    .Select(s => permissionTerms[s.PermissionType])
                    .OrderBy(p => p)
                    .ToList();
            }

            User recipient = sc
                .SignatoryContractRecipient
                .FirstOrDefault()
                ?.User;

            int recipientUserId = 0;
            string recipientUserName = string.Empty;

            if (recipient != null)
            {
                recipientUserId = recipient.UserId;

                if (string.IsNullOrEmpty(recipient.Name))
                {
                    recipientUserName = recipient.LoginName;
                }
                else
                {
                    recipientUserName = recipient.Name;
                }
            }

            SignatoryContractGridDTO dto = new SignatoryContractGridDTO
            {
                SignatoryContractId = sc.SignatoryContractId,
                ActorCompanyId = sc.ActorCompanyId,
                ParentSignatoryContractId = sc.ParentSignatoryContractId,
                SignedByUserId = sc.SignedByUserId,
                CreationMethodType = sc.CreationMethodType,
                RequiredAuthenticationMethodType = sc.RequiredAuthenticationMethodType,
                CanPropagate = sc.CanPropagate,
                Created = sc.Created,
                RevokedAtUTC = sc.RevokedAtUTC,
                RevokedBy = sc.RevokedBy,
                RevokedReason = sc.RevokedReason,
                PermissionTypes = sc.SignatoryContractPermission
                    .Select(s => s.PermissionType)
                    .ToList(),
                PermissionNames = permissionNames,
                RecipientUserId = recipientUserId,
                RecipientUserName = recipientUserName,
            };

            if (authenticationMethodTerms?
                .ContainsKey(sc.RequiredAuthenticationMethodType) == true)
            {
                dto.AuthenticationMethod =
                    authenticationMethodTerms[sc.RequiredAuthenticationMethodType];
            }

            return dto;
        }

        public static IEnumerable<SignatoryContractGridDTO> ToGridDTOs(
            this IEnumerable<SignatoryContract> l,
            Dictionary<int, string> permissionTerms = null,
            Dictionary<int, string> authenticationMethodTerms = null)
        {

            IEnumerable<SignatoryContractGridDTO> dtos
                = new List<SignatoryContractGridDTO>();
            if (l?.Any() == true)
            {
                dtos = l.Select(s => s.ToGridDTO(
                    permissionTerms, authenticationMethodTerms));
            }

            return dtos;
        }

        public static IEnumerable<SignatoryContractDTO> ToDTOs(
            this IEnumerable<SignatoryContract> l,
            Dictionary<int, string> permissionTerms = null)
        {

            IEnumerable<SignatoryContractDTO> dtos
                = new List<SignatoryContractDTO>();
            if (l?.Any() == true)
            {
                dtos = l.Select(s => s.ToDTO(permissionTerms));
            }

            return dtos;
        }


        public static SignatoryContractDTO ToDTO(
            this SignatoryContract sc,
            Dictionary<int, string> permissionTerms = null)
        {
            if (sc == null)
            {
                return null;
            }

            List<string> permissionNames = new List<string>();

            if (permissionTerms?.Any() == true)
            {
                permissionNames = sc.SignatoryContractPermission
                    .Where(s => permissionTerms.ContainsKey(s.PermissionType))
                    .Select(s => permissionTerms[s.PermissionType])
                    .OrderBy(p => p)
                    .ToList();
            }

            List<SignatoryContractDTO> childrenDtos
                = new List<SignatoryContractDTO>();

            string signedByUserName = string.Empty;

            if (sc.User != null)
            {
                if (string.IsNullOrEmpty(sc.User.Name))
                {
                    signedByUserName = sc.User.LoginName;
                }
                else
                {
                    signedByUserName = sc.User.Name;
                }
            }


            List<SignatoryContractRecipientDTO> recipients = sc
                .SignatoryContractRecipient
                .Select((scr) =>
                {
                    SignatoryContractRecipientDTO recipient = new SignatoryContractRecipientDTO
                    {
                        SignatoryContractRecipientId = scr.SignatoryContractRecipientId,
                        SignatoryContractId = scr.SignatoryContractId,
                        RecipientUserId = scr.RecipientUserId,
                        RecipientIdLoginGuid = scr.RecipientIdLoginGuid,
                        RecipientUserName = string.IsNullOrWhiteSpace(scr.User.Name) ?
                            scr.User.LoginName : scr.User.Name,
                    };

                    return recipient;
                })
                .ToList();

            int recipientUserId = 0;
            string recipientUserName = string.Empty;

            if (recipients.Count > 0)
            {
                recipientUserId = recipients[0].RecipientUserId;
                recipientUserName = recipients[0].RecipientUserName;
            }

            SignatoryContractDTO dto = new SignatoryContractDTO
            {
                SignatoryContractId = sc.SignatoryContractId,
                ActorCompanyId = sc.ActorCompanyId,
                ParentSignatoryContractId = sc.ParentSignatoryContractId,
                SignedByUserId = sc.SignedByUserId,
                CreationMethodType = sc.CreationMethodType,
                CanPropagate = sc.CanPropagate,
                RevokedBy = sc.RevokedBy,
                RevokedReason = sc.RevokedReason,
                Created = sc.Created,
                CreatedBy = sc.CreatedBy,
                RevokedAtUTC = sc.RevokedAtUTC,
                RequiredAuthenticationMethodType = sc.RequiredAuthenticationMethodType,
                SignedByUserName = signedByUserName,
                PermissionTypes = sc.SignatoryContractPermission
                    .Select(s => s.PermissionType)
                    .ToList(),
                PermissionNames = permissionNames,
                SubContracts = sc.SignatoryContract1
                    .ToDTOs(permissionTerms)
                    .ToList(),
                Recipients = recipients,
                RecipientUserId = recipientUserId,
                RecipientUserName = recipientUserName
            };

            return dto;
        }

        #endregion

        #region LazyLoaders

        #region PaymentRow

        /// <summary>
        /// <para>
        /// Caller must ensure that parameter-entity is in the same context where it was loaded or saved and that it is in state Active when calling this method.
        /// </para>
        /// <para>
        /// Ensures that reference is loaded before returning requested entity, indicated by the method name
        /// </para>
        /// </summary>
        /// <param name="paymentRow"></param>
        /// <returns></returns>
        public static Invoice InvoiceLazy(this PaymentRow paymentRow)
        {
            if (!paymentRow.InvoiceReference.IsLoaded)
            {
                paymentRow.InvoiceReference.Load();
                DataProjectLogCollector.LogLoadedEntityInExtension("paymentRow.InvoiceReference");
            }
            return paymentRow.Invoice;
        }

        #endregion

        #region Invoice

        /// <summary>
        /// <para>
        /// Caller must ensure that parameter-entity is in the same context where it was loaded or saved and that it is in state Active when calling this method.
        /// </para>
        /// <para>
        /// Ensures that reference is loaded before returning requested entity, indicated by the method name
        /// </para>
        /// </summary>
        /// <param name="paymentRow"></param>
        /// <returns></returns>
        public static Actor ActorLazy(this Invoice invoice)
        {
            if (!invoice.ActorReference.IsLoaded)
            {
                invoice.ActorReference.Load();
                DataProjectLogCollector.LogLoadedEntityInExtension("invoice.ActorReference");
            }
            return invoice.Actor;
        }

        /// <summary>
        /// <para>
        /// Caller must ensure that parameter-entity is in the same context where it was loaded or saved and that it is in state Active when calling this method.
        /// </para>
        /// <para>
        /// Ensures that reference is loaded before returning requested entity, indicated by the method name
        /// </para>
        /// </summary>
        /// <param name="paymentRow"></param>
        /// <returns></returns>
        public static Currency CurrencyLazy(this Invoice invoice)
        {
            if (!invoice.CurrencyReference.IsLoaded)
            {
                invoice.CurrencyReference.Load();
                DataProjectLogCollector.LogLoadedEntityInExtension("invoice.CurrencyReference");
            }
            return invoice.Currency;
        }

        #endregion

        #region Actor

        /// <summary>
        /// <para>
        /// Caller must ensure that parameter-entity is in the same context where it was loaded or saved and that it is in state Active when calling this method.
        /// </para>
        /// <para>
        /// Ensures that reference is loaded before returning requested entity, indicated by the method name
        /// </para>
        /// </summary>
        /// <param name="paymentRow"></param>
        /// <returns></returns>
        public static Supplier SupplierLazy(this Actor actor)
        {
            if (!actor.SupplierReference.IsLoaded)
            {
                actor.SupplierReference.Load();
                DataProjectLogCollector.LogLoadedEntityInExtension("actor.SupplierReference");
            }
            return actor.Supplier;
        }

        #endregion

        #endregion
    }

    public static class DebugCounter
    {
        public static int CurrentCount;
    }
}

using SoftOne.Communicator.Shared.DTO;
using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Transactions;

namespace SoftOne.Soe.Business.Core
{
    public class ProjectBudgetManager: ManagerBase
    {
        #region Variables

        // Create a logger for use in this class
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #endregion

        #region Ctor

        public ProjectBudgetManager(ParameterObject parameterObject) : base(parameterObject) { }

        #endregion

        #region Project Budget

        public bool HasExtendedProjectBudget(int projectId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return HasExtendedProjectBudget(entities, projectId);
        }

        public bool HasExtendedProjectBudget(CompEntities entities, int projectId)
        {
            return (from bh in entities.BudgetHead
                         where bh.ProjectId == projectId &&
                               (bh.Type == (int)DistributionCodeBudgetType.ProjectBudgetExtended ||
                               bh.Type == (int)DistributionCodeBudgetType.ProjectBudgetIB) &&
                               bh.State == (int)SoeEntityState.Active
                         select bh.BudgetHeadId).Any();
        }

        public BudgetHeadProjectDTO GetProjectBudgetHeadIncludingRows(int budgetHeadId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetProjectBudgetHeadIncludingRows(entities, budgetHeadId);
        }

        public BudgetHeadProjectDTO GetProjectBudgetHeadIncludingRows(CompEntities entities, int budgetHeadId, bool includeLog = false, bool forComparison = false)
        {
            var query = (from bh in entities.BudgetHead
                         where bh.BudgetHeadId == budgetHeadId
                         select bh);

            BudgetHeadProjectDTO budgetHead = null;
            if (includeLog)
                budgetHead = query.Select(EntityExtensions.ProjectBudgetHeadIncludingRowsAndLogs).FirstOrDefault();
            else
                budgetHead = query.Select(EntityExtensions.ProjectBudgetHeadIncludingRows).FirstOrDefault();

            if (budgetHead != null && budgetHead.Type == (int)DistributionCodeBudgetType.ProjectBudgetForecast)
            {
                // Consolidate rows
                DateTime? currentTimeStamp = null;
                string currentTimeStampValue = string.Empty;
                var tempRows = new List<BudgetRowProjectDTO>();
                foreach (var row in budgetHead.Rows)
                {
                    if (!row.ParentBudgetRowId.HasValue)
                    {
                        var childRow = budgetHead.Rows.FirstOrDefault(r => r.ParentBudgetRowId.HasValue && r.ParentBudgetRowId.Value == row.BudgetRowId);
                        if (childRow != null)
                        {
                            row.TotalAmountResult += childRow.TotalAmount;
                            row.TotalQuantityResult += childRow.TotalQuantity > 0 ? childRow.TotalQuantity : 0;
                            if (row.Type != (int)ProjectCentralBudgetRowType.OverheadCostPerHour)
                                row.TotalDiffResult = row.TotalAmount != 0 ? Decimal.Round((row.TotalAmountResult / row.TotalAmount) * 100, 2) : 0;

                            var rowTimeStamp = childRow.Modified.HasValue ? childRow.Modified.Value : childRow.Created.Value;
                            if (rowTimeStamp > currentTimeStamp || currentTimeStamp == null)
                            {
                                currentTimeStamp = rowTimeStamp;
                                currentTimeStampValue = rowTimeStamp.ToShortDateString() + " - " + (childRow.ModifiedBy ?? childRow.CreatedBy);
                            }
                        }

                        tempRows.Add(row);
                    }
                }

                if (string.IsNullOrEmpty(budgetHead.ResultModified))
                    budgetHead.ResultModified = currentTimeStampValue;

                budgetHead.Rows = tempRows;

                // Add values from budget to compare
                var parentQuery = (from bh in entities.BudgetHead
                             where bh.BudgetHeadId == budgetHead.ParentBudgetHeadId.Value
                             select bh);

                var parentBudgetHead = parentQuery.Select(EntityExtensions.ProjectBudgetHeadIncludingBaseRows).FirstOrDefault();
                if (parentBudgetHead != null)
                {
                    budgetHead.ParentBudgetHeadName = parentBudgetHead.Name;
                    foreach (var row in parentBudgetHead.Rows)
                    {
                        var parentRow = budgetHead.Rows.FirstOrDefault(r => r.Type == row.Type && r.TimeCodeId == row.TimeCodeId);
                        if (parentRow != null)
                        {
                            parentRow.TotalAmountCompBudget += row.TotalAmount;
                            parentRow.TotalQuantityCompBudget += row.TotalQuantity > 0 ? row.TotalQuantity : 0;
                        }
                        else
                        {
                            parentRow = budgetHead.Rows.FirstOrDefault(r => r.Type == row.Type);
                            if (parentRow != null)
                            {
                                parentRow.TotalAmountCompBudget += row.TotalAmount;
                                parentRow.TotalQuantityCompBudget += row.TotalQuantity > 0 ? row.TotalQuantity : 0;
                            }
                        }
                    }
                }
            }

            // Set default
            int type = 0;
            foreach(var row in budgetHead.Rows.OrderBy(r => r.Type).ThenBy(r => r.BudgetRowId))
            {
                if(row.Type.HasValue && type != row.Type.Value)
                {
                    type = row.Type.Value;
                    row.IsDefault = true;
                }

                row.BudgetRowNr = budgetHead.Rows.IndexOf(row) + 1;

                if(!forComparison)
                    row.TotalQuantity = row.TotalQuantity > 0 ? row.TotalQuantity / 60 : 0;
            }

            return budgetHead;
        }

        public BudgetHeadProjectDTO GetLatestProjectBudgetHeadIncludingRows(int projectId, DistributionCodeBudgetType type, bool excludeResultRows)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetLatestProjectBudgetHeadIncludingRows(entities, projectId, type, excludeResultRows);
        }

        public BudgetHeadProjectDTO GetLatestProjectBudgetHeadIncludingRows(CompEntities entities, int projectId, DistributionCodeBudgetType type, bool excludeResultRows)
        {
            var query = (from bh in entities.BudgetHead
                              where bh.ProjectId == projectId &&
                                    bh.Type == (int)type &&
                                    bh.State == (int)SoeEntityState.Active
                         orderby bh.Created descending
                              select bh);

            if(excludeResultRows) 
                return query.Select(EntityExtensions.ProjectBudgetHeadIncludingBaseRows).FirstOrDefault();
            else
                return query.Select(EntityExtensions.ProjectBudgetHeadIncludingRows).FirstOrDefault();
        }

        public ActionResult SaveBudgetHeadForProject(BudgetHeadProjectDTO dto)
        {
            ActionResult result = new ActionResult(true);

            try
            {
                using (CompEntities entities = new CompEntities())
                {
                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        result = SaveBudgetHeadForProject(transaction, entities, dto, false, base.ActorCompanyId);
                        //Commit transaction
                        if (result.Success)
                        {
                            transaction.Complete();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                result.Exception = ex;
                base.LogError(ex, this.log);
            }
            finally
            {
                if (!result.Success)
                    base.LogTransactionFailed(this.ToString(), this.log);
            }

            return result;
        }

        public ActionResult SaveBudgetHeadForProject(TransactionScope transaction, CompEntities entities, BudgetHeadProjectDTO dto, bool isMigration, int actorCompanyId)
        {
            ActionResult result = new ActionResult(true);

            BudgetHead head = null;

            if (entities.Connection.State != ConnectionState.Open)
                entities.Connection.Open();

            #region Prereq

            if (dto == null)
                return new ActionResult(false, (int)ActionResultSave.EntityIsNull, GetText(7044, "Felaktig inparameter"));

            if (dto.BudgetHeadId != 0)
                head = BudgetManager.GetBudgetHeadIncludingRows(entities, dto.BudgetHeadId);
            else
            {
                // Validate types on new
            }

            #endregion

            #region Perform

            if(head == null)
            {
                head = new BudgetHead()
                {
                    ActorCompanyId = actorCompanyId,
                    Type = dto.Type,
                    Name = dto.Name,
                    NoOfPeriods = dto.NoOfPeriods,
                    ProjectId = dto.ProjectId,
                    Status = dto.Status,
                    PeriodType = dto.PeriodType ?? 1,
                    FromDate = dto.FromDate,
                    ToDate = dto.ToDate,
                    ParentBudgetHeadId = dto.ParentBudgetHeadId.ToNullable(),

                    Created = dto.Created,
                    CreatedBy = dto.CreatedBy,

                };

                head.IsAdded();
                if(!isMigration)
                    SetCreatedProperties(head);
                else
                    SetModifiedProperties(head);

                entities.BudgetHead.AddObject(head);
            }
            else
            {
                head.Type = dto.Type;
                head.Name = dto.Name;
                head.NoOfPeriods = dto.NoOfPeriods;
                head.Status = dto.Status;
                head.PeriodType = dto.PeriodType ?? 1;
                head.FromDate = dto.FromDate;
                head.ToDate = dto.ToDate;
                head.ParentBudgetHeadId = dto.ParentBudgetHeadId.ToNullable();

                SetModifiedProperties(head);
            }

            foreach (BudgetRowProjectDTO row in dto.Rows)
            {
                BudgetRow budgetRow = null;

                if (row.BudgetRowId != 0)
                {
                    budgetRow = head.BudgetRow.FirstOrDefault(r => r.BudgetRowId == row.BudgetRowId);

                    if (budgetRow != null)
                    {
                        if (row.IsDeleted)
                        {
                            //Delete
                            foreach (BudgetRowPeriod period in budgetRow.BudgetRowPeriod.ToList())
                            {
                                DeleteEntityItem(entities, period);
                            }

                            DeleteEntityItem(entities, budgetRow);
                        }
                        else
                        {
                            if (row.IsModified)
                            {
                                //Update
                                budgetRow.Type = row.Type;
                                budgetRow.TimeCodeId = row.TimeCodeId.ToNullable();
                                budgetRow.TotalAmount = row.TotalAmount;
                                budgetRow.TotalQuantity = row.TotalQuantity > 0 ? row.TotalQuantity * 60 : 0;
                                budgetRow.Comment = row.Comment;

                                SetModifiedProperties(budgetRow);

                                if (head.Type == (int)DistributionCodeBudgetType.ProjectBudgetForecast && (row.TotalAmountResult != 0 || row.TotalQuantityResult != 0))
                                {
                                    var resultRow = head.BudgetRow.FirstOrDefault(r => r.ParentBudgetRowId == row.BudgetRowId && r.Type == row.Type);
                                    if (resultRow != null)
                                    {
                                        resultRow.TotalAmount = row.TotalAmountResult;
                                        resultRow.TotalQuantity = row.TotalQuantityResult > 0 ? row.TotalQuantityResult : 0;

                                        SetModifiedProperties(resultRow);
                                    }
                                    else
                                    {
                                        resultRow = new BudgetRow()
                                        {
                                            TimeCodeId = row.TimeCodeId.ToNullable(),
                                            TotalAmount = row.TotalAmountResult,
                                            TotalQuantity = row.TotalQuantityResult > 0 ? row.TotalQuantityResult : 0,
                                            Type = row.Type,
                                            Comment = row.Comment,
                                            Locked = true,

                                            BudgetHead = head,
                                            BudgetRow2 = budgetRow,
                                        };

                                        SetCreatedProperties(resultRow);
                                        entities.BudgetRow.AddObject(resultRow);
                                    }
                                }

                                // Add log posts
                                if (row.ChangeLogItems != null && row.ChangeLogItems.Any())
                                {
                                    // Should only be one log post per save
                                    var logPost = row.ChangeLogItems.FirstOrDefault();
                                    if (logPost != null)
                                    {
                                        BudgetRowChangeLog logEntry = new BudgetRowChangeLog()
                                        {
                                            BudgetRowId = budgetRow.BudgetRowId,
                                            FromTotalAmount = logPost.FromTotalAmount,
                                            ToTotalAmount = logPost.ToTotalAmount,
                                            FromTotalQuantity = logPost.FromTotalQuantity,
                                            ToTotalQuantity = logPost.ToTotalQuantity,
                                            Comment = logPost.Comment,
                                        };
                                        SetCreatedProperties(logEntry);
                                        entities.BudgetRowChangeLog.AddObject(logEntry);
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    budgetRow = new BudgetRow()
                    {
                        TimeCodeId = row.TimeCodeId.ToNullable(),
                        TotalAmount = row.TotalAmount,
                        TotalQuantity = row.TotalQuantity > 0 ? row.TotalQuantity * 60 : 0,
                        Type = row.Type,
                        Comment = row.Comment,
                        Locked = row.IsLocked,

                        BudgetHead = head,
                    };

                    if (isMigration || (head.Type == (int)DistributionCodeBudgetType.ProjectBudgetForecast && row.Created != null))
                    {
                        budgetRow.Created = row.Created;
                        budgetRow.CreatedBy = row.CreatedBy;

                        SetModifiedProperties(budgetRow);
                    }
                    else
                    {
                        SetCreatedProperties(budgetRow);
                    }

                    entities.BudgetRow.AddObject(budgetRow);

                    if (head.Type == (int)DistributionCodeBudgetType.ProjectBudgetForecast && (row.TotalAmountResult != 0 || row.TotalQuantityResult != 0))
                    {
                        var resultRow = new BudgetRow()
                        {
                            TimeCodeId = row.TimeCodeId.ToNullable(),
                            TotalAmount = row.TotalAmountResult,
                            TotalQuantity = row.TotalQuantityResult > 0 ? row.TotalQuantityResult : 0,
                            Type = row.Type,
                            Comment = row.Comment,
                            Locked = true,

                            BudgetHead = head,
                            BudgetRow2 = budgetRow,
                        };

                        SetCreatedProperties(resultRow);
                        entities.BudgetRow.AddObject(resultRow);
                    }

                    // Add log posts
                    if (row.ChangeLogItems != null && row.ChangeLogItems.Any())
                    {
                        foreach (var logPost in row.ChangeLogItems)
                        {
                            BudgetRowChangeLog logEntry = new BudgetRowChangeLog()
                            {
                                FromTotalAmount = logPost.FromTotalAmount,
                                ToTotalAmount = logPost.ToTotalAmount,
                                FromTotalQuantity = logPost.FromTotalQuantity,
                                ToTotalQuantity = logPost.ToTotalQuantity,
                                Comment = logPost.Comment,

                                BudgetRow = budgetRow,
                            };

                            if(isMigration || (head.Type == (int)DistributionCodeBudgetType.ProjectBudgetForecast && row.Created != null))
                            {
                                logEntry.Created = logPost.Created;
                                logEntry.CreatedBy = logPost.CreatedBy;
                            }
                            else
                            {
                                SetCreatedProperties(logEntry);
                            }

                            entities.BudgetRowChangeLog.AddObject(logEntry);
                        }
                    }
                    else
                    {
                        BudgetRowChangeLog logEntry = new BudgetRowChangeLog()
                        {
                            FromTotalAmount = 0,
                            ToTotalAmount = row.TotalAmount,
                            FromTotalQuantity = 0,
                            ToTotalQuantity = row.TotalQuantity,
                            Comment = String.Empty,

                            CreatedBy = row.CreatedBy,
                            Created = row.Created,

                            BudgetRow = budgetRow,
                        };

                        if (!isMigration)
                            SetCreatedProperties(logEntry);

                        entities.BudgetRowChangeLog.AddObject(logEntry);
                    }
                }
            }

            result = SaveChanges(entities);

            #endregion

            if (result.Success)
                result.IntegerValue = head.BudgetHeadId;

            return result;
        }

        #endregion

        #region Forecast

        public BudgetHeadProjectDTO CreateForecastFromProject(int projectId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return CreateForecastFromProject(entities, projectId);
        }

        public BudgetHeadProjectDTO CreateForecastFromProject(CompEntities entities, int projectId)
        {
            int projectBudgetId = (from bh in entities.BudgetHead
                                    where bh.ProjectId == projectId &&
                                    bh.Type == (int)DistributionCodeBudgetType.ProjectBudgetExtended && 
                                    bh.State == (int)SoeEntityState.Active
                                   select bh.BudgetHeadId).FirstOrDefault();

            if (projectBudgetId == 0)
                throw new Exception(GetText(7887, "Projektet har ingen budget att skapa prognos från."));

            return CreateForecastFromProjectBudget(entities, projectBudgetId);
        }
        
        public BudgetHeadProjectDTO CreateForecastFromProjectBudget(int projectBudgetHeadId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return CreateForecastFromProjectBudget(entities, projectBudgetHeadId);
        }

        public BudgetHeadProjectDTO CreateForecastFromProjectBudget(CompEntities entities, int projectBudgetHeadId)
        {
            var forecastDate = DateTime.Today.AddDays(1);

            #region Get Budget Head

            var projectBudgetHead = GetProjectBudgetHeadIncludingRows(entities, projectBudgetHeadId, true, true);
            projectBudgetHead.BudgetHeadId = 0; // Reset ID for new forecast
            projectBudgetHead.ParentBudgetHeadId = projectBudgetHeadId;
            projectBudgetHead.ParentBudgetHeadName = projectBudgetHead.Name;
            projectBudgetHead.Type = (int)DistributionCodeBudgetType.ProjectBudgetForecast;
            projectBudgetHead.Name = String.Empty;
            projectBudgetHead.Status = (int)BudgetHeadStatus.Preliminary;

            // Reset timestamps
            projectBudgetHead.Created = null;
            projectBudgetHead.CreatedBy = null;
            projectBudgetHead.Modified = null;
            projectBudgetHead.ModifiedBy = null;

            //Reset row values
            List<BudgetRowProjectDTO> forecastRows = projectBudgetHead.Rows;
            foreach (var row in forecastRows)
            {
                row.BudgetRowId = 0;
                row.TotalAmountResult = 0;
                row.TotalQuantityResult = 0;
                row.TotalDiffResult = 0;
                row.IsLocked = true;

                row.TotalAmountCompBudget = row.TotalAmount;
                row.TotalQuantityCompBudget = row.TotalQuantity;
                row.TotalQuantity = row.TotalQuantity / 60;

                foreach(var log in row.ChangeLogItems)
                {
                    log.BudgetRowChangeLogId = 0; // Reset ID for new forecast
                }
            }

            #endregion

            // Get project data
            forecastRows = ConsolidateProjectData(entities, projectBudgetHead.ProjectId.Value, forecastRows);

            // Set diffs
            foreach (var row in forecastRows)
            {
                row.TotalDiffResult = row.TotalAmount != 0 ? Decimal.Round((row.TotalAmountResult / row.TotalAmount) * 100, 2) : 0;
            }

            // Sort rows
            projectBudgetHead.Rows = forecastRows.OrderBy(r => r.Type).ThenBy(r => r.TypeCodeName).ToList();

            

            return projectBudgetHead;
        }

        public ActionResult UpdateBudgetForecastResult(int budgetHeadId)
        {
            BudgetHeadProjectDTO head = null;
            using (CompEntities entities = new CompEntities())
            {
                head = GetProjectBudgetHeadIncludingRows(entities, budgetHeadId);

                if(head != null)
                {
                    foreach (var row in head.Rows)
                    {
                        row.TotalAmountResult = 0;
                        row.TotalQuantityResult = 0;
                    }

                    head.Rows = ConsolidateProjectData(entities, head.ProjectId.Value, head.Rows);
                }
            }

            return SaveBudgetHeadForProject(head);
        }

        private List<BudgetRowProjectDTO> ConsolidateProjectData(CompEntities entities, int projectId, List<BudgetRowProjectDTO> forecastRows)
        {
            #region Handle Project Data
            using var entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            #region Prereq

            // Settings
            bool useProjectTimeBlock = SettingManager.GetBoolSetting(entities, SettingMainType.Company, (int)CompanySettingType.UseProjectTimeBlocks, this.UserId, this.ActorCompanyId, 0, false);
            int ROTDeductionProductId = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.ProductHouseholdTaxDeduction, 0, this.ActorCompanyId, 0);
            int ROT50DeductionProductId = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.ProductHousehold50TaxDeduction, 0, this.ActorCompanyId, 0);
            int RUTDeductionProductId = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.ProductRUTTaxDeduction, 0, this.ActorCompanyId, 0);
            int Green15DeductionProductId = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.ProductGreen15TaxDeduction, 0, this.ActorCompanyId, 0);
            int Green20DeductionProductId = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.ProductGreen20TaxDeduction, 0, this.ActorCompanyId, 0);
            int Green50DeductionProductId = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.ProductGreen50TaxDeduction, 0, this.ActorCompanyId, 0);
            bool getPurchasePriceFromInvoiceProduct = SettingManager.GetBoolSetting(entities, SettingMainType.Company, (int)CompanySettingType.GetPurchasePriceFromInvoiceProduct, this.UserId, this.ActorCompanyId, 0, true);
            int defaultStatusTransferredOrderToInvoice = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.BillingStatusTransferredOrderToInvoice, 0, this.ActorCompanyId, 0);
            int fixedPriceProductId = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.ProductFlatPrice, 0, this.ActorCompanyId, 0);
            int fixedPriceKeepPricesProductId = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.ProductFlatPriceKeepPrices, 0, this.ActorCompanyId, 0);

            // Get overhead cost rows
            var overheadRow = forecastRows.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.OverheadCost);
            var overheadRowPerHour = forecastRows.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.OverheadCostPerHour);

            // Get invoice rows
            var overviewRows = entitiesReadOnly.GetProjectOverview(projectId, false, false, base.ActorCompanyId).ToList();
            var rowsToIgnore = overviewRows.Where(i => i.TargetRowId != null).Select(i => (int)i.TargetRowId).ToList();

            // Get expense rows for project
            var expenseRows = ExpenseManager.GetExpenseRowsForProjectOverview(entities, base.ActorCompanyId, base.UserId, base.RoleId, 0, projectId);

            // Get timeinvoicetransactions
            var transactionItemsForProject = (from p in entitiesReadOnly.TimeInvoiceTransactionsProjectView
                                              where p.ProjectId == projectId
                                              select p).Where(t => t.TimeCodeTransactionType == (int)TimeCodeTransactionType.TimeProject && t.CustomerInvoiceRowId.HasValue).ToList();

            // Get timepayroltransactions
            var timeCodeTransactions = entities.GetTimeCodeTransactionsForProjectOverview(projectId, useProjectTimeBlock, false).ToList();

            #endregion

            #region Handle IB

            var budgetHeadIB = ProjectBudgetManager.GetLatestProjectBudgetHeadIncludingRows(projectId, DistributionCodeBudgetType.ProjectBudgetIB, true);
            if (budgetHeadIB != null)
            {
                foreach (var row in budgetHeadIB.Rows)
                {
                    BudgetRowProjectDTO forecastRow = forecastRows.Where(r => r.Type == row.Type && r.TimeCodeId == row.TimeCodeId).FirstOrDefault();
                    if (forecastRow == null)
                        forecastRow = forecastRows.Where(r => r.Type == row.Type).FirstOrDefault();
                    if (forecastRow == null)
                        continue;

                    forecastRow.TotalAmountResult += row.TotalAmount;
                    forecastRow.TotalQuantityResult += row.TotalQuantity;

                    if (row.Type == (int)ProjectCentralBudgetRowType.OverheadCost)
                    {
                        var budgetOverheadIBPerHourRow = budgetHeadIB.Rows.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.OverheadCostPerHour);
                        var budgetRowBillableMinutesQuantity = budgetHeadIB.Rows.Where(r => r.Type == (int)ProjectCentralBudgetRowType.CostPersonell).Sum(r => r.TotalQuantity);
                        if (budgetOverheadIBPerHourRow != null && budgetOverheadIBPerHourRow.TotalAmount > 0 && budgetRowBillableMinutesQuantity > 0)
                            forecastRow.TotalAmountResult += (budgetRowBillableMinutesQuantity / 60) * budgetOverheadIBPerHourRow.TotalAmount;
                    }

                    forecastRow.IsModified = true;
                }
            }

            #endregion

            #region Handle invoice rows

            // Handle invoice rows
            var invoicesForProject = overviewRows.GroupBy(i => i.InvoiceId).ToList();
            foreach (var invoiceGroup in invoicesForProject)
            {
                // Get an "invoice" to use
                var invoice = invoiceGroup.FirstOrDefault();

                foreach (var row in invoiceGroup.Where(r => r.ProductId == null || (r.ProductId.Value != ROTDeductionProductId && r.ProductId.Value != ROT50DeductionProductId && r.ProductId.Value != RUTDeductionProductId && r.ProductId.Value != Green15DeductionProductId && r.ProductId.Value != Green20DeductionProductId && r.ProductId.Value != Green50DeductionProductId)).ToList())
                {
                    #region Check dates and filter (unused)

                    // Do not include rows without dates
                    if (!row.Created.HasValue)
                        continue;

                    //Check for each row, not on the invoice head
                    /*if (dateFrom.HasValue || dateTo.HasValue)
                    {
                        // Do not include rows without dates
                        if (!row.Created.HasValue)
                            continue;

                        DateTime invRowCreatedDate;
                        if (invoice.RegistrationType == (int)OrderInvoiceRegistrationType.Order || invoice.OriginStatus == (int)SoeOriginStatus.Draft)
                        {
                            invRowCreatedDate = row.Date.HasValue ? row.Date.Value : row.Created.Value;

                            if (row.TargetRowId != null)
                            {
                                var targetRow = group.FirstOrDefault(r => r.CustomerInvoiceRowId == row.TargetRowId);
                                if (targetRow != null && targetRow.InvoiceDate.HasValue)
                                {
                                    invRowCreatedDate = targetRow.InvoiceDate.Value;

                                    if (projectBudgetHead.FromDate.HasValue && projectBudgetHead.FromDate.Value.Date > invRowCreatedDate.Date && row.CalculationType != (int)TermGroup_InvoiceProductCalculationType.Clearing && row.CalculationType != (int)TermGroup_InvoiceProductCalculationType.Lift)
                                    {
                                        continue;
                                    }
                                    else
                                    {
                                        if (useDateIntervalInIncomeNotInvoiced)
                                        {
                                            invRowCreatedDate = row.Date.HasValue ? row.Date.Value : row.Created.Value;

                                            if (row.ProductId != fixedPriceProductId && row.ProductId != fixedPriceKeepPricesProductId && (dateFrom.HasValue && dateFrom.Value.Date > invRowCreatedDate.Date || dateTo.HasValue && dateTo.Value.Date < invRowCreatedDate.Date))
                                                continue;
                                        }
                                    }
                                }
                                else
                                {
                                    if (useDateIntervalInIncomeNotInvoiced && row.ProductId != fixedPriceProductId && row.ProductId != fixedPriceKeepPricesProductId && (dateFrom.HasValue && dateFrom.Value.Date > invRowCreatedDate.Date || dateTo.HasValue && dateTo.Value.Date < invRowCreatedDate.Date))
                                        continue;
                                }
                            }
                            else
                            {
                                if (useDateIntervalInIncomeNotInvoiced && row.ProductId != fixedPriceProductId && row.ProductId != fixedPriceKeepPricesProductId && (dateFrom.HasValue && dateFrom.Value.Date > invRowCreatedDate.Date || dateTo.HasValue && dateTo.Value.Date < invRowCreatedDate.Date))
                                    continue;
                            }
                        }
                        else
                        {
                            invRowCreatedDate = invoice.InvoiceDate.Value;

                            if (dateFrom.HasValue && dateFrom.Value.Date > invRowCreatedDate.Date || dateTo.HasValue && dateTo.Value.Date < invRowCreatedDate.Date)
                                continue;
                        }
                    }*/

                    #endregion

                    #region CustomerInvoiceRow

                    decimal purchasePrice = row.PurchaseAmount.HasValue ? row.PurchaseAmount.Value : 0;

                    if (invoice.BillingType == (int)TermGroup_BillingType.Credit)
                        purchasePrice = Decimal.Negate(purchasePrice);

                    if (invoice.RegistrationType == (int)OrderInvoiceRegistrationType.Order)
                    {
                        if (row.TargetRowId == null) // Order rows not transferred to invoice
                        {
                            if (row.ProductId != null && (row.CalculationType == (int)TermGroup_InvoiceProductCalculationType.Clearing || row.CalculationType == (int)TermGroup_InvoiceProductCalculationType.Lift))
                                continue;

                            // Do not add fixed price rows?
                            /*if ((isFixedPriceOrder > 0 || isFixedPriceKeepPricesOrder > 0) && (row.ProductId.HasValue && ((row.ProductId.Value == fixedPriceProductId || row.ProductId.Value == fixedPriceKeepPricesProductId) || row.CalculationType == (int)TermGroup_InvoiceProductCalculationType.FixedPrice)))
                            {
                                var fixedDto = dtos.FirstOrDefault(d => d.AssociatedId == invoice.InvoiceId && d.Type == ProjectCentralBudgetRowType.FixedPriceTotal);
                                if (fixedDto != null)
                                {
                                    fixedDto.Value += row.SalesAmount.Value;
                                }
                                else
                                {
                                    var fixedPriceRowsExist = dtos.Any(d => d.Type == ProjectCentralBudgetRowType.FixedPriceTotal);
                                    dtos.Add(CreateProjectCentralStatusAmountValueRow(ProjectCentralHeaderGroupType.None, GetText(159, (int)TermGroup.ProjectCentral, "Fastpris"), ProjectCentralBudgetRowType.FixedPriceTotal, (SoeOriginType)invoice.OriginType, invoice.InvoiceId, "", "", "", fixedPriceRowsExist ? row.SalesAmount.Value : row.SalesAmount.Value + budgetIncomeIB, 0));
                                }
                            }*/

                            // Set date to use
                            DateTime invRowCreatedDate = row.Date.HasValue ? row.Date.Value.Date : row.Created.Value.Date;

                            /*if (invoice.OriginStatus != (int)SoeOriginStatus.OrderClosed && invoice.OriginStatus != (int)SoeOriginStatus.OrderFullyInvoice)
                            {
                                BudgetRowProjectDTO budgetRow = forecastRows.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.IncomeNotInvoiced);
                                if (budgetRow == null)
                                {
                                    budgetRow = CreateProjectBudgetRowFromTemplate(projectBudgetHead.Rows.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.IncomeNotInvoiced));
                                    if (budgetRow != null)
                                        forecastRows.Add(budgetRow);
                                }

                                if (row.SalesAmount.HasValue && row.SalesAmount.Value != 0)
                                {
                                    if(budgetRow.Periods.Count > 0)
                                    {
                                        var period = GetBudgetPeriodByDate(budgetRow, invRowCreatedDate);
                                        if(period != null)
                                            period.Amount += row.SalesAmount.Value;
                                    }

                                    budgetRow.TotalAmount += row.SalesAmount.Value;
                                }
                            }*/

                            if (row.ProductRowType != (int)SoeProductRowType.ExpenseRow)
                            {
                                if (row.ProductId.HasValue && row.ProductVatType == (int)TermGroup_InvoiceProductVatType.Service)
                                {
                                    if (row.IsTimeProjectRow.HasValue && !row.IsTimeProjectRow.Value && row.ProductRowType != (int)SoeProductRowType.ExpenseRow)
                                    {
                                        BudgetRowProjectDTO budgetRow = null;
                                        if (!String.IsNullOrEmpty(row.MaterialCode))
                                        {
                                            budgetRow = forecastRows.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.CostPersonell && r.TypeCodeName == row.MaterialCode);
                                            if (budgetRow == null)
                                            {
                                                budgetRow = forecastRows.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.CostPersonell);
                                                if (budgetRow != null)
                                                {
                                                    budgetRow.TotalAmountResult += purchasePrice;
                                                    budgetRow.TotalQuantityResult += row.Quantity.HasValue ? (row.Quantity.Value) : 0;
                                                    budgetRow.IsModified = true;
                                                }
                                            }
                                            else
                                            {
                                                budgetRow.TotalAmountResult += purchasePrice;
                                                budgetRow.TotalQuantityResult += row.Quantity.HasValue ? (row.Quantity.Value * 60) : 0;
                                                budgetRow.IsModified = true;
                                            }
                                        }
                                        else
                                        {
                                            budgetRow = forecastRows.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.CostPersonell);
                                            if (budgetRow != null)
                                            {
                                                budgetRow.TotalAmountResult += purchasePrice;
                                                budgetRow.TotalQuantityResult += row.Quantity.HasValue ? (row.Quantity.Value * 60) : 0;
                                                budgetRow.IsModified = true;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    BudgetRowProjectDTO budgetRow = null;
                                    if (!String.IsNullOrEmpty(row.MaterialCode))
                                    {
                                        budgetRow = forecastRows.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.CostMaterial && r.TypeCodeName == row.MaterialCode);
                                        if (budgetRow == null)
                                        {
                                            budgetRow = forecastRows.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.CostMaterial);
                                            if (budgetRow != null)
                                            {
                                                budgetRow.TotalAmountResult += purchasePrice;
                                                budgetRow.IsModified = true;
                                            }
                                        }
                                        else
                                        {
                                            budgetRow.TotalAmountResult += purchasePrice;
                                            budgetRow.IsModified = true;
                                        }
                                    }
                                    else
                                    {
                                        budgetRow = forecastRows.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.CostMaterial);
                                        if (budgetRow != null)
                                        {
                                            budgetRow.TotalAmountResult += purchasePrice;
                                            budgetRow.IsModified = true;
                                        }
                                    }
                                }
                            }
                        }// End of Order rows not transferred to invoice                                    
                        else // Order rows transferred to invoice
                        {
                            if (row.ProductRowType != (int)SoeProductRowType.ExpenseRow)
                            {
                                DateTime invRowCreatedDate = row.Date.HasValue ? row.Date.Value : row.Created.Value;
                                if (row.ProductId != null && row.ProductVatType == (int)TermGroup_InvoiceProductVatType.Service)
                                {
                                    if (!row.IsTimeProjectRow.HasValue || !row.IsTimeProjectRow.Value)
                                    {
                                        BudgetRowProjectDTO budgetRow = null;

                                        if (!String.IsNullOrEmpty(row.MaterialCode))
                                        {
                                            budgetRow = forecastRows.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.CostPersonell && r.TypeCodeName == row.MaterialCode);
                                            if (budgetRow == null)
                                            {
                                                budgetRow = forecastRows.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.CostPersonell);
                                                if (budgetRow != null)
                                                {
                                                    budgetRow.TotalAmountResult += purchasePrice;
                                                    budgetRow.TotalQuantity += row.Quantity.HasValue ? (row.Quantity.Value * 60) : 0;
                                                    budgetRow.IsModified = true;
                                                }
                                            }
                                            else
                                            {
                                                budgetRow.TotalAmountResult += purchasePrice;
                                                budgetRow.TotalQuantity += row.Quantity.HasValue ? (row.Quantity.Value * 60) : 0;
                                                budgetRow.IsModified = true;
                                            }
                                        }
                                        else
                                        {
                                            budgetRow = forecastRows.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.CostPersonell);
                                            if (budgetRow != null)
                                            {
                                                budgetRow.TotalAmountResult += purchasePrice;
                                                budgetRow.TotalQuantity += row.Quantity.HasValue ? (row.Quantity.Value * 60) : 0;
                                                budgetRow.IsModified = true;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    BudgetRowProjectDTO budgetRow = null;
                                    if (!String.IsNullOrEmpty(row.MaterialCode))
                                    {
                                        budgetRow = forecastRows.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.CostMaterial && r.TypeCodeName == row.MaterialCode);
                                        if (budgetRow == null)
                                        {
                                            budgetRow = forecastRows.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.CostMaterial);
                                            if (budgetRow != null)
                                            {
                                                budgetRow.TotalAmountResult += purchasePrice;
                                                budgetRow.IsModified = true;
                                            }
                                        }
                                        else
                                        {
                                            budgetRow.TotalAmountResult += purchasePrice;
                                            budgetRow.IsModified = true;
                                        }
                                    }
                                    else
                                    {
                                        budgetRow = forecastRows.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.CostMaterial);
                                        if (budgetRow != null)
                                        {
                                            budgetRow.TotalAmountResult += purchasePrice;
                                            budgetRow.IsModified = true;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else if (invoice.RegistrationType == (int)OrderInvoiceRegistrationType.Invoice)
                    {
                        if (invoice.OriginStatus == (int)SoeOriginStatus.Draft) //Invoice not invoiced! --- Preliminary invoices counts as not invoiced ---
                        {
                            DateTime invRowCreatedDate = row.Date?.Date ?? row.Created?.Date ?? DateTime.Today;

                            /*BudgetRowProjectDTO budgetRow = forecastRows.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.IncomeNotInvoiced);
                            if (budgetRow == null)
                            {
                                budgetRow = CreateProjectBudgetRowFromTemplate(projectBudgetHead.Rows.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.IncomeNotInvoiced));
                                if (budgetRow != null)
                                    forecastRows.Add(budgetRow);
                            }

                            if (row.SalesAmount.HasValue && row.SalesAmount.Value != 0)
                            {
                                if (budgetRow.Periods.Count > 0)
                                {
                                    var period = GetBudgetPeriodByDate(budgetRow, invRowCreatedDate);
                                    if (period != null)
                                        period.Amount += row.SalesAmount.Value;
                                }

                                budgetRow.TotalAmount += row.SalesAmount.Value;
                            }*/

                            if (row.ProductRowType != (int)SoeProductRowType.ExpenseRow)
                            {
                                if (!rowsToIgnore.Contains(row.CustomerInvoiceRowId.Value))
                                {
                                    if (row.ProductId != null && row.ProductVatType == (int)TermGroup_InvoiceProductVatType.Service)
                                    {
                                        if (row.IsTimeProjectRow.HasValue && !row.IsTimeProjectRow.Value && row.ProductRowType != (int)SoeProductRowType.ExpenseRow && invoice.BillingType == (int)TermGroup_BillingType.Debit)
                                        {
                                            BudgetRowProjectDTO budgetRow = null;

                                            if (!String.IsNullOrEmpty(row.MaterialCode))
                                            {
                                                budgetRow = forecastRows.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.CostPersonell && r.TypeCodeName == row.MaterialCode);
                                                if (budgetRow == null)
                                                {
                                                    budgetRow = forecastRows.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.CostPersonell);
                                                    if (budgetRow != null)
                                                    {
                                                        budgetRow.TotalAmountResult += purchasePrice;
                                                        budgetRow.TotalQuantityResult += row.Quantity.HasValue ? (row.Quantity.Value * 60) : 0;
                                                        budgetRow.IsModified = true;
                                                    }
                                                }
                                                else
                                                {
                                                    budgetRow.TotalAmountResult += purchasePrice;
                                                    budgetRow.TotalQuantityResult += row.Quantity.HasValue ? (row.Quantity.Value * 60) : 0;
                                                    budgetRow.IsModified = true;
                                                }
                                            }
                                            else
                                            {
                                                budgetRow = forecastRows.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.CostPersonell);
                                                if (budgetRow != null)
                                                {
                                                    budgetRow.TotalAmountResult += purchasePrice;
                                                    budgetRow.TotalQuantityResult += row.Quantity.HasValue ? (row.Quantity.Value * 60) : 0;
                                                    budgetRow.IsModified = true;
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        BudgetRowProjectDTO budgetRow = null;
                                        if (!String.IsNullOrEmpty(row.MaterialCode))
                                        {
                                            budgetRow = forecastRows.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.CostMaterial && r.TypeCodeName == row.MaterialCode);
                                            if (budgetRow == null)
                                            {
                                                budgetRow = forecastRows.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.CostMaterial);
                                                if (budgetRow != null)
                                                {
                                                    budgetRow.TotalAmountResult += purchasePrice;
                                                    budgetRow.IsModified = true;
                                                }
                                            }
                                            else
                                            {
                                                budgetRow.TotalAmountResult += purchasePrice;
                                                budgetRow.IsModified = true;
                                            }
                                        }
                                        else
                                        {
                                            budgetRow = forecastRows.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.CostMaterial);
                                            if (budgetRow != null)
                                            {
                                                budgetRow.TotalAmountResult += purchasePrice;
                                                budgetRow.IsModified = true;
                                            }
                                        }
                                    }
                                }
                            }
                        }//End of invoice not invoiced
                        else
                        {
                            BudgetRowProjectDTO budgetRow = null;
                            DateTime invRowCreatedDate = row.InvoiceDate.HasValue ? row.InvoiceDate.Value.Date : row.Created.Value.Date;

                            if (!row.IsTimeProjectRow.HasValue || !row.IsTimeProjectRow.Value)
                            {
                                if (!String.IsNullOrEmpty(row.MaterialCode))
                                {
                                    budgetRow = forecastRows.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.IncomeMaterialTotal && r.TypeCodeName == row.MaterialCode);
                                    if (budgetRow == null)
                                    {
                                        budgetRow = forecastRows.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.IncomeMaterialTotal);
                                        if (budgetRow != null)
                                        {
                                            budgetRow.TotalAmountResult += row.SalesAmount.HasValue ? row.SalesAmount.Value : 0;
                                            budgetRow.IsModified = true;
                                        }
                                    }
                                    else
                                    {
                                        budgetRow.TotalAmountResult += row.SalesAmount.HasValue ? row.SalesAmount.Value : 0;
                                        budgetRow.IsModified = true;
                                    }
                                }
                                else
                                {
                                    budgetRow = forecastRows.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.IncomeMaterialTotal);
                                    if (budgetRow != null)
                                    {
                                        budgetRow.TotalAmountResult += row.SalesAmount.HasValue ? row.SalesAmount.Value : 0;
                                        budgetRow.IsModified = true;
                                    }
                                }
                            }

                            if (row.ProductRowType != (int)SoeProductRowType.ExpenseRow)
                            {
                                if (!rowsToIgnore.Contains(row.CustomerInvoiceRowId.Value))
                                {
                                    if (row.ProductId != null && row.ProductVatType == (int)TermGroup_InvoiceProductVatType.Service)
                                    {
                                        if (row.IsTimeProjectRow.HasValue && !row.IsTimeProjectRow.Value && row.ProductRowType != (int)SoeProductRowType.ExpenseRow && invoice.BillingType == (int)TermGroup_BillingType.Debit)
                                        {
                                            budgetRow = null;

                                            if (!String.IsNullOrEmpty(row.MaterialCode))
                                            {
                                                budgetRow = forecastRows.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.CostPersonell && r.TypeCodeName == row.MaterialCode);
                                                if (budgetRow != null)
                                                {
                                                    budgetRow = forecastRows.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.CostPersonell);
                                                    if (budgetRow != null)
                                                    {
                                                        budgetRow.TotalAmountResult += purchasePrice;
                                                        budgetRow.TotalQuantityResult += row.Quantity.HasValue ? (row.Quantity.Value * 60) : 0;
                                                        budgetRow.IsModified = true;
                                                    }
                                                }
                                                else
                                                {
                                                    budgetRow.TotalAmountResult += purchasePrice;
                                                    budgetRow.TotalQuantityResult += row.Quantity.HasValue ? (row.Quantity.Value * 60) : 0;
                                                    budgetRow.IsModified = true;
                                                }
                                            }
                                            else
                                            {
                                                budgetRow = forecastRows.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.CostPersonell);
                                                if (budgetRow != null)
                                                {
                                                    budgetRow.TotalAmountResult += purchasePrice;
                                                    budgetRow.TotalQuantityResult += row.Quantity.HasValue ? (row.Quantity.Value * 60) : 0;
                                                    budgetRow.IsModified = true;
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        budgetRow = null;
                                        if (!String.IsNullOrEmpty(row.MaterialCode))
                                        {
                                            budgetRow = forecastRows.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.CostMaterial && r.TypeCodeName == row.MaterialCode);
                                            if (budgetRow == null)
                                            {
                                                budgetRow = forecastRows.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.CostMaterial);
                                                if (budgetRow != null)
                                                {
                                                    budgetRow.TotalAmountResult += purchasePrice;
                                                    budgetRow.IsModified = true;
                                                }
                                            }
                                            else
                                            {
                                                budgetRow.TotalAmountResult += purchasePrice;
                                                budgetRow.IsModified = true;
                                            }
                                        }
                                        else
                                        {
                                            budgetRow = forecastRows.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.CostMaterial);
                                            if (budgetRow != null)
                                            {
                                                budgetRow.TotalAmountResult += purchasePrice;
                                                budgetRow.IsModified = true;
                                            }
                                        }
                                    }
                                }
                            }
                        } //End of Invoice invoiced!
                    }

                    #endregion
                }
            }

            #endregion

            #region ExpenseRowTransactionView

            if (ExpenseManager.HasExpenseRows(base.ActorCompanyId))
            {
                foreach (var expenseRow in expenseRows)
                {
                    BudgetRowProjectDTO budgetRow = null;
                    if (!String.IsNullOrEmpty(expenseRow.TimeCodeName))
                    {
                        budgetRow = forecastRows.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.CostExpense && r.TypeCodeName == expenseRow.TimeCodeName);
                        if (budgetRow == null)
                        {
                            budgetRow = forecastRows.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.CostExpense);
                            if (budgetRow != null)
                            {
                                budgetRow.TotalAmountResult += (expenseRow.IsSpecifiedUnitPrice ? expenseRow.Amount : expenseRow.PayrollAmount);
                                if (expenseRow.TimeCodeRegistrationType == (int)TermGroup_TimeCodeRegistrationType.Time)
                                    budgetRow.TotalQuantityResult += expenseRow.Quantity;
                                budgetRow.IsModified = true;
                            }
                        }
                        else
                        {
                            budgetRow.TotalAmountResult += (expenseRow.IsSpecifiedUnitPrice ? expenseRow.Amount : expenseRow.PayrollAmount);
                            if (expenseRow.TimeCodeRegistrationType == (int)TermGroup_TimeCodeRegistrationType.Time)
                                budgetRow.TotalQuantityResult += expenseRow.Quantity;
                            budgetRow.IsModified = true;
                        }
                    }
                    else
                    {
                        budgetRow = forecastRows.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.CostExpense);
                        if (budgetRow != null)
                        {
                            budgetRow.TotalAmountResult += (expenseRow.IsSpecifiedUnitPrice ? expenseRow.Amount : expenseRow.PayrollAmount);
                            if (expenseRow.TimeCodeRegistrationType == (int)TermGroup_TimeCodeRegistrationType.Time)
                                budgetRow.TotalQuantityResult += expenseRow.Quantity;
                            budgetRow.IsModified = true;
                        }
                    }
                }
            }

            #endregion

            #region TimeInvoiceTransactionsProjectView

            foreach (var transactionGroup in transactionItemsForProject.GroupBy(t => t.CustomerInvoiceRowId))
            {
                bool isBilled = false;
                //bool targetRowMissing = false;
                decimal transactionQuantity = 0;

                var customerInvoiceRow = overviewRows.FirstOrDefault(r => r.CustomerInvoiceRowId == transactionGroup.Key);

                if (customerInvoiceRow == null)
                    continue;

                var invoiceGroup = invoicesForProject.FirstOrDefault(p => p.Key == customerInvoiceRow.InvoiceId);

                if (invoiceGroup == null)
                    continue;

                GetProjectOverview_Result targetRow = null;
                if (customerInvoiceRow.AttestStateId.HasValue && customerInvoiceRow.AttestStateId == defaultStatusTransferredOrderToInvoice && customerInvoiceRow.TargetRowId.HasValue)
                {
                    targetRow = overviewRows.FirstOrDefault(r => r.CustomerInvoiceRowId == customerInvoiceRow.TargetRowId);

                    if (targetRow == null || targetRow.OriginStatus == (int)SoeOriginStatus.Draft)
                        continue;
                    else
                        isBilled = true;

                    var isFixedPriceOrder = false;
                    if (fixedPriceProductId != 0)
                        isFixedPriceOrder = invoiceGroup.Any(r => r.ProductId == fixedPriceProductId && fixedPriceProductId != 0);

                    var isFixedPriceKeepPricesOrder = false;
                    if (fixedPriceKeepPricesProductId != 0)
                        isFixedPriceKeepPricesOrder = invoiceGroup.Any(r => r.ProductId == fixedPriceKeepPricesProductId);

                    DateTime? invRowCreatedDate = null;
                    foreach (var transactionItem in transactionGroup)
                    {
                        decimal quantity = transactionItem.InvoiceQuantity;
                        invRowCreatedDate = targetRow.InvoiceDate.HasValue ? targetRow.InvoiceDate.Value.Date : targetRow.Created.Value.Date;

                        if (customerInvoiceRow.BillingType == (int)TermGroup_BillingType.Credit)
                            quantity = decimal.Negate(quantity);

                        transactionQuantity += quantity;

                        if (transactionItem.Exported && targetRow != null)
                            isBilled = true;

                        if (customerInvoiceRow.OriginType == (int)SoeOriginType.Order)
                        {
                            if (isBilled)
                            {
                                var amount = 0m;
                                if (targetRow.SalesAmount.HasValue && targetRow.SalesAmount.Value != 0 && targetRow.Quantity.HasValue && targetRow.Quantity != 0 && transactionItem.InvoiceQuantity != 0)
                                    amount = (targetRow.SalesAmount.Value / (targetRow.Quantity.HasValue ? (decimal)targetRow.Quantity : 0)) * (transactionItem.InvoiceQuantity / 60);

                                BudgetRowProjectDTO budgetRow = null;
                                if (!String.IsNullOrEmpty(transactionItem.TimeCodeName))
                                {
                                    budgetRow = forecastRows.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.IncomePersonellTotal && r.TypeCodeName == transactionItem.TimeCodeName);
                                    if (budgetRow == null)
                                    {
                                        budgetRow = forecastRows.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.IncomePersonellTotal);
                                        if (budgetRow != null)
                                        {
                                            budgetRow.TotalAmountResult += amount;
                                            budgetRow.IsModified = true;
                                        }
                                    }
                                    else
                                    {
                                        budgetRow.TotalAmountResult += amount;
                                        budgetRow.IsModified = true;
                                    }
                                }
                                else
                                {
                                    budgetRow = forecastRows.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.IncomePersonellTotal);
                                    if (budgetRow != null)
                                    {
                                        budgetRow.TotalAmountResult += amount;
                                        budgetRow.IsModified = true;
                                    }
                                }
                            }
                        }
                        else if (customerInvoiceRow.OriginType == (int)SoeOriginType.CustomerInvoice)
                        {
                            var amount = ((customerInvoiceRow.SalesAmount.HasValue ? customerInvoiceRow.SalesAmount.Value : 0) / (customerInvoiceRow.Quantity.HasValue ? (decimal)customerInvoiceRow.Quantity : 0)) * (transactionItem.InvoiceQuantity / 60);
                            if (customerInvoiceRow.OriginStatus == (int)SoeOriginStatus.Draft)
                            {
                                /*BudgetRowProjectDTO budgetRow = forecastRows.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.IncomeNotInvoiced);
                                if (budgetRow == null)
                                {
                                    budgetRow = CreateProjectBudgetRowFromTemplate(projectBudgetHead.Rows.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.IncomeNotInvoiced));
                                    if (budgetRow != null)
                                        forecastRows.Add(budgetRow);
                                }

                                if (amount != 0)
                                {
                                    if (budgetRow.Periods.Count > 0)
                                    {
                                        var period = GetBudgetPeriodByDate(budgetRow, invRowCreatedDate.Value);
                                        if (period != null)
                                            period.Amount += amount;
                                    }

                                    budgetRow.TotalAmount += amount;
                                }*/
                            }
                            else
                            {
                                BudgetRowProjectDTO budgetRow = null;
                                if (!String.IsNullOrEmpty(transactionItem.TimeCodeName))
                                {
                                    budgetRow = forecastRows.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.IncomePersonellTotal && r.TypeCodeName == transactionItem.TimeCodeName);
                                    if (budgetRow == null)
                                    {
                                        budgetRow = forecastRows.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.IncomePersonellTotal);
                                        if (budgetRow != null)
                                        {
                                            budgetRow.TotalAmountResult += amount;
                                            budgetRow.IsModified = true;
                                        }
                                    }
                                    else
                                    {
                                        budgetRow.TotalAmountResult += amount;
                                        budgetRow.IsModified = true;
                                    }
                                }
                                else
                                {
                                    budgetRow = forecastRows.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.IncomePersonellTotal);
                                    if (budgetRow != null)
                                    {
                                        budgetRow.TotalAmountResult += amount;
                                        budgetRow.IsModified = true;
                                    }
                                }
                            }
                        }
                    }

                    // Add rests
                    var rowQuantity = customerInvoiceRow.Quantity.GetValueOrDefault();
                    var restQuantity = rowQuantity - (transactionQuantity / 60);
                    if (restQuantity != 0)
                    {
                        // Add rows for total
                        var salesAmount = customerInvoiceRow.SalesAmount.GetValueOrDefault();
                        var amount = rowQuantity != 0 ? (salesAmount / rowQuantity) * restQuantity : salesAmount * restQuantity;
                        if (amount != 0)
                        {
                            if ((customerInvoiceRow.OriginType == (int)SoeOriginType.Order && isBilled && targetRow.OriginStatus != (int)SoeOriginStatus.Draft) || (customerInvoiceRow.OriginType == (int)SoeOriginType.CustomerInvoice && targetRow.OriginStatus != (int)SoeOriginStatus.Draft))
                            {
                                BudgetRowProjectDTO budgetRow = budgetRow = forecastRows.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.IncomePersonellTotal);
                                if (budgetRow != null)
                                {
                                    budgetRow.TotalAmountResult += amount;
                                    budgetRow.IsModified = true;
                                }
                            }
                            /*else
                            {
                                BudgetRowProjectDTO budgetRow = forecastRows.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.IncomeNotInvoiced);
                                if (budgetRow == null)
                                {
                                    budgetRow = CreateProjectBudgetRowFromTemplate(projectBudgetHead.Rows.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.IncomeNotInvoiced));
                                    if (budgetRow != null)
                                        forecastRows.Add(budgetRow);
                                }

                                if (amount != 0)
                                {
                                    if (budgetRow.Periods.Count > 0)
                                    {
                                        var period = GetBudgetPeriodByDate(budgetRow, invRowCreatedDate.HasValue ? invRowCreatedDate.Value : DateTime.Today);
                                        if (period != null)
                                            period.Amount += amount;
                                    }

                                    budgetRow.TotalAmount += amount;
                                }
                            }*/
                        }
                    }
                }
            }

            #endregion

            #region TimeCodeTransactions
            ProjectCentralManager projectCentralManager = new ProjectCentralManager(this.parameterObject);

            foreach (var timeCodeTrans in timeCodeTransactions)
            {
                if (timeCodeTrans.SupplierInvoiceId.HasValue && timeCodeTrans.AmountCurrency.HasValue && timeCodeTrans.InvoiceQuantity.HasValue)
                {
                    bool doNotCharge = (timeCodeTrans.DoNotChargeProject != null && (bool)timeCodeTrans.DoNotChargeProject);
                    if (doNotCharge)
                        continue;

                    DateTime date = timeCodeTrans.SupplierInvoiceDate.HasValue ? timeCodeTrans.SupplierInvoiceDate.Value : timeCodeTrans.SupplierInvoiceCreated.Value;

                    if (timeCodeTrans.TimeCodeMaterialId.HasValue)
                    {
                        decimal costMat = doNotCharge ? 0 : (timeCodeTrans.AmountCurrency.Value * timeCodeTrans.InvoiceQuantity.Value);

                        BudgetRowProjectDTO budgetRow = forecastRows.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.CostMaterial);
                        if (budgetRow != null)
                        {
                            budgetRow.TotalAmountResult += costMat;
                            budgetRow.IsModified = true;
                        }
                    }
                    else
                    {
                        decimal costExp = doNotCharge ? 0 : (timeCodeTrans.AmountCurrency.Value * timeCodeTrans.InvoiceQuantity.Value);

                        BudgetRowProjectDTO budgetRow = budgetRow = forecastRows.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.CostExpense);
                        if (budgetRow != null)
                        {
                            budgetRow.TotalAmountResult += costExp;
                            budgetRow.IsModified = true;
                        }
                    }
                }
                else
                {
                    DateTime date = timeCodeTrans.SupplierInvoiceDate.HasValue ? timeCodeTrans.SupplierInvoiceDate.Value : timeCodeTrans.SupplierInvoiceCreated.Value;

                    if (!useProjectTimeBlock)
                    {
                        #region TimePayrollTransaction - when useProjectTimeBlock is false

                        if (timeCodeTrans.TimeCodeWorkId.HasValue)
                        {
                            decimal invoiceProductCost = 0;
                            if (!getPurchasePriceFromInvoiceProduct && timeCodeTrans.InvoiceRowPurchasePrice != 0)
                                invoiceProductCost = timeCodeTrans.InvoiceRowPurchasePrice;
                            else
                                invoiceProductCost = timeCodeTrans.PurchasePrice.HasValue ? timeCodeTrans.PurchasePrice.Value : 0;

                            // Personnel
                            /*string employeeText = string.Empty;
                            if (timeCodeTrans.Name != "")
                                employeeText = ", " + timeCodeTrans.Name;*/

                            string info = string.Empty;
                            decimal quantity = (timeCodeTrans.TransactionQuantity.HasValue && timeCodeTrans.TransactionQuantity.Value > 0 ? timeCodeTrans.TransactionQuantity.Value : 0);

                            if (timeCodeTrans.CustomerInvoiceBillingType == (int)TermGroup_BillingType.Credit)
                                quantity = decimal.Negate(quantity);

                            decimal price = timeCodeTrans.UseCalculatedCost.HasValue && timeCodeTrans.UseCalculatedCost.Value ? projectCentralManager.GetCalculatedCost(timeCodeTrans.EmployeeId, base.ActorCompanyId, date, projectId) : invoiceProductCost;
                            decimal cost = (quantity != 0 ? quantity / 60 : 0) * price;

                            BudgetRowProjectDTO budgetRow = null;

                            if (!String.IsNullOrEmpty(timeCodeTrans.TimeCodeName))
                            {
                                budgetRow = forecastRows.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.CostPersonell && r.TypeCodeName == timeCodeTrans.TimeCodeName);
                                if (budgetRow == null)
                                {
                                    budgetRow = forecastRows.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.CostPersonell);
                                    if (budgetRow != null)
                                    {
                                        budgetRow.TotalAmountResult += cost;
                                        budgetRow.TotalQuantityResult += quantity;
                                        budgetRow.IsModified = true;
                                    }
                                }
                                else
                                {
                                    budgetRow.TotalAmountResult += cost;
                                    budgetRow.TotalQuantityResult += quantity;
                                    budgetRow.IsModified = true;
                                }
                            }
                            else
                            {
                                budgetRow = forecastRows.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.CostPersonell);
                                if (budgetRow != null)
                                {
                                    budgetRow.TotalAmountResult += cost;
                                    budgetRow.TotalQuantityResult += quantity;
                                    budgetRow.IsModified = true;
                                }
                            }

                            if (overheadRow != null && overheadRowPerHour != null && overheadRowPerHour.TotalAmount > 0 && quantity != 0)
                            {
                                overheadRow.TotalAmountResult += (overheadRowPerHour.TotalAmount * (quantity / 60));
                                overheadRow.IsModified = true;
                            }
                        }
                        #endregion
                    }
                    else
                    {
                        #region ProjectTimeBlock - when useProjectTimeBlock is true

                        if (timeCodeTrans.TimeCodeWorkId.HasValue)
                        {
                            decimal invoiceProductCost = 0;
                            if (!getPurchasePriceFromInvoiceProduct && timeCodeTrans.InvoiceRowPurchasePrice != 0)
                                invoiceProductCost = timeCodeTrans.InvoiceRowPurchasePrice;
                            else
                                invoiceProductCost = timeCodeTrans.PurchasePrice.HasValue ? timeCodeTrans.PurchasePrice.Value : 0;

                            // Personnel
                            /*string employeeText = string.Empty;
                            if (timeCodeTrans.Name != "")
                                employeeText = ", " + timeCodeTrans.Name;*/

                            string info = string.Empty;
                            decimal quantity = Convert.ToDecimal(CalendarUtility.TimeSpanToMinutes(timeCodeTrans.Stop.Value, timeCodeTrans.Start.Value));

                            if (timeCodeTrans.CustomerInvoiceBillingType == (int)TermGroup_BillingType.Credit)
                                quantity = decimal.Negate(quantity);

                            decimal price = timeCodeTrans.UseCalculatedCost.HasValue && timeCodeTrans.UseCalculatedCost.Value ? projectCentralManager.GetCalculatedCost(timeCodeTrans.EmployeeId, base.ActorCompanyId, timeCodeTrans.SupplierInvoiceDate.Value.Date, projectId) : invoiceProductCost;
                            decimal cost = (quantity != 0 ? quantity / 60 : 0) * price;

                            BudgetRowProjectDTO budgetRow = null;

                            if (!String.IsNullOrEmpty(timeCodeTrans.TimeCodeName))
                            {
                                budgetRow = forecastRows.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.CostPersonell && r.TypeCodeName == timeCodeTrans.TimeCodeName);
                                if (budgetRow == null)
                                {
                                    budgetRow = forecastRows.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.CostPersonell);
                                    if (budgetRow != null)
                                    {
                                        budgetRow.TotalAmountResult += cost;
                                        budgetRow.TotalQuantityResult += quantity;
                                        budgetRow.IsModified = true;
                                    }
                                }
                                else
                                {
                                    budgetRow.TotalAmountResult += cost;
                                    budgetRow.TotalQuantityResult += quantity;
                                    budgetRow.IsModified = true;
                                }
                            }
                            else
                            {
                                budgetRow = forecastRows.FirstOrDefault(r => r.Type == (int)ProjectCentralBudgetRowType.CostPersonell);
                                if (budgetRow != null)
                                {
                                    budgetRow.TotalAmountResult += cost;
                                    budgetRow.TotalQuantityResult += quantity;
                                    budgetRow.IsModified = true;
                                }
                            }

                            if (overheadRow != null && overheadRowPerHour != null && overheadRowPerHour.TotalAmount > 0 && quantity != 0)
                            {
                                overheadRow.TotalAmountResult += (overheadRowPerHour.TotalAmount * (quantity / 60));
                                overheadRow.IsModified = true;
                            }
                        }
                        #endregion
                    }
                }
            }

            #endregion

            return forecastRows;

            #endregion
        }

        #region Period handling, not used for now

            /*private BudgetRowProjectDTO CreateProjectBudgetRowFromTemplate(BudgetRowProjectDTO templateRow)
            {
                BudgetRowProjectDTO newRow = new BudgetRowProjectDTO
                { 
                    Type = templateRow.Type,
                    TotalAmount = 0,
                    TotalQuantity = 0,
                    Comment = GetText(2303, (int)TermGroup.General, "Utfall"),
                    TimeCodeId = templateRow.TimeCodeId,

                    Periods = new List<BudgetPeriodProjectDTO>(),
                };

                foreach (var period in templateRow.Periods)
                {
                    var newPeriod = new BudgetPeriodProjectDTO
                    {
                        PeriodNr = period.PeriodNr,
                        Amount = 0,
                        Quantity = 0,
                        StartDate = period.StartDate,
                    };
                    newRow.Periods.Add(newPeriod);
                }

                return newRow;
            }

            private BudgetPeriodProjectDTO GetBudgetPeriodByDate(BudgetRowProjectDTO budgetRow, DateTime date)
            {
                if(date.Date < budgetRow.Periods[0].StartDate)
                    return budgetRow.Periods[0];

                if (date.Date > budgetRow.Periods[budgetRow.Periods.Count - 1].StartDate)
                    return budgetRow.Periods[0];

                BudgetPeriodProjectDTO period = null;
                for (int i = 0; i < budgetRow.Periods.Count; i++)
                {
                    if (date.Date >= budgetRow.Periods[i].StartDate && date.Date >= budgetRow.Periods[i + 1].StartDate)
                    {
                        period = budgetRow.Periods[i];
                    }
                }
                return period;
            }*/

            #endregion

        #endregion

        #region ChangeLog

        public List<BudgetRowProjectChangeLogDTO> GetProjectBudgetChangeLogPerRow(int budgetRowId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetProjectBudgetChangeLogPerRow(entities, budgetRowId);
        }

        public List<BudgetRowProjectChangeLogDTO> GetProjectBudgetChangeLogPerRow(CompEntities entities, int budgetRowId)
        {
            return (from r in entities.BudgetRowChangeLog
                       where r.BudgetRowId == budgetRowId
                    orderby r.Created descending
                    select new BudgetRowProjectChangeLogDTO()
                       {
                           BudgetRowChangeLogId = r.BudgetRowChangeLogId,
                           BudgetRowId = r.BudgetRowId,
                           Created = r.Created,
                           FromTotalAmount = r.FromTotalAmount,
                           ToTotalAmount = r.ToTotalAmount,
                           TotalAmountDiff = r.ToTotalAmount - r.FromTotalAmount,
                           FromTotalQuantity = r.FromTotalQuantity,
                           ToTotalQuantity = r.ToTotalQuantity,
                           TotalQuantityDiff = r.ToTotalQuantity - r.FromTotalQuantity,
                           CreatedBy = r.CreatedBy,
                           Comment = r.Comment,
                       }).ToList();
        }

        #endregion

        #region Migrate

        public List<int> GetBudgetHeadsToMigrateForCompany(int actorCompanyid)
        {
                using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
                return (from br in entities.BudgetRow
                                    where br.BudgetHead.Project.ActorCompanyId == actorCompanyid &&
                                            (br.BudgetHead.Type == 0 || br.BudgetHead.Type == (int)DistributionCodeBudgetType.ProjectBudget) &&
                                            (br.TotalAmount != 0 || br.TotalQuantity != 0) &&
                                            br.State == (int)SoeEntityState.Active &&
                                            br.BudgetHead.State == (int)SoeEntityState.Active &&
                                            br.BudgetHead.Project.State == (int)SoeEntityState.Active
                                   select br.BudgetHeadId).Distinct().ToList();
        }

        public ActionResult MigrateBudgetHeadIncludingRows(int budgetHeadId, bool deleteOrigin = true)
        {
            ActionResult result = new ActionResult();

            try
            {
                using (CompEntities entities = new CompEntities())
                {
                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        bool overheadCostAsAmountPerHour = SettingManager.GetBoolSetting(entities, SettingMainType.Company, (int)CompanySettingType.ProjectOverheadCostAsAmountPerHour, 0, this.ActorCompanyId, 0);
                        bool overheadCostAsFixedAmount = SettingManager.GetBoolSetting(entities, SettingMainType.Company, (int)CompanySettingType.ProjectOverheadCostAsFixedAmount, 0, this.ActorCompanyId, 0);

                        var budgetHead = GetProjectBudgetHeadIncludingRows(entities, budgetHeadId);
                        if (budgetHead == null)
                            return new ActionResult(false, (int)ActionResultSave.EntityIsNull, GetText(7044, "Felaktig inparameter"));

                        BudgetHeadProjectDTO migratedBudgetHead = budgetHead.CloneDTO();
                        migratedBudgetHead.BudgetHeadId = 0;
                        migratedBudgetHead.Name += " - " + GetText(7148, "Budget");
                        migratedBudgetHead.Type = (int)DistributionCodeBudgetType.ProjectBudgetExtended;
                        migratedBudgetHead.Status = (int)BudgetHeadStatus.Preliminary;

                        migratedBudgetHead.Rows = new List<BudgetRowProjectDTO>();

                        BudgetHeadProjectDTO migratedBudgetHeadIB = null;
                        if (budgetHead.Rows.Any(r => r.Type.Value >= 30 && r.TotalAmount != 0))
                        {
                            migratedBudgetHeadIB = budgetHead.CloneDTO();
                            migratedBudgetHeadIB.BudgetHeadId = 0;
                            migratedBudgetHeadIB.Name += " - " + GetText(1391, "Ingående balans");
                            migratedBudgetHeadIB.Type = (int)DistributionCodeBudgetType.ProjectBudgetIB;
                            migratedBudgetHeadIB.Status = (int)BudgetHeadStatus.Preliminary;

                            migratedBudgetHeadIB.Rows = new List<BudgetRowProjectDTO>();
                        }

                        // Handle rows for main budget head
                        foreach (var row in budgetHead.Rows.Where(r => r.Type < (int)ProjectCentralBudgetRowType.BillableMinutesInvoicedIB).OrderBy(r => r.Type).ThenBy(r => r.TimeCodeId))
                        {
                            if (row.Type.HasValue && (row.Type.Value == (int)ProjectCentralBudgetRowType.BillableMinutesInvoiced || row.Type.Value == (int)ProjectCentralBudgetRowType.BillableMinutesNotInvoiced || row.Type.Value == (int)ProjectCentralBudgetRowType.BillableMinutesTotal ||
                                row.Type.Value == (int)ProjectCentralBudgetRowType.IncomeTotal || row.Type.Value == (int)ProjectCentralBudgetRowType.CostTotal || row.Type.Value == (int)ProjectCentralBudgetRowType.BillableMinutesTotal))
                                continue;

                            BudgetRowProjectDTO migratedRow = row.CloneDTO();
                            migratedRow.BudgetRowId = 0;

                            if (migratedRow.Type == (int)ProjectCentralBudgetRowType.CostPersonell && row.TimeCodeId != 0)
                            {
                                if (migratedRow.TotalQuantity == 0)
                                {
                                    var sum = budgetHead.Rows.Where(r => r.Type == (int)ProjectCentralBudgetRowType.BillableMinutesInvoiced).Sum(r => r.TotalQuantity);
                                    migratedRow.TotalQuantity = sum;
                                }
                                else
                                {
                                    migratedRow.TotalQuantity = migratedRow.TotalQuantity;
                                }
                            }

                            migratedRow.IsLocked = migratedRow.TimeCodeId == 0;
                            migratedBudgetHead.Rows.Add(migratedRow);
                        }

                        if(!migratedBudgetHead.Rows.Any(r => r.Type == (int)ProjectCentralBudgetRowType.OverheadCostPerHour))
                        {
                            // Add overhead cost per hour row with 0 amount
                            BudgetRowProjectDTO overheadCostPerHourRow = new BudgetRowProjectDTO
                            {
                                Type = (int)ProjectCentralBudgetRowType.OverheadCostPerHour,
                                TotalAmount = 0,
                                TotalQuantity = 0,
                                IsLocked = true,
                                CreatedBy = GetUserDetails(),
                                Created = DateTime.Now
                            };
                            migratedBudgetHead.Rows.Add(overheadCostPerHourRow);
                        }

                        if(!migratedBudgetHead.Rows.Any(r => r.Type == (int)ProjectCentralBudgetRowType.OverheadCost))
                        {
                            // Add overhead cost row with 0 amount
                            BudgetRowProjectDTO overheadCostRow = new BudgetRowProjectDTO
                            {
                                Type = (int)ProjectCentralBudgetRowType.OverheadCost,
                                TotalAmount = 0,
                                TotalQuantity = 0,
                                IsLocked = true,
                                CreatedBy = GetUserDetails(),
                                Created = DateTime.Now
                            };
                            migratedBudgetHead.Rows.Add(overheadCostRow);
                        }

                        if (migratedBudgetHeadIB != null)
                        {
                            // Handle rows for incoming balances budget head
                            foreach (var row in budgetHead.Rows.Where(r => r.Type >= (int)ProjectCentralBudgetRowType.BillableMinutesInvoicedIB).OrderBy(r => r.Type).ThenBy(r => r.TimeCodeId))
                            {
                                if (row.Type.HasValue && (row.Type.Value == (int)ProjectCentralBudgetRowType.BillableMinutesInvoicedIB || row.Type.Value == (int)ProjectCentralBudgetRowType.IncomeTotalIB || row.Type.Value == (int)ProjectCentralBudgetRowType.CostTotalIB))
                                    continue;

                                BudgetRowProjectDTO migratedRow = row.CloneDTO();
                                migratedRow.BudgetRowId = 0;
                                migratedRow.Type = (int)GetBudgetRowType((ProjectCentralBudgetRowType)row.Type.Value);

                                if (migratedRow.Type == (int)ProjectCentralBudgetRowType.CostPersonell && row.TimeCodeId == 0)
                                {
                                    if (migratedRow.TotalQuantity == 0)
                                    {
                                        var sum = budgetHead.Rows.Where(r => r.Type == (int)ProjectCentralBudgetRowType.BillableMinutesInvoicedIB).Sum(r => r.TotalAmount);
                                        migratedRow.TotalQuantity = sum;
                                    }
                                    else
                                    {
                                        migratedRow.TotalQuantity = migratedRow.TotalQuantity;
                                    }
                                }

                                migratedRow.IsLocked = migratedRow.TimeCodeId == 0;
                                migratedBudgetHeadIB.Rows.Add(migratedRow);
                            }

                            if (!migratedBudgetHeadIB.Rows.Any(r => r.Type == (int)ProjectCentralBudgetRowType.OverheadCostPerHour))
                            {
                                // Add overhead cost per hour row with 0 amount
                                BudgetRowProjectDTO overheadCostPerHourRow = new BudgetRowProjectDTO
                                {
                                    Type = (int)ProjectCentralBudgetRowType.OverheadCostPerHour,
                                    TotalAmount = 0,
                                    TotalQuantity = 0,
                                    IsLocked = true,
                                    CreatedBy = GetUserDetails(),
                                    Created = DateTime.Now
                                };
                                migratedBudgetHeadIB.Rows.Add(overheadCostPerHourRow);
                            }

                            if (!migratedBudgetHeadIB.Rows.Any(r => r.Type == (int)ProjectCentralBudgetRowType.OverheadCost))
                            {
                                // Add overhead cost row with 0 amount
                                BudgetRowProjectDTO overheadCostRow = new BudgetRowProjectDTO
                                {
                                    Type = (int)ProjectCentralBudgetRowType.OverheadCost,
                                    TotalAmount = 0,
                                    TotalQuantity = 0,
                                    IsLocked = true,
                                    CreatedBy = GetUserDetails(),
                                    Created = DateTime.Now
                                };
                                migratedBudgetHeadIB.Rows.Add(overheadCostRow);
                            }
                        }

                        if (migratedBudgetHeadIB != null)
                            result = SaveBudgetHeadForProject(transaction, entities, migratedBudgetHeadIB, true, migratedBudgetHeadIB.ActorCompanyId);

                        if (result.Success)
                            result = SaveBudgetHeadForProject(transaction, entities, migratedBudgetHead, true, migratedBudgetHead.ActorCompanyId);

                        if (result.Success && deleteOrigin)
                        {
                            var budgetsToRemove = (from bh in entities.BudgetHead
                                                   where bh.ProjectId == budgetHead.ProjectId &&
                                                         (bh.Type == 0 ||
                                                         bh.Type == (int)DistributionCodeBudgetType.ProjectBudget) &&
                                                         bh.State == (int)SoeEntityState.Active
                                                   select bh.BudgetHeadId).ToList();

                            foreach (var headId in budgetsToRemove)
                            {
                                BudgetManager.DeleteBudgetHead(entities, headId);
                            }
                        }
                        
                        //Commit transaction
                        if (result.Success)
                        {
                            transaction.Complete();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = GetText(5265, (int)TermGroup.AngularBilling, "billing.projects.budget.migrationfailed");

                base.LogError(ex, this.log);
            }

            return result;
        }

        private ProjectCentralBudgetRowType GetBudgetRowType(ProjectCentralBudgetRowType budgetRowType)
        {
            switch (budgetRowType)
            {
                case ProjectCentralBudgetRowType.IncomePersonellTotalIB:
                    return ProjectCentralBudgetRowType.IncomePersonellTotal;
                case ProjectCentralBudgetRowType.IncomeMaterialTotalIB:
                    return ProjectCentralBudgetRowType.IncomeMaterialTotal;
                case ProjectCentralBudgetRowType.CostPersonellIB:
                    return ProjectCentralBudgetRowType.CostPersonell;
                case ProjectCentralBudgetRowType.CostMaterialIB:
                    return ProjectCentralBudgetRowType.CostMaterial;
                case ProjectCentralBudgetRowType.CostExpenseIB:
                    return ProjectCentralBudgetRowType.CostExpense; 
                case ProjectCentralBudgetRowType.OverheadCostPerHourIB:
                    return ProjectCentralBudgetRowType.OverheadCostPerHour; 
                case ProjectCentralBudgetRowType.OverheadCostIB:
                    return ProjectCentralBudgetRowType.OverheadCost; 
                default:
                    return budgetRowType;
            }
        }

        #endregion
    }
}

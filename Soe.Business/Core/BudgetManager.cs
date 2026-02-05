using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Transactions;

namespace SoftOne.Soe.Business.Core
{
    public class BudgetManager : ManagerBase
    {
        #region Variables

        // Create a logger for use in this class
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        //Budget collections
        Dictionary<int, bool> usedAccounts = null;
        List<int> usedInternalAccounts = null;
        List<AccountInternal> dim2Accounts = null;
        List<AccountInternal> dim3Accounts = null;
        List<AccountInternal> dim4Accounts = null;
        List<AccountInternal> dim5Accounts = null;
        List<AccountInternal> dim6Accounts = null;
        List<BudgetBalanceDTO> balanceItemList = new List<BudgetBalanceDTO>();
        AccountBalanceManager balanceManager = null;

        #endregion

        #region Ctor

        public BudgetManager(ParameterObject parameterObject) : base(parameterObject) { }

        #endregion

        #region Budget

        public List<BudgetHead> GetBudgetHeads(int actorCompanyId, int budgetType, bool loadRows = false, bool loadAccounts = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.DistributionCodeHead.NoTracking();
            return GetBudgetHeads(entities, actorCompanyId, budgetType, loadRows, loadAccounts);
        }

        public List<BudgetHead> GetBudgetHeads(CompEntities entities, int actorCompanyId, int budgetType, bool loadRows = false, bool loadAccounts = false)
        {
            IQueryable<BudgetHead> query = (from bh in entities.BudgetHead
                                            where bh.ActorCompanyId == actorCompanyId &&
                                            bh.Type == budgetType &&
                                            bh.State == (int)SoeEntityState.Active
                                            orderby bh.AccountYearId descending, bh.Created descending
                                            select bh);

            if (loadRows)
                query = query.Include("BudgetRow.BudgetRowPeriod");
            if (loadAccounts)
                query = query.Include("BudgetRow.AccountInternal.Account.AccountDim");

            return query.ToList();
        }

        public List<BudgetHead> GetBudgetHeads(int actorCompanyId, List<int> budgetTypes, bool loadRows = false, bool loadAccounts = false, bool onlyActive = false, DateTime? fromDate = null, DateTime? toDate = null)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.DistributionCodeHead.NoTracking();
            return GetBudgetHeads(entities, actorCompanyId, budgetTypes, loadRows, loadAccounts, onlyActive, fromDate, toDate);
        }

        public List<BudgetHead> GetBudgetHeads(CompEntities entities, int actorCompanyId, List<int> budgetTypes, bool loadRows = false, bool loadAccounts = false, bool onlyActive = false, DateTime? fromDate = null, DateTime? toDate = null)
        {
            IQueryable<BudgetHead> query = (from bh in entities.BudgetHead
                                            where bh.ActorCompanyId == actorCompanyId &&
                                            budgetTypes.Contains(bh.Type) &&
                                            bh.State == (int)SoeEntityState.Active
                                            orderby bh.AccountYearId descending, bh.Created descending
                                            select bh);

            if (loadRows)
                query = query.Include("BudgetRow.BudgetRowPeriod");
            if (loadAccounts)
                query = query.Include("BudgetRow.AccountInternal.Account.AccountDim");

            if (onlyActive)
                query = query.Where(bh => bh.Status == (int)BudgetHeadStatus.Active);

            if (fromDate.HasValue)
                query = query.Where(bh => !bh.ToDate.HasValue || bh.ToDate.Value >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(bh => !bh.FromDate.HasValue || bh.FromDate.Value <= toDate.Value);

            return query.ToList();
        }

        public List<BudgetHeadGridDTO> GetBudgetHeadForGrid(
            int actorCompanyId, int budgetType, int? budgetHeadId = null)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.BudgetHead.NoTracking();
            return GetBudgetHeadForGrid(entities, actorCompanyId, budgetType, budgetHeadId);
        }

        public List<BudgetHeadGridDTO> GetBudgetHeadForGrid(
            CompEntities entities, int actorCompanyId, int budgetType, int? budgetHeadId)
        {
            List<BudgetHeadGridDTO> dtos = new List<BudgetHeadGridDTO>();

            var typeTermsDict = base.GetTermGroupContent(TermGroup.BudgetType, skipUnknown: true);
            var statusTermsDict = base.GetTermGroupContent(TermGroup.BudgetStatus, skipUnknown: true);
            var heads = (from bh in entities.BudgetHead.Include("AccountYear")
                         where bh.ActorCompanyId == actorCompanyId &&
                         bh.Type == budgetType &&
                         bh.State == (int)SoeEntityState.Active &&
                         (!budgetHeadId.HasValue || bh.BudgetHeadId == budgetHeadId)
                         orderby bh.AccountYearId descending, bh.Created descending
                         select bh);

            foreach (BudgetHead head in heads)
            {
                BudgetHeadGridDTO dto = new BudgetHeadGridDTO()
                {
                    BudgetHeadId = head.BudgetHeadId,
                    Name = head.Name,
                    Created = head.Created != null ? ((DateTime)head.Created).ToShortDateString() : "",
                    NoOfPeriods = head.NoOfPeriods.ToString(),
                    AccountYearId = head.AccountYearId != null ? (int)head.AccountYearId : 0,
                    FromDate = head.FromDate,
                    ToDate = head.ToDate
                };

                GenericType typeTerm = typeTermsDict.FirstOrDefault(t => t.Id == head.Type);
                if (typeTerm != null)
                    dto.Type = typeTerm.Name;

                GenericType statusTerm = statusTermsDict.FirstOrDefault(t => t.Id == head.Status);
                if (statusTerm != null)
                    dto.Status = statusTerm.Name;

                if (head.AccountYear != null)
                    dto.AccountingYear = head.AccountYear.From.ToShortDateString() + " - " + head.AccountYear.To.ToShortDateString();

                dtos.Add(dto);
            }

            return dtos;
        }

        public List<BudgetHeadGridDTO> GetProjectBudgetHeadForGrid(int actorCompanyId, int projectId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.BudgetHead.NoTracking();
            return GetProjectBudgetHeadForGrid(entities, actorCompanyId, projectId);
        }

        public List<BudgetHeadGridDTO> GetProjectBudgetHeadForGrid(CompEntities entities, int actorCompanyId, int projectId)
        {
            List<BudgetHeadGridDTO> dtos = new List<BudgetHeadGridDTO>();

            var typeTermsDict = base.GetTermGroupContent(TermGroup.BudgetType, skipUnknown: true);
            var statusTermsDict = base.GetTermGroupContent(TermGroup.BudgetStatus, skipUnknown: true);
            var heads = (from bh in entities.BudgetHead.Include("AccountYear")
                         where bh.ActorCompanyId == actorCompanyId &&
                         bh.ProjectId == projectId &&
                         bh.State == (int)SoeEntityState.Active
                         orderby bh.AccountYearId descending, bh.Created descending
                         select bh);

            foreach (BudgetHead head in heads)
            {
                BudgetHeadGridDTO dto = new BudgetHeadGridDTO()
                {
                    BudgetHeadId = head.BudgetHeadId,
                    Name = head.Name,
                    Created = head.Created != null ? ((DateTime)head.Created).ToShortDateString() : "",
                    NoOfPeriods = head.NoOfPeriods.ToString(),
                    AccountYearId = head.AccountYearId != null ? (int)head.AccountYearId : 0,
                    FromDate = head.FromDate,
                    ToDate = head.ToDate,
                    BudgetTypeId = head.Type,
                };

                GenericType typeTerm = typeTermsDict.FirstOrDefault(t => t.Id == head.Type);
                if (typeTerm != null)
                    dto.Type = typeTerm.Name;

                GenericType statusTerm = statusTermsDict.FirstOrDefault(t => t.Id == head.Status);
                if (statusTerm != null)
                    dto.Status = statusTerm.Name;

                if (head.AccountYear != null)
                    dto.AccountingYear = head.AccountYear.From.ToShortDateString() + " - " + head.AccountYear.To.ToShortDateString();

                dtos.Add(dto);
            }

            return dtos;
        }

        public List<BudgetHeadGridDTO> GetSalesBudgetHeadForGrid(int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.BudgetHead.NoTracking();
            return GetSalesBudgetHeadForGrid(entities, actorCompanyId);
        }

        public List<BudgetHeadGridDTO> GetSalesBudgetHeadForGrid(CompEntities entities, int actorCompanyId)
        {
            var dtos = new List<BudgetHeadGridDTO>();

            var typeTermsDict = base.GetTermGroupContent(TermGroup.AccountingBudgetType, skipUnknown: true);
            var statusTermsDict = base.GetTermGroupContent(TermGroup.BudgetStatus, skipUnknown: true);

            var heads = (from bh in entities.BudgetHead.Include("AccountYear")
                         where bh.ActorCompanyId == actorCompanyId &&
                         (bh.Type == (int)TermGroup_AccountingBudgetType.SalesBudget ||
                            bh.Type == (int)TermGroup_AccountingBudgetType.SalesBudgetTime ||
                            bh.Type == (int)TermGroup_AccountingBudgetType.SalesBudgetSalaryCost) &&
                         bh.State == (int)SoeEntityState.Active
                         orderby bh.AccountYearId descending, bh.Created descending
                         select bh);

            foreach (BudgetHead head in heads)
            {
                var dto = new BudgetHeadGridDTO
                {
                    BudgetHeadId = head.BudgetHeadId,
                    Name = head.Name,
                    Created = head.Created != null ? ((DateTime)head.Created).ToShortDateString() : "",
                    NoOfPeriods = head.NoOfPeriods.ToString(),
                    AccountYearId = head.AccountYearId != null ? (int)head.AccountYearId : 0,
                    FromDate = head.FromDate,
                    ToDate = head.ToDate
                };

                GenericType typeTerm = typeTermsDict.FirstOrDefault(t => t.Id == head.Type);
                if (typeTerm != null)
                    dto.Type = typeTerm.Name;

                GenericType statusTerm = statusTermsDict.FirstOrDefault(t => t.Id == head.Status);
                if (statusTerm != null)
                    dto.Status = statusTerm.Name;

                if (head.AccountYear != null)
                    dto.AccountingYear = head.AccountYear.From.ToShortDateString() + " - " + head.AccountYear.To.ToShortDateString();

                dtos.Add(dto);
            }

            return dtos;
        }

        public BudgetHead GetBudgetHeadIncludingRows(int budgetHeadId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.DistributionCodeHead.NoTracking();
            return GetBudgetHeadIncludingRows(entities, budgetHeadId);
        }

        public BudgetHead GetBudgetHeadIncludingRows(CompEntities entities, int budgetHeadId)
        {
            return (from bh in entities.BudgetHead
                        .Include("BudgetRow.BudgetRowPeriod")
                        .Include("BudgetRow.AccountInternal.Account.AccountDim")
                    where bh.BudgetHeadId == budgetHeadId
                    select bh).FirstOrDefault();
        }

        public BudgetHeadFlattenedDTO GetSalesBudgetHeadIncludingRows(int budgetHeadId, int interval, DateTime fromDate)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.BudgetHead.NoTracking();
            return GetSalesBudgetHeadIncludingRows(entities, budgetHeadId, interval, fromDate);
        }

        public BudgetHeadFlattenedDTO GetSalesBudgetHeadIncludingRows(CompEntities entities, int budgetHeadId, int interval, DateTime fromDate)
        {
            BudgetHead head = (from bh in entities.BudgetHead
                       .Include("BudgetRow.BudgetRowPeriod")
                       .Include("BudgetRow.AccountInternal.Account.AccountDim")
                               where bh.BudgetHeadId == budgetHeadId
                               select bh).FirstOrDefault();

            //Convert to flattened
            BudgetHeadFlattenedDTO headToReturn = head.ToSalesBudgetFlattenedDTO();

            //Collect the periods depending on interval
            if (interval == (int)TermGroup_SalesBudgetInterval.Day)
            {
                foreach (var row in head.BudgetRow)
                {
                    //Get the row from DTO
                    BudgetRowFlattenedDTO RowToUpdate = headToReturn.Rows.FirstOrDefault(id => id.BudgetRowId == row.BudgetRowId);
                    List<BudgetRowPeriod> originalPeriods = row.BudgetRowPeriod.ToList();
                    int i = 1;
                    decimal totalAmount = 0;
                    decimal totalQuantity = 0;
                    Type t = RowToUpdate.GetType();
                    foreach (var period in originalPeriods)
                    {
                        DateTime periodDate = period.StartTime.Value;
                        //Compare only datepart
                        if (periodDate.Date == fromDate.Date)
                        {
                            totalAmount = totalAmount + period.Amount;
                            totalQuantity = totalQuantity + period.Quantity;

                            PropertyInfo periodIdProperty = t.GetProperty("BudgetRowPeriodId" + i);
                            if (periodIdProperty != null)
                                periodIdProperty.SetValue(RowToUpdate, period.BudgetRowPeriodId);

                            PropertyInfo periodNrProperty = t.GetProperty("PeriodNr" + i);
                            if (periodNrProperty != null)
                                periodNrProperty.SetValue(RowToUpdate, period.PeriodNr);

                            PropertyInfo startDateProperty = t.GetProperty("StartDate" + i);
                            if (startDateProperty != null)
                                startDateProperty.SetValue(RowToUpdate, period.StartTime);

                            PropertyInfo amountProperty = t.GetProperty("Amount" + i);
                            if (amountProperty != null)
                                amountProperty.SetValue(RowToUpdate, period.Amount);

                            PropertyInfo quantityProperty = t.GetProperty("Quantity" + i);
                            if (quantityProperty != null)
                                quantityProperty.SetValue(RowToUpdate, period.Quantity);

                            i = i + 1;
                        }
                    }
                    RowToUpdate.TotalAmount = totalAmount;
                    RowToUpdate.TotalQuantity = totalQuantity;
                }
            }
            if (interval == (int)TermGroup_SalesBudgetInterval.Year)
            {
                foreach (var row in head.BudgetRow)
                {
                    //Get the row from DTO
                    BudgetRowFlattenedDTO RowToUpdate = headToReturn.Rows.FirstOrDefault(id => id.BudgetRowId == row.BudgetRowId);
                    List<BudgetRowPeriod> originalPeriods = row.BudgetRowPeriod.ToList();
                    decimal totalAmount = 0;
                    decimal totalQuantity = 0;
                    for (int i = 1; i < 13; i++)
                    {
                        Type t = RowToUpdate.GetType();
                        decimal periodAmount = 0;
                        decimal periodQuantity = 0;
                        foreach (var period in originalPeriods)
                        {
                            DateTime periodDate = period.StartTime.Value;
                            //Compare only month
                            if (periodDate.Month == i)
                            {
                                totalAmount = totalAmount + period.Amount;
                                periodAmount = periodAmount + period.Amount;
                                totalQuantity = totalQuantity + period.Quantity;
                                periodQuantity = periodQuantity + period.Quantity;
                            }
                        }
                        PropertyInfo periodNrProperty = t.GetProperty("PeriodNr" + i);
                        if (periodNrProperty != null)
                            periodNrProperty.SetValue(RowToUpdate, i);

                        PropertyInfo startDateProperty = t.GetProperty("StartDate" + i);
                        if (startDateProperty != null)
                            startDateProperty.SetValue(RowToUpdate, new DateTime(fromDate.Year, i, 1));

                        PropertyInfo amountProperty = t.GetProperty("Amount" + i);
                        if (amountProperty != null)
                            amountProperty.SetValue(RowToUpdate, periodAmount);

                        PropertyInfo quantityProperty = t.GetProperty("Quantity" + i);
                        if (quantityProperty != null)
                            quantityProperty.SetValue(RowToUpdate, periodQuantity);

                    }
                    RowToUpdate.TotalAmount = totalAmount;
                    RowToUpdate.TotalQuantity = totalQuantity;
                }
            }
            if (interval == (int)TermGroup_SalesBudgetInterval.MonthDaily)
            {
                foreach (var row in head.BudgetRow)
                {
                    //Get the row from DTO
                    BudgetRowFlattenedDTO RowToUpdate = headToReturn.Rows.FirstOrDefault(id => id.BudgetRowId == row.BudgetRowId);
                    List<BudgetRowPeriod> originalPeriods = row.BudgetRowPeriod.ToList();
                    decimal totalAmount = 0;
                    decimal totalQuantity = 0;
                    for (int i = 1; i < DateTime.DaysInMonth(fromDate.Year, fromDate.Month) + 1; i++)
                    {
                        Type t = RowToUpdate.GetType();
                        decimal periodAmount = 0;
                        decimal periodQuantity = 0;
                        DateTime compareDate = new DateTime(fromDate.Year, fromDate.Month, i);
                        foreach (var period in originalPeriods)
                        {
                            DateTime periodDate = period.StartTime.Value;
                            //Compare only datepart
                            if (periodDate.Date == compareDate.Date)
                            {
                                totalAmount = totalAmount + period.Amount;
                                periodAmount = periodAmount + period.Amount;
                                totalQuantity = totalQuantity + period.Quantity;
                                periodQuantity = periodQuantity + period.Quantity;
                            }
                        }
                        PropertyInfo periodNrProperty = t.GetProperty("PeriodNr" + i);
                        if (periodNrProperty != null)
                            periodNrProperty.SetValue(RowToUpdate, i);

                        PropertyInfo startDateProperty = t.GetProperty("StartDate" + i);
                        if (startDateProperty != null)
                            startDateProperty.SetValue(RowToUpdate, new DateTime(fromDate.Year, fromDate.Month, i));

                        PropertyInfo amountProperty = t.GetProperty("Amount" + i);
                        if (amountProperty != null)
                            amountProperty.SetValue(RowToUpdate, periodAmount);

                        PropertyInfo quantityProperty = t.GetProperty("Quantity" + i);
                        if (quantityProperty != null)
                            quantityProperty.SetValue(RowToUpdate, periodQuantity);
                    }
                    RowToUpdate.TotalAmount = totalAmount;
                    RowToUpdate.TotalQuantity = totalQuantity;
                }
            }
            if (interval == (int)TermGroup_SalesBudgetInterval.MontWeekly)
            {
                foreach (var row in head.BudgetRow)
                {
                    //Get the row from DTO
                    BudgetRowFlattenedDTO RowToUpdate = headToReturn.Rows.FirstOrDefault(id => id.BudgetRowId == row.BudgetRowId);
                    List<BudgetRowPeriod> originalPeriods = row.BudgetRowPeriod.ToList();
                    decimal totalAmount = 0;
                    decimal periodAmount = 0;
                    decimal totalQuantity = 0;
                    decimal periodQuantity = 0;
                    bool period1filled = false;
                    bool period2filled = false;
                    bool period3filled = false;
                    bool period4filled = false;

                    for (int i = 1; i < DateTime.DaysInMonth(fromDate.Year, fromDate.Month) + 1; i++)
                    {
                        Type t = RowToUpdate.GetType();
                        DateTime compareDate = new DateTime(fromDate.Year, fromDate.Month, i);
                        foreach (var period in originalPeriods)
                        {
                            DateTime periodDate = period.StartTime.Value;
                            //Compare only datepart
                            if (periodDate.Date == compareDate.Date)
                            {

                                totalAmount = totalAmount + period.Amount;
                                periodAmount = periodAmount + period.Amount;
                                totalQuantity = totalQuantity + period.Quantity;
                                periodQuantity = periodQuantity + period.Quantity;

                                if (i == 8)
                                {
                                    if (!period1filled)
                                    {
                                        PropertyInfo periodNrProperty1 = t.GetProperty("PeriodNr" + 1);
                                        if (periodNrProperty1 != null)
                                            periodNrProperty1.SetValue(RowToUpdate, i);

                                        PropertyInfo startDateProperty1 = t.GetProperty("StartDate" + 1);
                                        if (startDateProperty1 != null)
                                            startDateProperty1.SetValue(RowToUpdate, new DateTime(fromDate.Year, fromDate.Month, 1));

                                        PropertyInfo amountProperty1 = t.GetProperty("Amount" + 1);
                                        if (amountProperty1 != null)
                                            amountProperty1.SetValue(RowToUpdate, periodAmount);

                                        PropertyInfo quantityProperty1 = t.GetProperty("Quantity" + 1);
                                        if (quantityProperty1 != null)
                                            quantityProperty1.SetValue(RowToUpdate, periodQuantity);

                                        periodAmount = 0;
                                        periodQuantity = 0;
                                        period1filled = true;
                                    }

                                }
                                if (i == 15)
                                {
                                    if (!period2filled)
                                    {
                                        PropertyInfo periodNrProperty2 = t.GetProperty("PeriodNr" + 2);
                                        if (periodNrProperty2 != null)
                                            periodNrProperty2.SetValue(RowToUpdate, i);

                                        PropertyInfo startDateProperty2 = t.GetProperty("StartDate" + 2);
                                        if (startDateProperty2 != null)
                                            startDateProperty2.SetValue(RowToUpdate, new DateTime(fromDate.Year, fromDate.Month, 8));

                                        PropertyInfo amountProperty2 = t.GetProperty("Amount" + 2);
                                        if (amountProperty2 != null)
                                            amountProperty2.SetValue(RowToUpdate, periodAmount);

                                        PropertyInfo quantityProperty2 = t.GetProperty("Quantity" + 2);
                                        if (quantityProperty2 != null)
                                            quantityProperty2.SetValue(RowToUpdate, periodQuantity);

                                        periodAmount = 0;
                                        periodQuantity = 0;
                                        period2filled = true;
                                    }
                                }
                                if (i == 22)
                                {
                                    if (!period3filled)
                                    {
                                        PropertyInfo periodNrProperty3 = t.GetProperty("PeriodNr" + 3);
                                        if (periodNrProperty3 != null)
                                            periodNrProperty3.SetValue(RowToUpdate, i);

                                        PropertyInfo startDateProperty3 = t.GetProperty("StartDate" + 3);
                                        if (startDateProperty3 != null)
                                            startDateProperty3.SetValue(RowToUpdate, new DateTime(fromDate.Year, fromDate.Month, 15));

                                        PropertyInfo amountProperty3 = t.GetProperty("Amount" + 3);
                                        if (amountProperty3 != null)
                                            amountProperty3.SetValue(RowToUpdate, periodAmount);

                                        PropertyInfo quantityProperty3 = t.GetProperty("Quantity" + 3);
                                        if (quantityProperty3 != null)
                                            quantityProperty3.SetValue(RowToUpdate, periodQuantity);

                                        periodAmount = 0;
                                        periodQuantity = 0;
                                        period3filled = true;
                                    }
                                }

                                if (i > 21 && DateTime.DaysInMonth(fromDate.Year, fromDate.Month) == 28 && i == 28)
                                {
                                    PropertyInfo periodNrProperty4 = t.GetProperty("PeriodNr" + 4);
                                    if (periodNrProperty4 != null)
                                        periodNrProperty4.SetValue(RowToUpdate, i);

                                    PropertyInfo startDateProperty4 = t.GetProperty("StartDate" + 4);
                                    if (startDateProperty4 != null)
                                        startDateProperty4.SetValue(RowToUpdate, new DateTime(fromDate.Year, fromDate.Month, 22));

                                    PropertyInfo amountProperty4 = t.GetProperty("Amount" + 4);
                                    if (amountProperty4 != null)
                                        amountProperty4.SetValue(RowToUpdate, periodAmount);

                                    PropertyInfo quantityProperty4 = t.GetProperty("Quantity" + 4);
                                    if (quantityProperty4 != null)
                                        quantityProperty4.SetValue(RowToUpdate, periodQuantity);

                                    period4filled = true;
                                }

                                if (i > 21 && DateTime.DaysInMonth(fromDate.Year, fromDate.Month) == 29 && i == 29)
                                {
                                    PropertyInfo periodNrProperty4 = t.GetProperty("PeriodNr" + 4);
                                    if (periodNrProperty4 != null)
                                        periodNrProperty4.SetValue(RowToUpdate, i);

                                    PropertyInfo startDateProperty4 = t.GetProperty("StartDate" + 4);
                                    if (startDateProperty4 != null)
                                        startDateProperty4.SetValue(RowToUpdate, new DateTime(fromDate.Year, fromDate.Month, 22));

                                    PropertyInfo amountProperty4 = t.GetProperty("Amount" + 4);
                                    if (amountProperty4 != null)
                                        amountProperty4.SetValue(RowToUpdate, periodAmount);

                                    PropertyInfo quantityProperty4 = t.GetProperty("Quantity" + 4);
                                    if (quantityProperty4 != null)
                                        quantityProperty4.SetValue(RowToUpdate, periodQuantity);
                                }
                                if (i == 29 && DateTime.DaysInMonth(fromDate.Year, fromDate.Month) >= 30)
                                {
                                    if (!period4filled)
                                    {
                                        PropertyInfo periodNrProperty4 = t.GetProperty("PeriodNr" + 4);
                                        if (periodNrProperty4 != null)
                                            periodNrProperty4.SetValue(RowToUpdate, i);

                                        PropertyInfo startDateProperty4 = t.GetProperty("StartDate" + 4);
                                        if (startDateProperty4 != null)
                                            startDateProperty4.SetValue(RowToUpdate, new DateTime(fromDate.Year, fromDate.Month, 22));

                                        PropertyInfo amountProperty4 = t.GetProperty("Amount" + 4);
                                        if (amountProperty4 != null)
                                            amountProperty4.SetValue(RowToUpdate, periodAmount);

                                        PropertyInfo quantityProperty4 = t.GetProperty("Quantity" + 4);
                                        if (quantityProperty4 != null)
                                            quantityProperty4.SetValue(RowToUpdate, periodQuantity);

                                        periodAmount = 0;
                                        periodQuantity = 0;
                                        period4filled = true;
                                    }
                                }

                                if (i > 28 && i < DateTime.DaysInMonth(fromDate.Year, fromDate.Month) + 1 && DateTime.DaysInMonth(fromDate.Year, fromDate.Month) >= 30)
                                {
                                    PropertyInfo periodNrProperty5 = t.GetProperty("PeriodNr" + 5);
                                    if (periodNrProperty5 != null)
                                        periodNrProperty5.SetValue(RowToUpdate, 5);

                                    PropertyInfo startDateProperty5 = t.GetProperty("StartDate" + 5);
                                    if (startDateProperty5 != null)
                                        startDateProperty5.SetValue(RowToUpdate, new DateTime(fromDate.Year, fromDate.Month, 29));

                                    PropertyInfo amountProperty5 = t.GetProperty("Amount" + 5);
                                    if (amountProperty5 != null)
                                        amountProperty5.SetValue(RowToUpdate, periodAmount);

                                    PropertyInfo quantityProperty5 = t.GetProperty("Quantity" + 5);
                                    if (quantityProperty5 != null)
                                        quantityProperty5.SetValue(RowToUpdate, periodQuantity);
                                }
                            }
                        }
                    }
                    RowToUpdate.TotalAmount = totalAmount;
                    RowToUpdate.TotalQuantity = totalQuantity;
                }
            }
            return headToReturn;
        }

        public BudgetHeadSalesDTO GetSalesBudgetHeadIncludingRows(int budgetHeadId) //Do we need date? , DateTime fromDate
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.BudgetHead.NoTracking();
            return GetSalesBudgetHeadIncludingRows(entities, budgetHeadId);
        }

        public BudgetHeadSalesDTO GetSalesBudgetHeadIncludingRows(CompEntities entities, int budgetHeadId) //Do we need date? , DateTime fromDate
        {
            #region prereq

            // Get account dimensions
            List<AccountDim> accountDims = AccountManager.GetAccountDimsByCompany(entities, base.ActorCompanyId, false, true, true, true, loadParentOrCalculateLevels: true).ToList();

            // Get distribution codes
            List<DistributionCodeHead> distributionCodes = GetDistributionCodes(entities, base.ActorCompanyId, true, false);

            #endregion

            BudgetHead head = (from bh in entities.BudgetHead
                               .Include("DistributionCodeHead")
                               .Include("BudgetRow.BudgetRowPeriod")
                               .Include("BudgetRow.AccountInternal.Account.AccountDim")
                               where bh.BudgetHeadId == budgetHeadId
                               select bh).FirstOrDefault();

            if (head == null || head.DistributionCodeHead == null)
                return null;

            DistributionCodeHead currentDistributionCode = distributionCodes.FirstOrDefault(c => c.DistributionCodeHeadId == head.DistributionCodeHeadId);

            //Convert to flattened
            BudgetHeadSalesDTO headToReturn = new BudgetHeadSalesDTO()
            {
                BudgetHeadId = head.BudgetHeadId,
                ActorCompanyId = head.ActorCompanyId,
                DistributionCodeHeadId = head.DistributionCodeHeadId,
                Name = head.Name,
                NoOfPeriods = head.NoOfPeriods,
                Type = head.Type,
                DistributionCodeSubType = head.DistributionCodeHead != null ? head.DistributionCodeHead.SubType : (int?)null,
                Status = head.Status,
                FromDate = head.FromDate,
                ToDate = head.ToDate,
                Created = head.Created,
                CreatedBy = head.CreatedBy,
                Modified = head.Modified,
                ModifiedBy = head.ModifiedBy,
                Rows = new List<BudgetRowSalesDTO>(),
            };

            int rowCounter = 1;
            foreach (BudgetRow row in head.BudgetRow.Where(r => r.State == (int)SoeEntityState.Active))
            {
                //Get the row from DTO
                BudgetRowSalesDTO rowDto = new BudgetRowSalesDTO()
                {
                    BudgetRowId = row.BudgetRowId,
                    BudgetHeadId = row.BudgetHeadId,
                    AccountId = row.AccountId != null ? (int)row.AccountId : 0,
                    TotalAmount = row.TotalAmount,
                    TotalQuantity = row.TotalQuantity,
                    BudgetRowNr = rowCounter,
                    Modified = row.Modified != null ? ((DateTime)row.Modified).ToString() : "",
                    ModifiedBy = row.ModifiedBy != null ? row.ModifiedBy : "",
                };

                // Set internal accounts
                if (row.AccountInternal != null && row.AccountInternal.Any())
                {
                    var type = rowDto.GetType();
                    int index = 1;
                    foreach (AccountDim accountDim in accountDims)
                    {
                        AccountInternal account = row.AccountInternal.FirstOrDefault(a => a.Account.AccountDimId == accountDim.AccountDimId);
                        if (account != null)
                        {
                            PropertyInfo idProperty = type.GetProperty("Dim" + index + "Id");
                            if (idProperty != null)
                                idProperty.SetValue(rowDto, account.AccountId);
                            PropertyInfo nrProperty = type.GetProperty("Dim" + index + "Nr");
                            if (nrProperty != null)
                                nrProperty.SetValue(rowDto, account.Account.AccountNr);
                            PropertyInfo nameProperty = type.GetProperty("Dim" + index + "Name");
                            if (nameProperty != null)
                                nameProperty.SetValue(rowDto, account.Account.Name);
                        }
                        index++;
                    }
                }

                rowDto.Periods = ParsePeriodsForSalesBudget(distributionCodes, currentDistributionCode, row.BudgetRowPeriod.ToList(), row.BudgetRowId, rowDto.BudgetRowNr, string.Empty, null);

                headToReturn.Rows.Add(rowDto);

                rowCounter++;
            }

            return headToReturn;
        }

        public List<BudgetPeriodSalesDTO> ParsePeriodsForSalesBudget(List<DistributionCodeHead> distributionCodes, DistributionCodeHead currentDistributionCode, List<BudgetRowPeriod> rowPeriods, int budgetRowId, int budgetRowNr, string identifier, Guid? parentGuid = null)
        {
            List<BudgetPeriodSalesDTO> periods = new List<BudgetPeriodSalesDTO>();

            //Set level identifier
            identifier = identifier + "100";

            // Convert to counter
            var identificationNr = Convert.ToInt32(identifier);

            foreach (var codePeriod in currentDistributionCode.DistributionCodePeriod)
            {
                if (codePeriod.ParentToDistributionCodeHeadId != null)
                {
                    var childCode = distributionCodes.FirstOrDefault(c => c.DistributionCodeHeadId == codePeriod.ParentToDistributionCodeHeadId);
                    if (childCode.SubType != (int)TermGroup_AccountingBudgetSubType.Day)
                    {
                        // Create period
                        var newPeriod = new BudgetPeriodSalesDTO()
                        {
                            BudgetRowId = budgetRowId,
                            Quantity = 0,
                            BudgetRowNr = budgetRowNr,
                            Guid = Guid.NewGuid(),
                            Percent = codePeriod.Percent,
                            DistributionCodeHeadId = childCode.DistributionCodeHeadId,
                        };

                        if (parentGuid.HasValue)
                            newPeriod.ParentGuid = parentGuid.Value;

                        // Get child periods
                        newPeriod.Periods = ParsePeriodsForSalesBudget(distributionCodes, childCode, rowPeriods, budgetRowId, budgetRowNr, identificationNr.ToString(), newPeriod.Guid);

                        // Set amount
                        newPeriod.Amount = newPeriod.Periods.Sum(p => p.Amount);

                        // Set quantity   
                        newPeriod.Quantity = newPeriod.Periods.Sum(p => p.Quantity);

                        if (childCode.OpeningHours != null)
                        {
                            newPeriod.StartHour = childCode.OpeningHours.OpeningTime.HasValue ? childCode.OpeningHours.OpeningTime.Value.Hour : 0;
                            newPeriod.ClosingHour = childCode.OpeningHours.ClosingTime.HasValue ? childCode.OpeningHours.ClosingTime.Value.Hour : 0;
                        }

                        periods.Add(newPeriod);
                    }
                    else
                    {
                        var period = rowPeriods.FirstOrDefault(p => p.PeriodNr == identificationNr);
                        if (period != null)
                        {
                            // Found
                            var newPeriod = new BudgetPeriodSalesDTO()
                            {
                                BudgetRowPeriodId = period.BudgetRowPeriodId,
                                BudgetRowId = period.BudgetRowId,
                                Amount = period.Amount,
                                Quantity = period.Quantity,
                                BudgetRowNr = budgetRowNr,
                                Guid = Guid.NewGuid(),
                                Percent = codePeriod.Percent,
                                StartDate = period.StartTime,
                            };

                            if (period.DistributionCodeHeadId.HasValue)
                                newPeriod.DistributionCodeHeadId = period.DistributionCodeHeadId.Value;

                            if (parentGuid.HasValue)
                                newPeriod.ParentGuid = parentGuid.Value;

                            periods.Add(newPeriod);
                        }
                        else
                        {
                            // Add empty
                            var newPeriod = new BudgetPeriodSalesDTO()
                            {
                                BudgetRowId = budgetRowId,
                                Amount = 0,
                                Quantity = 0,
                                BudgetRowNr = budgetRowNr,
                                Guid = Guid.NewGuid(),
                                Percent = codePeriod.Percent,
                            };

                            if (parentGuid.HasValue)
                                newPeriod.ParentGuid = parentGuid.Value;

                            periods.Add(newPeriod);
                        }
                    }
                }
                else
                {
                    var period = rowPeriods.FirstOrDefault(p => p.PeriodNr == identificationNr);
                    if (period != null)
                    {
                        // Found
                        var newPeriod = new BudgetPeriodSalesDTO()
                        {
                            BudgetRowPeriodId = period.BudgetRowPeriodId,
                            BudgetRowId = period.BudgetRowId,
                            Amount = period.Amount,
                            Quantity = period.Quantity,
                            BudgetRowNr = budgetRowNr,
                            Guid = Guid.NewGuid(),
                            Percent = codePeriod.Percent,
                            StartDate = period.StartTime,
                        };

                        if (period.DistributionCodeHeadId.HasValue)
                            newPeriod.DistributionCodeHeadId = period.DistributionCodeHeadId.Value;

                        if (parentGuid.HasValue)
                            newPeriod.ParentGuid = parentGuid.Value;

                        periods.Add(newPeriod);
                    }
                    else
                    {
                        // Add empty
                        var newPeriod = new BudgetPeriodSalesDTO()
                        {
                            BudgetRowId = budgetRowId,
                            Amount = 0,
                            Quantity = 0,
                            BudgetRowNr = budgetRowNr,
                            Guid = Guid.NewGuid(),
                            Percent = codePeriod.Percent,
                        };

                        if (parentGuid.HasValue)
                            newPeriod.ParentGuid = parentGuid.Value;

                        periods.Add(newPeriod);
                    }
                }

                identificationNr++;
            }

            return periods;
        }

        public List<SmallGenericType> GetBudgetHeadsDist(int actorCompanyId, int accountingBudget)
        {
            List<SmallGenericType> budgetHeadsdict = new List<SmallGenericType>();

            List<BudgetHead> budgetHeads = this.GetBudgetHeads(actorCompanyId, accountingBudget);
            foreach (BudgetHead budgetHead in budgetHeads)
            {
                if (!budgetHeadsdict.Any(a => a.Id == budgetHead.BudgetHeadId))
                    budgetHeadsdict.Add(new SmallGenericType(budgetHead.BudgetHeadId, budgetHead.Name));
            }

            return budgetHeadsdict;
        }

        public List<BudgetRow> GetBudgetRowsAndPeriod(CompEntities entities, int budgetHeadId)
        {
            return (from br in entities.BudgetRow
                     .Include("BudgetRowPeriod")
                     .Include("AccountInternal.Account.AccountDim")
                    where br.BudgetHeadId == budgetHeadId &&
                    br.State == (int)SoeEntityState.Active
                    select br).ToList();
        }

        public BudgetHead GetBudgetHeadIncludingRowsForProject(CompEntities entities, int projectId)
        {
            return (from bh in entities.BudgetHead.Include("BudgetRow")
                    where bh.ProjectId == projectId &&
                    bh.State == (int)SoeEntityState.Active
                    orderby bh.BudgetHeadId descending
                    select bh).FirstOrDefault();
        }

        public Dictionary<int, BalanceItemDTO> GetBudgetForPeriodFromDTO(int budgetHeadId, AccountYearDTO accountYear, DateTime dateFrom, DateTime dateTo, List<AccountDTO> accountStdsInInterval, List<AccountInternalDTO> accountInternals, int actorCompanyId, bool matchInternalAccounts, out List<VoucherHeadDTO> voucherHeads)
        {
            Dictionary<int, BalanceItemDTO> biDict = new Dictionary<int, BalanceItemDTO>();
            Dictionary<int, Dictionary<int, bool>> accountInternalsDict = new Dictionary<int, Dictionary<int, bool>>();

            #region Init

            voucherHeads = new List<VoucherHeadDTO>();

            InitBalanceItemDict(accountStdsInInterval, ref biDict);
            if (matchInternalAccounts)
            {
                InitDimInternalDict(accountInternals, ref accountInternalsDict);
            }

            if (accountYear == null)
                return biDict;

            #endregion

            #region Prereq

            BudgetHead budgetHead = GetBudgetHeadIncludingRows(budgetHeadId);
            if (accountYear.AccountYearId != budgetHead.AccountYearId || budgetHead == null)
                return biDict;

            List<AccountPeriod> accountPeriods = AccountManager.GetAccountPeriods(accountYear.AccountYearId, false);

            #endregion

            #region Budget

            foreach (BudgetRow budgetRow in budgetHead.BudgetRow.Where(i => i.State == (int)SoeEntityState.Active))
            {
                if (budgetRow.AccountId == null || budgetRow.AccountId == 0)
                    continue;

                if (budgetRow.State != (int)SoeEntityState.Active)
                    continue;

                //Matches budgetrow against accountinternals. Every dim in the report selection has to be matching against corresponding dim in budgetrow.
                //I.e. if selection contains costplace & project, then budgetrow needs to match both costplace & project dims.
                if (matchInternalAccounts && accountInternals.Any())
                {
                    bool matched = true;
                    foreach (var dimId in accountInternalsDict.Keys)
                    {
                        var accounts = budgetRow.AccountInternal.Where(r => r.Account.AccountDimId == dimId).ToList();
                        if (accounts.Count == 0)
                        {
                            matched = false;
                            break;
                        }
                        bool hasDim = false;
                        foreach (var account in accounts)
                        {
                            if (accountInternalsDict[dimId].ContainsKey(account.AccountId))
                            {
                                hasDim = true;
                                break;
                            }
                        }
                        if (!hasDim)
                        {
                            matched = false;
                            break;
                        }
                    }
                    if (matched == false)
                    {
                        continue;
                    }
                }


                DateTime date = new DateTime();

                foreach (BudgetRowPeriod budgetRowPeriod in budgetRow.BudgetRowPeriod)
                {
                    foreach (AccountPeriod period in accountPeriods)
                    {

                        if (period.PeriodNr == budgetRowPeriod.PeriodNr)
                        {
                            date = period.From;

                            if (!(date >= dateFrom && date <= dateTo))
                                continue;

                            //Create virtual VoucherHead
                            VoucherHeadDTO voucherHead = new VoucherHeadDTO()
                            {
                                VoucherNr = 1,
                                Date = date,
                                Text = budgetHead.Name,
                                BudgetAccountId = budgetRow.AccountId ?? 0, //BudgetAccountId is only used for performance in reports

                                //Set FK
                                ActorCompanyId = actorCompanyId,
                            };

                            VoucherRowDTO voucherRow = new VoucherRowDTO()
                            {
                                Amount = budgetRowPeriod.Amount,

                                //Set FK
                                Dim1Id = (budgetRow.AccountId ?? 0),
                                AccountInternalDTO_forReports = new List<AccountInternalDTO>(),
                            };

                            if (budgetRow.AccountInternal.Any())
                            {
                                foreach (AccountInternal ai in budgetRow.AccountInternal)
                                {
                                    voucherRow.AccountInternalDTO_forReports.Add(ai.ToDTO());
                                }
                            }

                            if (voucherHead.Rows == null)
                                voucherHead.Rows = new List<VoucherRowDTO>();

                            voucherHead.Rows.Add(voucherRow);
                            voucherHeads.Add(voucherHead);
                        }
                    }
                }
            }

            UpdateBalanceItemsFromVoucherHeadDTO(voucherHeads, biDict, accountInternals, true);

            #endregion

            return biDict;
        }

        public ActionResult SaveBudgetHead(CompEntities entities, BudgetHeadDTO dto)
        {
            ActionResult result = new ActionResult(true);

            try
            {
                using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                {
                    result = SaveBudgetHead(transaction, entities, dto);

                    //Commit transaction
                    if (result.Success)
                    {
                        transaction.Complete();
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

                entities.Connection.Close();
            }

            return result;
        }

        public ActionResult SaveBudgetHead(TransactionScope transaction, CompEntities entities, BudgetHeadDTO dto)
        {
            ActionResult result = new ActionResult(true);

            BudgetHead head = null;

            if (entities.Connection.State != ConnectionState.Open)
                entities.Connection.Open();

            #region Prereq

            if (dto == null)
                return new ActionResult(false, (int)ActionResultSave.EntityIsNull, GetText(7044, "Felaktig inparameter"));

            if (dto.BudgetHeadId != 0)
                head = GetBudgetHeadIncludingRows(entities, dto.BudgetHeadId);

            // Get internal accounts (Dim2-6)
            List<AccountInternal> accountInternals = AccountManager.GetAccountInternals(entities, dto.ActorCompanyId, true);
            AccountInternal accountInternal;

            #endregion

            #region Perform

            if (head != null)
            {
                //Update
                head.Type = dto.Type;
                head.Name = dto.Name;
                head.AccountYearId = dto.AccountYearId;
                head.NoOfPeriods = dto.NoOfPeriods;
                head.Status = dto.Status;

                if (!head.AccountInternal.IsLoaded)
                    head.AccountInternal.Load();

                head.AccountInternal.Clear();

                head.UseDim2 = dto.UseDim2;
                head.UseDim3 = dto.UseDim3;

                head.FromDate = dto.FromDate;
                head.ToDate = dto.ToDate;

                if (head.UseDim2.HasValue && head.UseDim2.Value && dto.Dim2Id != 0 && (accountInternal = accountInternals.FirstOrDefault(a => a.AccountId == dto.Dim2Id)) != null)
                    head.AccountInternal.Add(accountInternal);
                if (head.UseDim3.HasValue && head.UseDim3.Value && dto.Dim3Id != 0 && (accountInternal = accountInternals.FirstOrDefault(a => a.AccountId == dto.Dim3Id)) != null)
                    head.AccountInternal.Add(accountInternal);

                SetModifiedProperties(head);

                result = SaveChanges(entities);

                if (!result.Success)
                    return result;

                foreach (BudgetRowDTO row in dto.Rows)
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
                                    budgetRow.AccountId = row.Dim1Id.ToNullable();
                                    budgetRow.ShiftTypeId = row.ShiftTypeId;
                                    budgetRow.TotalAmount = row.TotalAmount;
                                    budgetRow.TotalQuantity = row.TotalQuantity;
                                    budgetRow.TimeCodeId = row.TimeCodeId.ToNullable();

                                    budgetRow.DistributionCodeHeadId = dto.DistributionCodeHeadId != 0 ? row.DistributionCodeHeadId : null;

                                    #region Accounts

                                    // Get standard account
                                    budgetRow.AccountStd = AccountManager.GetAccountStd(entities, row.Dim1Id, head.ActorCompanyId, true, false);

                                    if (!budgetRow.AccountInternal.IsLoaded)
                                        budgetRow.AccountInternal.Load();

                                    budgetRow.AccountInternal.Clear();

                                    if (row.Dim2Id != 0 && (accountInternal = accountInternals.FirstOrDefault(a => a.AccountId == row.Dim2Id)) != null)
                                        budgetRow.AccountInternal.Add(accountInternal);
                                    if (row.Dim3Id != 0 && (accountInternal = accountInternals.FirstOrDefault(a => a.AccountId == row.Dim3Id)) != null)
                                        budgetRow.AccountInternal.Add(accountInternal);
                                    if (row.Dim4Id != 0 && (accountInternal = accountInternals.FirstOrDefault(a => a.AccountId == row.Dim4Id)) != null)
                                        budgetRow.AccountInternal.Add(accountInternal);
                                    if (row.Dim5Id != 0 && (accountInternal = accountInternals.FirstOrDefault(a => a.AccountId == row.Dim5Id)) != null)
                                        budgetRow.AccountInternal.Add(accountInternal);
                                    if (row.Dim6Id != 0 && (accountInternal = accountInternals.FirstOrDefault(a => a.AccountId == row.Dim6Id)) != null)
                                        budgetRow.AccountInternal.Add(accountInternal);

                                    #endregion

                                    //Periods
                                    List<int> handledPeriodIds = new List<int>();

                                    if (row.Periods != null)
                                    {
                                        foreach (BudgetPeriodDTO period in row.Periods)
                                        {
                                            BudgetRowPeriod rowPeriod = null;

                                            if (period.BudgetRowPeriodId != 0)
                                            {
                                                rowPeriod = budgetRow.BudgetRowPeriod.FirstOrDefault(p => p.BudgetRowPeriodId == period.BudgetRowPeriodId);

                                                if (rowPeriod != null)
                                                {
                                                    rowPeriod.PeriodNr = period.PeriodNr;
                                                    rowPeriod.Amount = period.Amount;
                                                    rowPeriod.Quantity = period.Quantity;
                                                    rowPeriod.Type = period.Type.HasValue ? period.Type.Value : 0;
                                                    rowPeriod.StartTime = period.StartDate;

                                                    SetModifiedProperties(rowPeriod);

                                                    handledPeriodIds.Add(rowPeriod.BudgetRowPeriodId);
                                                }
                                            }
                                            else
                                            {
                                                rowPeriod = new BudgetRowPeriod()
                                                {
                                                    BudgetRowId = budgetRow.BudgetRowId,
                                                    PeriodNr = period.PeriodNr,
                                                    Amount = period.Amount,
                                                    Quantity = period.Quantity,
                                                    Type = period.Type.HasValue ? period.Type.Value : 0,
                                                    StartTime = period.StartDate,
                                                };

                                                //AddEntityItem(entities, rowPeriod, "BudgetRowPeriod", transaction);
                                                SetCreatedProperties(rowPeriod);
                                                entities.BudgetRowPeriod.AddObject(rowPeriod);
                                                handledPeriodIds.Add(rowPeriod.BudgetRowPeriodId);
                                            }
                                        }

                                        row.Periods.Where(p => !handledPeriodIds.Exists(i => i == p.BudgetRowPeriodId)).ToList().ForEach(p => entities.DeleteObject(p));
                                    }

                                    SetModifiedProperties(budgetRow);
                                }
                            }
                        }
                    }
                    else
                    {
                        budgetRow = new BudgetRow()
                        {
                            BudgetHeadId = head.BudgetHeadId,
                            AccountId = row.Dim1Id.ToNullable(),
                            ShiftTypeId = row.ShiftTypeId,
                            TotalAmount = row.TotalAmount,
                            TotalQuantity = row.TotalQuantity,
                            Type = row.Type,
                            TimeCodeId = row.TimeCodeId.ToNullable()
                        };

                        if (dto.DistributionCodeHeadId != 0)
                        {
                            budgetRow.DistributionCodeHeadId = row.DistributionCodeHeadId;
                        }

                        #region Accounts

                        // Get standard account
                        budgetRow.AccountStd = AccountManager.GetAccountStd(entities, row.Dim1Id, head.ActorCompanyId, true, false);

                        SetCreatedProperties(budgetRow);
                        entities.BudgetRow.AddObject(budgetRow);
                        result = SaveChanges(entities);

                        if (!result.Success)
                            return result;

                        if (row.Dim2Id != 0 && (accountInternal = accountInternals.FirstOrDefault(a => a.AccountId == row.Dim2Id)) != null)
                            budgetRow.AccountInternal.Add(accountInternal);
                        if (row.Dim3Id != 0 && (accountInternal = accountInternals.FirstOrDefault(a => a.AccountId == row.Dim3Id)) != null)
                            budgetRow.AccountInternal.Add(accountInternal);
                        if (row.Dim4Id != 0 && (accountInternal = accountInternals.FirstOrDefault(a => a.AccountId == row.Dim4Id)) != null)
                            budgetRow.AccountInternal.Add(accountInternal);
                        if (row.Dim5Id != 0 && (accountInternal = accountInternals.FirstOrDefault(a => a.AccountId == row.Dim5Id)) != null)
                            budgetRow.AccountInternal.Add(accountInternal);
                        if (row.Dim6Id != 0 && (accountInternal = accountInternals.FirstOrDefault(a => a.AccountId == row.Dim6Id)) != null)
                            budgetRow.AccountInternal.Add(accountInternal);

                        #endregion

                        SetModifiedProperties(budgetRow);

                        if (row.Periods != null)
                        {
                            foreach (BudgetPeriodDTO period in row.Periods)
                            {
                                BudgetRowPeriod rowPeriod = new BudgetRowPeriod()
                                {
                                    BudgetRowId = budgetRow.BudgetRowId,
                                    PeriodNr = period.PeriodNr,
                                    Amount = period.Amount,
                                    Quantity = period.Quantity,
                                    Type = period.Type.HasValue ? period.Type.Value : 0,
                                    StartTime = period.StartDate,
                                };

                                SetCreatedProperties(rowPeriod);
                                entities.BudgetRowPeriod.AddObject(rowPeriod);
                            }
                        }
                    }
                }
            }
            else
            {
                //New
                head = new BudgetHead()
                {
                    ActorCompanyId = dto.ActorCompanyId,
                    Type = dto.Type,
                    AccountYearId = dto.AccountYearId,
                    Name = dto.Name,
                    NoOfPeriods = dto.NoOfPeriods,
                    ProjectId = dto.ProjectId,
                    Status = dto.Status,
                    UseDim2 = dto.UseDim2.HasValue ? dto.UseDim2.Value : false,
                    UseDim3 = dto.UseDim3.HasValue ? dto.UseDim3.Value : false,
                    FromDate = dto.FromDate,
                    ToDate = dto.ToDate
                };

                if (dto.DistributionCodeHeadId != 0)
                    head.DistributionCodeHeadId = dto.DistributionCodeHeadId;

                head.IsAdded();
                SetCreatedProperties(head);
                entities.BudgetHead.AddObject(head);

                result = SaveChanges(entities);
                if (!result.Success)
                    return result;

                if (head.UseDim2.Value && dto.Dim2Id != 0 && (accountInternal = accountInternals.FirstOrDefault(a => a.AccountId == dto.Dim2Id)) != null)
                    head.AccountInternal.Add(accountInternal);
                if (head.UseDim3.Value && dto.Dim3Id != 0 && (accountInternal = accountInternals.FirstOrDefault(a => a.AccountId == dto.Dim3Id)) != null)
                    head.AccountInternal.Add(accountInternal);

                SetModifiedProperties(head);

                result = SaveChanges(entities, transaction);
                if (!result.Success)
                    return result;

                if (dto.Rows != null)
                {
                    foreach (BudgetRowDTO row in dto.Rows)
                    {
                        BudgetRow budgetRow = new BudgetRow()
                        {
                            BudgetHeadId = head.BudgetHeadId,
                            AccountId = row.Dim1Id.ToNullable(),
                            ShiftTypeId = row.ShiftTypeId,
                            TotalAmount = row.TotalAmount,
                            TotalQuantity = row.TotalQuantity,
                            Type = row.Type,
                            TimeCodeId = row.TimeCodeId.ToNullable()
                        };

                        if (dto.DistributionCodeHeadId != 0)
                        {
                            budgetRow.DistributionCodeHeadId = row.DistributionCodeHeadId;
                        }

                        #region Accounts

                        // Get standard account
                        budgetRow.AccountStd = AccountManager.GetAccountStd(entities, row.Dim1Id, head.ActorCompanyId, true, false);

                        if (row.Dim2Id != 0 && (accountInternal = accountInternals.FirstOrDefault(a => a.AccountId == row.Dim2Id)) != null)
                            budgetRow.AccountInternal.Add(accountInternal);
                        if (row.Dim3Id != 0 && (accountInternal = accountInternals.FirstOrDefault(a => a.AccountId == row.Dim3Id)) != null)
                            budgetRow.AccountInternal.Add(accountInternal);
                        if (row.Dim4Id != 0 && (accountInternal = accountInternals.FirstOrDefault(a => a.AccountId == row.Dim4Id)) != null)
                            budgetRow.AccountInternal.Add(accountInternal);
                        if (row.Dim5Id != 0 && (accountInternal = accountInternals.FirstOrDefault(a => a.AccountId == row.Dim5Id)) != null)
                            budgetRow.AccountInternal.Add(accountInternal);
                        if (row.Dim6Id != 0 && (accountInternal = accountInternals.FirstOrDefault(a => a.AccountId == row.Dim6Id)) != null)
                            budgetRow.AccountInternal.Add(accountInternal);

                        #endregion

                        SetCreatedProperties(budgetRow);
                        entities.BudgetRow.AddObject(budgetRow);
                        result = SaveChanges(entities);

                        if (!result.Success)
                            return result;

                        if (row.Periods != null)
                        {
                            foreach (BudgetPeriodDTO period in row.Periods)
                            {
                                BudgetRowPeriod rowPeriod = new BudgetRowPeriod()
                                {
                                    BudgetRowId = budgetRow.BudgetRowId,
                                    PeriodNr = period.PeriodNr,
                                    Amount = period.Amount,
                                    Quantity = period.Quantity,
                                    Type = period.Type.HasValue ? period.Type.Value : 0,
                                    StartTime = period.StartDate,
                                };

                                SetCreatedProperties(rowPeriod);
                                entities.BudgetRowPeriod.AddObject(rowPeriod);
                            }
                        }
                    }
                }
            }

            result = SaveChanges(entities);

            #endregion

            if (result.Success)
                result.IntegerValue = head.BudgetHeadId;

            return result;
        }

        public ActionResult SaveBudgetHeadFlattened(BudgetHeadFlattenedDTO dto)
        {
            ActionResult result = new ActionResult(true);

            try
            {
                using (CompEntities entities = new CompEntities())
                {
                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        result = SaveBudgetHeadFlattened(transaction, entities, dto);
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

        public ActionResult SaveBudgetHeadFlattened(TransactionScope transaction, CompEntities entities, BudgetHeadFlattenedDTO dto)
        {
            ActionResult result = new ActionResult(true);

            BudgetHead head = null;

            if (entities.Connection.State != ConnectionState.Open)
                entities.Connection.Open();

            #region Prereq

            if (dto == null)
                return new ActionResult(false, (int)ActionResultSave.EntityIsNull, GetText(7044, "Felaktig inparameter"));

            if (dto.BudgetHeadId != 0)
                head = GetBudgetHeadIncludingRows(entities, dto.BudgetHeadId);

            // Get internal accounts (Dim2-6)
            List<AccountInternal> accountInternals = AccountManager.GetAccountInternals(entities, base.ActorCompanyId, true);
            AccountInternal accountInternal;

            #endregion

            #region Perform

            if (head != null)
            {
                //Update
                head.Type = dto.Type;
                head.Name = dto.Name;
                head.AccountYearId = dto.AccountYearId;
                head.NoOfPeriods = dto.NoOfPeriods;
                head.Status = dto.Status;

                if (!head.AccountInternal.IsLoaded)
                    head.AccountInternal.Load();

                head.AccountInternal.Clear();

                head.UseDim2 = dto.UseDim2;
                head.UseDim3 = dto.UseDim3;

                if (head.UseDim2.HasValue && head.UseDim2.Value && dto.Dim2Id != 0 && (accountInternal = accountInternals.FirstOrDefault(a => a.AccountId == dto.Dim2Id)) != null)
                    head.AccountInternal.Add(accountInternal);
                if (head.UseDim3.HasValue && head.UseDim3.Value && dto.Dim3Id != 0 && (accountInternal = accountInternals.FirstOrDefault(a => a.AccountId == dto.Dim3Id)) != null)
                    head.AccountInternal.Add(accountInternal);



                if (dto.DistributionCodeHeadId.HasValue && dto.DistributionCodeHeadId != 0)
                {
                    head.DistributionCodeHeadId = dto.DistributionCodeHeadId;
                }

                SetModifiedProperties(head);

                result = SaveChanges(entities);

                if (!result.Success)
                    return result;

                foreach (BudgetRowFlattenedDTO row in dto.Rows)
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
                                    budgetRow.AccountId = row.Dim1Id.ToNullable();
                                    budgetRow.ShiftTypeId = row.ShiftTypeId;
                                    budgetRow.TotalAmount = row.TotalAmount;
                                    budgetRow.TotalQuantity = row.TotalQuantity;

                                    budgetRow.DistributionCodeHeadId = row.DistributionCodeHeadId.HasValue && row.DistributionCodeHeadId != 0 ? row.DistributionCodeHeadId : null;

                                    #region Accounts

                                    // Get standard account
                                    budgetRow.AccountStd = AccountManager.GetAccountStd(entities, row.Dim1Id, head.ActorCompanyId, true, false);

                                    if (!budgetRow.AccountInternal.IsLoaded)
                                        budgetRow.AccountInternal.Load();

                                    budgetRow.AccountInternal.Clear();

                                    if (row.Dim2Id != 0 && (accountInternal = accountInternals.FirstOrDefault(a => a.AccountId == row.Dim2Id)) != null)
                                        budgetRow.AccountInternal.Add(accountInternal);
                                    if (row.Dim3Id != 0 && (accountInternal = accountInternals.FirstOrDefault(a => a.AccountId == row.Dim3Id)) != null)
                                        budgetRow.AccountInternal.Add(accountInternal);
                                    if (row.Dim4Id != 0 && (accountInternal = accountInternals.FirstOrDefault(a => a.AccountId == row.Dim4Id)) != null)
                                        budgetRow.AccountInternal.Add(accountInternal);
                                    if (row.Dim5Id != 0 && (accountInternal = accountInternals.FirstOrDefault(a => a.AccountId == row.Dim5Id)) != null)
                                        budgetRow.AccountInternal.Add(accountInternal);
                                    if (row.Dim6Id != 0 && (accountInternal = accountInternals.FirstOrDefault(a => a.AccountId == row.Dim6Id)) != null)
                                        budgetRow.AccountInternal.Add(accountInternal);

                                    #endregion

                                    Type t = row.GetType();

                                    for (int i = 1; i <= 18; i++)
                                    {
                                        bool isValid = true;
                                        BudgetRowPeriod rowPeriod = null;

                                        PropertyInfo periodIdProperty = t.GetProperty("BudgetRowPeriodId" + i);
                                        if (periodIdProperty != null)
                                        {
                                            int? periodId = periodIdProperty.GetValue(row) as int?;
                                            if (periodId.HasValue && periodId.Value != 0)
                                                rowPeriod = budgetRow.BudgetRowPeriod.FirstOrDefault(p => p.BudgetRowPeriodId == periodId.Value);
                                        }
                                        else
                                        {
                                            isValid = false;
                                        }

                                        if (rowPeriod == null)
                                        {
                                            rowPeriod = new BudgetRowPeriod();
                                            rowPeriod.BudgetRowId = budgetRow.BudgetRowId;
                                        }

                                        PropertyInfo periodNrProperty = t.GetProperty("PeriodNr" + i);
                                        if (periodNrProperty != null)
                                        {
                                            int? periodNr = periodNrProperty.GetValue(row) as int?;
                                            if ((periodNr.HasValue) && (periodNr.Value > 0))
                                                rowPeriod.PeriodNr = periodNr.Value;
                                            else if ((periodNr.HasValue) && (periodNr.Value == 0))
                                                rowPeriod.PeriodNr = i;
                                            else
                                                isValid = false;
                                        }
                                        else
                                        {
                                            isValid = false;
                                        }

                                        PropertyInfo periodAmountProperty = t.GetProperty("Amount" + i);
                                        if (periodAmountProperty != null)
                                        {
                                            decimal? periodAmount = periodAmountProperty.GetValue(row) as decimal?;
                                            if (periodAmount.HasValue)
                                                rowPeriod.Amount = periodAmount.Value;
                                            else
                                                isValid = false;
                                        }
                                        else
                                        {
                                            isValid = false;
                                        }

                                        PropertyInfo periodQuantityProperty = t.GetProperty("Quantity" + i);
                                        if (periodQuantityProperty != null)
                                        {
                                            decimal? periodQuantity = periodQuantityProperty.GetValue(row) as decimal?;
                                            if (periodQuantity.HasValue)
                                                rowPeriod.Quantity = periodQuantity.Value;
                                            else
                                                isValid = false;
                                        }
                                        else
                                        {
                                            isValid = false;
                                        }

                                        PropertyInfo periodStartDateProperty = t.GetProperty("StartDate" + i);
                                        if (periodStartDateProperty != null)
                                        {
                                            rowPeriod.StartTime = periodStartDateProperty.GetValue(row) as DateTime?;
                                        }
                                        else
                                        {
                                            isValid = false;
                                        }

                                        if (isValid)
                                        {
                                            if (rowPeriod.BudgetRowPeriodId != 0)
                                            {
                                                SetModifiedProperties(rowPeriod);
                                            }
                                            else
                                            {
                                                SetCreatedProperties(rowPeriod);
                                                entities.BudgetRowPeriod.AddObject(rowPeriod);
                                            }
                                        }
                                    }

                                    #region OLD
                                    /*
                                    //Periods
                                    List<int> handledPeriodIds = new List<int>();

                                    if (row.Periods != null)
                                    {
                                        foreach (BudgetPeriodDTO period in row.Periods)
                                        {
                                            BudgetRowPeriod rowPeriod = null;

                                            if (period.BudgetRowPeriodId != 0)
                                            {
                                                rowPeriod = budgetRow.BudgetRowPeriod.Where(p => p.BudgetRowPeriodId == period.BudgetRowPeriodId).FirstOrDefault();

                                                if (rowPeriod != null)
                                                {
                                                    rowPeriod.PeriodNr = period.PeriodNr;
                                                    rowPeriod.Amount = period.Amount;
                                                    rowPeriod.Quantity = period.Quantity;
                                                    rowPeriod.Type = period.Type.HasValue ? period.Type.Value : 0;
                                                    rowPeriod.StartTime = period.StartDate;

                                                    SetModifiedProperties(rowPeriod);

                                                    handledPeriodIds.Add(rowPeriod.BudgetRowPeriodId);
                                                }
                                            }
                                            else
                                            {
                                                rowPeriod = new BudgetRowPeriod()
                                                {
                                                    BudgetRowId = budgetRow.BudgetRowId,
                                                    PeriodNr = period.PeriodNr,
                                                    Amount = period.Amount,
                                                    Quantity = period.Quantity,
                                                    Type = period.Type.HasValue ? period.Type.Value : 0,
                                                    StartTime = period.StartDate,
                                                };

                                                //AddEntityItem(entities, rowPeriod, "BudgetRowPeriod", transaction);
                                                SetCreatedProperties(rowPeriod);
                                                handledPeriodIds.Add(rowPeriod.BudgetRowPeriodId);
                                            }
                                        }

                                        row.Periods.Where(p => !handledPeriodIds.Exists(i => i == p.BudgetRowPeriodId)).ToList().ForEach(p => entities.DeleteObject(p));
                                    }*/
                                    #endregion

                                    SetModifiedProperties(budgetRow);
                                }
                            }
                        }
                    }
                    else
                    {
                        budgetRow = new BudgetRow()
                        {
                            BudgetHeadId = head.BudgetHeadId,
                            AccountId = row.Dim1Id.ToNullable(),
                            ShiftTypeId = row.ShiftTypeId,
                            TotalAmount = row.TotalAmount,
                            TotalQuantity = row.TotalQuantity,
                            Type = row.Type,
                        };

                        if (row.DistributionCodeHeadId.HasValue && row.DistributionCodeHeadId != 0)
                        {
                            budgetRow.DistributionCodeHeadId = row.DistributionCodeHeadId;
                        }

                        #region Accounts

                        // Get standard account
                        budgetRow.AccountStd = AccountManager.GetAccountStd(entities, row.Dim1Id, head.ActorCompanyId, true, false);

                        SetCreatedProperties(budgetRow);
                        entities.BudgetRow.AddObject(budgetRow);
                        result = SaveChanges(entities);

                        if (!result.Success)
                            return result;

                        if (row.Dim2Id != 0 && (accountInternal = accountInternals.FirstOrDefault(a => a.AccountId == row.Dim2Id)) != null)
                            budgetRow.AccountInternal.Add(accountInternal);
                        if (row.Dim3Id != 0 && (accountInternal = accountInternals.FirstOrDefault(a => a.AccountId == row.Dim3Id)) != null)
                            budgetRow.AccountInternal.Add(accountInternal);
                        if (row.Dim4Id != 0 && (accountInternal = accountInternals.FirstOrDefault(a => a.AccountId == row.Dim4Id)) != null)
                            budgetRow.AccountInternal.Add(accountInternal);
                        if (row.Dim5Id != 0 && (accountInternal = accountInternals.FirstOrDefault(a => a.AccountId == row.Dim5Id)) != null)
                            budgetRow.AccountInternal.Add(accountInternal);
                        if (row.Dim6Id != 0 && (accountInternal = accountInternals.FirstOrDefault(a => a.AccountId == row.Dim6Id)) != null)
                            budgetRow.AccountInternal.Add(accountInternal);

                        #endregion

                        SetModifiedProperties(budgetRow);

                        Type t = row.GetType();

                        for (int i = 1; i <= 18; i++)
                        {
                            bool isValid = true;
                            BudgetRowPeriod rowPeriod = new BudgetRowPeriod();
                            rowPeriod.BudgetRowId = budgetRow.BudgetRowId;

                            PropertyInfo periodNrProperty = t.GetProperty("PeriodNr" + i);
                            if (periodNrProperty != null)
                            {
                                int? periodNr = periodNrProperty.GetValue(row) as int?;
                                if (periodNr.HasValue)
                                    rowPeriod.PeriodNr = i;
                                //rowPeriod.PeriodNr = periodNr.Value;
                                else
                                    isValid = false;
                            }
                            else
                            {
                                isValid = false;
                            }

                            PropertyInfo periodAmountProperty = t.GetProperty("Amount" + i);
                            if (periodAmountProperty != null)
                            {
                                decimal? periodAmount = periodAmountProperty.GetValue(row) as decimal?;
                                if (periodAmount.HasValue)
                                    rowPeriod.Amount = periodAmount.Value;
                                else
                                    isValid = false;
                            }
                            else
                            {
                                isValid = false;
                            }

                            PropertyInfo periodQuantityProperty = t.GetProperty("Quantity" + i);
                            if (periodQuantityProperty != null)
                            {
                                decimal? periodQuantity = periodQuantityProperty.GetValue(row) as decimal?;
                                if (periodQuantity.HasValue)
                                    rowPeriod.Quantity = periodQuantity.Value;
                                else
                                    isValid = false;
                            }
                            else
                            {
                                isValid = false;
                            }

                            PropertyInfo periodStartDateProperty = t.GetProperty("StartDate" + i);
                            if (periodStartDateProperty != null)
                            {
                                rowPeriod.StartTime = periodStartDateProperty.GetValue(row) as DateTime?;
                            }
                            else
                            {
                                isValid = false;
                            }

                            if (isValid)
                            {
                                SetCreatedProperties(rowPeriod);
                                entities.BudgetRowPeriod.AddObject(rowPeriod);
                            }
                        }

                        #region OLD
                        /*if (row.Periods != null)
                        {
                            foreach (BudgetPeriodDTO period in row.Periods)
                            {
                                BudgetRowPeriod rowPeriod = new BudgetRowPeriod()
                                {
                                    BudgetRowId = budgetRow.BudgetRowId,
                                    PeriodNr = period.PeriodNr,
                                    Amount = period.Amount,
                                    Quantity = period.Quantity,
                                    Type = period.Type.HasValue ? period.Type.Value : 0,
                                    StartTime = period.StartDate,
                                };

                                SetCreatedProperties(rowPeriod);
                            }
                        }*/
                        #endregion
                    }
                }
            }
            else
            {
                //New
                head = new BudgetHead()
                {
                    ActorCompanyId = base.ActorCompanyId,
                    Type = dto.Type,
                    AccountYearId = dto.AccountYearId,
                    Name = dto.Name,
                    NoOfPeriods = dto.NoOfPeriods,
                    ProjectId = dto.ProjectId,
                    Status = dto.Status,
                    UseDim2 = dto.UseDim2.HasValue ? dto.UseDim2.Value : false,
                    UseDim3 = dto.UseDim3.HasValue ? dto.UseDim3.Value : false,
                };

                if (dto.DistributionCodeHeadId.HasValue && dto.DistributionCodeHeadId != 0)
                    head.DistributionCodeHeadId = dto.DistributionCodeHeadId;

                head.IsAdded();
                SetCreatedProperties(head);
                entities.BudgetHead.AddObject(head);
                result = SaveChanges(entities);

                if (!result.Success)
                    return result;

                if (head.UseDim2.Value && dto.Dim2Id != 0 && (accountInternal = accountInternals.FirstOrDefault(a => a.AccountId == dto.Dim2Id)) != null)
                    head.AccountInternal.Add(accountInternal);
                if (head.UseDim3.Value && dto.Dim3Id != 0 && (accountInternal = accountInternals.FirstOrDefault(a => a.AccountId == dto.Dim3Id)) != null)
                    head.AccountInternal.Add(accountInternal);

                

                result = SaveChanges(entities, transaction);
                if (!result.Success)
                    return result;

                if (dto.Rows != null)
                {
                    foreach (BudgetRowFlattenedDTO row in dto.Rows)
                    {
                        BudgetRow budgetRow = new BudgetRow()
                        {
                            BudgetHeadId = head.BudgetHeadId,
                            AccountId = row.Dim1Id.ToNullable(),
                            ShiftTypeId = row.ShiftTypeId,
                            TotalAmount = row.TotalAmount,
                            TotalQuantity = row.TotalQuantity,
                            Type = row.Type,
                        };

                        if (row.DistributionCodeHeadId.HasValue && row.DistributionCodeHeadId != 0)
                        {
                            budgetRow.DistributionCodeHeadId = row.DistributionCodeHeadId;
                        }

                        #region Accounts

                        // Get standard account
                        budgetRow.AccountStd = AccountManager.GetAccountStd(entities, row.Dim1Id, head.ActorCompanyId, true, false);

                        if (row.Dim2Id != 0 && (accountInternal = accountInternals.FirstOrDefault(a => a.AccountId == row.Dim2Id)) != null)
                            budgetRow.AccountInternal.Add(accountInternal);
                        if (row.Dim3Id != 0 && (accountInternal = accountInternals.FirstOrDefault(a => a.AccountId == row.Dim3Id)) != null)
                            budgetRow.AccountInternal.Add(accountInternal);
                        if (row.Dim4Id != 0 && (accountInternal = accountInternals.FirstOrDefault(a => a.AccountId == row.Dim4Id)) != null)
                            budgetRow.AccountInternal.Add(accountInternal);
                        if (row.Dim5Id != 0 && (accountInternal = accountInternals.FirstOrDefault(a => a.AccountId == row.Dim5Id)) != null)
                            budgetRow.AccountInternal.Add(accountInternal);
                        if (row.Dim6Id != 0 && (accountInternal = accountInternals.FirstOrDefault(a => a.AccountId == row.Dim6Id)) != null)
                            budgetRow.AccountInternal.Add(accountInternal);

                        #endregion

                        SetCreatedProperties(budgetRow);
                        entities.BudgetRow.AddObject(budgetRow);

                        result = SaveChanges(entities);

                        if (!result.Success)
                            return result;

                        Type t = row.GetType();

                        for (int i = 1; i <= 18; i++)
                        {
                            bool isValid = true;
                            BudgetRowPeriod rowPeriod = new BudgetRowPeriod();
                            rowPeriod.BudgetRowId = budgetRow.BudgetRowId;

                            PropertyInfo periodNrProperty = t.GetProperty("PeriodNr" + i);
                            if (periodNrProperty != null)
                            {
                                int? periodNr = periodNrProperty.GetValue(row) as int?;
                                if (periodNr.HasValue)
                                    rowPeriod.PeriodNr = i;
                                else
                                    isValid = false;
                            }
                            else
                            {
                                isValid = false;
                            }

                            PropertyInfo periodAmountProperty = t.GetProperty("Amount" + i);
                            if (periodAmountProperty != null)
                            {
                                decimal? periodAmount = periodAmountProperty.GetValue(row) as decimal?;
                                if (periodAmount.HasValue)
                                    rowPeriod.Amount = periodAmount.Value;
                                else
                                    isValid = false;
                            }
                            else
                            {
                                isValid = false;
                            }

                            PropertyInfo periodQuantityProperty = t.GetProperty("Quantity" + i);
                            if (periodQuantityProperty != null)
                            {
                                decimal? periodQuantity = periodQuantityProperty.GetValue(row) as decimal?;
                                if (periodQuantity.HasValue)
                                    rowPeriod.Quantity = periodQuantity.Value;
                                else
                                    isValid = false;
                            }
                            else
                            {
                                isValid = false;
                            }

                            PropertyInfo periodStartDateProperty = t.GetProperty("StartDate" + i);
                            if (periodStartDateProperty != null)
                            {
                                rowPeriod.StartTime = periodStartDateProperty.GetValue(row) as DateTime?;
                            }
                            else
                            {
                                isValid = false;
                            }

                            if (isValid)
                            {
                                SetCreatedProperties(rowPeriod);
                                entities.BudgetRowPeriod.AddObject(rowPeriod);
                            }
                        }

                        #region OLD
                        /*if (row.Periods != null)
                        {
                            foreach (BudgetPeriodDTO period in row.Periods)
                            {
                                BudgetRowPeriod rowPeriod = new BudgetRowPeriod()
                                {
                                    BudgetRowId = budgetRow.BudgetRowId,
                                    PeriodNr = period.PeriodNr,
                                    Amount = period.Amount,
                                    Quantity = period.Quantity,
                                    Type = period.Type.HasValue ? period.Type.Value : 0,
                                    StartTime = period.StartDate,
                                };

                                SetCreatedProperties(rowPeriod);
                            }
                        }*/
                        #endregion
                    }
                }
            }

            result = SaveChanges(entities);

            #endregion

            if (result.Success)
                result.IntegerValue = head.BudgetHeadId;

            return result;
        }
        
        public ActionResult CreateBudgetFromSales(int actorCompanyId, DateTime dateFrom, DateTime dateTo)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            var frequencies = base.GetStaffingNeedsFrequencyFromCache(entities, actorCompanyId, dateFrom, dateTo);
            frequencies = frequencies.Where(w => w.Amount > 0 && w.AccountId.HasValue).ToList();
            decimal factor = Convert.ToDecimal(365 / (dateTo - dateFrom).TotalDays);
            var codes = BudgetManager.GetDistributionCodesByType(actorCompanyId, (int)DistributionCodeBudgetType.SalesBudget, true);
            var accountStds = AccountManager.GetAccountsStdsByCompany(actorCompanyId, loadAccount: true);

            var salesAccount = accountStds.OrderBy(o => o.AccountNr).FirstOrDefault(f => f.AccountNr.StartsWith("30"));

            if (salesAccount == null)
                salesAccount = accountStds.FirstOrDefault();

            var yearlyCode = codes.FirstOrDefault(f => f.SubType == (int)TermGroup_AccountingBudgetSubType.Year);

            if (yearlyCode == null)
                return new ActionResult(new Exception("Code for Year is missing "), " Create Code and try again");

            BudgetHeadDTO head = new BudgetHeadDTO()
            {
                ActorCompanyId = actorCompanyId,
                Type = (int)DistributionCodeBudgetType.SalesBudget,
                Name = "Year",
                NoOfPeriods = 12,
                Status = (int)BudgetHeadStatus.Active,
                UseDim2 = true,
                Rows = new List<BudgetRowDTO>(),
                DistributionCodeHeadId = yearlyCode.DistributionCodeHeadId
            };

            var groupOnAccountId = frequencies.GroupBy(i => i.AccountId);

            foreach (var group in groupOnAccountId)
            {
                BudgetRowDTO row = new BudgetRowDTO()
                {
                    Dim2Id = group.Key.Value,
                    DistributionCodeHeadId = yearlyCode.DistributionCodeHeadId,
                    TotalAmount = decimal.Multiply(group.Sum(s => s.Amount), factor),
                    Dim1Id = salesAccount.AccountId
                };

                head.Rows.Add(row);
            }

            return SaveSalesBudgetHead(head);

        }

        public ActionResult CreateBudgetTimeFromSchedule(int actorCompanyId, DateTime dateFrom, DateTime dateTo)
        {
            using var entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            List<TimeScheduleTemplateBlock> shifts = (from t in entitiesReadOnly.TimeScheduleTemplateBlock.Include("AccountInternal")
                                                      where t.TimeScheduleTemplatePeriod.TimeScheduleTemplateHead.ActorCompanyId == actorCompanyId &&
                                                      t.Date.HasValue &&
                                                      t.Date.Value >= dateFrom &&
                                                      t.Date.Value <= dateTo &&
                                                      t.ShiftTypeId.HasValue &&
                                                      t.State == (int)SoeEntityState.Active
                                                      select t).ToList();

            shifts = shifts.Where(w => w.StartTime != w.StopTime && !w.IsBreak && !w.TimeScheduleScenarioHeadId.HasValue).ToList();

            var accountDims = AccountManager.GetAccountDimsByCompany(actorCompanyId);
            var accountDim = accountDims.FirstOrDefault(f => f.SysSieDimNr == (int)TermGroup_SieAccountDim.CostCentre);
            var accounts = AccountManager.GetAccounts(actorCompanyId).Where(w => w.AccountDimId == accountDim.AccountDimId).ToList();

            List<Tuple<int, decimal>> timePerAccount = new List<Tuple<int, decimal>>();

            foreach (var shift in shifts)
            {
                foreach (var ai in shift.AccountInternal)
                {
                    if (accounts.Any(a => a.AccountId == ai.AccountId))
                        timePerAccount.Add(Tuple.Create(ai.AccountId, decimal.Multiply(shift.TotalMinutes, new decimal(0.8))));
                }
            }

            Dictionary<int, decimal> timePerAccountDict = new Dictionary<int, decimal>();

            foreach (var a in timePerAccount.GroupBy(g => g.Item1))
                timePerAccountDict.Add(a.First().Item1, a.Sum(s => s.Item2));

            decimal factor = Convert.ToDecimal(365 / (dateTo - dateFrom).TotalDays);
            var codes = BudgetManager.GetDistributionCodesByType(actorCompanyId, (int)DistributionCodeBudgetType.SalesBudgetTime, true);
            var accountStds = AccountManager.GetAccountsStdsByCompany(actorCompanyId, loadAccount: true);

            var salesAccount = accountStds.OrderBy(o => o.AccountNr).FirstOrDefault(f => f.AccountNr.StartsWith("30"));

            if (salesAccount == null)
                salesAccount = accountStds.FirstOrDefault();

            var yearlyCode = codes.FirstOrDefault(f => f.SubType == (int)TermGroup_AccountingBudgetSubType.Year);

            if (yearlyCode == null)
                return new ActionResult(new Exception("Code for Year is missing "), " Create Code and try again");

            BudgetHeadDTO head = new BudgetHeadDTO()
            {
                ActorCompanyId = actorCompanyId,
                Type = (int)DistributionCodeBudgetType.SalesBudgetTime,
                Name = "Year time",
                NoOfPeriods = 12,
                Status = (int)BudgetHeadStatus.Active,
                UseDim2 = true,
                Rows = new List<BudgetRowDTO>(),
                DistributionCodeHeadId = yearlyCode.DistributionCodeHeadId
            };

            foreach (var group in timePerAccountDict)
            {
                BudgetRowDTO row = new BudgetRowDTO()
                {
                    Dim2Id = group.Key,
                    DistributionCodeHeadId = yearlyCode.DistributionCodeHeadId,
                    TotalQuantity = decimal.Multiply(group.Value, factor),
                    Dim1Id = salesAccount.AccountId
                };

                head.Rows.Add(row);
            }

            return SaveSalesBudgetHead(head);

        }

        public ActionResult SaveSalesBudgetHead(BudgetHeadDTO dto, bool import = false)
        {
            ActionResult result = new ActionResult(true);

            try
            {
                using (CompEntities entities = new CompEntities())
                {
                    entities.CommandTimeout = 60 * 60 * 2;
                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        result = SaveSalesBudgetHead(transaction, entities, dto, import);

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

        public ActionResult SaveSalesBudgetHeadV2(BudgetHeadSalesDTO dto)
        {
            ActionResult result = new ActionResult(true);

            try
            {
                using (CompEntities entities = new CompEntities())
                {
                    entities.CommandTimeout = 60 * 60 * 2;
                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        result = SaveSalesBudgetHeadV2(transaction, entities, dto);

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

        public ActionResult SaveSalesBudgetHead(TransactionScope transaction, CompEntities entities, BudgetHeadDTO dto, bool import = false)
        {
            ActionResult result = new ActionResult(true);
            BudgetHead head = null;

            if (entities.Connection.State != ConnectionState.Open)
                entities.Connection.Open();
            //Always distribute to lowest level a.k.a hours 

            #region Prereq

            if (dto == null)
                return new ActionResult(false, (int)ActionResultSave.EntityIsNull, GetText(7044, "Felaktig inparameter"));

            if (dto.BudgetHeadId != 0)
                head = GetBudgetHeadIncludingRows(entities, dto.BudgetHeadId);

            // Get internal accounts (Dim2-6)
            List<AccountInternal> accountInternals = AccountManager.GetAccountInternals(entities, base.ActorCompanyId, true);
            AccountInternal accountInternal;

            //Get distributionCodes
            List<DistributionCodeHead> distributionCodeHeads = GetDistributionCodes(entities, base.ActorCompanyId, true);
            DistributionCodeHead headDistributionCode = GetDistributionCode(entities, base.ActorCompanyId, (int)dto.DistributionCodeHeadId);

            #endregion

            #region Perform

            if (head != null)
            {
                //Update
                head.Type = dto.Type;
                head.Name = dto.Name;
                head.AccountYearId = dto.AccountYearId;
                head.NoOfPeriods = dto.NoOfPeriods;
                head.Status = dto.Status;

                if (!head.AccountInternal.IsLoaded)
                    head.AccountInternal.Load();

                head.AccountInternal.Clear();

                head.UseDim2 = true;
                head.UseDim3 = dto.UseDim3;

                head.FromDate = dto.FromDate;
                head.ToDate = dto.ToDate;

                if (head.UseDim2.HasValue && head.UseDim2.Value && dto.Dim2Id != 0 && (accountInternal = accountInternals.FirstOrDefault(a => a.AccountId == dto.Dim2Id)) != null)
                    head.AccountInternal.Add(accountInternal);
                if (head.UseDim3.HasValue && head.UseDim3.Value && dto.Dim3Id != 0 && (accountInternal = accountInternals.FirstOrDefault(a => a.AccountId == dto.Dim3Id)) != null)
                    head.AccountInternal.Add(accountInternal);

                SetModifiedProperties(head);

                result = SaveChanges(entities);
                if (!result.Success)
                    return result;

                foreach (BudgetRowDTO row in dto.Rows)
                {
                    BudgetRow budgetRow = null;
                    int subType = (int)headDistributionCode.SubType;
                    //Subtypes 0=year 1=mont , 2=week, 3=day 

                    if (row.BudgetRowId != 0)
                    {
                        //Save only lowest level = day
                    }
                    else
                    {
                        budgetRow = new BudgetRow()
                        {
                            BudgetHeadId = head.BudgetHeadId,
                            AccountId = row.Dim2Id.ToNullable(),
                            ShiftTypeId = row.ShiftTypeId,
                            TotalAmount = row.TotalAmount,
                            TotalQuantity = row.TotalQuantity,
                            Type = row.Type,
                        };

                        if (dto.DistributionCodeHeadId != 0)
                        {
                            budgetRow.DistributionCodeHeadId = row.DistributionCodeHeadId;
                        }

                        #region Accounts

                        // Get standard account
                        budgetRow.AccountStd = AccountManager.GetAccountStd(entities, row.Dim1Id, head.ActorCompanyId, true, false);

                        SetCreatedProperties(budgetRow);
                        entities.BudgetRow.AddObject(budgetRow);
                        result = SaveChanges(entities);

                        if (!result.Success)
                            return result;

                        budgetRow.AccountInternal = new System.Data.Entity.Core.Objects.DataClasses.EntityCollection<AccountInternal>();

                        if (row.Dim2Id != 0 && (accountInternal = accountInternals.FirstOrDefault(a => a.AccountId == row.Dim2Id)) != null)
                            budgetRow.AccountInternal.Add(accountInternal);
                        if (row.Dim3Id != 0 && (accountInternal = accountInternals.FirstOrDefault(a => a.AccountId == row.Dim3Id)) != null)
                            budgetRow.AccountInternal.Add(accountInternal);
                        if (row.Dim4Id != 0 && (accountInternal = accountInternals.FirstOrDefault(a => a.AccountId == row.Dim4Id)) != null)
                            budgetRow.AccountInternal.Add(accountInternal);
                        if (row.Dim5Id != 0 && (accountInternal = accountInternals.FirstOrDefault(a => a.AccountId == row.Dim5Id)) != null)
                            budgetRow.AccountInternal.Add(accountInternal);
                        if (row.Dim6Id != 0 && (accountInternal = accountInternals.FirstOrDefault(a => a.AccountId == row.Dim6Id)) != null)
                            budgetRow.AccountInternal.Add(accountInternal);

                        #endregion

                        SetModifiedProperties(budgetRow);

                        if (subType == 0) //Year
                        {
                            //Get week Skipped with new distribution
                            int rowNr = 1;
                            int month = 1;
                            foreach (var distributionRowPeriodMonth in headDistributionCode.DistributionCodePeriod)
                            {
                                var amountMonth = row.TotalAmount * (distributionRowPeriodMonth.Percent / 100);
                                int day = 1;
                                //Get month rules (now changed so, that each month has its own distribution rule)
                                DistributionCodeHead weekDistributionCode = GetDistributionCodeByParentAndSubType(entities, base.ActorCompanyId, headDistributionCode.DistributionCodeHeadId, (int)TermGroup_AccountingBudgetSubType.Week, distributionCodeHeads);

                                if (weekDistributionCode == null)
                                    return new ActionResult(new Exception("Week Distribution missing"), " Set up weekly distribution");

                                //Get day
                                DistributionCodeHead dayDistributionCode = GetDistributionCodeByParentAndSubType(entities, base.ActorCompanyId, weekDistributionCode.DistributionCodeHeadId, (int)TermGroup_AccountingBudgetSubType.Day, distributionCodeHeads);

                                while (day <= DateTime.DaysInMonth(DateTime.Now.Year, month))
                                {
                                    int dayNr = 0;
                                    foreach (var distributionRowPeriodDay in weekDistributionCode.DistributionCodePeriod)
                                    {
                                        dayNr++;
                                        if (day <= DateTime.DaysInMonth(DateTime.Now.Year, month))
                                        {

                                            if (dayNr != CalendarUtility.GetDayNr(new DateTime(DateTime.Now.Year, month, day)))
                                                continue;

                                            var amountDay = (amountMonth / DateTime.DaysInMonth(DateTime.Now.Year, month)) * 7 * (distributionRowPeriodDay.Percent / 100);
                                            int hour = 0;

                                            foreach (var distributionRowPeriodHour in dayDistributionCode.DistributionCodePeriod)
                                            {
                                                //create date for period
                                                var dateTimePeriod = new DateTime(DateTime.Now.Year, month, day, hour, 0, 0);
                                                var amountHour = amountDay * (distributionRowPeriodHour.Percent / 100);
                                                BudgetRowPeriod rowPeriod = new BudgetRowPeriod();
                                                rowPeriod.BudgetRowId = budgetRow.BudgetRowId;
                                                rowPeriod.Amount = amountHour;
                                                rowPeriod.StartTime = dateTimePeriod;
                                                rowPeriod.Quantity = 1;
                                                rowPeriod.Type = (int)BudgetRowPeriodType.Hour;
                                                rowPeriod.PeriodNr = rowNr;
                                                rowNr = rowNr + 1;
                                                SetCreatedProperties(rowPeriod);
                                                entities.BudgetRowPeriod.AddObject(rowPeriod);
                                                hour = hour + 1;
                                            }
                                        }
                                        day = day + 1;
                                    }
                                }
                                month = month + 1;
                            }
                        }
                    }
                }
            }
            else
            {
                //New Budget, Always have whole year setted as "base"
                head = new BudgetHead()
                {
                    ActorCompanyId = base.ActorCompanyId,
                    Type = dto.Type,
                    AccountYearId = dto.AccountYearId,
                    Name = dto.Name,
                    NoOfPeriods = dto.NoOfPeriods,
                    ProjectId = dto.ProjectId,
                    Status = dto.Status,
                    UseDim2 = dto.UseDim2.HasValue ? dto.UseDim2.Value : false,
                    UseDim3 = dto.UseDim3.HasValue ? dto.UseDim3.Value : false,
                    FromDate = dto.FromDate,
                    ToDate = dto.ToDate
                };

                if (dto.DistributionCodeHeadId != 0)
                    head.DistributionCodeHeadId = dto.DistributionCodeHeadId;

                head.IsAdded();
                SetCreatedProperties(head);
                entities.BudgetHead.AddObject(head);
                result = SaveChanges(entities);

                if (!result.Success)
                    return result;

                if (head.UseDim2.Value && dto.Dim2Id != 0 && (accountInternal = accountInternals.FirstOrDefault(a => a.AccountId == dto.Dim2Id)) != null)
                    head.AccountInternal.Add(accountInternal);
                if (head.UseDim3.Value && dto.Dim3Id != 0 && (accountInternal = accountInternals.FirstOrDefault(a => a.AccountId == dto.Dim3Id)) != null)
                    head.AccountInternal.Add(accountInternal);

                SetModifiedProperties(head);

                result = SaveChanges(entities, transaction);
                if (!result.Success)
                    return result;

                if (dto.Rows != null)
                {
                    foreach (BudgetRowDTO row in dto.Rows)
                    {
                        BudgetRow budgetRow = new BudgetRow()
                        {
                            BudgetHeadId = head.BudgetHeadId,
                            AccountId = row.Dim1Id.ToNullable(),
                            ShiftTypeId = row.ShiftTypeId,
                            TotalAmount = row.TotalAmount,
                            TotalQuantity = row.TotalQuantity,
                            Type = row.Type,
                            AccountStd = null,
                        };
                        TermGroup_AccountingBudgetSubType subType = (TermGroup_AccountingBudgetSubType)headDistributionCode.SubType;

                        if (dto.DistributionCodeHeadId != 0)
                        {
                            budgetRow.DistributionCodeHeadId = row.DistributionCodeHeadId;
                        }

                        #region AccountDim 

                        if (row.Dim1Id != 0 && (accountInternal = accountInternals.FirstOrDefault(a => a.AccountId == row.Dim1Id)) != null)
                            budgetRow.AccountInternal.Add(accountInternal);

                        #endregion

                        // Get standard account
                        budgetRow.AccountStd = AccountManager.GetAccountStd(entities, row.Dim1Id, head.ActorCompanyId, true, false);

                        if (row.Dim2Id != 0 && (accountInternal = accountInternals.FirstOrDefault(a => a.AccountId == row.Dim2Id)) != null)
                            budgetRow.AccountInternal.Add(accountInternal);
                        if (row.Dim3Id != 0 && (accountInternal = accountInternals.FirstOrDefault(a => a.AccountId == row.Dim3Id)) != null)
                            budgetRow.AccountInternal.Add(accountInternal);
                        if (row.Dim4Id != 0 && (accountInternal = accountInternals.FirstOrDefault(a => a.AccountId == row.Dim4Id)) != null)
                            budgetRow.AccountInternal.Add(accountInternal);
                        if (row.Dim5Id != 0 && (accountInternal = accountInternals.FirstOrDefault(a => a.AccountId == row.Dim5Id)) != null)
                            budgetRow.AccountInternal.Add(accountInternal);
                        if (row.Dim6Id != 0 && (accountInternal = accountInternals.FirstOrDefault(a => a.AccountId == row.Dim6Id)) != null)
                            budgetRow.AccountInternal.Add(accountInternal);

                        SetCreatedProperties(budgetRow);
                        entities.BudgetRow.AddObject(budgetRow);

                        if (!result.Success)
                            return result;

                        if (subType == 0) //Year
                        {
                            //Get week Skipped with new distribution
                            int rowNr = 1;
                            int month = 1;
                            foreach (var distributionRowPeriodMonth in headDistributionCode.DistributionCodePeriod)
                            {
                                var amountMonth = row.TotalAmount * (distributionRowPeriodMonth.Percent / 100);
                                int day = 1;
                                //Get month rules (now changed so, that each month has its own distribution rule)
                                DistributionCodeHead weekDistributionCode = GetDistributionCodeByParentAndSubType(entities, base.ActorCompanyId, headDistributionCode.DistributionCodeHeadId, (int)TermGroup_AccountingBudgetSubType.Week, distributionCodeHeads);

                                if (weekDistributionCode == null)
                                    return new ActionResult(new Exception("Week Distribution missing"), " Set up weekly distribution");

                                //Get day
                                DistributionCodeHead dayDistributionCode = GetDistributionCodeByParentAndSubType(entities, base.ActorCompanyId, weekDistributionCode.DistributionCodeHeadId, (int)TermGroup_AccountingBudgetSubType.Day, distributionCodeHeads);

                                while (day <= DateTime.DaysInMonth(DateTime.Now.Year, month))
                                {
                                    int dayNr = 0;
                                    foreach (var distributionRowPeriodDay in weekDistributionCode.DistributionCodePeriod)
                                    {
                                        dayNr++;
                                        if (day <= DateTime.DaysInMonth(DateTime.Now.Year, month))
                                        {
                                            if (dayNr != CalendarUtility.GetDayNr(new DateTime(DateTime.Now.Year, month, day)))
                                                continue;

                                            var amountDay = (amountMonth / DateTime.DaysInMonth(DateTime.Now.Year, month)) * 7 * (distributionRowPeriodDay.Percent / 100);
                                            int hour = 0;

                                            foreach (var distributionRowPeriodHour in dayDistributionCode.DistributionCodePeriod)
                                            {
                                                //create date for period
                                                var dateTimePeriod = new DateTime(DateTime.Now.Year, month, day, hour, 0, 0);
                                                var amountHour = amountDay * (distributionRowPeriodHour.Percent / 100);
                                                BudgetRowPeriod rowPeriod = new BudgetRowPeriod();
                                                rowPeriod.BudgetRowId = budgetRow.BudgetRowId;
                                                rowPeriod.Amount = amountHour;
                                                rowPeriod.StartTime = dateTimePeriod;
                                                rowPeriod.Quantity = 1;
                                                rowPeriod.Type = (int)BudgetRowPeriodType.Hour;
                                                rowPeriod.PeriodNr = rowNr;
                                                rowNr = rowNr + 1;
                                                SetCreatedProperties(rowPeriod);
                                                entities.BudgetRowPeriod.AddObject(rowPeriod);
                                                hour = hour + 1;
                                            }
                                        }
                                        day = day + 1;
                                    }
                                }
                                month = month + 1;
                            }
                            result = SaveChanges(entities);
                        }

                        result = SaveChanges(entities);
                    }
                }
            }

            result = SaveChanges(entities, transaction, true, true);

            #endregion

            if (result.Success)
                result.IntegerValue = head.BudgetHeadId;

            return result;
        }

        public ActionResult SaveSalesBudgetHeadV2(TransactionScope transaction, CompEntities entities, BudgetHeadSalesDTO dto)
        {
            ActionResult result = new ActionResult(true);
            BudgetHead head = null;

            if (entities.Connection.State != ConnectionState.Open)
                entities.Connection.Open();

            //Always distribute to lowest level a.k.a hours 
            #region Prereq

            if (dto == null)
                return new ActionResult(false, (int)ActionResultSave.EntityIsNull, GetText(7044, "Felaktig inparameter"));

            if (dto.BudgetHeadId != 0)
                head = GetBudgetHeadIncludingRows(entities, dto.BudgetHeadId);

            // Get internal accounts (Dim2-6)
            List<AccountInternal> accountInternals = AccountManager.GetAccountInternals(entities, base.ActorCompanyId, true);

            // Get distributioncode
            var distributionCode = GetDistributionCode(entities, base.ActorCompanyId, dto.DistributionCodeHeadId.Value);

            #endregion

            #region Perform

            if (head != null)
            {
                //Update
                head.Type = dto.Type;
                head.Name = dto.Name;
                head.NoOfPeriods = dto.NoOfPeriods;
                head.Status = dto.Status;
                head.FromDate = dto.FromDate;
                head.ToDate = dto.ToDate;

                head.DistributionCodeHeadId = dto.DistributionCodeHeadId.HasValue && dto.DistributionCodeHeadId.Value != 0 ? dto.DistributionCodeHeadId.Value : (int?)null;

                SetModifiedProperties(head);

                if (!result.Success)
                    return result;

                foreach (BudgetRowSalesDTO row in dto.Rows)
                {
                    if (row.BudgetRowId > 0)
                    {
                        var existingRow = head.BudgetRow.FirstOrDefault(r => r.BudgetRowId == row.BudgetRowId);
                        if (existingRow != null)
                        {
                            if (row.IsDeleted)
                            {
                                existingRow.State = (int)SoeEntityState.Deleted;
                                SetModifiedProperties(existingRow);

                                result = SaveChanges(entities);

                                if (!result.Success)
                                    return result;
                            }
                            else
                            {
                                UpdateSalesBudgetRow(entities, head, existingRow, row, accountInternals, distributionCode);
                            }
                        }
                        else
                        {
                            AddSalesBudgetRow(entities, head, row, accountInternals, distributionCode);
                        }
                    }
                    else
                    {
                        AddSalesBudgetRow(entities, head, row, accountInternals, distributionCode);
                    }
                }
            }
            else
            {
                head = new BudgetHead()
                {
                    ActorCompanyId = base.ActorCompanyId,
                    Type = dto.Type,
                    Name = dto.Name,
                    NoOfPeriods = dto.NoOfPeriods,
                    Status = dto.Status,
                    FromDate = dto.FromDate,
                    ToDate = dto.ToDate
                };

                // Distribution code budget is based on
                if (dto.DistributionCodeHeadId != 0)
                    head.DistributionCodeHeadId = dto.DistributionCodeHeadId;

                head.IsAdded();
                SetCreatedProperties(head);
                entities.BudgetHead.AddObject(head);

                if (!result.Success)
                    return result;

                if (dto.Rows != null)
                {
                    foreach (var row in dto.Rows)
                    {
                        AddSalesBudgetRow(entities, head, row, accountInternals, distributionCode);
                    }
                }
            }

            result = SaveChanges(entities, transaction);

            #endregion

            if (result.Success)
                result.IntegerValue = head.BudgetHeadId;

            return result;
        }

        private void AddSalesBudgetRow(CompEntities entities, BudgetHead head, BudgetRowSalesDTO row, List<AccountInternal> accountInternals, DistributionCodeHead distributionCode)
        {
            AccountInternal accountInternal;

            BudgetRow budgetRow = new BudgetRow()
            {
                BudgetHead = head,
                //AccountId = row.Dim1Id.ToNullable(),
                TotalAmount = row.TotalAmount,
                TotalQuantity = row.TotalQuantity,
                Type = row.Type,
                //AccountStd = null,
            };

            // Set internal accounts
            if (row.Dim1Id != 0 && (accountInternal = accountInternals.FirstOrDefault(a => a.AccountId == row.Dim1Id)) != null)
                budgetRow.AccountInternal.Add(accountInternal);
            if (row.Dim2Id != 0 && (accountInternal = accountInternals.FirstOrDefault(a => a.AccountId == row.Dim2Id)) != null)
                budgetRow.AccountInternal.Add(accountInternal);
            if (row.Dim3Id != 0 && (accountInternal = accountInternals.FirstOrDefault(a => a.AccountId == row.Dim3Id)) != null)
                budgetRow.AccountInternal.Add(accountInternal);
            if (row.Dim4Id != 0 && (accountInternal = accountInternals.FirstOrDefault(a => a.AccountId == row.Dim4Id)) != null)
                budgetRow.AccountInternal.Add(accountInternal);
            if (row.Dim5Id != 0 && (accountInternal = accountInternals.FirstOrDefault(a => a.AccountId == row.Dim5Id)) != null)
                budgetRow.AccountInternal.Add(accountInternal);
            if (row.Dim6Id != 0 && (accountInternal = accountInternals.FirstOrDefault(a => a.AccountId == row.Dim6Id)) != null)
                budgetRow.AccountInternal.Add(accountInternal);


            SetCreatedProperties(budgetRow);
            entities.BudgetRow.AddObject(budgetRow);

            var periodsToSave = GetPeriodsToSave(row.Periods, "");
            foreach (var period in periodsToSave)
            {
                AddSalesBudgetPeriod(entities, budgetRow, period, new List<BudgetRowPeriod>(), distributionCode, head.FromDate.HasValue ? head.FromDate.Value.Year : DateTime.Today.Year);
            }
        }

        private void UpdateSalesBudgetRow(CompEntities entities, BudgetHead head, BudgetRow existingRow, BudgetRowSalesDTO row, List<AccountInternal> accountInternals, DistributionCodeHead distributionCode)
        {
            AccountInternal accountInternal;

            //existingRow.AccountId = row.Dim1Id.ToNullable();
            existingRow.TotalAmount = row.TotalAmount;
            existingRow.TotalQuantity = row.TotalQuantity;
            existingRow.Type = row.Type;

            // Remove internal accounts
            existingRow.AccountInternal.Clear();

            // Set internal accounts
            if (row.Dim1Id != 0 && (accountInternal = accountInternals.FirstOrDefault(a => a.AccountId == row.Dim1Id)) != null)
                existingRow.AccountInternal.Add(accountInternal);
            if (row.Dim2Id != 0 && (accountInternal = accountInternals.FirstOrDefault(a => a.AccountId == row.Dim2Id)) != null)
                existingRow.AccountInternal.Add(accountInternal);
            if (row.Dim3Id != 0 && (accountInternal = accountInternals.FirstOrDefault(a => a.AccountId == row.Dim3Id)) != null)
                existingRow.AccountInternal.Add(accountInternal);
            if (row.Dim4Id != 0 && (accountInternal = accountInternals.FirstOrDefault(a => a.AccountId == row.Dim4Id)) != null)
                existingRow.AccountInternal.Add(accountInternal);
            if (row.Dim5Id != 0 && (accountInternal = accountInternals.FirstOrDefault(a => a.AccountId == row.Dim5Id)) != null)
                existingRow.AccountInternal.Add(accountInternal);
            if (row.Dim6Id != 0 && (accountInternal = accountInternals.FirstOrDefault(a => a.AccountId == row.Dim6Id)) != null)
                existingRow.AccountInternal.Add(accountInternal);


            SetModifiedProperties(existingRow);

            var periodsToSave = GetPeriodsToSave(row.Periods, "");
            foreach (var period in periodsToSave)
            {
                AddSalesBudgetPeriod(entities, existingRow, period, existingRow.BudgetRowPeriod.ToList(), distributionCode, head.FromDate.HasValue ? head.FromDate.Value.Year : DateTime.Today.Year);
            }
        }

        private void AddSalesBudgetPeriod(CompEntities entities, BudgetRow row, BudgetPeriodSalesDTO period, List<BudgetRowPeriod> existingPeriods, DistributionCodeHead distributionCode, int year)
        {
            BudgetRowPeriod existingPeriod = period.BudgetRowPeriodId > 0 ? existingPeriods.FirstOrDefault(p => p.BudgetRowPeriodId == period.BudgetRowPeriodId) : null;
            if (existingPeriod == null)
            {
                existingPeriod = new BudgetRowPeriod()
                {
                    BudgetRow = row,
                    Type = (int)BudgetRowPeriodType.Day,
                    PeriodNr = period.PeriodNr,
                    StartTime = GetDateFromPeriodNumber(period.PeriodNr, year, distributionCode)
                };
                SetCreatedProperties(existingPeriod);
                entities.BudgetRowPeriod.AddObject(existingPeriod);
            }
            else
            {
                SetModifiedProperties(existingPeriod);
            }

            existingPeriod.Amount = period.Amount;
            existingPeriod.Quantity = period.Amount > 0 ? 1 : period.Quantity;

            // Check distribution code
            if (period.DistributionCodeHeadId > 0)
                existingPeriod.DistributionCodeHeadId = period.DistributionCodeHeadId;
        }

        private List<BudgetPeriodSalesDTO> GetPeriodsToSave(List<BudgetPeriodSalesDTO> periods, string identificationKey)
        {
            int periodNr = 100;
            List<BudgetPeriodSalesDTO> periodsToSave = new List<BudgetPeriodSalesDTO>();
            foreach (var period in periods)
            {
                var identification = identificationKey + periodNr.ToString();
                if (!period.Periods.IsNullOrEmpty())
                {
                    periodsToSave.AddRange(GetPeriodsToSave(period.Periods, identification));
                }
                else
                {
                    // Only save existing periods or periods with amount
                    if (period.BudgetRowPeriodId > 0 || period.Amount > 0 || period.Quantity > 0)
                    {
                        period.PeriodNr = Convert.ToInt32(identification);
                        periodsToSave.Add(period);
                    }
                }
                periodNr++;
            }
            return periodsToSave;
        }

        private DateTime GetDateFromPeriodNumber(int periodNr, int year, DistributionCodeHead distributionCode)
        {
            if (periodNr.ToString().Length == 6)
            {
                bool isYearWeek = (distributionCode.SubType == (int)TermGroup_AccountingBudgetSubType.YearWeek);

                int month = 1;
                Int32.TryParse(periodNr.ToString().Left(3), out month);

                int week = 1;
                if (isYearWeek)
                    week = month - 99;

                if (month >= 100)
                    month = isYearWeek ? GetMonthFromWeek(year, week) : month - 99;

                int day = 1;
                Int32.TryParse(periodNr.ToString().Right(3), out day);
                if (day >= 100)
                    day -= 99;

                if (isYearWeek)
                {
                    DateTime date = CalendarUtility.GetFirstDateOfWeek(year, week);
                    return date.AddDays(day);
                }

                return new DateTime(year, month, day);
            }

            return DateTime.Today;
        }

        private int GetMonthFromWeek(int year, int week)
        {
            DateTime tDt = new DateTime(year, 1, 1);

            tDt.AddDays((week - 1) * 7);

            for (int i = 0; i <= 365; ++i)
            {
                int tWeek = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(
                    tDt,
                    CalendarWeekRule.FirstDay,
                    DayOfWeek.Monday);
                if (tWeek == week)
                    return tDt.Month;

                tDt = tDt.AddDays(1);
            }
            return 0;
        }

        public ActionResult DeleteBudgetHead(int budgetHeadId)
        {
            ActionResult result = new ActionResult(true);

            try
            {
                using (CompEntities entities = new CompEntities())
                {
                    if (entities.Connection.State != ConnectionState.Open)
                        entities.Connection.Open();

                    return DeleteBudgetHead(entities, budgetHeadId);
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

        public ActionResult DeleteBudgetHead(CompEntities entities, int budgetHeadId)
        {
            ActionResult result = new ActionResult(true);
                

            #region Prereq

            BudgetHead head = null;
            if (budgetHeadId != 0)
                head = GetBudgetHeadIncludingRows(entities, budgetHeadId);

            #endregion

            #region Perform

            if (head == null)
                return new ActionResult(false, (int)ActionResultSave.EntityIsNull, GetText(7044, "Felaktig inparameter"));

            //Update
            result = ChangeEntityState(head, SoeEntityState.Deleted);

            if (!result.Success)
                return result;

            if (!head.BudgetRow.IsLoaded)
                head.BudgetRow.Load();

            foreach (BudgetRow row in head.BudgetRow.ToList())
            {
                //Delete
                if (!row.BudgetRowPeriod.IsLoaded)
                    row.BudgetRowPeriod.Load();

                foreach (BudgetRowPeriod period in row.BudgetRowPeriod.ToList())
                {
                    DeleteEntityItem(entities, period);
                }

                if (!row.AccountStdReference.IsLoaded)
                    row.AccountStdReference.Load();

                row.AccountInternal.Clear();
                ChangeEntityState(row, SoeEntityState.Deleted);
            }

            return SaveChanges(entities);

            #endregion

        }

        public void GetBalanceChangePerPeriod(int noOfPeriods, int accountYearId, int accountId, int actorCompanyId, DateTime today, bool getPrevious, List<int> dims, ref SoeProgressInfo info, SoeMonitor monitor)
        {
            bool includeInternals = dims.Count > 0;

            balanceManager = new AccountBalanceManager(null, actorCompanyId);
            usedAccounts = new Dictionary<int, bool>();
            usedInternalAccounts = new List<int>();

            using (CompEntities entities = new CompEntities())
            {
                #region prereq

                AccountYear accountYear = AccountManager.GetAccountYear(entities, accountYearId, true);
                if (accountYear == null)
                    info.Abort = true;

                AccountYear prevAccountYear = accountYear != null ? AccountManager.GetPreviousAccountYear(entities, accountYear.From, actorCompanyId, true) : null;
                if (prevAccountYear == null)
                    info.Abort = true;

                #endregion

                if (accountId == 0)
                {
                    //GetPrevious
                    if (getPrevious && prevAccountYear != null)
                    {
                        balanceManager.GetAccountsAndInternalsForBalance(entities, prevAccountYear, actorCompanyId, includeInternals, ref usedAccounts, ref usedInternalAccounts);
                        GetBalanceChangePerPeriod(entities, prevAccountYear, noOfPeriods, accountId, actorCompanyId, today, true, dims, ref info);
                    }
                    else
                    {
                        //GetCurrent
                        if (accountYear != null && today.Year == accountYear.From.Year)
                        {
                            if (today.Month > accountYear.From.Month)
                            {
                                balanceManager.GetAccountsAndInternalsForBalance(entities, accountYear, actorCompanyId, includeInternals, ref usedAccounts, ref usedInternalAccounts);
                                GetBalanceChangePerPeriod(entities, accountYear, noOfPeriods, accountId, actorCompanyId, today, false, dims, ref info);
                            }
                        }
                        else if (accountYear != null && today.Year > accountYear.From.Year)
                        {
                            balanceManager.GetAccountsAndInternalsForBalance(entities, accountYear, actorCompanyId, includeInternals, ref usedAccounts, ref usedInternalAccounts);
                            GetBalanceChangePerPeriod(entities, accountYear, noOfPeriods, accountId, actorCompanyId, today, false, dims, ref info);
                        }
                    }
                }
                else
                {
                    GetBalanceChangePerPeriod(entities, getPrevious ? prevAccountYear : accountYear, noOfPeriods, accountId, actorCompanyId, today, getPrevious, dims, ref info);
                }

                info.Abort = true;
                monitor.AddResult(info.PollingKey, balanceItemList);
            }
        }

        public void GetBalanceChangePerPeriod(CompEntities entities, AccountYear accountYear, int noOfPeriods, int accountId, int actorCompanyId, DateTime today, bool getPrevious, List<int> dims, ref SoeProgressInfo info)
        {
            List<AccountStd> accountStds = new List<AccountStd>();

            if (accountId != 0)
            {
                AccountStd accountStd = AccountManager.GetAccountStd(entities, accountId, actorCompanyId, true, false);
                if (accountStd == null)
                    return;

                accountStds.Add(accountStd);
                usedAccounts.Add(accountId, true);
            }
            else
            {
                accountStds = AccountManager.GetAccountStdsByCompany(actorCompanyId, true, false, true);
            }

            #region AccountInternals

            List<AccountDim> accountDims = AccountManager.GetAccountDimsByCompany(actorCompanyId);

            AccountDim dim2 = accountDims.FirstOrDefault(a => a.AccountDimNr == 2);
            if (dim2 != null && dims.Contains(2))
            {
                dim2Accounts = AccountManager.GetAccountInternalsByDim(dim2.AccountDimId, actorCompanyId);
                dim2Accounts = dim2Accounts.Where(a => usedInternalAccounts.Contains(a.AccountId)).ToList();
            }

            AccountDim dim3 = accountDims.FirstOrDefault(a => a.AccountDimNr == 3);
            if (dim3 != null && dims.Contains(3))
            {
                dim3Accounts = AccountManager.GetAccountInternalsByDim(dim3.AccountDimId, actorCompanyId);
                dim3Accounts = dim3Accounts.Where(a => usedInternalAccounts.Contains(a.AccountId)).ToList();
            }

            AccountDim dim4 = accountDims.FirstOrDefault(a => a.AccountDimNr == 4);
            if (dim4 != null && dims.Contains(4))
            {
                dim4Accounts = AccountManager.GetAccountInternalsByDim(dim4.AccountDimId, actorCompanyId);
                dim4Accounts = dim4Accounts.Where(a => usedInternalAccounts.Contains(a.AccountId)).ToList();
            }

            AccountDim dim5 = accountDims.FirstOrDefault(a => a.AccountDimNr == 5);
            if (dim5 != null && dims.Contains(5))
            {
                dim5Accounts = AccountManager.GetAccountInternalsByDim(dim5.AccountDimId, actorCompanyId);
                dim5Accounts = dim5Accounts.Where(a => usedInternalAccounts.Contains(a.AccountId)).ToList();
            }

            AccountDim dim6 = accountDims.FirstOrDefault(a => a.AccountDimNr == 6);
            if (dim6 != null && dims.Contains(6))
            {
                dim6Accounts = AccountManager.GetAccountInternalsByDim(dim6.AccountDimId, actorCompanyId);
                dim6Accounts = dim6Accounts.Where(a => usedInternalAccounts.Contains(a.AccountId)).ToList();
            }

            #endregion

            int dim2calls = 0;

            foreach (AccountStd accountStd in accountStds.Where(a => usedAccounts.Keys.Contains(a.AccountId) && (a.AccountTypeSysTermId == (int)TermGroup_AccountType.Cost || a.AccountTypeSysTermId == (int)TermGroup_AccountType.Income)).OrderBy(a => a.Account.AccountNr))
            {
                info.Message = accountStd.Account.AccountNr + " " + accountStd.Account.Name;

                List<AccountInternal> internals = new List<AccountInternal>();

                BudgetBalanceDTO budgetDto = new BudgetBalanceDTO()
                {
                    AccountId = accountStd.AccountId,
                    BalancePeriods = new List<BalanceItemDTO>(),
                };

                if (GetBalanceChangePerPeriodForAccountStd(entities, actorCompanyId, noOfPeriods, getPrevious, budgetDto, today, accountYear, accountStd, internals, dims.Count > 0))
                {
                    balanceItemList.Add(budgetDto);
                }

                if (dims.Count > 0)
                {
                    GetBalanceChangeDimSwitch(entities, actorCompanyId, noOfPeriods, 0, getPrevious, today, accountYear, accountStd, internals, dims);
                    dim2calls++;
                }
            }
        }

        private void GetBalanceChangeDimSwitch(CompEntities entities, int actorCompanyId, int noOfPeriods, int dimCount, bool getPrevious, DateTime today, AccountYear accountYear, AccountStd accountStd, List<AccountInternal> internals, List<int> dims)
        {
            if (dimCount < dims.Count)
            {
                int dimNr = dims[dimCount];

                dimCount = dimCount + 1;

                switch (dimNr)
                {
                    case 2:
                        GetBalanceChangeDim(entities, actorCompanyId, noOfPeriods, dimNr, dimCount, getPrevious, today, accountYear, accountStd, dim2Accounts, internals, dims);
                        break;
                    case 3:
                        GetBalanceChangeDim(entities, actorCompanyId, noOfPeriods, dimNr, dimCount, getPrevious, today, accountYear, accountStd, dim3Accounts, internals, dims);
                        break;
                    case 4:
                        GetBalanceChangeDim(entities, actorCompanyId, noOfPeriods, dimNr, dimCount, getPrevious, today, accountYear, accountStd, dim4Accounts, internals, dims);
                        break;
                    case 5:
                        GetBalanceChangeDim(entities, actorCompanyId, noOfPeriods, dimNr, dimCount, getPrevious, today, accountYear, accountStd, dim5Accounts, internals, dims);
                        break;
                    case 6:
                        GetBalanceChangeDim(entities, actorCompanyId, noOfPeriods, dimNr, dimCount, getPrevious, today, accountYear, accountStd, dim6Accounts, internals, dims);
                        break;
                }
            }
        }

        private void GetBalanceChangeDim(CompEntities entities, int actorCompanyId, int noOfPeriods, int dimNr, int dimCount, bool getPrevious, DateTime today, AccountYear accountYear, AccountStd accountStd, List<AccountInternal> dimAccounts, List<AccountInternal> internals, List<int> dims)
        {
            //Hämta utan nuvarande dim
            GetBalanceChangeDimSwitch(entities, actorCompanyId, noOfPeriods, dimCount, getPrevious, today, accountYear, accountStd, internals, dims);

            if (dimAccounts != null)
            {
                foreach (AccountInternal acc in dimAccounts)
                {
                    BudgetBalanceDTO dto = new BudgetBalanceDTO()
                    {
                        AccountId = accountStd.AccountId,
                        BalancePeriods = new List<BalanceItemDTO>(),
                        Dim2Id = dimNr == 2 ? acc.AccountId : (internals.FirstOrDefault(a => a.Account.AccountDim?.AccountDimNr == 2)?.AccountId ?? 0),
                        Dim3Id = dimNr == 3 ? acc.AccountId : (internals.FirstOrDefault(a => a.Account.AccountDim?.AccountDimNr == 3)?.AccountId ?? 0),
                        Dim4Id = dimNr == 4 ? acc.AccountId : (internals.FirstOrDefault(a => a.Account.AccountDim?.AccountDimNr == 4)?.AccountId ?? 0),
                        Dim5Id = dimNr == 5 ? acc.AccountId : (internals.FirstOrDefault(a => a.Account.AccountDim?.AccountDimNr == 5)?.AccountId ?? 0),
                        Dim6Id = dimNr == 6 ? acc.AccountId : (internals.FirstOrDefault(a => a.Account.AccountDim?.AccountDimNr == 6)?.AccountId ?? 0),
                    };

                    internals.Add(acc);

                    if (GetBalanceChangePerPeriodForAccountStd(entities, actorCompanyId, noOfPeriods, getPrevious, dto, today, accountYear, accountStd, internals, true))
                    {
                        balanceItemList.Add(dto);
                    }

                    GetBalanceChangeDimSwitch(entities, actorCompanyId, noOfPeriods, dimCount, getPrevious, today, accountYear, accountStd, internals, dims);
                    internals.Remove(acc);
                }
            }
        }

        private bool GetBalanceChangePerPeriodForAccountStd(CompEntities entities, int actorCompanyId, int noOfPeriods, bool getPrevious, BudgetBalanceDTO budgetDto, DateTime today, AccountYear accountYear, AccountStd accountStd, List<AccountInternal> internals, bool checkInternals)
        {
            decimal sum = 0;

            if (getPrevious)
            {
                for (int i = 0; i < noOfPeriods && i < accountYear.AccountPeriod.Count; i++)
                {
                    var period = accountYear.AccountPeriod.ElementAt(i);
                    if (period != null)
                    {
                        BalanceItemDTO dto = balanceManager.GetBalanceChangeMatchInternals(entities, accountYear, period.From, period.To, accountStd, internals, actorCompanyId, checkInternals, includeVatVoucher: true);
                        if (dto != null)
                        {
                            dto.Flag = true;
                            sum += dto.Balance;
                            budgetDto.BalancePeriods.Add(dto);
                        }
                    }
                }
            }
            else
            {
                if (today.Month > accountYear.From.Month || accountYear.From.Year < today.Year)
                {
                    //Perioder är passerade, fyll på med dessa.
                    foreach (AccountPeriod period in accountYear.AccountPeriod.Where(p => (p.From.Month < today.Month || p.From.Year < today.Year)))
                    {
                        BalanceItemDTO dto = balanceManager.GetBalanceChangeMatchInternals(entities, accountYear, period.From, period.To, accountStd, internals, actorCompanyId, checkInternals, includeVatVoucher: true);
                        if (dto != null)
                        {
                            dto.Flag = false;
                            sum += dto.Balance;
                            budgetDto.BalancePeriods.Add(dto);
                        }
                    }
                }
            }

            if (sum != 0)
            {
                budgetDto.RowSum = sum;
                return true;
            }
            else
            {
                return false;
            }
        }

        public List<BudgetRowFlattenedDTO> ConvertResultToBudgetRow(IEnumerable<BudgetBalanceDTO> balanceItems)
        {
            List<BudgetRowFlattenedDTO> budgetRows = new List<BudgetRowFlattenedDTO>();

            foreach (BudgetBalanceDTO dto in balanceItems)
            {
                BudgetRowFlattenedDTO rowDto = new BudgetRowFlattenedDTO()
                {
                    AccountId = dto.AccountId,
                    TotalAmount = dto.RowSum,
                    Dim2Id = dto.Dim2Id,
                    Dim3Id = dto.Dim3Id,
                    Dim4Id = dto.Dim4Id,
                    Dim5Id = dto.Dim5Id,
                    Dim6Id = dto.Dim6Id,
                };

                int periodNr = 1;
                Type t = rowDto.GetType();
                foreach (BalanceItemDTO bDto in dto.BalancePeriods)
                {
                    PropertyInfo periodNrProperty = t.GetProperty("PeriodNr" + periodNr);
                    if (periodNrProperty != null)
                        periodNrProperty.SetValue(rowDto, periodNr);

                    PropertyInfo amountProperty = t.GetProperty("Amount" + periodNr);
                    if (amountProperty != null)
                        amountProperty.SetValue(rowDto, bDto.Balance);

                    periodNr++;
                }

                budgetRows.Add(rowDto);
            }

            return budgetRows;
        }

        #endregion

        #region DistributionCodes        

        public ActionResult SaveDistributionCode(int actorCompanyId, DistributionCodeHeadDTO dto)
        {
            ActionResult result = new ActionResult(true);
            DistributionCodeHead head = null;

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        if (entities.Connection.State != ConnectionState.Open)
                            entities.Connection.Open();

                        #region Prereq

                        if (dto == null)
                            return new ActionResult(false, (int)ActionResultSave.EntityIsNull, GetText(7044, "Felaktig inparameter"));

                        if (dto.DistributionCodeHeadId != 0)
                            head = GetDistributionCode(entities, actorCompanyId, dto.DistributionCodeHeadId);

                        #endregion

                        #region Perform

                        if (head != null)
                        {
                            //Update
                            head.Type = dto.TypeId;
                            head.Name = dto.Name;
                            head.NoOfPeriods = dto.NoOfPeriods;
                            head.SubType = dto.SubType;
                            head.OpeningHoursId = dto.OpeningHoursId.HasValue && dto.OpeningHoursId.Value > 0 ? dto.OpeningHoursId.Value : (int?)null;
                            head.AccountDimId = dto.AccountDimId.HasValue && dto.AccountDimId.Value > 0 ? dto.AccountDimId.Value : (int?)null;
                            head.ParentId = dto.ParentId;
                            head.FromDate = dto.FromDate;
                            SetModifiedProperties(head);

                            result = SaveChanges(entities);

                            if (!result.Success)
                                return result;

                            //Periods
                            List<int> handledPeriodIds = new List<int>();

                            foreach (DistributionCodePeriodDTO pDto in dto.Periods)
                            {
                                if (pDto.DistributionCodePeriodId != 0)
                                {
                                    //Update
                                    DistributionCodePeriod period = head.DistributionCodePeriod.FirstOrDefault(p => p.DistributionCodePeriodId == pDto.DistributionCodePeriodId);

                                    if (period != null)
                                    {
                                        handledPeriodIds.Add(pDto.DistributionCodePeriodId);

                                        period.ParentToDistributionCodeHeadId = pDto.ParentToDistributionCodePeriodId;
                                        period.Percent = pDto.Percent;
                                        period.Comment = pDto.Comment;

                                        SetModifiedProperties(period);
                                    }
                                    else
                                    {
                                        //New?
                                        DistributionCodePeriod newPeriod = new DistributionCodePeriod()
                                        {
                                            DistributionCodeHeadId = head.DistributionCodeHeadId,
                                            ParentToDistributionCodeHeadId = pDto.ParentToDistributionCodePeriodId,
                                            Percent = pDto.Percent,
                                            Comment = pDto.Comment,
                                        };

                                        SetCreatedProperties(newPeriod);
                                        entities.DistributionCodePeriod.AddObject(newPeriod);
                                    }
                                }
                                else
                                {
                                    //New
                                    DistributionCodePeriod period = new DistributionCodePeriod()
                                    {
                                        DistributionCodeHeadId = head.DistributionCodeHeadId,
                                        ParentToDistributionCodeHeadId = pDto.ParentToDistributionCodePeriodId,
                                        Percent = pDto.Percent,
                                        Comment = pDto.Comment,
                                    };

                                    SetCreatedProperties(period);
                                    entities.DistributionCodePeriod.AddObject(period);

                                    handledPeriodIds.Add(period.DistributionCodePeriodId);
                                }
                            }

                            head.DistributionCodePeriod.Where(p => !handledPeriodIds.Exists(i => i == p.DistributionCodePeriodId)).ToList().ForEach(p => entities.DeleteObject(p));
                        }
                        else
                        {
                            //New
                            head = new DistributionCodeHead()
                            {
                                ActorCompanyId = actorCompanyId,
                                Type = dto.TypeId,
                                Name = dto.Name,
                                NoOfPeriods = dto.NoOfPeriods,
                                SubType = dto.SubType,
                                OpeningHoursId = dto.OpeningHoursId.HasValue && dto.OpeningHoursId.Value > 0 ? dto.OpeningHoursId.Value : (int?)null,
                                AccountDimId = dto.AccountDimId.HasValue && dto.AccountDimId.Value > 0 ? dto.AccountDimId.Value : (int?)null,
                                ParentId = dto.ParentId,
                                FromDate = dto.FromDate
                            };

                            SetCreatedProperties(head);
                            entities.DistributionCodeHead.AddObject(head);

                            foreach (DistributionCodePeriodDTO pDto in dto.Periods)
                            {
                                //New
                                DistributionCodePeriod period = new DistributionCodePeriod()
                                {
                                    DistributionCodeHeadId = head.DistributionCodeHeadId,
                                    ParentToDistributionCodeHeadId = pDto.ParentToDistributionCodePeriodId,
                                    Percent = pDto.Percent,
                                    Comment = pDto.Comment,
                                };

                                SetCreatedProperties(period);
                                entities.DistributionCodePeriod.AddObject(period);
                            }
                        }

                        result = SaveChanges(entities, transaction);

                        #endregion

                        //Commit transaction
                        if (result.Success)
                            transaction.Complete();
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
                    else if (head != null)
                        result.IntegerValue = head.DistributionCodeHeadId;

                    entities.Connection.Close();
                }
            }

            return result;
        }

        public ActionResult DeleteDistributionCode(int actorCompanyId, int distributionCodeHeadId)
        {
            ActionResult result = new ActionResult(true);

            try
            {
                using (CompEntities entities = new CompEntities())
                {
                    DistributionCodeHead head = null;

                    if (entities.Connection.State != ConnectionState.Open)
                        entities.Connection.Open();

                    #region Prereq

                    if (distributionCodeHeadId == 0)
                        return new ActionResult(false, (int)ActionResultSave.EntityIsNull, GetText(7044, "Felaktig inparameter"));

                    head = GetDistributionCode(entities, actorCompanyId, distributionCodeHeadId);

                    #endregion

                    #region Perform

                    if (head != null)
                    {
                        // Moved to client //
                        /*if (!head.BudgetHead.IsLoaded)
                            head.BudgetHead.Load();

                        if (!head.DistributionCodePeriod1.IsLoaded)
                            head.DistributionCodePeriod1.Load();

                        if(head.BudgetHead.Where(h => h.State == (int)SoeEntityState.Active).Any() || head.DistributionCodePeriod1.Any())
                            return new ActionResult(false, (int)ActionResultSave.DistributionCodeInUse, GetText(10110, "Fördelningskoden används och kan därför ej tas bort"));*/

                        result = ChangeEntityState(entities, head, SoeEntityState.Deleted, true);
                    }
                    else
                    {
                        return new ActionResult(false, (int)ActionResultSave.EntityNotFound, GetText(7044, "DistributionCodeHead"));
                    }

                    #endregion
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

        public DistributionCodeHead GetDistributionCode(int actorCompanyId, int distributionCodeHeadId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.DistributionCodeHead.NoTracking();
            return GetDistributionCode(entities, actorCompanyId, distributionCodeHeadId);
        }

        public DistributionCodeHead GetDistributionCode(CompEntities entities, int actorCompanyId, int distributionCodeHeadId)
        {
            return (from dc in entities.DistributionCodeHead.Include("DistributionCodePeriod")
                    where dc.DistributionCodeHeadId == distributionCodeHeadId
                    select dc).FirstOrDefault();
        }

        public List<DistributionCodeGridDTO> GetDistributionCodesForGrid(int actorCompanyId, int? distributionId = null)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.DistributionCodeHead.NoTracking();
            return GetDistributionCodesForGrid(entities, actorCompanyId, distributionId);
        }

        public DistributionCodeHead GetDistributionCodeByParentAndSubType(CompEntities entities, int actorCompanyId, int parentDistributionCodeHeadId, int subType, List<DistributionCodeHead> distributionCodeHeads = null)
        {
            if (distributionCodeHeads.IsNullOrEmpty())
            {
                return (from dc in entities.DistributionCodeHead.Include("DistributionCodePeriod")
                        where dc.ParentId == parentDistributionCodeHeadId && dc.SubType == subType
                        select dc).FirstOrDefault();
            }
            else
            {
                return (from dc in distributionCodeHeads
                        where dc.ParentId == parentDistributionCodeHeadId && dc.SubType == subType
                        select dc).FirstOrDefault();
            }
        }

        public List<DistributionCodeHead> GetDistributionCodes(int actorCompanyId, bool includePeriods, bool onlyActive = true, DistributionCodeBudgetType? budgetType = null, DateTime? fromDate = null, DateTime? toDate = null)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.DistributionCodeHead.NoTracking();
            return GetDistributionCodes(entities, actorCompanyId, includePeriods, onlyActive, budgetType, fromDate, toDate);
        }

        public List<DistributionCodeHead> GetDistributionCodes(CompEntities entities, int actorCompanyId, bool includePeriods, bool onlyActive = true, DistributionCodeBudgetType? budgetType = null, DateTime? fromDate = null, DateTime? toDate = null)
        {
            IQueryable<DistributionCodeHead> query = (from dc in entities.DistributionCodeHead
                                                      where dc.ActorCompanyId == actorCompanyId
                                                      select dc);
            if (includePeriods)
                query = query.Include("DistributionCodePeriod");

            if (onlyActive)
                query = query.Where(dc => dc.State == (int)SoeEntityState.Active);
            else
                query = query.Where(dc => dc.State != (int)SoeEntityState.Deleted);

            if (budgetType.HasValue)
                query = query.Where(dc => dc.Type == (int)budgetType.Value);

            if (toDate.HasValue)
                query = query.Where(dc => !dc.FromDate.HasValue || dc.FromDate.Value <= toDate.Value);

            List<DistributionCodeHead> allHeads = query.ToList();

            if (fromDate.HasValue)
            {
                // If date interaval is specified, return only codes relevant for interval
                List<DistributionCodeHead> heads = new List<DistributionCodeHead>();

                var groupedHeads = allHeads.GroupBy(h => new { h.Type, h.SubType, h.ParentId, h.AccountDimId }).Select(gh => new { gh.Key.Type, gh.Key.SubType, gh.Key.ParentId, gh.Key.AccountDimId, Codes = gh.ToList() });
                foreach (var group in groupedHeads)
                {
                    if (group.Codes.Count == 1 || !group.Codes.Any(c => c.FromDate.HasValue))
                    {
                        // Only one, or all are without date
                        heads.AddRange(group.Codes);
                    }
                    else
                    {
                        // Add all that starts after from date (starts after to date is already removed in main query)
                        heads.AddRange(group.Codes.Where(c => c.FromDate.HasValue && c.FromDate.Value >= fromDate.Value));

                        // If none starts exactly on from date, we need to add the one starting before from date also
                        bool startsOnFromDate = group.Codes.Any(c => c.FromDate.HasValue && c.FromDate.Value == fromDate.Value);
                        if (!startsOnFromDate)
                        {
                            DistributionCodeHead head = group.Codes.Where(c => c.FromDate.HasValue && c.FromDate.Value < fromDate.Value).OrderByDescending(c => c.FromDate).FirstOrDefault();
                            if (head != null)
                                heads.Add(head);
                            else
                                heads.AddRange(group.Codes.Where(c => !c.FromDate.HasValue));
                        }
                    }
                }

                return heads.OrderBy(h => h.Name).ThenBy(h => h.FromDate).ToList();
            }
            else
            {
                return allHeads.OrderBy(h => h.Name).ThenBy(h => h.FromDate).ToList();
            }
        }

        public List<DistributionCodeGridDTO> GetDistributionCodesForGrid(CompEntities entities, int actorCompanyId, int? distributionId = null)
        {
            var dtos = new List<DistributionCodeGridDTO>();

            // Get codes
            var codes = (from dc in entities.DistributionCodeHead
                         where dc.ActorCompanyId == actorCompanyId &&
                         dc.State == (int)SoeEntityState.Active
                         select dc);

            if (codes.IsNullOrEmpty())
                return dtos;

            if (distributionId.HasValue)
            {
                codes = codes.Where(x => x.DistributionCodeHeadId == distributionId);
            }

            var openingHours = CalendarManager.GetOpeningHoursForCompany(actorCompanyId);
            var accountDims = AccountManager.GetAccountDimsByCompany(false, true);
            var types = base.GetTermGroupContent(TermGroup.AccountingBudgetType);
            var subTypes = base.GetTermGroupContent(TermGroup.AccountingBudgetSubType);
            DistributionCodeHead distributionCodeParent = null;

            foreach (var code in codes.ToList())
            {
                var dto = new DistributionCodeGridDTO()
                {
                    DistributionCodeHeadId = code.DistributionCodeHeadId,
                    Name = code.Name,
                    NoOfPeriods = code.NoOfPeriods,
                    FromDate = code.FromDate
                };

                var type = types.FirstOrDefault(t => t.Id == code.Type);
                if (type != null)
                {
                    dto.TypeId = type.Id;
                    dto.Type = type.Name;
                }

                if (dto.TypeId != (int)TermGroup_AccountingBudgetType.AccountingBudget)
                {
                    if (code.SubType.HasValue)
                    {
                        var subType = subTypes.FirstOrDefault(t => t.Id == code.SubType.Value);
                        if (subType != null)
                        {
                            dto.TypeOfPeriodId = subType.Id;
                            dto.TypeOfPeriod = subType.Name;
                        }
                    }
                    else
                    {
                        dto.TypeOfPeriodId = 0;
                        dto.TypeOfPeriod = GetText(1098, (int)TermGroup.AngularCommon, "Ingen");
                    }

                    if (code.ParentId.HasValue)
                    {
                        var parent = codes.ToList().FirstOrDefault(c => c.DistributionCodeHeadId == code.ParentId.Value);
                        if (parent != null)
                        {
                            dto.SubLevel = parent.Name;
                        }
                        else
                        {
                            if (distributionId.HasValue)
                            {
                                distributionCodeParent = GetDistributionCode(actorCompanyId, code.ParentId.Value);
                                if(distributionCodeParent != null)
                                    dto.SubLevel = distributionCodeParent.Name;
                            }
                        }
                    }

                    if (code.OpeningHoursId.HasValue)
                    {
                        var parent = openingHours.FirstOrDefault(o => o.OpeningHoursId == code.OpeningHoursId.Value);
                        if (parent != null)
                        {
                            dto.OpeningHour = parent.Name;
                        }
                    }
                }
                else
                {
                    dto.TypeOfPeriodId = 0;
                    dto.TypeOfPeriod = GetText(1098, (int)TermGroup.AngularCommon, "Ingen");
                }

                if (code.AccountDimId.HasValue)
                {
                    var accountDim = accountDims.FirstOrDefault(o => o.AccountDimId == code.AccountDimId.Value);
                    if (accountDim != null)
                    {
                        dto.AccountDim = accountDim.Name;
                    }
                }

                dtos.Add(dto);
            }

            return dtos.OrderBy(d => d.Name).ToList();
        }

        public Dictionary<int, string> GetDistributionCodeDict(int actorCompanyId, bool addEmptyRow, bool onlyActive = true)
        {
            Dictionary<int, string> dict = new Dictionary<int, string>();

            if (addEmptyRow)
                dict.Add(0, " ");

            List<DistributionCodeHead> distributioncodes = GetDistributionCodes(actorCompanyId, false, onlyActive);
            foreach (DistributionCodeHead distributioncode in distributioncodes)
            {
                if (!dict.ContainsKey(distributioncode.DistributionCodeHeadId))
                    dict.Add(distributioncode.DistributionCodeHeadId, distributioncode.Name);
            }

            return dict;
        }

        public IEnumerable<DistributionCodeHead> GetDistributionCodesByType(int actorCompanyId, int type, bool includePeriods)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.DistributionCodeHead.NoTracking();
            return GetDistributionCodesByType(entities, actorCompanyId, type, includePeriods);
        }

        public IEnumerable<DistributionCodeHead> GetDistributionCodesByType(CompEntities entities, int actorCompanyId, int type, bool includePeriods)
        {
            if (includePeriods)
            {
                return (from dc in entities.DistributionCodeHead
                        .Include("DistributionCodePeriod")
                        .Include("OpeningHours")
                        where dc.ActorCompanyId == actorCompanyId &&
                        dc.Type == type &&
                        dc.State == (int)SoeEntityState.Active
                        select dc);
            }
            else
            {
                return (from dc in entities.DistributionCodeHead
                        .Include("OpeningHours")
                        where dc.ActorCompanyId == actorCompanyId &&
                        dc.Type == type &&
                        dc.State == (int)SoeEntityState.Active
                        select dc);
            }
        }

        #endregion

        #region Help methods

        private void InitBalanceItemDict(List<AccountDTO> accountStds, ref Dictionary<int, BalanceItemDTO> dict)
        {
            if (dict == null)
                dict = new Dictionary<int, BalanceItemDTO>();

            if (accountStds != null)
            {
                foreach (AccountDTO accountStd in accountStds)
                {
                    dict[accountStd.AccountId] = new BalanceItemDTO()
                    {
                        AccountId = accountStd.AccountId,
                    };
                }
            }
        }

        private void InitDimInternalDict(List<AccountInternalDTO> accountInternals, ref Dictionary<int, Dictionary<int, bool>> dict)
        {
            if (dict == null)
                dict = new Dictionary<int, Dictionary<int, bool>>();

            if (accountInternals != null)
            {
                foreach (AccountInternalDTO account in accountInternals)
                {
                    if (!dict.ContainsKey(account.AccountDimId))
                    {
                        dict[account.AccountDimId] = new Dictionary<int, bool>();
                    }
                    if (!dict[account.AccountDimId].ContainsKey(account.AccountId))
                    {
                        dict[account.AccountDimId][account.AccountId] = true;
                    }

                }
            }
        }

        private void UpdateBalanceItemsFromVoucherHeadDTO(List<VoucherHeadDTO> voucherHeads, Dictionary<int, BalanceItemDTO> dict, List<AccountInternalDTO> accountInternals, bool ignoreValidation = false)
        {
            if (voucherHeads == null)
                return;

            foreach (VoucherHeadDTO voucherHead in voucherHeads)
            {
                voucherHead.AccountIds = voucherHead.Rows.Where(i => i.Amount != 0).Select(i => i.Dim1Id).Distinct().ToList();

                foreach (VoucherRowDTO voucherRow in voucherHead.Rows)
                {
                    #region VoucherRow

                    // Skip inactive or deleted rows
                    if (voucherRow.State != (int)SoeEntityState.Active)
                        continue;
                    if (!dict.ContainsKey(voucherRow.Dim1Id))
                        continue;

                    bool valid = ignoreValidation || Validator.IsAccountInInterval(accountInternals, voucherRow.AccountInternalDTO_forReports != null ? voucherRow.AccountInternalDTO_forReports.ToList() : null, approveOneAccountInternal: true);
                    if (valid)
                        UpdateBalanceItemFromVoucherRowDTO(voucherRow, dict);

                    #endregion
                }
            }
        }

        private void UpdateBalanceItemFromVoucherRowDTO(VoucherRowDTO voucherRow, Dictionary<int, BalanceItemDTO> dict)
        {
            if (voucherRow == null)
                return;

            //Get from dict
            BalanceItemDTO balanceItem = dict[voucherRow.Dim1Id];
            if (balanceItem != null)
            {
                //Update AccountStd
                balanceItem.Balance += voucherRow.Amount;
                balanceItem.BalanceEntCurrency += voucherRow.AmountEntCurrency;
                if (voucherRow.Quantity.HasValue)
                    balanceItem.Quantity += voucherRow.Quantity.Value;

                if (voucherRow.AccountInternalDTO_forReports != null && voucherRow.AccountInternalDTO_forReports.Count > 0)
                {
                    BalanceItemInternalDTO balanceItemInternal = balanceItem.GetBalanceItemInternal(voucherRow.AccountInternalDTO_forReports.ToList());
                    if (balanceItemInternal != null)
                    {
                        //Update AccountInternal
                        balanceItemInternal.Balance += voucherRow.Amount;
                        balanceItemInternal.BalanceEntCurrency += voucherRow.AmountEntCurrency;
                        if (voucherRow.Quantity.HasValue)
                            balanceItemInternal.Quantity += voucherRow.Quantity.Value;
                    }
                }

                //Update dict
                dict.Remove(voucherRow.Dim1Id);
                dict.Add(voucherRow.Dim1Id, balanceItem);
            }
        }


        #endregion
    }
}

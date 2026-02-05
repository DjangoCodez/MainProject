using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Transactions;

namespace SoftOne.Soe.Business.Core
{
    public class TimePeriodManager : ManagerBase
    {
        #region Variables

        // Create a logger for use in this class
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #endregion

        #region Ctor

        public TimePeriodManager(ParameterObject parameterObject) : base(parameterObject) { }

        #endregion

        #region TimePeriodHead

        public List<TimePeriodHead> GetTimePeriodHeads(int actorCompanyId, TermGroup_TimePeriodType type, bool setTypeName, bool setAccountName, int? accountId = null, bool setChildName = false, int? timePeriodHeadId = null)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.TimePeriodHead.NoTracking();
            return GetTimePeriodHeads(entities, actorCompanyId, type, setTypeName, setAccountName, accountId, setChildName, timePeriodHeadId);
        }

        public List<TimePeriodHead> GetTimePeriodHeads(CompEntities entities, int actorCompanyId, TermGroup_TimePeriodType type, bool setTypeName, bool setAccountName, int? accountId = null, bool setChildName = false, int? timePeriodHeadId = null)
        {
            IQueryable<TimePeriodHead> query = entities.TimePeriodHead;
            if (setAccountName)
                query = query.Include("Account");
            if (setChildName)
                query = query.Include("TimePeriodHeadChild");

            List<TimePeriodHead> timePeriodHeads = (from tph in query
                                                    where tph.ActorCompanyId == actorCompanyId &&
                                                    (tph.TimePeriodType == (int)type || type == TermGroup_TimePeriodType.Unknown) &&
                                                    (!tph.AccountId.HasValue || !accountId.HasValue || tph.AccountId == accountId) &&
                                                    tph.State == (int)SoeEntityState.Active
                                                    select tph).ToList();

            if (setTypeName || setAccountName || setChildName)
            {
                foreach (TimePeriodHead timePeriodHead in timePeriodHeads)
                {
                    if (setTypeName)
                        timePeriodHead.TimePeriodTypeName = GetText(timePeriodHead.TimePeriodType, (int)TermGroup.TimePeriodHeadType);

                    if (setAccountName && timePeriodHead.Account != null)
                        timePeriodHead.AccountName = timePeriodHead.Account.Name;

                    if (setChildName && timePeriodHead.TimePeriodHeadChild != null)
                        timePeriodHead.ChildName = timePeriodHead.TimePeriodHeadChild.Name;
                }
            }

            if (timePeriodHeadId.HasValue)
                timePeriodHeads = timePeriodHeads.Where(s => s.TimePeriodHeadId == timePeriodHeadId.Value).ToList();

            if (setTypeName)
                return timePeriodHeads.OrderBy(t => t.TimePeriodTypeName).ThenBy(t => t.Name).ToList();
            else
                return timePeriodHeads.OrderBy(t => t.Name).ToList();
        }

        public List<TimePeriodHead> GetTimePeriodHeadsIncludingPeriods(int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.TimePeriodHead.NoTracking();
            return (from tph in entities.TimePeriodHead
                        .Include("TimePeriod")
                    where tph.ActorCompanyId == actorCompanyId &&
                    tph.State == (int)SoeEntityState.Active
                    select tph).ToList();
        }

        public List<TimePeriodHead> GetTimePeriodHeadsIncludingPeriodsForType(int actorCompanyId, TermGroup_TimePeriodType type)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.TimePeriodHead.NoTracking();
            return (from tph in entities.TimePeriodHead
                        .Include("TimePeriod")
                    where tph.ActorCompanyId == actorCompanyId &&
                    tph.TimePeriodType == (int)type &&
                    tph.State == (int)SoeEntityState.Active
                    select tph).ToList();
        }

        public List<TimePeriodHead> GetTimePeriodHeadsConnectedToDistributionRule(int actorCompanyId, TermGroup_TimePeriodType type, int distributionRuleHeadId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.TimePeriodHead.NoTracking();
            return (from tph in entities.TimePeriodHead
                    where tph.ActorCompanyId == actorCompanyId &&
                    tph.TimePeriodType == (int)type &&
                    tph.State == (int)SoeEntityState.Active &&
                    tph.PayrollProductDistributionRuleHeadId == distributionRuleHeadId
                    select tph).ToList();
        }

        public List<TimePeriodHead> GetTimePeriodHeadParents(int timePeriodHeadId, int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.TimePeriodHead.NoTracking();
            return (from tph in entities.TimePeriodHead
                    where tph.ChildId == timePeriodHeadId &&
                    tph.ActorCompanyId == actorCompanyId &&
                    tph.State == (int)SoeEntityState.Active
                    orderby tph.Name
                    select tph).ToList();
        }

        public List<EmploymentTaxTimePeriodHeadItemDTO> GetEmploymentTaxTimePeriodHeadDTOs(int actorCompanyId, int year, List<int> employeeIds)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.TimePeriod.NoTracking();
            return GetEmploymentTaxTimePeriodHeadDTOs(entities, actorCompanyId, year, employeeIds);
        }

        public List<EmploymentTaxTimePeriodHeadItemDTO> GetEmploymentTaxTimePeriodHeadDTOs(CompEntities entities, int actorCompanyId, int year, List<int> employeeIds)
        {
            List<EmploymentTaxTimePeriodHeadItemDTO> headDTOs = new List<EmploymentTaxTimePeriodHeadItemDTO>();

            var timePeriods = TimePeriodManager.GetTimePeriodsWithPaymentDatesThisYear(entities, actorCompanyId, year);
            var employeeTimePeriods = TimePeriodManager.GetEmployeeTimePeriodsWithValuesNotOpen(entities, timePeriods.Select(p => p.TimePeriodId).ToList(), employeeIds, actorCompanyId);

            List<PayrollStartValueHead> payrollStartValueHeads = PayrollManager.GetPayrollStartValueHeads(entities, actorCompanyId);
            List<PayrollStartValueRow> startValues = new List<PayrollStartValueRow>();
            if (!payrollStartValueHeads.IsNullOrEmpty() && payrollStartValueHeads.Any(x => x.DateTo.Year == year))
                startValues = PayrollManager.GetPayrollStartValueRows(entities, actorCompanyId, employeeIds, payrollStartValueHeads.Where(x => x.DateTo.Year == year).Select(x => x.PayrollStartValueHeadId).ToList());

            foreach (var employeeId in employeeIds)
            {
                List<EmploymentTaxTimePeriodRowItemDTO> employmentTaxTimePeriodItemDTOs = new List<EmploymentTaxTimePeriodRowItemDTO>();
                decimal employmentTaxBasisPreviousThisYear = 0;
                List<PayrollStartValueRow> startValuesEmployee = startValues.Where(s => s.EmployeeId == employeeId).ToList();

                decimal startValueEmploymentTaxBasis = 0;
                decimal startValueEmploymentTaxCredit = 0;

                foreach (var value in startValuesEmployee)
                {
                    if (value.IsEmploymentTaxBasis())
                        startValueEmploymentTaxBasis += value.Amount;

                    if (value.IsEmploymentTaxCredit())
                        startValueEmploymentTaxCredit += value.Amount;
                }

                List<EmployeeTimePeriod> orderedEmployeeTimePeriods = new List<EmployeeTimePeriod>();
                foreach (TimePeriod timePeriod in timePeriods.OrderBy(t => t.PaymentDate.Value))
                {
                    foreach (EmployeeTimePeriod employeeTimePeriod in employeeTimePeriods.Where(p => p.TimePeriodId == timePeriod.TimePeriodId && p.EmployeeId == employeeId))
                    {
                        orderedEmployeeTimePeriods.Add(employeeTimePeriod);
                    }
                }

                foreach (EmployeeTimePeriod employeeTimePeriod in orderedEmployeeTimePeriods)
                {
                    List<EmployeeTimePeriodValue> values = employeeTimePeriod.EmployeeTimePeriodValue.Where(p => p.Type == (int)SoeEmployeeTimePeriodValueType.GrossSalary || p.Type == (int)SoeEmployeeTimePeriodValueType.Benefit).ToList();
                    if (values != null)
                        employmentTaxBasisPreviousThisYear += values.Sum(e => e.Value);

                    EmploymentTaxTimePeriodRowItemDTO employmentTaxTimePeriodItemDTO = new EmploymentTaxTimePeriodRowItemDTO()
                    {
                        TimePeriodId = employeeTimePeriod.TimePeriodId,
                        EmploymentTaxBasis = values != null ? values.Sum(e => e.Value) : 0,
                        PaymentDate = timePeriods.FirstOrDefault(t => t.TimePeriodId == employeeTimePeriod.TimePeriodId).PaymentDate.Value,
                        EmployeeTimePeriodDTO = employeeTimePeriod.ToDTO()
                    };

                    employmentTaxTimePeriodItemDTOs.Add(employmentTaxTimePeriodItemDTO);
                }

                headDTOs.Add(new EmploymentTaxTimePeriodHeadItemDTO(employeeId, year, employmentTaxTimePeriodItemDTOs, startValueEmploymentTaxBasis, startValueEmploymentTaxCredit));
            }

            return headDTOs;
        }

        public Dictionary<int, string> GetTimePeriodHeadsDict(int actorCompanyId, TermGroup_TimePeriodType type, bool addEmptyRow, int? accountId = null)
        {
            var dict = new Dictionary<int, string>();
            if (addEmptyRow)
                dict.Add(0, " ");

            List<TimePeriodHead> timePeriodHeads = GetTimePeriodHeads(actorCompanyId, type, false, false, accountId);
            foreach (TimePeriodHead timePeriodHead in timePeriodHeads)
            {
                dict.Add(timePeriodHead.TimePeriodHeadId, timePeriodHead.Name);
            }

            return dict;
        }

        public TimePeriodHead GetDefaultTimePeriodHead(int actorCompanyId, bool loadPeriods)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.TimePeriodHead.NoTracking();
            return GetDefaultTimePeriodHead(entities, actorCompanyId, loadPeriods);
        }

        public TimePeriodHead GetDefaultTimePeriodHead(CompEntities entities, int actorCompanyId, bool loadPeriods)
        {
            TimePeriodHead timePeriodHead = null;
            int defaultTimePeriodHeadId = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.TimeDefaultTimePeriodHead, 0, actorCompanyId, 0);
            if (defaultTimePeriodHeadId > 0)
                timePeriodHead = GetTimePeriodHead(entities, defaultTimePeriodHeadId, actorCompanyId, loadPeriods);
            return timePeriodHead;
        }

        public TimePeriodHead GetDefaultTimePeriodHead(CompEntities entities, int? payrollGroupId, int actorCompanyId, bool loadPeriods)
        {
            TimePeriodHead timePeriodHead = null;

            //Prio 1: PayrollGroup
            PayrollGroup payrollGroup = payrollGroupId.HasValue && payrollGroupId.Value > 0 ? PayrollManager.GetPayrollGroup(entities, payrollGroupId.Value, onlyActive: true, includeTimePeriod: loadPeriods) : null;
            if (payrollGroup != null && payrollGroup.TimePeriodHead != null && payrollGroup.TimePeriodHead.State == (int)SoeEntityState.Active && payrollGroup.TimePeriodHead.TimePeriod != null)
                timePeriodHead = payrollGroup.TimePeriodHead;

            //Prio 2: Company
            if (timePeriodHead == null)
                timePeriodHead = GetDefaultTimePeriodHead(entities, actorCompanyId, loadPeriods);

            return timePeriodHead;
        }

        public TimePeriodHead GetTimePeriodHead(int timePeriodHeadId, int actorCompanyId, bool loadPeriods = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.TimePeriodHead.NoTracking();
            return GetTimePeriodHead(entities, timePeriodHeadId, actorCompanyId, loadPeriods);
        }

        public TimePeriodHead GetTimePeriodHead(CompEntities entities, int timePeriodHeadId, int actorCompanyId, bool loadPeriods = false)
        {
            IQueryable<TimePeriodHead> query = entities.TimePeriodHead;
            if (loadPeriods)
                query = query.Include("TimePeriod");

            return (from h in query
                    where h.TimePeriodHeadId == timePeriodHeadId &&
                    h.ActorCompanyId == actorCompanyId
                    select h).FirstOrDefault();
        }

        public TimePeriodHead GetTimePeriodHeadByName(string name, int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.TimePeriodHead.NoTracking();
            return GetTimePeriodHeadByName(entities, name, actorCompanyId);
        }

        public TimePeriodHead GetTimePeriodHeadByName(CompEntities entities, string name, int actorCompanyId)
        {
            return (from tph in entities.TimePeriodHead
                    where tph.Name == name &&
                    tph.ActorCompanyId == actorCompanyId
                    select tph).FirstOrDefault();
        }

        public ActionResult SaveTimePeriodHead(TimePeriodHeadDTO timePeriodHeadInput, int actorCompanyId, bool removePeriodLinks)
        {
            ActionResult result = new ActionResult(true);

            if (timePeriodHeadInput == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "TimePeriodHead");

            int timePeriodHeadId = 0;

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        #region Validate input

                        if (String.IsNullOrEmpty(timePeriodHeadInput.Name))
                            return new ActionResult((int)ActionResultSave.TimePeriodHeadNameMandatory);

                        if (timePeriodHeadInput.TimePeriodType == TermGroup_TimePeriodType.Unknown)
                            return new ActionResult((int)ActionResultSave.TimePeriodHeadTypeMandatory);

                        if (timePeriodHeadInput.TimePeriods == null)
                            return new ActionResult((int)ActionResultSave.EntityNotFound, "TimePeriods");

                        #endregion

                        #region Validate time

                        var validationItemsTime = new List<DateIntervalValidationDTO>();
                        timePeriodHeadInput.TimePeriods.Where(tp => !tp.ExtraPeriod).OrderByDescending(o => o.RowNr).ToList().ForEach(tp => validationItemsTime.Add(new DateIntervalValidationDTO(tp.StartDate, tp.StopDate)));
                        result = validationItemsTime.Validate(true, false);
                        if (!result.Success)
                            return new ActionResult(result.ErrorNumber, GetText(9316, "Perioduppsättningen innehåller perioder med ogiltiga eller överlappande datum"));

                        #endregion

                        #region Validate payroll

                        var validationItemsPayroll = new List<DateIntervalValidationDTO>();
                        timePeriodHeadInput.TimePeriods.Where(tp => !tp.ExtraPeriod && tp.PayrollStartDate.HasValue && tp.PayrollStopDate.HasValue).ToList().ForEach(tp => validationItemsPayroll.Add(new DateIntervalValidationDTO(tp.PayrollStartDate.Value, tp.PayrollStopDate.Value)));
                        result = validationItemsPayroll.Validate(true, false);
                        if (!result.Success)
                            return new ActionResult(result.ErrorNumber, GetText(9316, "Perioduppsättningen innehåller perioder med ogiltiga eller överlappande datum"));

                        #endregion

                        TimePeriodHead timePeriodHead = null;
                        if (timePeriodHeadInput.TimePeriodHeadId == 0)
                        {
                            #region Add

                            #region TimePeriodHead

                            timePeriodHead = new TimePeriodHead()
                            {
                                Name = timePeriodHeadInput.Name,
                                Description = timePeriodHeadInput.Description,
                                TimePeriodType = (int)timePeriodHeadInput.TimePeriodType,
                                AccountId = timePeriodHeadInput.AccountId.ToNullable(),
                                ChildId = timePeriodHeadInput.ChildId.ToNullable(),
                                PayrollProductDistributionRuleHeadId = timePeriodHeadInput.PayrollProductDistributionRuleHeadId != 0 ? timePeriodHeadInput.PayrollProductDistributionRuleHeadId : null,

                                //Set FK
                                ActorCompanyId = actorCompanyId,
                            };
                            SetCreatedProperties(timePeriodHead);
                            entities.TimePeriodHead.AddObject(timePeriodHead);

                            #endregion

                            #region TimePeriod

                            foreach (var timePeriodInput in timePeriodHeadInput.TimePeriods.OrderBy(i => i.RowNr))
                            {
                                var timePeriod = new TimePeriod()
                                {
                                    RowNr = timePeriodInput.RowNr,
                                    Name = timePeriodInput.Name,
                                    StartDate = timePeriodInput.StartDate,
                                    StopDate = timePeriodInput.StopDate,
                                    PayrollStartDate = timePeriodInput.PayrollStartDate,
                                    PayrollStopDate = timePeriodInput.PayrollStopDate,
                                    PaymentDate = timePeriodInput.PaymentDate,
                                    ExtraPeriod = timePeriodInput.ExtraPeriod,
                                    Comment = StringUtility.NullToEmpty(timePeriodInput.Comment)
                                };
                                SetCreatedProperties(timePeriod);

                                timePeriodHead.TimePeriod.Add(timePeriod);
                            }

                            #endregion

                            #endregion
                        }
                        else
                        {
                            #region Update

                            timePeriodHead = GetTimePeriodHead(entities, timePeriodHeadInput.TimePeriodHeadId, actorCompanyId, loadPeriods: true);
                            if (timePeriodHead == null)
                                return new ActionResult((int)ActionResultSave.EntityNotFound, "TimePeriodHead");

                            #region TimePeriodHead

                            timePeriodHead.Name = timePeriodHeadInput.Name;
                            timePeriodHead.Description = timePeriodHeadInput.Description;
                            timePeriodHead.TimePeriodType = (int)timePeriodHeadInput.TimePeriodType;
                            timePeriodHead.AccountId = timePeriodHeadInput.AccountId.ToNullable();
                            timePeriodHead.ChildId = timePeriodHeadInput.ChildId.ToNullable();
                            timePeriodHead.PayrollProductDistributionRuleHeadId = timePeriodHeadInput.PayrollProductDistributionRuleHeadId != 0 ? timePeriodHeadInput.PayrollProductDistributionRuleHeadId : null;
                            SetModifiedProperties(timePeriodHead);

                            #endregion

                            List<TimePeriod> existingTimePeriods = timePeriodHead.TimePeriod.Where(x => x.State == (int)SoeEntityState.Active).OrderBy(i => i.RowNr).ToList();

                            #region Add TimePeriod

                            foreach (var timePeriodInput in timePeriodHeadInput.TimePeriods.Where(i => i.TimePeriodId == 0).OrderBy(i => i.RowNr))
                            {
                                TimePeriod timePeriod = new TimePeriod()
                                {
                                    RowNr = timePeriodInput.RowNr,
                                    Name = timePeriodInput.Name,
                                    StartDate = timePeriodInput.StartDate,
                                    StopDate = timePeriodInput.StopDate,
                                    PayrollStartDate = timePeriodInput.PayrollStartDate,
                                    PayrollStopDate = timePeriodInput.PayrollStopDate,
                                    PaymentDate = timePeriodInput.PaymentDate,
                                    ExtraPeriod = timePeriodInput.ExtraPeriod,
                                    Comment = StringUtility.NullToEmpty(timePeriodInput.Comment)
                                };
                                SetCreatedProperties(timePeriod);

                                timePeriodHead.TimePeriod.Add(timePeriod);
                            }

                            #endregion

                            #region Update/Delete TimePeriod

                            //Check all existing TimePeriods if they are updated or deleted in input collection
                            for (int i = existingTimePeriods.Count - 1; i >= 0; i--)
                            {
                                TimePeriod timePeriod = existingTimePeriods[i];
                                if (timePeriod == null || timePeriod.TimePeriodId == 0)
                                    continue;

                                TimePeriodDTO timePeriodInput = timePeriodHeadInput.TimePeriods.FirstOrDefault(tp => tp.TimePeriodId == timePeriod.TimePeriodId);
                                if (timePeriodInput == null)
                                {
                                    #region Delete

                                    if (HasTimePeriodTransactions(timePeriod))
                                    {
                                        return new ActionResult((int)ActionResultSave.TimePeriodHasTransactions, GetText(10131, "Perioden innehåller transaktioner och kan inte tas bort"));
                                    }
                                    else if (HasEmployeeTimePeriods(timePeriod))
                                    {
                                        return new ActionResult((int)ActionResultSave.TimePeriodHasEmployeeTimePeriod, GetText(110671, "En eller flera lönekörningar är kopplad till perioden och kan inte tas bort"));
                                    }
                                    else if (HasEmployeeGroupRuleWork(timePeriod))
                                    {
                                        if (removePeriodLinks)
                                        {
                                            foreach (EmployeeGroupRuleWorkTimePeriod rule in timePeriod.EmployeeGroupRuleWorkTimePeriod.ToList())
                                            {
                                                entities.DeleteObject(rule);
                                            }
                                            ChangeEntityState(timePeriod, SoeEntityState.Deleted);
                                        }
                                        else
                                        {
                                            return new ActionResult((int)ActionResultSave.TimePeriodHasEmployeeGroupRuleWork, GetText(10934, "Perioden är kopplad till ett eller flera tidavtal.\nFör att kunna ta bort perioden måste även kopplingen till tidavtalet tas bort.\n\nVill du fortsätta?"));
                                        }
                                    }
                                    else
                                    {
                                        ChangeEntityState(timePeriod, SoeEntityState.Deleted);
                                    }

                                    #endregion
                                }
                                else
                                {
                                    #region Update

                                    //Only update if changed (otherwise save times out on large TimePeriodHeads)
                                    bool isModified = false;
                                    if (timePeriod.RowNr != timePeriodInput.RowNr)
                                    {
                                        timePeriod.RowNr = timePeriodInput.RowNr;
                                        isModified = true;
                                    }
                                    if (timePeriod.Name != timePeriodInput.Name)
                                    {
                                        timePeriod.Name = timePeriodInput.Name;
                                        isModified = true;
                                    }
                                    if (timePeriod.StartDate != timePeriodInput.StartDate)
                                    {
                                        timePeriod.StartDate = timePeriodInput.StartDate;
                                        isModified = true;
                                    }
                                    if (timePeriod.StopDate != timePeriodInput.StopDate)
                                    {
                                        timePeriod.StopDate = timePeriodInput.StopDate;
                                        isModified = true;
                                    }
                                    if (timePeriod.PayrollStartDate != timePeriodInput.PayrollStartDate)
                                    {
                                        timePeriod.PayrollStartDate = timePeriodInput.PayrollStartDate;
                                        isModified = true;
                                    }
                                    if (timePeriod.PayrollStopDate != timePeriodInput.PayrollStopDate)
                                    {
                                        timePeriod.PayrollStopDate = timePeriodInput.PayrollStopDate;
                                        isModified = true;
                                    }
                                    if (timePeriod.PaymentDate != timePeriodInput.PaymentDate)
                                    {
                                        timePeriod.PaymentDate = timePeriodInput.PaymentDate;
                                        isModified = true;
                                    }
                                    if (timePeriod.ExtraPeriod != timePeriodInput.ExtraPeriod)
                                    {
                                        timePeriod.ExtraPeriod = timePeriodInput.ExtraPeriod;
                                        isModified = true;
                                    }
                                    if (timePeriod.Comment != timePeriodInput.Comment)
                                    {
                                        timePeriod.Comment = timePeriodInput.Comment;
                                        isModified = true;
                                    }

                                    if (isModified)
                                        SetModifiedProperties(timePeriod);

                                    #endregion
                                }
                            }

                            #endregion

                            #endregion
                        }

                        if (result.Success)
                        {
                            result = SaveChanges(entities, transaction);
                            timePeriodHeadId = timePeriodHead.TimePeriodHeadId;
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
                }
                finally
                {
                    if (result.Success)
                    {
                        //Set success properties
                    }
                    else
                        base.LogTransactionFailed(this.ToString(), this.log);

                    result.IntegerValue = timePeriodHeadId;

                    entities.Connection.Close();
                }

                return result;
            }
        }

        public ActionResult DeleteTimePeriodHead(int timePeriodHeadId, int actorCompanyId, bool removePeriodLinks)
        {
            if (timePeriodHeadId == 0)
                return new ActionResult((int)ActionResultDelete.EntityIsNull, "TimePeriodHead");

            using (CompEntities entities = new CompEntities())
            {
                TimePeriodHead originalTimePeriodHead = GetTimePeriodHead(entities, timePeriodHeadId, actorCompanyId, loadPeriods: true);
                if (originalTimePeriodHead == null)
                    return new ActionResult((int)ActionResultDelete.EntityNotFound, "Customer");

                foreach (TimePeriod timePeriod in originalTimePeriodHead.TimePeriod.Where(x => x.State == (int)SoeEntityState.Active).ToList())
                {
                    if (HasTimePeriodTransactions(timePeriod))
                    {
                        return new ActionResult((int)ActionResultSave.TimePeriodHasTransactions, GetText(10131, "Perioden innehåller transaktioner och kan inte tas bort"));
                    }
                    else if (HasEmployeeGroupRuleWork(timePeriod))
                    {
                        if (removePeriodLinks)
                        {
                            foreach (EmployeeGroupRuleWorkTimePeriod rule in timePeriod.EmployeeGroupRuleWorkTimePeriod.ToList())
                            {
                                entities.DeleteObject(rule);
                            }
                            entities.DeleteObject(timePeriod);
                        }
                        else
                        {
                            return new ActionResult((int)ActionResultSave.TimePeriodHasEmployeeGroupRuleWork, GetText(10934, "Perioden är kopplad till ett eller flera tidavtal.\nFör att kunna ta bort perioden måste även kopplingen till tidavtalet tas bort.\n\nVill du fortsätta?"));
                        }
                    }
                }

                List<TimePeriodHead> children = GetTimePeriodHeadParents(timePeriodHeadId, actorCompanyId);
                if (children.Any())
                {
                    string msg = GetText(10944, "Det finns en eller flera andra perioder som har denna period som undernivå.\nFör att kunna ta bort denna period måste först de relaterade perioderna tas bort eller kopplas mot en annan period.\n\nRelaterade perioder:");
                    msg += "\n" + String.Join("\n", children.Select(c => c.Name).ToArray());
                    return new ActionResult((int)ActionResultSave.TimePeriodHeadIsParent, msg);
                }

                ActionResult result = ChangeEntityState(originalTimePeriodHead, SoeEntityState.Deleted);
                if (result.Success)
                    result = SaveChanges(entities);

                return result;
            }
        }

        private bool HasTimePeriodTransactions(TimePeriod timePeriod)
        {
            if (timePeriod == null)
                return false;

            if (!timePeriod.TimePayrollTransaction.IsLoaded)
                timePeriod.TimePayrollTransaction.Load();

            return timePeriod.TimePayrollTransaction.Any(i => i.State == (int)SoeEntityState.Active);
        }

        private bool HasEmployeeTimePeriods(TimePeriod timePeriod)
        {
            if (timePeriod == null)
                return false;

            if (!timePeriod.EmployeeTimePeriod.IsLoaded)
                timePeriod.EmployeeTimePeriod.Load();

            return timePeriod.EmployeeTimePeriod.Any(i => i.State == (int)SoeEntityState.Active);
        }

        private bool HasEmployeeGroupRuleWork(TimePeriod timePeriod)
        {
            if (timePeriod == null)
                return false;

            if (!timePeriod.EmployeeGroupRuleWorkTimePeriod.IsLoaded)
                timePeriod.EmployeeGroupRuleWorkTimePeriod.Load();

            return timePeriod.EmployeeGroupRuleWorkTimePeriod.Any();
        }

        #endregion

        #region TimePeriod

        public List<TimePeriod> GetTimePeriods(List<int> timePeriodIds, int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.TimePeriod.NoTracking();
            return GetTimePeriods(entities, timePeriodIds, actorCompanyId);
        }

        public List<TimePeriod> GetTimePeriods(CompEntities entities, List<int> timePeriodIds, int actorCompanyId)
        {
            return (from tp in entities.TimePeriod
                    .Include("TimePeriodHead")
                    where timePeriodIds.Contains(tp.TimePeriodId) &&
                    tp.TimePeriodHead.ActorCompanyId == actorCompanyId &&
                    tp.TimePeriodHead.State == (int)SoeEntityState.Active && 
                    tp.State == (int)SoeEntityState.Active
                    orderby tp.RowNr descending
                    select tp).ToList();
        }

        public List<TimePeriod> GetTimePeriods(int timePeriodHeadId, int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.TimePeriod.NoTracking();
            return GetTimePeriods(entities, timePeriodHeadId, actorCompanyId);
        }

        public List<TimePeriod> GetTimePeriods(CompEntities entities, int timePeriodHeadId, int actorCompanyId)
        {
            return (from tp in entities.TimePeriod
                        .Include("TimePeriodHead")
                    where tp.TimePeriodHead.TimePeriodHeadId == timePeriodHeadId &&
                    tp.TimePeriodHead.ActorCompanyId == actorCompanyId && 
                    tp.State == (int)SoeEntityState.Active
                    orderby tp.RowNr descending
                    select tp).ToList();
        }

        public List<TimePeriod> GetTimePeriods(TermGroup_TimePeriodType periodType, int actorCompanyId, bool addTimePeriodHeadName = true)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.TimePeriod.NoTracking();
            return GetTimePeriods(entities, periodType, actorCompanyId, addTimePeriodHeadName);
        }

        public List<TimePeriod> GetTimePeriods(CompEntities entities, TermGroup_TimePeriodType periodType, int actorCompanyId, bool addTimePeriodHeadName = true)
        {
            var timePeriods = (from tp in entities.TimePeriod
                                   .Include("TimePeriodHead")
                               where tp.TimePeriodHead.ActorCompanyId == actorCompanyId &&
                               tp.TimePeriodHead.TimePeriodType == (int)periodType && 
                               tp.State == (int)SoeEntityState.Active
                               orderby tp.StartDate
                               select tp).ToList<TimePeriod>();

            if (addTimePeriodHeadName)
            {
                foreach (TimePeriod timePeriod in timePeriods)
                {
                    if (!timePeriod.Name.Contains(timePeriod.TimePeriodHead.Name))
                        timePeriod.Name = $"{timePeriod.Name} ({timePeriod.TimePeriodHead.Name})";
                }
            }

            return timePeriods;
        }

        public List<TimePeriod> GetTimePeriods(CompEntities entites, DateTime date, TermGroup_TimePeriodType periodType, int actorCompanyId)
        {
            return (from tp in entites.TimePeriod
                    where tp.TimePeriodHead.ActorCompanyId == actorCompanyId &&
                    tp.TimePeriodHead.TimePeriodType == (int)periodType &&
                    tp.StartDate <= date && tp.StopDate >= date &&
                    tp.State == (int)SoeEntityState.Active &&
                    tp.TimePeriodHead.State == (int)SoeEntityState.Active
                    select tp).ToList();
        }

        public List<TimePeriod> GetTimePeriodsWithPaymentDatesThisYear(CompEntities entities, int actorCompanyId, int year)
        {
            return (from tp in entities.TimePeriod
                     .Include("TimePeriodHead")
                    where tp.TimePeriodHead.ActorCompanyId == actorCompanyId &&
                    tp.TimePeriodHead.State == (int)SoeEntityState.Active &&
                    tp.PaymentDate.HasValue &&
                    tp.PaymentDate.Value.Year == year && 
                    tp.State == (int)SoeEntityState.Active
                    select tp).ToList();
        }
        
        public List<TimePeriod> GetVacationTimePeriodsForEmployees(CompEntities entities, int actorCompanyId, List<int> employeeIds, DateTime startDate, DateTime stopDate, int? mandatoryTimePeriodId = null)
        {
            var timePeriods = entities.TimePeriod
                .Include("TimePeriodHead")
                .Where(tp =>
                    tp.TimePeriodHead.ActorCompanyId == actorCompanyId &&
                    tp.TimePeriodHead.State == (int)SoeEntityState.Active &&
                    tp.State == (int)SoeEntityState.Active &&
                    tp.PaymentDate.HasValue &&
                    tp.PaymentDate >= startDate && tp.PaymentDate <= stopDate)
                .ToList();

            if (timePeriods.Any())
            {
                var validTimePeriodIds = entities.EmployeeTimePeriod
                    .Where(etp =>
                        employeeIds.Contains(etp.EmployeeId) &&
                        etp.State == (int)SoeEntityState.Active)
                    .Select(f => f.TimePeriodId)
                    .ToList();

                timePeriods = timePeriods.Where(l => validTimePeriodIds.Contains(l.TimePeriodId)).ToList();
            }

            if (mandatoryTimePeriodId.HasValue && !timePeriods.Any(a => a.TimePeriodId == mandatoryTimePeriodId.Value))
            {
                TimePeriod timePeriod = GetTimePeriod(entities, mandatoryTimePeriodId.Value, actorCompanyId);
                if (timePeriod != null)
                    timePeriods.Add(timePeriod);
            }

            return timePeriods;
        }

        public List<TimePeriod> GetTimePeriodsConnectedToPayrollGroups(TermGroup_TimePeriodType periodType, int actorCompanyId, bool addTimePeriodHeadName = true)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.TimePeriod.NoTracking();
            return GetTimePeriodsConnectedToPayrollGroups(entities, periodType, actorCompanyId);
        }

        public List<TimePeriod> GetTimePeriodsConnectedToPayrollGroups(CompEntities entities, TermGroup_TimePeriodType periodType, int actorCompanyId)
        {
            var timePeriods = (from tp in entities.TimePeriod
                                   .Include("TimePeriodHead")
                               where tp.TimePeriodHead.ActorCompanyId == actorCompanyId &&
                               tp.State == (int)SoeEntityState.Active &&
                               tp.TimePeriodHead.TimePeriodType == (int)periodType &&
                               tp.TimePeriodHead.PayrollGroup.Any() &&
                               tp.TimePeriodHead.State == (int)SoeEntityState.Active
                               orderby tp.StartDate
                               select tp).ToList<TimePeriod>();
            return timePeriods;
        }

        public List<TimePeriod> GetTimePeriodsToDate(int nrOfPeriods, int timePeriodId, int actorCompanyId, bool loadTimePeriodAccountValue = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.TimePeriod.NoTracking();
            return GetTimePeriodsToDate(entities, nrOfPeriods, timePeriodId, actorCompanyId, loadTimePeriodAccountValue);
        }

        public List<TimePeriod> GetTimePeriodsToDate(CompEntities entities, int nrOfPeriods, int timePeriodId, int actorCompanyId, bool loadTimePeriodAccountValue = false)
        {
            List<TimePeriod> timePeriods = new List<TimePeriod>();

            TimePeriod currentTimePeriod = TimePeriodManager.GetTimePeriod(entities, timePeriodId, actorCompanyId, loadTimePeriodHead: true);
            if (currentTimePeriod != null)
            {
                IQueryable<TimePeriod> oQuery = entities.TimePeriod;
                if (loadTimePeriodAccountValue)
                    oQuery = oQuery.Include("TimePeriodAccountValue");

                timePeriods = (from tp in oQuery
                               where tp.TimePeriodHead.ActorCompanyId == currentTimePeriod.TimePeriodHead.ActorCompanyId &&
                               tp.TimePeriodHead.TimePeriodHeadId == currentTimePeriod.TimePeriodHead.TimePeriodHeadId &&
                               tp.StopDate <= currentTimePeriod.StopDate && 
                               tp.State == (int)SoeEntityState.Active
                               orderby tp.StopDate descending
                               select tp).Take(nrOfPeriods).ToList();
            }

            return timePeriods;
        }

        public List<TimePeriod> GetDefaultTimePeriods(TermGroup_TimePeriodType periodType, bool onlyOpenPeriods, int? payrollGroupId, int? employeeId, int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.TimePeriod.NoTracking();
            return GetDefaultTimePeriods(entities, periodType, onlyOpenPeriods, payrollGroupId, employeeId, actorCompanyId);
        }

        public List<TimePeriod> GetDefaultTimePeriods(CompEntities entities, TermGroup_TimePeriodType periodType, bool onlyOpenPeriods, int? payrollGroupId, int? employeeId, int actorCompanyId)
        {
            List<TimePeriod> timePeriods = new List<TimePeriod>();

            TimePeriodHead timePeriodHead = GetDefaultTimePeriodHead(entities, payrollGroupId, actorCompanyId, loadPeriods: true);
            if (timePeriodHead == null)
                return timePeriods;

            if (onlyOpenPeriods && employeeId.HasValue)
            {
                List<EmployeeTimePeriod> employeeTimePeriods = GetEmployeeTimePeriods(entities, employeeId.Value, actorCompanyId);
                foreach (TimePeriod timePeriod in timePeriodHead.TimePeriod.Where(x => x.State == (int)SoeEntityState.Active))
                {
                    EmployeeTimePeriod employeeTimePeriod = employeeTimePeriods.FirstOrDefault(i => i.TimePeriodId == timePeriod.TimePeriodId);

                    bool valid = true;
                    if (employeeTimePeriod != null && employeeTimePeriod.Status != (int)SoeEmployeeTimePeriodStatus.Open)
                        valid = false;

                    if (valid)
                        timePeriods.Add(timePeriod);
                }
            }
            else
            {
                timePeriods.AddRange(timePeriodHead.TimePeriod);
            }

            return timePeriods;
        }

        public List<TimePeriod> GetDefaultTimePeriods(int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.TimePeriod.NoTracking();
            return GetDefaultTimePeriods(entities, actorCompanyId);
        }

        public List<TimePeriod> GetDefaultTimePeriods(CompEntities entities, int actorCompanyId)
        {
            int defaultTimePeriodHeadId = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.TimeDefaultTimePeriodHead, 0, actorCompanyId, 0);
            if (defaultTimePeriodHeadId == 0)
                return new List<TimePeriod>();

            return GetTimePeriods(entities, defaultTimePeriodHeadId, actorCompanyId);
        }

        public Dictionary<int, string> GetTimePeriodsDict(int timePeriodHeadId, bool addEmptyRow, int actorCompanyId)
        {
            var dict = new Dictionary<int, string>();
            if (addEmptyRow)
                dict.Add(0, " ");

            var timePeriods = GetTimePeriods(timePeriodHeadId, actorCompanyId);
            foreach (var timePeriod in timePeriods)
            {
                dict.Add(timePeriod.TimePeriodId, timePeriod.Name);
            }

            return dict;
        }

        public TimePeriod GetTimePeriod(int timePeriodId, int actorCompanyId, bool loadTimePeriodHead = false, bool loadTimePeriodAccountValue = false, List<TimePeriod> timePeriods = null)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.TimePeriod.NoTracking();
            return GetTimePeriod(entities, timePeriodId, actorCompanyId, loadTimePeriodHead, loadTimePeriodAccountValue, timePeriods);
        }

        public TimePeriod GetTimePeriod(CompEntities entities, int timePeriodId, int actorCompanyId, bool loadTimePeriodHead = false, bool loadTimePeriodAccountValue = false, List<TimePeriod> timePeriods = null)
        {
            TimePeriod timePeriod = null;
            if (timePeriods != null && !loadTimePeriodHead && !loadTimePeriodAccountValue)
            {
                timePeriod = timePeriods.FirstOrDefault(tp => tp.TimePeriodId == timePeriodId && tp.TimePeriodHead.ActorCompanyId == actorCompanyId);
                if (timePeriod != null)
                    return timePeriod;
            }

            IQueryable<TimePeriod> query = entities.TimePeriod;
            if (loadTimePeriodHead)
                query = query.Include("TimePeriodHead");
            if (loadTimePeriodAccountValue)
                query = query.Include("TimePeriodAccountValue");
            return query.FirstOrDefault(tp => tp.TimePeriodId == timePeriodId && tp.TimePeriodHead.ActorCompanyId == actorCompanyId && tp.State == (int)SoeEntityState.Active);
        }

        public TimePeriod GetTimePeriod(DateTime date, int timePeriodHeadId, int actorCompanyId, bool loadTimePeriodHead = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.TimePeriod.NoTracking();
            return GetTimePeriod(entities, date, timePeriodHeadId, actorCompanyId, loadTimePeriodHead);
        }

        public TimePeriod GetTimePeriod(CompEntities entities, DateTime date, int timePeriodHeadId, int actorCompanyId, bool loadTimePeriodHead = false)
        {
            IQueryable<TimePeriod> query = entities.TimePeriod;
            if (loadTimePeriodHead)
                query = query.Include("TimePeriodHead");

            return (from tp in query
                    where tp.TimePeriodHead.TimePeriodHeadId == timePeriodHeadId &&
                    tp.TimePeriodHead.ActorCompanyId == actorCompanyId &&
                    tp.StartDate <= date && tp.StopDate >= date && 
                    tp.State == (int)SoeEntityState.Active
                    select tp).FirstOrDefault();
        }

        public TimePeriod GetTimePeriod(CompEntities entities, int actorCompanyId, DateTime date, Employment employment = null, List<PayrollGroup> payrollGroups = null, bool usePayrollDate = false)
        {
            int? payrollGroupId = employment?.GetPayrollGroupId(date);

            int? timePeriodHeadId = null;
            if (payrollGroupId.HasValue)
                timePeriodHeadId = (payrollGroups?.FirstOrDefault(f => f.PayrollGroupId == payrollGroupId.Value) ?? PayrollManager.GetPayrollGroup(entities, payrollGroupId.Value))?.TimePeriodHeadId;

            if (usePayrollDate)
            {
                return (from tp in entities.TimePeriod
                        where tp.TimePeriodHead.ActorCompanyId == actorCompanyId &&
                        (!timePeriodHeadId.HasValue || tp.TimePeriodHead.TimePeriodHeadId == timePeriodHeadId.Value) &&
                        tp.TimePeriodHead.TimePeriodType == (int)TermGroup_TimePeriodType.Payroll &&
                        tp.PayrollStartDate.HasValue && tp.PayrollStopDate.HasValue &&
                        tp.PayrollStartDate.Value <= date && tp.PayrollStopDate.Value >= date && 
                        tp.State == (int)SoeEntityState.Active
                        select tp).FirstOrDefault();
            }
            else
            {
                return (from tp in entities.TimePeriod
                        where tp.TimePeriodHead.ActorCompanyId == actorCompanyId &&
                        (!timePeriodHeadId.HasValue || tp.TimePeriodHead.TimePeriodHeadId == timePeriodHeadId.Value) &&
                        tp.TimePeriodHead.TimePeriodType == (int)TermGroup_TimePeriodType.Payroll &&
                        tp.StartDate <= date && tp.StopDate >= date && 
                        tp.State == (int)SoeEntityState.Active
                        select tp).FirstOrDefault();
            }
        }

        public TimePeriod GetTimePeriod(CompEntities entities, List<TimePeriod> timePeriods, int actorCompanyId, Employment employment, DateTime date, bool usePayrollDate, List<PayrollGroup> payrollGroups = null)
        {
            int? timePeriodHeadId = null;
            if (employment != null)
            {
                int? payrollGroupId = employment.GetPayrollGroupId(date);
                if (payrollGroupId.HasValue)
                {
                    if (payrollGroups != null)
                    {
                        var payrollGroup = payrollGroups.FirstOrDefault(f => f.PayrollGroupId == payrollGroupId.Value);

                        if (payrollGroup?.TimePeriodHeadId != null)
                            timePeriodHeadId = payrollGroup.TimePeriodHeadId;
                    }

                    if (!timePeriodHeadId.HasValue)
                        timePeriodHeadId = PayrollManager.GetPayrollGroup(entities, payrollGroupId.Value)?.TimePeriodHeadId;
                }
            }

            if (usePayrollDate)
            {
                return (from tp in timePeriods
                        where tp.TimePeriodHead.ActorCompanyId == actorCompanyId &&
                        (!timePeriodHeadId.HasValue || tp.TimePeriodHead.TimePeriodHeadId == timePeriodHeadId.Value) &&
                        tp.TimePeriodHead.TimePeriodType == (int)TermGroup_TimePeriodType.Payroll &&
                        tp.PayrollStartDate.HasValue && tp.PayrollStopDate.HasValue &&
                        tp.PayrollStartDate.Value <= date && tp.PayrollStopDate.Value >= date
                        select tp).FirstOrDefault();
            }
            else
            {
                return (from tp in timePeriods
                        where tp.TimePeriodHead.ActorCompanyId == actorCompanyId &&
                        (!timePeriodHeadId.HasValue || tp.TimePeriodHead.TimePeriodHeadId == timePeriodHeadId.Value) &&
                        tp.TimePeriodHead.TimePeriodType == (int)TermGroup_TimePeriodType.Payroll &&
                        tp.StartDate <= date && tp.StopDate >= date
                        select tp).FirstOrDefault();
            }
        }

        public TimePeriod GetTimePeriod(int actorCompanyId, DateTime date, int noOfDays)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.TimePeriod.NoTracking();
            return GetTimePeriod(entities, actorCompanyId, date, noOfDays);
        }

        public TimePeriod GetTimePeriod(CompEntities entities, int actorCompanyId, DateTime date, int noOfDays)
        {
            TimePeriod period = null;

            List<TimePeriodHead> heads = GetTimePeriodHeadsIncludingPeriods(actorCompanyId);
            foreach (TimePeriodHead head in heads)
            {
                period = head.TimePeriod.FirstOrDefault(p => p.StopDate.AddDays(noOfDays).Date == date && p.State == (int)SoeEntityState.Active);
                if (period != null)
                    break;
            }

            return period;
        }

        public TimePeriod GetTimePeriod(DateTime date, TermGroup_TimePeriodType periodType, int actorCompanyId, bool loadTimePeriodHead = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.TimePeriod.NoTracking();
            return GetTimePeriod(entities, date, periodType, actorCompanyId, loadTimePeriodHead);
        }

        public TimePeriod GetTimePeriod(CompEntities entites, DateTime date, TermGroup_TimePeriodType periodType, int actorCompanyId, bool loadTimePeriodHead = false, List<TimePeriod> timePeriods = null, bool allowExtraPeriod = true)
        {
            TimePeriod timePeriod = null;
            if (timePeriods != null && !loadTimePeriodHead)
            {
                timePeriod = timePeriods.FirstOrDefault(tp =>
                                                        tp.TimePeriodHead.ActorCompanyId == actorCompanyId &&
                                                        tp.TimePeriodHead.TimePeriodType == (int)periodType &&
                                                        tp.State == (int)SoeEntityState.Active &&
                                                        tp.StartDate <= date && tp.StopDate >= date &&
                                                        (allowExtraPeriod || !tp.ExtraPeriod));
                
                if (timePeriod != null)
                    return timePeriod;
            }

            IQueryable<TimePeriod> query = entites.TimePeriod;
            if (loadTimePeriodHead)
                query = query.Include("TimePeriodHead");

            return query.FirstOrDefault(tp => 
                                        tp.TimePeriodHead.ActorCompanyId == actorCompanyId && 
                                        tp.TimePeriodHead.TimePeriodType == (int)periodType && 
                                        tp.State == (int)SoeEntityState.Active && 
                                        tp.StartDate <= date && tp.StopDate >= date &&
                                        (allowExtraPeriod || !tp.ExtraPeriod));
        }

        public decimal GetEmploymentTaxBasisBeforeGivenPeriod(int actorCompanyId, int timePeriodId, int employeeId)
        {
            TimePeriod timePeriod = GetTimePeriod(timePeriodId, actorCompanyId);
            if (timePeriod == null || !timePeriod.PaymentDate.HasValue)
                return 0;

            List<EmploymentTaxTimePeriodHeadItemDTO> employmentTaxTimePeriodHeads = GetEmploymentTaxTimePeriodHeadDTOs(actorCompanyId, timePeriod.PaymentDate.Value.Year, new List<int> { employeeId });
            EmploymentTaxTimePeriodHeadItemDTO employmentTaxTimePeriodHead = employmentTaxTimePeriodHeads.FirstOrDefault(x => x.EmployeeId == employeeId);

            if (employmentTaxTimePeriodHead != null)
                return employmentTaxTimePeriodHead.GetEmploymentTaxBasisBeforeGivenPeriod(timePeriod.PaymentDate.Value);
            else
                return 0;
        }

        #endregion

        #region EmployeeTimePeriod

        public List<EmployeeTimePeriod> GetEmployeeTimePeriodsForEmployees(CompEntities entities, List<int> employeeIds, int actorCompanyId, int timePeriodId)
        {
            List<int> validEmployeeIds = EmployeeManager.GetEmployeeIdsForQuery(entities, employeeIds);

            IQueryable<EmployeeTimePeriod> query = entities.EmployeeTimePeriod;
            if (!validEmployeeIds.IsNullOrEmpty())
                query = query.Where(w => validEmployeeIds.Contains(w.EmployeeId));

            List<EmployeeTimePeriod> periods = (from etp in query
                            where 
                            etp.TimePeriodId == timePeriodId &&
                            etp.ActorCompanyId == actorCompanyId &&
                            etp.State == (int)SoeEntityState.Active
                           select etp).ToList();
                       
            periods = periods.Where(p => p.IsOpenOrHigher()).OrderBy(p => p.Created).ThenBy(p => p.Modified).GroupBy(p => p.EmployeeId).Select(p => p.First()).ToList();
            return periods.Where(p => employeeIds.Contains(p.EmployeeId)).ToList();
        }

        public List<EmployeeTimePeriod> GetEmployeesTimePeriodsWithValues(CompEntities entities, List<int> timePeriodIds, List<int> employeeIds, int actorCompanyId)
        {
            return (from etp in entities.EmployeeTimePeriod
                        .Include("EmployeeTimePeriodValue")
                    where employeeIds.Contains(etp.EmployeeId) &&
                    timePeriodIds.Contains(etp.TimePeriodId) &&
                    etp.ActorCompanyId == actorCompanyId &&
                    etp.State == (int)SoeEntityState.Active
                    select etp).ToList();
        }

        public List<EmployeeTimePeriod> GetLockedAndAboveEmployeesTimePeriodsWithValuesAndTimeperiod(CompEntities entities, int actorCompanyId, List<int> employeeIds, out List<TimePeriod> periods)
        {
            List<EmployeeTimePeriod> employeeTimePeriods = (from etp in entities.EmployeeTimePeriod
                                                            .Include("EmployeeTimePeriodValue")
                                                            where employeeIds.Contains(etp.EmployeeId) &&
                                                            etp.ActorCompanyId == actorCompanyId &&
                                                            etp.State == (int)SoeEntityState.Active
                                                            select etp).ToList();

            employeeTimePeriods = employeeTimePeriods.Where(x => x.Status >= (int)SoeEmployeeTimePeriodStatus.Paid).ToList();

            List<int> periodids = employeeTimePeriods.Select(x => x.TimePeriodId).Distinct().ToList();
            periods = (from tp in entities.TimePeriod
                       where periodids.Contains(tp.TimePeriodId)
                       select tp).ToList();

            return employeeTimePeriods;
        }

        public List<EmployeeTimePeriod> GetEmployeeTimePeriods(int actorCompanyId, List<int> timePeriodIds)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetEmployeeTimePeriods(entities, actorCompanyId, timePeriodIds);
        }

        public List<EmployeeTimePeriod> GetEmployeeTimePeriods(CompEntities entities, int actorCompanyId, List<int> timePeriodIds)
        {
            return (from etp in entities.EmployeeTimePeriod
                    where etp.ActorCompanyId == actorCompanyId &&
                    timePeriodIds.Contains(etp.TimePeriodId)
                    select etp).ToList();
        }

        public List<EmployeeTimePeriod> GetEmployeeTimePeriods(CompEntities entities, int employeeId, int actorCompanyId)
        {
            return (from etp in entities.EmployeeTimePeriod
                        .Include("TimePeriod")
                    where etp.EmployeeId == employeeId &&
                    etp.ActorCompanyId == actorCompanyId &&
                    etp.State == (int)SoeEntityState.Active
                    select etp).ToList();
        }

        public List<EmployeeTimePeriod> GetLockedEmployeeTimePeriodsSameMonth(CompEntities entities, int employeeId, int actorCompanyId, DateTime payrollPaymentDate)
        {
            DateTime startDate = CalendarUtility.GetBeginningOfMonth(payrollPaymentDate);
            DateTime stopDate = CalendarUtility.GetEndOfMonth(payrollPaymentDate);

            return (from etp in entities.EmployeeTimePeriod
                    where etp.EmployeeId == employeeId &&
                    etp.TimePeriod.PaymentDate.HasValue &&
                    etp.TimePeriod.PaymentDate >= startDate && etp.TimePeriod.PaymentDate <= stopDate &&
                    etp.ActorCompanyId == actorCompanyId &&
                    (etp.Status == (int)SoeEmployeeTimePeriodStatus.Locked || etp.Status == (int)SoeEmployeeTimePeriodStatus.Paid) &&
                    etp.State == (int)SoeEntityState.Active
                    select etp).ToList();
        }

        public List<EmployeeTimePeriod> GetEmployeeTimePeriodsWithValues(CompEntities entities, int timePeriodId, List<int> employeeIds, int actorCompanyId)
        {
            return (from etp in entities.EmployeeTimePeriod
                        .Include("EmployeeTimePeriodValue")
                    where employeeIds.Contains(etp.EmployeeId) &&
                    etp.TimePeriodId == timePeriodId &&
                    etp.ActorCompanyId == actorCompanyId &&
                    etp.State == (int)SoeEntityState.Active
                    select etp).ToList();
        }

        public List<EmployeeTimePeriod> GetEmployeeTimePeriodsWithValuesNotOpen(CompEntities entities, List<int> timePeriodIds, List<int> employeeIds, int actorCompanyId)
        {
            return (from etp in entities.EmployeeTimePeriod
                        .Include("EmployeeTimePeriodValue")
                    where employeeIds.Contains(etp.EmployeeId) &&
                    timePeriodIds.Contains(etp.TimePeriodId) &&
                    etp.ActorCompanyId == actorCompanyId &&
                    etp.State == (int)SoeEntityState.Active &&
                    etp.Status != (int)SoeEmployeeTimePeriodStatus.Open
                    select etp).ToList();
        }

        public List<EmployeeTimePeriod> GetEmployeeTimePeriodsWithValuesAndTimePeriod(CompEntities entities, List<int> timePeriodIds, List<int> employeeIds, int actorCompanyId)
        {
            return (from etp in entities.EmployeeTimePeriod
                        .Include("EmployeeTimePeriodValue")
                        .Include("TimePeriod.TimePeriodHead")
                    where employeeIds.Contains(etp.EmployeeId) &&
                    timePeriodIds.Contains(etp.TimePeriodId) &&
                    etp.ActorCompanyId == actorCompanyId &&
                    etp.State == (int)SoeEntityState.Active
                    select etp).ToList();
        }

        public List<EmployeeTimePeriod> GetPreviousLockedOrPaidEmployeeTimePeriods(CompEntities entities, TimePeriod currentTimePeriod, int employeeId, int actorCompanyId)
        {
            if (!currentTimePeriod.PaymentDate.HasValue)
                return new List<EmployeeTimePeriod>();

            var periods = (from etpv in entities.EmployeeTimePeriod
                           .Include("EmployeeTimePeriodValue")
                           where etpv.EmployeeId == employeeId &&
                           etpv.ActorCompanyId == actorCompanyId &&
                           etpv.TimePeriod.PaymentDate.HasValue && etpv.TimePeriod.PaymentDate.Value.Year == currentTimePeriod.PaymentDate.Value.Year && etpv.TimePeriod.PaymentDate.Value < currentTimePeriod.PaymentDate.Value &&
                           (etpv.Status == (int)SoeEmployeeTimePeriodStatus.Locked || etpv.Status == (int)SoeEmployeeTimePeriodStatus.Paid) &&
                           etpv.State == (int)SoeEntityState.Active
                           select etpv).ToList();

            return periods.Where(x => x.State == (int)SoeEntityState.Active).ToList();
        }

        public List<EmployeeTimePeriod> GetEmployeeTimePeriodsForVacationDebtReport(CompEntities entities, List<int> employeeIds)
        {
            if (employeeIds.IsNullOrEmpty())
                return new List<EmployeeTimePeriod>();

            return entities.EmployeeTimePeriod
                    .Include("EmployeeTimePeriodValue")
                    .Include("EmployeeTimePeriodProductSetting")
                    .Include("TimePeriod.TimePeriodHead")
                    .Where(w => employeeIds.Contains(w.EmployeeId))
                    .ToList();
        }

        public List<EmployeeTimePeriodValue> GetPaidEmployeeTimePeriodValues(CompEntities entities, List<int> timePeriodIds, int employeeId, int actorCompanyId)
        {
            var values = (from etpv in entities.EmployeeTimePeriodValue
                          where etpv.EmployeeTimePeriod.EmployeeId == employeeId &&
                          etpv.EmployeeTimePeriod.ActorCompanyId == actorCompanyId &&
                          etpv.EmployeeTimePeriod.Status == (int)SoeEmployeeTimePeriodStatus.Paid &&
                          timePeriodIds.Contains(etpv.EmployeeTimePeriod.TimePeriodId) &&
                          etpv.EmployeeTimePeriod.State == (int)SoeEntityState.Active
                          select etpv).ToList();

            return values.Where(x => x.State == (int)SoeEntityState.Active).ToList();
        }

        public EmployeeTimePeriod GetEmployeeTimePeriod(int employeeId, int timePeriodId, int actorCompanyId, bool loadValues = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.EmployeeTimePeriod.NoTracking();
            return GetEmployeeTimePeriod(entities, employeeId, timePeriodId, actorCompanyId, loadValues);
        }

        public EmployeeTimePeriod GetEmployeeTimePeriod(CompEntities entities, int employeeId, int timePeriodId, int actorCompanyId, bool loadValues = false)
        {
            IQueryable<EmployeeTimePeriod> query = entities.EmployeeTimePeriod;
            if (loadValues)
                query = query.Include("EmployeeTimePeriodValue");

            return (from etp in query
                    where etp.EmployeeId == employeeId &&
                    etp.ActorCompanyId == actorCompanyId &&
                    etp.TimePeriodId == timePeriodId &&
                    etp.State == (int)SoeEntityState.Active
                    select etp).FirstOrDefault();
        }

        public EmployeeTimePeriod GetEmployeeTimePeriod(List<EmployeeTimePeriod> employeeTimePeriods, int employeeId, int timePeriodId, int actorCompanyId)
        {
            return (from etp in employeeTimePeriods
                    where etp.EmployeeId == employeeId &&
                    etp.ActorCompanyId == actorCompanyId &&
                    etp.TimePeriodId == timePeriodId &&
                    etp.State == (int)SoeEntityState.Active
                    select etp).FirstOrDefault();
        }

        public EmployeeTimePeriod GetEmployeeTimePeriodWithValues(CompEntities entities, int timePeriodId, int employeeId, int actorCompanyId)
        {
            return (from etp in entities.EmployeeTimePeriod
                        .Include("EmployeeTimePeriodValue")
                    where etp.EmployeeId == employeeId &&
                    etp.TimePeriodId == timePeriodId &&
                    etp.ActorCompanyId == actorCompanyId &&
                    etp.State == (int)SoeEntityState.Active
                    select etp).FirstOrDefault();
        }

        public EmployeeTimePeriod GetEmployeeTimePeriodWithValues(List<EmployeeTimePeriod> employeeTimePeriods, int timePeriodId, int employeeId, int actorCompanyId)
        {
            return (from etp in employeeTimePeriods
                    where etp.EmployeeId == employeeId &&
                    etp.TimePeriodId == timePeriodId &&
                    etp.ActorCompanyId == actorCompanyId &&
                    etp.State == (int)SoeEntityState.Active
                    select etp).FirstOrDefault();
        }

        #endregion

        #region EmployeeTimePeriodProductSetting

        public List<EmployeeTimePeriodProductSetting> GetEmployeeTimePeriodProductSettings(CompEntities entities, int employeeId, int timePeriodId, int actorCompanyId)
        {
            return (from x in entities.EmployeeTimePeriodProductSetting
                    where x.EmployeeTimePeriod.ActorCompanyId == actorCompanyId &&
                    x.EmployeeTimePeriod.EmployeeId == employeeId &&
                    x.EmployeeTimePeriod.TimePeriodId == timePeriodId &&
                    x.EmployeeTimePeriod.State == (int)SoeEntityState.Active &&
                    x.State == (int)SoeEntityState.Active
                    select x).ToList();
        }

        public EmployeeTimePeriodProductSetting GetEmployeeTimePeriodProductSetting(int payrollProductId, int employeeId, int timePeriodId, int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetEmployeeTimePeriodProductSetting(entities, payrollProductId, employeeId, timePeriodId, actorCompanyId);
        }

        private EmployeeTimePeriodProductSetting GetEmployeeTimePeriodProductSetting(CompEntities entities, int payrollProductId, int employeeId, int timePeriodId, int actorCompanyId)
        {
            return (from x in entities.EmployeeTimePeriodProductSetting
                    where x.EmployeeTimePeriod.ActorCompanyId == actorCompanyId &&
                    x.EmployeeTimePeriod.EmployeeId == employeeId &&
                    x.EmployeeTimePeriod.TimePeriodId == timePeriodId &&
                    x.PayrollProductId == payrollProductId &&
                    x.State == (int)SoeEntityState.Active
                    select x).FirstOrDefault();
        }

        private EmployeeTimePeriodProductSetting GetEmployeeTimePeriodProductSetting(CompEntities entities, int employeeTimePeriodProductSettingId)
        {
            return (from x in entities.EmployeeTimePeriodProductSetting
                    where x.EmployeeTimePeriodProductSettingId == employeeTimePeriodProductSettingId
                    select x).FirstOrDefault();
        }

        public ActionResult DeleteEmployeeTimePeriodProductSetting(int employeeTimePeriodProductSettingId)
        {
            ActionResult result = new ActionResult();

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {

                        var employeeTimePeriodProductSetting = GetEmployeeTimePeriodProductSetting(entities, employeeTimePeriodProductSettingId);

                        if (employeeTimePeriodProductSetting == null)
                            return new ActionResult((int)ActionResultDelete.EntityNotFound, "EmployeeTimePeriodProductSetting");

                        ChangeEntityState(employeeTimePeriodProductSetting, SoeEntityState.Deleted);

                        result = SaveChanges(entities);

                        //Commit transaction
                        if (result.Success)
                            transaction.Complete();
                    }
                }
                catch (Exception ex)
                {
                    base.LogError(ex, this.log);
                    result.Exception = ex;
                    log.Error(ex.Message, ex);
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

        public ActionResult SaveEmployeeTimePeriodProductSetting(int timePeriodId, int employeeId, EmployeeTimePeriodProductSettingDTO settingDTO, int actorCompanyId)
        {
            ActionResult result = new ActionResult();
            int employeeTimePeriodProductSettingId = settingDTO.EmployeeTimePeriodProductSettingId;

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {

                        EmployeeTimePeriod employeeTimePeriod = TimePeriodManager.GetEmployeeTimePeriod(entities, employeeId, timePeriodId, actorCompanyId);
                        if (employeeTimePeriod == null)
                            return new ActionResult((int)ActionResultDelete.EntityNotFound, "EmployeeTimePeriod");

                        EmployeeTimePeriodProductSetting setting = GetEmployeeTimePeriodProductSetting(entities, employeeTimePeriodProductSettingId);
                        if (setting == null)
                        {
                            #region Add

                            setting = new EmployeeTimePeriodProductSetting()
                            {
                                EmployeeTimePeriodId = employeeTimePeriod.EmployeeTimePeriodId,
                                State = (int)SoeEntityState.Active,
                            };

                            SetCreatedProperties(setting);
                            entities.EmployeeTimePeriodProductSetting.AddObject(setting);

                            #endregion
                        }
                        else
                        {
                            #region Update

                            SetModifiedProperties(setting);

                            #endregion
                        }

                        #region Common

                        setting.PayrollProductId = settingDTO.PayrollProductId;
                        setting.UseSettings = settingDTO.UseSettings;
                        setting.PrintOnSalarySpecification = settingDTO.PrintOnSalarySpecification;
                        setting.TaxCalculationType = (int)settingDTO.TaxCalculationType;
                        setting.Note = settingDTO.Note;

                        #endregion

                        result = SaveChanges(entities, transaction);
                        if (!result.Success)
                            return result;

                        //Commit transaction
                        if (result.Success)
                            transaction.Complete();
                    }
                }
                catch (Exception ex)
                {
                    base.LogError(ex, this.log);
                    result.Exception = ex;
                    log.Error(ex.Message, ex);
                }
                finally
                {
                    if (result.Success)
                    {
                        //Set success properties
                        result.IntegerValue = employeeTimePeriodProductSettingId;
                    }
                    else
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }

                return result;
            }

        }

        #endregion

        #region EmployeeGroupRuleWorkTimePeriod

        public List<int> EmployeeGroupsWithRuleWorkTimePeriods(int actorCompanyId, DateTime dateFrom, DateTime dateTo)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetEmployeeGroupsWithRuleWorkTimePeriods(entities, actorCompanyId, dateFrom, dateTo);
        }

        public List<int> GetEmployeeGroupsWithRuleWorkTimePeriods(CompEntities entities, int actorCompanyId, DateTime dateFrom, DateTime dateTo)
        {
            dateFrom = dateFrom.Date;
            dateTo = dateTo.Date; //DateTo cannot have clock > 00:00 because TimePeriod ends on 00:00

            return (from e in entities.EmployeeGroupRuleWorkTimePeriod
                    where e.EmployeeGroup.ActorCompanyId == actorCompanyId &&
                    e.EmployeeGroup.State == (int)SoeEntityState.Active &&
                    e.TimePeriod.StartDate <= dateFrom && e.TimePeriod.StopDate >= dateTo &&
                    e.TimePeriod.State == (int)SoeEntityState.Active &&
                    e.TimePeriod.TimePeriodHead.TimePeriodType == (int)TermGroup_TimePeriodType.RuleWorkTime &&
                    e.TimePeriod.TimePeriodHead.State == (int)SoeEntityState.Active
                    select e).Select(i => i.EmployeeGroupId).ToList();
        }

        public bool GetEmployeeGroupHasRuleWorkTimePeriods(int employeeGroupId, DateTime dateFrom, DateTime dateTo)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.EmployeeGroupRuleWorkTimePeriod.NoTracking();
            return EmployeeGroupHasRuleWorkTimePeriods(entities, employeeGroupId, dateFrom, dateTo);
        }

        public bool EmployeeGroupHasRuleWorkTimePeriods(CompEntities entities, int employeeGroupId, DateTime dateFrom, DateTime dateTo)
        {
            dateFrom = dateFrom.Date;
            dateTo = dateTo.Date; //DateTo cannot have clock > 00:00 because TimePeriod ends on 00:00

            return (from e in entities.EmployeeGroupRuleWorkTimePeriod
                    where e.EmployeeGroupId == employeeGroupId &&
                    e.TimePeriod.StartDate <= dateFrom && e.TimePeriod.StopDate >= dateTo &&
                    e.TimePeriod.TimePeriodHead.TimePeriodType == (int)TermGroup_TimePeriodType.RuleWorkTime &&
                    e.TimePeriod.TimePeriodHead.State == (int)SoeEntityState.Active
                    select e).Any();
        }

        public bool CompanyHasWithValidRuleWorkTimeSettings(CompEntities entities, int actorCompanyId, DateTime date)
        {
            if (entities.TimePeriodHead.Any(w => w.ActorCompanyId == actorCompanyId && w.TimePeriodType == (int)TermGroup_TimePeriodType.RuleWorkTime && w.State == (int)SoeEntityState.Active && w.TimePeriod.Any(a => a.StopDate >= date)))
            {
                if (SettingManager.GetBoolSetting(entities, SettingMainType.Company, (int)CompanySettingType.TimeCalculatePlanningPeriodScheduledTime, 0, actorCompanyId, 0))
                    return true;
            }

            return false;
        }

        public List<int> GetActorCompanyIdsWithValidRuleWorkTimePeriodSettings()
        {
            DateTime date = DateTime.Today;
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            var actorCompanIds = entitiesReadOnly.TimePeriodHead.Where(w => w.TimePeriodType == (int)TermGroup_TimePeriodType.RuleWorkTime && w.State == (int)SoeEntityState.Active && w.TimePeriod.Any(a => a.StopDate > date)).Select(s => s.ActorCompanyId).ToList();
            List<int> companyIdsWithSetting = SettingManager.GetCompanyIdsWithCompanyBoolSetting(CompanySettingType.TimeCalculatePlanningPeriodScheduledTime);
            return actorCompanIds.Where(w => companyIdsWithSetting.Contains(w)).ToList();
        }

        public int GetEmployeeGroupRuleWorkTime(int actorCompanyId, int employeeGroupId, DateTime date, int? timePeriodHeadId = null)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.EmployeeGroupRuleWorkTimePeriod.NoTracking();
            return GetEmployeeGroupRuleWorkTime(entities, actorCompanyId, employeeGroupId, date, timePeriodHeadId);
        }

        public int GetEmployeeGroupRuleWorkTime(CompEntities entities, int actorCompanyId, int employeeGroupId, DateTime date, int? timePeriodHeadId = null)
        {
            int ruleWorkTime = 0;

            if (!timePeriodHeadId.HasValidValue())
                timePeriodHeadId = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.TimeDefaultPlanningPeriod, 0, actorCompanyId, 0);

            if (timePeriodHeadId.HasValidValue())
            {
                EmployeeGroupRuleWorkTimePeriod period = (from e in entities.EmployeeGroupRuleWorkTimePeriod
                                                          where e.EmployeeGroupId == employeeGroupId &&
                                                          e.TimePeriod.TimePeriodHead.TimePeriodHeadId == timePeriodHeadId &&
                                                          e.TimePeriod.TimePeriodHead.TimePeriodType == (int)TermGroup_TimePeriodType.RuleWorkTime &&
                                                          e.TimePeriod.TimePeriodHead.State == (int)SoeEntityState.Active &&
                                                          e.TimePeriod.StartDate <= date && e.TimePeriod.StopDate >= date &&
                                                          e.TimePeriod.State == (int)SoeEntityState.Active
                                                          select e).FirstOrDefault();
                if (period != null)
                    ruleWorkTime = period.RuleWorkTime;
            }

            return ruleWorkTime;
        }

        public Dictionary<DateTime, decimal> GetEmployeeGroupRuleWorkTimes(int actorCompanyId, int employeeGroupId, DateTime dateFrom, DateTime dateTo, int? timePeriodHeadId = null)
        {
            Dictionary<DateTime, decimal> ruleWorkTimes = new Dictionary<DateTime, decimal>();
            if (dateFrom > dateTo)
                return ruleWorkTimes;

            if (!timePeriodHeadId.HasValidValue())
                timePeriodHeadId = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.TimeDefaultPlanningPeriod, 0, actorCompanyId, 0);

            if (timePeriodHeadId.HasValidValue())
            {
                using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.EmployeeGroupRuleWorkTimePeriod.NoTracking();
            List<EmployeeGroupRuleWorkTimePeriod> periods = (from e in entities.EmployeeGroupRuleWorkTimePeriod.Include("TimePeriod")
                                                                 where e.EmployeeGroupId == employeeGroupId &&
                                                                 e.TimePeriod.TimePeriodHead.TimePeriodHeadId == timePeriodHeadId &&
                                                                 e.TimePeriod.TimePeriodHead.TimePeriodType == (int)TermGroup_TimePeriodType.RuleWorkTime &&
                                                                 e.TimePeriod.TimePeriodHead.State == (int)SoeEntityState.Active &&
                                                                 e.TimePeriod.StartDate <= dateFrom && e.TimePeriod.StopDate >= dateTo &&
                                                                 e.TimePeriod.State == (int)SoeEntityState.Active 
                                                                 select e).ToList();

                DateTime currentDate = dateFrom;
                while (currentDate <= dateTo)
                {
                    EmployeeGroupRuleWorkTimePeriod period = periods.FirstOrDefault(p => p.TimePeriod.StartDate <= currentDate && p.TimePeriod.StopDate >= currentDate);
                    if (period != null)
                        ruleWorkTimes.Add(currentDate, Decimal.Divide(period.RuleWorkTime, period.TimePeriod.Days()));

                    currentDate = currentDate.AddDays(1);
                }
            }

            return ruleWorkTimes;
        }

        #endregion

        #region PlanningPeriod

        public PlanningPeriodHead GetPlanningPeriodHeadWithPeriods(DateTime date, int timePeriodHeadId, int actorCompanyId)
        {
            PlanningPeriodHead planningPeriodHead = new PlanningPeriodHead();
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();

            TimePeriodHead head = (from h in entitiesReadOnly.TimePeriodHead.Include("TimePeriod").Include("TimePeriodHeadChild.TimePeriod")
                                   where h.TimePeriodHeadId == timePeriodHeadId &&
                                   h.ActorCompanyId == actorCompanyId &&
                                   h.State == (int)SoeEntityState.Active
                                   select h).FirstOrDefault();

            if (head != null)
            {
                // Head
                planningPeriodHead.TimePeriodHeadId = head.TimePeriodHeadId;
                planningPeriodHead.Name = head.Name;

                // Parent period (only for current date)
                TimePeriod parentPeriod = head.TimePeriod.FirstOrDefault(p => p.StartDate <= date && p.StopDate >= date && p.State == (int)SoeEntityState.Active);
                if (parentPeriod != null)
                {
                    planningPeriodHead.ParentPeriods = new List<PlanningPeriod>
                    {
                        new PlanningPeriod()
                        {
                            TimePeriodId = parentPeriod.TimePeriodId,
                            Name = parentPeriod.Name,
                            StartDate = parentPeriod.StartDate,
                            StopDate = parentPeriod.StopDate
                        }
                    };

                    // Child head
                    if (head.TimePeriodHeadChild != null)
                    {
                        planningPeriodHead.ChildId = head.TimePeriodHeadChild.TimePeriodHeadId;
                        planningPeriodHead.ChildName = head.TimePeriodHeadChild.Name;

                        // Child periods (only within parent period)
                        planningPeriodHead.ChildPeriods = new List<PlanningPeriod>();
                        foreach (TimePeriod childPeriod in head.TimePeriodHeadChild.TimePeriod.Where(p => p.StartDate >= parentPeriod.StartDate && p.StopDate <= parentPeriod.StopDate && p.State == (int)SoeEntityState.Active).ToList())
                        {
                            planningPeriodHead.ChildPeriods.Add(new PlanningPeriod()
                            {
                                TimePeriodId = childPeriod.TimePeriodId,
                                Name = childPeriod.Name,
                                StartDate = childPeriod.StartDate,
                                StopDate = childPeriod.StopDate
                            });
                        }
                    }
                }
            }

            return planningPeriodHead;
        }

        public PeriodCalculationResultDTO GetPeriodsForCalculation(TermGroup_TimePeriodType periodType, DateTime dateFrom, DateTime dateTo, int actorCompanyId, bool onlyParent = false, bool includePeriodsWithoutChildren = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.TimePeriod.NoTracking();
            return GetPeriodsForCalculation(entities, periodType, dateFrom, dateTo, actorCompanyId, onlyParent, includePeriodsWithoutChildren);
        }
        public PeriodCalculationResultDTO GetPeriodsForCalculation(CompEntities entites, TermGroup_TimePeriodType periodType, DateTime dateFrom, DateTime dateTo, int actorCompanyId, bool onlyParent = false, bool includePeriodsWithoutChildren = false)
        {
            PeriodCalculationResultDTO periodCalculationResultDTO = new PeriodCalculationResultDTO();

            List<int> ids;
            var timePeriodHeads = (from th in entites.TimePeriodHead
                                   where th.ActorCompanyId == actorCompanyId &&
                                   th.TimePeriodType == (int)periodType &&
                                   th.State == (int)SoeEntityState.Active &&
                                   (th.ChildId.HasValue || includePeriodsWithoutChildren)
                                   select th).ToList();

            if (!timePeriodHeads.Any())
                return periodCalculationResultDTO;

            if (onlyParent)
                ids = timePeriodHeads.Select(w => w.TimePeriodHeadId).ToList();
            else
                ids = timePeriodHeads.Where(w=> w.ChildId.HasValue).Select(s => s.ChildId.Value).ToList();

            var periods = (from tp in entites.TimePeriod
                        .Include("TimePeriodHead")
                           where
                           ((!onlyParent && tp.StopDate <= dateTo && tp.StopDate >= dateFrom) ||
                           (onlyParent && tp.StopDate >= dateTo && tp.StartDate <= dateFrom)) &&
                           ids.Contains(tp.TimePeriodHead.TimePeriodHeadId) &&
                           tp.State == (int)SoeEntityState.Active &&
                           tp.TimePeriodHead.State == (int)SoeEntityState.Active
                           select tp).ToList();

            if (includePeriodsWithoutChildren && !periods.Any())
            {
                ids = timePeriodHeads.Select(w => w.TimePeriodHeadId).ToList();
                periods = (from tp in entites.TimePeriod
                        .Include("TimePeriodHead")
                           where tp.StopDate <= dateTo && tp.StopDate >= dateFrom &&
                           ids.Contains(tp.TimePeriodHead.TimePeriodHeadId) &&
                           tp.State == (int)SoeEntityState.Active &&
                           tp.TimePeriodHead.State == (int)SoeEntityState.Active
                           select tp).ToList();
            }

            if (periods.Any())
            {
                periodCalculationResultDTO.CurrentPeriod = periods.FirstOrDefault()?.TimePeriodHead?.Name ?? "";
                periodCalculationResultDTO.ParentPeriod = timePeriodHeads.FirstOrDefault(w=> w.ChildId.HasValue)?.Name ?? "";
                periodCalculationResultDTO.Periods.AddRange(periods.ToDTOs());
            }

            return periodCalculationResultDTO;
        }
        public Dictionary<int, string> PayrollProductFromTransactionGroupDict(List<TimePayrollTransaction> transactions)
        {
            Dictionary<int, string> dict = new Dictionary<int, string>();
            foreach (var items in transactions.GroupBy(g => g.EmployeeId))
            {
                var employeeId = items.First().EmployeeId;
                StringBuilder periodStr = new StringBuilder();

                foreach (var item in items.GroupBy(g => g.ProductId))
                {
                    var product = item.First().PayrollProduct.NumberAndName;
                    var sum = item.Sum(w => w.Quantity);
                    if (periodStr.Length != 0)
                        periodStr.Append(", ");

                    periodStr.Append(product + " (" + CalendarUtility.FormatTimeSpan(sum) + ")");
                }
                if (periodStr.Length != 0)
                    dict.Add(employeeId, periodStr.ToString());
            }

            return dict;
        }
        #endregion
    }
}

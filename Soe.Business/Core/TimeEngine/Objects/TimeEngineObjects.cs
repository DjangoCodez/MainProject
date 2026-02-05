using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects.DataClasses;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace SoftOne.Soe.Business.Core.TimeEngine
{
    #region IO

    public class TimeEngineInputDTO
    {
        #region Async variables

        //TimeScheduleTemplateBlock
        private List<TimeScheduleTemplateBlock> asyncTemplateBlocks;
        internal List<TimeScheduleTemplateBlock> AsyncTemplateBlocks
        {
            get
            {
                if (asyncTemplateBlocks == null)
                    asyncTemplateBlocks = new List<TimeScheduleTemplateBlock>();
                return asyncTemplateBlocks;
            }
        }

        #endregion

        #region Methods

        public string GetProperties(object obj)
        {
            StringBuilder sb = new StringBuilder();

            foreach (var property in obj.GetProperties())
            {
                try
                {
                    object value = property.GetValue(obj);
                    if (value.IsList())
                    {
                        if (value is List<int> list)
                            sb.Append($"{property.Name}={list.ToCommaSeparated()};");
                        else if (value is List<decimal> dec)
                            sb.Append($"{property.Name}={dec.ToCommaSeparated()};");
                        else if (value is List<string> str)
                            sb.Append($"{property.Name}={str.ToCommaSeparated()};");
                        else if (value is List<DateTime> dt)
                            sb.Append($"{property.Name}={dt.ToCommaSeparated()};");
                        else
                            sb.Append($"{property.Name}={value.GetListCount()} items;");
                    }
                    else if (value.IsDictionary())
                    {
                        sb.Append($"{property.Name}={value.GetDictionaryCount()} pairs;");
                    }
                    else if (value is object && !property.PropertyType.IsValueType)
                    {
                        var pair = value.GetKeyValue();
                        if (pair.Key != null)
                            sb.Append($"{pair.Key}={pair.Value};");
                        else
                            sb.Append($"{property.Name}={value};");
                    }
                    else
                    {
                        sb.Append($"{property.Name}={value};");
                    }
                }
                catch (Exception)
                {
                    sb.Append($"{property.Name}=FAILED;");
                }
            }

            return sb.ToString();
        }
        public override string ToString()
        {
            return GetProperties(this);
        }
        public virtual int? GetIdCount()
        {
            return null;
        }
        public virtual int? GetIntervalCount()
        {
            return null;
        }

        #endregion
    }
    public class TimeEngineOutputDTO
    {
        public ActionResult Result { get; set; }
        public TimeEngineOutputDTO(bool success = true)
        {
            Result = new ActionResult(success);
        }
    }

    #endregion

    #region Internal classes

    internal class TimeEnginePeriod
    {
        public int EmployeeId { get; private set; }
        public List<TimeEngineDay> Days { get; private set; }

        public bool HasDays => !this.Dates.IsNullOrEmpty();
        public List<int> TimeBlockDateIds => this.Days.Select(i => i.TimeBlockDateId).ToList();
        public List<DateTime> Dates => this.Days.Select(day => day.Date).ToList();
        public List<DateTime> DeviationDates => this.Days.Where(day => day.EmployeeGroup != null && day.EmployeeGroup.AutogenTimeblocks).Select(i => i.Date).ToList();

        public TimeEnginePeriod(int employeeId, List<TimeEngineDay> days)
        {
            this.EmployeeId = employeeId;
            this.Days = days;
        }
    }
    internal class TimeEngineDay
    {
        #region Properties

        public Guid Key { get; set; }

        public TimeBlockDate TimeBlockDate { get; set; }
        public int EmployeeId
        {
            get
            {
                return this.TimeBlockDate?.EmployeeId ?? 0;
            }
        }
        public int TimeBlockDateId
        {
            get
            {
                return this.TimeBlockDate?.TimeBlockDateId ?? 0;
            }
        }
        public DateTime Date
        {
            get
            {
                return this.TimeBlockDate?.Date ?? CalendarUtility.DATETIME_DEFAULT;
            }
        }
        public bool IsNew
        {
            get
            {
                return this.TimeBlockDate?.IsNew ?? false;
            }
        }

        public int? TemplatePeriodId { get; set; }
        public bool IsScheduleZeroDay { get; set; }
        public bool HasAttestStateNoneInitial { get; set; }
        public int? StandardTimeDeviationCauseId { get; set; }

        public EmployeeGroup EmployeeGroup { get; set; }
        public TimeBlockDate CalculatedTimeBlockDate { get; set; }
        public EmployeeSchedule CalculatedEmployeeSchedule { get; set; }
        public List<TimeCodeTransaction> AdditionalTimeCodeTransactions { get; set; }
        public List<int> TimeStampEntryIds { get; set; }
        public List<TimeBlock> TimeBlocks { get; set; }

        #endregion

        #region Ctor

        public TimeEngineDay(
            int? templatePeriodId, 
            TimeBlockDate timeBlockDate,
            EmployeeGroup employeeGroup = null, 
            List<TimeStampEntry> timeStampEntries = null, 
            List<TimeCodeTransaction> additionalTimeCodeTransactions = null, 
            int? standardTimeDeviationCauseId = null, 
            bool isScheduleZeroDay = false
            )
        {
            this.Key = Guid.NewGuid();            
            this.TemplatePeriodId = templatePeriodId;
            this.StandardTimeDeviationCauseId = standardTimeDeviationCauseId;
            this.IsScheduleZeroDay = isScheduleZeroDay;
            this.EmployeeGroup = employeeGroup;
            this.TimeBlockDate = timeBlockDate;
            this.AdditionalTimeCodeTransactions = additionalTimeCodeTransactions?.Where(t => t.TimeBlockDate.Date == timeBlockDate.Date).ToList();
            this.TimeStampEntryIds = timeStampEntries?.Where(t => t.TimeBlockDate.Date == timeBlockDate.Date).Select(i => i.TimeStampEntryId).Distinct().ToList();
            this.TimeBlocks = new List<TimeBlock>();
        }

        #endregion
    }
    internal class TimeEngineRestoreDay
    {
        public int EmployeeId { get; private set; }
        public int? TemplatePeriodId { get; private set; }
        public DateTime Date { get; private set; }
        public bool ClearScheduledAbsence { get; private set; }

        public TimeEngineRestoreDay(int employeeId, int? templatePeriodId, DateTime date, bool clearScheduledAbsence)
        {
            this.EmployeeId = employeeId;
            this.TemplatePeriodId = templatePeriodId;
            this.Date = date;
            this.ClearScheduledAbsence = clearScheduledAbsence;
        }
    }
    internal class TimeEngineAbsenceDay
    {
        public int EmployeeId { get; set; }
        public DateTime Date { get; set; }
        public SoeTimeBlockDateDetailType Type { get; set; }
        public decimal? Ratio { get; set; }
        public int? TimeDeviationCauseId { get; set; }
        public int? SysPayrollTypeLevel3 { get; set; }
        public List<int> SysPayrollTypeLevel3sExisting { get; set; }

        public TimeEngineAbsenceDay(int employeeId, DateTime date, SoeTimeBlockDateDetailType type, decimal? ratio, int? timeDeviationCauseId)
        {
            this.EmployeeId = employeeId;
            this.Date = date;
            this.Type = type;
            this.Ratio = ratio;
            this.TimeDeviationCauseId = timeDeviationCauseId;
        }

        public void Update(int sysPayrollTypeLevel3, int? timeDeviationCauseId)
        {
            this.Type = SoeTimeBlockDateDetailType.Absence;
            this.SysPayrollTypeLevel3 = sysPayrollTypeLevel3;
            if (!this.TimeDeviationCauseId.HasValue && timeDeviationCauseId.HasValue)
                this.TimeDeviationCauseId = timeDeviationCauseId;
        }

        public void SetExisting(List<int> sysPayrollTypeLevel3sExisting)
        {
            this.SysPayrollTypeLevel3sExisting = sysPayrollTypeLevel3sExisting;
        }
    }
    internal class TimeEngineVacationYearEndEmployee
    {
        public Employee Employee { get; private set; }
        public XElement EmployeeElement { get; private set; }
        public int EmployeeId => this.Employee?.EmployeeId ?? 0;
        public Employment Employment { get; private set; }
        public VacationGroup VacationGroup { get; private set; }
        public SoeVacationYearEndType Type { get; private set; }
        public DateTime Date { get; private set; }
        public TimePeriod TimePeriod { get; private set; }

        public bool HasValidVacationGroup { get; private set; }
        public void SetValidVacationGroup() => this.HasValidVacationGroup = true;

        public ActionResult Result { get; private set; } = new ActionResult(true);
        public void SetResult(ActionResult result)
        {
            this.Result = result;
            if (this.Result.Success)
                this.SetSucceeded();
            else
                this.SetFailed();
        }

        public TermGroup_VacationYearEndStatus Status { get; private set; }
        public void SetSucceeded() => this.Status = TermGroup_VacationYearEndStatus.Succeded;
        public void SetFailed() => this.Status = TermGroup_VacationYearEndStatus.Failed;
        public bool HasSucceeded => this.Status == TermGroup_VacationYearEndStatus.Succeded;
        public bool HasFailed => this.Status == TermGroup_VacationYearEndStatus.Failed;

        public DateTime VacationGroupFromDate { get; private set; }
        public DateTime VacationGroupToDate { get; private set; }       
        public (DateTime From, DateTime To) GetAbsenceRecalculationDates() => (VacationGroupToDate.AddDays(1), VacationGroupToDate.AddYears(1));

        public EmployeeVacationSE NewEmployeeVacationSE { get; private set; }
        public EmployeeVacationSE OldEmployeeVacationSE { get; private set; }
        public void SetEmployeeVacationSE(EmployeeVacationSE newVacationSE, EmployeeVacationSE oldVacationSE)
        {
            if (newVacationSE == null)
                return;
            this.NewEmployeeVacationSE = newVacationSE;
            this.OldEmployeeVacationSE = oldVacationSE;
            this.Track(newVacationSE, oldVacationSE);
        }

        public VacationYearEndRow VacationYearEndRow { get; private set; }
        public void SetVacationYearEndRow(VacationYearEndRow vacationYearEndRow)
        {
            if (vacationYearEndRow == null)
                return;
            this.VacationYearEndRow = vacationYearEndRow;
            this.Track(vacationYearEndRow);
        }

        public List<EmployeeFactor> EmployeeFactors { get; private set; } = new List<EmployeeFactor>();
        public void AddEmployeeFactor(EmployeeFactor employeeFactor)
        {
            if (employeeFactor == null)
                return;
            this.EmployeeFactors.Add(employeeFactor);
            this.Track(employeeFactor);
        }

        public List<TimeBlockDate> TimeBlockDates { get; private set; } = new List<TimeBlockDate>();
        public TimeBlockDate AddTimeBlockDate(TimeBlockDate timeBlockDate)
        {
            this.TimeBlockDates.Add(timeBlockDate);
            if (timeBlockDate.TimeBlockDateId == 0)
                Track(timeBlockDate);
            return timeBlockDate;
        }

        public List<TimeCodeTransaction> TimeCodeTransactions { get; private set; } = new List<TimeCodeTransaction>();
        public void AddTimeCodeTransactions(params TimeCodeTransaction[] timeCodeTransactions)
        {
            this.TimeCodeTransactions.AddRange(timeCodeTransactions);
            this.Track(timeCodeTransactions);
        }

        public List<TimePayrollTransaction> TimePayrollTransactions { get; private set; } = new List<TimePayrollTransaction>();
        public void AddTimePayrollTransactions(params TimePayrollTransaction[] timePayrollTransactions)
        {
            this.TimePayrollTransactions.AddRange(timePayrollTransactions);
            this.Track(timePayrollTransactions);
            this.Track(timePayrollTransactions.Where(t => t.TimePayrollTransactionExtended != null).Select(t => t.TimePayrollTransactionExtended).ToArray());
        }

        public List<EntityObject> Tracked { get; private set; } = new List<EntityObject>();
        private void Track(params EntityObject[] entitys) => Tracked.AddRange(entitys.Where(entity => entity != null));

        public TimeEngineVacationYearEndEmployee(Employee employee, Employment employment, XElement employeeElement, VacationGroup vacationGroup, SoeVacationYearEndType type, DateTime date, DateTime vacationGroupFromDate, DateTime vacationGroupToDate, TimePeriod timePeriod, VacationYearEndRow vacationYearEndRow = null)
        {
            this.Status = TermGroup_VacationYearEndStatus.Ongoing;
            this.Employee = employee;
            this.EmployeeElement = employeeElement;
            this.Employment = employment;
            this.VacationGroup = vacationGroup;
            this.Type = type;
            this.Date = date;
            this.VacationGroupFromDate = vacationGroupFromDate;
            this.VacationGroupToDate = vacationGroupToDate;
            this.TimePeriod = timePeriod;
            this.VacationYearEndRow = vacationYearEndRow;
        }
    }
    internal class TimeEngineVacationYearEndDayToPay
    {
        public decimal Days { get; private set; }
        public TermGroup_SysPayrollType Level2 { get; private set; }
        public TermGroup_SysPayrollType Level3 { get; private set; }
        public decimal Amount { get; private set; }

        public TimeEngineVacationYearEndDayToPay(TermGroup_SysPayrollType level2, TermGroup_SysPayrollType level3, decimal days, decimal amount = 0)
        {
            this.Days = days;
            this.Level2 = level2;
            this.Level3 = level3;
            this.Amount = amount;
        }
    }    
    internal class TimeEngineBlock
    {
        #region Properties

        public Guid Key { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime StopTime { get; set; }
        public bool IsBreak { get; set; }

        #endregion

        #region Ctor

        public TimeEngineBlock(Guid key, DateTime startTime, DateTime stopTime, bool isBreak = false)
        {
            this.Key = key;
            this.StartTime = startTime;
            this.StopTime = stopTime;
            this.IsBreak = isBreak;
        }

        #endregion

        #region Public methods

        public TimeScheduleTemplateBlock FindOriginal(List<TimeScheduleTemplateBlock> templateBlocks)
        {
            return templateBlocks?.FirstOrDefault(i => i.Guid == this.Key);
        }

        #endregion

        #region Static methods

        public static List<TimeEngineBlock> Create(List<TimeScheduleTemplateBlock> templateBlocks)
        {
            List<TimeEngineBlock> workBlocks = new List<TimeEngineBlock>();
            foreach (var tuple in templateBlocks.GetWork(skipZero: true))
            {
                workBlocks.Add(new TimeEngineBlock(tuple.Guid, tuple.StartTime, tuple.StopTime, tuple.IsBreak));
            }
            workBlocks = RemoveOverlapping(workBlocks);

            List<TimeEngineBlock> breakBlocks = new List<TimeEngineBlock>();
            foreach (var tuple in templateBlocks.GetBreaks(skipZero: true))
            {
                breakBlocks.Add(new TimeEngineBlock(tuple.Guid, tuple.StartTime, tuple.StopTime, tuple.IsBreak));
            }
            breakBlocks = RemoveOverlapping(breakBlocks);

            return Merge(workBlocks, breakBlocks);
        }

        private static List<TimeEngineBlock> RemoveOverlapping(List<TimeEngineBlock> blocks)
        {
            List<TimeEngineBlock> validBlocks = new List<TimeEngineBlock>();

            foreach (TimeEngineBlock block in blocks.OrderBy(i => i.StartTime).ThenBy(i => i.StopTime))
            {
                if (validBlocks.Any())
                {
                    TimeEngineBlock validBlock = validBlocks.OrderBy(i => i.StopTime).LastOrDefault();
                    if (validBlock == null)
                        continue;

                    DateTime currentStopTime = validBlock.StopTime;

                    //Check if current item is completely overlapped by an earlier item
                    if (currentStopTime >= block.StopTime)
                        continue;

                    if (currentStopTime > block.StartTime)
                        block.StartTime = currentStopTime;
                }

                validBlocks.Add(block);
            }

            return validBlocks;
        }

        private static List<TimeEngineBlock> Merge(List<TimeEngineBlock> workBlocks, List<TimeEngineBlock> breakItems)
        {
            List<TimeEngineBlock> validBlocks = new List<TimeEngineBlock>();

            if (workBlocks == null || breakItems == null)
                return validBlocks;

            #region Add breaks

            validBlocks.AddRange(breakItems);

            #endregion

            #region Add work

            //Loop until all work are valid. Re-start when work are splitted
            while (workBlocks.Any())
            {
                bool workValid = true;
                TimeEngineBlock workBock = workBlocks.OrderBy(i => i.StartTime).First();

                foreach (TimeEngineBlock breakItem in breakItems.OrderBy(i => i.StartTime))
                {
                    //Check if work is overlapped by break
                    if (CalendarUtility.IsNewOverlappedByCurrent(workBock.StartTime, workBock.StopTime, breakItem.StartTime, breakItem.StopTime))
                    {
                        //Remove work and re-start
                        workBlocks.Remove(workBock);
                        workValid = false;
                        break;
                    }

                    List<TimeEngineBlock> splittedWorkItems = Split(workBock, breakItem);
                    if (splittedWorkItems.Any())
                    {
                        //Replace work and re-start
                        workBlocks.Remove(workBock);
                        workBlocks.AddRange(splittedWorkItems);
                        workValid = false;
                        break;
                    }
                }

                if (workValid)
                {
                    //Move to validated collection
                    validBlocks.Add(workBock);
                    workBlocks.Remove(workBock);
                }
            }

            #endregion

            return validBlocks.OrderBy(i => i.StartTime).ToList();
        }

        private static List<TimeEngineBlock> Split(TimeEngineBlock workBlock, TimeEngineBlock breakBlock)
        {
            List<TimeEngineBlock> splittedItems = new List<TimeEngineBlock>();

            if (workBlock == null || breakBlock == null || workBlock.StartTime == workBlock.StopTime || breakBlock.StartTime == breakBlock.StopTime)
                return splittedItems;

            if (CalendarUtility.IsCurrentOverlappedByNew(workBlock.StartTime, workBlock.StopTime, breakBlock.StartTime, breakBlock.StopTime))
                BreakOverlappedByWork();
            else if (CalendarUtility.IsNewStopInCurrent(workBlock.StartTime, workBlock.StopTime, breakBlock.StartTime, breakBlock.StopTime))
                PlaceNewTimeBlockBeforeCurrent();
            else if (CalendarUtility.IsNewStartInCurrent(workBlock.StartTime, workBlock.StopTime, breakBlock.StartTime, breakBlock.StopTime))
                PlaceNewTimeBlockAfterCurrent();

            return splittedItems;

            /*
            *Work:   xxxxxxxxxx
            *Break:     xxxx
            */
            void BreakOverlappedByWork()
            {
                //Split workItem
                if (workBlock.StartTime < breakBlock.StartTime)
                    splittedItems.Add(CreateTimeEngineBlock(workBlock.Key, workBlock.StartTime, breakBlock.StartTime));
                if (breakBlock.StopTime < workBlock.StopTime)
                    splittedItems.Add(CreateTimeEngineBlock(workBlock.Key, breakBlock.StopTime, workBlock.StopTime));
            }

            /*
            *Work:   xxxxxxxxxx
            *Break:         xxxxxx
            */
            void PlaceNewTimeBlockBeforeCurrent()
            {
                //Change stop
                if (workBlock.StartTime < breakBlock.StartTime)
                    splittedItems.Add(CreateTimeEngineBlock(workBlock.Key, workBlock.StartTime, breakBlock.StartTime));
            }

            /*
            *Work:          xxxxxx
            *Break:  xxxxxxxxxx
            */
            void PlaceNewTimeBlockAfterCurrent()
            {
                //Change start
                if (breakBlock.StopTime < workBlock.StopTime)
                    splittedItems.Add(CreateTimeEngineBlock(workBlock.Key, breakBlock.StopTime, workBlock.StopTime));
            }

            TimeEngineBlock CreateTimeEngineBlock(Guid key, DateTime startTime, DateTime stopTime)
            {
                return new TimeEngineBlock(key, startTime, stopTime);
            }
        }

        #endregion
    }
    internal class TimeEnginePayrollProduct
    {
        public int PayrollProductId { get; set; }
        public decimal Quantity { get; set; }
        public int? TimePeriodId { get; set; }
        public int? PlanningPeriodCalculationId { get; set; }
    }
    internal class TimeEngineTemplateRepository
    {
        #region Variables

        private readonly List<TimeEngineTemplate> templates;

        #endregion

        #region Ctor

        public TimeEngineTemplateRepository()
        {
            this.templates = new List<TimeEngineTemplate>();
        }

        #endregion

        #region Public methods

        public TimeEngineTemplate GetTemplate(TimeEngineTemplateIdentity identity, SoeTimeEngineTemplateType type, bool usePayrollOrAbsenceRules, Func<EntityObject, bool> canLoadReferences)
        {
            List<TimeEngineTemplate> matchingTemplates = GetMatchingTemplates(identity, type, usePayrollOrAbsenceRules);
            if (matchingTemplates.IsNullOrEmpty())
                return null;

            foreach (TimeEngineTemplate matchingTemplate in matchingTemplates)
            {
                if (IsTimeEngineTemplateIdentityMatching(identity, matchingTemplate.Identity, type, canLoadReferences))
                    return matchingTemplate;
            }

            return null;
        }

        public void AddTemplate(TimeEngineTemplate template, Func<EntityObject, bool> canLoadReferences)
        {
            if (template?.Identity == null || template.Identity.IsUniqueDay || canLoadReferences == null)
                return;

            if (!template.Identity.ScheduleBlocks.IsNullOrEmpty())
            {
                foreach (var scheduleBlock in template.Identity.ScheduleBlocks)
                {
                    if (!scheduleBlock.AccountInternal.IsLoaded && canLoadReferences(scheduleBlock))
                        scheduleBlock.AccountInternal.Load();
                }
            }
            if (!template.Identity.TimeBlocks.IsNullOrEmpty())
            {
                foreach (var timeBlock in template.Identity.TimeBlocks)
                {
                    if (!timeBlock.AccountInternal.IsLoaded && canLoadReferences(timeBlock))
                        timeBlock.AccountInternal.Load();
                }
            }

            this.templates.Add(template);
        }

        public void UpdateOutcome(TimeEngineTemplate template)
        {
            if (template?.Identity?.TimeBlocks == null)
                return;

            TimeEngineTemplate dependencyTemplate = this.templates.FirstOrDefault(i => i.Identity.Guid == template.Identity.DependencyGuid);
            if (dependencyTemplate != null)
                dependencyTemplate.Outcome.TimeBlocks = template.Identity.TimeBlocks;
        }

        #endregion

        #region Private methods

        private List<TimeEngineTemplate> GetMatchingTemplates(TimeEngineTemplateIdentity identity, SoeTimeEngineTemplateType type, bool usePayrollOrAbsenceRules)
        {
            if (this.templates.IsNullOrEmpty())
                return null;
            if (identity == null)
                return null;

            switch (type)
            {
                case SoeTimeEngineTemplateType.TimeBlocksFromTemplate:
                    #region TimeBlocksFromTemplate

                    if (identity.EmployeeGroup == null || identity.ScheduleBlocks == null)
                        return null;

                    return (from t in this.templates
                            where t.Identity != null &&
                            t.Identity.TimeBlockDate.Date != identity.TimeBlockDate.Date &&
                            (t.Identity.TimeScheduleTemplatePeriodId == identity.TimeScheduleTemplatePeriodId) &&
                            (t.Identity.EmployeeGroup != null && t.Identity.EmployeeGroup.EmployeeGroupId == identity.EmployeeGroup.EmployeeGroupId) &&
                            (t.Identity.ScheduleBlocks != null && t.Identity.ScheduleBlocks.Count == identity.ScheduleBlocks.Count) &&
                            (t.Outcome.TimeBlocks != null)
                            select t).ToList();

                #endregion
                case SoeTimeEngineTemplateType.TransactionsFromTimeBlocks:
                    #region TransactionsFromTimeBlocks

                    if (identity.EmployeeGroup == null || identity.TimeBlocks == null || !identity.StandardTimeDeviationCauseId.HasValue) //DayType can be null
                        return null;

                    return (from t in this.templates
                            where t.Identity != null &&
                            (!usePayrollOrAbsenceRules || !t.Identity.IsAnyTimeBlockAbsence) &&
                            t.Identity.TimeBlockDate.Date != identity.TimeBlockDate.Date &&
                            (t.Identity.StandardTimeDeviationCauseId.HasValue && t.Identity.StandardTimeDeviationCauseId.Value == identity.StandardTimeDeviationCauseId.Value) &&
                            (t.Identity.EmployeeGroup != null && t.Identity.EmployeeGroup.EmployeeGroupId == identity.EmployeeGroup.EmployeeGroupId) &&
                            (t.Identity.TimeBlocks != null && t.Identity.TimeBlocks.Count == identity.TimeBlocks.Count) &&
                            (t.Identity.DayType != null && t.Identity.DayType.DayTypeId == identity.DayType.DayTypeId) &&
                            (t.Outcome.TimePayrollTransactions != null) &&
                            (t.Outcome.TimeCodeTransactions != null || !t.Outcome.TimeCodeTransactions.Any(t => t.IsTurnedByTimeCodeRanking))
                            select t).ToList();

                    #endregion
            }

            return null;
        }

        private bool IsTimeEngineTemplateIdentityMatching(TimeEngineTemplateIdentity identity, TimeEngineTemplateIdentity matchingIdentity, SoeTimeEngineTemplateType type, Func<EntityObject, bool> canLoadReferences)
        {
            if (identity == null || matchingIdentity == null)
                return false;

            switch (type)
            {
                case SoeTimeEngineTemplateType.TimeBlocksFromTemplate:
                    #region TimeBlocksFromTemplate

                    if (identity.ScheduleBlocks == null || matchingIdentity.ScheduleBlocks == null)
                        return false;

                    //All TimeScheduleTemplateBlock's must match
                    List<TimeScheduleTemplateBlock> scheduleBlocks = identity.ScheduleBlocks.Where(i => i.State == (int)SoeEntityState.Active).OrderBy(i => i.StartTime).ToList();
                    foreach (TimeScheduleTemplateBlock scheduleBlock in scheduleBlocks)
                    {
                        if (!scheduleBlock.AccountInternal.IsLoaded && scheduleBlock.AccountInternal.IsNullOrEmpty() && canLoadReferences(scheduleBlock))
                            scheduleBlock.AccountInternal.Load();

                        if (!IsTimeScheduleTemplateBlockMatching(scheduleBlock, matchingIdentity.ScheduleBlocks))
                            return false;
                    }

                    #endregion
                    break;
                case SoeTimeEngineTemplateType.TransactionsFromTimeBlocks:
                    #region TransactionsFromTimeBlocks

                    if (identity.TimeBlocks == null || matchingIdentity.TimeBlocks == null)
                        return false;

                    //All TimeBlock's must match
                    List<TimeBlock> timeBlocks = identity.TimeBlocks.Where(i => i.State == (int)SoeEntityState.Active).OrderBy(i => i.StartTime).ToList();
                    foreach (TimeBlock timeBlock in timeBlocks)
                    {
                        if (!timeBlock.AccountInternal.IsLoaded && timeBlock.AccountInternal.IsNullOrEmpty() && canLoadReferences(timeBlock))
                            timeBlock.AccountInternal.Load();

                        if (!IsTimeBlockMatching(timeBlock, matchingIdentity.TimeBlocks))
                            return false;
                    }

                    #endregion
                    break;
            }

            return true;
        }

        private bool IsTimeScheduleTemplateBlockMatching(TimeScheduleTemplateBlock templateBlock, List<TimeScheduleTemplateBlock> templateBlocks)
        {
            if (templateBlock == null || templateBlocks == null)
                return false;

            return (from tb in templateBlocks
                    where tb.TimeScheduleTemplatePeriodId == templateBlock.TimeScheduleTemplatePeriodId &&
                    tb.BreakType == templateBlock.BreakType &&
                    tb.StartTime == templateBlock.StartTime &&
                    tb.StopTime == templateBlock.StopTime &&
                    tb.TimeCodeId == templateBlock.TimeCodeId &&
                    tb.TimeHalfdayId == templateBlock.TimeHalfdayId &&
                    tb.IsPreliminary == templateBlock.IsPreliminary &&
                    tb.State == templateBlock.State &&
                    IsAccountInternalsMatching(tb.AccountInternal, templateBlock.AccountInternal)
                    select tb).Any();
        }

        private bool IsTimeBlockMatching(TimeBlock timeBlock, List<TimeBlock> timeBlocks)
        {
            if (timeBlock == null || timeBlocks == null)
                return false;

            return (from tb in timeBlocks
                    where tb.StartTime == timeBlock.StartTime &&
                    tb.StopTime == timeBlock.StopTime &&
                    tb.IsBreak == timeBlock.IsBreak &&
                    tb.IsPreliminary == timeBlock.IsPreliminary &&
                    tb.CalculatedShiftTypeId.Equals(timeBlock.CalculatedShiftTypeId) &&
                    tb.CalculatedTimeScheduleTypeId.Equals(timeBlock.CalculatedTimeScheduleTypeId) &&
                    tb.CalculatedTimeScheduleTypeIdFromShift.Equals(timeBlock.CalculatedTimeScheduleTypeIdFromShift) &&
                    tb.CalculatedTimeScheduleTypeIdFromShiftType.Equals(timeBlock.CalculatedTimeScheduleTypeIdFromShiftType) &&
                    tb.CalculatedTimeScheduleTypeIdsFromEmployee.AreEqualTo(timeBlock.CalculatedTimeScheduleTypeIdsFromEmployee) &&
                    tb.CalculatedTimeScheduleTypeIdsFromTimeStamp.AreEqualTo(timeBlock.CalculatedTimeScheduleTypeIdsFromTimeStamp) &&
                    tb.CalculatedTimeScheduleTypeIdsFromTimeLeisureCodes.AreEqualTo(timeBlock.CalculatedTimeScheduleTypeIdsFromTimeLeisureCodes) &&
                    tb.State == timeBlock.State &&
                    IsTimeDeviationCauseMatching(tb.TimeDeviationCauseStart, tb.TimeDeviationCauseStartId, timeBlock.TimeDeviationCauseStart, timeBlock.TimeDeviationCauseStartId) &&
                    IsAccountInternalsMatching(tb.AccountInternal, timeBlock.AccountInternal)
                    select tb).Any();
        }

        private bool IsTimeDeviationCauseMatching(TimeDeviationCause deviationCauses1, int? deviationCauseIds1, TimeDeviationCause deviationCauses2, int? deviationCauseIds2)
        {
            if (deviationCauses1 != null && deviationCauses2 != null && deviationCauses1.TimeDeviationCauseId != deviationCauses2.TimeDeviationCauseId)
                return false;
            if (deviationCauseIds1.HasValue && deviationCauseIds2.HasValue && deviationCauseIds1.Value != deviationCauseIds2.Value)
                return false;
            if (deviationCauses1 != null && deviationCauses2 == null)
                return false;
            if (deviationCauses1 == null && deviationCauses2 != null)
                return false;
            if (deviationCauseIds1.HasValue && !deviationCauseIds2.HasValue)
                return false;
            if (!deviationCauseIds1.HasValue && deviationCauseIds2.HasValue)
                return false;
            return true;
        }

        private bool IsAccountInternalsMatching(IEnumerable<AccountInternal> accountInternals1, IEnumerable<AccountInternal> accountInternals2)
        {
            //Examine if one or both is null
            if (accountInternals1 == null && accountInternals2 == null)
                return true;
            else if (accountInternals1 == null || accountInternals2 == null)
                return false;

            //Compare count
            if (accountInternals1.Count() != accountInternals2.Count())
                return false;

            //Match each AccountInternal
            foreach (AccountInternal accountInternal1 in accountInternals1)
            {
                if (!accountInternals2.Any(i => i.AccountId == accountInternal1.AccountId))
                    return false;
            }

            return true;
        }

        #endregion
    }
    internal class TimeEngineTemplate
    {
        #region Variables

        public TimeEngineTemplateIdentity Identity { get; set; }
        public TimeEngineTemplateOutcome Outcome { get; set; }

        public EmployeeGroup EmployeeGroup
        {
            get
            {
                return Identity?.EmployeeGroup;
            }
        }
        public int EmployeeGroupId
        {
            get
            {
                return Identity?.EmployeeGroup?.EmployeeGroupId ?? 0;
            }
        }
        public Employee Employee
        {
            get
            {
                return Identity?.Employee;
            }
        }
        public int EmployeeId
        {
            get
            {
                return Identity?.Employee?.EmployeeId ?? 0;
            }
        }
        public TimeBlockDate TimeBlockDate
        {
            get
            {
                return Identity?.TimeBlockDate;
            }
        }
        public DateTime Date
        {
            get
            {
                return Identity?.TimeBlockDate?.Date ?? CalendarUtility.DATETIME_DEFAULT;
            }
        }
        public int TimeBlockDateId
        {
            get
            {
                return Identity?.TimeBlockDate?.TimeBlockDateId ?? 0;
            }
        }
        public int? DayTypeId
        {
            get
            {
                return Identity?.DayType?.DayTypeId;
            }
        }

        #endregion

        #region Ctor

        public TimeEngineTemplate(TimeEngineTemplateIdentity identity)
        {
            this.Identity = identity;
            this.Outcome = new TimeEngineTemplateOutcome();
        }

        #endregion
    }
    internal class TimeEngineTemplateIdentity
    {
        #region Variables

        public Guid Guid { get; set; }
        public Guid? DependencyGuid { get; set; }
        public Employee Employee { get; set; }
        public EmployeeGroup EmployeeGroup { get; set; }
        public List<TimeScheduleTemplateBlock> ScheduleBlocks { get; set; }
        public int TimeScheduleTemplateHeadId { get; set; }
        public int? TimeScheduleTemplatePeriodId { get; set; }
        public DayType DayType { get; set; }
        public TimeBlockDate TimeBlockDate { get; set; }
        public List<TimeBlock> TimeBlocks { get; set; }
        public bool IsAnyTimeBlockAbsence { get; set; }
        public int? StandardTimeDeviationCauseId { get; set; }
        public bool IsUniqueDay { get; set; }

        #endregion

        #region Ctor

        public static TimeEngineTemplateIdentity CreateIdentity(Employee employee, EmployeeGroup employeeGroup, TimeBlockDate timeBlockDate, int? templatePeriodId = null, int? standardTimeDeviationCauseId = null, List<TimeScheduleTemplateBlock> scheduleBlocks = null, List<TimeBlock> timeBlocks = null, DayType dayType = null, bool isAnyTimeBlockAbsence = false, Guid? dependencyGuid = null)
        {
            return new TimeEngineTemplateIdentity()
            {
                Guid = Guid.NewGuid(),
                DependencyGuid = dependencyGuid,
                Employee = employee,
                EmployeeGroup = employeeGroup,
                TimeBlockDate = timeBlockDate,
                DayType = dayType,
                TimeScheduleTemplatePeriodId = templatePeriodId,
                TimeScheduleTemplateHeadId = 0,
                ScheduleBlocks = scheduleBlocks,
                TimeBlocks = timeBlocks,
                IsAnyTimeBlockAbsence = isAnyTimeBlockAbsence,
                StandardTimeDeviationCauseId = standardTimeDeviationCauseId,
                IsUniqueDay = false,
            };
        }

        #endregion
    }
    internal class TimeEngineTemplateOutcome
    {
        #region Variables

        public List<TimeAbsenceRuleHead> TimeAbsenceRules;
        public List<TimeBlock> TimeBlocks;
        public List<TimeCodeTransaction> TimeCodeTransactions;
        public List<TimeInvoiceTransaction> TimeInvoiceTransactions;
        public List<TimePayrollTransaction> TimePayrollTransactions;
        public List<TimePayrollScheduleTransaction> TimePayrollScheduleTransactions;

        public bool UseTimeAbsenceRules
        {
            get
            {
                return !TimeAbsenceRules.IsNullOrEmpty();
            }
        }
        public bool UseStandby { get; set; }

        #endregion

        #region Ctor

        public TimeEngineTemplateOutcome()
        {
            this.TimeAbsenceRules = new List<TimeAbsenceRuleHead>();
            this.TimeCodeTransactions = new List<TimeCodeTransaction>();
            this.TimeInvoiceTransactions = new List<TimeInvoiceTransaction>();
            this.TimePayrollTransactions = new List<TimePayrollTransaction>();
            this.TimePayrollScheduleTransactions = new List<TimePayrollScheduleTransaction>();
        }

        #endregion

        #region Public methods

        public List<TimeCodeTransaction> GetTimeCodeTransactionsForExternalTransactions()
        {
            return this.TimeCodeTransactions
                .Where(i => !i.IsTurnedByTimeCodeRanking)           // Exclude turned TimeCodeTransactions by TimeCodeRanking
                .OrderByDescending(i => i.IsScheduleTransaction)    // Must handle ScheduleTransactions last
                .ThenBy(i => i.Start)
                .ThenBy(i => i.IsSickDuringIwhOrStandbyTransaction)
                .ToList();
        }

        #endregion
    }
    internal static class TimeEngineExtensions
    {
        #region CreateAbsenceDetailResultDTO

        public static CreateAbsenceDetailResultDTO GetEmployeeResult(this List<CreateAbsenceDetailResultDTO> l, int employeeId)
        {
            return l?.FirstOrDefault(c => c.EmployeeId == employeeId) ?? new CreateAbsenceDetailResultDTO(employeeId);
        }

        public static void SetDayResult(this CreateAbsenceDetailResultDTO e, bool succeeded, params DateTime[] dates)
        {
            if (e?.DateResult == null || dates.IsNullOrEmpty())
                return;

            foreach (DateTime date in dates)
            {
                if (!e.DateResult.ContainsKey(date))
                    e.DateResult.Add(date, succeeded);
                else
                    e.DateResult[date] = succeeded;
            }
        }

        public static bool HasAnyChanges(this List<CreateAbsenceDetailResultDTO> l)
        {
            foreach (CreateAbsenceDetailResultDTO e in l)
            {
                if (e.DateResult.Any(r => r.Value))
                    return true;
            }
            return false;
        }

        public static Dictionary<int, int> GetFailedDaysByEmployee(this List<CreateAbsenceDetailResultDTO> l)
        {
            Dictionary<int, int> result = new Dictionary<int, int>();
            foreach (CreateAbsenceDetailResultDTO e in l)
            {
                int failedDays = e.DateResult.Count(r => !r.Value);
                if (failedDays > 0)
                    result.Add(e.EmployeeId, failedDays);
            }
            return result;
        }

        #endregion

        #region RecalculateDayBase

        public static List<DateTime> GetDates<T>(this IEnumerable<T> l) where T : RecalculateDayBase
        {
            return l?.Select(i => i.Date).Distinct().ToList() ?? new List<DateTime>();
        }

        #endregion

        #region RestoreAbsenceDayDTO

        public static List<int> GetSysPayrollTypeLevel3s(this List<RestoreAbsenceDay> l)
        {
            List<int> sysPayrollTypeLevel3s = new List<int>();
            if (!l.IsNullOrEmpty())
            {
                foreach (RestoreAbsenceDay e in l)
                {
                    foreach (int sysPayrollTypeLevel3 in e.SysPayrollTypeLevel3s)
                    {
                        if (!sysPayrollTypeLevel3s.Contains(sysPayrollTypeLevel3))
                            sysPayrollTypeLevel3s.Add(sysPayrollTypeLevel3);
                    }
                }
            }
            return sysPayrollTypeLevel3s;
        }
        public static List<DateTime> GetDates(this List<RestoreAbsenceDay> l, int sysPayrollTypeLevel3)
        {
            return l?.Where(e => e.SysPayrollTypeLevel3s != null && e.SysPayrollTypeLevel3s.Contains(sysPayrollTypeLevel3)).Select(i => i.Date).Distinct().OrderBy(d => d.Date).ToList() ?? new List<DateTime>();
        }

        #endregion

        #region TimeEngineTemplate

        public static List<TimePayrollTransaction> GetTimePayrollTransactions(this TimeEngineTemplate e, TimeCodeTransaction tct)
        {
            if (tct == null)
                return null;
            return e?.Outcome?.TimePayrollTransactions.Where(t => t.TimeCodeTransaction != null && t.TimeCodeTransaction.GuidTimeBlock == tct.GuidTimeBlock).ToList();
        }

        public static TimeAbsenceRuleHead GetTimeAbsenceRule(this TimeEngineTemplate e, TermGroup_TimeAbsenceRuleType type)
        {
            return e?.Outcome?.TimeAbsenceRules.FirstOrDefault(i => i.Type == (int)type);
        }

        public static bool HasAnyRuleExceptSickDuringIwhOrStandby(this TimeEngineTemplate e)
        {
            return e?.Outcome?.TimeAbsenceRules?.Any(i => !i.IsSickDuringIwhOrStandBy) ?? false;
        }

        public static bool HasValidIdentity(this TimeEngineTemplate e, bool requireEmployeeGroup = false)
        {
            return e?.Employee != null && e.TimeBlockDate != null && (!requireEmployeeGroup || e.EmployeeGroup != null);
        }

        #endregion

        #region TimeEngineDay

        public static List<TimeBlockDate> GetTimeBlockDates(this List<TimeEngineDay> l)
        {
            return l?.Where(e => e.TimeBlockDate != null).Select(d => d.TimeBlockDate).ToList() ?? new List<TimeBlockDate>();
        }

        public static List<int> GetTimeBlockDateIds(this List<TimeEngineDay> l, bool skipNew)
        {
            return l.GetTimeBlockDates().Where(e => !skipNew || !e.IsNew).Select(e => e.TimeBlockDateId).ToList();
        }

        public static List<TimeEngineDay> GetValidDays(this List<TimeEngineDay> l, Employee employee, List<EmployeeGroup> employeeGroups, out Dictionary<DateTime, bool> dateValidDict)
        {
            l = l?.Where(i => i.EmployeeId == employee.EmployeeId && i.TimeBlockDate != null).ToList() ?? new List<TimeEngineDay>();
            foreach (var e in l.Where(i => i.EmployeeGroup == null))
            {
                e.EmployeeGroup = employee.GetEmployeeGroup(e.Date, employeeGroups);
            }

            dateValidDict = new Dictionary<DateTime, bool>();
            foreach (var e in l)
            {
                if (!dateValidDict.ContainsKey(e.Date))
                    dateValidDict.Add(e.Date, e.EmployeeGroup != null);
            }

            return l;
        }

        public static List<TimeEngineDay> GetValidDays(this List<TimeEngineDay> l, Dictionary<DateTime, bool> dateValidDict)
        {
            return l?.Where(d => dateValidDict != null && dateValidDict.ContainsKey(d.Date) && dateValidDict[d.Date]).ToList() ?? new List<TimeEngineDay>();
        }

        public static List<DateTime> GetValidDates(this List<TimeEngineDay> l, Dictionary<DateTime, bool> dateValidDict)
        {
            return l.GetValidDays(dateValidDict).Select(d => d.Date).Distinct().ToList();
        }

        public static List<int> GetValidTimeBlockDateIds(this List<TimeEngineDay> l, Dictionary<DateTime, bool> dateValidDict)
        {
            return l.GetValidDays(dateValidDict).Select(d => d.TimeBlockDateId).Distinct().ToList();
        }

        public static DateTime GetStartDate(this List<TimeEngineDay> l)
        {
            return l?.OrderBy(s => s.Date).FirstOrDefault()?.Date ?? CalendarUtility.DATETIME_DEFAULT;
        }

        public static DateTime GetStopDate(this List<TimeEngineDay> l)
        {
            return l?.OrderBy(s => s.Date).LastOrDefault()?.Date ?? CalendarUtility.DATETIME_DEFAULT;
        }

        public static TimeEngineDay GetDay(this List<TimeEngineDay> l, int? templatePeriodId, TimeBlockDate timeBlockDate)
        {
            return l?.FirstOrDefault(r => r.TemplatePeriodId == templatePeriodId && r.EmployeeId == timeBlockDate.EmployeeId && r.Date == timeBlockDate.Date);
        }

        public static void AddDay(this List<TimeEngineDay> l, int? templatePeriodId, TimeBlockDate timeBlockDate, List<TimeCodeTransaction> additionalTimeCodeTransactions = null, EmployeeGroup employeeGroup = null)
        {
            if (timeBlockDate == null)
                return;

            TimeEngineDay e = l.GetDay(templatePeriodId, timeBlockDate);
            if (e == null)
            {
                e = new TimeEngineDay(
                    templatePeriodId: templatePeriodId, 
                    timeBlockDate: timeBlockDate,
                    employeeGroup, 
                    additionalTimeCodeTransactions: additionalTimeCodeTransactions
                    );
                l.Add(e);
            }
        }

        public static void AddDay(this List<TimeEngineDay> l, int? templatePeriodId, TimeBlockDate timeBlockDate, List<TimeBlock> timeBlocks, List<TimeCodeTransaction> additionalTimeCodeTransactions = null, EmployeeGroup employeeGroup = null, int? standardTimeDeviationCauseId = null)
        {
            if (timeBlocks.IsNullOrEmpty() || timeBlockDate == null)
                return;

            foreach (TimeBlock timeBlock in timeBlocks)
            {
                TimeEngineDay e = l.GetDay(templatePeriodId, timeBlockDate);
                if (e == null)
                {
                    e = new TimeEngineDay(
                        templatePeriodId: templatePeriodId, 
                        timeBlockDate: timeBlockDate,
                        employeeGroup: employeeGroup,
                        additionalTimeCodeTransactions: additionalTimeCodeTransactions,
                        standardTimeDeviationCauseId: standardTimeDeviationCauseId
                        );
                    l.Add(e);
                }
                e.TimeBlocks.Add(timeBlock);
            }
        }

        #endregion

        #region TimeEngineRestoreDay

        public static void AddDay(this List<TimeEngineRestoreDay> l, int employeeId, int? templatePeriodId, DateTime date, bool clearScheduledAbsence)
        {
            TimeEngineRestoreDay e = l.FirstOrDefault(r => r.EmployeeId == employeeId && r.TemplatePeriodId == templatePeriodId && r.Date == date);
            if (e == null)
            {
                e = new TimeEngineRestoreDay(employeeId, templatePeriodId, date, clearScheduledAbsence);
                l.Add(e);
            }
        }

        #endregion

        #region TimeEngineAbsenceDay

        public static bool Exists(this List<TimeEngineAbsenceDay> l, TimeEngineAbsenceDay other)
        {
            if (l.IsNullOrEmpty() || other == null)
                return false;
            return l.Any(e =>
                e.EmployeeId == other.EmployeeId &&
                e.Date == other.Date);
        }

        #endregion

        #region PayrollCalculationTransaction

        public static List<PayrollCalculationTransaction> ToPayrollCalculationTransactions(this IEnumerable<TimePayrollTransaction> l)
        {
            var transactions = new List<PayrollCalculationTransaction>();

            foreach (var e in l)
            {
                transactions.Add(e.ToPayrollCalculationTransaction());
            }

            return transactions;
        }

        public static List<PayrollCalculationTransaction> ToPayrollCalculationTransactions(this IEnumerable<TimePayrollScheduleTransaction> l)
        {
            var transactions = new List<PayrollCalculationTransaction>();

            foreach (var e in l)
            {
                transactions.Add(e.ToPayrollCalculationTransaction());
            }

            return transactions;
        }

        public static PayrollCalculationTransaction ToPayrollCalculationTransaction(this TimePayrollTransaction e)
        {
            return new PayrollCalculationTransaction
            {
                TransactionId = e.TimePayrollTransactionId,
                EmployeeId = e.EmployeeId,
                TimeBlockDateId = e.TimeBlockDateId,
                Date = e.TimeBlockDate != null ? e.TimeBlockDate.Date : CalendarUtility.DATETIME_DEFAULT,
                ProductId = e.ProductId,
                Amount = e.Amount ?? 0,
                IsScheduleTransaction = false,
                ScheduleType = SoeTimePayrollScheduleTransactionType.None,
                SysPayrollTypeLevel1 = e.SysPayrollTypeLevel1,
                SysPayrollTypeLevel2 = e.SysPayrollTypeLevel2,
                SysPayrollTypeLevel3 = e.SysPayrollTypeLevel3,
                SysPayrollTypeLevel4 = e.SysPayrollTypeLevel4,
                IsRetroTransaction = e.IsRetroTransaction(),
                AccountInternal = e.AccountInternal.ToList(),
            };
        }

        public static PayrollCalculationTransaction ToPayrollCalculationTransaction(this TimePayrollScheduleTransaction e)
        {
            return new PayrollCalculationTransaction
            {
                TransactionId = e.TimePayrollScheduleTransactionId,
                EmployeeId = e.EmployeeId,
                TimeBlockDateId = e.TimeBlockDateId,
                Date = e.TimeBlockDate != null ? e.TimeBlockDate.Date : CalendarUtility.DATETIME_DEFAULT,
                ProductId = e.ProductId,
                Amount = e.Amount ?? 0,
                IsScheduleTransaction = true,
                ScheduleType = e.Type.HasValue ? (SoeTimePayrollScheduleTransactionType)e.Type : SoeTimePayrollScheduleTransactionType.None,
                SysPayrollTypeLevel1 = e.SysPayrollTypeLevel1,
                SysPayrollTypeLevel2 = e.SysPayrollTypeLevel2,
                SysPayrollTypeLevel3 = e.SysPayrollTypeLevel3,
                SysPayrollTypeLevel4 = e.SysPayrollTypeLevel4,
                IsRetroTransaction = e.IsRetroTransaction(),
                AccountInternal = e.AccountInternal.ToList(),
            };
        }

        #endregion
    }

    #endregion

    #region Public classes

    //ReCalculation trackers
    public class ReCalculationContext
    {
        private readonly ReCalculationTrackerDetails<OvertimeTracker<OvertimeDay>, OvertimeDay> overtimeTrackerDetails;
        public ReCalculationTrackerDetails<OvertimeTracker<OvertimeDay>, OvertimeDay> OvertimeTrackerDetails => overtimeTrackerDetails;

        private readonly ReCalculationTrackerDetails<ApplyAbsenceTracker<ApplyAbsenceDay>, ApplyAbsenceDay> applyAbsenceTrackerDetails;
        public ReCalculationTrackerDetails<ApplyAbsenceTracker<ApplyAbsenceDay>, ApplyAbsenceDay> ApplyAbsenceTrackerDetails => applyAbsenceTrackerDetails;

        private readonly ReCalculationTrackerDetails<RestoreAbsenceTracker<RestoreAbsenceDay>, RestoreAbsenceDay> restoreAbsenceTrackerDetails;
        public ReCalculationTrackerDetails<RestoreAbsenceTracker<RestoreAbsenceDay>, RestoreAbsenceDay> RestoreAbsenceTrackerDetails => restoreAbsenceTrackerDetails;

        private readonly List<int> errorNumbers;
        public List<int> ErrorNumbers => errorNumbers;
        public bool HasErrorNumbers => errorNumbers.Any();

        private readonly List<DateTime> datesCalculated;
        public List<DateTime> DatesCalculated => datesCalculated;

        public ReCalculationContext(
            ReCalculationTrackerDetails<OvertimeTracker<OvertimeDay>, OvertimeDay> overtimeTrackerDetails,
            ReCalculationTrackerDetails<ApplyAbsenceTracker<ApplyAbsenceDay>, ApplyAbsenceDay> applyAbsenceTrackerDetails,
            ReCalculationTrackerDetails<RestoreAbsenceTracker<RestoreAbsenceDay>, RestoreAbsenceDay> restoreAbsenceTrackerDetails
            )
        {
            this.overtimeTrackerDetails = overtimeTrackerDetails;
            this.applyAbsenceTrackerDetails = applyAbsenceTrackerDetails;
            this.restoreAbsenceTrackerDetails = restoreAbsenceTrackerDetails;
            this.datesCalculated = new List<DateTime>();
            this.errorNumbers = new List<int>();
        }

        public bool ContainsErrorNumber(int errorNumber) => errorNumbers.Contains(errorNumber);

        public void AddDatesAsCalculated(List<DateTime> dates)
        {
            foreach (DateTime date in dates)
            {
                if (!this.datesCalculated.Contains(date))
                    this.datesCalculated.Add(date);
            }
        }

        public bool HasDaysCalculated() => HasOvertimeDaysCalculated() || HasApplyAbsenceDaysCalculated() || HasRestoreAbsenceDaysCalculated();
        public bool HasOvertimeDaysCalculated() => overtimeTrackerDetails.HasDaysRecalculated;
        public bool HasApplyAbsenceDaysCalculated() => applyAbsenceTrackerDetails.HasDaysRecalculated;
        public bool HasRestoreAbsenceDaysCalculated() => restoreAbsenceTrackerDetails.HasDaysRecalculated;

        public List<DateTime> GetUniqueDaysToRecalculate()
        {
            var allDates = new List<DateTime>();
            allDates.AddRange(applyAbsenceTrackerDetails.Days.Select(d => d.Date));
            allDates.AddRange(restoreAbsenceTrackerDetails.Days.Select(d => d.Date));
            allDates.AddRange(overtimeTrackerDetails.Days.Select(d => d.Date));
            return allDates.Distinct().OrderBy(d => d).ToList();
        }
    }
    public class ReCalculationTrackerDetails<TTracker, TDay>
        where TTracker : ReCalculateTrackerBase<TDay>
        where TDay : RecalculateDayBase
    {
        public TTracker Tracker { get; }
        public List<TDay> Days { get; }
        public List<DateTime> Dates => Days.GetDates();
        public bool HasDaysRecalculated { get; }

        public ReCalculationTrackerDetails(TTracker tracker, List<TDay> days, bool hasDaysRecalculated)
        {
            Tracker = tracker;
            Days = days ?? new List<TDay>();
            HasDaysRecalculated = hasDaysRecalculated;
        }
    }
    public abstract class ReCalculateTrackerBase<T> 
        where T : RecalculateDayBase
    {
        protected List<T> days = new List<T>();
        public List<T> Days => days;

        public T GetDay(DateTime date)
        {
            return days.FirstOrDefault(i => i.Date == date);
        }
        public bool HasDay(DateTime date)
        {
            return days.Any(i => i.Date == date);
        }
        public void AddDay(T day)
        {
            if (day != null && !HasDay(day.Date))
                days.Add(day);
        }
    }
    public abstract class ReCalculateTrackerAbsenceBase<T> : ReCalculateTrackerBase<T>
        where T : AbsenceDayBase
    {
        public List<DateTime> GetAbsenceDates(bool isVacation)
        {
            return days.Where(i => i.IsVacation == isVacation).Select(i => i.Date).OrderBy(i => i).ToList();
        }
        public bool HasVacationFiveDaysPerWeek()
        {
            return days.Any(i => i.IsVacationFiveDaysPerWeek);
        }
    }
    public class ApplyAbsenceTracker<T> : ReCalculateTrackerAbsenceBase<T> 
        where T : ApplyAbsenceDay
    {
        public ApplyAbsenceDay GetDay(DateTime date, int sysPayrollTypeLevel3)
        {
            return days.FirstOrDefault(i => i.Date == date && i.SysPayrollTypeLevel3 == sysPayrollTypeLevel3);
        }
        public void AddOrUpdate(T day)
        {
            if (day == null)
                return;

            var existingDay = GetDay(day.Date);
            if (existingDay == null)
                AddDay(day);
            else
                existingDay.Update(day);
        }
    }
    public class RestoreAbsenceTracker<T> : ReCalculateTrackerAbsenceBase<T> 
        where T : RestoreAbsenceDay
    {
    }
    public class OvertimeTracker<T> : ReCalculateTrackerBase<T> 
        where T : OvertimeDay
    {
    }

    //ReCalculation content classes
    public abstract class RecalculateDayBase
    {
        #region Variables

        public Guid Key { get; private set; }
        public DateTime Date { get; private set; }

        #endregion

        #region Ctor

        protected RecalculateDayBase(DateTime date)
        {
            this.Key = Guid.NewGuid();
            this.Date = date;
        }

        #endregion
    }
    public abstract class AbsenceDayBase : RecalculateDayBase
    {
        #region Variables

        public bool IsVacation { get; protected set; }
        public bool IsVacationFiveDaysPerWeek { get; protected set; }
        public bool? VacationFiveDaysPerWeekHasSchedule { get; protected set; }
        public bool? VacationFiveDaysPerWeekIsWeekend { get; protected set; }

        #endregion

        #region Ctor

        protected AbsenceDayBase(DateTime date, bool isVacation = false, bool isVacationFiveDaysPerWeek = false) : base(date)
        {
            this.IsVacation = isVacation;
            this.IsVacationFiveDaysPerWeek = isVacationFiveDaysPerWeek;
        }

        #endregion

        #region Public methods

        public void SetVacationFiveDays(DateTime date, int scheduleMinutes, bool isHoliday = false)
        {
            this.IsVacationFiveDaysPerWeek = true;
            this.VacationFiveDaysPerWeekIsWeekend = isHoliday || date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday;
            this.VacationFiveDaysPerWeekHasSchedule = scheduleMinutes > 0;
        }

        public virtual bool ContainsLeve3(TermGroup_SysPayrollType level3)
        {
            return false;
        }

        #endregion
    }
    public abstract class ApplyAbsenceDayBase : AbsenceDayBase
    {
        #region Variables

        public DateTime QualifyingDate { get; protected set; }
        public DateTime QualifyingWeekEnd
        {
            get
            {
                return CalendarUtility.GetEndOfWeek(this.QualifyingDate);
            }
        }
        public int? NewProductId { get; protected set; }
        public int AbsenceDayNumber { get; protected set; }
        public bool HasAbsenceDayNumberFromSequence { get; set; }
        public TimeAbsenceRuleHead TimeAbsenceRule { get; protected set; }
        public TimeAbsenceRuleRow TimeAbsenceRuleRow { get; protected set; }
        public bool IsSickDuringIwh
        {
            get
            {
                return this.TimeAbsenceRule?.IsSickDuringIwh ?? false;
            }
        }
        public bool IsSickDuringStandby
        {
            get
            {
                return this.TimeAbsenceRule?.IsSickDuringStandby ?? false;
            }
        }

        #endregion

        #region Ctor

        protected ApplyAbsenceDayBase(DateTime date, int? newProductId, bool isVacation = false, bool isVacationFiveDaysPerWeek = false) : base(date, isVacation, isVacationFiveDaysPerWeek)
        {
            this.QualifyingDate = date;
            this.NewProductId = newProductId;
        }

        #endregion

        #region Static methods

        public static (List<ApplyAbsenceDay> absenceDays, List<ApplyAbsenceSickIwhStandbyDay> absenceSickIwhStandbyDays) Parse(List<ApplyAbsenceDayBase> allAbsenceDays)
        {
            List<ApplyAbsenceDay> absenceDays = new List<ApplyAbsenceDay>();
            List<ApplyAbsenceSickIwhStandbyDay> absenceSickIwhStandbyDays = new List<ApplyAbsenceSickIwhStandbyDay>();
            if (allAbsenceDays != null)
            {
                foreach (ApplyAbsenceDayBase absenceDay in allAbsenceDays)
                {
                    if (absenceDay is ApplyAbsenceDay applyAbsenceDay)
                        absenceDays.Add(applyAbsenceDay);
                    else if (absenceDay is ApplyAbsenceSickIwhStandbyDay applyAbsenceSickIwhStandbyDay)
                        absenceSickIwhStandbyDays.Add(applyAbsenceSickIwhStandbyDay);
                }
            }
            return (absenceDays, absenceSickIwhStandbyDays);
        }

        public static List<ApplyAbsenceSickIwhStandbyDay> Merge(List<ApplyAbsenceSickIwhStandbyDay> absenceSickIwhStandbyDays)
        {
            if (absenceSickIwhStandbyDays.IsNullOrEmpty())
                return new List<ApplyAbsenceSickIwhStandbyDay>();
            if (absenceSickIwhStandbyDays.Count == 1)
                return absenceSickIwhStandbyDays;

            List<ApplyAbsenceSickIwhStandbyDay> merged = new List<ApplyAbsenceSickIwhStandbyDay>();
            foreach (ApplyAbsenceSickIwhStandbyDay day in absenceSickIwhStandbyDays.OrderBy(i => i.ReplaceStartTime).ThenBy(i => i.ReplaceStopTime))
            {
                if (merged.Any())
                    merged.Last().TryExtend(day);
                else
                    merged.Add(day);
            }

            return merged;
        }

        #endregion
    }
    public class ApplyAbsenceDay : ApplyAbsenceDayBase
    {
        #region Variables

        public int SysPayrollTypeLevel3 { get; private set; }
        public DateTime? FetchBackwardStartDate { get; private set; }
        public DateTime? FetchBackwardStopDate { get; private set; }
        public DateTime? FetchForwardStartDate { get; private set; }
        public DateTime? FetchForwardStopDate { get; private set; }
        public List<TimePayrollTransaction> TimePayrollTransactionsToRecalculate { get; private set; }
        public bool IsVacationReplacement { get; private set; }
        public bool IsVacationReplacementAndResulted
        {
            get
            {
                return this.IsVacationReplacement && base.NewProductId.HasValue;
            }
        }

        #endregion

        #region Ctor

        public ApplyAbsenceDay(DateTime date, PayrollProduct newPayrollProduct, bool isVacation = false, bool isVacationReplacement = false, ApplyAbsenceDay otherApplyAbsenceDTO = null) : base(date, newPayrollProduct?.ProductId, isVacation, false)
        {
            this.SysPayrollTypeLevel3 = newPayrollProduct?.SysPayrollTypeLevel3 ?? 0;
            this.IsVacationReplacement = isVacationReplacement;
            this.TimePayrollTransactionsToRecalculate = new List<TimePayrollTransaction>();
            this.VacationFiveDaysPerWeekHasSchedule = otherApplyAbsenceDTO?.VacationFiveDaysPerWeekHasSchedule;
            this.VacationFiveDaysPerWeekIsWeekend = otherApplyAbsenceDTO?.VacationFiveDaysPerWeekIsWeekend;
            AddTimePayrollTransactionsToRecalculate(otherApplyAbsenceDTO);
        }

        public ApplyAbsenceDay(DateTime date, int? productId, int sysPayrollTypeLevel3, bool isVacation = false, bool isVacationReplacement = false, ApplyAbsenceDay otherApplyAbsenceDTO = null) : base(date, productId, isVacation, false)
        {
            this.SysPayrollTypeLevel3 = sysPayrollTypeLevel3;
            this.IsVacationReplacement = isVacationReplacement;
            this.TimePayrollTransactionsToRecalculate = new List<TimePayrollTransaction>();
            AddTimePayrollTransactionsToRecalculate(otherApplyAbsenceDTO);
        }

        private void AddTimePayrollTransactionsToRecalculate(ApplyAbsenceDay otherApplyAbsenceDTO)
        {
            if (otherApplyAbsenceDTO != null && otherApplyAbsenceDTO.TimePayrollTransactionsToRecalculate != null)
                this.TimePayrollTransactionsToRecalculate.AddRange(otherApplyAbsenceDTO.TimePayrollTransactionsToRecalculate);
        }

        #endregion

        #region Public methods

        public ApplyAbsenceDTO ToDTO()
        {
            return new ApplyAbsenceDTO()
            {
                Date = this.Date,
                NewProductId = this.NewProductId,
                SysPayrollTypeLevel3 = this.SysPayrollTypeLevel3,
                TimePayrollTransactionIdsToRecalculate = this.TimePayrollTransactionsToRecalculate?.Select(i => i.TimePayrollTransactionId).ToList() ?? new List<int>(),
                IsVacation = this.IsVacation,
            };
        }

        public bool IsDateIncludedInForwardTransactions(DateTime date)
        {
            return this.FetchForwardStartDate.HasValue && this.FetchForwardStopDate.HasValue && CalendarUtility.IsDateInRange(date, this.FetchForwardStartDate.Value, this.FetchForwardStopDate.Value);
        }

        public bool HasTimePayrollTransactionsToRecalculateToRecalculate()
        {
            return !this.TimePayrollTransactionsToRecalculate.IsNullOrEmpty();
        }

        public bool TrySetFetchBack(DateTime date, int checkDaysBack, bool hadSameAbsenceBefore)
        {
            if (checkDaysBack <= 0 || IsInSameInterval(this.AbsenceDayNumber - checkDaysBack, true, hadSameAbsenceBefore))
                return false;

            List<DateTime> dates = CalendarUtility.GetDatesBack(date, checkDaysBack);
            if (dates.IsNullOrEmpty())
                return false;

            this.FetchBackwardStartDate = dates.Min();
            this.FetchBackwardStopDate = dates.Max();
            return true;
        }

        public bool TrySetFetchForward(DateTime date, int checkDaysForward, bool hadSameAbsenceBefore)
        {
            if (checkDaysForward <= 0 || IsInSameInterval(this.AbsenceDayNumber + checkDaysForward, false, hadSameAbsenceBefore))
                return false;

            List<DateTime> dates = CalendarUtility.GetDatesForward(date, checkDaysForward);
            if (dates.IsNullOrEmpty())
                return false;

            SetFetchForward(dates.Min(), dates.Max());
            return true;
        }

        public bool TrySetFetchForward(List<DateTime> dates, bool hadSameAbsenceBefore)
        {
            if (dates.IsNullOrEmpty() || IsInSameInterval(this.AbsenceDayNumber + CalendarUtility.GetTotalDays(this.Date, dates.Max()), false, hadSameAbsenceBefore))
                return false;

            SetFetchForward(dates.Min(), dates.Max());
            return true;
        }

        public void SetFetchForward(DateTime? startDate = null, DateTime? stopDate = null)
        {
            if (startDate.HasValue)
                this.FetchForwardStartDate = startDate;
            if (stopDate.HasValue)
                this.FetchForwardStopDate = stopDate;
        }

        public void SetTimePayrollTransactionsToRecalculate(List<TimePayrollTransaction> timePayrollTransactions)
        {
            this.TimePayrollTransactionsToRecalculate.AddRange(timePayrollTransactions);
        }

        public void Update(TimeAbsenceRuleHead timeAbsenceRule = null, TimeAbsenceRuleRow timeAbsenceRuleRow = null, int? absenceDayNumber = null, DateTime? qualifyingDate = null, int? newProductId = null, bool? hasAbsenceDayNumberFromSequence = null)
        {
            if (timeAbsenceRule != null)
                base.TimeAbsenceRule = timeAbsenceRule;
            if (timeAbsenceRuleRow != null)
                base.TimeAbsenceRuleRow = timeAbsenceRuleRow;
            if (absenceDayNumber.HasValue)
                base.AbsenceDayNumber = absenceDayNumber.Value;
            if (qualifyingDate.HasValue)
                base.QualifyingDate = qualifyingDate.Value;
            if (newProductId.HasValue)
                base.NewProductId = newProductId.Value;
            if (hasAbsenceDayNumberFromSequence.HasValue)
                base.HasAbsenceDayNumberFromSequence = hasAbsenceDayNumberFromSequence.Value;
        }

        public void Update(ApplyAbsenceDay day)
        {
            if (day == null)
                return;

            this.Update(day.TimeAbsenceRule, day.TimeAbsenceRuleRow);

            if (!day.TimePayrollTransactionsToRecalculate.IsNullOrEmpty())
                this.TimePayrollTransactionsToRecalculate.AddRange(day.TimePayrollTransactionsToRecalculate);
        }

        public override bool ContainsLeve3(TermGroup_SysPayrollType level3)
        {
            return this.SysPayrollTypeLevel3 == (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_Sick;
        }

        #endregion

        #region Private methods

        private bool IsInSameInterval(int dayNumber, bool isBack, bool hadSameAbsenceBefore)
        {
            return
                hadSameAbsenceBefore &&
                !this.HasAbsenceDayNumberFromSequence &&
                this.TimeAbsenceRule?.TimeAbsenceRuleRow != null &&
                this.TimeAbsenceRuleRow != null &&
                this.TimeAbsenceRuleRow.IsInInterval(dayNumber) &&               
                (!isBack || !this.TimeAbsenceRule.TimeAbsenceRuleRow.Any(r => r.State == (int)SoeEntityState.Active && r.IsInInterval(dayNumber) && r.TimeAbsenceRuleRowId != this.TimeAbsenceRuleRow.TimeAbsenceRuleRowId && r.Stop < this.TimeAbsenceRuleRow.Stop));
        }

        #endregion
    }
    public class ApplyAbsenceResult
    {
        #region Varibles

        public int OldProductId { get; set; }
        public int NewProductId { get; set; }
        public ApplyAbsenceDay AbsenceDay { get; set; }

        #endregion

        #region Ctor

        public ApplyAbsenceResult(ApplyAbsenceDay absenceDay, PayrollProduct oldProduct)
        {
            this.OldProductId = oldProduct?.ProductId ?? 0;
            this.NewProductId = absenceDay?.NewProductId ?? 0;
            this.AbsenceDay = absenceDay;
        }

        #endregion

        #region Static methods

        public static ApplyAbsenceResult GetResult(List<ApplyAbsenceResult> results, PayrollProduct oldProduct)
        {
            return oldProduct != null ? results?.FirstOrDefault(i => i.OldProductId == oldProduct.ProductId) : null;
        }

        #endregion
    }
    public class ApplyAbsenceSickIwhStandbyDay : ApplyAbsenceDayBase
    {
        #region Variables

        public int? NewTimeCodeId { get; private set; }
        public int? ReplaceRelatedProductId { get; private set; }
        public DateTime? ReplaceStartTime { get; set; }
        public DateTime? ReplaceStopTime { get; set; }
        public bool IsQualifyingStandby
        {
            get
            {
                return this.IsSickDuringStandby && this.ReplaceStartTime.HasValue && this.ReplaceStopTime.HasValue;
            }
        }

        #endregion

        #region Ctor

        public ApplyAbsenceSickIwhStandbyDay(DateTime date, int? newProductId) : base(date, newProductId)
        {
            this.NewTimeCodeId = null;
            this.ReplaceRelatedProductId = null;
        }

        #endregion

        #region Public methods

        public void Update(TimeAbsenceRuleHead timeAbsenceRule, TimeAbsenceRuleRow timeAbsenceRuleRow, int? absenceDayNumber = null, DateTime? qualifyingDate = null, int? newProductId = null, int? newTimeCodeId = null, int? replaceRelatedProductId = null, DateTime? replaceStartTime = null, DateTime? replaceStopTime = null)
        {
            if (timeAbsenceRule != null)
                base.TimeAbsenceRule = timeAbsenceRule;
            if (timeAbsenceRuleRow != null)
                base.TimeAbsenceRuleRow = timeAbsenceRuleRow;
            if (absenceDayNumber.HasValue)
                base.AbsenceDayNumber = absenceDayNumber.Value;
            if (qualifyingDate.HasValue)
                base.QualifyingDate = qualifyingDate.Value;
            if (newProductId.HasValue)
                base.NewProductId = newProductId.Value;
            if (newTimeCodeId.HasValue)
                this.NewTimeCodeId = newTimeCodeId.Value;
            if (replaceRelatedProductId.HasValue)
                this.ReplaceRelatedProductId = replaceRelatedProductId.Value;
            if (replaceStartTime.HasValue)
                this.ReplaceStartTime = replaceStartTime.Value;
            if (replaceStopTime.HasValue)
                this.ReplaceStopTime = replaceStopTime.Value;
        }

        public void TryExtend(ApplyAbsenceSickIwhStandbyDay next)
        {
            if (next == null || !IsConnected(next) || this.ReplaceStartTime > next.ReplaceStopTime)
                return;

            this.ReplaceStopTime = next.ReplaceStopTime;
        }

        public bool IsConnected(ApplyAbsenceSickIwhStandbyDay next)
        {
            if (next == null)
                return false;
            return this.NewTimeCodeId == next.NewTimeCodeId && this.ReplaceStopTime == next.ReplaceStartTime;
        }

        public bool IsMatch(int timeCodeId, DateTime replaceStartTime, DateTime replaceStopTime)
        {
            return this.NewTimeCodeId == timeCodeId && this.ReplaceStartTime == replaceStartTime && this.ReplaceStopTime == replaceStopTime;
        }

        #endregion
    }
    public class RestoreAbsenceDay : AbsenceDayBase
    {
        #region Variables

        public List<int> SysPayrollTypeLevel3s { get; set; }

        #endregion

        #region Ctor

        public RestoreAbsenceDay(DateTime date, List<int> sysPayrollTypeLevel3s, bool isVacationFiveDaysPerWeek) : base(date, sysPayrollTypeLevel3s?.Contains((int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_Vacation) ?? false, isVacationFiveDaysPerWeek)
        {
            this.SysPayrollTypeLevel3s = sysPayrollTypeLevel3s;
        }

        #endregion

        #region Public methods

        public override bool ContainsLeve3(TermGroup_SysPayrollType level3)
        {
            return this.SysPayrollTypeLevel3s.Any(l => l == (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_Sick);
        }

        #endregion
    }
    public class OvertimeDay : RecalculateDayBase
    {
        public OvertimeDay(DateTime date) : base(date)
        {

        }
    }


    public class AccountingDistributionDTO
    {
        #region Properties

        public int AccountStdId { get; set; }
        public decimal Quantity { get; set; }
        public List<AccountInternal> AccountInternal { get; set; }
        public string AccountingString
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(AccountStdId + ",");
                foreach (var item in AccountInternal.OrderBy(x => x.AccountId))
                {
                    if (sb.Length > 0)
                        sb.Append(",");
                    sb.Append(item.AccountId);
                }
                return sb.ToString();
            }
        }

        #endregion
    }
    public class AccountingPrioCacheItem
    {
        #region Properties

        public int? ProductId { get; set; }
        public DateTime Date { get; set; }
        public AccountingPrioDTO AccountingPrioDTO { get; set; }

        #endregion

        #region Ctor

        public AccountingPrioCacheItem(AccountingPrioDTO accountingPrio, DateTime date, int? productId)
        {
            this.AccountingPrioDTO = accountingPrio;
            this.Date = date;
            this.ProductId = productId;
        }

        #endregion
    }
    public class CreateAbsenceDetailResultDTO
    {
        #region Properties

        public int EmployeeId { get; set; }
        public Dictionary<DateTime, bool> DateResult { get; set; }

        #endregion

        #region Ctor

        public CreateAbsenceDetailResultDTO(int employeeId)
        {
            this.EmployeeId = employeeId;
            this.DateResult = new Dictionary<DateTime, bool>();
        }

        #endregion
    }
    public class EmployeesAttestResult
    {
        public int AttestStateToId { get; private set; }
        public List<EmployeeAttestResult> EmployeeResults { get; private set; }

        public bool Success => this.EmployeeResults.All(e => e.Success);

        public EmployeesAttestResult(int attestStateToId)
        {
            this.AttestStateToId = attestStateToId;
            this.EmployeeResults = new List<EmployeeAttestResult>();
        }

        public void AddEmployeeResult(EmployeeAttestResult employeeResult)
        {
            if (employeeResult != null)
                this.EmployeeResults.Add(employeeResult);
        }
    }
    public class EmployeeAttestResult
    {
        public int EmployeeId { get; set; }
        public string NumberAndName { get; private set; }
        public bool Success => NoOfTranscationsFailed <= 0;
        public int NoOfTranscationsAttested => transactionIdsAttested.Count;
        public int NoOfTranscationsFailed => transactionIdsFailed.Count;
        public int NoOfDaysFailed => datesFailed.Count;
        public int NoOfDaysWithStampingErrors => datesWithStampingErrors.Count;
        public string DatesFailedString
        {
            get
            {
                return datesFailed.Select(i => i.Date).GetCohereDateRangesText();
            }
        }

        private readonly List<int> transactionIdsAttested;
        private readonly List<int> transactionIdsFailed;
        private readonly List<DateTime> datesFailed;
        private readonly List<DateTime> datesWithStampingErrors;

        public EmployeeAttestResult(Employee employee)
        {
            if (employee == null)
                throw new ArgumentNullException(nameof(employee));

            this.EmployeeId = employee.EmployeeId;
            this.NumberAndName = employee.NumberAndName;

            this.transactionIdsAttested = new List<int>();
            this.transactionIdsFailed = new List<int>();
            this.datesFailed = new List<DateTime>();
            this.datesWithStampingErrors = new List<DateTime>();
        }

        public void AddTransactionAttested(TimePayrollTransaction timePayrollTransaction)
        {
            if (timePayrollTransaction == null)
                return;

            AddTransactionId(this.transactionIdsAttested, this.transactionIdsFailed, timePayrollTransaction.TimePayrollTransactionId);
        }

        public void AddTransactionFailed(TimePayrollTransaction timePayrollTransaction)
        {
            if (timePayrollTransaction == null)
                return;

            AddTransactionId(this.transactionIdsFailed, this.transactionIdsAttested, timePayrollTransaction.TimePayrollTransactionId);

            if (timePayrollTransaction.TimeBlockDate != null && !this.datesFailed.Contains(timePayrollTransaction.TimeBlockDate.Date))
                this.datesFailed.Add(timePayrollTransaction.TimeBlockDate.Date);
        }

        public void AddTransactionsFailed(IEnumerable<TimePayrollTransaction> timePayrollTransactions)
        {
            if (timePayrollTransactions.IsNullOrEmpty())
                return;

            foreach (var timePayrollTransaction in timePayrollTransactions)
            {
                AddTransactionFailed(timePayrollTransaction);
            }
        }

        public void AddStampingError(TimeBlockDate timeBlockDate)
        {
            if (timeBlockDate == null || this.datesWithStampingErrors.Contains(timeBlockDate.Date))
                return;

            this.datesWithStampingErrors.Add(timeBlockDate.Date);
        }

        private void AddTransactionId(List<int> addTo, List<int> removeFrom, int transactionId)
        {
            if (!addTo.Contains(transactionId))
            {
                addTo.Add(transactionId);
                if (removeFrom.Contains(transactionId))
                    removeFrom.Remove(transactionId);
            }
        }
    }
    public class ExtraShiftCalculationPeriod
    {
        #region Properties

        public int EmployeeId { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public int WorkMinutes { get; set; }
        public bool UseIgnoreIfExtraShifts { get; set; }
        public List<ExtraShiftCalculationItem> Basis { get; set; }

        #endregion

        #region Ctor

        public ExtraShiftCalculationPeriod(int employeeId, DateTime dateFrom, DateTime dateTo, bool useIgnoreIfExtraShifts)
        {
            this.EmployeeId = employeeId;
            this.DateFrom = dateFrom;
            this.DateTo = dateTo;
            this.UseIgnoreIfExtraShifts = useIgnoreIfExtraShifts;
            this.Basis = new List<ExtraShiftCalculationItem>();
        }

        #endregion

        #region Methods

        public bool IsSame(DateTime dateFrom, DateTime dateTo, bool useIgnoreIfExtraShifts)
        {
            return 
                this.DateFrom.Date == dateFrom.Date && 
                this.DateTo.Date == dateTo.Date &&
                this.UseIgnoreIfExtraShifts == useIgnoreIfExtraShifts;
        }
        public void AddBasis(TimeScheduleTemplateBlock extraShift, List<TimeScheduleTemplateBlock> breaks)
        {
            if (!extraShift.ExtraShift)
                return;

            this.Basis.Add(new ExtraShiftCalculationItem(extraShift, breaks));
        }

        #endregion
    }
    public class ExtraShiftCalculationItem
    {
        #region Properties

        public TimeScheduleTemplateBlock ExtraShift { get; set; }
        public List<TimeScheduleTemplateBlock> BreaksWithinExtraShift { get; set; }

        #endregion

        #region Ctor

        public ExtraShiftCalculationItem(TimeScheduleTemplateBlock extraShift, List<TimeScheduleTemplateBlock> breaks)
        {
            this.ExtraShift = extraShift;
            this.BreaksWithinExtraShift = breaks;
        }

        #endregion
    }
    public class PayrollProductRoundingDTO
    {
        #region Properties

        public int PayrollProductSettingId { get; set; }
        public int ProductId { get; set; }
        public int AccountStdId { get; set; }
        public decimal Amount { get; set; }
        public decimal Quantity { get; set; }
        public List<AccountInternal> AccountInternal { get; set; }
        public string AccountingString
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(AccountStdId + ",");
                foreach (var item in AccountInternal.OrderBy(x => x.AccountId))
                {
                    sb.Append(item.AccountId + ",");
                }
                return sb.ToString();
            }
        }

        #endregion
    }
    public class PayrollCalculationTransaction : IPayrollType
    {
        #region Properties

        public int TransactionId { get; set; }
        public int EmployeeId { get; set; }
        public int TimeBlockDateId { get; set; }
        public DateTime Date { get; set; }
        public int ProductId { get; set; }
        public decimal Amount { get; set; }
        public int? SysPayrollTypeLevel1 { get; set; }
        public int? SysPayrollTypeLevel2 { get; set; }
        public int? SysPayrollTypeLevel3 { get; set; }
        public int? SysPayrollTypeLevel4 { get; set; }
        public bool IsScheduleTransaction { get; set; }
        public SoeTimePayrollScheduleTransactionType ScheduleType { get; set; }
        public bool IsRetroTransaction { get; set; }
        public List<AccountInternal> AccountInternal { get; set; }
        public string Levels
        {
            get
            {
                return
                        (this.SysPayrollTypeLevel1.HasValue ? ((TermGroup_SysPayrollType)this.SysPayrollTypeLevel1.Value).ToString() + " - " : "") +
                        (this.SysPayrollTypeLevel2.HasValue ? ((TermGroup_SysPayrollType)this.SysPayrollTypeLevel2.Value).ToString() + " - " : "") +
                        (this.SysPayrollTypeLevel3.HasValue ? ((TermGroup_SysPayrollType)this.SysPayrollTypeLevel3.Value).ToString() + " - " : "") +
                        (this.SysPayrollTypeLevel4.HasValue ? ((TermGroup_SysPayrollType)this.SysPayrollTypeLevel4.Value).ToString() : "");
            }
        }
        public string InternalAccountingString
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                foreach (var item in AccountInternal.OrderBy(x => x.AccountId))
                {
                    sb.Append(item.AccountId);
                    sb.Append(",");
                }
                return sb.ToString();
            }
        }

        #endregion
    }
    public class PayrollAmountsDTO
    {
        #region Properties

        public decimal TableTaxTransactionsAmount { get; set; }
        public decimal OneTimeTaxTransactionsAmount { get; set; }
        public decimal OptionalTaxAmount { get; set; }
        public decimal EmploymentTaxDebitTransactionsAmount { get; set; }
        public decimal EmploymentTaxBasisTransactionsAmount { get; set; }
        public decimal SupplementChargeDebitTransactionsAmount { get; set; }
        public decimal GrosSalaryAmount { get; set; }
        public decimal CompensationAmount { get; set; }
        public decimal DeductionAmount { get; set; }
        public decimal BenefitAmount { get; set; }
        public decimal DeductionSalaryDistressAmount { get; set; }
        public decimal DeductionCarBenefit { get; set; }
        public decimal UnionFeePromotedAmount { get; set; }
        public decimal BenefitOtherAmount { get; set; }
        public decimal BenefitPropertyNotHouseAmount { get; set; }
        public decimal BenefitPropertyHouseAmount { get; set; }
        public decimal BenefitFuelAmount { get; set; }
        public decimal BenefitROTAmount { get; set; }
        public decimal BenefitRUTAmount { get; set; }
        public decimal BenefitFoodtherAmount { get; set; }
        public decimal BenefitBorrowedComputerAmount { get; set; }
        public decimal BenefitParkingAmount { get; set; }
        public decimal BenefitInterestAmount { get; set; }
        public decimal BenefitCompanyCarAmount { get; set; }
        public List<PayrollCalculationTransaction> VacationSalaryPromotedTransactions { get; set; }

        #endregion

        #region Ctor

        public PayrollAmountsDTO()
        {
            this.VacationSalaryPromotedTransactions = new List<PayrollCalculationTransaction>();
        }

        #endregion
    }   
    public class ReCalculatePayrollPeriodCompanyDTO
    {
        #region Properties

        public PayrollProduct PayrollProductTableTax { get; set; }
        public PayrollProduct PayrollProductOneTimeTax { get; set; }
        public PayrollProduct PayrollProductEmploymentTaxCredit { get; set; }
        public PayrollProduct PayrollProductEmploymentTaxDebet { get; set; }
        public PayrollProduct PayrollProductSupplementChargeDebet { get; set; }
        public PayrollProduct PayrollProductSupplementChargeCredit { get; set; }
        public PayrollProduct PayrollProductNetSalary { get; set; }
        public PayrollProduct PayrollProductNetSalaryRound { get; set; }
        public PayrollProduct PayrollProductVacationCompensationDR { get; set; }
        public PayrollProduct PayrollProductSalaryDistressAmount { get; set; }
        public PayrollProduct PayrollProductWeekendSalary { get; set; }
        public PayrollProduct PayrollProductGrossSalaryRound { get; set; }
        public List<PayrollProduct> PayrollProductUnionFees { get; set; }
        public List<PayrollProduct> PayrollProductMonthlySalaries { get; set; }
        public List<VacationGroup> VacationGroups { get; set; }
        public int SysCountryId { get; set; }
        public int InitialAttestStateId { get; set; }



        //Depends on TimePeriod
        public List<HolidayDTO> HolidaySalaryHolidaysCurrentPeriod { get; set; }

        #endregion
    }
    public class ReCalculatePayrollPeriodSysPayrollPriceDTO
    {
        #region Properties

        public decimal BaseAmount { get; set; }
        public decimal SysPayrollPriceVacationDayPercent { get; set; }

        #endregion
    }
    public class RetroactivePayrollCalculationDTO
    {
        #region Properties

        public DateTime Date { get; private set; }
        public TermGroup_PayrollResultType ResultType { get; set; }
        public int PayrollProductId { get; private set; }
        public bool IsRetroCalculated { get; private set; }
        public bool IsReversed { get; private set; }
        public bool IsScheduleTransaction { get; private set; }
        public bool IsPartOfPayrollPeriodCalculated { get; set; }
        public List<TimePayrollTransaction> TimePayrollTransactionBasis { get; private set; }
        public List<TimePayrollScheduleTransaction> TimePayrollScheduleTransactionBasis { get; private set; }
        private decimal retroUnitPrice;
        public decimal RetroUnitPrice
        {
            get { return retroUnitPrice; }
            set
            {
                this.retroUnitPrice = Decimal.Round(value, 4, MidpointRounding.AwayFromZero);
                this.IsRetroCalculated = true;
            }
        }
        private decimal transactionUnitPrice;
        public decimal TransactionUnitPrice
        {
            get { return transactionUnitPrice; }
            set
            {
                this.transactionUnitPrice = Decimal.Round(value, 4, MidpointRounding.AwayFromZero);
            }
        }
        private int errorCode;
        public int ErrorCode
        {
            get { return errorCode; }
            set
            {
                this.errorCode = value;
                this.IsRetroCalculated = false;
            }
        }

        #endregion

        #region Ctor

        public RetroactivePayrollCalculationDTO(int payrollProductId, DateTime date, TermGroup_PayrollResultType resultType, List<TimePayrollTransaction> timePayrollTransactionBasis, bool isReversed = false)
        {
            this.PayrollProductId = payrollProductId;
            this.Date = date;
            this.ResultType = resultType;
            this.IsReversed = isReversed;
            this.IsScheduleTransaction = false;
            this.TimePayrollTransactionBasis = timePayrollTransactionBasis ?? new List<TimePayrollTransaction>();
            this.TimePayrollScheduleTransactionBasis = new List<TimePayrollScheduleTransaction>();
        }

        public RetroactivePayrollCalculationDTO(int payrollProductId, DateTime date, TermGroup_PayrollResultType resultType, List<TimePayrollScheduleTransaction> timePayrollScheduleTransactionBasis, bool isReversed = false)
        {
            this.PayrollProductId = payrollProductId;
            this.Date = date;
            this.ResultType = resultType;
            this.IsReversed = isReversed;
            this.IsScheduleTransaction = true;
            this.TimePayrollTransactionBasis = new List<TimePayrollTransaction>();
            this.TimePayrollScheduleTransactionBasis = timePayrollScheduleTransactionBasis;
        }

        #endregion

        #region Static methods

        public static decimal GetTotalQuantity(List<RetroactivePayrollCalculationDTO> l)
        {
            decimal quantity = 0;
            foreach (var e in l)
            {
                if (e.IsScheduleTransaction)
                    quantity += e.TimePayrollScheduleTransactionBasis.Sum(i => i.Quantity);
                else
                    quantity += e.TimePayrollTransactionBasis.Sum(i => i.Quantity);
            }
            return quantity;
        }

        #endregion
    }
    public class QualifyingDeductionPeriod
    {
        #region Variables

        public SicknessPeriod SicknessPeriod { get; set; }
        public DateTime Date { get; }
        public DateTime StartTime { get; private set; }
        public DateTime StopTime { get; private set; }
        public int Length
        {
            get { return (int)this.StopTime.Subtract(this.StartTime).TotalMinutes; }
        }
        public bool IsStandby { get; private set; }
        public TimePayrollTransaction SicknessSalaryTimePayrollTransaction { get; }
        public int EmployeeId
        {
            get
            {
                return this.SicknessSalaryTimePayrollTransaction?.EmployeeId ?? 0;
            }
        }
        public int? TimePeriodId
        {
            get
            {
                return this.SicknessSalaryTimePayrollTransaction?.TimePeriodId ?? null;
            }
        }
        public int AttestStateId
        {
            get
            {
                return this.SicknessSalaryTimePayrollTransaction?.AttestStateId ?? 0;
            }
        }
        public TimeCodeTransaction SicknessSalaryTimeCodeTransaction
        {
            get
            {
                return SicknessSalaryTimePayrollTransaction?.TimeCodeTransaction;
            }
        }
        public TimeTransactionItem SicknessSalaryTimeTransactionItem { get; }

        #endregion

        #region Ctor

        private QualifyingDeductionPeriod(SicknessPeriod sicknessPeriod, TimePayrollTransaction timePayrollTransaction, DateTime date)
        {
            this.SicknessPeriod = sicknessPeriod;
            this.SicknessSalaryTimePayrollTransaction = timePayrollTransaction;
            this.Date = date;
            if (timePayrollTransaction.TimeCodeTransaction != null)
                this.Setup(timePayrollTransaction.TimeCodeTransaction.Start, timePayrollTransaction.TimeCodeTransaction.Stop);
            else
                this.Setup(CalendarUtility.DATETIME_DEFAULT.AddHours(8), CalendarUtility.DATETIME_DEFAULT.AddHours(8).AddMinutes((int)timePayrollTransaction.Quantity));
        }

        private QualifyingDeductionPeriod(SicknessPeriod sicknessPeriod, TimeTransactionItem timeTransactionItem, DateTime date)
        {
            this.SicknessPeriod = sicknessPeriod;
            this.SicknessSalaryTimeTransactionItem = timeTransactionItem;
            this.Date = date;
            this.Setup(timeTransactionItem.TimeCodeStart.Value, timeTransactionItem.TimeCodeStop.Value);
        }

        private void Setup(DateTime startTime, DateTime stopTime)
        {
            if (this.SicknessPeriod == null)
                return;

            var standby = CheckStandby(startTime, ref stopTime);
            if (standby.isDuringStandby)
            {
                this.IsStandby = true;
                if (!standby.isQualifyingDay)
                    stopTime = startTime;
            }
            else
            {
                int length = (int)stopTime.Subtract(startTime).TotalMinutes;
                if (length > this.SicknessPeriod.QualifyingDeductionMinutesRemaining)
                    length = this.SicknessPeriod.QualifyingDeductionMinutesRemaining;
                stopTime = startTime.AddMinutes(length);
            }

            this.StartTime = startTime;
            this.StopTime = stopTime;
        }

        #endregion

        #region Static methods

        public static DateTime GetStandbyStopTime(DateTime date)
        {
            return date.Date.AddDays(1);
        }

        #endregion

        #region Public methods

        public static QualifyingDeductionPeriod Create(SicknessPeriod sicknessPeriod, object transaction, DateTime date)
        {
            QualifyingDeductionPeriod period = null;
            if (transaction is TimePayrollTransaction timePayrollTransaction)
                period = new QualifyingDeductionPeriod(sicknessPeriod, timePayrollTransaction, date);
            else if (transaction is TimeTransactionItem timeTransactionItem)
                period = new QualifyingDeductionPeriod(sicknessPeriod, timeTransactionItem, date);
            return period;
        }

        #endregion

        #region Private methods

        private (bool isDuringStandby, bool isQualifyingDay) CheckStandby(DateTime startTime, ref DateTime stopTime)
        {
            if (!this.SicknessPeriod.StandbyIntervals.IsNullOrEmpty())
            {
                foreach (StandbyInterval qualifyingStandbyInterval in this.SicknessPeriod.StandbyIntervals.Where(i => i.Date == this.Date))
                {
                    if (CalendarUtility.IsDatesOverlapping(startTime, stopTime, qualifyingStandbyInterval.StartTime, qualifyingStandbyInterval.StopTime))
                    {
                        DateTime standbyStopTime = GetStandbyStopTime(startTime);
                        if (stopTime > qualifyingStandbyInterval.StopTime)
                            stopTime = qualifyingStandbyInterval.StopTime;
                        if (stopTime > standbyStopTime)
                            stopTime = standbyStopTime;
                        return (true, qualifyingStandbyInterval.AbsenceDayNumber == 1);
                    }
                }
            }

            return (false, false);
        }

        #endregion
    }
    public class SicknessPeriod
    {
        #region Variables

        public Guid Guid { get; set; }
        public int EmployeeId { get; }
        public int EmployeeWeekMinutes { get; }
        public bool IsQualifyingPercentDeducted { get; set; }
        public bool UseStandby { get; }
        public List<StandbyInterval> StandbyIntervals { get; }
        public List<DateTime> Dates { get; }
        public List<QualifyingDeductionPeriod> QualifyingDeductionPeriods { get; }
        public List<TimePayrollTransaction> AbsenceSickTimePayrollTransactions { get; }
        public List<TimeTransactionItem> AbsenceSickTimeTransactionsItems { get; }
        public bool HasAbsenceSickTransactions
        {
            get
            {
                return this.HasAbsenceSickTimePayrollTransactions || this.HasAbsenceSickTimeTransactionItems;
            }
        }
        public bool HasAbsenceSickTimePayrollTransactions
        {
            get
            {
                return this.AbsenceSickTimePayrollTransactions.Any();
            }
        }
        public bool HasAbsenceSickTimeTransactionItems
        {
            get
            {
                return this.AbsenceSickTimeTransactionsItems.Any();
            }
        }
        public bool HasQualifyingDate
        {
            get
            {
                return
                    this.AbsenceSickTimePayrollTransactions.Any(i => i.SysPayrollTypeLevel4 == (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_Sick_QualifyingDay)
                    ||
                    this.AbsenceSickTimeTransactionsItems.Any(i => i.TransactionSysPayrollTypeLevel4 == (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_Sick_QualifyingDay);
            }
        }
        public int QualifyingDeductionMinutesBoundary
        {
            get
            {
                return (int)(this.EmployeeWeekMinutes * 0.2); //20% av sjuklön

            }
        }
        public int QualifyingDeductionMinutesUsed
        {
            get
            {
                return (int)QualifyingDeductionPeriods.Where(i => !i.IsStandby).Sum(i => i.StopTime.Subtract(i.StartTime).TotalMinutes);
            }
        }
        public int QualifyingDeductionMinutesRemaining
        {
            get
            {
                return this.QualifyingDeductionMinutesBoundary - this.QualifyingDeductionMinutesUsed;
            }
        }

        #endregion

        #region Ctor

        private SicknessPeriod(DateTime startDate, int employeeId, int employeeWeekMinutes, bool useStandby, List<StandbyInterval> standbyIntervals, object transactions)
        {
            this.Guid = Guid.NewGuid();
            this.EmployeeId = employeeId;
            this.EmployeeWeekMinutes = employeeWeekMinutes;
            this.UseStandby = useStandby;
            this.StandbyIntervals = standbyIntervals;
            this.Dates = new List<DateTime>();
            this.AbsenceSickTimePayrollTransactions = new List<TimePayrollTransaction>();
            this.AbsenceSickTimeTransactionsItems = new List<TimeTransactionItem>();
            this.QualifyingDeductionPeriods = new List<QualifyingDeductionPeriod>();

            this.AddDate(startDate);
            this.AddTransactions(transactions);
        }

        #endregion

        #region Methods

        #region Dates

        public DateTime GetStartOfPeriod()
        {
            return this.Dates.OrderBy(date => date.Date).FirstOrDefault();
        }

        public DateTime GetEndOfPeriod()
        {
            return this.Dates.OrderByDescending(date => date.Date).FirstOrDefault();
        }

        public bool IsSamePeriod(DateTime date)
        {
            foreach (DateTime existingDate in this.Dates)
            {
                if (date >= existingDate.AddDays(-Constants.SICKNESS_RELAPSEDAYS) &&
                    date <= existingDate.AddDays(Constants.SICKNESS_RELAPSEDAYS))
                    return true;
            }
            return false;
        }

        private void AddDate(DateTime date)
        {
            this.Dates.Add(date);
        }

        #endregion

        #region Transactions

        public List<DateTime> GetAbsenceSickTimePayrollTransactionsDates()
        {
            return this.AbsenceSickTimePayrollTransactions.Select(i => i.TimeBlockDate.Date).Distinct().OrderBy(i => i.Date).ToList();
        }

        public List<DateTime> GetAbsenceSickTimePayrollTransactionItemsDates()
        {
            return this.AbsenceSickTimeTransactionsItems.Select(i => i.Date.Value).Distinct().OrderBy(i => i.Date).ToList();
        }

        public List<TimePayrollTransaction> GetTimePayrollTransactions(DateTime date)
        {
            return this.AbsenceSickTimePayrollTransactions.Where(i => i.TimeBlockDate.Date == date).ToList();
        }

        public List<TimeTransactionItem> GetTimePayrollTransactionItems(DateTime date)
        {
            return this.AbsenceSickTimeTransactionsItems.Where(i => i.Date.Value == date).ToList();
        }

        private void AddTransactions(object transactions)
        {
            if (transactions == null)
                return;

            if (transactions is IEnumerable<TimePayrollTransaction> timePayrollTransactions)
                this.AbsenceSickTimePayrollTransactions.AddRange(timePayrollTransactions);
            else if (transactions is IEnumerable<TimeTransactionItem> timeTransactionItems)
                this.AbsenceSickTimeTransactionsItems.AddRange(timeTransactionItems);
        }

        #endregion

        #region SicknessPeriod

        public static SicknessPeriod Start(DateTime startDate, int employeeId, int employeeWeekMinutes, bool useStandby, List<StandbyInterval> standbyIntervals, object transactions)
        {
            return new SicknessPeriod(startDate, employeeId, employeeWeekMinutes, useStandby, standbyIntervals, transactions);
        }

        public bool TryExtend(DateTime date, EmployeeGroup employeeGroup, object transactions, List<StandbyInterval> standbyIntervals)
        {
            if (!IsSamePeriod(date))
                return false;
            if (employeeGroup?.QualifyingDayCalculationRuleLimitFirstDay == true && !this.Dates.IsNullOrEmpty() && !this.Dates.Contains(date))
                return false;

            this.AddDate(date);
            this.AddTransactions(transactions);
            this.AddStandbyIntervals(standbyIntervals);
            return true;
        }

        #endregion

        #region QualifyingDeductionPeriod

        public List<QualifyingDeductionPeriod> GetPeriods(DateTime date)
        {
            return this.QualifyingDeductionPeriods.Where(x => x.Date == date).ToList();
        }

        public QualifyingDeductionPeriod CreateQualifyingDeductionPeriod(object transaction, DateTime date)
        {
            QualifyingDeductionPeriod qualifyingDeductionPeriod = QualifyingDeductionPeriod.Create(this, transaction, date);
            if (qualifyingDeductionPeriod == null || qualifyingDeductionPeriod.Length == 0)
                return null;

            this.QualifyingDeductionPeriods.Add(qualifyingDeductionPeriod);
            return qualifyingDeductionPeriod;
        }

        public bool DoBreakPeriodEvaluation(QualifyingDeductionPeriod qualifyingDeductionPeriod)
        {
            return this.QualifyingDeductionMinutesRemaining <= 0 && DoBreakPeriodStandbyEvaluation(qualifyingDeductionPeriod);
        }

        /// <summary>
        /// During this period all "sjuk-ob" transactions should be turned
        /// Take into account that a period can be ex 13-19 and "sjuk-ob" can be 18-20, thus only a subset of "sjuk-ob" should be turned
        /// </summary>
        /// <param name="date"></param>
        /// <param name="startTime"></param>
        /// <param name="stopTime"></param>
        public bool TryGetPeriodTimes(DateTime date, out DateTime? startTime, out DateTime? stopTime)
        {
            if (!this.QualifyingDeductionPeriods.IsNullOrEmpty())
            {
                startTime = this.QualifyingDeductionPeriods.Where(i => i.Date == date).OrderBy(i => i.StartTime).First().StartTime;
                stopTime = this.QualifyingDeductionPeriods.Where(i => i.Date == date).OrderBy(i => i.StopTime).Last().StopTime;
                return true;
            }
            else
            {
                startTime = null;
                stopTime = null;
                return false;
            }
        }

        #endregion

        #region Standby

        public bool HasNoneQualifyingStandbyInterval(DateTime date, DateTime startTime, DateTime stopTime)
        {
            return this.StandbyIntervals?.Any(i => i.AbsenceDayNumber > 1 && i.Exists(date, startTime, stopTime)) ?? false;
        }

        private bool DoBreakPeriodStandbyEvaluation(QualifyingDeductionPeriod qualifyingDeductionPeriod)
        {
            //Break if standby is not used or do not exists on this date
            if (!this.UseStandby || this.StandbyIntervals.IsNullOrEmpty())
                return true;

            //Continue to we have a period to evaluate
            if (qualifyingDeductionPeriod == null)
                return false;

            //Standby boundary reached
            if (qualifyingDeductionPeriod.StopTime == QualifyingDeductionPeriod.GetStandbyStopTime(qualifyingDeductionPeriod.StartTime))
                return true;

            //Continue if any standby period is later on date
            return !this.StandbyIntervals.Any(i => i.StopTime > qualifyingDeductionPeriod.StopTime);
        }

        private void AddStandbyIntervals(List<StandbyInterval> standbyIntervals)
        {
            if (standbyIntervals == null)
                return;

            foreach (StandbyInterval standbyInterval in standbyIntervals)
            {
                if (!this.StandbyIntervals.Any(i => i.Key == standbyInterval.Key))
                    this.StandbyIntervals.Add(standbyInterval);
            }
        }

        #endregion

        #endregion
    }
    public class StandbyInterval
    {
        #region Variables

        public Guid Key { get; set; }
        public int AbsenceDayNumber { get; private set; }
        public DateTime Date { get; private set; }
        public DateTime StartTime { get; private set; }
        public DateTime StopTime { get; private set; }

        #endregion

        #region Public methods

        public StandbyInterval(ApplyAbsenceSickIwhStandbyDay absenceDay)
        {
            this.Key = Guid.NewGuid();
            this.AbsenceDayNumber = absenceDay.AbsenceDayNumber;
            this.Date = absenceDay.Date;
            this.StartTime = absenceDay.ReplaceStartTime ?? CalendarUtility.DATETIME_DEFAULT;
            this.StopTime = absenceDay.ReplaceStopTime ?? CalendarUtility.DATETIME_DEFAULT;
        }

        public StandbyInterval(TimeScheduleTemplateBlock scheduleBlock)
        {
            this.Key = Guid.NewGuid();
            this.SetAbsenceDayNumber1();
            this.Date = scheduleBlock.Date ?? CalendarUtility.DATETIME_DEFAULT;
            this.StartTime = scheduleBlock.StartTime;
            this.StopTime = scheduleBlock.StopTime;
        }

        public bool Exists(DateTime date, DateTime startTime, DateTime stopTime)
        {
            return this.Date == date && this.StartTime == startTime && this.StopTime == stopTime;
        }

        public void SetAbsenceDayNumber1()
        {
            this.AbsenceDayNumber = 1;
        }

        public override string ToString()
        {
            return $"{this.AbsenceDayNumber}, {this.Date.ToShortDateString()} {this.StartTime}-{this.StopTime}";
        }

        #endregion
    }
    public class TimeWorkReductionEarningInterval
    {
        public Employee Employee { get; }
        public Dictionary<DateTime, Employment> EmploymentsByDate { get; }
        public EmployeeGroup EmployeeGroup { get; }
        public TimeAccumulator TimeAccumulator { get; }
        public List<TimeScheduleTemplateBlock> ScheduleBlocks { get; }
        public int WeightMinutes { get; }
        public DateTime CurrentDate { get; }
        public DateTime DateFrom { get; }
        public DateTime DateTo { get; }

        public int Days => (this.DateTo - this.DateFrom).Days + 1;

        public TimeWorkReductionEarningInterval(
            Employee employee,
            Dictionary<DateTime, Employment> employmentsByDate,
            EmployeeGroup employeeGroup, 
            TimeAccumulator timeAccumulator,
            List<TimeScheduleTemplateBlock> scheduleBlocks,
            int weightMinutes,
            DateTime currentDate,
            DateTime dateFrom,
            DateTime dateTo
            )
        {
            if (dateTo < dateFrom)
                throw new ArgumentException("dateTo must be greater than or equal to dateFrom");

            this.Employee = employee ?? throw new ArgumentNullException(nameof(employee));
            this.EmploymentsByDate = employmentsByDate ?? throw new ArgumentNullException(nameof(employmentsByDate));
            this.EmployeeGroup = employeeGroup ?? throw new ArgumentNullException(nameof(employeeGroup));
            this.TimeAccumulator = timeAccumulator ?? throw new ArgumentNullException(nameof(timeAccumulator));
            this.ScheduleBlocks = scheduleBlocks ?? new List<TimeScheduleTemplateBlock>();
            this.WeightMinutes = weightMinutes;
            this.CurrentDate = currentDate;
            this.DateFrom = dateFrom;
            this.DateTo = dateTo;
        }

        public static DateRangeDTO GetEarningPeriod(DateTime currentDate)
        {
            return new DateRangeDTO(CalendarUtility.GetWeek(currentDate));
        }
    }
    public class TimeWorkReductionEarningResult
    {
        private readonly string formulaPlain;

        public int Quantity { get; }

        public decimal EmploymentRatio { get; }
        public int ScheduleMinutes { get; }
        public int AccumulatedEarningMinutes { get; }

        public TimeWorkReductionEarningInterval EarningInterval { get; }
        public int Days => EarningInterval.Days;
        public int WeightMinutes => EarningInterval.WeightMinutes;

        private List<TimeCodeTransaction> timeCodeTransactions;
        public IEnumerable<TimeCodeTransaction> TimeCodeTransactions => timeCodeTransactions;

        private List<TimePayrollTransaction> timePayrollTransactions;
        public IEnumerable<TimePayrollTransaction> TimePayrollTransactions => timePayrollTransactions;

        public TimeWorkReductionEarningResult(
            TimeWorkReductionEarningInterval earningInterval, 
            decimal employmentPercentage, 
            int accumulatedEarningMinutes,
            int scheduleMinutes,
            Func<int, string, string> getText = null
            )
        {
            this.EarningInterval = earningInterval ?? throw new ArgumentNullException(nameof(earningInterval));
            this.EmploymentRatio = Math.Min(employmentPercentage / 100m, 1m);
            this.AccumulatedEarningMinutes = accumulatedEarningMinutes;
            this.ScheduleMinutes = scheduleMinutes;

            //Quantity = (Days / 7) * Weight minutes * Employment ratio * (Accumulated earning minutes ({0}) / Schedule minutes)
            this.formulaPlain = string.Format(getText(110685, "ATF = (Dagar / 7) * Injänande minuter * Syssgrad * (Intjänande saldo minuter (\"{0}\") / Schema minuter)"), earningInterval.TimeAccumulator.Name);
            this.Quantity = CalculateQuantity();
        }

        private int CalculateQuantity()
        {
            if (this.Days == 0 ||
                this.WeightMinutes <= 0 ||
                this.AccumulatedEarningMinutes <= 0 ||
                this.ScheduleMinutes == 0
                )
                return 0;

            decimal quantity = Decimal.Divide(this.Days, 7) *
                              this.WeightMinutes *
                              (this.EmploymentRatio > 0 ? this.EmploymentRatio : 1m) *
                              Decimal.Divide(this.AccumulatedEarningMinutes, this.ScheduleMinutes);

            return (int)Math.Round(quantity, MidpointRounding.AwayFromZero);
        }

        public void AddTimeCodeTransaction(TimeCodeTransaction timeCodeTransaction)
        {
            if (this.timeCodeTransactions == null)
                this.timeCodeTransactions = new List<TimeCodeTransaction>();
            this.timeCodeTransactions.Add(timeCodeTransaction);
        }
        public void AddTimePayrollTransaction(TimePayrollTransaction timePayrollTransaction)
        {
            if (this.timePayrollTransactions == null)
                this.timePayrollTransactions = new List<TimePayrollTransaction>();
            this.timePayrollTransactions.Add(timePayrollTransaction);
        }

        public string GetFormulaPlain() => formulaPlain;
        public string GetFormulaExtracted() => $"{Quantity} = ({Days} / 7) * {WeightMinutes} * {EmploymentRatio:0.##} * ({AccumulatedEarningMinutes} / {ScheduleMinutes})";
    }
    public class VacationCompensationAccountingDistributionDTO
    {
        #region Properties

        public decimal VacationCompensationAmount
        {
            get { return this.items.Sum(x => x.Amount); }
        }
        public List<VacationCompensationAccountingDistributionItemDTO> Items
        {
            get { return this.items; }
        }
        public VacationCompensationAccountingDistributionDTO()
        {
            this.items = new List<VacationCompensationAccountingDistributionItemDTO>();
        }
        readonly private List<VacationCompensationAccountingDistributionItemDTO> items;

        #endregion

        #region Ctor

        public void AddDistribution(decimal amount, List<AccountInternal> internalAccounts)
        {
            items.Add(new VacationCompensationAccountingDistributionItemDTO
            {
                Amount = amount,
                AccountInternal = internalAccounts,
            });
        }

        #endregion        
    }
    public class VacationCompensationAccountingDistributionItemDTO
    {
        #region Properties

        public decimal Amount { get; set; }
        public List<AccountInternal> AccountInternal { get; set; }

        #endregion
    }

    #endregion
}

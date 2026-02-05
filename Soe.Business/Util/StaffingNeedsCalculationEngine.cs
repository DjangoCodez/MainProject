using Newtonsoft.Json;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SoftOne.Soe.Business.Util
{
    public class StaffingNeedsCalculationStartEngine
    {
        private List<TimeCodeBreakDTO> TimeCodeBreaks { get; set; }
        public List<TimeBreakTemplateDTO> TimeBreakTemplateDTOs { get; set; }
        private ConcurrentBag<EmployeePostCyclesRun> EmployeePostCyclesRuns { get; set; }
        List<ScheduleCycleRuleDTO> AllScheduleCycleRuleDTOs { get; set; }
        private List<TimeScheduleTaskDTO> TimeScheduleTasks { get; set; }
        private List<IncomingDeliveryHeadDTO> IncomingDeliveryHeads { get; set; }

        public StaffingNeedsCalculationStartEngine(List<TimeBreakTemplateDTO> timeBreakTemplateDTOs, List<TimeCodeBreakDTO> timeCodeBreaks, List<ScheduleCycleRuleDTO> allScheduleCycleRuleDTOs)
        {
            this.TimeBreakTemplateDTOs = timeBreakTemplateDTOs;
            this.TimeCodeBreaks = timeCodeBreaks;
            this.EmployeePostCyclesRuns = new ConcurrentBag<EmployeePostCyclesRun>();
            this.AllScheduleCycleRuleDTOs = allScheduleCycleRuleDTOs;
        }

        public EmployeePostCalculationOutput GenerateScheduleForEmployeePosts(int actorCompanyId, List<StaffingNeedsHeadDTO> needHeads, List<StaffingNeedsHeadDTO> shiftHeads, List<TimeSchedulePlanningDayDTO> shifts, List<EmployeePostDTO> selectedEmployeePosts, List<EmployeePostDTO> allEmployeePosts, List<ShiftTypeDTO> shiftTypes, int timeCodeId, DateTime startDate, int interval, ref SoeProgressInfo info, List<TimeScheduleTaskDTO> timeScheduleTasks = null, List<IncomingDeliveryHeadDTO> incomingDeliverys = null, List<EmployeeGroupDTO> employeeGroups = null, ParameterObject parameterObject = null, List<OpeningHoursDTO> openingHours = null, List<TimeCodeBreakGroupDTO> timeCodeBreakGroups = null, bool connectedToDatabase = true)
        {
            this.TimeScheduleTasks = timeScheduleTasks;
            this.IncomingDeliveryHeads = incomingDeliverys;
            selectedEmployeePosts.ForEach(f => f.WorkTimeWeek = f.GetWorkTimeWeek(interval));

            #region Run
            bool match = false;
            List<List<int>> previousFirstEmployeePostSorts = new List<List<int>>();
            DateTime start = DateTime.UtcNow;
            int count = 1;

            foreach (var repeat in GetSortOrders(selectedEmployeePosts))
            {
                if (!match)
                {
                    EmployeePostCyclesRun run = new EmployeePostCyclesRun(repeat, allEmployeePosts, selectedEmployeePosts);
                    StaffingNeedsCalculationEngine staffingNeedsCalculationEngine = new StaffingNeedsCalculationEngine(this.TimeBreakTemplateDTOs, this.TimeCodeBreaks, this.TimeScheduleTasks, this.IncomingDeliveryHeads, this.AllScheduleCycleRuleDTOs, parameterObject, count, openingHours, timeCodeBreakGroups);
                    staffingNeedsCalculationEngine.ConnectedToDatabase = connectedToDatabase;
                    EmployeePostCalculationInfo employeePostCalculationInfo = new EmployeePostCalculationInfo(repeat, actorCompanyId, needHeads, shiftHeads, shifts, selectedEmployeePosts, allEmployeePosts, shiftTypes, timeCodeId, startDate, interval, timeScheduleTasks: timeScheduleTasks, incomingDeliverys: incomingDeliverys, previousFirstEmployeePostSorts: previousFirstEmployeePostSorts);
                    employeePostCalculationInfo.Number = count;
                    EmployeePostCalculationOutput calculationOutput = staffingNeedsCalculationEngine.GenerateScheduleForEmployeePosts(employeePostCalculationInfo, ref info);
                    previousFirstEmployeePostSorts = calculationOutput.PreviousFirstEmployeePostSorts;

                    List<EmployeePostSort> employeePostSorts = calculationOutput.GetEmployeePostsWithRemainingTime();
                    run.EmployeePostCycles.AddRange(calculationOutput.EmployeePostCycles);

                    if (calculationOutput.RemainingMinutes == 0 || calculationOutput.MeetPercentGoal)
                        match = true;

                    if (match && calculationOutput.RemainingMinutes != 0 && DateTime.UtcNow < start.AddSeconds(10 * selectedEmployeePosts.Count * count))
                        match = false;

                    this.EmployeePostCyclesRuns.Add(run);
                }

                count++;
            }
            #endregion

            StaffingNeedsCalculationEngine engine = new StaffingNeedsCalculationEngine(this.TimeBreakTemplateDTOs, this.TimeCodeBreaks, this.TimeScheduleTasks, this.IncomingDeliveryHeads, this.AllScheduleCycleRuleDTOs, parameterObject, 0, openingHours, timeCodeBreakGroups);

            var cycles = this.EmployeePostCyclesRuns.Where(w => w.NumberOfNotSaved > 0).OrderBy(i => i.RemainingMinutes).ThenBy(i => i.HasEmptyCycles).FirstOrDefault()?.EmployeePostCycles.Where(i => !i.FromSavedData);

            if (cycles != null)
            {
                Parallel.ForEach(cycles, new ParallelOptions { MaxDegreeOfParallelism = 1 }, cycle =>
                {
                    if (cycle.EmployeePost != null && cycle.EmployeePost.ScheduleCycleDTO != null)
                    {
                        int days = cycle.EmployeePost.ScheduleCycleDTO != null ? (int)decimal.Multiply(cycle.EmployeePost.ScheduleCycleDTO.NbrOfWeeks, 7) : 7;
                        cycle.Shifts = engine.CreateTimeSchedulePlanningDayDTO(cycle, cycle.EmployeePost, timeCodeId, days, startDate, startDate.AddDays(days), timeCodeBreakGroups);
                    }
                });
            }
            else
            {
                cycles = new List<EmployeePostCycle>();
            }

            EmployeePostCalculationOutput output = new EmployeePostCalculationOutput(cycles.ToList(), null);
            return output;
        }

        public EmployeePostCalculationOutput GeneratePreAnalysisInformation(int actorCompanyId, List<StaffingNeedsHeadDTO> needHeads, List<StaffingNeedsHeadDTO> shiftHeads, List<TimeSchedulePlanningDayDTO> shifts, EmployeePostDTO selectedEmployeePost, List<EmployeePostDTO> allEmployeePosts, List<ShiftTypeDTO> shiftTypes, int timeCodeId, DateTime startDate, int interval, ref SoeProgressInfo info, List<TimeScheduleTaskDTO> timeScheduleTasks = null, List<IncomingDeliveryHeadDTO> incomingDeliverys = null, List<EmployeeGroupDTO> employeeGroups = null, ParameterObject parameterObject = null, List<OpeningHoursDTO> openingHours = null, List<TimeCodeBreakGroupDTO> timeCodeBreakGroups = null)
        {
            #region Run
            this.TimeScheduleTasks = timeScheduleTasks;
            this.IncomingDeliveryHeads = incomingDeliverys;
            EmployeePostCyclesRun run = new EmployeePostCyclesRun(EmployeePostSortType.CompareScore, allEmployeePosts, new List<EmployeePostDTO>() { selectedEmployeePost });
            StaffingNeedsCalculationEngine staffingNeedsCalculationEngine = new StaffingNeedsCalculationEngine(this.TimeBreakTemplateDTOs, this.TimeCodeBreaks, this.TimeScheduleTasks, this.IncomingDeliveryHeads, this.AllScheduleCycleRuleDTOs, parameterObject, 0, openingHours);
            shifts = shifts.Where(w => w.EmployeePostId != selectedEmployeePost.EmployeePostId).ToList();
            EmployeePostCalculationInfo employeePostCalculationInfo = new EmployeePostCalculationInfo(EmployeePostSortType.CompareScore, actorCompanyId, needHeads, shiftHeads, shifts, new List<EmployeePostDTO>() { selectedEmployeePost }, allEmployeePosts, shiftTypes, timeCodeId, startDate, interval, timeScheduleTasks: timeScheduleTasks, incomingDeliverys: incomingDeliverys);
            EmployeePostCalculationOutput calculationOutput = staffingNeedsCalculationEngine.GenerateScheduleForEmployeePosts(employeePostCalculationInfo, ref info);
            run.EmployeePostCycles.AddRange(calculationOutput.EmployeePostCycles);
            this.EmployeePostCyclesRuns.Add(run);

            #endregion

            StaffingNeedsCalculationEngine engine = new StaffingNeedsCalculationEngine(this.TimeBreakTemplateDTOs, this.TimeCodeBreaks, this.TimeScheduleTasks, this.IncomingDeliveryHeads, this.AllScheduleCycleRuleDTOs, parameterObject, 0, openingHours);
            var cycles = this.EmployeePostCyclesRuns.Where(w => w.NumberOfNotSaved > 0).OrderBy(i => i.RemainingMinutes).ThenBy(i => i.HasEmptyCycles).FirstOrDefault().EmployeePostCycles.Where(i => !i.FromSavedData).ToList();
            EmployeePostCalculationOutput output = new EmployeePostCalculationOutput(cycles, cycles.Any() ? cycles.First().PreAnalysisInformation : new PreAnalysisInformation());
            return output;
        }

        private List<EmployeePostSortType> GetSortOrders(List<EmployeePostDTO> unSortedEmployeePosts)
        {
            unSortedEmployeePosts = unSortedEmployeePosts.Where(w => w.ScheduleCycleDTO != null).ToList();

            if (unSortedEmployeePosts.Count == 1)
                return new List<EmployeePostSortType>() { EmployeePostSortType.WorkTimePerDayDescending };


            List<EmployeePostSortType> orders = new List<EmployeePostSortType>();

            orders.Add(EmployeePostSortType.Uniques);
            orders.Add(EmployeePostSortType.CompareScore);
            orders.Add(EmployeePostSortType.NumberOfPossibleWeeks);

            List<int> sortedEmployeePostsWorkTimePerDayDescending = new List<int>();
            List<int> sortedEmployeePostsWorkTimePercentDescending = new List<int>();
            List<int> sortedEmployeePostsDayOfWeeksCount = new List<int>();
            List<int> sortedEmployeePostsNbrOfWeeks = new List<int>();
            List<int> sortedEmployeePostsNameWorkTimePercentDescending = new List<int>();
            List<int> sortedEmployeePostsNbrOfWeeksDecending = new List<int>();
            Dictionary<EmployeePostSortType, List<int>> dict = new Dictionary<EmployeePostSortType, List<int>>();

            foreach (var type in EnumUtility.GetValues<EmployeePostSortType>())
            {

                switch (type)
                {
                    case EmployeePostSortType.WorkTimePerDayDescending:
                        foreach (var post in unSortedEmployeePosts.OrderByDescending(d => d.WorkTimePerDay))
                            sortedEmployeePostsWorkTimePerDayDescending.Add(post.EmployeePostId);
                        dict.Add(EmployeePostSortType.WorkTimePerDayDescending, sortedEmployeePostsWorkTimePerDayDescending);
                        break;
                    case EmployeePostSortType.WorkTimePercentDescending:
                        foreach (var post in unSortedEmployeePosts.OrderByDescending(d => d.WorkTimePercent))
                        {
                            sortedEmployeePostsWorkTimePercentDescending.Add(post.EmployeePostId);
                        }
                        dict.Add(EmployeePostSortType.WorkTimePercentDescending, sortedEmployeePostsWorkTimePercentDescending);
                        break;
                    case EmployeePostSortType.DayOfWeeksCount:
                        foreach (var post in unSortedEmployeePosts.OrderByDescending(d => d.DayOfWeekIds.Count))
                        {
                            sortedEmployeePostsDayOfWeeksCount.Add(post.EmployeePostId);
                        }
                        dict.Add(EmployeePostSortType.DayOfWeeksCount, sortedEmployeePostsDayOfWeeksCount);
                        break;
                    case EmployeePostSortType.NbrOfWeeks:
                        foreach (var post in unSortedEmployeePosts.Where(w => w.ScheduleCycleDTO != null).OrderBy(d => d.ScheduleCycleDTO.NbrOfWeeks))
                        {
                            sortedEmployeePostsNbrOfWeeks.Add(post.EmployeePostId);
                        }
                        dict.Add(EmployeePostSortType.NbrOfWeeks, sortedEmployeePostsNbrOfWeeks);
                        break;
                    //case EmployeePostSortType.NameWorkTimePercentDescending:
                    //    foreach (var post in unSortedEmployeePosts.OrderByDescending(o => o.Name).ThenByDescending(d => d.WorkTimePercent))
                    //    {
                    //        sortedEmployeePostsNameWorkTimePercentDescending.Add(post.EmployeePostId);
                    //    }
                    //    dict.Add(EmployeePostSortType.NameWorkTimePercentDescending, sortedEmployeePostsNameWorkTimePercentDescending);
                    //    break;
                    case EmployeePostSortType.NbrOfWeeksDecending:
                        foreach (var post in unSortedEmployeePosts.OrderByDescending(d => d.ScheduleCycleDTO.NbrOfWeeks))
                        {
                            sortedEmployeePostsNbrOfWeeksDecending.Add(post.EmployeePostId);
                        }
                        dict.Add(EmployeePostSortType.NbrOfWeeksDecending, sortedEmployeePostsNbrOfWeeksDecending);
                        break;
                }
            }

            Dictionary<EmployeePostSortType, List<int>> filteredDict = new Dictionary<EmployeePostSortType, List<int>>();

            foreach (var pair in dict)
            {
                if (filteredDict.Count == 0)
                {
                    filteredDict.Add(pair.Key, pair.Value);
                    orders.Add(pair.Key);
                    continue;
                }

                foreach (var filteredpair in filteredDict)
                {
                    if (!Enumerable.SequenceEqual(filteredpair.Value, pair.Value) && filteredpair.Key != pair.Key)
                    {
                        filteredDict.Add(pair.Key, pair.Value);
                        orders.Add(pair.Key);
                        break;
                    }
                }
            }

            return orders.Distinct().ToList();

        }
    }

    public class StaffingNeedsCalculationEngine
    {
        #region Public properties

        //public int ActorCompanyId { get; set; }
        public int? DayTypeId { get; set; }
        public DayOfWeek? Weekday { get; set; }
        public DateTime? Date { get; set; }
        public DateTime MinTimeFrom { get; set; }
        public DateTime MaxTimeTo { get; set; }
        public List<StaffingNeedsRowFrequencyDTO> StaffingNeedsRowFrequencyDTOs { get; set; }
        public List<ScheduleCycleRuleDTO> AllScheduleCycleRuleDTOs { get; set; }
        public List<StaffingNeedsRowDTO> StaffingNeedsRowDTOs { get; set; }
        public List<TimeScheduleTemplateHeadDTO> TimeScheduleTemplateHeadDTOs { get; set; }
        public List<TimeBreakTemplateDTO> TimeBreakTemplateDTOs { get; set; }
        public List<int> DisposedEmployeePostIds { get; set; }
        public Dictionary<int, int> BreakMinutesOnLengthDict { get; set; }
        public bool ConnectedToDatabase { get; set; }

        #endregion

        #region Private properties

        private int maxPossibleBreak { get; set; }

        private int MaxPossibleBreak
        {
            get
            {
                if (maxPossibleBreak == 0)
                {
                    if (this.TimeBreakTemplateDTOs != null && this.TimeBreakTemplateDTOs.Count > 0)
                    {
                        maxPossibleBreak = this.TimeBreakTemplateDTOs.OrderByDescending(o => o.BreakLength).FirstOrDefault().BreakLength;
                        return maxPossibleBreak;
                    }
                    else
                        return 120;
                }

                return maxPossibleBreak;
            }
        }

        private bool? breaksIndependentOfTime { get; set; }

        private bool BreaksIndependentOfTime
        {
            get
            {
                if (breaksIndependentOfTime != null)
                {
                    breaksIndependentOfTime = true;
                    return true;
                }
                if (this.TimeBreakTemplateDTOs != null && this.TimeBreakTemplateDTOs.Count > 0)
                {
                    if (this.TimeBreakTemplateDTOs.GroupBy(g => g.ShiftStartFromTime).Count() == 1)
                    {
                        breaksIndependentOfTime = true;
                        return true;
                    }
                }

                breaksIndependentOfTime = false;
                return true;
            }
        }

        private List<PeriodItemsGroupHead> PeriodItemsGroupHeads { get; set; }
        private List<CalculationPeriodItem> NewBreakPeriodItems { get; set; }
        private List<TimeCodeBreakGroupDTO> TimeCodeBreakGroups { get; set; }
        private List<TimeCodeBreakDTO> TimeCodeBreaks { get; set; }

        public List<ShiftTypeDTO> ShiftTypes { get; set; }
        private List<TimeScheduleTaskDTO> TimeScheduleTasks { get; set; }
        private List<IncomingDeliveryHeadDTO> IncomingDeliveryHeads { get; set; }

        private List<IncomingDeliveryRowDTO> incomingDeliveryRows
        {
            get
            {
                if (this.IncomingDeliveryHeads != null)
                    return this.IncomingDeliveryHeads.SelectMany(s => s.Rows).ToList();
                else
                    return new List<IncomingDeliveryRowDTO>();
            }
        }
        private List<OpeningHoursDTO> OpeningHoursDTOs { get; set; }
        private List<TimeBreakInformation> TimeBreakInformations { get; set; }
        private CalculationOptions CalculationOptions { get; set; }
        private TimeBreakTemplateEvaluation BreakEvaluation { get; set; }
        private EmployeePostCyclesRun CurrentEmployeePostCyclesRun { get; set; }
        private List<EmployeePostCyclesRun> EmployeePostCyclesRuns { get; set; }

        private List<int> PercentageDone { get; set; }
        private List<int> MaxLengthShift { get; set; }

        private int MaxNumberOfPossibleWeeks
        {
            get
            {
                return 50;
            }
        }

        #endregion

        #region Ctor

        public StaffingNeedsCalculationEngine(ParameterObject parameterObject)
        {
            this.ConnectedToDatabase = true;
        }

        public StaffingNeedsCalculationEngine(List<TimeBreakTemplateDTO> timeBreakTemplateDTOs, ParameterObject parameterObject, List<OpeningHoursDTO> openingHours = null)
        {
            this.MinTimeFrom = CalendarUtility.DATETIME_DEFAULT;
            this.MaxTimeTo = CalendarUtility.GetEndOfDay(this.MinTimeFrom);
            this.TimeBreakTemplateDTOs = timeBreakTemplateDTOs;
            this.StaffingNeedsRowFrequencyDTOs = new List<StaffingNeedsRowFrequencyDTO>();
            this.BreakMinutesOnLengthDict = new Dictionary<int, int>();
            this.OpeningHoursDTOs = openingHours;
            this.PeriodItemsGroupHeads = new List<PeriodItemsGroupHead>();
            this.NewBreakPeriodItems = new List<CalculationPeriodItem>();
            this.CalculationOptions = new CalculationOptions();
            this.BreakEvaluation = new TimeBreakTemplateEvaluation();
            this.EmployeePostCyclesRuns = new List<EmployeePostCyclesRun>();
            this.CurrentEmployeePostCyclesRun = new EmployeePostCyclesRun(EmployeePostSortType.Unknown, new List<EmployeePostDTO>(), new List<EmployeePostDTO>());
            this.ShiftTypes = new List<ShiftTypeDTO>();
            this.DisposedEmployeePostIds = new List<int>();
            this.ConnectedToDatabase = true;
        }

        public StaffingNeedsCalculationEngine(List<TimeBreakTemplateDTO> timeBreakTemplateDTOs, List<TimeCodeBreakDTO> timeCodeBreaks, List<TimeScheduleTaskDTO> timeScheduleTasks, List<IncomingDeliveryHeadDTO> incomingDeliveryHeads, List<ScheduleCycleRuleDTO> allScheduleCycleRuleDTOs, ParameterObject parameterObject, int number = 0, List<OpeningHoursDTO> openingHours = null, List<TimeCodeBreakGroupDTO> timeCodeBreakGroups = null)
        {
            this.MinTimeFrom = CalendarUtility.DATETIME_DEFAULT;
            this.MaxTimeTo = CalendarUtility.GetEndOfDay(this.MinTimeFrom);
            this.TimeBreakTemplateDTOs = timeBreakTemplateDTOs;
            this.TimeCodeBreaks = timeCodeBreaks;
            this.OpeningHoursDTOs = openingHours;
            this.StaffingNeedsRowFrequencyDTOs = new List<StaffingNeedsRowFrequencyDTO>();
            this.BreakMinutesOnLengthDict = new Dictionary<int, int>();
            this.PeriodItemsGroupHeads = new List<PeriodItemsGroupHead>();
            this.NewBreakPeriodItems = new List<CalculationPeriodItem>();
            this.CalculationOptions = new CalculationOptions();
            this.BreakEvaluation = new TimeBreakTemplateEvaluation();
            this.EmployeePostCyclesRuns = new List<EmployeePostCyclesRun>();
            this.CurrentEmployeePostCyclesRun = new EmployeePostCyclesRun(EmployeePostSortType.Unknown, new List<EmployeePostDTO>(), new List<EmployeePostDTO>());
            this.CurrentEmployeePostCyclesRun.Number = number;
            this.ShiftTypes = new List<ShiftTypeDTO>();
            this.DisposedEmployeePostIds = new List<int>();
            this.AllScheduleCycleRuleDTOs = allScheduleCycleRuleDTOs;
            this.TimeScheduleTasks = timeScheduleTasks;
            this.IncomingDeliveryHeads = incomingDeliveryHeads;
            this.TimeCodeBreakGroups = timeCodeBreakGroups;
            this.ConnectedToDatabase = true;
        }

        public StaffingNeedsCalculationEngine(List<TimeBreakTemplateDTO> timeBreakTemplateDTOs, List<TimeScheduleTaskDTO> timeScheduleTasks, List<IncomingDeliveryHeadDTO> incomingDeliveryHeads, DateTime minFromTime, DateTime maxTimeTo, ParameterObject parameterObject, List<OpeningHoursDTO> openingHours = null)
        {
            this.OpeningHoursDTOs = openingHours;
            this.MinTimeFrom = minFromTime;
            this.MaxTimeTo = maxTimeTo;
            this.TimeBreakTemplateDTOs = timeBreakTemplateDTOs;
            this.StaffingNeedsRowFrequencyDTOs = new List<StaffingNeedsRowFrequencyDTO>();
            this.BreakMinutesOnLengthDict = new Dictionary<int, int>();
            this.PeriodItemsGroupHeads = new List<PeriodItemsGroupHead>();
            this.NewBreakPeriodItems = new List<CalculationPeriodItem>();
            this.CalculationOptions = new CalculationOptions();
            this.BreakEvaluation = new TimeBreakTemplateEvaluation();
            this.EmployeePostCyclesRuns = new List<EmployeePostCyclesRun>();
            this.CurrentEmployeePostCyclesRun = new EmployeePostCyclesRun(EmployeePostSortType.Unknown, new List<EmployeePostDTO>(), new List<EmployeePostDTO>());
            this.ShiftTypes = new List<ShiftTypeDTO>();
            this.DisposedEmployeePostIds = new List<int>();
            this.TimeScheduleTasks = timeScheduleTasks;
            this.IncomingDeliveryHeads = incomingDeliveryHeads;
            this.ConnectedToDatabase = true;

        }

        #endregion

        #region Public methods

        public void InetBeforeEmployeePostDay(DateTime date, bool allowUnDocked = false)
        {
            this.PeriodItemsGroupHeads = new List<PeriodItemsGroupHead>();
            this.TimeBreakInformations = new List<TimeBreakInformation>();
            this.NewBreakPeriodItems = new List<CalculationPeriodItem>();
            if (allowUnDocked)
            {
                this.CalculationOptions.AddOnlyDockedPeriodItems = false;
                this.CalculationOptions.ForceAddOnlyDockedPeriodItems = false;
            }
            else
            {
                this.CalculationOptions.AddOnlyDockedPeriodItems = true;
                this.CalculationOptions.ForceAddOnlyDockedPeriodItems = true;
            }

            this.Date = date;
        }

        public StaffingNeedsHeadDTO GenerateStaffingNeedsShifts(StaffingNeedsCalculationInput input)
        {
            //Set rules
            this.CalculationOptions.Interval = input.CalculationOptions.Interval;
            this.CalculationOptions.ApplyLengthRule = true;

            //Split according to workrules
            List<CalculationPeriodItem> periodItems = input.StaffingNeedsRows.ToCalculationPeriodItems(input.ShiftTypes, input.TimeScheduleTasks, input.IncomingDeliveryHeads);
            List<CalculationPeriodItem> periodItemsSplitted = SplitShifts(periodItems);

            //Create Shifts
            List<CalculationPeriodItem> staffingNeedsShift = FillStaffingNeedsShifts(periodItemsSplitted);
            List<CalculationPeriodItem> staffingNeedsShiftMerged = MergeShifts(staffingNeedsShift, true);
            List<StaffingNeedsRowDTO> staffingNeedsRows = staffingNeedsShiftMerged.ToStaffingNeedsRowDTOs(input.ShiftTypes, input.TimeScheduleTasks, input.IncomingDeliveryHeads);

            StaffingNeedsHeadDTO headDTO = new StaffingNeedsHeadDTO()
            {
                Date = input.Date,
                DayTypeId = input.DayTypeId,
                Weekday = input.DayOfWeek,
                Name = input.Name,
                Rows = new List<StaffingNeedsRowDTO>()
            };

            this.StaffingNeedsRowDTOs = CloneWithNoIds(staffingNeedsRows);
            headDTO.Rows.AddRange(CloneWithNoIds(this.StaffingNeedsRowDTOs));

            return headDTO;
        }

        public void GenerateStaffingNeedsRows(StaffingNeedsCalculationInput input)
        {
            if (input.TimeScheduleTasks.IsNullOrEmpty() && input.IncomingDeliveryHeads.IsNullOrEmpty() && input.StaffingNeedsAnalysisChartData.IsNullOrEmpty())
                return;

            this.CalculationOptions.Interval = input.CalculationOptions.Interval;

            List<CalculationPeriodItem> periodItems = new List<CalculationPeriodItem>();

            #region StaffingNeedsAnalysisChartData

            if (input.StaffingNeedsAnalysisChartData != null && input.StaffingNeedsAnalysisChartData.Count > 0)
            {
                periodItems.AddRange(GenerateStaffingNeedsPeriodItems(this.CalculationOptions.Interval, input.StaffingNeedsAnalysisChartData, input.ShiftTypeId, input.ShiftTypes == null ? new List<ShiftTypeDTO>() : input.ShiftTypes, staffingNeedsLocationGroupDTOs: input.StaffingNeedsLocationGroups, timeScheduleTaskDTOs: input.TimeScheduleTasks));

                if (input.ShiftTypeId == null)
                {
                    if (input.TimeScheduleTasks != null)
                    {
                        var isStaffingNeedsFrequencyTask = input.TimeScheduleTasks.FirstOrDefault(w => w.IsStaffingNeedsFrequency);

                        if (isStaffingNeedsFrequencyTask != null)
                        {
                            input.ShiftTypeId = isStaffingNeedsFrequencyTask.ShiftTypeId;
                        }
                    }
                }

                this.StaffingNeedsRowFrequencyDTOs = GenerateStaffingNeedsRowFrequencyItems(this.CalculationOptions.Interval, input.StaffingNeedsAnalysisChartData, input.ShiftTypeId);
            }

            #endregion

            #region TimeScheduleTasks

            if (input.TimeScheduleTasks != null && input.TimeScheduleTasks.Count > 0 && ((input.ForceIncludeTimeScheduleTaskItems.HasValue && input.ForceIncludeTimeScheduleTaskItems.Value) || input.StaffingNeedsAnalysisChartData == null))
                periodItems.AddRange(GenerateStaffingNeedsPeriodItems(input.CalculationOptions.Interval, input.ShiftTypes == null ? new List<ShiftTypeDTO>() : input.ShiftTypes, input.TimeScheduleTasks));

            #endregion

            #region IncomingDeliveryHeads

            if (!input.IncomingDeliveryHeads.IsNullOrEmpty())
                periodItems.AddRange(GenerateStaffingNeedsPeriodItems(input.CalculationOptions.Interval, input.ShiftTypes == null ? new List<ShiftTypeDTO>() : input.ShiftTypes, input.IncomingDeliveryHeads));

            #endregion

            #region Merge rows from staffingNeedsAnalysisChartData into rows from TimeScheduleTask

            List<CalculationPeriodItem> mergedStaffingNeedsPeriodItems = MergeShiftFromChartData(periodItems);

            #endregion

            #region Mountain

            this.CalculationOptions.MixShiftTypesOnHead = false;
            this.StaffingNeedsRowDTOs = BuildMountain(mergedStaffingNeedsPeriodItems);

            #endregion
        }

        //public EmployeePostCalculationOutput GenerateScheduleForEmployeePosts(int actorCompanyId, List<StaffingNeedsHeadDTO> needHeads, List<StaffingNeedsHeadDTO> shiftHeads, List<TimeSchedulePlanningDayDTO> shifts, List<EmployeePostDTO> selectedEmployeePosts, List<EmployeePostDTO> allEmployeePosts, List<ShiftTypeDTO> shiftTypes, int timeCodeId, DateTime startDate, int interval, List<TimeScheduleTaskDTO> timeScheduleTasks = null, List<IncomingDeliveryHeadDTO> incomingDeliverys = null, List<EmployeeGroupDTO> employeeGroups = null, List<int> filteredPeriodIds = null)
        //{
        //    #region Run

        //    bool ignoreFreeDays = false;
        //    int repeats = 9;//Math.Pow(selectedEmployeePosts.Count, 2) < 9 ? Convert.ToInt32(Math.Pow(selectedEmployeePosts.Count, 2)) : 9;

        //    foreach (var repeat in NumberUtility.GenerateIntListFromInterval(1, repeats))
        //    {
        //        if ((CurrentEmployeePostCyclesRun.RemainingMinutes != 0 || CurrentEmployeePostCyclesRun.HasEmptyCycles || CurrentEmployeePostCyclesRun.EmployeePostCycles.Count == 0))
        //        {
        //            EmployeePostCalculationInfo info = new EmployeePostCalculationInfo(repeat, actorCompanyId, needHeads, shiftHeads, shifts, selectedEmployeePosts, allEmployeePosts, shiftTypes, timeCodeId, startDate, interval, timeScheduleTasks: timeScheduleTasks, incomingDeliverys: incomingDeliverys, filteredPeriodIds: filteredPeriodIds);

        //            GenerateScheduleForEmployeePostsInner(info);
        //            this.EmployeePostCyclesRuns.Add(CurrentEmployeePostCyclesRun);
        //        }

        //        ignoreFreeDays = !ignoreFreeDays;
        //    }

        //    #endregion

        //    var cycles = this.EmployeePostCyclesRuns.Where(w => w.NumberOfNotSaved > 0).OrderBy(i => i.RemainingMinutes).ThenBy(i => i.HasEmptyCycles).FirstOrDefault().EmployeePostCycles.Where(i => !i.FromSavedData).ToList();

        //    foreach (var cycle in cycles)
        //    {
        //        if (cycle.EmployeePost != null && cycle.EmployeePost.ScheduleCycleDTO != null)
        //        {
        //            int days = cycle.EmployeePost.ScheduleCycleDTO != null ? (int)decimal.Multiply(cycle.EmployeePost.ScheduleCycleDTO.NbrOfWeeks, 7) : 7;
        //            cycle.Shifts = CreateTimeSchedulePlanningDayDTO(cycle, cycle.EmployeePost, timeCodeId, days, startDate, startDate.AddDays(days));
        //        }
        //    }

        //    EmployeePostCalculationOutput output = new EmployeePostCalculationOutput(cycles);
        //    return output;
        //}

        public EmployeePostCalculationOutput GenerateScheduleForEmployeePosts(EmployeePostCalculationInfo employeePostCalculationInfo, ref SoeProgressInfo info)
        {
            if ((CurrentEmployeePostCyclesRun.RemainingMinutes != 0 || CurrentEmployeePostCyclesRun.HasEmptyCycles || CurrentEmployeePostCyclesRun.EmployeePostCycles.Count == 0))
            {
                this.CurrentEmployeePostCyclesRun.PreviousFirstEmployeePostSorts = employeePostCalculationInfo.PreviousFirstEmployeePostSorts;
                this.ShiftTypes = employeePostCalculationInfo.ShiftTypes;
                GenerateScheduleForEmployeePostsInner(employeePostCalculationInfo, ref info);
                this.CurrentEmployeePostCyclesRun.EmployeePostCyclesRunInformation.Stop();
                this.EmployeePostCyclesRuns.Add(CurrentEmployeePostCyclesRun);
            }

            EmployeePostCalculationOutput output = new EmployeePostCalculationOutput(this.CurrentEmployeePostCyclesRun.EmployeePostCycles, null);
            this.CurrentEmployeePostCyclesRun.EmployeePostCyclesRunInformation.LogInfo($"{DateTime.UtcNow} EmployeePostSortType: {(int)employeePostCalculationInfo.EmployeePostSortType} - {employeePostCalculationInfo.EmployeePostSortType} Percent: {output.Percent} MeetPercentGoal: {output.MeetPercentGoal}");
            output.PreviousFirstEmployeePostSorts = this.CurrentEmployeePostCyclesRun.PreviousFirstEmployeePostSorts;
            output.PreviousFirstEmployeePostSorts.Add(this.CurrentEmployeePostCyclesRun.FirstEmployeePostSort);
            this.CurrentEmployeePostCyclesRun.EmployeePostCyclesRunInformation.EmployeePostCycleInformations = this.CurrentEmployeePostCyclesRun.EmployeePostCycles.Select(s => s.EmployeePostCycleInformation).ToList();
            LogInfo(this.CurrentEmployeePostCyclesRun.EmployeePostCyclesRunInformation.GetInformationLog(clearLog: true));
            LogInfo(JsonConvert.SerializeObject(this.CurrentEmployeePostCyclesRun.EmployeePostCyclesRunInformation, new JsonSerializerSettings() { MaxDepth = 10 }));
            return output;
        }

        public void GenerateScheduleForEmployeePostsInner(EmployeePostCalculationInfo employeePostCalculationInfo, ref SoeProgressInfo info)
        {
            //this.ActorCompanyId = employeePostCalculationInfo.ActorCompanyId;
            int maxNumberOfWeeks = GetFirstCommonNumberOfWeeks(employeePostCalculationInfo.AllEmployeePosts);
            this.CurrentEmployeePostCyclesRun = new EmployeePostCyclesRun(employeePostCalculationInfo.EmployeePostSortType, employeePostCalculationInfo.AllEmployeePosts, employeePostCalculationInfo.SelectedEmployeePosts);
            this.CurrentEmployeePostCyclesRun.Number = employeePostCalculationInfo.Number;

            #region Set ShiftTYpes

            SetValidShiftTypes(employeePostCalculationInfo.AllEmployeePosts);
            SetValidShiftTypes(employeePostCalculationInfo.SelectedEmployeePosts);

            #endregion

            #region Get unhandled need

            this.CalculationOptions = new CalculationOptions(employeePostCalculationInfo.Interval, true);
            this.CalculationOptions.MaxNumberOfWeeks = maxNumberOfWeeks;
            Dictionary<int, int> personsOnDelivery = new Dictionary<int, int>();
            Dictionary<int, int> personsOnTask = new Dictionary<int, int>();

            foreach (StaffingNeedsHeadDTO head in employeePostCalculationInfo.NeedHeads)
            {
                foreach (StaffingNeedsRowDTO row in head.Rows)
                {
                    row.Weekday = head.Weekday;
                    row.Date = head.Date;
                    row.DayTypeId = head.DayTypeId;
                    foreach (StaffingNeedsRowPeriodDTO period in row.Periods)
                    {
                        period.TimeSlot = null;
                    }
                }
                employeePostCalculationInfo.NeedPeriodItems.AddRange(head.Rows.ToCalculationPeriodItems(employeePostCalculationInfo.ShiftTypes, employeePostCalculationInfo.TimeScheduleTasks, employeePostCalculationInfo.IncomingDeliverys, newGuidOnPeriod: true, personsOnDelivery: personsOnDelivery, personsOnTask: personsOnTask));
            }

            #region Create EmployeePostCycles from saved Data

            personsOnDelivery = personsOnDelivery.ToDictionary(x => x.Key, x => x.Value - 1);
            personsOnTask = personsOnTask.ToDictionary(x => x.Key, x => x.Value - 1);
            personsOnDelivery = personsOnDelivery.Where(s => s.Value > 0).ToDictionary(x => x.Key, x => x.Value);
            personsOnTask = personsOnTask.Where(s => s.Value > 0).ToDictionary(x => x.Key, x => x.Value);

            List<CalculationPeriodItem> shiftPeriodItems = new List<CalculationPeriodItem>();
            foreach (StaffingNeedsHeadDTO head in employeePostCalculationInfo.ShiftHeads)
            {
                shiftPeriodItems.AddRange(head.Rows.ToCalculationPeriodItems(employeePostCalculationInfo.ShiftTypes, employeePostCalculationInfo.TimeScheduleTasks, employeePostCalculationInfo.IncomingDeliverys, personsOnDelivery: personsOnDelivery, personsOnTask: personsOnTask));
            }

            this.CurrentEmployeePostCyclesRun.EmployeePostCycles.AddRange(CreateEmployeePostCyclesFromSavedData(employeePostCalculationInfo.StartDate, employeePostCalculationInfo.AllEmployeePosts, employeePostCalculationInfo.Shifts, shiftPeriodItems, maxNumberOfWeeks));


            #endregion


            #endregion

            #region Find unique skills

            this.CurrentEmployeePostCyclesRun.UniqueShiftypeIds = GetShiftTypeIdsValidForOneEmployeePost(employeePostCalculationInfo.AllEmployeePosts, employeePostCalculationInfo.NeedPeriodItems, employeePostCalculationInfo.ShiftTypes);
            this.CurrentEmployeePostCyclesRun.PrioShiftTypesIds = GetShiftTypeIdsWithPrio(employeePostCalculationInfo.AllEmployeePosts, employeePostCalculationInfo.NeedPeriodItems, employeePostCalculationInfo.ShiftTypes);

            #endregion

            CreateAndAddEmployeePostCyclesRecursive(employeePostCalculationInfo.SelectedEmployeePosts, employeePostCalculationInfo.StartDate, employeePostCalculationInfo.NeedPeriodItems, employeePostCalculationInfo.ShiftTypes, employeePostCalculationInfo.TimeCodeId, employeePostCalculationInfo.EmployeePostSortType, employeePostCalculationInfo.EmployeePostSort, ref info);

        }

        private void CreateAndAddEmployeePostCyclesRecursive(List<EmployeePostDTO> employeePosts, DateTime startDate, List<CalculationPeriodItem> needPeriodItems, List<ShiftTypeDTO> shiftTypes, int timeCodeId, EmployeePostSortType employeePostSortType, List<EmployeePostSort> employeePostSorts, ref SoeProgressInfo info, bool first = true)
        {
            var sorted = SortEmployeePostDTOs(employeePosts, startDate, needPeriodItems, employeePostSortType, employeePostSorts, first, ref info);
            if (sorted.Count > 0 && !info.Abort)
            {
                string number = $"{(employeePosts.Count - sorted.Count) + 1} / {employeePosts.Count}";

                string progressMessage = GetText(11643, "Nu bearbetas") + $"  {number} (#{this.CurrentEmployeePostCyclesRun.Number}) " + Environment.NewLine + GetText(11544, "Tjänst") + ": " + sorted.First().EmployeePost.Name;
                LogInfo(progressMessage);
                var lastEmployeePostCycle = this.CurrentEmployeePostCyclesRun != null ? this.CurrentEmployeePostCyclesRun.EmployeePostCycles.LastOrDefault() : null;

                if (!first && lastEmployeePostCycle != null)
                {
                    progressMessage += Environment.NewLine + Environment.NewLine + GetText(11643, "Föregående bearbetad") + ": ";
                    progressMessage += lastEmployeePostCycle.EmployeePostCycleInformation.GetInformationForProgress(this.ConnectedToDatabase);
                }

                info.Message = progressMessage;

                var employeePost = sorted.First().EmployeePost;
                int days = employeePost.ScheduleCycleDTO != null ? (int)decimal.Multiply(employeePost.ScheduleCycleDTO.NbrOfWeeks, 7) : 7;
                CreateAndAddEmployeePostCycle(employeePost, needPeriodItems, sorted.First().PossibleWeeks, shiftTypes, days, timeCodeId, startDate, this.CalculationOptions.MaxNumberOfWeeks, sorted.First().DayOfWeekOrders);

                lastEmployeePostCycle = this.CurrentEmployeePostCyclesRun != null ? this.CurrentEmployeePostCyclesRun.EmployeePostCycles.LastOrDefault() : null;
                if (lastEmployeePostCycle != null)
                {
                    lastEmployeePostCycle.LogInformation();
                }

                CreateAndAddEmployeePostCyclesRecursive(employeePosts, startDate, needPeriodItems, shiftTypes, timeCodeId, employeePostSortType, employeePostSorts, ref info, first: false);
            }
        }

        private List<EmployeePostSort> SortEmployeePostDTOs(List<EmployeePostDTO> employeePosts, DateTime startDate, List<CalculationPeriodItem> needPeriodItems, EmployeePostSortType employeePostSortType, List<EmployeePostSort> employeePostSorts, bool first, ref SoeProgressInfo info)
        {
            List<EmployeePostSort> sortedEmployeePosts = new List<EmployeePostSort>();
            List<EmployeePostSort> unSortedEmployeePosts = new List<EmployeePostSort>();
            bool ignoreFreeDays = false;
            List<List<CalculationPeriodItem>> allPossibleWeeks = new List<List<CalculationPeriodItem>>();
            startDate = startDate.Date;

            foreach (var employeePostSort in GetPostsForNextSort(employeePosts))
            {
                if (employeePostSort.EmployeePost.ScheduleCycleDTO == null)
                {
                    info.Message += GetText(11644, "Tjänsten") + $" {employeePostSort.EmployeePost.Name} " + GetText(11645, "har ingen cykel");
                }

                SetCalculationOptions(employeePostSort.EmployeePost, this.CalculationOptions.MaxNumberOfWeeks, this.CalculationOptions.OpeningHours);
                EmployeePostCycle employeePostCycle = new EmployeePostCycle(employeePostSort.EmployeePost, startDate, this.CalculationOptions, this.AllScheduleCycleRuleDTOs);
                employeePostCycle.SetFreeDays(ignoreFreeDays, 0, false);
                List<CalculationPeriodItem> employeePostNeedPeriodItems = ClonePeriodItems(needPeriodItems);
                employeePostNeedPeriodItems = FilterValidPeriodItems(employeePostNeedPeriodItems, employeePostSort.EmployeePost);
                List<List<CalculationPeriodItem>> possibleWeeks = new List<List<CalculationPeriodItem>>();

                // if (needPeriodItems.Where(i => i.IsNetTime).Count > needPeriodItems.Count * 0.7)

                for (int i = 0; i < employeePostCycle.EmployeePostWeeks.Count; i++)
                {
                    if (i == 0)  //Only first week for now
                    {
                        EmployeePostWeekDayOfWeekOrder orderOfWeekDay = employeePostCycle.GetWeekDayOrder(employeePostCycle.EmployeePostWeeks[i].WeekNumber, ignoreFreeDays);
                        EmployeePostWeek copy = employeePostCycle.EmployeePostWeeks[i].Copy(0, employeePostCycle.EmployeePostWeeks[i].WeekNumber);
                        var postPossibleWeeks = GetPeriodItemsMatchingWeekTime(SetScheduleDate(ClonePeriodItems(employeePostNeedPeriodItems), copy.StartDate), copy.RemainingMinutesWeek, copy.StartDate, employeePostSort.EmployeePost, orderOfWeekDay.DayOrWeeksOrder, employeePostCycle.EmployeePostWeeks.Count, ignoreFreeDays);
                        employeePostSort.PossibleWeeks.AddRange(PrioPossibleWeeks(postPossibleWeeks));
                        allPossibleWeeks.AddRange(postPossibleWeeks);
                    }
                }

                unSortedEmployeePosts.Add(employeePostSort);
            }

            if (allPossibleWeeks.Any())
            {
                List<List<DayOfWeek>> orders = GetOrders(allPossibleWeeks);

                foreach (var post in unSortedEmployeePosts)
                {
                    post.DayOfWeekOrders = new List<List<DayOfWeek>>();
                    post.DayOfWeekOrders.AddRange(orders.Where(w => w.Count > 0));
                }
            }

            switch (employeePostSortType)
            {
                case EmployeePostSortType.Uniques:
                    SetUniques(unSortedEmployeePosts, needPeriodItems);
                    SetNumberOfPossibleQualifiedShiftTypes(unSortedEmployeePosts);
                    foreach (var post in unSortedEmployeePosts.Where(d => d.Uniques > 0).OrderBy(i => i.Uniques).ThenBy(i => i.NumberOfQualifiedShiftTypes).ThenBy(i => i.NumberOfPossibleShiftTypes).ThenByDescending(t => t.EmployeePost.WorkTimePercent))
                        sortedEmployeePosts.Add(post);
                    foreach (var post in unSortedEmployeePosts.Where(d => d.Uniques == 0).OrderBy(i => i.NumberOfQualifiedShiftTypes).ThenBy(i => i.NumberOfPossibleShiftTypes).ThenByDescending(i => i.EmployeePost.WorkTimePerDay).ThenByDescending(t => t.EmployeePost.WorkTimePercent))
                        sortedEmployeePosts.Add(post);
                    break;
                case EmployeePostSortType.CompareScore:
                    SetUniques(unSortedEmployeePosts, needPeriodItems);
                    SetNumberOfPossibleQualifiedShiftTypes(unSortedEmployeePosts);
                    foreach (var post in unSortedEmployeePosts.OrderBy(i => i.CompareScore).ThenBy(i => i.NumberOfQualifiedShiftTypes).ThenBy(i => i.NumberOfPossibleShiftTypes).ThenByDescending(t => t.EmployeePost.WorkTimePercent))
                        sortedEmployeePosts.Add(post);
                    break;
                case EmployeePostSortType.NumberOfPossibleWeeks:
                    foreach (var post in unSortedEmployeePosts.Where(d => d.NumberOfPossibleWeeks > 0).OrderBy(i => i.NumberOfPossibleWeeks).ThenByDescending(t => t.EmployeePost.WorkTimePercent))
                        sortedEmployeePosts.Add(post);
                    foreach (var post in unSortedEmployeePosts.Where(d => d.NumberOfPossibleWeeks == 0).OrderByDescending(t => t.EmployeePost.WorkTimePercent))
                        sortedEmployeePosts.Add(post);
                    break;
                case EmployeePostSortType.WorkTimePerDayDescending:
                    foreach (var post in unSortedEmployeePosts.OrderByDescending(d => d.EmployeePost.WorkTimePerDay).ThenByDescending(t => t.EmployeePost.WorkTimePercent))
                        sortedEmployeePosts.Add(post);
                    break;
                case EmployeePostSortType.WorkTimePercentDescending:
                    foreach (var post in unSortedEmployeePosts.OrderByDescending(d => d.EmployeePost.WorkTimePercent))
                    {
                        post.PossibleWeeks = post.PossibleWeeks.Take(Convert.ToInt32(post.NumberOfPossibleWeeks / 4)).ToList();
                        sortedEmployeePosts.Add(post);
                    }
                    break;
                case EmployeePostSortType.DayOfWeeksCount:
                    foreach (var post in unSortedEmployeePosts.OrderBy(d => d.EmployeePost.DayOfWeekIds.Count).ThenByDescending(t => t.EmployeePost.WorkTimePercent))
                    {
                        post.PossibleWeeks = post.PossibleWeeks.Take(Convert.ToInt32(post.NumberOfPossibleWeeks > 10 ? 10 : post.NumberOfPossibleWeeks)).ToList();
                        sortedEmployeePosts.Add(post);
                    }
                    break;
                case EmployeePostSortType.NbrOfWeeks:
                    foreach (var post in unSortedEmployeePosts.OrderBy(d => d.EmployeePost.ScheduleCycleDTO.NbrOfWeeks).ThenByDescending(t => t.EmployeePost.WorkTimePercent))
                    {
                        post.PossibleWeeks = post.PossibleWeeks.Take(Convert.ToInt32(post.NumberOfPossibleWeeks / 8)).ToList();
                        sortedEmployeePosts.Add(post);
                    }
                    break;
                case EmployeePostSortType.NameWorkTimePercentDescending:
                    foreach (var post in unSortedEmployeePosts.OrderByDescending(o => o.EmployeePost.Name).ThenByDescending(d => d.EmployeePost.WorkTimePercent))
                    {
                        post.PossibleWeeks = post.PossibleWeeks.Take(Convert.ToInt32(post.NumberOfPossibleWeeks > 10 ? 10 : post.NumberOfPossibleWeeks)).ToList();
                        sortedEmployeePosts.Add(post);
                    }
                    break;
                case EmployeePostSortType.NbrOfWeeksDecending:
                    foreach (var post in unSortedEmployeePosts.OrderByDescending(d => d.EmployeePost.ScheduleCycleDTO.NbrOfWeeks))
                    {
                        post.PossibleWeeks = post.PossibleWeeks.Take(Convert.ToInt32(post.NumberOfPossibleWeeks > 10 ? 10 : post.NumberOfPossibleWeeks)).ToList();
                        sortedEmployeePosts.Add(post);
                    }
                    break;
            }

            if (employeePostSorts.Any())
            {
                List<EmployeePostSort> old = sortedEmployeePosts;
                sortedEmployeePosts = new List<EmployeePostSort>();
                foreach (var post in employeePostSorts.OrderByDescending(o => o.RemainingMinutes))
                {
                    var oldPost = old.FirstOrDefault(i => i.EmployeePost.EmployeePostId == post.EmployeePost.EmployeePostId);
                    if (oldPost != null)
                    {
                        sortedEmployeePosts.Add(oldPost);
                        old.Remove(oldPost);
                    }
                }

                foreach (var post in old)
                    sortedEmployeePosts.Add(post);
            }

            var sorted = sortedEmployeePosts.Where(w => !this.DisposedEmployeePostIds.Contains(w.EmployeePost.EmployeePostId));

            if (sorted.Any())
            {
                sortedEmployeePosts = sorted.ToList();

                foreach (var post in sortedEmployeePosts)
                {
                    List<DayOfWeek> dayOfWeeks = new List<DayOfWeek>();
                    dayOfWeeks.Add(DayOfWeek.Sunday);
                    dayOfWeeks.Add(DayOfWeek.Saturday);
                    dayOfWeeks.Add(DayOfWeek.Friday);
                    dayOfWeeks.Add(DayOfWeek.Thursday);
                    dayOfWeeks.Add(DayOfWeek.Wednesday);
                    dayOfWeeks.Add(DayOfWeek.Tuesday);
                    dayOfWeeks.Add(DayOfWeek.Monday);

                    post.DayOfWeekOrders.Add(dayOfWeeks);

                    List<DayOfWeek> dayOfWeeks2 = new List<DayOfWeek>();
                    dayOfWeeks2.Add(DayOfWeek.Tuesday);
                    dayOfWeeks2.Add(DayOfWeek.Wednesday);
                    dayOfWeeks2.Add(DayOfWeek.Thursday);
                    dayOfWeeks2.Add(DayOfWeek.Monday);
                    dayOfWeeks2.Add(DayOfWeek.Friday);
                    dayOfWeeks2.Add(DayOfWeek.Saturday);
                    dayOfWeeks2.Add(DayOfWeek.Sunday);

                    post.DayOfWeekOrders.Add(dayOfWeeks);
                }
            }
            if (sortedEmployeePosts.Where(w => w.HasBeenDisposed).Any())
            {
                var disposed = sortedEmployeePosts.Where(w => w.HasBeenDisposed).ToList();
                var notDisposed = sortedEmployeePosts.Where(w => !w.HasBeenDisposed).ToList();
                sortedEmployeePosts = new List<EmployeePostSort>();
                sortedEmployeePosts.AddRange(notDisposed);
                sortedEmployeePosts.AddRange(disposed);
            }

            if (first)
            {
                this.CurrentEmployeePostCyclesRun.FirstEmployeePostSort.AddRange(sortedEmployeePosts.Select(s => s.EmployeePost.EmployeePostId));

                if (this.CurrentEmployeePostCyclesRun.OrderAlreadyUsed(sortedEmployeePosts))
                    sortedEmployeePosts = new List<EmployeePostSort>();
            }

            #region Logging

            foreach (var post in sortedEmployeePosts)
                this.CurrentEmployeePostCyclesRun.EmployeePostCyclesRunInformation.LogInfo($"{DateTime.UtcNow} SortType: {(int)employeePostSortType} - {employeePostSortType} Name: {post.EmployeePost.Name} WorkDays:{post.EmployeePost.WorkDaysWeek} WorkTimePerDay:{post.EmployeePost.WorkTimePerDay} Possible:{post.NumberOfPossibleWeeks} Uniques: {post.Uniques} ShiftTypesQ: {post.NumberOfQualifiedShiftTypes} ShiftTypesP: {post.NumberOfPossibleShiftTypes} Disposed: {post.HasBeenDisposed}");

            LogInfo(this.CurrentEmployeePostCyclesRun.EmployeePostCyclesRunInformation.GetInformationLog(clearLog: true));

            #endregion

            return sortedEmployeePosts;
        }

        private List<EmployeePostSort> GetPostsForNextSort(List<EmployeePostDTO> employeePosts)
        {
            List<EmployeePostSort> employeePostSorts = new List<EmployeePostSort>();

            if (employeePosts.Count == 1 && this.CurrentEmployeePostCyclesRun.EmployeePostCycles != null && !this.CurrentEmployeePostCyclesRun.EmployeePostCycles.Any(i => i.EmployeePost.EmployeePostId == employeePosts.FirstOrDefault().EmployeePostId))
                return new List<EmployeePostSort>() { new EmployeePostSort(employeePosts.FirstOrDefault()) };
            else if (employeePosts.Count == 1 && this.CurrentEmployeePostCyclesRun.EmployeePostCycles != null && this.CurrentEmployeePostCyclesRun.EmployeePostCycles.Any(i => i.EmployeePost.EmployeePostId == employeePosts.FirstOrDefault().EmployeePostId))
                return new List<EmployeePostSort>();

            foreach (var employeePost in employeePosts)
            {
                if (this.CurrentEmployeePostCyclesRun != null && this.CurrentEmployeePostCyclesRun.EmployeePostCycles != null && this.CurrentEmployeePostCyclesRun.EmployeePostCycles.Where(i => i.EmployeePost.EmployeePostId == employeePost.EmployeePostId && i.RemainingMinutes == 0).Any())
                    continue;

                if (this.CurrentEmployeePostCyclesRun.EmployeePostCycles.Where(i => i.EmployeePost.EmployeePostId == employeePost.EmployeePostId && i.DisposeThis).Any())
                {
                    if (!this.DisposedEmployeePostIds.Any(i => i == employeePost.EmployeePostId))
                    {
                        EmployeePostCycle cycleToRemove = this.CurrentEmployeePostCyclesRun.EmployeePostCycles.FirstOrDefault(i => i.EmployeePost.EmployeePostId == employeePost.EmployeePostId);

                        this.CurrentEmployeePostCyclesRun.EmployeePostCycles.Remove(cycleToRemove);
                        this.DisposedEmployeePostIds.Add(employeePost.EmployeePostId);
                        employeePostSorts.Add(new EmployeePostSort(employeePost, hasBeenDisposed: true));
                    }
                    continue;
                }
                else if (this.CurrentEmployeePostCyclesRun != null && this.CurrentEmployeePostCyclesRun.EmployeePostCycles != null && this.CurrentEmployeePostCyclesRun.EmployeePostCycles.Any(i => i.EmployeePost.EmployeePostId == employeePost.EmployeePostId))
                    continue;

                employeePostSorts.Add(new EmployeePostSort(employeePost));
            }

            return employeePostSorts;
        }

        private List<List<DayOfWeek>> GetOrders(List<List<CalculationPeriodItem>> allPossibleWeeks)
        {
            List<List<DayOfWeek>> orders = new List<List<DayOfWeek>>();
            List<CalculationPeriodItem> periodItems = new List<CalculationPeriodItem>();
            Dictionary<DayOfWeek, int> dict = new Dictionary<DayOfWeek, int>();

            foreach (var poss in allPossibleWeeks)
            {
                foreach (var item in poss)
                {
                    DayOfWeek dayOfWeek = item.ScheduleDate.DayOfWeek;
                    if (dict.Where(d => d.Key == dayOfWeek).Any())
                    {
                        var pair = dict.Where(d => d.Key == dayOfWeek).FirstOrDefault();
                        int value = pair.Value;

                        dict.Remove(pair.Key);

                        dict.Add(dayOfWeek, value + 1);
                    }
                }
            }

            List<DayOfWeek> lowestFirst = new List<DayOfWeek>();
            lowestFirst.AddRange(dict.OrderBy(o => o.Value).Select(s => s.Key));
            if (lowestFirst.Any())
                orders.Add(lowestFirst);

            List<DayOfWeek> highestFirst = new List<DayOfWeek>();
            lowestFirst.AddRange(dict.OrderByDescending(o => o.Value).Select(s => s.Key));

            if (highestFirst.Any())
                orders.Add(highestFirst);

            return orders;

        }

        private void SetNumberOfPossibleQualifiedShiftTypes(List<EmployeePostSort> unSortedEmployeePosts)
        {
            List<Tuple<int, int>> tuples = new List<Tuple<int, int>>();

            foreach (var employeePost in unSortedEmployeePosts)
            {
                foreach (var shiftType in this.ShiftTypes)
                {
                    if (shiftType.ShiftTypeSkills != null && shiftType.ShiftTypeSkills.Any())
                    {
                        if (employeePost.EmployeePost.SkillMatch(shiftType))
                            tuples.Add(Tuple.Create(employeePost.EmployeePost.EmployeePostId, shiftType.ShiftTypeId));
                    }
                }
            }

            var groups = tuples.GroupBy(g => g.Item2);

            foreach (var group in groups)
            {
                if (group.Count() < decimal.Divide(unSortedEmployeePosts.Count, 10) || group.Count() <= 4)
                {
                    foreach (var pair in group)
                    {
                        var post = unSortedEmployeePosts.FirstOrDefault(i => i.EmployeePost.EmployeePostId == pair.Item1);
                        post.NumberOfQualifiedShiftTypes++;
                    }
                }
            }

            foreach (var employeePost in unSortedEmployeePosts.Where(w => w.NumberOfQualifiedShiftTypes == 0))
                employeePost.NumberOfQualifiedShiftTypes = 999;
        }

        private void SetValidShiftTypes(List<EmployeePostDTO> employeePostDTOs)
        {
            List<Tuple<int, int>> tuples = new List<Tuple<int, int>>();

            foreach (var employeePost in employeePostDTOs)
            {
                foreach (var shiftType in this.ShiftTypes)
                {
                    if (shiftType.ShiftTypeSkills != null && shiftType.ShiftTypeSkills.Any())
                    {
                        if (employeePost.SkillMatch(shiftType) && !employeePost.ValidShiftTypes.Any(v => v.ShiftTypeId == shiftType.ShiftTypeId))
                            employeePost.ValidShiftTypes.Add(shiftType);
                    }
                    else if (!employeePost.ValidShiftTypes.Any(v => v.ShiftTypeId == shiftType.ShiftTypeId))
                        employeePost.ValidShiftTypes.Add(shiftType);
                }
            }


        }

        private void SetUniques(List<EmployeePostSort> unSortedEmployeePosts, List<CalculationPeriodItem> needPeriodItems)
        {
            Dictionary<string, int> uniqueDict = new Dictionary<string, int>();

            foreach (var post in unSortedEmployeePosts)
            {
                if (post.PossibleWeeks.Any())
                {
                    foreach (var week in post.PossibleWeeks)
                    {
                        foreach (var item in week)
                        {
                            string compareString = item.GetCompareString();

                            if (!uniqueDict.ContainsKey(compareString))
                                uniqueDict.Add(compareString, 1);
                            else
                            {
                                int value = uniqueDict.FirstOrDefault(d => d.Key == compareString).Value;
                                uniqueDict.Remove(compareString);
                                uniqueDict.Add(compareString, value + 1);
                            }
                        }
                    }
                }
            }

            if (uniqueDict.Count == 0)
            {
                foreach (var post in GetPeriodItemsOnlyValidForOneEmployeePost(unSortedEmployeePosts.Select(s => s.EmployeePost).ToList(), needPeriodItems, this.ShiftTypes))
                {
                    foreach (var item in post.Value)
                    {
                        string compareString = item.GetCompareString();

                        if (!uniqueDict.ContainsKey(compareString))
                            uniqueDict.Add(compareString, 1);
                        else
                        {
                            int value = uniqueDict.Where(d => d.Key == compareString).FirstOrDefault().Value;
                            uniqueDict.Remove(compareString);
                            uniqueDict.Add(compareString, value + 1);
                        }
                    }
                }
            }

            foreach (var pair in uniqueDict.OrderBy(i => i.Value))
            {
                foreach (var post in unSortedEmployeePosts)
                {
                    if (post.CompareStrings.Contains(pair.Key))
                    {
                        post.CompareScore += pair.Value;

                        if (pair.Value == 1)
                        {
                            post.Uniques++;
                            post.RemoveOther(pair.Key);
                        }
                    }
                }
            }
        }

        #endregion

        #region Help Methods

        #region EmployeePost

        private static readonly Dictionary<string, List<CalculationPeriodItem>> usedPeriodItemsCache = new Dictionary<string, List<CalculationPeriodItem>>();


        private List<CalculationPeriodItem> GetUsedPeriodItems(DateTime startDate, DateTime currentDate, int currentEmployeePostId, int numberOfWeeks, string caller)
        {
            // Create a unique key for caching
            string cacheKey = $"{startDate:yyyyMMdd}-{currentDate:yyyyMMdd}-{currentEmployeePostId}-{numberOfWeeks}-{this.CurrentEmployeePostCyclesRun.Guid}-{this.CurrentEmployeePostCyclesRun.FirstEmployeePostSort}-{caller}";

            // Check if the result is in the cache
            if (usedPeriodItemsCache.TryGetValue(cacheKey, out List<CalculationPeriodItem> cachedPeriodItems))
                return ClonePeriodItems(cachedPeriodItems);

            List<CalculationPeriodItem> periodItems = new List<CalculationPeriodItem>();
            var dates = CalendarUtility.GetDates(currentDate.DayOfWeek, startDate, startDate.AddDays(this.CalculationOptions.MaxNumberOfWeeks * 7));
            List<DateTime> filteredDates = new List<DateTime>();

            var days = numberOfWeeks * 7;
            foreach (var date in dates)
            {
                if (Math.Abs(Convert.ToInt32((currentDate - date).TotalDays)) % (days) == 0 || date == currentDate)
                    filteredDates.Add(date);
            }

            foreach (EmployeePostCycle cycle in this.CurrentEmployeePostCyclesRun.EmployeePostCycles)
            {
                if (cycle.EmployeePost.EmployeePostId != currentEmployeePostId)
                {
                    foreach (var date in filteredDates)
                    {
                        foreach (var item in cycle.GetSelectedPeriodItems(date))
                        {
                            if (!periodItems.Where(i => i.OriginalCalculationRowGuid == item.OriginalCalculationRowGuid && i.ShiftTypeId == item.ShiftTypeId && i.TimeSlot.From == item.TimeSlot.From && i.TimeSlot.To == item.TimeSlot.To).Any())
                                periodItems.Add(item);
                        }
                    }
                }
            }

            // Store the result in the cache
            usedPeriodItemsCache[cacheKey] = ClonePeriodItems(periodItems);

            return periodItems;
        }

        private List<Tuple<DateTime, List<CalculationPeriodItem>>> GetRemainingPeriodItemForWeeks(List<CalculationPeriodItem> needPeriodItems, DateTime startDate, int excludeEmployeePostId, int numberOfWeeks)
        {
            var tuples = new List<Tuple<DateTime, List<CalculationPeriodItem>>>();
            var datesInWeek = CalendarUtility.GetDates(startDate, startDate.AddDays(6));

            foreach (var day in datesInWeek)
            {
                var items = GetUsedPeriodItems(startDate, day, excludeEmployeePostId, numberOfWeeks, "GetRemainingPeriodItemForWeeks");

                bool isAnyNeedChanged;
                var needs = GetRemainingPeriodItems(needPeriodItems.Where(i => i.ScheduleDate == day).ToList(), items, out isAnyNeedChanged);
                tuples.Add(Tuple.Create(day, needs));
            }

            return tuples;
        }

        private void SetCalculationOptions(EmployeePostDTO employeePost, int maxNumberOfWeeks, List<OpeningHoursDTO> openingHours)
        {
            EmployeeGroupDTO employeeGroupDTO = employeePost.EmployeeGroupDTO != null ? employeePost.EmployeeGroupDTO : null;

            //Change CalculationOptions
            this.CalculationOptions.MaxLength = 600;
            this.CalculationOptions.EmployeePost = employeePost;
            this.CalculationOptions.ApplyLengthRule = true;
            if (employeeGroupDTO != null)
            {
                this.CalculationOptions.MinLength = employeeGroupDTO.RuleWorkTimeDayMinimum > 0 ? employeeGroupDTO.RuleWorkTimeDayMinimum : this.CalculationOptions.MinLength;
                this.CalculationOptions.MaxLength = employeeGroupDTO.RuleWorkTimeDayMaximumWorkDay > 0 ? employeeGroupDTO.RuleWorkTimeDayMaximumWorkDay : this.CalculationOptions.MaxLength > 0 ? this.CalculationOptions.MaxLength : 1440;
                this.CalculationOptions.MaxLenghtWeekend = employeeGroupDTO != null && employeeGroupDTO.RuleWorkTimeDayMaximumWeekend > 0 ? employeeGroupDTO.RuleWorkTimeDayMaximumWeekend : this.CalculationOptions.MaxLength;
            }
            this.CalculationOptions.OptimalLength = employeePost.WorkTimePerDay > 0 ? employeePost.WorkTimePerDay : this.CalculationOptions.OptimalLength;
            this.CalculationOptions.MaxNumberOfWeeks = maxNumberOfWeeks;

            if (this.CalculationOptions.MaxLength > employeePost.WorkTimeCycle && employeePost.WorkTimeCycle > 0)
                this.CalculationOptions.MaxLength = employeePost.WorkTimeCycle;

            this.CalculationOptions.OpeningHours = openingHours;
        }

        private void SetCalculationOptions(DayOfWeek dayOfWeek, DateTime date, int? DayTypeId)
        {
            this.CalculationOptions.Weekday = dayOfWeek;
            this.CalculationOptions.Date = date;
            this.CalculationOptions.DayTypeId = DayTypeId;
        }

        private void CreateAndAddEmployeePostCycle(EmployeePostDTO employeePost, List<CalculationPeriodItem> validPeriodItems, List<List<CalculationPeriodItem>> possibleWeeks, List<ShiftTypeDTO> shiftTypes, int days, int timeCodeId, DateTime startDate, int maxNumberOfWeeks, List<List<DayOfWeek>> dayOfWeekOrders)
        {
            SetCalculationOptions(employeePost, maxNumberOfWeeks, this.CalculationOptions.OpeningHours);
            List<EmployeePostWeekDayOfWeekOrder> employeePostWeekDayOfWeekOrder = new List<EmployeePostWeekDayOfWeekOrder>();

            foreach (var order in dayOfWeekOrders)
                employeePostWeekDayOfWeekOrder.Add(new EmployeePostWeekDayOfWeekOrder(order));

            EmployeePostCycle employeePostCycle = HandlePeriodItemsForEmployeePost(validPeriodItems, possibleWeeks, shiftTypes, startDate, days, true, employeePostWeekDayOfWeekOrder);
            employeePostCycle.MultipleWeeks(maxNumberOfWeeks);
            this.CurrentEmployeePostCyclesRun.EmployeePostCycles.Add(employeePostCycle);
        }

        private List<int> GetShiftTypeIdsValidForOneEmployeePost(List<EmployeePostDTO> employeePosts, List<CalculationPeriodItem> periodItems, List<ShiftTypeDTO> shiftTypes)
        {
            var releventEmployeePosts = employeePosts.Where(w => w.WorkTimeWeek > 0);
            List<int> list = new List<int>();
            List<Tuple<int, int>> tuples = new List<Tuple<int, int>>();

            foreach (var employeePost in releventEmployeePosts)
            {
                foreach (var shiftType in this.ShiftTypes)
                {
                    if (shiftType.ShiftTypeSkills != null && shiftType.ShiftTypeSkills.Any())
                    {
                        if (employeePost.SkillMatch(shiftType))
                            tuples.Add(Tuple.Create(employeePost.EmployeePostId, shiftType.ShiftTypeId));
                    }
                }
            }

            var groups = tuples.GroupBy(g => g.Item2);

            foreach (var group in groups)
            {
                if (group.Count() == 1)
                {
                    list.Add(group.FirstOrDefault().Item2);
                }
            }

            return list;
        }

        private List<int> GetShiftTypeIdsWithPrio(List<EmployeePostDTO> employeePosts, List<CalculationPeriodItem> periodItems, List<ShiftTypeDTO> shiftTypes)
        {
            var releventEmployeePosts = employeePosts.Where(w => w.WorkTimeWeek > 0).ToList();
            List<int> list = new List<int>();
            Dictionary<int, int> dict = new Dictionary<int, int>();

            foreach (var type in shiftTypes)
            {
                int count = 0;

                if (type.ShiftTypeSkills == null || type.ShiftTypeSkills.Count == 0)
                    continue;

                var periodItemsOnShiftType = periodItems.Where(w => w.ShiftTypeId == type.ShiftTypeId).ToList();

                if (periodItemsOnShiftType.Count == 0)
                    continue;


                foreach (var employeePost in releventEmployeePosts)
                {
                    if (employeePost.SkillMatch(type))
                        count++;
                }

                dict.Add(type.ShiftTypeId, count);
            }

            list.AddRange(dict.Where(d => d.Value > 0 && d.Value <= 3).OrderBy(o => o.Value).Select(s => s.Key));

            if (list.Count < decimal.Divide(releventEmployeePosts.Count, 5))
                list.AddRange(dict.Where(d => d.Value > 3 && d.Value <= 4).OrderBy(o => o.Value).Select(s => s.Key));

            return list;
        }

        private Dictionary<EmployeePostDTO, List<CalculationPeriodItem>> GetPeriodItemsOnlyValidForOneEmployeePost(List<EmployeePostDTO> employeePosts, List<CalculationPeriodItem> periodItems, List<ShiftTypeDTO> shiftTypes)
        {
            var releventEmployeePosts = employeePosts.Where(w => w.WorkTimeWeek > 0);
            Dictionary<EmployeePostDTO, List<CalculationPeriodItem>> dict = new Dictionary<EmployeePostDTO, List<CalculationPeriodItem>>();

            foreach (ShiftTypeDTO shiftTypeDTO in shiftTypes)
            {
                List<EmployeePostDTO> employeePostWithMatchedSkills = new List<EmployeePostDTO>();

                foreach (EmployeePostDTO employeePostDTO in releventEmployeePosts)
                {
                    if (MatchSkill(employeePostDTO, shiftTypeDTO))
                        employeePostWithMatchedSkills.Add(employeePostDTO);
                }

                if (employeePostWithMatchedSkills.Count == 1)
                {
                    foreach (EmployeePostDTO employeePost in employeePostWithMatchedSkills)
                    {
                        if (!dict.Any(d => d.Key == employeePost))
                            dict.Add(employeePost, FilterMatchedPeriodItems(periodItems, employeePost, shiftTypes, CalendarUtility.DATETIME_DEFAULT, CalendarUtility.DATETIME_DEFAULT));
                    }
                }
            }

            return dict;
        }

        private bool IsAlreadyHandled(EmployeePostDTO employeePost)
        {
            return this.CurrentEmployeePostCyclesRun.EmployeePostCycles.Where(e => e.EmployeePost.EmployeePostId == employeePost.EmployeePostId).Any();
        }

        public static int GetFirstCommonNumberOfWeeks(List<EmployeePostDTO> employeePosts)
        {
            int[] numberOfWeeks = employeePosts.Where(w => w.ScheduleCycleDTO != null).Select(i => i.ScheduleCycleDTO.NbrOfWeeks).ToArray();
            return NumberUtility.LestCommonMultiple(numberOfWeeks.Where(r => r != 0).ToArray());
        }

        #endregion

        #region EmployeePostCycle

        private List<EmployeePostCycle> CreateEmployeePostCyclesFromSavedData(DateTime startDate, List<EmployeePostDTO> employeePosts, List<TimeSchedulePlanningDayDTO> shifts, List<CalculationPeriodItem> shiftPeriodItems, int maxNumberOfWeeks)
        {
            List<EmployeePostCycle> employeeCycles = new List<EmployeePostCycle>();

            foreach (EmployeePostDTO employeePost in employeePosts)
            {
                List<TimeSchedulePlanningDayDTO> employeePostShifts = shifts.Where(s => s.EmployeePostId == employeePost.EmployeePostId).ToList();
                if (!employeePostShifts.Any())
                    continue;

                List<TimeSchedulePlanningDayDTO> firstCycle = new List<TimeSchedulePlanningDayDTO>();
                DateTime previousDate = CalendarUtility.DATETIME_DEFAULT;
                int previousDayNumber = 0;

                foreach (TimeSchedulePlanningDayDTO shift in employeePostShifts.OrderBy(o => o.StartTime))
                {
                    if ((previousDayNumber == shift.DayNumber && previousDate == shift.StartTime.Date) || (previousDayNumber < shift.DayNumber && previousDate != shift.StartTime.Date))
                    {
                        firstCycle.Add(shift);
                        previousDate = shift.StartTime.Date;
                        previousDayNumber = shift.DayNumber;
                    }
                }

                int weekNumber = 1;

                EmployeePostCycle cycle = new EmployeePostCycle(employeePost, startDate, this.CalculationOptions, this.AllScheduleCycleRuleDTOs);
                cycle.FromSavedData = true;
                cycle.EmployeePost = employeePost;

                var groupByEmployeePostAndWeek = firstCycle.GroupBy(i => $"id{i.EmployeePostId}#start{CalendarUtility.GetFirstDateOfWeek(i.StartTime.Date)}");
                foreach (var groupByEmployeePostAndWeekShifts in groupByEmployeePostAndWeek)
                {
                    EmployeePostWeek employeePostWeek = new EmployeePostWeek(weekNumber, startDate.AddDays(((weekNumber - 1) * 7)), new List<EmployeePostDay>(), employeePost, this.CalculationOptions);
                    foreach (var shiftOnDayGroup in groupByEmployeePostAndWeekShifts.GroupBy(i => i.StartTime.Date))
                    {
                        List<CalculationPeriodItem> periodItemsOnDay = new List<CalculationPeriodItem>();

                        foreach (TimeSchedulePlanningDayDTO shift in shiftOnDayGroup)
                        {
                            foreach (var item in shiftPeriodItems.Where(i => shift.StaffingNeedsRowId == i.StaffingNeedsRowId))
                            {
                                if (!periodItemsOnDay.Any(a => a.Info == item.Info))
                                    periodItemsOnDay.Add(ClonePeriodItem(item));
                            }
                        }

                        if (periodItemsOnDay.Any())
                        {
                            Guid originalCalculationRowGuid = Guid.NewGuid();
                            EmployeePostDay employeePostDay = new EmployeePostDay(shiftOnDayGroup.Key, this.CalculationOptions);
                            employeePostDay.SelectedItemsHead = GroupStaffingNeedsCalcutionPeriodItems(periodItemsOnDay).FirstOrDefault();

                            foreach (var item in employeePostDay.SelectedItemsHead.CalculationPeriodItems)
                            {
                                item.ScheduleDate = shiftOnDayGroup.Key;
                                item.Date = shiftOnDayGroup.Key;
                                item.CalculationGuid = originalCalculationRowGuid;
                                item.OriginalCalculationRowGuid = originalCalculationRowGuid;
                                item.PeriodGuid = originalCalculationRowGuid;
                                item.EmployeePostId = employeePost.EmployeePostId;
                            }

                            employeePostWeek.EmployeePostDays.Add(employeePostDay);
                            employeePostWeek.EmployeePost = employeePost;
                        }
                    }

                    cycle.EmployeePostWeeks.Add(employeePostWeek);
                    weekNumber++;
                }

                cycle.MultipleWeeks(maxNumberOfWeeks);
                employeeCycles.Add(cycle);
            }

            return employeeCycles.Where(s => s.FromSavedData).ToList();
        }

        #endregion

        #region StaffingNeedsRowDTO

        private List<StaffingNeedsRowDTO> CloneWithNoIds(List<StaffingNeedsRowDTO> rows)
        {
            List<StaffingNeedsRowDTO> clones = new List<StaffingNeedsRowDTO>();

            foreach (var item in rows)
            {
                StaffingNeedsRowDTO clone = item.CloneDTO();
                clone.StaffingNeedsHeadId = 0;
                clone.StaffingNeedsRowId = 0;

                foreach (var period in clone.Periods)
                {
                    period.StaffingNeedsRowId = 0;
                    period.StaffingNeedsRowPeriodId = 0;
                }

                clones.Add(clone);
            }

            return clones;
        }

        #endregion

        #region CalculationPeriodItem

        private List<CalculationPeriodItem> ClonePeriodItems(List<CalculationPeriodItem> periodItems, bool setTempGuidAsCalculationGuid = false)
        {
            List<CalculationPeriodItem> clones = new List<CalculationPeriodItem>();
            foreach (CalculationPeriodItem periodItem in periodItems)
            {
                var clone = ClonePeriodItem(periodItem);
                clone.TempGuid = clone.CalculationGuid;
                clones.Add(clone);
            }
            return clones;
        }

        private List<CalculationPeriodItem> SetScheduleDate(List<CalculationPeriodItem> periodItems, DateTime weekStartDate)
        {

            foreach (var weekDay in CalendarUtility.GetWeekDaysList())
            {
                var matching = periodItems.Where(i => i.Weekday == weekDay);

                foreach (var item in matching)
                    item.ScheduleDate = weekStartDate;

                weekStartDate = weekStartDate.AddDays(1);
            }

            return periodItems;
        }

        private CalculationPeriodItem ClonePeriodItem(CalculationPeriodItem periodItem)
        {
            if (periodItem == null)
                return periodItem;

            CalculationPeriodItem clone = periodItem.Clone();
            //clone.TimeSlot = periodItem.TimeSlot.Clone();
            return clone;
        }

        private List<StaffingNeedsRowFrequencyDTO> GenerateStaffingNeedsRowFrequencyItems(int interval, List<StaffingNeedsAnalysisChartData> staffingNeedsAnalysisChartData, int? shiftTypeId)
        {
            List<StaffingNeedsRowFrequencyDTO> items = new List<StaffingNeedsRowFrequencyDTO>();
            if (staffingNeedsAnalysisChartData.Count == 0 || !shiftTypeId.HasValue)
                return items;

            foreach (var item in staffingNeedsAnalysisChartData)
            {
                StaffingNeedsRowFrequencyDTO freq = new StaffingNeedsRowFrequencyDTO();

                freq.Value = item.OrginalValue;
                freq.ShiftTypeId = shiftTypeId;
                freq.Interval = interval;
                freq.StartTime = item.Date;

                items.Add(freq);
            }

            return items;
        }

        private List<CalculationPeriodItem> GenerateStaffingNeedsPeriodItems(int interval, List<StaffingNeedsAnalysisChartData> staffingNeedsAnalysisChartData, int? shiftTypeId, List<ShiftTypeDTO> shiftTypeDTOs, List<StaffingNeedsLocationGroupDTO> staffingNeedsLocationGroupDTOs = null, List<TimeScheduleTaskDTO> timeScheduleTaskDTOs = null)
        {
            List<CalculationPeriodItem> periodItems = new List<CalculationPeriodItem>();
            List<StaffingNeedsRowDTO> rows = new List<StaffingNeedsRowDTO>();

            foreach (StaffingNeedsAnalysisChartData chartData in staffingNeedsAnalysisChartData)
            {
                int value = 0;

                while (value != (int)chartData.Value)
                {
                    value++;

                    StaffingNeedsRowPeriodDTO period = new StaffingNeedsRowPeriodDTO()
                    {
                        StartTime = CalendarUtility.MergeDateAndTime(CalendarUtility.DATETIME_DEFAULT, chartData.Date, ignoreSqlServerDateTime: true),
                        Value = 1,//?
                        State = SoeEntityState.Active,
                    };

                    StaffingNeedsRowDTO row = rows.FirstOrDefault(x => x.TempId == value);
                    if (row == null)
                    {
                        row = new StaffingNeedsRowDTO()
                        {
                            TempId = value,
                            ShiftTypeId = shiftTypeId,
                            StaffingNeedsLocationGroupId = chartData.StaffingNeedsLocationGroupId.HasValue ? chartData.StaffingNeedsLocationGroupId : null,
                            Periods = new List<StaffingNeedsRowPeriodDTO>(),
                            State = SoeEntityState.Active,
                        };
                        row.Periods.Add(period);
                        rows.Add(row);
                    }
                    else
                    {
                        row.Periods.Add(period);
                    }
                }
            }

            #region Fix periods

            foreach (var row in rows)
            {
                DateTime startTime = row.Periods.OrderBy(o => o.StartTime).FirstOrDefault().StartTime;
                DateTime endTime = row.Periods.OrderByDescending(o => o.StartTime).FirstOrDefault().StartTime.AddMinutes(interval);
                List<StaffingNeedsRowPeriodDTO> newPeriods = new List<StaffingNeedsRowPeriodDTO>();
                DateTime currentTime = startTime;
                DateTime lastStart = currentTime;
                int periods = 0;

                while (currentTime < endTime)
                {
                    var currentPeriods = row.Periods.Where(i => i.StartTime == currentTime);

                    if (periods == 0 && currentPeriods.Any())
                        lastStart = currentTime;

                    if (currentPeriods.Any())
                        periods++;

                    var nextPeriods = row.Periods.Where(i => i.StartTime == currentTime.AddMinutes(interval)).ToList();

                    if (nextPeriods.Count == 0 && periods > 0)
                    {
                        StaffingNeedsRowPeriodDTO newDTO = new StaffingNeedsRowPeriodDTO()
                        {
                            StartTime = lastStart,
                            Length = periods * interval
                        };

                        newPeriods.Add(newDTO);
                        periods = 0;
                    }

                    currentTime = currentTime.AddMinutes(interval);
                }

                if (periods > 0)
                {
                    StaffingNeedsRowPeriodDTO newDTO = new StaffingNeedsRowPeriodDTO()
                    {
                        StartTime = lastStart,
                        Length = periods * interval
                    };

                    newPeriods.Add(newDTO);
                }

                row.Periods = new List<StaffingNeedsRowPeriodDTO>();
                row.Periods.AddRange(newPeriods);
            }

            #endregion

            foreach (StaffingNeedsRowDTO row in rows)
            {
                Guid guid = Guid.NewGuid();
                int? timeScheduleTaskId = null;
                ShiftTypeDTO shiftTypeDTO = null;
                StaffingNeedsCalculationTimeSlot timeSlot = new StaffingNeedsCalculationTimeSlot();

                foreach (var period in row.Periods)
                {
                    if (period != null)
                    {
                        StaffingNeedsLocationGroupDTO staffingNeedsLocationGroupDTO = staffingNeedsLocationGroupDTOs != null && row.StaffingNeedsLocationGroupId.HasValue ? staffingNeedsLocationGroupDTOs.FirstOrDefault(s => s.StaffingNeedsLocationGroupId == row.StaffingNeedsLocationGroupId) : null;
                        if (staffingNeedsLocationGroupDTO != null)
                        {
                            timeScheduleTaskId = staffingNeedsLocationGroupDTO.TimeScheduleTaskId;

                            if (timeScheduleTaskDTOs != null && timeScheduleTaskId.HasValue)
                            {
                                var timeScheduleTaskDTO = timeScheduleTaskDTOs.Where(t => t.TimeScheduleTaskId == timeScheduleTaskId.Value).FirstOrDefault();

                                if (timeScheduleTaskDTO != null && timeScheduleTaskDTO.ShiftTypeId.HasValue)
                                {
                                    shiftTypeDTO = shiftTypeDTOs.FirstOrDefault(s => s.ShiftTypeId == timeScheduleTaskDTO.ShiftTypeId);
                                    if (shiftTypeDTO != null)
                                        shiftTypeId = shiftTypeDTO.ShiftTypeId;
                                }
                            }

                        }

                        CalculationPeriodItem periodItem = new CalculationPeriodItem()
                        {
                            CalculationPeriodItemGuid = Guid.NewGuid(),
                            CalculationGuid = guid,
                            StaffingNeedsRowGuid = guid,
                            OriginalCalculationRowGuid = guid,

                            StaffingNeedsHeadId = row.StaffingNeedsHeadId,
                            StaffingNeedsRowId = row.StaffingNeedsRowId,
                            StaffingNeedsRowPeriodId = period.StaffingNeedsRowPeriodId,
                            ShiftTypeId = shiftTypeId,
                            TimeScheduleTaskId = timeScheduleTaskId,
                            TimeScheduleTaskTypeId = null,
                            IncomingDeliveryRowId = null,
                            IncomingDeliveryRowKey = string.Empty,
                            DayTypeId = row.DayTypeId,

                            ShiftType = shiftTypeDTO,
                            TimeSlot = timeSlot.AsFixedSlot(period.StartTime, period.Length),

                            Type = row.Type,
                            OriginType = StaffingNeedsRowOriginType.StaffingNeedsAnalysisChartData,
                            Name = row.Name,
                            Weekday = row.Weekday,
                            Date = row.Date,
                            StartTime = period.StartTime,
                            Interval = period.Interval,
                            Value = period.Value,

                            PeriodState = period.State,
                            RowState = row.State,

                            Created = row.Created,
                            CreatedBy = row.CreatedBy,
                            Modified = row.Modified,
                            ModifiedBy = row.ModifiedBy,
                        };
                        periodItem.SetTimeScheduleTaskKey(1);

                        if (periodItem.Length > 0)
                            periodItems.AddRange(ClonePeriodItems(new List<CalculationPeriodItem>() { periodItem }));
                    }
                }
            }

            return periodItems;
        }

        private List<CalculationPeriodItem> GenerateStaffingNeedsPeriodItems(int interval, List<ShiftTypeDTO> shiftTypes, List<TimeScheduleTaskDTO> timeScheduleTasks)
        {
            List<CalculationPeriodItem> periodItems = new List<CalculationPeriodItem>();

            foreach (var item in timeScheduleTasks)
            {
                if (!item.StartTime.HasValue)
                    continue;

                int person = 1;
                int persons = item.NbrOfPersons == 0 || item.NbrOfPersons > 100 ? 1 : item.NbrOfPersons;
                int remainingLength = CalendarUtility.AdjustAccordingToInterval(item.Length, this.CalculationOptions.Interval, alwaysReduce: true);
                int maxLenght = Convert.ToInt32((item.StopTime.Value - item.StartTime.Value).TotalMinutes);

                while (person <= persons && remainingLength >= 0)
                {
                    int personLength = 0;

                    if (remainingLength <= maxLenght)
                        personLength = remainingLength;
                    else
                        personLength = maxLenght;

                    personLength = CalendarUtility.AdjustAccordingToInterval(personLength, this.CalculationOptions.Interval);

                    remainingLength = remainingLength - personLength;

                    //Set TimeSlot
                    StaffingNeedsCalculationTimeSlot timeSlot = new StaffingNeedsCalculationTimeSlot(item.StartTime.Value, item.StopTime.Value, personLength);

                    var startTime = CalendarUtility.MergeDateAndTime(CalendarUtility.DATETIME_DEFAULT, item.StartTime.Value, ignoreSqlServerDateTime: true);
                    Guid guid = Guid.NewGuid();

                    CalculationPeriodItem periodItem = new CalculationPeriodItem()
                    {
                        CalculationPeriodItemGuid = Guid.NewGuid(),
                        CalculationGuid = guid,
                        StaffingNeedsRowGuid = guid,
                        OriginalCalculationRowGuid = guid,

                        StaffingNeedsHeadId = 0,
                        StaffingNeedsRowId = 0,
                        StaffingNeedsRowPeriodId = 0,
                        ShiftTypeId = item.ShiftTypeId,
                        TimeScheduleTaskId = item.TimeScheduleTaskId,
                        TimeScheduleTaskTypeId = item.TimeScheduleTaskTypeId,
                        IncomingDeliveryRowId = null,
                        IncomingDeliveryRowKey = string.Empty,
                        DayTypeId = 0,

                        TimeSlot = timeSlot,
                        ShiftType = shiftTypes.FirstOrDefault(s => s.ShiftTypeId == item.ShiftTypeId) != null ? shiftTypes.FirstOrDefault(s => s.ShiftTypeId == item.ShiftTypeId).CloneDTO() : null,

                        Type = StaffingNeedsRowType.Normal,
                        OriginType = StaffingNeedsRowOriginType.TimeScheduleTask,
                        Weekday = null,
                        Date = null,
                        StartTime = startTime,
                        Name = String.Empty,
                        Interval = interval,
                        Value = 1,
                        CalculationRowNr = 0,
                        MinSplitLength = item.MinSplitLength,
                        FromBreakRules = false,
                        DontAssignBreakLeftovers = item.DontAssignBreakLeftovers,
                        IsStaffingNeedsFrequency = item.IsStaffingNeedsFrequency,
                        OnlyOneEmployee = item.OnlyOneEmployee,
                        AllowOverlapping = item.AllowOverlapping,
                        IsBreak = false,

                        PeriodState = SoeEntityState.Active,
                        RowState = SoeEntityState.Active,
                    };

                    periodItem.SetTimeScheduleTaskKey(person);

                    if (periodItem.Length > 0)
                        periodItems.AddRange(ClonePeriodItems(new List<CalculationPeriodItem>() { periodItem }));

                    person++;

                }
            }

            return periodItems;
        }

        private List<CalculationPeriodItem> GenerateStaffingNeedsPeriodItems(int interval, List<ShiftTypeDTO> shiftTypes, List<IncomingDeliveryHeadDTO> incomingDeliveryHeads)
        {
            List<CalculationPeriodItem> periodItems = new List<CalculationPeriodItem>();

            foreach (var item in incomingDeliveryHeads)
            {
                foreach (var row in item.Rows)
                {
                    if (!row.StartTime.HasValue)
                        continue;

                    int person = 1;
                    int persons = row.NbrOfPersons != 0 ? row.NbrOfPersons : 1;

                    int length = CalendarUtility.AdjustAccordingToInterval(row.Length * persons, this.CalculationOptions.Interval, alwaysReduce: true);
                    int addedLength = 0;
                    int rest = 0;

                    while (person <= persons || addedLength > length)
                    {
                        person++;
                        int personLength = Convert.ToInt32(decimal.Divide(length, persons));
                        personLength = CalendarUtility.AdjustAccordingToInterval(personLength, this.CalculationOptions.Interval);

                        if (!CalendarUtility.AccordingToInterval(this.CalculationOptions.Interval, personLength))
                        {
                            if (personLength >= this.CalculationOptions.Interval)
                            {
                                if (!CalendarUtility.AccordingToInterval(this.CalculationOptions.Interval, rest + personLength) && !CalendarUtility.AccordingToInterval(this.CalculationOptions.Interval, rest + personLength + 1) && (rest == 0 || !CalendarUtility.AccordingToInterval(this.CalculationOptions.Interval, rest + personLength)))
                                {
                                    int adjustedPersonLength = CalendarUtility.AdjustAccordingToInterval(personLength, this.CalculationOptions.Interval, alwaysReduce: true);
                                    rest = rest + personLength - adjustedPersonLength;
                                    personLength = adjustedPersonLength;
                                }
                                else if (CalendarUtility.AccordingToInterval(this.CalculationOptions.Interval, rest + personLength))
                                {
                                    personLength = rest + personLength;
                                    rest = 0;
                                }
                            }

                            if (!CalendarUtility.AccordingToInterval(this.CalculationOptions.Interval, personLength))
                            {
                                if (CalendarUtility.AccordingToInterval(this.CalculationOptions.Interval, rest + personLength + 1))
                                {
                                    int adjustedPersonLength = CalendarUtility.AdjustAccordingToInterval(rest + personLength + 1, this.CalculationOptions.Interval);
                                    rest = rest + personLength - adjustedPersonLength;
                                    personLength = adjustedPersonLength;
                                }
                                else
                                {
                                    int adjustedPersonLength = CalendarUtility.AdjustAccordingToInterval(rest + personLength, this.CalculationOptions.Interval);
                                    rest = rest + personLength - adjustedPersonLength;
                                    personLength = adjustedPersonLength;
                                }
                            }
                        }

                        if (!CalendarUtility.AccordingToInterval(this.CalculationOptions.Interval, personLength))
                        {
                            rest = rest + personLength;
                            continue;
                        }

                        addedLength = addedLength + personLength;

                        if (personLength == 0 || addedLength > length + this.CalculationOptions.Interval)
                            continue;

                        //Set TimeSlot
                        StaffingNeedsCalculationTimeSlot timeSlot = new StaffingNeedsCalculationTimeSlot(row.StartTime.Value, row.StopTime.Value, personLength);

                        var startTime = CalendarUtility.MergeDateAndTime(CalendarUtility.DATETIME_DEFAULT, row.StartTime.Value, ignoreSqlServerDateTime: true);
                        Guid guid = Guid.NewGuid();

                        CalculationPeriodItem periodItem = new CalculationPeriodItem()
                        {
                            CalculationPeriodItemGuid = Guid.NewGuid(),
                            CalculationGuid = guid,
                            StaffingNeedsRowGuid = guid,

                            StaffingNeedsHeadId = 0,
                            StaffingNeedsRowId = 0,
                            StaffingNeedsRowPeriodId = 0,
                            ShiftTypeId = row.ShiftTypeId,
                            TimeScheduleTaskId = null,
                            TimeScheduleTaskTypeId = null,
                            IncomingDeliveryRowId = row.IncomingDeliveryRowId,
                            DayTypeId = 0,

                            TimeSlot = timeSlot,
                            ShiftType = shiftTypes.FirstOrDefault(s => s.ShiftTypeId == row.ShiftTypeId),

                            Type = StaffingNeedsRowType.Normal,
                            OriginType = StaffingNeedsRowOriginType.IncomingDelivery,
                            Name = String.Empty,
                            Weekday = null,
                            Date = null,
                            StartTime = startTime,
                            Interval = interval,
                            Value = 1,
                            CalculationRowNr = 0,
                            FromBreakRules = false,
                            DontAssignBreakLeftovers = row.DontAssignBreakLeftovers,
                            OnlyOneEmployee = row.OnlyOneEmployee,
                            AllowOverlapping = row.AllowOverlapping,
                            MinSplitLength = row.MinSplitLength,
                            IsBreak = false,

                            PeriodState = SoeEntityState.Active,
                            RowState = SoeEntityState.Active,
                        };

                        periodItem.SetIncomingDeliveryRowKey(person);

                        if (periodItem.Length > 0)
                            periodItems.Add(periodItem);
                    }
                }
            }

            return periodItems;
        }

        private void AddStats()
        {
            this.initStats();
            if (this.PeriodItemsGroupHeads != null && this.PeriodItemsGroupHeads.Count > 0)
            {
                this.PercentageDone.Add(Convert.ToInt32(decimal.Multiply(decimal.Divide(this.PeriodItemsGroupHeads.Where(o => o.Done).Count(), this.PeriodItemsGroupHeads.Count), 100)));
                int maxLengthShift = this.PeriodItemsGroupHeads.Where(w => w.Length <= this.CalculationOptions.MaxLength).Any() ? this.PeriodItemsGroupHeads.Where(w => w.Length <= this.CalculationOptions.MaxLength).Max(m => m.Length) : 0;
                this.MaxLengthShift.Add(maxLengthShift);
            }
        }

        private void initStats()
        {
            if (this.PercentageDone == null)
                this.PercentageDone = new List<int>();
            if (this.MaxLengthShift == null)
                this.MaxLengthShift = new List<int>();
        }

        private void RemoveStats()
        {
            this.PercentageDone = new List<int>();
            this.MaxLengthShift = new List<int>();
        }

        private bool AbortBasedOnStats(int iterations, int maxNumberOfIterations)
        {
            if (iterations < decimal.Divide(maxNumberOfIterations, 5) || iterations < 5)
                return false;

            this.initStats();

            if (this.MaxLengthShift.Count > decimal.Divide(iterations, 2) || AnalyzeResultIsSame())
            {
                if (this.MaxLengthShift.Where(i => i >= this.CalculationOptions.MinLength).Any() && this.MaxLengthShift.LastOrDefault() > 0)
                {
                    return true;
                }
            }

            if (this.PercentageDone.Count > decimal.Divide(iterations, 2))
            {
                if (this.PercentageDone.Where(i => i == 0).Count() == this.PercentageDone.Count)
                {
                    if (this.MaxLengthShift.LastOrDefault() > 0 && this.MaxLengthShift.Max(m => m) == this.MaxLengthShift.LastOrDefault())
                        return true;
                }
            }

            return false;

        }

        private bool AnalyzeResultIsSame()
        {
            int count = this.MaxLengthShift.Count;
            int percentLimit = 70;

            if (count < 6)
                return false;

            var groups = this.MaxLengthShift.GroupBy(g => g);

            if (groups.Count() == 1)
                return true;

            return groups.OrderBy(o => o.Count() > decimal.Multiply(count, percentLimit / 100)).Any();
        }

        private void OrderAndSetCalcutionRowNumber(List<CalculationPeriodItem> periodItems, bool groupOnShiftType = false)
        {
            //   Order by 
            // - StartTime from first on row
            // - Length from first to last on row

            int rowNr = 0;

            List<Tuple<List<CalculationPeriodItem>, DateTime, DateTime, int, string>> tuples = new List<Tuple<List<CalculationPeriodItem>, DateTime, DateTime, int, string>>();

            foreach (var group in periodItems.GroupBy(o => o.CalculationGuid))
            {
                var first = group.OrderBy(g => g.TimeSlot.From).FirstOrDefault();

                if (first.TimeSlot == null || first.ShiftType == null)
                    continue;

                DateTime firstTime = first.TimeSlot.From;
                DateTime lastTime = group.OrderBy(g => g.TimeSlot.To).Last().TimeSlot.To;
                int lenght = Convert.ToInt32((lastTime - firstTime).TotalMinutes);
                string shiftTypeName = group.FirstOrDefault() != null && first.ShiftType != null ? first.ShiftType.Name : string.Empty;

                Tuple<List<CalculationPeriodItem>, DateTime, DateTime, int, string> tuple = Tuple.Create(group.ToList(), firstTime, lastTime, lenght, shiftTypeName);

                tuples.Add(tuple);
            }
            if (groupOnShiftType)
                tuples = tuples.OrderBy(o => o.Item5).ThenByDescending(t => t.Item2).ThenBy(t => t.Item4).ToList();
            else
                tuples = tuples.OrderByDescending(t => t.Item2).ThenBy(t => t.Item4).ToList();

            foreach (var item in tuples)
            {

                rowNr++;

                foreach (var row in item.Item1)
                    row.CalculationRowNr = rowNr;
            }
        }

        private void SortCalculationRowsOnShiftTypeId(List<CalculationPeriodItem> periodItems, int interval)
        {
            foreach (var group in periodItems.GroupBy(o => o.ShiftTypeId))
            {
                Guid newGuid = Guid.NewGuid();

                foreach (var guidGroup in group.GroupBy(g => g.CalculationGuid))
                {
                    CalculationPeriodItem prevStaffingNeedsCalcutionRow = null;

                    foreach (var item in guidGroup.ToList().OrderBy(g => g.TimeSlot.From))
                    {
                        if (prevStaffingNeedsCalcutionRow != null && prevStaffingNeedsCalcutionRow.TimeSlot.To > item.TimeSlot.From)
                            newGuid = Guid.NewGuid();

                        item.CalculationGuid = newGuid;

                        prevStaffingNeedsCalcutionRow = item;
                    }
                }
            }
        }

        private List<CalculationPeriodItem> OrderPeriodItemsBasedOnPriority(CalculationPeriodItem periodItem, List<CalculationPeriodItem> periodItems, List<Guid> fittingGuids, EmployeePostDTO employeePostDTO = null)
        {
            List<CalculationPeriodItem> prioPeriodItems = new List<CalculationPeriodItem>();
            List<CalculationPeriodItem> newPeriodItems = new List<CalculationPeriodItem>();

            SetTimeSlotToMinFrom(periodItems);

            #region Sort on Lenght

            periodItems = periodItems.OrderByDescending(o => o.TimeSlot.IsFixed).ThenBy(s => s.TimeSlot.MinFrom).ThenByDescending(t => t.TimeSlot.TimeSlotLength).ToList();

            #endregion

            #region Sort Docked

            periodItems = periodItems.OrderBy(p => !fittingGuids.Contains(p.CalculationGuid)).ThenByDescending(o => o.TimeSlot.IsFixed).ThenBy(s => s.TimeSlot.MinFrom).ThenByDescending(t => t.TimeSlot.TimeSlotLength).ToList();

            #endregion

            #region Uniques Skills 

            if (this.CalculationOptions.HasUniques)
            {
                if (periodItems.Any(w => w.ShiftTypeId.HasValue && this.CurrentEmployeePostCyclesRun.UniqueShiftypeIds.Contains(w.ShiftTypeId.Value)))
                {
                    foreach (var item in periodItems.Where(r => r.IsUnique(this.CurrentEmployeePostCyclesRun)))
                        prioPeriodItems.Add(item);
                }
            }

            #endregion

            #region First Skills 

            var skillsKey = periodItem.GetKey(CalculationPeriodItemGroupByType.Skills);

            foreach (var item in periodItems.Where(r => r.GetKey(CalculationPeriodItemGroupByType.Skills).Equals(skillsKey)))
                prioPeriodItems.Add(item);

            #endregion

            #region Second ShiftType

            var shiftTypesKey = periodItem.GetKey(CalculationPeriodItemGroupByType.ShiftType);

            foreach (var item in periodItems.Where(r => r.ShiftTypeId.HasValue && r.ShiftTypeId.Value == periodItem.ShiftTypeId))
            {
                if (!prioPeriodItems.ContainsItem(item))
                    prioPeriodItems.Add(item);
            }

            #endregion

            #region Third ShiftTypeLink

            var shiftTypeLinkKey = periodItem.GetKey(CalculationPeriodItemGroupByType.ShiftTypeLink);

            foreach (var item in periodItems.Where(r => r.GetKey(CalculationPeriodItemGroupByType.ShiftTypeLink).Equals(shiftTypeLinkKey)))
            {
                if (!prioPeriodItems.ContainsItem(item))
                    prioPeriodItems.Add(item);
            }

            #endregion

            #region The rest

            foreach (var item in periodItems)
            {
                if (!prioPeriodItems.ContainsItem(item))
                    prioPeriodItems.Add(item);
            }

            #endregion

            if (employeePostDTO == null)
                return prioPeriodItems;
            else
            {
                foreach (var pItem in prioPeriodItems)
                {
                    if (PeriodItemIsValid(pItem, periodItem.ShiftTypeId, employeePostDTO))
                        newPeriodItems.Add(pItem);
                }

                return newPeriodItems;
            }
        }

        private List<CalculationPeriodItem> CleanList(List<CalculationPeriodItem> periodItems, CalculationPeriodItemGroupByType calculationPeriodItemGroupByType = CalculationPeriodItemGroupByType.Unknown)
        {
            List<CalculationPeriodItem> newPeriodItems = new List<CalculationPeriodItem>();

            foreach (var periodItem in periodItems)
            {
                if (!SkipItem(periodItem.GetKey(calculationPeriodItemGroupByType), periodItem.CalculationGuid))
                    newPeriodItems.Add(periodItem);
            }

            return newPeriodItems;
        }

        private List<CalculationPeriodItem> FilterValidPeriodItems(List<CalculationPeriodItem> periodItems, EmployeePostDTO employeePost)
        {
            List<CalculationPeriodItem> filteredValidPeriodItems = new List<CalculationPeriodItem>();

            var groups = periodItems.GroupBy(i => i.ShiftTypeId);

            foreach (var group in groups)
            {
                foreach (var item in group)
                {
                    if (PeriodItemIsValid(item, group.Key, employeePost))
                        filteredValidPeriodItems.Add(item);
                }
            }

            return filteredValidPeriodItems;
        }

        private bool PeriodItemIsValid(CalculationPeriodItem periodItem, int? shiftTypeId, EmployeePostDTO employeePostDTO)
        {
            if (employeePostDTO == null)
                return true;

            if (employeePostDTO.SkillMatch(periodItem.ShiftType))
            {
                if (!shiftTypeId.HasValue || periodItem.ShiftType.LinkedShiftTypeIds.Contains(shiftTypeId.Value))
                    return true;

                if (shiftTypeId.HasValue && periodItem.ShiftTypeId.HasValue && shiftTypeId.Value == periodItem.ShiftTypeId.Value)
                    return true;

                var shiftType = this.ShiftTypes.FirstOrDefault(i => i.ShiftTypeId == shiftTypeId);
                if (shiftType != null && !shiftType.LinkedShiftTypeIds.Any() && !periodItem.ShiftType.LinkedShiftTypeIds.Any())
                    return true;

                return false;

            }

            return false;
        }

        #endregion

        #region Optimize

        #region Mountain

        private List<StaffingNeedsRowDTO> BuildMountain(List<CalculationPeriodItem> periodItems)
        {
            //sortedStaffingNeedsPeriodItems = FallLikeTetris(periodItems);
            OrderAndSetCalcutionRowNumber(periodItems, groupOnShiftType: true);

            return periodItems.ToStaffingNeedsRowDTOs();
        }

        #endregion

        #region TimeSlots

        private void SetTimeSlotToMinFrom(List<CalculationPeriodItem> periodItems)
        {
            foreach (var periodItem in periodItems)
                AdjustTimeSlot(periodItem.TimeSlot, CalculationOptions.Interval, true, true);
        }

        private StaffingNeedsCalculationTimeSlot AdjustTimeSlot(StaffingNeedsCalculationTimeSlot timeSlot, int interval, bool setToOtherLimit = false, bool forward = true, DateTime? startTime = null)
        {
            DateTime past = new DateTime(1818, 1, 1);
            DateTime future = new DateTime(2118, 1, 1);

            if (timeSlot.To > future || timeSlot.From < past)
                return timeSlot;

            int minutes = timeSlot.Minutes;

            if (setToOtherLimit && forward)
            {
                timeSlot.From = timeSlot.MinFrom;
                timeSlot.To = timeSlot.From.AddMinutes(minutes);
            }
            else if (setToOtherLimit && !forward)
            {
                timeSlot.To = timeSlot.MaxTo;
                timeSlot.From = timeSlot.To.AddMinutes(-minutes);
            }
            else
            {
                DateTime oldFrom = timeSlot.From;
                DateTime oldTo = timeSlot.To;

                if (forward && oldFrom.AddMinutes(interval) <= timeSlot.MaxTo && oldTo.AddMinutes(interval) <= timeSlot.MaxTo)
                {
                    timeSlot.From = timeSlot.From.AddMinutes(interval);
                    timeSlot.To = timeSlot.To.AddMinutes(interval);
                }
                else if (!forward && oldTo.AddMinutes(-interval) >= timeSlot.MinFrom && oldFrom.AddMinutes(-interval) >= timeSlot.MinFrom)
                {
                    timeSlot.From = timeSlot.From.AddMinutes(-interval);
                    timeSlot.To = timeSlot.To.AddMinutes(-interval);
                }
            }

            return timeSlot;
        }

        #endregion

        #region Shifts

        private List<CalculationPeriodItem> FillStaffingNeedsShifts(List<CalculationPeriodItem> periodItems)
        {
            List<CalculationPeriodItem> newPeriodItems = new List<CalculationPeriodItem>();
            CalculationOptions.ApplyLengthRule = true;
            this.CalculationOptions.AddOnlyDockedPeriodItems = true;

            foreach (var item in periodItems)
                item.CalculationGuid = Guid.NewGuid();

            newPeriodItems = FallLikeTetris(periodItems, allowSplit: true);
            OrderAndSetCalcutionRowNumber(newPeriodItems);

            return newPeriodItems;
        }

        private bool IsPeriodItemsStillValid(List<CalculationPeriodItem> needPeriodItems, List<CalculationPeriodItem> shiftOnDayPeriodItems, List<CalculationPeriodItem> shiftPeriodItems)
        {
            if (shiftPeriodItems.Count == 0)
                return true;

            bool isAnyNeedChanged = true;
            int interval = this.CalculationOptions.Interval;
            var timeScheduleTaskIds = needPeriodItems.Where(s => s.IsStaffingNeedsFrequency).Select(s => s.TimeScheduleTaskId).Distinct().ToList();

            if (shiftOnDayPeriodItems.Any() && shiftOnDayPeriodItems.Any(i => i.IsStaffingNeedsFrequency))
            {
                foreach (var timeScheduleTaskId in timeScheduleTaskIds)
                {
                    var needs = needPeriodItems.Where(w => w.TimeScheduleTaskId == timeScheduleTaskId).ToList();
                    var shifts = shiftOnDayPeriodItems.Where(w => w.TimeScheduleTaskId == timeScheduleTaskId && !w.IsBreak).ToList();

                    if (shifts.Count == 0)
                        continue;

                    Dictionary<DateTime, int> timeNeedDict = new Dictionary<DateTime, int>();
                    DateTime start = shifts.Min(i => i.TimeSlot.From) < (needs.Any() ? needs.Min(i => i.TimeSlot.From) : DateTime.MinValue) ? shifts.Min(i => i.TimeSlot.From) : needs.Any() ? needs.Min(i => i.TimeSlot.From) : shifts.Min(i => i.TimeSlot.From);
                    DateTime stop = shifts.Max(i => i.TimeSlot.To) > (needs.Any() ? needs.Max(i => i.TimeSlot.To) : DateTime.MaxValue) ? shifts.Max(i => i.TimeSlot.To) : needs.Any() ? needs.Max(i => i.TimeSlot.To) : shifts.Max(i => i.TimeSlot.To);
                    DateTime current = start;
                    while (current.AddMinutes(interval) <= stop)
                    {
                        int fromShift = shifts.Where(period => CalendarUtility.IsNewOverlappedByCurrent(current, current.AddMinutes(interval), period.TimeSlot.From, period.TimeSlot.To)).Count();
                        int fromNeed = needs.Where(w => CalendarUtility.IsNewOverlappedByCurrent(current, current.AddMinutes(interval), w.TimeSlot.From, w.TimeSlot.To)).Count();

                        if (fromShift > fromNeed)
                            return false;

                        current = current.AddMinutes(interval);
                    }
                }
            }

            List<CalculationPeriodItem> remainingNeedPeriodItems = GetRemainingPeriodItems(shiftOnDayPeriodItems.Where(w => (!w.TimeScheduleTaskId.HasValue || !timeScheduleTaskIds.Contains(w.TimeScheduleTaskId.Value))).ToList(), shiftPeriodItems.Where(w => (!w.TimeScheduleTaskId.HasValue || !timeScheduleTaskIds.Contains(w.TimeScheduleTaskId.Value))).ToList(), out isAnyNeedChanged);
            return !isAnyNeedChanged;
        }

        private Dictionary<DayOfWeek, int> GetRemainingTimeOnDatesInWeek(int employeePostId, List<CalculationPeriodItem> needPeriodItems, DateTime weekStartDate)
        {
            Dictionary<DayOfWeek, int> dict = new Dictionary<DayOfWeek, int>();
            var cloneNeedPeriodItems = SetScheduleDate(ClonePeriodItems(needPeriodItems), weekStartDate);
            bool adjusted = false;

            foreach (var currentDate in CalendarUtility.GetDatesInInterval(weekStartDate, CalendarUtility.AdjustDateToEndOfWeek(weekStartDate)))
            {
                var needOnDay = cloneNeedPeriodItems.Where(w => w.ScheduleDate == currentDate).ToList();
                var shiftOnDay = SetScheduleDate(ClonePeriodItems(GetUsedPeriodItems(weekStartDate, currentDate, employeePostId, 1, "GetRemainingTimeOnDatesInWeek")), weekStartDate).Where(w => w.Date == currentDate).ToList();

                var remaining = GetRemainingPeriodItems(needOnDay, shiftOnDay, out adjusted);

                if (remaining.Count > 0)
                    dict.Add(currentDate.DayOfWeek, remaining.Sum(s => s.Length));
                else
                    dict.Add(currentDate.DayOfWeek, 0);
            }

            return dict;
        }

        //Temporary public (for unit testing)
        public List<CalculationPeriodItem> GetRemainingPeriodItems(List<CalculationPeriodItem> needPeriodItems, List<CalculationPeriodItem> shiftPeriodItems, out bool isAnyNeedChanged)
        {
            List<CalculationPeriodItem> newPeriodItems = new List<CalculationPeriodItem>();
            isAnyNeedChanged = false;

            needPeriodItems = ClonePeriodItems(needPeriodItems);
            shiftPeriodItems = ClonePeriodItems(shiftPeriodItems);

            #region remove need from saved data when isstaffingneedfrequancy

            if (shiftPeriodItems.Count > 0 && shiftPeriodItems.Any(i => i.IsStaffingNeedsFrequency))
            {
                var timeScheduleTaskIds = needPeriodItems.Where(s => s.IsStaffingNeedsFrequency).Select(s => s.TimeScheduleTaskId).Distinct().ToList();

                foreach (var timeScheduleTaskId in timeScheduleTaskIds)
                {
                    var needsOnTask = needPeriodItems.Where(w => w.TimeScheduleTaskId == timeScheduleTaskId).ToList();
                    var shiftsOnTask = shiftPeriodItems.Where(w => w.TimeScheduleTaskId == timeScheduleTaskId && !w.IsBreak).ToList();
                    var newList = RemoveFromNeed(needsOnTask, shiftsOnTask);

                    if (newList.Count != needsOnTask.Count || newList.Sum(s => s.Length) != needsOnTask.Sum(s => s.Length))
                        isAnyNeedChanged = true;

                    needPeriodItems = needPeriodItems.Where(i => i.TimeScheduleTaskId != timeScheduleTaskId).ToList();
                    needPeriodItems.AddRange(newList);
                }
            }

            #endregion

            foreach (CalculationPeriodItem needPeriodItem in needPeriodItems)
            {
                if (needPeriodItem.Ignore)
                    continue;

                bool isNeedChanged = false;
                needPeriodItem.Ignore = true;
                bool isStaffingNeedsFrequency = needPeriodItem.IsStaffingNeedsFrequency;

                List<CalculationPeriodItem> shiftPeriodItemsForNeed = new List<CalculationPeriodItem>();

                if (!needPeriodItem.IsStaffingNeedsFrequency)
                {
                    shiftPeriodItemsForNeed = (from r in shiftPeriodItems
                                               where !r.Ignore &&
                                               //r.ScheduleDate == needPeriodItem.ScheduleDate &&
                                               (!r.IsBreak || r.IsBreak && r.BreakFillsNeed) &&
                                               r.ShiftTypeId.HasValue &&
                                               !r.IsStaffingNeedsFrequency &&
                                               r.ShiftTypeId.Value == needPeriodItem.ShiftTypeId &&
                                               (
                                               (r.IncomingDeliveryRowId.HasValue && r.IncomingDeliveryRowKey == needPeriodItem.IncomingDeliveryRowKey)
                                               ||
                                               (r.TimeScheduleTaskId.HasValue && r.TimeScheduleTaskKey == needPeriodItem.TimeScheduleTaskKey)
                                               ||
                                               (r.OriginType == StaffingNeedsRowOriginType.StaffingNeedsAnalysisChartData)
                                               )
                                               select r).OrderBy(i => i.StartTime).ToList();
                }

                if (shiftPeriodItemsForNeed.Count > 0)
                {
                    #region Need has shifts

                    bool isOverlapping = false;

                    if (!isStaffingNeedsFrequency)
                    {
                        foreach (var shiftPeriodItemForNeedOuter in shiftPeriodItemsForNeed)
                        {
                            foreach (var shiftPeriodItemForNeedInner in shiftPeriodItemsForNeed)
                            {
                                if (shiftPeriodItemForNeedOuter.GetHashCode() != shiftPeriodItemForNeedInner.GetHashCode())
                                {
                                    if (CalendarUtility.IsTimesOverlappingNew(shiftPeriodItemForNeedOuter.TimeSlot.From, shiftPeriodItemForNeedOuter.TimeSlot.To, shiftPeriodItemForNeedInner.TimeSlot.From, shiftPeriodItemForNeedInner.TimeSlot.To))
                                        isOverlapping = true;
                                }
                            }
                        }
                    }

                    Guid needGuid = Guid.NewGuid();
                    List<CalculationPeriodItem> newPeriodItemsForNeed = new List<CalculationPeriodItem>();

                    if (needPeriodItem.TimeSlot.IsFixed && !isOverlapping)
                    {
                        #region Need is fixed (time within need)

                        DateTime currentNeedStart = needPeriodItem.TimeSlot.From;
                        for (int shiftNr = 1; shiftNr <= shiftPeriodItemsForNeed.Count; shiftNr++)
                        {
                            CalculationPeriodItem shiftPeriodItem = shiftPeriodItemsForNeed[shiftNr - 1];
                            int needMinutes = (int)shiftPeriodItem.TimeSlot.From.Subtract(currentNeedStart).TotalMinutes;
                            if (needMinutes > 0)
                                newPeriodItemsForNeed.Add(needPeriodItem.CloneAndSetNewTimeFrom(currentNeedStart, needMinutes, setNewCalculationPeriodItemGuid: true));

                            if (shiftNr == shiftPeriodItemsForNeed.Count)
                            {
                                needMinutes = (int)needPeriodItem.TimeSlot.To.Subtract(shiftPeriodItem.TimeSlot.To).TotalMinutes;
                                if (needMinutes > 0)
                                    newPeriodItemsForNeed.Add(needPeriodItem.CloneAndSetNewTimeFrom(shiftPeriodItem.TimeSlot.To, needMinutes, setNewCalculationPeriodItemGuid: true));
                            }

                            currentNeedStart = shiftPeriodItem.TimeSlot.To;
                        }

                        #endregion
                    }
                    else
                    {
                        #region Need is not fixed (time can be outside need)

                        #region If many rows on one item in the need
                        int minutesFromConnectedNeed = 0;

                        List<CalculationPeriodItem> connectedNeed = (from r in needPeriodItems.Where(r => !r.Ignore)
                                                                     where !r.Ignore &&
                                                                      //r.ScheduleDate == needPeriodItem.ScheduleDate &&
                                                                      !r.IsBreak &&
                                                                      r.ShiftTypeId.HasValue &&
                                                                      r.ShiftTypeId.Value == needPeriodItem.ShiftTypeId &&
                                                                      (
                                                                      (r.IncomingDeliveryRowId.HasValue && r.IncomingDeliveryRowKey == needPeriodItem.IncomingDeliveryRowKey)
                                                                      ||
                                                                      (r.TimeScheduleTaskId.HasValue && r.TimeScheduleTaskKey == needPeriodItem.TimeScheduleTaskKey)
                                                                      ||
                                                                      (r.OriginType == StaffingNeedsRowOriginType.StaffingNeedsAnalysisChartData)
                                                                      )
                                                                     select r).OrderBy(i => i.StartTime).ToList();

                        if (connectedNeed.Any())
                        {
                            connectedNeed.Remove(needPeriodItem);

                            if (connectedNeed.Any())
                            {
                                minutesFromConnectedNeed = connectedNeed.Sum(i => i.TimeSlot.Minutes);

                                foreach (var item in connectedNeed)
                                    item.Ignore = true;
                            }
                        }

                        #endregion


                        var task = this.TimeScheduleTasks != null && needPeriodItem.TimeScheduleTaskId.HasValue ? this.TimeScheduleTasks.FirstOrDefault(f => f.TimeScheduleTaskId == needPeriodItem.TimeScheduleTaskId.Value) : null;
                        var delivery = this.IncomingDeliveryHeads != null && needPeriodItem.IncomingDeliveryRowId.HasValue ? this.incomingDeliveryRows.FirstOrDefault(f => f.IncomingDeliveryHeadId == needPeriodItem.IncomingDeliveryRowId.Value) : null;

                        int totalNeedMinutes = needPeriodItem.TimeSlot.Minutes + minutesFromConnectedNeed;

                        //if (task != null && task.Length != totalNeedMinutes)
                        //    totalNeedMinutes = task.Length;

                        int totalShiftMinutes = shiftPeriodItemsForNeed.Sum(i => i.TimeSlot.Minutes);
                        int remainingMinutes = totalNeedMinutes - totalShiftMinutes;
                        if (remainingMinutes > 0 && !needPeriodItem.IncomingDeliveryRowId.HasValue)
                            newPeriodItemsForNeed.Add(needPeriodItem.CloneAndSetNewTimeFrom(needPeriodItem.TimeSlot.From, remainingMinutes, setNewCalculationPeriodItemGuid: true));
                        else if (remainingMinutes > 0 && needPeriodItem.IncomingDeliveryRowId.HasValue && remainingMinutes >= needPeriodItem.Length)
                        {
                            var cloned = needPeriodItem.Clone();
                            cloned.CalculationGuid = Guid.NewGuid();
                            newPeriodItemsForNeed.Add(cloned);
                        }

                        #endregion
                    }

                    if (newPeriodItemsForNeed.Count > 0)
                    {
                        #region New need created

                        foreach (CalculationPeriodItem newNeedPeriodItem in newPeriodItemsForNeed.OrderBy(o => o.TimeSlot.From).ToList())
                        {
                            newNeedPeriodItem.CalculationGuid = needGuid;
                            newNeedPeriodItem.OriginalCalculationRowGuid = needGuid;
                            if (!newPeriodItems.ContainsItem(newNeedPeriodItem))
                                newPeriodItems.Add(newNeedPeriodItem);
                        }

                        foreach (CalculationPeriodItem shiftPeriodItem in shiftPeriodItemsForNeed)
                        {
                            shiftPeriodItem.Ignore = true;
                        }

                        isNeedChanged = true;

                        #endregion
                    }
                    else
                    {
                        #region Need is fulfilled

                        needPeriodItem.Ignore = true;
                        isNeedChanged = true;

                        #endregion
                    }

                    #endregion
                }
                else
                {
                    #region Need has no shifts

                    newPeriodItems.Add(needPeriodItem);

                    #endregion
                }

                if (isNeedChanged)
                    isAnyNeedChanged = true;
            }

            foreach (var item in newPeriodItems)
                item.Ignore = false;

            foreach (var item in needPeriodItems)
                item.Ignore = false;

            foreach (var item in shiftPeriodItems)
                item.Ignore = false;

            return MergeShifts(newPeriodItems, allowAdjustment: true);
        }

        public List<StaffingNeedsRowDTO> MergeShiftFromChartData(List<TimeScheduleTask> timeScheduleTasks, List<StaffingNeedsRowDTO> chartRows, List<ShiftTypeDTO> shiftTypes, List<TimeScheduleTaskDTO> tasks, DateTime date, bool buildMountain = false)
        {
            List<CalculationPeriodItem> periodItems = new List<CalculationPeriodItem>();

            foreach (TimeScheduleTask timeScheduleTask in timeScheduleTasks.Where(w => w.RecurringDates.DayOfWeekValid(date.DayOfWeek)))
            {
                Guid guid = Guid.NewGuid();
                DateTime startTime = timeScheduleTask.StartTime.HasValue ? timeScheduleTask.StartTime.Value : CalendarUtility.DATETIME_DEFAULT;
                DateTime stopTime = timeScheduleTask.StopTime.HasValue ? timeScheduleTask.StopTime.Value : startTime.AddMinutes(timeScheduleTask.Length);

                CalculationPeriodItem taskPeriodItem = new CalculationPeriodItem()
                {
                    CalculationGuid = guid,
                    OriginType = StaffingNeedsRowOriginType.TimeScheduleTask,
                    Date = date,
                    StartTime = startTime,
                    TimeScheduleTaskId = timeScheduleTask.TimeScheduleTaskId,
                    IsStaffingNeedsFrequency = true
                };
                taskPeriodItem.TimeSlot = new StaffingNeedsCalculationTimeSlot(startTime, stopTime, timeScheduleTask.Length);
                taskPeriodItem.ShiftTypeId = timeScheduleTask.ShiftTypeId;
                periodItems.Add(taskPeriodItem);
            }

            List<CalculationPeriodItem> chartPeriodItems = chartRows?.ToCalculationPeriodItems(shiftTypes, tasks, new List<IncomingDeliveryHeadDTO>()) ?? new List<CalculationPeriodItem>();
            chartPeriodItems.ForEach(i => i.OriginType = StaffingNeedsRowOriginType.StaffingNeedsAnalysisChartData);
            periodItems.AddRange(chartPeriodItems);

            return MergeShiftFromChartData(periodItems, buildMountain).ToStaffingNeedsRowDTOs(shiftTypes, tasks);
        }

        public List<StaffingNeedsRowDTO> MergeShiftFromChartData(List<StaffingNeedsRowDTO> headTaskAndDeliveryRows, List<StaffingNeedsRowDTO> chartRows, List<ShiftTypeDTO> shiftTypes, List<TimeScheduleTaskDTO> timeScheduleTasks, bool buildMountain = false)
        {
            List<CalculationPeriodItem> periodItems = new List<CalculationPeriodItem>();
            List<StaffingNeedsRowDTO> rowFrequencys = chartRows.Where(w => w.RowFrequencys != null && w.RowFrequencys.Any()).ToList();
            chartRows = chartRows.Where(w => w.RowFrequencys == null || w.RowFrequencys.Count == 0).ToList();

            List<CalculationPeriodItem> taskPeriodItems = headTaskAndDeliveryRows != null ? headTaskAndDeliveryRows.ToCalculationPeriodItems(shiftTypes, timeScheduleTasks, new List<IncomingDeliveryHeadDTO>()) : new List<CalculationPeriodItem>();
            taskPeriodItems.ForEach(i => i.OriginType = StaffingNeedsRowOriginType.TimeScheduleTask);
            periodItems.AddRange(taskPeriodItems);

            List<CalculationPeriodItem> chartPeriodItems = chartRows.ToCalculationPeriodItems(shiftTypes, timeScheduleTasks, new List<IncomingDeliveryHeadDTO>()) ?? new List<CalculationPeriodItem>();
            chartPeriodItems.ForEach(i => i.OriginType = StaffingNeedsRowOriginType.StaffingNeedsAnalysisChartData);
            periodItems.AddRange(chartPeriodItems);

            var merged = MergeShiftFromChartData(periodItems, buildMountain).ToStaffingNeedsRowDTOs(shiftTypes, timeScheduleTasks);
            merged.AddRange(rowFrequencys);
            return merged;
        }

        private List<CalculationPeriodItem> MergeShiftFromChartData_old(List<CalculationPeriodItem> periodItems)
        {
            List<CalculationPeriodItem> chartPeriodItems = periodItems.Where(r => r.OriginType == StaffingNeedsRowOriginType.StaffingNeedsAnalysisChartData).ToList();
            List<CalculationPeriodItem> taskAndDeliveryPeriodItems = periodItems.Where(r => r.OriginType == StaffingNeedsRowOriginType.TimeScheduleTask).ToList();
            List<CalculationPeriodItem> newPeriodItems = new List<CalculationPeriodItem>();

            bool changes = true;

            while (changes)
            {
                changes = false;

                foreach (CalculationPeriodItem newPeriodItem in newPeriodItems)
                {
                    if (!taskAndDeliveryPeriodItems.ContainsItem(newPeriodItem) && newPeriodItem.OriginType == StaffingNeedsRowOriginType.TimeScheduleTask)
                        taskAndDeliveryPeriodItems.Add(newPeriodItem);
                }

                foreach (CalculationPeriodItem taskAndDeliveryPeriodItem in taskAndDeliveryPeriodItems.Where(r => !r.Ignore))
                {
                    Guid guid = Guid.NewGuid();

                    foreach (CalculationPeriodItem newPeriodItem in newPeriodItems)
                    {
                        if (!chartPeriodItems.ContainsItem(newPeriodItem) && newPeriodItem.OriginType == StaffingNeedsRowOriginType.StaffingNeedsAnalysisChartData)
                            chartPeriodItems.Add(newPeriodItem);
                    }

                    foreach (CalculationPeriodItem chartPeriodItem in chartPeriodItems.Where(r => r.TimeScheduleTaskId.HasValue && r.ShiftTypeId.HasValue && r.ShiftTypeId == taskAndDeliveryPeriodItem.ShiftTypeId && !r.Ignore).OrderByDescending(o => o.TimeSlot.Minutes))
                    {
                        if (CalendarUtility.IsCurrentOverlappedByNew(taskAndDeliveryPeriodItem.TimeSlot.From, taskAndDeliveryPeriodItem.TimeSlot.To, chartPeriodItem.TimeSlot.From, chartPeriodItem.TimeSlot.To))
                        {
                            #region Task is longer on both sides 

                            taskAndDeliveryPeriodItem.CalculationGuid = guid;
                            taskAndDeliveryPeriodItem.OriginalCalculationRowGuid = guid;
                            chartPeriodItem.CalculationGuid = guid;
                            chartPeriodItem.OriginalCalculationRowGuid = guid;

                            taskAndDeliveryPeriodItem.Clone();

                            List<CalculationPeriodItem> splittedPeriodItems = SplitShift(taskAndDeliveryPeriodItem, chartPeriodItem.TimeSlot.From, chartPeriodItem.TimeSlot.To, runRecursive: false, setNewGuid: false);
                            if (splittedPeriodItems.Count > 1)
                            {
                                splittedPeriodItems = splittedPeriodItems.OrderBy(o => o.TimeSlot.From).ToList();

                                var splittedItemFrom = splittedPeriodItems.FirstOrDefault(w => w.TimeSlot.From == chartPeriodItem.TimeSlot.From);
                                if (splittedItemFrom != null)
                                    splittedItemFrom.RowState = SoeEntityState.Deleted;

                                foreach (CalculationPeriodItem splittedPeriodItem in splittedPeriodItems.Where(r => r.RowState != SoeEntityState.Deleted))
                                {
                                    splittedPeriodItem.CalculationGuid = guid;
                                    splittedPeriodItem.OriginalCalculationRowGuid = guid;
                                    if (!newPeriodItems.ContainsItem(splittedPeriodItem))
                                        newPeriodItems.Add(splittedPeriodItem);
                                }
                            }
                            else
                            {
                                periodItems.Remove(taskAndDeliveryPeriodItem);
                                break;
                            }

                            chartPeriodItem.Ignore = true;
                            changes = true;
                            break;

                            #endregion
                        }
                        else if (CalendarUtility.IsNewOverlappedByCurrent(taskAndDeliveryPeriodItem.TimeSlot.From, taskAndDeliveryPeriodItem.TimeSlot.To, chartPeriodItem.TimeSlot.From, chartPeriodItem.TimeSlot.To))
                        {
                            #region Task can fit inside

                            taskAndDeliveryPeriodItem.CalculationGuid = guid;
                            taskAndDeliveryPeriodItem.OriginalCalculationRowGuid = guid;
                            chartPeriodItem.CalculationGuid = guid;
                            chartPeriodItem.OriginalCalculationRowGuid = guid;

                            CalculationPeriodItem backup = taskAndDeliveryPeriodItem.Clone();
                            List<CalculationPeriodItem> splittedPeriodItems = SplitShift(taskAndDeliveryPeriodItem, taskAndDeliveryPeriodItem.TimeSlot.From, taskAndDeliveryPeriodItem.TimeSlot.To, runRecursive: false, setNewGuid: false);

                            if (splittedPeriodItems.Count > 2)
                            {
                                int count = 0;
                                foreach (CalculationPeriodItem splittedPeriodItem in splittedPeriodItems)
                                {
                                    splittedPeriodItem.CalculationGuid = guid;
                                    splittedPeriodItem.OriginalCalculationRowGuid = guid;

                                    if (count == 1)
                                        splittedPeriodItem.Ignore = true;

                                    if (!newPeriodItems.ContainsItem(splittedPeriodItem))
                                        newPeriodItems.Add(splittedPeriodItem);
                                }
                            }
                            else
                            {
                                periodItems.Remove(taskAndDeliveryPeriodItem);
                                periodItems.Add(backup);
                            }

                            taskAndDeliveryPeriodItem.Ignore = true;
                            chartPeriodItem.Ignore = true;
                            changes = true;
                            break;

                            #endregion
                        }
                        else if (CalendarUtility.IsTimesOverlappingNew(taskAndDeliveryPeriodItem.TimeSlot.From, taskAndDeliveryPeriodItem.TimeSlot.To, chartPeriodItem.TimeSlot.From, chartPeriodItem.TimeSlot.To) && taskAndDeliveryPeriodItem.TimeSlot.From < chartPeriodItem.TimeSlot.From)
                        {
                            #region Task overlapping before

                            int minutesBeforeOverlap = (int)(chartPeriodItem.TimeSlot.From - taskAndDeliveryPeriodItem.TimeSlot.From).TotalMinutes;
                            int minutesOverlap = (int)(taskAndDeliveryPeriodItem.TimeSlot.To - chartPeriodItem.TimeSlot.From).TotalMinutes;
                            taskAndDeliveryPeriodItem.TimeSlot.To = taskAndDeliveryPeriodItem.TimeSlot.From.AddMinutes(minutesBeforeOverlap);
                            taskAndDeliveryPeriodItem.TimeSlot.MaxTo = taskAndDeliveryPeriodItem.TimeSlot.To;
                            taskAndDeliveryPeriodItem.CalculationGuid = guid;
                            taskAndDeliveryPeriodItem.OriginalCalculationRowGuid = guid;
                            chartPeriodItem.CalculationGuid = guid;
                            chartPeriodItem.OriginalCalculationRowGuid = guid;
                            chartPeriodItem.Ignore = true;

                            int count = 0;
                            List<CalculationPeriodItem> splittedPeriodItems = SplitShift(chartPeriodItem, minutesOverlap);
                            foreach (CalculationPeriodItem splittedPeriodItem in splittedPeriodItems)
                            {
                                if (count == 0)
                                    splittedPeriodItem.Ignore = true;

                                if (!newPeriodItems.ContainsItem(splittedPeriodItem))
                                    newPeriodItems.Add(splittedPeriodItem);

                                count++;
                            }

                            changes = true;
                            break;

                            #endregion
                        }
                        else if (CalendarUtility.IsTimesOverlappingNew(taskAndDeliveryPeriodItem.TimeSlot.From, taskAndDeliveryPeriodItem.TimeSlot.To, chartPeriodItem.TimeSlot.From, chartPeriodItem.TimeSlot.To) && taskAndDeliveryPeriodItem.TimeSlot.To > chartPeriodItem.TimeSlot.To)
                        {
                            #region Task overlapping after

                            int minutesAfterOverlap = (int)(taskAndDeliveryPeriodItem.TimeSlot.To - chartPeriodItem.TimeSlot.To).TotalMinutes;
                            taskAndDeliveryPeriodItem.TimeSlot.From = taskAndDeliveryPeriodItem.TimeSlot.To.AddMinutes(-minutesAfterOverlap);
                            taskAndDeliveryPeriodItem.TimeSlot.MinFrom = taskAndDeliveryPeriodItem.TimeSlot.From;
                            taskAndDeliveryPeriodItem.CalculationGuid = guid;
                            taskAndDeliveryPeriodItem.OriginalCalculationRowGuid = guid;
                            chartPeriodItem.CalculationGuid = guid;
                            chartPeriodItem.OriginalCalculationRowGuid = guid;
                            chartPeriodItem.Ignore = true;
                            changes = true;
                            break;

                            #endregion
                        }
                    }

                    if (changes)
                        break;
                }
            }

            foreach (CalculationPeriodItem newPeriodItem in newPeriodItems)
            {
                if (!periodItems.ContainsItem(newPeriodItem))
                    periodItems.Add(newPeriodItem);
            }

            return MergeShifts(periodItems);
        }

        private List<CalculationPeriodItem> RemoveFromNeed(List<CalculationPeriodItem> needs, List<CalculationPeriodItem> shifts)
        {
            List<CalculationPeriodItem> newPeriodItems = new List<CalculationPeriodItem>();
            int interval = this.CalculationOptions.Interval;
            List<Tuple<DateTime, DateTime, int>> remainingBlocks = new List<Tuple<DateTime, DateTime, int>>();

            if (shifts != null && shifts.Count > 0)
            {
                Dictionary<DateTime, int> timeNeedDict = new Dictionary<DateTime, int>();
                DateTime start = shifts.Min(i => i.TimeSlot.From) < (needs.Any() ? needs.Min(i => i.TimeSlot.From) : DateTime.MinValue) ? shifts.Min(i => i.TimeSlot.From) : needs.Any() ? needs.Min(i => i.TimeSlot.From) : shifts.Min(i => i.TimeSlot.From);
                DateTime stop = shifts.Max(i => i.TimeSlot.To) > (needs.Any() ? needs.Max(i => i.TimeSlot.To) : DateTime.MaxValue) ? shifts.Max(i => i.TimeSlot.To) : needs.Any() ? needs.Max(i => i.TimeSlot.To) : shifts.Max(i => i.TimeSlot.To);
                DateTime current = start;
                while (current.AddMinutes(interval) <= stop)
                {
                    int fromShift = shifts.Where(period => CalendarUtility.IsNewOverlappedByCurrent(current, current.AddMinutes(interval), period.TimeSlot.From, period.TimeSlot.To)).Count();
                    int fromNeed = needs.Where(w => CalendarUtility.IsNewOverlappedByCurrent(current, current.AddMinutes(interval), w.TimeSlot.From, w.TimeSlot.To)).Count();
                    int remainingNeed = fromNeed - fromShift;
                    if (remainingNeed > 0)
                        timeNeedDict.Add(current, remainingNeed);
                    current = current.AddMinutes(interval);
                }

                int completedIterations = 0;
                Dictionary<DateTime, int> remainingTimeNeedDict = new Dictionary<DateTime, int>();
                remainingTimeNeedDict.AddRange(timeNeedDict.ToList());
                while (remainingTimeNeedDict.Count > 0)
                {
                    List<Tuple<DateTime, DateTime>> ranges = remainingTimeNeedDict.Select(i => i.Key).GetCoherentTimeRanges(interval);
                    foreach (Tuple<DateTime, DateTime> range in ranges)
                    {
                        remainingBlocks.Add(Tuple.Create(CalendarUtility.DATETIME_DEFAULT, range.Item1, (int)range.Item2.Subtract(range.Item1).TotalMinutes));
                    }
                    completedIterations++;
                    remainingTimeNeedDict = new Dictionary<DateTime, int>();
                    remainingTimeNeedDict.AddRange(timeNeedDict.Where(i => i.Value > completedIterations).ToList());
                }
            }
            else
            {
                return needs;
            }

            if (needs.Any())
            {
                int tempPeriodId = int.MinValue;

                foreach (Tuple<DateTime, DateTime, int> tuple in remainingBlocks)
                {
                    CalculationPeriodItem clone = ClonePeriodItem(needs.FirstOrDefault());
                    clone.TimeSlot = new StaffingNeedsCalculationTimeSlot(tuple.Item2, tuple.Item2.AddMinutes(tuple.Item3), tuple.Item3);
                    clone.CalculationGuid = Guid.NewGuid();
                    clone.OriginalCalculationRowGuid = Guid.NewGuid();
                    clone.CalculationPeriodItemGuid = Guid.NewGuid();
                    clone.StaffingNeedsRowPeriodId = tempPeriodId;
                    newPeriodItems.Add(clone);
                    tempPeriodId++;
                }
            }

            return newPeriodItems;
        }

        private List<CalculationPeriodItem> MergeShiftFromChartData(List<CalculationPeriodItem> periodItems, bool buildMountain = false)
        {
            List<CalculationPeriodItem> allChartPeriodItems = periodItems.Where(r => r.OriginType == StaffingNeedsRowOriginType.StaffingNeedsAnalysisChartData).ToList();
            List<CalculationPeriodItem> allTaskAndDeliveryPeriodItems = periodItems.Where(r => r.OriginType == StaffingNeedsRowOriginType.TimeScheduleTask && r.IsStaffingNeedsFrequency).ToList();
            List<CalculationPeriodItem> newPeriodItems = periodItems.Where(r => !allChartPeriodItems.Contains(r)).ToList();

            int interval = this.CalculationOptions.Interval;

            foreach (var chartPeriodItems in allChartPeriodItems.GroupBy(g => g.TimeScheduleTaskKey + "#" + g.IncomingDeliveryRowKey))
            {
                List<CalculationPeriodItem> taskAndDeliveryPeriodItems = periodItems.Where(r => !allChartPeriodItems.Contains(r) && (r.TimeScheduleTaskKey + "#" + r.IncomingDeliveryRowKey) == chartPeriodItems.Key && r.OriginType == StaffingNeedsRowOriginType.TimeScheduleTask && r.IsStaffingNeedsFrequency).ToList();

                if (taskAndDeliveryPeriodItems.Count == 0)
                {
                    newPeriodItems.AddRange(chartPeriodItems.ToList());
                    continue;
                }

                foreach (var item in taskAndDeliveryPeriodItems)
                    newPeriodItems.Remove(item);

                //Date, StartTime, length, isFixed
                List<Tuple<DateTime, DateTime, int>> remainingBlocks = new List<Tuple<DateTime, DateTime, int>>();
                if (chartPeriodItems != null && chartPeriodItems.Any())
                {

                    Dictionary<DateTime, int> timeNeedDict = new Dictionary<DateTime, int>();
                    DateTime start = chartPeriodItems.Min(i => i.TimeSlot.From) < (taskAndDeliveryPeriodItems.Any() ? taskAndDeliveryPeriodItems.Min(i => i.TimeSlot.From) : DateTime.MinValue) ? chartPeriodItems.Min(i => i.TimeSlot.From) : taskAndDeliveryPeriodItems.Any() ? taskAndDeliveryPeriodItems.Min(i => i.TimeSlot.From) : chartPeriodItems.Min(i => i.TimeSlot.From);
                    DateTime stop = chartPeriodItems.Max(i => i.TimeSlot.To) > (taskAndDeliveryPeriodItems.Any() ? taskAndDeliveryPeriodItems.Max(i => i.TimeSlot.To) : DateTime.MaxValue) ? chartPeriodItems.Max(i => i.TimeSlot.To) : taskAndDeliveryPeriodItems.Any() ? taskAndDeliveryPeriodItems.Max(i => i.TimeSlot.To) : chartPeriodItems.Max(i => i.TimeSlot.To);
                    DateTime current = start;
                    while (current.AddMinutes(interval) <= stop)
                    {
                        int fromChart = chartPeriodItems.Where(period => CalendarUtility.IsNewOverlappedByCurrent(current, current.AddMinutes(interval), period.TimeSlot.From, period.TimeSlot.To)).Count();
                        int fromTask = taskAndDeliveryPeriodItems.Where(w => CalendarUtility.IsNewOverlappedByCurrent(current, current.AddMinutes(interval), w.TimeSlot.From, w.TimeSlot.To)).Count();
                        int remainingNeed = fromChart - fromTask;
                        if (remainingNeed > 0)
                            timeNeedDict.Add(current, remainingNeed);
                        current = current.AddMinutes(interval);
                    }

                    int completedIterations = 0;
                    Dictionary<DateTime, int> remainingTimeNeedDict = new Dictionary<DateTime, int>();
                    remainingTimeNeedDict.AddRange(timeNeedDict.ToList());
                    while (remainingTimeNeedDict.Count > 0)
                    {
                        List<Tuple<DateTime, DateTime>> ranges = remainingTimeNeedDict.Select(i => i.Key).GetCoherentTimeRanges(interval);
                        foreach (Tuple<DateTime, DateTime> range in ranges)
                        {
                            if (taskAndDeliveryPeriodItems.Any(a => a.TimeSlot.To == range.Item1))
                            {
                                var first = taskAndDeliveryPeriodItems.FirstOrDefault(f => f.TimeSlot.To == range.Item1);
                                first.TimeSlot.MaxTo = first.TimeSlot.MaxTo.AddMinutes((int)range.Item2.Subtract(range.Item1).TotalMinutes);
                                first.TimeSlot.To = first.TimeSlot.To.AddMinutes((int)range.Item2.Subtract(range.Item1).TotalMinutes);
                            }
                            else if (taskAndDeliveryPeriodItems.Any(a => a.TimeSlot.From == range.Item2))
                            {
                                var first = taskAndDeliveryPeriodItems.FirstOrDefault(f => f.TimeSlot.From == range.Item2);
                                first.TimeSlot.MinFrom = first.TimeSlot.MinFrom.AddMinutes(-(int)range.Item2.Subtract(range.Item1).TotalMinutes);
                                first.TimeSlot.From = first.TimeSlot.From.AddMinutes(-(int)range.Item2.Subtract(range.Item1).TotalMinutes);
                            }
                            else
                                remainingBlocks.Add(Tuple.Create(CalendarUtility.DATETIME_DEFAULT, range.Item1, (int)range.Item2.Subtract(range.Item1).TotalMinutes));
                        }
                        completedIterations++;
                        remainingTimeNeedDict = new Dictionary<DateTime, int>();
                        remainingTimeNeedDict.AddRange(timeNeedDict.Where(i => i.Value > completedIterations).ToList());
                    }
                }
                else
                {
                    continue;
                }

                if (taskAndDeliveryPeriodItems.Any())
                {
                    int tempPeriodId = int.MinValue;

                    foreach (Tuple<DateTime, DateTime, int> tuple in remainingBlocks)
                    {
                        CalculationPeriodItem clone = ClonePeriodItem(taskAndDeliveryPeriodItems.FirstOrDefault());
                        clone.TimeSlot = new StaffingNeedsCalculationTimeSlot(tuple.Item2, tuple.Item2.AddMinutes(tuple.Item3), tuple.Item3);
                        clone.CalculationGuid = Guid.NewGuid();
                        clone.OriginalCalculationRowGuid = Guid.NewGuid();
                        clone.CalculationPeriodItemGuid = Guid.NewGuid();
                        clone.StaffingNeedsRowPeriodId = tempPeriodId;
                        tempPeriodId++;
                        newPeriodItems.Add(clone);
                    }


                    foreach (var task in taskAndDeliveryPeriodItems)
                    {
                        task.StaffingNeedsRowPeriodId = tempPeriodId;
                        task.CalculationGuid = Guid.NewGuid();
                        task.OriginalCalculationRowGuid = Guid.NewGuid();
                        task.CalculationPeriodItemGuid = Guid.NewGuid();
                        tempPeriodId++;
                    }

                    newPeriodItems.AddRange(taskAndDeliveryPeriodItems);
                }
            }

            if (buildMountain)
            {
                newPeriodItems = FallLikeTetris(newPeriodItems, false);
                OrderAndSetCalcutionRowNumber(newPeriodItems, false);
            }

            return newPeriodItems;
        }

        private List<CalculationPeriodItem> MergeShifts(List<CalculationPeriodItem> periodItems, bool allowAdjustment = false)
        {
            List<CalculationPeriodItem> newPeriodItems = new List<CalculationPeriodItem>();

            foreach (var firstGrouping in periodItems.GroupBy(r => r.KeyForMerge()))
            {
                foreach (var secondGrouping in firstGrouping.GroupBy(b => b.CalculationGuid))
                {
                    newPeriodItems.AddRange(MergeShift(secondGrouping.ToList(), allowAdjustment: allowAdjustment));
                }
            }

            return newPeriodItems.OrderBy(i => i.StartTime).ToList();
        }

        private List<CalculationPeriodItem> MergeShift(List<CalculationPeriodItem> periodItems, bool allowAdjustment = false)
        {
            List<CalculationPeriodItem> newPeriodItems = new List<CalculationPeriodItem>();

            #region Check if shifts are connected

            List<Tuple<Guid, CalculationPeriodItem>> connectedShifts = GetConnectedShifts(periodItems, allowAdjustment);

            #endregion

            #region Create new Rows

            var connectedGroups = connectedShifts.GroupBy(i => i.Item1);

            foreach (var connectedGroup in connectedGroups)
            {
                if (connectedGroup.Count() == 1)
                {
                    newPeriodItems.Add(connectedGroup.FirstOrDefault().Item2);
                    continue;
                }
                CalculationPeriodItem first = connectedGroup.FirstOrDefault().Item2;
                CalculationPeriodItem last = connectedGroup.LastOrDefault().Item2;

                #region Set Values on TimeSlot

                StaffingNeedsCalculationTimeSlot timeslot = first.TimeSlot;
                timeslot.To = last.TimeSlot.To;
                timeslot.MaxTo = last.TimeSlot.MaxTo;
                first.TimeSlot = timeslot;
                newPeriodItems.Add(first);

                #endregion
            }

            #endregion

            return newPeriodItems;
        }

        private List<StaffingNeedsCalculationTimeSlot> CreateFakeTimeSlots()
        {
            List<StaffingNeedsCalculationTimeSlot> fakeTimeSlots = new List<StaffingNeedsCalculationTimeSlot>();
            DateTime? date = this.Date;

            //var connectedShifts = GetAllNotDoneConnectedShifts();
            //foreach (var connectedGroup in connectedShifts.GroupBy(i => i.Item1))
            //{
            //    List<CalculationPeriodItem> itemOnPartOfGroup = connectedGroup.Select(t => t.Item2).Where(w => !w.IsBreak).OrderBy(r => r.TimeSlot.From).ToList();
            //    if (itemOnPartOfGroup.Count == 0)
            //        continue;

            //    CalculationPeriodItem firstItem = itemOnPartOfGroup.First();
            //    CalculationPeriodItem lastItem = itemOnPartOfGroup.Last();
            //    int length = (int)((lastItem.TimeSlot.To - lastItem.TimeSlot.From).TotalMinutes);

            //    StaffingNeedsCalculationTimeSlot timeSlot = new StaffingNeedsCalculationTimeSlot(firstItem.TimeSlot.From, lastItem.TimeSlot.To, length);
            //    timeSlot.ShiftTypeId = lastItem.ShiftTypeId;
            //    timeSlot.CalculationGuid = lastItem.CalculationGuid;
            //    fakeTimeSlots.Add(timeSlot);
            //}

            if (date.HasValue && date.Value > DateTime.UtcNow.AddYears(-10) && this.CurrentEmployeePostCyclesRun != null && this.CurrentEmployeePostCyclesRun.EmployeePostCycles != null && this.CurrentEmployeePostCyclesRun.EmployeePostCycles.Any())
            {
                foreach (var cycle in this.CurrentEmployeePostCyclesRun.EmployeePostCycles)
                {
                    var oldConnectedShifts = GetConnectedShifts(ClonePeriodItems(cycle.GetSelectedPeriodItems(date.Value).Where(u => u.IsBreak).ToList()), allowAdjustment: false);

                    foreach (var connectedGroup in oldConnectedShifts.GroupBy(i => i.Item1))
                    {
                        List<CalculationPeriodItem> itemOnPartOfGroup = connectedGroup.Select(t => t.Item2).OrderBy(r => r.TimeSlot.From).ToList();
                        if (itemOnPartOfGroup.Count == 0)
                            continue;

                        CalculationPeriodItem firstItem = itemOnPartOfGroup.First();
                        CalculationPeriodItem lastItem = itemOnPartOfGroup.Last();
                        int length = (int)((lastItem.TimeSlot.To - lastItem.TimeSlot.From).TotalMinutes);

                        StaffingNeedsCalculationTimeSlot timeSlot = new StaffingNeedsCalculationTimeSlot(firstItem.TimeSlot.From, lastItem.TimeSlot.To, length);
                        timeSlot.ShiftTypeId = lastItem.ShiftTypeId;
                        timeSlot.CalculationGuid = lastItem.CalculationGuid;
                        timeSlot.IsBreak = true;
                        fakeTimeSlots.Add(timeSlot);
                    }
                }
            }



            return fakeTimeSlots;
        }

        private List<Tuple<Guid, CalculationPeriodItem>> GetAllNotDoneConnectedShifts()
        {
            List<Tuple<Guid, CalculationPeriodItem>> connectedShifts = new List<Tuple<Guid, CalculationPeriodItem>>();

            if (this.PeriodItemsGroupHeads != null && this.PeriodItemsGroupHeads.Any())
            {
                foreach (PeriodItemsGroupHead head in this.PeriodItemsGroupHeads.Where(h => !h.Done))
                {
                    connectedShifts.AddRange(GetConnectedShifts(head.CalculationPeriodItems));
                }
            }

            return connectedShifts;
        }

        private List<Tuple<Guid, CalculationPeriodItem>> GetConnectedShifts(List<CalculationPeriodItem> periodItems, bool allowAdjustment = false, bool checkValidLength = false, bool ignoreGuid = false, bool ignoreAllowOverlapping = false)
        {
            List<Tuple<Guid, CalculationPeriodItem>> connectedShifts = new List<Tuple<Guid, CalculationPeriodItem>>();

            CalculationPeriodItem prevStaffingNeedsCalcutionRow = null;
            Guid guid = Guid.NewGuid();

            periodItems = periodItems.OrderBy(t => t.TimeSlot.From).ToList();
            foreach (CalculationPeriodItem periodItem in periodItems.OrderBy(i => i.TimeSlot.From))
            {
                if (prevStaffingNeedsCalcutionRow == null)
                {
                    connectedShifts.Add(Tuple.Create(guid, periodItem));
                    prevStaffingNeedsCalcutionRow = periodItem;
                    continue;

                }
                if (!periodItem.IsBreak && !prevStaffingNeedsCalcutionRow.IsBreak && prevStaffingNeedsCalcutionRow.TimeSlot.To == periodItem.TimeSlot.From && (ignoreGuid || prevStaffingNeedsCalcutionRow.CalculationGuid == periodItem.CalculationGuid))
                {
                    if (checkValidLength)
                    {
                        int length = connectedShifts.Where(i => i.Item1 == guid).Sum(i => i.Item2.Length) + periodItem.Length - GetBreakMinutesBeforeSplit(periodItem);

                        if (length <= this.CalculationOptions.MaxLength)
                        {
                            connectedShifts.Add(Tuple.Create(guid, periodItem));
                            prevStaffingNeedsCalcutionRow = periodItem;
                            continue;
                        }
                    }
                    else
                    {
                        connectedShifts.Add(Tuple.Create(guid, periodItem));
                    }
                }
                else if (!periodItem.IsBreak && !prevStaffingNeedsCalcutionRow.IsBreak && allowAdjustment && !periodItem.TimeSlot.IsFixed && (ignoreAllowOverlapping || periodItem.AllowOverlapping))
                {
                    #region Check if we can merge after adjustment

                    StaffingNeedsCalculationTimeSlot timeSlot = periodItem.TimeSlot.Clone();

                    AdjustTimeSlot(timeSlot, CalculationOptions.Interval, true, true);

                    int iterations = 24 * 60 / CalculationOptions.Interval;
                    int iteration = 0;
                    bool added = false;

                    while (iteration <= iterations && timeSlot.To <= periodItem.TimeSlot.MaxTo)
                    {
                        if (prevStaffingNeedsCalcutionRow.TimeSlot.To == timeSlot.From && (ignoreGuid || prevStaffingNeedsCalcutionRow.CalculationGuid == periodItem.CalculationGuid))
                        {
                            periodItem.TimeSlot = timeSlot;
                            if (checkValidLength)
                            {
                                int length = connectedShifts.Where(i => i.Item1 == guid).Sum(i => i.Item2.Length) + periodItem.Length - GetBreakMinutesBeforeSplit(periodItem);

                                if (length <= this.CalculationOptions.MaxLength)
                                {
                                    connectedShifts.Add(Tuple.Create(guid, periodItem));
                                    prevStaffingNeedsCalcutionRow = periodItem;
                                    iteration = iterations;
                                    added = true;
                                }
                            }
                            else
                            {
                                connectedShifts.Add(Tuple.Create(guid, periodItem));
                                prevStaffingNeedsCalcutionRow = periodItem;
                                iteration = iterations;
                                added = true;
                            }
                        }
                        else
                            AdjustTimeSlot(timeSlot, CalculationOptions.Interval, false, true);

                        iteration++;
                    }

                    if (!added)
                    {
                        guid = Guid.NewGuid();
                        connectedShifts.Add(Tuple.Create(guid, periodItem));
                    }

                    #endregion
                }
                else if (!this.CalculationOptions.AddOnlyDockedPeriodItems && this.CalculationOptions.EmployeePost != null && !periodItem.IsBreak && (connectedShifts.Count == 0 || connectedShifts.Select(s => s.Item2.OriginalCalculationRowGuid).Contains(periodItem.OriginalCalculationRowGuid)))
                {
                    connectedShifts.Add(Tuple.Create(guid, periodItem));
                }
                else if (!this.CalculationOptions.AddOnlyDockedPeriodItems && this.CalculationOptions.EmployeePost == null)
                {
                    connectedShifts.Add(Tuple.Create(guid, periodItem));
                }
                else
                {
                    guid = Guid.NewGuid();
                    connectedShifts.Add(Tuple.Create(guid, periodItem));
                }

                prevStaffingNeedsCalcutionRow = periodItem;
            }

            return connectedShifts;
        }

        private List<CalculationPeriodItem> SplitShifts(List<CalculationPeriodItem> periodItems, int forceLengthMinutes, List<DateTime> splitToTimes, List<DateTime> splitFromTimes, bool discardOnlyOneEmployee = false)
        {
            List<CalculationPeriodItem> remainingPeriodItems = new List<CalculationPeriodItem>();
            List<CalculationPeriodItem> newPeriodItems = new List<CalculationPeriodItem>();

            foreach (var splitTime in splitToTimes)
            {
                foreach (CalculationPeriodItem periodItem in periodItems)
                {
                    if (CalendarUtility.GetOverlappingMinutes(periodItem.TimeSlot.MinFrom, periodItem.TimeSlot.MaxTo, splitTime.AddMinutes(-forceLengthMinutes), splitTime) >= forceLengthMinutes)
                    {
                        var splitted = SplitShift(periodItem, splitTime.AddMinutes(-forceLengthMinutes), splitTime, discardOnlyOneEmployee: discardOnlyOneEmployee);

                        foreach (var split in splitted.Where(w => w.TimeSlot.To == splitTime).ToList())
                        {
                            if (!newPeriodItems.Select(s => s.OriginalCalculationRowGuid).Contains(split.OriginalCalculationRowGuid))
                                newPeriodItems.Add(split);
                        }
                    }
                    else
                    {
                        remainingPeriodItems.Add(periodItem);
                    }
                }
            }

            foreach (var splitTime in splitFromTimes.OrderByDescending(o => o))
            {
                foreach (CalculationPeriodItem periodItem in remainingPeriodItems)
                {
                    if (CalendarUtility.GetOverlappingMinutes(periodItem.TimeSlot.MinFrom, periodItem.TimeSlot.MaxTo, splitTime, splitTime.AddMinutes(forceLengthMinutes)) >= forceLengthMinutes)
                    {
                        var splitted = SplitShift(periodItem, splitTime, splitTime.AddMinutes(forceLengthMinutes), discardOnlyOneEmployee: discardOnlyOneEmployee);

                        foreach (var split in splitted.Where(w => w.TimeSlot.From == splitTime).ToList())
                        {
                            if (!newPeriodItems.Select(s => s.OriginalCalculationRowGuid).Contains(split.OriginalCalculationRowGuid))
                                newPeriodItems.Add(split);
                        }
                    }
                }
            }

            return newPeriodItems;
        }

        private List<CalculationPeriodItem> SplitShifts(List<CalculationPeriodItem> periodItems, int forceLengthMinutes = 0, bool discardOnlyOneEmployee = false, bool runRecursive = true, bool setNewGuid = true, bool forceMinSplitLength = false, bool tryReverse = false)
        {
            List<CalculationPeriodItem> newPeriodItems = new List<CalculationPeriodItem>();
            foreach (CalculationPeriodItem periodItem in periodItems)
            {
                bool reverse = false;

                if (tryReverse && periodItem.TimeSlot.From > CalendarUtility.DATETIME_DEFAULT.AddHours(16))
                    reverse = true;

                newPeriodItems.AddRange(SplitShift(periodItem, forceLengthMinutes: forceLengthMinutes, runRecursive: runRecursive, forceMinSplitLength: forceMinSplitLength, setNewGuid: setNewGuid, discardOnlyOneEmployee: discardOnlyOneEmployee, reverse: reverse));
            }
            return newPeriodItems;
        }

        private List<CalculationPeriodItem> SplitShift(CalculationPeriodItem periodItem, DateTime firstSplit, DateTime secondSplit, bool discardOnlyOneEmployee = false, bool runRecursive = false, bool setNewGuid = true)
        {
            List<CalculationPeriodItem> newPeriodItems = new List<CalculationPeriodItem>();
            if (periodItem.OnlyOneEmployee && !discardOnlyOneEmployee)
            {
                newPeriodItems.Add(periodItem);
                return newPeriodItems;
            }

            int firstLength = (int)(firstSplit - periodItem.TimeSlot.From).TotalMinutes;
            if (firstLength == 0)
                firstLength = CalculationOptions.Interval;

            List<CalculationPeriodItem> periodItemsAfterSplitOne = SplitShift(periodItem, firstLength, discardOnlyOneEmployee: discardOnlyOneEmployee, runRecursive: runRecursive, setNewGuid: setNewGuid).OrderByDescending(o => o.TimeSlot.From).ToList();
            if (periodItemsAfterSplitOne.Any())
            {
                newPeriodItems.Add(periodItemsAfterSplitOne.LastOrDefault());

                int secondLength = (int)(secondSplit - (periodItemsAfterSplitOne.FirstOrDefault()?.TimeSlot.From ?? secondSplit)).TotalMinutes;
                if (secondLength == 0)
                    secondLength = CalculationOptions.Interval;

                if (newPeriodItems.Any())
                {
                    List<CalculationPeriodItem> periodItemsAfterSplitTwo = SplitShift(periodItemsAfterSplitOne.FirstOrDefault(), secondLength, discardOnlyOneEmployee: discardOnlyOneEmployee, runRecursive: runRecursive, setNewGuid: setNewGuid);
                    newPeriodItems.AddRange(periodItemsAfterSplitTwo);
                }
            }
            return newPeriodItems.OrderBy(i => i.TimeSlot.From).ToList();
        }

        private List<CalculationPeriodItem> SplitShift(CalculationPeriodItem periodItem, DateTime splitTime, bool discardOnlyOneEmployee = false, bool runRecursive = false, bool setNewGuid = true, bool forceMinSplitLength = false)
        {
            if (CalendarUtility.IsDateInRange(splitTime, periodItem.TimeSlot.From, periodItem.TimeSlot.To))
            {
                int forceLengthMinutes = Convert.ToInt32((splitTime - periodItem.TimeSlot.From).TotalMinutes);
                return SplitShift(periodItem, forceLengthMinutes, discardOnlyOneEmployee, runRecursive, setNewGuid, forceMinSplitLength, reverse: false);
            }
            else
                return new List<CalculationPeriodItem>() { periodItem };
        }

        private List<CalculationPeriodItem> SplitShift(CalculationPeriodItem periodItem, int forceLengthMinutes = 0, bool discardOnlyOneEmployee = false, bool runRecursive = true, bool setNewGuid = true, bool forceMinSplitLength = false, bool reverse = false)
        {
            List<CalculationPeriodItem> newPeriodItems = new List<CalculationPeriodItem>();
            if (periodItem.OnlyOneEmployee && !discardOnlyOneEmployee)
            {
                newPeriodItems.Add(periodItem);
                return newPeriodItems;
            }


            Guid originalCalculationRowGuid = periodItem.CalculationGuid;

            int lengthMinutes = int.MaxValue;
            if (forceLengthMinutes != 0)
            {
                lengthMinutes = CalendarUtility.AdjustAccordingToInterval(forceLengthMinutes, this.CalculationOptions.Interval, alwaysReduce: true);
            }
            else if (CalculationOptions.ApplyLengthRule)
            {
                lengthMinutes = GetSplitLength(periodItem);
                if (lengthMinutes < 0)
                    return new List<CalculationPeriodItem>() { periodItem };

                if (lengthMinutes < int.MaxValue)
                    lengthMinutes = lengthMinutes + GetBreakMinutesBeforeSplit(periodItem);

                if (periodItem.UnderMinSplitLenght(lengthMinutes))
                    lengthMinutes = periodItem.MinSplitLength;

                if (forceMinSplitLength && periodItem.OverMinSplitLenght(lengthMinutes))
                {
                    lengthMinutes = periodItem.MinSplitLength;
                }
            }

            if (periodItem.TimeSlot.Minutes <= lengthMinutes)
                return new List<CalculationPeriodItem>() { periodItem };

            if (periodItem.TimeSlot.Minutes > lengthMinutes && (periodItem.OverMinSplitLenght(periodItem.TimeSlot.Minutes) || forceLengthMinutes != 0))
            {
                #region Set Values on new periodItem

                CalculationPeriodItem newPeriodItem = !reverse ? periodItem.CloneAndSetNewTime(lengthMinutes, setNewCalculationPeriodItemGuid: true) : periodItem.CloneAndSetNewTimeTo(periodItem.TimeSlot.To, lengthMinutes, setNewCalculationPeriodItemGuid: true);

                #endregion

                #region NotReverse

                if (!reverse)
                {
                    #region Change values on the old row

                    periodItem.TimeSlot.To = newPeriodItem.TimeSlot.From;
                    if (!newPeriodItem.AllowOverlapping)
                        periodItem.TimeSlot.MaxTo = newPeriodItem.TimeSlot.From;

                    #endregion

                    #region Add rows to new list. If new timeslot is still to long, run recursive

                    newPeriodItems.Add(periodItem);
                    if (runRecursive && newPeriodItem.TimeSlot.Minutes > lengthMinutes)
                        newPeriodItems.AddRange(SplitShift(newPeriodItem, lengthMinutes));
                    else
                        newPeriodItems.Add(newPeriodItem);

                    #endregion
                }
                #endregion

                #region Reverse (else)

                else if (reverse)
                {
                    #region Change values on the old row

                    periodItem.TimeSlot.From = newPeriodItem.TimeSlot.To;
                    if (!newPeriodItem.AllowOverlapping)
                        periodItem.TimeSlot.MinFrom = newPeriodItem.TimeSlot.To;

                    #endregion

                    #region Add rows to new list. If new timeslot is still to long, run recursive

                    newPeriodItems.Add(periodItem);
                    if (runRecursive && newPeriodItem.TimeSlot.Minutes > lengthMinutes)
                        newPeriodItems.AddRange(SplitShift(newPeriodItem, lengthMinutes));
                    else
                        newPeriodItems.Add(newPeriodItem);

                    #endregion
                }
                #endregion
            }
            else
            {
                newPeriodItems.Add(periodItem);
            }

            foreach (CalculationPeriodItem newPeriodItem in newPeriodItems)
            {
                Guid guid = setNewGuid ? Guid.NewGuid() : originalCalculationRowGuid;
                newPeriodItem.CalculationGuid = guid;
                newPeriodItem.OriginalCalculationRowGuid = originalCalculationRowGuid;
            }

            return newPeriodItems.OrderBy(i => i.TimeSlot.From).ToList();
        }

        #endregion

        #region Schedule 

        private TimeScheduleTemplateHeadDTO CreateTimeScheduleTemplateHeadDTO(Dictionary<DateTime, List<CalculationPeriodItem>> dict, EmployeePostDTO employeePostDTO, int timeCodeId, int noOfDays, DateTime startDate, DateTime stopDate)
        {
            TimeScheduleTemplateHeadDTO timeScheduleTemplateHeadDTO = new TimeScheduleTemplateHeadDTO()
            {
                TimeScheduleTemplateHeadId = 0,
                ActorCompanyId = 0,
                EmployeePostId = employeePostDTO.EmployeePostId,
                Name = string.Empty,
                Description = string.Empty,
                NoOfDays = noOfDays,
                StartOnFirstDayOfWeek = true,
                StartDate = startDate,
                StopDate = stopDate,
                FlexForceSchedule = false,
                Locked = false,
                State = SoeEntityState.Active,
                TimeScheduleTemplatePeriods = new List<TimeScheduleTemplatePeriodDTO>()
            };

            int currentDay = 1;
            DateTime date = startDate;

            while (currentDay <= noOfDays)
            {
                TimeScheduleTemplatePeriodDTO timeScheduleTemplatePeriodDTO = new TimeScheduleTemplatePeriodDTO()
                {
                    DayNumber = currentDay,
                    State = SoeEntityState.Active
                };

                Guid guid = Guid.NewGuid();
                var periodItems = new List<CalculationPeriodItem>();
                foreach (var item in dict.Where(k => k.Key == startDate))
                    periodItems.AddRange(item.Value);

                timeScheduleTemplatePeriodDTO.TimeScheduleTemplateBlocks = new List<TimeScheduleTemplateBlockDTO>();

                foreach (var periodItem in periodItems)
                {
                    TimeScheduleTemplateBlockDTO timeScheduleTemplateBlockDTO = new TimeScheduleTemplateBlockDTO()
                    {
                        TimeDeviationCauseStatus = SoeTimeScheduleDeviationCauseStatus.None,
                        TimeCodeId = timeCodeId,
                        Type = TermGroup_TimeScheduleTemplateBlockType.Need,
                        ShiftTypeId = periodItem.ShiftTypeId,
                        StartTime = periodItem.TimeSlot.From,
                        StopTime = periodItem.TimeSlot.To,
                        BreakType = periodItem.IsBreak ? SoeTimeScheduleTemplateBlockBreakType.NormalBreak : SoeTimeScheduleTemplateBlockBreakType.None,
                        Link = guid,
                        StaffingNeedsRowId = periodItem.StaffingNeedsRowId,
                        State = SoeEntityState.Active
                    };

                    timeScheduleTemplatePeriodDTO.TimeScheduleTemplateBlocks.Add(timeScheduleTemplateBlockDTO);
                }

                timeScheduleTemplateHeadDTO.TimeScheduleTemplatePeriods.Add(timeScheduleTemplatePeriodDTO);
                currentDay++;
            }

            return timeScheduleTemplateHeadDTO;
        }

        public List<TimeSchedulePlanningDayDTO> CreateTimeSchedulePlanningDayDTO(EmployeePostCycle employeePostCycle, EmployeePostDTO employeePost, int timeCodeId, int noOfDays, DateTime startDate, DateTime stopDate, List<TimeCodeBreakGroupDTO> timeCodeBreakGroups)
        {
            List<TimeSchedulePlanningDayDTO> days = new List<TimeSchedulePlanningDayDTO>();
            List<EmployeePostDay> employeePostDays = new List<EmployeePostDay>();
            TimeCodeManager tcm = new TimeCodeManager(null);

            foreach (EmployeePostWeek week in employeePostCycle.EmployeePostWeeks.Where(w => !w.IsCopy))
                foreach (var day in week.EmployeePostDays)
                    employeePostDays.Add(day);

            #region Create Zerodays

            DateTime currentDay = startDate;
            while (currentDay < startDate.AddDays(noOfDays))
            {
                if (employeePostDays.Where(d => d.Date == currentDay).Count() == 0)
                    employeePostDays.Add(new EmployeePostDay(currentDay, new List<CalculationPeriodItem>() { new CalculationPeriodItem() }, this.CalculationOptions));
                currentDay = currentDay.AddDays(1);
            }

            #endregion

            int dayNumber = 1;

            foreach (var day in employeePostDays.OrderBy(o => o.Date))
            {
                Guid link = Guid.NewGuid();
                List<CalculationPeriodItem> inputBreaks = day.SelectedItemsHead.CalculationPeriodItems.Where(v => v.IsBreak).ToList();
                List<CalculationPeriodItem> inputNoneBreaks = day.SelectedItemsHead.CalculationPeriodItems.Where(v => !v.IsBreak).ToList();

                foreach (CalculationPeriodItem periodItem in inputNoneBreaks)
                {
                    DateTime startTime = CalendarUtility.MergeDateAndTime(day.Date, periodItem.TimeSlot != null ? periodItem.TimeSlot.From : CalendarUtility.DATETIME_DEFAULT, ignoreSqlServerDateTime: true);
                    DateTime stopTime = CalendarUtility.MergeDateAndTime(day.Date, periodItem.TimeSlot != null ? periodItem.TimeSlot.To : CalendarUtility.DATETIME_DEFAULT, ignoreSqlServerDateTime: true);
                    if (startTime == stopTime)
                        continue;

                    #region Overlap breaks

                    if (inputBreaks.Any())
                    {
                        var dockingBreak = inputBreaks.Where(i => i.TimeSlot.From == periodItem.TimeSlot.To).FirstOrDefault();

                        if (dockingBreak != null)
                        {
                            double minutes = (dockingBreak.TimeSlot.To - periodItem.TimeSlot.To).TotalMinutes;
                            stopTime = stopTime.AddMinutes(minutes);
                        }
                    }

                    #endregion

                    TimeSchedulePlanningDayDTO dayDTO = new TimeSchedulePlanningDayDTO();
                    dayDTO.AccountId = employeePost.AccountId;
                    dayDTO.PeriodGuid = periodItem.PeriodGuid;
                    dayDTO.Type = (TermGroup_TimeScheduleTemplateBlockType)periodItem.Type;
                    dayDTO.UserId = (int?)null;
                    dayDTO.EmployeeId = 0;
                    dayDTO.EmployeeName = employeePost.Name;
                    dayDTO.EmployeeInfo = employeePost.Description;
                    dayDTO.IsHiddenEmployee = false;
                    dayDTO.EmployeePostId = employeePost.EmployeePostId;
                    dayDTO.IsVacant = false;
                    dayDTO.TimeScheduleTemplateBlockId = 0;
                    dayDTO.TimeScheduleTemplateHeadId = 0;
                    dayDTO.TimeScheduleTemplatePeriodId = 0;
                    dayDTO.TimeScheduleEmployeePeriodId = 0;
                    dayDTO.TimeDeviationCauseId = 0;
                    dayDTO.TimeDeviationCauseName = String.Empty;
                    dayDTO.AbsenceType = TermGroup_TimeScheduleTemplateBlockAbsenceType.Standard;
                    dayDTO.TimeCodeId = timeCodeId;
                    dayDTO.TimeScheduleTypeId = 0;
                    dayDTO.TimeScheduleTypeCode = String.Empty;
                    dayDTO.TimeScheduleTypeName = String.Empty;
                    dayDTO.TimeScheduleTypeIsNotScheduleTime = false;
                    dayDTO.TimeScheduleTypeFactors = null;
                    dayDTO.ShiftTypeTimeScheduleTypeId = 0;
                    dayDTO.ShiftTypeTimeScheduleTypeCode = String.Empty;
                    dayDTO.ShiftTypeTimeScheduleTypeName = String.Empty;
                    dayDTO.ShiftTypeId = periodItem.ShiftTypeId.HasValue ? periodItem.ShiftTypeId.Value : 0;
                    dayDTO.ShiftTypeName = periodItem.ShiftType != null ? periodItem.ShiftType.Name : string.Empty;
                    dayDTO.ShiftTypeDesc = periodItem.ShiftType != null ? periodItem.ShiftType.Description : string.Empty;
                    dayDTO.ShiftTypeColor = periodItem.ShiftType != null ? periodItem.ShiftType.Color : string.Empty;
                    dayDTO.TimeScheduleTaskId = periodItem.TimeScheduleTaskId;
                    dayDTO.IncomingDeliveryRowId = periodItem.IncomingDeliveryRowId;
                    dayDTO.StartTime = startTime;
                    dayDTO.StopTime = stopTime;
                    dayDTO.WeekNr = CalendarUtility.GetWeekNr(dayDTO.StartTime);
                    dayDTO.BelongsToPreviousDay = false;
                    dayDTO.BelongsToNextDay = false;
                    dayDTO.Description = string.Empty;
                    dayDTO.ShiftStatus = TermGroup_TimeScheduleTemplateBlockShiftStatus.Assigned;
                    dayDTO.ShiftUserStatus = TermGroup_TimeScheduleTemplateBlockShiftUserStatus.Accepted;
                    dayDTO.IsPreliminary = false;
                    dayDTO.NbrOfWantedInQueue = 0;
                    dayDTO.IamInQueue = false;
                    dayDTO.DayNumber = dayNumber;
                    dayDTO.NbrOfWeeks = noOfDays / 7;
                    dayDTO.Link = link;
                    dayDTO.CalculationGuid = periodItem.CalculationGuid;

                    // Staffing needs
                    dayDTO.StaffingNeedsRowId = periodItem.StaffingNeedsRowId;
                    dayDTO.StaffingNeedsRowPeriodId = periodItem.StaffingNeedsRowPeriodId;
                    dayDTO.StaffingNeedsOrigin = periodItem.Name;
                    dayDTO.StaffingNeedsWeekday = periodItem.Weekday;
                    dayDTO.StaffingNeedsDayTypeId = periodItem.DayTypeId;
                    dayDTO.StaffingNeedsDate = day.Date;

                    int breakNr = 0;

                    foreach (var inputBreak in inputBreaks.OrderBy(x => x.StartTime))
                    {
                        if (!inputBreak.IsBreak)
                            continue;

                        breakNr++;
                        Guid? breakGuid = link;

                        TimeCodeBreakDTO timeCode = null;

                        if (!timeCodeBreakGroups.IsNullOrEmpty() && employeePost.EmployeeGroupId.HasValue && inputBreak.TimeCodeBreakGroupId.HasValue)
                        {
                            var tc = timeCodeBreakGroups.FirstOrDefault(f => f.TimeCodeBreakGroupId == inputBreak.TimeCodeBreakGroupId && !f.TimeCodeBreaks.IsNullOrEmpty())?.TimeCodeBreaks.FirstOrDefault();

                            if (tc != null)
                                timeCode = new TimeCodeBreakDTO() { DefaultMinutes = tc.DefaultMinutes, TimeCodeId = tc.TimeCodeId };
                        }

                        if (timeCode == null && employeePost.EmployeeGroupId.HasValue && inputBreak.TimeCodeBreakGroupId.HasValue)
                            timeCode = tcm.GetTimeCodeBreakDTOForEmployeeGroup(employeePost.EmployeeGroupId.Value, inputBreak.TimeCodeBreakGroupId.Value);

                        if (breakNr == 1)
                        {
                            dayDTO.Break1Id = 0;
                            dayDTO.Break1Link = breakGuid;
                            dayDTO.Break1Minutes = (timeCode != null) ? timeCode.DefaultMinutes : 0;
                            dayDTO.Break1StartTime = CalendarUtility.MergeDateAndTime(day.Date, inputBreak.TimeSlot != null ? inputBreak.TimeSlot.From : CalendarUtility.DATETIME_DEFAULT, ignoreSqlServerDateTime: true);
                            dayDTO.Break1TimeCodeId = (timeCode != null) ? timeCode.TimeCodeId : 0;
                        }
                        if (breakNr == 2)
                        {
                            dayDTO.Break2Id = 0;
                            dayDTO.Break2Link = breakGuid;
                            dayDTO.Break2Minutes = (timeCode != null) ? timeCode.DefaultMinutes : 0;
                            dayDTO.Break2StartTime = CalendarUtility.MergeDateAndTime(day.Date, inputBreak.TimeSlot != null ? inputBreak.TimeSlot.From : CalendarUtility.DATETIME_DEFAULT, ignoreSqlServerDateTime: true);
                            dayDTO.Break2TimeCodeId = (timeCode != null) ? timeCode.TimeCodeId : 0;
                        }
                        if (breakNr == 3)
                        {
                            dayDTO.Break3Id = 0;
                            dayDTO.Break3Link = breakGuid;
                            dayDTO.Break3Minutes = (timeCode != null) ? timeCode.DefaultMinutes : 0;
                            dayDTO.Break3StartTime = CalendarUtility.MergeDateAndTime(day.Date, inputBreak.TimeSlot != null ? inputBreak.TimeSlot.From : CalendarUtility.DATETIME_DEFAULT, ignoreSqlServerDateTime: true);
                            dayDTO.Break3TimeCodeId = (timeCode != null) ? timeCode.TimeCodeId : 0;
                        }
                        if (breakNr == 4)
                        {
                            dayDTO.Break4Id = 0;
                            dayDTO.Break4Link = breakGuid;
                            dayDTO.Break4Minutes = (timeCode != null) ? timeCode.DefaultMinutes : 0;
                            dayDTO.Break4StartTime = CalendarUtility.MergeDateAndTime(day.Date, inputBreak.TimeSlot != null ? inputBreak.TimeSlot.From : CalendarUtility.DATETIME_DEFAULT, ignoreSqlServerDateTime: true);
                            dayDTO.Break4TimeCodeId = (timeCode != null) ? timeCode.TimeCodeId : 0;
                        }
                    }
                    days.Add(dayDTO);
                }
                dayNumber++;
            }
            return MergeTimeSchedulePlanningDayDTOs(days);
        }

        private List<TimeSchedulePlanningDayDTO> MergeTimeSchedulePlanningDayDTOs(List<TimeSchedulePlanningDayDTO> timeSchedulePlanningDayDTOs)
        {
            TimeSchedulePlanningDayDTO previous = null;

            foreach (var item in timeSchedulePlanningDayDTOs.ToList().OrderBy(o => o.StartTime))
            {
                if (item.Tasks == null)
                    item.Tasks = new List<TimeScheduleTemplateBlockTaskDTO>();

                if (item.TimeScheduleTaskId != null)
                    item.Tasks.Add(new TimeScheduleTemplateBlockTaskDTO() { TimeScheduleTaskId = item.TimeScheduleTaskId.Value, StartTime = item.StartTime, StopTime = item.StopTime });

                if (item.IncomingDeliveryRowId != null)
                    item.Tasks.Add(new TimeScheduleTemplateBlockTaskDTO() { IncomingDeliveryRowId = item.IncomingDeliveryRowId.Value, StartTime = item.StartTime, StopTime = item.StopTime });

                if (previous == null || item.ShiftTypeId != previous.ShiftTypeId || previous.StopTime != item.StartTime || previous.DayNumber != item.DayNumber)
                {
                    previous = item;
                    continue;
                }
                else
                {
                    previous.StopTime = item.StopTime;

                    if (previous.TaskKey == item.TaskKey && previous.Tasks.FirstOrDefault(f => f.TaskKey == item.TaskKey && f.StopTime == item.StartTime) != null)
                        previous.Tasks.FirstOrDefault(f => f.TaskKey == item.TaskKey && f.StopTime == item.StartTime).StopTime = item.StopTime;
                    else
                        previous.Tasks.Add(new TimeScheduleTemplateBlockTaskDTO() { TimeScheduleTaskId = item.TimeScheduleTaskId, IncomingDeliveryRowId = item.IncomingDeliveryRowId, StartTime = item.StartTime, StopTime = item.StopTime });

                    item.IsDeleted = true;
                    continue;
                }
            }

            timeSchedulePlanningDayDTOs = timeSchedulePlanningDayDTOs.Where(t => !t.IsDeleted).ToList();

            foreach (var item in timeSchedulePlanningDayDTOs)
                item.Tasks = item.Tasks.GroupBy(x => $"{x.StartTime}#{x.TaskKey}").Select(g => g.First()).ToList();

            foreach (var item in timeSchedulePlanningDayDTOs)
            {
                int lenght = Convert.ToInt32((item.StopTime - item.StartTime).TotalMinutes);
                int taskLenght = item.Tasks.Select(s => Convert.ToInt32((s.StopTime - s.StartTime).TotalMinutes)).Sum();

                if (lenght != taskLenght)
                    LogCollector.LogCollector.LogError($"lenght({lenght}) != taskLenght(taskLenght) employeePostId{item.EmployeePostId} date{item.ActualDate} starttime {item.StartTime}");
            }

            return timeSchedulePlanningDayDTOs;
        }

        public EmployeePostWeekDayOfWeekOrder GetWeekDayOrderBasedOnDatesInWeek(EmployeePostDTO employeePost, List<CalculationPeriodItem> needPeriodItems, DateTime weekStartDate, bool forceNotWeekEndWeek, bool ignoreFeeDays, bool isWeekendWeek)
        {
            List<DayOfWeek> dayOfWeekOrder = new List<DayOfWeek>();
            var remainingOnWeek = GetRemainingTimeOnDatesInWeek(employeePost.EmployeePostId, needPeriodItems, weekStartDate);
            Dictionary<DayOfWeek, int> adjustedRemainingOnWeek = new Dictionary<DayOfWeek, int>();
            int count = 1;
            foreach (var day in remainingOnWeek.OrderByDescending(o => o.Value))
            {
                bool isAnyNeedChanged;
                var remainingForEmployee = count > 2 ? GetRemainingPeriodItems(needPeriodItems, GetUsedPeriodItems(weekStartDate, CalendarUtility.GetDateFromDayOfWeek(weekStartDate, day.Key), employeePost.EmployeePostId, 1, "GetWeekDayOrderBasedOnDatesInWeek"), out isAnyNeedChanged) : new List<CalculationPeriodItem>();
                if (count > 2 && day.Value < this.CalculationOptions.OptimalLength + 60 && day.Value > this.CalculationOptions.MinLength + 60 && this.CurrentEmployeePostCyclesRun.PrioShiftTypesIds.ContainsAny(remainingForEmployee.Select(s => s.ShiftTypeId.Value)))
                    adjustedRemainingOnWeek.Add(day.Key, day.Value * 10); // making sure a day with little time but with prio gets earlier in week.
                else
                    adjustedRemainingOnWeek.Add(day.Key, day.Value);
                count++;
            }

            foreach (var day in adjustedRemainingOnWeek.OrderByDescending(o => o.Value))
            {
                dayOfWeekOrder.Add(day.Key);
            }

            if (isWeekendWeek)
            {
                dayOfWeekOrder = dayOfWeekOrder.Where(w => w != DayOfWeek.Sunday && w != DayOfWeek.Saturday).ToList();
                dayOfWeekOrder.Insert(0, DayOfWeek.Saturday);
                dayOfWeekOrder.Insert(0, DayOfWeek.Sunday);
            }
            else if (forceNotWeekEndWeek)
            {
                dayOfWeekOrder = dayOfWeekOrder.Where(w => w != DayOfWeek.Sunday && w != DayOfWeek.Saturday).ToList();
                dayOfWeekOrder.Add(DayOfWeek.Saturday);
                dayOfWeekOrder.Add(DayOfWeek.Sunday);
            }

            if (employeePost.DayOfWeeks.Any() && !ignoreFeeDays)
            {

                dayOfWeekOrder = dayOfWeekOrder.Where(w => !employeePost.FreeDays.Contains(w)).ToList();
                dayOfWeekOrder.AddRange(employeePost.FreeDays);
            }

            return new EmployeePostWeekDayOfWeekOrder(dayOfWeekOrder);
        }

        public List<List<CalculationPeriodItem>> GetPeriodItemsMatchingWeekTime(List<CalculationPeriodItem> periodItems, int weekWorkTime, DateTime weekStart, EmployeePostDTO employeePost, List<DayOfWeek> orderOfWeekDays, int numberOfWeeks, bool ignoreFreeDays)
        {
            List<List<CalculationPeriodItem>> listOfWeeks = new List<List<CalculationPeriodItem>>();
            List<Tuple<DateTime, int, Guid>> tuples = new List<Tuple<DateTime, int, Guid>>();
            SetCalculationOptions(employeePost, this.CalculationOptions.MaxNumberOfWeeks, this.CalculationOptions.OpeningHours);
            List<CalculationPeriodItem> employeePostNeedPeriodItems = FilterValidPeriodItems(SplitShifts(SetScheduleDate(ClonePeriodItems(periodItems), weekStart)), employeePost);
            List<Tuple<DateTime, List<CalculationPeriodItem>>> remainingPeriodItemsOnDatesTuple = GetRemainingPeriodItemForWeeks(employeePostNeedPeriodItems, weekStart, employeePost.EmployeePostId, numberOfWeeks);
            List<CalculationPeriodItem> remainingPeriodItemsOnDates = new List<CalculationPeriodItem>();
            List<PeriodItemsGroupHead> groupHeads = new List<PeriodItemsGroupHead>();

            foreach (var periodItemsOnDate in remainingPeriodItemsOnDatesTuple)
            {
                remainingPeriodItemsOnDates.AddRange(periodItemsOnDate.Item2);

                if (employeePost.DayOfWeekValid(periodItemsOnDate.Item1.DayOfWeek, ignoreFreeDays))
                {
                    foreach (var itemGroups in MergePeriodItemsIfPossible(periodItemsOnDate.Item2).GroupBy(i => i.Item1))
                    {
                        List<CalculationPeriodItem> items = new List<CalculationPeriodItem>();

                        Guid guid = Guid.NewGuid();

                        foreach (var itemGroup in itemGroups)
                        {
                            items.Add(itemGroup.Item2);
                            foreach (var item in items)
                                item.TempGuid = guid;
                        }

                        var heads = GroupStaffingNeedsCalcutionPeriodItems(items);
                        heads = heads.Where(i => !i.HasHoles).ToList();

                        foreach (var head in heads)
                        {
                            Guid newGuid = Guid.NewGuid();
                            foreach (var item in head.CalculationPeriodItems)
                                item.CalculationGuid = newGuid;

                            head.Date = periodItemsOnDate.Item1;
                        }

                        foreach (var head in heads.Where(h => h.HasNetTime && h.Length >= this.CalculationOptions.MinLength))
                        {
                            head.TempGuid = guid;
                            groupHeads.Add(head);
                            tuples.Add(Tuple.Create(items.FirstOrDefault().ScheduleDate, head.TempNetLength, guid));
                        }

                        foreach (var head in heads.Where(h => !h.HasNetTime && h.Length >= this.CalculationOptions.MinLength))
                        {
                            var breakMinutes = GetBreakMinutes(ClonePeriodItems(head.CalculationPeriodItems));
                            head.TempGuid = guid;
                            head.TempBreakMinutes = breakMinutes;
                            groupHeads.Add(head);
                            tuples.Add(Tuple.Create(items.FirstOrDefault().ScheduleDate, head.TempNetLength, guid));
                        }
                    }
                }
            }

            List<Tuple<DateTime, int, Guid>> filteredToOneLengthPerDay = new List<Tuple<DateTime, int, Guid>>();
            var groups = tuples.GroupBy(i => i.Item1.ToString() + "#" + i.Item2.ToString());
            foreach (var t in groups)
                filteredToOneLengthPerDay.Add(t.FirstOrDefault());

            List<List<Tuple<DateTime, int, Guid>>> matchingMinutes = SolveKnapSack(weekWorkTime, filteredToOneLengthPerDay.OrderBy(d => orderOfWeekDays.IndexOf(d.Item1.DayOfWeek)).ToList());

            if (matchingMinutes.Count == 0 && !ignoreFreeDays)
            {
                return GetPeriodItemsMatchingWeekTime(periodItems, weekWorkTime, weekStart, employeePost, orderOfWeekDays, numberOfWeeks, ignoreFreeDays: true);
            }

            foreach (var matchingSack in matchingMinutes)
            {
                List<CalculationPeriodItem> matchingSackPeriodItems = new List<CalculationPeriodItem>();
                foreach (var sack in matchingSack)
                {
                    foreach (var group in groupHeads.Where(d => d.Date.HasValue && d.Date.Value == sack.Item1))
                    {
                        if (group.TempNetLength > 0 && group.TempNetLength == sack.Item2)
                            matchingSackPeriodItems.AddRange(group.CalculationPeriodItems);
                    }
                }
                listOfWeeks.Add(matchingSackPeriodItems);
            }

            return listOfWeeks;
        }


        private List<List<CalculationPeriodItem>> PrioPossibleWeeks(List<List<CalculationPeriodItem>> possibleWeeks)
        {
            List<List<CalculationPeriodItem>> prioPossibleWeeks = new List<List<CalculationPeriodItem>>();

            if (possibleWeeks.Count > this.MaxNumberOfPossibleWeeks)
                possibleWeeks = possibleWeeks.Take(this.MaxNumberOfPossibleWeeks).ToList();

            foreach (var possibleWeek in possibleWeeks)
            {
                List<CalculationPeriodItem> sunDays = possibleWeek.Where(w => w.Weekday == DayOfWeek.Sunday).ToList();
                List<CalculationPeriodItem> satDays = possibleWeek.Where(w => w.Weekday == DayOfWeek.Saturday).ToList();

                if ((sunDays.Any() && satDays.Any()) && sunDays[0].Length != satDays[0].Length)
                    prioPossibleWeeks.Add(possibleWeek);
            }

            if (prioPossibleWeeks.Count == 0)
            {
                foreach (var possibleWeek in possibleWeeks)
                {
                    List<CalculationPeriodItem> sunDays = possibleWeek.Where(w => w.Weekday == DayOfWeek.Sunday).ToList();
                    List<CalculationPeriodItem> satDays = possibleWeek.Where(w => w.Weekday == DayOfWeek.Saturday).ToList();

                    if ((sunDays.Any() || satDays.Any()))
                        prioPossibleWeeks.Add(possibleWeek);
                }
            }

            foreach (var possible in possibleWeeks)
            {
                if (!prioPossibleWeeks.Contains(possible))
                    prioPossibleWeeks.Add(possible);
            }


            return prioPossibleWeeks;

        }
        private List<Tuple<Guid, CalculationPeriodItem>> MergePeriodItemsIfPossible(List<CalculationPeriodItem> periodItems)
        {
            return GetConnectedShifts(periodItems, allowAdjustment: true, checkValidLength: true, ignoreGuid: true, ignoreAllowOverlapping: true);
        }


        public List<List<Tuple<DateTime, int, Guid>>> SolveKnapSack(int goal, List<Tuple<DateTime, int, Guid>> minutes)
        {
            recursiveCount = 0;
            List<List<Tuple<DateTime, int, Guid>>> matchingSacks = new List<List<Tuple<DateTime, int, Guid>>>();
            RecursiveSolve(matchingSacks, goal, 0, new List<Tuple<DateTime, int, Guid>>(), new List<Tuple<DateTime, int, Guid>>(minutes), 0);
            return matchingSacks;
        }

        private int recursiveMax
        {
            get
            {
                return 100000;
            }
        }

        private int recursiveCount { get; set; }
        private void RecursiveSolve(List<List<Tuple<DateTime, int, Guid>>> matchingSacks, int goal, int currentSum, List<Tuple<DateTime, int, Guid>> included, List<Tuple<DateTime, int, Guid>> notIncluded, int startIndex)
        {
            if (matchingSacks.Count == 0 && recursiveCount > (recursiveMax / 100))
                return;

            if (recursiveCount > recursiveMax)
                return;

            for (int index = startIndex; index < notIncluded.Count; index++)
            {
                var tuple = notIncluded[index];
                int nextValue = tuple.Item2;
                recursiveCount++;

                var ondate = included.Where(i => i.Item1 == tuple.Item1);
                if (ondate.Any())
                {
                    if (ondate.Sum(s => s.Item2) + tuple.Item2 > this.CalculationOptions.MaxLength)
                        continue;
                }

                if (currentSum + nextValue == goal)
                {
                    List<Tuple<DateTime, int, Guid>> newResult = new List<Tuple<DateTime, int, Guid>>(included);
                    newResult.Add(tuple);

                    if (!AlreadyAdded(newResult, matchingSacks))
                        matchingSacks.Add(newResult);
                }
                else if (currentSum + nextValue < goal)
                {
                    List<Tuple<DateTime, int, Guid>> nextIncluded = new List<Tuple<DateTime, int, Guid>>(included);
                    nextIncluded.Add(tuple);
                    List<Tuple<DateTime, int, Guid>> nextNotIncluded = new List<Tuple<DateTime, int, Guid>>(notIncluded);
                    nextNotIncluded.Remove(tuple);

                    if (matchingSacks.Count < this.MaxNumberOfPossibleWeeks)
                        RecursiveSolve(matchingSacks, goal, currentSum + nextValue, nextIncluded, nextNotIncluded, startIndex++);
                }
            }
        }

        private bool AlreadyAdded(List<Tuple<DateTime, int, Guid>> result, List<List<Tuple<DateTime, int, Guid>>> matchingSacks)
        {
            foreach (var sack in matchingSacks)
            {
                int count = 0;

                foreach (var res in result)
                {
                    if (sack.Where(i => i.Item1 == res.Item1 && i.Item2 == res.Item2).Any())
                        count++;
                }

                if (count == sack.Count)
                    return true;
            }

            return false;
        }

        private bool IsLengthSame(PeriodItemsGroupHead grouphead1, PeriodItemsGroupHead grouphead2)
        {
            int isNetlength1 = 0;
            foreach (var item in grouphead1.CalculationPeriodItems.Where(i => i.IsNetTime))
                isNetlength1 += item.Length;

            int isNetlength2 = 0;
            foreach (var item in grouphead2.CalculationPeriodItems.Where(i => i.IsNetTime))
                isNetlength2 += item.Length;


            if (isNetlength1 == isNetlength2)
            {
                int length1 = 0;
                foreach (var item in grouphead1.CalculationPeriodItems.Where(i => i.IsNetTime))
                    length1 += item.Length;

                int length2 = 0;
                foreach (var item in grouphead2.CalculationPeriodItems.Where(i => i.IsNetTime))
                    length2 += item.Length;

                return length1 == length2;
            }

            return false;
        }

        #endregion

        #region EmployeePostDTO

        private EmployeePostCycle HandlePeriodItemsForEmployeePost(List<CalculationPeriodItem> needPeriodItems, List<List<CalculationPeriodItem>> allPossibleWeeks, List<ShiftTypeDTO> shiftTypeDTOs, DateTime startDate, int days, bool allowSplit, List<EmployeePostWeekDayOfWeekOrder> preSetDayOfWeekOrders)
        {
            EmployeePostDTO employeePost = this.CalculationOptions.EmployeePost;
            this.CalculationOptions.AddOnlyDockedPeriodItems = true;
            EmployeePostCycle employeePostCycle = new EmployeePostCycle(employeePost, startDate, this.CalculationOptions, this.AllScheduleCycleRuleDTOs);
            this.CurrentEmployeePostCyclesRun.ClearOtherPreviousPeriodItem();
            List<CalculationPeriodItem> employeePostNeedPeriodItems = ClonePeriodItems(needPeriodItems);
            employeePostCycle.SetFreeDays(employeePostCycle.IgnoreFreeDays, 0, false);
            employeePostNeedPeriodItems = FilterValidPeriodItems(employeePostNeedPeriodItems, employeePost);
            employeePostCycle.AddPreAnalysisInformation(employeePost, (ClonePeriodItems(employeePostNeedPeriodItems)));
            employeePostCycle.InitialValidItems = ClonePeriodItems(employeePostNeedPeriodItems);
            int workDaysWeek = employeePost.WorkDaysWeek;
            int remainingMinutesFromPreviousWeek = 0;
            this.CalculationOptions.HasUniques = this.CurrentEmployeePostCyclesRun != null && this.CurrentEmployeePostCyclesRun.UniqueShiftypeIds != null ? this.CurrentEmployeePostCyclesRun.UniqueShiftypeIds.Any() : false;

            if (employeePostNeedPeriodItems.Count == 0)
            {
                employeePostCycle.NoValidItems = true;
                return employeePostCycle;
            }

            #region Create possible days

            for (int i = 0; i < employeePostCycle.EmployeePostWeeks.Count; i++)
            {
                List<EmployeePostWeekDayOfWeekOrder> dayOfWeekOrders = preSetDayOfWeekOrders.ToList();
                employeePostCycle.RemovePreSetWeekDayOrders();
                dayOfWeekOrders.Insert(0, employeePostCycle.GetWeekDayOrder(employeePostCycle.EmployeePostWeeks[i].WeekNumber, employeePostCycle.IgnoreFreeDays));
                bool forceNotWeekEndWeek = employeePostCycle.ForceNotWeekEndWeek(employeePostCycle.EmployeePostWeeks[i].WeekNumber);
                bool hasNextWeekendPrioShiftTypes = employeePostCycle.EmployeePostWeeks.Count > 1 && employeePostCycle.EmployeePostWeeks.Count < i + 1 &&
                                           this.CurrentEmployeePostCyclesRun.HasNextWeekendPrioShiftTypes(GetRemainingPeriodItemForWeeks(employeePostNeedPeriodItems, employeePostCycle.EmployeePostWeeks[i + 1].StartDate, employeePost.EmployeePostId, 1), employeePost.EmployeePostId);

                bool isWeekendWeek = employeePostCycle.IsWeekendWeek(employeePostCycle.EmployeePostWeeks[i].WeekNumber, employeePostCycle.IgnoreFreeDays);

                if (!forceNotWeekEndWeek && hasNextWeekendPrioShiftTypes)
                    forceNotWeekEndWeek = true;

                if (dayOfWeekOrders.Where(c => c.DayOrWeeksOrder.Any()).Count() == 1 || forceNotWeekEndWeek)
                {
                    bool isChronologicalWeek = true;
                    int day = 1;
                    foreach (var dayOfWeek in dayOfWeekOrders.FirstOrDefault().DayOrWeeksOrder)
                    {
                        if (day == 1 && dayOfWeek != DayOfWeek.Monday)
                            isChronologicalWeek = false;

                        if (day == 2 && dayOfWeek != DayOfWeek.Tuesday)
                            isChronologicalWeek = false;

                        if (day == 3 && dayOfWeek != DayOfWeek.Wednesday)
                            isChronologicalWeek = false;

                        if (day == 4 && dayOfWeek != DayOfWeek.Thursday)
                            isChronologicalWeek = false;

                        if (day == 5 && dayOfWeek != DayOfWeek.Friday)
                            isChronologicalWeek = false;

                        if (day == 6 && dayOfWeek != DayOfWeek.Saturday)
                            isChronologicalWeek = false;

                        if (day == 7 && dayOfWeek != DayOfWeek.Sunday)
                            isChronologicalWeek = false;

                        day++;
                    }

                    if (!isChronologicalWeek)
                    {
                        List<DayOfWeek> dayOfWeeks = new List<DayOfWeek>();

                        dayOfWeeks.Add(DayOfWeek.Monday);
                        dayOfWeeks.Add(DayOfWeek.Tuesday);
                        dayOfWeeks.Add(DayOfWeek.Wednesday);
                        dayOfWeeks.Add(DayOfWeek.Thursday);
                        dayOfWeeks.Add(DayOfWeek.Friday);
                        dayOfWeeks.Add(DayOfWeek.Saturday);
                        dayOfWeeks.Add(DayOfWeek.Sunday);

                        var weekEndWeek = dayOfWeekOrders.FirstOrDefault(f => f.DayOrWeeksOrder.First() == DayOfWeek.Saturday || f.DayOrWeeksOrder.First() == DayOfWeek.Sunday);

                        if (forceNotWeekEndWeek)
                            dayOfWeekOrders = new List<EmployeePostWeekDayOfWeekOrder>();

                        dayOfWeekOrders.Add(new EmployeePostWeekDayOfWeekOrder(dayOfWeeks));

                        if (forceNotWeekEndWeek)
                            dayOfWeekOrders.Add(weekEndWeek);
                    }
                }

                if (forceNotWeekEndWeek)
                    isWeekendWeek = false;

                EmployeePostWeekDayOfWeekOrder basedOnRemaining = GetWeekDayOrderBasedOnDatesInWeek(employeePost, employeePostNeedPeriodItems, employeePostCycle.EmployeePostWeeks[i].StartDate, forceNotWeekEndWeek, employeePostCycle.IgnoreFreeDays, isWeekendWeek);
                dayOfWeekOrders.Insert(0, basedOnRemaining);


                bool adjusted = false;
                var remainingOnWeek = GetRemainingPeriodItemForWeeks(SetScheduleDate(ClonePeriodItems(employeePostNeedPeriodItems), employeePostCycle.EmployeePostWeeks[i].StartDate), employeePostCycle.EmployeePostWeeks[i].StartDate, employeePost.EmployeePostId, 1);
                List<DayOfWeek> basedOnRemainingSkillsDayOfWeekOrder = this.CurrentEmployeePostCyclesRun.GetWeekDayOrderBasedOnSkillsOfRemainingEmployeePosts(remainingOnWeek, employeePost.EmployeePostId, dayOfWeekOrders[0].DayOrWeeksOrder, forceNotWeekEndWeek, isWeekendWeek, ref adjusted);
                if (adjusted)
                    dayOfWeekOrders = new List<EmployeePostWeekDayOfWeekOrder>() { new EmployeePostWeekDayOfWeekOrder(basedOnRemainingSkillsDayOfWeekOrder, EmployeePostWeekDayOfWeekOrderType.BasedOnRemainingSkills) };
                else
                    dayOfWeekOrders.Insert(0, new EmployeePostWeekDayOfWeekOrder(basedOnRemainingSkillsDayOfWeekOrder, EmployeePostWeekDayOfWeekOrderType.BasedOnRemainingSkills));

                if (i > 0)
                {
                    List<DayOfWeek> basedOnPreviousWeek = this.CurrentEmployeePostCyclesRun.GetWeekDayOrderBasedOnPreviousWeek(employeePostCycle, i + 1, dayOfWeekOrders[0].DayOrWeeksOrder, isWeekendWeek, ref adjusted);
                    if (adjusted)
                        dayOfWeekOrders = new List<EmployeePostWeekDayOfWeekOrder>() { new EmployeePostWeekDayOfWeekOrder(basedOnPreviousWeek, EmployeePostWeekDayOfWeekOrderType.BasedOnPreviousWeek) };
                    else
                        dayOfWeekOrders.Insert(0, new EmployeePostWeekDayOfWeekOrder(basedOnPreviousWeek, EmployeePostWeekDayOfWeekOrderType.BasedOnPreviousWeek));
                }

                employeePostCycle.SetPreSetWeekDayOrders(dayOfWeekOrders.ToList());
                EmployeePostWeek copy = employeePostCycle.EmployeePostWeeks[i].Copy(0, employeePostCycle.EmployeePostWeeks[i].WeekNumber);
                copy.RemainingMinutesWeekFromPreviousWeek = remainingMinutesFromPreviousWeek;
                copy.EmployeePost.WorkDaysWeek = workDaysWeek;
                EmployeePostWeek employeePostWeek = new EmployeePostWeek();
                EmployeePostWeekDayOfWeekOrder orderOfWeekDays = employeePostCycle.GetWeekDayOrder(employeePostCycle.EmployeePostWeeks[i].WeekNumber, employeePostCycle.IgnoreFreeDays);
                List<List<CalculationPeriodItem>> possibleWeeks = allPossibleWeeks.Where(w => w.Where(s => CalendarUtility.GetDatesInWeek(copy.StartDate).Contains(s.ScheduleDate)).Any()).ToList();

                if (employeePostNeedPeriodItems.Where(w => w.IsNetTime).Count() > employeePostNeedPeriodItems.Count() * 0.8 && !this.CurrentEmployeePostCyclesRun.PrioShiftTypesIds.ContainsAny(employeePostNeedPeriodItems.Select(s => s.ShiftTypeId.Value)))
                    possibleWeeks = GetPeriodItemsMatchingWeekTime(SetScheduleDate(ClonePeriodItems(employeePostNeedPeriodItems), copy.StartDate), copy.RemainingMinutesWeekInclPreviousWeek, copy.StartDate, employeePost, dayOfWeekOrders[0].DayOrWeeksOrder, employeePostCycle.EmployeePostWeeks.Count, ignoreFreeDays: false);

                int index = 0;
                int weekAttempts = 0;
                remainingOnWeek = GetRemainingPeriodItemForWeeks(SetScheduleDate(ClonePeriodItems(employeePostNeedPeriodItems), employeePostCycle.EmployeePostWeeks[i].StartDate), employeePostCycle.EmployeePostWeeks[i].StartDate, employeePost.EmployeePostId, 1);
                List<CalculationPeriodItem> items = new List<CalculationPeriodItem>();
                remainingOnWeek.ForEach(f => items.AddRange(f.Item2));
                employeePostCycle.AddDaysPreAnalysisInformation(items);

                foreach (var order in employeePostCycle.PreSetDayOfWeekOrders.Where(c => c.DayOrWeeksOrder.Any()))
                {
                    if (employeePostWeek.Match)
                        continue;
                    employeePostCycle.SetPreSetWeekDayOrder(index);
                    index++;

                    #region Try With possible weeks first

                    if (possibleWeeks.Any() && i == 0) //since we only create for first week right now
                    {
                        List<List<CalculationPeriodItem>> prioPossibleWeeks = PrioPossibleWeeks(possibleWeeks);

                        if (this.CurrentEmployeePostCyclesRun.UniqueShiftypeIds.Any() && DateTime.UtcNow.Hour == 2)
                        {
                            employeePostWeek = HandleEmployeePostWeekPossibleWeeks(i, prioPossibleWeeks, employeePostCycle, copy, employeePost, shiftTypeDTOs, allowSplit);
                            weekAttempts++;

                            if (!employeePostWeek.Match && employeePost.DayOfWeekIds.Any() && CurrentEmployeePostCyclesRun.UniqueShiftypeIds.Any())
                            {
                                employeePostCycle.IgnoreFreeDays = true;
                                employeePostWeek = HandleEmployeePostWeekPossibleWeeks(i, prioPossibleWeeks, employeePostCycle, copy, employeePost, shiftTypeDTOs, allowSplit);
                                weekAttempts++;
                            }
                        }

                        employeePostCycle.IgnoreFreeDays = false;
                    }

                    #endregion

                    //Ignore on the first week
                    if (!employeePostWeek.Match && i > 0)
                    {
                        employeePostCycle.TryOnlyShiftsFromPreviousWeeks = true;
                        employeePostCycle.IgnoreFreeDays = false;
                        employeePostWeek = HandleEmployeePostWeek(i, employeePostNeedPeriodItems, employeePostCycle, copy, employeePost, shiftTypeDTOs, allowSplit);
                        employeePostCycle.TryOnlyShiftsFromPreviousWeeks = false;
                        weekAttempts++;
                    }

                    if (!employeePostWeek.Match && employeePostCycle.FreeDays.Any())
                    {
                        employeePostCycle.FocusOnShiftsFromPreviousWeeks = true;
                        employeePostCycle.IgnoreFreeDays = false;
                        employeePostWeek = HandleEmployeePostWeek(i, employeePostNeedPeriodItems, employeePostCycle, copy, employeePost, shiftTypeDTOs, allowSplit);
                        employeePostCycle.FocusOnShiftsFromPreviousWeeks = false;
                        weekAttempts++;
                    }

                    if (!employeePostWeek.Match)
                    {
                        employeePostCycle.FocusOnShiftsFromPreviousWeeks = true;
                        employeePostCycle.IgnoreFreeDays = true;
                        employeePostWeek = HandleEmployeePostWeek(i, employeePostNeedPeriodItems, employeePostCycle, copy, employeePost, shiftTypeDTOs, allowSplit);
                        employeePostCycle.FocusOnShiftsFromPreviousWeeks = false;
                        weekAttempts++;
                    }

                    var diffMinutesInLastAttempt = employeePostWeek.DiffMinutesInLastAttempt;
                    if (!employeePostWeek.Match && diffMinutesInLastAttempt != 0)
                    {
                        employeePostWeek = HandleEmployeePostWeek(i, employeePostNeedPeriodItems, employeePostCycle, copy, employeePost, shiftTypeDTOs, allowSplit, diffMinutesInLastAttempt);
                        weekAttempts++;
                    }

                    if (!employeePostWeek.Match && employeePostWeek.DiffMinutesInLastAttempt != diffMinutesInLastAttempt)
                    {
                        employeePostWeek = HandleEmployeePostWeek(i, employeePostNeedPeriodItems, employeePostCycle, copy, employeePost, shiftTypeDTOs, allowSplit, employeePostWeek.DiffMinutesInLastAttempt);
                        weekAttempts++;
                    }

                    if (!employeePostWeek.Match)
                    {
                        employeePostCycle.OnlyForcePreferredLenghtOnLastWeek = false;

                        if (this.CurrentEmployeePostCyclesRun.UniqueShiftypeIds.Any() || employeePostCycle.EmployeePostWeeks.Count == i + 1)
                        {
                            employeePostWeek = HandleEmployeePostWeek(i, employeePostNeedPeriodItems, employeePostCycle, copy, employeePost, shiftTypeDTOs, allowSplit);
                            weekAttempts++;
                        }
                    }

                    if (!employeePostWeek.Match)
                    {
                        employeePostCycle.OnlyForcePreferredLenghtOnLastWeek = true;
                        employeePostCycle.IgnoreFreeDays = true;

                        if (this.CurrentEmployeePostCyclesRun.UniqueShiftypeIds.Any() || employeePostCycle.EmployeePostWeeks.Count == i + 1)
                        {
                            employeePostWeek = HandleEmployeePostWeek(i, employeePostNeedPeriodItems, employeePostCycle, copy, employeePost, shiftTypeDTOs, allowSplit);
                            weekAttempts++;
                        }
                    }

                    if (!employeePostWeek.Match)
                    {
                        employeePostCycle.OnlyForcePreferredLenghtOnLastWeek = false;
                        employeePostCycle.IgnoreFreeDays = true;

                        if (this.CurrentEmployeePostCyclesRun.UniqueShiftypeIds.Any() || employeePostCycle.EmployeePostWeeks.Count == i + 1)
                        {
                            employeePostWeek = HandleEmployeePostWeek(i, employeePostNeedPeriodItems, employeePostCycle, copy, employeePost, shiftTypeDTOs, allowSplit);
                            weekAttempts++;
                        }
                    }

                    if (employeePost.ActorCompanyId == 701609)
                    {
                        if (!employeePostWeek.Match && index == employeePostCycle.PreSetDayOfWeekOrders.Count && this.CalculationOptions.AddOnlyDockedPeriodItems)
                        {
                            this.CalculationOptions.AddOnlyDockedPeriodItems = false;
                            if (this.CurrentEmployeePostCyclesRun.UniqueShiftypeIds.Any() || employeePostCycle.EmployeePostWeeks.Count == i + 1)
                            {
                                employeePostWeek = HandleEmployeePostWeek(i, employeePostNeedPeriodItems, employeePostCycle, copy, employeePost, shiftTypeDTOs, allowSplit);
                                weekAttempts++;
                            }
                            this.CalculationOptions.AddOnlyDockedPeriodItems = true;
                        }

                        if (!employeePostWeek.Match && index == employeePostCycle.PreSetDayOfWeekOrders.Count && this.CalculationOptions.AddOnlyDockedPeriodItems)
                        {
                            this.CalculationOptions.AddOnlyDockedPeriodItems = false;

                            if (this.CurrentEmployeePostCyclesRun.UniqueShiftypeIds.Any() || employeePostCycle.EmployeePostWeeks.Count == i + 1)
                            {
                                employeePostWeek = HandleEmployeePostWeek(i, employeePostNeedPeriodItems, employeePostCycle, copy, employeePost, shiftTypeDTOs, allowSplit, setHalf: true);
                                weekAttempts++;
                            }
                            this.CalculationOptions.AddOnlyDockedPeriodItems = true;
                        }
                    }
                }

                #region Logging

                if (employeePostCycle.AttemptedWeeks.Where(w => w.Length > 0).Any())
                {
                    foreach (var week in employeePostCycle.AttemptedWeeks)
                        week.LogInformation(0, attemptsWeek: true);
                }

                if (employeePostWeek.Match)
                    employeePostWeek.LogInformation(weekAttempts, attemptsWeek: false);

                #endregion

                if (!employeePostWeek.Match && employeePostCycle.AttemptedWeeks.Where(w => w.Length > 0).Any())
                {
                    if (employeePostCycle.AttemptedWeeks.Where(w => w.EmployeePostDays.Where(we => (we.DayOfWeek == DayOfWeek.Sunday || we.DayOfWeek == DayOfWeek.Saturday)).Any() && !w.IsWeekendWeek).Any())
                    {
                        var validNotWeekEndWeeks = employeePostCycle.AttemptedWeeks.Where(w => w.EmployeePostDays.Where(we => (we.DayOfWeek == DayOfWeek.Sunday || we.DayOfWeek == DayOfWeek.Saturday)).Count() == 0 && !w.IsWeekendWeek).ToList();

                        if (validNotWeekEndWeeks.Count > 0)
                        {
                            var bestValidNotWeekendWeek = BestAttemptEmployeePostWeek(validNotWeekEndWeeks, employeePostCycle, i);
                            var bestNotValidNotWeekendWeek = BestAttemptEmployeePostWeek(employeePostCycle.AttemptedWeeks, employeePostCycle, i);

                            if (Math.Abs(bestValidNotWeekendWeek.RemainingMinutesWeekInclPreviousWeek) < employeePost.WorkTimeWeek / 2 && Math.Abs(bestValidNotWeekendWeek.RemainingMinutesWeekInclPreviousWeek) < Math.Abs(bestNotValidNotWeekendWeek.RemainingMinutesWeekInclPreviousWeek) * 5)
                                employeePostCycle.EmployeePostWeeks[i] = bestValidNotWeekendWeek;
                            else
                                employeePostCycle.EmployeePostWeeks[i] = bestNotValidNotWeekendWeek;
                        }
                    }
                    else
                        employeePostCycle.EmployeePostWeeks[i] = BestAttemptEmployeePostWeek(employeePostCycle.AttemptedWeeks, employeePostCycle, i);

                }

                remainingMinutesFromPreviousWeek = employeePostCycle.EmployeePostWeeks[i].RemainingMinutesWeekFromPreviousWeek;
                employeePostCycle.AttemptedWeeks = new List<EmployeePostWeek>();

            }

            #endregion

            return employeePostCycle.IsCycleValid() ? employeePostCycle : new EmployeePostCycle(null, new List<ScheduleCycleRuleDTO>());
        }

        private EmployeePostWeek BestAttemptEmployeePostWeek(List<EmployeePostWeek> attemptedEmployeePostWeeks, EmployeePostCycle employeePostCycle, int index)
        {
            EmployeePostWeek bestEmployeePostWeek = null;

            if (employeePostCycle.EmployeePostWeeks.Count == 1)
                bestEmployeePostWeek = attemptedEmployeePostWeeks.Where(w => w.Length > 0).OrderBy(a => a.RemainingMinutesWeek).ThenBy(t => t.NbrOfHoles).First();
            if (employeePostCycle.EmployeePostWeeks.Count > 1)
            {
                if (employeePostCycle.EmployeePostWeeks.Count != employeePostCycle.EmployeePostWeeks[index].WeekNumber)
                {
                    if (!employeePostCycle.IgnoreFreeDays)
                        bestEmployeePostWeek = attemptedEmployeePostWeeks.Where(w => w.Length > 0).OrderBy(a => a.RemainingMinutesWeek).ThenBy(t => t.NbrOfHoles).First();
                    else
                        bestEmployeePostWeek = attemptedEmployeePostWeeks.Where(w => w.Length > 0).OrderBy(a => a.RemainingMinutesWeek).FirstOrDefault();
                }
                else
                {
                    if (!employeePostCycle.IgnoreFreeDays)
                        bestEmployeePostWeek = attemptedEmployeePostWeeks.Where(w => w.Length > 0).OrderBy(a => a.RemainingMinutesWeekInclPreviousWeek).ThenBy(t => t.NbrOfHoles).First();
                    else
                        bestEmployeePostWeek = attemptedEmployeePostWeeks.Where(w => w.Length > 0).OrderBy(a => a.RemainingMinutesWeekInclPreviousWeek).FirstOrDefault();
                }
            }

            if (bestEmployeePostWeek == null)
                return attemptedEmployeePostWeeks.FirstOrDefault();

            return bestEmployeePostWeek;
        }

        private EmployeePostWeek HandleEmployeePostWeek(int index, List<CalculationPeriodItem> employeePostNeedPeriodItems, EmployeePostCycle employeePostCycle, EmployeePostWeek copy, EmployeePostDTO employeePost, List<ShiftTypeDTO> shiftTypeDTOs, bool allowSplit, int diffMinutesInLastAttempt = 0, bool setHalf = false)
        {
            int maxRestTime;
            DateTime maxRestTimeStarts;

            EmployeePostWeek employeePostWeek = employeePostCycle.EmployeePostWeeks[index] = copy.Copy(0, copy.WeekNumber);
            employeePostWeek.SetHalf = setHalf;
            employeePostWeek.DiffMinutesInLastAttempt = diffMinutesInLastAttempt;
            employeePostCycle.SetRemainingNumberOfWeeks(employeePostWeek.WeekNumber);
            employeePostCycle.SetRemainingFromPreviousWeeks(employeePostWeek.WeekNumber);
            employeePost.IgnoreDaysOfWeekIds = false;
            employeePost.OverWriteDayOfWeekIds = null;
            employeePostWeek = HandlePeriodItemsForEmployeePostWeek(employeePostCycle, employeePostWeek, employeePost, employeePostNeedPeriodItems, shiftTypeDTOs, allowSplit);
            employeePostWeek.LogInformation();
            employeePostCycle.EmployeePostCycleInformation.AddEmployeePostWeekInformation(employeePostWeek.EmployeePostWeekInformation);
            bool limitOnLastWeek = employeePostCycle.EmployeePostWeeks.Count == employeePostWeek.WeekNumber && employeePostCycle.FocusOnShiftsFromPreviousWeeks;

            diffMinutesInLastAttempt = 0;

            if (!employeePostCycle.EvaluateRestTimeWeek(employeePost.EmployeeGroupDTO.RuleRestTimeWeek, employeePostCycle.GetWeekShifts(employeePostWeek.WeekNumber), employeePostWeek.StartDate, employeePostWeek.StartDate.AddDays(7), out maxRestTime, out maxRestTimeStarts))
            {
                employeePostWeek.LogInformation("employeePostCycle.EvaluateRestTimeWeek failed");
                employeePostWeek = new EmployeePostWeek();
            }
            else
            {
                if (employeePostWeek.Length > 0 && employeePostCycle.EmployeePostWeeks.Count == employeePostWeek.WeekNumber && employeePostWeek.Length == employeePost.WorkTimeWeek && employeePostWeek.NbrOfHoles <= 1)
                {
                    employeePostCycle.EmployeePostWeeks[index] = employeePostWeek;
                    employeePostWeek.Match = true;
                    return employeePostWeek;
                }
                else if (employeePostWeek.Length > 0 && employeePostWeek.Length <= employeePostCycle.WorkTimeWeekMax && employeePostWeek.Length >= employeePostCycle.WorkTimeWeekMin && employeePostWeek.NbrOfHoles <= 1 && !limitOnLastWeek)
                {
                    employeePostCycle.EmployeePostWeeks[index] = employeePostWeek;
                    employeePostWeek.Match = true;
                    return employeePostWeek;
                }
                else if (employeePostWeek.Length > 0 && employeePostWeek.Length <= employeePostCycle.WorkTimeWeekMax && employeePostWeek.Length >= employeePostCycle.WorkTimeWeekMin && employeePostWeek.NbrOfHoles <= 1 &&
                      employeePostNeedPeriodItems.Where(o => o.IsUnique(this.CurrentEmployeePostCyclesRun)).Any() &&
                      employeePostNeedPeriodItems.Where(o => o.IsUnique(this.CurrentEmployeePostCyclesRun)).Count() <= employeePostWeek.GetAllMatchedCalculationPeriodItems().Where(o => o.IsUnique(this.CurrentEmployeePostCyclesRun)).Count())
                {
                    employeePostCycle.EmployeePostWeeks[index] = employeePostWeek;
                    employeePostWeek.Match = true;
                    return employeePostWeek;
                }
                else if (!limitOnLastWeek && employeePostWeek.Length == employeePostCycle.RemainingMinutes)
                {
                    employeePostCycle.EmployeePostWeeks[index] = employeePostWeek;
                    employeePostWeek.Match = true;
                    return employeePostWeek;
                }
                else if (employeePostCycle.TryOnlyShiftsFromPreviousWeeks && employeePostWeek.EmployeePostDays.Count + 1 == employeePost.WorkDaysWeek && employeePostCycle.PreviousWeekWasWeekendWeek(employeePostWeek.WeekNumber, employeePostWeek.IsWeekendWeek))
                {
                    employeePostCycle.EmployeePostWeeks[index] = employeePostWeek;
                    employeePostWeek.Match = true;
                    return employeePostWeek;
                }
                else
                {
                    if (employeePostWeek.Length <= employeePostCycle.WorkTimeWeekMin)
                        diffMinutesInLastAttempt = employeePostWeek.Length - employeePostCycle.WorkTimeWeekMin;
                    else if (employeePostWeek.Length >= employeePostCycle.WorkTimeWeekMax)
                        diffMinutesInLastAttempt = employeePostWeek.Length - employeePostCycle.WorkTimeWeekMax;

                    employeePostWeek.EmployeePostWeekInformation.AttemptsWeek = true;
                    employeePostCycle.AttemptedWeeks.Add(employeePostWeek.Copy(0, copy.WeekNumber));
                }
            }

            var week = new EmployeePostWeek();
            week.DiffMinutesInLastAttempt = diffMinutesInLastAttempt;
            return week;
        }

        private EmployeePostWeek HandleEmployeePostWeekPossibleWeeks(int index, List<List<CalculationPeriodItem>> prioPossibleWeeks, EmployeePostCycle employeePostCycle, EmployeePostWeek copy, EmployeePostDTO employeePost, List<ShiftTypeDTO> shiftTypeDTOs, bool allowSplit)
        {
            int maxRestTime;
            DateTime maxRestTimeStarts;

            foreach (var possibleWeekPeriodItems in prioPossibleWeeks)
            {
                var employeePostWeek = employeePostCycle.EmployeePostWeeks[index] = copy.Copy(0, copy.WeekNumber);
                employeePostCycle.SetRemainingFromPreviousWeeks(employeePostWeek.WeekNumber);
                employeePostCycle.SetRemainingNumberOfWeeks(employeePostWeek.WeekNumber);
                employeePost.IgnoreDaysOfWeekIds = false;
                employeePost.OverWriteDayOfWeekIds = null;
                employeePostWeek = HandlePeriodItemsForEmployeePostWeek(employeePostCycle, employeePostWeek, employeePost, possibleWeekPeriodItems.ToList(), shiftTypeDTOs, allowSplit);

                if (employeePostCycle.EvaluateRestTimeWeek(employeePost.EmployeeGroupDTO.RuleRestTimeWeek, employeePostCycle.GetWeekShifts(employeePostWeek.WeekNumber), employeePostWeek.StartDate, employeePostWeek.StartDate.AddDays(7), out maxRestTime, out maxRestTimeStarts))
                {
                    if (employeePostCycle.EmployeePostWeeks.Count == employeePostWeek.WeekNumber && employeePostWeek.Length == employeePost.WorkTimeWeek && employeePostWeek.NbrOfHoles <= 1)
                    {
                        employeePostCycle.EmployeePostWeeks[index] = employeePostWeek;
                        employeePostWeek.Match = true;
                        return employeePostWeek;
                    }
                    else if (employeePostWeek.Length <= employeePostCycle.WorkTimeWeekMax && employeePostWeek.Length >= employeePostCycle.WorkTimeWeekMin && employeePostWeek.NbrOfHoles <= 1 && (employeePostWeek.RemainingDaysWeek - employeePostWeek.RemainingDaysFromPreviousWeek) == 0)
                    {
                        employeePostCycle.EmployeePostWeeks[index] = employeePostWeek;
                        employeePostWeek.Match = true;
                        return employeePostWeek;
                    }
                }
            }

            return new EmployeePostWeek();
        }

        private EmployeePostWeek HandlePeriodItemsForEmployeePostWeek(EmployeePostCycle employeePostCycle, EmployeePostWeek employeePostWeek, EmployeePostDTO employeePost, List<CalculationPeriodItem> employeePostNeedPeriodItems, List<ShiftTypeDTO> shiftTypeDTOs, bool allowSplit)
        {
            bool isWeekendWeek = employeePostCycle.IsWeekendWeek(employeePostWeek.WeekNumber, employeePostCycle.IgnoreFreeDays);
            bool removeWeekEndDayIfOne = true;
            bool forceStopOnWeek = false;

            if (!isWeekendWeek && employeePostCycle.UseFreeDaysOnDayNoneWeekendWeeks)
                employeePostCycle.SetFreeDays(employeePostCycle.IgnoreFreeDays, employeePostWeek.WeekNumber, false);
            else if (isWeekendWeek && employeePostWeek.RemainingDaysFromPreviousWeek <= 0)
                employeePostCycle.SetFreeDays(employeePostCycle.IgnoreFreeDays, employeePostWeek.WeekNumber, false);
            else
            {
                if (isWeekendWeek)
                    employeePostWeek.IsWeekendWeek = isWeekendWeek;

                employeePostWeek.FreeDays = new List<DayOfWeek>();

                if (employeePostWeek.WeekWithDeductedDayOfWork(employeePostCycle))
                    employeePostCycle.SetFreeDays(employeePostCycle.IgnoreFreeDays, employeePostWeek.WeekNumber, true);
            }

            if (!isWeekendWeek && !employeePostWeek.WeekWithDeductedDayOfWork(employeePostCycle) && !employeePostCycle.UseFreeDaysOnDayNoneWeekendWeeks)
                employeePost.IgnoreDaysOfWeekIds = true;

            AddOrDeductDaysOnWeek(employeePostCycle, employeePostWeek, employeePostNeedPeriodItems, employeePostCycle.IgnoreFreeDays, isWeekendWeek);

            foreach (DayOfWeek dayOfWeek in employeePostCycle.GetWeekDayOrder(employeePostWeek.WeekNumber, employeePostCycle.IgnoreFreeDays).DayOrWeeksOrder)
            {
                var order = employeePostCycle.GetWeekDayOrder(employeePostWeek.WeekNumber, employeePostCycle.IgnoreFreeDays);

                if (forceStopOnWeek)
                {
                    employeePostWeek.EmployeePostDays = new List<EmployeePostDay>();
                    continue;
                }

                DateTime currentDate = CalendarUtility.GetDateFromDayOfWeek(employeePostWeek.StartDate, dayOfWeek);
                SetCalculationOptions(dayOfWeek, currentDate, null);
                employeePostWeek.HandledDays.Add(dayOfWeek);
                List<CalculationPeriodItem> needPeriodItemsForDate = ClonePeriodItems(employeePostNeedPeriodItems.Where(d => (d.Weekday == currentDate.DayOfWeek)).ToList());

                if (isWeekendWeek)
                {
                    if (dayOfWeek == DayOfWeek.Saturday || dayOfWeek == DayOfWeek.Sunday) // If only one day is possible, do not remove the other weekend day
                    {
                        if (!this.CurrentEmployeePostCyclesRun.PrioShiftTypesIds.ContainsAny(needPeriodItemsForDate.Select(s => s.ShiftTypeId.Value)))
                        {
                            DayOfWeek otherDayOfWeek = currentDate.DayOfWeek == DayOfWeek.Saturday ? DayOfWeek.Sunday : DayOfWeek.Saturday;
                            var otherNeedPeriodItemsForDate = ClonePeriodItems(employeePostNeedPeriodItems.Where(d => (d.Weekday == otherDayOfWeek)).ToList());

                            if (needPeriodItemsForDate.Count == 0 || otherNeedPeriodItemsForDate.Count == 0)
                                removeWeekEndDayIfOne = false;
                        }

                        if (employeePostWeek.FreeDays.Contains(dayOfWeek) && !employeePostCycle.IgnoreFreeDays)
                            removeWeekEndDayIfOne = false;

                        if (needPeriodItemsForDate.Any() && employeePostWeek.HandledDays.Any(a => a == DayOfWeek.Sunday || a == DayOfWeek.Saturday) || !employeePostWeek.HandledButEmptyDays.Any(a => a == DayOfWeek.Sunday || a == DayOfWeek.Saturday))
                            removeWeekEndDayIfOne = false;
                    }

                    if (removeWeekEndDayIfOne && employeePost.ScheduleCycleDTO.OnlyHasOneWeekEndDayPerWeek(employeePost))
                        removeWeekEndDayIfOne = false;

                    if (employeePostWeek.FreeDays.Contains(dayOfWeek) && !employeePostCycle.IgnoreFreeDays)
                    {
                        employeePostWeek.HandledButEmptyDays.Add(dayOfWeek);
                        employeePostWeek.RemoveFreeDays(dayOfWeek, employeePostCycle, removeWeekEndDayIfOne: removeWeekEndDayIfOne);
                        continue;
                    }
                }
                else if (!employeePostCycle.IgnoreFreeDays && employeePostWeek.FreeDays.Contains(dayOfWeek) && employeePostWeek.WeekWithDeductedDayOfWork(employeePostCycle) && order.Type != EmployeePostWeekDayOfWeekOrderType.BasedOnPreviousWeek)
                {
                    continue;
                }

                if (needPeriodItemsForDate.Any() && (employeePost.DayOfWeekValid(dayOfWeek, employeePostCycle.IgnoreFreeDays)) && employeePostWeek.GetNumberOfPossibleWorkDaysLeftInWeek(dayOfWeek, employeePostCycle) > 0 && !employeePostCycle.WillReachMaxDaysInARow(currentDate))
                {
                    bool isAnyNeedChanged;
                    bool allItemsChecked = false;
                    bool stop = false;
                    EmployeePostDay employeePostDay = null;
                    var limited = new List<CalculationPeriodItem>();
                    List<CalculationPeriodItem> shiftPeriodItems = GetUsedPeriodItems(employeePostCycle.StartDate, currentDate, employeePost.EmployeePostId, employeePostCycle.EmployeePostWeeks.Count, "HandlePeriodItemsForEmployeePostWeek");
                    bool forcePreferredDayLength = employeePostCycle.ForcePreferredDayLength(employeePostWeek.WeekNumber, dayOfWeek);
                    int preferredDayLength = employeePostWeek.PreferredDayLength(dayOfWeek, employeePostCycle);

                    if (employeePostCycle.PreviousWeekWasWeekendWeek(employeePostWeek.WeekNumber, isWeekendWeek) && !forcePreferredDayLength && (dayOfWeek == DayOfWeek.Saturday || dayOfWeek == DayOfWeek.Sunday))
                    {
                        employeePostWeek.HandledButEmptyDays.Add(dayOfWeek);
                        employeePostWeek.RemoveFreeDays(dayOfWeek, employeePostCycle, removeWeekEndDayIfOne: removeWeekEndDayIfOne);
                        continue;
                    }

                    this.CalculationOptions.OptimalLength = preferredDayLength;
                    needPeriodItemsForDate = GetRemainingPeriodItems(needPeriodItemsForDate, shiftPeriodItems, out isAnyNeedChanged);
                    bool skipPreviousWeek = employeePost.ValidShiftTypes.Count <= 3 && this.CurrentEmployeePostCyclesRun.PrioShiftTypesIds.ContainsAny(needPeriodItemsForDate.Select(s => s.ShiftTypeId.Value));
                    var selectedRemainingEmployeePosts = this.CurrentEmployeePostCyclesRun.GetRemainingSelectedEmployeePosts();
                    bool foundShiftsOnPreviousWeek = true;

                    if (!skipPreviousWeek)
                        employeePostDay = CheckPreviousWeeks(employeePostDay, employeePostCycle, employeePostWeek, shiftPeriodItems, needPeriodItemsForDate, isWeekendWeek, dayOfWeek, currentDate, allowSplit, forcePreferredDayLength, employeePost, shiftTypeDTOs, preferredDayLength, out stop, out foundShiftsOnPreviousWeek);

                    if (employeePostCycle.TryOnlyShiftsFromPreviousWeeks && foundShiftsOnPreviousWeek && employeePostDay == null)
                        forceStopOnWeek = true;

                    if (!stop)
                        employeePostDay = TryFindEmployePostDay(employeePostDay, employeePostCycle, employeePostWeek, preferredDayLength, needPeriodItemsForDate, dayOfWeek, currentDate, allowSplit, forcePreferredDayLength, employeePost, shiftTypeDTOs, percentFromPreferedLength: 10, allItemsChecked: allItemsChecked, stopLimited: out stop);
                    employeePostWeek.RemoveFreeDays(dayOfWeek, employeePostCycle, removeWeekEndDayIfOne: removeWeekEndDayIfOne);
                }
            }

            return employeePostWeek;
        }

        private void AddOrDeductDaysOnWeek(EmployeePostCycle employeePostCycle, EmployeePostWeek employeePostWeek, List<CalculationPeriodItem> employeePostNeedPeriodItems, bool IgnoreFreeDays, bool isWeekendWeek)
        {
            decimal factor = 1;

            if (employeePostWeek.WeekWithAddedExtraDayOfWork(employeePostCycle))
            {
                if (employeePostWeek.EmployeePost.EmployeeGroupDTO.MaxScheduleTimeFullTime > 0 && employeePostWeek.EmployeePost.IsFullTime())
                    factor = decimal.Divide(employeePostWeek.EmployeePost.WorkTimePerDay, employeePostWeek.EmployeePost.EmployeeGroupDTO.MaxScheduleTimeFullTime);
                else if (employeePostWeek.EmployeePost.EmployeeGroupDTO.MaxScheduleTimePartTime > 0)
                    factor = decimal.Divide(employeePostWeek.EmployeePost.WorkTimePerDay, employeePostWeek.EmployeePost.EmployeeGroupDTO.MaxScheduleTimePartTime);

                if (factor > 1)
                    factor = 1;

                employeePostWeek.FreeDays = new List<DayOfWeek>();

                if (employeePostCycle.EmployeePostWeeks.Count != employeePostWeek.WeekNumber)
                {
                    if (employeePostWeek.EmployeePost.IsFullTime())
                        employeePostWeek.RemainingExtraMinutesOnWeek = Convert.ToInt32(employeePostWeek.EmployeePost.EmployeeGroupDTO.MaxScheduleTimeFullTime * factor) - employeePostWeek.RemainingMinutesWeekFromPreviousWeek;
                    else
                        employeePostWeek.RemainingExtraMinutesOnWeek = Convert.ToInt32(employeePostWeek.EmployeePost.EmployeeGroupDTO.MaxScheduleTimePartTime * factor) - employeePostWeek.RemainingMinutesWeekFromPreviousWeek;

                    if ((employeePostWeek.RemainingExtraMinutesOnWeek + employeePostWeek.RemainingMinutesWeekFromPreviousWeek) < employeePostWeek.EmployeePost.EmployeeGroupDTO.RuleWorkTimeDayMinimum)
                        employeePostWeek.RemainingExtraMinutesOnWeek = employeePostWeek.EmployeePost.EmployeeGroupDTO.RuleWorkTimeDayMinimum;

                }
            }
            else if (employeePostWeek.WeekWithDeductedDayOfWork(employeePostCycle) && employeePostCycle.PreviousWeekWasWeekendWeek(employeePostWeek.WeekNumber, isWeekendWeek))
            {
                if (employeePostWeek.EmployeePost.EmployeeGroupDTO.MinScheduleTimeFullTime < 0 && employeePostWeek.EmployeePost.IsFullTime())
                    factor = decimal.Divide(employeePostWeek.EmployeePost.WorkTimePerDay, employeePostWeek.EmployeePost.EmployeeGroupDTO.MinScheduleTimeFullTime);
                else if (employeePostWeek.EmployeePost.EmployeeGroupDTO.MinScheduleTimePartTime < 0)
                    factor = decimal.Divide(employeePostWeek.EmployeePost.WorkTimePerDay, employeePostWeek.EmployeePost.EmployeeGroupDTO.MinScheduleTimePartTime);

                if (factor < -1)
                    factor = -1;

                bool adjusted = false;
                List<DayOfWeek> orders = CurrentEmployeePostCyclesRun.GetWeekDayOrderBasedOnSkillsOfRemainingEmployeePosts(GetRemainingPeriodItemForWeeks(SetScheduleDate(ClonePeriodItems(employeePostNeedPeriodItems), employeePostCycle.StartDate), employeePostWeek.StartDate, employeePostWeek.EmployeePost.EmployeePostId, 1), employeePostWeek.EmployeePost.EmployeePostId, employeePostCycle.GetWeekDayOrder(employeePostWeek.WeekNumber, employeePostCycle.IgnoreFreeDays).DayOrWeeksOrder, false, isWeekendWeek, ref adjusted);

                var remainingOnWeek = GetRemainingTimeOnDatesInWeek(employeePostWeek.EmployeePost.EmployeePostId, employeePostNeedPeriodItems, employeePostWeek.StartDate);

                employeePostWeek.AddFreeDaysOnNoWeekendWeek(orders, remainingOnWeek, employeePostWeek, employeePostCycle, CurrentEmployeePostCyclesRun, employeePostNeedPeriodItems);

                if (employeePostCycle.EmployeePostWeeks.Count != employeePostWeek.WeekNumber && employeePostWeek.RemainingDaysFromPreviousWeek <= 0)
                {

                    if (employeePostWeek.EmployeePost.IsFullTime())
                        employeePostWeek.RemainingExtraMinutesOnWeek = Convert.ToInt32(employeePostWeek.EmployeePost.EmployeeGroupDTO.MinScheduleTimeFullTime * -factor) - employeePostWeek.RemainingMinutesWeekFromPreviousWeek;
                    else
                        employeePostWeek.RemainingExtraMinutesOnWeek = Convert.ToInt32(employeePostWeek.EmployeePost.EmployeeGroupDTO.MinScheduleTimePartTime * -factor) - employeePostWeek.RemainingMinutesWeekFromPreviousWeek;

                    if (employeePostWeek.RemainingMinutesWeekFromPreviousWeek < -100)
                        employeePostWeek.RemainingExtraMinutesOnWeek = 0;
                    else if (employeePostWeek.RemainingMinutesWeekInclPreviousWeek < (employeePostWeek.EmployeePost.WorkTimeWeek / 2))
                        employeePostWeek.RemainingExtraMinutesOnWeek = 0;

                }
            }

        }

        private EmployeePostDay CheckPreviousWeeks(EmployeePostDay employeePostDay, EmployeePostCycle employeePostCycle, EmployeePostWeek employeePostWeek, List<CalculationPeriodItem> shiftPeriodItems, List<CalculationPeriodItem> needPeriodItemsForDate, bool isWeekendWeek, DayOfWeek dayOfWeek, DateTime currentDate, bool allowSplit, bool forcePreferredDayLength, EmployeePostDTO employeePost, List<ShiftTypeDTO> shiftTypeDTOs, int preferredDayLength, out bool stopLimited, out bool foundShifts)
        {
            stopLimited = false;
            foundShifts = false;
            List<CalculationPeriodItem> sortedPeriodItemsOnDate = new List<CalculationPeriodItem>();
            if (employeePostCycle.FocusOnShiftsFromPreviousWeeks || employeePostCycle.TryOnlyShiftsFromPreviousWeeks)
                forcePreferredDayLength = false;

            if (employeePostWeek.DiffMinutesInLastAttempt != 0 && (decimal)employeePostWeek.GetNumberOfPossibleWorkDaysLeftInWeek(dayOfWeek, employeePostCycle) <= decimal.Divide((decimal)employeePostWeek.GetNumberOfWorkDaysWeek(), new decimal(2.5)))
                return null;

            if (needPeriodItemsForDate.Count == 0)
                return null;

            if (employeePostWeek.WeekNumber > 2)
            {
                if (employeePostCycle.EmployeePostWeeks.Count == employeePostWeek.WeekNumber &&
                    ((employeePostWeek.RemainingDaysFromPreviousWeek >= 0 && employeePostWeek.WeekWithDeductedDayOfWork(employeePostCycle)) ||
                     (employeePostWeek.RemainingDaysFromPreviousWeek <= 0 && employeePostWeek.WeekWithAddedExtraDayOfWork(employeePostCycle))) &&
                    employeePostCycle.RemainingDays <= 3)
                    return null;

                List<CalculationPeriodItem> periodItemsFromPreviousPreviousWeek = ClonePeriodItems(employeePostCycle.GetPeriodItemsForPreviousOrFutureeWeekDay(employeePostWeek.WeekNumber - 1, dayOfWeek));

                if (periodItemsFromPreviousPreviousWeek.Count == 0 && employeePostWeek.WeekNumber > 2)
                    periodItemsFromPreviousPreviousWeek = ClonePeriodItems(employeePostCycle.GetPeriodItemsForPreviousOrFutureeWeekDay(employeePostWeek.WeekNumber - 2, dayOfWeek));

                if (periodItemsFromPreviousPreviousWeek.Count == 0 && employeePostWeek.WeekNumber > 3)
                    periodItemsFromPreviousPreviousWeek = ClonePeriodItems(employeePostCycle.GetPeriodItemsForPreviousOrFutureeWeekDay(employeePostWeek.WeekNumber - 3, dayOfWeek));

                if (periodItemsFromPreviousPreviousWeek.Any())
                {
                    foundShifts = true;
                    int originalLength = periodItemsFromPreviousPreviousWeek.Where(s => !s.IsBreak).Sum(s => s.Length);
                    int breakLength = periodItemsFromPreviousPreviousWeek.Where(s => s.IsBreak).Sum(s => s.Length);

                    if (forcePreferredDayLength && originalLength != preferredDayLength)
                    {
                        periodItemsFromPreviousPreviousWeek = ClonePeriodItems(employeePostCycle.GetPeriodItemsForPreviousOrFutureeWeekDay(employeePostWeek.WeekNumber - 1, dayOfWeek));
                        originalLength = periodItemsFromPreviousPreviousWeek.Where(s => !s.IsBreak).Sum(s => s.Length);
                        breakLength = periodItemsFromPreviousPreviousWeek.Where(s => s.IsBreak).Sum(s => s.Length);

                        if (forcePreferredDayLength && originalLength != preferredDayLength)
                        {
                            return null;
                        }
                    }

                    int lower = isWeekendWeek ? 8 : 7;
                    int upper = isWeekendWeek ? 12 : 13;

                    if ((employeePostCycle.TryOnlyShiftsFromPreviousWeeks || employeePostCycle.FocusOnShiftsFromPreviousWeeks) && (employeePostWeek.WeekNumber == employeePostCycle.EmployeePostWeeks.Count || employeePostWeek.MinutesPerDayRemainingFromPreviousWeek(employeePostWeek.WeekWithDeductedDayOfWork(employeePostCycle)) <= this.CalculationOptions.MinLength))
                    {
                        upper = 14;
                        lower = 5;
                    }

                    if (originalLength < decimal.Multiply(preferredDayLength, decimal.Divide(lower, 10)) || originalLength > decimal.Multiply(preferredDayLength, decimal.Divide(upper, 10)))
                        return null;

                    decimal lowerLimit = decimal.Divide(lower, 10);//TODO Setting
                    decimal upperLimit = decimal.Divide(upper, 10);//TODO Setting

                    if (employeePostCycle.EmployeePostWeeks.Count - employeePostWeek.WeekNumber >= 2)
                    {
                        lowerLimit = decimal.Divide(lower - 1, 10);  //TODO Setting
                        upperLimit = decimal.Divide(upper + 1, 10); //TODO Setting
                    }

                    if (originalLength < decimal.Multiply(preferredDayLength, lowerLimit) || originalLength > decimal.Multiply(preferredDayLength, upperLimit))
                    {
                        periodItemsFromPreviousPreviousWeek = ClonePeriodItems(employeePostCycle.GetPeriodItemsForPreviousOrFutureeWeekDay(employeePostWeek.WeekNumber - 1, dayOfWeek));
                        originalLength = periodItemsFromPreviousPreviousWeek.Where(s => !s.IsBreak).Sum(s => s.Length);
                        breakLength = periodItemsFromPreviousPreviousWeek.Where(s => s.IsBreak).Sum(s => s.Length);

                        if (originalLength < decimal.Multiply(preferredDayLength, lowerLimit) || originalLength > decimal.Multiply(preferredDayLength, upperLimit))
                            return null;
                    }

                    if (IsPeriodItemsStillValid(needPeriodItemsForDate, periodItemsFromPreviousPreviousWeek.Where(w => !w.IsBreak).ToList(), shiftPeriodItems))
                    {
                        if (periodItemsFromPreviousPreviousWeek.Any())
                        {
                            Guid guid = Guid.NewGuid();
                            sortedPeriodItemsOnDate = ClonePeriodItems(periodItemsFromPreviousPreviousWeek);
                            foreach (CalculationPeriodItem periodItem in sortedPeriodItemsOnDate)
                            {
                                periodItem.Date = currentDate;
                                periodItem.ScheduleDate = currentDate;
                                periodItem.CalculationGuid = guid;
                                periodItem.OriginalCalculationRowGuid = guid;
                                periodItem.Weekday = dayOfWeek;
                                periodItem.PeriodGuid = Guid.NewGuid();
                            }

                            //sortedPeriodItemsOnDate = HandlePeriodItemsForEmployeePost(dayOfWeek, currentDate, employeePostCycle, employeePostWeek, ClonePeriodItems(periodItemsFromPreviousPreviousWeek), allowSplit, forcePreferredDayLength, employeePost: employeePost, shiftTypeDTOs: shiftTypeDTOs);

                            if (sortedPeriodItemsOnDate.Any() && sortedPeriodItemsOnDate.Where(s => !s.IsBreak).Sum(s => s.Length) == originalLength)
                            {
                                bool valid = false;

                                if (preferredDayLength != originalLength)
                                {
                                    var previousWeek = employeePostCycle.EmployeePostWeeks.Where(w => w.WeekNumber == employeePostWeek.WeekNumber - 1).FirstOrDefault();

                                    if (previousWeek != null && previousWeek.Percent > 70)
                                        valid = true;
                                }
                                else
                                    valid = true;

                                var factorOfDiff = 9;

                                if (employeePostCycle.EmployeePostWeeks.Count == employeePostWeek.WeekNumber)
                                {
                                    factorOfDiff = 8;

                                    if (employeePostWeek.GetNumberOfPossibleWorkDaysLeftInWeek(dayOfWeek, employeePostCycle) <= 4)
                                    {
                                        factorOfDiff = 4;

                                        if (employeePostWeek.EmployeePostDays.Where(w => w.DayOfWeek == DayOfWeek.Sunday || w.DayOfWeek == DayOfWeek.Saturday).Any() && (dayOfWeek == DayOfWeek.Sunday || dayOfWeek == DayOfWeek.Saturday))
                                            factorOfDiff = 6;
                                    }

                                    if (employeePostWeek.GetNumberOfPossibleWorkDaysLeftInWeek(dayOfWeek, employeePostCycle) <= 2)
                                        factorOfDiff = 2;
                                }

                                if (employeePostCycle.FocusOnShiftsFromPreviousWeeks)
                                    factorOfDiff = factorOfDiff * 4;

                                else if (employeePostCycle.TryOnlyShiftsFromPreviousWeeks)
                                    factorOfDiff = factorOfDiff * 100;

                                if (valid && (originalLength > (preferredDayLength - decimal.Multiply(CalculationOptions.Interval15, factorOfDiff)) && originalLength < (preferredDayLength + decimal.Multiply(CalculationOptions.Interval15, factorOfDiff))))
                                {
                                    employeePostDay = CreateAndAddEmployeePostDay(employeePostWeek, employeePostCycle, currentDate, sortedPeriodItemsOnDate, forcePreferredDayLength, needPeriodItemsForDate);

                                    if (employeePostDay != null && employeePostDay.Length > 0)
                                    {
                                        stopLimited = true;
                                        employeePostWeek.AddEmployeePostDayInformation(employeePostDay);
                                        return employeePostDay;
                                    }

                                    if (employeePostDay == null || employeePostDay.Length == 0)
                                    {
                                        sortedPeriodItemsOnDate = new List<CalculationPeriodItem>();
                                        employeePostWeek.AddEmployeePostDayInformation(employeePostDay, true);
                                        if (employeePostDay != null)
                                            employeePostWeek.EmployeePostDays.Remove(employeePostDay);
                                    }
                                }
                                else
                                {
                                    sortedPeriodItemsOnDate = new List<CalculationPeriodItem>();
                                }
                            }
                        }

                    }
                }
            }
            else if (employeePostWeek.WeekNumber == 2)
            {
                List<CalculationPeriodItem> periodItemsFromPreviousWeek = ClonePeriodItems(employeePostCycle.GetPeriodItemsForPreviousOrFutureeWeekDay(employeePostWeek.WeekNumber - 1, dayOfWeek));

                if (periodItemsFromPreviousWeek.Any())
                {
                    foundShifts = true;
                    int originalLength = periodItemsFromPreviousWeek.Where(s => !s.IsBreak).Sum(s => s.Length);
                    int breakLength = periodItemsFromPreviousWeek.Where(s => s.IsBreak).Sum(s => s.Length);

                    if (forcePreferredDayLength && originalLength != preferredDayLength)
                        return null;

                    int lower = isWeekendWeek ? 8 : 7;
                    int upper = isWeekendWeek ? 12 : 13;

                    if (employeePostCycle.TryOnlyShiftsFromPreviousWeeks && employeePostWeek.MinutesPerDayRemainingFromPreviousWeek(employeePostWeek.WeekWithDeductedDayOfWork(employeePostCycle)) < 120)
                    {
                        upper = 14;
                        lower = 5;
                    }

                    if (originalLength < decimal.Multiply(preferredDayLength, decimal.Divide(lower, 10)) || originalLength > decimal.Multiply(preferredDayLength, decimal.Divide(upper, 10)))
                        return null;

                    if (IsPeriodItemsStillValid(needPeriodItemsForDate, periodItemsFromPreviousWeek.Where(w => !w.IsBreak).ToList(), shiftPeriodItems))
                    {
                        Guid guid = Guid.NewGuid();
                        sortedPeriodItemsOnDate = ClonePeriodItems(periodItemsFromPreviousWeek);
                        foreach (CalculationPeriodItem periodItem in sortedPeriodItemsOnDate)
                        {
                            periodItem.Date = currentDate;
                            periodItem.ScheduleDate = currentDate;
                            periodItem.CalculationGuid = guid;
                            periodItem.OriginalCalculationRowGuid = guid;
                            periodItem.Weekday = dayOfWeek;
                            periodItem.PeriodGuid = Guid.NewGuid();
                        }

                        if ((sortedPeriodItemsOnDate.Where(s => !s.IsBreak).Sum(s => s.Length) == preferredDayLength && sortedPeriodItemsOnDate.Where(s => !s.IsBreak).Sum(s => s.Length) == originalLength) || !forcePreferredDayLength)
                        {
                            employeePostDay = CreateAndAddEmployeePostDay(employeePostWeek, employeePostCycle, currentDate, sortedPeriodItemsOnDate, forcePreferredDayLength, needPeriodItemsForDate);

                            if (employeePostDay != null && employeePostDay.Length > 0)
                            {
                                stopLimited = true;
                                employeePostWeek.AddEmployeePostDayInformation(employeePostDay);
                                return employeePostDay;
                            }
                            if (employeePostDay == null || employeePostDay.Length == 0)
                            {
                                sortedPeriodItemsOnDate = new List<CalculationPeriodItem>();
                                employeePostWeek.AddEmployeePostDayInformation(employeePostDay);
                                if (employeePostDay != null)
                                    employeePostWeek.EmployeePostDays.Remove(employeePostDay);
                            }
                        }
                        else
                        {
                            sortedPeriodItemsOnDate = new List<CalculationPeriodItem>();
                        }
                    }
                }
            }

            return null;
        }

        private void SetPercentAvailableFutureWeeks(List<PeriodItemsGroupHead> heads, EmployeePostCycle employeePostCycle, EmployeePostWeek employeePostWeek, List<CalculationPeriodItem> employeePostNeedPeriodItems)
        {
            int remainingWeeks = employeePostCycle.EmployeePostWeeks.Count - employeePostWeek.WeekNumber;
            if (remainingWeeks == 0)
                return;

            foreach (var head in heads)
            {
                List<CalculationPeriodItem> shiftOnDayPeriodItems = head.CalculationPeriodItems;
                int currentRemaingWeeks = remainingWeeks;
                DayOfWeek dayOfWeek = shiftOnDayPeriodItems.First().ScheduleDate.DayOfWeek;
                int valid = 0;

                while (currentRemaingWeeks > 0)
                {
                    int currentFutureWeek = employeePostCycle.EmployeePostWeeks.Count - (currentRemaingWeeks - 1);
                    if (currentFutureWeek <= 0)
                        break;

                    var currentDate = employeePostCycle.EmployeePostWeeks.FirstOrDefault(f => f.WeekNumber == currentFutureWeek).StartDate.AddDays((dayOfWeek == DayOfWeek.Sunday ? 6 : (int)dayOfWeek - 1));
                    List<CalculationPeriodItem> shiftPeriodItems = ClonePeriodItems(GetUsedPeriodItems(employeePostCycle.StartDate, currentDate, employeePostCycle.EmployeePost.EmployeePostId, employeePostCycle.EmployeePostWeeks.Count, "SetPercentAvailableFutureWeeks"));
                    List<CalculationPeriodItem> needPeriodItemsForDate = ClonePeriodItems(employeePostNeedPeriodItems.Where(d => (d.Weekday == dayOfWeek)).ToList());
                    bool isAnyNeedChanged;
                    needPeriodItemsForDate = GetRemainingPeriodItems(needPeriodItemsForDate, shiftPeriodItems, out isAnyNeedChanged);
                    if (CheckFutureWeek(shiftOnDayPeriodItems, employeePostCycle, currentFutureWeek, shiftPeriodItems, needPeriodItemsForDate, dayOfWeek, currentDate))
                        valid++;
                    else if (head.Date.HasValue && (head.Date.Value.DayOfWeek == DayOfWeek.Saturday || head.Date.Value.DayOfWeek == DayOfWeek.Sunday) && employeePostCycle.RemainingWeekEnds(currentFutureWeek) < currentRemaingWeeks)
                        valid++;

                    currentRemaingWeeks--;
                }

                head.PercentFutureWeeks = decimal.Multiply(100, decimal.Divide(valid, remainingWeeks));
            }
        }

        private bool CheckFutureWeek(List<CalculationPeriodItem> shiftOnDayPeriodItems, EmployeePostCycle employeePostCycle, int currentFutureWeek, List<CalculationPeriodItem> shiftPeriodItems, List<CalculationPeriodItem> needPeriodItemsForDate, DayOfWeek dayOfWeek, DateTime currentDate)
        {
            EmployeePostWeek nextEmployeePostWeek = employeePostCycle.EmployeePostWeeks.FirstOrDefault(f => f.WeekNumber == currentFutureWeek);

            if (nextEmployeePostWeek == null)
                return false;

            if (!needPeriodItemsForDate.Any())
                return false;

            if (shiftOnDayPeriodItems.Any())
            {
                //int originalLength = shiftOnDayPeriodItems.Where(s => !s.IsBreak).Sum(s => s.Length);
                //int breakLength = shiftOnDayPeriodItems.Where(s => s.IsBreak).Sum(s => s.Length);

                if (IsPeriodItemsStillValid(needPeriodItemsForDate, shiftOnDayPeriodItems.Where(w => !w.IsBreak).ToList(), shiftPeriodItems))
                    return true;
            }

            return false;
        }

        private EmployeePostDay TryFindEmployePostDay(EmployeePostDay employeePostDay, EmployeePostCycle employeePostCycle, EmployeePostWeek employeePostWeek, int preferredDayLength, List<CalculationPeriodItem> needPeriodItemsForDate, DayOfWeek dayOfWeek, DateTime currentDate, bool allowSplit, bool forcePreferredDayLength, EmployeePostDTO employeePost, List<ShiftTypeDTO> shiftTypeDTOs, int percentFromPreferedLength, bool allItemsChecked, out bool stopLimited, bool skipPrioShift = false)
        {
            var limited = new List<CalculationPeriodItem>();
            int missingLength = 0;
            stopLimited = false;
            var allRemainingEmployeePosts = this.CurrentEmployeePostCyclesRun.GetRemainingAllEmployeePosts();
            List<int> percents = new List<int>() { 0, 10, 20, 30, 70, 99 };
            List<int> percentsFromPreferedLength = new List<int> { 10, 10, 15, 50, 80, 80 };
            int index = 0;
            List<Tuple<bool, int, EmployeePostDay>> handledDays = new List<Tuple<bool, int, EmployeePostDay>>();
            bool onlyUnique = needPeriodItemsForDate.Any(item => item.IsUnique(this.CurrentEmployeePostCyclesRun));
            EmployeePostDayValidation employeePostDayValidation = new EmployeePostDayValidation(preferredDayLength, this.CurrentEmployeePostCyclesRun);

            while (index < 6 && !allItemsChecked)
            {
                missingLength = int.MinValue;
                limited = LimitPeriodItemsOnSkills(ClonePeriodItems(needPeriodItemsForDate), allRemainingEmployeePosts, employeePost.EmployeePostId, onlyUnique, percent: percents[index], addShorterShiftsThanMissingLenght: false, splitshifts: false, missingLength: missingLength);
                if (limited.Select(s => s.OriginalCalculationRowGuid).Distinct().Count() == needPeriodItemsForDate.Select(s => s.OriginalCalculationRowGuid).Distinct().Count())
                    allItemsChecked = true;
                employeePostDay = HandleEmployePostDay(employeePostDay, employeePostCycle, employeePostWeek, limited, dayOfWeek, currentDate, allowSplit, forcePreferredDayLength, employeePost, shiftTypeDTOs, percentFromPreferedLength: percentsFromPreferedLength[index], allItemsChecked: allItemsChecked, stopLimited: out stopLimited, missingLength: out missingLength);

                if (limited.Any() && missingLength > 60)
                {
                    limited = LimitPeriodItemsOnSkills(ClonePeriodItems(needPeriodItemsForDate), allRemainingEmployeePosts, employeePost.EmployeePostId, onlyUnique, percent: percents[index], addShorterShiftsThanMissingLenght: true, splitshifts: false, missingLength: missingLength);
                    employeePostDay = HandleEmployePostDay(employeePostDay, employeePostCycle, employeePostWeek, limited, dayOfWeek, currentDate, allowSplit, forcePreferredDayLength, employeePost, shiftTypeDTOs, percentFromPreferedLength: percentsFromPreferedLength[index], allItemsChecked: allItemsChecked, stopLimited: out stopLimited, missingLength: out missingLength);

                    if (employeePostDay != null)
                    {
                        employeePostCycle.TryRemoveEmployeePostDay(employeePostDay);
                        employeePostDayValidation.AddEmployeePostDay(employeePostDay, percents[index]);
                    }
                    else
                    {
                        limited = LimitPeriodItemsOnSkills(ClonePeriodItems(needPeriodItemsForDate), allRemainingEmployeePosts, employeePost.EmployeePostId, onlyUnique, percent: percents[index], addShorterShiftsThanMissingLenght: true, splitshifts: true, missingLength: missingLength);
                        employeePostDay = HandleEmployePostDay(employeePostDay, employeePostCycle, employeePostWeek, limited, dayOfWeek, currentDate, allowSplit, forcePreferredDayLength, employeePost, shiftTypeDTOs, percentFromPreferedLength: percentsFromPreferedLength[index], allItemsChecked: allItemsChecked, stopLimited: out stopLimited, missingLength: out missingLength);

                        if (employeePostDay != null)
                        {
                            employeePostCycle.TryRemoveEmployeePostDay(employeePostDay);
                            employeePostDayValidation.AddEmployeePostDay(employeePostDay, percents[index]);
                        }
                    }
                }
                else if (employeePostDay != null)
                {
                    employeePostCycle.TryRemoveEmployeePostDay(employeePostDay);
                    employeePostDayValidation.AddEmployeePostDay(employeePostDay, percents[index]);
                }

                if (employeePostDay != null && missingLength == 0)
                    break;

                index++;

                if (index == 6 && onlyUnique)
                {
                    index = 0;
                    onlyUnique = false;
                }
            }

            if (allItemsChecked && dayOfWeek == DayOfWeek.Sunday && currentDate.Day == 29 && !onlyUnique && index == 5 && employeePost.EmployeePostId == 320)
            {
                index = index +1 -1;
            }

            if (employeePostDayValidation.EmployeePostDayValidationRows.IsNullOrEmpty()) // The rest
            {
                allItemsChecked = true;
                employeePostDay = HandleEmployePostDay(employeePostDay, employeePostCycle, employeePostWeek, ClonePeriodItems(needPeriodItemsForDate), dayOfWeek, currentDate, allowSplit, forcePreferredDayLength, employeePost, shiftTypeDTOs, percentFromPreferedLength: 80, allItemsChecked: true, stopLimited: out stopLimited, missingLength: out missingLength);

                if (employeePostDay != null)
                {
                    employeePostCycle.TryRemoveEmployeePostDay(employeePostDay);
                    employeePostDayValidation.AddEmployeePostDay(employeePostDay, 100);
                }
            }

            if (!employeePostDayValidation.EmployeePostDayValidationRows.IsNullOrEmpty())
            {
                employeePostDay = employeePostDayValidation.GetBestEmployeePostDay();
                employeePostCycle.TryAddAndValidateEmployeeDay(employeePostDay);
            }
            else
            {
                if ((employeePostDay == null || !employeePostDay.SelectedItemsHead.HasUniques(this.CurrentEmployeePostCyclesRun)) && employeePostCycle.AttemptedWeeks.Count > 2 && !employeePostWeek.PurgedEmployeePostDaysWithUniqueSkills.IsNullOrEmpty())
                {
                    var purgeEmployeePostDays = employeePostWeek.PurgedEmployeePostDaysWithUniqueSkills.OrderByDescending(o => o.Length).ToList();

                    foreach (var purgeEmployeePostDay in purgeEmployeePostDays)
                    {
                        if (employeePostDay == null || (Math.Abs(purgeEmployeePostDay.Length - preferredDayLength) < Math.Abs(employeePostDay.Length - preferredDayLength)))
                        {
                            employeePostDay = purgeEmployeePostDay;
                            if (employeePostCycle.TryAddAndValidateEmployeeDay(employeePostDay))
                                break;
                        }
                    }
                }

                if (employeePostDay == null && employeePostCycle.AttemptedWeeks.Count > 2 && !employeePostWeek.PurgedEmployeePostDaysWithPrioSkills.IsNullOrEmpty())
                {
                    var purgeEmployeePostDays = employeePostWeek.PurgedEmployeePostDaysWithPrioSkills.OrderByDescending(o => o.Length).ToList();

                    foreach (var purgeEmployeePostDay in purgeEmployeePostDays)
                    {
                        if (employeePostDay == null || (Math.Abs(purgeEmployeePostDay.Length - preferredDayLength) < Math.Abs(employeePostDay.Length - preferredDayLength)))
                        {
                            employeePostDay = purgeEmployeePostDay;
                            if (employeePostCycle.TryAddAndValidateEmployeeDay(purgeEmployeePostDay))
                                break;
                        }

                    }
                }

                if (employeePostDay == null && !employeePostWeek.PurgedEmployeePostDaysWithPrioSkills.IsNullOrEmpty())
                {
                    employeePostDay = employeePostWeek.PurgedEmployeePostDaysWithPrioSkills.OrderByDescending(o => o.Length).FirstOrDefault();
                    employeePostCycle.TryAddAndValidateEmployeeDay(employeePostDay);
                }

                if (employeePostDay == null || employeePostDay.Length == 0)
                    employeePostWeek.HandledButEmptyDays.Add(dayOfWeek);
            }

            return employeePostDay;

        }


        private EmployeePostDay HandleEmployePostDay(EmployeePostDay employeePostDay, EmployeePostCycle employeePostCycle, EmployeePostWeek employeePostWeek, List<CalculationPeriodItem> needPeriodItemsForDate, DayOfWeek dayOfWeek, DateTime currentDate, bool allowSplit, bool forcePreferredDayLength, EmployeePostDTO employeePost, List<ShiftTypeDTO> shiftTypeDTOs, int percentFromPreferedLength, bool allItemsChecked, out bool stopLimited, out int missingLength, bool skipPrioShift = false)
        {
            stopLimited = false;
            missingLength = int.MinValue;
            int preferredDayLength = employeePostWeek.PreferredDayLength(dayOfWeek, employeePostCycle);

            if (needPeriodItemsForDate.Any())
            {
                List<CalculationPeriodItem> sortedPeriodItemsOnDate = HandlePeriodItemsForEmployeePost(dayOfWeek, currentDate, employeePostCycle, employeePostWeek, ClonePeriodItems(needPeriodItemsForDate), allowSplit, forcePreferredDayLength, employeePost: employeePost, shiftTypeDTOs: shiftTypeDTOs);

                if (sortedPeriodItemsOnDate.Any())
                {
                    int preferredLength = employeePostWeek.PreferredDayLength(dayOfWeek, employeePostCycle);
                    var hasUniques = sortedPeriodItemsOnDate.Where(w => w.IsUnique(this.CurrentEmployeePostCyclesRun)).Any();
                    employeePostDay = CreateAndAddEmployeePostDay(employeePostWeek, employeePostCycle, currentDate, sortedPeriodItemsOnDate, forcePreferredDayLength, needPeriodItemsForDate, skipPrioShift);

                    //if (employeePostDay?.SelectedItemsHead != null && employeePostDay.SelectedItemsHead.PercentFutureWeeks == 100 && employeePostWeek.WeekNumber == 1)
                    //{
                    //    if (employeePostCycle.TryOnlyShiftsFromPreviousWeeks)
                    //        percentFromPreferedLength = Convert.ToInt32(decimal.Multiply(percentFromPreferedLength, 3));
                    //    else if (employeePostCycle.FocusOnShiftsFromPreviousWeeks)
                    //        percentFromPreferedLength = Convert.ToInt32(decimal.Multiply(percentFromPreferedLength, 2));
                    //}
                    //if (percentFromPreferedLength > 100)
                    //    percentFromPreferedLength = 100;

                    if (employeePostDay != null && percentFromPreferedLength > 0)
                    {
                        bool lastDayOfWeekAndAllItemsChecked = allItemsChecked && employeePostWeek.RemainingDaysWeek == 0;

                        if (!lastDayOfWeekAndAllItemsChecked && (employeePostDay.PercentFromPreferredLength(preferredLength) > Convert.ToDecimal(percentFromPreferedLength) || employeePostDay.Length == 0))
                        {
                            employeePostCycle.AddDaysPreAnalysisInformationDayShift(employeePostDay, true,
                                $"!lastDayOfWeekAndAllItemsChecked && (!allItemsChecked || employeePostDay.PercentFromPreferredLength(preferredLength) > Convert.ToDecimal(percentFromPreferedLength) || employeePostDay.Length == 0)" +
                                $" {!lastDayOfWeekAndAllItemsChecked} && ({employeePostDay.PercentFromPreferredLength(preferredLength)} > {Convert.ToDecimal(percentFromPreferedLength)} || {employeePostDay.Length == 0})"
                                , preferredLength);

                            if (hasUniques && employeePostDay.Length > 0)
                            {
                                if (employeePostWeek.PurgedEmployeePostDaysWithUniqueSkills == null)
                                    employeePostWeek.PurgedEmployeePostDaysWithUniqueSkills = new List<EmployeePostDay>();

                                employeePostWeek.PurgedEmployeePostDaysWithUniqueSkills.Add(employeePostDay);
                            }

                            if (!hasUniques && employeePostDay.SelectedItemsHead.HasPrioShiftTypes(this.CurrentEmployeePostCyclesRun.PrioShiftTypesIds) && employeePostDay.Length > 0)
                            {
                                if (employeePostWeek.PurgedEmployeePostDaysWithPrioSkills == null)
                                    employeePostWeek.PurgedEmployeePostDaysWithPrioSkills = new List<EmployeePostDay>();

                                employeePostWeek.PurgedEmployeePostDaysWithPrioSkills.Add(employeePostDay);
                            }

                            if (!hasUniques && employeePostWeek.HandledButEmptyDays != null && (7 - employeePostWeek.HandledButEmptyDays.Count) <= (Convert.ToInt16(decimal.Divide(employeePost.WorkDaysWeek, 3)) > 1 ? Convert.ToInt16(decimal.Divide(employeePost.WorkDaysWeek, 3)) : 2) && employeePostDay.Length > 0)
                            {
                                if (employeePostWeek.PurgedEmployeePostDaysInEndOfWeek == null)
                                    employeePostWeek.PurgedEmployeePostDaysInEndOfWeek = new List<EmployeePostDay>();

                                employeePostWeek.PurgedEmployeePostDaysInEndOfWeek.Add(employeePostDay);
                            }

                            employeePostWeek.AddEmployeePostDayInformation(employeePostDay, true);
                            employeePostCycle.TryRemoveEmployeePostDay(employeePostDay);
                            missingLength = preferredDayLength - employeePostDay.Length;
                            employeePostDay = null;

                        }
                        else
                        {
                            missingLength = preferredDayLength - employeePostDay.Length;
                            employeePostCycle.AddDaysPreAnalysisInformationDayShift(employeePostDay, false, "", preferredLength);
                            employeePostWeek.AddEmployeePostDayInformation(employeePostDay);
                        }
                    }

                    if (employeePostDay != null && employeePostDay.Length > 0)
                        stopLimited = true;
                }
            }

            return employeePostDay;
        }

        private List<CalculationPeriodItem> LimitPeriodItemsOnSkills(List<CalculationPeriodItem> periodItems, List<EmployeePostDTO> employeePosts, int currentEmployeePostId, bool onlyUnique, int percent, bool addShorterShiftsThanMissingLenght, bool splitshifts, int missingLength)
        {
            List<CalculationPeriodItem> limited = new List<CalculationPeriodItem>();
            var releventEmployeePosts = employeePosts.Where(w => w.WorkTimeWeek > 0).ToList();

            if (!periodItems.Any())
                return limited;

            var clones = ClonePeriodItems(periodItems);

            foreach (var clone in clones)
                clone.CalculationGuid = Guid.NewGuid();

            if (this.CurrentEmployeePostCyclesRun == null)
                return periodItems;

            List<int> shiftTypeIds = clones.Where(s => s.ShiftTypeId.HasValue).Select(s => s.ShiftTypeId.Value).Distinct().ToList();

            if (shiftTypeIds.Count == 0)
                return periodItems;

            if (this.ShiftTypes == null || this.ShiftTypes.Count == 0)
                return periodItems;

            foreach (var id in shiftTypeIds)
            {
                ShiftTypeDTO shiftType = this.ShiftTypes.Where(s => s.ShiftTypeId == id).FirstOrDefault();
                int matches = 0;

                if (shiftType == null)
                    continue;

                int numberOfPossible = this.CurrentEmployeePostCyclesRun.NumberOfPossibleEmployeePostsLeftOneShiftType(shiftType);

                if (numberOfPossible == 0)
                {
                    limited.AddRange(clones.Where(p => p.ShiftTypeId == id));
                    continue;
                }

                foreach (var employeePost in releventEmployeePosts.Where(w => w.EmployeePostId != currentEmployeePostId))
                {
                    if (employeePost.SkillMatch(shiftType))
                        matches++;
                }

                if (matches == 0 && onlyUnique)
                    limited.AddRange(clones.Where(p => p.ShiftTypeId == id).ToList());
                else if (!onlyUnique)
                {
                    if (decimal.Divide(matches, releventEmployeePosts.Count) < decimal.Divide(percent, 100))
                        limited.AddRange(clones.Where(p => p.ShiftTypeId == id).ToList());
                }
            }

            if (addShorterShiftsThanMissingLenght && missingLength > 0 && limited.Count != periodItems.Count)
            {
                var remainingClones = clones.Where(w => !limited.Select(s => s.CalculationGuid).Contains(w.CalculationGuid)).ToList();

                if (remainingClones.Any())
                {
                    DateTime start = limited.OrderBy(o => o.TimeSlot.MinFrom).First().TimeSlot.MinFrom;
                    DateTime stop = limited.OrderBy(o => o.TimeSlot.MaxTo).Last().TimeSlot.MaxTo;
                    limited.AddRange(ClonePeriodItems(remainingClones, setTempGuidAsCalculationGuid: true).Where(i => (i.TimeSlot.MinFrom >= start || i.TimeSlot.MaxTo <= stop) && i.Length <= missingLength).ToList());
                }
            }
            if (splitshifts && missingLength > 0 && limited.Count != periodItems.Count)
            {
                var remainingClones = clones.Where(w => !limited.Select(s => s.CalculationGuid).Contains(w.CalculationGuid)).ToList();

                if (remainingClones.Any())
                {
                    DateTime start = limited.OrderBy(o => o.TimeSlot.MinFrom).First().TimeSlot.MinFrom;
                    DateTime stop = limited.OrderBy(o => o.TimeSlot.MaxTo).Last().TimeSlot.MaxTo;
                    DateTime middle = start.AddMinutes(Convert.ToInt32(decimal.Divide(Convert.ToDecimal((stop - start).TotalMinutes), 2)));
                    List<DateTime> splitToTimes = new List<DateTime>();
                    List<DateTime> splitFromTimes = new List<DateTime>();
                    limited.ForEach(f => splitToTimes.Add(f.TimeSlot.From));
                    limited.ForEach(f => splitFromTimes.Add(f.TimeSlot.To));
                    splitToTimes = splitToTimes.Distinct().Where(w => w <= middle).OrderBy(o => o).ToList();
                    splitFromTimes = splitFromTimes.Distinct().Where(w => w >= middle).OrderBy(o => o).ToList();
                    List<CalculationPeriodItem> splittableItems = ClonePeriodItems(remainingClones, setTempGuidAsCalculationGuid: true).Where(i => (i.TimeSlot.MinFrom >= start || i.TimeSlot.MaxTo <= stop) && i.MinSplitLength > 0).ToList();
                    List<CalculationPeriodItem> splittedItems = new List<CalculationPeriodItem>();

                    if (missingLength > 0 && splittableItems.Any())
                    {
                        foreach (var item in ClonePeriodItems(splittableItems.Where(w => w.MinSplitLength <= missingLength).ToList()))
                        {
                            foreach (var split in ClonePeriodItems(SplitShifts(splittableItems, missingLength, splitToTimes, splitFromTimes)))
                            {
                                if (split.Length < missingLength + this.CalculationOptions.Interval && split.Length > missingLength - this.CalculationOptions.Interval)
                                    splittedItems.Add(split);
                            }
                        }

                        if (splittedItems.Any())
                        {
                            limited.RemoveAll(i => splittedItems.Select(s => s.TempGuid).Contains(i.TempGuid));
                            limited.AddRange(splittedItems);
                        }
                    }
                }
            }

            return limited;
        }

        private EmployeePostDay CreateAndAddEmployeePostDay(EmployeePostWeek employeePostWeek, EmployeePostCycle employeePostCycle, DateTime currentDate, List<CalculationPeriodItem> sortedPeriodItemsOnDate, bool forcePreferedLength, List<CalculationPeriodItem> needPeriodItemsForDate, bool skipPrioShift = false)
        {
            int restSincePrevDayBreachMinutes = 0;
            int restToNextDayBreachMinutes = 0;
            var preferedLength = employeePostWeek.PreferredDayLength(currentDate.DayOfWeek, employeePostCycle);
            int remainingMinutesWeekInclPreviousWeek = employeePostWeek.RemainingMinutesWeekInclPreviousWeek;

            foreach (var item in sortedPeriodItemsOnDate)
            {
                item.Date = currentDate;
                item.ScheduleDate = currentDate;
            }

            #region Validate Against need

            List<CalculationPeriodItem> needFromTask = needPeriodItemsForDate.Where(w => w.TimeScheduleTaskId.HasValue).ToList();
            List<CalculationPeriodItem> needFromDelivery = needPeriodItemsForDate.Where(w => w.IncomingDeliveryRowId.HasValue).ToList();
            List<CalculationPeriodItem> fromTask = sortedPeriodItemsOnDate.Where(w => w.TimeScheduleTaskId.HasValue).ToList();
            List<CalculationPeriodItem> fromDelivery = sortedPeriodItemsOnDate.Where(w => w.IncomingDeliveryRowId.HasValue).ToList();
            List<Guid> inValidGroups = new List<Guid>();

            if (fromTask.Any())
            {
                var groups = fromTask.GroupBy(g => g.TimeScheduleTaskKey + "#" + g.CalculationGuid);

                foreach (var group in groups)
                {
                    string taskId = group.FirstOrDefault().TimeScheduleTaskKey;
                    if (group.Where(w => !w.IsBreak).Sum(s => s.Length) > needFromTask.Where(w => w.TimeScheduleTaskKey == taskId).Sum(s => s.Length))
                        inValidGroups.Add(group.FirstOrDefault().CalculationGuid);
                }
            }

            if (fromDelivery.Any())
            {
                var groups = fromDelivery.GroupBy(g => g.IncomingDeliveryRowKey + "#" + g.CalculationGuid);

                foreach (var group in groups)
                {
                    string rowKey = group.FirstOrDefault().IncomingDeliveryRowKey;
                    if (group.Where(w => !w.IsBreak).Sum(s => s.Length) > needFromDelivery.Where(w => w.IncomingDeliveryRowKey == rowKey).Sum(s => s.Length))
                        inValidGroups.Add(group.FirstOrDefault().CalculationGuid);
                }
            }

            if (inValidGroups.Count > 0)
            {
                sortedPeriodItemsOnDate = sortedPeriodItemsOnDate.Where(w => !inValidGroups.Contains(w.CalculationGuid)).ToList();
                this.CurrentEmployeePostCyclesRun.EmployeePostCyclesRunInformation.LogInfo("Invalid: Time exceeded task or delivery");
            }

            #endregion

            if (sortedPeriodItemsOnDate.Any())
            {
                bool doNotAllowLongerthanPreferdLength = forcePreferedLength && employeePostWeek.RemainingDaysWeek == 1 && employeePostWeek.WeekNumber == employeePostCycle.EmployeePostWeeks.Count;

                EmployeePostDay employeePostDay = new EmployeePostDay(currentDate, sortedPeriodItemsOnDate, this.CalculationOptions);
                employeePostCycle.TryAddAndValidateEmployeeDay(employeePostDay);

                var allHeads = GroupStaffingNeedsCalcutionPeriodItems(employeePostDay.AllMatchedCalculationPeriodItems);

                #region Check future weeks

                SetPercentAvailableFutureWeeks(allHeads, employeePostCycle, employeePostWeek, employeePostCycle.InitialValidItems);

                #endregion

                if (doNotAllowLongerthanPreferdLength)
                    allHeads = allHeads.Where(w => w.Length <= preferedLength).ToList();

                if (forcePreferedLength && employeePostWeek.WeekNumber == employeePostCycle.EmployeePostWeeks.Count)
                    allHeads = allHeads.Where(w => w.Length <= remainingMinutesWeekInclPreviousWeek).ToList();

                allHeads = allHeads.Where(i => employeePostCycle.IsDayValid(i, employeePostDay.Date, true, out restSincePrevDayBreachMinutes, out restToNextDayBreachMinutes)).ToList();
                var allHeadsValidRules = employeePostCycle.GetItemsValidAgainstScheduleCycleRules(employeePostDay.Date, employeePostDay.AllMatchedCalculationPeriodItems);

                var bestHeadIgnoreRules = employeePostCycle.GetBestPeriodItemsGroupHead(this.CurrentEmployeePostCyclesRun, employeePostWeek, currentDate, allHeads.Where(i => !i.HasHoles).ToList(), forcePreferedLength, preferedLength);
                var bestHeadValidRules = employeePostCycle.GetBestPeriodItemsGroupHead(this.CurrentEmployeePostCyclesRun, employeePostWeek, currentDate, allHeadsValidRules.Where(i => !i.HasHoles).ToList(), forcePreferedLength, preferedLength);

                if ((currentDate.DayOfWeek != DayOfWeek.Sunday && currentDate.DayOfWeek != DayOfWeek.Saturday) && employeePostWeek.WeekNumber != 1 && bestHeadIgnoreRules.Length != bestHeadValidRules.Length)
                {
                    decimal percentIgnoreRules = 10000;
                    if (bestHeadIgnoreRules != null && bestHeadIgnoreRules.Length > 0)
                        percentIgnoreRules = Math.Abs((decimal.Multiply(100, decimal.Divide((bestHeadIgnoreRules.Length - preferedLength), bestHeadIgnoreRules.Length))));

                    decimal percentValidRules = 10000;
                    if (bestHeadValidRules != null && bestHeadValidRules.Length > 0)
                        percentValidRules = Math.Abs((decimal.Multiply(100, decimal.Divide((bestHeadValidRules.Length - preferedLength), bestHeadValidRules.Length))));

                    if ((percentValidRules > 25 && percentValidRules / 2 > percentIgnoreRules) ||
                        (forcePreferedLength && percentValidRules > 4 && percentValidRules > percentIgnoreRules))
                    {
                        var bestRule = employeePostCycle.GetBestMatchingRule(currentDate.DayOfWeek, bestHeadIgnoreRules.StartTime, bestHeadIgnoreRules.StopTime);
                        var ruleIsMaxedOut = false;

                        if (bestRule != null && !employeePostCycle.IsScheduleCycleTypeMaxedOutCurrentWeek(currentDate, bestRule.ScheduleCycleRuleTypeDTO, employeePostWeek.WeekNumber) && employeePostWeek.WeekNumber != employeePostCycle.EmployeePostWeeks.Count)
                            ruleIsMaxedOut = true;

                        if (!ruleIsMaxedOut && (percentIgnoreRules < 5 || (forcePreferedLength && preferedLength <= bestHeadIgnoreRules.Length)))
                            employeePostDay.SelectedItemsHead = bestHeadIgnoreRules;
                    }
                }

                if (employeePostDay.SelectedItemsHead == null || employeePostDay.SelectedItemsHead.Length == 0)
                {
                    List<PeriodItemsGroupHead> heads = employeePostCycle.GetItemsValidAgainstScheduleCycleRules(employeePostDay.Date, employeePostDay.AllMatchedCalculationPeriodItems);

                    if (doNotAllowLongerthanPreferdLength)
                        heads = heads.Where(w => w.Length <= preferedLength).ToList();

                    if (forcePreferedLength && employeePostWeek.WeekNumber == employeePostCycle.EmployeePostWeeks.Count)
                        heads = heads.Where(w => w.Length <= remainingMinutesWeekInclPreviousWeek).ToList();

                    if (forcePreferedLength && currentDate.DayOfWeek != DayOfWeek.Sunday && currentDate.DayOfWeek != DayOfWeek.Saturday && heads.Count == 0)
                    {
                        heads = GroupStaffingNeedsCalcutionPeriodItems(employeePostDay.AllMatchedCalculationPeriodItems);

                        if (doNotAllowLongerthanPreferdLength)
                            heads = heads.Where(w => w.Length <= preferedLength).ToList();

                        if (forcePreferedLength && employeePostWeek.WeekNumber == employeePostCycle.EmployeePostWeeks.Count)
                            heads = heads.Where(w => w.Length <= remainingMinutesWeekInclPreviousWeek).ToList();

                        heads = heads.Where(i => employeePostCycle.IsDayValid(i, employeePostDay.Date, true, out restSincePrevDayBreachMinutes, out restToNextDayBreachMinutes)).ToList();
                    }
                    else
                    {
                        heads = heads.Where(i => employeePostCycle.IsDayValid(i, employeePostDay.Date, false, out restSincePrevDayBreachMinutes, out restToNextDayBreachMinutes)).ToList();
                        heads = employeePostCycle.HasAvailableScheduleCycleRuleTypes(employeePostDay.Date, heads);
                    }
                    heads = heads.Where(i => !i.HasHoles).ToList();

                    #region Check future weeks

                    SetPercentAvailableFutureWeeks(heads, employeePostCycle, employeePostWeek, employeePostCycle.InitialValidItems);

                    #endregion

                    if (heads.Any())
                    {
                        var bestHead = employeePostCycle.GetBestPeriodItemsGroupHead(this.CurrentEmployeePostCyclesRun, employeePostWeek, currentDate, heads.Where(i => !i.HasHoles).ToList(), forcePreferedLength, preferedLength, skipPrioShift);
                        employeePostDay.SelectedItemsHead = bestHead;
                    }
                    else
                    {
                        employeePostCycle.TryRemoveEmployeePostDay(employeePostDay);
                        employeePostDay = null;
                    }
                }

                return employeePostDay;
            }

            return null;
        }

        private List<CalculationPeriodItem> HandlePeriodItemsForEmployeePost(DayOfWeek dayOfWeek, DateTime currentDate, EmployeePostCycle employeePostCycle, EmployeePostWeek employeePostWeek, List<CalculationPeriodItem> periodItems, bool allowSplit, bool forcePreferredDayLength, EmployeePostDTO employeePost, List<ShiftTypeDTO> shiftTypeDTOs)
        {
            if (periodItems.Count == 0)
                return periodItems;

            foreach (var item in periodItems.Where(i => !i.TempGuid.HasValue))
                item.CalculationGuid = Guid.NewGuid();

            #region Prereq

            //Our EmployeePost, get all that fits, if a day is valid after first try, keep it that way. If not continue.

            InetBeforeEmployeePostDay(currentDate, !this.CalculationOptions.AddOnlyDockedPeriodItems);
            List<CalculationPeriodItem> filteredPeriodItems = new List<CalculationPeriodItem>();
            List<CalculationPeriodItem> secondFilteredPeriodItems = new List<CalculationPeriodItem>();
            List<CalculationPeriodItem> sortedPeriodItems = new List<CalculationPeriodItem>();
            List<ShiftTypeDTO> filteredShiftTypeDTOs = new List<ShiftTypeDTO>();

            int preferredDayLength = employeePostWeek.PreferredDayLength(dayOfWeek, employeePostCycle);
            if (preferredDayLength == 0)
                return new List<CalculationPeriodItem>();

            #region FirstPossible Start move and cut.
            bool adjustedStart = false;
            this.CalculationOptions.FirstPossibleStart = employeePostCycle.GetFirstPossibleStart(currentDate, employeePost, employeePostWeek, employeePostCycle, periodItems, out adjustedStart);
            this.CalculationOptions.LastPossibleStop = employeePostCycle.GetLastPossibleStop(currentDate, employeePost, employeePostWeek, employeePostCycle, periodItems, adjustedStart);

            foreach (var item in periodItems)
            {
                item.TimeSlot.MoveToEndOfSlot();

                if (CalendarUtility.MergeDateAndTime(currentDate, item.TimeSlot.From, ignoreSqlServerDateTime: true) < this.CalculationOptions.FirstPossibleStart)
                {
                    var splitted = SplitShift(item, CalendarUtility.MergeDateAndTime(item.TimeSlot.From, this.CalculationOptions.FirstPossibleStart, ignoreSqlServerDateTime: true), item.TimeSlot.To, setNewGuid: true);

                    foreach (var split in splitted)
                    {
                        split.TimeSlot.MoveToEndOfSlot();
                        if (CalendarUtility.MergeDateAndTime(this.CalculationOptions.FirstPossibleStart, split.TimeSlot.From, ignoreSqlServerDateTime: true) >= this.CalculationOptions.FirstPossibleStart)
                            filteredPeriodItems.Add(split);
                    }
                }
                else
                {
                    filteredPeriodItems.Add(item);
                }
            }

            foreach (var item in periodItems)
                item.TimeSlot.MoveToMiddleOfSlot(this.CalculationOptions.Interval);

            filteredPeriodItems = filteredPeriodItems.Where(w => CalendarUtility.MergeDateAndTime(currentDate, (w.StartTime.AddMinutes(preferredDayLength) < w.TimeSlot.To ? w.StartTime.AddMinutes(preferredDayLength) : w.TimeSlot.To), ignoreSqlServerDateTime: true).AddMinutes(w.IsNetTime ? GetBreakMinutesBeforeSplit(w) : 0) <= this.CalculationOptions.LastPossibleStop).ToList();

            #endregion

            #endregion

            #region Set ScheduleRuleType time Interval

            var unMaxed = employeePostCycle.GetUnMaxScheduleCycleRuleTypesForCycle();
            var filteredUnMaxed = unMaxed.Where(w => w.Item2.DayOfWeekIds.Contains((int)dayOfWeek));

            if (filteredUnMaxed.Any())
            {
                this.MinTimeFrom = filteredUnMaxed.OrderBy(o => o.Item2.StartTime).FirstOrDefault().Item2.StartTime.AddMinutes(-this.CalculationOptions.MaxLength);
                this.MinTimeFrom = this.MinTimeFrom < CalendarUtility.DATETIME_DEFAULT ? CalendarUtility.DATETIME_DEFAULT : this.MinTimeFrom;
                this.MinTimeFrom = CalendarUtility.AdjustAccordingToInterval(this.MinTimeFrom, 0, this.CalculationOptions.Interval, alwaysReduce: true);
                this.MaxTimeTo = filteredUnMaxed.OrderBy(o => o.Item2.StopTime).LastOrDefault().Item2.StopTime;
            }
            else
            {
                this.MinTimeFrom = CalendarUtility.DATETIME_DEFAULT;
                this.MaxTimeTo = CalendarUtility.GetEndOfDay(this.MinTimeFrom);
            }


            #endregion

            #region Find fitting rows

            #region Filter

            filteredShiftTypeDTOs = MatchSkills(employeePost, shiftTypeDTOs);
            filteredPeriodItems = FilterMatchedPeriodItems(filteredPeriodItems, employeePost, filteredShiftTypeDTOs, currentDate, this.CalculationOptions.FirstPossibleStart, dayOfWeek, checkLength: false, ignoreFreeDays: employeePostCycle.IgnoreFreeDays);
            bool? isEvening = null;

            if (!employeePostCycle.IgnoreFreeDays && (employeePostWeek.FreeDays.Contains(currentDate.AddDays(1).DayOfWeek) || (!employeePostWeek.IsWeekendWeek && dayOfWeek == DayOfWeek.Friday)))
                isEvening = false;
            else if (!employeePostCycle.IgnoreFreeDays && employeePostWeek.FreeDays.Contains(currentDate.AddDays(-1).DayOfWeek))
                isEvening = true;

            if (!isEvening.HasValue && filteredUnMaxed.Any())
            {
                DateTime eveningStarts = employeePostCycle.EveningStarts(dayOfWeek); //TODO Setting on rule?

                var unreachedRules = employeePostCycle.GetUnReachedScheduleCycleRuleTypesForCycle().OrderByDescending(o => o.Item1);

                foreach (var ruleGroup in unreachedRules.Where(w => w.Item2.DayOfWeekIds.Contains((int)dayOfWeek)).GroupBy(g => g.Item1).Take(1))
                {
                    foreach (var rule in ruleGroup.OrderByDescending(o => o.Item2.StopTime))
                    {
                        isEvening = employeePostCycle.IsEvening(rule.Item2, currentDate.DayOfWeek);

                        if (isEvening.HasValue && isEvening.Value && employeePostCycle.IsScheduleCycleTypeUsedOutCurrentWeek(currentDate, ruleGroup.FirstOrDefault().Item2, employeePostWeek.WeekNumber))
                            isEvening = null;

                        if (isEvening.HasValue)
                            break;
                    }

                    if (isEvening.HasValue)
                        break;
                }
            }

            if (!isEvening.HasValue)
                isEvening = false;

            if (allowSplit)
                filteredPeriodItems = SplitItemsToFitUniqueOrPrioShifts(filteredPeriodItems, employeePost, forcePreferredDayLength, preferredDayLength, isEvening.Value);

            #endregion

            #endregion

            #region Tetris

            this.CalculationOptions.OptimalLength = preferredDayLength;

            if (filteredPeriodItems.Count > 0)
                sortedPeriodItems = FallLikeTetris(filteredPeriodItems, allowSplit: allowSplit, employeePostDTO: employeePost);
            else
                sortedPeriodItems = new List<CalculationPeriodItem>();

            foreach (var setPeriodGuid in sortedPeriodItems)
            {
                setPeriodGuid.EmployeePostId = employeePost.EmployeePostId;
                setPeriodGuid.Weekday = dayOfWeek;
                setPeriodGuid.PeriodGuid = Guid.NewGuid();
                setPeriodGuid.ScheduleDate = currentDate;
            }
            #endregion

            return MergeShifts(sortedPeriodItems);
        }

        private List<CalculationPeriodItem> SplitItemsToFitUniqueOrPrioShifts(List<CalculationPeriodItem> periodItems, EmployeePostDTO employeePost, bool forcePreferredDayLength, int preferredDayLength, bool isEvening)
        {
            bool hasUnique = false;
            bool hasPrio = false;
            DateTime preferredSplitTime = CalendarUtility.DATETIME_DEFAULT;
            int minutesFromUniqueShifts = 0;
            Guid? suggestedTargetCalculationGuid = null;

            #region Try to find good splitime based on PrioItems

            var itemsShorterThan = forcePreferredDayLength ? preferredDayLength : Convert.ToInt32(decimal.Multiply(preferredDayLength, new decimal(0.8)));
            var toShortUniqueItems = this.CurrentEmployeePostCyclesRun.GetItemsOnlyEmployeePostCanTake(periodItems, employeePost.EmployeePostId).Where(w => w.Length < preferredDayLength);

            if (toShortUniqueItems.Any())
            {
                if (isEvening && toShortUniqueItems.OrderByDescending(o => o.TimeSlot.To).First().TimeSlot.To.Hour < 16)
                    isEvening = false;

                hasUnique = true;
                if (!isEvening)
                {
                    preferredSplitTime = toShortUniqueItems.OrderBy(o => o.TimeSlot.From).First().TimeSlot.To;
                    suggestedTargetCalculationGuid = toShortUniqueItems.OrderBy(o => o.TimeSlot.From).First().CalculationGuid;
                    minutesFromUniqueShifts = toShortUniqueItems.OrderBy(o => o.TimeSlot.From).First().TimeSlot.TimeSlotLength;
                }
                else
                {
                    preferredSplitTime = toShortUniqueItems.OrderByDescending(o => o.TimeSlot.To).First().TimeSlot.From;
                    suggestedTargetCalculationGuid = toShortUniqueItems.OrderByDescending(o => o.TimeSlot.To).First().CalculationGuid;
                    minutesFromUniqueShifts = toShortUniqueItems.OrderByDescending(o => o.TimeSlot.To).First().TimeSlot.TimeSlotLength;
                }
            }
            else if (this.CurrentEmployeePostCyclesRun.PrioShiftTypesIds.Any())
            {
                var prioItems = periodItems.Where(w => w.ShiftTypeId.HasValue && w.Length < preferredDayLength && this.CurrentEmployeePostCyclesRun.PrioShiftTypesIds.Contains(w.ShiftTypeId.Value));

                if (prioItems.Any())
                {
                    if (isEvening && prioItems.OrderByDescending(o => o.TimeSlot.To).First().TimeSlot.To.Hour < 13)
                        isEvening = false;

                    hasPrio = true;
                    if (!isEvening)
                    {
                        preferredSplitTime = prioItems.OrderBy(o => o.TimeSlot.From).First().TimeSlot.To;
                        suggestedTargetCalculationGuid = prioItems.OrderBy(o => o.TimeSlot.From).First().CalculationGuid;
                        minutesFromUniqueShifts = prioItems.OrderBy(o => o.TimeSlot.From).First().TimeSlot.TimeSlotLength;
                    }
                    else
                    {
                        preferredSplitTime = prioItems.OrderByDescending(o => o.TimeSlot.To).First().TimeSlot.From;
                        suggestedTargetCalculationGuid = prioItems.OrderByDescending(o => o.TimeSlot.To).First().CalculationGuid;
                        minutesFromUniqueShifts = prioItems.OrderByDescending(o => o.TimeSlot.To).First().TimeSlot.TimeSlotLength;
                    }
                }
            }

            int breaklength = minutesFromUniqueShifts != 0 ? GetBreakMinutesBeforeSplit(preferredDayLength, periodItems.FirstOrDefault().Date.Value, periodItems.FirstOrDefault().TimeSlot.From, periodItems.FirstOrDefault().ShiftTypeId, periodItems.FirstOrDefault().Date.Value.DayOfWeek, isNetTime: false) : 0;
            minutesFromUniqueShifts -= CalendarUtility.AdjustAccordingToInterval(Convert.ToInt32(decimal.Multiply(decimal.Divide(minutesFromUniqueShifts, preferredDayLength), breaklength)), this.CalculationOptions.Interval, alwaysReduce: true);
            List<CalculationPeriodItem> splittable = new List<CalculationPeriodItem>();

            #endregion

            if (minutesFromUniqueShifts != 0)
            {
                if (forcePreferredDayLength || (hasUnique || hasPrio))
                    splittable = periodItems.Where(w => w.MinSplitLength > 0 && w.IsFixed && !w.OnlyOneEmployee && CalendarUtility.IsDateInRange(preferredSplitTime, w.TimeSlot.From, w.TimeSlot.To) && preferredSplitTime != w.TimeSlot.From && preferredSplitTime != w.TimeSlot.To).ToList();
                else
                    splittable = periodItems.Where(w => w.MinSplitLength > 0 && w.ShiftType != null && w.ShiftType.ShiftTypeSkills != null && w.ShiftType.ShiftTypeSkills.Any() && w.IsFixed && !w.OnlyOneEmployee && w.Length > Decimal.Multiply((preferredDayLength - minutesFromUniqueShifts), new decimal(1.2))).ToList();

                var unsplittable = periodItems.Where(w => !splittable.Contains(w)).ToList();

                if (splittable.Any())
                {
                    periodItems = new List<CalculationPeriodItem>();

                    foreach (var item in splittable)
                    {
                        if (preferredSplitTime != CalendarUtility.DATETIME_DEFAULT && CalendarUtility.IsDateInRange(preferredSplitTime, item.TimeSlot.From, item.TimeSlot.To) && preferredSplitTime != item.TimeSlot.From && preferredSplitTime != item.TimeSlot.To)
                        {
                            int minutes = Convert.ToInt32(Math.Abs((preferredSplitTime - item.TimeSlot.From).TotalMinutes));

                            if (hasPrio && minutes <= 30)
                            {
                                periodItems.Add(item);
                                continue;
                            }

                            minutes = Convert.ToInt32(Math.Abs((preferredSplitTime - item.TimeSlot.To).TotalMinutes));

                            if (hasPrio && minutes <= 30)
                            {
                                periodItems.Add(item);
                                continue;
                            }

                            var splitted = SplitShift(item, preferredSplitTime);
                            var remainingMinutes = preferredDayLength - minutesFromUniqueShifts;
                            List<CalculationPeriodItem> splittedAgain = new List<CalculationPeriodItem>();

                            foreach (var alreadySplitted in splitted)
                            {
                                if (alreadySplitted.Length > Decimal.Multiply(remainingMinutes, new decimal(1.0)))
                                {
                                    splittedAgain.AddRange(SplitShift(alreadySplitted, forceLengthMinutes: isEvening ? alreadySplitted.Length - remainingMinutes : remainingMinutes, runRecursive: false, reverse: !isEvening));

                                    if (splittedAgain.Count > 1)
                                        splittedAgain.Where(w => w.TimeSlot.From == preferredSplitTime || w.TimeSlot.To == preferredSplitTime).ToList().ForEach(f => f.SuggestedTargetCalculationGuid = suggestedTargetCalculationGuid);
                                }
                                else
                                    splittedAgain.Add(alreadySplitted);
                            }

                            periodItems.AddRange(splittedAgain);
                        }
                        else
                            periodItems.AddRange(SplitShift(item, runRecursive: true, reverse: isEvening));
                    }

                    periodItems.AddRange(unsplittable);

                    return periodItems;
                }
            }

            if (!forcePreferredDayLength)
            {
                var splittable1 = periodItems.Where(w => w.MinSplitLength > 0 && w.IsFixed && !w.OnlyOneEmployee && w.Length > Decimal.Multiply(preferredDayLength, new decimal(1.2))).ToList();
                var unsplittable1 = periodItems.Where(w => !splittable1.Contains(w)).ToList();
                periodItems = new List<CalculationPeriodItem>();
                foreach (var item in splittable1)
                {
                    periodItems.AddRange(SplitShift(item, runRecursive: true, reverse: isEvening));
                }

                // splittable = SplitShifts(splittable, forceLengthMinutes: CalendarUtility.AjustAccordingToInterval(preferredDayLength / 3, this.CalculationOptions.Interval), tryReverse: true);
                //filteredPeriodItems = splittable;
                periodItems.AddRange(unsplittable1);
            }
            else if (forcePreferredDayLength)
            {
                var splittable2 = periodItems.Where(w => w.MinSplitLength > 0 && w.IsFixed && !w.OnlyOneEmployee && w.Length > preferredDayLength).ToList();
                var unsplittable2 = periodItems.Where(w => !splittable2.Contains(w)).ToList();
                periodItems = new List<CalculationPeriodItem>();
                foreach (var item in splittable2)
                {
                    periodItems.AddRange(SplitShift(item, runRecursive: true, reverse: isEvening));
                }
                periodItems.AddRange(unsplittable2);
            }

            return periodItems;
        }

        private List<CalculationPeriodItem> LimitNumberOfPeriodItems(EmployeePostCycle employeePostCycle, DayOfWeek dayOfWeek, DateTime date, List<CalculationPeriodItem> periodItems, int preferredDayLength, DateTime firstPossibleStart, bool allowSplit, bool forcePreferredDayLength, EmployeePostDTO employeePostDTO, List<ShiftTypeDTO> filteredShiftTypeDTOs)
        {
            List<CalculationPeriodItem> filteredItems = periodItems.ToList();
            List<CalculationPeriodItem> secondFilteredPeriodItems = new List<CalculationPeriodItem>();

            if (!forcePreferredDayLength)
            {
                secondFilteredPeriodItems = FilterMatchedPeriodItems(filteredItems, employeePostDTO, filteredShiftTypeDTOs, date, firstPossibleStart, dayOfWeek, checkLength: true, getRange: true, minMinutes: 0, maxMinutes: (int)(preferredDayLength * 1.2), ignoreFreeDays: employeePostCycle.IgnoreFreeDays);
            }
            else
            {
                foreach (var itemGroup in filteredItems.GroupBy(g => $"{g.ShiftTypeId}#{filteredItems.FirstOrDefault().TimeSlot.From.Hour}"))
                {
                    int breaklength = GetBreakMinutesBeforeSplit(preferredDayLength, (DateTime)itemGroup.FirstOrDefault().Date, itemGroup.FirstOrDefault().TimeSlot.From, itemGroup.FirstOrDefault().ShiftTypeId, dayOfWeek, isNetTime: true);

                    foreach (var item in itemGroup.Where(i => i.Length + breaklength == preferredDayLength))
                        secondFilteredPeriodItems.Add(item);
                }
            }
            if (secondFilteredPeriodItems.Count == 0 && allowSplit && filteredItems.Any())
            {
                foreach (var periodItem in filteredItems.ToList().Where(i => i.Length > preferredDayLength))
                {
                    int breaklength = GetBreakMinutesBeforeSplit(preferredDayLength, (DateTime)periodItem.Date, periodItem.TimeSlot.From, periodItem.ShiftTypeId, dayOfWeek, isNetTime: true);
                    bool reverse = false;

                    if (periodItem.TimeSlot.From > CalendarUtility.DATETIME_DEFAULT.AddHours(16))
                        reverse = true;

                    secondFilteredPeriodItems.AddRange(SplitShift(periodItem, preferredDayLength + breaklength, setNewGuid: true, reverse: reverse).Where(w => w.Length == preferredDayLength + breaklength));
                }

                if (secondFilteredPeriodItems.Count == 0)
                {
                    secondFilteredPeriodItems = filteredItems;
                    filteredItems = new List<CalculationPeriodItem>();
                }
                else
                    filteredItems = new List<CalculationPeriodItem>();

                if (secondFilteredPeriodItems.Any())
                    filteredItems = secondFilteredPeriodItems;
            }

            if (filteredItems.Count > 100)
                filteredItems = filteredItems.OrderByDescending(i => i.Length).Take(filteredItems.Count / 2).ToList();

            filteredItems = FilterMatchedPeriodItems(filteredItems, employeePostDTO, filteredShiftTypeDTOs, date, firstPossibleStart, dayOfWeek, checkLength: true, ignoreFreeDays: employeePostCycle.IgnoreFreeDays);

            foreach (var item in filteredItems)
            {
                item.ScheduleDate = date;
                item.CalculationGuid = Guid.NewGuid();
                item.TempGuid = null;
            }

            return filteredItems;
        }

        private List<ShiftTypeDTO> MatchSkills(EmployeePostDTO employeePostDTO, List<ShiftTypeDTO> shiftTypeDTOs)
        {
            List<ShiftTypeDTO> filteredShiftTypeDTOs = new List<ShiftTypeDTO>();

            foreach (var type in shiftTypeDTOs)
            {
                if (employeePostDTO.SkillMatch(type))
                    filteredShiftTypeDTOs.Add(type);
                else if (type.ShiftTypeSkills.Count == 0)
                    filteredShiftTypeDTOs.Add(type);
            }

            return filteredShiftTypeDTOs;
        }

        private bool MatchSkill(EmployeePostDTO employeePostDTO, ShiftTypeDTO shiftTypeDTO)
        {
            return employeePostDTO.SkillMatch(shiftTypeDTO);
        }

        private List<CalculationPeriodItem> FilterMatchedPeriodItems(List<CalculationPeriodItem> periodItems, EmployeePostDTO employeePostDTO, List<ShiftTypeDTO> shiftTypeDTOs, DateTime date, DateTime startTime, DayOfWeek? dayOfWeek = null, bool checkLength = false, bool getRange = false, int minMinutes = 0, int maxMinutes = 0, bool ignoreFreeDays = false)
        {
            List<CalculationPeriodItem> filteredPeriodItems = new List<CalculationPeriodItem>();

            if (dayOfWeek == null || employeePostDTO.DayOfWeekValid(dayOfWeek.Value, ignoreFreeDays))
            {
                foreach (var group in GroupStaffingNeedsCalcutionPeriodItems(periodItems))
                {
                    var groupPeriodItems = new List<CalculationPeriodItem>();

                    foreach (var periodItem in group.CalculationPeriodItems.Where(g => CalendarUtility.MergeDateAndTime(date, g.TimeSlot.From, ignoreSqlServerDateTime: true) >= startTime))
                    {
                        foreach (var shiftTypeDTO in shiftTypeDTOs)
                        {
                            if (PeriodItemIsValid(periodItem, shiftTypeDTO.ShiftTypeId, employeePostDTO) && !groupPeriodItems.Contains(periodItem))
                                groupPeriodItems.Add(periodItem);
                        }
                    }

                    // Everything must be valid
                    if (groupPeriodItems.Any())
                        filteredPeriodItems.AddRange(groupPeriodItems);
                }
            }

            #region Check if we can filter out even more

            List<CalculationPeriodItem> filter2 = new List<CalculationPeriodItem>();

            if (checkLength)
            {
                if (getRange && maxMinutes != 0)
                    filteredPeriodItems = GetRangeOfPeriodItems(filteredPeriodItems, minMinutes, maxMinutes, ifZeroReturnUntouched: false);
                else
                    filteredPeriodItems = GetRangeOfPeriodItems(filteredPeriodItems, this.CalculationOptions.OptimalLength, this.CalculationOptions.OptimalLength);
            }

            #endregion

            return filteredPeriodItems;
        }

        private List<CalculationPeriodItem> GetRangeOfPeriodItems(List<CalculationPeriodItem> periodItems, int minMinutes, int maxMinutes, bool ifZeroReturnUntouched = true)
        {
            List<CalculationPeriodItem> filteredItems = new List<CalculationPeriodItem>();

            foreach (var item in periodItems)
            {
                var length = item.TimeSlot.Minutes;
                if (length >= minMinutes && length <= maxMinutes + GetBreakMinutesBeforeSplit(item))
                    filteredItems.Add(item);
            }

            return filteredItems.Count == 0 && ifZeroReturnUntouched ? periodItems : filteredItems;
        }

        #endregion

        #region Tetris

        public List<CalculationPeriodItem> FallLikeTetris(List<CalculationPeriodItem> calculationPeriodItems, bool allowSplit = false, CalculationPeriodItemGroupByType calculationPeriodItemGroupByType = CalculationPeriodItemGroupByType.Unknown, EmployeePostDTO employeePostDTO = null)
        {
            SetTimeSlotToMinFrom(calculationPeriodItems);
            OrderAndSetCalcutionRowNumber(calculationPeriodItems);

            int iterations = Convert.ToInt32(calculationPeriodItems.Count);
            if (iterations < 10)
                iterations = 10;
            int iteration = 0;
            int numberOfMissedIterations = 0;
            int maxNumberOfIterations = iterations < 20 ? iterations * 5 : 60;
            bool forward = true;
            bool forceAddStaffingNeedsCalcutionHead = false;
            int abortBasedOnStats = 0;
            bool hasEmployeePost = employeePostDTO != null;
            bool addOnlyDockedPeriodItems = this.CalculationOptions.AddOnlyDockedPeriodItems;

            if (!hasEmployeePost)
                foreach (var item in GroupStaffingNeedsCalcutionPeriodItems(calculationPeriodItems))
                    this.PeriodItemsGroupHeads.Add(item);

            if (hasEmployeePost)
                iterations = 4;

            while (iteration < iterations)
            {
                iteration++;

                OrderAndSetCalcutionRowNumber(calculationPeriodItems);
                bool changesInIteration = false;

                FallLikeTetriswhile(calculationPeriodItems, this.CalculationOptions.Interval, forward, ref changesInIteration, calculationPeriodItemGroupByType: calculationPeriodItemGroupByType, employeePostDTO: employeePostDTO, forceAddStaffingNeedsCalcutionHead: forceAddStaffingNeedsCalcutionHead);

                if (this.NewBreakPeriodItems.Any())
                {
                    calculationPeriodItems.AddRange(this.NewBreakPeriodItems);
                    this.NewBreakPeriodItems = new List<CalculationPeriodItem>();

                    if (iterations < maxNumberOfIterations)
                        iterations++;

                    if (numberOfMissedIterations > 0)
                        numberOfMissedIterations = 0;
                }

                forceAddStaffingNeedsCalcutionHead = false;

                if (!changesInIteration)
                    numberOfMissedIterations++;
                else
                    numberOfMissedIterations = 0;

                if (!this.CalculationOptions.ForceAddOnlyDockedPeriodItems && (numberOfMissedIterations > 1 || iteration > iterations * 0.5))
                {
                    this.CalculationOptions.AddOnlyDockedPeriodItems = false;
                }

                if (numberOfMissedIterations > 2 || (iteration > iterations * 0.8 && numberOfMissedIterations <= 0))
                {
                    forceAddStaffingNeedsCalcutionHead = true;
                    numberOfMissedIterations = 0;
                }

                if (!forceAddStaffingNeedsCalcutionHead && this.AbortBasedOnStats(iterations, maxNumberOfIterations) && abortBasedOnStats < 3)
                {
                    abortBasedOnStats++;
                    forceAddStaffingNeedsCalcutionHead = true;
                }

                forward = !forward;
                if (!hasEmployeePost)
                    calculationPeriodItems = AddPeriodItemsFromHeads(calculationPeriodItems, calculationPeriodItemGroupByType);

                calculationPeriodItems = calculationPeriodItems.OrderByDescending(o => o.TimeSlot.From).ToList();

                if (this.CalculationOptions.AddOnlyDockedPeriodItems && !hasEmployeePost)
                {
                    var groups = GroupStaffingNeedsCalcutionPeriodItems(calculationPeriodItems);

                    foreach (var group in groups.Where(g => g.HasHoles))
                    {
                        foreach (var item in group.CalculationPeriodItems)
                        {
                            item.CalculationGuid = Guid.NewGuid();
                        }
                    }
                }

                if (numberOfMissedIterations > 5)
                    break;

                this.AddStats();
                if ((this.AbortBasedOnStats(iterations, maxNumberOfIterations) && abortBasedOnStats > 2) || (iteration > 9 && hasEmployeePost))
                    break;
            }

            RemoveStats();

            this.CalculationOptions.AddOnlyDockedPeriodItems = addOnlyDockedPeriodItems;
            List<CalculationPeriodItem> fromHeads = new List<CalculationPeriodItem>();
            if (hasEmployeePost)
            {
                foreach (var item in this.PeriodItemsGroupHeads.Where(w => w.Done))
                {
                    fromHeads.AddRange(item.CalculationPeriodItems);
                }

                return fromHeads;
            }
            else
                return calculationPeriodItems;
        }

        private void FallLikeTetriswhile(List<CalculationPeriodItem> periodItems, int interval, bool forward, ref bool changesInIteration, CalculationPeriodItemGroupByType calculationPeriodItemGroupByType = CalculationPeriodItemGroupByType.Unknown, EmployeePostDTO employeePostDTO = null, bool forceAddStaffingNeedsCalcutionHead = false, bool prioOrderLenght = true)
        {
            changesInIteration = false;
            bool changesInInnerIteration = changesInIteration;
            int iteration = 0;
            int iterations = 0;
            bool hasUniques = this.CalculationOptions.HasUniques;
            bool hasEmployeePost = employeePostDTO != null;

            periodItems = CleanList(periodItems);

            if (calculationPeriodItemGroupByType == CalculationPeriodItemGroupByType.ShiftType)
                SortCalculationRowsOnShiftTypeId(periodItems, interval);

            List<string> checkedGroups = new List<string>();

            List<PeriodItemsGroupHead> groups = GroupStaffingNeedsCalcutionPeriodItems(periodItems, calculationPeriodItemGroupByType: calculationPeriodItemGroupByType, hasUniques: hasUniques);
            iterations = groups.Count;

            if (hasEmployeePost)
            {
                if (prioOrderLenght)
                    groups = GroupStaffingNeedsCalcutionPeriodItems(periodItems, calculationPeriodItemGroupByType: calculationPeriodItemGroupByType).OrderByDescending(o => o.Length).ToList();
                else
                    groups = GroupStaffingNeedsCalcutionPeriodItems(periodItems, calculationPeriodItemGroupByType: calculationPeriodItemGroupByType).OrderBy(o => (forward ? o.StartTime : o.StopTime)).ToList();

                foreach (PeriodItemsGroupHead group in groups)
                {
                    var clones = ClonePeriodItems(periodItems);
                    group.CalculationPeriodItems = clones.Where(w => w.CalculationGuid == group.CalculationGuid).ToList();

                    FallLikeTetrisGroup(group, clones, interval, forward, ref changesInInnerIteration, calculationPeriodItemGroupByType: calculationPeriodItemGroupByType, employeePostDTO: employeePostDTO);
                    //clones.AddRange(group.CalculationPeriodItems.Where(f => f.ReplaceWithBreak));

                    if (TryAddStaffingNeedsCalcutionHead(clones, group.Key, calculationPeriodItemGroupByType, forceAddStaffingNeedsCalcutionHead, false))
                    {
                        PeriodItemsGroupHead calcuationHead = this.PeriodItemsGroupHeads.Where(h => h.Key == group.Key).FirstOrDefault();
                        if (calcuationHead.Done)
                        {
                            if (CalculationOptions.ApplyLengthRule)
                            {
                                if (!calcuationHead.BreaksApplied)
                                {
                                    List<CalculationPeriodItem> groupedPeriodItems = calcuationHead.CalculationPeriodItems;
                                    List<CalculationPeriodItem> fromBreakPeriodItems = GetBreaks(groupedPeriodItems);

                                    Guid newGuid = Guid.NewGuid();
                                    AddPeriodItemsToExistingCalcutionHeadAfterBreaksApplied(fromBreakPeriodItems.Where(f => f.GetKey(calculationPeriodItemGroupByType).Equals(group.Key)).ToList(), group.Key, calculationPeriodItemGroupByType, newGuid);
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                while (iteration < iterations)
                {
                    iteration++;

                    groups = GroupStaffingNeedsCalcutionPeriodItems(periodItems, calculationPeriodItemGroupByType: calculationPeriodItemGroupByType).OrderByDescending(o => o.Length).ToList();
                    foreach (PeriodItemsGroupHead group in groups)
                    {
                        if (SkipGroup(group.Key))
                            continue;
                        if (checkedGroups.Contains(group.Key))
                            continue;

                        FallLikeTetrisGroup(group, periodItems, interval, forward, ref changesInInnerIteration, calculationPeriodItemGroupByType: calculationPeriodItemGroupByType, employeePostDTO: employeePostDTO);

                        if (TryAddStaffingNeedsCalcutionHead(periodItems, group.Key, calculationPeriodItemGroupByType, forceAddStaffingNeedsCalcutionHead))
                        {
                            PeriodItemsGroupHead calcuationHead = this.PeriodItemsGroupHeads.FirstOrDefault(h => h.Key == group.Key);
                            if (calcuationHead != null && (forceAddStaffingNeedsCalcutionHead || calcuationHead.Done) && CalculationOptions.ApplyLengthRule && !calcuationHead.BreaksApplied)
                            {
                                List<CalculationPeriodItem> groupedPeriodItems = calcuationHead.CalculationPeriodItems;
                                List<CalculationPeriodItem> fromBreakPeriodItems = GetBreaks(groupedPeriodItems);

                                foreach (CalculationPeriodItem fromBreakPeriodItem in fromBreakPeriodItems)
                                {
                                    if (!periodItems.Contains(fromBreakPeriodItem))
                                        periodItems.Add(fromBreakPeriodItem);
                                }

                                AddPeriodItemsToExistingCalcutionHeadAfterBreaksApplied(fromBreakPeriodItems.Where(f => f.GetKey(calculationPeriodItemGroupByType).Equals(group.Key)).ToList(), group.Key, calculationPeriodItemGroupByType);

                                changesInIteration = true;
                                return;
                            }
                        }

                        checkedGroups.Add(group.Key);

                        if (changesInInnerIteration)
                            break;
                    }

                    if (changesInInnerIteration)
                    {
                        changesInIteration = changesInInnerIteration;
                        changesInInnerIteration = false;
                    }
                    else
                    {
                        iteration = iterations;
                    }
                }
            }
        }

        private void FallLikeTetrisGroup(PeriodItemsGroupHead group, List<CalculationPeriodItem> periodItems, int interval, bool forward, ref bool changesInIteration, CalculationPeriodItemGroupByType calculationPeriodItemGroupByType = CalculationPeriodItemGroupByType.Unknown, EmployeePostDTO employeePostDTO = null)
        {
            var firstRow = group.CalculationPeriodItems.FirstOrDefault();
            int? shiftTypeId = this.CalculationOptions.MixShiftTypesOnHead ? null : firstRow.ShiftTypeId;
            Guid guid = firstRow.CalculationGuid;
            int rowNr = firstRow.CalculationRowNr;
            int length = group.Length;
            string key = group.Key;

            if (CalculationOptions.ApplyLengthRule)
            {
                if (length > CalculationOptions.MaxLength)
                    return;
            }

            Dictionary<Guid, bool> fittingDict = GetDictWithGuidsAndIsDockedThatCanFit(periodItems, interval, forward, calculationPeriodItemGroupByType, key: key, shiftTypeId: shiftTypeId);

            if (fittingDict.Count < 2 && periodItems.All(a => a.IsFixed))
            {
                periodItems = TryFillUnmergableWithBreaks(periodItems);
                //periodItems.Where(w => w.ReplaceWithBreak && w.CalculationGuid == group.CalculationGuid).ToList().ForEach(f => group.CalculationPeriodItems.Add(f));
                fittingDict = GetDictWithGuidsAndIsDockedThatCanFit(periodItems, interval, forward, calculationPeriodItemGroupByType, key: key, shiftTypeId: shiftTypeId);
            }

            List<CalculationPeriodItem> newPeriodItems = periodItems.Where(r => fittingDict.Select(s => s.Key).Contains(r.CalculationGuid)).ToList();
            List<Guid> fittingGuids = new List<Guid>();
            if (this.CalculationOptions.AddOnlyDockedPeriodItems)
                fittingGuids = fittingDict.Where(f => f.Value).Select(s => s.Key).ToList();
            else
                fittingGuids = fittingDict.Select(s => s.Key).ToList();

            newPeriodItems = OrderPeriodItemsBasedOnPriority(firstRow, newPeriodItems, fittingGuids, employeePostDTO);

            foreach (var item in newPeriodItems.Where(n => n.ShiftTypeId == firstRow.ShiftTypeId && n.SuggestedTargetCalculationGuid == firstRow.CalculationGuid))
            {
                if (item.GetKey(calculationPeriodItemGroupByType).Equals(key))
                    continue;

                item.TempGuid = null;
                DateTime maxTimeTo = this.MaxTimeTo;

                if (item.IsNetTime)
                    maxTimeTo = this.MaxTimeTo.AddMinutes(-60); //TODO: can we set t a better time?

                List<FreeTimeSlot> freeTimeSlots = GetFreeTimeSlots(periodItems, this.MinTimeFrom, maxTimeTo, interval, calculationPeriodItemGroupByType: calculationPeriodItemGroupByType, key: key);

                if (forward)
                    freeTimeSlots = freeTimeSlots.OrderBy(f => f.From).ToList();
                else
                    freeTimeSlots = freeTimeSlots.OrderByDescending(f => f.From).ToList();

                SetGuidAndRowNrOnBestSuitedTimeSlot(item, guid, rowNr, periodItems.Where(r => r.CalculationGuid == guid).ToList(), freeTimeSlots, forward, ref changesInIteration);
            }

            foreach (var item in newPeriodItems.ToList())
            {
                if (item == null)
                    continue;

                if (item.GetKey(calculationPeriodItemGroupByType).Equals(key) || item.PeriodState == SoeEntityState.Deleted)
                    continue;

                item.TempGuid = null;
                bool changesInloop = false;
                List<CalculationPeriodItem> addPeriodItems = new List<CalculationPeriodItem>();
                DateTime maxTimeTo = this.MaxTimeTo;

                if (item.IsNetTime)
                    maxTimeTo = this.MaxTimeTo.AddMinutes(-60); //TODO: can we set t a better time?

                Guid previousGuid = item.CalculationGuid;
                var periodItemsOnGuid = periodItems.Where(r => r.CalculationGuid == guid && r.PeriodState != SoeEntityState.Deleted).ToList();

                List<FreeTimeSlot> freeTimeSlots = GetFreeTimeSlots(periodItemsOnGuid, this.MinTimeFrom, maxTimeTo, interval, calculationPeriodItemGroupByType: calculationPeriodItemGroupByType, key: key);

                if (forward)
                    freeTimeSlots = freeTimeSlots.OrderBy(f => f.From).ToList();
                else
                    freeTimeSlots = freeTimeSlots.OrderByDescending(f => f.From).ToList();

                if (!TryMergeIntoGroup(item, guid, ref changesInloop, ref periodItems))
                {
                    SetGuidAndRowNrOnBestSuitedTimeSlot(item, guid, rowNr, periodItemsOnGuid, freeTimeSlots, forward, ref changesInloop);

                    if (periodItemsOnGuid.Count == 1 && periodItemsOnGuid.Where(i => !i.TimeSlot.IsFixed).Count() == 1 && !changesInloop)
                    {
                        periodItemsOnGuid.FirstOrDefault().TimeSlot.MoveToMiddleOfSlot(this.CalculationOptions.Interval);
                        freeTimeSlots = GetFreeTimeSlots(periodItems, this.MinTimeFrom, maxTimeTo, interval, calculationPeriodItemGroupByType: calculationPeriodItemGroupByType, key: key);

                        if (forward)
                            freeTimeSlots = freeTimeSlots.OrderBy(f => f.From).ToList();
                        else
                            freeTimeSlots = freeTimeSlots.OrderByDescending(f => f.From).ToList();

                        SetGuidAndRowNrOnBestSuitedTimeSlot(item, guid, rowNr, periodItemsOnGuid, freeTimeSlots, forward, ref changesInloop);
                    }

                    if (periodItemsOnGuid.Count == 1 && periodItemsOnGuid.Where(i => !i.TimeSlot.IsFixed).Count() == 1 && !changesInloop)
                    {
                        periodItemsOnGuid.FirstOrDefault().TimeSlot.MoveToEndOfSlot();
                        freeTimeSlots = GetFreeTimeSlots(periodItems, this.MinTimeFrom, maxTimeTo, interval, calculationPeriodItemGroupByType: calculationPeriodItemGroupByType, key: key);

                        if (forward)
                            freeTimeSlots = freeTimeSlots.OrderBy(f => f.From).ToList();
                        else
                            freeTimeSlots = freeTimeSlots.OrderByDescending(f => f.From).ToList();

                        SetGuidAndRowNrOnBestSuitedTimeSlot(item, guid, rowNr, periodItemsOnGuid, freeTimeSlots, forward, ref changesInloop);
                    }
                }

                if (changesInloop)
                {
                    foreach (var periodItem in newPeriodItems.Where(g => g.CalculationGuid == previousGuid))
                    {
                        periodItem.CalculationGuid = Guid.NewGuid();
                    }

                    changesInIteration = changesInloop;
                }
            }

            if (changesInIteration)
                return;
        }

        private Dictionary<Guid, bool> GetDictWithGuidsAndIsDockedThatCanFit(List<CalculationPeriodItem> periodItems, int interval, bool forward, CalculationPeriodItemGroupByType calculationPeriodItemGroupByType = CalculationPeriodItemGroupByType.Unknown, string key = null, int? shiftTypeId = null, bool includeBreaks = false)
        {
            Dictionary<Guid, bool> calculationRowGuids = new Dictionary<Guid, bool>();

            List<FreeTimeSlot> freeTimeSlots = GetFreeTimeSlots(periodItems, this.MinTimeFrom, this.MaxTimeTo, interval, calculationPeriodItemGroupByType: calculationPeriodItemGroupByType, key: key);

            calculationRowGuids.AddRange(GetDictWithGuidsAndIsDockedThatCanFitInner(freeTimeSlots, periodItems, interval, forward, calculationPeriodItemGroupByType, key, shiftTypeId));

            if (periodItems.Where(w => !w.GetKey(calculationPeriodItemGroupByType).Equals(key) && (includeBreaks ? !w.TimeSlot.IsBreak : w.TimeSlot.IsBreak || !w.TimeSlot.IsBreak)).Any())
            {
                var clones = ClonePeriodItems(periodItems);

                foreach (var clone in clones.Where(w => !w.GetKey(calculationPeriodItemGroupByType).Equals(key) && !w.TimeSlot.IsBreak))
                    clone.TimeSlot.MoveToEndOfSlot();

                freeTimeSlots = GetFreeTimeSlots(clones, this.MinTimeFrom, this.MaxTimeTo, interval, calculationPeriodItemGroupByType: calculationPeriodItemGroupByType, key: key);
                var dict = GetDictWithGuidsAndIsDockedThatCanFitInner(freeTimeSlots, clones, interval, forward, calculationPeriodItemGroupByType, key, shiftTypeId);

                foreach (var pair in dict)
                {
                    if (calculationRowGuids.Where(w => w.Key == pair.Key).Count() == 0)
                        calculationRowGuids.Add(pair.Key, pair.Value);
                }
            }

            return calculationRowGuids;
        }

        private Dictionary<Guid, bool> GetDictWithGuidsAndIsDockedThatCanFitInner(List<FreeTimeSlot> freeTimeSlots, List<CalculationPeriodItem> periodItems, int interval, bool forward, CalculationPeriodItemGroupByType calculationPeriodItemGroupByType = CalculationPeriodItemGroupByType.Unknown, string key = null, int? shiftTypeId = null)
        {
            Dictionary<Guid, bool> calculationRowGuids = new Dictionary<Guid, bool>();

            foreach (var item in periodItems.ToList())
            {
                if (calculationRowGuids.ContainsKey(item.CalculationGuid) && calculationRowGuids.GetValue(item.CalculationGuid))
                    continue;

                if (item.GetKey(calculationPeriodItemGroupByType).Equals(key))
                    continue;

                if (!this.CalculationOptions.MixShiftTypesOnHead && shiftTypeId.HasValue && item.ShiftTypeId != shiftTypeId)
                    continue;

                if (SkipItem(item.GetKey(calculationPeriodItemGroupByType), item.CalculationGuid))
                    continue;

                if (forward)
                    freeTimeSlots = freeTimeSlots.OrderBy(f => f.From).ToList();
                else
                    freeTimeSlots = freeTimeSlots.OrderByDescending(f => f.From).ToList();

                bool? itemDoesNotFitInterval = null; 
                foreach (var slot in freeTimeSlots)
                {
                    var clone = item.TimeSlot.Clone();


                    if (clone.IsFixed)
                    {
                        if ((this.CalculationOptions.AddOnlyDockedPeriodItems && (clone.To == slot.To || clone.From == slot.From)) || (!this.CalculationOptions.AddOnlyDockedPeriodItems && CalendarUtility.IsDatesInInterval(clone.From, clone.To, slot.From, slot.To)))
                        {
                            if (!calculationRowGuids.ContainsKey(item.CalculationGuid) && this.CurrentEmployeePostCyclesRun.ContinueOnOverlappning(item, this.Date.HasValue ? this.Date.Value : item.ScheduleDate))
                                calculationRowGuids.Add(item.CalculationGuid, (clone.To == slot.To || clone.From == slot.From));
                        }
                    }
                    else
                    {
                        var freeSlotDoesNotFitInterval = slot.From.Minute % CalculationOptions.Interval != 0 || slot.To.Minute % CalculationOptions.Interval != 0;
                        if (!itemDoesNotFitInterval.HasValue)
                            itemDoesNotFitInterval = item.TimeSlot.From.Minute % CalculationOptions.Interval != 0 || item.TimeSlot.To.Minute % CalculationOptions.Interval != 0;

                        if (freeSlotDoesNotFitInterval && itemDoesNotFitInterval.Value)
                            continue;

                        bool match = false;
                        bool moved = clone.TrySetStopTime(slot.To);

                        if (!moved)
                            moved = clone.TrySetStartTime(slot.From);

                        if (moved)
                        {
                            if ((this.CalculationOptions.AddOnlyDockedPeriodItems && (clone.To == slot.To || clone.From == slot.From)) || (!this.CalculationOptions.AddOnlyDockedPeriodItems && CalendarUtility.IsDatesInInterval(clone.From, clone.To, slot.From, slot.To)))
                            {
                                if (!calculationRowGuids.ContainsKey(item.CalculationGuid) && this.CurrentEmployeePostCyclesRun.ContinueOnOverlappning(item, this.Date.HasValue ? this.Date.Value : item.ScheduleDate))
                                {
                                    calculationRowGuids.Add(item.CalculationGuid, (clone.To == slot.To || clone.From == slot.From));
                                    match = true;
                                }
                            }
                        }

                        if (!match && this.CalculationOptions.AddOnlyDockedPeriodItems)
                        {
                            DateTime startTime = clone.From;

                            int iterations = 24 * 60 / CalculationOptions.Interval;
                            int iteration = 0;

                            if (!forward)
                                clone.TrySetStopTime(slot.From);
                            else
                                clone.TrySetStartTime(slot.To);

                            while (iteration <= iterations)
                            {
                                if (iteration != 0)
                                    AdjustTimeSlot(clone, CalculationOptions.Interval, false, forward: forward);

                                if ((this.CalculationOptions.AddOnlyDockedPeriodItems && (clone.To == slot.To || clone.From == slot.From)) || (!this.CalculationOptions.AddOnlyDockedPeriodItems && CalendarUtility.IsDatesInInterval(clone.From, clone.To, slot.From, slot.To)))
                                {
                                    if (!calculationRowGuids.ContainsKey(item.CalculationGuid))
                                    {
                                        if (this.CurrentEmployeePostCyclesRun.ContinueOnOverlappning(item, this.Date.HasValue ? this.Date.Value : item.ScheduleDate))
                                        {
                                            calculationRowGuids.Add(item.CalculationGuid, (clone.To == slot.To || clone.From == slot.From));
                                            iteration = iterations;
                                        }
                                    }
                                }

                                iteration++;
                            }
                        }
                    }
                }
            }

            return calculationRowGuids;
        }

        private bool TryMergeIntoGroup(CalculationPeriodItem periodItem, Guid guid, ref bool changesInIteration, ref List<CalculationPeriodItem> periodItems)
        {
            var periodItemsOnGuid = periodItems.Where(r => r.CalculationGuid == guid).ToList();

            #region Validate if possible to set the entire shift with the groups limits

            if (periodItemsOnGuid.Where(w => w.MinSplitLength > 0).Any(a => a.IsMergable) && ValidTime(periodItem) && ValidLength(periodItemsOnGuid, periodItem))
            {
                foreach (var item in periodItemsOnGuid)
                {
                    if (!PeriodItemIsValid(periodItem, item.ShiftTypeId, this.CalculationOptions.EmployeePost))
                        return false;
                }

                var group = GroupStaffingNeedsCalcutionPeriodItems(periodItemsOnGuid).FirstOrDefault();

                if (CalendarUtility.GetOverlappingMinutes(periodItem.TimeSlot.From, periodItem.TimeSlot.To, group.StartTime, group.StopTime) > 0)
                {
                    if (!periodItem.IsFixed)
                    {
                        var clone = ClonePeriodItem(periodItem);
                        if (!clone.TimeSlot.TrySetCloseToMiddleTime(group.StartTime, group.StopTime, this.CalculationOptions.Interval))
                            if (!clone.TimeSlot.TrySetStartTime(group.StartTime.AddMinutes(this.CalculationOptions.Interval)))
                                if (!clone.TimeSlot.TrySetStopTime(group.StopTime.AddMinutes(-this.CalculationOptions.Interval)))
                                    return false;

                        if (clone.TimeSlot.From <= group.StartTime || clone.TimeSlot.To >= group.StopTime)
                            return false;

                        if (CalendarUtility.GetOverlappingMinutes(clone.TimeSlot.From, clone.TimeSlot.To, group.StartTime, group.StopTime) == 0)
                            return false;
                        else
                        {
                            if (!periodItem.TimeSlot.TrySetCloseToMiddleTime(group.StartTime, group.StopTime, this.CalculationOptions.Interval))
                                if (!periodItem.TimeSlot.TrySetStartTime(group.StartTime.AddMinutes(this.CalculationOptions.Interval)))
                                    if (!periodItem.TimeSlot.TrySetStopTime(group.StopTime.AddMinutes(-this.CalculationOptions.Interval)))
                                    { }
                        }
                    }
                    else
                    {
                        if (periodItem.TimeSlot.From <= group.StartTime || periodItem.TimeSlot.To >= group.StopTime)
                            return false;

                        if (CalendarUtility.GetOverlappingMinutes(periodItem.TimeSlot.From, periodItem.TimeSlot.To, group.StartTime, group.StopTime) != periodItem.TimeSlot.Minutes)
                            return false;
                    }
                }
                else
                    return false;
            }
            else
            {
                return false;
            }

            #endregion

            #region Try to split shift and mvoed affected shifts to each side

            int length = periodItem.TimeSlot.Minutes;
            bool changes = false;
            var periodsBeforeAffectedShift = periodItemsOnGuid.Where(i => i.TimeSlot.From <= periodItem.TimeSlot.From && !i.IsFixed).OrderBy(o => o.TimeSlot.From).ToList();
            var periodsAfterAffectedShift = periodItemsOnGuid.Where(i => i.TimeSlot.From >= periodItem.TimeSlot.From && !i.IsFixed).OrderBy(o => o.TimeSlot.From).ToList();

            List<CalculationPeriodItem> overlapping = new List<CalculationPeriodItem>();

            if (periodsAfterAffectedShift.Any() &&
                periodsAfterAffectedShift.Last().TimeSlot.To.AddMinutes(length) <= CalendarUtility.MergeDateAndTime(CalendarUtility.DATETIME_DEFAULT, this.CalculationOptions.LastPossibleStop, ignoreSqlServerDateTime: true) &&
                periodsAfterAffectedShift.Last().TimeSlot.To.AddMinutes(length) <= periodsAfterAffectedShift.Last().TimeSlot.MaxTo)
            {
                overlapping = periodsAfterAffectedShift.Where(w => CalendarUtility.GetOverlappingMinutes(w.TimeSlot.From, w.TimeSlot.To, periodItem.TimeSlot.From, periodItem.TimeSlot.To) == length).ToList();
                var last = periodsAfterAffectedShift.Last();

                //Only one overlapped since otherwise it would be to complicated
                if (overlapping.Count == 1 && !overlapping.First().IsFixed && overlapping.First().TimeSlot.From != periodItem.TimeSlot.From && overlapping.First().TimeSlot.To != periodItem.TimeSlot.To)
                {
                    bool stopChanged = false;

                    if (periodsAfterAffectedShift.Count == periodsAfterAffectedShift.Where(w => !w.IsFixed).Count() && last.TimeSlot.MaxTo >= last.TimeSlot.To.AddMinutes(length + this.CalculationOptions.Interval))
                    {
                        foreach (var periodAfter in periodsAfterAffectedShift)
                            periodAfter.TimeSlot.MoveForward(length);
                        stopChanged = true;
                    }
                    else if (!last.IsFixed && last.TimeScheduleTaskId == overlapping.First().TimeScheduleTaskId && last.IncomingDeliveryRowId == overlapping.First().IncomingDeliveryRowId && last.TimeSlot.MaxTo >= last.TimeSlot.To.AddMinutes(length + this.CalculationOptions.Interval))
                    {
                        last.TimeSlot.To = last.TimeSlot.To.AddMinutes(length);
                        stopChanged = true;
                    }

                    if (stopChanged)
                    {
                        if (!overlapping.First().OnlyOneEmployee)
                        {
                            var clone = ClonePeriodItem(overlapping.First());
                            periodItems.Remove(overlapping.First());
                            var splitted = SplitShift(overlapping.First(), periodItem.TimeSlot.From, periodItem.TimeSlot.To, discardOnlyOneEmployee: true, setNewGuid: false);

                            if (splitted.Count == 3)
                            {
                                splitted[1].PeriodState = SoeEntityState.Deleted;
                                splitted[0].CalculationGuid = guid;
                                splitted[2].CalculationGuid = guid;
                                periodItem.CalculationGuid = guid;
                                periodItems.AddRange(splitted.Where(w => w.PeriodState != SoeEntityState.Deleted));
                                changes = true;
                            }
                            else
                            {
                                periodItems.Add(clone);
                                foreach (var periodAfter in periodsAfterAffectedShift)
                                    periodAfter.TimeSlot.MoveBackward(length);
                            }
                        }
                    }
                }
            }

            if (!changes && periodsBeforeAffectedShift.Any() &&
                periodsBeforeAffectedShift.First().TimeSlot.From.AddMinutes(-length) >= CalendarUtility.MergeDateAndTime(CalendarUtility.DATETIME_DEFAULT, this.CalculationOptions.FirstPossibleStart, ignoreSqlServerDateTime: true) &&
                periodsBeforeAffectedShift.First().TimeSlot.From.AddMinutes(-length) >= periodsBeforeAffectedShift.First().TimeSlot.MinFrom)
            {
                overlapping = periodsBeforeAffectedShift.Where(w => CalendarUtility.GetOverlappingMinutes(w.TimeSlot.From, w.TimeSlot.To, periodItem.TimeSlot.From, periodItem.TimeSlot.To) == length).ToList();
                var first = periodsBeforeAffectedShift.First();

                //Only one overlapped since otherwise it would be to complicated
                if (overlapping.Count == 1 && !overlapping.First().IsFixed && overlapping.First().TimeSlot.From != periodItem.TimeSlot.From && overlapping.First().TimeSlot.To != periodItem.TimeSlot.To)
                {
                    bool startChanged = false;

                    if (periodsBeforeAffectedShift.Count == periodsBeforeAffectedShift.Where(w => !w.IsFixed).Count() && first.TimeSlot.MinFrom <= first.TimeSlot.From.AddMinutes(-(length + this.CalculationOptions.Interval)))
                    {
                        foreach (var periodAfter in periodsAfterAffectedShift)
                            periodAfter.TimeSlot.MoveForward(length);
                        startChanged = true;
                    }
                    else if (!first.IsFixed && first.TimeScheduleTaskId == overlapping.First().TimeScheduleTaskId && first.IncomingDeliveryRowId == overlapping.First().IncomingDeliveryRowId && first.TimeSlot.MinFrom <= first.TimeSlot.From.AddMinutes(-(length + this.CalculationOptions.Interval)))
                    {
                        first.TimeSlot.From = first.TimeSlot.From.AddMinutes(-length);
                        startChanged = true;
                    }

                    if (startChanged)
                    {
                        if (!overlapping.First().OnlyOneEmployee)
                        {
                            var clone = ClonePeriodItem(overlapping.First());
                            periodItems.Remove(overlapping.First());
                            var splitted = SplitShift(overlapping.First(), periodItem.TimeSlot.From, periodItem.TimeSlot.To, discardOnlyOneEmployee: true, setNewGuid: false);

                            if (splitted.Count == 3)
                            {
                                splitted[1].PeriodState = SoeEntityState.Deleted;
                                splitted[0].CalculationGuid = guid;
                                splitted[2].CalculationGuid = guid;
                                periodItems.AddRange(splitted.Where(w => w.PeriodState != SoeEntityState.Deleted));
                                periodItem.CalculationGuid = guid;
                                changes = true;
                            }
                            else
                            {
                                periodItems.Add(clone);
                                foreach (var periodBefore in periodsBeforeAffectedShift)
                                    periodBefore.TimeSlot.MoveForward(length);
                            }
                        }
                    }
                }
            }

            #endregion

            if (!changes)
                return false;

            periodItems = periodItems.Where(w => w.PeriodState != SoeEntityState.Deleted).ToList();

            changesInIteration = changes;

            return changes;
        }

        private void SetGuidAndRowNrOnBestSuitedTimeSlot(CalculationPeriodItem item, Guid guid, int rowNr, List<CalculationPeriodItem> periodItemsOnGuid, List<FreeTimeSlot> timeSlotsOnGuid, bool forward, ref bool changesInIteration)
        {
            //TODO
            //Method to see if periodItemsOnGuid can move if item can not fit directly

            if (!ValidLength(periodItemsOnGuid))
                return;

            foreach (var freeSlot in timeSlotsOnGuid)
            {
                var clone = item.TimeSlot.Clone();

                if (clone.IsFixed)
                {
                    if (Valid(periodItemsOnGuid, item, clone, freeSlot, timeSlotsOnGuid))
                    {
                        item.CalculationGuid = guid;
                        item.CalculationRowNr = rowNr;
                        item.MovedNrOfTimes = item.MovedNrOfTimes + 1;
                        item.TimeSlot = clone;
                        changesInIteration = true;
                        return;
                    }
                }
                else
                {
                    if (Valid(periodItemsOnGuid, item, clone, freeSlot, timeSlotsOnGuid))
                    {
                        var tempClone = ClonePeriodItem(item);
                        tempClone.TimeSlot = clone;

                        item.CalculationGuid = guid;
                        item.CalculationRowNr = rowNr;
                        item.MovedNrOfTimes = item.MovedNrOfTimes + 1;
                        item.TimeSlot = clone;
                        changesInIteration = true;
                        return;
                    }
                    else
                    {
                        AdjustTimeSlot(clone, CalculationOptions.Interval, true, forward);

                        var tempClone = ClonePeriodItem(item);
                        tempClone.TimeSlot = clone;

                        if (Valid(periodItemsOnGuid, tempClone, clone, freeSlot, timeSlotsOnGuid))
                        {
                            item.CalculationGuid = guid;
                            item.CalculationRowNr = rowNr;
                            item.MovedNrOfTimes = item.MovedNrOfTimes + 1;
                            item.TimeSlot = clone;
                            changesInIteration = true;
                            return;
                        }
                        else
                        {
                            bool moved = clone.TrySetStopTime(freeSlot.To);

                            if (moved)
                            {
                                if (CalendarUtility.GetOverlappingMinutes(freeSlot.From, freeSlot.To, clone.From, clone.To) == 0)
                                    moved = false;
                            }

                            if (moved)
                            {
                                if ((clone.To == freeSlot.To || clone.From == freeSlot.From))
                                {
                                    tempClone = ClonePeriodItem(item);
                                    tempClone.TimeSlot = clone;

                                    if (Valid(periodItemsOnGuid, tempClone, clone, freeSlot, timeSlotsOnGuid, checkValidMove: true))
                                    {
                                        item.CalculationGuid = guid;
                                        item.CalculationRowNr = rowNr;
                                        item.MovedNrOfTimes = item.MovedNrOfTimes + 1;
                                        item.TimeSlot = clone;
                                        changesInIteration = true;
                                        return;
                                    }
                                }
                            }

                            moved = clone.TrySetStartTime(freeSlot.From);

                            if (moved)
                            {
                                if (CalendarUtility.GetOverlappingMinutes(freeSlot.From, freeSlot.To, clone.From, clone.To) == 0)
                                    moved = false;
                            }

                            if (moved)
                            {
                                if ((clone.To == freeSlot.To || clone.From == freeSlot.From))
                                {
                                    tempClone = ClonePeriodItem(item);
                                    tempClone.TimeSlot = clone;

                                    if (Valid(periodItemsOnGuid, tempClone, clone, freeSlot, timeSlotsOnGuid, checkValidMove: true))
                                    {
                                        item.CalculationGuid = guid;
                                        item.CalculationRowNr = rowNr;
                                        item.MovedNrOfTimes = item.MovedNrOfTimes + 1;
                                        item.TimeSlot = clone;
                                        changesInIteration = true;
                                        return;
                                    }
                                }
                            }

                            if (this.CalculationOptions.AddOnlyDockedPeriodItems)
                            {
                                int iterations = 24 * 60 / CalculationOptions.Interval;
                                int iteration = 0;

                                while (iteration <= iterations)
                                {
                                    DateTime oldFrom = clone.From;
                                    DateTime oldTo = clone.To;

                                    AdjustTimeSlot(clone, CalculationOptions.Interval, false, forward: forward);

                                    if ((clone.To == freeSlot.To || clone.From == freeSlot.From))
                                    {
                                        tempClone = ClonePeriodItem(item);
                                        tempClone.TimeSlot = clone;

                                        if (Valid(periodItemsOnGuid, tempClone, clone, freeSlot, timeSlotsOnGuid, checkValidMove: true))
                                        {
                                            item.CalculationGuid = guid;
                                            item.CalculationRowNr = rowNr;
                                            item.MovedNrOfTimes = item.MovedNrOfTimes + 1;
                                            item.TimeSlot = clone;
                                            changesInIteration = true;
                                            iteration = iterations;
                                            return;
                                        }
                                    }

                                    iteration++;
                                }
                            }
                        }
                    }
                }
            }
        }

        private bool AllowOnlyDocked(CalculationPeriodItem item)
        {
            if (this.CalculationOptions.AddOnlyDockedPeriodItems)
                return true;

            var head = this.PeriodItemsGroupHeads.Where(h => h.CalculationGuid == item.CalculationGuid).FirstOrDefault();

            if (head != null)
                return AllowOnlyDockedOnHead(head);
            else
                return false;

        }

        private bool Valid(List<CalculationPeriodItem> periodItems, CalculationPeriodItem periodItem, StaffingNeedsCalculationTimeSlot timeSlot, FreeTimeSlot freeTimeSlot, List<FreeTimeSlot> timeSlotsOnGuid, bool checkValidMove = false)
        {
            bool allowOnlyDocked = AllowOnlyDocked(periodItems.FirstOrDefault());

            if (ValidTime(periodItem))
            {
                if (allowOnlyDocked)
                {
                    DateTime maxTimeTo = CalendarUtility.GetEarliestDate(timeSlotsOnGuid.OrderBy(o => o.To).Last().To, this.MaxTimeTo);
                    DateTime minTimeFrom = CalendarUtility.GetLatestDate(timeSlotsOnGuid.OrderBy(o => o.From).First().From, this.MinTimeFrom);

                    if ((maxTimeTo != timeSlot.To && timeSlot.To == freeTimeSlot.To) || (minTimeFrom != timeSlot.From && timeSlot.From == freeTimeSlot.From))
                    {
                        if (checkValidMove && ValidMovePossible(periodItems, periodItem))
                            return (ValidLength(periodItems, periodItem) && CalendarUtility.IsDatesInInterval(timeSlot.From, timeSlot.To, freeTimeSlot.From, freeTimeSlot.To));
                        else if (!checkValidMove)
                            return (ValidLength(periodItems, periodItem) && CalendarUtility.IsDatesInInterval(timeSlot.From, timeSlot.To, freeTimeSlot.From, freeTimeSlot.To));
                        return false;
                    }
                    else
                        return false;
                }
                else
                {
                    if (ValidMovePossible(periodItems, periodItem))
                        return (ValidLength(periodItems, periodItem) && CalendarUtility.IsDatesInInterval(timeSlot.From, timeSlot.To, freeTimeSlot.From, freeTimeSlot.To));
                    return false;
                }
            }
            return false;
        }

        private bool ValidTime(CalculationPeriodItem periodItem)
        {
            if (this.Date.HasValue && this.Date.Value == CalendarUtility.MergeDateAndTime(CalendarUtility.DATETIME_DEFAULT, this.CalculationOptions.FirstPossibleStart, ignoreSqlServerDateTime: true))
            {
                if (periodItem.TimeSlot.From < CalendarUtility.MergeDateAndTime(CalendarUtility.DATETIME_DEFAULT, this.CalculationOptions.FirstPossibleStart, ignoreSqlServerDateTime: true))
                    return false;
            }

            if (this.Date.HasValue && this.Date.Value.Date == CalendarUtility.MergeDateAndTime(CalendarUtility.DATETIME_DEFAULT, this.CalculationOptions.LastPossibleStop, ignoreSqlServerDateTime: true))
            {
                if (periodItem.TimeSlot.To > CalendarUtility.MergeDateAndTime(CalendarUtility.DATETIME_DEFAULT, this.CalculationOptions.LastPossibleStop, ignoreSqlServerDateTime: true))
                    return false;
            }

            if (this.Date.HasValue && this.Date.Value.Date.AddDays(1) == CalendarUtility.MergeDateAndTime(CalendarUtility.DATETIME_DEFAULT, this.CalculationOptions.LastPossibleStop, ignoreSqlServerDateTime: true))
            {
                if (periodItem.TimeSlot.To > CalendarUtility.MergeDateAndTime(CalendarUtility.DATETIME_DEFAULT, this.CalculationOptions.LastPossibleStop, ignoreSqlServerDateTime: true).AddDays(1))
                    return false;
            }

            if (this.Date.HasValue && this.Date.Value.Date == this.CalculationOptions.FirstPossibleStart.Date)
            {
                if (periodItem.TimeSlot.From < CalendarUtility.MergeDateAndTime(CalendarUtility.DATETIME_DEFAULT, this.CalculationOptions.FirstPossibleStart, ignoreSqlServerDateTime: true))
                    return false;
            }

            if (this.Date.HasValue && this.Date.Value.Date == this.CalculationOptions.LastPossibleStop.Date)
            {
                if (periodItem.TimeSlot.To > CalendarUtility.MergeDateAndTime(CalendarUtility.DATETIME_DEFAULT, this.CalculationOptions.LastPossibleStop, ignoreSqlServerDateTime: true))
                    return false;
            }

            if (this.Date.HasValue && this.Date.Value.Date.AddDays(1) == this.CalculationOptions.LastPossibleStop.Date)
            {
                if (periodItem.TimeSlot.To > CalendarUtility.MergeDateAndTime(CalendarUtility.DATETIME_DEFAULT, this.CalculationOptions.LastPossibleStop, ignoreSqlServerDateTime: true).AddDays(1))
                    return false;
            }

            return true;
        }

        public bool ValidMovePossible(List<CalculationPeriodItem> periodItems, CalculationPeriodItem periodItem)
        {
            List<CalculationPeriodItem> newList = periodItems.ToList();
            if (periodItem != null)
                newList.Add(periodItem);
            var periodsBeforeAffectedShift = newList.Where(i => i.TimeSlot.From <= periodItem.TimeSlot.From).OrderBy(o => o.TimeSlot.From);
            var periodsAfterAffectedShift = newList.Where(i => i.TimeSlot.From >= periodItem.TimeSlot.From).OrderBy(o => o.TimeSlot.From);
            bool movePossible = true;
            int breakMinutes = 0;
            var shiftMinutes = newList.Sum(s => s.Length);

            if (periodsBeforeAffectedShift.Where(w => w.IsNetTime).Any())
            {
                foreach (var item in periodsBeforeAffectedShift.Where(w => w.IsNetTime))
                {
                    var minFrom = item.TimeSlot.MinFrom;

                    if (minFrom == item.TimeSlot.From)
                        movePossible = false;

                    bool skipBreakCheck = false;

                    if (movePossible)
                    {
                        if (minFrom < item.TimeSlot.From.AddMinutes(-this.MaxPossibleBreak))
                            skipBreakCheck = true;
                    }

                    //TODO create simple getbreakmethod only for minutes 

                    if (movePossible && !skipBreakCheck)
                    {
                        breakMinutes = GetBreakMinutes(newList);

                        if (minFrom > item.TimeSlot.From.AddMinutes(-breakMinutes))
                            movePossible = false;
                    }
                }
            }

            if (!movePossible && periodsAfterAffectedShift.Any())
            {
                movePossible = true;

                foreach (var item in periodsBeforeAffectedShift.Where(w => w.IsNetTime))
                {

                    if (item.TimeSlot.MaxTo == item.TimeSlot.To)
                        movePossible = false;

                    bool skipBreakCheck = false;

                    if (movePossible)
                    {
                        if (item.TimeSlot.MaxTo > item.TimeSlot.To.AddMinutes(-this.MaxPossibleBreak))
                            skipBreakCheck = true;
                    }

                    if (movePossible && !skipBreakCheck)
                    {
                        breakMinutes = GetBreakMinutes(newList);
                        if (item.TimeSlot.MaxTo < item.TimeSlot.To.AddMinutes(breakMinutes))
                            movePossible = false;
                    }
                }
            }

            return movePossible;
        }

        private bool Valid(List<CalculationPeriodItem> periodItems, StaffingNeedsCalculationTimeSlot timeSlot, FreeTimeSlot freeTimeSlot)
        {
            return (ValidLength(periodItems) && CalendarUtility.IsDatesInInterval(timeSlot.From, timeSlot.To, freeTimeSlot.From, freeTimeSlot.To));
        }

        private bool ValidLength(List<CalculationPeriodItem> periodItems, CalculationPeriodItem periodItem = null)
        {
            if (periodItems != null && periodItems.Any())
            {
                List<CalculationPeriodItem> newList = periodItems.ToList();
                if (periodItem != null)
                    newList.Add(periodItem);

                if (ValidMaxLength(newList.Sum(r => r.Length), IsWeekend(periodItem)))
                    return ContinueOnOverlapping(periodItem);

                if (ValidMaxLength(newList.Sum(r => r.Length) - GetBreakMinutes(newList.Where(w => !w.IsNetTime).ToList()), IsWeekend(periodItem)))
                    return ContinueOnOverlapping(periodItem);
                else
                    return false;
            }
            else
                return false;

        }

        private bool ValidMaxLength(int length, bool isWeekend)
        {
            int maxLength = int.MaxValue;
            if (CalculationOptions.ApplyLengthRule)
                maxLength = isWeekend ? CalculationOptions.MaxLenghtWeekend : CalculationOptions.MaxLength;

            if (length >= maxLength)
                return false;
            else
                return true;
        }

        private bool ContinueOnOverlapping(CalculationPeriodItem periodItem)
        {
            if (periodItem == null)
                return true;
            else
                return this.CurrentEmployeePostCyclesRun.ContinueOnOverlappning(periodItem, this.Date.HasValue ? this.Date.Value : periodItem.ScheduleDate);
        }

        private bool IsWeekend(CalculationPeriodItem periodItem)
        {
            if (periodItem == null)
                return false;

            if (periodItem.Weekday != null && (periodItem.Weekday == DayOfWeek.Sunday || periodItem.Weekday == DayOfWeek.Saturday))
                return true;

            if (periodItem.Date.HasValue && (periodItem.Date.Value.DayOfWeek == DayOfWeek.Sunday || periodItem.Date.Value.DayOfWeek == DayOfWeek.Saturday))
                return true;

            return
                false;
        }

        private List<FreeTimeSlot> GetFreeTimeSlots(List<CalculationPeriodItem> periodItems, DateTime minTimeTimeSlot, DateTime maxTimeTimeSlot, int interval, CalculationPeriodItemGroupByType calculationPeriodItemGroupByType = CalculationPeriodItemGroupByType.Unknown, string key = null, bool ignoreHoles = false)
        {
            List<FreeTimeSlot> allFreeSlots = new List<FreeTimeSlot>();

            var groups = GroupStaffingNeedsCalcutionPeriodItems(periodItems, calculationPeriodItemGroupByType: calculationPeriodItemGroupByType, key: key);

            foreach (var group in groups)
            {
                if (group.CalculationPeriodItems == null)
                    continue;

                List<StaffingNeedsCalculationTimeSlot> calcTimeSlots = group.CalculationPeriodItems.Select(g => g.TimeSlot).ToList();
                List<FreeTimeSlot> freeSlots = new List<FreeTimeSlot>();
                int? shiftTypeId = null;
                if (calculationPeriodItemGroupByType == CalculationPeriodItemGroupByType.ShiftType)
                    shiftTypeId = group.CalculationPeriodItems.FirstOrDefault().ShiftTypeId;

                #region Need is fixed (time within need)

                calcTimeSlots = calcTimeSlots.OrderBy(o => o.From).ThenBy(t => t.To).ToList();
                DateTime currentStart = minTimeTimeSlot;
                for (int shiftNr = 1; shiftNr <= calcTimeSlots.Count; shiftNr++)
                {
                    StaffingNeedsCalculationTimeSlot shiftTimeSlot = calcTimeSlots[shiftNr - 1];
                    if (currentStart < shiftTimeSlot.From)
                        freeSlots.Add(new FreeTimeSlot() { From = currentStart, To = shiftTimeSlot.From });

                    if (shiftNr == calcTimeSlots.Count)
                    {
                        if (maxTimeTimeSlot > shiftTimeSlot.To)
                            freeSlots.Add(new FreeTimeSlot() { From = shiftTimeSlot.To, To = maxTimeTimeSlot });
                    }

                    currentStart = shiftTimeSlot.To;
                }

                #endregion

                foreach (var freeSlot in freeSlots)
                {
                    freeSlot.CalculationRowGuid = group.CalculationPeriodItems.FirstOrDefault().CalculationGuid;
                    if (calculationPeriodItemGroupByType == CalculationPeriodItemGroupByType.ShiftType && shiftTypeId.HasValue)
                        freeSlot.ShiftTypeId = shiftTypeId.Value;
                }

                allFreeSlots.AddRange(freeSlots);
            }

            if (ignoreHoles && allFreeSlots.Count > 2)
            {
                List<FreeTimeSlot> filteredTimeSlots = new List<FreeTimeSlot>();

                filteredTimeSlots.AddRange(allFreeSlots.Take(1));
                filteredTimeSlots.AddRange(allFreeSlots.OrderByDescending(o => o.To).Take(1));
                allFreeSlots = filteredTimeSlots;
            }

            return allFreeSlots;
        }

        #endregion

        #region Grouping

        private List<PeriodItemsGroupHead> GroupStaffingNeedsCalcutionPeriodItems(List<CalculationPeriodItem> periodItems, CalculationPeriodItemGroupByType calculationPeriodItemGroupByType = CalculationPeriodItemGroupByType.Unknown, string key = null, bool hasUniques = false)
        {
            List<PeriodItemsGroupHead> heads = new List<PeriodItemsGroupHead>();

            var periodItemsGroupedByType = periodItems.GroupBy(r => r.GetKey(calculationPeriodItemGroupByType));
            if (!String.IsNullOrEmpty(key))
                periodItemsGroupedByType = periodItemsGroupedByType.Where(g => g.Key.Equals(key)).ToList();

            foreach (var periodItemsGroup in periodItemsGroupedByType)
            {
                PeriodItemsGroupHead head = new PeriodItemsGroupHead()
                {
                    Key = periodItemsGroup.Key,
                    CalculationPeriodItems = periodItemsGroup.ToList(),
                    Done = false
                };
                heads.Add(head);
            }

            if (hasUniques)
                return heads.OrderBy(r => r.CalculationPeriodItems.OrderBy(o => !o.IsUnique(this.CurrentEmployeePostCyclesRun)).ThenBy(sr => sr.TimeSlot.From).FirstOrDefault().TimeSlot.From).ThenByDescending(t => t.Length).ToList();
            else
                return heads.OrderBy(r => r.CalculationPeriodItems.OrderBy(sr => sr.TimeSlot.From).FirstOrDefault().TimeSlot.From).ThenByDescending(t => t.Length).ToList();
        }

        private List<CalculationPeriodItem> GetRowsOnKey(List<CalculationPeriodItem> periodItems, CalculationPeriodItemGroupByType calculationPeriodItemGroupByType, string key)
        {
            return periodItems != null ? periodItems.Where(r => r.GetKey(calculationPeriodItemGroupByType).Equals(key)).ToList() : new List<CalculationPeriodItem>();
        }

        #endregion

        #region StaffingNeedsCalcutionHead

        private bool TryAddStaffingNeedsCalcutionHead(List<CalculationPeriodItem> allPeriodItems, string key, CalculationPeriodItemGroupByType calculationPeriodItemGroupByType = CalculationPeriodItemGroupByType.Unknown, bool forceAddStaffingNeedsCalcutionHead = false, bool setDone = false)
        {
            bool done = true;

            if (allPeriodItems.Count == 0)
                return false;

            #region Check break length

            List<CalculationPeriodItem> groupedPeriodItems = allPeriodItems.Where(r => r.GetKey(calculationPeriodItemGroupByType: calculationPeriodItemGroupByType).Equals(key)).ToList();
            if (groupedPeriodItems.Count == 0)
                return false;

            groupedPeriodItems = groupedPeriodItems.OrderBy(g => g.TimeSlot.From).ToList();

            if (!forceAddStaffingNeedsCalcutionHead)
            {
                int netLength = groupedPeriodItems.Sum(s => s.TimeSlot.Minutes) - GetBreakMinutes(groupedPeriodItems.Where(w => !w.IsNetTime).ToList());

                if (this.CalculationOptions.ApplyLengthRule)
                {
                    if (netLength < decimal.Multiply(this.CalculationOptions.OptimalLength, new decimal(0.85)))
                        done = false;
                }
                else if (netLength < this.CalculationOptions.OptimalLength)
                {
                    done = false;
                }
            }

            #endregion

            Guid guid = groupedPeriodItems.First().CalculationGuid;

            PeriodItemsGroupHead head = new PeriodItemsGroupHead()
            {
                CalculationPeriodItems = groupedPeriodItems,
                CalculationRowGroupByType = calculationPeriodItemGroupByType,
                Done = done,
                Key = key,
            };

            if (forceAddStaffingNeedsCalcutionHead)
                head.Done = true;

            if (setDone)
                head.Done = true;

            if (head.Done && this.CalculationOptions.AddOnlyDockedPeriodItems && head.HasHoles)
                head.Done = false;



            foreach (var item in head.CalculationPeriodItems)
                item.CalculationGuid = guid;

            var existingHead = this.PeriodItemsGroupHeads.Where(s => s.Key == key).FirstOrDefault();

            if (existingHead != null)
                this.PeriodItemsGroupHeads.Remove(existingHead);

            this.PeriodItemsGroupHeads.Add(head);

            return true;
        }

        private void AddPeriodItemsToExistingCalcutionHeadAfterBreaksApplied(List<CalculationPeriodItem> periodItems, string key, CalculationPeriodItemGroupByType calculationPeriodItemGroupByType = CalculationPeriodItemGroupByType.Unknown, Guid? newGuid = null)
        {
            PeriodItemsGroupHead head = this.PeriodItemsGroupHeads.Where(h => h.Key.Equals(key) && h.CalculationGuid.Equals(periodItems.FirstOrDefault().CalculationGuid)).FirstOrDefault();
            if (head != null)
            {
                head.Done = true;
                head.BreaksApplied = true;
                foreach (CalculationPeriodItem row in periodItems)
                {
                    if (!head.CalculationPeriodItems.Contains(row))
                        head.CalculationPeriodItems.Add(row);
                }

                if (newGuid.HasValue)

                {
                    foreach (var item in head.CalculationPeriodItems)
                    {
                        item.CalculationGuid = newGuid.Value;
                    }
                }
            }
        }

        private bool AllowOnlyDockedOnHead(PeriodItemsGroupHead staffingNeedsCalcutionHead)
        {
            if (staffingNeedsCalcutionHead.Done)
                return true;

            #region Over optimalLength

            if (staffingNeedsCalcutionHead.Length > this.CalculationOptions.OptimalLength)
                return true;

            #endregion

            #region Close to OptimalLength

            if (staffingNeedsCalcutionHead.Length > this.CalculationOptions.OptimalLength - (this.CalculationOptions.Interval * 2))
                return true;

            #endregion


            return false;
        }

        private List<CalculationPeriodItem> TryToRemoveAlreadyonHead(List<CalculationPeriodItem> periodItems, CalculationPeriodItemGroupByType calculationPeriodItemGroupByType = CalculationPeriodItemGroupByType.Unknown)
        {
            List<CalculationPeriodItem> newPeriodItems = new List<CalculationPeriodItem>();
            bool alreadyAdded = false;

            foreach (var row in periodItems)
            {
                foreach (var head in this.PeriodItemsGroupHeads)
                {
                    if (head.CalculationPeriodItems.ContainsItem(row))
                        alreadyAdded = true;
                }

                if (!alreadyAdded)
                    newPeriodItems.Add(row);
            }

            return newPeriodItems;
        }

        private List<CalculationPeriodItem> AddPeriodItemsFromHeads(List<CalculationPeriodItem> periodItems, CalculationPeriodItemGroupByType calculationPeriodItemGroupByType = CalculationPeriodItemGroupByType.Unknown)
        {
            foreach (var head in this.PeriodItemsGroupHeads)
            {
                foreach (var row in head.CalculationPeriodItems)
                {
                    if (!periodItems.ContainsItem(row))
                        periodItems.Add(row);
                }
            }

            return periodItems;
        }

        private bool SkipItem(string key, Guid calculationRowGuid)
        {
            return this.PeriodItemsGroupHeads.Any(r => r.Done && r.CalculationGuid.Equals(calculationRowGuid) && r.Key.Equals(key));
        }

        private bool SkipGroup(string key)
        {
            return this.PeriodItemsGroupHeads.Any(r => r.Key.Equals(key) && r.Done);
        }

        #endregion

        #endregion

        #region Breaks

        private TimeBreakTemplateEvaluationOutput EvaluateBreaks(DateTime startTime, ref DateTime stopTime, DateTime? date, DayOfWeek? dayOfWeek, int? shiftTypeId, int? dayTypeId, bool isNetTime, List<TimeBreakTemplateTimeSlot> lockedTimeSlots = null)
        {
            List<int> shiftTypeIds = shiftTypeId.HasValue ? new List<int> { shiftTypeId.Value } : null;
            TimeBreakTemplateEvaluationInput input = GetBreakEvaluationInput(startTime, stopTime, date, dayOfWeek, shiftTypeIds, dayTypeId, lockedTimeSlots: lockedTimeSlots, isNetTime: isNetTime);
            TimeBreakTemplateEvaluationOutput output = this.BreakEvaluation.Evaluate(input, this.TimeBreakTemplateDTOs);
            if (output.Success)
            {
                if (isNetTime && output.AdjustedToGrossStopTime)
                    stopTime = output.StopTime;

                if (output.TimeCodeBreakGroups != null)
                {
                    foreach (TimeCodeBreakGroupDTO timeCodeBreakGroup in output.TimeCodeBreakGroups)
                    {
                        if (this.TimeCodeBreakGroups == null)
                            this.TimeCodeBreakGroups = new List<TimeCodeBreakGroupDTO>();
                        if (!this.TimeCodeBreakGroups.Contains(timeCodeBreakGroup))
                            this.TimeCodeBreakGroups.Add(timeCodeBreakGroup);
                    }
                }
            }
            return output;
        }

        private TimeBreakTemplateEvaluationInput GetBreakEvaluationInput(DateTime startTime, DateTime stopTime, DateTime? date, DayOfWeek? dayOfWeek, List<int> shiftTypeIds, int? dayTypeId, List<TimeBreakTemplateTimeSlot> lockedTimeSlots = null, bool isNetTime = false)
        {
            return new TimeBreakTemplateEvaluationInput(SoeTimeBreakTemplateEvaluation.Automatic, startTime, stopTime, date, shiftTypeIds: shiftTypeIds, dayTypeId: dayTypeId, dayOfWeek: dayOfWeek, lockedTimeSlots: lockedTimeSlots, timeCodeBreakGroups: this.TimeCodeBreakGroups, debugParameters: null, isNetTime: isNetTime);
        }

        private int GetBreakMinutesFromDict(List<CalculationPeriodItem> groupedPeriodItems)
        {
            int breakMinutes = -1;

            if (!this.BreaksIndependentOfTime)
                return int.MinValue;

            var length = groupedPeriodItems.Sum(s => s.Length);

            if (this.BreakMinutesOnLengthDict.TryGetValue(length, out breakMinutes))
                return breakMinutes;

            breakMinutes = GetBreakMinutes(groupedPeriodItems, checkGetBreakMinutesFromDict: false);

            this.BreakMinutesOnLengthDict.Add(length, breakMinutes);

            return breakMinutes;

        }

        private int GetBreakMinutes(List<CalculationPeriodItem> groupedPeriodItems, bool checkGetBreakMinutesFromDict = true)
        {
            if (groupedPeriodItems.Count == 0)
                return 0;

            int breakLength = 0;
            if (CalculationOptions.ApplyLengthRule)
            {
                if (checkGetBreakMinutesFromDict)
                {
                    breakLength = GetBreakMinutesFromDict(groupedPeriodItems);

                    if (breakLength > -1)
                        return breakLength;
                    else
                        breakLength = 0;
                }

                var connectedShifts = GetConnectedShifts(groupedPeriodItems);
                var connectedGroups = connectedShifts.GroupBy(i => i.Item1);
                foreach (var connectedGroup in connectedGroups)
                {
                    List<CalculationPeriodItem> itemsOnPartOfGroup = connectedGroup.Select(t => t.Item2).OrderBy(r => r.TimeSlot.From).ToList();
                    if (itemsOnPartOfGroup.Count == 0)
                        continue;

                    CalculationPeriodItem firstItem = itemsOnPartOfGroup.First();
                    CalculationPeriodItem lastItem = itemsOnPartOfGroup.Last();
                    DateTime startTime = firstItem.TimeSlot.From;
                    DateTime stopTime = lastItem.TimeSlot.To;
                    bool isNetTime = firstItem.IsNetTime && lastItem.IsNetTime;

                    // Get breaks
                    breakLength = GetBreakMinutes(startTime, ref stopTime, firstItem.Date, dayOfWeek: firstItem.Weekday, shiftTypeId: firstItem.ShiftTypeId, dayTypeId: firstItem.DayTypeId, isNetTime: isNetTime);
                }
            }

            return breakLength;
        }

        private int GetBreakMinutes(DateTime startTime, ref DateTime stopTime, DateTime? date, DayOfWeek? dayOfWeek, int? shiftTypeId, int? dayTypeId, bool isNetTime, List<TimeBreakTemplateTimeSlot> lockedTimeSlots = null)
        {
            int breakLength = 0;

            if (Convert.ToDecimal((stopTime - startTime).TotalMinutes) < decimal.Divide(this.CalculationOptions.MinLength, 2))
                return 0;

            TimeBreakTemplateEvaluationOutput breakEvaluationOutput = EvaluateBreaks(startTime, ref stopTime, date, dayOfWeek, shiftTypeId, dayTypeId, isNetTime, lockedTimeSlots: lockedTimeSlots);
            if (breakEvaluationOutput.Success && breakEvaluationOutput.BreakSlots != null && breakEvaluationOutput.BreakSlots.Sum(b => b.Length) > 0)
                breakLength += breakEvaluationOutput.BreakSlots.Sum(b => b.Length);

            return breakLength;
        }

        private int GetBreakMinutesBeforeSplit(CalculationPeriodItem periodItem)
        {
            int length = GetSplitLength(periodItem);

            if (Convert.ToDecimal(length) < decimal.Divide(this.CalculationOptions.MinLength, 2))
                return 0;

            if (length > 0 && length < periodItem.TimeSlot.Minutes)
            {
                CalculationPeriodItem clone = periodItem.Clone();
                clone.TimeSlot = periodItem.TimeSlot.Clone();
                if (clone.TimeSlot.Minutes > length)
                    clone.TimeSlot.To = clone.TimeSlot.From.AddMinutes(length);

                CalculationPeriodItem splitted = SplitShift(clone, length, discardOnlyOneEmployee: true, runRecursive: false, setNewGuid: false).FirstOrDefault();
                int breakMinutes = GetBreakMinutes(new List<CalculationPeriodItem>() { splitted });

                clone = periodItem.Clone();
                clone.TimeSlot = periodItem.TimeSlot.Clone();
                if (clone.TimeSlot.Minutes > length)
                    clone.TimeSlot.To = clone.TimeSlot.From.AddMinutes(length + breakMinutes);
                splitted = SplitShift(clone, length + breakMinutes, discardOnlyOneEmployee: true, runRecursive: false, setNewGuid: false).FirstOrDefault();

                return GetBreakMinutes(new List<CalculationPeriodItem>() { splitted });
            }

            return GetBreakMinutes(new List<CalculationPeriodItem>() { periodItem });
        }

        private int GetBreakMinutesBeforeSplit(int length, DateTime date, DateTime startTime, int? shiftTypeId, DayOfWeek dayOfWeek, bool isNetTime = false)
        {
            StaffingNeedsCalculationTimeSlot timeSlot = new StaffingNeedsCalculationTimeSlot(startTime, startTime.AddMinutes(length), length);

            CalculationPeriodItem item = new CalculationPeriodItem()
            {
                TimeSlot = timeSlot,
                ShiftTypeId = shiftTypeId,
                Date = date
            };

            if (isNetTime)
            {
                timeSlot.MaxTo = timeSlot.MaxTo.AddMinutes(this.CalculationOptions.Interval);
                item.DontAssignBreakLeftovers = true;
            }

            DateTime stopTime = item.TimeSlot.To;

            // Get breaks
            int breakLength = GetBreakMinutes(startTime, ref stopTime, item.Date, dayOfWeek: dayOfWeek, shiftTypeId: item.ShiftTypeId, dayTypeId: item.DayTypeId, isNetTime: isNetTime);

            return breakLength;
        }

        private int GetSplitLength(CalculationPeriodItem periodItem)
        {
            return GetSplitLength(periodItem.TimeSlot.Minutes);
        }

        public int GetSplitLength(int minutes)
        {
            int length = CalculationOptions.OptimalLength;
            if (minutes < CalculationOptions.MaxLength && minutes > CalculationOptions.OptimalLength)
                return -1;

            length = CalculationOptions.OptimalLength;

            return length;
        }

        private List<CalculationPeriodItem> GetBreaks(List<CalculationPeriodItem> groupedPeriodItems)
        {
            List<CalculationPeriodItem> splitPeriodItems = new List<CalculationPeriodItem>();
            List<CalculationPeriodItem> newPeriodItems = new List<CalculationPeriodItem>();

            if (this.NewBreakPeriodItems.Any())
                this.NewBreakPeriodItems = new List<CalculationPeriodItem>();



            var connectedShifts = GetConnectedShifts(groupedPeriodItems);
            foreach (var connectedGroup in connectedShifts.GroupBy(i => i.Item1))
            {
                List<CalculationPeriodItem> connectedPeriodItems = connectedGroup.Select(t => t.Item2).OrderBy(r => r.TimeSlot.From).ToList();
                if (connectedPeriodItems.Count == 0)
                    continue;

                CalculationPeriodItem firstItem = connectedPeriodItems.First();
                CalculationPeriodItem lastItem = connectedPeriodItems.Last();
                DateTime startTime = firstItem.TimeSlot.From;
                DateTime stopTime = lastItem.TimeSlot.To;
                bool isNetTime = connectedPeriodItems.Any(a => a.IsNetTime);

                List<TimeBreakTemplateTimeSlot> lockedTimeSlots = null;
                if (connectedPeriodItems.Where(i => !i.AllowBreaks).Any())
                {
                    lockedTimeSlots = new List<TimeBreakTemplateTimeSlot>();
                    foreach (var item in connectedPeriodItems.Where(i => !i.AllowBreaks && !i.ReplaceWithBreak))
                    {
                        lockedTimeSlots.Add(new TimeBreakTemplateTimeSlot(item.TimeSlot.From, item.TimeSlot.To));
                    }
                }

                var replaceWithBreak = connectedPeriodItems.Where(w => w.ReplaceWithBreak).ToList();
                connectedPeriodItems = connectedPeriodItems.Where(w => !w.ReplaceWithBreak).ToList();
                TimeBreakTemplateEvaluationOutput breakEvaluationOutput = EvaluateBreaks(startTime, ref stopTime, firstItem.Date, dayOfWeek: firstItem.Weekday, shiftTypeId: firstItem.ShiftTypeId, dayTypeId: firstItem.DayTypeId, isNetTime: isNetTime, lockedTimeSlots: lockedTimeSlots);
                if (breakEvaluationOutput.Success && breakEvaluationOutput.BreakSlots != null)
                {
                    #region SplitShifts based on the returned breaks

                    splitPeriodItems.AddRange(SplitBreakLeftOvers(connectedPeriodItems.ToList(), breakEvaluationOutput));

                    foreach (var replaceG in replaceWithBreak.GroupBy(g => g.TimeSlot.From))
                    {
                        var replace = replaceG.First();
                        if (breakEvaluationOutput.BreakSlots.Any(a => a.StartTime == replace.TimeSlot.From && a.StopTime == replace.TimeSlot.To))
                        {
                            replace.ReplaceWithBreak = false;
                            replace.IsBreak = true;
                            replace.TimeCodeBreakGroupId = breakEvaluationOutput.BreakSlots.FirstOrDefault(a => a.StartTime == replace.TimeSlot.From && a.StopTime == replace.TimeSlot.To).TimeCodeBreakGroupId;
                            splitPeriodItems.Add(replace);
                        }
                    }

                    #endregion

                    #region Create new breakPeriodItems with same Guid and Add them to same headRow. Set IsBreak

                    Guid calculationGuid = Guid.NewGuid();

                    if (!firstItem.DontAssignBreakLeftovers && !breakEvaluationOutput.AdjustedToGrossStopTime)
                    {
                        foreach (CalculationPeriodItem breakPeriodItem in splitPeriodItems.Where(r => r.IsBreak))
                        {
                            CalculationPeriodItem newBreakPeriodItem = breakPeriodItem.Clone();
                            newBreakPeriodItem.TimeSlot = breakPeriodItem.TimeSlot.Clone();
                            newBreakPeriodItem.CalculationGuid = calculationGuid;
                            newBreakPeriodItem.IsBreak = false;
                            newBreakPeriodItem.FromBreakRules = true;
                            newPeriodItems.Add(newBreakPeriodItem);
                        }

                        newPeriodItems = MergeBreakLeftOvers(newPeriodItems);

                        foreach (CalculationPeriodItem breakPeriodItem in newPeriodItems)
                        {
                            breakPeriodItem.CalculationGuid = Guid.NewGuid();
                        }

                        this.NewBreakPeriodItems.AddRange(newPeriodItems);
                    }

                    AddTimeBreakInformation(breakEvaluationOutput.BreakSlots, splitPeriodItems.Where(r => r.IsBreak).ToList());

                    #endregion
                }
            }

            if (splitPeriodItems.Count == 0)
                splitPeriodItems.AddRange(groupedPeriodItems);

            return splitPeriodItems;
        }

        private List<CalculationPeriodItem> TryFillUnmergableWithBreaks(List<CalculationPeriodItem> items)
        {
            List<CalculationPeriodItem> newPeriodItems = items;// ClonePeriodItems(items);
            items.ForEach(f => f.OriginalCalculationRowGuid = items.First().OriginalCalculationRowGuid);
            var grouped = items.GroupBy(o => o.OriginalCalculationRowGuid);
            foreach (var group in grouped)
            {
                var connectedPeriodItems = group.OrderBy(o => o.TimeSlot.From);
                CalculationPeriodItem firstItem = connectedPeriodItems.First();
                CalculationPeriodItem lastItem = connectedPeriodItems.Last();
                DateTime startTime = firstItem.TimeSlot.From;
                DateTime stopTime = lastItem.TimeSlot.To;
                bool isNetTime = connectedPeriodItems.Any(a => a.IsNetTime);
                List<TimeBreakTemplateTimeSlot> lockedTimeSlots = connectedPeriodItems.Select(s => new TimeBreakTemplateTimeSlot(s.TimeSlot.From, s.TimeSlot.To)).ToList();

                TimeBreakTemplateEvaluationOutput breakEvaluationOutput = EvaluateBreaks(startTime, ref stopTime, firstItem.Date, dayOfWeek: firstItem.Weekday, shiftTypeId: firstItem.ShiftTypeId, dayTypeId: firstItem.DayTypeId, isNetTime: isNetTime, lockedTimeSlots: lockedTimeSlots);
                if (breakEvaluationOutput.Success && !breakEvaluationOutput.BreakSlots.IsNullOrEmpty())
                {
                    foreach (var breakSlot in breakEvaluationOutput.BreakSlots)
                    {
                        if (connectedPeriodItems.Any(a => a.TimeSlot.From == breakSlot.StopTime))
                        {
                            var prevItem = connectedPeriodItems.FirstOrDefault(f => f.TimeSlot.To == breakSlot.StartTime);
                        }

                        CalculationPeriodItem newBreakPeriodItem = firstItem.Clone();
                        newBreakPeriodItem.TimeSlot = new StaffingNeedsCalculationTimeSlot(breakSlot.StartTime, breakSlot.StopTime, breakSlot.StartTime, breakSlot.StopTime);
                        newBreakPeriodItem.ReplaceWithBreak = true;
                        newPeriodItems.Add(newBreakPeriodItem);
                        AddTimeBreakInformation(breakEvaluationOutput.BreakSlots, newBreakPeriodItem.ObjToList());
                    }
                }
            }

            return newPeriodItems;
        }
        private void AddTimeBreakInformation(List<TimeBreakTemplateBreakSlot> timeBreakTemplateTimeSlots, List<CalculationPeriodItem> newPeriodItems)
        {
            if (this.TimeBreakInformations == null)
                this.TimeBreakInformations = new List<TimeBreakInformation>();

            if (newPeriodItems != null)
            {
                foreach (CalculationPeriodItem newRow in newPeriodItems)
                {
                    TimeBreakInformation information = new TimeBreakInformation(newRow.ShiftTypeId, timeBreakTemplateTimeSlots);
                    if (newRow.TimeSlot != null)
                    {
                        StaffingNeedsCalculationTimeSlot clone = newRow.TimeSlot.Clone();
                        clone.CalculationGuid = newRow.CalculationGuid;
                        information.BreakTimeSlot = clone;
                    }
                    this.TimeBreakInformations.Add(information);
                }
            }
        }

        private List<CalculationPeriodItem> SplitBreakLeftOvers(List<CalculationPeriodItem> periodItems, TimeBreakTemplateEvaluationOutput breakEvaluationOutput)
        {
            DateTime? prevBreakStopTime = null;

            List<TimeBreakTemplateBreakSlot> breakSlots = new List<TimeBreakTemplateBreakSlot>();
            foreach (TimeBreakTemplateBreakSlot breakSlot in breakEvaluationOutput.BreakSlots)
            {
                breakSlots.Add(breakSlot.Clone());
            }

            if (breakEvaluationOutput.AdjustedToGrossStopTime && periodItems != null && periodItems.Count == 1)
            {
                periodItems.LastOrDefault().TimeSlot.MoveToStartOfSlot();

                if (periodItems.LastOrDefault().TimeSlot.To < breakSlots.LastOrDefault().StopTime)
                    periodItems.LastOrDefault().TimeSlot.MoveToEndOfSlot();
            }

            Dictionary<int, bool> breakSlotsChangedDict = new Dictionary<int, bool>();

            for (int breakIndex = 0; breakIndex < breakSlots.Count; breakIndex++)
            {
                TimeBreakTemplateBreakSlot breakSlot = breakSlots[breakIndex];

                //Re evaluate start boundary if previous break was modified and thus boundaries may be obsolete
                if (breakIndex > 0 && breakSlotsChangedDict.ContainsKey(breakIndex - 1))
                {
                    bool isPrevBreakChanged = breakSlotsChangedDict[breakIndex - 1];
                    if (isPrevBreakChanged)
                        breakSlot = this.BreakEvaluation.ReEvaluateStartBoundary(breakEvaluationOutput, breakSlots, breakSlot);
                }

                List<CalculationPeriodItem> periodItemsInInterval = periodItems.Where(r => CalendarUtility.IsTimesOverlappingNew(breakSlot.StartTime, breakSlot.StopTime, r.TimeSlot.From, r.TimeSlot.To)).OrderBy(o => o.TimeSlot.From).ToList();
                if (periodItemsInInterval.Count == 0)
                    continue;

                CalculationPeriodItem periodItem = GetPeriodWithMostBreakTime(breakSlot, periodItemsInInterval);
                if (periodItem == null)
                    continue;

                Guid? calculationGuid = null;
                TimeBreakTemplateBreakSlot breakSlotClone = TryAdjustBreakAfterPrevCreatedBreaks(breakSlot, periodItem, prevBreakStopTime, out calculationGuid);
                if (breakSlotClone == null)
                    continue;

                if (breakEvaluationOutput.AdjustedToGrossStopTime)
                {
                    AdjustNetTimePeriodItems(periodItem, periodItems, breakSlot.Length);
                }

                List<CalculationPeriodItem> splittedPeriodItems = SplitShift(periodItem, breakSlotClone.StartTime, breakSlotClone.StopTime, discardOnlyOneEmployee: true, runRecursive: false, setNewGuid: false);
                if (splittedPeriodItems.Count > 1)
                {
                    splittedPeriodItems = splittedPeriodItems.OrderBy(o => o.TimeSlot.From).ToList();

                    CalculationPeriodItem breakPeriodItem = splittedPeriodItems[1];
                    breakPeriodItem.IsBreak = true;
                    breakPeriodItem.SuggestedTargetCalculationGuid = calculationGuid;
                    breakPeriodItem.TimeCodeBreakGroupId = breakSlot.TimeCodeBreakGroupId;
                    periodItems.Remove(periodItem); // Make sure original is removed since splitshift clones.

                    foreach (CalculationPeriodItem splittedPeriodItem in splittedPeriodItems)
                        periodItems.Add(splittedPeriodItem);
                }

                prevBreakStopTime = breakSlotClone.StopTime;

                bool isBreakChanged = breakSlot.IsCloned && breakSlot.StartTime != breakSlotClone.StartTime || breakSlot.StopTime != breakSlotClone.StopTime;
                breakSlotsChangedDict.Add(breakIndex, isBreakChanged);
                if (isBreakChanged)
                    breakSlots[breakIndex] = breakSlotClone;
            }

            return periodItems;
        }

        private void AdjustNetTimePeriodItems(CalculationPeriodItem periodItem, List<CalculationPeriodItem> periodItems, int breakLength)
        {
            if (periodItem.IsFixed)
                return;

            var periodsBeforeAffectedShift = periodItems.Where(i => i.TimeSlot.From < periodItem.TimeSlot.From).OrderBy(o => o.TimeSlot.From).ToList();
            var periodsAfterAffectedShift = periodItems.Where(i => i.TimeSlot.From > periodItem.TimeSlot.From).OrderBy(o => o.TimeSlot.From).ToList();

            var last = periodsAfterAffectedShift.LastOrDefault();
            var first = periodsBeforeAffectedShift.FirstOrDefault();

            if (periodsAfterAffectedShift.Any() && periodsAfterAffectedShift.Count == periodsAfterAffectedShift.Where(w => !w.TimeSlot.IsFixed).Count() &&
                periodsAfterAffectedShift.Last().TimeSlot.To.AddMinutes(breakLength) <= CalendarUtility.MergeDateAndTime(CalendarUtility.DATETIME_DEFAULT, this.CalculationOptions.LastPossibleStop, ignoreSqlServerDateTime: true) &&
                periodsAfterAffectedShift.Last().TimeSlot.To.AddMinutes(breakLength) <= periodsAfterAffectedShift.Last().TimeSlot.MaxTo)
            {

                periodItem.TimeSlot.To = periodItem.TimeSlot.To.AddMinutes(breakLength);

                foreach (var periodAfter in periodsAfterAffectedShift)
                {
                    periodAfter.TimeSlot.MoveForward(breakLength);
                }
            }
            else if (periodsBeforeAffectedShift.Any() && periodsBeforeAffectedShift.Count == periodsBeforeAffectedShift.Where(w => !w.TimeSlot.IsFixed).Count() &&
                periodsBeforeAffectedShift.First().TimeSlot.From.AddMinutes(-breakLength) >= CalendarUtility.MergeDateAndTime(CalendarUtility.DATETIME_DEFAULT, this.CalculationOptions.FirstPossibleStart, ignoreSqlServerDateTime: true) &&
                periodsBeforeAffectedShift.First().TimeSlot.From.AddMinutes(-breakLength) >= periodsBeforeAffectedShift.First().TimeSlot.MinFrom)
            {
                periodItem.TimeSlot.From = periodItem.TimeSlot.From.AddMinutes(-breakLength);

                foreach (var periodAfter in periodsBeforeAffectedShift)
                {
                    periodAfter.TimeSlot.MoveBackward(breakLength);
                }
            }
            else if (periodsAfterAffectedShift.Count == 0 && periodItem.TimeSlot.To.AddMinutes(breakLength) <= CalendarUtility.MergeDateAndTime(CalendarUtility.DATETIME_DEFAULT, this.CalculationOptions.LastPossibleStop, ignoreSqlServerDateTime: true) && periodItem.TimeSlot.To.AddMinutes(breakLength) <= periodItem.TimeSlot.MaxTo)
                periodItem.TimeSlot.To = periodItem.TimeSlot.To.AddMinutes(breakLength);
            else if (periodsBeforeAffectedShift.Count == 0 && periodItem.TimeSlot.From.AddMinutes(-breakLength) >= CalendarUtility.MergeDateAndTime(CalendarUtility.DATETIME_DEFAULT, this.CalculationOptions.FirstPossibleStart, ignoreSqlServerDateTime: true) && periodItem.TimeSlot.From.AddMinutes(-breakLength) >= periodItem.TimeSlot.MinFrom)
                periodItem.TimeSlot.From = periodItem.TimeSlot.From.AddMinutes(-breakLength);
            else if (periodsAfterAffectedShift.Count == 0 && periodItem.TimeSlot.To.AddMinutes(breakLength) <= periodItem.TimeSlot.MaxTo)
                periodItem.TimeSlot.To = periodItem.TimeSlot.To.AddMinutes(breakLength);
            else if (periodsBeforeAffectedShift.Count == 0 && periodItem.TimeSlot.From.AddMinutes(-breakLength) >= periodItem.TimeSlot.MinFrom)
                periodItem.TimeSlot.From = periodItem.TimeSlot.From.AddMinutes(-breakLength);
            else if (periodsAfterAffectedShift.Count == 0 && periodItem.TimeSlot.To.AddMinutes(breakLength) <= CalendarUtility.MergeDateAndTime(CalendarUtility.DATETIME_DEFAULT, this.CalculationOptions.LastPossibleStop, ignoreSqlServerDateTime: true))
                periodItem.TimeSlot.To = periodItem.TimeSlot.To.AddMinutes(breakLength);
            else if (last != null && !last.IsFixed && last.TimeScheduleTaskId == periodItem.TimeScheduleTaskId && last.IncomingDeliveryRowId == periodItem.IncomingDeliveryRowId && periodsAfterAffectedShift.Where(w => !w.TimeSlot.IsFixed).Any() && last.TimeSlot.To.AddMinutes(breakLength) <= last.TimeSlot.MaxTo)
            {
                if (periodsAfterAffectedShift.Where(w => !w.TimeSlot.IsFixed).Count() == periodsAfterAffectedShift.Count)
                {
                    periodItem.TimeSlot.To = periodItem.TimeSlot.To.AddMinutes(breakLength);

                    foreach (var periodAfter in periodsAfterAffectedShift)
                    {
                        periodAfter.TimeSlot.MoveForward(breakLength);
                    }
                }
                else
                {
                    last.TimeSlot.To = last.TimeSlot.To.AddMinutes(breakLength);
                }
            }
            else if (first != null && !first.IsFixed && first.TimeScheduleTaskId == periodItem.TimeScheduleTaskId && first.IncomingDeliveryRowId == periodItem.IncomingDeliveryRowId && periodsBeforeAffectedShift.Where(w => !w.TimeSlot.IsFixed).Any() && first.TimeSlot.From.AddMinutes(-breakLength) >= first.TimeSlot.MinFrom)
            {
                if (periodsBeforeAffectedShift.Where(w => !w.TimeSlot.IsFixed).Count() == periodsBeforeAffectedShift.Count)
                {
                    periodItem.TimeSlot.From = periodItem.TimeSlot.From.AddMinutes(-breakLength);

                    foreach (var periodAfter in periodsBeforeAffectedShift)
                    {
                        periodAfter.TimeSlot.MoveBackward(breakLength);
                    }
                }
                else
                {
                    first.TimeSlot.From = first.TimeSlot.From.AddMinutes(-breakLength);
                }
            }
            else if (last != null && !last.IsFixed && last.TimeScheduleTaskId == periodItem.TimeScheduleTaskId && last.IncomingDeliveryRowId == periodItem.IncomingDeliveryRowId && periodsAfterAffectedShift.Where(w => !w.TimeSlot.IsFixed).Any() && last.TimeSlot.To.AddMinutes(breakLength) <= CalendarUtility.MergeDateAndTime(CalendarUtility.DATETIME_DEFAULT, this.CalculationOptions.LastPossibleStop, ignoreSqlServerDateTime: true))
            {
                if (periodsAfterAffectedShift.Where(w => !w.TimeSlot.IsFixed).Count() == periodsAfterAffectedShift.Count)
                {
                    periodItem.TimeSlot.To = periodItem.TimeSlot.To.AddMinutes(breakLength);

                    foreach (var periodAfter in periodsAfterAffectedShift)
                    {
                        periodAfter.TimeSlot.MoveForward(breakLength);
                    }
                }
                else
                {
                    last.TimeSlot.To = last.TimeSlot.To.AddMinutes(breakLength);
                }
            }
            else if (first != null && !first.IsFixed && first.TimeScheduleTaskId == periodItem.TimeScheduleTaskId && first.IncomingDeliveryRowId == periodItem.IncomingDeliveryRowId && periodsBeforeAffectedShift.Where(w => !w.TimeSlot.IsFixed).Any() && first.TimeSlot.From.AddMinutes(-breakLength) >= CalendarUtility.MergeDateAndTime(CalendarUtility.DATETIME_DEFAULT, this.CalculationOptions.FirstPossibleStart, ignoreSqlServerDateTime: true))
            {
                if (periodsBeforeAffectedShift.Where(w => !w.TimeSlot.IsFixed).Count() == periodsBeforeAffectedShift.Count())
                {
                    periodItem.TimeSlot.From = periodItem.TimeSlot.From.AddMinutes(-breakLength);

                    foreach (var periodAfter in periodsBeforeAffectedShift)
                    {
                        periodAfter.TimeSlot.MoveBackward(breakLength);
                    }
                }
                else
                {
                    first.TimeSlot.From = first.TimeSlot.From.AddMinutes(-breakLength);
                }
            }
            else if (periodsAfterAffectedShift.Where(w => !w.TimeSlot.IsFixed).Any() && periodsAfterAffectedShift.Where(w => !w.TimeSlot.IsFixed).Count() == periodsAfterAffectedShift.Count() &&
                periodsAfterAffectedShift.Where(w => w.TimeSlot.IsFixed).Count() == 0 && periodsAfterAffectedShift.Last().TimeSlot.To.AddMinutes(breakLength) <= CalendarUtility.MergeDateAndTime(CalendarUtility.DATETIME_DEFAULT, this.CalculationOptions.LastPossibleStop, ignoreSqlServerDateTime: true))
            {
                periodItem.TimeSlot.To = periodItem.TimeSlot.To.AddMinutes(breakLength);

                foreach (var periodAfter in periodsAfterAffectedShift)
                {
                    periodAfter.TimeSlot.MoveForward(breakLength);
                }
            }
            else if (periodsBeforeAffectedShift.Where(w => !w.TimeSlot.IsFixed).Any() && periodsBeforeAffectedShift.Where(w => !w.TimeSlot.IsFixed).Count() == periodsBeforeAffectedShift.Count &&
                    periodsBeforeAffectedShift.Where(w => w.TimeSlot.IsFixed).Count() == 0 && periodsBeforeAffectedShift.First().TimeSlot.From.AddMinutes(-breakLength) >= CalendarUtility.MergeDateAndTime(CalendarUtility.DATETIME_DEFAULT, this.CalculationOptions.FirstPossibleStart, ignoreSqlServerDateTime: true))
            {
                periodItem.TimeSlot.From = periodItem.TimeSlot.From.AddMinutes(-breakLength);

                foreach (var periodAfter in periodsBeforeAffectedShift)
                {
                    periodAfter.TimeSlot.MoveBackward(breakLength);
                }
            }
            else if (periodsAfterAffectedShift.Count == 0 && periodItem.TimeSlot.To.AddMinutes(breakLength) <= this.CalculationOptions.LastPossibleStop)
                periodItem.TimeSlot.To = periodItem.TimeSlot.To.AddMinutes(breakLength);
            else if (periodsBeforeAffectedShift.Count == 0 && periodItem.TimeSlot.From.AddMinutes(-breakLength) >= this.CalculationOptions.FirstPossibleStart)
                periodItem.TimeSlot.From = periodItem.TimeSlot.From.AddMinutes(-breakLength);
            else
            {
                if (periodsAfterAffectedShift.Count == 0)
                    periodItem.TimeSlot.To = periodItem.TimeSlot.To.AddMinutes(breakLength);
            }
        }

        private CalculationPeriodItem GetPeriodWithMostBreakTime(TimeBreakTemplateBreakSlot breakSlot, List<CalculationPeriodItem> periodItems)
        {
            if (breakSlot == null || periodItems == null)
                return null;
            if (periodItems.Count == 1)
                return periodItems.First();

            Dictionary<int, int> indexMinutesDict = new Dictionary<int, int>();
            for (int i = 0; i < periodItems.Count; i++)
            {
                indexMinutesDict.Add(i, CalendarUtility.GetOverlappingMinutes(breakSlot.StartTime, breakSlot.StopTime, periodItems[i].TimeSlot.From, periodItems[i].TimeSlot.To));
            }

            CalculationPeriodItem periodItem = periodItems[indexMinutesDict.OrderByDescending(i => i.Value).First().Key];

            #region if break overlaps periodItem, try to set it to any of the closest if they can handle it.

            if (breakSlot.StartTime < periodItem.TimeSlot.From && breakSlot.StopTime > periodItem.TimeSlot.To)
            {
                var periodAfter = periodItems.Where(i => i.TimeSlot.From == periodItem.TimeSlot.To).FirstOrDefault();

                if (periodAfter != null && periodItem.Length > breakSlot.Length)
                    return periodAfter;

                var periodBefore = periodItems.Where(i => i.TimeSlot.From == periodItem.TimeSlot.To).FirstOrDefault();

                if (periodBefore != null && periodItem.Length > breakSlot.Length)
                    return periodBefore;
            }

            #endregion

            return periodItem;
        }

        private TimeBreakTemplateBreakSlot TryAdjustBreakAfterPrevCreatedBreaks(TimeBreakTemplateBreakSlot breakSlot, CalculationPeriodItem periodItem, DateTime? prevBreakStopTime, out Guid? calculationGuid)
        {
            calculationGuid = null;

            if (breakSlot == null || periodItem == null)
                return null;

            if (!breakSlot.HasBoundaries())
                return breakSlot;

            TimeBreakTemplateBreakSlot breakSlotClone = breakSlot.Clone();

            //Narrow start boundary according to interval setting
            if (prevBreakStopTime.HasValue && prevBreakStopTime.Value == breakSlotClone.BoundaryStartTime)
                breakSlotClone.BoundaryStartTime = prevBreakStopTime.Value.AddMinutes(this.CalculationOptions.Interval);
            if (breakSlotClone.BoundaryStartTime < periodItem.TimeSlot.From)
                breakSlotClone.BoundaryStartTime = periodItem.TimeSlot.From.AddMinutes(CalculationOptions.Interval);
            if (breakSlotClone.BoundaryStopTime > periodItem.TimeSlot.To)
                breakSlotClone.BoundaryStopTime = periodItem.TimeSlot.To.AddMinutes(-CalculationOptions.Interval);

            int minutes = (int)breakSlotClone.BoundaryStartTime.Value.Subtract(breakSlotClone.StartTime).TotalMinutes;
            if (minutes > 0 && !breakSlotClone.TryMoveForward(minutes, TimeBreakTemplateRule.StaffingNeedsCalculation))
                return breakSlot;//Move forward to boundary

            minutes = (int)breakSlotClone.BoundaryStopTime.Value.Subtract(breakSlotClone.StopTime).TotalMinutes;
            if (minutes < 0 && !breakSlotClone.TryMoveBackward(-minutes, TimeBreakTemplateRule.StaffingNeedsCalculation))
                return breakSlot; //Move forward to boundary

            List<TimeBreakTemplateBreakSlot> possibleBreakSlots = CreatePossibleBreakSlots(breakSlotClone, periodItem);
            TimeBreakTemplateBreakSlot newBreakSlot = FindBestPossibleBreakSlot(possibleBreakSlots, periodItem, out calculationGuid);
            return newBreakSlot ?? breakSlot;
        }

        private TimeBreakTemplateBreakSlot FindBestPossibleBreakSlot(List<TimeBreakTemplateBreakSlot> possibleBreakSlots, CalculationPeriodItem periodItem, out Guid? calculationGuid)
        {
            calculationGuid = null;

            if (possibleBreakSlots.IsNullOrEmpty())
                return null;

            if (possibleBreakSlots.Count == 1)
                return possibleBreakSlots.FirstOrDefault();

            possibleBreakSlots = possibleBreakSlots.OrderBy(o => o.StartTime).ToList();
            List<StaffingNeedsCalculationTimeSlot> timeSlots = new List<StaffingNeedsCalculationTimeSlot>();
            timeSlots.AddRange(CreateFakeTimeSlots());
            timeSlots = timeSlots.Where(i => i.CalculationGuid != periodItem.CalculationGuid).ToList();

            int count = possibleBreakSlots.Count;
            if (count > 1)
            {
                DateTime start = possibleBreakSlots.First().StartTime;
                DateTime stop = possibleBreakSlots.Last().StopTime;
                DateTime medianTime = possibleBreakSlots[(int)(count / 2)].Middle;
                timeSlots = timeSlots.Where(w => w.From >= start && w.To <= stop).ToList();
                timeSlots = timeSlots.OrderBy(o => Math.Abs(Convert.ToDecimal(((o.Middle - medianTime).TotalMinutes)))).ToList(); //Middle out
                                                                                                                                  //  timeSlots = timeSlots.Where(o => Math.Abs(Convert.ToDecimal(((o.From - medianTime).TotalMinutes))) < breakMinutes * 6 || Math.Abs(Convert.ToDecimal(((o.To - medianTime).TotalMinutes))) < breakMinutes * 6).ToList(); //Middle out
            }

            TimeBreakTemplateBreakSlot breakSlot = GetFittingBreakSlot(false, timeSlots, periodItem, possibleBreakSlots, out calculationGuid);
            if (breakSlot != null)
                return breakSlot;

            breakSlot = GetFittingBreakSlotOther(false, timeSlots, periodItem, possibleBreakSlots, out calculationGuid);
            if (breakSlot != null)
                return breakSlot;

            breakSlot = GetFittingBreakSlot(true, timeSlots, periodItem, possibleBreakSlots, out calculationGuid);
            if (breakSlot != null)
                return breakSlot;

            breakSlot = GetFittingBreakSlotOther(true, timeSlots, periodItem, possibleBreakSlots, out calculationGuid);
            if (breakSlot != null)
                return breakSlot;

            #region Find closest with same ShiftTypeId

            if (breakSlot == null && timeSlots.Count > 0)
            {
                var reakSlotSuggestedTime = possibleBreakSlots.Where(w => w.StartTime == timeSlots.First().From).FirstOrDefault();
                if (reakSlotSuggestedTime != null)
                {
                    calculationGuid = timeSlots.First().CalculationGuid;
                    return reakSlotSuggestedTime;
                }
            }

            #endregion

            // Return middle
            return possibleBreakSlots[(int)(count / 2)];
        }

        private TimeBreakTemplateBreakSlot GetFittingBreakSlot(bool allowOverlapping, List<StaffingNeedsCalculationTimeSlot> timeSlots, CalculationPeriodItem periodItem, List<TimeBreakTemplateBreakSlot> possibleBreakSlots, out Guid? calculationGuid)
        {
            foreach (StaffingNeedsCalculationTimeSlot periodItemBreakTimeSlot in timeSlots.Where(p => p.ShiftTypeId == periodItem.ShiftTypeId))
            {

                #region Same ShiftTypeId that docks

                List<TimeBreakTemplateBreakSlot> breakSlotDocksLeft = possibleBreakSlots.OrderByDescending(o => o.Length).Where(w => w.StopTime == periodItemBreakTimeSlot.From).ToList();

                foreach (var left in breakSlotDocksLeft)
                {
                    //Other Breaks
                    if (!allowOverlapping && timeSlots.Any(t => t.IsBreak && t.ShiftTypeId == periodItem.ShiftTypeId && (t.From == left.StartTime || t.To == left.StopTime)))
                        continue;

                    calculationGuid = periodItemBreakTimeSlot.CalculationGuid;
                    return left;
                }

                List<TimeBreakTemplateBreakSlot> breakSlotDocksRight = possibleBreakSlots.OrderByDescending(o => o.Length).Where(w => w.StartTime == periodItemBreakTimeSlot.To).ToList();
                foreach (var right in breakSlotDocksRight)
                {
                    #region Other Breaks

                    if (!allowOverlapping)
                    {
                        var overlappingTimeSlots = timeSlots.Where(t => t.IsBreak && t.ShiftTypeId == periodItem.ShiftTypeId && (t.From == right.StartTime || t.To == right.StopTime));

                        if (overlappingTimeSlots.Any())
                            continue;
                    }

                    #endregion

                    calculationGuid = periodItemBreakTimeSlot.CalculationGuid;
                    return right;
                }

                #endregion
            }
            calculationGuid = null;
            return null;
        }

        private TimeBreakTemplateBreakSlot GetFittingBreakSlotOther(bool allowOverlapping, List<StaffingNeedsCalculationTimeSlot> timeSlots, CalculationPeriodItem periodItem, List<TimeBreakTemplateBreakSlot> possibleBreakSlots, out Guid? calculationGuid)
        {
            foreach (StaffingNeedsCalculationTimeSlot periodItemBreakTimeSlot in timeSlots.Where(p => p.ShiftTypeId != periodItem.ShiftTypeId))
            {
                //if (!allowOverlapping)
                //{
                //    var overlappingTimeSlots = timeSlots.Where(t => t.IsBreak && t.ShiftTypeId != periodItem.ShiftTypeId && (t.From == periodItemBreakTimeSlot.From || t.To == periodItemBreakTimeSlot.To));

                //    if (overlappingTimeSlots.Any())
                //        continue;
                //}

                #region All that docks

                List<TimeBreakTemplateBreakSlot> breakSlotDocksLeftAll = possibleBreakSlots.OrderByDescending(o => o.Length).Where(w => w.StopTime == periodItemBreakTimeSlot.From).ToList();

                foreach (var left in breakSlotDocksLeftAll)
                {
                    #region Other Breaks

                    if (!allowOverlapping)
                    {
                        var overlappingTimeSlots = timeSlots.Where(t => t.IsBreak && t.ShiftTypeId != periodItem.ShiftTypeId && (t.From == left.StartTime || t.To == left.StopTime));

                        if (overlappingTimeSlots.Any())
                            continue;
                    }

                    #endregion

                    if (left.ValidateBoundary(left.StartTime, left.StopTime))
                    {
                        calculationGuid = periodItemBreakTimeSlot.CalculationGuid;
                        return left;
                    }
                }

                List<TimeBreakTemplateBreakSlot> breakSlotDocksRightAll = possibleBreakSlots.OrderByDescending(o => o.Length).Where(w => w.StartTime == periodItemBreakTimeSlot.To).ToList();

                foreach (var right in breakSlotDocksLeftAll)
                {
                    #region Other Breaks

                    if (!allowOverlapping)
                    {
                        var overlappingTimeSlots = timeSlots.Where(t => t.IsBreak && t.ShiftTypeId != periodItem.ShiftTypeId && (t.From == right.StartTime || t.To == right.StopTime));

                        if (overlappingTimeSlots.Any())
                            continue;
                    }

                    #endregion
                    if (right.ValidateBoundary(right.StartTime, right.StopTime))
                    {
                        calculationGuid = periodItemBreakTimeSlot.CalculationGuid;
                        return right;
                    }
                }

                #endregion
            }

            calculationGuid = null;
            return null;
        }

        private List<TimeBreakTemplateBreakSlot> CreatePossibleBreakSlots(TimeBreakTemplateBreakSlot breakSlot, CalculationPeriodItem periodItem)
        {
            List<TimeBreakTemplateBreakSlot> possibleBreakSlots = new List<TimeBreakTemplateBreakSlot>()
            {
                breakSlot
            };

            if (!breakSlot.HasBoundaries())
                return possibleBreakSlots;

            //List<StaffingNeedsCalculationTimeSlot> generatedBreakTimeSlots = GetGeneratedBreakTimeSlots();

            DateTime currentTime = breakSlot.StartTime.AddMinutes(-CalculationOptions.Interval);
            DateTime boundaryStartTime = periodItem.TimeSlot.From.AddMinutes(CalculationOptions.Interval) > breakSlot.BoundaryStartTime.Value ? periodItem.TimeSlot.From.AddMinutes(CalculationOptions.Interval) : breakSlot.BoundaryStartTime.Value;
            while (currentTime > boundaryStartTime)
            {
                TimeBreakTemplateBreakSlot breakSlotClone = breakSlot.Clone();
                breakSlotClone.StartTime = currentTime;
                breakSlotClone.StopTime = currentTime.AddMinutes(breakSlot.Length);

                //List<StaffingNeedsCalculationTimeSlot> overlappingBreakTimeSlots = generatedBreakTimeSlots.Where(s => CalendarUtility.IsDatesOverlapping(breakSlotClone.StartTime, breakSlotClone.StopTime, s.From, s.To)).ToList();
                //if (overlappingBreakTimeSlots.Count == 0)
                possibleBreakSlots.Add(breakSlotClone);

                currentTime = currentTime.AddMinutes(-CalculationOptions.Interval);
            }

            currentTime = breakSlot.StartTime.AddMinutes(CalculationOptions.Interval);
            DateTime boundaryStopTime = periodItem.TimeSlot.To.AddMinutes(-breakSlot.Length) < breakSlot.BoundaryStopTime.Value ? periodItem.TimeSlot.To.AddMinutes(-breakSlot.Length) : breakSlot.BoundaryStopTime.Value.AddMinutes(-breakSlot.Length);
            while (currentTime <= boundaryStopTime)
            {
                TimeBreakTemplateBreakSlot breakSlotClone = breakSlot.Clone();
                breakSlotClone.StartTime = currentTime;
                breakSlotClone.StopTime = currentTime.AddMinutes(breakSlot.Length);

                //List<StaffingNeedsCalculationTimeSlot> overlappingBreakTimeSlots = generatedBreakTimeSlots.Where(s => CalendarUtility.IsDatesOverlapping(breakSlotClone.StartTime, breakSlotClone.StopTime, s.From, s.To)).ToList();
                //if (overlappingBreakTimeSlots.Count == 0)
                possibleBreakSlots.Add(breakSlotClone);

                currentTime = currentTime.AddMinutes(CalculationOptions.Interval);
            }

            return possibleBreakSlots;
        }

        private List<CalculationPeriodItem> MergeBreakLeftOvers(List<CalculationPeriodItem> fromBreakRows)
        {
            return MergeShifts(fromBreakRows, allowAdjustment: true);
        }

        #endregion

        #endregion

        private void LogInfo(string message)
        {
            if (this.ConnectedToDatabase)
                Task.Run(() => CompEntitiesProvider.RunWithTaskScopedReadOnlyEntities(() => LogCollector.LogCollector.LogInfo(message)));
        }

        private string GetText(int sysTermId, string defaultTerm)
        {
            if (!this.ConnectedToDatabase)
                return defaultTerm;
            else
                return new TermManager(null).GetText(sysTermId, 1, defaultTerm);
        }
    }

    public class StaffingNeedsCalculationInput
    {
        #region Public properties

        public DateTime? Date { get; set; }
        public DateTime? StartDate { get; set; }
        public int? Days { get; set; }
        public int? DayTypeId { get; set; }
        public DayOfWeek? DayOfWeek { get; set; }
        public int? ShiftTypeId { get; set; }
        public string Name { get; set; }
        public List<IncomingDeliveryHeadDTO> IncomingDeliveryHeads { get; set; }
        public List<StaffingNeedsRowDTO> StaffingNeedsRows { get; set; }
        public List<TimeScheduleTaskDTO> TimeScheduleTasks { get; set; }
        public List<EmployeePostDTO> EmployeePosts { get; set; }
        public List<ShiftTypeDTO> ShiftTypes { get; set; }
        public List<StaffingNeedsAnalysisChartData> StaffingNeedsAnalysisChartData { get; set; }
        public List<StaffingNeedsLocationGroupDTO> StaffingNeedsLocationGroups { get; set; }
        public CalculationOptions CalculationOptions { get; set; }

        public bool? ForceIncludeTimeScheduleTaskItems { get; set; }

        #endregion

        #region Ctor

        public StaffingNeedsCalculationInput(DateTime? date, int? dayTypeId, DayOfWeek? dayOfWeek, string name, List<StaffingNeedsRowDTO> rows, List<IncomingDeliveryHeadDTO> heads, List<TimeScheduleTaskDTO> timeScheduleTasks, List<ShiftTypeDTO> shiftTypeDTOs, List<EmployeePostDTO> employeePosts, int interval, bool applyLengthRule, DateTime? startDate = null, int? days = null, List<ShiftTypeDTO> shiftTypeDTO = null)
        {
            this.Date = date;
            this.StartDate = StartDate;
            this.Days = days;
            this.DayTypeId = dayTypeId;
            this.DayOfWeek = dayOfWeek;
            this.Name = name;
            this.IncomingDeliveryHeads = heads;
            this.StaffingNeedsRows = rows;
            this.TimeScheduleTasks = timeScheduleTasks;
            this.EmployeePosts = employeePosts;
            this.ShiftTypes = shiftTypeDTOs;
            this.StaffingNeedsAnalysisChartData = null;
            this.StaffingNeedsLocationGroups = null;
            this.CalculationOptions = new CalculationOptions(interval, applyLengthRule);
        }

        public StaffingNeedsCalculationInput(int interval, List<TimeScheduleTaskDTO> timeScheduleTaskDTOs = null, List<IncomingDeliveryHeadDTO> incomingDeliveryHeadDTOs = null, List<StaffingNeedsAnalysisChartData> staffingNeedsAnalysisChartData = null, List<ShiftTypeDTO> shiftTypeDTOs = null, int? shiftTypeId = null, List<StaffingNeedsLocationGroupDTO> staffingNeedsLocationGroupDTOs = null)
        {
            this.StaffingNeedsRows = new List<StaffingNeedsRowDTO>();
            this.Date = null;
            this.DayTypeId = null;
            this.DayOfWeek = null;
            this.Name = string.Empty;
            this.ShiftTypes = shiftTypeDTOs;
            this.TimeScheduleTasks = timeScheduleTaskDTOs;
            this.StaffingNeedsAnalysisChartData = staffingNeedsAnalysisChartData;
            this.IncomingDeliveryHeads = incomingDeliveryHeadDTOs;
            this.EmployeePosts = EmployeePosts;
            this.StaffingNeedsLocationGroups = staffingNeedsLocationGroupDTOs;
            this.Days = null;
            this.StartDate = StartDate;
            this.ShiftTypeId = shiftTypeId;

            CalculationOptions options = new CalculationOptions(interval);
            options.ApplyLengthRule = false;
            this.CalculationOptions = options;
        }

        #endregion

        #region Public methods

        #endregion
    }

    public class EmployeePostCalculationInfo
    {
        #region Properties

        public EmployeePostSortType EmployeePostSortType { get; set; }
        public int ActorCompanyId { get; set; }
        public List<StaffingNeedsHeadDTO> NeedHeads { get; set; }
        public List<StaffingNeedsHeadDTO> ShiftHeads { get; set; }
        public List<TimeSchedulePlanningDayDTO> Shifts { get; set; }
        public List<EmployeePostDTO> SelectedEmployeePosts { get; set; }
        public List<EmployeePostDTO> AllEmployeePosts { get; set; }
        public List<ShiftTypeDTO> ShiftTypes { get; set; }
        public int TimeCodeId { get; set; }
        public DateTime StartDate { get; set; }
        public int Interval { get; set; }
        public List<TimeScheduleTaskDTO> TimeScheduleTasks { get; set; }
        public List<IncomingDeliveryHeadDTO> IncomingDeliverys { get; set; }
        public List<EmployeeGroupDTO> EmployeeGroups { get; set; }
        public List<CalculationPeriodItem> NeedPeriodItems { get; set; }
        public List<EmployeePostSort> EmployeePostSort { get; set; }
        public List<List<int>> PreviousFirstEmployeePostSorts { get; set; }
        public int Number { get; set; }


        #endregion

        #region Ctor

        public EmployeePostCalculationInfo(
                    EmployeePostSortType employeePostSortType, int actorCompanyId, List<StaffingNeedsHeadDTO> needHeads, List<StaffingNeedsHeadDTO> shiftHeads, List<TimeSchedulePlanningDayDTO> shifts,
                    List<EmployeePostDTO> selectedEmployeePosts, List<EmployeePostDTO> allEmployeePosts, List<ShiftTypeDTO> shiftTypes, int timeCodeId, DateTime startDate, int interval,
                    List<TimeScheduleTaskDTO> timeScheduleTasks = null, List<IncomingDeliveryHeadDTO> incomingDeliverys = null, List<EmployeeGroupDTO> employeeGroups = null, List<List<int>> previousFirstEmployeePostSorts = null)
        {
            this.EmployeePostSortType = employeePostSortType;
            this.ActorCompanyId = actorCompanyId;
            this.NeedHeads = needHeads;
            this.ShiftHeads = shiftHeads;
            this.Shifts = shifts;
            this.SelectedEmployeePosts = selectedEmployeePosts;
            this.AllEmployeePosts = allEmployeePosts;
            this.ShiftTypes = shiftTypes;
            this.TimeCodeId = timeCodeId;
            this.StartDate = startDate;
            this.Interval = interval;
            this.TimeScheduleTasks = timeScheduleTasks;
            this.IncomingDeliverys = incomingDeliverys;
            this.EmployeeGroups = employeeGroups;
            this.NeedPeriodItems = new List<CalculationPeriodItem>();
            this.EmployeePostSort = new List<EmployeePostSort>();
            this.PreviousFirstEmployeePostSorts = previousFirstEmployeePostSorts;
        }

        #endregion
    }
    public class EmployeePostCalculationOutput
    {
        #region Public properties

        public List<EmployeePostCycle> EmployeePostCycles { get; set; }
        public List<int> FirstEmployeePostSort { get; set; }
        public List<List<int>> PreviousFirstEmployeePostSorts { get; set; }
        public PreAnalysisInformation PreAnalysisInformation { get; set; }

        public decimal Percent
        {
            get
            {
                var workWeekTime = 0;

                foreach (var cycle in this.EmployeePostCycles)
                {
                    foreach (var week in cycle.EmployeePostWeeks.Where(w => w.Length > 0))
                    {
                        if (cycle.EmployeePost.ValidShiftTypes != null && cycle.EmployeePost.ValidShiftTypes.Any())
                            workWeekTime += cycle.EmployeePost.WorkTimeWeek;
                    }
                }

                if (workWeekTime > 0)
                    return ((decimal.Divide((workWeekTime - RemainingMinutes), workWeekTime)) * 100);
                else
                    return 0;
            }
        }

        public bool MeetPercentGoal
        {
            get
            {
                var percentLimit = Convert.ToInt32(100 - (this.EmployeePostCycles.Count * 2));
                return percentLimit < 75 ? this.Percent > 75 : this.Percent > percentLimit;
            }
        }

        public int RemainingMinutes
        {
            get
            {
                int remainingMinutes = 0;
                foreach (var cycle in this.EmployeePostCycles.Where(i => i.RemainingMinutes > 0))
                {
                    if (cycle.EmployeePost.ValidShiftTypes != null && cycle.EmployeePost.ValidShiftTypes.Any())
                        remainingMinutes += cycle.RemainingMinutes;
                }

                return remainingMinutes;
            }
        }

        #endregion

        #region Ctor

        public EmployeePostCalculationOutput(List<EmployeePostCycle> employeePostCycles, PreAnalysisInformation preAnalysisInformation)
        {
            this.EmployeePostCycles = employeePostCycles;
            this.FirstEmployeePostSort = new List<int>();
            this.PreviousFirstEmployeePostSorts = new List<List<int>>();
            this.PreAnalysisInformation = preAnalysisInformation;
        }

        #endregion

        #region Public methods

        public List<EmployeePostSort> GetEmployeePostsWithRemainingTime()
        {
            List<EmployeePostSort> employeePostSorts = new List<EmployeePostSort>();

            foreach (var cycle in this.EmployeePostCycles.Where(i => i.RemainingMinutes > 0))
            {
                employeePostSorts.Add(new EmployeePostSort(cycle.EmployeePost, remainingMinutes: cycle.RemainingMinutes));
            }

            return employeePostSorts;
        }



        #endregion
    }

    public class EmployeePostCyclesRun
    {
        #region Properties

        public EmployeePostCyclesRunInformation EmployeePostCyclesRunInformation { get; set; }


        public List<EmployeePostCycle> EmployeePostCycles { get; set; }

        public List<int> UniqueShiftypeIds { get; set; }

        public List<int> PrioShiftTypesIds { get; set; }

        public List<EmployeePostDTO> AllEmployeePosts { get; set; }

        public List<EmployeePostDTO> SelectedEmployeePosts { get; set; }
        public Guid Guid { get; }
        public List<int> FirstEmployeePostSort { get; set; }
        public List<List<int>> PreviousFirstEmployeePostSorts { get; set; }

        public int Number { get; set; }

        public int RemainingMinutes
        {
            get
            {
                int remainingMinutes = 0;

                foreach (var cycle in this.EmployeePostCycles.Where(i => !i.FromSavedData))
                    remainingMinutes += cycle.RemainingMinutes;

                return remainingMinutes;

            }
        }

        public bool HasEmptyCycles
        {
            get
            {
                foreach (var cycles in this.EmployeePostCycles)
                {
                    if (cycles.EmployeePostWeeks.Sum(w => w.Length) == 0)
                        return true;
                }

                return false;
            }
        }

        public int NumberOfNotSaved
        {
            get
            {
                return this.EmployeePostCycles.Where(i => !i.FromSavedData).Count();
            }
        }

        public decimal Percent
        {
            get
            {
                var workWeekTime = 0;

                foreach (var cycle in this.EmployeePostCycles)
                {
                    foreach (var week in cycle.EmployeePostWeeks.Where(w => w.Length > 0))
                    {
                        workWeekTime += cycle.EmployeePost.WorkTimeWeek;
                    }
                }

                if (workWeekTime > 0)
                    return ((decimal.Divide((workWeekTime - this.RemainingMinutes), workWeekTime)) * 100);
                else
                    return 0;
            }
        }

        public bool MeetPercentGoal
        {
            get
            {
                return this.Percent > 80;
            }
        }

        public bool DisposeThis
        {
            get
            {
                return this.Percent < 60;
            }
        }

        #endregion

        #region ctor

        public EmployeePostCyclesRun(EmployeePostSortType employeePostSortType, List<EmployeePostDTO> allEmployeePosts, List<EmployeePostDTO> selectedEmployeePosts)
        {
            this.EmployeePostCycles = new List<EmployeePostCycle>();
            this.UniqueShiftypeIds = new List<int>();
            this.EmployeePostCyclesRunInformation = new EmployeePostCyclesRunInformation(employeePostSortType);
            this.FirstEmployeePostSort = new List<int>();
            this.PreviousFirstEmployeePostSorts = new List<List<int>>();
            this.AllEmployeePosts = allEmployeePosts;
            this.SelectedEmployeePosts = selectedEmployeePosts;
            this.Guid = Guid.NewGuid();
        }

        #endregion

        #region Methods

        public bool OrderAlreadyUsed(List<EmployeePostSort> employeePostSort)
        {
            if (this.PreviousFirstEmployeePostSorts.Count == 0)
                return false;

            List<int> employeePostSortIds = employeePostSort.Select(s => s.EmployeePost.EmployeePostId).ToList();

            foreach (var sort in this.PreviousFirstEmployeePostSorts)
            {
                if (!Enumerable.SequenceEqual(sort, employeePostSortIds))
                    return true;
            }

            return false;
        }

        public bool IsUnique(CalculationPeriodItem periodItem)
        {
            if (this.UniqueShiftypeIds.Count > 0)
            {
                if (!periodItem.ShiftTypeId.HasValue)
                    return false;

                return this.UniqueShiftypeIds.Contains(periodItem.ShiftTypeId.Value);
            }

            return false;
        }

        public List<CalculationPeriodItem> GetItemsOnlyEmployeePostCanTake(List<CalculationPeriodItem> needPeriodItems, int currentEmployeePostId)
        {
            List<CalculationPeriodItem> filteredPeriodItems = new List<CalculationPeriodItem>();
            List<EmployeePostDTO> remainingEmployeePosts = GetRemainingAllEmployeePosts().Where(e => e.EmployeePostId != currentEmployeePostId).ToList();

            if (remainingEmployeePosts.Count > decimal.Multiply(AllEmployeePosts.Count, 0.20m))
            {
                foreach (var periodItem in needPeriodItems)
                {
                    bool matched = false;
                    foreach (var EmployeePost in remainingEmployeePosts)
                    {
                        if (EmployeePost.SkillMatch(periodItem.ShiftType))
                        {
                            matched = true;
                            break;
                        }
                    }

                    if (!matched)
                        filteredPeriodItems.Add(periodItem);
                }
            }
            else
            {
                foreach (var periodItem in needPeriodItems)
                {
                    if (!IsUnique(periodItem))
                        continue;

                    bool matched = false;
                    foreach (var EmployeePost in remainingEmployeePosts)
                    {
                        if (EmployeePost.SkillMatch(periodItem.ShiftType))
                        {
                            matched = true;
                            break;
                        }
                    }

                    if (!matched)
                        filteredPeriodItems.Add(periodItem);
                }
            }

            return filteredPeriodItems;
        }

        public List<DayOfWeek> GetWeekDayOrderBasedOnPreviousWeek(EmployeePostCycle employeePostCycle, int currentEmployeeWeekNr, List<DayOfWeek> orderOfWeekDays, bool isWeekendWeek, ref bool adjusted)
        {
            List<DayOfWeek> basedOnSKills = orderOfWeekDays;
            var all = CalendarUtility.GetWeekDaysList();

            var prevWeek = employeePostCycle.GetPreviousEmployeeWeek(currentEmployeeWeekNr);

            if (prevWeek != null)
            {
                adjusted = true;
                List<DayOfWeek> newList = new List<DayOfWeek>();
                List<DayOfWeek> fromPrevWeekOrder = prevWeek.EmployeePostDays.Select(s => s.DayOfWeek).ToList();

                if (!isWeekendWeek)
                    newList = fromPrevWeekOrder.Where(w => w != DayOfWeek.Sunday && w != DayOfWeek.Saturday).ToList();
                else
                {
                    newList = fromPrevWeekOrder;

                    if (newList.Contains(DayOfWeek.Friday))
                    {
                        newList.Remove(DayOfWeek.Friday);
                        newList.Insert(0, DayOfWeek.Friday);
                    }

                    if (!newList.Contains(DayOfWeek.Sunday))
                        newList.Insert(0, DayOfWeek.Sunday);

                    if (!newList.Contains(DayOfWeek.Saturday))
                        newList.Insert(0, DayOfWeek.Saturday);
                }

                foreach (var dayOfWeek in basedOnSKills)
                {
                    if (!newList.Any(a => a == dayOfWeek))
                        newList.Add(dayOfWeek);
                }

                foreach (var dayOfWeek in all)
                {
                    if (!newList.Any(a => a == dayOfWeek))
                        newList.Add(dayOfWeek);
                }

                return newList;
            }
            else
            {
                adjusted = false;
                return basedOnSKills;
            }
        }

        public List<DayOfWeek> GetWeekDayOrderBasedOnSkillsOfRemainingEmployeePosts(List<Tuple<DateTime, List<CalculationPeriodItem>>> needPeriodItems, int currentEmployeePostId, List<DayOfWeek> orderOfWeekDays, bool forceNotWeekEndWeek, bool isWeekendWeek, ref bool adjusted)
        {
            List<DayOfWeek> allDayOfWeeks = new List<DayOfWeek>();
            List<DayOfWeek> dayOfWeekOrder = new List<DayOfWeek>();
            List<CalculationPeriodItem> items = new List<CalculationPeriodItem>();

            foreach (var item in needPeriodItems)
                items.AddRange(item.Item2);

            foreach (var item in GetItemsOnlyEmployeePostCanTake(items, currentEmployeePostId))
                allDayOfWeeks.Add(item.Weekday.Value);

            if (allDayOfWeeks.Count == 0)
                return new List<DayOfWeek>();

            adjusted = true;

            var groups = allDayOfWeeks.GroupBy(g => g).OrderByDescending(u => u.Count()).ThenBy(d => orderOfWeekDays.IndexOf(d.First()));

            foreach (var day in groups)
                dayOfWeekOrder.Add(day.FirstOrDefault());

            foreach (var day in orderOfWeekDays)
            {
                if (!dayOfWeekOrder.Contains(day))
                    dayOfWeekOrder.Add(day);
            }

            if (forceNotWeekEndWeek || !isWeekendWeek)
            {
                dayOfWeekOrder = dayOfWeekOrder.Where(w => w != DayOfWeek.Sunday && w != DayOfWeek.Saturday).ToList();
                dayOfWeekOrder.Add(DayOfWeek.Saturday);
                dayOfWeekOrder.Add(DayOfWeek.Sunday);
            }

            return dayOfWeekOrder;
        }

        public bool HasNextWeekendPrioShiftTypes(List<Tuple<DateTime, List<CalculationPeriodItem>>> needPeriodItems, int currentEmployeePostId)
        {
            List<CalculationPeriodItem> items = new List<CalculationPeriodItem>();
            needPeriodItems = needPeriodItems.Where(i => i.Item1.DayOfWeek == DayOfWeek.Saturday || i.Item1.DayOfWeek == DayOfWeek.Sunday).ToList();
            foreach (var item in needPeriodItems)
                items.AddRange(item.Item2);

            foreach (var item in GetItemsOnlyEmployeePostCanTake(items, currentEmployeePostId))
                return true;

            return false;

        }

        public bool ContinueOnOverlappning(CalculationPeriodItem periodItem, DateTime date)
        {
            if (periodItem.AllowOverlapping || periodItem.MinSplitLength == 0)
                return true;

            if (periodItem.OnlyOneEmployee)
                return true;

            if (periodItem.IsBreak)
                return true;

            foreach (var item in GetPeriodItemsNotOverlapping(date, periodItem.TimeScheduleTaskKey, periodItem.IncomingDeliveryRowKey))
            {
                if (CalendarUtility.GetOverlappingMinutes(item.TimeSlot.From, item.TimeSlot.To, periodItem.TimeSlot.From, periodItem.TimeSlot.To) > 0)
                    return false;
            }

            return true;
        }

        private List<CalculationPeriodItem> GetPeriodItemsNotOverlapping(DateTime date, string timeScheduleTaskKey, string incomingDeliveryRowKey)
        {
            if (string.IsNullOrEmpty(timeScheduleTaskKey) && string.IsNullOrEmpty(incomingDeliveryRowKey))
                return new List<CalculationPeriodItem>();

            var periodItems = GetPeriodItems(date).Where(p => !p.AllowOverlapping).ToList();

            if (!string.IsNullOrEmpty(timeScheduleTaskKey))
                return periodItems.Where(p => p.TimeScheduleTaskKey == timeScheduleTaskKey).ToList();

            if (!string.IsNullOrEmpty(incomingDeliveryRowKey))
                return periodItems.Where(p => p.IncomingDeliveryRowKey == incomingDeliveryRowKey).ToList();

            return new List<CalculationPeriodItem>();
        }

        private Dictionary<DateTime, List<CalculationPeriodItem>> otherPreviousPeriodItem { get; set; }

        public void ClearOtherPreviousPeriodItem()
        {
            otherPreviousPeriodItem = new Dictionary<DateTime, List<CalculationPeriodItem>>();
        }

        public List<CalculationPeriodItem> GetPeriodItems(DateTime date)
        {
            List<CalculationPeriodItem> periodItems = new List<CalculationPeriodItem>();

            if (otherPreviousPeriodItem != null && otherPreviousPeriodItem.ContainsKey(date))
            {
                var match = otherPreviousPeriodItem.FirstOrDefault(w => w.Key == date);

                if (match.Value != null)
                    return match.Value;
            }

            foreach (var cycle in EmployeePostCycles)
            {
                periodItems.AddRange(cycle.GetEmployeePostDay(date).SelectedItemsHead.CalculationPeriodItems);
            }

            if (periodItems.Any())
            {
                if (!otherPreviousPeriodItem.ContainsKey(date))
                    otherPreviousPeriodItem.Add(date, periodItems);
            }

            return periodItems;
        }

        public List<EmployeePostDTO> GetRemainingAllEmployeePosts()
        {
            return this.AllEmployeePosts.Where(e => !this.EmployeePostCycles.Select(s => s.EmployeePost.EmployeePostId).Contains(e.EmployeePostId)).ToList();
        }

        public List<EmployeePostDTO> GetRemainingSelectedEmployeePosts()
        {
            return this.AllEmployeePosts.Where(e => !this.EmployeePostCycles.Select(s => s.EmployeePost.EmployeePostId).Contains(e.EmployeePostId)).ToList();
        }

        public int NumberOfPossibleEmployeePostsLeftOneShiftType(ShiftTypeDTO shiftType)
        {
            int numberOfPossible = 0;

            foreach (var employeePost in this.GetRemainingAllEmployeePosts())
            {
                if (employeePost.SkillMatch(shiftType))
                    numberOfPossible++;
            }

            return numberOfPossible;
        }

        #endregion
    }

    public class EmployeePostWeekDayOfWeekOrder
    {
        public EmployeePostWeekDayOfWeekOrder(List<DayOfWeek> dayOrWeeksOrder, EmployeePostWeekDayOfWeekOrderType type = EmployeePostWeekDayOfWeekOrderType.Unknown)
        {
            DayOrWeeksOrder = dayOrWeeksOrder;
            Type = type;
        }
        public EmployeePostWeekDayOfWeekOrder()
        {
            DayOrWeeksOrder = new List<DayOfWeek>();
        }

        public EmployeePostWeekDayOfWeekOrderType Type { get; set; }
        public List<DayOfWeek> DayOrWeeksOrder { get; set; }
    }

    public enum EmployeePostWeekDayOfWeekOrderType
    {
        Unknown = 0,
        BasedOnPreviousWeek = 1,
        BasedOnRemainingSkills = 2,
    }

    public class EmployeePostCycle
    {
        #region Properties


        public PreAnalysisInformation PreAnalysisInformation { get; set; }

        public bool FromSavedData { get; set; }

        public List<EmployeePostWeek> EmployeePostWeeks { get; set; }
        public EmployeePostDTO EmployeePost { get; set; }
        public List<ScheduleCycleRuleTypeDTO> AllScheduleCycleRuleTypeDTOs { get; set; }
        public List<TimeSchedulePlanningDayDTO> Shifts { get; set; }

        public bool NoValidItems { get; set; }

        public EmployeePostCycleInformation EmployeePostCycleInformation { get; set; }
        public int NbrOfWorkDays
        {
            get
            {
                return this.EmployeePost.ScheduleCycleDTO.NbrOfWeeks * this.EmployeePost.WorkDaysWeek;
            }
        }
        public int NbrOfWorkedDays
        {
            get
            {
                return GetEmployeePostDays().Where(i => i.Length > 0).Count();
            }
        }
        public int RemainingMinutes
        {
            get
            {
                int remainingMinutes = 0;

                foreach (var week in this.EmployeePostWeeks)
                    remainingMinutes = remainingMinutes + week.RemainingMinutesWeek;

                return remainingMinutes;
            }
        }

        public int Length
        {
            get
            {
                int length = 0;

                foreach (var week in this.EmployeePostWeeks)
                    length += week.Length;

                return length;
            }
        }

        public int RemainingMinutesWeekFromPreviousWeek
        {
            get
            {
                int remainingMinutes = 0;

                foreach (var week in this.EmployeePostWeeks)
                    remainingMinutes = remainingMinutes + week.RemainingMinutesWeekFromPreviousWeek;

                return remainingMinutes;
            }
        }

        public int RemainingMinutesWeekInclPreviousWeek
        {
            get
            {
                int remainingMinutes = 0;

                foreach (var week in this.EmployeePostWeeks)
                    remainingMinutes = remainingMinutes + week.RemainingMinutesWeekInclPreviousWeek;

                return remainingMinutes;
            }
        }

        public int RemainingDays
        {
            get
            {
                int remainingDays = 0;

                foreach (var week in this.EmployeePostWeeks)
                    remainingDays = remainingDays + week.RemainingDaysWeek;

                return remainingDays;
            }
        }

        public int WorkTimeWeekMax
        {
            get
            {
                if (this.EmployeePost.WorkTimeWeek == this.EmployeePost.EmployeeGroupDTO.RuleWorkTimeWeek)
                    return this.EmployeePost.EmployeeGroupDTO.RuleWorkTimeWeek + Math.Abs(this.EmployeePost.EmployeeGroupDTO.MaxScheduleTimeFullTime);
                else
                    return this.EmployeePost.WorkTimeWeek + Math.Abs(this.EmployeePost.EmployeeGroupDTO.MaxScheduleTimePartTime);
            }
        }

        public int WorkTimeWeekMin
        {
            get
            {
                if (this.EmployeePost.WorkTimeWeek == this.EmployeePost.EmployeeGroupDTO.RuleWorkTimeWeek)
                    return this.EmployeePost.EmployeeGroupDTO.RuleWorkTimeWeek - Math.Abs(this.EmployeePost.EmployeeGroupDTO.MinScheduleTimeFullTime);
                else
                    return this.EmployeePost.WorkTimeWeek - Math.Abs(this.EmployeePost.EmployeeGroupDTO.MinScheduleTimePartTime);
            }
        }

        public decimal Percent
        {
            get
            {
                var workWeekTime = 0;

                foreach (var week in this.EmployeePostWeeks)
                    workWeekTime += this.EmployeePost.WorkTimeWeek;

                if (workWeekTime > 0)
                    return ((decimal.Divide((workWeekTime - this.RemainingMinutes), workWeekTime)) * 100);
                else
                    return 0;
            }
        }

        public bool MeetPercentGoal
        {
            get
            {
                return this.Percent < 95;
            }
        }

        public bool DisposeThis
        {
            get
            {
                return this.Percent < 70;
            }
        }

        public DateTime StartDate
        {
            get { return this.EmployeePostWeeks.OrderBy(o => o.StartDate).FirstOrDefault().StartDate; }
        }

        public DateTime StopDate
        {
            get { return CalendarUtility.GetLastDateOfWeek(this.EmployeePostWeeks.OrderByDescending(o => o.StartDate).FirstOrDefault().StartDate); }
        }

        public int NbrOfDays
        {
            get { return this.EmployeePostWeeks.Count * 7; }
        }

        public List<DayOfWeek> FreeDays { get; set; }

        public bool IgnoreFreeDays { get; set; }

        public bool UseFreeDaysOnDayNoneWeekendWeeks
        {
            get
            {
                return !EmployeePost.DayOfWeekIds.IsNullOrEmpty() && this.EmployeePost.DayOfWeekIds.Count > 2 && this.EmployeePost.DayOfWeekIds.Contains((int)DayOfWeek.Sunday) && this.EmployeePost.DayOfWeekIds.Contains((int)DayOfWeek.Saturday);
            }
        }

        public bool TryOnlyShiftsFromPreviousWeeks { get; set; }

        public bool FocusOnShiftsFromPreviousWeeks { get; set; }


        public bool OnlyForcePreferredLenghtOnLastWeek { get; set; }

        public List<EmployeePostWeek> AttemptedWeeks { get; set; }

        public List<CalculationPeriodItem> InitialValidItems { get; set; }

        #endregion

        #region Ctor

        public EmployeePostCycle()
        {
            this.OnlyForcePreferredLenghtOnLastWeek = true;
            this.PreAnalysisInformation = new PreAnalysisInformation();
        }

        public EmployeePostCycle(EmployeePostDTO employeePost, List<ScheduleCycleRuleDTO> allScheduleCycleRuleDTOs)
        {
            this.EmployeePostWeeks = new List<EmployeePostWeek>();
            this.Shifts = new List<TimeSchedulePlanningDayDTO>();
            this.FreeDays = new List<DayOfWeek>();
            this.EmployeePost = new EmployeePostDTO();
            this.AttemptedWeeks = new List<EmployeePostWeek>();
            this.InitialValidItems = new List<CalculationPeriodItem>();
            this.EmployeePost = employeePost;
            this.EmployeePostCycleInformation = new EmployeePostCycleInformation(this.EmployeePost.EmployeePostId, this.EmployeePost.Name);
            this.PreAnalysisInformation = new PreAnalysisInformation();
            this.AllScheduleCycleRuleTypeDTOs = new List<ScheduleCycleRuleTypeDTO>();
            if (!allScheduleCycleRuleDTOs.IsNullOrEmpty())
                foreach (var item in allScheduleCycleRuleDTOs.Where(w => w.MinOccurrences > 0).Select(s => s.ScheduleCycleRuleTypeDTO).GroupBy(g => g.ScheduleCycleRuleTypeId))
                    this.AllScheduleCycleRuleTypeDTOs.Add(item.First());

        }

        public EmployeePostCycle(EmployeePostDTO employeePost, DateTime startDate, CalculationOptions calculationOptions, List<ScheduleCycleRuleDTO> allScheduleCycleRuleDTOs)
        {
            int nbrOfWeeks = calculationOptions?.EmployeePost?.ScheduleCycleDTO?.NbrOfWeeks ?? 1;
            this.EmployeePostWeeks = new List<EmployeePostWeek>();
            this.Shifts = new List<TimeSchedulePlanningDayDTO>();
            this.EmployeePost = calculationOptions.EmployeePost;
            this.FreeDays = new List<DayOfWeek>();
            this.AttemptedWeeks = new List<EmployeePostWeek>();
            this.InitialValidItems = new List<CalculationPeriodItem>();
            this.EmployeePost = employeePost;
            this.EmployeePostCycleInformation = new EmployeePostCycleInformation(this.EmployeePost.EmployeePostId, this.EmployeePost.Name);
            this.PreAnalysisInformation = new PreAnalysisInformation();
            this.AllScheduleCycleRuleTypeDTOs = new List<ScheduleCycleRuleTypeDTO>();
            if (!allScheduleCycleRuleDTOs.IsNullOrEmpty())
                foreach (var item in allScheduleCycleRuleDTOs.Where(w => w.MinOccurrences > 0).Select(s => s.ScheduleCycleRuleTypeDTO).GroupBy(g => g.ScheduleCycleRuleTypeId))
                    this.AllScheduleCycleRuleTypeDTOs.Add(item.First());

            for (int i = 0; i < nbrOfWeeks; i++)
            {
                this.EmployeePostWeeks.Add(new EmployeePostWeek(i + 1, startDate.AddDays(i * 7).Date, new List<EmployeePostDay>(), this.EmployeePost, calculationOptions));
            }
        }

        #endregion

        #region Public methods

        #region Calculate
        public void SetFreeDays(bool ignoreFreeDays, int weekNumber, bool addOneOnNoneWeekEndDays)
        {
            var currentWeek = GetEmployeePostWeek(weekNumber);
            bool weekWithAddedExtraDayOfWork = currentWeek != null ? currentWeek.WeekWithAddedExtraDayOfWork(this) : false;

            //TODO Ta hänsyn till om personen redan har ledig dag, lägg i anslutning till denna.
            if (this.EmployeePostWeeks.Count > 0)
            {
                List<DayOfWeek> validDays = CalendarUtility.GetWeekDaysList().Take(5).ToList();
                List<DayOfWeek> filteredValidDays = new List<DayOfWeek>();
                List<DayOfWeek> freeDays = new List<DayOfWeek>();

                if (!this.IgnoreFreeDays || UseFreeDaysOnDayNoneWeekendWeeks)
                    freeDays = this.EmployeePost.DayOfWeekIds.Select(w => (DayOfWeek)w).ToList();

                if (freeDays.Count >= 2)
                {
                    this.FreeDays = freeDays;
                }
                else if (validDays.Count > 1)
                {
                    if (this.EmployeePost.FreeDays.Where(w => w == DayOfWeek.Saturday || w == DayOfWeek.Sunday).Count() == 2)
                        filteredValidDays = this.EmployeePost.FreeDays;
                    else
                    {
                        if (validDays.Contains(DayOfWeek.Tuesday))
                        {
                            filteredValidDays.Add(DayOfWeek.Tuesday);
                        }
                    }

                    if (!weekWithAddedExtraDayOfWork && filteredValidDays.Count < 2 && validDays.Contains(DayOfWeek.Wednesday))
                    {
                        filteredValidDays.Add(DayOfWeek.Wednesday);
                    }

                    if (filteredValidDays.Count < 2)
                    {
                        if (freeDays.Count > 0)
                            filteredValidDays.Add((DayOfWeek)((int)freeDays.FirstOrDefault() + 1));
                    }

                    if (filteredValidDays.Count < 2)
                    {
                        filteredValidDays.Add(validDays.Where(f => !filteredValidDays.Contains(f)).FirstOrDefault());
                    }


                    this.FreeDays = filteredValidDays;
                }
                else
                {
                    this.FreeDays = validDays;
                }

                if (EmployeePost.ScheduleCycleDTO.OnlyHasOneWeekEndDayPerWeek(EmployeePost))
                {
                    var maxFreeDays = 7 - EmployeePost.WorkDaysWeek;

                    if (this.FreeDays.Count > maxFreeDays - 1) //minus the other weekend day
                    {
                        this.FreeDays = this.FreeDays.Take(maxFreeDays - 1).ToList();
                    }
                }
            }

            if (ignoreFreeDays && weekNumber > 0)
                EmployeePost.OverWriteDayOfWeekIds = this.FreeDays.Select(s => (int)s).ToList();

            if (weekNumber != 0 && this.FreeDays.Count > 1 && weekWithAddedExtraDayOfWork && this.EmployeePost.WorkDaysWeek == 5)
                this.FreeDays = this.FreeDays.Take(1).ToList();

            foreach (var week in this.EmployeePostWeeks)
            {
                if (weekNumber == 0)
                {
                    if (IsWeekendWeek(week.WeekNumber, ignoreFreeDays, UseFreeDaysOnDayNoneWeekendWeeks))
                    {
                        week.IsWeekendWeek = true;
                        week.FreeDays = this.FreeDays;
                    }
                }
                else if (weekNumber == week.WeekNumber)
                {
                    if (!IsWeekendWeek(week.WeekNumber, ignoreFreeDays) && UseFreeDaysOnDayNoneWeekendWeeks)
                    {
                        week.IsWeekendWeek = true;
                        week.FreeDays = this.FreeDays;
                    }
                    else if (IsWeekendWeek(week.WeekNumber, ignoreFreeDays))
                    {
                        week.IsWeekendWeek = true;
                        week.FreeDays = this.FreeDays;
                    }
                    else if (addOneOnNoneWeekEndDays && !this.FreeDays.IsNullOrEmpty())
                        week.FreeDays = this.EmployeePost.DayOfWeekIds.Where(w => w != (int)DayOfWeek.Sunday && w != (int)DayOfWeek.Saturday).Take(1).Select(s => (DayOfWeek)s).ToList();
                }
            }
        }

        public bool IsEvenWeek(int weekNumber)
        {
            EmployeePostWeek week = GetEmployeePostWeek(weekNumber);

            return week.IsEvenWeek;
        }

        public List<DayOfWeek> GetValidFreeDays(bool ignoreFreeDays)
        {
            List<DayOfWeek> validDays = new List<DayOfWeek>();

            foreach (var day in EnumUtility.GetValues<DayOfWeek>())
                if (this.EmployeePost.DayOfWeekValid(day, ignoreFreeDays) && day != DayOfWeek.Sunday && day != DayOfWeek.Saturday)
                    validDays.Add(day);

            return validDays;
        }

        public bool ForcePreferredDayLength(int weekNumber, DayOfWeek dayOfWeek)
        {
            if (this.OnlyForcePreferredLenghtOnLastWeek && weekNumber != this.EmployeePostWeeks.Count)
                return false;

            if (this.EmployeePostWeeks.Count == weekNumber && this.RemainingMinutesWeekInclPreviousWeek - this.RemainingMinutes > (this.EmployeePost.WorkTimeWeek * 0.2))
                return true;

            int internalCycle = GetInternalCycle();

            if (internalCycle != 0 && internalCycle < this.EmployeePostWeeks.Count)
            {
                if (weekNumber % internalCycle == 0)
                {
                    var week = GetEmployeePostWeek(weekNumber);
                    return (week.GetNumberOfPossibleWorkDaysLeftInWeek(dayOfWeek, this) <= this.EmployeePost.WorkDaysWeek * 0.6);
                }
            }

            return this.RemainingDays <= 2;

            // return ((this.RemainingDays < 10) && (this.EmployeePostWeeks.Count == weekNumber || this.RemainingMinutesWeekFromPreviousWeek > Convert.ToInt32(decimal.Divide(this.WorkTimeWeekMin, this.EmployeePost.WorkDaysWeek))) && this.RemainingDays <= this.NbrOfWorkDays * 0.4);
        }

        public int GetInternalCycle()
        {
            int internalCycle = this.EmployeePostWeeks.Count;

            if (this.EmployeePostWeeks.Count > 0)
            {
                int numberOfWeekends = this.EmployeePost.ScheduleCycleDTO.GetNumberOfWeekEnds();

                if (numberOfWeekends > 0 && numberOfWeekends < internalCycle)
                    internalCycle = Convert.ToInt32(decimal.Round(decimal.Divide(this.EmployeePostWeeks.Count, numberOfWeekends), 0));

                if (internalCycle <= 0 || internalCycle > this.EmployeePostWeeks.Count)
                    internalCycle = this.EmployeePostWeeks.Count;
            }

            return internalCycle;
        }

        public bool PreviousWeekWasWeekendWeek(int weekNumber, bool? isWeekendWeek = null)
        {
            if (weekNumber > 1)
            {
                DateTime date = this.EmployeePostWeeks.FirstOrDefault(w => w.WeekNumber == weekNumber).StartDate;
                EmployeePostWeek prevWeek = GetPreviousEmployeePostWeek(date);

                return prevWeek.WeekEndDays() > 0;
            }

            return isWeekendWeek.HasValue ? !isWeekendWeek.Value : false;
        }

        public int RemainingWeekEnds(int currentWeekNumber)
        {
            int currentRemainingWeeks = EmployeePostWeeks.Count - currentWeekNumber;
            int numberOfWeekends = 0;

            while (currentRemainingWeeks > 0)
            {
                if (NextWeekIsWeekendWeek(currentWeekNumber))
                    numberOfWeekends++;

                currentWeekNumber++;
            }

            return numberOfWeekends;
        }

        public bool NextWeekIsWeekendWeek(int currentWeekNumber)
        {
            if (this.EmployeePostWeeks.Count != currentWeekNumber)
            {
                return IsWeekendWeek(currentWeekNumber + 1, false, true);
            }
            else if (this.EmployeePostWeeks.Count > 1 && this.EmployeePostWeeks.Count == currentWeekNumber)
            {
                return false;
                //var weekOne = GetEmployeePostWeek(1);
                //return weekOne != null && weekOne.EmployeePostDays.Where(w => w.DayOfWeek == DayOfWeek.Sunday || w.DayOfWeek == DayOfWeek.Saturday).Count > 0 ? true : false;
            }
            return false;
        }

        public bool WillReachMaxDaysInARow(DateTime currentDate)
        {
            int maxDays = 7; //TODO Setting
            var prevShift = GetPreviousShifts(CalendarUtility.AdjustDateToEndOfWeek(currentDate));

            if (prevShift.Count == 0)
                return false;

            var freeDayBefore = currentDate.AddDays(-(maxDays + 1));
            var freeDayAfter = currentDate.AddDays(maxDays + 1);
            DateTime checkDate = currentDate;

            while (checkDate > freeDayBefore)
            {
                checkDate = checkDate.AddDays(-1);

                if (!prevShift.Any(a => a.StartTime.Date == checkDate))
                {
                    freeDayBefore = checkDate;
                    break;
                }
            }

            checkDate = currentDate;

            while (checkDate < freeDayAfter)
            {
                checkDate = checkDate.AddDays(+1);

                if (!prevShift.Any(a => a.StartTime.Date == checkDate))
                {
                    freeDayAfter = checkDate;
                    break;
                }
            }

            if (((freeDayAfter - freeDayBefore).TotalDays - 2) >= maxDays)
                return true;
            else
                return false;

        }

        public bool ForceNotWeekEndWeek(int weekNumber)
        {
            bool valid = weekNumber == 1;

            if (!valid)
                return valid;

            return (this.EmployeePost.EmployeePostWeekendType == TermGroup_EmployeePostWeekendType.PreferOddWeekWeekend && this.EmployeePostWeeks[weekNumber - 1].IsEvenWeek) || (this.EmployeePost.EmployeePostWeekendType == TermGroup_EmployeePostWeekendType.PreferEvenWeekWeekend && !this.EmployeePostWeeks[weekNumber - 1].IsEvenWeek);
        }

        public bool IsWeekendWeek(int weekNumber, bool ignoreFreeDays, bool ignorePreSetCheck = false)
        {
            if (!EmployeePost.ScheduleCycleDTO.HasWeekends())
                return false;

            if (ForceNotWeekEndWeek(weekNumber))
                return false;

            if (this.EmployeePost.FreeDays.Where(w => w == DayOfWeek.Saturday || w == DayOfWeek.Sunday).Count() == 2)
                return false;

            if (this.EmployeePost.DayOfWeekValid(DayOfWeek.Sunday, ignoreFreeDays) || this.EmployeePost.DayOfWeekValid(DayOfWeek.Saturday, ignoreFreeDays))
            {
                if (!ignorePreSetCheck && this.PreSetDayOfWeekOrder?.DayOrWeeksOrder != null && this.PreSetDayOfWeekOrder.DayOrWeeksOrder.FirstOrDefault() == DayOfWeek.Monday)
                    return false;

                DateTime date = this.EmployeePostWeeks.FirstOrDefault(w => w.WeekNumber == weekNumber).StartDate.Date;

                if (this.EmployeePostWeeks.Count >= 4)
                {
                    var prevWeek = GetPreviousEmployeePostWeek(date);

                    if (prevWeek != null)
                    {
                        if (prevWeek.EmployeePostDays.Where(i => i.DayOfWeek == DayOfWeek.Sunday || i.DayOfWeek == DayOfWeek.Saturday).Any())
                            return false;

                        if (this.GetScheduleCycleRuleTypes().Count == 1)
                            return true;
                    }
                }

                var previousShifts = GetPreviousShifts(date.Date);
                var typesSunday = this.GetAvailableScheduleCycleRuleTypes(DayOfWeek.Sunday, previousShifts);
                var typesSaturday = this.GetAvailableScheduleCycleRuleTypes(DayOfWeek.Saturday, previousShifts);

                if (typesSunday.Count > 0 || typesSaturday.Count > 0)
                {
                    if (weekNumber != 1)
                    {
                        if (typesSaturday.Where(t => !t.IsWeekEndOnly).Any() && typesSunday.Where(t => !t.IsWeekEndOnly).Any())
                        {
                            if (previousShifts.Where(w => w.DayOfWeek == DayOfWeek.Sunday || w.DayOfWeek == DayOfWeek.Saturday).Count() > 1)
                            {
                                return false;
                            }
                        }

                    }
                    return true;
                }

            }
            return false;
        }

        private List<EmployeePostWeekDayOfWeekOrder> preSetDayOfWeekOrders { get; set; }
        public List<EmployeePostWeekDayOfWeekOrder> PreSetDayOfWeekOrders
        {
            get
            {
                return this.preSetDayOfWeekOrders;
            }
        }

        private EmployeePostWeekDayOfWeekOrder PreSetDayOfWeekOrder { get; set; }

        public void SetPreSetWeekDayOrders(List<EmployeePostWeekDayOfWeekOrder> dayOfWeekOrders)
        {
            this.preSetDayOfWeekOrders = new List<EmployeePostWeekDayOfWeekOrder>();

            foreach (var dayOfWeekOrder in dayOfWeekOrders)
            {
                if (OrderAlreadyUsed(dayOfWeekOrder))
                    continue;

                this.preSetDayOfWeekOrders.Add(dayOfWeekOrder);
            }
        }



        private bool OrderAlreadyUsed(EmployeePostWeekDayOfWeekOrder dayOfWeekOrder)
        {
            if (this.preSetDayOfWeekOrders.Count == 0)
                return false;

            foreach (var sort in this.preSetDayOfWeekOrders)
            {
                if (Enumerable.SequenceEqual(sort.DayOrWeeksOrder, dayOfWeekOrder.DayOrWeeksOrder))
                    return true;
            }

            return false;
        }

        public void RemovePreSetWeekDayOrders()
        {
            this.PreSetDayOfWeekOrder = new EmployeePostWeekDayOfWeekOrder();
            this.preSetDayOfWeekOrders = new List<EmployeePostWeekDayOfWeekOrder>();
        }

        public void SetPreSetWeekDayOrder(int index)
        {
            try
            {
                this.preSetDayOfWeekOrders = this.preSetDayOfWeekOrders.Where(w => w.DayOrWeeksOrder.Any()).ToList();

                this.PreSetDayOfWeekOrder = new EmployeePostWeekDayOfWeekOrder();
                if (this.preSetDayOfWeekOrders.Count >= index + 1)
                    this.PreSetDayOfWeekOrder = preSetDayOfWeekOrders[index];

            }
            catch
            {
                // Ignore exception
                // NOSONAR
            }

        }

        public EmployeePostWeekDayOfWeekOrder GetWeekDayOrder(int weekNumber, bool ignoreFreeDays)
        {
            EmployeePostWeekDayOfWeekOrder order = new EmployeePostWeekDayOfWeekOrder();

            if (this.PreSetDayOfWeekOrder != null && this.PreSetDayOfWeekOrder.DayOrWeeksOrder.Any() && this.PreSetDayOfWeekOrder.DayOrWeeksOrder.Count <= 7)
            {
                order = PreSetDayOfWeekOrder;
            }
            else
            {
                List<DayOfWeek> dayOfWeeks = new List<DayOfWeek>();
                if (this.IsWeekendWeek(weekNumber, ignoreFreeDays))
                {
                    dayOfWeeks.Add(DayOfWeek.Saturday);
                    dayOfWeeks.Add(DayOfWeek.Sunday);
                    dayOfWeeks.Add(DayOfWeek.Friday);
                    dayOfWeeks.Add(DayOfWeek.Thursday);
                    dayOfWeeks.Add(DayOfWeek.Monday);
                    dayOfWeeks.Add(DayOfWeek.Tuesday);
                    dayOfWeeks.Add(DayOfWeek.Wednesday);
                }
                else
                {
                    dayOfWeeks.Add(DayOfWeek.Monday);
                    dayOfWeeks.Add(DayOfWeek.Tuesday);
                    dayOfWeeks.Add(DayOfWeek.Wednesday);
                    dayOfWeeks.Add(DayOfWeek.Thursday);
                    dayOfWeeks.Add(DayOfWeek.Friday);
                    dayOfWeeks.Add(DayOfWeek.Saturday);
                    dayOfWeeks.Add(DayOfWeek.Sunday);
                }

                order = new EmployeePostWeekDayOfWeekOrder(dayOfWeeks);
            }

            var week = this.GetEmployeePostWeek(weekNumber);
            week.DayOfWeekOrder = order.DayOrWeeksOrder;

            return order;
        }

        public EmployeePostWeek GetPreviousEmployeeWeek(int currentEmployeeWeekNr)
        {
            return this.EmployeePostWeeks.FirstOrDefault(f => f.WeekNumber == currentEmployeeWeekNr - 1);
        }

        public List<ScheduleRuleEvaluationItem> GetPreviousShifts(DateTime startDate, DateTime? stopDate = null)
        {
            if (startDate == DateTime.MinValue)
                return new List<ScheduleRuleEvaluationItem>();

            if (!stopDate.HasValue)
                stopDate = DateTime.MaxValue;

            List<ScheduleRuleEvaluationItem> previousShifts = new List<ScheduleRuleEvaluationItem>();

            List<EmployeePostWeek> previousWeeks = GetPreviousEmployeePostWeeks(startDate);
            List<EmployeePostDay> employeePostDays = new List<EmployeePostDay>();

            foreach (var prevWeek in previousWeeks.Where(i => i.EmployeePostDays.Count > 0))
                employeePostDays.AddRange(prevWeek.EmployeePostDays);

            EmployeePostWeek currentWeek = GetEmployeePostWeek(startDate);
            if (currentWeek == null)
                return new List<ScheduleRuleEvaluationItem>();
            List<DayOfWeek> handledDays = currentWeek.HandledDays;
            employeePostDays.AddRange(currentWeek.EmployeePostDays.Where(d => d.Length > 0 && d.Date != startDate && d.Date <= stopDate.Value && currentWeek.HandledDays.Contains(d.Date.DayOfWeek)));

            foreach (EmployeePostDay employeePostDay in employeePostDays)
            {
                ScheduleRuleEvaluationItem item = new ScheduleRuleEvaluationItem(CalendarUtility.MergeDateAndTime(employeePostDay.Date, employeePostDay.StartTime, ignoreSqlServerDateTime: true), CalendarUtility.MergeDateAndTime(employeePostDay.Date, employeePostDay.StopTime, ignoreSqlServerDateTime: true));
                item.DayOfWeek = employeePostDay.DayOfWeek;
                previousShifts.Add(item);
            }

            return previousShifts;
        }

        public List<ScheduleRuleEvaluationItem> GetAllCycleShifts(List<DateTime> excludeDates = null)
        {
            excludeDates = excludeDates != null ? excludeDates : new List<DateTime>();

            List<ScheduleRuleEvaluationItem> allShifts = new List<ScheduleRuleEvaluationItem>();

            foreach (EmployeePostWeek employeePostWeek in this.EmployeePostWeeks)
            {
                foreach (EmployeePostDay employeePostDay in employeePostWeek.EmployeePostDays)
                {
                    if (excludeDates.Contains(employeePostDay.Date))
                        continue;

                    ScheduleRuleEvaluationItem item = new ScheduleRuleEvaluationItem(CalendarUtility.MergeDateAndTime(employeePostDay.Date, employeePostDay.StartTime, ignoreSqlServerDateTime: true), CalendarUtility.MergeDateAndTime(employeePostDay.Date, employeePostDay.StopTime, ignoreSqlServerDateTime: true));
                    item.DayOfWeek = employeePostDay.DayOfWeek;
                    allShifts.Add(item);
                }
            }

            return allShifts;
        }

        public List<ScheduleRuleEvaluationItem> GetWeekShifts(int weekNumber)
        {
            List<ScheduleRuleEvaluationItem> allShifts = new List<ScheduleRuleEvaluationItem>();
            var employeePostDays = this.GetEmployeePostDaysInWeek(weekNumber);

            foreach (EmployeePostDay employeePostDay in employeePostDays)
            {
                ScheduleRuleEvaluationItem item = new ScheduleRuleEvaluationItem(CalendarUtility.MergeDateAndTime(employeePostDay.Date, employeePostDay.StartTime, ignoreSqlServerDateTime: true), CalendarUtility.MergeDateAndTime(employeePostDay.Date, employeePostDay.StopTime, ignoreSqlServerDateTime: true));
                item.DayOfWeek = employeePostDay.DayOfWeek;
                allShifts.Add(item);
            }

            return allShifts;

        }

        public DateTime GetFirstPossibleStart(DateTime dateTime, EmployeePostDTO employeePost, EmployeePostWeek employeePostWeek, EmployeePostCycle employeePostCycle, List<CalculationPeriodItem> periodItems, out bool adjustedStart)
        {
            adjustedStart = false;
            var firstTaskTime = dateTime.Date;
            var lastTime = GetLastStopDate(dateTime);

            var firstTask = periodItems.OrderBy(o => o.TimeSlot.MinFrom).FirstOrDefault();

            if (firstTask == null && lastTime == DateTime.MinValue)
                firstTaskTime = employeePostWeek.CalculationOptions.GetOpeningTime(dateTime, CalendarUtility.DATETIME_DEFAULT);
            else
                firstTaskTime = CalendarUtility.MergeDateAndTime(dateTime, firstTask.TimeSlot.MinFrom, ignoreSqlServerDateTime: true);

            if (this.EmployeePost == null || this.EmployeePost.EmployeeGroupDTO == null)
                return employeePostWeek.CalculationOptions.GetOpeningTime(dateTime, firstTaskTime);

            if (!(employeePostWeek.WeekNumber == 1 && employeePostWeek.WeekWithAddedExtraDayOfWork(employeePostCycle)) && employeePostWeek.EmployeePostDays.Count > 4 && employeePost.EmployeeGroupDTO.RuleRestTimeWeek > 0)
            {
                int maxRestTime;
                DateTime maxRestTimeStarts;
                if (employeePostCycle.EvaluateRestTimeWeek(employeePost.EmployeeGroupDTO.RuleRestTimeWeek, employeePostCycle.GetWeekShifts(employeePostWeek.WeekNumber), employeePostWeek.StartDate, employeePostWeek.StartDate.AddDays(7), out maxRestTime, out maxRestTimeStarts))
                {
                    if (maxRestTimeStarts < dateTime)
                    {
                        adjustedStart = true;
                        var time = lastTime.AddMinutes(employeePost.EmployeeGroupDTO.RuleRestTimeWeek);
                        var basedOnOpening = employeePostWeek.CalculationOptions.GetOpeningTime(dateTime, time);

                        if (time < firstTaskTime)
                            return firstTaskTime;

                        if (time < basedOnOpening)
                            return basedOnOpening;

                        return time;
                    }
                }
                else
                    return DateTime.MaxValue;
            }

            if (employeePostWeek.WeekNumber == 1 && dateTime.DayOfWeek == DayOfWeek.Monday && WorksLateOnDay(DayOfWeek.Sunday))
                lastTime = LastPossibleTimeOnDay(DayOfWeek.Sunday, lastTime != DateTime.MinValue ? lastTime : firstTaskTime);

            var fallbackTime = lastTime.AddMinutes(this.EmployeePost.EmployeeGroupDTO.RuleRestTimeDay);

            if (fallbackTime < firstTaskTime)
                fallbackTime = firstTaskTime;

            if (fallbackTime <= employeePostWeek.CalculationOptions.GetOpeningTime(dateTime, fallbackTime))
                return fallbackTime;
            else return employeePostWeek.CalculationOptions.GetOpeningTime(dateTime, fallbackTime);
        }

        public DateTime GetLastPossibleStop(DateTime dateTime, EmployeePostDTO employeePost, EmployeePostWeek employeePostWeek, EmployeePostCycle employeePostCycle, List<CalculationPeriodItem> periodItems, bool skipRestTimeWeek)
        {
            var nextStart = GetNextStartDate(dateTime);
            var lastTaskTime = dateTime.Date.AddDays(1).AddMinutes(-1);

            var lastTask = periodItems.OrderByDescending(o => o.TimeSlot.MaxTo).FirstOrDefault();

            if (lastTask == null && nextStart == DateTime.MaxValue)
                lastTaskTime = employeePostWeek.CalculationOptions.GetClosingTime(dateTime, CalendarUtility.DATETIME_DEFAULT);
            else
                lastTaskTime = CalendarUtility.MergeDateAndTime(dateTime, lastTask.TimeSlot.MaxTo, ignoreSqlServerDateTime: true);

            if (this.EmployeePost == null || this.EmployeePost.EmployeeGroupDTO == null)
                return employeePostWeek.CalculationOptions.GetClosingTime(dateTime, dateTime);

            if (!(employeePostWeek.WeekNumber == 1 && employeePostWeek.WeekWithAddedExtraDayOfWork(employeePostCycle)) && !skipRestTimeWeek && employeePostWeek.EmployeePostDays.Count > 4 && employeePost.EmployeeGroupDTO.RuleRestTimeWeek > 0)
            {
                int maxRestTime;
                DateTime maxRestTimeStarts;
                if (employeePostCycle.EvaluateRestTimeWeek(employeePost.EmployeeGroupDTO.RuleRestTimeWeek, employeePostCycle.GetWeekShifts(employeePostWeek.WeekNumber), employeePostWeek.StartDate, employeePostWeek.StartDate.AddDays(7), out maxRestTime, out maxRestTimeStarts))
                {
                    if (maxRestTimeStarts < dateTime)
                    {
                        var time = nextStart.AddMinutes(-employeePost.EmployeeGroupDTO.RuleRestTimeWeek);
                        var basedOnClosing = employeePostWeek.CalculationOptions.GetClosingTime(dateTime, time);

                        if (time > lastTaskTime)
                            return lastTaskTime;

                        if (time > basedOnClosing)
                            return basedOnClosing;

                        return time;
                    }
                }
            }

            var fallbackTime = nextStart.AddMinutes(-this.EmployeePost.EmployeeGroupDTO.RuleRestTimeDay);

            if (fallbackTime >= employeePostWeek.CalculationOptions.GetClosingTime(dateTime, fallbackTime))
                return fallbackTime;
            else
                return employeePostWeek.CalculationOptions.GetClosingTime(dateTime, fallbackTime);
        }

        public DateTime GetLastStopDate(DateTime date)
        {

            if (this.GetEmployeePostDays() == null || this.GetEmployeePostDays().Count == 0)
                return DateTime.MinValue;

            var last = this.GetEmployeePostDays().Where(i => i.Date < date).OrderBy(o => o.Date).LastOrDefault();

            if (last != null)
                return CalendarUtility.MergeDateAndTime(last.Date, last.StopTime, ignoreSqlServerDateTime: true);
            else
                return DateTime.MinValue;
        }

        public DateTime GetNextStartDate(DateTime date)
        {
            if (this.GetEmployeePostDays() == null || this.GetEmployeePostDays().Count == 0)
                return DateTime.MaxValue;

            var next = this.GetEmployeePostDays().Where(i => i.Date > date).OrderBy(o => o.Date).FirstOrDefault();

            if (next == null && date == this.StopDate)
            {
                next = this.GetEmployeePostDays().Where(i => i.Date.Date == this.StartDate.Date).FirstOrDefault();

                if (next != null)
                    return CalendarUtility.MergeDateAndTime(this.StopDate.AddDays(1), next.StartTime, ignoreSqlServerDateTime: true);
            }

            if (next != null)
                return CalendarUtility.MergeDateAndTime(next.Date, next.StartTime, ignoreSqlServerDateTime: true);
            else
                return DateTime.MaxValue;
        }

        public bool IsCycleValid()
        {
            //List<Tuple<int, ScheduleCycleRuleTypeDTO>> unreached = this.GetUnReachedScheduleCycleRuleTypesForCycle();

            //if (unreached.Count > 0)
            //    return false;

            return true;
        }

        public bool IsEvening(ScheduleCycleRuleTypeDTO scheduleCycleRuleType, DayOfWeek dayOfWeek)
        {
            var valid = scheduleCycleRuleType.IsEvening(EveningStarts(dayOfWeek));

            if (!valid)
            {
                if (dayOfWeek != DayOfWeek.Monday && scheduleCycleRuleType.DayOfWeekIds.Contains((int)DayOfWeek.Monday) && WorksLateOnDay(DayOfWeek.Sunday))
                    valid = true;
            }
            else if (scheduleCycleRuleType.DayOfWeekIds.Count == 1 && scheduleCycleRuleType.DayOfWeekIds.Contains(((int)dayOfWeek) - 1))
                valid = false;

            return valid;
        }

        public bool IsDayValid(PeriodItemsGroupHead periodItemsGroupHead, DateTime date, bool ignoreScheduleCycleRules, out int restSincePrevDayBreachMinutes, out int restToNextDayBreachMinutes)
        {
            #region prereq

            restSincePrevDayBreachMinutes = 0;
            restToNextDayBreachMinutes = 0;

            bool valid = true;

            #endregion

            if (!periodItemsGroupHead.Consecutive)
                return false;

            ScheduleRuleEvaluationItem currentDayScheduleRuleEvaluationItem = new ScheduleRuleEvaluationItem();
            var currentDate = date;
            var currentDay = GetEmployeePostDay(currentDate);

            if (!ignoreScheduleCycleRules && periodItemsGroupHead != null && periodItemsGroupHead.CalculationPeriodItems != null)
            {
                var head = periodItemsGroupHead;
                currentDayScheduleRuleEvaluationItem = new ScheduleRuleEvaluationItem(currentDate.DayOfWeek, CalendarUtility.MergeDateAndTime(currentDate, head.StartTime, ignoreSqlServerDateTime: true), CalendarUtility.MergeDateAndTime(currentDate.Date, head.StopTime, ignoreSqlServerDateTime: true));
                valid = IsShiftValidAgainstScheduleCycleRules(GetPreviousShifts(currentDay.Date), currentDayScheduleRuleEvaluationItem);
            }

            if (!valid)
                return valid;

            ScheduleRuleEvaluationItem previousDayScheduleRuleEvaluationItem = new ScheduleRuleEvaluationItem();
            var previousDate = date.AddDays(-1);
            var previousday = GetEmployeePostDay(previousDate);

            if (previousday != null && previousday.SelectedItemsHead != null && previousday.SelectedItemsHead.CalculationPeriodItems != null && previousday.SelectedItemsHead.CalculationPeriodItems.Count > 0)
            {
                PeriodItemsGroupHead head = previousday.SelectedItemsHead;
                previousDayScheduleRuleEvaluationItem = new ScheduleRuleEvaluationItem(previousDate.DayOfWeek, CalendarUtility.MergeDateAndTime(previousDate, head.StartTime, ignoreSqlServerDateTime: true), CalendarUtility.MergeDateAndTime(previousDate.Date, head.StopTime, ignoreSqlServerDateTime: true));
            }

            ScheduleRuleEvaluationItem nextDayScheduleRuleEvaluationItem = new ScheduleRuleEvaluationItem();
            var nextDate = date.AddDays(+1);
            var nextday = GetEmployeePostDay(nextDate);

            if (nextday != null && nextday.SelectedItemsHead != null && nextday.SelectedItemsHead.CalculationPeriodItems != null && nextday.SelectedItemsHead.CalculationPeriodItems.Count > 0)
            {
                PeriodItemsGroupHead head = nextday.SelectedItemsHead;
                nextDayScheduleRuleEvaluationItem = new ScheduleRuleEvaluationItem(nextDate.DayOfWeek, CalendarUtility.MergeDateAndTime(nextDate, head.StartTime, ignoreSqlServerDateTime: true), CalendarUtility.MergeDateAndTime(nextDate.Date, head.StopTime, ignoreSqlServerDateTime: true));
            }

            return valid = EvaluateRestTimeDay(this.EmployeePost.EmployeeGroupDTO.RuleRestTimeDay, currentDayScheduleRuleEvaluationItem, previousDayScheduleRuleEvaluationItem, nextDayScheduleRuleEvaluationItem, out restSincePrevDayBreachMinutes, out restToNextDayBreachMinutes);

        }

        public PeriodItemsGroupHead GetBestPeriodItemsGroupHead(EmployeePostCyclesRun employeePostCyclesRun, EmployeePostWeek employeePostWeek, DateTime currentDate, List<PeriodItemsGroupHead> allPossiblePeriodGroupHeads, bool forcePreferedLenght, int preferedLenght = 0, bool skipPrioShift = false)
        {
            var workTimePerDay = this.EmployeePost.WorkTimePerDay != 0 ? this.EmployeePost.WorkTimePerDay : 8;
            List<Tuple<int, ScheduleCycleRuleTypeDTO, int, int>> unmaxed = this.GetUnMaxScheduleCycleRuleTypesForCycle();
            List<ScheduleRuleEvaluationItem> scheduleRuleEvaluationItems = new List<ScheduleRuleEvaluationItem>();
            List<PeriodItemsGroupHead> unMaxedPossiblePeriodGroupHeads = new List<PeriodItemsGroupHead>();
            List<PeriodItemsGroupHead> possiblePeriodGroupHeads = allPossiblePeriodGroupHeads.ToList();

            #region Validate Against Week

            if (this.EmployeePostWeeks.Count == 1)
                possiblePeriodGroupHeads = possiblePeriodGroupHeads.Where(i => i.Length <= employeePostWeek.RemainingMinutesWeek).ToList();
            else if (this.EmployeePostWeeks.Count > 1)
                possiblePeriodGroupHeads = possiblePeriodGroupHeads.Where(i => i.Length <= employeePostWeek.RemainingMinutesWeekToMax).ToList();

            if (possiblePeriodGroupHeads.Count == 0)
                return new PeriodItemsGroupHead();

            #endregion

            #region Checkrules

            int restSincePrevDayBreachMinutes;
            int restToNextDayBreachMinutes;
            possiblePeriodGroupHeads = possiblePeriodGroupHeads.Where(w => IsDayValid(w, currentDate, true, out restSincePrevDayBreachMinutes, out restToNextDayBreachMinutes)).ToList();

            if (possiblePeriodGroupHeads.Where(w => w.HasUniques(employeePostCyclesRun)).Any())
                possiblePeriodGroupHeads = possiblePeriodGroupHeads.Where(w => w.HasUniques(employeePostCyclesRun)).ToList();

            #endregion

            decimal factor = new decimal(0.8);
            bool revertFactor = unmaxed.Any(a => a.Item3 > 0 && (a.Item1 + a.Item3) > this.AttemptedWeeks.Count && (a.Item1 - a.Item3) < a.Item1 / 2);
            if (revertFactor)
                factor = decimal.Divide(factor, 3);

            foreach (var item in unmaxed.OrderByDescending(o => o.Item1 - o.Item3))
                scheduleRuleEvaluationItems.AddRange(GetValidShifts(possiblePeriodGroupHeads, item.Item2));

            foreach (ScheduleRuleEvaluationItem scheduleRuleEvaluationItem in scheduleRuleEvaluationItems)
                unMaxedPossiblePeriodGroupHeads.Add(scheduleRuleEvaluationItem.GetPeriodItemsGroupHead());

            if (!unMaxedPossiblePeriodGroupHeads.Any())
            {
                unmaxed = this.GetScheduleCycleRuleTypesForCycle();

                revertFactor = unmaxed.Any(a => a.Item3 > 0 && (a.Item1 + a.Item3) > this.AttemptedWeeks.Count && (a.Item1 - a.Item3) < a.Item1 / 2);
                if (revertFactor)
                    factor = 0;

                foreach (var item in unmaxed.OrderByDescending(o => o.Item1 - o.Item3))
                    scheduleRuleEvaluationItems.AddRange(GetValidShifts(possiblePeriodGroupHeads, item.Item2));

                foreach (ScheduleRuleEvaluationItem scheduleRuleEvaluationItem in scheduleRuleEvaluationItems)
                    unMaxedPossiblePeriodGroupHeads.Add(scheduleRuleEvaluationItem.GetPeriodItemsGroupHead());
            }

            #region FutureWeeks

            //TODO Setting
            if (unMaxedPossiblePeriodGroupHeads.Any(a => a.PercentFutureWeeks >= 50) && (employeePostWeek.WeekNumber == 1 || EmployeePost.WorkDaysWeek < 5))
            {
                DayOfWeek dayOfWeek = currentDate.DayOfWeek;

                var filtered = possiblePeriodGroupHeads.Where(a => a.PercentFutureWeeks >= 75 && a.Length > decimal.Multiply(preferedLenght, new decimal(0.7)) && a.Length < decimal.Multiply(preferedLenght, new decimal(1.3))).ToList();
                filtered = FilterOnRules(filtered, preferedLenght, forcePreferedLenght, employeePostWeek, currentDate, revertFactor, factor, skipPrioShift);
                if (filtered.Any())
                    return NearestSumTime(preferedLenght == 0 ? workTimePerDay : preferedLenght, filtered);

                if (dayOfWeek == DayOfWeek.Saturday || dayOfWeek == DayOfWeek.Sunday)
                {
                    int rules = unmaxed.Where(w => w.Item2.IsWeekEndOnly && w.Item2.DayOfWeekIds.Contains((int)dayOfWeek)).Count();
                    int percent = 100;

                    if (rules > 0)
                        percent = 25;

                    filtered = possiblePeriodGroupHeads.Where(a => a.PercentFutureWeeks >= percent && a.Length > decimal.Multiply(preferedLenght, new decimal(0.7)) && a.Length < decimal.Multiply(preferedLenght, new decimal(1.3))).ToList();
                    filtered = FilterOnRules(filtered, preferedLenght, forcePreferedLenght, employeePostWeek, currentDate, revertFactor, factor, skipPrioShift);
                    if (filtered.Any())
                        return NearestSumTime(preferedLenght == 0 ? workTimePerDay : preferedLenght, filtered);
                }
            }

            #endregion

            unMaxedPossiblePeriodGroupHeads = FilterOnRules(unMaxedPossiblePeriodGroupHeads, preferedLenght, forcePreferedLenght, employeePostWeek, currentDate, revertFactor, factor, skipPrioShift);

            #region Prio ShiftTypes

            if (unMaxedPossiblePeriodGroupHeads.Count > 0)
            {
                var prioUnreachedPossiblePeriodItems = unMaxedPossiblePeriodGroupHeads.Where(u => u.HasPrioShiftTypes(employeePostCyclesRun.PrioShiftTypesIds)).ToList();

                if (prioUnreachedPossiblePeriodItems.Count > 0)
                {
                    if (employeePostWeek.EmployeePost.EmployeePostSkillDTOs != null && employeePostWeek.EmployeePost.EmployeePostSkillDTOs.Count > 0)
                        prioUnreachedPossiblePeriodItems = unMaxedPossiblePeriodGroupHeads.Where(u => !u.HasShiftTypesWithoutSkills()).ToList();

                    if (prioUnreachedPossiblePeriodItems.Count > 0)
                    {
                        return NearestSumTime(preferedLenght == 0 ? workTimePerDay : preferedLenght, prioUnreachedPossiblePeriodItems);
                    }
                }
            }

            if (!skipPrioShift)
            {
                var priopossiblePeriodGroupHeads = possiblePeriodGroupHeads.Where(u => u.HasPrioShiftTypes(employeePostCyclesRun.PrioShiftTypesIds)).ToList();

                if (priopossiblePeriodGroupHeads.Count > 0)
                {
                    if (employeePostWeek.EmployeePost.EmployeePostSkillDTOs != null && employeePostWeek.EmployeePost.EmployeePostSkillDTOs.Count > 0)
                        priopossiblePeriodGroupHeads = priopossiblePeriodGroupHeads.Where(u => !u.HasShiftTypesWithoutSkills()).ToList();

                    if (priopossiblePeriodGroupHeads.Count > 0)
                    {
                        return NearestSumTime(preferedLenght == 0 ? workTimePerDay : preferedLenght, priopossiblePeriodGroupHeads);
                    }
                }
            }

            var filter = employeePostWeek.WeekNumber == 1 ? possiblePeriodGroupHeads.Where(w => !IsScheduleCycleTypeMaxedOutCurrentWeek(currentDate, w.StartTime, w.StopTime, employeePostWeek.WeekNumber)).ToList() : null;

            if (!filter.IsNullOrEmpty())
                possiblePeriodGroupHeads = filter;

            if (possiblePeriodGroupHeads.Any(a => a.PercentFutureWeeks >= 50))
            {
                DayOfWeek dayOfWeek = currentDate.DayOfWeek;

                var filtered = possiblePeriodGroupHeads.Where(a => a.PercentFutureWeeks >= 75 && a.Length > decimal.Multiply(preferedLenght, new decimal(0.7)) && a.Length < decimal.Multiply(preferedLenght, new decimal(1.3))).ToList();

                if (filtered.Any())
                    return NearestSumTime(preferedLenght == 0 ? workTimePerDay : preferedLenght, filtered);

                if (dayOfWeek == DayOfWeek.Saturday || dayOfWeek == DayOfWeek.Sunday)
                {
                    int rules = unmaxed.Where(w => w.Item2.IsWeekEndOnly && w.Item2.DayOfWeekIds.Contains((int)dayOfWeek)).Count();
                    int percent = 100;

                    if (rules > 0)
                        percent = 25;

                    filtered = possiblePeriodGroupHeads.Where(a => a.PercentFutureWeeks >= percent && a.Length > decimal.Multiply(preferedLenght, new decimal(0.7)) && a.Length < decimal.Multiply(preferedLenght, new decimal(1.3))).ToList();

                    if (filtered.Any())
                        return NearestSumTime(preferedLenght == 0 ? workTimePerDay : preferedLenght, filtered);
                }
            }

            #endregion

            if (unMaxedPossiblePeriodGroupHeads.Count == 0)
                return NearestSumTime(preferedLenght == 0 ? workTimePerDay : preferedLenght, possiblePeriodGroupHeads);
            else
                return NearestSumTime(preferedLenght == 0 ? workTimePerDay : preferedLenght, unMaxedPossiblePeriodGroupHeads);
        }

        private List<PeriodItemsGroupHead> FilterOnRules(List<PeriodItemsGroupHead> unreachedPossiblePeriodGroupHeads, int preferedLenght, bool forcePreferedLenght, EmployeePostWeek employeePostWeek, DateTime currentDate, bool revertFactor, decimal factor, bool skipPrioShift)
        {
            List<PeriodItemsGroupHead> filteredUnreachedPossiblePeriodItems = new List<PeriodItemsGroupHead>();

            if (!forcePreferedLenght && unreachedPossiblePeriodGroupHeads.Count > 0)
            {
                //if (employeePostWeek.WeekNumber == 1 && currentDate.DayOfWeek == DayOfWeek.Monday && EmployeePost.ScheduleCycleDTO.HasWeekends() && !IsWeekendWeek(employeePostWeek.WeekNumber, this.IgnoreFreeDays) && employeePostWeek.WeekNumber == 1)
                //{
                //    var rules = GetScheduleCycleRuleTypesForCycle().OrderByDescending(o => o.Item1);

                //    foreach (var ruleGroup in rules.Where(w => w.Item2.DayOfWeekIds.Contains((int)currentDate.DayOfWeek)).GroupBy(g => g.Item1))
                //    {
                //        List<PeriodItemsGroupHead> validOnRule = new List<PeriodItemsGroupHead>();

                //        foreach (var rule in ruleGroup)
                //        {
                //            if ((IsEvening(rule.Item2, currentDate.DayOfWeek) && !IsScheduleCycleTypeMaxedOutCurrentWeek(currentDate, rule.Item2, employeePostWeek.WeekNumber)))
                //            {
                //                foreach (var head in unreachedPossiblePeriodGroupHeads)
                //                    if (rule.Item2.Valid(currentDate.DayOfWeek, head.StartTime, head.StopTime))
                //                    {
                //                        if (IsEvening(rule.Item2, currentDate.DayOfWeek) && !IgnoreFreeDays && (employeePostWeek.FreeDays.Contains(currentDate.AddDays(1).DayOfWeek) || (!employeePostWeek.IsWeekendWeek && currentDate.DayOfWeek == DayOfWeek.Friday)))
                //                            continue;

                //                        validOnRule.Add(head);
                //                    }
                //            }
                //        }

                //        if (validOnRule.Any())
                //            return validOnRule;
                //    }
                //}


                if (employeePostWeek.WeekNumber == 1 || EmployeePost.WorkDaysWeek < 5)
                {
                    var rules = GetScheduleCycleRuleTypesForCycle().OrderByDescending(o => o.Item1);

                    //if (employeePostWeek.FreeDays.Contains(currentDate.AddDays(-1).DayOfWeek) && !employeePostWeek.EmployeePostDays.Any(a => a.Date.DayOfWeek == currentDate.AddDays(-1).DayOfWeek))
                    //{
                    //    var notMorningRules = rules.Where(w => w.Item2.DayOfWeekIds.Contains((int)currentDate.DayOfWeek) && w.Item2.StartTime.Hour >= 10);

                    //    if (notMorningRules.Any())
                    //    {
                    //        List<PeriodItemsGroupHead> validOnRule = new List<PeriodItemsGroupHead>();
                    //        var rule = notMorningRules.First();

                    //        if (rule.Item1 >= rules.First().Item1 - 1)
                    //        {
                    //            foreach (var head in unreachedPossiblePeriodGroupHeads)
                    //                if (rule.Item2.Valid(currentDate.DayOfWeek, head.StartTime, head.StopTime))
                    //                    validOnRule.Add(head);

                    //            if (validOnRule.Any())
                    //                return validOnRule;
                    //        }
                    //    }
                    //}

                    foreach (var ruleGroup in rules.Where(w => w.Item2.DayOfWeekIds.Contains((int)currentDate.DayOfWeek)).GroupBy(g => g.Item1))
                    {
                        List<PeriodItemsGroupHead> validOnRule = new List<PeriodItemsGroupHead>();

                        foreach (var rule in ruleGroup)
                        {
                            if ((!IsEvening(rule.Item2, currentDate.DayOfWeek) && !IsScheduleCycleTypeMaxedOutCurrentWeek(currentDate, rule.Item2, employeePostWeek.WeekNumber)) || !IsScheduleCycleTypeUsedOutCurrentWeek(currentDate, rule.Item2, employeePostWeek.WeekNumber))
                            {
                                foreach (var head in unreachedPossiblePeriodGroupHeads)
                                    if (rule.Item2.Valid(currentDate.DayOfWeek, head.StartTime, head.StopTime))
                                    {
                                        bool valid = true;
                                        if (IsEvening(rule.Item2, currentDate.DayOfWeek) && !IgnoreFreeDays && (employeePostWeek.FreeDays.Contains(currentDate.AddDays(1).DayOfWeek) || (!employeePostWeek.IsWeekendWeek && currentDate.DayOfWeek == DayOfWeek.Friday)))
                                            valid = false;

                                        //if (!valid && !validOnRule.Any(w => w.StartTime.Hour > 10) && !IsWeekendWeek(employeePostWeek.WeekNumber, IgnoreFreeDays) && IsEvening(rule.Item2, currentDate.DayOfWeek) && employeePostWeek.WeekNumber == 1 && currentDate.DayOfWeek == DayOfWeek.Monday && NextWeekIsWeekendWeek(employeePostWeek.WeekNumber) && !IsScheduleCycleTypeUsedOutCurrentWeek(currentDate, rule.Item2, employeePostWeek.WeekNumber))
                                        //{
                                        //    validOnRule = validOnRule.Where(w => w.StartTime.Hour > 10).ToList();
                                        //    valid = true;
                                        //}

                                        if (valid)
                                            validOnRule.Add(head);
                                    }
                            }
                        }

                        if (validOnRule.Any())
                        {
                            //if (validOnRule.Count > 1 && employeePostWeek.FreeDays.Contains(currentDate.AddDays(-1).DayOfWeek))
                            //{
                            //    //prefer evening
                            //    var evenings = validOnRule.Where(a => a.StartTime.Hour > 10 && a.Length > decimal.Multiply(preferedLenght, new decimal(0.7)) && a.Length < decimal.Multiply(preferedLenght, new decimal(1.3))).ToList();

                            //    if (evenings.Any())
                            //        return evenings;
                            //}

                            return validOnRule;
                        }
                    }

                    foreach (var ruleGroup in rules.Where(w => w.Item2.DayOfWeekIds.Contains((int)currentDate.DayOfWeek)).GroupBy(g => g.Item1))
                    {
                        List<PeriodItemsGroupHead> validOnRule = new List<PeriodItemsGroupHead>();

                        foreach (var rule in ruleGroup)
                        {
                            if (!IsScheduleCycleTypeMaxedOutCurrentWeek(currentDate, rule.Item2, employeePostWeek.WeekNumber))
                            {
                                foreach (var head in unreachedPossiblePeriodGroupHeads)
                                    if (rule.Item2.Valid(currentDate.DayOfWeek, head.StartTime, head.StopTime))
                                    {
                                        if (IsEvening(rule.Item2, currentDate.DayOfWeek) && !IgnoreFreeDays && (employeePostWeek.FreeDays.Contains(currentDate.AddDays(1).DayOfWeek) || (!employeePostWeek.IsWeekendWeek && currentDate.DayOfWeek == DayOfWeek.Friday)))
                                            continue;

                                        validOnRule.Add(head);
                                    }
                            }
                        }

                        if (validOnRule.Any())
                            return validOnRule;
                    }

                    foreach (var ruleGroup in rules.Where(w => w.Item2.DayOfWeekIds.Contains((int)currentDate.DayOfWeek)).GroupBy(g => g.Item1))
                    {
                        List<PeriodItemsGroupHead> validOnRule = new List<PeriodItemsGroupHead>();

                        foreach (var rule in ruleGroup)
                        {
                            foreach (var head in unreachedPossiblePeriodGroupHeads)
                                if (rule.Item2.Valid(currentDate.DayOfWeek, head.StartTime, head.StopTime))
                                    validOnRule.Add(head);
                        }

                        if (validOnRule.Any())
                            return validOnRule;
                    }
                }

                var previousDay = employeePostWeek.EmployeePostDays.Where(d => d.Date.Date == currentDate.Date.AddDays(-1)).FirstOrDefault();
                var nextDay = employeePostWeek.EmployeePostDays.Where(d => d.Date.Date == currentDate.Date.AddDays(1)).FirstOrDefault();

                if (previousDay == null)
                    previousDay = employeePostWeek.EmployeePostDays.Where(d => d.Date.Date == currentDate.Date.AddDays(-2)).FirstOrDefault();

                foreach (var item in unreachedPossiblePeriodGroupHeads)
                {
                    bool added = false;
                    if (previousDay != null && previousDay.SelectedItemsHead != null && previousDay.SelectedItemsHead.StartTime != CalendarUtility.DATETIME_DEFAULT)
                    {
                        int overLappingMinutes = CalendarUtility.GetOverlappingMinutes(previousDay.SelectedItemsHead.StartTime, previousDay.SelectedItemsHead.StopTime, item.StartTime, item.StopTime);
                        if (!revertFactor)
                        {
                            if (decimal.Divide(overLappingMinutes, item.Length) > factor)
                            {
                                filteredUnreachedPossiblePeriodItems.Add(item);
                                added = true;
                            }
                        }
                        else
                        {
                            if (decimal.Divide(overLappingMinutes, item.Length) < factor)
                            {
                                filteredUnreachedPossiblePeriodItems.Add(item);
                                added = true;
                            }
                        }
                    }

                    if (!added && nextDay != null && nextDay.SelectedItemsHead != null && nextDay.SelectedItemsHead.StartTime != CalendarUtility.DATETIME_DEFAULT)
                    {
                        int overLappingMinutes = CalendarUtility.GetOverlappingMinutes(nextDay.SelectedItemsHead.StartTime, nextDay.SelectedItemsHead.StopTime, item.StartTime, item.StopTime);
                        if (!revertFactor)
                        {
                            if (decimal.Divide(overLappingMinutes, item.Length) > factor)
                                filteredUnreachedPossiblePeriodItems.Add(item);
                        }
                        else
                        {
                            if (decimal.Divide(overLappingMinutes, item.Length) < factor)
                                filteredUnreachedPossiblePeriodItems.Add(item);
                        }
                    }
                }

                if (filteredUnreachedPossiblePeriodItems.Count > 0)
                    unreachedPossiblePeriodGroupHeads = filteredUnreachedPossiblePeriodItems.ToList();

                if (!skipPrioShift && unreachedPossiblePeriodGroupHeads.Any(i => i.Length > (preferedLenght - 60) && i.Length < (preferedLenght + 60)))
                    unreachedPossiblePeriodGroupHeads = unreachedPossiblePeriodGroupHeads.Where(i => i.Length > (preferedLenght - 60) && i.Length < (preferedLenght + 60)).ToList();
            }

            return filteredUnreachedPossiblePeriodItems;

        }


        private PeriodItemsGroupHead NearestSumTime(int workTimePerDay, List<PeriodItemsGroupHead> possiblePeriodItems)
        {
            return possiblePeriodItems.OrderBy(v => Math.Abs((long)v.Length - workTimePerDay)).ThenByDescending(t => t.NumberOfFixedTasksOrDelivery).First();
        }

        public bool EvaluateRestTimeDay(int limitRestTimeDayMinutes, ScheduleRuleEvaluationItem currentDayShift, ScheduleRuleEvaluationItem prevDayShift, ScheduleRuleEvaluationItem nextDayShift, out int restSincePrevDayBreachMinutes, out int restToNextDayBreachMinutes)
        {
            restSincePrevDayBreachMinutes = 0;
            restToNextDayBreachMinutes = 0;

            if (prevDayShift != null)
            {
                int restMinutes = (int)currentDayShift.StartTime.Subtract(prevDayShift.StopTime).TotalMinutes;
                if (restMinutes > 0)
                {
                    int breachMinutes = limitRestTimeDayMinutes - restMinutes;
                    if (breachMinutes > 0)
                        restSincePrevDayBreachMinutes = breachMinutes;
                }
            }

            if (nextDayShift != null)
            {
                int restMinutes = (int)nextDayShift.StartTime.Subtract(currentDayShift.StopTime).TotalMinutes;
                if (restMinutes > 0)
                {
                    int breachMinutes = limitRestTimeDayMinutes - restMinutes;
                    if (breachMinutes > 0)
                        restToNextDayBreachMinutes = breachMinutes;
                }
            }

            return restSincePrevDayBreachMinutes <= 0 && restToNextDayBreachMinutes <= 0;
        }

        public bool EvaluateRestTimeWeek(int limitRestTimeWeekMinutes, List<ScheduleRuleEvaluationItem> shifts, DateTime weekStart, DateTime weekStop, out int maxRestTime, out DateTime maxRestTimeStarts)
        {
            maxRestTime = 0;
            maxRestTimeStarts = CalendarUtility.DATETIME_DEFAULT;

            List<ScheduleRuleEvaluationItem> shiftsForWeek = shifts.Where(shift => shift.StartTime != shift.StopTime && CalendarUtility.IsTimesOverlappingNew(shift.StartTime, shift.StopTime, weekStart, weekStop)).OrderBy(shift => shift.StartTime).ToList();
            if (shiftsForWeek.Count == 0)
                return true;

            DateTime latestStop = weekStart;
            int shiftCount = 0;

            foreach (var shift in shiftsForWeek)
            {
                shiftCount++;

                //Measure rest to previous shift, or for first shift - beginning of week
                int restToPrevShiftMinutes = (int)shift.StartTime.Subtract(latestStop).TotalMinutes;
                if (restToPrevShiftMinutes > maxRestTime)
                {
                    maxRestTime = restToPrevShiftMinutes;
                    maxRestTimeStarts = latestStop;
                }

                //Measure rest from shift to end of week
                bool isLastShiftInWeek = shiftCount == shiftsForWeek.Count;
                if (isLastShiftInWeek)
                {
                    int restToLatestShiftInWeekMinutes = (int)weekStop.Subtract(shift.StopTime).TotalMinutes;
                    if (restToLatestShiftInWeekMinutes > maxRestTime)
                    {
                        maxRestTime = restToLatestShiftInWeekMinutes;
                        maxRestTimeStarts = shift.StopTime;
                    }
                }

                latestStop = shift.StopTime;
            }

            return maxRestTime >= limitRestTimeWeekMinutes;
        }

        public bool WorksLateOnDay(DayOfWeek dayOfWeek)
        {
            var rulesOnDay = this.EmployeePost.ScheduleCycleDTO.ScheduleCycleRuleDTOs.Where(x => x.ScheduleCycleRuleTypeDTO != null && x.ScheduleCycleRuleTypeDTO.DayOfWeekIds.Contains((int)dayOfWeek)).ToList();

            if (rulesOnDay.Any())
            {
                return rulesOnDay.Any(a => a.ScheduleCycleRuleTypeDTO.StopTime > EveningStarts(dayOfWeek));
            }

            return false;
        }

        public DateTime LastPossibleTimeOnDay(DayOfWeek dayOfWeek, DateTime fallBackDateTime)
        {
            var rulesOnDay = this.EmployeePost.ScheduleCycleDTO.ScheduleCycleRuleDTOs.Where(x => x.ScheduleCycleRuleTypeDTO != null && x.ScheduleCycleRuleTypeDTO.DayOfWeekIds.Contains((int)dayOfWeek)).ToList();

            if (rulesOnDay.Any())
            {
                var max = rulesOnDay.Max(a => a.ScheduleCycleRuleTypeDTO.StopTime);

                var maxOnItems = this.InitialValidItems.Max(s => s.TimeSlot.MaxTo);

                if (maxOnItems < max)
                    max = maxOnItems;

                if (max.Hour >= fallBackDateTime.Hour)
                    return CalendarUtility.MergeDateAndDefaultTime(fallBackDateTime.AddDays(-1), max);
            }

            return fallBackDateTime;
        }

        public DateTime EveningStarts(DayOfWeek dayOfWeek)
        {
            var rulesOnDay = this.EmployeePost.ScheduleCycleDTO.ScheduleCycleRuleDTOs.Where(x => x.ScheduleCycleRuleTypeDTO != null && x.ScheduleCycleRuleTypeDTO.DayOfWeekIds.Contains((int)dayOfWeek)).ToList();
            var allRulesOnDay = !this.AllScheduleCycleRuleTypeDTOs.IsNullOrEmpty() ? this.AllScheduleCycleRuleTypeDTOs.Where(x => x.DayOfWeekIds.Contains((int)dayOfWeek)).ToList() : new List<ScheduleCycleRuleTypeDTO>();

            if (rulesOnDay.Any() && rulesOnDay.Count > 1)
            {
                rulesOnDay = rulesOnDay.OrderBy(o => o.ScheduleCycleRuleTypeDTO.StopTime).ToList();
                return rulesOnDay.Last().ScheduleCycleRuleTypeDTO.StartTime;
            }
            else if (allRulesOnDay.Any() && allRulesOnDay.Count > 1)
            {
                allRulesOnDay = allRulesOnDay.OrderBy(o => o.StopTime).ToList();
                return allRulesOnDay.Last().StartTime;
            }
            else if (rulesOnDay.Any())
            {
                return rulesOnDay.Last().ScheduleCycleRuleTypeDTO.StopTime.AddMinutes(-EmployeePost.EmployeeGroupDTO.RuleWorkTimeDayMinimum);
            }
            else
                return CalendarUtility.DATETIME_DEFAULT.AddHours(18);

        }
        public ScheduleCycleRuleDTO GetBestMatchingRule(DayOfWeek dayOfWeek, DateTime startTime, DateTime stopTime)
        {
            var matchingRules = this.EmployeePost.ScheduleCycleDTO.ScheduleCycleRuleDTOs.Where(x => x.Valid(dayOfWeek, startTime, stopTime)).ToList();
            int maxOverlappingMinutes = 0;
            ScheduleCycleRuleDTO bestMatchingRule = null;

            if (matchingRules.Count == 1)
            {
                bestMatchingRule = matchingRules.FirstOrDefault();
            }
            else
            {
                foreach (var matchingRule in matchingRules)
                {
                    if (matchingRule.GetOverlappingMinutes(startTime, stopTime) > maxOverlappingMinutes)
                    {
                        bestMatchingRule = matchingRule;
                        maxOverlappingMinutes = matchingRule.GetOverlappingMinutes(startTime, stopTime);
                    }
                }
            }

            return bestMatchingRule;
        }

        public bool IsShiftValidAgainstScheduleCycleRules(List<ScheduleRuleEvaluationItem> previousShifts, ScheduleRuleEvaluationItem shiftToEvaluate)
        {
            if (this.EmployeePost != null && this.EmployeePost.ScheduleCycleDTO != null)
            {
                if (this.EmployeePost.ScheduleCycleDTO.ScheduleCycleRuleDTOs == null) //if no rules, allow all shifts
                    return true;

                var bestMatchingRule = GetBestMatchingRule(shiftToEvaluate.DayOfWeek, shiftToEvaluate.StartTime, shiftToEvaluate.StopTime);

                if (bestMatchingRule != null)
                {
                    #region compare how many times matchingrule has been used with the given settings

                    int alreadyUsedCount = previousShifts.Count(x => bestMatchingRule.Valid(x.DayOfWeek, x.StartTime, x.StopTime));
                    int evaluateCount = alreadyUsedCount + 1;
                    if (evaluateCount <= bestMatchingRule.MaxOccurrences)
                        return true;

                    #endregion

                }
            }
            return false;
        }

        public List<PeriodItemsGroupHead> HasAvailableScheduleCycleRuleTypes(DateTime date, List<PeriodItemsGroupHead> periodItemsGroupHeads)
        {
            List<PeriodItemsGroupHead> newPeriodGroupHeads = new List<PeriodItemsGroupHead>();
            var types = GetAvailableScheduleCycleRuleTypes(date.DayOfWeek, GetPreviousShifts(date));

            foreach (var head in periodItemsGroupHeads)
            {
                bool valid = false;

                foreach (var type in types)
                {
                    if (type.Valid(date.DayOfWeek, head.StartTime, head.StopTime))
                        valid = true;
                }

                if (valid)
                    newPeriodGroupHeads.Add(head);
            }

            return newPeriodGroupHeads;
        }

        public List<Tuple<int, ScheduleCycleRuleTypeDTO, int, int>> GetScheduleCycleRuleTypesForCycle()
        {
            return GetScheduleCycleRuleTypes(this.GetAllCycleShifts());
        }

        public List<Tuple<int, ScheduleCycleRuleTypeDTO, int, int>> GetUnReachedScheduleCycleRuleTypesForCycle()
        {
            return GetUnReachedScheduleCycleRuleTypes(this.GetAllCycleShifts());
        }

        public bool IsScheduleCycleTypeMaxedOut(ScheduleCycleRuleTypeDTO dto)
        {
            return !GetUnMaxScheduleCycleRuleTypesForCycle().Any(a => a.Item2.ScheduleCycleRuleTypeId == dto.ScheduleCycleRuleTypeId);
        }

        public bool IsScheduleCycleTypeMaxedOutCurrentWeek(DateTime date, DateTime startTime, DateTime stopTime, int weekNr)
        {
            var rule = GetBestMatchingRule(date.DayOfWeek, startTime, stopTime);
            if (rule != null)
                return IsScheduleCycleTypeMaxedOutCurrentWeek(date, rule.ScheduleCycleRuleTypeDTO, weekNr);
            return false;
        }

        public bool IsScheduleCycleTypeMaxedOutCurrentWeek(DateTime date, ScheduleCycleRuleTypeDTO dto, int weekNr)
        {
            if (dto == null || weekNr == this.EmployeePostWeeks.Count)
                return false;


            var tupleType = GetUnMaxScheduleCycleRuleTypesForCycle(new List<DateTime>() { date }).FirstOrDefault(a => a.Item2.ScheduleCycleRuleTypeId == dto.ScheduleCycleRuleTypeId);

            if (tupleType == null)
                return true;

            if (tupleType != null && weekNr != this.EmployeePostWeeks.Count)
            {
                if (tupleType.Item3 == 0)
                    return false;

                decimal numberPerRemaningWeek = (decimal.Subtract(tupleType.Item4, tupleType.Item3) / decimal.Subtract(this.EmployeePostWeeks.Count, weekNr));

                if (numberPerRemaningWeek <= new decimal(1))
                    return true;
            }

            return false;
        }

        public bool IsScheduleCycleTypeUsedOutCurrentWeek(DateTime date, ScheduleCycleRuleTypeDTO dto, int weekNr)
        {
            if (dto == null || weekNr == this.EmployeePostWeeks.Count)
                return false;

            var tupleType = GetUnMaxScheduleCycleRuleTypesForCycle(new List<DateTime>() { date }).FirstOrDefault(a => a.Item2.ScheduleCycleRuleTypeId == dto.ScheduleCycleRuleTypeId);

            if (tupleType == null)
                return true;

            if (tupleType != null && weekNr != this.EmployeePostWeeks.Count)
            {
                if (tupleType.Item3 == 0)
                    return false;

                decimal numberPerRemaningWeek = Convert.ToDecimal(tupleType.Item1) / decimal.Subtract(this.EmployeePostWeeks.Count, weekNr);

                if (numberPerRemaningWeek <= new decimal(1))
                    return true;
            }

            return false;
        }

        public List<Tuple<int, ScheduleCycleRuleTypeDTO, int, int>> GetUnMaxScheduleCycleRuleTypesForCycle(List<DateTime> excludeDates = null)
        {
            return GetUnMaxedScheduleCycleRuleTypes(this.GetAllCycleShifts(excludeDates));
        }

        public List<Tuple<int, ScheduleCycleRuleTypeDTO>> GetReachedScheduleCycleRuleTypesForCycle()
        {
            return GetReachedScheduleCycleRuleTypes(this.GetAllCycleShifts());
        }

        private bool HasAvailableScheduleCycleRuleTypes(DateTime date, PeriodItemsGroupHead head)
        {
            var types = GetAvailableScheduleCycleRuleTypes(date.DayOfWeek, GetPreviousShifts(date));

            foreach (var type in types)
            {
                if (type.Valid(date.DayOfWeek, head.StartTime, head.StopTime))
                    return true;
            }

            return false;
        }

        public List<PeriodItemsGroupHead> GetItemsValidAgainstScheduleCycleRules(DateTime date, List<CalculationPeriodItem> periodItems)
        {
            List<PeriodItemsGroupHead> periodItemsGroupHeads = new List<PeriodItemsGroupHead>();
            var groups = periodItems.GroupBy(r => r.CalculationGuid);

            foreach (var group in groups)
            {
                PeriodItemsGroupHead head = new PeriodItemsGroupHead()
                {
                    Key = group.First().CalculationGuid.ToString(),
                    CalculationPeriodItems = group.ToList(),
                    Done = false
                };

                if (head.Consecutive && !head.HasHoles && IsShiftValidAgainstScheduleCycleRules(date, head))
                    periodItemsGroupHeads.Add(head);
            }

            return periodItemsGroupHeads;
        }

        public bool IsShiftValidAgainstScheduleCycleRules(DateTime date, PeriodItemsGroupHead periodItemsGroupHead)
        {
            ScheduleRuleEvaluationItem currentDayScheduleRuleEvaluationItem = new ScheduleRuleEvaluationItem();
            var currentDate = date;
            var currentDay = GetEmployeePostDay(currentDate);

            if (periodItemsGroupHead != null && periodItemsGroupHead.CalculationPeriodItems != null && periodItemsGroupHead.CalculationPeriodItems.Count > 0)
            {
                var head = periodItemsGroupHead;
                currentDayScheduleRuleEvaluationItem = new ScheduleRuleEvaluationItem(currentDate.DayOfWeek, CalendarUtility.MergeDateAndTime(currentDate, head.StartTime, ignoreSqlServerDateTime: true), CalendarUtility.MergeDateAndTime(currentDate.Date, head.StopTime, ignoreSqlServerDateTime: true));
                return IsShiftValidAgainstScheduleCycleRules(GetPreviousShifts(currentDay.Date), currentDayScheduleRuleEvaluationItem);
            }

            return false;
        }

        #endregion

        #region Match

        public List<ScheduleCycleRuleTypeDTO> GetScheduleCycleRuleTypes()
        {
            List<ScheduleCycleRuleTypeDTO> availableScheduleCycleRuleTypes = new List<ScheduleCycleRuleTypeDTO>();

            if (this.EmployeePost != null && this.EmployeePost.ScheduleCycleDTO != null && this.EmployeePost.ScheduleCycleDTO.ScheduleCycleRuleDTOs != null)
            {
                foreach (var rule in this.EmployeePost.ScheduleCycleDTO.ScheduleCycleRuleDTOs.Where(x => x.ScheduleCycleRuleTypeDTO != null).ToList())
                    availableScheduleCycleRuleTypes.Add(rule.ScheduleCycleRuleTypeDTO);
            }

            return availableScheduleCycleRuleTypes;
        }

        public List<ScheduleCycleRuleTypeDTO> GetAvailableScheduleCycleRuleTypes(DayOfWeek dayOfWeek, List<ScheduleRuleEvaluationItem> previousShifts)
        {
            List<ScheduleCycleRuleTypeDTO> availableScheduleCycleRuleTypes = new List<ScheduleCycleRuleTypeDTO>();

            List<ScheduleRuleEvaluationItem> prevShifts = new List<ScheduleRuleEvaluationItem>();
            prevShifts.AddRange(previousShifts);

            if (this.EmployeePost != null && this.EmployeePost.ScheduleCycleDTO != null && this.EmployeePost.ScheduleCycleDTO.ScheduleCycleRuleDTOs != null)
            {
                foreach (var rule in this.EmployeePost.ScheduleCycleDTO.ScheduleCycleRuleDTOs.Where(x => x.ScheduleCycleRuleTypeDTO != null && x.ScheduleCycleRuleTypeDTO.DayOfWeekIds.Contains((int)dayOfWeek)).ToList())
                {
                    var matchShifts = prevShifts.Where(x => rule.Valid(x.DayOfWeek, x.StartTime, x.StopTime)).ToList();

                    if (matchShifts.Count > 0)
                    {
                        foreach (var item in matchShifts)
                        {
                            prevShifts.Remove(item);
                        }
                    }

                    int alreadyUsedCount = matchShifts.Count;
                    if (alreadyUsedCount < rule.MaxOccurrences)
                        availableScheduleCycleRuleTypes.Add(rule.ScheduleCycleRuleTypeDTO);
                }
            }

            return availableScheduleCycleRuleTypes;
        }

        public List<ScheduleRuleEvaluationItem> GetValidShifts(List<PeriodItemsGroupHead> periodItemsGroupHeads, ScheduleCycleRuleTypeDTO ruleTypeDTO)
        {
            List<ScheduleRuleEvaluationItem> dayScheduleRuleEvaluationItems = new List<ScheduleRuleEvaluationItem>();

            foreach (var head in periodItemsGroupHeads)
            {
                var currentDate = head.CalculationPeriodItems.FirstOrDefault().ScheduleDate;
                dayScheduleRuleEvaluationItems.Add(new ScheduleRuleEvaluationItem(currentDate.DayOfWeek, CalendarUtility.MergeDateAndTime(currentDate, head.StartTime, ignoreSqlServerDateTime: true), CalendarUtility.MergeDateAndTime(currentDate.Date, head.StopTime, ignoreSqlServerDateTime: true), head));
            }

            return GetValidShifts(dayScheduleRuleEvaluationItems, ruleTypeDTO);
        }


        public List<ScheduleRuleEvaluationItem> GetValidShifts(List<ScheduleRuleEvaluationItem> shifts, ScheduleCycleRuleTypeDTO ruleTypeDTO)
        {
            return shifts.Where(x => ruleTypeDTO.Valid(x.DayOfWeek, x.StartTime, x.StopTime)).ToList();
        }

        public List<Tuple<int, ScheduleCycleRuleTypeDTO, int, int>> GetScheduleCycleRuleTypes(List<ScheduleRuleEvaluationItem> cycleShifts)
        {
            List<Tuple<int, ScheduleCycleRuleTypeDTO, int, int>> unReachedScheduleCycleRuleTypes = new List<Tuple<int, ScheduleCycleRuleTypeDTO, int, int>>();

            if (this.EmployeePost != null && this.EmployeePost.ScheduleCycleDTO != null && this.EmployeePost.ScheduleCycleDTO.ScheduleCycleRuleDTOs != null)
            {
                foreach (var rule in this.EmployeePost.ScheduleCycleDTO.ScheduleCycleRuleDTOs.Where(x => x.ScheduleCycleRuleTypeDTO != null).ToList())
                {
                    int usedCount = cycleShifts.Count(x => rule.Valid(x.DayOfWeek, x.StartTime, x.StopTime));
                    unReachedScheduleCycleRuleTypes.Add(Tuple.Create(rule.MinOccurrences - usedCount, rule.ScheduleCycleRuleTypeDTO, usedCount, rule.MaxOccurrences));
                }
            }
            return unReachedScheduleCycleRuleTypes;
        }

        public List<Tuple<int, ScheduleCycleRuleTypeDTO, int, int>> GetUnReachedScheduleCycleRuleTypes(List<ScheduleRuleEvaluationItem> cycleShifts)
        {
            List<Tuple<int, ScheduleCycleRuleTypeDTO, int, int>> unReachedScheduleCycleRuleTypes = new List<Tuple<int, ScheduleCycleRuleTypeDTO, int, int>>();

            if (this.EmployeePost != null && this.EmployeePost.ScheduleCycleDTO != null && this.EmployeePost.ScheduleCycleDTO.ScheduleCycleRuleDTOs != null)
            {
                foreach (var rule in this.EmployeePost.ScheduleCycleDTO.ScheduleCycleRuleDTOs.Where(x => x.ScheduleCycleRuleTypeDTO != null).ToList())
                {
                    int usedCount = cycleShifts.Where(x => rule.Valid(x.DayOfWeek, x.StartTime, x.StopTime)).ToList().Count;
                    if (usedCount < rule.MinOccurrences)
                        unReachedScheduleCycleRuleTypes.Add(Tuple.Create(rule.MinOccurrences - usedCount, rule.ScheduleCycleRuleTypeDTO, usedCount, rule.MaxOccurrences));
                }
            }
            return unReachedScheduleCycleRuleTypes;
        }

        public List<Tuple<int, ScheduleCycleRuleTypeDTO, int, int>> GetUnMaxedScheduleCycleRuleTypes(List<ScheduleRuleEvaluationItem> cycleShifts)
        {
            List<Tuple<int, ScheduleCycleRuleTypeDTO, int, int>> unReachedScheduleCycleRuleTypes = new List<Tuple<int, ScheduleCycleRuleTypeDTO, int, int>>();

            if (this.EmployeePost != null && this.EmployeePost.ScheduleCycleDTO != null && this.EmployeePost.ScheduleCycleDTO.ScheduleCycleRuleDTOs != null)
            {
                foreach (var rule in this.EmployeePost.ScheduleCycleDTO.ScheduleCycleRuleDTOs.Where(x => x.ScheduleCycleRuleTypeDTO != null).ToList())
                {
                    int usedCount = cycleShifts.Where(x => rule.Valid(x.DayOfWeek, x.StartTime, x.StopTime)).ToList().Count;
                    if (usedCount < rule.MaxOccurrences)
                        unReachedScheduleCycleRuleTypes.Add(Tuple.Create(rule.MinOccurrences - usedCount, rule.ScheduleCycleRuleTypeDTO, usedCount, rule.MaxOccurrences));
                }
            }
            return unReachedScheduleCycleRuleTypes;
        }


        public List<Tuple<int, ScheduleCycleRuleTypeDTO>> GetReachedScheduleCycleRuleTypes(List<ScheduleRuleEvaluationItem> cycleShifts)
        {
            List<Tuple<int, ScheduleCycleRuleTypeDTO>> reachedScheduleCycleRuleTypes = new List<Tuple<int, ScheduleCycleRuleTypeDTO>>();

            if (this.EmployeePost != null && this.EmployeePost.ScheduleCycleDTO != null && this.EmployeePost.ScheduleCycleDTO.ScheduleCycleRuleDTOs != null)
            {
                foreach (var rule in this.EmployeePost.ScheduleCycleDTO.ScheduleCycleRuleDTOs.Where(x => x.ScheduleCycleRuleTypeDTO != null).ToList())
                {
                    int usedCount = cycleShifts.Where(x => rule.Valid(x.DayOfWeek, x.StartTime, x.StopTime)).ToList().Count;
                    if (usedCount >= rule.MaxOccurrences)
                        reachedScheduleCycleRuleTypes.Add(Tuple.Create(rule.MinOccurrences - usedCount, rule.ScheduleCycleRuleTypeDTO));
                }
            }
            return reachedScheduleCycleRuleTypes;
        }

        public void SelectBestFit()
        {
            var employeePost = this.EmployeePost;

            if (this.RemainingMinutes == 0)
                return;

            #region Försök hitta delar som summerar till veckoarbetstiden

            foreach (var week in this.EmployeePostWeeks)
            {
                if (week.Length > this.WorkTimeWeekMin && week.Length < this.WorkTimeWeekMax)
                    continue;

                int noOfWorkDays = 0;
                int minutesOfWork = 0;
                int remaingMinutesOfWork = employeePost.WorkTimeWeek;
                int iterations = 0;
                int maxIterations = 20;
                int preferedLenght = (int)(week.CalculationOptions.OptimalLength / 1.4);
                List<EmployeePostDay> selectedDays = new List<EmployeePostDay>();
                Dictionary<int, List<EmployeePostDay>> attempts = new Dictionary<int, List<EmployeePostDay>>();

                while (minutesOfWork != employeePost.WorkTimeWeek && iterations < maxIterations)
                {
                    minutesOfWork = 0;
                    List<EmployeePostDay> currentDays = new List<EmployeePostDay>();
                    remaingMinutesOfWork = employeePost.WorkTimeWeek;
                    int maxRestTime = 0;
                    DateTime maxRestTimeStarts = CalendarUtility.DATETIME_DEFAULT;

                    foreach (var employeePostDay in week.EmployeePostDays.OrderBy(k => k.Date))
                    {
                        //var heads = employeePostDay.PeriodItemsGroupHeads;
                        int restSincePrevDayBreachMinutes = 0;
                        int restToNextDayBreachMinutes = 0;

                        if (noOfWorkDays >= this.EmployeePost.WorkDaysWeek)
                            continue;

                        var heads = GetItemsValidAgainstScheduleCycleRules(employeePostDay.Date, employeePostDay.AllMatchedCalculationPeriodItems);

                        heads = HasAvailableScheduleCycleRuleTypes(employeePostDay.Date, heads);

                        if (heads.Count > 0)
                        {
                            var bestHead = this.GetBestPeriodItemsGroupHead(null, week, employeePostDay.Date, heads, false, preferedLenght);

                            ScheduleRuleEvaluationItem shiftToEvaluate = new ScheduleRuleEvaluationItem(employeePostDay.DayOfWeek, CalendarUtility.MergeDateAndTime(employeePostDay.Date, bestHead.StartTime, ignoreSqlServerDateTime: true), CalendarUtility.MergeDateAndTime(employeePostDay.Date, bestHead.StopTime, ignoreSqlServerDateTime: true));
                            bool isValid = IsDayValid(bestHead, employeePostDay.Date, false, out restSincePrevDayBreachMinutes, out restToNextDayBreachMinutes);

                            if (!isValid)
                            {

                                foreach (var head in employeePostDay.PeriodItemsGroupHeads.Where(l => l.Length == bestHead.Length))
                                {
                                    ScheduleRuleEvaluationItem item = new ScheduleRuleEvaluationItem(employeePostDay.DayOfWeek, CalendarUtility.MergeDateAndTime(employeePostDay.Date, head.StartTime, ignoreSqlServerDateTime: true), CalendarUtility.MergeDateAndTime(employeePostDay.Date, head.StopTime, ignoreSqlServerDateTime: true));
                                    bool itemIsValid = IsDayValid(head, employeePostDay.Date, false, out restSincePrevDayBreachMinutes, out restToNextDayBreachMinutes);

                                    if (itemIsValid)
                                    {
                                        isValid = true;
                                        bestHead = head;
                                        break;
                                    }
                                }

                                if (!isValid)
                                {
                                    foreach (var head in employeePostDay.PeriodItemsGroupHeads.OrderBy(l => l.Length))
                                    {
                                        ScheduleRuleEvaluationItem item = new ScheduleRuleEvaluationItem(employeePostDay.DayOfWeek, CalendarUtility.MergeDateAndTime(employeePostDay.Date, head.StartTime, ignoreSqlServerDateTime: true), CalendarUtility.MergeDateAndTime(employeePostDay.Date, head.StopTime, ignoreSqlServerDateTime: true));
                                        bool itemIsValid = IsDayValid(head, employeePostDay.Date, false, out restSincePrevDayBreachMinutes, out restToNextDayBreachMinutes);

                                        if (itemIsValid)
                                        {
                                            isValid = true;
                                            bestHead = head;
                                            break;
                                        }
                                    }
                                }
                            }

                            var minutesOnDay = bestHead.Length;
                            noOfWorkDays++;
                            minutesOfWork = minutesOfWork + minutesOnDay;
                            remaingMinutesOfWork = remaingMinutesOfWork - minutesOnDay;
                            employeePostDay.EmployeePost = employeePost;
                            employeePostDay.SelectedItemsHead = bestHead;
                            employeePostDay.SetValidRules(employeePost);
                            currentDays.Add(employeePostDay);
                        }
                    }

                    if (this.EvaluateRestTimeWeek(this.EmployeePost.EmployeeGroupDTO.RuleRestTimeWeek, GetWeekShifts(week.WeekNumber), week.StartDate, week.StartDate.AddDays(7), out maxRestTime, out maxRestTimeStarts))
                    {
                        if (minutesOfWork == employeePost.WorkTimeWeek)
                        {
                            selectedDays = currentDays;
                            iterations = maxIterations;
                            break;
                        }
                        else
                        {
                            if (attempts.Where(a => a.Key == Math.Abs(remaingMinutesOfWork)).Count() == 0)
                                attempts.Add(Math.Abs(remaingMinutesOfWork), currentDays);
                        }
                    }
                    else
                    {
                        iterations = iterations + 1 - 1;
                    }

                    noOfWorkDays = 0;
                    preferedLenght = preferedLenght + week.CalculationOptions.Interval;

                    iterations++;
                }


                if (selectedDays.Count == 0)
                {
                    var closest = attempts.OrderBy(a => a.Key).FirstOrDefault();
                    selectedDays = closest.Value;
                    noOfWorkDays = 0;
                }

                week.EmployeePostDays = selectedDays;
            }

            #endregion
        }


        #endregion

        #region Add

        public void MultipleWeeks(int maxNumberOfWeeks)
        {
            int numberOfWeeks = this.EmployeePostWeeks.Count;

            if (numberOfWeeks == maxNumberOfWeeks)
                return;

            if (numberOfWeeks == 0)
                return;

            int weeks = maxNumberOfWeeks / numberOfWeeks;
            int week = 1;
            List<EmployeePostWeek> newEmployeePostWeeks = new List<EmployeePostWeek>();

            while (week < weeks)
            {
                foreach (var employeePostWeek in this.EmployeePostWeeks)
                {
                    newEmployeePostWeeks.Add(employeePostWeek.Copy(addWeeks: numberOfWeeks * week, weekNumber: numberOfWeeks + week, employeePost: this.EmployeePost, setIsCopy: true));
                }
                week++;
            }

            this.EmployeePostWeeks.AddRange(newEmployeePostWeeks);
        }

        public bool TryAddAndValidateEmployeeDay(EmployeePostDay employeePostDay)
        {
            bool add = false;

            if (employeePostDay.ValidScheduleCycleRules.IsNullOrEmpty())
            {
                employeePostDay.ValidScheduleCycleRules = new List<ScheduleCycleRuleDTO>();
                add = true;
            }

            foreach (ScheduleCycleRuleDTO rule in employeePostDay.ValidScheduleCycleRules)
            {
                if (ValidScheduleCycleRule(rule))
                    add = true;
            }

            if (add)
            {
                employeePostDay.FilterAccordingToWorkRules();

                if (employeePostDay.AllMatchedCalculationPeriodItems.Any() && TryAddEmployeePostDay(employeePostDay))
                    return true;
            }

            return add;
        }

        public void SetRemainingFromPreviousWeeks(int weekNumber)
        {
            int remainingMinutes = 0;
            int remainingDays = 0;

            if (weekNumber > 1)
            {

                foreach (var week in this.EmployeePostWeeks.Where(w => w.WeekNumber < weekNumber))
                {
                    remainingMinutes = remainingMinutes + week.RemainingMinutesWeek;
                    remainingDays = remainingDays + week.RemainingDaysWeek;
                }

                var currentWeek = this.GetEmployeePostWeek(weekNumber);
                currentWeek.RemainingMinutesWeekFromPreviousWeek = remainingMinutes;
                currentWeek.RemainingDaysFromPreviousWeek = remainingDays > 1 ? 1 : remainingDays;

                if (remainingDays > 0)
                {
                    this.FreeDays = new List<DayOfWeek>();
                }
            }
        }

        public void SetRemainingNumberOfWeeks(int weekNumber)
        {
            var remainingWeeks = this.EmployeePostWeeks.Count - weekNumber;
            var currentWeek = this.GetEmployeePostWeek(weekNumber);
            currentWeek.RemainingNumberOfWeeks = remainingWeeks;
        }

        #endregion

        #region Get

        private EmployeePostWeek GetEmployeePostWeek(int weekNumber)
        {
            return this.EmployeePostWeeks.FirstOrDefault(i => i.WeekNumber == weekNumber);
        }

        private EmployeePostWeek GetEmployeePostWeek(DateTime date)
        {
            try
            {
                return this.EmployeePostWeeks.FirstOrDefault(i => i.StartDate == CalendarUtility.GetFirstDateOfWeek(date).Date);
            }
            catch
            {
                return new EmployeePostWeek();
            }

        }

        private List<EmployeePostWeek> GetPreviousEmployeePostWeeks(DateTime date)
        {
            try
            {
                return this.EmployeePostWeeks.Where(i => i.StartDate < CalendarUtility.GetFirstDateOfWeek(date).Date).ToList();
            }
            catch
            {
                return new List<EmployeePostWeek>();
            }

        }
        private EmployeePostWeek GetPreviousEmployeePostWeek(DateTime date)
        {
            try
            {
                return this.EmployeePostWeeks.OrderBy(o => o.StartDate).Where(i => i.StartDate < CalendarUtility.GetFirstDateOfWeek(date).Date).LastOrDefault();
            }
            catch
            {
                return new EmployeePostWeek();
            }

        }

        private List<EmployeePostWeek> GetPreviousEmployeePostWeeks(int weekNumber)
        {
            return this.EmployeePostWeeks.Where(i => i.WeekNumber < weekNumber).ToList();
        }

        public List<CalculationPeriodItem> GetSelectedPeriodItems(DateTime date)
        {
            return GetEmployeePostDay(date).SelectedItemsHead.CalculationPeriodItems;
        }

        public List<CalculationPeriodItem> GetPeriodItemsForPreviousOrFutureeWeekDay(int getWeekNumber, DayOfWeek dayOfWeek)
        {
            List<CalculationPeriodItem> periodItems = new List<CalculationPeriodItem>();

            EmployeePostWeek week = this.EmployeePostWeeks.Where(i => i.WeekNumber == getWeekNumber).FirstOrDefault();
            if (week == null)
                return periodItems;

            EmployeePostWeek newWeek = week.Copy();
            EmployeePostDay day = week.EmployeePostDays.Where(i => i.DayOfWeekId == (int)dayOfWeek).FirstOrDefault();
            if (day == null || day.SelectedItemsHead == null)
                return periodItems;

            return day.SelectedItemsHead.CalculationPeriodItems;
        }

        #endregion

        #region Logging

        private void InitInformation()
        {
            if (this.EmployeePostCycleInformation == null && this.EmployeePost != null)
                this.EmployeePostCycleInformation = new EmployeePostCycleInformation(this.EmployeePost.EmployeePostId, this.EmployeePost.Name);
            else if (this.EmployeePostCycleInformation == null)
                this.EmployeePostCycleInformation = new EmployeePostCycleInformation(0, "");
        }

        public void LogInformation()
        {
            this.InitInformation();

            this.EmployeePostCycleInformation.EmployeePostId = this.EmployeePost.EmployeePostId;
            this.EmployeePostCycleInformation.EmployeePostName = this.EmployeePost.Name;
            this.EmployeePostCycleInformation.Percent = this.Percent;
            this.EmployeePostCycleInformation.RemainingMinutes = this.RemainingMinutes;
            this.EmployeePostCycleInformation.Length = this.Length;
            this.EmployeePostCycleInformation.NumberOfEmployeePostDays = this.GetEmployeePostDays().Where(w => w.Length > 0).Count();

            foreach (var info in this.EmployeePostCycleInformation.EmployeePostWeekInformations)
            {
                info.EmployeePostId = this.EmployeePost.EmployeePostId;
                info.EmployeePostName = this.EmployeePost.Name;
            }
        }

        #endregion

        public void AddPreAnalysisInformation(EmployeePostDTO employeePost, List<CalculationPeriodItem> allEmployeePostPeriodItems)
        {
            this.PreAnalysisInformation.EmployeePost = employeePost;
            this.PreAnalysisInformation.AllEmployeePostPeriodItems = allEmployeePostPeriodItems;
            this.PreAnalysisInformation.RemainingEmployeePostPeriodItems = new List<CalculationPeriodItem>();
        }

        public void AddDaysPreAnalysisInformation(List<CalculationPeriodItem> remainingEmployeePostPeriodItems)
        {
            this.PreAnalysisInformation.RemainingEmployeePostPeriodItems.AddRange(remainingEmployeePostPeriodItems);
            foreach (var group in remainingEmployeePostPeriodItems.GroupBy(g => g.ScheduleDate))
            {
                this.PreAnalysisInformation.PreAnalysisInformationDays.Add(new PreAnalysisInformationDay(this.PreAnalysisInformation.EmployeePost, group.Key, group.ToList()));
            }
        }
        public void AddDaysPreAnalysisInformationDayShift(EmployeePostDay employeePostDay, bool disposed, string disposeReason, int preferredLength)
        {
            AddDaysPreAnalysisInformationDayShift(employeePostDay.Date, disposed, disposeReason, employeePostDay.ShiftTypeSkillDTOs, employeePostDay.StartTime, employeePostDay.StopTime, preferredLength, employeePostDay.Length, employeePostDay.BreakLength);
        }

        public void AddDaysPreAnalysisInformationDayShift(DateTime date, bool disposed, string disposeReason, List<ShiftTypeSkillDTO> shiftTypeSkillDTOs, DateTime start, DateTime stop, int preferredLength, int length, int breakLength)
        {
            if (this.PreAnalysisInformation?.PreAnalysisInformationDays != null)
            {
                var day = this.PreAnalysisInformation.PreAnalysisInformationDays.FirstOrDefault(f => f.Date == date);

                if (day != null)
                {
                    if (day.PreAnalysysInformationDayShifts != null)
                    {
                        day.PreAnalysysInformationDayShifts.Add(new PreAnalysysInformationDayShift(disposed, disposeReason, shiftTypeSkillDTOs, start, stop, preferredLength, length, breakLength));
                    }
                }
            }
        }

        #endregion

        #region Help

        private List<EmployeePostDay> GetEmployeePostDays()
        {
            List<EmployeePostDay> employeePostDays = new List<EmployeePostDay>();

            foreach (var week in this.EmployeePostWeeks)
                foreach (var day in week.EmployeePostDays)
                    employeePostDays.Add(day);

            return employeePostDays;
        }

        private List<EmployeePostDay> GetEmployeePostDaysInWeek(int weekNumber)
        {
            List<EmployeePostDay> employeePostDays = new List<EmployeePostDay>();

            foreach (var week in this.EmployeePostWeeks.Where(i => i.WeekNumber == weekNumber))
                foreach (var day in week.EmployeePostDays)
                    employeePostDays.Add(day);

            return employeePostDays;
        }

        public EmployeePostDay GetEmployeePostDay(DateTime date)
        {
            var dateInCycle = date;// GetDateForCycle(date);

            foreach (var week in this.EmployeePostWeeks)
                foreach (var day in week.EmployeePostDays)
                {
                    if (day.Date == dateInCycle)
                        return day;
                }

            return new EmployeePostDay();
        }

        private DateTime GetDateForCycle(DateTime date)
        {
            int nbrOfWeeks = this.EmployeePostWeeks.Count;
            DateTime startDate = this.StartDate;

            if (date < startDate.AddDays(nbrOfWeeks * 7))
                return date;
            else
            {
                DateTime currentDate = startDate;
                int day = 0;
                List<Tuple<DateTime, int>> dateTuple = new List<Tuple<DateTime, int>>();

                while (currentDate <= date)
                {
                    day++;

                    if (day > this.NbrOfDays)
                        day = 1;

                    dateTuple.Add(Tuple.Create(currentDate, day));
                    currentDate = currentDate.AddDays(1);
                }

                return dateTuple.FirstOrDefault(i => i.Item2 == day).Item1;

            }
        }

        private bool ValidScheduleCycleRule(ScheduleCycleRuleDTO scheduleCycleRuleDTO)
        {
            foreach (var week in this.EmployeePostWeeks)
            {
                if (!week.ValidScheduleCycleRule(scheduleCycleRuleDTO))
                    return false;
            }

            return true;
        }

        private bool TryAddEmployeePostDay(EmployeePostDay employeePostDay)
        {
            #region find week

            DateTime monday = CalendarUtility.GetFirstDateOfWeek(employeePostDay.Date);

            EmployeePostWeek employeePostWeek = this.EmployeePostWeeks.Where(i => i.StartDate == monday).FirstOrDefault();

            if (employeePostWeek != null)
            {
                var existing = employeePostWeek.EmployeePostDays.FirstOrDefault(f => f.DayOfWeek == employeePostDay.DayOfWeek);

                if (existing != null)
                    employeePostWeek.EmployeePostDays.Remove(existing);

                employeePostWeek.EmployeePostDays.Add(employeePostDay);
            }

            #endregion

            return true; //TODO Some validation
        }

        public bool TryRemoveEmployeePostDay(EmployeePostDay employeePostDay)
        {
            #region find week

            DateTime monday = CalendarUtility.GetFirstDateOfWeek(employeePostDay.Date);

            EmployeePostWeek employeePostWeek = this.EmployeePostWeeks.Where(i => i.StartDate == monday).FirstOrDefault();

            if (employeePostWeek != null)
                employeePostWeek.EmployeePostDays.Remove(employeePostDay);

            #endregion

            return true; //TODO Some validation
        }


        #endregion
    }

    public class EmployeePostWeek
    {
        #region Properties

        public EmployeePostWeekInformation EmployeePostWeekInformation { get; set; }

        public bool IsCopy { get; set; }
        public bool Match { get; set; }
        public DateTime StartDate { get; set; }
        public int WeekNumber { get; set; }
        public int DiffMinutesInLastAttempt { get; set; }
        public int RemainingMinutesWeekFromPreviousWeek { get; set; }
        public int RemainingExtraMinutesOnWeek { get; set; }
        public int RemainingDaysFromPreviousWeek { get; set; }
        public int RemainingNumberOfWeeks { get; set; }

        public int RemainingMinutesWeek
        {
            get
            {
                var sum = this.EmployeePostDays.Sum(s => s.Length);

                if (this.EmployeePost != null)
                    return this.EmployeePost.WorkTimeWeek - sum;
                else
                    return 0;
            }
        }

        public int RemainingMinutesWeekToMin
        {
            get
            {
                var sum = this.EmployeePostDays.Sum(s => s.Length);

                return this.WorkTimeWeekMin - sum;
            }
        }

        public int RemainingMinutesWeekToMax
        {
            get
            {
                var sum = this.EmployeePostDays.Sum(s => s.Length);

                return this.WorkTimeWeekMax - sum;
            }
        }

        public int RemainingMinutesWeekInclPreviousWeek
        {
            get
            {
                return this.RemainingMinutesWeek + this.RemainingMinutesWeekFromPreviousWeek + this.RemainingExtraMinutesOnWeek;
            }
        }

        public int RemainingDaysWeek
        {
            get
            {
                int nrOfDays = this.EmployeePostDays.Count(t => t.Length > 0);
                List<int> dayOfWeekIds = this.EmployeePost.ScheduleCycleDTO.GetValidWeekDayIds();
                foreach (int day in this.EmployeePost.DayOfWeekIds)
                {
                    dayOfWeekIds.Remove(day);
                }

                if (dayOfWeekIds.Count < nrOfDays)
                    nrOfDays = dayOfWeekIds.Count;

                return this.EmployeePost.RemainingWorkDaysWeek - nrOfDays;
            }
        }
        public int Length
        {
            get
            {
                int length = 0;

                if (this.EmployeePostDays != null && this.EmployeePostDays.Count > 0)
                {
                    foreach (var day in this.EmployeePostDays)
                    {
                        length += day.Length;
                    }
                }
                return length;
            }
        }

        public int UsedMinutes
        {
            get
            {
                return this.EmployeePost.WorkTimeWeek - this.RemainingMinutesWeekInclPreviousWeek;
            }
        }

        public int WorkTimeWeekMax
        {
            get
            {
                if (this.EmployeePost.WorkTimeWeek == this.EmployeePost.EmployeeGroupDTO.RuleWorkTimeWeek)
                    return this.EmployeePost.EmployeeGroupDTO.RuleWorkTimeWeek + this.EmployeePost.EmployeeGroupDTO.MaxScheduleTimeFullTime;
                else
                    return this.EmployeePost.EmployeeGroupDTO.RuleWorkTimeWeek + this.EmployeePost.EmployeeGroupDTO.MaxScheduleTimePartTime;
            }
        }

        public int WorkTimeWeekMin
        {
            get
            {
                if (this.EmployeePost.WorkTimeWeek == this.EmployeePost.EmployeeGroupDTO.RuleWorkTimeWeek)
                    return this.EmployeePost.EmployeeGroupDTO.RuleWorkTimeWeek - this.EmployeePost.EmployeeGroupDTO.MinScheduleTimeFullTime;
                else
                    return this.EmployeePost.EmployeeGroupDTO.RuleWorkTimeWeek - this.EmployeePost.EmployeeGroupDTO.MinScheduleTimePartTime;
            }
        }

        public int MinutesPerDayRemainingFromPreviousWeek(bool deductedWeek)
        {
            if (!deductedWeek)
                return Math.Abs(RemainingMinutesWeekFromPreviousWeek);

            var perday = this.EmployeePost.WorkTimePerDay;

            return perday - Math.Abs(RemainingMinutesWeekFromPreviousWeek);
        }

        public bool IsEvenWeek
        {
            get
            {
                int weekNumber = CalendarUtility.GetWeekNr(this.StartDate);

                if ((weekNumber % 2) == 0)
                    return true;

                return false;
            }
        }

        public int NbrOfHoles
        {
            get
            {
                if (this.EmployeePostDays.Count > 0)
                {
                    int holes = 0;
                    int currentHole = 0;

                    foreach (var day in CalendarUtility.GetWeekDaysList())
                    {
                        if (this.EmployeePostDays.Where(w => w.DayOfWeek == day).Count() == 0)
                        {
                            if (!this.EmployeePost.DayOfWeekIds.Contains((int)day))
                            {
                                currentHole++;
                                continue;
                            }
                        }
                        else
                        {
                            if (currentHole > 0)
                            {
                                holes++;
                                currentHole = 0;
                            }
                        }
                    }

                    //if it ends on free days on weekend that is not a hole..
                    //if (currentHole > 0)
                    //    holes++;

                    return holes;
                }
                else return 0;
            }
        }

        public int WeekEndDays()
        {
            return this.EmployeePostDays.Where(i => i.DayOfWeek == DayOfWeek.Saturday || i.DayOfWeek == DayOfWeek.Sunday).Count();

        }

        public EmployeePostDTO EmployeePost { get; set; }
        public CalculationOptions CalculationOptions { get; set; }
        public List<EmployeePostDay> EmployeePostDays { get; set; }

        public List<EmployeePostDay> PurgedEmployeePostDaysWithUniqueSkills { get; set; }

        public List<EmployeePostDay> PurgedEmployeePostDaysWithPrioSkills { get; set; }
        public bool RecheckPreviousWeek { get; set; }
        public List<EmployeePostDay> PurgedEmployeePostDaysInEndOfWeek { get; set; }

        public List<DayOfWeek> FreeDays { get; set; }
        public List<DayOfWeek> HandledDays { get; set; }
        public List<DayOfWeek> HandledButEmptyDays { get; set; }

        public List<DayOfWeek> DayOfWeekOrder { get; set; }

        public bool SunDayhandledWithNoTime
        {
            get
            {
                if (this.HandledDays.Where(s => s == DayOfWeek.Sunday).Any())
                {
                    var sunday = this.EmployeePostDays.Where(i => i.DayOfWeek == DayOfWeek.Sunday).FirstOrDefault();

                    if (sunday != null && sunday.Length > 0)
                        return false;
                    else
                        return true;
                }

                return false;
            }
        }

        public bool SaturDayhandledWithNoTime
        {
            get
            {
                if (this.HandledDays.Where(s => s == DayOfWeek.Saturday).Any())
                {
                    var saturday = this.EmployeePostDays.Where(i => i.DayOfWeek == DayOfWeek.Saturday).FirstOrDefault();

                    if (saturday != null && saturday.Length > 0)
                        return false;
                    else
                        return true;
                }

                return false;
            }
        }

        public decimal Percent
        {
            get
            {
                var workWeekTime = this.EmployeePost.WorkTimeWeek;

                if (workWeekTime > 0)
                    return ((decimal.Divide((workWeekTime - this.RemainingMinutesWeekInclPreviousWeek), workWeekTime)) * 100);
                else
                    return 0;
            }
        }

        public bool MeetPercentGoal
        {
            get
            {
                return this.Percent < 95;
            }
        }

        public bool IsWeekendWeek { get; set; }

        #endregion

        #region Ctor

        public EmployeePostWeek(EmployeePostDTO employeePost = null)
        {
            this.EmployeePostDays = new List<EmployeePostDay>();
            this.FreeDays = new List<DayOfWeek>();
            this.HandledDays = new List<DayOfWeek>();
            this.DayOfWeekOrder = new List<DayOfWeek>();
            this.HandledButEmptyDays = new List<DayOfWeek>();
            this.EmployeePost = employeePost;
            this.InitInformation();
        }

        public EmployeePostWeek(int weekNumber, DateTime startDate, List<EmployeePostDay> employeePostDays, EmployeePostDTO employeePostDTO, CalculationOptions calculationOptions)
        {
            this.WeekNumber = weekNumber;
            this.StartDate = startDate;
            this.EmployeePostDays = new List<EmployeePostDay>();
            this.EmployeePostDays.AddRange(employeePostDays);
            this.EmployeePost = employeePostDTO;
            this.CalculationOptions = calculationOptions;
            this.FreeDays = new List<DayOfWeek>();
            this.HandledDays = new List<DayOfWeek>();
            this.DayOfWeekOrder = new List<DayOfWeek>();
            this.HandledButEmptyDays = new List<DayOfWeek>();
            this.InitInformation();

            FilterAccordingToWorkRules();
        }

        #endregion

        #region Public methods

        #region Validate

        public bool ValidScheduleCycleRule(ScheduleCycleRuleDTO scheduleCycleRuleDTO)
        {
            if (this.EmployeePost != null && this.EmployeePost.ScheduleCycleDTO != null && this.EmployeePost.ScheduleCycleDTO.ScheduleCycleRuleDTOs != null)
            {
                List<ScheduleCycleRuleDTO> rules = this.EmployeePost.ScheduleCycleDTO.ScheduleCycleRuleDTOs;
                List<ScheduleCycleRuleDTO> usedRules = new List<ScheduleCycleRuleDTO>();

                foreach (var day in this.EmployeePostDays)
                    usedRules.AddRange(day.ValidScheduleCycleRules);

                foreach (var rule in rules.Where(i => i.ScheduleCycleRuleId == scheduleCycleRuleDTO.ScheduleCycleRuleId))
                {
                    int min = rule.MinOccurrences;
                    int max = rule.MaxOccurrences;

                    var alreadyUsed = usedRules.Where(i => i.ScheduleCycleRuleTypeId == rule.ScheduleCycleRuleTypeId).ToList();
                    if (alreadyUsed.Count >= min && alreadyUsed.Count <= max)
                        return true;
                    else
                        return false;
                }
            }

            return true;
        }

        public DateTime GetFirstPossibleStart(DateTime dateTime)
        {
            var lastTime = GetLastStopDate(dateTime);



            if (lastTime == DateTime.MinValue)
                return CalendarUtility.DATETIME_DEFAULT;

            if (this.CalculationOptions == null || this.CalculationOptions.EmployeePost == null || this.CalculationOptions.EmployeePost.EmployeeGroupDTO == null)
                return dateTime;

            return lastTime.AddMinutes(this.CalculationOptions.EmployeePost.EmployeeGroupDTO.RuleRestTimeDay);
        }

        public DateTime GetLastPossibleStop(DateTime dateTime)
        {
            var nextStart = GetNextStartDate(dateTime);

            if (nextStart == DateTime.MaxValue)
                return DateTime.MaxValue;

            if (this.CalculationOptions == null || this.CalculationOptions.EmployeePost == null || this.CalculationOptions.EmployeePost.EmployeeGroupDTO == null)
                return dateTime;

            return nextStart.AddMinutes(-this.CalculationOptions.EmployeePost.EmployeeGroupDTO.RuleRestTimeDay);
        }

        #endregion

        #region Match

        public int GetNumberOfWorkDaysWeek()
        {
            if (this.EmployeePost?.EmployeeGroupDTO != null)
            {
                var possibleDaysBasedOnlyOnMinTime = Convert.ToInt32(this.EmployeePost.WorkTimeWeek / this.CalculationOptions.MinLength);

                if (this.EmployeePost.WorkDaysWeek >= possibleDaysBasedOnlyOnMinTime)
                    return possibleDaysBasedOnlyOnMinTime;

                var minDaysBasedOnlyOnMaxTime = 1 + Convert.ToInt32(this.EmployeePost.WorkTimeWeek / this.CalculationOptions.MaxLength);

                if (this.EmployeePost.WorkDaysWeek <= minDaysBasedOnlyOnMaxTime)
                    return minDaysBasedOnlyOnMaxTime;
            }

            return this.EmployeePost.WorkDaysWeek;
        }

        public List<int> GetDayOfWeeksIds()
        {
            List<int> dayOfWeekIds = this.EmployeePost.ScheduleCycleDTO.GetValidWeekDayIds();
            dayOfWeekIds = dayOfWeekIds.Where(d => !this.FreeDays.Select(i => (int)i).Contains(d)).ToList();

            if (!IsWeekendWeek)
                dayOfWeekIds = dayOfWeekIds.Where(i => (DayOfWeek)i != DayOfWeek.Sunday && (DayOfWeek)i != DayOfWeek.Saturday).ToList();

            return dayOfWeekIds;
        }

        public int GetNumberOfPossibleWorkDaysLeftInWeek(DayOfWeek dayOfWeek, EmployeePostCycle employeePostCycle)
        {
            List<int> dayOfWeekIds = GetDayOfWeeksIds();

            List<DayOfWeek> remainingDays = new List<DayOfWeek>();
            bool add = false;
            foreach (var day in this.DayOfWeekOrder)
            {
                if (day == dayOfWeek)
                    add = true;

                if (add && dayOfWeekIds.Contains((int)day))
                    remainingDays.Add(day);
            }

            int count = remainingDays.Count;
            if (this.EmployeePostDays == null)
                this.EmployeePostDays = new List<EmployeePostDay>();
            int daysUsed = this.EmployeePostDays.Where(t => t.Length > 0).Count();
            int workDaysWeek = GetNumberOfWorkDaysWeek() + this.RemainingDaysFromPreviousWeek;

            bool minTimeAdjusted = false;
            while (!minTimeAdjusted && count > 0)
            {
                if (count == 1 && this.CalculationOptions.MinLength > this.RemainingMinutesWeekInclPreviousWeek)
                {
                    count = 0;
                }
                else if (this.CalculationOptions.MinLength * count > this.RemainingMinutesWeekInclPreviousWeek)
                {
                    bool minus = true;
                    if (employeePostCycle.EmployeePostWeeks.Count == WeekNumber)
                    {
                        minus = true;
                    }
                    else if (WeekNumber == 1 && employeePostCycle.EmployeePostWeeks.Count > 1)
                    {
                        if (count == 1 && this.CalculationOptions.MinLength * count > this.RemainingMinutesWeekToMax)
                            minus = false;
                    }
                    else if (employeePostCycle.EmployeePostWeeks.Count > 1 && employeePostCycle.EmployeePostWeeks.Count - 1 <= WeekNumber)
                    {
                        if (count == 1 && this.CalculationOptions.MinLength * count > this.RemainingMinutesWeekToMax / 2)
                            minus = false;
                    }
                    if (minus)
                        count--;
                }
                else
                {
                    minTimeAdjusted = true;
                }
            }

            if (count > workDaysWeek - daysUsed)
                count = workDaysWeek - daysUsed;

            if (WeekWithAddedExtraDayOfWork(employeePostCycle) && this.RemainingDaysFromPreviousWeek <= 0)
            {
                if (daysUsed <= workDaysWeek + 1)
                    count++; // Added day in order for 6 days of work on weekend week to happen more offen.
            }
            else if (workDaysWeek > 3 && WeekWithDeductedDayOfWork(employeePostCycle) && this.RemainingDaysFromPreviousWeek >= 0)
            {
                if ((dayOfWeekIds.Count >= workDaysWeek || this.DiffMinutesInLastAttempt != 0) && this.RemainingDaysFromPreviousWeek >= 0 && count > 0 && daysUsed <= workDaysWeek)
                    count--;
            }

            return count;
        }

        public bool WeekWithAddedExtraDayOfWork(EmployeePostCycle employeePostCycle)
        {
            if (!employeePostCycle.EmployeePost.ScheduleCycleDTO.HasWeekends())
                return false;

            var add = IsWeekendWeek && this.EmployeePost.HasMinMaxTimeSpan && (EmployeePost.WorkTimePercent > 70 || (EmployeePost.WorkTimePercent >= 50 && GetNumberOfWorkDaysWeek() == 5)) && GetNumberOfWorkDaysWeek() > 3 && this.RemainingDaysFromPreviousWeek >= 0; //this.RemainingNumberOfWeeks > 0;

            if (add)
            {
                add = !employeePostCycle.EmployeePost.ScheduleCycleDTO.OnlyHasOneWeekEndDayPerWeek(EmployeePost);

                if (add)
                {
                    var previousShifts = employeePostCycle.GetPreviousShifts(this.StartDate, this.StartDate);
                    var sunday = employeePostCycle.GetAvailableScheduleCycleRuleTypes(DayOfWeek.Sunday, previousShifts);
                    var saturday = employeePostCycle.GetAvailableScheduleCycleRuleTypes(DayOfWeek.Saturday, previousShifts);

                    if (saturday.Count > 0)
                    {
                        var type = employeePostCycle.GetScheduleCycleRuleTypes().FirstOrDefault(f => f.ScheduleCycleRuleTypeId == saturday.First().ScheduleCycleRuleTypeId);

                        if (type != null && type.DayOfWeekIds.Where(w => (DayOfWeek)w == DayOfWeek.Saturday || (DayOfWeek)w == DayOfWeek.Sunday).Count() == 1)
                            saturday = saturday.Where(w => !sunday.Select(s => s.ScheduleCycleRuleTypeId).Contains(w.ScheduleCycleRuleTypeId)).ToList();
                    }

                    if (sunday.Count > 0 && saturday.Count > 0 &&
                        employeePostCycle.InitialValidItems.Any(w => w.Date.HasValue && w.Date.Value.DayOfWeek == DayOfWeek.Sunday) &&
                        employeePostCycle.InitialValidItems.Any(w => w.Date.HasValue && w.Date.Value.DayOfWeek == DayOfWeek.Saturday))
                    {
                        add = true;
                    }
                    else
                        add = false;
                }
            }

            if (add)
            {
                decimal factor = new decimal(1);
                if (EmployeePost.EmployeeGroupDTO.MaxScheduleTimeFullTime > 0 && EmployeePost.IsFullTime())
                    factor = decimal.Divide(EmployeePost.WorkTimePerDay, EmployeePost.EmployeeGroupDTO.MaxScheduleTimeFullTime);
                else if (EmployeePost.EmployeeGroupDTO.MaxScheduleTimePartTime > 0)
                    factor = decimal.Divide(EmployeePost.WorkTimePerDay, EmployeePost.EmployeeGroupDTO.MaxScheduleTimePartTime);

                if (factor > 1)
                    return false;
            }

            return add;
        }

        public bool WeekWithDeductedDayOfWork(EmployeePostCycle employeePostCycle)
        {
            if (!employeePostCycle.EmployeePost.ScheduleCycleDTO.HasWeekends())
                return false;

            var deduct = this.EmployeePost.HasMinMaxTimeSpan && !this.IsWeekendWeek && !employeePostCycle.IgnoreFreeDays && (EmployeePost.WorkTimePercent > 70 || (EmployeePost.WorkTimePercent >= 50 && GetNumberOfWorkDaysWeek() == 5)) && GetNumberOfWorkDaysWeek() > 3 && !employeePostCycle.EmployeePost.ScheduleCycleDTO.OnlyHasOneWeekEndDayPerWeek(EmployeePost) && (RemainingDaysFromPreviousWeek < 0 || (RemainingDaysFromPreviousWeek <= 0 && employeePostCycle.PreviousWeekWasWeekendWeek(WeekNumber, IsWeekendWeek)));

            if (deduct)
            {
                decimal factor = new decimal(-1);
                if (EmployeePost.EmployeeGroupDTO.MinScheduleTimeFullTime < 0 && EmployeePost.IsFullTime())
                    factor = decimal.Divide(EmployeePost.WorkTimePerDay, EmployeePost.EmployeeGroupDTO.MinScheduleTimeFullTime);
                else if (EmployeePost.EmployeeGroupDTO.MinScheduleTimePartTime < 0)
                    factor = decimal.Divide(EmployeePost.WorkTimePerDay, EmployeePost.EmployeeGroupDTO.MinScheduleTimePartTime);

                if (factor < -1)
                    return false;
            }

            return deduct;

            //return employeePostCycle.NextWeekIsWeekendWeek(this.WeekNumber) && this.RemainingDaysFromPreviousWeek <= 0; //this.RemainingNumberOfWeeks > 0;
        }

        public int PreferredDayLength(DayOfWeek dayOfWeek, EmployeePostCycle employeePostCycle)
        {
            int length = 0;
            int possibleDays = GetNumberOfPossibleWorkDaysLeftInWeek(dayOfWeek, employeePostCycle);

            if (possibleDays == 0)
                return 0;

            if (possibleDays == 1)
            {
                length = this.RemainingMinutesWeekInclPreviousWeek;
            }
            else
            {
                //var nextDayRemainingGoal = (this.RemainingMinutesWeekInclPreviousWeek / (possibleDays + 1)) * possibleDays-1;
                //length = this.RemainingMinutesWeekInclPreviousWeek - nextDayRemainingGoal;
                length = this.RemainingMinutesWeekInclPreviousWeek / possibleDays;
            }

            if (this.RemainingMinutesWeekInclPreviousWeek > RemainingMinutesWeekToMax)
                length = RemainingMinutesWeekToMax / possibleDays;

            if (DiffMinutesInLastAttempt != 0 && (decimal)GetNumberOfPossibleWorkDaysLeftInWeek(dayOfWeek, employeePostCycle) <= decimal.Divide((decimal)GetNumberOfWorkDaysWeek(), new decimal(2.5)))
                length += -DiffMinutesInLastAttempt;

            if (length < this.CalculationOptions.MinLength)
                length = this.CalculationOptions.MinLength;

            if (length > this.CalculationOptions.MaxLength)
                length = this.CalculationOptions.MaxLength;

            if (length > this.EmployeePost.WorkTimeCycle || length + this.CalculationOptions.MinLength > this.EmployeePost.WorkTimeCycle)
                length = this.EmployeePost.WorkTimeCycle;

            if (SetHalf)
                length = Convert.ToInt32(decimal.Divide(length, 2));

            return CalendarUtility.AdjustAccordingToInterval(length, this.CalculationOptions.Interval, alwaysReduce: true);
        }

        public bool SetHalf { get; set; }

        public int PreferredDayLength_old(DayOfWeek dayOfWeek, EmployeePostCycle employeePostCycle)
        {
            int possibleDays = GetNumberOfPossibleWorkDaysLeftInWeek(dayOfWeek, employeePostCycle);

            if (possibleDays == 0)
                return 0;

            int length = this.RemainingMinutesWeekInclPreviousWeek / (possibleDays != 0 ? possibleDays : this.EmployeePost.RemainingWorkDaysWeek);

            if (length < this.CalculationOptions.MinLength)
                length = this.CalculationOptions.MinLength;

            if ((length * possibleDays) + this.Length > this.EmployeePost.WorkTimeWeek)
            {
                int days = (int)(Decimal.Round(this.EmployeePost.WorkTimeWeek / length));
                this.EmployeePost.RemainingWorkDaysWeek = days;
                length = this.RemainingMinutesWeekInclPreviousWeek / possibleDays;
            }

            //if (length < this.EmployeePost.WorkTimeWeek / this.EmployeePost.WorkDaysWeek && !this.forcePreferredDayLength(length, dayOfWeek))
            //{
            //    if (this.UsedMinutes > this.EmployeePost.WorkTimePerDay + this.HandledDays.Count)
            //    {
            //        length = (this.Length * possibleDays) / this.RemainingDaysWeek - 1;
            //    }
            //}
            //else
            if (length < this.EmployeePost.WorkTimeWeek / this.EmployeePost.WorkDaysWeek && !this.ForcePreferredDayLength(length, dayOfWeek, employeePostCycle))
            {
                var days = this.EmployeePostDays.Where(i => i.Length > 0);

                if (days.Any())
                {
                    var avarageDayMinutes = this.UsedMinutes / days.Count();

                    if (avarageDayMinutes < this.EmployeePost.WorkTimePerDay)
                    {
                        length = length + this.EmployeePost.WorkTimePerDay - avarageDayMinutes;
                    }
                }
            }

            if (length < this.CalculationOptions.MinLength)
                length = this.CalculationOptions.MinLength;

            if (length > this.CalculationOptions.MaxLength)
                length = this.CalculationOptions.MaxLength;

            return CalendarUtility.AdjustAccordingToInterval(length, this.CalculationOptions.Interval, alwaysReduce: true);
        }

        public bool ForcePreferredDayLength(int length, DayOfWeek dayOfWeek, EmployeePostCycle employeePostCycle)
        {
            if (GetNumberOfPossibleWorkDaysLeftInWeek(dayOfWeek, employeePostCycle) > 1)
                return (this.RemainingMinutesWeek - length < this.CalculationOptions.MinLength);
            else
                return false;
        }

        #endregion

        #region Add

        public void AddFreeDaysOnNoWeekendWeek(List<DayOfWeek> dayOfWeeksBasedOnSkills, Dictionary<DayOfWeek, int> remainingOnDays, EmployeePostWeek employeePostWeek, EmployeePostCycle employeePostCycle, EmployeePostCyclesRun currentEmployeePostCyclesRun, List<CalculationPeriodItem> calculationPeriodItems)
        {
            if (this.FreeDays.Count > 0)
                return;

            var usedDays = remainingOnDays.Where(w => w.Key == DayOfWeek.Monday || w.Key == DayOfWeek.Tuesday || w.Key == DayOfWeek.Wednesday);

            var sorted = usedDays.Where(w => w.Value > 0).OrderBy(o => o.Value);

            if (sorted.Any())
            {
                var onDay = calculationPeriodItems.Where(w => w.Weekday == sorted.First().Key).ToList();
                if (onDay.Count == 0 || !onDay.Any(a => currentEmployeePostCyclesRun.IsUnique(a)))
                {
                    this.FreeDays.Add(sorted.First().Key);
                }
                return;
            }

            if (dayOfWeeksBasedOnSkills.Count > 0)
            {
                dayOfWeeksBasedOnSkills.Reverse();
                dayOfWeeksBasedOnSkills.Take(3);

                foreach (var day in dayOfWeeksBasedOnSkills)
                {
                    if (day == DayOfWeek.Wednesday || day == DayOfWeek.Tuesday)
                    {
                        this.FreeDays.Add(day);
                        return;
                    }
                }
            }

            var previousEmployeePostWeek = employeePostCycle.GetPreviousEmployeeWeek(employeePostWeek.WeekNumber);

            if (previousEmployeePostWeek != null && previousEmployeePostWeek.EmployeePostDays.Where(w => w.DayOfWeek == DayOfWeek.Sunday || w.DayOfWeek == DayOfWeek.Saturday).Count() == 2)
                this.FreeDays.Add(DayOfWeek.Monday);
            else if (this.EmployeePost.WorkDaysWeek <= 3 || this.EmployeePost.WorkTimePercent < 70)
                this.FreeDays.Add(DayOfWeek.Tuesday);
            else
                this.FreeDays.Add(DayOfWeek.Wednesday);
        }
        #endregion

        #region Filter

        public void FilterAccordingToWorkRules()
        {
            FilterAccordingToWorkRulesMaxTime();
            FilterAccordingToWorkRulesMinTime();
            FilterAccordingToDayRest();
            FilterAccordingToWeekRest();
        }

        public void FilterAccordingToDayRest()
        {
        }
        public void FilterAccordingToWeekRest()
        {
        }

        public void FilterAccordingToWorkRulesMaxTime()
        {
            List<EmployeePostDay> filteredDays = new List<EmployeePostDay>();
            this.EmployeePostDays = this.EmployeePostDays.Where(d => d.Length < this.CalculationOptions.GetMaxLenght()).ToList();
        }

        public void FilterAccordingToWorkRulesMinTime()
        {
            List<EmployeePostDay> filteredDays = new List<EmployeePostDay>();
            this.EmployeePostDays = this.EmployeePostDays.Where(d => d.Length > this.CalculationOptions.MinLength).ToList();
        }
        #endregion

        #region Find

        public DateTime GetLastStopDate(DateTime date)
        {
            if (this.EmployeePostDays == null || this.EmployeePostDays.Count == 0)
                return DateTime.MinValue;


            var last = this.EmployeePostDays.Where(i => i.Date < date).OrderBy(o => o.Date).LastOrDefault();

            if (last != null)
                return CalendarUtility.MergeDateAndTime(last.Date, last.StopTime, ignoreSqlServerDateTime: true);
            else
                return DateTime.MinValue;
        }

        public DateTime GetNextStartDate(DateTime date)
        {
            if (this.EmployeePostDays == null || this.EmployeePostDays.Count == 0)
                return DateTime.MaxValue;

            var next = this.EmployeePostDays.Where(i => i.Date > date).OrderBy(o => o.Date).FirstOrDefault();

            if (next != null)
                return CalendarUtility.MergeDateAndTime(next.Date, next.StartTime, ignoreSqlServerDateTime: true);
            else
                return DateTime.MaxValue;
        }

        public List<CalculationPeriodItem> GetAllMatchedCalculationPeriodItems()
        {
            List<CalculationPeriodItem> list = new List<CalculationPeriodItem>();
            foreach (var day in this.EmployeePostDays)
                list.AddRange(day.AllMatchedCalculationPeriodItems);

            return list;
        }


        #endregion

        #region Copy

        public EmployeePostWeek Copy(int addWeeks = 1, int weekNumber = 0, EmployeePostDTO employeePost = null, bool setIsCopy = false)
        {
            List<EmployeePostDay> newEmployeePostDays = new List<EmployeePostDay>();
            DateTime startDate = this.StartDate.AddDays(7 * addWeeks);

            foreach (var day in this.EmployeePostDays)
            {
                EmployeePostDay newDay = day.CloneDTO();
                newDay.AllMatchedCalculationPeriodItems = new List<CalculationPeriodItem>();
                newDay.Date = startDate.AddDays(CalendarUtility.GetDayNr(day.Date) - 1);

                foreach (var item in day.AllMatchedCalculationPeriodItems)
                {
                    var clone = item.Clone();
                    clone.TimeSlot = item.TimeSlot.Clone();
                    newDay.AllMatchedCalculationPeriodItems.Add(clone);
                }

                List<CalculationPeriodItem> items = day.SelectedItemsHead.CalculationPeriodItems.ToList();
                day.SelectedItemsHead.CalculationPeriodItems = new List<CalculationPeriodItem>();
                newDay.SelectedItemsHead = day.SelectedItemsHead.CloneDTO();
                day.SelectedItemsHead.CalculationPeriodItems.AddRange(items);
                newDay.SelectedItemsHead.CalculationPeriodItems = new List<CalculationPeriodItem>();

                foreach (var item in items)
                {
                    var clone = item.Clone();
                    clone.TimeSlot = item.TimeSlot.Clone();
                    clone.ScheduleDate = newDay.Date;
                    clone.Date = newDay.Date;
                    newDay.SelectedItemsHead.CalculationPeriodItems.Add(clone);
                }

                newEmployeePostDays.Add(newDay);
            }

            employeePost = employeePost == null ? this.CalculationOptions.EmployeePost == employeePost ? employeePost : this.CalculationOptions.EmployeePost : null;

            EmployeePostWeek newWeek = new EmployeePostWeek(employeePost)
            {
                CalculationOptions = this.CalculationOptions,
                EmployeePost = employeePost,
                StartDate = startDate,
                EmployeePostDays = newEmployeePostDays,
                WeekNumber = weekNumber == 0 ? this.WeekNumber + 1 : weekNumber,
                IsCopy = setIsCopy,
                SetHalf = this.SetHalf
            };

            return newWeek;
        }

        #endregion

        #region Remove

        public void RemoveFreeDays(DayOfWeek dayOfWeek, EmployeePostCycle employeePostCycle, bool removeWeekEndDayIfOne = true)
        {
            if (this.FreeDays.Count > 0 && (this.HandledButEmptyDays.Count + this.HandledDays.Count >= 6) && this.GetNumberOfPossibleWorkDaysLeftInWeek(dayOfWeek, employeePostCycle) > 0)
            {
                this.FreeDays = new List<DayOfWeek>();
                return;
            }

            if (removeWeekEndDayIfOne && this.HandledDays.Where(w => w == DayOfWeek.Sunday || w == DayOfWeek.Saturday).Count() > 1)
            {
                if (this.HandledButEmptyDays.Where(w => w == DayOfWeek.Sunday || w == DayOfWeek.Saturday).Count() == 1)
                {
                    var sunday = this.EmployeePostDays.Where(w => w.DayOfWeek == DayOfWeek.Sunday).FirstOrDefault();

                    if (this.HandledButEmptyDays.Where(w => w == DayOfWeek.Sunday).Count() == 0 && sunday != null)
                    {
                        this.EmployeePostDays.Remove(sunday);
                        this.HandledButEmptyDays.Add(DayOfWeek.Sunday);
                    }
                    else
                    {

                        var saturday = this.EmployeePostDays.Where(w => w.DayOfWeek == DayOfWeek.Saturday).FirstOrDefault();

                        if (this.HandledButEmptyDays.Where(w => w == DayOfWeek.Saturday).Count() == 0 && saturday != null)
                        {
                            this.EmployeePostDays.Remove(saturday);
                            this.HandledButEmptyDays.Add(DayOfWeek.Saturday);
                        }
                    }

                    this.FreeDays = new List<DayOfWeek>();
                    return;
                }
            }

            if (this.HandledDays.Count > 0)
            {
                if (this.FreeDays.Count > 0)

                    if (SaturDayhandledWithNoTime)
                    {
                        this.FreeDays = this.FreeDays.Take(1).ToList();
                    }

                if (SunDayhandledWithNoTime)
                {
                    this.FreeDays = this.FreeDays.Take(1).ToList();
                }

                if (SaturDayhandledWithNoTime && SunDayhandledWithNoTime)
                    this.FreeDays = new List<DayOfWeek>();

                if (this.HandledButEmptyDays.Count > 0)
                {
                    if (this.FreeDays.Count <= HandledButEmptyDays.Count)
                        this.FreeDays = new List<DayOfWeek>();
                    else if (this.FreeDays.Count > HandledButEmptyDays.Count)
                        this.FreeDays = this.FreeDays.Take(this.FreeDays.Count - HandledButEmptyDays.Count).ToList();
                }
            }
        }

        #endregion

        #region Help


        #endregion

        #region Logging

        private void InitInformation()
        {
            var employeePost = this.EmployeePost == null && this.CalculationOptions != null ? this.CalculationOptions.EmployeePost : this.EmployeePost;

            if (this.EmployeePostWeekInformation == null && employeePost != null)
                this.EmployeePostWeekInformation = new EmployeePostWeekInformation(employeePost.EmployeePostId, employeePost.Name, this.StartDate);
        }

        public void LogInformation()
        {
            this.InitInformation();

            this.EmployeePostWeekInformation.EmployeePostId = this.EmployeePost.EmployeePostId;
            this.EmployeePostWeekInformation.EmployeePostName = this.EmployeePost.Name;
            this.EmployeePostWeekInformation.Percent = this.Percent;
            this.EmployeePostWeekInformation.NumberOfEmployeePostDays = this.EmployeePostDays.Where(w => w.Length > 0).Count();
            this.EmployeePostWeekInformation.RemainingMinutes = this.RemainingMinutesWeek;
            this.EmployeePostWeekInformation.Percent = this.Percent;
            this.EmployeePostWeekInformation.StartDate = this.StartDate;
        }

        public void LogInformation(string discardedInformation)
        {
            this.InitInformation();
            this.EmployeePostWeekInformation.DiscardedInformation = discardedInformation;
        }

        public void LogInformation(int weekAttempts, bool attemptsWeek)
        {
            LogInformation();
            this.EmployeePostWeekInformation.AttemptsWeek = attemptsWeek;
            this.EmployeePostWeekInformation.WeekAttempts = weekAttempts;
        }

        public void AddEmployeePostDayInformation(EmployeePostDay employeePostDay, bool disposed = false)
        {
            if (employeePostDay == null)
                return;

            employeePostDay.EmployeePostDayInformation.Disposed = disposed;
            this.EmployeePostWeekInformation.AddEmployeePostDayInformation(employeePostDay.EmployeePostDayInformation);
        }

        #endregion


        #endregion
    }

    public class EmployeePostSort
    {

        #region Properties
        public EmployeePostDTO EmployeePost { get; set; }
        public List<List<CalculationPeriodItem>> PossibleWeeks { get; set; }
        public int CompareScore { get; set; }
        public int Uniques { get; set; }

        public int NumberOfPossibleShiftTypes
        {
            get
            {
                if (this.EmployeePost != null && this.EmployeePost.ValidShiftTypes != null)
                {
                    return this.EmployeePost.ValidShiftTypes.Count;
                }

                return 0;
            }
        }


        public int NumberOfQualifiedShiftTypes { get; set; }

        public bool HasBeenDisposed { get; set; }
        public List<string> CompareStrings
        {
            get
            {
                List<string> strings = new List<string>();

                foreach (var item in this.PossibleWeeks)
                {
                    strings.AddRange(item.Select(i => i.GetCompareString()));
                }

                return strings.Distinct().ToList();
            }
        }
        public int NumberOfPossibleWeeks
        {
            get
            {
                return this.PossibleWeeks.Count;
            }
        }

        public List<List<DayOfWeek>> DayOfWeekOrders { get; set; }

        public int RemainingMinutes { get; set; }

        #endregion

        #region ctor

        public List<List<CalculationPeriodItem>> GetPossibleWeeks(DateTime date)
        {
            return this.PossibleWeeks.Where(w => w.Where(s => CalendarUtility.GetDatesInWeek(date).Contains(s.ScheduleDate)).Any()).ToList();
        }

        public EmployeePostSort(EmployeePostDTO employeePost, int remainingMinutes = 0, bool hasBeenDisposed = false)
        {
            this.EmployeePost = employeePost;
            this.PossibleWeeks = new List<List<CalculationPeriodItem>>();
            this.RemainingMinutes = remainingMinutes;
            this.DayOfWeekOrders = new List<List<DayOfWeek>>();
            this.HasBeenDisposed = hasBeenDisposed;
        }

        public void RemoveOther(string compareString)
        {
            List<List<CalculationPeriodItem>> verifiedList = new List<List<CalculationPeriodItem>>();

            foreach (var week in this.PossibleWeeks)
            {
                var items = week.Where(s => s.GetCompareString().Equals(compareString)).ToList();

                if (items.Count > 0)
                {
                    var date = items.FirstOrDefault().ScheduleDate;
                    var otherOnDate = week.Where(s => s.ScheduleDate == date && s.CalculationGuid != items.FirstOrDefault().CalculationGuid);

                    List<CalculationPeriodItem> verifiedItems = new List<CalculationPeriodItem>();
                    verifiedItems = week.Where(i => !otherOnDate.Contains(i)).ToList();

                    verifiedList.Add(verifiedItems);
                }
            }

            this.PossibleWeeks = verifiedList.ToList();
        }

        #endregion

        #region Methods
        public int GetCountInPossibleWeeks(int? timeScheduleTaskId, int? incomingDeliveryRowId)
        {
            int count = 0;

            foreach (var weeks in this.PossibleWeeks)
            {
                foreach (var item in weeks)
                {
                    if (item.TimeScheduleTaskId.HasValue && item.TimeScheduleTaskId == timeScheduleTaskId)
                        count++;
                    else if (item.IncomingDeliveryRowId.HasValue && item.IncomingDeliveryRowId == incomingDeliveryRowId)
                        count++;

                }
            }

            return count;
        }
        #endregion

    }

    public class EmployeePostDay
    {
        #region Properties

        public DateTime Date { get; set; }
        public CalculationOptions CalculationOptions { get; set; }
        public List<CalculationPeriodItem> AllMatchedCalculationPeriodItems { get; set; }
        public EmployeePostDayInformation EmployeePostDayInformation { get; set; }
        public int DayOfWeekId
        {
            get
            {
                return (int)Date.DayOfWeek;
            }
        }

        public DayOfWeek DayOfWeek
        {
            get
            {
                return this.Date.DayOfWeek;
            }
        }

        public List<ShiftTypeSkillDTO> ShiftTypeSkillDTOs
        {
            get
            {
                List<ShiftTypeSkillDTO> shiftTypeSkillDTOs = new List<ShiftTypeSkillDTO>();

                if (SelectedItemsHead?.CalculationPeriodItems != null)
                {
                    foreach (var item in SelectedItemsHead.CalculationPeriodItems)
                    {
                        if (item.ShiftType?.ShiftTypeSkills != null)
                        {
                            foreach (var skill in item.ShiftType.ShiftTypeSkills)
                            {
                                if (!shiftTypeSkillDTOs.Select(s => s.SkillId).Contains(skill.SkillId))
                                    shiftTypeSkillDTOs.Add(skill);
                            }

                        }
                    }
                }

                return shiftTypeSkillDTOs;
            }
        }
        public int Length
        {
            get
            {
                if (this.SelectedItemsHead != null)
                    return this.SelectedItemsHead.Length;
                else
                    return 0;
            }
        }

        public int BreakLength
        {
            get
            {
                if (this.SelectedItemsHead?.BreakSlots != null)
                    return this.SelectedItemsHead.BreakSlots.Sum(s => s.Length);
                else
                    return 0;
            }
        }

        public int LeftOverLenght
        {
            get
            {
                if (LeftOverStaffingNeedsCalcutionPeriodItems != null)
                    return LeftOverStaffingNeedsCalcutionPeriodItems.Sum(t => t.Length);
                else
                    return 0;
            }
        }
        public DateTime StartTime
        {
            get
            {
                if (this.SelectedItemsHead != null && this.SelectedItemsHead.CalculationPeriodItems != null && this.SelectedItemsHead.CalculationPeriodItems.Count > 0)
                    return this.SelectedItemsHead.CalculationPeriodItems.OrderBy(o => o.TimeSlot.From).FirstOrDefault().TimeSlot.From;
                else
                    return CalendarUtility.DATETIME_DEFAULT;
            }
        }
        public DateTime StopTime
        {
            get
            {
                if (this.SelectedItemsHead != null && this.SelectedItemsHead.CalculationPeriodItems != null && this.SelectedItemsHead.CalculationPeriodItems.Count > 0)
                    return this.SelectedItemsHead.CalculationPeriodItems.OrderByDescending(o => o.TimeSlot.To).FirstOrDefault().TimeSlot.To;
                else
                    return CalendarUtility.DATETIME_DEFAULT;
            }
        }
        public List<PeriodItemsGroupHead> PeriodItemsGroupHeads
        {
            get
            {
                List<PeriodItemsGroupHead> heads = new List<PeriodItemsGroupHead>();
                var groups = this.AllMatchedCalculationPeriodItems.GroupBy(r => r.CalculationGuid);

                foreach (var group in groups)
                {
                    PeriodItemsGroupHead head = new PeriodItemsGroupHead()
                    {
                        Key = group.First().CalculationGuid.ToString(),
                        CalculationPeriodItems = group.ToList(),
                        Done = false
                    };
                    heads.Add(head);
                }

                return heads.OrderBy(r => r.CalculationPeriodItems.OrderBy(sr => sr.TimeSlot.From).FirstOrDefault().TimeSlot.From).ThenByDescending(t => t.Length).ToList();
            }

        }
        public EmployeePostDTO EmployeePost { get; set; }
        public PeriodItemsGroupHead SelectedItemsHead { get; set; }
        public List<CalculationPeriodItem> LeftOverStaffingNeedsCalcutionPeriodItems { get; set; }
        public List<ScheduleCycleRuleDTO> ValidScheduleCycleRules { get; set; }

        #endregion

        #region Ctor

        public EmployeePostDay()
        {
            this.SelectedItemsHead = new PeriodItemsGroupHead();
            this.AllMatchedCalculationPeriodItems = new List<CalculationPeriodItem>();
            this.ValidScheduleCycleRules = new List<ScheduleCycleRuleDTO>();
            this.InitInformation();
        }

        public EmployeePostDay(DateTime date, List<CalculationPeriodItem> matchingPeriodItems, CalculationOptions calculationOptions)
        {
            this.Date = date;
            this.AllMatchedCalculationPeriodItems = matchingPeriodItems;
            this.CalculationOptions = calculationOptions;
            this.SelectedItemsHead = new PeriodItemsGroupHead();
            this.ValidScheduleCycleRules = new List<ScheduleCycleRuleDTO>();
            this.InitInformation();

            FilterAccordingToWorkRules();
        }

        public EmployeePostDay(DateTime date, CalculationOptions calculationOptions)
        {
            this.Date = date;
            this.AllMatchedCalculationPeriodItems = new List<CalculationPeriodItem>();
            this.CalculationOptions = calculationOptions;
            this.SelectedItemsHead = new PeriodItemsGroupHead();
            this.ValidScheduleCycleRules = new List<ScheduleCycleRuleDTO>();
            this.InitInformation();

            FilterAccordingToWorkRules();
        }

        #endregion

        #region Public methods

        #region Validate

        public string EmployeePostDayValidationSort(int preferredDayLength)
        {
            if (SelectedItemsHead != null)
                return $"PercentNotInFutureWeeks:{SelectedItemsHead.PercentFutureWeeksStringReverseForOrder}#MinutesFromPreferredDay: {StringUtility.SortInvoiceNr(MinutesFromPreferredLength(preferredDayLength).ToString())}";
            return "";
        }


        public int MinutesFromPreferredLength(int preferedLength)
        {
            if (this.Length > 0)
                return Convert.ToInt32(Math.Abs(this.Length - preferedLength));
            return 0;
        }

        public decimal PercentFromPreferredLength(int preferedLength)
        {
            if (this.Length > 0)
                return Math.Abs((decimal.Multiply(100, decimal.Divide((this.Length - preferedLength), this.Length))));
            return 0;
        }
        public void SetValidRules(EmployeePostDTO employeePostDTO)
        {
            var rules = employeePostDTO.GetScheduleCycleRuleDTOs();
            if (this.ValidScheduleCycleRules == null)
                this.ValidScheduleCycleRules = new List<ScheduleCycleRuleDTO>();

            this.ValidScheduleCycleRules.AddRange(rules);
        }

        #endregion

        #region Filter

        public void FilterAccordingToWorkRules()
        {
            FilterAccordingToWorkRulesMaxTime();
            FilterAccordingToWorkRulesMinTime();
        }

        public void FilterAccordingToWorkRulesMaxTime()
        {
            var groups = this.AllMatchedCalculationPeriodItems.GroupBy(i => i.CalculationGuid);

            this.AllMatchedCalculationPeriodItems = new List<CalculationPeriodItem>();

            foreach (var group in groups)
            {
                var periods = group.ToList();

                if (periods.Where(w => !w.IsBreak).Sum(s => s.Length) <= this.CalculationOptions.GetMaxLenght())
                    this.AllMatchedCalculationPeriodItems.AddRange(periods);
            }
        }

        public void FilterAccordingToWorkRulesMinTime()
        {
            var groups = this.AllMatchedCalculationPeriodItems.GroupBy(i => i.CalculationGuid);

            this.AllMatchedCalculationPeriodItems = new List<CalculationPeriodItem>();

            foreach (var group in groups)
            {
                var periods = group.ToList();

                if (periods.Where(w => !w.IsBreak).Sum(s => s.Length) >= this.CalculationOptions.MinLength)
                    this.AllMatchedCalculationPeriodItems.AddRange(periods);
            }
        }

        #endregion

        #region Logging

        private void InitInformation()
        {
            if (this.EmployeePostDayInformation == null && this.EmployeePost != null)
                this.EmployeePostDayInformation = new EmployeePostDayInformation(this.EmployeePost.EmployeePostId, this.EmployeePost.Name);
            else
                this.EmployeePostDayInformation = new EmployeePostDayInformation(0, "");
        }

        public void LogInformation()
        {
            this.InitInformation();
            this.EmployeePostDayInformation.Length = this.Length;
        }

        #endregion

        #endregion
    }

    public class EmployeePostDayValidation
    {
        #region Properties

        public List<EmployeePostDayValidationRow> EmployeePostDayValidationRows { get; set; }
        public int PreferredDayLength { get; set; }
        public EmployeePostCyclesRun EmployeePostCyclesRun { get; set; }
        #endregion

        #region Ctor

        public EmployeePostDayValidation(int preferredDayLength, EmployeePostCyclesRun employeePostCyclesRun)
        {
            this.EmployeePostDayValidationRows = new List<EmployeePostDayValidationRow>();
            this.PreferredDayLength = preferredDayLength;
            this.EmployeePostCyclesRun = employeePostCyclesRun;
        }

        #endregion

        #region Public methods

        public void AddEmployeePostDay(EmployeePostDay employeePostDay, int percentLimited)
        {
            this.EmployeePostDayValidationRows.Add(new EmployeePostDayValidationRow(employeePostDay, this.PreferredDayLength, EmployeePostCyclesRun));
        }

        public EmployeePostDay GetBestEmployeePostDay()
        {
            return this.EmployeePostDayValidationRows.OrderBy(o => o.PrioSort).FirstOrDefault()?.EmployeePostDay;
        }

        #endregion
    }

    public class EmployeePostDayValidationRow
    {
        #region Properties

        public EmployeePostDay EmployeePostDay { get; set; }
        public string PrioSort { get; set; }
        public bool HasUniques { get; set; }
        public bool HasPrioShiftTypes { get; set; }
        public int MinutesFromPreferredLength { get; set; }

        #endregion

        #region Ctor

        public EmployeePostDayValidationRow(EmployeePostDay employeePostDay, int preferredDayLength, EmployeePostCyclesRun employeePostCyclesRun)
        {
            EmployeePostDay = employeePostDay;
            HasUniques = this.EmployeePostDay.SelectedItemsHead.HasUniques(employeePostCyclesRun);
            HasPrioShiftTypes = this.EmployeePostDay.SelectedItemsHead.HasPrioShiftTypes(employeePostCyclesRun.PrioShiftTypesIds);
            MinutesFromPreferredLength = this.EmployeePostDay.MinutesFromPreferredLength(preferredDayLength);

            int points = 0;
            int maxPoints = 1000;

            if (this.EmployeePostDay.SelectedItemsHead.PercentFutureWeeks > 0)
                points = Convert.ToInt32(this.EmployeePostDay.SelectedItemsHead.PercentFutureWeeks);

            if (HasPrioShiftTypes)
                points += 40;

            if (HasUniques)
                points += 70;

            if (MinutesFromPreferredLength == 0)
                points += 100;
            else if (MinutesFromPreferredLength > 0 && MinutesFromPreferredLength < 120)
                points += 200 - MinutesFromPreferredLength;
            else if (MinutesFromPreferredLength >= 120 && MinutesFromPreferredLength < 200)
                points += 100 - Convert.ToInt32((decimal.Divide(MinutesFromPreferredLength, 2)));

            if (points <= 0)
                points = 0;

            string pointString = StringUtility.SortInvoiceNr((maxPoints - points).ToString());

            PrioSort = $"Points {pointString} {this.EmployeePostDay.EmployeePostDayValidationSort(preferredDayLength)} HasUniques {HasUniques} HasPrioShiftTypes {HasPrioShiftTypes}";
        }

        #endregion

        #region Public methods

        public override string ToString()
        {
            return $"{CalendarUtility.ToTime(this.EmployeePostDay.StartTime)} - {CalendarUtility.ToTime(this.EmployeePostDay.StopTime)}  {decimal.Round(decimal.Divide(EmployeePostDay.Length, 60), 2)} h ";
        }

        #endregion
    }

    public class FreeTimeSlot
    {
        #region Properties

        public Guid CalculationRowGuid { get; set; }
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public int? ShiftTypeId { get; set; }
        public bool UnfinishSlot { get; set; }

        #endregion

        #region Ctor

        public FreeTimeSlot()
        {

        }

        #endregion

        #region Public methods

        #endregion
    }

    public class CalculationOptions
    {
        #region Properties

        public EmployeePostDTO EmployeePost { get; set; }
        public DayOfWeek? Weekday { get; set; }
        public DateTime StartOfDay { get; set; }
        public DateTime EndOfDay { get; set; }
        public DateTime Date { get; set; }
        public int? DayTypeId { get; set; }
        public int Interval { get; set; }
        public int Interval15
        {
            get
            {
                return 15;
            }
        }
        public int MinLength { get; set; }
        private int optimalLenght { get; set; }
        public bool HasUniques { get; set; }
        public int OptimalLength
        {
            get
            {
                int length = this.optimalLenght;

                if (length < this.MinLength)
                    return this.MinLength;
                else
                    return CalendarUtility.AdjustAccordingToInterval(length, this.Interval);
            }
            set
            {
                int length = value;

                if (length < this.MinLength)
                    length = this.MinLength;

                this.optimalLenght = CalendarUtility.AdjustAccordingToInterval(length, this.Interval);
            }
        }
        public int MaxLength { get; set; }
        private int maxLenghtWeekend { get; set; }
        public int MaxLenghtWeekend
        {
            get
            {
                if (maxLenghtWeekend == 0)
                    return MaxLength;
                else
                    return maxLenghtWeekend;
            }
            set
            {
                maxLenghtWeekend = value;
            }
        }
        public bool ApplyLengthRule { get; set; }
        public int MaxNumberOfWeeks { get; set; }
        public bool ForceAddOnlyDockedPeriodItems { get; set; }
        public bool AddOnlyDockedPeriodItems { get; set; }
        public bool MixShiftTypesOnHead { get; set; }
        public int RangeMinMinutes
        {
            get
            {
                int result = OptimalLength - this.Interval * 4;

                if (result < this.MinLength)
                    return this.MinLength;
                else
                    return result;
            }
        }
        public int RangeMaxMinutes
        {
            get
            {
                int result = OptimalLength + this.Interval * 4;

                if (result > this.MaxLength)
                    return this.MaxLength;
                else
                    return result;
            }
        }

        private DateTime? firstPossibleStart { get; set; }

        public DateTime FirstPossibleStart
        {
            get
            {
                if (firstPossibleStart.HasValue)
                    return this.firstPossibleStart.Value;

                return CalendarUtility.DATETIME_DEFAULT.AddDays(-1);
            }
            set
            {
                this.firstPossibleStart = value;
            }

        }

        private DateTime? lastPossibleStop { get; set; }

        public DateTime LastPossibleStop
        {
            get
            {
                if (lastPossibleStop.HasValue)
                    return this.lastPossibleStop.Value;

                return CalendarUtility.DATETIME_DEFAULT.AddDays(2);
            }
            set
            {
                this.lastPossibleStop = value;
            }
        }

        public List<OpeningHoursDTO> OpeningHours { get; set; }


        public List<CalculationPeriodItem> LeftOverStaffingNeedsCalcutionPeriodItems { get; set; }

        #endregion

        #region Ctor

        public CalculationOptions(int interval = 15, bool applyLengthRule = false, int minLenght = 120, int optimalLenght = 420, int maxLenght = 600, List<OpeningHoursDTO> openingHours = null)
        {


            this.Interval = interval != 0 ? interval : 1;
            this.MinLength = minLenght;
            this.OptimalLength = optimalLenght;
            this.MaxLength = maxLenght;
            this.ApplyLengthRule = false;
            this.MixShiftTypesOnHead = true;
            this.LeftOverStaffingNeedsCalcutionPeriodItems = new List<CalculationPeriodItem>();
            this.StartOfDay = CalendarUtility.DATETIME_DEFAULT.AddHours(6);
            this.EndOfDay = CalendarUtility.DATETIME_DEFAULT.AddHours(23);
            this.OpeningHours = openingHours;
        }

        #endregion

        #region Public methods  

        public int GetMaxLenght()
        {
            if (this.Weekday == DayOfWeek.Sunday || this.Weekday == DayOfWeek.Saturday)
                return this.MaxLenghtWeekend;
            else
                return this.MaxLength;
        }

        public DateTime GetOpeningTime(DateTime date, DateTime fallBackTime)
        {
            if (this.OpeningHours != null)
            {
                var match = this.OpeningHours.FirstOrDefault(f => f.StandardWeekDay == (int)date.DayOfWeek);

                if (match != null && match.OpeningTime.HasValue)
                    return CalendarUtility.MergeDateAndTime(date, match.OpeningTime.Value.AddHours(-1));
                else
                    return fallBackTime;
            }
            else
                return fallBackTime;
        }

        public DateTime GetClosingTime(DateTime date, DateTime fallBackTime)
        {
            if (this.OpeningHours != null)
            {
                var match = this.OpeningHours.FirstOrDefault(f => f.StandardWeekDay == (int)date.DayOfWeek);

                if (match != null && match.ClosingTime.HasValue)
                    return CalendarUtility.MergeDateAndTime(date, match.ClosingTime.Value.AddHours(1));
                else
                    return fallBackTime;
            }
            else
                return fallBackTime;
        }

        #endregion
    }

    public class PeriodItemsGroupHead
    {
        #region Properties

        public string Key { get; set; }

        public Guid CalculationGuid
        {
            get
            {
                return this.CalculationPeriodItems?.FirstOrDefault()?.CalculationGuid ?? Guid.NewGuid();
            }
        }
        public Guid? TempGuid { get; set; }
        public int TempBreakMinutes { get; set; }
        public CalculationPeriodItemGroupByType CalculationRowGroupByType { get; set; }
        public List<CalculationPeriodItem> CalculationPeriodItems { get; set; }
        public bool Done { get; set; }
        public bool BreaksApplied { get; set; }
        public List<TimeBreakTemplateBreakSlot> BreakSlots { get; set; }
        public decimal PercentFutureWeeks { get; set; }
        public string PercentFutureWeeksString
        {
            get
            {
                if (PercentFutureWeeks < 10)
                    return "00" + PercentFutureWeeks.ToString();
                if (PercentFutureWeeks < 100)
                    return "0" + PercentFutureWeeks.ToString();
                return PercentFutureWeeks.ToString();
            }
        }

        public string PercentFutureWeeksStringReverseForOrder
        {
            get
            {
                if (PercentFutureWeeks == 0)
                    return "999";

                var fromHundred = 100 - PercentFutureWeeks;
                if (fromHundred < 10)
                    return "00" + fromHundred.ToString();
                else if (fromHundred < 100)
                    return "0" + fromHundred.ToString();

                return "998";
            }
        }

        public bool Consecutive
        {
            get
            {
                DateTime currentTime = StartTime;

                foreach (var item in CalculationPeriodItems.OrderBy(o => o.TimeSlot.From))
                {
                    if (currentTime > item.TimeSlot.From)
                        return false;

                    currentTime = item.TimeSlot.To;
                }

                return true;
            }
        }

        public DateTime? Date { get; set; }

        public bool HasNetTime
        {
            get
            {
                foreach (var item in this.CalculationPeriodItems)
                {
                    if (item.IsNetTime)
                        return true;
                }

                return false;
            }
        }

        public bool HasFixedTime
        {
            get
            {
                foreach (var item in this.CalculationPeriodItems)
                {
                    if (item.IsFixed)
                        return true;
                }

                return false;
            }
        }

        public int NumberOfFixedTasksOrDelivery
        {
            get
            {
                int count = 0;
                foreach (var item in this.CalculationPeriodItems.Where(w => !w.IsBreak).GroupBy(g => $"{g.TimeScheduleTaskKey}#{g.IncomingDeliveryRowKey}"))
                {
                    if (item.Any(a => a.IsFixed))
                        count++;
                }

                return count;
            }
        }

        public DateTime StartTime
        {
            get
            {
                if (this.CalculationPeriodItems != null && this.CalculationPeriodItems.Count > 0)
                    return this.CalculationPeriodItems.OrderBy(o => o.TimeSlot.From).FirstOrDefault().TimeSlot.From;
                else
                    return CalendarUtility.DATETIME_DEFAULT;
            }
        }
        public DateTime StopTime
        {
            get
            {
                if (this.CalculationPeriodItems != null && this.CalculationPeriodItems.Count > 0)
                    return this.CalculationPeriodItems.OrderByDescending(o => o.TimeSlot.To).FirstOrDefault().TimeSlot.To;
                else
                    return CalendarUtility.DATETIME_DEFAULT;
            }
        }

        public int Length
        {
            get
            {
                if (this.CalculationPeriodItems != null)
                    return this.CalculationPeriodItems.Where(s => !s.IsBreak).Sum(s => s.TimeSlot.Minutes);
                else
                    return 0;
            }
        }

        public int TempNetLength
        {
            get
            {
                return this.Length - TempBreakMinutes;
            }
        }

        public bool HasHoles
        {
            get
            {
                if (this.CalculationPeriodItems.Count == 1)
                    return false;

                DateTime? prevToTime = null;

                foreach (var item in this.CalculationPeriodItems.OrderBy(t => t.TimeSlot.From))
                {
                    if (prevToTime.HasValue && prevToTime != item.TimeSlot.From)
                        return true;

                    prevToTime = item.TimeSlot.To;
                }

                return false;
            }
        }

        public string CompareKey
        {
            get
            {
                if (this.CalculationPeriodItems.Count > 0)
                    return $"{this.StartTime}#{this.StopTime}#{this.CalculationPeriodItems.First().ShiftTypeId}#{this.CalculationPeriodItems.Last().ShiftTypeId}#{this.Length}#{this.CalculationPeriodItems.Count}#{this.CalculationPeriodItems.First().TimeScheduleTaskKey}#{this.CalculationPeriodItems.Last().TimeScheduleTaskKey}#{this.CalculationPeriodItems.First().IncomingDeliveryRowKey}#{this.CalculationPeriodItems.Last().IncomingDeliveryRowKey}#";
                else
                    return Guid.NewGuid().ToString();
            }
        }

        #endregion

        #region Ctor

        public PeriodItemsGroupHead()
        {
            this.CalculationPeriodItems = new List<CalculationPeriodItem>();
        }

        #endregion

        #region Public methods

        public bool HasUniques(EmployeePostCyclesRun employeePostCyclesRun)
        {
            if (this.CalculationPeriodItems.Count == 1)
                return false;

            foreach (var item in this.CalculationPeriodItems.OrderBy(t => t.TimeSlot.From))
            {
                if (employeePostCyclesRun.IsUnique(item))
                    return true;
            }

            return false;
        }

        public bool HasPrioShiftTypes(List<int> prioShiftTypeIds)
        {
            if (this.CalculationPeriodItems.Count == 0)
                return false;

            foreach (var item in this.CalculationPeriodItems.OrderBy(t => t.TimeSlot.From))
            {
                if (item.ShiftTypeId.HasValue && prioShiftTypeIds.Contains(item.ShiftTypeId.Value))
                    return true;
            }

            return false;
        }

        public bool HasShiftTypesWithoutSkills()
        {
            foreach (var item in this.CalculationPeriodItems.OrderBy(t => t.TimeSlot.From))
            {
                if (!item.ShiftTypeId.HasValue || (item.ShiftType != null && item.ShiftType.ShiftTypeSkills.IsNullOrEmpty()))
                    return true;
            }

            return false;
        }

        #endregion
    }

    public class TimeBreakInformation
    {
        #region Properties

        public int? ShiftTypeId { get; set; }
        public List<TimeBreakTemplateBreakSlot> BreakSlots { get; set; }
        public StaffingNeedsCalculationTimeSlot BreakTimeSlot { get; set; }
        public DateTime Created { get; set; }

        #endregion

        #region Ctor

        public TimeBreakInformation(int? shiftTypeId, List<TimeBreakTemplateBreakSlot> breakSlots)
        {
            this.ShiftTypeId = shiftTypeId;
            this.BreakSlots = breakSlots;
            this.BreakTimeSlot = null;
            this.Created = DateTime.UtcNow;
        }

        #endregion

        #region Public methods

        #endregion
    }

    public class CalculationPeriodItem
    {
        #region Properties

        public string Info
        {
            get
            {
                return $"Guid: {this.CalculationGuid} Date: {this.Date.ToShortDateString()} From: {this.TimeSlot.From.ToShortTimeString()} To: {this.TimeSlot.To.ToShortTimeString()} Length: {this.TimeSlot.Minutes} IsFixed: {this.TimeSlot.IsFixed} IsNetTime: {this.IsNetTime} ShiftType: {this.ShiftType.Name} TaskKey: {this.TimeScheduleTaskKey} DevRowId: {this.IncomingDeliveryRowKey} ScheduleDate: {this.ScheduleDate.ToShortDateString()} OrgGuid: {this.OriginalCalculationRowGuid}";
            }
        }

        public Guid CalculationPeriodItemGuid { get; set; } // Keep unique
        public Guid CalculationGuid { get; set; }
        public Guid StaffingNeedsRowGuid { get; set; }
        public Guid? SuggestedTargetCalculationGuid { get; set; }
        public Guid StaffingNeedsCalcutionHeadRowGuid { get; set; }
        private Guid originalCalculationRowGuid { get; set; }
        public Guid OriginalCalculationRowGuid
        {
            get
            {
                if (originalCalculationRowGuid == new Guid())
                    return CalculationGuid;
                else
                    return originalCalculationRowGuid;
            }
            set
            {
                originalCalculationRowGuid = value;
            }
        }
        public Guid PeriodGuid { get; set; }
        public Guid? TempGuid { get; set; }

        public int StaffingNeedsHeadId { get; set; }
        public int StaffingNeedsRowId { get; set; }
        public int StaffingNeedsRowPeriodId { get; set; }
        public int? ShiftTypeId { get; set; }
        public int? TimeScheduleTaskId { get; set; }
        public int? TimeScheduleTaskTypeId { get; set; }
        public int? IncomingDeliveryRowId { get; set; }
        public int? TimeCodeBreakGroupId { get; set; }
        public int? DayTypeId { get; set; }
        public int EmployeePostId { get; set; }

        public PeriodItemsGroupHead StaffingNeedsCalcutionHeadRow { get; set; }
        public StaffingNeedsCalculationTimeSlot TimeSlot { get; set; }
        public List<TimeBreakTemplateTimeSlot> BreakTimeSlot { get; set; }
        public ShiftTypeDTO ShiftType { get; set; }

        public StaffingNeedsRowType Type { get; set; }
        public StaffingNeedsRowOriginType OriginType { get; set; }
        public string Name { get; set; }
        public DayOfWeek? Weekday { get; set; }
        public DateTime? Date { get; set; }
        public DateTime StartTime { get; set; }
        public int Length
        {
            get
            {
                return this.TimeSlot.Minutes;
            }
        }
        public int Interval { get; set; }
        public decimal Value { get; set; }
        public int CalculationRowNr { get; set; }
        public int MovedNrOfTimes { get; set; }

        public int MinSplitLength { get; set; }
        public bool FromBreakRules { get; set; }
        public bool OnlyOneEmployee { get; set; }
        public bool DontAssignBreakLeftovers { get; set; }
        public bool AllowOverlapping { get; set; }
        public bool IsStaffingNeedsFrequency { get; set; }
        public bool IsBreak { get; set; }
        public int? ParentId { get; set; }
        public bool Ignore { get; set; }

        public bool IsFixed
        {
            get
            {
                if (this.TimeSlot != null)
                    return this.TimeSlot.IsFixed;
                else
                    return true;
            }
        }

        public bool IsMergable
        {
            get
            {
                if (this.MinSplitLength == 0)
                    return false;

                if (this.TimeSlot != null)
                {
                    if (this.TimeSlot.IsFixed)
                        return false;

                    if (this.TimeSlot.TimeSlotLength < decimal.Multiply(this.TimeSlot.Minutes, Convert.ToDecimal(1.3)))
                        return false;

                    return true;
                }

                return false;
            }
        }

        private bool forceGrossTime { get; set; }
        public bool IsNetTime
        {
            get
            {
                if (!forceGrossTime)
                    return this.DontAssignBreakLeftovers && this.TimeSlot != null && !this.TimeSlot.IsFixed;
                else
                    return false;
            }
        }
        public bool AllowBreaks
        {
            get
            {
                return !(this.DontAssignBreakLeftovers && this.TimeSlot.IsFixed && this.MinSplitLength == 0);
            }
        }

        public bool BreakFillsNeed
        {
            get
            {
                return this.DontAssignBreakLeftovers && IsFixed;
            }
        }

        public int TempBreakMinutes { get; set; }

        public SoeEntityState RowState { get; set; }
        public SoeEntityState PeriodState { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }

        public DateTime ScheduleDate { get; set; }

        public SplitInformation SplitInformation { get; set; }
        public string IncomingDeliveryRowKey { get; internal set; }

        public void SetIncomingDeliveryRowKey(int person)
        {
            if (IncomingDeliveryRowId.HasValue)
                IncomingDeliveryRowKey = $"rowid{IncomingDeliveryRowId}person{person}";
            else
                IncomingDeliveryRowKey = string.Empty;
        }

        public string TimeScheduleTaskKey { get; internal set; }
        public bool ReplaceWithBreak { get; internal set; }

        public void SetTimeScheduleTaskKey(int person)
        {
            if (TimeScheduleTaskId.HasValue)
            {
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.Append("taskId");
                stringBuilder.Append(TimeScheduleTaskId.ToString());
                stringBuilder.Append("person");
                stringBuilder.Append(person.ToString());
                TimeScheduleTaskKey = stringBuilder.ToString();
            }
            else
                TimeScheduleTaskKey = string.Empty;
        }

        #endregion

        #region Ctor

        public CalculationPeriodItem()
        {
            this.TimeSlot = new StaffingNeedsCalculationTimeSlot();
        }

        public CalculationPeriodItem(Guid calculationPeriodItemGuid, Guid calculationGuid, Guid staffingNeedsRowGuid, Guid? suggestedTargetCalculationGuid, Guid staffingNeedsCalcutionHeadRowGuid, Guid originalCalculationRowGuid, Guid periodGuid, Guid? tempGuid, int staffingNeedsHeadId, int staffingNeedsRowId, int staffingNeedsRowPeriodId, int? shiftTypeId, int? timeScheduleTaskId, int? timeScheduleTaskTypeId, int? incomingDeliveryRowId, int? timeCodeBreakGroupId, int? dayTypeId, int employeePostId, PeriodItemsGroupHead staffingNeedsCalcutionHeadRow, StaffingNeedsCalculationTimeSlot timeSlot, List<TimeBreakTemplateTimeSlot> breakTimeSlot, ShiftTypeDTO shiftType, StaffingNeedsRowType type, StaffingNeedsRowOriginType originType, string name, DayOfWeek? weekday, DateTime? date, DateTime startTime, int interval, decimal value, int calculationRowNr, int movedNrOfTimes, int minSplitLength, bool fromBreakRules, bool onlyOneEmployee, bool dontAssignBreakLeftovers, bool allowOverlapping, bool isStaffingNeedsFrequency, bool isBreak, int? parentId, bool ignore, bool forceGrossTime, int tempBreakMinutes, SoeEntityState rowState, SoeEntityState periodState, DateTime? created, string createdBy, DateTime? modified, string modifiedBy, DateTime scheduleDate, SplitInformation splitInformation)
        {
            CalculationPeriodItemGuid = calculationPeriodItemGuid;
            CalculationGuid = calculationGuid;
            StaffingNeedsRowGuid = staffingNeedsRowGuid;
            SuggestedTargetCalculationGuid = suggestedTargetCalculationGuid;
            StaffingNeedsCalcutionHeadRowGuid = staffingNeedsCalcutionHeadRowGuid;
            this.originalCalculationRowGuid = originalCalculationRowGuid;
            PeriodGuid = periodGuid;
            TempGuid = tempGuid;
            StaffingNeedsHeadId = staffingNeedsHeadId;
            StaffingNeedsRowId = staffingNeedsRowId;
            StaffingNeedsRowPeriodId = staffingNeedsRowPeriodId;
            ShiftTypeId = shiftTypeId;
            TimeScheduleTaskId = timeScheduleTaskId;
            TimeScheduleTaskTypeId = timeScheduleTaskTypeId;
            IncomingDeliveryRowId = incomingDeliveryRowId;
            TimeCodeBreakGroupId = timeCodeBreakGroupId;
            DayTypeId = dayTypeId;
            EmployeePostId = employeePostId;
            StaffingNeedsCalcutionHeadRow = staffingNeedsCalcutionHeadRow;
            TimeSlot = timeSlot != null ? timeSlot.Clone() : null;
            BreakTimeSlot = breakTimeSlot;
            ShiftType = shiftType;
            Type = type;
            OriginType = originType;
            Name = name;
            Weekday = weekday;
            Date = date;
            StartTime = startTime;
            Interval = interval;
            Value = value;
            CalculationRowNr = calculationRowNr;
            MovedNrOfTimes = movedNrOfTimes;
            MinSplitLength = minSplitLength;
            FromBreakRules = fromBreakRules;
            OnlyOneEmployee = onlyOneEmployee;
            DontAssignBreakLeftovers = dontAssignBreakLeftovers;
            AllowOverlapping = allowOverlapping;
            IsStaffingNeedsFrequency = isStaffingNeedsFrequency;
            IsBreak = isBreak;
            ParentId = parentId;
            Ignore = ignore;
            this.forceGrossTime = forceGrossTime;
            TempBreakMinutes = tempBreakMinutes;
            RowState = rowState;
            PeriodState = periodState;
            Created = created;
            CreatedBy = createdBy;
            Modified = modified;
            ModifiedBy = modifiedBy;
            ScheduleDate = scheduleDate;
            SplitInformation = splitInformation;
        }





        #endregion

        #region Public methods


        public bool IsUnique(EmployeePostCyclesRun employeePostCyclesRun)
        {

            if (employeePostCyclesRun.IsUnique(this))
                return true;

            return false;
        }

        public void SetForceGrossTime()
        {
            if (this.IsNetTime)
                this.forceGrossTime = true;
        }

        public string GetCompareString()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append(this.TimeScheduleTaskKey);
            stringBuilder.Append("#");
            stringBuilder.Append(this.IncomingDeliveryRowKey);
            stringBuilder.Append("#");
            stringBuilder.Append(this.ScheduleDate.ToShortDateString());
            stringBuilder.Append("#");
            stringBuilder.Append(this.Length);

            return stringBuilder.ToString();
        }

        public string KeyForMerge()
        {
            var sb = new StringBuilder();

            sb.Append(OriginalCalculationRowGuid.ToString());

            sb.Append('#');
            sb.Append(ShiftTypeId.HasValue ? ShiftTypeId.Value.ToString() : "0");

            sb.Append('#');
            if (TimeScheduleTaskId.HasValue)
            {
                sb.Append(!string.IsNullOrEmpty(TimeScheduleTaskKey) ? TimeScheduleTaskKey : TimeScheduleTaskId.Value.ToString());
            }
            else
            {
                sb.Append("0");
            }

            sb.Append('#');
            sb.Append(IncomingDeliveryRowId.HasValue ? IncomingDeliveryRowKey : "");

            return sb.ToString();
        }

        public string GetKey(CalculationPeriodItemGroupByType calculationPeriodItemGroupByType)
        {
            switch (calculationPeriodItemGroupByType)
            {
                case CalculationPeriodItemGroupByType.Unknown:
                    return CalculationGuid.ToString();
                case CalculationPeriodItemGroupByType.Skills:
                    if (ShiftType?.ShiftTypeSkills != null)
                    {
                        int count = ShiftType.ShiftTypeSkills.Count;
                        int idSum = ShiftType.ShiftTypeSkills.Sum(i => i.SkillId);
                        int skillSum = ShiftType.ShiftTypeSkills.Sum(i => i.SkillLevel);

                        if (count + idSum + skillSum != 0)
                            return $"{count}#{idSum}#{skillSum}";
                    }
                    break;
                case CalculationPeriodItemGroupByType.ShiftType:
                    return $"SHIFTTYPEID#{ShiftTypeId}";
                case CalculationPeriodItemGroupByType.ShiftTypeLink:
                    if (ShiftType?.LinkedShiftTypeIds != null && ShiftType.LinkedShiftTypeIds.Any())
                    {
                        var orderedIds = ShiftType.LinkedShiftTypeIds.OrderBy(o => o);
                        return $"SHIFTTYPELINK#{string.Join("#", orderedIds)}";
                    }
                    break;
                case CalculationPeriodItemGroupByType.NoGrouping:
                    return string.Empty;
            }

            // If we get here, it means none of the conditions were met, so we return a default key.
            return $"DEFAULTKEY#{Guid.NewGuid()}";
        }

        public CalculationPeriodItem CloneAndSetNewTimeFrom(DateTime from, int lengthMinutes, bool setNewCalculationPeriodItemGuid = false)
        {
            bool isFixed = this.TimeSlot.IsFixed;
            CalculationPeriodItem newPeriodItem = this.Clone();
            newPeriodItem.Ignore = false; //do not copy ignore flag
            newPeriodItem.TimeSlot = this.TimeSlot.Clone();
            newPeriodItem.TimeSlot.From = from;
            newPeriodItem.TimeSlot.To = from.AddMinutes(lengthMinutes);
            newPeriodItem.StartTime = newPeriodItem.TimeSlot.From;
            if (!newPeriodItem.AllowOverlapping)
                newPeriodItem.TimeSlot.MinFrom = newPeriodItem.TimeSlot.From;

            if (isFixed)
            {
                newPeriodItem.TimeSlot.MaxTo = newPeriodItem.TimeSlot.To;
                newPeriodItem.TimeSlot.MinFrom = newPeriodItem.TimeSlot.From;
            }

            if (setNewCalculationPeriodItemGuid)
                newPeriodItem.CalculationPeriodItemGuid = Guid.NewGuid();

            return newPeriodItem;
        }

        public CalculationPeriodItem CloneAndSetNewTimeTo(DateTime to, int lengthMinutes, bool setNewCalculationPeriodItemGuid = false)
        {
            bool isFixed = this.TimeSlot.IsFixed;
            CalculationPeriodItem newPeriodItem = this.Clone();
            newPeriodItem.Ignore = false; //do not copy ignore flag
            newPeriodItem.TimeSlot = this.TimeSlot.Clone();
            newPeriodItem.TimeSlot.To = to;
            newPeriodItem.TimeSlot.From = newPeriodItem.TimeSlot.MinFrom < to ? to.AddMinutes(-lengthMinutes) : (newPeriodItem.TimeSlot.MinFrom > to ? newPeriodItem.TimeSlot.MinFrom : to);
            newPeriodItem.StartTime = newPeriodItem.TimeSlot.From;
            if (!newPeriodItem.AllowOverlapping)
                newPeriodItem.TimeSlot.MinFrom = newPeriodItem.TimeSlot.From;

            if (isFixed)
            {
                newPeriodItem.TimeSlot.MaxTo = newPeriodItem.TimeSlot.To;
                newPeriodItem.TimeSlot.MinFrom = newPeriodItem.TimeSlot.From;
            }

            if (setNewCalculationPeriodItemGuid)
                newPeriodItem.CalculationPeriodItemGuid = Guid.NewGuid();

            return newPeriodItem;
        }

        public CalculationPeriodItem CloneAndSetNewTime(int lengthMinutes, bool setNewCalculationPeriodItemGuid = false)
        {
            bool isFixed = this.TimeSlot.IsFixed;
            CalculationPeriodItem newPeriodItem = this.Clone();
            newPeriodItem.Ignore = false; //do not copy ignore flag
            newPeriodItem.TimeSlot = this.TimeSlot.Clone();
            newPeriodItem.TimeSlot.From = this.TimeSlot.From.AddMinutes(lengthMinutes);
            newPeriodItem.TimeSlot.To = newPeriodItem.TimeSlot.From.AddMinutes(this.TimeSlot.Minutes - lengthMinutes);
            newPeriodItem.StartTime = newPeriodItem.TimeSlot.From;
            if (!newPeriodItem.AllowOverlapping)
                newPeriodItem.TimeSlot.MinFrom = newPeriodItem.TimeSlot.From;

            if (isFixed)
            {
                newPeriodItem.TimeSlot.MaxTo = newPeriodItem.TimeSlot.To;
                newPeriodItem.TimeSlot.MinFrom = newPeriodItem.TimeSlot.From;
            }

            if (setNewCalculationPeriodItemGuid)
                newPeriodItem.CalculationPeriodItemGuid = Guid.NewGuid();

            return newPeriodItem;
        }

        public bool OverMinSplitLenght(int value)
        {
            if (this.MinSplitLength == 0)
                return true;
            else
                return value > this.MinSplitLength;
        }

        public bool UnderMinSplitLenght(int value)
        {
            if (this.MinSplitLength == 0)
                return false;
            else
                return value < this.MinSplitLength;
        }

        #endregion
    }

    public class CalculationPeriodItemComparer : IEqualityComparer<CalculationPeriodItem>
    {
        public bool Equals(CalculationPeriodItem x, CalculationPeriodItem y)
        {
            return x.OriginalCalculationRowGuid == y.OriginalCalculationRowGuid &&
                   x.ShiftTypeId == y.ShiftTypeId &&
                   x.TimeSlot.From == y.TimeSlot.From &&
                   x.TimeSlot.To == y.TimeSlot.To;
        }

        public int GetHashCode(CalculationPeriodItem obj)
        {
            unchecked // Overflow is fine, just wrap
            {
                int hash = 17;
                hash = hash * 23 + obj.OriginalCalculationRowGuid.GetHashCode();
                hash = hash * 23 + obj.ShiftTypeId.GetHashCode();
                hash = hash * 23 + obj.TimeSlot.From.GetHashCode();
                hash = hash * 23 + obj.TimeSlot.To.GetHashCode();
                return hash;
            }
        }
    }
    public class SplitInformation
    {
        public DateTime? Split1 { get; set; }
        public DateTime? Split2 { get; set; }
        public bool ValidAfterSplit2 { get; set; }
        public bool ValidBeforeSplit1 { get; set; }
        public bool ValidAfterSplit1 { get; set; }
    }

    public class ScheduleRuleEvaluationItem
    {
        #region Variables

        public DayOfWeek DayOfWeek { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime StopTime { get; set; }
        private PeriodItemsGroupHead PeriodItemsGroupHead { get; set; }

        #endregion

        #region Ctor

        public ScheduleRuleEvaluationItem()
        {

        }

        public ScheduleRuleEvaluationItem(DateTime startTime, DateTime stopTime)
        {
            this.DayOfWeek = startTime.DayOfWeek;
            this.StartTime = startTime;
            this.StopTime = stopTime;

        }

        public ScheduleRuleEvaluationItem(DayOfWeek dayOfWeek, DateTime startTime, DateTime stopTime, PeriodItemsGroupHead periodItemsGroupHead = null)
        {
            this.DayOfWeek = dayOfWeek;
            this.StartTime = startTime;
            this.StopTime = stopTime;
            this.PeriodItemsGroupHead = periodItemsGroupHead;
        }

        public PeriodItemsGroupHead GetPeriodItemsGroupHead()
        {
            return this.PeriodItemsGroupHead;
        }

        #endregion
    }


    public static class StaffingNeedsCalculationExtensions
    {
        public static string GetKey(this List<CalculationPeriodItem> possibleWeeks)
        {
            string key = possibleWeeks.Count.ToString();

            foreach (var item in possibleWeeks)
            {
                key += "#" + (item.ShiftTypeId.HasValue ? item.ShiftTypeId.Value.ToString() : "null");
                key += "#" + (item.TimeScheduleTaskId.HasValue ? item.TimeScheduleTaskKey : "null");
                key += "#" + (item.IncomingDeliveryRowId.HasValue ? item.IncomingDeliveryRowKey : "null");
                key += "#" + item.TimeSlot.From.ToString();
                key += "#" + item.TimeSlot.To.ToString();
            }

            return key;
        }

        public static StaffingNeedsCalculationTimeSlot Clone(this StaffingNeedsCalculationTimeSlot e)
        {
            return new StaffingNeedsCalculationTimeSlot()
            {
                CalculationGuid = e.CalculationGuid,
                From = e.From,
                To = e.To,
                IsBreak = e.IsBreak,
                MaxTo = e.MaxTo,
                MinFrom = e.MinFrom
            };
        }

        public static List<TimeBreakTemplateTimeSlot> Clone(this List<TimeBreakTemplateTimeSlot> e)
        {
            List<TimeBreakTemplateTimeSlot> clones = new List<TimeBreakTemplateTimeSlot>();

            foreach (var item in e)
            {
                clones.Add(item.Clone());
            }

            return clones;
        }

        public static TimeBreakTemplateTimeSlot Clone(this TimeBreakTemplateTimeSlot e)
        {
            return new TimeBreakTemplateTimeSlot()
            {
                StartTime = e.StartTime,
                StopTime = e.StopTime
            };

        }

        public static CalculationPeriodItem Clone(this CalculationPeriodItem e)
        {

            return new CalculationPeriodItem()
            {
                CalculationPeriodItemGuid = e.CalculationPeriodItemGuid,
                CalculationGuid = e.CalculationGuid,
                StaffingNeedsRowGuid = e.StaffingNeedsRowGuid,
                SuggestedTargetCalculationGuid = e.SuggestedTargetCalculationGuid,
                StaffingNeedsCalcutionHeadRowGuid = e.StaffingNeedsCalcutionHeadRowGuid,
                OriginalCalculationRowGuid = e.OriginalCalculationRowGuid,
                PeriodGuid = e.PeriodGuid,
                TempGuid = e.TempGuid,
                StaffingNeedsHeadId = e.StaffingNeedsHeadId,
                StaffingNeedsRowId = e.StaffingNeedsRowId,
                StaffingNeedsRowPeriodId = e.StaffingNeedsRowPeriodId,
                ShiftTypeId = e.ShiftTypeId,
                TimeScheduleTaskId = e.TimeScheduleTaskId,
                TimeScheduleTaskTypeId = e.TimeScheduleTaskTypeId,
                IncomingDeliveryRowId = e.IncomingDeliveryRowId,
                IncomingDeliveryRowKey = e.IncomingDeliveryRowKey,
                TimeScheduleTaskKey = e.TimeScheduleTaskKey,
                ReplaceWithBreak = e.ReplaceWithBreak,
                TimeCodeBreakGroupId = e.TimeCodeBreakGroupId,
                DayTypeId = e.DayTypeId,
                EmployeePostId = e.EmployeePostId,
                StaffingNeedsCalcutionHeadRow = e.StaffingNeedsCalcutionHeadRow,
                TimeSlot = e.TimeSlot != null ? e.TimeSlot.Clone() : null,
                BreakTimeSlot = e.BreakTimeSlot != null ? e.BreakTimeSlot.Clone() : null,
                ShiftType = e.ShiftType,
                Type = e.Type,
                OriginType = e.OriginType,
                Name = e.Name,
                Weekday = e.Weekday,
                Date = e.Date,
                StartTime = e.StartTime,
                Interval = e.Interval,
                Value = e.Value,
                CalculationRowNr = e.CalculationRowNr,
                MovedNrOfTimes = e.MovedNrOfTimes,
                MinSplitLength = e.MinSplitLength,
                FromBreakRules = e.FromBreakRules,
                OnlyOneEmployee = e.OnlyOneEmployee,
                DontAssignBreakLeftovers = e.DontAssignBreakLeftovers,
                AllowOverlapping = e.AllowOverlapping,
                IsStaffingNeedsFrequency = e.IsStaffingNeedsFrequency,
                IsBreak = e.IsBreak,
                ParentId = e.ParentId,
                Ignore = e.Ignore,
                TempBreakMinutes = e.TempBreakMinutes,
                RowState = e.RowState,
                PeriodState = e.PeriodState,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                ScheduleDate = e.ScheduleDate,
                SplitInformation = e.SplitInformation
            };
        }

        public static List<CalculationPeriodItem> GetUsedPeriodItemsForDate(this List<EmployeePostCycle> employeePostCycles, DateTime date)
        {
            List<CalculationPeriodItem> periodItems = new List<CalculationPeriodItem>();

            foreach (var cycle in employeePostCycles)
            {
                foreach (var week in cycle.EmployeePostWeeks)
                {
                    foreach (var item in week.EmployeePostDays.Where(i => i.Date == date))
                    {
                        periodItems.AddRange(item.SelectedItemsHead.CalculationPeriodItems);
                    }
                }
            }

            return periodItems;
        }

        public static bool ContainsItem(this List<CalculationPeriodItem> periodItems, CalculationPeriodItem periodItem, bool useContainsObject = true)
        {
            if (!useContainsObject && periodItem.CalculationPeriodItemGuid == new Guid())
                return false;

            if (!useContainsObject && periodItems.Where(w => w.CalculationPeriodItemGuid.Equals(periodItem.CalculationPeriodItemGuid)).Any())
                return true;

            if (periodItems.Contains(periodItem))
                return true;

            return false;
        }

        public static List<CalculationPeriodItem> ToCalculationPeriodItems(this List<StaffingNeedsRowDTO> needRows, List<ShiftTypeDTO> shiftTypes, List<TimeScheduleTaskDTO> timeScheduleTasks, List<IncomingDeliveryHeadDTO> incomingDeliveryHeads, bool newGuidOnPeriod = false, Dictionary<int, int> personsOnDelivery = null, Dictionary<int, int> personsOnTask = null)
        {
            List<CalculationPeriodItem> periodItems = new List<CalculationPeriodItem>();
            personsOnDelivery = personsOnDelivery == null ? new Dictionary<int, int>() : personsOnDelivery;
            personsOnTask = personsOnTask == null ? new Dictionary<int, int>() : personsOnTask;

            foreach (StaffingNeedsRowDTO needRow in needRows)
            {
                Guid guid = Guid.NewGuid();
                bool rowUsed = false;

                foreach (StaffingNeedsRowPeriodDTO period in needRow.Periods)
                {
                    bool dontAssignBreakLeftovers = false;
                    bool onlyOneEmployee = false;
                    bool allowOverlapping = false;
                    bool isStaffingNeedsFrequency = false;
                    int minSplitLength = 0;
                    StaffingNeedsRowOriginType originType = StaffingNeedsRowOriginType.StaffingNeedsAnalysisChartData;
                    StaffingNeedsCalculationTimeSlot timeSlot = period.TimeSlot;
                    bool isFixed = timeSlot?.IsFixed ?? true;

                    if (period.TimeScheduleTaskId.HasValue || period.IncomingDeliveryRowId.HasValue)
                    {
                        if (period.TimeScheduleTaskId.HasValue && timeScheduleTasks != null)
                        {
                            TimeScheduleTaskDTO timeScheduleTask = timeScheduleTasks.FirstOrDefault(t => t.TimeScheduleTaskId == period.TimeScheduleTaskId.Value);
                            if (timeScheduleTask != null)
                            {
                                dontAssignBreakLeftovers = timeScheduleTask.DontAssignBreakLeftovers;
                                onlyOneEmployee = timeScheduleTask.OnlyOneEmployee;
                                allowOverlapping = timeScheduleTask.AllowOverlapping;
                                minSplitLength = timeScheduleTask.MinSplitLength;
                                isStaffingNeedsFrequency = timeScheduleTask.IsStaffingNeedsFrequency;
                                originType = StaffingNeedsRowOriginType.TimeScheduleTask;

                                if (allowOverlapping || timeScheduleTask.NbrOfPersons > 1)
                                {
                                    timeSlot = new StaffingNeedsCalculationTimeSlot(timeScheduleTask.StartTime.Value, timeScheduleTask.StopTime.Value, period.Length);
                                    isFixed = false;
                                }
                                else if (timeSlot == null)
                                {
                                    timeSlot = new StaffingNeedsCalculationTimeSlot(timeScheduleTask.StartTime.HasValue ? timeScheduleTask.StartTime.Value : period.StartTime, timeScheduleTask.StopTime.HasValue ? timeScheduleTask.StopTime.Value : period.StartTime.AddMinutes(period.Length), timeScheduleTask.Length != 0 ? timeScheduleTask.Length : period.Length);
                                    isFixed = timeSlot.IsFixed;
                                }

                                if (timeScheduleTask.NbrOfPersons > 1 && personsOnTask.Any(a => a.Key == period.TimeScheduleTaskId.Value))
                                {
                                    int value;
                                    personsOnTask.TryGetValue(period.TimeScheduleTaskId.Value, out value);
                                    value++;
                                    personsOnTask.Remove(period.TimeScheduleTaskId.Value);
                                    personsOnTask.Add(period.TimeScheduleTaskId.Value, value);
                                }
                                else if (timeScheduleTask.NbrOfPersons > 1)
                                {
                                    personsOnTask.Add(period.TimeScheduleTaskId.Value, 1);
                                }
                            }
                        }
                        else if (period.IncomingDeliveryRowId.HasValue && incomingDeliveryHeads != null)
                        {
                            List<IncomingDeliveryRowDTO> incomingDeliveryRows = new List<IncomingDeliveryRowDTO>();
                            foreach (IncomingDeliveryHeadDTO incomingDeliveryHead in incomingDeliveryHeads)
                            {
                                incomingDeliveryRows.AddRange(incomingDeliveryHead.Rows);
                            }

                            IncomingDeliveryRowDTO incomingDeliveryRow = incomingDeliveryRows.FirstOrDefault(t => t.IncomingDeliveryRowId == period.IncomingDeliveryRowId.Value);
                            if (incomingDeliveryRow != null)
                            {
                                dontAssignBreakLeftovers = incomingDeliveryRow.DontAssignBreakLeftovers;
                                onlyOneEmployee = incomingDeliveryRow.OnlyOneEmployee;
                                allowOverlapping = incomingDeliveryRow.AllowOverlapping;
                                minSplitLength = incomingDeliveryRow.MinSplitLength;
                                timeSlot = new StaffingNeedsCalculationTimeSlot(incomingDeliveryRow.StartTime.Value, incomingDeliveryRow.StopTime.Value, incomingDeliveryRow.Length);
                                isFixed = timeSlot.IsFixed;
                                originType = StaffingNeedsRowOriginType.IncomingDelivery;

                                if (allowOverlapping || incomingDeliveryRow.NbrOfPersons > 1)
                                {
                                    timeSlot = new StaffingNeedsCalculationTimeSlot(incomingDeliveryRow.StartTime.Value, incomingDeliveryRow.StopTime.Value, period.Length);
                                    isFixed = false;
                                }
                                else if (timeSlot == null)
                                {
                                    timeSlot = new StaffingNeedsCalculationTimeSlot(incomingDeliveryRow.StartTime.Value, incomingDeliveryRow.StopTime.Value, incomingDeliveryRow.Length);
                                    isFixed = timeSlot.IsFixed;
                                }
                            }

                            //TODO multiple deliveries on a single day will not work right now.
                            if (!rowUsed && personsOnDelivery.Any(a => a.Key == period.IncomingDeliveryRowId.Value))
                            {
                                int value;
                                personsOnDelivery.TryGetValue(period.IncomingDeliveryRowId.Value, out value);
                                value++;
                                personsOnDelivery.Remove(period.IncomingDeliveryRowId.Value);
                                personsOnDelivery.Add(period.IncomingDeliveryRowId.Value, value);
                                rowUsed = true;
                            }
                            else if (!rowUsed)
                            {
                                personsOnDelivery.Add(period.IncomingDeliveryRowId.Value, 1);
                                rowUsed = true;
                            }
                        }
                    }

                    if (timeSlot == null)
                    {
                        timeSlot = new StaffingNeedsCalculationTimeSlot(period.StartTime, period.StartTime.AddMinutes(period.Length), period.Length);
                    }
                    else
                    {
                        timeSlot.From = period.StartTime;
                        timeSlot.To = period.StartTime.AddMinutes(period.Length);

                        if (isFixed)
                        {
                            timeSlot.MaxTo = timeSlot.To;
                            timeSlot.MinFrom = timeSlot.From;
                        }
                    }

                    CalculationPeriodItem newRow = new CalculationPeriodItem()
                    {
                        CalculationPeriodItemGuid = Guid.NewGuid(),
                        CalculationGuid = newGuidOnPeriod ? Guid.NewGuid() : guid,
                        StaffingNeedsRowGuid = guid,

                        StaffingNeedsHeadId = needRow.StaffingNeedsHeadId,
                        StaffingNeedsRowId = needRow.StaffingNeedsRowId,
                        StaffingNeedsRowPeriodId = period.StaffingNeedsRowPeriodId,
                        ShiftTypeId = period.ShiftTypeId,
                        TimeScheduleTaskId = period.TimeScheduleTaskId,
                        TimeScheduleTaskTypeId = null,
                        IncomingDeliveryRowId = period.IncomingDeliveryRowId,
                        IncomingDeliveryRowKey = string.Empty,
                        TimeScheduleTaskKey = string.Empty,
                        DayTypeId = needRow.DayTypeId,

                        TimeSlot = timeSlot.Clone(),
                        ShiftType = shiftTypes != null ? shiftTypes.FirstOrDefault(s => s.ShiftTypeId == period.ShiftTypeId) : null,

                        Type = needRow.Type,
                        OriginType = originType,
                        Name = needRow.Name,
                        Weekday = needRow.Weekday,
                        Date = needRow.Date,
                        StartTime = period.StartTime,
                        Interval = period.Interval,
                        Value = period.Value,
                        CalculationRowNr = 0,
                        MinSplitLength = minSplitLength,
                        FromBreakRules = false,
                        DontAssignBreakLeftovers = dontAssignBreakLeftovers,
                        OnlyOneEmployee = onlyOneEmployee,
                        AllowOverlapping = allowOverlapping,
                        IsStaffingNeedsFrequency = isStaffingNeedsFrequency,
                        IsBreak = period.IsBreak,
                        ParentId = period.ParentId,

                        PeriodState = period.State,
                        RowState = needRow.State,

                        Created = needRow.Created,
                        CreatedBy = needRow.CreatedBy,
                        Modified = needRow.Modified,
                        ModifiedBy = needRow.ModifiedBy,
                    };

                    if (period.IncomingDeliveryRowId.HasValue && personsOnDelivery.ContainsKey(period.IncomingDeliveryRowId.Value))
                        newRow.SetIncomingDeliveryRowKey(personsOnDelivery[period.IncomingDeliveryRowId.Value]);
                    else if (period.IncomingDeliveryRowId.HasValue)
                        newRow.SetIncomingDeliveryRowKey(0);

                    if (period.TimeScheduleTaskId.HasValue && personsOnTask.ContainsKey(period.TimeScheduleTaskId.Value))
                        newRow.SetTimeScheduleTaskKey(personsOnTask[period.TimeScheduleTaskId.Value]);
                    else if (period.TimeScheduleTaskId.HasValue)
                        newRow.SetTimeScheduleTaskKey(0);


                    periodItems.Add(newRow);
                }
            }

            return periodItems;
        }

        public static List<StaffingNeedsRowDTO> ToStaffingNeedsRowDTOs(this List<CalculationPeriodItem> periodItems, List<ShiftTypeDTO> shiftTypes = null, List<TimeScheduleTaskDTO> timeScheduleTaskDTOs = null, List<IncomingDeliveryHeadDTO> incomingDeliveryHeadDTOs = null)
        {
            List<StaffingNeedsRowDTO> staffingNeedsRowDTOs = new List<StaffingNeedsRowDTO>();

            foreach (var rowGroup in periodItems.GroupBy(s => s.CalculationGuid))
            {
                var row = rowGroup.FirstOrDefault();

                StaffingNeedsRowDTO rowDTO = new StaffingNeedsRowDTO()
                {
                    StaffingNeedsRowId = row.StaffingNeedsRowId,
                    StaffingNeedsHeadId = row.StaffingNeedsHeadId,
                    ShiftTypeId = row.ShiftTypeId,
                    Name = row.Name,
                    Type = (StaffingNeedsRowType)row.Type,
                    Date = row.Date,
                    Created = row.Created,
                    CreatedBy = row.CreatedBy,
                    Modified = row.Modified,
                    ModifiedBy = row.ModifiedBy,
                    State = SoeEntityState.Active,
                    RowNr = row.CalculationRowNr,
                };

                rowDTO.Periods = new List<StaffingNeedsRowPeriodDTO>();

                foreach (var periodinfo in rowGroup)
                {
                    StaffingNeedsCalculationTimeSlot timeSlot = periodinfo.TimeSlot;

                    if (timeSlot == null && (periodinfo.TimeScheduleTaskId.HasValue || periodinfo.IncomingDeliveryRowId.HasValue))
                    {
                        if (periodinfo.TimeScheduleTaskId.HasValue && timeScheduleTaskDTOs != null)
                        {
                            var timeScheduleTaskDTO = timeScheduleTaskDTOs.Where(t => t.TimeScheduleTaskId == periodinfo.TimeScheduleTaskId.Value).FirstOrDefault();

                            if (timeScheduleTaskDTO != null)
                            {
                                timeSlot = new StaffingNeedsCalculationTimeSlot(timeScheduleTaskDTO.StartTime.Value, timeScheduleTaskDTO.StopTime.Value, timeScheduleTaskDTO.Length);
                            }
                        }
                        else if (periodinfo.IncomingDeliveryRowId.HasValue && incomingDeliveryHeadDTOs != null)
                        {
                            List<IncomingDeliveryRowDTO> rows = new List<IncomingDeliveryRowDTO>();

                            foreach (var head in incomingDeliveryHeadDTOs)
                                rows.AddRange(head.Rows);

                            var incomingDeliveryRowDTO = rows.Where(t => t.IncomingDeliveryRowId == periodinfo.IncomingDeliveryRowId.Value).FirstOrDefault();

                            if (incomingDeliveryRowDTO != null)
                            {
                                timeSlot = new StaffingNeedsCalculationTimeSlot(incomingDeliveryRowDTO.StartTime.Value, incomingDeliveryRowDTO.StopTime.Value, incomingDeliveryRowDTO.Length);
                            }
                        }
                    }

                    rowDTO.Periods.Add(new StaffingNeedsRowPeriodDTO()
                    {
                        PeriodGuid = periodinfo.PeriodGuid,
                        StaffingNeedsRowPeriodId = periodinfo.StaffingNeedsRowPeriodId,
                        StaffingNeedsRowId = periodinfo.StaffingNeedsRowId,
                        ShiftTypeId = periodinfo.ShiftTypeId,
                        Interval = periodinfo.Interval,
                        Length = periodinfo.TimeSlot.Minutes,
                        ParentId = periodinfo.ParentId,
                        StartTime = periodinfo.TimeSlot.From,
                        Value = periodinfo.Value,
                        IsBreak = periodinfo.IsBreak,
                        Created = periodinfo.Created,
                        CreatedBy = periodinfo.CreatedBy,
                        Modified = periodinfo.Modified,
                        ModifiedBy = periodinfo.ModifiedBy,
                        State = SoeEntityState.Active,
                        TimeSlot = periodinfo.TimeSlot,
                        IncomingDeliveryRowId = periodinfo.IncomingDeliveryRowId,
                        TimeScheduleTaskId = periodinfo.TimeScheduleTaskId
                    });
                }

                staffingNeedsRowDTOs.Add(rowDTO);
            }

            int rowNr = 1;
            foreach (var item in staffingNeedsRowDTOs.OrderBy(s => s.RowNr))
            {
                if (item.RowNr == 0)
                    item.RowNr = rowNr;
                rowNr++;
            }

            return staffingNeedsRowDTOs;
        }

        public static List<EmployeePostDay> ToEmployeePostDays(this List<PeriodItemsGroupHead> staffingNeedsCalcutionHeads, DateTime date, EmployeePostDTO employeePostDTO)
        {
            List<EmployeePostDay> employeePostDays = new List<EmployeePostDay>();

            foreach (var head in staffingNeedsCalcutionHeads)
            {
                EmployeePostDay day = new EmployeePostDay()
                {
                    AllMatchedCalculationPeriodItems = head.CalculationPeriodItems,
                    Date = date,
                    EmployeePost = employeePostDTO,
                    SelectedItemsHead = head
                };

                employeePostDays.Add(day);
            }

            return employeePostDays;
        }
    }

    public class EmployeePostCyclesRunInformation
    {
        public EmployeePostCyclesRunInformation()
        {
            this.StartTime = DateTime.UtcNow;
        }

        #region Properties

        #region private

        private EmployeePostSortType EmployeePostSortType { get; set; }
        private DateTime StartTime { get; set; }
        private DateTime StopTime { get; set; }
        private List<string> InfoLog { get; set; }

        #endregion

        #region Public



        #endregion

        #endregion

        public List<EmployeePostCycleInformation> EmployeePostCycleInformations { get; set; }

        public void init()
        {
            this.StartTime = DateTime.UtcNow;
        }

        public void Stop()
        {
            this.StopTime = DateTime.UtcNow;
        }

        public void LogInfo(string information)
        {
            this.InfoLog.Add(information);
        }

        public string GetInformationLog(bool clearLog)
        {
            string values = string.Join(Environment.NewLine, this.InfoLog.ToArray());

            if (clearLog)
                this.InfoLog = new List<string>();

            return values;
        }

        #region

        public EmployeePostCyclesRunInformation(EmployeePostSortType employeePostSortType)
        {
            this.init();
            this.EmployeePostCycleInformations = new List<EmployeePostCycleInformation>();
            this.EmployeePostSortType = employeePostSortType;
            this.InfoLog = new List<string>();
        }

        #endregion
    }

    public class EmployeePostCycleInformation
    {
        public EmployeePostCycleInformation()
        {
            this.StartTime = DateTime.UtcNow;
        }

        public int EmployeePostId { get; set; }
        public string EmployeePostName { get; set; }
        public decimal Percent { get; set; }

        public int Length { get; set; }
        public int WeekAttempts
        {
            get
            {
                if (this.EmployeePostWeekInformations != null && this.EmployeePostWeekInformations.Count > 0)
                {
                    int value = 0;
                    foreach (var weekInfo in this.EmployeePostWeekInformations)
                    {
                        value += weekInfo.WeekAttempts;
                    }

                    return value;
                }

                return 0;
            }
        }
        public int NumberOfEmployeePostDays { get; set; }
        public int RemainingMinutes { get; set; }
        public DateTime StartDate { get; set; }
        private DateTime StartTime { get; set; }
        private DateTime StopTime { get; set; }
        public int NumberOfPossibleCycles { get; set; }
        public List<EmployeePostWeekInformation> EmployeePostWeekInformations { get; set; }

        public void init()
        {
            this.StartTime = DateTime.UtcNow;
            this.EmployeePostWeekInformations = new List<EmployeePostWeekInformation>();
        }

        public void Stop()
        {
            this.StopTime = DateTime.UtcNow;
        }

        #region Ctor

        public EmployeePostCycleInformation(int employeePostId, string employeePostName)
        {
            this.init();
            this.EmployeePostId = employeePostId;
            this.EmployeePostName = employeePostName;
        }

        #endregion

        #region Methods

        public void SetEndResult(int percent)
        {
            this.Percent = percent;
        }

        public void AddEmployeePostWeekInformation(EmployeePostWeekInformation employeePostWeekInformation)
        {
            if (this.EmployeePostWeekInformations == null)
                this.EmployeePostWeekInformations = new List<EmployeePostWeekInformation>();

            this.EmployeePostWeekInformations.Add(employeePostWeekInformation);
        }

        public string GetInformationForProgress(bool connectedToDatabase)
        {
            TimeScheduleManager scm = new TimeScheduleManager(null);
            StringBuilder sb = new StringBuilder();
            sb.Append(Environment.NewLine);
            if (connectedToDatabase)
            {
                sb.Append($"{TermCacheManager.Instance.GetText(11644, (int)TermGroup.General, "Tjänst", Thread.CurrentThread.CurrentCulture.Name)}: {this.EmployeePostName}"); sb.Append(Environment.NewLine);
                sb.Append($"{TermCacheManager.Instance.GetText(11645, (int)TermGroup.General, "Schemalagd tid", Thread.CurrentThread.CurrentCulture.Name)}: {CalendarUtility.GetHoursAndMinutesString(this.Length, false)}"); sb.Append(Environment.NewLine);
                sb.Append($"{TermCacheManager.Instance.GetText(11646, (int)TermGroup.General, "Ej Schemalagd tid", Thread.CurrentThread.CurrentCulture.Name)}: {CalendarUtility.GetHoursAndMinutesString(this.RemainingMinutes, false)}"); sb.Append(Environment.NewLine);
                sb.Append($"{TermCacheManager.Instance.GetText(11647, (int)TermGroup.General, "Schemalagda dagar", Thread.CurrentThread.CurrentCulture.Name)}: {this.NumberOfEmployeePostDays}"); sb.Append(Environment.NewLine);
            }
            else
            {
                sb.Append($"Tjänst {this.EmployeePostName}"); sb.Append(Environment.NewLine);
                sb.Append($"Schemalagd tid {CalendarUtility.GetHoursAndMinutesString(this.Length, false)}"); sb.Append(Environment.NewLine);
                sb.Append($"Ej Schemalagd tid {CalendarUtility.GetHoursAndMinutesString(this.RemainingMinutes, false)}"); sb.Append(Environment.NewLine);
                sb.Append($"Schemalagda dagar {this.NumberOfEmployeePostDays}"); sb.Append(Environment.NewLine);
            }
            return sb.ToString();
        }

        #endregion
    }

    public class EmployeePostWeekInformation
    {

        public int EmployeePostId { get; set; }
        public string EmployeePostName { get; set; }
        public decimal Percent { get; set; }
        public int WeekAttempts { get; set; }

        public bool AttemptsWeek { get; set; }
        public int NumberOfEmployeePostDays { get; set; }
        public int RemainingMinutes { get; set; }
        public DateTime StartDate { get; set; }
        private DateTime StartTime { get; set; }
        private DateTime StopTime { get; set; }
        public int NumberOfPossibleWeeks { get; set; }
        public List<EmployeePostDayInformation> EmployeePostDayInformations { get; set; }

        public string DiscardedInformation { get; set; }

        public void init()
        {
            if (this.StartTime == DateTime.MinValue)
                this.StartTime = DateTime.UtcNow;
            this.EmployeePostDayInformations = new List<EmployeePostDayInformation>();
        }

        public void Stop()
        {
            this.StopTime = DateTime.UtcNow;
        }

        #region Ctor

        public EmployeePostWeekInformation(int employeePostId, string employeePostName, DateTime startDate)
        {
            this.init();
            this.EmployeePostId = employeePostId;
            this.EmployeePostName = employeePostName;
            this.StartDate = startDate;

        }

        #endregion

        #region Methods

        public void AddEmployeePostDayInformation(EmployeePostDayInformation employeePostDayInformation)
        {
            this.EmployeePostDayInformations.Add(employeePostDayInformation);
        }

        #endregion
    }

    public class EmployeePostDayInformation
    {
        public DateTime StartTime { get; set; }
        public DateTime StopTime { get; set; }
        public bool Disposed { get; set; }
        public int EmployeePostId { get; set; }
        public string EmployeePostName { get; set; }
        public decimal Length { get; set; }

        public void init()
        {
            if (this.StartTime == DateTime.MinValue)
                this.StartTime = DateTime.UtcNow;
        }

        public void Stop()
        {
            this.StopTime = DateTime.UtcNow;
        }

        #region  Ctor

        public EmployeePostDayInformation(int employeePostId, string employeePostName)
        {
            this.init();
            this.EmployeePostId = employeePostId;
            this.EmployeePostName = employeePostName;
        }

        #endregion

        #region Methods


        #endregion
    }

    #region EmployeePost PreAnalysisInformation
    public class PreAnalysisInformation
    {
        public PreAnalysisInformation()
        {
            this.PreAnalysisInformationDays = new List<PreAnalysisInformationDay>();
        }

        public string Skills
        {
            get
            {
                if (EmployeePost?.EmployeePostSkillDTOs != null)
                    return EmployeePost.EmployeePostSkillDTOs.Select(s => s.SkillName).JoinToString(",");

                return string.Empty;
            }
        }

        public decimal WorkTimePerDay
        {
            get
            {
                if (this.EmployeePost != null)
                    return this.EmployeePost.WorkTimePerDay;

                return 0;
            }
        }

        public bool WorkTimePerDayLessThanRuleWorkTimeDayMinimum
        {
            get
            {
                return WorkTimePerDay < RuleWorkTimeDayMinimum;
            }
        }

        public bool WorkTimePerDayMoreThanRuleWorkTimeDayMaximumWorkDay
        {
            get
            {
                return WorkTimePerDay > RuleWorkTimeDayMaximumWorkDay;
            }
        }

        public decimal RuleWorkTimeDayMaximumWorkDay
        {
            get
            {
                if (this.EmployeePost?.EmployeeGroupDTO != null)
                {
                    int ruleWorkTimeDayMaximumWorkDay = this.EmployeePost.EmployeeGroupDTO.RuleWorkTimeDayMaximumWorkDay;

                    if (ruleWorkTimeDayMaximumWorkDay == 0)
                        ruleWorkTimeDayMaximumWorkDay = 48 * 60;

                    return ruleWorkTimeDayMaximumWorkDay;
                }

                return 0;
            }
        }

        public decimal RuleWorkTimeDayMinimum
        {
            get
            {
                if (this.EmployeePost?.EmployeeGroupDTO != null)
                {
                    int ruleWorkTimeDayMinimum = this.EmployeePost.EmployeeGroupDTO.RuleWorkTimeDayMinimum;

                    if (ruleWorkTimeDayMinimum == 0)
                        ruleWorkTimeDayMinimum = 1;

                    return ruleWorkTimeDayMinimum;
                }

                return 0;
            }
        }

        public decimal WorkTimeWeekMin
        {
            get
            {
                if (this.EmployeePost != null)
                    return this.EmployeePost.WorkTimeWeekMin;

                return 0;
            }
        }

        public decimal WorkTimeWeekMax
        {
            get
            {
                if (this.EmployeePost != null)
                    return this.EmployeePost.WorkTimeWeekMax;

                return 0;
            }
        }

        public EmployeePostDTO EmployeePost { get; set; }
        public List<PreAnalysisInformationDay> PreAnalysisInformationDays { get; set; }
        public List<CalculationPeriodItem> AllEmployeePostPeriodItems { get; set; }
        public List<CalculationPeriodItem> RemainingEmployeePostPeriodItems { get; set; }
        public decimal TotalAllSkillPercent
        {
            get
            {
                int totalMinutes = 0;
                int workTime = this.EmployeePost.WorkTimeWeek;
                foreach (var item in this.AllEmployeePostPeriodItems)
                {
                    totalMinutes += item.Length;
                }

                if (workTime != 0)
                    return decimal.Round(decimal.Multiply(100, decimal.Divide(totalMinutes, workTime)), 2);

                return 0;
            }
        }

        public decimal TotalRemainingSkillPercent
        {
            get
            {
                int totalMinutes = 0;
                int workTime = this.EmployeePost.WorkTimeWeek * this.EmployeePost.GetNumberOfWeeks();
                foreach (var item in this.RemainingEmployeePostPeriodItems)
                {
                    totalMinutes += item.Length;
                }

                if (workTime != 0)
                    return decimal.Round(decimal.Multiply(100, decimal.Divide(totalMinutes, workTime)), 2);

                return 0;
            }
        }
    }
    public class PreAnalysisInformationDay
    {
        public PreAnalysisInformationDay(EmployeePostDTO employeePost, DateTime date, List<CalculationPeriodItem> items)
        {
            this.items = items;
            this.Date = date;
            this.EmployeePost = employeePost;
            this.PreAnalysysInformationDayShifts = new List<PreAnalysysInformationDayShift>();
        }
        public DateTime Date { get; set; }
        public bool FreeDay
        {
            get
            {
                if (this.EmployeePost?.FreeDays != null)
                {
                    return this.EmployeePost.FreeDays.Contains(Date.DayOfWeek);
                }

                return false;
            }
        }
        public List<CalculationPeriodItem> items { get; set; }
        public EmployeePostDTO EmployeePost { get; set; }

        public decimal TotalDaySkillPercent
        {
            get
            {
                int totalMinutes = 0;
                int workTime = this.EmployeePost.WorkTimePerDay;
                foreach (var item in this.items)
                {
                    totalMinutes += item.Length;
                }

                if (workTime != 0)
                    return decimal.Round(decimal.Multiply(100, decimal.Divide(totalMinutes, workTime)), 2);

                return 0;
            }
        }

        public List<PreAnalysysInformationDayShift> PreAnalysysInformationDayShifts { get; set; }
    }
    public class PreAnalysysInformationDayShift
    {
        public PreAnalysysInformationDayShift() { }
        public PreAnalysysInformationDayShift(bool disposed, string disposeReason, List<ShiftTypeSkillDTO> shiftTypeSkillDTOs, DateTime start, DateTime stop, int preferredLength, int length, int breakLength)
        {
            Disposed = disposed;
            DisposeReason = disposeReason;
            Skills = shiftTypeSkillDTOs.Select(s => s.SkillName).JoinToString(",");
            Start = start;
            Stop = stop;
            Length = length;
            PreferredLength = preferredLength;
            BreakLength = breakLength;
        }
        public bool Disposed { get; set; }
        public string DisposeReason { get; set; }
        public string Skills { get; set; }
        public DateTime Start { get; set; }
        public DateTime Stop { get; set; }
        public int Length { get; set; }
        public int BreakLength { get; set; }
        public int PreferredLength { get; set; }
    }

    #endregion



}

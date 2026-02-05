using Newtonsoft.Json;
using SoftOne.Soe.Common.Attributes;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using TypeLite;

namespace SoftOne.Soe.Common.DTO
{
    #region TimeTree

    public class TimeEmployeeTreeDTO
    {
        public string CacheKey { get; set; }
        public int ActorCompanyId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime StopDate { get; set; }
        public TimePeriodDTO TimePeriod { get; set; }
        public TermGroup_AttestTreeGrouping Grouping { get; set; }
        public TermGroup_AttestTreeSorting Sorting { get; set; }

        public SoeAttestTreeMode Mode { get; set; }
        [TsIgnore]
        [JsonIgnore]
        public bool IsModeTimeAttest => this.Mode == SoeAttestTreeMode.TimeAttest;
        [TsIgnore]
        [JsonIgnore]
        public bool IsModePayrollCalculation => this.Mode == SoeAttestTreeMode.PayrollCalculation;

        public List<TimeEmployeeTreeGroupNodeDTO> GroupNodes { get; set; }
        [TsIgnore]
        [JsonIgnore]
        public List<int> GroupNodesIds { get; private set; }
        [TsIgnore]
        [JsonIgnore]
        public List<int> EmployeeNodeIds { get; private set; }
        [TsIgnore]
        [JsonIgnore]
        public bool IsValid { get; set; }

        public TimeEmployeeTreeSettings Settings { get; set; }
        public void SetSettings(TimeEmployeeTreeSettings settings) => this.Settings = TimeEmployeeTreeSettings.Init(settings);

        public TimeEmployeeTreeDTO()
        {
            //Needed for web-api serialization
            this.IsValid = false;
        }
        public TimeEmployeeTreeDTO(Guid cacheKey, int actorCompanyId, TimePeriodDTO timePeriod, SoeAttestTreeMode mode, TermGroup_AttestTreeGrouping grouping, TermGroup_AttestTreeSorting sorting, TimeEmployeeTreeSettings settings)
        {
            this.IsValid = timePeriod?.TimePeriodHead != null && timePeriod.PaymentDate.HasValue && timePeriod.PayrollStartDate.HasValue && timePeriod.PayrollStopDate.HasValue;
            this.TimePeriod = timePeriod;
            Init(cacheKey, actorCompanyId, timePeriod?.StartDate, timePeriod?.StopDate, mode, grouping, sorting, settings);
        }
        public TimeEmployeeTreeDTO(Guid cacheKey, int actorCompanyId, DateTime startDate, DateTime stopDate, SoeAttestTreeMode mode, TermGroup_AttestTreeGrouping grouping, TermGroup_AttestTreeSorting sorting, TimeEmployeeTreeSettings settings)
        {
            this.IsValid = startDate <= stopDate;
            Init(cacheKey, actorCompanyId, startDate, stopDate, mode, grouping, sorting, settings);
        }

        #region Methods

        private void Init(Guid cacheKey, int actorCompanyId, DateTime? startDate, DateTime? stopDate, SoeAttestTreeMode mode, TermGroup_AttestTreeGrouping grouping, TermGroup_AttestTreeSorting sorting, TimeEmployeeTreeSettings settings)
        {
            this.CacheKey = cacheKey.ToString();
            this.ActorCompanyId = actorCompanyId;
            this.StartDate = startDate ?? CalendarUtility.DATETIME_DEFAULT;
            this.StopDate = stopDate ?? CalendarUtility.DATETIME_DEFAULT;
            this.Grouping = grouping;
            this.Sorting = sorting;
            this.Mode = mode;
            this.GroupNodes = new List<TimeEmployeeTreeGroupNodeDTO>();
            this.GroupNodesIds = new List<int>();
            this.EmployeeNodeIds = new List<int>();
            this.SetSettings(settings);
        }

        public DateTime GetTimePeriodEarliestStartDate()
        {
            return this.TimePeriod != null ? CalendarUtility.GetEarliestDate(this.TimePeriod.StartDate, this.TimePeriod.PayrollStartDate) : this.StartDate;
        }

        public DateTime GetTimePeriodLatestStopDate()
        {
            return this.TimePeriod != null ? CalendarUtility.GetLatestDate(this.TimePeriod.StopDate, this.TimePeriod.PayrollStopDate) : this.StopDate;
        }

        public void AddGroupNode(TimeEmployeeTreeGroupNodeDTO groupNode)
        {
            if (this.DoShowGroupNode(groupNode))
                this.GroupNodes.Add(groupNode);

            SetGroupAsHandled(groupNode);
        }

        public TimeEmployeeTreeGroupNodeDTO GetGroupNode(Guid guid)
        {
            return this.GetGroupNodeRecursive(this.GroupNodes, guid);
        }

        private TimeEmployeeTreeGroupNodeDTO GetGroupNodeRecursive(List<TimeEmployeeTreeGroupNodeDTO> groupNodes, Guid guid)
        {
            if (groupNodes.IsNullOrEmpty())
                return null;

            TimeEmployeeTreeGroupNodeDTO targetGroupNode = null;
            foreach (TimeEmployeeTreeGroupNodeDTO groupNode in groupNodes)
            {
                if (groupNode == null)
                    continue;

                if (groupNode.Guid == guid)
                {
                    targetGroupNode = groupNode;
                    break;
                }

                targetGroupNode = GetGroupNodeRecursive(groupNode.ChildGroupNodes, guid);
            }

            return targetGroupNode;
        }

        public bool DoShowGroupNode(TimeEmployeeTreeGroupNodeDTO groupNode, bool calculateChildGroupNodes = false)
        {
            bool doShowGroupNode = groupNode != null && (this.Settings.IncludeEmptyGroups || groupNode.HasEmployeeNodes());
            if (doShowGroupNode && calculateChildGroupNodes)
                this.SetGroupNodeVisibility(groupNode);
            return doShowGroupNode;
        }

        public void SetGroupNodeVisibility()
        {
            this.GroupNodes = GetVisibleGroupNodes(this.GroupNodes);
        }

        private void SetGroupNodeVisibility(TimeEmployeeTreeGroupNodeDTO groupNode)
        {
            groupNode.ChildGroupNodes = GetVisibleGroupNodes(groupNode.ChildGroupNodes);
        }

        private List<TimeEmployeeTreeGroupNodeDTO> GetVisibleGroupNodes(List<TimeEmployeeTreeGroupNodeDTO> groupNodes)
        {
            var groups = new List<TimeEmployeeTreeGroupNodeDTO>();
            if (groupNodes != null)
            {
                foreach (var groupNode in groupNodes)
                {
                    if (this.DoShowGroupNode(groupNode, calculateChildGroupNodes: true))
                        groups.Add(groupNode);
                }
            }
            return groups;
        }

        public void ClearGroupNodes(bool keepAdditional = true)
        {
            if (this.GroupNodes.IsNullOrEmpty())
                return;

            if (keepAdditional)
            {
                this.GroupNodes = this.GroupNodes.Where(i => i.IsAdditional).ToList();
                this.GroupNodesIds = this.GroupNodes.Select(i => i.Id).ToList();
            }
            else
            {
                this.GroupNodes.Clear();
                this.GroupNodesIds.Clear();
            }
        }

        public void SetGroupAsHandled(TimeEmployeeTreeGroupNodeDTO groupNode)
        {
            if (groupNode != null && !this.GroupNodesIds.Contains(groupNode.Id))
                this.GroupNodesIds.Add(groupNode.Id);
        }

        public void SetEmployeeAsHandled(TimeEmployeeTreeNodeDTO employeeNode)
        {
            if (employeeNode != null && !this.EmployeeNodeIds.Contains(employeeNode.EmployeeId))
                this.EmployeeNodeIds.Add(employeeNode.EmployeeId);
        }

        public bool IsEmployeeHandled(int employeeId)
        {
            return this.EmployeeNodeIds.Contains(employeeId);
        }

        public List<int> GetEmployeeIdsFromNodes()
        {
            var employeeIds = new HashSet<int>();
            foreach (var groupNode in this.GroupNodes)
            {
                foreach (int employeeId in groupNode.GetEmployeeIdsDeep())
                {
                    employeeIds.Add(employeeId);
                }
            }
            return employeeIds.ToList();
        }

        public Dictionary<int, List<int>> GetAdditionalEmployeeIds(List<int> excludeEmployeeIds = null)
        {
            List<TimeEmployeeTreeNodeDTO> employeeNodes = this.GroupNodes?.FirstOrDefault(i => i.IsAdditional)?.EmployeeNodes;
            if (!employeeNodes.IsNullOrEmpty() && excludeEmployeeIds != null)
                employeeNodes = employeeNodes.Where(e => !excludeEmployeeIds.Contains(e.EmployeeId)).ToList();
            return employeeNodes?.ToDictionary(k => k.EmployeeId, v => v.AdditionalOnAccountIds) ?? new Dictionary<int, List<int>>();
        }

        public Guid GetCacheKey(bool flush = false)
        {
            if (flush || string.IsNullOrEmpty(this.CacheKey) || !Guid.TryParse(this.CacheKey, out Guid cacheKey))
                cacheKey = Guid.NewGuid();
            return cacheKey;
        }

        #endregion
    }

    public class TimeEmployeeTreeGroupNodeDTO : ITimeTreeNode
    {
        public Guid Guid { get; set; }
        public int Id { get; set; }
        public int Type { get; set; }
        public int DefinedSort { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public bool IsAdditional { get; set; }
        public bool HasEnded { get; set; }
        public bool Expanded { get; set; }

        public List<TimeEmployeeTreeGroupNodeDTO> ChildGroupNodes { get; set; }
        public List<TimeEmployeeTreeNodeDTO> EmployeeNodes { get; set; }
        public List<AttestEmployeePeriodDTO> TimeEmployeePeriods { get; set; }
        [TsIgnore]
        [JsonIgnore]
        private Dictionary<int, TimeEmployeeTreeNodeDTO> EmployeeNodesDict { get; set; }
        public List<AttestStateDTO> AttestStates { get; set; }
        public int AttestStateId { get; set; }
        public int AttestStateSort { get; set; }
        public string AttestStateColor { get; set; }
        public string AttestStateName { get; set; }

        [TsIgnore]
        [JsonIgnore]
        public List<TimeTreeEmployeeWarning> Warnings { get; set; }
        public string WarningMessageTime { get; set; }
        public string WarningMessagePayroll { get; set; }
        public string WarningMessagePayrollStopping { get; set; }
        public bool HasWarningsTime
        {
            get
            {
                return !this.GetWarnings(SoeTimeAttestWarningGroup.Time, false).IsNullOrEmpty();
            }
            set
            {
                //Needed for web-api serialization
                _ = value.ToString(); //prevent compiler warning
            }
        }
        public bool HasWarningsPayroll
        {
            get
            {
                return !this.GetWarnings(SoeTimeAttestWarningGroup.Payroll, false).IsNullOrEmpty();
            }
            set
            {
                //Needed for web-api serialization
                _ = value.ToString(); //prevent compiler warning
            }
        }
        public bool HasWarningsPayrollStopping
        {
            get
            {
                return !this.GetWarnings(SoeTimeAttestWarningGroup.Payroll, true).IsNullOrEmpty();
            }
            set
            {
                //Needed for web-api serialization
                _ = value.ToString(); //prevent compiler warning
            }
        }

        public TimeEmployeeTreeGroupNodeDTO(int id, string name, string code = "", int type = 0, int definedSort = 0, bool isAdditional = false, bool hasEnded = false)
        {
            this.Guid = Guid.NewGuid();
            this.Id = id;
            this.Type = type;
            this.Code = code;
            this.Name = name;
            this.DefinedSort = definedSort;
            this.IsAdditional = isAdditional;
            this.HasEnded = hasEnded;
            this.Expanded = false;

            this.ChildGroupNodes = new List<TimeEmployeeTreeGroupNodeDTO>();
            this.EmployeeNodes = new List<TimeEmployeeTreeNodeDTO>();
            this.EmployeeNodesDict = new Dictionary<int, TimeEmployeeTreeNodeDTO>();
            this.Warnings = new List<TimeTreeEmployeeWarning>();
        }

        public void AddEmployeeNodes(TimeEmployeeTreeDTO tree, List<TimeEmployeeTreeNodeDTO> employeeNodes)
        {
            if (employeeNodes.IsNullOrEmpty())
                return;

            foreach (TimeEmployeeTreeNodeDTO employeeNode in employeeNodes)
            {
                AddEmployeeNode(tree, employeeNode);
            }
        }

        public void AddEmployeeNode(TimeEmployeeTreeDTO tree, TimeEmployeeTreeNodeDTO employeeNode)
        {
            if (this.DoShowEmployeeNode(tree, employeeNode))
            {
                this.EmployeeNodes.Add(employeeNode);
                if (!this.EmployeeNodesDict.ContainsKey(employeeNode.EmployeeId))
                    this.EmployeeNodesDict.Add(employeeNode.EmployeeId, employeeNode);
            }
        }

        public void AddGroupNode(TimeEmployeeTreeDTO tree, TimeEmployeeTreeGroupNodeDTO childGroupNode)
        {
            this.ChildGroupNodes.Add(childGroupNode);
            tree.SetGroupAsHandled(childGroupNode);
        }

        public bool DoShowEmployeeNode(TimeEmployeeTreeDTO tree, TimeEmployeeTreeNodeDTO employeeNode, List<AttestStateDTO> attestStatesForEmployee = null, int? highestAttestStateSort = null)
        {
            if (tree == null || employeeNode == null)
                return false;
            if (tree.Settings.ShowOnlyApplyFinalSalary && employeeNode.FinalSalaryStatus != SoeEmploymentFinalSalaryStatus.ApplyFinalSalary)
                return false;
            if (tree.Settings.ShowOnlyAppliedFinalSalary && employeeNode.FinalSalaryStatus != SoeEmploymentFinalSalaryStatus.AppliedFinalSalary && employeeNode.FinalSalaryStatus != SoeEmploymentFinalSalaryStatus.AppliedFinalSalaryManually)
                return false;
            if (attestStatesForEmployee != null)
            {
                if (!attestStatesForEmployee.Any() && tree.Settings.DoNotShowWithoutTransactions)
                    return false;
                if (attestStatesForEmployee.Any() && highestAttestStateSort.HasValue && !attestStatesForEmployee.Any(i => i.Sort < highestAttestStateSort.Value))
                    return false;
                employeeNode.SetAttestStates(attestStatesForEmployee);
            }

            if (!tree.Settings.FilterAttestStateIds.IsNullOrEmpty() && !tree.Settings.FilterAttestStateIds.Contains(employeeNode.AttestStateId))
                return false;

            return true;
        }

        public TimeEmployeeTreeNodeDTO GetEmployeeNode(int employeeId)
        {
            return this.EmployeeNodesDict.ContainsKey(employeeId) ? this.EmployeeNodesDict[employeeId] : null;
        }

        public List<int> GetEmployeeIdsDeep()
        {
            return GetChildEmployeeNodes().Select(i => i.EmployeeId).Distinct().ToList();
        }

        public List<int> GetEmployeeIdsStamping()
        {
            return this.EmployeeNodes.Where(e => !e.AutogenTimeblocks).Select(e => e.EmployeeId).ToList();
        }

        public bool HasEmployeeNodes()
        {
            return !GetChildEmployeeNodes().IsNullOrEmpty();
        }

        public bool ContainsAnyEmployee(List<int> employeeIds)
        {
            if (employeeIds.IsNullOrEmpty())
                return false;

            List<int> existingEmployeeIds = this.GetEmployeeIdsDeep();
            if (!existingEmployeeIds.IsNullOrEmpty())
            {
                foreach (int employeeId in employeeIds)
                {
                    if (existingEmployeeIds.Contains(employeeId))
                        return true;
                }
            }

            return false;
        }

        private List<TimeEmployeeTreeNodeDTO> GetChildEmployeeNodes()
        {
            List<TimeEmployeeTreeNodeDTO> childEmployeeNodes = new List<TimeEmployeeTreeNodeDTO>();
            childEmployeeNodes.AddRange(this.EmployeeNodes);
            childEmployeeNodes.AddRange(GetChildEmployeeNodesRecursive(this.ChildGroupNodes));
            return childEmployeeNodes;
        }

        private List<TimeEmployeeTreeNodeDTO> GetChildEmployeeNodesRecursive(List<TimeEmployeeTreeGroupNodeDTO> childGroupNodes)
        {
            List<TimeEmployeeTreeNodeDTO> childEmployeeNodes = new List<TimeEmployeeTreeNodeDTO>();
            if (childGroupNodes != null)
            {
                foreach (TimeEmployeeTreeGroupNodeDTO childGroupNode in childGroupNodes)
                {
                    childEmployeeNodes.AddRange(childGroupNode.EmployeeNodes);
                    childEmployeeNodes.AddRange(GetChildEmployeeNodesRecursive(childGroupNode.ChildGroupNodes));
                }
            }
            return childEmployeeNodes;
        }

        public void SetWarnings(TermGroup_TimeTreeWarningFilter filter, List<int> employeeIds = null)
        {
            this.Warnings = new List<TimeTreeEmployeeWarning>();
            if (!this.EmployeeNodes.IsNullOrEmpty())
            {
                switch (filter)
                {
                    case TermGroup_TimeTreeWarningFilter.Any:
                        this.EmployeeNodes = this.EmployeeNodes.Where(e => e.HasAnyWarning()).ToList();
                        break;
                    case TermGroup_TimeTreeWarningFilter.Time:
                        this.EmployeeNodes = this.EmployeeNodes.Where(e => e.HasWarningForGroup(SoeTimeAttestWarningGroup.Time)).ToList();
                        break;
                    case TermGroup_TimeTreeWarningFilter.Payroll:
                        this.EmployeeNodes = this.EmployeeNodes.Where(e => e.HasWarningForGroup(SoeTimeAttestWarningGroup.Payroll)).ToList();
                        break;
                    case TermGroup_TimeTreeWarningFilter.PayrollStopping:
                        this.EmployeeNodes = this.EmployeeNodes.Where(e => e.HasWarningForGroup(SoeTimeAttestWarningGroup.Payroll, onlyStopping: true)).ToList();
                        break;
                }

                this.Warnings.AddRange(this.EmployeeNodes.GetWarnings());
            }
            if (!this.ChildGroupNodes.IsNullOrEmpty())
            {
                this.Warnings.AddRange(this.ChildGroupNodes.GetWarnings());

            }

            this.FormatWarnings();
        }
    }

    public class TimeEmployeeTreeNodeDTO : ITimeTreeNode
    {
        public Guid Guid { get; set; }
        public int GroupId { get; set; }
        public int? UserId { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeNr { get; set; }
        public string EmployeeFirstName { get; set; }
        public string EmployeeLastName { get; set; }
        public string EmployeeName { get; set; }
        public string SocialSec { get; set; }
        public string HibernatingText { get; set; }
        public string EmployeeNrAndName { get; set; } //needed for filtering in angular
        public TermGroup_Sex EmployeeSex { get; set; }
        public int DisbursementMethod { get; set; }
        public string DisbursementMethodName { get; set; }
        public string DisbursementAccountNr { get; set; }
        public bool DisbursementAccountNrIsMissing { get; set; }
        public bool DisbursementMethodIsCash { get; set; }
        public bool DisbursementMethodIsUnknown { get; set; }
        public bool TaxSettingsAreMissing { get; set; }
        public DateTime? EmployeeEndDate { get; set; }
        public int EmployeeGroupId { get; set; }
        public bool AutogenTimeblocks { get; set; }
        public bool IsStamping { get; set; }
        public bool IsAdditional { get; set; }
        public List<int> AdditionalOnAccountIds { get; set; }
        public bool HasEnded { get; set; }
        public TermGroup_TimeReportType TimeReportType { get; set; }
        public SoeEmploymentFinalSalaryStatus FinalSalaryStatus { get; set; }
        public string Tooltip { get; set; }

        public List<AttestStateDTO> AttestStates { get; set; }
        public int AttestStateId { get; set; }
        public int AttestStateSort { get; set; }
        public string AttestStateColor { get; set; }
        public string AttestStateName { get; set; }
        public bool Visible { get; set; } = true;

        public List<TimeTreeEmployeeWarning> Warnings { get; set; }
        public string WarningMessageTime { get; set; }
        public string WarningMessagePayroll { get; set; }
        public string WarningMessagePayrollStopping { get; set; }
        public bool HasWarningsTime
        {
            get
            {
                return !this.GetWarnings(SoeTimeAttestWarningGroup.Time, false).IsNullOrEmpty();
            }
            set
            {
                //Needed for web-api serialization
                _ = value.ToString(); //prevent compiler warning
            }
        }
        public bool HasWarningsPayroll
        {
            get
            {
                return !this.GetWarnings(SoeTimeAttestWarningGroup.Payroll, false).IsNullOrEmpty();
            }
            set
            {
                //Needed for web-api serialization
                _ = value.ToString(); //prevent compiler warning
            }
        }
        public bool HasWarningsPayrollStopping
        {
            get
            {
                return !this.GetWarnings(SoeTimeAttestWarningGroup.Payroll, true).IsNullOrEmpty();
            }
            set
            {
                //Needed for web-api serialization
                _ = value.ToString(); //prevent compiler warning
            }
        }

        public TimeEmployeeTreeNodeDTO()
        {
            this.Warnings = new List<TimeTreeEmployeeWarning>();
        }

        public void SetWarnings(List<TimeTreeEmployeeWarning> timeWarnings, List<TimeTreeEmployeeWarning> payrollWarnings)
        {
            this.Warnings = timeWarnings.Concat(payrollWarnings).ToList();
            this.FormatWarnings();
        }

        public bool HasAnyWarning()
        {
            return this.Warnings.Any();
        }
        public bool HasWarningForGroup(SoeTimeAttestWarningGroup group, bool onlyStopping = false)
        {
            return this.Warnings.Exists(w => w.WarningGroup == group && (!onlyStopping || w.IsStopping));
        }
    }

    public class TimeEmployeeTreeSettings
    {
        public List<int> FilterEmployeeAuthModelIds { get; set; }
        public List<int> FilterEmployeeIds { get; set; }
        public List<int> FilterAttestStateIds { get; set; }
        public int? FilterMessageGroupId { get; set; }
        public string SearchPattern { get; set; }
        public string CacheKeyToUse { get; set; }

        [TsIgnore]
        [JsonIgnore]
        public SoeAttestTreeLoadMode LoadMode { get; set; }
        [TsIgnore]
        [JsonIgnore]
        public TermGroup_TimeTreeWarningFilter WarningFilter { get; set; }
        public bool IncludeEnded { get; set; }

        //TimeAttest
        public bool IncludeEmptyGroups { get; set; }
        public bool DoNotShowAttested { get; set; }
        public bool DoNotShowWithoutTransactions { get; set; }
        public bool IncludeAdditionalEmployees { get; set; }
        public bool DoNotShowDaysOutsideEmployeeAccount { get; set; }
        public bool DoShowOnlyShiftSwaps { get; set; }
        public bool IsProjectAttest { get; set; }
        public bool ExcludeDuplicateEmployees { get; set; }

        //PayrollCalculation
        public bool ShowOnlyApplyFinalSalary { get; set; }
        public bool ShowOnlyAppliedFinalSalary { get; set; }
        public bool DoRefreshFinalSalaryStatus { get; set; }
        public bool DoNotShowCalculated { get; set; }

        [TsIgnore]
        [JsonIgnore]
        public bool HasFilterOnEmployees
        {
            get
            {
                return !this.FilterEmployeeIds.IsNullOrEmpty() || !String.IsNullOrEmpty(this.SearchPattern);
            }
        }
        [TsIgnore]
        [JsonIgnore]
        public bool HasFilterOnMessageGroupId
        {
            get
            {
                return FilterMessageGroupId.HasValue && FilterMessageGroupId.Value > 0;
            }
        }
        [TsIgnore]
        [JsonIgnore]
        public bool HasFilterOnAttestStates
        {
            get
            {
                return !this.FilterAttestStateIds.IsNullOrEmpty();
            }
        }
        [TsIgnore]
        [JsonIgnore]
        public bool DoLoadTransactions
        {
            get
            {
                return this.LoadMode == SoeAttestTreeLoadMode.Full || this.LoadMode == SoeAttestTreeLoadMode.OnlyEmployeesAndIsAttested;
            }
        }

        public TimeEmployeeTreeSettings()
        {
            this.LoadMode = SoeAttestTreeLoadMode.Full;
        }

        #region Public methods

        public bool HasFilter()
        {
            return !this.FilterAttestStateIds.IsNullOrEmpty() || !this.FilterEmployeeIds.IsNullOrEmpty() || !this.FilterEmployeeAuthModelIds.IsNullOrEmpty();
        }
        public void SetIncludeAdditionalEmployees(bool value)
        {
            this.IncludeAdditionalEmployees = value;
        }
        public void SetExcludeDuplicateEmployees(bool value)
        {
            this.ExcludeDuplicateEmployees = value;
        }

        #endregion

        #region Static methods

        public static TimeEmployeeTreeSettings Init(TimeEmployeeTreeSettings settings = null)
        {
            if (settings != null)
            {
                settings.FilterEmployeeAuthModelIds = settings.FilterEmployeeAuthModelIds.ToNullable();
                settings.FilterEmployeeIds = settings.FilterEmployeeIds.ToNullable();
            }
            else
                settings = new TimeEmployeeTreeSettings();

            return settings;
        }

        #endregion
    }

    public abstract class TimeEmployeeTreeAdditional
    {
        public int EmployeeId { get; private set; }
        public int AccountId { get; private set; }

        protected TimeEmployeeTreeAdditional(int employeeId, int accountId)
        {
            this.EmployeeId = employeeId;
            this.AccountId = accountId;
        }
    }

    public class TimeEmployeeTreeAdditionalCategory : TimeEmployeeTreeAdditional
    {
        public int CategoryId { get; private set; }

        public TimeEmployeeTreeAdditionalCategory(int employeeId, int accountId, int categoryId) : base(employeeId, accountId)
        {
            this.CategoryId = categoryId;
        }
    }

    public class TimeEmployeeTreeAdditionalAccount : TimeEmployeeTreeAdditional
    {
        public TimeEmployeeTreeAdditionalAccount(int employeeId, int accountId) : base(employeeId, accountId)
        {

        }
    }

    public class TimeTreeWarningsRepository
    {
        public TimeTreeWarningsRepository()
        {
        }

        private List<TimeTreeWarning<SoeTimeAttestWarning>> timeWarnings;
        public List<TimeTreeWarning<SoeTimeAttestWarning>> GetTimeWarnings() => this.timeWarnings;
        public void AddTimeWarnings(SoeTimeAttestWarning warning, List<int> employeeIds)
        {
            if (employeeIds.IsNullOrEmpty())
                return;

            AddWarning(ref this.timeWarnings, employeeIds, warning);
        }

        private List<TimeTreeWarning<TermGroup_PayrollControlFunctionType>> payrollWarnings;
        public List<TimeTreeWarning<TermGroup_PayrollControlFunctionType>> GetPayrollWarnings() => this.payrollWarnings;
        public void AddPayrollWarnings(Dictionary<TermGroup_PayrollControlFunctionType, List<int>> employeeIdsByType)
        {
            if (employeeIdsByType.IsNullOrEmpty())
                return;

            foreach (var type in employeeIdsByType)
            {
                AddWarning(ref this.payrollWarnings, type.Value, type.Key, SoeTimeAttestWarning.PayrollControlFunction);
            }
        }

        private static TimeTreeWarning<T> GetWarning<T>(List<TimeTreeWarning<T>> list, T type) where T : Enum => list.Find(warning => warning.Type.Equals(type));
        private void AddWarning<T>(ref List<TimeTreeWarning<T>> list, List<int> employeeIds, T type, SoeTimeAttestWarning? parentType = null) where T : Enum
        {
            if (list == null)
                list = new List<TimeTreeWarning<T>>();

            var timeTreeWarning = GetWarning(list, type);
            if (timeTreeWarning == null)
                list.Add(new TimeTreeWarning<T>(employeeIds, type, parentType));
            else
                timeTreeWarning.EmployeeIds.AddRange(employeeIds);
        }
    }

    public class TimeTreeWarning<T> where T : Enum
    {
        public List<int> EmployeeIds { get; private set; }
        public T Type { get; private set; }
        public SoeTimeAttestWarning? ParentType { get; private set; }
        public int Key { get; set; }

        public string Message { get; private set; }
        public void SetMessage(string message) => this.Message = message;

        public TimeTreeWarning(List<int> employeeIds, T type, SoeTimeAttestWarning? parentType)
        {
            this.EmployeeIds = employeeIds ?? new List<int>();
            this.Type = type;
            this.ParentType = parentType;

            if (this.Type is SoeTimeAttestWarning)
                this.Key = (int)(SoeTimeAttestWarning)(object)this.Type;
            else if (this.Type is TermGroup_PayrollControlFunctionType)
                this.Key = (int)SoeTimeAttestWarning.PayrollControlFunction + (int)(object)this.Type;
        }

        public bool IsStoppingPayrollWarning() => this.Type is TermGroup_PayrollControlFunctionType && PayrollWarningsUtil.GetStoppingPayrollWarnings().Contains((TermGroup_PayrollControlFunctionType)(object)this.Type);

        public static List<SoeTimeAttestWarning> GetTimeStampWarnings() => new List<SoeTimeAttestWarning>
        {
            SoeTimeAttestWarning.TimeStampErrors,
            SoeTimeAttestWarning.TimeStampsWithoutTransactions
        };
        public bool IsTimeStampWarning() => this.Type is SoeTimeAttestWarning && GetTimeStampWarnings().Contains((SoeTimeAttestWarning)(object)this.Type);
    }

    public abstract class TimeTreeEmployeeWarning
    {
        public int EmployeeId { get; set; }
        public int Key { get; set; }
        public SoeTimeAttestWarningGroup WarningGroup { get; set; }
        public bool IsStopping { get; set; }
        public string Message { get; set; }
    }

    public class TimeTreeEmployeeTimeWarning : TimeTreeEmployeeWarning
    {
        public SoeTimeAttestWarning Type { get; set; }
        public bool IsTimeStampingWarning { get; set; }

        public TimeTreeEmployeeTimeWarning(int employeeId, TimeTreeWarning<SoeTimeAttestWarning> warning)
        {
            base.EmployeeId = employeeId;
            this.Type = warning?.Type ?? SoeTimeAttestWarning.None;
            this.Message = warning?.Message;
            base.Key = warning?.Key ?? 0;
            base.WarningGroup = SoeTimeAttestWarningGroup.Time;
            this.IsTimeStampingWarning = warning?.IsTimeStampWarning() ?? false;
        }
    }

    public class TimeTreeEmployeePayrollWarning : TimeTreeEmployeeWarning
    {
        public TermGroup_PayrollControlFunctionType Type { get; set; }
        public SoeTimeAttestWarning? ParentType { get; set; }

        public TimeTreeEmployeePayrollWarning(int employeeId, TimeTreeWarning<TermGroup_PayrollControlFunctionType> warning)
        {
            base.EmployeeId = employeeId;
            this.Type = warning?.Type ?? TermGroup_PayrollControlFunctionType.None;
            this.ParentType = warning?.ParentType;
            this.Message = warning?.Message;
            base.Key = warning?.Key ?? 0;
            base.WarningGroup = SoeTimeAttestWarningGroup.Payroll;
            base.IsStopping = warning?.IsStoppingPayrollWarning() ?? false;
        }
    }

    public interface ITimeTreeNode : IAttestStateLowest
    {
        List<AttestStateDTO> AttestStates { get; set; }
        List<TimeTreeEmployeeWarning> Warnings { get; set; }
        string WarningMessageTime { get; set; }
        string WarningMessagePayroll { get; set; }
        string WarningMessagePayrollStopping { get; set; }
        bool HasWarningsTime { get; set; }
        bool HasWarningsPayroll { get; set; }
        bool HasWarningsPayrollStopping { get; set; }
    }

    #endregion

    #region PayrollCalculation

    public class PayrollCalculationProductDTO : IPayrollType, IAttestStateLowest
    {
        #region Variables

        //General
        public string UniqueId { get; set; }

        //Employee
        public int EmployeeId { get; set; }

        //PayrollProduct
        public int PayrollProductId { get; set; }
        public string PayrollProductNumber { get; set; }
        public string PayrollProductNumberSort
        {
            get
            {
                return !String.IsNullOrEmpty(this.PayrollProductNumber) ? this.PayrollProductNumber.PadLeft(100, '0') : String.Empty;
            }
        }
        public string PayrollProductName { get; set; }
        public string PayrollProductString
        {
            get
            {
                string text = "";
                if (!String.IsNullOrEmpty(this.PayrollProductNumber) && !String.IsNullOrEmpty(this.PayrollProductName))
                    text = $"{PayrollProductNumber}, {(this.IsRetroactive ? "(R)" : "")}{PayrollProductName}";
                return text;
            }
        }
        public string PayrollProductShortName { get; set; }
        public decimal PayrollProductFactor { get; set; }
        public bool PayrollProductPayed { get; set; }
        public bool PayrollProductExport { get; set; }

        //TimePayrollTransaction
        public List<AttestPayrollTransactionDTO> AttestPayrollTransactions { get; set; }
        public DateTime? DateFrom { get; set; }
        public string DateFromString
        {
            get
            {
                if (HideDate || !DateFrom.HasValue)
                    return string.Empty;
                return DateFrom.Value.ToString("yyyy-MM-dd");
            }
        }
        public DateTime? DateTo { get; set; }
        public string DateToString
        {
            get
            {
                if (HideDate || !DateTo.HasValue)
                    return string.Empty;
                return DateTo.Value.ToString("yyyy-MM-dd");
            }
        }
        public string TransactionComment
        {
            get
            {
                return IsAdded ? AttestPayrollTransactions.First().Comment : string.Empty;

            }
        }
        public decimal? Quantity { get; set; }
        public string QuantityString
        {
            get
            {
                if (HideQuantity)
                    return string.Empty;
                return IsQuantityOrFixed ? this.Quantity.ToString() : CalendarUtility.GetHoursAndMinutesString(Convert.ToInt32(this.Quantity), false);
            }
        }
        public string Quantity_HH_DD_String
        {
            get
            {
                if (HideQuantity)
                    return string.Empty;
                return IsQuantityOrFixed ? this.Quantity.ToString() : Decimal.Round((this.Quantity ?? 0) / 60M, 2).ToString();
            }
        }

        public bool IsAdded
        {
            get
            {
                return (AttestPayrollTransactions != null && AttestPayrollTransactions.Any() && AttestPayrollTransactions.First().IsAdded);
            }
        }
        public bool IsQuantityOrFixed
        {
            get
            {
                return (this.IsQuantityDays || (AttestPayrollTransactions != null && AttestPayrollTransactions.First().IsQuantityOrFixed));
            }
        }
        public bool IsQuantityDays
        {
            get
            {
                return (this.TimeUnit != (int)TermGroup_PayrollProductTimeUnit.Hours);
            }
        }
        public decimal QuantityWorkDays
        {
            get
            {
                if (AttestPayrollTransactions != null)
                    return AttestPayrollTransactions.First().QuantityWorkDays;
                else
                    return 0;
            }
        }
        public decimal QuantityCalendarDays
        {
            get
            {
                if (AttestPayrollTransactions != null)
                    return AttestPayrollTransactions.First().QuantityCalendarDays;
                else
                    return 0;
            }
        }
        public decimal CalenderDayFactor
        {
            get
            {
                if (AttestPayrollTransactions != null)
                    return AttestPayrollTransactions.First().CalenderDayFactor;
                else
                    return 0;
            }
        }
        public bool HideUnitprice
        {
            get
            {
                return this.HasMultipleUnitPrice || this.IsEmploymentTax() || this.IsSupplementCharge() || this.IsTaxAndNotOptional() || this.IsNetSalary() || this.IsSalaryDistress() || this.IsVacationCompensationDirectPaid() || this.IsCentRounding || this.IsUnionFee() || this.IsEmployeeVehicleTransaction();
            }
        }
        public bool HideQuantity
        {
            get
            {
                return this.IsEmploymentTax() || this.IsSupplementCharge() || this.IsTaxAndNotOptional() || this.IsNetSalary() || this.IsSalaryDistress() || this.IsVacationCompensationDirectPaid() || this.IsCentRounding || this.IsUnionFee() || this.IsEmployeeVehicleTransaction();
            }
        }
        public bool HideDate
        {
            get
            {
                return this.IsEmploymentTax() || this.IsSupplementCharge() || this.IsTaxAndNotOptional() || this.IsNetSalary() || this.IsSalaryDistress() || this.IsVacationCompensationDirectPaid() || this.IsRounding || this.IsUnionFee() || this.IsEmployeeVehicleTransaction();
            }
        }
        public bool IsRounding
        {
            get
            {
                return this.IsCentRounding || this.IsQuantityRounding;
            }
        }
        public decimal? UnitPrice { get; set; }
        public string UnitPriceString
        {
            get
            {
                return HideUnitprice ? string.Empty : (UnitPrice ?? 0).ToString();
            }
        }
        public decimal? UnitPriceCurrency { get; set; }
        public string UnitPriceCurrencyString
        {
            get
            {
                return HideUnitprice ? string.Empty : (UnitPriceCurrency ?? 0).ToString();
            }
        }
        public decimal? UnitPriceEntCurrency { get; set; }
        public string UnitPriceEntCurrencyString
        {
            get
            {
                return HideUnitprice ? string.Empty : (UnitPriceEntCurrency ?? 0).ToString();
            }
        }
        public decimal? Amount { get; set; }
        public decimal? AmountCurrency { get; set; }
        public decimal? AmountEntCurrency { get; set; }
        public bool HasPayrollTransactions { get; set; }
        public bool HasPayrollScheduleTransactions { get; set; }
        public bool HasComment { get; set; }
        public bool IsCentRounding { get; set; }
        public bool IsQuantityRounding { get; set; }
        public bool IncludedInPayrollProductChain { get; set; }
        public int? SysPayrollTypeLevel1 { get; set; }
        public int? SysPayrollTypeLevel2 { get; set; }
        public int? SysPayrollTypeLevel3 { get; set; }
        public int? SysPayrollTypeLevel4 { get; set; }
        public string SysPayrollTypeLevel1Name { get; set; }
        public string SysPayrollTypeLevel2Name { get; set; }
        public string SysPayrollTypeLevel3Name { get; set; }
        public string SysPayrollTypeLevel4Name { get; set; }
        public int TimeUnit { get; set; }
        public bool IsAverageCalculated { get; set; }
        public bool IsMonthlySalary
        {
            get { return this.IsMonthlySalary(); }
        }
        public bool IsRetroactive { get; set; }
        public bool IsVacationFiveDaysPerWeek { get; set; }

        //AttestState
        public List<AttestStateDTO> AttestStates { get; set; }
        public int AttestStateId { get; set; }
        public int AttestStateSort { get; set; }
        public string AttestStateColor { get; set; }
        public string AttestStateName { get; set; }
        public bool HasSameAttestState { get; set; } //Is not calculated because in some cases dependent on AttestPayrollTransactionDTO.HasSameAttestState
        public bool HasAttestStates
        {
            get
            {
                return this.AttestStates != null && this.AttestStates.Any(i => i.AttestStateId > 0);
            }
        }

        //AttestTransitions
        public List<AttestTransitionLogDTO> AttestTransitionLogs { get; set; }

        //Accounting
        public List<AccountDimDTO> AccountDims { get; set; }
        public AccountDTO AccountStd { get; set; }
        public List<AccountDTO> AccountInternals { get; set; }
        public string AccountingShortString { get; set; }
        public string AccountingLongString { get; set; }

        //Flags      
        public bool HasInfo { get; set; }
        public bool IsBelowEmploymentTaxLimitRuleHidden { get; set; }
        public bool IsBelowEmploymentTaxLimitRuleFromPreviousPeriods { get; set; }
        [TsIgnore]
        public bool HasMultipleUnitPrice { get; set; }
        [TsIgnore]
        public bool HasMultipleUnitPriceCurrency { get; set; }
        [TsIgnore]
        public bool HasMultipleUnitPriceEntCurrency { get; set; }
        [TsIgnore]
        public bool HasMultipleIsCentRounding { get; set; }
        [TsIgnore]
        public bool HasMultipleIsQuantityRounding { get; set; }
        [TsIgnore]
        public bool HasMultipleIncludedInPayrollProductChain { get; set; }
        [TsIgnore]
        public bool HasMultipleIsAverageCalculated { get; set; }
        [TsIgnore]
        public bool HasMultipleTimeUnit { get; set; }

        #endregion

        #region Ctor

        public PayrollCalculationProductDTO()
        {
            this.UniqueId = Guid.NewGuid().ToString();
            this.AttestPayrollTransactions = new List<AttestPayrollTransactionDTO>();
            this.AttestStates = new List<AttestStateDTO>();
            this.AttestTransitionLogs = new List<AttestTransitionLogDTO>();
            this.AccountInternals = new List<AccountDTO>();
        }

        #endregion
    }

    public class PayrollCalculationEmployeePeriodDTO : IAttestStateLowest
    {
        #region Variables

        public int EmployeeId { get; set; }
        public int TimePeriodId { get; set; }
        public string EmployeeNr { get; set; }
        public string EmployeeName { get; set; }
        public string EmployeeNrAndName
        {
            get
            {
                return $"{this.EmployeeNr} {this.EmployeeName}";
            }
        }
        public PayrollCalculationPeriodSumDTO PeriodSum { get; set; }
        public List<AttestStateDTO> AttestStates { get; set; }
        public bool HasAttestStates
        {
            get
            {
                return !this.AttestStates.IsNullOrEmpty();
            }
        }
        public int AttestStateId { get; set; }
        public int AttestStateSort { get; set; }
        public string AttestStateName { get; set; }
        public string AttestStateColor { get; set; }
        public DateTime? CreatedOrModified { get; set; }

        #endregion
    }

    public class PayrollCalculationPeriodSumDTO
    {
        #region Variables

        private decimal gross;
        public decimal Gross
        {
            get
            {
                return Math.Round(this.gross, 2);
            }
            set
            {
                this.gross = value;
            }
        }
        private decimal benefitInvertExcluded;
        public decimal BenefitInvertExcluded
        {
            get
            {
                return Math.Round(this.benefitInvertExcluded, 2);
            }
            set
            {
                this.benefitInvertExcluded = value;
            }
        }
        private decimal tax;
        public decimal Tax
        {
            get
            {
                return Math.Round(this.tax, 2);
            }
            set
            {
                this.tax = value;
            }
        }
        private decimal compensation;
        public decimal Compensation
        {
            get
            {
                return Math.Round(this.compensation, 2);
            }
            set
            {
                this.compensation = value;
            }
        }
        private decimal deduction;
        public decimal Deduction
        {
            get
            {
                return Math.Round(this.deduction, 2);
            }
            set
            {
                this.deduction = value;
            }
        }
        private decimal employmentTaxDebit;
        public decimal EmploymentTaxDebit
        {
            get
            {
                return Math.Round(this.employmentTaxDebit, 2);
            }
            set
            {
                this.employmentTaxDebit = value;
            }
        }
        private decimal employmentTaxCredit;
        public decimal EmploymentTaxCredit
        {
            get
            {
                return Math.Round(this.employmentTaxCredit, 2);
            }
            set
            {
                this.employmentTaxCredit = value;
            }
        }
        private decimal supplementChargeDebit;
        public decimal SupplementChargeDebit
        {
            get
            {
                return Math.Round(this.supplementChargeDebit, 2);
            }
            set
            {
                this.supplementChargeDebit = value;
            }
        }
        private decimal supplementChargeCredit;
        public decimal SupplementChargeCredit
        {
            get
            {
                return Math.Round(this.supplementChargeCredit, 2);
            }
            set
            {
                this.supplementChargeCredit = value;
            }
        }
        private decimal net;
        public decimal Net
        {
            get
            {
                return Math.Round(this.net, 2);
            }
            set
            {
                this.net = value;
            }
        }
        private decimal transactionNet;
        public decimal TransactionNet
        {
            get
            {
                return Math.Round(this.transactionNet, 2);
            }
            set
            {
                this.transactionNet = value;
            }
        }

        #region Warnings

        public bool HasWarning
        {
            get
            {
                return (this.IsTaxMissing || this.IsEmploymentTaxMissing || this.HasEmploymentTaxDiff || this.HasSupplementChargeDiff || this.IsNetSalaryMissing || this.IsNetSalaryNegative || this.HasNetSalaryDiff || this.IsGrossSalaryNegative);
            }
        }
        public bool IsTaxMissing
        {
            get
            {
                return this.Tax == 0;
            }
        }
        public bool IsEmploymentTaxMissing
        {
            get
            {
                return (this.EmploymentTaxCredit == 0 || this.EmploymentTaxDebit == 0);
            }
        }
        public bool HasEmploymentTaxDiff
        {
            get
            {
                return (Math.Abs(this.EmploymentTaxCredit) != Math.Abs(this.EmploymentTaxDebit));
            }
        }
        public bool HasSupplementChargeDiff
        {
            get
            {
                return (Math.Abs(this.SupplementChargeCredit) != Math.Abs(this.SupplementChargeDebit));
            }
        }
        public bool IsGrossSalaryNegative
        {
            get
            {
                return this.Gross < 0;
            }
        }
        public bool IsNetSalaryMissing
        {
            get
            {
                return this.Net == 0;
            }
        }
        public bool IsNetSalaryNegative
        {
            get
            {
                return this.Net < 0;
            }
        }
        public bool HasNetSalaryDiff
        {
            get
            {
                return this.TransactionNet != this.Net;
            }
        }
        public bool IsBenefitNegative
        {
            get
            {
                return this.BenefitInvertExcluded < 0;
            }
        }
        #endregion

        #endregion

        #region Ctor

        public PayrollCalculationPeriodSumDTO()
        {
            this.Gross = 0;
            this.BenefitInvertExcluded = 0;
            this.Tax = 0;
            this.Compensation = 0;
            this.Deduction = 0;
            this.EmploymentTaxDebit = 0;
        }

        #endregion
    }

    public class PayrollCalculationPeriodSumItemDTO
    {
        public int? SysPayrollTypeLevel1 { get; set; }
        public int? SysPayrollTypeLevel2 { get; set; }
        public int? SysPayrollTypeLevel3 { get; set; }
        public int? SysPayrollTypeLevel4 { get; set; }
        public decimal? Amount { get; set; }


        public static PayrollCalculationPeriodSumItemDTO Create(int? sysPayrollTypeLevel1, int? sysPayrollTypeLevel2, int? sysPayrollTypeLevel3, int? sysPayrollTypeLevel4, decimal? amount)
        {
            return new PayrollCalculationPeriodSumItemDTO
            {
                SysPayrollTypeLevel1 = sysPayrollTypeLevel1,
                SysPayrollTypeLevel2 = sysPayrollTypeLevel2,
                SysPayrollTypeLevel3 = sysPayrollTypeLevel3,
                SysPayrollTypeLevel4 = sysPayrollTypeLevel4,
                Amount = amount
            };
        }
    }

    #endregion

    #region TimeAttest

    public class AttestEmployeeListDTO
    {
        #region Variables

        public int EmployeeId { get; set; }
        public string EmployeeNr { get; set; }
        public string EmployeeNrSort
        {
            get
            {
                return EmployeeNr.PadLeft(50, '0');
            }
        }
        public string Name { get; set; }
        public List<EmployeeListEmploymentDTO> Employments { get; set; }
        public List<CompanyCategoryRecordDTO> CategoryRecords { get; set; }

        #endregion

        #region Public methods

        // Get employment within specified period
        public EmployeeListEmploymentDTO GetEmployment(DateTime dateFrom, DateTime dateTo)
        {
            EmployeeListEmploymentDTO result = null;

            DateTime date = dateFrom;
            while (date <= dateTo)
            {
                result = EmployeeListEmploymentDTO.GetEmployment(this.Employments, date);
                if (result != null)
                    break;

                date = date.AddDays(1);
            }

            return result;
        }

        public bool HasEmployment(DateTime date)
        {
            return EmployeeListEmploymentDTO.HasEmployment(this.Employments, date);
        }

        #endregion
    }

    public class AttestUserRoleDTO
    {
        #region Variables

        public int AttestStateFromId { get; set; }
        public int AttestStateToId { get; set; }
        public int AttestTransitionId { get; set; }
        public int UserId { get; set; }
        public int AttestRoleId { get; set; }
        public int ActorCompanyId { get; set; }
        public SoeModule Module { get; set; }
        public bool ShowUncategorized { get; set; }
        public bool ShowAllCategories { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public decimal MaxAmount { get; set; }

        #endregion
    }

    public class AttestEmployeeDayDTO : IAttestStateLowest
    {
        #region Variables

        public string UniqueId { get; set; }

        #region Employee

        public int EmployeeId { get; set; }
        public int EmployeeGroupId { get; set; }
        public bool AutogenTimeblocks { get; set; }
        public int TimeReportType { get; set; }
        public bool IsReadonly
        {
            get
            {
                return HasScheduledPlacement || IsPrel || IsAttested || HasNoneInitialTransactions;
            }
        }

        #endregion

        #region Calendar

        public DateTime Date { get; set; }
        public int TimeBlockDateId { get; set; }
        public int WeekNr { get; set; }
        public string WeekNrMonday { get; set; }
        public string WeekInfo { get; set; }
        public int DayOfWeekNr { get; set; }
        public int Day { get; set; }
        public string DayName { get; set; }
        public int? HolidayId { get; set; }
        public string HolidayName { get; set; }
        public bool IsHoliday { get; set; }
        public int DayTypeId { get; set; }
        public string DayTypeName { get; set; }
        public string HolidayAndDayTypeName
        {
            get
            {
                if (!String.IsNullOrEmpty(HolidayName))
                    return $"{DayTypeName} ({HolidayName})";
                else
                    return $"{DayTypeName}";
            }
        }
        public int EmployeeScheduleId { get; set; }
        public int TimeScheduleTemplateHeadId { get; set; }
        public string TimeScheduleTemplateHeadName { get; set; }
        public int TimeScheduleTemplatePeriodId { get; set; }
        public int DayNumber { get; set; }
        public int RecalculateTimeRecordId { get; set; }
        public TermGroup_RecalculateTimeRecordStatus RecalculateTimeRecordStatus { get; set; }

        #endregion

        #region Schedule

        [TsIgnore]
        public List<AttestEmployeeDayShiftDTO> Shifts { get; set; }
        [TsIgnore]
        public List<AttestEmployeeDayShiftDTO> StandbyShifts { get; set; }
        public DateTime ScheduleStartTime { get; set; }
        public DateTime ScheduleStopTime { get; set; }
        [TsIgnore]
        public DateTime StandByStartTime { get; set; }
        [TsIgnore]
        public DateTime StandByStopTime { get; set; }
        public TimeSpan ScheduleTime { get; set; }
        public TimeSpan OccupiedTime { get; set; }
        [TsIgnore]
        public bool HasSchedule
        {
            get
            {
                return this.ScheduleStartTime < this.ScheduleStopTime; //Cannot check ScheduleTime beacuse it can be zero depending on TimeScheduleType factors or "not schedule time"
            }
        }
        public int TimeScheduleTypeFactorMinutes { get; set; }
        public TimeSpan ScheduleBreakTime { get; set; }
        public int NoOfScheduleBreaks { get; set; }
        public int ScheduleBreakMinutes
        {
            get
            {
                return (int)this.ScheduleBreakTime.TotalMinutes;
            }
            set
            {
                this.ScheduleBreakTime = new TimeSpan(0, value, 0);
            }
        }
        public bool IsScheduleZeroDay { get; set; }
        public DateTime ScheduleBreak1Start { get; set; }
        [TsIgnore]
        public DateTime ScheduleBreak1Stop
        {
            get
            {
                return this.ScheduleBreak1Start.AddMinutes(this.ScheduleBreak1Minutes);
            }
        }
        public int ScheduleBreak1Minutes { get; set; }
        public DateTime ScheduleBreak2Start { get; set; }
        [TsIgnore]
        public DateTime ScheduleBreak2Stop
        {
            get
            {
                return this.ScheduleBreak2Start.AddMinutes(this.ScheduleBreak2Minutes);
            }
        }
        public int ScheduleBreak2Minutes { get; set; }
        public DateTime ScheduleBreak3Start { get; set; }
        [TsIgnore]
        public DateTime ScheduleBreak3Stop
        {
            get
            {
                return this.ScheduleBreak3Start.AddMinutes(this.ScheduleBreak3Minutes);
            }
        }
        public int ScheduleBreak3Minutes { get; set; }
        public DateTime ScheduleBreak4Start { get; set; }
        [TsIgnore]
        public DateTime ScheduleBreak4Stop
        {
            get
            {
                return this.ScheduleBreak4Start.AddMinutes(this.ScheduleBreak4Minutes);
            }
        }
        public int ScheduleBreak4Minutes { get; set; }
        public bool IsNotScheduleTime { get; set; }
        public bool IsPrel { get; set; }
        public string IsPreliminary { get; set; }
        public List<TermGroup_TimeScheduleTemplateBlockShiftUserStatus> ShiftUserStatuses { get; set; }
        public List<GrossNetCostDTO> GrossNetCosts;

        #endregion

        #region Schedule-template

        [TsIgnore]
        [JsonIgnore]
        public bool IsTemplateSheduleLoaded { get; set; }
        public DateTime TemplateScheduleStartTime { get; set; }
        public DateTime TemplateScheduleStopTime { get; set; }
        public TimeSpan TemplateScheduleTime { get; set; }
        public TimeSpan TemplateScheduleBreakTime { get; set; }
        public int TemplateScheduleBreakMinutes
        {
            get
            {
                return (int)this.TemplateScheduleBreakTime.TotalMinutes;
            }
            set
            {
                this.TemplateScheduleBreakTime = new TimeSpan(0, value, 0);
            }
        }
        public DateTime TemplateScheduleBreak1Start { get; set; }
        public int TemplateScheduleBreak1Minutes { get; set; }
        public DateTime TemplateScheduleBreak2Start { get; set; }
        public int TemplateScheduleBreak2Minutes { get; set; }
        public DateTime TemplateScheduleBreak3Start { get; set; }
        public int TemplateScheduleBreak3Minutes { get; set; }
        public DateTime TemplateScheduleBreak4Start { get; set; }
        public int TemplateScheduleBreak4Minutes { get; set; }
        public bool IsTemplateScheduleZeroDay { get; set; }

        #endregion

        #region Stamping

        public List<AttestEmployeeDayTimeStampDTO> TimeStampEntrys { get; set; }
        public int TimeBlockDateStampingStatus { get; set; }
        public bool HasTimeStampEntries { get; set; }

        #endregion

        #region TimeBlocks

        public List<AttestEmployeeDayTimeBlockDTO> TimeBlocks { get; set; }
        public List<ProjectTimeBlockDTO> ProjectTimeBlocks { get; set; }
        public DateTime? PresenceStartTime { get; set; }
        public DateTime? PresenceStopTime { get; set; }
        public TimeSpan? PresenceTime { get; set; }
        public TimeSpan? AbsenceTime { get; set; }
        public TimeSpan? StandbyTime { get; set; }
        public TimeSpan? PresencePayedTime { get; set; }
        public TimeSpan? PresenceBreakTime { get; set; }
        public TimeSpan? PresenceInsideScheduleTime { get; set; }
        public TimeSpan? PresenceOutsideScheduleTime { get; set; }
        [TsIgnore]
        [JsonIgnore]
        public Dictionary<string, TimeSpan> AbsenceTimeDetails { get; set; }
        [TsIgnore]
        [JsonIgnore]
        public Dictionary<string, TimeSpan> PresenceInsideScheduleTimeDetails { get; set; }
        [TsIgnore]
        [JsonIgnore]
        public Dictionary<string, TimeSpan> PresenceOutsideScheduleTimeDetails { get; set; }
        [TsIgnore]
        [JsonIgnore]
        public Dictionary<string, TimeSpan> StandbyTimeDetails { get; set; }
        public int? PresenceBreakMinutes
        {
            get
            {
                return this.PresenceBreakTime.HasValue ? (int)this.PresenceBreakTime.Value.TotalMinutes : 0;
            }
            set
            {
                this.PresenceBreakTime = new TimeSpan(0, value ?? 0, 0);
            }
        }
        public List<AttestEmployeeBreakDTO> PresenceBreakItems { get; set; }
        public int TimeBlockDateStatus { get; set; }
        public bool ManuallyAdjusted { get; set; }
        public bool IsAbsenceDay { get; set; }
        public bool IsWholedayAbsence { get; set; }
        public int? WholedayAbsenseTimeDeviationCauseFromTimeBlock { get; set; }
        public bool HasDeviations { get; set; }
        public string TimeDeviationCauseNames { get; set; }
        public bool HasWorkedInsideSchedule { get; set; }
        public bool HasWorkedOutsideSchedule { get; set; }
        public bool HasAbsenceTime { get; set; }
        public bool HasOvertime { get; set; }
        public bool HasPeriodOvertime { get; set; }
        public bool HasTimeWorkReduction { get; set; }
        public bool HasPeriodTimeWorkReduction { get; set; }
        public bool HasUnhandledExtraShiftChanges { get; set; }
        public bool HasUnhandledShiftChanges { get; set; }
        public bool HasStandbyTime
        {
            get
            {
                return this.StandbyTime.HasValue && this.StandbyTime.Value.TotalMinutes > 0;
            }
        }

        [TsIgnore]
        [JsonIgnore]
        public int? FirstPresenceTimeBlockId { get; set; }
        [TsIgnore]
        [JsonIgnore]
        public int? FirstPresenceTimeBlockDeviationCauseId { get; set; }
        [TsIgnore]
        [JsonIgnore]
        public string FirstPresenceTimeBlockComment { get; set; }
        [TsIgnore]
        [JsonIgnore]
        public int? LastPresenceTimeBlockId { get; set; }
        [TsIgnore]
        [JsonIgnore]
        public int? LastPresenceTimeBlockDeviationCauseId { get; set; }
        [TsIgnore]
        [JsonIgnore]
        public string LastPresenceTimeBlockComment { get; set; }

        #endregion

        #region Transactions

        public List<AttestPayrollTransactionDTO> AttestPayrollTransactions { get; set; }
        public List<AttestEmployeeDayTimeCodeTransactionDTO> TimeCodeTransactions { get; set; }
        public List<AttestEmployeeDayTimeInvoiceTransactionDTO> TimeInvoiceTransactions { get; set; }

        public bool HasPayrollTransactions { get; set; }
        public bool HasPayrollScheduleTransactions { get; set; }
        public bool HasTransactions
        {
            get { return this.HasPayrollTransactions || this.HasPayrollScheduleTransactions; }
        }
        public bool HasComment { get; set; }
        public bool IsGeneratingTransactions { get; set; }

        #endregion

        #region Expense

        [TsIgnore]
        public List<AttestExpenseSumDTO> Expenses { get; set; }
        public bool HasExpense
        {
            get
            {
                return this.Expenses.HasAny();
            }
        }
        public int SumExpenseRows
        {
            get
            {
                return this.Expenses.SumRows();
            }
        }
        public decimal SumExpenseAmount
        {
            get
            {
                return this.Expenses.SumAmount();
            }
        }

        #endregion

        #region Sums

        //Time
        public TimeSpan SumTimeAccumulator { get; set; }
        public TimeSpan SumTimeAccumulatorOverTime { get; set; }
        public TimeSpan SumTimeWorkedScheduledTime { get; set; }
        public TimeSpan SumInvoicedTime { get; set; }
        //Absence
        public string SumGrossSalaryAbsenceText { get; set; }
        public TimeSpan SumGrossSalaryAbsence { get; set; }
        public TimeSpan SumGrossSalaryAbsenceVacation { get; set; }
        public TimeSpan SumGrossSalaryAbsenceSick { get; set; }
        public TimeSpan SumGrossSalaryAbsenceLeaveOfAbsence { get; set; }
        public TimeSpan SumGrossSalaryAbsenceParentalLeave { get; set; }
        public TimeSpan SumGrossSalaryAbsenceTemporaryParentalLeave { get; set; }
        //Weekend salary
        public TimeSpan SumGrossSalaryWeekendSalary { get; set; }
        //Duty salary
        public TimeSpan SumGrossSalaryDuty { get; set; }
        //Additional time
        public TimeSpan SumGrossSalaryAdditionalTime { get; set; }
        public TimeSpan SumGrossSalaryAdditionalTime35 { get; set; }
        public TimeSpan SumGrossSalaryAdditionalTime70 { get; set; }
        public TimeSpan SumGrossSalaryAdditionalTime100 { get; set; }
        //Over time
        public TimeSpan SumGrossSalaryOvertime { get; set; }
        public TimeSpan SumGrossSalaryOvertime35 { get; set; }
        public TimeSpan SumGrossSalaryOvertime50 { get; set; }
        public TimeSpan SumGrossSalaryOvertime70 { get; set; }
        public TimeSpan SumGrossSalaryOvertime100 { get; set; }
        //OB addition
        public TimeSpan SumGrossSalaryOBAddition { get; set; }
        public TimeSpan SumGrossSalaryOBAddition40 { get; set; }
        public TimeSpan SumGrossSalaryOBAddition50 { get; set; }
        public TimeSpan SumGrossSalaryOBAddition57 { get; set; }
        public TimeSpan SumGrossSalaryOBAddition70 { get; set; }
        public TimeSpan SumGrossSalaryOBAddition79 { get; set; }
        public TimeSpan SumGrossSalaryOBAddition100 { get; set; }
        public TimeSpan SumGrossSalaryOBAddition113 { get; set; }

        #endregion

        #region PayrollProduct

        public decimal PayrollWorkMinutes { get; set; }
        [TsIgnore]
        public Dictionary<string, decimal> PayrollAbsenceMinutes { get; set; }
        public decimal PayrollOverTimeMinutes { get; set; }
        public decimal PayrollAddedTimeMinutes { get; set; }
        public decimal PayrollInconvinientWorkingHoursMinutes { get; set; }
        public decimal PayrollInconvinientWorkingHoursScaledMinutes { get; set; }

        #endregion

        #region Attest

        public List<AttestStateDTO> AttestStates { get; set; }
        public int AttestStateId { get; set; }
        public int AttestStateSort { get; set; }
        public string AttestStateColor { get; set; }
        public string AttestStateName { get; set; }
        public bool HasSameAttestState
        {
            get
            {
                return this.AttestStates == null || this.AttestStates.Count <= 1;
            }
        }
        public bool HasScheduledPlacement { get; set; }
        public bool HasNoneInitialTransactions { get; set; }
        public bool IsAttested { get; set; }
        [TsIgnore]
        public bool IsToBeAttested { get; set; }

        #endregion

        #region PayrollImportTransactions

        public List<PayrollImportEmployeeTransactionDTO> PayrollImportEmployeeTransactions { get; set; }
        public bool HasPayrollImportEmployeeTransactions { get; set; }

        #endregion

        #region UnhandledShiftChanges

        public TimeUnhandledShiftChangesEmployeeDTO UnhandledEmployee { get; set; }

        #endregion

        #region Informations

        public bool HasInformations
        {
            get
            {
                return !this.Informations.IsNullOrEmpty();
            }
        }
        public List<SoeTimeAttestInformation> Informations
        {
            get
            {
                var informations = new List<SoeTimeAttestInformation>();
                if (this.HasShiftSwaps)
                    informations.Add(SoeTimeAttestInformation.HasShiftSwaps);
                return informations;
            }
        }

        public bool HasShiftSwaps { get; set; }

        #endregion

        #region Warnings

        public bool HasWarnings
        {
            get
            {
                return !this.Warnings.IsNullOrEmpty();
            }
        }
        public List<SoeTimeAttestWarning> Warnings
        {
            get
            {
                var warnings = new List<SoeTimeAttestWarning>();
                if (this.IsScheduleChangedFromTemplate)
                    warnings.Add(SoeTimeAttestWarning.ScheduleIsChangedFromTemplate);
                if (this.HasScheduleWithoutTransactions)
                    warnings.Add(SoeTimeAttestWarning.ScheduleWithoutTransactions);
                if (this.HasPeriodTimeScheduleTypeFactorMinutes)
                    warnings.Add(SoeTimeAttestWarning.TimeScheduleTypeFactorMinutes);
                if (this.HasPeriodDiscardedBreakEvaluation)
                    warnings.Add(SoeTimeAttestWarning.DiscardedBreakEvaluation);
                if (this.HasTimeStampsWithoutTransactions)
                    warnings.Add(SoeTimeAttestWarning.TimeStampsWithoutTransactions);
                if (this.HasInvalidTimeStamps)
                    warnings.Add(SoeTimeAttestWarning.TimeStampErrors);
                if (this.ContainsDuplicateTimeBlocks)
                    warnings.Add(SoeTimeAttestWarning.ContainsDuplicateTimeBlocks);
                if (this.HasPayrollImportWarnings)
                    warnings.Add(SoeTimeAttestWarning.PayrollImport);
                return warnings;
            }
        }

        public bool IsScheduleChangedFromTemplate
        {
            get
            {
                return
                    !this.IsAttested &&
                    this.IsTemplateSheduleLoaded &&
                    (
                        this.ScheduleTime.TotalSeconds != this.TemplateScheduleTime.TotalSeconds ||
                        this.ScheduleStartTime != this.TemplateScheduleStartTime ||
                        this.ScheduleStopTime != this.TemplateScheduleStopTime ||
                        this.ScheduleBreakMinutes != this.TemplateScheduleBreakMinutes
                    );
            }
        }
        public bool HasScheduleWithoutTransactions { get; set; }
        public bool HasPeriodDiscardedBreakEvaluation
        {
            get
            {
                return !this.IsAttested && this.DiscardedBreakEvaluation && !this.AlwaysDiscardBreakEvaluation;
            }
        }
        public bool HasPeriodTimeScheduleTypeFactorMinutes
        {
            get
            {
                return this.TimeScheduleTypeFactorMinutes > 0;
            }
        }
        public bool DiscardedBreakEvaluation { get; set; }
        public bool AlwaysDiscardBreakEvaluation { get; set; }
        public bool HasTimeStampsWithoutTransactions { get; set; }
        public bool HasInvalidTimeStamps
        {
            get
            {
                return
                    this.TimeBlockDateStampingStatus != (int)TermGroup_TimeBlockDateStampingStatus.NoStamps &&
                    this.TimeBlockDateStampingStatus != (int)TermGroup_TimeBlockDateStampingStatus.Complete;
            }

        }
        public bool HasPayrollImportWarnings { get; set; }
        public bool ContainsDuplicateTimeBlocks { get; set; }
        public bool IsPartlyAdditional { get; set; }
        public bool IsCompletelyAdditional { get; set; }

        #endregion

        #endregion

        #region Ctor

        public AttestEmployeeDayDTO(int employeeId, DateTime date, EmployeeGroupDTO employeeGroup = null)
        {
            this.UniqueId = Guid.NewGuid().ToString();
            this.EmployeeId = employeeId;
            this.Date = date;

            this.EmployeeGroupId = employeeGroup?.EmployeeGroupId ?? 0;
            this.AutogenTimeblocks = employeeGroup?.AutogenTimeblocks ?? false;
            this.AlwaysDiscardBreakEvaluation = employeeGroup?.AlwaysDiscardBreakEvaluation ?? false;
            this.TimeReportType = employeeGroup?.TimeReportType ?? 0;

            this.SetEmptyValues();
        }

        #endregion

        #region Public methods

        public void SetEmptyValues()
        {
            this.ScheduleTime = new TimeSpan(); //Not needed
            this.ScheduleStartTime = CalendarUtility.DATETIME_DEFAULT;
            this.ScheduleStopTime = CalendarUtility.DATETIME_DEFAULT;
            this.ScheduleBreak1Start = CalendarUtility.DATETIME_DEFAULT;
            this.ScheduleBreak1Minutes = 0;
            this.ScheduleBreak2Start = CalendarUtility.DATETIME_DEFAULT;
            this.ScheduleBreak2Minutes = 0;
            this.ScheduleBreak3Start = CalendarUtility.DATETIME_DEFAULT;
            this.ScheduleBreak3Minutes = 0;
            this.ScheduleBreak4Start = CalendarUtility.DATETIME_DEFAULT;
            this.ScheduleBreak4Minutes = 0;
            this.TemplateScheduleStartTime = CalendarUtility.DATETIME_DEFAULT;
            this.TemplateScheduleStopTime = CalendarUtility.DATETIME_DEFAULT;
            this.TemplateScheduleTime = new TimeSpan();
            this.TemplateScheduleBreakMinutes = 0;
            this.TemplateScheduleBreak1Start = CalendarUtility.DATETIME_DEFAULT;
            this.TemplateScheduleBreak1Minutes = 0;
            this.TemplateScheduleBreak2Start = CalendarUtility.DATETIME_DEFAULT;
            this.TemplateScheduleBreak2Minutes = 0;
            this.TemplateScheduleBreak3Start = CalendarUtility.DATETIME_DEFAULT;
            this.TemplateScheduleBreak3Minutes = 0;
            this.TemplateScheduleBreak4Start = CalendarUtility.DATETIME_DEFAULT;
            this.TemplateScheduleBreak4Minutes = 0;
            this.PresenceStartTime = null;
            this.PresenceStopTime = null;
            this.PresenceTime = null;
            this.PresencePayedTime = null;
            this.AbsenceTime = null;
            this.PresenceBreakMinutes = null;
            this.PresenceBreakItems = null;
            this.ManuallyAdjusted = false;
            this.DiscardedBreakEvaluation = false;
            this.TimeScheduleTypeFactorMinutes = 0;
            this.RecalculateTimeRecordId = 0;
            this.RecalculateTimeRecordStatus = TermGroup_RecalculateTimeRecordStatus.None;
            this.TimeBlockDateStatus = (int)SoeTimeBlockDateStatus.None;
            this.TimeBlockDateStampingStatus = (int)TermGroup_TimeBlockDateStampingStatus.NoStamps;
            this.IsPrel = false;
            this.IsScheduleZeroDay = false;
            this.IsNotScheduleTime = false;
            this.IsTemplateScheduleZeroDay = false;
            this.IsAbsenceDay = false;
            this.IsWholedayAbsence = false;
            this.HasTimeStampEntries = false;

            this.Shifts = new List<AttestEmployeeDayShiftDTO>();
            this.StandbyShifts = new List<AttestEmployeeDayShiftDTO>();
            this.ShiftUserStatuses = new List<TermGroup_TimeScheduleTemplateBlockShiftUserStatus>();
            this.GrossNetCosts = new List<GrossNetCostDTO>();
            this.PresenceBreakItems = new List<AttestEmployeeBreakDTO>();
            this.PayrollAbsenceMinutes = new Dictionary<string, decimal>();
            this.AttestPayrollTransactions = new List<AttestPayrollTransactionDTO>();
            this.AttestStates = new List<AttestStateDTO>();
            this.ProjectTimeBlocks = new List<ProjectTimeBlockDTO>();
        }

        public void SetProjectTimeBlocks(IEnumerable<ProjectTimeBlockDTO> projectTimeBlocks)
        {
            if (projectTimeBlocks.IsNullOrEmpty() || this.TimeCodeTransactions.IsNullOrEmpty())
                return;

            foreach (var projectTimeBlock in projectTimeBlocks.Where(ptb => ptb.AdditionalTime))
            {
                foreach (var timeCodeTransaction in this.TimeCodeTransactions.Where(t => t.ProjectTimeBlockId == projectTimeBlock.ProjectTimeBlockId))
                {
                    timeCodeTransaction.StartTime = CalendarUtility.DATETIME_0VALUE;
                    timeCodeTransaction.StopTime = CalendarUtility.DATETIME_0VALUE;

                    foreach (var payrollTransaction in this.AttestPayrollTransactions.Where(t => t.TimeCodeTransactionId == timeCodeTransaction.TimeCodeTransactionId))
                    {
                        payrollTransaction.StartTime = CalendarUtility.DATETIME_0VALUE;
                        payrollTransaction.StopTime = CalendarUtility.DATETIME_0VALUE;
                    }
                }
            }

            this.ProjectTimeBlocks = projectTimeBlocks
                .Where(ptb => ptb.Date == this.Date && !ptb.AdditionalTime)
                .OrderBy(ptb => ptb.StartTime)
                .ThenBy(ptb => ptb.StopTime)
                .ToList();
        }

        public bool MatchTransactions(AttestPayrollTransactionDTO attestPayrollTransaction)
        {
            bool foundMatch = false;

            //Check for matches (same PayrollProduct and AccountInternal combination)
            foreach (var existingAttestPayrollTransaction in this.AttestPayrollTransactions)
            {
                if (existingAttestPayrollTransaction.Match(attestPayrollTransaction))
                {
                    //Update
                    existingAttestPayrollTransaction.Update(attestPayrollTransaction);
                    foundMatch = true;
                    break;
                }
            }

            return foundMatch;
        }

        public bool CalculateHasScheduleWithoutTransactions(bool untilNow)
        {
            return this.HasSchedule && !this.HasTransactions && (!untilNow || CalendarUtility.IsBeforeNow(this.Date, this.ScheduleStopTime));
        }

        public void CalculateHasDeviations()
        {
            if (this.ScheduleStartTime != this.PresenceStartTime)
                this.HasDeviations = true;
            else if (this.ScheduleStopTime != this.PresenceStopTime)
                this.HasDeviations = true;
            else if (this.PresenceBreakItems.Count != this.NoOfScheduleBreaks)
                this.HasDeviations = true;
            else if (this.ScheduleBreakTime != this.PresenceBreakTime)
                this.HasDeviations = true;
            else if (this.PresenceBreakItems.Count >= 1 && this.PresenceBreakItems[0].StartTime != this.ScheduleBreak1Start)
                this.HasDeviations = true;
            else if (this.PresenceBreakItems.Count >= 2 && this.PresenceBreakItems[1].StartTime != this.ScheduleBreak2Start)
                this.HasDeviations = true;
            else if (this.PresenceBreakItems.Count >= 3 && this.PresenceBreakItems[2].StartTime != this.ScheduleBreak3Start)
                this.HasDeviations = true;
            else if (this.PresenceBreakItems.Count >= 4 && this.PresenceBreakItems[3].StartTime != this.ScheduleBreak4Start)
                this.HasDeviations = true;
        }

        public void CalculateTransactionAggregations(AttestPayrollTransactionDTO transactionItem)
        {
            if (!transactionItem.RetroactivePayrollOutcomeId.HasValue)
            {
                AddinutesFromPayrollType(transactionItem);
                AddPresencePayedTime(transactionItem);
            }
        }

        public void AddShiftUserStatus(TermGroup_TimeScheduleTemplateBlockShiftUserStatus shiftUserStatus)
        {
            if (shiftUserStatus != TermGroup_TimeScheduleTemplateBlockShiftUserStatus.None && !this.ShiftUserStatuses.Contains(shiftUserStatus))
                this.ShiftUserStatuses.Add(shiftUserStatus);
        }

        public void AddinutesFromPayrollType(AttestPayrollTransactionDTO transactionItem)
        {
            if (!transactionItem.RetroactivePayrollOutcomeId.HasValue)
            {
                if (transactionItem.IsWork())
                {
                    this.PayrollWorkMinutes += transactionItem.Quantity;
                }
                else if (transactionItem.IsAbsence())
                {
                    if (this.PayrollAbsenceMinutes.ContainsKey(transactionItem.PayrollProductNumber))
                        this.PayrollAbsenceMinutes[transactionItem.PayrollProductNumber] += transactionItem.Quantity;
                    else
                        this.PayrollAbsenceMinutes.Add(transactionItem.PayrollProductNumber, transactionItem.Quantity);
                }
                else if (transactionItem.IsOvertimeCompensation())
                {
                    this.PayrollOverTimeMinutes += transactionItem.Quantity;
                }
                else if (transactionItem.IsAddedTime())
                {
                    this.PayrollAddedTimeMinutes += transactionItem.Quantity;
                }
                else if (transactionItem.IsOBAddition() && !transactionItem.IsScheduleTransaction)
                {
                    this.PayrollInconvinientWorkingHoursMinutes += transactionItem.Quantity;
                    this.PayrollInconvinientWorkingHoursScaledMinutes += (transactionItem.Quantity * transactionItem.PayrollProductFactor);
                }
            }
        }

        public void AddPresencePayedTime(AttestPayrollTransactionDTO transactionItem)
        {
            if (!transactionItem.RetroactivePayrollOutcomeId.HasValue && transactionItem.PayrollProductPayed && !transactionItem.IsScheduleTransaction)
            {
                TimeSpan presencePayedTime = new TimeSpan(0, (int)transactionItem.Quantity, 0);
                if (this.PresencePayedTime.HasValue)
                    this.PresencePayedTime = this.PresencePayedTime.Value.Add(presencePayedTime);
                else
                    this.PresencePayedTime = presencePayedTime;
            }
        }

        public void AddExpense(int? timeCodeTransactionId, decimal? amount, string defaultName = "")
        {
            if (!timeCodeTransactionId.HasValue)
                return;

            if (this.Expenses == null)
                this.Expenses = new List<AttestExpenseSumDTO>();
            string name = this.TimeCodeTransactions?.FirstOrDefault(i => i.TimeCodeTransactionId == timeCodeTransactionId)?.TimeCodeName ?? defaultName;
            this.Expenses.Upsert(timeCodeTransactionId.Value, name, amount);
        }

        public bool HasAnyTransaction(DateTime startTime, DateTime stopTime)
        {
            return this.AttestPayrollTransactions?.Any(transaction => transaction.StartTime >= startTime && transaction.StartTime <= stopTime) ?? false;
        }

        #endregion
    }

    public class AttestEmployeeOverviewDTO
    {
        #region Variables

        public int EmployeeId { get; set; }
        public TimeSpan PresenceInsideScheduleTime { get; set; }
        public TimeSpan PresenceOutsideScheduleTime { get; set; }
        public TimeSpan AbsenceTime { get; set; }
        public TimeSpan StandbyTime { get; set; }
        public Dictionary<string, TimeSpan> PresenceInsideScheduleTimeDetails { get; set; }
        public Dictionary<string, TimeSpan> PresenceOutsideScheduleTimeDetails { get; set; }
        public Dictionary<string, TimeSpan> AbsenceTimeDetails { get; set; }
        public Dictionary<string, TimeSpan> StandbyTimeDetails { get; set; }
        public Dictionary<string, string> ExpenseDetails { get; set; }
        public int SumExpeseRows { get; set; }
        public decimal SumExpenseAmount { get; set; }
        public List<AttestEmployeeOverviewMessage> Messages { get; set; }
        public List<SoeTimeAttestWarning> Warnings { get; set; }
        public bool HasWarnings { get; set; }
        public bool IsPeriodAttested { get; set; }
        public int NrOfDaysWithResultingAttest { get; set; }
        public int NrOfDaysWithInitialAttest { get; set; }

        #endregion

        #region Ctor

        public AttestEmployeeOverviewDTO()
        {
            this.PresenceInsideScheduleTime = new TimeSpan();
            this.PresenceOutsideScheduleTime = new TimeSpan();
            this.AbsenceTime = new TimeSpan();
            this.StandbyTime = new TimeSpan();
            this.Warnings = new List<SoeTimeAttestWarning>();
            this.Messages = new List<AttestEmployeeOverviewMessage>();
        }

        #endregion
    }

    public class AttestEmployeeOverviewMessage
    {
        #region Variables

        public string Message { get; set; }
        public string Color { get; set; }
        public bool IsWarning { get; set; }

        #endregion

        #region Ctor

        public AttestEmployeeOverviewMessage(string message, bool isWarning)
        {
            this.Message = message;
            this.Color = string.Empty;
            this.IsWarning = isWarning;
        }

        public AttestEmployeeOverviewMessage(string message, string color)
        {
            this.Message = message;
            this.Color = color;
            this.IsWarning = false;
        }

        #endregion
    }

    public class AttestEmployeeDaySmallDTO
    {
        #region Variables

        public int EmployeeId { get; set; }
        public int TimeBlockDateId { get; set; }
        public DateTime Date { get; set; }
        public int? TimeScheduleTemplatePeriodId { get; set; }
        [TsIgnore]
        public bool IsDayForward { get; set; }

        #endregion

        public AttestEmployeeDaySmallDTO()
        {

        }

        public AttestEmployeeDaySmallDTO(int employeeId, DateTime date, int timeBlockDateId, int? timeScheduleTemplatePeriodId = null, bool isDayForward = false)
        {
            this.EmployeeId = employeeId;
            this.Date = date;
            this.TimeBlockDateId = timeBlockDateId;
            this.TimeScheduleTemplatePeriodId = timeScheduleTemplatePeriodId.ToNullable();
            this.IsDayForward = isDayForward;
        }
    }

    public class AttestEmployeesDaySmallDTO
    {
        #region Variables

        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public int EmployeeId { get; set; }

        #endregion
    }

    public class AttestEmployeeDayShiftDTO
    {
        #region Variables

        public int EmployeeId { get; set; }
        public int TimeScheduleTemplateBlockId { get; set; }
        public int ShiftTypeId { get; set; }
        public string ShiftTypeName { get; set; }
        public string ShiftTypeColor { get; set; }
        public string ShiftTypeDescription { get; set; }
        public int TimeScheduleTypeId { get; set; }
        public string TimeScheduleTypeName { get; set; }
        public int? TimeDeviationCauseId { get; set; }
        public string TimeDeviationCauseName { get; set; }
        public int? AccountId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime StopTime { get; set; }
        public string Link { get; set; }
        public TermGroup_TimeScheduleTemplateBlockType Type { get; set; }

        public int Break1Id { get; set; }
        public int Break1TimeCodeId { get; set; }
        public string Break1TimeCode { get; set; }
        public DateTime Break1StartTime { get; set; }
        public int Break1Minutes { get; set; }
        public int Break2Id { get; set; }
        public int Break2TimeCodeId { get; set; }
        public string Break2TimeCode { get; set; }
        public DateTime Break2StartTime { get; set; }
        public int Break2Minutes { get; set; }
        public int Break3Id { get; set; }
        public int Break3TimeCodeId { get; set; }
        public string Break3TimeCode { get; set; }
        public DateTime Break3StartTime { get; set; }
        public int Break3Minutes { get; set; }
        public int Break4Id { get; set; }
        public int Break4TimeCodeId { get; set; }
        public string Break4TimeCode { get; set; }
        public DateTime Break4StartTime { get; set; }
        public int Break4Minutes { get; set; }
        public string Description { get; set; }

        #endregion


    }

    public class AttestEmployeeDayTimeStampDTO
    {
        #region Variables

        public int TimeStampEntryId { get; set; }
        public int? TimeTerminalId { get; set; }
        public int EmployeeId { get; set; }
        public int? AccountId { get; set; }
        public int? AccountId2 { get; set; } // Extended on new timestampentry..
        public int? TimeTerminalAccountId { get; set; }
        public int? TimeScheduleTemplatePeriodId { get; set; }
        public int? TimeBlockDateId { get; set; }
        public int? TimeDeviationCauseId { get; set; }
        public int? EmployeeChildId { get; set; }
        public int? ShiftTypeId { get; set; }
        public int? TimeScheduleTypeId { get; set; }
        public string TimeScheduleTypeName { get; set; }
        public TimeStampEntryType Type { get; set; }
        public TermGroup_TimeStampEntryStatus Status { get; set; }
        public TermGroup_TimeStampEntryOriginType OriginType { get; set; }
        public DateTime Time { get; set; }
        public string Note { get; set; }
        public bool IsBreak { get; set; }
        public bool IsPaidBreak { get; set; }
        public bool IsDistanceWork { get; set; }
        public bool ManuallyAdjusted { get; set; }
        public bool EmployeeManuallyAdjusted { get; set; }
        public bool AutoStampOut { get; set; }

        // Relations
        public List<TimeStampEntryExtendedDTO> Extended { get; set; }

        #endregion
    }

    public class AttestEmployeeDayTimeBlockDTO
    {
        #region Variables

        public string GuidId { get; set; }

        public int TimeBlockId { get; set; }
        public int EmployeeId { get; set; }
        public int TimeBlockDateId { get; set; }
        public int? TimeScheduleTemplatePeriodId { get; set; }
        public int? TimeScheduleTemplateBlockBreakId { get; set; }
        public int? TimeDeviationCauseStartId { get; set; }
        public int? TimeDeviationCauseStopId { get; set; }
        public int? AccountId { get; set; }
        public int? EmployeeChildId { get; set; }
        public int? ShiftTypeId { get; set; }
        public int? TimeScheduleTypeId { get; set; }

        public List<AccountInternalDTO> DeviationAccounts { get; set; }
        public List<TimeCodeDTO> TimeCodes { get; set; }

        public DateTime StopTime { get; set; }
        public DateTime StartTime { get; set; }
        public bool IsBreak { get; set; }
        public bool IsGeneratedFromBreak { get; set; }
        public bool IsPreliminary { get; set; }
        public bool ManuallyAdjusted { get; set; }
        public string TimeDeviationCauseName { get; set; }
        public string Comment { get; set; }
        public bool IsAbsence { get; set; }
        public bool IsPresence { get; set; }
        public bool IsOvertime { get; set; }
        public bool IsOutsideScheduleNotOvertime { get; set; }
        public bool IsReadonlyLeft { get; set; }
        public bool IsReadonlyRight { get; set; }

        #endregion

        #region Ctor

        public AttestEmployeeDayTimeBlockDTO()
        {
            this.GuidId = Guid.NewGuid().ToString();
        }

        #endregion
    }

    public class AttestEmployeePeriodDTO : IAttestStateLowest
    {
        #region Variables

        public string UniqueId { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeNr { get; set; }
        public string EmployeeName { get; set; }
        public string EmployeeNrAndName { get; set; }
        public TermGroup_Sex EmployeeSex { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime StopDate { get; set; }

        #region Schedule

        public TimeSpan ScheduleTime { get; set; }
        public string ScheduleTimeInfo
        {
            get
            {
                return this.ScheduleTime.TotalMinutes > 0 ? CalendarUtility.FormatTimeSpan(this.ScheduleTime, false, false, false) : string.Empty;
            }
        }
        public TimeSpan ScheduleBreakTime { get; set; }
        public string ScheduleBreakTimeInfo
        {
            get
            {
                return this.ScheduleBreakTime.TotalMinutes > 0 ? CalendarUtility.FormatTimeSpan(this.ScheduleBreakTime, false, false, false) : string.Empty;
            }
        }
        public int ScheduleBreakMinutes
        {
            get
            {
                return (int)this.ScheduleBreakTime.TotalMinutes;
            }
            set
            {
                this.ScheduleBreakTime = new TimeSpan(0, value, 0);
            }
        }
        public int ScheduleDays { get; set; }
        public TimeSpan StandbyTime { get; set; }
        public bool HasStandbyTime
        {
            get
            {
                return this.StandbyTime.TotalMinutes > 0;
            }
        }
        [TsIgnore]
        public List<int> AdditionalOnAccountIds { get; set; } //Only used for Mobile, otherwise AdditionalOnAccountIds is set in Tree

        #endregion

        #region TimeBlocks

        public int PresenceDays { get; set; }
        public bool HasWorkedInsideSchedule { get; set; }
        public bool HasWorkedOutsideSchedule { get; set; }
        public TimeSpan PresenceTime { get; set; }
        public string PresenceTimeInfo
        {
            get
            {
                return this.PresenceTime.TotalMinutes > 0 ? CalendarUtility.FormatTimeSpan(this.PresenceTime, false, false, false) : string.Empty;
            }
        }
        public TimeSpan PresencePayedTime { get; set; }
        public string PresencePayedTimeInfo
        {
            get
            {
                return this.PresencePayedTime.TotalMinutes > 0 ? CalendarUtility.FormatTimeSpan(this.PresencePayedTime, false, false, false) : string.Empty;
            }
        }
        public TimeSpan PresenceBreakTime { get; set; }
        public string PresenceBreakTimeInfo
        {
            get
            {
                return this.PresenceBreakTime.TotalMinutes > 0 ? CalendarUtility.FormatTimeSpan(this.PresenceBreakTime, false, false, false) : string.Empty;
            }
        }
        public int? PresenceBreakMinutes
        {
            get
            {
                return (int)this.PresenceBreakTime.TotalMinutes;
            }
        }
        public TimeSpan AbsenceTime { get; set; }
        public bool HasAbsenceTime
        {
            get
            {
                return this.AbsenceTime.TotalMinutes > 0;
            }
        }
        public bool HasOvertime
        {
            get
            {
                return !this.OvertimeDates.IsNullOrEmpty();
            }
        }
        [TsIgnore]
        public List<DateTime> AbsenceSickDates { get; set; } = new List<DateTime>();
        [TsIgnore]
        public List<DateTime> OvertimeDates { get; set; } = new List<DateTime>();

        #endregion

        #region Expense

        [TsIgnore]
        public List<AttestExpenseSumDTO> Expenses { get; set; }
        public bool HasExpense
        {
            get
            {
                return this.Expenses.HasAny();
            }
        }
        public int SumExpenseRows
        {
            get
            {
                return this.Expenses.SumRows();
            }
        }
        public decimal SumExpenseAmount
        {
            get
            {
                return this.Expenses.SumAmount();
            }
        }

        #endregion

        #region TimePayrollTransaction

        public bool HasTransactions { get; set; }

        //Time
        public TimeSpan SumTimeAccumulator { get; set; }
        public TimeSpan SumTimeAccumulatorOverTime { get; set; }
        public TimeSpan SumTimeWorkedScheduledTime { get; set; }
        public TimeSpan SumInvoicedTime { get; set; }
        public string SumTimeAccumulatorText { get; set; }
        public string SumTimeAccumulatorOverTimeText { get; set; }
        public string SumTimeWorkedScheduledTimeText { get; set; }
        public string SumInvoicedTimeText { get; set; }
        //Absence
        public TimeSpan SumGrossSalaryAbsence { get; set; }
        public TimeSpan SumGrossSalaryAbsenceVacation { get; set; }
        public TimeSpan SumGrossSalaryAbsenceSick { get; set; }
        public TimeSpan SumGrossSalaryAbsenceLeaveOfAbsence { get; set; }
        public TimeSpan SumGrossSalaryAbsenceParentalLeave { get; set; }
        public TimeSpan SumGrossSalaryAbsenceTemporaryParentalLeave { get; set; }
        public string SumGrossSalaryAbsenceText { get; set; }
        public string SumGrossSalaryAbsenceVacationText { get; set; }
        public string SumGrossSalaryAbsenceSickText { get; set; }
        public string SumGrossSalaryAbsenceLeaveOfAbsenceText { get; set; }
        public string SumGrossSalaryAbsenceParentalLeaveText { get; set; }
        public string SumGrossSalaryAbsenceTemporaryParentalLeaveText { get; set; }
        //Weekend salary
        public TimeSpan SumGrossSalaryWeekendSalary { get; set; }
        public string SumGrossSalaryWeekendSalaryText { get; set; }
        //Duty salary
        public TimeSpan SumGrossSalaryDuty { get; set; }
        public string SumGrossSalaryDutyText { get; set; }
        //Additional time
        public TimeSpan SumGrossSalaryAdditionalTime { get; set; }
        public TimeSpan SumGrossSalaryAdditionalTime35 { get; set; }
        public TimeSpan SumGrossSalaryAdditionalTime70 { get; set; }
        public TimeSpan SumGrossSalaryAdditionalTime100 { get; set; }
        public string SumGrossSalaryAdditionalTimeText { get; set; }
        public string SumGrossSalaryAdditionalTime35Text { get; set; }
        public string SumGrossSalaryAdditionalTime70Text { get; set; }
        public string SumGrossSalaryAdditionalTime100Text { get; set; }
        //Overtime
        public TimeSpan SumGrossSalaryOvertime { get; set; }
        public TimeSpan SumGrossSalaryOvertime35 { get; set; }
        public TimeSpan SumGrossSalaryOvertime50 { get; set; }
        public TimeSpan SumGrossSalaryOvertime70 { get; set; }
        public TimeSpan SumGrossSalaryOvertime100 { get; set; }
        public string SumGrossSalaryOvertimeText { get; set; }
        public string SumGrossSalaryOvertime35Text { get; set; }
        public string SumGrossSalaryOvertime50Text { get; set; }
        public string SumGrossSalaryOvertime70Text { get; set; }
        public string SumGrossSalaryOvertime100Text { get; set; }
        //OB addition
        public TimeSpan SumGrossSalaryOBAddition { get; set; }
        public TimeSpan SumGrossSalaryOBAddition40 { get; set; }
        public TimeSpan SumGrossSalaryOBAddition50 { get; set; }
        public TimeSpan SumGrossSalaryOBAddition57 { get; set; }
        public TimeSpan SumGrossSalaryOBAddition70 { get; set; }
        public TimeSpan SumGrossSalaryOBAddition79 { get; set; }
        public TimeSpan SumGrossSalaryOBAddition100 { get; set; }
        public TimeSpan SumGrossSalaryOBAddition113 { get; set; }
        public string SumGrossSalaryOBAdditionText { get; set; }
        public string SumGrossSalaryOBAddition40Text { get; set; }
        public string SumGrossSalaryOBAddition50Text { get; set; }
        public string SumGrossSalaryOBAddition57Text { get; set; }
        public string SumGrossSalaryOBAddition70Text { get; set; }
        public string SumGrossSalaryOBAddition79Text { get; set; }
        public string SumGrossSalaryOBAddition100Text { get; set; }
        public string SumGrossSalaryOBAddition113Text { get; set; }

        #endregion

        #region Attest

        public List<AttestStateDTO> AttestStates { get; set; }
        public int AttestStateId { get; set; }
        public int AttestStateSort { get; set; }
        public string AttestStateColor { get; set; }
        public string AttestStateName { get; set; }
        public bool HasSameAttestState
        {
            get { return this.AttestStates == null || this.AttestStates.Count <= 1; }
        }
        [TsIgnore]
        public bool IsAttested { get; set; }
        [TsIgnore]
        public bool IsToBeAttested { get; set; }

        #endregion

        #region UnhandledShiftChanges

        public TimeUnhandledShiftChangesEmployeeDTO UnhandledEmployee { get; set; }

        #endregion

        #region Informations

        public bool HasInformations
        {
            get
            {
                return !this.Informations.IsNullOrEmpty();
            }
        }
        public List<SoeTimeAttestInformation> Informations
        {
            get
            {
                var informations = new List<SoeTimeAttestInformation>();
                if (this.HasShiftSwaps)
                    informations.Add(SoeTimeAttestInformation.HasShiftSwaps);
                return informations;
            }
        }

        public bool HasShiftSwaps { get; set; }

        #endregion

        #region Warnings

        public bool HasWarnings
        {
            get
            {
                return !this.Warnings.IsNullOrEmpty();
            }
        }
        public List<SoeTimeAttestWarning> Warnings
        {
            get
            {
                List<SoeTimeAttestWarning> warnings = new List<SoeTimeAttestWarning>();
                if (this.HasScheduleWithoutTransactions)
                    warnings.Add(SoeTimeAttestWarning.ScheduleWithoutTransactions);
                if (this.HasTimeStampsWithoutTransactions)
                    warnings.Add(SoeTimeAttestWarning.TimeStampsWithoutTransactions);
                if (this.HasInvalidTimeStamps)
                    warnings.Add(SoeTimeAttestWarning.TimeStampErrors);
                if (this.HasPayrollImports)
                    warnings.Add(SoeTimeAttestWarning.PayrollImport);
                return warnings;
            }
        }

        public bool HasScheduleWithoutTransactions { get; set; }
        public bool HasTimeStampsWithoutTransactions { get; set; }
        public bool HasInvalidTimeStamps { get; set; }
        public bool HasPayrollImports { get; set; }

        #endregion

        #endregion

        #region Ctor

        public AttestEmployeePeriodDTO()
        {
            this.UniqueId = Guid.NewGuid().ToString();
            this.ScheduleTime = new TimeSpan();
            this.AttestStates = new List<AttestStateDTO>();
        }

        #endregion

        #region Public methods

        public void AddExpense(int? timeCodeTransactionId, decimal? amount, string name)
        {
            if (!timeCodeTransactionId.HasValue)
                return;

            if (this.Expenses == null)
                this.Expenses = new List<AttestExpenseSumDTO>();
            this.Expenses.Upsert(timeCodeTransactionId.Value, name, amount);
        }

        #endregion
    }

    public class AttestEmployeeAdditionDeductionDTO
    {
        #region Variables

        public int ExpenseRowId { get; set; }
        public int TimeCodeTransactionId { get; set; }
        public int TimeCodeId { get; set; }
        public string TimeCodeName { get; set; }
        public SoeTimeCodeType TimeCodeType { get; set; }
        public TermGroup_ExpenseType TimeCodeExpenseType { get; set; }
        public TermGroup_TimeCodeRegistrationType RegistrationType { get; set; }
        public string TimeCodeComment { get; set; }
        public bool TimeCodeStopAtDateStart { get; set; }
        public bool TimeCodeStopAtDateStop { get; set; }
        public bool TimeCodeStopAtPrice { get; set; }
        public bool TimeCodeStopAtVat { get; set; }
        public bool TimeCodeStopAtAccounting { get; set; }
        public bool TimeCodeStopAtComment { get; set; }
        public bool TimeCodeCommentMandatory { get; set; }
        public DateTime Start { get; set; }
        public DateTime Stop { get; set; }
        public decimal Quantity { get; set; }
        public string QuantityText { get; set; }
        public decimal? UnitPrice { get; set; }
        public bool IsSpecifiedUnitPrice { get; set; }
        public decimal? Amount { get; set; }
        public decimal? VatAmount { get; set; }
        public string Comment { get; set; }
        public string Accounting { get; set; }
        public SoeEntityState State { get; set; }
        public int? TimePeriodId { get; set; }
        public int? CustomerInvoiceId { get; set; }
        public int? CustomerInvoiceRowId { get; set; }
        public int? ProjectId { get; set; }

        //Calculated
        public int? AttestStateId { get; set; }
        public string AttestStateName { get; set; }
        public string AttestStateColor { get; set; }
        public bool IsAttested { get; set; }
        public bool IsReadOnly { get; set; }
        public string InvoiceNumber { get; set; }
        public string CustomerNumber { get; set; }
        public string CustomerName { get; set; }
        public string ProjectNumber { get; set; }
        public string ProjectName { get; set; }
        public bool PriceListInclVat { get; set; }
        public bool HasFiles { get; set; }

        public List<AttestEmployeeAdditionDeductionTransactionDTO> Transactions { get; set; }

        #endregion
    }

    public class AttestEmployeeAdditionDeductionTransactionDTO
    {
        #region Variables

        public SoeTimeTransactionType TransactionType { get; set; }
        public int TransactionId { get; set; }
        public DateTime Date { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int AttestStateId { get; set; }
        public string AttestStateName { get; set; }
        public string AttestStateColor { get; set; }
        public bool IsAttested { get; set; }
        public bool IsReadOnly { get; set; }
        public decimal Quantity { get; set; }
        public string QuantityText { get; set; }
        public decimal? UnitPrice { get; set; }
        public decimal? Amount { get; set; }
        public decimal? VatAmount { get; set; }
        public string Comment { get; set; }
        public string AccountingString { get; set; }

        #endregion

    }

    public class AttestEmployeeBreakDTO
    {
        #region Variables

        public int TimeBlockDateId { get; set; }
        public int TimeBlockId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime StopTime { get; set; }
        public int Minutes { get; set; }

        //Flags
        public bool IsModified { get; set; }

        #endregion
    }

    public class AttestEmployeeDayTimeCodeTransactionDTO
    {
        #region Variables

        public string GuidId { get; set; }
        public string GuidIdTimeBlock { get; set; }

        public int TimeCodeTransactionId { get; set; }
        public int TimeCodeId { get; set; }
        public int? TimeRuleId { get; set; }
        public int? TimeBlockId { get; set; }
        public int? ProjectTimeBlockId { get; set; }

        public DateTime StartTime { get; set; }
        public DateTime StopTime { get; set; }
        public decimal Quantity { get; set; }
        public string QuantityString
        {
            get
            {
                return CalendarUtility.GetHoursAndMinutesString(Convert.ToInt32(this.Quantity));
            }
        }
        public string TimeCodeName { get; set; }
        public SoeTimeCodeType TimeCodeType { get; set; }
        public TermGroup_TimeCodeRegistrationType TimeCodeRegistrationType { get; set; }
        public string TimeRuleName { get; set; }
        public int? TimeRuleSort { get; set; }
        public bool IsTimeCodePresenceOutsideScheduleTime { get; set; }
        public bool IsTimeCodeAbsenceTime { get; set; }

        #endregion

        #region Ctor

        public AttestEmployeeDayTimeCodeTransactionDTO()
        {
            this.GuidId = Guid.NewGuid().ToString();
        }

        #endregion
    }

    public class AttestEmployeeDayTimeInvoiceTransactionDTO
    {
        #region Variables

        public string GuidId { get; set; }

        #endregion

        #region Ctor

        public AttestEmployeeDayTimeInvoiceTransactionDTO()
        {
            this.GuidId = Guid.NewGuid().ToString();
        }

        #endregion
    }

    [TSInclude]
    public class AttestPayrollTransactionDTO : IPayrollTransaction
    {
        #region Variables

        //Guids
        public string GuidId { get; set; }
        public string ParentGuidId { get; set; }
        public string GuidIdTimeBlock { get; set; }
        public string GuidIdTimeCodeTransaction { get; set; }

        //General
        public string AttestItemUniqueId { get; set; }
        public string PayrollCalculationProductUniqueId { get; set; }
        public int EmployeeId { get; set; }

        //TimePayrollTransaction
        public int TimePayrollTransactionId { get; set; }
        public List<int> AllTimePayrollTransactionIds { get; set; } //Only used in PayrollCalculation when all transactions for one day is merged to one virtual transaction
        public decimal? UnitPrice { get; set; }
        public decimal? UnitPriceCurrency { get; set; }
        public decimal? UnitPriceEntCurrency { get; set; }
        public decimal? UnitPriceGrouping
        {
            get
            {
                if (this.IsEmploymentTax())
                    return 0;
                else if (this.IsSupplementCharge())
                    return 1;
                else if (this.IsVacationAddition())
                    return 1;
                else
                    return this.UnitPrice;
            }
        }
        public decimal? UnitPricePayrollSlipGrouping
        {
            get
            {
                if (this.IsVacationSalary() || this.IsAbsenceVacationAll())
                    return 1;
                else
                    return UnitPriceGrouping;
            }
        }
        public int CommentGrouping
        {
            get
            {
                if (this.IsAdded && !String.IsNullOrEmpty(this.Comment))
                    return 0;
                else
                    return 1;
            }
        }
        public decimal? Amount { get; set; }
        public decimal? AmountCurrency { get; set; }
        public decimal? AmountEntCurrency { get; set; }
        public decimal? VatAmount { get; set; }
        public decimal? VatAmountCurrency { get; set; }
        public decimal? VatAmountEntCurrency { get; set; }
        public decimal Quantity { get; set; }
        public string QuantityString
        {
            get
            {
                return IsQuantityOrFixed ? this.Quantity.ToString() : CalendarUtility.GetHoursAndMinutesString(Convert.ToInt32(this.Quantity), false);
            }
        }
        public bool IsQuantityOrFixed
        {
            get
            {
                if (TimeCodeRegistrationType == TermGroup_TimeCodeRegistrationType.Quantity || this.IsFixed || this.IsCentRounding || this.IsAdded || this.IsUnionFee || this.IsEmployeeVehicle /* || this.IsVacationYearEnd*/)
                    return true;
                if (this.IsTax())
                    return true;
                if (this.IsEmploymentTaxCredit())
                    return true;
                if (this.IsEmploymentTaxDebit())
                    return true;
                if (this.IsSupplementCharge())
                    return true;
                if (this.IsNetSalary())
                    return true;
                if (this.IsVacationCompensationDirectPaid())
                    return true;
                if (this.IsVacationCompensationSavedOverdueVariable())
                    return true;
                if (this.IsVacationAdditionOrSalaryPrepaymentInvert())
                    return true;
                if (this.IsVacationAdditionOrSalaryVariablePrepaymentInvert())
                    return true;
                if (this.IsVacationAdditionOrSalaryPrepaymentPaid())
                    return true;
                if (this.IsVacationAdditionOrSalaryVariablePrepaymentPaid())
                    return true;
                if (this.IsBenefitInvert())
                    return true;
                if (this.IsVacationCompensationAdvance())
                    return true;
                if (this.IsWeekendSalary())
                    return true;
                return false;
            }
        }
        public DateTime? ReversedDate { get; set; }
        public bool IsReversed { get; set; }
        public bool IsPreliminary { get; set; }
        public bool IsExported { get; set; }
        public bool IsEmploymentTaxAndHidden
        {
            get
            {
                return this.IsEmploymentTax() && this.IsBelowEmploymentTaxLimitRuleHidden;
            }
        }
        public bool IsBelowEmploymentTaxLimitRuleHidden { get; set; }
        public bool IsBelowEmploymentTaxLimitRuleFromPreviousPeriods { get; set; }
        public bool ManuallyAdded { get; set; }
        public String Comment { get; set; }
        public bool HasComment
        {
            get
            {
                return !String.IsNullOrEmpty(Comment);
            }
        }
        public bool HasInfo { get; set; }
        public DateTime? AddedDateFrom { get; set; }
        public DateTime? AddedDateTo { get; set; }
        public bool IsAdded { get; set; }
        public bool IsFixed { get; set; }
        public bool IsCentRounding { get; set; }
        public bool IsQuantityRounding { get; set; }
        public bool IsSpecifiedUnitPrice { get; set; }
        public bool IsAdditionOrDeduction { get; set; }
        public bool IsVacationReplacement { get; set; }
        public bool IsAddedOrFixed
        {
            get
            {
                return this.IsAdded || this.IsFixed;
            }
        }
        public bool IsRounding
        {
            get
            {
                return this.IsCentRounding || this.IsQuantityRounding;
            }
        }
        public bool ShowEdit
        {
            get
            {
                if (this.IsAdded)
                    return true;
                else if (this.IsEmploymentTax() || this.IsTaxAndNotOptional() || this.IsSupplementCharge() || this.IsNetSalary() || this.IsRounding || this.IsVacationCompensationDirectPaid() || this.IsUnionFee || this.IsEmployeeVehicle)
                    return false;
                else
                    return true;
            }
        }
        public bool IsPayrollProductChainMainParent
        {
            get
            {
                return this.IncludedInPayrollProductChain && !this.ParentId.HasValue;
            }
        }
        public bool IncludedInPayrollProductChain { get; set; }
        public bool UpdateChildren { get; set; }
        public int? ParentId { get; set; }
        public int? UnionFeeId { get; set; }
        public bool IsUnionFee
        {
            get
            {
                return this.UnionFeeId.HasValue;
            }

        }
        public int? EmployeeVehicleId { get; set; }
        public bool IsEmployeeVehicle
        {
            get
            {
                return this.EmployeeVehicleId.HasValue;
            }

        }
        public int? EarningTimeAccumulatorId { get; set; }
        public int? EmployeeChildId { get; set; }
        public string EmployeeChildName { get; set; }
        public int? PayrollStartValueRowId { get; set; }
        public bool IsPayrollStartValue
        {
            get
            {
                return this.PayrollStartValueRowId.HasValue;
            }

        }
        public int? RetroactivePayrollOutcomeId { get; set; }
        public bool IsRetroactive
        {
            get
            {
                return this.RetroactivePayrollOutcomeId.HasValue;
            }

        }
        public int? VacationYearEndRowId { get; set; }
        public bool IsVacationYearEnd
        {
            get
            {
                return this.VacationYearEndRowId.HasValue;
            }

        }
        public bool IsVacationFiveDaysPerWeek { get; set; }
        public int? TransactionSysPayrollTypeLevel1 { get; set; }
        public int? TransactionSysPayrollTypeLevel2 { get; set; }
        public int? TransactionSysPayrollTypeLevel3 { get; set; }
        public int? TransactionSysPayrollTypeLevel4 { get; set; }
        public int AbsenceIntervalNr { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }

        //TimePayrollTransactionExtended
        public int? PayrollPriceFormulaId { get; set; }
        public int? PayrollPriceTypeId { get; set; }
        public string Formula { get; set; }
        public string FormulaPlain { get; set; }
        public string FormulaExtracted { get; set; }
        public string FormulaNames { get; set; }
        public string FormulaOrigin { get; set; }
        public bool? PayrollCalculationPerformed { get; set; }
        public bool IsDistributed { get; set; }
        public bool IsAverageCalculated { get; set; }
        public int TimeUnit { get; set; }
        public decimal QuantityWorkDays { get; set; }
        public decimal QuantityCalendarDays { get; set; }
        public decimal CalenderDayFactor { get; set; }
        public decimal QuantityDays
        {
            get
            {
                if (this.TimeUnit == (int)TermGroup_PayrollProductTimeUnit.WorkDays)
                    return this.QuantityWorkDays;
                else if (this.TimeUnit == (int)TermGroup_PayrollProductTimeUnit.CalenderDays || this.TimeUnit == (int)TermGroup_PayrollProductTimeUnit.CalenderDayFactor || this.TimeUnit == (int)TermGroup_PayrollProductTimeUnit.VacationCoefficient)
                    return this.QuantityCalendarDays;
                else
                    return 0;
            }

        }

        //TimePayrollScheduleTransaction
        public SoeTimePayrollScheduleTransactionType ScheduleTransactionType { get; set; }
        public bool IsScheduleTransaction { get; set; }

        //PayrollProduct
        public int PayrollProductId { get; set; }
        public string PayrollProductNumber { get; set; }
        public string PayrollProductName { get; set; }
        public string PayrollProductString
        {
            get
            {
                string text = "";
                if (!String.IsNullOrEmpty(this.PayrollProductNumber) && !String.IsNullOrEmpty(this.PayrollProductName))
                    text = PayrollProductNumber + ", " + PayrollProductName;
                return text;
            }
        }
        public string PayrollProductShortName { get; set; }
        public decimal PayrollProductFactor { get; set; }
        public bool PayrollProductPayed { get; set; }
        public bool PayrollProductExport { get; set; }
        public bool PayrollProductUseInPayroll { get; set; }
        public int? PayrollProductSysPayrollTypeLevel1 { get; set; }
        public int? PayrollProductSysPayrollTypeLevel2 { get; set; }
        public int? PayrollProductSysPayrollTypeLevel3 { get; set; }
        public int? PayrollProductSysPayrollTypeLevel4 { get; set; }

        //TimeCodeTransaction
        public int? TimeCodeTransactionId { get; set; }
        public DateTime? StartTime { get; set; }
        public string StartTimeString
        {
            get
            {
                if (this.IsAdded)
                    return this.AddedDateFrom.HasValue ? this.AddedDateFrom.Value.ToString("yyMMdd") : "00:00";
                else
                    return this.StartTime.HasValue ? CalendarUtility.GetHoursAndMinutesString(this.StartTime.Value) : "00:00";
            }
        }
        public DateTime? StopTime { get; set; }
        public string StopTimeString
        {
            get
            {
                if (this.IsAdded)
                    return this.AddedDateTo.HasValue ? this.AddedDateTo.Value.ToString("yyMMdd") : "00:00";
                else
                    return this.StopTime.HasValue ? CalendarUtility.GetHoursAndMinutesString(this.StopTime.Value) : "00:00";
            }
        }

        //TimeCode
        public SoeTimeCodeType TimeCodeType { get; set; }
        public TermGroup_TimeCodeRegistrationType TimeCodeRegistrationType { get; set; }
        public int? NoOfPresenceWorkOutsideScheduleTime { get; set; }
        public bool IsPresenceWorkOutsideScheduleTime //IsWorkOutsideSchedule on TimeCodeWork
        {
            get
            {
                return this.NoOfPresenceWorkOutsideScheduleTime.HasValue && this.NoOfPresenceWorkOutsideScheduleTime.Value > 0;
            }
        }
        public int? NoOfAbsenceAbsenceTime { get; set; }
        public bool IsAbsenceAbsenceTime //IsAbsence on TimeCodeAbsense
        {
            get
            {
                return this.NoOfAbsenceAbsenceTime.HasValue && this.NoOfAbsenceAbsenceTime.Value > 0;
            }
        }

        //TimeBlockDate
        public int TimeBlockDateId { get; set; }
        public DateTime Date { get; set; }

        //TimeBlock
        public int? TimeBlockId { get; set; }

        //AttestState
        public int AttestStateId { get; set; }
        public string AttestStateName { get; set; }
        public string AttestStateColor { get; set; }
        public bool AttestStateInitial { get; set; }
        public int AttestStateSort { get; set; }
        public bool HasSameAttestState { get; set; } //Only used in PayrollCalculation when all transactions for one day is merged to one virtual transaction
        public bool HasAttestState
        {
            get
            {
                return this.AttestStateId != 0;
            }
        }

        //AttestTransitions
        public List<AttestTransitionLogDTO> AttestTransitionLogs { get; set; }

        //Accounting
        public List<AccountDimDTO> AccountDims { get; set; }
        public AccountDTO AccountStd { get; set; }
        public List<AccountDTO> AccountInternals { get; set; }
        public string AccountingShortString { get; set; }
        public string AccountingLongString { get; set; }
        public List<AccountingSettingsRowDTO> AccountingSettings { get; set; } //Only used for addedtransactions
        public string AccountingDescription
        {
            get
            {
                if (this.IsDistributed)
                    return "1";
                else
                    return this.AccountingShortString;
            }

        }
        public int AccountStdId { get; set; }
        public List<int> AccountInternalIds { get; set; }

        //TimePeriod
        public int? TimePeriodId { get; set; }
        public string TimePeriodName { get; set; }

        //Retro
        public string RetroTransactionType { get; set; }

        //Billing attest
        public int? InvoiceQuantity { get; set; }

        //Flags
        public bool IsModified { get; set; }
        public bool IsSelected { get; set; }
        public bool IsSelectDisabled { get; set; }
        public bool IsPresence { get; set; }
        public bool IsAbsence { get; set; }

        //Temp links
        public int? PayrollImportEmployeeTransactionId { get; set; }


        #endregion

        #region IPayrollTransaction implementation

        [TsIgnore]
        public int ProductId => this.PayrollProductId;

        #endregion

        #region IPayrollType implementation

        public int? SysPayrollTypeLevel1 => this.TransactionSysPayrollTypeLevel1;
        public int? SysPayrollTypeLevel2 => this.TransactionSysPayrollTypeLevel2;
        public int? SysPayrollTypeLevel3 => this.TransactionSysPayrollTypeLevel3;
        public int? SysPayrollTypeLevel4 => this.TransactionSysPayrollTypeLevel4;

        #endregion

        #region Ctor

        public AttestPayrollTransactionDTO()
        {
            this.GuidId = Guid.NewGuid().ToString();
            this.AccountStd = null; //Not always used. Null meaning not use.
            this.AccountInternals = new List<AccountDTO>();
            this.AttestTransitionLogs = new List<AttestTransitionLogDTO>();
        }

        #endregion

        #region Public methods

        public bool Match(AttestPayrollTransactionDTO item)
        {
            if (item == null)
                return false;

            if (this.TimeBlockDateId != item.TimeBlockDateId ||
                this.PayrollProductId != item.PayrollProductId ||
                this.IsPreliminary != item.IsPreliminary ||
                this.IsPresence != item.IsPresence ||
                this.IsAbsence != item.IsAbsence ||
                this.IsScheduleTransaction != item.IsScheduleTransaction ||
                this.AttestStateId != item.AttestStateId)
                return false;

            var currentAccountInternals = this.AccountInternals != null ? this.AccountInternals.OrderBy(i => i.AccountId).ToList() : new List<AccountDTO>();
            var inputAccountInternals = item.AccountInternals != null ? item.AccountInternals.OrderBy(i => i.AccountId).ToList() : new List<AccountDTO>();
            if (currentAccountInternals.Count != inputAccountInternals.Count)
                return false;

            for (int i = 0; i < currentAccountInternals.Count; i++)
            {
                if (currentAccountInternals[i].AccountId != inputAccountInternals[i].AccountId)
                    return false;
            }

            return true;
        }

        public void Update(AttestPayrollTransactionDTO item)
        {
            if (item == null)
                return;

            //Quantity
            this.Quantity += item.Quantity;

            //Amount
            if (item.Amount.HasValue)
                this.Amount += item.Amount.Value;
            if (item.AmountCurrency.HasValue)
                this.AmountCurrency += item.AmountCurrency.Value;
            if (item.AmountEntCurrency.HasValue)
                this.AmountEntCurrency += item.AmountEntCurrency.Value;

            //VatAmount
            if (item.VatAmount.HasValue)
                this.VatAmount += item.VatAmount.Value;
            if (item.VatAmountCurrency.HasValue)
                this.VatAmountCurrency += item.VatAmountCurrency.Value;
            if (item.VatAmountEntCurrency.HasValue)
                this.VatAmountEntCurrency += item.VatAmountEntCurrency.Value;

            //Comment
            if (!item.Comment.IsNullOrEmpty())
            {
                if (!this.Comment.IsNullOrEmpty())
                    this.Comment += ", ";

                this.Comment += item.Comment;
            }
        }

        public bool ContainsAnyAccount(List<int> accountIds)
        {
            return accountIds != null && this.AccountInternals != null && accountIds.ContainsAny(this.AccountInternals.Select(i => i.AccountId).ToList());
        }

        public bool ShowAsAbsence()
        {
            return this.IsAbsence && this.IsAbsenceAbsenceTime && (this.IsAbsence() || this.IsTimeAccumulatorMinusTime());

        }

        #endregion
    }

    public class AttestExpenseSumDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Amount { get; set; }

        public AttestExpenseSumDTO(int id, string name, decimal amount)
        {
            this.Id = id;
            this.Name = name;
            this.Amount = amount;
        }

        public void Update(decimal amount)
        {
            this.Amount += amount;
        }
    }

    [TSInclude]
    public class AttestTransitionLogDTO
    {
        #region Variables

        public int? AttestTransitionLogId { get; set; }
        public int TimePayrollTransactionId { get; set; }
        public DateTime AttestTransitionDate { get; set; }
        public string AttestStateFromName { get; set; }
        public string AttestStateToName { get; set; }
        public int AttestTransitionUserId { get; set; }
        public string AttestTransitionUserName { get; set; }
        public bool AttestTransitionCreatedBySupport { get; set; }

        #endregion
    }

    public class AttestFunctionOptionDescription
    {
        public string Description1 { get; set; }
        public string Description2Caption { get; set; }
        public string Description2 { get; set; }
        public string Description3Caption { get; set; }
        public string Description3 { get; set; }
    }

    public class TimeUnhandledShiftChangesEmployeeDTO
    {
        public int EmployeeId { get; set; }
        public List<TimeUnhandledShiftChangesWeekDTO> Weeks { get; set; } = new List<TimeUnhandledShiftChangesWeekDTO>();
        public bool HasDays
        {
            get
            {
                return this.HasShiftDays || this.HasExtraShiftDays;
            }
        }
        public bool HasShiftDays
        {
            get
            {
                return this.Weeks.Any(w => w.HasShiftDays);
            }
        }
        public bool HasExtraShiftDays
        {
            get
            {
                return this.Weeks.Any(w => w.HasExtraShiftDays);
            }
        }

        public TimeUnhandledShiftChangesEmployeeDTO(int employeeId)
        {
            this.EmployeeId = employeeId;
        }

        public void AddDaysToWeek((DateTime StartDate, DateTime StopDate) week, List<TimeBlockDateDTO> shiftDays = null, List<TimeBlockDateDTO> extraShiftDays = null)
        {
            TimeUnhandledShiftChangesWeekDTO unhandledWeek = this.Weeks.FirstOrDefault(w => w.Match(week));
            if (unhandledWeek == null)
            {
                unhandledWeek = new TimeUnhandledShiftChangesWeekDTO(week);
                this.Weeks.Add(unhandledWeek);
            }
            if (shiftDays != null)
                unhandledWeek.ShiftDays = shiftDays;
            if (extraShiftDays != null)
                unhandledWeek.ExtraShiftDays = extraShiftDays;
        }
    }

    public class TimeUnhandledShiftChangesWeekDTO
    {
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public int WeekNr { get; set; }
        public List<TimeBlockDateDTO> ShiftDays { get; set; } = new List<TimeBlockDateDTO>();
        public List<TimeBlockDateDTO> ExtraShiftDays { get; set; } = new List<TimeBlockDateDTO>();
        public bool HasDays
        {
            get
            {
                return this.HasShiftDays || this.HasExtraShiftDays;
            }
        }
        public bool HasShiftDays
        {
            get
            {
                return this.ShiftDays.Any();
            }
        }
        public bool HasExtraShiftDays
        {
            get
            {
                return ExtraShiftDays.Any();
            }
        }

        public TimeUnhandledShiftChangesWeekDTO((DateTime StartDate, DateTime StopDate) week)
        {
            this.DateFrom = week.StartDate;
            this.DateTo = week.StopDate;
            this.WeekNr = CalendarUtility.GetWeekNr(week.StartDate);
        }

        public bool Match((DateTime StartDate, DateTime StopDate) week)
        {
            return this.DateFrom == week.StartDate && this.DateTo == week.StopDate;
        }
    }

    #region Save DTOs

    public class SaveAttestEmployeeDayDTO
    {
        #region Variables

        public string OriginalUniqueId { get; set; }
        public DateTime Date { get; set; }
        public int TimeBlockDateId { get; set; }

        #endregion
    }

    public class SaveAttestEmployeeValidationDTO
    {
        public bool Success { get; set; }
        public bool CanOverride { get; set; }
        public bool CanSkipDialog { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public List<SaveAttestEmployeeDayDTO> ValidItems { get; set; } //TODO: ValidDays
    }

    [TSInclude]
    public class SaveAttestTransactionsValidationDTO
    {
        public bool Success { get; set; }
        public bool CanOverride { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public List<SaveAttestTransactionDTO> ValidItems { get; set; }
    }

    public class ReverseTransactionsValidationDTO
    {
        public bool Success { get; set; }
        public bool ApplySilent { get; set; }
        public bool CanContinue { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public bool UsePayroll { get; set; }
        public List<DateTime> ValidDates { get; set; }
        public List<TimePeriodDTO> ValidPeriods { get; set; }
        public List<TimeDeviationCauseDTO> ValidCauses { get; set; }
    }

    public class TimeAttestCalculationFunctionValidationDTO
    {
        public bool Success { get; set; }
        public bool ApplySilent { get; set; }
        public bool CanOverride { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public List<AttestEmployeeDaySmallDTO> ValidItems { get; set; }
        public SoeTimeAttestFunctionOption Option { get; set; }
    }

    public class PeriodCalculationResultDTO
    {
        public int EmployeeId { get; set; }
        public List<TimePeriodDTO> Periods { get; set; }
        public string CurrentPeriod { get; set; }
        public string ParentPeriod { get; set; }

        public PeriodCalculationResultDTO()
        {
            Periods = new List<TimePeriodDTO>();
        }
    }

    [TSInclude]
    public class SaveAttestTransactionDTO
    {
        #region Variables

        public string OriginalUniqueId { get; set; }
        public int EmployeeId { get; set; }
        public DateTime Date { get; set; }
        public List<int> TimePayrollTransactionIds { get; set; }

        #endregion
    }

    public class SaveDeviationsEmployeeDTO
    {
        #region Variables

        public DateTime Date { get; set; }
        public int TimeScheduleTemplatePeriodId { get; set; }

        //Collections
        public List<SaveTimeBlockDTO> TimeBlockItems { get; set; }
        public List<SaveTimeBlockDTO> BreakItems { get; set; }

        #endregion

        #region Ctor

        public SaveDeviationsEmployeeDTO()
        {
            TimeBlockItems = new List<SaveTimeBlockDTO>();
            BreakItems = new List<SaveTimeBlockDTO>();
        }

        #endregion
    }

    public class SaveTimeBlockDTO
    {
        #region Variables

        public int TimeBlockId { get; set; }
        public int TimeBlockDateId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime StopTime { get; set; }

        #endregion

        #region Public methods

        public bool IsModified(DateTime startTime, DateTime stopTime)
        {
            return this.StartTime != startTime || this.StopTime != stopTime;
        }

        #endregion
    }

    #endregion

    #endregion

    public static class AttestExtensions
    {
        #region AttestEmployeeDayDTO

        public static List<AttestEmployeeDayDTO> Filter(this List<AttestEmployeeDayDTO> l, DateTime startDate, DateTime stopDate)
        {
            return l?.Where(e => e.Date >= startDate && e.Date <= stopDate).ToList() ?? new List<AttestEmployeeDayDTO>();
        }

        public static void RemoveBreaksOutsideSchedule(this AttestEmployeeDayDTO e, List<AttestEmployeeDayShiftDTO> shifts, out int scheduleBreakMinutes)
        {
            scheduleBreakMinutes = e.ScheduleBreakMinutes;

            bool isBreakOutside1, isBreakOutside2, isBreakOutside3, isBreakOutside4;
            isBreakOutside1 = isBreakOutside2 = isBreakOutside3 = isBreakOutside4 = true;

            if (shifts != null)
            {
                foreach (var shift in shifts)
                {
                    DateTime shiftStartTime = CalendarUtility.GetScheduleTime(shift.StartTime, shift.StartTime.Date, shift.StopTime.Date);
                    DateTime shiftStopTime = CalendarUtility.GetScheduleTime(shift.StopTime, shift.StartTime.Date, shift.StopTime.Date);

                    // Break is completely overlapped by a presence shift
                    if (isBreakOutside1 && CalendarUtility.GetOverlappingMinutes(e.ScheduleBreak1Start, e.ScheduleBreak1Stop, shiftStartTime, shiftStopTime) > 1)
                        isBreakOutside1 = false;
                    // Break is completely overlapped by a presence shift
                    if (isBreakOutside2 && CalendarUtility.GetOverlappingMinutes(e.ScheduleBreak2Start, e.ScheduleBreak2Stop, shiftStartTime, shiftStopTime) > 1)
                        isBreakOutside2 = false;
                    // Break is completely overlapped by a presence shift
                    if (isBreakOutside3 && CalendarUtility.GetOverlappingMinutes(e.ScheduleBreak3Start, e.ScheduleBreak3Stop, shiftStartTime, shiftStopTime) > 1)
                        isBreakOutside3 = false;
                    // Break is completely overlapped by a presence shift
                    if (isBreakOutside4 && CalendarUtility.GetOverlappingMinutes(e.ScheduleBreak4Start, e.ScheduleBreak4Stop, shiftStartTime, shiftStopTime) > 1)
                        isBreakOutside4 = false;
                }
            }

            if (isBreakOutside1 || isBreakOutside2 || isBreakOutside3 || isBreakOutside4)
            {
                e.ClearBreaks(isBreakOutside1, isBreakOutside2, isBreakOutside3, isBreakOutside4);
                scheduleBreakMinutes = e.ScheduleBreak1Minutes + e.ScheduleBreak2Minutes + e.ScheduleBreak3Minutes + e.ScheduleBreak4Minutes;
            }
        }

        public static void TrimDayFromOnDutyOutsideSchedule(this AttestEmployeeDayDTO e)
        {
            if (e.Shifts.Any(s => s.Type == TermGroup_TimeScheduleTemplateBlockType.OnDuty))
            {
                DateTime firstOnDutyTime = e.Shifts.Where(s => s.Type == TermGroup_TimeScheduleTemplateBlockType.OnDuty).OrderBy(s => s.StartTime).Select(s => s.StartTime).FirstOrDefault();
                DateTime lastOnDutyTime = e.Shifts.Where(s => s.Type == TermGroup_TimeScheduleTemplateBlockType.OnDuty).OrderByDescending(s => s.StopTime).Select(s => s.StopTime).FirstOrDefault();
                DateTime firstShiftTime = e.Shifts.Where(s => s.Type != TermGroup_TimeScheduleTemplateBlockType.OnDuty).OrderBy(s => s.StartTime).Select(s => s.StartTime).FirstOrDefault();
                DateTime lastShiftTime = e.Shifts.Where(s => s.Type != TermGroup_TimeScheduleTemplateBlockType.OnDuty).OrderByDescending(s => s.StopTime).Select(s => s.StopTime).FirstOrDefault();

                if (firstOnDutyTime < firstShiftTime)
                    e.ScheduleStartTime = CalendarUtility.GetActualDateTime(CalendarUtility.DATETIME_DEFAULT, firstShiftTime);
                if (lastOnDutyTime > lastShiftTime)
                    e.ScheduleStopTime = CalendarUtility.GetActualDateTime(CalendarUtility.DATETIME_DEFAULT, lastShiftTime);
            }
        }

        public static void ClearBreaks(this AttestEmployeeDayDTO e, bool clearBreak1 = true, bool clearBreak2 = true, bool clearBreak3 = true, bool clearBreak4 = true)
        {
            if (clearBreak1)
            {
                e.ScheduleBreak1Minutes = 0;
                e.ScheduleBreak1Start = CalendarUtility.DATETIME_DEFAULT;
            }
            if (clearBreak2)
            {
                e.ScheduleBreak2Minutes = 0;
                e.ScheduleBreak2Start = CalendarUtility.DATETIME_DEFAULT;
            }
            if (clearBreak3)
            {
                e.ScheduleBreak3Minutes = 0;
                e.ScheduleBreak3Start = CalendarUtility.DATETIME_DEFAULT;
            }
            if (clearBreak4)
            {
                e.ScheduleBreak4Minutes = 0;
                e.ScheduleBreak4Start = CalendarUtility.DATETIME_DEFAULT;
            }
        }

        public static bool HasAbsenceSick(this List<AttestEmployeeDayDTO> l)
        {
            return l?.Any(e => e.SumGrossSalaryAbsenceSick.TotalMinutes > 0) ?? false;
        }

        public static bool HasOvertime(this List<AttestEmployeeDayDTO> l)
        {
            return l?.Any(e => e.HasOvertime) ?? false;
        }

        public static bool HasUnhandledShiftChanges(this List<AttestEmployeeDayDTO> l)
        {
            return l?.Any(e => e.HasUnhandledShiftChanges) ?? false;
        }

        public static bool HasUnhandledExtraShiftChanges(this List<AttestEmployeeDayDTO> l)
        {
            return l?.Any(e => e.HasUnhandledExtraShiftChanges) ?? false;
        }

        #endregion

        #region AttestEmployeePeriodDTO

        public static bool HasOvertime(this List<AttestEmployeePeriodDTO> l)
        {
            return l?.Any(e => e.HasOvertime) ?? false;
        }

        public static bool HasOvertimeInWeek(this AttestEmployeePeriodDTO e, (DateTime StartDate, DateTime StopDate) week)
        {
            return e != null && e.HasOvertime && e.OvertimeDates.Any(date => CalendarUtility.IsDateInRange(date, week.StartDate, week.StopDate));
        }

        public static bool HasAbsenceSick(this List<AttestEmployeePeriodDTO> l)
        {
            return l?.Any(e => e.HasAbsenceSick()) ?? false;
        }

        public static bool HasAbsenceSick(this AttestEmployeePeriodDTO e)
        {
            return e.SumGrossSalaryAbsenceSick.TotalMinutes > 0;
        }

        public static bool HasAbsenceSickInWeek(this AttestEmployeePeriodDTO e, (DateTime StartDate, DateTime StopDate) week)
        {
            return e != null && e.HasAbsenceSick() && e.AbsenceSickDates.Any(date => CalendarUtility.IsDateInRange(date, week.StartDate, week.StopDate));
        }

        #endregion

        #region AttestExpenseSumDTO

        public static AttestExpenseSumDTO Find(this List<AttestExpenseSumDTO> l, int id)
        {
            return l?.FirstOrDefault(i => i.Id == id);
        }

        public static void Upsert(this List<AttestExpenseSumDTO> l, int id, string name, decimal? amount)
        {
            var e = l.Find(id);
            if (e != null && amount.HasValue)
                e.Update(amount.Value);
            else if (e == null)
                l.Add(new AttestExpenseSumDTO(id, name, amount ?? 0));
        }

        public static bool HasAny(this List<AttestExpenseSumDTO> l)
        {
            return !l.IsNullOrEmpty();
        }

        public static int SumRows(this List<AttestExpenseSumDTO> l)
        {
            return l?.Distinct().Count() ?? 0;
        }

        public static decimal SumAmount(this List<AttestExpenseSumDTO> l)
        {
            return l?.Sum(i => i.Amount) ?? Decimal.Zero;
        }

        #endregion

        #region TimeEmployeeTreeNodeDTO

        public static void AddEmployee(this Dictionary<int, List<TimeEmployeeTreeNodeDTO>> d, int groupId, TimeEmployeeTreeNodeDTO employeeNode)
        {
            if (d == null || employeeNode == null)
                return;

            if (!d.ContainsKey(groupId))
                d.Add(groupId, new List<TimeEmployeeTreeNodeDTO>());
            if (!d[groupId].Exists(i => i.EmployeeId == employeeNode.EmployeeId))
                d[groupId].Add(employeeNode);
        }

        #endregion
    }
}

using SoftOne.Soe.Business.DataCache;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SoftOne.Soe.Business.Core.TimeTree
{
    public abstract class TimeTreeBaseManager : ManagerBase
    {
        #region Ctor

        protected TimeTreeBaseManager(ParameterObject parameterObject) : base(parameterObject) { }

        #endregion

        #region Tree

        protected void GenerateTimeEmployeeTree(CompEntities entities, ref TimeEmployeeTreeDTO tree, GenerateTimeEmployeeTreeInput input)
        {
            if (tree?.Settings == null || input?.EmployeeAuthModel == null)
                return;

            if (tree.Grouping == TermGroup_AttestTreeGrouping.EmployeeAuthModel)
                CreateTimeEmployeeTreeByAuthModel(entities, ref tree, input);
            else if (tree.Grouping == TermGroup_AttestTreeGrouping.EmployeeGroup)
                CreateTimeEmployeeTreeByEmployeeGroups(entities, ref tree, input);
            else if (tree.Grouping == TermGroup_AttestTreeGrouping.PayrollGroup)
                CreateTimeEmployeeTreeByPayrollGroups(entities, ref tree, input);
            else if (tree.Grouping == TermGroup_AttestTreeGrouping.AttestState)
                CreateTimeEmployeeTreeByAttestStates(entities, ref tree, input);
            else if (tree.Grouping == TermGroup_AttestTreeGrouping.All)
                CreateTimeEmployeeTreeByAll(entities, ref tree, input);

            tree.SetGroupNodeVisibility();
            tree.Sort();

            if (tree.Settings.IncludeEnded)
                AddEndedEmployeesToTree(ref tree, input.EndedEmployees);

            //Clean
            tree.Settings = null;
        }

        protected void RefreshGroupNode(CompEntities entities, TimeEmployeeTreeDTO tree, TimeEmployeeTreeGroupNodeDTO groupNode, SoeAttestTreeMode mode, List<TimePayrollTransactionTreeDTO> transactionItems, List<AttestState> attestStates, AttestState highestAttestState = null, List<VacationYearEndRow> finalSalaries = null)
        {
            if (tree?.Settings == null || !groupNode.ContainsAnyEmployee(tree.Settings.FilterEmployeeIds))
                return;

            List<int> employeeIdsToRemove = new List<int>();

            foreach (TimeEmployeeTreeNodeDTO employeeNode in groupNode.EmployeeNodes.ToList())
            {
                if (!tree.Settings.FilterEmployeeIds.Contains(employeeNode.EmployeeId))
                    continue;

                if (tree.IsModePayrollCalculation && tree.Settings.DoRefreshFinalSalaryStatus)
                {
                    Employee employee = EmployeeManager.GetEmployee(entities, employeeNode.EmployeeId, base.ActorCompanyId, loadEmployment: true);
                    SetTimeTreeEmployment(tree, employee, out _, finalSalaries: finalSalaries);
                    SetEmployeeFinalSalaryStatus(employee, employeeNode);
                }

                List<AttestStateDTO> attestStatesForEmployee = GetAttestStatesForEmployee(transactionItems, mode, employeeNode.EmployeeId, attestStates).ToDTOs().ToList();
                if (groupNode.DoShowEmployeeNode(tree, employeeNode, attestStatesForEmployee, highestAttestState?.Sort))
                {
                    if (employeeNode.AttestStateId != groupNode.Id)
                        TryMoveEmployeeNodeToOtherAttestGroup(tree, employeeNode, groupNode, employeeNode.AttestStateId, attestStates);
                }
                else
                    employeeIdsToRemove.Add(employeeNode.EmployeeId);
            }

            groupNode.EmployeeNodes = groupNode.EmployeeNodes.Where(node => node != null && !employeeIdsToRemove.Contains(node.EmployeeId)).ToList();
            groupNode.SetAttestStates(groupNode.EmployeeNodes.GetAttestStates());

            foreach (TimeEmployeeTreeGroupNodeDTO childGroupNode in groupNode.ChildGroupNodes)
            {
                RefreshGroupNode(entities, tree, childGroupNode, mode, transactionItems, attestStates, highestAttestState);
                groupNode.AddAttestStates(childGroupNode.GetAttestStates());
            }
        }

        private void TryMoveEmployeeNodeToOtherAttestGroup(TimeEmployeeTreeDTO tree, TimeEmployeeTreeNodeDTO employeeNode, TimeEmployeeTreeGroupNodeDTO groupNode, int attestStateId, List<AttestState> attestStates)
        {
            if (tree == null || tree.Grouping != TermGroup_AttestTreeGrouping.AttestState || employeeNode == null || groupNode?.EmployeeNodes == null || !groupNode.EmployeeNodes.Remove(employeeNode))
                return;

            TimeEmployeeTreeGroupNodeDTO groupNodeForAttestState = tree.GroupNodes.FirstOrDefault(g => g.Id == attestStateId);
            if (groupNodeForAttestState == null)
            {
                AttestState attestState = attestStates?.FirstOrDefault(a => a.AttestStateId == attestStateId);
                if (attestState != null)
                    groupNodeForAttestState = CreateGroupNode(tree, employeeNode.ObjToList(), attestState.AttestStateId, attestState.Name, definedSort: attestState.Sort);
                else if (attestStateId == 0)
                    groupNodeForAttestState = CreateGroupNode(tree, employeeNode.ObjToList(), 0, GetAttestStateMissingMessage(), definedSort: Int32.MaxValue);

                if (groupNodeForAttestState != null)
                    tree.GroupNodes.Add(groupNodeForAttestState);
            }
            else
                groupNodeForAttestState.EmployeeNodes.Add(employeeNode);

            if (groupNodeForAttestState != null)
                groupNodeForAttestState.Expanded = true;
        }

        protected void CreateTimeEmployeeTreeByAuthModel(CompEntities entities, ref TimeEmployeeTreeDTO tree, GenerateTimeEmployeeTreeInput input, bool excludeDuplicateEmployees = false)
        {
            if (tree == null || input.EmployeeAuthModel == null)
                return;

            if (excludeDuplicateEmployees)
                tree.Settings.SetExcludeDuplicateEmployees(true);
            if (tree.IsModePayrollCalculation || tree.Settings.HasFilterOnEmployees || input.TimePeriod.IsExtraPeriod())
                tree.Settings.SetIncludeAdditionalEmployees(false);

            if (input.EmployeeAuthModel.UseAccountHierarchy)
                CreateTimeEmployeeTreeByAccounts(entities, input, ref tree);
            else
                CreateTimeEmployeeTreeByCategories(entities, ref tree, input);
        }

        protected void CreateTimeEmployeeTreeByCategories(CompEntities entities, ref TimeEmployeeTreeDTO tree, GenerateTimeEmployeeTreeInput input)
        {
            if (tree == null)
                return;

            tree.GroupNodes.Clear();

            List<Category> categories = AttestManager.GetCategoriesForAttestRoleUser(entities, SoeCategoryType.Employee, SoeCategoryRecordEntity.AttestRole, base.ActorCompanyId, base.UserId, input.EmployeeAuthModel.GetAttestRoleUsers(), input.StartDate, input.StopDate);
            if (categories.IsNullOrEmpty())
                return;

            if (input.EmployeeAuthModel.FilterIds != null)
            {
                categories = categories.Where(i => input.EmployeeAuthModel.FilterIds.Contains(i.CategoryId)).ToList();
                if (categories.IsNullOrEmpty())
                    return;
            }

            List<CategoryDTO> topCategories = (input.EmployeeAuthModel.FilterIds != null ? categories : categories.GetTopCategories()).ToDTOs(false).ToList();
            if (!topCategories.IsNullOrEmpty())
            {
                Dictionary<Employee, List<int>> employeeCategories = input.Employees.GetCategories(input.EmployeeAuthModel.GetCategoryRecords(), input.StartDate, input.StopDate, onlyDefaultCategories: true);

                foreach (CategoryDTO category in topCategories.OrderBy(i => i.Name))
                {
                    TimeEmployeeTreeGroupNodeDTO groupNode = CreateGroupNodeByCategory(tree, input, category, employeeCategories);
                    if (groupNode != null)
                        tree.AddGroupNode(groupNode);
                }
            }

            if (tree.Settings.IncludeAdditionalEmployees)
                AddAdditionalEmployeesToTreeByCategories(entities, ref tree, input, categories.Select(i => i.CategoryId).ToList());
        }

        protected void CreateTimeEmployeeTreeByAccounts(CompEntities entities, GenerateTimeEmployeeTreeInput input, ref TimeEmployeeTreeDTO tree)
        {
            if (tree == null || input?.EmployeeAuthModel?.AccountRepository == null)
                return;

            tree.GroupNodes.Clear();

            Dictionary<int, AccountDTO> accountsForAttestRole = input.EmployeeAuthModel.AccountRepository.GetAccountsDict(includeVirtualParented: true);
            if (accountsForAttestRole.IsNullOrEmpty())
                return;

            int employeeAccountDimId = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.DefaultEmployeeAccountDimEmployee, 0, base.ActorCompanyId, 0);
            List<int> accountDimIdsAboveEmployeeAccountDim = input.EmployeeAuthModel.AccountRepository.GetAccountDimIdsAboveEmployeeAccountDim(employeeAccountDimId);
            if (!accountDimIdsAboveEmployeeAccountDim.IsNullOrEmpty())
            {
                List<AccountDTO> accountsForEmployeeAccounts = input.EmployeeAuthModel.AccountRepository.GetDefaultAccountsFromEmployeeAccounts();
                if (!accountsForEmployeeAccounts.IsNullOrEmpty())
                {
                    foreach (int accountDimId in accountDimIdsAboveEmployeeAccountDim)
                    {
                        if (!accountsForEmployeeAccounts.Any(a => a.AccountDimId == accountDimId))
                            accountsForAttestRole = accountsForAttestRole.Where(a => a.Value.AccountDimId != accountDimId).ToDictionary(p => p.Key, p => p.Value);
                    }
                }
            }

            int startLevel = accountsForAttestRole.Values.Where(a => !a.IsAbstract).Min(i => i.AccountDim.Level);
            if (startLevel <= 0)
                return;

            List<AccountDTO> topAccounts = accountsForAttestRole.Values.Where(i => i.AccountDim.Level == startLevel).ToList();
            CreateGroupNodesByAccount(ref tree, input, topAccounts, accountsForAttestRole, startLevel);

            int stopLevel = accountsForAttestRole.Values.Max(i => i.AccountDim.Level);
            int currentLevel = startLevel + 1;
            while (tree.GroupNodesIds.Count < accountsForAttestRole.Count && currentLevel <= stopLevel)
            {
                List<int> accountIdsHandled = tree.GroupNodesIds;
                Dictionary<int, AccountDTO> accountsForAttestRoleUnhandled = accountsForAttestRole.Where(i => !accountIdsHandled.Contains(i.Key)).ToDictionary(i => i.Key, i => i.Value);
                List<AccountDTO> currentLevelTopAccounts = accountsForAttestRoleUnhandled.Where(i => i.Value.AccountDim.Level == currentLevel).Select(i => i.Value).ToList();
                CreateGroupNodesByAccount(ref tree, input, currentLevelTopAccounts, accountsForAttestRoleUnhandled, currentLevel);
                currentLevel++;
            }

            if (tree.Settings.IncludeAdditionalEmployees)
            {
                List<int> accountIds = accountsForAttestRole.Values.Where(i => i.AccountDimId == employeeAccountDimId && i.AccountDim.Level >= startLevel).Select(i => i.AccountId).ToList();
                AddAdditionalEmployeesToTreeByAccounts(entities, ref tree, input, accountIds);
            }
        }

        protected void CreateTimeEmployeeTreeByEmployeeGroups(CompEntities entities, ref TimeEmployeeTreeDTO tree, GenerateTimeEmployeeTreeInput input)
        {
            if (tree == null || input?.EmployeeGroups == null)
                return;

            CreateTimeEmployeeTreeByAuthModel(entities, ref tree, input, excludeDuplicateEmployees: true);
            List<TimeEmployeeTreeNodeDTO> employeeNodes = tree.GetEmployeeNodes();
            if (employeeNodes.IsNullOrEmpty())
                return;

            Dictionary<int, List<TimeEmployeeTreeNodeDTO>> groupNodesDict = new Dictionary<int, List<TimeEmployeeTreeNodeDTO>>();

            foreach (TimeEmployeeTreeNodeDTO employeeNode in employeeNodes)
            {
                Employee employee = GetEmployeeForTree(tree, input, employeeNode);
                if (employee == null)
                    continue;

                EmployeeGroup employeeGroup = employee.TimeTreeEmployment.GetEmployeeGroup(employee.GetTimeTreeStartDate(input.StartDate), employee.GetTimeTreeStopDate(input.StopDate), input.EmployeeGroups, forward: false);
                if (employeeGroup != null)
                    groupNodesDict.AddEmployee(employeeGroup.EmployeeGroupId, employeeNode);
            }

            tree.ClearGroupNodes();

            foreach (var pair in groupNodesDict.Where(d => !d.Value.IsNullOrEmpty()))
            {
                EmployeeGroup employeeGroup = input.EmployeeGroups.FirstOrDefault(i => i.EmployeeGroupId == pair.Key);
                if (employeeGroup == null)
                    continue;

                TimeEmployeeTreeGroupNodeDTO groupNode = CreateGroupNode(tree, pair.Value, employeeGroup.EmployeeGroupId, employeeGroup.Name);
                if (groupNode != null)
                    tree.AddGroupNode(groupNode);
            }
        }

        protected void CreateTimeEmployeeTreeByPayrollGroups(CompEntities entities, ref TimeEmployeeTreeDTO tree, GenerateTimeEmployeeTreeInput input)
        {
            if (tree == null || input?.PayrollGroups == null)
                return;

            CreateTimeEmployeeTreeByAuthModel(entities, ref tree, input, excludeDuplicateEmployees: true);
            List<TimeEmployeeTreeNodeDTO> employeeNodes = tree.GetEmployeeNodes();
            if (employeeNodes.IsNullOrEmpty())
                return;

            Dictionary<int, List<TimeEmployeeTreeNodeDTO>> groupNodesDict = new Dictionary<int, List<TimeEmployeeTreeNodeDTO>>();

            foreach (TimeEmployeeTreeNodeDTO employeeNode in employeeNodes)
            {
                Employee employee = GetEmployeeForTree(tree, input, employeeNode);
                if (employee == null)
                    continue;

                PayrollGroup payrollGroup = employee.TimeTreeEmployment.GetPayrollGroup(employee.GetTimeTreeStartDate(input.StartDate), employee.GetTimeTreeStopDate(input.StopDate), input.PayrollGroups, forward: false);
                groupNodesDict.AddEmployee(payrollGroup?.PayrollGroupId ?? 0, employeeNode);
            }

            tree.ClearGroupNodes();

            foreach (var pair in groupNodesDict.Where(d => !d.Value.IsNullOrEmpty()))
            {
                PayrollGroup payrollGroup = input.PayrollGroups.FirstOrDefault(i => i.PayrollGroupId == pair.Key);
                TimeEmployeeTreeGroupNodeDTO groupNode = CreateGroupNode(tree, pair.Value, payrollGroup?.PayrollGroupId ?? 0, payrollGroup?.Name ?? GetPayrolGroupMissingMessage());
                if (groupNode != null)
                    tree.AddGroupNode(groupNode);
            }
        }

        protected void CreateTimeEmployeeTreeByAttestStates(CompEntities entities, ref TimeEmployeeTreeDTO tree, GenerateTimeEmployeeTreeInput input)
        {
            if (tree == null || input?.AttestStates == null)
                return;

            CreateTimeEmployeeTreeByAuthModel(entities, ref tree, input, excludeDuplicateEmployees: true);
            List<TimeEmployeeTreeNodeDTO> employeeNodes = tree.GetEmployeeNodes();
            if (employeeNodes.IsNullOrEmpty())
                return;

            Dictionary<int, List<TimeEmployeeTreeNodeDTO>> groupNodesDict = new Dictionary<int, List<TimeEmployeeTreeNodeDTO>>();

            foreach (TimeEmployeeTreeNodeDTO employeeNode in employeeNodes)
            {
                Employee employee = GetEmployeeForTree(tree, input, employeeNode);
                if (employee == null)
                    continue;

                groupNodesDict.AddEmployee(employeeNode.AttestStateId, employeeNode);
            }

            tree.ClearGroupNodes();

            foreach (var pair in groupNodesDict.Where(d => !d.Value.IsNullOrEmpty()))
            {
                AttestState attestState = input.AttestStates.FirstOrDefault(i => i.AttestStateId == pair.Key);
                TimeEmployeeTreeGroupNodeDTO groupNode = CreateGroupNode(tree, pair.Value, attestState?.AttestStateId ?? 0, attestState?.Name ?? GetAttestStateMissingMessage(), definedSort: attestState?.AttestStateId > 0 ? attestState.Sort : Int32.MaxValue);
                if (groupNode != null)
                    tree.AddGroupNode(groupNode);
            }
        }

        protected void CreateTimeEmployeeTreeByAll(CompEntities entities, ref TimeEmployeeTreeDTO tree, GenerateTimeEmployeeTreeInput input)
        {
            if (tree == null)
                return;

            CreateTimeEmployeeTreeByAuthModel(entities, ref tree, input, excludeDuplicateEmployees: true);
            List<TimeEmployeeTreeNodeDTO> employeeNodesInGroup = tree.GetEmployeeNodes();
            tree.ClearGroupNodes();

            TimeEmployeeTreeGroupNodeDTO groupNode = CreateGroupNode(tree, employeeNodesInGroup.Where(i => !i.IsAdditional).ToList(), Constants.ATTESTTREE_GROUP_ALL, GetAttestStateAllMessage());
            if (groupNode != null)
                tree.AddGroupNode(groupNode);
        }

        protected TimeEmployeeTreeGroupNodeDTO CreateGroupNodeByCategory(TimeEmployeeTreeDTO tree, GenerateTimeEmployeeTreeInput input, CategoryDTO category, Dictionary<Employee, List<int>> employeeCategories)
        {
            List<TimeEmployeeTreeNodeDTO> employeeNodes = new List<TimeEmployeeTreeNodeDTO>();

            foreach (var employeeCategory in employeeCategories.Where(i => i.Key.TimeTreeEmployeeGroup != null))
            {
                List<int> categories = employeeCategory.Value.ToList();
                if (!categories.Contains(category.CategoryId))
                    continue;

                TimeEmployeeTreeNodeDTO employeeNode = CreateEmployeeNode(tree, employeeCategory.Key, category.CategoryId);
                if (employeeNode != null)
                    employeeNodes.Add(employeeNode);
            }

            TimeEmployeeTreeGroupNodeDTO groupNode = CreateGroupNode(tree, employeeNodes, category.CategoryId, category.Name, category.Code, (int)category.Type);
            if (groupNode != null && !category.Children.IsNullOrEmpty())
            {
                foreach (CategoryDTO childCategory in category.Children)
                {
                    if (groupNode.ChildGroupNodes.Any(i => i.Id == childCategory.CategoryId))
                        continue;

                    TimeEmployeeTreeGroupNodeDTO childGroupNode = CreateGroupNodeByCategory(tree, input, childCategory, employeeCategories);
                    if (childGroupNode != null)
                        groupNode.ChildGroupNodes.Add(childGroupNode);
                }

                if (!groupNode.ChildGroupNodes.IsNullOrEmpty())
                    groupNode.AddAttestStates(groupNode.ChildGroupNodes.GetAttestStates());
            }

            return groupNode;
        }

        protected void CreateGroupNodesByAccount(ref TimeEmployeeTreeDTO tree, GenerateTimeEmployeeTreeInput input, List<AccountDTO> accounts, Dictionary<int, AccountDTO> accountsForAttestRole, int startLevel)
        {
            if (accounts.IsNullOrEmpty())
                return;

            foreach (AccountDTO account in accounts)
            {
                TimeEmployeeTreeGroupNodeDTO groupNode = CreateGroupNodeByAccount(tree, input, account, accountsForAttestRole, startLevel, input.EmployeeAuthModel.FilterIds);
                if (groupNode != null)
                    tree.AddGroupNode(groupNode);
            }
        }

        protected TimeEmployeeTreeGroupNodeDTO CreateGroupNodeByAccount(TimeEmployeeTreeDTO tree, GenerateTimeEmployeeTreeInput input, AccountDTO account, Dictionary<int, AccountDTO> accountsForAttestRole, int startLevel, List<int> filterAccountIds = null)
        {
            List<TimeEmployeeTreeNodeDTO> employeeNodes = new List<TimeEmployeeTreeNodeDTO>();

            List<AccountDTO> childAccounts = account.GetChildrens(accountsForAttestRole.Values.ToList());
            if (!account.IsAccountOrChildValidInFilter(childAccounts, filterAccountIds, out bool isAccountAbstract))
                return null;

            Dictionary<int, string> childAccountsHierarchyState = null;

            if (!isAccountAbstract)
            {
                string hierarchyId = account.HierachyId;
                childAccountsHierarchyState = childAccounts.GetHierarchyState();

                foreach (Employee employee in input.Employees)
                {
                    if (tree.Settings.ExcludeDuplicateEmployees && tree.IsEmployeeHandled(employee.EmployeeId))
                        continue;

                    List<EmployeeAccount> employeeAccounts = input.EmployeeAuthModel?.AccountRepository?.EmployeeAccounts?.GetList(employee.EmployeeId).Where(i => i.Default).ToList();
                    if (employeeAccounts.IsNullOrEmpty())
                        continue;

                    List<AccountDTO> validAccounts = employeeAccounts.GetValidAccounts(
                        employee.EmployeeId,
                        employee.GetTimeTreeStartDate(input.StartDate),
                        employee.GetTimeTreeStopDate(input.StopDate),
                        input.EmployeeAuthModel?.AccountRepository?.AllAccountInternalsDict,
                        accountsForAttestRole,
                        onlyDefaultAccounts: true);

                    if (!validAccounts.Any(a => a.ContainsHiearchy(hierarchyId)))
                        continue;

                    TimeEmployeeTreeNodeDTO employeeNode = CreateEmployeeNode(tree, employee, account.AccountId);
                    if (employeeNode != null)
                        employeeNodes.Add(employeeNode);
                }
            }

            TimeEmployeeTreeGroupNodeDTO groupNode = CreateGroupNode(tree, employeeNodes, account.AccountId, account.Name, account.AccountNr, account.AccountDim.AccountDimNr);
            if (groupNode != null && !childAccounts.IsNullOrEmpty())
            {
                foreach (AccountDTO childAccount in childAccounts)
                {
                    if (childAccountsHierarchyState != null)
                        childAccount.ApplyHierarchyState(childAccountsHierarchyState); //Current hierarchy can have been changed by GetValidAccounts() above

                    if (groupNode.ChildGroupNodes.Any(i => i.Id == childAccount.AccountId))
                        continue;
                    if (!childAccount.IsAccountInFilter(filterAccountIds))
                        continue;

                    TimeEmployeeTreeGroupNodeDTO childGroupNode = CreateGroupNodeByAccount(tree, input, childAccount, accountsForAttestRole, startLevel); //no need to send filterAccountIds to next level (filter only available on second level)
                    if (childGroupNode != null)
                        groupNode.AddGroupNode(tree, childGroupNode);
                }

                if (!groupNode.ChildGroupNodes.IsNullOrEmpty())
                    groupNode.AddAttestStates(groupNode.ChildGroupNodes.GetAttestStates());
            }

            return groupNode;
        }

        protected TimeEmployeeTreeGroupNodeDTO CreateGroupNode(TimeEmployeeTreeDTO tree, List<TimeEmployeeTreeNodeDTO> employeeNodes, int id, string name, string code = "", int type = 0, int definedSort = 0, bool isAdditional = false, bool hasEnded = false)
        {
            if (employeeNodes == null)
                employeeNodes = new List<TimeEmployeeTreeNodeDTO>();

            TimeEmployeeTreeGroupNodeDTO groupNode = new TimeEmployeeTreeGroupNodeDTO(id, name, code, type, definedSort, isAdditional, hasEnded);

            foreach (TimeEmployeeTreeNodeDTO employeeNode in employeeNodes)
            {
                TimeEmployeeTreeNodeDTO existingEmployeeNode = groupNode.GetEmployeeNode(employeeNode.EmployeeId);
                if (existingEmployeeNode == null)
                {
                    groupNode.AddEmployeeNode(tree, employeeNode);
                }
                else if (employeeNode.IsAdditional)
                {
                    foreach (int accountId in employeeNode.AdditionalOnAccountIds)
                    {
                        if (!existingEmployeeNode.AdditionalOnAccountIds.Contains(accountId))
                            existingEmployeeNode.AdditionalOnAccountIds.Add(accountId);
                    }
                }

                employeeNode.GroupId = groupNode.Id;
            }

            groupNode.SetAttestStates(employeeNodes.GetAttestStates());

            return groupNode;
        }

        protected TimeEmployeeTreeNodeDTO CreateEmployeeNode(TimeEmployeeTreeDTO tree, Employee employee, int groupId, bool isAdditional = false, int? additionalOnAccountId = null, bool hasEnded = false)
        {
            return CreateEmployeeNode(tree, employee, groupId, employee.TimeTreeEmployeeGroup, employee.TimeTreeAttestStates.ToDTOs().ToList(), isAdditional, additionalOnAccountId, hasEnded);
        }

        protected TimeEmployeeTreeNodeDTO CreateEmployeeNode(TimeEmployeeTreeDTO tree, Employee employee, int groupId, EmployeeGroup employeeGroup, List<AttestStateDTO> attestStates, bool isAdditional = false, int? additionalOnAccountId = null, bool hasEnded = false)
        {
            if (tree == null || employee == null || employeeGroup == null)
                return null;

            if (attestStates == null)
                attestStates = new List<AttestStateDTO>();

            TimeEmployeeTreeNodeDTO employeeNode = new TimeEmployeeTreeNodeDTO()
            {
                Guid = Guid.NewGuid(),
                GroupId = groupId,
                UserId = employee.UserId,
                EmployeeId = employee.EmployeeId,
                EmployeeNr = employee.EmployeeNr,
                EmployeeFirstName = employee.FirstName,
                EmployeeLastName = employee.LastName,
                EmployeeName = employee.Name,
                SocialSec = employee.SocialSec,
                EmployeeNrAndName = employee.EmployeeNrAndName,
                HibernatingText = employee.HibernatingText,
                EmployeeSex = employee.Sex,
                EmployeeEndDate = employee.EndDate,
                EmployeeGroupId = employeeGroup.EmployeeGroupId,
                AutogenTimeblocks = employeeGroup.AutogenTimeblocks,
                TimeReportType = (TermGroup_TimeReportType)employeeGroup.TimeReportType,
                IsStamping = employee.IsStamping,
                IsAdditional = isAdditional,
                HasEnded = hasEnded,
                FinalSalaryStatus = SoeEmploymentFinalSalaryStatus.None
            };

            if (tree.IsModePayrollCalculation && employee.TimeTreeEmployment != null)
                SetEmployeeFinalSalaryStatus(employee, employeeNode);

            employeeNode.SetAttestStates(attestStates);

            if (isAdditional && additionalOnAccountId.HasValue)
            {
                employeeNode.IsAdditional = true;

                if (employeeNode.AdditionalOnAccountIds == null)
                    employeeNode.AdditionalOnAccountIds = new List<int>();
                if (!employeeNode.AdditionalOnAccountIds.Contains(additionalOnAccountId.Value))
                    employeeNode.AdditionalOnAccountIds.Add(additionalOnAccountId.Value);

                string additionalPrefix = GetAdditionalPrefix();
                if (!employeeNode.EmployeeName.StartsWith(additionalPrefix))
                    employeeNode.EmployeeName = additionalPrefix + employeeNode.EmployeeName;
            }

            tree.SetEmployeeAsHandled(employeeNode);

            return employeeNode;
        }

        protected void AddAdditionalEmployeesToTreeByAccounts(CompEntities entities, ref TimeEmployeeTreeDTO tree, GenerateTimeEmployeeTreeInput input, List<int> accountIds)
        {
            if (accountIds.IsNullOrEmpty())
                return;

            var items = (from tb in entities.TimeScheduleTemplateBlock
                         where tb.Employee.ActorCompanyId == ActorCompanyId &&
                         tb.EmployeeId.HasValue &&
                         tb.AccountId.HasValue &&
                         tb.State == (int)SoeEntityState.Active &&
                         tb.Date.HasValue &&
                         tb.Date.Value >= input.StartDate &&
                         tb.Date.Value <= input.StopDate &&
                         tb.StartTime < tb.StopTime &&
                         !tb.TimeScheduleScenarioHeadId.HasValue &&
                         accountIds.Contains(tb.AccountId.Value)
                         select new
                         {
                             EmployeeId = tb.EmployeeId.Value,
                             AccountId = tb.AccountId.Value
                         }).ToList();

            if (!items.IsNullOrEmpty())
            {
                List<TimeEmployeeTreeAdditionalAccount> additionals = new List<TimeEmployeeTreeAdditionalAccount>();
                foreach (var itemsByAccount in items.GroupBy(i => i.AccountId))
                {
                    foreach (var itemsByEmployee in itemsByAccount.GroupBy(i => i.EmployeeId))
                    {
                        if (!tree.ContainsEmployeeNode(itemsByEmployee.Key, itemsByAccount.Key))
                            additionals.Add(new TimeEmployeeTreeAdditionalAccount(itemsByEmployee.Key, itemsByAccount.Key));
                    }
                }
                AddAdditionalEmployeesToTree(entities, ref tree, input, additionals);
            }
        }

        protected void AddAdditionalEmployeesToTreeByCategories(CompEntities entities, ref TimeEmployeeTreeDTO tree, GenerateTimeEmployeeTreeInput input, List<int> categoryIds)
        {
            List<CategoryAccount> categoryAccounts = base.GetCategoryAccountsFromCache(entities, CacheConfig.Company(base.ActorCompanyId));
            if (categoryAccounts.IsNullOrEmpty())
                return;

            List<TimeEmployeeTreeAdditionalCategory> additionals = new List<TimeEmployeeTreeAdditionalCategory>();

            var items = entities.GetAdditionalEmployeesBasedOnSchedule(base.ActorCompanyId, input.StartDate, input.StopDate).Where(i => categoryIds.Contains(i.CategoryId)).ToList();
            if (!items.IsNullOrEmpty())
            {
                foreach (var itemsByCategory in items.GroupBy(i => i.CategoryId))
                {
                    int categoryId = itemsByCategory.Key;

                    foreach (var item in itemsByCategory.Where(i => i.EmployeeId.HasValue))
                    {
                        if (!tree.ContainsEmployeeNode(item.EmployeeId.Value, categoryId))
                            additionals.Add(new TimeEmployeeTreeAdditionalCategory(item.EmployeeId.Value, item.AccountId, categoryId));
                    }
                }
            }

            AddAdditionalEmployeesToTree(entities, ref tree, input, additionals);
        }

        protected void AddAdditionalEmployeesToTree(CompEntities entities, ref TimeEmployeeTreeDTO tree, GenerateTimeEmployeeTreeInput input, IEnumerable<TimeEmployeeTreeAdditional> additionals)
        {
            List<TimeEmployeeTreeNodeDTO> additionalEmployeeNodes = new List<TimeEmployeeTreeNodeDTO>();

            if (!additionals.IsNullOrEmpty())
            {
                List<int> additionalEmployeeIds = additionals.Select(i => i.EmployeeId).Distinct().ToList();

                //Add already fetched employees/transactions from regular
                List<Employee> employees = input.Employees.Where(employee => additionalEmployeeIds.Contains(employee.EmployeeId)).ToList();
                List<TimePayrollTransactionTreeDTO> transactions = input.TransactionsItems?.Where(t => additionalEmployeeIds.Contains(t.EmployeeId)).ToList() ?? new List<TimePayrollTransactionTreeDTO>();

                //Remove all regular employeeIds from additionalEmployeeIds
                additionalEmployeeIds = additionalEmployeeIds.Where(id => !employees.Any(e => e.EmployeeId == id)).ToList();

                //Fetch employees/transactions for additional
                employees.AddRange(EmployeeManager.GetAllEmployeesByIds(entities, base.ActorCompanyId, additionalEmployeeIds, active: true, getVacant: false, loadEmployment: true, loadContact: true));
                transactions.AddRange(GetTimePayrollTransactionsForTreeFromCache(entities, CacheConfig.Company(base.ActorCompanyId), input.StartDate, input.StopDate, input.TimePeriod, employeeIds: additionalEmployeeIds, flushCache: true));
                Dictionary<int, List<TimePayrollTransactionTreeDTO>> additionalTransactionsByEmployee = transactions.GroupBy(i => i.EmployeeId).ToDictionary(i => i.Key, i => i.ToList());

                foreach (var employeeId in additionalEmployeeIds)
                {
                    Employee employee = employees.FirstOrDefault(i => i.EmployeeId == employeeId);
                    if (employee == null || employee.Hidden)
                        continue;

                    var additionalsForEmployee = additionals.Where(i => i.EmployeeId == employee.EmployeeId).ToList();

                    List<TimePayrollTransactionTreeDTO> additionalTransactionsForEmployee = null;
                    if (additionalTransactionsByEmployee.ContainsKey(employee.EmployeeId))
                    {
                        List<DateTime> employmentDates = employee.GetEmploymentDates(input.StartDate, input.StopDate);
                        additionalTransactionsForEmployee = additionalTransactionsByEmployee[employee.EmployeeId].Where(i => i.TimePeriodId.HasValue || employmentDates.Contains(i.Date)).ToList();
                    }

                    if (!TrySetTimeTreeEmployeeProperties(tree, employee, input.StartDate, input.StopDate, input.TimePeriod, input.AttestStates, additionalTransactionsForEmployee, out _, true, input.EmployeeGroups, input.PayrollGroups, input.HighestAttestState))
                        continue;

                    if (additionalsForEmployee.Any(item => item is TimeEmployeeTreeAdditionalCategory))
                    {
                        foreach (var additionalsByCategory in additionalsForEmployee.Select(item => item as TimeEmployeeTreeAdditionalCategory).GroupBy(i => i.CategoryId))
                        {
                            int categoryId = additionalsByCategory.Key;
                            foreach (var additional in additionalsByCategory)
                            {
                                TimeEmployeeTreeNodeDTO employeeTreeNode = CreateEmployeeNode(tree, employee, categoryId, isAdditional: true, additionalOnAccountId: additional.AccountId);
                                if (employeeTreeNode != null)
                                    additionalEmployeeNodes.Add(employeeTreeNode);
                            }
                        }
                    }
                    else if (additionalsForEmployee.Any(item => item is TimeEmployeeTreeAdditionalAccount))
                    {
                        foreach (var additionalsByAccount in additionalsForEmployee.Select(item => item as TimeEmployeeTreeAdditionalAccount).GroupBy(i => i.AccountId))
                        {
                            int accountId = additionalsByAccount.Key;
                            foreach (var additional in additionalsByAccount)
                            {
                                TimeEmployeeTreeNodeDTO employeeTreeNode = CreateEmployeeNode(tree, employee, accountId, isAdditional: true, additionalOnAccountId: additional.AccountId);
                                if (employeeTreeNode != null)
                                    additionalEmployeeNodes.Add(employeeTreeNode);
                            }
                        }
                    }
                }
            }

            if (!additionalEmployeeNodes.IsNullOrEmpty())
            {
                foreach (TimeEmployeeTreeGroupNodeDTO groupNode in tree.GroupNodes)
                {
                    groupNode.AddEmployeeNodes(tree, additionalEmployeeNodes.Where(i => i.GroupId == groupNode.Id).ToList());
                }
            }

            TimeEmployeeTreeGroupNodeDTO additionalGroupNode = CreateGroupNode(tree, additionalEmployeeNodes, Constants.ATTESTTREE_GROUP_ADDITIONAL, GetText(5584, "Inlånade"), isAdditional: true);
            if (additionalGroupNode != null)
                tree.AddGroupNode(additionalGroupNode);
        }

        protected void AddEndedEmployeesToTree(ref TimeEmployeeTreeDTO tree, List<Employee> endedEmployees)
        {
            List<TimeEmployeeTreeNodeDTO> employeeNodesInGroup = new List<TimeEmployeeTreeNodeDTO>();
            foreach (Employee employee in endedEmployees)
            {
                if (employee.TimeTreeEmployeeGroup == null)
                    continue;

                TimeEmployeeTreeNodeDTO employeeTreeNode = CreateEmployeeNode(tree, employee, Constants.ATTESTTREE_GROUP_ENDED, hasEnded: true);
                if (employeeTreeNode != null)
                    employeeNodesInGroup.Add(employeeTreeNode);
            }

            //Create ended Category
            TimeEmployeeTreeGroupNodeDTO endedGroupNode = CreateGroupNode(tree, employeeNodesInGroup, Constants.ATTESTTREE_GROUP_ENDED, GetText(8716, "Avslutade"), hasEnded: true);
            if (endedGroupNode != null)
                tree.AddGroupNode(endedGroupNode);
        }

        #endregion

        #region AttestState

        protected static List<AttestState> GetAttestStatesForEmployee<T>(List<T> transactions, SoeAttestTreeMode mode, int employeeId, List<AttestState> attestStates) where T : IPayrollTransaction
        {
            List<AttestState> validAttestStates = new List<AttestState>();

            List<T> validTransactions = null;
            if (mode == SoeAttestTreeMode.TimeAttest)
                validTransactions = transactions?.Where(t => !t.IsExcludedInTime()).ToList();
            else if (mode == SoeAttestTreeMode.PayrollCalculation)
                validTransactions = transactions?.Where(t => t.PayrollProductUseInPayroll).ToList();
            else
                return validAttestStates;

            if (!validTransactions.IsNullOrEmpty())
            {
                foreach (T transactionItem in validTransactions.Where(i => i.EmployeeId == employeeId))
                {
                    if (validAttestStates.Any(i => i.AttestStateId == transactionItem.AttestStateId))
                        continue;

                    AttestState attestState = attestStates.FirstOrDefault(i => i.AttestStateId == transactionItem.AttestStateId);
                    if (attestState != null)
                        validAttestStates.Add(attestState);
                }
            }

            return validAttestStates;
        }

        #endregion

        #region PayrollImport

        public List<PayrollImportEmployeeTransaction> GetPayrollImportTransactions(CompEntities entities, int actorCompanyId, int employeeId, DateTime startDate, DateTime stopDate)
        {
            if (employeeId <= 0 || !HasPayrollImports(entities, actorCompanyId))
                return null;

            return PayrollManager.GetPayrollImportEmployeeTransactionsForEmployee(entities, employeeId, startDate, stopDate);
        }

        public Dictionary<int, List<DateTime>> GetPayrollImportErrors(CompEntities entities, int actorCompanyId, List<int> employeeIds, DateTime startDate, DateTime stopDate, List<PayrollImportEmployeeTransaction> importEmployeeTransactions = null)
        {
            if (employeeIds == null || !HasPayrollImports(entities, actorCompanyId))
                return new Dictionary<int, List<DateTime>>();

            return PayrollManager.GetPayrollImportUnhandledOrError(entities, actorCompanyId, employeeIds, startDate, stopDate, importEmployeeTransactions: importEmployeeTransactions);
        }

        protected bool HasPayrollImports(CompEntities entities, int actorCompanyId)
        {
            return base.HasPayrollImportHeadsFromCache(entities, actorCompanyId);
        }

        #endregion

        #region Employee

        protected List<TimeTreeEmployeeInfoDTO> GetValidEmployeeInfos(List<Employee> employees, List<EmployeeGroup> employeeGroups, DateTime startDate, DateTime stopDate)
        {
            List<TimeTreeEmployeeInfoDTO> employeeInfos = new List<TimeTreeEmployeeInfoDTO>();
            if (employees != null)
            {
                foreach (Employee employee in employees)
                {
                    if (employeeInfos.Any(i => i.Employee?.EmployeeId == employee.EmployeeId))
                        continue;

                    TimeTreeEmployeeInfoDTO employeeInfo = new TimeTreeEmployeeInfoDTO(employee, employeeGroups, startDate, stopDate);
                    if (employeeInfo.IsValid())
                        employeeInfos.Add(employeeInfo);
                }
            }
            return employeeInfos;
        }

        private Employee GetEmployeeForTree(TimeEmployeeTreeDTO tree, GenerateTimeEmployeeTreeInput input, TimeEmployeeTreeNodeDTO employeeNode)
        {
            Employee employee = input.Employees.FirstOrDefault(i => i.EmployeeId == employeeNode.EmployeeId);
            if (employee == null)
                return null;

            if (employee.TimeTreeEmployment == null && tree.Settings.IncludeEnded)
                employee.TimeTreeEmployment = employee.GetLastEmployment();
            if (employee.TimeTreeEmployment == null)
                return null;

            return employee;
        }

        private List<DateTime> allDates = null;
        protected bool TrySetTimeTreeEmployeeProperties(TimeEmployeeTreeDTO tree, Employee employee, DateTime startDate, DateTime stopDate, TimePeriod timePeriod, List<AttestState> attestStates, List<TimePayrollTransactionTreeDTO> transactions, out bool employeeIsEnded, bool setHibernatingText, List<EmployeeGroup> employeeGroups = null, List<PayrollGroup> payrollGroups = null, AttestState highestAttestState = null, List<VacationYearEndRow> finalSalaries = null)
        {
            employeeIsEnded = false;

            if (this.allDates == null)
                this.allDates = CalendarUtility.GetDates(startDate, stopDate);
            List<DateTime> employmentDates = employee.GetEmploymentDates(startDate, stopDate, this.allDates);

            if (tree.IsModePayrollCalculation && timePeriod.HasPayrollDates() && timePeriod.StartDate != timePeriod.PayrollStartDate.Value && employmentDates.IsNullOrEmpty())
            {
                employee.CalculationStartDate = timePeriod.PayrollStartDate.Value;
                employee.CalculationStopDate = timePeriod.PayrollStopDate.Value;
                employmentDates = employee.GetEmploymentDates(employee.CalculationStartDate.Value, employee.CalculationStopDate.Value);
            }

            //Employment
            SetTimeTreeEmployment(tree, employee, out employeeIsEnded, employmentDates, employeeGroups, finalSalaries);
            if (employee.TimeTreeEmployment == null)
                return false;

            //EmployeeGroup
            employee.TimeTreeEmployeeGroup = employee.TimeTreeEmployment.GetEmployeeGroup(employee.GetTimeTreeStartDate(startDate), employee.GetTimeTreeStopDate(stopDate), employeeGroups, forward: false, useLastIfCurrentNotExists: tree.Settings.IncludeEnded);
            if (employee.TimeTreeEmployeeGroup == null)
                return false;

            //PayrollGroup
            if (tree.IsModePayrollCalculation && timePeriod?.TimePeriodHead != null && timePeriod.PaymentDate.HasValue)
            {
                employee.TimeTreePayrollGroup = employee.TimeTreeEmployment.GetPayrollGroup(employee.GetTimeTreeStartDate(startDate), employee.GetTimeTreeStopDate(stopDate), payrollGroups, forward: false);
                if (employee.TimeTreePayrollGroup == null || employee.TimeTreePayrollGroup.TimePeriodHeadId != timePeriod.TimePeriodHead.TimePeriodHeadId)
                    return false;
            }

            //AttestStates
            employee.TimeTreeAttestStates = GetAttestStatesForEmployee(transactions, tree.Mode, employee.EmployeeId, attestStates);
            if (tree.IsModeTimeAttest && tree.Settings.DoNotShowWithoutTransactions && employee.TimeTreeAttestStates.IsNullOrEmpty())
                return false;
            if (highestAttestState != null && !employee.TimeTreeAttestStates.IsNullOrEmpty() && !employee.TimeTreeAttestStates.Any(i => i.Sort < highestAttestState.Sort))
                return false;

            if (setHibernatingText)
                EmployeeManager.SetEmployeeHibernatingText(employee, tree.StartDate, tree.StopDate);

            return true;
        }

        protected void SetTimeTreeEmployment(TimeEmployeeTreeDTO tree, Employee employee, out bool employeeIsEnded, List<DateTime> employmentDates = null, List<EmployeeGroup> employeeGroups = null, List<VacationYearEndRow> finalSalaries = null)
        {
            employeeIsEnded = false;

            if (tree == null || employee == null)
                return;

            if (employmentDates == null)
                employmentDates = employee.GetEmploymentDates(tree.StartDate, tree.StopDate);

            employee.TimeTreeEmployment = !employmentDates.IsNullOrEmpty() ? employee.GetEmployment(employmentDates.Last()) : null;
            if (employee.TimeTreeEmployment == null && tree.Settings.IncludeEnded)
            {
                employee.TimeTreeEmployment = employee.GetLastEmployment(EmployeeManager.GetEmployeeStartDateBasedOnIncludeEnded(tree.Settings.IncludeEnded, tree.StopDate));
                if (employee.TimeTreeEmployment != null)
                    employeeIsEnded = true;
            }

            if (tree.IsModeTimeAttest && employee.TimeTreeEmployment != null)
                employee.IsStamping = employee.IsStampingInPeriod(tree.StartDate, tree.StopDate, employeeGroups);
            if (tree.IsModePayrollCalculation && tree.TimePeriod != null)
                employee.TimeTreeFinalSalaryStatus = (int?)PayrollManager.GetFinalSalaryStatus(employee, tree.TimePeriod, finalSalaries);
        }

        protected void SetEmployeeFinalSalaryStatus(Employee employee, TimeEmployeeTreeNodeDTO employeeNode)
        {
            if (employee?.TimeTreeEmployment == null || !employee.TimeTreeFinalSalaryStatus.HasValue || employeeNode == null)
                return;

            employeeNode.FinalSalaryStatus = (SoeEmploymentFinalSalaryStatus)employee.TimeTreeFinalSalaryStatus.Value;
            employeeNode.Tooltip = $"{GetText(11910, "Anställning")} {employee.TimeTreeEmployment.DateFrom?.ToString("yyyMMdd")}-{employee.TimeTreeEmployment.DateTo?.ToString("yyyMMdd")}";
            if (employeeNode.FinalSalaryStatus == SoeEmploymentFinalSalaryStatus.ApplyFinalSalary)
                employeeNode.Tooltip += $" {GetText(11911, "Skall slutavräknas").ToLower()}";
            else if (employeeNode.FinalSalaryStatus == SoeEmploymentFinalSalaryStatus.AppliedFinalSalary)
                employeeNode.Tooltip += $" {GetText(11912, "Har slutavräknats").ToLower()}";
            else if (employeeNode.FinalSalaryStatus == SoeEmploymentFinalSalaryStatus.AppliedFinalSalaryManually)
                employeeNode.Tooltip += $" {GetText(11995, "Har markerats som slutavräknad").ToLower()}";
        }

        #endregion

        #region Warnings

        protected void GenerateWarningsForTimeTree(TimeEmployeeTreeDTO tree, TimeTreeWarningsRepository warningRepository, TimePeriod timePeriod, List<EmployeeGroup> employeeGroups, List<int> employeeIds = null)
        {
            if (tree?.GroupNodes == null)
                return;

            foreach (var groupNode in tree.GroupNodes)
            {
                GenerateWarningsForTimeTreeGroupNode(tree, warningRepository, groupNode, timePeriod, employeeGroups, employeeIds);
            }

            tree.SetGroupNodeVisibility();
        }

        private void GenerateWarningsForTimeTreeGroupNode(TimeEmployeeTreeDTO tree, TimeTreeWarningsRepository warningRepository, TimeEmployeeTreeGroupNodeDTO groupNode, TimePeriod timePeriod, List<EmployeeGroup> employeeGroups, List<int> employeeIds = null)
        {
            if (tree == null || groupNode == null)
                return;

            var timeWarningsByEmployee = GetTimeWarnings(warningRepository, groupNode.GetEmployeeIdsStamping());
            var payrollWarningsByEmployee = GetPayrollWarnings(warningRepository);

            var employeeNodesToRefresh = new List<TimeEmployeeTreeNodeDTO>();
            foreach (var employeeNode in groupNode.EmployeeNodes)
            {
                if (employeeIds != null && employeeIds.Contains(employeeNode.EmployeeId))
                    employeeNode.SetWarnings(timeWarningsByEmployee.GetList(employeeNode.EmployeeId), payrollWarningsByEmployee.GetList(employeeNode.EmployeeId));
                else
                    employeeNodesToRefresh.Add(employeeNode);

            }

            groupNode.ChildGroupNodes.ForEach(childGroup => GenerateWarningsForTimeTreeGroupNode(tree, warningRepository, childGroup, timePeriod, employeeGroups, employeeIds));
            groupNode.SetWarnings(tree.Settings.WarningFilter, employeeIds);
        }

        private Dictionary<int, List<TimeTreeEmployeeWarning>> GetTimeWarnings(TimeTreeWarningsRepository warningRepository, List<int> stampingEmployeeIds)
        {
            var warningsByEmployee = new Dictionary<int, List<TimeTreeEmployeeWarning>>();

            var warnings = warningRepository.GetTimeWarnings();
            if (!warnings.IsNullOrEmpty())
            {
                foreach (var warning in warnings)
                {
                    warning.SetMessage(GetTimeWarningMessage(warning.Type, longMessage: true));

                    bool isStampingWarning = warning.Type == SoeTimeAttestWarning.TimeStampErrors || warning.Type == SoeTimeAttestWarning.TimeStampsWithoutTransactions;

                    foreach (int employeeId in warning.EmployeeIds.Where(employeeId => !isStampingWarning || stampingEmployeeIds.Contains(employeeId)))
                    {
                        if (!warningsByEmployee.ContainsKey(employeeId))
                            warningsByEmployee.Add(employeeId, new List<TimeTreeEmployeeWarning>());
                        warningsByEmployee[employeeId].Add(new TimeTreeEmployeeTimeWarning(employeeId, warning));
                    }
                }
            }

            return warningsByEmployee;
        }

        protected Dictionary<int, List<TimeTreeEmployeeWarning>> GetPayrollWarnings(TimeTreeWarningsRepository warningRepository)
        {
            var warningsByEmployee = new Dictionary<int, List<TimeTreeEmployeeWarning>>();            
            var warnings = warningRepository.GetPayrollWarnings();
            if (!warnings.IsNullOrEmpty())
            {
                foreach (var warning in warnings)
                {
                    warning.SetMessage(GetPayrollWarningMessage(warning.Type));

                    foreach (int employeeId in warning.EmployeeIds)
                    {
                        if (!warningsByEmployee.ContainsKey(employeeId))
                            warningsByEmployee.Add(employeeId, new List<TimeTreeEmployeeWarning>());
                        warningsByEmployee[employeeId].Add(new TimeTreeEmployeePayrollWarning(employeeId, warning));
                    }

                }
            }

            return warningsByEmployee;
        }

        public AttestFunctionOptionDescription GetTimeAttestFunctionOptionDescription(int option)
        {
            AttestFunctionOptionDescription result = new AttestFunctionOptionDescription();

            AttestState attestStateInitial = AttestManager.GetInitialAttestState(base.ActorCompanyId, TermGroup_AttestEntity.PayrollTime);
            string attestStateInitialName = attestStateInitial?.Name ?? "Reg";

            AttestState attestStateMinAttestStatus = AttestManager.GetAttestState(SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.SalaryExportPayrollMinimumAttestStatus, 0, base.ActorCompanyId, 0));
            string attestStateMinimumAttestStatusName = attestStateMinAttestStatus?.Name ?? "Attest";

            switch (option)
            {
                case (int)SoeTimeAttestFunctionOption.RestoreToSchedule:
                    result.Description1 = string.Format(GetText(11896, "Återställer endast dagar med attestnivå {0}. Dagar med högre attestnivå hoppas över. Hanterar anställda tillhörande tidrapporteringstyp avvikelseregistrering och stämpling olika"), attestStateInitialName);
                    result.Description2Caption = GetText(11901, "Avvikelseregistrering");
                    result.Description2 = string.Format(GetText(11897, "Tar bort avvikelser och återställer utfallet på valda dagar"), attestStateInitialName);
                    result.Description3Caption = GetText(11902, "Stämpling");
                    result.Description3 = string.Format(GetText(11898, "Tar bort utfall (inklusive planerad frånvaro) på valda dagar men behåller stämplingar samt eventuella avvikelseorsaker kopplade till stämplingarna"), attestStateInitialName);
                    break;
                case (int)SoeTimeAttestFunctionOption.RestoreToScheduleDiscardDeviations:
                    result.Description1 = string.Format(GetText(91941, "Aktiverar tomma dagar med schema. Gäller endast avvikelseregistrering"));
                    break;
                case (int)SoeTimeAttestFunctionOption.RestoreScheduleToTemplate:
                    result.Description1 = string.Format(GetText(11896, "Återställer endast dagar med attestnivå {0}. Dagar med högre attestnivå hoppas över. Hanterar anställda tillhörande tidrapporteringstyp avvikelseregistrering och stämpling olika"), attestStateInitialName);
                    result.Description2Caption = GetText(11901, "Avvikelseregistrering");
                    result.Description2 = string.Format(GetText(11899, "Återställer schemat till grundschema samt tar bort avvikelser och återställer utfallet på valda dagar"), attestStateInitialName);
                    result.Description3Caption = GetText(11902, "Stämpling");
                    result.Description3 = string.Format(GetText(11900, "Återställer schemat till grundschema samt tar bort utfall (inklusive planerad frånvaro) på valda dagar men behåller stämplingar samt eventuella avvikelseorsaker kopplade till stämplingarna"), attestStateInitialName);
                    break;
                case (int)SoeTimeAttestFunctionOption.ReGenerateDaysBasedOnTimeStamps:
                    result.Description1 = string.Format(GetText(11903, "Räknar om stämplingar på valda dagar med attestnivå {0}. Dagar med högre attestnivå räknas inte om. Likställigt med att på en dag öppna stämplingar och välja 'Spara'"), attestStateInitialName);
                    break;
                case (int)SoeTimeAttestFunctionOption.ReGenerateTransactionsDiscardAttest:
                    StringBuilder sb = new StringBuilder();
                    sb.Append(string.Format(GetText(11904, "Räknar om utfallet och behåller atteststatus på valda dagar upp till och med attestnivå {0}. Dagar med högre attestnivå räknas inte om"), attestStateMinimumAttestStatusName));
                    sb.Append(". ");
                    sb.Append(string.Format(GetText(91990, "Ska ej användas för stämplare. Då kommer istället funktionen 'Räknar om stämplingar' köras om alla transaktioner har attestnivå {0}"), attestStateInitialName));
                    result.Description1 = sb.ToString();
                    break;
                case (int)SoeTimeAttestFunctionOption.ReGenerateVacationsTransactionsDiscardAttest:
                    result.Description1 = string.Format(GetText(11905, "Räknar om utfallet och behåller atteststatus på valda dagar som innehåller semester upp till och med attestnivå {0}. Dagar med högre attestnivå räknas inte om"), attestStateMinimumAttestStatusName);
                    break;
                case (int)SoeTimeAttestFunctionOption.DeleteTimeBlocksAndTransactions:
                    result.Description1 = string.Format(GetText(11906, "Tar bort utfall på valda dagar med attestnivå {0}. Dagar med högre attestnivå hanteras inte. Rapporterade stämplingar ligger kvar på dagen, men utan utfall"), attestStateInitialName);
                    break;
                case (int)SoeTimeAttestFunctionOption.ScenarioRemoveAbsence:
                    result.Description1 = string.Format(GetText(8922, "Tar bort frånvaron på valda dagar"));
                    break;
            }

            return result;
        }

        protected AttestState GetAttestStateWarningScheduledPlacement(bool longMessage)
        {
            return new AttestState()
            {
                AttestStateId = Constants.ATTESTSTATEID_SCHEDULEPLACEMENT_DUMMY,
                Name = GetTimeWarningMessage(SoeTimeAttestWarning.PlacementIsScheduled, longMessage),
                Color = "#FFFFFF",
            };
        }

        #endregion

        #region Messages

        private string GetAdditionalPrefix()
        {
            return String.Format("({0}) ", GetText(5583, "Inlånad"));
        }

        protected string GetAttestForEmployeeValidMessage(int total, List<AttestEmployeeDayDTO> additionDeductionItems, AttestStateDTO attestState)
        {
            if (total <= 0 || attestState == null)
                return String.Empty;

            StringBuilder message = new StringBuilder();
            message.Append($"{total} {GetText(10077, "dagar kommer att få attestnivå")} {attestState.Name}. \r\n");

            //Addition/Deduction
            if (!additionDeductionItems.IsNullOrEmpty())
            {
                var additionDeductionItemsWithDifferentAttestState = additionDeductionItems.Where(i => i.AttestPayrollTransactions.Any(t => t.AttestStateId != attestState.AttestStateId && t.IsAdditionOrDeduction)).ToList();
                if (additionDeductionItemsWithDifferentAttestState.Any())
                {
                    message.Append($"{additionDeductionItemsWithDifferentAttestState.Count} {GetText(10078, "dagar har resa/utlägg transaktioner som kommer att få attestnivå")} {attestState.Name}. \r\n");
                    additionDeductionItemsWithDifferentAttestState.ForEach(item => message.Append($"\t - {item.Date.ToShortDateString()} \r\n"));
                }
            }

            return message.ToString();
        }

        protected string GetAttestForEmployeePreliminaryMessage(int total, AttestStateDTO attestState)
        {
            if (total <= 0 || attestState == null)
                return String.Empty;
            return $"{total} {GetText(10080, "dagar är preliminära och kan ej ändras till")} {attestState.Name}. \r\n";
        }

        protected string GetAttestForEmployeeNoTransitionMessage(int total, AttestStateDTO attestState)
        {
            if (total <= 0 || attestState == null)
                return String.Empty;
            return $"{total} {GetText(11940, "dagar kan inte få attestnivå")} {attestState.Name} {GetText(11938, "för att giltig attestövergång saknas")}. \r\n";
        }

        protected string GetAttestForEmployeeLockedMessage(int total, AttestStateDTO attestState)
        {
            if (total <= 0 || attestState == null)
                return String.Empty;
            return $"{total} {GetText(11940, "dagar kan inte få attestnivå")} {attestState.Name} {GetText(11939, "för att de är låsta eller exporterade")}. \r\n";
        }

        protected string GetAttestForEmployeeUnauthMessage(int total, AttestStateDTO attestState)
        {
            if (total <= 0 || attestState == null)
                return String.Empty;
            return $"{total} {GetText(10128, "dagar har transaktioner som har kontering på annan tillhörighet och kommer inte få attestnivå")} {attestState.Name}. \r\n";
        }

        protected string GetAttestForEmployeeInvalidStampingStatusMessage(int total, AttestStateDTO attestState)
        {
            if (total <= 0 || attestState == null)
                return String.Empty;
            return $"{total} {GetText(91944, "dagar har felaktiga stämplingar och kommer inte få attestnivå")} {attestState.Name}. \r\n";
        }

        protected string GetAttestForTransactionsValidMessage(int total, AttestStateDTO attestState)
        {
            if (total <= 0 || attestState == null)
                return String.Empty;
            return $"{total} {GetText(10093, "transaktioner kommer att få attestnivå")} {attestState.Name}. \r\n";
        }

        protected string GetAttestForTransactionsPreliminaryMessage(int total, AttestStateDTO attestState)
        {
            if (total <= 0 || attestState == null)
                return String.Empty;
            return $"{total} {GetText(10095, "transaktioner är preliminära och kan ej ändras till")} {attestState.Name}. \r\n";
        }

        protected string GetAttestForTransactionNoTransitionMessage(int total, AttestStateDTO attestState)
        {
            if (total <= 0 || attestState == null)
                return String.Empty;
            return $"{total} {GetText(11937, "transaktioner kan inte få attestnivå")} {attestState.Name} {GetText(11938, "för att giltig attestövergång saknas")}. \r\n";
        }

        protected string GetAttestForTransactionsLockedMessage(int total, AttestStateDTO attestState)
        {
            if (total <= 0 || attestState == null)
                return String.Empty;
            return $"{total} {GetText(11937, "transaktioner kan inte få attestnivå")} {attestState.Name} {GetText(11939, "för att dom är låsta eller exporterade")}. \r\n";
        }
        public string GetInfoMessage(List<SoeTimeAttestInformation> infos) 
        {
            if (infos.IsNullOrEmpty())
                return string.Empty;

            StringBuilder sb = new StringBuilder();

            foreach (SoeTimeAttestInformation info in infos.Distinct()) 
            {
                if (sb.Length > 0)
                    sb.Append(".\n");

                sb.Append(GetTimeInfoMessage(info));
            }

            return sb.ToString();
        }

        public string GetWarningMessage(List<SoeTimeAttestWarning> warnings)
        {
            if (warnings.IsNullOrEmpty())
                return string.Empty;

            StringBuilder sb = new StringBuilder();

            foreach (SoeTimeAttestWarning warning in warnings.Distinct())
            {
                if (sb.Length > 0)
                    sb.Append(".\n");

                sb.Append(GetTimeWarningMessage(warning, longMessage: true));
            }

            return sb.ToString();
        }
        protected string GetTimeInfoMessage(SoeTimeAttestInformation info) 
        {
            string message = "";
            switch (info)
            {
                case SoeTimeAttestInformation.HasShiftSwaps:
                    message = GetText(8958, "Har godkända passbyten");
                    break;
            }
            return message;
        }
        protected string GetTimeWarningMessage(SoeTimeAttestWarning warning, bool longMessage = false)
        {
            string message = "";

            switch (warning)
            {
                case SoeTimeAttestWarning.ScheduleIsChangedFromTemplate:
                    message = GetText(11930, "Aktivt schema har ändrats från grundschemat");
                    break;
                case SoeTimeAttestWarning.ScheduleWithoutTransactions:
                    message = GetText(5577, "Det finns schemalagda dagar utan transaktioner");
                    break;
                case SoeTimeAttestWarning.PlacementIsScheduled:
                    message = longMessage ? GetText(11868, "Aktivering schemalagd") : GetText(11909, "Schemalagd");
                    break;
                case SoeTimeAttestWarning.TimeScheduleTypeFactorMinutes:
                    message = GetText(11928, "Aktiverade schema har räknats upp med faktor på schematyp");
                    break;
                case SoeTimeAttestWarning.DiscardedBreakEvaluation:
                    message = GetText(11929, "Dagen har raster som ej är räknade enligt rastregelverk");
                    break;
                case SoeTimeAttestWarning.TimeStampsWithoutTransactions:
                    message = GetText(5576, "Det finns stämplingar som ej genererat transaktioner");
                    break;
                case SoeTimeAttestWarning.TimeStampErrors:
                    message = GetText(11786, "Det finns dagar som innehåller felaktiga stämplingar");
                    break;
                case SoeTimeAttestWarning.ContainsDuplicateTimeBlocks:
                    message = GetText(9961, "Dagen innehåller dubbla närvarotider");
                    break;
                case SoeTimeAttestWarning.PayrollImport:
                    message = GetText(12108, "Det finns obehandlat underlag från försystem");
                    break;
            }

            return message;
        }

        protected string GetPayrollWarningMessage(TermGroup_PayrollControlFunctionType warning)
        {
            return base.GetText((int)warning, (int)TermGroup.PayrollControlFunctionType);
        }

        private string _attestStateNameAllMessage;
        protected string GetAttestStateAllMessage() => _attestStateNameAllMessage ?? (_attestStateNameAllMessage = GetText(4366, "Alla"));

        private string _attestStateNameMissingMessage;
        protected string GetAttestStateMissingMessage() => _attestStateNameMissingMessage ?? (_attestStateNameMissingMessage = GetText(91965, "Saknar atteststatus"));

        private string _payrollGroupMissingMessage;
        protected string GetPayrolGroupMissingMessage() => _payrollGroupMissingMessage ?? (_payrollGroupMissingMessage = GetText(11950, "Saknar löneavtal"));

        #endregion
    }
}


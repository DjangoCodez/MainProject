using SoftOne.Soe.Business.Core.SoftOneId;
using SoftOne.Soe.Business.Core.SysService;
using SoftOne.Soe.Business.DataCache;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.DTO.SoftOneId;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core.Objects.DataClasses;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Transactions;

namespace SoftOne.Soe.Business.Core
{
	public class UserManager : ManagerBase
	{
		#region Variables

		// Create a logger for use in this class
		private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		#endregion

		#region Ctor

		public UserManager(ParameterObject parameterObject) : base(parameterObject) { }

		#endregion

		#region User

		public List<User> FilterUsersByAccountHierarchy(
			CompEntities entities,
			int actorCompanyId,
			int roleId,
			int userId,
			List<User> users,
			List<int> includeUserIds = null,
			DateTime? startDate = null,
			DateTime? stopDate = null,
			bool skipNonEmployeeUsers = false,
			bool includeEmployeesWithSameAccountOnAttestRole = false,
			bool includeEmployeesWithSameAccount = false,
			bool includeEnded = false,
			bool includeNotStarted = false
			)
		{
			if (users.IsNullOrEmpty() || !base.UseAccountHierarchyOnCompanyFromCache(entities, actorCompanyId))
				return users;

			// Håkan 2019-03-06: Ugly Axfood workaround for seeing users not connected to any employee
			// Butikschef or Teamchef should not se them, only higher roles. Lower roles does not have permission to the user grid.
			// Don't blame me, blame Rickard for the solution :-)
			// Niclas 2024-01-18: Still ugly solution but now Säljchef is also included...
			// Still blame Rickard :-)
			Role role = RoleManager.GetRole(entities, roleId, actorCompanyId);
			if (role != null && (role.Name == "Butikschef" || role.Name == "Teamchef" || role.Name == "Säljchef"))
				skipNonEmployeeUsers = true;

			List<Employee> allEmployees = EmployeeManager.GetAllEmployeesWithUser(entities, actorCompanyId);
			List<Employee> validEmployees = EmployeeManager.GetValidEmployeeByAccountHierarchy(
				entities,
				actorCompanyId,
				roleId,
				userId,
				allEmployees,
				EmployeeManager.GetEmployeeByUser(entities, actorCompanyId, userId),
				startDate ?? DateTime.Today,
				stopDate ?? DateTime.Today,
				useShowOtherEmployeesPermission: true,
				onlyDefaultAccounts: false,
				includeEmployeesWithSameAccountOnAttestRole: includeEmployeesWithSameAccountOnAttestRole,
				includeEmployeesWithSameAccount: includeEmployeesWithSameAccount,
				includeEnded: includeEnded,
				includeNotStarted: includeNotStarted);

			List<int> allEmployeeUserIds = allEmployees.Select(e => e.UserId.Value).ToList();
			List<int> validEmployeeUserIds = validEmployees.Select(e => e.UserId.Value).ToList();
			if (includeUserIds != null)
				validEmployeeUserIds.AddRange(includeUserIds);

			return users.Where(user => IsValid(user)).ToList();

			bool IsValid(User user)
			{
				if (user == null)
					return false;
				if (!skipNonEmployeeUsers && !allEmployeeUserIds.Contains(user.UserId))
					return true;

				return validEmployeeUserIds.Contains(user.UserId);
			}
		}

		public List<User> GetUsers(List<int> userIds, bool? active = true)
		{
			using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
			return GetUsers(entities, userIds, active);
		}

		public List<User> GetUsers(CompEntities entities, List<int> userIds, bool? active = true)
		{
			IQueryable<User> query = (from u in entities.User
									  where userIds.Contains(u.UserId) &&
									  u.State != (int)SoeEntityState.Deleted
									  select u);
			if (active == true)
				query = query.Where(i => i.State == (int)SoeEntityState.Active);

			return query.ToList();
		}

		public List<User> GetUsersByLicense(int licenseId, int actorCompanyId, int roleId, int userId, bool setDefaultRoleName = false, bool? active = true, bool includeEnded = false, bool includeNotStarted = false, bool excludeDemoOnlyUsers = false)
		{
			using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
			entities.User.NoTracking();
			return GetUsersByLicense(entities, licenseId, actorCompanyId, roleId, userId, setDefaultRoleName, active, includeEnded, includeNotStarted, excludeDemoOnlyUsers);
		}

		public List<User> GetUsersByLicense(CompEntities entities, int licenseId, int actorCompanyId, int roleId, int userId, bool setDefaultRoleName = false, bool? active = true, bool includeEnded = false, bool includeNotStarted = false, bool excludeDemoOnlyUsers = false)
		{
			bool isSupportLicense = licenseId != base.LicenseId && LicenseManager.GetLicense(entities, base.LicenseId)?.Support == true;
			if (!isSupportLicense && (GetUser(entities, base.UserId)?.LicenseId != licenseId || !UserValidOnCompany(entities, actorCompanyId, base.UserId)))
				return new List<User>();

			DateTime startDate = EmployeeManager.GetEmployeeStartDateBasedOnIncludeEnded(includeEnded);
			DateTime stopDate = EmployeeManager.GetEmployeeStopDateBasedOnIncludeNotStarted(includeNotStarted);

			List<User> users = (
				excludeDemoOnlyUsers
					? entities.UserCompanyRole
						.Include("User")
						.Where(u => u.Company.LicenseId == licenseId && !u.Company.Demo && u.Company.State == (int)SoeEntityState.Active)
						.Select(ucr => ucr.User)
						.Distinct()
					: entities.User
						.Where(u => u.LicenseId == licenseId && u.State != (int)SoeEntityState.Deleted)
			).ToList(startDate, stopDate, active);

			if (!isSupportLicense)
			{
				users = FilterUsersByAccountHierarchy(
					entities,
					actorCompanyId,
					roleId,
					userId,
					users,
					startDate: startDate,
					stopDate: stopDate
					);
			}
			if (users.IsNullOrEmpty())
				return new List<User>();

			if (setDefaultRoleName)
				SetDefaultRoleName(entities, users);

			return users.OrderByLogin();
		}

		public List<User> GetUsersByCompany(int actorCompanyId, int roleId, int userId, bool setDefaultRoleName = false, bool? active = true, bool includeEnded = false, bool includeNotStarted = false, bool skipNonEmployeeUsers = false, bool includeEmployeesWithSameAccountOnAttestRole = false, bool includeEmployeesWithSameAccount = false, DateTime? userCompanyRoleDate = null, bool setEmployeeCategories = false, bool setSoftOneIdLoginName = false)
		{
			using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
			entities.UserCompanyRole.NoTracking();
			return GetUsersByCompany(entities, actorCompanyId, roleId, userId, setDefaultRoleName, active, includeEnded, includeNotStarted, skipNonEmployeeUsers, includeEmployeesWithSameAccountOnAttestRole, includeEmployeesWithSameAccount, userCompanyRoleDate, setEmployeeCategories, setSoftOneIdLoginName);
		}

		public List<User> GetUsersByCompany(CompEntities entities, int actorCompanyId, int roleId, int userId, bool setDefaultRoleName = false, bool? active = true, bool includeEnded = false, bool includeNotStarted = false, bool skipNonEmployeeUsers = false, bool includeEmployeesWithSameAccountOnAttestRole = false, bool includeEmployeesWithSameAccount = false, DateTime? userCompanyRoleDate = null, bool setEmployeeCategories = false, bool setSoftOneIdLoginName = false)
		{
			Company company = CompanyManager.GetCompany(entities, actorCompanyId);
			bool isSupportLicense = company?.LicenseId != base.LicenseId && LicenseManager.GetLicense(entities, base.LicenseId)?.Support == true;
			if (!isSupportLicense && !UserValidOnCompany(entities, actorCompanyId, base.UserId))
				return new List<User>();

			DateTime startDate = userCompanyRoleDate ?? EmployeeManager.GetEmployeeStartDateBasedOnIncludeEnded(includeEnded);
			DateTime stopDate = EmployeeManager.GetEmployeeStopDateBasedOnIncludeNotStarted(includeNotStarted);

			List<User> users = entities.UserCompanyRole
				.Where(ucr => ucr.Company.ActorCompanyId == actorCompanyId && ucr.User.LicenseId == company.LicenseId)
				.ToList(startDate, stopDate, active);

			if (!isSupportLicense)
			{
				users = FilterUsersByAccountHierarchy(
					entities,
					actorCompanyId,
					roleId,
					userId,
					users: users,
					startDate: startDate,
					stopDate: stopDate,
					skipNonEmployeeUsers: skipNonEmployeeUsers,
					includeEmployeesWithSameAccountOnAttestRole: includeEmployeesWithSameAccountOnAttestRole,
					includeEmployeesWithSameAccount: includeEmployeesWithSameAccount
					);
			}
			if (users.IsNullOrEmpty())
				return new List<User>();

			if (setDefaultRoleName)
				SetDefaultRoleName(entities, users);
			if (setEmployeeCategories)
				SetUserEmployeeCategories(entities, users, actorCompanyId);
			SetUserExternalAuthId(company.LicenseId, users);
			if (setSoftOneIdLoginName)
				SetUserSoftOneIdLoginName(company.LicenseId, users);

			return users.OrderByLogin();
		}

		public List<User> GetUsersByRole(int actorCompanyId, int roleId, int userId, bool setDefaultRoleName = false, bool? active = true, bool includeEnded = false, bool includeNotStarted = false, bool filterUsersByAccountHierarchy = true)
		{
			using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
			entities.UserCompanyRole.NoTracking();
			return GetUsersByRole(entities, actorCompanyId, roleId, userId, setDefaultRoleName, active, includeEnded, includeNotStarted, filterUsersByAccountHierarchy);
		}

		public List<User> GetUsersByRole(CompEntities entities, int actorCompanyId, int roleId, int userId, bool setDefaultRoleName = false, bool? active = true, bool includeEnded = false, bool includeNotStarted = false, bool filterUsersByAccountHierarchy = true, bool noUserValidation = false)
		{
			Company company = CompanyManager.GetCompany(entities, actorCompanyId);
			bool isSupportLicense = company?.LicenseId != base.LicenseId && LicenseManager.GetLicense(entities, base.LicenseId)?.Support == true;

			DateTime startDate = EmployeeManager.GetEmployeeStartDateBasedOnIncludeEnded(includeEnded);
			DateTime stopDate = EmployeeManager.GetEmployeeStopDateBasedOnIncludeNotStarted(includeNotStarted);

			List<User> users = entities.UserCompanyRole.Where(ucr => ucr.Role.RoleId == roleId).ToList(startDate, stopDate, active);
			if (filterUsersByAccountHierarchy && !isSupportLicense && !noUserValidation)
			{
				users = FilterUsersByAccountHierarchy(
					entities,
					actorCompanyId,
					roleId,
					userId,
					users: users,
					startDate: startDate,
					stopDate: stopDate
					);
			}
			if (users.IsNullOrEmpty())
				return new List<User>();

			if (setDefaultRoleName)
				SetDefaultRoleName(entities, users);

			return users.OrderByLogin();
		}

		public List<User> GetUsersByEmployeeGroup(int actorCompanyId, int roleId, int userId, int employeeGroupId)
		{
			Employee currentEmployee = null;
			using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
			bool useAccountHierarchy = base.UseAccountHierarchyOnCompanyFromCache(entities, actorCompanyId);
			if (useAccountHierarchy)
				currentEmployee = EmployeeManager.GetEmployeeByUser(actorCompanyId, userId, loadEmployeeAccount: true);

			return GetUsersByEmployeeGroup(entities, actorCompanyId, roleId, userId, employeeGroupId, useAccountHierarchy, currentEmployee);
		}

		public List<User> GetUsersByEmployeeGroup(CompEntities entities, int actorCompanyId, int roleId, int userId, int employeeGroupId, bool useAccountHierarchy, Employee currentEmployee, bool noUserValidation = false)
		{
			List<Employee> employeesByGroup = EmployeeManager.GetAllEmployeesByGroup(entities, actorCompanyId, employeeGroupId);

			if (useAccountHierarchy && !noUserValidation)
			{
				List<int> employeeIds = EmployeeManager.GetValidEmployeeByAccountHierarchy(entities, actorCompanyId, roleId, userId, employeesByGroup.Select(e => e.EmployeeId).ToList(), currentEmployee, DateTime.Today, DateTime.Today, onlyDefaultAccounts: false);
				employeesByGroup = employeesByGroup.Where(e => employeeIds.Contains(e.EmployeeId)).ToList();
			}

			List<User> result = employeesByGroup.Where(e => e.User != null && e.User.State == (int)SoeEntityState.Active && e.User.UserId != userId).Select(e => e.User).ToList();

			return result;
		}

		public List<User> GetUsersByCategory(int actorCompanyId, int roleId, int userId, int categoryId)
		{
			Employee currentEmployee = null;
			using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
			bool useAccountHierarchy = base.UseAccountHierarchyOnCompanyFromCache(entities, actorCompanyId);
			if (useAccountHierarchy)
				currentEmployee = EmployeeManager.GetEmployeeByUser(actorCompanyId, userId, loadEmployeeAccount: true);

			return GetUsersByCategory(entities, actorCompanyId, roleId, userId, categoryId, useAccountHierarchy, currentEmployee);
		}

		public List<User> GetUsersByCategory(CompEntities entities, int actorCompanyId, int roleId, int userId, int categoryId, bool useAccountHierarchy, Employee currentEmployee, bool noUserValidation = false)
		{
			List<Employee> employeesByCategory = EmployeeManager.GetEmployeesByCategory(entities, categoryId, actorCompanyId, DateTime.Today, DateTime.Today);

			if (useAccountHierarchy && !noUserValidation)
			{
				List<int> employeeIds = EmployeeManager.GetValidEmployeeByAccountHierarchy(entities, actorCompanyId, roleId, userId, employeesByCategory.Select(e => e.EmployeeId).ToList(), currentEmployee, DateTime.Today, DateTime.Today, onlyDefaultAccounts: false);
				employeesByCategory = employeesByCategory.Where(e => employeeIds.Contains(e.EmployeeId)).ToList();
			}

			List<User> result = employeesByCategory.Where(e => e.User != null && e.User.State == (int)SoeEntityState.Active && e.User.UserId != userId).Select(e => e.User).ToList();

			return result;
		}

		public List<User> GetUsersByAccount(int actorCompanyId, int accountId, DateTime? date = null)
		{
			using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
			return GetUsersByAccount(entities, actorCompanyId, accountId, date);
		}

		public List<User> GetUsersByAccount(CompEntities entities, int actorCompanyId, int accountId, DateTime? date = null)
		{
			if (!date.HasValue)
				date = DateTime.Today;

			return (from e in entities.Employee
					where e.ActorCompanyId == actorCompanyId &&
					e.State == (int)SoeEntityState.Active &&
					e.User.State == (int)SoeEntityState.Active &&
					e.EmployeeAccount.Where(ea => ea.AccountId == accountId && ea.State == (int)SoeEntityState.Active && ea.AccountId.HasValue && ea.DateFrom <= date.Value && (!ea.DateTo.HasValue || ea.DateTo >= date.Value)).Select(ea => ea.AccountId.Value).Any()
					select e.User).ToList();
		}

		public List<User> GetUsersByAccounts(int actorCompanyId, List<int> accountIds, DateTime? date = null)
		{
			using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
			return GetUsersByAccounts(entities, actorCompanyId, accountIds, date);
		}

		public List<User> GetUsersByAccounts(CompEntities entities, int actorCompanyId, List<int> accountIds, DateTime? date = null)
		{
			int employeeAccountDimId = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.DefaultEmployeeAccountDimEmployee, 0, actorCompanyId, 0);

			if (accountIds.Count == 1)
			{
				var account = AccountManager.GetAccount(entities, actorCompanyId, accountIds.First());
				if (account == null)
					return new List<User>();

				if (account.AccountDimId == employeeAccountDimId)
					return UserManager.GetUsersByAccount(entities, actorCompanyId, accountIds.First());
			}

			List<Account> childrenAccounts = new List<Account>();
			foreach (var accountId in accountIds)
			{
				childrenAccounts.AddRange(AccountManager.GetAllChildrenAccounts(entities, actorCompanyId, accountId));
			}

			childrenAccounts = childrenAccounts.Where(w => w.AccountDimId == employeeAccountDimId).ToList();
			accountIds = childrenAccounts.Select(s => s.AccountId).ToList();

			if (!date.HasValue)
				date = DateTime.Today;

			List<User> users = new List<User>();
			foreach (var id in accountIds)
			{
				users.AddRange(GetUsersByAccount(entities, actorCompanyId, id, date));
			}

			return users.Distinct().ToList();
		}

		public List<User> GetUsersBySearch(int actorCompanyId, string searchValue, DateTime? date = null)
		{
			if (string.IsNullOrEmpty(searchValue))
				return new List<User>();

			if (!date.HasValue)
				date = DateTime.Today;

			using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
			entities.UserCompanyRole.NoTracking();
			return (from ucr in entities.UserCompanyRole
					where ((ucr.Company.ActorCompanyId == actorCompanyId) &&
					ucr.State == (int)SoeEntityState.Active &&
					(!ucr.DateFrom.HasValue || ucr.DateFrom <= date) &&
					(!ucr.DateTo.HasValue || ucr.DateTo >= date) &&
					(ucr.User.Name.ToLower().Contains(searchValue) || ucr.User.LoginName.ToLower().Contains(searchValue)) &&
					(ucr.User.State == (int)SoeEntityState.Active))
					select ucr.User).Distinct().ToList();
		}

		public List<User> GetUsersByName(string name)
		{
			if (string.IsNullOrEmpty(name))
				return new List<User>();

			name = name.ToLower();

			using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
			entities.UserCompanyRole.NoTracking();
			return (from u in entities.User
					where (u.Name.ToLower().Contains(name)) &&
					(u.State == (int)SoeEntityState.Active)
					select u).ToList();
		}

		public List<int> GetUserIdsByEmployeesIds(int actorCompanyId, List<int> employeeIds)
		{
			if (employeeIds.IsNullOrEmpty())
				return new List<int>();

			using var entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
			return (from e in entitiesReadOnly.Employee
					where e.ActorCompanyId == actorCompanyId &&
					employeeIds.Contains(e.EmployeeId) &&
					e.UserId.HasValue
					select e.UserId.Value).ToList();
		}

		public List<int> GetUserIdsByAttestRoleUserAccountIds(CompEntities entities, List<int> accountIds, DateTime? date = null)
		{
			if (accountIds.IsNullOrEmpty())
				return new List<int>();

			if (!date.HasValue)
				date = DateTime.Today;

			return (from a in entities.AttestRoleUser
					where a.State == (int)SoeEntityState.Active &&
					a.AccountId.HasValue &&
					accountIds.Contains(a.AccountId.Value) &&
					(!a.DateFrom.HasValue || a.DateFrom <= date.Value) &&
					(!a.DateTo.HasValue || a.DateTo >= date.Value)
					select a.UserId).ToList();
		}

		public List<UserDTO> GetUsersMissingEmail(int? licenseId)
		{
			DateTime date = DateTime.Now.AddDays(-90);

			using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
			entitiesReadOnly.User.NoTracking();
			if (licenseId.HasValue)
				return entitiesReadOnly.User.Where(w => string.IsNullOrEmpty(w.Email) && w.LicenseId == licenseId.Value && w.UserSession.Any(u => u.Login > date && u.MobileLogin) && w.State == (int)SoeEntityState.Active).ToDTOs().ToList();
			else
				return entitiesReadOnly.User.Where(w => string.IsNullOrEmpty(w.Email) && w.UserSession.Any(u => u.Login > date && u.MobileLogin) && w.State == (int)SoeEntityState.Active).ToDTOs().ToList();
		}

		public List<User> GetUsersWithoutEmployees(int licenseId, int actorCompanyId, int? includeUserId)
		{
			using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
			entities.UserCompanyRole.NoTracking();
			return (from ucr in entities.UserCompanyRole
					where (ucr.User.LicenseId == licenseId) &&
					(!ucr.User.Employee.Any(i => i.ActorCompanyId == actorCompanyId) || (includeUserId.HasValue && ucr.User.UserId == includeUserId.Value)) &&
					(ucr.User.State == (int)SoeEntityState.Active)
					select ucr.User).Distinct().OrderBy(u => u.LoginName).ToList();
		}

		public List<UserDTO> GetExecutiveForAccount(CompEntities entities, int actorCompanyId, List<int> accountIds, DateTime startDate, DateTime stopDate)
		{
			bool useAccountHierarchy = base.UseAccountHierarchyOnCompanyFromCache(entities, actorCompanyId);
			return useAccountHierarchy ? AttestManager.GetAttestRoleUsersForEmployeeAccounts(entities, actorCompanyId, accountIds, startDate, stopDate, onlyExecutive: true).Select(s => s.User).ToList() : new List<UserDTO>();
		}

		public List<UserDTO> GetEmployeeNearestExecutives(Employee employee, DateTime startDate, DateTime stopDate, int actorCompanyId, bool onlyExecutive = true)
		{
			using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
			return GetEmployeeNearestExecutives(entities, employee, startDate, stopDate, actorCompanyId, onlyExecutive: onlyExecutive);
		}

		public List<UserDTO> GetEmployeeNearestExecutives(CompEntities entities, Employee employee, DateTime startDate, DateTime stopDate, int actorCompanyId, List<EmployeeAccount> employeeAccounts = null, bool onlyExecutive = true, bool? onlyDefault = null, bool? onlyMainAllocation = null)
		{
			return GetEmployeeExtendedNearestExecutives(entities, employee, startDate, stopDate, actorCompanyId, employeeAccounts, onlyExecutive, onlyDefault, onlyMainAllocation).DistinctBy(d => d.User.UserId).Select(s => s.User).ToList();
		}

		public List<AttestRoleExtendedUserDTO> GetEmployeeExtendedNearestExecutivesBasedOnAttestRole(CompEntities entities, Employee employee, DateTime startDate, DateTime stopDate, int actorCompanyId, List<EmployeeAccount> employeeAccounts = null)
		{
			var users = GetEmployeeExtendedNearestExecutives(entities, employee, startDate, stopDate, actorCompanyId, employeeAccounts, onlyExecutive: true);

			var useHierachy = base.UseAccountHierarchyOnCompanyFromCache(entities, actorCompanyId);
			var roles = base.GetRolesFromCache(entities, CacheConfig.Company(actorCompanyId));
			List<UserCompanyRoleDTO> companyUserRolesOnUsers = new List<UserCompanyRoleDTO>();

			var attestRoles = base.GetTimeAttestRolesFromCache(entities, CacheConfig.Company(actorCompanyId));
			var validatedAttestUsers = new List<AttestRoleExtendedUserDTO>();
			if (attestRoles.Any(a => a.Sort > 0))
			{
				validatedAttestUsers = NearestExcutivesbasedOnAttestRole(entities, actorCompanyId, users, employee, startDate, stopDate, employeeAccounts, useHierachy, companyUserRolesOnUsers, true);

				if (validatedAttestUsers.Any())
					users = validatedAttestUsers;
			}

			return users;

		}
		public List<AttestRoleExtendedUserDTO> GetEmployeeExtendedNearestExecutives(CompEntities entities, Employee employee, DateTime startDate, DateTime stopDate, int actorCompanyId, List<EmployeeAccount> employeeAccounts = null, bool onlyExecutive = true, bool? onlyDefault = null, bool? onlyMainAllocation = null)
		{
			List<AttestRoleExtendedUserDTO> users = new List<AttestRoleExtendedUserDTO>();
			if (employee == null)
				return users;

			bool useAccountHierarchy = base.UseAccountHierarchyOnCompanyFromCache(entities, actorCompanyId);
			if (useAccountHierarchy)
			{
				onlyDefault = onlyDefault ?? base.IsTele2(entities, actorCompanyId);
				onlyMainAllocation = onlyMainAllocation ?? false;
				employeeAccounts = employeeAccounts == null ? EmployeeManager.GetEmployeeAccounts(entities, employee.ActorCompanyId, employee.EmployeeId) : employeeAccounts.Where(w => w.EmployeeId == employee.EmployeeId).ToList();

				if (!employeeAccounts.IsNullOrEmpty())
				{
					employeeAccounts = employeeAccounts.Where(w => !w.ParentEmployeeAccountId.HasValue).ToList();
					if (employeeAccounts.Any())
					{
						if (onlyDefault.Value)
							employeeAccounts = employeeAccounts.Where(w => w.Default).ToList();

						if (onlyMainAllocation.Value)
							employeeAccounts = employeeAccounts.Where(w => w.MainAllocation).ToList();

						if (!employeeAccounts.Any())
							return users;

						List<int> employeeAccountIds = employeeAccounts.Where(w => w.AccountId.HasValue && EmployeeManager.IsEmployeeAccountValid(w, startDate, stopDate)).Select(s => s.AccountId.Value).ToList();
						List<int> connectedAccountIds = new List<int>();
						foreach (var accountId in employeeAccountIds)
							connectedAccountIds.AddRange(AccountManager.GetAccountInternalAndParents(entities, accountId, actorCompanyId).Select(x => x.AccountId));

						foreach (var child in employeeAccounts.Where(w => !w.Children.IsNullOrEmpty()))
						{
							connectedAccountIds.Add(child.AccountId ?? 0);
							if (!child.Children.IsNullOrEmpty())
							{
								foreach (var childChild in child.Children)
								{
									connectedAccountIds.Add(childChild.AccountId ?? 0);

									if (!childChild.Children.IsNullOrEmpty())
									{
										foreach (var childChildChild in childChild.Children)
										{
											connectedAccountIds.Add(childChildChild.AccountId ?? 0);
										}
									}
								}
							}
						}

						connectedAccountIds = connectedAccountIds.Where(w => w != 0).Distinct().ToList();
						users = AttestManager.GetAttestRoleUsersForEmployeeAccounts(entities, actorCompanyId, connectedAccountIds, startDate, stopDate, onlyExecutive: onlyExecutive);
					}
				}

				return users;
			}
			else
			{
				List<CompanyCategoryRecord> categoryRecordsAttestRole = CategoryManager.GetCompanyCategoryRecords(entities, SoeCategoryType.Employee, SoeCategoryRecordEntity.AttestRole, actorCompanyId);
				List<Category> defaultCategories = CategoryManager.GetCategories(entities, SoeCategoryType.Employee, SoeCategoryRecordEntity.Employee, employee.EmployeeId, actorCompanyId, startDate, stopDate, onlyExecutive: false);
				foreach (Category defaultCategory in defaultCategories)
				{
					users.AddRange(AttestManager.GetAttestRoleUsersForCategory(entities, categoryRecordsAttestRole, new List<CompanyCategoryRecord>(), defaultCategory.CategoryId, actorCompanyId, startDate, stopDate, SoeModule.Time, onlyExecutive: true).ToDTOs().Select(s => new AttestRoleExtendedUserDTO() { User = s }));
				}

				return users.DistinctBy(x => x.User.UserId).ToList();
			}
		}

		public UserDTO GetEmployeeNearestExecutive(CompEntities entities, Employee employee, DateTime startDate, DateTime stopDate, int actorCompanyId, List<EmployeeAccount> employeeAccounts = null, List<GetAttestTransitionLogsForEmployeeResult> transistionsLogs = null,
			List<TimeScheduleTemplateBlock> templateBlocks = null)
		{
			var users = GetEmployeeExtendedNearestExecutives(entities, employee, startDate, stopDate, actorCompanyId, employeeAccounts, onlyDefault: true, onlyMainAllocation: true);

			if (!users.Any())
				users = GetEmployeeExtendedNearestExecutives(entities, employee, startDate, stopDate, actorCompanyId, employeeAccounts, onlyExecutive: false, onlyMainAllocation: true);

			if (!users.Any())
				users = GetEmployeeExtendedNearestExecutives(entities, employee, startDate, stopDate, actorCompanyId, employeeAccounts, onlyDefault: false, onlyMainAllocation: true);

			if (!users.Any())
				users = GetEmployeeExtendedNearestExecutives(entities, employee, startDate, stopDate, actorCompanyId, employeeAccounts, onlyDefault: true);

			if (!users.Any())
				users = GetEmployeeExtendedNearestExecutives(entities, employee, startDate, stopDate, actorCompanyId, employeeAccounts, onlyDefault: false);

			if (!users.Any())
				users = GetEmployeeExtendedNearestExecutives(entities, employee, startDate, stopDate, actorCompanyId, employeeAccounts, onlyExecutive: false);

			if (!users.Any())
				return null;

			if (users.Count == 1)
				return users.First().User;

			if (employee.UserId.HasValue && users.Any(a => a.User.UserId == employee.UserId.Value))
				users = users.Where(w => w.User.UserId != employee.UserId.Value).ToList();

			if (users.Count == 1)
				return users.First().User;

			var useHierachy = base.UseAccountHierarchyOnCompanyFromCache(entities, actorCompanyId);
			var roles = base.GetRolesFromCache(entities, CacheConfig.Company(actorCompanyId));
			List<UserCompanyRoleDTO> companyUserRolesOnUsers = new List<UserCompanyRoleDTO>();

			var attestRoles = base.GetTimeAttestRolesFromCache(entities, CacheConfig.Company(actorCompanyId));
			var validatedAttestUsers = new List<AttestRoleExtendedUserDTO>();
			if (attestRoles.Any(a => a.Sort > 0))
			{
				validatedAttestUsers = NearestExcutivesbasedOnAttestRole(entities, actorCompanyId, users, employee, startDate, stopDate, employeeAccounts, useHierachy, companyUserRolesOnUsers, true);

				if (validatedAttestUsers.Any())
					users = validatedAttestUsers;

				if (validatedAttestUsers.Count == 1)
					return validatedAttestUsers.First().User;
			}

			if (roles.Any(a => a.Sort > 0))
			{
				var validatedUsers = NearestExcutivesbasedOnRole(entities, actorCompanyId, users, employee);

				if (validatedUsers.Any())
					users = validatedUsers;

				if (users.Count == 1)
					return users.First().User;
			}

			if (attestRoles.Any(a => a.Sort > 0))
			{
				if (!validatedAttestUsers.Any())
					validatedAttestUsers = NearestExcutivesbasedOnAttestRole(entities, actorCompanyId, users, employee, startDate, stopDate, employeeAccounts, useHierachy, companyUserRolesOnUsers, false);

				if (validatedAttestUsers.Any())
					users = validatedAttestUsers;
			}

			if (users.Count == 1)
				return users.First().User;

			var initialAttestStateId = 0;

			transistionsLogs = transistionsLogs ?? AttestManager.GetLimitedAttestTransitionLogsForEmployee(entities, employee.EmployeeId, DateTime.Today.AddDays(-30), DateTime.Today);
			initialAttestStateId = AttestManager.GetInitialAttestStateId(entities, employee.ActorCompanyId, TermGroup_AttestEntity.PayrollTime);

			var usersWithInitialTransistions = new List<UserDTO>();

			foreach (var user in users)
			{
				var userTransastions = transistionsLogs.Where(a => a.AttestStateFromId == initialAttestStateId && a.AttestTransitionUserId == user.User.UserId).ToList();

				if (employee.UserId.HasValue)
					userTransastions = userTransastions.Where(a => a.AttestTransitionUserId != employee.UserId.Value).ToList();

				if (userTransastions.Any())
					usersWithInitialTransistions.Add(user.User);
			}

			if (usersWithInitialTransistions.Any() && usersWithInitialTransistions.Count == 1)
				return usersWithInitialTransistions.First();

			templateBlocks = templateBlocks ?? entities.TimeScheduleTemplateBlock.Where(w => w.EmployeeId == employee.EmployeeId && w.Date.HasValue && w.Date >= startDate && w.Date <= stopDate && w.State == (int)SoeEntityState.Active).ToList();
			if (!templateBlocks.Any())
			{
				var weekoldStartDate = startDate.AddDays(-7);
				templateBlocks = entities.TimeScheduleTemplateBlock.Where(w => w.EmployeeId == employee.EmployeeId && w.Date.HasValue && w.Date >= weekoldStartDate && w.Date <= stopDate && w.State == (int)SoeEntityState.Active).ToList();
			}

			if (templateBlocks.Any())
			{
				var usersTemplateBlocks = new List<UserDTO>();

				foreach (var user in users)
				{
					var userTemplateBlocks = templateBlocks.Where(a => a.CreatedBy == user.User.LoginName).ToList();

					if (userTemplateBlocks.Any())
						usersTemplateBlocks.Add(user.User);
				}

				if (usersTemplateBlocks.Any() && usersTemplateBlocks.Count == 1)
					return usersTemplateBlocks.First();
			}

			var usersWithTransistions = new List<UserDTO>();

			foreach (var user in users)
			{
				var userTransastions = transistionsLogs.Where(a => a.AttestTransitionUserId == user.User.UserId).ToList();

				if (userTransastions.Any())
					usersWithTransistions.Add(user.User);
			}

			if (usersWithTransistions.Any())
				return usersWithTransistions.First();

			return users.FirstOrDefault(u => u.User.UserId != employee.UserId)?.User;
		}

		private List<AttestRoleExtendedUserDTO> NearestExcutivesbasedOnRole(CompEntities entities, int actorCompanyId, List<AttestRoleExtendedUserDTO> users, Employee employee)
		{
			var companyUserRolesOnUsers = users.SelectMany(s => s.UserCompanyRoles).ToList();

			if (companyUserRolesOnUsers.Any())
			{
				int sort = 0;
				if (employee?.UserId != null)
				{
					var userRolesOnUser = base.GetUserCompanyRolesForCompanyFromCache(entities, CacheConfig.Company(actorCompanyId)).Where(a => a.UserId == employee.UserId.Value && a.Default).ToList();

					if (!userRolesOnUser.Any())
						userRolesOnUser = base.GetUserCompanyRolesForCompanyFromCache(entities, CacheConfig.Company(actorCompanyId)).Where(a => a.UserId == employee.UserId.Value).ToList();

					if (userRolesOnUser.Any())
						sort = userRolesOnUser.Max(m => m.Role.Sort);

					if (sort != 0)
					{
						var companyUserRoleWithHigherSort = companyUserRolesOnUsers.GetExecutiveUserCompanyRoleUsers(users.Select(s => s.User.UserId).ToList(), sort).Where(w => w.Default);

						if (!companyUserRoleWithHigherSort.Any())
							companyUserRoleWithHigherSort = companyUserRolesOnUsers.GetExecutiveUserCompanyRoleUsers(users.Select(s => s.User.UserId).ToList(), sort);

						if (companyUserRoleWithHigherSort.Count() == 1)
							return users.Where(f => f.User.UserId == companyUserRoleWithHigherSort.First().UserId).ToList();
						else if (companyUserRoleWithHigherSort.Count() > 1)
							users = users.Where(w => companyUserRoleWithHigherSort.Select(s => s.UserId).ToList().Contains(w.User.UserId)).ToList();
					}
				}

				var companyUserRoles = companyUserRolesOnUsers.GetExecutiveUserCompanyRoleUsers(users.Select(s => s.User.UserId).ToList()).Where(w => w.Default).ToList();

				if (!companyUserRoles.Any())
					companyUserRoles = companyUserRolesOnUsers.GetExecutiveUserCompanyRoleUsers(users.Select(s => s.User.UserId).ToList()).ToList();

				if (companyUserRoles.Count == 1)
					return users.Where(f => f.User.UserId == companyUserRoles.First().UserId).ToList();
				else
					users = users.Where(w => companyUserRoles.Select(s => s.UserId).ToList().Contains(w.User.UserId)).ToList();
			}

			int roleSort = 0;
			if (companyUserRolesOnUsers.Any())
			{
				var companyUserRolesOnUsersHavingRoleSortGreaterThanZero = companyUserRolesOnUsers.Where(a => a.RoleSort > 0);
				if (companyUserRolesOnUsersHavingRoleSortGreaterThanZero.Any())
				{
					roleSort = companyUserRolesOnUsersHavingRoleSortGreaterThanZero.Min(a => a.RoleSort);
				}
			}

			if (roleSort > 0)
			{
				companyUserRolesOnUsers = companyUserRolesOnUsers.Where(a => a.RoleSort == roleSort).ToList();
				if (companyUserRolesOnUsers.Any())
				{
					users = users.Where(w => companyUserRolesOnUsers.Select(s => s.UserId).ToList().Contains(w.User.UserId)).ToList();
				}
			}

			return users;
		}

		private List<AttestRoleExtendedUserDTO> NearestExcutivesbasedOnAttestRole(CompEntities entities, int actorCompanyId, List<AttestRoleExtendedUserDTO> users, Employee employee, DateTime startDate, DateTime stopDate, List<EmployeeAccount> employeeAccounts, bool useHierachy, List<UserCompanyRoleDTO> companyUserRolesOnUsers, bool childrenOnly)
		{
			var accountsOnLevel = new List<AccountDTO>();
			if (useHierachy)
			{
				var defaultAccountDim = base.GetDefaultEmployeeAccountDimFromCache(entities, actorCompanyId);
				if (defaultAccountDim != null)
				{
					var accounts = base.GetAccountInternalsFromCache(entities, CacheConfig.Company(actorCompanyId));
					accountsOnLevel = accounts.Where(w => w.AccountDimId == defaultAccountDim.AccountDimId).ToList();
				}
			}

			var accountIdsOnLevel = accountsOnLevel.Select(s => s.AccountId).ToList();
			List<int> employeeAccountIds = employeeAccounts != null ? employeeAccounts.Where(w => w.EmployeeId == employee.EmployeeId && w.AccountId.HasValue && EmployeeManager.IsEmployeeAccountValid(w, startDate, stopDate)).Select(s => s.AccountId.Value).ToList() : null;
			if (!employeeAccountIds.IsNullOrEmpty())
			{
				List<int> employeeAccountsAndparentIds = new List<int>();
				foreach (var accountId in employeeAccountIds)
				{
					if (accountsOnLevel.Any(a => a.AccountId == accountId))
						employeeAccountsAndparentIds.AddRange(AccountManager.GetAccountInternalAndParents(entities, accountId, actorCompanyId).Select(x => x.AccountId));
				}
				employeeAccountIds = employeeAccountsAndparentIds.Distinct().ToList();
			}

			var attestRoleUsersForCompany = base.GetAttestRoleUsersFromCache(entities, CacheConfig.Company(actorCompanyId), startDate, stopDate);
			var attestRolesOnUsers = users.SelectMany(s => s.UserAttestRoles).ToList();
			var filteredRolesOnUsers = new List<UserAttestRoleDTO>();

			if (childrenOnly)
			{
				var attestRoleUsersWithParent = attestRolesOnUsers.Where(a => a.ParentAttestRoleUserId.HasValue).ToList();
				if (attestRoleUsersWithParent.Any())
					foreach (var arou in attestRolesOnUsers)
						arou.Children = attestRoleUsersWithParent.Where(w => w.ParentAttestRoleUserId == arou.AttestRoleUserId).ToList();

				var attestRoleUsersWithChildren = attestRolesOnUsers.Where(a => a.Children != null && a.Children.Any()).ToList();

				foreach (var parentAttestRoleUsers in attestRoleUsersWithChildren)
				{
					if (employeeAccounts.Any(a => a.AccountId == parentAttestRoleUsers.AccountId))
					{
						if (parentAttestRoleUsers.Children.IsNullOrEmpty())
							filteredRolesOnUsers.Add(parentAttestRoleUsers);
						else
						{
							foreach (var child in parentAttestRoleUsers.Children)
							{
								if (child.Children.IsNullOrEmpty())
									filteredRolesOnUsers.Add(parentAttestRoleUsers);
								else
								{
									foreach (var childchild in child.Children)
									{
										if (childchild.Children.IsNullOrEmpty())
											filteredRolesOnUsers.Add(parentAttestRoleUsers);
									}
								}
							}
						}
					}
				}

				if (filteredRolesOnUsers.Any())
				{
					users = users.Where(w => filteredRolesOnUsers.Select(s => s.UserId).ToList().Contains(w.User.UserId)).ToList();

					if (users.Count == 1)
						return users;

					attestRolesOnUsers = filteredRolesOnUsers.Distinct().ToList();
				}
			}

			int sort = 0;
			if (employee?.UserId != null)
			{
				var attestRolesOnEmployee = attestRoleUsersForCompany.GetByUser(employee.UserId.Value);
				if (attestRolesOnEmployee.Any(a => a.AttestRole != null && a.AttestRole.Sort != 0))
					sort = attestRolesOnEmployee.Max(m => m.AttestRole.Sort);

				if (sort != 0)
				{
					var companyUserRoleWithHigherSort = attestRolesOnUsers.GetExecutiveAttestRoleUsers(users.Select(s => s.User.UserId).ToList(), sort);

					if (useHierachy && companyUserRoleWithHigherSort.Any() && !employeeAccountIds.IsNullOrEmpty())
					{
						var withAccount = companyUserRoleWithHigherSort.Where(a => a.AccountId.HasValue && employeeAccountIds.Contains(a.AccountId.Value)).ToList();

						if (withAccount.Any())
							companyUserRoleWithHigherSort = withAccount;
					}

					if (companyUserRoleWithHigherSort.Count == 1)
						return users.Where(f => f.User.UserId == companyUserRoleWithHigherSort[0].UserId).ToList();
					else if (companyUserRoleWithHigherSort.Count > 1)
						users = users.Where(w => companyUserRoleWithHigherSort.Select(s => s.UserId).ToList().Contains(w.User.UserId)).ToList();
				}
			}

			if (attestRolesOnUsers.Any(a => a.RoleId.HasValue))
			{
				var filteredAttestRolesOnUsers = attestRolesOnUsers.Where(a => a.RoleId.HasValue && companyUserRolesOnUsers.Select(s => s.RoleId).ToList().Contains(a.RoleId.Value)).ToList();

				if (filteredAttestRolesOnUsers.Any())
					attestRolesOnUsers = filteredAttestRolesOnUsers;
			}

			int attestRoleSort = 0;
			if (attestRolesOnUsers.Any())
			{
				var attestRolesOnUsersHavingAttestRoleSortGreaterThanZero = attestRolesOnUsers.Where(a => a.AttestRoleSort > 0);
				if (attestRolesOnUsersHavingAttestRoleSortGreaterThanZero.Any())
				{
					attestRoleSort = attestRolesOnUsersHavingAttestRoleSortGreaterThanZero.Min(a => a.AttestRoleSort) ?? 0;
				}
			}

			if (attestRoleSort > 0)
			{
				attestRolesOnUsers = attestRolesOnUsers.Where(a => a.AttestRoleSort == attestRoleSort).ToList();
				if (attestRolesOnUsers.Any())
				{
					users = users.Where(w => attestRolesOnUsers.Select(s => s.UserId).ToList().Contains(w.User.UserId)).ToList();
				}
			}

			return users;
		}

		public List<DateIntervalValidationDTO> HasPermissionToEmployeeIntervals(CompEntities entities, DateTime dateFrom, DateTime dateTo, int actorCompanyId, int userId, int roleId, Employee employee)
		{
			List<DateIntervalValidationDTO> dateIntervals = new List<DateIntervalValidationDTO>();
			var extendedEmployeeInformation = GetEmployeeExtendedNearestExecutives(entities, employee, dateFrom, dateTo, actorCompanyId, onlyExecutive: false);
			var attestRoleUsers = extendedEmployeeInformation.SelectMany(sm => sm.UserAttestRoles.Where(w => w.State == SoeEntityState.Active && w.UserId == UserId && (!w.RoleId.HasValue) || w.RoleId == roleId)).ToList();

			if (!attestRoleUsers.Any())
				return new List<DateIntervalValidationDTO>();
			if (UseAccountHierarchyOnCompanyFromCache(entities, actorCompanyId))
			{
				var employeeAccounts = EmployeeManager.GetEmployeeAccounts(entities, actorCompanyId, employee.EmployeeId, dateFrom, dateTo);
				if (employeeAccounts.IsNullOrEmpty())
					return dateIntervals;

				employeeAccounts = employeeAccounts.Where(w => w.AccountId.HasValue && EmployeeManager.IsEmployeeAccountValid(w, dateFrom, dateTo)).ToList();
				if (employeeAccounts.IsNullOrEmpty())
					return dateIntervals;

				foreach (var attestRole in attestRoleUsers.Where(w => w.AccountId.HasValue))
				{
					var validEmployeeAccounts = employeeAccounts.Where(w => w.IsDateValid(attestRole.DateFrom.Value, attestRole.DateTo ?? DateTime.Today.AddYears(100)) && w.IsAccountValid(attestRole.AccountId.Value));

					foreach (var validEmployeeAccount in validEmployeeAccounts)
					{
						var startDate = CalendarUtility.GetLatestDate(attestRole.DateFrom, validEmployeeAccount.DateFrom);
						var stopDate = CalendarUtility.GetEarliestDate(attestRole.DateTo, validEmployeeAccount.DateTo);

						if (startDate <= stopDate)
							dateIntervals.Add(new DateIntervalValidationDTO(dateFrom, dateTo));
					}
				}
			}

			return dateIntervals;
		}

		public bool HasPermissionToEmployee(CompEntities entities, DateTime dateFrom, DateTime dateTo, int actorCompanyId, int userId, int roleId, Employee employee)
		{
			var cacheKey = $"HasPermissionToEmployee_{actorCompanyId}_{userId}_{roleId}_{employee.EmployeeId}";
			var cachedIntervals = BusinessMemoryCache<List<DateInterval>>.Get(cacheKey) ?? new List<DateInterval>();

			// Check if any cached interval overlaps with the requested interval
			foreach (var interval in cachedIntervals)
			{
				if (interval.TimeFrom <= dateTo && interval.TimeTo >= dateFrom)
				{
					return true;
				}
			}

			// If no overlapping interval is found, call GetEmployeesForUsersAttestRoles
			employee = EmployeeManager.GetEmployeesForUsersAttestRoles(out _, actorCompanyId, userId, roleId, dateFrom: dateFrom, dateTo: dateTo, employeeFilter: employee.EmployeeId.ObjToList()).FirstOrDefault();
			var valid = employee != null;

			if (valid)
			{
				// Add the new interval to the cache
				cachedIntervals.Add(new DateInterval { TimeFrom = dateFrom, TimeTo = dateTo });
				BusinessMemoryCache<List<DateInterval>>.Set(cacheKey, cachedIntervals, 10);
			}

			return valid;
		}

		public class DateInterval
		{
			public DateTime TimeFrom { get; set; }
			public DateTime TimeTo { get; set; }
		}


		public List<UserWithNameAndLoginDTO> GetUsersWithNameAndLogin(int actorCompanyId)
		{
			var userWithNameAndLogins = new List<UserWithNameAndLoginDTO>();

			using var entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
			List<User> users = (from ucr in entitiesReadOnly.UserCompanyRole
								where ucr.Company.ActorCompanyId == actorCompanyId &&
								ucr.User.State != (int)SoeEntityState.Deleted &&
								ucr.Role.RoleFeature.Any(rf => rf.SysFeatureId == (int)Feature.Economy_Accounting_Vouchers_Edit)
								select ucr.User).Distinct().OrderBy(u => u.LoginName).ToList();

			foreach (User user in users)
			{
				userWithNameAndLogins.Add(new UserWithNameAndLoginDTO()
				{
					LoginName = user.LoginName,
					UserNameAndLogin = string.Format("{0} ({1})", user.Name, user.LoginName)
				});
			}

			return userWithNameAndLogins;
		}

		public Dictionary<int, string> GetUsersByCompanyDict(int actorCompanyId, int roleId, int userId, bool addEmptyRow, bool includeKey, bool useFullName, bool includeLoginName)
		{
			return GetUsersByCompany(actorCompanyId, roleId, userId).ToDict(addEmptyRow, includeKey, useFullName, includeLoginName);
		}

		public Dictionary<int, string> GetUsersWithoutEmployeesDict(int licenseId, int actorCompanyId, int? includeUserId, bool addEmptyRow, bool includeKey, bool useFullName, bool includeLoginName)
		{
			return GetUsersWithoutEmployees(licenseId, actorCompanyId, includeUserId).ToDict(addEmptyRow, includeKey, useFullName, includeLoginName);
		}

		public List<UserRequestTypeDTO> GetAvalibilityByUsers(IEnumerable<User> users, int actorCompanyId, DateTime fromDate, DateTime stopDate)
		{
			var retList = new List<UserRequestTypeDTO>();

			using var entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
			entitiesReadOnly.EmployeeRequest.NoTracking();
			var baseQuery = (from entry in entitiesReadOnly.EmployeeRequest
							 where entry.ActorCompanyId == actorCompanyId &&
							 fromDate < entry.Stop &&
							 stopDate > entry.Start &&
							 entry.State == (int)SoeEntityState.Active
							 select entry);

			//For performance, see if there are any request in the interval before fetching employee
			var employeeRequestsExist = (from entry in baseQuery select entry).Any();

			foreach (var u in users)
			{
				TermGroup_EmployeeRequestTypeFlags requestType = TermGroup_EmployeeRequestTypeFlags.Undefined;

				if (employeeRequestsExist)
				{
					var employeeQuery = (from entry in baseQuery
										 where entry.Employee.UserId == u.UserId &&
										 entry.Employee.ActorCompanyId == actorCompanyId &&
										 entry.Employee.State == (int)SoeEntityState.Active
										 orderby entry.Type ascending
										 select entry);

					bool employeeExists = employeeQuery.Any();

					if (employeeExists)
					{
						var employeeRequest = employeeQuery.FirstOrDefault();
						if (employeeRequest != null)
						{
							requestType = (TermGroup_EmployeeRequestTypeFlags)employeeRequest.Type;
							if (employeeRequest.Start > fromDate || employeeRequest.Stop < stopDate)
								requestType = requestType | TermGroup_EmployeeRequestTypeFlags.PartyDefined;
						}
					}
				}

				retList.Add(u.ToRequestTypeDTO(requestType));
			}

			return retList;
		}

		public List<UserRequestTypeDTO> GetUsersByLicenseAndAvailability(int actorCompanyId, int roleId, int userId, int licenseId, DateTime fromDate, DateTime stopDate, bool setDefaultRoleName = false, bool? active = true)
		{
			var users = GetUsersByLicense(licenseId, actorCompanyId, roleId, userId, setDefaultRoleName, active);
			return GetAvalibilityByUsers(users, actorCompanyId, fromDate, stopDate);
		}

		public List<UserRequestTypeDTO> GetUsersByCompanyAndAvailability(int actorCompanyId, int roleId, int userId, DateTime fromDate, DateTime stopDate, bool setDefaultRoleName = false, bool? active = true)
		{
			var users = GetUsersByCompany(actorCompanyId, roleId, userId, setDefaultRoleName: setDefaultRoleName, active: active);
			return GetAvalibilityByUsers(users, actorCompanyId, fromDate, stopDate);
		}

		public GenericType<int, int> GetNrOfUsersAndMaxByLicense(int licenseId, bool onlyActive = true)
		{
			using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
			entities.User.NoTracking();
			entities.License.NoTracking();
			return GetNrOfUsersAndMaxByLicense(entities, licenseId, onlyActive);
		}

		public GenericType<int, int> GetNrOfUsersAndMaxByLicense(CompEntities entities, int licenseId, bool onlyActive = true)
		{
			var nrOfUsers = new GenericType<int, int>();

			var users = (from ucr in entities.UserCompanyRole.Include("User")
						 where ((ucr.Company.LicenseId == licenseId) &&
						 (ucr.User.State == (int)SoeEntityState.Active) &&
						 (ucr.Company.State == (int)SoeEntityState.Active) &&
						 !ucr.Company.Demo)
						 orderby ucr.Company.Name
						 select ucr.User).Distinct();

			if (onlyActive)
				users = users.Where(u => u.State == (int)SoeEntityState.Active);

			var maxNrOfUsers = (from l in entities.License
								where l.LicenseId == licenseId
								select l.MaxNrOfUsers).FirstOrDefault();

			nrOfUsers.Field1 = users.Count();
			nrOfUsers.Field2 = maxNrOfUsers;

			return nrOfUsers;
		}

		public List<Guid> GetIdLoginGuidsWithMultipleUsers()
		{
			var guids = BusinessMemoryCache<List<Guid>>.Get("GetIdLoginGuidsWithMultipleUsers");

			if (guids != null)
				return guids;

			guids = new List<Guid>();
			using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
			entitiesReadOnly.User.NoTracking();
			var groups = entitiesReadOnly.User.Where(w => w.State == (int)SoeEntityState.Active && w.License.State == (int)SoeEntityState.Active && w.idLoginGuid.HasValue).GroupBy(g => g.idLoginGuid.Value).ToList();

			foreach (var item in groups.Where(w => w.Count() > 1).ToList())
			{
				var users = item.ToList();
				guids.Add(users.First().idLoginGuid.Value);
			}

			BusinessMemoryCache<List<Guid>>.Set("GetIdLoginGuidsWithMultipleUsers", guids, 60 * 5);
			return guids;
		}

		public User GetUserForMobileLogin(Guid idLoginGuid)
		{
			var fromCache = BusinessMemoryCache<User>.Get("GetUserForMobileLogin" + idLoginGuid.ToString());

			if (fromCache != null)
			{
				return fromCache;
			}

			if (!HasIdLoginGuidsWithMultipleUsers(idLoginGuid))
				return GetUser(idLoginGuid);

			var chosenSoeLicenseId = SoftOneIdConnector.GetChosenMobileApiEndPointSoeLicenseId(idLoginGuid);

			if (chosenSoeLicenseId == 0)
				return GetUser(idLoginGuid);

			using var entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
			var onChosen = (from u in entitiesReadOnly.User
							where u.idLoginGuid == idLoginGuid &&
							u.LicenseId == chosenSoeLicenseId &&
							(u.State == (int)SoeEntityState.Active) &&
							(u.License.State == (int)SoeEntityState.Active)
							select u).FirstOrDefault<User>();

			if (onChosen != null)
				BusinessMemoryCache<User>.Set("GetUserForMobileLogin" + idLoginGuid.ToString(), onChosen, 10);

			return onChosen ?? GetUser(idLoginGuid);
		}

		public List<User> GetUsersWithGuid(Guid idLoginGuid)
		{
			var key = "GetUsersWithGuid_" + idLoginGuid.ToString();
			var fromCache = BusinessMemoryCache<List<User>>.Get(key);
			if (fromCache != null)
				return fromCache;
			using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
			entities.User.NoTracking();
			var result = (from u in entities.User
						  where u.idLoginGuid == idLoginGuid &&
											 (u.State == (int)SoeEntityState.Active)
						  select u).ToList();

			if (result != null)
				BusinessMemoryCache<List<User>>.Set(key, result, 10);

			return result;
		}

		public User GetUser(Guid idLoginGuid, bool includeLicense = false)
		{
			using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
			entities.User.NoTracking();
			return GetUser(entities, idLoginGuid, includeLicense);
		}
		public User GetUser(CompEntities entities, Guid idLoginGuid, bool includeLicense = false)
		{
			if (idLoginGuid == Guid.Empty)
				return null;

			User user = (from u in entities.User
						 where u.idLoginGuid == idLoginGuid &&
						 (u.State == (int)SoeEntityState.Active) &&
						 (u.License.State == (int)SoeEntityState.Active)
						 select u).FirstOrDefault<User>();


			if (user != null && includeLicense && !user.LicenseReference.IsLoaded)
				user.LicenseReference.Load();

			return user;
		}

		public List<User> GetUsers(Guid IdLoginGuid, bool includeLicense = false)
		{
			using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
			entities.User.NoTracking();
			return (from u in entities.User
					where u.idLoginGuid == IdLoginGuid &&
					(u.State == (int)SoeEntityState.Active)
					select u).ToList();
		}

		public User GetUser(string licenseNr, string loginName, bool includeLicense)
		{
			using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
			entities.User.NoTracking();
			User user = (from u in entities.User
						 where ((u.License.LicenseNr == licenseNr) &&
						 (u.LoginName.ToLower() == loginName.ToLower()) &&
						 (u.State == (int)SoeEntityState.Active) &&
						 (u.License.State == (int)SoeEntityState.Active))
						 select u).FirstOrDefault<User>();

			if (user != null && includeLicense && !user.LicenseReference.IsLoaded)
				user.LicenseReference.Load();

			return user;
		}

		public User GetUser(string licenseNr, string loginName, byte[] passwordHash, bool includeLicense)
		{
			using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
			entities.User.NoTracking();
			User user = (from u in entities.User
						 where ((u.passwordhash != null) &&
						 (u.License.LicenseNr == licenseNr) &&
						 (u.LoginName.ToLower() == loginName.ToLower()) &&
						 (u.passwordhash == passwordHash) &&
						 (u.State == (int)SoeEntityState.Active) &&
						 (u.License.State == (int)SoeEntityState.Active))
						 select u).FirstOrDefault<User>();

			if (user != null && includeLicense && !user.LicenseReference.IsLoaded)
				user.LicenseReference.Load();

			return user;
		}

		public User GetUser(int userId, bool onlyActive = true, bool loadUserCompanyRole = false, bool loadAttestRoleUser = false, bool loadLicense = false, bool loadEmployee = false, bool loadEmployment = false, bool loadInactive = false)
		{
			using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
			entities.User.NoTracking();
			return GetUser(entities, userId, onlyActive, loadUserCompanyRole, loadAttestRoleUser, loadLicense, loadEmployee, loadEmployment, loadInactive);
		}

		public User GetUser(CompEntities entities, int userId, bool onlyActive = true, bool loadUserCompanyRole = false, bool loadAttestRoleUser = false, bool loadLicense = false, bool loadEmployee = false, bool loadEmployment = false, bool loadInactive = false)
		{
			IQueryable<User> query = (from u in entities.User
									  where u.UserId == userId
									  select u);

			if (loadUserCompanyRole)
				query = query.Include("UserCompanyRole");
			if (loadAttestRoleUser)
				query = query.Include("AttestRoleUser");
			if (loadLicense)
				query = query.Include("License");
			if (loadEmployee)
			{
				if (loadEmployment)
				{
					query = query.Include("Employee.Employment.EmploymentChangeBatch.EmploymentChange")
								 .Include("Employee.Employment.OriginalEmployeeGroup")
								 .Include("Employee.Employment.OriginalPayrollGroup");
				}
				else
					query = query.Include("Employee");
			}


			// Get state
			if (onlyActive && !loadInactive)
				query = query.Where(a => a.State == (int)SoeEntityState.Active);

			if (!onlyActive && !loadInactive)
				query = query.Where(a => a.State != (int)SoeEntityState.Deleted);

			if (!onlyActive && loadInactive)
				query = query.Where(a => a.State != (int)SoeEntityState.Deleted);

			if (onlyActive && loadInactive)
				query = query.Where(a => a.State != (int)SoeEntityState.Deleted);

			return query.FirstOrDefault();
		}

		public User GetUserIgnoreState(int userId, bool loadUserCompanyRole = false, bool loadLicense = false, bool loadEmployee = false, bool loadContactPerson = false, bool loadAttestRoleUser = false)
		{
			using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
			entities.User.NoTracking();
			return GetUserIgnoreState(entities, userId, loadUserCompanyRole, loadLicense, loadEmployee, loadContactPerson, loadAttestRoleUser);
		}

		public User GetUserIgnoreState(CompEntities entities, int userId, bool loadUserCompanyRole = false, bool loadLicense = false, bool loadEmployee = false, bool loadContactPerson = false, bool loadAttestRoleUser = false)
		{
			IQueryable<User> query = (from u in entities.User
									  where u.UserId == userId &&
									  u.State != (int)SoeEntityState.Deleted //Ignore active or inactive, but not deleted
									  select u);

			if (loadUserCompanyRole)
				query = query.Include("UserCompanyRole");
			if (loadLicense)
				query = query.Include("License");
			if (loadEmployee)
				query = query.Include("Employee");
			if (loadContactPerson)
			{
				query = query.Include("ContactPerson");
				if (loadEmployee)
					query = query.Include("Employee.ContactPerson");
			}
			if (loadAttestRoleUser)
				query = query.Include("AttestRoleUser");

			return query.FirstOrDefault();
		}

		public User GetUserByEmployeeId(int employeeId, int actorCompanyId)
		{
			using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
			entities.User.NoTracking();
			return GetUserByEmployeeId(entities, employeeId, actorCompanyId);
		}

		public User GetUserByEmployeeId(CompEntities entities, int employeeId, int actorCompanyId, bool getVacant = true)
		{
			Employee employee = EmployeeManager.GetEmployee(entities, employeeId, actorCompanyId, loadUser: true);
			if (employee != null && employee.User != null)
			{
				if (!getVacant && employee.Vacant)
					return null;
				return employee.User;
			}

			return null;
		}

		public User GetUserOnLicense(string licenseNr, string loginName, bool includeInactive = false)
		{
			using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
			entities.User.NoTracking();
			return (from u in entities.User
					where ((u.License.LicenseNr == licenseNr) &&
					(u.LoginName.ToLower() == loginName.ToLower()) &&
					(includeInactive ? (u.State != (int)SoeEntityState.Deleted) : (u.State == (int)SoeEntityState.Active)) &&
					(u.License.State == (int)SoeEntityState.Active))
					select u).FirstOrDefault();
		}

		public User GetAdminUser(int actorCompanyId)
		{
			Role role = RoleManager.GetRoleAdmin(actorCompanyId);
			if (role == null)
				return null;

			User user = UserManager.GetUserInCompanyRole(role.RoleId, actorCompanyId);
			if (user == null)
				return null;

			return user;
		}

		public User GetUserInCompanyRole(int roleId, int actorCompanyId)
		{
			int licenseId = LicenseManager.GetLicenseIdByCompanyId(actorCompanyId);
			DateTime date = DateTime.Today;

			using var entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
			return (from u in entitiesReadOnly.User
					.Include("License")
					where u.LicenseId == licenseId &&
					u.State == (int)SoeEntityState.Active &&
					u.UserCompanyRole.Any(ucr =>
					   ucr.ActorCompanyId == actorCompanyId &&
					   ucr.RoleId == roleId &&
					   ucr.State == (int)SoeEntityState.Active &&
					   ucr.Role.State == (int)SoeEntityState.Active &&
						(!ucr.DateFrom.HasValue || ucr.DateFrom <= date) &&
						(!ucr.DateTo.HasValue || ucr.DateTo >= date)
					   )
					orderby u.SysUser descending
					select u).FirstOrDefault();
		}

		public List<LicenseLoginInfo> GetAllLicenseLogins()
		{
			using var entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
			return (from u in entitiesReadOnly.User.Include("License")
					select new LicenseLoginInfo
					{
						LoginGuid = u.idLoginGuid.Value,
						LicenseId = u.LicenseId,
						State = u.State,
						LicenseState = u.License.State
					}).ToList();
		}

		public List<LicenseLoginInfo> GetLicenseLogins(Guid idLoginGuid)
		{
			using var entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
			entitiesReadOnly.User.NoTracking();
			return (from u in entitiesReadOnly.User
					where u.idLoginGuid == idLoginGuid
					select new LicenseLoginInfo
					{
						LoginGuid = u.idLoginGuid.Value,
						LicenseId = u.LicenseId,
						State = u.State,
						LicenseState = u.License.State
					}).ToList();
		}

		public UserInfoDTO GetUserInfoDTO(Guid idLoginGuid, bool checkMissingMandatoryInformation = false)
		{
			using var entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
			entitiesReadOnly.User.NoTracking();
			entitiesReadOnly.License.NoTracking();
			User user = (from u in entitiesReadOnly.User
						 .Include("license")
						 where u.idLoginGuid.HasValue &&
						 u.idLoginGuid.Value == idLoginGuid
						 select u).FirstOrDefault();

			if (user == null)
				return new UserInfoDTO();

			UserInfoDTO userInfoDTO = new UserInfoDTO();
			userInfoDTO.IdLoginGuid = idLoginGuid;
			userInfoDTO.LicenseNr = user.License.LicenseNr;
			userInfoDTO.LicenseId = user.License.LicenseId;
			userInfoDTO.LicenseName = user.License.Name;
			userInfoDTO.Email = user.Email;
			userInfoDTO.UserId = user.UserId;
			userInfoDTO.SysCompDbId = Convert.ToInt32(CompDbCache.Instance.SysCompDbId);
			userInfoDTO.SysServerId = user.License.SysServerId.HasValue ? user.License.SysServerId.Value : 0;
			userInfoDTO.Url = SysCompanyConnector.GetWebUri(userInfoDTO.SysCompDbId);
			userInfoDTO.MobilePhone = user.EstatusLoginId;
			userInfoDTO.SysLanguageId = user.LangId ?? Constants.SYSLANGUAGE_SYSLANGUAGEID_DEFAULT;
			userInfoDTO.UsernameInGo = user.LoginName;
			userInfoDTO.MissingMandatoryInformation = new List<MissingMandatoryInformation>();

			string idProviderGuidStr = SettingManager.GetStringSetting(SettingMainType.License, (int)LicenseSettingType.SSO_Key, 0, 0, user.LicenseId);

			if (!string.IsNullOrEmpty(idProviderGuidStr) && Guid.TryParse(idProviderGuidStr, out Guid guid))
			{
				userInfoDTO.IdProviderGuid = guid;

				if (!user.idLoginActive)
				{
					string externalAuthId = !user.idLoginActive ? SettingManager.GetStringSetting(SettingMainType.User, (int)UserSettingType.ExternalAuthId, user.UserId, 0, 0) : string.Empty;
					if (!string.IsNullOrEmpty(externalAuthId))
						userInfoDTO.ExternalAuthId = externalAuthId;
				}
			}

			if (checkMissingMandatoryInformation)
				AddMandatoryInformationToUserInfoDTO(userInfoDTO, user.UserId);

			return userInfoDTO;
		}

		public UserDTO GetSoeUserAdmin(int actorCompanyId)
		{
			return GetSoeUser(actorCompanyId, GetAdminUser(actorCompanyId));
		}

		public UserDTO GetSoeUser(int actorCompanyId, int userId)
		{
			return GetSoeUser(actorCompanyId, GetUser(userId, loadUserCompanyRole: true));
		}

		public UserDTO GetSoeUser(int actorCompanyId, User user, int? roleId = null)
		{
			if (user == null)
				return null;

			int defaultRoleId = roleId ?? UserManager.GetDefaultRoleId(actorCompanyId, user);
			return user.ToDTO(defaultRoleId);
		}

		public int GetActorContactPersonId(int userId)
		{
			using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
			entities.User.NoTracking();
			return (from u in entities.User
					where u.UserId == userId
					select u.ContactPerson.ActorContactPersonId).FirstOrDefault();
		}

		public int GetNrOfUsersByLicense(CompEntities entities, int licenseId, int actorCompanyId, int roleId, int userId, bool? active = true, int userStateTransition = 0)
		{
			var users = GetUsersByLicense(entities, licenseId, actorCompanyId, roleId, userId, active: active, setDefaultRoleName: false, excludeDemoOnlyUsers: true);
			int nrOfUsers = users.Count;

			//Adjust according to transition
			switch (userStateTransition)
			{
				case (int)SoeEntityStateTransition.ActiveToInactive:
				case (int)SoeEntityStateTransition.ActiveToDeleted:
					nrOfUsers--;
					break;
				case (int)SoeEntityStateTransition.InactiveToActive:
				case (int)SoeEntityStateTransition.DeletedToActive:
					nrOfUsers++;
					break;
			}

			return nrOfUsers;
		}

		public int GetUserIdByEmployeeId(CompEntities entities, int employeeId, int actorCompanyId)
		{
			Employee employee = EmployeeManager.GetEmployee(entities, employeeId, actorCompanyId);
			return employee != null && employee.UserId.HasValue ? employee.UserId.Value : 0;
		}

		public bool TryGetUserDefaultRoleAndCompanyFromSetting(User user, out Company company, out Role role)
		{
			company = null;
			role = null;

			if (user == null)
				return false;

			int actorCompanyId = SettingManager.GetIntSetting(SettingMainType.User, (int)UserSettingType.CoreCompanyId, user.UserId, 0, 0);
			if (actorCompanyId > 0)
			{
				int roleId = SettingManager.GetIntSetting(SettingMainType.User, (int)UserSettingType.CoreRoleId, user.UserId, 0, 0);
				if (roleId > 0)
				{
					company = CompanyManager.GetCompany(actorCompanyId, true);
					if (company != null && UserManager.ExistUserCompanyRoleMapping(user.UserId, company.ActorCompanyId, roleId))
						role = RoleManager.GetRole(roleId, company.ActorCompanyId);
				}
			}

			return company != null && role != null;
		}

		public bool TryGetUserDefaultRoleAndCompany(User user, bool mobileLogin, out Company company, out Role role)
		{
			using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
			return TryGetUserDefaultRoleAndCompany(entities, user, mobileLogin, out company, out role);
		}

		public bool TryGetUserDefaultRoleAndCompany(CompEntities entities, User user, bool mobileLogin, out Company company, out Role role)
		{
			company = null;
			role = null;
			if (user.DefaultActorCompanyId.HasValue && (mobileLogin || !TryGetUserDefaultRoleAndCompanyFromSetting(user, out company, out role)))
			{
				company = CompanyManager.GetCompany(user.DefaultActorCompanyId.Value);
				if (company != null)
				{
					int defaultRoleId = GetDefaultRoleId(entities, company.ActorCompanyId, user.UserId);
					if (defaultRoleId > 0)
						role = RoleManager.GetRole(defaultRoleId);
				}
			}

			return company != null && role != null;
		}

		public int GetUserLangId(int userId)
		{
			using var entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
			entitiesReadOnly.User.NoTracking();
			int? langId = (from u in entitiesReadOnly.User where u.UserId == userId select u.LangId).FirstOrDefault();
			return langId ?? 0;
		}

		public bool UserExist(CompEntities entities, int licenseId, string loginName, bool onlyActive = true, int? discardUserId = null)
		{
			var query = (from u in entities.User
						 where u.LoginName.ToLower() == loginName.ToLower() &&
						 u.LicenseId == licenseId &&
						 (!discardUserId.HasValue || u.UserId != discardUserId.Value)
						 select u);

			// We deliberately ignore state in this query!
			// Even deleted or inactive employees should be checked.
			// Unless parameter onlyActive is set

			if (onlyActive)
				query = query.Where(u => u.State == (int)SoeEntityState.Active);

			var users = query.ToList();
			if (!users.Any())
				return false;

			if (users.Any(u => u.State == (int)SoeEntityState.Active))
				return true;

			foreach (var user in users)
			{
				if (user.State == (int)SoeEntityState.Inactive && user.DefaultActorCompanyId.HasValue)
				{
					// Check if default company is deleted
					var company = (from c in entities.Company
								   where c.ActorCompanyId == user.DefaultActorCompanyId.Value
								   select c).FirstOrDefault();
					if (company != null && company.State == (int)SoeEntityState.Active)
						return true;
				}
			}

			return false;
		}

		public bool IsUserConnectedToCompany(int userId, int actorCompanyId)
		{
			using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
			entities.UserCompanyRole.NoTracking();
			int counter = (from ucr in entities.UserCompanyRole
						   where ucr.User.UserId == userId &&
						   ucr.Company.ActorCompanyId == actorCompanyId
						   select ucr).Count();

			if (counter > 0)
				return true;
			return false;
		}

		public bool IsUserAdminInCompany(UserDTO user, int actorCompanyId)
		{
			if (user == null)
				return false;

			var date = DateTime.Today;

			List<UserCompanyRole> userCompanyRoles = GetUserCompanyRolesByUserAndCompany(user.UserId, actorCompanyId, true, true, true);

			return (from ucr in userCompanyRoles
					where ucr.User != null &&
					ucr.Role != null &&
					ucr.State == (int)SoeEntityState.Active &&
					(!ucr.DateFrom.HasValue || ucr.DateFrom <= date) &&
					(!ucr.DateTo.HasValue || ucr.DateTo >= date) &&
					(ucr.User.SysUser || ucr.Role.TermId == (int)TermGroup_Roles.Systemadmin)
					select ucr).Any();
		}

		public bool CanAddUserToRole(CompEntities entities, int licenseId, User user)
		{
			if (!user.UserCompanyRole.IsLoaded)
				user.UserCompanyRole.Load();

			foreach (var role in user.UserCompanyRole)
			{
				if (!role.CompanyReference.IsLoaded)
					role.CompanyReference.Load();

				if (!role.Company.Demo && role.EntityState != EntityState.Added)
					return true;
			}

			// User is not in company so check license
			var licenseDetails = GetNrOfUsersAndMaxByLicense(entities, licenseId);
			return licenseDetails.Field1 < licenseDetails.Field2;
		}

		public string GetUserPhoneMobile(CompEntities entities, int userId)
		{
			ContactPerson contactPerson = (from u in entities.User
											.Include("ContactPerson")
										   where u.UserId == userId
										   select u.ContactPerson).FirstOrDefault();

			if (contactPerson == null)
				return String.Empty;

			ContactECom mobile = (from c in entities.ContactECom
								  where c.Contact != null &&
								  c.Contact.Actor != null &&
								  c.Contact.Actor.ActorId == contactPerson.ActorContactPersonId &&
								  c.SysContactEComTypeId == (int)TermGroup_SysContactEComType.PhoneMobile
								  select c).FirstOrDefault();

			if (mobile == null)
				return String.Empty;

			return mobile.Text;
		}

		private void SetDefaultUserSettings(int userId, int actorCompanyId)
		{
			#region Core

			var boolValues = new Dictionary<int, bool>();
			boolValues.Add((int)UserSettingType.CoreShowAnimations, false);
			SettingManager.UpdateInsertBoolSettings(SettingMainType.User, boolValues, userId, actorCompanyId, 0);

			#endregion

			#region Accounting

			// Set Account year to current
			AccountYear accountYear = AccountManager.GetAccountYear(DateTime.Today, actorCompanyId);
			if (accountYear != null)
				SettingManager.UpdateInsertIntSetting(SettingMainType.UserAndCompany, (int)UserSettingType.AccountingAccountYear, accountYear.AccountYearId, userId, actorCompanyId, 0);

			#endregion
		}

		private void SetUserEmployeeCategories(CompEntities entities, List<User> users, int actorCompanyId)
		{
			if (users.IsNullOrEmpty())
				return;

			var employeesByUserId = EmployeeManager.GetAllEmployeesUserIds(entities, actorCompanyId);
			var employeeCategories = CategoryManager.GetCompanyCategoryRecords(entities, SoeCategoryType.Employee, SoeCategoryRecordEntity.Employee, actorCompanyId);

			foreach (var user in users)
			{
				int employeeId = employeesByUserId.GetValue(user.UserId);
				if (employeeId != 0)
					user.Categories = String.Join(", ", employeeCategories.Where(e => e.RecordId == employeeId).Select(c => c.Category.Name));
				else
					user.Categories = "";
			}
		}

		public ActionResult AddUser(User user, int employeeId, int licenseId, int actorCompanyId, int roleId)
		{
			if (user == null)
				return new ActionResult((int)ActionResultSave.EntityIsNull, "User");

			using (CompEntities entities = new CompEntities())
			{
				user.License = LicenseManager.GetLicense(entities, licenseId);
				if (user.License == null)
					return new ActionResult((int)ActionResultSave.EntityIsNull, GetText(11889, "Licensen hittades inte"));

				if (String.IsNullOrEmpty(user.LoginName))
					return new ActionResult((int)ActionResultSave.UserInvalidUserName);

				user.DefaultActorCompanyId = actorCompanyId;
				if (user.DefaultActorCompanyId == 0)
					return new ActionResult((int)ActionResultSave.UserDefaultCompanyNotFound);

				if (employeeId > 0)
				{
					Employee employee = EmployeeManager.GetEmployee(entities, employeeId, actorCompanyId);
					if (employee == null)
						return new ActionResult((int)ActionResultSave.EntityNotFound, "Employee");

					user.Employee.Add(employee);
				}

				ActionResult result = AddEntityItem(entities, user, "User");
				if (result.Success)
					SetDefaultUserSettings(user.UserId, actorCompanyId);

				if (!user.idLoginGuid.HasValue)
					user.idLoginGuid = Guid.NewGuid();

				AddUserToSoftOneId(user);

				return result;
			}
		}

		public ActionResult UpdateUser(User user)
		{
			if (user == null)
				return new ActionResult((int)ActionResultSave.EntityIsNull, "User");

			//Flush UserCompanyRole cache
			CompDbCache.Instance.FlushUserCompanyRoles(user.UserId);

			using (CompEntities entities = new CompEntities())
			{
				User originalUser = UserManager.GetUser(entities, user.UserId);
				if (originalUser == null)
					return new ActionResult((int)ActionResultSave.EntityNotFound, "User");

				return UpdateEntityItem(entities, originalUser, user, "User");
			}
		}

		public ActionResult DeleteUser(DeleteUserDTO input, int actorCompanyId)
		{
			ActionResult result = new ActionResult();

			#region Validation

			if (input.Action == DeleteUserAction.Inactivate || input.Action == DeleteUserAction.RemoveInfo || input.Action == DeleteUserAction.Unidentify || input.Action == DeleteUserAction.Delete)
			{
				result = ActorManager.ValidateDeleteUser(input.UserId);
				if (!result.Success)
				{
					result.ErrorMessage = result.Strings.JoinToString("\n");
					return result;
				}
			}

			#endregion

			using (CompEntities entities = new CompEntities())
			{
				try
				{
					entities.Connection.Open();

					#region Prereq

					User user = GetUser(entities, input.UserId, onlyActive: false);
					if (user == null)
						return new ActionResult((int)ActionResultDelete.EntityNotFound, GetText(10083, "Anställd hittades inte"));

					if (input.Action == DeleteUserAction.Cancel)
						return new ActionResult((int)ActionResultDelete.InsufficientInput, GetText(2099, "Felaktig indata"));

					#endregion

					using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
					{
						#region Perform

						switch (input.Action)
						{
							case DeleteUserAction.Inactivate:
								DeleteUserInactivate(user);
								break;
							case DeleteUserAction.RemoveInfo:
								DeleteUserRemoveInfo(entities, user, input, false);
								break;
							case DeleteUserAction.Unidentify:
								DeleteUserUnidentify(entities, user, input);
								break;
							case DeleteUserAction.Delete:
								DeleteUserDelete(entities, user, input);
								break;
						}

						if (input.DisconnectEmployee)
						{
							Employee employee = EmployeeManager.GetEmployeeByUser(entities, actorCompanyId, input.UserId, loadContactPerson: true);
							if (employee != null && employee.UserId.HasValue)
							{
								user.Employee.Clear();

								// Create new ContactPerson

								#region ContactPerson

								Actor actor = new Actor()
								{
									ActorType = (int)SoeActorType.ContactPerson,
								};
								SetCreatedProperties(actor);

								ContactPerson contactPerson = new ContactPerson()
								{
									Actor = actor
								};

								ContactPerson originalContactPerson = employee.ContactPerson;
								if (originalContactPerson != null)
								{
									contactPerson.FirstName = originalContactPerson.FirstName;
									contactPerson.LastName = originalContactPerson.LastName;
									contactPerson.SocialSec = originalContactPerson.SocialSec;
									contactPerson.Sex = originalContactPerson.Sex;
								}

								SetCreatedProperties(contactPerson);
								entities.ContactPerson.AddObject(contactPerson);

								result = SaveChanges(entities, transaction);
								if (!result.Success)
									return result;

								#endregion

								// Copy addresses

								#region Addresses

								if (originalContactPerson != null)
								{
									List<ContactAddressItem> contactAddresses = ContactManager.GetContactAddressItems(entities, originalContactPerson.ActorContactPersonId);
									foreach (ContactAddressItem addr in contactAddresses)
									{
										// Will copy addresses to new contact person
										addr.ContactAddressId = 0;
										addr.ContactEComId = 0;
									}

									result = ContactManager.SaveContactAddresses(entities, contactAddresses, contactPerson.ActorContactPersonId, TermGroup_SysContactType.Company);
									if (!result.Success)
									{
										result.ErrorNumber = (int)ActionResultSave.EmployeeUserContactsAndTeleComNotSaved;
										result.ErrorMessage = GetText(11048, "Kontaktuppgifter ej sparade");
										return result;
									}
								}

								#endregion
							}
						}

						// Common for all actions
						SetModifiedProperties(user);

						result = SaveChanges(entities, transaction);
						if (result.Success)
						{
							//Commit transaction
							transaction.Complete();
						}

						#endregion
					}
				}
				catch (Exception ex)
				{
					base.LogError(ex, this.log);
					result.Exception = ex;
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

		private void DeleteUserInactivate(User user)
		{
			// Inactivate user
			user.State = (int)SoeEntityState.Inactive;
		}

		private void DeleteUserRemoveInfo(CompEntities entities, User user, DeleteUserDTO input, bool forceAll)
		{
			// First inactivate user
			DeleteUserInactivate(user);

			// Remove personal information based on user selections

			if (input.RemoveInfoAddress || forceAll)
			{
				#region Adresser

				if (!user.ContactPersonReference.IsLoaded)
					user.ContactPersonReference.Load();

				Contact contact = (from c in entities.Contact.Include("ContactAddress.ContactAddressRow")
								   where c.Actor.ActorId == user.ContactPerson.ActorContactPersonId
								   select c).FirstOrDefault();

				if (contact == null)
					return;

				foreach (ContactAddress address in contact.ContactAddress.ToList())
				{
					foreach (ContactAddressRow row in address.ContactAddressRow.ToList())
					{
						entities.DeleteObject(row);
					}
					entities.DeleteObject(address);
				}

				#endregion
			}
			if (input.RemoveInfoPhone || input.RemoveInfoEmail || input.RemoveInfoClosestRelative || input.RemoveInfoOtherContactInfo || forceAll)
			{
				#region Telefonnummer, E-postadresser, Närmast anhöriga, Övrig kontaktinformation

				if (!user.ContactPersonReference.IsLoaded)
					user.ContactPersonReference.Load();

				Contact contact = (from c in entities.Contact.Include("ContactECom")
								   where c.Actor.ActorId == user.ContactPerson.ActorContactPersonId
								   select c).FirstOrDefault();

				if (contact == null)
					return;

				List<int> ecomTypes = new List<int>();
				if (input.RemoveInfoPhone || forceAll)
				{
					ecomTypes.Add((int)TermGroup_SysContactEComType.PhoneHome);
					ecomTypes.Add((int)TermGroup_SysContactEComType.PhoneJob);
					ecomTypes.Add((int)TermGroup_SysContactEComType.PhoneMobile);
					ecomTypes.Add((int)TermGroup_SysContactEComType.Fax);
				}
				if (input.RemoveInfoEmail || forceAll)
				{
					ecomTypes.Add((int)TermGroup_SysContactEComType.Email);
					ecomTypes.Add((int)TermGroup_SysContactEComType.CompanyAdminEmail);
				}
				if (input.RemoveInfoClosestRelative || forceAll)
				{
					ecomTypes.Add((int)TermGroup_SysContactEComType.ClosestRelative);
				}
				if (input.RemoveInfoOtherContactInfo || forceAll)
				{
					ecomTypes.Add((int)TermGroup_SysContactEComType.Web);
					ecomTypes.Add((int)TermGroup_SysContactEComType.Coordinates);
					ecomTypes.Add((int)TermGroup_SysContactEComType.IndividualTaxNumber);
					ecomTypes.Add((int)TermGroup_SysContactEComType.GlnNumber);
				}

				foreach (ContactECom ecom in contact.ContactECom.Where(c => ecomTypes.Contains(c.SysContactEComTypeId)).ToList())
				{
					entities.DeleteObject(ecom);
				}

				#endregion
			}
		}

		private void DeleteUserUnidentify(CompEntities entities, User user, DeleteUserDTO input)
		{
			// First remove personal information
			DeleteUserRemoveInfo(entities, user, input, true);

			// Set user as deleted
			user.State = (int)SoeEntityState.Deleted;
			SetDeletedProperties(user);

			// TODO: A job will do the actual unidentify?
			// user.JobDate = null;
		}

		private void DeleteUserDelete(CompEntities entities, User user, DeleteUserDTO input)
		{
			// Currently same as unidentify
			DeleteUserUnidentify(entities, user, input);
		}

		#endregion

		#region User credentials / SoftOne online

		public static string GetUserCacheCredentials(User user)
		{
			if (user == null)
				return String.Empty;

			return Constants.CACHE_USERCACHECREDENTIALS_PREFIX + user.UserId + "_" + user.LoginName;
		}

		public static string GetUserCacheCredentials(UserDTO user)
		{
			if (user == null)
				return String.Empty;

			return Constants.CACHE_USERCACHECREDENTIALS_PREFIX + user.UserId + "_" + user.LoginName;
		}

		public ParameterClaimsObjectDTO GetParameterClaimsObjectDTO(Guid idLoginGuid, out User user, int licenseId = 0)
		{
			user = null;

			#region ParameterClaimsObjectDTO

			ParameterClaimsObjectDTO parameterClaimsObjectDTO = new ParameterClaimsObjectDTO();

			using (CompEntities entities = new CompEntities())
			{
				var users = (from u in entities.User
								.Include("license")
							 where u.State == (int)SoeEntityState.Active &&
							 u.idLoginGuid.HasValue &&
							 u.idLoginGuid == idLoginGuid
							 select u).ToList();

				if (licenseId != 0 && users.Count > 1)
					user = users.FirstOrDefault(f => f.LicenseId == licenseId);
				if (user == null)
					user = users.FirstOrDefault();

				if (user != null)
				{
					if (!TryGetUserDefaultRoleAndCompany(entities, user, false, out Company company, out Role role))
						return null;

					user.SetRole(role);

					parameterClaimsObjectDTO.ActorCompanyId = company.ActorCompanyId;
					parameterClaimsObjectDTO.RoleId = role.RoleId;
					parameterClaimsObjectDTO.UserId = user.UserId;
					parameterClaimsObjectDTO.UserName = user.LoginName;
					parameterClaimsObjectDTO.IdUserGuid = idLoginGuid;
				}
			}

			return parameterClaimsObjectDTO;

			#endregion
		}

		public List<Guid> GetIdLoginGuidsWhereMandatoryInformationSetting()
		{
			List<Guid> guids = new List<Guid>();
			List<int> ids = SettingManager.GetCompanyIdsWithCompanyBoolSetting(CompanySettingType.UseMissingMandatoryInformation, true);

			foreach (var id in ids)
			{
				List<User> users = GetUsersByCompany(id, 0, 0, active: true);
				guids.AddRange(users.Where(w => w.idLoginGuid.HasValue).Select(s => s.idLoginGuid.Value).ToList());
			}

			if (!ids.Any())
				guids.Add(Guid.Empty);

			return guids;
		}

		public void AddMandatoryInformationToUserInfoDTO(UserInfoDTO userInfoDTO, int userId)
		{
			using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();

			var companyRoles = entitiesReadOnly.UserCompanyRole.Where(w => w.UserId == userId && !w.Company.Demo).ToList();
			if (companyRoles.IsNullOrEmpty())
				return;

			var actorCompanyIds = companyRoles.Select(s => s.ActorCompanyId).ToList();
			var companies = entitiesReadOnly.Company.Where(w => !w.Demo && actorCompanyIds.Contains(w.ActorCompanyId)).ToList();
			if (companies.IsNullOrEmpty())
				return;

			using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
			List<SysContactEComType> eComRowTypes = sysEntitiesReadOnly.SysContactEComType.ToList();
			List<SysContactAddressRowType> addressRowTypes = sysEntitiesReadOnly.SysContactAddressRowType.ToList();

			foreach (var company in companies)
			{
				if (!userInfoDTO.MissingMandatoryInformation.IsNullOrEmpty() || company.Demo)
					continue;

				var useSetting = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.UseMissingMandatoryInformation, 0, company.ActorCompanyId, 0);
				if (useSetting)
				{
					var infos = new List<MissingMandatoryInformation>();
					var employee = EmployeeManager.GetEmployeeByUser(company.ActorCompanyId, userId);
					User user = null;
					Contact contact = null;

					if (employee == null)
					{
						user = UserManager.GetUserIgnoreState(userId, loadContactPerson: true);
						if (user != null)
							contact = ContactManager.GetContactFromActor(user.ContactPerson.ActorContactPersonId, loadActor: true, loadAllContactInfo: true);
					}
					else
					{
						contact = ContactManager.GetContactFromActor(employee.ContactPersonId, loadActor: true, loadAllContactInfo: true);
					}

					if (contact == null)
						continue;

					string checkboxName = GetText(2903, (int)TermGroup.AngularCommon, "Visa enbart för chef");

					List<ContactECom> eComs = contact.ContactECom?.ToList() ?? new List<ContactECom>();

					if (!eComs.Any(w => w.SysContactEComTypeId == (int)TermGroup_SysContactEComType.Email))
						infos.Add(new MissingMandatoryInformation() { ActorCompanyId = company.ActorCompanyId, CheckBox = false, CheckBoxName = checkboxName, Group = "", Mandatory = true, Type = SetMissingMandatoyInformationSysContactEComType(TermGroup_SysContactEComType.Email), Name = GetEComRowTypeName(eComRowTypes, TermGroup_SysContactEComType.Email) });

					if (!eComs.Any(w => w.SysContactEComTypeId == (int)TermGroup_SysContactEComType.PhoneMobile))
						infos.Add(new MissingMandatoryInformation() { ActorCompanyId = company.ActorCompanyId, CheckBox = false, CheckBoxName = checkboxName, Group = "", Mandatory = true, Type = SetMissingMandatoyInformationSysContactEComType(TermGroup_SysContactEComType.PhoneMobile), Name = GetEComRowTypeName(eComRowTypes, TermGroup_SysContactEComType.PhoneMobile) });

					if (!eComs.Any(w => w.SysContactEComTypeId == (int)TermGroup_SysContactEComType.PhoneHome))
						infos.Add(new MissingMandatoryInformation() { ActorCompanyId = company.ActorCompanyId, CheckBox = false, CheckBoxName = checkboxName, Group = "", Mandatory = true, Type = SetMissingMandatoyInformationSysContactEComType(TermGroup_SysContactEComType.PhoneHome), Name = GetEComRowTypeName(eComRowTypes, TermGroup_SysContactEComType.PhoneHome) });

					if (!eComs.Any(w => w.SysContactEComTypeId == (int)TermGroup_SysContactEComType.ClosestRelative))
						infos.Add(new MissingMandatoryInformation() { ActorCompanyId = company.ActorCompanyId, CheckBox = false, CheckBoxName = checkboxName, Group = "", Mandatory = true, Type = SetMissingMandatoyInformationSysContactEComType(TermGroup_SysContactEComType.ClosestRelative), Name = GetEComRowTypeName(eComRowTypes, TermGroup_SysContactEComType.ClosestRelative) });

					if (contact.ContactAddress != null)
					{
						string group = company.Name + ": " + GetText(11, "Utdelningsadress");
						var address = contact.ContactAddress.FirstOrDefault(w => w.SysContactAddressTypeId == (int)TermGroup_SysContactAddressType.Distribution);
						if (address == null || address.ContactAddressRow.IsNullOrEmpty())
						{
							infos.Add(new MissingMandatoryInformation() { ActorCompanyId = company.ActorCompanyId, CheckBox = false, CheckBoxName = checkboxName, Group = group, Mandatory = true, Type = SetMissingMandatoyInformationSysContactAddressRowType(TermGroup_SysContactAddressRowType.StreetAddress), Name = GetAddressRowTypeName(addressRowTypes, TermGroup_SysContactAddressRowType.StreetAddress) });
							infos.Add(new MissingMandatoryInformation() { ActorCompanyId = company.ActorCompanyId, CheckBox = false, CheckBoxName = checkboxName, Group = group, Mandatory = true, Type = SetMissingMandatoyInformationSysContactAddressRowType(TermGroup_SysContactAddressRowType.AddressCO), Name = GetAddressRowTypeName(addressRowTypes, TermGroup_SysContactAddressRowType.AddressCO) });
							infos.Add(new MissingMandatoryInformation() { ActorCompanyId = company.ActorCompanyId, CheckBox = false, CheckBoxName = checkboxName, Group = group, Mandatory = true, Type = SetMissingMandatoyInformationSysContactAddressRowType(TermGroup_SysContactAddressRowType.PostalCode), Name = GetAddressRowTypeName(addressRowTypes, TermGroup_SysContactAddressRowType.PostalCode) });
							infos.Add(new MissingMandatoryInformation() { ActorCompanyId = company.ActorCompanyId, CheckBox = false, CheckBoxName = checkboxName, Group = group, Mandatory = true, Type = SetMissingMandatoyInformationSysContactAddressRowType(TermGroup_SysContactAddressRowType.PostalAddress), Name = GetAddressRowTypeName(addressRowTypes, TermGroup_SysContactAddressRowType.PostalAddress) });
						}
						else
						{
							if (!address.ContactAddressRow.Any(w => w.SysContactAddressRowTypeId == (int)TermGroup_SysContactAddressRowType.StreetAddress))
								infos.Add(new MissingMandatoryInformation() { ActorCompanyId = company.ActorCompanyId, CheckBox = false, CheckBoxName = checkboxName, Group = group, Mandatory = true, Type = SetMissingMandatoyInformationSysContactAddressRowType(TermGroup_SysContactAddressRowType.StreetAddress), Name = GetAddressRowTypeName(addressRowTypes, TermGroup_SysContactAddressRowType.Address) });
							if (!address.ContactAddressRow.Any(w => w.SysContactAddressRowTypeId == (int)TermGroup_SysContactAddressRowType.PostalCode))
								infos.Add(new MissingMandatoryInformation() { ActorCompanyId = company.ActorCompanyId, CheckBox = false, CheckBoxName = checkboxName, Group = group, Mandatory = true, Type = SetMissingMandatoyInformationSysContactAddressRowType(TermGroup_SysContactAddressRowType.PostalCode), Name = GetAddressRowTypeName(addressRowTypes, TermGroup_SysContactAddressRowType.PostalCode) });
							if (!address.ContactAddressRow.Any(w => w.SysContactAddressRowTypeId == (int)TermGroup_SysContactAddressRowType.PostalAddress))
								infos.Add(new MissingMandatoryInformation() { ActorCompanyId = company.ActorCompanyId, CheckBox = false, CheckBoxName = checkboxName, Group = group, Mandatory = true, Type = SetMissingMandatoyInformationSysContactAddressRowType(TermGroup_SysContactAddressRowType.PostalAddress), Name = GetAddressRowTypeName(addressRowTypes, TermGroup_SysContactAddressRowType.PostalAddress) });
						}
					}
					if (infos.Any())
						userInfoDTO.MissingMandatoryInformation = infos;
				}
			}
		}

		public int GetUserIdFromCache(string cacheItem)
		{
			int userId = 0;

			if (!String.IsNullOrEmpty(cacheItem) && cacheItem.StartsWith(Constants.CACHE_USERCACHECREDENTIALS_PREFIX))
			{
				//Dependent on the User.CacheCredentials value
				string[] arr = cacheItem.Split('_');
				if (arr != null)
					Int32.TryParse(arr[1], out userId);
			}

			return userId;
		}

		/// <summary>
		/// Password matching expression. 
		/// - must include at least one upper case letter, one lower case letter, and one numeric digit
		/// - must be between 6 and 20 characters (Company setting min/max)
		/// </summary>
		/// <param name="password">The password</param>
		/// <returns>True if the password is strong</returns>
		public bool IsPasswordStrong(string password)
		{
			int passwordMinLength = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.CorePasswordMinLength, 0, base.ActorCompanyId, 0);
			if (passwordMinLength == 0)
				passwordMinLength = Constants.PASSWORD_DEFAULT_MIN_LENGTH;
			int passwordMaxLength = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.CorePasswordMaxLength, 0, base.ActorCompanyId, 0);
			if (passwordMaxLength == 0)
				passwordMaxLength = Constants.PASSWORD_DEFAULT_MAX_LENGTH;

			string expression = @"^(?=.*\d)(?=.*[a-zåäö])(?=.*[A-ZÅÄÖ]).{" + passwordMinLength + "," + passwordMaxLength + "}$";
			return Regex.IsMatch(password, expression);
		}

		public bool HasIdLoginGuidsWithMultipleUsers(Guid idLoginGuid)
		{
			var fromCache = BusinessMemoryCache<bool?>.Get("HasIdLoginGuidsWithMultipleUsers" + idLoginGuid.ToString());
			if (fromCache.HasValue)
				return fromCache.Value;

			var fromDb = GetIdLoginGuidsWithMultipleUsers().Contains(idLoginGuid);
			BusinessMemoryCache<bool?>.Set("HasIdLoginGuidsWithMultipleUsers" + idLoginGuid.ToString(), fromDb, (fromDb ? 60 * 10 : 60 * 3));
			return fromDb;
		}

		public ActionResult SendActivationEmail(List<int> userIds)
		{
			ActionResult result = new ActionResult();

			foreach (int userId in userIds)
			{
				User user = GetUser(userId);
				ActionResult userResult = AddUserToSoftOneId(user, fireAndForget: false);
				if (!userResult.Success)
				{
					result.Success = false;
					if (result.Strings == null)
						result.Strings = new List<string>();
					result.Strings.Add(user.Email);
				}
			}

			return result;
		}

		public ActionResult SendForgottenUsername(List<int> userIds)
		{
			ActionResult result = new ActionResult();
			int successful = 0;
			int unSuccesful = 0;
			List<string> unsuccessfulNames = new List<string>();


			foreach (int userId in userIds)
			{
				User user = GetUser(userId);

				if (user != null && user.idLoginGuid != null && user.idLoginGuid != Guid.Empty)
				{
					var res = SoftOneIdConnector.SendForgottenUsername(user.idLoginGuid.Value);
					if (res.Success)
						successful++;
					else
					{
						unSuccesful++;
						unsuccessfulNames.Add(user.LoginName);
					}
				}
			}

			result.Success = unSuccesful == 0;
			result.ErrorMessage = unSuccesful > 0 ? $"{unSuccesful}/{successful + unSuccesful} {GetText(11949, 1)} {string.Join(",", unsuccessfulNames)}" : string.Empty;

			return result;
		}

		public ActionResult SaveMandatoryInformationFromUserInfoDTO(UserInfoDTO userInfoDTO)
		{
			if (userInfoDTO == null || userInfoDTO.MissingMandatoryInformation.IsNullOrEmpty())
				return new ActionResult(false);

			var user = UserManager.GetUserIgnoreState(userInfoDTO.UserId, loadContactPerson: true);
			if (user == null)
			{
				user = UserManager.GetUser(userInfoDTO.IdLoginGuid);
				user.ContactPersonReference.Load();
			}

			if (user.ContactPerson == null)
				return new ActionResult(false);

			var current = GetUserInfoDTO(userInfoDTO.IdLoginGuid, checkMissingMandatoryInformation: true);
			if (current == null)
				return new ActionResult(false);

			List<string> validTypes = new List<string>();
			if (!current.MissingMandatoryInformation.IsNullOrEmpty())
			{
				foreach (var item in current.MissingMandatoryInformation)
					validTypes.Add(item.Type);
			}

			if (!validTypes.Any())
				return new ActionResult();

			using (CompEntities entities = new CompEntities())
			{
				var contact = ContactManager.GetContactFromActor(entities, user.ContactPerson.ActorContactPersonId, loadActor: true, loadAllContactInfo: true);
				if (contact == null)
					return new ActionResult(false);

				var contactAddresses = ContactManager.GetContactAddresses(entities, contact.ContactId);
				var distributionAddress = contactAddresses.FirstOrDefault(f => f.SysContactAddressTypeId == (int)TermGroup_SysContactAddressType.Distribution);

				if (distributionAddress == null)
				{
					distributionAddress = new ContactAddress()
					{
						SysContactAddressTypeId = (int)TermGroup_SysContactAddressType.Distribution,
						Contact = contact,
						Name = GetText((int)TermGroup_SysContactAddressType.Distribution, (int)TermGroup.SysContactAddressType),
					};

					entities.ContactAddress.AddObject(distributionAddress);
				}

				foreach (var into in userInfoDTO.MissingMandatoryInformation)
				{
					if (!validTypes.Any(a => a == into.Type) || !into.Value.HasValue())
						continue;

					if (GetSysContactEComType(into.Type) == TermGroup_SysContactEComType.Email)
						ContactManager.AddContactECom(entities, contact, (int)TermGroup_SysContactEComType.Email, into.Value, null, false, "", into.CheckBox);
					else if (GetSysContactEComType(into.Type) == TermGroup_SysContactEComType.PhoneHome)
						ContactManager.AddContactECom(entities, contact, (int)TermGroup_SysContactEComType.PhoneHome, into.Value, null, false, "", into.CheckBox);
					else if (GetSysContactEComType(into.Type) == TermGroup_SysContactEComType.PhoneMobile)
						ContactManager.AddContactECom(entities, contact, (int)TermGroup_SysContactEComType.PhoneMobile, into.Value, null, false, "", into.CheckBox);
					else if (GetSysContactEComType(into.Type) == TermGroup_SysContactEComType.ClosestRelative)
					{
						if (into.Value.Contains(";"))
						{
							string phone = string.Empty;
							string relation = string.Empty;
							string name = string.Empty;
							string description = string.Empty;

							if (into.Value.Split(';').Count() == 3)
							{
								name = into.Value.Split(';')[0];
								relation = into.Value.Split(';')[1];
								phone = into.Value.Split(';')[2];
								description = $"{name};{relation}";

								ContactManager.AddContactECom(entities, contact, (int)TermGroup_SysContactEComType.ClosestRelative, phone, null, description: description, isSecret: into.CheckBox);
							}
						}
						else
						{
							ContactManager.AddContactECom(entities, contact, (int)TermGroup_SysContactEComType.ClosestRelative, into.Value, null, false, "", into.CheckBox);
						}
					}

					if (GetSysContactAddressRowType(into.Type) == TermGroup_SysContactAddressRowType.Address)
						entities.ContactAddressRow.AddObject(new ContactAddressRow() { SysContactAddressRowTypeId = (int)TermGroup_SysContactAddressRowType.Address, Text = into.Value, RowNr = 1, ContactAddress = distributionAddress });

					if (GetSysContactAddressRowType(into.Type) == TermGroup_SysContactAddressRowType.StreetAddress)
					{
						entities.ContactAddressRow.AddObject(new ContactAddressRow() { SysContactAddressRowTypeId = (int)TermGroup_SysContactAddressRowType.StreetAddress, Text = into.Value, RowNr = 1, ContactAddress = distributionAddress });
						distributionAddress.IsSecret = into.CheckBox;
					}

					if (GetSysContactAddressRowType(into.Type) == TermGroup_SysContactAddressRowType.AddressCO)
						entities.ContactAddressRow.AddObject(new ContactAddressRow() { SysContactAddressRowTypeId = (int)TermGroup_SysContactAddressRowType.AddressCO, Text = into.Value, RowNr = 0, ContactAddress = distributionAddress });

					if (GetSysContactAddressRowType(into.Type) == TermGroup_SysContactAddressRowType.PostalCode)
						entities.ContactAddressRow.AddObject(new ContactAddressRow() { SysContactAddressRowTypeId = (int)TermGroup_SysContactAddressRowType.PostalCode, Text = into.Value, RowNr = 2, ContactAddress = distributionAddress });

					if (GetSysContactAddressRowType(into.Type) == TermGroup_SysContactAddressRowType.PostalAddress)
						entities.ContactAddressRow.AddObject(new ContactAddressRow() { SysContactAddressRowTypeId = (int)TermGroup_SysContactAddressRowType.PostalAddress, Text = into.Value, RowNr = 3, ContactAddress = distributionAddress });
				}

				return SaveChanges(entities);

			}

		}

		public ActionResult AddUserToSoftOneId(User user, bool fireAndForget = true, string externalAuthId = null)
		{
			if (user == null || string.IsNullOrEmpty(user.Email))
				return new ActionResult(true);

			int? sysCompDbId = SysServiceManager.GetSysCompDBId();
			bool validEmailForTest = user.Email.ToLower().Contains("softone.se") || user.Email.ToLower().Contains("softone.fi") || user.Email.StartsWith("ValidForGoTest.");
			bool isTestSite = CompDbCache.Instance.SiteType == TermGroup_SysPageStatusSiteType.Test;
			bool addToIdLogin = !isTestSite || validEmailForTest;

			if (!addToIdLogin) return new ActionResult();

			try
			{
				if (fireAndForget)
				{
					Task.Run(() => SoftOneIdConnector.AddIdLogin(user.idLoginGuid.Value, user.Email, user.EstatusLoginId, user.LicenseId, sysCompDbId.GetValueOrDefault(), externalAuthId, user.LangId));
					return new ActionResult();
				}

				return SoftOneIdConnector.AddIdLogin(user.idLoginGuid.Value, user.Email, user.EstatusLoginId, user.LicenseId, sysCompDbId.GetValueOrDefault(), externalAuthId, user.LangId);
			}
			catch
			{
				return new ActionResult(false);
			}
		}

		#region Help-methods

		private void SetUserExternalAuthId(int licenseId, List<User> users)
		{
			if (users.IsNullOrEmpty())
				return;

			if (FeatureManager.HasRolePermission(Feature.Manage_Preferences_LicenseSettings, Permission.Modify, base.RoleId, base.ActorCompanyId))
			{
				string providerSetting = SettingManager.GetStringSetting(SettingMainType.License, (int)LicenseSettingType.SSO_Key, 0, 0, licenseId);
				if (!string.IsNullOrEmpty(providerSetting) && Guid.TryParse(providerSetting, out Guid providerGuid))
				{
					int? sysCompDbId = SysServiceManager.GetSysCompDBId();
					if (sysCompDbId.HasValue)
					{
						foreach (var confidential in SoftOneIdConnector.GetExternalAuthIds(providerGuid, licenseId, sysCompDbId.Value))
						{
							var user = users.FirstOrDefault(f => f.idLoginGuid == confidential.IdLoginGuid);
							if (user != null)
							{
								user.ExternalAuthId = confidential.Confidential;
							}
						}
					}
				}
			}
		}

		private void SetUserSoftOneIdLoginName(int licenseId, List<User> users)
		{
			if (users.IsNullOrEmpty())
				return;

			var requestingUser = UserManager.GetUser(base.UserId);

			if (requestingUser?.idLoginGuid == null)
				return;

			if (FeatureManager.HasRolePermission(Feature.Manage_Users_Edit, Permission.Modify, base.RoleId, base.ActorCompanyId))
			{
				int? sysCompDbId = ConfigurationSetupUtil.GetCurrentSysCompDbId();
				if (sysCompDbId.HasValue)
				{
					foreach (var confidential in SoftOneIdConnector.GetLoginNames(requestingUser.idLoginGuid.Value, licenseId, sysCompDbId.Value))
					{
						var user = users.FirstOrDefault(f => f.idLoginGuid == confidential.IdLoginGuid);
						if (user != null)
						{
							user.SoftOneIdLoginName = confidential.UserName;
						}
					}
				}
			}
		}

		private string SetMissingMandatoyInformationSysContactEComType(TermGroup_SysContactEComType type)
		{
			return "TermGroup_SysContactEComType." + Enum.GetName(typeof(TermGroup_SysContactEComType), type);
		}

		private string SetMissingMandatoyInformationSysContactAddressRowType(TermGroup_SysContactAddressRowType type)
		{
			return "TermGroup_SysContactAddressRowType." + Enum.GetName(typeof(TermGroup_SysContactAddressRowType), type);
		}

		private string GetEComRowTypeName(List<SysContactEComType> eComRowTypes, TermGroup_SysContactEComType type)
		{
			var eComRowType = eComRowTypes.FirstOrDefault(r => r.SysContactEComTypeId == (int)type);
			if (eComRowType == null)
				return string.Empty;

			return GetText(eComRowType.SysTermId, (int)TermGroup.SysContactEComType);
		}

		private string GetAddressRowTypeName(List<SysContactAddressRowType> addressRowTypes, TermGroup_SysContactAddressRowType type)
		{
			var addressRowType = addressRowTypes.FirstOrDefault(r => r.SysContactAddressRowTypeId == (int)type);
			if (addressRowType == null)
				return string.Empty;

			return GetText(addressRowType.SysTermId, (int)TermGroup.SysContactAddressRowType);
		}

		private TermGroup_SysContactEComType GetSysContactEComType(string missingMandatoyInformationType)
		{
			if (!missingMandatoyInformationType.Contains('.'))
				return TermGroup_SysContactEComType.Unknown;

			var enumValue = missingMandatoyInformationType.Split('.')[1];
			if (Enum.TryParse(enumValue, out TermGroup_SysContactEComType enumType))
				return enumType;
			else
				return TermGroup_SysContactEComType.Unknown;
		}

		private TermGroup_SysContactAddressRowType GetSysContactAddressRowType(string missingMandatoyInformationType)
		{
			if (!missingMandatoyInformationType.Contains('.'))
				return TermGroup_SysContactAddressRowType.Unknown;

			string enumValue = missingMandatoyInformationType.Split('.')[1];
			if (Enum.TryParse(enumValue, out TermGroup_SysContactAddressRowType enumType))
				return enumType;
			else
				return TermGroup_SysContactAddressRowType.Unknown;
		}

		#endregion

		#endregion

		#region UserCompanyRole

		public List<UserCompanyRole> GetUserCompanyRolesByUser(int userId, bool onlyActiveUser = true, bool loadUser = false, bool loadCompany = false, bool loadRole = false, bool setNames = false, DateTime? date = null, bool ignoreDate = false)
		{
			using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
			entities.UserCompanyRole.NoTracking();
			return GetUserCompanyRolesByUser(entities, userId, onlyActiveUser, loadUser, loadCompany, loadRole, setNames, date, ignoreDate);
		}

		public List<UserCompanyRole> GetUserCompanyRolesByUser(CompEntities entities, int userId, bool onlyActiveUser = true, bool loadUser = false, bool loadCompany = false, bool loadRole = false, bool setNames = false, DateTime? date = null, bool ignoreDate = false)
		{
			if (!date.HasValue && !ignoreDate)
				date = DateTime.Today;

			IQueryable<UserCompanyRole> query = (from ucr in entities.UserCompanyRole
												 where ucr.UserId == userId &&
												 (onlyActiveUser ? ucr.User.State == (int)SoeEntityState.Active : ucr.User.State != (int)SoeEntityState.Deleted) &&
												  ucr.State == (int)SoeEntityState.Active &&
												(ignoreDate || !ucr.DateFrom.HasValue || ucr.DateFrom <= date.Value) &&
												(ignoreDate || !ucr.DateTo.HasValue || ucr.DateTo >= date.Value) &&
												 ucr.Company.State == (int)SoeEntityState.Active
												 select ucr);

			if (loadUser)
				query = query.Include("User");
			if (loadCompany)
				query = query.Include("Company.License");
			if (loadRole)
				query = query.Include("Role");

			if (setNames)
			{
				List<UserCompanyRole> userCompanyRoles = query.ToList();
				foreach (Role role in userCompanyRoles.Select(ucr => ucr.Role))
				{
					role.Name = RoleManager.GetRoleNameText(role);
				}
				return userCompanyRoles;
			}
			else
			{
				return query.ToList();
			}
		}

		public List<UserCompanyRole> GetUserCompanyRolesByUserAndCompany(int userId, int actorCompanyId, bool loadUser = false, bool loadCompany = false, bool loadRole = false, DateTime? date = null)
		{
			using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();

			entitiesReadOnly.UserCompanyRole.NoTracking();
			var result = GetUserCompanyRolesByUserAndCompany(entitiesReadOnly, userId, actorCompanyId, loadUser, loadCompany, loadRole, date);
			return result;
		}

		public List<UserCompanyRole> GetUserCompanyRolesByUserAndCompany(CompEntities entities, int userId, int actorCompanyId, bool loadUser = false, bool loadCompany = false, bool loadRole = false, DateTime? date = null)
		{
			if (!date.HasValue)
				date = DateTime.Today;

			IQueryable<UserCompanyRole> query = (from ucr in entities.UserCompanyRole
												 where ucr.UserId == userId &&
												 ucr.ActorCompanyId == actorCompanyId &&
												 ucr.State == (int)SoeEntityState.Active &&
												(!ucr.DateFrom.HasValue || ucr.DateFrom <= date.Value) &&
												(!ucr.DateTo.HasValue || ucr.DateTo >= date.Value) &&
												 ucr.User.State == (int)SoeEntityState.Active &&
												 ucr.Company.State == (int)SoeEntityState.Active
												 select ucr);
			if (loadUser)
				query = query.Include("User");
			if (loadCompany)
				query = query.Include("Company.License");
			if (loadRole)
				query = query.Include("Role");

			return query.ToList();
		}

		public List<UserCompanyRole> GetUserCompanyRolesForCompany(int actorCompanyId, bool loadUser = false, bool loadCompany = false, bool loadRole = false, DateTime? date = null)
		{
			using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
			entities.UserCompanyRole.NoTracking();
			return GetUserCompanyRolesForCompany(entities, actorCompanyId, loadUser, loadCompany, loadRole, date);
		}

		public List<UserCompanyRole> GetUserCompanyRolesForCompany(CompEntities entities, int actorCompanyId, bool loadUser = false, bool loadCompany = false, bool loadRole = false, DateTime? date = null)
		{
			if (!date.HasValue)
				date = DateTime.Today;

			IQueryable<UserCompanyRole> query = (from ucr in entities.UserCompanyRole
												 where ucr.ActorCompanyId == actorCompanyId &&
												  ucr.State == (int)SoeEntityState.Active &&
												(!ucr.DateFrom.HasValue || ucr.DateFrom <= date.Value) &&
												(!ucr.DateTo.HasValue || ucr.DateTo >= date.Value)
												 select ucr);
			if (loadUser)
				query = query.Include("User");
			if (loadCompany)
				query = query.Include("Company.License");
			if (loadRole)
				query = query.Include("Role");

			return query.ToList();
		}

		public UserCompanyRole GetUserCompanyRole(CompEntities entities, int userId, int actorCompanyId, int userCompanyRoleId)
		{
			return entities.UserCompanyRole.FirstOrDefault(f => f.UserCompanyRoleId == userCompanyRoleId && f.UserId == userId && f.ActorCompanyId == actorCompanyId);
		}

		public UserCompanyRole GetUserCompanyRoleMapping(int userId, int actorCompanyId, DateTime? date = null)
		{
			if (!date.HasValue)
				date = DateTime.Today;

			using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
			entities.UserCompanyRole.NoTracking();
			return (from ucr in entities.UserCompanyRole
					where ucr.UserId == userId &&
					ucr.State == (int)SoeEntityState.Active &&
					(!ucr.DateFrom.HasValue || ucr.DateFrom <= date.Value) &&
					(!ucr.DateTo.HasValue || ucr.DateTo >= date.Value) &&
					ucr.ActorCompanyId == actorCompanyId
					select ucr).FirstOrDefault();
		}

		public UserCompanyRoleDTO GetSelectedDefaultRole(List<UserRolesDTO> userRoles)
		{
			if (userRoles.IsNullOrEmpty())
				return null;

			foreach (UserRolesDTO userRole in userRoles)
			{
				// First check roles without startDate intervals
				UserCompanyRoleDTO role = userRole.Roles?.FirstOrDefault(r => r.Default && !r.DateFrom.HasValue && !r.DateTo.HasValue);
				if (role != null)
					return role;

				role = userRole.Roles?.FirstOrDefault(r => r.Default);
				if (role != null)
					return role;
			}

			return null;
		}

		public List<UserRolesDTO> GetUserRolesDTO(int userId, bool ignoreDate)
		{
			using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
			return GetUserRolesDTO(entities, userId, ignoreDate);
		}

		public List<UserRolesDTO> GetUserRolesDTO(CompEntities entities, int userId, bool ignoreDate)
		{
			List<UserRolesDTO> dtos = new List<UserRolesDTO>();

			User user = GetUser(entities, userId, onlyActive: false);
			if (user != null)
			{
				List<UserCompanyRole> userCompanyRoles = GetUserCompanyRolesByUser(entities, userId, onlyActiveUser: false, loadRole: true, setNames: true, ignoreDate: ignoreDate);
				List<AttestRoleUser> attestRoleUsers = AttestManager.GetAttestRoleUsersByLicense(entities, userId, user.LicenseId, includeAttestRole: true, includeAccountAndChildren: true);

				List<int> companyIds = new List<int>();
				companyIds.AddRange(userCompanyRoles.Select(r => r.ActorCompanyId).Distinct());
				companyIds.AddRange(attestRoleUsers.Select(r => r.AttestRole.ActorCompanyId).Distinct());

				if (!companyIds.IsNullOrEmpty())
				{
					List<Company> companies = GetCompaniesByLicenseFromCache(entities, user.LicenseId);
					List<GenericType> permissionTypes = base.GetTermGroupContent(TermGroup.AttestRoleUserAccountPermissionType);

					foreach (int companyId in companyIds.Distinct())
					{
						UserRolesDTO dto = new UserRolesDTO()
						{
							ActorCompanyId = companyId,
							CompanyName = companies.GetName(companyId),
							DefaultCompany = companyId == user.DefaultActorCompanyId
						};

						dto.Roles = userCompanyRoles
							.Where(r => r.ActorCompanyId == companyId)
							.ToDTOs();

						dto.AttestRoles = attestRoleUsers
							.Where(r => r.AttestRole.ActorCompanyId == companyId && !r.ParentAttestRoleUserId.HasValue)
							.ToDTOs(GetAccountDimsFromCache(entities, CacheConfig.Company(companyId)), permissionTypes, false);

						dtos.Add(dto);
					}
				}
			}

			dtos = dtos
				.OrderByDescending(d => d.DefaultCompany)
				.ThenBy(d => d.CompanyName)
				.ToList();

			return dtos;
		}

		public Role GetDefaultRole(int actorCompanyId, int userId, DateTime? date = null, List<UserCompanyRoleDTO> userCompanyRoles = null, List<Role> roles = null)
		{
			using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
			return GetDefaultRole(entities, actorCompanyId, userId, date, userCompanyRoles, roles);
		}

		public Role GetDefaultRole(CompEntities entities, int actorCompanyId, int userId, DateTime? date = null, List<UserCompanyRoleDTO> userCompanyRoles = null, List<Role> roles = null)
		{
			int defaultRoleId = GetDefaultRoleId(entities, actorCompanyId, userId, date, userCompanyRoles);
			if (defaultRoleId <= 0)
				return null;

			return roles?.FirstOrDefault(i => i.RoleId == defaultRoleId) ?? RoleManager.GetRole(entities, defaultRoleId, actorCompanyId);
		}

		public int GetDefaultRoleId(int actorCompanyId, User user, DateTime? date = null, List<UserCompanyRoleDTO> userCompanyRoles = null)
		{
			using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
			return GetDefaultRoleId(entities, actorCompanyId, user, date, userCompanyRoles);
		}

		public int GetDefaultRoleId(CompEntities entities, int actorCompanyId, User user, DateTime? date = null, List<UserCompanyRoleDTO> userCompanyRoles = null)
		{
			if (user == null)
				return 0;

			if (userCompanyRoles == null && user.UserCompanyRole.IsLoaded)
				userCompanyRoles = user.UserCompanyRole.ToDTOs().ToList();

			return GetDefaultRoleId(entities, actorCompanyId, user.UserId, date, userCompanyRoles, legacyDefaultRoleId: user.DefaultRoleId);
		}

		public int GetDefaultRoleId(int actorCompanyId, int userId, DateTime? date = null, List<UserCompanyRoleDTO> userCompanyRoles = null)
		{
			using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
			return GetDefaultRoleId(entities, actorCompanyId, userId, date, userCompanyRoles);
		}

		public int GetDefaultRoleId(CompEntities entities, int actorCompanyId, int userId, DateTime? date = null, List<UserCompanyRoleDTO> userCompanyRoles = null, int? legacyDefaultRoleId = null)
		{
			if (userCompanyRoles != null)
				return GetDefaultRoleId(entities, userCompanyRoles, actorCompanyId, userId, date, legacyDefaultRoleId);
			else
				return GetDefaultRoleId(entities, GetUserCompanyRolesByUser(entities, userId, onlyActiveUser: true), actorCompanyId, userId, date, legacyDefaultRoleId);
		}

		public int GetDefaultRoleId<T>(CompEntities entities, List<T> userCompanyRoles, int actorCompanyId, int userId, DateTime? date, int? legacyDefaultRoleId = null) where T : IUserCompanyRole
		{
			//Temp backward-compabillity
			if (!userCompanyRoles.IsNullOrEmpty() && !userCompanyRoles.Any(i => i.Default))
				return legacyDefaultRoleId ?? (GetUser(entities, userId)?.DefaultRoleId ?? 0);

			return userCompanyRoles.GetDefaultRoleId(actorCompanyId, date);
		}

		private void SetDefaultRoleName(CompEntities entities, List<User> users)
		{
			if (users == null)
				return;

			List<User> validUsers = users.Where(i => i.DefaultActorCompanyId.HasValue).ToList();
			List<int> userIds = validUsers.Select(i => i.UserId).Distinct().ToList();
			List<UserCompanyRole> userCompanyRoles = entities.UserCompanyRole.Include("Company").Where(ucr => userIds.Contains(ucr.UserId) && ucr.State == (int)SoeEntityState.Active).ToList();
			Dictionary<int, List<UserCompanyRole>> userCompanyRolesByUser = userCompanyRoles.GroupBy(i => i.UserId).ToDictionary(k => k.Key, v => v.ToList());

			Dictionary<int, List<User>> usersByRoleId = new Dictionary<int, List<User>>();
			foreach (User user in validUsers)
			{
				List<int> defaultRoleIds = userCompanyRolesByUser
					.GetList(user.UserId)
					.Where(r => r.Default && r.Company.LicenseId == user.LicenseId && (!r.DateTo.HasValue || r.DateTo.Value >= DateTime.Today))
					.Select(r => r.RoleId)
					.ToList();

				foreach (int defaultRoleId in defaultRoleIds)
				{
					if (usersByRoleId.ContainsKey(defaultRoleId))
						usersByRoleId[defaultRoleId].Add(user);
					else
						usersByRoleId.Add(defaultRoleId, new List<User> { user });
				}
			}

			foreach (var userRoles in usersByRoleId)
			{
				string roleName = RoleManager.GetRoleName(userRoles.Key, true);
				foreach (User user in userRoles.Value)
				{
					if (!string.IsNullOrEmpty(user.DefaultRoleName))
						user.DefaultRoleName += ", " + roleName;
					else
						user.DefaultRoleName = roleName;
				}
			}
		}

		public bool ExistUserCompanyRoleMapping(int userId, int actorCompanyId, int roleId)
		{
			using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
			entities.UserCompanyRole.NoTracking();
			return ExistUserCompanyRoleMapping(entities, userId, actorCompanyId, roleId);
		}

		public bool ExistUserCompanyRoleMapping(CompEntities entities, int userId, int actorCompanyId, int roleId)
		{
			DateTime date = DateTime.Today;
			return (from ucr in entities.UserCompanyRole
					where ucr.UserId == userId &&
					ucr.ActorCompanyId == actorCompanyId &&
					ucr.RoleId == roleId &&
					ucr.State == (int)SoeEntityState.Active &&
					(!ucr.DateFrom.HasValue || ucr.DateFrom <= date) &&
					(!ucr.DateTo.HasValue || ucr.DateTo >= date)
					select ucr).Any();
		}

		public bool HasUserCompanyRole(List<UserCompanyRole> userCompanyRoles, User user, Company company, Role role)
		{
			if (userCompanyRoles == null || user == null || company == null || role == null)
				return false;

			return userCompanyRoles.Any(i => i.UserId == user.UserId && i.ActorCompanyId == company.ActorCompanyId && i.RoleId == role.RoleId);
		}

		public bool HasUserCompanyRole(List<UserCompanyRole> userCompanyRoles, UserDTO user, CompanyDTO company, Role role)
		{
			if (userCompanyRoles == null || user == null || company == null || role == null)
				return false;

			return userCompanyRoles.Any(i => i.UserId == user.UserId && i.ActorCompanyId == company.ActorCompanyId && i.RoleId == role.RoleId);
		}

		public bool UserValidOnCompany(int actorCompanyId, int userId)
		{
			using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
			return UserValidOnCompany(entities, actorCompanyId, userId);
		}
		public bool UserValidOnCompany(CompEntities entities, int actorCompanyId, int userId)
		{
			if (base.ActorCompanyId == actorCompanyId)
				return true;

			var userRoles = GetUserRolesDTO(entities, userId, false);

			return userRoles.Any(a => a.ActorCompanyId == actorCompanyId);
		}

		public ActionResult SaveUserCompanyRoles(CompEntities entities, List<UserRolesDTO> userRoles, int licenseId, User user, User currentUser)
		{
			var dict = userRoles.ToDictionary(k => k.ActorCompanyId, v => v.Roles);
			bool saveDelta = userRoles.Any(i => i.IsDeltaChange);

			return SaveUserCompanyRoles(entities, dict, licenseId, user, currentUser, saveDelta);
		}

		public ActionResult SaveUserCompanyRoles(CompEntities entities, Dictionary<int, List<UserCompanyRoleDTO>> dict, int licenseId, User user, User currentUser, bool saveDelta)
		{
			if (user == null || currentUser == null)
				return new ActionResult((int)ActionResultSave.EntityNotFound, "User");

			CompDbCache.Instance.FlushUserCompanyRoles(user.UserId);

			if (!user.UserCompanyRole.IsLoaded)
				user.UserCompanyRole.Load();

			if (saveDelta)
			{
				#region Delta save (API)

				foreach (int actorCompanyId in dict.Keys)
				{
					Company company = CompanyManager.GetCompany(entities, actorCompanyId);
					if (company == null)
						continue;

					List<UserCompanyRoleDTO> userCompanyRoles = dict[actorCompanyId];
					if (userCompanyRoles.IsNullOrEmpty() && !userCompanyRoles.Any(i => i.IsModified))
						continue;

					foreach (UserCompanyRoleDTO userCompanyRolesInput in userCompanyRoles.Where(i => i.IsModified))
					{
						Role role = RoleManager.GetRole(entities, userCompanyRolesInput.RoleId, userCompanyRolesInput.ActorCompanyId);
						if (role == null)
							continue;

						UserCompanyRole userCompanyRole = userCompanyRolesInput.UserCompanyRoleId > 0 ? UserManager.GetUserCompanyRole(entities, userCompanyRolesInput.UserId, userCompanyRolesInput.ActorCompanyId, userCompanyRolesInput.UserCompanyRoleId) : null;
						if (userCompanyRole != null)
						{
							#region Update

							bool changesOnUserRole = false;

							if (userCompanyRolesInput.Default != userCompanyRole.Default)
							{
								userCompanyRole.Default = userCompanyRolesInput.Default;
								changesOnUserRole = true;
							}
							if (userCompanyRolesInput.DateFrom != userCompanyRole.DateFrom)
							{
								userCompanyRole.DateFrom = userCompanyRolesInput.DateFrom;
								changesOnUserRole = true;
							}
							if (userCompanyRolesInput.DateTo != userCompanyRole.DateTo)
							{
								userCompanyRole.DateTo = userCompanyRolesInput.DateTo;
								changesOnUserRole = true;
							}
							if ((int)userCompanyRolesInput.State != userCompanyRole.State)
							{
								userCompanyRole.State = (int)userCompanyRolesInput.State;
								changesOnUserRole = true;
							}

							if (changesOnUserRole)
								SetModifiedProperties(userCompanyRole);

							#endregion
						}
						else
						{
							#region Add

							ActionResult validationResult = ValidateAddNewUserCompanyRole(entities, licenseId, actorCompanyId, user);
							if (!validationResult.Success)
								return validationResult;

							UserCompanyRole newUserCompanyRole = new UserCompanyRole()
							{
								Default = userCompanyRolesInput.Default,
								DateFrom = userCompanyRolesInput.DateFrom,
								DateTo = userCompanyRolesInput.DateTo,
								State = (int)userCompanyRolesInput.State,

								//Set references
								Company = company,
								Role = role,
							};
							SetCreatedProperties(newUserCompanyRole);
							user.UserCompanyRole.Add(newUserCompanyRole);

							#endregion
						}
					}
				}

				#endregion
			}
			else
			{
				#region Complete save (GUI)

				if (!currentUser.UserCompanyRole.IsLoaded)
					currentUser.UserCompanyRole.Load();

				#region Update or delete existing roles

				foreach (UserCompanyRole userCompanyRole in user.UserCompanyRole.Where(ucr => ucr.State == (int)SoeEntityState.Active))
				{
					List<UserCompanyRoleDTO> userCompanyRoleInputs = dict.GetList(userCompanyRole.ActorCompanyId, nullIfNotFound: true);
					UserCompanyRoleDTO userCompanyRoleInput = userCompanyRoleInputs?.FirstOrDefault(r => r.UserCompanyRoleId == userCompanyRole.UserCompanyRoleId);

					bool doDeleteOrUpdate = userCompanyRoleInput == null || userCompanyRole.IsModified(userCompanyRoleInput);
					if (doDeleteOrUpdate && !currentUser.HasUserRole(this.parameterObject.RoleId, userCompanyRole.RoleId, userCompanyRoleInput?.RoleId))
						return new ActionResult((int)ActionResultSave.NothingSaved, GetText(91963, "Bara administratörer kan ge, uppdatera och ta bort en roll man inte själv har"));

					if (userCompanyRoleInput != null)
					{
						#region Update

						if (userCompanyRole.IsModified(userCompanyRoleInput))
						{
							userCompanyRole.Update(userCompanyRoleInput);
							SetModifiedProperties(userCompanyRole);
						}
						userCompanyRoleInputs.Remove(userCompanyRoleInput);

						#endregion
					}
					else
					{
						#region Delete

						// Cannot delete the mapping that the current user is running as
						if (!userCompanyRole.IsCurrent(base.ActorCompanyId, currentUser.ActiveRoleId, currentUser.UserId))
							ChangeEntityState(entities, userCompanyRole, SoeEntityState.Deleted, false);

						#endregion
					}
				}

				#endregion

				#region Add new roles (remaining in input)

				foreach (int actorCompanyId in dict.Keys)
				{
					List<UserCompanyRoleDTO> inputUserCompanyRoles = dict[actorCompanyId];
					foreach (UserCompanyRoleDTO inputUserCompanyRole in inputUserCompanyRoles)
					{
						if (!currentUser.HasUserRole(this.parameterObject.RoleId, inputUserCompanyRole.RoleId))
							return new ActionResult((int)ActionResultSave.NothingSaved, GetText(91963, "Bara administratörer kan ge, uppdatera och ta bort en roll man inte själv har"));

						ActionResult validationResult = ValidateAddNewUserCompanyRole(entities, licenseId, actorCompanyId, user);
						if (!validationResult.Success)
							return validationResult;

						UserCompanyRole newUserCompanyRole = new UserCompanyRole()
						{
							Default = inputUserCompanyRole.Default,
							DateFrom = inputUserCompanyRole.DateFrom,
							DateTo = inputUserCompanyRole.DateTo,

							//Set Fk
							ActorCompanyId = actorCompanyId,
							RoleId = inputUserCompanyRole.RoleId,
						};
						SetCreatedProperties(newUserCompanyRole);
						user.UserCompanyRole.Add(newUserCompanyRole);
					}
				}

				#endregion

				#endregion
			}

			return SaveChanges(entities);
		}

		public ActionResult AddUserCompanyRoleMapping(int userId, int actorCompanyId, int roleId, bool defaultRole)
		{
			//Flush UserCompanyRole cache
			CompDbCache.Instance.FlushUserCompanyRoles(userId);

			using (CompEntities entities = new CompEntities())
			{
				User user = UserManager.GetUser(entities, userId, loadUserCompanyRole: true);
				if (user == null)
					return new ActionResult((int)ActionResultSave.EntityNotFound, "User");

				Company company = CompanyManager.GetCompany(entities, actorCompanyId);
				if (company == null)
					return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte"));

				Role role = RoleManager.GetRole(entities, roleId);
				if (role == null)
					return new ActionResult((int)ActionResultSave.EntityNotFound, "Role");

				UserCompanyRole userCompanyRole = new UserCompanyRole()
				{
					Default = defaultRole,
					DateFrom = null,
					DateTo = null,

					//Set references
					User = user,
					Company = company,
					Role = role,
				};
				SetCreatedProperties(userCompanyRole);
				return AddEntityItem(entities, userCompanyRole, "UserCompanyRole");
			}
		}

		public ActionResult AddUserCompanyRoleMapping(CompEntities entities, int userId, int actorCompanyId, int roleId, bool defaultRole)
		{
			//Flush UserCompanyRole cache
			CompDbCache.Instance.FlushUserCompanyRoles(userId);

			User user = UserManager.GetUser(entities, userId, loadUserCompanyRole: true);
			if (user == null)
				return new ActionResult((int)ActionResultSave.EntityNotFound, "User");

			Company company = CompanyManager.GetCompany(entities, actorCompanyId);
			if (company == null)
				return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte"));

			Role role = RoleManager.GetRole(entities, roleId);
			if (role == null)
				return new ActionResult((int)ActionResultSave.EntityNotFound, "Role");

			UserCompanyRole userCompanyRole = new UserCompanyRole()
			{
				Default = defaultRole,
				DateFrom = null,
				DateTo = null,

				//Set references
				User = user,
				Company = company,
				Role = role,
			};
			SetCreatedProperties(userCompanyRole);
			return AddEntityItem(entities, userCompanyRole, "UserCompanyRole");
		}

		public ActionResult RunJobToSetCurrentRole(int licenseId, out int noOfUpdatedUsers, out int noOfIgnoredUsers, out int noOfAddedUcr, out bool abortedDueToIgnoreHandled, out List<string> invalidUsers, bool createNewUserCompanyRoles = false, bool ignoredHandled = false)
		{
			const int DEFAULTROLEID_NOTFOUND = 1;
			const int DEFAULTROLEID_INACTIVE = 2;
			const int DEFAULTROLEID_DEFAULTCOMPANYID_NOTINSYNC = 3;
			const int USERCOMPANYROLE_MISSING = 4;

			noOfUpdatedUsers = 0;
			noOfIgnoredUsers = 0;
			noOfAddedUcr = 0;
			abortedDueToIgnoreHandled = false;
			invalidUsers = new List<string>();

			using (CompEntities entities = new CompEntities())
			{
				List<User> users = (from u in entities.User
										.Include("UserCompanyRole")
									where u.LicenseId == licenseId &&
									u.State != (int)SoeEntityState.Deleted
									select u).ToList();

				List<Role> roles = new List<Role>();

				foreach (User user in users)
				{
					if (ignoredHandled && noOfIgnoredUsers >= 10)
					{
						abortedDueToIgnoreHandled = true;
						break;
					}

					bool updated = false;
					string userString = $"{user.UserId}.{user.LoginName}";
					string companyRoleString = $"(DefaultActorCompanyId:{user.DefaultActorCompanyId},DefaultRoleId:{user.DefaultRoleId})";

					int legacyDefaulRoleId = user.DefaultRoleId;
					if (legacyDefaulRoleId <= 0)
					{
						invalidUsers.Add($"Felkod:{DEFAULTROLEID_NOTFOUND}. {userString} {companyRoleString}");
						continue;
					}
					if (user.UserCompanyRole.Any(i => i.State == (int)SoeEntityState.Active && i.Default))
					{
						noOfIgnoredUsers++;
						continue;
					}

					Role role = roles.FirstOrDefault(i => i.RoleId == legacyDefaulRoleId);
					if (role == null)
					{
						role = RoleManager.GetRole(entities, legacyDefaulRoleId);
						if (role != null)
							roles.Add(role);
					}
					if (role == null)
					{
						invalidUsers.Add($"Felkod:{DEFAULTROLEID_INACTIVE}. {userString} {companyRoleString}");
						continue;
					}
					if (user.DefaultActorCompanyId.HasValue && user.DefaultActorCompanyId.Value > 0 && user.DefaultActorCompanyId.Value != role.ActorCompanyId)
					{
						invalidUsers.Add($"Felkod:{DEFAULTROLEID_DEFAULTCOMPANYID_NOTINSYNC}. {userString} {companyRoleString}");
						continue;
					}

					List<UserCompanyRole> userCompanyRoles = user.UserCompanyRole
						.Where(i =>
							i.ActorCompanyId == role.ActorCompanyId &&
							i.RoleId == role.RoleId &&
							i.State == (int)SoeEntityState.Active)
						.OrderBy(i => i.DateFrom ?? DateTime.MinValue).ToList();

					if (userCompanyRoles.Any())
					{
						foreach (UserCompanyRole userCompanyRole in userCompanyRoles)
						{
							if (!userCompanyRole.Default)
							{
								userCompanyRole.Default = true;
								updated = true;
							}
						}
					}
					else if (role.ActorCompanyId.HasValue)
					{
						invalidUsers.Add($"Felkod:{USERCOMPANYROLE_MISSING}. {userString} {companyRoleString}");

						if (createNewUserCompanyRoles)
						{
							var newUserCompanyRole = new UserCompanyRole()
							{
								Default = true,
								DateFrom = null,
								DateTo = null,
								State = (int)SoeEntityState.Active,
								Created = DateTime.Now,
								CreatedBy = "SoftOne job",

								//Set FK
								ActorCompanyId = role.ActorCompanyId.Value,
								RoleId = role.RoleId,
							};
							SetCreatedProperties(newUserCompanyRole);
							entities.UserCompanyRole.AddObject(newUserCompanyRole);
							user.UserCompanyRole.Add(newUserCompanyRole);
							noOfAddedUcr++;
						}
					}

					if (updated)
						noOfUpdatedUsers++;
				}

				return SaveChanges(entities);
			}
		}

		private ActionResult ValidateAddNewUserCompanyRole(CompEntities entities, int licenseId, int actorCompanyId, User user)
		{
			// If not a demo company we need to check the license if we can add new users to the license
			Company company = CompanyManager.GetCompany(entities, actorCompanyId);
			if (company != null && !company.Demo && !CanAddUserToRole(entities, licenseId, user))
				return new ActionResult((int)ActionResultSave.UserLicenseViolation, GetText(9138, "Max antal användare på licensen är uppnådd"));
			else
				return new ActionResult(true);
		}

		public bool ValidateMobileUserCompanyRole(int licenseId, int userId, int roleId, int actorCompanyId)
		{
			if (CompDbCache.Instance.SiteType == TermGroup_SysPageStatusSiteType.Test)
				LogInfo($"ValidateMobileUserCompanyRole licenseId:{licenseId} userId:{userId} roleId:{roleId} actorCompanyId:{actorCompanyId}");

			DateTime date = DateTime.Today;
			using var entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
			return (from ucr in entitiesReadOnly.UserCompanyRole.Include("Company")
					where ucr.UserId == userId &&
					ucr.Company.ActorCompanyId == actorCompanyId &&
					ucr.Company.LicenseId == licenseId &&
					ucr.RoleId == roleId &&
					 ucr.State == (int)SoeEntityState.Active &&
					(!ucr.DateFrom.HasValue || ucr.DateFrom <= date) &&
					(!ucr.DateTo.HasValue || ucr.DateTo >= date) &&
					(ucr.User.State == (int)SoeEntityState.Active)
					select ucr).Any();
		}

		#endregion

		#region UserCompanyRoleDelegateHistory

		public List<UserCompanyRoleDelegateHistoryGridDTO> GetUserCompanyRoleDelegateHistoryForUser(int actorCompanyId, int roleId, int currentUserId, int userId)
		{
			bool deletePermission = FeatureManager.HasRolePermission(currentUserId == userId ? Feature.Manage_Users_Edit_Delegate_MySelf_OwnRolesAndAttestRoles : Feature.Manage_Users_Edit_Delegate_OtherUsers_OwnRolesAndAttestRoles, Permission.Modify, roleId, actorCompanyId);

			List<UserCompanyRoleDelegateHistoryGridDTO> dtos = new List<UserCompanyRoleDelegateHistoryGridDTO>();

			// Also return deleted records, they are shown in GUI
			using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
			entities.UserCompanyRoleDelegateHistoryHead.NoTracking();
			entities.UserCompanyRoleDelegateHistoryRow.NoTracking();
			List<UserCompanyRoleDelegateHistoryHead> heads = (from u in entities.UserCompanyRoleDelegateHistoryHead
																.Include("UserCompanyRoleDelegateHistoryRow.Role")
																.Include("UserCompanyRoleDelegateHistoryRow.AttestRole")
																.Include("UserCompanyRoleDelegateHistoryRow.Account")
																.Include("FromUser.ContactPerson")
																.Include("ToUser.ContactPerson")
																.Include("ByUser.ContactPerson")
															  where u.ActorCompanyId == actorCompanyId &&
															  (u.FromUserId == userId || u.ToUserId == userId)
															  select u).ToList();

			foreach (UserCompanyRoleDelegateHistoryHead head in heads)
			{
				foreach (UserCompanyRoleDelegateHistoryRow row in head.UserCompanyRoleDelegateHistoryRow)
				{
					UserCompanyRoleDelegateHistoryGridDTO dto = new UserCompanyRoleDelegateHistoryGridDTO()
					{
						UserCompanyRoleDelegateHistoryHeadId = head.UserCompanyRoleDelegateHistoryHeadId,
						FromUserId = head.FromUserId,
						FromUserName = string.Format("{0} ({1})", head.FromUser.Name, head.FromUser.LoginName),
						ToUserId = head.ToUserId,
						ToUserName = string.Format("{0} ({1})", head.ToUser.Name, head.ToUser.LoginName),
						ByUserId = head.ByUserId,
						ByUserName = string.Format("{0} ({1})", head.ByUser.Name, head.ByUser.LoginName),
						DateFrom = row.DateFrom,
						DateTo = row.DateTo,
						Created = head.Created,
						State = (SoeEntityState)row.State,
						ShowDelete = deletePermission && head.FromUserId == userId && row.State != (int)SoeEntityState.Deleted
					};

					if (row.Role != null)
						dto.RoleNames = row.Role.Name;
					if (row.AttestRole != null)
					{
						dto.AttestRoleNames = row.AttestRole.Name;
						if (row.Account != null)
							dto.AttestRoleNames = String.Format("{0} ({1})", dto.AttestRoleNames, row.Account.Name);
					}

					dtos.Add(dto);
				}
			}

			return dtos.OrderByDescending(d => d.UserCompanyRoleDelegateHistoryHeadId).ThenBy(d => d.DateFrom).ThenBy(d => d.DateTo).ThenBy(d => d.RoleNames).ThenBy(d => d.AttestRoleNames).ToList();
		}

		public UserCompanyRoleDelegateHistoryHead GetUserCompanyRoleDelegateHistoryHead(CompEntities entities, int userCompanyRoleDelegateHistoryHeadId, int actorCompanyId)
		{
			UserCompanyRoleDelegateHistoryHead head = (from u in entities.UserCompanyRoleDelegateHistoryHead
														   .Include("UserCompanyRoleDelegateHistoryRow.AttestRoleUser")
														   .Include("UserCompanyRoleDelegateHistoryRow.UserCompanyRole")
													   where u.UserCompanyRoleDelegateHistoryHeadId == userCompanyRoleDelegateHistoryHeadId &&
													   u.ActorCompanyId == actorCompanyId &&
													   u.State == (int)SoeEntityState.Active
													   select u).FirstOrDefault();
			return head;
		}

		public UserCompanyRoleDelegateHistoryUserDTO SearchTargetUserForDelegation(int actorCompanyId, int sourceUserId, int currentUserId, string userCondition)
		{
			UserCompanyRoleDelegateHistoryUserDTO dto = null;
			using var entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
			// Search for user
			using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
			entities.User.NoTracking();
			User targetUser = (from u in entities.User.Include("ContactPerson")
							   where u.LoginName == userCondition &&
							   u.UserCompanyRole.Any(r => r.ActorCompanyId == actorCompanyId) &&
							   u.State == (int)SoeEntityState.Active
							   select u).FirstOrDefault();

			if (targetUser == null)
			{
				// Search for employee
				entities.Employee.NoTracking();
				Employee employee = (from e in entities.Employee
									 where e.EmployeeNr == userCondition &&
									 e.ActorCompanyId == actorCompanyId &&
									 e.UserId != null &&
									 e.State == (int)SoeEntityState.Active
									 select e).FirstOrDefault();

				if (employee != null)
				{
					// Get user connected to employee
					targetUser = (from u in entitiesReadOnly.User.Include("ContactPerson")
								  where u.UserId == employee.UserId.Value &&
								  u.UserCompanyRole.Any(r => r.ActorCompanyId == actorCompanyId) &&
								  u.State == (int)SoeEntityState.Active
								  select u).FirstOrDefault();
				}
			}

			if (targetUser == null || targetUser.UserId == sourceUserId)
				return dto;

			GetValidAttestRolesAndRoles(targetUser, sourceUserId, currentUserId, actorCompanyId, out List<UserAttestRoleDTO> validAttestRoles, out List<UserCompanyRoleDTO> validUserCompanyRoles);

			dto = new UserCompanyRoleDelegateHistoryUserDTO()
			{
				UserId = targetUser.UserId,
				LoginName = targetUser.LoginName,
				Name = targetUser.Name,
				PossibleTargetRoles = validUserCompanyRoles,
				PossibleTargetAttestRoles = validAttestRoles
			};

			return dto;
		}

		private ActionResult ValidateAttestRolesAndRoles(UserCompanyRoleDelegateHistoryUserDTO targetUser, int actorCompanyId, int sourceUserId, int currentUserId)
		{
			if (targetUser == null || (targetUser.TargetAttestRoles.IsNullOrEmpty() && targetUser.TargetRoles.IsNullOrEmpty()))
				return new ActionResult(false, 0, "Empty");

			using var entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
			var user = (from u in entitiesReadOnly.User.Include("ContactPerson").Include("UserCompanyRole.Role").Include("AttestRoleUser.AttestRole")
						where u.UserId == targetUser.UserId &&
						u.UserCompanyRole.Any(r => r.ActorCompanyId == actorCompanyId) &&
						u.State == (int)SoeEntityState.Active
						select u).FirstOrDefault();

			if (user == null)
				return new ActionResult(false, 0, GetText(1288));

			GetValidAttestRolesAndRoles(user, sourceUserId, currentUserId, actorCompanyId, out List<UserAttestRoleDTO> validAttestRoles, out List<UserCompanyRoleDTO> validUserCompanyRoles);

			List<UserAttestRoleDTO> invalidAttestRoles = new List<UserAttestRoleDTO>();
			List<UserCompanyRoleDTO> invalidUserCompanyRoles = new List<UserCompanyRoleDTO>();

			if (!targetUser.TargetAttestRoles.IsNullOrEmpty())
				invalidAttestRoles = targetUser.TargetAttestRoles.Where(a => !validAttestRoles.Select(s => s.AttestRoleId).Contains(a.AttestRoleId)).ToList();

			if (!targetUser.TargetRoles.IsNullOrEmpty())
				invalidUserCompanyRoles = targetUser.TargetRoles.Where(a => !validUserCompanyRoles.Select(s => s.RoleId).Contains(a.RoleId)).ToList();


			if (invalidAttestRoles.Any() || invalidUserCompanyRoles.Any())
			{
				StringBuilder sb = new StringBuilder();
				if (invalidAttestRoles.Any())
				{
					if (invalidAttestRoles.Count == 1)
					{
						sb.Append(GetText(12037, "Ogiltiga attestroller:" + Environment.NewLine));

						foreach (var item in invalidAttestRoles)
							sb.Append(item.Name + Environment.NewLine);
					}
					else
						sb.Append(GetText(12038, "Ogiltig attestroll " + invalidAttestRoles.First().Name + Environment.NewLine));
				}

				if (invalidUserCompanyRoles.Any())
				{
					if (sb.Length > 0)
						sb.Append(Environment.NewLine);

					if (invalidUserCompanyRoles.Count == 1)
					{
						sb.Append(GetText(12039, "Ogiltiga roller:" + Environment.NewLine));

						foreach (var item in invalidUserCompanyRoles)
							sb.Append(item.Name + Environment.NewLine);
					}
					else
						sb.Append(GetText(12040, "Ogiltig roller " + invalidUserCompanyRoles.First().Name + Environment.NewLine));
				}


				return new ActionResult(false, 1, GetText(11891) + Environment.NewLine + Environment.NewLine + sb.ToString());
			}

			return new ActionResult();

		}

		private void GetValidAttestRolesAndRoles(User targetUser, int sourceUserId, int currentUserId, int actorCompanyId, out List<UserAttestRoleDTO> validUserAttestRoles, out List<UserCompanyRoleDTO> validUserCompanyRoles)
		{
			#region Prereq

			validUserAttestRoles = new List<UserAttestRoleDTO>();
			validUserCompanyRoles = new List<UserCompanyRoleDTO>();

			if (targetUser == null)
				return;

			#endregion

			#region Permissions

			bool delegatePermission = false;
			bool delegateAllRolesPermission = false;
			bool delegateAllAttestRolesPermission = false;

			if (currentUserId == sourceUserId)
			{
				delegatePermission = FeatureManager.HasRolePermission(Feature.Manage_Users_Edit_Delegate_MySelf_OwnRolesAndAttestRoles, Permission.Modify, base.RoleId, base.ActorCompanyId);
				delegateAllRolesPermission = FeatureManager.HasRolePermission(Feature.Manage_Users_Edit_Delegate_MySelf_AllRoles, Permission.Modify, base.RoleId, base.ActorCompanyId);
				delegateAllAttestRolesPermission = FeatureManager.HasRolePermission(Feature.Manage_Users_Edit_Delegate_MySelf_AllAttestRoles, Permission.Modify, base.RoleId, base.ActorCompanyId);
			}
			else
			{
				delegatePermission = FeatureManager.HasRolePermission(Feature.Manage_Users_Edit_Delegate_OtherUsers_OwnRolesAndAttestRoles, Permission.Modify, base.RoleId, base.ActorCompanyId);
				delegateAllRolesPermission = FeatureManager.HasRolePermission(Feature.Manage_Users_Edit_Delegate_OtherUsers_AllRoles, Permission.Modify, base.RoleId, base.ActorCompanyId);
				delegateAllAttestRolesPermission = FeatureManager.HasRolePermission(Feature.Manage_Users_Edit_Delegate_OtherUsers_AllAttestRoles, Permission.Modify, base.RoleId, base.ActorCompanyId);
			}

			if (!delegatePermission)
				return;

			#endregion

			#region Source user

			// Get source user
			using var entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
			User sourceUser = (from u in entitiesReadOnly.User
								.Include("ContactPerson")
								.Include("UserCompanyRole.Role")
								.Include("AttestRoleUser.AttestRole")
								.Include("AttestRoleUser.Account.AccountDim")
								.Include("AttestRoleUser.Children.Account.AccountDim")
							   where u.UserId == sourceUserId &&
							   u.UserCompanyRole.Any(r => r.ActorCompanyId == actorCompanyId) &&
							   u.State == (int)SoeEntityState.Active
							   select u).FirstOrDefault();

			if (sourceUser == null)
				return;

			#endregion

			#region Roles

			List<UserCompanyRole> rolesOnSourceUser = sourceUser.UserCompanyRole.Where(r => r.ActorCompanyId == actorCompanyId && r.State == (int)SoeEntityState.Active).ToList();
			List<int> validRoleIds = (delegateAllRolesPermission ? rolesOnSourceUser : GetUserCompanyRolesByUserAndCompany(targetUser.UserId, actorCompanyId)).Select(r => r.RoleId).ToList();
			validUserCompanyRoles.AddRange(rolesOnSourceUser.Where(r => validRoleIds.Contains(r.RoleId)).ToDTOs());

			#endregion

			#region Attest roles

			List<AccountDimDTO> accountDims = GetAccountDimsFromCache(entitiesReadOnly, CacheConfig.Company(actorCompanyId));
			List<GenericType> permissionTypes = base.GetTermGroupContent(TermGroup.AttestRoleUserAccountPermissionType);
			List<AttestRoleUser> attestRolesOnSourceUser = sourceUser.AttestRoleUser.Where(r => r.AttestRole.ActorCompanyId == actorCompanyId && r.State == (int)SoeEntityState.Active && !r.ParentAttestRoleUserId.HasValue).ToList();
			List<int> validAttestRoleIds = delegateAllAttestRolesPermission ? attestRolesOnSourceUser.Select(r => r.AttestRoleId).ToList() : AttestManager.GetAttestRolesForUser(actorCompanyId, targetUser.UserId).Select(r => r.AttestRoleId).ToList();
			validUserAttestRoles.AddRange(attestRolesOnSourceUser.Where(r => validAttestRoleIds.Contains(r.AttestRoleId)).ToDTOs(accountDims, permissionTypes, true));

			#endregion
		}

		public ActionResult SaveUserCompanyRoleDelegation(UserCompanyRoleDelegateHistoryUserDTO targetUser, int actorCompanyId, int sourceUserId, int currentUserId)
		{
			ActionResult result = ValidateAttestRolesAndRoles(targetUser, actorCompanyId, sourceUserId, currentUserId);

			if (!result.Success)
				return result;

			using (CompEntities entities = new CompEntities())
			{
				try
				{
					entities.Connection.Open();

					using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
					{
						#region UserCompanyRoleDelegateHistoryHead

						UserCompanyRoleDelegateHistoryHead head = new UserCompanyRoleDelegateHistoryHead()
						{
							ActorCompanyId = actorCompanyId,
							FromUserId = sourceUserId,
							ToUserId = targetUser.UserId,
							ByUserId = currentUserId
						};
						SetCreatedProperties(head);
						entities.AddObject("UserCompanyRoleDelegateHistoryHead", head);

						#endregion

						#region UserCompanyRoleDelegateHistoryRow

						#region Roles

						// Date is mandatory in GUI
						foreach (UserCompanyRoleDTO role in targetUser.TargetRoles.Where(r => r.DateFrom.HasValue && r.DateTo.HasValue).ToList())
						{
							UserCompanyRoleDelegateHistoryRow row = new UserCompanyRoleDelegateHistoryRow()
							{
								RoleId = role.RoleId,
								DateFrom = role.DateFrom.Value,
								DateTo = role.DateTo.Value
							};
							SetCreatedProperties(row);
							head.UserCompanyRoleDelegateHistoryRow.Add(row);

							UserCompanyRole ucr = new UserCompanyRole()
							{
								UserCompanyRoleDelegateHistoryRow = row,
								RoleId = role.RoleId,
								ActorCompanyId = actorCompanyId,
								UserId = targetUser.UserId,
								Default = false,
								DateFrom = role.DateFrom.Value,
								DateTo = role.DateTo.Value
							};
							SetCreatedProperties(ucr);
							entities.AddObject("UserCompanyRole", ucr);
						}

						#endregion

						#region AttestRoles

						// Date is mandatory in GUI
						foreach (UserAttestRoleDTO attestRole in targetUser.TargetAttestRoles.Where(r => r.DateFrom.HasValue && r.DateTo.HasValue).ToList())
						{
							AttestRoleUser sourceAttestRole = AttestManager.GetAttestRoleUser(entities, attestRole.AttestRoleUserId);
							if (sourceAttestRole != null)
							{
								UserCompanyRoleDelegateHistoryRow row = new UserCompanyRoleDelegateHistoryRow()
								{
									AttestRoleId = sourceAttestRole.AttestRoleId,
									AccountId = sourceAttestRole.AccountId,
									DateFrom = attestRole.DateFrom.Value,
									DateTo = attestRole.DateTo.Value
								};
								SetCreatedProperties(row);
								head.UserCompanyRoleDelegateHistoryRow.Add(row);

								AttestRoleUser aru = new AttestRoleUser()
								{
									UserCompanyRoleDelegateHistoryRow = row,
									AttestRoleId = sourceAttestRole.AttestRoleId,
									UserId = targetUser.UserId,
									DateFrom = attestRole.DateFrom.Value,
									DateTo = attestRole.DateTo.Value,
									MaxAmount = sourceAttestRole.MaxAmount,
									AccountId = sourceAttestRole.AccountId,
									IsExecutive = sourceAttestRole.IsExecutive,
									IsNearestManager = sourceAttestRole.IsNearestManager,
									AccountPermissionType = sourceAttestRole.AccountPermissionType
								};
								SetCreatedProperties(aru);
								entities.AddObject("AttestRoleUser", aru);

								#region Children

								if (attestRole.Children != null && attestRole.Children.Any())
								{
									foreach (UserAttestRoleDTO childAttestRole in attestRole.Children.Where(r => r.DateFrom.HasValue && r.DateTo.HasValue).ToList())
									{
										AttestRoleUser sourceChildRole = AttestManager.GetAttestRoleUser(entities, childAttestRole.AttestRoleUserId);
										if (sourceChildRole != null)
										{
											UserCompanyRoleDelegateHistoryRow childRow = new UserCompanyRoleDelegateHistoryRow()
											{
												Parent = row,
												AttestRoleId = sourceChildRole.AttestRoleId,
												AccountId = sourceChildRole.AccountId,
												DateFrom = childAttestRole.DateFrom.Value,
												DateTo = childAttestRole.DateTo.Value
											};
											SetCreatedProperties(childRow);
											head.UserCompanyRoleDelegateHistoryRow.Add(childRow);

											AttestRoleUser childAru = new AttestRoleUser()
											{
												AttestRoleId = sourceChildRole.AttestRoleId,
												UserId = targetUser.UserId,
												DateFrom = childAttestRole.DateFrom.Value,
												DateTo = childAttestRole.DateTo.Value,
												MaxAmount = sourceChildRole.MaxAmount,
												AccountId = sourceChildRole.AccountId,
												IsExecutive = sourceChildRole.IsExecutive,
												IsNearestManager = sourceChildRole.IsNearestManager,
												AccountPermissionType = sourceChildRole.AccountPermissionType
											};
											SetCreatedProperties(childAru);
											if (aru.Children == null)
												aru.Children = new EntityCollection<AttestRoleUser>();
											aru.Children.Add(childAru);
										}
									}
								}

								#endregion
							}
						}

						#endregion

						#endregion

						result = SaveChanges(entities, transaction);
						if (result.Success)
							transaction.Complete();
					}
				}
				catch (Exception ex)
				{
					base.LogError(ex, this.log);
					result = new ActionResult(ex);
				}
				finally
				{
					if (result != null && !result.Success)
						base.LogTransactionFailed(this.ToString(), this.log);

					else
						SendCompanyRoleDelegation(targetUser, sourceUserId);
					entities.Connection.Close();
				}
			}

			return result;
		}

		private void SendCompanyRoleDelegation(UserCompanyRoleDelegateHistoryUserDTO targetUser, int sourceUserId)
		{
			if ((targetUser.TargetAttestRoles.IsNullOrEmpty() && targetUser.TargetRoles.IsNullOrEmpty()) || targetUser.UserId == UserId)
				return;

			List<string> attestRoles = targetUser.TargetAttestRoles?.Select(s => $"{s.Name} {CalendarUtility.ToShortDateString(s.DateFrom)} - {CalendarUtility.ToShortDateString(s.DateTo)}").ToList();
			List<string> roles = targetUser.TargetRoles?.Select(s => $"{s.Name} {CalendarUtility.ToShortDateString(s.DateFrom)} - {CalendarUtility.ToShortDateString(s.DateTo)}").ToList();
			string subject = GetText(12030, "Du har blivit delegerad nya rättigheter");
			string attestrolesName = GetText(5214, "Attestroller");
			string rolesName = GetText(5728, "Roller");

			if (attestRoles.IsNullOrEmpty() && roles.IsNullOrEmpty())
				return;

			StringBuilder sb = new StringBuilder();

			sb.Append(subject + Environment.NewLine + Environment.NewLine);
			if (roles != null && roles.Any())
			{
				sb.Append(rolesName + ":" + Environment.NewLine);
				roles.ForEach(f => sb.Append(f + Environment.NewLine));
				sb.Append(Environment.NewLine);
			}
			if (attestRoles != null && attestRoles.Any())
			{
				sb.Append(attestrolesName + ":" + Environment.NewLine);
				attestRoles.ForEach(f => sb.Append(f + Environment.NewLine));
			}
			using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
			var mailToTargetUser = new MessageEditDTO()
			{
				ActorCompanyId = ActorCompanyId,
				LicenseId = entitiesReadOnly.Company.First(f => f.ActorCompanyId == ActorCompanyId).LicenseId,
				SenderUserId = UserId,
				SenderName = FullName,
				Subject = subject,
				Text = sb.ToString(),
				Created = DateTime.Now,
				AnswerType = XEMailAnswerType.None,
				MessagePriority = TermGroup_MessagePriority.High,
				MessageType = TermGroup_MessageType.Delegate,
				MessageDeliveryType = TermGroup_MessageDeliveryType.XEmail,
				MessageTextType = TermGroup_MessageTextType.Text,
				Recievers = new List<MessageRecipientDTO>()
				{
					new MessageRecipientDTO()
					{
						UserId = targetUser.UserId,
					}
				},
			};

			CommunicationManager.SendXEMail(mailToTargetUser, ActorCompanyId, 0, UserId);

			if (sourceUserId != UserId)
			{
				subject = GetText(12031, string.Format("{0} har fått delar av dina rättigheter", targetUser.Name));

				var mailToSourceUser = new MessageEditDTO()
				{
					ActorCompanyId = ActorCompanyId,
					LicenseId = entitiesReadOnly.Company.First(f => f.ActorCompanyId == ActorCompanyId).LicenseId,
					SenderUserId = UserId,
					SenderName = FullName,
					Subject = subject,
					Text = sb.ToString(),
					Created = DateTime.Now,
					AnswerType = XEMailAnswerType.None,
					MessagePriority = TermGroup_MessagePriority.High,
					MessageType = TermGroup_MessageType.Delegate,
					MessageDeliveryType = TermGroup_MessageDeliveryType.XEmail,
					MessageTextType = TermGroup_MessageTextType.Text,
					Recievers = new List<MessageRecipientDTO>()
					{
						new MessageRecipientDTO()
						{
							UserId = sourceUserId,
						}
					},
				};

				CommunicationManager.SendXEMail(mailToSourceUser, ActorCompanyId, 0, UserId);
			}
		}

		public ActionResult DeleteUserCompanyRoleDelegation(int userCompanyRoleDelegateHistoryHeadId, int actorCompanyId)
		{
			ActionResult result = new ActionResult();

			using (CompEntities entities = new CompEntities())
			{
				try
				{
					entities.Connection.Open();

					using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
					{
						UserCompanyRoleDelegateHistoryHead head = GetUserCompanyRoleDelegateHistoryHead(entities, userCompanyRoleDelegateHistoryHeadId, actorCompanyId);
						if (head == null)
							return new ActionResult((int)ActionResultDelete.EntityNotFound, "UserCompanyRoleDelegateHistoryHead");

						SetModifiedProperties(head);
						result = ChangeEntityState(entities, head, SoeEntityState.Deleted, false);
						if (!result.Success)
							return result;

						foreach (UserCompanyRoleDelegateHistoryRow row in head.UserCompanyRoleDelegateHistoryRow)
						{
							SetModifiedProperties(row);
							result = ChangeEntityState(entities, row, SoeEntityState.Deleted, false);
							if (!result.Success)
								return result;

							// Delegated roles
							if (row.UserCompanyRole != null)
							{
								foreach (UserCompanyRole role in row.UserCompanyRole)
								{
									SetModifiedProperties(role);
									result = ChangeEntityState(entities, role, SoeEntityState.Deleted, false);
									if (!result.Success)
										return result;
								}
							}

							// Delegated attest roles
							if (row.AttestRoleUser != null)
							{
								foreach (AttestRoleUser attestRole in row.AttestRoleUser)
								{
									SetModifiedProperties(attestRole);
									result = ChangeEntityState(entities, attestRole, SoeEntityState.Deleted, false);
									if (!result.Success)
										return result;
								}
							}
						}

						result = SaveChanges(entities, transaction);
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
					if (!result.Success)
						base.LogTransactionFailed(this.ToString(), this.log);

					entities.Connection.Close();
				}
			}

			return result;
		}

		#endregion

		#region UserSelection

		public Dictionary<int, string> GetUserSelectionsDict(UserSelectionType type, int userId, int roleId, int actorCompanyId)
		{
			using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
			entities.UserSelection.NoTracking();
			return GetUserSelectionsDict(entities, type, userId, roleId, actorCompanyId);
		}

		public Dictionary<int, string> GetUserSelectionsDict(CompEntities entities, UserSelectionType type, int userId, int roleId, int actorCompanyId)
		{
			var selections = (from us in entities.UserSelection.Include("UserSelectionAccess")
							  where us.Type == (int)type &&
							  us.ActorCompanyId == actorCompanyId &&
							  (!us.UserId.HasValue || us.UserId.Value == userId) &&
							  us.State == (int)SoeEntityState.Active
							  orderby us.Name
							  select new
							  {
								  us.UserSelectionId,
								  us.Name,
								  us.UserId,
								  us.UserSelectionAccess
							  }).ToList();

			Dictionary<int, string> result = new Dictionary<int, string>();
			foreach (var selection in selections)
			{
				List<UserSelectionAccess> accesses = selection.UserSelectionAccess.Where(a => a.State == (int)SoeEntityState.Active).ToList();
				if (accesses.IsNullOrEmpty())
				{
					// No access records, it's either privat or public and has been filtered in original query
					result.Add(selection.UserSelectionId, selection.Name);
				}
				else
				{
					TermGroup_ReportUserSelectionAccessType accessType = (TermGroup_ReportUserSelectionAccessType)accesses[0].Type;
					if (accessType == TermGroup_ReportUserSelectionAccessType.Role)
					{
						if (accesses.Any(a => a.RoleId == roleId))
							result.Add(selection.UserSelectionId, selection.Name);
					}
					else if (accessType == TermGroup_ReportUserSelectionAccessType.MessageGroup)
					{
						foreach (UserSelectionAccess access in accesses.Where(a => a.MessageGroupId.HasValue).ToList())
						{
							MessageGroup group = CommunicationManager.GetMessageGroup(entities, access.MessageGroupId.Value, true);
							if (CommunicationManager.IsUserInMessageGroup(group, actorCompanyId, userId, roleId: roleId, dateFrom: DateTime.Today, dateTo: DateTime.Today))
							{
								result.Add(selection.UserSelectionId, selection.Name);
								break;
							}
						}
					}
				}
			}

			return result;
		}

        public List<UserSelection> GetUserSelections(UserSelectionType type, int userId, int roleId, int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.UserSelection.NoTracking();
            return GetUserSelections(entities, type, userId, roleId, actorCompanyId);
        }

        public List<UserSelection> GetUserSelections(CompEntities entities, UserSelectionType type, int userId, int roleId, int actorCompanyId)
        {
            List<UserSelection> selections = (from us in entities.UserSelection.Include("UserSelectionAccess")
											  where us.Type == (int)type &&
											  us.ActorCompanyId == actorCompanyId &&
											  (!us.UserId.HasValue || us.UserId.Value == userId) &&
											  us.State == (int)SoeEntityState.Active
											  orderby us.Name
											  select us).ToList();

            List<UserSelection> result = new List<UserSelection>();
            foreach (var selection in selections)
            {
                List<UserSelectionAccess> accesses = selection.UserSelectionAccess.Where(a => a.State == (int)SoeEntityState.Active).ToList();
                if (accesses.IsNullOrEmpty())
                {
                    // No access records, it's either privat or public and has been filtered in original query
                    result.Add(selection);
                }
                else
                {
                    TermGroup_ReportUserSelectionAccessType accessType = (TermGroup_ReportUserSelectionAccessType)accesses[0].Type;
                    if (accessType == TermGroup_ReportUserSelectionAccessType.Role)
                    {
                        if (accesses.Any(a => a.RoleId == roleId))
                            result.Add(selection);
                    }
                    else if (accessType == TermGroup_ReportUserSelectionAccessType.MessageGroup)
                    {
                        foreach (UserSelectionAccess access in accesses.Where(a => a.MessageGroupId.HasValue).ToList())
                        {
                            MessageGroup group = CommunicationManager.GetMessageGroup(entities, access.MessageGroupId.Value, true);
                            if (CommunicationManager.IsUserInMessageGroup(group, actorCompanyId, userId, roleId: roleId, dateFrom: DateTime.Today, dateTo: DateTime.Today))
                            {
                                result.Add(selection);
                                break;
                            }
                        }
                    }
                }
            }

            return result;
        }

        public UserSelection GetUserSelection(int userSelectionId, bool loadAccess = false)
		{
			using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
			entities.UserSelection.NoTracking();
			return GetUserSelection(entities, userSelectionId, loadAccess);
		}

		public UserSelection GetUserSelection(CompEntities entities, int userSelectionId, bool loadAccess = false)
		{

			IQueryable<UserSelection> query = (from us in entities.UserSelection
											   where us.UserSelectionId == userSelectionId &&
											   us.State == (int)SoeEntityState.Active
											   select us);
			if (loadAccess)
				query = query.Include("UserSelectionAccess");

			return query.FirstOrDefault();
		}

		public ActionResult SaveUserSelection(UserSelectionDTO dto)
		{
			bool schedulePlanning = dto.Type == UserSelectionType.SchedulePlanningView_Day || dto.Type == UserSelectionType.SchedulePlanningView_Schedule;

            if (dto.Selections.IsNullOrEmpty() && !schedulePlanning)
				return new ActionResult((int)ActionResultSave.EntityNotFound, "ReportDataSelectionDTO");

			ActionResult result = null;

			using (CompEntities entities = new CompEntities())
			{
				try
				{
					entities.Connection.Open();

					using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
					{
						// Get existing
						UserSelection userSelection = dto.UserSelectionId > 0 ? GetUserSelection(entities, dto.UserSelectionId, loadAccess: true) : null;
						if (userSelection == null)
						{
							#region Add

							userSelection = new UserSelection()
							{
								ActorCompanyId = base.ActorCompanyId,
							};
							entities.UserSelection.AddObject(userSelection);
							SetCreatedProperties(userSelection);

							#endregion
						}
						else
						{
							#region Update

							SetModifiedProperties(userSelection);

							#endregion
						}

						// Common
						userSelection.UserId = dto.UserId;
						userSelection.Type = (int)dto.Type;
						userSelection.Name = dto.Name;
						userSelection.Description = dto.Description;
						userSelection.State = (int)dto.State;
						userSelection.Selection = schedulePlanning ? dto.Selection : ReportDataSelectionDTO.ToJSON(dto.Selections);
                        
						//TODO: Temporary set always to default for new schedule planning selections
						if (schedulePlanning)
							userSelection.Default = true;

                        #region Access

                        if (userSelection.UserSelectionAccess != null)
						{
							// Check existing
							// If still exists in input, remove from input
							// If no longer left in input, set as deleted
							foreach (UserSelectionAccess access in userSelection.UserSelectionAccess.Where(a => a.State == (int)SoeEntityState.Active).ToList())
							{
								if (dto.Access != null)
								{
									UserSelectionAccessDTO acc = null;
									if (access.Type == (int)TermGroup_ReportUserSelectionAccessType.Role)
										acc = dto.Access.FirstOrDefault(a => a.Type == (TermGroup_ReportUserSelectionAccessType)access.Type && a.RoleId == access.RoleId);
									else if (access.Type == (int)TermGroup_ReportUserSelectionAccessType.MessageGroup)
										acc = dto.Access.FirstOrDefault(a => a.Type == (TermGroup_ReportUserSelectionAccessType)access.Type && a.MessageGroupId == access.MessageGroupId);

									if (acc != null)
										dto.Access.Remove(acc);
									else
										ChangeEntityState(access, SoeEntityState.Deleted);
								}
							}
						}

						// Add new, if input still contains items
						if (!dto.Access.IsNullOrEmpty())
						{
							if (userSelection.UserSelectionAccess == null)
								userSelection.UserSelectionAccess = new EntityCollection<UserSelectionAccess>();

							foreach (UserSelectionAccessDTO acc in dto.Access)
							{
								UserSelectionAccess access = new UserSelectionAccess() { Type = (int)acc.Type };
								if (acc.Type == TermGroup_ReportUserSelectionAccessType.Role)
									access.RoleId = acc.RoleId;
								else if (acc.Type == TermGroup_ReportUserSelectionAccessType.MessageGroup)
									access.MessageGroupId = acc.MessageGroupId;

								SetCreatedProperties(access);
								userSelection.UserSelectionAccess.Add(access);
							}
						}

						#endregion

						result = SaveChanges(entities, transaction);
						if (result.Success)
						{
							transaction.Complete();
							result.IntegerValue = userSelection.UserSelectionId;
						}
					}
				}
				catch (Exception ex)
				{
					base.LogError(ex, this.log);
					result.Exception = ex;
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

		public ActionResult DeleteUserSelection(int userSelectionId)
		{
			using (CompEntities entities = new CompEntities())
			{
				UserSelection userSelection = GetUserSelection(entities, userSelectionId, loadAccess: true);
				if (userSelection == null)
					return new ActionResult((int)ActionResultDelete.EntityNotFound, "UserSelection");

				foreach (UserSelectionAccess access in userSelection.UserSelectionAccess.Where(a => a.State != (int)SoeEntityState.Deleted).ToList())
				{
					ChangeEntityState(entities, access, SoeEntityState.Deleted, false);
				}

				return ChangeEntityState(entities, userSelection, SoeEntityState.Deleted, true);
			}
		}

		#endregion

		#region UserSession

		public class UserSessionIncludingHistoryDTO
		{
			public string Description { get; set; }
			public DateTime Login { get; set; }
			public DateTime? Logout { get; set; }
			public bool MobileLogin { get; set; }
			public bool RemoteLogin { get; set; }
			public string CacheCredentials { get; set; }

			public UserSessionIncludingHistoryDTO(UserSession userSession)
			{
				Description = userSession.Description;
				Login = userSession.Login;
				Logout = userSession.Logout;
				MobileLogin = userSession.MobileLogin;
				RemoteLogin = userSession.RemoteLogin;
				CacheCredentials = userSession.CacheCredentials;
			}

			public UserSessionIncludingHistoryDTO(UserSessionHistory userSessionHistory)
			{
				Description = userSessionHistory.Description;
				Login = userSessionHistory.Login;
				Logout = userSessionHistory.Logout;
				MobileLogin = userSessionHistory.MobileLogin;
				RemoteLogin = userSessionHistory.RemoteLogin;
				CacheCredentials = userSessionHistory.CacheCredentials;
			}
		}

		public IEnumerable<UserSessionIncludingHistoryDTO> GetUserSessionIncludingHistory(int userId)
		{
			using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
			entities.UserSession.NoTracking();
			var userSessions = (from us in entities.UserSession
								where us.User.UserId == userId
								orderby us.Login descending, us.Logout descending
								select us).ToList<UserSession>().ConvertAll<UserSessionIncludingHistoryDTO>(us => new UserSessionIncludingHistoryDTO(us));

			entities.UserSessionHistory.NoTracking();
			var userSessionHistories = (from ush in entities.UserSessionHistory
										where ush.User.UserId == userId
										orderby ush.Login descending, ush.Logout descending
										select ush).ToList<UserSessionHistory>().ConvertAll<UserSessionIncludingHistoryDTO>(ush => new UserSessionIncludingHistoryDTO(ush));

			return userSessions.Concat(userSessionHistories);
		}

		public UserSession GetUserSession(CompEntities entities, int userSessionId)
		{
			return (from us in entities.UserSession
					where us.UserSessonId == userSessionId
					select us).FirstOrDefault<UserSession>();
		}

		public UserSession GetUserSession(CompEntities entities, string token)
		{
			return (from us in entities.UserSession
					.Include("User.License")
					where us.Token == token
					select us).FirstOrDefault<UserSession>();
		}

		public UserSession GetLastUserSession(int userId, bool onlyLast24Hours = false)
		{
			using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
			entities.UserSession.NoTracking();
			return GetLastUserSession(entities, userId, onlyLast24Hours);
		}

		public UserSession GetLastUserSession(CompEntities entities, int userId, bool onlyLast24Hours = false, bool onlyWithEmptyToken = true)
		{
			if (onlyLast24Hours)
			{
				DateTime after = DateTime.Now.AddDays(-1);

				return (from us in entities.UserSession
						where us.User.UserId == userId &&
						us.Login > after
						orderby us.Login descending
						select us).FirstOrDefault<UserSession>();
			}
			else
			{
				return (from us in entities.UserSession
						where us.User.UserId == userId
						orderby us.Login descending
						select us).FirstOrDefault<UserSession>();
			}
		}

		public ActionResult LoginUserSession(int userId, string loginName, int actorCompanyId, string companyName, int? supportUserId = null, bool mobileLogin = false, bool softOneIdLogin = false, string mobileApiVersion = "")
		{
			var result = new ActionResult(false);

			ConnectUtil connectUtil = new ConnectUtil(parameterObject);

			using (CompEntities entities = new CompEntities())
			{
				bool remoteLogin = supportUserId.HasValue;
				string errorPrefix = "";
				if (mobileLogin)
					errorPrefix = "Login UsersSession failed [mobile]";
				else if (remoteLogin)
					errorPrefix = "Login UsersSession failed [remote]";
				else
					errorPrefix = "Login UsersSession failed";

				//Create UserSession
				UserSession userSession = new UserSession()
				{
					Login = DateTime.Now,
					RemoteLogin = remoteLogin,
					MobileLogin = mobileLogin,
					Token = connectUtil.CreateToken(SettingManager.GetStringSetting(entities, SettingMainType.Company, (int)CompanySettingType.CompanyAPIKey, userId, actorCompanyId, 0), "cak", false, ref userId),
				};

				//User
				userSession.User = GetUser(entities, userId);
				if (userSession.User == null)
				{
					result.Success = false;
					result.ErrorMessage = errorPrefix + " Could not find User";
					result.IntegerValue = (int)SoeLoginState.Unknown;
					LogWarning(result.ErrorMessage);
					return result;
				}

				//Role
				var userCompanyRoles = GetUserCompanyRolesByUserAndCompany(entities, userId, actorCompanyId);
				if (userCompanyRoles.IsNullOrEmpty())
				{
					result.Success = false;
					result.ErrorMessage = errorPrefix + " Could not find Role";
					result.IntegerValue = (int)SoeLoginState.RoleNotConnectedToCompany;
					LogWarning(result.ErrorMessage);
					return result;
				}

				//Description
				if (remoteLogin)
				{
					User supportUser = GetUser(supportUserId.Value);
					userSession.Description = $"Support User [{supportUserId}. {supportUser?.Name ?? String.Empty}] logged in to Company {actorCompanyId}. {companyName}] as User [{userId}. {loginName}]";
				}

				//SoftOneId
				if (softOneIdLogin && userSession.User != null)
					userSession.User.idLoginActive = true;

				//Mobile version
				if (!string.IsNullOrEmpty(mobileApiVersion))
					userSession.Platform += $"Mobile Api Version: {mobileApiVersion} {userSession.Platform}";

				result = AddEntityItem(entities, userSession, "UserSession");
				if (result.Success)
					result.StringValue = userSession.UserSessonId.ToString();
				else
					if (log.IsWarnEnabled) log.Warn(errorPrefix + "Could not create");

				return result;
			}
		}

		public ActionResult LogoutUserSession(UserDTO user, int? userSessionId = null, int? supportUserId = null, string description = "")
		{
			var result = new ActionResult(false);

			using (CompEntities entities = new CompEntities())
			{
				UserSession userSession = userSessionId.HasValue ? GetUserSession(entities, userSessionId.Value) : GetLastUserSession(entities, user?.UserId ?? 0);
				if (userSession == null)
				{
					if (log.IsWarnEnabled) log.Warn($"Logout UsersSession failed {(supportUserId.HasValue ? "[remote]" : string.Empty)}. Could not find UserSession [{(userSessionId.HasValue ? userSessionId.Value.ToString() : "LAST")}] User [{(user?.UserId.ToString() ?? "UNKNOWN USERID")}. {(user?.LoginName ?? "UNKNOWN USERNAME")}] ");
					return result;
				}

				if (supportUserId.HasValue)
				{
					StringBuilder sb = new StringBuilder();
					User supportUser = GetUser(supportUserId.Value);
					if (supportUser == null)
						sb.Append("SupportUser not found");
					else
						sb.Append($"Admin User [{supportUserId}. {supportUser.Name}] logged out User [{user?.UserId.ToString() ?? "UNKNOWN USERID"}. {user?.LoginName ?? "UNKNOWN USERNAME]"} ");
					userSession.Description = description + sb.ToString();
				}

				userSession.Logout = DateTime.Now;

				return SaveChanges(entities);
			}
		}

		#endregion
	}
}

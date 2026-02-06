using Newtonsoft.Json;
using SoftOne.Soe.Business.DataCache;
using SoftOne.Soe.Business.DTO;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Business.Util.LogCollector;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Data.Util;
using SoftOne.Soe.Shared.DTO.Events;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;

namespace SoftOne.Soe.Business.Core
{
	public class TimeStampManager : ManagerBase
	{
		#region Variables

		// Create a logger for use in this class
		private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		#endregion

		#region Ctor

		public TimeStampManager(ParameterObject parameterObject) : base(parameterObject) { }

		#endregion

		#region Employee

		/// <summary>
		/// Get one employee based on employee number
		/// </summary>
		/// <param name="actorCompanyId">Company ID</param>
		/// <param name="employeeNr">Employee number</param>
		/// <returns>One employee or null if no employee with specified number exists</returns>
		public GenericType<ActionResultSelect, TSEmployeeItem> GetEmployee(int actorCompanyId, string employeeNr, int? timeTerminalId = null)
		{
			Employee employee = EmployeeManager.GetEmployeeByNr(employeeNr, actorCompanyId, loadContactPerson: true);
			ActionResultSelect result = ActionResultSelect.Unknown;
			if (employee == null)
				return null;

			using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
			bool useAccountHierarchy = base.UseAccountHierarchyOnCompanyFromCache(entities, actorCompanyId);
			if (useAccountHierarchy)
			{
				bool limitToAccount = timeTerminalId.HasValue && GetTimeTerminalBoolSetting(TimeTerminalSettingType.LimitTimeTerminalToAccount, timeTerminalId.Value);
				if (limitToAccount && !GetEmployeeIdsByTimeTerminalAccount(actorCompanyId, timeTerminalId.Value).Contains(employee.EmployeeId))
				{
					// Employee not in this terminal's account
					return null;
				}
			}
			else
			{
				bool limitToCategories = timeTerminalId.HasValue && GetTimeTerminalBoolSetting(TimeTerminalSettingType.LimitTimeTerminalToCategories, timeTerminalId.Value);
				if (limitToCategories && !GetEmployeeIdsByTimeTerminalCategory(actorCompanyId, timeTerminalId.Value).Contains(employee.EmployeeId))
				{
					// Employee not in this terminal's categories
					result = ActionResultSelect.EmployeeFoundButIsNotInTimeTerminalCategory;
				}
			}

			// Get current employee group and make sure it's a stamping group
			EmployeeGroup employeeGroup = employee.CurrentEmployeeGroup;
			if (employeeGroup != null && employeeGroup.AutogenTimeblocks)
				return null;

			if (employeeGroup == null)
			{
				// No current group found, check in the future (happens if employment has not started yet)
				employeeGroup = employee.GetNextEmployeeGroup(DateTime.Today, null);
				if (employeeGroup == null || employeeGroup.AutogenTimeblocks)
					return null;
			}

			return new GenericType<ActionResultSelect, TSEmployeeItem>()
			{
				Field1 = result,
				Field2 = new TSEmployeeItem()
				{
					EmployeeId = employee.EmployeeId,
					EmployeeNr = employee.EmployeeNr,
					Name = employee.ContactPerson.FirstName + " " + employee.ContactPerson.LastName,
					EmployeeGroupId = employeeGroup.EmployeeGroupId,
					Created = employee.Created,
					Modified = employee.Modified,
					State = employee.State
				}
			};
		}

		#endregion

		#region TimeTerminal

		public List<TimeTerminal> GetAllTimeTerminals()
		{
			using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();

			return GetAllTimeTerminals(entitiesReadOnly);
		}

		public List<TimeTerminal> GetAllTimeTerminals(CompEntities entities)
		{
			return (from t in entities.TimeTerminal.Include("Company").Include("TimeTerminalSetting")
					where t.State == (int)SoeEntityState.Active
					select t).ToList();
		}

		public List<Guid> GetAllTimeTerminalGuids()
		{
			using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();

			IQueryable<TimeTerminal> query = entitiesReadOnly.TimeTerminal;
			return query.Where(t => t.State == (int)SoeEntityState.Active).Select(s => s.TimeTerminalGuid).Where(w => w != Guid.Empty).ToList();
		}

		/// <summary>
		/// Get all terminals for specified company
		/// </summary>
		/// <param name="actorCompanyId">Company ID</param>
		/// <param name="type">Terminal type. If 'Unknown', all terminals regardless of type is returned</param>
		/// <param name="onlyActive">If true, only active terminals are returned</param>
		/// <param name="onlyRegistered">If true, only registered terminals are returned</param>
		/// <param name="onlySynchronized">If true, only terminals that has been synchronized at least once will be returned</param>
		/// <param name="loadSettings">If true, terminal settings are loaded</param>
		/// <param name="loadCompanies">If true, company relation is loaded</param>
		/// <param name="loadTypeNames">If true, TypeName is populated with TimeTerminalType TermGroup content</param>
		/// <param name="ignoreLimitToAccount">If true, don't validate which terminals you are allowed to see based on terminal account limit</param>
		/// <returns>Collection of terminals</returns>
		public List<TimeTerminal> GetTimeTerminals(int actorCompanyId, TimeTerminalType type, bool onlyActive, bool onlyRegistered, bool onlySynchronized, bool loadSettings = false, bool loadCompanies = false, bool loadTypeNames = false, bool ignoreLimitToAccount = false)
		{
			List<int> accountIds = null;
			using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
			bool useAccountHierarchy = base.UseAccountHierarchyOnCompanyFromCache(entities, base.ActorCompanyId);
			if (useAccountHierarchy && !ignoreLimitToAccount)
			{
				// Get accounts available for user
				AccountHierarchyInput input = AccountHierarchyInput.GetInstance(AccountHierarchyParamType.IncludeVirtualParented, AccountHierarchyParamType.UseEmployeeAccountIfNoAttestRole);
				accountIds = AccountManager.GetAccountIdsFromHierarchyByUser(base.ActorCompanyId, base.UserId, DateTime.Today, DateTime.Today, input);
				loadSettings = true;
			}

			using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
			entitiesReadOnly.TimeTerminal.NoTracking();
			IQueryable<TimeTerminal> query = entitiesReadOnly.TimeTerminal;

			if (loadSettings)
				query = query.Include("TimeTerminalSetting");

			if (loadCompanies)
				query = query.Include("Company");

			query = query.Where(t => t.State != (int)SoeEntityState.Deleted);

			// Filter on company
			if (actorCompanyId != 0)
				query = query.Where(t => t.ActorCompanyId == actorCompanyId);

			// Filter on type
			if (type != TimeTerminalType.Unknown)
				query = query.Where(t => t.Type == (int)type);

			// Filter on state
			if (onlyActive)
				query = query.Where(t => t.State == (int)SoeEntityState.Active);

			// Filter on registered
			if (onlyRegistered)
				query = query.Where(t => t.Registered);

			// Filter on synchronized
			if (onlySynchronized)
				query = query.Where(t => t.LastSync.HasValue);

			List<TimeTerminal> terminals = query.ToList();

			foreach (TimeTerminal terminal in terminals.ToList())
			{
				// Set type names and/or filter on terminal account
				if (loadTypeNames || accountIds != null)
				{
					if (accountIds != null)
					{
						List<TimeTerminalSetting> settings = terminal.TimeTerminalSetting.Where(s => s.Type == (int)TimeTerminalSettingType.LimitAccount && s.State == (int)SoeEntityState.Active).ToList();
						if (settings.Any())
						{
							List<int> terminalAccountIds = new List<int>();
							foreach (TimeTerminalSetting setting in settings)
							{
								// Previously account was stored as string, now it is stored as int
								int terminalAccountId = 0;
								if (setting.IntData.HasValue)
									terminalAccountId = setting.IntData.Value;
								else
									Int32.TryParse(setting.StrData, out terminalAccountId);

								if (terminalAccountId != 0)
									terminalAccountIds.Add(terminalAccountId);
							}
							if (!terminalAccountIds.Intersect(accountIds).Any())
							{
								terminals.Remove(terminal);
								continue;
							}
						}
					}

					if (loadTypeNames)
						terminal.TypeName = GetText(terminal.Type, (int)TermGroup.TimeTerminalType);
				}

				SetCorrectLastSyncBasedOnTimeZone(terminal);
			}

			return terminals.OrderBy(t => t.Name).ToList();
		}

		public List<TimeTerminal> GetAutoStampOutTerminals()
		{
			using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
			entities.TimeTerminal.NoTracking();
			return (from t in entities.TimeTerminal.Include("Company").Include("TimeTerminalSetting")
					where t.TimeTerminalSetting.Any(s => s.Type == (int)TimeTerminalSettingType.UseAutoStampOut && s.BoolData == true) &&
					t.TimeTerminalSetting.Any(s => s.Type == (int)TimeTerminalSettingType.UseAutoStampOutTime && s.IntData.HasValue)
					select t).ToList();
		}

		public List<int> GetTimeTerminalIdsForPubSub(CompEntities entities, int actorCompanyId)
		{
			return (from t in entities.TimeTerminal
					where t.ActorCompanyId == actorCompanyId &&
					t.Type == (int)TimeTerminalType.GoTimeStamp &&
					t.State == (int)SoeEntityState.Active
					select t.TimeTerminalId).ToList();
		}

		public TimeTerminal GetTimeTerminal(int timeTerminalId, int actorCompanyId, TimeTerminalType type = TimeTerminalType.Unknown)
		{
			using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
			entities.TimeTerminal.NoTracking();
			return GetTimeTerminal(entities, timeTerminalId, actorCompanyId, type);
		}

		public TimeTerminal GetTimeTerminal(CompEntities entities, int timeTerminalId, int actorCompanyId, TimeTerminalType type = TimeTerminalType.Unknown)
		{
			int terminalType = (int)type;

			return (from t in entities.TimeTerminal
						.Include("Company")
						.Include("TimeTerminalSetting")
					where t.TimeTerminalId == timeTerminalId &&
					t.ActorCompanyId == actorCompanyId &&
					(terminalType == (int)TimeTerminalType.Unknown || t.Type == terminalType)
					select t).FirstOrDefault();
		}

		public TimeTerminal GetTimeTerminal(int actorCompanyId, string macAddress, TimeTerminalType type = TimeTerminalType.Unknown)
		{
			using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
			entities.TimeTerminal.NoTracking();
			return GetTimeTerminal(entities, actorCompanyId, macAddress, type);
		}

		public TimeTerminal GetTimeTerminal(CompEntities entities, int actorCompanyId, string macAddress, TimeTerminalType type = TimeTerminalType.Unknown)
		{
			int terminalType = (int)type;

			return (from t in entities.TimeTerminal
						.Include("Company")
						.Include("TimeTerminalSetting")
					where t.ActorCompanyId == actorCompanyId &&
					t.MacAddress == macAddress &&
					(terminalType == (int)TimeTerminalType.Unknown || t.Type == terminalType) &&
					t.State == (int)SoeEntityState.Active
					select t).FirstOrDefault();
		}

		public TimeTerminal GetTimeTerminalDiscardState(int timeTerminalId, bool setCorrectLastSyncBasedOnTimeZone = false)
		{
			using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
			entities.TimeTerminal.NoTracking();
			return GetTimeTerminalDiscardState(entities, timeTerminalId, setCorrectLastSyncBasedOnTimeZone);
		}

		public TimeTerminal GetTimeTerminalDiscardState(CompEntities entities, int timeTerminalId, bool setCorrectLastSyncBasedOnTimeZone = false)
		{
			TimeTerminal terminal = (from t in entities.TimeTerminal
									.Include("Company")
									.Include("TimeTerminalSetting")
									 where t.TimeTerminalId == timeTerminalId
									 select t).FirstOrDefault();

			if (setCorrectLastSyncBasedOnTimeZone)
				SetCorrectLastSyncBasedOnTimeZone(terminal);

			return terminal;
		}

		public TimeTerminal GetTimeTerminalDiscardStateWithCache(int? timeTerminalId, ref List<TimeTerminal> timeTerminals)
		{
			TimeTerminal timeTerminal = null;

			if (timeTerminalId.HasValue)
			{
				timeTerminal = timeTerminals?.FirstOrDefault(i => i.TimeTerminalId == timeTerminalId.Value);
				if (timeTerminal == null)
				{
					timeTerminal = GetTimeTerminalDiscardState(timeTerminalId.Value);
					if (timeTerminal != null && timeTerminals != null)
						timeTerminals.Add(timeTerminal);
				}
			}

			return timeTerminal;
		}

		/// <summary>
		/// Get oldest sync date for all terminals
		/// </summary>
		/// <param name="entities">Object context</param>
		/// <param name="ActorCompanyID">ActorCompanyID</param>
		/// <returns>One terminal or null if no terminal found</returns>
		public DateTime? GetTimeTerminalOldestSync(CompEntities entities, int actorCompanyId)
		{
			DateTime date = DateTime.Now.AddDays(-7);

			TimeTerminal terminal = (from t in entities.TimeTerminal
									 where t.ActorCompanyId == actorCompanyId &&
									 t.Registered &&
									 t.Type != (int)TimeTerminalType.WebTimeStamp &&
									 t.LastSync.HasValue && t.LastSync >= date &&
									 t.State == (int)SoeEntityState.Active
									 select t).OrderBy(t => t.LastSync).FirstOrDefault();

			return (terminal != null && terminal.LastSync != null) ? terminal.LastSync : null;
		}

		public bool TimeTerminalExists(CompEntities entities, string timeTerminalName, int actorCompanyId)
		{
			return (from t in entities.TimeTerminal
					where t.Name == timeTerminalName &&
					t.ActorCompanyId == actorCompanyId &&
					t.State == (int)SoeEntityState.Active
					select t).Any();
		}

		/// <summary>
		/// Get AccountDim for specified terminal.
		/// If timeTerminalId is 0, take first terminal on specified company with an account dim setting.
		/// </summary>
		/// <param name="actorCompanyId">Company ID</param>
		/// <param name="timeTerminalId">Terminal ID</param>
		/// <param name="dimNr">1 or 2</param>
		/// <returns>GenericType where Id is the AccountDimId and Name is the AccountDim name</returns>
		public AccountDimSmallDTO GetTimeTerminalAccountDim(int actorCompanyId, int timeTerminalId, int dimNr)
		{
			AccountDimSmallDTO dto;
			int accountDimId = 0;
			if (timeTerminalId != 0)
			{
				// A terminal is specified, get AccountDimId from setting
				accountDimId = GetTimeTerminalIntSetting(dimNr == 1 ? TimeTerminalSettingType.AccountDim : TimeTerminalSettingType.AccountDim2, timeTerminalId);
			}
			else
			{
				// No terminal specified, loop through all terminals for specified company
				using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
				entities.TimeTerminal.NoTracking();
				List<TimeTerminal> terminals = (from t in entities.TimeTerminal.Include("TimeTerminalSetting")
												where t.ActorCompanyId == actorCompanyId &&
												t.State == (int)SoeEntityState.Active
												select t).ToList();

				foreach (TimeTerminal terminal in terminals)
				{
					if (terminal.Type == (int)TimeTerminalType.XETimeStamp && !terminal.Registered)
						continue;

					// If the terminal has a specified AccountDim ID, return it, otherwise check the next terminal
					accountDimId = GetTimeTerminalIntSetting(dimNr == 1 ? TimeTerminalSettingType.AccountDim : TimeTerminalSettingType.AccountDim2, terminal);
					if (accountDimId != 0)
						break;
				}
			}

			AccountDim accountDim = accountDimId > 0 ? AccountManager.GetAccountDim(accountDimId, actorCompanyId) : null;
			if (accountDim != null)
			{
				if (!accountDim.ParentReference.IsLoaded)
					accountDim.ParentReference.Load();
				dto = accountDim.ToSmallDTO();
			}
			else
			{
				dto = new AccountDimSmallDTO()
				{
					AccountDimId = accountDimId
				};
			}

			return dto;
		}

		public int GetTimeTerminalAccountDimId(CompEntities entities, int actorCompanyId, int timeTerminalId)
		{
			int accountDimId = 0;
			if (timeTerminalId != 0)
			{
				// A terminal is specified, get AccountDimId from setting
				accountDimId = GetTimeTerminalIntSetting(entities, TimeTerminalSettingType.AccountDim, timeTerminalId);
			}
			else
			{
				// No terminal specified, loop through all terminals for specified company
				List<TimeTerminal> terminals = (from t in entities.TimeTerminal.Include("TimeTerminalSetting")
												where t.ActorCompanyId == actorCompanyId &&
												t.State == (int)SoeEntityState.Active &&
												t.Registered
												select t).ToList();

				foreach (TimeTerminal terminal in terminals)
				{
					// If the terminal has a specified AccountDim ID, return it, otherwise check the next terminal
					accountDimId = GetTimeTerminalIntSetting(entities, TimeTerminalSettingType.AccountDim, terminal.TimeTerminalId);
					if (accountDimId != 0)
						break;
				}
			}

			return accountDimId;
		}

		public int GetTimeTerminalDefaultLanguage(CompEntities entities, int actorCompanyId, int timeTerminalId)
		{
			int? fromCache = BusinessMemoryCache<int?>.Get($"GetTimeTerminalDefaultLanguage#a{actorCompanyId}#t{timeTerminalId}");

			if (fromCache.HasValue)
				return fromCache.Value;

			int defaultLanguage = 0;
			if (timeTerminalId != 0)
				defaultLanguage = GetTimeTerminalIntSetting(entities, TimeTerminalSettingType.Languages, timeTerminalId);

			if (defaultLanguage == 0)
				defaultLanguage = 1;

			BusinessMemoryCache<int?>.Set($"GetTimeTerminalDefaultLanguage#a{actorCompanyId}#t{timeTerminalId}", defaultLanguage, 60 * 5);

			return defaultLanguage;
		}

		public List<int> GetAccountIdsByTimeTerminal(int timeTerminalId)
		{
			using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
			return GetAccountIdsByTimeTerminal(entities, timeTerminalId);
		}

		public List<int> GetCachedAccountIdsByTimeTerminal(CompEntities entities, int timeTerminalId, bool useCache = true)
		{
			string key = $"GetCachedAccountIdsByTimeTerminal#{timeTerminalId}";
			List<int> accountIds = useCache ? BusinessMemoryCache<List<int>>.Get(key) : null;
			if (accountIds == null)
			{
				accountIds = GetAccountIdsByTimeTerminal(entities, timeTerminalId);
				BusinessMemoryCache<List<int>>.Set(key, accountIds, 60);
			}
			return accountIds;
		}

		public List<int> GetAccountIdsByTimeTerminal(CompEntities entities, int timeTerminalId)
		{
			List<int> terminalAccountIds = new List<int>();
			List<TimeTerminalSetting> settings = GetTimeTerminalSettings(entities, timeTerminalId).Where(s => s.Type == (int)TimeTerminalSettingType.LimitAccount).ToList();
			foreach (TimeTerminalSetting setting in settings)
			{
				// Previously account was stored as string, now it is stored as int
				int terminalAccountId = 0;
				if (setting.IntData.HasValue)
					terminalAccountId = setting.IntData.Value;
				else
					Int32.TryParse(setting.StrData, out terminalAccountId);

				if (terminalAccountId != 0)
					terminalAccountIds.Add(terminalAccountId);
			}

			return terminalAccountIds;
		}

		public List<Category> GetCategoriesByTimeTerminal(int actorCompanyId, int timeTerminalId, bool loadCategoryRecords = false)
		{
			using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
			entities.Category.NoTracking();
			return GetCategoriesByTimeTerminal(entities, actorCompanyId, timeTerminalId, loadCategoryRecords);
		}

		public List<Category> GetCategoriesByTimeTerminal(CompEntities entities, int actorCompanyId, int timeTerminalId, bool loadCategoryRecords = false)
		{
			IQueryable<Category> query = (from c in entities.Category
										  join cr in entities.CompanyCategoryRecord
										  on c.CategoryId equals cr.CategoryId
										  where c.State == (int)SoeEntityState.Active &&
										  cr.Entity == (int)SoeCategoryRecordEntity.TimeTerminal &&
										  c.Type == (int)SoeCategoryType.Employee &&
										  cr.RecordId == timeTerminalId &&
										  c.ActorCompanyId == actorCompanyId
										  select c);

			if (loadCategoryRecords)
				query = query.Include("CompanyCategoryRecord");

			return query.ToList();
		}

		public ActionResult SaveTimeTerminal(TimeTerminalDTO terminalInput, int actorCompanyId)
		{
			if (terminalInput == null)
				return new ActionResult((int)ActionResultSave.EntityIsNull, "TimeTerminal");

			// Default result is successful
			ActionResult result = new ActionResult();

			int terminalId = terminalInput.TimeTerminalId;

			using (CompEntities entities = new CompEntities())
			{
				try
				{
					#region TimeTerminal

					// Get existing terminal
					TimeTerminal terminal = GetTimeTerminal(entities, terminalId, actorCompanyId);
					if (terminal == null)
					{
						#region Add

						if (TimeTerminalExists(entities, terminalInput.Name, actorCompanyId))
							return new ActionResult((int)ActionResultSave.EntityExists, GetText(10113, "En terminal med samma namn finns redan. Byt namn och försök igen."));

						terminal = new TimeTerminal()
						{
							ActorCompanyId = actorCompanyId,
							TimeTerminalGuid = Guid.NewGuid()
						};
						SetCreatedProperties(terminal);
						entities.TimeTerminal.AddObject(terminal);

						#endregion
					}
					else
					{
						#region Update

						SetModifiedProperties(terminal);

						#endregion
					}

					#region Set fields

					terminal.Type = (int)terminalInput.Type;
					terminal.Name = terminalInput.Name;
					terminal.MacAddress = terminalInput.MacAddress;
					terminal.MacName = terminalInput.MacName;
					terminal.MacNumber = terminalInput.MacNumber;
					terminal.Registered = terminalInput.Registered;
					terminal.State = (int)terminalInput.State;

					#endregion

					#endregion

					#region Settings

					List<TimeTerminalSettingType> validSettingTypes = GetValidSettingTypes(terminalInput.Type);

					foreach (TimeTerminalSettingDTO settingInput in terminalInput.TimeTerminalSettings)
					{
						TimeTerminalSetting originalSetting = terminal.TimeTerminalSetting.FirstOrDefault(s => s.Type == (int)settingInput.Type && !s.ParentId.HasValue);

						if (validSettingTypes.Contains(settingInput.Type))
						{
							if (originalSetting == null)
							{
								// Add new setting
								originalSetting = new TimeTerminalSetting()
								{
									Type = (int)settingInput.Type,
									DataType = (int)settingInput.DataType,
									Name = Enum.GetName(typeof(TimeTerminalSettingType), settingInput.Type),
								};
								SetCreatedProperties(originalSetting);
								terminal.TimeTerminalSetting.Add(originalSetting);
							}
							else
							{
								SetModifiedProperties(originalSetting);

								// LimitAccount setting has moved from string to int, update old values
								if (settingInput.Type == TimeTerminalSettingType.LimitAccount && originalSetting.DataType != (int)TimeTerminalSettingDataType.Integer)
									originalSetting.DataType = (int)TimeTerminalSettingDataType.Integer;
							}

							// Update existing setting
							// Only the value can be updated
							originalSetting.StrData = settingInput.StrData;
							originalSetting.IntData = settingInput.IntData;
							originalSetting.DecimalData = settingInput.DecimalData;
							originalSetting.BoolData = settingInput.BoolData;
							originalSetting.DateData = settingInput.DateData;
							originalSetting.TimeData = settingInput.TimeData;
							originalSetting.State = (int)SoeEntityState.Active;

							#region Children

							// Always delete children and recreate below if they still exists
							List<TimeTerminalSetting> originalChildren = terminal.TimeTerminalSetting.Where(s => s.Type == (int)settingInput.Type && s.ParentId.HasValue).ToList();
							foreach (TimeTerminalSetting originalChild in originalChildren)
							{
								entities.DeleteObject(originalChild);
							}

							if (settingInput.HasChildren)
							{
								// Add new children
								foreach (TimeTerminalSettingDTO childInput in settingInput.Children)
								{
									TimeTerminalSetting newChild = new TimeTerminalSetting()
									{
										Parent = originalSetting,
										Type = (int)settingInput.Type,
										DataType = (int)settingInput.DataType,
										Name = Enum.GetName(typeof(TimeTerminalSettingType), settingInput.Type),
										StrData = childInput.StrData,
										IntData = childInput.IntData,
										DecimalData = childInput.DecimalData,
										BoolData = childInput.BoolData,
										DateData = childInput.DateData,
										TimeData = childInput.TimeData,
										State = (int)SoeEntityState.Active
									};
									SetCreatedProperties(newChild);
									terminal.TimeTerminalSetting.Add(newChild);
								}
							}

							#endregion
						}
						else
						{
							// Setting not valid for terminal type, delete it
							if (originalSetting != null && originalSetting.State == (int)SoeEntityState.Active)
							{
								// Children
								List<TimeTerminalSetting> originalChildren = terminal.TimeTerminalSetting.Where(s => s.Type == (int)settingInput.Type && s.ParentId.HasValue).ToList();
								foreach (TimeTerminalSetting originalChild in originalChildren)
								{
									entities.DeleteObject(originalChild);
								}

								entities.DeleteObject(originalSetting);
							}
						}
					}

					#endregion

					result = SaveChanges(entities);
					if (result.Success)
					{
						terminalId = terminal.TimeTerminalId;

						#region Categories

						#region Update/Delete CompanyCategoryRecord

						CompanyCategoryRecord categoryRecord = null;
						List<CompanyCategoryRecord> categoryRecords = CategoryManager.GetCompanyCategoryRecords(entities, SoeCategoryType.Employee, SoeCategoryRecordEntity.TimeTerminal, terminalId, actorCompanyId);
						for (int r = categoryRecords.Count - 1; r >= 0; r--)
						{
							categoryRecord = categoryRecords[r];

							// Check for existing CompanyCategoryRecord with same Category as input item
							if (terminalInput.CategoryIds.Contains(categoryRecord.CategoryId))
							{
								// Remove from input collection
								terminalInput.CategoryIds.Remove(categoryRecord.CategoryId);
							}
							else
							{
								// Delete existing CategoryRecord
								entities.DeleteObject(categoryRecord);
								categoryRecords.Remove(categoryRecord);
							}
						}

						#endregion

						#region Add CompanyCategoryRecord

						foreach (int categoryId in terminalInput.CategoryIds)
						{
							// Prevent duplicates
							if (categoryRecords.Count > 0)
							{
								categoryRecord = categoryRecords.FirstOrDefault(i => i.CategoryId == categoryId);
								if (categoryRecord != null)
									continue;
							}

							categoryRecord = new CompanyCategoryRecord()
							{
								CategoryId = categoryId,
								ActorCompanyId = actorCompanyId,
								RecordId = terminalId,
								Entity = (int)SoeCategoryRecordEntity.TimeTerminal,
								Default = false,
								DateFrom = null,
								DateTo = null,
								IsExecutive = true,
							};
							entities.CompanyCategoryRecord.AddObject(categoryRecord);

							// Add to collection to be able to check for duplicates
							categoryRecords.Add(categoryRecord);
						}

						#endregion

						result = SaveChanges(entities);

						if (result.Success && terminalInput.Type == TimeTerminalType.GoTimeStamp)
							base.WebPubSubSendMessage(terminal.GetTerminalPubSubKey(), terminal.GetUpdateMessage());

						#endregion
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
						// Set success properties
						result.IntegerValue = terminalId;
					}
					else
						base.LogTransactionFailed(this.ToString(), this.log);
				}

				return result;
			}
		}

		/// <summary>
		/// Add a new terminal
		/// </summary>
		/// <param name="timeTerminal">Terminal object to create</param>
		/// <param name="actorCompanyId">Company ID</param>
		/// <returns>ActionResult</returns>
		public ActionResult AddTimeTerminal(TimeTerminal timeTerminal, int actorCompanyId)
		{
			ActionResult result;

			if (timeTerminal == null)
				return new ActionResult((int)ActionResultSave.EntityIsNull);

			using (CompEntities entities = new CompEntities())
			{
				timeTerminal.Company = CompanyManager.GetCompany(entities, actorCompanyId);
				timeTerminal.TimeTerminalGuid = Guid.NewGuid();
				if (timeTerminal.Company == null)
					return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte"));

				result = AddEntityItem(entities, timeTerminal, "TimeTerminal");
				if (result.Success)
					result.IntegerValue = timeTerminal.TimeTerminalId;
			}

			return result;
		}

		/// <summary>
		/// Update existing terminal
		/// </summary>
		/// <param name="timeTerminal">Terminal object with updated properties</param>
		/// <returns>ActionResult</returns>
		public ActionResult UpdateTimeTerminal(TimeTerminal timeTerminal)
		{
			if (timeTerminal == null)
				return new ActionResult((int)ActionResultSave.EntityIsNull, "TimeTerminal");

			using (CompEntities entities = new CompEntities())
			{
				TimeTerminal originalTimeTerminal = GetTimeTerminalDiscardState(entities, timeTerminal.TimeTerminalId);
				if (originalTimeTerminal == null)
					return new ActionResult((int)ActionResultSave.EntityNotFound, "TimeTerminal");

				return UpdateEntityItem(entities, originalTimeTerminal, timeTerminal, "TimeTerminal");
			}
		}

		public ActionResult UpdateDbSchemaVersion(int timeTerminalId, int dbSchemaVersion, string assemblyVersion)
		{
			ActionResult result = new ActionResult();

			using (var entities = new CompEntities())
			{
				TimeTerminal timeTerminal = null;
				if (timeTerminalId > 0)
				{
					timeTerminal = (from entry in entities.TimeTerminal
									where entry.TimeTerminalId == timeTerminalId
									select entry).FirstOrDefault();

					if (timeTerminal == null)
						return new ActionResult(false, (int)ActionResultSave.EntityIsNull, "Error, could not find the timeterminal with id " + timeTerminalId);

					timeTerminal.TerminalDbSchemaVersion = dbSchemaVersion;
					timeTerminal.TerminalVersion = assemblyVersion;

					result = SaveChanges(entities);
				}

				// Get the version of timestamp that XE currently support (this version needs to be updated when needed)
				var minTimeTerminalVersion = SettingManager.GetStringSetting(entities, SettingMainType.Application, (int)ApplicationSettingType.MinimumTimeStampVersion, 0, 0, 0);

				// These following versions should not be used since they causes errors
				if (assemblyVersion != null && (assemblyVersion == "1.1.1.0" || assemblyVersion == "1.1.1.1" || assemblyVersion == "1.1.1.2"))
				{
					minTimeTerminalVersion = "1.2";
				}

				// String value is the minimum assembly version to be used
				result.StringValue = minTimeTerminalVersion;

				// Value is the download link to latest version.
				result.Value = "http://txe.softone.se/timestamp/";

				// Value2 is the latest version (but not required to download).
				result.Value2 = "1.4";

				return result;
			}
		}

		/// <summary>
		/// Update registered status on specified terminal
		/// </summary>
		/// <param name="timeTerminalId">TimeTerminal ID</param>
		/// <param name="registered">Set to true when register, false when resetting terminal</param>
		/// <returns>ActionResult</returns>
		public ActionResult UpdateTimeTerminalRegisteredStatus(int timeTerminalId, bool registered)
		{
			ActionResult result = new ActionResult();

			using (CompEntities entities = new CompEntities())
			{
				TimeTerminal originalTimeTerminal = GetTimeTerminalDiscardState(entities, timeTerminalId);
				if (originalTimeTerminal == null)
					return new ActionResult((int)ActionResultSave.EntityNotFound, "TimeTerminal");

				originalTimeTerminal.Registered = registered;
				result.ObjectsAffected = entities.SaveChanges();
				if (result.ObjectsAffected == 0)
				{
					result.Success = false;
					result.ErrorNumber = (int)ActionResultSave.NothingSaved;
				}
			}

			return result;
		}

		public ActionResult AddEmployeeToTerminal(int actorCompanyId, int timeTerminalId, int employeeId)
		{
			Employee employee = EmployeeManager.GetEmployeeIgnoreState(actorCompanyId, employeeId);
			if (employee == null)
				return new ActionResult((int)ActionResultSave.EntityNotFound, "Employee");

			// Update, only supports adding category if it does not exist for the employee
			List<Category> categories = GetCategoriesByTimeTerminal(actorCompanyId, timeTerminalId);
			using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
			List<int> employeeIds = EmployeeManager.GetEmployeeIdsByCategoryIds(entitiesReadOnly, actorCompanyId, categories.Select(c => c.CategoryId));
			if (employeeIds.Contains(employeeId))
				return new ActionResult() { ErrorNumber = (int)ActionResultSave.NothingSaved };

			return CategoryManager.AddCompanyCategoryRecord(employee.EmployeeId, categories.FirstOrDefault().CategoryId, SoeCategoryRecordEntity.Employee, actorCompanyId);
		}

		private void SetCorrectLastSyncBasedOnTimeZone(TimeTerminal terminal)
		{
			if (terminal != null && terminal.LastSync.HasValue)
			{
				double offset = GetTimeZoneOffsetFromDefault(terminal.TimeTerminalId);
				if (offset != 0)
					terminal.LastSync = terminal.LastSync.Value.AddHours(offset);
			}
		}

		#endregion

		#region TimeTerminalSetting

		/// <summary>
		/// Get all settings for specified terminal
		/// </summary>
		/// <param name="timeTerminalId">TimeTerminal ID</param>
		/// <returns>Collection of settings</returns>
		public List<TimeTerminalSetting> GetTimeTerminalSettings(int timeTerminalId)
		{
			using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
			entities.TimeTerminalSetting.NoTracking();
			return GetTimeTerminalSettings(entities, timeTerminalId);
		}

		public List<TimeTerminalSetting> GetTimeTerminalSettings(CompEntities entities, int timeTerminalId)
		{
			return (from s in entities.TimeTerminalSetting
					where s.TimeTerminal.TimeTerminalId == timeTerminalId &&
					s.State == (int)SoeEntityState.Active
					select s).ToList();
		}

		public List<TimeTerminalSetting> GetTimeTerminalSettings(TimeTerminalSettingType type, int timeTerminalId)
		{
			using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
			entities.TimeTerminalSetting.NoTracking();
			return GetTimeTerminalSettings(entities, type, timeTerminalId);
		}

		public List<TimeTerminalSetting> GetTimeTerminalSettings(CompEntities entities, TimeTerminalSettingType type, int timeTerminalId)
		{
			int settingType = (int)type;

			return (from s in entities.TimeTerminalSetting
					where s.TimeTerminal.TimeTerminalId == timeTerminalId &&
					s.Type == settingType &&
					s.State == (int)SoeEntityState.Active
					select s).ToList();
		}

		/// <summary>
		/// Get one terminal setting
		/// </summary>
		/// <param name="type">Terminal setting type</param>
		/// <param name="timeTerminalId">Terminal ID</param>
		/// <returns>One terminal setting or null if no setting found</returns>
		public TimeTerminalSetting GetTimeTerminalSetting(TimeTerminalSettingType type, int timeTerminalId, bool onlyParent = true)
		{
			using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
			entities.TimeTerminalSetting.NoTracking();
			return GetTimeTerminalSetting(entities, type, timeTerminalId, onlyParent);
		}

		/// <summary>
		/// Get one terminal setting
		/// </summary>
		/// <param name="entities">Object context</param>
		/// <param name="type">Terminal setting type</param>
		/// <param name="timeTerminalId">Terminal ID</param>
		/// <returns>One terminal setting or null if no setting found</returns>
		public TimeTerminalSetting GetTimeTerminalSetting(CompEntities entities, TimeTerminalSettingType type, int timeTerminalId, bool onlyParent = true)
		{
			int settingType = (int)type;

			return (from s in entities.TimeTerminalSetting.Include("TimeTerminal")
					where s.TimeTerminal.TimeTerminalId == timeTerminalId &&
					s.Type == settingType &&
					(!s.ParentId.HasValue || !onlyParent) &&
					s.State == (int)SoeEntityState.Active
					select s).FirstOrDefault();
		}

		public TimeTerminalSetting GetTimeTerminalSetting(TimeTerminalSettingType type, TimeTerminal terminal)
		{
			return terminal?.TimeTerminalSetting?.FirstOrDefault(s => s.Type == (int)type && s.State == (int)SoeEntityState.Active);
		}

		public bool GetTimeTerminalBoolSetting(CompEntities entities, TimeTerminalSettingType type, int timeTerminalId)
		{
			var key = $"GetTimeTerminalBoolSetting#{timeTerminalId}#{type}";
			bool? fromCache = BusinessMemoryCache<bool?>.Get(key);

			if (fromCache.HasValue)
				return fromCache.Value;

			TimeTerminalSetting setting = this.GetTimeTerminalSetting(entities, type, timeTerminalId);
			var value = setting != null && setting.BoolData == true;
			BusinessMemoryCache<bool?>.Set(key, value, 60);

			return value;
		}

		public bool GetTimeTerminalBoolSetting(TimeTerminalSettingType type, int timeTerminalId)
		{
			var key = $"GetTimeTerminalBoolSetting#{timeTerminalId}#{type}";
			bool? fromCache = BusinessMemoryCache<bool?>.Get(key);

			if (fromCache.HasValue)
				return fromCache.Value;

			TimeTerminalSetting setting = this.GetTimeTerminalSetting(type, timeTerminalId);
			var value = setting != null && setting.BoolData == true;
			BusinessMemoryCache<bool?>.Set(key, value, 60);
			return value;
		}

		public bool GetTimeTerminalBoolSetting(TimeTerminalSettingType type, TimeTerminal terminal)
		{
			var key = $"GetTimeTerminalBoolSetting#{terminal.TimeTerminalId}#{type}";
			bool? fromCache = BusinessMemoryCache<bool?>.Get(key);

			if (fromCache.HasValue)
				return fromCache.Value;

			TimeTerminalSetting setting = GetTimeTerminalSetting(type, terminal);
			var value = setting != null && setting.BoolData == true;

			BusinessMemoryCache<bool?>.Set(key, value, 60);
			return value;
		}

		public int GetTimeTerminalIntSetting(TimeTerminalSettingType type, int timeTerminalId)
		{
			var key = $"GetTimeTerminalIntSetting#{timeTerminalId}#{type}";
			int? fromCache = BusinessMemoryCache<int?>.Get(key);

			if (fromCache.HasValue)
				return fromCache.Value;

			TimeTerminalSetting setting = this.GetTimeTerminalSetting(type, timeTerminalId);
			var value = setting != null && setting.IntData.HasValue ? setting.IntData.Value : 0;

			BusinessMemoryCache<int?>.Set(key, value, 60);
			return value;
		}

		public int GetTimeTerminalIntSetting(CompEntities entities, TimeTerminalSettingType type, int timeTerminalId)
		{
			var key = $"GetTimeTerminalIntSetting#{timeTerminalId}#{type}";
			int? fromCache = BusinessMemoryCache<int?>.Get(key);

			if (fromCache.HasValue)
				return fromCache.Value;

			TimeTerminalSetting setting = this.GetTimeTerminalSetting(entities, type, timeTerminalId);
			var value = setting != null && setting.IntData.HasValue ? setting.IntData.Value : 0;

			BusinessMemoryCache<int?>.Set(key, value, 60);
			return value;
		}

		public int GetTimeTerminalIntSetting(TimeTerminalSettingType type, TimeTerminal terminal)
		{
			var key = $"GetTimeTerminalIntSetting#{terminal.TimeTerminalId}#{type}";
			int? fromCache = BusinessMemoryCache<int?>.Get(key);

			if (fromCache.HasValue)
				return fromCache.Value;

			TimeTerminalSetting setting = GetTimeTerminalSetting(type, terminal);
			var value = setting != null && setting.IntData.HasValue ? setting.IntData.Value : 0;

			BusinessMemoryCache<int?>.Set(key, value, 60);
			return value;
		}

		public string GetTimeTerminalStringSetting(TimeTerminalSettingType type, int timeTerminalId)
		{
			var key = $"GetTimeTerminalStringSetting#{timeTerminalId}#{type}";
			string fromCache = BusinessMemoryCache<string>.Get(key);

			if (!string.IsNullOrEmpty(fromCache))
				return fromCache;

			var setting = this.GetTimeTerminalSetting(type, timeTerminalId);
			var value = setting != null ? setting.StrData : string.Empty;

			BusinessMemoryCache<string>.Set(key, value, 60);
			return value;
		}

		public string GetTimeTerminalStringSetting(TimeTerminalSettingType type, TimeTerminal terminal)
		{
			var key = $"GetTimeTerminalStringSetting#{terminal.TimeTerminalId}#{type}";
			string fromCache = BusinessMemoryCache<string>.Get(key);

			if (!string.IsNullOrEmpty(fromCache))
				return fromCache;

			TimeTerminalSetting setting = GetTimeTerminalSetting(type, terminal);
			var value = setting != null ? setting.StrData : string.Empty;

			BusinessMemoryCache<string>.Set(key, value, 60);
			return value;
		}

		public List<TimeTerminalSettingType> GetValidSettingTypes(TimeTerminalType terminalType)
		{
			List<TimeTerminalSettingType> settingTypes = new List<TimeTerminalSettingType>();

			switch (terminalType)
			{
				case TimeTerminalType.TimeSpot:
					settingTypes.Add(TimeTerminalSettingType.AccountDim);
					settingTypes.Add(TimeTerminalSettingType.LimitTimeTerminalToAccount);
					settingTypes.Add(TimeTerminalSettingType.LimitTimeTerminalToCategories);
					settingTypes.Add(TimeTerminalSettingType.InternalAccountDim1Id);
					settingTypes.Add(TimeTerminalSettingType.LimitAccount);
					break;
				case TimeTerminalType.XETimeStamp:
					settingTypes.Add(TimeTerminalSettingType.SyncInterval);
					settingTypes.Add(TimeTerminalSettingType.InactivityDelay);
					settingTypes.Add(TimeTerminalSettingType.AccountDim);
					settingTypes.Add(TimeTerminalSettingType.MaximizeWindow);
					settingTypes.Add(TimeTerminalSettingType.NewEmployee);
					settingTypes.Add(TimeTerminalSettingType.SysCountryId);
					settingTypes.Add(TimeTerminalSettingType.LimitTimeTerminalToCategories);
					settingTypes.Add(TimeTerminalSettingType.ForceCauseGraceMinutes);
					settingTypes.Add(TimeTerminalSettingType.ForceCauseIfOutOfSchedule);
					settingTypes.Add(TimeTerminalSettingType.OnlyStampWithTag);
					settingTypes.Add(TimeTerminalSettingType.ForceSocialSecNbr);
					settingTypes.Add(TimeTerminalSettingType.UseAutoStampOut);
					settingTypes.Add(TimeTerminalSettingType.UseAutoStampOutTime);
					settingTypes.Add(TimeTerminalSettingType.HideAttendanceView);
					settingTypes.Add(TimeTerminalSettingType.ShowTimeInAttendanceView);
					settingTypes.Add(TimeTerminalSettingType.InternalAccountDim1Id);
					settingTypes.Add(TimeTerminalSettingType.AdjustTime);
					settingTypes.Add(TimeTerminalSettingType.ShowOnlyBreakAcc);
					settingTypes.Add(TimeTerminalSettingType.HideInformationButton);
					settingTypes.Add(TimeTerminalSettingType.LimitAccount);
					settingTypes.Add(TimeTerminalSettingType.LimitTimeTerminalToAccount);
					settingTypes.Add(TimeTerminalSettingType.ForceCorrectTypeTimelineOrder);
					settingTypes.Add(TimeTerminalSettingType.OnlyDigitsInCardNumber);
					break;
				case TimeTerminalType.WebTimeStamp:
					settingTypes.Add(TimeTerminalSettingType.AccountDim);
					settingTypes.Add(TimeTerminalSettingType.SysCountryId);
					settingTypes.Add(TimeTerminalSettingType.LimitTimeTerminalToCategories);
					settingTypes.Add(TimeTerminalSettingType.UseAutoStampOut);
					settingTypes.Add(TimeTerminalSettingType.UseAutoStampOutTime);
					settingTypes.Add(TimeTerminalSettingType.HideAttendanceView);
					settingTypes.Add(TimeTerminalSettingType.ShowTimeInAttendanceView);
					settingTypes.Add(TimeTerminalSettingType.InternalAccountDim1Id);
					settingTypes.Add(TimeTerminalSettingType.ShowPicturesInAttendenceView);
					settingTypes.Add(TimeTerminalSettingType.IpFilter);
					settingTypes.Add(TimeTerminalSettingType.AdjustTime);
					settingTypes.Add(TimeTerminalSettingType.LimitTimeTerminalToAccount);
					settingTypes.Add(TimeTerminalSettingType.LimitAccount);
					break;
				case TimeTerminalType.GoTimeStamp:
					settingTypes.Add(TimeTerminalSettingType.InactivityDelay);
					settingTypes.Add(TimeTerminalSettingType.AccountDim);
					//settingTypes.Add(TimeTerminalSettingType.NewEmployee);
					settingTypes.Add(TimeTerminalSettingType.LimitTimeTerminalToCategories);
					settingTypes.Add(TimeTerminalSettingType.ForceCauseIfOutOfSchedule);
					//settingTypes.Add(TimeTerminalSettingType.ForceSocialSecNbr);
					settingTypes.Add(TimeTerminalSettingType.UseAutoStampOut);
					settingTypes.Add(TimeTerminalSettingType.UseAutoStampOutTime);
					settingTypes.Add(TimeTerminalSettingType.HideAttendanceView);
					settingTypes.Add(TimeTerminalSettingType.ShowTimeInAttendanceView);
					settingTypes.Add(TimeTerminalSettingType.InternalAccountDim1Id);
					//settingTypes.Add(TimeTerminalSettingType.ShowPicturesInAttendenceView);
					settingTypes.Add(TimeTerminalSettingType.IpFilter);
					settingTypes.Add(TimeTerminalSettingType.LimitTimeTerminalToAccount);
					settingTypes.Add(TimeTerminalSettingType.LimitAccount);
					settingTypes.Add(TimeTerminalSettingType.ForceCorrectTypeTimelineOrder);
					settingTypes.Add(TimeTerminalSettingType.OnlyDigitsInCardNumber);
					settingTypes.Add(TimeTerminalSettingType.StartpageSubject);
					settingTypes.Add(TimeTerminalSettingType.StartpageShortText);
					settingTypes.Add(TimeTerminalSettingType.StartpageText);
					settingTypes.Add(TimeTerminalSettingType.IdentifyType);
					settingTypes.Add(TimeTerminalSettingType.Languages);
					settingTypes.Add(TimeTerminalSettingType.ShowActionBreak);
					settingTypes.Add(TimeTerminalSettingType.TimeZone);
					settingTypes.Add(TimeTerminalSettingType.ShowCurrentSchedule);
					settingTypes.Add(TimeTerminalSettingType.ShowNextSchedule);
					settingTypes.Add(TimeTerminalSettingType.ShowBreakAcc);
					settingTypes.Add(TimeTerminalSettingType.ShowOtherAcc);
					settingTypes.Add(TimeTerminalSettingType.LimitSelectableAccounts);
					settingTypes.Add(TimeTerminalSettingType.SelectedAccounts);
					settingTypes.Add(TimeTerminalSettingType.ForceCauseGraceMinutesOutsideSchedule);
					settingTypes.Add(TimeTerminalSettingType.ForceCauseGraceMinutesInsideSchedule);
					settingTypes.Add(TimeTerminalSettingType.ValidateNoSchedule);
					settingTypes.Add(TimeTerminalSettingType.ValidateAbsence);
					settingTypes.Add(TimeTerminalSettingType.AttendanceViewSortOrder);
					settingTypes.Add(TimeTerminalSettingType.ShowNotificationWhenStamping);
					settingTypes.Add(TimeTerminalSettingType.IgnoreForceCauseOnBreak);
					settingTypes.Add(TimeTerminalSettingType.TerminalGroupName);
					settingTypes.Add(TimeTerminalSettingType.ShowUnreadInformation);
					settingTypes.Add(TimeTerminalSettingType.ShowTimeStampHistory);
					settingTypes.Add(TimeTerminalSettingType.UseDistanceWork);
					settingTypes.Add(TimeTerminalSettingType.DistanceWorkButtonName);
					settingTypes.Add(TimeTerminalSettingType.DistanceWorkButtonIcon);
					settingTypes.Add(TimeTerminalSettingType.BreakIsPaid);
					settingTypes.Add(TimeTerminalSettingType.BreakTimeDeviationCause);
					settingTypes.Add(TimeTerminalSettingType.BreakButtonName);
					settingTypes.Add(TimeTerminalSettingType.BreakButtonIcon);
					settingTypes.Add(TimeTerminalSettingType.ShowActionBreakAlt);
					settingTypes.Add(TimeTerminalSettingType.BreakAltIsPaid);
					settingTypes.Add(TimeTerminalSettingType.BreakAltTimeDeviationCause);
					settingTypes.Add(TimeTerminalSettingType.BreakAltButtonName);
					settingTypes.Add(TimeTerminalSettingType.BreakAltButtonIcon);
					settingTypes.Add(TimeTerminalSettingType.LimitSelectableAdditions);
					settingTypes.Add(TimeTerminalSettingType.SelectedAdditions);
					settingTypes.Add(TimeTerminalSettingType.AccountDim2);
					settingTypes.Add(TimeTerminalSettingType.LimitSelectableAccounts2);
					settingTypes.Add(TimeTerminalSettingType.SelectedAccounts2);
					settingTypes.Add(TimeTerminalSettingType.AccountDimsInHierarchy);
					settingTypes.Add(TimeTerminalSettingType.SelectAccountsWithoutImmediateStamp);
					settingTypes.Add(TimeTerminalSettingType.RememberAccountsAfterBreak);
					settingTypes.Add(TimeTerminalSettingType.LogLevel);
					settingTypes.Add(TimeTerminalSettingType.LimitTimeTerminalToAccountDim);
					break;
			}

			return settingTypes;
		}

		public List<StringKeyValue> GetTimeZones()
		{
			List<StringKeyValue> list = new List<StringKeyValue>();

			foreach (TimeZoneInfo z in TimeZoneInfo.GetSystemTimeZones())
			{
				list.Add(new StringKeyValue(z.Id, z.DisplayName));
			}

			return list;
		}

		public string GetTimeZoneForTerminal(int timeTerminalId)
		{
			string timeZoneId = GetTimeTerminalStringSetting(TimeTerminalSettingType.TimeZone, timeTerminalId);
			// Default to sweden
			if (string.IsNullOrEmpty(timeZoneId))
				timeZoneId = Constants.TIMEZONE_DEFAULT;

			return timeZoneId;
		}

		public DateTime GetLocalTimeForTerminal(DateTime utcTime, int timeTerminalId)
		{
			string timeZoneId = GetTimeZoneForTerminal(timeTerminalId);
			return GetLocalTimeFromUTC(utcTime, timeZoneId);
		}

		public DateTime GetLocalTimeFromUTC(DateTime utcTime, string timeZoneId)
		{
			if (!timeZoneId.IsNullOrEmpty())
			{
				TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
				if (timeZone != null)
					return TimeZoneInfo.ConvertTimeFromUtc(utcTime, timeZone);
			}

			return utcTime;
		}

		public double GetTimeZoneOffsetFromDefault(int timeTerminalId)
		{
			string timeZoneId = GetTimeZoneForTerminal(timeTerminalId);
			if (!timeZoneId.IsNullOrEmpty())
			{
				if (timeZoneId == Constants.TIMEZONE_DEFAULT)
					return 0;

				try
				{
					TimeZoneInfo defaultTimeZone = TimeZoneInfo.FindSystemTimeZoneById(Constants.TIMEZONE_DEFAULT);
					TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
					if (defaultTimeZone != null && timeZone != null)
						return (timeZone.BaseUtcOffset.TotalHours - defaultTimeZone.BaseUtcOffset.TotalHours);
				}
				catch (Exception ex)
				{
					base.LogError(ex, this.log);
				}
			}

			return 0;
		}

		public List<string> GetTerminalGroupNames(int actorCompanyId)
		{
			using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
			entities.TimeTerminalSetting.NoTracking();
			return (from s in entities.TimeTerminalSetting
					where s.TimeTerminal.ActorCompanyId == actorCompanyId &&
					s.TimeTerminal.State == (int)SoeEntityState.Active &&
					s.Type == (int)TimeTerminalSettingType.TerminalGroupName &&
					s.StrData.Length > 0 &&
					s.State == (int)SoeEntityState.Active
					orderby s.StrData
					select s.StrData).Distinct().ToList();
		}

		public bool HasAnyTerminalSpecifiedBoolSetting(int actorCompanyId, TimeTerminalSettingType settingType)
		{
			using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
			entities.TimeTerminalSetting.NoTracking();
			return (from s in entities.TimeTerminalSetting
					where s.TimeTerminal.ActorCompanyId == actorCompanyId &&
					s.TimeTerminal.State == (int)SoeEntityState.Active &&
					s.Type == (int)settingType &&
					s.BoolData == true &&
					s.State == (int)SoeEntityState.Active
					select s).Any();
		}

		public bool HasAnyTerminalSpecifiedIntSetting(int actorCompanyId, TimeTerminalSettingType settingType, bool allowZero)
		{
			using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
			entities.TimeTerminalSetting.NoTracking();
			return (from s in entities.TimeTerminalSetting
					where s.TimeTerminal.ActorCompanyId == actorCompanyId &&
					s.TimeTerminal.State == (int)SoeEntityState.Active &&
					s.Type == (int)settingType &&
					s.IntData.HasValue &&
					(allowZero || s.IntData.Value > 0) &&
					s.State == (int)SoeEntityState.Active
					select s).Any();
		}

		public int GetAnyTerminalSpecifiedIntSetting(int actorCompanyId, TimeTerminalSettingType settingType)
		{
			using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
			entities.TimeTerminalSetting.NoTracking();
			var setting = (from s in entities.TimeTerminalSetting
						   where s.TimeTerminal.ActorCompanyId == actorCompanyId &&
						   s.TimeTerminal.State == (int)SoeEntityState.Active &&
						   s.Type == (int)settingType &&
						   s.IntData.HasValue &&
						   s.IntData.Value > 0 &&
						   s.State == (int)SoeEntityState.Active
						   select s).FirstOrDefault();

			return setting?.IntData ?? 0;
		}

		public bool IsTimeStampStatusValid(EmployeeGroup employeeGroup, int stampingStatus, AttestStateDTO attestStateTo)
		{
			if (employeeGroup == null)
				return false;
			return IsTimeStampStatusValid(employeeGroup.AutogenTimeblocks, stampingStatus, attestStateTo);
		}

		public bool IsTimeStampStatusValid(bool autogenTimeblocks, int stampingStatus, AttestStateDTO attestStateTo)
		{
			if (autogenTimeblocks)
				return true;
			if (attestStateTo == null || attestStateTo.Initial)
				return true;
			return stampingStatus == (int)TermGroup_TimeBlockDateStampingStatus.NoStamps || stampingStatus == (int)TermGroup_TimeBlockDateStampingStatus.Complete;
		}

		#endregion

		#region TimeStampEntry

		public List<TimeStampEntryDTO> CreateTimeStampsAccourdingToSchedule(int timeScheduleTemplatePeriodId, DateTime date, int employeeId, int employeeGroupId, int actorCompanyId, int userId)
		{
			List<TimeStampEntryDTO> timeStampEntrys = new List<TimeStampEntryDTO>();

			TimeDeviationCause standardDeviationCause = TimeDeviationCauseManager.GetTimeDeviationCauseFromPrio(employeeId, employeeGroupId, actorCompanyId);
			if (standardDeviationCause == null)
				return timeStampEntrys;

			TimeScheduleTemplatePeriod timeScheduleTemplatePeriod = TimeEngineManager(actorCompanyId, userId).GetSequentialSchedule(date, timeScheduleTemplatePeriodId, employeeId);
			List<TimeScheduleTemplateBlock> scheduleBlocks = timeScheduleTemplatePeriod.TimeScheduleTemplateBlock.Where(b => !b.IsBreak && b.ActualStartTime.HasValue && b.ActualStopTime.HasValue).ToList();
			if (scheduleBlocks.IsNullOrEmpty())
				return timeStampEntrys;

			using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
			List<ShiftType> shiftTypes = GetShiftTypesFromCache(entitiesReadOnly, CacheConfig.Company(actorCompanyId));
			List<TimeScheduleType> scheduleTypes = GetTimeScheduleTypesFromCache(entitiesReadOnly, CacheConfig.Company(actorCompanyId));
			List<GenericType> typeNames = base.GetTermGroupContent(TermGroup.TimeStampEntryType);

			List<int> accountIds = new List<int>();
			using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
			bool useAccountHierarchy = base.UseAccountHierarchyOnCompanyFromCache(entities, actorCompanyId);
			if (useAccountHierarchy)
				accountIds = AccountManager.GetSelectableEmployeeShiftAccounts(entitiesReadOnly, userId, actorCompanyId, employeeId, date, includeAbstract: true).Select(a => a.AccountId).ToList();

			List<Account> cachedAccounts = new List<Account>();

			foreach (TimeScheduleTemplateBlock scheduleBlock in scheduleBlocks)
			{
				if (!ValidateBlock(scheduleBlock))
					continue;

                string scheduleTypeName = scheduleBlock.TimeScheduleTypeId.HasValue ? scheduleTypes.FirstOrDefault(s => s.TimeScheduleTypeId == scheduleBlock.TimeScheduleTypeId.Value)?.Name : null;
                
                // As of 2026-01-21 we do not set account IDs on timestamp entries when using the create from schedule function.
                // The reason it that if the account on schedule is changed later we can't get the correct account on the timestamp entry by recalculating,
                // it will be stuck with the ID it already has, since that is how it works for stamps created from the terminal when you select an account there.

                //int? shiftTypeAccountId = scheduleBlock.ShiftTypeId.HasValue ? shiftTypes.FirstOrDefault(s => s.ShiftTypeId == scheduleBlock.ShiftTypeId.Value)?.AccountId : null;
                //int? timeTerminalAccountId = useAccountHierarchy ? scheduleBlock.AccountId : null;

                TimeStampEntryDTO stampIn = new TimeStampEntryDTO()
                {
                    Date = scheduleBlock.ActualStartTime.Value.Date,
                    Time = scheduleBlock.ActualStartTime.Value,
                    Type = TimeStampEntryType.In,
                    TypeName = typeNames.FirstOrDefault(e => e.Id == (int)TimeStampEntryType.In)?.Name,
                    TimeDeviationCauseId = standardDeviationCause.TimeDeviationCauseId,
                    TimeDeviationCauseName = standardDeviationCause.Name,
                    TimeScheduleTypeId = scheduleBlock.TimeScheduleTypeId,
                    TimeScheduleTypeName = scheduleTypeName,
                    //AccountId = scheduleBlock.AccountId.HasValidValue() ? scheduleBlock.AccountId.Value : shiftTypeAccountId,
                    //TimeTerminalAccountId = timeTerminalAccountId,
                    ManuallyAdjusted = true,
                };
                timeStampEntrys.Add(stampIn);

                TimeStampEntryDTO stampOut = new TimeStampEntryDTO()
                {
                    Date = scheduleBlock.ActualStopTime.Value.Date,
                    Time = scheduleBlock.ActualStopTime.Value,
                    Type = TimeStampEntryType.Out,
                    TypeName = typeNames.FirstOrDefault(e => e.Id == (int)TimeStampEntryType.Out)?.Name,
                    TimeDeviationCauseId = standardDeviationCause.TimeDeviationCauseId,
                    TimeDeviationCauseName = standardDeviationCause.Name,
                    TimeScheduleTypeId = scheduleBlock.TimeScheduleTypeId,
                    TimeScheduleTypeName = scheduleTypeName,
                    //AccountId = scheduleBlock.AccountId.HasValidValue() ? scheduleBlock.AccountId.Value : shiftTypeAccountId,
                    //TimeTerminalAccountId = timeTerminalAccountId,
                    ManuallyAdjusted = true,
                };
                timeStampEntrys.Add(stampOut);
            }

			bool ValidateBlock(TimeScheduleTemplateBlock scheduleBlock)
			{
				bool isValid = true;
				if (useAccountHierarchy && scheduleBlock.AccountId.HasValue && !accountIds.Contains(scheduleBlock.AccountId.Value))
				{
					isValid = false;
					int lowestAccountId = accountIds.Last();
					Account account = GetAccount(scheduleBlock.AccountId.Value);
					while (account?.ParentAccountId != null && !isValid)
					{
						if (account.ParentAccountId.Value == lowestAccountId)
							isValid = true;
						else
							account = GetAccount(account.ParentAccountId.Value);
					}
				}
				return isValid;
			}
			Account GetAccount(int accountId)
			{
				Account account = cachedAccounts.FirstOrDefault(a => a.AccountId == accountId);
				if (account == null)
				{
					account = AccountManager.GetAccount(base.ActorCompanyId, accountId);
					if (account != null)
						cachedAccounts.Add(account);
				}
				return account;
			}

			return timeStampEntrys.OrderBy(t => t.Time).ToList();
		}

		public List<TimeStampEntryDTO> GetTimeStampEntriesDTO(DateTime dateFrom, DateTime dateTo, List<int> employeeIds, int actorCompanyId, bool onlyActive = true)
		{
			List<TimeStampEntryDTO> dtos = new List<TimeStampEntryDTO>();
			if (employeeIds.IsNullOrEmpty())
				return dtos;

			List<Employee> employees = EmployeeManager.GetAllEmployeesByIds(actorCompanyId, employeeIds, active: true);
			if (employees.IsNullOrEmpty())
				return dtos;

			List<GenericType> typeNames = base.GetTermGroupContent(TermGroup.TimeStampEntryType);
			foreach (int employeeId in employeeIds)
			{
				Employee employee = employees.FirstOrDefault(e => e.EmployeeId == employeeId);
				if (employee == null)
					continue;

				DateTime currentDate = dateFrom;
				while (currentDate <= dateTo)
				{
					TimeBlockDate timeBlockDate = TimeBlockManager.GetTimeBlockDate(actorCompanyId, employeeId, currentDate);
					if (timeBlockDate != null)
					{
						List<TimeStampEntry> timeStampEntries = GetTimeStampEntriesForRecalculation(timeBlockDate.TimeBlockDateId, onlyActive);
						foreach (TimeStampEntry entry in timeStampEntries)
						{
							TimeStampEntryDTO dto = entry.ToDTO();
							dto.EmployeeId = employeeId;
							dto.EmployeeNr = employee.EmployeeNr;
							dto.EmployeeName = employee.Name;
							dto.TypeName = typeNames?.FirstOrDefault(e => e.Id == entry.Type)?.Name ?? string.Empty;
							dtos.Add(dto);
						}
					}
					currentDate = currentDate.AddDays(1);
				}
			}

			return dtos.OrderBy(t => t.EmployeeNr).ThenBy(t => t.Time).ToList();
		}

		public List<TimeStampEntryDTO> GetTimeStampEntriesDTO(DateTime dateFrom, DateTime dateTo, List<Employee> employees, int actorCompanyId, bool onlyActive = true)
		{
			List<TimeStampEntryDTO> dtos = new List<TimeStampEntryDTO>();
			if (employees.IsNullOrEmpty())
				return dtos;

			using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
			var timeBlockDates = TimeBlockManager.GetTimeBlockDates(entitiesReadOnly, actorCompanyId, employees.Select(e => e.EmployeeId).ToList(), dateFrom, dateTo);

			if (timeBlockDates.IsNullOrEmpty())
				return dtos;

			//Fetch all TimeStampEntries for the specified TimeBlockDates in batches of 1000 ids at a time
			List<int> timeBlockDateIds = timeBlockDates.Select(t => t.TimeBlockDateId).ToList();

			List<TimeStampEntry> timeStampEntries = new List<TimeStampEntry>();
			for (int i = 0; i < timeBlockDateIds.Count; i += 2000)
			{
				List<int> batch = timeBlockDateIds.Skip(i).Take(2000).ToList();
				timeStampEntries.AddRange(GetTimeStampEntries(entitiesReadOnly, batch, loadExtended: true));
			}

			var timeStampeEntriesDict = timeStampEntries.GroupBy(t => t.EmployeeId).ToDictionary(t => t.Key, t => t.ToList());
			var timeBlockDatesDict = timeBlockDates.ToDictionary(t => t.TimeBlockDateId, t => t);

			List<GenericType> typeNames = base.GetTermGroupContent(TermGroup.TimeStampEntryType);
			foreach (Employee employee in employees)
			{
				if (timeStampeEntriesDict.TryGetValue(employee.EmployeeId, out timeStampEntries))
				{
					foreach (TimeStampEntry entry in timeStampEntries)
					{
						TimeStampEntryDTO dto = entry.ToDTO();
						dto.EmployeeId = employee.EmployeeId;
						dto.EmployeeNr = employee.EmployeeNr;
						dto.EmployeeName = employee.Name;
						dto.TypeName = typeNames?.FirstOrDefault(e => e.Id == entry.Type)?.Name ?? string.Empty;

						if (entry.TimeBlockDateId.HasValue && timeBlockDatesDict.TryGetValue(entry.TimeBlockDateId.Value, out TimeBlockDate timeBlockDate))
							dto.DateFromTimeBlockDate = timeBlockDate.Date;

						dtos.Add(dto);
					}
				}
			}

			return dtos;
		}

		public List<TimeStampEntry> GetTimeStampsForCompany(DateTime timeFrom, DateTime timeTo, DateTime? lastChanged, int actorCompanyId)
		{
			using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
			return GetTimeStampsForCompany(entities, timeFrom, timeTo, lastChanged, actorCompanyId);
		}

		public List<TimeStampEntry> GetTimeStampsForCompany(CompEntities entities, DateTime fromTime, DateTime toTime, DateTime? lastChanged, int actorCompanyId)
		{
			return (from t in entities.TimeStampEntry
						.Include("TimeDeviationCause")
						.Include("Account")
						.Include("TimeBlockDate")
						.Include("TimeTerminal")
						.Include("Employee")
					where t.ActorCompanyId == actorCompanyId &&
					t.State == (int)SoeEntityState.Active &&
					t.Time > fromTime &&
					t.Time < toTime
					select t).ToList();
		}

		public List<TimeStampEntry> GetTimeStampEntries(DateTime dateFrom, DateTime dateTo, int employeeId, bool loadTimeBlock = false, bool loadTimeBlockDate = false, bool loadTimeDeviationCause = false)
		{
			using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
			entities.TimeStampEntry.NoTracking();
			return GetTimeStampEntries(entities, dateFrom, dateTo, employeeId, loadTimeBlock, loadTimeBlockDate, loadTimeDeviationCause);
		}

		public List<TimeStampEntry> GetTimeStampEntries(CompEntities entities, DateTime dateFrom, DateTime dateTo, int employeeId, bool loadTimeBlock = false, bool loadTimeBlockDate = false, bool loadTimeDeviationCause = false)
		{
			IQueryable<TimeStampEntry> query = entities.TimeStampEntry;

			if (loadTimeBlock)
				query = query.Include("TimeBlock");
			if (loadTimeBlockDate)
				query = query.Include("TimeBlockDate");
			if (loadTimeDeviationCause)
				query = query.Include("TimeDeviationCause");

			return (from tse in query
					where tse.EmployeeId == employeeId &&
					(
					(tse.TimeBlockDate.Date >= dateFrom && tse.TimeBlockDate.Date <= dateTo) ||
					(tse.Time >= dateFrom && tse.Time <= dateTo)
					)
					//&& tse.State == (int)SoeEntityState.Active Including everything because of personalliggaren
					select tse).ToList();
		}

		public List<TimeStampEntry> GetTimeStampEntries(CompEntities entities, List<int> timeBlockDateIds, bool loadExtended = false)
		{
			// Get existing time stamp entries for specified TimeBlockDate
			using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
			entitiesReadOnly.TimeStampEntry.NoTracking();
			IQueryable<TimeStampEntry> query = entitiesReadOnly.TimeStampEntry;

			if (loadExtended)
				query = query.Include("TimeStampEntryExtended");

			return (from t in query
					where t.TimeBlockDateId.HasValue &&
					timeBlockDateIds.Contains(t.TimeBlockDateId.Value) &&
					t.State == (int)SoeEntityState.Active
					select t).ToList();
		}

		public List<TimeStampEntry> GetLastTimeStampEntriesForEmployee(int employeeId, int nbrOfEntries, bool loadExtended)
		{
			using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
			entitiesReadOnly.TimeStampEntry.NoTracking();
			IQueryable<TimeStampEntry> query = entitiesReadOnly.TimeStampEntry;

			// If using setting RememberAccountsAfterBreak, two last stamps are fetched to make sure one of them is in
			bool onlyOne = (nbrOfEntries == 1 || nbrOfEntries == 2);

			if (loadExtended)
				query = query.Include("TimeStampEntryExtended");

			if (!onlyOne)
			{
				query = query.Include("TimeDeviationCause");
				query = query.Include("Account");
				query = query.Include("TimeTerminal");

				if (loadExtended)
				{
					query = query.Include("TimeStampEntryExtended.TimeScheduleType");
					query = query.Include("TimeStampEntryExtended.TimeCode");
					query = query.Include("TimeStampEntryExtended.Account");
				}
			}

			DateTime maxTime = DateTime.Today.AddDays(2);

			List<TimeStampEntry> entries = (from t in query
											where t.EmployeeId == employeeId &&
											t.State == (int)SoeEntityState.Active &&
											t.Time < maxTime
											orderby t.Time descending, t.TimeStampEntryId descending
											select t).Take(nbrOfEntries).ToList();

			// If only returning the latest to decide if in or out, only check three days back
			if (onlyOne && entries.Any() && entries[0].Time < DateTime.Today.AddDays(-3))
				return new List<TimeStampEntry>();

			if (!onlyOne)
			{
				foreach (TimeStampEntry entry in entries)
				{
					if (entry.TimeDeviationCause != null)
						entry.DeviationCauseName = entry.TimeDeviationCause.Name;
					if (entry.Account != null)
						entry.AccountName = entry.Account.Name;
					if (entry.TimeTerminal != null)
						entry.TerminalName = entry.TimeTerminal.Name;
					else
						entry.TerminalName = "Web";
				}
			}

			return entries;
		}

		public List<TimeStampEntry> GetLastTimeStampEntryForEachEmployee(int timeTerminalId, TimeStampEntryType? type)
		{
			using var entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
			entitiesReadOnly.TimeStampEntry.NoTracking();
			List<int> entryIds = (from t in entitiesReadOnly.TimeStampEntry
								  where t.TimeTerminalId == timeTerminalId &&
								  t.State == (int)SoeEntityState.Active &&
								  t.Status != (int)TermGroup_TimeStampEntryStatus.ProcessedWithNoResult
								  group t by t.EmployeeId into s
								  select s.OrderByDescending(i => i.Time).ThenByDescending(i => i.TimeStampEntryId).FirstOrDefault().TimeStampEntryId).ToList();

			// Can not do includes in query above, therefore we just select the ids above and then make a new query with full entries below
			var query = (from t in entitiesReadOnly.TimeStampEntry
							.Include("Company")
							.Include("Employee.User")
							.Include("Employee.ContactPerson")
						 where entryIds.Contains(t.TimeStampEntryId)
						 select t);

			if (type.HasValue)
				query = query.Where(t => t.Type == (int)type.Value);

			return query.ToList();
		}

		public List<TimeStampEntry> GetTimeStampEntriesForRecalculation(int timeBlockDateId, bool onlyActive = true)
		{
			using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
			entities.TimeStampEntry.NoTracking();
			return GetTimeStampEntriesForRecalculation(entities, timeBlockDateId, onlyActive);
		}

		public List<TimeStampEntry> GetTimeStampEntriesForRecalculation(CompEntities entities, int timeBlockDateId, bool onlyActive = true)
		{
			var query = (from t in entities.TimeStampEntry
							.Include("TimeStampEntryExtended")
							.Include("TimeDeviationCause")
							.Include("Account")
							.Include("TimeBlockDate")
							.Include("TimeTerminal")
						 where t.TimeBlockDateId == timeBlockDateId
						 select t);

			if (onlyActive)
				query = query.Where(a => a.State == (int)SoeEntityState.Active);

			return query.ToList();
		}

		public List<TimeStampEntry> GetTimeStampEntriesForRecalculation(GetDataInBatchesModel model)
		{
			return (from t in model.Entities.TimeStampEntry
						.Include("TimeStampEntryExtended")
						.Include("TimeDeviationCause")
						.Include("Account")
						.Include("TimeBlockDate")
						.Include("TimeTerminal")
					where t.TimeBlockDateId.HasValue &&
					model.BatchIds.Contains(t.TimeBlockDateId.Value) &&
					t.State == (int)SoeEntityState.Active
					select t).ToList();
		}

		public TimeStampEntry GetLastTimeStampEntryForEmployee(int employeeId, bool excludeFuture = false, int? actorCompanyId = null)
		{
			using var entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
			entitiesReadOnly.TimeStampEntry.NoTracking();

			var query = from t in entitiesReadOnly.TimeStampEntry
						where t.EmployeeId == employeeId &&
						t.State == (int)SoeEntityState.Active
						select t;

			if (excludeFuture)
			{
				var now = DateTime.Now;

				// TODO: Should check terminal setting

				// Taxi Kurir, Different Time Zone
				if (actorCompanyId.HasValue && actorCompanyId.Value == 594154)
					now = now.AddHours(2);

				query = query.Where(e => e.Time < now);
			}

			return query.OrderByDescending(t => t.Time).ThenByDescending(t => t.TimeStampEntryId).FirstOrDefault();
		}


		public List<EmployeeDatesDTO> GetTimeStampEntryDatesByEmployee(GetDataInBatchesModel model)
		{
			if (!model.IsValid(requireDates: true))
				return new List<EmployeeDatesDTO>();

			var query = from tbd in model.Entities.TimeBlockDate
						where model.BatchIds.Contains(tbd.EmployeeId) &&
						tbd.ActorCompanyId == model.ActorCompanyId
						select tbd;

			if (model.StartDate.Value == model.StopDate.Value)
				query = query.Where(tbd => tbd.Date == model.StartDate.Value);
			else
				query = query.Where(tbd => model.StartDate.Value <= tbd.Date && tbd.Date <= model.StopDate.Value);

			var timeStampEntries = (from tbd in query
									where model.Entities.TimeStampEntry.Any(tse =>
									tse.TimeBlockDateId == tbd.TimeBlockDateId &&
									tse.ActorCompanyId == model.ActorCompanyId &&
									tse.State == (int)SoeEntityState.Active)
									select new
									{
										tbd.EmployeeId,
										tbd.Date,
									}).ToList();

			return timeStampEntries
				.GroupBy(tse => tse.EmployeeId)
				.ToDictionary(k => k.Key, v => v.Select(tse => tse.Date).ToList())
				.ToEmployeeDates();
		}

		public Dictionary<DateTime, DateTime> GetFirstStampIn(CompEntities entities, int employeeId, DateTime startDate, DateTime stopDate)
		{
			var results = (from t in entities.TimeStampEntry
						   where t.EmployeeId == employeeId &&
						   (t.TimeBlockDate.Date >= startDate && t.TimeBlockDate.Date <= stopDate) &&
						   t.Type == (int)TimeStampEntryType.In &&
						   t.State == (int)SoeEntityState.Active
						   group t by t.TimeBlockDate.Date into g
						   select new { Date = g.Key, Time = g.OrderBy(g2 => g2.Time).Select(g2 => g2.Time).FirstOrDefault() });

			Dictionary<DateTime, DateTime> dict = new Dictionary<DateTime, DateTime>();
			foreach (var result in results)
			{
				dict.Add(result.Date, result.Time);
			}

			return dict;
		}

		public Dictionary<DateTime, DateTime> GetLastStampOut(CompEntities entities, int employeeId, DateTime startDate, DateTime stopDate)
		{
			var results = (from t in entities.TimeStampEntry
						   where t.EmployeeId == employeeId &&
						   (t.TimeBlockDate.Date >= startDate && t.TimeBlockDate.Date <= stopDate) &&
						   t.Type == (int)TimeStampEntryType.Out &&
						   t.State == (int)SoeEntityState.Active
						   group t by t.TimeBlockDate.Date into g
						   select new { Date = g.Key, Time = g.OrderByDescending(g2 => g2.Time).Select(g2 => g2.Time).FirstOrDefault() });

			Dictionary<DateTime, DateTime> dict = new Dictionary<DateTime, DateTime>();
			foreach (var result in results)
			{
				dict.Add(result.Date, result.Time);
			}

			return dict;
		}

		public UserAgentClientInfoDTO GetTimeStampEntryUserAgentClientInfo(int actorCompanyId, int timeStampEntryId)
		{
			using var entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
			string data = (from t in entitiesReadOnly.TimeStampEntry
						   where t.ActorCompanyId == actorCompanyId &&
						   t.TimeStampEntryId == timeStampEntryId
						   select t.TerminalStampData).FirstOrDefault();

			return UserAgentUtility.ParseToDTO(data);
		}

		public int GetNumberOfTimeStampEntries(int timeBlockDateId)
		{
			// Get existing time stamp entries for specified TimeBlockDate
			using var entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
			return (from t in entitiesReadOnly.TimeStampEntry
					where t.TimeBlockDateId == timeBlockDateId &&
					t.State == (int)SoeEntityState.Active
					select t).Count();
		}

		public bool TimeStampEntriesExists(int actorCompanyId)
		{
			using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
			entities.TimeStampEntry.NoTracking();
			return (from t in entities.TimeStampEntry
					where t.ActorCompanyId == actorCompanyId &&
						  t.State == (int)SoeEntityState.Active
					select t).Any();
		}

		public Tuple<int?, int?> TryGetShiftTypeAndTimeScheduleType(CompEntities entities, int actorCompanyId, int accountId)
		{
			string key = $"TryGetShiftTypeAndTimeScheduleTypeAccountId{accountId}Actorcompanyid{actorCompanyId}";

			Tuple<int?, int?> tuple = BusinessMemoryCache<Tuple<int?, int?>>.Get(key);

			if (tuple != null)
				return tuple;

			AccountDimDTO dim = base.GetShiftTypeAccountDimFromCache(entities, actorCompanyId);
			if (dim != null)
			{
				AccountDTO account = AccountManager.GetAccountDTO(entities, actorCompanyId, accountId, true);
				if (account != null && account.AccountDimId == dim.AccountDimId)
					tuple = TimeScheduleManager.GetShiftTypeTupleFromLinkedAccountId(entities, accountId, actorCompanyId);
			}

			if (tuple == null)
				tuple = Tuple.Create((int?)null, (int?)null);

			BusinessMemoryCache<Tuple<int?, int?>>.Set(key, tuple, 60 * 60);

			return tuple;
		}

		public ActionResult SaveTimeStampEntries(List<AttestEmployeeDayTimeStampDTO> inputDTOs, DateTime inputDate, int employeeId, int actorCompanyId, bool? discardBreakEvaluation = null)
		{
			List<TimeStampEntryDTO> entryDTOs = new List<TimeStampEntryDTO>();

			foreach (var input in inputDTOs)
			{
				if (input.Extended.IsNullOrEmpty() && input.AccountId2.HasValidValue())
				{
					if (input.Extended == null)
						input.Extended = new List<TimeStampEntryExtendedDTO>();

					var addExtended = new TimeStampEntryExtendedDTO()
					{
						TimeStampEntryId = input.TimeStampEntryId,
						AccountId = input.AccountId2,
					};

					input.Extended.Add(addExtended);
				}

				entryDTOs.Add(new TimeStampEntryDTO()
				{
					TimeStampEntryId = input.TimeStampEntryId,
					TimeTerminalId = input.TimeTerminalId,
					EmployeeId = input.EmployeeId,
					TimeDeviationCauseId = input.TimeDeviationCauseId,
					AccountId = input.AccountId,
					TimeTerminalAccountId = input.TimeTerminalAccountId,
					TimeScheduleTemplatePeriodId = input.TimeScheduleTemplatePeriodId,
					TimeBlockDateId = input.TimeBlockDateId,
					EmployeeChildId = input.EmployeeChildId,
					ShiftTypeId = input.ShiftTypeId,
					TimeScheduleTypeId = input.TimeScheduleTypeId,
					Type = input.Type,
					OriginType = input.OriginType,
					Note = input.Note,
					Date = input.Time,
					Time = input.Time,
					ManuallyAdjusted = input.ManuallyAdjusted,
					EmployeeManuallyAdjusted = input.EmployeeManuallyAdjusted,
					Status = input.Status,
					IsBreak = input.IsBreak,
					IsPaidBreak = input.IsPaidBreak,
					IsDistanceWork = input.IsDistanceWork,
					Extended = input.Extended,
				});
			}

			return SaveTimeStampEntries(entryDTOs, inputDate, employeeId, actorCompanyId, discardBreakEvaluation);
		}

		public ActionResult SaveTimeStampEntries(List<TimeStampEntryDTO> inputDTOs, DateTime inputDate, int employeeId, int actorCompanyId, bool? discardBreakEvaluation = null)
		{
			if (inputDTOs == null)
				return new ActionResult((int)ActionResultSave.EntityIsNull, "TimeStampEntryDTO");

			// Default result is successful
			ActionResult result = new ActionResult();

			List<int> hasInsert = new List<int>();
			List<int> hasUpdate = new List<int>();
			List<int> hasDelete = new List<int>();
			List<int> expenseRowIdsToDelete = new List<int>();

			List<TrackChangesDTO> trackChangesItems = new List<TrackChangesDTO>();
			TermGroup_TrackChangesActionMethod actionMethod = TermGroup_TrackChangesActionMethod.TimeStampEntry_Save_AttestTime;
			using var entitiesReadonly = CompEntitiesProvider.LeaseReadOnlyContext();
			var hasEventActivated = base.HasEventActivatedScheduledJob(entitiesReadonly, actorCompanyId, TermGroup_ScheduleJobEventActivationType.TimeStampCreated);
			List<Employee> employees = new List<Employee>();
			List<Account> accounts = new List<Account>();
			List<ScheduledJobSetting> eventJobsSettings = null;
			using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();

			if (hasEventActivated)
				eventJobsSettings = ScheduledJobManager.GetScheduledJobSettingsWithEventActivaction(entitiesReadOnly, actorCompanyId).Where(w => w.IntData.HasValue && w.IntData == (int)TermGroup_ScheduleJobEventActivationType.TimeStampCreated).ToList();

			if (eventJobsSettings.IsNullOrEmpty())
				hasEventActivated = false;

			if (hasEventActivated)
			{
				List<int> employeeIds = inputDTOs.Select(s => s.EmployeeId).Distinct().ToList();
				employees = entitiesReadOnly.Employee.Where(w => w.ActorCompanyId == actorCompanyId && employeeIds.Contains(w.EmployeeId)).ToList();
				List<int> accountIds = inputDTOs.Where(w => w.AccountId.HasValue).Select(s => s.AccountId.Value).Distinct().ToList();
				accounts = entitiesReadOnly.Account.Where(w => w.ActorCompanyId == actorCompanyId && accountIds.Contains(w.AccountId)).ToList();
			}

			using (CompEntities entities = new CompEntities())
			{
				List<TimeStampEntry> allTimeStampEntries = new List<TimeStampEntry>();
				List<TimeStampEntry> pubSubEntries = new List<TimeStampEntry>();
				List<TimeStampEntry> eventEntries = new List<TimeStampEntry>();

				bool? useTimeScheduleTypeFromTimeStampEntry = (bool?)null;
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

						bool isEmployeeCurrentUser = EmployeeManager.IsEmployeeCurrentUser(entities, employeeId, parameterObject.SoeUser != null ? base.UserId : 0);
						if (isEmployeeCurrentUser)
							actionMethod = TermGroup_TrackChangesActionMethod.TimeStampEntry_Save_MyTime;

						#endregion

						#region Dates

						// Get TimeBlockDate
						TimeBlockDateDTO timeBlockDate = TimeBlockManager.GetTimeBlockDateFromCache(entities, actorCompanyId, employeeId, inputDate, true);

						// Set date on DTOs from input date
						foreach (var inputDTO in inputDTOs)
						{
							inputDTO.Time = CalendarUtility.MergeDateAndTime(inputDTO.Date, inputDTO.Time);
						}

						#endregion

						#region TimeStampEntry

						// Get all existing active entries for current date. Load all needed data once
						List<TimeStampEntry> employeeEntries = GetTimeStampEntriesForRecalculation(entities, timeBlockDate.TimeBlockDateId).ToList();
						allTimeStampEntries.AddRange(employeeEntries);

						if (inputDTOs.Count > 0)
						{
							#region Add/Update

							foreach (TimeStampEntryDTO entryDTO in inputDTOs)
							{
								#region TimeStampEntry

								TimeStampEntry entry = null;
								DateTime time = new DateTime(entryDTO.Date.Year, entryDTO.Date.Month, entryDTO.Date.Day, entryDTO.Time.Hour, entryDTO.Time.Minute, entryDTO.Time.Second);
								bool timeValidForPubSub = (time > DateTime.Now.AddDays(-1) && time < DateTime.Now.AddDays(1));
								bool isModified = false;
								bool isNew = false;
								int? employeeChildId = entryDTO.EmployeeChildId;

								// Try get existing entry
								if (entryDTO.TimeStampEntryId != 0)
									entry = allTimeStampEntries.FirstOrDefault(i => i.TimeStampEntryId == entryDTO.TimeStampEntryId);

								if (entry == null)
								{
									#region Add

									#region TimeStampEntry

									isNew = true;

									entry = new TimeStampEntry()
									{
										ManuallyAdjusted = true,
										Status = (int)TermGroup_TimeStampEntryStatus.New,
										OriginType = (int)entryDTO.OriginType,
										OriginalTime = time,

										//Set FK
										EmployeeId = employeeId,
										EmployeeChildId = employeeChildId,
										IsBreak = entryDTO.IsBreak,
										IsPaidBreak = entryDTO.IsPaidBreak,
										IsDistanceWork = entryDTO.IsDistanceWork,

										//Set references
										TimeBlockDateId = timeBlockDate.TimeBlockDateId,
										Company = company,
									};
									SetCreatedProperties(entry);

									allTimeStampEntries.Add(entry);

									if (!hasInsert.Contains(employeeId))
										hasInsert.Add(employeeId);

									if (hasEventActivated)
										eventEntries.Add(entry);

									#endregion

									#region TimeStampEntryExtended

									if (!entryDTO.Extended.IsNullOrEmpty())
									{
										foreach (TimeStampEntryExtendedDTO extendedDTO in entryDTO.Extended)
										{
											if (extendedDTO.TimeScheduleTypeId.HasValue || extendedDTO.TimeCodeId.HasValue || extendedDTO.AccountId.HasValue)
											{
												var extended = extendedDTO.TimeStampEntryExtendedId != 0 ? entities.TimeStampEntryExtended.FirstOrDefault(f => f.TimeStampEntryExtendedId == extendedDTO.TimeStampEntryExtendedId) ?? new TimeStampEntryExtended() : new TimeStampEntryExtended();

												if (extended.TimeStampEntryExtendedId == 0)
												{
													extended = new TimeStampEntryExtended()
													{
														TimeStampEntry = entry,
														TimeScheduleTypeId = extendedDTO.TimeScheduleTypeId,
														TimeCodeId = extendedDTO.TimeCodeId,
														AccountId = extendedDTO.AccountId,
														Quantity = extendedDTO.Quantity,
														State = (int)extendedDTO.State,
													};
												}
												else
												{
													extended.TimeCodeId = extendedDTO.TimeCodeId;
													extended.State = (int)entryDTO.State;
													extended.Quantity = extendedDTO.Quantity;
													extended.TimeScheduleTypeId = extendedDTO.TimeScheduleTypeId;
													extended.AccountId = extendedDTO.AccountId;
												}
												SetCreatedProperties(extended);
											}
										}
									}

									#endregion

									#endregion
								}

								#region Update

								#region TimeStampEntry

								// Only set as modified if something really changed
								if (entry.TimeTerminalId != entryDTO.TimeTerminalId && (entry.TimeTerminalId.HasValue || entryDTO.TimeTerminalId != 0))
								{
									if (entryDTO.TimeTerminalId != 0)
										entry.TimeTerminalId = entryDTO.TimeTerminalId;
									else
										entry.TimeTerminal = null;

									isModified = true;
								}
								if (entry.TimeDeviationCauseId != entryDTO.TimeDeviationCauseId && (entry.TimeDeviationCauseId.HasValue || entryDTO.TimeDeviationCauseId != 0))
								{
									if (!isNew)
									{
										string fromValueName = !entry.TimeDeviationCauseId.IsNullOrEmpty() ? TimeDeviationCauseManager.GetTimeDeviationCause(entities, entry.TimeDeviationCauseId.Value, actorCompanyId)?.Name : null;
										string toValueName = !entryDTO.TimeDeviationCauseId.IsNullOrEmpty() ? TimeDeviationCauseManager.GetTimeDeviationCause(entities, entryDTO.TimeDeviationCauseId.Value, actorCompanyId)?.Name : null;
										trackChangesItems.Add(TrackChangesManager.InitTrackChanges(entities, actorCompanyId, actionMethod, TermGroup_TrackChangesAction.Update, SoeEntityType.Employee, entry.EmployeeId, SoeEntityType.TimeStampEntry, entry.TimeStampEntryId, SettingDataType.Integer, null, TermGroup_TrackChangesColumnType.TimeStampEntry_TimeDeviationCauseId, entry.TimeDeviationCauseId.ToString(), entryDTO.TimeDeviationCauseId.ToString(), fromValueName, toValueName));
									}

									if (entryDTO.TimeDeviationCauseId != 0)
										entry.TimeDeviationCauseId = entryDTO.TimeDeviationCauseId;

									isModified = true;
								}
								if (entry.AccountId != entryDTO.AccountId && (entry.AccountId.HasValue || entryDTO.AccountId != 0))
								{
									if (!isNew)
									{
										string fromValueName = !entry.AccountId.IsNullOrEmpty() ? AccountManager.GetAccount(entities, actorCompanyId, entry.AccountId.Value)?.Name : null;
										string toValueName = !entryDTO.AccountId.IsNullOrEmpty() ? AccountManager.GetAccount(entities, actorCompanyId, entryDTO.AccountId.Value)?.Name : null;
										trackChangesItems.Add(TrackChangesManager.InitTrackChanges(entities, actorCompanyId, actionMethod, TermGroup_TrackChangesAction.Update, SoeEntityType.Employee, entry.EmployeeId, SoeEntityType.TimeStampEntry, entry.TimeStampEntryId, SettingDataType.Integer, null, TermGroup_TrackChangesColumnType.TimeStampEntry_AccountId, entry.AccountId.ToString(), entryDTO.AccountId.ToString(), fromValueName, toValueName));
									}

									if (entryDTO.AccountId != 0)
										entry.AccountId = entryDTO.AccountId;
									else
										entry.Account = null;
									isModified = true;
								}
								if (entry.TimeTerminalAccountId != entryDTO.TimeTerminalAccountId && (entry.TimeTerminalAccountId.HasValue || entryDTO.TimeTerminalAccountId != 0))
								{
									if (!isNew)
									{
										string fromValueName = !entry.TimeTerminalAccountId.IsNullOrEmpty() ? AccountManager.GetAccount(entities, actorCompanyId, entry.TimeTerminalAccountId.Value)?.Name : null;
										string toValueName = !entryDTO.TimeTerminalAccountId.IsNullOrEmpty() ? AccountManager.GetAccount(entities, actorCompanyId, entryDTO.TimeTerminalAccountId.Value)?.Name : null;
										trackChangesItems.Add(TrackChangesManager.InitTrackChanges(entities, actorCompanyId, actionMethod, TermGroup_TrackChangesAction.Update, SoeEntityType.Employee, entry.EmployeeId, SoeEntityType.TimeStampEntry, entry.TimeStampEntryId, SettingDataType.Integer, null, TermGroup_TrackChangesColumnType.TimeStampEntry_TimeTerminalAccountId, entry.TimeTerminalAccountId.ToString(), entryDTO.TimeTerminalAccountId.ToString(), fromValueName, toValueName));
									}

									if (entryDTO.TimeTerminalAccountId != 0)
										entry.TimeTerminalAccountId = entryDTO.TimeTerminalAccountId;
									else
										entry.TimeTerminalAccountId = null;
									isModified = true;
								}
								if (entry.TimeScheduleTemplatePeriodId != entryDTO.TimeScheduleTemplatePeriodId && entry.TimeScheduleTemplatePeriodId.HasValidValue())
								{
									if (entryDTO.TimeScheduleTemplatePeriodId != 0)
										entry.TimeScheduleTemplatePeriodId = entryDTO.TimeScheduleTemplatePeriodId;
									else
										entry.TimeScheduleTemplatePeriod = null;
									isModified = true;
								}
								if (entry.Type != (int)entryDTO.Type)
								{
									if (!isNew)
										trackChangesItems.Add(TrackChangesManager.InitTrackChanges(entities, actorCompanyId, actionMethod, TermGroup_TrackChangesAction.Update, SoeEntityType.Employee, entry.EmployeeId, SoeEntityType.TimeStampEntry, entry.TimeStampEntryId, SettingDataType.Integer, null, TermGroup_TrackChangesColumnType.TimeStampEntry_Type, entry.Type.ToString(), ((int)entryDTO.Type).ToString()));

									entry.Type = (int)entryDTO.Type;
									isModified = true;
								}
								if (entry.Time != time)
								{
									if (!isNew)
									{
										bool dateHasChanged = entry.Time.Date != entryDTO.Time.Date;
										trackChangesItems.Add(TrackChangesManager.InitTrackChanges(entities, actorCompanyId, actionMethod, TermGroup_TrackChangesAction.Update, SoeEntityType.Employee, entry.EmployeeId, SoeEntityType.TimeStampEntry, entry.TimeStampEntryId, SettingDataType.Time, null, TermGroup_TrackChangesColumnType.TimeStampEntry_Time, dateHasChanged ? CalendarUtility.ToShortDateTimeString(entry.Time) : CalendarUtility.ToTime(entry.Time), dateHasChanged ? CalendarUtility.ToShortDateTimeString(entryDTO.Time) : CalendarUtility.ToTime(entryDTO.Time)));
									}

									entry.Time = time;
									isModified = true;
								}
								if (entry.Note != entryDTO.Note)
								{
									if (!isNew)
										trackChangesItems.Add(TrackChangesManager.InitTrackChanges(entities, actorCompanyId, actionMethod, TermGroup_TrackChangesAction.Update, SoeEntityType.Employee, entry.EmployeeId, SoeEntityType.TimeStampEntry, entry.TimeStampEntryId, SettingDataType.String, null, TermGroup_TrackChangesColumnType.TimeStampEntry_Note, entry.Note, entryDTO.Note));

									entry.Note = entryDTO.Note;
									isModified = true;
								}
								if (entry.EmployeeChildId != entryDTO.EmployeeChildId)
								{
									if (!isNew)
									{
										string fromValueName = !entry.EmployeeChildId.IsNullOrEmpty() ? EmployeeManager.GetEmployeeChild(entities, entry.EmployeeChildId.Value)?.Name : null;
										string toValueName = !entryDTO.EmployeeChildId.IsNullOrEmpty() ? EmployeeManager.GetEmployeeChild(entities, entryDTO.EmployeeChildId.Value)?.Name : null;
										trackChangesItems.Add(TrackChangesManager.InitTrackChanges(entities, actorCompanyId, actionMethod, TermGroup_TrackChangesAction.Update, SoeEntityType.Employee, entry.EmployeeId, SoeEntityType.TimeStampEntry, entry.TimeStampEntryId, SettingDataType.Integer, null, TermGroup_TrackChangesColumnType.TimeStampEntry_EmployeeChildId, entry.EmployeeChildId.ToString(), entryDTO.EmployeeChildId.ToString(), fromValueName, toValueName));
									}

									entry.EmployeeChildId = entryDTO.EmployeeChildId;
									isModified = true;
								}
								if (entry.IsBreak != entryDTO.IsBreak)
								{
									entry.IsBreak = entryDTO.IsBreak;
									isModified = true;
								}
								if (entry.IsPaidBreak != entryDTO.IsPaidBreak)
								{
									entry.IsPaidBreak = entryDTO.IsPaidBreak;
									isModified = true;
								}
								if (entry.IsDistanceWork != entryDTO.IsDistanceWork)
								{
									entry.IsDistanceWork = entryDTO.IsDistanceWork;
									isModified = true;
								}

								#endregion

								#region ShiftType

								bool tryGetShiftTypeAndTimeScheduleType = false;
								if (entryDTO.ShiftTypeId.HasValue && entry.ShiftTypeId != entryDTO.ShiftTypeId)
								{
									entry.ShiftTypeId = entryDTO.ShiftTypeId;
									isModified = true;
								}
								else
									tryGetShiftTypeAndTimeScheduleType = true;

								if (entryDTO.ShiftTypeId.HasValue && entry.TimeScheduleTypeId != entryDTO.TimeScheduleTypeId)
								{
									entry.TimeScheduleTypeId = entryDTO.TimeScheduleTypeId;
									isModified = true;
								}
								else
									tryGetShiftTypeAndTimeScheduleType = true;

								if (tryGetShiftTypeAndTimeScheduleType && entry.AccountId.HasValue)
								{
									if (!useTimeScheduleTypeFromTimeStampEntry.HasValue)
										useTimeScheduleTypeFromTimeStampEntry = SettingManager.GetBoolSetting(entities, SettingMainType.Company, (int)CompanySettingType.UseTimeScheduleTypeFromTime, 0, actorCompanyId, 0);

									if (useTimeScheduleTypeFromTimeStampEntry.HasValue && useTimeScheduleTypeFromTimeStampEntry.Value)
										SetShiftTypeAndTimeScheduleType(entities, entry, actorCompanyId, ref isModified);
								}

								if (entryDTO.TimeScheduleTypeId.HasValue && entryDTO.TimeScheduleTypeId != entry.TimeScheduleTypeId)
								{
									// Revert TimeScheduleType if it was set from the beginning
									entry.TimeScheduleTypeId = entryDTO.TimeScheduleTypeId;
									isModified = true;
								}

								#endregion

								#region TimeStampEntryExtended

								if (!entry.TimeStampEntryExtended.IsNullOrEmpty())
								{
									foreach (TimeStampEntryExtended extended in entry.TimeStampEntryExtended.Where(t => t.State == (int)SoeEntityState.Active).ToList())
									{
										bool isExtendedModified = false;

										if (!entryDTO.Extended.IsNullOrEmpty())
										{
											TimeStampEntryExtendedDTO extendedDTO = entryDTO.Extended.FirstOrDefault(e => e.TimeStampEntryExtendedId == extended.TimeStampEntryExtendedId);
											if (extendedDTO != null)
											{

												if (extendedDTO.TimeScheduleTypeId.HasValue || extendedDTO.TimeCodeId.HasValue || extendedDTO.AccountId.HasValue)
												{
													#region Update

													// Only set as modified if something really changed
													if (extended.TimeScheduleTypeId != extendedDTO.TimeScheduleTypeId)
													{
														trackChangesItems.Add(TrackChangesManager.InitTrackChanges(entities, actorCompanyId, actionMethod, TermGroup_TrackChangesAction.Update, SoeEntityType.Employee, entry.EmployeeId, SoeEntityType.TimeStampEntryExtended, extended.TimeStampEntryExtendedId, SettingDataType.Integer, null, TermGroup_TrackChangesColumnType.TimeStampEntryExtended_TimeScheduleTypeId, SoeEntityType.TimeStampEntry, entry.TimeStampEntryId, extended.TimeScheduleTypeId.ToString(), extendedDTO.TimeScheduleTypeId.ToString()));

														extended.TimeScheduleTypeId = extendedDTO.TimeScheduleTypeId;
														isExtendedModified = true;
													}
													if (extended.TimeCodeId != extendedDTO.TimeCodeId)
													{
														trackChangesItems.Add(TrackChangesManager.InitTrackChanges(entities, actorCompanyId, actionMethod, TermGroup_TrackChangesAction.Update, SoeEntityType.Employee, entry.EmployeeId, SoeEntityType.TimeStampEntryExtended, extended.TimeStampEntryExtendedId, SettingDataType.Integer, null, TermGroup_TrackChangesColumnType.TimeStampEntryExtended_TimeCodeId, SoeEntityType.TimeStampEntry, entry.TimeStampEntryId, extended.TimeCodeId.ToString(), extendedDTO.TimeCodeId.ToString()));

														extended.TimeCodeId = extendedDTO.TimeCodeId;
														isExtendedModified = true;
													}
													if (extended.AccountId != extendedDTO.AccountId)
													{
														trackChangesItems.Add(TrackChangesManager.InitTrackChanges(entities, actorCompanyId, actionMethod, TermGroup_TrackChangesAction.Update, SoeEntityType.Employee, entry.EmployeeId, SoeEntityType.TimeStampEntryExtended, extended.TimeStampEntryExtendedId, SettingDataType.Integer, null, TermGroup_TrackChangesColumnType.TimeStampEntryExtended_AccountId, SoeEntityType.TimeStampEntry, entry.TimeStampEntryId, extended.AccountId.ToString(), extendedDTO.AccountId.ToString()));

														extended.AccountId = extendedDTO.AccountId;
														isExtendedModified = true;
													}
													if (extended.Quantity != extendedDTO.Quantity)
													{
														trackChangesItems.Add(TrackChangesManager.InitTrackChanges(entities, actorCompanyId, actionMethod, TermGroup_TrackChangesAction.Update, SoeEntityType.Employee, entry.EmployeeId, SoeEntityType.TimeStampEntryExtended, extended.TimeStampEntryExtendedId, SettingDataType.Decimal, null, TermGroup_TrackChangesColumnType.TimeStampEntryExtended_Quantity, SoeEntityType.TimeStampEntry, entry.TimeStampEntryId, NumberUtility.GetFormattedDecimalStringValue(extended.Quantity, 2, true), NumberUtility.GetFormattedDecimalStringValue(extendedDTO.Quantity, 2, true)));

														extended.Quantity = extendedDTO.Quantity;
														isExtendedModified = true;
													}

													if (isExtendedModified)
														SetModifiedProperties(extended);

													#endregion
												}
												else
												{
													#region Delete

													// Existing TimeScheduleType, TimeCode or Account was cleared, delete it
													trackChangesItems.Add(TrackChangesManager.InitTrackChanges(entities, actorCompanyId, actionMethod, TermGroup_TrackChangesAction.Delete, SoeEntityType.Employee, entry.EmployeeId, SoeEntityType.TimeStampEntryExtended, extended.TimeStampEntryExtendedId, SoeEntityType.TimeStampEntry, entry.TimeStampEntryId));
													isExtendedModified = true;
													ChangeEntityState(extended, SoeEntityState.Deleted);

													if (extended.TimeCodeId.HasValue)
													{
														int? expenseRowId = entities.ExpenseRow.FirstOrDefault(f => f.TimeStampEntryExtendedId == extended.TimeStampEntryExtendedId)?.ExpenseRowId;
														if (expenseRowId.HasValue)
															expenseRowIdsToDelete.Add(expenseRowId.Value);

													}

													#endregion
												}

												// Remove from input list to prevent adding it below
												entryDTO.Extended.Remove(extendedDTO);
											}
											else
											{
												#region Delete

												// Not found in input, delete it
												trackChangesItems.Add(TrackChangesManager.InitTrackChanges(entities, actorCompanyId, actionMethod, TermGroup_TrackChangesAction.Delete, SoeEntityType.Employee, entry.EmployeeId, SoeEntityType.TimeStampEntryExtended, extended.TimeStampEntryExtendedId, SoeEntityType.TimeStampEntry, entry.TimeStampEntryId));
												isExtendedModified = true;
												ChangeEntityState(extended, SoeEntityState.Deleted);

												if (extended.TimeCodeId.HasValue)
												{
													int? expenseRowId = entities.ExpenseRow.FirstOrDefault(f => f.TimeStampEntryExtendedId == extended.TimeStampEntryExtendedId)?.ExpenseRowId;
													if (expenseRowId.HasValue)
														expenseRowIdsToDelete.Add(expenseRowId.Value);

												}

												#endregion
											}
										}
										else
										{
											#region Delete

											// Not found in input, delete it
											trackChangesItems.Add(TrackChangesManager.InitTrackChanges(entities, actorCompanyId, actionMethod, TermGroup_TrackChangesAction.Delete, SoeEntityType.Employee, entry.EmployeeId, SoeEntityType.TimeStampEntryExtended, extended.TimeStampEntryExtendedId, SoeEntityType.TimeStampEntry, entry.TimeStampEntryId));
											isExtendedModified = true;
											ChangeEntityState(extended, SoeEntityState.Deleted);

											if (extended.TimeCodeId.HasValue)
											{
												int? expenseRowId = entities.ExpenseRow.FirstOrDefault(f => f.TimeStampEntryExtendedId == extended.TimeStampEntryExtendedId)?.ExpenseRowId;
												if (expenseRowId.HasValue)
													expenseRowIdsToDelete.Add(expenseRowId.Value);
											}

											#endregion
										}

										if (isExtendedModified)
											isModified = true;
									}
								}

								#region Add

								if (!entryDTO.Extended.IsNullOrEmpty())
								{
									foreach (TimeStampEntryExtendedDTO extendedDTO in entryDTO.Extended)
									{
										if (extendedDTO.TimeScheduleTypeId.HasValue || extendedDTO.TimeCodeId.HasValue || extendedDTO.AccountId.HasValue)
										{
											TimeStampEntryExtended extended = new TimeStampEntryExtended()
											{
												TimeStampEntry = entry,
												TimeScheduleTypeId = extendedDTO.TimeScheduleTypeId,
												TimeCodeId = extendedDTO.TimeCodeId,
												AccountId = extendedDTO.AccountId,
												Quantity = extendedDTO.Quantity
											};
											SetCreatedProperties(extended);
											isModified = true;
										}
									}
								}

								#endregion

								#endregion

								#endregion

								#region Set modified

								if (isModified && entryDTO.TimeStampEntryId != 0)
								{
									// Entry has been modified
									entry.ManuallyAdjusted = true;
									entry.Status = (int)TermGroup_TimeStampEntryStatus.New;
									SetModifiedProperties(entry);

									// If an auto stamp out entry is modified, remove flag and note
									if (entry.AutoStampOut)
									{
										trackChangesItems.Add(TrackChangesManager.InitTrackChanges(entities, actorCompanyId, actionMethod, TermGroup_TrackChangesAction.Update, SoeEntityType.Employee, entry.EmployeeId, SoeEntityType.TimeStampEntry, entry.TimeStampEntryId, SettingDataType.Boolean, null, TermGroup_TrackChangesColumnType.TimeStampEntry_AutoStampOut, entry.AutoStampOut.ToString(), (!entry.AutoStampOut).ToString()));

										entry.AutoStampOut = false;

										if (!String.IsNullOrEmpty(entry.Note))
										{
											if (!entry.EmployeeReference.IsLoaded)
												entry.EmployeeReference.Load();
											if (entry.Employee != null && !entry.Employee.UserReference.IsLoaded)
												entry.Employee.UserReference.Load();

											int langId = entry.Employee.User != null && entry.Employee.User.LangId != null ? (int)entry.Employee.User.LangId : (int)TermGroup_Languages.Swedish;
											string note = TermCacheManager.Instance.GetText(3139, 1, "Automatisk utstämpling", langId);

											if (entry.Note == note)
											{
												trackChangesItems.Add(TrackChangesManager.InitTrackChanges(entities, actorCompanyId, actionMethod, TermGroup_TrackChangesAction.Update, SoeEntityType.Employee, entry.EmployeeId, SoeEntityType.TimeStampEntry, entry.TimeStampEntryId, SettingDataType.String, null, TermGroup_TrackChangesColumnType.TimeStampEntry_Note, entry.Note, null));

												entry.Note = null;
											}
										}
									}
								}

								if (isModified && isEmployeeCurrentUser)
									entry.EmployeeManuallyAdjusted = true;

								// Only add one timestamp per employee
								if (isModified && timeValidForPubSub)
								{
									// Always force a refresh on terminal if a stamp is updated
									if (entryDTO.TimeStampEntryId != 0 && !hasUpdate.Contains(employeeId))
										hasUpdate.Add(employeeId);

									if (!pubSubEntries.Any(e => e.EmployeeId == entry.EmployeeId))
										pubSubEntries.Add(entry);
								}

								#endregion

								#endregion
							}

							#endregion

							#region Delete

							foreach (TimeStampEntry entry in allTimeStampEntries)
							{
								// Check if added above, then dont delete it
								if (entry.TimeStampEntryId == 0)
									continue;

								// Check if existing entry also exists in input, otherwise delete it
								if (!inputDTOs.Any(e => e.TimeStampEntryId == entry.TimeStampEntryId))
								{
									trackChangesItems.Add(TrackChangesManager.InitTrackChanges(entities, actorCompanyId, actionMethod, TermGroup_TrackChangesAction.Delete, SoeEntityType.Employee, entry.EmployeeId, SoeEntityType.TimeStampEntry, entry.TimeStampEntryId));

									ChangeEntityState(entry, SoeEntityState.Deleted);

									#region TimeStampEntryExtended

									if (!entry.TimeStampEntryExtended.IsNullOrEmpty())
									{
										foreach (TimeStampEntryExtended extended in entry.TimeStampEntryExtended.Where(t => t.State == (int)SoeEntityState.Active).ToList())
										{
											trackChangesItems.Add(TrackChangesManager.InitTrackChanges(entities, actorCompanyId, actionMethod, TermGroup_TrackChangesAction.Delete, SoeEntityType.Employee, entry.EmployeeId, SoeEntityType.TimeStampEntryExtended, extended.TimeStampEntryExtendedId, SoeEntityType.TimeStampEntry, entry.TimeStampEntryId));
											ChangeEntityState(extended, SoeEntityState.Deleted);

											if (extended.TimeCodeId.HasValue)
											{
												int? expenseRowId = entities.ExpenseRow.FirstOrDefault(f => f.TimeStampEntryExtendedId == extended.TimeStampEntryExtendedId)?.ExpenseRowId;
												if (expenseRowId.HasValue)
													expenseRowIdsToDelete.Add(expenseRowId.Value);
											}
										}
									}

									#endregion
								}

								// Always force a refresh on terminal if a stamp is deleted
								if (!hasDelete.Contains(employeeId))
									hasDelete.Add(employeeId);

								// Only add one timestamp per employee
								bool timeValidForPubSub = (entry.Time > DateTime.Now.AddDays(-1) && entry.Time < DateTime.Now.AddDays(1));
								if (timeValidForPubSub && !pubSubEntries.Any(e => e.EmployeeId == entry.EmployeeId))
									pubSubEntries.Add(entry);
							}

							#endregion
						}
						else
						{
							#region Delete all

							// All entries for specified date deleted
							foreach (TimeStampEntry entry in allTimeStampEntries)
							{
								trackChangesItems.Add(TrackChangesManager.InitTrackChanges(entities, actorCompanyId, actionMethod, TermGroup_TrackChangesAction.Delete, SoeEntityType.Employee, entry.EmployeeId, SoeEntityType.TimeStampEntry, entry.TimeStampEntryId));

								ChangeEntityState(entry, SoeEntityState.Deleted);

								#region TimeStampEntryExtended

								if (!entry.TimeStampEntryExtended.IsNullOrEmpty())
								{
									foreach (TimeStampEntryExtended extended in entry.TimeStampEntryExtended.Where(t => t.State == (int)SoeEntityState.Active).ToList())
									{
										trackChangesItems.Add(TrackChangesManager.InitTrackChanges(entities, actorCompanyId, actionMethod, TermGroup_TrackChangesAction.Delete, SoeEntityType.Employee, entry.EmployeeId, SoeEntityType.TimeStampEntryExtended, extended.TimeStampEntryExtendedId, SoeEntityType.TimeStampEntry, entry.TimeStampEntryId));
										ChangeEntityState(extended, SoeEntityState.Deleted);

										if (extended.TimeCodeId.HasValue)
										{
											int? expenseRowId = entities.ExpenseRow.FirstOrDefault(f => f.TimeStampEntryExtendedId == extended.TimeStampEntryExtendedId)?.ExpenseRowId;
											if (expenseRowId.HasValue)
												expenseRowIdsToDelete.Add(expenseRowId.Value);
										}

									}
								}

								#endregion

								// Always force a refresh on terminal if a stamp is deleted
								if (!hasDelete.Contains(employeeId))
									hasDelete.Add(employeeId);

								// Only add one timestamp per employee
								bool timeValidForPubSub = (entry.Time > DateTime.Now.AddDays(-1) && entry.Time < DateTime.Now.AddDays(1));
								if (timeValidForPubSub && !pubSubEntries.Any(e => e.EmployeeId == entry.EmployeeId))
									pubSubEntries.Add(entry);
							}

							#endregion
						}

						#endregion

						result = SaveChanges(entities, transaction);

						//Commit transaction
						if (result.Success)
						{
							#region TrackChanges

							// Add track changes
							if (trackChangesItems.Any())
								result = TrackChangesManager.AddTrackChanges(entities, transaction, trackChangesItems);

							#endregion

							transaction.Complete();
						}
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
					}
					else
						base.LogTransactionFailed(this.ToString(), this.log);

					entities.Connection.Close();
				}

				#region WebPubSub

				if (pubSubEntries.Any())
				{
					Task.Run(() =>
					{
						using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
						List<int> terminalIds = TimeStampManager.GetTimeTerminalIdsForPubSub(entitiesReadOnly, actorCompanyId);
						foreach (TimeStampEntry entry in pubSubEntries)
						{
							// Set action based on importance for the terminal.
							// If delete or update, the terminal always needs to refresh the last stamp,
							// If insert, it will check if it was inserted from the same terminal.
							WebPubSubMessageAction action = WebPubSubMessageAction.Insert;
							if (hasDelete.Contains(entry.EmployeeId))
								action = WebPubSubMessageAction.Delete;
							else if (hasUpdate.Contains(entry.EmployeeId))
								action = WebPubSubMessageAction.Update;

							SendWebPubSubMessage(entitiesReadOnly, entry, action, terminalIds);
						}
					});
				}

				#endregion

				#region Event

				if (eventEntries.Any() && eventJobsSettings != null)
				{
					var timeStamps = eventEntries.Select(e => new TimeStampEventMessage()
					{
						ENr = employees?.FirstOrDefault(f => f.EmployeeId == e.EmployeeId)?.EmployeeNr ?? "",
						ECode = employees?.FirstOrDefault(f => f.EmployeeId == e.EmployeeId)?.ExternalCode ?? "NA",
						In = e.Type == (int)TimeStampEntryType.In,
						Time = e.Time,
						Acc = accounts?.FirstOrDefault(f => f.AccountId == e.AccountId)?.ExternalCode ?? accounts?.FirstOrDefault(f => f.AccountId == e.AccountId)?.AccountNr ?? ""
					}).ToList();

					var info = string.Join("####", timeStamps.Select(s => JsonConvert.SerializeObject(s, Formatting.Indented)));

					foreach (var eventSettings in eventJobsSettings)
					{
						try
						{
							ScheduledJobManager.RunBridgeJobFireAndForget(base.ActorCompanyId, 0, DateTime.Now, ScheduledJobManager.GetScheduledJobHead(entities, eventSettings.ScheduledJobHeadId, base.ActorCompanyId, loadRows: true, loadLogs: true, loadSettings: true, loadSettingOptions: true, false, false), null, eventInfo: info);
						}
						catch (Exception ex)
						{
							LogCollector.LogError(ex, $"Error when running bridge job for TimeStampCreated event actorcompanyId {actorCompanyId}");
						}
					}
				}

				#endregion

				if (result.Success)
				{
					// Regenerate day
					// Must pass in entrys that where deleted above, otherwise day wont be recalculated correctly in SaveDeviationsFromTimeStamps
					var tem = TimeEngineManager(actorCompanyId, base.UserId);
					result = tem.ReGenerateDayBasedOnTimeStamps(allTimeStampEntries, discardBreakEvaluation: discardBreakEvaluation);
					if (!result.Success)
						return result;

					if (result.Success && expenseRowIdsToDelete.Any())
					{
						foreach (int expenseRowId in expenseRowIdsToDelete)
						{
							result = tem.DeleteExpense(expenseRowId, true);
							if (!result.Success)
								return result;
						}
					}
				}

				return result;
			}
		}

		public ActionResult SaveAdjustedTimeStampEntries(List<TimeStampEntryDTO> inputDTOs)
		{
			if (inputDTOs.IsNullOrEmpty())
				return new ActionResult((int)ActionResultSave.EntityIsNull, "TimeStampEntryDTO");

			// Default result is successful
			ActionResult result = null;

			List<TrackChangesDTO> trackChangesItems = new List<TrackChangesDTO>();
			TermGroup_TrackChangesActionMethod actionMethod = TermGroup_TrackChangesActionMethod.TimeStampEntry_Save_Adjust;

			using (CompEntities entities = new CompEntities())
			{
				List<TimeStampEntry> allTimeStampEntries = new List<TimeStampEntry>();
				List<int> handledTimeBlockDateIds = new List<int>();

				try
				{
					entities.Connection.Open();

					using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
					{
						#region TimeStampEntry

						TimeBlockDate oldTimeBlockDate = null;
						TimeBlockDate newTimeBlockDate = null;
						foreach (var dto in inputDTOs.Where(t => t.AdjustedTimeBlockDateDate.HasValue || t.AdjustedTime.HasValue).OrderBy(t => t.EmployeeId).ThenBy(t => t.TimeBlockDateId))
						{
							// Old TimeBlockDate still in TimeBlockDateId
							if (dto.TimeBlockDateId.HasValue && (oldTimeBlockDate == null || oldTimeBlockDate.TimeBlockDateId != dto.TimeBlockDateId.Value))
								oldTimeBlockDate = TimeBlockManager.GetTimeBlockDate(entities, dto.TimeBlockDateId.Value, dto.EmployeeId);

							// New TimeBlockDate in AdjustedTimeBlockDateDate
							if (newTimeBlockDate == null || newTimeBlockDate.EmployeeId != dto.EmployeeId || newTimeBlockDate.Date != dto.AdjustedTimeBlockDateDate.Value)
								newTimeBlockDate = TimeBlockManager.GetTimeBlockDate(entities, dto.ActorCompanyId, dto.EmployeeId, dto.AdjustedTimeBlockDateDate.Value, true);

							if (newTimeBlockDate != null && newTimeBlockDate.TimeBlockDateId == 0)
								SaveChanges(entities, transaction);

							// Add TimeBlockDateId to modified collection
							if (oldTimeBlockDate != null && !handledTimeBlockDateIds.Contains(oldTimeBlockDate.TimeBlockDateId))
								handledTimeBlockDateIds.Add(oldTimeBlockDate.TimeBlockDateId);

							// Add TimeBlockDateId to modified collection
							if (newTimeBlockDate != null && !handledTimeBlockDateIds.Contains(newTimeBlockDate.TimeBlockDateId))
								handledTimeBlockDateIds.Add(newTimeBlockDate.TimeBlockDateId);

							// Get existing entry
							TimeStampEntry entry = GetTimeStampEntry(entities, dto.TimeStampEntryId, true, false);
							if (!entry.TimeBlockDateReference.IsLoaded)
								entry.TimeBlockDateReference.Load();

							// Update TimeBlockDate
							if (newTimeBlockDate != null && oldTimeBlockDate != newTimeBlockDate)
							{
								trackChangesItems.Add(TrackChangesManager.InitTrackChanges(entities, dto.ActorCompanyId, actionMethod, TermGroup_TrackChangesAction.Update, SoeEntityType.Employee, entry.EmployeeId, SoeEntityType.TimeStampEntry, entry.TimeStampEntryId, SettingDataType.Time, null, TermGroup_TrackChangesColumnType.TimeStampEntry_TimeBlockDateId, CalendarUtility.ToShortDateString(entry.Time), CalendarUtility.ToShortDateString(newTimeBlockDate.Date)));

								entry.TimeBlockDate = newTimeBlockDate;
								entry.TimeScheduleTemplatePeriod = TimeScheduleManager.GetTimeScheduleTemplatePeriod(entities, dto.EmployeeId, newTimeBlockDate.Date);
							}
							// Update Time
							if (dto.AdjustedTime.HasValue && entry.Time != dto.AdjustedTime.Value)
							{
								bool dateHasChanged = entry.Time.Date != dto.AdjustedTime.Value.Date;
								trackChangesItems.Add(TrackChangesManager.InitTrackChanges(entities, dto.ActorCompanyId, actionMethod, TermGroup_TrackChangesAction.Update, SoeEntityType.Employee, entry.EmployeeId, SoeEntityType.TimeStampEntry, entry.TimeStampEntryId, SettingDataType.Time, null, TermGroup_TrackChangesColumnType.TimeStampEntry_Time, dateHasChanged ? CalendarUtility.ToShortDateTimeString(entry.Time) : CalendarUtility.ToTime(entry.Time), dateHasChanged ? CalendarUtility.ToShortDateTimeString(dto.AdjustedTime.Value) : CalendarUtility.ToTime(dto.AdjustedTime.Value)));

								entry.Time = dto.AdjustedTime.Value;
							}
							entry.ManuallyAdjusted = true;
							SetModifiedProperties(entry);

							// If an auto stamp out entry is modified, remove flag and note
							if (entry.AutoStampOut)
							{
								trackChangesItems.Add(TrackChangesManager.InitTrackChanges(entities, dto.ActorCompanyId, actionMethod, TermGroup_TrackChangesAction.Update, SoeEntityType.Employee, entry.EmployeeId, SoeEntityType.TimeStampEntry, entry.TimeStampEntryId, SettingDataType.Boolean, null, TermGroup_TrackChangesColumnType.TimeStampEntry_AutoStampOut, entry.AutoStampOut.ToString(), (!entry.AutoStampOut).ToString()));

								entry.AutoStampOut = false;

								if (!String.IsNullOrEmpty(entry.Note))
								{
									if (!entry.EmployeeReference.IsLoaded)
										entry.EmployeeReference.Load();
									if (entry.Employee != null && !entry.Employee.UserReference.IsLoaded)
										entry.Employee.UserReference.Load();

									int langId = entry.Employee.User != null && entry.Employee.User.LangId != null ? (int)entry.Employee.User.LangId : (int)TermGroup_Languages.Swedish;
									string note = TermCacheManager.Instance.GetText(3139, 1, "Automatisk utstämpling", langId);

									if (entry.Note == note)
									{
										trackChangesItems.Add(TrackChangesManager.InitTrackChanges(entities, dto.ActorCompanyId, actionMethod, TermGroup_TrackChangesAction.Update, SoeEntityType.Employee, entry.EmployeeId, SoeEntityType.TimeStampEntry, entry.TimeStampEntryId, SettingDataType.String, null, TermGroup_TrackChangesColumnType.TimeStampEntry_Note, entry.Note, null));

										entry.Note = null;
									}
								}
							}
						}

						#endregion

						result = SaveChanges(entities, transaction);

						//Commit transaction
						if (result.Success)
						{
							#region TrackChanges

							// Add track changes
							if (trackChangesItems.Any())
								result = TrackChangesManager.AddTrackChanges(entities, transaction, trackChangesItems);

							#endregion

							transaction.Complete();
						}
					}
				}
				catch (Exception ex)
				{
					base.LogError(ex, this.log);
					result = new ActionResult(ex);
				}
				finally
				{
					if (result != null && result.Success)
					{
						//Set success properties
					}
					else
						base.LogTransactionFailed(this.ToString(), this.log);

					entities.Connection.Close();
				}

				if (result.Success)
				{
					// Get all entries for every modified day
					foreach (int timeBlockDateId in handledTimeBlockDateIds)
					{
						allTimeStampEntries.AddRange(GetTimeStampEntriesForRecalculation(entities, timeBlockDateId));
					}

					// Regenerate day
					result = TimeEngineManager(base.ActorCompanyId, base.UserId).ReGenerateDayBasedOnTimeStamps(allTimeStampEntries, false);
					if (!result.Success)
						return result;
				}

				return result;
			}
		}

		public void SetShiftTypeAndTimeScheduleType(CompEntities entities, TimeStampEntry entry, int actorCompanyId)
		{
			bool modified = false;
			SetShiftTypeAndTimeScheduleType(entities, entry, actorCompanyId, ref modified);
		}

		public void SetShiftTypeAndTimeScheduleType(CompEntities entities, TimeStampEntry entry, int actorCompanyId, ref bool modfied)
		{
			if (entry.AccountId.HasValue)
			{
				var tuple = TryGetShiftTypeAndTimeScheduleType(entities, actorCompanyId, entry.AccountId.Value);

				if (tuple == null)
					tuple = Tuple.Create((int?)null, (int?)null);

				if (tuple.Item1.HasValue)
				{
					entry.ShiftTypeId = tuple.Item1;
					modfied = true;
				}

				if (tuple.Item2.HasValue)
				{
					entry.TimeScheduleTypeId = tuple.Item2;
					modfied = true;
				}
			}
		}

		#endregion

		#region TimeStampAddition

		public List<TimeStampAdditionDTO> GetTimeStampAdditions(int actorCompanyId, bool isMySelf = false, int timeTerminalId = 0)
		{
			List<TimeStampAdditionDTO> timeStampAdditions = new List<TimeStampAdditionDTO>();

			bool isLimited = false;
			List<int> validTimeScheduleTypes = new List<int>();
			List<int> validTimeCodes = new List<int>();

			if (timeTerminalId != 0)
			{
				// Called from terminal, check settings if limited on terminal
				isLimited = GetTimeTerminalBoolSetting(TimeTerminalSettingType.LimitSelectableAdditions, timeTerminalId);
				if (isLimited)
				{
					string additionsSetting = GetTimeTerminalStringSetting(TimeTerminalSettingType.SelectedAdditions, timeTerminalId);
					string[] hashedString = additionsSetting.Split(',');
					if (hashedString.Any())
					{
						foreach (string str in hashedString)
						{
							string[] ids = str.Split('#');
							if (ids.Count() == 2 && int.TryParse(ids[0], out int type) && int.TryParse(ids[1], out int id))
							{
								if (type == (int)TimeStampAdditionType.TimeScheduleType)
									validTimeScheduleTypes.Add(id);
								else if (type == (int)TimeStampAdditionType.TimeCodeConstantValue || type == (int)TimeStampAdditionType.TimeCodeVariableValue)
									validTimeCodes.Add(id);
							}
						}
					}
				}
			}

			using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
			if (SettingManager.GetBoolSetting(entitiesReadOnly, SettingMainType.Company, (int)CompanySettingType.PossibilityToRegisterAdditionsInTerminal, 0, actorCompanyId, 0))
			{
				List<TimeScheduleTypeDTO> timeScheduleTypes = TimeScheduleManager.GetTimeScheduleTypes(entitiesReadOnly, actorCompanyId).ToDTOs(false, onlyShowInTerminal: true).ToList();
				List<TimeCodeAdditionDeductionDTO> timeCodes = TimeCodeManager.GetTimeCodeAdditionDeductions(entitiesReadOnly, actorCompanyId, isMySelf: isMySelf).ToAdditionDeductionDTOs(onlyShowInTerminal: true);

				if (timeScheduleTypes.Any())
				{
					foreach (TimeScheduleTypeDTO timeScheduleType in timeScheduleTypes)
					{
						if (isLimited && !validTimeScheduleTypes.Contains(timeScheduleType.TimeScheduleTypeId))
							continue;

						timeStampAdditions.Add(new TimeStampAdditionDTO()
						{
							Id = timeScheduleType.TimeScheduleTypeId,
							Name = timeScheduleType.Name,
							Type = TimeStampAdditionType.TimeScheduleType
						});
					}
				}

				if (timeCodes.Any(w => w.RegistrationType == TermGroup_TimeCodeRegistrationType.Quantity))
				{
					foreach (TimeCodeAdditionDeductionDTO timeCode in timeCodes.Where(w => w.RegistrationType == TermGroup_TimeCodeRegistrationType.Quantity))
					{
						if (isLimited && !validTimeCodes.Contains(timeCode.TimeCodeId))
							continue;

						timeStampAdditions.Add(new TimeStampAdditionDTO()
						{
							Id = timeCode.TimeCodeId,
							Name = timeCode.Name,
							Type = timeCode.FixedQuantity.HasValue && timeCode.FixedQuantity.Value > 0 ? TimeStampAdditionType.TimeCodeConstantValue : TimeStampAdditionType.TimeCodeVariableValue,
							FixedQuantity = timeCode.FixedQuantity
						});
					}
				}
			}

			return timeStampAdditions.OrderBy(o => o.Name).ToList();
		}

		#endregion

		#region TimeStampAttendenceEvacuationList

		public List<TimeStampEntry> GetTimeStampEvacuationList(int actorCompanyId, List<int> employeeIds = null)
		{
			DateTime fromDate = DateTime.Now.AddDays(-1);
			DateTime toDate = DateTime.Today.AddDays(1);
			using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
			entities.TimeStampEntry.NoTracking();
			IQueryable<TimeStampEntry> query = (from e in entities.TimeStampEntry.Include("Employee").Include("TimeDeviationCause").Include("Account")
												where e.ActorCompanyId == actorCompanyId &&
													e.State == (int)SoeEntityState.Active &&
													e.Time <= toDate && e.Time >= fromDate &&
													employeeIds.Contains(e.EmployeeId)
												select e);
			return query.ToList();

		}
		#endregion

		#region GetTimeStampEntrysEmployeeSummaryResult

		public List<GetTimeStampEntrysEmployeeSummaryResult> GetTimeStampEntrysEmployeeSummary(CompEntities entities, List<TimeBlockDate> timeBlockDates, int employeeId)
		{
			if (timeBlockDates.IsNullOrEmpty())
				return new List<GetTimeStampEntrysEmployeeSummaryResult>();

			DateTime startDate = timeBlockDates.Select(i => i.Date).Min();
			DateTime stopDate = timeBlockDates.Select(i => i.Date).Max();
			return GetTimeStampEntrysEmployeeSummary(entities, startDate, stopDate, employeeId);
		}

		public List<GetTimeStampEntrysEmployeeSummaryResult> GetTimeStampEntrysEmployeeSummary(CompEntities entities, DateTime startDate, DateTime stopDate, int employeeId)
		{
			return entities.GetTimeStampEntrysEmployeeSummary(employeeId, startDate, stopDate).ToList();
		}

		#endregion

		#region TimeStampRaw

		public List<int> GetUnhandledTimeStampEntryRawIds()
		{
			using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
			entities.TimeStampEntryRaw.NoTracking();
			return (from tser in entities.TimeStampEntryRaw
					where tser.Status == (int)TermGroup_TimeStampEntryStatus.New
					select tser.TimeStampEntryRawId).ToList();
		}

		public TimeStampEntryRaw GetTimeStampEntryRaw(CompEntities entities, int id)
		{
			return (from tser in entities.TimeStampEntryRaw
					where tser.TimeStampEntryRawId == id
					select tser).FirstOrDefault();
		}

		public ActionResult SaveTimeStampEntryRaw(TimeStampEntryRawDTO inputDTO)
		{
			if (inputDTO == null)
				return new ActionResult((int)ActionResultSave.EntityIsNull, "TimeStampEntryRawDTO");

			// Default result is successful
			ActionResult result = null;

			using (CompEntities entities = new CompEntities())
			{
				try
				{
					TimeStampEntryRaw ts = new TimeStampEntryRaw()
					{
						EmployeeNr = inputDTO.EmployeeNr,
						Type = (int)inputDTO.Type,
						Status = (int)inputDTO.Status,
						TerminalStampData = inputDTO.TerminalStampData,
						Time = inputDTO.Time,
						OriginalTime = inputDTO.Time,
						IsBreak = inputDTO.IsBreak,

						//Set FK
						TimeDeviationCauseRecordid = inputDTO.TimeDeviationCauseRecordId,
						TimeTerminalRecordId = inputDTO.TimeTerminalRecordId,
						AccountRecordId = inputDTO.AccountRecordId,
						ActorCompanyRecordId = inputDTO.ActorCompanyRecordId,
						EmployeeChildRecordId = inputDTO.EmployeeChildRecordId,
					};
					SetCreatedProperties(ts);
					entities.TimeStampEntryRaw.AddObject(ts);

					result = SaveChanges(entities);
				}
				catch (Exception ex)
				{
					base.LogError(ex, this.log);
					result = new ActionResult(ex);
				}
				finally
				{
					if (result != null && result.Success)
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

		public ActionResult TransferFromTimeStampEntryRawToTimeStampEntry(int timeStampEntryRawId)
		{
			ActionResult result = new ActionResult();

			using (CompEntities entities = new CompEntities())
			{
				TimeStampEntryRaw raw = GetTimeStampEntryRaw(entities, timeStampEntryRawId);
				if (raw != null)
				{
					try
					{
						Employee employee = EmployeeManager.GetEmployeeByNr(entities, raw.EmployeeNr, raw.ActorCompanyRecordId);
						if (employee != null)
						{
							int? timeDeviationCauseId = null;
							if (raw.TimeDeviationCauseRecordid.HasValue && raw.TimeDeviationCauseRecordid.Value != 0)
								timeDeviationCauseId = raw.TimeDeviationCauseRecordid.Value;
							else
								timeDeviationCauseId = TimeDeviationCauseManager.GetTimeDeviationCauseIdFromPrio(employee, raw.Time.Date);

							if (timeDeviationCauseId == 0)
								timeDeviationCauseId = null;

							TimeTerminal terminal = GetTimeTerminalDiscardState(entities, raw.TimeTerminalRecordId);

							if (terminal != null && terminal.ActorCompanyId == raw.ActorCompanyRecordId)
							{
								EmployeeGroup employeeGroup = employee.CurrentEmployeeGroup;
								int breakDayMinutesAfterMidninght = employeeGroup != null ? employeeGroup.BreakDayMinutesAfterMidnight : 0;

								//Need to control that the kid is connected to the correct parent(employee)
								int? employeeChildId = null;
								if (raw.EmployeeChildRecordId.HasValue)
								{
									EmployeeChild employeeChild = EmployeeManager.GetEmployeeChild(entities, (int)raw.EmployeeChildRecordId);

									if (employeeChild != null && employeeChild.EmployeeId == employee.EmployeeId)
										employeeChildId = employeeChild.EmployeeChildId;
								}

								//Check if InternalAccount setting on terminal
								int? internalAccountId = raw.AccountRecordId != 0 ? raw.AccountRecordId : (int?)null;

								if (raw.AccountRecordId == 0 && !internalAccountId.HasValue)
								{
									var internalAccountIdfromSetting = GetTimeTerminalSetting(TimeTerminalSettingType.InternalAccountDim1Id, terminal.TimeTerminalId);

									if (internalAccountIdfromSetting != null)
										internalAccountId = (internalAccountIdfromSetting.IntData.HasValue && internalAccountIdfromSetting.IntData.Value != 0) ? internalAccountIdfromSetting.IntData : (int?)null;
								}

								terminal.LastSync = DateTime.Now;

								TimeStampEntry entry = new TimeStampEntry()
								{
									Type = raw.Type,
									Status = (int)TermGroup_TimeStampEntryStatus.New,
									OriginType = (int)TermGroup_TimeStampEntryOriginType.TerminalUnspecified,
									TerminalStampData = raw.TerminalStampData,
									Time = raw.Time,
									OriginalTime = raw.OriginalTime,
									Created = DateTime.Now,
									IsBreak = raw.IsBreak,

									//Set FK
									ActorCompanyId = raw.ActorCompanyRecordId,
									EmployeeChildId = employeeChildId,
									AccountId = internalAccountId,
									TimeDeviationCauseId = timeDeviationCauseId,
									TimeBlockDateId = TimeBlockManager.GetTimeBlockDateFromCache(entities, employee.ActorCompanyId, employee.EmployeeId, raw.Time.AddMinutes(-breakDayMinutesAfterMidninght).Date, true).TimeBlockDateId,

									//Set references
									TimeTerminal = terminal,
									Employee = employee,
								};
								entities.TimeStampEntry.AddObject(entry);
								result = SaveChanges(entities);

								if (result.Success)
								{
									raw.TimeStampEntryId = entry.TimeStampEntryId;
									raw.Status = (int)TermGroup_TimeStampEntryStatus.Processed;
									SetModifiedProperties(raw);

									result = SaveChanges(entities);
								}
							}
							else
							{
								raw.ErrorMessage = "Invalid terminal id: " + raw.TimeTerminalRecordId;
								raw.Status = (int)TermGroup_TimeStampEntryStatus.ProcessedWithNoResult;
								SetModifiedProperties(raw);

								result = SaveChanges(entities);
							}
						}
						else
						{
							Company company = CompanyManager.GetCompany(raw.ActorCompanyRecordId);

							if (company != null)
							{

								SystemInfoLog siLog = new SystemInfoLog()
								{
									Entity = (int)SoeEntityType.Employee,
									LogLevel = (int)SystemInfoLogLevel.Warning,
									RecordId = 0,
									Date = DateTime.Now,
									Text = "{0} " + raw.EmployeeNr + " {1}",
									Type = (int)SystemInfoType.TimeStamp_EmployeeMissing,
									DeleteManually = true,

									//Set FK
									ActorCompanyId = raw.ActorCompanyRecordId,
								};

								GeneralManager.AddSystemInfoLogEntry(entities, siLog);
							}

							raw.ErrorMessage = "Invalid employee number: " + raw.EmployeeNr;
							raw.Status = (int)TermGroup_TimeStampEntryStatus.ProcessedWithNoResult;
							SetModifiedProperties(raw);


							result = SaveChanges(entities);
						}
					}
					catch (Exception ex)
					{
						raw.ErrorMessage = "Exception thrown: " + ex.Message;
						raw.Status = (int)TermGroup_TimeStampEntryStatus.ProcessedWithNoResult;
						SetModifiedProperties(raw);

						result = SaveChanges(entities);

						base.LogError(ex, this.log);
						result.Exception = ex;
					}
				}
			}

			return result;
		}

		#endregion

		#region TimeStampRounding

		/// <summary>
		/// Get TimeStampRounding for employeeGroup
		/// </summary>
		/// <param name="employeeGroupId">EmployeeGroup ID</param>
		/// <returns>TimeStampRounding</returns>
		public TimeStampRounding GetTimeStampRoundingByEmployeeGroup(int employeeGroupId)
		{
			using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
			entities.TimeAccumulator.NoTracking();
			return GetTimeStampRoundingByEmployeeGroup(entities, employeeGroupId);
		}

		/// <summary>
		/// Get TimeStampRounding for employeeGroup
		/// </summary>       
		/// <param name="entities">Object context</param>
		/// <param name="employeeGroupId">EmployeeGroup ID</param>
		/// <param name="actorCompanyId">Company ID</param>
		/// <returns>TimeStampRounding</returns>
		public TimeStampRounding GetTimeStampRoundingByEmployeeGroup(CompEntities entities, int employeeGroupId)
		{
			return (from tsr in entities.TimeStampRounding
						.Include("EmployeeGroup")
					where tsr.EmployeeGroup.EmployeeGroupId == employeeGroupId
					select tsr).FirstOrDefault();
		}

		/// <summary>
		/// Get TimeStampRounding by TimeStampRoundingId
		/// </summary>       
		/// <param name="entities">Object context</param>
		/// <param name="timeStampRoundingId">TimeStampRounding ID</param>        
		/// <returns>TimeStampRounding</returns>
		public TimeStampRounding GetTimeStampRoundingById(CompEntities entities, int timeStampRoundingId)
		{

			TimeStampRounding timeStampRounding = (from tsr in entities.TimeStampRounding
												   where tsr.TimeStampRoundingId == timeStampRoundingId
												   select tsr).FirstOrDefault();
			return timeStampRounding;
		}

		/// <summary>
		/// Save TimeStampRounding for employeeGroup
		/// </summary>       
		/// <param name="timeStampRounding">TimeStampRounding object to insert</param>
		/// <param name="employeeGroupId">EmployeeGroup ID</param>
		/// <param name="actorCompanyId">Company ID</param>
		/// <returns>ActionResult</returns>
		public ActionResult AddTimeStampRounding(TimeStampRounding timeStampRounding, int employeeGroupId)
		{
			if (timeStampRounding == null)
				return new ActionResult((int)ActionResultSave.EntityIsNull, "TimeStampRounding");

			using (CompEntities entities = new CompEntities())
			{
				timeStampRounding.EmployeeGroup = EmployeeManager.GetEmployeeGroup(entities, employeeGroupId);
				if (timeStampRounding.EmployeeGroup == null)
					return new ActionResult((int)ActionResultSave.EntityNotFound, "EmployeeGroup");

				return AddEntityItem(entities, timeStampRounding, "TimeStampRounding");
			}
		}

		/// <summary>
		/// Update TimeStampRounding settings
		/// </summary>       
		/// <param name="timeStampRounding">TimeStampRounding object with updated data</param>         
		/// <returns>ActionResult</returns>
		public ActionResult UpdateTimeStampRounding(TimeStampRounding timeStampRounding)
		{
			if (timeStampRounding == null)
				return new ActionResult((int)ActionResultSave.EntityIsNull, "TimeStampRounding");

			using (CompEntities entities = new CompEntities())
			{
				TimeStampRounding originalTimeStampRounding = GetTimeStampRoundingById(entities, timeStampRounding.TimeStampRoundingId);
				if (originalTimeStampRounding == null)
					return new ActionResult((int)ActionResultSave.EntityNotFound, "TimeStampRounding");

				return UpdateEntityItem(entities, originalTimeStampRounding, timeStampRounding, "TimeStampRounding");
			}
		}

		#endregion

		#region Synchronize

		#region TimeTerminal

		/// <summary>
		/// Synchronize time terminal (get modifications)
		/// </summary>
		/// <param name="actorCompanyId">Company ID</param>
		/// <param name="timeTerminalId">TimeTerminal ID</param>
		/// <param name="type">Terminal type</param>
		/// <param name="prevSyncDate">Last sync or null if all terminals should be returned</param>
		/// <returns>One terminal if modified, else null</returns>
		public TimeTerminal SyncTimeTerminal(int actorCompanyId, int timeTerminalId, TimeTerminalType type, DateTime? prevSyncDate)
		{
			int terminalType = (int)type;

			using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
			entities.TimeTerminal.NoTracking();
			return (from t in entities.TimeTerminal.Include("Company").Include("TimeTerminalSetting")
					where t.TimeTerminalId == timeTerminalId &&
					t.ActorCompanyId == actorCompanyId &&
					t.Type == terminalType &&
					(!prevSyncDate.HasValue ||
					((t.Modified.HasValue && t.Modified.Value > prevSyncDate.Value) ||
						(!t.Modified.HasValue && t.Created.HasValue && t.Created.Value > prevSyncDate.Value) ||
						!t.Created.HasValue))
					select t).FirstOrDefault();
		}

		public ActionResult SetTimeTerminalLastSync(int timeTerminalId)
		{
			DateTime syncTime = DateTime.Now;

			ActionResult result = new ActionResult();

			using (CompEntities entities = new CompEntities())
			{
				TimeTerminal timeTerminal = GetTimeTerminalDiscardState(entities, timeTerminalId);
				if (timeTerminal == null)
					return new ActionResult((int)ActionResultSave.EntityNotFound, "TimeTerminal");

				timeTerminal.LastSync = syncTime;
				result.ObjectsAffected = entities.SaveChanges();
				if (result.ObjectsAffected == 0)
				{
					result.Success = false;
					result.ErrorNumber = (int)ActionResultSave.NothingSaved;
				}
				result.DateTimeValue = syncTime;
			}

			return result;
		}

		#endregion

		#region Account

		/// <summary>
		/// Synchronize accounts (get modifications)
		/// </summary>
		/// <param name="actorCompanyId">Company ID</param>
		/// <param name="accountDimId">Account dim to synchronize</param>
		/// <param name="prevSyncDate">Last sync or null if all accounts should be returned</param>
		/// <returns>List of accounts created or modified since last sync</returns>
		public IEnumerable<TSAccountItem> SyncAccount(int actorCompanyId, int accountDimId, DateTime? prevSyncDate)
		{
			using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
			entities.Account.NoTracking();
			return (from a in entities.Account
					where a.ActorCompanyId == actorCompanyId &&
					a.AccountDimId == accountDimId &&
					(!prevSyncDate.HasValue ||
					((a.Modified.HasValue && a.Modified.Value > prevSyncDate.Value) ||
					   (!a.Modified.HasValue && a.Created.HasValue && a.Created.Value > prevSyncDate.Value) ||
					   !a.Created.HasValue))
					select new TSAccountItem()
					{
						AccountId = a.AccountId,
						AccountNr = a.AccountNr,
						Name = a.Name,
						Created = a.Created,
						Modified = a.Modified,
						State = a.State
					}).ToList();
		}

		/// <summary>
		/// Synchronize accounts (get modifications)
		/// </summary>
		/// <param name="actorCompanyId">Company ID</param>
		/// <param name="timeTerminalId">Terminal to synchronize</param>
		/// <param name="prevSyncDate">Last sync or null if all accounts should be returned</param>
		/// <returns>List of accounts connected to terminals categories created or modified since last sync</returns>
		public IEnumerable<TSAccountItem> SyncAccountWithLimits(int actorCompanyId, int timeTerminalId, DateTime? prevSyncDate)
		{
			// Get terminal
			TimeTerminal terminal = GetTimeTerminalDiscardState(timeTerminalId);
			if (terminal == null)
				return new List<TSAccountItem>();

			bool limitToAccounts = GetTimeTerminalBoolSetting(TimeTerminalSettingType.LimitTimeTerminalToAccount, terminal);
			bool limitToCategories = GetTimeTerminalBoolSetting(TimeTerminalSettingType.LimitTimeTerminalToCategories, terminal);

			// Get account dim
			int accountDimId = GetTimeTerminalIntSetting(TimeTerminalSettingType.AccountDim, terminal);

			// If not limited, sync all accounts
			if (!limitToAccounts && !limitToCategories)
				return SyncAccount(actorCompanyId, accountDimId, prevSyncDate);

			// Get all accounts
			using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
			entities.Account.NoTracking();
			List<Account> accounts = (from a in entities.Account
									  where a.ActorCompanyId == actorCompanyId &&
									  a.AccountDimId == accountDimId &&
									  (!prevSyncDate.HasValue ||
									  ((a.Modified.HasValue && a.Modified.Value > prevSyncDate.Value) ||
										 (!a.Modified.HasValue && a.Created.HasValue && a.Created.Value > prevSyncDate.Value) ||
										 !a.Created.HasValue))
									  select a).ToList();


			return limitToAccounts ? SyncAccountWithAccountLimits(actorCompanyId, timeTerminalId, accounts) : SyncAccountWithCategoryLimits(actorCompanyId, timeTerminalId, accounts);
		}

		public IEnumerable<TSAccountItem> SyncAccountWithAccountLimits(int actorCompanyId, int timeTerminalId, List<Account> accounts)
		{
			List<TSAccountItem> accountItems = new List<TSAccountItem>();

			// Get accounts linked to terminal
			List<int> terminalAccountIds = GetAccountIdsByTimeTerminal(timeTerminalId);
			List<int> terminalHierarchyAccountIds = new List<int>();

			AccountHierarchyInput input = AccountHierarchyInput.GetInstance(AccountHierarchyParamType.IncludeVirtualParented);
			foreach (int terminalAccountId in terminalAccountIds)
			{
				terminalHierarchyAccountIds.AddRange(AccountManager.GetAccountsFromHierarchyById(actorCompanyId, terminalAccountId, input).Select(a => a.AccountId));
			}
			terminalHierarchyAccountIds = terminalHierarchyAccountIds.Distinct().ToList();

			foreach (Account account in accounts)
			{
				if (terminalHierarchyAccountIds.Contains(account.AccountId))
				{
					accountItems.Add(new TSAccountItem()
					{
						AccountId = account.AccountId,
						AccountNr = account.AccountNr,
						Name = account.Name,
						Created = account.Created,
						Modified = account.Modified,
						State = account.State
					});
				}
			}

			return accountItems;
		}

		public IEnumerable<TSAccountItem> SyncAccountWithCategoryLimits(int actorCompanyId, int timeTerminalId, List<Account> accounts)
		{
			List<TSAccountItem> accountItems = new List<TSAccountItem>();
			using var entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();

			// Get categories linked to terminal
			List<int> terminalCategoryIds = GetCategoriesByTimeTerminal(actorCompanyId, timeTerminalId).Select(c => c.CategoryId).ToList();

			// Loop through accounts and check if they are connected to a category specified on the terminal
			entitiesReadOnly.CategoryAccount.NoTracking();
			foreach (Account account in accounts)
			{
				List<int> catAccountIds = (from ca in entitiesReadOnly.CategoryAccount
										   where ca.AccountId == account.AccountId &&
										   ca.State == (int)SoeEntityState.Active
										   select ca.CategoryId).ToList();

				if (terminalCategoryIds.ContainsAny(catAccountIds))
				{
					accountItems.Add(new TSAccountItem()
					{
						AccountId = account.AccountId,
						AccountNr = account.AccountNr,
						Name = account.Name,
						Created = account.Created,
						Modified = account.Modified,
						State = account.State
					});
				}
			}

			return accountItems;
		}

		#endregion

		#region Employee

		/// <summary>
		/// Synchronize employees (get modifications)
		/// </summary>
		/// <param name="actorCompanyId">Company ID</param>
		/// <param name="prevSyncDate">Last sync or null if all employees should be returned</param>
		/// <returns>List of employees created or modified since last sync</returns>
		public List<TSEmployeeItem> SyncEmployee(int actorCompanyId, DateTime? prevSyncDate, int? timeTerminalId = null, bool? includeLastEmployeeGroup = false)
		{
			using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
			bool useAccountHierarchy = base.UseAccountHierarchyOnCompanyFromCache(entities, actorCompanyId);
			using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
			var employeeGroups = GetEmployeeGroupsFromCache(entitiesReadOnly, CacheConfig.Company(actorCompanyId));
			if (prevSyncDate.HasValue)
				prevSyncDate = prevSyncDate.Value.AddHours(-2);

			entities.Employee.NoTracking();
			IQueryable<Employee> query = (from e in entities.Employee.Include("ContactPerson").Include("User").Include("Employment.EmploymentChangeBatch.EmploymentChange")
										  where e.ActorCompanyId == actorCompanyId &&
										  !e.Hidden &&
										  e.State < (int)SoeEntityState.Temporary
										  select e);

			if (prevSyncDate.HasValue)
			{
				query = query.Where(e =>
					(e.Modified.HasValue && e.Modified.Value > prevSyncDate.Value) ||
					(e.Created.HasValue && e.Created.Value > prevSyncDate.Value) ||
					(!e.Created.HasValue && !e.Modified.HasValue));
			}

			if (timeTerminalId.HasValue)
			{
				List<int> employeeIds = null;
				if (useAccountHierarchy && GetTimeTerminalBoolSetting(TimeTerminalSettingType.LimitTimeTerminalToAccount, timeTerminalId.Value))
					employeeIds = GetEmployeeIdsByTimeTerminalAccount(actorCompanyId, timeTerminalId.Value);
				else if (!useAccountHierarchy && GetTimeTerminalBoolSetting(TimeTerminalSettingType.LimitTimeTerminalToCategories, timeTerminalId.Value))
					employeeIds = this.GetEmployeeIdsByTimeTerminalCategory(actorCompanyId, timeTerminalId.Value);

				if (employeeIds != null)
					query = query.Where(tse => employeeIds.Contains(tse.EmployeeId));
			}

			// Execute query
			List<Employee> employees = query.ToList();

			var syncEmployees = (from e in employees
								 where (e.HasCurrentEmployeeGroupAutogenTimeblocks(employeeGroups) == false || (includeLastEmployeeGroup == true && e.GetCurrentEmployeeGroup(employeeGroups) == null && e.GetLastEmployeeGroup(employeeGroups) != null && !e.GetLastEmployeeGroup(employeeGroups).AutogenTimeblocks))
								 select new TSEmployeeItem()
								 {
									 EmployeeId = e.EmployeeId,
									 EmployeeNr = e.EmployeeNr,
									 Name = e.ContactPerson.FirstName + " " + e.ContactPerson.LastName,
									 EmployeeGroupId = e.GetCurrentEmployeeGroupId(employeeGroups),
									 SocialSec = "*****", // StringUtility.SocialSecYYMMDD_Dash_Stars(e.ContactPerson.SocialSec),
									 CardNumber = e.CardNumber,
									 FirstName = e.ContactPerson.FirstName,
									 LastName = e.ContactPerson.LastName,
									 EMail = e.User?.Email,
									 Created = e.Created,
									 Modified = e.Modified,
									 State = e.State
								 }).ToList();

			// Add employees that only have future employment.
			foreach (var employee in employees)
			{
				if (syncEmployees.Select(s => s.EmployeeId).Contains(employee.EmployeeId))
					continue;

				EmployeeGroup employeeGroup = employee.GetNextEmployeeGroup(DateTime.Today, employeeGroups);
				if (employeeGroup == null || employeeGroup.AutogenTimeblocks)
					continue;

				syncEmployees.Add(new TSEmployeeItem()
				{
					EmployeeId = employee.EmployeeId,
					EmployeeNr = employee.EmployeeNr,
					Name = employee.ContactPerson.FirstName + " " + employee.ContactPerson.LastName,
					EmployeeGroupId = employee.GetCurrentEmployeeGroupId(employeeGroups),
					SocialSec = employee.ContactPerson.SocialSec,
					CardNumber = employee.CardNumber,
					FirstName = employee.ContactPerson.FirstName,
					LastName = employee.ContactPerson.LastName,
					EMail = employee.User?.Email,
					Created = employee.Created,
					Modified = employee.Modified,
					State = employee.State
				});

			}

			return syncEmployees;
		}

		public List<int> GetEmployeeIdsForTerminal(int actorCompanyId, int timeTerminalId, DateTime? date = null)
		{
			List<int> employeeIds = null;
			if (GetTimeTerminalBoolSetting(TimeTerminalSettingType.LimitTimeTerminalToCategories, timeTerminalId))
				employeeIds = GetEmployeeIdsByTimeTerminalCategory(actorCompanyId, timeTerminalId, dateFrom: date, dateTo: date);
			else if (GetTimeTerminalBoolSetting(TimeTerminalSettingType.LimitTimeTerminalToAccount, timeTerminalId))
				employeeIds = GetEmployeeIdsByTimeTerminalAccount(actorCompanyId, timeTerminalId, dateFrom: date, dateTo: date);
			return employeeIds;
		}

		public bool IsEmployeeConnectedToTimeTerminal(int actorCompanyId, int timeTerminalId, int employeeId, DateTime? date = null, bool useCache = true)
		{
			using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
			return IsEmployeeConnectedToTimeTerminal(entities, actorCompanyId, timeTerminalId, employeeId, date, useCache);
		}

		public bool IsEmployeeConnectedToTimeTerminal(CompEntities entities, int actorCompanyId, int timeTerminalId, int employeeId, DateTime? date = null, bool useCache = true)
		{
			if (IsTimeTerminalLimitToCategoriesCached(entities, actorCompanyId, timeTerminalId))
				return IsEmployeeConnectedToTimeTerminalByCategory(entities, actorCompanyId, timeTerminalId, employeeId, dateFrom: date, dateTo: date);
			else if (IsTimeTerminalLimitToAccountCached(entities, actorCompanyId, timeTerminalId))
				return IsEmployeeConnectedToTimeTerminalByAccount(entities, actorCompanyId, timeTerminalId, employeeId, dateFrom: date, dateTo: date, useCache);
			else
				return true;
		}

		public bool IsTimeTerminalLimitToAccountCached(CompEntities entities, int actorCompanyId, int timeTerminalId)
		{
			if (!base.UseAccountHierarchyOnCompanyFromCache(entities, actorCompanyId))
				return false;

			var key = $"IsTimeTerminalLimitToAccount{actorCompanyId}#{timeTerminalId}#{ConfigurationSetupUtil.GetCurrentSysCompDbId()}";
			var cached = BusinessMemoryCache<bool?>.Get(key);

			if (cached.HasValue)
				return cached.Value;

			var fromDb = GetTimeTerminalBoolSetting(entities, TimeTerminalSettingType.LimitTimeTerminalToAccount, timeTerminalId);

			if (fromDb)
				BusinessMemoryCache<bool?>.Set(key, true, 60 * 15);
			else
				BusinessMemoryCache<bool?>.Set(key, false, 60 * 1);

			return fromDb;
		}

		public bool IsTimeTerminalLimitToCategoriesCached(CompEntities entities, int actorCompanyId, int timeTerminalId)
		{
			if (base.UseAccountHierarchyOnCompanyFromCache(entities, actorCompanyId))
				return false;

			var key = $"IsTimeTerminalLimitToCategories{actorCompanyId}#{timeTerminalId}#{ConfigurationSetupUtil.GetCurrentSysCompDbId()}";
			var cached = BusinessMemoryCache<bool?>.Get(key);
			if (cached.HasValue)
				return cached.Value;

			var nbrOfTerminals = entities.TimeTerminal.Count(s => s.ActorCompanyId == actorCompanyId && s.State == (int)SoeEntityState.Active);

			if (nbrOfTerminals == 1)
			{
				BusinessMemoryCache<bool?>.Set(key, false, 60 * 20);
				return false;
			}

			var fromDb = GetTimeTerminalBoolSetting(entities, TimeTerminalSettingType.LimitTimeTerminalToCategories, timeTerminalId);

			BusinessMemoryCache<bool?>.Set(key, fromDb, 60 * 5);

			return fromDb;
		}

		public List<int> GetEmployeeIdsByTimeTerminalAccount(int actorCompanyId, int timeTerminalId, DateTime? dateFrom = null, DateTime? dateTo = null)
		{
			dateFrom = dateFrom ?? DateTime.Today;
			dateTo = dateTo ?? DateTime.Today;
			List<int> accountIds = GetAccountIdsByTimeTerminal(timeTerminalId);
			List<int> employeeIds = EmployeeManager.GetEmployeeIdsByAccount(actorCompanyId, accountIds, dateFrom: dateFrom, dateTo: dateTo);
			return employeeIds;
		}

		public List<int> GetEmployeeIdsByTimeTerminalCategory(int actorCompanyId, int timeTerminalId, DateTime? dateFrom = null, DateTime? dateTo = null)
		{
			dateFrom = dateFrom ?? DateTime.Today;
			dateTo = dateTo ?? DateTime.Today;
			List<int> categoryIds = GetCategoriesByTimeTerminal(actorCompanyId, timeTerminalId).Select(e => e.CategoryId).ToList();
			using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
			List<int> employeeIds = EmployeeManager.GetEmployeeIdsByCategoryIds(entitiesReadOnly, actorCompanyId, categoryIds, dateFrom: dateFrom, dateTo: dateTo, onlyActive: false);
			return employeeIds;
		}

		public bool IsEmployeeConnectedToTimeTerminalByAccount(CompEntities entities, int actorCompanyId, int timeTerminalId, int employeeId, DateTime? dateFrom = null, DateTime? dateTo = null, bool useCache = false)
		{
			dateFrom = dateFrom ?? DateTime.Today;
			dateTo = dateTo ?? DateTime.Today;
			List<int> accountIds = GetCachedAccountIdsByTimeTerminal(entities, timeTerminalId, useCache);
			List<int> employeeAccountIs = EmployeeManager.GetCachedEmployeeAccountIds(entities, actorCompanyId, employeeId, dateFrom: dateFrom, dateTo: dateTo, useCache);
			return employeeAccountIs.Intersect(accountIds).Any();
		}

		public bool IsEmployeeConnectedToTimeTerminalByCategory(CompEntities entities, int actorCompanyId, int timeTerminalId, int employeeId, DateTime? dateFrom = null, DateTime? dateTo = null)
		{
			dateFrom = dateFrom ?? DateTime.Today;
			dateTo = dateTo ?? DateTime.Today;
			List<int> categoryIds = GetCategoriesByTimeTerminal(entities, actorCompanyId, timeTerminalId).Select(e => e.CategoryId).ToList();
			List<int> employeeCategoryIds = EmployeeManager.GetEmployeeCategoryIds(entities, employeeId, actorCompanyId, onlyDefaultCategories: false, dateFrom: dateFrom, dateTo: dateTo);
			return employeeCategoryIds.Intersect(categoryIds).Any();
		}

		public ActionResult SaveEmployeeFromTimeStamp(int actorCompanyId, IEmployeeUserBasic employeeItem, int? timeTerminalId = null, bool allowUpdatingEmployee = true, int? categorieId = null)
		{
			using (var entities = new CompEntities())
			{
				return this.SaveEmployeeFromTimeStamp(entities, actorCompanyId, employeeItem, timeTerminalId, allowUpdatingEmployee);
			}
		}

		public ActionResult SaveEmployeeFromTimeStamp(CompEntities entities, int actorCompanyId, IEmployeeUserBasic employeeItem, int? timeTerminalId = null, bool allowUpdatingEmployee = true, int? categorieId = null)
		{
			if (employeeItem.EmployeeId > 0 && timeTerminalId.HasValue && !allowUpdatingEmployee)
			{
				// This is the old behavior, when you could not update an employee then employeeId was used as an indicator that the category should be updated. 
				// Remove when no versions under 1.3 exists anymore
				return AddEmployeeToTerminal(actorCompanyId, timeTerminalId.Value, employeeItem.EmployeeId);
			}

			int employeeGroupId = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.TimeDefaultEmployeeGroup, 0, actorCompanyId, 0);
			EmployeeGroup employeeGroup = EmployeeManager.GetEmployeeGroup(employeeGroupId);
			if (employeeGroup == null)
				return new ActionResult(false, (int)ActionResultSave.EntityIsNull, "Standard employee group");
			else if (employeeGroup.AutogenTimeblocks)// Time terminal must have AutogenTimeblocks set to false
				return new ActionResult(false, (int)ActionResultSave.EmployeeGroupAutogenTimeBlocksIsTrue, "Time terminal must have AutogenTimeblocks set to false");

			var dto = new EmployeeUserDTO()
			{
				SaveEmployee = true,
				SaveUser = false,
				DefaultActorCompanyId = actorCompanyId,
				EmployeeNr = employeeItem.EmployeeNr,
				FirstName = employeeItem.FirstName,
				LastName = employeeItem.LastName,
				CardNumber = employeeItem.CardNumber,
				SocialSec = employeeItem.SocialSec,
				Sex = (int)TermGroup_Sex.Unknown,
				CurrentEmployeeGroupId = employeeGroupId,

				//Set FK
				EmployeeId = employeeItem.EmployeeId,
				ActorCompanyId = actorCompanyId,
			};

			var result = ActorManager.SaveEmployeeUserFromTerminal(actorCompanyId, dto, null);
			if (result.Success && result.IntDict.TryGetValue((int)SaveEmployeeUserResult.EmployeeId, out int employeeId))
			{
				Employee employee = EmployeeManager.GetEmployeeIgnoreState(actorCompanyId, employeeId);
				if (employee != null)
				{
					if (timeTerminalId.HasValue && GetTimeTerminalBoolSetting(TimeTerminalSettingType.LimitTimeTerminalToCategories, timeTerminalId.Value))
					{
						// Category
						Category category = GetCategoriesByTimeTerminal(actorCompanyId, timeTerminalId.Value).FirstOrDefault();
						if (category == null)
							return new ActionResult((int)ActionResultSave.EntityNotFound, "Category");

						result = CategoryManager.AddCompanyCategoryRecord(employee.EmployeeId, category.CategoryId, SoeCategoryRecordEntity.Employee, actorCompanyId);
					}

					result.IntegerValue = employeeId;
					result.IntegerValue2 = employeeGroupId;
				}
			}
			else if (!result.Success && result.ErrorNumber == (int)ActionResultSave.EmployeeNumberExists)
			{
				result.StringValue = EmployeeManager.GetNextEmployeeNr(actorCompanyId);
				result.ErrorMessage = String.Format(GetText(5882, "Anställningsnumret '{0}' är upptaget"), result.StringValue);
			}

			return result;
		}

		#endregion

		#region EmployeeGroup

		/// <summary>
		/// Synchronize employee groups (get modifications)
		/// </summary>
		/// <param name="actorCompanyId">Company ID</param>
		/// <param name="prevSyncDate">Last sync or null if all employee groups should be returned</param>
		/// <returns>List of employee groups created or modified since last sync</returns>
		public IEnumerable<TSEmployeeGroupItem> SyncEmployeeGroup(int actorCompanyId, DateTime? prevSyncDate)
		{
			using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
			entities.EmployeeGroup.NoTracking();
			return (from eg in entities.EmployeeGroup
					where eg.ActorCompanyId == actorCompanyId &&
					!eg.AutogenTimeblocks
					orderby eg.State ascending
					select new TSEmployeeGroupItem()
					{
						EmployeeGroupId = eg.EmployeeGroupId,
						Name = eg.Name,
						Created = eg.Created,
						Modified = eg.Modified,
						State = eg.State
					}).ToList();
		}

		#endregion

		#region EmployeeSchedule

		/// <summary>
		/// Synchronize employee placements (get modifications) within specified date period
		/// </summary>
		/// <param name="actorCompanyId">Company ID</param>
		/// <param name="accountDimId">ID of account dim for the internal account to receive</param>
		/// <param name="startDate">Date range start</param>
		/// <param name="stopDate">Date range stop</param>
		/// <param name="prevSyncDate">Last sync or null if all schedules should be returned</param>
		/// <param name="timeTerminalId">If specifed then the query only fetches employeeschedules for this terminal id.</param>
		/// <param name="fetchAll">Temporary parameter, set this to true if terminal version is less then 1.2. Might crash if true.</param>
		/// <returns>List of schedule blocks created or modified since last sync. AllItemsFetched is set to false if there are more items.</returns>
		public TSSyncEmployeeScheduleResult SyncEmployeeSchedule(int actorCompanyId, int accountDimId, DateTime startDate, DateTime stopDate, DateTime? prevSyncDate, int? timeTerminalId = null, bool fetchAll = false)
		{
			var result = new TSSyncEmployeeScheduleResult() { Items = new List<TSEmployeeScheduleItem>() };

			// First get all deleted blocks ordered by BreakType decending so that break blocks will be deleted before their parent blocks in the terminal.
			using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
			entities.TimeScheduleTemplateBlock.NoTracking();
			var query = (from tb in entities.TimeScheduleTemplateBlock
							.Include("AccountInternal.Account")
						 where tb.Employee.ActorCompanyId == actorCompanyId &&
						 (tb.Date >= startDate && tb.Date <= stopDate) &&
						 !tb.IsPreliminary &&
						 (!prevSyncDate.HasValue ||
						  ((tb.Modified.HasValue && tb.Modified.Value > prevSyncDate.Value) ||
						   (!tb.Modified.HasValue && tb.Created.HasValue && tb.Created.Value > prevSyncDate.Value) ||
						   !tb.Created.HasValue))
						 select tb);

			if (timeTerminalId.HasValue && GetTimeTerminalBoolSetting(TimeTerminalSettingType.LimitTimeTerminalToCategories, timeTerminalId.Value))
			{
				List<int> employeeIds = GetEmployeeIdsByTimeTerminalCategory(actorCompanyId, timeTerminalId.Value, startDate, stopDate);
				query = query.Where(tst => tst.EmployeeId.HasValue && employeeIds.Contains(tst.EmployeeId.Value));
			}
			else if (timeTerminalId.HasValue && GetTimeTerminalBoolSetting(TimeTerminalSettingType.LimitTimeTerminalToAccount, timeTerminalId.Value))
			{
				List<int> employeeIds = GetEmployeeIdsByTimeTerminalAccount(actorCompanyId, timeTerminalId.Value, startDate, stopDate);
				query = query.Where(tst => tst.EmployeeId.HasValue && employeeIds.Contains(tst.EmployeeId.Value));
			}

			// Increase timeout for this call since it can be reached
			using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
			var oldTimeOut = entitiesReadOnly.CommandTimeout;
			entitiesReadOnly.CommandTimeout = 180; // 3 minutes

			List<TimeScheduleTemplateBlock> templateBlocks;
			if (fetchAll)
			{
				// This section exists in order to support older timestamps version < 1.2
				templateBlocks = query.ToList();
			}
			else
			{
				const int TAKE = 1000;
				templateBlocks = query.OrderBy(tse => tse.Modified == null ? tse.Created : tse.Modified).Take(TAKE).ToList();

				result.AllItemsFetched = templateBlocks.Count < TAKE;
			}
			entitiesReadOnly.CommandTimeout = oldTimeOut;

			foreach (TimeScheduleTemplateBlock templateBlock in templateBlocks.Where(w => !w.TimeScheduleScenarioHeadId.HasValue))
			{
				TSEmployeeScheduleItem item = CreateTSEmployeeScheduleItemFromBlock(templateBlock, accountDimId);
				if (item != null)
					result.Items.Add(item);
			}

			return result;
		}

		/// <summary>
		/// Synchronize employee placements (get modifications) within specified date period
		/// </summary>
		/// <param name="accountDimId">ID of account dim for the internal account to receive</param>
		/// <param name="employeeId">Employee ID</param>
		/// <param name="startDate">Date range start</param>
		/// <param name="stopDate">Date range stop</param>
		/// <returns>List of schedule blocks for specified employee</returns>
		public TSSyncEmployeeScheduleResult SyncOneEmployeeSchedule(int accountDimId, int employeeId, DateTime startDate, DateTime stopDate)
		{
			var result = new TSSyncEmployeeScheduleResult() { Items = new List<TSEmployeeScheduleItem>() };

			// First get all deleted blocks ordered by BreakType decending so that break blocks will be deleted before their parent blocks in the terminal.
			using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
			entities.TimeScheduleTemplateBlock.NoTracking();
			var query = (from tb in entities.TimeScheduleTemplateBlock
							.Include("AccountInternal.Account")
						 where tb.EmployeeId == employeeId &&
						 (tb.Date >= startDate && tb.Date <= stopDate) &&
						 !tb.IsPreliminary
						 select tb);

			List<TimeScheduleTemplateBlock> templateBlocks = query.ToList();

			foreach (TimeScheduleTemplateBlock templateBlock in templateBlocks.Where(w => !w.TimeScheduleScenarioHeadId.HasValue))
			{
				TSEmployeeScheduleItem item = CreateTSEmployeeScheduleItemFromBlock(templateBlock, accountDimId);
				if (item != null)
					result.Items.Add(item);
			}

			return result;
		}

		private TSEmployeeScheduleItem CreateTSEmployeeScheduleItemFromBlock(TimeScheduleTemplateBlock templateBlock, int accountDimId)
		{
			TSEmployeeScheduleItem item = null;

			try
			{
				// Get internal account for specified account dim
				int accountId = 0;
				AccountInternal accInt = templateBlock.AccountInternal.FirstOrDefault(a => a.Account.AccountDimId == accountDimId);
				if (accInt != null)
					accountId = accInt.AccountId;

				// Handle shifts over midnight
				int addDays = (int)(templateBlock.StopTime.Date - templateBlock.StartTime.Date).TotalDays;

				item = new TSEmployeeScheduleItem()
				{
					TimeScheduleTemplateBlockId = templateBlock.TimeScheduleTemplateBlockId,
					TimeScheduleTemplatePeriodId = templateBlock.TimeScheduleTemplatePeriodId,
					TimeScheduleEmployeePeriodId = templateBlock.TimeScheduleEmployeePeriodId,
					TimeCodeId = templateBlock.TimeCodeId,
					EmployeeId = templateBlock.EmployeeId ?? 0,
					AccountId = accountId,
					StartTime = new DateTime(templateBlock.Date.Value.Year, templateBlock.Date.Value.Month, templateBlock.Date.Value.Day, templateBlock.StartTime.Hour, templateBlock.StartTime.Minute, templateBlock.StartTime.Second),
					StopTime = new DateTime(templateBlock.Date.Value.Year, templateBlock.Date.Value.Month, templateBlock.Date.Value.Day, templateBlock.StopTime.Hour, templateBlock.StopTime.Minute, templateBlock.StopTime.Second).AddDays(addDays),
					NoSchedule = false,
					Created = templateBlock.Created,
					Modified = templateBlock.Modified,
					BreakType = templateBlock.BreakType,
					State = templateBlock.State
				};
			}
			catch (Exception ex)
			{
				ex.ToString(); //prevent compiler warning
			}

			return item;
		}

		#endregion

		#region TimeAccumulator

		public IEnumerable<TSTimeAccumulatorEmployeeItem> GetTimeAccumulator(int actorCompanyId, int employeeId, int employeeGroupId, DateTime startDate, DateTime stopDateTime, int timeTerminalId = 0)
		{
			DateTime syncDate = DateTime.Now;
			SysScheduledJobManager.BackupRunJobs();
			List<TSTimeAccumulatorEmployeeItem> accumulators = new List<TSTimeAccumulatorEmployeeItem>();
			List<TimeAccumulatorItem> items = new List<TimeAccumulatorItem>();

			// Get accumulator items for specified employee within specified date interval if allAccs
			bool allAccs = (timeTerminalId == 0 || !GetTimeTerminalBoolSetting(TimeTerminalSettingType.ShowOnlyBreakAcc, timeTerminalId));
			if (allAccs)
			{
				GetTimeAccumulatorItemsInput timeAccInput = GetTimeAccumulatorItemsInput.CreateInput(actorCompanyId, 0, employeeId, startDate.Date, stopDateTime.Date, calculateDay: true, calculatePeriod: true, calculateYear: true, calculateAccToday: true, calculatePlanningPeriod: true);
				items.AddRange(TimeAccumulatorManager.GetTimeAccumulatorItems(timeAccInput));
			}

			// Add break accumulator for today only (stopDate)
			items.Add(TimeAccumulatorManager.GetBreakTimeAccumulatorItem(actorCompanyId, stopDateTime, employeeId, employeeGroupId));

			// Convert to sync DTO to reduce data to send
			foreach (var item in items)
			{
				TSTimeAccumulatorEmployeeItem accumulator = new TSTimeAccumulatorEmployeeItem()
				{
					TimeAccumulatorId = item.TimeAccumulatorId,
					EmployeeId = employeeId,
					Name = item.Name,
					SumToday = item.SumToday,
					SumPeriod = item.SumPeriod,
					SumAccToday = item.SumAccToday,
					SumYear = item.SumYear,
					SyncDate = syncDate
				};
				accumulators.Add(accumulator);
			}

			return accumulators;
		}

		#endregion

		#region TimeCode

		/// <summary>
		/// Synchronize time codes (get modifications)
		/// </summary>
		/// <param name="actorCompanyId">Company ID</param>
		/// <param name="prevSyncDate">Last sync or null if all time codes should be returned</param>
		/// <returns>List of time codes created or modified since last sync</returns>
		public IEnumerable<TSTimeCodeItem> SyncTimeCode(int actorCompanyId, DateTime? prevSyncDate)
		{
			int typeBreak = (int)SoeTimeCodeType.Break;

			// Get Work and Absense time codes
			IEnumerable<TimeCode> query;
			using var entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
			entitiesReadOnly.TimeCode.NoTracking();
			query = (from t in entitiesReadOnly.TimeCode
					 where t.ActorCompanyId == actorCompanyId &&
					 t.Type != typeBreak &&
					 (!prevSyncDate.HasValue ||
					 ((t.Modified.HasValue && t.Modified.Value > prevSyncDate.Value) ||
					  (!t.Modified.HasValue && t.Created.HasValue && t.Created.Value > prevSyncDate.Value) ||
					  !t.Created.HasValue))
					 select t).ToList();

			// Get Break time codes
			IEnumerable<TimeCodeBreak> breakQuery;
			entitiesReadOnly.TimeCode.OfType<TimeCodeBreak>().NoTracking();
			breakQuery = (from t in entitiesReadOnly.TimeCode.OfType<TimeCodeBreak>()
						  where t.ActorCompanyId == actorCompanyId &&
						  (!prevSyncDate.HasValue ||
						  ((t.Modified.HasValue && t.Modified.Value > prevSyncDate.Value) ||
							 (!t.Modified.HasValue && t.Created.HasValue && t.Created.Value > prevSyncDate.Value) ||
							 !t.Created.HasValue))
						  select t).ToList();

			// Convert time codes to TimeCodeItems
			List<TSTimeCodeItem> items = new List<TSTimeCodeItem>();
			TSTimeCodeItem item;
			foreach (TimeCode timeCode in query)
			{
				item = new TSTimeCodeItem()
				{
					TimeCodeId = timeCode.TimeCodeId,
					Type = timeCode.Type,
					Name = timeCode.Name,
					Created = timeCode.Created ?? DateTime.Now,
					Modified = timeCode.Modified ?? DateTime.Now,
					State = timeCode.State
				};
				items.Add(item);
			}
			foreach (TimeCodeBreak timeCode in breakQuery)
			{
				item = new TSTimeCodeItem()
				{
					TimeCodeId = timeCode.TimeCodeId,
					Type = timeCode.Type,
					Name = timeCode.Name,
					BreakMinMinutes = timeCode.MinMinutes,
					BreakMaxMinutes = timeCode.MaxMinutes,
					BreakDefaultMinutes = timeCode.DefaultMinutes,
					BreakStartType = timeCode.StartType,
					BreakStopType = timeCode.StopType,
					BreakStartTimeMinutes = timeCode.StartTimeMinutes,
					BreakStopTimeMinutes = timeCode.StopTimeMinutes,
					Created = timeCode.Created ?? DateTime.Now,
					Modified = timeCode.Modified ?? DateTime.Now,
					State = timeCode.State
				};
				items.Add(item);
			}

			return items;
		}

		#endregion

		#region TimeDeviationCause

		/// <summary>
		/// Synchronize deviation causes (get modifications)
		/// </summary>
		/// <param name="actorCompanyId">Company ID</param>
		/// <param name="prevSyncDate">Last sync or null if all deviation causes should be returned</param>
		/// <returns>List of deviation causes created or modified since last sync</returns>
		public IEnumerable<TSTimeDeviationCauseItem> SyncDeviationCause(int actorCompanyId, DateTime? prevSyncDate)
		{
			List<TSTimeDeviationCauseItem> items = new List<TSTimeDeviationCauseItem>();

			//// Get all deviation causes, created/modified will be checked below
			using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
			entities.TimeDeviationCause.NoTracking();
			List<TimeDeviationCause> timeDeviationCauses = (from t in entities.TimeDeviationCause.Include("EmployeeGroupTimeDeviationCause.EmployeeGroup")
															where t.ActorCompanyId == actorCompanyId &&
															!t.OnlyWholeDay
															select t).ToList();

			foreach (TimeDeviationCause timeDeviationCause in timeDeviationCauses)
			{
				// If no prev sync date, always sync
				bool isModified = !prevSyncDate.HasValue;
				if (!isModified)
				{
					// Check created/modified on cause
					if (((timeDeviationCause.Modified.HasValue && timeDeviationCause.Modified.Value > prevSyncDate.Value) || (!timeDeviationCause.Modified.HasValue && timeDeviationCause.Created.HasValue && timeDeviationCause.Created.Value > prevSyncDate.Value) || !timeDeviationCause.Created.HasValue))
						isModified = true;

					if (!isModified)
					{
						// Check created/modified on related employee groups
						foreach (EmployeeGroup employeeGroup in timeDeviationCause.EmployeeGroupTimeDeviationCause.Where(i => i.UseInTimeTerminal && i.State == (int)SoeEntityState.Active).Select(i => i.EmployeeGroup))
						{
							if (((employeeGroup.Modified.HasValue && employeeGroup.Modified.Value > prevSyncDate.Value) || (!employeeGroup.Modified.HasValue && employeeGroup.Created.HasValue && employeeGroup.Created.Value > prevSyncDate.Value) || !employeeGroup.Created.HasValue))
							{
								isModified = true;
								break;
							}
						}
					}
				}

				if (!isModified)
					continue;

				TSTimeDeviationCauseItem item = new TSTimeDeviationCauseItem()
				{
					TimeDeviationCauseId = timeDeviationCause.TimeDeviationCauseId,
					Type = timeDeviationCause.Type,
					Name = timeDeviationCause.Name,
					Created = timeDeviationCause.Created,
					Modified = timeDeviationCause.Modified,
					State = timeDeviationCause.State
				};

				item.EmployeeGroupIds = new List<int>();
				foreach (EmployeeGroupTimeDeviationCause employeeGroupTimeDeviationCause in timeDeviationCause.EmployeeGroupTimeDeviationCause.Where(i => i.UseInTimeTerminal && i.State == (int)SoeEntityState.Active))
				{
					item.EmployeeGroupIds.Add(employeeGroupTimeDeviationCause.EmployeeGroupId);
				}

				items.Add(item);
			}

			return items;
		}

		#endregion

		#region SysTerm

		/// <summary>
		/// Gets all systerms modified after prevSyncDate parameter.
		/// </summary>
		/// <param name="timeTerminalId">Uses timeterminalid to fetch the correct language.</param>
		/// <param name="actorCompanyId">The actor company id.</param>
		/// <param name="tsSysCountryId">Is only used when actorCompanyId is not specified.</param>
		/// <param name="termGroupIds">The termgroups to look inside.</param>
		/// <param name="prevSyncDate">If null then will sync all terms in one language.</param>
		/// <returns></returns>
		public List<SysTermDTO> GetSysTerms(int timeTerminalId, int actorCompanyId, int? tsSysCountryId, IEnumerable<int> termGroupIds, DateTime? prevSyncDate)
		{
			using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
			// Get country id by 1: looking in timeterminal settings, 2: looking at company country, 3: Swedish as default
			int sysCountryId = (int)TermGroup_Languages.Swedish;
			if (timeTerminalId > 0)
			{
				var setting = GetTimeTerminalSetting(TimeTerminalSettingType.SysCountryId, timeTerminalId);
				if (setting != null && setting.IntData.HasValue && setting.IntData > 0)
					sysCountryId = setting.IntData.Value;
				else
					sysCountryId = CountryCurrencyManager.GetSysCountryIdFromCompany(actorCompanyId) ?? 0;
			}
			else if (tsSysCountryId.HasValue && actorCompanyId <= 0)
			{
				// If actorCompanyId is not specified the terminal is not yet registred so use terminal os language
				sysCountryId = tsSysCountryId.Value;
			}

			if (!tsSysCountryId.HasValue || tsSysCountryId != sysCountryId || !prevSyncDate.HasValue)
			{
				// If syscountry has changed then sync all;
				return (from t in sysEntitiesReadOnly.SysTerm
						where
						termGroupIds.Contains(t.SysTermGroupId) &&
						t.LangId == sysCountryId
						select t).ToDTOs().ToList();
			}

			var systerms = (from t in sysEntitiesReadOnly.SysTerm
							where
							termGroupIds.Contains(t.SysTermGroupId) &&
							t.LangId == sysCountryId &&
							(
								(t.Modified.HasValue && t.Modified.Value > prevSyncDate) ||
								(!t.Modified.HasValue && t.Created.HasValue && t.Created.Value > prevSyncDate) ||
								!t.Created.HasValue
							)
							select t).ToList();

			return systerms.ToDTOs();

		}

		#endregion

		#region TimeStampEntry

		/// <summary>
		/// Synchronize time stamp entries (from other terminals)
		/// </summary>
		/// <param name="actorCompanyId">Company ID</param>
		/// <param name="prevSyncDate">Last sync</param>
		/// <returns>List of entries created since last sync</returns>
		public List<TSTimeStampEntryItem> SyncTimeStampEntry(int actorCompanyId, DateTime prevSyncDate, int? terminalId = null)
		{
			using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
			entities.TimeStampEntry.NoTracking();
			var query = (from tse in entities.TimeStampEntry
							.Include("Employee.Employment")
						 where tse.Type != (int)TimeStampEntryType.Unknown &&
						 tse.Company.ActorCompanyId == actorCompanyId &&
						 ((tse.Modified.HasValue && tse.Modified.Value > prevSyncDate) ||
						  (!tse.Modified.HasValue && tse.Created.HasValue && tse.Created.Value > prevSyncDate) ||
						   !tse.Created.HasValue && !tse.Modified.HasValue)
						 select tse);

			if (terminalId.HasValue)
			{
				// Do not sync time stamps to other terminals (company setting)
				bool limitAttendanceView = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.LimitAttendanceViewToStampedTerminal, 0, actorCompanyId, 0);
				if (limitAttendanceView)
				{
					query = query.Where(tse => tse.TimeTerminalId == terminalId.Value);
				}

				// If prevSyncDate is yesterday (exactly), it is a new empty terminal
				// In that case we should ignore the following condition
				if (prevSyncDate != DateTime.Today.AddDays(-1))
				{
					// Do not sync back the ones that come from the terminal itself (except if it has been edited)
					query = query.Where(tse => (!tse.TimeTerminalId.HasValue || tse.TimeTerminalId != terminalId || (tse.Modified.HasValue && tse.Modified.Value > prevSyncDate)));
				}

				// Limit the query to only the employees that the timestamp is using
				if (GetTimeTerminalBoolSetting(TimeTerminalSettingType.LimitTimeTerminalToCategories, terminalId.Value))
				{
					List<int> employeeIds = GetEmployeeIdsByTimeTerminalCategory(actorCompanyId, terminalId.Value);
					query = query.Where(tse => employeeIds.Contains(tse.EmployeeId));
				}
				else if (GetTimeTerminalBoolSetting(TimeTerminalSettingType.LimitTimeTerminalToAccount, terminalId.Value))
				{
					List<int> employeeIds = GetEmployeeIdsByTimeTerminalAccount(actorCompanyId, terminalId.Value);
					query = query.Where(tse => employeeIds.Contains(tse.EmployeeId));
				}
			}

			// Increase timeout for this call since it can be reached
			using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
			var oldTimeOut = entitiesReadOnly.CommandTimeout;
			entitiesReadOnly.CommandTimeout = 180; // 3 minutes
			List<TimeStampEntry> timeStampEntries = query.OrderBy(tse => tse.Modified == null ? tse.Created : tse.Modified).Take(2000).ToList();
			entitiesReadOnly.CommandTimeout = oldTimeOut;

			return (from tse in timeStampEntries
					// where tse.Employee.CurrentEmployeeGroup.AutogenTimeblocks == false
					select new TSTimeStampEntryItem()
					{
						TimeStampEntryId = tse.TimeStampEntryId,
						EmployeeId = tse.EmployeeId,
						TimeDeviationCauseId = tse.TimeDeviationCauseId,
						AccountId = tse.AccountId,
						TimeScheduleTemplatePeriodId = tse.TimeScheduleTemplatePeriodId,
						Type = tse.Type,
						Time = tse.Time,
						Created = tse.Created,
						Modified = tse.Modified,
						Status = tse.Status,
						State = tse.State
					}).ToList();
		}

		/// <summary>
		/// Get one TimeStampEntry
		/// </summary>
		/// <param name="entities">Object context</param>
		/// <param name="timeStampEntryId">TimeStampEntry ID</param>
		/// <param name="onlyActive">If true, entry is only returned if it is active</param>
		/// <param name="loadRelations">If true, foreign key relations are loaded</param>
		/// <returns>One TimeStampEntry or null if no TimeStampEntry found</returns>
		public TimeStampEntry GetTimeStampEntry(CompEntities entities, int timeStampEntryId, bool onlyActive, bool loadRelations)
		{
			TimeStampEntry entry = (from t in entities.TimeStampEntry
									where t.TimeStampEntryId == timeStampEntryId
									select t).FirstOrDefault();
			if (entry != null)
			{
				if (onlyActive && entry.State != (int)SoeEntityState.Active)
					return null;

				if (loadRelations)
					LoadTimeStampEntry(entities, entry, true, true, true);
			}

			return entry;
		}

		public TimeStampEntry GetTimeStampEntry(int timeStampEntryId)
		{
			using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
			entities.TimeStampEntry.NoTracking();
			TimeStampEntry entry = (from t in entities.TimeStampEntry.Include("TimeTerminal")
									where t.TimeStampEntryId == timeStampEntryId
									select t).FirstOrDefault();

			return entry;
		}

		public List<TimeSpotTimeStampView> GetTimeSpotTimeStampForEmployee(int employeeId, DateTime sinceDate, int quantity)
		{
			using (CompEntities entities = new CompEntities())
			{

				List<TimeSpotTimeStampView> entries = (from t in entities.TimeSpotTimeStampView
													   where t.EmployeeId == employeeId &&
													   t.Changed >= sinceDate
													   select t).Take(quantity).OrderByDescending(c => c.Time).ToList();

				return entries;
			}
		}

		public List<TimeSpotEmployeeView> GetTimeSpotEmployeeForCompany(int actorCompanyId)
		{
			using (CompEntities entities = new CompEntities())
			{
				List<TimeSpotEmployeeView> employees = (from t in entities.TimeSpotEmployeeView
														where t.ActorCompanyId == actorCompanyId
														select t).ToList();

				return employees;
			}
		}

		public List<TimeSpotTimeStampView> GetTimeSpotTimeStampForCompany(int actorCompanyId, DateTime sinceDate)
		{
			using (CompEntities entities = new CompEntities())
			{

				List<TimeSpotTimeStampView> entries = (from t in entities.TimeSpotTimeStampView
													   where t.ActorCompanyId == actorCompanyId &&
													   t.Changed >= sinceDate
													   select t).ToList();

				return entries;
			}
		}

		public List<TimeSpotTimeCodeView> GetTimeSpotTimeCodeForCompany(int actorCompanyId)
		{
			using (CompEntities entities = new CompEntities())
			{

				List<TimeSpotTimeCodeView> timeCodes = (from t in entities.TimeSpotTimeCodeView
														where t.ActorCompanyId == actorCompanyId
														select t).ToList();

				return timeCodes;
			}
		}

		public List<TimeSpotTimeCodeViewForEmployee> GetTimeSpotTimeCodeForCompany(int actorCompanyId, DateTime lastSync)
		{
			using (CompEntities entities = new CompEntities())
			{
				List<TimeSpotTimeCodeViewForEmployee> timeCodes = (from t in entities.TimeSpotTimeCodeViewForEmployee
																   where t.ActorCompanyId == actorCompanyId &&
																   (t.Modified >= lastSync || t.Created >= lastSync)
																   select t).ToList();

				return timeCodes;
			}
		}

		public List<TimeSpotTimeCodeViewForEmployee> GetTimeSpotTimeCodeForEmployee(int actorCompanyId)
		{
			using (CompEntities entities = new CompEntities())
			{

				List<TimeSpotTimeCodeViewForEmployee> timeCodes = (from t in entities.TimeSpotTimeCodeViewForEmployee
																   where t.ActorCompanyId == actorCompanyId &&
																   t.Type == 2
																   select t).ToList();

				return timeCodes;
			}
		}

		public bool HasExtendedTimeStamps(CompEntities entities, int actorCompanyId)
		{
			var key = $"HasExtendedTimeStamps_{actorCompanyId}";

			var value = BusinessMemoryCache<bool?>.Get(key);

			if (value.HasValue)
				return value.Value;

			var hasExtended = entities.TimeStampEntryExtended.Any(a => a.TimeStampEntry.ActorCompanyId == actorCompanyId);

			BusinessMemoryCache<bool?>.Set(key, hasExtended, hasExtended ? 60 * 60 : 5 * 60);

			return hasExtended;
		}

		public List<TimeStampEntry> GetNewTimeStampEntries(CompEntities entities, int actorCompanyId, DateTime? toTime, bool loadEmployment = false, bool loadTimeTerminal = false, DateTime? after = null, bool loadExtended = false)
		{
			entities.CommandTimeout = entities.CommandTimeout.HasValue && entities.CommandTimeout.Value < 300 ? 300 : entities.CommandTimeout;

			if (!after.HasValue)
				after = DateTime.Now.AddMonths(-2);

			IQueryable<TimeStampEntry> query = entities.TimeStampEntry;
			if (toTime.HasValue)
				query = query.Where(ts => ts.Time < toTime.Value);
			if (loadEmployment)
				query = query.Include("Employee.Employment");
			if (loadTimeTerminal)
				query = query.Include("TimeTerminal");

			if (loadExtended)
				query = query.Include("TimeStampEntryExtended");

			// Sometimes stamps get stuck in status processing, therefore we include them as well.
			// Although in a later state we check if all stamps are in status processing, we skip that employee,
			// because then there might be another job running.
			List<TimeStampEntry> timeStampEntries = (from ts in query
													 where ts.ActorCompanyId == actorCompanyId &&
													 (ts.Status == (int)TermGroup_TimeStampEntryStatus.New || ts.Status == (int)TermGroup_TimeStampEntryStatus.Processing || ts.Status == (int)TermGroup_TimeStampEntryStatus.Partial) &&
													 ts.Time > after &&
													 ts.State == (int)SoeEntityState.Active
													 select ts).ToList();

			return timeStampEntries.OrderBy(ts => ts.EmployeeId).ThenBy(ts => ts.Time).ThenBy(ts => ts.TimeStampEntryId).ToList();
		}

		public List<int> GetCompanyIdsWithNewEntries(DateTime? after = null)
		{
			if (after == null)
				after = DateTime.Now.AddMonths(-2);
			using var entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
			return (from ts in entitiesReadOnly.TimeStampEntry
					where ts.Status == (int)TermGroup_TimeStampEntryStatus.New && ts.State == (int)SoeEntityState.Active && ts.Time > after
					select ts.ActorCompanyId).Distinct().OrderBy(o => o).ToList();
		}

		/// <summary>
		/// Synchronise TimeStampEntries
		/// </summary>
		/// <param name="timeStampEntryItems">Collection of new entries since last sync</param>
		/// <param name="timeTerminalId">Terminal ID</param>
		/// <param name="accountDimId">Account dim ID</param>
		/// <param name="actorCompanyId">Company ID</param>
		/// <returns>Dictionary where Key = TimeStampEntryInternalId and Value = TimeStampEntryId</returns>
		public Dictionary<int, int> SyncNewTimeStampEntries(List<TSTimeStampEntryItem> timeStampEntryItems, int timeTerminalId, int accountDimId, int actorCompanyId, int userId)
		{
			return TimeEngineManager(actorCompanyId, userId).SynchTimeStamps(timeStampEntryItems, timeTerminalId, accountDimId);
		}

		public ActionResult CreateFakeTimeStamps(int actorCompanyId, DateTime dateFrom, DateTime dateTo)
		{
			try
			{
				List<EmployeeGroup> employeeGroups;
				List<PayrollGroup> payrollGroups;
				List<PayrollPriceType> payrollPriceTypes;
				List<Employee> employees;
				List<AccountDim> accountDims;
				AccountDim accountDim;
				List<TimeDeviationCause> timedeviationCodes;
				Random random = new Random();

				using (CompEntities entities = new CompEntities())
				{
					dateTo = dateTo.AddDays(1);
					employeeGroups = EmployeeManager.GetEmployeeGroups(entities, actorCompanyId, true, true, true, true);
					payrollGroups = PayrollManager.GetPayrollGroups(entities, actorCompanyId, true, true, loadSettings: true, loadAccountStd: true);
					payrollPriceTypes = PayrollManager.GetPayrollPriceTypes(entities, actorCompanyId, null, true);
					employees = EmployeeManager.GetAllEmployees(actorCompanyId, true, loadEmployment: true);
					timedeviationCodes = TimeDeviationCauseManager.GetTimeDeviationCauses(entities, actorCompanyId);
					accountDims = AccountManager.GetAccountDimsByCompany(actorCompanyId);
					accountDim = accountDims.FirstOrDefault(f => f.SysSieDimNr == (int)TermGroup_SieAccountDim.Department);
					if (accountDim == null)
						return new ActionResult(false);
				}

				var sickTimeDeviationCode = timedeviationCodes.FirstOrDefault(f => f.Name.ToLower().Contains("sjuk"));
				var standardTimeDeviationCode = timedeviationCodes.FirstOrDefault(f => f.Name.ToLower().Contains("standard"));

				if (sickTimeDeviationCode == null)
					sickTimeDeviationCode = timedeviationCodes.FirstOrDefault(f => f.Type == (int)TermGroup_TimeDeviationCauseType.Absence);

				if (standardTimeDeviationCode == null)
					standardTimeDeviationCode = timedeviationCodes.FirstOrDefault(f => f.Type == (int)TermGroup_TimeDeviationCauseType.PresenceAndAbsence);

				if (standardTimeDeviationCode == null || sickTimeDeviationCode == null)
					return new ActionResult(false);

				int count = 0;
				foreach (var employee in employees)
				{
					try
					{
						using (CompEntities entities = new CompEntities())
						{
							count++;
							var existingTimeStamps = entities.TimeStampEntry.Include("TimeBlockDate").Where(a => a.State == (int)SoeEntityState.Active && a.EmployeeId == employee.EmployeeId && a.ActorCompanyId == actorCompanyId && a.TimeBlockDate.Date >= dateFrom && a.TimeBlockDate.Date <= dateTo).ToList();
							var shifts = TimeScheduleManager.GetShifts(employee.EmployeeId, CalendarUtility.GetDatesInInterval(dateFrom, dateTo));

							if (!shifts.Any())
								continue;

							List<TSTimeStampEntryItem> entryItems = new List<TSTimeStampEntryItem>();

							var createTimeStampsOnAllBlocks = random.Next(1, 20) == 5;

							foreach (var date in CalendarUtility.GetDatesInInterval(dateFrom, dateTo))
							{
								if (existingTimeStamps.Any(a => a.EmployeeId == employee.EmployeeId && a.ActorCompanyId == actorCompanyId && a.TimeBlockDate.Date == date))
								{
									//if timestamps are only that day and there are closer to eachother than 60 minutes. We need to delete them
									var existingOnDate = existingTimeStamps.Where(a => a.EmployeeId == employee.EmployeeId && a.ActorCompanyId == actorCompanyId && a.TimeBlockDate.Date == date).ToList();

									if (date < new DateTime(2025, 12, 11) && existingOnDate.Count() == 2)
									{
										var diff = Math.Abs((existingTimeStamps.Max(m => m.Time) - existingTimeStamps.Min(m => m.Time)).TotalMinutes) < 60;
										if (diff)
										{
											existingOnDate.ForEach(f => f.State = (int)SoeEntityState.Deleted);
											existingOnDate.ForEach(f => f.ModifiedBy = "Del exjob 251008");
											SaveChanges(entities);
										}
									}

									if (existingTimeStamps.Where(a => a.EmployeeId == employee.EmployeeId && a.ActorCompanyId == actorCompanyId && a.TimeBlockDate.Date == date).All(a => !a.TimeScheduleTemplatePeriodId.HasValue))
									{
										existingTimeStamps.Where(a => a.EmployeeId == employee.EmployeeId && a.ActorCompanyId == actorCompanyId && a.TimeBlockDate.Date == date && !a.TimeScheduleTemplatePeriodId.HasValue).ToList().ForEach(f => f.State = (int)SoeEntityState.Deleted);
										existingTimeStamps.Where(a => a.EmployeeId == employee.EmployeeId && a.ActorCompanyId == actorCompanyId && a.TimeBlockDate.Date == date && !a.TimeScheduleTemplatePeriodId.HasValue).ToList().ForEach(f => f.ModifiedBy = "Del exjob 324324");
										SaveChanges(entities);
									}
									else
									{
										continue;
									}
								}

								EmployeeGroup employeeGroup = employee.GetEmployeeGroup(date, employeeGroups);
								if (employeeGroup == null || employeeGroup.AutogenTimeblocks)
									continue;

								var shiftsOnDate = shifts.Where(w => w.Date == date).OrderBy(o => o.StartTime).ToList();
								bool firstOnDate = true;

								if (!createTimeStampsOnAllBlocks && shiftsOnDate.Any())
								{
									var firstShift = shiftsOnDate.FirstOrDefault();
									var lastShift = shiftsOnDate.LastOrDefault();


									TSTimeStampEntryItem inItem = new TSTimeStampEntryItem()
									{
										Time = CalendarUtility.MergeDateAndTime(date, firstShift.StartTime),
										EmployeeId = employee.EmployeeId,
										TimeStampEntryInternalId = 0,
										TimeDeviationCauseId = standardTimeDeviationCode.TimeDeviationCauseId,
										Type = (int)TermGroup_TimeStampEntryType.In,
										Created = DateTime.Now,
									};

									TSTimeStampEntryItem outItem = new TSTimeStampEntryItem()
									{
										Time = CalendarUtility.MergeDateAndTime(date, lastShift.StopTime),
										EmployeeId = employee.EmployeeId,
										TimeStampEntryInternalId = 0,
										TimeDeviationCauseId = standardTimeDeviationCode.TimeDeviationCauseId,
										Type = (int)TermGroup_TimeStampEntryType.Out,
										Created = DateTime.Now
									};

									var nextShift = shiftsOnDate.FirstOrDefault(f => f.StartTime > firstShift.StartTime);
									var maxSickMinutes = nextShift != null ? Convert.ToInt32((nextShift.StartTime - firstShift.StartTime).TotalMinutes) : 60;

									if (maxSickMinutes > 60)
										maxSickMinutes = 60;

									if (random.Next(1, 20) == 1)
									{
										if (maxSickMinutes > firstShift.TotalMinutes)
											maxSickMinutes = Convert.ToInt32(firstShift.TotalMinutes / 2);

										if (maxSickMinutes > 5)
										{
											inItem.TimeDeviationCauseId = sickTimeDeviationCode.TimeDeviationCauseId;
											inItem.Time = inItem.Time.AddMinutes(random.Next(5, maxSickMinutes));
										}
									}
									else if (random.Next(1, 3) == 1)
									{
										var maxLate = Convert.ToInt32(decimal.Divide(maxSickMinutes, 5));

										if (maxLate > 5)
											maxLate = 4;
										inItem.Time = inItem.Time.AddMinutes(random.Next(-5, maxLate));
									}
									else if (random.Next(1, 4) == 1)
									{
										var earlyMinutes = -random.Next(1, 5);
										inItem.Time = inItem.Time.AddMinutes(earlyMinutes);
									}

									entryItems.Add(inItem);

									if (random.Next(1, 4) == 1)
									{
										var lateMinutes = random.Next(-5, 5);
										outItem.Time = outItem.Time.AddMinutes(lateMinutes);
									}

									entryItems.Add(outItem);
								}
								else
								{
									foreach (var shift in shiftsOnDate)
									{
										bool lastOnDate = shift.StopTime == shiftsOnDate.OrderBy(o => o.StopTime).LastOrDefault().StopTime;

										TSTimeStampEntryItem inItem = new TSTimeStampEntryItem()
										{
											Time = CalendarUtility.MergeDateAndTime(date, shift.StartTime),
											EmployeeId = employee.EmployeeId,
											TimeStampEntryInternalId = 0,
											TimeDeviationCauseId = standardTimeDeviationCode.TimeDeviationCauseId,
											Type = !shift.IsBreak ? (int)TermGroup_TimeStampEntryType.In : (int)TermGroup_TimeStampEntryType.Out,
											Created = DateTime.Now,
										};

										if (firstOnDate && !shift.IsBreak)
										{
											var nextShift = shiftsOnDate.FirstOrDefault(f => f.StartTime > shift.StartTime);
											var maxSickMinutes = nextShift != null ? Convert.ToInt32((nextShift.StartTime - shift.StartTime).TotalMinutes) : 60;

											if (maxSickMinutes > 60)
												maxSickMinutes = 60;

											if (random.Next(1, 20) == 1)
											{
												if (maxSickMinutes > shift.TotalMinutes)
													maxSickMinutes = Convert.ToInt32(shift.TotalMinutes / 2);

												if (maxSickMinutes > 5)
												{
													inItem.TimeDeviationCauseId = sickTimeDeviationCode.TimeDeviationCauseId;
													inItem.Time = inItem.Time.AddMinutes(random.Next(5, maxSickMinutes));
												}
											}
											else if (random.Next(1, 3) == 1)
											{
												var maxLate = Convert.ToInt32(decimal.Divide(maxSickMinutes, 5));

												if (maxLate > 10)
													maxLate = 10;
												inItem.Time = inItem.Time.AddMinutes(random.Next(-10, maxLate));
											}
										}

										if (shift.IsBreak)
											inItem.Time = inItem.Time.AddMinutes(random.Next(-5, 5));

										firstOnDate = false;
										entryItems.Add(inItem);

										TSTimeStampEntryItem outItem = new TSTimeStampEntryItem()
										{
											Time = CalendarUtility.MergeDateAndTime(date, shift.StopTime),
											EmployeeId = employee.EmployeeId,
											TimeStampEntryInternalId = 0,
											TimeDeviationCauseId = standardTimeDeviationCode.TimeDeviationCauseId,
											Type = !shift.IsBreak ? (int)TermGroup_TimeStampEntryType.Out : (int)TermGroup_TimeStampEntryType.In
										};

										if (lastOnDate)
										{
											var prevShift = shiftsOnDate.LastOrDefault(f => f.StartTime < shift.StartTime);
											var maxSickMinutes = prevShift != null ? Convert.ToInt32((shift.StopTime - prevShift.StopTime).TotalMinutes) : 60;

											if (random.Next(1, 20) == 1)
											{
												if (maxSickMinutes > 60)
													maxSickMinutes = 60;

												if (maxSickMinutes > shift.TotalMinutes)
													maxSickMinutes = Convert.ToInt32(shift.TotalMinutes / 2);

												if (maxSickMinutes > 5)
												{
													outItem.TimeDeviationCauseId = sickTimeDeviationCode.TimeDeviationCauseId;
													outItem.Time = outItem.Time.AddMinutes(-random.Next(1, maxSickMinutes));
												}
											}
											else if (random.Next(1, 3) == 1)
											{
												var maxLeaveEarly = Convert.ToInt32(decimal.Divide(maxSickMinutes, 5));

												if (maxLeaveEarly > 10)
													maxLeaveEarly = 10;
												inItem.Time = inItem.Time.AddMinutes(random.Next(-maxLeaveEarly, 10));
											}
										}

										if (shift.IsBreak)
											outItem.Time = outItem.Time.AddMinutes(random.Next(-5, 5));

										entryItems.Add(outItem);
									}
								}
							}
							if (entryItems.Any())
								SyncNewTimeStampEntries(entryItems, 0, accountDim.AccountDimId, actorCompanyId, 0);
						}
					}
					catch (Exception ex)
					{
						base.LogError(ex, this.log);
					}
				}
			}
			catch (Exception ex)
			{
				return new ActionResult(ex, "fake failed");
			}

			return new ActionResult();
		}
		public Dictionary<int, int> SyncNewEmployees(List<TSEmployeeItem> employees, int timeTerminalId, int actorCompanyId)
		{
			ActionResult result = new ActionResult();
			var updatedEmployees = new Dictionary<int, int>();
			using (var entities = new CompEntities())
			{
				try
				{
					entities.Connection.Open();
					using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
					{
						foreach (var item in employees)
						{
							if (item.EmployeeId <= 0)
							{
								// Sync does not support creating new entries
							}
							else
							{
								// Check that the cardnr does not exist on inactive employees
								var existingEmployees = (from entry in entities.Employee
														 where entry.CardNumber == item.CardNumber &&
														 entry.ActorCompanyId == actorCompanyId
														 select entry).ToList();

								foreach (var e in existingEmployees)
								{
									if (e.State == (int)SoeEntityState.Active)
										return updatedEmployees;

									e.CardNumber = null;
									SaveChanges(entities);
								}

								var employee = EmployeeManager.GetEmployee(entities, item.EmployeeId, actorCompanyId);
								if (employee != null && (employee.Modified == null || employee.Modified < item.Modified))
								{
									employee.CardNumber = item.CardNumber;
									SetModifiedProperties(employee);
									updatedEmployees.Add(employee.EmployeeId, default(int));
								}
							}
						}

						result = SaveChanges(entities, transaction);
						if (result.Success)
						{
							transaction.Complete();
						}
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
					}
					else
						base.LogTransactionFailed(this.ToString(), this.log);
				}
			}

			return updatedEmployees;
		}

		public void LoadTimeStampEntry(CompEntities entities, TimeStampEntry entry, bool loadAccount, bool loadTimeDeviationCause, bool loadTimeBlockDate)
		{
			if (entry == null)
				return;

			if (loadAccount && !entry.AccountReference.IsLoaded)
				entry.AccountReference.Load();
			if (loadTimeDeviationCause && !entry.TimeDeviationCauseReference.IsLoaded)
				entry.TimeDeviationCauseReference.Load();
			if (loadTimeBlockDate && !entry.TimeBlockDateReference.IsLoaded)
				entry.TimeBlockDateReference.Load();
		}

		#endregion

		#endregion

		#region Convert TimeStampEntry

		public bool DoNotRecalulateTimeStampEntryType(bool doNotModifyTimeStampEntryTypeCompSetting, TimeStampEntry entry)
		{
			if (!entry.TimeTerminalReference.IsLoaded)
				entry.TimeTerminalReference.Load();
			return DoNotRecalulateTimeStampEntryType(doNotModifyTimeStampEntryTypeCompSetting, entry, entry.TimeTerminal);
		}

		public bool DoNotRecalulateTimeStampEntryType(bool doNotModifyTimeStampEntryTypeCompSetting, TimeStampEntry entry, TimeTerminal timeTerminal)
		{
			return
				entry.CreatedBy == Constants.CREATED_BY_AUTO_STAMP_OUT_JOB
				||
				(doNotModifyTimeStampEntryTypeCompSetting &&
				entry.Type != (int)TimeStampEntryType.Unknown &&
				(timeTerminal == null || timeTerminal.Type == (int)TimeTerminalType.GoTimeStamp || timeTerminal.Type == (int)TimeTerminalType.XETimeStamp || timeTerminal.Type == (int)TimeTerminalType.WebTimeStamp));
		}

		public ActionResult ConvertTimeStampsToTimeBlocks(int actorCompanyId, DateTime? after = null)
		{
			ActionResult result;

			List<TimeStampEntry> timeStampItems = new List<TimeStampEntry>();

			using (CompEntities entities = new CompEntities())
			{
				// Get entries for current company
				bool ignoreOffline = SettingManager.GetBoolSetting(entities, SettingMainType.Company, (int)CompanySettingType.TimeIgnoreOfflineTerminals, 0, actorCompanyId, 0);
				DateTime? oldestSync = ignoreOffline ? null : GetTimeTerminalOldestSync(entities, actorCompanyId);
				List<TimeStampEntry> timeStampEntries = GetNewTimeStampEntries(entities, actorCompanyId, oldestSync, loadTimeTerminal: true, after: after, loadExtended: HasExtendedTimeStamps(entities, actorCompanyId));
				if (!timeStampEntries.IsNullOrEmpty())
				{
					List<int> employeeIds = timeStampEntries.Select(s => s.EmployeeId).Distinct().ToList();
					List<Employee> employees = EmployeeManager.GetAllEmployeesByIds(actorCompanyId, employeeIds, loadEmployment: true);
					List<EmployeeGroup> employeeGroups = EmployeeManager.GetEmployeeGroups(entities, actorCompanyId);

					bool doNotModifyTimeStampEntryType = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.TimeDoNotModifyTimeStampEntryType, 0, actorCompanyId, 0, defaultValue: false);
					using var entitiesReadonly = CompEntitiesProvider.LeaseReadOnlyContext();
					bool useAccountHierarchy = base.UseAccountHierarchyOnCompanyFromCache(entitiesReadonly, actorCompanyId);
					bool? useTimeScheduleTypeFromTimeStampEntry = null;

					List<Tuple<int, int?>> timeTerminalAccountSetting = new List<Tuple<int, int?>>();
					List<Tuple<int, int?>> timeTerminalTerminalAccountSetting = new List<Tuple<int, int?>>();
					DateTime lastDate = DateTime.Now;
					TimeStampEntryType lastEntryType = TimeStampEntryType.Out;
					int? prevEmployeeId = null;

					foreach (var timeStampEntriesByEmployee in timeStampEntries.GroupBy(i => i.EmployeeId).ToList())
					{
						// Sort entries by time
						List<TimeStampEntry> timeStampEntriesForEmployee = timeStampEntriesByEmployee.OrderBy(i => i.Time).ToList();
						if (!timeStampEntriesForEmployee.Any())
							continue;

						// If all stamps are in status processing, skip this employee, because then there might be another job running
						if (!timeStampEntriesForEmployee.Any(t => t.Status != (int)TermGroup_TimeStampEntryStatus.Processing))
							continue;

						Employee employee = employees.FirstOrDefault(f => f.EmployeeId == timeStampEntriesByEmployee.Key);
						if (employee == null)
							continue;

						DateTime date = timeStampEntriesForEmployee[0].Time.Date;
						EmployeeGroup employeeGroup = employee.GetEmployeeGroup(date, employeeGroups: employeeGroups);
						if (employeeGroup == null)
							continue;

						DateTime firstTBDate = timeStampEntriesByEmployee.OrderBy(o => o.Time).First().Time.AddDays(-2).Date;
						DateTime lastTBDate = timeStampEntriesByEmployee.OrderBy(o => o.Time).Last().Time.AddDays(3).Date;
						List<TimeBlockDate> timeBlockDates = entities.TimeBlockDate.Where(w => w.EmployeeId == timeStampEntriesByEmployee.Key && w.Date > firstTBDate && w.Date < lastTBDate).ToList();
						List<int> timeBlockDateIdsWithAutoStampOutsToRemove = new List<int>();

						foreach (TimeStampEntry entry in timeStampEntriesForEmployee)
						{
							int minutesAfterMidnight = employeeGroup.BreakDayMinutesAfterMidnight;
							int keepStampsTogetherWithinMinutes = employeeGroup.KeepStampsTogetherWithinMinutes;

							if (keepStampsTogetherWithinMinutes >= 800)
								keepStampsTogetherWithinMinutes = 0; //unrealistic setting from template company

							// Adjust midnight if last timestamp is before daybreak and type=in and not more then XX hours before,
							// where XX = KeepStampsTogetherWithinMinutes on EmployeeGroup.

							if (minutesAfterMidnight >= 0 && keepStampsTogetherWithinMinutes > 0 && keepStampsTogetherWithinMinutes >= minutesAfterMidnight && entry.Time.Date != entry.Time.AddMinutes(-keepStampsTogetherWithinMinutes).Date)
							{
								var from = entry.Time.AddMinutes(-keepStampsTogetherWithinMinutes);
								var to = entry.Time.Date.AddMinutes(minutesAfterMidnight);
								var prevStamps = timeStampEntriesForEmployee.Where(w => w.EmployeeId == employee.EmployeeId && w.State == (int)SoeEntityState.Active && w.Time > from && w.Time < to && w.TimeStampEntryId != entry.TimeStampEntryId).ToList();

								if (!prevStamps.IsNullOrEmpty() && prevStamps.Any(a => a.Type == (int)TimeStampEntryType.In))
									minutesAfterMidnight = keepStampsTogetherWithinMinutes;
								else if (entry.Time.Hour < 9)
								{
									prevStamps = entities.TimeStampEntry.Where(w => w.EmployeeId == employee.EmployeeId && w.State == (int)SoeEntityState.Active && w.Time > from && w.Time < to && w.TimeStampEntryId != entry.TimeStampEntryId).OrderBy(o => o.Time).ToList();

									if (!prevStamps.IsNullOrEmpty() && prevStamps.Any(a => a.Type == (int)TimeStampEntryType.In))
										minutesAfterMidnight = keepStampsTogetherWithinMinutes;
								}
							}

							// Check InternalAccount setting on terminal
							int? accountInternalId = entry.AccountId;
							if (!entry.AccountId.HasValue && entry.TimeTerminalId.HasValue)
							{
								if (timeTerminalAccountSetting.Any(a => a.Item1 == entry.TimeTerminalId.Value))
								{
									accountInternalId = timeTerminalAccountSetting.FirstOrDefault(f => f.Item1 == entry.TimeTerminalId.Value).Item2;
								}
								else
								{
									TimeTerminalSetting accountInternalSetting = GetTimeTerminalSetting(TimeTerminalSettingType.InternalAccountDim1Id, (int)entry.TimeTerminalId);
									if (accountInternalSetting != null)
										accountInternalId = (accountInternalSetting.IntData.HasValue && accountInternalSetting.IntData.Value != 0) ? accountInternalSetting.IntData : (int?)null;

									timeTerminalAccountSetting.Add(Tuple.Create(entry.TimeTerminalId.Value, accountInternalId));
								}
							}

							// Check Account setting on terminal
							int? terminalAccountId = entry.TimeTerminalAccountId;
							if (useAccountHierarchy && !entry.TimeTerminalAccountId.HasValue && entry.TimeTerminalId.HasValue)
							{
								if (timeTerminalTerminalAccountSetting.Any(a => a.Item1 == entry.TimeTerminalId.Value))
								{
									// Get from cache
									terminalAccountId = timeTerminalTerminalAccountSetting.FirstOrDefault(f => f.Item1 == entry.TimeTerminalId.Value).Item2;
								}
								else
								{
									// Note! This will only set the account if the terminal is linked to one account, and one account only
									List<TimeTerminalSetting> terminalAccountSettings = GetTimeTerminalSettings(TimeTerminalSettingType.LimitAccount, (int)entry.TimeTerminalId);
									if (terminalAccountSettings.Count == 1)
									{
										TimeTerminalSetting terminalAccountSetting = terminalAccountSettings.First();

										// Previously account was stored as string, now it is stored as int
										int accountId = 0;
										if (terminalAccountSetting.IntData.HasValue)
											accountId = terminalAccountSetting.IntData.Value;
										else
											Int32.TryParse(terminalAccountSetting.StrData, out accountId);

										if (accountId != 0)
											terminalAccountId = accountId;
									}
									else
									{
										terminalAccountId = null;
									}

									// Add to cache
									timeTerminalTerminalAccountSetting.Add(Tuple.Create(entry.TimeTerminalId.Value, terminalAccountId));
								}
							}

							//SetShiftType
							if (entry.AccountId.HasValue)
							{
								if (!useTimeScheduleTypeFromTimeStampEntry.HasValue)
									useTimeScheduleTypeFromTimeStampEntry = SettingManager.GetBoolSetting(entities, SettingMainType.Company, (int)CompanySettingType.UseTimeScheduleTypeFromTime, 0, actorCompanyId, 0);

								if (useTimeScheduleTypeFromTimeStampEntry.HasValue && useTimeScheduleTypeFromTimeStampEntry.Value)
									SetShiftTypeAndTimeScheduleType(entities, entry, actorCompanyId);
							}

							// Set TimeBlockDate
							TimeBlockDate timeBlockDate = TimeBlockManager.GetTimeBlockDate(entities, actorCompanyId, entry.EmployeeId, entry.Time.AddMinutes(-minutesAfterMidnight).Date, true, timeBlockDates);
							entry.TimeBlockDate = timeBlockDate;
							if (timeBlockDate != null && entry.Status == (int)TermGroup_TimeStampEntryStatus.New && !timeBlockDateIdsWithAutoStampOutsToRemove.Contains(timeBlockDate.TimeBlockDateId))
							{
								// Keep track of all TimeBlockDates to be able to remove auto stamp out entries below
								timeBlockDateIdsWithAutoStampOutsToRemove.Add(timeBlockDate.TimeBlockDateId);
							}

							// Calculate entry type (only if companysetting says so)
							TimeStampEntryType thisEntryType;
							if (DoNotRecalulateTimeStampEntryType(doNotModifyTimeStampEntryType, entry))
							{
								thisEntryType = (TimeStampEntryType)entry.Type;
							}
							else
							{
								DateTime breakTime = entry.Time.Date.AddMinutes(minutesAfterMidnight);
								thisEntryType = (lastEntryType == TimeStampEntryType.Out || (lastDate < breakTime && entry.Time > breakTime) || prevEmployeeId != entry.EmployeeId) ? TimeStampEntryType.In : TimeStampEntryType.Out;
							}

							//Cabonline - Latvia
							if (actorCompanyId == 594154)
							{
								int? settingMinutesBeforeMidnight = 30;
								int? settingMinutesAfterMidnight = 30;
								if (settingMinutesBeforeMidnight.HasValue && settingMinutesBeforeMidnight.Value > 0 && settingMinutesAfterMidnight.HasValue && settingMinutesAfterMidnight.Value > 0 && timeBlockDate != null)
								{
									DateTime nextDay = timeBlockDate.Date.AddDays(1);
									if (entry.Time >= nextDay.AddMinutes(-settingMinutesBeforeMidnight.Value))
									{
										List<TimeScheduleTemplateBlock> templateBlocksNextDay = TimeScheduleManager.GetTimeScheduleTemplateBlocksForDay(entities, entry.EmployeeId, nextDay);
										DateTime scheduleInNextDay = CalendarUtility.GetDateTime(nextDay, templateBlocksNextDay.GetScheduleIn());
										if (scheduleInNextDay <= nextDay.AddMinutes(settingMinutesAfterMidnight.Value))
											entry.TimeBlockDate = TimeBlockManager.GetTimeBlockDate(entities, actorCompanyId, entry.EmployeeId, nextDay, true);
									}
								}
							}

							// Update current entry
							entry.Type = (int)thisEntryType;
							entry.AccountId = accountInternalId;
							entry.TimeTerminalAccountId = terminalAccountId;
							// To prevent "faulty" last ModifiedBy, job should not set modified properties (#91775)
							//SetModifiedProperties(entry);
							timeStampItems.Add(entry);

							// Remember some values to next entry
							prevEmployeeId = entry.EmployeeId;
							lastDate = entry.Time;
							lastEntryType = thisEntryType;
						}

						// Remove previously created auto stamp out entries.
						// Can happen if stamping out after auto stamp out was created or terminal is synced after.
						RemoveAutoStampOutEntries(entities, timeBlockDateIdsWithAutoStampOutsToRemove);

						result = SaveChanges(entities, null, useBulkSaveChanges: true);
						if (!result.Success)
							return result;
					}
				}
			}

			result = TimeEngineManager(actorCompanyId, base.UserId).SaveTimeStampsFromJob(timeStampItems);
			if (result.Success)
				result = RevertProcessingStatus(timeStampItems);

			return result;
		}

		private void RemoveAutoStampOutEntries(CompEntities entities, List<int> timeBlockDateIds)
		{
			List<TimeStampEntry> entries = (from t in entities.TimeStampEntry
											where t.TimeBlockDateId.HasValue && timeBlockDateIds.Contains(t.TimeBlockDateId.Value) &&
											t.AutoStampOut &&
											t.State == (int)SoeEntityState.Active &&
											t.Status != (int)TermGroup_TimeStampEntryStatus.New
											select t).ToList();

			foreach (TimeStampEntry entry in entries)
			{
				entry.State = (int)SoeEntityState.Deleted;
				entry.Modified = DateTime.Now;
				entry.ModifiedBy = Constants.MODIFIED_BY_AUTO_STAMP_OUT_JOB;
			}
		}

		public ActionResult RemoveDuplicateTimeStampEntries(int actorCompanyId)
		{
			ActionResult result = new ActionResult(true);

			string sql = string.Empty;

			try
			{
				using (CompEntities entities = new CompEntities())
				{
					entities.CommandTimeout = 300;
					DateTime fromDate = GetTimeTerminalOldestSync(entities, actorCompanyId) ?? DateTime.Now.AddDays(-1);

					if (fromDate > DateTime.Now.AddDays(-4))
						fromDate = DateTime.Now.AddDays(-4);

					entities.CommandTimeout = (60 * 30);
					sql = $@"with cte ([Time], EmployeeId, [Type],TimeTerminalId, ActorCompanyId, state,isbreak,rn ) as ( 
                                    select [Time], EmployeeId, [Type],TimeTerminalId, ActorCompanyId, isbreak, state,  
                                    row_number() over(partition by[Time], EmployeeId, [Type], TimeTerminalId, ActorCompanyId, state, isbreak
                                    order by TimeStampEntryId desc) as [rn]
                                    from TimeStampEntry where state = 0 and actorCompanyId ={actorCompanyId} and created > '{CalendarUtility.ToSqlFriendlyDateTime(fromDate)}')
                                    update cte set state = 9999 where rn> 1";

					int rowsAffected = FrownedUponSQLClient.ExecuteSql(entities, sql, 600);

					if (rowsAffected > 0)
					{
						var timeStampEntrys = (from tse in entities.TimeStampEntry
											   where tse.State == 9999 &&
											   tse.ActorCompanyId == actorCompanyId &&
											   tse.Created > fromDate
											   select new
											   {
												   tse.TimeStampEntryId,
												   tse.TimeBlockDateId,
											   }).ToList();

						if (!timeStampEntrys.Any())
							return result;

						List<int> timeStampEntryIds = timeStampEntrys.Select(tse => tse.TimeStampEntryId).Distinct().ToList();
						List<int> timeBlockDateIds = timeStampEntrys.Where(tse => tse.TimeBlockDateId.HasValue).Select(t => t.TimeBlockDateId.Value).Distinct().ToList();
						string timeStampEntryIdsString = timeStampEntryIds.JoinToString(",");
						DateTime now = DateTime.Now;

						if (timeStampEntrys.Count < 1000)
						{
							sql = $@"update TimeStampEntry set 
                                                                state = 2, 
                                                                Modified = '{CalendarUtility.ToSqlFriendlyDateTime(now)}',
                                                                ModifiedBy = 'RemByDupJob' 
                                                                where timestampEntryId in ({timeStampEntryIdsString})";
							FrownedUponSQLClient.ExecuteSql(entities, sql);
						}
						else
						{
							sql = $@"update TimeStampEntry set 
                                                                state = 2, 
                                                                Modified = '{CalendarUtility.ToSqlFriendlyDateTime(now)}',
                                                                ModifiedBy = 'RemByDupJob' 
                                                                where actorCompanyId ={actorCompanyId} and state = 9999 and created > '{CalendarUtility.ToSqlFriendlyDateTime(fromDate)}'";
							FrownedUponSQLClient.ExecuteSql(entities, sql);
						}

						if (!timeStampEntryIds.IsNullOrEmpty())
							SysLogManager.AddSysLogWarningMessage("TimeStampManager", "RemoveDuplicateTimeStampEntries", $"Found duplicate timestamp entry, setting state to deleted on TimeStampEntryIds: {timeStampEntryIdsString}", null, null, SoeSysLogRecordType.TimeTerminal);

						UpdateTimeBlockDateStampingStatus(entities, timeBlockDateIds);

						result.IntegerValue = rowsAffected;
					}

					return result;
				}
			}
			catch (Exception ex)
			{
				result.Success = false;
				result.ErrorMessage = ex.Message + "sql: " + sql;
				result.Exception = ex;
				base.LogError(ex, log);
				return result;
			}
		}

		private void UpdateTimeBlockDateStampingStatus(CompEntities entities, List<int> timeBlockDateIds)
		{
			if (timeBlockDateIds.IsNullOrEmpty())
				return;

			var timeBlockDates = entities.TimeBlockDate.Where(tbd => timeBlockDateIds.Contains(tbd.TimeBlockDateId)).ToList();
			timeBlockDates = timeBlockDates.Where(tbd => tbd.StampingStatus > (int)TermGroup_TimeBlockDateStampingStatus.Complete).ToList();
			if (timeBlockDates.IsNullOrEmpty())
				return;

			bool changed = false;
			foreach (TimeBlockDate timeBlockdate in timeBlockDates)
			{
				if (!entities.TimeStampEntry.Any(tse => tse.TimeBlockDateId == timeBlockdate.TimeBlockDateId && tse.State == (int)SoeEntityState.Active))
				{
					timeBlockdate.StampingStatus = (int)TermGroup_TimeBlockDateStampingStatus.NoStamps;
					SysLogManager.AddSysLogInfoMessage("TimeStampManager", "RemoveDuplicateTimeStampEntries", $"Setting StampingStatus to NoStamps on TimeBlockDate: {timeBlockdate.Date.ToShortDateString()} ({timeBlockdate.TimeBlockDateId})", null, null, SoeSysLogRecordType.TimeTerminal);
					changed = true;
				}
			}

			if (changed)
				entities.SaveChanges();
		}

		/// <summary>
		/// Set TimeStampEntry Status to 'New' on entries with status 'Processing'
		/// </summary>
		/// <param name="timeStampItems">List of TimeStampEntries</param>
		/// <returns>ActionResult</returns>
		private ActionResult RevertProcessingStatus(List<TimeStampEntry> timeStampItems)
		{
			ActionResult result;

			try
			{
				using (CompEntities entities = new CompEntities())
				{
					foreach (TimeStampEntry timeStampEntry in timeStampItems)
					{
						TimeStampEntry originalTimeStampEntry = GetTimeStampEntry(entities, timeStampEntry.TimeStampEntryId, false, false);
						if (originalTimeStampEntry.Status == (int)TermGroup_TimeStampEntryStatus.Processing)
							originalTimeStampEntry.Status = (int)TermGroup_TimeStampEntryStatus.New;
					}
					result = SaveChanges(entities);
				}
			}
			catch (Exception ex)
			{
				result = new ActionResult(ex);
				base.LogError(ex, this.log);
			}

			return result;
		}

		public ActionResult DiscardUnsuccessfulEntries(int actorCompanyId)
		{
			ActionResult result;

			using (CompEntities entities = new CompEntities())
			{
				// Get entries for current company that has not been successfully processed for three days
				var oldestSync = GetTimeTerminalOldestSync(entities, actorCompanyId) ?? DateTime.Now;
				oldestSync = oldestSync.AddDays(-7);

				// Change state on entries
				List<TimeStampEntry> timeStampEntries = GetNewTimeStampEntries(entities, actorCompanyId, oldestSync);
				foreach (TimeStampEntry entry in timeStampEntries)
				{
					entry.Status = (int)TermGroup_TimeStampEntryStatus.ProcessedWithNoResult;
				}

				result = SaveChanges(entities);
				if (result.Success)
					result.IntegerValue = timeStampEntries.Count;
			}

			return result;
		}

		#endregion

		#region WebPubSub

		public void SendWebPubSubMessage(CompEntities entities, TimeStampEntry timeStampEntry, WebPubSubMessageAction action, List<int> terminalIds = null)
		{
			// Publish message to all terminals connected to current employee.
			// This will do a refresh of the attendance view in each relevant terminal.

			if (terminalIds == null)
				terminalIds = TimeStampManager.GetTimeTerminalIdsForPubSub(entities, timeStampEntry.ActorCompanyId);

			bool useCache = false;
			foreach (int terminalId in terminalIds)
			{
				if (terminalIds.Count == 1 || IsEmployeeConnectedToTimeTerminal(entities, timeStampEntry.ActorCompanyId, terminalId, timeStampEntry.EmployeeId, timeStampEntry.Time, useCache))
					base.WebPubSubSendMessage(GoTimeStampExtensions.GetTerminalPubSubKey(timeStampEntry.ActorCompanyId, terminalId), timeStampEntry.GetUpdateMessage(action));

				useCache = true;
			}
		}

		#endregion
	}
}

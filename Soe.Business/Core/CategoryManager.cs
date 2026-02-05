using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Data.Entity.Core.Objects.DataClasses;
using System.Linq;
using System.Text;
using System.Transactions;

namespace SoftOne.Soe.Business.Core
{
	public class CategoryManager : ManagerBase
	{
		#region Ctor

		public CategoryManager(ParameterObject parameterObject) : base(parameterObject) { }

		#endregion

		#region Category

		public SmallGenericType[] GetCategoryKeyValues(SoeCategoryType type, int actorCompanyId)
		{
			using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();

			return entitiesReadOnly.Category.AsNoTracking()
				.Where(c => c.ActorCompanyId == actorCompanyId &&
							 c.Type == (int)type &&
							 c.State == (int)SoeEntityState.Active)
				.Select(c => new SmallGenericType() { Id = c.CategoryId, Name = c.Name })
				.ToArray();
		}

		public IEnumerable<Category> GetCategoriesBySearch(int actorCompanyId, string search, int no)
		{
			using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
			entities.Category.NoTracking();
			return (from c in entities.Category
					where c.ActorCompanyId == actorCompanyId &&
					c.Name.ToLower().Contains(search.ToLower()) &&
					c.State == (int)SoeEntityState.Active
					orderby c.Name ascending
					select c).Take(no).ToList();
		}

		public List<Category> GetAllCategoriesWithRecords(int actorCompanyId)
		{
			using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
			entities.Category.NoTracking();
			return (from c in entities.Category
					.Include("CompanyCategoryRecord")
					where c.ActorCompanyId == actorCompanyId &&
					c.State == (int)SoeEntityState.Active
					orderby c.Name ascending
					select c).ToList();
		}

		public List<CategoryGridDTO> GetCategoriesForGrid(SoeCategoryType type, int actorCompanyId, int? categoryId = null)
		{
			using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
			return this.GetCategoriesForGrid(entitiesReadOnly, type, actorCompanyId, categoryId);
		}

		public List<CategoryGridDTO> GetCategoriesForGrid(CompEntities entities, SoeCategoryType type, int actorCompanyId, int? categoryId = null)
		{
			var query = (from c in entities.Category
						 where c.ActorCompanyId == actorCompanyId &&
						 c.Type == (int)type &&
						 c.State == (int)SoeEntityState.Active
						 select c);

			var tempDTO = query.Select(e => new 
			{
				e.CategoryId,
				e.Code,
				e.Name,
				ChildrenNamesString = e.Children.Where(c => c.State == (int)SoeEntityState.Active).Select(i => i.Name).Distinct(),
                CategoryGroupName = e.CategoryGroup.Name
			}).ToList();

			return tempDTO.Select(c=> new CategoryGridDTO
			{
				CategoryId = c.CategoryId,
				Code = c.Code,
				Name = c.Name,
				ChildrenNamesString = c.ChildrenNamesString.ToCommaSeparated(),
				CategoryGroupName = c.CategoryGroupName
			}).OrderBy(c=> c.Name).ToList();
        }

		public List<CategoryDTO> GetCategoryDTOs(SoeCategoryType type, int actorCompanyId, bool loadCompanyCategoryRecord = false, bool loadChildren = false, bool loadCategoryGroups = false)
		{
			var entities = CompEntitiesProvider.LeaseReadOnlyContext();
			var categories = GetCategories(entities, type, actorCompanyId, loadCompanyCategoryRecord, loadChildren, loadCategoryGroups);
			return categories.ToDTOs(loadCompanyCategoryRecord).ToList();
		}

		public List<CategoryGridDTO> GetCategoryGridDTOs(SoeCategoryType type, int actorCompanyId, bool loadCompanyCategoryRecord = false, bool loadChildren = false, bool loadCategoryGroups = false)
		{
			var entities = CompEntitiesProvider.LeaseReadOnlyContext();
			var categories = GetCategories(entities, type, actorCompanyId, loadCompanyCategoryRecord, loadChildren, loadCategoryGroups);
			return categories.ToGridDTOs().ToList();
		}

        public List<Category> GetCategories(CompEntities entities, SoeCategoryType type, List<int> filterCategoryIds, int actorCompanyId)
        {
            return this.GetCategoriesQuery(entities, type, filterCategoryIds, actorCompanyId).ToList();
        }

        public List<Category> GetCategories(SoeCategoryType type, int actorCompanyId, bool loadCompanyCategoryRecord = false, bool loadChildren = false, bool loadCategoryGroups = false)
		{
			using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
			entities.Category.NoTracking();
			return GetCategories(entities, type, actorCompanyId, loadCompanyCategoryRecord, loadChildren, loadCategoryGroups);
		}

		public List<Category> GetCategories(CompEntities entities, SoeCategoryType type, int actorCompanyId, bool loadCompanyCategoryRecord = false, bool loadChildren = false, bool loadCategoryGroups = false)
		{
			List<Category> categories = (from c in entities.Category
										 .Include("Children")
										 where c.ActorCompanyId == actorCompanyId &&
										 c.Type == (int)type &&
										 c.State == (int)SoeEntityState.Active
										 orderby c.Name
										 select c).ToList();

			foreach (Category category in categories)
			{
				if (loadChildren)
				{
					if (!category.Children.IsLoaded)
						category.Children.Load();

					if (category.Children != null && category.HasValidChildrenChain())
						category.ChildrenNamesString = category.Children.Where(c => c.State == (int)SoeEntityState.Active).Select(i => i.Name).Distinct().ToCommaSeparated();
				}

				if (loadCompanyCategoryRecord && !category.CompanyCategoryRecord.IsLoaded)
					category.CompanyCategoryRecord.Load();

				if (loadCategoryGroups)
				{
					if (!category.CategoryGroupReference.IsLoaded)
						category.CategoryGroupReference.Load();

					if (category.CategoryGroup != null && !String.IsNullOrEmpty(category.CategoryGroup.Name))
						category.CategoryGroupName = category.CategoryGroup.Name;
				}
			}

			return categories;
		}

		public List<Category> GetCategories(List<CompanyCategoryRecord> categoryRecords, int recordId, DateTime? dateFrom = null, DateTime? dateTo = null)
		{
			categoryRecords = categoryRecords.GetCategoryRecords(dateFrom, dateTo).Where(x => x.RecordId == recordId).ToList();

			List<Category> categories = new List<Category>();
			foreach (var record in categoryRecords)
			{
				categories.Add(record.Category);
			}

			return categories;
		}

		public List<Category> GetCategories(CompEntities entities, SoeCategoryType type, SoeCategoryRecordEntity entity, int recordId, int actorCompanyId, DateTime? dateFrom = null, DateTime? dateTo = null, bool onlyDefaultCategories = true, bool onlyExecutive = false)
		{
			List<CompanyCategoryRecord> categoryRecords = GetCompanyCategoryRecords(entities, type, entity, recordId, actorCompanyId, onlyDefaultCategories, onlyExecutive: onlyExecutive);
			categoryRecords = categoryRecords.GetCategoryRecords(dateFrom, dateTo);

			List<Category> categories = new List<Category>();
			foreach (var record in categoryRecords)
			{
				categories.Add(record.Category);
			}

			return categories;
		}

		public IEnumerable<Category> GetCategoriesQuery(CompEntities entities, SoeCategoryType type, List<int> filterCategoryIds, int actorCompanyId)
		{
			return (from i in entities.Category
					where i.ActorCompanyId == actorCompanyId &&
					i.Type == (int)type &&
					i.State == (int)SoeEntityState.Active &&
					filterCategoryIds.Contains(i.CategoryId)
					orderby i.Name
					select i).ToList();
		}

		public Dictionary<int, string> GetCategoriesDict(SoeCategoryType type, int actorCompanyId, bool addEmptyRow, int? excludeCategoryId = null)
		{
			var dict = new Dictionary<int, string>();
			if (addEmptyRow)
				dict.Add(0, " ");

			var categories = GetCategories(type, actorCompanyId);
			foreach (var category in categories)
			{
				if (excludeCategoryId.HasValue && excludeCategoryId.Value == category.CategoryId)
					continue;
				if (!dict.ContainsKey(category.CategoryId))
					dict.Add(category.CategoryId, category.Name);
			}
			return dict;
		}

		public Dictionary<int, string> GetCategoriesForRoleFromTypeDict(int actorCompanyId, int userId, int employeeId, SoeCategoryType type, bool isAdmin, bool includeSecondary, bool addEmptyRow, DateTime? dateFrom = null, DateTime? dateTo = null)
		{
			using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
			entities.Category.NoTracking();
			return GetCategoriesForRoleFromTypeDict(entities, actorCompanyId, userId, employeeId, type, isAdmin, includeSecondary, addEmptyRow, dateFrom, dateTo);
		}

		public Dictionary<int, string> GetCategoriesForRoleFromTypeDict(CompEntities entities, int actorCompanyId, int userId, int employeeId, SoeCategoryType type, bool isAdmin, bool includeSecondary, bool addEmptyRow, DateTime? dateFrom = null, DateTime? dateTo = null)
		{
			var dict = new Dictionary<int, string>();
			if (addEmptyRow)
				dict.Add(0, " ");

			if (isAdmin)
			{
				var attestRoles = AttestManager.GetAttestRolesForUser(entities, userId, actorCompanyId, module: SoeModule.Time);
				if (attestRoles.Any(r => r.ShowAllCategories))
				{
					var allCategories = GetCategories(entities, type, actorCompanyId);
					foreach (var category in allCategories)
					{
						if (!dict.ContainsKey(category.CategoryId))
							dict.Add(category.CategoryId, category.Name);
					}
				}
				else
				{
					foreach (var attestRole in attestRoles)
					{
						var primaryCategories = GetCompanyCategoryRecords(entities, type, SoeCategoryRecordEntity.AttestRole, attestRole.AttestRoleId, actorCompanyId);
						foreach (var categoryRecord in primaryCategories)
						{
							if (!dict.ContainsKey(categoryRecord.CategoryId))
								dict.Add(categoryRecord.CategoryId, categoryRecord.Category.Name);
						}

						if (includeSecondary)
						{
							var secondaryCategories = GetCompanyCategoryRecords(entities, type, SoeCategoryRecordEntity.AttestRoleSecondary, attestRole.AttestRoleId, actorCompanyId);
							foreach (var categoryRecord in secondaryCategories)
							{
								if (!dict.ContainsKey(categoryRecord.CategoryId))
									dict.Add(categoryRecord.CategoryId, categoryRecord.Category.Name);
							}

							if (attestRoles.Any(r => r.ShowAllSecondaryCategories))
							{
								var allSecondaryCategories = GetCategories(entities, type, actorCompanyId);
								foreach (var secondaryCategory in allSecondaryCategories)
								{
									if (!dict.ContainsKey(secondaryCategory.CategoryId))
										dict.Add(secondaryCategory.CategoryId, secondaryCategory.Name);
								}
							}
						}
					}
				}
			}
			else
			{
				Employee employee = EmployeeManager.GetEmployeeIgnoreState(entities, actorCompanyId, employeeId);
				if (employee != null)
				{
					var categories = GetCompanyCategoryRecords(entities, type, SoeCategoryRecordEntity.Employee, employee.EmployeeId, actorCompanyId, !includeSecondary, dateFrom, dateTo);
					foreach (var category in categories)
					{
						if (!dict.ContainsKey(category.CategoryId) && (includeSecondary || category.Default || categories.Count == 1))
							dict.Add(category.CategoryId, category.Category.Name);
					}
				}
			}

			return dict.Sort();
		}

		public CategoryDTO GetCategoryDTO(int categoryId, int actorCompanyId, bool loadCategoryRecords = false)
		{
			using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
			entities.Category.NoTracking();
			var category = GetCategory(entities, categoryId, actorCompanyId, loadCategoryRecords);
			return category?.ToDTO(loadCategoryRecords);
		}

		public Category GetCategory(int categoryId, int actorCompanyId, bool loadCategoryRecords = false)
		{
			using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
			entities.Category.NoTracking();
			return GetCategory(entities, categoryId, actorCompanyId, loadCategoryRecords);
		}

		public Category GetCategory(CompEntities entities, int categoryId, int actorCompanyId, bool loadCategoryRecords = false)
		{
			if (loadCategoryRecords)
			{
				return (from c in entities.Category
							.Include("CompanyCategoryRecord")
						where c.CategoryId == categoryId &&
						c.ActorCompanyId == actorCompanyId &&
						c.State == (int)SoeEntityState.Active
						select c).FirstOrDefault();
			}
			else
			{
				return (from c in entities.Category
						where c.CategoryId == categoryId &&
						c.ActorCompanyId == actorCompanyId &&
						c.State == (int)SoeEntityState.Active
						select c).FirstOrDefault();
			}
		}

		public Category GetCategory(CompEntities entities, string code, int type, int actorCompanyId)
		{
			return (from c in entities.Category
					where c.ActorCompanyId == actorCompanyId &&
					c.Type == type &&
					c.Code == code &&
					c.State == (int)SoeEntityState.Active
					select c).FirstOrDefault();
		}

		public Category GetCategory(SoeCategoryType type, SoeCategoryRecordEntity entity, int recordId, int actorCompanyId, DateTime? dateFrom = null, DateTime? dateTo = null, bool onlyDefaultCategory = true, bool onlyExecutive = false)
		{
			using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
			entities.Category.NoTracking();
			return GetCategory(entities, type, entity, recordId, actorCompanyId, dateFrom, dateTo, onlyDefaultCategory, onlyExecutive);
		}

		public Category GetCategory(CompEntities entities, SoeCategoryType type, SoeCategoryRecordEntity entity, int recordId, int actorCompanyId, DateTime? dateFrom = null, DateTime? dateTo = null, bool onlyDefaultCategory = true, bool onlyExecutive = false)
		{
			List<CompanyCategoryRecord> categoryRecords = GetCompanyCategoryRecords(entities, type, entity, recordId, actorCompanyId, onlyDefaultCategory, onlyExecutive: onlyExecutive);
			categoryRecords = categoryRecords.GetCategoryRecords(dateFrom, dateTo);
			return categoryRecords.FirstOrDefault()?.Category;
		}

		public Category GetPrevNextCategory(int categoryId, SoeCategoryType type, int actorCompanyId, SoeFormMode mode)
		{
			Category category = null;
			List<Category> categories = GetCategories(type, actorCompanyId);

			if (mode == SoeFormMode.Next)
			{
				category = (from c in categories
							where ((c.CategoryId > categoryId) &&
							(c.State == (int)SoeEntityState.Active))
							orderby c.CategoryId ascending
							select c).FirstOrDefault<Category>();
			}
			else
			{
				category = (from c in categories
							where ((c.CategoryId < categoryId) &&
							(c.State == (int)SoeEntityState.Active))
							orderby c.CategoryId descending
							select c).FirstOrDefault<Category>();
			}

			return category;
		}

		public string GetCategoryCode(CompEntities entities, int categoryId, int actorCompanyId)
		{
			return (from c in entities.Category
					where c.CategoryId == categoryId &&
					c.ActorCompanyId == actorCompanyId &&
					c.State == (int)SoeEntityState.Active
					select c.Code).FirstOrDefault();
		}

		public bool CategoryExists(CompEntities entities, Category category, int actorCompanyId)
		{
			return (from c in entities.Category
					where c.CategoryId != category.CategoryId &&
					c.Type == category.Type &&
					(c.Code == category.Code || c.Name == category.Name) &&
					c.State == (int)SoeEntityState.Active &&
					c.ActorCompanyId == actorCompanyId
					select c).Any();
		}

		public ActionResult HasCategoryAnyActiveCompanyCategoryRecords(CompEntities entities, Category category)
		{
			if (!category.CompanyCategoryRecord.IsLoaded)
				category.CompanyCategoryRecord.Load();

			if (category.CompanyCategoryRecord.Count == 0)
			{
				return new ActionResult() { Success = true };
			}
			var sb = new StringBuilder();
			List<String> entitys = new List<String>();

			foreach (CompanyCategoryRecord record in category.CompanyCategoryRecord)
			{
				switch (record.Entity)
				{
					case (int)SoeCategoryRecordEntity.Product:
						entitys.Add(GetText(1860, "Artiklar"));
						break;
					case (int)SoeCategoryRecordEntity.Customer:
						entitys.Add(GetText(48, "Kunder"));
						break;
					case (int)SoeCategoryRecordEntity.Supplier:
						entitys.Add(GetText(49, "Leverantörer"));
						break;
					case (int)SoeCategoryRecordEntity.ContactPerson:
						var contactPerson = ContactManager.GetContactPerson(entities, record.RecordId);
						if (contactPerson != null && contactPerson.State == (int)SoeEntityState.Active)
							entitys.Add(GetText(1588, "Kontaktpersoner"));
						break;
					case (int)SoeCategoryRecordEntity.AttestRole:
						var attestRole = AttestManager.GetAttestRole(entities, record.RecordId, category.ActorCompanyId);
						if (attestRole != null && attestRole.State == (int)SoeEntityState.Active)
							entitys.Add(GetText(5214, "Attestroller"));
						break;
					case (int)SoeCategoryRecordEntity.Employee:
						var employee = EmployeeManager.GetEmployee(entities, record.RecordId, category.ActorCompanyId);
						if (employee != null && employee.State == (int)SoeEntityState.Active)
							entitys.Add(GetText(5018, "Anställda"));
						break;
					case (int)SoeCategoryRecordEntity.Project:
						entitys.Add(GetText(3357, "Projekt"));
						break;
					case (int)SoeCategoryRecordEntity.Contract:
						entitys.Add(GetText(1, "Avtal"));
						break;
					case (int)SoeCategoryRecordEntity.Inventory:
						var intentory = InventoryManager.GetInventory(entities, record.RecordId, category.ActorCompanyId);
						if (intentory != null && intentory.State == (int)SoeEntityState.Active)
							entitys.Add(GetText(4642, "Inventering"));
						break;
					case (int)SoeCategoryRecordEntity.AttestRoleSecondary:
						var attestRoleSecondary = AttestManager.GetAttestRole(entities, record.RecordId, category.ActorCompanyId);
						if (attestRoleSecondary != null && attestRoleSecondary.State == (int)SoeEntityState.Active)
							entitys.Add(GetText(5214, "Attestroller"));
						break;
					case (int)SoeCategoryRecordEntity.ShiftType:
						var shiftType = TimeScheduleManager.GetShiftType(entities, record.RecordId);
						if (shiftType != null && shiftType.State == (int)SoeEntityState.Active)
							entitys.Add(GetText(3791, "Passtyper"));
						break;
					case (int)SoeCategoryRecordEntity.TimeTerminal:
						var timeTerminal = TimeStampManager.GetTimeTerminalDiscardState(entities, record.RecordId);
						if (timeTerminal != null && timeTerminal.State == (int)SoeEntityState.Active)
							entitys.Add(GetText(3406, "Terminaler"));
						break;
					case (int)SoeCategoryRecordEntity.Order:
						entitys.Add(GetText(3797, "Order"));
						break;
				}

			}
			if (!entitys.IsNullOrEmpty())
			{
				foreach (var ent in entitys.Distinct())
				{
					if (sb.Length != 0)
						sb.Append(", ");
					sb.Append(ent);
				}

				return new ActionResult()
				{
					Success = false,
					ErrorMessage = sb.ToString()
				};
			}

			return new ActionResult() { Success = true };
		}
		public ActionResult SaveCategory(CategoryDTO model, int actorCompanyId)
		{
			ActionResult result = new ActionResult();
			if (model.CategoryId > 0)
			{
				//update
				var category = GetCategory(model.CategoryId, actorCompanyId, false);
				if (category == null)
					return new ActionResult((int)ActionResultSave.EntityIsNull, "Category");
				category.Name = model.Name;
				category.Code = model.Code;
				category.CategoryGroupId = model.CategoryGroupId;
				category.ParentId = model.ParentId;

				result = UpdateCategory(category, actorCompanyId);
				result.IntegerValue = category.CategoryId;
			}
			else
			{
				//add
				Category category = new Category();
				category.Name = model.Name;
				category.Code = model.Code;
				category.CategoryGroupId = model.CategoryGroupId;
				category.ParentId = model.ParentId;
				category.ActorCompanyId = actorCompanyId;
				category.Type = (int)model.Type;
				result = AddCategory(category, actorCompanyId);
			}
			return result;
		}

		public ActionResult AddCategory(Category category, int actorCompanyId)
		{
			if (category == null)
				return new ActionResult((int)ActionResultSave.EntityIsNull, "Category");

			using (CompEntities entities = new CompEntities())
			{
				category.Company = CompanyManager.GetCompany(entities, actorCompanyId);
				if (category.Company == null)
					return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte"));

				if (category.ParentId.HasValue && category.ParentId.Value > 0)
				{
					category.Parent = GetCategory(entities, category.ParentId.Value, actorCompanyId);
					if (category.Parent == null)
						return new ActionResult((int)ActionResultSave.EntityNotFound, "ParentCategory");
				}

				if (CategoryExists(entities, category, actorCompanyId))
					return new ActionResult((int)ActionResultSave.CategoryExists, GetText(3364, "Kategori kunde inte sparas, kod och/eller namn finns redan på annan kategori"));
				if (!category.HasValidParentChain())
					return new ActionResult((int)ActionResultSave.CategoryInvalidChain, GetText(8720, "Kategori kunde inte sparas, vald underkategori skapar oändlig kedja av kategorier"));

				var result = AddEntityItem(entities, category, "Category");
				if (result.Success)
				{
					result.IntegerValue = category.CategoryId;
				}
				return result;
			}
		}

		public ActionResult UpdateCategory(Category category, int actorCompanyId)
		{
			if (category == null)
				return new ActionResult((int)ActionResultDelete.EntityIsNull, "Category");

			using (CompEntities entities = new CompEntities())
			{
				Category originalCategory = GetCategory(entities, category.CategoryId, actorCompanyId);
				if (originalCategory == null)
					return new ActionResult((int)ActionResultSave.EntityNotFound, "Category");

				if (CategoryExists(entities, category, actorCompanyId))
					return new ActionResult((int)ActionResultSave.CategoryExists, GetText(3364, "Kategori kunde inte sparas, kod och/eller namn finns redan på annan kategori"));

				if (originalCategory.ParentId != category.ParentId)
				{
					if (category.ParentId.HasValue)
					{
						originalCategory.Parent = GetCategory(entities, category.ParentId.Value, actorCompanyId);
						if (originalCategory.Parent == null)
							return new ActionResult((int)ActionResultSave.EntityNotFound, "ParentCategory");
					}
					else
					{
						originalCategory.Parent = null;
					}
				}

				if (!originalCategory.HasValidParentChain())
					return new ActionResult((int)ActionResultSave.CategoryInvalidChain, GetText(8720, "Kategori kunde inte sparas, vald underkategori skapar oändlig kedja av kategorier"));

				return this.UpdateEntityItem(entities, originalCategory, category, "Category");
			}
		}

		public ActionResult DeleteCategory(Category category, int actorCompanyId)
		{
			if (category == null)
				return new ActionResult((int)ActionResultDelete.EntityIsNull, "Category");

			using (CompEntities entities = new CompEntities())
			{
				Category originalCategory = GetCategory(entities, category.CategoryId, actorCompanyId);
				if (originalCategory == null)
					return new ActionResult((int)ActionResultDelete.EntityNotFound, "Category");

				//Check relation dependencies
				var result = HasCategoryAnyActiveCompanyCategoryRecords(entities, category);
				if (!result.Success)
					return new ActionResult((int)ActionResultDelete.CategoryHasCompanyCategoryRecords, $"{GetText(8758, "Kategori kunde inte tas bort, den används till")}: {result.ErrorMessage}");

				return ChangeEntityState(entities, originalCategory, SoeEntityState.Deleted, true);
			}
		}

		public ActionResult DeleteCategory(int categoryId, int actorCompanyId)
		{
			var model = GetCategory(categoryId, actorCompanyId, false);

			return DeleteCategory(model, actorCompanyId);
		}

		#endregion

		#region CompanyCategoryRecord

		public List<CompanyCategoryRecord> GetCompanyCategoryRecords(SoeCategoryType categoryType, int actorCompanyId)
		{
			using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
			entities.CompanyCategoryRecord.NoTracking();
			return GetCompanyCategoryRecords(entities, categoryType, actorCompanyId);
		}

		public List<CompanyCategoryRecord> GetCompanyCategoryRecords(CompEntities entities, SoeCategoryType categoryType, int actorCompanyId)
		{
			return (from i in entities.CompanyCategoryRecord.Include("Category")
					where i.Category.Type == (int)categoryType &&
					i.Category.ActorCompanyId == actorCompanyId &&
					i.Category.State == (int)SoeEntityState.Active
					select i).ToList();
		}

		public List<CompanyCategoryRecord> GetCompanyCategoryRecords(SoeCategoryType categoryType, SoeCategoryRecordEntity categoryRecordEntity, int actorCompanyId, List<int> categoryIds = null)
		{
			using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
			entities.CompanyCategoryRecord.NoTracking();
			return GetCompanyCategoryRecords(entities, categoryType, categoryRecordEntity, actorCompanyId, categoryIds);
		}

		public List<CompanyCategoryRecord> GetCompanyCategoryRecords(CompEntities entities, SoeCategoryType categoryType, SoeCategoryRecordEntity categoryRecordEntity, int actorCompanyId, List<int> categoryIds = null)
		{
			var companyCategoryRecords = (from i in entities.CompanyCategoryRecord.Include("Category")
										  where i.ActorCompanyId == actorCompanyId &&
										  i.Entity == (int)categoryRecordEntity &&
										  i.Category.Type == (int)categoryType &&
										  i.Category.ActorCompanyId == actorCompanyId &&
										  i.Category.State == (int)SoeEntityState.Active
										  orderby i.Default descending, i.Category.Name
										  select i).ToList();

			if (categoryIds != null)
				companyCategoryRecords = companyCategoryRecords.Where(i => categoryIds.Contains(i.CategoryId)).ToList();

			return companyCategoryRecords;
		}

		public List<CompanyCategoryRecord> GetCompanyCategoryRecords(CompEntities entities, SoeCategoryType categoryType, SoeCategoryRecordEntity categoryRecordEntity, List<int> filterRecordIds, int actorCompanyId, bool onlyDefaultCategories = false, DateTime? dateFrom = null, DateTime? dateTo = null)
		{
			List<CompanyCategoryRecord> categoryRecords = (from i in entities.CompanyCategoryRecord
																.Include("Category.Children")
														   where i.Entity == (int)categoryRecordEntity &&
														   (!onlyDefaultCategories || i.Default) &&
														   i.Category.Type == (int)categoryType &&
														   i.Category.ActorCompanyId == actorCompanyId &&
														   i.Category.State == (int)SoeEntityState.Active
														   select i).ToList();

			//Filter dates
			categoryRecords = categoryRecords.GetCategoryRecords(dateFrom, dateTo);

			return categoryRecords.Where(i => filterRecordIds.Contains(i.RecordId)).ToList();
		}

		public List<CompanyCategoryRecord> GetCompanyCategoryRecords(SoeCategoryType categoryType, SoeCategoryRecordEntity categoryRecordEntity, int recordId, int actorCompanyId, bool onlyDefaultCategories = false, DateTime? dateFrom = null, DateTime? dateTo = null, bool onlyExecutive = false)
		{
			using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
			entities.CompanyCategoryRecord.NoTracking();
			return GetCompanyCategoryRecords(entities, categoryType, categoryRecordEntity, recordId, actorCompanyId, onlyDefaultCategories, dateFrom, dateTo, onlyExecutive);
		}

		public List<CompanyCategoryRecord> GetCompanyCategoryRecords(CompEntities entities, SoeCategoryType categoryType, SoeCategoryRecordEntity categoryRecordEntity, int recordId, int actorCompanyId, bool onlyDefaultCategories = false, DateTime? dateFrom = null, DateTime? dateTo = null, bool onlyExecutive = false)
		{
			var categoryRecords = (from i in entities.CompanyCategoryRecord
									.Include("Category")
								   where i.RecordId == recordId &&
								   i.Entity == (int)categoryRecordEntity &&
								   (!onlyDefaultCategories || i.Default) &&
								   i.Category.Type == (int)categoryType &&
								   i.Category.ActorCompanyId == actorCompanyId &&
								   i.Category.State == (int)SoeEntityState.Active
								   select i).ToList();

			//Filter dates
			categoryRecords = categoryRecords.GetCategoryRecords(dateFrom, dateTo);

			//Filter Executive
			if (onlyExecutive)
				categoryRecords = categoryRecords.Where(c => c.IsExecutive).ToList();

			return categoryRecords;
		}

		public List<CompanyCategoryRecord> GetCompanyCategoryRecordsFromEmployeeKeyDict(Dictionary<string, List<CompanyCategoryRecord>> employeeKeyDict, int recordId, int categoryId, DateTime? date = null, bool discardDateIfEmpty = false, bool onlyDefaultCategories = false)
		{
			string key = CompanyCategoryRecord.ConstructFullKey((int)SoeCategoryRecordEntity.Employee, recordId, categoryId);

			employeeKeyDict.TryGetValue(key, out List<CompanyCategoryRecord> records);
			return records.GetCategoryRecords(recordId, categoryId, date: date, onlyDefaultCategories: onlyDefaultCategories, discardDateIfEmpty: discardDateIfEmpty);
		}

		public Dictionary<int, string> GetCompanyCategoryRecordsDict(CompEntities entities, SoeCategoryType categoryType, SoeCategoryRecordEntity categoryRecordEntity, int recordId, int actorCompanyId, bool onlyDefaultCategories = false, DateTime? dateFrom = null, DateTime? dateTo = null)
		{
			Dictionary<int, string> dict = new Dictionary<int, string>();

			var categoryRecords = GetCompanyCategoryRecords(entities, categoryType, categoryRecordEntity, recordId, actorCompanyId, onlyDefaultCategories, dateFrom, dateTo);
			foreach (CompanyCategoryRecord categoryRecord in categoryRecords.OrderBy(c => c.Category.Name))
			{
				if (!dict.ContainsKey(categoryRecord.CategoryId))
					dict.Add(categoryRecord.CategoryId, categoryRecord.Category.Name);
			}

			return dict;
		}

		public Dictionary<string, List<CompanyCategoryRecord>> GetCompanyCategoryRecordsFullKeyDict(List<CompanyCategoryRecord> companyCategoryRecords, SoeCategoryType categoryType, SoeCategoryRecordEntity categoryRecordEntity, int actorCompanyId, List<int> categoryIds = null)
		{
			using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
			entities.CompanyCategoryRecord.NoTracking();
			Dictionary<string, List<CompanyCategoryRecord>> dict = new Dictionary<string, List<CompanyCategoryRecord>>();
			foreach (var record in companyCategoryRecords.GroupBy(g => g.CategoryFullKey))
				dict.Add(record.Key, record.ToList());
			return dict;
		}

		public Dictionary<string, List<CompanyCategoryRecord>> GetCompanyCategoryRecordsRecordKeyDict(List<CompanyCategoryRecord> companyCategoryRecords, SoeCategoryType categoryType, SoeCategoryRecordEntity categoryRecordEntity, int actorCompanyId, List<int> categoryIds = null)
		{
			using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
			entities.CompanyCategoryRecord.NoTracking();
			Dictionary<string, List<CompanyCategoryRecord>> dict = new Dictionary<string, List<CompanyCategoryRecord>>();
			foreach (var record in companyCategoryRecords.GroupBy(g => g.CategoryRecordKey))
				dict.Add(record.Key, record.ToList());
			return dict;
		}

		public bool HasCategoryCompanyCategoryRecords(List<CompanyCategoryRecord> categoryRecordsInput, SoeCategoryRecordEntity categoryRecordEntity, int recordId, int categoryId, int actorCompanyId, DateTime? dateFrom, DateTime? dateTo = null, bool onlyExecutive = false)
		{
			var categoryRecords = (from i in categoryRecordsInput
								   where i.Entity == (int)categoryRecordEntity &&
								   i.RecordId == recordId &&
								   i.CategoryId == categoryId &&
								   i.Category != null &&
								   i.Category.State == (int)SoeEntityState.Active &&
								   i.Category.ActorCompanyId == actorCompanyId
								   select i).ToList();

			//Filter dates
			categoryRecords = categoryRecords.GetCategoryRecords(dateFrom, dateTo);

			//Is Executive
			if (onlyExecutive)
				categoryRecords = categoryRecords.Where(c => c.IsExecutive).ToList();

			return categoryRecords.Count > 0;
		}

		public ActionResult AddCompanyCategoryRecord(int recordId, int categoryId, SoeCategoryRecordEntity entity, int actorCompanyId, DateTime? dateFrom = null, DateTime? dateTo = null)
		{
			using (CompEntities entities = new CompEntities())
			{
				CompanyCategoryRecord categoryRecord = new CompanyCategoryRecord()
				{
					RecordId = recordId,
					Entity = (int)entity,
					DateFrom = dateFrom,
					DateTo = dateTo,
				};

				categoryRecord.Company = CompanyManager.GetCompany(entities, actorCompanyId);
				if (categoryRecord.Company == null)
					return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte"));

				categoryRecord.Category = GetCategory(entities, categoryId, actorCompanyId);
				if (categoryRecord.Category == null)
					return new ActionResult((int)ActionResultSave.EntityNotFound, "Category");

				return AddEntityItem(entities, categoryRecord, "CompanyCategoryRecord");
			}
		}

		public ActionResult SaveCompanyCategoryRecords(Collection<FormIntervalEntryItem> formIntervalEntryItems, int actorCompanyId, SoeCategoryType categoryType, SoeCategoryRecordEntity categoryRecordEntity, int recordId)
		{
			if (formIntervalEntryItems == null)
				return new ActionResult();

			List<TrackChangesDTO> trackChangesItems = new List<TrackChangesDTO>();
			Dictionary<int, EntityObject> tcDict = new Dictionary<int, EntityObject>();
			int tempIdCounter = 0;
			bool useTrackChanges = (categoryRecordEntity == SoeCategoryRecordEntity.AttestRole || categoryRecordEntity == SoeCategoryRecordEntity.AttestRoleSecondary);

			using (CompEntities entities = new CompEntities())
			{
				#region Prereq

				Company company = CompanyManager.GetCompany(entities, actorCompanyId);
				if (company == null)
					return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte"));

				#endregion

				#region Update/Delete CompanyCategoryRecord

				CompanyCategoryRecord categoryRecord = null;
				List<CompanyCategoryRecord> categoryRecords = GetCompanyCategoryRecords(entities, categoryType, categoryRecordEntity, recordId, actorCompanyId);
				for (int r = categoryRecords.Count - 1; r >= 0; r--)
				{
					categoryRecord = categoryRecords[r];

					//Check for existing CompanyCategoryRecord with same Category as input item
					var inputItem = formIntervalEntryItems.FirstOrDefault(i => i.From == categoryRecord.CategoryId.ToString() && i.Checked == categoryRecord.IsExecutive);
					if (inputItem != null && Convert.ToInt32(inputItem.From) > 0)
					{
						//Remove from input collection
						formIntervalEntryItems.Remove(inputItem);
					}
					else
					{
						//Delete exising TimeCodeInvocieProduct
						entities.DeleteObject(categoryRecord);
						categoryRecords.Remove(categoryRecord);

						if (useTrackChanges)
						{
							string fromValueName = entities.Category.FirstOrDefault(c => c.CategoryId == categoryRecord.CategoryId)?.Name;
							trackChangesItems.Add(TrackChangesManager.InitTrackChanges(entities, actorCompanyId, TermGroup_TrackChangesActionMethod.CommonUpdate, TermGroup_TrackChangesAction.Delete, SoeEntityType.AttestRole, recordId, GetTrackChangesEntityType(categoryRecordEntity), categoryRecord.CompanyCategoryId, SettingDataType.Integer, null, GetTrackChangesColumnType(categoryRecordEntity), categoryRecord.CategoryId.ToString(), fromValueName));
						}
					}
				}

				#endregion

				#region Add CompanyCategoryRecord

				foreach (var item in formIntervalEntryItems)
				{
					//Empty
					int categoryId = Convert.ToInt32(item.From);
					if (categoryId == 0)
						continue;

					//IsExecutive
					bool isExecutive = false;
					if (item.Checked)
						isExecutive = true;


					//Prevent duplicates
					if (categoryRecords.Count > 0)
					{
						categoryRecord = categoryRecords.FirstOrDefault(i => i.CategoryId.ToString() == item.From);
						if (categoryRecord != null)
							continue;
					}

					categoryRecord = new CompanyCategoryRecord()
					{
						RecordId = recordId,
						Entity = (int)categoryRecordEntity,
						Default = false,
						DateFrom = null,
						DateTo = null,
						IsExecutive = isExecutive,

						//Set FK
						CategoryId = categoryId,
						ActorCompanyId = actorCompanyId,
					};
					entities.CompanyCategoryRecord.AddObject(categoryRecord);

					//Add to collection to be able to check for duplicates
					categoryRecords.Add(categoryRecord);

					#region Track changes

					if (useTrackChanges)
					{
						string toValueName = entities.Category.FirstOrDefault(c => c.CategoryId == categoryId)?.Name;
						tempIdCounter++;
						trackChangesItems.Add(TrackChangesManager.InitTrackChanges(entities, actorCompanyId, TermGroup_TrackChangesActionMethod.CommonUpdate, TermGroup_TrackChangesAction.Insert, SoeEntityType.AttestRole, recordId, GetTrackChangesEntityType(categoryRecordEntity), tempIdCounter, SettingDataType.Integer, null, GetTrackChangesColumnType(categoryRecordEntity), null, categoryId.ToString(), null, toValueName));
						tcDict.Add(tempIdCounter, categoryRecord);
					}

					#endregion
				}

				#endregion

				ActionResult result = SaveChanges(entities);
				if (!result.Success)
					return result;

				#region TrackChanges

				// Add track changes
				if (useTrackChanges)
				{
					foreach (TrackChangesDTO dto in trackChangesItems.Where(t => t.Action == TermGroup_TrackChangesAction.Insert))
					{
						// Replace temp ids with actual ids created on save
						if (dto.Entity == SoeEntityType.AttestRole_PrimaryCategory || dto.Entity == SoeEntityType.AttestRole_SecondaryCategory)
						{
							CompanyCategoryRecord r = tcDict[dto.RecordId] as CompanyCategoryRecord;
							if (r != null)
								dto.RecordId = r.CompanyCategoryId;
						}
					}
					if (trackChangesItems.Any())
						result = TrackChangesManager.AddTrackChanges(entities, null, trackChangesItems);
				}

				#endregion

				return result;
			}
		}

		private SoeEntityType GetTrackChangesEntityType(SoeCategoryRecordEntity categoryRecordEntity)
		{
			return (categoryRecordEntity == SoeCategoryRecordEntity.AttestRole ? SoeEntityType.AttestRole_PrimaryCategory : SoeEntityType.AttestRole_SecondaryCategory);
		}

		private TermGroup_TrackChangesColumnType GetTrackChangesColumnType(SoeCategoryRecordEntity categoryRecordEntity)
		{
			return (categoryRecordEntity == SoeCategoryRecordEntity.AttestRole ? TermGroup_TrackChangesColumnType.AttestRole_PrimaryCategory : TermGroup_TrackChangesColumnType.AttestRole_SecondaryCategory);
		}

		public ActionResult SaveCompanyCategoryRecords(CompEntities entities, TransactionScope transaction, List<CompanyCategoryRecordDTO> companyCategoryRecordsInput, int actorCompanyId, SoeCategoryType categoryType, SoeCategoryRecordEntity categoryRecordEntity, int recordId)
		{
			//NULL meaning dont save
			if (companyCategoryRecordsInput == null)
				return new ActionResult();

			CompanyCategoryRecord categoryRecord = null;

			bool changeAffectTerminal = false;

			List<TrackChangesDTO> trackChangesItems = new List<TrackChangesDTO>();
			Dictionary<int, EntityObject> tcDict = new Dictionary<int, EntityObject>();
			int tempIdCounter = 0;
			bool useTrackChanges = (categoryRecordEntity == SoeCategoryRecordEntity.AttestRole || categoryRecordEntity == SoeCategoryRecordEntity.AttestRoleSecondary);

			#region Update/Delete CompanyCategoryRecord

			List<CompanyCategoryRecord> categoryRecords = GetCompanyCategoryRecords(entities, categoryType, categoryRecordEntity, recordId, actorCompanyId);
			for (int r = categoryRecords.Count - 1; r >= 0; r--)
			{
				categoryRecord = categoryRecords[r];

				var inputItem = companyCategoryRecordsInput.FirstOrDefault(i => i.CategoryId == categoryRecord.CategoryId);
				if (inputItem != null && inputItem.CategoryId > 0)
				{
					if (categoryRecord.Default != inputItem.Default || categoryRecord.DateFrom != inputItem.DateFrom || categoryRecord.DateTo != inputItem.DateTo)
						changeAffectTerminal = true;

					// Update values
					categoryRecord.Default = inputItem.Default;
					categoryRecord.DateFrom = inputItem.DateFrom;
					categoryRecord.DateTo = inputItem.DateTo;
					categoryRecord.IsExecutive = inputItem.IsExecutive;

					// Remove from input collection
					companyCategoryRecordsInput.Remove(inputItem);
				}
				else
				{
					// Delete all existing categoryrecords
					entities.DeleteObject(categoryRecord);
					categoryRecords.Remove(categoryRecord);

					changeAffectTerminal = true;

					if (useTrackChanges)
					{
						string fromValueName = entities.Category.FirstOrDefault(c => c.CategoryId == categoryRecord.CategoryId)?.Name;
						trackChangesItems.Add(TrackChangesManager.InitTrackChanges(entities, actorCompanyId, TermGroup_TrackChangesActionMethod.CommonUpdate, TermGroup_TrackChangesAction.Delete, SoeEntityType.AttestRole, recordId, GetTrackChangesEntityType(categoryRecordEntity), categoryRecord.CompanyCategoryId, SettingDataType.Integer, null, GetTrackChangesColumnType(categoryRecordEntity), categoryRecord.CategoryId.ToString(), fromValueName));
					}
				}
			}

			#endregion

			#region Add CompanyCategoryRecord

			foreach (var categoryRecordInput in companyCategoryRecordsInput)
			{
				//Prevent empty
				int categoryId = Convert.ToInt32(categoryRecordInput.CategoryId);
				if (categoryId == 0)
					continue;

				categoryRecord = new CompanyCategoryRecord()
				{
					RecordId = recordId,
					Entity = (int)categoryRecordEntity,
					Default = categoryRecordInput.Default,
					DateFrom = categoryRecordInput.DateFrom,
					DateTo = categoryRecordInput.DateTo,
					IsExecutive = categoryRecordInput.IsExecutive,

					//Set FK
					CategoryId = categoryId,
					ActorCompanyId = actorCompanyId,
				};
				entities.CompanyCategoryRecord.AddObject(categoryRecord);
				//Add to collection to be able to check for duplicates
				categoryRecords.Add(categoryRecord);

				changeAffectTerminal = true;

				#region Track changes

				if (useTrackChanges)
				{
					string toValueName = entities.Category.FirstOrDefault(c => c.CategoryId == categoryId)?.Name;
					tempIdCounter++;
					trackChangesItems.Add(TrackChangesManager.InitTrackChanges(entities, actorCompanyId, TermGroup_TrackChangesActionMethod.CommonUpdate, TermGroup_TrackChangesAction.Insert, SoeEntityType.AttestRole, recordId, GetTrackChangesEntityType(categoryRecordEntity), tempIdCounter, SettingDataType.Integer, null, GetTrackChangesColumnType(categoryRecordEntity), null, categoryId.ToString(), null, toValueName));
					tcDict.Add(tempIdCounter, categoryRecord);
				}

				#endregion
			}

			#endregion

			ActionResult result = SaveChanges(entities, transaction);
			if (!result.Success)
				return result;

			#region TrackChanges

			// Add track changes
			if (useTrackChanges)
			{
				foreach (TrackChangesDTO dto in trackChangesItems.Where(t => t.Action == TermGroup_TrackChangesAction.Insert))
				{
					// Replace temp ids with actual ids created on save
					if (dto.Entity == SoeEntityType.AttestRole_PrimaryCategory || dto.Entity == SoeEntityType.AttestRole_SecondaryCategory)
					{
						CompanyCategoryRecord r = tcDict[dto.RecordId] as CompanyCategoryRecord;
						if (r != null)
							dto.RecordId = r.CompanyCategoryId;
					}
				}
				if (trackChangesItems.Any())
					result = TrackChangesManager.AddTrackChanges(entities, null, trackChangesItems);
			}

			#endregion

			if (changeAffectTerminal)
				result.BooleanValue2 = true;

			return result;
		}

		public ActionResult SaveCompanyCategoryRecords(CompEntities entities, TransactionScope transaction, List<int> categoryIds, int actorCompanyId, SoeCategoryType categoryType, SoeCategoryRecordEntity categoryRecordEntity, int recordId)
		{
			// Simple save of CompanyCategoryRecords where only CategoryId is used

			if (categoryIds == null)
				return new ActionResult();

			#region Update/Delete CompanyCategoryRecord

			// Loop over existing categories
			List<CompanyCategoryRecord> categoryRecords = GetCompanyCategoryRecords(entities, categoryType, categoryRecordEntity, recordId, actorCompanyId);
			foreach (CompanyCategoryRecord categoryRecord in categoryRecords.ToList())
			{
				if (categoryIds.Contains(categoryRecord.CategoryId))
				{
					// Category still exists in input, keep it and remove it from input collection to prevet adding it again below
					categoryIds.Remove(categoryRecord.CategoryId);
				}
				else
				{
					// Category does not exist in input, delete it
					entities.DeleteObject(categoryRecord);
					categoryRecords.Remove(categoryRecord);
				}
			}

			#endregion

			#region Add CompanyCategoryRecord

			foreach (int categoryId in categoryIds)
			{
				CompanyCategoryRecord categoryRecord = new CompanyCategoryRecord()
				{
					ActorCompanyId = actorCompanyId,
					CategoryId = categoryId,
					RecordId = recordId,
					Entity = (int)categoryRecordEntity,
				};
				entities.CompanyCategoryRecord.AddObject(categoryRecord);
			}

			#endregion

			return SaveChanges(entities, transaction);
		}

		#endregion

		#region CategoryAccount

		public List<CategoryAccount> GetCategoryAccountsByCompany(CompEntities entities, int actorCompanyId)
		{
			return (from ca in entities.CategoryAccount
					where ca.ActorCompanyId == actorCompanyId &&
					ca.State == (int)SoeEntityState.Active
					select ca).ToList();
		}

		public List<CategoryAccount> GetCategoryAccountsByAccount(int accountId, int actorCompanyId, bool loadCategory = false)
		{
			using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
			entities.CategoryAccount.NoTracking();
			return GetCategoryAccountsByAccount(entities, accountId, actorCompanyId, loadCategory);
		}

		public List<CategoryAccount> GetCategoryAccountsByAccount(CompEntities entities, int accountId, int actorCompanyId, bool loadCategory = false)
		{
			var query = (from ca in entities.CategoryAccount
						 where ca.AccountId == accountId &&
						 ca.ActorCompanyId == actorCompanyId &&
						 ca.State == (int)SoeEntityState.Active
						 select ca);

			if (loadCategory)
				query = query.Include("Category");

			return query.ToList();
		}

		public ActionResult SaveCategoryAccounts(Collection<FormIntervalEntryItem> formIntervalEntryItems, int accountId, int actorCompanyId)
		{
			if (formIntervalEntryItems == null || formIntervalEntryItems.Count == 0)
				return new ActionResult();

			using (CompEntities entities = new CompEntities())
			{
				#region Update/Delete CategoryAccount

				CategoryAccount categoryAccount = null;
				List<CategoryAccount> categoryAccounts = GetCategoryAccountsByAccount(entities, accountId, actorCompanyId);
				for (int ca = categoryAccounts.Count - 1; ca >= 0; ca--)
				{
					categoryAccount = categoryAccounts[ca];

					var inputItem = formIntervalEntryItems.FirstOrDefault(i => i.From == categoryAccount.CategoryId.ToString());
					if (inputItem != null && Convert.ToInt32(inputItem.From) > 0)
					{
						//Remove from input collection
						formIntervalEntryItems.Remove(inputItem);
					}
					else
					{
						//Delete exising TimeCodeInvoiceProduct
						entities.DeleteObject(categoryAccount);
						categoryAccounts.Remove(categoryAccount);
					}
				}

				#endregion

				#region Add CompanyCategoryRecord

				foreach (var item in formIntervalEntryItems)
				{
					//Prevent empty
					int categoryId = Convert.ToInt32(item.From);
					if (categoryId == 0)
						continue;

					//Prevent duplicates
					if (categoryAccounts.Count > 0)
					{
						categoryAccount = categoryAccounts.FirstOrDefault(i => i.CategoryId.ToString() == item.From);
						if (categoryAccount != null)
							continue;
					}

					categoryAccount = new CategoryAccount()
					{
						DateFrom = null,
						DateTo = null,
						State = (int)SoeEntityState.Active,

						//Set FK
						CategoryId = categoryId,
						AccountId = accountId,
						ActorCompanyId = actorCompanyId,
					};

					//Add to collection to be able to check for duplicates
					categoryAccounts.Add(categoryAccount);

					entities.CategoryAccount.AddObject(categoryAccount);
				}

				#endregion

				return SaveChanges(entities);
			}
		}

		#endregion

		#region CategoryGroup

		public List<CategoryGroup> GetCategoryGroups(int actorCompanyId, SoeCategoryType type, bool loadAll)
		{
			using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
			entities.CategoryGroup.NoTracking();
			return GetCategoryGroups(entities, type, actorCompanyId, loadAll);
		}

		public List<CategoryGroup> GetCategoryGroups(SoeCategoryType type, int actorCompanyId)
		{
			using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
			entities.CategoryGroup.NoTracking();
			return GetCategoryGroups(entities, type, actorCompanyId);
		}

		public List<CategoryGroup> GetCategoryGroups(CompEntities entities, SoeCategoryType type, int actorCompanyId, bool loadAll = false)
		{
			if (loadAll)
			{
				List<CategoryGroup> categoryGroups = (from c in entities.CategoryGroup
													  where c.ActorCompanyId == actorCompanyId &&
													  c.State == (int)SoeEntityState.Active
													  orderby c.Name
													  select c).ToList();
				return categoryGroups;
			}
			else
			{
				List<CategoryGroup> categoryGroups = (from c in entities.CategoryGroup
													  where c.ActorCompanyId == actorCompanyId &&
													  (c.Type == (int)type || c.Type == 0) &&
													  c.State == (int)SoeEntityState.Active
													  orderby c.Name
													  select c).ToList();
				return categoryGroups;
			}

		}

		public Dictionary<int, string> GetCategoryGroupsDict(SoeCategoryType type, int actorCompanyId, bool addEmptyRow)
		{
			var dict = new Dictionary<int, string>();
			if (addEmptyRow)
				dict.Add(0, " ");

			var categoryGroups = GetCategoryGroups(type, actorCompanyId);
			foreach (var group in categoryGroups)
			{
				if (!dict.ContainsKey(group.CategoryGroupId))
					dict.Add(group.CategoryGroupId, group.Name);
			}
			return dict;
		}

		public CategoryGroup GetCategoryGroupById(CompEntities entities, int categoryGroupId)
		{
			CategoryGroup categoryGroup = (from c in entities.CategoryGroup
										   where c.CategoryGroupId == categoryGroupId
										   select c).FirstOrDefault();
			return categoryGroup;

		}

		public ActionResult SaveCategoryGroup(CompEntities entities, CategoryGroupDTO categoryGroupInput)
		{
			if (categoryGroupInput == null)
				return new ActionResult((int)ActionResultSave.EntityNotFound, "CategoryGroup");

			CategoryGroup categoryGroup;
			if (categoryGroupInput.CategoryGroupId > 0)
			{
				categoryGroup = GetCategoryGroupById(entities, categoryGroupInput.CategoryGroupId);
				if (categoryGroup == null)
					return new ActionResult((int)ActionResultSave.EntityNotFound, "CategoryGroup");

				categoryGroup.ActorCompanyId = categoryGroupInput.ActorCompanyId;
				categoryGroup.Name = categoryGroupInput.Name;
				categoryGroup.Type = (int)categoryGroupInput.Type;
				categoryGroup.State = (int)categoryGroupInput.State;
				SetModifiedProperties(categoryGroup);
			}
			else
			{
				CategoryGroup NewCategoryGroup = new CategoryGroup()
				{
					ActorCompanyId = categoryGroupInput.ActorCompanyId,
					Name = categoryGroupInput.Name,
					Type = (int)categoryGroupInput.Type
				};
				SetCreatedProperties(NewCategoryGroup);
				entities.CategoryGroup.AddObject(NewCategoryGroup);
			}

			return SaveChanges(entities);
		}

        #endregion

        #region CategoryType

		public List<SmallGenericType> GetCategoryTypeByPermission()
		{
            var langId = base.GetLangId();
			var types = base.GetTermGroupDict(TermGroup.CategoryType, langId);
			var dtos = new List<SmallGenericType>();

            foreach (var type in types)
			{
				var feature = Feature.None;

                switch ((SoeCategoryType)type.Key)
				{
					case SoeCategoryType.Project:
						feature = Feature.Common_Categories_Project;
						break;
					case SoeCategoryType.Customer:
						feature = Feature.Common_Categories_Customer;
						break;
					case SoeCategoryType.Product:
						feature = Feature.Common_Categories_Product;
						break;
					case SoeCategoryType.Supplier:
						feature = Feature.Common_Categories_Supplier;
						break;
					case SoeCategoryType.ContactPerson:
						feature = Feature.Common_Categories_ContactPersons;
						break;
					case SoeCategoryType.AttestRole:
						feature = Feature.Common_Categories_AttestRole;
						break;
					case SoeCategoryType.Employee:
						feature = Feature.Common_Categories_Employee;
						break;
					case SoeCategoryType.Contract:
						feature = Feature.Common_Categories_Contract;
						break;
					case SoeCategoryType.Inventory:
						feature = Feature.Common_Categories_Inventory;
						break;
					case SoeCategoryType.Order:
						feature = Feature.Common_Categories_Order;
						break;
					case SoeCategoryType.PayrollProduct:
						feature = Feature.Common_Categories_PayrollProduct;
						break;
                    case SoeCategoryType.Dokument:
                        feature = Feature.Common_Categories_Document;
                        break;
                }

                if (feature != Feature.None && FeatureManager.HasRolePermission(feature, Permission.Modify, this.RoleId, base.ActorCompanyId))
				{
                    dtos.Add(new SmallGenericType()
					{
						Id = type.Key,
						Name = type.Value
					});
                }
            }

			return dtos.OrderBy(x=> x.Name).ToList();
        }

        #endregion
    }
}

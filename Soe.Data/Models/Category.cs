using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data.Util;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace SoftOne.Soe.Data
{
    public partial class Category : ICreatedModified, IState
    {
        public string ChildrenNamesString { get; set; }
        public string CategoryGroupName { get; set; }
    }

    public partial class CategoryAccount : ICreatedModified, IState
    {

    }

    public partial class CategoryGroup : ICreatedModified, IState
    {
        public string TypeName { get; set; }
    }

    public partial class CompanyCategoryRecord : IEmployeeAuthModel
    {
        private string categoryFullKey;
        public string CategoryFullKey
        {
            get
            {
                if (!string.IsNullOrEmpty(this.categoryFullKey))
                    return this.categoryFullKey;

                this.categoryFullKey = ConstructFullKey(this.Entity, this.RecordId, this.CategoryId);
                return this.categoryFullKey;
            }
        }

        public static string ConstructFullKey(int entity, int recordId, int categoryId)
        {
            return $"{entity}#{recordId}#{categoryId}#";
        }

        private string categoryRecordKey;
        public string CategoryRecordKey
        {
            get
            {
                if (!string.IsNullOrEmpty(this.categoryRecordKey))
                    return this.categoryRecordKey;

                this.categoryRecordKey = ConstructKey(this.Entity, this.RecordId);
                return this.categoryRecordKey;
            }
        }

        public static string ConstructKey(int entity, int recordId)
        {
            return $"{entity}#{recordId}#";
        }
    }

    public static partial class EntityExtensions
    {
        #region Category

        public static List<Category> GetParents(this Category e, out bool valid)
        {
            List<Category> parents = new List<Category>();
            Category category = e;
            valid = true;

            while (valid && category != null)
            {
                if (category.Parent == null && category.EntityState != EntityState.Added && !category.ParentReference.IsLoaded)
                {
                    category.ParentReference.Load();
                    DataProjectLogCollector.LogLoadedEntityInExtension("Category.cs category.ParentReference");
                }

                if (category.Parent != null && parents.Any(i => i.CategoryId == category.Parent.CategoryId))
                    valid = false;
                else if (category.Parent != null)
                    parents.Add(category.Parent);

                category = category.Parent;
            }

            return parents;
        }

        public static void GetChildren(this Category e, ref List<Category> children, out bool valid)
        {
            if (children == null)
                children = new List<Category>();
            valid = true;

            if (!e.Children.IsLoaded)
            {
                e.Children.Load();
                DataProjectLogCollector.LogLoadedEntityInExtension("Category.cs category.ParentReference");
            }

            foreach (Category child in e.Children)
            {
                if (children.Any(i => i.CategoryId == child.CategoryId))
                {
                    valid = false;
                    break;
                }
                else
                {
                    children.Add(child);
                    child.GetChildren(ref children, out valid);
                }
            }
        }

        public static IEnumerable<CategoryGridDTO> ToGridDTOs(this IEnumerable<Category> l)
        {
            var dtos = new List<CategoryGridDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToGridDTO());
                }
            }
            return dtos;
        }

        public static CategoryGridDTO ToGridDTO(this Category e)
        {
            if (e == null)
                return null;

            CategoryGridDTO dto = new CategoryGridDTO()
            {
                CategoryId = e.CategoryId,                
                Code = e.Code,
                Name = e.Name
            };

            if (e.Children != null && e.HasValidChildrenChain())
                dto.ChildrenNamesString = e.Children.Where(c => c.State == (int)SoeEntityState.Active).Select(i => i.Name).Distinct().ToCommaSeparated();

            if (e.CategoryGroup != null && !String.IsNullOrEmpty(e.CategoryGroup.Name))
                dto.CategoryGroupName = e.CategoryGroup.Name;

            return dto;
        }


        public static CategoryDTO ToDTO(this Category e, bool includeCompanyCategoryRecords)
        {
            if (e == null)
                return null;

            #region Try load

            try
            {
                if (!e.IsAdded() && includeCompanyCategoryRecords && !e.CompanyCategoryRecord.IsLoaded)
                {
                    e.CompanyCategoryRecord.Load();
                    DataProjectLogCollector.LogLoadedEntityInExtension("Category.cs e.CompanyCategoryRecord");
                }
            }
            catch (InvalidOperationException ex) { ex.ToString(); }

            #endregion

            CategoryDTO dto = new CategoryDTO()
            {
                CategoryId = e.CategoryId,
                ActorCompanyId = e.ActorCompanyId,
                ParentId = e.ParentId,
                Type = (SoeCategoryType)e.Type,
                Code = e.Code,
                Name = e.Name,
                State = (SoeEntityState)e.State,
                CategoryGroupId = e.CategoryGroupId,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy
            };

            if (includeCompanyCategoryRecords)
                dto.CompanyCategoryRecords = (e.CompanyCategoryRecord != null && e.CompanyCategoryRecord.Count > 0) ? e.CompanyCategoryRecord.ToDTOs(false).ToList() : new List<CompanyCategoryRecordDTO>();

            dto.Children = new List<CategoryDTO>();
            if (e.Children != null && e.Children.IsLoaded && e.Children.Count > 0 && e.HasValidChildrenChain())
            {
                foreach (var childCategory in e.Children)
                {
                    dto.Children.Add(childCategory.ToDTO(includeCompanyCategoryRecords));
                }
            }

            dto.ChildrenNamesString = e.ChildrenNamesString;
            dto.CategoryGroupName = e.CategoryGroupName;

            return dto;
        }

        public static IEnumerable<CategoryDTO> ToDTOs(this IEnumerable<Category> l, bool includeCompanyCategoryRecords)
        {
            var dtos = new List<CategoryDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO(includeCompanyCategoryRecords));
                }
            }
            return dtos;
        }

        public static List<Category> GetSubCategories(this List<Category> l, int parentCategoryId)
        {
            return l.Where(i => i.ParentId.HasValue && i.ParentId.Value == parentCategoryId).ToList();
        }

        public static List<Category> GetTopCategories(this List<Category> l)
        {
            List<Category> validCategories = l.Where(i => !i.ParentId.HasValue).ToList();
            if (!validCategories.Any())
            {
                //Will not support cases where categories has different parentids (ex: Syd-Malmö, Norr-Sundsvall)
                List<int> parentIds = l.Where(i => i.ParentId.HasValue).Select(i => i.ParentId.Value).Distinct().ToList();
                if (parentIds.Count == 1)
                    validCategories.AddRange(l);
            }
            return validCategories;
        }

        public static CategoryComboDTO GetCategoryComboDTO(this List<Category> l)
        {
            CategoryComboDTO dto = new CategoryComboDTO();

            if (l.IsNullOrEmpty())
                return dto;

            int position = 1;
            foreach (var category in l.OrderBy(c => c.Code))
            {
                dto.SetCategoryValues(position, category.CategoryId, category.Code, category.Name);
                position++;
            }

            return dto;
        }

        public static bool HasValidParentChain(this Category e)
        {
            e.GetParents(out bool valid);
            return valid;
        }

        public static bool HasValidChildrenChain(this Category e)
        {
            List<Category> children = new List<Category>();
            e.GetChildren(ref children, out bool valid);
            return valid;
        }

        #endregion

        #region CompanyCategoryRecord

        public static CompanyCategoryRecordDTO ToDTO(this CompanyCategoryRecord e, bool includeCategory)
        {
            if (e == null)
                return null;

            CompanyCategoryRecordDTO dto = new CompanyCategoryRecordDTO()
            {
                CompanyCategoryId = e.CompanyCategoryId,
                ActorCompanyId = e.Company != null ? e.ActorCompanyId : 0,
                CategoryId = e.Category != null ? e.CategoryId : 0,
                Entity = (SoeCategoryRecordEntity)e.Entity,
                RecordId = e.RecordId,
                Default = e.Default,
                DateFrom = e.DateFrom,
                DateTo = e.DateTo,
                IsExecutive = e.IsExecutive
            };

            if (includeCategory && e.Category != null)
                dto.Category = e.Category.ToDTO(false);

            return dto;
        }

        public static IEnumerable<CompanyCategoryRecordDTO> ToDTOs(this IEnumerable<CompanyCategoryRecord> l, bool includeCategory)
        {
            var dtos = new List<CompanyCategoryRecordDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO(includeCategory));
                }
            }
            return dtos;
        }

        public static Dictionary<int, List<CompanyCategoryRecord>> ToDict(this IEnumerable<CompanyCategoryRecord> l)
        {
            Dictionary<int, List<CompanyCategoryRecord>> dict = new Dictionary<int, List<CompanyCategoryRecord>>();
            if (l != null)
            {
                foreach (var grouping in l.GroupBy(g => g.RecordId))
                    dict.Add(grouping.Key, grouping.ToList());
            }
            return dict;
        }

        public static List<CompanyCategoryRecord> GetCategoryRecordsDiscardDates(this IEnumerable<CompanyCategoryRecord> l, SoeCategoryRecordEntity entity, bool onlyDefaultCategories = false)
        {
            if (l.IsNullOrEmpty())
                return new List<CompanyCategoryRecord>();

            int entityId = (int)entity;
            return l.Where(i => i.Entity == entityId && (!onlyDefaultCategories || i.Default)).ToList();
        }

        public static List<CompanyCategoryRecord> GetCategoryRecords(this IEnumerable<CompanyCategoryRecord> l, int recordId, DateTime dateFrom, DateTime dateTo, bool onlyDefaultCategories = false)
        {
            if (l.IsNullOrEmpty())
                return new List<CompanyCategoryRecord>();

            return l.Where(i => i.RecordId == recordId && (!onlyDefaultCategories || i.Default)).GetCategoryRecords(dateFrom, dateTo);
        }

        public static List<CompanyCategoryRecord> GetCategoryRecords(this IEnumerable<CompanyCategoryRecord> l, int recordId, DateTime? date = null, bool discardDateIfEmpty = false, bool onlyDefaultCategories = false)
        {
            if (l.IsNullOrEmpty())
                return new List<CompanyCategoryRecord>();
            return l.Where(i => i.RecordId == recordId && (!onlyDefaultCategories || i.Default)).GetCategoryRecords(date, discardDateIfEmpty);
        }

        public static List<CompanyCategoryRecord> GetCategoryRecords(this IEnumerable<CompanyCategoryRecord> l, int recordId, int categoryId, DateTime dateFrom, DateTime dateTo, bool onlyDefaultCategories = false)
        {
            if (l.IsNullOrEmpty())
                return new List<CompanyCategoryRecord>();
            return l.Where(i => i.RecordId == recordId && i.CategoryId == categoryId && (!onlyDefaultCategories || i.Default)).GetCategoryRecords(dateFrom, dateTo);
        }

        public static List<CompanyCategoryRecord> GetCategoryRecords(this IEnumerable<CompanyCategoryRecord> l, int recordId, int categoryId, DateTime? date = null, bool onlyDefaultCategories = false, bool discardDateIfEmpty = false)
        {
            if (l.IsNullOrEmpty())
                return new List<CompanyCategoryRecord>();
            return l.Where(i => i.RecordId == recordId && i.CategoryId == categoryId && (!onlyDefaultCategories || i.Default)).GetCategoryRecords(date, discardDateIfEmpty);
        }

        public static List<CompanyCategoryRecord> GetCategoryRecords(this IEnumerable<CompanyCategoryRecord> l, SoeCategoryRecordEntity entity, DateTime dateFrom, DateTime dateTo, bool onlyDefaultCategories = false)
        {
            if (l.IsNullOrEmpty())
                return new List<CompanyCategoryRecord>();

            int entityId = (int)entity;
            return l.Where(i => i.Entity == entityId && (!onlyDefaultCategories || i.Default)).GetCategoryRecords(dateFrom, dateTo);
        }

        public static List<CompanyCategoryRecord> GetCategoryRecords(this IEnumerable<CompanyCategoryRecord> l, SoeCategoryRecordEntity entity, DateTime? date = null, bool onlyDefaultCategories = false, bool discardDateIfEmpty = false)
        {
            if (l.IsNullOrEmpty())
                return new List<CompanyCategoryRecord>();

            int entityId = (int)entity;
            return l.Where(i => i.Entity == entityId && (!onlyDefaultCategories || i.Default)).GetCategoryRecords(date, discardDateIfEmpty);
        }

        public static List<CompanyCategoryRecord> GetCategoryRecords(this IEnumerable<CompanyCategoryRecord> l, SoeCategoryRecordEntity entity, int recordId, DateTime dateFrom, DateTime dateTo, bool onlyDefaultCategories = false)
        {
            if (l.IsNullOrEmpty())
                return new List<CompanyCategoryRecord>();

            int entityId = (int)entity;
            return l.Where(i => i.RecordId == recordId && i.Entity == entityId && (!onlyDefaultCategories || i.Default)).GetCategoryRecords(dateFrom, dateTo);
        }

        public static List<CompanyCategoryRecord> GetCategoryRecords(this IEnumerable<CompanyCategoryRecord> l, SoeCategoryRecordEntity entity, int recordId, DateTime? date = null, bool onlyDefaultCategories = false, bool discardDateIfEmpty = false)
        {
            if (l.IsNullOrEmpty())
                return new List<CompanyCategoryRecord>();

            int entityId = (int)entity;
            return l.Where(i => i.RecordId == recordId && i.Entity == entityId && (!onlyDefaultCategories || i.Default)).GetCategoryRecords(date, discardDateIfEmpty);
        }

        public static List<CompanyCategoryRecord> GetCategoryRecords(this IEnumerable<CompanyCategoryRecord> l, DateTime? dateFrom, DateTime? dateTo)
        {
            if (l.IsNullOrEmpty())
                return new List<CompanyCategoryRecord>();

            CalendarUtility.MinAndMaxToNull(ref dateFrom, ref dateTo);
            if (!dateFrom.HasValue && !dateTo.HasValue)
                return l.ToList();

            CalendarUtility.NullToToday(ref dateFrom, ref dateTo);
            List<CompanyCategoryRecord> filtered = l.Where(i => !i.DateTo.HasValue || i.DateTo.Value >= dateFrom).OrderBy(i => i.DateFrom).ToList();
            if (filtered.IsNullOrEmpty())
                return new List<CompanyCategoryRecord>();

            dateFrom = CalendarUtility.GetLatestDate(dateFrom.Value, filtered.First().DateFrom);

            return (from e in filtered
                    where CalendarUtility.IsDatesOverlapping(dateFrom.Value, dateTo.Value, e.DateFrom ?? DateTime.MinValue, e.DateTo ?? DateTime.MaxValue, validateDatesAreTouching: true)
                    orderby e.DateFrom
                    select e).ToList();
        }

        public static List<CompanyCategoryRecord> GetCategoryRecords(this IEnumerable<CompanyCategoryRecord> l, DateTime? date, bool discardDateIfEmpty = false)
        {
            if (l.IsNullOrEmpty())
                return new List<CompanyCategoryRecord>();

            if (!date.HasValue)
            {
                if (discardDateIfEmpty)
                    return l.ToList();

                date = DateTime.Today;
            }

            date = date.Value.Date;

            return (from e in l
                    where (!e.DateFrom.HasValue || e.DateFrom.Value.Date <= date) &&
                    (!e.DateTo.HasValue || e.DateTo.Value.Date >= date)
                    orderby e.DateFrom
                    select e).ToList();
        }

        public static List<CompanyCategoryRecord> GetCategoryRecords(this Dictionary<int, List<CompanyCategoryRecord>> dict, int recordId, DateTime dateFrom, DateTime dateTo)
        {
            if (dict != null && dict.ContainsKey(recordId))
                return dict[recordId].GetCategoryRecords(dateFrom, dateTo);
            else
                return new List<CompanyCategoryRecord>();
        }

        public static void GetCodeAndName(this List<CompanyCategoryRecord> l, out string code, out string name)
        {
            int count = l?.Count ?? 0;
            if (count == 1)
            {
                code = l.First().Category.Code;
                name = l.First().Category.Name;
            }
            else if (count > 1)
            {
                code = "*";
                name = "*";
            }
            else
            {
                code = "";
                name = "";
            }
        }

        public static List<string> GetCategoryNames(this IEnumerable<CompanyCategoryRecord> l)
        {
            return l?.Select(i => i.Category.Name).Distinct().OrderBy(i => i).ToList() ?? new List<string>();
        }

        public static string GetCategoryNamesString(this IEnumerable<CompanyCategoryRecord> l)
        {
            var names = GetCategoryNames(l).Where(w => !string.IsNullOrEmpty(w)).Select(s => s.Trim()).ToList();
            return string.Join(",", names);
        }

        public static string GetCategoryName(this IEnumerable<CompanyCategoryRecord> l, string defaultIfMany = "*")
        {
            if (l.IsNullOrEmpty())
                return String.Empty;
            return l.Count() == 1 ? l.FirstOrDefault()?.Category?.Name : defaultIfMany;
        }

        public static string GetCategoryCode(this IEnumerable<CompanyCategoryRecord> l, string defaultIfMany = "*")
        {
            if (l.IsNullOrEmpty())
                return String.Empty;
            return l.Count() == 1 ? l.FirstOrDefault()?.Category?.Code : defaultIfMany;
        }

        public static bool ContainsAny(this List<CompanyCategoryRecord> l, List<CompanyCategoryRecord> otherRecords)
        {
            foreach (var e in l)
            {
                if (otherRecords.Any(i => i.CategoryId == e.CategoryId))
                    return true;
            }
            return false;
        }

        public static bool HasAnyCategory(this Dictionary<int, List<CompanyCategoryRecord>> dict, int recordId, List<int> categoryIds, DateTime dateFrom, DateTime dateTo)
        {
            if (dict != null && dict.ContainsKey(recordId))
                return dict[recordId].HasAnyCategory(recordId, categoryIds, dateFrom, dateTo);
            else
                return false;
        }

        public static bool HasAnyCategory(this IEnumerable<CompanyCategoryRecord> l, int recordId, List<int> categoryIds, DateTime dateFrom, DateTime dateTo)
        {
            foreach (int categoryId in categoryIds)
            {
                if (l.GetCategoryRecords(recordId, categoryId, dateFrom, dateTo, onlyDefaultCategories: false).Any())
                    return true;
            }
            return false;
        }

        public static bool IsDateValid(this CompanyCategoryRecord e, DateTime date)
        {
            return (!e.DateFrom.HasValue || e.DateFrom.Value <= date) && (!e.DateTo.HasValue || e.DateTo.Value >= date);
        }

        #endregion

        #region CategoryGroup

        public static CategoryGroupDTO ToDTO(this CategoryGroup e)
        {
            if (e == null)
                return null;

            return new CategoryGroupDTO()
            {
                CategoryGroupId = e.CategoryGroupId,
                ActorCompanyId = e.ActorCompanyId,
                Type = (SoeCategoryType)e.Type,
                Code = e.Code,
                Name = e.Name,
                TypeName = e.TypeName,
                State = (SoeEntityState)e.State,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy
            };
        }

        public static IEnumerable<CategoryGroupDTO> ToDTOs(this IEnumerable<CategoryGroup> l)
        {
            var dtos = new List<CategoryGroupDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        #endregion

        #region CategoryAccount

        public static CategoryAccountDTO ToDTO(this CategoryAccount e)
        {
            if (e == null)
                return null;

            return new CategoryAccountDTO()
            {
                CategoryAccountId = e.CategoryAccountId,
                CategoryId = e.CategoryId,
                AccountId = e.AccountId,
                ActorCompanyId = e.ActorCompanyId,
                DateFrom = e.DateFrom,
                DateTo = e.DateTo,
                State = (SoeEntityState)e.State,
            };
        }

        public static IEnumerable<CategoryAccountDTO> ToDTOs(this IEnumerable<CategoryAccount> l)
        {
            var dtos = new List<CategoryAccountDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        #endregion
    }
}

using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Business.Util
{
    public class EmployeeAuthModel
    {
        public bool UseAccountHierarchy { get; }
        public AccountRepository AccountRepository { get; private set; }
        public CategoryRepository CategoryRepository { get; private set; }
        public List<int> FilterIds { get; }

        private EmployeeAuthModel(AccountRepository accountRepository, List<int> filterIds = null)
        {
            this.UseAccountHierarchy = true;
            this.AccountRepository = accountRepository;
            this.FilterIds = filterIds;
        }

        private EmployeeAuthModel(CategoryRepository categoryRepository, List<int> filterIds = null)
        {
            this.UseAccountHierarchy = false;
            this.CategoryRepository = categoryRepository;
            this.FilterIds = filterIds;
        }

        public List<AttestRoleUser> GetAttestRoleUsers()
        {
            return (this.UseAccountHierarchy ? this.AccountRepository?.AttestRoleUsers : this.CategoryRepository?.AttestRoleUsers)
                ?? new List<AttestRoleUser>();
        }

        public List<CompanyCategoryRecord> GetCategoryRecords()
        {
            if (this.UseAccountHierarchy)
                return new List<CompanyCategoryRecord>();

            return this.CategoryRepository?.CategoryRecords?
                .Where(i => i.Entity == (int)SoeCategoryRecordEntity.Employee)
                .ToList() ?? new List<CompanyCategoryRecord>();
        }

        public static EmployeeAuthModel Create(EmployeeAuthModelRepository repository, List<int> filterIds)
        {
            if (repository is AccountRepository accountRepository)
                return new EmployeeAuthModel(accountRepository, filterIds);
            else if (repository is CategoryRepository categoryRepository)
                return new EmployeeAuthModel(categoryRepository, filterIds);
            return null;
        }
    }
}

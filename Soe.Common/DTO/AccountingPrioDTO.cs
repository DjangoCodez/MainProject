using SoftOne.Soe.Common.Util;
using System.Collections.Generic;
using System.Linq;


namespace SoftOne.Soe.Common.DTO
{
    public class AccountingPrioDTO
    {
        public int? AccountId { get; set; }
        public string AccountNr { get; set; }
        public string AccountName { get; set; }
        public decimal Percent { get; set; }
        public List<string> Log { get; set; } = new List<string>();

        public CustomerAccountType CustomerType { get; set; }
        public EmploymentAccountType EmploymentType { get; set; }
        public EmployeeGroupAccountType EmployeeGroupType { get; set; }
        public PayrollGroupAccountType PayrollGroupType { get; set; }
        public ProductAccountType ProductType { get; set; }
        public ProjectAccountType ProjectType { get; set; }
        public CompanySettingType CompanyType { get; set; }

        public List<AccountInternalDTO> AccountInternals { get; set; }
        public AccountInternalDTO GetAccountInternal(int accountDimId)
        {
            return this.AccountInternals != null ? this.AccountInternals.FirstOrDefault(i => i.AccountDimId == accountDimId) : null;
        }
        public int? GetAccountInternalId(int accountDimId)
        {
            var accountInternal = GetAccountInternal(accountDimId);
            return accountInternal != null ? accountInternal.AccountId : (int?)null;
        }

        public bool HasAccountInternal(int dimNr)
        {
            return this.AccountInternals != null && this.AccountInternals.FirstOrDefault(i => i.AccountDimNr == dimNr) != null;
        }

        public bool HasAccountOnDim(int dimNr)
        {
            if (dimNr == Constants.ACCOUNTDIM_STANDARD)
                return AccountId.HasValue;
            else
                return HasAccountInternal(dimNr);

        }

        public void MergeAccountInternalsByDim(List<AccountInternalDTO> source)
        {
            foreach(var internalAccount in source)
            {
                if (!this.AccountInternals.Exists(x=> x.AccountDimId == internalAccount.AccountDimId))
                {
                    AccountInternals.Add(internalAccount);
                }
            }
        }

        public void AddLog(string log)
        {
            this.Log.Add(log);
        }
    }
}

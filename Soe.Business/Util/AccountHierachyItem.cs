using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Business.Util
{
    public class AccountHierarchy
    {
        public AccountDTO InputAccount { get; }
        public Dictionary<string, AccountDTO> AccountsByHierarchy { get; }
        public Dictionary<int, List<AccountDTO>> AccountsById { get; }

        public AccountHierarchy(AccountDTO inputAccount)
        {
            this.InputAccount = inputAccount;
            this.AccountsByHierarchy = new Dictionary<string, AccountDTO>();
            this.AccountsById = new Dictionary<int, List<AccountDTO>>();
        }

        public void AddAccount(AccountDTO account)
        {
            if (account == null)
                return;

            if (!this.AccountsByHierarchy.ContainsKey(account.HierachyId))
            {
                this.AccountsByHierarchy.Add(account.HierachyId, account);
                if (this.AccountsById.ContainsKey(account.AccountId))
                    this.AccountsById[account.AccountId].Add(account);
                else
                    this.AccountsById.Add(account.AccountId, account.ObjToList());
            }            
        }
        public List<AccountDTO> GetAllAccounts()
        {
            return this.AccountsByHierarchy.Select(a => a.Value).ToList();
        }
        public List<AccountDTO> GetAccounts(int accountId)
        {
            return this.AccountsById.ContainsKey(accountId) ? this.AccountsById[accountId] : new List<AccountDTO>();
        }
        public List<AccountDTO> GetAccountsByDim(int accountDimId)
        {
            return this.AccountsByHierarchy.Select(a => a.Value).Where(i => i.AccountDimId == accountDimId).ToList();
        }
        public bool ContainsAccount(int accountId)
        {
            return this.AccountsById.ContainsKey(accountId);
        }
        public bool ContainsAccountAsAbstract(int accountId)
        {
            return this.AccountsById.ContainsKey(accountId) && this.AccountsById[accountId].Exists(a => a.IsAbstract);
        } 
    }
}

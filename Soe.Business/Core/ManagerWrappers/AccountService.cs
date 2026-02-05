using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftOne.Soe.Business.Core.ManagerWrappers
{
    public interface IAccountService
    {
        Account GetAccount(int actorCompanyId, int accountId);
    }
    public class AccountService: IAccountService
    {
        AccountManager _accountManagerBase;
        CompEntities _entities;
        public AccountService(CompEntities entities, AccountManager accountManagerBase)
        {
            _entities = entities;
            _accountManagerBase = accountManagerBase;
        }

        public Account GetAccount(int actorCompanyId, int accountId)
        {
            return _accountManagerBase.GetAccount(_entities, actorCompanyId, accountId);
        }
    }
}

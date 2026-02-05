using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftOne.Soe.Business.Core.ManagerWrappers
{
    public interface IAccountDistributionEntryRowCurrencySetter
    {
        void SetCurrencyAmounts(AccountDistributionEntryRow entity);
    }
    public class CurrencySetter : IAccountDistributionEntryRowCurrencySetter
    {
        CompEntities _entities;
        CountryCurrencyManager _ccm;
        int _actorCompanyId;
        public CurrencySetter(CompEntities entities, int actorCompanyId, CountryCurrencyManager ccm) {
            _entities = entities;
            _ccm = ccm;
            _actorCompanyId = actorCompanyId;
        }
        public void SetCurrencyAmounts(AccountDistributionEntryRow entity)
        {
            _ccm.SetCurrencyAmounts(_entities, _actorCompanyId, entity);
        }
    }
}

using SoftOne.Soe.Business.Core.ManagerWrappers;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Soe.Business.Tests.Business.Core.AccountDistribution.Stubs
{
    internal class StubCurrencySetter : IAccountDistributionEntryRowCurrencySetter
    {
        public void SetCurrencyAmounts(AccountDistributionEntryRow row) { }
    }
}

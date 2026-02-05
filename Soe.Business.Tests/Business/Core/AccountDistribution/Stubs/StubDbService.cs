using SoftOne.Soe.Business.Core.ManagerWrappers;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Soe.Business.Tests.Business.Core.AccountDistribution.Stubs
{
    internal class StubDbService : IDBBulkService
    {
        public bool SaveCalled { get; private set; }
        public ActionResult SaveChanges()
        {
            SaveCalled = true;
            return new ActionResult(true);
        }
    }
}

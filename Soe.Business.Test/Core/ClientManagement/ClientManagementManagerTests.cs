using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoftOne.Soe.Common.Util;

namespace SoftOne.Soe.Business.Core.Tests
{
    [TestClass()]
    public class ClientManagementManagerTests : TestBase
    {
        [TestMethod()]
        public void CreateMultiCompanyConnectionRequestPermissionTest()
        {
            var cmm = new ClientManagementManager((GetParameterObject(7, 72)));
            var result = cmm.CreateMultiCompanyConnectionRequest();
            Assert.IsTrue(!result.Success && result.ErrorNumber == (int)ActionResultSave.ClientManagementNoConnectionCreationPermission);
        }
    }
}

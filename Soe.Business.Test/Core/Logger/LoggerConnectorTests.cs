using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoftOne.Soe.Business.DataCache;
using SoftOne.Soe.Common.DTO.SoftOneLogger;
using System.Threading.Tasks;

namespace SoftOne.Soe.Business.Core.Logger.Tests
{
    [TestClass()]
    public class LoggerConnectorTests
    {
        [TestMethod()]
        public async Task SavePersonalDataLogTest()
        {
            CompDbCache.Instance.SiteType = Common.Util.TermGroup_SysPageStatusSiteType.Live;
            LoggerConnector.Init();
            int length = 200;
            for (int i = 0; i < length; i++)
            {
                await LoggerConnector.SavePersonalDataLog(new PersonalDataLogBatchDTO());
            }
            Assert.IsTrue(true);
        }
    }
}
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoftOne.Soe.Business.Util.SoftOne_Stage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftOne.Soe.Business.Util.SoftOne_Stage.Tests
{
    [TestClass()]
    public class SoftOneStageUtilTests
    {
        [TestMethod()]
        public void SyncTest()
        {
            SoftOneStageUtil ut = new SoftOneStageUtil();

            StageSyncDTO d = new StageSyncDTO();
            d.StageSyncItemDTOs = new List<StageSyncItemDTO>();
            d.CompanyApiKey = Guid.Parse("7EECF7AE-BB1D-4F36-A360-79A2FA94EFC2");

            StageSyncItemDTO vouchers = new StageSyncItemDTO();
            vouchers.StageSyncItemType = StageSyncItemType.Vouchers;
            d.StageSyncItemDTOs.Add(vouchers);

            StageSyncItemDTO offers = new StageSyncItemDTO();
            offers.StageSyncItemType = StageSyncItemType.Offers;
            d.StageSyncItemDTOs.Add(offers);

            StageSyncItemDTO contracts = new StageSyncItemDTO();
            contracts.StageSyncItemType = StageSyncItemType.Contracts;
            d.StageSyncItemDTOs.Add(contracts);

            StageSyncItemDTO orders = new StageSyncItemDTO();
            orders.StageSyncItemType = StageSyncItemType.Orders;
            d.StageSyncItemDTOs.Add(orders);

            StageSyncItemDTO customerinvoices = new StageSyncItemDTO();
            customerinvoices.StageSyncItemType = StageSyncItemType.Customerinvoices;
            d.StageSyncItemDTOs.Add(customerinvoices);

            var result = ut.Sync(d);
            Assert.IsTrue(result != null);
        }
    }
}
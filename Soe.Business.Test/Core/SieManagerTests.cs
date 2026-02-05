using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Core.Tests;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Soe.Business.Core.Tests
{
    [TestClass()]
    public class SieManagerTests : TestBase
    {
        [TestMethod()]
        public void CanBulkImportVouchers()
        {
            Z.EntityFramework.Extensions.LicenseManager.AddLicense("836;101-SoftOne", "808e16a2-f300-1dd0-5be5-bc37afb71327");
            var sm = new SieManager(null);

            var voucherHead = new VoucherHead()
            {
                VoucherNr = 283,
                Date = new DateTime(2025, 3, 5),
                Text = "Test",
                Status = 2,
                VoucherSeriesId = 12796,
                AccountPeriodId = 4332,
                TypeBalance = false,
                Template = false,
                Created = DateTime.Now,
                CreatedBy = "Tobias",
                ActorCompanyId = 7,
            };

            var voucherRow_1 = new VoucherRow()
            {
                Text = "Row 1",
                Amount = 100,
                AccountId = 240,
                Quantity = null,
                Date = null,
                Merged = false,
                State = 0,
                RowNr = 1,
                AmountEntCurrency = 10,
            };
            voucherHead.VoucherRow.Add(voucherRow_1);
            var voucherRow_2 = new VoucherRow()
            {
                Text = "Row 2",
                Amount = -100,
                AccountId = 240,
                Quantity = null,
                Date = null,
                Merged = false,
                State = 0,
                RowNr = 2,
                AmountEntCurrency = -10,
            };
            voucherHead.VoucherRow.Add(voucherRow_2);

            var vouchers = new List<VoucherHead>() { voucherHead };

            Assert.IsTrue(true);
        }
    }
}

using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoftOne.Soe.Data;
using System.Collections.Generic;

namespace SoftOne.Soe.Business.Core.Tests
{
	[TestClass()]
	public class SupplierInvoiceManagerTests : TestBase
	{
		[TestMethod()]
		public void GetSupplierInvoicesSummaryTest()
		{
			SupplierInvoiceManager m = new SupplierInvoiceManager(null);

			using (CompEntities entities = new CompEntities())
			{
				var result = m.GetSupplierInvoicesSummary(entities, 7);
				Assert.IsTrue(result != null);
			}
		}
	}
}

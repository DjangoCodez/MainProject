using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Data;
using System;

namespace Soe.Business.Test
{
    [TestClass]
    public class ExportSveFakturaTest
    {
        [TestMethod]
        public void Export_SveFaktura_Test()
        {
            ParameterObject parameterObject = null;
            var iex = new ElectronicInvoiceMananger(parameterObject);
            var result = iex.CreatePeppolSveFaktura(17, 0, 44131, String.Empty, null,null, SoftOne.Soe.Common.Util.TermGroup_EInvoiceFormat.Svefaktura, out byte[] data); //Add userid
            Assert.IsTrue(result.Success && data != null);
        }
    }
}

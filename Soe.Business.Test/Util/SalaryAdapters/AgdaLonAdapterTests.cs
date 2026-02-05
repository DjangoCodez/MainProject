using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoftOne.Soe.Business.Util.SalaryAdapters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftOne.Soe.Business.Util.SalaryAdapters.Tests
{
    [TestClass()]
    public class AgdaLonAdapterTests
    {
        [TestMethod()]
        public void GetAgdaLonMinutesValue_ExternalExportIdEmpty_ReturnsQuantity()
        {
            var adapter = new AgdaLonAdapter(null, null, "", false);
            var result = adapter.GetAgdaLonMinutesValue("120", string.Empty, false);
            Assert.AreEqual("120", result);
        }

        [TestMethod()]
        public void GetAgdaLonMinutesValue_ExternalExportIdC_ReturnsFormattedValue()
        {
            var adapter = new AgdaLonAdapter(null, null, "", false);
            var result = adapter.GetAgdaLonMinutesValue("90", "C", false);
            Assert.AreEqual("150", result);
        }

        [TestMethod()]
        public void GetAgdaLonMinutesValue_ExternalExportIdCWithSchedule_ReturnsFormattedValue()
        {
            var adapter = new AgdaLonAdapter(null, null, "", false);
            var result = adapter.GetAgdaLonMinutesValue("90", "C", true);
            Assert.AreEqual("150", result);
        }

        [TestMethod()]
        public void GetAgdaLonMinutesValue_ExternalExportIdCWithScheduleOverTenHours_ReturnsFormattedValue()
        {
            var adapter = new AgdaLonAdapter(null, null, "", false);
            var result = adapter.GetAgdaLonMinutesValue("666", "C", true);
            Assert.AreEqual("B10", result);
        }

        [TestMethod()]
        public void GetAgdaLonMinutesValue_ExternalExportIdM_ReturnsDaySchedule()
        {
            var adapter = new AgdaLonAdapter(null, null, "", false);
            var result = adapter.GetAgdaLonMinutesValue("120", "M", false);
            Assert.AreEqual("M20", result);
        }

        [TestMethod()]
        public void GetAgdaLonMinutesValue_ExternalExportIdMWithSchedule_ReturnsDaySchedule()
        {
            var adapter = new AgdaLonAdapter(null, null, "", false);
            var result = adapter.GetAgdaLonMinutesValue("120", "M", true);
            Assert.AreEqual("M20", result);
        }
    }
}
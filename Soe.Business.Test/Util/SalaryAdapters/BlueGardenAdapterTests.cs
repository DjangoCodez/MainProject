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
    public class BlueGardenAdapterTests
    {
        [TestMethod()]
        public void GetTime4PositionsFromMinutesTest()
        {
            BlueGardenAdapter apd = new BlueGardenAdapter();
            var Threehours = apd.GetTime4PositionsFromMinutes(180);
            var Threeminutes = apd.GetTime4PositionsFromMinutes(3);
            var TenMinutes = apd.GetTime4PositionsFromMinutes(10);

            if (Threehours.Equals("0300") && Threeminutes.Equals("0005") && TenMinutes.Equals("0010"))
            {
                //Test Sucesse
            }
            else
            {
                Assert.Fail();
            }
        }

        [TestMethod()]
        public void GetEmployeeNrTest()
        {
            BlueGardenAdapter apd = new BlueGardenAdapter();

            string code = @"01@102";

            string code2 = @"02";
            string employee = "103";

            string emptyCode = null;
            string emptyEmployee = null;


            var empnr = apd.GetEmployeeNr(employee, code);
            var excode = apd.GetExternalCode(employee, code);

            var empnr2 = apd.GetEmployeeNr(employee, code2);
            var excode2 = apd.GetExternalCode(employee, code2);


            var empnr3 = apd.GetEmployeeNr(emptyEmployee, emptyCode);
            var excode3 = apd.GetExternalCode(emptyEmployee, emptyCode);

            Assert.AreEqual(empnr, "102");
            Assert.AreEqual(empnr2, "103");
            Assert.AreEqual(excode, "01");
            Assert.AreEqual(excode2, "02");
        }
    }
}
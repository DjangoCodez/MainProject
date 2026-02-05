using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoftOne.Soe.Business.Util.ExportFiles.Common;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftOne.Soe.Business.Util.ExportFiles.Common.Tests
{
    [TestClass()]
    public class ExportFilesHelperTests
    {
        [TestMethod()]
        public void SocialSecYYYYMMDD_Dash_XXXXTest()
        {
            List<string> values = new List<string>();

            values.Add("0103111920");
            values.Add("200103111920");
            values.Add("010311-1920");
            values.Add("20010311-1920");

            foreach (var value in values)
            {
                var fixedValue = StringUtility.SocialSecYYYYMMDD_Dash_XXXX(value);

                if (!fixedValue.Equals("20010311-1920"))
                    Assert.Fail();
            }

            List<string> values2 = new List<string>();

            values2.Add("7703111920");
            values2.Add("197703111920");
            values2.Add("770311-1920");
            values2.Add("770311-1920");

            foreach (var value in values2)
            {
                var fixedValue = StringUtility.SocialSecYYYYMMDD_Dash_XXXX(value);

                if (!fixedValue.Equals("19770311-1920"))
                    Assert.Fail();
            }
        }

        [TestMethod()]
        public void Orgnr16XXXXXX_Dash_XXXXTest()
        {
            List<string> values = new List<string>();

            values.Add("5560640137");
            values.Add("556064-0137");
            values.Add("165560640137");
            values.Add("16556064-0137");

            foreach (var value in values)
            {
                var fixedValue = StringUtility.Orgnr16XXXXXX_Dash_XXXX(value);

                if (!fixedValue.Equals("16556064-0137"))
                    Assert.Fail();
            }
        }
    }
}
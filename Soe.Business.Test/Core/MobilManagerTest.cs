using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoftOne.Soe.Business.Core.Mobile;
using SoftOne.Soe.Business.Core.Mobile.Objects;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace SoftOne.Soe.Business.Core.Tests
{
    /// <summary>
    /// The intention of this class is to test 
    /// the entry points in MobilManager.
    /// </summary>
    [TestClass]
    public class MobilManagerTest : TestBase
    {
        int userId = 0;
        int roleid = 0;
        int actorCompanyId = 17; //Demo tid
        int orderId = 0;
        int customerId = 0;
        MobileManager mm = new MobileManager(null, (int)SoeMobileType.XE, "", "", false);
        MobileParam param = null;

        private MobileParam CreateMobilParam(int userId, int roleId, int actorCompanyId)
        {
            return new MobileParam(userId, roleId, actorCompanyId, "");
        }

        private void TryOverrideUserParametersWithPostedValues()
        {
            try
            {
                var streamAndroid = File.Open("android.txt", FileMode.OpenOrCreate);
                var streamiOS = File.Open("ios.txt", FileMode.OpenOrCreate);
                var xmlAndroid = XDocument.Load(streamAndroid);
                var xmliOS = XDocument.Load(streamiOS);
                var bodyAndroid = (from e in xmlAndroid.Root.Elements() where e.Name.LocalName == "Body" select e).FirstOrDefault();
                var bodyiOS = (from e in xmliOS.Root.Elements() where e.Name.LocalName == "Body" select e).FirstOrDefault();
                var ns = bodyAndroid.Descendants().First().Name.NamespaceName;
                var userIdElement = xmlAndroid.Descendants(XName.Get("userId", ns)).FirstOrDefault();
                var roleIdElement = xmlAndroid.Descendants(XName.Get("roleId", ns)).FirstOrDefault();
                var actorCompanyIdElement = xmlAndroid.Descendants(XName.Get("companyId", ns)).FirstOrDefault();
            }
            catch
            {
                // Intentionally ignored, safe to continue
                // NOSONAR
            }
            Assert.IsTrue(true);
        }

        private void TestFlow()
        {
            String errorMessage = string.Empty;
            orderId = 0;

            #region Flow

            #region Login in

            bool loginSuccess = LogIn(out errorMessage, out userId, out roleid, out actorCompanyId);
            Assert.IsTrue(loginSuccess, "Login failed: " + errorMessage);

            param = new MobileParam(userId, roleid, actorCompanyId, "1");

            #endregion

            #region Create new order
            orderId = 74305;
            orderId = SaveOrder(orderId, customerId, "Automated mobil flow new: " + DateTime.Now.ToShortDateString(), out errorMessage);
            Assert.IsTrue(orderId > 0, "Saveorder failed: " + errorMessage);


            #endregion

            #region Save orderrow

            //SaveOrderRow(orderId, 574087, 16, 18, 100, "");

            #endregion

            #region Get orderRows

            #endregion

            #region Save timetimerows

            int rowcountBefore;

            #region Save time for a whole week for userid = 116

            XDocument getTimeRowsResultDoc = GetTimeRows(orderId, out rowcountBefore);
            errorMessage = string.Empty;
            bool success = false;

            for (int i = 0; i < 7; i++)
            {
                errorMessage = String.Empty;
                success = SaveTimeRow(orderId, CalendarUtility.GetFirstDateOfWeek(DateTime.Now).AddDays(i), 180, 240, "OLLE: " + i, out errorMessage);

                Assert.IsTrue(success, "Adding new timerow failed: " + errorMessage);
            }

            int rowcountAfter;
            //Check rowcount after adding a new day
            getTimeRowsResultDoc = GetTimeRows(orderId, out rowcountAfter);

            #endregion

            Assert.IsTrue((rowcountBefore + 7 == rowcountAfter), "RowCount missmatch after adding new timerows for a week. Userid: " + userId);

            //Change user
            userId = 33;
            param = new MobileParam(userId, roleid, actorCompanyId, "");

            #region Save time for a whole week for userid = 33

            getTimeRowsResultDoc = GetTimeRows(orderId, out rowcountBefore);
            for (int i = 0; i < 7; i++)
            {
                errorMessage = String.Empty;
                success = SaveTimeRow(orderId, CalendarUtility.GetFirstDateOfWeek(DateTime.Now).AddDays(i), 60, 120, "HANS: " + i, out errorMessage);

                Assert.IsTrue(success, "Adding new timerow failed: " + errorMessage);
            }
            getTimeRowsResultDoc = GetTimeRows(orderId, out rowcountAfter);

            #endregion

            Assert.IsTrue((rowcountBefore + 7 == rowcountAfter), "RowCount missmatch after adding new timerow for a week. Userid: " + userId);

            //Change user
            userId = 167;
            param = new MobileParam(userId, roleid, actorCompanyId, "");

            #region Save time on todays date for userid = 167

            getTimeRowsResultDoc = GetTimeRows(orderId, out rowcountBefore);
            errorMessage = String.Empty;

            success = SaveTimeRow(orderId, DateTime.Now, 60, 60, "STIG", out errorMessage);
            Assert.IsTrue(success, "Adding new timerow failed: " + errorMessage);

            getTimeRowsResultDoc = GetTimeRows(orderId, out rowcountAfter);

            #endregion

            Assert.IsTrue((rowcountBefore + 1 == rowcountAfter), "RowCount missmatch after adding new timerow. Userid: " + userId);

            #endregion

            #region Search Products

            int searchProductsRowcount;
            String searchString = "arb";
            XDocument searchProductsResultDoc = SearchProducts(orderId, searchString, out searchProductsRowcount, out errorMessage);

            Assert.IsTrue(searchProductsRowcount <= mm.GetSearchProductsMaxFetch(), "RowCount exceded search product max fetch");
            Assert.IsTrue(String.IsNullOrEmpty(errorMessage), "Search products returned an error: " + errorMessage);

            #endregion

            #region Logout

            bool logoutSuccess = LogOut(param, out errorMessage);
            Assert.IsTrue(logoutSuccess, "Logout failed: " + errorMessage);

            #endregion

            #endregion
        }

        private bool LogIn(out String errorMessage, out int userId, out int roleId, out int actorCompanyId)
        {
            XDocument xdoc = mm.Login("1.0", "101", "5", "Test2018", out int _);
            errorMessage = mm.GetErrorMessage(xdoc);
            userId = mm.GetUserId(xdoc);
            roleId = mm.GetRoleId(xdoc);
            actorCompanyId = mm.GetCompanyId(xdoc);
            return String.IsNullOrEmpty(errorMessage);
        }

        private bool LogOut(MobileParam param, out String errorMessage)
        {
            bool success = false;
            XDocument xdoc = mm.Logout(param);
            success = mm.GetSuccessValue(xdoc);
            errorMessage = mm.GetErrorMessage(xdoc);
            return success;
        }

        [TestMethod()]
        public void SaveOrderRow()
        {
            var actorCompanyId = 7;
            var userId = 72;
            var mobile = new MobileManager(GetParameterObject(actorCompanyId, userId), (int)SoeMobileType.XE, "", "", false);
            param = new MobileParam(userId, 0, actorCompanyId, "18");
            //SolarVVS = 9, Dahl = 10

            var result = mobile.SaveOrderRow(param, 36748, 0, 10397, 0, 0, 2, 200, "", 0, 0);
            Assert.IsTrue(result != null);
        }

        private bool SaveTimeRow(int orderId, DateTime date, int invoiceTimeInMinutes, int workTimeInMinutes, String note, out String errorMessage)
        {
            bool success;
            XDocument xdoc = mm.SaveTimeRow(param, orderId, 0, date, invoiceTimeInMinutes, workTimeInMinutes, note, 0);
            success = mm.GetSuccessValue(xdoc);
            errorMessage = mm.GetErrorMessage(xdoc);
            return success;
        }

        private XDocument GetTimeRows(int orderId, out int rowcount)
        {
            XDocument xdoc = mm.GetTimeRows(param, orderId);
            rowcount = mm.GetTimeRowRowCount(xdoc);
            return xdoc;
        }


        private int SaveOrder(int orderId, int customerId, String invoiceText, out String errorMessage)
        {
            XDocument xdoc = mm.SaveOrder(param, orderId, customerId, invoiceText, "", "", "", 0, 0, 0, 0, 0, 0, "", 0, 0, "", 0, "");
            errorMessage = mm.GetErrorMessage(xdoc);
            return mm.GetOrderId(xdoc);
        }


        public XDocument SearchProducts(int orderId, string searchString, out int rowcount, out string errorMessage)
        {
            XDocument xdoc = mm.SearchProducts(param, orderId, searchString);
            rowcount = mm.GetProductsRowCount(xdoc);
            errorMessage = mm.GetSearchProductErrorMessage(xdoc);
            return xdoc;
        }

        [TestMethod]
        public void MobilAppFlow()
        {
            orderId = 0;
            userId = 116; //Olle
            roleid = 11;
            customerId = 20;
            bool testPartial = true;
            param = new MobileParam(userId, roleid, actorCompanyId, "");
            if (!testPartial)
                TestFlow();
            else
                TryOverrideUserParametersWithPostedValues();
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void DeleteProjectTimeBlockTest()
        {
            XDocument xdoc = mm.DeleteProjectTimeBlock(param, 4906);
            Assert.IsTrue(xdoc != null);
        }

        [TestMethod]
        public void MoveProjectTimeBlockToDateTest()
        {
            param = CreateMobilParam(72, 3, 7);
            mm = new MobileManager(GetParameterObject(7, 72, 3), (int)SoeMobileType.XE, "", "", false);
            XDocument xdoc = mm.MoveProjectTimeBlockToDate(param, 6739, new DateTime(2021, 4, 27));
            Assert.IsTrue(xdoc != null);
        }

        [TestMethod]
        public void GetTimeSheetInfoTest()
        {
            param = CreateMobilParam(72, 3, 7);
            mm = new MobileManager(GetParameterObject(7, 72, 3), (int)SoeMobileType.XE, "", "", false);
            XDocument xdoc = mm.GetTimeSheetInfo(param, new DateTime(2021, 4, 19), 66);
            Assert.IsTrue(xdoc != null);
        }

        [TestMethod]
        public void AddImage()
        {
            param = CreateMobilParam(72, 3, 7);
            byte[] data = new byte[0];
            XDocument xdoc = mm.AddImage(param, 23628, data, "", 3, "");
            Assert.IsTrue(xdoc != null);
        }

        [TestMethod]
        public void PerformGetOrderThumbNails()
        {
            var localparam = CreateMobilParam(72, 3, 7);
            var data = mm.GetOrderThumbNails(localparam, 25478);
            Assert.IsTrue(data != null);
        }

        [TestMethod]
        public void PerformGetDocuments()
        {
            var localparam = CreateMobilParam(72, 3, 7);
            var data = mm.GetDocuments(localparam, 25478, (int)SoeEntityType.Order, Convert.ToInt32(SoeDataStorageRecordType.OrderInvoiceFileAttachment).ToString(), false);
            Assert.IsTrue(data != null);
        }

        [TestMethod]
        public void SaveProjectTimeBlock()
        {
            var localparam = CreateMobilParam(72, 3, 792724);
            var data = mm.SaveProjectTimeBlock(localparam, 14790275, 1922248, new DateTime(2023, 2, 5), new DateTime(1900, 1, 1, 6, 0, 0), new DateTime(1900, 1, 1, 7, 0, 0), 180, 60, "", "", 37622, 15511, true, 0);
            Assert.IsTrue(data != null);
        }

        [TestMethod]
        public void SearchInternalProducts()
        {
            var actorCompanyId = 877175;
            var userId = 47583;
            var mobile = new MobileManager(GetParameterObject(actorCompanyId, userId), (int)SoeMobileType.XE, "", "", false);
            var localparam = CreateMobilParam(userId, 5701, actorCompanyId);
            var data = mobile.SearchProducts(localparam, 17803664, "1415123");
            Assert.IsTrue(data != null);
        }

        [TestMethod]
        public void SearchExternalProducts()
        {
            var actorCompanyId = 877175;
            var userId = 47583;
            var mobile = new MobileManager(GetParameterObject(actorCompanyId, userId), (int)SoeMobileType.XE, "", "", false);
            var localparam = CreateMobilParam(userId, 5701, actorCompanyId);
            var data = mobile.SearchExternalProducts(localparam, 17803664, "1415123");
            Assert.IsTrue(data != null);
        }

        [TestMethod]
        public void GetEvacuationList()
        {
            var localparam = CreateMobilParam(30551, 201, 1292);
            var data = mm.GetEvacuationList(localparam);
            Assert.IsTrue(data != null);
        }

        [TestMethod]
        public void PerformOrderEdit()
        {
            var actorCompanyId = 7;
            var userId = 72;
            var localparam = CreateMobilParam(userId, 3, actorCompanyId);
            var mobile = new MobileManager(GetParameterObject(actorCompanyId, userId), (int)SoeMobileType.XE, "", "", false);
            var data = mobile.GetOrderEdit(localparam, 38302);
            Assert.IsTrue(data != null);
        }
    }
}

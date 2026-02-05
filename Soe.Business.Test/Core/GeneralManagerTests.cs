using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;

namespace SoftOne.Soe.Business.Core.Tests
{
    [TestClass()]
    public class GeneralManagerTests
    {
        [TestMethod()]
        public void RemoveEmployeeInfoJobTest()
        {
            GeneralManager gm = new GeneralManager(null);
            var result = gm.RemoveEmployeeInfoJob();
            Assert.IsTrue(result.Success);
        }

        [TestMethod()]
        public void GetDataStorageByDataStorageRecordIdTest()
        {
            GeneralManager gm = new GeneralManager(null);
            using (var entities = new CompEntities())
            {
                var result = gm.GetDataStorageByDataStorageRecordId(entities, 1, 7, false);
                Assert.IsTrue(result != null);

                result = gm.GetDataStorageByDataStorageRecordId(entities, 1, 7, true);
                Assert.IsTrue(result != null);
            }

        }

        [TestMethod()]
        public void GetDataStorageTest()
        {
            GeneralManager gm = new GeneralManager(null);
            using (var entities = new CompEntities())
            {
                var result = gm.GetDataStorage(entities, 6944, 7, false, false);
                Assert.IsTrue(result.DataStorageId != 0);

                result = gm.GetDataStorage(entities, 6944, 7, false, true);
                Assert.IsTrue(result.DataStorageId != 0);

                result = gm.GetDataStorage(entities, 6944, 7, true, false);
                Assert.IsTrue(result.DataStorageId != 0);

                result = gm.GetDataStorage(entities, 6944, 7, true, true);
                Assert.IsTrue(result.DataStorageId != 0);

            }

        }
        
        [TestMethod()]
        public void GetDataStorageRecordsTest()
        {
            GeneralManager gm = new GeneralManager(null);
            using (var entities = new CompEntities())
            {
                var result = gm.GetDataStorageRecords(entities, 7, SoeDataStorageRecordType.InvoicePdf, false);
                Assert.IsTrue(result.Count > 0);

                result = gm.GetDataStorageRecords(entities, 7, SoeDataStorageRecordType.InvoiceBitmap, true);
                Assert.IsTrue(result.Count > 0);
            }

        }

        [TestMethod()]
        public void GetDataStorageRecordQueryTest()
        {
            GeneralManager gm = new GeneralManager(null);
            using (var entities = new CompEntities())
            {
                var result = gm.GetDataStorageRecordQuery(entities, 87, SoeEntityType.SupplierInvoice, false, false, false, false);
                Assert.IsTrue(result.ToDTOs().Count != -1);

                result = gm.GetDataStorageRecordQuery(entities, 87, SoeEntityType.SupplierInvoice, false, true, false, false);
                Assert.IsTrue(result.ToDTOs().Count != -1);

                result = gm.GetDataStorageRecordQuery(entities, 4919, SoeEntityType.SupplierInvoice, true, false, false, false);
                Assert.IsTrue(result.ToDTOs().Count != -1);

                result = gm.GetDataStorageRecordQuery(entities, 8126, SoeEntityType.XEMail, false, false, true, false);
                Assert.IsTrue(result.ToDTOs().Count != -1);

                result = gm.GetDataStorageRecordQuery(entities, 87, SoeEntityType.SupplierInvoice, false, false, false, true);
                Assert.IsTrue(result.ToDTOs().Count != -1);
            }

        }
        [TestMethod()]
        public void GetCompanyInformationsTest()
        {
            GeneralManager gm = new GeneralManager(null);
           
                var result = gm.GetCompanyInformations(291, false, false, false);
                Assert.IsTrue(result.Count > 0);

                result = gm.GetCompanyInformations(291, true, false, false);
                Assert.IsTrue(result.Count > 0);

                result = gm.GetCompanyInformations(291, true, true, false);
                Assert.IsTrue(result.Count > 0);

                result = gm.GetCompanyInformations(291, false, true, false);
                Assert.IsTrue(result.Count > 0);

                result = gm.GetCompanyInformations(291, true, true, true);
                Assert.IsTrue(result.Count > 0);

                result = gm.GetCompanyInformations(291, false, false, true);
                Assert.IsTrue(result.Count > 0);

                result = gm.GetCompanyInformations(291, false, true, true);
                Assert.IsTrue(result.Count > 0);

        }

        [TestMethod()]
        public void GetCompanyInformationsForSendPushNotificationsJobTest()
        {
            GeneralManager gm = new GeneralManager(null);
            using (var entities = new CompEntities())
            {
                var result = gm.GetCompanyInformationsForSendPushNotificationsJob(entities, 90, DateTime.Now);
                Assert.IsTrue(result.Count > 0);

                result = gm.GetCompanyInformationsForSendPushNotificationsJob(entities, 90, DateTime.Now.AddMonths(-1));
                Assert.IsTrue(result.Count > 0);

                result = gm.GetCompanyInformationsForSendPushNotificationsJob(entities, null, DateTime.Now);
                Assert.IsTrue(result.Count > 0);

                result = gm.GetCompanyInformationsForSendPushNotificationsJob(entities, null, DateTime.Now);
                Assert.IsTrue(result.Count > 0);
            }

        }

        [TestMethod()]
        public void GetCompanyInformationTest()
        {
            GeneralManager gm = new GeneralManager(null);
            using (var entities = new CompEntities())
            {
                var result = gm.GetCompanyInformation(entities, 27, 291, false, false);
                Assert.IsTrue(result.InformationId > 0);

                result = gm.GetCompanyInformation(entities, 32, 291, false, true);
                Assert.IsTrue(result.InformationId > 0);

                result = gm.GetCompanyInformation(entities, 34, 291, true, true);
                Assert.IsTrue(result.InformationId > 0);

                result = gm.GetCompanyInformation(entities, 27, 291, true, false);
                Assert.IsTrue(result.InformationId > 0);
            }

        }
        
        [TestMethod()]
        public void GetEventHistoriesTest()
        {
            GeneralManager gm = new GeneralManager(null);

            var result = gm.GetEventHistories(291, TermGroup_EventHistoryType.CollectumNyAnmalan, SoeEntityType.Employee, 181, new DateTime(2020, 10, 2), new DateTime(2020, 10, 3), false);
            Assert.IsTrue(result.Count > 0);

            result = gm.GetEventHistories(291, TermGroup_EventHistoryType.CollectumNyAnmalan, SoeEntityType.Employee, 182, new DateTime(2020, 10, 2), new DateTime(2020, 10, 3), true);
            Assert.IsTrue(result.Count > 0);

        }

        [TestMethod()]
        public void GetTextblockTest()
        {
            GeneralManager gm = new GeneralManager(null);
            using (var entities = new CompEntities())
            {
                var result = gm.GetTextblock(entities,2, false);
                Assert.IsTrue(result.TextblockId > 0);

                result = gm.GetTextblock(entities,2, true);
                Assert.IsTrue(result.TextblockId > 0);
            }
        }

        [TestMethod()]
        public void GetSysInformationsTest()
        {
            GeneralManager gm = new GeneralManager(null);

            var result = gm.GetSysInformations(false, false, false);
            Assert.IsTrue(result.Count > 0);

            result = gm.GetSysInformations(false, true, false);
            Assert.IsTrue(result.Count > 0);

            result = gm.GetSysInformations(true, true, false);
            Assert.IsTrue(result.Count > 0);

            result = gm.GetSysInformations(true, false, false);
            Assert.IsTrue(result.Count > 0);

            result = gm.GetSysInformations(false, false, true);
            Assert.IsTrue(result.Count > 0);

            result = gm.GetSysInformations(false, true, true);
            Assert.IsTrue(result.Count > 0);

            result = gm.GetSysInformations(true, true, true);
            Assert.IsTrue(result.Count > 0);

            result = gm.GetSysInformations(true, false, true);
            Assert.IsTrue(result.Count > 0);


        }

        [TestMethod()]
        public void GetSysInformationTest()
        {
            GeneralManager gm = new GeneralManager(null);
            using (var entities = new SOESysEntities())
            {
                var result = gm.GetSysInformation(entities, 1, false, false, false);
                Assert.IsTrue(result.SysInformationId > 0);

                result = gm.GetSysInformation(entities, 1, false, true, false);
                Assert.IsTrue(result.SysInformationId > 0);

                result = gm.GetSysInformation(entities, 3, true, false, false);
                Assert.IsTrue(result.SysInformationId > 0);

                result = gm.GetSysInformation(entities, 4, true, true, false);
                Assert.IsTrue(result.SysInformationId > 0);

                result = gm.GetSysInformation(entities, 1, false, false, true);
                Assert.IsTrue(result.SysInformationId > 0);

                result = gm.GetSysInformation(entities, 1, false, true, true);
                Assert.IsTrue(result.SysInformationId > 0);

                result = gm.GetSysInformation(entities, 1, true, false, true);
                Assert.IsTrue(result.SysInformationId > 0);

                result = gm.GetSysInformation(entities, 1, true, true, true);
                Assert.IsTrue(result.SysInformationId > 0);
            }

        }

    }
}
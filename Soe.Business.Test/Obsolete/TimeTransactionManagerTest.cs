using SoftOne.Soe.Business.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoftOne.Soe.Util;
using SoftOne.Soe.Data;
using System.Collections.Generic;
using SoftOne.Soe.Business.Util;
using System;
using System.Data.Entity.Core.EntityClient;
using System.Transactions;
using SoftOne.Soe.Common.Util;

namespace Soe.Business.Test
{


    /// <summary>
    ///This is a test class for TimeTransactionManagerTest and is intended
    ///to contain all TimeTransactionManagerTest Unit Tests
    ///</summary>
    [TestClass()]
    public class TimeTransactionManagerTest
    {


        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        [TestMethod()]
        [Ignore] //transaction scope causes test to fail, unclear why?
        public void SaveTimeCodeTransactions_WhenSupplyingNewTransactions_StoreTransPersistent()
        {

            //Arrange                
            ParameterObject parameterObject = null;
            TimeTransactionManager target = new TimeTransactionManager(parameterObject);

            List<TimeCodeTransaction> timeCodeTransactions = new List<TimeCodeTransaction>();

            TimeCodeTransaction timeCodeTrans = new TimeCodeTransaction()
            {
                Quantity = 2,
                Start = new DateTime(1900, 1, 1),
                Stop = new DateTime(1900, 1, 1),
            };

            TimeBlockManager tbm = new TimeBlockManager(parameterObject);
            TimeCodeManager tcm = new TimeCodeManager(parameterObject);

            //Add references
            timeCodeTrans.TimeBlock = tbm.GetTimeBlockDiscardState(81);
            timeCodeTrans.TimeCode = tcm.GetTimeCode(2, 2, false);

            //Add to list
            timeCodeTransactions.Add(timeCodeTrans);
            timeCodeTransactions.Add(timeCodeTrans);

            //Act
            ActionResult actual = null; // target.SaveTimeCodeTransactions(entities, timeCodeTransactions, actorCompanyId, employeeId, userId);

            //Assert
            Assert.IsTrue(actual.Success);
        }

        /// <summary>
        ///A test for SaveTimeCodeTransaction
        ///</summary>
        [TestMethod()]
        [Ignore] //broken due to refactoring
        [DeploymentItem("SoftOne.Soe.Business.dll")]
        public void SaveTimeCodeTransaction_SaveSingleTransaction_StorePersistent()
        {
            //Arrange                
            ParameterObject parameterObject = null;

            //Managers
            //TimeTransactionManager_Accessor target = new TimeTransactionManager_Accessor(parameterObject);
            TimeBlockManager tbm = new TimeBlockManager(parameterObject);
            TimeCodeManager tcm = new TimeCodeManager(parameterObject);
            EmployeeManager em = new EmployeeManager(parameterObject);
            UserManager um = new UserManager(parameterObject);
            TimeRuleManager trm = new TimeRuleManager(parameterObject);
            SettingManager sm = new SettingManager(parameterObject);

            int actorCompanyId = 2;
            int employeeId = 1;
            int userId = 1;

            TimeCodeTransaction timeCodeTransaction = new TimeCodeTransaction()
            {
                Quantity = 2,
                Start = new DateTime(1900, 1, 1),
                Stop = new DateTime(1900, 1, 1),
            };

            //Add references
            timeCodeTransaction.TimeBlock = tbm.GetTimeBlockDiscardState(81);
            timeCodeTransaction.TimeCode = tcm.GetTimeCode(2, 2, false);
            //timeCodeTransaction.TimeRule = trm.GetTimeRule(actorCompanyId, 60, true);

            Employee employee = em.GetEmployee(employeeId, actorCompanyId);
            User user = um.GetUser(userId);
            int accountId = sm.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.AccountInvoiceProductSales, userId, actorCompanyId, 0);

            //Act
            ActionResult actual = null; // target.SaveTimeCodeTransaction(entities, timeCodeTransaction, actorCompanyId, employee, user, transactionScope, accountId);

            //Assert
            Assert.IsTrue(actual.Success);
        }
    }
}

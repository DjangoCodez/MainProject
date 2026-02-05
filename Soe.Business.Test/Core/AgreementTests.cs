using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Business.Core.Tests
{
    [TestClass()]
    public class AgreementTests: TestBase
    {
        [TestMethod()]
        public void GetCustomerInvoicesFromSelectionTest()
        {
            InvoiceManager im = new InvoiceManager(GetParameterObject(7));
            using (CompEntities entities = new CompEntities())
            {
                var openAgreements = im.GetContractsForGrid(entities, SoeOriginStatusClassification.ContractsRunning, 7, 72, true, false, false, false, null, null, false, null, false, null).OrderByDescending(a => a.NextInvoiceDate).Take(10).Select(a => a.CustomerInvoiceId);
                Assert.IsTrue(openAgreements != null);

                foreach (var agreementId in openAgreements)
                {
                    var contract = im.GetCustomerInvoice(entities, agreementId, loadContractGroup: true);
                    if (contract != null && contract.ContractGroup != null)
                    {
                        Tuple<int, int> nextPeriod = CalendarUtility.CalculateNextPeriod((TermGroup_ContractGroupPeriod)contract.ContractGroup.Period, contract.ContractGroup.Interval, contract.NextContractPeriodYear, contract.NextContractPeriodValue);
                        var nextContractPeriodYear = nextPeriod.Item1;
                        var nextContractPeriodValue = nextPeriod.Item2;
                        var nextContractPeriodDate = CalendarUtility.ConvertContractPeriodToDate((TermGroup_ContractGroupPeriod)contract.ContractGroup.Period, contract.InvoiceDate.Value, nextPeriod.Item1, nextPeriod.Item2, contract.ContractGroup.DayInMonth);

                        Tuple<int, int> prevoiusPeriod = CalendarUtility.CalculatePreviousPeriod((TermGroup_ContractGroupPeriod)contract.ContractGroup.Period, contract.ContractGroup.Interval, nextContractPeriodYear, nextContractPeriodValue);
                        var prevoiusContractPeriodYear = prevoiusPeriod.Item1;
                        var prevoiusContractPeriodValue = prevoiusPeriod.Item2;
                        var prevoiusContractPeriodDate = CalendarUtility.ConvertContractPeriodToDate((TermGroup_ContractGroupPeriod)contract.ContractGroup.Period, contract.InvoiceDate.Value, prevoiusPeriod.Item1, prevoiusPeriod.Item2, contract.ContractGroup.DayInMonth);

                        //Assert.IsTrue(contract.NextContractPeriodYear == prevoiusContractPeriodYear);
                        //Assert.IsTrue(contract.NextContractPeriodValue == prevoiusContractPeriodValue);
                        //Assert.IsTrue(contract.NextContractPeriodDate == prevoiusContractPeriodDate);
                    }
                }
            }
        }
    }
}
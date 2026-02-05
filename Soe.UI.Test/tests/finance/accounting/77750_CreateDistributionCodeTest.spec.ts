import { test } from '../../fixtures/finance-fixture';
import * as allure from "allure-js-commons";
import { getAccountExValue } from '../../../utils/properties';

let testCaseId: string = '77750';
let envUrl: string;

test.beforeEach(async ({ accountEx, financeBasePage }) => {
  await allure.parentSuite("Finance");
  await allure.suite("Accounting");
  await allure.subSuite("Budget");
  await allure.link('https://dev.azure.com/softonedev/XE/_workitems/edit/' + 77750 ,"Test case : " + 77750, );
  envUrl = accountEx.baseUrl;
  await financeBasePage.SetupFinance(envUrl, getAccountExValue(accountEx, 'role')?.toString() ?? 'Admin');
});

test(testCaseId + ': Create Distribution code : DK', { tag: ['@Finance', '@Accounting', '@Regression'] }, async ({ distributionCodePage }) => {
  {
    await distributionCodePage.goToMenu('Settings_Finance', 'Distribution Codes', true, 'Accounting');
    await distributionCodePage.waitForPageLoad();
    await distributionCodePage.createItem();
    let DistributionCodeName: string = 'DCName' + Math.random().toString(36).substring(2, 7);
    await distributionCodePage.setName(DistributionCodeName)
    await distributionCodePage.verifyNumberOfPeriods('12');
    await distributionCodePage.addDistributionCodeValidFrom();
    await distributionCodePage.save();
    await distributionCodePage.verifyShareDistribution(12,'8.33','8.37');
    await distributionCodePage.closeTab();
    await distributionCodePage.filterByDistributionCodeName(DistributionCodeName);
    await distributionCodePage.verifyFilteredRowCount('1');
  }
});
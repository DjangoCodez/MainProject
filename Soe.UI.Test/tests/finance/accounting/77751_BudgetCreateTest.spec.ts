import { test } from '../../fixtures/finance-fixture';
import * as allure from "allure-js-commons";
import { getAccountExValue, getEnvironmentValue } from "../../../utils/properties";

let testCaseId: string = '77751';
let envUrl: string;

test.beforeEach(async ({ accountEx, financeBasePage }) => {
  await allure.parentSuite("Finance");
  await allure.suite("Accounting");
  await allure.subSuite("Budget");
  await allure.link('https://dev.azure.com/softonedev/XE/_workitems/edit/' + 77751 ,"Test case : " + 77751, );
  envUrl = accountEx.baseUrl;
  await financeBasePage.SetupFinance(envUrl, getAccountExValue(accountEx, 'role')?.toString() ?? 'Admin');
});

test(testCaseId + ': Budget Create : DK', { tag: ['@Finance', '@Accounting', '@Regression'] }, async ({ budgetPage, page }) => {
  {
    await budgetPage.goToMenu('Accounting', 'Budget');
    await budgetPage.goToPageVersion(budgetPage.ang_version);
    await budgetPage.createItem();
    let BudgetName: string = 'Budget' + Math.random().toString(36).substring(2, 7);
    await budgetPage.setBudgetName(BudgetName)
    await budgetPage.verifyNumberOfPeriods('12');
    await budgetPage.setLastFinancialYear();
    await budgetPage.clickYesLoadResults();
    await budgetPage.save();
    await page.waitForTimeout(3000);
    await budgetPage.checkCostCenterCheckbox();
    await budgetPage.addCostCenter(getEnvironmentValue('default_costCenter')?.toString() ?? '');
    await budgetPage.addStandardDistributionCode(getEnvironmentValue('default_distributionCode')?.toString() ?? '');
    await budgetPage.AddRow();
    await budgetPage.clickClear();
    await budgetPage.AddRow();
    await budgetPage.addAccount("1010");
    await budgetPage.save();
    await budgetPage.closeTab();
    await budgetPage.filterByBudgetName(BudgetName);
    await budgetPage.VerifyRowCount("1");
    console.log("Budget is created successfully" + BudgetName);
    await budgetPage.goToEditBudget();
    await budgetPage.clickClear();
    await budgetPage.removeBudget();
    await budgetPage.filterByBudgetName(BudgetName);
    await budgetPage.VerifyRowCount("0");
    console.log("Budget is deleted successfully" + BudgetName);
  }
});
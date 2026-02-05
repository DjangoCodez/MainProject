import { test } from '../../../fixtures/sales-fixture';
import * as allure from "allure-js-commons";
import { getAccountExValue } from '../../../../utils/properties';
 
let testCaseId: string = '115203';
let envUrl: string;
 
test.beforeEach(async ({ accountEx, salesBasePage }) => {
  await allure.parentSuite("Sales");
  await allure.suite("Settings");
  await allure.subSuite("Product group (NEW)");
  await allure.link('https://dev.azure.com/softonedev/XE/_workitems/edit/' + testCaseId, "Test case : " + testCaseId);
  envUrl = accountEx.baseUrl;
  await salesBasePage.SetupSales(envUrl, getAccountExValue(accountEx, 'role')?.toString() ?? 'Admin');
});

test(testCaseId + ': Create Product Group : DK', { tag: ['@Sales', '@Settings', '@Regression'] }, async ({ productGroupsPage }) => {
    {
      await productGroupsPage.goToMenu('Settings_Sales', 'Product Groups', true, 'Products');
      await productGroupsPage.waitForPageLoad();
      await productGroupsPage.createItem();
      let productGroupCode: string = 'ProdGCode' + Math.random().toString(36).substring(2, 7);
      let productGroupName: string = 'ProdGName' + Math.random().toString(36).substring(2, 7);
      await productGroupsPage.setCode(productGroupCode);
      await productGroupsPage.setName(productGroupName)
      await productGroupsPage.saveProductGroup();
      await productGroupsPage.closeTab();
      await productGroupsPage.filterByProductGroupName(productGroupName);
      await productGroupsPage.VerifyRowCount('1');
    }
});
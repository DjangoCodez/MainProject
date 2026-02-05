import { test } from '../../../fixtures/sales-fixture';
import * as allure from "allure-js-commons";
import { getAccountExValue } from '../../../../utils/properties';

let testCaseId: string = '115205';
let envUrl: string;

test.beforeEach(async ({ accountEx, salesBasePage }) => {
  await allure.parentSuite("Sales");
  await allure.suite("Settings");
  await allure.subSuite("Product unit (NEW)");
  await allure.link('https://dev.azure.com/softonedev/XE/_workitems/edit/' + testCaseId, "Test case : " + testCaseId);
  envUrl = accountEx.baseUrl;
  await salesBasePage.SetupSales(envUrl, getAccountExValue(accountEx, 'role')?.toString() ?? 'Admin');
});

test(testCaseId + ': Create Product Unit : DK ', { tag: ['@Sales', '@Settings', '@Regression'] }, async ({ productUnitsPage }) => {
  {
    await productUnitsPage.goToMenu('Settings_Sales', 'Product Units', true, 'Products');
    await productUnitsPage.createItem();
    let productUnitCode: string = 'ProdUCode' + Math.random().toString(36).substring(2, 7);
    let productUnitName: string = 'ProdUName' + Math.random().toString(36).substring(2, 7);
    await productUnitsPage.setCode(productUnitCode);
    await productUnitsPage.setName(productUnitName);
    await productUnitsPage.save();
    await productUnitsPage.waitForSave();
    await productUnitsPage.closeTab();
    await productUnitsPage.filterByProductUnitName(productUnitName);
    await productUnitsPage.VerifyRowCount('1');
  }
});
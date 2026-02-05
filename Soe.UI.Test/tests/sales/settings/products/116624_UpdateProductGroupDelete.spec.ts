import { test } from '../../../fixtures/sales-fixture';
import * as allure from "allure-js-commons";
import { getAccountExValue } from '../../../../utils/properties';
import { ProductSettingsUtil } from '../../../../apis/utils/ProductSettingsUtil';

let productGroupUtil: ProductSettingsUtil;
let testCaseId: string = '116624';
let envUrl: string;
let code: string;
let name: string;

test.beforeEach(async ({ page, accountEx, salesBasePage }) => {
  await allure.parentSuite("Sales");
  await allure.suite("Settings");
  await allure.subSuite("Product group (NEW)");
  await allure.link('https://dev.azure.com/softonedev/XE/_workitems/edit/' + testCaseId, "Test case : " + testCaseId);
  envUrl = accountEx.baseUrl;
  await salesBasePage.SetupSales(envUrl, getAccountExValue(accountEx, 'role')?.toString() ?? 'Admin');
  productGroupUtil = new ProductSettingsUtil(page, envUrl);
  code = 'ProG' + Math.floor(100000 + Math.random() * 900000);
  name = 'ProGName' + Math.floor(100000 + Math.random() * 900000);
  await productGroupUtil.createProductGroup( name , code);
});

test(testCaseId + ': Update/Delete product group : DK ', { tag: ['@Sales', '@Settings', '@Regression'] }, async ({ salesBasePage, productGroupsPage }) => {
    {
      await salesBasePage.goToMenu('Settings_Sales', 'Product Groups', true, 'Products');
      console.log('New Product group is created (API)' + name);
      await productGroupsPage.filterByProductGroupName(name);
      await productGroupsPage.VerifyRowCount('1');
      await productGroupsPage.editProductGroup();
      await productGroupsPage.updateProductGroupName(name + ' EDIT');
      await salesBasePage.save();
      await salesBasePage.closeTab();
      await productGroupsPage.filterByProductGroupName(name + ' EDIT');
      console.log('Product group is edited successfully' + name);
      await productGroupsPage.editProductGroup();
      await productGroupsPage.deleteProductGroup();
      await productGroupsPage.filterByProductGroupName(name + ' EDIT');
      await productGroupsPage.VerifyRowCount('0');
      console.log('Product group is deleted successfully' + name);
    }
});
import { test } from '../../fixtures/sales-fixture';
import * as allure from "allure-js-commons";
import { getAccountExValue } from '../../../utils/properties';
import { ProductUtils } from '../../../apis/utils/ProductUtil';
import { AgreementUtil } from '../../../apis/utils/AgreementUtil';

let testCaseId: string = '78489';
let envUrl: string;
let productUtils: ProductUtils;
let products: any;
let agreemntsUtil: AgreementUtil;
let agreementsGroups: any = [];


test.beforeEach(async ({ page, accountEx, salesBasePage }) => {
  await allure.parentSuite("Sales");
  await allure.suite("Agreements");
  await allure.subSuite("Agreement");
  await allure.link('https://dev.azure.com/softonedev/XE/_workitems/edit/' + testCaseId, "Test case : " + testCaseId);
  envUrl = accountEx.baseUrl;
  await salesBasePage.SetupSales(envUrl, (getAccountExValue(accountEx, 'role')?.toString() ?? 'Admin'));
  productUtils = new ProductUtils(page, envUrl);
  products = await productUtils.getProducts();
  agreemntsUtil = new AgreementUtil(page, envUrl);
  agreementsGroups = await agreemntsUtil.getAgreementGroups();
});

test(testCaseId + ': Create Agreement Group : AP', { tag: ['@Sales', '@Agreements', '@Regression'] }, async ({ page, salesBasePage, agreementsGroupsPage, agreementsPage }) => {
  const groups = ['Month', 'Year', 'Quarter'];
  for (const group of groups) {
    const isGroupAvailable = agreementsGroups.some((item: { name: string; }) => item.name === group);
    if (isGroupAvailable) {
      console.log('Contract group already exists: ' + group);
    } else {
      await salesBasePage.goToMenu('Agreements', 'Groups', true);
      await agreementsGroupsPage.createItem();
      await agreementsGroupsPage.setName(group);
      await agreementsGroupsPage.setPeriod(group);
      await agreementsGroupsPage.setDayOfMonth(1);
      await agreementsGroupsPage.setPriceManagement('Static price (unchanged)');
      await salesBasePage.save();
    }
    if (!page.url().includes('/soe/billing/contract/status/default.aspx')) {
      await salesBasePage.goToMenu('Agreements ', 'Agreements');
      await agreementsPage.waitForPageLoad();
    }
    await agreementsPage.createItem();
    await agreementsPage.editCustomer();
    await page.waitForTimeout(3000);
    const CustomerNo: string = Math.floor(100000 + Math.random() * 900000) + '_' + testCaseId;
    await agreementsPage.createCustomer(CustomerNo, 'Test Customer');
    await agreementsPage.setInternalText(CustomerNo);
    await agreementsPage.setContractGroup(group);
    await agreementsPage.nextInvoiceDate();
    await agreementsPage.expandProductRows();
    await agreementsPage.addProductRow();
    await agreementsPage.addProductNo(products[0].name);
    await agreementsPage.clickOk();
    await agreementsPage.addProductAmount(10);
    await agreementsPage.addProductPrice(2300);
    await agreementsPage.save();
    await agreementsPage.close();
    await agreementsPage.moveToAgreementTab();
    await agreementsPage.filterByInternalTextAgreements(CustomerNo);
    await agreementsPage.VerifyRowCount();
    await agreementsPage.VerifyAgreementGroup(group);
    await agreementsPage.VerifyTotalVatAmount('23000,00');
  }
});



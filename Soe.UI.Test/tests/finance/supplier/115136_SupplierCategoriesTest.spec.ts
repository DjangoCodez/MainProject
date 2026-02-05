import { test } from '../../fixtures/finance-fixture';
import * as allure from "allure-js-commons";
import { getAccountExValue } from '../../../utils/properties';

let testCaseId: string = '115136';
let envUrl: string;
let categoryNumber: string = testCaseId + Math.floor(10000 + Math.random() * 90000);
let categoryCode: string = 'SupplierCategory_Code_' + categoryNumber;
let categoryName: string = 'Category_Name_ ' + categoryNumber;

test.beforeEach(async ({ accountEx, financeBasePage }) => {
    await allure.parentSuite("Finance");
    await allure.suite("Supplier");
    await allure.subSuite("Supplier");
    await allure.link('https://dev.azure.com/softonedev/XE/_workitems/edit/' + testCaseId, "Test case : " + testCaseId);
    envUrl = accountEx.baseUrl;
    await financeBasePage.SetupFinance(envUrl, getAccountExValue(accountEx, 'role')?.toString() ?? 'Admin');
});

test(testCaseId + ': Supplier Processing History : AP', { tag: ['@Finance', '@Supplier', '@Regression'] }, async ({ page, financeBasePage, categoriesPage }) => {
    await financeBasePage.goToMenu('Supplier', 'Categories', true, 'Index');
    await categoriesPage.createItem();
    await categoriesPage.addCode(categoryCode);
    await categoriesPage.addName(categoryName);
    await categoriesPage.save();
    await categoriesPage.closeTab();
    await categoriesPage.createItem();
    await categoriesPage.addCode(categoryCode)
    await categoriesPage.addName(categoryNumber);
    await categoriesPage.save();
    await categoriesPage.verifyCodeNameAdreadyExistsMessage('Code or name already exists');
    await categoriesPage.clickAlertMessage('OK');
    await page.waitForTimeout(1000);
    await categoriesPage.closeTab();
    await categoriesPage.filterbyCode(categoryCode);
    await categoriesPage.edit();
    await page.waitForTimeout(500);
    await categoriesPage.remove();
    await categoriesPage.clickAlertMessage('OK');
    await categoriesPage.reload();
    await page.waitForTimeout(500);
    await categoriesPage.verifyNoRecords();
});



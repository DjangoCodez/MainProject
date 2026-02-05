import { test } from '../../fixtures/sales-fixture';
import * as allure from "allure-js-commons";
import { getAccountExValue } from '../../../utils/properties';
import { ProjectUtils } from '../../../apis/utils/ProjectUtils';
import { ProductUtils } from '../../../apis/utils/ProductUtil';
import { generateRandomId, getCurrentDateUtilWithFormat } from '../../../utils/CommonUtil';

let testCaseId: string = '78829';
let envUrl: string;
let projectName: string;
let productName: string = 'Auto Supplier ' + testCaseId;

test.use({ account: { domain: process.env.defualt_domain!, user: 'adminERP' } });

test.beforeEach(async ({ page, accountEx, salesBasePage }, testInfo) => {
  const uniqueId = generateRandomId(testInfo,testCaseId);
  await allure.parentSuite("Sales");
  await allure.suite("Projects");
  await allure.subSuite("Project");
  await allure.link('https://dev.azure.com/softonedev/XE/_workitems/edit/' + testCaseId, "Test case : " + testCaseId);
  envUrl = accountEx.baseUrl;
  await salesBasePage.SetupSales(envUrl, getAccountExValue(accountEx, 'role')?.toString() ?? 'Admin');
  projectName = 'Auto ' + Math.random().toString(36).substring(2, 7) + "_" + testCaseId;
  let projectUtils: ProjectUtils = new ProjectUtils(page, envUrl);
  await projectUtils.addProject(projectName, uniqueId);
  let productUtils = new ProductUtils(page, envUrl);
  await productUtils.createProduct(testCaseId, productName);
});

test(testCaseId + ': Create project supplier invoice : MG', { tag: ['@Sales', '@Projects', '@Regression'] }, async ({ page, orderPageJS, supplierInvoicePageJS, projectPageJS, projectOverviewPageJS }) => {
  await orderPageJS.goToMenu('Order', 'Order');
  await orderPageJS.waitForPageLoad();
  await orderPageJS.createItem();
  await orderPageJS.addCustomer('Playwright');
  let internalText = 'Auto ' + Math.random().toString(36).substring(2, 7) + "_" + testCaseId;
  await orderPageJS.addInternalText(internalText);
  await orderPageJS.linkProject(projectName);
  await orderPageJS.expandProducts();
  await orderPageJS.addNewProduct(productName, '23000');
  await orderPageJS.saveOrder();
  await page.waitForTimeout(3000); // Waiting for order to be saved properly
  await orderPageJS.switchMenu('Finance');
  await supplierInvoicePageJS.goToMenu('Supplier', 'Invoices');
  await supplierInvoicePageJS.waitForPageLoad();
  await supplierInvoicePageJS.createItem();
  await supplierInvoicePageJS.setSupplier('1');
  let invoiceNumber = 'INV-' + Math.random().toString(36).substring(2, 7) + "_" + testCaseId
  await supplierInvoicePageJS.setInvoiceNumber(invoiceNumber);
  const currentDate = await getCurrentDateUtilWithFormat();
  await supplierInvoicePageJS.setInvoiceDate(currentDate);
  await supplierInvoicePageJS.setTotal('100000');
  await supplierInvoicePageJS.linkOrder(internalText);
  await supplierInvoicePageJS.scrollToAllocateCostGrid();
  await supplierInvoicePageJS.chargeCostToProject(true);
  await supplierInvoicePageJS.saveInvoice();
  await orderPageJS.switchMenu('Sales');
  await orderPageJS.goToMenu('Projects', 'Projects');
  await projectPageJS.waitForPageLoad();
  await projectPageJS.filterProjectName(projectName);
  await projectPageJS.goToPrjectOverview();
  await projectOverviewPageJS.waitForPageLoad();
  await projectOverviewPageJS.verifyIncomeInvoiced('1 500,00');
  await projectOverviewPageJS.verifyUninvoiceIncome('23 000,00');
  await projectOverviewPageJS.verifyExpenses('80 000,00');
  await projectOverviewPageJS.switchMenu('Finance');
  await projectOverviewPageJS.goToMenu('Supplier', 'Invoices');
  await supplierInvoicePageJS.waitForPageLoad();
  await page.waitForTimeout(3000);
  await supplierInvoicePageJS.openInvoiceByNumber(invoiceNumber);
  await supplierInvoicePageJS.scrollToAllocateCostGrid();
  await supplierInvoicePageJS.expandAllocateCostGrid();
  await supplierInvoicePageJS.chargeCostToProject(false);
  await supplierInvoicePageJS.saveInvoice();
  await orderPageJS.switchMenu('Sales');
  await orderPageJS.goToMenu('Projects', 'Projects');
  await projectPageJS.waitForPageLoad();
  await projectPageJS.filterProjectName(projectName);
  await projectPageJS.goToPrjectOverview();
  await projectOverviewPageJS.waitForPageLoad();
  await projectOverviewPageJS.verifyIncomeInvoiced('1 500,00');
  await projectOverviewPageJS.verifyUninvoiceIncome('23 000,00');
  await projectOverviewPageJS.verifyExpenses('0,00');
});

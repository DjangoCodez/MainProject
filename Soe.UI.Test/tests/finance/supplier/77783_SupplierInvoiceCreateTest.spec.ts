import { test } from '../../fixtures/finance-fixture';
import * as allure from "allure-js-commons";
import { getAccountExValue, getEnvironmentValue } from '../../../utils/properties';
import { getDateUtil } from 'utils/CommonUtil';

let testCaseId: string = '77783';
let envUrl: string;
const invoiceNumber: string = "Inv-" + testCaseId + Math.floor(100000 + Math.random() * 900000);
const supplier: string = getEnvironmentValue('default_supplier') ?? '';
const totalAmount = '100000,00';
const vatAmount = '20000,00';

test.beforeEach(async ({ accountEx, financeBasePage }) => {
  await allure.parentSuite("Finance");
  await allure.suite("Supplier");
  await allure.subSuite("Supplier");
  await allure.link('https://dev.azure.com/softonedev/XE/_workitems/edit/' + testCaseId, "Test case : " + testCaseId);
  envUrl = accountEx.baseUrl;
  await financeBasePage.SetupFinance(envUrl, getAccountExValue(accountEx, 'role')?.toString() ?? 'Admin');
});

test(testCaseId + ': Supplier Invoice Create : AP', { tag: ['@Finance', '@Supplier', '@Regression'] }, async ({ financeBasePage, suppliersPage, supplierInvoicePageJS }) => {
  await financeBasePage.goToMenu('Supplier', 'Suppliers', true, 'Index');
  await suppliersPage.waitForSupplierGridLoaded();
  const supplierNumber = await suppliersPage.getSupplierNumber();
  const paymentAccount = await suppliersPage.getPaymentAccount();
  const SupplierName = await suppliersPage.getSupplierName();
  await financeBasePage.goToMenu('Supplier', 'Invoices');
  await supplierInvoicePageJS.waitForSupplierInvoiceGridLoaded();
  await financeBasePage.createItem();
  await supplierInvoicePageJS.waitForNewSupplierInvoiceLoaded();
  await supplierInvoicePageJS.setSupplier(supplier);
  await supplierInvoicePageJS.editSupplier(false);
  await supplierInvoicePageJS.waitForDataLoad('//maps.googleapis.com/maps-api-v3/api');
  await supplierInvoicePageJS.verifySupplierNo(supplierNumber?.toString() ?? '');
  await supplierInvoicePageJS.verifySupplierName(SupplierName?.toString() ?? '');
  await supplierInvoicePageJS.verifyPaymentAccount(paymentAccount?.toString() ?? '');
  await supplierInvoicePageJS.closeSupplierModal();
  await supplierInvoicePageJS.setInvoiceNumber(invoiceNumber);
  await supplierInvoicePageJS.setTotal(totalAmount);
  await supplierInvoicePageJS.setInvoiceDate(await getDateUtil(0));
  await supplierInvoicePageJS.verifyVatAmmount(vatAmount);
  await supplierInvoicePageJS.verifyAccountingDate(await getDateUtil(0));
  await supplierInvoicePageJS.verifyDueDateIsNotInvoiceDate(await getDateUtil(0));
  await supplierInvoicePageJS.uploadPhoto(`test-data/Påminnelsemedfakturakopia123429SelTestCustomer.pdf`);
  await supplierInvoicePageJS.deletePhoto();
  await supplierInvoicePageJS.clickPreliminary();
  await supplierInvoicePageJS.saveSupplierInvoice(true);
  await supplierInvoicePageJS.waitForSupplierInvoiceGridLoaded();
  await supplierInvoicePageJS.openInvoiceByNumber(invoiceNumber, false);
  await supplierInvoicePageJS.verifyInvoiceStatus('Preliminary');
  await supplierInvoicePageJS.clearAllFilters();
  await supplierInvoicePageJS.openInvoiceByNumber(invoiceNumber);
  await supplierInvoicePageJS.waitForNewSupplierInvoiceLoaded();
  await supplierInvoicePageJS.verifyVatAmmount(vatAmount);
  await supplierInvoicePageJS.verifyPaymentAccount(paymentAccount?.toString() ?? '');
  await supplierInvoicePageJS.verifyTotalAmount(totalAmount);
  await supplierInvoicePageJS.clickPreliminary(false);
  await supplierInvoicePageJS.saveSupplierInvoice(true);
  await supplierInvoicePageJS.clearAllFilters();
  await supplierInvoicePageJS.openInvoiceByNumber(invoiceNumber);
  await supplierInvoicePageJS.verifyInvoiceStatus('Documentation');
});


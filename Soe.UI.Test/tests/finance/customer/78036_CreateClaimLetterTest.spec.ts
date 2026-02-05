import { test } from '../../fixtures/finance-fixture';
import * as allure from "allure-js-commons";
import { getAccountExValue } from '../../../utils/properties';
import { InvoiceUtil } from '../../../apis/utils/InvoiceUtil';
import { getDateUtil } from '../../../utils/CommonUtil';

let testCaseId: string = '78036';
let envUrl: string;
let invoiceUtil: InvoiceUtil;
let customerInvoiceNumber: string = Math.floor(100000 + Math.random() * 900000) + '_' + testCaseId;

test.use({ account: { domain: process.env.defualt_domain!, user: 'adminERP' } });
test.beforeEach(async ({ page, accountEx, financeBasePage }) => {
  await allure.parentSuite("Finance");
  await allure.suite("Customer");
  await allure.subSuite("Customer Payments");
  await allure.link('https://dev.azure.com/softonedev/XE/_workitems/edit/' + testCaseId, "Test case : " + testCaseId);
  envUrl = accountEx.baseUrl;
  await financeBasePage.SetupFinance(envUrl, getAccountExValue(accountEx, 'role')?.toString() ?? 'Admin');
  invoiceUtil = new InvoiceUtil(page, envUrl);
  invoiceUtil.CreateCustomerInvoice(customerInvoiceNumber);
});

test(testCaseId + ': Customer Payment Create Claim Letter : AP', { tag: ['@Sales', '@Customer', '@Regression'] }, async ({ financeBasePage, paymentsPage }) => {
  await financeBasePage.goToMenu('Customer', 'Payments');
  await paymentsPage.waitForGridLoad();
  await paymentsPage.filterByInvoiceNo(customerInvoiceNumber);
  await paymentsPage.editInvoice();
  await paymentsPage.expandCustomerInvoiceSection();
  await paymentsPage.changeDueDate(await getDateUtil(0));
  await paymentsPage.saveCustomerInvoice();
  await paymentsPage.close();
  await paymentsPage.moveToDemandTab();
  await paymentsPage.filterInvoiceNumberDemand(customerInvoiceNumber);
  const requirementLevel = await paymentsPage.getRequirementLevel(customerInvoiceNumber) ?? '';
  await paymentsPage.selectInvoice();
  await paymentsPage.printDemandLetter();
  await paymentsPage.reload(1);
  await paymentsPage.verifyDowloadPDF();
  const updatedRequirementLevel = await paymentsPage.getRequirementLevel(customerInvoiceNumber);
  await paymentsPage.verifyIncreasedRequirementLevel(requirementLevel, updatedRequirementLevel?.toString() ?? '');
});



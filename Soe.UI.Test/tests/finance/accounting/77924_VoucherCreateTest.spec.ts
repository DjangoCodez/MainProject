import { test } from '../../fixtures/finance-fixture';
import * as allure from "allure-js-commons";
import { getAccountExValue } from '../../../utils/properties';

let testCaseId: string = '77924';
let envUrl: string;
let internalText_1: string;
let accountNumber: string;

test.beforeEach(async ({ accountEx, financeBasePage }) => {
  await allure.parentSuite("Finance");
  await allure.suite("Accounting");
  await allure.subSuite("Voucher");
  await allure.link('https://dev.azure.com/softonedev/XE/_workitems/edit/' + testCaseId, "Test case : " + testCaseId);
  envUrl = accountEx.baseUrl;
  await financeBasePage.SetupFinance(envUrl, getAccountExValue(accountEx, 'role')?.toString() ?? 'Admin');
  accountNumber = testCaseId + Math.floor(100000 + Math.random() * 900000);
  internalText_1 = 'Voucher_' + accountNumber;
});

test(testCaseId + ': Voucher Create Account : AP', { tag: ['@Finance', '@Accounting', '@Regression'] }, async ({ financeBasePage, voucherPageJS }) => {
  await financeBasePage.goToMenu('Accounting', 'Voucher');
  await voucherPageJS.waitForPageLoad();
  await voucherPageJS.createItem();
  await voucherPageJS.waitingForCodingRowsLoading();
  await voucherPageJS.addText(internalText_1);
  await voucherPageJS.addCodingRow();
  await voucherPageJS.addAccount(accountNumber, true);
  await voucherPageJS.verifyAccountNumberNotFound();
  await voucherPageJS.clickAlertMessage('OK');
  await voucherPageJS.addAccountType('Asset');
  await voucherPageJS.addNameForNewAccount('Test Account');
  await voucherPageJS.saveNewAccount();
  await voucherPageJS.addDebitAmount('100', 0);
  await voucherPageJS.addAccount(accountNumber);
  await voucherPageJS.addCreditAmount('100', 1);
  await voucherPageJS.saveVoucher();
  await voucherPageJS.closeVoucher();
  await voucherPageJS.selectbyVoucherByText(internalText_1);
  await voucherPageJS.editVoucher();
  await voucherPageJS.waitingForCodingRowsLoading();
  await voucherPageJS.verifyDebitAmountandAccount('100,00', `${accountNumber}- Test Account`);
  await voucherPageJS.verifyCreditAmountandAccount('100,00', `${accountNumber}- Test Account`);
});



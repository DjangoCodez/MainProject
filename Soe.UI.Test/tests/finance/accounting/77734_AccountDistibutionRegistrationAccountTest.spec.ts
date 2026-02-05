import { test } from '../../fixtures/finance-fixture';
import * as allure from "allure-js-commons";
import { getAccountExValue } from '../../../utils/properties';
import { getDateUtil, getLastDateOfCurrentMonth } from '../../../utils/CommonUtil';

let testCaseId: string = '77734';
let envUrl: string;
let internalText_1: string;

test.beforeEach(async ({ accountEx, financeBasePage }) => {
  await allure.parentSuite("Finance");
  await allure.suite("Accounting");
  await allure.subSuite("Accruals");
  await allure.link('https://dev.azure.com/softonedev/XE/_workitems/edit/' + testCaseId, "Test case : " + testCaseId);
  envUrl = accountEx.baseUrl;
  await financeBasePage.SetupFinance(envUrl, getAccountExValue(accountEx, 'role')?.toString() ?? 'Admin');
  internalText_1 = 'Voucher_' + testCaseId + Math.floor(100000 + Math.random() * 900000) + '_1';
});

test(testCaseId + ': Create Voucher : AP', { tag: ['@Finance', '@Accounting', '@Regression'] }, async ({ page, financeBasePage, accountPage, voucherPageJS, accuralsPageJS }) => {
  await financeBasePage.goToMenu('Accounting', 'Konto', true);
  await financeBasePage.goToPageVersion(accountPage.ang_version);
  await accountPage.waitForPageLoad();
  await accountPage.filterAccount('1720');
  await accountPage.openAccount();
  await page.waitForTimeout(3000);
  await accountPage.verifyIfAccrualAccount();
  await financeBasePage.goToMenu('Accounting', 'Voucher');
  await voucherPageJS.waitForPageLoad();
  await voucherPageJS.createItem();
  await voucherPageJS.selectSeries('Automatkontering');
  const voucherNumber = await voucherPageJS.getVoucherNumber();
  const today = await getDateUtil(0, false);
  await voucherPageJS.addTodayDate(today);
  await voucherPageJS.addText(internalText_1);
  await voucherPageJS.addCodingRow();
  await voucherPageJS.addAccount('1930');
  await page.waitForTimeout(2000);
  await voucherPageJS.addCreditAmount('5000');
  await voucherPageJS.addAccount('1720');
  await page.waitForTimeout(2000);
  await voucherPageJS.addDebitAmount('5000', 1);
  const voucherName = `Voucher ${voucherNumber}, Automatkontering, ${today}`;
  await voucherPageJS.verifyName(voucherName);
  await page.waitForTimeout(2000);
  await voucherPageJS.verifyVoucherSeries('Automatkontering')
  await voucherPageJS.verifyStartDate(await getLastDateOfCurrentMonth());
  await voucherPageJS.verifyEndDate();
  await voucherPageJS.verifyNumberOfTimes();
  await page.waitForTimeout(4000);
  await voucherPageJS.verifyOppositeSign('100,00');
  await voucherPageJS.addNumberOfTimes(1);
  await voucherPageJS.verifyEndDateUpdated();
  await voucherPageJS.addCalculationType('Percent');
  await voucherPageJS.addAccountInPopup('5615');
  await voucherPageJS.save();
  await voucherPageJS.saveVoucher();
  await financeBasePage.goToMenu('Accounting', 'Accruals');
  await accuralsPageJS.filterbyVoucherNumber('Voucher ' + voucherNumber);
  await accuralsPageJS.selectAccrualsGridRow();
  await accuralsPageJS.clickAccurals();
  await accuralsPageJS.clickAlertMessage('OK');
  await accuralsPageJS.clickVoucherNumberEdit();
  await page.waitForTimeout(3000);
  await accuralsPageJS.verifyVoucherSeries('Automatkontering');
  await accuralsPageJS.verifyVoucherDate(await getLastDateOfCurrentMonth());
  await accuralsPageJS.verifyVoucherText(`Voucher ${voucherNumber}, Automatkontering, ${today}`);
  await page.waitForTimeout(3000);
  await accuralsPageJS.verifyCreditAmountandAccount('5000,00', '1720- 1720');
  await accuralsPageJS.verifyDebitAmountandAccount('5000,00', '5615- 5615');
  await accuralsPageJS.closeVoucher();
  await page.waitForTimeout(3000);
  await accuralsPageJS.filterbyVoucherNumber(voucherNumber);
  await accuralsPageJS.selectAccrualsGridRow();
  await accuralsPageJS.clickAccurals();
  await accuralsPageJS.verifyMessage('Only preliminary accruals can be accrued');
  await accuralsPageJS.clickAlertMessage('OK');
});



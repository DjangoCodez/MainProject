import { test } from '../../fixtures/finance-fixture';
import * as allure from "allure-js-commons";
import { getAccountExValue } from '../../../utils/properties';
import { VoucherUtil } from '../../../apis/utils/VoucherUtil';

let testCaseId: string = '77919';
let envUrl: string;
let text_1: string;
let accountNumber: string;
let voucherUtil: VoucherUtil;
let konto_1: string = '1010';
const amounts = [{ type: "debit", amount: 100, balance: 150, amount_2: 100 }, { type: "credit", amount: -100, balance: -310, amount_2: -100 }];

test.beforeEach(async ({ page, accountEx, financeBasePage }) => {
    await allure.parentSuite("Finance");
    await allure.suite("Accounting");
    await allure.subSuite("Voucher");
    envUrl = accountEx.baseUrl;
    await financeBasePage.SetupFinance(envUrl, getAccountExValue(accountEx, 'role')?.toString() ?? 'Admin');
    accountNumber = testCaseId + Math.floor(100000 + Math.random() * 900000);
    text_1 = 'Voucher_' + accountNumber;
    voucherUtil = new VoucherUtil(page, envUrl);
    await voucherUtil.createVoucher(text_1, amounts);
});

test(testCaseId + ': Voucher Balance Comparison : AP', { tag: ['@Finance', '@Accounting', '@Regression'] }, async ({ financeBasePage, voucherPageJS, reportPage }) => {
    await financeBasePage.goToMenu('Accounting', 'Voucher');
    await voucherPageJS.filterbyText(text_1);
    await voucherPageJS.editVoucher();
    const balance = await voucherPageJS.getBalance();
    await financeBasePage.toggleRightSideMenu('report', 'Reports');
    await reportPage.selectReport('Huvudbok 2dim');
    await reportPage.selectAccount(konto_1);
    await reportPage.createReport();
    await financeBasePage.selectSubMenu('Printed (queue)');
    await reportPage.printedQueueReportReload();
    await reportPage.rightClickReportPdf();
    const reportPath = await reportPage.openReport();
    await reportPage.rightClickReportPdf();
    await reportPage.removeReport();
    await reportPage.verifyTotalValueInPdf(reportPath, balance);
    await reportPage.deleteDownloadedFile(reportPath);
});



import { test } from '../../../fixtures/sales-fixture';
import * as allure from "allure-js-commons";
import { getAccountExValue } from '../../../../utils/properties';

let testCaseId: string = '118111';
let envUrl: string;

test.beforeEach(async ({ accountEx, staffBasePage }) => {
  await allure.parentSuite("Staff");
  await allure.suite("Employee");
  await allure.subSuite("Employee");
  await allure.link('https://dev.azure.com/softonedev/XE/_workitems/edit/' + testCaseId, "Salary Agreement : " + testCaseId);
  envUrl = accountEx.baseUrl;
  await staffBasePage.SetupStaff(envUrl, getAccountExValue(accountEx, 'role')?.toString() ?? 'Admin');
});

test(testCaseId + ': Salary Agreement : DK', { tag: ['@Staff', '@Employees', '@Regression'] }, async ({ salaryAgreementPage }) => {
  {
    await salaryAgreementPage.goToMenu('Employee', 'Salary agreement', true, 'Index');
    await salaryAgreementPage.waitForPageLoad();
    await salaryAgreementPage.createItem();
    let salaryAgreementName: string = 'salaryAgreement_' + Math.random().toString(36).substring(2, 7);
    await salaryAgreementPage.setName(salaryAgreementName);
    await salaryAgreementPage.setPeriodSet('Månad');
    await salaryAgreementPage.setSalaryType('Månadslön heltid');
    await salaryAgreementPage.setSalaryFormulas('INST - aktuell månadslön');
    await salaryAgreementPage.setCalculationOneTimeTax('INST - aktuell månadslön');
    await salaryAgreementPage.setCalculationValidMonths('INST - Beredskap helg fridag');
    await salaryAgreementPage.expandSettingsTab();
    await salaryAgreementPage.setNewHolidayAgreement('Handels Månadsavlönad');
    await salaryAgreementPage.expandReportsTab();
    await salaryAgreementPage.checkFirstReport('Anställningsbevis Anställd - Anställningsbevis detaljhandel');
    await salaryAgreementPage.expandStatisticsTab();
    await salaryAgreementPage.setSCBInfo('Workers', 'Day work', 'Monthly salary', 'INST - aktuell månadslön', '44', '101');
    await salaryAgreementPage.saveSalaryAgreement();
    await salaryAgreementPage.closeAllTabs();
    await salaryAgreementPage.filterBySalaryAgreementName(salaryAgreementName);
    await salaryAgreementPage.verifyRowCount();
  }
});
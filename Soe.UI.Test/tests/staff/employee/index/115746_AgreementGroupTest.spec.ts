import { test } from '../../../fixtures/sales-fixture';
import * as allure from "allure-js-commons";
import { getAccountExValue, getEnvironmentValue } from '../../../../utils/properties';

let testCaseId: string = '115746';
let envUrl: string;

test.beforeEach(async ({ accountEx, staffBasePage }) => {
  await allure.parentSuite("Staff");
  await allure.suite("Employee");
  await allure.subSuite("Employee");
  await allure.link('https://dev.azure.com/softonedev/XE/_workitems/edit/' + testCaseId, "Agreement Group : " + testCaseId);
  envUrl = accountEx.baseUrl;
  await staffBasePage.SetupStaff(envUrl, getAccountExValue(accountEx, 'role')?.toString() ?? 'Admin');
});

test(testCaseId + ': Agreement Group : DK', { tag: ['@Staff', '@Employees', '@Regression'] }, async ({ agreementGroupsPage }) => {
  {
    await agreementGroupsPage.goToMenu('Employee', 'Agreement groups', true, 'Index');
    await agreementGroupsPage.waitForPageLoad();
    await agreementGroupsPage.createItem();
    let agreementGroupCode: string = 'MT18' + Math.random().toString(36).substring(2, 7);
    let agreementGroupName: string = 'AGName' + Math.random().toString(36).substring(2, 7);
    await agreementGroupsPage.setCode(agreementGroupCode);
    await agreementGroupsPage.setName(agreementGroupName)
    await agreementGroupsPage.setDescription('Playwright test agreement group description');
    await agreementGroupsPage.selectTimeAgreement(getEnvironmentValue('default_timeAgreement2')?.toString() ?? '');
    await agreementGroupsPage.selectSalaryAgreement(getEnvironmentValue('default_salaryAgreement2')?.toString() ?? '');
    await agreementGroupsPage.selectHolidayAgreement(getEnvironmentValue('default_HolidayAgreement')?.toString() ?? '');
    await agreementGroupsPage.saveAgreementGroup();
    await agreementGroupsPage.closeTab();
    await agreementGroupsPage.filterByAgreementGroupName(agreementGroupName);
    await agreementGroupsPage.verifyFilteredRowCount('1');
    await agreementGroupsPage.editAgreementGroup();
    await agreementGroupsPage.setName(agreementGroupName + '_EDIT');
    await agreementGroupsPage.saveAgreementGroup();
    await agreementGroupsPage.closeTab();
    await agreementGroupsPage.filterByAgreementGroupName(agreementGroupName + '_EDIT');
    await agreementGroupsPage.verifyFilteredRowCount('1');
    await agreementGroupsPage.editAgreementGroup();
    await agreementGroupsPage.setActiveStatus(false);
    await agreementGroupsPage.saveAgreementGroup();
    await agreementGroupsPage.closeTab();
    await agreementGroupsPage.filterByAgreementGroupName(agreementGroupName + '_EDIT');
    await agreementGroupsPage.verifyFilteredRowCount('0');
    await agreementGroupsPage.clickGridActiveCheckbox();
    await agreementGroupsPage.verifyFilteredRowCount('1');
    await agreementGroupsPage.editAgreementGroup();
    await agreementGroupsPage.setActiveStatus(true);
    await agreementGroupsPage.saveAgreementGroup();
    await agreementGroupsPage.closeTab();
    await agreementGroupsPage.filterByAgreementGroupName(agreementGroupName + '_EDIT');
    await agreementGroupsPage.clickGridActiveCheckbox();
    await agreementGroupsPage.verifyFilteredRowCount('1');
    await agreementGroupsPage.editAgreementGroup();
    await agreementGroupsPage.deleteAgreementGroup();
    await agreementGroupsPage.filterByAgreementGroupName(agreementGroupName + '_EDIT');
    await agreementGroupsPage.verifyFilteredRowCount('0');
  }
});
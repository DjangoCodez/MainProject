import { test } from '../../fixtures/sales-fixture';
import * as allure from "allure-js-commons";
import { getAccountExValue } from '../../../utils/properties';
import { AgreementUtil } from '../../../apis/utils/AgreementUtil';

let testCaseId: string = '115108';
let envUrl: string;
let agreementGroupUtil: AgreementUtil;
const group = 'Agreement_' + testCaseId + Math.floor(100000 + Math.random() * 900000) + '_1';

test.beforeEach(async ({ page, accountEx, salesBasePage }) => {
    await allure.parentSuite("Sales");
    await allure.suite("Agreements");
    await allure.subSuite("Agreement Groups");
    await allure.link('https://dev.azure.com/softonedev/XE/_workitems/edit/' + testCaseId, "Test case : " + testCaseId);
    envUrl = accountEx.baseUrl;
    await salesBasePage.SetupSales(envUrl, getAccountExValue(accountEx, 'role')?.toString() ?? 'Admin');
    agreementGroupUtil = new AgreementUtil(page, envUrl);
    agreementGroupUtil.createAgreementGroup(group, 'Månad', 1, 1)
});

test(testCaseId + ': Delete Agreement Group : AP ', { tag: ['@Sales', '@Agreements', '@Regression'] }, async ({ salesBasePage, agreementsGroupsPage }) => {
    await salesBasePage.goToMenu('Agreements', 'Groups', true);
    await agreementsGroupsPage.searchContractGroupByName(group);
    await agreementsGroupsPage.editContractGroup();
    await agreementsGroupsPage.updateContractGroupName(group + ' EDIT');
    await agreementsGroupsPage.updateContractGroupDescription(group + ' DescEDIT');
    await agreementsGroupsPage.setInterval(2);
    await salesBasePage.save();
    await agreementsGroupsPage.closeTab();
    await agreementsGroupsPage.searchContractGroupByName(group + ' EDIT');
    await agreementsGroupsPage.editContractGroup();
    await agreementsGroupsPage.deleteContractGroup();
    await salesBasePage.goToMenu('Agreements', 'Groups', true);
    await agreementsGroupsPage.searchContractGroupByName(group + ' EDIT');
    await agreementsGroupsPage.verifyRowCount(0);
});



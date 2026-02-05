import { test } from '../../../fixtures/staff-fixture';
import * as allure from "allure-js-commons";
import { getAccountExValue } from '../../../../utils/properties';

let testCaseId: string = '119631';
let envUrl: string;
let terminalName : string = 'Auto-' + Math.floor(1000 + Math.random() * 9000)+ '_' + testCaseId;
let heading : string = 'Auto-' + Math.floor(1000 + Math.random() * 9000)+ '_' + testCaseId;

test.beforeEach(async ({ page, accountEx, staffBasePage }) => {
    await allure.parentSuite("Staff");
    await allure.suite("Time");
    await allure.subSuite("Attendance terminal");
    await allure.link('https://dev.azure.com/softonedev/XE/_workitems/edit/' + testCaseId, "Test case : " + testCaseId);
    envUrl = accountEx.baseUrl;
    await staffBasePage.SetupStaff(envUrl, getAccountExValue(accountEx, 'role')?.toString() ?? 'Admin');
});

test(testCaseId + ': Attendance Terminal : MG', { tag: ['@Staff', '@Time', '@Smoke'] }, async ({ staffBasePage, terminalsPage, attestTimePage }) => {
    await staffBasePage.goToMenu('Settings_Staff', 'Terminals',true,'Time');
    await terminalsPage.waitforPageLoad();
    await terminalsPage.createTerminalIfNotExists(testCaseId, terminalName, heading);
    await terminalsPage.openTerminalDetails(testCaseId);
    const punchTime = await terminalsPage.openTerminalPunchEmployee() ?? '';
    await staffBasePage.goToMenu('Time', 'Attest Time');
    await attestTimePage.waitforPageLoad();
    await attestTimePage.moveToCurrentMonth();
    await attestTimePage.searchEmployee('169', 'Anna Gustavsson');
    await attestTimePage.checkPunchTime(punchTime);
});

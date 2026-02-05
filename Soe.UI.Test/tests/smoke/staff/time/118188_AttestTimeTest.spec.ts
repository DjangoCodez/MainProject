import { test } from '../../../fixtures/staff-fixture';
import * as allure from "allure-js-commons";
import { getAccountExValue } from '../../../../utils/properties';

let testCaseId: string = '118188';
let envUrl: string;

test.beforeEach(async ({ accountEx, staffBasePage }) => {
    await allure.parentSuite("Staff");
    await allure.suite("Time");
    await allure.subSuite("Attest Time");
    await allure.link('https://dev.azure.com/softonedev/XE/_workitems/edit/' + testCaseId, "Test case : " + testCaseId);
    envUrl = accountEx.baseUrl;
    await staffBasePage.SetupStaff(envUrl, getAccountExValue(accountEx, 'role')?.toString() ?? 'Admin')
});

test(testCaseId + ': Attest Time : DK', { tag: ['@Staff', '@Time', '@Smoke'] }, async ({ basicSchedulePage, staffBasePage, attestTimePage }) => {
    await staffBasePage.goToMenu('Planning', 'Basic schedule');
    await basicSchedulePage.waitforPageLoad();
    await basicSchedulePage.chooseFilterAndEmployee("110");
    await basicSchedulePage.clickViewAll();
    await basicSchedulePage.page.waitForTimeout(2000);
    let attempts = 0;
    while (await basicSchedulePage.isScheduleActivated() && attempts < 3) {
        await basicSchedulePage.deactivateSchedule();
        attempts++;
        await basicSchedulePage.page.waitForTimeout(3000);
    }
    await basicSchedulePage.removeBasicScheduleIfPresent();
    await basicSchedulePage.addNewBasicSchedule();
    await basicSchedulePage.activateBasicSchedulewithEndDate();
    await basicSchedulePage.verifyScheduleActivated();
    await staffBasePage.goToMenu('Time', 'Attest Time');
    await attestTimePage.waitforPageLoad();
    await attestTimePage.moveToCurrentMonth();
    await attestTimePage.searchEmployee("110", "Arne Olsson");
    await attestTimePage.clickEditIconByRowIndex(0);
    await attestTimePage.createPunchesAccordingToSchdule();
    await attestTimePage.selectRowByIndex(1);
    await attestTimePage.clickFunctionButton();
    await attestTimePage.setAbsence("Föräldraledig");
    await attestTimePage.setToAttestationLevel("Attesterad");
    await attestTimePage.verifyAttestation("Attesterad");
    await attestTimePage.setToAttestationLevel("Registrerad");
    await attestTimePage.verifyAttestation("Registrerad");
    await attestTimePage.selectRowByIndex(1);
    await attestTimePage.clickFunctionButton();
    await attestTimePage.selectRestoreToActiveSchedule();
    await attestTimePage.clickEditIconByRowIndex(0);
    await attestTimePage.deletePunches();
    await attestTimePage.setMonthToAttesterad();
});
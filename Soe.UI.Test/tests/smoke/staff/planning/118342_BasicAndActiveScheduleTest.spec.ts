import { test } from '../../../fixtures/staff-fixture';
import * as allure from "allure-js-commons";
import { getAccountExValue } from '../../../../utils/properties';

let testCaseId: string = '118342';
let envUrl: string;

test.beforeEach(async ({ page, accountEx, staffBasePage }) => {
    await allure.parentSuite("Staff");
    await allure.suite("Planning");
    await allure.subSuite("Planning");
    await allure.link('https://dev.azure.com/softonedev/XE/_workitems/edit/' + testCaseId, "Test case : " + testCaseId);
    envUrl = accountEx.baseUrl;
    await staffBasePage.SetupStaff(envUrl, getAccountExValue(accountEx, 'role')?.toString() ?? 'Admin');
});

test(testCaseId + ': Basic schedule and active schedule : MG', { tag: ['@Staff', '@Planning', '@Smoke'] }, async ({ staffBasePage, basicSchedulePage }) => {
    await staffBasePage.goToMenu('Planning', 'Basic schedule');
    await basicSchedulePage.waitforPageLoad();
    await basicSchedulePage.chooseFilterAndEmployee('169');
    await basicSchedulePage.clickViewAll();
    await basicSchedulePage.page.waitForTimeout(2000);
    let attempts = 0;
    while (await basicSchedulePage.isScheduleActivated() && attempts < 3) {
        await basicSchedulePage.deactivateSchedule();
        attempts++;
        await basicSchedulePage.page.waitForTimeout(3000);
    }
    await basicSchedulePage.removeBasicScheduleIfPresent();
    await basicSchedulePage.addNewScheduleMonday();
    await basicSchedulePage.newShiftToday("08:00", "16:00", "12:00", "13:00");
    await basicSchedulePage.selectWeeks('Four weeks');
    await basicSchedulePage.verifyRepeatingShift(2);
    await basicSchedulePage.activateBasicSchedule();
    await basicSchedulePage.verifyScheduleActivated();
    await basicSchedulePage.selectActiveSchedule();
    await basicSchedulePage.verifyActiveScheduleRepeatingShift(2,'Butiksansvarig');
    await basicSchedulePage.dragDropShiftToNextDay();
    await basicSchedulePage.openNextDayShiftDetails();
    await basicSchedulePage.verifyLengthAndBreaks('08:00', '16:00', '8:00', '12:00', '13:00');
    await basicSchedulePage.setShiftEndTime('18:00');
    await basicSchedulePage.setShiftType(2);
    await basicSchedulePage.saveShiftDetails();
    await basicSchedulePage.printActiveSchedule('Veckoschema med färg');
    const report = await basicSchedulePage.openReport();
    await basicSchedulePage.verifyValueInPdf(report, 'Butiksansvarig', 'Chark', '640 Schema - Veckoschema med färg', '08:00-18:00');
});

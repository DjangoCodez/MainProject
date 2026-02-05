import { Page } from "@playwright/test";
import { TimeAPI } from "../TimeAPI";
import fs from 'fs'
import { setJsonValues } from "utils/CommonUtil";

export class EmployeeUtil {

    readonly page: Page;
    readonly dominaUrl: string;
    readonly timeapi: TimeAPI;
    readonly basePathJsons: string = './apis/jsons/';

    constructor(page: Page, url: string) {
        this.page = page;
        this.dominaUrl = url;
        this.timeapi = new TimeAPI(page, url);
    }

    async getEmployees() {
        const response = await this.timeapi.getEmployees();
        return response;
    }

    async getLastEmployeeNumber() {
        const employees = await this.getEmployees();
        const employeeNumbers = employees
            .map((emp: any) => parseInt(emp.employeeId))
            .filter((num: number) => !isNaN(num));
        const maxEmployeeNumber = Math.max(...employeeNumbers);
        return maxEmployeeNumber;
    }

    async getEmployeeBySSN(ssn: string) {
        console.log(`Searching employee with SSN: ${ssn}`);
        const employees = await this.getEmployees();
        return employees.find((emp: any) => emp.socialSec === ssn);
    }

    async validateEmployee({
        actorCompanyId,
        salaryAgreement,
        employmentTypeName,
        employeeGroupName,
        firstName,
        lastName,
        employeeNr,
        ssn,
        timeCodeId
    }: {
        actorCompanyId: number,
        salaryAgreement: string,
        employmentTypeName: string,
        employeeGroupName: string,
        firstName: string,
        lastName: string,
        employeeNr: number,
        ssn: string,
        timeCodeId: number
    }) {
        const employeeData = await this.getEmployeeValidationData({
            actorCompanyId,
            salaryAgreement,
            employmentTypeName,
            employeeGroupName,
            firstName,
            lastName,
            employeeNr,
            ssn,
            timeCodeId
        });
        const validationResult = await this.timeapi.validateEmployee(employeeData);
        if (!validationResult?.success) {
            console.warn(`Employee validation failed for SSN: ${ssn}`);
        } else {
            console.log(`Employee validation successful for SSN: ${ssn}`);
        }
        return {
            success: !!validationResult?.success,
            employeeData
        };
    }

    async createEmployee({
        actorCompanyId,
        salaryAgreement,
        employmentTypeName,
        employeeGroupName,
        firstName,
        lastName,
        ssn,
        timeCodeId
    }: {
        actorCompanyId: number,
        salaryAgreement: string,
        employmentTypeName: string,
        employeeGroupName: string,
        firstName: string,
        lastName: string,
        ssn: string,
        timeCodeId: number
    }) {
        const lastEmployeeId = await this.getLastEmployeeNumber();
        const nextEmployeeId = lastEmployeeId + 1;
        const { success, employeeData } = await this.validateEmployee({
            actorCompanyId,
            salaryAgreement,
            employmentTypeName,
            employeeGroupName,
            firstName,
            lastName,
            employeeNr: nextEmployeeId,
            ssn,
            timeCodeId
        });

        if (!success) {
            throw new Error('Employee data validation failed.');
        }
        const existingEmployee = await this.getEmployeeBySSN(ssn);
        if (!existingEmployee) {
            const payload = this.getEmployeeCreationData(employeeData);
            await this.timeapi.createEmployee(payload);
            const name = `${firstName} ${lastName}`;
            console.log(`Employee created: ${name} with Employee ID: ${nextEmployeeId}`);
            return { name, employeeNr: nextEmployeeId };
        } else {
            const name = `${existingEmployee.firstName ?? firstName} ${existingEmployee.lastName ?? lastName}`;
            console.log(`Employee already exists: ${name}`);
            return { name, employeeNr: existingEmployee.employeeNr };
        }
    }

    /**
     * Saves a time schedule template for an employee.
     * @param employeeSSN - The employee's social security number.
     * @param timeTemplateData - The time template configuration.
     * @returns An object containing the template name and ID.
     */
    async saveTimeSchedule(
        employeeSSN: string,
        timeTemplateData: {
            timeCodeId: number;
            shiftTypeId: number;
            name: string;
            startDate: string;
            firstMondayOfCycle: string;
            noOfDays: number;
        }
    ) {
        const employee = await this.getEmployeeBySSN(employeeSSN);

        if (!employee || !employee.employeeId) {
            throw new Error(`Employee with SSN ${employeeSSN} not found.`);
        }
        const { employeeId } = employee;
        const { timeCodeId, shiftTypeId, name, startDate, firstMondayOfCycle, noOfDays } = timeTemplateData;
        const activateSchedule = await this.getActiveTimeSchedules(employeeId, timeTemplateData.name);
        if (activateSchedule.length > 0) {
            return { name, timeScheduleId: activateSchedule[0].timeScheduleTemplateHeadId };
        }
        const data = {
            head: {
                state: 0,
                startOnFirstDayOfWeek: true,
                timeScheduleTemplatePeriods: this.generateTimeScheduleHeadBlocks({
                    timeCodeId,
                    shiftTypeId,
                    numberOfDays: noOfDays
                }),
                employeeSchedules: [],
                noOfDays: noOfDays,
                simpleSchedule: true,
                employeeId,
                name,
                startDate,
                firstMondayOfCycle
            },
            blocks: this.generateTimeScheduleBlock({ timeCodeId, shiftTypeId, numberOfDays: noOfDays })
        };
        try {
            const { integerValue } = await this.timeapi.createTimeTemplate(data);
            console.log(`Time template saved: ${name} with ID: ${integerValue}`);
            return { name, timeScheduleId: integerValue };
        } catch (error) {
            console.error(`Failed to save time template for employee ${employeeSSN}:`, error);
            throw error;
        }
    }

    private generateTimeScheduleBlock({
        timeCodeId,
        shiftTypeId,
        startTime = "1900-01-01T08:00:00.000Z",
        stopTime = "1900-01-01T17:00:00.000Z",
        numberOfDays = 7
    }: {
        timeCodeId: number,
        shiftTypeId: number,
        startTime?: string,
        stopTime?: string,
        numberOfDays?: number
    }) {
        return Array.from({ length: numberOfDays }, (_, i) => ({
            timeCodeId,
            dayNumber: i + 1,
            startTime,
            stopTime,
            shiftTypeId
        }));
    }

    private generateTimeScheduleHeadBlocks({
        timeCodeId,
        shiftTypeId,
        startTime = "1900-01-01T08:00:00.000Z",
        stopTime = "1900-01-01T17:00:00.000Z",
        numberOfDays = 7
    }: {
        timeCodeId: number,
        shiftTypeId: number,
        startTime?: string,
        stopTime?: string,
        numberOfDays?: number
    }) {
        return Array.from({ length: numberOfDays }, (_, i) => ({
            dayNumber: i + 1,
            timeScheduleTemplateBlocks: [],
            blocks: [
                {
                    timeCodeId,
                    dayNumber: i + 1,
                    startTime,
                    stopTime,
                    overlappingBreak1: false,
                    overlappingBreak2: false,
                    overlappingBreak3: false,
                    overlappingBreak4: false,
                    overlapping: false,
                    shiftTypeId
                }
            ]
        }));
    }

    private async getActiveTimeSchedules(empId: number, scheduleName: string) {
        const response = await this.timeapi.getActiveTimeSchedules(empId);
        return response.filter((schedule: { name: string; }) => schedule.name === scheduleName) as Array<{ timeScheduleTemplateHeadId: number; name: string }>;
    }

    /**
     * Activates an employee's schedule by SSN within a specified date range.
     * Handles control activation, recalculation, activation, and grid activation steps.
     * @param ssn - The employee's social security number.
     * @param startDate - The start date in ISO format.
     * @param stopDate - The stop date in ISO format.
     */
    async activateEmployeeSchedule(timeScheduleId: number, ssn: string, startDate: string, stopDate: string) {
        const employee = await this.getEmployeeBySSN(ssn);
        if (!employee || !employee.employeeId) {
            throw new Error(`Employee with SSN ${ssn} not found.`);
        }
        const { employeeId } = employee;
        console.log(`Starting control activation for employeeId: ${employeeId}...`);
        const controlResult = await this.controlActivation(employeeId, startDate, stopDate);
        if (!controlResult || !controlResult.key) {
            throw new Error(`Control activation failed for employeeId: ${employeeId}`);
        }
        const {
            key,
            hasWarnings,
            discardCheckesAll,
            discardCheckesForAbsence,
            discardCheckesForManuallyAdjusted
        } = controlResult;
        await this.timeapi.recalculateTimeHead(key);
        await this.page.waitForTimeout(5000);
        console.log(`Activating schedule for employeeId: ${employeeId}...`);
        try {
            await this.activateSchedule(
                timeScheduleId,
                key,
                hasWarnings,
                discardCheckesAll,
                discardCheckesForAbsence,
                discardCheckesForManuallyAdjusted,
                employeeId,
                startDate,
                stopDate
            );
        } catch (error) {
            console.warn(`Schedule activation failed, retrying once for employeeId: ${employeeId}...`);
            await this.page.waitForTimeout(3000);
            await this.activateSchedule(
                timeScheduleId,
                key,
                hasWarnings,
                discardCheckesAll,
                discardCheckesForAbsence,
                discardCheckesForManuallyAdjusted,
                employeeId,
                startDate,
                stopDate
            );
        }
        await this.timeapi.recalculateTimeHead(key);
        console.log(`Activating grid for employeeId: ${employeeId}...`);
        await this.activateGrid(employeeId);
        console.log(`Employee schedule activated for employeeId: ${employeeId}.`);
    }

    async getSalaryAgreements(name: string) {
        const agreements = await this.timeapi.getPayrollGroups();
        const agreement = agreements.find((agr: any) => agr.name === name);
        return agreement;
    }

    /**
     * Activates the grid for the specified employee.
     * @param employeeId - The ID of the employee.
     */
    private async activateGrid(employeeId: number) {
        const now = new Date();
        const dateFrom = now.toISOString();
        const dateTo = new Date(Date.UTC(now.getUTCFullYear() + 2, now.getUTCMonth(), now.getUTCDate())).toISOString();
        const data = {
            onlyLatest: false,
            addEmptyPlacement: false,
            employeeIds: [employeeId],
            dateFrom,
            dateTo
        };
        try {
            await this.timeapi.activateGrid(data);
            console.log(`Grid activated for employeeId: ${employeeId}`);
        } catch (error) {
            console.error(`Grid activation failed for employeeId ${employeeId}:`, error);
            throw error;
        }
    }

    /**
     * Activates an employee's schedule with the provided parameters.
     * @param key - The activation key.
     * @param hasWarnings - Indicates if there are warnings.
     * @param discardCheckesAll - Discard all checks.
     * @param discardCheckesForAbsence - Discard checks for absence.
     * @param discardCheckesForManuallyAdjusted - Discard checks for manually adjusted entries.
     * @param employeeId - The employee's ID.
     * @param startDate - The start date in ISO format.
     * @param stopDate - The stop date in ISO format.
     */
    private async activateSchedule(
        timeScheduleId: number,
        key: string,
        hasWarnings: boolean,
        discardCheckesAll: boolean,
        discardCheckesForAbsence: boolean,
        discardCheckesForManuallyAdjusted: boolean,
        employeeId: number,
        startDate: string,
        stopDate: string
    ) {
        const tiemeSchedules = await this.getTimeScheduleTemplatePeriod(timeScheduleId);
        const foundSchedule = tiemeSchedules.find((ts: any) => ts.dayNumber === 1);
        if (!foundSchedule) {
            throw new Error('No time schedule template period found with dayNumber === 1');
        }
        const {
            timeScheduleTemplatePeriodId,
            timeScheduleTemplateHeadId,
        } = foundSchedule;
        const data = {
            control: {
                key,
                hasWarnings,
                discardCheckesAll,
                discardCheckesForAbsence,
                discardCheckesForManuallyAdjusted
            },
            items: [{ employeeId }],
            function: 1,
            timeScheduleTemplateHeadId: timeScheduleTemplateHeadId,
            timeScheduleTemplatePeriodId: timeScheduleTemplatePeriodId,
            startDate,
            stopDate,
            preliminary: false
        };
        try {
            await this.timeapi.activateEmployeeSchedule(data);
            console.log(`Activation successful for employeeId: ${employeeId}`);
        } catch (error) {
            console.error(`Activation failed for employeeId ${employeeId}:`, error);
            throw error;
        }
    }
    /**
     * Retrieves the time schedule template periods for a given time schedule ID.
     * @param timeScheduleId - The ID of the time schedule template.
     * @returns An array of time schedule template period objects.
     */
    private async getTimeScheduleTemplatePeriod(timeScheduleId: number) {
        const periods = await this.timeapi.getTimeScheduleTemplatePeriod(timeScheduleId);
        return (periods ?? []).map((period: any) => ({
            timeScheduleTemplatePeriodId: period.timeScheduleTemplatePeriodId,
            timeScheduleTemplateHeadId: period.timeScheduleTemplateHeadId,
            dayNumber: period.dayNumber
        })) as Array<{
            timeScheduleTemplatePeriodId: number,
            timeScheduleTemplateHeadId: number,
            dayNumber: number
        }>;
    }

    /**
     * Performs control activation for an employee within a specified date range.
     * @param employeeId - The ID of the employee.
     * @param startDate - The start date in ISO format.
     * @param stopDate - The stop date in ISO format.
     * @returns The result of the control activation from the API.
     */
    private async controlActivation(employeeId: number, startDate: string, stopDate: string) {
        const payload = {
            items: [{ employeeId }],
            startDate,
            stopDate
        };
        try {
            return await this.timeapi.controlActivation(payload);
        } catch (error) {
            console.error(`Control activation failed for employeeId ${employeeId}:`, error);
            throw error;
        }
    }

    async getEmployeementDetails({
        actorCompanyId = 90,
        salaryAgreement = "HAO Månadslön",
        employmentTypeName = "Permanent Position",
        employeeGroupName = "118149_Time_Agreement"
    } = {}) {
        const [employmentVacationGroup, currentEmployeeGroup] = await Promise.all([
            this.getEmployeeVacationGroups([salaryAgreement]),
            this.getEmployeeTimeAgreement(employeeGroupName)
        ]);
        const dateFrom = new Date(Date.UTC(new Date().getUTCFullYear(), new Date().getUTCMonth(), 1)).toISOString(); // Employee's start date as first day of current month
        return {
            dateFrom,
            calculatedExperienceMonths: 0,
            employmentEndReason: 0,
            isSecondaryEmployment: false,
            isTemporaryPrimary: false,
            isAddingEmployment: true,
            isChangingEmployment: false,
            isChangingEmploymentDates: false,
            isDeletingEmployment: false,
            isEdited: true,
            isNewFromCopy: false,
            name: "",
            employmentType: 4,
            workTimeWeek: 2400,
            employeeGroupWorkTimeWeek: 0,
            percent: 100,
            experienceMonths: 0,
            experienceAgreedOrEstablished: true,
            workTasks: null,
            workPlace: null,
            specialConditions: null,
            state: 0,
            baseWorkTimeWeek: 0,
            substituteFor: null,
            substituteForDueTo: null,
            externalCode: null,
            employmentId: 0,
            actorCompanyId,
            employeeId: 0,
            employeeGroupId: currentEmployeeGroup.employeeGroupId,
            payrollGroupId: employmentVacationGroup.payrollGroupId,
            hibernatingTimeDeviationCauseId: null,
            priceTypes: null,
            accountingSettings: null,
            employmentTypeName,
            excludeFromWorkTimeWeekCalculationOnSecondaryEmployment: null,
            employmentEndReasonName: "",
            employeeGroupName,
            currentEmployeeGroup,
            payrollGroupName: salaryAgreement,
            employmentVacationGroup: employmentVacationGroup.vacationGroups
        };
    }

    async getEmployeeVacationGroups(payrollGroups: string[]) {
        let payrollObj: { payrollGroupId?: number; vacationGroups?: any[] } = {}
        const payroolGroups = await this.timeapi.getPayrollGroups();
        for (const payrollGroup of payrollGroups) {
            const { payrollGroupId } = payroolGroups.find((pg: any) => pg.name === payrollGroup);
            const vacationGroupsData = await this.timeapi.getVacationGroups(payrollGroupId);
            const vacationGroups: any[] = [];
            for (const vg of vacationGroupsData) {
                const { vacationGroupId, name, calculationType, vacationHandleRule, vacationDaysHandleRule } = vg;
                vacationGroups.push({
                    employmentId: 0,
                    vacationGroupId,
                    name,
                    calculationType,
                    vacationHandleRule,
                    vacationDaysHandleRule
                });
            }
            payrollObj = { payrollGroupId, vacationGroups };
        }
        return payrollObj;
    }

    async getEmployeeTimeAgreement(timeAgreementName: string) {
        const timeAgreements = await this.timeapi.getEmployeeTimeAgreements();
        const agreement = timeAgreements.find((ta: any) => ta.name === timeAgreementName);
        if (!agreement) {
            throw new Error(`Employee time agreement not found: ${timeAgreementName}`);
        }
        const { employeeGroupId, name, ruleWorkTimeWeek, autogenTimeblocks } = agreement;
        return {
            employeeGroupId,
            name,
            ruleWorkTimeWeek,
            autogenTimeblocks
        };
    }

    private getEmployeeCreationData(data: any = {}) {
        data.actionMethod = 4;
        data.employeePositions = [];
        data.employeeSkills = [];
        data.employeeTax = null;
        data.extraFields = [];
        data.files = [];
        data.employeeUser.name = `${data.employeeUser.firstName} ${data.employeeUser.lastName}`;
        return data;
    }

    private async getEmployeeValidationData({
        actorCompanyId,
        salaryAgreement,
        employmentTypeName,
        employeeGroupName,
        firstName,
        lastName,
        employeeNr,
        ssn,
        timeCodeId
    }: {
        actorCompanyId: number,
        salaryAgreement: string,
        employmentTypeName: string,
        employeeGroupName: string,
        firstName: string,
        lastName: string,
        employeeNr: number,
        ssn: string,
        timeCodeId: number
    }) {
        const employments = await this.getEmployeementDetails({
            actorCompanyId,
            salaryAgreement,
            employmentTypeName,
            employeeGroupName,
        });
        const email = `${firstName.trim().toLowerCase()}.${lastName.trim().toLowerCase()}@example.com`;
        const data: any = {
            employeeUser: {
                licenseId: 3,
                actorCompanyId,
                state: 0,
                employments: [employments],
                factors: [],
                unionFees: [],
                employeeTimeWorkAccounts: [],
                categoryRecords: [
                    {
                        categoryId: 18,
                        dateFrom: null,
                        dateTo: null,
                        isExecutive: false,
                        default: true
                    }
                ],
                calculatedCosts: [],
                employeeSkills: [],
                disbursementMethod: 1,
                timeDeviationCauseId: 0,
                timeCodeId: timeCodeId,
                langId: 2,
                isMobileUser: true,
                bygglosenMunicipalCode: "",
                bygglosenProfessionCategory: "",
                bygglosenLendedToOrgNr: "",
                employeeSettings: [],
                firstName,
                lastName,
                saveUser: true,
                disbursementClearingNr: "",
                disbursementAccountNr: "",
                dontValidateDisbursementAccountNr: false,
                employeeNr,
                loginName: employeeNr,
                socialSec: ssn,
                saveEmployee: true
            },
            contactAdresses: [
                {
                    contactAddressItemType: 11,
                    isAddress: false,
                    sysContactEComTypeId: 1,
                    name: "Email address",
                    displayAddress: email,
                    eComText: email,
                    ag_node_id: "0"
                }
            ]
        };
        return data;
    }
}
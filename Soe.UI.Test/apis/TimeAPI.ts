import { Page } from "@playwright/test";

export class TimeAPI {

    readonly page: Page;
    readonly dominaUrl: string;

    constructor(page: Page, url: string) {
        this.page = page;
        this.dominaUrl = url + "/api/";
    }

    async removeSchedule(jsonData: object) {
        const response = await this.page.request.post(this.dominaUrl + 'Time/Schedule/Shift/DeleteShifts/', { data: jsonData });
        if (!response.ok()) {
            throw new Error(`Failed to remove schedule: ${response.status()} ${response.statusText()}`);
        }
        const data = await response.json();
        return data;
    }

    async getEmployees(date?: string) {
        const dateParam = date || new Date().toISOString().replace(/[-:]/g, '').split('.')[0];
        const response = await this.page.request.get(this.dominaUrl + `Time/Employee/EmployeeForGrid/?date=${dateParam}&employeeFilter=null&loadPayrollGroups=true&showInactive=false&showEnded=false&showNotStarted=false&setAge=false&loadAnnualLeaveGroups=false`);
        if (!response.ok()) {
            throw new Error(`Failed to get employees: ${response.status()} ${response.statusText()}`);
        }
        const data = await response.json();
        return data;
    }

    async validateEmployee(jsonData: object) {
        const response = await this.page.request.post(this.dominaUrl + 'Time/Employee/ValidateSaveEmployee/', { data: jsonData });
        if (!response.ok()) {
            throw new Error(`Failed to validate employee: ${response.status()} ${response.statusText()}`);
        }
        const data = await response.json();
        return data;
    }

    async createEmployee(jsonData: object) {
        const response = await this.page.request.post(this.dominaUrl + 'Time/Employee/', { data: jsonData });
        if (!response.ok()) {
            throw new Error(`Failed to create employee: ${response.status()} ${response.statusText()}`);
        }
        const data = await response.json();
        return data;
    }

    async getTimeAgreements() {
        const url = this.dominaUrl + '/api/V2/Time/EmployeeGroup/Grid/';
        const response = await this.page.request.get(url);
        if (!response.ok()) {
            console.log(`Failed to get time agreements: ${response.status()} ${response.statusText()}`);
            return []
        }
        return await response.json() as [];
    }

    async createTimeAgreement(jsonData: object) {
        const response = await this.page.request.post(this.dominaUrl + 'V2/Time/EmployeeGroup', { data: jsonData });
        if (!response.ok()) {
            throw new Error(`Failed to create time agreement: ${response.status()} ${response.statusText()}`);
        }
        const data = await response.json();
        return data;
    }

    async createTimeTemplate(jsonData: object) {
        const response = await this.page.request.post(this.dominaUrl + 'Time/Schedule/TimeScheduleTemplate/SaveTimeScheduleTemplate/', { data: jsonData });
        if (!response.ok()) {
            throw new Error(`Failed to create time template: ${response.status()} ${response.statusText()}`);
        }
        const data = await response.json();
        return data;
    }

    async controlActivation(jsonData: object) {
        const response = await this.page.request.post(this.dominaUrl + 'Time/Schedule/EmployeeSchedule/ControlActivations/', { data: jsonData });
        if (!response.ok()) {
            throw new Error(`Failed to control activation: ${response.status()} ${response.statusText()}`);
        }
        const { key, hasWarnings, discardCheckesAll, discardCheckesForAbsence, discardCheckesForManuallyAdjusted } = await response.json();
        return {
            key,
            hasWarnings,
            discardCheckesAll,
            discardCheckesForAbsence,
            discardCheckesForManuallyAdjusted
        };
    }

    async getTimeScheduleTemplatePeriod(timeScheduleId: number) {
        const response = await this.page.request.get(this.dominaUrl + `Time/Schedule/TimeScheduleTemplatePeriod/Activate/${timeScheduleId}`);
        if (!response.ok()) {
            throw new Error(`Failed to get activate grid: ${response.status()} ${response.statusText()}`);
        }
        return await response.json() as [];
    }

    async activateEmployeeSchedule(jsonData: object) {
        const response = await this.page.request.post(this.dominaUrl + 'Time/Schedule/EmployeeSchedule/Activate/', { data: jsonData });
        if (!response.ok()) {
            throw new Error(`Failed to activate schedule: ${response.status()} ${response.statusText()}`);
        }
        const data = await response.json();
        return data;
    }

    async getActiveTimeSchedules(empId: number) {
        const url = this.dominaUrl + `Time/Schedule/TimeScheduleTemplateHead/${empId}/null/null/false/false/true`;
        const response = await this.page.request.get(url);
        if (!response.ok()) {
            throw new Error(`Failed to activate schedule: ${response.status()} ${response.statusText()}`);
        }
        const data = await response.json();
        return data as [];
    }

    async activateGrid(jsonData: object) {
        const response = await this.page.request.post(this.dominaUrl + 'Time/Schedule/EmployeeSchedule/ForActivateGrid/', { data: jsonData });
        if (!response.ok()) {
            throw new Error(`Failed to activate schedule grid: ${response.status()} ${response.statusText()}`);
        }
        const data = await response.json() as [];
        return data;
    }

    async recalculateTimeHead(key: string) {
        const url = this.dominaUrl + `Time/Schedule/RecalculateTimeHead/${key}`;
        const response = await this.page.request.get(url);
        if (!response.ok()) {
            throw new Error(`Failed to recalculate time head: ${response.status()} ${response.statusText()}`);
        }
        const data = await response.json();
        return data;
    }

    async getPayrollGroups() {
        const url = this.dominaUrl + 'Time/Payroll/PayrollGroup/?addEmptyRow=true';
        const response = await this.page.request.get(url);
        if (!response.ok()) {
            throw new Error(`Failed to get salary agreements: ${response.status()} ${response.statusText()}`);
        }
        const data = await response.json();
        return data;
    }

    async getVacationGroups(payrollGroupId: number) {
        const url = this.dominaUrl + `Time/Payroll/PayrollGroupVacationGroup/${payrollGroupId}/true`;
        const response = await this.page.request.get(url);
        if (!response.ok()) {
            throw new Error(`Failed to get vacation groups: ${response.status()} ${response.statusText()}`);
        }
        const data = await response.json();
        return data;
    }

    async getEmployeeTimeAgreements() {
        const url = this.dominaUrl + 'Time/Employee/EmployeeGroup/';
        const response = await this.page.request.get(url);
        if (!response.ok()) {
            throw new Error(`Failed to get time agreements: ${response.status()} ${response.statusText()}`);
        }
        const data = await response.json();
        return data;
    }
}
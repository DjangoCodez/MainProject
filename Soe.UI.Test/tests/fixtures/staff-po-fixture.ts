import { test as base } from '@playwright/test';
import { BasePage } from '../../pages/common/BasePage';
import { StaffBasePage } from '../../pages/staff/StaffBasePage';
import { EmployeesPage } from '../../pages/staff/employee/EmployeesPage';
import { AgreementGroupsPage } from '../../pages/staff/employee/index/AgreementGroupsPage';
import { BasicSchedulePage } from 'pages/staff/planning/BasicSchedulePage';
import { TerminalsPage } from 'pages/staff/settings/TeminalsPage';
import { AttestTimePage } from 'pages/staff/Time/AttestTimePage';
import { SalaryAgreementPage } from 'pages/staff/employee/index/SalaryAgreementPage';
import { PlanningPage } from 'pages/staff/settings/PlanningPage';
import { SalaryPayrollPage } from 'pages/staff/salary/SalaryPayrollPage';

type StaffFixtures = {
  basePage: BasePage;
  staffBasePage: StaffBasePage;
  employeesPage: EmployeesPage;
  agreementGroupsPage: AgreementGroupsPage;
  basicSchedulePage: BasicSchedulePage;
  terminalsPage: TerminalsPage;
  attestTimePage: AttestTimePage;
  salaryAgreementPage: SalaryAgreementPage;
  staffPlanningPage: PlanningPage;
  salaryPayrollPage: SalaryPayrollPage;
};

export const test = base.extend<StaffFixtures>({
  staffPlanningPage: async ({ page }, use) => {
    await use(new PlanningPage(page));
  },
  employeesPage: async ({ page }, use) => {
    await use(new EmployeesPage(page));
  },
  basePage: async ({ page }, use) => {
    await use(new BasePage(page));
  },
  staffBasePage: async ({ page }, use) => {
    await use(new StaffBasePage(page));
  },
  agreementGroupsPage: async ({ page }, use) => {
    await use(new AgreementGroupsPage(page));
  },
  basicSchedulePage: async ({ page }, use) => {
    await use(new BasicSchedulePage(page));
  },
  terminalsPage: async ({ page }, use) => {
    await use(new TerminalsPage(page));
  },
  attestTimePage: async ({ page }, use) => {
    await use(new AttestTimePage(page));
  },
  salaryAgreementPage: async ({ page }, use) => {
    await use(new SalaryAgreementPage(page));
  },
  salaryPayrollPage: async ({ page }, use) => {
    await use(new SalaryPayrollPage(page));
  },
});
export { expect } from '@playwright/test';
import '../Module';
import '../../../Common/GoogleMaps/Module';

import { ScheduleService as SharedScheduleService } from "../../../Shared/Time/Schedule/Scheduleservice";
import { TimeService as SharedTimeService } from "../../../Shared/Time/Time/TimeService";
import { EmployeeService } from '../../Employee/EmployeeService';
import { EmployeeService as SharedEmployeeService } from "../../../Shared/Time/Employee/EmployeeService";
import { AccountingSettingsDirectiveFactory } from '../../../Common/Directives/AccountingSettings/AccountingSettingsDirective';
import { ContactAddressesDirectiveFactory } from '../../../Common/Directives/ContactAddresses/ContactAddressesDirective';
import { EmploymentsDirectiveFactory } from "../../Employee/Employees/Directives/Employments/EmploymentsDirective";
import { EmploymentVacationGroupsDirectiveFactory } from '../../Employee/Employees/Directives/EmploymentVacationGroups/EmploymentVacationGroupsDirective';
import { EmploymentPriceTypesDirectiveFactory } from '../../Employee/Employees/Directives/EmploymentPriceTypes/EmploymentPriceTypesDirective';
import { EmploymentPriceFormulasDirectiveFactory } from '../../Employee/Employees/Directives/EmploymentPriceFormulas/EmploymentPriceFormulasDirective';
import { EmployeeAccountsDirectiveFactory } from '../../Employee/Employees/Directives/EmployeeAccounts/EmployeeAccountsDirective';
import { EmployeeTaxDirectiveFactory } from "../../Employee/Employees/Directives/Tax/EmployeeTaxDirective";
import { EmployeeChildsDirectiveFactory } from '../../Employee/Employees/Directives/EmployeeChilds/EmployeeChildsDirecitive';
import { EmployeeFactorsDirectiveFactory } from '../../Employee/Employees/Directives/EmployeeFactors/EmployeeFactorsDirective';
import { EmployeeVacationDirectiveFactory } from '../../Employee/Employees/Directives/EmployeeVacation/EmployeeVacationDirective';
import { SkillsDirectiveFactory } from "../../../Common/Directives/Skills/SkillsDirective";
import { SkillMatcherDirectiveFactory } from '../../../Shared/Time/Directives/SkillMatcher/SkillMatcherDirective';
import { PositionsDirectiveFactory } from "../../../Common/Directives/Positions/PositionsDirective";
import { CategoriesDirectiveFactory } from '../../../Common/Directives/Categories/CategoriesDirective';
import { SelectEmployeesDirectiveFactory } from '../../Employee/Employees/Directives/SelectEmployees/SelectEmployeesDirective';
import { SelectAttestRolesDirectiveFactory } from '../../Employee/Employees/Directives/SelectAttestRoles/SelectAttestRolesDirective';
import { EmployeeValidationDirectiveFactory } from '../../Employee/Employees/Directives/EmployeeValidationDirective';
import { EntityLogViewerDirectiveFactory } from '../../../Common/Directives/EntityLogViewer/EntityLogViewerDirective';
import { UserDirectiveFactory } from '../../../Common/Directives/User/UserDirective';
import { UserRolesDirectiveFactory } from '../../../Common/Directives/UserRoles/UserRolesDirective';
import { AdditionDeductionsDirectiveFactory } from '../../../Common/Directives/AdditionDeductions/AdditionDeductionsDirective';
import { AccountingService as SharedAccountingService } from "../../../Shared/Economy/Accounting/AccountingService";
import { PayrollCalculationGroupDirectiveFactory } from './DIrectives/PayrollCalculationGroup/PayrollCalculationGroupDirective';
import { PayrollCalculationGroupChartsDirectiveFactory } from './DIrectives/PayrollCalculationGroupCharts/PayrollCalculationGroupChartsDirective';
import { PayrollCalculationEmployeeDirectiveFactory } from './DIrectives/PayrollCalculationEmployee/PayrollCalculationEmployeeDirective';
import { PayrollCalculationEmployeeChartsDirectiveFactory } from './DIrectives/PayrollCalculationEmployeeCharts/PayrollCalculationEmployeeChartsDirective';
import { PayrollCalculationFixedDirectiveFactory } from './DIrectives/PayrollCalculationFixed/PayrollCalculationFixedDirective';
import { PayrollCalculationRetroactiveDirectiveFactory } from './DIrectives/PayrollCalculationRetroactive/PayrollCalculationRetroactiveDirective';
import { ScheduleDirectiveFactory as EmployeeScheduleDirectiveFactory } from '../../Employee/Employees/Directives/Schedule/ScheduleDirective';
import { ScheduleService } from '../../Schedule/ScheduleService';
import { EditPlacementValidationDirectiveFactory } from '../../Employee/Employees/Directives/Schedule/EditPlacementValidationDirective';
import { TimeService } from '../../Time/TimeService';
import { ExtraFieldRecordsDirectiveFactory } from '../../../Common/Directives/ExtraFields/ExtraFieldsDirective';
import { AbsenceDetailsDirectiveFactory } from '../../Directives/AbsenceDetails/AbsenceDetailsDirective';
import { EmployeeTimeWorkAccountDirectiveFactory } from '../../Employee/Employees/Directives/EmployeeTimeWorkAccount/EmployeeTimeWorkAccountDirective';
import { EmployeeSettingsDirectiveFactory } from '../../Employee/Employees/Directives/EmployeeSettings/EmployeeSettingsDirective';
import { EmployeeSettingDialogValidationDirectiveFactory } from '../../Employee/Employees/Directives/EmployeeSettings/EmployeeSettingDialogValidationDirective';

angular.module("Soe.Time.Payroll.PayrollCalculation.Module", ['Soe.Time.Payroll', 'Soe.Common.GoogleMaps'])
    .service("scheduleService", ScheduleService)
    .service("sharedScheduleService", SharedScheduleService)
    .service("employeeService", EmployeeService)
    .service("sharedEmployeeService", SharedEmployeeService)
    .service("sharedAccountingService", SharedAccountingService)
    .service("timeService", TimeService)
    .service("sharedTimeService", SharedTimeService)
    .directive("payrollCalculationGroup", PayrollCalculationGroupDirectiveFactory.create)
    .directive("payrollCalculationGroupCharts", PayrollCalculationGroupChartsDirectiveFactory.create)
    .directive("payrollCalculationEmployee", PayrollCalculationEmployeeDirectiveFactory.create)
    .directive("payrollCalculationEmployeeCharts", PayrollCalculationEmployeeChartsDirectiveFactory.create)
    .directive("payrollCalculationFixed", PayrollCalculationFixedDirectiveFactory.create)
    .directive("payrollCalculationRetroactive", PayrollCalculationRetroactiveDirectiveFactory.create)
    .directive("contactAddresses", ContactAddressesDirectiveFactory.create)
    .directive("employments", EmploymentsDirectiveFactory.create)
    .directive("employmentVacationGroups", EmploymentVacationGroupsDirectiveFactory.create)
    .directive("employmentPriceTypes", EmploymentPriceTypesDirectiveFactory.create)
    .directive("employmentPriceFormulas", EmploymentPriceFormulasDirectiveFactory.create)
    .directive("employeeAccounts", EmployeeAccountsDirectiveFactory.create)
    .directive("employeeTax", EmployeeTaxDirectiveFactory.create)
    .directive("employeeChilds", EmployeeChildsDirectiveFactory.create)
    .directive("employeeFactors", EmployeeFactorsDirectiveFactory.create)
    .directive("employeeSettings", EmployeeSettingsDirectiveFactory.create)
    .directive("employeeSettingDialogValidation", EmployeeSettingDialogValidationDirectiveFactory.create)
    .directive("employeeVacation", EmployeeVacationDirectiveFactory.create)
    .directive("schedule", EmployeeScheduleDirectiveFactory.create)
    .directive("editPlacementValidation", EditPlacementValidationDirectiveFactory.create)
    .directive("skills", SkillsDirectiveFactory.create)
    .directive("skillMatcher", SkillMatcherDirectiveFactory.create)
    .directive("positions", PositionsDirectiveFactory.create)
    .directive("categories", CategoriesDirectiveFactory.create)
    .directive("accountingSettings", AccountingSettingsDirectiveFactory.create)
    .directive("user", UserDirectiveFactory.create)
    .directive("userRoles", UserRolesDirectiveFactory.create)
    .directive("selectEmployees", SelectEmployeesDirectiveFactory.create)
    .directive("selectAttestRoles", SelectAttestRolesDirectiveFactory.create)
    .directive("employeeValidation", EmployeeValidationDirectiveFactory.create)
    .directive("additionDeductions", AdditionDeductionsDirectiveFactory.create)
    .directive("absenceDetails", AbsenceDetailsDirectiveFactory.create)
    .directive("entityLogViewer", EntityLogViewerDirectiveFactory.create)
    .directive("extraFields", ExtraFieldRecordsDirectiveFactory.create)
    .directive("employeeTimeWorkAccount", EmployeeTimeWorkAccountDirectiveFactory.create);

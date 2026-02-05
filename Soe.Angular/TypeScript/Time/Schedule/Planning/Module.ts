import '../Module';
import "../../../Shared/Billing/Module";
import '../../../Shared/Time/Schedule/Absencerequests/Module'

import { TimeService } from "../../Time/timeservice";
import { TimeService as SharedTimeService } from "../../../Shared/Time/Time/TimeService";
import { AvailableEmployeesDirectiveFactory } from "../../../Common/Directives/AvailableEmployees/AvailableEmployeesDirective";
import { SkillsDirectiveFactory } from "../../../Common/Directives/Skills/SkillsDirective";
import { SkillMatcherDirectiveFactory } from '../../../Shared/Time/Directives/SkillMatcher/SkillMatcherDirective';
import { ShiftQueueDirectiveFactory } from "../../../Shared/Time/Directives/ShiftQueue/ShiftQueueDirective";
import { ShiftTasksDirectiveFactory } from "../../../Shared/Time/Directives/ShiftTasks/ShiftTasks";
import { CopyScheduleValidationDirectiveFactory } from "./Dialogs/CopySchedule/CopyScheduleValidationDirective";
import { TemplateScheduleValidationDirectiveFactory } from "./Dialogs/TemplateSchedule/TemplateScheduleValidationDirective";
import { EmployeeService } from "../../Employee/EmployeeService";
import { EmployeeService as SharedEmployeeService } from "../../../Shared/Time/Employee/EmployeeService";
import { AccountingService as SharedAccountingService } from '../../../Shared/Economy/Accounting/AccountingService';
import { ScheduleService as SharedScheduleService } from "../../../Shared/Time/Schedule/ScheduleService";
import { PayrollService } from '../../Payroll/PayrollService';
import { EmploymentsDirectiveFactory } from "../../Employee/Employees/Directives/Employments/EmploymentsDirective";
import { EmploymentVacationGroupsDirectiveFactory } from '../../Employee/Employees/Directives/EmploymentVacationGroups/EmploymentVacationGroupsDirective';
import { EmploymentPriceTypesDirectiveFactory } from '../../Employee/Employees/Directives/EmploymentPriceTypes/EmploymentPriceTypesDirective';
import { EmploymentPriceFormulasDirectiveFactory } from '../../Employee/Employees/Directives/EmploymentPriceFormulas/EmploymentPriceFormulasDirective';
import { EmployeeAccountsDirectiveFactory } from '../../Employee/Employees/Directives/EmployeeAccounts/EmployeeAccountsDirective';
import { EmployeeTaxDirectiveFactory } from "../../Employee/Employees/Directives/Tax/EmployeeTaxDirective";
import { EmployeeChildsDirectiveFactory } from '../../Employee/Employees/Directives/EmployeeChilds/EmployeeChildsDirecitive';
import { EmployeeFactorsDirectiveFactory } from '../../Employee/Employees/Directives/EmployeeFactors/EmployeeFactorsDirective';
import { EmployeeVacationDirectiveFactory } from '../../Employee/Employees/Directives/EmployeeVacation/EmployeeVacationDirective';
import { PositionsDirectiveFactory } from "../../../Common/Directives/Positions/PositionsDirective";
import { UserDirectiveFactory } from '../../../Common/Directives/User/UserDirective';
import { UserRolesDirectiveFactory } from '../../../Common/Directives/UserRoles/UserRolesDirective';
import { SelectEmployeesDirectiveFactory } from '../../Employee/Employees/Directives/SelectEmployees/SelectEmployeesDirective';
import { SelectAttestRolesDirectiveFactory } from '../../Employee/Employees/Directives/SelectAttestRoles/SelectAttestRolesDirective';
import { EmployeeValidationDirectiveFactory } from '../../Employee/Employees/Directives/EmployeeValidationDirective';
import { EntityLogViewerDirectiveFactory } from '../../../Common/Directives/EntityLogViewer/EntityLogViewerDirective';
import { ActivateValidationDirectiveFactory } from '../Activate/ActivateValidationDirective';
import { ScheduleDirectiveFactory } from '../../Employee/Employees/Directives/Schedule/ScheduleDirective';
import { EditPlacementValidationDirectiveFactory } from '../../Employee/Employees/Directives/Schedule/EditPlacementValidationDirective';
import { EditEmployeeAvailabilityValidationDirectiveFactory } from '../../../Common/Dialogs/EditEmployeeAvailability/EditEmployeeAvailabilityValidationDirective';
import { ExtraFieldRecordsDirectiveFactory } from '../../../Common/Directives/ExtraFields/ExtraFieldsDirective';
import { TemplateDesignerDirectiveFactory } from '../../Employee/EmployeeTemplates/Directives/TemplateDesigner/TemplateDesignerDirective';
import { EmployeeTemplatesValidationDirectiveFactory } from '../../Employee/EmployeeTemplates/Directives/EmployeeTemplatesValidationDirective';
import { CreateFromTemplateValidationDirectiveFactory } from '../../Employee/Employees/Dialogs/CreateFromTemplate/CreateFromTemplateValidationDirective';
import { EmployeeAccountFactory } from '../../Employee/EmployeeTemplates/Directives/TemplateDesigner/Components/EmployeeAccount/EmployeeAccountController';
import { DisbursementAccountFactory } from '../../Employee/EmployeeTemplates/Directives/TemplateDesigner/Components/DisbursementAccount/DisbursementAccountController';
import { EmploymentPriceTypesFactory } from '../../Employee/EmployeeTemplates/Directives/TemplateDesigner/Components/EmploymentPriceTypes/EmploymentPriceTypesController';

import 'angularjs-gauge';
import 'file-saver';
import 'xlsx';
import 'angular-in-viewport';
import { PositionFactory } from '../../Employee/EmployeeTemplates/Directives/TemplateDesigner/Components/Position/PositionController';
import { EmployeeSettingsDirectiveFactory } from '../../Employee/Employees/Directives/EmployeeSettings/EmployeeSettingsDirective';
import { EmployeeSettingDialogValidationDirectiveFactory } from '../../Employee/Employees/Directives/EmployeeSettings/EmployeeSettingDialogValidationDirective';

angular.module("Soe.Time.Schedule.Planning.Module", ['Soe.Time.Schedule', 'angularjs-gauge', 'Soe.Shared.Billing', 'in-viewport', 'Soe.Shared.Time.Schedule.Absencerequests.Module', 'Soe.Common.Dialogs.AddDocumentToAttestFlow'])
    .service("timeService", TimeService)
    .service("sharedTimeService", SharedTimeService)
    .directive("availableEmployees", AvailableEmployeesDirectiveFactory.create)
    .directive("skills", SkillsDirectiveFactory.create)
    .directive("skillMatcher", SkillMatcherDirectiveFactory.create)
    .directive("shiftQueue", ShiftQueueDirectiveFactory.create)
    .directive("shiftTasks", ShiftTasksDirectiveFactory.create)
    .directive("copyScheduleValidation", CopyScheduleValidationDirectiveFactory.create)
    .directive("templateScheduleValidation", TemplateScheduleValidationDirectiveFactory.create)
    // Employee
    .service("employeeService", EmployeeService)
    .service("sharedEmployeeService", SharedEmployeeService)
    .service("sharedAccountingService", SharedAccountingService)
    .service("sharedScheduleService", SharedScheduleService)
    .service("payrollService", PayrollService)
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
    .directive("schedule", ScheduleDirectiveFactory.create)
    .directive("editPlacementValidation", EditPlacementValidationDirectiveFactory.create)
    .directive("positions", PositionsDirectiveFactory.create)
    .directive("user", UserDirectiveFactory.create)
    .directive("userRoles", UserRolesDirectiveFactory.create)
    .directive("selectEmployees", SelectEmployeesDirectiveFactory.create)
    .directive("selectAttestRoles", SelectAttestRolesDirectiveFactory.create)
    .directive("employeeValidation", EmployeeValidationDirectiveFactory.create)
    .directive("entityLogViewer", EntityLogViewerDirectiveFactory.create)
    .directive("extraFields", ExtraFieldRecordsDirectiveFactory.create)
    // EmployeeTemplate
    .directive("templateDesigner", TemplateDesignerDirectiveFactory.create)
    .directive("employeeTemplatesValidation", EmployeeTemplatesValidationDirectiveFactory.create)
    .directive("createFromTemplateValidation", CreateFromTemplateValidationDirectiveFactory.create)
    .directive("employeeAccountComponent", EmployeeAccountFactory.create)
    .directive("disbursementAccountComponent", DisbursementAccountFactory.create)
    .directive("employmentPriceTypesComponent", EmploymentPriceTypesFactory.create)
    .directive("positionComponent", PositionFactory.create)
    // Activate
    .directive("activateValidation", ActivateValidationDirectiveFactory.create)
    // Availability
    .directive("editEmployeeAvailabilityValidation", EditEmployeeAvailabilityValidationDirectiveFactory.create);


import '../Module';
import '../../../Common/GoogleMaps/Module';
import '../../../Common/Dialogs/AddDocumentToAttestFlow/Module';

import { AccountingService as SharedAccountingService } from '../../../Shared/Economy/Accounting/AccountingService';
import { EmployeeService as SharedEmployeeService } from '../../../Shared/Time/Employee/EmployeeService';
import { ScheduleService as SharedScheduleService } from "../../../Shared/Time/Schedule/Scheduleservice";
import { ScheduleService } from '../../Schedule/ScheduleService';
import { PayrollService } from '../../Payroll/PayrollService';
import { ContactAddressesDirectiveFactory } from '../../../Common/Directives/ContactAddresses/ContactAddressesDirective';
import { EmploymentsDirectiveFactory } from "./Directives/Employments/EmploymentsDirective";
import { EmploymentVacationGroupsDirectiveFactory } from './Directives/EmploymentVacationGroups/EmploymentVacationGroupsDirective';
import { EmploymentPriceTypesDirectiveFactory } from './Directives/EmploymentPriceTypes/EmploymentPriceTypesDirective';
import { EmploymentPriceFormulasDirectiveFactory } from './Directives/EmploymentPriceFormulas/EmploymentPriceFormulasDirective';
import { EmployeeAccountsDirectiveFactory } from './Directives/EmployeeAccounts/EmployeeAccountsDirective';
import { EmployeeTaxDirectiveFactory } from "./Directives/Tax/EmployeeTaxDirective";
import { EmployeeTimeWorkAccountDirectiveFactory } from "./Directives/EmployeeTimeWorkAccount/EmployeeTimeWorkAccountDirective";
import { EmployeeChildsDirectiveFactory } from './Directives/EmployeeChilds/EmployeeChildsDirecitive';
import { EmployeeFactorsDirectiveFactory } from './Directives/EmployeeFactors/EmployeeFactorsDirective';
import { EmployeeVacationDirectiveFactory } from './Directives/EmployeeVacation/EmployeeVacationDirective';
import { SkillsDirectiveFactory } from "../../../Common/Directives/Skills/SkillsDirective";
import { SkillMatcherDirectiveFactory } from '../../../Shared/Time/Directives/SkillMatcher/SkillMatcherDirective';
import { PositionsDirectiveFactory } from "../../../Common/Directives/Positions/PositionsDirective";
import { CategoriesDirectiveFactory } from '../../../Common/Directives/Categories/CategoriesDirective';
import { AccountingSettingsDirectiveFactory } from '../../../Common/Directives/AccountingSettings/AccountingSettingsDirective';
import { SelectEmployeesDirectiveFactory } from './Directives/SelectEmployees/SelectEmployeesDirective';
import { SelectAttestRolesDirectiveFactory } from './Directives/SelectAttestRoles/SelectAttestRolesDirective';
import { EmployeeValidationDirectiveFactory } from './Directives/EmployeeValidationDirective';
import { EntityLogViewerDirectiveFactory } from '../../../Common/Directives/EntityLogViewer/EntityLogViewerDirective';
import { UserDirectiveFactory } from '../../../Common/Directives/User/UserDirective';
import { UserRolesDirectiveFactory } from '../../../Common/Directives/UserRoles/UserRolesDirective';
import { EmployeeCalculatedCostsDirectiveFactory } from './Directives/EmployeeCalculatedCosts/EmployeeCalculatedCostsDirective';
import { ScheduleDirectiveFactory } from './Directives/Schedule/ScheduleDirective';
import { EditPlacementValidationDirectiveFactory } from './Directives/Schedule/EditPlacementValidationDirective';
import { TemplatesValidationDirectiveFactory } from '../../Schedule/Templates/TemplatesValidationDirective';
import { ExtraFieldRecordsDirectiveFactory } from '../../../Common/Directives/ExtraFields/ExtraFieldsDirective';
import { TemplateDesignerDirectiveFactory } from '../EmployeeTemplates/Directives/TemplateDesigner/TemplateDesignerDirective';
import { EmployeeTemplatesValidationDirectiveFactory } from '../EmployeeTemplates/Directives/EmployeeTemplatesValidationDirective';
import { EmployeeAccountFactory } from '../EmployeeTemplates/Directives/TemplateDesigner/Components/EmployeeAccount/EmployeeAccountController';
import { DisbursementAccountFactory } from '../EmployeeTemplates/Directives/TemplateDesigner/Components/DisbursementAccount/DisbursementAccountController';
import { EmploymentPriceTypesFactory } from '../EmployeeTemplates/Directives/TemplateDesigner/Components/EmploymentPriceTypes/EmploymentPriceTypesController';
import { CreateFromTemplateValidationDirectiveFactory } from './Dialogs/CreateFromTemplate/CreateFromTemplateValidationDirective';
import { EmployeeSettingsDirectiveFactory } from './Directives/EmployeeSettings/EmployeeSettingsDirective';
import { EmployeeSettingDialogValidationDirectiveFactory } from './Directives/EmployeeSettings/EmployeeSettingDialogValidationDirective';
import { PositionFactory } from '../EmployeeTemplates/Directives/TemplateDesigner/Components/Position/PositionController';

angular.module("Soe.Time.Employee.Employees.Module", ['Soe.Time.Employee', 'Soe.Common.GoogleMaps', 'Soe.Common.Dialogs.AddDocumentToAttestFlow'])
    .service("sharedAccountingService", SharedAccountingService)
    .service("sharedEmployeeService", SharedEmployeeService)
    .service("sharedScheduleService", SharedScheduleService)
    .service("scheduleService", ScheduleService)
    .service("payrollService", PayrollService)
    .directive("templateDesigner", TemplateDesignerDirectiveFactory.create)
    .directive("employeeTemplatesValidation", EmployeeTemplatesValidationDirectiveFactory.create)
    .directive("createFromTemplateValidation", CreateFromTemplateValidationDirectiveFactory.create)
    .directive("employeeAccountComponent", EmployeeAccountFactory.create)
    .directive("disbursementAccountComponent", DisbursementAccountFactory.create)
    .directive("employmentPriceTypesComponent", EmploymentPriceTypesFactory.create)
    .directive("positionComponent", PositionFactory.create)
    .directive("contactAddresses", ContactAddressesDirectiveFactory.create)
    .directive("employments", EmploymentsDirectiveFactory.create)
    .directive("employmentVacationGroups", EmploymentVacationGroupsDirectiveFactory.create)
    .directive("employmentPriceTypes", EmploymentPriceTypesDirectiveFactory.create)
    .directive("employmentPriceFormulas", EmploymentPriceFormulasDirectiveFactory.create)
    .directive("employeeAccounts", EmployeeAccountsDirectiveFactory.create)
    .directive("employeeTax", EmployeeTaxDirectiveFactory.create)
    .directive("employeeTimeWorkAccount", EmployeeTimeWorkAccountDirectiveFactory.create)
    .directive("employeeChilds", EmployeeChildsDirectiveFactory.create)
    .directive("employeeFactors", EmployeeFactorsDirectiveFactory.create)
    .directive("employeeSettings", EmployeeSettingsDirectiveFactory.create)
    .directive("employeeSettingDialogValidation", EmployeeSettingDialogValidationDirectiveFactory.create)
    .directive("employeeVacation", EmployeeVacationDirectiveFactory.create)
    .directive("schedule", ScheduleDirectiveFactory.create)
    .directive("templatesValidation", TemplatesValidationDirectiveFactory.create)
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
    .directive("entityLogViewer", EntityLogViewerDirectiveFactory.create)
    .directive("employeeCalculatedCosts", EmployeeCalculatedCostsDirectiveFactory.create)
    .directive("extraFields", ExtraFieldRecordsDirectiveFactory.create);

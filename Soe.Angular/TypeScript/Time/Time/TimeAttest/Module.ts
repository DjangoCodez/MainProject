import '../Module';
import '../../../Common/GoogleMaps/Module';
import'../../../Shared/Time/Schedule/Absencerequests/Module'

import { ScheduleService as SharedScheduleService } from "../../../Shared/Time/Schedule/ScheduleService";
import { EmployeeService as SharedEmployeeService } from "../../../Shared/Time/Employee/EmployeeService";
import { EmployeeService } from '../../Employee/EmployeeService';
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
import { AttestGroupDirectiveFactory } from '../../Directives/AttestGroup/AttestGroupDirective';
import { AttestEmployeeDirectiveFactory } from '../../Directives/AttestEmployee/AttestEmployeeDirective';
import { PayrollService } from '../../Payroll/PayrollService';
import { RowDetailDirectiveFactory } from '../../Directives/AttestEmployee/RowDetailDirective';
import { ScheduleDirectiveFactory } from '../../Directives/AttestEmployee/ScheduleDirective';
import { ScheduleService } from '../../Schedule/ScheduleService';
import { TimeStampDirectiveFactory } from '../../Directives/AttestEmployee/TimeStampDirective';
import { TimePayrollTransactionDirectiveFactory } from '../../Directives/AttestEmployee/TimePayrollTransactionDirective';
import { TimeCodeTransactionDirectiveFactory } from '../../Directives/AttestEmployee/TimeCodeTransactionDirective';
import { PayrollImportTransactionDirectiveFactory } from '../../Directives/AttestEmployee/PayrollImportTransactionDirective';
import { AdditionDeductionsDirectiveFactory } from '../../../Common/Directives/AdditionDeductions/AdditionDeductionsDirective';
import { AbsenceDetailsDirectiveFactory } from '../../Directives/AbsenceDetails/AbsenceDetailsDirective';
import { AccountingService as SharedAccountingService } from "../../../Shared/Economy/Accounting/AccountingService";
import { TimeRuleValidationDirectiveFactory } from '../TimeRules/TimeRuleValidationDirective';
import { ExpressionContainerDirectiveFactory } from '../../../Common/FormulaBuilder/ExpressionContainerDirective';
import { FormulaContainerDirectiveFactory } from '../../../Common/FormulaBuilder/FormulaContainerDirective';
import { WidgetDirectiveFactory } from '../../../Common/FormulaBuilder/WidgetDirective';
import { ScheduleInWidgetDirectiveFactory } from '../../../Common/FormulaBuilder/Widgets/ScheduleInWidgetDirective';
import { ScheduleOutWidgetDirectiveFactory } from '../../../Common/FormulaBuilder/Widgets/ScheduleOutWidgetDirective';
import { ClockWidgetDirectiveFactory } from '../../../Common/FormulaBuilder/Widgets/ClockWidgetDirective';
import { BalanceWidgetDirectiveFactory } from '../../../Common/FormulaBuilder/Widgets/BalanceWidgetDirective';
import { NotWidgetDirectiveFactory } from '../../../Common/FormulaBuilder/Widgets/NotWidgetDirective';
import { StartParanthesisWidgetDirectiveFactory } from '../../../Common/FormulaBuilder/Widgets/StartParanthesisWidgetDirective';
import { EndParanthesisWidgetDirectiveFactory } from '../../../Common/FormulaBuilder/Widgets/EndParanthesisWidgetDirective';
import { AndWidgetDirectiveFactory } from '../../../Common/FormulaBuilder/Widgets/AndWidgetDirective';
import { OrWidgetDirectiveFactory } from '../../../Common/FormulaBuilder/Widgets/OrWidgetDirective';
import { AttestEmployeeChartsDirectiveFactory } from '../../Directives/AttestEmployee/Directives/AttestEmployeeCharts/AttestEmployeeChartsDirective';
import { AttestGroupChartsDirectiveFactory } from '../../Directives/AttestGroup/Directives/AttestGroupCharts/AttestGroupChartsDirective';
import { ScheduleDirectiveFactory as EmployeeScheduleDirectiveFactory} from '../../Employee/Employees/Directives/Schedule/ScheduleDirective';
import { EditPlacementValidationDirectiveFactory } from '../../Employee/Employees/Directives/Schedule/EditPlacementValidationDirective';
import { ExtraFieldRecordsDirectiveFactory } from '../../../Common/Directives/ExtraFields/ExtraFieldsDirective';
import { TemplateDesignerDirectiveFactory } from '../../Employee/EmployeeTemplates/Directives/TemplateDesigner/TemplateDesignerDirective';
import { EmployeeTemplatesValidationDirectiveFactory } from '../../Employee/EmployeeTemplates/Directives/EmployeeTemplatesValidationDirective';
import { CreateFromTemplateValidationDirectiveFactory } from '../../Employee/Employees/Dialogs/CreateFromTemplate/CreateFromTemplateValidationDirective';
import { EmployeeAccountFactory } from '../../Employee/EmployeeTemplates/Directives/TemplateDesigner/Components/EmployeeAccount/EmployeeAccountController';
import { DisbursementAccountFactory } from '../../Employee/EmployeeTemplates/Directives/TemplateDesigner/Components/DisbursementAccount/DisbursementAccountController';
import { EmploymentPriceTypesFactory } from '../../Employee/EmployeeTemplates/Directives/TemplateDesigner/Components/EmploymentPriceTypes/EmploymentPriceTypesController';
import { PositionFactory } from '../../Employee/EmployeeTemplates/Directives/TemplateDesigner/Components/Position/PositionController';
import { EmployeeSettingsDirectiveFactory } from '../../Employee/Employees/Directives/EmployeeSettings/EmployeeSettingsDirective';
import { EmployeeSettingDialogValidationDirectiveFactory } from '../../Employee/Employees/Directives/EmployeeSettings/EmployeeSettingDialogValidationDirective';

angular.module("Soe.Time.Time.TimeAttest.Module", ['Soe.Time.Time', 'Soe.Common.GoogleMaps', 'Soe.Shared.Time.Schedule.Absencerequests.Module'])
    .service("scheduleService", ScheduleService)
    .service("sharedScheduleService", SharedScheduleService)
    .service("employeeService", EmployeeService)
    .service("sharedEmployeeService", SharedEmployeeService)
    .service("sharedAccountingService", SharedAccountingService)
    .service("payrollService", PayrollService)
    .directive("attestGroup", AttestGroupDirectiveFactory.create)
    .directive("attestGroupCharts", AttestGroupChartsDirectiveFactory.create)
    .directive("attestEmployee", AttestEmployeeDirectiveFactory.create)
    .directive("attestEmployeeCharts", AttestEmployeeChartsDirectiveFactory.create)
    .directive("rowDetail", RowDetailDirectiveFactory.create)
    .directive("daySchedule", ScheduleDirectiveFactory.create)
    .directive("timeStamp", TimeStampDirectiveFactory.create)
    .directive("timePayrollTransaction", TimePayrollTransactionDirectiveFactory.create)
    .directive("timeCodeTransaction", TimeCodeTransactionDirectiveFactory.create)
    .directive("payrollImportTransaction", PayrollImportTransactionDirectiveFactory.create)
    .directive("additionDeductions", AdditionDeductionsDirectiveFactory.create)
    .directive("absenceDetails", AbsenceDetailsDirectiveFactory.create)
    .directive("contactAddresses", ContactAddressesDirectiveFactory.create)
    .directive("employments", EmploymentsDirectiveFactory.create)
    .directive("employmentVacationGroups", EmploymentVacationGroupsDirectiveFactory.create)
    .directive("employmentPriceTypes", EmploymentPriceTypesDirectiveFactory.create)
    .directive("employeeAccounts", EmployeeAccountsDirectiveFactory.create)
    .directive("employmentPriceFormulas", EmploymentPriceFormulasDirectiveFactory.create)
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
    .directive("entityLogViewer", EntityLogViewerDirectiveFactory.create)
    .directive("timeRuleValidation", TimeRuleValidationDirectiveFactory.create)
    .directive("expressionContainer", ExpressionContainerDirectiveFactory.create)
    .directive("formulaContainer", FormulaContainerDirectiveFactory.create)
    .directive("widget", WidgetDirectiveFactory.create)
    .directive("scheduleInWidget", ScheduleInWidgetDirectiveFactory.create)
    .directive("scheduleOutWidget", ScheduleOutWidgetDirectiveFactory.create)
    .directive("clockWidget", ClockWidgetDirectiveFactory.create)
    .directive("balanceWidget", BalanceWidgetDirectiveFactory.create)
    .directive("notWidget", NotWidgetDirectiveFactory.create)
    .directive("startParanthesisWidget", StartParanthesisWidgetDirectiveFactory.create)
    .directive("endParanthesisWidget", EndParanthesisWidgetDirectiveFactory.create)
    .directive("andWidget", AndWidgetDirectiveFactory.create)
    .directive("orWidget", OrWidgetDirectiveFactory.create)
    .directive("extraFields", ExtraFieldRecordsDirectiveFactory.create)
    // EmployeeTemplate
    .directive("templateDesigner", TemplateDesignerDirectiveFactory.create)
    .directive("employeeTemplatesValidation", EmployeeTemplatesValidationDirectiveFactory.create)
    .directive("createFromTemplateValidation", CreateFromTemplateValidationDirectiveFactory.create)
    .directive("employeeAccountComponent", EmployeeAccountFactory.create)
    .directive("disbursementAccountComponent", DisbursementAccountFactory.create)
    .directive("employmentPriceTypesComponent", EmploymentPriceTypesFactory.create)
    .directive("positionComponent", PositionFactory.create);


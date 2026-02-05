import '../Module';
import { EmployeeTemplatesValidationDirectiveFactory } from './Directives/EmployeeTemplatesValidationDirective';
import { DisbursementAccountFactory } from './Directives/TemplateDesigner/Components/DisbursementAccount/DisbursementAccountController';
import { EmployeeAccountFactory } from './Directives/TemplateDesigner/Components/EmployeeAccount/EmployeeAccountController';
import { EmploymentPriceTypesFactory } from './Directives/TemplateDesigner/Components/EmploymentPriceTypes/EmploymentPriceTypesController';
import { PositionFactory } from './Directives/TemplateDesigner/Components/Position/PositionController';
import { TemplateDesignerDirectiveFactory } from './Directives/TemplateDesigner/TemplateDesignerDirective';

angular.module("Soe.Time.Employee.EmployeeTemplates.Module", ['Soe.Time.Employee'])
    .directive("templateDesigner", TemplateDesignerDirectiveFactory.create)
    .directive("employeeTemplatesValidation", EmployeeTemplatesValidationDirectiveFactory.create)
    .directive("employeeAccountComponent", EmployeeAccountFactory.create)
    .directive("disbursementAccountComponent", DisbursementAccountFactory.create)
    .directive("employmentPriceTypesComponent", EmploymentPriceTypesFactory.create)
    .directive("positionComponent", PositionFactory.create);


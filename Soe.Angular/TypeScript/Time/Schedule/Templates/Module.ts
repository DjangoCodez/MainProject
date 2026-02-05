import '../Module';

import { EmployeeService } from '../../Employee/EmployeeService';
import { TemplatesValidationDirectiveFactory } from './TemplatesValidationDirective';
import { TimeService } from '../../Time/TimeService';

angular.module("Soe.Time.Schedule.Templates.Module", ['Soe.Time.Schedule'])
    .service("employeeService", EmployeeService)
    .service("timeService", TimeService)
    .directive("templatesValidation", TemplatesValidationDirectiveFactory.create);
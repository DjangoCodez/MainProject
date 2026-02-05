import '../Module';

import { EmployeeService as SharedEmployeeService } from "../../../Shared/Time/Employee/EmployeeService";
import { AccountingService } from '../../../Shared/Economy/Accounting/AccountingService';
import { AccountDimsDirectiveFactory } from '../../../Common/Directives/accountdims/accountdimsdirective';
import { MassRegistrationValidationDirectiveFactory } from './Directives/MassRegistrationValidationDirective';

angular.module("Soe.Time.Payroll.MassRegistration.Module", ['Soe.Time.Payroll'])
    .service("sharedEmployeeService", SharedEmployeeService)
    .service("accountingService", AccountingService)
    .directive("accountDims", AccountDimsDirectiveFactory.create)
    .directive("massRegistrationValidation", MassRegistrationValidationDirectiveFactory.create);

import '../../../Core/Module';

import { EmployeeService as SharedEmployeeService } from '../../../Shared/Time/Employee/EmployeeService';

angular.module("Soe.Common.Dialogs.SelectEmployees.Module", ['Soe.Core'])
    .service("sharedEmployeeService", SharedEmployeeService);


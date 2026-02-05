import '../../Module';

import { EmployeeService } from '../EmployeeService';

angular.module("Soe.Time.Employee.Accumulators.Module", ['Soe.Time'])
    .service("employeeService", EmployeeService);     

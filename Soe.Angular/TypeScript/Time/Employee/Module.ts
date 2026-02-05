import '../Module';

import { EmployeeService } from "./EmployeeService";
import { PayrollService } from '../Payroll/PayrollService';
import { TimeService } from '../Time/timeservice';

angular.module("Soe.Time.Employee", ['Soe.Time'])
    .service("employeeService", EmployeeService)
    .service("payrollService", PayrollService)
    .service("timeService", TimeService);


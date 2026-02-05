import '../Module';
import { EmployeeService } from './../EmployeeService';

angular.module("Soe.Time.Employee.MassUpdateEmployeeFields.Module", ['Soe.Time.Employee'])
    .service("employeeService", EmployeeService)

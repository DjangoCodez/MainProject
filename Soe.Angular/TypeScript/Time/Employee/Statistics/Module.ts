import '../Module';
import { EmployeeService as SharedEmployeeService } from "../../../Shared/Time/Employee/EmployeeService";
import 'd3';
import 'nvd3';
import 'angular-nvd3';

angular.module("Soe.Time.Employee.Statistics.Module", ['Soe.Time.Employee', 'nvd3'])
    .service("sharedEmployeeService", SharedEmployeeService);

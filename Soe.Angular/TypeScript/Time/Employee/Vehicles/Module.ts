import '../Module';

import { EmployeeService } from "../EmployeeService";
import { EmployeeVehicleValidationDirectiveFactory } from "./EmployeeVehicleValidationDirective";

angular.module("Soe.Time.Employee.Vehicles.Module", ['Soe.Time.Employee'])
    .directive("employeeVehicleValidation", EmployeeVehicleValidationDirectiveFactory.create);

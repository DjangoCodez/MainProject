import '../Module';

import { PayrollService } from "./PayrollService";

angular.module("Soe.Time.Payroll", ['Soe.Time'])
    .service("payrollService", PayrollService);


import '../Module';

import { PayrollService } from '../Payroll/PayrollService';

angular.module("Soe.Time.Import", ['Soe.Time'])
    .service("payrollService", PayrollService);


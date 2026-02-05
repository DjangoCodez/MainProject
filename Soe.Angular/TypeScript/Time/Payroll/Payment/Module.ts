import '../../Module';
import '../Module';

import { PayrollEmployeesSelection } from './Directives/PayrollEmployeesSelection/PayrollEmployeesSelectionComponent';
import { TimeService } from '../../Time/TimeService';
import { PayrollService } from '../PayrollService';

angular.module("Soe.Time.Payroll.Payment.Module", ['Soe.Time', 'Soe.Time.Payroll'])
    .component(PayrollEmployeesSelection.componentKey, PayrollEmployeesSelection.component())
    .service("timeService", TimeService)
    .service("payrollService", PayrollService)

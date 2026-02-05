import '../Module';

import { EmployeeGroupsDirectiveFactory } from '../../../Common/Directives/EmployeeGroups/EmployeeGroupsDirective';
import { TimeCodeTimeDeviationCausesDirectiveFactory } from '../../Directives/TimeCodeTimeDeviationCauses/TimeCodeTimeDeviationCausesDirective';

angular.module("Soe.Time.Time.TimeCodeBreaks.Module", ['Soe.Time.Time'])
    .directive("employeeGroups", EmployeeGroupsDirectiveFactory.create)
    .directive("timeCodeTimeDeviationCause", TimeCodeTimeDeviationCausesDirectiveFactory.create);

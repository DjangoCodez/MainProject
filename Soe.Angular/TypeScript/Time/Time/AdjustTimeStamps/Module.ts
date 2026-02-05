import '../Module';

import { EmployeeService as SharedEmployeeService } from "../../../Shared/Time/Employee/EmployeeService";
import { EntityLogViewerDirectiveFactory } from '../../../Common/Directives/EntityLogViewer/EntityLogViewerDirective';

angular.module("Soe.Time.Time.AdjustTimeStamps.Module", ['Soe.Time.Time'])
    .service("sharedEmployeeService", SharedEmployeeService)
    .directive("entityLogViewer", EntityLogViewerDirectiveFactory.create);


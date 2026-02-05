import '../../Module';

import { RegistryService } from "../RegistryService";
import { ReceiversListDirectiveFactory } from "../../../Common/Directives/ReceiversList/ReceiversListDirective";
import { AvailableEmployeesDirectiveFactory } from '../../../Common/Directives/AvailableEmployees/AvailableEmployeesDirective';
import { ScheduleService as SharedScheduleService } from '../../../Shared/Time/Schedule/ScheduleService';

angular.module("Soe.Manage.Registry.ReceiverGroups.Module", ['Soe.Manage'])
    .service("registryService", RegistryService)
    .service("sharedScheduleService", SharedScheduleService)
    .directive("receiversList", ReceiversListDirectiveFactory.create)
    .directive("availableEmployees", AvailableEmployeesDirectiveFactory.create);

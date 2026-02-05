import '../../Module';

import { RegistryService } from "../RegistryService";
import { LogsDirectiveFactory } from './Directives/Logs/LogsDirective';
import { SettingsDirectiveFactory } from './Directives/Settings/SettingsDirective';

angular.module("Soe.Manage.Registry.ScheduledJobs.Module", ['Soe.Manage'])
    .service("registryService", RegistryService)
    .directive("logs", LogsDirectiveFactory.create)
    .directive("jobSettings", SettingsDirectiveFactory.create);

import '../../Module';

import { RegistryService } from "../RegistryService";
import { ChecklistRowsDirectiveFactory } from "./Directives/ChecklistRowsDirective";

angular.module("Soe.Manage.Registry.Checklists.Module", ['Soe.Manage'])
    .service("registryService", RegistryService)
    .directive("checklistRows", ChecklistRowsDirectiveFactory.create);

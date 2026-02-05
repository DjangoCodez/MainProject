import '../../../Core/Module';

import { SelectProjectService } from "./SelectProjectService";

angular.module("Soe.Common.Dialogs.SelectProject.Module", ['Soe.Core'])
    .service("selectProjectService", SelectProjectService);

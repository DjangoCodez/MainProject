import '../../Core/Module';

import { StartService } from "./StartService";

angular.module("Soe.Common.Start.Module", ['Soe.Core'])
    .service("startService", StartService);

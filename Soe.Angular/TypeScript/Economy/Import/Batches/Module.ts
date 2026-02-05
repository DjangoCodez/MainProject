import '../Module';

import { ImportService } from "../ImportService";
import { ConnectService } from "../../../Common/Connect/ConnectService";
import { ImportRowsDirectiveFactory } from "../../../Common/Connect/Directives/ImportRowsDirective";

angular.module("Soe.Economy.Import.Batches.Module", ['Soe.Economy.Import'])
    .service("importService", ImportService)
    .service("connectService", ConnectService)
    .directive("importRows", ImportRowsDirectiveFactory.create);
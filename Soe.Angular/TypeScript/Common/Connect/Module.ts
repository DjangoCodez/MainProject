import '../../Core/Module';

import { ImportRowsDirectiveFactory } from "./Directives/ImportRowsDirective";
import { ConnectService } from './ConnectService';
import { AccountingService } from '../../Shared/Economy/Accounting/AccountingService';
import { AccountDimsDirectiveFactory } from "../../Common/Directives/accountdims/accountdimsdirective";

angular.module("Soe.Common.Connect.Module", ['Soe.Core'])
    .service("connectService", ConnectService)
    .service("accountingService", AccountingService)
    .directive("importRows", ImportRowsDirectiveFactory.create)
    .directive("accountDims", AccountDimsDirectiveFactory.create);


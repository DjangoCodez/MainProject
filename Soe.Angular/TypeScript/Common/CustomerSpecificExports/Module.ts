import '../../Core/Module'
import { AccountingService } from '../../Shared/Economy/Accounting/AccountingService';

angular.module("Soe.Common.CustomerSpecificExports.Module", ['Soe.Core'])
    .service("accountingService", AccountingService)

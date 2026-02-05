import '../../../Core/Module';

import { SelectSupplierService } from "./SelectSupplierService";

angular.module("Soe.Common.Dialogs.SelectSupplier.Module", ['Soe.Core'])
    .service("selectSupplierService", SelectSupplierService);

import '../Module';
import { ImportService } from "../../../Shared/Billing/Import/ImportService";
import { SupplierService } from "../../../Shared/Economy/Supplier/SupplierService";

angular.module("Soe.Economy.Import.Finvoice.Module", ['Soe.Economy.Import'])
    .service("importService", ImportService)
    .service("supplierService", SupplierService);

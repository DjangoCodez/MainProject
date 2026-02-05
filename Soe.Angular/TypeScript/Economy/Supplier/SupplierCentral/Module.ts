import '../Module';
import '../Invoices/Module';

import { SelectSupplierService } from "../../../Common/Dialogs/SelectSupplier/selectsupplierservice";

angular.module("Soe.Economy.Supplier.SupplierCentral.Module", ['Soe.Economy.Supplier', 'Soe.Economy.Supplier.Invoices.Module'])
    .service("selectSupplierService", SelectSupplierService);
    
import '../Module';
import '../Suppliers/Module';

import { AddInvoiceToAttestFlowService } from "../../../Common/Dialogs/addinvoicetoattestflow/addinvoicetoattestflowservice";
import { expandableGridDirective } from "../../../Shared/Economy/Supplier/Invoices/Directives/ExpandableGridDirective";
import { SupplierPaymentValidationDirectiveFactory } from "./SupplierPaymentValidationDirective";
import { UserSelectorForTemplateHeadRowDirectiveFactory } from "../../../Common/Dialogs/addinvoicetoattestflow/Directives/userselectorfortemplateheadrowdirective";
import { DistributionRowsDirectiveFactory } from "../../../Common/Directives/DistributionRows/distributionrowsdirective";

angular.module("Soe.Economy.Supplier.Payments.Module", ['Soe.Economy.Supplier', 'Soe.Economy.Supplier.Suppliers.Module'])
    .service("addInvoiceToAttestFlowService", AddInvoiceToAttestFlowService)
    .directive("expandableGrid", expandableGridDirective)
    .directive("supplierPaymentValidation", SupplierPaymentValidationDirectiveFactory.create)
    .directive("userSelectorForTemplateHeadRow", UserSelectorForTemplateHeadRowDirectiveFactory.create)
    //.directive("distributionRows", DistributionRowsDirectiveFactory.create);

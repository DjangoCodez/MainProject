import '../Module';
import '../../../Common/GoogleMaps/Module';

import { ActorPaymentDirective } from "./Directives/ActorPaymentDirective";
import { SupplierValidationDirectiveFactory } from '../../../Shared/Economy/Supplier/Suppliers/Directives/SupplierValidationDirective';
import { TrackChangesViewDirective } from '../../../Common/Directives/TrackChangesView/TrackChangesView';

angular.module("Soe.Economy.Supplier.Suppliers.Module", ['Soe.Economy.Supplier'])
    .directive("actorPayment", ActorPaymentDirective)
    .directive("trackChangesView", TrackChangesViewDirective)
    .directive("supplierValidation", SupplierValidationDirectiveFactory.create);
    


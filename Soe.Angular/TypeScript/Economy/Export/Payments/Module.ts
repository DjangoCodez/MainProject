import '../Module';
import '../../Supplier/Payments/Module';
import '../../../Shared/Economy/Module';

angular.module("Soe.Economy.Export.Payments.Module", ['Soe.Shared.Economy', 'Soe.Economy.Export', 'Soe.Economy.Supplier.Payments.Module']);

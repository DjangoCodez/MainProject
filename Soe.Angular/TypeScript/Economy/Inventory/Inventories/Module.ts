import '../Module';
import '../../../Shared/Economy/Module';
import '../../Supplier/Invoices/Module';

import { CommonCustomerService } from '../../../Common/Customer/CommonCustomerService';

angular.module("Soe.Economy.Inventory.Inventories.Module", ['Soe.Economy.Inventory', 'Soe.Shared.Economy', 'Soe.Economy.Supplier.Invoices.Module'])
    .service("commonCustomerService", CommonCustomerService);
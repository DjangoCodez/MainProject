import '../Module';
import '../../Shared/Billing/Module';

import { ProductService } from "../../Shared/Billing/Products/ProductService";

angular.module("Soe.Billing.Products", [/*'Soe.Core',*/ 'Soe.Billing', 'Soe.Shared.Billing'])
    .service("productService", ProductService);


import '../../../Module';
import '../../../../Shared/Billing/Module';
import { ITranslationService } from '../../../../Core/Services/TranslationService';
import { TermPartsLoaderProvider } from '../../../../Core/Services/termpartsloader';
import { ProductService } from '../../../../Shared/Billing/Products/ProductService';
import { SupplierService } from '../../../../Shared/Economy/Supplier/SupplierService';
import { SupplierProductService } from '../../../../Shared/Billing/Purchase/Purchase/SupplierProductService';
import { SupplierPicelistPricesDirectiveFactory } from '../../Directives/SupplierPricelistPrices/SupplierPricelistPrices';


angular.module("Soe.Billing.Purchase.Products.Pricelists.Module", ['Soe.Billing', 'Soe.Shared.Billing'])
    .service("productService", ProductService)
    .service("supplierService", SupplierService)
    .service("supplierProductService", SupplierProductService)
    .directive("supplierPricelistPrices", SupplierPicelistPricesDirectiveFactory.create)
    .config( /*@ngInject*/(termPartsLoaderProvider: TermPartsLoaderProvider) => {
        termPartsLoaderProvider.addPart('economy');
    })
    .run(/*@ngInject*/(translationService: ITranslationService) => {
        // Need to refresh here to load the added part
        translationService.refresh();
    });
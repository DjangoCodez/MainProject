import '../Module';
import '../../Common/Customer/Customers/Module';
import '../../Shared/Billing/Module';

import { CommonCustomerService } from "../../Common/Customer/CommonCustomerService";
import { AddInvoiceToAttestFlowService } from "../../Common/Dialogs/AddInvoiceToAttestFlow/addinvoicetoattestflowservice";
import { SelectProjectService } from "../../Common/Dialogs/SelectProject/SelectProjectService";
import { TermPartsLoaderProvider } from "../../Core/Services/TermPartsLoader";
import { ITranslationService } from "../../Core/Services/TranslationService";
import { OrderValidationDirectiveFactory } from "../../Shared/Billing/Orders/OrderValidationDirective";
import { AccountDimsDirectiveFactory } from "../../Common/Directives/accountdims/accountdimsdirective";
import { CategoriesDirectiveFactory } from "../../Common/Directives/Categories/CategoriesDirective";
import { DocumentsDirectiveControllerFactory } from '../../Common/Directives/Documents/DocumentsDirective';

angular.module("Soe.Billing.Orders.Module", ['Soe.Billing', 'Soe.Shared.Billing', 'Soe.Common.Customer.Customers.Module'])
    .service("commonCustomerService", CommonCustomerService)
    .service("selectProjectService", SelectProjectService)
    .directive("orderValidation", OrderValidationDirectiveFactory.create)
    .directive("accountDims", AccountDimsDirectiveFactory.create)
    .directive("categories", CategoriesDirectiveFactory.create)
    .directive("soeDocuments", DocumentsDirectiveControllerFactory.create)
    .config(/*@ngInject*/(termPartsLoaderProvider: TermPartsLoaderProvider) => {
        termPartsLoaderProvider.addPart('billing');
    })
    .run(/*@ngInject*/(translationService: ITranslationService) => {
        // Need to refresh here to load the added part
        translationService.refresh();
    });
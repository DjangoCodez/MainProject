import '../Module';
import '../../Shared/Billing/Module';

import { CommonCustomerService } from "../../Common/Customer/CommonCustomerService";
import { AddInvoiceToAttestFlowService } from "../../Common/Dialogs/AddInvoiceToAttestFlow/addinvoicetoattestflowservice";
import { SelectProjectService } from "../../Common/Dialogs/SelectProject/SelectProjectService";
import { OrderService } from "../../Shared/Billing/Orders/OrderService";
import { ProductService } from "../../Shared/Billing/Products/ProductService";
import { InvoiceService } from "../../Shared/Billing/Invoices/InvoiceService";
import { UserSelectorForTemplateHeadRowDirectiveFactory } from "../../Common/Dialogs/AddInvoiceToAttestFlow/Directives/userselectorfortemplateheadrowdirective";
import { TermPartsLoaderProvider } from "../../Core/Services/TermPartsLoader";
import { ITranslationService } from "../../Core/Services/TranslationService";
import { AccountingService } from '../../Shared/Economy/Accounting/AccountingService';
import { plannedShiftsDirective } from '../../Shared/Billing/Orders/Directives/PlannedShiftsDirective';
import { AngularFeatureCheckService } from '../../Core/Services/AngularFeatureCheckService';

angular.module("Soe.Billing.Projects.Module", ['Soe.Billing', 'Soe.Shared.Billing'])
    .service("invoiceService", InvoiceService)
    .service("commonCustomerService", CommonCustomerService)
    .service("selectProjectService", SelectProjectService)
    .service("addInvoiceToAttestFlowService", AddInvoiceToAttestFlowService)
    .service("orderService", OrderService)
    .service("productService", ProductService)
    .service("accountingService", AccountingService)
    .service("angularFeatureCheckService", AngularFeatureCheckService)
    .directive("plannedShifts", plannedShiftsDirective)
    .directive("userSelectorForTemplateHeadRow", UserSelectorForTemplateHeadRowDirectiveFactory.create)
    .config( /*@ngInject*/(termPartsLoaderProvider: TermPartsLoaderProvider) => {
        termPartsLoaderProvider.addPart('economy');
    })
    .run(/*@ngInject*/(translationService: ITranslationService) => {
        // Need to refresh here to load the added part
        translationService.refresh();
    });

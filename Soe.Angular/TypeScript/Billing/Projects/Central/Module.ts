import '../../Module';
import '../../../Shared/Economy/Module';

import { CoreService } from "../../../Core/Services/CoreService";
import { CommonCustomerService } from "../../../Common/Customer/CommonCustomerService";
import { InvoiceService } from "../../../Shared/Billing/Invoices/InvoiceService";
import { SelectProjectService } from "../../../Common/Dialogs/SelectProject/SelectProjectService";
import { AddInvoiceToAttestFlowService } from "../../../Common/Dialogs/AddInvoiceToAttestFlow/addinvoicetoattestflowservice";
import { OrderService } from "../../../Shared/Billing/Orders/OrderService";
import { ProductService } from "../../../Shared/Billing/Products/ProductService";
import { UserSelectorForTemplateHeadRowDirectiveFactory } from "../../../Common/Dialogs/AddInvoiceToAttestFlow/Directives/userselectorfortemplateheadrowdirective";
import { TermPartsLoaderProvider } from "../../../Core/Services/TermPartsLoader";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { ProjectService } from "../../../Shared/Billing/Projects/ProjectService";
import { plannedShiftsDirective } from '../../../Shared/Billing/Orders/Directives/PlannedShiftsDirective';

angular.module("Soe.Billing.Projects.Central.Module", ['Soe.Billing', 'Soe.Shared.Economy'])
    .service("coreService", CoreService)
    .service("invoiceService", InvoiceService)
    .service("commonCustomerService", CommonCustomerService)
    .service("selectProjectService", SelectProjectService)
    .service("addInvoiceToAttestFlowService", AddInvoiceToAttestFlowService)
    .service("orderService", OrderService)
    .service("productService", ProductService)
    .service("projectService", ProjectService)
    //.directive("plannedShifts", plannedShiftsDirective)
    //.directive("invoiceValidation", InvoiceValidationDirectiveFactory.create)
    //.directive("traceRows", traceRowsDirective)
    .directive("userSelectorForTemplateHeadRow", UserSelectorForTemplateHeadRowDirectiveFactory.create)
    .config( /*@ngInject*/(termPartsLoaderProvider: TermPartsLoaderProvider) => {
        termPartsLoaderProvider.addPart('economy');
    })
    .run(/*@ngInject*/(translationService: ITranslationService) => {
        // Need to refresh here to load the added part
        translationService.refresh();
    });

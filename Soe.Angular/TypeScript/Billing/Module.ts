import '../Core/Module';

//import { ProjectProductsDirectiveFactory } from "./Projects/Directives/ProjectProductsDirective";
//import { ProjectTimeCodesFactory } from "./Projects/Directives/ProjectTimeCodesDirective";
import { PayrollProductAccountingPriorityDirectiveFactory } from "./Projects/Directives/PayrollProductAccountingPriorityDirective";
import { InvoiceProductAccountingPriorityDirectiveFactory } from "./Projects/Directives/InvoiceProductAccountingPriorityDirective";
//import { ProjectValidationDirectiveFactory } from "./Projects/Directives/ProjectValidationDirective";
import { TermPartsLoaderProvider } from "../Core/Services/TermPartsLoader";
import { ITranslationService } from "../Core/Services/TranslationService";
import { ContractService } from "./Contracts/ContractService";

angular.module("Soe.Billing", ['Soe.Core'])
    //.directive("projectProducts", ProjectProductsDirectiveFactory.create)
    //.directive("projectTimeCodes", ProjectTimeCodesFactory.create)
    .directive("payrollProductAccountingPriority", PayrollProductAccountingPriorityDirectiveFactory.create)
    .directive("invoiceProductAccountingPriority", InvoiceProductAccountingPriorityDirectiveFactory.create)
    //.directive("projectValidation", ProjectValidationDirectiveFactory.create)
    .service("contractService", ContractService)
    .config(/*@ngInject*/(termPartsLoaderProvider: TermPartsLoaderProvider) => {
        termPartsLoaderProvider.addPart('billing');
    })
    .run(/*@ngInject*/(translationService: ITranslationService) => {
        // Need to refresh here to load the added part
        translationService.refresh();
    });

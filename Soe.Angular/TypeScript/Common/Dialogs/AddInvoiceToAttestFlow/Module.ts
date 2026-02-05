import '../../../Core/Module';

import { AddInvoiceToAttestFlowService } from "./AddInvoiceToAttestFlowService";
import { UserSelectorForTemplateHeadRowDirectiveFactory } from "./Directives/UserSelectorForTemplateHeadRowDirective";

angular.module("Soe.Common.Dialogs.AddInvoiceToAttestFlow.Module", ['Soe.Core'])
    .service("AddInvoiceToAttestFlowService", AddInvoiceToAttestFlowService)
    .directive("userSelectorForTemplateHeadRow", UserSelectorForTemplateHeadRowDirectiveFactory.create);

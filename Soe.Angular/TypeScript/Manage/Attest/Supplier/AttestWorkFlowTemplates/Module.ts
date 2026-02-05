import '../../../Module';

import { AttestService } from "../../AttestService";
import { AttestWorkFlowTemplateRowDirectiveFactory } from "./Directives/AttestWorkFlowTemplateRowDirective"

angular.module("Soe.Manage.Attest.Supplier.AttestWorkFlowTemplates.Module", ['Soe.Manage'])
    .service("attestService", AttestService)
    .directive("attestWorkFlowTemplateRow", AttestWorkFlowTemplateRowDirectiveFactory.create)

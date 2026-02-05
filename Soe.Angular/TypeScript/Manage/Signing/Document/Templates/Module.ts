import '../../../Module';

import { SigningService } from "../../SigningService";
import { AttestWorkFlowTemplateRowsDirectiveFactory } from '../../../../Common/Directives/AttestWorkFlowTemplateRows/AttestWorkFlowTemplateRowsDirective';
import { AttestWorkFlowTemplateValidationDirectiveFactory } from './AttestWorkFlowTemplateValidationDirective';

angular.module("Soe.Manage.Signing.Document.Templates.Module", ['Soe.Manage'])
    .service("signingService", SigningService)
    .directive("attestWorkFlowTemplateRows", AttestWorkFlowTemplateRowsDirectiveFactory.create)
    .directive("attestWorkFlowTemplateValidation", AttestWorkFlowTemplateValidationDirectiveFactory.create);

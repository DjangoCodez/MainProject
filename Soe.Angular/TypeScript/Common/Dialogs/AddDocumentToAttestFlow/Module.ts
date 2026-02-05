import '../../../Core/Module';

import { AddDocumentToAttestFlowService } from "./AddDocumentToAttestFlowService";
import { AddDocumentToAttestFlowUserDirectiveFactory } from './Directives/AddDocumentToAttestFlowUserDirective';

angular.module("Soe.Common.Dialogs.AddDocumentToAttestFlow", ['Soe.Core'])
    .service("addDocumentToAttestFlowService", AddDocumentToAttestFlowService)
    .directive("addDocumentToAttestFlowUser", AddDocumentToAttestFlowUserDirectiveFactory.create);

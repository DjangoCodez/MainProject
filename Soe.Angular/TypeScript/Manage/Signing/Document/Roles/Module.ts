import '../../../Module';

import { SigningService } from "../../SigningService";
import { EntityLogViewerDirectiveFactory } from '../../../../Common/Directives/EntityLogViewer/EntityLogViewerDirective';
import { AttestTransitionsDirectiveFactory } from '../../../../Common/Directives/AttestTransitions/AttestTransitionsDirective';

angular.module("Soe.Manage.Signing.Document.Roles.Module", ['Soe.Manage'])
    .service("signingService", SigningService)
    .directive("entityLogViewer", EntityLogViewerDirectiveFactory.create)    
    .directive("attestTransitions", AttestTransitionsDirectiveFactory.create);


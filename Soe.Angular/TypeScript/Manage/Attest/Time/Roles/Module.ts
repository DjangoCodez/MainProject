import '../../../Module';

import { AttestService } from "../../AttestService";
import { EntityLogViewerDirectiveFactory } from '../../../../Common/Directives/EntityLogViewer/EntityLogViewerDirective';
import { AttestTransitionsDirectiveFactory } from '../../../../Common/Directives/AttestTransitions/AttestTransitionsDirective';
import { CategoriesDirectiveFactory } from '../../../../Common/Directives/Categories/CategoriesDirective';
import { AttestRoleMappingDirectiveFactory } from '../../../../Common/Directives/AttestRoleMapping/AttestRoleMappingDirective';

angular.module("Soe.Manage.Attest.Time.Roles.Module", ['Soe.Manage'])
    .service("attestService", AttestService)
    .directive("entityLogViewer", EntityLogViewerDirectiveFactory.create)
    .directive("categories", CategoriesDirectiveFactory.create)
    .directive("attestRoleMapping", AttestRoleMappingDirectiveFactory.create)
    .directive("attestTransitions", AttestTransitionsDirectiveFactory.create);

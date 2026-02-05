import '../../Module';

import { RoleService } from '../RoleService';
import { EntityLogViewerDirectiveFactory } from '../../../Common/Directives/EntityLogViewer/EntityLogViewerDirective';

angular.module("Soe.Manage.Role.Roles.Module", ['Soe.Manage'])
    .service("roleService", RoleService)
    .directive("entityLogViewer", EntityLogViewerDirectiveFactory.create);


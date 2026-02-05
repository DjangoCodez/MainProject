import '../../Module';
import '../../../Common/GoogleMaps/Module';

import { ContactAddressesDirectiveFactory } from '../../../Common/Directives/ContactAddresses/ContactAddressesDirective';
import { UserValidationDirectiveFactory } from './Directives/UserValidationDirective';
import { UserService } from '../UserService';
import { UserDirectiveFactory } from '../../../Common/Directives/User/UserDirective';
import { UserRolesDirectiveFactory } from '../../../Common/Directives/UserRoles/UserRolesDirective';
import { DelegateHistoryDirectiveFactory } from './Directives/DelegateHistory/DelegateHistoryDirective';
import { EntityLogViewerDirectiveFactory } from '../../../Common/Directives/EntityLogViewer/EntityLogViewerDirective';

angular.module("Soe.Manage.User.Users.Module", ['Soe.Manage', 'Soe.Common.GoogleMaps'])
    .service("userService", UserService)
    .directive("contactAddresses", ContactAddressesDirectiveFactory.create)
    .directive("user", UserDirectiveFactory.create)
    .directive("userRoles", UserRolesDirectiveFactory.create)
    .directive("userValidation", UserValidationDirectiveFactory.create)
    .directive("delegateHistory", DelegateHistoryDirectiveFactory.create)
    .directive("entityLogViewer", EntityLogViewerDirectiveFactory.create);


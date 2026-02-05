import '../Module';

import { SystemService } from "./SystemService";
import { UserService } from '../User/UserService';

angular.module("Soe.Manage.System", ['Soe.Manage'])
    .service("systemService", SystemService)
    .service("userService", UserService);

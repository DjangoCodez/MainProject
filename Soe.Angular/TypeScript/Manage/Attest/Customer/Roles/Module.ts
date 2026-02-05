import '../../../Module';

import { IUrlHelperService, IUrlHelperServiceProvider } from "../../../../Core/Services/UrlHelperService";
import { AttestService } from "../../AttestService";

angular.module("Soe.Manage.Attest.Customer.Roles.Module", ['Soe.Manage'])
    .service("attestService", AttestService);

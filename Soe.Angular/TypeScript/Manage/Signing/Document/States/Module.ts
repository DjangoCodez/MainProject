import '../../../Module';

import { SigningService } from "../../SigningService";

angular.module("Soe.Manage.Signing.Document.States.Module", ['Soe.Manage'])
    .service("signingService", SigningService);

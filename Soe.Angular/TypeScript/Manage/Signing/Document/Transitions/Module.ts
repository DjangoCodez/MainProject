import '../../../Module';

import { SigningService } from "../../SigningService";

angular.module("Soe.Manage.Signing.Document.Transitions.Module", ['Soe.Manage'])
    .service("signingService", SigningService);

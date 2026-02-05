import '../../../Module';

import { GDPRService } from "../../GDPRService";

angular.module("Soe.Manage.Gdpr.Registry.HandleInfo.Module", ['Soe.Manage'])
    .service("gdprService", GDPRService);

import '../../Module';

import { RegistryService } from "../RegistryService";
import { AttestService } from "../../Attest/AttestService";

angular.module("Soe.Manage.Registry.ExternalCodes.Module", ['Soe.Manage'])
    .service("registryService", RegistryService)
    .service("attestService", AttestService);

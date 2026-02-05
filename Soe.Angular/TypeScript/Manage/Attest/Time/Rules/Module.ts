import '../../../Module';

import { AttestService } from "../../AttestService";
import { EmployeeGroupsDirectiveFactory } from '../../../../Common/Directives/EmployeeGroups/EmployeeGroupsDirective';
import { AttestRuleRowsDirectiveFactory } from './Directives/AttestRuleRows/AttestRuleRowsDirective';
import { RegistryService } from '../../../Registry/RegistryService';

angular.module("Soe.Manage.Attest.Time.Rules.Module", ['Soe.Manage'])
    .service("attestService", AttestService)
    .service("registryService", RegistryService)
    .directive("employeeGroups", EmployeeGroupsDirectiveFactory.create)
    .directive("attestRuleRows", AttestRuleRowsDirectiveFactory.create);

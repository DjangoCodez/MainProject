import '../Module';

import { UserSelectorForTemplateHeadRowDirectiveFactory } from "./Directives/UserSelectorForTemplateHeadRowDirective";

angular.module("Soe.Economy.Supplier.AttestGroups.Module", ['Soe.Economy.Supplier'])
    .directive("userSelectorForTemplateHeadRow", UserSelectorForTemplateHeadRowDirectiveFactory.create);

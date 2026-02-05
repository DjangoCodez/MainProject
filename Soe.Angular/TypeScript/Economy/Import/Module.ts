import '../Module';

import { ImportService } from "./ImportService";
import { UserSelectorForTemplateHeadRowDirectiveFactory } from '../../Common/Dialogs/addinvoicetoattestflow/Directives/UserSelectorForTemplateHeadRowDirective';

angular.module("Soe.Economy.Import", ['Soe.Economy', 'Soe.Shared.Economy'])
    .service("importService", ImportService)
    .directive("userSelectorForTemplateHeadRow", UserSelectorForTemplateHeadRowDirectiveFactory.create);   


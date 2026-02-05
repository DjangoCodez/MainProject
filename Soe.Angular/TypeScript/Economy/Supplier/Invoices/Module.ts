import '../Module';
import '../Suppliers/Module';
import '../../../Shared/Economy/Module';

import { IUrlHelperService, IUrlHelperServiceProvider } from "../../../Core/Services/UrlHelperService";
import { UserSelectorForTemplateHeadRowDirectiveFactory } from "../../../Common/Dialogs/addinvoicetoattestflow/Directives/userselectorfortemplateheadrowdirective";
//import { traceRowsDirective } from "../../../Common/Directives/TraceRows/TraceRows";

angular.module("Soe.Economy.Supplier.Invoices.Module", ['Soe.Economy.Supplier', 'Soe.Economy.Supplier.Suppliers.Module'])
    //.directive("traceRows", traceRowsDirective)
    .directive("userSelectorForTemplateHeadRow", UserSelectorForTemplateHeadRowDirectiveFactory.create)

import { IUrlHelperService } from "../../../../Core/Services/UrlHelperService";
import { IOrderShiftDTO } from "../../../../Scripts/TypeLite.Net4";

//@ngInject
export function plannedShiftsDirective(urlHelperService: IUrlHelperService): ng.IDirective {
    return {
        templateUrl: urlHelperService.getGlobalUrl("/Billing/Orders/Views/plannedShifts.html"),
        replace: true,
        restrict: "E",
        controller: PlannedShiftsController,
        controllerAs: "ctrl",
        bindToController: true,
        scope: {
            shifts: '='
        },
    }
}

export class PlannedShiftsController {
    private shifts: IOrderShiftDTO[];
}

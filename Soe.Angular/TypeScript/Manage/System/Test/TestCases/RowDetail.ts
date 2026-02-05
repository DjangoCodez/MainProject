import { IUrlHelperService } from "../../../../Core/Services/UrlHelperService";


export class RowDetailDirectiveFactory {
    //@ngInject
    public static create(urlHelperService: IUrlHelperService): ng.IDirective {
        return {
            templateUrl: urlHelperService.getGlobalUrl('Manage/System/Test/TestCases/Views/RowDetail.html'),
            scope: {
            },
            restrict: 'E',
            replace: true,
            controller: RowDetailController,
            controllerAs: 'ctrl',
            bindToController: true,
        };
    }
}

export class RowDetailController {

    // Init parameters

    //@ngInject
    constructor(private $scope: ng.IScope) {
        console.log("injdected")
    }

    // EVENTS

    public $onInit() {
        console.log('heh')
    }
}


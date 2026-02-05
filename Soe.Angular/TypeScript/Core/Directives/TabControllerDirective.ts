export class TabControllerDirectiveFactory {
    //@ngInject
    public static create(): ng.IDirective {
        return {
            template:
                '<div ng-controller="tab.controller as ctrl" ng-init="ctrl.onInit(tab.parameters)" data-ng-include="tab.templateUrl"></div>',
            scope: { tab: "=" }
        }
    }
}
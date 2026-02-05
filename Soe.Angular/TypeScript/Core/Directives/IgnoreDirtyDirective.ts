export class IgnoreDirtyDirectiveFactory {
    //@ngInject
    public static create(): ng.IDirective {
        return {
            require: 'ngModel',
            link: function (scope, element, attrs, ngModelCtrl: ng.IFormController) {
                // override the $setDirty method on ngModelController
                ngModelCtrl.$setDirty = angular.noop;
            }
        };
    }
}

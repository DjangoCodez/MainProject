export class AccountDimValidationDirectiveFactory {
    //@ngInject
    public static create(): ng.IDirective {
        return {
            restrict: 'A',
            require: '?ngModel',
            scope: {
                ngModel: '=',
                linkedToProject: '=',
                linkedToShiftType: '='
            },
            link(scope: ng.IScope, elem: JQuery, attributes: ng.IAttributes, ngModelController: ng.INgModelController) {
                scope.$watchGroup(['linkedToProject', 'linkedToShiftType'], (newValues, oldValues, scope) => {
                    ngModelController.$setValidity("linkedToMultiple", !(scope['linkedToProject'] && scope['linkedToShiftType']));
                });
            }
        }
    }
}



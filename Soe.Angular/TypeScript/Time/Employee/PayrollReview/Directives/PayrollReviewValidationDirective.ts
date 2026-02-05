export class PayrollReviewValidationDirectiveFactory {
    //@ngInject
    public static create(): ng.IDirective {
        return {
            restrict: 'A',
            require: '?ngModel',
            scope: {
                ngModel: '=',
                rows: '='
            },
            link(scope: ng.IScope, elem: JQuery, attributes: ng.IAttributes, ngModelController: ng.INgModelController) {
                scope.$watch(() => scope['rows'], (newValues, oldValues, scope) => {
                    let rowsHasErrors: boolean = _.filter(scope['rows'], r => r.errorMessage).length > 0;
                    ngModelController.$setValidity("rowValid", !rowsHasErrors);
                }, true);
            }
        }
    }
}
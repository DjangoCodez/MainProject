export class EmployeeVehicleValidationDirectiveFactory {
    //@ngInject
    public static create(): ng.IDirective {
        return {
            restrict: 'A',
            require: '?ngModel',
            scope: {
                ngModel: '=',
                fromDate: '=',
                toDate: '=',
            },
            link(scope: ng.IScope, elem: JQuery, attributes: ng.IAttributes, ngModelController: ng.INgModelController) {
                scope.$watchGroup(['fromDate', 'toDate'], (newValues, oldValues, scope) => {
                    if (newValues) {
                        // From date must be before to date
                        var fromDate = scope['fromDate'];
                        var toDate = scope['toDate'];
                        ngModelController.$setValidity("dateRange", !fromDate || !toDate || fromDate.isBefore(toDate));
                    }
                });
            }
        }
    }
}


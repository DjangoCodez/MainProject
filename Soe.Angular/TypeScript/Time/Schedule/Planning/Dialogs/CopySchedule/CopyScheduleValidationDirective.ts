export class CopyScheduleValidationDirectiveFactory {
    //@ngInject
    public static create(): ng.IDirective {
        return {
            restrict: 'A',
            require: '?ngModel',
            scope: {
                ngModel: '=',
                sourceEmployeeId: '=',
                targetEmployeeId: '=',
                targetDateStart: '=',
                targetDateEnd: '=',
            },
            link(scope: ng.IScope, elem: JQuery, attributes: ng.IAttributes, ngModelController: ng.INgModelController) {
                scope.$watchGroup(['sourceEmployeeId', 'targetEmployeeId', 'targetDateStart', 'targetDateEnd'], (newValues, oldValues) => {
                    // Init parameters
                    var sourceEmployeeId: number = scope["sourceEmployeeId"];
                    var targetEmployeeId: number = scope["targetEmployeeId"];
                    var targetDateStart: Date = scope["targetDateStart"];
                    var targetDateEnd: Date = scope["targetDateEnd"];

                    ngModelController.$setValidity("sameEmployee", sourceEmployeeId !== targetEmployeeId || !sourceEmployeeId || !targetEmployeeId);
                    ngModelController.$setValidity("targetDates", targetDateStart && targetDateEnd && targetDateEnd.isAfterOnDay(targetDateStart) || !targetDateEnd);
                });
            }
        }
    }
}




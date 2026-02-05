import { EmployeeRequestDTO } from "../../Models/EmployeeRequestDTO";

export class EditEmployeeAvailabilityValidationDirectiveFactory {
    //@ngInject
    public static create(): ng.IDirective {
        return {
            restrict: 'A',
            require: '?ngModel',
            scope: {
                ngModel: '=',
                commentMandatory: '=',
                requests: '='
            },
            link(scope: ng.IScope, elem: JQuery, attributes: ng.IAttributes, ngModelController: ng.INgModelController) {

                scope.$watch(() => scope['requests'], (newValues, oldValues, scope) => {
                    let missingComment: boolean = false;

                    if (scope['commentMandatory']) {
                        _.forEach(scope['requests'], (req: EmployeeRequestDTO) => {
                            if (!req.comment)
                                missingComment = true;
                        });
                    }

                    ngModelController.$setValidity("requestComment", !missingComment);
                }, true);
            }
        }
    }
}
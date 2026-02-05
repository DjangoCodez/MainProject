import { AttestWorkFlowTemplateRowDTO } from "../../../../Common/Models/AttestWorkFlowDTOs";

export class AttestWorkFlowTemplateValidationDirectiveFactory {
    //@ngInject
    public static create(): ng.IDirective {
        return {
            restrict: 'A',
            require: '?ngModel',
            scope: {
                ngModel: '=',
            },
            link(scope: ng.IScope, elem: JQuery, attributes: ng.IAttributes, ngModelController: ng.INgModelController) {
                scope.$watch(() => (ngModelController.$modelValue), (newValue, oldvalue, scope) => {
                    let rows: AttestWorkFlowTemplateRowDTO[] = ngModelController.$modelValue;
                    ngModelController.$setValidity("rowsSelected", rows && rows.length > 0 && rows.filter(r => r.checked).length > 0);
                }, true);
            }
        }
    }
}
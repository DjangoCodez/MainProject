export class AccountDimsValidationDirectiveFactory {
    //@ngInject
    public static create(): ng.IDirective {
        return {
            restrict: 'A',
            require: '?ngModel',
            scope: {
                ngModel: '=',
                dimAccount1: '=',
                dimAccount2: '=',
                dimAccount3: '=',
                dimAccount4: '=',
                dimAccount5: '=',
                dimAccount6: '=',
                dim1Mandatory: '=',
                dim2Mandatory: '=',
                dim3Mandatory: '=',
                dim4Mandatory: '=',
                dim5Mandatory: '=',
                dim6Mandatory: '=',
                dim1Label: '=',
                dim2Label: '=',
                dim3Label: '=',
                dim4Label: '=',
                dim5Label: '=',
                dim6Label: '=',
            },
            link(scope: ng.IScope, elem: JQuery, attributes: ng.IAttributes, ngModelController: ng.INgModelController) {
                scope.$watchGroup(['dimAccount1', 'dimAccount2', 'dimAccount3', 'dimAccount4', 'dimAccount5', 'dimAccount6', 'dim1Mandatory', 'dim2Mandatory', 'dim3Mandatory', 'dim4Mandatory', 'dim5Mandatory', 'dim6Mandatory'], (newValues, oldValues, scope) => {
                    if (scope['dim1Mandatory'])
                        ngModelController.$setValidity("DIM_" + scope['dim1Label'], scope['dimAccount1'] > 0);
                    if (scope['dim2Mandatory'])
                        ngModelController.$setValidity("DIM_" + scope['dim2Label'], scope['dimAccount2'] > 0);
                    if (scope['dim3Mandatory'])
                        ngModelController.$setValidity("DIM_" + scope['dim3Label'], scope['dimAccount3'] > 0);
                    if (scope['dim4Mandatory'])
                        ngModelController.$setValidity("DIM_" + scope['dim4Label'], scope['dimAccount4'] > 0);
                    if (scope['dim5Mandatory'])
                        ngModelController.$setValidity("DIM_" + scope['dim5Label'], scope['dimAccount5'] > 0);
                    if (scope['dim6Mandatory'])
                        ngModelController.$setValidity("DIM_" + scope['dim6Label'], scope['dimAccount6'] > 0);
                });
            }
        }
    }
}
import { SplitAccountingRowDTO } from "../../../../Common/Models/AccountingRowDTO";

export class SplitAccountingValidationDirectiveFactory {
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

                    if (newValue) {
                        var accountingRows: SplitAccountingRowDTO[] = ngModelController.$modelValue;

                        var accountStandardMissing: boolean = false;
                        var accountInternalMissing: boolean = false;

                        _.forEach(accountingRows, (row) => {
                            // Standard account mandatory
                            if (!row.dim1Id || row.dim1Id === 0)
                                accountStandardMissing = true;

                            // Mandatory internal accounts
                            if (row.dim2Mandatory && (!row.dim2Id || row.dim2Id === 0))
                                accountInternalMissing = true;
                            if (row.dim3Mandatory && (!row.dim3Id || row.dim3Id === 0))
                                accountInternalMissing = true;
                            if (row.dim4Mandatory && (!row.dim4Id || row.dim4Id === 0))
                                accountInternalMissing = true;
                            if (row.dim5Mandatory && (!row.dim5Id || row.dim5Id === 0))
                                accountInternalMissing = true;
                        });

                        ngModelController.$setValidity("accountStandard", accountStandardMissing === false);
                        ngModelController.$setValidity("accountInternal", accountInternalMissing === false);
                    }
                }, true);
            }
        }
    }
}



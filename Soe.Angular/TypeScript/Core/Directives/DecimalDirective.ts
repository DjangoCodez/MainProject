import { NumberUtility } from "../../Util/NumberUtility";

export class DecimalDirectiveFactory {
    //@ngInject
    public static create(): ng.IDirective {
        return {
            restrict: 'A',
            require: 'ngModel',
            scope: false,
            link(scope: any, element: JQuery, attributes: any, ngModelController: ng.INgModelController) {
                // Get decimals (default is zero)
                let decimals = 0;
                if (attributes.decimal)
                    decimals = parseInt(attributes.decimal, 10);
                let maxDecimals = 0;
                if (attributes.maxDecimals)
                    maxDecimals = parseInt(attributes.maxDecimals, 10);

                let allowEmpty: boolean = attributes['allowEmpty'];

                ngModelController.$parsers.push(function (data) {
                    // Parse value to model
                    if (!data && allowEmpty)
                        return null;

                    return NumberUtility.parseDecimal(data);
                });

                ngModelController.$formatters.push(function (data) {
                    // Format value to GUI
                    if (!data) {
                        if (allowEmpty)
                            return null;
                        else
                            data = 0;
                    }

                    let dec: number = decimals;
                    if (attributes.noDecimalsIfInteger && Number.isInteger(data))
                        dec = 0;

                    if (attributes.clearZero && data == 0)
                        return null;

                    return data.toString().toFormattedNumber(dec, maxDecimals > dec ? maxDecimals : dec);
                });

                element.bind('blur', e => {
                    // Formatters will not run on data entered in GUI
                    var viewValue = ngModelController.$viewValue;
                    if (!viewValue && allowEmpty) {
                        ngModelController.$viewValue = null;
                    } else {
                        if (!viewValue)
                            viewValue = 0;

                        let dec: number = decimals;
                        if (attributes.noDecimalsIfInteger && Number.isInteger(NumberUtility.parseDecimal(viewValue)))
                            dec = 0;

                        ngModelController.$viewValue = viewValue.toString().toFormattedNumber(dec, maxDecimals > dec ? maxDecimals : dec);
                    }

                    ngModelController.$render();
                });
            }
        }
    }
}
import { TermGroup_ScanningInterpretation } from "../../Util/CommonEnumerations";
import { StringUtility } from "../../Util/StringUtility";

export class Validators {

    public static isAlphaNumeric(str: string) {
        var regex = /^[a-zåäöA-ZÅÄÖ0-9_]+$/;

        return regex.test(str);
    }

    public static isNumeric(value: string, allowDecimals: boolean = true, allowNegative: boolean = true) {
        if (!value)
            return true;

        var regex = new RegExp('^{0}\\d*{1}$'.format(allowNegative ? '[+-−]?' : '', allowDecimals ? '[\\.?\\,?]*\\d*' : ''));

        return regex.test(value);
    }

    public static isNumericNotZero(value: string) {
        var isNumeric: boolean = this.isNumeric(value);
        if (isNumeric) {
            var nbr: number = Number(value);
            if (nbr !== 0)
                return true;
        }

        return false;
    }

    public static isValidEmailAddress(eMail: string) {
        if (!eMail)
            return false;

        //var regex = /^(?("")(""[^""]+?""@)|(([0-9a-z_]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z_])@))(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9]{2,17}))$/g
        var regex = /^(([^<>()\[\]\\.,;:\s@"]+(\.[^<>()\[\]\\.,;:\s@"]+)*)|(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/;

        return regex.test(eMail);
    }

    public static isValidBic(bic: string, acceptEmpty: boolean) {
        if (!bic || StringUtility.isEmpty(bic))
            return acceptEmpty;

        const regex = new RegExp('^[A-Z0-9]{4}[A-Z]{2}[A-Z0-9]{2}([A-Z0-9]{3})?$'); 

        return regex.test(bic);
    }

    public static validateScanningEntryRow(newText: string, validationError: string) {
        var interpretation = TermGroup_ScanningInterpretation.ValueNotFound;
        if (newText && newText != "") {
            //Value is changed by user
            interpretation = TermGroup_ScanningInterpretation.ValueIsValid;
        }
        else {
            //0 - No errors. The value is correct.
            //1 - Possible interpretation error. For example, the interpretation seems correct, but it does not match a calculated value.
            //2 - Error. The field has probably not been interpreted correctly.
            if (validationError == "0")
                interpretation = TermGroup_ScanningInterpretation.ValueIsValid;
            else if (validationError == "1")
                interpretation = TermGroup_ScanningInterpretation.ValueIsUnsettled;
            else if (validationError == "2")
                interpretation = TermGroup_ScanningInterpretation.ValueNotFound;
        }
        return interpretation;
    }
}
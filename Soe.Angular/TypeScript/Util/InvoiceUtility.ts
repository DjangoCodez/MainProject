import { NumberUtility } from "./NumberUtility";

export class InvoiceUtility {

    static IsInvoiceDatesEntered(invoices: any[]): boolean {
        var valid = true;
        _.forEach(invoices, (invoice: any) => {
            if (invoice.invoiceDate == null)
                valid = false;
        });
        return valid;
    }

    static IsDateWithinCurrentAccountYear(date: Date, fromDate: Date, toDate: Date): boolean {
        return date != null && (new Date(<any>date).isAfterOnDay(fromDate)) && (new Date(<any>date).isBeforeOnDay(toDate));
    }

    static IsInvoiceDatesWithinCurrentAccountYear(invoices: any[], fromDate: Date, toDate: Date): boolean {
        var valid = true;
        _.forEach(invoices, (invoice: any) => {
            if (invoice.invoiceDate == null || (new Date(invoice.invoiceDate).isBeforeOnDay(fromDate)) || (new Date(invoice.invoiceDate).isAfterOnDay(toDate))) {
                valid = false;
            }
        });
        return valid;
    }

    static IsDueDatesEntered(invoices: any[]): boolean {
        var valid = true;
        _.forEach(invoices, (invoice: any) => {
            if (invoice.dueDate == null)
                valid = false;
        });
        return valid;
    }

    static IsDueDatesWithinCurrentAccountYear(invoices: any[], fromDate: Date, toDate: Date): boolean {
        var valid = true;
        _.forEach(invoices, (invoice: any) => {
            if (invoice.invoiceDate == null || (new Date(invoice.dueDate).isBeforeOnDay(fromDate)) || (new Date(invoice.dueDate).isAfterOnDay(toDate))) {
                valid = false;
            }
        });
        return valid;
    }

    static IsPayDatesEntered(invoices: any[]): boolean {
        var valid = true;
        _.forEach(invoices, (row) => {
            if (row.payDate === null) {
                valid = false;
            }
        });

        return valid;
    }

    static IsPayDatesWithinCurrentAccountYear(invoices: any[], fromDate: Date, toDate: Date): boolean {
        var valid = true;
        _.forEach(invoices, (invoice: any) => {
            if (invoice.payDate == null || (new Date(invoice.payDate).isBeforeOnDay(fromDate)) || (new Date(invoice.payDate).isAfterOnDay(toDate))) {
                valid = false;
            }
        });
        return valid;
    }

    static validateFIBankPaymentReference(referencenumber: string): boolean {
        if (referencenumber.substr(0, 2).toLowerCase() == "rf") {
            var numericString = referencenumber.substring(4, referencenumber.length - 1) + "2715" + referencenumber.substring(2, 2);
            return NumberUtility.parseDecimal(numericString) % 97 === 1;
        }
        else {
            var formedFIReference: string = "";
            var baseReference: string;

            /// <summary>
            /// FI - We need reference number with checksum. Minimum length is 4 characters. 
            /// Valid reference consists of a number which length is 3-19 characters + 1 checksum that is calculated here and added into
            /// reference. Banks are using reference to check validity of previous numbers in reference. 
            /// We copy all the characters, except last one and create reference based on copied value. If it's identical with reference 
            /// passed here, we can say it's a valid one. 
            /// </summary>

            var line: string;
            line = referencenumber;
            var linelength: number = line.length;
            if (linelength == 0)  // Empty must be able to insert
            {
                return true;
            }
            if (linelength < 4)  // First check that it's long enough
            {
                return false;
            }

            // Original ref without checksum
            baseReference = referencenumber.substring(0, line.length - 1);
            line = baseReference;
            linelength = line.length;

            var summa: number = 0;
            var multiplier: number = 7;  // Starting right, weighted by 7,2,1,7,3,1 etc...
            while (linelength > 0) {
                var character: any = line[linelength - 1];
                summa += multiplier * (character);//-48
                switch (multiplier) {
                    case 7: multiplier = 3; break;
                    case 3: multiplier = 1; break;
                    case 1: multiplier = 7; break;
                }
                summa %= 10;

                linelength -= 1;
            }
            summa = 10 - summa;
            if (summa != 10)
                formedFIReference = line + summa.toString();
            else
                formedFIReference = line + "0";

            if (formedFIReference == referencenumber) {
                return true;
            }
            return false;
        }
    }

    static existInInterval(value: number, from: number, to: number): boolean {
        if ((value >= from && value <= to))
            return true;
        else
            return false;
    }

    static addZeros(str: string, limit: number): string {
        var fillchar: string = "0";

        // Fill zeroes or empties before value
        if (str = null)
            str = "";

        var mystring: string = str;
        mystring = mystring.trim();

        //Remove ,
        mystring = mystring.replace(",", "");

        for (var i = 0; i < limit; i++) {
            if (mystring.length < limit) {
                mystring = fillchar + mystring;
            }
        }

        return mystring;
    }

    static validateModulus(modulus: number, validatestring: string, controldigit: string): boolean {
        if (modulus == 10) {
            var digits: number[] = [];
            _.forEach(validatestring.split(''), (char: string) => {
                digits.push(char.toOnlyNumbers(true));
            });

            var digitSum: number = 0;
            var checkDigit: number = 0;
            var paritet: number = digits.length - 1 % 2;
            for (var index = digits.length - 2; index >= 0; index--) {
                var digitValue: number = digits[index];
                // varannan multipliceras med 2 och varannan med 1...
                digitValue = digitValue * (((index + paritet) % 2) + 1);
                if (digitValue > 9) {
                    digitSum += digitValue / 10;
                    digitSum += digitValue % 10;
                }
                else {
                    digitSum += digitValue;
                }
            }

            checkDigit = (10 - (digitSum % 10)) % 10;

            if (checkDigit == controldigit.toOnlyNumbers(true))
                return true;
        }

        if (modulus == 11) {
            var digits: number[] = [];
            _.forEach(validatestring.split(''), (char: string) => {
                digits.push(char.toOnlyNumbers(true));
            });

            var digitSum: number = 0;
            var checkDigit = 0;
            var paritet = 2;
            for (var index = digits.length - 2; index >= 0; index--) {
                var digitValue: number = digits[index];

                // varannan multipliceras med 2 och varannan med 1...
                digitValue = digitValue * paritet;
                digitSum += digitValue;
                paritet++;

                if (paritet == 11)
                    paritet = 1;
            }

            checkDigit = (11 - (digitSum % 11)) % 11;

            if (checkDigit == controldigit.toOnlyNumbers(true))
                return true;
        }

        return false;
    }
}

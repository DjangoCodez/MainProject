export class DirectiveHelper {

    public static createTemplateElement(html: string, attrs: ng.IAttributes) {
        // Create root div
        var element = window.document.createElement('div');

        // Set form name, default is ctrl.edit if not passed in form attribute
        var formName = "ctrl.edit";
        if (attrs['form'])
            formName = attrs['form'];
        element.setAttribute("form", formName);

        // Set HTML content
        element.innerHTML = html;

        return element;
    }

    public static addEvent(scope: ng.IScope, element: any, subElementTagName: string, eventName: string) {
        if (scope[eventName]) {
            var elem;
            if (subElementTagName) {
                elem = element[0].getElementsByTagName(subElementTagName)[0];
            } else {
                elem = element[0];
            }
            elem.addEventListener(eventName, (x) => {
                scope[eventName]();
            });
        }
    }

    public static applyAttributes(element: any, attrs: any, controllerName: string) {
        // Get input element
        var inputElem = this.getInputElement(element);

        // Set Id and name on input element
        var idDefault: string = attrs['model'];
        var id = attrs['inputid'];
        if (id) {
            idDefault = id;
        }
        if (idDefault) {
            idDefault = idDefault.replace(/\./g, '_').replace(/\[/g, '').replace(/\]/g, '');

            if (inputElem) {
                inputElem.setAttribute("id", idDefault);
                inputElem.setAttribute("name", idDefault);
            }
        }

        if (inputElem) {
            // Read only
            if (attrs['readonly']) {
                inputElem.setAttribute('readonly', '');

                // Hide calendar button on date picker
                var datepicker = element[0].getElementsByClassName("datepicker-button")[0];
                if (datepicker)
                    datepicker.className = datepicker.className + " hidden"
            }

            // Checked
            if (attrs['checked'])
                inputElem.setAttribute('checked', 'checked');

            // Disabled
            if (attrs['disabled'])
                inputElem.setAttribute('disabled', '');

            //if (this.isSoeMultiselect(element))
            //    inputElem.setAttribute('data-ng-disabled', 'true'); //controllerName + '.disabled');

            // Validation
            var useValidation: boolean = false;

            // Required
            if (attrs['required']) {
                inputElem.setAttribute('required', '');
                useValidation = true;
            }
            
            //if (attrs['isRequired']) {
            //    console.log("attrs['isRequired']", attrs['isRequired']);
            //    inputElem.setAttribute('required', attrs['isRequired']);
            //    useValidation = true;
            //}
            // Date
            if (attrs['date']) {
                inputElem.setAttribute('date', '');
                useValidation = true;
            }

            // Text length
            if (attrs['minlength']) {
                inputElem.setAttribute('minlength', attrs['minlength']);
                useValidation = true;
            }
            if (attrs['maxlength']) {
                inputElem.setAttribute('maxlength', attrs['maxlength']);
                useValidation = true;
            }

            // Pattern
            if (attrs['pattern']) {
                inputElem.setAttribute('ng-pattern', attrs['pattern']);
                useValidation = true;
            }

            // Alpha numeric
            if (attrs['alphaNumeric']) {
                inputElem.setAttribute('validate-alpha-numeric', '');
                useValidation = true;
            }

            // Numeric
            if (attrs['numeric']) {
                inputElem.setAttribute('validate-numeric', '');
                if (attrs['min'])
                    inputElem.setAttribute('ng-min', attrs['min']);
                if (attrs['max'])
                    inputElem.setAttribute('ng-max', attrs['max']);
                if (attrs['noDecimals'])
                    inputElem.setAttribute('no-decimals', true);
                if (attrs['noNegative'])
                    inputElem.setAttribute('no-negative', true);
                if (attrs['noDecimalsIfInteger'])
                    inputElem.setAttribute('no-decimals-if-integer', true);
                if (attrs['clearZero'])
                    inputElem.setAttribute('clear-zero', true);
                useValidation = true;
            }

            // Numeric not zero
            if (attrs['numericNotZero']) {
                inputElem.setAttribute('validate-numeric-not-zero', '');
                useValidation = true;
            }

            if (attrs['allowEmpty']) {
                inputElem.setAttribute('allow-empty', attrs['allowEmpty']);
            }

            // Email
            if (attrs['email']) {
                inputElem.setAttribute('validate-email', '');
                useValidation = true;
            }

            // Social security number
            if (attrs['socialSecurityNumber']) {
                inputElem.setAttribute('validate-social-security-number', '');
                inputElem.setAttribute('check-valid-date', attrs['checkValidDate']);
                inputElem.setAttribute('must-specify-century', attrs['mustSpecifyCentury']);
                inputElem.setAttribute('must-specify-dash', attrs['mustSpecifyDash']);
                inputElem.setAttribute('sex', attrs['sex']);
                useValidation = true;
            }

            if (useValidation && !attrs['readonly']) {
                var elemName: string = "";
                if (controllerName)
                    elemName = elemName + controllerName + ".";
                elemName = elemName + "form." + idDefault;
                element[0].setAttribute('data-ng-class', "{\'has-error has-feedback\':(" + elemName + ".$touched && " + elemName + ".$invalid)}");
            }

            // Focus
            if (attrs['autoFocus'])
                inputElem.setAttribute('autofocus', '');

            // Tab index
            if (attrs['tabindex'])
                inputElem.setAttribute('tabindex', attrs['tabindex']);
        }

        // Get label element
        var labelElem = this.getLabelElement(element, inputElem);

        // Label for
        if (labelElem && idDefault) {
            labelElem.setAttribute("for", idDefault);
        }
        return idDefault;
    }

    public static removeAttributes(element: any, attrs: any) {
        // Remove attribute on the directive itself

        // Get input element
        var inputElem = this.getInputElement(element);

        if (inputElem) {
            // Read only
            if (attrs['readonly'])
                element[0].removeAttribute('readonly');

            // Checked
            if (attrs['checked'])
                element[0].removeAttribute('checked');

            // Disabled
            if (attrs['disabled'])
                element[0].removeAttribute('disabled');
            
            // Required
            if (attrs['required'] /*|| (attrs['isRequired'])*/)
                element[0].removeAttribute('required');

            // Text length
            if (attrs['minlength'])
                element[0].removeAttribute('minlength');
            if (attrs['maxlength'])
                element[0].removeAttribute('maxlength');

            // Pattern
            if (attrs['pattern'])
                element[0].removeAttribute('pattern');

            // Alpha numeric
            if (attrs['alphaNumeric'])
                element[0].removeAttribute('alpha-numeric');

            // Email
            if (attrs['email'])
                element[0].removeAttribute('email');

            // Social security number
            if (attrs['socialSecurityNumber']) {
                element[0].removeAttribute('social-security-number');
                element[0].removeAttribute('check-valid-date');
                element[0].removeAttribute('must-specify-century');
                element[0].removeAttribute('must-specify-dash');
                element[0].removeAttribute('allow-empty');
                element[0].removeAttribute('sex');
            }

            // Set focus
            if (attrs['autoFocus'])
                element[0].removeAttribute('auto-focus');

            // Tab index
            if (attrs['tabindex'])
                element[0].removeAttribute('tabindex');
        }
    }

    public static getInputElement(element: any): any {
        // Find input element
        var inputElem;
        if (element[0].getElementsByClassName("multiselect-parent")) {
            var div = element[0].getElementsByClassName("multiselect-parent")[0];
            if (div) {
                inputElem = element[0].getElementsByTagName("button")[0];
            }
        }
        if (!inputElem) {
            inputElem = element[0].getElementsByTagName("select")[0];
        }
        if (!inputElem) {
            inputElem = element[0].getElementsByTagName("input")[0];
        }
        if (!inputElem) {
            inputElem = element[0].getElementsByTagName("textarea")[0];
        }

        return inputElem;
    }

    public static getLabelElement(element: any, inputElem: any): any {
        var labelElem;
        if (inputElem && inputElem.type != "checkbox" && inputElem.type != "radio") {
            labelElem = element[0].getElementsByTagName("label")[0];
        }

        return labelElem;
    }

    public static isSoeMultiselect(element: any): boolean {
        if (element[0] && element[0].getElementsByClassName("multiselect-parent")) {
            var div = element[0].getElementsByClassName("multiselect-parent")[0];
            if (div) {
                var button = element[0].getElementsByTagName("button")[0];
                if (button)
                    return true;
            }
        }

        return false;
    }
}
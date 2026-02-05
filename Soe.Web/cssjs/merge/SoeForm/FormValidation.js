var FormValidation = {
    formInfos: {},
    invalidFields: null,

    initForm: function (form) {
        FormValidation.invalidFields = new Array();
        form.onsubmit = function () { return FormValidation.validateAll(this); };
        var formInfo = { groups: {}, fields: {} };
        FormValidation.formInfos[SOE.getID(form)] = formInfo;
        $(form).find('input').each(function () {
            var classes = this.className.split(' ');
            for (var i = 0; i < classes.length; i++) {
                if (classes[i].substring(0, 9) == 'validate-') {
                    $(this).bind('blur', function () {
                        FormValidation.validateField(this);
                    });
                    //JO bugfix: redundat events to prevent quirky tab order when validation field is last item before accessing save button
                    $(this).bind('keydown', function () { FormValidation.validateField(this); });

                    if (this.type == 'checkbox')
                        this.onclick = FormValidation.onChangeEvent;
                    else
                        this.onkeyup = FormValidation.onChangeEvent;

                    var fieldInfo = {
                        eventListeners: new Array(),
                        invalidTextElement: $$('invalid-' + SOE.getID(this)),
                        validated: false
                    }
                    formInfo.fields[this.id] = fieldInfo;

                    var parts = classes[i].split('-');
                    for (var p = 1; p < parts.length; p++) {
                        switch (parts[p]) {
                            case 'required':
                                fieldInfo.eventListeners.push(FormValidation.validateRequired);
                                break;
                            case 'email':
                                fieldInfo.eventListeners.push(FormValidation.validateEmail);
                                break;
                            case 'minlength':
                                p++;
                                fieldInfo.eventListeners.push(FormValidation.validateMinLength);
                                break;
                            case 'match':
                                p++;
                                fieldInfo.eventListeners.push(FormValidation.validateMatch);
                                break;
                            case 'luhn':
                                fieldInfo.eventListeners.push(FormValidation.validateLuhn);
                                break;
                        }
                    }

                    FormValidation.validateField(this);
                }
            }
        });
        $(form).find('select').each(function () { //JO 23.04.2009 Bugfix select validation
            var classes = this.className.split(' ');
            for (var i = 0; i < classes.length; i++) {
                if (classes[i].substring(0, 9) == 'validate-') {
                    this.onchange = function () { FormValidation.validateField(this); };
                    this.onkeyup = FormValidation.onChangeEvent;
                    var fieldInfo = {
                        eventListeners: new Array(),
                        invalidTextElement: $$('invalid-' + SOE.getID(this)),
                        validated: false
                    }
                    formInfo.fields[this.id] = fieldInfo;
                    var parts = classes[i].split('-');
                    for (var p = 1; p < parts.length; p++) {
                        switch (parts[p]) {
                            case 'notempty':
                                fieldInfo.eventListeners.push(FormValidation.validateNotEmpty);
                                break;
                        }
                    }
                    FormValidation.validateField(this);
                }
            }
        });
    },

    onChangeEvent: function () {
        if (FormValidation.formInfos[SOE.getID(this.form)].fields[SOE.getID(this)].validated)
            this.blur;
    },

    validateField: function (field) {
        var valid = true;
        if (field.form) {
            var fi = FormValidation.formInfos[field.form.id].fields[field.id];
            fi.eventListeners.each(function () {
                if (valid) {
                    if (!this(field)) // 'this' is a function!
                        valid = false;
                }
            });
            fi.validated = true;
            if (fi.invalidTextElement) {
                if (valid) {
                    fi.invalidTextElement.style.display = 'none';
                } else {
                    ToolTip.add(fi.invalidTextElement, fi.invalidTextElement.innerHTML);
                    fi.invalidTextElement.style.display = 'inline-block';
                }
            }
        }
        if (valid) {
            //FormValidation.invalidFields.remove(field); //Array.remove doesn' work in IE /NI 080707
            var index = -1;
            for (var i = 0; i < FormValidation.invalidFields.length; i++) {
                var tempField = FormValidation.invalidFields[i];
                if (tempField == field)
                    index = i;
            }
            if (index > -1) {
                FormValidation.invalidFields.splice(index, 1);
            }
        } else if (!FormValidation.invalidFields.contains(field)) {
            FormValidation.invalidFields.push(field);
        }
        $(field.form).find('*[type=submit]').each(function () {
            this.disabled = FormValidation.invalidFields.length > 0;
        });
        return valid;
    },

    //TODO: Don't allow hyphen as first or last character in domain name part.
    validateEmail: function () {
        if (!this.value)
            return true;

        var validCharsPostAt = 'abcdefghijklmnopqrstuvwxyz0123456789-';
        var validChars = validCharsPostAt + "!£#$%&'*+-/=?^_`{|}~";
        var foundAt = false;
        var foundDotAfterAt = false;
        var lastc = '';
        var email = this.value.toLowerCase();

        for (var i = 0; i < email.length; i++) {
            var c = email.charAt(i); // Indexer does not work in IE!
            if (c == '@') {
                if (foundAt || lastc == '.' || i == 0)
                    return false;
                foundAt = true;
                validChars = validCharsPostAt
            } else if (c == '.') {
                if (lastc == '.' || lastc == '@' || i == 0 || i == this.value.length - 1)
                    return false;
                foundDotAfterAt = foundAt;
            } else if (validChars.indexOf(c) < 0) {
                return false;
            }
            lastc = c;
        }

        if (!(foundAt && foundDotAfterAt))
            return false;

        return true;
    },
    validateNotEmpty: function (field) {
        for (var i = 0; i < field.length; i++) {
            if (field[i].selected == true) {
                if (field[i].text != "" || field.style.display == 'none')
                    return true;
            }
        }
        return false;
    },
    validateRequired: function (field) {
        var type = field.type.toLowerCase();
        return (type == 'text' || type == 'password') ? field.value || field.style.display == 'none' : field.checked;
    },
    validateMinLength: function () {
        var minLength = FormValidation.getParam(this.className, 'minlength');
        var length = this.value.length;
        return length == 0 || length >= minLength;
    },
    validateMatch: function () {
        var match = FormValidation.getParam(this.className, 'match');
        if (match) {
            var f = $(match);
            if (f)
                return this.value == f.value;
        }
        return true;
    },
    validateLuhn: function () {
        var sum = 0;
        for (var i = this.value.length - 1; i >= 0; i--) {
            var val = this.value.substring(i, i + 1) * (i % 2 - 2) * -1;
            if (val > 9)
                val -= 9;
            sum += val;
        }
        return sum % 10 == 0;
    },
    getParam: function (className, s) {
        var r = false;
        foreach(className.split(' '), function (cn) {
            if (cn.substring(0, 8) == 'validate') {
                var parts = cn.split('-');
                for (var i = 1; i <= parts.length; i++) {
                    if (parts[i] == s) {
                        if (++i <= parts.length)
                            r = parts[i];
                    }
                }
            }
        });
        return r;
    },
    validateAll: function (form) {
        var valid = true;
        var fields = FormValidation.formInfos[form.id].fields;
        for (var key in fields) {
            if (!FormValidation.validateField($$(key)))
                valid = false;
        }
        return valid;
    }
}

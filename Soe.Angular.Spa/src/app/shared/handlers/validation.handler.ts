import { Injectable } from '@angular/core';
import { AbstractControl, FormArray, ValidationErrors } from '@angular/forms';
import { TranslateService } from '@ngx-translate/core';
import { SoeFormGroup } from '@shared/extensions';
import { MessageboxService } from '@ui/dialog/services/messagebox.service';

export interface ErrorValue {
  value: string;
  translationKey: string;
  max: number;
  min: number;
  requiredLength: number;
  valueFrom: string;
  valueTo: string;
}
export interface AllValidationErrors {
  control_name: string;
  error_name: string;
  error_value: ErrorValue;
}

export interface FormGroupControls {
  [key: string]: AbstractControl;
}

export interface ValidationFieldTerms {
  [controlName: string]: string;
}

@Injectable({
  providedIn: 'root',
})
export class ValidationHandler {
  constructor(
    private translationService: TranslateService,
    private messageboxService: MessageboxService
  ) {}

  getFormValidationErrors(form: SoeFormGroup): AllValidationErrors[] {
    const errors = this.getFormValidationControlErrors(form.controls);

    const formErrors = form.errors;
    if (formErrors !== null) {
      Object.keys(formErrors).forEach(keyError => {
        // The TextEditor (Quill) has it's own required error key, don't show it as it will be duplicates
        if (keyError !== 'requiredError') {
          errors.push({
            control_name: 'form',
            error_name: keyError,
            error_value: formErrors[keyError],
          });
        }
      });
    }
    return errors;
  }

  getFormValidationControlErrors(
    controls: FormGroupControls
  ): AllValidationErrors[] {
    let errors: AllValidationErrors[] = [];
    Object.keys(controls).forEach(key => {
      const control = controls[key];
      if (control instanceof SoeFormGroup) {
        errors = errors.concat(this.getFormValidationErrors(control));
      } else if (control instanceof FormArray) {
        Object.keys(control.controls).forEach((key2: any) => {
          const controlItem = control.controls[key2];
          if (controlItem instanceof SoeFormGroup) {
            errors = errors.concat(this.getFormValidationErrors(controlItem));
          } else if (controlItem) {
            const controlErrors = controlItem.errors;
            if (controlErrors) {
              Object.keys(controlErrors).forEach(keyError => {
                if (keyError !== 'requiredError') {
                  errors.push({
                    control_name: `${key}[${key2}]`,
                    error_name: keyError,
                    error_value: controlErrors[keyError],
                  });
                }
              });
            }
          }
        });
      }
      const controlErrors: ValidationErrors | null = controls[key].errors;
      if (controlErrors !== null) {
        Object.keys(controlErrors).forEach(keyError => {
          // The TextEditor (Quill) has it's own required error key, don't show it as it will be duplicates
          if (keyError !== 'requiredError') {
            errors.push({
              control_name: key,
              error_name: keyError,
              error_value: controlErrors[keyError],
            });
          }
        });
      }
    });
    return errors;
  }

  showFormValidationErrors(
    form: SoeFormGroup,
    validationErrorStrings: string[] | null = null
  ) {
    let errorString = '';
    let formFieldString: string | null = null;
    let formFieldStrings: string[] = [];
    let formFieldTerms: ValidationFieldTerms = {};
    const termKeys = [
      'core.missingmandatoryfield',
      'core.exceededmaxvalueof',
      'core.exceededminvalueof',
      'core.notallowedvalue',
      'error.invaliddaterange',
      'core.incorrectmindecimals',
      'core.incorrectmaxdecimals',
      'core.incorrectdecimals',
      'core.invalidformat',
      'core.validation.minlength',
      'core.validation.maxlength',
      'core.validation.invalidrange',
    ];

    formFieldTerms = form.getValidationFieldTerms();
    formFieldStrings = form.getValidationFieldStrings();

    Object.keys(formFieldTerms).forEach(key => {
      if (termKeys.indexOf(formFieldTerms[key]) == -1) {
        termKeys.push(formFieldTerms[key]);
      }
    });

    formFieldStrings.forEach(value => {
      formFieldString += value + '\n';
    });

    if (!form.valid || form.hasNestedControls) {
      this.translationService
        .get(termKeys)
        .toPromise()
        .then(terms => {
          const errors = this.getFormValidationErrors(form);

          if (errors && errors.length > 0 && formFieldTerms) {
            errors.forEach(error => {
              const fieldName = formFieldTerms[error.control_name]
                ? terms[formFieldTerms[error.control_name]]
                : '';
              let text;
              switch (error.error_name) {
                case 'required':
                  text = terms['core.missingmandatoryfield'] + ' ' + fieldName;
                  break;
                case 'max':
                  text =
                    fieldName +
                    ' ' +
                    terms['core.exceededmaxvalueof'] +
                    ' ' +
                    error.error_value.max;
                  break;
                case 'min':
                  text = `${fieldName} ${terms['core.exceededminvalueof']} ${error.error_value.min}`;
                  break;
                case 'notAllowed':
                  text =
                    fieldName +
                    ' ' +
                    terms['core.notallowedvalue'] +
                    ' ' +
                    error.error_value.value;
                  break;
                case 'minlength':
                  text = terms['core.validation.minlength']
                    .replace('{0}', fieldName)
                    .replace('{1}', error.error_value.requiredLength);
                  break;
                case 'maxlength':
                  text = terms['core.validation.maxlength']
                    .replace('{0}', fieldName)
                    .replace('{1}', error.error_value.requiredLength);
                  break;
                case 'minDecimals':
                  text =
                    fieldName +
                    ' ' +
                    terms['core.incorrectmindecimals'] +
                    ' ' +
                    error.error_value.value;
                  break;
                case 'maxDecimals':
                  text =
                    fieldName +
                    ' ' +
                    terms['core.incorrectmaxdecimals'] +
                    ' ' +
                    error.error_value.value;
                  break;
                case 'decimals':
                  text =
                    fieldName +
                    ' ' +
                    terms['core.incorrectdecimals'] +
                    ' ' +
                    error.error_value.value;
                  break;
                case 'invalidFormat':
                case 'email':
                  text = fieldName + ' ' + terms['core.invalidformat'];
                  break;
                //case 'pattern': text = `${fieldName} has wrong pattern`; break;
                case 'equal':
                  text =
                    fieldName +
                    'must be equal to ' +
                    error.error_value.value +
                    '!';
                  break;
                case 'dateValid':
                  text = fieldName + ': ' + terms['error.invaliddaterange'];
                  break;
                case 'custom':
                  /**
                   * I would like to use fieldName in the string builder,
                   * however, this service cannot access all term types so
                   * becomes a problem. Not sure if this is the best solution.
                   */
                  if (error.error_value.value) text = error.error_value.value;
                  if (error.error_value.translationKey)
                    text = this.translationService.instant(
                      error.error_value.translationKey
                    );
                  break;
                case 'invalidRange':
                  text = terms['core.validation.invalidrange'].format(
                    fieldName,
                    error.error_value.valueFrom,
                    error.error_value.valueTo
                  );
                  break;
                // case 'minRange':
                //   text =
                //     fieldName + ': Är mindre än ' + error.error_value.value;
                //   break;
                // case 'maxRange':
                //   text =
                //     fieldName + ': Är större än ' + error.error_value.value;
                //   break;
                default:
                  if (error.error_value?.toString() === 'true')
                    text = error.error_name;
                  else
                    text = `${fieldName}: ${error.error_name}: ${error.error_value}`;
              }
              errorString += text + '.\n';
            });
          }
          // Add any custom validation error strings
          if (validationErrorStrings) {
            validationErrorStrings.forEach(value => {
              errorString += value + '\n';
            });
          }
          if (formFieldString && formFieldString != '') {
            errorString += formFieldString;
          }
          this.messageboxService.error(
            form.getValidationMessageBoxTitleTranslationKey(),
            errorString
          );
        });
    }
  }
}

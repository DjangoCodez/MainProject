import {
  AbstractControl,
  ValidationErrors,
  AsyncValidatorFn,
} from '@angular/forms';

export class EmailValidator {
  /**
   * This validator was added because Angular's built-in was not enough.
   * It mimics the same regex pattern used when validating email ContactECom's in the Soe.Business.Core.ContactManager.
   */

  static pattern =
    /^(?!.*\.\.)(("[^"]+?")|(([^\s@])*(?<=[0-9a-z_])))@(\[(\d{1,3}\.){3}\d{1,3}\]|([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9]{2,17})$/;

  static isValid(value: string): boolean {
    return !value ? true : this.pattern.test(value);
  }
  static validateEmailFormat(): AsyncValidatorFn {
    return (control: AbstractControl): Promise<ValidationErrors | null> => {
      return new Promise<ValidationErrors | null>(resolve => {
        !this.isValid(control.value)
          ? resolve({ invalidFormat: { value: control.value } })
          : resolve(null);
      });
    };
  }
}

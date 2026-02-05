import {
  SoeCheckboxFormControl,
  SoeDateFormControl,
  SoeFormGroup,
  SoeSelectFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { ContactPersonDTO } from './contact-persons.model';
import { arrayToFormArray, clearAndSetFormArray } from '@shared/util/form-util';
import { FormArray, FormControl } from '@angular/forms';
import { EmailValidator } from '@shared/validators/email.validator';

interface IContactPersonsForm {
  validationHandler: ValidationHandler;
  element: ContactPersonDTO | undefined;
  openInDialog: boolean;
}
export class ContactPersonForm extends SoeFormGroup {
  public openInDialog: boolean;

  constructor({
    validationHandler,
    element,
    openInDialog,
  }: IContactPersonsForm) {
    super(validationHandler, {
      actorContactPersonId: new SoeTextFormControl(
        element?.actorContactPersonId || 0,
        {
          isIdField: true,
        }
      ),
      firstName: new SoeTextFormControl(
        element?.firstName || '',
        { isNameField: true, required: true, maxLength: 100, minLength: 1 },
        'common.firstname'
      ),
      lastName: new SoeTextFormControl(
        element?.lastName || '',
        { maxLength: 100 },
        'common.lastname'
      ),
      email: new SoeTextFormControl(
        element?.email || '',
        { maxLength: 50 },
        'common.email'
      ),
      phoneNumber: new SoeTextFormControl(
        element?.phoneNumber || '',
        { maxLength: 50 },
        'common.phone'
      ),
      position: new SoeSelectFormControl(element?.position || ''),
      hasConsent: new SoeCheckboxFormControl(element?.hasConsent || false),
      consentDate: new SoeDateFormControl(
        element?.consentDate || element?.hasConsent ? new Date() : '',
        { disabled: element?.hasConsent === false || true }
      ),
      categoryIds: arrayToFormArray(element?.categoryIds || []),
    });
    this.addEmailValidators();
    this.onCopy = this.doOnCopy.bind(this);
    this.openInDialog = openInDialog;
  }

  addEmailValidators(): void {
    this.email.addAsyncValidators(EmailValidator.validateEmailFormat());
    this.email.updateValueAndValidity();
  }

  doOnCopy(): void {
    clearAndSetFormArray(this.value.categoryIds, this.categoryIds);
  }

  reset(value: ContactPersonDTO): void {
    clearAndSetFormArray(value?.categoryIds, this.categoryIds);
  }

  customCategoryIdsPatchValue(categoryIds: number[]) {
    clearAndSetFormArray(categoryIds, this.categoryIds);
  }

  customPatchValue(value: ContactPersonDTO) {
    this.reset(value);
    this.customCategoryIdsPatchValue(value.categoryIds);
  }

  get actorContactPersonId(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.actorContactPersonId;
  }

  get firstName(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.firstName;
  }

  get lastName(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.lastName;
  }

  get email(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.email;
  }

  get phoneNumber(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.phoneNumber;
  }

  get position(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.position;
  }

  get consentDate(): SoeDateFormControl {
    return <SoeDateFormControl>this.controls.consentDate;
  }

  get hasConsent(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.hasconsent;
  }

  get categoryIds(): FormArray<FormControl<number>> {
    return <FormArray>this.controls.categoryIds;
  }
}

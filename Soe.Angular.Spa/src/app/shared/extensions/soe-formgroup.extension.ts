import { Type } from '@angular/core';
import {
  AbstractControl,
  AbstractControlOptions,
  AsyncValidatorFn,
  FormArray,
  FormControl,
  FormGroup,
  ValidationErrors,
  ValidatorFn,
  Validators,
} from '@angular/forms';
import { ValidationFieldTerms, ValidationHandler } from '@shared/handlers';
import { DateUtil } from '@shared/util/date-util';
import { assignWith } from 'lodash';
import { DateValidators } from '../ui-components/validators/date.validator';
import { SoeEntityState } from '@shared/models/generated-interfaces/Enumerations';
import { take } from 'rxjs/operators';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { DateRangeValidator } from '@shared/validators/daterange.validator';
import { NumberRangeValidator } from '@shared/validators/numberrange.validator';
import { TimeRangeValidator } from '@shared/validators/timerange.validator';
import {
  TimeRangeSingleValue,
  TimeRangeValue,
} from '@ui/forms/timebox/timerange/timerange.component';

export enum SoeValidationOperands {
  Equal = 0,
  Minimum = 1,
  Maximum = 2,
}

export class SoeGetFormValueOptions {
  includeDisabled?: boolean;
}

export class SoeFormControlOptions {
  isIdField?: boolean;
  isNameField?: boolean;
  required?: boolean;
  disabled?: boolean;
  defaultValue?: any;

  public static default(): SoeFormControlOptions {
    return {} as SoeFormControlOptions;
  }

  public static getValidatorFns(
    options: SoeFormControlOptions
  ): ValidatorFn[] | null {
    const validatorFns: ValidatorFn[] = [];
    if (options.required) validatorFns.push(Validators.required);
    return validatorFns;
  }

  public static getAsyncValidatorFns(
    options: SoeFormControlOptions
  ): AsyncValidatorFn[] | null {
    return null;
  }
}

export class DateFormControlOptions extends SoeFormControlOptions {
  noValidate?: boolean;
  greaterThanDate?: string;
  lessThanDate?: string;

  public static default(): SoeFormControlOptions {
    return {} as SoeFormControlOptions;
  }

  public static getValidatorFns(
    options: DateFormControlOptions
  ): ValidatorFn[] | null {
    const validatorFns: ValidatorFn[] = [];

    if (options.greaterThanDate) {
      validatorFns.push(DateValidators.dateLessThan(options.greaterThanDate));
    }

    if (options.lessThanDate) {
      validatorFns.push(DateValidators.dateGreaterThan(options.lessThanDate));
    }

    return validatorFns.concat(super.getValidatorFns(options) ?? []);
  }

  public static getAsyncValidatorFns(
    options: DateFormControlOptions
  ): AsyncValidatorFn[] | null {
    const validatorFns: AsyncValidatorFn[] = [];
    if (!options.noValidate)
      validatorFns.push(SoeFormControl.validateDateFormat());
    return validatorFns.concat(super.getAsyncValidatorFns(options) ?? []);
  }
}

export class DateRangeFormControlOptions extends SoeFormControlOptions {
  requiredFrom?: boolean;
  requiredTo?: boolean;
  validateRange?: boolean;

  public static default(): SoeFormControlOptions {
    return {} as SoeFormControlOptions;
  }

  public static getValidatorFns(
    options: DateRangeFormControlOptions
  ): ValidatorFn[] | null {
    const validatorFns: ValidatorFn[] = [];

    if (options.requiredFrom)
      validatorFns.push(DateRangeValidator.requiredFrom);
    if (options.requiredTo) validatorFns.push(DateRangeValidator.requiredTo);

    return validatorFns.concat(super.getValidatorFns(options) ?? []);
  }

  public static getAsyncValidatorFns(
    options: DateRangeFormControlOptions
  ): AsyncValidatorFn[] | null {
    const validatorFns: AsyncValidatorFn[] = [];
    if (!options.validateRange)
      validatorFns.push(SoeFormControl.validateDateRange());
    return validatorFns.concat(super.getAsyncValidatorFns(options) ?? []);
  }
}

export class NumberFormControlOptions extends SoeFormControlOptions {
  maxValue?: number;
  minValue?: number;
  maxDecimals?: number;
  minDecimals?: number;
  decimals?: number;
  zeroNotAllowed?: boolean;

  public static default(): SoeFormControlOptions {
    return {} as SoeFormControlOptions;
  }

  public static getValidatorFns(
    options: NumberFormControlOptions
  ): ValidatorFn[] | null {
    const validatorFns: ValidatorFn[] = [];
    if (options.maxValue || options.maxValue === 0)
      validatorFns.push(Validators.max(options.maxValue));
    if (options.minValue || options.minValue === 0)
      validatorFns.push(Validators.min(options.minValue));
    return validatorFns.concat(super.getValidatorFns(options) ?? []);
  }

  public static getAsyncValidatorFns(
    options: NumberFormControlOptions
  ): AsyncValidatorFn[] | null {
    const validatorFns: AsyncValidatorFn[] = [];
    if (options.zeroNotAllowed)
      validatorFns.push(SoeFormControl.validateZeroNotAllowed());
    if (options.minDecimals)
      validatorFns.push(
        SoeFormControl.validateDecimals(
          options.minDecimals,
          SoeValidationOperands.Minimum
        )
      );
    if (options.maxDecimals)
      validatorFns.push(
        SoeFormControl.validateDecimals(
          options.maxDecimals,
          SoeValidationOperands.Maximum
        )
      );
    if (options.decimals)
      validatorFns.push(
        SoeFormControl.validateDecimals(
          options.decimals,
          SoeValidationOperands.Equal
        )
      );
    return validatorFns.concat(super.getAsyncValidatorFns(options) ?? []);
  }
}

export class NumberRangeFormControlOptions extends SoeFormControlOptions {
  requiredFrom?: boolean;
  requiredTo?: boolean;
  validateRange?: boolean;

  public static default(): SoeFormControlOptions {
    return {} as SoeFormControlOptions;
  }

  public static getValidatorFns(
    options: NumberRangeFormControlOptions
  ): ValidatorFn[] | null {
    const validatorFns: ValidatorFn[] = [];

    if (options.requiredFrom)
      validatorFns.push(NumberRangeValidator.requiredFrom);
    if (options.requiredTo) validatorFns.push(NumberRangeValidator.requiredTo);

    return validatorFns.concat(super.getValidatorFns(options) ?? []);
  }

  public static getAsyncValidatorFns(
    options: NumberRangeFormControlOptions
  ): AsyncValidatorFn[] | null {
    const validatorFns: AsyncValidatorFn[] = [];
    if (!options.validateRange)
      validatorFns.push(SoeFormControl.validateNumberRange());
    return validatorFns.concat(super.getAsyncValidatorFns(options) ?? []);
  }
}

export class TimeFormControlOptions extends SoeFormControlOptions {
  maxValue?: number;
  minValue?: number;

  public static default(): SoeFormControlOptions {
    return {} as SoeFormControlOptions;
  }

  public static getValidatorFns(
    options: NumberFormControlOptions
  ): ValidatorFn[] | null {
    const validatorFns: ValidatorFn[] = [];
    if (options.maxValue || options.maxValue === 0)
      validatorFns.push(Validators.max(options.maxValue));
    if (options.minValue || options.minValue === 0)
      validatorFns.push(Validators.min(options.minValue));
    return validatorFns.concat(super.getValidatorFns(options) ?? []);
  }

  public static getAsyncValidatorFns(
    options: NumberFormControlOptions
  ): AsyncValidatorFn[] | null {
    let validatorFns: AsyncValidatorFn[] = [];

    validatorFns = super.getAsyncValidatorFns(options) ?? [];
    return validatorFns;
  }
}

export class TimeRangeFormControlOptions extends SoeFormControlOptions {
  requiredFrom?: boolean;
  requiredTo?: boolean;
  validateRange?: boolean;

  public static default(): SoeFormControlOptions {
    return {} as SoeFormControlOptions;
  }

  public static getValidatorFns(
    options: TimeRangeFormControlOptions
  ): ValidatorFn[] | null {
    const validatorFns: ValidatorFn[] = [];

    if (options.requiredFrom)
      validatorFns.push(TimeRangeValidator.requiredFrom);
    if (options.requiredTo) validatorFns.push(TimeRangeValidator.requiredTo);

    return validatorFns.concat(super.getValidatorFns(options) ?? []);
  }

  public static getAsyncValidatorFns(
    options: NumberRangeFormControlOptions
  ): AsyncValidatorFn[] | null {
    const validatorFns: AsyncValidatorFn[] = [];
    if (!options.validateRange)
      validatorFns.push(SoeFormControl.validateTimeRange());
    return validatorFns.concat(super.getAsyncValidatorFns(options) ?? []);
  }
}

export class SelectFormControlOptions extends SoeFormControlOptions {
  zeroNotAllowed?: boolean;

  public static default(): SoeFormControlOptions {
    return {} as SoeFormControlOptions;
  }

  public static getValidatorFns(
    options: SelectFormControlOptions
  ): ValidatorFn[] | null {
    return super.getValidatorFns(options);
  }

  public static getAsyncValidatorFns(
    options: SelectFormControlOptions
  ): AsyncValidatorFn[] | null {
    const validatorFns: AsyncValidatorFn[] = [];
    if (options.zeroNotAllowed)
      validatorFns.push(SoeFormControl.validateZeroNotAllowed());
    return validatorFns.concat(super.getAsyncValidatorFns(options) ?? []);
  }
}

export class TextFormControlOptions extends SoeFormControlOptions {
  minLength?: number;
  maxLength?: number;

  public static default(): SoeFormControlOptions {
    return {} as SoeFormControlOptions;
  }

  public static getValidatorFns(
    options: TextFormControlOptions
  ): ValidatorFn[] | null {
    const validatorFns: ValidatorFn[] = [];
    if (options.minLength)
      validatorFns.push(Validators.minLength(options.minLength));
    if (options.maxLength)
      validatorFns.push(Validators.maxLength(options.maxLength));
    return validatorFns.concat(super.getValidatorFns(options) ?? []);
  }

  public static getAsyncValidatorFns(
    options: TextFormControlOptions
  ): AsyncValidatorFn[] | null {
    const validatorFns: AsyncValidatorFn[] = [];
    if (options.required)
      validatorFns.push(SoeFormControl.validateSpacesIfRequired());
    return validatorFns.concat(super.getAsyncValidatorFns(options) ?? []);
  }
}

export class SoeFormGroup<TValue = any> extends FormGroup {
  // Properties
  modifyPermission: boolean | undefined;
  readOnlyPermission: boolean | undefined;
  modelId = 0;
  isNew = false;
  isCopy = false;
  additionalPropsOnCopy: any;
  hasNestedControls = false;
  data: any;
  dataType!: Type<any>;
  onCopy?: () => void;
  idFieldName: string;
  nameFieldName: string;
  records: SmallGenericType[] = [];
  gridData: any; // Sent through additionalProps from grid
  validationMessageBoxTitleTranslationKey: string = 'error.unabletosave_title';
  formValidationHandler!: ValidationHandler;

  constructor(
    private validationHandler: ValidationHandler,
    controls: any,
    validatorOrOpts?:
      | ValidatorFn
      | ValidatorFn[]
      | AbstractControlOptions
      | null
      | undefined,
    asyncValidator?: AsyncValidatorFn | AsyncValidatorFn[] | null | undefined,
    addCommonControls = true
  ) {
    super(controls, validatorOrOpts, asyncValidator);

    this.formValidationHandler = validationHandler;

    if (addCommonControls) this.addCommonControls();
    this.idFieldName = this.getIdFieldName();
    this.nameFieldName = this.getNameFieldName();

    // Check if any of the controls is a FormArray.
    // Tells the validation to check recursively
    Object.keys(controls).forEach(key => {
      if (
        controls[key] instanceof SoeFormGroup ||
        controls[key] instanceof FormArray
      ) {
        this.hasNestedControls = true;
      } else if (controls[key] instanceof SoeFormControl) {
        if ((<SoeFormControl>controls[key]).options?.disabled) {
          (<SoeFormControl>controls[key]).disable();
        }
      }
    });
  }

  public setvalidationMessageBoxTitleTranslationKey(title: string) {
    this.validationMessageBoxTitleTranslationKey = title;
  }
  public getValidationMessageBoxTitleTranslationKey(): string {
    return this.validationMessageBoxTitleTranslationKey;
  }

  private addCommonControls() {
    this.addControl(
      'created',
      new SoeDateFormControl(undefined, { noValidate: true })
    );
    this.addControl('createdBy', new SoeTextFormControl(undefined));
    this.addControl(
      'modified',
      new SoeDateFormControl(undefined, {
        noValidate: true,
      })
    );
    this.addControl('modifiedBy', new SoeTextFormControl(undefined));
    this.addControl('state', new SoeNumberFormControl(0));
    this.addControl('isActive', new SoeCheckboxFormControl(true));
    this.setUpStateToActiveConverter();
  }

  private setUpStateToActiveConverter(): void {
    this.controls.state.valueChanges.pipe(take(1)).subscribe(state => {
      (this.controls.isActive as SoeCheckboxFormControl).patchValue(
        state === SoeEntityState.Active
      );
    });
    this.controls.isActive.valueChanges.subscribe(isActive => {
      const isNotSet = typeof isActive === 'undefined' || isActive === null;
      if (isNotSet) {
        (this.controls.isActive as SoeCheckboxFormControl).setValue(
          this.controls.state.value === SoeEntityState.Active,
          { emitEvent: false }
        );
      }

      (this.controls.state as SoeNumberFormControl).patchValue(
        isActive || isNotSet ? SoeEntityState.Active : SoeEntityState.Inactive
      );
    });
  }

  getIdControl(): SoeFormControl | undefined {
    let ctrl: SoeFormControl | undefined = undefined;

    for (const key of Object.keys(this.controls)) {
      if (this.controls[key] instanceof SoeFormControl) {
        const formControl = <SoeFormControl>this.controls[key];
        if (formControl?.options?.isIdField) {
          ctrl = formControl;
          break;
        }
      }
    }

    return ctrl;
  }

  getIdFieldName(): string {
    let fieldName = '';

    for (const key of Object.keys(this.controls)) {
      if (this.controls[key] instanceof SoeFormControl) {
        const formControl = <SoeFormControl>this.controls[key];
        if (formControl?.options?.isIdField) {
          fieldName = key;
          break;
        }
      }
    }

    return fieldName;
  }

  getNameControl(): SoeFormControl | undefined {
    let ctrl: SoeFormControl | undefined = undefined;

    for (const key of Object.keys(this.controls)) {
      if (this.controls[key] instanceof SoeFormControl) {
        const formControl = <SoeFormControl>this.controls[key];
        if (formControl?.options?.isNameField) {
          ctrl = formControl;
          break;
        }
      }
    }

    return ctrl;
  }

  getNameFieldName(): string {
    let fieldName = '';

    for (const key of Object.keys(this.controls)) {
      if (this.controls[key] instanceof SoeFormControl) {
        const formControl = <SoeFormControl>this.controls[key];
        if (formControl?.options?.isNameField) {
          fieldName = key;
          break;
        }
      }
    }

    return fieldName;
  }

  getValidationFieldTerms() {
    let validationFieldTerms: ValidationFieldTerms = {};
    Object.keys(this.controls).forEach(key => {
      if (this.controls[key] instanceof SoeFormGroup) {
        const formGroup = <SoeFormGroup>this.controls[key];
        if (formGroup instanceof SoeFormGroup) {
          validationFieldTerms = Object.assign(
            validationFieldTerms,
            formGroup.getValidationFieldTerms()
          );
        }
      } else if (this.controls[key] instanceof FormArray) {
        const formArray = <FormArray>this.controls[key];
        Object.keys(formArray.controls).forEach((key2: any) => {
          const formGroup = <SoeFormGroup>formArray.controls[key2];
          if (formGroup instanceof SoeFormGroup) {
            validationFieldTerms = Object.assign(
              validationFieldTerms,
              formGroup.getValidationFieldTerms()
            );
          }
        });
      } else {
        const termValue = (<SoeFormControl<string>>this.controls[key])
          .validationFieldTermKey;
        if (termValue) validationFieldTerms[key] = termValue;
      }
    });
    return validationFieldTerms;
  }

  getValidationFieldStrings() {
    let validationFieldStrings: string[] = [];
    Object.keys(this.controls).forEach(key => {
      if (this.controls[key] instanceof SoeFormGroup) {
        const formGroup = <SoeFormGroup>this.controls[key];
        validationFieldStrings = validationFieldStrings.concat(
          formGroup.getValidationFieldStrings()
        );
      } else if (this.controls[key] instanceof FormArray) {
        const formArray = <FormArray>this.controls[key];
        Object.keys(formArray.controls).forEach((key2: any) => {
          const formGroup = <SoeFormGroup>formArray.controls[key2];
          if (formGroup instanceof SoeFormGroup) {
            validationFieldStrings = validationFieldStrings.concat(
              formGroup.getValidationFieldStrings()
            );
          }
        });
      } else {
        const stringValue = (<SoeFormControl<string>>this.controls[key])
          .validationFieldString;
        if (stringValue) validationFieldStrings?.push(stringValue);
      }
    });
    return validationFieldStrings;
  }

  openFormValidationErrors(additionalErors: string[] = []) {
    this.validationHandler.showFormValidationErrors(
      this,
      // This is used to add additional errors (already translated) values to the form
      // from outside the regular soe-form-group logic.

      additionalErors
    );
  }

  getAllValues(options?: SoeGetFormValueOptions) {
    const rawValue: any = {};
    Object.keys(this.controls).forEach(key => {
      const subControl = this.controls[key];
      let include = true;
      if (subControl.disabled && !options?.includeDisabled) include = false;

      if (include) {
        rawValue[key] = this.getAllValuesHelper(subControl, options);
      }
    });
    return rawValue;
  }

  private getAllValuesHelper(control: any, options?: SoeGetFormValueOptions) {
    if (control instanceof FormGroup || control instanceof SoeFormGroup) {
      const rawValue: any = {};
      Object.keys(control.controls).forEach(key => {
        const subControl = control.controls[key];
        let include = true;
        if (subControl.disabled && !options?.includeDisabled) include = false;
        if (include) {
          rawValue[key] = this.getAllValuesHelper(subControl, options);
        }
      });
      return rawValue;
    } else {
      return control.value;
    }
  }

  setData(data: any, modelId: number, isNew = false) {
    this.data = data;
    this.modelId = modelId;
    this.isNew = isNew;
    this.patchValue(this.data);
  }

  setDataType(dataType: Type<any>) {
    this.dataType = dataType;
    this.data = new this.dataType();
  }

  clearIfExists(ctrl: SoeFormControl) {
    if (ctrl) ctrl.patchValue(undefined);
  }
}

export class SoeFormControl<TValue = any> extends FormControl {
  public options?: SoeFormControlOptions;
  public validationFieldTermKey = '';
  public validationFieldString = '';

  constructor(
    formState: any,
    options?: SoeFormControlOptions,
    validatorOrOpts?:
      | ValidatorFn
      | ValidatorFn[]
      | AbstractControlOptions
      | null
      | undefined,
    asyncValidator?: AsyncValidatorFn | AsyncValidatorFn[] | null | undefined,
    validatorTermKey?: string,
    validatorString?: string
  ) {
    super(formState, validatorOrOpts, asyncValidator);
    this.options = options;

    if (validatorTermKey) this.setValidatorTermKey(validatorTermKey);
    if (validatorString) this.setValidatorString(validatorString);
  }

  setValidatorTermKey(key: string) {
    this.validationFieldTermKey = key;
  }

  setValidatorString(text: string) {
    this.validationFieldString = text;
  }

  /*static zeroNotAllowed(): ValidatorFn {
        var regExp: RegExp = /^0/;
        return (control: AbstractControl): ValidationErrors | null => {
            const forbidden = regExp.test(control.value);
            console.log(forbidden);
            console.log("control.value: ", control.value);
            return forbidden ? {notAllowed: {value: control.value}} : null;
        };
    }*/

  static validateSpacesIfRequired(): AsyncValidatorFn {
    const isWhitespaceString = (str: string) => !str.replace(/\s/g, '').length;
    return (control: AbstractControl): Promise<ValidationErrors | null> => {
      return new Promise<ValidationErrors | null>(resolve => {
        isWhitespaceString(control.value.toString())
          ? resolve({ required: { value: control.value } })
          : resolve(null);
      });
    };
  }

  static validateZeroNotAllowed(): AsyncValidatorFn {
    const regExp = /^0/;
    return (control: AbstractControl): Promise<ValidationErrors | null> => {
      const forbidden = regExp.test(control.value);
      return new Promise<ValidationErrors | null>(resolve => {
        forbidden
          ? resolve({ required: { value: control.value } })
          : resolve(null);
      });
    };
  }

  static validateDecimals(
    value: number,
    operand: SoeValidationOperands
  ): AsyncValidatorFn {
    const countDecimals = function (val: number) {
      if (Math.floor(val) !== val) {
        /*const delimiter = (1.2).toLocaleString(SoeConfigUtil.languageCode)[1];
        return (
          val?.toString().replace('.', delimiter).split(delimiter)[1]?.length ||
          0
        );*/
        const delimiter = (1.2).toLocaleString(undefined)[1];
        return val?.toString().split(delimiter)[1]?.length || 0;
      }
      return 0;
    };
    return (control: AbstractControl): Promise<ValidationErrors | null> => {
      return new Promise<ValidationErrors | null>(resolve => {
        const nrOfDecimals = countDecimals(control.value);
        switch (operand) {
          case SoeValidationOperands.Equal:
            nrOfDecimals != 0 && nrOfDecimals != value
              ? resolve({ decimals: { value } })
              : resolve(null);
            break;
          case SoeValidationOperands.Minimum:
            nrOfDecimals != 0 && nrOfDecimals < value
              ? resolve({ minDecimals: { value } })
              : resolve(null);
            break;
          case SoeValidationOperands.Maximum:
            nrOfDecimals != 0 && nrOfDecimals > value
              ? resolve({ maxDecimals: { value } })
              : resolve(null);
            break;
          default:
            resolve(null);
            break;
        }
      });
    };
  }

  static validateDateFormat(): AsyncValidatorFn {
    return (control: AbstractControl): Promise<ValidationErrors | null> => {
      const isValid = DateUtil.isValidDateOrString(control.value);
      return new Promise<ValidationErrors | null>(resolve => {
        !isValid
          ? resolve({ invalidFormat: { value: control.value } })
          : resolve(null);
      });
    };
  }

  static validateDateRange(): AsyncValidatorFn {
    return (control: AbstractControl): Promise<ValidationErrors | null> => {
      const dateFrom = control.value ? control.value[0] : null;
      const dateTo = control.value ? control.value[1] : null;
      const isInvalid = dateFrom && dateTo && dateFrom > dateTo;
      return new Promise<ValidationErrors | null>(resolve => {
        isInvalid
          ? resolve({
              invalidRange: {
                valueFrom: control.value
                  ? DateUtil.format(
                      control.value[0],
                      DateUtil.dateFnsLanguageDateFormats
                    )
                  : null,
                valueTo: control.value
                  ? DateUtil.format(
                      control.value[1],
                      DateUtil.dateFnsLanguageDateFormats
                    )
                  : null,
              },
            })
          : resolve(null);
      });
    };
  }

  static validateNumberRange(): AsyncValidatorFn {
    return (control: AbstractControl): Promise<ValidationErrors | null> => {
      const valueFrom = control.value ? control.value[0] : null;
      const valueTo = control.value ? control.value[1] : null;
      const isInvalid = valueFrom && valueTo && valueFrom > valueTo;
      return new Promise<ValidationErrors | null>(resolve => {
        isInvalid
          ? resolve({
              invalidRange: {
                valueFrom: control.value ? control.value[0] : null,
                valueTo: control.value ? control.value[1] : null,
              },
            })
          : resolve(null);
      });
    };
  }

  static validateTimeRange(): AsyncValidatorFn {
    return (control: AbstractControl): Promise<ValidationErrors | null> => {
      const valueFrom = control.value ? control.value[0] : null;
      const valueTo = control.value ? control.value[1] : null;
      const isInvalid = valueFrom && valueTo && valueFrom > valueTo;
      return new Promise<ValidationErrors | null>(resolve => {
        isInvalid
          ? resolve({
              invalidRange: {
                valueFrom: control.value ? control.value[0] : null,
                valueTo: control.value ? control.value[1] : null,
              },
            })
          : resolve(null);
      });
    };
  }

  static setDefaultOptionsIfNeeded<T extends SoeFormControlOptions>(
    options: T,
    defaultOptions: T
  ): T {
    return assignWith<T, T>(
      options,
      defaultOptions,
      (destinationValue, srcValue) =>
        destinationValue === undefined ? srcValue : destinationValue
    );
  }
}

export class SoeCheckboxFormControl<TValue = any> extends SoeFormControl {
  constructor(
    formState: any,
    options?: SoeFormControlOptions,
    validatorTermKey?: string,
    validatorString?: string
  ) {
    options = SoeFormControl.setDefaultOptionsIfNeeded(
      options || {},
      SoeFormControlOptions.default()
    );
    super(
      formState,
      options,
      SoeFormControlOptions.getValidatorFns(options),
      SoeFormControlOptions.getAsyncValidatorFns(options),
      validatorTermKey,
      validatorString
    );
  }
}

export class SoeDateFormControl<TValue = string> extends SoeFormControl {
  constructor(
    formState: any,
    options?: DateFormControlOptions,
    validatorTermKey?: string,
    validatorString?: string
  ) {
    options = SoeFormControl.setDefaultOptionsIfNeeded(
      options || {},
      DateFormControlOptions.default()
    );
    super(
      formState,
      options,
      DateFormControlOptions.getValidatorFns(options),
      DateFormControlOptions.getAsyncValidatorFns(options),
      validatorTermKey,
      validatorString
    );
  }
}

export class SoeDateRangeFormControl<
  TValue = [string, string],
> extends SoeFormControl {
  constructor(
    formState: any,
    options?: DateRangeFormControlOptions,
    validatorTermKey?: string,
    validatorString?: string
  ) {
    options = SoeFormControl.setDefaultOptionsIfNeeded(
      options || {},
      DateRangeFormControlOptions.default()
    );
    super(
      formState,
      options,
      DateRangeFormControlOptions.getValidatorFns(options),
      DateRangeFormControlOptions.getAsyncValidatorFns(options),
      validatorTermKey,
      validatorString
    );
  }
}

export class SoeNumberFormControl<TValue = number> extends SoeFormControl {
  constructor(
    formState: any,
    options?: NumberFormControlOptions,
    validatorTermKey?: string,
    validatorString?: string
  ) {
    options = SoeFormControl.setDefaultOptionsIfNeeded(
      options || {},
      NumberFormControlOptions.default()
    );
    super(
      formState,
      options,
      NumberFormControlOptions.getValidatorFns(options),
      NumberFormControlOptions.getAsyncValidatorFns(options),
      validatorTermKey,
      validatorString
    );
  }
}

export class SoeNumberRangeFormControl<
  TValue = [number, number],
> extends SoeFormControl {
  constructor(
    formState: any,
    options?: NumberRangeFormControlOptions,
    validatorTermKey?: string,
    validatorString?: string
  ) {
    options = SoeFormControl.setDefaultOptionsIfNeeded(
      options || {},
      NumberRangeFormControlOptions.default()
    );
    super(
      formState,
      options,
      NumberRangeFormControlOptions.getValidatorFns(options),
      NumberRangeFormControlOptions.getAsyncValidatorFns(options),
      validatorTermKey,
      validatorString
    );
  }
}

export class SoeTimeFormControl<
  TValue = TimeRangeSingleValue,
> extends SoeFormControl {
  constructor(
    formState: any,
    options?: TimeFormControlOptions,
    validatorTermKey?: string,
    validatorString?: string
  ) {
    options = SoeFormControl.setDefaultOptionsIfNeeded(
      options || {},
      TimeFormControlOptions.default()
    );
    super(
      formState,
      options,
      TimeFormControlOptions.getValidatorFns(options),
      TimeFormControlOptions.getAsyncValidatorFns(options),
      validatorTermKey,
      validatorString
    );
  }
}

export class SoeTimeRangeFormControl<
  TValue = TimeRangeValue,
> extends SoeFormControl {
  constructor(
    formState: any,
    options?: TimeRangeFormControlOptions,
    validatorTermKey?: string,
    validatorString?: string
  ) {
    options = SoeFormControl.setDefaultOptionsIfNeeded(
      options || {},
      TimeRangeFormControlOptions.default()
    );
    super(
      formState,
      options,
      TimeRangeFormControlOptions.getValidatorFns(options),
      TimeRangeFormControlOptions.getAsyncValidatorFns(options),
      validatorTermKey,
      validatorString
    );
  }
}

export class SoeRadioFormControl<TValue = any> extends SoeFormControl {
  constructor(
    formState: any,
    options?: SoeFormControlOptions,
    validatorTermKey?: string,
    validatorString?: string
  ) {
    options = SoeFormControl.setDefaultOptionsIfNeeded(
      options || {},
      SoeFormControlOptions.default()
    );
    super(
      formState,
      options,
      SoeFormControlOptions.getValidatorFns(options),
      SoeFormControlOptions.getAsyncValidatorFns(options),
      validatorTermKey,
      validatorString
    );
  }
}

export class SoeSelectFormControl<TValue = number> extends SoeFormControl {
  constructor(
    formState: any,
    options?: SelectFormControlOptions,
    validatorTermKey?: string,
    validatorString?: string
  ) {
    options = SoeFormControl.setDefaultOptionsIfNeeded(
      options || {},
      SelectFormControlOptions.default()
    );
    super(
      formState,
      options,
      SelectFormControlOptions.getValidatorFns(options),
      SelectFormControlOptions.getAsyncValidatorFns(options),
      validatorTermKey,
      validatorString
    );
  }
}

export class SoeSwitchFormControl<TValue = any> extends SoeFormControl {
  constructor(
    formState: any,
    options?: SoeFormControlOptions,
    validatorTermKey?: string,
    validatorString?: string
  ) {
    options = SoeFormControl.setDefaultOptionsIfNeeded(
      options || {},
      SoeFormControlOptions.default()
    );
    super(
      formState,
      options,
      SoeFormControlOptions.getValidatorFns(options),
      SoeFormControlOptions.getAsyncValidatorFns(options),
      validatorTermKey,
      validatorString
    );
  }
}

export class SoeTextFormControl<TValue = string> extends SoeFormControl {
  constructor(
    formState: any,
    options?: TextFormControlOptions,
    validatorTermKey?: string,
    validatorString?: string
  ) {
    options = SoeFormControl.setDefaultOptionsIfNeeded(
      options || {},
      TextFormControlOptions.default()
    );
    super(
      formState,
      options,
      TextFormControlOptions.getValidatorFns(options),
      TextFormControlOptions.getAsyncValidatorFns(options),
      validatorTermKey,
      validatorString
    );
  }
}

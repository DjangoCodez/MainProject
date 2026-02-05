import { FormArray, FormControl, FormGroup } from '@angular/forms';

export function arrayToFormArray(dataArray: any[]): FormArray {
  try {
    const formArray = new FormArray<any>([]);
    dataArray instanceof Array &&
      dataArray?.forEach(data => {
        formArray.push(objectToFormGroup(data));
      });

    return formArray;
  } catch (err: any) {
    console.error('Unable to create form array from array: ', err.message);
    return new FormArray<any>([]);
  }
}

export function objectToFormGroup(data: any) {
  const formGroup = new FormGroup<any>({});

  for (const key in data) {
    if (data.hasOwnProperty(key)) {
      const value = data[key];
      const formControl = new FormControl(value);
      formGroup.addControl(key, formControl);
    }
  }

  return typeof data === 'object' && !(data instanceof Date)
    ? formGroup
    : new FormControl(data);
}

export function clearAndSetFormArray(
  items: any[],
  formArray: FormArray,
  markAsDirty = false
) {
  formArray.clear();
  const n = items?.length;
  items?.forEach((item, i) => {
    if (i !== n - 1) {
      // I saw some excessive event emissions here, since it's called for each push.
      formArray.push(objectToFormGroup(item), { emitEvent: false });
    } else {
      formArray.push(objectToFormGroup(item));
    }
  });
  if (markAsDirty) formArray.markAsDirty();
}

import {
  SoeDateFormControl,
  SoeFormGroup,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { CommodityCodeUploadDTO } from './commodity-codes.model';

interface ICommodityCodesUploadForm {
  validationHandler: ValidationHandler;
  element: CommodityCodeUploadDTO | undefined;
}
export class CommodityCodesUploadForm extends SoeFormGroup {
  constructor({ validationHandler, element }: ICommodityCodesUploadForm) {
    super(validationHandler, {
      year: new SoeTextFormControl(element?.year, {
        required: true,
      }),
      fileString: new SoeTextFormControl(
        element?.fileString || '',
        {
          required: true,
        },
        'core.fileupload.filename'
      ),
      fileName: new SoeTextFormControl(element?.fileName || ''),
      selectedDate: new SoeDateFormControl(
        element?.selectedDate,
        {
          required: true,
        },
        'common.date'
      ),
    });

    const date = new Date();
    this.patchValue({
      year: date.getFullYear(),
      selectedDate: new Date(date.getFullYear(), 0, 1),
    });
    this.markAsDirty();
  }

  get year(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.year;
  }

  get selectedDate(): SoeDateFormControl {
    return <SoeDateFormControl>this.controls.selectedDate;
  }

  get fileString(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.fileString;
  }

  get fileName(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.fileName;
  }
}

import {
  SoeDateFormControl,
  SoeFormGroup,
  SoeSelectFormControl,
} from '@shared/extensions';
import { IntrastatExportGridHeaderDTO } from './intrastat-export.model';
import { ValidationHandler } from '@shared/handlers';
import { IntrastatReportingType } from '@shared/models/generated-interfaces/Enumerations';

interface IIntrastatExportGridHeaderForm {
  validationHandler: ValidationHandler;
  element: IntrastatExportGridHeaderDTO;
}

export class IntrastatExportGridHeaderForm extends SoeFormGroup {
  constructor({ validationHandler, element }: IIntrastatExportGridHeaderForm) {
    super(validationHandler, {
      fromDate: new SoeDateFormControl(element.fromDate || ''),
      endDate: new SoeDateFormControl(element.endDate || ''),
      reportingType: new SoeSelectFormControl(
        element.reportingType || IntrastatReportingType.Both
      ),
    });
  }

  get fromDate(): SoeDateFormControl {
    return <SoeDateFormControl>this.controls.fromDate;
  }

  get endDate(): SoeDateFormControl {
    return <SoeDateFormControl>this.controls.endDate;
  }

  get reportingType(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.reportingType;
  }
}

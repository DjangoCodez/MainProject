import { ValidationHandler } from '@shared/handlers';
import { SearchSysLogsDTO } from './support-logs.model';
import {
  SoeCheckboxFormControl,
  SoeDateFormControl,
  SoeFormGroup,
  SoeNumberFormControl,
  SoeSelectFormControl,
  SoeTextFormControl,
} from '@shared/extensions';

interface ISearchSysLogsForm {
  validationHandler: ValidationHandler;
  element: SearchSysLogsDTO | undefined;
}

export class SearchSysLogsForm extends SoeFormGroup {
  constructor({ validationHandler, element }: ISearchSysLogsForm) {
    super(validationHandler, {
      licenseSearch: new SoeTextFormControl(element?.licenseSearch || ''),
      companySearch: new SoeTextFormControl(element?.companySearch || ''),
      roleSearch: new SoeTextFormControl(element?.roleSearch || ''),
      userSearch: new SoeTextFormControl(element?.userSearch || ''),
      fromDate: new SoeDateFormControl(element?.fromDate || undefined),
      fromTime: new SoeTextFormControl(element?.fromTime || ''),
      toDate: new SoeDateFormControl(element?.toDate || undefined),
      toTime: new SoeTextFormControl(element?.toTime || ''),
      incMessageSearch: new SoeTextFormControl(element?.incMessageSearch || ''),
      exlMessageSearch: new SoeTextFormControl(element?.exlMessageSearch || ''),
      incExceptionSearch: new SoeTextFormControl(
        element?.incExceptionSearch || ''
      ),
      exExceptionSearch: new SoeTextFormControl(
        element?.exExceptionSearch || ''
      ),
      level: new SoeTextFormControl(element?.level || 'NONE'),
      levelSelect: new SoeSelectFormControl(element?.levelSelect || 1),
      noOfrecords: new SoeNumberFormControl(element?.noOfrecords || ''),
      showUnique: new SoeCheckboxFormControl(element?.showUnique || false),
    });
  }

  get licenseSearch(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.licenseSearch;
  }

  get companySearch(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.companySearch;
  }

  get roleSearch(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.roleSearch;
  }

  get userSearch(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.userSearch;
  }

  get fromDate(): SoeDateFormControl {
    return <SoeDateFormControl>this.controls.fromDate;
  }

  get fromTime(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.fromTime;
  }

  get toDate(): SoeDateFormControl {
    return <SoeDateFormControl>this.controls.toDate;
  }

  get toTime(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.toTime;
  }

  get incMessageSearch(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.incMessageSearch;
  }

  get exlMessageSearch(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.exlMessageSearch;
  }

  get incExceptionSearch(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.incExceptionSearch;
  }

  get exExceptionSearch(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.exExceptionSearch;
  }

  get level(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.level;
  }

  get levelSelect(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.levelSelect;
  }

  get noOfrecords(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.noOfrecords;
  }
}

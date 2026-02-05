import { ValidationHandler } from '@shared/handlers';
import { ProjectCentralSelectionDTO } from './project-central.model';
import {
  SoeCheckboxFormControl,
  SoeDateFormControl,
  SoeFormGroup,
} from '@shared/extensions';

interface IProjectCentralSelectionForm {
  validationHandler: ValidationHandler;
  element: ProjectCentralSelectionDTO | undefined;
}

export class ProjectCentralSelectionForm extends SoeFormGroup {
  constructor({ validationHandler, element }: IProjectCentralSelectionForm) {
    super(validationHandler, {
      dateFrom: new SoeDateFormControl(element?.dateFrom || undefined),
      dateTo: new SoeDateFormControl(element?.dateTo || undefined),
      includeSubProjects: new SoeCheckboxFormControl(
        element?.includeSubProjects || false
      ),
      showDetailedInformation: new SoeCheckboxFormControl(
        element?.showDetailedInformation || false
      ),
    });
  }

  get dateFrom(): SoeDateFormControl {
    return <SoeDateFormControl>this.controls.dateFrom;
  }

  get dateTo(): SoeDateFormControl {
    return <SoeDateFormControl>this.controls.dateTo;
  }

  get includeSubProjects(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.includeSubProjects;
  }

  get showDetailedInformation(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.showDetailedInformation;
  }
}

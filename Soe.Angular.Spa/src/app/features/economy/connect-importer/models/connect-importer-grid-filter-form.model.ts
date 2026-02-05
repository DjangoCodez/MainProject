import { SoeFormGroup, SoeSelectFormControl } from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { ConnectImporterGridFilterDTO } from './connect-importer.model';
import { TermGroup_GridDateSelectionType } from '@shared/models/generated-interfaces/Enumerations';

interface IConnectImporterGridFilterForm {
  validationHandler: ValidationHandler;
  element: ConnectImporterGridFilterDTO | undefined;
}
export class ConnectImporterGridFilterForm extends SoeFormGroup {
  constructor({ validationHandler, element }: IConnectImporterGridFilterForm) {
    super(validationHandler, {
      iOImportHeadType: new SoeSelectFormControl(
        element?.iOImportHeadType || 0,
        { zeroNotAllowed: false }
      ),
      dateSelectionId: new SoeSelectFormControl(
        element?.dateSelectionId || TermGroup_GridDateSelectionType.One_Month,
        {
          zeroNotAllowed: false,
        }
      ),
    });
  }

  get iOImportHeadType(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.iOImportHeadType;
  }

  get dateSelectionId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.dateSelectionId;
  }
}

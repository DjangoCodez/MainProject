import {
  SoeCheckboxFormControl,
  SoeFormGroup,
  SoeNumberFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { SoeEntityState } from '@shared/models/generated-interfaces/Enumerations';
import { IAccountDistributionRowDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';

interface IAccountDistributionRowForm {
  validationHandler: ValidationHandler;
  element: IAccountDistributionRowDTO | undefined;
}

export class AccountDistributionRowForm extends SoeFormGroup {
  constructor({ validationHandler, element }: IAccountDistributionRowForm) {
    super(validationHandler, {
      accountDistributionRowId: new SoeNumberFormControl(
        element?.accountDistributionRowId ?? 0,
        {
          isIdField: true,
        }
      ),
      accountDistributionHeadId: new SoeNumberFormControl(
        element?.accountDistributionHeadId ?? 0
      ),
      rowNbr: new SoeNumberFormControl(element?.rowNbr ?? null),
      calculateRowNbr: new SoeNumberFormControl(element?.calculateRowNbr ?? 0),
      sameBalance: new SoeNumberFormControl(element?.sameBalance ?? 0),
      oppositeBalance: new SoeNumberFormControl(element?.oppositeBalance ?? 0),
      description: new SoeTextFormControl(element?.description ?? ''),
      state: new SoeNumberFormControl(element?.state ?? SoeEntityState.Active),
      dim1Id: new SoeNumberFormControl(element?.dim1Id ?? null),
      dim1Nr: new SoeTextFormControl(element?.dim1Nr ?? ''),
      dim1Name: new SoeTextFormControl(element?.dim1Name ?? ''),
      dim1Disabled: new SoeCheckboxFormControl(
        element?.dim1Disabled ?? undefined
      ),
      dim1Mandatory: new SoeCheckboxFormControl(
        element?.dim1Mandatory ?? undefined
      ),
      previousRowNbr: new SoeNumberFormControl(element?.previousRowNbr ?? 0),
      dim2Id: new SoeNumberFormControl(element?.dim2Id ?? null),
      dim2Nr: new SoeTextFormControl(element?.dim2Nr ?? ''),
      dim2Name: new SoeTextFormControl(element?.dim2Name ?? ''),
      dim2Disabled: new SoeCheckboxFormControl(
        element?.dim2Disabled ?? undefined
      ),
      dim2Mandatory: new SoeCheckboxFormControl(
        element?.dim2Mandatory ?? undefined
      ),
      dim2KeepSourceRowAccount: new SoeCheckboxFormControl(
        element?.dim2KeepSourceRowAccount ?? undefined
      ),
      dim3Id: new SoeNumberFormControl(element?.dim3Id ?? null),
      dim3Nr: new SoeTextFormControl(element?.dim3Nr ?? ''),
      dim3Name: new SoeTextFormControl(element?.dim3Name ?? ''),
      dim3Disabled: new SoeCheckboxFormControl(
        element?.dim3Disabled ?? undefined
      ),
      dim3Mandatory: new SoeCheckboxFormControl(
        element?.dim3Mandatory ?? undefined
      ),
      dim3KeepSourceRowAccount: new SoeCheckboxFormControl(
        element?.dim3KeepSourceRowAccount ?? undefined
      ),
      dim4Id: new SoeNumberFormControl(element?.dim4Id ?? null),
      dim4Nr: new SoeTextFormControl(element?.dim4Nr ?? ''),
      dim4Name: new SoeTextFormControl(element?.dim4Name ?? ''),
      dim4Disabled: new SoeCheckboxFormControl(
        element?.dim4Disabled ?? undefined
      ),
      dim4Mandatory: new SoeCheckboxFormControl(
        element?.dim4Mandatory ?? undefined
      ),
      dim4KeepSourceRowAccount: new SoeCheckboxFormControl(
        element?.dim4KeepSourceRowAccount ?? undefined
      ),
      dim5Id: new SoeNumberFormControl(element?.dim5Id ?? null),
      dim5Nr: new SoeTextFormControl(element?.dim5Nr ?? ''),
      dim5Name: new SoeTextFormControl(element?.dim5Name ?? ''),
      dim5Disabled: new SoeCheckboxFormControl(
        element?.dim5Disabled ?? undefined
      ),
      dim5Mandatory: new SoeCheckboxFormControl(
        element?.dim5Mandatory ?? undefined
      ),
      dim5KeepSourceRowAccount: new SoeCheckboxFormControl(
        element?.dim5KeepSourceRowAccount ?? undefined
      ),
      dim6Id: new SoeNumberFormControl(element?.dim6Id ?? null),
      dim6Nr: new SoeTextFormControl(element?.dim6Nr ?? ''),
      dim6Name: new SoeTextFormControl(element?.dim6Name ?? ''),
      dim6Disabled: new SoeCheckboxFormControl(
        element?.dim6Disabled ?? undefined
      ),
      dim6Mandatory: new SoeCheckboxFormControl(
        element?.dim6Mandatory ?? undefined
      ),
      dim6KeepSourceRowAccount: new SoeCheckboxFormControl(
        element?.dim6KeepSourceRowAccount ?? undefined
      ),
    });
  }

  get accountDistributionRowId(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.accountDistributionRowId;
  }
}

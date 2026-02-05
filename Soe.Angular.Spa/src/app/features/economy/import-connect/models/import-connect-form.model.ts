import { ValidationHandler } from '@shared/handlers';
import { ImportDTO, SimpleFile } from './import-connect.model';
import {
  SoeCheckboxFormControl,
  SoeDateFormControl,
  SoeFormGroup,
  SoeNumberFormControl,
  SoeSelectFormControl,
  SoeTextFormControl,
} from '@shared/extensions';

interface IImportConnectFormModel {
  element: ImportDTO | undefined;
  validationHandler: ValidationHandler;
}

export class ImportConnectFormModel extends SoeFormGroup {
  gridData: {
    files?: SimpleFile[];
  } = {};

  constructor({ element, validationHandler }: IImportConnectFormModel) {
    super(validationHandler, {
      file: new SoeTextFormControl(''),
      importId: new SoeNumberFormControl(element?.importId, {
        isIdField: true,
      }),
      actorCompanyId: new SoeNumberFormControl(element?.actorCompanyId),
      importDefinitionId: new SoeSelectFormControl(
        element?.importDefinitionId,
        {
          required: true,
        },
        'common.connect.standardimportdefinition'
      ),
      accountYearId: new SoeSelectFormControl(element?.accountYearId),
      voucherSeriesId: new SoeSelectFormControl(element?.voucherSeriesId),
      module: new SoeNumberFormControl(element?.module),
      name: new SoeTextFormControl(
        element?.name,
        {
          isNameField: true,
          required: true,
        },
        'common.name'
      ),
      headName: new SoeTextFormControl(element?.headName),
      state: new SoeNumberFormControl(element?.state),
      importHeadType: new SoeNumberFormControl(element?.importHeadType),
      type: new SoeSelectFormControl(
        element?.type,
        {
          required: true,
        },
        'common.connect.importtype'
      ),
      typeText: new SoeTextFormControl(element?.typeText, { disabled: true }),
      useAccountDistribution: new SoeCheckboxFormControl(
        element?.useAccountDistribution
      ),
      useAccountDimensions: new SoeCheckboxFormControl(
        element?.useAccountDimensions
      ),
      updateExistingInvoice: new SoeCheckboxFormControl(
        element?.updateExistingInvoice
      ),
      dim1AccountId: new SoeSelectFormControl(element?.dim1AccountId),
      dim2AccountId: new SoeSelectFormControl(element?.dim2AccountId),
      dim3AccountId: new SoeSelectFormControl(element?.dim3AccountId),
      dim4AccountId: new SoeSelectFormControl(element?.dim4AccountId),
      dim5AccountId: new SoeSelectFormControl(element?.dim5AccountId),
      dim6AccountId: new SoeSelectFormControl(element?.dim6AccountId),
      guid: new SoeTextFormControl(element?.guid),
      specialFunctionality: new SoeTextFormControl(
        element?.specialFunctionality
      ),
      isStandard: new SoeCheckboxFormControl(element?.isStandard),
      isStandardText: new SoeTextFormControl(element?.isStandardText),
      created: new SoeDateFormControl(element?.created),
      createdBy: new SoeTextFormControl(element?.createdBy),
      modified: new SoeDateFormControl(element?.modified),
      modifiedBy: new SoeTextFormControl(element?.modifiedBy),
    });
  }

  get name(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.name;
  }

  get useAccountDistribution(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.useAccountDistribution;
  }

  get useAccountDimensions(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.useAccountDimensions;
  }

  get updateExistingInvoice(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.updateExistingInvoice;
  }

  get importId(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.importId;
  }

  get accountYearId(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.accountYearId;
  }

  get voucherSeriesId(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.voucherSeriesId;
  }

  get importDefinitionId(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.importDefinitionId;
  }

  get dim1AccountId(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.dim1AccountId;
  }

  get dim2AccountId(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.dim2AccountId;
  }

  get dim3AccountId(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.dim3AccountId;
  }

  get dim4AccountId(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.dim4AccountId;
  }

  get dim5AccountId(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.dim5AccountId;
  }

  get dim6AccountId(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.dim6AccountId;
  }

  get type(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.type;
  }

  get typeText(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.typeText;
  }

  get module(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.module;
  }

  get actorCompanyId(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.actorCompanyId;
  }

  get headName(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.headName;
  }

  get state(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.state;
  }

  get importHeadType(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.importHeadType;
  }

  get specialFunctionality(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.specialFunctionality;
  }

  get isStandard(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.isStandard;
  }

  get isStandardText(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.isStandardText;
  }
}

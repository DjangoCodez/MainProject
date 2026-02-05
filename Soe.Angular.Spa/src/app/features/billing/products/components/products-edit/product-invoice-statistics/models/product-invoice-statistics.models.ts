import {
  SoeFormGroup,
  SoeSelectFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { IProductStatisticsModel } from '@shared/models/generated-interfaces/BillingModels';
import { TermGroup_ChangeStatusGridAllItemsSelection } from '@shared/models/generated-interfaces/Enumerations';

export class ProductStatisticsModel implements IProductStatisticsModel {
  productId: number;
  originType: number;
  allItemSelection: TermGroup_ChangeStatusGridAllItemsSelection;

  constructor() {
    this.productId = 0;
    this.originType = undefined!;
    this.allItemSelection = TermGroup_ChangeStatusGridAllItemsSelection.All;
  }
}

interface IProductStatisticsForm {
  validationHandler: ValidationHandler;
  element?: ProductStatisticsModel;
}

export class ProductStatisticsForm extends SoeFormGroup {
  constructor({ validationHandler, element }: IProductStatisticsForm) {
    super(validationHandler, {
      productId: new SoeTextFormControl(element?.productId || 0),
      originType: new SoeSelectFormControl(element?.originType || 0),
      allItemSelection: new SoeSelectFormControl(
        element?.allItemSelection ||
          TermGroup_ChangeStatusGridAllItemsSelection.All
      ),
    });
  }

  get productId(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.productId;
  }

  get originType(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.originType;
  }

  get allItemSelection(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.allItemSelection;
  }
}

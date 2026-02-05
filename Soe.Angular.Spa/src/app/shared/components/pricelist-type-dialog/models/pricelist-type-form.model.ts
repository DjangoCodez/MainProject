import { SoeCheckboxFormControl, SoeFormGroup, SoeSelectFormControl, SoeTextFormControl } from "@shared/extensions";
import { ValidationHandler } from "@shared/handlers";
import { IPriceListTypeDTO } from "@shared/models/generated-interfaces/PriceListTypeDTOs";

interface IPricelistTypeForm {
    validationHandler: ValidationHandler;
    element: IPriceListTypeDTO;
}
export class PricelistTypeForm extends SoeFormGroup {
    constructor({validationHandler, element}: IPricelistTypeForm) {
        super(validationHandler, {
            priceListTypeId: new SoeSelectFormControl(element?.priceListTypeId || 0, {required: true}),
            currencyId: new SoeSelectFormControl(element?.currencyId || 0),
            name: new SoeTextFormControl(element?.name || '', { required: true}),
            description: new SoeTextFormControl(element?.description || ''),
            inclusiveVat: new SoeCheckboxFormControl(element?.inclusiveVat || false),
            isProjectPriceList: new SoeCheckboxFormControl(element?.isProjectPriceList || false),
        });
    }

    get priceListTypeId(): SoeSelectFormControl {
        return <SoeSelectFormControl>this.controls.priceListTypeId;
    }

    get currencyId(): SoeSelectFormControl {
        return <SoeSelectFormControl>this.controls.currencyId;
    }

    get name(): SoeTextFormControl {
        return <SoeSelectFormControl>this.controls.name;
    }

    get description(): SoeTextFormControl {
        return <SoeSelectFormControl>this.controls.description;
    }

    get inclusiveVat(): SoeCheckboxFormControl {
        return <SoeCheckboxFormControl>this.controls.inclusiveVat;
    }

    get isProjectPriceList(): SoeCheckboxFormControl {
        return <SoeCheckboxFormControl>this.controls.isProjectPriceList;
    }
}
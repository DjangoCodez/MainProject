import { SettingMainType } from "../../../../../Util/CommonEnumerations";

export class UpdateContractPricesController {
    // Properties
    private rounding: number = 0;

    private _percent: number = 0;
    public get percent(): number {
        return this._percent;
    }
    public set percent(value: number) {
        this._percent = value;
        if (this._amount !== 0)
            this._amount = 0;
    }

    private _amount: number = 0;
    public get amount(): number {
        return this._amount;
    }
    public set amount(value: number) {
        this._amount = value;
        if (this._percent !== 100)
            this._percent = 0;
    }
    
    //@ngInject
    constructor(private $uibModalInstance) {
    }

    buttonOkClick() {
        this.$uibModalInstance.close({ rounding: this.rounding, percent: this.percent, amount: this.amount });
    }

    buttonCancelClick() {
        this.$uibModalInstance.dismiss('cancel');
    }
}
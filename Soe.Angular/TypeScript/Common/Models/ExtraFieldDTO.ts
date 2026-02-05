import { ICompTermDTO, IExtraFieldDTO, IExtraFieldGridDTO, IExtraFieldRecordDTO, IExtraFieldValueDTO } from "../../Scripts/TypeLite.Net4";
import { SettingDataType, SoeEntityState, SoeEntityType, TermGroup_ExtraFieldType, TermGroup_ExtraFieldValueType } from "../../Util/CommonEnumerations";

export class ExtraFieldDTO implements IExtraFieldDTO {
    entity: SoeEntityType;
    extraFieldId: number;
    extraFieldRecords: IExtraFieldRecordDTO[];
    extraFieldValues: IExtraFieldValueDTO[];
    text: string;
    translations: ICompTermDTO[];
    type: TermGroup_ExtraFieldType;
    connectedEntity: number;
    connectedRecordId: number;
    externalCodesString: string;
}

export class ExtraFieldValueDTO implements IExtraFieldValueDTO {
    created: Date;
    createdBy: string;
    extraFieldId: number;
    extraFieldValueId: number;
    modified: Date;
    modifiedBy: string;
    sort: number;
    state: SoeEntityState;
    type: TermGroup_ExtraFieldValueType;
    value: string;
}

export class ExtraFieldGridDTO implements IExtraFieldGridDTO {
    accountDimId: number;
    accountDimName: string;
    extraFieldId: number;
    extraFieldValues: IExtraFieldValueDTO[];
    hasRecords: boolean;
    text: string;
    type: number;

    // Extensions
    typeName: string;

    get isText(): boolean {
        return this.type === TermGroup_ExtraFieldType.FreeText;
    }
    get isInteger(): boolean {
        return this.type === TermGroup_ExtraFieldType.Integer;
    }
    get isDecimal(): boolean {
        return this.type === TermGroup_ExtraFieldType.Decimal;
    }
    get isYesNo(): boolean {
        return this.type === TermGroup_ExtraFieldType.YesNo;
    }
    get isCheckbox(): boolean {
        return this.type === TermGroup_ExtraFieldType.Checkbox;
    }
    get isDate(): boolean {
        return this.type === TermGroup_ExtraFieldType.Date;
    }
    get isSingleChoice(): boolean {
        return this.type === TermGroup_ExtraFieldType.SingleChoice;
    }
    get isMultiChoice(): boolean {
        return this.type === TermGroup_ExtraFieldType.MultiChoice;
    }
}

export class ExtraFieldRecordDTO implements IExtraFieldRecordDTO {
    boolData: boolean;
    comment: string;
    dataTypeId: number;
    dateData: Date;
    decimalData: number;
    extraFieldId: number;
    extraFieldRecordId: number;
    extraFieldText: string;
    extraFieldType: number;
    extraFieldValues: IExtraFieldValueDTO[];
    _intData: number;
    recordId: number;
    strData: string;
    _value: string;

    toPlainObject(): object {
        return {
            ...this,
            intData: this.intData,
            value: this.value
        };
    }

    get value(): string {
        let value = "";

        switch (this.dataTypeId) {
            case SettingDataType.Boolean:
                if (this.boolData !== undefined && this.boolData !== null)
                    value = this.boolData.toString();
                break;
            case SettingDataType.Decimal:
                if (this.decimalData !== undefined && this.decimalData !== null)
                    value = this.decimalData.toFixed(4);
                break;
            case SettingDataType.Integer:
                if (this.intData !== undefined && this.intData !== null)
                    value = this.intData.toString();
                break;
            case SettingDataType.String:
                if (this.strData)
                    value = this.strData.toString();
                break;
            case SettingDataType.Date:
                if (this.dateData)
                    value = this.dateData.toLocaleDateString();
                break;
            default:
                break;
        }

        return value;
    }

    set value(value: string) {
        this._value = value;
    }

    get intData() {
        return this._intData;
    }
    set intData(value) {
        if (!value)
            value = 0;

        if (!Number.isInteger(value))
            this._intData = Number.parseInt(value.toString());
        else
            this._intData = value;
    }

    // Extensions
    isModified: boolean;
}
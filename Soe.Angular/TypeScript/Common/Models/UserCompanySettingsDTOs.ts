import { IUserCompanySettingEditDTO } from "../../Scripts/TypeLite.Net4";
import { SettingDataType, LicenseSettingType } from "../../Util/CommonEnumerations";
import { CalendarUtility } from "../../Util/CalendarUtility";
import { SmallGenericType } from "./SmallGenericType";

export class UserCompanySettingEditDTO implements IUserCompanySettingEditDTO {
    booleanValue: boolean;
    dataType: SettingDataType;
    dateValue: Date;
    decimalValue: number;
    groupLevel1: string;
    groupLevel2: string;
    groupLevel3: string;
    integerValue: number;
    isModified: boolean;
    name: string;
    options: SmallGenericType[];
    settingMainType: any;
    settingTypeId: number;
    stringValue: string;
    userCompanySettingId: number;
    visibleOnlyForSupportAdmin: boolean;

    // Extensions
    isEditing: boolean;

    public fixDates() {
        this.dateValue = CalendarUtility.convertToDate(this.dateValue);
    }

    public get value() {
        if (!this.userCompanySettingId && !this.isModified && this.dataType !== SettingDataType.Boolean)
            return '';

        switch (this.dataType) {
            case SettingDataType.String:
                return this.stringValue || '';
            case SettingDataType.Integer:
                if (this.options) {
                    if (typeof this.integerValue === 'undefined')
                        return '';
                    else
                        return this.options.find(o => o.id === (this.integerValue || 0))?.name;
                } else {
                    return this.integerValue || 0;
                }
            case SettingDataType.Decimal:
                return this.decimalValue || 0;
            case SettingDataType.Boolean:
                return this.booleanValue ? 'fa-check-square' : 'fa-square';
            case SettingDataType.Date:
                return this.dateValue ? this.dateValue.toFormattedDate() : '';
            case SettingDataType.Time:
                return this.dateValue ? this.dateValue.toFormattedTime() : '';
        }

        return '';
    }

    public get icon(): string {
        switch (this.dataType) {
            case SettingDataType.String:
                return "fal fa-font";
            case SettingDataType.Integer:
                return this.options ? "fal fa-list-ul" : "fal fa-hashtag";
            case SettingDataType.Decimal:
                return "fal fa-hashtag";
            case SettingDataType.Boolean:
                return "fal fa-check";
            case SettingDataType.Date:
                return "fal fa-calendar-alt";
            case SettingDataType.Time:
                return "fal fa-clock";
        }

        return '';
    }
}
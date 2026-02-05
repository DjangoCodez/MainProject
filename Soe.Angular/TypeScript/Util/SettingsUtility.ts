import { UserSettingType, CompanySettingType, LicenseSettingType } from "../Util/CommonEnumerations";

export class SettingsUtility {

    // User setting
    public static getStringUserSetting(settings: any[], type: UserSettingType, defaultValue: string = ''): string {
        return settings[type] !== undefined ? settings[type] : defaultValue;
    }

    public static getIntUserSetting(settings: any[], type: UserSettingType, defaultValue: number = 0, allowZero: boolean = true): number {
        var value: number = settings[type] !== undefined ? settings[type] : defaultValue;

        if (value === 0 && !allowZero)
            value = defaultValue;

        return value;
    }

    public static getBoolUserSetting(settings: any[], type: UserSettingType, defaultValue: boolean = false): boolean {
        return settings[type] !== undefined ? settings[type] : defaultValue;
    }

    // Company setting
    public static getStringCompanySetting(settings: any[], type: CompanySettingType, defaultValue: string = ''): string {
        return settings[type] !== undefined ? settings[type] : defaultValue;
    }

    public static getIntCompanySetting(settings: any[], type: CompanySettingType, defaultValue: number = 0, allowZero: boolean = true): number {
        var value: number = settings[type] !== undefined ? settings[type] : defaultValue;

        if (value === 0 && !allowZero)
            value = defaultValue;

        return value;
    }

    public static getBoolCompanySetting(settings: any[], type: CompanySettingType, defaultValue: boolean = false): boolean {
        return settings[type] !== undefined ? settings[type] : defaultValue;
    }

    // License setting
    public static getStringLicenseSetting(settings: any[], type: LicenseSettingType, defaultValue: string = ''): string {
        return settings[type] !== undefined ? settings[type] : defaultValue;
    }

    public static getIntLicenseSetting(settings: any[], type: LicenseSettingType, defaultValue: number = 0, allowZero: boolean = true): number {
        var value: number = settings[type] !== undefined ? settings[type] : defaultValue;

        if (value === 0 && !allowZero)
            value = defaultValue;

        return value;
    }

    public static getBoolLicenseSetting(settings: any[], type: LicenseSettingType, defaultValue: boolean = false): boolean {
        return settings[type] !== undefined ? settings[type] : defaultValue;
    }
}

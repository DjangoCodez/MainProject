import {
  CompanySettingType,
  LicenseSettingType,
  UserSettingType,
} from '../models/generated-interfaces/Enumerations';

export type UserCompanySettingCollection = { [type: number]: any };

export class SettingsUtil {
  // User setting
  public static getStringUserSetting(
    settings: UserCompanySettingCollection | any[],
    type: UserSettingType,
    defaultValue = ''
  ): string {
    return settings[type] ?? defaultValue;
  }

  public static getIntUserSetting(
    settings: UserCompanySettingCollection | any[],
    type: UserSettingType,
    defaultValue = 0,
    allowZero = true
  ): number {
    let value: number = settings[type] ?? defaultValue;

    if (value === 0 && !allowZero) value = defaultValue;

    return value;
  }

  public static getBoolUserSetting(
    settings: UserCompanySettingCollection | any[],
    type: UserSettingType,
    defaultValue = false
  ): boolean {
    return settings[type] ?? defaultValue;
  }

  // Company setting
  public static getStringCompanySetting(
    settings: UserCompanySettingCollection | any[],
    type: CompanySettingType,
    defaultValue = ''
  ): string {
    return settings[type] ?? defaultValue;
  }

  public static getIntCompanySetting(
    settings: UserCompanySettingCollection | any[],
    type: CompanySettingType,
    defaultValue = 0,
    allowZero = true
  ): number {
    let value: number = settings[type] ?? defaultValue;

    if (value === 0 && !allowZero) value = defaultValue;

    return value;
  }

  public static getBoolCompanySetting(
    settings: UserCompanySettingCollection | any[],
    type: CompanySettingType,
    defaultValue = false
  ): boolean {
    return settings[type] ?? defaultValue;
  }

  // License setting
  public static getStringLicenseSetting(
    settings: UserCompanySettingCollection | any[],
    type: LicenseSettingType,
    defaultValue = ''
  ): string {
    return settings[type] ?? defaultValue;
  }

  public static getIntLicenseSetting(
    settings: UserCompanySettingCollection | any[],
    type: LicenseSettingType,
    defaultValue = 0,
    allowZero = true
  ): number {
    let value: number = settings[type] ?? defaultValue;

    if (value === 0 && !allowZero) value = defaultValue;

    return value;
  }

  public static getBoolLicenseSetting(
    settings: UserCompanySettingCollection | any[],
    type: LicenseSettingType,
    defaultValue = false
  ): boolean {
    return settings[type] ?? defaultValue;
  }
}

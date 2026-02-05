import { ExtendedWindow } from '@core/services/authentication/authentication.service';
import {
  Feature,
  TermGroup_Country,
  TermGroup_Languages,
} from '@shared/models/generated-interfaces/Enumerations';
import { SoeLogType } from '@shared/models/generated-interfaces/Enumerations';
import { DateUtil } from './date-util';
import { isEqual } from 'lodash';
import { Locale } from 'date-fns';
import { da, enUS, fi, nb, sv } from 'date-fns/locale';

const extendedWindow = window as ExtendedWindow;
const soeConfig = extendedWindow.soeConfig;

export class SoeConfigUtil {
  // Please don't add more soeConfig properties here.
  // Prefer handling page specific params via url props.

  // Terms
  static get termVersionNr(): string {
    return (
      soeConfig?.termVersionNr || DateUtil.format(new Date(), 'yyyy.MM.dd.HHmm')
    );
  }

  static get termVersionNrInt(): number {
    let parts = this.termVersionNr.split('.');
    if (parts.length !== 4)
      parts = DateUtil.format(new Date(), 'yyyy.MM.dd.HHmm').split('.');

    return parseInt(
      `${parts[0]}${parts[1]}${parts[2]}${parts[3].padStart(4, '0')}`,
      10
    );
  }

  static get fieldSettingsType(): number {
    return soeConfig?.fieldSettingsType;
  }

  // Country/Language
  static get sysCountryId(): number {
    return soeConfig?.sysCountryId || TermGroup_Country.SE;
  }

  static get language(): string {
    return soeConfig?.language || 'sv-SE';
  }

  static get languageCode(): string {
    return this.language.substring(0, 2).toLowerCase();
  }

  static get languageId(): number {
    const lang: string = this.language.substring(0, 2).toLowerCase();
    if (lang === 'sv') return TermGroup_Languages.Swedish;
    else if (lang === 'en') return TermGroup_Languages.English;
    else if (lang === 'fi') return TermGroup_Languages.Finnish;
    else if (lang === 'no') return TermGroup_Languages.Norwegian;
    else if (lang === 'da') return TermGroup_Languages.Danish;

    return TermGroup_Languages.Swedish;
  }

  static get bsLanguage(): string {
    const langId: TermGroup_Languages = this.languageId;
    if (langId === TermGroup_Languages.Swedish) return 'sv';
    else if (langId === TermGroup_Languages.English) return 'engb';
    else if (langId === TermGroup_Languages.Finnish) return 'fi';
    else if (langId === TermGroup_Languages.Norwegian) return 'nb';
    else if (langId === TermGroup_Languages.Danish) return 'da';

    return 'sv';
  }

  static get dateFnsLocale(): Locale {
    switch (this.languageId) {
      case TermGroup_Languages.Swedish:
        return sv;
      case TermGroup_Languages.English:
        return enUS;
      case TermGroup_Languages.Finnish:
        return fi;
      case TermGroup_Languages.Norwegian:
        return nb;
      case TermGroup_Languages.Danish:
        return da;
      default:
        return sv;
    }
  }

  static get firstDayOfWeek(): number {
    return 1; // TODO: Now always return monday as first day of week
  }

  // License/Company/Role/User
  static get licenseId(): number {
    return soeConfig?.licenseId || 0;
  }

  static get licenseNr(): string {
    return soeConfig?.licenseNr || '';
  }

  static get actorCompanyId(): number {
    return soeConfig?.actorCompanyId || 0;
  }

  static get roleId(): number {
    return soeConfig?.roleId || 0;
  }

  static get userId(): number {
    return soeConfig?.userId || 0;
  }

  static get supportUserId(): number {
    return soeConfig?.supportUserId ?? 0;
  }

  static get loginName(): string {
    return soeConfig?.loginName || '';
  }

  static get userToken(): string {
    return soeConfig?.token || '';
  }

  static get employeeId(): number {
    return soeConfig?.employeeId || 0;
  }

  static get type(): string {
    return soeConfig?.type || '';
  }

  static get feature(): Feature {
    return <Feature>soeConfig?.feature || Feature.None;
  }

  static getCustomValue(name: string): string {
    return soeConfig?.[name] || '';
  }

  // Support admin
  public static get isSupportAdmin(): boolean {
    return soeConfig?.isSupportAdmin || false;
  }

  public static get isSupportSuperAdmin(): boolean {
    return soeConfig?.isSupportSuperAdmin || false;
  }

  public static get supportLogType() {
    return soeConfig.supportLogType as SoeLogType;
  }

  public static set supportLogType(value: SoeLogType) {
    soeConfig.supportLogType = value;
  }

  public static cloneDTO(dto: any) {
    return JSON.parse(JSON.stringify(dto));
  }

  public static cloneDTOs(dtos: any[]) {
    const clones: any[] = [];
    if (dtos) {
      dtos.forEach(dto => {
        clones.push(this.cloneDTO(dto));
      });
    }

    return clones;
  }

  public static diffDTO(
    original: any,
    modified: any,
    skipKeys: string[] = [],
    toLower = false
  ) {
    const diff: any = {};
    for (const key in original) {
      if (!skipKeys.includes(key) && !isEqual(original[key], modified[key])) {
        diff[toLower ? key.toLowerCase() : key] =
          modified[key] == undefined ? null : modified[key];
      }
    }

    for (const key in modified) {
      if (
        !skipKeys.includes(key) &&
        !Object.prototype.hasOwnProperty.call(original, key)
      ) {
        diff[toLower ? key.toLowerCase() : key] =
          modified[key] == undefined ? null : modified[key];
      }
    }
    return diff;
  }

  public static toDTO(item: any, skipKeys: string[] = [], toLower = false) {
    const dto: any = {};
    for (const key in item) {
      dto[toLower ? key.toLowerCase() : key] = item[key];
    }
    return dto;
  }
}

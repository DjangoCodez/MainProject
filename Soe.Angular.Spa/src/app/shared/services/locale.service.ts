import { Injectable } from '@angular/core';
import { registerLocaleData } from '@angular/common';
import localeSv from '@angular/common/locales/sv';
import localeSvExtra from '@angular/common/locales/extra/sv';
import localeEn from '@angular/common/locales/en';
import localeEnExtra from '@angular/common/locales/extra/en';
import localeFi from '@angular/common/locales/fi';
import localeFiExtra from '@angular/common/locales/extra/fi';
import localeNo from '@angular/common/locales/no';
import localeNoExtra from '@angular/common/locales/extra/no';
import localeDa from '@angular/common/locales/da';
import localeDaExtra from '@angular/common/locales/extra/da';
import { SoeConfigUtil } from '@shared/util/soeconfig-util';

@Injectable({
  providedIn: 'root',
})
export class LocaleService {
  get locale(): string {
    return this.registerCulture();
  }

  public registerCulture(): string {
    let locale = SoeConfigUtil.language;

    switch (SoeConfigUtil.language) {
      case 'sv-SE': {
        registerLocaleData(localeSv, SoeConfigUtil.language, localeSvExtra);
        break;
      }
      case 'en-US': {
        registerLocaleData(localeEn, SoeConfigUtil.language, localeEnExtra);
        break;
      }
      case 'fi-FI': {
        registerLocaleData(localeFi, SoeConfigUtil.language, localeFiExtra);
        break;
      }
      case 'nb-NO': {
        registerLocaleData(localeNo, SoeConfigUtil.language, localeNoExtra);
        break;
      }
      case 'da-DK': {
        registerLocaleData(localeDa, SoeConfigUtil.language, localeDaExtra);
        break;
      }
      default: {
        locale = 'sv-SE';
        registerLocaleData(localeSv, 'sv-SE', localeSvExtra);
        break;
      }
    }

    return locale;
  }
}

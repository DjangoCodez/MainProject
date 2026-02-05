import { ExtendedWindow } from '@core/services/authentication/authentication.service';

const extendedWindow = window as ExtendedWindow;
const ajsLegacy = extendedWindow.ajsLegacy;

export class AjsLegacyUtil {
  static get ajsLegacy() {
    return ajsLegacy;
  }
}

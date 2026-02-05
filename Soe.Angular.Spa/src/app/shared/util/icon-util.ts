import {
  IconName,
  IconPrefix,
  IconProp,
} from '@fortawesome/fontawesome-svg-core';

export class IconUtil {
  public static createIcon(prefix?: IconPrefix, name?: IconName): IconProp {
    return [prefix || 'fal', name || 'starfighter-twin-ion-engine-advanced'];
  }
}

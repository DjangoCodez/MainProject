import { IconName, IconPrefix } from '@fortawesome/fontawesome-svg-core';
import { FieldOrPredicate } from '../util/column-util';

export interface IIconButtonConfiguration<T> {
  iconPrefix: IconPrefix;
  iconName: IconName;
  iconClass?: string;
  onClick: (data: any) => void;
  tooltip?: string;
  show?: FieldOrPredicate<T>;
}

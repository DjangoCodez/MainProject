import { SoeFormGroup } from '@shared/extensions';
import { SmallGenericType } from '@shared/models/generic-type.model';

export interface Tab<T extends SoeFormGroup> {
  id?: number;
  label: string;
  form: T;
  records: SmallGenericType[];
  selectedRecord?: SmallGenericType;
  additionalProps?: any;
}

import { SoeFormGroup } from '@shared/extensions';

export interface IEditForm {
  // Flags
  inProgress: boolean;

  // Form
  form: SoeFormGroup;
}

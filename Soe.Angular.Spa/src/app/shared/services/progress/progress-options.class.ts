import { BackendResponse } from '@shared/interfaces/backend-response.interface';

export class ProgressOptions {
  showDialog?: boolean;
  showDialogDelay?: number;
  showDialogOnComplete?: boolean;
  showDialogOnError?: boolean;
  showToast?: boolean;
  showToastOnComplete?: boolean;
  showToastOnError?: boolean;
  failIfNoObjectsAffected?: boolean;
  title?: string;
  message?: string;
  keepExistingDialog?: boolean;
  callback?: (val: BackendResponse) => any;
  errorCallback?: (val: BackendResponse) => void;
}

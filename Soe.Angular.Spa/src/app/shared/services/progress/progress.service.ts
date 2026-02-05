import { Injectable, signal } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { MessageboxComponent } from '@ui/dialog/messagebox/messagebox.component';
import { MessageboxData } from '@ui/dialog/models/messagebox';
import { MessageboxService } from '@ui/dialog/services/messagebox.service';
import { ToasterService } from '@ui/toaster/services/toaster.service';
import { ProgressStatus, ProgressType } from './progress-enums';
import { ProgressOptions } from './progress-options.class';
import { MatDialogRef } from '@angular/material/dialog';
import { SoeConfigUtil } from '@shared/util/soeconfig-util';

@Injectable({
  providedIn: 'root',
})
export class ProgressService {
  private _inProgress = signal(false);
  private _hasError = signal(false);

  private dialogRef?: MatDialogRef<MessageboxComponent<MessageboxData>>;
  private loadCounter = 0; // Keeps the progress dialog open on multiple loads until all are complete

  get inProgress() {
    return this._inProgress;
  }

  get hasError() {
    return this._hasError;
  }

  constructor(
    private messageboxService: MessageboxService,
    private toasterService: ToasterService,
    private translate: TranslateService
  ) {}

  load(options?: ProgressOptions) {
    this.loadCounter++;
    if (this.loadCounter === 1) {
      // Only open dialog on first load
      if (options?.showDialogDelay) {
        // If delay is specified, show dialog after delay
        // Used to prevent flickering dialog on fast operations
        setTimeout(() => {
          if (this.loadCounter > 0) {
            this.showLoadProgress(options);
          }
        }, options.showDialogDelay);
      } else {
        this.showLoadProgress(options);
      }
    }
  }

  private showLoadProgress(options?: ProgressOptions) {
    this.showProgress(ProgressType.Load, ProgressStatus.Progress, options);
  }

  loadComplete(options?: ProgressOptions) {
    this.loadCounter--;
    if (this.loadCounter === 0) {
      // Only close dialog on last load complete
      this.showProgress(ProgressType.Load, ProgressStatus.Complete, options);
    }
  }

  loadError(options?: ProgressOptions) {
    this.resetLoadCounter();
    // Always show dialog on error
    this.showProgress(ProgressType.Load, ProgressStatus.Error, options);
  }

  save(options?: ProgressOptions) {
    if (options?.showDialogDelay) {
      // If delay is specified, show dialog after delay
      // Used to prevent flickering dialog on fast operations
      setTimeout(() => {
        this.showSaveProgress(options);
      }, options.showDialogDelay);
    } else {
      this.showSaveProgress(options);
    }
  }

  private showSaveProgress(options?: ProgressOptions) {
    this.showProgress(ProgressType.Save, ProgressStatus.Progress, options);
  }

  saveComplete(options?: ProgressOptions, objectsAffected?: number) {
    if (objectsAffected === 0 && options?.failIfNoObjectsAffected) {
      const message = this.translate.instant('core.noobjectsaffected');
      this.showProgress(ProgressType.Save, ProgressStatus.Error, {
        ...options,
        message,
      });
    } else {
      this.showProgress(ProgressType.Save, ProgressStatus.Complete, options);
    }
  }

  saveError(options?: ProgressOptions) {
    this.showProgress(ProgressType.Save, ProgressStatus.Error, options);
  }

  delete(options?: ProgressOptions) {
    if (options?.showDialogDelay) {
      // If delay is specified, show dialog after delay
      // Used to prevent flickering dialog on fast operations
      setTimeout(() => {
        this.showDeleteProgress(options);
      }, options.showDialogDelay);
    } else {
      this.showDeleteProgress(options);
    }
  }

  private showDeleteProgress(options?: ProgressOptions) {
    this.showProgress(ProgressType.Delete, ProgressStatus.Progress, options);
  }

  deleteComplete(options?: ProgressOptions) {
    this.showProgress(ProgressType.Delete, ProgressStatus.Complete, options);
  }

  deleteError(options?: ProgressOptions) {
    this.showProgress(ProgressType.Delete, ProgressStatus.Error, options);
  }

  work(options?: ProgressOptions) {
    if (options?.showDialogDelay) {
      // If delay is specified, show dialog after delay
      // Used to prevent flickering dialog on fast operations
      setTimeout(() => {
        this.showWorkProgress(options);
      }, options.showDialogDelay);
    } else {
      this.showWorkProgress(options);
    }
  }

  private showWorkProgress(options?: ProgressOptions) {
    this.showProgress(ProgressType.Work, ProgressStatus.Progress, options);
  }

  workComplete(options?: ProgressOptions) {
    this.showProgress(ProgressType.Work, ProgressStatus.Complete, options);
  }

  workError(options?: ProgressOptions) {
    this.showProgress(ProgressType.Work, ProgressStatus.Error, options);
  }

  private showProgress(
    type: ProgressType,
    status: ProgressStatus,
    options?: ProgressOptions
  ) {
    this._inProgress.set(status === ProgressStatus.Progress);
    this._hasError.set(status === ProgressStatus.Error);

    // Get default options for specified progress type
    const defaultOptions: Partial<ProgressOptions> =
      this.getDefaultOptions(type);

    // Merge with specified custom options (if any)
    if (options && Object.keys(options).length !== 0)
      Object.assign(defaultOptions, defaultOptions, options);

    // Check if dialog should be shown
    const showDialogProgress =
      status === ProgressStatus.Progress && defaultOptions.showDialog;
    const showDialogComplete =
      status === ProgressStatus.Complete && defaultOptions.showDialogOnComplete;
    const showDialogError =
      status === ProgressStatus.Error && defaultOptions.showDialogOnError;

    if (!options?.keepExistingDialog || status === ProgressStatus.Complete)
      this.hideDialog();

    // Check if toast should be shown
    const showToastProgress =
      status === ProgressStatus.Progress && defaultOptions.showToast;
    const showToastComplete =
      status === ProgressStatus.Complete && defaultOptions.showToastOnComplete;
    const showToastError =
      status === ProgressStatus.Error && defaultOptions.showToastOnError;

    if (
      showDialogProgress ||
      showDialogComplete ||
      showDialogError ||
      showToastProgress ||
      showToastComplete ||
      showToastError
    ) {
      // Get default title and message, or custom if specified
      const texts = this.getTitleAndMessage(type, status, options);

      // Show dialog
      if (showDialogProgress) {
        this.dialogRef = this.messageboxService.progress(
          texts.title,
          texts.message,
          { enableCloseProgress: SoeConfigUtil.isSupportAdmin }
        );
      } else if (showDialogComplete) {
        this.dialogRef = this.messageboxService.information(
          texts.title,
          texts.message
        );
      } else if (showDialogError) {
        this.dialogRef = this.messageboxService.error(
          texts.title,
          texts.message
        );
      }

      // Show toast
      if (showToastProgress) {
        this.toasterService.info(texts.message, texts.title);
      } else if (showToastComplete) {
        this.toasterService.success(texts.message, texts.title);
      } else if (showToastError) {
        this.toasterService.error(texts.message, texts.title);
      }
    }
  }

  private getDefaultOptions(type: ProgressType): Partial<ProgressOptions> {
    const defaultOptions: Partial<ProgressOptions> = {};
    switch (type) {
      case ProgressType.Load:
        defaultOptions.showDialog = true;
        defaultOptions.showDialogOnError = true;
        break;
      case ProgressType.Save:
        defaultOptions.showDialog = true;
        defaultOptions.showDialogOnError = true;
        defaultOptions.showToastOnComplete = true;
        break;
      case ProgressType.Delete:
        defaultOptions.showDialog = true;
        defaultOptions.showDialogOnError = true;
        defaultOptions.showToastOnComplete = true;
        break;
      case ProgressType.Work:
        defaultOptions.showDialog = true;
        defaultOptions.showDialogOnError = true;
        defaultOptions.showToastOnComplete = true;
        break;
    }

    return defaultOptions;
  }

  private getTitleAndMessage(
    type: ProgressType,
    status: ProgressStatus,
    customOptions?: ProgressOptions
  ): { title: string; message: string } {
    // If both title and message is specified, return them directly
    let title = customOptions?.title;
    let message = customOptions?.message;
    if (title && message) return { title, message };

    // Get default title and message for specified type and status
    let titleKey = '';
    let messageKey = '';
    switch (type) {
      case ProgressType.Load:
        titleKey = 'common.status';
        if (status === ProgressStatus.Progress) messageKey = 'core.loading';
        else if (status === ProgressStatus.Complete) messageKey = 'core.loaded';
        else messageKey = 'core.loadfailed';
        break;
      case ProgressType.Save:
        titleKey = 'common.status';
        if (status === ProgressStatus.Progress) messageKey = 'core.saving';
        else if (status === ProgressStatus.Complete) messageKey = 'core.saved';
        else messageKey = 'core.savefailed';
        break;
      case ProgressType.Delete:
        titleKey = 'common.status';
        if (status === ProgressStatus.Progress) messageKey = 'core.deleting';
        else if (status === ProgressStatus.Complete)
          messageKey = 'core.deleted';
        else messageKey = 'core.deletefailed';
        break;
      case ProgressType.Work:
        titleKey = 'common.status';
        if (status === ProgressStatus.Progress) messageKey = 'core.working';
        else if (status === ProgressStatus.Complete) messageKey = 'core.worked';
        else messageKey = 'core.workfailed';
        break;
    }
    if (!title && titleKey) title = this.translate.instant(titleKey);
    if (!message && messageKey) message = this.translate.instant(messageKey);

    return { title: title || '', message: message || '' };
  }

  public hideDialog() {
    if (this.dialogRef) this.dialogRef.close();
  }

  resetLoadCounter() {
    this.loadCounter = 0;
  }
}

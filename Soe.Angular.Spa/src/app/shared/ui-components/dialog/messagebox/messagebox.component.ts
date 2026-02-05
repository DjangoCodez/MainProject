import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { StringUtil } from '@shared/util/string-util';
import { focusOnElementBySelector } from '@shared/util/focus-util';
import { IconPrefix, IconName } from '@fortawesome/pro-regular-svg-icons';
import { AnimationProp } from '@fortawesome/angular-fontawesome';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import {
  MatDialogRef,
  MAT_DIALOG_DATA,
  MatDialogModule,
} from '@angular/material/dialog';
import { DragDropModule } from '@angular/cdk/drag-drop';
import {
  IMessageboxComponentResponse,
  MessageboxData,
} from '../models/messagebox';
import { take } from 'rxjs';
import { ButtonComponent } from '@ui/button/button/button.component'
import { DatepickerComponent } from '@ui/forms/datepicker/datepicker.component'
import { IconModule } from '@ui/icon/icon.module'
import { NumberboxComponent } from '@ui/forms/numberbox/numberbox.component';

@Component({
  selector: 'soe-messagebox',
  imports: [
    CommonModule,
    FormsModule,
    MatDialogModule,
    DragDropModule,
    ButtonComponent,
    IconModule,
    TranslatePipe,
    DatepickerComponent,
    NumberboxComponent,
  ],
  templateUrl: './messagebox.component.html',
  styleUrls: ['./messagebox.component.scss'],
})
export class MessageboxComponent<T extends MessageboxData> {
  data: T = inject(MAT_DIALOG_DATA);
  dialogRef = inject(MatDialogRef);

  // Header
  headerClass = '';

  // Hidden text
  showHiddenText = false;

  // Icons
  iconPrefix: IconPrefix = 'fal';
  iconName!: IconName;
  iconAnimation?: AnimationProp;
  noIcon = false;

  // Buttons
  showButtonOk = false;
  showButtonCancel = false;
  showButtonYes = false;
  showButtonNo = false;

  buttonOkLabel = '';
  buttonYesLabel = '';
  buttonNoLabel = '';
  buttonCancelLabel = '';

  constructor(private translationService: TranslateService) {
    this.data.size = this.data.size || 'md';
    this.dialogRef.addPanelClass(`size-${this.data.size}`);

    if (this.data.showInputText)
      this.data.inputTextRows = this.data.inputTextRows || 1;

    // Format HTML text
    if (this.data.text) {
      this.data.text = StringUtil.newLineToBr(
        this.translationService.instant(this.data.text)
      );
    }

    this.setupHeaderAndIcon();
    this.setupButtons();
  }

  // SETUP

  private setupHeaderAndIcon() {
    switch (this.data.type) {
      case 'information':
        this.headerClass = 'information';
        this.iconName = 'circle-info';
        if (!this.data.iconClass) this.data.iconClass = 'information-color';
        break;
      case 'warning':
        this.headerClass = 'warning';
        this.iconName = 'circle-exclamation';
        if (!this.data.iconClass) this.data.iconClass = 'warning-color';
        break;
      case 'error':
        this.headerClass = 'error';
        this.iconName = 'triangle-exclamation';
        if (!this.data.iconClass) this.data.iconClass = 'error-color';
        break;
      case 'success':
        this.headerClass = 'success';
        this.iconName = 'circle-check';
        if (!this.data.iconClass) this.data.iconClass = 'success-color';
        break;
      case 'question':
      case 'questionAbort':
        this.headerClass = 'information';
        this.iconName = 'circle-question';
        if (!this.data.iconClass) this.data.iconClass = 'information-color';
        break;
      case 'forbidden':
        this.headerClass = 'error';
        this.iconName = 'ban';
        if (!this.data.iconClass) this.data.iconClass = 'error-color';
        break;
      case 'progress':
        this.iconPrefix = 'far';
        this.iconName = 'spinner';
        if (!this.data.iconClass) this.data.iconClass = 'text-color';
        this.iconAnimation = 'spin';
        break;
      case 'custom':
        break;
      default:
        this.noIcon = true;
        break;
    }
    if (this.data.customIconName) this.iconName = this.data.customIconName;
    if (!this.data.iconClass) this.data.iconClass = 'text-color';
  }

  private setupButtons() {
    const keys: string[] = [];

    if (!this.data.buttonOkLabelKey) this.data.buttonOkLabelKey = 'core.ok';
    if (!this.data.buttonYesLabelKey) this.data.buttonYesLabelKey = 'core.yes';
    if (!this.data.buttonNoLabelKey) this.data.buttonNoLabelKey = 'core.no';
    if (!this.data.buttonCancelLabelKey)
      this.data.buttonCancelLabelKey = 'core.cancel';

    keys.push(this.data.buttonOkLabelKey);
    keys.push(this.data.buttonYesLabelKey);
    keys.push(this.data.buttonNoLabelKey);
    keys.push(this.data.buttonCancelLabelKey);

    this.translationService
      .get(keys)
      .pipe(take(1))
      .subscribe(terms => {
        this.buttonOkLabel = terms[this.data.buttonOkLabelKey];
        this.buttonYesLabel = terms[this.data.buttonYesLabelKey];
        this.buttonNoLabel = terms[this.data.buttonNoLabelKey];
        this.buttonCancelLabel = terms[this.data.buttonCancelLabelKey];
      });

    let focusButtonId = '';
    switch (this.data.buttons) {
      case 'ok':
        this.showButtonOk = true;
        focusButtonId = '#button-ok';
        break;
      case 'okCancel':
        this.showButtonOk = true;
        this.showButtonCancel = true;
        focusButtonId = '#button-cancel';
        break;
      case 'yesNo':
        this.showButtonYes = true;
        this.showButtonNo = true;
        focusButtonId = '#button-no';
        break;
      case 'yesNoCancel':
        this.showButtonYes = true;
        this.showButtonNo = true;
        this.showButtonCancel = true;
        focusButtonId = '#button-cancel';
        break;
    }
    this.closeOnEscapeOrBackdropClick(
      this.showButtonCancel ||
        this.data.buttons === 'ok' ||
        (this.data.type === 'progress' && this.data.enableCloseProgress)
    );
    if (focusButtonId) focusOnElementBySelector(focusButtonId);
  }

  // EVENTS

  private closeOnEscapeOrBackdropClick(enable: boolean) {
    this.dialogRef.disableClose = !enable;
    if (enable) {
      // This will make the escape key return a result
      this.dialogRef.afterClosed().subscribe(result => {
        if (typeof result === 'undefined') this.closeDialog();
      });
      // This will make the backdrop click return a result
      this.dialogRef.backdropClick().subscribe(result => {
        this.closeDialog();
      });
    }
  }

  onNumberChanged(number: number) {
    this.data.inputNumberValue = number;
  }

  onNumberKeyUp(event: KeyboardEvent) {
    if (event.key === 'Enter' || event.key === 'NumpadEnter')
      this.enterPressed();
  }

  onDateChanged(date?: Date) {
    this.data.inputDateValue = date;
  }

  enterPressed() {
    if (this.showButtonOk || this.showButtonYes) this.closeDialog(true);
  }

  private _doubleClickCount = 0;
  iconDoubleClick() {
    if (!this.data.hiddenText) return;

    this._doubleClickCount++;
    if (this._doubleClickCount >= 2) {
      this._doubleClickCount = 0;
      this.showHiddenText = true;
      this.logHiddenText();
    }
  }

  private logHiddenText() {
    try {
      const json = JSON.parse(this.data.hiddenText);
      if (json) console.log(json);
    } catch (e) {
      console.log(this.data.hiddenText);
    }
  }

  private setResponseValues(result?: boolean): IMessageboxComponentResponse {
    return {
      result: result,
      textValue: this.data.inputTextValue,
      numberValue: this.data.inputNumberValue,
      checkboxValue: this.data.inputCheckboxValue,
      dateValue: this.data.inputDateValue,
      data: this.data,
    };
  }

  closeDialog(response?: boolean): void {
    this.dialogRef.close(this.setResponseValues(response));
  }
}

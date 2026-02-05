import { inject, Injectable } from '@angular/core';
import { MatDialog, MatDialogRef } from '@angular/material/dialog';
import { MessageboxComponent } from '../messagebox/messagebox.component';
import { MessageboxData } from '../models/messagebox';

@Injectable({
  providedIn: 'root',
})
export class MessageboxService {
  dialog = inject(MatDialog);

  information(
    title: string,
    text: string,
    data?: Partial<MessageboxData>
  ): MatDialogRef<MessageboxComponent<MessageboxData>> {
    if (!data) data = {};
    if (!data.type) data.type = 'information';
    data.title = title;
    data.text = text;
    if (!data.buttons) data.buttons = 'ok';

    return this.dialog.open(MessageboxComponent, {
      data: data,
    });
  }

  warning(
    title: string,
    text: string,
    data?: Partial<MessageboxData>
  ): MatDialogRef<MessageboxComponent<MessageboxData>> {
    if (!data) data = {};
    if (!data.type) data.type = 'warning';
    data.title = title;
    data.text = text;
    data.size = 'lg';
    if (!data.buttons) data.buttons = 'okCancel';

    return this.dialog.open(MessageboxComponent, {
      data: data,
    });
  }

  error(
    title: string,
    text: string,
    data?: Partial<MessageboxData>
  ): MatDialogRef<MessageboxComponent<MessageboxData>> {
    if (!data) data = {};
    if (!data.type) data.type = 'error';
    data.title = title;
    data.text = text;
    if (!data.buttons) data.buttons = 'ok';

    return this.dialog.open(MessageboxComponent, {
      data: data,
    });
  }

  success(
    title: string,
    text: string,
    data?: Partial<MessageboxData>
  ): MatDialogRef<MessageboxComponent<MessageboxData>> {
    if (!data) data = {};
    if (!data.type) data.type = 'success';
    data.title = title;
    data.text = text;
    if (!data.buttons) data.buttons = 'ok';

    return this.dialog.open(MessageboxComponent, {
      data: data,
    });
  }

  progress(
    title: string,
    text: string,
    data?: Partial<MessageboxData>
  ): MatDialogRef<MessageboxComponent<MessageboxData>> {
    if (!data) data = {};
    if (!data.type) data.type = 'progress';
    data.title = title;
    data.text = text;
    if (!data.buttons) data.buttons = 'none';

    data.hideCloseButton = true;

    return this.dialog.open(MessageboxComponent, {
      data: data,
    });
  }

  question(
    title: string,
    text: string,
    data?: Partial<MessageboxData>
  ): MatDialogRef<MessageboxComponent<MessageboxData>> {
    if (!data) data = {};
    if (!data.type) data.type = 'question';
    data.title = title;
    data.text = text;
    if (!data.buttons) data.buttons = 'yesNo';

    return this.dialog.open(MessageboxComponent, {
      data: data,
    });
  }

  questionAbort(
    title: string,
    text: string,
    data?: Partial<MessageboxData>
  ): MatDialogRef<MessageboxComponent<MessageboxData>> {
    if (!data) data = {};
    if (!data.type) data.type = 'questionAbort';
    data.title = title;
    data.text = text;
    if (!data.buttons) data.buttons = 'yesNoCancel';

    return this.dialog.open(MessageboxComponent, {
      data: data,
    });
  }

  show(
    title: string,
    text: string,
    data?: Partial<MessageboxData>
  ): MatDialogRef<MessageboxComponent<MessageboxData>> {
    if (!data) data = {};
    if (!data.type) data.type = 'custom';
    data.title = title;
    data.text = text;
    if (!data.buttons) data.buttons = 'ok';

    return this.dialog.open(MessageboxComponent, {
      data: data,
    });
  }
}

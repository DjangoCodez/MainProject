import {
  Component,
  inject,
  signal,
  ViewChild,
  AfterViewInit,
} from '@angular/core';
import { DialogComponent } from '@ui/dialog/dialog/dialog.component';
import {
  FileUploadComponent,
  AttachedFile,
} from '@ui/forms/file-upload/file-upload.component';
import {
  FileUploadDialogDTO,
  FileUploadDialogForm,
} from './file-upload-dialog.model';
import { ValidationHandler } from '@shared/handlers';
import { ReactiveFormsModule } from '@angular/forms';
import { MatDialogRef } from '@angular/material/dialog';
import { FileUploadDialogData } from './models/file-upload-dialog.model';

@Component({
  selector: 'soe-file-upload-dialog',
  templateUrl: './file-upload-dialog.component.html',
  styleUrls: ['./file-upload-dialog.component.scss'],
  imports: [DialogComponent, FileUploadComponent, ReactiveFormsModule],
})
export class FileUploadDialogComponent
  extends DialogComponent<FileUploadDialogData>
  implements AfterViewInit
{
  @ViewChild(FileUploadComponent) fileUploadComponent!: FileUploadComponent;

  validationHandler = inject(ValidationHandler);
  formFUDialog: FileUploadDialogForm = new FileUploadDialogForm({
    validationHandler: this.validationHandler,
    element: new FileUploadDialogDTO(),
  });
  dialogRef = inject(MatDialogRef);

  filesAreAttached = signal(false);

  constructor() {
    super();
  }

  protected afterFilesUploaded($event: {
    success: boolean;
    files: AttachedFile[];
  }): void {
    if ($event.success && this.data.closeOnUpload) {
      this.close($event.files);
    }
  }

  protected afterFilesAttached($event: AttachedFile[]): void {
    this.filesAreAttached.set($event.length > 0);
    if (this.data.closeOnAttach) {
      this.close($event);
    }
  }

  protected close(files: AttachedFile[]): void {
    if (this.data.multipleFiles) {
      this.dialogRef.close(files || []);
    } else {
      this.dialogRef.close(files.length > 0 ? files[0] : null);
    }
  }

  ngAfterViewInit(): void {
    if (!this.data.replaceFile) return;

    const existing = this.data.replaceFile;
    const attached: AttachedFile = {
      id: existing.fileId.toString(),
      name: existing.fileName,
      size: existing.fileSize,
    };

    this.fileUploadComponent.updateFiles([attached]);
  }
}

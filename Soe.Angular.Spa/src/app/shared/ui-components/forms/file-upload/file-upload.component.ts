import { CommonModule } from '@angular/common';
import {
  Component,
  computed,
  input,
  output,
  signal,
  OnInit,
  ViewChild,
  ElementRef,
  effect,
  Injector,
  inject,
  untracked,
} from '@angular/core';
import { NG_VALUE_ACCESSOR, ReactiveFormsModule } from '@angular/forms';
import { Guid } from '@shared/util/string-util';
import { ValueAccessorDirective } from '@ui/forms/directives/value-accessor.directive';
import { ButtonComponent } from '@ui/button/button/button.component';
import { IconButtonComponent } from '@ui/button/icon-button/icon-button.component';
import { LabelComponent } from '@ui/label/label.component';
import { Observable, Subscriber } from 'rxjs';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { ByteFormatterPipe } from '@shared/pipes';
import { MessageboxService } from '@ui/dialog/services/messagebox.service';
export class AttachedFile {
  id = Guid.newGuid();
  name?: string;
  content?: string;
  binaryContent?: Uint8Array;
  size?: number;
  extension?: string;
}

export class ExtendedAttachedFile extends AttachedFile {
  fileUploadStatus?: 'Attached' | 'Uploading' | 'Uploaded' | 'Error';
  errorMessage?: string;
}

export interface IFileUploader {
  uploadFile(file: AttachedFile): Observable<{
    success: boolean;
    errorMessage?: string;
  }>;
}

export enum FileUploaderMode {
  EmitFile, //Used when parent component handles saving of file
  SaveFile, //Used when file is saved by the file uploader
}

@Component({
  selector: 'soe-file-upload',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    ButtonComponent,
    IconButtonComponent,
    LabelComponent,
    ByteFormatterPipe,
    TranslatePipe,
  ],
  templateUrl: './file-upload.component.html',
  styleUrls: ['./file-upload.component.scss'],
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      multi: true,
      useExisting: FileUploadComponent,
    },
  ],
})
export class FileUploadComponent
  extends ValueAccessorDirective<string>
  implements OnInit
{
  messageboxService = inject(MessageboxService);
  translateService = inject(TranslateService);

  @ViewChild('fileImportInput')
  fileImportInput!: ElementRef;

  inputId = input<string>(Math.random().toString(24));
  labelKey = input('');
  placeholderKey = input('');
  inline = input(false);
  hideDetails = input(false);
  hideDropZone = input(false);
  hideInputField = input(false);
  asBinary = input(false);
  multipleFiles = input(false);
  showStatus = input(false);
  uploadOnAttach = input(false);
  showDeleteButton = input(true);
  isFileReplace = input(false);

  showCancelButton = input(false);
  showClearButton = input(false);
  showUploadButton = input(true);
  allowExtraLargeFiles = input(false);

  showActionButtons = computed<boolean>(() => {
    return (
      this.showCancelButton() ||
      this.showClearButton() ||
      this.showUploadButton()
    );
  });

  fileUploader = input<IFileUploader | undefined>(undefined);
  updateAttachedFiles = input<string[] | undefined>(undefined);

  // fileSizeControl = input<FormControl>(new FormControl());
  // fileNameControl = input<FormControl>(new FormControl());

  /**
   * @description
   * set the accepted file type extensions
   * ## Example
   *
   * Extensions: ['.csv','.txt','.docx','.xlsx']
   **/
  acceptedFileExtensions = input<string[]>([]);

  afterFilesAttached = output<AttachedFile[]>();
  afterFilesUploaded = output<{ success: boolean; files: AttachedFile[] }>();
  cancelled = output<void>();

  isFileOver = signal(false);
  attachedFiles = signal<ExtendedAttachedFile[]>([]);
  hasFile = computed<boolean>(() => this.attachedFiles().length > 0);
  fileHasBeenUploaded = computed<boolean>(() =>
    this.attachedFiles().some(f => f.fileUploadStatus === 'Uploaded')
  );
  isDoingWork = computed<boolean>(() => {
    return this.attachedFiles().some(f => f.fileUploadStatus === 'Uploading');
  });

  readonly #MAX_FILE_SIZE = 20 * 1024 * 1024; //20 MB
  readonly #MAX_FILE_SIZE_EX = 40 * 1024 * 1024; //40 MB

  constructor() {
    super(inject(Injector));

    effect(() => {
      const names = this.updateAttachedFiles();
      if (!names) return;

      untracked(() => {
        const files: ExtendedAttachedFile[] = [];
        const uploadedFiles = this.attachedFiles();

        names.forEach(name => {
          const uploadedFile = uploadedFiles.find(f => f.name === name);
          if (uploadedFile) files.push(uploadedFile);
        });

        this.updateFiles(files);
      });
    });
  }

  onFileChange(event: Event) {
    const target = event.target as HTMLInputElement;
    this.startLoadAndSet(this.fileListToArray(target.files as FileList));
  }

  onFileOver($event: DragEvent) {
    $event.preventDefault();
    if (this.isDoingWork()) return;
    this.isFileOver.set(true);
  }

  onFileLeave($event: DragEvent) {
    $event.preventDefault();
    if (this.isDoingWork()) return;
    this.isFileOver.set(false);
  }

  onFileDropped($event: DragEvent) {
    $event.preventDefault();
    if (this.isDoingWork()) return;
    this.isFileOver.set(false);

    let validFiles = this.fileListToArray(
      $event.dataTransfer?.files as FileList
    );

    if (!this.multipleFiles() && validFiles.length > 0) {
      validFiles = [validFiles[0]];
    }
    this.startLoadAndSet(validFiles);
  }

  fileListToArray(files: FileList) {
    const validFiles: File[] = [];
    for (let i = 0; i < files.length; i++) {
      if (this.isValidFile(files[i])) {
        validFiles.push(files[i]);
      }
    }
    return validFiles;
  }

  filesToFileList(files: File[]): FileList {
    const dataTransfer = new DataTransfer();
    files.forEach(file => dataTransfer.items.add(file));
    return dataTransfer.files;
  }

  attachedFilesToFileList(files: AttachedFile[]): FileList {
    const dataTransfer = new DataTransfer();
    files.forEach(file => {
      if (file.binaryContent) {
        dataTransfer.items.add(
          this.binaryContentToFile(file.binaryContent, file.name ?? '')
        );
      }
    });
    return dataTransfer.files;
  }

  binaryContentToFile(binary: Uint8Array, name: string): File {
    // Ensure binary is a valid BlobPart (Uint8Array backed by ArrayBuffer)
    const uint8 = new Uint8Array(binary);
    const blob = new Blob([uint8]);
    return new File([blob], name);
  }

  startLoadAndSet(files: File[]) {
    this.fileImportInput.nativeElement.files = this.filesToFileList(files);
    if (!files) {
      this.fileImportInput.nativeElement.value = '';
      return;
    }

    // Only support for one file, take first if multiple are dropped
    // and check whether file extension is in the accepted file extensions
    const validFiles: File[] = [];
    for (let i = 0; i < files.length; i++) {
      const file = files[i];

      if (this.isFileTooBig(file.name, file.size)) return;

      if (this.isValidFile(file)) {
        validFiles.push(file);
      }
    }
    this.loadAndSetFiles(validFiles);
  }

  updateFiles(files: AttachedFile[]) {
    if (!this.fileImportInput) return;
    this.attachedFiles.set(files);
    const filesList = this.attachedFilesToFileList(files);

    if (filesList && filesList.length > 0) {
      this.fileImportInput.nativeElement.files = filesList;
    }
    if (!files || files.length === 0) {
      this.fileImportInput.nativeElement.value = '';
      return;
    }
  }

  removeFile(file: AttachedFile): void {
    const files = this.attachedFiles().filter(f => f !== file);
    this.updateFiles(files);
  }

  onCancel() {
    this.cancelled.emit();
  }

  onClear() {
    const blockedStatuses = ['Uploading', 'Uploaded'];
    const files = this.attachedFiles().filter(
      f => !f.fileUploadStatus || blockedStatuses.includes(f.fileUploadStatus)
    );
    this.updateFiles(files);
  }

  onUpload() {
    if (!this.fileUploader()) return;

    let attachedCount = 0;
    const obs = new Observable<ExtendedAttachedFile>(subscriber => {
      this.attachedFiles().forEach(file => {
        if (file.fileUploadStatus === 'Attached') {
          file.fileUploadStatus = 'Uploading';
          subscriber.next(file);
          attachedCount++;
        }
      });
    });

    let uploaded = 0;
    let successful = 0;
    const files: ExtendedAttachedFile[] = [];
    obs.subscribe(attachedFile => {
      this.fileUploader()
        ?.uploadFile(attachedFile)
        .subscribe(res => {
          uploaded++;
          if (res.success) {
            attachedFile.fileUploadStatus = 'Uploaded';
            successful++;
          } else {
            attachedFile.fileUploadStatus = 'Error';
            attachedFile.errorMessage = res.errorMessage;
          }
          files.push(attachedFile);

          if (attachedCount === uploaded) {
            this.attachedFiles.set(files);
            this.afterFilesUploaded.emit({
              success: successful === attachedCount,
              files: this.attachedFiles(),
            });
          }
        });
    });
  }

  private isValidFile(file: File): boolean {
    if (this.acceptedFileExtensions().length === 0) return true;
    return this.acceptedFileExtensions().includes(
      this.getFileExtension(file.name.toLowerCase())
    );
  }

  private loadAndSetFiles(files: File[]) {
    const existing = this.attachedFiles()[0];

    if (!this.multipleFiles()) {
      this.attachedFiles.set([]);
    }
    this.control.markAsDirty();

    const obs = new Observable<AttachedFile>(subscriber => {
      files.forEach(file => {
        this.loadAndSetFile(subscriber, file);
      });
    });

    let loaded = 0;
    obs.subscribe(attachedFile => {
      loaded++;

      if (this.isFileReplace()) {
        attachedFile.id = existing.id;
        this.attachedFiles.set([attachedFile]);
      } else {
        // Add new file
        this.attachedFiles.set([...this.attachedFiles(), attachedFile]);
      }

      if (files.length === loaded) {
        this.afterFilesAttached.emit(this.attachedFiles());
        this.updateFiles(this.attachedFiles());
        if (this.uploadOnAttach()) {
          this.onUpload();
        }
      }
    });
  }

  private loadAndSetFile(obs: Subscriber<AttachedFile>, file: File): void {
    const reader = new FileReader();
    if (this.asBinary()) reader.readAsArrayBuffer(file);
    else reader.readAsDataURL(file);
    reader.onload = () => {
      const attachedFile: ExtendedAttachedFile = {
        id: Guid.newGuid(),
        fileUploadStatus: 'Attached',
        name: file.name,
        size: file.size,
        extension: this.getFileExtension(file.name),
      };

      if (this.asBinary()) {
        let binary: Uint8Array | null = null;
        if (reader.result instanceof ArrayBuffer) {
          binary = new Uint8Array(reader.result);
        } else {
          binary = null; // handle string/null case as needed
        }
        attachedFile.binaryContent = binary ?? undefined;
      } else {
        let base64String = reader.result as string;

        // Strip first identifying part eg: 'data:application/pdf;base64,'
        base64String = base64String.substring(base64String.indexOf(',') + 1);
        attachedFile.content = base64String;
      }
      obs.next(attachedFile);
    };
    reader.onabort = () => obs.error('file reading was aborted');
    reader.onerror = () => obs.error('file reading has failed');
  }

  private getFileExtension(fName: string): string {
    return fName.substring(fName.lastIndexOf('.'));
  }

  clearAllFiles() {
    this.updateFiles([]);
  }

  isFileTooBig(fileName: string, fileSize: number): boolean {
    const maxSize = this.allowExtraLargeFiles()
      ? this.#MAX_FILE_SIZE_EX
      : this.#MAX_FILE_SIZE;

    if (fileSize > maxSize) {
      const message = this.translateService
        .instant('core.fileupload.filetoolarge.message')
        .replace('{0}', this.bToMb(fileSize))
        .replace('{1}', this.bToMb(this.#MAX_FILE_SIZE));
      this.messageboxService.error(fileName, message);
      return true;
    }
    return false;
  }

  bToMb(sizeInBytes: number) {
    return (sizeInBytes / (1024 * 1024)).round(0).toString();
  }
}

import { FileRecordDTO } from '@shared/models/file.model';
import { DialogData } from '@ui/dialog/models/dialog';
import { IFileUploader } from '@ui/forms/file-upload/file-upload.component';

export interface FileUploadDialogData extends DialogData {
  closeOnUpload: boolean;
  closeOnAttach: boolean;
  multipleFiles: boolean;
  asBinary: boolean;
  acceptedFileExtensions?: string[];

  // A file uploader is required if the file upload dialog is used to upload files
  fileUploader?: IFileUploader;
  replaceFile?: FileRecordDTO;
}

export const defaultFileUploadDialogData = (
  fileUploader?: IFileUploader,
  asBinary = false
): FileUploadDialogData => ({
  size: 'lg',
  title: 'core.fileupload.fileupload',
  multipleFiles: true,
  hideFooter: true,
  fileUploader: fileUploader,
  closeOnUpload: true,
  closeOnAttach: false,
  asBinary,
});

export function fileReplaceDialogData(
  uploader?: IFileUploader,
  replaceFile?: FileRecordDTO
): FileUploadDialogData {
  return {
    fileUploader: uploader,
    multipleFiles: false,
    closeOnUpload: true,
    replaceFile,
    closeOnAttach: false,
    size: 'lg',
    title: 'core.fileupload.filereplace',
    hideFooter: true,
    asBinary: false,
  };
}

export const fileUploadDialogData = (options: {
  multipleFiles: boolean;
  asBinary: boolean;
  acceptedFileExtensions?: string[];
}): FileUploadDialogData => ({
  size: 'lg',
  title: 'core.fileupload.fileupload',
  multipleFiles: options.multipleFiles,
  hideFooter: true,
  fileUploader: undefined,
  closeOnUpload: false,
  closeOnAttach: true,
  asBinary: options.asBinary,
  acceptedFileExtensions: options.acceptedFileExtensions,
});

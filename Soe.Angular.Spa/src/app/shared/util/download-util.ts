import { IDownloadFileDTO } from '@shared/models/generated-interfaces/FileUploadDTO';
import { FileUtility } from './file-util';

export class DownloadUtility {
  static openInSameTab($window: any, url: string) {
    $window.open(url, '_self');
  }

  static openInNewTab($window: any, url: string) {
    $window.open(url, '_blank');
  }

  static openInNewWindow($window: any, url: string) {
    $window.open(url, 'newwindow');
  }

  static downloadFileDTO(file: IDownloadFileDTO) {
    DownloadUtility.downloadFile(file.fileName, file.fileType, file.content);
  }

  static downloadFile(
    fileName: string,
    fileType: string,
    dataBase64: string
  ): void {
    const byteArray = FileUtility.base64StringToByteArray(dataBase64);

    const blob = new Blob([byteArray], { type: fileType });
    const link = window.document.createElement('a');
    const url = window.URL.createObjectURL(blob);
    link.href = url;
    link.download = fileName;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    window.URL.revokeObjectURL(url);
  }
}

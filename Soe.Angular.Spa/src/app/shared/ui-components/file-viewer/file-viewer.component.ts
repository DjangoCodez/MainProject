import { Component } from '@angular/core';
import { DialogComponent } from '@ui/dialog/dialog/dialog.component'
import { DialogData, DialogSize } from '@ui/dialog/models/dialog'
import { ImageViewerComponent } from '@ui/image-viewer/image-viewer.component'
import { PdfViewerComponent } from '@ui/pdf-viewer/pdf-viewer.component';

export class FilePreviewDialogData implements DialogData {
  title: string;
  size: DialogSize = 'xl';
  hideFooter = true;

  constructor(
    filename: string,
    public base64Data: string,
    public fileExtension: string
  ) {
    this.title = filename;
  }

  static canBePreviewed(fileExtension: string): boolean {
    return ['pdf', 'png', 'jpg', 'jpeg', 'gif'].includes(
      fileExtension.replace('.', '')
    );
  }
}

@Component({
  selector: 'soe-file-viewer',
  imports: [DialogComponent, ImageViewerComponent, PdfViewerComponent],
  templateUrl: './file-viewer.component.html',
  styleUrls: ['./file-viewer.component.scss'],
})
export class FileViewerComponent extends DialogComponent<FilePreviewDialogData> {
  // This component is a work in progress
  // Should add download possibility and look over the type of files
  // we can support.

  base64Data!: string;
  fileExtension!: string;
  fileName!: string;

  constructor() {
    super();
    this.setDialogParam();
  }

  setDialogParam() {
    if (this.data) {
      this.base64Data = this.data.base64Data;
      this.fileExtension = this.data.fileExtension;
      this.fileName = this.data.title;
    }
  }
}

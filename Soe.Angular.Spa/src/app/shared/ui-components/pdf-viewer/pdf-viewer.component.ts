// https://github.com/vadimdez/ng2-pdf-viewer


import {
  Component,
  ViewChild,
  computed,
  effect,
  inject,
  input,
  signal,
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { DownloadUtility } from '@shared/util/download-util';
import { IconModule } from '@ui/icon/icon.module'
import { MenuButtonItem } from '@ui/button/menu-button/menu-button.component';
import {
  PdfViewerComponent as Ng2PdfViewerComponent,
  PdfViewerModule as NgxPdfViewerModule,
  PDFDocumentProxy,
} from 'ng2-pdf-viewer';

@Component({
  selector: 'soe-pdf-viewer',
  imports: [
    FormsModule,
    NgxPdfViewerModule,
    IconModule,
    TranslatePipe
],
  templateUrl: './pdf-viewer.component.html',
  styleUrls: ['./pdf-viewer.component.scss'],
})
export class PdfViewerComponent {
  @ViewChild(Ng2PdfViewerComponent)
  private readonly pdfViewer?: Ng2PdfViewerComponent;

  base64Data = input('');
  fileName = input('');
  width = input(800);

  translate = inject(TranslateService);

  searchText = '';
  rotation = 0;
  searchVisible = signal(false);

  pdfBinary?: Uint8Array;

  loading = signal(false);

  page = 1;
  totalPages = signal(1);

  zoom = signal(1);
  zoomPercent = computed(() => {
    return `${Math.round(this.zoom() * 100)}%`;
  });

  constructor() {
    effect(() => {
      if (this.base64Data()) {
        this.pdfBinary = this.base64ToArrayBuffer(this.base64Data());
      }
    });
  }

  changeZoom = (step: number) => this.zoom.set(this.zoom() + step);
  movePage = (step: number) => (this.page = this.page + step);
  moveToLastPage = () => (this.page = this.totalPages());
  moveToFirstPage = () => (this.page = 1);

  rotatePage = () => {
    this.rotation = this.rotation + 90;
    if (this.rotation === 360) {
      this.rotation = 0;
    }
  };

  toggleSearch() {
    this.searchVisible.set(!this.searchVisible());
    if (!this.searchVisible()) {
      this.searchText = '';
      this.search(true);
    }
  }

  pdfLoaded(pdf: PDFDocumentProxy) {
    this.totalPages.set(pdf.numPages);
    this.loading.set(false);
  }

  changeZoomBySelection(item: MenuButtonItem) {
    this.zoom.set(Number(item.label?.replace('%', '')) / 100);
  }

  downloadPdf() {
    DownloadUtility.downloadFile(
      this.fileName() ||
        `${new Date().toDateString().replaceAll(' ', '_')}.pdf`,
      'application/pdf',
      `${this.base64Data()}`
    );
  }

  base64ToArrayBuffer(base64: string) {
    this.loading.set(true);
    const binaryString = window.atob(base64);
    const len = binaryString.length;
    const bytes = new Uint8Array(len);
    for (let i = 0; i < len; i++) {
      bytes[i] = binaryString.charCodeAt(i);
    }
    return bytes;
  }

  search(findNext: boolean) {
    this.pdfViewer?.eventBus.dispatch('find', {
      query: this.searchText,
      type: 'again',
      caseSensitive: false,
      findPrevious: !findNext,
      highlightAll: true,
      phraseSearch: true,
    });
  }
}

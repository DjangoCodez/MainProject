import {
  AfterViewInit,
  Component,
  computed,
  ElementRef,
  input,
  signal,
  ViewChild,
} from '@angular/core';
import { DownloadUtility } from '@shared/util/download-util';
import { IconModule } from '@ui/icon/icon.module';

@Component({
  selector: 'soe-image-viewer',
  imports: [IconModule],
  templateUrl: './image-viewer.component.html',
  styleUrl: './image-viewer.component.scss',
})
export class ImageViewerComponent implements AfterViewInit {
  @ViewChild('image') image?: ElementRef;
  @ViewChild('viewPort') viewPort?: ElementRef;

  extension = input('');
  base64Data = input('');
  base64Source = computed(
    () => `data:image/${this.extension()};base64,${this.base64Data()}`
  );
  fileName = input('');
  loading = signal(false);
  error = signal(false);
  zoom = 1;
  rotation = 0;
  hasTransformed = false;

  constructor() {
    this.loading.set(true);
  }

  ngAfterViewInit() {}

  onImageLoad() {
    this.loading.set(false);
  }

  downloadImage() {
    const extension = this.extension();
    DownloadUtility.downloadFile(
      this.fileName() ||
        `${new Date().toDateString().replaceAll(' ', '_')}.${extension}`,
      `image/${extension}`,
      `${this.base64Data()}`
    );
  }

  rotateImage() {
    this.rotation = (this.rotation + 90) % 360;
    this.applyTransform();
  }

  changeZoom = (step: number) => {
    this.zoom = Math.max(0.1, this.zoom + step);
    this.applyTransform();
  };

  onImageError(error: ErrorEvent) {
    console.log(error);
    this.error.set(true);
  }

  applyTransform() {
    if (this.image && this.viewPort) {
      const viewPort = this.viewPort.nativeElement;
      const image = this.image.nativeElement;

      if (!this.hasTransformed) {
        this.hasTransformed = true;
      }

      image.style.transform = `scale(${this.zoom}) rotate(${this.rotation}deg)`;

      //To counter the transformation and have the entire image scrollable.
      setTimeout(() => {
        const viewRect = viewPort.getBoundingClientRect();
        const imageRect = image.getBoundingClientRect();
        const deltaX = imageRect.width - viewRect.width;
        const deltaY = imageRect.height - viewRect.height;
        image.style.marginLeft = `${Math.max(deltaX, 0)}px`;
        image.style.marginTop = `${Math.max(deltaY, 0)}px`;
      }, 0);
    }
  }
}

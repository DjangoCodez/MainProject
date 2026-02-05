import { Pipe } from '@angular/core';

@Pipe({
  name: 'byteFormatter',
  standalone: true,
})
export class ByteFormatterPipe {
  transform(value?: number): string {
    if (!value) return '';
    const k = 1024;
    const sizes = ['B', 'KB', 'MB', 'GB', 'TB', 'PB', 'EB', 'ZB', 'YB'];
    const i = Math.floor(Math.log(value) / Math.log(k));
    return parseFloat((value / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
  }
}

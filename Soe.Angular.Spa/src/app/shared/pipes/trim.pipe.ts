import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'trim',
  standalone: true,
})
export class TrimPipe implements PipeTransform {
  transform(value: string): unknown {
    console.log('|' + value + '|');
    console.log('|' + (value ? value.trim() : '') + '|');

    if (!value) return '';
    return value.trim();
  }
}

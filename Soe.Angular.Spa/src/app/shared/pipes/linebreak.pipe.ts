import { Pipe, PipeTransform } from '@angular/core';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';

@Pipe({
  name: 'linebreak',
})
export class LinebreakPipe implements PipeTransform {
  constructor(private sanitizer: DomSanitizer) {}

  transform(value: string | null | undefined): SafeHtml {
    if (!value) return '';
    const html = value
      .replace(/\\n/g, '<br/>') // Turn escaped \n into line breaks
      .replace(/\n/g, '<br/>') // Turn actual newline chars into line breaks
      .replace(/<br\s*\/?>/gi, '<br/>'); // Normalize variants
    return this.sanitizer.bypassSecurityTrustHtml(html);
  }
}

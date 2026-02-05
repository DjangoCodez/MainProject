import { inject, Injectable } from '@angular/core';
import { ToasterService } from '@ui/toaster/services/toaster.service';

@Injectable({
  providedIn: 'root',
})
export class ClipBoardService {
  private readonly toasterService = inject(ToasterService);

  public writeToClipboard(value: string): void {
    navigator.clipboard.writeText(value).then(() => {
      this.toasterService.success('clipboard.copied', 'clipboard.title');
    });
  }
}

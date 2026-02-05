import { inject, Injectable, signal } from '@angular/core';
import { UrlHelperService } from '@shared/services/url-params.service';

@Injectable()
export class VoucherParamsService {
  urlService = inject(UrlHelperService);
  isTemplate = signal(this.urlService.path.includes('/vouchertemplates'));
}

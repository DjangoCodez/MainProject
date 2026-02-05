import { inject, Injectable } from '@angular/core';
import { UrlHelperService } from '@shared/services/url-params.service';

@Injectable()
export class ShiftTypeParamsService {
  urlHelper = inject(UrlHelperService);
  get isOrder(): boolean {
    return this.urlHelper.path.includes('/invoicesettings');
  }
}

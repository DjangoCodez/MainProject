// https://www.npmjs.com/package/ngx-toastr

import { inject, Injectable } from '@angular/core';
import { IndividualConfig, ToastrService } from 'ngx-toastr';

@Injectable({
  providedIn: 'root',
})
export class ToasterService {
  private readonly toastr = inject(ToastrService);

  info<ConfigPayload = any>(
    message?: string,
    title?: string,
    options?: Partial<IndividualConfig<ConfigPayload>>
  ) {
    this.toastr.info(message, title, options);
  }

  warning<ConfigPayload = any>(
    message?: string,
    title?: string,
    options?: Partial<IndividualConfig<ConfigPayload>>
  ) {
    this.toastr.warning(message, title, options);
  }

  error<ConfigPayload = any>(
    message?: string,
    title?: string,
    options?: Partial<IndividualConfig<ConfigPayload>>
  ) {
    this.toastr.error(message, title, options);
  }

  success<ConfigPayload = any>(
    message?: string,
    title?: string,
    options?: Partial<IndividualConfig<ConfigPayload>>
  ) {
    this.toastr.success(message, title, options);
  }

  show<ConfigPayload = any>(
    message?: string,
    title?: string,
    options?: Partial<IndividualConfig<ConfigPayload>>
  ) {
    this.toastr.show(message, title, options);
  }
}

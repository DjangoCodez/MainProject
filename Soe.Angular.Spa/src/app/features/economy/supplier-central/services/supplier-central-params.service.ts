import { inject, Injectable, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { UrlHelperService } from '@shared/services/url-params.service';

@Injectable()
export class SupplierCentralUrlParamsService {
  private readonly route = inject(ActivatedRoute);
  private readonly urlHelper = inject(UrlHelperService);

  public supplierId = signal(
    Number(this.route.snapshot.queryParams['supplier']) || 0
  );

  public setSupplierId(id: number) {
    this.supplierId.set(id);
    this.urlHelper.setQueryParam('supplier', id);
  }
}

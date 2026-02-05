import { Component, EventEmitter, Output, inject } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { ValidationHandler } from '@shared/handlers';
import { SmallGenericType } from '@shared/models/generic-type.model';
import {
  TermGroup,
  SoeOriginType,
  Feature,
} from '@shared/models/generated-interfaces/Enumerations';
import { CoreService } from '@shared/services/core.service'
import { ProgressService } from '@shared/services/progress/progress.service';
import { Perform } from '@shared/util/perform.class';
import { forkJoin, tap } from 'rxjs';
import { ProductStatisticsFilterForm } from '../../models/product-statistics-filter-form.model';
import { ProductStatisticsRequest } from '../../models/product-statistics.model';
import { ProductService } from '../../../products/services/product.service';

@Component({
    selector: 'soe-product-statistics-grid-filter',
    templateUrl: './product-statistics-grid-filter.component.html',
    styleUrls: ['./product-statistics-grid-filter.component.scss'],
    standalone: false
})
export class ProductStatisticsGridFilterComponent {
  @Output() searchClick = new EventEmitter<ProductStatisticsRequest>();

  validationHandler = inject(ValidationHandler);
  coreService = inject(CoreService);
  productService = inject(ProductService);
  performLoad = new Perform<SmallGenericType[]>(this.progressService);
  types: SmallGenericType[] = [];
  products: SmallGenericType[] = [];
  formFilter: ProductStatisticsFilterForm = new ProductStatisticsFilterForm({
    validationHandler: this.validationHandler,
    element: new ProductStatisticsRequest(),
  });

  constructor(
    private progressService: ProgressService,
    private translationService: TranslateService
  ) {
    this.loadAll();
  }

  loadAll() {
    this.performLoad.load(
      forkJoin([
        this.translationService.get('common.all'),
        this.coreService.getTermGroupContent(
          TermGroup.OriginType,
          false,
          false
        ),
        this.coreService.hasReadOnlyPermissions([Feature.Billing_Purchase]),
        this.productService.getProducts(),
      ]).pipe(
        tap(([allTerm, originTypes, permissions, products]) => {
          this.products = products;
          this.types = originTypes
            .filter(
              x =>
                x.id === SoeOriginType.CustomerInvoice ||
                (x.id === SoeOriginType.Purchase &&
                  permissions[Feature.Billing_Purchase]) ||
                (x.id === SoeOriginType.None &&
                  permissions[Feature.Billing_Purchase])
            )
            .map(x =>
              x.id === SoeOriginType.None ? { ...x, name: allTerm } : x
            )
            .sort((a, _) => (a.id === SoeOriginType.None ? 1 : -1));
        })
      )
    );
  }

  search() {
    this.searchClick.emit({
      productIds: this.formFilter.productIds.value,
      originType: this.formFilter.originType.value,
      fromDate: this.formFilter.fromDate.value,
      toDate: this.formFilter.toDate.value,
      includeServiceProducts: this.formFilter.includeServiceProducts.value,
    });
  }
}

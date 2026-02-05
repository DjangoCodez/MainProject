import { Component, EventEmitter, Output, inject } from '@angular/core';
import { ValidationHandler } from '@shared/handlers';
import { SmallGenericType } from '@shared/models/generic-type.model';
import {
  SoeOriginType,
  TermGroup,
} from '@shared/models/generated-interfaces/Enumerations';
import { CoreService } from '@shared/services/core.service'
import { ProgressService } from '@shared/services/progress/progress.service';
import { Perform } from '@shared/util/perform.class';
import { tap } from 'rxjs';
import { SalesStatisticsFilterForm } from '../../models/sales-statistics-filter-form.model';
import { GeneralProductStatisticsDTO } from '../../models/sales-statistics.model';
import { SalesStatisticsService } from '../../services/sales-statistics.service';

@Component({
  selector: 'soe-sales-statistics-grid-filter',
  templateUrl: './sales-statistics-grid-filter.component.html',
  standalone: false,
})
export class SalesStatisticsGridFilterComponent {
  @Output() searchClick = new EventEmitter<GeneralProductStatisticsDTO>();
  @Output() filterChange = new EventEmitter<SoeOriginType>();

  validationHandler = inject(ValidationHandler);
  coreService = inject(CoreService);
  service = inject(SalesStatisticsService);
  performTypes = new Perform<SmallGenericType[]>(this.progressService);
  types: SmallGenericType[] = [];
  formFilter: SalesStatisticsFilterForm = new SalesStatisticsFilterForm({
    validationHandler: this.validationHandler,
    element: new GeneralProductStatisticsDTO(),
  });

  constructor(private progressService: ProgressService) {
    this.loadTypes();
  }

  loadTypes() {
    this.performTypes.load(
      this.coreService
        .getTermGroupContent(TermGroup.OriginType, false, false)
        .pipe(
          tap(value => {
            value.forEach(element => {
              if (
                element.id === SoeOriginType.CustomerInvoice ||
                element.id === SoeOriginType.Order ||
                element.id === SoeOriginType.Offer ||
                element.id === SoeOriginType.Contract
              ) {
                this.types.push({ id: element.id, name: element.name });
              }
            });
          })
        )
    );
  }

  search(): void {
    const searchDto = this.formFilter.value as GeneralProductStatisticsDTO;
    this.searchClick.emit({
      originType: searchDto.originType,
      fromDate: searchDto.fromDate,
      toDate: new Date(
        searchDto.toDate.getFullYear(),
        searchDto.toDate.getMonth(),
        searchDto.toDate.getDate() + 1
      ),
    });
  }
}

import {
  Component,
  EventEmitter,
  OnInit,
  Output,
  inject,
  input,
  signal,
} from '@angular/core';
import { ValidationHandler } from '@shared/handlers';
import { SmallGenericType } from '@shared/models/generic-type.model';
import {
  SoeEntityType,
  SoeOriginType,
} from '@shared/models/generated-interfaces/Enumerations';
import { ProgressService } from '@shared/services/progress/progress.service';
import { Perform } from '@shared/util/perform.class';
import { tap } from 'rxjs';
import { ExtraFieldsService } from '@shared/features/extra-fields/services/extra-fields.service';
import { ExtraFieldsFilterForm } from '@shared/features/extra-fields/models/extra-fields-filter-form.model';
import { ExtraFieldsUrlParamsService } from '@shared/features/extra-fields/services/extra-fields-url.service';

export interface ISearchExtraFieldsGridModel {
  entity: SoeEntityType;
}

@Component({
  selector: 'soe-extra-fields-grid-filter',
  templateUrl: './extra-fields-grid-filter.component.html',
  standalone: false,
  providers: [ExtraFieldsUrlParamsService],
})
export class ExtraFieldsGridFilterComponent implements OnInit {
  @Output() searchClick = new EventEmitter<ISearchExtraFieldsGridModel>();
  @Output() filterChange = new EventEmitter<SoeOriginType>();

  loadingEntities = signal(false);
  selectedEntity = input<SoeEntityType>(0);

  validationHandler = inject(ValidationHandler);
  service = inject(ExtraFieldsService);
  performEntities = new Perform<SmallGenericType[]>(this.progressService);
  entities: SmallGenericType[] = [];

  formFilter: ExtraFieldsFilterForm = new ExtraFieldsFilterForm({
    validationHandler: this.validationHandler,
    element: {} as ISearchExtraFieldsGridModel,
  });

  constructor(
    private progressService: ProgressService,
    private urlService: ExtraFieldsUrlParamsService
  ) {}

  ngOnInit() {
    this.formFilter.entity.patchValue(this.urlService.entityType());
    this.loadingEntities.set(true);
    this.loadEntities();

    if (this.urlService.entityType() != SoeEntityType.None) {
      this.search();
    }
  }

  loadEntities() {
    this.performEntities.load(
      this.service
        .getValidEntityTypes([
          SoeEntityType.Employee,
          SoeEntityType.Account,
          SoeEntityType.PayrollProductSetting,
          SoeEntityType.Customer,
          SoeEntityType.Supplier,
          SoeEntityType.InvoiceProduct,
        ])
        .pipe(
          tap(value => {
            this.entities = value;
            this.loadingEntities.set(false);
          })
        )
    );
  }

  search(): void {
    const searchDto = this.formFilter.value as ISearchExtraFieldsGridModel;
    this.searchClick.emit({
      entity: searchDto.entity,
    });
  }
}

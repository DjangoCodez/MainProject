import { Component, EventEmitter, Input, Output, inject } from '@angular/core';
import { IAccountYearDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { FlowHandlerService } from '@shared/services/flow-handler.service'
import { ProgressService } from '@shared/services/progress/progress.service';
import { Perform } from '@shared/util/perform.class';
import { VoucherGridFilterDTO } from '../../../models/voucher.model';
import { VoucherGridFilterForm } from '../../../models/voucher-grid-filter-form.model';
import { ValidationHandler } from '@shared/handlers';
import { VoucherService } from '../../../services/voucher.service';
import { tap } from 'rxjs';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { TranslateService } from '@ngx-translate/core';
import { VoucherParamsService } from '@features/economy/voucher/services/voucher-params.service';
import { PersistedAccountingYearService } from '@features/economy/services/accounting-year.service';

@Component({
  selector: 'soe-voucher-grid-filter',
  templateUrl: './voucher-grid-filter.component.html',
  styleUrl: './voucher-grid-filter.component.scss',
  providers: [FlowHandlerService],
  standalone: false,
})
export class VoucherGridFilterComponent {
  constructor(public handler: FlowHandlerService) {
    this.handler.execute({
      lookups: [this.loadAccountPeriods()],
      onFinished: () => {
        this.patchValue();
        this.onFilter();
      },
    });
  }

  @Input() voucherSeriesTypes: SmallGenericType[] = [];
  @Input() selectedVoucherSeriesType: number = 0;
  @Output() filter = new EventEmitter<VoucherGridFilterDTO>();
  @Output() saveVoucherSeriesType = new EventEmitter<number>();

  service = inject(VoucherService);
  progressService = inject(ProgressService);
  validationHandler = inject(ValidationHandler);
  translate = inject(TranslateService);
  urlService = inject(VoucherParamsService);
  ayService = inject(PersistedAccountingYearService);

  performLoad = new Perform<any>(this.progressService);

  accountYears: IAccountYearDTO[] = [];
  formFilter: VoucherGridFilterForm = new VoucherGridFilterForm({
    validationHandler: this.validationHandler,
    element: new VoucherGridFilterDTO(),
  });

  onFilter() {
    if (this.formFilter.accountYearId.value) {
      this.loadVoucherSeriesTypes().subscribe(() => {
        if (this.selectedVoucherSeriesType !== 0) {
          const voucherSeriesType = this.voucherSeriesTypes.find(
            v => v.id === this.selectedVoucherSeriesType
          );
          this.formFilter.patchValue({
            voucherSeriesTypeId: voucherSeriesType
              ? this.selectedVoucherSeriesType
              : 0,
          });
        }

        this.filter.emit(this.formFilter.value);
      });
    }
  }

  onVoucherSeriesTypesChanged() {
    this.saveVoucherSeriesType.emit(this.formFilter.voucherSeriesTypeId.value);
    this.filter.emit(this.formFilter.value);
  }

  patchValue() {
    this.formFilter.customVoucherSeriesTypeIdPatch(
      this.selectedVoucherSeriesType
    );
  }

  loadAccountPeriods() {
    return this.performLoad.load$(
      this.ayService.ensureAccountYearIsLoaded$(() => {
        return this.service.getAccountYears(false, true).pipe(
          tap(data => {
            this.accountYears = data.reverse();
            this.formFilter.patchValue({
              accountYearId: this.ayService.selectedAccountYearId(),
            });
          })
        );
      })
    );
  }

  loadVoucherSeriesTypes() {
    return this.performLoad.load$(
      this.service
        .getVoucherSeriesByYear(this.formFilter.accountYearId.value, false)
        .pipe(
          tap(data => {
            this.voucherSeriesTypes = [];
            this.voucherSeriesTypes.push(
              new SmallGenericType(0, this.translate.instant('common.all'))
            );
            data.forEach(v => {
              this.voucherSeriesTypes.push(
                new SmallGenericType(
                  v.voucherSeriesTypeId,
                  v.voucherSeriesTypeNr + '. ' + v.voucherSeriesTypeName
                )
              );
            });
          })
        )
    );
  }
}

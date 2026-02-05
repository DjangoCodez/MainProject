import {
  Component,
  computed,
  inject,
  input,
  OnDestroy,
  OnInit,
  output,
  signal,
} from '@angular/core';
import { ValidationHandler } from '@shared/handlers';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { Subject, takeUntil } from 'rxjs';
import { ActorInvoiceMatchesFilterForm } from '../../models/actor-invoice-matches-filter-form.model';
import { ActorInvoiceMatchesFilterDTO } from '../../models/actor-invoice-matches-filter-dto.model';

@Component({
  selector: 'soe-actor-invoice-matches-filter',
  standalone: false,
  templateUrl: './actor-invoice-matches-filter.component.html',
})
export class ActorInvoiceMatchesFilterComponent implements OnInit, OnDestroy {
  private readonly validationHandler = inject(ValidationHandler);
  private readonly _destroy$ = new Subject<void>();

  actors = input.required<SmallGenericType[]>();
  types = input.required<SmallGenericType[]>();
  actorLabelKey = input.required<string>();

  protected searchClicked = output<ActorInvoiceMatchesFilterDTO>();

  protected readonly form = new ActorInvoiceMatchesFilterForm({
    validationHandler: this.validationHandler,
    element: new ActorInvoiceMatchesFilterDTO(),
  });

  protected moreFilterOpened = signal(false);
  protected moreFilterLabel = computed(() => {
    return this.moreFilterOpened() ? 'common.showless' : 'common.showmore';
  });

  ngOnInit(): void {
    this.form.actorId.setValidatorTermKey(this.actorLabelKey());

    this.form.amountFrom.valueChanges
      .pipe(takeUntil(this._destroy$))
      .subscribe(value => {
        if (
          value &&
          this.form.amountTo.value &&
          value > this.form.amountTo.value
        ) {
          this.form.amountTo.setValue(null);
        }
      });

    this.form.amountTo.valueChanges
      .pipe(takeUntil(this._destroy$))
      .subscribe(value => {
        if (
          value &&
          this.form.amountFrom.value &&
          value < this.form.amountFrom.value
        ) {
          this.form.amountFrom.setValue(null);
        }
      });

    this.form.dateFrom.valueChanges
      .pipe(takeUntil(this._destroy$))
      .subscribe(value => {
        if (value && this.form.dateTo.value && value > this.form.dateTo.value) {
          this.form.dateTo.setValue(null);
        }
      });

    this.form.dateTo.valueChanges
      .pipe(takeUntil(this._destroy$))
      .subscribe(value => {
        if (
          value &&
          this.form.dateFrom.value &&
          value < this.form.dateFrom.value
        ) {
          this.form.dateFrom.setValue(null);
        }
      });
  }

  ngOnDestroy(): void {
    this._destroy$.next();
    this._destroy$.complete();
  }

  protected moreFilterExpanded(isOpened: boolean): void {
    this.moreFilterOpened.set(isOpened);
  }

  protected triggerSearch(): void {
    this.searchClicked.emit(
      <ActorInvoiceMatchesFilterDTO>this.form.getRawValue()
    );
  }
}

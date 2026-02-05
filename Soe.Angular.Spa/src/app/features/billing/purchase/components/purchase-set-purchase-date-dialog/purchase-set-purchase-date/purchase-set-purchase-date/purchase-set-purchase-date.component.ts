import {
  Component,
  EventEmitter,
  Input,
  OnDestroy,
  OnInit,
  Output,
  inject,
} from '@angular/core';
import { ValidationHandler } from '@shared/handlers';
import {
  Feature,
  SoeOriginStatus,
} from '@shared/models/generated-interfaces/Enumerations';
import { PurchaseSetPurchaseDateForm } from '../../../../models/purchase-set-purchase-date-form.model';
import {
  PurchaseSetPurchaseDateDTO,
  ReturnSetPurchaseDateDialog,
} from '../../../../models/purchase.model';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { IPurchaseRowDTO } from '@shared/models/generated-interfaces/PurchaseDTOs';
import { BehaviorSubject, Subscription, take } from 'rxjs';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { TranslateService } from '@ngx-translate/core';
import { PurchaseRowDTO } from '@features/billing/purchase/models/purchase-rows.model';

@Component({
  selector: 'soe-purchase-set-purchase-date',
  templateUrl: './purchase-set-purchase-date.component.html',
  styleUrls: ['./purchase-set-purchase-date.component.scss'],
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class PurchaseSetPurchaseDateComponent
  extends GridBaseDirective<IPurchaseRowDTO>
  implements OnInit, OnDestroy
{
  @Input() purchaseRowData = new BehaviorSubject<PurchaseRowDTO[]>([]);

  @Input() newStatus!: SoeOriginStatus;
  @Input() confirmedDeliveryDate!: Date;
  @Input() useConfirmed!: boolean;
  @Output() dateChange: EventEmitter<ReturnSetPurchaseDateDialog> =
    new EventEmitter<ReturnSetPurchaseDateDialog>();
  dateLabel = '';
  propName = '';
  _dateHead!: Date;
  originalDateSet = false;
  originalDate!: any;
  validationHandler = inject(ValidationHandler);
  translationService = inject(TranslateService);
  formSetPurchaseDate: PurchaseSetPurchaseDateForm =
    new PurchaseSetPurchaseDateForm({
      validationHandler: this.validationHandler,
      element: new PurchaseSetPurchaseDateDTO(),
    });
  purchaseRowsChanges: PurchaseRowDTO[] = [];

  purchaseRowDataSubscription!: Subscription;

  get dateHead() {
    return this._dateHead;
  }
  set dateHead(date) {
    if (!date) return;
    this._dateHead = new Date(date);
    this.purchaseRowsChanges.forEach(r => {
      if (this.propName === 'accDeliveryDate') {
        if (
          !r.accDeliveryDate ||
          (((this._dateHead && this._dateHead.isSameDay(r.accDeliveryDate)) ||
            (this.confirmedDeliveryDate &&
              this.confirmedDeliveryDate.isAfter(r.accDeliveryDate))) &&
            !r.isLocked)
        ) {
          if (!this.originalDateSet) {
            this.originalDate = r.accDeliveryDate;
            this.originalDateSet = true;
          }
          r.accDeliveryDate = new Date(date);
        }
      } else if (this.propName === 'wantedDeliveryDate') {
        if (
          !r.wantedDeliveryDate ||
          (((this._dateHead &&
            this._dateHead.isSameDay(r.wantedDeliveryDate)) ||
            (this.confirmedDeliveryDate &&
              this.confirmedDeliveryDate.isAfter(r.wantedDeliveryDate))) &&
            !r.isLocked)
        ) {
          if (!this.originalDateSet) {
            this.originalDate = r.wantedDeliveryDate;
            this.originalDateSet = true;
          }
          r.wantedDeliveryDate = new Date(date);
        }
      }
    });

    this.setData();
  }

  ngOnInit() {
    super.ngOnInit();
    this.purchaseRowDataSubscription = this.purchaseRowData.subscribe(data => {
      this.purchaseRowsChanges = data;
    });
    if (
      this.newStatus === SoeOriginStatus.PurchaseAccepted ||
      this.useConfirmed
    ) {
      if (this.useConfirmed && this.confirmedDeliveryDate)
        this._dateHead = this.confirmedDeliveryDate;

      this.propName = 'accDeliveryDate';
    } else if (this.newStatus === SoeOriginStatus.Origin) {
      this.propName = 'wantedDeliveryDate';
    }
    this.startFlow(
      Feature.Billing_Purchase_Purchase_Edit,
      'billing.purchase.rows',
      {
        skipInitialLoad: true,
        useLegacyToolbar: true,
      }
    );
  }

  override createLegacyGridToolbar(): void {
    super.createLegacyGridToolbar({
      hideReload: true,
      hideClearFilters: true,
    });
  }

  override onGridReadyToDefine(grid: GridComponent<IPurchaseRowDTO>) {
    super.onGridReadyToDefine(grid);

    this.translationService
      .get([
        'common.name',
        'core.saving',
        'core.aggrid.totals.filtered',
        'core.aggrid.totals.total',
        'core.aggrid.totals.selected',
        'billing.productrows.productnr',
        'billing.purchaserows.accdeliverydate',
        'billing.purchaserows.wanteddeliverydate',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnText(
          'productNr',
          terms['billing.productrows.productnr'],
          { flex: 1 }
        );
        this.grid.addColumnText('text', terms['common.name'], { flex: 1 });

        if (this.newStatus === SoeOriginStatus.PurchaseAccepted) {
          this.grid.addColumnDate(
            'accDeliveryDate',
            terms['billing.purchaserows.accdeliverydate'],
            {
              enableHiding: false,
              flex: 1,
              editable: data => {
                return !data.data?.isLocked;
              },
            }
          );
          this.dateLabel = terms['billing.purchaserows.accdeliverydate'];
        } else if (this.newStatus === SoeOriginStatus.Origin) {
          this.grid.addColumnDate(
            'wantedDeliveryDate',
            terms['billing.purchaserows.wanteddeliverydate'],
            {
              enableHiding: false,
              flex: 1,
              editable: data => {
                return !data.data?.isLocked;
              },
            }
          );
          this.dateLabel = terms['billing.purchaserows.wanteddeliverydate'];
        }

        super.finalizeInitGrid();

        this.setData();
      });
  }

  setData() {
    this.grid.setData(this.purchaseRowsChanges);
  }

  changedPurchaseDate(selectedDate?: Date) {
    if (selectedDate) {
      this.dateHead = selectedDate;
      setTimeout(() => {
        this.dateChange.emit(
          new ReturnSetPurchaseDateDialog(
            this.dateHead,
            this.originalDate,
            this.originalDateSet,
            this.propName,
            this.purchaseRowsChanges
          )
        );
      }, 200);
    }
  }

  ngOnDestroy() {
    this.purchaseRowDataSubscription.unsubscribe();
  }
}

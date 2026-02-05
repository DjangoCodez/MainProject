import {
  Component,
  inject,
  Input,
  OnChanges,
  OnDestroy,
  OnInit,
  output,
  SimpleChanges,
} from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { IInventoryTraceViewDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ProgressService } from '@shared/services/progress/progress.service';
import { BrowserUtil } from '@shared/util/browser-util';
import { Perform } from '@shared/util/perform.class';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { Observable, Subject, take, takeUntil } from 'rxjs';
import { VoucherEditComponent } from '../../../voucher/components/voucher-edit/voucher-edit.component';
import { VoucherForm } from '../../../voucher/models/voucher-form.model';
import { InventoriesService } from '../../services/inventories.service';

@Component({
  selector: 'soe-inventories-tracing-grid',
  templateUrl: './inventories-tracing-grid.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class InventoriesTracingGridComponent
  extends GridBaseDirective<IInventoryTraceViewDTO>
  implements OnInit, OnChanges, OnDestroy
{
  @Input() inventoryId: number | undefined;
  @Input({ required: true }) reload$!: Observable<void>;
  traceRowsLoaded = output<IInventoryTraceViewDTO[]>();
  inventoriesService = inject(InventoriesService);
  readonly progressService = inject(ProgressService);

  private destroy$ = new Subject<void>();

  performInventoryTrace = new Perform<IInventoryTraceViewDTO[]>(
    this.progressService
  );

  ngOnInit() {
    super.ngOnInit();

    this.startFlow(
      Feature.Economy_Inventory_Inventories_Edit,
      'economy.inventory.inventories.inventorytraceview'
    );

    this.reload$.pipe(takeUntil(this.destroy$)).subscribe(() => {
      this.refreshGrid();
    });
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes.inventoryId && changes.inventoryId.currentValue) {
      this.refreshGrid();
    }
  }

  override onGridReadyToDefine(grid: GridComponent<IInventoryTraceViewDTO>) {
    super.onGridReadyToDefine(grid);

    this.translate
      .get([
        'common.date',
        'common.type',
        'common.amount',
        'economy.accounting.voucher.voucher',
        'economy.inventory.inventories.invoice',
        'core.edit',
        'core.aggrid.totals.filtered',
        'core.aggrid.totals.total',
        'core.aggrid.totals.selected',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnDate('date', terms['common.date'], {
          flex: 1,
          enableHiding: false,
        });
        this.grid.addColumnText('typeName', terms['common.type'], {
          flex: 1,
          enableHiding: false,
        });
        this.grid.addColumnNumber('amount', terms['common.amount'], {
          flex: 1,
          decimals: 2,
          enableHiding: false,
        });
        this.grid.addColumnText(
          'voucherNr',
          terms['economy.accounting.voucher.voucher'],
          {
            flex: 1,
            enableHiding: false,
            alignRight: true,
          }
        );
        this.grid.addColumnText(
          'invoiceNr',
          terms['economy.inventory.inventories.invoice'],
          {
            flex: 1,
            enableHiding: false,
            alignRight: true,
          }
        );

        this.grid.addColumnIcon('', '', {
          flex: 1,
          iconName: 'pen',
          iconClass: 'pen',
          showIcon: row => {
            return this.showEdit(row);
          },
          onClick: (row: IInventoryTraceViewDTO) => {
            if (row.voucherHeadId && row.voucherHeadId > 0) {
              this.openVoucher(row.voucherHeadId);
            } else if (row.invoiceId && row.invoiceId > 0 && row.type == 1) {
              this.openSupplierInvoice(row.invoiceId);
            }
          },
        });

        this.grid.context.suppressGridMenu = true;
        super.finalizeInitGrid({ hidden: true });
      });
  }

  private showEdit(row: IInventoryTraceViewDTO): boolean {
    if (row.voucherHeadId || row.invoiceId) return true;
    return false;
  }

  private openSupplierInvoice(id?: number) {
    if (id)
      BrowserUtil.openInNewWindow(
        window,
        `/soe/economy/supplier/invoice/status/?invoiceId=${id}`
      );
  }

  openVoucher(voucherHeadId: number) {
    this.openEditInNewTab.emit({
      id: voucherHeadId,
      additionalProps: {
        editComponent: VoucherEditComponent,
        editTabLabel: 'economy.accounting.voucher.voucher',
        FormClass: VoucherForm,
      },
    });
  }

  override loadData(
    id?: number | undefined,
    additionalProps?: any
  ): Observable<IInventoryTraceViewDTO[]> {
    return this.performInventoryTrace.load$(
      this.inventoriesService.getInventoryTraceViews(this.inventoryId || 0)
    );
  }

  override onAfterLoadData(data: IInventoryTraceViewDTO[]) {
    this.traceRowsLoaded.emit(data ?? []);
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }
}

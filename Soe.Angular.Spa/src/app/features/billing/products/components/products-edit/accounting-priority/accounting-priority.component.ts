import { Component, OnInit, effect, inject, input, model } from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { SoeFormGroup } from '@shared/extensions';
import { SmallGenericType } from '@shared/models/generic-type.model';
import {
  Feature,
  TermGroup,
} from '@shared/models/generated-interfaces/Enumerations';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { CellValueChangedEvent } from 'ag-grid-community';
import { Observable, take, tap } from 'rxjs';
import { PriorityAccountRow } from '../../../models/product.model';

@Component({
  selector: 'soe-accounting-priority',
  templateUrl: './accounting-priority.component.html',
  styleUrls: ['./accounting-priority.component.scss'],
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class AccountingPriorityComponent
  extends GridBaseDirective<PriorityAccountRow>
  implements OnInit
{
  productAccountPriorityRows = model.required<PriorityAccountRow[]>();
  form = input.required<SoeFormGroup>();
  readonly = input<boolean>();

  private readonly coreService = inject(CoreService);
  private accountingPrios: SmallGenericType[] = [];

  constructor() {
    super();
    effect((): void => {
      const rows = this.productAccountPriorityRows();
      setTimeout(() => {
        this.rowData.next(rows);
      }, 50);
    });
  }

  ngOnInit(): void {
    super.ngOnInit();
    this.startFlow(
      Feature.Billing_Product_Products_Edit,
      'Billing.Products.Products.Views.AccountingPriority',
      {
        skipInitialLoad: true,
        lookups: [this.loadAccountingPriority()],
      }
    );
  }

  override onGridReadyToDefine(grid: GridComponent<PriorityAccountRow>): void {
    super.onGridReadyToDefine(grid);

    this.grid.api.updateGridOptions({
      onCellValueChanged: this.onCellValueChanged.bind(this),
    });

    this.translate
      .get([
        'billing.products.products.dimname',
        'billing.products.products.priority',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnText(
          'dimName',
          terms['billing.products.products.dimname'],
          { width: 165, suppressFilter: true }
        );
        this.grid.addColumnSelect(
          'prioNr',
          terms['billing.products.products.priority'],
          this.accountingPrios,
          undefined,
          {
            dropDownIdLabel: 'id',
            dropDownValueLabel: 'name',
            editable: true,
            flex: 1,
            suppressFilter: true,
          }
        );

        this.grid.setNbrOfRowsToShow(5, 5);
        this.grid.context.suppressGridMenu = true;
        this.grid.context.suppressFiltering = true;
        super.finalizeInitGrid({ hidden: true });
        this.grid.updateGridHeightBasedOnNbrOfRows();
      });
  }

  private loadAccountingPriority(): Observable<SmallGenericType[]> {
    return this.coreService
      .getTermGroupContent(TermGroup.InvoiceProductAccountingPrio, true, false)
      .pipe(tap(x => (this.accountingPrios = x)));
  }

  private onCellValueChanged({
    newValue,
    oldValue,
    colDef,
    data,
  }: CellValueChangedEvent): void {
    if (newValue === oldValue) return;

    if (colDef.field === 'prioNr') {
      const accPrio = this.accountingPrios.find(x => x.id === newValue);
      if (accPrio) {
        this.productAccountPriorityRows.update(rows => {
          const row = rows.find(z => z.dimNr === <number>data.dimNr);
          if (row) {
            row.prioNr = accPrio.id;
            row.prioName = accPrio.name;
            this.form().markAsDirty();
          }
          return rows;
        });
      }
    }
  }
}

import {
  Component,
  EventEmitter,
  inject,
  Input,
  OnChanges,
  OnInit,
  Output,
  signal,
  SimpleChanges,
} from '@angular/core';
import { ProjectForm } from '@features/billing/project/models/project-form.model';
import { ProjectService } from '@features/billing/project/services/project.service';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { SoeFormGroup } from '@shared/extensions';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import { IPriceListDTO } from '@shared/models/generated-interfaces/PriceListDTOs';
import { IPriceListTypeDTO } from '@shared/models/generated-interfaces/PriceListTypeDTOs';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { CellValueChangedEvent } from 'ag-grid-enterprise';
import { BehaviorSubject, take } from 'rxjs';

@Component({
  selector: 'soe-project-pricelist',
  templateUrl: './project-pricelist.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class ProjectPricelistComponent
  extends GridBaseDirective<IPriceListDTO, any>
  implements OnInit, OnChanges
{
  @Input({ required: true }) form!: SoeFormGroup<ProjectForm>;
  @Input() priceList: IPriceListDTO[] = [];
  @Input() comparisonPriceLists!: ISmallGenericType[];
  @Input() projectPriceLists!: ISmallGenericType[];
  @Input() comparisonPricelistId!: number;
  @Input() pricelistId!: number;
  @Output() pricelistChanged = new EventEmitter<number>();
  @Output() comparisonPricelistChanged = new EventEmitter<number>();
  @Output() priceDateChanged = new EventEmitter<Date>();
  @Output() loadAllProductsChanged = new EventEmitter<boolean>();
  @Output() pricesChanged = new EventEmitter<boolean>();

  priceListRows = new BehaviorSubject<IPriceListDTO[]>([]);
  allPriceLists: IPriceListTypeDTO[] = [];

  pService = inject(ProjectService);

  ngOnInit(): void {
    super.ngOnInit();
    this.startFlow(
      Feature.None,
      'Billing.Projects.List.Directives.PriceLists',
      { skipInitialLoad: true }
    );
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes.priceList && changes.priceList.currentValue) {
      this.setGridData();
    }
  }

  override createGridToolbar(): void {
    super.createGridToolbar({
      clearFiltersOption: { hidden: signal(true) },
      reloadOption: { hidden: signal(true) },
    });
  }

  override onGridReadyToDefine(grid: GridComponent<IPriceListDTO>): void {
    super.onGridReadyToDefine(grid);

    const keys: string[] = [
      'billing.projects.list.productnr',
      'common.name',
      'billing.projects.list.purchaseprice',
      'billing.projects.list.comparisonprice',
      'billing.projects.list.price',
      'billing.products.pricelists.startdate',
      'billing.products.pricelists.stopdate',
      'core.aggrid.totals.filtered',
      'core.aggrid.totals.total',
    ];

    this.grid.api.updateGridOptions({
      onCellValueChanged: this.onCellChanged.bind(this),
    });

    this.translate
      .get(keys)
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnModified('isModified');
        this.grid.addColumnText(
          'number',
          terms['billing.projects.list.productnr'],
          { flex: 1 }
        );
        this.grid.addColumnText('name', terms['common.name'], { flex: 1 });
        this.grid.addColumnNumber(
          'purchasePrice',
          terms['billing.projects.list.purchaseprice'],
          { flex: 1, decimals: 2 }
        );
        this.grid.addColumnNumber(
          'comparisonPrice',
          terms['billing.projects.list.comparisonprice'],
          { flex: 1, decimals: 2 }
        );
        this.grid.addColumnNumber(
          'price',
          terms['billing.projects.list.price'],
          { flex: 1, editable: true, enableHiding: false, decimals: 2 }
        );
        this.grid.addColumnDate(
          'startDateDisplay',
          terms['billing.products.pricelists.startdate'],
          { flex: 1 }
        );
        this.grid.addColumnDate(
          'stopDateDisplay',
          terms['billing.products.pricelists.stopdate'],
          { flex: 1 }
        );

        // this.grid.api.setRowCount(15);
        this.grid.setNbrOfRowsToShow(5, 5);
        this.grid.context.suppressGridMenu = true;
        super.finalizeInitGrid();
      });
  }

  private setGridData() {
    this.priceListRows.next(this.priceList);
    this.grid?.refreshCells();
  }

  onPricelistChanged(value: number) {
    this.pricelistChanged.emit(value);
  }

  onCellChanged(event: CellValueChangedEvent) {
    const { colDef, data, newValue, oldValue } = event;
    if (newValue !== oldValue && newValue) {
      this.rowChanged(data);
    }
  }

  rowChanged(row: any) {
    row.isModified = true;
    this.grid.refreshCells();
    this.pricesChanged.emit(true);
  }
}

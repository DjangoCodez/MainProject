import { ColDef, GridSizeChangedEvent } from 'ag-grid-community';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { BehaviorSubject } from 'rxjs';

@Component({
  selector: 'soe-dynamic-grid',
  templateUrl: './dynamic-grid.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class DynamicGridComponent extends GridBaseDirective<any> {
  @Input() gridRef?: GridComponent<any>;
  @Input({ required: true }) gridName = '';
  @Input() height = 300;
  @Input() suppressHeader = false;
  @Input() minRowsToShow = 10;
  @Input() maxRowsToShow = 10;
  @Input() suppressGridMenu = false;
  @Input() suppressFiltering = false;
  @Input() suppressRowCount = false;
  @Input() fitColumnsToWidth = true;
  @Input() columns: ColDef[] = [];
  @Input() gridData = new BehaviorSubject<any[]>([]);
  @Input() enableRowSelection = false;
  @Input() isSingleRowSelection: boolean = false;
  @Output() rowSelectionChanged = new EventEmitter<any[]>();
  @Output() gridRefChange = new EventEmitter<GridComponent<any>>();

  private isDefined = false;
  private _height = 0;

  constructor(public flowHandler: FlowHandlerService) {
    super();
    this.startFlow(Feature.None, this.gridName, { skipInitialLoad: true });
  }

  onGridReadyToDefine(grid: GridComponent<any>): void {
    if (!this.isDefined) {
      grid.api.updateGridOptions({
        onGridSizeChanged: (event: GridSizeChangedEvent): void => {
          this._height = event.clientHeight;
          if (this._height > 0 && this.fitColumnsToWidth)
            this.grid?.api.sizeColumnsToFit();
        },
        onDisplayedColumnsChanged: (): void => {
          if (this._height > 0 && this.fitColumnsToWidth)
            this.grid?.api.sizeColumnsToFit();
        },
      });

      super.onGridReadyToDefine(grid);

      this.grid.columns = this.columns;
      if (this.enableRowSelection)
        this.grid?.enableRowSelection(undefined, this.isSingleRowSelection);

      this.grid.setNbrOfRowsToShow(this.minRowsToShow, this.maxRowsToShow);

      if (this.suppressHeader) this.grid.headerHeight.set(0);
      else this.grid.headerHeight.set(32);
      if (this.suppressGridMenu) this.grid.context.suppressGridMenu = true;
      if (this.suppressFiltering) this.grid.context.suppressFiltering = true;

      super.finalizeInitGrid({
        hidden: this.suppressRowCount,
      });
      this.isDefined = true;
      this.gridRefChange.emit(this.grid);
    }
  }

  selectionChanged(rows: any[]): void {
    this.rowSelectionChanged.emit(rows);
  }
}

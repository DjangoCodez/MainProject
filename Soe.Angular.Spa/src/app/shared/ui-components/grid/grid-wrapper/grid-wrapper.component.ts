import {
  Component,
  computed,
  input,
  Input,
  output,
  ViewChild,
} from '@angular/core';
import { AutoHeightDirective } from '@shared/directives/auto-height/auto-height.directive';
import { AutoHeightService } from '@shared/directives/auto-height/auto-height.service';
import { Dict } from '@ui/grid/services/selected-item.service'
import { GridComponent } from '@ui/grid/grid.component'
import { ToolbarComponent } from '@ui/toolbar/toolbar.component'
import { ToolbarGroups, ToolbarItemGroupConfig } from '@ui/toolbar/models/toolbar';
import { CellClickedEvent, CellKeyDownEvent } from 'ag-grid-community';
import { BehaviorSubject } from 'rxjs';

@Component({
  selector: 'soe-grid-wrapper',
  imports: [GridComponent, ToolbarComponent, AutoHeightDirective],
  templateUrl: './grid-wrapper.component.html',
  styleUrls: ['./grid-wrapper.component.scss'],
  providers: [AutoHeightService],
})
export class GridWrapperComponent<T> {
  toolbarItemGroups = input<ToolbarItemGroupConfig[]>([]);
  toolbarGroups = input<ToolbarGroups[]>([]);
  @Input() rows = new BehaviorSubject<T[]>([]);
  height = input(0);
  masterDetail = input(false);
  parentGuid = input.required<string>();
  noMargin = input(false);
  toolbarNoPadding = input(false);
  toolbarNoTopBottomPadding = input(false);
  toolbarNoMargin = input(false);
  toolbarNoBorder = input(false);
  selectedItemsChanged = output<Dict>();
  rowSelected = output<T | undefined>();
  selectionChanged = output<T[]>();
  editRowClicked = output<T>();
  cellKeyDown = output<CellKeyDownEvent>();
  cellClicked = output<CellClickedEvent>();

  setAutoHeight = computed(() => this.height() === 0);

  @ViewChild(GridComponent)
  grid!: GridComponent<T>;

  edit($event: T): void {
    this.editRowClicked.emit($event);
  }

  triggerRowSelected(row: T | undefined): void {
    this.rowSelected.emit(row);
  }

  triggerSelectionChanged(rows: T[]): void {
    this.selectionChanged.emit(rows);
  }

  triggerSelectedItemsChanged(items: Dict): void {
    this.selectedItemsChanged.emit(items);
  }

  triggerCellKeyDown($event: CellKeyDownEvent) {
    this.cellKeyDown.emit($event);
  }

  triggerCellClicked($event: CellClickedEvent) {
    this.cellClicked.emit($event);
  }
}

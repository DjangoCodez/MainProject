import { NumberUtil } from '@shared/util/number-util';
import { AgGridAngular } from 'ag-grid-angular';
import { SortDirection } from 'ag-grid-community';
import { GridComponent } from '../grid.component';
import { filter } from 'lodash';

export class SortUtil<T> {
  private agGrid: AgGridAngular;

  constructor(private grid: GridComponent<T>) {
    this.agGrid = grid.agGrid;
  }

  sort(field: string, direction: SortDirection) {
    this.agGrid.api.applyColumnState({
      state: [
        {
          colId: field,
          sort: direction,
        },
      ],
      defaultState: { sort: null },
    });
  }

  sortFirst(sortColumnName = '') {
    if (!sortColumnName) sortColumnName = 'sort';

    const rows = this.agGrid.rowData;
    const currentRow = this.getCurrentRow();
    if (rows && currentRow && (<any>currentRow)[sortColumnName] > 1) {
      // Move row to the top
      (<any>currentRow)[sortColumnName] = -1;
      this.reNumberRows(sortColumnName);
      this.resetRows(currentRow);
    }
  }

  sortUp(sortColumnName = '') {
    if (!sortColumnName) sortColumnName = 'sort';

    const rows = this.agGrid.rowData;
    const currentRow = this.getCurrentRow();
    if (rows && currentRow && (<any>currentRow)[sortColumnName] > 1) {
      const filterObj: any = {};
      filterObj[sortColumnName] = (<any>currentRow)[sortColumnName] - 1;
      // Get previous row
      const prevRow = filter(rows, filterObj)[0];
      // Move row up
      if (prevRow) {
        // Multiply each row number by 10, to be able to insert row numbers in between
        this.multiplyRowNr(rows, sortColumnName);

        // Move current row before previous row
        (<any>currentRow)[sortColumnName] -= 19;
        this.reNumberRows(sortColumnName);
        this.resetRows(currentRow);
      }
    }
  }

  sortDown(sortColumnName = '') {
    if (!sortColumnName) sortColumnName = 'sort';

    const rows = this.agGrid.rowData;
    const currentRow = this.getCurrentRow();
    if (rows && currentRow && (<any>currentRow)[sortColumnName] < rows.length) {
      const filterObj: any = {};
      filterObj[sortColumnName] = (<any>currentRow)[sortColumnName] + 1;
      // Get next row
      const nextRow = filter(rows, filterObj)[0];
      // Move row down
      if (nextRow) {
        // Multiply each row number by 10, to be able to insert row numbers in between
        this.multiplyRowNr(rows, sortColumnName);

        // Move current row after next row
        (<any>currentRow)[sortColumnName] += 12;
        this.reNumberRows(sortColumnName);
        this.resetRows(currentRow);
      }
    }
  }

  sortLast(sortColumnName = '') {
    if (!sortColumnName) sortColumnName = 'sort';

    const rows = this.agGrid.rowData;
    const currentRow = this.getCurrentRow();
    if (rows && currentRow && (<any>currentRow)[sortColumnName] < rows.length) {
      // Move row to the bottom
      (<any>currentRow)[sortColumnName] =
        NumberUtil.max(rows, sortColumnName) + 2;
      this.reNumberRows(sortColumnName);
      this.resetRows(currentRow);
    }
  }

  getCurrentRow(): T | undefined {
    let row = undefined;
    // Get focused cell
    const cellItem = this.agGrid.api.getFocusedCell();
    let rowNode = null;
    if (cellItem)
      rowNode = this.agGrid.api.getDisplayedRowAtIndex(cellItem?.rowIndex);
    if (rowNode) {
      row = rowNode.data;
    }
    return row;
  }

  reNumberRows(sortColumnName = '') {
    if (!sortColumnName) sortColumnName = 'sort';

    //this sorts inline, keeping all data-bindings and references intact.
    let rows = this.agGrid.rowData;
    if (rows) {
      rows = this.sortRows(rows, sortColumnName);
      let i = 0;
      rows.forEach((row: T) => {
        i++;
        if ((<any>row)[sortColumnName] !== i) {
          (<any>row)[sortColumnName] = i;
          (<any>row).isModified = true;
        }
      });
      this.agGrid.rowData = rows;
    }
  }

  private sortRows(rows: T[], sortColumnName = '') {
    if (!sortColumnName) sortColumnName = 'sort';

    const sortable = Array.from(rows);
    sortable.sort((r1: T, r2: T) => {
      if ((<any>r1)[sortColumnName] < (<any>r2)[sortColumnName]) return -1;
      if ((<any>r1)[sortColumnName] > (<any>r2)[sortColumnName]) return 1;
      return 0;
    });
    const sorted: T[] = [];
    const i = 0;
    for (const r in sortable) {
      // eslint-disable-next-line no-prototype-builtins
      if (sortable.hasOwnProperty(r)) {
        sorted.push(sortable[r]);
      }
    }
    return sorted;
  }

  resetRows(row?: T, sortColumnName = '') {
    if (!sortColumnName) sortColumnName = 'sort';

    let rows: T[] = [];
    const data = this.agGrid.rowData;
    if (data) rows = data;

    rows = this.sortRows(rows, sortColumnName);
    this.grid.setData(rows);

    if (row) {
      this.scrollToFocus(row, sortColumnName);
    }
  }

  scrollToFocus(row: T, columnName?: string) {
    const rows = this.agGrid.rowData;
    if (rows) {
      const index: number = rows.indexOf(row);
      this.agGrid.api.forEachNode(node => {
        if (node.rowIndex == index) {
          this.agGrid.api.clearFocusedCell();
          node.setSelected(true, true);
          this.agGrid.api.ensureIndexVisible(node.rowIndex, 'middle');
          if (columnName)
            this.agGrid.api.setFocusedCell(node.rowIndex, columnName);
        }
      });
    }
  }

  private multiplyRowNr(rows: T[], sortColumnName = '') {
    if (rows) {
      rows.forEach((row: T) => {
        if (sortColumnName) (<any>row)[sortColumnName] *= 10;
      });
    }
  }

  sortIndexNrOnDragEnd(sortFieldName: string) {
    let i = 1;
    const newRows: any[] = [];
    this.grid.getFilteredRows().forEach((row: any) => {
      if (row[sortFieldName] !== i) row.isModified = true;
      row[sortFieldName] = i;
      newRows.push(row);
      i++;
    });
    this.grid.setData(newRows);
  }
}

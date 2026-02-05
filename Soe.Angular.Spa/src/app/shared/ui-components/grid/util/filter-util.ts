import { AgGridAngular } from 'ag-grid-angular';
import { GridComponent } from '../grid.component';
import { IDefaultFilterSettings } from '../interfaces';
import { SoeColumnType } from './column-util';
import { ColDef, GridApi } from 'ag-grid-enterprise';

export class FilterUtil<T> {
  private api: GridApi;

  constructor(private gridApi: GridApi) {
    this.api = gridApi;
  }

  getFilterModel(): { [key: string]: any } {
    return this.api?.getFilterModel();
  }

  setFilter(field: string, filterModel: any) {
    this.api?.setColumnFilterModel(field, filterModel);
    this.api.onFilterChanged();
  }

  setDefaultFilter(
    defaultFilter: IDefaultFilterSettings | undefined,
    columns: ColDef[]
  ) {
    // If active column is used, set default filter to only show active records
    const activeColumn = columns.find(
      c => c.context?.soeColumnType === SoeColumnType.Active
    );
    if (activeColumn) {
      this.setFilter(activeColumn.field || 'state', {
        values: ['true'],
      });
    }

    if (defaultFilter) {
      this.setFilter(defaultFilter.field, defaultFilter.filterModel);
    }
  }

  clearFilters() {
    this.api?.setFilterModel(null);
  }
}

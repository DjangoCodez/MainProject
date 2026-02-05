// https://www.ag-grid.com/angular-data-grid/column-menu/

import { SoeConfigUtil } from '@shared/util/soeconfig-util';
import { GridComponent } from '../grid.component';
import { SoeColDefContext, SoeColumnType } from './column-util';
import { GridExportType } from './export-util';
import { GridResizeType } from '../enums/resize-type.enum';
import { ColDef, MenuItemDef, SideBarDef } from 'ag-grid-community';

export declare type GridMenuItem = MenuItemDef | 'separator';

export class GridMenuUtil {
  gridMenuItems: GridMenuItem[] = [];

  constructor(private grid: GridComponent<any>) {}

  // GRID MENU

  createGridMenu(): ColDef {
    return {
      context: {
        soeColumnType: SoeColumnType.Text,
        suppressExport: true,
      } as SoeColDefContext,
      field: 'soe-grid-menu-column',
      headerName: '',
      pinned: 'right',
      width: 22,
      sortable: false,
      suppressSizeToFit: true,
      suppressMovable: true,
      filter: false,
      floatingFilter: false,
      resizable: false,
      suppressNavigable: true,
      suppressColumnsToolPanel: true,
      suppressHeaderMenuButton: false,
      menuTabs: ['generalMenuTab', 'columnsMenuTab'],
    };
  }

  createDefaultMenu(
    gridName: string,
    addPdfExportOption = false,
    addExcelTableExportOption = true
  ): GridMenuItem[] {
    this.gridMenuItems = [];

    this.addGridMenuItem({
      name: this.grid.getLocaleText('soe.clearFilters'),
      icon: this.createGridMenuIcon('fa-filter-slash'),
      action: () => this.grid.clearFilters(),
    });

    this.addGridMenuItem('separator');

    // Resize controls
    this.addGridMenuItem({
      name: this.grid.getLocaleText('soe.resizeToFit'),
      icon: this.createGridMenuIcon('fa-columns'),
      action: () => this.grid.resizeColumns(GridResizeType.ToFit),
    });

    this.addGridMenuItem({
      name: this.grid.getLocaleText('soe.resizeAutoAllExceptHeaders'),
      icon: this.createGridMenuIcon('fa-columns'),
      action: () =>
        this.grid.resizeColumns(GridResizeType.AutoAllExceptHeaders),
    });

    this.addGridMenuItem({
      name: this.grid.getLocaleText('soe.resizeAutoAllAndHeaders'),
      icon: this.createGridMenuIcon('fa-columns'),
      action: () => this.grid.resizeColumns(GridResizeType.AutoAllAndHeaders),
    });

    if (!gridName) {
      if (SoeConfigUtil.isSupportAdmin) {
        this.addGridMenuItem('separator');
        this.addGridMenuItem({
          name: 'GRID NAME MISSING',
          icon: this.createGridMenuIcon('fa-exclamation-triangle error-color'),
        });
        this.addGridMenuItem({
          name: 'Make sure the grid extends GridBaseDirective',
        });
      }
    } else {
      this.addGridMenuItem('separator');

      // this.addGridMenuItem({
      //   name: this.grid.getLocaleText('soe.export') + ':',
      //   disabled: true,
      // });
      this.addGridMenuItem({
        name: this.grid.getLocaleText('soe.exportAllExcel'),
        icon: this.createGridMenuIcon('fa-file-excel'),
        action: () => this.grid.exportRows(GridExportType.Excel, true),
      });
      this.addGridMenuItem({
        name: this.grid.getLocaleText('soe.exportFilteredExcel'),
        icon: this.createGridMenuIcon('fa-file-excel'),
        action: () => this.grid.exportRows(GridExportType.Excel),
      });
      if (addExcelTableExportOption) {
        // https://www.ag-grid.com/angular-data-grid/excel-export-tables/
        this.addGridMenuItem({
          name: this.grid.getLocaleText('soe.exportAllExcelTable'),
          icon: this.createGridMenuIcon('fa-file-xls'),
          action: () => this.grid.exportRows(GridExportType.ExcelTable, true),
        });
        this.addGridMenuItem({
          name: this.grid.getLocaleText('soe.exportFilteredExcelTable'),
          icon: this.createGridMenuIcon('fa-file-xls'),
          action: () => this.grid.exportRows(GridExportType.ExcelTable),
        });
      }
      this.addGridMenuItem({
        name: this.grid.getLocaleText('soe.exportAllCsv'),
        icon: this.createGridMenuIcon('fa-file-csv'),
        action: () => this.grid.exportRows(GridExportType.Csv, true),
      });
      this.addGridMenuItem({
        name: this.grid.getLocaleText('soe.exportFilteredCsv'),
        icon: this.createGridMenuIcon('fa-file-csv'),
        action: () => this.grid.exportRows(GridExportType.Csv),
      });

      if (addPdfExportOption) {
        this.addGridMenuItem({
          name: this.grid.getLocaleText('soe.exportAllPdf'),
          icon: this.createGridMenuIcon('fa-file-pdf'),
          action: () => this.grid.exportRows(GridExportType.Pdf),
        });
      }

      this.addGridMenuItem('separator');

      // this.addGridMenuItem({
      //   name: this.grid.getLocaleText('soe.gridState') + ':',
      //   disabled: true,
      // });

      if (SoeConfigUtil.isSupportAdmin) {
        this.addGridMenuItem({
          name: this.grid.getLocaleText('soe.gridStateSaveDefault'),
          icon: this.createGridMenuIcon('fa-save'),
          action: () => {
            this.grid.initSaveDefaultGridState();
          },
        });
        this.addGridMenuItem({
          name: this.grid.getLocaleText('soe.gridStateDeleteDefault'),
          icon: this.createGridMenuIcon('fa-times icon-delete'),
          action: () => {
            this.grid.initDeleteDefaultGridState();
          },
        });
      }
      this.addGridMenuItem({
        name: this.grid.getLocaleText('soe.gridStateSave'),
        icon: this.createGridMenuIcon('fa-save'),
        action: () => this.grid.saveUserGridState(),
      });
      this.addGridMenuItem({
        name: this.grid.getLocaleText('soe.gridStateDefault'),
        icon: this.createGridMenuIcon('fa-columns'),
        action: () => this.grid.restoreGridStateToDefault(),
      });
      this.addGridMenuItem({
        name: this.grid.getLocaleText('soe.gridStateDelete'),
        icon: this.createGridMenuIcon('fa-undo icon-delete'),
        action: () => this.grid.deleteUserGridState(),
      });
    }

    return this.gridMenuItems;
  }

  private createGridMenuIcon(iconName: string): string {
    return `<span class='fal ${iconName}' style='width: 100%; text-align: center;' />`;
  }

  private addGridMenuItem(
    menuItem: GridMenuItem,
    colId = 'soe-grid-menu-column'
  ) {
    this.gridMenuItems.push(menuItem);
  }

  // SIDEBAR

  enableSideBar() {
    // https://www.ag-grid.com/angular-data-grid/side-bar/
    // https://www.ag-grid.com/angular-data-grid/tool-panel-columns/

    const sideBarDef: SideBarDef = {
      toolPanels: [
        {
          id: 'columns',
          labelDefault: 'Columns',
          labelKey: 'columns',
          iconKey: 'columns',
          toolPanel: 'agColumnsToolPanel',
          toolPanelParams: {
            suppressPivots: true,
            suppressPivotMode: true,
            suppressRowGroups: true,
            suppressValues: true,
          },
          minWidth: 225,
          maxWidth: 225,
          width: 225,
        },
      ],
      defaultToolPanel: 'columns',
    };
    this.grid.agGrid.api.updateGridOptions({ sideBar: sideBarDef });
  }

  isSideBarVisible(): boolean {
    return this.grid.agGrid.api.isSideBarVisible();
  }

  setSideBarVisible(value: boolean) {
    this.grid.agGrid.api.updateGridOptions({ sideBar: value });
  }

  openToolPanel(key: string) {
    this.grid.agGrid.api.openToolPanel(key);
  }

  closeToolPanel() {
    this.grid.agGrid.api.closeToolPanel();
  }
}

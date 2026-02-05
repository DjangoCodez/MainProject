import {
  AfterViewInit,
  Component,
  OnInit,
  inject,
  signal,
} from '@angular/core';
import { EmbeddedGridBaseDirective } from '@shared/directives/grid-base/embedded-grid-base.directive';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { GridComponent } from '@ui/grid/grid.component';
import { MessageboxService } from '@ui/dialog/services/messagebox.service';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { Observable, of } from 'rxjs';
import { UiComponentsTestForm } from '../../models/ui-components-test-form.model';

export class GroupedHeadersGridTestDataDTO {
  col1: string;
  col2: string;
  col3: string;
  col4: string;
  col5: number;
  col6: number;
  typeId: number;

  constructor() {
    this.col1 = '';
    this.col2 = '';
    this.col3 = '';
    this.col4 = '';
    this.col5 = 0;
    this.col6 = 0;
    this.typeId = 0;
  }
}

@Component({
  selector: 'soe-grouped-headers-grid-test',
  templateUrl:
    '../../../../../../shared/ui-components/grid/grid-wrapper/embedded-grid-wrapper-template.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class GroupedHeadersGridTestComponent
  extends EmbeddedGridBaseDirective<
    GroupedHeadersGridTestDataDTO,
    UiComponentsTestForm
  >
  implements OnInit, AfterViewInit
{
  readonly flowHandler = inject(FlowHandlerService);
  messageboxService = inject(MessageboxService);

  constructor() {
    super();
  }

  ngAfterViewInit() {
    // this.setupGrid(this.grid);
  }

  ngOnInit() {
    super.ngOnInit();
    this.startFlow(Feature.None, 'Manage.System.GroupedGrid');
  }

  override createGridToolbar(): void {
    super.createGridToolbar({
      showSorting: true,
      hideNew: true,
      sortingField: 'index',
    });
    this.toolbarService.createItemGroup({
      alignLeft: true,
      items: [
        this.toolbarService.createToolbarLabel('label', {
          labelKey: signal('Grouped headers and single row select'),
        }),
      ],
    });
  }

  getData(): Observable<GroupedHeadersGridTestDataDTO[]> {
    // testdata
    return of([
      {
        index: 1,
        col1: 'Stockholm',
        col2: 'Sweden',
        col3: 'Europe',
        col4: 'Coastal',
        col5: 188,
        col6: 979000,
        typeId: 1,
        isModified: false,
      } as GroupedHeadersGridTestDataDTO,
      {
        index: 2,
        col1: 'Söderhamn',
        col2: 'Sweden',
        col3: 'Europe',
        col4: 'Coastal',
        col5: 10.36,
        col6: 12000,
        typeId: 1,
        isModified: false,
      } as GroupedHeadersGridTestDataDTO,
      {
        index: 3,
        col1: 'Colombo',
        col2: 'Sri Lanka',
        col3: 'Asia',
        col4: 'Coastal',
        col5: 198.7,
        col6: 248000,
        typeId: 2,
        isModified: false,
      } as GroupedHeadersGridTestDataDTO,
      {
        index: 4,
        col1: 'Helsinki',
        col2: 'Finland',
        col3: 'Europe',
        col4: 'Coastal',
        col5: 213.8,
        col6: 659000,
        typeId: 0,
        isModified: false,
      } as GroupedHeadersGridTestDataDTO,
      {
        index: 5,
        col1: 'New York',
        col2: 'USA',
        col3: 'North America',
        col4: 'Coastal',
        col5: 783.8,
        col6: 8336000,
        typeId: 0,
        isModified: false,
      } as GroupedHeadersGridTestDataDTO,
      {
        index: 6,
        col1: 'Beijing',
        col2: 'China',
        col3: 'Asia',
        col4: 'Inland',
        col5: 16411,
        col6: 21450000,
        typeId: 0,
        isModified: false,
      } as GroupedHeadersGridTestDataDTO,
    ]);
  }

  override onGridReadyToDefine(
    grid: GridComponent<GroupedHeadersGridTestDataDTO>
  ) {
    super.onGridReadyToDefine(grid);
    this.grid = grid;
    // set testdata
    this.getData().subscribe(rows => {
      this.rowData.next(rows);
    });

    const header1 = this.grid.addColumnHeader('', 'Geography');
    const header2 = this.grid.addColumnHeader('', 'Facts', {
      tooltip: 'Some facts..',
    });

    this.grid.enableRowSelection(undefined, true);
    this.grid.addColumnModified('isModified');
    this.grid.addColumnNumber('index', 'Index', {
      width: 40,
      rowDragable: true,
      headerColumnDef: header1,
      sortable: true,
    });
    this.grid.addColumnText('col1', 'City', {
      flex: 1,
      showSetFilter: true,
      headerColumnDef: header1,
    });
    this.grid.addColumnText('col2', 'Country', {
      flex: 1,
      headerColumnDef: header1,
    });
    this.grid.addColumnText('col3', 'Continent', {
      flex: 1,
      maxWidth: 200,
      headerColumnDef: header1,
    });
    this.grid.addColumnText('col4', 'Type of location', {
      flex: 1,
      headerColumnDef: header2,
    });
    this.grid.addColumnNumber('col5', 'Area (km2)', {
      decimals: 2,
      flex: 1,
      headerColumnDef: header2,
    });
    this.grid.addColumnNumber('col6', 'Population', {
      decimals: 2,
      flex: 1,
      resizable: false,
      headerColumnDef: header2,
    });
    this.grid.addColumnIconEdit({
      flex: 1,
      tooltip: 'Edit',
      enableHiding: true,
      onClick: (row: any) => {
        this.messageboxService.information(
          'Edit',
          'Editing post with name of city: ' + row.col1
        );
      },
    });

    this.grid.setRowSelection('singleRow');

    this.grid.applyDragOptions({
      rowDragFinishedSortIndexNrFieldName: 'index',
      hideContentOnDrag: true,
    });

    super.finalizeInitGrid({
      termTotal: 'Total',
      termFiltered: 'Filtered',
      tooltip: 'Nr of items',
    });
  }
}

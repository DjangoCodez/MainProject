import { Component, OnInit, inject, signal } from '@angular/core';
import { EmbeddedGridBaseDirective } from '@shared/directives/grid-base/embedded-grid-base.directive';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ColumnUtil } from '@ui/grid/util/column-util';
import { GridComponent } from '@ui/grid/grid.component';
import { MessageboxService } from '@ui/dialog/services/messagebox.service';
import { ToolbarEmbeddedGridConfig } from '@ui/toolbar/models/toolbar';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { CellClassParams } from 'ag-grid-community';
import { Observable, of } from 'rxjs';
import { UiComponentsTestForm } from '../../models/ui-components-test-form.model';

export class MasterDetailGridTestDataDTO {
  col1: number;
  col2: string;
  col3: string;
  col4: string;
  col5: string;
  typeId: number;

  constructor() {
    this.col1 = 0;
    this.col2 = '';
    this.col3 = '';
    this.col4 = '';
    this.col5 = '';
    this.typeId = 0;
  }
}

@Component({
  selector: 'soe-master-detail-grid-test',
  templateUrl:
    '../../../../../../shared/ui-components/grid/grid-wrapper/embedded-grid-wrapper-template.html', //'./master-detail-grid.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class MasterDetailGridTestComponent
  extends EmbeddedGridBaseDirective<
    MasterDetailGridTestDataDTO,
    UiComponentsTestForm
  >
  implements OnInit
{
  readonly flowHandler = inject(FlowHandlerService);
  messageboxService = inject(MessageboxService);

  constructor() {
    super();
  }

  ngOnInit() {
    super.ngOnInit();
    this.startFlow(Feature.None, 'Manage.System.MasterDetailGrid');
  }

  getData(): Observable<MasterDetailGridTestDataDTO[]> {
    // testdata
    return of([
      {
        col1: 55,
        col2: 'Testing 1-1',
        col3: 'Testing 1-2',
        col4: 'Testing 1-3',
        col5: 'Type 1',
        typeId: 1,
      } as MasterDetailGridTestDataDTO,
      {
        col1: 2,
        col2: 'Testing 2-1',
        col3: 'Testing 2-2',
        col4: 'Testing 2-3',
        col5: 'Type 2',
        typeId: 1,
      } as MasterDetailGridTestDataDTO,
      {
        col1: 7,
        col2: 'Testing 3-1',
        col3: 'Testing 3-2',
        col4: 'Testing 3-3',
        col5: 'Type 2',
        typeId: 2,
      } as MasterDetailGridTestDataDTO,
      {
        col1: 99,
        col2: 'Testing 4-1',
        col3: 'Testing 4-2',
        col4: 'Testing 4-3',
        col5: 'Type 3',
        typeId: 0,
      } as MasterDetailGridTestDataDTO,
      {
        col1: 21,
        col2: 'Testing 5-1, spans two columns',
        col3: 'Testing 5-2',
        col4: 'Testing 5-3',
        col5: 'Type 1',
        typeId: 3,
      } as MasterDetailGridTestDataDTO,
    ]);
  }

  override createGridToolbar(
    config?: Partial<ToolbarEmbeddedGridConfig>
  ): void {
    super.createGridToolbar();
    this.toolbarService.createItemGroup({
      alignLeft: true,
      items: [
        this.toolbarService.createToolbarLabel('label', {
          labelKey: signal('Expandable grid (master/detail)'),
        }),
      ],
    });
  }

  override onGridReadyToDefine(
    grid: GridComponent<MasterDetailGridTestDataDTO>
  ): void {
    super.onGridReadyToDefine(grid);

    this.grid = grid;
    // set testdata
    this.getData().subscribe(rows => {
      this.rowData.next(rows);
    });

    //Details
    this.grid.enableMasterDetail(
      {
        detailRowHeight: 300,

        columnDefs: [
          this.grid.addColumnNumber('id', 'Id', {
            flex: 1,
            suppressFilter: true,
            buttonConfiguration: {
              iconPrefix: 'fal',
              iconName: 'pen',
              iconClass: 'warning-color',
              tooltip: 'Click to bring up information about this row',
              onClick: (data: any) => {
                console.log(data);
                this.messageboxService.warning(
                  'Warning',
                  'Id: ' + data.id + ' with value: ' + data.value
                );
              },
              show: () => {
                return true;
              },
            },
            returnable: true,
          }),
          this.grid.addColumnText('value', 'Value', {
            flex: 9,
            returnable: true,
          }),
        ],
      },
      {
        // Sub level definition
        detailOptions: {
          detailRowHeight: 200,
          columnDefs: [
            this.grid.addColumnText('field1', 'Field 1', {
              flex: 1,
              returnable: true,
            }),
            this.grid.addColumnText('field2', 'Field 2', {
              flex: 1,
              returnable: true,
            }),
          ],
        },
        detailContext: {
          // Sub sub level definition
          detailOptions: {
            detailRowHeight: 200,
            columnDefs: [
              this.grid.addColumnText('id', 'Id', {
                width: 100,
                returnable: true,
              }),
              this.grid.addColumnText('text', 'Text', {
                flex: 1,
                returnable: true,
              }),
            ],
          },
          detailContext: {
            getDetailRowData: (params: any) => {
              this.loadSubSubDetailRows(params);
            },
          },
          getDetailRowData: (params: any) => {
            this.loadSubDetailRows(params);
          },
        },
        autoHeight: false,
        getDetailRowData: (params: any) => {
          this.loadDetailRows(params);
        },
      }
    );

    // Testing cellClassRules
    const nameCellRules = {
      'warning-background-color': (params: CellClassParams) =>
        params.value == 'Testing 2-1',
    };

    this.grid.addColumnNumber('col1', 'Number', {
      flex: 1,
      showSetFilter: true,
      aggFuncOnGrouping: 'sum',
    });
    this.grid.addColumnText('col2', 'Text', {
      flex: 3,
      cellClassRules: nameCellRules,
      strikeThrough: (row: any) => row.value == 'Testing 4-1', // Testing strikethrough
      colSpan: (params: any) => (params.data?.col1 === 21 ? 2 : 1),
    });
    this.grid.addColumnText('col3', 'Text with shape', {
      flex: 1,
      maxWidth: 200,
      tooltip: 'Some text',
      shapeConfiguration: {
        shape: 'circle',
        color: 'red',
        width: 16,
        tooltip: 'A simple shape',
      },
    });
    this.grid.addColumnText('col4', 'Text with iconbutton', {
      flex: 1,
      buttonConfiguration: {
        iconPrefix: 'fal',
        iconName: 'starfighter',
        iconClass: 'information-color',
        tooltip: 'Click to bring up information about this row',
        onClick: data => {
          this.messageboxService.information(
            'Information',
            data.col2 + ' with a number of ' + data.col1
          );
        },
        show: () => {
          return true;
        },
      },
    });
    this.grid.addColumnText('col5', 'Text for grouping', {
      flex: 1,
      enableGrouping: true,
    });
    this.grid.addColumnIconEdit({
      flex: 1,
      tooltip: 'Edit',
      enableHiding: true,
      onClick: (row: any) => {
        this.messageboxService.information(
          'Edit',
          'Editing post with Text: ' + row.col2
        );
      },
    });

    this.grid.setExportExcelOptions({
      groupedTotals: true,
      rowGroupExpandState: 'expanded',
      termGroupedSubTotal: 'Delsumma',
      termGroupedGrandTotal: 'Total',
    });
    this.grid.addGroupTimeSpanSumAggFunction(true);
    this.grid.enableGroupFooter();
    this.grid.enableGroupTotalFooter();
    this.grid.showGroupPanel();
    this.grid.finalizeInitGrid({
      termTotal: 'Total',
      termFiltered: 'Filtered',
      tooltip: 'Nr of items',
    });
  }

  loadDetailRows(params: any) {
    const data = [
      {
        id: '1',
        value: 'This is the first detailrow',
      },
      {
        id: '2',
        value: 'This is the second detailrow',
      },
      {
        id: '3',
        value: 'This is the third detailrow',
      },
    ];
    params.successCallback(data);
  }

  loadSubDetailRows(params: any) {
    const data = [
      {
        field1: 'Sub level row 1',
        field2: 'This is the first sub-detailrow',
      },
      {
        field1: 'Sub level row 2',
        field2: 'This is the second sub-detailrow',
      },
      {
        field1: 'Sub level row 3',
        field2: 'This is the third sub-detailrow',
      },
    ];
    params.successCallback(data);
  }

  loadSubSubDetailRows(params: any) {
    const data = [
      {
        id: '5',
        text: 'This is the text for record with id 5',
      },
      {
        id: '31',
        text: 'This is the text for record with id 31',
      },
    ];
    params.successCallback(data);
  }
}

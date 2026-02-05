import { Component, Input, OnInit, signal } from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { AG_NODE, GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { CellValueChangedEvent } from 'ag-grid-community';
import { BehaviorSubject, take } from 'rxjs';
import { SysCompanyUniqueValueDTO } from 'src/app/features/manage/models/sysCompany.model';
import { SysCompanyForm } from '../../../models/sys-company-form.model';

@Component({
  selector: 'soe-sys-company-unique-values-grid',
  templateUrl: './sys-company-unique-values-grid.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class SysCompanyUniqueValuesGridComponent
  extends GridBaseDirective<SysCompanyUniqueValueDTO>
  implements OnInit
{
  @Input({ required: true }) uniqueValues = new BehaviorSubject<
    SysCompanyUniqueValueDTO[]
  >([]);
  @Input({ required: true }) form: SysCompanyForm | undefined;

  uniqueValueTypes: SmallGenericType[] = [
    {
      id: 0,
      name: 'Unknown',
    },
    {
      id: 1,
      name: 'Email',
    },
  ];

  ngOnInit(): void {
    super.ngOnInit();
    this.startFlow(
      Feature.Manage_System,
      'Soe.Manage.System.SysCompany.SysCompany.UniqueValues',
      {
        skipInitialLoad: true,
      }
    );
  }

  override createGridToolbar(): void {
    super.createGridToolbar({
      hideReload: true,
    });

    this.toolbarService.createItemGroup({
      items: [
        this.toolbarService.createToolbarButton('newrow', {
          iconName: signal('plus'),
          caption: signal('common.newrow'),
          tooltip: signal('common.newrow'),
          onAction: () => this.onToolbarButtonClick(),
        }),
      ],
    });
  }

  onToolbarButtonClick(): void {
    const row = new SysCompanyUniqueValueDTO();
    this.form?.addUniqueValue(row);
    this.grid?.addRow(row);
    this.grid?.clearSelectedRows();
  }

  override onGridReadyToDefine(
    grid: GridComponent<SysCompanyUniqueValueDTO>
  ): void {
    super.onGridReadyToDefine(grid);

    this.grid.api.updateGridOptions({
      onCellValueChanged: this.onCellValueChanged.bind(this),
    });

    this.translate
      .get(['common.type', 'common.value', 'core.delete'])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnModified('isModifed');

        this.grid.addColumnSelect(
          'uniqueValueType',
          terms['common.type'],
          this.uniqueValueTypes,
          undefined,
          {
            dropDownIdLabel: 'id',
            dropDownValueLabel: 'name',
            flex: 1,
            editable: true,
          }
        );

        this.grid.addColumnText('value', terms['common.value'], {
          flex: 1,
          editable: true,
        });

        this.grid.addColumnIconDelete({
          tooltip: terms['core.delete'],
          onClick: row => {
            this.deleteRow(row);
          },
        });
        this.grid.setNbrOfRowsToShow(10);
        super.finalizeInitGrid();
      });
  }

  onCellValueChanged(event: CellValueChangedEvent): void {
    if (
      event.rowIndex !== null &&
      event.rowIndex !== undefined &&
      event.rowIndex >= 0
    ) {
      event.data.isModifed = true;
      this.form?.updateUniqueValue(event.rowIndex, event.data);
    }
  }

  deleteRow(row: AG_NODE<SysCompanyUniqueValueDTO>): void {
    this.form?.deleteUniqueValue(+row.AG_NODE_ID);
    this.grid?.deleteRow(row);
    this.grid?.clearSelectedRows();
  }
}

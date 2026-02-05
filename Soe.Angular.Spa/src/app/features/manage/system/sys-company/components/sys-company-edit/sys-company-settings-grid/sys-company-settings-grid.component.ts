import { Component, Input, OnInit, signal } from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { Perform } from '@shared/util/perform.class';
import { AG_NODE, GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { CellValueChangedEvent } from 'ag-grid-community';
import { orderBy } from 'lodash';
import { BehaviorSubject, Observable, of, take } from 'rxjs';
import { tap } from 'rxjs/operators';
import {
  SysCompanySettingDTO,
  SysCompanySettingType,
} from 'src/app/features/manage/models/sysCompany.model';
import { SysCompanyForm } from '../../../models/sys-company-form.model';

@Component({
  selector: 'soe-sys-company-settings-grid',
  templateUrl: './sys-company-settings-grid.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class SysCompanySettingsGridComponent
  extends GridBaseDirective<SysCompanySettingDTO>
  implements OnInit
{
  @Input({ required: true }) settingRows = new BehaviorSubject<
    SysCompanySettingDTO[]
  >([]);
  @Input({ required: true }) form: SysCompanyForm | undefined;
  private readonly performLoad = new Perform(this.progressService);

  rowData = new BehaviorSubject<SysCompanySettingDTO[]>([]);
  companySettings: SmallGenericType[] = [];

  ngOnInit(): void {
    super.ngOnInit();
    this.startFlow(
      Feature.Manage_System,
      'Soe.Manage.System.SysCompany.SysCompany.SettingRows',
      {
        skipInitialLoad: true,
        lookups: [this.loadSettings()],
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
    const row = new SysCompanySettingDTO();
    row.sysCompanySettingId = 0;
    //row.settingType = SysCompanySettingType.WholesellerCustomerNumber;
    row.sysCompanyId = this.form?.sysCompanyId.value;
    row.stringValue = '';
    row.intValue = undefined;
    row.decimalValue = undefined;
    row.boolValue = false;

    this.form?.addCompanySetting(row);
    this.grid?.addRow(row);
    this.grid?.clearSelectedRows();
  }

  private loadSettings(): Observable<unknown> {
    this.companySettings = [];
    return this.performLoad.load$(
      of(Object.entries(SysCompanySettingType)).pipe(
        tap(
          (settingTypes: [string, string | SysCompanySettingType][]): void => {
            for (const [key, value] of settingTypes) {
              if (!isNaN(Number(key))) {
                this.companySettings.push(
                  new SmallGenericType(Number(key), String(value))
                );
              }
            }

            this.companySettings = orderBy(
              this.companySettings,
              ['name'],
              ['asc']
            );
          }
        )
      )
    );
  }

  override onGridReadyToDefine(
    grid: GridComponent<SysCompanySettingDTO>
  ): void {
    super.onGridReadyToDefine(grid);

    this.grid.api.updateGridOptions({
      onCellValueChanged: this.onCellValueChanged.bind(this),
    });

    this.translate
      .get([
        'common.type',
        'common.string',
        'common.int',
        'common.decimal',
        'common.bool',
        'core.delete',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnSelect(
          'settingType',
          terms['common.type'],
          this.companySettings,
          undefined,
          {
            dropDownIdLabel: 'id',
            dropDownValueLabel: 'name',
            flex: 1,
            editable: true,
          }
        );
        this.grid.addColumnText('stringValue', terms['common.string'], {
          flex: 1,
          editable: true,
        });
        this.grid.addColumnNumber('intValue', terms['common.int'], {
          flex: 1,
          editable: true,
        });
        this.grid.addColumnNumber('decimalValue', terms['common.decimal'], {
          flex: 1,
          editable: true,
        });
        this.grid.addColumnBool('boolvalue', terms['common.bool'], {
          flex: 1,
          editable: true,
          onClick: (data, row): void => {
            row.boolValue = data;
            this.form?.updateCompanySetting(+row.AG_NODE_ID, row);
          },
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
      this.form?.updateCompanySetting(event.rowIndex, event.data);
    }
  }

  deleteRow(row: AG_NODE<SysCompanySettingDTO>): void {
    this.form?.deleteCompanySetting(+row.AG_NODE_ID);
    this.grid?.deleteRow(row);
    this.grid?.clearSelectedRows();
  }
}

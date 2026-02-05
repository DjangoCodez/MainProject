import { Component, inject, Input, OnInit } from '@angular/core';
import { FieldSettingsForm } from '../../../models/field-settings-form.model';
import { EditBaseDirective } from '@shared/directives/edit-base/edit-base.directive';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import {
  IFieldSettingDTO,
  IRoleFieldSettingDTO,
} from '@shared/models/generated-interfaces/FieldSettingDTO';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { FieldSettingsService } from '../../../services/field-settings.service';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { CellKeyDownEvent, CellValueChangedEvent } from 'ag-grid-community';
import { GridComponent } from '@ui/grid/grid.component';
import { BehaviorSubject, take } from 'rxjs';

@Component({
  selector: 'soe-field-settings-edit-grid',
  templateUrl: './field-settings-edit-grid.component.html',
  providers: [FlowHandlerService],
  standalone: false,
})
export class FieldSettingsEditGridComponent
  extends EditBaseDirective<
    IFieldSettingDTO,
    FieldSettingsService,
    FieldSettingsForm
  >
  implements OnInit
{
  @Input({ required: true }) form!: FieldSettingsForm;
  @Input({ required: true }) yesNoValues!: SmallGenericType[];

  readonly service = inject(FieldSettingsService);

  subData = new BehaviorSubject<RoleFieldSettingDTO[]>([]);
  subGrid!: GridComponent<RoleFieldSettingDTO>;

  ngOnInit() {
    super.ngOnInit();

    this.flowHandler.execute({
      permission: Feature.Manage_Preferences_FieldSettings_Edit,
      setupGrid: this.setupRowsGrid.bind(this),
    });

    this.form?.valueChanges.subscribe(v => {
      this.initRows(v.roleSettings);
    });
  }

  subGridVisibleChanged(row: any) {
    const rowData = row.data;
    if (!rowData) return;
    const obj = this.yesNoValues.find((d: any) => {
      return d.id == row.data.visibleId;
    });
    if (!obj) return;

    const visibleValue = this.getVisibleBoolFromId(obj.id);

    const roleSettings = this.form?.value.roleSettings;
    const setting = roleSettings.find(
      (setting: any) => setting.roleId === rowData.roleId
    );

    if (setting) {
      setting.visible = visibleValue;

      this.form?.patchValue({
        roleSettings: roleSettings,
      });
    }
  }

  private getVisibleBoolFromId(value: number | undefined): boolean | null {
    if (value === 1) return true;
    else if (value === 0) return false;
    else return null;
  }

  private getVisibleIdFromBool(value: boolean | undefined): number {
    const id = value === true ? 1 : value === false ? 0 : 2;
    return this.yesNoValues.find((x: SmallGenericType) => x.id === id)?.id ?? 2;
  }

  onCellValueChanged(evt: CellValueChangedEvent): void {
    this.form?.markAsDirty();
  }

  onCellKeyDown(event: CellKeyDownEvent) {
    this.form?.markAsDirty();
  }

  private setupRowsGrid(grid: GridComponent<RoleFieldSettingDTO>) {
    this.subGrid = grid;

    this.translate
      .get(['common.role', 'manage.preferences.fieldsettings.fieldshown'])
      .pipe(take(1))
      .subscribe(terms => {
        this.subGrid.addColumnText('roleName', terms['common.role'], {
          editable: false,
          flex: 50,
        });
        this.subGrid.addColumnSelect(
          'visibleId',
          terms['manage.preferences.fieldsettings.fieldshown'],
          this.yesNoValues,
          this.subGridVisibleChanged.bind(this),
          {
            editable: true,
            dropDownIdLabel: 'id',
            dropDownValueLabel: 'name',
            flex: 50,
          }
        );

        this.subGrid.context.suppressGridMenu = true;
        this.subGrid.context.suppressFiltering = true;

        this.subGrid.finalizeInitGrid();
      });
  }

  private initRows(rows: RoleFieldSettingDTO[]) {
    if (rows.length === 0) return;

    this.subGrid.setNbrOfRowsToShow(1, rows.length + 1);

    const subData = rows.map(x => {
      x['roleName'] = x.roleName;
      x['visibleId'] = this.getVisibleIdFromBool(x.visible ?? undefined);

      return x;
    });

    this.subData.next(subData);
  }
}

class RoleFieldSettingDTO implements IRoleFieldSettingDTO {
  roleId: number;
  roleName: string;
  label: string;
  visible?: boolean;
  skipTabStop?: boolean;
  readOnly?: boolean;
  boldLabel?: boolean;
  visibleName: string;
  visibleId: number;

  constructor() {
    this.roleId = 0;
    this.roleName = '';
    this.label = '';
    this.visible = undefined;
    this.skipTabStop = undefined;
    this.readOnly = undefined;
    this.boldLabel = undefined;
    this.visibleName = '';
    this.visibleId = 0;
  }
}

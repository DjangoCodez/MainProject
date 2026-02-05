import { Component, Input, OnDestroy, OnInit } from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { IEmployeeGroupTimeLeisureCodeSettingDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { take, BehaviorSubject, takeUntil, Subject } from 'rxjs';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { LeisureCodesForm } from '../../models/leisure-codes-form.model';

@Component({
  selector: 'soe-leisure-codes-edit-grid',
  templateUrl: './leisure-codes-settings-grid.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class LeisureCodesEditGridComponent
  extends GridBaseDirective<IEmployeeGroupTimeLeisureCodeSettingDTO>
  implements OnInit, OnDestroy
{
  @Input({ required: true }) form!: LeisureCodesForm;

  settingRows: IEmployeeGroupTimeLeisureCodeSettingDTO[] = [];
  rows = new BehaviorSubject<IEmployeeGroupTimeLeisureCodeSettingDTO[]>([]);

  private _destroy$ = new Subject<void>();

  gridHeight = 200;

  override ngOnInit(): void {
    super.ngOnInit();

    this.startFlow(
      Feature.Time_Preferences_ScheduleSettings_LeisureCode_Edit,
      '',
      { skipInitialLoad: true }
    );

    this.form?.settings.valueChanges
      .pipe(takeUntil(this._destroy$))
      .subscribe(rows => {
        if (!this.form?.settings.dirty) {
          this.settingRows = <IEmployeeGroupTimeLeisureCodeSettingDTO[]>(
            rows.filter(
              (x: IEmployeeGroupTimeLeisureCodeSettingDTO) =>
                x.employeeGroupTimeLeisureCodeSettingId !== null
            )
          );
          this.setGridData();
        }
      });

    this.settingRows = <IEmployeeGroupTimeLeisureCodeSettingDTO[]>(
      this.form?.settings.value
    );
    this.setGridData();
  }

  onGridReadyToDefine(
    grid: GridComponent<IEmployeeGroupTimeLeisureCodeSettingDTO>
  ) {
    super.onGridReadyToDefine(grid);

    this.translate
      .get(['common.type', 'common.value', 'core.delete'])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnText('name', terms['common.type'], {
          flex: 1,
        });
        this.grid.addColumnText('settingValue', terms['common.value'], {
          flex: 1,
        });
        this.grid.addColumnIconDelete({
          tooltip: terms['core.delete'],
          onClick: row => {
            this.deleteSetting(row);
          },
        });

        this.grid.context.suppressGridMenu = true;
        super.finalizeInitGrid();
      });
  }

  private setGridData() {
    const rows = this.settingRows;
    this.rows.next(rows);
  }

  deleteSetting(row: any) {
    const settingForm = this.form?.settings.controls.find(
      s =>
        s.employeeGroupTimeLeisureCodeSettingId.value ===
        row.employeeGroupTimeLeisureCodeSettingId
    );
    if (settingForm) {
      this.form?.removeSettingForm(settingForm);
      this.form?.markAsDirty();
    }
  }

  ngOnDestroy(): void {
    this._destroy$.next();
    this._destroy$.complete();
  }
}

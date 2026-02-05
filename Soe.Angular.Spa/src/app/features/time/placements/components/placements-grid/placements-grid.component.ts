import { Component, inject, OnDestroy, OnInit, signal } from '@angular/core';
import { EconomyService } from '@features/economy/services/economy.service';
import { EmployeeGroupsService } from '@features/time/employee-groups/services/employee-groups.service';
import { PlacementsRecalculateStatusDialogComponent } from '@shared/components/time/placements-recalculate-status-dialog/placements-recalculate-status-dialog.component';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { CrudActionTypeEnum } from '@shared/enums';
import { TermCollection } from '@shared/localization/term-types';
import {
  CompanySettingType,
  Feature,
} from '@shared/models/generated-interfaces/Enumerations';
import {
  IAccountDimDTO,
  IActivateScheduleControlDTO,
  IActivateScheduleGridDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { DateUtil } from '@shared/util/date-util';
import { SettingsUtil } from '@shared/util/settings-util';
import { SoeConfigUtil } from '@shared/util/soeconfig-util';
import { DialogData } from '@ui/dialog/models/dialog';
import { DialogService } from '@ui/dialog/services/dialog.service';
import { MessageboxService } from '@ui/dialog/services/messagebox.service';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { ToolbarCheckboxAction } from '@ui/toolbar/toolbar-checkbox/toolbar-checkbox.component';
import { Observable } from 'rxjs';
import { take, tap } from 'rxjs/operators';
import {
  IPlacementsControlDialogData,
  PlacementsControlDialogComponent,
} from '../../../../../shared/components/time/placements-control-dialog/placements-control-dialog.component';
import { PlacementsService } from '../../services/placements.service';
import { IActivationResult } from './placements-grid-footer/placements-grid-footer.component';

@Component({
  selector: 'soe-placements-grid',
  standalone: false,
  templateUrl: './placements-grid.component.html',
  providers: [FlowHandlerService, ToolbarService],
})
export class PlacementsGridComponent
  extends GridBaseDirective<IActivateScheduleGridDTO, PlacementsService>
  implements OnInit, OnDestroy
{
  // Services
  readonly service = inject(PlacementsService);
  private readonly coreService = inject(CoreService);
  private readonly messageboxService = inject(MessageboxService);
  private readonly economyService = inject(EconomyService);
  private readonly dialogService = inject(DialogService);
  private readonly employeeGroupService = inject(EmployeeGroupsService);

  // Subscriptions
  private selectionSubscription?: { unsubscribe: () => void };

  // Flags
  public selectedRowsSignal = signal<IActivateScheduleGridDTO[]>([]);
  private deleteMessage = signal('');
  private useAccountsHierarchy = signal(false);
  private defaultEmployeeAccountDimId = signal(0);
  private defaultEmployeeAccountDimName = signal('');
  private showOnlyLatest = signal(true);
  private editHiddenPermission = signal(false);
  private readonly hiddenEmployeeId = signal(0);

  // Data
  private employeeGroups: SmallGenericType[] = [];

  ngOnInit() {
    super.ngOnInit();

    this.startFlow(Feature.Time_Schedule_Placement, 'Time.Schedule.Activate', {
      lookups: [this.loadEmployeeGroups(), this.loadHiddenEmployeeId()],
      additionalModifyPermissions: [
        Feature.Time_Schedule_SchedulePlanning_TemplateSchedule_EditHiddenEmployee,
      ],
    });
  }

  ngOnDestroy(): void {
    this.selectionSubscription?.unsubscribe();
  }

  override onPermissionsLoaded(): void {
    this.editHiddenPermission.set(
      this.flowHandler.hasModifyAccess(
        Feature.Time_Schedule_SchedulePlanning_TemplateSchedule_EditHiddenEmployee
      )
    );
  }

  override createGridToolbar(): void {
    super.createGridToolbar();

    this.toolbarService.createItemGroup({
      alignLeft: true,
      items: [
        this.toolbarService.createToolbarCheckbox('onlyLatest', {
          labelKey: signal('time.schedule.activate.onlylatest'),
          checked: this.showOnlyLatest,
          onValueChanged: event =>
            this.showOnlyLatestChanged((event as ToolbarCheckboxAction).value),
        }),
      ],
    });
    this.toolbarService.createItemGroup({
      alignLeft: false,
      items: [
        this.toolbarService.createToolbarButton('activationStatus', {
          iconName: signal('calendar-check'),
          caption: signal('time.recalculatetimestatus'),
          tooltip: signal('time.recalculatetimestatus'),
          onAction: () => this.openActivationStatus(),
        }),
      ],
    });
  }

  override onGridReadyToDefine(grid: GridComponent<IActivateScheduleGridDTO>) {
    super.onGridReadyToDefine(grid);
    this.grid.enableRowSelection(
      row =>
        this.editHiddenPermission() ||
        (!!row.data && row.data.employeeId !== this.hiddenEmployeeId())
    );
    this.grid.addContextMenu(undefined, [
      this.grid.contextMenuService.deleteButton({
        disabled: params => {
          return !this.canDeleteRow(params?.node?.data);
        },
        action: params => {
          const row = params?.node?.data as IActivateScheduleGridDTO;
          if (row) this.deleteRow(row);
        },
      }),
    ]);

    this.grid.addColumnText(
      'employeeNr',
      this.terms['time.employee.employee.employeenrshort'],
      {
        flex: 5,
      }
    );
    this.grid.addColumnText('employeeName', this.terms['common.name'], {
      flex: 10,
    });
    this.grid.addColumnSelect(
      'employeeGroupId',
      this.terms['time.employee.employeegroup.employeegroup'],
      this.employeeGroups || [],
      null,
      {
        flex: 10,
      }
    );
    if (this.useAccountsHierarchy()) {
      this.grid.addColumnText(
        'accountNamesString',
        this.terms['time.employee.employee.accountswithdefault'],
        {
          flex: 15,
        }
      );
    } else {
      this.grid.addColumnText(
        'categoryNamesString',
        this.terms['time.employee.employee.categories'],
        {
          flex: 15,
        }
      );
    }
    this.grid.addColumnDate(
      'employmentEndDate',
      this.terms['time.employee.employee.employmentenddate'],
      {
        flex: 8,
      }
    );
    this.grid.addColumnBool(
      'isPlaced',
      this.terms['time.schedule.activate.isplaced'],
      {
        flex: 3,
      }
    );
    this.grid.addColumnText(
      'timeScheduleTemplateHeadName',
      this.terms['time.schedule.activate.templatename'],
      {
        flex: 15,
      }
    );
    this.grid.addColumnNumber(
      'employeeScheduleStartDayNumber',
      this.terms['time.schedule.activate.startday'],
      {
        flex: 4,
      }
    );
    this.grid.addColumnDate(
      'employeeScheduleStartDate',
      this.terms['common.startdate'],
      {
        flex: 8,
      }
    );
    this.grid.addColumnDate(
      'employeeScheduleStopDate',
      this.terms['common.stopdate'],
      {
        flex: 8,
      }
    );
    if (this.flowHandler.modifyPermission()) {
      this.grid.addColumnIconDelete({
        tooltip: this.terms['core.delete'],
        onClick: row => {
          this.deleteRow(row);
        },
        showIcon: row => this.canDeleteRow(row),
      });
    }

    this.selectionSubscription = this.grid.selectionChanged.subscribe(data =>
      this.selectedRowsSignal.set(data)
    );
    super.finalizeInitGrid();
  }
  private canDeleteRow(row: any) {
    return (
      row.isPlaced &&
      (this.editHiddenPermission() ||
        row.employeeId !== this.hiddenEmployeeId())
    );
  }

  // LOAD DATA
  override loadCompanySettings() {
    return this.coreService
      .getCompanySettings([
        CompanySettingType.UseAccountHierarchy,
        CompanySettingType.DefaultEmployeeAccountDimEmployee,
      ])
      .pipe(
        tap(x => {
          this.useAccountsHierarchy.set(
            SettingsUtil.getBoolCompanySetting(
              x,
              CompanySettingType.UseAccountHierarchy
            )
          );
          this.defaultEmployeeAccountDimId.set(
            SettingsUtil.getIntCompanySetting(
              x,
              CompanySettingType.DefaultEmployeeAccountDimEmployee
            )
          );
          if (this.useAccountsHierarchy()) {
            this.loadDefaultEmployeeAccount().subscribe();
          }
        })
      );
  }

  override loadTerms(translationsKeys?: string[]): Observable<TermCollection> {
    return super.loadTerms([
      'time.employee.employee.employeenrshort',
      'common.name',
      'time.employee.employeegroup.employeegroup',
      'time.employee.employee.categories',
      'time.employee.employee.accountswithdefault',
      'time.employee.employee.employmentenddate',
      'time.schedule.activate.isplaced',
      'time.schedule.activate.templatename',
      'time.schedule.activate.startday',
      'common.startdate',
      'common.stopdate',
      'core.delete',
      'time.schedule.activate.delete.message',
      'time.schedule.activate.delete.message.nocheck',
      'time.schedule.activate.delete.message.nocheck.info',
      'time.recalculatetimestatus.activateschedulecontrol',
      'time.schedule.activate.delete.message.hidden.info.category',
      'time.schedule.activate.delete.message.hidden.info.accountshierarchy',
      'time.schedule.activate.to',
    ]);
  }

  private loadDefaultEmployeeAccount(): Observable<IAccountDimDTO> {
    return this.economyService
      .getAccountDimByAccountDimId(this.defaultEmployeeAccountDimId(), false)
      .pipe(
        tap(x => {
          this.defaultEmployeeAccountDimName.set(x.name);
        })
      );
  }

  override loadData(
    id?: number
    // additionalProps?: any
  ): Observable<IActivateScheduleGridDTO[]> {
    return this.performLoadData
      .load$(
        this.service.getGrid(id, {
          onlyLatest: this.showOnlyLatest(),
          addEmptyPlacement: true,
        }),
        {
          showDialogDelay: 1000,
        }
      )
      .pipe(tap(data => {}));
  }

  private loadEmployeeGroups(): Observable<SmallGenericType[]> {
    return this.employeeGroupService.getEmployeeGroupsDict(false).pipe(
      tap(x => {
        this.employeeGroups = x;
      })
    );
  }

  private loadHiddenEmployeeId(): Observable<number> {
    return this.service.getHiddenEmployeeId().pipe(
      tap(x => {
        this.hiddenEmployeeId.set(x);
      })
    );
  }

  // EVENTS

  private deleteRow(row: IActivateScheduleGridDTO) {
    this.setDeleteMessage(row);

    this.messageboxService
      .question('time.schedule.activate.delete', this.deleteMessage(), {
        showInputCheckbox: SoeConfigUtil.isSupportAdmin,
        inputCheckboxLabel: 'time.schedule.activate.delete.message.nocheck',
      })
      .afterClosed()
      .subscribe(result => {
        if (result.result) {
          const rows: IActivateScheduleGridDTO[] = [];
          rows.push(row);
          this.performControlActivation(
            rows,
            undefined,
            undefined,
            true
          ).subscribe(control => {
            // Set based on question checkboxvalue
            control.discardCheckesAll = !!result.checkboxValue;
            if (!control.hasWarnings) {
              this.performDelete(control, row);
            } else {
              const dialogData: IPlacementsControlDialogData = {
                size: 'fullscreen',
                title:
                  this.terms['time.schedule.activate.to'] +
                  ' ' +
                  DateUtil.localeDateFormat(
                    row.employeeScheduleStartDate?.addDays(-1) as Date
                  ),
                control: control,
                disableClose: true,
              };
              this.dialogService
                .open(PlacementsControlDialogComponent, dialogData)
                .afterClosed()
                .subscribe(placementsControlResult => {
                  if (placementsControlResult) {
                    this.performDelete(placementsControlResult, row);
                  }
                });
            }
          });
        }
      });
  }

  private openActivationStatus() {
    this.translate
      .get(['time.recalculatetimestatus'])
      .pipe(take(1))
      .subscribe(terms => {
        const dialogData: DialogData = {
          size: 'fullscreen',
          title: terms['time.recalculatetimestatus'],
          disableClose: true,
        };
        this.dialogService.open(
          PlacementsRecalculateStatusDialogComponent,
          dialogData
        );
      });
  }

  private showOnlyLatestChanged(showOnlyLatest: boolean) {
    this.showOnlyLatest.set(showOnlyLatest);
    this.refreshGrid();
  }

  public onActivationFinished(activationResult: IActivationResult) {
    if (activationResult.activationSuccessful === true) {
      this.refreshGrid();
    }
  }

  // HELPER METHODS
  private setDeleteMessage(row: IActivateScheduleGridDTO) {
    this.deleteMessage.set(this.terms['time.schedule.activate.delete.message']);
    if (row.employeeHidden) {
      if (this.useAccountsHierarchy())
        this.deleteMessage.set(
          this.deleteMessage() +
            '\n<b>' +
            this.terms[
              'time.schedule.activate.delete.message.hidden.info.accountshierarchy'
            ] +
            ' ' +
            this.defaultEmployeeAccountDimName() +
            '</b>'
        );
      else
        this.deleteMessage.set(
          this.deleteMessage() +
            '\n<b>' +
            this.terms[
              'time.schedule.activate.delete.message.hidden.info.category'
            ] +
            '</b>'
        );
    }
    if (SoeConfigUtil.isSupportAdmin)
      this.deleteMessage.set(
        this.deleteMessage() +
          '\n\n' +
          this.terms['time.schedule.activate.delete.message.nocheck.info']
      );
  }

  private performDelete(
    control: IActivateScheduleControlDTO,
    row: IActivateScheduleGridDTO
  ) {
    return this.performLoadData.crud(
      CrudActionTypeEnum.Work,
      this.service.deletePlacement(control, row),
      result => {
        if (result.success) {
          this.refreshGrid();
        }
      }
    );
  }

  private performControlActivation(
    rows: IActivateScheduleGridDTO[],
    startDate?: Date,
    stopDate?: Date,
    isDelete?: boolean
  ): Observable<IActivateScheduleControlDTO> {
    return this.performLoadData.load$(
      this.service.controlActivations(rows, startDate, stopDate, isDelete)
    );
  }
}

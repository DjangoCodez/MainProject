import {
  AfterViewInit,
  Component,
  DestroyRef,
  ElementRef,
  inject,
  OnInit,
  signal,
  ViewChild,
} from '@angular/core';
import { ShortcutService } from '@core/services/shortcut.service';
import {
  DailyRecurrencePatternDialogComponent,
  DailyRecurrencePatternDialogResult,
} from '@shared/components/daily-recurrence-pattern-dialog/daily-recurrence-pattern-dialog.component';
import { DailyRecurrencePatternDialogData } from '@shared/components/daily-recurrence-pattern-dialog/models/daily-recurrence-pattern-dialog-form.model';
import { EditBaseDirective } from '@shared/directives/edit-base/edit-base.directive';
import { ShiftTypeService } from '@shared/features/shift-type/services/shift-type.service';
import { TermCollection } from '@shared/localization/term-types';
import { AccountDTO } from '@shared/models/account.model';
import { SmallGenericType } from '@shared/models/generic-type.model';
import {
  CompanySettingType,
  Feature,
  UserSettingType,
} from '@shared/models/generated-interfaces/Enumerations';
import { ITimeScheduleTaskDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import {
  DailyRecurrenceParamsDTO,
  DailyRecurrenceRangeDTO,
} from '@shared/models/recurrence.model';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { SharedService } from '@shared/services/shared.service';
import { DateUtil } from '@shared/util/date-util';
import { Perform } from '@shared/util/perform.class';
import { SettingsUtil } from '@shared/util/settings-util';
import { focusOnElement } from '@shared/util/focus-util';
import { DialogService } from '@ui/dialog/services/dialog.service';
import { MenuButtonItem } from '@ui/button/menu-button/menu-button.component';
import { TimeboxValue } from '@ui/forms/timebox/timebox.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { Observable, of, take, tap } from 'rxjs';
import { GeneratedNeedsDialogData } from '../../models/generated-needs-form.model';
import { TimeScheduleTasksForm } from '../../models/time-schedule-tasks-form.model';
import { TimeScheduleTasksService } from '../../services/time-schedule-tasks.service';
import { GeneratedNeedsComponent } from '../generated-needs/generated-needs.component';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';

enum SaveMenuFunction {
  Save = 1,
  SaveAndNew = 2,
  SaveAndClose = 3,
}

@Component({
  selector: 'soe-time-schedule-tasks-edit',
  templateUrl: './time-schedule-tasks-edit.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class TimeScheduleTasksEditComponent
  extends EditBaseDirective<
    ITimeScheduleTaskDTO,
    TimeScheduleTasksService,
    TimeScheduleTasksForm
  >
  implements OnInit, AfterViewInit
{
  service = inject(TimeScheduleTasksService);
  private coreService = inject(CoreService);
  private sharedService = inject(SharedService);
  private shortcutService = inject(ShortcutService);
  shiftTypeService = inject(ShiftTypeService);
  dialogService = inject(DialogService);
  performAccounts = new Perform<AccountDTO[]>(this.progressService);

  readonly DEFAULT_MIN_LENGTH = 15;
  useAccountsHierarchy = signal(false);
  minLength = signal(this.DEFAULT_MIN_LENGTH);
  lengthInfoLabel = '';

  accounts: SmallGenericType[] = [];
  saveMenuList: MenuButtonItem[] = [];
  private newAfterSave = false;
  private closeAfterSave = false;

  staffingNeedsReadPermission = signal(false);

  @ViewChild('nameField') nameField!: ElementRef;

  constructor(
    private element: ElementRef,
    private destroyRef: DestroyRef
  ) {
    super();
  }

  // INIT

  ngOnInit() {
    super.ngOnInit();

    this.startFlow(Feature.Time_Schedule_StaffingNeeds_Tasks, {
      additionalReadPermissions: [Feature.Time_Schedule_StaffingNeeds],
      lookups: [this.loadTaskTypes()],
    });

    this.form?.onlyOneEmployee.valueChanges.subscribe((value: boolean) => {
      // Min split length disabled if only one employee is selected
      if (value) this.form?.minSplitLengthFormatted.disable();
      else if (this.flowHandler.modifyPermission())
        this.form?.minSplitLengthFormatted.enable();
    });
  }

  ngAfterViewInit(): void {
    this.setupKeyboardShortcuts();
  }

  override onPermissionsLoaded() {
    super.onPermissionsLoaded();
    this.staffingNeedsReadPermission.set(
      this.flowHandler.hasReadAccess(Feature.Time_Schedule_StaffingNeeds)
    );
  }

  override loadTerms(): Observable<TermCollection> {
    return super.loadTerms([
      'core.time.short.minute',
      'common.insertformat',
      'core.save',
      'core.saveandnew',
      'core.saveandclose',
    ]);
  }

  override loadCompanySettings(): Observable<any> {
    return this.coreService
      .getCompanySettings([
        CompanySettingType.UseAccountHierarchy,
        CompanySettingType.TimeSchedulePlanningDayViewMinorTickLength,
      ])
      .pipe(
        tap(x => {
          this.useAccountsHierarchy.set(
            SettingsUtil.getBoolCompanySetting(
              x,
              CompanySettingType.UseAccountHierarchy
            )
          );

          if (this.useAccountsHierarchy()) {
            this.loadAccounts().subscribe();
          }

          let val = SettingsUtil.getIntCompanySetting(
            x,
            CompanySettingType.TimeSchedulePlanningDayViewMinorTickLength
          );
          if (val === 0) val = this.DEFAULT_MIN_LENGTH;
          this.minLength.set(val);
        })
      );
  }

  override loadUserSettings(): Observable<void> {
    return this.coreService.getUserSettings([
      UserSettingType.TimeSchedulePlanningDayViewDefaultGroupBy,
    ]);
  }

  override onSettingsLoaded() {
    this.lengthInfoLabel = `${this.terms['common.insertformat']} ${this.minLength()} ${this.terms['core.time.short.minute'].toLowerCase()}`;

    this.saveMenuList.push({
      id: SaveMenuFunction.Save,
      label: `${this.terms['core.save']} (Ctrl + S)`,
    });
    this.saveMenuList.push({
      id: SaveMenuFunction.SaveAndNew,
      label: `${this.terms['core.saveandnew']} (Ctrl + Alt + N)`,
    });
    this.saveMenuList.push({
      id: SaveMenuFunction.SaveAndClose,
      label: `${this.terms['core.saveandclose']} (Ctrl + Alt + Enter)`,
    });
  }

  loadTaskTypes(): Observable<SmallGenericType[]> {
    return this.service.getTaskTypesDict(false);
  }

  override loadData(): Observable<void> {
    return this.performLoadData.load$(
      this.service.get(this.form?.getIdControl()?.value, true, true, true).pipe(
        tap((value: ITimeScheduleTaskDTO) => {
          this.form?.customPatchValue(value);
          this.form?.setInitialFormattedValues();
          this.setRecurrenceInfo();
        })
      )
    );
  }

  private loadAccounts(): Observable<AccountDTO[] | undefined> {
    if (!this.useAccountsHierarchy()) return of(undefined);

    const today = DateUtil.getToday();
    this.accounts = [];
    return this.sharedService
      .getAccountsFromHierarchyByUserSetting(
        today,
        today,
        true,
        false,
        false,
        true
      )
      .pipe(
        tap(accounts => {
          this.accounts = accounts.map(a => ({
            id: a.accountId,
            name: a.name,
          }));
          this.accounts.unshift(new SmallGenericType(0, ''));
        })
      );
  }

  override newRecord(): Observable<void> {
    if (this.form?.isCopy) {
      // Copy record, nothing to clear, keep fields
    } else if (this.form?.isNew) {
      // New record, clear fields and set default values
      this.form?.reset();
      this.form?.patchValue({
        isActive: true,
        shiftTypeId: 0,
        length: 0,
        minSplitLength: this.minLength(),
        nbrOfPersonsOne: 1,
        nbrOfPersons: 1,
        onlyOneEmployee: false,
        dontAssignBreakLeftovers: false,
        allowOverlapping: false,
        isstaffingNeedsFrequency: false,
        startDate: DateUtil.getToday(),
      });
      this.form.customExcludedDatesPatchValue([]);
    }

    this.form?.setInitialFormattedValues();
    this.setRecurrenceInfo();
    if (this.nameField)
      focusOnElement((<any>this.nameField).inputER.nativeElement);

    return of(void 0);
  }

  private setRecurrenceInfo() {
    if (this.form?.value) {
      this.service
        .getRecurrenceDescription(this.form?.value.recurrencePattern)
        .pipe(take(1))
        .subscribe((desc: string) => {
          this.form?.patchValue({ recurrencePatternDescription: desc });
          DailyRecurrenceRangeDTO.setRecurrenceInfo(this.form, this.translate);
        });

      if (
        this.form?.value.excludedDates &&
        this.form?.value.excludedDates.length > 0
      ) {
        this.form.patchValue({
          excludedDatesDescription: this.form.value.excludedDates
            .map((d: Date) => d.toFormattedDate())
            .join(', '),
        });
      }
    }
  }

  private setupKeyboardShortcuts() {
    this.shortcutService.bindShortcut(
      this.element,
      this.destroyRef,
      ['Control', 's'],
      e => this.save()
    );

    this.shortcutService.bindShortcut(
      this.element,
      this.destroyRef,
      ['Control', 'Alt', 'n'],
      e => this.saveAndNew()
    );

    this.shortcutService.bindShortcut(
      this.element,
      this.destroyRef,
      ['Control', 'Alt', 'Enter'],
      e => this.saveAndClose()
    );
  }

  // EVENTS

  timeChanged(value: TimeboxValue) {
    this.form?.calculateLength();
  }

  lengthChanged(value: TimeboxValue) {
    this.form?.formatLength(this.minLength());
  }

  minSplitLengthChanged(value: TimeboxValue) {
    this.form?.formatMinSplitLength(this.minLength());
  }

  openRecurrencePatternDialog() {
    const params = new DailyRecurrenceParamsDTO(this.form);

    const dialogData: DailyRecurrencePatternDialogData =
      new DailyRecurrencePatternDialogData();
    dialogData.size = 'xl';
    dialogData.title = this.form?.hasRecurrencePattern
      ? 'common.dailyrecurrencepattern.editpattern'
      : 'common.dailyrecurrencepattern.createpattern';
    dialogData.pattern = params.pattern;
    dialogData.range = params.range;
    dialogData.excludedDates = this.form?.value.excludedDates;
    dialogData.date = params.date;

    this.dialogService
      .open(DailyRecurrencePatternDialogComponent, dialogData)
      .afterClosed()
      .subscribe((result: DailyRecurrencePatternDialogResult) => {
        if (result) {
          params.parseResult(this.form, result);
          this.setRecurrenceInfo();
          this.form?.markAsDirty();
        }
      });
  }

  openGeneratedNeedsDialog() {
    const dialogData: GeneratedNeedsDialogData = new GeneratedNeedsDialogData();
    dialogData.size = 'lg';
    dialogData.title = 'time.schedule.timescheduletask.generatedneed';
    dialogData.timeScheduleTaskId = this.form?.getIdControl()?.value;
    dialogData.date = DateUtil.getToday();

    this.dialogService.open(GeneratedNeedsComponent, dialogData);
  }

  onSaveMenuListItemSelected(item: MenuButtonItem) {
    switch (item.id) {
      case SaveMenuFunction.Save:
        this.save();
        break;
      case SaveMenuFunction.SaveAndNew:
        this.saveAndNew();
        break;
      case SaveMenuFunction.SaveAndClose:
        this.saveAndClose();
        break;
    }
  }

  private save() {
    if (!this.flowHandler.modifyPermission()) return;

    this.newAfterSave = false;
    this.closeAfterSave = false;
    this.performSave();
  }

  private saveAndNew() {
    if (!this.flowHandler.modifyPermission()) return;

    this.newAfterSave = true;
    this.performSave(undefined, true);
  }

  private saveAndClose() {
    if (!this.flowHandler.modifyPermission()) return;

    this.closeAfterSave = true;
    this.additionalSaveProps = { closeTabOnSave: true };
    this.performSave(undefined, true);
  }

  override onSaveCompleted(backendResponse: BackendResponse): void {
    if (backendResponse.success) {
      if (this.newAfterSave) {
        this.newAfterSave = false;
        if (this.form) this.form.isNew = true;
        this.setNewRefOnTab();
        this.newRecord().subscribe();
      } else if (this.closeAfterSave) {
        this.closeAfterSave = false;
      }
    }
  }
}

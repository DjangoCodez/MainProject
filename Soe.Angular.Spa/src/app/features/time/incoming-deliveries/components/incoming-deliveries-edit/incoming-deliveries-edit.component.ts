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
import { TermCollection } from '@shared/localization/term-types';
import { AccountDTO } from '@shared/models/account.model';
import { SmallGenericType } from '@shared/models/generic-type.model';
import {
  CompanySettingType,
  Feature,
} from '@shared/models/generated-interfaces/Enumerations';
import { IIncomingDeliveryHeadDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import {
  DailyRecurrenceParamsDTO,
  DailyRecurrenceRangeDTO,
} from '@shared/models/recurrence.model';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { SharedService } from '@shared/services/shared.service';
import { ProgressOptions } from '@shared/services/progress';
import { DateUtil } from '@shared/util/date-util';
import { SettingsUtil } from '@shared/util/settings-util';
import { focusOnElement } from '@shared/util/focus-util';
import { DialogService } from '@ui/dialog/services/dialog.service';
import { MenuButtonItem } from '@ui/button/menu-button/menu-button.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { Observable, of, take, tap } from 'rxjs';
import { IncomingDeliveriesForm } from '../../models/incoming-deliveries-form.model';
import { IncomingDeliveriesService } from '../../services/incoming-deliveries.service';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';

enum SaveMenuFunction {
  Save = 1,
  SaveAndNew = 2,
  SaveAndClose = 3,
}

@Component({
  selector: 'soe-incoming-deliveries-edit',
  templateUrl: './incoming-deliveries-edit.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class IncomingDeliveriesEditComponent
  extends EditBaseDirective<
    IIncomingDeliveryHeadDTO,
    IncomingDeliveriesService,
    IncomingDeliveriesForm
  >
  implements OnInit, AfterViewInit
{
  service = inject(IncomingDeliveriesService);
  private coreService = inject(CoreService);
  private sharedService = inject(SharedService);
  private shortcutService = inject(ShortcutService);
  dialogService = inject(DialogService);

  readonly DEFAULT_MIN_LENGTH = 15;
  useAccountsHierarchy = signal(false);
  minLength = signal(this.DEFAULT_MIN_LENGTH);

  accounts: SmallGenericType[] = [];
  saveMenuList: MenuButtonItem[] = [];
  private newAfterSave = false;
  private closeAfterSave = false;

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

    this.startFlow(Feature.Time_Schedule_StaffingNeeds_IncomingDeliveries);
  }

  ngAfterViewInit(): void {
    this.setupKeyboardShortcuts();
  }

  override loadTerms(): Observable<TermCollection> {
    return super.loadTerms([
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

  override onSettingsLoaded() {
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

  override loadData(): Observable<void> {
    return this.performLoadData.load$(
      this.service.get(this.form?.getIdControl()?.value).pipe(
        tap((value: IIncomingDeliveryHeadDTO) => {
          this.form?.customPatchValue(value);
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
    let clearValues = () => {};

    // If using save and new, keep the start date
    const startDate = this.form?.value.startDate || DateUtil.getToday();

    if (this.form?.isCopy) {
      // Copy record
      // Patch rows and clear row ids
      clearValues = () => {
        this.form?.onDoCopy();
      };
    } else if (this.form?.isNew) {
      // New record, clear fields and set default values
      this.form?.reset();
      this.form?.patchValue({
        isActive: true,
        startDate: startDate,
      });
      this.form.customExcludedDatesPatchValue([]);
      this.form.patchRows([]);
    }

    this.setRecurrenceInfo();
    if (this.nameField)
      focusOnElement((<any>this.nameField).inputER.nativeElement);

    return of(clearValues());
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

  override performSave(options?: ProgressOptions, skipLoadData = false): void {
    super.performSave(options, skipLoadData);
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

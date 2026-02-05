import { Component, inject, OnInit, signal, ViewChild } from '@angular/core';
import { EditBaseDirective } from '@shared/directives/edit-base/edit-base.directive';
import { ShiftTypeService } from '../../services/shift-type.service';
import {
  CompanySettingType,
  Feature,
  TermGroup,
  TermGroup_TimeScheduleTemplateBlockType,
} from '@shared/models/generated-interfaces/Enumerations';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { BehaviorSubject, Observable, take, tap } from 'rxjs';
import { AccountDTO } from '@shared/models/account.model';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { DateUtil } from '@shared/util/date-util';
import { SettingsUtil } from '@shared/util/settings-util';
import {
  IAccountingSettingsRowDTO,
  IEmployeePostSkillDTO,
  IShiftTypeDTO,
  IShiftTypeSkillDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { ShiftTypeForm } from '../../models/shift-type-form.model';
import { AccountDimDTO } from 'src/app/features/economy/accounting-coding-levels/models/accounting-coding-levels.model';
import { ProgressOptions } from '@shared/services/progress';
import { CrudActionTypeEnum } from '@shared/enums';
import { TimeService } from '@features/time/services/time.service';
import { TermCollection } from '@shared/localization/term-types';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import { CategoryItem } from '@shared/components/categories/categories.model';
import { AccountingSettingsComponent } from '@shared/components/accounting-settings/accounting-settings/accounting-settings.component';
import { ShiftTypeParamsService } from '../../services/shift-type-params.service';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';

@Component({
  selector: 'soe-shift-type-edit',
  templateUrl: './shift-type-edit.component.html',
  styleUrl: './shift-type-edit.component.scss',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class ShiftTypeEditComponent
  extends EditBaseDirective<IShiftTypeDTO, ShiftTypeService, ShiftTypeForm>
  implements OnInit
{
  @ViewChild(AccountingSettingsComponent)
  accountingSettingsComponent!: AccountingSettingsComponent;

  service = inject(ShiftTypeService);
  coreService = inject(CoreService);
  timeService = inject(TimeService);
  urlService = inject(ShiftTypeParamsService);

  selectedSkills = new BehaviorSubject<IShiftTypeSkillDTO[]>([]);

  hasSchedulePermission = signal(false);
  hasOrderPermission = signal(false);
  hasBookingPermission = signal(false);
  hasStandbyPermission = signal(false);
  hasPlanningPermission = signal(false);
  hasEmployeeStatisticsPermission = signal<boolean>(false);

  isLeadingZero = signal(false);
  useAccountHierarchy = signal(false);
  defaultEmployeeAccountDimEmployee = signal(0);
  timeScheduleTypeVisible = signal(false);

  baseAccounts: ISmallGenericType[] = [];
  accountingSettings: IAccountingSettingsRowDTO[] = [];
  timeScheduleTemplateBlockTypes: SmallGenericType[] = [];
  timeScheduleTypes: SmallGenericType[] = [];
  shiftTypeAccountDim: AccountDimDTO | undefined;
  settingTypes: SmallGenericType[] = [];
  selectedCategories: number[] = [];
  skills: IEmployeePostSkillDTO[] = [];
  accordionName = signal('');
  typeInfo = signal('');

  linkedToInactivatedAccountInfo = signal('');

  ngOnInit() {
    super.ngOnInit();

    this.startFlow(
      this.urlService.isOrder
        ? Feature.Billing_Preferences_InvoiceSettings_ShiftType_Edit
        : Feature.Time_Preferences_ScheduleSettings_ShiftType_Edit,
      {
        lookups: [this.loadShiftTypeAccountDim(), this.loadTimeScheduleTypes()],
      }
    );
  }

  override newRecord(): Observable<void> {
    return this.loadModifyPermissions().pipe(
      take(1),
      tap(() => {
        if (this.form?.isCopy) {
          this.selectedSkills.next(this.form.shiftTypeSkills.value);
          // To make accountingSettings show on copy
          setTimeout(() => {
            this.accountingSettings = [
              this.form?.controls.accountingSettings.value,
            ];
          }, 500);
          // this.form?.patchValue({ accountId: 0 });
          this.form?.customHierarchyAccountsPatchValue(
            this.form?.hierarchyAccounts.value
          );
        }
      })
    );
  }

  convertSkills(value: IShiftTypeDTO) {
    // Convert to employeepost skill for skills directive
    const convertedSkills: IEmployeePostSkillDTO[] = [];
    value.shiftTypeSkills.forEach((y: IShiftTypeSkillDTO) => {
      const skillToAdd: Partial<IEmployeePostSkillDTO> = {};
      skillToAdd.employeePostSkillId = y.shiftTypeSkillId;
      skillToAdd.skillLevel = y.skillLevel;
      skillToAdd.skillLevelStars = y.skillLevelStars;
      skillToAdd.skillName = y.skillName;
      skillToAdd.skillTypeName = y.skillTypeName;
      convertedSkills.push(<IEmployeePostSkillDTO>skillToAdd);
    });
    this.skills = convertedSkills;
  }

  defaultLengthFormat(defaultLength: number) {
    this.form?.patchValue({
      defaultLengthFormatted: DateUtil.minutesToTimeSpan(defaultLength),
    });
    this.isLeadingZero.set(defaultLength == 0);
  }

  private setupSettingTypes() {
    this.settingTypes = [];
    this.settingTypes.push(
      new SmallGenericType(0, this.terms['common.accountingsettings.account'])
    );
  }

  private setupTerms() {
    // Init parameters
    if (this.urlService.isOrder) {
      this.accordionName.set(
        this.terms['time.schedule.shifttype.ordershifttype']
      );
      this.typeInfo.set(this.terms['time.schedule.ordershifttype.typeinfo']);
    } else {
      this.accordionName.set(this.terms['time.schedule.shifttype.shifttype']);
      this.typeInfo.set(this.terms['time.schedule.shifttype.typeinfo']);
    }
  }

  override performSave(options?: ProgressOptions): void {
    if (!this.form || this.form.invalid || !this.service) return;
    if (!options) {
      options = {};
    }
    options.callback = (val: BackendResponse) => {
      if (val.success === true) {
        // Reload accounts after save to populate with newly created account
        this.loadShiftTypeAccountDim().subscribe();
        this.accountingSettingsComponent.reloadGridWithNewAccounts();
      }
    };
    this.performAction.crud(
      CrudActionTypeEnum.Save,
      this.service
        .save(this.form?.getAllValues())
        .pipe(tap(this.updateFormValueAndEmitChange)),
      options.callback, //Check if successful (success = true) here, then load
      undefined,
      options
    );
  }

  // Load Data
  override loadData(): Observable<void> {
    return this.loadModifyPermissions().pipe(
      take(1),
      tap(() => {
        this.performLoadData.load(
          this.service
            .get(
              this.form?.getIdControl()?.value,
              true,
              true,
              this.hasEmployeeStatisticsPermission(),
              this.hasEmployeeStatisticsPermission(),
              true,
              this.useAccountHierarchy()
            )
            .pipe(
              tap(value => {
                this.accountingSettings = [value.accountingSettings];

                this.selectedSkills.next(value.shiftTypeSkills);
                if (value.categoryIds)
                  this.selectedCategories = value.categoryIds;

                this.form?.customPatchValue(value);

                // Set accountDim select to 0 if null
                if (this.shiftTypeAccountDim && !value.accountId) {
                  this.form?.patchValue({ accountId: 0 });
                }
                this.convertSkills(value);

                this.defaultLengthFormat(value.defaultLength);

                // Set information term if linked account is inactive
                if (value.accountIsNotActive) {
                  this.linkedToInactivatedAccountInfo.set(
                    this.terms[
                      'time.schedule.shifttype.linkedtoinactivatedaccountinfo'
                    ].format(value.accountNrAndName)
                  );
                }
              })
            )
        );
      })
    );
  }

  override loadCompanySettings() {
    const settingTypes: number[] = [];
    settingTypes.push(CompanySettingType.UseAccountHierarchy);
    settingTypes.push(CompanySettingType.DefaultEmployeeAccountDimEmployee);

    return this.coreService.getCompanySettings(settingTypes).pipe(
      tap(setting => {
        this.useAccountHierarchy.set(
          SettingsUtil.getBoolCompanySetting(
            setting,
            CompanySettingType.UseAccountHierarchy
          )
        );
        this.defaultEmployeeAccountDimEmployee.set(
          SettingsUtil.getIntCompanySetting(
            setting,
            CompanySettingType.DefaultEmployeeAccountDimEmployee
          )
        );
      })
    );
  }

  loadModifyPermissions(): Observable<any> {
    const featureIds: number[] = [];

    featureIds.push(Feature.Billing_Preferences_InvoiceSettings_ShiftType_Edit);
    featureIds.push(Feature.Time_Schedule_SchedulePlanning);
    featureIds.push(Feature.Billing_Order_Planning);
    featureIds.push(Feature.Time_Schedule_SchedulePlanning_Bookings);
    featureIds.push(Feature.Time_Schedule_SchedulePlanning_StandbyShifts);
    featureIds.push(Feature.Time_Schedule_Needs_Planning);
    featureIds.push(Feature.Time_Employee_Statistics);
    featureIds.push(Feature.Billing_Order_Planning_Bookings);
    featureIds.push(Feature.Time_Schedule_Needs_Shifts);
    featureIds.push(Feature.Time_Employee_Statistics);

    return this.performLoadData.load$(
      this.coreService.hasModifyPermissions(featureIds).pipe(
        tap(response => {
          this.hasSchedulePermission.set(
            response[Feature.Time_Schedule_SchedulePlanning]
          );
          this.hasOrderPermission.set(response[Feature.Billing_Order_Planning]);
          this.hasBookingPermission.set(
            response[Feature.Time_Schedule_SchedulePlanning_Bookings] ||
              response[Feature.Billing_Order_Planning_Bookings]
          );
          this.hasStandbyPermission.set(
            response[Feature.Time_Schedule_SchedulePlanning_StandbyShifts]
          );
          this.hasPlanningPermission.set(
            response[Feature.Time_Schedule_Needs_Planning] ||
              response[Feature.Time_Schedule_Needs_Shifts]
          );
          this.hasEmployeeStatisticsPermission.set(
            response[Feature.Time_Employee_Statistics]
          );

          this.flowHandler.modifyPermission();

          this.loadTimeScheduleTemplateBlockTypes().subscribe(); // Runs after permissions set
        })
      )
    );
  }

  private loadTimeScheduleTemplateBlockTypes(): Observable<SmallGenericType[]> {
    return this.coreService
      .getTermGroupContent(TermGroup.TimeScheduleTemplateBlockType, true, false)
      .pipe(
        tap(x => {
          this.timeScheduleTemplateBlockTypes = [];
          this.timeScheduleTemplateBlockTypes.push({ id: -1, name: ' ' });
          x.forEach((type: any) => {
            if (this.urlService.isOrder) {
              if (
                type.id == TermGroup_TimeScheduleTemplateBlockType.Order &&
                this.hasOrderPermission()
              )
                this.timeScheduleTemplateBlockTypes.push(type);
              if (
                type.id == TermGroup_TimeScheduleTemplateBlockType.Booking &&
                this.hasBookingPermission()
              )
                this.timeScheduleTemplateBlockTypes.push(type);
            }
            if (!this.urlService.isOrder) {
              // Types shown from Shifttype edit
              if (
                type.id == TermGroup_TimeScheduleTemplateBlockType.Schedule &&
                this.hasSchedulePermission()
              )
                this.timeScheduleTemplateBlockTypes.push(type);
              if (
                type.id == TermGroup_TimeScheduleTemplateBlockType.Booking &&
                this.hasBookingPermission()
              )
                this.timeScheduleTemplateBlockTypes.push(type);
              if (
                type.id == TermGroup_TimeScheduleTemplateBlockType.Standby &&
                this.hasStandbyPermission()
              )
                this.timeScheduleTemplateBlockTypes.push(type);
            }
          });
        })
      );
  }

  private loadTimeScheduleTypes() {
    return this.timeService.getTimeScheduleTypesDict(false, true).pipe(
      tap(x => {
        this.timeScheduleTypes = x;
        if (this.timeScheduleTypes.length > 1)
          this.timeScheduleTypeVisible.set(true);
      })
    );
  }

  private loadShiftTypeAccountDim() {
    return this.service.getShiftTypeAccountDim(true, false).pipe(
      tap(x => {
        this.shiftTypeAccountDim = x;

        if (this.shiftTypeAccountDim) {
          let account: AccountDTO = new AccountDTO();
          account.accountId = 0;
          account.numberName =
            this.terms['time.schedule.shifttype.nolinkedaccount'];
          this.shiftTypeAccountDim.accounts.splice(0, 0, account);

          account = new AccountDTO();
          account.accountId = -1;
          account.numberName =
            this.terms['time.schedule.shifttype.addnewaccount'];
          this.shiftTypeAccountDim.accounts.splice(0, 0, account);
        }
      })
    );
  }

  override loadTerms(): Observable<TermCollection> {
    return super
      .loadTerms([
        'time.schedule.shifttype.addnewaccount',
        'time.schedule.shifttype.addnewaccount',
        'common.accountingsettings.account',
        'time.schedule.shifttype.shifttype',
        'time.schedule.shifttype.nolinkedaccount',
        'time.schedule.shifttype.addnewaccount',
        'time.schedule.shifttype.linkedtoinactivatedaccountinfo',
        'time.schedule.shifttype.ordershifttype',
        'time.schedule.shifttype.typeinfo',
        'time.schedule.ordershifttype.typeinfo',
      ])
      .pipe(
        tap(() => {
          this.setupSettingTypes();
          this.setupTerms();
        })
      );
  }

  // Events

  accountSettingsChanged(rows: IAccountingSettingsRowDTO[]) {
    this.form?.patchValue({ accountingSettings: rows[0] }); // [0] since only one row is supported
  }

  categoriesChanged(categories: CategoryItem[]) {
    this.form?.customCategoryIdsPatchValue(categories.map(c => c.categoryId));
  }

  skillsChanged(rows: IShiftTypeSkillDTO[]) {
    this.form?.customSkillIdsPatchValue(rows);
  }

  linkedAccountChanged(accountId: number) {
    const acc = this.shiftTypeAccountDim?.accounts.find(
      account => account.accountId == accountId
    );
    if (acc) {
      this.form?.patchValue({ needsCode: acc?.accountNr });
    }
  }
}

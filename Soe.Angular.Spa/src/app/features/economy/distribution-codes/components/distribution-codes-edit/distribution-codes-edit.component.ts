import { Component, OnInit, inject, signal, viewChild } from '@angular/core';
import { EditBaseDirective } from '@shared/directives/edit-base/edit-base.directive';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import {
  DistributionCodePeriodDTO,
  DistributionCodeHeadDTO,
  PeriodSummery,
} from '../../models/distribution-codes.model';
import { DistributionCodesService } from '../../services/distribution-codes.service';
import { BehaviorSubject, Observable, distinctUntilChanged, tap } from 'rxjs';
import {
  Feature,
  TermGroup,
  TermGroup_AccountingBudgetSubType,
  TermGroup_AccountingBudgetType,
} from '@shared/models/generated-interfaces/Enumerations';
import {
  addPeriodValidator,
  DistributionCodeHeadForm,
} from '../../models/distribution-codes-head-form.model';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { EconomyService } from '../../../services/economy.service';
import { OpeningHoursService } from '@src/app/features/manage/opening-hours/services/opening-hours.service';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import { IDistributionCodePeriodDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { Validators } from '@angular/forms';
import { CrudActionTypeEnum } from '@shared/enums';
import { DistributionCodesEditGridComponent } from './distribution-codes-edit-grid/distribution-codes-edit-grid.component';
import { DateUtil } from '@shared/util/date-util';
import { addEmptyOption } from '@shared/util/array-util';
import { TermCollection } from '@shared/localization/term-types';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';

@Component({
  selector: 'soe-distribution-codes-edit',
  templateUrl: './distribution-codes-edit.component.html',
  styleUrl: './distribution-codes-edit.component.scss',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class DistributionCodesEditComponent
  extends EditBaseDirective<
    DistributionCodeHeadDTO,
    DistributionCodesService,
    DistributionCodeHeadForm
  >
  implements OnInit
{
  service = inject(DistributionCodesService);
  coreService = inject(CoreService);
  economyService = inject(EconomyService);
  openingHourService = inject(OpeningHoursService);
  subLevelDict: DistributionCodeHeadDTO[] = [];
  subLevelDictList = new BehaviorSubject<DistributionCodeHeadDTO[]>([]);
  openingHours: SmallGenericType[] = [];
  accountDimSmallOptions: ISmallGenericType[] = [];
  subTypes: SmallGenericType[] = [];
  types: SmallGenericType[] = [];

  showNumberOfPeriods = signal(true);
  showOpeningHours = signal(false);
  showPeriodButton = signal(true);
  isAccountingBudge = signal(false);

  // SubGrid
  periodDataGrid = viewChild(DistributionCodesEditGridComponent);
  periodData = new BehaviorSubject<IDistributionCodePeriodDTO[]>([]);
  sumPeriods = 0;
  sumPercent = 0;
  diff = 0;
  validationErrors: string[] = [];
  AccountDimValidationErrors: string[] = [];
  hideSublevel = true;
  hidePeriodInfo = true;
  get periodData$() {
    return this.periodData.asObservable();
  }

  ngOnInit() {
    super.ngOnInit();

    this.startFlow(
      Feature.Economy_Preferences_VoucherSettings_DistributionCodes_Edit,
      {
        lookups: [
          this.loadTypePeriod(),
          this.loadOpeningHours(),
          this.loadDistributionCodes(),
          this.loadSubTypes(),
          this.loadAccountDims(),
        ],
      }
    );

    if (this.form?.isCopy) {
      this.periodData.next(this.form.periods.value);
      this.fieldVisible(this.form.typeId.value);
    }
    if (this.form?.isNew && !this.form.isCopy)
      this.addRows(this.form?.value.noOfPeriods);
    this.columnVisible(this.form?.typeId.value);
    this.addValidators();

    this.form?.noOfPeriods.valueChanges
      .pipe(distinctUntilChanged())
      .subscribe(value => {
        {
          this.addRows(value);
        }
      });

    this.form?.parentId.valueChanges.subscribe(x => {
      if (x) this.populateGridSublevelData(x);
    });

    //Ensure to apply edited data in the grid when clicking outside
    document.body.addEventListener('click', (event: any) => {
      if (event.target.closest('ag-grid-angular') == null) {
        this.applyChanges();
      }
    });
  }

  override onFinished(): void {
    super.onFinished();
    this.form?.addValidators(
      addPeriodValidator(
        this.translate.instant(
          'economy.accounting.distributioncode.diffValidation'
        )
      )
    );
  }

  override loadTerms(): Observable<TermCollection> {
    return super.loadTerms(['common.week']);
  }

  //populate grid data
  populateGridSublevelData(parentId: number) {
    this.form?.periods.value.forEach((period: DistributionCodePeriodDTO) => {
      period.parentToDistributionCodePeriodId =
        parentId == 0 ? undefined : parentId;
    });
    this.periodData.next(this.form!.periods.value);
  }

  loadTypePeriod() {
    return this.performLoadData.load$(
      this.coreService
        .getTermGroupContent(TermGroup.AccountingBudgetType, false, false)
        .pipe(
          tap(x => {
            //Have to leave staffBudget and ProjectBudget out
            x.forEach((b: SmallGenericType) => {
              if (
                b.id != TermGroup_AccountingBudgetType.StaffBudget &&
                b.id != TermGroup_AccountingBudgetType.ProjectBudget
              ) {
                this.types.push(b);
              }
            });
          })
        )
    );
  }

  loadSubTypes() {
    return this.performLoadData.load$(
      this.coreService
        .getTermGroupContent(
          TermGroup.AccountingBudgetSubType,
          true,
          false,
          true
        )
        .pipe(
          tap(x => {
            //Sort subtypes - putting year - month last
            x.forEach(b => {
              if (b.id > 0) this.subTypes.push(b);
            });
            this.subTypes.push(x[0]);
          })
        )
    );
  }

  loadDistributionCodes() {
    return this.performLoadData.load$(
      this.service.getDistributionCodes(false).pipe(
        tap(s => {
          this.subLevelDict = s;
          this.subLevelDictList.next(s);
          if (this.form?.isCopy)
            this.populateGridSublevelData(this.form.parentId.value);
        })
      )
    );
  }

  loadAccountDims() {
    this.accountDimSmallOptions = [];
    return this.economyService
      .getAccountDimsSmall(
        false,
        true,
        false,
        false,
        false,
        false,
        false,
        false
      )
      .pipe(
        tap(x => {
          this.accountDimSmallOptions = [];
          x.forEach(y => {
            this.accountDimSmallOptions.push({
              id: y.accountDimId,
              name: y.name,
            });
          });
        })
      );
  }

  loadOpeningHours() {
    return this.performLoadData.load$(
      this.openingHourService.getOpeningHoursDict(true, true).pipe(
        tap(s => {
          this.openingHours = s;
        })
      )
    );
  }

  //SUB GRID DATA && EDIT
  override loadData(): Observable<void> {
    return this.performLoadData.load$(
      this.service.get(this.form?.getIdControl()?.value).pipe(
        tap(value => {
          this.fieldVisible(value.typeId);
          this.columnVisible(value.typeId);

          this.form?.patchValue({ periods: [] });
          value.periods.forEach(element => {
            this.form?.periods.value.push(element);
          });

          this.periodInfoPopulate(value.periods);
          this.form?.customPatch(value);
          this.periodData.next(value.periods);

          this.showOpeningHours.set(
            this.form?.value.openingHoursId ===
              TermGroup_AccountingBudgetSubType.Day
          );
        })
      )
    );
  }

  periodInfoPopulate(periods = this.periodData.value) {
    if (
      this.form?.typeId.value !==
      TermGroup_AccountingBudgetType.AccountingBudget
    ) {
      periods.forEach(element => {
        element.percent = Number(element.percent.toFixed(2));
        element.periodSubTypeName = element.number.toString() + '.';
      });
    }
  }

  ChangeType(id: number) {
    this.columnVisible(id);

    this.form?.patchValue({
      subType: undefined,
      noOfPeriods: '0',
      accountDimId: undefined,
    });

    this.form?.customPeriodsPatchValue([]);
    this.periodData.next([]);

    this.fieldVisible(id);
    this.addValidators();
  }

  addValidators() {
    if (
      this.form?.value.typeId == TermGroup_AccountingBudgetType.AccountingBudget
    ) {
      this.form?.subType.clearValidators();
      this.form?.noOfPeriods.addValidators(Validators.required);
    } else if (
      this.form?.value.typeId !=
        TermGroup_AccountingBudgetType.AccountingBudget &&
      this.form?.value.subType == undefined
    ) {
      this.form?.noOfPeriods.clearValidators();
      this.form?.subType.addValidators(Validators.required);
    } else {
      this.form?.noOfPeriods.clearValidators();
    }
    this.form?.noOfPeriods.updateValueAndValidity();
    this.form?.subType.updateValueAndValidity();
    this.form?.updateValueAndValidity();
  }

  changePeriodType(id: number) {
    this.form?.periods.clear();
    this.periodData.next([]);

    this.showOpeningHours.set(id === TermGroup_AccountingBudgetSubType.Day);
    this.hideSublevel = id === TermGroup_AccountingBudgetSubType.Day;

    const date = this.form?.value.fromDate
      ? this.form?.value.fromDate
      : new Date();

    switch (id) {
      case TermGroup_AccountingBudgetSubType.January:
      case TermGroup_AccountingBudgetSubType.February:
      case TermGroup_AccountingBudgetSubType.March:
      case TermGroup_AccountingBudgetSubType.April:
      case TermGroup_AccountingBudgetSubType.May:
      case TermGroup_AccountingBudgetSubType.June:
      case TermGroup_AccountingBudgetSubType.July:
      case TermGroup_AccountingBudgetSubType.August:
      case TermGroup_AccountingBudgetSubType.September:
      case TermGroup_AccountingBudgetSubType.October:
      case TermGroup_AccountingBudgetSubType.November:
      case TermGroup_AccountingBudgetSubType.December:
        this.addRows(new Date(date.getFullYear(), id, 0).getDate(), id);
        break;
      case TermGroup_AccountingBudgetSubType.Week:
        this.addRows(7, TermGroup_AccountingBudgetSubType.Week);
        break;
      case TermGroup_AccountingBudgetSubType.Day:
        this.addRows(24, TermGroup_AccountingBudgetSubType.Day);
        break;
      case TermGroup_AccountingBudgetSubType.Year:
        this.addRows(12, TermGroup_AccountingBudgetSubType.Year);
        break;
      case TermGroup_AccountingBudgetSubType.YearWeek:
        this.addRows(
          DateUtil.getWeekCountInYear(date.getFullYear()),
          TermGroup_AccountingBudgetSubType.YearWeek
        );
        break;
      default:
        break;
    }
  }

  fieldVisible(id: number) {
    this.isAccountingBudge.set(
      id === TermGroup_AccountingBudgetType.AccountingBudget
    );
    this.showOpeningHours.set(
      id === TermGroup_AccountingBudgetType.AccountingBudget
    );
    this.showPeriodButton.set(
      id === TermGroup_AccountingBudgetType.AccountingBudget
    );
    if (id === 0) {
      this.form?.accountDimId.disable();
      this.form?.subType.disable();
      this.form?.parentId.disable();
    }

    this.showNumberOfPeriods.set(
      id === TermGroup_AccountingBudgetType.AccountingBudget
    );
  }

  columnVisible(id: number) {
    this.hideSublevel = !(
      id !== TermGroup_AccountingBudgetType.AccountingBudget
    );

    this.hidePeriodInfo =
      id === TermGroup_AccountingBudgetType.AccountingBudget;

    if (this.form?.value.openingHoursId)
      this.hideSublevel =
        this.form?.value.openingHoursId ===
        TermGroup_AccountingBudgetSubType.Day;
  }

  addRows(value: number, typePeriod = 0) {
    this.form?.patchValue({ periods: [] });
    this.form?.periods.clear();
    this.periodData.next([]);

    if (
      this.form?.typeId.value != TermGroup_AccountingBudgetType.AccountingBudget
    )
      this.filterSubLevel(value, typePeriod);

    const percent = 100 / value;
    let totSum = 0;

    if (value != 0) {
      for (let i = 0; i < value; i++) {
        if (i === value - 1) {
          this.addRow(((100 - totSum) * 100.0) / 100.0, typePeriod);
        } else {
          const cleanedPercent = this.cleanPercent(percent);
          this.addRow(percent, typePeriod);
          totSum = totSum + +cleanedPercent;
        }
      }
    }
    this.periodData.next(this.form!.periods.value);
  }

  filterSubLevel(value: number, typePeriod?: number) {
    this.initSetupPeriods();

    if (typePeriod == TermGroup_AccountingBudgetSubType.Year)
      this.setUpYearMonth();
    if (typePeriod == TermGroup_AccountingBudgetSubType.YearWeek)
      this.setUpYearWeek();
    else this.setUpMonth();
  }

  private initSetupPeriods() {
    const levels: DistributionCodeHeadDTO[] = [];
    addEmptyOption(levels);
    this.subLevelDict = levels;
  }

  setUpMonth() {
    this.subLevelDictList.value
      ?.filter(sl => sl.typeId == this.form?.value.typeId)
      .forEach(x => {
        if (x.subType == TermGroup_AccountingBudgetSubType.Day) {
          this.subLevelDict.push(x);
        } else {
          this.setParentUndefined(x.distributionCodeHeadId);
        }
      });
  }

  setUpYearWeek() {
    this.subLevelDictList.value
      ?.filter(sl => sl.typeId === this.form?.value.typeId)
      .forEach(x => {
        if (x.subType === TermGroup_AccountingBudgetSubType.Week) {
          this.subLevelDict.push(x);
        } else {
          this.setParentUndefined(x.distributionCodeHeadId);
        }
      });
  }

  setUpYearMonth() {
    this.subLevelDictList.value
      ?.filter(sl => sl.typeId === this.form?.value.typeId)
      .forEach(x => {
        if (
          x.subType &&
          x.subType > TermGroup_AccountingBudgetSubType.Year &&
          x.subType < TermGroup_AccountingBudgetSubType.Week
        ) {
          this.subLevelDict.push(x);
        } else {
          this.setParentUndefined(x.distributionCodeHeadId);
        }
      });
  }

  setParentUndefined(distributionCodeHeadId: number) {
    if (
      this.form?.value.parentId &&
      this.form?.value.parentId === distributionCodeHeadId
    )
      this.form.patchValue({ parentId: undefined });
  }

  addNewRow() {
    const periods = this.periodData.value;
    periods.push({
      number: this.periodData.value.length + 1,
      percent: this.cleanPercent(0),
      distributionCodePeriodId: 0,
      isAdded: false,
      isModified: false,
      comment: '',
      periodSubTypeName: '',
    });
    this.periodData.next(periods);
  }

  addRow(percent: number, typePeriod = 20) {
    if (this.form?.periods.value.length === undefined) {
      this.form?.periods.patchValue([]);
      this.periodData.next([]);

      this.form?.addPeriod({
        number: this.form?.periods.value.length,
        percent: this.cleanPercent(percent),
      });
    } else if (typePeriod === TermGroup_AccountingBudgetSubType.Year) {
      this.form?.addPeriod({
        number: this.form?.periods.value.length + 1,
        percent: this.cleanPercent(percent),
        periodSubTypeName: DateUtil.getMonthName(
          this.form?.periods.value.length
        ),
      });
    } else if (typePeriod === TermGroup_AccountingBudgetSubType.YearWeek) {
      this.form?.addPeriod({
        number: this.form?.periods.value.length + 1,
        percent: this.cleanPercent(percent),
        periodSubTypeName: `${this.terms['common.week']} ${this.form?.periods.value.length + 1}`,
      });
    } else if (typePeriod === TermGroup_AccountingBudgetSubType.Week) {
      this.form?.addPeriod({
        number: this.form?.periods.value.length + 1,
        percent: this.cleanPercent(percent),
        periodSubTypeName: DateUtil.getDayOfWeekName(
          this.form?.periods.value.length
        ),
      });
    } else if (typePeriod === TermGroup_AccountingBudgetSubType.Day) {
      this.form?.addPeriod({
        number: this.form?.periods.value.length + 1,
        percent: this.cleanPercent(percent),
        periodSubTypeName: this.getTimeRangeList(
          this.form?.periods.value.length
        ),
      });
    } else if (typePeriod === 21) {
      //Number of periods values
      this.form?.addPeriod({
        number: this.form?.periods.value.length + 1,
        percent: this.cleanPercent(percent),
      });
    } else {
      this.form?.addPeriod({
        number: this.form?.periods.value.length + 1,
        percent: this.cleanPercent(percent),
        periodSubTypeName: this.form?.periods.value.length + 1 + '.',
      });
    }
  }

  getTimeRangeList(value: number): string {
    let formattedTime = '';
    if (value + 1 == 24) {
      formattedTime = `${String(value).padStart(2, '0')}:00-` + `00:00`;
    } else {
      formattedTime =
        `${String(value).padStart(2, '0')}:00-` +
        `${String(value + 1).padStart(2, '0')}:00`;
    }
    return formattedTime;
  }

  cleanPercent(val: number) {
    if (!val) return 0;
    return Number(val.toFixed(2));
  }

  updateSummery(summarize: PeriodSummery) {
    const difference = Number(summarize.diff.toFixed(2));
    this.diff = difference == 0 ? Math.abs(difference) : difference;
    this.sumPercent = summarize.sumPercent;
  }

  private applyChanges(): void {
    this.periodDataGrid()?.grid?.applyChanges();
  }

  override performSave(): void {
    this.applyChanges();
    setTimeout(() => {
      if (!this.form || this.form.invalid || !this.service) return;

      if (this.form.parentId.value === 0)
        this.form.parentId.patchValue(undefined);

      const model = <DistributionCodeHeadDTO>this.form?.getRawValue();
      model.periods = this.periodData.value;

      this.performAction.crud(
        CrudActionTypeEnum.Save,
        this.service.save(model).pipe(tap(this.updateFormValueAndEmitChange))
      );
    }, 100);
  }
}

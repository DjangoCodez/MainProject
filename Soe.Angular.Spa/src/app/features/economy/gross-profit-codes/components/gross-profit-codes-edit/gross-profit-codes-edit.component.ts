import { Component, OnInit, inject, signal } from '@angular/core';
import { EditBaseDirective } from '@shared/directives/edit-base/edit-base.directive';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { GrossProfitCodeDTO } from '../../models/gross-profit-codes.model';
import { GrossProfitCodesService } from '../../services/gross-profit-codes.service';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { Observable } from 'rxjs';
import { tap } from 'rxjs/operators';
import { EconomyService } from '@src/app/features/economy/services/economy.service';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import { DateUtil } from '@shared/util/date-util';
import { Perform } from '@shared/util/perform.class';
import {
  IAccountDimSmallDTO,
  IAccountSmallDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { orderBy } from 'lodash';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';

@Component({
  selector: 'soe-gross-profit-codes-edit',
  templateUrl: './gross-profit-codes-edit.component.html',
  styleUrls: ['./gross-profit-codes-edit.component.scss'],
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class GrossProfitCodesEditComponent
  extends EditBaseDirective<GrossProfitCodeDTO, GrossProfitCodesService>
  implements OnInit
{
  service = inject(GrossProfitCodesService);
  economyService = inject(EconomyService);
  performLoadAccountYear = new Perform<ISmallGenericType[]>(
    this.progressService
  );
  performLoadAccountDims = new Perform<IAccountDimSmallDTO[]>(
    this.progressService
  );
  performLoadAccounts = new Perform<IAccountSmallDTO[]>(this.progressService);
  performTranslationLoad = new Perform<any[]>(this.progressService);

  accountYearFilterOptions: ISmallGenericType[] = [];
  accountDimSmallOptions: IAccountDimSmallDTO[] = [];
  accountFilterOptions: ISmallGenericType[] = [];

  period1SecondaryLabelPostfixText = signal('');
  period2SecondaryLabelPostfixText = signal('');
  period3SecondaryLabelPostfixText = signal('');
  period4SecondaryLabelPostfixText = signal('');
  period5SecondaryLabelPostfixText = signal('');
  period6SecondaryLabelPostfixText = signal('');
  period7SecondaryLabelPostfixText = signal('');
  period8SecondaryLabelPostfixText = signal('');
  period9SecondaryLabelPostfixText = signal('');
  period10SecondaryLabelPostfixText = signal('');
  period11SecondaryLabelPostfixText = signal('');
  period12SecondaryLabelPostfixText = signal('');

  ngOnInit() {
    super.ngOnInit();
    this.startFlow(
      Feature.Economy_Preferences_VoucherSettings_GrossProfitCodes_Edit,
      {
        lookups: [this.loadAccountYearDict(), this.loadAccountDims()],
      }
    );

    if (this.form?.isCopy) {
      this.loadAccounts();
    }
  }

  override onFinished(): void {
    if (this.form?.isNew && !this.form?.isCopy) {
      this.form?.patchValue({
        code: 0,
      });
    }
  }

  setPeriodSecondaryLabelText() {
    this.clearPeriodSecondaryLabelText();
    const accountYear = this.accountYearFilterOptions.find(
      f => f.id == this.form?.value.accountYearId
    );
    if (accountYear) {
      let startDate = DateUtil.parseDate(
        accountYear.name.split('-')[0].trim(),
        'yyyyMMdd'
      );
      const endDate = DateUtil.parseDate(
        accountYear.name.split('-')[1].trim(),
        'yyyyMMdd'
      );
      if (startDate && endDate) {
        const maxYear = startDate.addYears(1);
        while (startDate < endDate && startDate < maxYear) {
          switch (startDate.getMonth() + 1) {
            case 1:
              this.period1SecondaryLabelPostfixText.set(
                startDate.getFullYear().toString()
              );
              break;

            case 2:
              this.period2SecondaryLabelPostfixText.set(
                startDate.getFullYear().toString()
              );
              break;
            case 3:
              this.period3SecondaryLabelPostfixText.set(
                startDate.getFullYear().toString()
              );
              break;
            case 4:
              this.period4SecondaryLabelPostfixText.set(
                startDate.getFullYear().toString()
              );
              break;
            case 5:
              this.period5SecondaryLabelPostfixText.set(
                startDate.getFullYear().toString()
              );
              break;
            case 6:
              this.period6SecondaryLabelPostfixText.set(
                startDate.getFullYear().toString()
              );
              break;
            case 7:
              this.period7SecondaryLabelPostfixText.set(
                startDate.getFullYear().toString()
              );
              break;
            case 8:
              this.period8SecondaryLabelPostfixText.set(
                startDate.getFullYear().toString()
              );
              break;
            case 9:
              this.period9SecondaryLabelPostfixText.set(
                startDate.getFullYear().toString()
              );
              break;
            case 10:
              this.period10SecondaryLabelPostfixText.set(
                startDate.getFullYear().toString()
              );
              break;
            case 11:
              this.period11SecondaryLabelPostfixText.set(
                startDate.getFullYear().toString()
              );
              break;
            case 12:
              this.period12SecondaryLabelPostfixText.set(
                startDate.getFullYear().toString()
              );
              break;
            default:
              break;
          }
          startDate = startDate.addMonths(1);
        }
      }
    }
  }

  clearPeriodSecondaryLabelText() {
    this.period1SecondaryLabelPostfixText.set('');
    this.period2SecondaryLabelPostfixText.set('');
    this.period3SecondaryLabelPostfixText.set('');
    this.period4SecondaryLabelPostfixText.set('');
    this.period5SecondaryLabelPostfixText.set('');
    this.period6SecondaryLabelPostfixText.set('');
    this.period7SecondaryLabelPostfixText.set('');
    this.period8SecondaryLabelPostfixText.set('');
    this.period9SecondaryLabelPostfixText.set('');
    this.period10SecondaryLabelPostfixText.set('');
    this.period11SecondaryLabelPostfixText.set('');
    this.period12SecondaryLabelPostfixText.set('');
  }
  accountYearIdChanged(value: number) {
    this.setPeriodSecondaryLabelText();
    this.loadAccounts();
  }

  accountDimIdChanged(value: number) {
    this.form?.patchValue({
      accountId: undefined,
    });
    this.loadAccounts();
  }

  loadAccountYearDict(): Observable<ISmallGenericType[]> {
    return this.performLoadAccountYear.load$(
      this.economyService.getAccountYears().pipe(
        tap((data: ISmallGenericType[]) => {
          this.accountYearFilterOptions = data;
          this.accountYearFilterOptions.reverse();
          if (this.form?.isNew && !this.form?.isCopy) {
            if (this.accountYearFilterOptions.length > 0) {
              this.form?.patchValue({
                accountYearId: this.accountYearFilterOptions[0].id,
              });
              this.setPeriodSecondaryLabelText();
            }
          }
        })
      )
    );
  }

  loadAccountDims(): Observable<IAccountDimSmallDTO[]> {
    return this.performLoadAccountDims.load$(
      this.economyService
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
          tap((data: IAccountDimSmallDTO[]) => {
            this.accountDimSmallOptions = data;
          })
        )
    );
  }

  loadAccounts(): void {
    if (this.form?.value.accountDimId && this.form?.value.accountYearId) {
      this.performLoadAccounts.load(
        this.economyService
          .getAccountsSmall(
            this.form?.value.accountDimId,
            this.form?.value.accountYearId
          )
          .pipe(
            tap((data: IAccountSmallDTO[]) => {
              this.accountFilterOptions = orderBy(
                data,
                ['number'],
                ['asc']
              ).map(
                a =>
                  <ISmallGenericType>{
                    id: a.accountId,
                    name: `${a.number} - ${a.name}`,
                  }
              );
              if (
                !this.form?.value.accountId &&
                this.accountFilterOptions.length > 0
              ) {
                this.form?.patchValue({
                  accountId: this.accountFilterOptions[0].id,
                });
              }
            })
          )
      );
    }
  }

  override loadData(): Observable<void> {
    return this.performLoadData.load$(
      this.service.get(this.form?.getIdControl()?.value).pipe(
        tap(value => {
          this.form?.reset(value);
          this.setPeriodSecondaryLabelText();
          if (this.form?.value.accountDimId && this.form?.value.accountYearId) {
            this.loadAccounts();
          }
        })
      )
    );
  }
}

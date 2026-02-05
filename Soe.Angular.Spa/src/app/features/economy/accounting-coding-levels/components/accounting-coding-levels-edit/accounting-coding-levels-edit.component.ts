import { Component, OnInit, inject, signal } from '@angular/core';
import { EditBaseDirective } from '@shared/directives/edit-base/edit-base.directive';
import { AccountDimDTO } from '../../models/accounting-coding-levels.model';
import { AccountingCodingLevelsService } from '../../services/accounting-coding-levels.service';
import {
  AccountDimForm,
  projectShiftSelectionValidator,
} from '../../models/accounting-coding-levels-form.model';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import {
  CompanySettingType,
  Feature,
  SoeEntityState,
  TermGroup,
} from '@shared/models/generated-interfaces/Enumerations';
import { EMPTY, Observable, catchError, of, tap } from 'rxjs';
import { SettingsUtil } from '@shared/util/settings-util';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { EconomyService } from '../../../services/economy.service';
import { IAccountDimDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { ProgressOptions } from '@shared/services/progress';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';
import { ResponseUtil } from '@shared/util/response-util';

@Component({
  selector: 'soe-accounting-coding-levels-edit',
  templateUrl: './accounting-coding-levels-edit.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class AccountingCodingLevelsEditComponent
  extends EditBaseDirective<
    AccountDimDTO,
    AccountingCodingLevelsService,
    AccountDimForm
  >
  implements OnInit
{
  private readonly coreService = inject(CoreService);
  private readonly econService = inject(EconomyService);
  readonly service = inject(AccountingCodingLevelsService);
  protected readonly deleteOptions: ProgressOptions = {
    showToastOnComplete: false,
  };

  private loading: boolean = false;
  private useAccountsHierarchy: boolean = false;
  private resetAccountInternals: boolean = false;
  private accountDimsOriginal: Array<IAccountDimDTO> = [];
  protected dimChars: Array<SmallGenericType> = [];
  protected sieDims: Array<SmallGenericType> = [];
  protected sysAccountStdTypes: Array<SmallGenericType> = [];
  protected accountDims: Array<SmallGenericType> = [];
  protected vatDeductionExists = signal(false);
  protected inactivatePermission: boolean = false;

  ngOnInit(): void {
    super.ngOnInit();
    this.startFlow(Feature.Economy_Accounting_AccountRoles_Edit, {
      additionalModifyPermissions: [
        Feature.Economy_Accounting_AccountRoles_Inactivate,
      ],
      lookups: [
        this.loadChars(),
        this.loadSie(),
        this.loadAccountStdTypes(),
        this.loadAccountDims(),
      ],
    });

    this.form?.addValidators(
      projectShiftSelectionValidator(
        this.translate.instant(
          'economy.accounting.cannotbelinkedtobothprojectandshifttype'
        )
      )
    );
    this.form?.useVatDeduction.valueChanges.subscribe((): void => {
      if (!this.loading) this.useVatDeductionClick();
    });
  }

  override onPermissionsLoaded(): void {
    super.onPermissionsLoaded();

    this.inactivatePermission = this.flowHandler.hasModifyAccess(
      Feature.Economy_Accounting_AccountRoles_Inactivate
    );
  }

  override newRecord(): Observable<void> {
    const clearValues = () => {};
    if (this.form?.isNew || this.form?.isCopy) {
      this.form?.state.reset(SoeEntityState.Active);
      this.form?.isActive.reset(true);
    }

    return of(clearValues());
  }

  override loadData(): Observable<void> {
    this.loading = true;
    return this.performLoadData.load$(
      this.service.get(this.form?.getIdControl()?.value).pipe(
        tap((d: AccountDimDTO): void => {
          this.form?.customPatch(d);
          this.setupAccountDims();
          if (d.isStandard) this.form?.accountDimNr.disable();
          this.loading = false;
        }),
        catchError((): Observable<void> => {
          this.loading = false;
          return EMPTY;
        })
      )
    );
  }

  override loadCompanySettings(): Observable<void> {
    return this.performLoadData.load$(
      this.coreService
        .getCompanySettings([CompanySettingType.UseAccountHierarchy])
        .pipe(
          tap(settings => {
            this.useAccountsHierarchy = SettingsUtil.getBoolCompanySetting(
              settings,
              CompanySettingType.UseAccountHierarchy
            );
          })
        )
    );
  }

  private loadChars(): Observable<void> {
    return this.performLoadData.load$(
      this.service.getAccountDimChars().pipe(
        tap(chars => {
          this.dimChars = chars;
        })
      )
    );
  }

  private loadSie(): Observable<void> {
    return this.performLoadData.load$(
      this.coreService
        .getTermGroupContent(TermGroup.SieAccountDim, false, false)
        .pipe(
          tap(accountDims => {
            this.sieDims = accountDims;
          })
        )
    );
  }

  private loadAccountStdTypes(): Observable<void> {
    return this.performLoadData.load$(
      this.econService.getSysAccountStdTypes().pipe(
        tap(stdTypes => {
          this.sysAccountStdTypes = stdTypes;
        })
      )
    );
  }

  private loadAccountDims(): Observable<void> {
    return this.performLoadData.load$(
      this.econService
        .getAccountDims(false, true, false, false, false, false, false, false)
        .pipe(
          tap(acctDims => {
            this.accountDimsOriginal = acctDims;
            this.setupAccountDims();
          })
        )
    );
  }

  private setupAccountDims(): void {
    this.accountDims = [];
    this.vatDeductionExists.set(
      this.accountDimsOriginal.filter(
        x =>
          x.accountDimId !== this.form?.accountDimId.value && x.useVatDeduction
      ).length > 0
    );

    this.accountDims.push(new SmallGenericType(0, ''));
    this.accountDims = [
      ...this.accountDims,
      ...this.accountDimsOriginal
        .filter(ad => ad.accountDimId !== this.form?.accountDimId.value)
        .map(ad => {
          return new SmallGenericType(ad.accountDimId, ad.name);
        }),
    ];
  }

  private useVatDeductionClick(): void {
    if (this.form?.useVatDeduction.value === true) {
      this.resetAccountInternals = false;
      this.messageboxService
        .warning(
          'core.warning',
          'economy.accounting.accountdim.vatdeductionwarning',
          { buttons: 'okCancel', hideCloseButton: true }
        )
        .afterClosed()
        .subscribe(res => {
          if (res.result) {
            this.resetAccountInternals = true;
          } else {
            this.form?.useVatDeduction.patchValue(false);
          }
        });
    }
  }

  protected triggerSave(): void {
    this.additionalSaveData = { reset: this.resetAccountInternals };
    super.performSave();
  }

  override emitActionDeleted(response: BackendResponse): void {
    const msg = ResponseUtil.getMessageValue(response);
    if (msg?.length) {
      this.messageboxService.success('core.worked', msg);
    }

    // if (!response.objectsAffected) {
    //   this.additionalDeleteProps = {
    //     skipUpdateGrid: true,
    //   };
    // }

    super.emitActionDeleted(response);
  }

  validateAccountDimNr($event: Event) {
    const value = ($event.target as HTMLInputElement).value;
    if (!value) return;

    this.service
      .validateAccountNr(
        Number(value),
        this.form?.accountDimId.getRawValue() ?? 0
      )
      .pipe(
        tap(result => {
          if (!result.success)
            this.messageboxService.warning(
              this.translate.instant('core.warning'),
              ResponseUtil.getErrorMessage(result) ?? '',
              {
                buttons: 'ok',
                size: 'sm',
              }
            );
        })
      )
      .subscribe();
  }
}

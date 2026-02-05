import {
  Component,
  inject,
  OnInit,
  signal,
  ViewChild,
  WritableSignal,
} from '@angular/core';
import { EditBaseDirective } from '@shared/directives/edit-base/edit-base.directive';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import {
  CategoryAccountDTO,
  SaveAccountModel,
} from '../../models/accounts.model';
import { AccountsService } from '../../services/accounts.service';
import { AccountForm } from '../../models/accounts-form.model';
import {
  CompanySettingType,
  CompTermsRecordType,
  Feature,
  SoeCategoryType,
  SoeEntityState,
  SoeEntityType,
  SoeTimeSalaryExportTarget,
  TermGroup,
  TermGroup_SieAccountDim,
} from '@shared/models/generated-interfaces/Enumerations';
import {
  IAccountBalanceDTO,
  IAccountDimDTO,
  IAccountDTO,
  IAccountMappingDTO,
  ICategoryAccountDTO,
  ICompTermDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { map, Observable, take, tap } from 'rxjs';
import { SettingsUtil } from '@shared/util/settings-util';
import { SoeConfigUtil } from '@shared/util/soeconfig-util';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { SupplierService } from '../../../services/supplier.service';
import { EconomyService } from '../../../services/economy.service';
import { ProgressOptions } from '@shared/services/progress';
import { CrudActionTypeEnum } from '@shared/enums';
import { LanguageTranslationsService } from '@shared/features/language-translations/services/language-translations.service';
import { FormControl } from '@angular/forms';
import { IMessageboxComponentResponse } from '@ui/dialog/models/messagebox';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { IRowNode } from 'ag-grid-community';
import { LanguageTranslationsComponent } from '@shared/features/language-translations/language-translations.component';
import { cloneDeep } from 'lodash';
import { IExtraFieldRecordDTO } from '@shared/models/generated-interfaces/ExtraFieldDTO';
import { RequestReportService } from '@shared/services/request-report.service';
import { AccountUrlParamsService } from '../../services/account-params.service';
import { TermCollection } from '@shared/localization/term-types';
import { ResponseUtil } from '@shared/util/response-util';

@Component({
  selector: 'soe-accounts-edit',
  templateUrl: './accounts-edit.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class AccountsEditComponent
  extends EditBaseDirective<SaveAccountModel, AccountsService, AccountForm>
  implements OnInit
{
  @ViewChild(LanguageTranslationsComponent)
  languageTranslationsComponent!: LanguageTranslationsComponent;

  paramService = inject(AccountUrlParamsService);
  service = inject(AccountsService);
  private readonly coreService = inject(CoreService);
  private readonly economyService = inject(EconomyService);
  private readonly languageService = inject(LanguageTranslationsService);
  private readonly supplierService = inject(SupplierService);
  private readonly requestReportService = inject(RequestReportService);
  showExternalCodes: boolean = false;
  categories: SmallGenericType[] = [];
  selectedCategories: SmallGenericType[] = [];
  originalSelectedCategories: SmallGenericType[] = [];
  attestGroups: SmallGenericType[] = [];
  categoryAccounts: ICategoryAccountDTO[] = [];
  originalCategoryAccounts: ICategoryAccountDTO[] = [];
  copiedExtraFieldRecords: IExtraFieldRecordDTO[] = [];
  copiedTranslations: ICompTermDTO[] = [];
  childrenAccounts: IAccountDTO[] = [];
  attestFlowPermission = signal(false);
  toolbarPrintDisabled = signal(false);

  accounts: SmallGenericType[] = [];
  accountTypes: SmallGenericType[] = [];
  fieldTypes: SmallGenericType[] = [];
  compTermsRecordType: number = CompTermsRecordType.AccountName;
  translations: ICompTermDTO[] = [];
  extraFields: IExtraFieldRecordDTO[] = [];
  sysVatAccounts: SmallGenericType[] = [];
  sysAccountSruCodes: SmallGenericType[] = [];
  amountStopTypes: SmallGenericType[] = [];
  accountBalances: IAccountBalanceDTO[] = [];
  originalAccountNr: string = '';

  accountDim?: IAccountDimDTO;
  projectAccountDim!: IAccountDimDTO;
  shiftTypeAccountDim!: IAccountDimDTO;
  accountId!: number;
  parentAccountDimId!: number;
  parentAccountDimName: WritableSignal<string> = signal('');
  isCostPlaceDim: boolean = false;
  balanceExpanderInitiallyOpened: boolean = false;
  calcAllAccounts: FormControl = new FormControl<boolean>(false);

  entityType = SoeEntityType.Account;
  connectedEntityType = SoeEntityType.AccountDim;

  ngOnInit(): void {
    super.ngOnInit();
    this.form!.isStdAccount = this.paramService.isAccountStd;
    this.accountId = this.form?.getIdControl()?.value ?? 0;
    this.startFlow(Feature.Economy_Accounting_Accounts_Edit, {
      additionalReadPermissions: [
        Feature.Economy_Accounting_Accounts_Edit,
        Feature.Economy_Supplier_Invoice_AttestFlow,
      ],
      additionalModifyPermissions: [
        Feature.Economy_Accounting_Accounts_Edit,
        Feature.Economy_Supplier_Invoice_AttestFlow,
        Feature.Common_ExtraFields_Account,
      ],
      lookups: [
        this.loadCompanySettings(),
        this.loadCategories(),
        this.loadAccountDim(),
        this.loadProjectAccountDim(),
        this.loadShiftTypeAccountDim(),
        this.loadAttestGroups(),
        this.loadAccountTypes(),
        this.loadAmountStopTypes(),
        this.loadSysVatAccounts(),
        this.loadSysAccountSruCodes(),
        this.loadAccountBalance(),
        this.loadAccountMappings(),
        this.loadChildrenAccounts(),
      ],
    });

    //Ensure to apply edited data in the grid when clicking outside
    document.body.addEventListener('click', (event: any) => {
      if (!event.target.closest('ag-grid-angular')) {
        this.applyChanges();
      }
    });
  }

  override onFinished() {
    this.form?.markAsUntouched();
    this.form?.markAsPristine();
  }

  override copy() {
    if (!this.form) return;
    this.form.patchValue({
      accountId: 0,
      accountNr: '',
    });

    this.setNewRefOnTab(undefined, true);
    this.form.isCopy = true;
    this.form.resetFormForCopy();

    this.form.markAsDirty();
    this.form.markAsTouched();
  }

  override loadTerms(): Observable<TermCollection> {
    return super.loadTerms(['core.edit']);
  }
  override createEditToolbar(): void {
    super.createEditToolbar();

    this.toolbarService.createItemGroup({
      items: [
        this.toolbarService.createToolbarButton('print', {
          iconName: signal('print'),
          caption: signal('economy.accounting.generalledger'),
          tooltip: signal('economy.accounting.generalledger'),
          disabled: this.toolbarPrintDisabled,
          onAction: () => {
            if (this.accountId > 0) {
              this.printAccount(this.accountId);
            }
          },
        }),
      ],
    });
  }

  override loadData(): Observable<void> {
    return this.performLoadData.load$(
      this.service.get(this.form?.getIdControl()?.value).pipe(
        tap(value => {
          this.accountId = value.accountId;
          this.originalAccountNr = value.accountNr || '';
          this.form?.reset(value);
          this.loadCategoryAccounts();
          this.loadTranslations();
          this.loadAccountMappings().subscribe();
          this.sysVatAccountChanged();
        })
      )
    );
  }

  extraFieldsChanged(items: IExtraFieldRecordDTO[]) {
    this.extraFields = items;
    this.form?.markAsDirty();
  }

  loadCompanySettings() {
    const settingTypes: number[] = [CompanySettingType.SalaryExportTarget];

    return this.coreService.getCompanySettings(settingTypes).pipe(
      tap(x => {
        this.showExternalCodes =
          SettingsUtil.getIntCompanySetting(
            x,
            CompanySettingType.SalaryExportTarget
          ) == SoeTimeSalaryExportTarget.BlueGarden;
      })
    );
  }

  onReCalculateBalances() {
    this.balanceExpanderInitiallyOpened = true;

    if (this.calcAllAccounts.getRawValue()) {
      this.performAction.crud(
        CrudActionTypeEnum.Work,
        this.economyService.calculateAccountBalanceForAccountsAllYears().pipe(
          tap(res => {
            if (!res.success) {
              this.messageboxService.error(
                'core.error',
                ResponseUtil.getErrorMessage(res) ?? ''
              );
            }
          })
        )
      );
    }

    this.performAction.crud(
      CrudActionTypeEnum.Work,
      this.economyService
        .calculateAccountBalanceForAccountInAccountYears(this.accountId)
        .pipe(
          tap(res => {
            if (!res.success) {
              this.messageboxService.error(
                'core.error',
                ResponseUtil.getErrorMessage(res) ?? ''
              );
            }
          })
        ),
      () => this.loadAccountBalance().subscribe()
    );
  }

  private loadCategories() {
    return this.coreService
      .getCategoriesDict(SoeCategoryType.Employee, false)
      .pipe(
        tap(x => {
          this.categories = x;
        })
      );
  }

  private loadCategoryAccounts() {
    this.selectedCategories = [];
    const categoryIds: number[] = [];
    this.coreService
      .getCategoryAccounts(this.accountId, false)
      .pipe(
        map(data => {
          return data.map(row => {
            const category = this.categories.find(c => c.id === row.categoryId);
            if (category) {
              this.selectedCategories.push(
                new SmallGenericType(category.id, category.name)
              );
              categoryIds.push(category.id);
            }

            return row;
          });
        })
      )
      .subscribe((x: ICategoryAccountDTO[]) => {
        this.categoryAccounts = x;
        this.originalCategoryAccounts = cloneDeep(x);
        this.form?.categoryIds.setValue(categoryIds);
      });
  }

  private loadAccountBalance(): Observable<IAccountBalanceDTO[]> {
    return this.economyService
      .getAccountBalanceByAccount(this.accountId, true)
      .pipe(
        take(1),
        tap(x => {
          this.accountBalances = x;
        })
      );
  }

  private loadAttestGroups() {
    return this.supplierService.getAttestWorkFlowGroupsDict(true).pipe(
      tap(x => {
        this.attestGroups = x;
      })
    );
  }

  private loadAccountDim() {
    return this.economyService
      .getAccountDimByAccountDimId(this.paramService.accountDimId, false)
      .pipe(
        tap(x => {
          this.accountDim = x;
          if (
            x.sysSieDimNr == TermGroup_SieAccountDim.CostCentre &&
            this.attestFlowPermission()
          ) {
            this.isCostPlaceDim = true;
          }
          this.parentAccountDimId = x.parentAccountDimId || 0;
          this.form?.accountDimId.setValue(this.paramService.accountDimId);

          if (this.parentAccountDimId > 0) {
            this.loadParentAccountDim().subscribe();
            this.loadAccountsDict();
          }
        })
      );
  }

  private loadParentAccountDim() {
    return this.economyService
      .getAccountDimByAccountDimId(this.parentAccountDimId, false)
      .pipe(
        tap(x => {
          this.parentAccountDimName.set(x.name);
        })
      );
  }

  private loadAccountsDict() {
    if (this.parentAccountDimId) {
      this.economyService
        .getAccountsDict(this.parentAccountDimId, true)
        .pipe(
          tap(x => {
            this.accounts = x;
          })
        )
        .subscribe();
    }
  }

  private loadTranslations(): void {
    this.languageService
      .getTranslations(
        this.compTermsRecordType,
        this.form?.accountId.value,
        true
      )
      .subscribe(x => {
        this.translations = x;
        this.form?.customPatchTranslations(x);
      });
  }
  private loadChildrenAccounts() {
    return this.service.getChildrenAccounts(this.accountId).pipe(
      tap(x => {
        this.childrenAccounts = x;
      })
    );
  }

  private loadAccountMappings() {
    return this.economyService.getAccountMappings(this.accountId || 0).pipe(
      tap(x => {
        this.form?.customPatchAccountMappings(x);
      })
    );
  }

  private loadAccountTypes(): Observable<SmallGenericType[]> {
    return this.coreService
      .getTermGroupContent(TermGroup.AccountType, false, true)
      .pipe(
        tap(x => {
          this.accountTypes = x;
        })
      );
  }

  private loadAmountStopTypes(): Observable<SmallGenericType[]> {
    return this.coreService
      .getTermGroupContent(TermGroup.AmountStop, false, true)
      .pipe(
        tap(x => {
          this.amountStopTypes = x;
        })
      );
  }

  private loadSysVatAccounts() {
    return this.economyService
      .getSysVatAccounts(SoeConfigUtil.sysCountryId, true)
      .pipe(
        tap(x => {
          this.sysVatAccounts = x;
        })
      );
  }

  private loadSysAccountSruCodes() {
    return this.economyService.getSysAccountSruCodes(true).pipe(
      tap(x => {
        this.sysAccountSruCodes = x;
      })
    );
  }

  private loadProjectAccountDim() {
    return this.economyService.getProjectAccountDim().pipe(
      tap(x => {
        this.projectAccountDim = x;
      })
    );
  }

  private loadShiftTypeAccountDim() {
    return this.economyService.getShiftTypeAccountDim(false).pipe(
      tap(x => {
        this.shiftTypeAccountDim = x;
      })
    );
  }

  get isProjectAccountDim(): boolean {
    if (
      this.projectAccountDim &&
      this.paramService.accountDimId &&
      this.projectAccountDim.accountDimId === this.paramService.accountDimId
    ) {
      return true;
    }
    return false;
  }

  get isShiftTypeAccountDim(): boolean {
    if (
      this.shiftTypeAccountDim &&
      this.paramService.accountDimId &&
      this.shiftTypeAccountDim.accountDimId === this.paramService.accountDimId
    ) {
      return true;
    }
    return false;
  }

  applyChanges(): void {
    this.languageTranslationsComponent?.grid?.applyChanges();
  }

  sysVatAccountChanged() {
    if (!this.form?.sysVatAccountId.value) {
      this.form?.sysVatRate.setValue('');
      return;
    }
    this.economyService
      .getAccountSysVatRate(this.form?.sysVatAccountId.value)
      .subscribe(x => {
        this.form?.sysVatRate.setValue(x + ' %');
      });
  }

  override onPermissionsLoaded() {
    super.onPermissionsLoaded();
    this.attestFlowPermission.set(
      this.flowHandler.hasModifyAccess(
        Feature.Economy_Supplier_Invoice_AttestFlow
      )
    );
  }

  addOrRemoveCategoryAccounts(id: number) {
    const account = this.categoryAccounts.find(x => x.categoryId == id);
    if (!account) {
      const oriAccount = this.originalCategoryAccounts.find(
        o => o.categoryId == id
      );
      if (oriAccount) {
        this.categoryAccounts.push(oriAccount);
      } else {
        const catAccount: CategoryAccountDTO = {
          accountId: this.accountId,
          categoryAccountId: 0,
          categoryId: id,
          actorCompanyId: 0,
          state: 0,
        };
        this.categoryAccounts.push(catAccount);
      }
    }
  }

  onCategorySelectionChanged(event: SmallGenericType) {
    this.addOrRemoveCategoryAccounts(event.id);
  }

  override performSave(
    options?: ProgressOptions,
    skipStateValidation: boolean = false
  ): void {
    if (!this.form || this.form.invalid || !this.service) return;
    options = options || {};
    options.showDialogOnError = false;
    const model = new SaveAccountModel();
    model.account = this.form?.getRawValue();
    model.categoryAccounts = [];
    const categoryIds: number[] = this.form?.getRawValue().categoryIds;
    categoryIds.forEach(f => {
      const ca = this.categoryAccounts.find(ca => ca.categoryId == f);
      if (ca) {
        model.categoryAccounts.push(ca);
      }
    });
    model.accountMappings =
      this.form?.accountMappings.getRawValue() as IAccountMappingDTO[];
    model.extraFields = this.extraFields;
    model.skipStateValidation = skipStateValidation;
    this.applyChanges();
    const translations: ICompTermDTO[] = [];
    this.languageTranslationsComponent?.grid?.api.forEachNode(
      (row: IRowNode<ICompTermDTO>) => {
        translations.push(row.data as ICompTermDTO);
      }
    );
    model.translations = this.translations = translations;

    this.form?.customPatchTranslations(
      this.translations.filter(t => t.state !== SoeEntityState.Deleted)
    );

    this.performAction.crud(
      CrudActionTypeEnum.Save,
      this.service.save(model).pipe(
        tap((result: any) => {
          if (!result.success) {
            if (result.canUserOverride) {
              const dialog = this.messageboxService.question(
                this.translate.instant(
                  'time.employee.employee.delete.action.inactivate'
                ),
                result.errorMessage
              );
              dialog
                .afterClosed()
                .subscribe((response: IMessageboxComponentResponse) => {
                  if (response.result) {
                    this.performSave({}, true);
                  }
                });
            } else {
              this.messageboxService.error(
                'time.employee.employee.delete.action.inactivate',
                result.errorMessage,
                {
                  type: 'forbidden',
                }
              );
            }
          }
        })
      ),
      this.updateFormValueAndEmitChange,
      undefined,
      options
    );
  }

  validateAccountNr() {
    if (
      !this.form?.accountNr.getRawValue() ||
      this.form?.accountNr.getRawValue() === this.originalAccountNr
    )
      return;

    this.service
      .validateAccountNr(
        this.form?.accountNr.getRawValue(),
        this.form?.accountId.getRawValue()
          ? this.form?.accountId.getRawValue()
          : 0,
        this.paramService.accountDimId
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

  private printAccount(accountId: number): void {
    this.toolbarPrintDisabled.set(true);
    this.performLoadData.load(
      this.requestReportService.printAccount(accountId).pipe(
        tap(() => {
          this.toolbarPrintDisabled.set(false);
        })
      )
    );
  }

  openEditAccount(account: IAccountDTO) {
    this.openEditInNewTabSignal()?.set({
      id: account.accountId,
      additionalProps: {
        editComponent: AccountsEditComponent,
        FormClass: AccountForm,
        editTabLabel: 'economy.accounting.account',
        isNew: false,
      },
    });
  }
}

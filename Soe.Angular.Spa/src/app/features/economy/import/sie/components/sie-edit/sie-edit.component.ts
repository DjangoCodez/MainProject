import {
  Component,
  OnInit,
  inject,
  signal,
  computed,
  DestroyRef,
  ViewChild,
} from '@angular/core';
import { EditBaseDirective } from '@shared/directives/edit-base/edit-base.directive';
import { SieService } from '../../services/sie.service';
import { SieImportForm } from '../../models/sie-form.model';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { distinctUntilChanged, Observable, tap } from 'rxjs';
import { AccountingService } from '@features/economy/services/accounting.service';
import { VoucherSeriesDTO } from '@features/economy/account-years-and-periods/models/account-years-and-periods.model';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { ValidationHandler } from '@shared/handlers';
import {
  AttachedFile,
  FileUploadComponent,
} from '@ui/forms/file-upload/file-upload.component';
import { DialogData } from '@ui/dialog/models/dialog';
import { DialogService } from '@ui/dialog/services/dialog.service';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { SieUploadService } from '../../services/sie-upload.service';
import {
  ISieAccountDimMappingDTO,
  ISieImportConflictDTO,
  ISieImportPreviewDTO,
} from '@shared/models/generated-interfaces/SieImportDTO';
import { DateUtil } from '@shared/util/date-util';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { IAccountDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { AccountDTO } from '@shared/components/accounting-rows/models/accounting-rows-model';
import { SieImportHistoryDialogComponent } from '../sie-import-history-dialog/sie-import-history-dialog.component';
import { AccountDimDTO } from '@features/economy/accounting-coding-levels/models/accounting-coding-levels.model';
import { SieImportVoucheriesMappingForm } from '../../models/sie-series-selection-form.model';

@Component({
  selector: 'soe-economy-import-sie',
  templateUrl: './sie-edit.component.html',
  styleUrls: ['./sie-edit.component.scss'],
  providers: [FlowHandlerService, ToolbarService, SieUploadService],
  standalone: false,
})
export class SieEditComponent
  extends EditBaseDirective<any, SieService, SieImportForm>
  implements OnInit
{
  @ViewChild(FileUploadComponent)
  fileUploadRef!: FileUploadComponent;

  private readonly destroyRef = inject(DestroyRef);
  readonly coreService = inject(CoreService);
  service = inject(SieService);
  validationHandler = inject(ValidationHandler);
  accountingService = inject(AccountingService);
  uploadService = inject(SieUploadService);
  dialogService = inject(DialogService);

  accounts: AccountDTO[] = [];
  dimInternals: AccountDimDTO[] = [];
  stdAccounts: AccountDTO[] = [];

  accountingYears: Array<ISmallGenericType> = [];
  voucherSeries: Array<VoucherSeriesDTO> = [];
  voucherSeriesTypes: Array<ISmallGenericType> = [];

  conflictRows = signal<ISieImportConflictDTO[]>([]);
  hasConflictRows = computed(() => this.conflictRows().length > 0);
  hasDimErrors = signal(false);
  hasAccountErrors = signal(true);

  preview = signal<ISieImportPreviewDTO | null>(null);

  hasPreview = computed(() => {
    return this.preview();
  });

  hasPreviewErrors = computed(() => {
    const previewData = this.preview?.();
    if (!previewData) return false;
    return !(
      previewData.fileContainsAccountStd &&
      previewData.fileContainsAccountBalances &&
      previewData.fileContainsVouchers
    );
  });

  previewErrors = computed(() => {
    const previewData = this.preview?.();
    const errors: string[] = [];
    if (!previewData) return errors;

    if (!this.preview()?.fileContainsAccountStd)
      errors.push(
        this.translate.instant(
          'economy.import.sie.preview.message.missingchartofaccounts'
        )
      );
    if (!this.preview()?.fileContainsAccountBalances)
      errors.push(
        this.translate.instant(
          'economy.import.sie.preview.message.missingopeningbalances'
        )
      );
    if (!this.preview()?.fileContainsVouchers)
      errors.push(
        this.translate.instant(
          'economy.import.sie.preview.message.missingverificationentries'
        )
      );
    return errors;
  });

  importIsProcessing = signal(false);

  noYearFoundMessage = computed(() => {
    const previewData = this.preview();
    if (previewData && previewData.accountingYearId) return '';

    if (!previewData?.accountingYearFrom || !previewData?.accountingYearTo)
      return ''; //TODO: Should add message that no account year identified in file.

    const from = DateUtil.localeDateFormat(
      new Date(previewData.accountingYearFrom)
    );
    const to = DateUtil.localeDateFormat(
      new Date(previewData.accountingYearTo)
    );
    return this.translate
      .instant('economy.import.sie.nomatchingyearfound')
      .replace('{0}', `${from} - ${to}`);
  });

  private toolbarClearDisabled = signal(true);

  override ngOnInit(): void {
    super.ngOnInit();
    this.form = new SieImportForm({
      validationHandler: this.validationHandler,
    });

    this.startFlow(Feature.Economy_Import_Sie, {
      lookups: [
        this.loadAccountingYears(),
        this.loadAccounts(),
        this.loadStdAccounts(),
        this.loadAccountDimInternals(),
      ],
    });

    this.setSubscriptions();
    this.setDefalutControlValues(true);
    this.form?.get('importAccounts')?.disable();
  }
  private setSubscriptions() {
    this.form?.accountYearId.valueChanges
      .pipe(distinctUntilChanged(), takeUntilDestroyed(this.destroyRef))
      .subscribe(accountYearId => {
        this.voucherSeries = [];
        this.voucherSeriesTypes = [];
        if (accountYearId) this.loadVoucherSeries(accountYearId);
      });
    this.form?.importAccountInternal.valueChanges.subscribe(internal => {
      this.preview()?.accountDims.forEach(dim => {
        dim.isImport = internal;
      });
    });
    this.form?.importAccounts.valueChanges.subscribe(importAccount => {
      this.setDefalutControlValues(importAccount);
    });
    this.form?.preview.accountDims.valueChanges.subscribe((dim: any[]) => {
      this.hasDimErrors.set(false);
      if (dim.length > 0)
        dim.forEach((d: ISieAccountDimMappingDTO) => {
          if (!d.isImport) {
            this.hasDimErrors.set(true);
          }
        });
    });
  }
  private setDefalutControlValues(importAccountStd = true) {
    this.form?.get('importAccountStd')?.patchValue(importAccountStd);
  }
  private loadAccounts(): Observable<IAccountDTO[]> {
    return this.performLoadData.load$(
      this.accountingService
        .getAccountsInternalsByCompany(true, true, true)
        .pipe(
          tap(x => {
            this.accounts = x;
          })
        )
    );
  }

  private loadAccountDimInternals(): Observable<AccountDimDTO[]> {
    return this.performLoadData.load$(
      this.accountingService.getAccountDimInternals().pipe(
        tap(x => {
          this.dimInternals = x;
        })
      )
    );
  }

  private loadStdAccounts(): Observable<IAccountDTO[]> {
    return this.performLoadData.load$(
      this.accountingService.getStdAccounts().pipe(
        tap(x => {
          this.stdAccounts = x;
        })
      )
    );
  }

  override createEditToolbar(): void {
    this.toolbarService.createItemGroup({
      items: [
        this.toolbarService.createToolbarButton('sie.import.history', {
          iconName: signal('clock-rotate-left'),
          caption: signal('common.history'),
          tooltip: signal('common.history'),
          onAction: this.showImportHistoryDialog.bind(this),
        }),
      ],
    });
    this.toolbarService.createItemGroup({
      items: [
        this.toolbarService.createToolbarButton('clear', {
          caption: signal('common.clear'),
          tooltip: signal('common.clear'),
          disabled: this.toolbarClearDisabled,
          onAction: this.clearAllSieImport.bind(this),
        }),
      ],
    });
  }

  private loadAccountingYears() {
    return this.performLoadData.load$(
      this.accountingService.getAccountYearDict(false, true).pipe(
        tap(years => {
          this.accountingYears = years;
        })
      )
    );
  }

  private loadVoucherSeries(accountingYearId: number) {
    return this.performAction.load(
      this.accountingService
        .getVoucherSeriesByYear(accountingYearId, false)
        .pipe(
          tap(series => {
            this.voucherSeries = series;
            this.voucherSeriesTypes = series.map(s => ({
              id: s.voucherSeriesTypeId,
              name: s.voucherSeriesTypeName,
            }));
            this.form?.setVoucherSeries(series);
          })
        )
    );
  }

  isImportValueChanged(value: boolean, dim: ISieAccountDimMappingDTO) {
    if (this.form?.importAccounts.value) {
      dim.isImport = value;
    }
    return;
  }

  isAccountStdImportValueChanged(value: boolean) {
    const pv = this.preview();
    if (!pv) return;
    this.preview.set(pv);
    this.form?.preview.accountStd.patchValue({ isImport: value });
  }

  triggerImport(): void {
    this.importIsProcessing.set(true);
    const importModel = this.form?.value;

    const map: { [key: string]: number } = {};
    this.form?.voucherSeriesMappings.getRawValue().forEach(m => {
      map[m.number] = m.voucherSeriesTypeId;
    });
    importModel.voucherSeriesTypesMappingDict = map;
    importModel.sieImportPreview = this.form?.preview?.value;

    this.performAction.load(
      this.service.import(importModel).pipe(
        tap(result => {
          this.importIsProcessing.set(false);
          if (!result) return;

          if (result.success)
            this.messageboxService.success(
              this.translate.instant('common.status'),
              result.message
            );
          else
            this.messageboxService.error(
              this.translate.instant('core.error'),
              result.message
            );

          this.conflictRows.set(result.importConflicts);
        })
      )
    );
  }

  sieAccountDimMappingChanged(previewDim: ISieAccountDimMappingDTO) {}
  sieAccountStdMappingChanged(previewStd: ISieAccountDimMappingDTO) {
    this.form?.preview.accountStd.patchValue(previewStd);
    this.form?.preview.accountStd.customPatchValues(previewStd.accountMappings);
  }

  afterFilesAttached = (files: AttachedFile[]): void => {
    this.clearSieImport();
  };

  afterFilesUploaded = (result: {
    files: AttachedFile[];
    success: boolean;
  }): void => {
    const preview = this.uploadService.filePreview;
    if (result.success && preview) {
      this.form?.patchValue({
        file: this.uploadService.file,
        accountYearId: preview.accountingYearId,
        fileHasAccounts: preview.fileContainsAccountStd,
        fileHasVouchers: preview.fileContainsVouchers,
        fileHasIngoingBalance: preview.fileContainsAccountBalances,
        importAccountStd: preview.fileContainsAccountStd,
        importAccountInternal: preview.fileContainsAccountStd,
        skipAlreadyExistingVouchers: preview.fileContainsVouchers,
      });
      this.form?.addVoucherSeriesMappings(preview.voucherSeriesMappings);
      this.setToolbarClearButtonDisability(false);
    }
    this.setPreviewData(preview);
  };

  private setPreviewData(preview: ISieImportPreviewDTO | undefined) {
    if (preview) {
      if (preview.fileContainsAccountStd) {
        this.form?.importAccounts.enable();
      } else {
        this.form?.get('importAccounts')?.patchValue(false);
        this.form?.importAccounts.disable();
      }
      if (preview.fileContainsAccountBalances)
        this.form?.importAccountBalances.enable();
      else this.form?.importAccountBalances.disable();
      if (preview.fileContainsVouchers) this.form?.importVouchers.enable();
      else this.form?.importVouchers.disable();

      preview.accountStd.isImport = this.form?.importAccountStd.value;
      preview.accountDims.forEach(dim => {
        dim.isImport =
          this.form?.importAccounts.value &&
          this.form.importAccountInternal.value;
      });
      this.conflictRows.set(preview.conflicts);
      this.preview.set(preview);
      this.form?.customPatchValue(preview);
      this.setDimControllDisability();
      setTimeout(() => {
        this.setVoucherControllDisability();
      }, 100);
    }
  }

  setDimControllDisability() {
    this.form?.preview.accountDims.controls.forEach(dim => {
      if (this.form?.importAccounts.value) dim.isImport.enable();
      else dim.isImport.disable();
    });
  }

  setVoucherControllDisability() {
    this.form?.voucherSeriesMappings.controls.forEach(
      (voucher: SieImportVoucheriesMappingForm) => {
        voucher.get('voucherNrFrom')?.disable();
        voucher.get('voucherNrTo')?.disable();
      }
    );
  }

  private showImportHistoryDialog(): void {
    this.dialogService.open(SieImportHistoryDialogComponent, <
      Partial<DialogData>
    >{
      title: 'economy.import.sie.reverse.gird.dialog.title',
      size: 'xl',
      hideFooter: true,
    });
  }

  setToolbarClearButtonDisability(isDisabled: boolean): void {
    this.toolbarClearDisabled.set(isDisabled);
  }

  clearAllSieImport(): void {
    this.uploadService.file = undefined;
    this.fileUploadRef.clearAllFiles();
    this.clearSieImport();
  }

  clearSieImport(): void {
    this.form?.doReset();
    this.preview.set(null);
    this.conflictRows.set([]);
    this.hasDimErrors.set(false);
    this.hasAccountErrors.set(true);
    this.importIsProcessing.set(false);
    this.setToolbarClearButtonDisability(true);
  }
}

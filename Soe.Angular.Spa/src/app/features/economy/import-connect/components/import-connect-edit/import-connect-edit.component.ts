import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { VoucherSeriesDTO } from '@features/economy/account-years-and-periods/models/account-years-and-periods.model';
import { AccountStdNumberNameDTO } from '@features/economy/models/account-std.model';
import { AccountingService } from '@features/economy/services/accounting.service';
import {
  AccountDims,
  AccountDimsForm,
  SelectedAccounts,
  SelectedAccountsChangeSet,
} from '@shared/components/account-dims/account-dims-form.model';
import { EditBaseDirective } from '@shared/directives/edit-base/edit-base.directive';
import { CrudActionTypeEnum } from '@shared/enums';
import { ValidationHandler } from '@shared/handlers';
import { TermCollection } from '@shared/localization/term-types';
import {
  Feature,
  SoeModule,
  TermGroup,
  TermGroup_IOImportHeadType,
} from '@shared/models/generated-interfaces/Enumerations';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import { IImportDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import {
  ISysImportDefinitionDTO,
  ISysImportHeadDTO,
} from '@shared/models/generated-interfaces/SOESysModelDTOs';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ProgressService } from '@shared/services/progress/progress.service';
import { ProgressOptions } from '@shared/services/progress';
import { Guid } from '@shared/util/string-util';
import { Perform } from '@shared/util/perform.class';
import { addEmptyOption } from '@shared/util/array-util';
import { AttachedFile } from '@ui/forms/file-upload/file-upload.component';
import { DialogService } from '@ui/dialog/services/dialog.service';
import { IMessageboxComponentResponse } from '@ui/dialog/models/messagebox';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { map, Observable, take, tap } from 'rxjs';
import { FileUploader } from '../../models/file-uploader';
import { ImportConnectFormModel } from '../../models/import-connect-form.model';
import { ImportDTO, SimpleFile } from '../../models/import-connect.model';
import { ImportConnectService } from '../../services/import-connect.service';
import { PersistedAccountingYearService } from '@features/economy/services/accounting-year.service';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';
import { ResponseUtil } from '@shared/util/response-util';

@Component({
  selector: 'soe-import-connect-edit',
  templateUrl: './import-connect-edit.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class ImportConnectEditComponent
  extends EditBaseDirective<
    IImportDTO,
    ImportConnectService,
    ImportConnectFormModel
  >
  implements OnInit
{
  service = inject(ImportConnectService);
  coreService = inject(CoreService);
  accountingService = inject(AccountingService);
  dialogService = inject(DialogService);
  progressService = inject(ProgressService);
  ayService = inject(PersistedAccountingYearService);

  performImport = new Perform<BackendResponse>(this.progressService);

  module: number = SoeModule.Economy;
  importDefinitions!: ISysImportDefinitionDTO[];
  definitionTypes!: ISmallGenericType[];
  sysImportHeads!: ISysImportHeadDTO[];
  accountYears!: ISmallGenericType[];
  accountStds!: AccountStdNumberNameDTO[];
  voucherSeries!: Observable<VoucherSeriesDTO[]>;
  isCustomerInvoiceRow: boolean = false;
  isCustomerInvoice: boolean = false;
  isVoucher: boolean = false;
  showAccountingYear: boolean = false;
  fileUploadInitiallyOpen: boolean = true;
  showImportRows: boolean = false;
  fileUploader = new FileUploader(this.coreService);

  updateAttachedFiles: string[] = [];
  uploadedFiles = signal<SimpleFile[]>([]);
  somethingToImport = computed(() =>
    this.uploadedFiles().some(f => !f.isImported)
  );

  accountDimsForm!: AccountDimsForm;
  accounts!: AccountDims;
  parentGuid!: Guid;
  batchId!: string;
  importHeadType!: number;

  ngOnInit(): void {
    super.ngOnInit();

    this.startFlow(Feature.Economy_Import_XEConnect, {
      lookups: [
        this.loadSysImportDefinitions(),
        this.loadSysImportHeads(),
        this.loadDefinitionTypes(),
        this.loadAccountYears(),
        this.loadAccountStd(),
        this.ayService.loadSelectedAccountYear(),
      ],
    });

    if (this.form?.isNew) {
      this.form?.reset(this.new());
    }

    this.accountDimsForm = new AccountDimsForm({
      accountDimsValidationHandler: new ValidationHandler(
        this.translate,
        this.messageboxService
      ),
      element: this.accounts,
    });
  }

  override loadData(): Observable<void> {
    return this.performLoadData.load$(
      this.ayService.ensureAccountYearIsLoaded$(() =>
        (<ImportConnectService>this.service)
          .get(this.form?.getIdControl()?.value)
          .pipe(
            tap(value => {
              let importDto = value;
              this.accounts = {
                account1: value.dim1AccountId ?? 0,
                account2: value.dim2AccountId ?? 0,
                account3: value.dim3AccountId ?? 0,
                account4: value.dim4AccountId ?? 0,
                account5: value.dim5AccountId ?? 0,
                account6: value.dim6AccountId ?? 0,
              };
              // this.files = this.form?.gridData.files;

              this.accountDimsForm.reset(this.accounts);

              //reset form control visibility
              importDto = this.initializeImportFlags(importDto);

              this.form?.reset(importDto);
              if (this.form?.gridData.files) {
                this.uploadedFiles.set(this.form?.gridData.files);
                this.importFiles();
              }
            })
          )
      )
    );
  }

  private initializeImportFlags(importDto: IImportDTO): IImportDTO {
    this.isCustomerInvoice = false;
    this.isCustomerInvoiceRow = false;
    this.isVoucher = false;
    this.showAccountingYear = false;

    switch (importDto.importHeadType) {
      case TermGroup_IOImportHeadType.CustomerInvoice:
        this.isCustomerInvoice = true;
        break;
      case TermGroup_IOImportHeadType.CustomerInvoiceRow:
        this.isCustomerInvoiceRow = true;
        break;
      case TermGroup_IOImportHeadType.Voucher:
        this.isVoucher = true;
        break;
      case TermGroup_IOImportHeadType.Budget:
        if (importDto.specialFunctionality !== 'ICABudget') break;

        this.showAccountingYear = true;
        if (!importDto.accountYearId) {
          importDto.accountYearId = this.ayService.selectedAccountYearId();
        }
        break;
    }

    if (importDto.accountYearId && importDto.accountYearId > 0) {
      this.accountYearChanged(importDto.accountYearId);
    }

    return importDto;
  }

  override loadTerms(): Observable<TermCollection> {
    return super.loadTerms([
      'common.connect.imports',
      'core.fileupload.choosefiletoimport',
      'core.error',
      'core.info',
      'common.connect.fileuploadnotsuccess',
      'common.connect.importsuccess',
      'common.connect.importnotsuccess',
      'common.connect.fileallreadyimported',
    ]);
  }

  override performSave(options?: ProgressOptions, skipLoadData = false): void {
    if (!this.form || this.form.invalid || !this.service) return;
    const formValue = this.form.getRawValue();
    if (formValue.voucherSeriesId == 0) formValue.voucherSeriesId = undefined;
    this.performAction.crud(
      CrudActionTypeEnum.Save,
      this.service.save(formValue).pipe(
        tap(res => {
          this.updateFormValueAndEmitChange(res, skipLoadData);
          if (res.success) this.triggerCloseDialog(res);
        })
      ),
      undefined,
      undefined,
      options
    );
  }

  loadSysImportDefinitions(): Observable<ISysImportDefinitionDTO[]> {
    return this.service.getSysImportDefinitions(this.module).pipe(
      take(1),
      tap(x => {
        this.importDefinitions = x;
      })
    );
  }

  loadSysImportHeads(): Observable<ISysImportHeadDTO[]> {
    return this.service.getSysImportHeads().pipe(
      take(1),
      tap(x => {
        this.sysImportHeads = x;
      })
    );
  }

  loadDefinitionTypes(): Observable<ISmallGenericType[]> {
    return this.coreService
      .getTermGroupContent(TermGroup.SysImportDefinitionType, false, false)
      .pipe(
        take(1),
        tap(x => {
          this.definitionTypes = x;
        })
      );
  }

  loadAccountStd() {
    return this.accountingService.getAccountStdsNumberName(true).pipe(
      take(1),
      tap(x => (this.accountStds = x))
    );
  }

  loadAccountYears(): Observable<ISmallGenericType[]> {
    return this.accountingService.getAccountYearDict(false).pipe(
      take(1),
      tap(x => {
        this.accountYears = x.reverse();
      })
    );
  }

  accountYearChanged(accountYearId: number): Observable<VoucherSeriesDTO[]> {
    this.voucherSeries = this.accountingService
      .getVoucherSeriesByYear(accountYearId, false)
      .pipe(
        take(1),
        map(vss => {
          vss = vss.map(vs => {
            vs.voucherSeriesTypeName = `${vs.voucherSeriesTypeNr}. ${vs.voucherSeriesTypeName}`;
            return vs;
          });
          addEmptyOption(vss);
          return vss;
        })
      );

    return this.voucherSeries;
  }

  accountDimsChanged(dimsChanged: SelectedAccountsChangeSet): void {
    this.form?.markAsDirty();
    this.form?.dim2AccountId.setValue(
      dimsChanged.selectedAccounts.account2?.accountId ?? 0
    );
    this.form?.dim3AccountId.setValue(
      dimsChanged.selectedAccounts.account3?.accountId ?? 0
    );
    this.form?.dim4AccountId.setValue(
      dimsChanged.selectedAccounts.account4?.accountId ?? 0
    );
    this.form?.dim5AccountId.setValue(
      dimsChanged.selectedAccounts.account5?.accountId ?? 0
    );
    this.form?.dim6AccountId.setValue(
      dimsChanged.selectedAccounts.account6?.accountId ?? 0
    );
  }

  onFilesUploaded(result: { success: boolean; files: AttachedFile[] }): void {
    const { success, files } = result;
    const fileBatch = this.fileUploader.fileLookup;
    if (!success) {
      this.messageboxService.error(
        this.translate.instant('core.error'),
        this.translate.instant('common.api.importfile.error')
      );
      this.uploadedFiles.set(
        this.uploadedFiles().map(f => ({ ...f, isDoingWork: false }))
      );
      return;
    }

    if (!files || files.length === 0) return;
    this.uploadedFiles.set([
      ...this.uploadedFiles(),
      ...fileBatch.files.map(f => ({
        dataStorageId: f.dataStorageId,
        fileName: f.fileName,
        isImported: false,
        fileSize: f.file?.size ?? 0,
        fileType: '',
        isDuplicate: false,
        isDoingWork: true,
      })),
    ]);

    this.service
      .checkForDuplicates(fileBatch)
      .pipe(
        tap(duplicates => {
          this.uploadedFiles.set(
            this.uploadedFiles().map(f => {
              f.isDoingWork = false;
              return f;
            })
          );

          const duplicateRows: SimpleFile[] = [];

          duplicates.forEach(d => {
            const file = this.uploadedFiles().find(x => x.fileName == d);
            if (file) {
              file.isDuplicate = true;
              duplicateRows.push(file);
            }
          });

          if (duplicates.length > 0) {
            const messageTermKey =
              duplicates.length > 1
                ? 'common.connect.filesExistWarning'
                : 'common.connect.fileExistsWarning';
            const message = `${this.translate.instant(messageTermKey)}\n\n${duplicates.join('\n')}`;
            this.messageboxService
              .question(this.translate.instant('core.error'), message)
              .afterClosed()
              .subscribe((userResponse: IMessageboxComponentResponse) => {
                if (userResponse.result) {
                  //Do remove duplicates.
                  this.uploadedFiles.set(
                    this.uploadedFiles().filter(
                      f => !duplicateRows.some(d => d.fileName == f.fileName)
                    )
                  );
                  this.updateAttachedFiles = this.uploadedFiles().map(
                    f => f.fileName
                  );
                }
                this.importFiles();
              });
          } else {
            // No duplicates found, proceed with import
            this.importFiles();
          }
        })
      )
      .subscribe();

    this.fileUploader.fileLookup.files = [];
  }

  public importFiles() {
    const dataStorageIds = this.uploadedFiles()
      .filter(f => {
        if (f.isImported) return false;
        f.isDoingWork = true;
        return true;
      })
      .map(f => f.dataStorageId);

    if (!this.form) return;
    this.performImport.crud(
      CrudActionTypeEnum.Save,
      this.service
        .importFiles(
          this.form.importId.getRawValue(),
          dataStorageIds,
          this.form.accountYearId.getRawValue(),
          this.form.voucherSeriesId.getRawValue(),
          this.form.importDefinitionId.getRawValue()
        )
        .pipe(
          take(1),
          tap((result: BackendResponse) => {
            const entityId = ResponseUtil.getEntityId(result);
            if (
              result.success &&
              (entityId == TermGroup_IOImportHeadType.Supplier ||
                entityId == TermGroup_IOImportHeadType.SupplierInvoice ||
                entityId == TermGroup_IOImportHeadType.SupplierInvoiceAnsjo ||
                entityId == TermGroup_IOImportHeadType.Customer ||
                entityId == TermGroup_IOImportHeadType.CustomerInvoice ||
                entityId == TermGroup_IOImportHeadType.CustomerInvoiceRow ||
                entityId == TermGroup_IOImportHeadType.Voucher ||
                entityId == TermGroup_IOImportHeadType.Project)
            ) {
              //load import data to dynamic grid
              this.batchId = ResponseUtil.getStringValue(result);
              this.importHeadType = entityId;
              this.showImportRows = true;
              this.form?.name.disable();
              this.form?.importDefinitionId.disable();
              this.uploadedFiles.set(
                this.uploadedFiles().map(f => {
                  if (dataStorageIds.includes(f.dataStorageId)) {
                    f.isDoingWork = false;
                    f.isImported = true;
                  }
                  return f;
                })
              );
            } else {
              this.showImportRows = false;
              this.uploadedFiles.set(
                this.uploadedFiles().map(f => {
                  if (dataStorageIds.includes(f.dataStorageId)) {
                    f.isDoingWork = false;
                  }
                  return f;
                })
              );
            }
          })
        )
    );
  }

  importDefinitionChanged(item: number): void {
    const importDto = this.form?.getRawValue();
    const importDefinition = this.importDefinitions.find(
      i => i.sysImportDefinitionId == item
    );
    const sysImportHead = this.sysImportHeads.find(
      i => i.sysImportHeadId == importDefinition?.sysImportHeadId
    );

    if (
      sysImportHead?.sysImportHeadTypeId ==
      TermGroup_IOImportHeadType.CustomerInvoice
    )
      this.isCustomerInvoice = true;
    else this.isCustomerInvoice = false;
    if (
      sysImportHead?.sysImportHeadTypeId ==
      TermGroup_IOImportHeadType.CustomerInvoiceRow
    )
      this.isCustomerInvoiceRow = true;
    else this.isCustomerInvoiceRow = false;
    if (
      sysImportHead?.sysImportHeadTypeId == TermGroup_IOImportHeadType.Voucher
    ) {
      importDto.accountYearId = this.ayService.selectedAccountYearId();
      this.accountYearChanged(importDto.accountYearId);
      this.isVoucher = true;
    } else {
      if (this.showAccountingYear)
        importDto.accountYearId = this.ayService.selectedAccountYearId();
      else importDto.accountYearId = undefined;

      importDto.voucherSeriesId = undefined;
      this.isVoucher = false;
    }

    const definitionType = this.definitionTypes.find(
      type => type.id == importDefinition?.type
    );

    if (
      definitionType?.name &&
      definitionType?.id != null &&
      importDefinition?.sysImportDefinitionId
    ) {
      importDto.typeText = definitionType?.name;
      importDto.type = definitionType?.id;
      importDto.importDefinitionId = importDefinition?.sysImportDefinitionId;
    }

    this.form?.reset(importDto);
  }

  private new() {
    const importDto = new ImportDTO();
    importDto.isStandard = true;
    importDto.isStandardText = '';
    importDto.module = this.module;
    importDto.updateExistingInvoice = false;
    importDto.useAccountDistribution = false;
    importDto.useAccountDimensions = false;

    this.fileUploadInitiallyOpen = false;
    this.accounts = {
      account1: 0,
      account2: 0,
      account3: 0,
      account4: 0,
      account5: 0,
      account6: 0,
    };

    return importDto;
  }
}

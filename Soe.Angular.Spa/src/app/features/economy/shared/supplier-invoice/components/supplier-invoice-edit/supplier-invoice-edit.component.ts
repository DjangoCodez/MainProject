import {
  Component,
  computed,
  DestroyRef,
  inject,
  OnInit,
  Signal,
  signal,
  viewChild,
} from '@angular/core';
import {
  SupplierInvoiceCostAllocationDTO,
  SupplierInvoiceDTO,
} from '@features/economy/shared/supplier-invoice/models/supplier-invoice.model';
import { EditBaseDirective } from '@shared/directives/edit-base/edit-base.directive';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import {
  constructIdField,
  deconstructIdField,
  SupplierInvoiceForm,
} from '../../models/supplier-invoice-form.model';
import { SupplierInvoiceService } from '../../services/supplier-invoice.service';
import {
  Feature,
  SupplierAccountType,
  SoeEntityType,
  SoeDataStorageRecordType,
} from '@shared/models/generated-interfaces/Enumerations';
import { SupplierInvoiceLoaderService } from '../../services/supplier-invoice-loader.service';
import { SupplierInvoiceFeatureService } from '../../services/supplier-invoice-feature.service';
import { InvoiceFieldClasses, InvoiceIds } from '../../models/utility-models';
import {
  combineLatest,
  debounceTime,
  Observable,
  of,
  Subscription,
  tap,
} from 'rxjs';
import {
  IEdiEntryDTO,
  ISupplierDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { AutoHeightService } from '@shared/directives/auto-height/auto-height.service';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { SupplierEditInputParameters } from '@features/economy/suppliers/models/edit-parameters.model';
import { InvoicePaymentConditionService } from '../../domain-services/payment-condition.service';
import { CurrencyService } from '@shared/services/currency.service';
import { InvoiceVatService } from '../../domain-services/vat.service';
import { AccountingRowDTO } from '@shared/components/accounting-rows/models/accounting-rows-model';
import {
  CreateAccountingRowContext,
  CreateAccountingRowParams,
  GenerateAccountingRowsInput,
  InvoiceAccountingRowsService,
} from '../../domain-services/accounting-rows.service';
import { FilesHelper } from '@shared/components/files-helper/files-helper.component';
import { TraceRowPageName } from '@shared/components/trace-rows/models/trace-rows.model';
import { CostAllocationLoaderService } from '../../services/cost-allocation-loader.service';
import { SupplierInvoiceProjectOrderLoaderService } from '../../services/supplier-invoice-projectorder-loader.service';
import { IProjectTinyDTO } from '@shared/models/generated-interfaces/ProjectDTOs';
import { ICustomerInvoiceSmallGridDTO } from '@shared/models/generated-interfaces/CustomerInvoiceDTOs';
import { SupplierInvoiceSettingsService } from '../../services/supplier-invoice-settings.service';
import { CostAllocationComponent } from '../cost-allocation/cost-allocation';
import { DatePipe } from '@angular/common';

/**
 * Known todos:
 * - Saving
 * - Toolbar
 * - Syncing with grid
 * - All expanders...
 * - Etc.
 */
@Component({
  selector: 'soe-supplier-invoice-edit',
  templateUrl: './supplier-invoice-edit.component.html',
  styleUrls: ['./supplier-invoice-edit.component.scss'],
  providers: [
    FlowHandlerService,
    ToolbarService,
    AutoHeightService,
    InvoicePaymentConditionService,
    InvoiceVatService,
    CurrencyService,
    DatePipe,
  ],
  standalone: false,
})
export class SupplierInvoiceEditComponent
  extends EditBaseDirective<
    SupplierInvoiceDTO,
    SupplierInvoiceService,
    SupplierInvoiceForm
  >
  implements OnInit
{
  public readonly service = inject(SupplierInvoiceService);
  private readonly loaderService = inject(SupplierInvoiceLoaderService);
  private readonly projectOrderLoaderService = inject(
    SupplierInvoiceProjectOrderLoaderService
  );
  private readonly destroyRef = inject(DestroyRef);
  protected readonly featureService = inject(SupplierInvoiceFeatureService);
  private readonly datePipe = inject(DatePipe);

  private readonly accountingRowsService = inject(InvoiceAccountingRowsService);
  private readonly settingService = inject(SupplierInvoiceSettingsService);
  private readonly vatService = inject(InvoiceVatService);
  protected readonly costAllocationLoaderService = inject(
    CostAllocationLoaderService
  );

  costAllocationComponent = viewChild(CostAllocationComponent);

  headFieldClasses = signal<InvoiceFieldClasses>({});
  supplier = signal<ISupplierDTO | null>(null);
  supplierEditInput = signal<SupplierEditInputParameters | null>(null);

  ids = signal<InvoiceIds>({});
  idsSubscription$!: Subscription;

  isLocked = signal(true);
  invoiceIdField = signal('');
  lockIcon = computed(() => (this.isLocked() ? 'lock-open' : 'lock'));
  isCostAllocationOpen = signal(false);

  filesHelper!: FilesHelper;
  isFileDisplayAccordionOpen = signal(false);
  isCostAllocationDataLoaded = signal(false);
  supplierInvoiceId = computed(() => this.ids().invoiceId ?? 0);

  traceRowsRendered = signal(false);
  traceRowPageName = TraceRowPageName.SupplierInvoice;

  productRowsRendered = signal(false);

  private readonly ediEntry = signal<IEdiEntryDTO | null>(null);
  private readonly receivedDateTime: Signal<Date | undefined> = computed(
    () => this.ediEntry()?.created ?? undefined
  );

  // Signal to track invoice number changes
  private readonly invoiceNrSignal = signal<string | null>(null);

  protected readonly headerTitle = computed(() => {
    const invoiceNr = this.invoiceNrSignal();
    let title: string = ''
    
    if (this.form!.isNew) {
      title = `${this.translate.instant(
        'economy.supplier.invoice.new')}`;
    } else if (invoiceNr) {
      title = `${this.translate.instant(
        'economy.supplier.invoice.invoicenr')} ${invoiceNr}`;
    }
    
    return title;
  });

  protected readonly headerDescription = computed(() => {
    const dateTime = this.receivedDateTime();
    let description: string | undefined;
    
    if (dateTime) {
      const formattedDate = this.datePipe.transform(dateTime, 'yyyy-MM-dd HH:mm:ss');
      description = `${this.translate.instant(
        'economy.supplier.invoice.receiveddatetime')} ${formattedDate}`;
    }

    return description;
  });

  constructor() {
    super();
    /*
    Here data is saving with Entity 0 and Type 53.
    */
    this.filesHelper = new FilesHelper(
      true,
      SoeEntityType.None,
      SoeDataStorageRecordType.OrderInvoiceFileAttachment,
      Feature.Economy_Supplier_Invoice,
      this.performLoadData
    );
  }

  // Life cycle hooks
  ngOnInit(): void {
    super.ngOnInit();
    this.startFlow(Feature.Economy_Supplier_Invoice, {
      additionalModifyPermissions: [Feature.Economy_Supplier_Invoice],
      lookups: [
        this.loaderService.load(),
        this.costAllocationLoaderService.load(),
        this.loadEdiEntry(),
      ],
    });
    this.idsSubscription();
    this.orderProjectSubscription();
    this.invoiceNrSubscription();
  }

  // Subscriptions
  private idsSubscription() {
    if (!this.form) return;

    this.idsSubscription$ = combineLatest({
      invoiceId: this.form.invoiceId.valueChanges,
      supplierId: this.form.actorId.valueChanges,
      ediEntryId: this.form.ediEntryId.valueChanges,
      scanningEntryId: this.form.scanningEntryId.valueChanges,
    })
      .pipe(takeUntilDestroyed(this.destroyRef), debounceTime(0))
      .subscribe(ids => {
        this.ids.set(ids);
        this.form?.idField.patchValue(constructIdField(ids));
      });
  }

  private invoiceNrSubscription(): void {
    if (!this.form) return;

    // Set initial value
    this.invoiceNrSignal.set(this.form.invoiceNr.value);

    // Subscribe to changes
    this.form.invoiceNr.valueChanges
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(value => {
        this.invoiceNrSignal.set(value);
      });
  }
  private orderProjectSubscription() {
    if (!this.form) return;
    this.form.orderCustomerInvoiceId.valueChanges
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(orderCustomerInvoiceId => {
        if (orderCustomerInvoiceId)
          this.changeOrderIdOrProjectId(orderCustomerInvoiceId, undefined);
      });
    this.form.projectId.valueChanges
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(projectId => {
        if (projectId) this.changeOrderIdOrProjectId(undefined, projectId);
      });
  }

  changeOrderIdOrProjectId(
    orderId: number | undefined,
    projectId: number | undefined
  ) {
    if (!this.isCostAllocationOpen() && !this.isCostAllocationDataLoaded()) {
      this.loadSupplierInvoiceCostAllocationRows().subscribe(() => {
        this.isCostAllocationDataLoaded.set(true);
      });
    }
    if (orderId) {
      this.updateProjectOrderInCostAllocationRows(orderId, undefined);
    } else if (projectId) {
      this.updateProjectOrderInCostAllocationRows(undefined, projectId);
    }
  }

  updateProjectOrderInCostAllocationRows(
    orderId: number | undefined,
    projectId: number | undefined
  ) {
    const order = this.projectOrderLoaderService.getOrder(orderId);
    const project = this.projectOrderLoaderService.getProject(projectId);
    this.addChargedToProjectIfRowsEmpty(order, project);
    this.updateCostAllocationRows(order, project);
    this.isCostAllocationDataLoaded.set(true);
    this.doCostAllocationReload();
    this.form?.setFormDirty();
  }

  doCostAllocationReload() {
    setTimeout(() => {
      if (this.costAllocationComponent()) {
        this.costAllocationComponent()?.updateGridRowsFromCostAllocationRows();
      }
    });
  }

  addChargedToProjectIfRowsEmpty(
    order: ICustomerInvoiceSmallGridDTO | undefined,
    project: IProjectTinyDTO | undefined
  ) {
    if (
      this.form?.supplierInvoiceCostAllocationRows?.value.filter(
        f => f.isConnectToProjectRow
      ).length == 0
    ) {
      if (order || project) {
        const row = new SupplierInvoiceCostAllocationDTO();
        row.isConnectToProjectRow = true;
        row.isTransferToOrderRow = false;
        row.includeSupplierInvoiceImage = true;
        this.projectOrderLoaderService.setOrderDetails(row, order);
        this.projectOrderLoaderService.setProjectDetails(row, project);
        row.projectAmountCurrency = this.form?.totalAmount?.value ?? 0.0;
        this.costAllocationLoaderService.setTimeCodeDetails(row);
        row.timeCodeTransactionId = -(
          (this.form?.supplierInvoiceCostAllocationRows?.value.length ?? 0) + 1
        );

        const rows: SupplierInvoiceCostAllocationDTO[] =
          this.form?.supplierInvoiceCostAllocationRows?.value || [];
        rows.push(row);
        this.patchSupplierInvoiceCostAllocationRows(rows);
        this.isCostAllocationDataLoaded.set(true);
      }
    }
  }
  updateCostAllocationRows(
    order: ICustomerInvoiceSmallGridDTO | undefined,
    project: IProjectTinyDTO | undefined
  ) {
    const allRows: SupplierInvoiceCostAllocationDTO[] =
      this.form?.supplierInvoiceCostAllocationRows?.value || [];

    const rows = allRows.map((r: SupplierInvoiceCostAllocationDTO) => {
      let row = {
        ...r,
      } as SupplierInvoiceCostAllocationDTO;
      if (order) {
        row.orderId = order.invoiceId;
        row.customerInvoiceNumberName =
          order.customerInvoiceNumberNameWithoutDescription;
        row.orderNr = order.invoiceNr;
      }
      if (project) {
        row.projectId = project.projectId;
        row.projectName = project.name;
        row.projectNr = project.number;
        row.projectNrName = `${project.number} ${project.name}`;
      }
      return row;
    });
    this.patchSupplierInvoiceCostAllocationRows(rows);
  }

  private patchSupplierInvoiceCostAllocationRows(
    rows: SupplierInvoiceCostAllocationDTO[]
  ) {
    this.form?.patchSupplierInvoiceCostAllocationRows(rows);
  }

  // Load data
  override loadData(): Observable<void> {
    const idValue = this.form?.idField.value ?? '';
    const ids = deconstructIdField(idValue);

    if (!ids.hasId) {
      this.form?.reset();
      this.headFieldClasses.set({});
      return of();
    }

    this.filesHelper.recordId.set(ids.invoiceId ?? 0);

    return this.performLoadData.load$(
      this.service
        .loadInvoiceByStrategy(
          ids.invoiceId,
          ids.scanningEntryId,
          ids.ediEntryId
        )
        .pipe(
          tap(([dto, classes]) => {
            this.headFieldClasses.set(classes ?? {});
            this.form?.reset(dto);
            this.invoiceIdField.set(idValue);
          })
        ),
      { showDialogDelay: 300 }
    );
  }

  private loadEdiEntry(): Observable<IEdiEntryDTO | null> {
    const idValue = this.form?.idField.value ?? '';
    const ids = deconstructIdField(idValue);
    if (!ids.ediEntryId) return of(null);
    return this.service.getEdiEntry(ids.ediEntryId, false).pipe(
      tap(ediEntry => {
        this.ediEntry.set(ediEntry);
      })
    );
  }

  loadSupplierInvoiceCostAllocationRows() {
    if (this.isCostAllocationDataLoaded()) return of();
    if (!this.form?.invoiceId?.value) return of();
    return this.performLoadData.load$(
      this.service
        .getSupplierInvoiceCostAllocationRows(this.form?.invoiceId.value)
        .pipe(
          tap(rows => {
            this.patchSupplierInvoiceCostAllocationRows(rows);
          })
        )
    );
  }

  accountingRowsReturned(accountingRows: AccountingRowDTO[]): void {
    if (accountingRows.length === 0) return;
    const accountingRowParam: CreateAccountingRowParams[] = accountingRows.map(
      row => {
        return {
          type: SupplierAccountType.Debit,
          amount: 0,
          amountBaseCurrency: 0,
          isDebitRow: true,
          isVatRow: false,
          isContractorVatRow: false,

          dim1Id: row.dim1Id,
          dim2Id: row.dim2Id,
          dim3Id: row.dim3Id,
          dim4Id: row.dim4Id,
          dim5Id: row.dim5Id,
          dim6Id: row.dim6Id,
        };
      }
    );
    this.generateAccountingRows(accountingRowParam);
  }

  toggleLocked() {
    this.isLocked.set(!this.isLocked());
  }

  generateAccountingRows(
    applyCostRows: CreateAccountingRowParams[] | null = null
  ) {
    if (!this.form) return;

    const accountingRowParams: GenerateAccountingRowsInput = {
      amountBaseCurrency: this.form.totalAmount.value ?? 0,
      vatAmountBaseCurrency: this.form.vatAmount.value ?? 0,
      amountTransactionCurrency: this.form.totalAmountCurrency.value ?? 0,
      vatAmountTransactionCurrency: this.form.vatAmountCurrency.value ?? 0,
      costRows: applyCostRows,
    };
    const accountingRowContext: CreateAccountingRowContext = {
      accountingRows: [],
      accountingSettings: this.supplier()?.accountingSettings ?? [],
      isInterimInvoice: false,
      vatType: Number(this.form.vatType.value),
      vatRate: this.vatService.vatRate(),
      voucherDate: this.form.voucherDate.value,
      isCreditInvoice: this.form.totalAmount.value < 0,
      vatCodeVatAccountId: this.vatService.purchaseVATAccountId(),
    };

    const rows = this.accountingRowsService.generateAccountingRows(
      accountingRowParams,
      accountingRowContext
    );

    this.form.patchAccountingRows(rows as AccountingRowDTO[]);

    if (this.vatService.shouldShowVatAsZero(this.form.vatType.value)) {
      // Override VAT amounts to ZERO for the invoice while keeping it for accounting rows
      this.form.vatAmount.setValue(0, { emitEvent: false });
      this.form.vatAmountCurrency.setValue(0, { emitEvent: false });
    }
  }

  costAllocationOpened(opened: boolean) {
    this.isCostAllocationOpen.set(opened);
  }

  fileListOpened(opened: boolean) {
    this.isFileDisplayAccordionOpen.set(opened);
    this.loadFileList();
  }

  loadFileList() {
    if (this.isFileDisplayAccordionOpen()) {
      this.performLoadData
        .load$(this.filesHelper.loadFiles(true, true))
        .subscribe();
    }
  }

  toggleTracingOpened(isOpen: boolean) {
    this.traceRowsRendered.set(isOpen);
  }

  toggleProductRowsOpened(isOpen: boolean) {
    this.productRowsRendered.set(isOpen);
  }
}

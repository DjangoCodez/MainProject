// Angular Core
import {
  Component,
  computed,
  DestroyRef,
  inject,
  input,
  OnInit,
  output,
  signal,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

// RxJS
import {
  auditTime,
  debounceTime,
  distinctUntilChanged,
  filter,
  forkJoin,
  Observable,
  of,
  startWith,
  tap,
  withLatestFrom,
  mergeMap,
  Subject,
} from 'rxjs';

// Translation
import { TranslateService } from '@ngx-translate/core';

// Shared Services
import { ProgressService } from '@shared/services/progress/progress.service';
import { CurrencyService } from '@shared/services/currency.service';
import { DateUtil } from '@shared/util/date-util';
import { Perform } from '@shared/util/perform.class';
import { ValidationHandler } from '@shared/handlers';

// UI Components
import { DialogService } from '@ui/dialog/services/dialog.service';
import { EditComponentDialogData } from '@ui/dialog/edit-component-dialog/edit-component-dialog.component';
import { MessageboxService } from '@ui/dialog/services/messagebox.service';

// Models and DTOs
import {
  IPaymentInformationViewDTO,
  ISupplierDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import {
  TermGroup_CurrencyType,
  TermGroup_InvoiceVatType,
} from '@shared/models/generated-interfaces/Enumerations';
import { SupplierInvoiceForm } from '../../models/supplier-invoice-form.model';
import { InvoiceFieldClasses } from '../../models/utility-models';
import { SupplierHeadForm } from '@features/economy/suppliers/models/supplier-head-form.model';
import { SupplierDTO } from '@features/economy/suppliers/models/supplier.model';
import { SupplierEditInputParameters } from '@features/economy/suppliers/models/edit-parameters.model';

// Services
import { SupplierInvoiceService } from '../../services/supplier-invoice.service';
import { SupplierInvoiceLoaderService } from '../../services/supplier-invoice-loader.service';
import { SupplierInvoiceFeatureService } from '../../services/supplier-invoice-feature.service';
import { SupplierInvoiceSettingsService } from '../../services/supplier-invoice-settings.service';
import { SupplierService } from '@features/economy/services/supplier.service';

// Domain Services
import { InvoicePaymentConditionService } from '../../domain-services/payment-condition.service';
import { InvoiceVatService } from '../../domain-services/vat.service';
import { InvoiceAccountingRowsService } from '../../domain-services/accounting-rows.service';

// Components
import { SuppliersEditComponent } from '@features/economy/suppliers/components/suppliers-edit/suppliers-edit.component';
import { SupplierInvoiceProjectOrderLoaderService } from '../../services/supplier-invoice-projectorder-loader.service';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';

/***
 * Component for editing supplier invoice header information.
 *
 * Handles complex form interactions including:
 * - Supplier selection and payment information
 * - Currency and VAT calculations
 * - Date and payment condition management
 * - Form validation and field dependencies
 *
 * Known todos:
 *  - Fix supplier typeahead keyboard navigation
 *  - Review payment information logic
 *  - Implement comprehensive currency handling UI
 *  - Add amount change notification flow to parent
 */

@Component({
  selector: 'soe-supplier-invoice-edit-head',
  templateUrl: './supplier-invoice-edit-head.component.html',
  styleUrls: ['./supplier-invoice-edit-head.component.scss'],
  standalone: false,
})
export class SupplierInvoiceEditHeadComponent implements OnInit {
  // Utility Services
  private readonly progressService = inject(ProgressService);
  private readonly dialogService = inject(DialogService);
  private readonly validationHandler = inject(ValidationHandler);
  private readonly messageboxService = inject(MessageboxService);
  private readonly translate = inject(TranslateService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly performLoadData = new Perform<any>(this.progressService);

  // API Services
  private readonly service = inject(SupplierInvoiceService);
  readonly invoiceLoader = inject(SupplierInvoiceLoaderService);
  readonly projectOrderLoader = inject(
    SupplierInvoiceProjectOrderLoaderService
  );
  readonly featureService = inject(SupplierInvoiceFeatureService);
  private readonly settingService = inject(SupplierInvoiceSettingsService);

  // Domain Services
  private readonly paymentConditionService = inject(
    InvoicePaymentConditionService
  );
  private readonly vatService = inject(InvoiceVatService);
  readonly currencyService = inject(CurrencyService);
  readonly accountingRowsService = inject(InvoiceAccountingRowsService);

  // Debounced accounting rows update
  private readonly updateAccountingRowsSubject = new Subject<void>();

  form = input.required<SupplierInvoiceForm>();
  fieldClasses = input<InvoiceFieldClasses>({});
  isLocked = input(true);
  invoiceIdsText = input<string>('');

  supplierChanged = output<ISupplierDTO | null>();
  generateAccountingRows = output<void>();

  // State

  supplier = signal<ISupplierDTO | null>(null);
  supplierId = computed(() => this.supplier()?.actorSupplierId ?? null);
  paymentInformationRows = signal<IPaymentInformationViewDTO[]>([]);
  selectedPaymentInformationRow = signal<IPaymentInformationViewDTO | null>(
    null
  );

  /**
   * Computes the suggested payment information row based on priority rules:
   * 1. Default account within default payment type
   * 2. Any default account
   * 3. First account of default payment type
   * 4. Fallback: first in list
   */
  suggestedPaymentInformationRow = computed(() => {
    const paymentInfos = this.paymentInformationRows();

    if (!paymentInfos || paymentInfos.length === 0) {
      return null;
    }

    const defaultInType = paymentInfos.find(
      info =>
        info.sysPaymentTypeId === info.defaultSysPaymentTypeId &&
        info.default === true
    );
    if (defaultInType) return defaultInType;

    const anyDefault = paymentInfos.find(info => info.default === true);
    if (anyDefault) return anyDefault;

    const firstDefaultType = paymentInfos.find(
      info => info.sysPaymentTypeId === info.defaultSysPaymentTypeId
    );
    if (firstDefaultType) return firstDefaultType;

    return paymentInfos[0];
  });

  // Constants
  private static readonly DEBOUNCE_TIME_MS = 150;
  private static readonly UNIQUENESS_CHECK_DEBOUNCE_MS = 500;
  private static readonly AUDIT_TIME_MS = 0;

  /**
   * Form field dependency flows:
   *
   * 1. Supplier changes → Update currency, VAT type, VAT code, payment information
   * 2. Invoice date changes → Update currency rates and due dates
   * 3. VAT type changes → Update VAT amount calculations
   * 4. VAT code changes → Update VAT amount calculations
   * 5. Currency changes → Update rates, amounts, and accounting rows
   * 6. Total amount changes → Update VAT amounts and accounting rows
   * 7. VAT amount changes → Update accounting rows
   * 8. Invoice number changes → Auto-generate OCR and validate uniqueness
   * 9. When amounts or supplier change → Regenerate accounting rows
   */

  // Listeners
  onSupplierChanged(
    supplierId: number | null
  ): Observable<[ISupplierDTO, IPaymentInformationViewDTO[]] | null> {
    if (supplierId === this.supplier()?.actorSupplierId) {
      return of([this.supplier()!, this.paymentInformationRows()]);
    }

    if (!supplierId) {
      this.supplier.set(null);
      this.supplierChanged.emit(null);
      this.paymentInformationRows.set([]);
      return of(null);
    }

    return this.loadSupplierAndPaymentInformation(supplierId);
  }

  onCurrencyDependentChanged(currencyId?: number, invoiceDate?: Date) {
    this.currencyService.setCurrencyIdAndDate(
      currencyId ?? 0,
      invoiceDate ?? new Date()
    );
  }

  editSupplier() {
    const dialogData: EditComponentDialogData<
      SupplierDTO,
      SupplierService,
      SupplierHeadForm,
      SupplierEditInputParameters
    > = {
      title: this.translate.instant('economy.supplier.supplier.supplier'),
      size: 'lg',
      hasBackdrop: false,
      hideFooter: true,
      noToolbar: true,
      maxHeight: '80vh',
      form: new SupplierHeadForm({
        validationHandler: this.validationHandler,
        element: {
          actorSupplierId: this.supplierId(),
        } as ISupplierDTO,
      }),
      editComponent: SuppliersEditComponent,
      parameters: new SupplierEditInputParameters(),
    };

    this.dialogService
      .openEditComponent(dialogData)
      .afterClosed()
      .subscribe(
        (result: { response: BackendResponse; value: SupplierDTO }) => {
          if (!result) return;
          const { response, value } = result;
          this.onEditSupplier(response, value);
        }
      );
  }

  onEditSupplier(result: BackendResponse, supplier: SupplierDTO) {
    if (!result) return;

    if (result.success && supplier) {
      const supplierId = supplier.actorSupplierId;
      this.invoiceLoader.upsertSupplier({
        id: supplierId,
        name: `${supplier.supplierNr} ${supplier.name}`,
      });

      if (supplierId === this.supplierId()) {
        this.loadSupplierAndPaymentInformation(supplierId).subscribe();
      } else {
        this.form().actorId.patchValue(supplier.actorSupplierId);
      }
    }
  }

  // Loaders

  loadSupplierAndPaymentInformation(
    supplierId: number
  ): Observable<[ISupplierDTO, IPaymentInformationViewDTO[]]> {
    return this.performLoadData.load$(
      forkJoin([
        this.loadSupplier(supplierId),
        this.loadPaymentInformation(supplierId),
      ])
    );
  }

  loadSupplier(supplierId: number) {
    return this.service.loadSupplier(supplierId).pipe(
      tap(supplier => {
        this.supplier.set(supplier);
        this.supplierChanged.emit(supplier);
      })
    );
  }

  loadPaymentInformation(supplierId: number) {
    return this.service.loadPaymentInformation(supplierId).pipe(
      tap(paymentInfo => {
        this.paymentInformationRows.set(paymentInfo);
      })
    );
  }

  // Actions
  getFieldClasses(field: string) {
    if (this.isLocked()) return '';

    return this.fieldClasses()[field];
  }

  ngOnInit(): void {
    this.setupCurrencyServiceSubscription();
    this.setupFormSubscriptions();
    this.setupDebouncedAccountingRowsUpdate();
    this.setupLoaderServicesSubscription();
  }

  private setupLoaderServicesSubscription(): void {
    this.projectOrderLoader.load().subscribe();
  }

  /**
   * Sets up debounced accounting rows update
   */
  private setupDebouncedAccountingRowsUpdate(): void {
    this.updateAccountingRowsSubject
      .pipe(
        debounceTime(SupplierInvoiceEditHeadComponent.DEBOUNCE_TIME_MS),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe(() => {
        this.regenerateAccountingRows();
      });
  }

  /**
   * Triggers a debounced update of accounting rows
   */
  private triggerAccountingRowsUpdate(): void {
    this.updateAccountingRowsSubject.next();
  }

  /**
   * Sets up subscription to currency service changes
   */
  private setupCurrencyServiceSubscription(): void {
    this.currencyService.currencyIdChanged$.subscribe(currencyId => {
      this.form().patchValue({
        currencyId: currencyId,
        currencyDate: this.currencyService.currencyRateDate,
        currencyRate: this.currencyService.transactionCurrencyRate,
      });
    });
  }

  /**
   * Sets up all form field subscriptions and their dependencies
   */
  private setupFormSubscriptions(): void {
    this.setupInvoiceIdsChangeSubscription();
    this.setupSupplierChangeSubscription();
    this.setupInvoiceDateChangeSubscription();
    this.setupVatTypeChangeSubscription();
    this.setupVatCodeChangeSubscription();
    this.setupCurrencyChangeSubscription();
    this.setupTotalAmountChangeSubscription();
    this.setupVatAmountChangeSubscription();
    this.setupInvoiceNumberChangeSubscription();
    this.setupVatTypeValidationSubscription();
    this.setupPaymentNumberChangeSubscription();
    this.setupPaymentInformationRowChangeSubscription();
    this.setupInvoiceNumberUniquenessValidation();
  }

  /**
   * Initializes default values for a new invoice based on business rules
   * Order optimized to minimize unnecessary recalculations
   */
  private initializeNewInvoiceDefaults(): void {
    const form = this.form();
    if (form.invoiceId.value) {
      return; // Only initialize for new invoices
    }

    // 1. Handle supplier change first (sets currency, VAT type, VAT code, payment info)
    if (form.actorId.value) {
      this.handleSupplierChange();
    }

    // 2. Initialize dates (affects currency rates and due dates)
    const invoiceDate = form.invoiceDate.value;
    if (invoiceDate) {
      this.recalcDatesFromInvoiceDate(invoiceDate);
    }

    // 3. Set up currency and FX (affects all amount calculations)
    const currencyId = form.currencyId.value;
    if (currencyId && invoiceDate) {
      this.refreshFxAndAmounts(currencyId, invoiceDate);
    }

    // 4. Handle VAT type validation (may disable/enable fields)
    const vatType = form.vatType.value;
    if (vatType) {
      this.handleVatTypeValidation(vatType);
    }

    // 5. Calculate VAT amounts (depends on totals and VAT settings)
    this.recalcVatFromTotals();

    // 6. Handle payment information (independent of amounts)
    const paymentNr = form.paymentNr.value;
    if (paymentNr) {
      this.paymentNrChanged(paymentNr);
    }

    const paymentRowId = form.paymentInformationRowId.value;
    if (paymentRowId) {
      this.paymentRowIdChanged(paymentRowId);
    }

    // 7. Handle invoice number (may auto-generate OCR)
    const invoiceNr = form.invoiceNr.value;
    if (invoiceNr) {
      this.handleInvoiceNumberChange(invoiceNr);
      this.checkIfInvoiceNrIsUnique(invoiceNr);
    }

    // 8. Update accounting rows last (depends on all previous calculations)
    this.triggerAccountingRowsUpdate();
  }

  private setupInvoiceIdsChangeSubscription(): void {
    this.form()
      .idField.valueChanges.pipe(
        distinctUntilChanged(),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe(() => this.initializeNewInvoiceDefaults());
  }

  /**
   * EFFECT 1: Supplier changes → Update currency, VAT type, VAT code, payment information
   */
  private setupSupplierChangeSubscription(): void {
    this.form()
      .actorId.valueChanges.pipe(
        distinctUntilChanged(),
        filter(id => !!id),
        mergeMap((id: number) => this.onSupplierChanged(id)),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe(() => this.handleSupplierChange());
  }

  /**
   * EFFECT 2: Invoice date changes → Update currency and due dates
   */
  private setupInvoiceDateChangeSubscription(): void {
    this.form()
      .invoiceDate.valueChanges.pipe(
        distinctUntilChanged(DateUtil.dateEquals),
        filter((d): d is Date => !!d),
        withLatestFrom(
          this.form().currencyId.valueChanges.pipe(
            startWith(this.form().currencyId.value)
          )
        ),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe(([date, ccy]) => {
        this.recalcDatesFromInvoiceDate(date);
        this.refreshFxAndAmounts(ccy, date);
      });
  }

  /**
   * EFFECT 3: VAT type changes → Update VAT amount calculations
   */
  private setupVatTypeChangeSubscription(): void {
    this.form()
      .vatType.valueChanges.pipe(
        startWith(this.form().vatType.value),
        distinctUntilChanged(),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe(() => {
        this.recalcVatFromTotals();
        this.triggerAccountingRowsUpdate();
      });
  }

  /**
   * EFFECT 4: VAT code changes → Update VAT amount calculations
   */
  private setupVatCodeChangeSubscription(): void {
    this.form()
      .vatCodeId.valueChanges.pipe(
        startWith(this.form().vatCodeId.value),
        distinctUntilChanged(),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe(() => {
        this.recalcVatFromTotals();
        this.triggerAccountingRowsUpdate();
      });
  }

  /**
   * EFFECT 5: Currency changes → Update currency rate, amounts, and accounting rows
   */
  private setupCurrencyChangeSubscription(): void {
    this.form()
      .currencyId.valueChanges.pipe(
        startWith(this.form().currencyId.value),
        distinctUntilChanged(),
        withLatestFrom(
          this.form().invoiceDate.valueChanges.pipe(
            startWith(this.form().invoiceDate.value)
          )
        ),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe(([ccy, date]) => {
        if (!ccy) return;
        this.refreshFxAndAmounts(ccy, date ?? undefined);
      });
  }

  /**
   * EFFECT 6: Total amount changes → Update VAT amounts and accounting rows
   */
  private setupTotalAmountChangeSubscription(): void {
    this.form()
      .totalAmountCurrency.valueChanges.pipe(
        startWith(this.form().totalAmountCurrency.value),
        distinctUntilChanged(),
        auditTime(SupplierInvoiceEditHeadComponent.AUDIT_TIME_MS),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe(() => {
        this.recalcBaseTotalsFromCurrencyTotals();
        this.recalcVatFromTotals();
        this.triggerAccountingRowsUpdate();
      });
  }

  /**
   * EFFECT 7: VAT amount changes → Update accounting rows
   */
  private setupVatAmountChangeSubscription(): void {
    this.form()
      .vatAmountCurrency.valueChanges.pipe(
        startWith(this.form().vatAmountCurrency.value),
        distinctUntilChanged(),
        auditTime(SupplierInvoiceEditHeadComponent.AUDIT_TIME_MS),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe(() => {
        this.recalcBaseVatFromCurrencyVat();
        this.triggerAccountingRowsUpdate();
      });
  }

  /**
   * EFFECT 8: Invoice number changes → Auto-generate OCR and validate uniqueness
   */
  private setupInvoiceNumberChangeSubscription(): void {
    this.form()
      .invoiceNr.valueChanges.pipe(
        startWith(this.form().invoiceNr.value),
        distinctUntilChanged(),
        debounceTime(SupplierInvoiceEditHeadComponent.DEBOUNCE_TIME_MS),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe(nr => {
        this.handleInvoiceNumberChange(nr);
      });
  }

  /**
   * Sets up VAT type validation and field enabling/disabling
   */
  private setupVatTypeValidationSubscription(): void {
    this.form()
      .vatType.valueChanges.pipe(
        startWith(this.form().vatType.value),
        distinctUntilChanged(),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe(vatType => {
        this.handleVatTypeValidation(vatType);
      });
  }

  /**
   * Sets up payment number change subscription
   */
  private setupPaymentNumberChangeSubscription(): void {
    this.form()
      .paymentNr.valueChanges.pipe(
        startWith(this.form().paymentNr.value),
        distinctUntilChanged(),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe(paymentNr => {
        this.paymentNrChanged(paymentNr);
      });
  }

  /**
   * Sets up payment information row change subscription
   */
  private setupPaymentInformationRowChangeSubscription(): void {
    this.form()
      .paymentInformationRowId.valueChanges.pipe(
        startWith(this.form().paymentInformationRowId.value),
        distinctUntilChanged(),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe(paymentInformationRowId => {
        this.paymentRowIdChanged(paymentInformationRowId);
      });
  }

  /**
   * Sets up invoice number uniqueness validation
   */
  private setupInvoiceNumberUniquenessValidation(): void {
    this.form()
      .invoiceNr.valueChanges.pipe(
        debounceTime(
          SupplierInvoiceEditHeadComponent.UNIQUENESS_CHECK_DEBOUNCE_MS
        ),
        distinctUntilChanged(),
        filter((nr): nr is string => !!nr),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe(nr => {
        this.checkIfInvoiceNrIsUnique(nr);
      });
  }

  // --- Event Handlers --------------------------------------------------------

  /**
   * Handles supplier change and updates related form fields
   */
  private handleSupplierChange(): void {
    const supplier = this.supplier();
    if (!supplier) return;

    const form = this.form();

    // Update form fields based on supplier data
    form.maybeSet(
      form.currencyId,
      supplier.currencyId || this.currencyService.baseCurrencyId
    );
    form.maybeSet(
      form.vatType,
      supplier.vatType || this.settingService.defaultVatType
    );
    form.maybeSet(
      form.vatCodeId,
      supplier.vatCodeId || this.settingService.accountingDefaultVatCodeId
    );

    // Update payment information
    this.selectedPaymentInformationRow.set(
      this.suggestedPaymentInformationRow()
    );

    form.maybeSet(
      form.paymentInformationRowId,
      this.selectedPaymentInformationRow()?.paymentInformationRowId ?? null
    );

    // Update payment condition
    const paymentCondition = this.invoiceLoader
      .paymentConditions()
      .find(
        pc =>
          pc.paymentConditionId ===
          (supplier.paymentConditionId ??
            this.settingService.defaultPaymentConditionId)
      );

    this.paymentConditionService.changePaymentCondition(paymentCondition);
    this.updateTimeDiscount();
  }

  /**
   * Handles invoice number change and auto-generates OCR if needed
   */
  private handleInvoiceNumberChange(invoiceNumber?: string): void {
    const form = this.form();
    if (
      !form.ocr.value &&
      invoiceNumber &&
      this.supplier()?.copyInvoiceNrToOcr
    ) {
      form.maybeSet(form.ocr, invoiceNumber);
    }
  }

  /**
   * Handles VAT type validation and field enabling/disabling
   */
  private handleVatTypeValidation(vatType?: TermGroup_InvoiceVatType): void {
    const form = this.form();
    if (vatType && this.vatService.isVatLocked(vatType)) {
      form.vatAmountCurrency.disable();
    } else {
      form.vatAmountCurrency.enable();
    }

    if (vatType && this.vatService.isVatExempt(vatType)) {
      form.maybeSet(form.vatCodeId, null);
    }
  }

  // --- Currency and FX Calculations -----------------------------------------

  /**
   * Refreshes foreign exchange data and recomputes base amounts from currency amounts
   */
  private refreshFxAndAmounts(currencyId: number, asOf?: Date): void {
    const form = this.form();
    const date = asOf ?? form.voucherDate.value ?? new Date();

    // Update currency service with new values
    this.currencyService.setCurrencyIdAndDate(currencyId, date);

    // Update form with new currency rate and date

    form.maybeSet(
      form.controls.currencyRate,
      this.currencyService.transactionCurrencyRate
    );
    form.maybeSet(
      form.controls.currencyDate,
      this.currencyService.currencyRateDate
    );

    // Recompute all amounts based on new currency rate
    this.recalcBaseTotalsFromCurrencyTotals();
    this.recalcBaseVatFromCurrencyVat();
    this.triggerAccountingRowsUpdate();
  }

  /**
   * Recomputes total amount (base currency) from total amount (transaction currency) and exchange rate
   */
  private recalcBaseTotalsFromCurrencyTotals(): void {
    const form = this.form();
    const totalCurrency = form.totalAmountCurrency.value ?? 0;
    const totalBase = this.currencyService.getCurrencyAmount(
      totalCurrency,
      TermGroup_CurrencyType.TransactionCurrency,
      TermGroup_CurrencyType.BaseCurrency
    );
    form.maybeSet(form.totalAmount, totalBase);
  }

  /**
   * Recomputes VAT amount (base currency) from VAT amount (transaction currency) and exchange rate
   */
  private recalcBaseVatFromCurrencyVat(): void {
    const form = this.form();
    const vatCurrency = form.vatAmountCurrency.value ?? 0;
    const vatBase = this.currencyService.getCurrencyAmount(
      vatCurrency,
      TermGroup_CurrencyType.TransactionCurrency,
      TermGroup_CurrencyType.BaseCurrency
    );
    form.maybeSet(form.controls.vatAmount, vatBase);
  }

  // --- VAT Calculations ------------------------------------------------------

  /**
   * Recomputes VAT amounts (both currency and base) from totals and VAT settings
   */
  private recalcVatFromTotals(): void {
    const form = this.form();
    const vatCodeId = Number(form.vatCodeId.value);

    // Update VAT service if code has changed
    if (this.vatService.vatCodeId() !== vatCodeId) {
      const vatCode = this.invoiceLoader
        .vatCodes()
        .find(v => v.vatCodeId === vatCodeId);
      if (vatCode?.vatCodeId) this.vatService.setVatCode(vatCode);
      else this.vatService.setVatCode(null);
    }

    const totalCurrency = form.totalAmountCurrency.value ?? 0;
    const vatType = Number(form.vatType.value);
    const vatAmountCurrency = this.vatService.calculateInvoiceVat(
      totalCurrency,
      vatType
    );

    // Update VAT amount in transaction currency
    form.maybeSet(form.vatAmountCurrency, vatAmountCurrency);

    // Convert to base currency
    const vatAmountBase = this.currencyService.getCurrencyAmount(
      vatAmountCurrency,
      TermGroup_CurrencyType.TransactionCurrency,
      TermGroup_CurrencyType.BaseCurrency
    );
    form.maybeSet(form.vatAmount, vatAmountBase);
  }

  // --- Date Calculations -----------------------------------------------------

  /**
   * Recalculates due date and voucher date based on invoice date
   */
  private recalcDatesFromInvoiceDate(invoiceDate: Date): void {
    const form = this.form();
    const dueDate = this.paymentConditionService.getDueDate(invoiceDate);
    form.maybeSet(form.dueDate, dueDate);
    form.maybeSet(form.voucherDate, invoiceDate);
  }

  /**
   * Updates time discount information based on payment conditions and invoice date
   */
  private updateTimeDiscount(): void {
    const form = this.form();

    // Clear time discount if not applicable
    if (
      !this.settingService.usesTimeDiscount ||
      !this.paymentConditionService.paymentCondition()
    ) {
      form.patchSilently({
        timeDiscountPercent: null,
        timeDiscountDate: null,
      });
      return;
    }

    // Calculate and set time discount
    const timeDiscount = this.paymentConditionService.getTimeDiscount(
      form.invoiceDate.value
    );
    form.maybeSet(form.timeDiscountPercent, timeDiscount.percent);
    form.maybeSet(form.timeDiscountDate, timeDiscount.date);
  }

  // --- Payment Information Handling -------------------------------------------

  /**
   * Handles payment number change and updates payment information row ID
   */
  paymentNrChanged(paymentNr?: string): void {
    const form = this.form();
    const row = this.paymentInformationRows().find(
      item => item.paymentNr === paymentNr
    );

    form.maybeSet(
      form.paymentInformationRowId,
      row?.paymentInformationRowId ?? null
    );
  }

  /**
   * Handles payment information row ID change and updates payment number
   */
  paymentRowIdChanged(paymentInformationRowId?: number): void {
    const form = this.form();
    const row = this.paymentInformationRows().find(
      item => item.paymentInformationRowId === paymentInformationRowId
    );

    form.patchSilently({
      paymentNumber: row?.paymentNr ?? null,
    });
    this.selectedPaymentInformationRow.set(row ?? null);
  }

  // --- Validation Methods -----------------------------------------------------

  /**
   * Checks if the invoice number is unique for the current supplier
   */
  checkIfInvoiceNrIsUnique(invoiceNr: string): void {
    const form = this.form();
    this.service
      .invoiceNrIsUnique(form.actorId.value, invoiceNr, form.invoiceId.value)
      .pipe(
        tap(result => {
          if (result.success) {
            this.messageboxService.warning(
              this.translate.instant('core.warning'),
              this.translate.instant(
                'economy.supplier.invoice.invoicenumberalreadyexist'
              )
            );
          }
        })
      );
  }

  // --- Accounting and Data Updates -------------------------------------------

  /**
   * Updates accounting rows based on current form state
   * Note: Accounting rows service integration is pending implementation
   */
  private regenerateAccountingRows(): void {
    this.generateAccountingRows.emit();
  }
}

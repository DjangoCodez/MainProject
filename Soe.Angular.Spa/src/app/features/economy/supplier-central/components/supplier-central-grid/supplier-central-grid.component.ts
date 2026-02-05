import { Component, inject, OnDestroy, OnInit, signal } from '@angular/core';
import { SupplierService } from '@features/economy/services/supplier.service';
import { SuppliersEditComponent } from '@features/economy/suppliers/components/suppliers-edit/suppliers-edit.component';
import { SupplierHeadForm } from '@features/economy/suppliers/models/supplier-head-form.model';
import { SupplierDTO } from '@features/economy/suppliers/models/supplier.model';
import { SelectSupplierModalComponent } from '@shared/components/select-supplier-modal/select-supplier-modal.component';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { TermCollection } from '@shared/localization/term-types';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { ChangeViewStatusGridViewBalanceDTO } from '@shared/models/change-status-grid-view-balance-dto.model';
import {
  Feature,
  SettingMainType,
  SoeOriginStatusClassification,
  SoeOriginStatusClassificationGroup,
  TermGroup,
  UserSettingType,
} from '@shared/models/generated-interfaces/Enumerations';
import { ICompCurrencySmallDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { ISupplierInvoiceGridDTO } from '@shared/models/generated-interfaces/SupplierInvoiceDTOs';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ProgressService } from '@shared/services/progress/progress.service';
import { BrowserUtil } from '@shared/util/browser-util';
import { IconUtil } from '@shared/util/icon-util';
import { Perform } from '@shared/util/perform.class';
import { SettingsUtil } from '@shared/util/settings-util';
import { DialogData } from '@ui/dialog/models/dialog';
import { DialogService } from '@ui/dialog/services/dialog.service';
import { GridComponent } from '@ui/grid/grid.component';
import { MessageboxService } from '@ui/dialog/services/messagebox.service';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import {
  concatMap,
  distinctUntilChanged,
  forkJoin,
  Observable,
  of,
  Subject,
  takeUntil,
  tap,
} from 'rxjs';
import { ExtSupplierDTO } from '../../models/extended-supplier-dto.model';
import { SupplierCentralGridForm } from '../../models/supplier-central-grid-from.model';
import { SupplierCentralService } from '../../services/supplier-central.service';
import { SupplierCentralUrlParamsService } from '../../services/supplier-central-params.service';

@Component({
  selector: 'soe-supplier-central-grid',
  templateUrl: './supplier-central-grid.component.html',
  styleUrl: './supplier-central-grid.component.scss',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class SupplierCentralGridComponent
  extends GridBaseDirective<
    ISupplierInvoiceGridDTO | any,
    SupplierCentralService
  >
  implements OnInit, OnDestroy
{
  service = inject(SupplierCentralService);
  coreService = inject(CoreService);
  dialogService = inject(DialogService);
  progressService = inject(ProgressService);
  supplierService = inject(SupplierService);
  messageBoxService = inject(MessageboxService);
  urlService = inject(SupplierCentralUrlParamsService);

  unsubscribe = new Subject<void>();
  private supplier!: ExtSupplierDTO;
  //private suplierIdParameter: number = 0;

  // Terms
  terms!: TermCollection;

  // Permissions
  supplierSupplierPermission = false;
  supplierSupplierSuppliersEditPermission = false;
  supplierInvoicePermission = false;
  supplierInvoiceInvoiceInvoicesEditPermission = false;
  supplierInvoiceStatusForeignPermission = false;
  supplierInvoiceStatusAttestFlowPermission = false;

  // Data
  supplierNumber!: string;
  supplierName: string = '';
  supplierAddress!: string;
  supplierPhone!: string;

  hasCurrencyPermission: boolean = false;
  allItemsSelectionDict!: SmallGenericType[];
  invoiceBillingTypes!: SmallGenericType[];
  originStatus!: SmallGenericType[];
  setupComplete = false;

  currencies!: ICompCurrencySmallDTO[];
  yesNoDict!: { yes: string; no: string };

  supplierCentralGridForm: SupplierCentralGridForm =
    new SupplierCentralGridForm(this.translate, this.messageBoxService);

  supplierCentralCountersAndBalances: Subject<
    ChangeViewStatusGridViewBalanceDTO[]
  > = new Subject<ChangeViewStatusGridViewBalanceDTO[]>();
  supplierInput: Subject<ExtSupplierDTO> = new Subject<ExtSupplierDTO>();
  supplierInvoiceStatusForeignPermissionInput: Subject<boolean> =
    new Subject<boolean>();

  performGridLoad = new Perform<ISupplierInvoiceGridDTO[]>(
    this.progressService
  );

  ngOnInit(): void {
    super.ngOnInit();

    this.startFlow(Feature.Economy_Supplier_Suppliers, 'invoiceGrid', {
      lookups: [this.doLookups()],
      useLegacyToolbar: true,
    });

    this.supplierCentralGridForm.allItemsSelection.valueChanges
      .pipe(
        distinctUntilChanged((prev, current) => prev == current),
        takeUntil(this.unsubscribe),
        tap(item => {
          if (item) this.updateItemsSelection(item);
        })
      )
      .subscribe();

    this.supplierCentralGridForm.loadOpen.valueChanges
      .pipe(
        takeUntil(this.unsubscribe),
        tap(loadOpen => {
          if (loadOpen != null) this.refreshGrid();
        })
      )
      .subscribe();

    this.supplierCentralGridForm.loadClosed.valueChanges
      .pipe(
        takeUntil(this.unsubscribe),
        tap(loadClosed => {
          if (loadClosed != null) this.refreshGrid();
        })
      )
      .subscribe();
  }

  override loadData(
    id?: number | undefined
  ): Observable<ISupplierInvoiceGridDTO[]> {
    const allItemSelection =
      this.supplierCentralGridForm.allItemsSelection.getRawValue();
    const loadClosed = this.supplierCentralGridForm.loadClosed.getRawValue();
    const loadOpen = this.supplierCentralGridForm.loadOpen.getRawValue();
    if (!this.urlService.supplierId() || !allItemSelection) return of([]);
    return this.service.getGrid(this.urlService.supplierId(), {
      loadOpen: loadOpen,
      loadClosed: loadClosed,
      onlyMine: false,
      allItemsSelection: allItemSelection,
    });
  }

  private doLookups(): Observable<
    [
      Record<number, boolean>,
      SmallGenericType[],
      SmallGenericType[],
      SmallGenericType[],
      ICompCurrencySmallDTO[],
    ]
  > {
    return forkJoin([
      this.loadModifyPermissions(),
      this.loadOriginStatus(),
      this.loadSelectionTypes(),
      this.loadInvoiceBillingTypes(),
      this.loadCurrencies(),
    ]);
  }

  override onFinished(): void {
    this.startAction();
  }

  private startAction() {
    if (this.urlService.supplierId()) this.showSupplier();
    else this.showSelectSupplier();
  }

  override loadTerms(): Observable<TermCollection> {
    return super.loadTerms([
      'economy.supplier.supplier.supplier',
      'economy.supplier.invoice.new',
      'core.yes',
      'core.no',
      'common.type',
      'economy.supplier.invoice.seqnr',
      'economy.supplier.invoice.invoicenr',
      'economy.supplier.invoice.invoicetype',
      'common.tracerows.status',
      'economy.supplier.supplier.supplier',
      'economy.supplier.invoice.amountexvat',
      'economy.supplier.invoice.amountincvat',
      'economy.supplier.invoice.remainingamount',
      'economy.supplier.invoice.foreignamount',
      'economy.supplier.invoice.foreignremainingamount',
      'economy.supplier.invoice.currencycode',
      'economy.supplier.invoice.invoicedate',
      'economy.supplier.invoice.duedate',
      'economy.supplier.invoice.paiddate',
      'economy.supplier.invoice.attest',
      'economy.supplier.invoice.attestname',
      'core.edit',
      'economy.supplier.invoice.invoice',
    ]);
  }

  private loadCurrencies(): Observable<ICompCurrencySmallDTO[]> {
    return this.coreService.getCompCurrenciesSmall().pipe(
      tap(x => {
        this.currencies = x;
      })
    );
  }

  private getPaymentCondition(paymentConditionId: number) {
    if (!paymentConditionId) {
      return;
    }
    this.service
      .getPaymentCondition(paymentConditionId)
      .pipe(
        tap(x => {
          this.supplier.paymentConditionName = x.name;
        })
      )
      .subscribe();
  }

  override createLegacyGridToolbar(): void {
    this.toolbarUtils.createLegacyGroup({
      buttons: [
        this.toolbarUtils.createLegacyButton({
          icon: IconUtil.createIcon('fal', 'search'),
          title: 'economy.supplier.suppliercentral.seeksupplierbutton',
          label: 'economy.supplier.suppliercentral.seeksupplierbutton',
          onClick: () => this.seekSupplier(),
          disabled: signal(false),
          hidden: signal(false),
        }),
      ],
    });
    this.toolbarUtils.createLegacyGroup({
      buttons: [
        this.toolbarUtils.createLegacyButton({
          icon: IconUtil.createIcon('fal', 'plus'),
          title: 'economy.supplier.invoice.createinvoice',
          label: 'economy.supplier.invoice.createinvoice',
          onClick: () => this.createInvoice(),
          disabled: signal(false),
          hidden: signal(false),
        }),
      ],
    });
  }

  createInvoice() {
    BrowserUtil.openInNewTab(
      window,
      `/soe/economy/supplier/invoice/status/default.aspx?classificationgroup=${SoeOriginStatusClassificationGroup.HandleSupplierInvoices}&invoiceId=0`
    );
  }

  editSupplierInvoice(row: ISupplierInvoiceGridDTO) {
    BrowserUtil.openInNewTab(
      window,
      `/soe/economy/supplier/invoice/status/default.aspx?classificationgroup=${SoeOriginStatusClassificationGroup.HandleSupplierInvoices}&invoiceId=${row.supplierInvoiceId}&invoiceNr=${row.invoiceNr}`
    );
  }

  supplierEdit(row: SupplierDTO) {
    const additionalProps = {
      editComponent: SuppliersEditComponent,
      FormClass: SupplierHeadForm,
      editTabLabel: this.translate.instant(
        'economy.supplier.supplier.supplier'
      ),
    };
    this.rowEdited.set({
      gridIndex: this.gridIndex(),
      rows: [],
      row,
      filteredRows: [],
      additionalProps,
    });
  }

  override onGridReadyToDefine(
    grid: GridComponent<ISupplierInvoiceGridDTO>
  ): void {
    super.onGridReadyToDefine(grid);
    this.grid.enableRowSelection();
    this.grid.addColumnNumber(
      'seqNr',
      this.terms['economy.supplier.invoice.seqnr'],
      {
        clearZero: true,
        flex: 1,
      }
    );
    this.grid.addColumnText(
      'invoiceNr',
      this.terms['economy.supplier.invoice.invoicenr'],
      { flex: 1 }
    );
    this.grid.addColumnText(
      'billingTypeName',
      this.terms['economy.supplier.invoice.invoicetype'],
      { flex: 1 }
    );
    this.grid.addColumnText(
      'attestStateName',
      this.terms['economy.supplier.invoice.attest'],
      { flex: 1 }
    );
    this.grid.addColumnText(
      'statusName',
      this.terms['common.tracerows.status'],
      { flex: 1 }
    );
    this.grid.addColumnText(
      'supplierName',
      this.terms['economy.supplier.supplier.supplier'],
      { flex: 1 }
    );
    this.grid.addColumnNumber(
      'totalAmountExVat',
      this.terms['economy.supplier.invoice.amountexvat'],
      { decimals: 2, flex: 1 }
    );
    this.grid.addColumnNumber(
      'totalAmount',
      this.terms['economy.supplier.invoice.amountincvat'],
      { decimals: 2, flex: 1 }
    );
    this.grid.addColumnNumber(
      'payAmount',
      this.terms['economy.supplier.invoice.remainingamount'],
      { decimals: 2, flex: 1 }
    );
    if (this.hasCurrencyPermission) {
      this.grid.addColumnNumber(
        'totalAmountCurrency',
        this.terms['economy.supplier.invoice.foreignamount'],
        { decimals: 2, flex: 1 }
      );
      this.grid.addColumnNumber(
        'payAmountCurrency',
        this.terms['economy.supplier.invoice.foreignremainingamount'],
        { decimals: 2, flex: 1 }
      );
      this.grid.addColumnText(
        'currencyCode',
        this.terms['economy.supplier.invoice.currencycode'],
        { flex: 1 }
      );
    }
    this.grid.addColumnDate(
      'invoiceDate',
      this.terms['economy.supplier.invoice.invoicedate'],
      { flex: 1 }
    );
    this.grid.addColumnDate(
      'dueDate',
      this.terms['economy.supplier.invoice.duedate'],
      { flex: 1 }
    );
    this.grid.addColumnDate(
      'payDate',
      this.terms['economy.supplier.invoice.paiddate'],
      { flex: 1 }
    );

    this.grid.addColumnIconEdit({
      tooltip: this.terms['core.edit'],
      onClick: row => {
        this.editSupplierInvoice(row);
      },
      flex: 1,
    });

    this.exportFilenameKey.set('economy.supplier.invoice.invoices');
    super.finalizeInitGrid();
  }

  private seekSupplier() {
    this.showSelectSupplier();
  }

  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  override loadUserSettings(): Observable<any> {
    const settingTypes: number[] = [
      UserSettingType.SupplierInvoiceAllItemsSelection,
    ];

    return this.coreService.getUserSettings(settingTypes).pipe(
      tap(x => {
        const setting = SettingsUtil.getIntUserSetting(
          x,
          UserSettingType.SupplierInvoiceAllItemsSelection,
          1
        );
        if (setting)
          this.supplierCentralGridForm.allItemsSelection.setValue(setting);
      })
    );
  }

  private loadInvoiceBillingTypes(): Observable<SmallGenericType[]> {
    return this.coreService
      .getTermGroupContent(TermGroup.InvoiceBillingType, false, false)
      .pipe(
        tap(x => {
          this.invoiceBillingTypes = x;
        })
      );
  }

  private loadOriginStatus(): Observable<SmallGenericType[]> {
    return this.coreService
      .getTermGroupContent(TermGroup.OriginStatus, false, false)
      .pipe(
        tap(x => {
          this.originStatus = x;
        })
      );
  }

  private showSelectSupplier() {
    const dialogOpts = <Partial<DialogData>>{
      size: 'lg',
      title: this.translate.instant(
        'economy.supplier.suppliercentral.selectsupplier'
      ),
    };
    this.dialogService
      .open(SelectSupplierModalComponent, dialogOpts)
      .afterClosed()
      .pipe(
        tap(supplier => {
          if (supplier) {
            this.urlService.setSupplierId(supplier.actorSupplierId);
            this.showSupplier();
          }
        })
      )
      .subscribe();
  }

  private showSupplier() {
    const counterTypes: number[] = [
      SoeOriginStatusClassification.SupplierPaymentsSupplierCentralUnpayed,
      SoeOriginStatusClassification.SupplierInvoicesOverdue,
      SoeOriginStatusClassification.SupplierPaymentsSupplierCentralPayed,
      SoeOriginStatusClassification.SupplierPaymentsSupplierCentralUnpayedForeign,
      SoeOriginStatusClassification.SupplierInvoicesOverdueForeign,
      SoeOriginStatusClassification.SupplierPaymentsSupplierCentralPayedForeign,
    ];
    this.supplierService
      .getSupplier(this.urlService.supplierId(), true, true, true, false)
      .pipe(
        tap(supplier => {
          if (!supplier && this.currencies) return;
          this.supplier = supplier as ExtSupplierDTO;
          this.getPaymentCondition(supplier.paymentConditionId ?? 0);
          this.supplierNumber = supplier.supplierNr;
          this.supplierName = supplier.name;
          this.supplier.currencyName =
            this.currencies.find(c => c.currencyId == this.supplier.currencyId)
              ?.name ?? '';
          this.supplier.blockPaymentString = this.supplier.blockPayment
            ? this.terms['core.yes']
            : this.terms['core.no'];
          this.supplier.isPrivatePersonString = this.supplier.isPrivatePerson
            ? this.terms['core.yes']
            : this.terms['core.no'];
        }),
        concatMap(() =>
          this.supplierService.getSupplierCentralCountersAndBalance(
            counterTypes,
            this.urlService.supplierId()
          )
        ),
        tap(x => {
          this.supplierCentralCountersAndBalances.next(x);
          this.supplierInvoiceStatusForeignPermissionInput.next(
            this.supplierInvoiceStatusForeignPermission
          );
          this.supplierInput.next(this.supplier);
        })
      )
      .subscribe();

    this.refreshGrid();
  }

  public updateItemsSelection(item: number) {
    if (!item) return;
    const model = {
      settingMainType: SettingMainType.User,
      settingTypeId: UserSettingType.SupplierInvoiceAllItemsSelection,
      intValue: item,
    };
    this.coreService
      .saveIntSetting(model)
      .pipe(
        tap(() => {
          this.refreshGrid();
        })
      )
      .subscribe();
  }

  private loadSelectionTypes(): Observable<SmallGenericType[]> {
    return this.coreService
      .getTermGroupContent(
        TermGroup.ChangeStatusGridAllItemsSelection,
        false,
        true,
        true
      )
      .pipe(
        tap(x => {
          this.allItemsSelectionDict = x;
          this.supplierCentralGridForm.allItemsSelection.setValue(
            x.find(x => x.id == 1)?.id ?? 1
          );
        })
      );
  }

  private loadModifyPermissions(): Observable<Record<number, boolean>> {
    const featureIds: number[] = [
      Feature.Economy_Supplier,
      Feature.Economy_Supplier_Suppliers_Edit,
      Feature.Economy_Supplier_Invoice,
      Feature.Economy_Supplier_Invoice_Invoices_Edit,
      Feature.Economy_Supplier_Invoice_Status_Foreign,
      Feature.Economy_Supplier_Invoice_AttestFlow,
    ];

    return this.coreService.hasModifyPermissions(featureIds).pipe(
      tap(x => {
        if (x[Feature.Economy_Supplier]) this.supplierSupplierPermission = true;
        if (x[Feature.Economy_Supplier_Suppliers_Edit])
          this.supplierSupplierSuppliersEditPermission = true;
        if (x[Feature.Economy_Supplier_Invoice])
          this.supplierInvoicePermission = true;
        if (x[Feature.Economy_Supplier_Invoice_Invoices_Edit])
          this.supplierInvoiceInvoiceInvoicesEditPermission = true;
        if (x[Feature.Economy_Supplier_Invoice_Status_Foreign])
          this.supplierInvoiceStatusForeignPermission = true;
        if (x[Feature.Economy_Supplier_Invoice_AttestFlow])
          this.supplierInvoiceStatusAttestFlowPermission = true;
      })
    );
  }

  ngOnDestroy(): void {
    this.unsubscribe.next();
    this.unsubscribe.complete();
  }
}

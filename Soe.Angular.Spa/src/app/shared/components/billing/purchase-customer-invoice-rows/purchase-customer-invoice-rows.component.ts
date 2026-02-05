import { Component, Input, OnInit, inject, signal } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { CrudActionTypeEnum } from '@shared/enums';
import { ValidationHandler } from '@shared/handlers';
import { TermCollection } from '@shared/localization/term-types';
import { SmallGenericType } from '@shared/models/generic-type.model';
import {
  CompanySettingType,
  Feature,
  PurchaseCustomerInvoiceViewType,
  TermGroup_AttestEntity,
} from '@shared/models/generated-interfaces/Enumerations';
import { ICustomerInvoiceRowPurchaseDTO } from '@shared/models/generated-interfaces/PurchaseDTOs';
import {
  IAttestStateDTO,
  IAttestTransitionDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ProgressService } from '@shared/services/progress/progress.service';
import { ProgressOptions } from '@shared/services/progress';
import { BrowserUtil } from '@shared/util/browser-util';
import { Perform } from '@shared/util/perform.class';
import { SettingsUtil } from '@shared/util/settings-util';
import { PurchaseService } from '@src/app/features/billing/purchase/services/purchase.service';
import { BillingService } from '@src/app/features/billing/services/services/billing.service';
import { ManageService } from '@src/app/features/manage/services/manage.service';
import { ColumnUtil } from '@ui/grid/util/column-util';
import { GridComponent } from '@ui/grid/grid.component';
import { MessageboxService } from '@ui/dialog/services/messagebox.service';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { CellClassParams } from 'ag-grid-community';
import { Observable, of } from 'rxjs';
import { tap } from 'rxjs/operators';
import { PurchaseCustomerInvoiceRowsForm } from './models/purchase-customer-invoice-rows-form.model';
import {
  AttestStateDTO,
  PurchaseCustomerInvoiceRowsDTO,
} from './models/purchase-customer-invoice-rows.model';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';

@Component({
  selector: 'soe-purchase-customer-invoice-rows',
  templateUrl: './purchase-customer-invoice-rows.component.html',
  styleUrls: ['./purchase-customer-invoice-rows.component.scss'],
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class PurchaseCustomerInvoiceRowsComponent
  extends GridBaseDirective<ICustomerInvoiceRowPurchaseDTO>
  implements OnInit
{
  @Input() viewType!: number;
  @Input() id!: number;

  validationHandler = inject(ValidationHandler);
  coreService = inject(CoreService);
  purchaseService = inject(PurchaseService);
  progressService = inject(ProgressService);
  manageService = inject(ManageService);
  billingService = inject(BillingService);
  messageboxService = inject(MessageboxService);
  translationService = inject(TranslateService);

  performLoad = new Perform<any>(this.progressService);
  performAction = new Perform<BillingService>(this.progressService);

  form: PurchaseCustomerInvoiceRowsForm = new PurchaseCustomerInvoiceRowsForm({
    validationHandler: this.validationHandler,
    element: new PurchaseCustomerInvoiceRowsDTO(),
  });

  initialAttestState!: IAttestStateDTO;
  attestTransitions: IAttestTransitionDTO[] = [];
  attestStates: IAttestStateDTO[] = [];
  availableAttestStates: AttestStateDTO[] = [];
  availableAttestStateOptions: { name: string; id: number }[] = [];
  attestStateTo: IAttestTransitionDTO[] = [];
  _attestStateTo: SmallGenericType[] = [];
  excludedAttestStates: number[] = [];

  disabledsaveAttestButton = signal(true);
  get useDetail(): boolean {
    return (
      this.viewType !== PurchaseCustomerInvoiceViewType.FromCustomerInvoiceRow
    );
  }

  private _selectedState = 0;
  get selectedState(): number {
    return this._selectedState;
  }
  set selectedState(value: number) {
    this._selectedState = value;
  }

  override ngOnInit() {
    super.ngOnInit();

    this.startFlow(
      Feature.Billing_Order_SupplierInvoices,
      'billing.purchase.rows',
      {
        additionalModifyPermissions: [
          Feature.Economy_Supplier_Invoice_Invoices_Edit,
        ],
        skipInitialLoad: true,
        lookups: [this.onLoadCompanySettings()],
      }
    );
  }

  override onFinished(): void {
    this.loadOrderGridvalues(this.viewType, this.id).subscribe();
    this.loadUserAttestTransitions().subscribe();
    this.setDisabledsaveAttestButtonSaveAttestButton();
  }

  loadOrderGridvalues(viewType: number, id: number) {
    return of(
      this.performLoad.load(
        this.purchaseService.getCustomerInvoiceRows(viewType, id).pipe(
          tap(value => {
            this.grid.setData(value);
          })
        )
      )
    );
  }

  onLoadCompanySettings() {
    const settingTypes: number[] = [
      CompanySettingType.BillingStatusTransferredOrderToInvoice,
      CompanySettingType.BillingStatusTransferredOrderToContract,
    ];
    return this.coreService.getUserCompanySettingForCompany(settingTypes).pipe(
      tap(setting => {
        const attestStateTransferredOrderToInvoiceId =
          SettingsUtil.getIntCompanySetting(
            setting,
            CompanySettingType.BillingStatusTransferredOrderToInvoice
          );

        if (attestStateTransferredOrderToInvoiceId !== 0)
          this.excludedAttestStates.push(
            attestStateTransferredOrderToInvoiceId
          );

        const attestStateTransferredOrderToContractId =
          SettingsUtil.getIntCompanySetting(
            setting,
            CompanySettingType.BillingStatusTransferredOrderToContract
          );

        if (attestStateTransferredOrderToContractId !== 0)
          this.excludedAttestStates.push(
            attestStateTransferredOrderToContractId
          );
      })
    );
  }

  loadUserAttestTransitions() {
    return of(
      this.performLoad.load(
        this.manageService
          .getUserAttestTransitions(TermGroup_AttestEntity.Order, 0, 0)
          .pipe(
            tap(result => {
              this.attestTransitions = result;
              // Add states from returned transitions
              this.attestTransitions.forEach(t => {
                if (
                  !this.attestStates.find(
                    a => a.attestStateId === t.attestStateToId
                  )
                )
                  this.attestStates.push(t.attestStateTo);
              });

              // Sort states
              this.attestStates = this.attestStates.sort(a => a.sort);

              // Get initial state

              const initAttestSts = this.attestStates.find(
                a => a.initial === true
              );
              if (initAttestSts) {
                this.initialAttestState = initAttestSts;
              } else {
                this.loadInitialAttestState(
                  TermGroup_AttestEntity.Order
                ).subscribe();
              }

              // Setup available states (exclude finished states)
              this.availableAttestStates = [];
              this.attestStates.forEach(attestState => {
                if (
                  !this.excludedAttestStates.find(
                    ex => ex === attestState.attestStateId
                  )
                ) {
                  this.availableAttestStates.push({
                    ...new AttestStateDTO(),
                    ...attestState,
                  });
                }
              });

              // Setup available states for selector
              this.availableAttestStateOptions = [];
              this.availableAttestStateOptions = [
                {
                  id: 0,
                  name: this.terms['billing.productrows.changeatteststate'],
                },
                ...this.availableAttestStates.map(a => {
                  return { id: a.attestStateId, name: a.name };
                }),
              ];
            })
          )
      )
    );
  }

  attestStateChanged(type: number) {
    if (!type) return;
    const attestState = this.getSelectedAttestState(type);
    if (attestState) {
      this.grid?.api.getRenderedNodes().forEach((r, idx) => {
        if (
          this.attestTransitions.find(
            a =>
              a.attestStateFromId === r.data.attestStateId &&
              attestState.attestStateId === a.attestStateToId
          )
        )
          this.grid?.api.getRenderedNodes()[idx].setSelected(true);
        else this.grid?.api.getRenderedNodes()[idx].setSelected(false);
      });
    }
    this.setDisabledsaveAttestButtonSaveAttestButton();
  }

  setDisabledsaveAttestButtonSaveAttestButton() {
    this.disabledsaveAttestButton.set(
      !(this.grid.getSelectedCount() > 0 && this.form.attestStateTo.value !== 0)
    );
  }

  selectionChanged(selectedRows: any[]): void {
    this.setDisabledsaveAttestButtonSaveAttestButton();
  }

  public getSelectedAttestState(type: number) {
    return this.attestStates.find(a => a.attestStateId === type);
  }

  saveAttestState() {
    const ids = this.grid.getSelectedRows().map(item => {
      return {
        field1: item.invoiceId,
        field2: item.customerInvoiceRowId,
      };
    });
    const model = {
      items: ids,
      attestStateId: this.form.attestStateTo.value,
    };
    this.performPriceUpdate(model);
  }

  performPriceUpdate(model: unknown) {
    this.performAction.crud(
      CrudActionTypeEnum.Save,
      this.billingService.updateOrderRowAttestState(model),
      (successResponse: BackendResponse) =>
        this.afterSaveChangedStatusSuccess(successResponse),
      undefined,
      { showDialogOnError: true, showToastOnError: true }
    );
  }

  afterSaveChangedStatusSuccess(response: BackendResponse) {
    this.progressService.saveComplete(<ProgressOptions>{
      showDialogOnComplete: true,
      showToastOnComplete: false,
    });
    this.loadOrderGridvalues(this.viewType, this.id);
  }

  override loadTerms(): Observable<TermCollection> {
    return super.loadTerms([
      'billing.productrows.initialstatemissing.title',
      'billing.productrows.initialstatemissing.message',
      'billing.productrows.changeatteststate',
      'common.customer.invoices.seqnr',
      'common.unit',
      'common.status',
      'common.type',
      'common.text',
      'common.date',
      'common.report.selection.purchasenr',

      'billing.productrows.quantity',
      'billing.productrows.invoicequantity',
      'billing.productrows.productnr',
      'billing.productrows.purchasestatus',
      'billing.order.deliverydate',
      'billing.order.ordernr',

      'billing.purchase.supplierno',
      'billing.purchase.suppliername',
      'billing.purchase.delivery.remainingqty',
      'billing.purchase.delivery.purchaseqty',
      'billing.purchaserows.quantity',
      'billing.purchaserows.deliveredquantity',
      'billing.purchaserows.wanteddeliverydate',
      'billing.purchaserows.accdeliverydate',
      'billing.purchaserows.deliverydate',
      'billing.projects.list.invoicedquantity',
      'billing.order.orderdeliverydate',
    ]);
  }

  override onGridReadyToDefine(
    grid: GridComponent<ICustomerInvoiceRowPurchaseDTO>
  ) {
    super.onGridReadyToDefine(grid);

    this.grid.enableRowSelection();
    if (this.useDetail) {
      this.setupPurchaseRowsGrid(grid);
      this.setupCustomerInvoiceRowsGrid(grid);
    } else {
      this.setupPurchaseRowsGrid(grid);
    }

    this.exportFilenameKey.set('Shared.Directives.PurchaseCustomerInvoiceRows');
    super.finalizeInitGrid();
  }

  private setupPurchaseRowsGrid(
    grid: GridComponent<ICustomerInvoiceRowPurchaseDTO>
  ) {
    //Details
    grid.enableMasterDetail(
      {
        floatingFiltersHeight: 0,

        columnDefs: [
          ColumnUtil.createColumnText(
            'purchaseNr',
            this.terms['common.report.selection.purchasenr'],
            { flex: 1 }
          ),
          ColumnUtil.createColumnText(
            'purchaseStatusName',
            this.terms['billing.productrows.purchasestatus'],
            { flex: 1 }
          ),
          ColumnUtil.createColumnText(
            'supplierName',
            this.terms['billing.purchase.suppliername'],
            { flex: 1 }
          ),
          ColumnUtil.createColumnText(
            'supplierNr',
            this.terms['billing.purchase.supplierno'],
            { flex: 1 }
          ),
          ColumnUtil.createColumnText(
            'productNr',
            this.terms['billing.productrows.productnr'],
            { flex: 1 }
          ),
          ColumnUtil.createColumnText('text', this.terms['common.text'], {
            flex: 1,
          }),
          ColumnUtil.createColumnNumber(
            'purchaseQuantity',
            this.terms['billing.purchase.delivery.purchaseqty'],
            { flex: 1 }
          ),
          ColumnUtil.createColumnNumber(
            'deliveredQuantity',
            this.terms['billing.purchaserows.deliveredquantity'],
            { flex: 1 }
          ),
          ColumnUtil.createColumnNumber(
            'remainingQuantity',
            this.terms['billing.purchase.delivery.remainingqty'],
            { flex: 1 }
          ),
          ColumnUtil.createColumnText('unit', this.terms['common.unit'], {
            flex: 1,
          }),
          ColumnUtil.createColumnText(
            'rowStatusName',
            this.terms['common.status'],
            {
              flex: 1,
            }
          ),
          ColumnUtil.createColumnText('dateStatus', this.terms['common.type'], {
            flex: 1,
          }),
          ColumnUtil.createColumnNumber('date', this.terms['common.date'], {
            flex: 1,
          }),
        ],
      },
      {
        getDetailRowData: (params: any) => {
          this.loadDetailRows(params);
        },
      }
    );
  }

  private setupCustomerInvoiceRowsGrid(
    grid: GridComponent<ICustomerInvoiceRowPurchaseDTO>
  ) {
    grid.addColumnNumber('invoiceSeqNr', this.terms['billing.order.ordernr'], {
      flex: 1,
      enableHiding: false,
      buttonConfiguration: {
        iconPrefix: 'fal',
        iconName: 'pen',
        show: () => true,
        tooltip: this.terms['billing.productrows.edit'],
        onClick: (row: ICustomerInvoiceRowPurchaseDTO) => this.openOrder(row),
      },
    });
    grid.addColumnText(
      'productNr',
      this.terms['billing.productrows.productnr'],
      {
        flex: 1,
        enableHiding: false,
      }
    );
    grid.addColumnText('text', this.terms['common.text'], {
      flex: 1,
      enableHiding: false,
    });
    grid.addColumnText('unit', this.terms['common.unit'], {
      flex: 1,
      enableHiding: false,
    });
    grid.addColumnNumber(
      'quantity',
      this.terms['billing.productrows.quantity'],
      {
        flex: 1,
        enableHiding: false,
        cellClassRules: {
          'success-background-color': (params: CellClassParams) =>
            this.validateRules(params),
        },
      }
    );
    grid.addColumnText(
      'invoiceQuantity',
      this.terms['billing.productrows.invoicequantity'],
      {
        flex: 1,
        enableHiding: false,
        hide: true,
      }
    );
    grid.addColumnNumber(
      'deliveredPurchaseQuantity',
      this.terms['billing.purchaserows.deliveredquantity'],
      {
        flex: 1,
        enableHiding: false,
        cellClassRules: {
          'success-background-color': (params: CellClassParams) =>
            this.validateRules(params),
        },
      }
    );
    grid.addColumnDate(
      'deliveryDate',
      this.terms['billing.order.orderdeliverydate'],
      {
        flex: 1,
        enableHiding: false,
      }
    );
    grid.addColumnShape('attestStateColor', '', {
      flex: 1,
      enableHiding: false,
      shape: 'circle',
      colorField: 'attestStateColor',
      tooltipField: 'attestStatus',
    });
  }

  loadDetailRows(params: any) {
    params.data.purchaseRows.forEach((purchaseRow: any) => {
      if (purchaseRow.deliveryDate) {
        purchaseRow.date = params.data.purchaseRows.deliveryDate;
        purchaseRow.dateStatus =
          this.terms['billing.purchaserows.deliverydate'];
      }
      if (purchaseRow.confirmedDate) {
        purchaseRow.date = params.data.purchaseRows.confirmedDate;
        purchaseRow.dateStatus =
          this.terms['billing.purchaserows.accdeliverydate'];
      }
      if (purchaseRow.requestedDate) {
        purchaseRow.date = params.data.purchaseRows.requestedDate;
        purchaseRow.dateStatus =
          this.terms['billing.purchaserows.wanteddeliverydate'];
      }
    });
    params.successCallback(params.data.purchaseRows);
  }

  validateRules(params: CellClassParams) {
    return params.data.quantity <= params.data.deliveredPurchaseQuantity;
  }

  loadInitialAttestState(
    entity: TermGroup_AttestEntity
  ): Observable<IAttestStateDTO> {
    return this.coreService.getAttestStateInitial(entity).pipe(
      tap(x => {
        this.initialAttestState = x;

        if (!this.initialAttestState) {
          this.messageboxService.error(
            this.terms['billing.productrows.initialstatemissing.title'],
            this.terms['billing.productrows.initialstatemissing.message']
          );
        } else {
          this.attestStates.push(this.initialAttestState);
          // Sort states
          this.attestStates = this.attestStates.sort((a, b) => a.sort - b.sort);
        }
      })
    );
  }

  private openOrder(row: ICustomerInvoiceRowPurchaseDTO) {
    BrowserUtil.openInNewTab(
      window,
      `/soe/billing/order/status/default.aspx?invoiceId=${row.invoiceId}&invoiceNr=${row.invoiceSeqNr}`
    );
  }
}

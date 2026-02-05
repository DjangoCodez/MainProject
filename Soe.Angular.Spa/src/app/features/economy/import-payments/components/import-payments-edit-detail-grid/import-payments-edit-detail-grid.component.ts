import {
  Component,
  EventEmitter,
  Input,
  OnInit,
  Output,
  inject,
  signal,
} from '@angular/core';
import { SelectCustomerInvoiceDialogComponent } from '@shared/components/select-customer-invoice-dialog/component/select-customer-invoice-dialog/select-customer-invoice-dialog.component';
import { CustomerInvoiceSearchDTO } from '@shared/components/select-customer-invoice-dialog/model/customer-invoice-search.model';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { TermCollection } from '@shared/localization/term-types';
import {
  Feature,
  ImportPaymentIOState,
  ImportPaymentIOStatus,
  ImportPaymentType,
  SoeOriginType,
} from '@shared/models/generated-interfaces/Enumerations';
import { IMatchCodeDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { BrowserUtil } from '@shared/util/browser-util';
import { NumberUtil } from '@shared/util/number-util';
import { SoeConfigUtil } from '@shared/util/soeconfig-util';
import { StringUtil } from '@shared/util/string-util';
import { AggregationType } from '@ui/grid/interfaces';
import { DialogService } from '@ui/dialog/services/dialog.service';
import { GridComponent } from '@ui/grid/grid.component';
import { IMessageboxComponentResponse } from '@ui/dialog/models/messagebox';
import { MenuButtonItem } from '@ui/button/menu-button/menu-button.component';
import { MessageboxService } from '@ui/dialog/services/messagebox.service';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { CellClassParams, CellValueChangedEvent } from 'ag-grid-community';
import { BehaviorSubject, of, take, tap } from 'rxjs';
import { ImportPaymentsForm } from '../../models/import-payments-form.model';
import {
  PaymentImportIODTO,
  PaymentImportUpdateFunctions,
  SaveCustomerPaymentImportIODTOModel,
  SavePaymentImportIODTOModel,
} from '../../models/import-payments.model';
import { ImportPaymentsService } from '../../services/import-payments.service';
import { ImportPaymentSharedHandler } from '../../services/import-payments.shared.handler';
import { PersistedAccountingYearService } from '@features/economy/services/accounting-year.service';

@Component({
  selector: 'soe-import-payments-edit-detail-grid',
  templateUrl: './import-payments-edit-detail-grid.component.html',
  styleUrls: ['./import-payments-edit-detail-grid.component.scss'],
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class ImportPaymentsEditDetailGridComponent
  extends GridBaseDirective<PaymentImportIODTO>
  implements OnInit
{
  @Input({ required: true }) form!: ImportPaymentsForm;
  @Input({ required: true }) importPaymentTypeId!: ImportPaymentType;
  @Input() useExternalInvoiceNr: boolean = false;

  @Input() terms!: TermCollection;
  @Input() matchCodes: IMatchCodeDTO[] = [];
  @Input() rows = new BehaviorSubject<PaymentImportIODTO[]>([]);
  @Input() customerPaymentEditPermission = signal(false);
  @Output() peformUpdateAction = new EventEmitter<any>();
  flowHandler = inject(FlowHandlerService);
  importPaymentsService = inject(ImportPaymentsService);
  messageboxService = inject(MessageboxService);
  dialogService = inject(DialogService);
  importPaymentSharedHandler = inject(ImportPaymentSharedHandler);
  ayService = inject(PersistedAccountingYearService);
  disabledUpdatePaymentButton = signal(true);
  disabledAddRowButton = signal(true);
  filteredPaid = signal(0);
  selectedPaid = signal(0);
  menuList: MenuButtonItem[] = [];

  ngOnInit() {
    super.ngOnInit();
    this.startFlow(
      Feature.Economy_Import_Payments,
      'Economy.Import.Payment.Payments.ImportedIoInvoices',
      {
        skipInitialLoad: true,
        useLegacyToolbar: true,
        lookups: [this.ayService.loadSelectedAccountYear()],
      }
    );
    this.buildPaymentImportUpdateFunctionsList();
    this.setValueChangeHandlers();
  }

  private setValueChangeHandlers() {
    this.form?.paymentImportId.valueChanges.subscribe(id => {
      this.disabledAddRowButton.set(id <= 0);
    });
  }
  override createLegacyGridToolbar(): void {
    if (this.importPaymentTypeId === ImportPaymentType.CustomerPayment) {
      super.createLegacyGridToolbar({
        hideReload: true,
        hideClearFilters: true,
      });

      this.toolbarUtils.toolbarGroups[0].buttons.push(
        this.toolbarUtils.createLegacyButton({
          icon: 'plus',
          title: 'core.newrow',
          label: 'core.newrow',
          onClick: () => this.add(),
          disabled: this.disabledAddRowButton,
          hidden: signal(false),
        })
      );
    }
  }

  override onFinished(): void {
    this.setInitFilteredTotalPaidAmount();
    this.setSubscribers();
  }

  setSubscribers() {
    this.rows.subscribe(rows => {
      if (rows.length > 0) {
        this.setFilteredTotalPaidAmount();
      }
    });
  }

  buildPaymentImportUpdateFunctionsList() {
    this.menuList = [];

    this.menuList.push(
      {
        id: PaymentImportUpdateFunctions.UpdatePayment,
        label: this.translate.instant(
          'economy.import.payment.updatepaymentsbutton'
        ),
      },
      {
        id: PaymentImportUpdateFunctions.UpdateStatus,
        label: this.translate.instant(
          'economy.import.payment.updatestatusbutton'
        ),
      }
    );
  }

  add() {
    const newRow = new PaymentImportIODTO();

    newRow.tempRowId = this.rows.getValue().length + 1;
    newRow.status = ImportPaymentIOStatus.Manual;
    newRow.statusId = ImportPaymentIOStatus.Manual;
    newRow.importType = ImportPaymentType.CustomerPayment;
    newRow.paidDate = this.form?.importDate.value
      ? this.form?.importDate.value
      : new Date();

    this.importPaymentSharedHandler.setStatus(newRow, this.terms);

    const currentRows = this.rows.getValue();
    const updatedRows = [...currentRows, newRow];
    this.rows.next(updatedRows);
    this.grid.options.context.newRow = true;
    this.form.markAsDirty();
  }

  override onGridReadyToDefine(grid: GridComponent<PaymentImportIODTO>) {
    super.onGridReadyToDefine(grid);
    this.exportFilenameKey.set('economy.import.payment.payments');
    this.grid.api.updateGridOptions({
      onCellValueChanged: this.onCellValueChanged.bind(this),
      onFilterModified: this.onFilterModified.bind(this),
    });

    this.translate
      .get([
        'core.warning',
        'common.type',
        'economy.import.payment.customer',
        'economy.import.payment.invoiceseries',
        'economy.import.payment.invoicenr',
        'economy.import.payment.paymentdate',
        'economy.import.payment.invoicetotalamount',
        'economy.import.payment.duedate',
        'economy.import.payment.matchcode',
        'economy.import.payment.paidamount',
        'economy.import.payment.paiddate',
        'economy.import.payment.paymenttypename',
        'economy.import.payment.currency',
        'economy.supplier.invoice.remainingamount',
        'economy.import.payment.status',
        'economy.import.payment.supplier',
        'economy.import.payment.debit',
        'economy.import.payment.credit',
        'economy.import.payment.fullypaid',
        'economy.import.payment.matched',
        'economy.import.payment.paid',
        'economy.import.payment.partly_paid',
        'economy.import.payment.rest',
        'economy.import.payment.unknown',
        'economy.import.payment.error',
        'economy.supplier.invoice.duedate',
        'core.aggrid.totals.filtered',
        'core.aggrid.totals.total',
        'economy.import.payment.error.paymentrowerror',
        'core.manual',
        'economy.import.payment.amountvalidationfailed',
        'common.customer.payment.paymentseqnr',
        'common.customer.payment.payment',
        'economy.import.payment.deleted',
        'economy.import.payments.importdate',
        'economy.supplier.invoice.ocr',
        'core.comment',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.setNbrOfRowsToShow(13, 13);
        this.grid.enableRowSelection((row: any) => {
          return (
            row.data.statusId !== ImportPaymentIOStatus.Paid &&
            row.data.state != ImportPaymentIOState.Closed
          );
        }, false);
        this.grid.addColumnModified('isModified');
        this.grid.addColumnText('typeName', terms['common.type'], {
          flex: 1,
          editable: false,
        });
        if (
          this.importPaymentTypeId === ImportPaymentType.SupplierPayment
        ) {
          this.grid.addColumnText(
            'customer',
            terms['economy.import.payment.supplier'],
            {
              flex: 1,
              editable: false,
            }
          );
          this.grid.addColumnText(
            'invoiceSeqnr',
            terms['economy.import.payment.invoiceseries'],
            {
              flex: 1,
              editable: false,
            }
          );
        } else {
          this.grid.addColumnText(
            'customer',
            terms['economy.import.payment.customer'],
            {
              flex: 1,
              editable: false,
            }
          );
          this.grid.addColumnText(
            'ocr',
            terms['economy.supplier.invoice.ocr'],
            {
              flex: 1,
              editable: false,
              enableHiding: true,
            }
          );
        }
        this.grid.addColumnText(
          'invoiceNr',
          terms['economy.import.payment.invoicenr'],
          {
            flex: 1,
            editable: row => {
              return row.data?.state !== ImportPaymentIOState.Closed;
            },
            buttonConfiguration: {
              iconPrefix: 'fal',
              iconName: 'search',

              show: row =>
                this.importPaymentTypeId ===
                  ImportPaymentType.CustomerPayment &&
                row.state !== ImportPaymentIOState.Closed &&
                row.statusId !== ImportPaymentIOStatus.Paid,
              onClick: row => {
                if (row.state !== ImportPaymentIOState.Closed)
                  this.openSearchInvoiceDialog(row);
              },
            },
          }
        );
        this.grid.addColumnDate(
          'dueDate',
          terms['economy.supplier.invoice.duedate'],
          {
            flex: 1,
            editable: false,
            enableHiding:
              this.importPaymentTypeId ===
              ImportPaymentType.CustomerPayment,
          }
        );
        this.grid.addColumnDate(
          'paidDate',
          terms['economy.import.payment.paiddate'],
          {
            flex: 1,
            editable: row => {
              return row.data?.state !== ImportPaymentIOState.Closed;
            },
            enableHiding: false,
          }
        );
        if (
          this.importPaymentTypeId === ImportPaymentType.SupplierPayment
        ) {
          this.grid.addColumnText(
            'paymentTypeName',
            terms['economy.import.payment.paymenttypename'],
            {
              flex: 1,
              editable: false,
            }
          );
        }
        this.grid.addColumnNumber(
          'invoiceAmount',
          terms['economy.import.payment.invoicetotalamount'],
          {
            editable: false,
            decimals: 2,
            cellClassRules: {
              'error-background-color': (params: CellClassParams) =>
                this.amountToPayCellRules(params),
            },
          }
        );
        this.grid.addColumnNumber(
          'paidAmount',
          terms['economy.import.payment.paidamount'],
          {
            editable: row => {
              return row.data?.state !== ImportPaymentIOState.Closed;
            },
            decimals: 2,
            cellClassRules: {
              'error-background-color': (params: CellClassParams) =>
                this.paidAmountCellRules(params),
            },
          }
        );
        this.grid.addColumnNumber(
          'restAmount',
          terms['economy.supplier.invoice.remainingamount'],
          {
            editable: false,
            decimals: 2,
          }
        );

        this.grid.addColumnText(
          'statusName',
          terms['economy.import.payment.status'],
          {
            flex: 1,
            editable: false,
          }
        );

        if (
          this.importPaymentTypeId === ImportPaymentType.CustomerPayment
        ) {
          this.grid.addColumnNumber(
            'paymentRowSeqNr',
            terms['common.customer.payment.paymentseqnr'],
            {
              flex: 1,
              editable: false,
              formatAsText: true,
              buttonConfiguration: {
                iconPrefix: 'fal',
                iconName: 'pen',
                show: row => {
                  return this.isPaymentRowSeqNrEditable(row);
                },
                onClick: row => this.openPayment(row),
              },
            }
          );
          this.grid.addColumnAutocomplete(
            'matchCodeId',
            terms['economy.import.payment.matchcode'],
            {
              editable: true,
              flex: 1,
              source: () => this.matchCodes,
              optionIdField: 'matchCodeId',
              optionNameField: 'name',
            }
          );

          this.grid.addColumnIconDelete({
            tooltip: terms['core.delete'],
            showIcon: row => this.showDelete(row),
            onClick: row => {
              this.rowDelete(row);
            },
          });
        }

        this.grid.addColumnText('comment', terms['core.comment'], {
          flex: 1,
          editable: false,
          enableHiding: true,
          tooltipField: 'comment',
        });

        this.grid.columns.forEach(col => {
          const cellcls: string = col.cellClass ? col.cellClass.toString() : '';
          col.cellClass = (grid: any) => {
            if (grid.data.isSelectDisabled)
              return cellcls + ' disabled-grid-row-background-color';
            else return cellcls;
          };
        });

        this.grid.addAggregationsRow({
          invoiceAmount: AggregationType.Sum,
          paidAmount: AggregationType.Sum,
          restAmount: AggregationType.Sum,
        });
        super.finalizeInitGrid();
      });
  }

  disabledCellRules(params: any) {
    if (params.data.isSelectDisabled) {
      return true;
    }
    return false;
  }

  paidAmountCellRules(params: any) {
    return (
      params.data.status === ImportPaymentIOStatus.PartlyPaid &&
      params.data.state === ImportPaymentIOState.Open
    );
  }

  amountToPayCellRules(params: any) {
    return (
      params.data.status === ImportPaymentIOStatus.Unknown &&
      params.data.invoiceAmount === 0
    );
  }

  openPayment(row: PaymentImportIODTO) {
    if (row.paymentRowId) {
      BrowserUtil.openInNewWindow(
        window,
        `/soe/economy/customer/invoice/status/?classificationgroup=9&paymentId=${row.paymentRowId}&seqNr=${row.paymentRowSeqNr}`
      );
    }
  }

  matchCodeChanged(row: PaymentImportIODTO) {
    if (StringUtil.isNumeric(row.matchCodeName)) {
      row.matchCodeId = Number(row.matchCodeName);
    }
  }

  openSearchInvoiceDialog(row: PaymentImportIODTO) {
    const dialogData = new CustomerInvoiceSearchDTO();
    dialogData.title = this.translate.instant(
      'common.customer.invoices.chooseinvoice'
    );

    dialogData.size = 'xl';
    dialogData.originType =
      this.importPaymentTypeId == ImportPaymentType.CustomerPayment
        ? SoeOriginType.CustomerInvoice
        : SoeOriginType.SupplierInvoice;
    dialogData.isNew = true;
    dialogData.ignoreChildren = false;
    dialogData.customerId = undefined;
    dialogData.projectId = undefined;
    dialogData.invoiceId = undefined;
    dialogData.currentMainInvoiceId = undefined;
    dialogData.selectedProjectName = '';
    dialogData.userId = undefined;
    dialogData.includePreliminary = false;
    dialogData.includeVoucher = true;
    dialogData.fullyPaid = false;
    dialogData.useExternalInvoiceNr = this.useExternalInvoiceNr;
    dialogData.importRow = row;
    this.dialogService
      .open(SelectCustomerInvoiceDialogComponent, dialogData)
      .afterClosed()
      .subscribe(result => {
        this.grid.applyChanges();
        if (result && result.number) {
          row.invoiceNr = result.number;
          this.updatePaymentImportIO(row);
        }
      });
  }

  private isCellEditable(row: PaymentImportIODTO) {
    return row.state !== ImportPaymentIOState.Closed;
  }

  private isPaymentRowSeqNrEditable(row: PaymentImportIODTO): boolean {
    if (
      row.paymentRowId &&
      row.paymentRowId > 0 &&
      this.customerPaymentEditPermission()
    )
      return true;
    return false;
  }

  showDelete(row: PaymentImportIODTO) {
    let allowDelete: boolean =
      row.statusId === ImportPaymentIOStatus.Manual &&
      (row.paymentImportIOId === undefined || row.paymentImportIOId === 0);
    if (!allowDelete) {
      allowDelete =
        row.statusId === ImportPaymentIOStatus.Unknown && row.paidAmount === 0;
    }

    return allowDelete;
  }

  onGridSelectionChanged(rows: PaymentImportIODTO[]) {
    this.disabledUpdatePaymentButton.set(true);
    let sum = 0;
    if (rows && rows.length > 0) {
      this.disabledUpdatePaymentButton.set(false);
      this.selectedPaid.set(0);

      rows.forEach(f => {
        if (f.paidAmount) sum = sum + f.paidAmount;
      });
    }
    this.selectedPaid.set(sum);
  }

  onCellValueChanged(event: CellValueChangedEvent) {
    event.data.isModified = true;
    switch (event.colDef.field) {
      case 'invoiceNr':
        this.updatePaymentImportIO(event.data);
        break;
      case 'paidAmount':
        event.data.restAmount =
          event.data.invoiceAmount < 0
            ? event.data.invoiceAmount + event.data.paidAmount * -1
            : event.data.invoiceAmount - event.data.paidAmount;

        this.grid.agGrid.api.setFocusedCell(event.rowIndex || 0, 'matchCodeId');
        this.grid.agGrid.api.startEditingCell({
          rowIndex: event.rowIndex || 0,
          colKey: 'matchCodeId',
        });
        break;
      case 'matchCodeId':
        var id = Number(event.data.matchCodeId);
        var matchCode = this.getMatchCode(id);
        if (matchCode) {
          event.data.matchCodeName = matchCode.name;
        }
        break;
      default:
        break;
    }

    this.form?.setDirtyOnPaymentRowChange(event.data.paymentImportIOId);
  }

  onFilterModified(event: any) {
    this.setFilteredTotalPaidAmount();
  }
  private setInitFilteredTotalPaidAmount() {
    let sum = 0;
    this.rows.getValue().forEach(f => {
      if (f.paidAmount) sum = sum + f.paidAmount;
    });
    this.filteredPaid.set(sum);
  }

  private setFilteredTotalPaidAmount() {
    let sum = 0;
    this.grid.getFilteredRows().forEach(f => {
      if (f.paidAmount) sum = sum + f.paidAmount;
    });
    this.filteredPaid.set(sum);
  }

  rowDelete(row: PaymentImportIODTO) {
    if (row.paymentImportIOId) {
      this.importPaymentsService
        .deletePaymentImportIORow(row.paymentImportIOId)
        .pipe(
          tap((result: any) => {
            if (result.success) {
              this.peformUpdateAction.emit(result);
            } else {
              this.messageboxService.error(
                this.translate.instant('core.error'),
                result.errorMessage
              );
            }
          })
        );
    } else {
      const initRows = this.rows.getValue();
      const updatedRows = initRows.filter(r => r.tempRowId !== row.tempRowId);
      this.rows.next(updatedRows);
    }
  }

  peformAction(selected: MenuButtonItem): void {
    switch (selected.id) {
      case PaymentImportUpdateFunctions.UpdatePayment:
        this.initSaveBulkPaymentImportIOInvoices();
        break;
      case PaymentImportUpdateFunctions.UpdateStatus:
        this.initUpdatePaymentImportIOStatus();
        break;
    }
  }

  private updatePaymentImportIO(row: PaymentImportIODTO): void {
    // Set required properties
    row.actorCompanyId = SoeConfigUtil.actorCompanyId;
    row.importType = this.importPaymentTypeId;

    this.importPaymentsService
      .updatePaymentImportIO(row)
      .pipe(
        tap(result => {
          if (result.errorMessage) {
            this.messageboxService.error(
              this.translate.instant('core.error'),
              result.errorMessage
            );
            return;
          }

          if (!result.value) {
            return;
          }

          const originalRows = this.rows.getValue();
          const isNewRow =
            row.paymentImportIOId === undefined || row.paymentImportIOId === 0;
          const changedRow = isNewRow
            ? originalRows.find(r => r === row)
            : originalRows.find(
                r => r.paymentImportIOId === row.paymentImportIOId
              );

          if (changedRow) {
            this.updateRow(changedRow, result.value);
            this.rows.next(originalRows);

            if (isNewRow) {
              return;
            }
          }

          // Emit update action for rows that need a full reload
          if (!isNewRow) {
            this.peformUpdateAction.emit(result);
          }
        })
      )
      .subscribe();
  }

  private updateRow(
    row: PaymentImportIODTO,
    updatedValues: PaymentImportIODTO
  ): void {
    Object.assign(row, updatedValues);
    row.isModified = true;
    this.importPaymentSharedHandler.setStatusTexts(
      row,
      this.terms,
      this.matchCodes
    );
  }

  private initUpdatePaymentImportIOStatus() {
    const mb = this.messageboxService.show(
      this.translate.instant('core.comment'),
      ' ',
      {
        showInputText: true,
        inputTextRows: 3,
      }
    );

    mb.afterClosed().subscribe((response: IMessageboxComponentResponse) => {
      if (response.textValue) {
        this.updatePaymentImportIOStatus(response.textValue);
      } else {
        this.messageboxService.warning(
          this.translate.instant('core.warning'),
          this.translate.instant('core.missingcomment')
        );
      }
    });
  }

  private updatePaymentImportIOStatus(comment: string) {
    const rowsToUpdate: PaymentImportIODTO[] = [];

    this.grid
      .getSelectedRows()
      .filter(
        r =>
          r.status === ImportPaymentIOStatus.Unknown ||
          r.status === ImportPaymentIOStatus.ManuallyHandled
      )
      .forEach(row => {
        row.comment = comment;
        rowsToUpdate.push(row);
      });
    const obj = new SaveCustomerPaymentImportIODTOModel(
      rowsToUpdate,
      new Date(),
      0,
      0
    );
    this.importPaymentsService
      .updatePaymentImportIODTOSStatus(obj)
      .pipe(
        tap(result => {
          if (result.success) {
            this.messageboxService.success(
              this.translate.instant('common.status'),
              this.translate
                .instant('economy.import.payment.updatedstatusmessage')
                .format(
                  rowsToUpdate.length.toString(),
                  this.grid.getSelectedRows().length.toString()
                )
            );
          } else {
            this.messageboxService.error(
              this.translate.instant('core.error'),
              result.errorMessage
            );
          }
        })
      )
      .subscribe(res => {
        this.peformUpdateAction.emit(res);
      });
  }

  private initSaveBulkPaymentImportIOInvoices() {
    const paymentImportIODTOsToUpdate = this.grid
      .getSelectedRows()
      .filter(
        r =>
          r.paidAmount &&
          r.paidAmount !== 0 &&
          r.invoiceId &&
          r.invoiceId !== 0 &&
          r.status !== ImportPaymentIOStatus.Unknown
      );
    if (this.importPaymentTypeId === ImportPaymentType.CustomerPayment) {
      if (!this.validatePayments()) return;
    }
    this.saveBulkPaymentImportIOInvoices(paymentImportIODTOsToUpdate);
  }

  private validatePayments() {
    let totalPaidAmount = 0;
    this.rows.getValue().forEach(f => {
      totalPaidAmount = totalPaidAmount + (f.paidAmount ? f.paidAmount : 0);
    });

    if (
      Number(this.form.totalAmount.value).round(4) !== totalPaidAmount.round(4)
    ) {
      this.messageboxService.warning(
        this.translate.instant('core.warning'),
        this.translate.instant('economy.import.payment.amountvalidationfailed')
      );
      return false;
    }
    return true;
  }

  private saveBulkPaymentImportIOInvoices(
    paymentImportIODTOsToUpdate: PaymentImportIODTO[]
  ) {
    paymentImportIODTOsToUpdate.forEach(p => {
      const paidAmount = p.paidAmount ?? 0;
      const invoiceAmount = p.invoiceAmount ?? 0;
      p.amountDiff = NumberUtil.parseDecimal(
        (paidAmount - invoiceAmount).toFixed(2)
      );
      p.batchNr = this.form.batchId.value;
    });

    if (this.importPaymentTypeId === ImportPaymentType.CustomerPayment) {
      this.updateCustomerPaymentImportIODTOs(paymentImportIODTOsToUpdate);
    } else {
      this.updatePaymentImportIODTOs(paymentImportIODTOsToUpdate);
    }
  }

  private updateCustomerPaymentImportIODTOs(
    paymentImportIODTOs: PaymentImportIODTO[]
  ) {
    const obj = new SaveCustomerPaymentImportIODTOModel(
      paymentImportIODTOs,
      this.form?.importDate.value,
      this.ayService.selectedAccountYearId(),
      this.form.type.value
    );

    this.importPaymentsService
      .updateCustomerPaymentImportIODTOS(obj)
      .pipe(
        tap(result => {
          if (result.success) {
            this.peformUpdateAction.emit(result);
          } else {
            this.messageboxService.error(
              this.translate.instant('core.error'),
              result.errorMessage
            );
          }
        })
      )
      .subscribe();
  }

  private updatePaymentImportIODTOs(paymentImportIODTOs: PaymentImportIODTO[]) {
    const obj = new SavePaymentImportIODTOModel(
      paymentImportIODTOs,
      this.form.importDate.value,
      this.ayService.selectedAccountYearId()
    );
    this.importPaymentsService
      .updatePaymentImportIODTOS(obj)
      .pipe(
        tap(result => {
          if (result.success) {
            this.peformUpdateAction.emit(result);
          } else {
            this.messageboxService.error(
              this.translate.instant('core.error'),
              result.errorMessage
            );
          }
        })
      )
      .subscribe();
  }

  private getMatchCode(matchCodeId: number) {
    return this.matchCodes.find(p => p.matchCodeId == matchCodeId);
  }
}

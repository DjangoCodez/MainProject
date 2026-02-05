import {
  Component,
  EventEmitter,
  OnDestroy,
  OnInit,
  Output,
  inject,
  signal,
} from '@angular/core';
import { FileUploadDialogComponent } from '@shared/components/file-upload-dialog/file-upload-dialog.component';
import { defaultFileUploadDialogData } from '@shared/components/file-upload-dialog/models/file-upload-dialog.model';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { CrudActionTypeEnum } from '@shared/enums';
import { ValidationHandler } from '@shared/handlers';
import { TermCollection } from '@shared/localization/term-types';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { ICustomerInvoiceSmallGridDTO } from '@shared/models/generated-interfaces/CustomerInvoiceDTOs';
import {
  CompanySettingType,
  EdiImportSource,
  Feature,
  SoeEntityState,
  SoeOriginStatusClassificationGroup,
  SoeReportTemplateType,
  TermGroup,
  TermGroup_EDIInvoiceStatus,
  TermGroup_EDIOrderStatus,
  TermGroup_EDIStatus,
  TermGroup_EdiMessageType,
} from '@shared/models/generated-interfaces/Enumerations';
import { IEdiEntryViewDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ProgressService } from '@shared/services/progress/progress.service';
import { BrowserUtil } from '@shared/util/browser-util';
import { IconUtil } from '@shared/util/icon-util';
import { Perform } from '@shared/util/perform.class';
import {
  SettingsUtil,
  UserCompanySettingCollection,
} from '@shared/util/settings-util';
import { SoeConfigUtil } from '@shared/util/soeconfig-util';
import { DownloadUtility } from '@shared/util/download-util';
import { FinvoiceGridFunctions } from '@shared/util/Enumerations';
import { DialogService } from '@ui/dialog/services/dialog.service';
import { ExtendedAttachedFile } from '@ui/forms/file-upload/file-upload.component';
import { GridComponent } from '@ui/grid/grid.component';
import { MenuButtonItem } from '@ui/button/menu-button/menu-button.component';
import { MessageboxService } from '@ui/dialog/services/messagebox.service';
import { ToasterService } from '@ui/toaster/services/toaster.service';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { Observable, Subject, tap } from 'rxjs';
import { take } from 'rxjs/operators';
import { SupplierService } from '../../../services/supplier.service';
import {
  EdiEntryViewDTO,
  FInvoiceModel,
  FinvoiceGridFilterDTO,
  TransferEdiStateModel,
  UpdateEdiEntryDTO,
} from '../../models/imports-invoices-finvoice.model';
import { FInvoiceAttachmentUploaderService } from '../../services/finvoice-attachment-uploader.service';
import { FInvoiceFileUploaderService } from '../../services/finvoice-file-uploader.service';
import { ImportsInvoicesFinvoiceService } from '../../services/imports-invoices-finvoice.service';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';
import { ResponseUtil } from '@shared/util/response-util';

@Component({
  selector: 'soe-imports-invoices-finvoice-grid',
  templateUrl: './imports-invoices-finvoice-grid.component.html',
  providers: [
    FlowHandlerService,
    ToolbarService,
    FInvoiceFileUploaderService,
    FInvoiceAttachmentUploaderService,
  ],
  standalone: false,
})
export class ImportsInvoicesFinvoiceGridComponent
  extends GridBaseDirective<EdiEntryViewDTO, ImportsInvoicesFinvoiceService>
  implements OnInit, OnDestroy
{
  unsubscribe = new Subject<void>();
  @Output() actionTaken = new EventEmitter<CrudActionTypeEnum>();
  service = inject(ImportsInvoicesFinvoiceService);
  coreService = inject(CoreService);
  dialogService = inject(DialogService);
  progressService = inject(ProgressService);
  toast = inject(ToasterService);
  supplierService = inject(SupplierService);
  messageboxService = inject(MessageboxService);
  validationHandler = inject(ValidationHandler);
  fileUploader = inject(FInvoiceFileUploaderService);
  attachmentUploader = inject(FInvoiceAttachmentUploaderService);
  classification = SoeEntityState.Active;
  allItemsSelection = 1;
  onlyUnHandled = false;
  performFinvoiceLoad = new Perform<IEdiEntryViewDTO[]>(this.progressService);
  performUploadFInvoices = new Perform<BackendResponse>(this.progressService);
  performInvoiceStatuses = new Perform<SmallGenericType[]>(
    this.progressService
  );
  performOrdertatuses = new Perform<SmallGenericType[]>(this.progressService);
  performTermGroupContent = new Perform<SmallGenericType[]>(
    this.progressService
  );
  performAction = new Perform<SupplierService>(this.progressService);

  performStatusName = new Perform<SmallGenericType[]>(this.progressService);
  performBillingTypes = new Perform<SmallGenericType[]>(this.progressService);
  performSuppliers = new Perform<SmallGenericType[]>(this.progressService);
  performOrdersForSupplierInvoiceEdit = new Perform<
    ICustomerInvoiceSmallGridDTO[]
  >(this.progressService);
  terms: TermCollection = {};

  public buttonFunction: MenuButtonItem[] = [];
  private invoiceStatuses: SmallGenericType[] = [];
  private statues: SmallGenericType[] = [];
  private billings: SmallGenericType[] = [];
  private customerInvoices: ICustomerInvoiceSmallGridDTO[] = [];
  private supplierList: SmallGenericType[] = [];
  actorCompanyId = 0;
  useOrder: boolean = false;
  disableFunctionButton = signal(true);

  override ngOnInit(): void {
    super.ngOnInit();
    this.startFlow(
      Feature.Economy_Import_Invoices_Finvoice,
      'Economy.Import.Finvoice',
      {
        skipInitialLoad: true,
        useLegacyToolbar: true,
        lookups: [
          this.loadInvStatuses(),
          this.loadStausNames(),
          this.loadBillingTypes(),
          this.loadSuppliers(),
          this.loadOrdersForSupplierInvoiceEdit(),
          this.loadCompanySettingsCollection(),
        ],
      }
    );
    this.loadButtonFunctions();
  }

  override loadTerms(): Observable<TermCollection> {
    return super.loadTerms([
      'core.warning',
      'core.verifyquestion',
      'common.edistransfertoinvoicevalid',
      'common.editransfertoinvoicevalid',
      'common.edistransfertoinvoiceinvalid',
      'common.editransfertoinvoiceinvalid',
      'common.invoiceswascreated',
      'common.invoicewascreated',
      'common.edistransfertoinvoicefailed',
      'common.editransfertoinvoicefailed',
      'core.warning',
      'core.verifyquestion',
      'billing.import.edi.transferposttoorder',
      'billing.import.edi.transferpoststoorder',
      'billing.import.edi.invalidtransferposttoorder',
      'billing.import.edi.invalidtransferpoststoorder',
      'billing.import.edi.posttransferedtoorder',
      'billing.import.edi.poststransferedtoorder',
      'billing.import.edi.transferposttoorderfailed',
      'billing.import.edi.transferpoststoorderfailed',
      'core.warning',
      'core.verifyquestion',
      'common.edideletevalid',
      'common.edisdeletevalid',
      'common.edideletefailed',
      'common.edisdeletefailed',
      'economy.import.finvoice.bankinttooltip',
    ]);
  }

  private loadGridData() {
    this.performFinvoiceLoad.load(
      this.service
        .getGrid(undefined, {
          classification: this.classification,
          allItemsSelection: this.allItemsSelection,
          onlyUnHandled: this.onlyUnHandled,
        })
        .pipe(
          tap(rows => {
            rows.forEach(row => {
              row.supplierNrName = row.supplierNr + ' ' + row.supplierName;
              if (!row.supplierNr) row.supplierNrName = '';

              if (row.orderNr) {
                const customerInvoice = this.customerInvoices.find(
                  x => x.invoiceNr == row.orderNr
                );
                if (customerInvoice)
                  row.customerInvoiceNumberName =
                    customerInvoice.customerInvoiceNumberName;
              }

              row.hasInvalidSupplier =
                this.supplierList.filter(s => s.id == row.supplierId).length > 0
                  ? false
                  : true;

              if (!row.invoiceNr) {
                row.errorMessage =
                  row.errorMessage != null
                    ? row.errorMessage +
                      ', ' +
                      this.terms['billing.import.edi.invoicenrmissing']
                    : this.terms['billing.import.edi.invoicenrmissing'];
              }

              if (!row.supplierId) {
                row.errorMessage =
                  row.errorMessage != null
                    ? row.errorMessage
                    : this.terms['billing.import.edi.suppliermissing'];
              }

              if (row.seqNr) row.editIcon = 'fal fa-pencil iconEdit';
            });
            this.grid.setData(rows);
            this.grid.api.sizeColumnsToFit();
          })
        )
    );
  }

  loadButtonFunctions(): void {
    this.buttonFunction.push({
      id: FinvoiceGridFunctions.Save,
      label: this.translate.instant('core.save'),
    });
    this.buttonFunction.push({
      id: FinvoiceGridFunctions.CreateSupplierInvoice,
      label: this.translate.instant('billing.import.edi.createinvoice'),
    });
    if (this.useOrder) {
      this.buttonFunction.push({
        id: FinvoiceGridFunctions.TransferToOrder,
        label: this.translate.instant('billing.import.edi.transferorderrows'),
      });
    }
    this.buttonFunction.push({
      id: FinvoiceGridFunctions.Delete,
      label: this.translate.instant('core.delete'),
    });
  }

  executeButtonFunction(buttonFunction: MenuButtonItem) {
    switch (buttonFunction.id) {
      case FinvoiceGridFunctions.Save:
        this.save();
        break;
      case FinvoiceGridFunctions.CreatePdf:
        this.createEdiPDFs();
        break;
      case FinvoiceGridFunctions.CreateSupplierInvoice:
        this.initCreateInvoice();
        break;
      case FinvoiceGridFunctions.TransferToOrder:
        this.updateEdiEntryAndTransferToOrder();
        break;
      case FinvoiceGridFunctions.Delete:
        this.deleteFinvoice();
        break;
    }
  }

  save(row?: EdiEntryViewDTO): void {
    const items: UpdateEdiEntryDTO[] = [];
    this.grid.getSelectedRows().forEach(o => {
      const item = new UpdateEdiEntryDTO();
      item.ediEntryId = o.ediEntryId;
      item.supplierId = o.supplierId;
      item.orderNr = o.orderNr;
      items.push(item);
    });

    if (row) {
      const item = new UpdateEdiEntryDTO();
      item.ediEntryId = row.ediEntryId;
      item.supplierId = row.supplierId;
      item.orderNr = row.orderNr;
      items.push(item);
    }

    if (items.length > 0) {
      this.performAction.crud(
        CrudActionTypeEnum.Save,
        this.supplierService.updateEdiEntrys(items),
        this.loadGridData.bind(this),
        (res: BackendResponse) =>
          this.errorMessages(
            this.translate.instant('common.status'),
            ResponseUtil.getErrorMessage(res) ?? ''
          )
      );
    }
  }

  createEdiPDFs(): void {
    const dict: number[] = this.grid.getSelectedIds('ediEntryId');

    this.supplierService.generateReportForFinvoice(dict).subscribe(result => {
      if (result.success) this.loadGridData();
      else {
        this.messageboxService.error(
          this.translate.instant('common.status'),
          result.errorMessage
        );
      }
    });
  }

  initCreateInvoice() {
    let nbrOfValid = 0;
    let nbrOfInvalid = 0;
    const dict: number[] = [];
    this.grid.getSelectedRows().forEach(row => {
      if (this.isOkTransferToSupplierInvoice(row)) {
        dict.push(row.ediEntryId);
        nbrOfValid = nbrOfValid + 1;
      } else {
        nbrOfInvalid = nbrOfInvalid + 1;
      }
    });

    const title = '';
    let message = '';

    if (nbrOfInvalid === 0) {
      message =
        nbrOfValid.toString() +
        ' ' +
        (nbrOfValid > 1
          ? this.terms['common.edistransfertoinvoicevalid']
          : this.terms['common.editransfertoinvoicevalid']);
    } else {
      message =
        nbrOfValid.toString() +
        ' ' +
        (nbrOfValid > 1
          ? this.terms['common.edistransfertoinvoicevalid']
          : this.terms['common.editransfertoinvoicevalid']);
      message +=
        '\n' +
        String(nbrOfInvalid) +
        ' ' +
        (nbrOfInvalid > 1
          ? this.terms['common.edistransfertoinvoiceinvalid']
          : this.terms['common.editransfertoinvoiceinvalid']);
    }
    const model = this.messageboxService.warning(title, message);
    model.afterClosed().subscribe(response => {
      if (response.result && nbrOfValid > 0) this.createInvoice(dict);
    });
  }

  isOkTransferToSupplierInvoice(item: IEdiEntryViewDTO) {
    return (
      item.invoiceNr &&
      item.invoiceNr.length > 0 &&
      !item.invoiceId &&
      item.supplierId &&
      item.supplierId > 0 &&
      item.ediMessageType == TermGroup_EdiMessageType.SupplierInvoice &&
      item.invoiceStatus == TermGroup_EDIInvoiceStatus.Unprocessed &&
      (item.status == TermGroup_EDIStatus.UnderProcessing ||
        item.status == TermGroup_EDIStatus.Processed)
    );
  }

  createInvoice(dict: number[]) {
    this.performAction.crud(
      CrudActionTypeEnum.Save,
      this.supplierService.transferEdiToInvoices(dict),
      this.loadGridData.bind(this),
      (res: BackendResponse) =>
        this.errorMessages(
          this.translate.instant('common.status'),
          ResponseUtil.getErrorMessage(res) ?? ''
        )
    );
  }

  emitActionLoad() {
    this.actionTaken.emit(CrudActionTypeEnum.Save);
  }

  updateEdiEntryAndTransferToOrder() {
    const items: UpdateEdiEntryDTO[] = [];

    this.grid.getSelectedRows().forEach(e => {
      const item = new UpdateEdiEntryDTO();
      item.ediEntryId = e.ediEntryId;
      item.supplierId = e.supplierId;
      item.orderNr = e.orderNr;
      items.push(item);
    });
    if (items.length > 0) {
      this.supplierService.updateEdiEntrys(items).subscribe(result => {
        if (result.success) {
          this.initTransferToOrder();
        } else {
          this.messageboxService.error(
            this.translate.instant('common.status'),
            result.errorMessage
          );
        }
      });
    }
  }

  initTransferToOrder() {
    const dict: number[] = [];
    let nbrOfValid = 0;
    let nbrOfInvalid = 0;
    this.grid.getSelectedRows().forEach(row => {
      if (this.isOkTransferToOrder(row)) {
        dict.push(row.ediEntryId);
        nbrOfValid = nbrOfValid + 1;
      } else {
        nbrOfInvalid = nbrOfInvalid + 1;
      }
    });

    const title = '';
    let message = '';

    if (nbrOfInvalid === 0) {
      message =
        nbrOfValid.toString() +
        ' ' +
        (nbrOfValid > 1
          ? this.terms['billing.import.edi.transferpoststoorder']
          : this.terms['billing.import.edi.transferposttoorder']);
    } else {
      message =
        nbrOfValid.toString() +
        ' ' +
        (nbrOfValid > 1
          ? this.terms['billing.import.edi.transferpoststoorder']
          : this.terms['billing.import.edi.transferposttoorder']);
      message +=
        '\n' +
        nbrOfInvalid.toString() +
        ' ' +
        (nbrOfInvalid > 1
          ? this.terms['billing.import.edi.invalidtransferpoststoorder']
          : this.terms['billing.import.edi.invalidtransferposttoorder']);
    }

    const model = this.messageboxService.question(title, message, {
      buttons: 'okCancel',
    });
    model.afterClosed().subscribe(val => {
      if (val.result) {
        this.transferToOrder(dict);
      }
    });
  }

  transferToOrder(dict: number[]) {
    this.performAction.crud(
      CrudActionTypeEnum.Save,
      this.supplierService.transferEdiToOrder(dict),
      this.loadGridData.bind(this),
      (res: BackendResponse) =>
        this.errorMessages(
          this.translate.instant('common.status'),
          ResponseUtil.getErrorMessage(res) ?? ''
        )
    );
  }

  isOkTransferToOrder(item: IEdiEntryViewDTO) {
    return (
      item.orderNr &&
      item.orderNr.length > 0 &&
      item.orderStatus == TermGroup_EDIOrderStatus.Unprocessed &&
      (item.status == TermGroup_EDIStatus.Processed ||
        item.status == TermGroup_EDIStatus.UnderProcessing)
    );
  }

  deleteFinvoice() {
    const dict: number[] = [];
    const title = '';
    let message = '';
    let nbrOfValid = 0;
    this.grid.getSelectedRows().forEach(row => {
      if (
        row.invoiceStatus == TermGroup_EDIInvoiceStatus.Unprocessed ||
        row.invoiceStatus == TermGroup_EDIInvoiceStatus.Error
      ) {
        dict.push(row.ediEntryId);
        nbrOfValid = nbrOfValid + 1;
      }
    });
    message =
      nbrOfValid.toString() +
      ' ' +
      (nbrOfValid > 1
        ? this.terms['common.edisdeletevalid']
        : this.terms['common.edideletevalid']);
    //this.performDelete(dict);
    const model = this.messageboxService.question(title, message);
    model.afterClosed().subscribe(val => {
      if (val.result) {
        this.performDelete(dict);
      }
    });
  }

  performDelete(dict: number[]) {
    const model = new TransferEdiStateModel();
    model.idsToTransfer = dict;
    model.stateTo = SoeEntityState.Deleted;
    this.performAction.crud(
      CrudActionTypeEnum.Delete,
      this.supplierService.transferEdiState(model),
      this.actionDeleteSuccess.bind(this),
      (res: BackendResponse) =>
        this.errorMessages(
          this.translate.instant('common.status'),
          ResponseUtil.getErrorMessage(res) ?? ''
        )
    );
  }

  actionDeleteSuccess(): void {
    this.actionTaken.emit(CrudActionTypeEnum.Delete);
    this.loadGridData();
  }

  onGridFilterChange(data: FinvoiceGridFilterDTO) {
    this.allItemsSelection = data.allItemsSelection;
    this.onlyUnHandled = data.showOnlyUnHandled;
    this.loadGridData();
  }

  override onGridReadyToDefine(grid: GridComponent<EdiEntryViewDTO>) {
    super.onGridReadyToDefine(grid);
    this.grid.api.updateGridOptions({
      onCellValueChanged: this.loadGridData.bind(this),
    });
    this.translate
      .get([
        'economy.import.finvoice.importdate',
        'billing.import.edi.downloadstatus',
        'billing.import.edi.invoicestatus',
        'billing.import.edi.type',
        'billing.import.edi.seqnr',
        'billing.import.edi.invoicenr',
        'common.customer.invoices.invoicedate',
        'common.customer.invoices.duedate',
        'billing.import.edi.supplier',
        'common.amount',
        'common.errormessage',
        'billing.import.edi.ordernr',
        'billing.import.edi.invoicenrmissing',
        'billing.import.edi.suppliermissing',
        'billing.import.edi.orderstatus',
        'economy.import.finvoice.showfinvoice',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.terms = { ...this.terms, ...terms };
        this.grid.enableRowSelection();
        this.grid.addColumnModified('isModified');
        this.grid.addColumnDate(
          'date',
          terms['economy.import.finvoice.importdate'],
          { flex: 1 }
        );
        this.grid.addColumnSelect(
          'status',
          terms['billing.import.edi.downloadstatus'],
          this.statues || [],
          () => {},
          { flex: 1 }
        );
        this.grid.addColumnSelect(
          'invoiceStatus',
          terms['billing.import.edi.invoicestatus'],
          this.invoiceStatuses || [],
          null,
          { flex: 1 }
        );
        if (this.useOrder) {
          this.grid.addColumnSelect(
            'orderStatus',
            terms['billing.import.edi.orderstatus'],
            this.invoiceStatuses || [],
            null,
            { flex: 1 }
          );
        }

        this.grid.addColumnSelect(
          'billingType',
          terms['billing.import.edi.type'],
          this.billings || [],
          null,
          { flex: 1 }
        );

        this.grid.addColumnNumber('seqNr', terms['billing.import.edi.seqnr'], {
          flex: 1,
          clearZero: true,
          buttonConfiguration: {
            iconPrefix: 'fal',
            iconName: 'pen',
            show: row => (row.invoiceId ? true : false),
            onClick: row => this.editSupplierInvoice(row),
          },
        });
        this.grid.addColumnText(
          'invoiceNr',
          terms['billing.import.edi.invoicenr'],
          { flex: 1 }
        );
        this.grid.addColumnDate(
          'invoiceDate',
          terms['common.customer.invoices.invoicedate'],
          { flex: 1 }
        );
        this.grid.addColumnDate(
          'dueDate',
          terms['common.customer.invoices.duedate'],
          { flex: 1 }
        );

        this.grid.addColumnAutocomplete(
          'supplierId',
          terms['billing.import.edi.supplier'],
          {
            flex: 1,
            editable: true,
            source: () => this.supplierList,
            updater: (row, value) => {
              row.supplierNr = value?.name ?? '';
              row.supplierId = value?.id ?? 0;
              row.isModified = true;
              this.save(row);
            },
            optionIdField: 'id',
            optionNameField: 'name',
            limit: 7,
            optionDisplayNameField: 'supplierNrName',
            buttonConfiguration: {
              iconPrefix: 'fal',
              iconName: 'plus',
              show: row => this.showAddSupplier(row),
              onClick: (row: IEdiEntryViewDTO) => {
                this.createNewSupplier(row);
              },
              tooltip: terms['economy.import.finvoice.createsuppliertooltip'],
            },
          }
        );

        this.grid.addColumnNumber('sum', terms['common.amount'], {
          decimals: 2,
        });

        if (this.useOrder) {
          this.grid.addColumnAutocomplete(
            'orderId',
            terms['billing.import.edi.ordernr'],
            {
              flex: 1,
              editable: true,
              source: () => this.customerInvoices,
              optionIdField: 'invoiceId',
              optionNameField: 'customerInvoiceNumberName',
              limit: 7,
              optionDisplayNameField: 'customerInvoiceNumberName',
              updater: (row, value) => {
                row.orderNr = value?.invoiceNr ?? '';
                row.isModified = true;
                this.save(row);
              },
            }
          );
        }

        this.grid.addColumnText(
          'errorMessage',
          terms['common.errormessage'],
          {}
        );
        this.grid.addColumnIcon(null, '', {
          showIcon: 'hasPdf',
          iconName: 'file-pdf',
          headerSeparator: true,
          onClick: row => {
            this.showPdf(row.ediEntryId);
          },
        });
        this.grid.addColumnIcon(null, '', {
          iconName: 'file',
          tooltip: this.terms['economy.import.finvoice.showfinvoice'],
          headerSeparator: true,
          onClick: row => {
            this.showFinvoice(row.ediEntryId);
          },
        });
        this.grid.addColumnIcon('sourceIcon', '', {
          showIcon: row => this.showSourceIcon(row),
          iconName: 'cloud-download',
          iconClass: 'success-color',
          tooltip: this.terms['economy.import.finvoice.bankinttooltip'],
          headerSeparator: true,
        });

        this.grid.selectionChanged.subscribe(rows => {
          if (rows.length > 0) {
            this.disableFunctionButton.set(false);
          } else {
            this.disableFunctionButton.set(true);
          }
        });

        super.finalizeInitGrid();
        this.loadGridData();
      });
  }

  createNewSupplier(row: IEdiEntryViewDTO) {
    this.messageboxService
      .question(
        'core.verifyquestion',
        'economy.import.finvoice.createsupplierquestion'
      )
      .afterClosed()
      .subscribe(({ result }) => {
        if (result)
          this.performAction.crud(
            CrudActionTypeEnum.Work,
            this.supplierService.saveSupplierFromFinvoice(row.ediEntryId),
            this.loadGridData.bind(this)
          );
      });
  }

  editSupplierInvoice(row: IEdiEntryViewDTO) {
    BrowserUtil.openInNewTab(
      window,
      `/soe/economy/supplier/invoice/status/default.aspx??classificationgroup=${SoeOriginStatusClassificationGroup.HandleSupplierInvoices}&invoiceId=${row.invoiceId}&invoiceNr=${row.invoiceNr}`
    );
  }

  showAddSupplier(row: IEdiEntryViewDTO): boolean {
    if (row.supplierId) return false;
    return true;
  }

  showSourceIcon(row: IEdiEntryViewDTO): boolean {
    return row.importSource === EdiImportSource.BankIntegration;
  }

  showPdf(ediEntryId: number): void {
    const ediPdfReportUrl: string =
      '/ajax/downloadReport.aspx?templatetype=' +
      SoeReportTemplateType.SymbrioEdiSupplierInvoice +
      '&edientryid=' +
      ediEntryId;
    DownloadUtility.openInNewTab(window, ediPdfReportUrl);
  }

  showFinvoice(ediEntryId: number) {
    let uri = window.location.protocol + '//' + window.location.host;
    uri =
      uri +
      '/soe/common/xslt/' +
      '?templatetype=' +
      SoeReportTemplateType.FinvoiceEdiSupplierInvoice +
      '&id=' +
      ediEntryId +
      '&c=' +
      SoeConfigUtil.actorCompanyId;
    window.open(uri, '_blank');
  }

  override createLegacyGridToolbar(): void {
    super.createLegacyGridToolbar({
      reloadOption: {
        onClick: () => this.loadGridData(),
      },
    });

    this.toolbarUtils.createLegacyGroup({
      buttons: [
        this.toolbarUtils.createLegacyButton({
          icon: IconUtil.createIcon('fal', 'download'),
          title: 'economy.import.finvoice.selectfiles',
          label: 'economy.import.finvoice.selectfiles',
          onClick: () => this.fileImport(),
          disabled: signal(false),
          hidden: signal(false),
        }),
      ],
    });

    this.toolbarUtils.createLegacyGroup({
      buttons: [
        this.toolbarUtils.createLegacyButton({
          icon: IconUtil.createIcon('fal', 'download'),
          title: 'economy.import.finvoice.selectattachments',
          label: 'economy.import.finvoice.selectattachments',
          onClick: () => this.importAttachements(),
          disabled: signal(false),
          hidden: signal(false),
        }),
      ],
    });
  }

  importAttachements() {
    const fileDialog = this.dialogService.open(
      FileUploadDialogComponent,
      defaultFileUploadDialogData(this.attachmentUploader, false)
    );

    fileDialog.afterClosed().subscribe((files: ExtendedAttachedFile[]) => {
      if (
        files instanceof Array &&
        files.filter(f => f.fileUploadStatus === 'Uploaded').length > 0
      ) {
        this.loadGridData();
      }
    });
  }

  uploadAttachements(model: FInvoiceModel): void {
    this.performUploadFInvoices.crud(
      CrudActionTypeEnum.Save,
      this.service.attacheFile(model),
      this.loadGridData.bind(this),
      (res: BackendResponse) =>
        this.errorMessages(
          this.translate.instant('common.status'),
          ResponseUtil.getErrorMessage(res) ?? ''
        )
    );
  }

  fileImport() {
    this.fileUploader.reset();
    const fileDialog = this.dialogService.open(
      FileUploadDialogComponent,
      defaultFileUploadDialogData(this.fileUploader, true)
    );

    fileDialog
      .afterClosed()
      .subscribe((files: ExtendedAttachedFile[] | null) => {
        const dataStorageIds = this.fileUploader.popDataStorageIds();
        dataStorageIds.length &&
          files?.length &&
          this.performAction.crud(
            CrudActionTypeEnum.Save,
            this.service.fileUpload(dataStorageIds),
            () => this.loadGridData()
          );
      });
  }

  loadInvStatuses(): Observable<SmallGenericType[]> {
    return this.performTermGroupContent.load$(
      this.coreService
        .getTermGroupContent(TermGroup.EDIInvoiceStatus, false, false)
        .pipe(tap(x => (this.invoiceStatuses = x)))
    );
  }

  loadStausNames(): Observable<SmallGenericType[]> {
    return this.performTermGroupContent.load$(
      this.coreService
        .getTermGroupContent(TermGroup.EDIStatus, false, false)
        .pipe(tap(x => (this.statues = x)))
    );
  }

  loadBillingTypes(): Observable<SmallGenericType[]> {
    return this.performTermGroupContent.load$(
      this.coreService
        .getTermGroupContent(TermGroup.InvoiceBillingType, false, false)
        .pipe(tap(x => (this.billings = x)))
    );
  }

  loadSuppliers(): Observable<SmallGenericType[]> {
    return this.performSuppliers.load$(
      this.supplierService
        .getSupplierDict(false, false)
        .pipe(tap(suppliers => (this.supplierList = suppliers)))
    );
  }

  loadOrdersForSupplierInvoiceEdit(): Observable<
    ICustomerInvoiceSmallGridDTO[]
  > {
    return this.performOrdersForSupplierInvoiceEdit.load$(
      this.supplierService
        .getOrdersForSupplierInvoiceEdit()
        .pipe(
          tap(customerInvoices => (this.customerInvoices = customerInvoices))
        )
    );
  }

  loadCompanySettingsCollection(): Observable<UserCompanySettingCollection> {
    const settingTypes: number[] = [
      CompanySettingType.FinvoiceUseTransferToOrder,
    ];

    return this.coreService.getCompanySettings(settingTypes).pipe(
      take(1),
      tap(x => {
        this.useOrder = SettingsUtil.getBoolCompanySetting(
          x,
          CompanySettingType.FinvoiceUseTransferToOrder
        );
      })
    );
  }

  errorMessages(title: string, text: string): void {
    this.messageboxService.error(title, text);
  }

  ngOnDestroy(): void {
    this.unsubscribe.next();
    this.unsubscribe.complete();
  }
}

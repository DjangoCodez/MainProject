import { Component, inject, OnInit, signal } from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { SupplierInvoicesArrivalHallService } from '../../services/supplier-invoices-arrival-hall.service';
import { SupplierInvoicesArrivalHallDTO } from '../../models/supplier-invoices-arrival-hall.model';
import {
  defer,
  forkJoin,
  iif,
  map,
  mergeMap,
  Observable,
  of,
  take,
  tap,
  throwError,
} from 'rxjs';
import {
  CompanySettingType,
  Feature,
  SoeEntityType,
  SoeOriginStatus,
  TermGroup,
  TermGroup_EDISourceType,
  TermGroup_SupplierInvoiceType,
  SoeEntityState,
} from '@shared/models/generated-interfaces/Enumerations';
import { AttachedFile } from '@ui/forms/file-upload/file-upload.component';
import { DialogService } from '@ui/dialog/services/dialog.service';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { TermCollection } from '@shared/localization/term-types';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { SupplierInvoiceUtilityService } from '@features/economy/shared/supplier-invoice/domain-services/supplier-invoice-utility.service';
import { IRowNode } from 'ag-grid-community';
import { FileUploader } from '@features/economy/import-connect/models/file-uploader';
import { FileUploadDialogComponent } from '@shared/components/file-upload-dialog/file-upload-dialog.component';
import { defaultFileUploadDialogData } from '@shared/components/file-upload-dialog/models/file-upload-dialog.model';
import { Perform } from '@shared/util/perform.class';
import { CrudActionTypeEnum } from '@shared/enums';
import { SupplierInvoiceSharedService } from '@features/economy/shared/supplier-invoice/services/supplier-invoice-shared.service';
import { ContextMenuBuilder } from '@ui/grid/menu-items/context-menu-builder';
import { IconProp } from '@fortawesome/fontawesome-svg-core';
import { SupplierService } from '@features/economy/services/supplier.service';
import { CurrenciesService } from '@features/economy/currencies/services/currencies.service';
import { MenuButtonItem } from '@ui/button/menu-button/menu-button.component';
import { SupplierGridButtonFunctions } from '@shared/util/Enumerations';
import { MessageboxService } from '@ui/dialog/services/messagebox.service';
import { EdiService } from '@features/billing/edi/services/edi.service';
import { AttestationGroupsService } from '@features/economy/attestation-groups/services/attestation-groups.service';
import { AttestWorkFlowHeadDTO } from '@features/economy/attestation-groups/models/attestation-groups.model';
import { SettingsUtil } from '@shared/util/settings-util';
import { IMessageboxComponentResponse } from '@ui/dialog/models/messagebox';
import { ISaveAttestWorkFlowForInvoicesModel } from '@shared/models/generated-interfaces/CoreModels';
import { AddInvoiceToAttestFlowDialogData } from '@features/economy/shared/add-invoice-to-attest-flow-dialog/models/add-invoice-to-attest-flow-dialog-data.model';
import { AddInvoiceToAttestFlowDialogComponent } from '@features/economy/shared/add-invoice-to-attest-flow-dialog/components/add-invoice-to-attest-flow-dialog/add-invoice-to-attest-flow-dialog.component';
import { AddInvoiceToAttestFlowInvoice } from '@features/economy/shared/add-invoice-to-attest-flow-dialog/models/add-invoice-to-attest-flow-invoice.model';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';
import { ResponseUtil } from '@shared/util/response-util';

@Component({
  selector: 'soe-supplier-invoices-arrival-hall-grid',
  templateUrl: './supplier-invoices-arrival-hall-grid.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class SupplierInvoicesArrivalHallGridComponent
  extends GridBaseDirective<
    SupplierInvoicesArrivalHallDTO,
    SupplierInvoicesArrivalHallService
  >
  implements OnInit
{
  service = inject(SupplierInvoicesArrivalHallService);
  private readonly dialogService = inject(DialogService);
  private readonly coreService = inject(CoreService);
  private readonly supplierInvoiceUtilityService = inject(
    SupplierInvoiceUtilityService
  );
  private readonly supplierService = inject(SupplierInvoiceSharedService);
  private readonly economySupplierService = inject(SupplierService);
  private readonly currenciesService = inject(CurrenciesService);
  private readonly messageboxService = inject(MessageboxService);
  private readonly ediService = inject(EdiService);
  private readonly attestationService = inject(AttestationGroupsService);

  private invoiceSources: SmallGenericType[] = [];
  private billingTypes: SmallGenericType[] = [];
  private invoiceStates: SmallGenericType[] = [];
  private currencyCodes: SmallGenericType[] = [];
  private attestGroups: SmallGenericType[] = [];
  private hasCurrencyPermission: boolean = false;
  private hasAttestFlowPermission: boolean = false;
  private hasAttestAdminPermission: boolean = false;
  private hasAttestAddPermission: boolean = false;
  private hasEDIPermission: boolean = false;
  private userIdNeededWithTotalAmount: number = 0;
  private totalAmountWhenUserRequired: number = 0;

  performService = new Perform<any>(this.progressService);
  performAttestCheck = new Perform<
    Array<{ flow: AttestWorkFlowHeadDTO | null }>
  >(this.progressService);
  protected buttonFunctions = signal<MenuButtonItem[]>([]);

  constructor(public flowHandler: FlowHandlerService) {
    super();
  }

  ngOnInit(): void {
    this.startFlow(
      Feature.Economy_Supplier_Invoice_Incoming,
      'Economy.Supplier.Invoice.Incoming',
      {
        additionalReadPermissions: [
          Feature.Economy_Supplier_Invoice_Status_Foreign,
          Feature.Economy_Supplier_Invoice_AttestFlow,
        ],
        additionalModifyPermissions: [
          Feature.Billing_Import_EDI,
          Feature.Economy_Supplier_Invoice_AttestFlow_Admin,
          Feature.Economy_Supplier_Invoice_AttestFlow_Add,
        ],
        lookups: [
          this.loadInvoiceSources(),
          this.loadInvoiceBillingTypes(),
          this.loadInvoiceState(),
          this.loadCurrencies(),
          this.loadAttestGroups(),
        ],
      }
    );
  }

  override onPermissionsLoaded() {
    super.onPermissionsLoaded();
    this.hasCurrencyPermission = this.flowHandler.hasReadAccess(
      Feature.Economy_Supplier_Invoice_Status_Foreign
    );
    this.hasAttestFlowPermission = this.flowHandler.hasReadAccess(
      Feature.Economy_Supplier_Invoice_AttestFlow
    );
    this.hasAttestAdminPermission = this.flowHandler.hasModifyAccess(
      Feature.Economy_Supplier_Invoice_AttestFlow_Admin
    );
    this.hasAttestAddPermission = this.flowHandler.hasModifyAccess(
      Feature.Economy_Supplier_Invoice_AttestFlow_Add
    );
    this.hasEDIPermission = this.flowHandler.hasModifyAccess(
      Feature.Billing_Import_EDI
    );
  }

  override createGridToolbar(): void {
    super.createGridToolbar();

    this.toolbarService.createItemGroup({
      items: [
        this.toolbarService.createToolbarButton('uploadimages', {
          iconName: signal('upload'),
          caption: signal('economy.supplier.invoice.uploadimages'),
          tooltip: signal('economy.supplier.invoice.uploadimages'),
          onAction: () => this.uploadImages(),
        }),
      ],
    });
  }

  override loadTerms(): Observable<TermCollection> {
    return super.loadTerms([
      'common.created',
      'common.state',
      'common.type',
      'core.source',
      'core.edit',
      'economy.supplier.invoice.invoicenr',
      'economy.supplier.supplier.suppliernr.grid',
      'economy.supplier.supplier.supplier',
      'economy.supplier.invoice.description',
      'economy.supplier.invoice.amountexvat',
      'economy.supplier.invoice.amountincvat',
      'economy.supplier.invoice.invoicedate',
      'economy.supplier.invoice.duedate',
      'economy.supplier.invoice.paymentblocked',
      'economy.supplier.invoice.underinvestigation',
      'economy.supplier.invoice.currencycode',
      'economy.supplier.invoice.foreignamount',
      'economy.supplier.invoice.attestgroup',
      'core.transfertodeleted',
      'core.addtoattestflow',
      'core.startattestflow',
      'core.createpdffromedi',
      'common.postinvalid',
      'common.postsinvalid',
      'core.warning',
      'core.error',
      'core.verifyquestion',
      'common.createpdfvalid',
      'common.createpdfsvalid',
      'core.continue',
      'common.edisdeleteinvalid',
      'common.edideleteinvalid',
      'common.edisdeletevalid',
      'common.edideletevalid',
      'economy.supplier.invoice.startattestflowinvalid',
      'economy.supplier.invoice.existingattestflowmessage',
      'economy.supplier.invoice.sendattestmessage',
      'economy.supplier.invoice.addedtoattestflowsuccess',
      'economy.supplier.invoice.addedtoattestflownotsuccess',
      'economy.supplier.invoice.itemsunderlimitwarning',
      'economy.supplier.invoice.addtoattestflow',
    ]);
  }

  override onGridReadyToDefine(
    grid: GridComponent<SupplierInvoicesArrivalHallDTO>
  ) {
    super.onGridReadyToDefine(grid);
    this.setupContextMenu(grid);
    this.loadTerms()
      .pipe(take(1))
      .subscribe(() => {
        this.grid.enableRowSelection();
        this.grid.addColumnDate('created', this.terms['common.created'], {
          enableHiding: true,
          flex: 1,
        });
        this.grid.addColumnSelect(
          'invoiceSource',
          this.terms['core.source'],
          this.invoiceSources,
          undefined,
          {
            enableHiding: true,
            flex: 1,
          }
        );
        this.grid.addColumnSelect(
          'billingTypeId',
          this.terms['common.type'],
          this.billingTypes,
          undefined,
          {
            enableHiding: true,
            flex: 1,
          }
        );
        this.grid.addColumnText(
          'invoiceNr',
          this.terms['economy.supplier.invoice.invoicenr'],
          {
            flex: 1,
          }
        );
        this.grid.addColumnText(
          'supplierNr',
          this.terms['economy.supplier.supplier.suppliernr.grid'],
          {
            enableHiding: true,
            flex: 1,
          }
        );
        this.grid.addColumnText(
          'supplierName',
          this.terms['economy.supplier.supplier.supplier'],
          {
            flex: 1,
          }
        );
        this.grid.addColumnText(
          'internalText',
          this.terms['economy.supplier.invoice.description'],
          {
            enableHiding: true,
            flex: 1,
          }
        );

        if (this.hasCurrencyPermission) {
          this.grid.addColumnSelect(
            'sysCurrencyId',
            this.terms['economy.supplier.invoice.currencycode'],
            this.currencyCodes,
            undefined,
            {
              flex: 1,
              enableHiding: true,
              hide: true,
            }
          );

          this.grid.addColumnNumber(
            'totalAmountCurrency',
            this.terms['economy.supplier.invoice.foreignamount'],
            {
              flex: 1,
              decimals: 2,
              enableHiding: true,
              hide: true,
            }
          );
        }

        this.grid.addColumnNumber(
          'totalAmountExcludingVat',
          this.terms['economy.supplier.invoice.amountexvat'],
          {
            flex: 1,
            decimals: 2,
          }
        );
        this.grid.addColumnNumber(
          'totalAmount',
          this.terms['economy.supplier.invoice.amountincvat'],
          {
            enableHiding: true,
            hide: true,
            flex: 1,
            decimals: 2,
          }
        );
        this.grid.addColumnDate(
          'invoiceDate',
          this.terms['economy.supplier.invoice.invoicedate'],
          {
            flex: 1,
          }
        );
        this.grid.addColumnDate(
          'dueDate',
          this.terms['economy.supplier.invoice.duedate'],
          {
            flex: 1,
          }
        );
        this.grid.addColumnSelect(
          'invoiceState',
          this.terms['common.state'],
          this.invoiceStates,
          undefined,
          {
            enableHiding: true,
            flex: 1,
          }
        );

        if (this.hasAttestFlowPermission) {
          this.grid.addColumnSelect(
            'attestGroupId',
            this.terms['economy.supplier.invoice.attestgroup'],
            this.attestGroups,
            undefined,
            {
              flex: 1,
              enableHiding: true,
              hide: true,
            }
          );
        }

        this.grid.addColumnIcon(null, '...', {
          tooltip: this.terms['economy.supplier.invoice.paymentblocked'],
          enableHiding: false,
          suppressExport: true,
          iconPrefix: 'fal',
          iconName: 'lock-alt',
          showIcon: row => row.blockPayment,
        });

        this.grid.addColumnIcon(null, '...', {
          tooltip: this.terms['economy.supplier.invoice.underinvestigation'],
          enableHiding: false,
          suppressExport: true,
          iconPrefix: 'fal',
          iconName: 'eye',
          showIcon: row => row.underInvestigation,
        });

        this.grid.setRowClassCallback(params => {
          if (params?.data?.isOverdue) return 'error-background-color';
          return undefined;
        });

        this.grid.addColumnIconEdit({
          tooltip: this.terms['core.edit'],
          onClick: row => {
            this.edit(row);
          },
        });

        super.finalizeInitGrid();
      });
  }

  override onFinished(): void {
    super.onFinished();
    this.setSplitButtonFunctions();
  }

  private setSplitButtonFunctions(): void {
    const buttonFunctions: MenuButtonItem[] = [];

    if (this.flowHandler.modifyPermission()) {
      buttonFunctions.push({
        id: SupplierGridButtonFunctions.RemoveDraftOrEdi,
        label: this.terms['core.transfertodeleted'],
        icon: 'trash',
      });

      if (this.hasAttestAdminPermission || this.hasAttestAddPermission) {
        buttonFunctions.push({
          id: SupplierGridButtonFunctions.AddToAttestFlow,
          label: this.terms['core.addtoattestflow'],
          icon: 'check-circle',
        });
        buttonFunctions.push({
          id: SupplierGridButtonFunctions.StartAttestFlow,
          label: this.terms['core.startattestflow'],
          icon: 'play-circle',
        });
      }

      if (this.hasEDIPermission) {
        buttonFunctions.push({
          id: SupplierGridButtonFunctions.CreatePDF,
          label: this.terms['core.createpdffromedi'],
          icon: 'file-pdf',
        });
      }
    }

    this.buttonFunctions.set(buttonFunctions);
  }

  protected functionSelected(item: MenuButtonItem): void {
    switch (item.id) {
      case SupplierGridButtonFunctions.RemoveDraftOrEdi:
        this.removeDraftOrEdi();
        break;
      case SupplierGridButtonFunctions.AddToAttestFlow:
        this.addToAttestFlow();
        break;
      case SupplierGridButtonFunctions.StartAttestFlow:
        this.startAttestFlow();
        break;
      case SupplierGridButtonFunctions.CreatePDF:
        this.createPDF();
        break;
    }
  }

  private setupContextMenu(
    grid: GridComponent<SupplierInvoicesArrivalHallDTO>
  ) {
    grid.setContextMenuCallback((data, params, builder) => {
      this.addBlockPaymentContextMenuItem(builder, params.node, data);
      this.addUnderInvestigationContextMenuItem(builder, params.node, data);
      builder.addIconButton({
        caption: 'economy.supplier.invoice.supplieroverview',
        disabled: !data?.supplierId,
        icon: ['fal', 'calculator-alt'],
        action: () =>
          this.supplierInvoiceUtilityService.openSupplierCentralInNewTab(
            data?.supplierId
          ),
      });
      return builder.build();
    });
  }

  private addBlockPaymentContextMenuItem(
    builder: ContextMenuBuilder<SupplierInvoicesArrivalHallDTO>,
    node: IRowNode<SupplierInvoicesArrivalHallDTO> | null,
    data?: SupplierInvoicesArrivalHallDTO
  ) {
    const caption = data?.blockPayment
      ? 'economy.supplier.invoice.unblockforpayment'
      : 'economy.supplier.invoice.paymentblock';
    const icon: IconProp = data?.blockPayment
      ? ['fal', 'unlock']
      : ['fal', 'lock'];
    const disabled = !data?.invoiceId;

    return builder.addIconButton({
      caption: caption,
      icon: icon,
      disabled: disabled,
      action: () =>
        data?.invoiceId &&
        this.supplierInvoiceUtilityService.showBlockForPaymentDialog(
          !data.blockPayment,
          blockPayment => this.updateNode(node, { blockPayment }),
          data.invoiceId
        ),
    });
  }

  private addUnderInvestigationContextMenuItem(
    builder: ContextMenuBuilder<SupplierInvoicesArrivalHallDTO>,
    node: IRowNode<SupplierInvoicesArrivalHallDTO> | null,
    data?: SupplierInvoicesArrivalHallDTO
  ) {
    const caption = data?.underInvestigation
      ? 'economy.supplier.invoice.notunderinvestigation'
      : 'economy.supplier.invoice.underinvestigation';
    const icon: IconProp = data?.underInvestigation
      ? ['fal', 'eye-slash']
      : ['fal', 'eye'];
    const disabled = !data?.invoiceId && !data?.ediEntryId;

    return builder.addIconButton({
      caption: caption,
      icon: icon,
      disabled: disabled,
      action: () =>
        (data?.invoiceId || data?.ediEntryId) &&
        this.supplierInvoiceUtilityService.showUnderInvestigationDialog(
          !data?.underInvestigation,
          underInvestigation =>
            this.updateNode(node, {
              underInvestigation,
            }),
          data.invoiceId,
          data.ediEntryId
        ),
    });
  }

  private loadInvoiceSources(): Observable<SmallGenericType[]> {
    return this.coreService
      .getTermGroupContent(TermGroup.SupplierInvoiceSource, false, false, true)
      .pipe(tap(res => (this.invoiceSources = res)));
  }

  private loadInvoiceBillingTypes(): Observable<SmallGenericType[]> {
    return this.coreService
      .getTermGroupContent(TermGroup.InvoiceBillingType, false, false, true)
      .pipe(tap(res => (this.billingTypes = res)));
  }

  private loadInvoiceState(): Observable<SmallGenericType[]> {
    return this.coreService
      .getTermGroupContent(TermGroup.SupplierInvoiceState, false, false, true)
      .pipe(tap(res => (this.invoiceStates = res)));
  }

  //Actions
  private updateNode(
    node: IRowNode<SupplierInvoicesArrivalHallDTO> | null,
    partial: Partial<SupplierInvoicesArrivalHallDTO>
  ) {
    if (!node?.data) return;

    node.setData({
      ...node.data,
      ...partial,
    } as SupplierInvoicesArrivalHallDTO);
  }

  private uploadFiles() {
    const fileUploader = new FileUploader(this.coreService);

    fileUploader.uploadFile = (file: AttachedFile) => {
      if (!file.binaryContent) return of({ success: false });

      return this.coreService
        .uploadInvoiceFileByEntityType(
          SoeEntityType.SupplierInvoice,
          file.binaryContent,
          file.name || ''
        )
        .pipe(
          tap((res: BackendResponse) => {
            if (res.success) {
              fileUploader.addFileLookup(res, file);
            }
          })
        );
    };

    return this.dialogService
      .open(
        FileUploadDialogComponent,
        defaultFileUploadDialogData(fileUploader, true)
      )
      .afterClosed()
      .pipe(
        map(() => {
          return fileUploader.fileLookup.files.map(f => f.dataStorageId);
        })
      );
  }

  private uploadImages() {
    this.uploadFiles()
      .pipe(
        tap((dataStorageIds: number[] | undefined) => {
          if (!dataStorageIds || dataStorageIds.length === 0) return;

          this.performService.crud(
            CrudActionTypeEnum.Save,
            this.supplierService.saveInvoicesForUploadedImages(dataStorageIds),
            () => this.refreshGrid()
          );
        })
      )
      .subscribe();
  }

  private loadCurrencies(): Observable<SmallGenericType[]> {
    return iif(
      () => this.hasCurrencyPermission,
      this.currenciesService.getSysCurrenciesDict(false, true).pipe(
        tap(currencies => {
          this.currencyCodes = currencies;
        })
      ),
      of([])
    );
  }

  private loadAttestGroups(): Observable<SmallGenericType[]> {
    return iif(
      () => this.hasAttestFlowPermission,
      this.economySupplierService.getAttestWorkFlowGroupsDict(false).pipe(
        tap(groups => {
          this.attestGroups = groups;
        })
      ),
      of([])
    );
  }

  override loadCompanySettings(): Observable<void> {
    return this.coreService
      .getCompanySettings([
        CompanySettingType.SupplierInvoiceAttestFlowUserIdRequired,
        CompanySettingType.SupplierInvoiceAttestFlowAmountWhenUserIdIsRequired,
      ])
      .pipe(
        tap(settings => {
          this.userIdNeededWithTotalAmount = SettingsUtil.getIntCompanySetting(
            settings,
            CompanySettingType.SupplierInvoiceAttestFlowUserIdRequired
          );
          this.totalAmountWhenUserRequired = SettingsUtil.getIntCompanySetting(
            settings,
            CompanySettingType.SupplierInvoiceAttestFlowAmountWhenUserIdIsRequired
          );
        }),
        map(() => undefined)
      );
  }

  private removeDraftOrEdi(): void {
    const selectedRows = this.grid.getSelectedRows();

    const draftInvoices: SupplierInvoicesArrivalHallDTO[] = [];
    const ediEntries: SupplierInvoicesArrivalHallDTO[] = [];
    const invalidItems: SupplierInvoicesArrivalHallDTO[] = [];

    selectedRows.forEach(row => {
      if (row.ediEntryId > 0 && row.ediType > 0) {
        ediEntries.push(row);
      } else if (
        row.invoiceId > 0 &&
        (row.originStatus === SoeOriginStatus.Draft ||
          row.supplierInvoiceType === TermGroup_SupplierInvoiceType.Uploaded)
      ) {
        draftInvoices.push(row);
      } else {
        invalidItems.push(row);
      }
    });

    const invalidMessage =
      invalidItems.length > 1
        ? this.terms['common.edisdeleteinvalid']
        : this.terms['common.edideleteinvalid'];

    const validCount = draftInvoices.length + ediEntries.length;

    if (validCount === 0) {
      const title = this.terms['core.error'];
      let text = '';
      text += invalidItems.length.toString() + ' ' + invalidMessage + '<br>';
      this.messageboxService.error(title, text);
    } else {
      const title = this.terms['core.verifyquestion'];

      const validMessage =
        validCount > 1
          ? this.terms['common.edisdeletevalid']
          : this.terms['common.edideletevalid'];
      let text = '';

      if (invalidItems.length > 0) {
        text += invalidItems.length.toString() + ' ' + invalidMessage + '<br>';
      }
      text += validCount.toString() + ' ' + validMessage + '<br>';
      text += this.terms['core.continue'];

      const dialog = this.messageboxService.question(title, text, {
        buttons: 'okCancel',
      });

      dialog.afterClosed().subscribe(response => {
        if (response.result) {
          this.performDelete(draftInvoices, ediEntries);
        }
      });
    }
  }

  private performDelete(
    draftInvoices: SupplierInvoicesArrivalHallDTO[],
    ediEntries: SupplierInvoicesArrivalHallDTO[]
  ): void {
    const deleteOperation$ = defer(() => {
      const ediEntryIds = ediEntries.map(item => item.ediEntryId);
      const draftInvoiceIds = draftInvoices.map(item => item.invoiceId);

      const hasEdi = ediEntryIds.length > 0;
      const hasDrafts = draftInvoiceIds.length > 0;

      if (!hasEdi && !hasDrafts) {
        return of({ success: true } as BackendResponse);
      }

      if (!hasEdi) {
        return this.service.deleteDraftInvoices(draftInvoiceIds);
      }

      return this.ediService
        .changeEdiState(ediEntryIds, SoeEntityState.Deleted)
        .pipe(
          mergeMap(ediResponse => {
            if (!ediResponse.success) {
              return throwError(() => ediResponse);
            }

            if (hasDrafts) {
              return this.service.deleteDraftInvoices(draftInvoiceIds);
            }

            return of(ediResponse);
          })
        );
    });

    this.performService.crud(CrudActionTypeEnum.Delete, deleteOperation$, () =>
      this.refreshGrid()
    );
  }

  private createPDF(): void {
    const selectedRows = this.grid.getSelectedRows();

    const validatedItems: SupplierInvoicesArrivalHallDTO[] = [];
    const invalidItems: SupplierInvoicesArrivalHallDTO[] = [];

    selectedRows.forEach(row => {
      if (
        row.ediEntryId > 0 &&
        !row.hasPDF &&
        row.ediType === TermGroup_EDISourceType.EDI
      ) {
        validatedItems.push(row);
      } else {
        invalidItems.push(row);
      }
    });

    const invalidMessage =
      invalidItems.length > 1
        ? this.terms['common.postsinvalid']
        : this.terms['common.postinvalid'];
    if (validatedItems.length === 0) {
      const title = this.terms['core.error'];

      let text = '';
      text += invalidItems.length.toString() + ' ' + invalidMessage + '<br>';

      this.messageboxService.error(title, text);
    } else {
      const title = this.terms['core.verifyquestion'];
      const validMessage =
        validatedItems.length > 1
          ? this.terms['common.createpdfsvalid']
          : this.terms['common.createpdfvalid'];
      let text = '';

      if (invalidItems.length > 0) {
        text += invalidItems.length.toString() + ' ' + invalidMessage + '<br>';
      }

      text += validatedItems.length.toString() + ' ' + validMessage + '<br>';
      text += this.terms['core.continue'];

      const dialog = this.messageboxService.question(title, text, {
        buttons: 'okCancel',
      });

      dialog.afterClosed().subscribe(response => {
        if (response.result) {
          const ediEntries = validatedItems.map(item => item.ediEntryId);
          this.createPDFs(ediEntries);
        }
      });
    }
  }

  private createPDFs(ediEntries: number[]): void {
    this.performService.crud(
      CrudActionTypeEnum.Work,
      this.ediService.generateReportForEdi(ediEntries),
      () => this.refreshGrid()
    );
  }

  private addToAttestFlow(): void {
    const selectedRows = this.grid.getSelectedRows();
    const invoicesToAttest = selectedRows.filter(row => row.invoiceId > 0);

    if (invoicesToAttest.length === 0) {
      this.messageboxService.error(
        this.terms['core.error'],
        this.terms['economy.supplier.invoice.startattestflowinvalid'],
        { buttons: 'ok' }
      );
    } else {
      const selectedInvoices = invoicesToAttest.map(row => {
        const i: AddInvoiceToAttestFlowInvoice = {
          invoiceId: row.invoiceId,
          totalAmount: row.totalAmount,
        };
        return i;
      });

      const data: AddInvoiceToAttestFlowDialogData = {
        title: this.terms['economy.supplier.invoice.addtoattestflow'],
        size: 'lg',
        supplierInvoices: selectedInvoices,
      };

      this.dialogService
        .open(AddInvoiceToAttestFlowDialogComponent, data)
        .afterClosed()
        .subscribe(result => {
          if (result && result.success) {
            const message =
              this.terms[
                'economy.supplier.invoice.addedtoattestflowsuccess'
              ].format(result.affectedInvoiceCount.toString()) + '.\\n';

            this.messageboxService.success(
              this.terms['core.success'],
              message,
              { buttons: 'ok' }
            );
            this.refreshGrid();
          } else if (result && !result.success) {
            this.messageboxService.error(
              this.terms['core.error'],
              this.terms[
                'economy.supplier.invoice.addedtoattestflownotsuccess'
              ],
              { buttons: 'ok' }
            );
          }
        });
    }
  }

  private startAttestFlow(): void {
    const selectedRows = this.grid.getSelectedRows();

    let invoicesToAttest = selectedRows.filter(row => row.invoiceId > 0);

    if (!invoicesToAttest.length) {
      this.messageboxService.error(
        this.terms['core.error'],
        this.terms['economy.supplier.invoice.startattestflowinvalid'],
        { buttons: 'ok' }
      );
    } else if (
      this.userIdNeededWithTotalAmount > 0 &&
      this.totalAmountWhenUserRequired > 0
    ) {
      const originalCount = invoicesToAttest.length;
      invoicesToAttest = invoicesToAttest.filter(
        row => row.totalAmount <= this.totalAmountWhenUserRequired
      );

      if (originalCount !== invoicesToAttest.length) {
        this.messageboxService
          .question(
            this.terms['core.warning'],
            this.terms['economy.supplier.invoice.itemsunderlimitwarning'],
            { buttons: 'okCancel' }
          )
          .afterClosed()
          .subscribe((response: IMessageboxComponentResponse) => {
            if (response.result) {
              if (invoicesToAttest.length > 0) {
                this.checkForExistingAttestFlows(invoicesToAttest);
              }
            }
          });
      } else {
        this.checkForExistingAttestFlows(invoicesToAttest);
      }
    } else {
      this.checkForExistingAttestFlows(invoicesToAttest);
    }
  }

  private checkForExistingAttestFlows(
    invoicesToAttest: SupplierInvoicesArrivalHallDTO[]
  ): void {
    const checkPromises = invoicesToAttest.map(invoice =>
      this.economySupplierService
        .getAttestWorkFlowHeadFromInvoiceId(
          invoice.invoiceId,
          false,
          false,
          false,
          false
        )
        .pipe(map(flow => ({ flow })))
    );

    this.performAttestCheck
      .load$(checkPromises.length > 0 ? forkJoin(checkPromises) : of([]), {
        showDialogDelay: 500,
      })
      .subscribe(results => {
        const existingFlowIds: number[] = [];
        results.forEach(result => {
          if (result.flow && result.flow.attestWorkFlowHeadId) {
            existingFlowIds.push(result.flow.attestWorkFlowHeadId);
          }
        });

        if (existingFlowIds.length > 0) {
          this.messageboxService
            .question(
              this.terms['core.verifyquestion'],
              this.terms['economy.supplier.invoice.existingattestflowmessage'],
              { buttons: 'yesNo' }
            )
            .afterClosed()
            .subscribe((response: IMessageboxComponentResponse) => {
              if (response.result) {
                this.performService.crud(
                  CrudActionTypeEnum.Delete,
                  this.attestationService.deleteAttestWorkFlows(
                    existingFlowIds
                  ),
                  () => {
                    this.executeStartAttestFlow(invoicesToAttest);
                  }
                );
              }
            });
        } else {
          this.executeStartAttestFlow(invoicesToAttest);
        }
      });
  }

  private executeStartAttestFlow(
    invoices: SupplierInvoicesArrivalHallDTO[]
  ): void {
    const invoiceIds = invoices.map(inv => inv.invoiceId);

    this.messageboxService
      .question(
        this.terms['core.startattestflow'],
        this.terms['economy.supplier.invoice.sendattestmessage'] + '?',
        { buttons: 'yesNo' }
      )
      .afterClosed()
      .subscribe((dialogResponse: IMessageboxComponentResponse) => {
        const model: ISaveAttestWorkFlowForInvoicesModel = {
          idsToTransfer: invoiceIds,
          sendMessage: dialogResponse.result ?? false,
        };

        this.performService.crud(
          CrudActionTypeEnum.Save,
          this.attestationService.saveAttestWorkFlowForInvoices(model),
          (response: BackendResponse) => {
            let message = '';
            const entityId = ResponseUtil.getEntityId(response);
            const numberValue = ResponseUtil.getNumberValue(response);
            if (entityId && entityId > 0) {
              message =
                this.terms[
                  'economy.supplier.invoice.addedtoattestflowsuccess'
                ].replace('{0}', entityId.toString()) + '\n';
            }
            if (numberValue && numberValue > 0) {
              message +=
                this.terms[
                  'economy.supplier.invoice.addedtoattestflownotsuccess'
                ].replace('{0}', numberValue.toString()) + '\n';
            }

            if (message) {
              this.messageboxService.information('', message, {
                buttons: 'ok',
              });
            }
            this.refreshGrid();
          }
        );
      });
  }
}

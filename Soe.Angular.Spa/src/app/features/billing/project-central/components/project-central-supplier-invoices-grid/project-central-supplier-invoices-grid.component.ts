import {
  AfterViewInit,
  Component,
  inject,
  OnInit,
  signal,
} from '@angular/core';
import { SupplierService } from '@features/economy/services/supplier.service';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { IInvoicesForProjectCentralModel } from '@shared/models/generated-interfaces/EconomyModels';
import {
  Feature,
  SoeModule,
  SoeOriginStatusClassification,
  SoeOriginType,
  SoeReportTemplateType,
  TermGroup_AttestEntity,
  TermGroup_EDISourceType,
} from '@shared/models/generated-interfaces/Enumerations';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { Observable, of, tap } from 'rxjs';
import { ProjectCentralDataService } from '../../services/project-central-data.service';
import {
  ProjectCentralSummaryDTO,
  SupplierInvoiceGridDTO,
} from '../../models/project-central.model';
import { BrowserUtil } from '@shared/util/browser-util';
import { SoeConfigUtil } from '@shared/util/soeconfig-util';
import { TermCollection } from '@shared/localization/term-types';

@Component({
  selector: 'soe-project-central-supplier-invoices-grid',
  standalone: false,
  templateUrl: './project-central-supplier-invoices-grid.component.html',
  providers: [FlowHandlerService, ToolbarService],
})
export class ProjectCentralSupplierInvoicesGridComponent
  extends GridBaseDirective<SupplierInvoiceGridDTO>
  implements OnInit, AfterViewInit
{
  readonly projectCentralDataService = inject(ProjectCentralDataService);
  supplierService = inject(SupplierService);
  gridDataParams!: ProjectCentralSummaryDTO;
  attestStates: any[] = [];
  isDownloadButtonDisabled = signal(true);
  isLoaded = false;

  // Terms
  noAttestStateTerm!: string;
  attestRejectedTerm!: string;

  filteredTotal = 0;
  filteredTotalLinkedToProject = 0;
  filteredTotalLinkedToOrder = 0;
  filteredTotalLinkedToOrderSale = 0;
  filteredTotalLinkedCost = 0;
  surchargePercentage = 0;

  //Permissions
  readPermission: boolean = false;
  modifyPermission: boolean = false;
  hasAttestFlowPermission: boolean = false;

  ngOnInit(): void {
    this.startFlow(
      Feature.Economy_Supplier_Invoice_Invoices,
      'Billing.Project.Central.SupplierInvoices',
      {
        skipInitialLoad: true,
        lookups: [this.loadAttestStates()],
        additionalReadPermissions: [
          Feature.Economy_Supplier_Invoice_AttestFlow,
          Feature.Economy_Supplier_Invoice_Invoices_Edit,
        ],
        additionalModifyPermissions: [
          Feature.Economy_Supplier_Invoice_Invoices_Edit,
          Feature.Economy_Supplier_Invoice_AttestFlow,
        ],
      }
    );
  }
  ngAfterViewInit(): void {
    this.projectCentralDataService.projectCentralData$.subscribe(
      (data: ProjectCentralSummaryDTO) => {
        this.gridDataParams = data;
      }
    );
  }

  override onTabActivated(): void {
    if (!this.isLoaded) {
      this.performLoadData.load(
        this.loadData().pipe(
          tap(rows => {
            this.grid.setData(rows);
            this.isLoaded = true;
          })
        )
      );
    }
  }

  override onPermissionsLoaded(): void {
    this.readPermission =
      this.flowHandler.readPermission() || this.flowHandler.modifyPermission();
    this.modifyPermission =
      this.flowHandler.hasModifyAccess(
        Feature.Economy_Supplier_Invoice_Invoices_Edit
      ) ||
      this.flowHandler.hasReadAccess(
        Feature.Economy_Supplier_Invoice_Invoices_Edit
      );
    this.hasAttestFlowPermission =
      this.flowHandler.hasReadAccess(
        Feature.Economy_Supplier_Invoice_AttestFlow
      ) ||
      this.flowHandler.hasModifyAccess(
        Feature.Economy_Supplier_Invoice_AttestFlow
      );
  }

  override loadTerms(): Observable<TermCollection> {
    const keys: string[] = [
      'core.edit',
      'common.imported',
      'economy.supplier.invoice.seqnr',
      'economy.supplier.invoice.invoicenr',
      'economy.supplier.invoice.invoicetype',
      'common.tracerows.status',
      'economy.supplier.supplier.suppliernr.grid',
      'economy.supplier.supplier.suppliername.grid',
      'economy.supplier.invoice.amountexvat',
      'economy.supplier.invoice.amountincvat',
      'economy.supplier.invoice.remainingamount',
      'economy.supplier.invoice.invoicedate',
      'economy.supplier.invoice.duedate',
      'common.customer.invoices.paydate',
      'economy.supplier.invoice.attest',
      'economy.supplier.invoice.attestname',
      'economy.supplier.invoice.noatteststate',
      'economy.supplier.invoice.attestrejected',
      'economy.supplier.invoice.sumlinkedtoorder',
      'economy.supplier.invoice.sumlinkedtoproject',
      'economy.supplier.invoice.openpdf',
      'common.reason',
      'economy.supplier.invoice.paidshort',
      'economy.supplier.invoice.paidbutnotcheckedshort',
      'economy.supplier.invoice.partlypaidshort',
      'economy.supplier.invoice.unpaidshort',
      'core.attestflowregistered',
      'economy.supplier.invoice.sumlinkedtoordercost',
    ];

    return super.loadTerms(keys).pipe(
      tap(terms => {
        this.noAttestStateTerm =
          terms['economy.supplier.invoice.noatteststate'];
        this.attestRejectedTerm =
          terms['economy.supplier.invoice.attestrejected'];
      })
    );
  }

  override createGridToolbar(): void {
    super.createGridToolbar();

    this.toolbarService.createItemGroup({
      items: [
        this.toolbarService.createToolbarButton('downloadinvoiceimages', {
          iconName: signal('download'),
          caption: signal('economy.supplier.invoice.downloadinvoiceimages'),
          tooltip: signal('economy.supplier.invoice.downloadinvoiceimages'),
          disabled: this.isDownloadButtonDisabled,
          onAction: () => this.downloadInvoiceImages(),
        }),
      ],
    });
  }

  override loadData(): Observable<SupplierInvoiceGridDTO[]> {
    const data = this.gridDataParams;

    if (this.gridDataParams.projectId) {
      const model: IInvoicesForProjectCentralModel = {
        classification: SoeOriginStatusClassification.SupplierInvoicesAll,
        originType: SoeOriginType.SupplierInvoice,
        projectId: data.projectId ?? 0,
        loadChildProjects: data.includeChildProjects || false,
        invoiceIds: [],
      };

      return this.supplierService
        .getSupplierInvoicesForProjectCentral(model)
        .pipe(
          tap(invoices => {
            invoices.forEach(i => this.setPaymentStatusIcon(i));

            this.summarizeFiltered(invoices);
          })
        );
    } else {
      return of([]);
    }
  }

  onGridReadyToDefine(grid: GridComponent<SupplierInvoiceGridDTO>): void {
    super.onGridReadyToDefine(grid);
    this.grid.enableRowSelection();

    this.grid.addColumnNumber(
      'seqNr',
      this.terms['economy.supplier.invoice.seqnr'],
      {
        alignLeft: true,
        flex: 1,
        formatAsText: true,
      }
    );
    this.grid.addColumnText(
      'invoiceNr',
      this.terms['economy.supplier.invoice.invoicenr'],
      { flex: 1 }
    );
    this.grid.addColumnSelect(
      'billingTypeId',
      this.terms['economy.supplier.invoice.invoicetype'],
      [],
      undefined,
      {
        dropDownIdLabel: 'billingTypeId',
        dropDownValueLabel: 'billingTypeName',
        flex: 1,
        enableHiding: true,
        hide: true,
      }
    );
    this.grid.addColumnSelect(
      'status',
      this.terms['common.tracerows.status'],
      [],
      undefined,
      {
        dropDownIdLabel: 'status',
        dropDownValueLabel: 'statusName',
        enableHiding: true,
        hide: true,
      }
    );
    this.grid.addColumnText(
      'supplierNr',
      this.terms['economy.supplier.supplier.suppliernr.grid'],
      {
        enableHiding: true,
        enableGrouping: true,
        flex: 1,
        hide: true,
      }
    );
    this.grid.addColumnText(
      'supplierName',
      this.terms['economy.supplier.supplier.suppliername.grid'],
      {
        enableGrouping: true,
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
    this.grid.addColumnNumber(
      'totalAmountExVat',
      this.terms['economy.supplier.invoice.amountexvat'],
      {
        enableHiding: true,
        flex: 1,
        decimals: 2,
      }
    );
    this.grid.addColumnNumber(
      'totalAmount',
      this.terms['economy.supplier.invoice.amountincvat'],
      {
        enableHiding: true,
        decimals: 2,
        flex: 1,
        hide: true,
      }
    );
    this.grid.addColumnNumber(
      'payAmount',
      this.terms['economy.supplier.invoice.remainingamount'],
      {
        enableHiding: true,
        decimals: 2,
        flex: 1,
        hide: true,
      }
    );

    this.grid.addColumnDate(
      'invoiceDate',
      this.terms['economy.supplier.invoice.invoicedate'],
      { flex: 1 }
    );
    this.grid.addColumnDate(
      'dueDate',
      this.terms['economy.supplier.invoice.duedate'],
      {
        flex: 1,
      }
    );
    this.grid.addColumnDate(
      'payDate',
      this.terms['common.customer.invoices.paydate'],
      { flex: 1, hide: true }
    );

    if (this.hasAttestFlowPermission) {
      this.grid.addColumnSelect(
        'attestStateId',
        this.terms['economy.supplier.invoice.attest'],
        [],
        undefined,
        {
          dropDownIdLabel: 'attestStateId',
          dropDownValueLabel: 'attestStateName',
          flex: 1,
          hide: true,
          enableHiding: true,
        }
      );
      this.grid.addColumnText(
        'currentAttestUserName',
        this.terms['economy.supplier.invoice.attestname'],
        {
          enableHiding: true,
          hide: true,
          flex: 1,
        }
      );
    }

    this.grid.addColumnNumber(
      'projectInvoicedAmount',
      this.terms['economy.supplier.invoice.sumlinkedtoordercost'],
      {
        enableHiding: true,
        decimals: 2,
        flex: 1,
      }
    );
    this.grid.addColumnNumber(
      'projectInvoicedSalesAmount',
      this.terms['economy.supplier.invoice.sumlinkedtoorder'],
      {
        enableHiding: true,
        decimals: 2,
        flex: 1,
        hide: true,
      }
    );
    this.grid.addColumnNumber(
      'projectAmount',
      this.terms['economy.supplier.invoice.sumlinkedtoproject'],
      {
        enableHiding: true,
        decimals: 2,
        flex: 1,
      }
    );
    this.grid.addColumnIcon('', '', {
      iconName: 'comment-dots',
      showIcon: row => this.showAttestCommentIcon(row),
      onClick: row => this.openAttestCommentDialog(row),
    });
    this.grid.addColumnIcon('blockIcon', '', {
      tooltipField: 'blockReason',
      onClick: row => this.showBlockReason(row),
    });
    this.grid.addColumnShape('paymentStatusIcon', '', {
      alignCenter: true,
      enableHiding: false,
      shape: 'circle',
      colorField: 'paymentStatusColor',
      tooltipField: 'paymentStatusTooltip',
      pinned: 'right',
    });
    this.grid.addColumnIcon('pdfIcon', '', {
      tooltip: this.terms['economy.supplier.invoice.openpdf'],
      iconName: 'file-search',
      showIcon: row => this.showPdfIcon(row),
      onClick: row => this.openPicture(row),
    });

    if (this.modifyPermission) {
      this.grid.addColumnIconEdit({
        tooltip: this.terms['core.edit'],
        onClick: row => this.openSupplierInvoiceInNewTab(row),
      });
    }
    this.grid.columns.forEach(c => {
      if (c.field == 'dueDate') {
        c.cellClassRules = {
          'disabled-grid-row-background-color': row =>
            row.data?.useClosedStyle || false,
          'error-background-color': row => row.data?.isOverdue || false,
          'warning-background-color': row => row.data?.blockPayment || false,
        };
      } else {
        c.cellClassRules = {
          'disabled-grid-row-background-color': row =>
            row.data?.useClosedStyle || false,
          'warning-background-color': row => row.data?.blockPayment || false,
        };
      }
    });

    this.grid.selectionChanged.subscribe(data =>
      this.onRowSelectionChanged(data)
    );

    this.grid.filterModified.subscribe(() => {
      this.summarizeFiltered(this.grid.getFilteredRows());
    });

    super.finalizeInitGrid();
  }

  private getSelectedIds(): number[] {
    return this.grid
      .getSelectedRows()
      .filter(r => r.hasPDF)
      .map(r => r.supplierInvoiceId);
  }

  private downloadInvoiceImages(): void {
    const imageUrl: string = `/ajax/downloadReport.aspx?templatetype=${SoeReportTemplateType.SupplierInvoiceImage}&c=${SoeConfigUtil.actorCompanyId}&invoiceIds=${this.getSelectedIds()}`;
    window.open(imageUrl, '_blank');
  }

  private showAttestCommentIcon(row: SupplierInvoiceGridDTO) {
    return row.hasAttestComment;
  }

  private openAttestCommentDialog(row: SupplierInvoiceGridDTO): void {
    console.log('Open attest comment dialog for row:', row);
  }

  private showBlockReason(row: SupplierInvoiceGridDTO): void {
    console.log('Show block reason for row:', row);
  }

  private showPdfIcon(row: SupplierInvoiceGridDTO): boolean {
    if (row.hasPDF || row.ediType === TermGroup_EDISourceType.Finvoice) {
      return true;
    }
    return false;
  }

  openPicture(row: SupplierInvoiceGridDTO): void {
    console.log('Open picture for row:', row);
  }

  setPaymentStatusIcon(invoice: SupplierInvoiceGridDTO) {
    if (invoice.fullyPaid) {
      if (invoice.noOfPaymentRows == invoice.noOfCheckedPaymentRows) {
        invoice.paymentStatusColor = 'green';
        invoice.paymentStatusTooltip =
          this.terms['economy.supplier.invoice.paidshort'];
      } else {
        invoice.paymentStatusColor = 'orange';
        invoice.paymentStatusTooltip =
          this.terms['economy.supplier.invoice.paidbutnotcheckedshort'];
      }
    } else if (invoice.paidAmount !== 0 && !invoice.fullyPaid) {
      invoice.paymentStatusColor = 'yellow';
      invoice['paymentStatusTooltip'] =
        this.terms['economy.supplier.invoice.partlypaidshort'];
    } else {
      invoice.paymentStatusColor = 'red';
      invoice['paymentStatusTooltip'] =
        this.terms['economy.supplier.invoice.unpaidshort'];
    }

    if (invoice.blockPayment) invoice.blockIcon = 'fal fa-lock-alt errorColor';

    if (invoice.hasPDF) invoice.pdfIcon = 'fal fa-file-pdf';
  }

  openSupplierInvoiceInNewTab(row: SupplierInvoiceGridDTO) {
    const url = `/soe/economy/supplier/invoice/status/default.aspx?invoiceId=${row.supplierInvoiceId}&invoiceNr=${row.invoiceNr}`;
    BrowserUtil.openInNewTab(window, url);
  }

  loadAttestStates() {
    return this.supplierService
      .getAttestStates(
        TermGroup_AttestEntity.SupplierInvoice,
        SoeModule.Economy,
        false
      )
      .pipe(
        tap(attestStates => {
          this.attestStates = [];
          this.attestStates.push({
            value: this.noAttestStateTerm,
            label: -100,
          });
          this.attestStates.push({
            value: this.attestRejectedTerm,
            label: -200,
          });
          attestStates.forEach((state: any) => {
            this.attestStates.push({
              value: state.name,
              label: state.attestStateId,
            });
          });
        })
      );
  }

  onRowSelectionChanged(rows: SupplierInvoiceGridDTO[]): void {
    this.isDownloadButtonDisabled.set(rows.length === 0);
  }

  private summarizeFiltered(rows: SupplierInvoiceGridDTO[]) {
    this.filteredTotal = 0;
    this.filteredTotalLinkedToProject = 0;
    this.filteredTotalLinkedToOrder = 0;
    this.filteredTotalLinkedToOrderSale = 0;

    rows.forEach((r: SupplierInvoiceGridDTO) => {
      this.filteredTotal += r.totalAmountExVat;
      this.filteredTotalLinkedToProject += r.projectAmount;
      this.filteredTotalLinkedToOrder += r.projectInvoicedAmount;
      this.filteredTotalLinkedToOrderSale += r.projectInvoicedSalesAmount;
    });

    this.surchargePercentage =
      this.filteredTotalLinkedToOrder > 0
        ? (this.filteredTotalLinkedToOrderSale /
            this.filteredTotalLinkedToOrder) *
            100 -
          100
        : 0;
    this.surchargePercentage = Math.round(this.surchargePercentage * 100) / 100;
    this.filteredTotalLinkedCost =
      this.filteredTotalLinkedToOrder + this.filteredTotalLinkedToProject;
  }
}

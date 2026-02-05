import { Component, inject, OnInit, signal } from '@angular/core';
import { ProjectService } from '@features/billing/project/services/project.service';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import {
  Feature,
  SoeOriginType,
  SoeReportTemplateType,
  TermGroup,
} from '@shared/models/generated-interfaces/Enumerations';
import { IProjectProductRowDTO } from '@shared/models/generated-interfaces/ProjectProductRowDTO';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { map, Observable, of, tap } from 'rxjs';
import {
  IProjectProductGridRow,
  ProjectCentralSummaryDTO,
} from '../../models/project-central.model';
import { BrowserUtil } from '@shared/util/browser-util';
import { SoeConfigUtil } from '@shared/util/soeconfig-util';
import { TermCollection } from '@shared/localization/term-types';
import { ProjectCentralDataService } from '../../services/project-central-data.service';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import { ToolbarSelectAction } from '@ui/toolbar/toolbar-select/toolbar-select.component';
import { ToolbarDatepickerAction } from '@ui/toolbar/toolbar-datepicker/toolbar-datepicker.component';
import { ToolbarCheckboxAction } from '@ui/toolbar/toolbar-checkbox/toolbar-checkbox.component';

@Component({
  selector: 'soe-project-central-product-rows',
  standalone: false,
  templateUrl: './project-central-product-rows.component.html',
  providers: [FlowHandlerService, ToolbarService],
})
export class ProjectCentralProductRowsComponent
  extends GridBaseDirective<IProjectProductGridRow>
  implements OnInit
{
  readonly projectCentralDataService = inject(ProjectCentralDataService);
  projectService = inject(ProjectService);
  coreService = inject(CoreService);
  gridDataParams!: ProjectCentralSummaryDTO;
  selectedOriginType: SoeOriginType = SoeOriginType.Order;

  //Permissions
  modifyPermission: boolean = false;
  hasOrderPermission: boolean = false;
  hasInvoicePermission: boolean = false;
  hasOrderRowsPermission: boolean = false;
  hasInvoiceRowsPermission: boolean = false;
  hasSalesPricePermission: boolean = false;
  hasPurchasePricePermission: boolean = false;

  originTypes: ISmallGenericType[] = [];

  toolbarIncludeChildProjectsChecked = signal(false);

  ngOnInit(): void {
    super.ngOnInit();
    this.startFlow(
      Feature.Billing_Project_List,
      'Billing.Projects.Project.ProductRows',
      {
        skipInitialLoad: true,
        additionalModifyPermissions: [
          Feature.Billing_Order_Orders_Edit_ProductRows,
          Feature.Billing_Invoice_Invoices_Edit_ProductRows,
          Feature.Billing_Product_Products_ShowSalesPrice,
          Feature.Billing_Product_Products_ShowPurchasePrice,
          Feature.Billing_Invoice_Invoices_Edit,
          Feature.Billing_Order_Orders_Edit,
        ],
        lookups: [this.loadOriginTypes()],
      }
    );
    this.projectCentralDataService.projectCentralData$.subscribe(data => {
      this.gridDataParams = data;
      // Set initial value of checkbox in toolbar
      this.toolbarIncludeChildProjectsChecked.set(
        this.gridDataParams.includeChildProjects
      );
      this.refreshGrid();
    });
  }

  override loadData(
    id?: number,
    additionalProps?: any
  ): Observable<IProjectProductGridRow[]> {
    if (!this.gridDataParams || !this.gridDataParams.projectId) return of([]);
    const fromDate = this.gridDataParams.fromDate
      ? this.gridDataParams.fromDate.toDateString()
      : '';
    const toDate = this.gridDataParams.toDate
      ? this.gridDataParams.toDate.toDateString()
      : '';
    return this.projectService
      .getProductRows(
        this.gridDataParams.projectId,
        this.selectedOriginType,
        this.gridDataParams.includeChildProjects,
        fromDate,
        toDate
      )
      .pipe(
        map(rows => {
          return rows.map(row => {
            const productRow = <IProjectProductGridRow>{
              ...row,
              customerInvoiceNumber: row.invoiceNumber ?? '',
              customerInvoiceId: row.invoiceId ?? 0,
              associatedId: row.invoiceId ?? 0,
              invoiceNr: row.invoiceNumber ?? '',
              orderNumber: row.invoiceNumber ?? '',
              orderDate: row.invoiceDate ?? undefined,
            };
            this.setRowTypeIcon(productRow);
            return productRow;
          });
        })
      );
  }

  override createGridToolbar(): void {
    super.createGridToolbar();

    this.toolbarService.createItemGroup({
      items: [
        this.toolbarService.createToolbarSelect('selectOriginType', {
          labelKey: signal('common.type'),
          optionIdField: signal('id'),
          optionNameField: signal('name'),
          items: signal(this.originTypes),
          initialSelectedId: signal(this.selectedOriginType),
          onValueChanged: event => {
            this.selectedOriginType = (event as ToolbarSelectAction).value;
            this.refreshGrid();
          },
        }),
      ],
      alignLeft: true,
    });

    this.toolbarService.createItemGroup({
      items: [
        this.toolbarService.createToolbarDatepicker('fromDate', {
          labelKey: signal('common.from'),
          initialDate: signal(this.gridDataParams.fromDate),
          onValueChanged: event => {
            this.gridDataParams.fromDate = (
              event as ToolbarDatepickerAction
            )?.value;
          },
        }),
      ],
      alignLeft: true,
    });

    this.toolbarService.createItemGroup({
      items: [
        this.toolbarService.createToolbarDatepicker('toDate', {
          labelKey: signal('common.to'),
          initialDate: signal(this.gridDataParams.toDate),
          onValueChanged: event => {
            this.gridDataParams.toDate = (
              event as ToolbarDatepickerAction
            )?.value;
          },
        }),
      ],
      alignLeft: true,
    });

    this.toolbarService.createItemGroup({
      items: [
        this.toolbarService.createToolbarCheckbox('includeChildProjects', {
          labelKey: signal('billing.project.central.inclchildprojects'),
          checked: this.toolbarIncludeChildProjectsChecked,
          onValueChanged: event => {
            this.gridDataParams.includeChildProjects = (
              event as ToolbarCheckboxAction
            )?.value;
          },
        }),
      ],
      alignLeft: true,
    });

    this.toolbarService.createItemGroup({
      items: [
        this.toolbarService.createToolbarButton('search', {
          caption: signal(this.terms['core.search']),
          tooltip: signal(this.terms['core.search']),
          onAction: () => this.refreshGrid(),
        }),
      ],
      alignLeft: true,
    });
  }

  override onPermissionsLoaded(): void {
    super.onPermissionsLoaded();
    this.modifyPermission = this.flowHandler.modifyPermission();
    // this.modifyPermission = x[SoeConfigUtil.Fea]
    this.hasOrderPermission = this.flowHandler.hasModifyAccess(
      Feature.Billing_Order_Orders_Edit_ProductRows
    );
    this.hasInvoiceRowsPermission = this.flowHandler.hasModifyAccess(
      Feature.Billing_Invoice_Invoices_Edit_ProductRows
    );
    this.hasSalesPricePermission = this.flowHandler.hasModifyAccess(
      Feature.Billing_Product_Products_ShowSalesPrice
    );
    this.hasPurchasePricePermission = this.flowHandler.hasModifyAccess(
      Feature.Billing_Product_Products_ShowPurchasePrice
    );
    this.hasOrderPermission = this.flowHandler.hasModifyAccess(
      Feature.Billing_Order_Orders_Edit
    );
    this.hasInvoicePermission = this.flowHandler.hasModifyAccess(
      Feature.Billing_Invoice_Invoices_Edit
    );
  }

  override loadTerms(): Observable<TermCollection> {
    const keys: string[] = [
      'billing.productrows.discount',
      'billing.productrows.purchaseprice',
      'billing.productrows.purchasepricesum',
      'billing.productrows.marginalincomeratio.short',
      'billing.productrows.marginalincome.short',
      'billing.productrows.productunit',
      'billing.productrows.quantity',
      'common.date',
      'common.description',
      'common.productnr',
      'common.createdby',
      'common.created',
      'common.modified',
      'common.modifiedby',
      'billing.productrows.amount',
      'billing.productrows.sumamount',
      'common.customer.invoices.rowstatus',
      'billing.project.project',
      'billing.order.projectnr',
      'billing.order.ordernr',
      'common.invoicenr',
      'common.customer.invoices.articlename',
      'billing.product.materialcode',
      'billing.product.productgroup',
      'common.customer.customer.wholesellername',
      'economy.supplier.supplier.supplier',
      'common.customer.invoices.invoicedate',
      'billing.order.invoicedate',
      'economy.supplier.invoice.openpdf',
      'core.search',
    ];
    return super.loadTerms(keys);
  }

  override onGridReadyToDefine(
    grid: GridComponent<IProjectProductGridRow>
  ): void {
    super.onGridReadyToDefine(grid);

    this.grid.addColumnIcon('rowTypeIcon', '', {
      useIconFromField: true,
      pinned: 'left',
      width: 50,
      enableHiding: false,
    });

    this.grid.addColumnText(
      'projectName',
      this.terms['billing.project.project'],
      {
        flex: 1,
        enableGrouping: true,
        enableHiding: true,
      }
    );
    this.grid.addColumnText(
      'projectNumber',
      this.terms['billing.order.projectnr'],
      {
        flex: 1,
        enableGrouping: true,
        enableHiding: true,
      }
    );

    if (this.hasInvoicePermission) {
      this.grid.addColumnText(
        'customerInvoiceNumber',
        this.terms['common.invoicenr'],
        {
          flex: 1,
          enableGrouping: true,
          enableHiding: true,
          hide: true,
          buttonConfiguration: {
            iconPrefix: 'fal',
            iconName: 'pen',
            onClick: row => this.openOrderInvoice(row),
          },
        }
      );
    }
    if (this.hasOrderPermission) {
      this.grid.addColumnText(
        'orderNumber',
        this.terms['billing.order.ordernr'],
        {
          flex: 1,
          enableGrouping: true,
          enableHiding: true,
          buttonConfiguration: {
            iconPrefix: 'fal',
            iconName: 'pen',
            onClick: row => this.openOrderInvoice(row),
          },
        }
      );
    }

    this.grid.addColumnDate(
      'invoiceDate',
      this.terms['common.customer.invoices.invoicedate'],
      { flex: 1, enableGrouping: true, enableHiding: true }
    );
    this.grid.addColumnDate(
      'orderDate',
      this.terms['billing.order.invoicedate'],
      { flex: 1, enableGrouping: true, enableHiding: true }
    );
    this.grid.addColumnText('articleNumber', this.terms['common.productnr'], {
      enableGrouping: true,
      enableHiding: true,
      flex: 1,
    });
    this.grid.addColumnText(
      'articleName',
      this.terms['common.customer.invoices.articlename'],
      {
        enableGrouping: true,
        enableHiding: true,
        flex: 1,
      }
    );
    this.grid.addColumnText(
      'productGroupName',
      this.terms['billing.product.productgroup'],
      {
        enableGrouping: true,
        enableHiding: true,
        flex: 1,
      }
    );
    this.grid.addColumnText(
      'materialCode',
      this.terms['billing.product.materialcode'],
      {
        enableGrouping: true,
        enableHiding: true,
        flex: 1,
      }
    );
    this.grid.addColumnText('description', this.terms['common.description'], {
      enableGrouping: true,
      enableHiding: true,
      flex: 1,
    });
    this.grid.addColumnText(
      'supplierName',
      this.terms['economy.supplier.supplier.supplier'],
      {
        enableGrouping: true,
        enableHiding: true,
        flex: 1,
      }
    );

    this.grid.addColumnShape(
      'attestState',
      this.terms['common.customer.invoices.rowstatus'],
      {
        shape: 'circle',
        colorField: 'attestColor',
        tooltipField: 'attestState',
        width: 50,
        flex: 1,
      }
    );

    this.grid.addColumnText(
      'unit',
      this.terms['billing.productrows.productunit'],
      {
        enableGrouping: true,
        enableHiding: true,
        flex: 1,
      }
    );
    this.grid.addColumnNumber(
      'quantity',
      this.terms['billing.productrows.quantity'],
      {
        decimals: 2,
        maxDecimals: 4,
        aggFuncOnGrouping: 'sum',
        flex: 1,
      }
    );
    if (this.hasPurchasePricePermission) {
      this.grid.addColumnNumber(
        'purchasePrice',
        this.terms['billing.productrows.purchaseprice'],
        {
          decimals: 2,
          maxDecimals: 4,
          flex: 1,
        }
      );
      this.grid.addColumnNumber(
        'purchaseAmount',
        this.terms['billing.productrows.purchasepricesum'],
        {
          decimals: 2,
          maxDecimals: 4,
          aggFuncOnGrouping: 'sum',
          flex: 1,
        }
      );
      this.grid.addColumnIcon(null, '', {
        iconName: 'file-pdf',
        tooltip: this.terms['economy.supplier.invoice.openpdf'],
        width: 22,
        suppressFilter: true,
        headerSeparator: true,
        showIcon: row => this.showIcon(row),
        onClick: (row: IProjectProductRowDTO) => this.showPdf(row),
      });
    }

    if (this.hasSalesPricePermission) {
      this.grid.addColumnNumber(
        'salesPrice',
        this.terms['billing.productrows.amount'],
        {
          decimals: 2,
          maxDecimals: 4,
          flex: 1,
        }
      );
      this.grid.addColumnNumber(
        'salesAmount',
        this.terms['billing.productrows.sumamount'],
        {
          decimals: 2,
          maxDecimals: 4,
          aggFuncOnGrouping: 'sum',
          flex: 1,
        }
      );
    }

    if (this.hasSalesPricePermission && this.hasPurchasePricePermission) {
      this.grid.addColumnNumber(
        'marginalIncome',
        this.terms['billing.productrows.marginalincome.short'],
        {
          decimals: 2,
          maxDecimals: 4,
          aggFuncOnGrouping: 'sum',
          flex: 1,
        }
      );
      this.grid.addColumnNumber(
        'marginalIncomeRatio',
        this.terms['billing.productrows.marginalincomeratio.short'],
        {
          decimals: 2,
          maxDecimals: 4,
          flex: 1,
        }
      );
      this.grid.addColumnNumber(
        'discountPercent',
        this.terms['billing.productrows.discount'],
        {
          decimals: 2,
          maxDecimals: 4,
          flex: 1,
        }
      );
    }

    this.grid.addColumnDate('date', this.terms['common.date'], {
      enableGrouping: true,
      enableHiding: true,
    });
    this.grid.addColumnDate('created', this.terms['common.created'], {
      enableGrouping: true,
      enableHiding: true,
      hide: true,
    });

    this.grid.addColumnText('createdBy', this.terms['common.createdby'], {
      enableGrouping: true,
      enableHiding: true,
      hide: true,
    });

    this.grid.addColumnDate('modified', this.terms['common.modified'], {
      enableGrouping: true,
      enableHiding: true,
      hide: true,
    });
    this.grid.addColumnText('modifiedBy', this.terms['common.modifiedby'], {
      enableGrouping: true,
      enableHiding: true,
      hide: true,
    });

    this.grid.useGrouping({
      stickyGroupTotalRow: 'bottom',
    });
    this.grid.enableGroupTotalFooter();
    super.finalizeInitGrid();
  }

  private loadOriginTypes() {
    this.selectedOriginType = SoeOriginType.Order;
    return this.coreService
      .getTermGroupContent(TermGroup.OriginType, false, false)
      .pipe(
        tap(types => {
          this.originTypes = [];
          types.forEach(t => {
            if (
              (t.id === SoeOriginType.Order && this.hasOrderPermission) ||
              (t.id === SoeOriginType.CustomerInvoice &&
                this.hasInvoicePermission)
            )
              this.originTypes.push(t);
          });
        })
      );
  }

  openOrderInvoice(row: IProjectProductGridRow): void {
    let url = '';
    if (this.selectedOriginType === SoeOriginType.Order) {
      url = `/soe/billing/order/status/default.aspx?invoiceId=${row.customerInvoiceId}&invoiceNr=${row.invoiceNr}`;
    } else if (this.selectedOriginType === SoeOriginType.CustomerInvoice) {
      url = `/soe/billing/invoice/status/?invoiceId=${row.customerInvoiceId}&invoiceNr=${row.invoiceNr}`;
    }

    BrowserUtil.openInNewTab(window, url);
  }

  private showPdf(row: IProjectProductRowDTO): void {
    if (row.supplierInvoiceId ?? 0 > 0) {
      const imageUrl = `/ajax/downloadReport.aspx?templatetype=${SoeReportTemplateType.SupplierInvoiceImage} &invoiceId=${row.supplierInvoiceId}&c=${SoeConfigUtil.actorCompanyId}`;
      window.open(imageUrl, '_blank');
    }
  }

  private setRowTypeIcon(row: IProjectProductGridRow) {
    if (row.isTimeProjectRow) {
      row.rowTypeIcon = 'clock';
    } else {
      row.rowTypeIcon = 'box-alt';
    }
  }

  private showIcon(row: IProjectProductRowDTO): boolean {
    if (!row) return false;
    return (row.ediEntryId ?? 0) > 0 || (row.supplierInvoiceId ?? 0) > 0;
  }
}

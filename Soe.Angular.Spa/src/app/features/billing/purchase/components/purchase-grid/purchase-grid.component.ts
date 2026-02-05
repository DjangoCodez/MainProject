import { Component, OnDestroy, OnInit, inject, signal } from '@angular/core';
import { SelectEmailDialogComponent } from '@shared/components/select-email-dialog/components/select-email-dialog/select-email-dialog.component';
import {
  GetReportsForTypesModel,
  SelectEmailDialogCloseData,
  SelectEmailDialogData,
} from '@shared/components/select-email-dialog/models/select-email-dialog.model';
import { SelectReportDialogComponent } from '@shared/components/select-report-dialog/components/select-report-dialog/select-report-dialog.component';
import {
  GetPurchasePrintUrlModel,
  SelectReportDialogCloseData,
  SelectReportDialogData,
} from '@shared/components/select-report-dialog/models/select-report-dialog.model';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { SmallGenericType } from '@shared/models/generic-type.model';
import {
  CompanySettingType,
  EmailTemplateType,
  Feature,
  PurchaseDeliveryStatus,
  SoeOriginStatus,
  SoeReportTemplateType,
  SoeStatusIcon,
} from '@shared/models/generated-interfaces/Enumerations';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import { IPurchaseGridDTO } from '@shared/models/generated-interfaces/PurchaseDTOs';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ProgressService } from '@shared/services/progress/progress.service';
import { ReportService } from '@shared/services/report.service';
import { BrowserUtil } from '@shared/util/browser-util';
import { Perform } from '@shared/util/perform.class';
import { SettingsUtil } from '@shared/util/settings-util';
import { StringUtil } from '@shared/util/string-util';
import { DialogService } from '@ui/dialog/services/dialog.service';
import { GridComponent } from '@ui/grid/grid.component';
import { MenuButtonItem } from '@ui/button/menu-button/menu-button.component';
import { ToasterService } from '@ui/toaster/services/toaster.service';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { Observable, Subscription, of } from 'rxjs';
import { map, take, tap } from 'rxjs/operators';
import {
  PurchaseFilterDTO,
  PurchaseStatusTextDTO,
  SendPurchaseEmail,
} from '../../models/purchase.model';
import { PurchaseService } from '../../services/purchase.service';

export enum FunctionType {
  Print = 1,
  SendAsEmail = 2,
}

@Component({
  selector: 'soe-purchase-grid',
  templateUrl: './purchase-grid.component.html',
  styleUrls: ['./purchase-grid.component.scss'],
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class PurchaseGridComponent
  extends GridBaseDirective<IPurchaseGridDTO, PurchaseService>
  implements OnInit, OnDestroy
{
  sendPurchasesAsEmailSubscription!: Subscription;
  selectEmailDialogSubscription!: Subscription;
  defaultEmailTemplatePurchase!: number;
  defaultReportTemplatePurchase!: number;

  selectedPurchaseStatusIds: number[] = [];
  allItemsSelection = 0;
  hasDisabledFunctionButton = signal(true);

  isGridReady = false;
  isFilterLoad = false;

  service = inject(PurchaseService);
  coreService = inject(CoreService);
  progressService = inject(ProgressService);
  dialogServiceV2 = inject(DialogService);
  reportService = inject(ReportService);
  toasterService = inject(ToasterService);

  performLoad = new Perform<any>(this.progressService);
  menuList: MenuButtonItem[] = [];
  purchaseStatus: ISmallGenericType[] = [];

  filter!: PurchaseFilterDTO;
  purchaseStatusText!: PurchaseStatusTextDTO;

  ngOnInit() {
    super.ngOnInit();

    this.service.gridFilter$.subscribe(filter => {
      this.filter = filter;
    });

    this.service.purchaseStatusText$.subscribe(purchaseStatusText => {
      this.purchaseStatusText = purchaseStatusText;
    });

    this.startFlow(
      Feature.Billing_Purchase_Purchase_List,
      'Billing.Purchase.Purchase',
      {
        lookups: [this.loadPurchaseStatus()],
      }
    );
  }

  ngOnDestroy() {
    this.sendPurchasesAsEmailSubscription.unsubscribe();
    this.selectEmailDialogSubscription.unsubscribe();
  }

  override loadCompanySettings(): Observable<void> {
    const settingTypes: number[] = [
      CompanySettingType.BillingDefaultEmailTemplatePurchase,
      CompanySettingType.BillingDefaultPurchaseOrderReportTemplate,
    ];
    return this.performLoad.load$(
      this.coreService.getCompanySettings(settingTypes).pipe(
        tap(x => {
          this.defaultEmailTemplatePurchase = SettingsUtil.getIntCompanySetting(
            x,
            CompanySettingType.BillingDefaultEmailTemplatePurchase
          );

          this.defaultReportTemplatePurchase =
            SettingsUtil.getIntCompanySetting(
              x,
              CompanySettingType.BillingDefaultPurchaseOrderReportTemplate
            );
        })
      )
    );
  }

  buildFunctionList(): Observable<unknown> {
    this.menuList = [];
    return of(
      this.menuList.push(
        {
          id: FunctionType.Print,
          label: this.translate.instant('core.print'),
        },
        {
          id: FunctionType.SendAsEmail,
          label: this.translate.instant('billing.purchase.sendasemail'),
        }
      )
    );
  }

  override onFinished(): void {
    this.buildFunctionList();
    this.isGridReady = true;

    const filterDTO = new PurchaseFilterDTO();
    filterDTO.allItemsSelection = 0;
    filterDTO.selectedPurchaseStatusIds = [
      SoeOriginStatus.Origin,
      SoeOriginStatus.PurchaseDone,
      SoeOriginStatus.PurchaseSent,
      SoeOriginStatus.PurchaseAccepted,
    ];
    this.service.setFilterSubject(filterDTO);
    if (this.isFilterLoad) this.refreshGrid();
  }

  peformAction(selected: MenuButtonItem): void {
    switch (selected.id) {
      case FunctionType.Print:
        this.print();
        break;
      case FunctionType.SendAsEmail:
        this.sendAsEmail();
        break;
    }
  }

  print() {
    this.openSelectReport();
  }
  sendAsEmail() {
    this.openSelectEmail();
  }

  openSelectEmail() {
    const model = new GetReportsForTypesModel(
      [SoeReportTemplateType.PurchaseOrder],
      true,
      false,
      undefined
    );
    this.performLoad.load(
      this.reportService.getReportsForType(model).pipe(
        tap(data => {
          const reportsSmall: ISmallGenericType[] = [];
          data.forEach(f => {
            reportsSmall.push(
              new SmallGenericType(f.reportId, f.reportNr + ' ' + f.reportName)
            );
          });
          this.openSelectEmailDialog(reportsSmall);
        })
      )
    );
  }

  openSelectEmailDialog(reportsSmall: ISmallGenericType[]) {
    const dialogData = new SelectEmailDialogData();
    dialogData.title = 'common.checkdistribution';
    dialogData.size = 'lg';
    dialogData.defaultEmail = null;
    dialogData.defaultEmailTemplateId = this.defaultEmailTemplatePurchase;
    dialogData.recipients = [];
    dialogData.attachments = [];
    dialogData.attachmentsSelected = false;
    dialogData.checklists = [];
    dialogData.types = this.terms;
    dialogData.grid = true;
    dialogData.type = EmailTemplateType.PurchaseOrder;
    dialogData.showReportSelection = true;
    dialogData.reports = reportsSmall;
    dialogData.defaultReportTemplateId = this.defaultReportTemplatePurchase;
    dialogData.langId = undefined;
    const selectEmailDialog = this.dialogServiceV2.open(
      SelectEmailDialogComponent,
      dialogData
    );
    this.selectEmailDialogSubscription = selectEmailDialog
      .afterClosed()
      .subscribe((result: SelectEmailDialogCloseData) => {
        if (result && result.reportId) {
          const purchaseIds: number[] = [];
          this.grid.getSelectedRows().forEach(f => {
            purchaseIds.push(f.purchaseId);
          });
          const model = new SendPurchaseEmail(
            purchaseIds,
            result.emailTemplateId,
            result.languageId
          );
          this.sendPurchasesAsEmailSubscription = this.service
            .sendPurchasesAsEmail(model)
            .pipe(
              tap((res: any) => {
                if (res.success) {
                  this.toasterService.success(
                    this.translate.instant('common.sent'),
                    ''
                  );
                }
              })
            )
            .subscribe();
        }
      });
  }

  openSelectReport() {
    const dialogData = new SelectReportDialogData();
    dialogData.title = 'common.selectreport';
    dialogData.size = 'lg';
    dialogData.reportTypes = [SoeReportTemplateType.PurchaseOrder];
    dialogData.showCopy = false;
    dialogData.showEmail = false;
    dialogData.copyValue = false;
    dialogData.reports = [];
    dialogData.defaultReportId = 0;
    dialogData.langId = undefined;
    dialogData.showReminder = false;
    dialogData.showLangSelection = true;
    dialogData.showSavePrintout = false;
    dialogData.savePrintout = false;
    const selectReportDialog = this.dialogServiceV2.open(
      SelectReportDialogComponent,
      dialogData
    );

    selectReportDialog
      .afterClosed()
      .subscribe((result: SelectReportDialogCloseData) => {
        if (result && result.reportId) {
          const purchaseIds: number[] = [];
          this.grid.getSelectedRows().forEach(f => {
            purchaseIds.push(f.purchaseId);
          });
          const model = new GetPurchasePrintUrlModel(
            purchaseIds,
            [],
            result.reportId,
            result.languageId
          );

          this.performLoad.load(
            this.reportService.getPurchasePrintUrl(model).pipe(
              tap(url => {
                BrowserUtil.openInSameTab(window, url);
              })
            )
          );
        }
      });
  }

  selectionChanged(data: any) {
    this.hasDisabledFunctionButton.set(data.length <= 0);
  }

  loadPurchaseStatus() {
    return of(
      this.performLoad.load(
        this.service.getPurchaseStatus().pipe(
          tap(x => {
            this.purchaseStatus = x;

            const purchaseStatusTextDTO = new PurchaseStatusTextDTO();
            purchaseStatusTextDTO.lateText = this.translate.instant(
              'billing.purchase.late'
            );
            purchaseStatusTextDTO.restText =
              this.purchaseStatus.find((d: any) => {
                return d.id == SoeOriginStatus.Origin;
              })?.name +
              '/' +
              this.purchaseStatus.find((d: any) => {
                return d.id == SoeOriginStatus.PurchaseDone;
              })?.name +
              '/' +
              this.purchaseStatus.find((d: any) => {
                return d.id == SoeOriginStatus.PurchaseSent;
              })?.name;
            this.service.setPurchaseStatusTextSubject(purchaseStatusTextDTO);
          })
        )
      )
    );
  }

  onFilterReady(filterDTO: PurchaseFilterDTO) {
    this.isFilterLoad = true;
    this.service.setFilterSubject(filterDTO);
    if (this.isGridReady) {
      this.refreshGrid();
    }
  }

  filterOnChange(filterDTO: PurchaseFilterDTO) {
    if (this.isFilterLoad && this.isGridReady) {
      this.allItemsSelection = filterDTO.allItemsSelection;
      this.selectedPurchaseStatusIds = filterDTO.selectedPurchaseStatusIds;
      this.service.setFilterSubject(filterDTO);
      this.refreshGrid();
    }
  }

  override onGridReadyToDefine(grid: GridComponent<IPurchaseGridDTO>) {
    super.onGridReadyToDefine(grid);
    this.translate
      .get([
        'billing.purchase.purchasenr',
        'billing.project.project',
        'billing.purchase.supplierno',
        'billing.purchase.suppliername',
        'billing.purchase.origindescription',
        'billing.purchase.purchasestatus',
        'billing.purchaserow.totalexvat',
        'billing.purchase.foreignamount',
        'common.currency',
        'common.status',
        'billing.purchase.purchasedate',
        'billing.purchase.deliverydate',
        'billing.purchase.confirmeddate',

        'common.customer.invoices.emailsent',
        'core.edit',
        'billing.purchase.late',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.enableRowSelection();
        this.grid.addColumnText(
          'purchaseNr',
          terms['billing.purchase.purchasenr'],
          {
            flex: 1,
          }
        );
        this.grid.addColumnText('projectNr', terms['billing.project.project'], {
          flex: 1,
          enableHiding: true,
        });
        this.grid.addColumnText(
          'supplierNr',
          terms['billing.purchase.supplierno'],
          {
            flex: 1,
          }
        );
        this.grid.addColumnText(
          'supplierName',
          terms['billing.purchase.suppliername'],
          {
            flex: 1,
          }
        );
        this.grid.addColumnText(
          'origindescription',
          terms['billing.purchase.origindescription'],
          {
            enableHiding: true,
            flex: 1,
          }
        );
        this.grid.addColumnText(
          'statusName',
          terms['billing.purchase.purchasestatus'],
          {
            flex: 1,
          }
        );

        this.grid.addColumnNumber(
          'totalAmountExVat',
          terms['billing.purchaserow.totalexvat'],
          {
            decimals: 2,
            flex: 1,
          }
        );
        this.grid.addColumnNumber(
          'totalAmountExVatCurrency',
          terms['billing.purchase.foreignamount'],
          {
            flex: 1,
            enableHiding: true,
            hide: true,
          }
        );
        this.grid.addColumnText('currencyCode', terms['common.currency'], {
          flex: 1,
          enableHiding: true,
          hide: true,
        });

        this.grid.addColumnShape('deliveryStatus', terms['common.status'], {
          flex: 1,
          shape: 'circle',
          colorField: 'deliveryStatusColor',
          tooltipField: 'deliveryStatusText',
          enableHiding: true,
        });
        //suppressSorting
        this.grid.addColumnDate(
          'purchaseDate',
          terms['billing.purchase.purchasedate'],
          {
            flex: 1,
          }
        );
        this.grid.addColumnDate(
          'deliverydate',
          terms['billing.purchase.deliverydate'],
          {
            flex: 1,
          }
        );
        this.grid.addColumnDate(
          'confirmedDate',
          terms['billing.purchase.confirmeddate'],
          {
            flex: 1,
          }
        );
        this.grid.addColumnIcon('statusIconValue', '', {
          flex: 1,
          showIcon: row => row.statusIcon == SoeStatusIcon.Email,
          iconName: 'envelope',
          tooltip: terms['common.customer.invoices.emailsent'],
        });
        this.grid.addColumnIconEdit({
          tooltip: terms['core.edit'],
          onClick: row => {
            this.edit(row);
          },
        });
        super.finalizeInitGrid();
      });
  }

  override loadData(
    id?: number | undefined,
    additionalProps?: {
      allItemsSelection?: number;
      selectedPurchaseStatusIds?: number[];
    }
  ): Observable<IPurchaseGridDTO[]> {
    if (
      this.filter &&
      this.filter.allItemsSelection &&
      this.filter.selectedPurchaseStatusIds &&
      this.purchaseStatusText &&
      !StringUtil.isEmpty(this.purchaseStatusText.lateText) &&
      !StringUtil.isEmpty(this.purchaseStatusText.restText)
    )
      return this.service
        .getGrid(id, {
          allItemsSelection: this.filter.allItemsSelection,
          selectedPurchaseStatusIds: this.filter.selectedPurchaseStatusIds,
        })
        .pipe(
          map(data => {
            this.setInformationIconAndTooltip(data);
            return data;
          })
        );
    else return of([]);
  }

  private setInformationIconAndTooltip(rows: any[]) {
    rows.forEach(row => {
      switch (row.deliveryStatus) {
        case PurchaseDeliveryStatus.Delivered:
          row.deliveryStatusColor = '#24a148';
          row.deliveryStatusText = row.statusName;
          break;
        case PurchaseDeliveryStatus.PartlyDelivered:
          row.deliveryStatusColor = '#0565c9';
          row.deliveryStatusText = row.statusName;
          break;
        case PurchaseDeliveryStatus.Accepted:
          row.deliveryStatusColor = '#ff832b';
          row.deliveryStatusText = row.statusName;
          break;
        case PurchaseDeliveryStatus.Late:
          row.deliveryStatusColor = '#da1e28';
          row.deliveryStatusText = this.purchaseStatusText.lateText;
          break;
        default:
          row.deliveryStatusColor = '#dfdfdf';
          row.deliveryStatusText = this.purchaseStatusText.restText;
          break;
      }
    });
  }
}

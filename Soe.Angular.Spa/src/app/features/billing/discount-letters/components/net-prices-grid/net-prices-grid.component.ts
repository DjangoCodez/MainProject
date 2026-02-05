import { Component, OnInit, inject, signal } from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { CrudActionTypeEnum } from '@shared/enums';
import { SmallGenericType } from '@shared/models/generic-type.model';
import {
  ISupplierAgreementModel,
  ISupplierNetPricesDeleteModel,
} from '@shared/models/generated-interfaces/BillingModels';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { IWholsellerNetPriceRowDTO } from '@shared/models/generated-interfaces/WholsellerNetPriceDTO';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ProgressOptions } from '@shared/services/progress';
import { DateUtil } from '@shared/util/date-util';
import { Perform } from '@shared/util/perform.class';
import { DialogService } from '@ui/dialog/services/dialog.service';
import { GridComponent } from '@ui/grid/grid.component';
import { MessageboxService } from '@ui/dialog/services/messagebox.service';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { Observable, filter, forkJoin, of, take, tap } from 'rxjs';
import { DiscountLettersService } from '../../services/discount-letters.service';
import {
  ImportAgreementDialogComponent,
  ImportAgreementDialogContainer,
  ImportAgreementDialogData,
} from '../discount-letters-grid/import-agreement-dialog/import-agreement-dialog.component';
import { ResponseUtil } from '@shared/util/response-util';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';

enum WholeSellerType {
  SelectWholeSeller = 0,
  All = 99999,
}

@Component({
  selector: 'soe-net-prices-grid',
  templateUrl: './net-prices-grid.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class NetPricesGridComponent
  extends GridBaseDirective<IWholsellerNetPriceRowDTO>
  implements OnInit
{
  discountLettersService = inject(DiscountLettersService);
  dialogService = inject(DialogService);
  private readonly messageboxService = inject(MessageboxService);

  perform = new Perform<unknown>(this.progressService);
  performLoadData = new Perform<IWholsellerNetPriceRowDTO[]>(
    this.progressService
  );
  performSaveData = new Perform<BackendResponse>(this.progressService);

  priceLists: SmallGenericType[] = [];
  wholesellersDict: SmallGenericType[] = [];
  wholesellersWithSeparateFileDict: SmallGenericType[] = [];
  selectedWholesellerId: number = 0;
  disableDeleteFunction = signal(true);

  ngOnInit(): void {
    super.ngOnInit();

    this.startFlow(
      Feature.Billing_Preferences_InvoiceSettings_SupplierAgreement,
      'Billing.Preferences.InvoiceSettings.NetPrices',
      {
        additionalModifyPermissions: [
          Feature.Billing_Preferences_InvoiceSettings_SupplierAgreement_Edit,
        ],
        skipInitialLoad: true,
        lookups: [
          this.loadWholeseller(),
          this.loadWholesellerWithSeparateFile(),
          this.loadPriceLists(),
        ],
      }
    );
  }

  loadPriceLists() {
    return this.perform.load$(
      this.discountLettersService.getPriceLists(true).pipe(
        tap(res => {
          this.priceLists = res;
        })
      )
    );
  }

  loadWholeseller() {
    return this.perform.load$(
      forkJoin([
        this.translate
          .get(['common.all', 'common.searchinvoiceproduct.selectwholeseller'])
          .pipe(take(1)),
        this.discountLettersService.getNetWholeSellers(true, false),
      ]).pipe(
        tap(([terms, res]) => {
          this.wholesellersDict = [
            {
              id: 0,
              name: terms['common.searchinvoiceproduct.selectwholeseller'],
            },
            { id: WholeSellerType.All, name: terms['common.all'] },
            ...res,
          ];
        })
      )
    );
  }

  loadWholesellerWithSeparateFile() {
    return this.perform.load$(
      this.discountLettersService.getNetWholeSellers(true, true).pipe(
        tap(res => {
          this.wholesellersWithSeparateFileDict = res;
        })
      )
    );
  }

  override onGridReadyToDefine(grid: GridComponent<IWholsellerNetPriceRowDTO>) {
    super.onGridReadyToDefine(grid);
    this.grid.enableRowSelection();
    this.translate
      .get([
        'common.customer.customer.wholesellername',
        'billing.product.number',
        'billing.product.name',
        'billing.order.pricelisttype',
        'common.syswholesellerprices.gnp',
        'common.syswholesellerprices.nettonetto',
        'common.date',
        'core.info',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnText(
          'wholesellerName',
          terms['common.customer.customer.wholesellername'],
          {
            flex: 80,
            showSetFilter: true,
          }
        );
        this.grid.addColumnText(
          'priceListTypeName',
          terms['billing.order.pricelisttype'],
          {
            flex: 80,
            showSetFilter: true,
          }
        );
        this.grid.addColumnText('productNr', terms['billing.product.number'], {
          flex: 80,
        });
        this.grid.addColumnText('productName', terms['billing.product.name'], {
          flex: 80,
        });
        this.grid.addColumnNumber(
          'gnp',
          terms['common.syswholesellerprices.gnp'],
          {
            flex: 80,
          }
        );
        this.grid.addColumnNumber(
          'netPrice',
          terms['common.syswholesellerprices.nettonetto'],
          {
            flex: 80,
          }
        );
        this.grid.addColumnDate('date', terms['common.date'], {
          flex: 80,
        });
        this.grid.addColumnIcon(null, '', {
          flex: 20,
          iconName: 'info-circle',
          iconClass: 'information-color',
          enableHiding: false,
          tooltip: terms['core.info'],
          onClick: row => {
            this.showInfo(row);
          },
        });

        super.finalizeInitGrid();
      });
  }

  changeWholeseller(wholesellerId: number) {
    this.selectedWholesellerId = wholesellerId;
    this.refreshGrid();
  }

  override loadData(
    id?: number | undefined
  ): Observable<IWholsellerNetPriceRowDTO[]> {
    if (this.selectedWholesellerId === WholeSellerType.SelectWholeSeller) {
      return of([]);
    }

    return this.performLoadData.load$(
      this.discountLettersService.getNetPrices(
        this.selectedWholesellerId === WholeSellerType.All
          ? 0
          : this.selectedWholesellerId
      )
    );
  }

  selectionChanged(data: IWholsellerNetPriceRowDTO[]) {
    this.disableDeleteFunction.set(data.length <= 0);
  }

  importNetPrices() {
    const dialogData: ImportAgreementDialogData = {
      title: this.translate.instant(
        'billing.invoices.supplieragreement.addnetprices'
      ),
      size: 'lg',
      priceLists: this.priceLists,
      showHeaderInfo: false,
      wholesellersDict: this.wholesellersWithSeparateFileDict,
      container: ImportAgreementDialogContainer.NetPrices,
    };
    this.dialogService
      .open(ImportAgreementDialogComponent, dialogData)
      .afterClosed()
      .pipe(filter(value => !!value))
      .subscribe((value: ISupplierAgreementModel) => {
        this.performSaveData.crud(
          CrudActionTypeEnum.Save,
          this.discountLettersService.importNetPrices(value),
          result => {
            const message = ResponseUtil.getMessageValue(result);
            if (message && message.length > 0) {
              this.progressService.saveComplete(<ProgressOptions>{
                showDialogOnComplete: true,
                showToastOnComplete: false,
                title: 'core.info',
                message: message,
              });
            }
            this.changeWholeseller(this.selectedWholesellerId);
          }
        );
      });
  }

  removeNetPrices() {
    const selectedRows = this.grid.getSelectedRows();
    const ids = selectedRows.map(p => p.wholsellerNetPriceRowId);

    const deleteModel = {
      wholsellerNetPriceRowIds: ids,
    } as ISupplierNetPricesDeleteModel;

    this.performSaveData.crud(
      CrudActionTypeEnum.Delete,
      this.discountLettersService.deleteNetPriceRows(deleteModel),
      () => this.refreshGrid(),
      undefined,
      {}
    );
  }

  showInfo(row: IWholsellerNetPriceRowDTO) {
    this.messageboxService.information(
      'core.info',
      this.translate
        .instant('common.createdbyat')
        .format(
          row.createdBy,
          row.created
            ? DateUtil.format(new Date(row.created), 'yyyy-MM-dd HH:mm')
            : ''
        )
    );
  }
}

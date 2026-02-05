import { Component, inject, OnInit } from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { ISupplierAgreementDTO } from '@shared/models/generated-interfaces/SupplierAgreementDTOs';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ProgressService } from '@shared/services/progress/progress.service';
import { Perform } from '@shared/util/perform.class';
import { DiscountLettersService } from '../../services/discount-letters.service';
import {
  Feature,
  SoeSupplierAgreemntCodeType,
} from '@shared/models/generated-interfaces/Enumerations';
import { filter, forkJoin, Observable, of, take, tap } from 'rxjs';
import { SmallGenericType } from '@shared/models/generic-type.model';
import {
  SupplierAgreementDialogComponent,
  SupplierAgreementDialogData,
} from './supplier-agreement-dialog/supplier-agreement-dialog.component';
import { DialogService } from '@ui/dialog/services/dialog.service';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { CrudActionTypeEnum } from '@shared/enums';
import {
  ImportAgreementDialogComponent,
  ImportAgreementDialogData,
  ImportAgreementDialogContainer,
} from './import-agreement-dialog/import-agreement-dialog.component';
import { ISupplierAgreementModel } from '@shared/models/generated-interfaces/BillingModels';
import {
  DeleteAgreementDialogComponent,
  DeleteAgreementDialogData,
} from './delete-agreement-dialog/delete-agreement-dialog.component';
import { ProgressOptions } from '@shared/services/progress';
import { ResponseUtil } from '@shared/util/response-util';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';

const ALL_PROVIDERS = 99999;

@Component({
  selector: 'soe-discount-letters-grid',
  templateUrl: './discount-letters-grid.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class DiscountLettersGridComponent
  extends GridBaseDirective<ISupplierAgreementDTO, DiscountLettersService>
  implements OnInit
{
  discountLettersService = inject(DiscountLettersService);
  progressService = inject(ProgressService);
  dialogService = inject(DialogService);
  performLoadPriceLists = new Perform<SmallGenericType[]>(this.progressService);
  performLoadProviders = new Perform<SmallGenericType[]>(this.progressService);
  performLoadData = new Perform<ISupplierAgreementDTO[]>(this.progressService);
  performSaveData = new Perform<BackendResponse>(this.progressService);

  priceLists: SmallGenericType[] = [];
  wholesellersDict: SmallGenericType[] = [];
  codeTypes: SmallGenericType[] = [];

  selectedWholesellerId = 0;

  ngOnInit(): void {
    super.ngOnInit();

    this.startFlow(
      Feature.Billing_Preferences_InvoiceSettings_SupplierAgreement,
      'Billing.Preferences.InvoiceSettings.SupplierAgreement',
      {
        additionalModifyPermissions: [
          Feature.Billing_Preferences_InvoiceSettings_SupplierAgreement_Edit,
        ],
        skipInitialLoad: true,
        lookups: [
          this.loadPriceLists(),
          this.loadAgreementsProviders(),
          this.setupSoeSupplierAgreemntCodeTypes(),
        ],
      }
    );
  }

  loadPriceLists() {
    return this.performLoadPriceLists.load$(
      this.discountLettersService.getPriceLists(true).pipe(
        tap(res => {
          this.priceLists = res;
        })
      )
    );
  }

  loadAgreementsProviders() {
    return this.performLoadProviders.load$(
      forkJoin([
        this.translate
          .get(['common.all', 'common.searchinvoiceproduct.selectwholeseller'])
          .pipe(take(1)),
        this.discountLettersService.getProvidersDict(),
      ]).pipe(
        tap(([terms, res]) => {
          this.wholesellersDict = [
            {
              id: 0,
              name: terms['common.searchinvoiceproduct.selectwholeseller'],
            },
            { id: ALL_PROVIDERS, name: terms['common.all'] },
            ...res,
          ];
        })
      )
    );
  }

  setupSoeSupplierAgreemntCodeTypes() {
    return this.translate
      .get([
        'billing.invoices.supplieragreement.materialclass',
        'billing.invoices.supplieragreement.productnr',
        'common.general',
      ])
      .pipe(
        take(1),
        tap(terms => {
          this.codeTypes = [
            {
              id: <number>SoeSupplierAgreemntCodeType.Generic,
              name: terms['common.general'],
            },
            {
              id: <number>SoeSupplierAgreemntCodeType.MaterialCode,
              name: terms['billing.invoices.supplieragreement.materialclass'],
            },
            {
              id: <number>SoeSupplierAgreemntCodeType.Product,
              name: terms['billing.invoices.supplieragreement.productnr'],
            },
          ];
        })
      );
  }

  override onGridReadyToDefine(grid: GridComponent<ISupplierAgreementDTO>) {
    super.onGridReadyToDefine(grid);

    this.translate
      .get([
        'common.customer.customer.wholesellername',
        'billing.order.pricelisttype',
        'billing.invoices.supplieragreement.materialclassproductnr',
        'billing.productrows.dialogs.discountpercent',
        'common.date',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnText(
          'wholesellerName',
          terms['common.customer.customer.wholesellername'],
          {
            flex: 1,
            showSetFilter: true,
          }
        );
        this.grid.addColumnText(
          'priceListTypeName',
          terms['billing.order.pricelisttype'],
          {
            flex: 1,
            showSetFilter: true,
          }
        );
        this.grid.addColumnText(
          'code',
          terms['billing.invoices.supplieragreement.materialclassproductnr'],
          {
            flex: 1,
          }
        );
        this.grid.addColumnNumber(
          'discountPercent',
          terms['billing.productrows.dialogs.discountpercent'],
          {
            flex: 1,
            decimals: 2,
          }
        );
        this.grid.addColumnDate('date', terms['common.date'], {
          flex: 1,
        });
        if (this.flowHandler.modifyPermission()) {
          this.grid.addColumnIconEdit({
            onClick: this.openEditDialog.bind(this),
            showIcon: (row: ISupplierAgreementDTO) => {
              return (
                row.sysWholesellerId === 23 ||
                row.sysWholesellerId === 94 ||
                row.codeType === SoeSupplierAgreemntCodeType.Generic
              );
            },
          });
        }

        super.finalizeInitGrid();
      });
  }

  changeWholeseller(wholesellerId: number) {
    this.selectedWholesellerId = wholesellerId;
    this.refreshGrid();
  }

  override loadData(
    id?: number | undefined
  ): Observable<ISupplierAgreementDTO[]> {
    if (this.selectedWholesellerId === 0) {
      return of([]);
    }

    return this.performLoadData.load$(
      this.discountLettersService.getSupplierAgreements(
        this.selectedWholesellerId === ALL_PROVIDERS
          ? 0
          : this.selectedWholesellerId
      )
    );
  }

  openImportAgreementDialog() {
    const dialogData: ImportAgreementDialogData = {
      title: this.translate.instant(
        'billing.invoices.supplieragreement.addagreement'
      ),
      size: 'lg',
      priceLists: this.priceLists,
      showHeaderInfo: true,
      wholesellersDict: this.wholesellersDict.filter(x => x.id !== 0),
      container: ImportAgreementDialogContainer.SupplierAgreement,
    };
    this.dialogService
      .open(ImportAgreementDialogComponent, dialogData)
      .afterClosed()
      .pipe(filter(value => !!value))
      .subscribe((value: ISupplierAgreementModel) => {
        this.performSaveData.crud(
          CrudActionTypeEnum.Save,
          this.discountLettersService.importSupplierAgreement(value),
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

  openRemoveAgreementDialog() {
    const dialogData: DeleteAgreementDialogData = {
      title: this.translate.instant(
        'billing.invoices.supplieragreement.deleteagreement'
      ),
      size: 'lg',
      priceLists: this.priceLists,
      wholesellersDict: this.wholesellersDict.filter(x => x.id !== 0),
    };
    this.dialogService
      .open(DeleteAgreementDialogComponent, dialogData)
      .afterClosed()
      .pipe(filter(value => !!value))
      .subscribe(
        (value: { wholesellerId: number; priceListTypeId: number }) => {
          this.performSaveData.crud(
            CrudActionTypeEnum.Delete,
            this.discountLettersService.delete(
              value.wholesellerId,
              value.priceListTypeId
            ),
            () => {
              this.changeWholeseller(this.selectedWholesellerId);
            }
          );
        }
      );
  }

  openEditDialog(rowToUpdate?: ISupplierAgreementDTO) {
    const dialogData: SupplierAgreementDialogData = {
      title: this.translate.instant(
        'billing.invoices.supplieragreement.adddiscount'
      ),
      size: 'lg',
      rowToUpdate,
      priceLists: this.priceLists,
      wholesellersDict: this.wholesellersDict.filter(x => x.id !== 0),
      codeTypes: this.codeTypes,
    };
    this.dialogService
      .open(SupplierAgreementDialogComponent, dialogData)
      .afterClosed()
      .pipe(filter(value => !!value))
      .subscribe((value: ISupplierAgreementDTO) => {
        this.performSaveData.crud(
          CrudActionTypeEnum.Save,
          this.discountLettersService.save(value),
          () => {
            this.changeWholeseller(this.selectedWholesellerId);
          }
        );
      });
  }
}

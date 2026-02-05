import {
  Component,
  OnDestroy,
  OnInit,
  computed,
  inject,
  signal,
} from '@angular/core';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import {
  ISupplierProductImportDTO,
  ISupplierProductPriceComparisonDTO,
  ISupplierProductPriceDTO,
} from '@shared/models/generated-interfaces/SupplierProductDTOs';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { Perform } from '@shared/util/perform.class';
import { PurchaseProductPricelistService } from '../../services/purchase-product-pricelist.service';
import { BehaviorSubject, Observable, Subscription, of, tap } from 'rxjs';
import {
  SupplierProductPriceComparisonDTO,
  SupplierProductPriceListSaveDTO,
  SupplierProductPricelistDTO,
} from '../../models/purchase-product-pricelist.model';
import { EditBaseDirective } from '@shared/directives/edit-base/edit-base.directive';
import { SupplierProductPriceListForm } from '../../models/purchase-product-pricelist-form.model';
import { SupplierService } from '@src/app/features/economy/services/supplier.service';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { ProgressOptions } from '@shared/services/progress';
import { CrudActionTypeEnum } from '@shared/enums';
import {
  IImportDynamicResultDTO,
  IImportOptionsDTO,
  ISupplierProductImportRawDTO,
} from '@shared/models/generated-interfaces/ImportDynamicDTO';
import { ImportDynamicService } from '@shared/components/billing/import-dynamic/import-dynamic/import-dynamic.service';
import { DialogService } from '@ui/dialog/services/dialog.service';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import {
  ImportDyanmicDialogData,
  ImportDynamicDTO,
} from '@shared/components/billing/import-dynamic/import-dynamic/import-dynamic.model';
import { ImportDynamicComponent } from '@shared/components/billing/import-dynamic/import-dynamic/import-dynamic.component';
import { CurrencyService } from '@shared/services/currency.service';

@Component({
  selector: 'soe-purchase-product-pricelist-edit',
  templateUrl: './purchase-product-pricelist-edit.component.html',
  providers: [FlowHandlerService, ToolbarService, CurrencyService],
  standalone: false,
})
export class PurchaseProductPricelistEditComponent
  extends EditBaseDirective<
    SupplierProductPricelistDTO,
    PurchaseProductPricelistService,
    SupplierProductPriceListForm
  >
  implements OnInit, OnDestroy
{
  readonly service = inject(PurchaseProductPricelistService);
  readonly coreService = inject(CoreService);
  readonly supplierService = inject(SupplierService);
  readonly importDynamicService = inject(ImportDynamicService);
  readonly currencyService = inject(CurrencyService);
  readonly dialogService = inject(DialogService);
  performSupplierPrice = new Perform<ISupplierProductPriceComparisonDTO[]>(
    this.progressService
  );

  suppliers: SmallGenericType[] = [];
  suppliersCopy: SmallGenericType[] = [];
  priceData = new BehaviorSubject<SupplierProductPriceComparisonDTO[]>([]);
  currencyIdSubscription?: Subscription;
  formStatusSubscription?: Subscription;
  rowCount = signal(0);
  isInActiveSupplier = signal(false);
  formDirty = signal(false);
  formHasSupplier = signal(false);

  disableFileImport = computed((): boolean => {
    return (
      this.isInActiveSupplier() || !this.formHasSupplier() || this.formDirty()
    );
  });

  ngOnInit(): void {
    super.ngOnInit();

    this.startFlow(Feature.Billing_Purchase_Pricelists, {
      additionalModifyPermissions: [Feature.Economy_Supplier_Suppliers_Edit],
      lookups: [this.loadSuppliers()],
    });

    this.formHasSupplier.set(
      !this.form?.isNew || !!this.form?.supplierProductPriceListId.value
    );

    this.formStatusSubscription = this.form?.statusChanges.subscribe(
      (): void => {
        this.formDirty.set(!!this.form?.dirty);
      }
    );

    this.currencyIdSubscription =
      this.currencyService.currencyIdChanged$.subscribe(currencyId => {
        this.form?.patchValue({
          currencyId: currencyId,
        });
      });

    this.priceData.subscribe(() => {
      this.form?.resetPriceRows(this.priceData.value);
    });

    this.form?.startDate.valueChanges.subscribe(startDate => {
      this.onStartDateChange(startDate);
    });
  }

  override newRecord(): Observable<void> {
    const resetValues = (): void => {
      if (this.form?.isNew && !this.form?.isCopy) {
        this.form?.patchValue({
          currencyId: this.currencyService.getCurrencyId(),
        });
      }
    };

    if (this.form?.isCopy) {
      this.priceData.next(this.form.priceRows.value);
    }

    return of(resetValues());
  }

  override createEditToolbar(): void {
    super.createEditToolbar();

    this.toolbarService.createItemGroup({
      items: [
        this.toolbarService.createToolbarButton('import', {
          iconName: signal('file-import'),
          tooltip: signal('billing.purchase.product.importpricelist'),
          caption: signal('billing.purchase.product.importpricelist'),
          disabled: this.disableFileImport,
          onAction: () => this.openImportDialog(),
        }),
      ],
    });
  }

  loadSuppliers(): Observable<SmallGenericType[]> {
    return this.performLoadData.load$(
      this.supplierService
        .getSupplierDict(true, false)
        .pipe(tap(s => (this.suppliers = <SmallGenericType[]>s)))
    );
  }

  private openImportDialog(): void {
    const importCallBack = (
      data: ISupplierProductImportRawDTO[],
      options: IImportOptionsDTO
    ): Observable<IImportDynamicResultDTO> => {
      const model: ISupplierProductImportDTO = {
        importToPriceList: true,
        importPrices: true,
        supplierId: this.form?.supplierId.value,
        priceListId: this.form?.supplierProductPriceListId.value,
        rows: data,
        options: options,
      };

      return this.service.performPricelistImport(model);
    };

    this.importDynamicService
      .getSupplierPricelistImport(false, true, false)
      .subscribe(config => {
        this.dialogService
          .open(ImportDynamicComponent, <ImportDyanmicDialogData>{
            size: 'xl',
            title: 'billing.stock.stocks.importfromfile',
            disableClose: true,
            importDTO: config as ImportDynamicDTO,
            callback: importCallBack,
          })
          .afterClosed()
          .subscribe((value): void => this.closeDialog(value));
      });
  }

  private closeDialog(value: unknown): void {
    if (value) this.loadData().subscribe();
  }

  override loadData(): Observable<void> {
    this.enableControls();
    return this.performLoadData.load$(
      this.service.get(this.form?.getIdControl()?.value).pipe(
        tap(value => {
          this.form?.reset(value);
          const supplier = new SmallGenericType(
            value.supplierId,
            value.supplierNr + ' ' + value.supplierName
          );

          this.form?.supplierId.disable();
          this.form?.startDate.disable();
          this.form?.currencyId.disable();
          this.loadPriceRows(value.supplierProductPriceListId);

          this.suppliersCopy = [...this.suppliers];
          if (
            !this.suppliers.some(s => s.id === supplier.id) &&
            !this.form?.isNew
          ) {
            this.isInActiveSupplier.set(true);
            this.form?.disable();
            this.copyDisabled.set(true);
          }
          this.suppliers = [supplier];
        })
      )
    );
  }

  private enableControls(): void {
    if (this.flowHandler.modifyPermission()) this.form?.enable();
    this.isInActiveSupplier.set(false);
    this.copyDisabled.set(false);
    this.suppliers = [...this.suppliersCopy];
  }

  loadPriceRows(pricelistId: number): void {
    this.performSupplierPrice
      .load$(this.service.getSupplierPriceGrid(pricelistId, true))
      .subscribe(value => {
        this.priceData.next(value as SupplierProductPriceComparisonDTO[]);
        this.form?.resetPriceRows(this.priceData.value);
        this.rowCount.set(this.priceData.value.length);
      });
  }

  onStartDateChange(startDate?: Date): void {
    if (!startDate) return;
    if (this.form?.endDate.value > startDate) return;

    this.form?.endDate.setValue(startDate);
    this.form?.endDate.updateValueAndValidity();
  }

  override performSave(options?: ProgressOptions): void {
    this.priceData.subscribe(e => {
      this.form?.updatePriceRows(e);
    });

    const model = new SupplierProductPriceListSaveDTO();
    model.priceList = <SupplierProductPricelistDTO>this.form?.getRawValue();
    model.priceRows =
      <ISupplierProductPriceDTO[]>this.form?.priceRows.value ?? [];

    this.performAction.crud(
      CrudActionTypeEnum.Save,
      this.service.saveData(model).pipe(tap(this.updateFormValueAndEmitChange)),
      undefined,
      undefined,
      options
    );
  }

  ngOnDestroy(): void {
    this.currencyIdSubscription?.unsubscribe();
    this.formStatusSubscription?.unsubscribe();
  }
}

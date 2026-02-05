import {
  AfterViewInit,
  Component,
  computed,
  ElementRef,
  inject,
  OnDestroy,
  OnInit,
  signal,
  ViewChild,
} from '@angular/core';
import { StockBalanceFileImportDialogData } from '@features/billing/shared/components/stock-balance-file-import/stock-balance-file-import/models/stock-balance-file-import.model';
import { StockBalanceFileImportComponent } from '@features/billing/shared/components/stock-balance-file-import/stock-balance-file-import/stock-balance-file-import.component';
import { SelectReportDialogComponent } from '@shared/components/select-report-dialog/components/select-report-dialog/select-report-dialog.component';
import {
  SelectReportDialogCloseData,
  SelectReportDialogData,
} from '@shared/components/select-report-dialog/models/select-report-dialog.model';
import { EditBaseDirective } from '@shared/directives/edit-base/edit-base.directive';
import { CrudActionTypeEnum } from '@shared/enums';
import { ValidationHandler } from '@shared/handlers';
import { SmallGenericType } from '@shared/models/generic-type.model';
import {
  Feature,
  SoeReportTemplateType,
} from '@shared/models/generated-interfaces/Enumerations';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import { IProductSmallDTO } from '@shared/models/generated-interfaces/ProductDTOs';
import {
  IProductGroupGridDTO,
  IStockInventoryRowDTO,
  IStockShelfDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ProgressOptions, ProgressService } from '@shared/services/progress';
import { Perform } from '@shared/util/perform.class';
import { SoeConfigUtil } from '@shared/util/soeconfig-util';
import { focusOnElement } from '@shared/util/focus-util';
import { DownloadUtility } from '@shared/util/download-util';
import { DialogService } from '@ui/dialog/services/dialog.service';
import { MenuButtonItem } from '@ui/button/menu-button/menu-button.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { BehaviorSubject, Observable, Subject, takeUntil, tap } from 'rxjs';
import { ProductGroupsService } from '../../../product-groups/services/product-groups.service';
import { StockBalanceService } from '../../../stock-balance/services/stock-balance.service';
import { StockWarehouseService } from '../../../stock-warehouse/services/stock-warehouse.service';
import { StockInventoryHeadForm } from '../../models/stock-inventory-head-form.model';
import { StockInventorySelectionForm } from '../../models/stock-inventory-selection-form.model';
import {
  StockInventoryFilterDTO,
  StockInventoryHeadDTO,
} from '../../models/stock-inventory.model';
import { StockInventoryService } from '../../services/stock-inventory.service';
import { StockInventoryEditItemGridComponent } from './stock-inventory-edit-item-grid/stock-inventory-edit-item-grid.component';
import { RequestReportService } from '@shared/services/request-report.service';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';
import { ResponseUtil } from '@shared/util/response-util';

export enum UpdateFunctionType {
  UpdateTransactionDate = 1,
  UpdateStockQuantity = 2,
}

@Component({
  selector: 'soe-stock-inventory-edit',
  templateUrl: './stock-inventory-edit.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class StockInventoryEditComponent
  extends EditBaseDirective<
    StockInventoryHeadDTO,
    StockInventoryService,
    StockInventoryHeadForm
  >
  implements OnInit, OnDestroy, AfterViewInit
{
  @ViewChild('headerText')
  public headerText!: ElementRef;
  @ViewChild(StockInventoryEditItemGridComponent)
  editItemGrid!: StockInventoryEditItemGridComponent;
  private _destroy$ = new Subject<void>();
  private _terms: any;
  private _generatedData: IStockInventoryRowDTO[] = [];
  readonly validationHandler = inject(ValidationHandler);
  readonly service = inject(StockInventoryService);
  readonly serviceWarehouse = inject(StockWarehouseService);
  readonly serviceStockBalance = inject(StockBalanceService);
  readonly serviceProductGroup = inject(ProductGroupsService);
  readonly dialogServicev2 = inject(DialogService);
  private readonly progress = inject(ProgressService);
  private readonly perform = new Perform<any>(this.progress);
  private readonly requestReportService = inject(RequestReportService);

  warehouses: ISmallGenericType[] = [];
  shelves: IStockShelfDTO[] = [];

  menuList: MenuButtonItem[] = [];
  filteredStockPlaces = signal<SmallGenericType[]>([]);
  stockProducts = signal<IProductSmallDTO[]>([]);
  productGroups = signal<SmallGenericType[]>([]);
  hideImport = signal(true);
  rowData = new BehaviorSubject<IStockInventoryRowDTO[]>([]);
  disableUpdateButton = signal(true);
  disableAcceptButton = signal(false);

  private printButtonDisabled = signal(false);
  private printButtonLocked = computed((): boolean => {
    return !(this.form?.value.stockInventoryHeadId > 0);
  });

  formSelection: StockInventorySelectionForm = new StockInventorySelectionForm({
    validationHandler: this.validationHandler,
    element: new StockInventoryFilterDTO(),
  });

  get isEditable() {
    return (
      this.form?.value.stockInventoryHeadId > 0 &&
      !this.form?.value.inventoryStop
    );
  }

  ngOnInit() {
    super.ngOnInit();
    this.recordConfig.hideRecordNavigator = true;

    this.startFlow(Feature.Billing_Stock_Inventory, {
      lookups: [
        this.loadWareHouses(),
        this.loadStockLocations(),
        this.loadProductGroups(),
        this.loadTranslations(),
      ],
    });
    this.buildUpdateFunctionList();

    this.formSelection?.productNrFromId.valueChanges
      .pipe(takeUntil(this._destroy$))
      .subscribe(x => {
        if (x === null) {
          this.formSelection.patchValue({ productNrFrom: undefined });
        }
      });

    this.formSelection?.productNrToId.valueChanges
      .pipe(takeUntil(this._destroy$))
      .subscribe(x => {
        if (x === null) {
          this.formSelection.patchValue({ productNrTo: undefined });
        }
      });
  }

  ngAfterViewInit(): void {
    if (this.form?.isNew) {
      focusOnElement((<any>this.headerText).inputER.nativeElement, 50);
    }
  }

  override createEditToolbar(): void {
    super.createEditToolbar({ hideCopy: true });

    this.toolbarService.createItemGroup({
      items: [
        this.toolbarService.createToolbarButton('import', {
          iconName: signal('file-import'),
          caption: signal('billing.stock.stockinventory.importinventoryfile'),
          tooltip: signal('billing.stock.stockinventory.importinventoryfile'),
          hidden: this.hideImport,
          onAction: () => this.importFileDialog(),
        }),
        this.toolbarService.createToolbarButton('print', {
          iconName: signal('print'),
          caption: signal('core.print'),
          tooltip: signal('core.print'),
          disabled: this.printButtonDisabled,
          hidden: this.printButtonLocked,
          onAction: () => this.openReportDialog(),
        }),
      ],
    });
  }

  //#region LOOKUPS

  loadStockLocations(): Observable<IStockShelfDTO[]> {
    return this.performLoadData.load$(
      this.serviceWarehouse.getStockShelves(true, 0).pipe(
        tap(x => {
          this.shelves = x;
        })
      )
    );
  }

  loadWareHouses(): Observable<ISmallGenericType[]> {
    return this.performLoadData.load$(
      this.serviceWarehouse.getStockWarehousesDict(false).pipe(
        tap(x => {
          this.warehouses = x;
        })
      )
    );
  }

  override loadData(): Observable<void> {
    return this.performLoadData.load$(
      this.service.get(this.form?.getIdControl()?.value).pipe(
        tap(value => {
          this.disableControls();
          value.stockInventoryRows.sort((a, b) => {
            const nameA = a.productNumber.toUpperCase(); // ignore upper and lowercase
            const nameB = b.productNumber.toUpperCase(); // ignore upper and lowercase
            if (nameA < nameB) {
              return -1;
            }
            if (nameA > nameB) {
              return 1;
            }

            // names must be equal
            return 0;
          });
          this.form?.customPatchValue(value);
          this.setGridData(value.stockInventoryRows);
          this.setImportButtonVisibility(
            value.stockInventoryHeadId,
            value.inventoryStop
          );
        })
      )
    );
  }

  loadStockProducts(stockId: number): void {
    this.performLoadData.load(
      this.serviceStockBalance
        .getStockProductProducts(stockId, true)
        .pipe(tap(p => this.stockProducts.set(p)))
    );
  }

  loadProductGroups(): Observable<IProductGroupGridDTO[]> {
    return this.performLoadData.load$(
      this.serviceProductGroup.getGrid().pipe(
        tap(x => {
          const prodGroups: SmallGenericType[] = [];
          prodGroups.push(<SmallGenericType>{
            id: 0,
            name: '-',
          });
          x.map(p =>
            prodGroups.push(<SmallGenericType>{
              id: p.productGroupId,
              name: p.code + ' - ' + p.name,
            })
          );
          this.productGroups.set(prodGroups);
        })
      )
    );
  }

  loadTranslations(): Observable<any> {
    return this.translate
      .get([
        'billing.stock.stockinventory.norowsgenerated',
        'common.status',
        'core.question',
        'billing.stock.stockinventory.acceptinventoryverifymessage',
      ])
      .pipe(tap(t => (this._terms = t)));
  }

  //#endregion

  //#region EVENTS

  override performSave(options?: ProgressOptions): void {
    if (!this.form || this.form.invalid || !this.service) return;

    //If no stock Inventory rows selected in the grid,
    //all filtered rows will be saved to inventory
    if (
      this.form?.stockInventoryRows.length == 0 &&
      this._generatedData.length > 0
    ) {
      this.form?.addStockInventoryRows(this._generatedData);
    }

    this.performAction.crud(
      CrudActionTypeEnum.Save,
      this.service.save(this.form?.getRawValue()).pipe(
        tap(res => {
          if (res) {
            this.updateFormValueAndEmitChange(res);
          }
        })
      ),
      undefined,
      undefined,
      options
    );
  }

  updateFormValueAndEmitChange = (backendResponse: BackendResponse) => {
    if (backendResponse.success) {
      const entityId = ResponseUtil.getEntityId(backendResponse);
      if (entityId && entityId > 0) {
        this.form?.patchValue({
          [this.idFieldName]: entityId,
        });

        this.actionTakenSignal().set({
          rowItemId: entityId,
          ref: this.ref(),
          type: CrudActionTypeEnum.Save,
          form: this.form,
          additionalProps: this.additionalSaveProps,
        });
      }
      this.loadData().subscribe();
    }
  };

  acceptInventory(): void {
    this.messageboxService
      .question(
        this._terms['core.question'],
        this._terms[
          'billing.stock.stockinventory.acceptinventoryverifymessage'
        ],
        { size: 'lg' }
      )
      .afterClosed()
      .subscribe(res => {
        if (res.result) {
          this.performAction.crud(
            CrudActionTypeEnum.Save,
            this.service
              .closeInventory(this.form?.stockInventoryHeadId.value)
              .pipe(
                tap(res => {
                  this.disableAcceptButton.set(true);
                  this.updateFormValueAndEmitChange(res);

                  //Makesure its no longer editable
                  if (res.success)
                    this.form?.patchValue({ inventoryStop: new Date() });
                })
              ),
            undefined,
            undefined,
            undefined
          );
        }
      });
  }

  importFileDialog(): void {
    const dialogData = new StockBalanceFileImportDialogData();
    dialogData.title = 'core.fileupload.choosefiletoimport';
    dialogData.size = 'lg';
    dialogData.stockInventoryHeadId = this.form?.stockInventoryHeadId.value;

    this.dialogServicev2
      .open(StockBalanceFileImportComponent, dialogData)
      .afterClosed()
      .subscribe(importStatues => {
        if (importStatues) this.loadData().subscribe();
      });
  }

  onWareshouseChange(value: number) {
    this.filteredStockPlaces.set([]);
    this.stockProducts.set([]);
    if (value > 0) {
      this.loadStockProducts(value);
      this.setShelves(value);

      this.formSelection.patchValue({ stockId: value });
    }
  }

  stockFromProductChanged(product: IProductSmallDTO) {
    this.formSelection?.patchValue({
      productNrFrom: product.number,
    });
  }

  stockToProductChanged(product: IProductSmallDTO) {
    this.formSelection?.patchValue({
      productNrTo: product.number,
    });
  }

  generateData() {
    const selection = this.formSelection.value as StockInventoryFilterDTO;
    selection.productNrFrom =
      this.stockProducts().find(i => selection.productNrFromId === i.productId)
        ?.number || '';
    selection.productNrTo =
      this.stockProducts().find(i => selection.productNrToId === i.productId)
        ?.number || '';

    this.performLoadData
      .load$(this.service.generateStockInventoryRows(selection))
      .subscribe(x => {
        this._generatedData = x;
        this.setGridData(x || []);
        if (x.length === 0) {
          this.messageboxService.error(
            this._terms['common.status'],
            this._terms['billing.stock.stockinventory.norowsgenerated']
          );
        }
        this.form?.hasGeneratedRows.patchValue(x.length > 0 ? true : undefined);
      });
  }

  enableItemUpdate(value: boolean): void {
    this.disableUpdateButton.set(!value);
  }

  peformUpdates(selected: MenuButtonItem): void {
    switch (selected.id) {
      case UpdateFunctionType.UpdateStockQuantity:
        this.editItemGrid.updateQuantity();
        break;
      case UpdateFunctionType.UpdateTransactionDate:
        this.editItemGrid.setTransactionDate();
        break;
    }
  }

  private openReportDialog(): void {
    const dialogData = new SelectReportDialogData();
    dialogData.title = 'common.selectreport';
    dialogData.size = 'lg';
    dialogData.reportTypes = [SoeReportTemplateType.StockInventoryReport];
    dialogData.showCopy = false;
    dialogData.showEmail = false;
    dialogData.copyValue = false;
    dialogData.reports = [];
    dialogData.defaultReportId = 0;
    dialogData.langId = SoeConfigUtil.languageId;
    dialogData.showReminder = false;
    dialogData.showLangSelection = false;
    dialogData.showSavePrintout = false;
    dialogData.savePrintout = false;
    const selectReportDialog = this.dialogServicev2.open(
      SelectReportDialogComponent,
      dialogData
    );

    selectReportDialog
      .afterClosed()
      .subscribe((result: SelectReportDialogCloseData) => {
        if (
          result &&
          result.reportId &&
          this.form?.value.stockInventoryHeadId
        ) {
          this.printButtonDisabled.set(true);
          this.perform.load(
            this.requestReportService
              .printStockInventory(
                result.reportId,
                this.form?.value.stockInventoryHeadId
              )
              .pipe(
                tap(() => {
                  this.printButtonDisabled.set(false);
                })
              )
          );
        }
      });
  }

  //#endregion

  //#region HELPERS

  private setShelves(stockId: number): void {
    this.filteredStockPlaces.set(
      (this.shelves as IStockShelfDTO[])
        .filter(x => x.stockId == stockId)
        .map(x => <SmallGenericType>{ id: x.stockShelfId, name: x.name })
    );
  }

  private setGridData(rows: IStockInventoryRowDTO[]) {
    this.rowData.next(rows);
  }

  private disableControls(): void {
    this.form?.headerText.disable();
    this.form?.stockId.disable();
  }

  private setImportButtonVisibility(headId?: number, inventoryStop?: Date) {
    this.hideImport.set(
      !(headId !== undefined && headId > 0 && !inventoryStop)
    );
  }

  private buildUpdateFunctionList(): void {
    this.menuList.push({
      id: UpdateFunctionType.UpdateTransactionDate,
      label: this.translate.instant(
        'billing.stock.stockinventory.updatetransactiondate'
      ),
    });
    this.menuList.push({
      id: UpdateFunctionType.UpdateStockQuantity,
      label: this.translate.instant(
        'billing.stock.stockinventory.updateinventoryquantity'
      ),
    });
  }
  //#endregion

  ngOnDestroy(): void {
    this._destroy$.next();
    this._destroy$.complete();
  }
}

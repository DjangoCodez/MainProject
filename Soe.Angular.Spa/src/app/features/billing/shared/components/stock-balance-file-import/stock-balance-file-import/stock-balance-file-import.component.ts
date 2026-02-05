import { Component, OnInit, inject } from '@angular/core';
import { Validators } from '@angular/forms';
import { TranslateService } from '@ngx-translate/core';
import { FileUploadDialogComponent } from '@shared/components/file-upload-dialog/file-upload-dialog.component';
import { fileUploadDialogData } from '@shared/components/file-upload-dialog/models/file-upload-dialog.model';
import { SoeFormGroup } from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { IActionResult } from '@shared/models/generated-interfaces/ActionResult';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import { FlowHandlerService } from '@shared/services/flow-handler.service'
import { ProgressService } from '@shared/services/progress/progress.service';
import { Perform } from '@shared/util/perform.class';
import { DialogComponent } from '@ui/dialog/dialog/dialog.component'
import { MessageboxService } from '@ui/dialog/services/messagebox.service';
import { forkJoin, of, tap } from 'rxjs';
import { StockBalanceService } from '../../../../stock-balance/services/stock-balance.service';
import {
  ImportStockBalancesDTO,
  StockBalanceFileImportDialogData,
} from './models/stock-balance-file-import.model';
import { StockBalanceImportForm } from './models/stock-balance-import-form.model';
import { StockBalanceFileImportService } from './services/stock-balance-file-import.service';
import { DialogService } from '@ui/dialog/services/dialog.service';

@Component({
  selector: 'soe-stock-balance-file-import',
  templateUrl: './stock-balance-file-import.component.html',
  providers: [FlowHandlerService],
  standalone: false,
})
export class StockBalanceFileImportComponent
  extends DialogComponent<StockBalanceFileImportDialogData>
  implements OnInit
{
  fromInventory = false;
  validationHandler = inject(ValidationHandler);
  handler = inject(FlowHandlerService);
  progressService = inject(ProgressService);
  service = inject(StockBalanceFileImportService);
  serviceStockBalance = inject(StockBalanceService);
  translate = inject(TranslateService);
  messageBoxService = inject(MessageboxService);
  private readonly dialogService = inject(DialogService);
  performWholeSellersLoad = new Perform<ISmallGenericType[]>(
    this.progressService
  );
  performStockLoad = new Perform<ISmallGenericType[]>(this.progressService);
  performFileImport = new Perform<IActionResult>(this.progressService);
  isImportSucess = false;

  form: StockBalanceImportForm = new StockBalanceImportForm({
    validationHandler: this.validationHandler,
    element: new ImportStockBalancesDTO(),
  });

  constructor() {
    super();
    this.setControlDisabled();
    this.setDialogParam();
  }

  ngOnInit() {
    this.handler.execute({
      lookups: [this.loadLookupData()],
    });

    this.setConditionalValidators();
  }

  loadLookupData() {
    if (!this.fromInventory) {
      return forkJoin([this.loadWholeSellers(), this.loadStocks()]);
    } else {
      return of(true);
    }
  }

  setControlDisabled() {
    this.form.fileName.disable();
  }

  setDialogParam() {
    let stockInventoryHeadId = 0;
    if (
      this.data &&
      this.data.stockInventoryHeadId &&
      this.data.stockInventoryHeadId > 0
    ) {
      this.fromInventory = true;
      stockInventoryHeadId = this.data.stockInventoryHeadId;
    }
    this.form.patchValue({
      stockInventoryHeadId: stockInventoryHeadId,
    });
  }

  setConditionalValidators() {
    this.form.stockId.clearValidators();
    if (!this.fromInventory) {
      this.form.stockId.setValidators(Validators.required);
    }
    this.form.stockId.updateValueAndValidity();
  }

  loadWholeSellers() {
    return this.performWholeSellersLoad.load$(
      this.serviceStockBalance.getSmallGenericSysWholesellers(true)
    );
  }

  loadStocks() {
    return this.performStockLoad.load$(
      this.serviceStockBalance.getStocksDict(false)
    );
  }

  openFormValidationErrors(): void {
    this.validationHandler.showFormValidationErrors(this.form as SoeFormGroup);
  }

  openFileUpload() {
    const fileDialog = this.dialogService.open(
      FileUploadDialogComponent,
      fileUploadDialogData({
        asBinary: false,
        multipleFiles: false,
      })
    );

    fileDialog.afterClosed().subscribe(res => {
      this.form.patchValue({
        fileName: res.name,
        fileString: res.content,
      });
    });
  }

  cancel() {
    this.dialogRef.close(this.isImportSucess);
  }

  import() {
    if (this.fromInventory) {
      this.performFileImport.load(
        this.service
          .importStockInventory(this.form.getRawValue())
          .pipe(tap(result => this.handleResult(result)))
      );
    } else {
      this.performFileImport.load(
        this.service
          .importStockBalances(this.form.getRawValue())
          .pipe(tap(result => this.handleResult(result)))
      );
    }
  }

  private handleResult(result: IActionResult) {
    if (result.success) {
      this.isImportSucess = result.success;
      this.cancel();
    } else {
      this.messageBoxService.error(
        this.translate.instant('common.status'),
        result.errorMessage
      );
    }
  }
}

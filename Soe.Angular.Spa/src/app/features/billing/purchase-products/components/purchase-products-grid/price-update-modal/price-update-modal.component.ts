import {
  AfterViewInit,
  ChangeDetectorRef,
  Component,
  OnInit,
} from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { SoeFormGroup } from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { ProgressService } from '@shared/services/progress/progress.service'
import { SharedService } from '@shared/services/shared.service';
import { Perform } from '@shared/util/perform.class';
import { DialogComponent } from '@ui/dialog/dialog/dialog.component'
import { DialogData } from '@ui/dialog/models/dialog'
import { IMessageboxComponentResponse } from '@ui/dialog/models/messagebox'
import { MessageboxService } from '@ui/dialog/services/messagebox.service';
import { forkJoin } from 'rxjs';
import { CrudActionTypeEnum } from '../../../../../../shared/enums';
import { PriceUpdateForm, PriceUpdateModel } from './price-update-form.model';
import { PriceUpdateModalService } from './price-update-modal.service';

export interface PriceUpdateDialogData extends DialogData {
  selectedRows: number[];
}

@Component({
  selector: 'soe-price-update-modal',
  templateUrl: './price-update-modal.component.html',
  standalone: false,
})
export class PriceUpdateComponent
  extends DialogComponent<PriceUpdateDialogData>
  implements OnInit, AfterViewInit
{
  form: SoeFormGroup | undefined;
  performCurrencies = new Perform<SmallGenericType[]>(this.progressService);
  performRoundingOptions = new Perform<SmallGenericType[]>(
    this.progressService
  );
  performAction = new Perform<PriceUpdateModalService>(this.progressService);
  selectedSupplierProductsText = '';

  constructor(
    private cdr: ChangeDetectorRef,
    private progressService: ProgressService,
    private messageboxService: MessageboxService,
    private validationHandler: ValidationHandler,
    private priceUpdateService: PriceUpdateModalService,
    private translateService: TranslateService,
    private sharedService: SharedService
  ) {
    super();
    this.setData(this.data.selectedRows);
  }

  ngAfterViewInit(): void {
    this.cdr.detectChanges();
  }

  ngOnInit(): void {
    forkJoin([
      this.performCurrencies.load$(this.sharedService.getCurrencies()),
      this.performRoundingOptions.load$(
        this.priceUpdateService.getRoundingOptions()
      ),
    ]).subscribe(() => {
      if (this.form && this.performCurrencies.data)
        this.form.controls.currencyId.patchValue(
          this.performCurrencies.data[0].id
        );
    });
  }

  setData(ids: number[]) {
    this.form = new PriceUpdateForm({
      validationHandler: this.validationHandler,
      priceUpdateModel: new PriceUpdateModel(ids),
    });
    this.selectedSupplierProductsText = this.translateService
      .instant('billing.purchase.product.selectedproducts')
      .replace('{0}', ids.length.toString());
  }

  triggerPriceUpdate() {
    const mb = this.messageboxService.warning(
      'core.continue',
      'billing.product.pricelist.areyousure'
    );
    mb.afterClosed().subscribe((response: IMessageboxComponentResponse) => {
      if (response?.result) this.performPriceUpdate();
    });
  }

  performPriceUpdate() {
    this.performAction.crud(
      CrudActionTypeEnum.Save,
      this.priceUpdateService.performPriceUpdate(this.form?.value),
      undefined,
      undefined,
      {
        showDialogOnError: true,
        showToastOnError: true,
        failIfNoObjectsAffected: true,
      }
    );
    this.dialogRef.close(true);
  }

  triggerCancel() {
    this.dialogRef.close(false);
  }
  openFormValidationErrors(): void {
    this.validationHandler.showFormValidationErrors(this.form as SoeFormGroup);
  }
}

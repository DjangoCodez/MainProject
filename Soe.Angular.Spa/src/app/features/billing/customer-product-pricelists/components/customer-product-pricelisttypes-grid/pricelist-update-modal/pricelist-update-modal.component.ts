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
import { FlowHandlerService } from '@shared/services/flow-handler.service'
import { ProgressService } from '@shared/services/progress/progress.service';
import { Perform } from '@shared/util/perform.class';
import { DialogComponent } from '@ui/dialog/dialog/dialog.component'
import { DialogData } from '@ui/dialog/models/dialog'
import { IMessageboxComponentResponse } from '@ui/dialog/models/messagebox'
import { MessageboxService } from '@ui/dialog/services/messagebox.service';
import { CrudActionTypeEnum } from '../../../../../../shared/enums';
import {
  PriceUpdateForm,
  PriceUpdateModel,
} from './pricelist-update-form.model';
import { PricelistUpdateModalService } from './pricelist-update-modal.service';

export interface PriceListUpdateDialogData extends DialogData {
  selectedRows: number[];
}

@Component({
  selector: 'soe-pricelist-update-modal',
  templateUrl: './pricelist-update-modal.component.html',
  providers: [FlowHandlerService, ValidationHandler],
  standalone: false,
})
export class PriceListUpdateComponent
  extends DialogComponent<PriceListUpdateDialogData>
  implements OnInit, AfterViewInit
{
  form: SoeFormGroup | undefined;
  performVatTypes = new Perform<SmallGenericType[]>(this.progressService);
  performMaterialCodes = new Perform<SmallGenericType[]>(this.progressService);
  performProductGroups = new Perform<SmallGenericType[]>(this.progressService);
  performRoundingOptions = new Perform<SmallGenericType[]>(
    this.progressService
  );
  performAction = new Perform<PricelistUpdateModalService>(
    this.progressService
  );
  selectedPricelistsText = '';

  constructor(
    public handler: FlowHandlerService,
    private cdr: ChangeDetectorRef,
    private progressService: ProgressService,
    private messageboxService: MessageboxService,
    private validationHandler: ValidationHandler,
    private pricelistUpdateService: PricelistUpdateModalService,
    private translateService: TranslateService
  ) {
    super();
    this.setData(this.data.selectedRows);
  }

  ngAfterViewInit(): void {
    this.cdr.detectChanges();
  }

  ngOnInit(): void {
    this.performMaterialCodes.load(
      this.pricelistUpdateService.getMaterialCodes()
    );
    this.performVatTypes.load(this.pricelistUpdateService.getProductVatTypes());
    this.performRoundingOptions.load(
      this.pricelistUpdateService.getRoundingOptions()
    );
    this.performProductGroups.load(
      this.pricelistUpdateService.getProductGroups()
    );
  }

  setData(ids: number[]) {
    this.form = new PriceUpdateForm({
      validationHandler: this.validationHandler,
      priceUpdateModel: new PriceUpdateModel(ids),
    });
    this.selectedPricelistsText = this.translateService
      .instant('billing.product.pricelist.selectedpricelists')
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
    if (
      this.form?.value.priceUpdate.rounding &&
      this.form?.value.priceUpdate.decimalCount > 1
    ) {
      let dec = 0.1;

      //Can be calculated by x ** -decimalCount but comes with rounding errors.
      switch (this.form?.value.priceUpdate.decimalCount) {
        case 2:
          dec = 0.01;
          break;
        case 3:
          dec = 0.001;
          break;
        case 4:
          dec = 0.0001;
          break;
      }

      this.form.value.priceUpdate.rounding = dec;
    }

    this.performAction.crud(
      CrudActionTypeEnum.Save,
      this.pricelistUpdateService.performPriceUpdate(this.form?.value),
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

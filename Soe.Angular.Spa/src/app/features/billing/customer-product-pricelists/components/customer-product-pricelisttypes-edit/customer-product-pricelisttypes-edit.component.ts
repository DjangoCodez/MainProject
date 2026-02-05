import { Component, Input, OnInit, inject } from '@angular/core';
import { CrudActionTypeEnum } from '@shared/enums';
import { SoeFormGroup } from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { SharedService } from '@shared/services/shared.service';
import { Perform } from '@shared/util/perform.class';
import { Observable, of } from 'rxjs';
import { tap } from 'rxjs/operators';
import { CustomerProductPricelistsTypeService } from '../../services/customer-product-priceliststype.service';
import { CustomerProductPriceListsService } from '../customer-product-pricelists/services/customer-product-pricelists.service';
import { PriceListTypeDTO } from '../../models/customer-product-pricelist.model';
import { EditBaseDirective } from '@shared/directives/edit-base/edit-base.directive';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { PriceListDTO } from '@features/billing/models/pricelist.model';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';
import { ResponseUtil } from '@shared/util/response-util';

@Component({
  selector: 'soe-customer-product-pricelisttypes-edit',
  templateUrl: './customer-product-pricelisttypes-edit.component.html',
  providers: [
    FlowHandlerService,
    ToolbarService,
    CustomerProductPriceListsService,
  ],
  standalone: false,
})
export class CustomerProductPriceListTypesEditComponent
  extends EditBaseDirective<
    PriceListTypeDTO,
    CustomerProductPricelistsTypeService
  >
  implements OnInit
{
  @Input() records: SmallGenericType[] = [];
  @Input() form: SoeFormGroup | undefined;

  performCurrencies = new Perform<SmallGenericType[]>(this.progressService);
  performAction = new Perform<CustomerProductPricelistsTypeService>(
    this.progressService
  );
  readonly priceListsService = inject(CustomerProductPriceListsService);
  readonly service = inject(CustomerProductPricelistsTypeService);
  readonly sharedService = inject(SharedService);
  readonly validationHandler = inject(ValidationHandler);

  isRowsLoaded = false;

  protected get priceListTypeId() {
    return this.form?.value[this.idFieldName];
  }

  ngOnInit() {
    super.ngOnInit();
    this.startFlow(
      Feature.Billing_Preferences_InvoiceSettings_Pricelists_Edit,
      { lookups: [this.loadCurrencies()] }
    );
  }

  override copy() {
    super.copy();
  }

  override newRecord(): Observable<void> {
    const clearValues = () => {
      this.form?.patchValue({ name: '' });
    };

    return of(clearValues());
  }

  override loadData(): Observable<void> {
    const id = this.form?.getIdControl()?.value;
    if (id) {
      return this.performLoadData.load$(
        this.service.get(this.form?.getIdControl()?.value).pipe(
          tap(value => {
            this.form?.patchValue(value);
            this.loadPriceLists();
          })
        )
      );
    } else {
      return this.newRecord();
    }
  }

  performSave(): void {
    if (!this.form || this.form.invalid) return;

    this.performAction.crud(
      CrudActionTypeEnum.Save,
      this.service
        .save(this.form?.value, this.priceListsService.getDataForSave())
        .pipe(tap(this.updateFormValueAndEmitChange))
    );
  }

  updateFormValueAndEmitChange = (backendResponse: BackendResponse) => {
    if (!backendResponse.success) return;

    const entityId = ResponseUtil.getEntityId(backendResponse);
    if (entityId) {
      this.form?.patchValue({ [this.idFieldName]: entityId });
    }
    if (backendResponse.success) {
      this.loadPriceLists();
    }
    this.form?.markAsUntouched();
    this.form?.markAsPristine();
    this.actionTakenSignal().set({
      rowItemId: entityId,
      ref: this.ref(),
      form: this.form,
      type: CrudActionTypeEnum.Save,
      additionalProps: this.additionalSaveProps,
    });
  };

  triggerError(message: string, title?: string) {
    this.messageboxService.error(
      title || this.translate.instant('core.error'),
      message
    );
  }

  openFormValidationErrors(): void {
    this.validationHandler.showFormValidationErrors(this.form as SoeFormGroup);
  }

  loadCurrencies() {
    return of(this.performCurrencies.load(this.sharedService.getCurrencies()));
  }

  onPriceListRowsOpened() {
    if (this.priceListTypeId && !this.isRowsLoaded) {
      this.loadPriceLists();
    }
  }

  loadPriceLists(): void {
    if (this.priceListTypeId) {
      return this.performLoadData.load(
        this.service.getPriceLists(this.priceListTypeId).pipe(
          tap(data => {
            this.isRowsLoaded = true;
            this.priceListsService.setData(
              data.map(x => PriceListDTO.fromServer(x))
            );
          })
        )
      );
    } else {
      this.isRowsLoaded = true;
    }
  }
}

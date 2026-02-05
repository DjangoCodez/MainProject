import { Component, inject, Input, OnDestroy, OnInit } from '@angular/core';
import { DialogComponent } from '@ui/dialog/dialog/dialog.component';
import {
  CustomerSearchModelDTO,
  SelectCustomerGridFormDTO,
  SelectCustomerSearchDialogData,
} from '../../models/select-customer-dialog.model';
import { BehaviorSubject, tap } from 'rxjs';
import { ICustomerSearchModel } from '@shared/models/generated-interfaces/CoreModels';
import { FlowHandlerService } from '@shared/services/flow-handler.service'
import { ProgressService } from '@shared/services/progress/progress.service';
import { SelectCustomerService } from '../../services/select-customer.service';
import { Perform } from '@shared/util/perform.class';
import { ValidationHandler } from '@shared/handlers';
import { SelectCustomerSearchForm } from '../../models/select-customer-search-form.model';
import { SoeOriginType } from '@shared/models/generated-interfaces/Enumerations';

@Component({
  selector: 'soe-select-customer-dialog',
  templateUrl: './select-customer-dialog.component.html',
  providers: [FlowHandlerService],
  standalone: false,
})
export class SelectCustomerDialogComponent
  extends DialogComponent<SelectCustomerSearchDialogData>
  implements OnInit, OnDestroy
{
  @Input() currentMainCustomerId: any;

  // data load
  handler = inject(FlowHandlerService);
  progressService = inject(ProgressService);
  selectCustomerService = inject(SelectCustomerService);
  performCustomers = new Perform<ICustomerSearchModel[]>(this.progressService);
  isSearching: boolean = false;
  customerData = new BehaviorSubject<ICustomerSearchModel[]>([]);

  validationHandler = inject(ValidationHandler);
  form: SelectCustomerSearchForm = new SelectCustomerSearchForm({
    validationHandler: this.validationHandler,
    element: new SelectCustomerGridFormDTO(),
  });

  originType = SoeOriginType.None;
  selectedCustomers: CustomerSearchModelDTO[] | undefined;

  ngOnInit(): void {}

  ngOnDestroy(): void {
    this.customerData.unsubscribe();
  }

  loadCustomers(model: CustomerSearchModelDTO) {
    if (!this.isValidSearchModel(model)) {
      this.customerData.next([]);
      this.isSearching = false;
      return;
    }
    this.isSearching = true;
    this.performCustomers.load(
      this.selectCustomerService.getCustomersBySearch(model).pipe(
        tap(value => {
          this.customerData.next(value);
          this.isSearching = false;
        })
      )
    );
  }

  cancel() {
    this.dialogRef.close(this.data.customerValue);
  }

  protected ok(): void {
    if (this.selectedCustomers) {
      this.dialogRef.close(this.selectedCustomers);
    } else this.cancel();
  }

  changeSelection(event: any) {
    this.selectedCustomers = event;
  }

  onRowDoubleClicked(data: any) {
    this.selectedCustomers = data;
    this.ok();
  }

  filterChange(value: any): void {
    const model = new CustomerSearchModelDTO();
    model.name = value.name ? value.name?.filter : '';
    model.customerNr = value.customerNr ? value.customerNr?.filter : '';
    model.billingAddress = value.billingAddress
      ? value.billingAddress?.filter
      : '';
    model.deliveryAddress = value.deliveryAddress
      ? value.deliveryAddress.filter
      : '';
    model.note = value.note ? value.note.filter : '';

    if (!this.isSearching) {
      this.loadCustomers(model);
    }
  }

  private isValidSearchModel(model: CustomerSearchModelDTO): boolean {
    return !!(
      model.name ||
      model.customerNr ||
      model.billingAddress ||
      model.deliveryAddress ||
      model.note
    );
  }
}

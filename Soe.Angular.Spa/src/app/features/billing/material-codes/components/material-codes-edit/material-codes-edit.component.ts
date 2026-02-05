import { Component, OnInit, ViewChild, inject } from '@angular/core';
import { EditBaseDirective } from '@shared/directives/edit-base/edit-base.directive';
import {
  Feature,
  SoeEntityState,
  SoeTimeCodeType,
  TermGroup_ExpenseType,
} from '@shared/models/generated-interfaces/Enumerations';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { Observable, of } from 'rxjs';
import { tap } from 'rxjs/operators';
import { MaterialCodesEditItemGridComponent } from '../material-codes-edit-item-grid/material-codes-edit-item-grid.component';
import { CrudActionTypeEnum } from '@shared/enums';
import { TimeCodeMaterialDTO } from '../../models/material-codes.model';
import { TimeCodeMaterialsForm } from '../../models/material-codes-form.model';
import { MaterialCodesService } from '../../services/material-codes.service';
import { IRowNode } from 'ag-grid-community';
import { ITimeCodeInvoiceProductDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';

@Component({
  selector: 'soe-material-codes-edit',
  templateUrl: './material-codes-edit.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class MaterialCodesEditComponent
  extends EditBaseDirective<
    TimeCodeMaterialDTO,
    MaterialCodesService,
    TimeCodeMaterialsForm
  >
  implements OnInit
{
  service = inject(MaterialCodesService);
  timeCodeType = SoeTimeCodeType.Material;
  expenseType = TermGroup_ExpenseType.Unknown;
  @ViewChild(MaterialCodesEditItemGridComponent)
  materialProductGrid!: MaterialCodesEditItemGridComponent;

  ngOnInit() {
    super.ngOnInit();

    this.startFlow(
      Feature.Billing_Preferences_ProductSettings_MaterialCode_Edit
    );

    //Ensure to apply edited data in the grid when clicking outside
    document.body.addEventListener('click', (event: any) => {
      if (event.target.closest('ag-grid-angular') == null) {
        this.applyChanges();
      }
    });
  }

  override loadData(): Observable<void> {
    return this.performLoadData.load$(
      this.service.get(this.form?.getIdControl()?.value).pipe(
        tap((value: TimeCodeMaterialDTO) => {
          this.form?.customPatchValue(value);
        })
      )
    );
  }

  override newRecord(): Observable<void> {
    let clearValues = () => {};

    if (this.form?.isNew || this.form?.isCopy) {
      this.form?.state.setValue(SoeEntityState.Active);
    }

    if (this.form?.isCopy) {
      clearValues = () => {
        this.form?.onDoCopy();
      };
    }

    return of(clearValues());
  }

  activeChanged(value: boolean) {
    if (this.form) {
      this.form.patchValue({ isActive: value });
      this.form.markAsDirty();
    }
  }

  applyChanges(): void {
    this.materialProductGrid?.grid?.applyChanges();
  }

  override performSave(): void {
    this.applyChanges();
    const products: ITimeCodeInvoiceProductDTO[] = [];
    this.materialProductGrid?.grid?.api.forEachNode(
      (row: IRowNode<ITimeCodeInvoiceProductDTO>, _) => {
        products.push(row.data as ITimeCodeInvoiceProductDTO);
      }
    );
    this.form?.invoiceProducts.clear();
    this.form?.patchInvoiceProducts(products);

    if (!this.form || this.form.invalid || !this.service) return;
    this.performAction.crud(
      CrudActionTypeEnum.Save,
      this.service.save(this.form?.getAllValues()).pipe(
        tap(res => {
          this.updateFormValueAndEmitChange(res);
          if (res.success) this.triggerCloseDialog(res);
        })
      )
    );
  }
}

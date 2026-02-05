import { Component, inject, OnInit } from '@angular/core';
import { EditBaseDirective } from '@shared/directives/edit-base/edit-base.directive';
import {
  Feature,
  TermGroup,
} from '@shared/models/generated-interfaces/Enumerations';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { UnionFeesService } from '../../services/union-fees.service';
import { UnionFeesForm } from '../../models/union-fees-form.model';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { IUnionFeeDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { IProductSmallDTO } from '@shared/models/generated-interfaces/ProductDTOs';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import { Observable, tap } from 'rxjs';

@Component({
  selector: 'soe-union-fees-edit',
  templateUrl: './union-fees-edit.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class UnionFeesEditComponent
  extends EditBaseDirective<IUnionFeeDTO, UnionFeesService, UnionFeesForm>
  implements OnInit
{
  service = inject(UnionFeesService);
  coreService = inject(CoreService);

  payrollPriceTypes: ISmallGenericType[] = [];
  unionFeePayrollProducts: IProductSmallDTO[] = [];
  associations: ISmallGenericType[] = [];

  ngOnInit() {
    super.ngOnInit();
    this.startFlow(Feature.Time_Payroll_UnionFee, {
      lookups: [
        this.loadPayrollPriceTypes(),
        this.loadUnionFeePayrollProducts(),
        this.loadAssociations(),
      ],
    });
  }

  loadPayrollPriceTypes(): Observable<void> {
    return this.performLoadData.load$(
      this.service.getPayrollPriceTypesDict().pipe(
        tap((value: ISmallGenericType[]) => {
          this.payrollPriceTypes = value;
        })
      )
    );
  }

  loadUnionFeePayrollProducts(): Observable<void> {
    return this.performLoadData.load$(
      this.service.getUnionFeePayrollProducts().pipe(
        tap((value: IProductSmallDTO[]) => {
          this.unionFeePayrollProducts = value;
        })
      )
    );
  }

  loadAssociations(): Observable<void> {
    return this.performLoadData.load$(
      this.coreService
        .getTermGroupContent(TermGroup.UnionFeeAssociation, false, false, true)
        .pipe(tap(value => (this.associations = value)))
    );
  }

  payrollPriceTypeIdPercentChanged(value: number): void {
    if (value === 0) {
      this.form?.controls.payrollPriceTypeIdPercentCeiling.setValue(0);
    }
  }
}

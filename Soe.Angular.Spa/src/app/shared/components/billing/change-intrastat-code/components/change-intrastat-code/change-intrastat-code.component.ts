import { Component, inject } from '@angular/core';
import { DialogComponent } from '@ui/dialog/dialog/dialog.component';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { CoreService } from '@shared/services/core.service'
import { FlowHandlerService } from '@shared/services/flow-handler.service'
import { ProgressService } from '@shared/services/progress/progress.service';
import { Perform } from '@shared/util/perform.class';
import {
  SoeEntityState,
  SoeOriginType,
} from '@shared/models/generated-interfaces/Enumerations';
import { BehaviorSubject } from 'rxjs';
import { ChangeIntrastatCodeDialogData } from '../../models/change-intrastat-code.model';
import { ChangeIntrastatCodeService } from '../../services/change-intrastat-code.service';
import { ISaveIntrastatTransactionModel } from '@shared/models/generated-interfaces/BillingModels';
import { CrudActionTypeEnum } from '@shared/enums';

@Component({
  selector: 'soe-change-intrastat-code',
  templateUrl: './change-intrastat-code.component.html',
  providers: [FlowHandlerService],
  standalone: false,
})
export class ChangeIntrastatCodeComponent extends DialogComponent<ChangeIntrastatCodeDialogData> {
  private readonly progressService = inject(ProgressService);
  private readonly coreService = inject(CoreService);
  private readonly service = inject(ChangeIntrastatCodeService);
  performAction = new Perform<ISaveIntrastatTransactionModel>(
    this.progressService
  );

  protected performLoadCode = new Perform<SmallGenericType[]>(
    this.progressService
  );

  protected performLoadTransactionType = new Perform<SmallGenericType[]>(
    this.progressService
  );
  protected performLoadCountry = new Perform<SmallGenericType[]>(
    this.progressService
  );

  restAmount = 0;
  totalAmount = 0;
  workingMessage = 0;
  originType: SoeOriginType | undefined;
  transactions: any;
  originId: any;
  rows = new BehaviorSubject<any[]>([]);
  form: any;

  constructor() {
    super();
  }

  buttonOkClick() {
    const model = <any>this.form?.value;
    this.performAction.crud(
      CrudActionTypeEnum.Save,
      this.service.save(model)
      // .pipe(tap(this.updateFormValueAndEmitChange))
    );
  }

  buttonEnabled() {
    if (
      this.transactions.some(
        (r: {
          isModified: any;
          notIntrastat: any;
          intrastatCodeId: any;
          intrastatTransactionType: any;
        }) =>
          (r.isModified && !r.notIntrastat && !r.intrastatCodeId) ||
          !r.intrastatTransactionType
      )
    ) {
      return false;
    } else {
      return this.originType === SoeOriginType.SupplierInvoice
        ? this.transactions.some(
            (r: { state: SoeEntityState; isModified: any }) =>
              r.state !== SoeEntityState.Deleted && r.isModified
          ) && this.restAmount === 0
        : this.transactions.some(
            (t: { state: SoeEntityState; isModified: any }) =>
              t.state !== SoeEntityState.Deleted && t.isModified
          );
    }
  }
}

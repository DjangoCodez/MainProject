import { Component, EventEmitter, Output, inject } from '@angular/core';
import { ValidationHandler } from '@shared/handlers';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import { PurchaseFilterForm } from '../../models/purchase-filter-form.model';
import {
  PurchaseFilterDTO,
  SaveUserCompanySettingModel,
} from '../../models/purchase.model';
import { CoreService } from '@shared/services/core.service'
import { FlowHandlerService } from '@shared/services/flow-handler.service'
import { ProgressService } from '@shared/services/progress/progress.service';
import { Perform } from '@shared/util/perform.class'
import { SettingsUtil } from '@shared/util/settings-util';
import {
  Feature,
  SettingMainType,
  UserSettingType,
  SoeOriginStatus,
  TermGroup,
} from '@shared/models/generated-interfaces/Enumerations';
import { of } from 'rxjs';
import { PurchaseService } from '../../services/purchase.service';
import { SmallGenericType } from '@shared/models/generic-type.model';

@Component({
  selector: 'soe-purchase-grid-header',
  templateUrl: './purchase-grid-header.component.html',
  providers: [FlowHandlerService],
  standalone: false,
})
export class PurchaseGridHeaderComponent {
  validationHandler = inject(ValidationHandler);
  progressService = inject(ProgressService);
  service = inject(PurchaseService);
  coreService = inject(CoreService);
  performLoadPurchaseStatus = new Perform<any>(this.progressService);
  performLoadSelectionTypes = new Perform<any>(this.progressService);

  @Output() filterChange = new EventEmitter<PurchaseFilterDTO>();
  @Output() filterReady = new EventEmitter<PurchaseFilterDTO>();

  defaultStatusList: number[] = [
    SoeOriginStatus.Origin,
    SoeOriginStatus.PurchaseDone,
    SoeOriginStatus.PurchaseSent,
    SoeOriginStatus.PurchaseAccepted,
  ];
  selectedPurchaseStatusIds: number[] = [];
  purchaseStatus: ISmallGenericType[] = [];
  allItemsSelectionDict: SmallGenericType[] = [];

  formFilter: PurchaseFilterForm = new PurchaseFilterForm({
    validationHandler: this.validationHandler,
    element: new PurchaseFilterDTO(),
  });

  constructor(public handler: FlowHandlerService) {
    this.handler.execute({
      permission: Feature.Billing_Purchase_Purchase_List,
      lookups: [this.loadPurchaseStatus(), this.loadSelectionTypes()],
      onFinished: this.finished.bind(this),
    });
  }

  finished() {
    const filterDto = this.formFilter.value as PurchaseFilterDTO;
    this.filterReady.emit(filterDto);
    this.loadUserSettings();
  }

  allItemsSelectionChanged() {
    this.emitFilterOnChange();
    this.updateItemSelection();
  }

  purchaseStatusSelectionComplete() {
    this.emitFilterOnChange();
  }

  emitFilterOnChange() {
    const filterDto = this.formFilter.value as PurchaseFilterDTO;
    this.filterChange.emit(filterDto);
  }

  loadSelectionTypes() {
    return of(
      this.performLoadSelectionTypes.load(
        this.coreService.getTermGroupContent(
          TermGroup.ChangeStatusGridAllItemsSelection,
          false,
          true,
          true
        )
      )
    );
  }

  loadPurchaseStatus() {
    return of(
      this.performLoadPurchaseStatus.load(this.service.getPurchaseStatus())
    );
  }

  private loadUserSettings() {
    const settingTypes: number[] = [
      UserSettingType.BillingPurchaseAllItemsSelection,
    ];
    return this.coreService
      .getUserSettings(settingTypes, false)
      .subscribe(x => {
        this.formFilter.allItemsSelection.setValue(
          SettingsUtil.getIntUserSetting(
            x,
            UserSettingType.BillingPurchaseAllItemsSelection,
            1,
            false
          )
        );
      });
  }

  updateItemSelection() {
    const updateModel: SaveUserCompanySettingModel =
      new SaveUserCompanySettingModel(
        SettingMainType.User,
        UserSettingType.BillingPurchaseAllItemsSelection,
        this.formFilter.allItemsSelection.value
      );
    this.coreService.saveIntSetting(updateModel).subscribe();
  }
}

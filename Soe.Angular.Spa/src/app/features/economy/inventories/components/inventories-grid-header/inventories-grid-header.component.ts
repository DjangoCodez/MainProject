import { Component, EventEmitter, OnInit, Output, inject } from '@angular/core';
import { ValidationHandler } from '@shared/handlers';
import { CoreService } from '@shared/services/core.service'
import { FlowHandlerService } from '@shared/services/flow-handler.service'
import { ProgressService } from '@shared/services/progress/progress.service';
import { Constants } from '@shared/util/client-constants'
import { Perform } from '@shared/util/perform.class'
import { SettingsUtil } from '@shared/util/settings-util';
import {
  SettingMainType,
  TermGroup,
  UserSettingType,
} from '@shared/models/generated-interfaces/Enumerations';
import { tap } from 'rxjs';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { InventoriesFilterForm } from '../../models/inventories-filter-form.model';
import { InventoryFilterDTO } from '../../models/inventories.model';

@Component({
  selector: 'soe-inventories-grid-header',
  templateUrl: './inventories-grid-header.component.html',
  providers: [FlowHandlerService],
  standalone: false,
})
export class InventoriesGridHeaderComponent implements OnInit {
  validationHandler = inject(ValidationHandler);
  progressService = inject(ProgressService);
  coreService = inject(CoreService);

  performLoad = new Perform<any[]>(this.progressService);
  inventoryStatus!: SmallGenericType[];
  selectedStatus: Array<number> = [];

  @Output() filterChange = new EventEmitter<InventoryFilterDTO>();

  formFilter: InventoriesFilterForm = new InventoriesFilterForm({
    validationHandler: this.validationHandler,
    element: new InventoryFilterDTO(),
  });

  ngOnInit(): void {
    this.loadInventoryStatus();
    this.loadUserInventoryStatus();
  }

  private loadInventoryStatus(): void {
    this.performLoad.load(
      this.coreService
        .getTermGroupContent(TermGroup.InventoryStatus, false, true, false)
        .pipe(
          tap(statusDict => {
            this.inventoryStatus = statusDict;
          })
        )
    );
  }

  private loadUserInventoryStatus(): void {
    this.performLoad.load(
      this.coreService
        .getUserSettings([UserSettingType.InventoryPreSelectedStatuses], false)
        .pipe(
          tap(x => {
            const ids = SettingsUtil.getStringUserSetting(
              x,
              UserSettingType.InventoryPreSelectedStatuses
            );
            if (ids !== Constants.WEBAPI_STRING_EMPTY && ids.length > 0) {
              this.formFilter.selectedStatusIds.setValue(
                ids
                  .split(',')
                  .filter((s: string) => s !== Constants.WEBAPI_STRING_EMPTY)
                  .map(Number)
              );
            }
            this.gridLoadFilter(false);
          })
        )
    );
  }

  private saveStringSetting() {
    const selectedIds: Array<number> = this.formFilter.selectedStatusIds.value;
    const model = {
      settingMainType: SettingMainType.User,
      settingTypeId: UserSettingType.InventoryPreSelectedStatuses,
      stringValue:
        selectedIds.length === 0
          ? Constants.WEBAPI_STRING_EMPTY
          : selectedIds.join(','),
    };

    this.performLoad.load(this.coreService.saveStringSetting(model));
  }

  protected gridLoadFilter(saveSettings: boolean = true) {
    const filterDto = this.formFilter.value as InventoryFilterDTO;
    if (saveSettings && this.formFilter.dirty) {
      this.saveStringSetting();
    }

    if (!saveSettings || this.formFilter.dirty) {
      this.filterChange.emit(filterDto);
    }
    this.formFilter.markAsPristine();
    this.formFilter.markAsUntouched();
  }
}

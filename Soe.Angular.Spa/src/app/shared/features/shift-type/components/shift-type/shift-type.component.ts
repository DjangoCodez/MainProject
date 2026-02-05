import { Component, inject } from '@angular/core';
import { ShiftTypeGridComponent } from '../shift-type-grid/shift-type-grid.component';
import { ShiftTypeEditComponent } from '../shift-type-edit/shift-type-edit.component';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';
import { ShiftTypeForm } from '../../models/shift-type-form.model';
import { ShiftTypeParamsService } from '../../services/shift-type-params.service';

@Component({
  selector: 'soe-shift-type',
  templateUrl:
    '../../../../../shared/ui-components/tab/multi-tab-wrapper/multi-tab-wrapper-template.html',
  standalone: false,
  providers: [ShiftTypeParamsService],
})
export class ShiftTypeComponent {
  urlService = inject(ShiftTypeParamsService);

  config: MultiTabConfig[] = [
    {
      gridComponent: ShiftTypeGridComponent,
      editComponent: ShiftTypeEditComponent,
      FormClass: ShiftTypeForm,
      gridTabLabel: this.urlService.isOrder
        ? 'time.schedule.shifttype.ordershifttypes'
        : 'time.schedule.shifttype.shifttypes',
      editTabLabel: this.urlService.isOrder
        ? 'time.schedule.shifttype.ordershifttype'
        : 'time.schedule.shifttype.shifttype',
      createTabLabel: this.urlService.isOrder
        ? 'time.schedule.shifttype.ordernew'
        : 'time.schedule.shifttype.new',
    },
  ];
}

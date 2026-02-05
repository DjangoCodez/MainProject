import { Component, inject } from '@angular/core';
import { SpEmployeeDetailComponent } from './sp-employee-detail/sp-employee-detail.component';
import { SpShiftDetailComponent } from './sp-shift-detail/sp-shift-detail.component';
import { IconModule } from '@ui/icon/icon.module';
import { SpSettingService } from '../../services/sp-setting.service';
import { TranslatePipe } from '@ngx-translate/core';
import { SpDaySlotDetailComponent } from './sp-day-slot-detail/sp-day-slot-detail.component';
import { SpHourSlotDetailComponent } from './sp-hour-slot-detail/sp-hour-slot-detail.component';

@Component({
  selector: 'sp-right-content',
  imports: [
    IconModule,
    TranslatePipe,
    SpEmployeeDetailComponent,
    SpShiftDetailComponent,
    SpDaySlotDetailComponent,
    SpHourSlotDetailComponent,
  ],
  templateUrl: './sp-right-content.component.html',
  styleUrl: './sp-right-content.component.scss',
})
export class SpRightContentComponent {
  private readonly settingService = inject(SpSettingService);
  onClose() {
    this.settingService.showRightContent.set(false);
  }
}

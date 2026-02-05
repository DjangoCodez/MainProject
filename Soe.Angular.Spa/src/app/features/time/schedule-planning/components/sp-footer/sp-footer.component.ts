import { Component, inject, input } from '@angular/core';
import { SpTimeSlotSumComponent } from '../sp-time-slot-sum/sp-time-slot-sum.component';
import { SpSettingService } from '../../services/sp-setting.service';

@Component({
  selector: 'sp-footer',
  imports: [SpTimeSlotSumComponent],
  templateUrl: './sp-footer.component.html',
  styleUrl: './sp-footer.component.scss',
})
export class SpFooterComponent {
  hasScrollbar = input(false);

  readonly settingService = inject(SpSettingService);
}

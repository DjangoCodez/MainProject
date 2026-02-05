import { CommonModule } from '@angular/common';
import { Component, computed, input, output } from '@angular/core';
import { TranslatePipe } from '@ngx-translate/core';
import { IconModule } from '@ui/icon/icon.module';

@Component({
  selector: 'soe-close-button',
  templateUrl: './close-button.component.html',
  styleUrls: [
    '../../../styles/shared-styles/shared-button-styles.scss',
    '../button/button.component.scss',
  ],
  imports: [CommonModule, IconModule, TranslatePipe],
})
export class CloseButtonComponent {
  caption = input('core.close');
  tooltip = input('core.close');
  inline = input(false);
  disabled = input(false);

  action = output<Event>();

  isDisabled = computed(() => {
    return this.disabled();
  });

  onAction(action: Event): void {
    this.action.emit(action);
  }
}

import { CommonModule } from '@angular/common';
import { Component, computed, input, output } from '@angular/core';
import { IconName, IconPrefix } from '@fortawesome/fontawesome-svg-core';
import { TranslatePipe } from '@ngx-translate/core';
import { IconUtil } from '@shared/util/icon-util';
import { IconModule } from '@ui/icon/icon.module';

export type ButtonBehaviour = 'standard' | 'primary' | 'danger';

@Component({
  selector: 'soe-button',
  imports: [CommonModule, IconModule, TranslatePipe],
  templateUrl: './button.component.html',
  styleUrls: [
    '../../../styles/shared-styles/shared-button-styles.scss',
    './button.component.scss',
  ],
})
export class ButtonComponent {
  behaviour = input<ButtonBehaviour>('standard');
  caption = input('');
  tooltip = input('');
  iconPrefix = input<IconPrefix>('fal');
  iconName = input<IconName | undefined>();
  iconClass = input('');
  inline = input(false);
  disabled = input(false);
  inProgress = input(false);

  action = output<Event>();

  typeClass = computed(() => {
    return this.getTypeClass();
  });

  isDisabled = computed(() => {
    return this.disabled() || this.inProgress();
  });

  icon = computed(() =>
    this.iconName()
      ? IconUtil.createIcon(this.iconPrefix(), this.iconName())
      : undefined
  );

  onAction(action: Event): void {
    this.action.emit(action);
  }

  private getTypeClass(): string {
    switch (this.behaviour()) {
      case 'standard':
        return 'btn btn-secondary';
      case 'primary':
        return 'btn btn-primary';
      case 'danger':
        return 'btn btn-danger';
    }
  }
}

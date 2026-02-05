import { CommonModule } from '@angular/common';
import {
  Component,
  OnInit,
  computed,
  input,
  model,
  output,
  signal,
} from '@angular/core';
import {
  IconName,
  IconPrefix,
  IconProp,
} from '@fortawesome/fontawesome-svg-core';
import { TranslatePipe } from '@ngx-translate/core';
import { IconUtil } from '@shared/util/icon-util';
import { ButtonBehaviour } from '@ui/button/button/button.component'
import { IconModule } from '@ui/icon/icon.module';

@Component({
  selector: 'soe-delete-button',
  templateUrl: './delete-button.component.html',
  styleUrls: [
    '../../../styles/shared-styles/shared-button-styles.scss',
    '../button/button.component.scss',
  ],
  imports: [CommonModule, IconModule, TranslatePipe],
})
export class DeleteButtonComponent implements OnInit {
  behaviour = input<ButtonBehaviour>('danger');
  caption = input('core.delete');
  tooltip = input('core.delete');
  iconPrefix = input<IconPrefix>('fal');
  iconName = input<IconName | undefined>();
  iconClass = model('');
  showDefaultIcon = input(false);
  inline = input(false);
  disabled = input(false);
  dirty = input(false);
  inProgress = input(false);

  action = output<Event>();

  typeClass = computed(() => {
    return this.getTypeClass();
  });

  isDisabled = computed(() => {
    return this.disabled() || this.inProgress();
  });

  icon = signal<IconProp | undefined>(undefined);

  ngOnInit() {
    if (this.iconName()) {
      this.icon.set(IconUtil.createIcon(this.iconPrefix(), this.iconName()));
    } else if (this.showDefaultIcon()) {
      this.icon.set(IconUtil.createIcon(this.iconPrefix(), 'xmark'));
      if (!this.iconClass()) {
        this.iconClass.set('color-error');
      }
    }
  }

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

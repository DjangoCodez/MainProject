import { CommonModule } from '@angular/common';
import {
  Component,
  ComponentRef,
  computed,
  effect,
  input,
  OnInit,
  output,
  signal,
} from '@angular/core';
import {
  IconName,
  IconPrefix,
  IconProp,
} from '@fortawesome/fontawesome-svg-core';
import { TranslateModule } from '@ngx-translate/core';
import { IconUtil } from '@shared/util/icon-util';
import { IconModule } from '@ui/icon/icon.module';

export type ToolbarButtonBehaviour = 'standard' | 'primary' | 'danger';
export type ToolbarButtonAction = { key: string; event: Event } | undefined;
export type ToolbarButtonDisabledSignal =
  | {
      key: string;
      comp: ComponentRef<any>;
      value: boolean;
    }
  | undefined;

@Component({
  selector: 'soe-toolbar-button',
  imports: [CommonModule, TranslateModule, IconModule],
  templateUrl: './toolbar-button.component.html',
  styleUrls: [
    '../../../styles/shared-styles/shared-button-styles.scss',
    './toolbar-button.component.scss',
  ],
})
export class ToolbarButtonComponent implements OnInit {
  key = input('');
  behaviour = input<ToolbarButtonBehaviour>('standard');
  caption = input('');
  tooltip = input('');
  iconPrefix = input<IconPrefix>('fal');
  iconName = input<IconName | undefined>();
  iconClass = input('');
  disabled = input(false);
  hidden = input(false);

  onAction = output<ToolbarButtonAction>();

  typeClass = computed(() => {
    return this.getTypeClass();
  });

  icon = signal<IconProp | undefined>(undefined);

  constructor() {
    effect(() => {
      const iconPrefixSignal = this.iconPrefix();
      if (iconPrefixSignal) this.createIcon();
    });
    effect(() => {
      const iconNameSignal = this.iconName();
      if (iconNameSignal) this.createIcon();
    });
  }

  ngOnInit() {
    if (this.iconName()) this.createIcon();
  }

  onClick(event: Event): void {
    this.onAction.emit({ key: this.key(), event: event });
  }

  private createIcon() {
    if (this.iconName())
      this.icon.set(IconUtil.createIcon(this.iconPrefix(), this.iconName()));
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

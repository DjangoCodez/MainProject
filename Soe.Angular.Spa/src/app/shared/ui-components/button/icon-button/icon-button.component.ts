import { CommonModule } from '@angular/common';
import { Component, ViewEncapsulation, input, output } from '@angular/core';
import { AnimationProp } from '@fortawesome/angular-fontawesome';
import { IconPrefix, IconName } from '@fortawesome/fontawesome-svg-core';
import { TranslatePipe } from '@ngx-translate/core';
import { IconModule } from '@ui/icon/icon.module';

@Component({
  selector: 'soe-icon-button',
  imports: [CommonModule, IconModule, TranslatePipe],
  templateUrl: './icon-button.component.html',
  styleUrls: ['./icon-button.component.scss'],
  encapsulation: ViewEncapsulation.None,
})
export class IconButtonComponent {
  iconPrefix = input<IconPrefix>('fal');
  iconName = input.required<IconName>();
  iconClass = input('');
  tooltip = input('');
  noBorder = input(false);
  noMargins = input(false);
  transparent = input(false);
  disabled = input(false);
  outsideGroup = input(false);
  insideGroup = input(false);
  firstInGroup = input(false);
  lastInGroup = input(false);
  narrow = input(false);
  attachedToTextarea = input(false);
  attachedToTextbox = input(false);
  animation = input<AnimationProp | undefined>(undefined);

  action = output<Event>();

  performAction(action: Event): void {
    this.action.emit(action);
  }
}

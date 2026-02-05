import { Component, effect, input, model, output } from '@angular/core';
import { IconName, IconPrefix } from '@fortawesome/angular-fontawesome';
import { MenuButtonBehaviour, MenuButtonComponent, MenuButtonItem, MenuButtonVariant } from '@ui/button/menu-button/menu-button.component';

export type ToolbarMenuButtonAction = { key: string; value: MenuButtonItem };
@Component({
  selector: 'soe-toolbar-menu-button',
  imports: [MenuButtonComponent],
  templateUrl: './toolbar-menu-button.component.html',
  styleUrl: './toolbar-menu-button.component.scss',
})
export class ToolbarMenuButtonComponent {
  key = input('');
  caption = input('');
  tooltip = input('');
  iconName = input<IconName>();
  iconPrefix = input<IconPrefix>('fal');
  iconClass = input('');
  variant = input<MenuButtonVariant>('menu');
  behaviour = input<MenuButtonBehaviour>('secondary');
  insideGroup = input(false);
  dropUp = input(false);
  dropLeft = input(false);
  hideDropdownArrow = input(false);
  disabled = input(false);
  hidden = input(false);
  list = input<MenuButtonItem[]>([]);
  selectedItem = model<MenuButtonItem | undefined>();
  showSelectedItemIcon = input(false);
  initialSelectedItemId = input<number | undefined>();
  unselectItemAfterSelect = input(false);

  onItemSelected = output<ToolbarMenuButtonAction>();

  itemSelected = (value: MenuButtonItem) => {
    this.onItemSelected.emit({ key: this.key(), value: value });
  };

  constructor() {
    effect(() => {
      const initialSelectedItemSignal = this.initialSelectedItemId();
      if (initialSelectedItemSignal !== undefined) {
        this.selectedItem.set(
          this.list().find(item => item.id === initialSelectedItemSignal)
        );
      }
    });
  }
}

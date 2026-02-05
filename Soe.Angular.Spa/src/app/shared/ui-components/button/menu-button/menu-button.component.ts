import { CommonModule } from '@angular/common';
import {
  Component,
  OnInit,
  Signal,
  computed,
  input,
  model,
  output,
  signal,
} from '@angular/core';
import { IconName, IconPrefix } from '@fortawesome/angular-fontawesome';
import { IconProp } from '@fortawesome/fontawesome-svg-core';
import { TranslatePipe } from '@ngx-translate/core';
import { ClickOutsideDirective } from '@shared/directives/click-outside/click-outside.directive';
import { IconModule } from '@ui/icon/icon.module';

export class MenuButtonItem {
  id?: number;
  icon?: IconProp;
  iconClass?: string;
  label?: string;
  disabled?: Signal<boolean>;
  hidden?: Signal<boolean>;
  type?: MenuButtonItemType;

  constructor() {
    this.type = 'text';
  }
}
export type MenuButtonItemType = 'text' | 'header' | 'divider';
export type MenuButtonVariant = 'split' | 'menu';
export type MenuButtonBehaviour = 'primary' | 'secondary';

@Component({
  selector: 'soe-menu-button',
  imports: [CommonModule, ClickOutsideDirective, IconModule, TranslatePipe],
  templateUrl: './menu-button.component.html',
  styleUrls: [
    '../../../styles/shared-styles/shared-button-styles.scss',
    './menu-button.component.scss',
  ],
})
export class MenuButtonComponent implements OnInit {
  caption = input('');
  tooltip = input('');
  iconPrefix = input<IconPrefix>('fal');
  iconName = input<IconName>();
  iconClass = input('');
  variant = input<MenuButtonVariant>('menu');
  behaviour = input<MenuButtonBehaviour>('primary');
  insideGroup = input(false);
  dropUp = input(false);
  dropLeft = input(false);
  hideDropdownArrow = input(false);
  noBorderRadius = input(false);
  disabled = input(false);
  hidden = input(false);
  invalid = input(false);
  list = input<MenuButtonItem[]>([]);
  selectedItem = model<MenuButtonItem | undefined>();
  showSelectedItemIcon = input(false);
  showSelectedItemLabel = input(false);
  unselectItemAfterSelect = input(false);
  width = input(0);
  fitContainer = input(false);

  typeClass = computed(() => {
    return this.getTypeClass();
  });

  isSplit = computed(() => {
    return this.variant() === 'split';
  });

  isMenu = computed(() => {
    return this.variant() === 'menu';
  });

  itemSelected = output<MenuButtonItem>();
  validationErrorsAction = output<Event>();

  isOpen = signal(false);

  ngOnInit(): void {
    // If split button, select first item as default
    if (this.isSplit() && !this.selectedItem() && this.list().length > 0) {
      this.selectedItem.set(this.list()[0]);
    }
  }

  selectItem(item: MenuButtonItem): void {
    if (
      !item ||
      (item?.disabled && item.disabled()) ||
      item.type === 'header'
    ) {
      return;
    }

    this.selectedItem.set(item);
    this.itemSelected.emit(item);
    this.isOpen.set(false);

    if (this.unselectItemAfterSelect()) {
      this.selectedItem.set(undefined);
    }
  }

  updateOption(item: MenuButtonItem): void {
    if (
      !item ||
      (item?.disabled && item.disabled()) ||
      item.type === 'header'
    ) {
      return;
    }

    this.selectItem(item);
  }

  onButtonAction(): void {
    this.isSplit()
      ? this.selectItem(this.selectedItem() as MenuButtonItem)
      : this.toggleList();
  }

  closeList(): void {
    this.isOpen.set(false);
  }

  onValidationErrorsAction(action: Event): void {
    this.validationErrorsAction.emit(action);
  }

  toggleList(): void {
    this.isOpen.set(!this.isOpen());
  }

  private getTypeClass(): string {
    let strClass = '';
    switch (this.behaviour()) {
      case 'primary':
        strClass = 'btn btn-primary';
        break;
      case 'secondary':
        strClass = 'btn btn-secondary';
        break;
    }
    return strClass;
  }
}

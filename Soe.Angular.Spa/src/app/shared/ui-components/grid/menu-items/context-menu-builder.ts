import { SoeGridContext } from '@ui/grid/util/options-util';
import {
  DefaultMenuItem,
  GetContextMenuItemsParams,
  IMenuActionParams,
  MenuItemDef,
} from 'ag-grid-community';
import { IconMenuItem } from './icon-menu-item/icon-menu-item.component';
import { inject, Injectable } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { IconProp } from '@fortawesome/fontawesome-svg-core';

type MenuItem<TData = any> =
  | DefaultMenuItem
  | MenuItemDef<TData, SoeGridContext>;

type MenuItemReturn<TData> = MenuItem<TData>[] | Promise<MenuItem<TData>[]>;

export declare type GetContextMenuCallbackExtended<T> = (
  data: T | undefined,
  params: GetContextMenuItemsParams<T, SoeGridContext>,
  builder: ContextMenuBuilder<T>
) => MenuItemReturn<T>;

export declare type GetContextMenuCallback<T> = (
  params: GetContextMenuItemsParams<T, SoeGridContext>
) => MenuItemReturn<T>;

export interface ContextMenuOptions<TData, SoeGridContext> {
  caption: string;
  action: (params: IMenuActionParams<TData, SoeGridContext>) => void;
  icon?: IconProp;
  tooltip?: string;
  disabled?:
    | boolean
    | ((params: IMenuActionParams<TData, SoeGridContext>) => boolean);
  rowClasses?: string[];
  iconClasses?: string[];
}

export class ContextMenuConfig {
  hideCopy = false;
  hideCopyWithHeaders = false;
}

@Injectable({
  providedIn: 'root',
})
export class ContextMenuService {
  translate = inject(TranslateService);
  public builder<TData>() {
    const menu = new ContextMenuBuilder<TData>(this.translate);
    return menu;
  }

  public deleteButton<TData>(options: {
    action: (params: IMenuActionParams<TData, SoeGridContext>) => void;
    disabled?:
      | boolean
      | ((params: IMenuActionParams<TData, SoeGridContext>) => boolean);
    tooltip?: string;
    caption?: string;
    iconClasses?: string[];
    rowClasses?: string[];
    separatorAfter?: boolean;
  }): ContextMenuOptions<TData, SoeGridContext> {
    return {
      type: 'icon',
      caption: options.caption ?? 'core.delete',
      icon: 'times',
      iconClasses: options.iconClasses ?? ['icon-delete'],
      tooltip: options.tooltip,
      action: options.action,
      disabled: options.disabled,
      rowClasses: options.rowClasses,
      separatorAfter: options.separatorAfter ?? false,
    } as ContextMenuOptions<TData, SoeGridContext>;
  }

  public editButton<TData>(options: {
    action: (params: IMenuActionParams<TData, SoeGridContext>) => void;
    disabled?:
      | boolean
      | ((params: IMenuActionParams<TData, SoeGridContext>) => boolean);
    tooltip?: string;
    caption?: string;
    iconClasses?: string[];
    rowClasses?: string[];
    separatorAfter?: boolean;
  }): ContextMenuOptions<TData, SoeGridContext> {
    return {
      type: 'icon',
      caption: options.caption ?? 'core.edit',
      icon: 'pen',
      iconClasses: options.iconClasses,
      tooltip: options.tooltip,
      action: options.action,
      disabled: options.disabled,
      rowClasses: options.rowClasses,
      separatorAfter: options.separatorAfter ?? false,
    } as ContextMenuOptions<TData, SoeGridContext>;
  }
}

export class ContextMenuBuilder<TData> {
  translateService: TranslateService;
  menuItems: MenuItem[];

  constructor(translateService: TranslateService) {
    this.menuItems = [];
    this.translateService = translateService;
  }

  public buildDefaultContextMenu(
    params: IMenuActionParams<TData, SoeGridContext>,
    config: Partial<ContextMenuConfig>,
    extendedContextMenuItems?: (
      | ContextMenuOptions<TData, SoeGridContext>
      | DefaultMenuItem
    )[]
  ) {
    if (extendedContextMenuItems?.length) {
      this.addExtendedItems(params, extendedContextMenuItems);
    }

    if (
      (config && Object.keys(config).length > 0) ||
      (extendedContextMenuItems && extendedContextMenuItems.length > 0)
    )
      this.addSeparator();

    if (!config.hideCopy) this.addCopy();
    if (!config.hideCopyWithHeaders) this.addCopyWithHeaders();

    return this.build();
  }

  private addExtendedItems(
    params: IMenuActionParams<TData, SoeGridContext>,
    items: (ContextMenuOptions<TData, SoeGridContext> | DefaultMenuItem)[]
  ) {
    for (const item of items) {
      // Is DefaultMenuItem
      if (typeof item === 'string') {
        this.addItem(item as MenuItem<TData>);
        continue;
      }

      // Is ContextMenuOption
      const disabled =
        typeof item.disabled === 'function'
          ? item.disabled(params)
          : item.disabled;
      this.addIconButton({
        ...item,
        action: () => item.action(params),
        disabled,
      });
    }
  }

  public addIconButton(options: ContextMenuOptions<TData, SoeGridContext>) {
    const isDisabled =
      typeof options.disabled === 'function'
        ? options.disabled({} as IMenuActionParams<TData, SoeGridContext>)
        : options.disabled;

    this.addItem({
      name: options.caption,
      disabled: isDisabled,
      menuItem: IconMenuItem<TData>,
      menuItemParams: {
        caption: options.caption,
        iconProp: options.icon,
        tooltip: options.tooltip,
        disabled: isDisabled,
        action: options.action,
        rowClasses: options.rowClasses,
        iconClasses: options.iconClasses,
      },
    });
    return this;
  }

  public addSubMenuButton(options: {
    caption: string;
    tooltip?: string;
    disabled?: boolean;
    cssClasses?: string[];
    addSubMenu: (builder: ContextMenuBuilder<TData>) => MenuItem<TData>[];
  }) {
    this.addItem({
      ...options,
      name: this.translateService.instant(options.caption) || options.caption,
      subMenu: options.addSubMenu(
        new ContextMenuBuilder(this.translateService)
      ),
    });
    return this;
  }

  public addSeparator() {
    this.addItem('separator');
    return this;
  }

  public addCopy() {
    this.addItem('copy');
    return this;
  }

  public addDefaultItem(type: DefaultMenuItem) {
    this.addItem(type);
    return this;
  }

  public addCopyWithHeaders() {
    this.addItem('copyWithHeaders');
    return this;
  }

  private addItem(item: MenuItem<TData>) {
    this.menuItems.push(item);
  }

  public build() {
    return this.menuItems;
  }
}

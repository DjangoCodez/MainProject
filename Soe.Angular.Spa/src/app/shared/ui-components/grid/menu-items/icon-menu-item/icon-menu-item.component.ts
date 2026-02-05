import { Component } from '@angular/core';
import { IconProp } from '@fortawesome/fontawesome-svg-core';
import { SharedModule } from '@shared/shared.module';
import { IconModule } from '@ui/icon/icon.module'
import { SoeGridContext } from '@ui/grid/util/options-util';

import type { IMenuItemAngularComp } from 'ag-grid-angular';
import type {
  IMenuActionParams,
  IMenuConfigParams,
  IMenuItemParams,
} from 'ag-grid-community';

export interface CustomMenuItemParams extends IMenuItemParams {
  caption: string;
  iconProp?: IconProp;
  rowClasses?: string[];
  iconClasses?: string[];
}

@Component({
  imports: [IconModule, SharedModule],
  template: ` <div
    (click)="onClick()"
    [title]="tooltip | translate"
    [class.ag-menu-option-disabled]="disabled"
    [classList]="rowClasses">
    @if (icon) {
      <fa-icon
        [classList]="iconClasses"
        [icon]="icon"
        [fixedWidth]="true"></fa-icon>
    } @else {
      <span class="ag-menu-option-part ag-menu-option-icon"></span>
    }
    <span class="ag-menu-option-part ag-menu-option-text">{{
      caption | translate
    }}</span>
    <span class="ag-menu-option-part ag-menu-option-shortcut"></span>
    <span class="ag-menu-option-part ag-menu-option-popup-pointer"></span>
  </div>`,
})
export class IconMenuItem<TData = any> implements IMenuItemAngularComp {
  name!: string;
  showSubMenu!: boolean;
  caption!: string;
  icon?: IconProp;
  tooltip!: string;
  disabled!: boolean;
  rowClasses!: string;
  iconClasses!: string;
  action?: (params: IMenuActionParams<TData, SoeGridContext>) => void;
  params!: CustomMenuItemParams;

  agInit(params: CustomMenuItemParams): void {
    this.name = params.name;
    this.showSubMenu = !!params.subMenu;
    this.caption = params.caption;
    this.icon = params.iconProp;
    this.disabled = !!params.disabled;
    this.tooltip = params.tooltip ?? this.caption;

    const defaultRowClasses = ['ag-menu-option'];
    this.rowClasses = defaultRowClasses
      .concat(params.rowClasses || [])
      .join(' ');

    const defaultIconClasses = ['ag-menu-option-part', 'ag-menu-option-icon'];
    this.iconClasses = defaultIconClasses
      .concat(params.iconClasses || [])
      .join(' ');

    this.action = params.action;
    this.params = params.menuItemParams;
  }

  configureDefaults(): boolean | IMenuConfigParams {
    return true;
  }

  onClick(): void {
    !this.disabled && this.action && this.action(this.params.menuItemParams);
  }
}

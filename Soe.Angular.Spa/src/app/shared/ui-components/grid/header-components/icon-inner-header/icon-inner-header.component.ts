import { NgClass } from '@angular/common';
import { Component } from '@angular/core';
import { IconName, IconPrefix } from '@fortawesome/fontawesome-svg-core';
import { IconModule } from '@ui/icon/icon.module';
import { IHeaderAngularComp } from 'ag-grid-angular';
import { IHeaderParams } from 'ag-grid-community';

export interface IIconInnerHeaderParams {
  iconPrefix?: IconPrefix;
  iconName?: IconName;
  iconClass?: string;
}

@Component({
  selector: 'soe-icon-inner-header',
  imports: [NgClass, IconModule],
  templateUrl: './icon-inner-header.component.html',
  styleUrl: './icon-inner-header.component.scss',
})
export class IconInnerHeaderComponent implements IHeaderAngularComp {
  public params!: IHeaderParams & IIconInnerHeaderParams;

  agInit(params: IHeaderParams & IIconInnerHeaderParams): void {
    this.params = params;

    if (!this.params.iconPrefix) this.params.iconPrefix = 'far';
  }

  refresh(params: IHeaderParams): boolean {
    return true;
  }
}

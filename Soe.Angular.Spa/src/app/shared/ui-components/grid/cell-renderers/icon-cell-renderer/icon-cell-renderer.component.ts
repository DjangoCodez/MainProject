import { Component } from '@angular/core';
import {
  IconName,
  IconPrefix,
  IconProp,
} from '@fortawesome/fontawesome-svg-core';
import { IconModule } from '@ui/icon/icon.module';
import { IconCellRendererParams } from '@ui/grid/interfaces';
import { ICellRendererAngularComp } from 'ag-grid-angular';

@Component({
  selector: 'soe-icon-cell-renderer',
  imports: [IconModule],
  templateUrl: './icon-cell-renderer.component.html',
  styleUrls: ['./icon-cell-renderer.component.scss'],
})
export class IconCellRenderer implements ICellRendererAngularComp {
  params!: IconCellRendererParams;

  noLink = false;
  showIcon = false;
  filterText = '';
  filterClass = '';

  agInit(params: IconCellRendererParams): void {
    this.params = params;

    if (this.params.isFilter && params.value && params.value.includes('|')) {
      const [prefix, icon, text, cssClass] = params.value
        .split('|')
        .map((p: string) => p.trim());

      this.params.icon = [prefix, icon];
      this.filterText = text || '';
      this.filterClass = cssClass || '';
    } else {
      if (
        this.params.useIconFromField &&
        this.params.data &&
        this.params.colDef?.field
      ) {
        const fieldData = this.params.data[this.params.colDef.field!];
        if (fieldData) {
          if (this.params.icon && this.hasPrefix(this.params.icon)) {
            this.params.icon = [this.params.icon[0], fieldData];
          } else {
            this.params.icon = fieldData;
          }
        } else {
          this.params.icon = undefined;
        }
      }
    }
    if (this.params.onClick?.length === 0) this.noLink = true;

    if (this.params.showIcon) {
      if (typeof this.params.showIcon === 'string') {
        // showIcon is a string which then should be a property on the data row.
        // Use it to check if value is true.
        this.showIcon = Boolean(this.params.data[this.params.showIcon]);
      } else if (typeof this.params.showIcon === 'function') {
        // showIcon is a function. Evaluate function to check if result is true.
        this.showIcon = this.params.showIcon(this.params.data);
      }
    } else {
      // showIcon is not specified, then always show it.
      this.showIcon = true;
    }
  }

  hasPrefix(prop: IconProp): prop is [IconPrefix, IconName] {
    return Array.isArray(prop);
  }

  refresh(params: IconCellRendererParams): boolean {
    return false;
  }

  isIconInFilter(): boolean {
    return this.params.value?.includes('|') && !!this.params.isFilter;
  }
}

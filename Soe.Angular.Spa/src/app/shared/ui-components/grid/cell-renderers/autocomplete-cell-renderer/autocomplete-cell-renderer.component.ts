import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { TranslatePipe } from '@ngx-translate/core';
import {
  StringKeyOfNumberProperty,
  StringKeyOfStringProperty,
} from '@shared/types';
import { AG_NODE_PROPS } from '@ui/grid/grid.component';
import { IconModule } from '@ui/icon/icon.module';
import { getAutocompleteCacheKey } from '@ui/grid/cell-editors/autocomplete-cell-editor/autocomplete-cell-editor.component';
import { AutocompleteCellRendererParams } from '@ui/grid/interfaces';
import { ICellRendererAngularComp } from 'ag-grid-angular';

type Option = { id: number; name: string };

@Component({
  selector: 'soe-autocomplete-cell-renderer',
  imports: [CommonModule, IconModule, TranslatePipe],
  templateUrl: './autocomplete-cell-renderer.component.html',
  styleUrls: ['./autocomplete-cell-renderer.component.scss'],
})
export class AutocompleteCellRenderer<T extends AG_NODE_PROPS, U>
  implements ICellRendererAngularComp
{
  public params!: AutocompleteCellRendererParams<T, U>;
  public value!: number | null | undefined;
  public tooltip!: string;
  public iconClass!: string;
  public show!: boolean;
  displayValue: string = '';

  agInit(params: AutocompleteCellRendererParams<T, U>): void {
    this.params = params;
    this.tooltip = this.params.buttonConfiguration.tooltip
      ? this.params.buttonConfiguration.tooltip
      : '';
    this.iconClass = this.params.buttonConfiguration.iconClass
      ? this.params.buttonConfiguration.iconClass
      : '';
    this.value = this.params.value;

    if (this.params.buttonConfiguration.show) {
      this.setButtonConfigurationPredicate();
    } else {
      this.show = false;
    }
    this.displayValue = this.params.valueFormatted || '';
  }

  setButtonConfigurationPredicate() {
    if (typeof this.params.buttonConfiguration.show === 'number') {
      // show is a number which then should be a property on the data row.
      // Use it to check if value is true.
      this.show = Boolean(
        this.params.data == this.params.buttonConfiguration.show
      );
    } else if (typeof this.params.buttonConfiguration.show === 'function') {
      // show is a function. Evaluate function to check if result is true.
      this.show =
        this.params.data !== undefined
          ? this.params.buttonConfiguration.show(this.params.data)
          : false;
    }
  }

  refresh(params: AutocompleteCellRendererParams<T, U>): boolean {
    this.params = params;
    this.value = this.params.value;
    return true;
  }

  onChange(event: any) {
    this.value = event.target.value;
    if (
      this.params.buttonConfiguration.onClick &&
      this.params.data !== undefined
    ) {
      this.params.buttonConfiguration.onClick(this.params.data);
    }
  }
}

import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { IconProp } from '@fortawesome/fontawesome-svg-core';
import { IconModule } from '@ui/icon/icon.module';
import { ShapeCellRendererParams } from '@ui/grid/interfaces';
import { ICellRendererAngularComp } from 'ag-grid-angular';

export type ShapeType =
  | 'circle'
  | 'square'
  | 'rectangle'
  | 'triangle-up'
  | 'triangle-right'
  | 'triangle-down'
  | 'triangle-left';

@Component({
  selector: 'soe-shape-cell-renderer',
  imports: [CommonModule, IconModule],
  templateUrl: './shape-cell-renderer.component.html',
  styleUrls: ['./shape-cell-renderer.component.scss'],
})
export class ShapeCellRenderer implements ICellRendererAngularComp {
  params!: ShapeCellRendererParams;
  data: any;
  isVisible = true;

  shape: ShapeType = 'circle';
  width = 20;
  color = 'transparent';
  showShapeField = '';
  useGradient = false;
  gradientField = '';
  tooltip = '';
  shapeTooltip = '';
  isText = false;
  isSelect = false;
  isFilter = false;
  filterText = '';
  showIcon = false;
  icon: IconProp = ['fal', 'circle'];

  agInit(params: ShapeCellRendererParams): void {
    this.params = params;
    this.data = this.params.data;

    if (this.params.isFilter) this.isFilter = this.params.isFilter;
    if (this.params.shape) this.shape = this.params.shape as ShapeType;
    if (this.params.width) this.width = this.params.width;
    if (this.params.color) this.color = this.params.color;
    if (
      this.params.colorField &&
      this.data &&
      this.data.hasOwnProperty(this.params.colorField)
    )
      this.color = this.data[this.params.colorField];
    if (this.params.isFilter && params.value) {
      const parts = params.value.split('|');
      this.color = parts[0] == 'undefined' ? '#FFFFFF' : parts[0];
      this.filterText = parts[1];
    }
    if (this.params.showShapeField)
      this.showShapeField = this.params.showShapeField;
    if (this.params.useGradient) this.useGradient = this.params.useGradient;
    if (this.params.gradientField)
      this.gradientField = this.params.gradientField;

    if (!this.params.isSelect) {
      if (this.gradientField)
        this.isVisible = this.data
          ? !!(this.data[this.gradientField] as boolean)
          : false;
      else if (this.showShapeField)
        this.isVisible = this.data
          ? !!(this.data[this.showShapeField] as boolean)
          : false;
    }
    if (this.params.tooltip) this.tooltip = this.params.tooltip;
    if (this.params.shapeTooltip) this.shapeTooltip = this.params.shapeTooltip;
    if (this.params.isText) this.isText = this.params.isText;
    if (this.params.isSelect) this.isSelect = this.params.isSelect;

    // Icon
    if (this.params.showIcon) {
      this.showIcon = this.params.showIcon(this.params.data);
      this.isVisible = true;
    } else {
      this.showIcon = false;
    }
    if (params.icon) this.icon = params.icon;
  }

  refresh(params: ShapeCellRendererParams): boolean {
    return false;
  }

  get isCircle(): boolean {
    return this.shape === 'circle';
  }

  get isSquare(): boolean {
    return this.shape === 'square';
  }

  get isRectangle(): boolean {
    return this.shape === 'rectangle';
  }

  get isTriangleUp(): boolean {
    return this.shape === 'triangle-up';
  }

  get isTriangleRight(): boolean {
    return this.shape === 'triangle-right';
  }

  get isTriangleDown(): boolean {
    return this.shape === 'triangle-down';
  }

  get isTriangleLeft(): boolean {
    return this.shape === 'triangle-left';
  }

  getValue() {
    if (this.isText) return this.params.value;
    else if (this.isSelect) return this.params.valueFormatted;
  }

  isColorInFilter() {
    return this.isFilter && this.color && this.color.includes('#');
  }
}

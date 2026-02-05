import { Component, input, output } from '@angular/core';
import { MatSliderModule } from '@angular/material/slider';

@Component({
  selector: 'soe-slider',
  imports: [MatSliderModule],
  templateUrl: './slider.component.html',
  styleUrl: './slider.component.scss',
})
export class SliderComponent {
  min = input(0);
  max = input(0);
  step = input(0);
  value = input(0);
  formatValue = input<(value: number) => string>();
  showTicks = input(true);
  showActiveTrack = input(false);
  showThumbLabel = input(true);
  showMin = input(false);
  showMax = input(false);
  disabled = input(false);
  useRange = input(false);
  valueStart = input(0);
  valueEnd = input(0);

  valueChanged = output<number>();
  rangeValueChanged = output<{ start: number; end: number }>();

  formatSliderValue = (value: number) => {
    const customFormatter = this.formatValue();
    if (customFormatter && typeof customFormatter === 'function') {
      return customFormatter(value);
    }
    return value.toString();
  };

  onSliderChange(value: number) {
    this.valueChanged.emit(value);
  }

  onSliderStartChange(value: number) {
    this.rangeValueChanged.emit({
      start: value,
      end: this.valueEnd(),
    });
  }

  onSliderEndChange(value: number) {
    this.rangeValueChanged.emit({
      start: this.valueStart(),
      end: value,
    });
  }
}

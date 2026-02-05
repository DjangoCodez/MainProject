import { CommonModule } from '@angular/common';
import { Component, computed, input, output } from '@angular/core';
import { NG_VALUE_ACCESSOR, ReactiveFormsModule } from '@angular/forms';
import { TranslatePipe } from '@ngx-translate/core';
import { ValueAccessorDirective } from '@ui/forms/directives/value-accessor.directive';
import { IconName, IconPrefix } from '@fortawesome/fontawesome-svg-core';
import { IconModule } from '@ui/icon/icon.module';
import { IconUtil } from '@shared/util/icon-util';

@Component({
  selector: 'soe-radio',
  imports: [CommonModule, ReactiveFormsModule, TranslatePipe, IconModule],
  templateUrl: './radio.component.html',
  styleUrls: [
    '../../../styles/shared-styles/shared-button-styles.scss',
    './radio.component.scss',
  ],
  providers: [
    { provide: NG_VALUE_ACCESSOR, multi: true, useExisting: RadioComponent },
  ],
})
export class RadioComponent<T> extends ValueAccessorDirective<T> {
  inputId = input<string>(Math.random().toString(24));
  labelKey = input('');
  iconPrefix = input<IconPrefix>('fal');
  iconName = input<IconName | undefined>();
  iconClass = input('');
  inline = input(false);
  styleAsButton = input(false);
  noMargin = input(false);
  group = input.required<string>();

  valueChanged = output<T | undefined>();

  icon = computed(() =>
    this.iconName()
      ? IconUtil.createIcon(this.iconPrefix(), this.iconName())
      : undefined
  );

  onValueChange = (event: Event) => this.valueChanged.emit(this.value());
}

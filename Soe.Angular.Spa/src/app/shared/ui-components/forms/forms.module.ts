import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ValueAccessorDirective } from './directives/value-accessor.directive';

@NgModule({
  declarations: [ValueAccessorDirective],
  exports: [ValueAccessorDirective],
  imports: [CommonModule],
})
export class FormsModule {}

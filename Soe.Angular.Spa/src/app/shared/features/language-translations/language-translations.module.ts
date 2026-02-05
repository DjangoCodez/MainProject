import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LanguageTranslationsComponent } from './language-translations.component';
import { ButtonComponent } from '@ui/button/button/button.component'
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component';

@NgModule({
  declarations: [LanguageTranslationsComponent],
  exports: [LanguageTranslationsComponent],
  imports: [CommonModule, GridWrapperComponent, ButtonComponent],
})
export class LanguageTranslationsModule {}

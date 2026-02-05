import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  FontAwesomeModule,
  FaIconLibrary,
} from '@fortawesome/angular-fontawesome';
import { fal } from '@fortawesome/pro-light-svg-icons';
import { far } from '@fortawesome/pro-regular-svg-icons';
import { fas } from '@fortawesome/pro-solid-svg-icons';

@NgModule({
  imports: [CommonModule, FontAwesomeModule],
  exports: [FontAwesomeModule],
})
export class IconModule {
  constructor(faLibrary: FaIconLibrary) {
    faLibrary.addIconPacks(fal);
    faLibrary.addIconPacks(far);
    faLibrary.addIconPacks(fas);
  }
}

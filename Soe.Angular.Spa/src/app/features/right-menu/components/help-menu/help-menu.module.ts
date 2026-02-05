import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SharedModule } from '@shared/shared.module';
import { HelpMenuComponent } from './help-menu.component';
import { IconModule } from '@ui/icon/icon.module';

@NgModule({
  declarations: [HelpMenuComponent],
  exports: [HelpMenuComponent],
  imports: [CommonModule, SharedModule, IconModule],
})
export class HelpMenuModule {}

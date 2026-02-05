import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { InformationMenuComponent } from './information-menu.component';
import { SharedModule } from '@shared/shared.module';
import { IconModule } from '@ui/icon/icon.module';

@NgModule({
  declarations: [InformationMenuComponent],
  exports: [InformationMenuComponent],
  imports: [CommonModule, SharedModule, IconModule],
})
export class InformationMenuModule {}

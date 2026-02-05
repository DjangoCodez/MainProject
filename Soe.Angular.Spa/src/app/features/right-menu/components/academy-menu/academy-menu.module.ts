import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AcademyMenuComponent } from './academy-menu.component';
import { IconModule } from '@ui/icon/icon.module';
import { SharedModule } from '@shared/shared.module';

@NgModule({
  declarations: [AcademyMenuComponent],
  exports: [AcademyMenuComponent],
  imports: [CommonModule, IconModule, SharedModule],
})
export class AcademyMenuModule {}

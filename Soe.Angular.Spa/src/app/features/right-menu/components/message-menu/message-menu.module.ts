import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MessageMenuComponent } from './message-menu.component';
import { IconModule } from '@ui/icon/icon.module';
import { SharedModule } from '@shared/shared.module';

@NgModule({
  declarations: [MessageMenuComponent],
  exports: [MessageMenuComponent],
  imports: [CommonModule, SharedModule, IconModule],
})
export class MessageMenuModule {}

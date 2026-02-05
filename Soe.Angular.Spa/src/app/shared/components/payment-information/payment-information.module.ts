import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { TranslateModule } from '@ngx-translate/core';
import { SharedModule } from '@shared/shared.module';
import { ButtonComponent } from '@ui/button/button/button.component'
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component'
import { LabelComponent } from '@ui/label/label.component';
import { PaymentInformationComponent } from './payment-information/payment-information.component';

@NgModule({
  declarations: [PaymentInformationComponent],
  exports: [PaymentInformationComponent],
  imports: [
    CommonModule,
    SharedModule,
    TranslateModule,
    GridWrapperComponent,
    LabelComponent,
    ButtonComponent,
  ],
})
export class PaymentInformationModule {}

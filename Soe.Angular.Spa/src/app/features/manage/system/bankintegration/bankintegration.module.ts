import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { SharedModule } from '@shared/shared.module';
import { ButtonComponent } from '@ui/button/button/button.component';
import { CheckboxComponent } from '@ui/forms/checkbox/checkbox.component';
import { DatepickerComponent } from '@ui/forms/datepicker/datepicker.component';
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component';
import { MultiTabWrapperComponent } from '@ui/tab/multi-tab-wrapper/multi-tab-wrapper.component';
import { TextboxComponent } from '@ui/forms/textbox/textbox.component';
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';
import { BankintegrationRoutingModule } from './bankintegration-routing.module';
import { BankintegrationDownloadRequestGridFilterComponent } from './components/bankintegration-downloadrequest-grid-filter/bankintegration-downloadrequest-grid-filter.component';
import { BankintegrationDownloadRequestGridComponent } from './components/bankintegration-downloadrequest-grid/bankintegration-downloadrequest-grid.component';
import { BankintegrationOnboardingGridComponent } from './components/bankintegration-onboarding-grid/bankintegration-onboarding-grid.component';
import { BankintegrationComponent } from './components/bankintegration/bankintegration.component';

@NgModule({
  declarations: [
    BankintegrationComponent,
    BankintegrationDownloadRequestGridComponent,
    BankintegrationDownloadRequestGridFilterComponent,
  ],
  imports: [
    BankintegrationOnboardingGridComponent,
    BankintegrationRoutingModule,
    CheckboxComponent,
    DatepickerComponent,
    CommonModule,
    SharedModule,
    ButtonComponent,
    ReactiveFormsModule,
    GridWrapperComponent,
    MultiTabWrapperComponent,
    TextboxComponent,
    ToolbarComponent,
  ],
})
export class BankintegrationModule {}

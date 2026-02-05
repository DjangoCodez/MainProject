import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ButtonComponent } from '@ui/button/button/button.component'
import { DatepickerComponent } from '@ui/forms/datepicker/datepicker.component'
import { EditFooterComponent } from '@ui/footer/edit-footer/edit-footer.component'
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component'
import { MultiTabWrapperComponent } from '@ui/tab/multi-tab-wrapper/multi-tab-wrapper.component'
import { SelectComponent } from '@ui/forms/select/select.component'
import { TextboxComponent } from '@ui/forms/textbox/textbox.component'
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';
import { IntrastatExportRoutingModule } from './intrastat-export-routing.module';
import { IntrastatExportComponent } from './components/intrastat-export/intrastat-export.component';
import { IntrastatExportGridComponent } from './components/intrastat-export-grid/intrastat-export-grid.component';
import { ReactiveFormsModule } from '@angular/forms';
import { IntrastatExportGridHeaderComponent } from './components/intrastat-export-grid-header/intrastat-export-grid-header.component';
import { SharedModule } from '@shared/shared.module';
import { PurchaseModule } from '@features/billing/purchase/purchase.module';
import { PurchaseDeliveryModule } from '@features/billing/purchase-delivery/purchase-delivery.module';

@NgModule({
  declarations: [
    IntrastatExportComponent,
    IntrastatExportGridComponent,
    IntrastatExportGridHeaderComponent,
  ],
  imports: [
    CommonModule,
    SharedModule,
    ButtonComponent,
    EditFooterComponent,
    DatepickerComponent,
    GridWrapperComponent,
    SelectComponent,
    ReactiveFormsModule,
    MultiTabWrapperComponent,
    TextboxComponent,
    ToolbarComponent,
    IntrastatExportRoutingModule,
    PurchaseModule,
    PurchaseDeliveryModule,
  ],
})
export class IntrastatExportModule {}

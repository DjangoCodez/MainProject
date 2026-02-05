import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { SharedModule } from '@shared/shared.module';
import { ButtonComponent } from '@ui/button/button/button.component';
import { CheckboxComponent } from '@ui/forms/checkbox/checkbox.component';
import { EditFooterComponent } from '@ui/footer/edit-footer/edit-footer.component';
import { ExpansionPanelComponent } from '@ui/expansion-panel/expansion-panel.component';
import { MultiTabWrapperComponent } from '@ui/tab/multi-tab-wrapper/multi-tab-wrapper.component';
import { NumberboxComponent } from '@ui/forms/numberbox/numberbox.component';
import { SaveButtonComponent } from '@ui/button/save-button/save-button.component';
import { SelectComponent } from '@ui/forms/select/select.component';
import { FinnishTaxExportEditComponent } from './components/finnish-tax-export-edit/finnish-tax-export-edit.component';
import { FinnishTaxExportGridComponent } from './components/finnish-tax-export-grid/finnish-tax-export-grid.component';
import { FinnishTaxExportComponent } from './components/finnish-tax-export/finnish-tax-export.component';
import { FinnishTaxExportRoutingModule } from './finnish-tax-export-routing.module';

@NgModule({
  declarations: [
    FinnishTaxExportComponent,
    FinnishTaxExportEditComponent,
    FinnishTaxExportGridComponent,
  ],
  imports: [
    CommonModule,
    FinnishTaxExportRoutingModule,
    ExpansionPanelComponent,
    SharedModule,
    ButtonComponent,
    SaveButtonComponent,
    EditFooterComponent,
    ReactiveFormsModule,
    MultiTabWrapperComponent,
    CheckboxComponent,
    SelectComponent,
    NumberboxComponent,
  ],
})
export class FinnishTaxExportModule {}

import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { EmployeeCsrExportRoutingModule } from './employee-csr-export-routing.module';
import { EmployeeCsrExportComponent } from './components/employee-csr-export/employee-csr-export.component';
import { EmployeeCsrExportGridComponent } from './components/employee-csr-export-grid/employee-csr-export-grid.component';
import { SharedModule } from '@shared/shared.module';
import { ReactiveFormsModule } from '@angular/forms';
import { ButtonComponent } from '@ui/button/button/button.component'
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component'
import { MultiTabWrapperComponent } from '@ui/tab/multi-tab-wrapper/multi-tab-wrapper.component'
import { RadioComponent } from '@ui/forms/radio/radio.component'
import { SelectComponent } from '@ui/forms/select/select.component';

@NgModule({
  declarations: [EmployeeCsrExportComponent, EmployeeCsrExportGridComponent],
  imports: [
    CommonModule,
    SharedModule,
    ReactiveFormsModule,
    EmployeeCsrExportRoutingModule,
    MultiTabWrapperComponent,
    GridWrapperComponent,
    SelectComponent,
    RadioComponent,
    ButtonComponent,
  ],
})
export class EmployeeCsrExportModule {}

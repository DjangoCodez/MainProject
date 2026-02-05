import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { SharedModule } from '@shared/shared.module';
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component';
import { MultiTabWrapperComponent } from '@ui/tab/multi-tab-wrapper/multi-tab-wrapper.component';
import { SelectComponent } from '@ui/forms/select/select.component';
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';
import { LoggedWarningsRoutingModule } from './logged-warnings-routing.module';
import { LoggedWarningsComponent } from './components/logged-warnings/logged-warnings.component';
import { LoggedWarningsGridComponent } from './components/logged-warnings-grid/logged-warnings-grid.component';
import { LoggedWarningsGridFilterComponent } from './components/logged-warnings-grid/logged-warnings-grid-filter/logged-warnings-grid-filter.component';

@NgModule({
  declarations: [
    LoggedWarningsComponent,
    LoggedWarningsGridComponent,
    LoggedWarningsGridFilterComponent,
  ],
  imports: [
    CommonModule,
    SharedModule,
    FormsModule,
    ReactiveFormsModule,
    LoggedWarningsRoutingModule,
    GridWrapperComponent,
    MultiTabWrapperComponent,
    SelectComponent,
    ToolbarComponent,
  ],
})
export class LoggedWarningsModule {}

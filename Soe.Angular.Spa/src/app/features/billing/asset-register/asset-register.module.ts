import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component';
import { MenuButtonComponent } from '@ui/button/menu-button/menu-button.component';
import { MultiSelectComponent } from '@ui/forms/select/multi-select/multi-select.component';
import { MultiTabWrapperComponent } from '@ui/tab/multi-tab-wrapper/multi-tab-wrapper.component';
import { SelectComponent } from '@ui/forms/select/select.component';
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';
import { ReactiveFormsModule } from '@angular/forms';
import { AssetRegisterRoutingModule } from './asset-register-routing.module';
import { SharedModule } from '@shared/shared.module';
import { AssetRegisterGridComponent } from './components/asset-register-grid/asset-register-grid.component';
import { AssetRegisterComponent } from './components/asset-register/asset-register.component';

@NgModule({
  declarations: [AssetRegisterComponent, AssetRegisterGridComponent],
  imports: [
    CommonModule,
    AssetRegisterRoutingModule,
    GridWrapperComponent,
    MultiTabWrapperComponent,
    ToolbarComponent,
    ReactiveFormsModule,
    MultiSelectComponent,
    MenuButtonComponent,
    SelectComponent,
    SharedModule,
  ],
})
export class AssetRegisterModule {}

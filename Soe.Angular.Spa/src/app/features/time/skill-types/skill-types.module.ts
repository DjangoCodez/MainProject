import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { SharedModule } from '@shared/shared.module';
import { ButtonComponent } from '@ui/button/button/button.component'
import { EditFooterComponent } from '@ui/footer/edit-footer/edit-footer.component'
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component'
import { MultiTabWrapperComponent } from '@ui/tab/multi-tab-wrapper/multi-tab-wrapper.component'
import { TextboxComponent } from '@ui/forms/textbox/textbox.component'
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';
import { SkillTypesEditComponent } from './components/skill-types-edit/skill-types-edit.component';
import { SkillTypesGridComponent } from './components/skill-types-grid/skill-types-grid.component';
import { SkillTypesComponent } from './components/skill-types/skill-types.component';
import { SkillTypesRoutingModule } from './skill-types-routing.module';

@NgModule({
  declarations: [
    SkillTypesComponent,
    SkillTypesGridComponent,
    SkillTypesEditComponent,
  ],
  imports: [
    SharedModule,
    SkillTypesRoutingModule,
    CommonModule,
    ButtonComponent,
    EditFooterComponent,
    GridWrapperComponent,
    ReactiveFormsModule,
    MultiTabWrapperComponent,
    TextboxComponent,
    ToolbarComponent,
  ],
})
export class SkillTypesModule {}

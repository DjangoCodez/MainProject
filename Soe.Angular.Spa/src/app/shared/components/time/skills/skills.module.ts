import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { TranslateModule } from '@ngx-translate/core';
import { SharedModule } from '@shared/shared.module';
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component';
import { SkillsComponent } from './skills.component';

@NgModule({
  declarations: [SkillsComponent],
  exports: [SkillsComponent],
  imports: [CommonModule, SharedModule, TranslateModule, GridWrapperComponent],
})
export class SkillsModule {}

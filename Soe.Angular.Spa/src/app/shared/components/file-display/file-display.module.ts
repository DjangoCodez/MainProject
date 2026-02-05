import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FileDisplayComponent } from './file-display.component';
import { SharedModule } from '@shared/shared.module';
import { TranslateModule } from '@ngx-translate/core';
import { FileViewerComponent } from '@ui/file-viewer/file-viewer.component'
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component'
import { IconModule } from '@ui/icon/icon.module'
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';

@NgModule({
  declarations: [FileDisplayComponent],
  exports: [FileDisplayComponent],
  imports: [
    CommonModule,
    FileViewerComponent,
    GridWrapperComponent,
    IconModule,
    SharedModule,
    TranslateModule,
    ToolbarComponent,
  ],
})
export class FileDisplayModule {}

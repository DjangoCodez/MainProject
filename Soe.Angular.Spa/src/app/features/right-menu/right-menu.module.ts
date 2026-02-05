import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RightMenuRoutingModule } from './right-menu-routing.module';
import { ReleaseNotesComponent } from './pages/release-notes/release-notes.component';
import { FaqComponent } from './pages/faq/faq.component';
import { InformationMenuModule } from './components/information-menu/information-menu.module';
import { HelpMenuModule } from './components/help-menu/help-menu.module';
import { AcademyMenuModule } from './components/academy-menu/academy-menu.module';
import { MessageMenuModule } from './components/message-menu/message-menu.module';
import { ReportMenuModule } from './components/report-menu/report-menu.module';
import { DocumentMenuModule } from './components/document-menu/document-menu.module';

@NgModule({
  declarations: [ReleaseNotesComponent, FaqComponent],
  imports: [
    CommonModule,
    RightMenuRoutingModule,
    InformationMenuModule,
    HelpMenuModule,
    AcademyMenuModule,
    MessageMenuModule,
    ReportMenuModule,
    DocumentMenuModule,
  ],
  exports: [
    InformationMenuModule,
    HelpMenuModule,
    AcademyMenuModule,
    MessageMenuModule,
    ReportMenuModule,
    DocumentMenuModule,
  ],
})
export class RightMenuModule {}

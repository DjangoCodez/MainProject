import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { ExportComponent } from '@shared/features/export/components/export/export.component';

const routes: Routes = [{ path: 'default.aspx', component: ExportComponent }];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class ExportRoutingModule {}

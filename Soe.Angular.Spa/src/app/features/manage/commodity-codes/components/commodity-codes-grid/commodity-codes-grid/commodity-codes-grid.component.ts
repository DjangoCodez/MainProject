import { Component, inject, OnInit, signal } from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { ICommodityCodeDTO } from '@shared/models/generated-interfaces/CommodityCodeDTO';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { SoeConfigUtil } from '@shared/util/soeconfig-util';
import { take } from 'rxjs/operators';
import { CommodityCodesUploadForm } from '../../../models/commodity-codes-upload-form.model';
import { CommodityCodeUploadDTO } from '../../../models/commodity-codes.model';
import { ValidationHandler } from '@shared/handlers';
import { DialogService } from '@ui/dialog/services/dialog.service';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { CommodityCodesUploadComponent } from '../../commodity-codes-upload/commodity-codes-upload.component';
import { Observable } from 'rxjs';
import { CommodityCodesService } from '@features/manage/commodity-codes/services/commodity-codes.service';

@Component({
  selector: 'soe-commodity-codes-grid',
  templateUrl: './commodity-codes-grid.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class CommodityCodesGridComponent
  extends GridBaseDirective<ICommodityCodeDTO, CommodityCodesService>
  implements OnInit
{
  service = inject(CommodityCodesService);
  validationHandler = inject(ValidationHandler);
  dialogService = inject(DialogService);

  form: CommodityCodesUploadForm = new CommodityCodesUploadForm({
    validationHandler: this.validationHandler,
    element: new CommodityCodeUploadDTO(),
  });

  override ngOnInit(): void {
    super.ngOnInit();
    this.startFlow(
      Feature.Manage_System,
      'manage.system.commoditycode.commoditycodes'
    );
  }

  override createGridToolbar(): void {
    super.createGridToolbar();

    this.toolbarService.createItemGroup({
      items: [
        this.toolbarService.createToolbarButton(
          'manage.system.commoditycode.import',
          {
            iconName: signal('upload'),
            tooltip: signal('manage.system.commoditycode.import'),
            onAction: () => this.uploadFiles(),
          }
        ),
      ],
    });
  }

  uploadFiles() {
    const uploadDialog = this.dialogService.open(
      CommodityCodesUploadComponent,
      {
        title: 'core.fileupload.choosefiletoimport',
        size: 'sm',
      }
    );
    uploadDialog.afterClosed().subscribe((result: boolean) => {
      if (result) {
        this.refreshGrid();
      }
    });
  }

  onGridReadyToDefine(grid: GridComponent<ICommodityCodeDTO>) {
    super.onGridReadyToDefine(grid);

    this.translate
      .get([
        'manage.system.commoditycode.code',
        'manage.system.commoditycode.description',
        'manage.system.commoditycode.useotherqualifier',
        'manage.system.commoditycode.startdate',
        'manage.system.commoditycode.enddate',
        'manage.system.commoditycode.import',
        'manage.system.commoditycode.commoditycodes',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnText(
          'code',
          terms['manage.system.commoditycode.code'],
          {
            flex: 40,
          }
        );
        this.grid.addColumnText(
          'text',
          terms['manage.system.commoditycode.description'],
          {
            flex: 60,
          }
        );
        this.grid.addColumnBool(
          'useOtherQuantity',
          terms['manage.system.commoditycode.useotherqualifier'],
          {
            editable: false,
            suppressFilter: false,
          }
        );
        this.grid.addColumnDate(
          'startDate',
          terms['manage.system.commoditycode.startdate'],
          { flex: 20 }
        );
        this.grid.addColumnDate(
          'endDate',
          terms['manage.system.commoditycode.enddate'],
          { flex: 20 }
        );
        super.finalizeInitGrid();
      });
  }

  override loadData(
    id?: number | undefined,
    additionalProps?: { langId: number }
  ): Observable<ICommodityCodeDTO[]> {
    return super.loadData(id, { langId: SoeConfigUtil.languageId });
  }
}

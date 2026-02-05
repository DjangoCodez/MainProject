import { Component, inject, OnInit, signal } from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import {
  ISysPriceListHeadGridDTO,
  ISysPricelistProviderDTO,
} from '@shared/models/generated-interfaces/SOESysModelDTOs';
import { ImportPriceListService } from '../../services/import-price-list.service';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { Observable, take, tap } from 'rxjs';
import { DialogService } from '@ui/dialog/services/dialog.service';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import {
  ImportPriceListUploadComponent,
  SysPriceImportDialogData,
} from '../import-price-list-upload/import-price-list-upload.component';

@Component({
  selector: 'soe-import-price-list-grid',
  templateUrl: './import-price-list-grid.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class ImportPriceListGridComponent
  extends GridBaseDirective<ISysPriceListHeadGridDTO, ImportPriceListService>
  implements OnInit
{
  service = inject(ImportPriceListService);
  dialogService = inject(DialogService);

  pricelistProviders: ISysPricelistProviderDTO[] = [];

  ngOnInit() {
    super.ngOnInit();

    this.startFlow(
      Feature.Manage_System_Price_List,
      'Manage.System.ImportPriceList',
      {
        lookups: [this.loadPricelistProviders()],
      }
    );
  }

  //#region Overrides
  override createGridToolbar(): void {
    super.createGridToolbar();

    this.toolbarService.createItemGroup({
      items: [
        this.toolbarService.createToolbarButton(
          'manage.system.import.price.list.import',
          {
            iconName: signal('upload'),
            tooltip: signal('manage.system.import.price.list.import'),
            onAction: () => this.uploadFiles(),
          }
        ),
      ],
    });
  }

  override onGridReadyToDefine(grid: GridComponent<ISysPriceListHeadGridDTO>) {
    super.onGridReadyToDefine(grid);
    this.translate
      .get([
        'manage.system.importpricelist.wholeseller',
        'manage.system.importpricelist.provider',
        'manage.system.importpricelist.created',
        'manage.system.importpricelist.createdby',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnText(
          'sysWholesellerName',
          terms['manage.system.importpricelist.wholeseller']
        );
        this.grid.addColumnText(
          'providerName',
          terms['manage.system.importpricelist.provider'],
          { flex: 1, enableHiding: true }
        );
        this.grid.addColumnDate(
          'created',
          terms['manage.system.importpricelist.created'],
          { flex: 1, enableHiding: true }
        );
        this.grid.addColumnText(
          'createdBy',
          terms['manage.system.importpricelist.createdby'],
          { flex: 1, enableHiding: true }
        );
        super.finalizeInitGrid();
      });
  }

  override loadData(
    id?: number | undefined
  ): Observable<ISysPriceListHeadGridDTO[]> {
    return super.loadData(id);
  }

  loadPricelistProviders() {
    return this.performLoadData.load$(
      this.service.getSysPricelistProvider().pipe(
        tap(res => {
          this.pricelistProviders = res;
        })
      )
    );
  }
  //#endregion

  uploadFiles() {
    const dialogData: SysPriceImportDialogData = {
      title: 'core.fileupload.choosefiletoimport',
      size: 'sm',
      providers: this.pricelistProviders,
    };

    const uploadDialog = this.dialogService.open(
      ImportPriceListUploadComponent,
      dialogData
    );
    uploadDialog.afterClosed().subscribe((result: boolean) => {
      if (result) {
        this.refreshGrid();
      }
    });
  }
}

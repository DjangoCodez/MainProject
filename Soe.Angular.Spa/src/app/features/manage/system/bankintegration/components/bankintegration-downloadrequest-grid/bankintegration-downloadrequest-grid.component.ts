import { Component, OnInit, inject } from '@angular/core';
import { ColumnUtil } from '@ui/grid/util/column-util';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { take, tap } from 'rxjs/operators';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ProgressService } from '@shared/services/progress/progress.service';
import { ISoeBankerDownloadFileDTO } from '@shared/models/generated-interfaces/BankIntegrationDTOs';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { BankintegrationDownloadRequestService } from '../../services/bankintegration-downloadrequest.service';
import { Perform } from '@shared/util/perform.class';
import { SoeBankerRequestFilterDTO } from '@src/app/features/manage/models/bankintegration.model';
import { Observable } from 'rxjs';
import { ISoeBankerDownloadRequestGridDTO } from '../../models/bankintegration-downloadrequest-grid.models';
import { SysCompanyService } from '@features/manage/system/sys-company/services/sys-company.service';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';

@Component({
  selector: 'soe-bankintegration-downloadrequest-grid',
  templateUrl: './bankintegration-downloadrequest-grid.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class BankintegrationDownloadRequestGridComponent
  extends GridBaseDirective<
    ISoeBankerDownloadRequestGridDTO,
    BankintegrationDownloadRequestService
  >
  implements OnInit
{
  readonly service = inject(BankintegrationDownloadRequestService);
  readonly sysCompnayService = inject(SysCompanyService);

  progressService = inject(ProgressService);
  performGridLoad = new Perform<ISoeBankerDownloadRequestGridDTO[]>(
    this.progressService
  );
  performDetailGridLoad = new Perform<ISoeBankerDownloadFileDTO[]>(
    this.progressService
  );
  gridDataFilter = new SoeBankerRequestFilterDTO();
  sysCompanies: ISmallGenericType[] = [];

  ngOnInit(): void {
    this.startFlow(
      Feature.Manage_System_BankIntegration,
      'Manage.System.BankIntegration.DownloadRequest',
      {
        lookups: [this.loadSysCompanies()],
      }
    );
  }

  onGridReadyToDefine(grid: GridComponent<ISoeBankerDownloadRequestGridDTO>) {
    super.onGridReadyToDefine(grid);

    this.translate
      .get([
        'common.status',
        'common.message',
        'common.created',
        'common.modified',
        'common.type',
        'manage.system.bankintegration.downloadrequest.bank',
        'common.company',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.enableMasterDetail(
          {
            detailRowHeight: 120,
            floatingFiltersHeight: 0,

            columnDefs: [
              ColumnUtil.createColumnSelect(
                'actorCompanyId',
                terms['common.company'],
                this.sysCompanies,
                undefined,
                {
                  flex: 2,
                  editable: false,
                }
              ),
              ColumnUtil.createColumnText('status', terms['common.status'], {
                flex: 1,
              }),
              ColumnUtil.createColumnText('message', terms['common.message'], {
                flex: 7,
              }),
            ],
          },
          {
            addDefaultExpanderCol: false,
            getDetailRowData: this.loadDetailRows.bind(this),
          }
        );

        this.grid.addColumnText('material', terms['common.type'], { flex: 20 });
        this.grid.addColumnText(
          'bankName',
          terms['manage.system.bankintegration.downloadrequest.bank'],
          {
            flex: 10,
          }
        );
        this.grid.addColumnSelect(
          'statusCode',
          terms['common.status'],
          this.service.statuses,
          undefined,
          {
            editable: false,
            flex: 30,
          }
        );
        this.grid.addColumnText('message', terms['common.message'], {
          flex: 30,
        });
        this.grid.addColumnDateTime('created', terms['common.created'], {
          minWidth: 10,
        });
        this.grid.addColumnDateTime('modified', terms['common.modified'], {
          minWidth: 10,
        });

        this.grid.addColumnIcon('status', '', {
          width: 60,
          tooltipField: 'statusMessage',
          columnSeparator: true,
          iconName: 'triangle-exclamation',
          iconClass: 'color-warning',
          showIcon: row => this.service.showStatusMessage(row.statusCode),
        });
        super.finalizeInitGrid();
      });
  }

  private loadSysCompanies(): Observable<ISmallGenericType[]> {
    return this.sysCompnayService
      .getSysCompanyDict()
      .pipe(tap(companies => (this.sysCompanies = companies)));
  }

  override loadData(
    id?: number | undefined,
    additionalProps?: any
  ): Observable<ISoeBankerDownloadRequestGridDTO[]> {
    return this.performGridLoad.load$(this.service.search(this.gridDataFilter));
  }

  doSearch(filter: SoeBankerRequestFilterDTO) {
    this.gridDataFilter = filter;
    return this.performGridLoad.load(
      this.service
        .search(this.gridDataFilter)
        .pipe(tap(value => this.grid.setData(value)))
    );
  }

  loadDetailRows(params: any) {
    if (!params.data.filesLoaded) {
      this.performDetailGridLoad.load(
        this.service.getFiles(params.data.avaloDownloadRequestId).pipe(
          tap(value => {
            params.data.filesLoaded = true;
            params.data.downloadFiles = value;
            params.successCallback(value);
          })
        )
      );
    } else {
      params.successCallback(params.data.downloadFiles);
    }
  }
}

import {
  Component,
  HostListener,
  OnInit,
  computed,
  inject,
  signal,
} from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { ValidationHandler } from '@shared/handlers';
import {
  Feature,
  SoeLogType,
} from '@shared/models/generated-interfaces/Enumerations';
import { ISysLogGridDTO } from '@shared/models/generated-interfaces/SOESysModelDTOs';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ProgressService } from '@shared/services/progress/progress.service';
import { BrowserUtil } from '@shared/util/browser-util';
import { IconUtil } from '@shared/util/icon-util';
import { Perform } from '@shared/util/perform.class';
import { SoeConfigUtil } from '@shared/util/soeconfig-util';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { Observable, of } from 'rxjs';
import { take, tap } from 'rxjs/operators';
import { LevelOption, SearchSysLogsDTO } from '../../models/support-logs.model';
import { SupportLogsService } from '../../services/support-logs.service';

@Component({
  selector: 'soe-support-logs-grid',
  templateUrl: './support-logs-grid.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class SupportLogsGridComponent
  extends GridBaseDirective<ISysLogGridDTO, SupportLogsService>
  implements OnInit
{
  sysLogType = SoeConfigUtil.supportLogType;

  translate = inject(TranslateService);
  service = inject(SupportLogsService);
  validationHandler = inject(ValidationHandler);
  progressService = inject(ProgressService);
  performGridLoad = new Perform<ISysLogGridDTO[]>(this.progressService);
  isSearch = signal<boolean>(false);
  searchDto: SearchSysLogsDTO = new SearchSysLogsDTO();
  logRecords: ISysLogGridDTO[] = [];
  levelFilterOptions: LevelOption[] = [];

  private toolbarHeight = 230;
  private availableScreenHeight = signal(0);
  gridHeight = computed(() => {
    return this.isSearch()
      ? this.availableScreenHeight() - 292 //search fields section height
      : this.availableScreenHeight() - 42; //search TextBox height
  });

  ngOnInit(): void {
    super.ngOnInit();

    this.isSearch.set(+this.sysLogType == SoeLogType.System_Search);
    this.availableScreenHeight.set(window.innerHeight - this.toolbarHeight);
    this.startFlow(Feature.Manage_Support_Logs, 'Manage.Support.Logs', {
      useLegacyToolbar: true,
    });
  }

  override createLegacyGridToolbar(): void {
    super.createLegacyGridToolbar({
      reloadOption: {
        onClick: () => this.refreshGrid(),
      },
    });

    this.toolbarUtils.createLegacyGroup({
      buttons: [
        this.toolbarUtils.createLegacyButton({
          disabled: signal(false),
          hidden: signal(false),
          icon: IconUtil.createIcon('fal', 'sync'),
          onClick: () => this.loadUnique(),
          title: this.isSearch()
            ? 'manage.support.logs.searchunique'
            : 'manage.support.logs.showunique',
          label: this.isSearch()
            ? 'manage.support.logs.searchunique'
            : 'manage.support.logs.showunique',
        }),
      ],
    });

    if (this.isSearch()) {
      this.toolbarUtils.createLegacyGroup({
        buttons: [
          this.toolbarUtils.createLegacyButton({
            disabled: signal(false),
            hidden: signal(false),
            icon: IconUtil.createIcon('fal', 'search'),
            onClick: () => this.searchSysLogs(),
            title: 'core.search',
            label: 'core.search',
          }),
        ],
      });
    }
  }

  override onGridReadyToDefine(grid: GridComponent<ISysLogGridDTO>) {
    super.onGridReadyToDefine(grid);

    this.translate
      .get([
        'manage.support.logs.level',
        'common.date',
        'common.company',
        'common.message',
        'common.quantity',
        'common.download',
        'common.stacktrace',
        'core.edit',
        'core.search',
        'manage.support.logs.searchunique',
        'manage.support.logs.showunique',
        'core.aggrid.totals.filtered',
        'core.aggrid.totals.total',
        'core.aggrid.totals.selected',

        'manage.support.logs',
        'manage.support.logs.error',
        'manage.support.logs.warning',
        'manage.support.logs.information',
        'manage.support.logs.search',
        'manage.support.logs.all',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnText('level', terms['manage.support.logs.level'], {
          width: 60,
          enableHiding: false,
        });
        this.grid.addColumnDateTime('date', terms['common.date'], {
          width: 175,
          enableHiding: false,
        });
        this.grid.addColumnText('companyName', terms['common.company'], {
          width: 175,
          enableHiding: false,
        });
        this.grid.addColumnText('message', terms['common.message'], {
          flex: 1,
          enableHiding: true,
        });
        this.grid.addColumnText('stackTrace', terms['common.stacktrace'], {
          flex: 1,
          enableHiding: true,
        });
        this.grid.addColumnNumber('uniqueCounter', terms['common.quantity'], {
          width: 50,
          enableHiding: false,
        });
        this.grid.addColumnIcon(null, '', {
          width: 22,
          iconName: 'download',
          enableHiding: false,
          tooltip: terms['common.download'],
          onClick: row => {
            this.downloadFile(row);
          },
        });
        this.grid.addColumnIconEdit({
          tooltip: terms['core.edit'],
          onClick: row => {
            this.edit(row);
          },
        });

        this.exportFilenameKey.set(
          `${terms['manage.support.logs']}${
            this.loadLogs()
              ? '_' +
                terms[this.service.getLabelTerm(+this.sysLogType)] +
                '_' +
                new Date().toFormattedDate('yyyy-MM-dd')
              : ''
          }`
        );
        super.finalizeInitGrid();
      });
  }

  override loadData(
    id?: number | undefined,
    additionalProps?: any
  ): Observable<ISysLogGridDTO[]> {
    if (!this.isSearch() && this.loadLogs()) {
      return this.performGridLoad.load$(
        this.service
          .getGrid(undefined, {
            logType: this.sysLogType,
            showUnique: false,
          })
          .pipe(
            tap(data => {
              this.logRecords = data;
              return data;
            })
          )
      );
    } else {
      return of([]);
    }
  }

  downloadFile(row: ISysLogGridDTO) {
    BrowserUtil.openInSameTab(
      window,
      '/soe/manage/support/logs/edit/download/?sysLogId=' + row.sysLogId
    );
  }

  protected loadLogs(): boolean {
    return (
      +this.sysLogType == SoeLogType.System_All_Today ||
      +this.sysLogType == SoeLogType.System_Error_Today ||
      +this.sysLogType == SoeLogType.System_Warning_Today ||
      +this.sysLogType == SoeLogType.System_Information_Today
    );
  }

  loadSysLogs(showUnique = false) {
    if (!this.isSearch()) {
      this.performGridLoad.load(
        this.service
          .getGrid(undefined, {
            logType: this.sysLogType,
            showUnique: showUnique,
          })
          .pipe(
            tap(x => {
              this.logRecords = x;
              this.setGridData(this.logRecords);
            })
          )
      );
    }
  }

  searchSysLogs(showUnique = false) {
    if (this.isSearch()) {
      this.searchDto.showUnique = showUnique;
      this.performGridLoad.load(
        this.service.searchLogs(this.searchDto).pipe(
          tap(x => {
            this.logRecords = x;
            this.setGridData(this.logRecords);
          })
        )
      );
    }
  }

  setGridData(records: ISysLogGridDTO[]) {
    records = records.map(r => {
      r.date = new Date(r.date);
      return r;
    });
    this.grid.setData(records);
  }

  loadUnique(): void {
    if (this.isSearch()) {
      this.searchSysLogs(true);
    } else {
      this.loadSysLogs(true);
    }
  }

  filterChange(value?: string | null): void {
    let tempRecords = this.logRecords;

    if (!(!this.logRecords || !value)) {
      if (value && value.length > 0) {
        const excludes: string[] = value.split(';');
        excludes.forEach((exclude: string) => {
          if (exclude && exclude.length > 0)
            tempRecords = tempRecords.filter(
              sysLog => !sysLog.message.includes(exclude)
            );
        });
      }
    }

    this.setGridData(tempRecords);
  }

  @HostListener('window:resize', ['$event'])
  handleGridHeight(event: Event): void {
    if (window.innerHeight)
      this.availableScreenHeight.set(window.innerHeight - this.toolbarHeight);
  }
}

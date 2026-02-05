import {
  Component,
  EventEmitter,
  Input,
  OnDestroy,
  OnInit,
  Output,
  inject,
  signal,
} from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import {
  Feature,
  SettingMainType,
  UserSettingType,
} from '@shared/models/generated-interfaces/Enumerations';
import { IProjectSearchResultDTO } from '@shared/models/generated-interfaces/ProjectDTOs';
import { CoreService } from '@shared/services/core.service';
import { ProgressService } from '@shared/services/progress/progress.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { Perform } from '@shared/util/perform.class';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { Observable, of, take, tap } from 'rxjs';
import { SelectProjectDialogForm } from '../../models/select-project-dialog-form.model';
import {
  ProjectSearchModel,
  SaveUserCompanySettingModel,
} from '../../models/select-project-dialog.model';
import { SelectProjectService } from '../../services/select-project.service';
import { debounce } from 'lodash';

@Component({
  selector: 'soe-select-project',
  templateUrl: './select-project.component.html',
  styleUrls: ['./select-project.component.scss'],
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class SelectProjectComponent
  extends GridBaseDirective<IProjectSearchResultDTO>
  implements OnInit, OnDestroy
{
  @Input() form: SelectProjectDialogForm | undefined;

  @Input() customerId: number | undefined;
  @Input() useDelete?: boolean;

  @Input() currentProjectNr? = '';
  @Input() currentProjectId? = 0;
  @Input() showAllProjects = false;
  @Input() excludeProjectId: number = 0;
  @Output() ProjectSelected: EventEmitter<IProjectSearchResultDTO> =
    new EventEmitter<IProjectSearchResultDTO>();
  @Output() rowDoubleClicked = new EventEmitter<IProjectSearchResultDTO>();
  flowHandler = inject(FlowHandlerService);
  selectProjectService = inject(SelectProjectService);
  progressService = inject(ProgressService);
  coreService = inject(CoreService);
  performProjectLoad = new Perform<IProjectSearchResultDTO[]>(
    this.progressService
  );
  searching = false;
  setupFinished = false;
  _showHidden = false;
  onlyMineLocked = signal(true);
  allProjects: IProjectSearchResultDTO[] = [];

  get showHidden(): boolean {
    return this._showHidden;
  }
  set showHidden(item: boolean) {
    this._showHidden = item;
  }

  ngOnInit(): void {
    super.ngOnInit();
    this.startFlow(
      Feature.Billing_Purchase_Purchase_List,
      'common.dialogs.searchprojects',
      { skipInitialLoad: true }
    );

    this.form?.showWithoutCustomer.valueChanges.subscribe((x: boolean) => {
      if (this.setupFinished) {
        this.saveBoolSetting(x);
      }
    });
    this.form?.showFindHidden.valueChanges.subscribe((x: boolean) => {
      if (this.setupFinished) {
        this.loadProjects();
      }
    });
  }

  ngOnDestroy(): void {
    this.grid.onRowDoubleClicked = () => {};
  }

  saveBoolSetting(showWithoutCustomer: boolean): Observable<any> {
    const model = new SaveUserCompanySettingModel(
      SettingMainType.User,
      UserSettingType.ProjectDefaultExcludeMissingCustomer,
      showWithoutCustomer
    );

    return of(
      this.coreService.saveBoolSetting(model).pipe(
        tap(val => {
          this.loadProjects();
        })
      )
    );
  }

  override createGridToolbar(): void {
    super.createGridToolbar({
      hideReload: true,
      hideClearFilters: true,
    });
  }

  override onGridReadyToDefine(grid: GridComponent<IProjectSearchResultDTO>) {
    super.onGridReadyToDefine(grid);

    this.grid.api.updateGridOptions({
      onFilterModified: this.onFilterModified.bind(this),
    });
    this.translate
      .get([
        'common.number',
        'common.name',
        'billing.projects.list.status',
        'billing.projects.list.number',
        'billing.projects.list.name',
        'billing.projects.list.info',
        'billing.projects.list.categories',
        'billing.projects.list.customer',
        'billing.projects.list.underproject',
        'billing.projects.list.openprojectcentral',
        'billing.projects.list.customernr',
        'billing.projects.list.leader',
        'billing.projects.list.ordernr',
        'core.edit',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.enableRowSelection();
        this.grid.addColumnText(
          'number',
          terms['billing.projects.list.number'],
          {
            flex: 1,
            editable: false,
          }
        );
        this.grid.addColumnText('name', terms['billing.projects.list.name'], {
          flex: 1,
          editable: false,
        });
        this.grid.addColumnText(
          'customerNr',
          terms['billing.projects.list.customernr'],
          {
            flex: 1,
            editable: false,
          }
        );
        this.grid.addColumnText(
          'customerName',
          terms['billing.projects.list.customer'],
          {
            flex: 1,
            editable: false,
          }
        );
        this.grid.addColumnText(
          'managerName',
          terms['billing.projects.list.leader'],
          {
            flex: 1,
            editable: false,
          }
        );
        this.grid.addColumnText(
          'orderNr',
          terms['billing.projects.list.ordernr'],
          {
            flex: 1,
            editable: false,
          }
        );

        this.grid.context.suppressGridMenu = true;

        this.grid.onRowDoubleClicked = (event: any) => {
          this.rowDoubleClicked.emit(event.data);
        };
        this.grid.setRowSelection('singleRow');
        super.finalizeInitGrid();

        if (this.customerId && this.customerId > 0) {
          this.loadProjects(
            this.currentProjectId && this.currentProjectNr && this.useDelete
              ? this.currentProjectNr
              : undefined
          );
        } else if (
          this.currentProjectId &&
          this.currentProjectNr &&
          this.useDelete
        ) {
          this.loadProjects(this.currentProjectNr);
        }
      });
  }

  selectionChanged($event: any): void {
    this.setSelectedProject();
  }

  onFilterModified() {
    debounce(() => {
      if (!this.searching) {
        this.loadProjects();
      }
    }, 500)();
  }

  loadProjects(overrideNr: any = undefined) {
    this.searching = true;
    this.allProjects = [];
    const filterModels = this.grid.agGrid.api.getFilterModel();

    if (!filterModels && this.grid.agGrid.api.getDisplayedRowCount() == 0) {
      return;
    }

    const columnValueNumber = overrideNr
      ? overrideNr
      : filterModels['number']
        ? filterModels['number'].filter
        : '';
    const columnValueName = filterModels['name']
      ? filterModels['name'].filter
      : '';
    const columnValueCustomerNumber = filterModels['customerNr']
      ? filterModels['customerNr'].filter
      : '';
    const columnValueCustomerName = filterModels['customerName']
      ? filterModels['customerName'].filter
      : '';
    const columnValueManagerName = filterModels['managerName']
      ? filterModels['managerName'].filter
      : '';
    const columnValueOrderNr = filterModels['orderNr']
      ? filterModels['orderNr'].filter
      : '';
    if (
      !columnValueNumber &&
      !columnValueName &&
      !columnValueCustomerNumber &&
      !columnValueCustomerName &&
      !columnValueManagerName &&
      !columnValueOrderNr &&
      !this.customerId &&
      this.grid.agGrid.api.getDisplayedRowCount() == 0
    ) {
      this.searching = false;
      return;
    }
    const model = new ProjectSearchModel(
      columnValueNumber,
      columnValueName,
      columnValueCustomerNumber,
      columnValueCustomerName,
      columnValueManagerName,
      columnValueOrderNr,
      true,
      this.form?.getRawValue().showFindHidden,
      this.form?.getRawValue().showWithoutCustomer,
      this.form?.getRawValue().showMine,
      this.customerId && this.customerId > 0 ? this.customerId : undefined,
      this.showAllProjects
    );
    return of(
      this.performProjectLoad.load(
        this.selectProjectService.getProjectsBySearch(model).pipe(
          tap(projects => {
            this.allProjects =
              this.excludeProjectId && this.excludeProjectId > 0
                ? projects.filter(p => p.projectId !== this.excludeProjectId)
                : projects;
            this.grid.setData(this.allProjects);
            if (this.currentProjectId && this.currentProjectId > 0) {
              this.selectCurrentProject();
            } else this.selectFirstRow();

            this.setSelectedProject();

            this.searching = false;
          })
        )
      )
    );
  }

  selectFirstRow() {
    this.grid.agGrid.api.getRowNode('0')?.setSelected(true);
  }

  selectCurrentProject() {
    if (this.currentProjectId && this.currentProjectId > 0) {
      for (let i = 0; i < this.allProjects.length; i++) {
        if (
          this.grid.agGrid.api.getRowNode(i.toString())?.data.projectId ==
          this.currentProjectId
        ) {
          this.grid.agGrid.api.getRowNode(i.toString())?.setSelected(true);
          break;
        }
      }
    }
  }
  setSelectedProject() {
    const selectedProject = this.grid.getSelectedRows()[0];
    this.ProjectSelected.emit(selectedProject);
  }
}

import { Component, inject, OnInit } from '@angular/core';
import { AccountDimSmallDTO } from '@shared/components/accounting-rows/models/accounting-rows-model';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import {
  Feature,
  TermGroup,
} from '@shared/models/generated-interfaces/Enumerations';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import { IProjectGridDTO } from '@shared/models/generated-interfaces/ProjectDTOs';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ProgressService } from '@shared/services/progress/progress.service';
import { BrowserUtil } from '@shared/util/browser-util';
import { Perform } from '@shared/util/perform.class';
import { GridComponent } from '@ui/grid/grid.component';
import { MenuButtonItem } from '@ui/button/menu-button/menu-button.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { take, tap } from 'rxjs';
import { ProjectService } from '../../services/project.service';
import { ProjectUrlParamsService } from '../../services/project-url.service';
import { ProjectExtendedGridDTO } from '../../models/project.model';
import { MultiValueCellRenderer } from '@ui/grid/cell-renderers/multi-value-cell-renderer/multi-value-cell-renderer.component';

@Component({
  selector: 'soe-project-grid',
  templateUrl: './project-grid.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class ProjectGridComponent
  extends GridBaseDirective<ProjectExtendedGridDTO, ProjectService>
  implements OnInit
{
  service = inject(ProjectService);
  coreService = inject(CoreService);
  progressService = inject(ProgressService);
  performLoad = new Perform<any>(this.progressService);
  urlService = inject(ProjectUrlParamsService);

  projectStatuses!: ISmallGenericType[];
  projectStatusSelections!: number[];
  accountDims: AccountDimSmallDTO[] = [];
  showMine: boolean = false;
  buttonFunctions: MenuButtonItem[] = [];

  // Permissions
  private onlyMineLocked: boolean = false;
  private hasEditProjectPermission: boolean = false;
  private hasProjectCentralPermission: boolean = false;

  ngOnInit(): void {
    super.ngOnInit();

    this.startFlow(Feature.Billing_Project_Edit, 'Billing.Projects.Project', {
      additionalModifyPermissions: [
        Feature.Billing_Project_Edit,
        Feature.Billing_Project_ProjectsUser,
        Feature.Billing_Project_Central,
      ],
      lookups: [this.loadProjectStatus(), this.loadAccountDims()],
    });
    this.projectStatusSelections = this.additionalGridProps().projectStatuses;
  }

  override onPermissionsLoaded() {
    super.onPermissionsLoaded();

    this.hasEditProjectPermission = this.flowHandler.hasModifyAccess(
      Feature.Billing_Project_Edit
    );
    this.onlyMineLocked = this.flowHandler.hasModifyAccess(
      Feature.Billing_Project_ProjectsUser
    );
    this.hasProjectCentralPermission = this.flowHandler.hasModifyAccess(
      Feature.Billing_Project_Central
    );
  }

  override onFinished() {
    const projectid = this.urlService.projectId();

    if (projectid) {
      this.edit({ projectId: projectid } as ProjectExtendedGridDTO);
    }
  }

  protected override refreshGrid(): void {
    this.reloadFilteredGridData(this.projectStatusSelections, this.showMine);
  }

  override onGridReadyToDefine(
    grid: GridComponent<ProjectExtendedGridDTO>
  ): void {
    super.onGridReadyToDefine(grid);
    this.translate
      .get([
        'billing.projects.list.project',
        'billing.projects.list.status',
        'billing.projects.list.number',
        'billing.projects.list.name',
        'billing.projects.list.info',
        'billing.projects.list.categories',
        'billing.projects.list.customer',
        'billing.projects.list.underproject',
        'billing.projects.list.openprojectcentral',
        'billing.projects.list.leader',
        'core.edit',
        'common.all',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.enableRowSelection();
        this.grid.addColumnText(
          'number',
          terms['billing.projects.list.number'],
          { flex: 1 }
        );
        this.grid.addColumnText('name', terms['billing.projects.list.name'], {
          flex: 1,
        });
        this.grid.addColumnText(
          'description',
          terms['billing.projects.list.info'],
          { flex: 1, enableHiding: true }
        );

        this.grid.addColumnText(
          'categoriesArray',
          terms['billing.projects.list.categories'],
          {
            flex: 1,
            enableHiding: true,
            cellRenderer: MultiValueCellRenderer,
            filter: 'agSetColumnFilter',
          }
        );

        this.grid.addColumnText(
          'customerName',
          terms['billing.projects.list.customer'],
          { flex: 1 }
        );
        this.grid.addColumnText(
          'childProjects',
          terms['billing.projects.list.underproject'],
          { flex: 1, enableHiding: true }
        );
        this.grid.addColumnText(
          'managerName',
          terms['billing.projects.list.leader'],
          { flex: 1 }
        );

        if (this.accountDims) {
          this.accountDims.forEach((ad, i) => {
            const index = i + 1;
            if (ad.accountDimNr !== 1) {
              this.grid.addColumnText(
                'defaultDim' + index + 'AccountName',
                ad.name,
                { flex: 1, enableHiding: true, hide: true }
              );
            }
          });
        }

        this.grid.addColumnText(
          'statusName',
          terms['billing.projects.list.status'],
          { flex: 1, enableHiding: true }
        );

        if (this.hasProjectCentralPermission)
          this.grid.addColumnIcon(
            null,
            terms['billing.projects.list.openprojectcentral'],
            {
              iconName: 'calculator-alt',
              enableHiding: false,
              tooltip: terms['billing.projects.list.openprojectcentral'],
              suppressExport: true,
              onClick: row => this.openProjectCentral(row),
            }
          );

        if (this.hasEditProjectPermission)
          this.grid.addColumnIconEdit({
            tooltip: terms['core.edit'],
            onClick: row => this.edit(row),
          });

        super.finalizeInitGrid();
      });
  }

  private loadAccountDims() {
    return this.coreService
      .getAccountDimsSmall(
        false,
        false,
        false,
        true,
        false,
        false,
        false,
        false,
        true
      )
      .pipe(
        tap(dims => {
          this.accountDims = dims;
        })
      );
  }

  private loadProjectStatus() {
    return this.coreService
      .getTermGroupContent(TermGroup.ProjectStatus, false, false)
      .pipe(
        tap(statuses => {
          this.projectStatuses = statuses;
          statuses.forEach((status, i) => {
            this.buttonFunctions.push({ id: status.id, label: status.name });
          });
        })
      );
  }

  setProjectStatusSelection(values: number[]) {
    this.projectStatusSelections = values;
    this.reloadFilteredGridData(this.projectStatusSelections, this.showMine);
  }

  setShowMineSelection(value: boolean) {
    this.showMine = this.onlyMineLocked ? true : value;
    this.reloadFilteredGridData(this.projectStatusSelections, this.showMine);
  }

  private reloadFilteredGridData(projectStatuses: number[], showMine: boolean) {
    this.performLoad.load(
      this.service
        .getGrid(undefined, { projectStatuses, onlyMine: showMine })
        .pipe(
          tap(x => {
            this.rowData?.next(x);
          })
        )
    );
  }

  private openProjectCentral(row: IProjectGridDTO) {
    const url = `/soe/billing/project/central/?project=${row.projectId}`;

    BrowserUtil.openInNewTab(window, url);
  }

  executeButtonFunction(option: MenuButtonItem) {
    if (!option.id) return;

    this.transferStatus(option.id);
  }

  private transferStatus(newState: number) {
    const dict: number[] = [];
    const rows = this.grid.getSelectedRows();

    if (rows.length === 0) return;

    rows.forEach((row, i) => {
      dict.push(row.projectId);
    });

    this.service.updateProjectStatus(dict, newState).subscribe(result => {
      if (result.success) {
        this.refreshGrid();
      }
    });
  }
}

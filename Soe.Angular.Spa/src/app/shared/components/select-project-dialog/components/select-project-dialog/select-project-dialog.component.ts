import { Component, inject, signal } from '@angular/core';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { DialogComponent } from '@ui/dialog/dialog/dialog.component';
import {
  ProjectSearchResultDTO,
  SelectProjectDialogData,
  SelectProjectDialogFormDTO,
} from '../../models/select-project-dialog.model';
import { IProjectSearchResultDTO } from '@shared/models/generated-interfaces/ProjectDTOs';
import {
  Feature,
  UserSettingType,
} from '@shared/models/generated-interfaces/Enumerations';
import { ValidationHandler } from '@shared/handlers';
import { SelectProjectDialogForm } from '../../models/select-project-dialog-form.model';
import { of, tap } from 'rxjs';
import { SettingsUtil } from '@shared/util/settings-util';
import { CoreService } from '@shared/services/core.service';

@Component({
  selector: 'soe-select-project-dialog',
  templateUrl: './select-project-dialog.component.html',
  styleUrls: ['./select-project-dialog.component.scss'],
  providers: [FlowHandlerService],
  standalone: false,
})
export class SelectProjectDialogComponent extends DialogComponent<SelectProjectDialogData> {
  selectedProject!: ProjectSearchResultDTO;
  customerId!: number;
  projectsWithoutCustomer = false;
  showFindHidden = false;
  loadHidden = false;
  useDelete? = false;
  currentProjectNr?: string;
  currentProjectId?: number;
  handler = inject(FlowHandlerService);
  validationHandler = inject(ValidationHandler);
  coreService = inject(CoreService);
  hideProjectsWithoutCustomer = signal(true);
  hideShowFindHidden = signal(true);
  showAllProjects!: boolean;
  excludeProjectId?: number;

  form: SelectProjectDialogForm = new SelectProjectDialogForm({
    validationHandler: this.validationHandler,
    element: new SelectProjectDialogFormDTO(),
  });

  constructor() {
    super();
    this.selectedProject = new ProjectSearchResultDTO();
    this.setDialogParam();
    this.handler.execute({
      permission: Feature.Billing_Project_ProjectsUser,
    });
    this.setDisableOnControllers();
    this.setVisibilityOnControllers();
  }

  setDialogParam() {
    if (this.data) {
      if (this.data.customerId) {
        this.customerId = this.data.customerId;
      }
      if (this.data.projectsWithoutCustomer) {
        this.projectsWithoutCustomer = this.data.projectsWithoutCustomer;
        this.form.patchValue({
          projectsWithoutCustomer: this.projectsWithoutCustomer,
        });
      }
      if (this.data.showFindHidden) {
        this.showFindHidden = this.data.showFindHidden;
        this.form.patchValue({ showFindHidden: this.showFindHidden });
      }
      if (this.data.loadHidden) {
        this.loadHidden = this.data.loadHidden;
      }
      if (this.data.useDelete) {
        this.useDelete = this.data.useDelete;
      }
      if (this.data.currentProjectNr) {
        this.currentProjectNr = this.data.currentProjectNr;
      }

      if (this.data.currentProjectId) {
        this.currentProjectId = this.data.currentProjectId;
      }

      this.showAllProjects = this.data.showAllProjects ?? false;
      this.excludeProjectId = this.data.excludeProjectId;
    }
  }

  setVisibilityOnControllers() {
    this.hideShowFindHidden.set(this.loadHidden);

    if (this.projectsWithoutCustomer) {
      this.getProjectsWithoutCustomerSetting();
    } else {
      this.hideProjectsWithoutCustomer.set(false);
      this.form.patchValue({ showWithoutCustomer: true });
    }
  }

  setDisableOnControllers() {
    const onlyMineLocked = this.handler.modifyPermission();
    this.form.patchValue({ showMine: onlyMineLocked });
    if (onlyMineLocked) {
      this.form.showMine.disable();
    } else {
      this.form.showMine.enable();
    }
  }

  getProjectsWithoutCustomerSetting() {
    return of(
      this.coreService
        .getUserSettings(
          [UserSettingType.ProjectDefaultExcludeMissingCustomer],
          false
        )
        .pipe(
          tap(data => {
            const val = SettingsUtil.getBoolUserSetting(
              data,
              UserSettingType.ProjectDefaultExcludeMissingCustomer,
              false
            );
            this.form.patchValue({
              showWithoutCustomer: val,
            });
          })
        )
    );
  }

  onProjectSelected(project: IProjectSearchResultDTO) {
    this.selectedProject = project;
  }

  onRowDoublClicked(data: IProjectSearchResultDTO) {
    this.onProjectSelected(data);
    this.selectProject();
  }

  cancel() {
    this.dialogRef.close(false);
  }

  protected selectProject(): void {
    this.dialogRef.close(this.selectedProject);
  }
}

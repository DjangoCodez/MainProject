import { Component, OnInit, Input, inject, signal } from '@angular/core';
import { take, tap } from 'rxjs/operators';
import { ExportStandardDefinitionsService } from '../../services/export-standard-definitions.service';
import {
  CompanySettingType,
  Feature,
  SoeEntityState,
  TermGroup,
} from '@shared/models/generated-interfaces/Enumerations';
import {
  ExportStandardDefinitionsLevelEditComponent,
  IExportDefinitionLevelEventObject,
  IExportStandardDefinitionsLevelDialogData,
} from './export-standard-definitions-level-edit/export-standard-definitions-level-edit.component';
import { DialogService } from '@ui/dialog/services/dialog.service';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import {
  ExportDefinitionDTO,
  ExportDefinitionLevelColumnDTO,
} from '../../../../models/export.model';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ExportStandardDefinitionForm } from '../../models/export-standard-definition-form.model';
import { CrudActionTypeEnum } from '@shared/enums';
import { Perform } from '@shared/util/perform.class';
import { SettingsUtil } from '@shared/util/settings-util';
import { ExportStandardDefinitionLevelColumnForm } from '../../models/export-standard-definition-level-column-form.model';
import { ExportStandardDefinitionLevelForm } from '../../models/export-standard-definition-level-form.model';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { Observable } from 'rxjs';
import { EditBaseDirective } from '@shared/directives/edit-base/edit-base.directive';
import { cloneDeep } from 'lodash';

@Component({
  selector: 'soe-export-standard-definitions-edit',
  templateUrl: './export-standard-definitions-edit.component.html',
  styleUrls: ['./export-standard-definitions-edit.component.scss'],
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class ExportStandardDefinitionsEditComponent
  extends EditBaseDirective<
    ExportDefinitionDTO,
    ExportStandardDefinitionsService
  >
  implements OnInit
{
  @Input() form: ExportStandardDefinitionForm | undefined;

  private readonly definitionsService = inject(
    ExportStandardDefinitionsService
  );
  private readonly coreService = inject(CoreService);
  private readonly dialogService = inject(DialogService);
  service = inject(ExportStandardDefinitionsService);
  performDefinitions = new Perform<SmallGenericType[]>(this.progressService);

  useAccountsHierarchy = signal(false); // This is not used - should it be?

  levelsToolbarService = inject(ToolbarService);
  levelColumnsToolbarService = inject(ToolbarService);

  private _selectedType = 0;
  get selectedType(): number {
    return this._selectedType;
  }
  set selectedType(value: number) {
    this._selectedType = value;

    if (this.form) {
      this.form.patchValue({ type: this._selectedType });
    }
  }

  ngOnInit() {
    super.ngOnInit();

    this.startFlow(Feature.Time_Export_StandardDefinitions, {
      lookups: this.loadDefinitionTypes(),
    });
  }

  loadData(): Observable<void> {
    return this.performLoadData.load$(
      this.definitionsService.get(this.form?.getIdControl()?.value).pipe(
        tap(value => {
          this.form?.customPatchValue(value);
          this.form?.markAsPristine();
          this.form?.markAsUntouched();
        })
      )
    );
  }

  override onFinished(): void {
    this.setupToolbar();
  }

  // SETUP

  private setupToolbar(): void {
    if (!this.flowHandler.modifyPermission()) return;

    this.levelsToolbarService.createItemGroup({
      items: [
        this.levelsToolbarService.createToolbarButton('new', {
          iconName: signal('plus'),
          caption: signal('time.export.standarddefinitionlevel.new'),
          tooltip: signal('time.export.standarddefinitionlevel.new'),
          onAction: () => {
            this.editLevel();
            this.form?.markAsDirty();
          },
        }),
      ],
    });

    this.levelColumnsToolbarService.createItemGroup({
      items: [
        this.levelColumnsToolbarService.createToolbarButton('new', {
          iconName: signal('plus'),
          caption: signal('time.export.standarddefinitionlevelcolumn.new'),
          tooltip: signal('time.export.standarddefinitionlevelcolumn.new'),
          onAction: () => {
            this.addLevelColumn();
            this.form?.markAsDirty();
          },
        }),
      ],
    });
  }

  loadCompanySettings() {
    return this.coreService
      .getCompanySettings([CompanySettingType.UseAccountHierarchy])
      .pipe(
        tap(setting => {
          this.useAccountsHierarchy.set(
            SettingsUtil.getBoolCompanySetting(
              setting,
              CompanySettingType.UseAccountHierarchy
            )
          );
        })
      );
  }

  loadDefinitionTypes() {
    return this.performDefinitions.load$(
      this.coreService.getTermGroupContent(
        TermGroup.SysExportDefinitionType,
        false,
        false
      )
    );
  }

  activeChanged(value: boolean) {
    this.form?.patchValue({
      state: value ? SoeEntityState.Active : SoeEntityState.Inactive,
    });
    this.form?.markAsDirty();
  }

  selectLevel(level: any) {
    this.form!.selectedLevelForm = level;
  }

  editLevel(form?: any) {
    const isNew =
      !form?.value.exportDefinitionLevelId ||
      form.value.exportDefinitionLevelId === 0;

    this.dialogService
      .open(ExportStandardDefinitionsLevelEditComponent, {
        title: form
          ? 'time.export.standarddefinitionlevel.edit'
          : 'time.export.standarddefinitionlevel.new',
        size: 'lg',
        hideFooter: true,
        form: cloneDeep(form),
      } as IExportStandardDefinitionsLevelDialogData)
      .afterClosed()
      .pipe(take(1))
      .subscribe(({ object, action }: IExportDefinitionLevelEventObject) => {
        if (action === CrudActionTypeEnum.Save) {
          isNew
            ? this.form?.addLevelForm(object)
            : form?.patchValue(object!.value);
          this.form?.markAsDirty();
        } else if (action === CrudActionTypeEnum.Delete && object) {
          this.removeLevel(object);
        }
      });
  }

  removeLevel(levelForm: ExportStandardDefinitionLevelForm) {
    this.form?.exportDefinitionLevels.value.forEach(
      (el: ExportStandardDefinitionForm, i: number) => {
        el.value === levelForm.value &&
          this.form?.exportDefinitionLevels.removeAt(i);
      }
    );
  }

  addLevelColumn() {
    const column = new ExportDefinitionLevelColumnDTO();
    this.form?.selectedLevelForm?.addColumnForm(column);
  }

  deleteLevelColumn(index: number) {
    this.form?.selectedLevelForm?.removeColumnForm(index);
  }

  getColumnHeadName(
    columnForm: ExportStandardDefinitionLevelColumnForm
  ): string {
    return <number>columnForm.controls.exportDefinitionLevelColumnId.value > 0
      ? columnForm.controls.name.value
      : this.translate.instant('time.export.standarddefinitionlevelcolumn.new');
  }

  orderBy(collection: any[], field: string) {
    return collection.sort((a, b) =>
      a[field] > b[field] ? 1 : a[field] === b[field] ? 0 : -1
    );
  }
}

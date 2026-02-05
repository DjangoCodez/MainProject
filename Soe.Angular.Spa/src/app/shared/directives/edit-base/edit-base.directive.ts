import {
  ChangeDetectorRef,
  Directive,
  Input,
  OnChanges,
  OnInit,
  SimpleChanges,
  WritableSignal,
  inject,
  input,
  signal,
} from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { CrudActionTypeEnum } from '@shared/enums';
import { SoeFormControl, SoeFormGroup } from '@shared/extensions';
import {
  FlowHandlerOptions,
  FlowHandlerService,
} from '@shared/services/flow-handler.service';
import { ProgressService } from '@shared/services/progress/progress.service';
import { Guid } from '@shared/util/string-util';
import { Perform } from '@shared/util/perform.class';
import { pairwise, startWith, take, tap } from 'rxjs/operators';
import { Observable, of } from 'rxjs';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { ProgressOptions } from '@shared/services/progress';
import { IMessageboxComponentResponse } from '@ui/dialog/models/messagebox';
import { MessageboxService } from '@ui/dialog/services/messagebox.service';
import { NavigatorRecordConfig } from '@ui/record-navigator/record-navigator.component';
import { OpenEditInNewTab } from '@ui/tab/models/multi-tab-wrapper.model';
import {
  ToolbarEditConfig,
  ToolbarEditOptions,
} from '@ui/toolbar/models/toolbar';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { ToolbarUtils } from '@ui/toolbar/utils/toolbar.utils';
import {
  Feature,
  SoeEntityState,
} from '@shared/models/generated-interfaces/Enumerations';
import { cloneDeep } from 'lodash';
import { TermCollection } from '@shared/localization/term-types';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';
import { ResponseUtil } from '@shared/util/response-util';

export interface IApiService<T> {
  save: (item: T, additionalData?: any) => Observable<BackendResponse>;
  delete: (id: number, additionalData?: any) => Observable<BackendResponse>;
  // Used for calling update on the grid after save
  getGrid?: (id?: number, additionalProps?: any) => Observable<any[]>;
}

abstract class IEditBase {
  protected loadDataFn?: () => Observable<any>;
}

export interface ActionTaken {
  type: CrudActionTypeEnum;
  additionalProps?:
    | CrudActionAdditionalSaveProps
    | CrudActionAdditionalDeleteProps;
  ref?: string;
  rowItemId: number;
  form?: SoeFormGroup;
  updateGrid?: () => Observable<any[]>;
}

export interface CopyActionTaken {
  ref?: string;
  form: any;
  filteredRows: any[];
  additionalProps?: any;
}

export interface SetNewRefOnTab {
  ref: Guid;
  newRef: Guid;
  isNew: boolean;
}

export type CrudActionAdditionalSaveProps =
  | Partial<{
      closeTabOnSave: boolean;
      keepNewFormOnAfterSave: boolean;
    }>
  | any;

export type CrudActionAdditionalDeleteProps =
  | Partial<{
      skipUpdateGrid: boolean;
    }>
  | any;

@Directive({
  selector: '[soeEditBase]',
  standalone: true,
})
export class EditBaseDirective<
    R,
    T extends IApiService<R>,
    FormType extends SoeFormGroup = SoeFormGroup,
  >
  extends IEditBase
  implements OnInit, OnChanges
{
  @Input() form: FormType | undefined;
  @Input() selectedRecord?: SmallGenericType;
  ref = input('');
  // This is used for the multi-tab-wrapper
  actionTakenSignal = input<WritableSignal<ActionTaken | undefined>>(
    signal(undefined)
  );
  copyActionTakenSignal = input<WritableSignal<CopyActionTaken | undefined>>(
    signal(undefined)
  );
  openEditInNewTabSignal = input<WritableSignal<OpenEditInNewTab | undefined>>(
    signal(undefined)
  );
  setNewRefOnTabSignal = input<WritableSignal<SetNewRefOnTab | undefined>>(
    signal(undefined)
  );
  // This is used for the edit-component-dialog
  closeDialogSignal = input<
    WritableSignal<BackendResponse | boolean | undefined>
  >(signal(undefined));

  addOptionId = input<number | undefined>(undefined);

  translate = inject(TranslateService);
  messageboxService = inject(MessageboxService);
  flowHandler = inject(FlowHandlerService);
  progressService = inject(ProgressService);
  cdr = inject(ChangeDetectorRef);
  service!: T;
  /** Extra config data that will be passed to the service on change */
  additionalSaveData?: any;
  /** Extra config data that will be passed to the service on delete */
  additionalDeleteData?: any;
  /** Extra config data that will be passed all the way to the TabWrapper on change */
  additionalSaveProps?: CrudActionAdditionalSaveProps;
  /** Extra config data that will be passed all the way to the TabWrapper on delete */
  additionalDeleteProps?: CrudActionAdditionalDeleteProps;
  performLoadData = new Perform<any>(this.progressService);
  performAction = new Perform<any>(this.progressService);

  recordConfig = new NavigatorRecordConfig();
  toolbarService = inject(ToolbarService);
  toolbarUtils = new ToolbarUtils();
  copyDisabled = signal(true);

  // Form state signals
  isFormDirty = signal(false);
  isFormPristine = signal(true);
  isFormValid = signal(false);
  isFormInvalid = signal(true);

  terms: TermCollection = {};

  get idFieldName() {
    return this.form?.idFieldName || '';
  }

  ngOnChanges({ selectedRecord }: SimpleChanges): void {
    if (selectedRecord?.currentValue && !selectedRecord.firstChange) {
      this.form?.patchValue({
        [this.idFieldName]: selectedRecord.currentValue.id,
      });
    }
  }

  ngOnInit(): void {
    if (!this.service)
      console.error(
        "A service hasn't been initalized in the container component where this directive is extended from."
      );
    if (!this.flowHandler)
      console.error(
        'The flow-handler needs to be added as a provider: [FlowHandlerService] in the container component.'
      );

    this.form
      ?.getIdControl()
      ?.valueChanges.pipe(startWith(null), pairwise())
      .subscribe(([prev, next]: [number, number]) => {
        if (prev && next && prev !== next) {
          this.loadDataFn && this.loadDataFn().pipe(take(1)).subscribe();
        }
      });

    // Check form VALID/INVALID changes
    this.form?.statusChanges.subscribe((status: string) => {
      this.isFormValid.set(status === 'VALID');
      this.isFormInvalid.set(status === 'INVALID');
    });

    // Check form dirty/pristine changes
    this.form?.valueChanges.subscribe(() => {
      this.isFormDirty.set(this.form?.dirty || false);
      this.isFormPristine.set(this.form?.pristine || false);
    });
  }

  startFlow(
    permission: Feature,
    options?: Omit<
      FlowHandlerOptions,
      | 'terms'
      | 'companySettings'
      | 'userSettings'
      | 'onPermissionsLoaded'
      | 'onSettingsLoaded'
      | 'setupDefaultToolbar'
      | 'data'
      | 'skipInitialLoad'
      | 'onFinished'
    > // Omit since they are implemented in sub class and not passed as options
  ) {
    this.loadDataFn = this.loadData;

    this.flowHandler.options = {
      permission: permission,
      additionalReadPermissions: options?.additionalReadPermissions,
      additionalModifyPermissions: options?.additionalModifyPermissions,
      terms: this.loadTerms(),
      companySettings: this.loadCompanySettings(),
      userSettings: this.loadUserSettings(),
      lookups: options?.lookups,
      skipDefaultToolbar: options?.skipDefaultToolbar,
      useLegacyToolbar: options?.useLegacyToolbar,
      onPermissionsLoaded: this.onPermissionsLoaded.bind(this),
      onSettingsLoaded: this.onSettingsLoaded.bind(this),
      onFinished: this.onFinished.bind(this),
    };

    // Default functionality for load data and new record if not overrided in edit component
    this.flowHandler.options.data = this.form?.getIdControl()?.value
      ? this.loadData()
      : this.newRecord();

    // If nothing is specified in edit component, disableControlsByPermission will be called after permissions are loaded
    // this.flowHandler.options.permissionsLoaded = options?.permissionsLoaded
    //   ? options?.permissionsLoaded
    //   : this.disableControlsByPermission.bind(this);

    // Create default toolbar if not skipped in edit component
    if (!this.flowHandler.options.skipDefaultToolbar) {
      if (this.flowHandler.options.useLegacyToolbar) {
        this.flowHandler.options.setupDefaultToolbar =
          this.createLegacyEditToolbar.bind(this);
      } else {
        this.flowHandler.options.setupDefaultToolbar =
          this.createEditToolbar.bind(this);
      }
    }

    this.flowHandler.executeForEdit();
  }

  onPermissionsLoaded(): void {
    // Override in edit component
    this.disableControlsByPermission();
  }

  loadTerms(translationsKeys: string[] = []): Observable<TermCollection> {
    // Override compleately in edit component or call super.loadTerms and pass translationsKeys

    if (translationsKeys.length > 0) {
      return this.translate.get(translationsKeys).pipe(
        tap(terms => {
          this.terms = terms;
        })
      );
    }

    return of({});
  }

  loadCompanySettings(): Observable<void> {
    // Override in edit component
    return of(undefined);
  }

  loadUserSettings(): Observable<void> {
    // Override in edit component
    return of(undefined);
  }

  onSettingsLoaded(): void {
    // Override in edit component
  }

  loadData(): Observable<void> {
    // Override in edit component
    return this.performLoadData.load$(
      (<any>this.service).get(this.form?.getIdControl()?.value).pipe(
        tap(value => {
          this.form?.reset(value);
        })
      ),
      { showDialogDelay: 1000 }
    );
  }

  onFinished(): void {
    // Override in edit component
  }

  onTabActivated(): void {
    // Override in edit component
  }

  newRecord(): Observable<void> {
    // Override in edit component
    return of(undefined);
  }

  copy(additionalProps?: any): void {
    if (this.form) {
      const newForm: FormType = cloneDeep(this.form);

      // Clear some values
      newForm.getIdControl()?.patchValue(0);
      newForm.clearIfExists(<SoeFormControl>newForm.controls.created);
      newForm.clearIfExists(<SoeFormControl>newForm.controls.createdBy);
      newForm.clearIfExists(<SoeFormControl>newForm.controls.modified);
      newForm.clearIfExists(<SoeFormControl>newForm.controls.modifiedBy);

      this.copyActionTakenSignal()?.set({
        ref: this.ref(),
        form: newForm,
        filteredRows: this.form.records,
        additionalProps: additionalProps,
      });
    }
  }

  openEditInNewTab(data: OpenEditInNewTab) {
    this.openEditInNewTabSignal()?.set(data);
  }

  createLegacyEditToolbar(
    options: Partial<ToolbarEditOptions> = new ToolbarEditOptions()
  ): void {
    this.toolbarUtils.createDefaultLegacyEditToolbar(
      {
        onClick: () => this.copy(),
        disabled: this.copyDisabled,
      },
      options
    );
  }

  createEditToolbar(config?: Partial<ToolbarEditConfig>) {
    this.toolbarService.createDefaultEditToolbar(
      config || this.getDefaultToolbarOptions()
    );
  }

  protected getDefaultToolbarOptions(): Partial<ToolbarEditConfig> {
    return {
      copyOption: {
        disabled: this.copyDisabled,
        onAction: () => this.copy(),
      },
    };
  }

  disableControlsByPermission() {
    if (!this.flowHandler.modifyPermission()) this.form?.disable();
    this.copyDisabled.set(
      !this.flowHandler.modifyPermission() || !this.form?.getIdControl()?.value
    );
  }

  navigatorRecordChanged(record: SmallGenericType) {
    if (!this.form || record.id === this.form?.getIdControl()?.value) return;

    this.form.patchValue({
      [this.form.idFieldName]: record.id,
    });
  }

  performSave(options?: ProgressOptions, skipLoadData = false): void {
    if (!this.form || this.form.invalid || !this.service) return;
    this.performAction.crud(
      CrudActionTypeEnum.Save,
      this.service.save(this.form?.getRawValue(), this.additionalSaveData).pipe(
        tap(res => {
          console.log('res: ', res);
          this.updateFormValueAndEmitChange(res, skipLoadData);
          if (res.success) this.triggerCloseDialog(res);
        })
      ),
      options?.callback,
      options?.errorCallback,
      options
    );
  }

  updateFormValueAndEmitChange = (
    backendResponse: BackendResponse,
    skipLoadData = false
  ) => {
    const entityId = ResponseUtil.getEntityId(backendResponse);

    if (entityId && entityId !== 0) {
      this.form?.patchValue({ [this.idFieldName]: entityId });

      this.form?.markAsPristine();
      this.form?.markAsUntouched();

      if (!skipLoadData) this.loadData().subscribe();

      const action: ActionTaken = {
        rowItemId: entityId,
        ref: this.ref(),
        type: CrudActionTypeEnum.Save,
        form: this.form,
        additionalProps: this.additionalSaveProps,
      };

      if (this.service.getGrid) {
        action.updateGrid = () => {
          return this.service.getGrid!(entityId);
        };
      }

      this.actionTakenSignal().set(action);
      //this.updateToolbarProps();
      this.disableControlsByPermission();

      this.onSaveCompleted(backendResponse);
    }
  };

  onSaveCompleted(backendResponse: BackendResponse): void {
    //Override in edit component
  }

  // updateToolbarProps() {
  //   if (
  //     this.flowHandler.modifyPermission() &&
  //     this.form?.getIdControl()?.value
  //   ) {
  //     this.toolbarService.setButtonProps({
  //       key: 'copy',
  //       disabled: false,
  //     });
  //   }
  // }

  triggerCloseDialog(backendResponse: BackendResponse | undefined) {
    this.closeDialogSignal().set(backendResponse || true);
  }

  triggerDelete(options?: ProgressOptions): void {
    const mb = this.messageboxService.warning(
      'core.delete',
      'core.deletewarning'
    );
    mb.afterClosed().subscribe((response: IMessageboxComponentResponse) => {
      if (response?.result) this.performDelete(options);
    });
  }

  performDelete(options?: ProgressOptions): void {
    if (!this.form || this.form.invalid || !this.service) return;
    this.performAction.crud(
      CrudActionTypeEnum.Delete,
      this.service?.delete(
        this.form?.value[this.idFieldName],
        this.additionalDeleteData
      ),
      options?.callback || this.emitActionDeleted.bind(this),
      options?.errorCallback,
      options
    );
  }

  emitActionDeleted(response: BackendResponse): void {
    this.actionTakenSignal().set({
      rowItemId: this.form?.value[this.idFieldName],
      ref: this.ref(),
      type: CrudActionTypeEnum.Delete,
      additionalProps: this.additionalDeleteProps,
    });
    this.triggerCloseDialog(response);
  }

  activeChanged(value: boolean) {
    if (this.form) {
      this.form.patchValue({
        isActive: value,
        state: value ? SoeEntityState.Active : SoeEntityState.Inactive,
      });
      this.form.markAsDirty();
    }
  }

  setNewRefOnTab(newRef?: Guid, isNew?: boolean) {
    this.setNewRefOnTabSignal().set({
      ref: this.ref(),
      newRef: newRef || Guid.newGuid(),
      isNew: isNew || false,
    });
  }
}

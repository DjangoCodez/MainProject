import { Component, inject, OnInit } from '@angular/core';
import { FieldSettingsForm } from '../../models/field-settings-form.model';
import { EditBaseDirective } from '@shared/directives/edit-base/edit-base.directive';
import {
  Feature,
  SoeFieldSettingType,
  TermGroup,
} from '@shared/models/generated-interfaces/Enumerations';
import { IFieldSettingDTO } from '@shared/models/generated-interfaces/FieldSettingDTO';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { FieldSettingsService } from '../../services/field-settings.service';
import { SoeConfigUtil } from '@shared/util/soeconfig-util';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { Observable, tap } from 'rxjs';

@Component({
  selector: 'soe-field-settings-edit',
  templateUrl: './field-settings-edit.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class FieldSettingsEditComponent
  extends EditBaseDirective<
    IFieldSettingDTO,
    FieldSettingsService,
    FieldSettingsForm
  >
  implements OnInit
{
  readonly service = inject(FieldSettingsService);
  readonly coreService = inject(CoreService);

  // Lookups
  yesNoValues: SmallGenericType[] = [];

  ngOnInit() {
    super.ngOnInit();

    this.startFlow(Feature.Manage_Preferences_FieldSettings_Edit, {
      lookups: [this.loadYesNoValues()],
    });
  }

  setCompanySettingVisibleIdFromForm() {
    if (
      this.form?.value?.companySetting?.visible !== undefined &&
      this.yesNoValues &&
      this.yesNoValues.length > 0
    ) {
      const visibleId = this.getVisibleIdFromBool(
        this.form.value.companySetting.visible
      );

      if (this.form?.value.companySettingVisibleId !== visibleId) {
        this.form?.patchValue({
          companySettingVisibleId: visibleId,
        });
      }
    }
  }

  companySettingVisibleIdChanged() {
    if (!this.form) return;

    const visibleValue = this.getVisibleBoolFromId(
      this.form.value.companySettingVisibleId
    );

    this.form?.patchValue({
      companySetting: {
        visible: visibleValue,
      },
    });
  }

  private getVisibleBoolFromId(value: number | undefined): boolean | null {
    if (value === 1) return true;
    else if (value === 0) return false;
    else return null;
  }

  private getVisibleIdFromBool(value: boolean | undefined): number | undefined {
    const id = value === true ? 1 : value === false ? 0 : 2;
    return this.yesNoValues.find((x: SmallGenericType) => x.id === id)?.id ?? 2;
  }

  override loadData(): Observable<void> {
    const fieldSettingsType = SoeConfigUtil.fieldSettingsType
      ? SoeConfigUtil.fieldSettingsType
      : SoeFieldSettingType.Mobile;

    return this.performLoadData.load$(
      (<FieldSettingsService>this.service)
        .get(fieldSettingsType, this.form?.getIdControl()?.value)
        .pipe(
          tap(value => {
            this.form?.reset(value);
            this.form?.customPatch(value);
            this.setCompanySettingVisibleIdFromForm();
          })
        )
    );
  }

  private loadYesNoValues() {
    return this.coreService
      .getTermGroupContent(TermGroup.YesNoDefault, true, false)
      .pipe(
        tap(res => {
          this.yesNoValues = res;
          this.setCompanySettingVisibleIdFromForm();
        })
      );
  }
}

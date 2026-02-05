import {
  SoeCheckboxFormControl,
  SoeDateFormControl,
  SoeFormGroup,
  SoeNumberFormControl,
  SoeSelectFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { TimeProjectDTO } from './project.model';
import {
  AccountingSettingsForm,
  AccountingSettingsFormArray,
} from '@shared/components/accounting-settings/accounting-settings/accounting-settings-form.model';
import { IAccountingSettingsRowDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { TermGroup_ProjectStatus } from '@shared/models/generated-interfaces/Enumerations';

interface IProjectForm {
  validationHandler: ValidationHandler;
  element: TimeProjectDTO | undefined;
}

export class ProjectForm extends SoeFormGroup {
  projectValidationHandler: ValidationHandler;

  constructor({ validationHandler, element }: IProjectForm) {
    super(validationHandler, {
      projectId: new SoeTextFormControl(element?.projectId || 0, {
        isIdField: true,
      }),
      number: new SoeTextFormControl(
        element?.number || '',
        {
          required: true,
        },
        'billing.project.projectnr'
      ),
      name: new SoeTextFormControl(
        element?.name || '',
        {
          isNameField: true,
          required: true,
        },
        'common.name'
      ),
      description: new SoeTextFormControl(element?.description || ''),
      parentProjectId: new SoeSelectFormControl(element?.parentProjectId || 0),
      allocationType: new SoeSelectFormControl(element?.allocationType || 0),
      startDate: new SoeDateFormControl(element?.startDate || undefined),
      stopDate: new SoeDateFormControl(element?.stopDate || undefined),
      workSiteKey: new SoeTextFormControl(element?.workSiteKey || '', {
        maxLength: 100,
      }),
      workSiteNumber: new SoeTextFormControl(element?.workSiteNumber || '', {
        maxLength: 100,
      }),
      attestWorkFlowHeadId: new SoeSelectFormControl(
        element?.attestWorkFlowHeadId || 0
      ),
      selectedOrderTemplate: new SoeSelectFormControl(
        element?.orderTemplateId || 0
      ),

      defaultDim1AccountId: new SoeSelectFormControl(
        element?.defaultDim1AccountId
      ),
      defaultDim2AccountId: new SoeSelectFormControl(
        element?.defaultDim2AccountId
      ),
      defaultDim3AccountId: new SoeSelectFormControl(
        element?.defaultDim3AccountId
      ),
      defaultDim4AccountId: new SoeSelectFormControl(
        element?.defaultDim4AccountId
      ),
      defaultDim5AccountId: new SoeSelectFormControl(
        element?.defaultDim5AccountId
      ),
      defaultDim6AccountId: new SoeSelectFormControl(
        element?.defaultDim6AccountId
      ),

      customerId: new SoeSelectFormControl(element?.customerId || 0, {
        disabled: (element?.projectId && element.projectId > 0) || false,
      }),

      projectAccountingSettings: new AccountingSettingsFormArray(
        validationHandler
      ),

      note: new SoeTextFormControl(element?.note || ''),
      useAccounting: new SoeCheckboxFormControl(
        element?.useAccounting || false
      ),
      payrollProductAccountingPrio: new SoeTextFormControl(
        element?.payrollProductAccountingPrio || ''
      ),
      invoiceProductAccountingPrio: new SoeTextFormControl(
        element?.invoiceProductAccountingPrio || ''
      ),

      invoicePrio1: new SoeSelectFormControl(0),
      invoicePrio2: new SoeSelectFormControl(0),
      invoicePrio3: new SoeSelectFormControl(0),
      invoicePrio4: new SoeSelectFormControl(0),
      invoicePrio5: new SoeSelectFormControl(0),
      payrollPrio1: new SoeSelectFormControl(0),
      payrollPrio2: new SoeSelectFormControl(0),
      payrollPrio3: new SoeSelectFormControl(0),
      payrollPrio4: new SoeSelectFormControl(0),
      payrollPrio5: new SoeSelectFormControl(0),

      priceListTypeId: new SoeSelectFormControl(
        element?.priceListTypeId || undefined
      ),
      projectStatus: new SoeSelectFormControl(
        element?.status || TermGroup_ProjectStatus.Active
      ),
    });

    this.projectValidationHandler = validationHandler;
    this.projectAccountingSettings.rawPatch(element?.accountingSettings ?? []);
  }

  get projectId(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.projectId;
  }

  get number(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.number;
  }

  get name(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.name;
  }

  get defaultDim1AccountId(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.defaultDim1AccountId;
  }

  get defaultDim2AccountId(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.defaultDim2AccountId;
  }

  get defaultDim3AccountId(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.defaultDim3AccountId;
  }

  get defaultDim4AccountId(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.defaultDim4AccountId;
  }

  get defaultDim5AccountId(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.defaultDim5AccountId;
  }

  get defaultDim6AccountId(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.defaultDim6AccountId;
  }

  get customerId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.customerId;
  }

  get selectedOrderTemplate(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.selectedOrderTemplate;
  }

  get projectAccountingSettings(): AccountingSettingsFormArray {
    return <AccountingSettingsFormArray>this.controls.projectAccountingSettings;
  }

  get note(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.note;
  }

  get useAccounting(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.useAccounting;
  }

  get payrollProductAccountingPrio(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.payrollProductAccountingPrio;
  }

  get invoiceProductAccountingPrio(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.invoiceProductAccountingPrio;
  }

  get invoicePrio1(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.invoicePrio1;
  }

  get invoicePrio2(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.invoicePrio2;
  }

  get invoicePrio3(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.invoicePrio3;
  }

  get invoicePrio4(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.invoicePrio4;
  }

  get invoicePrio5(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.invoicePrio5;
  }

  get payrollPrio1(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.payrollPrio1;
  }

  get payrollPrio2(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.payrollPrio2;
  }

  get payrollPrio3(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.payrollPrio3;
  }

  get payrollPrio4(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.payrollPrio4;
  }

  get parentProjectId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.parentProjectId;
  }

  get payrollPrio5(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.payrollPrio5;
  }

  get priceListTypeId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.priceListTypeId;
  }

  get projectStatus(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.projectStatus;
  }

  customProjectAccountingSettingsPatchValue(rows: IAccountingSettingsRowDTO[]) {
    this.projectAccountingSettings.rawPatch(rows);
  }

  private patchProjectAccountingSettings(
    rows: IAccountingSettingsRowDTO[] | undefined
  ) {
    this.projectAccountingSettings?.clear({ emitEvent: false });
    if (rows && rows.length > 0) {
      rows.forEach(r => {
        this.projectAccountingSettings.push(
          new AccountingSettingsForm({
            validationHandler: this.projectValidationHandler,
            element: r,
          }),
          { emitEvent: false }
        );
      });
      this.projectAccountingSettings.updateValueAndValidity();
    }
  }

  getInvoiceProductAccountingPrio(): string {
    return (
      this.invoicePrio1.value +
      ',' +
      this.invoicePrio2.value +
      ',' +
      this.invoicePrio3.value +
      ',' +
      this.invoicePrio4.value +
      ',' +
      this.invoicePrio5.value
    );
  }

  getPayrollProductAccountingPrio(): string {
    return (
      this.payrollPrio1.value +
      ',' +
      this.payrollPrio2.value +
      ',' +
      this.payrollPrio3.value +
      ',' +
      this.payrollPrio4.value +
      ',' +
      this.payrollPrio5.value
    );
  }

  customPatchValue(value: TimeProjectDTO) {
    const patchProject = {
      ...value,
      projectStatus: value.status,
    };
    this.patchValue(patchProject);
    this.invoiceProductAccountingPrio.reset();
    this.payrollProductAccountingPrio.reset();
    this.projectAccountingSettings.clear({ emitEvent: false });
    this.customProjectAccountingSettingsPatchValue(value.accountingSettings);
    this.customPatchInvoicePrio(value.invoiceProductAccountingPrio);
    this.customPatchPayrollPrio(value.payrollProductAccountingPrio);
  }

  private customPatchInvoicePrio(accountingPrios: string) {
    const prios: string[] = accountingPrios.split(',');

    if (prios.length <= 0) return;

    prios.forEach((prio, index) => {
      switch (index) {
        case 0:
          this.invoicePrio1.setValue(parseInt(prio));
          break;
        case 1:
          this.invoicePrio2.setValue(parseInt(prio));
          break;
        case 2:
          this.invoicePrio3.setValue(parseInt(prio));
          break;
        case 3:
          this.invoicePrio4.setValue(parseInt(prio));
          break;
        case 4:
          this.invoicePrio5.setValue(parseInt(prio));
          break;
      }
    });
  }

  private customPatchPayrollPrio(accountingPrios: string) {
    const prios: string[] = accountingPrios.split(',');

    if (prios.length <= 0) return;

    prios.forEach((prio, index) => {
      switch (index) {
        case 0:
          this.payrollPrio1.setValue(parseInt(prio));
          break;
        case 1:
          this.payrollPrio2.setValue(parseInt(prio));
          break;
        case 2:
          this.payrollPrio3.setValue(parseInt(prio));
          break;
        case 3:
          this.payrollPrio4.setValue(parseInt(prio));
          break;
        case 4:
          this.payrollPrio5.setValue(parseInt(prio));
          break;
        default:
          break;
      }
    });
  }
}

import { ProjectSearchResultDTO } from '@shared/components/select-project-dialog/models/select-project-dialog.model';
import { SoeFormGroup, SoeTextFormControl } from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { IProjectGridDTO } from '@shared/models/generated-interfaces/ProjectDTOs';

interface IProjectCentralForm {
  validationHandler: ValidationHandler;
  element: ProjectSearchResultDTO;
}
export class ProjectCentralForm extends SoeFormGroup {
  thisValidationHandler: ValidationHandler;

  constructor({ validationHandler, element }: IProjectCentralForm) {
    super(validationHandler, {
      projectId: new SoeTextFormControl(element?.projectId || 0, {
        isIdField: true,
      }),
      projectName: new SoeTextFormControl(element?.name || '', {
        isNameField: true,
      }),
      number: new SoeTextFormControl(element?.number || '', {}),
      customerNr: new SoeTextFormControl(element?.customerNr || '', {}),
      customerName: new SoeTextFormControl(element?.customerName || '', {}),
      managerName: new SoeTextFormControl(element?.managerName || '', {}),
      orderNumber: new SoeTextFormControl(element?.orderNr || '', {}),
      statusName: new SoeTextFormControl(element?.status || false, {}),
    });

    this.thisValidationHandler = validationHandler;
  }

  get projectId(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.projectId;
  }

  get projectName(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.projectName;
  }

  get number(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.number;
  }

  get customerNr(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.customerNr;
  }

  get customerName(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.customerName;
  }

  get managerName(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.managerName;
  }

  get statusName(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.statusName;
  }

  patchProjectCentralData(projectGridDTO: IProjectGridDTO): void {
    this.patchValue({
      projectId: projectGridDTO.projectId,
      number: projectGridDTO.number,
      projectName: projectGridDTO.name,
      customerNr: projectGridDTO.customerNr,
      customerName: projectGridDTO.customerName,
      managerName: projectGridDTO.managerName,
      orderNumber: projectGridDTO.orderNr,
      statusName: projectGridDTO.statusName,
    });
  }
}

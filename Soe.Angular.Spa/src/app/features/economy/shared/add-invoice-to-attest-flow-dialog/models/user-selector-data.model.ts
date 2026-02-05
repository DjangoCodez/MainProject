import { IAttestWorkFlowTemplateRowDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';

export interface UserSelectorData {
  row: IAttestWorkFlowTemplateRowDTO;
  mode: number; // 0 = Users, 1 = Roles
}

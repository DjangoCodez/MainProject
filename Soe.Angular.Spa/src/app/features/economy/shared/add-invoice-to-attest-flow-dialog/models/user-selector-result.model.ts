import { IAttestWorkFlowRowDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';

export interface UserSelectorResult {
  rows: IAttestWorkFlowRowDTO[];
  attestTransitionId: number;
}

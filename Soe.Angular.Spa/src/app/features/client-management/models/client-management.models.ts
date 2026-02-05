import { IMultiCompanyErrorDTO } from '@shared/models/generated-interfaces/MultiCompanyResponseDTO';

export interface IMultiCompanyResponseDTO<TResult> {
  value: TResult;
  errors: IMultiCompanyErrorDTO[];
}
